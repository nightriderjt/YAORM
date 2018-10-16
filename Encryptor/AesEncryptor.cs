using System;
using static Encryptor.Attributes;

namespace Encryptor
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Encryptor.IAesEncryptor" />
    public class AesEncryptor : IAesEncryptor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AesEncryptor"/> class.
        /// </summary>
        public AesEncryptor()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AesEncryptor"/> class.
        /// </summary>
        /// <param name="hashKey">The hash key.</param>
        public AesEncryptor(string hashKey)
        {
            _hashKey = hashKey ;
        }

        private string _hashKey { get; set; } = "";
       
        public string AesDecrypt(string input)
        {
            if (string.IsNullOrEmpty(input) == false)
            {
                System.Security.Cryptography.RijndaelManaged AES = new System.Security.Cryptography.RijndaelManaged();
                System.Security.Cryptography.MD5CryptoServiceProvider Hash_AES = new System.Security.Cryptography.MD5CryptoServiceProvider();
                try
                {
                    byte[] hash = new byte[32];
                    byte[] temp = Hash_AES.ComputeHash(System.Text.Encoding.UTF8.GetBytes(_hashKey));
                    Array.Copy(temp, 0, hash, 0, 16);
                    Array.Copy(temp, 0, hash, 15, 16);
                    AES.Key = hash;
                    AES.Mode = System.Security.Cryptography.CipherMode.ECB;
                    System.Security.Cryptography.ICryptoTransform DESDecrypter = AES.CreateDecryptor();
                    byte[] buffer = Convert.FromBase64String(input);
                    var decrypted = System.Text.Encoding.UTF8.GetString(DESDecrypter.TransformFinalBlock(buffer, 0, buffer.Length));
                    return decrypted;
                }
                catch (Exception)
                {
                    return "";
                }
            }
            else
                return "";
        }

        public string AesEncrypt(string input)
        {
            System.Security.Cryptography.RijndaelManaged aes = new System.Security.Cryptography.RijndaelManaged();
            System.Security.Cryptography.MD5CryptoServiceProvider hashAes = new System.Security.Cryptography.MD5CryptoServiceProvider();
            try
            {
                byte[] hash = new byte[32];
                byte[] temp = hashAes.ComputeHash(System.Text.Encoding.UTF8.GetBytes(_hashKey));
                Array.Copy(temp, 0, hash, 0, 16);
                Array.Copy(temp, 0, hash, 15, 16);
                aes.Key = hash;
                aes.Mode = System.Security.Cryptography.CipherMode.ECB;
                System.Security.Cryptography.ICryptoTransform DESEncrypter = aes.CreateEncryptor();
                byte[] Buffer = System.Text.Encoding.UTF8.GetBytes(input);
                var encrypted = Convert.ToBase64String(DESEncrypter.TransformFinalBlock(Buffer, 0, Buffer.Length));
                return encrypted;
            }
            catch (Exception)
            {
                return "";
            }
        }

        public void Decryptproperties(IEncryptableClass  ob)
        {
            var _properties = ob.GetType().GetProperties();
            foreach (var _property in _properties)
            {
                var _propertyAttribute = Attribute.GetCustomAttribute(_property, typeof(MustEncrypt));
                if (_propertyAttribute != null)
                    if (_property.GetValue(ob) != null)
                    {
                        _property.SetValue(ob, AesDecrypt(_property.GetValue(ob).ToString()));
                    }
            }
        }

        public void EncryptProperties(IEncryptableClass ob)
        {
            var _properties = ob.GetType().GetProperties();
            foreach (var _property in _properties)
            {
                var _propertyAttribute = Attribute.GetCustomAttribute(_property, typeof(MustEncrypt));
                if (_propertyAttribute != null)
                    if (_property.GetValue(ob) != null)
                    {
                        _property.SetValue(ob, AesEncrypt(_property.GetValue(ob).ToString()));
                    }
            }
        }
    }
}
