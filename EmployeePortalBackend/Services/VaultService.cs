using EmployeePortalBackend.DTO.CustomerDtos;
using EmployeePortalBackend.Model;
using Microsoft.Extensions.Options;
using System.Net.Http;
using System.Security.Cryptography;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Text.Json;
using VaultSharp;
using VaultSharp.V1.AuthMethods.AppRole;
using VaultSharp.V1.AuthMethods.Token;
using VaultSharp.V1.SecretsEngines.Transit;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace EmployeePortalBackend.Services
{
    public class VaultService
    {
        IVaultClient vaultClient;
        private readonly VaultOptions _opts;
        private readonly ILogger _logger;
        private Timer? _renewalTimer;
        public VaultService(IOptions<VaultOptions> opts, ILogger<VaultService> logger)
        {
            _opts = opts.Value;
            _logger = logger;
        }

        public async Task init()
        {
            var authMethod = new AppRoleAuthMethodInfo(
            roleId: _opts.RoleId,
            secretId: _opts.SecretId
            );

            var vaultClientSettings = new VaultClientSettings(
               "http://vault:8200", //_opts.VaultAddress
               authMethod
            //new TokenAuthMethodInfo("hvs.rt0CvTff0bB83UxT4QH53hYS") // use AppRole in prod-like setups
            );
            vaultClient = new VaultClient(vaultClientSettings);
            var renewed = await vaultClient.V1.Auth.Token.RenewSelfAsync();
            ScheduleRenewal(renewed.LeaseDurationSeconds);

            _logger.LogInformation("Vault AppRole login successful: " + renewed.LeaseDurationSeconds.ToString());
        }
        private void ScheduleRenewal(long ttlSeconds)
        {
            var renewIn = TimeSpan.FromSeconds(ttlSeconds * 0.8);

            _renewalTimer?.Dispose();
            _renewalTimer = new Timer(async _ =>
            {
                try
                {
                    var renewed = await vaultClient.V1.Auth.Token.RenewSelfAsync();
                    _logger.LogInformation("Vault token renewed, new TTL: {TTL}s",
                        renewed.LeaseDurationSeconds);
                    ScheduleRenewal(renewed.LeaseDurationSeconds);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Token renewal failed — re-authenticating");
                    await init();
                }
            }, null, renewIn, Timeout.InfiniteTimeSpan);
        }

        public async Task<string> GetHmacSecretAsync()
        {
            var secret = await vaultClient.V1.Secrets.KeyValue.V2
                .ReadSecretAsync(
                    path: "hmac-vault-secret",
                    mountPoint: "secret");

            return secret.Data.Data["value"].ToString()!;
        }
        public async Task<string> ComputeHmacAsync(string input)
        {
            var secret = await GetHmacSecretAsync();
            var keyBytes = Convert.FromBase64String(secret);

            using var hmac = new HMACSHA256(keyBytes);
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(input));
            return Convert.ToHexString(hash).ToLowerInvariant();
        }
        public async Task<List<string>> ComputeHmacBatchAsync(IEnumerable<string> inputs)
        {
            var batchItems = inputs
                .Select(input => new HmacSingleInput
                {
                    Base64EncodedInput = Convert.ToBase64String(Encoding.UTF8.GetBytes(input))
                })
                .ToList();

            var hmacRequest = new HmacRequestOptions
            {
                BatchInput = batchItems
                // No KeyVersion set = always uses latest (and only) version
            };

            var response = await vaultClient.V1.Secrets.Transit
                .GenerateHmacAsync(
                    "kek-standard",
                    hmacRequest);

            return response.Data.BatchResults
                .Select(r => r.Hmac)
                .ToList();
        }


        public async Task<Customer> EncryptCustomerAsync(CustomerInternalDto customer, string key)
        {
           
            Customer c = new Customer();

            c.FirstName = await EncryptField(key, customer.FirstName);
            c.LastName = await EncryptField(key, customer.LastName);
            c.Birthday = await EncryptField(key, customer.Birthday);
            c.email = await EncryptField(key, customer.Email);
            c.Id = customer.Id;

            c.FirstNameHash = await EncryptSha(customer.FirstName, "donutdonutgodonuts");

            return c;
        }
        public async Task<DecryptedBasicCustomerobject> DecrypteCustomer(Customer customer, string key)
        {
            DecryptedBasicCustomerobject Decryptedcustomer = new DecryptedBasicCustomerobject();

            Decryptedcustomer.FirstName = await DecryptField(key, customer.FirstName);
            Decryptedcustomer.LastName = await DecryptField(key, customer.LastName);
            Decryptedcustomer.Birthday = await DecryptField(key, customer.Birthday);
            Decryptedcustomer.Email = await DecryptField(key, customer.email);
            Decryptedcustomer.Id = customer.Id;

            return Decryptedcustomer;

        }

        public async Task<string> EncryptField(string key, string field)
        {
            var plainTextBytes = Encoding.UTF8.GetBytes(field);
            var base64PlainText = Convert.ToBase64String(plainTextBytes);

            var encryptResult = await vaultClient.V1.Secrets.Transit.EncryptAsync(
                key,
                new EncryptRequestOptions { Base64EncodedPlainText = base64PlainText }
            );
            string storedCiphertext = encryptResult.Data.CipherText;
            return storedCiphertext;
        }

        public async Task<string> DecryptField(string key, string encryptedString)
        {
            string storedCiphertext = encryptedString;

            var decryptResult = await vaultClient.V1.Secrets.Transit.DecryptAsync(
                key,
                new DecryptRequestOptions { CipherText = storedCiphertext }
            );
            byte[] dek = Convert.FromBase64String(decryptResult.Data.Base64EncodedPlainText);
            return Encoding.ASCII.GetString(dek);
        }

        public async Task<string> EncryptSha(string Value, string Key)
        {
            using (SHA512 sha512 = SHA512.Create())
            {
                var hashBytes = sha512.ComputeHash(Encoding.UTF8.GetBytes(Value.ToLower()+Key));
                return Convert.ToHexString(hashBytes).ToLower();
            }
        }
    }
    public class VaultOptions
    {
        public string VaultAddress { get; set; }
        public string RoleId { get; set; }   // non-secret, can be in appsettings
        public string SecretId { get; set; }  // SECRET — inject via env/K8s
    }
}
