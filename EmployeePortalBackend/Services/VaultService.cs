using EmployeePortalBackend.DTO.CustomerDtos;
using EmployeePortalBackend.DTO.ìmageDtos;
using EmployeePortalBackend.Enums;
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

            var authMethod = new TokenAuthMethodInfo(_opts.AgentToken);

            var vaultClientSettings = new VaultClientSettings(
                "http://vault-agent:8007",
                authMethod
            );

            vaultClient = new VaultClient(vaultClientSettings);
            _logger.LogInformation("VaultService initialised — using Vault Agent proxy at {Address}",
                "http://vault-agent:8007");
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

        public async Task<IdRequest> EncryptIdRequest(UploadRequestActiveDto uploadRequest, string key)
        {
            var idRequest = new IdRequest
            {
                Id = uploadRequest.Id,
                CustomerId = uploadRequest.CustomerId,
                EmployeeId = await EncryptField(key, uploadRequest.EmployeeId),
                EmployeeName = await EncryptField(key, uploadRequest.EmployeeName),
                status = await EncryptField(key, uploadRequest.status.ToString()),
                CreatedDate = uploadRequest.CreatedDate,
                ValidUntilDate =  uploadRequest.ValidUntilDate,
                ObjectKey = await EncryptField(key, uploadRequest.ObjectKey),
                Customer = uploadRequest.Customer
            };

            return idRequest;
        }

        public async Task<UploadRequestActiveDto> DecryptIdRequest(IdRequest idRequest, string key)
        {
            var decryptedIdRequest = new UploadRequestActiveDto
            {
                Id = idRequest.Id,
                CustomerId = idRequest.CustomerId,
                EmployeeId = await DecryptField(key, idRequest.EmployeeId),
                EmployeeName = await DecryptField(key, idRequest.EmployeeName),
                status = (UploadStatus)Enum.Parse(typeof(UploadStatus), await DecryptField(key, idRequest.status)),
                CreatedDate = idRequest.CreatedDate,
                ValidUntilDate = idRequest.ValidUntilDate,
                ObjectKey = await DecryptField(key, idRequest.ObjectKey),
                Customer = idRequest.Customer
            };
            return decryptedIdRequest;
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
        public string AgentAddress { get; set; }
        public string RoleId { get; set; }   // non-secret, can be in appsettings
        public string SecretId { get; set; }  // SECRET — inject via env/K8s

        public string AgentToken { get; set; } // For Vault Agent Token Auth method
    }
}
