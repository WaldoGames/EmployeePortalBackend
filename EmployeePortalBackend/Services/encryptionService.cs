using EmployeePortalBackend.DTO.CustomerDtos;
using EmployeePortalBackend.Model;
using System.Security.Cryptography;
using System.Security.Cryptography.Xml;
using System.Text;
using VaultSharp;
using VaultSharp.V1.AuthMethods.Token;
using VaultSharp.V1.SecretsEngines.Transit;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace EmployeePortalBackend.Services
{
    public class encryptionService
    {
        VaultClient vaultClient;
        public encryptionService()
        {
            var vaultClientSettings = new VaultClientSettings(
               "http://vault:8200",
               new TokenAuthMethodInfo("hvs.rt0CvTff0bB83UxT4QH53hYS") // use AppRole in prod-like setups
            );
            vaultClient = new VaultClient(vaultClientSettings);
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
}
