using System;
using static Encryptor.Attributes;

namespace Encryptor
{
   public static class Encryptor
    {

        public static string AesEncrypt(string input, string pass)
        {
            System.Security.Cryptography.RijndaelManaged aes = new System.Security.Cryptography.RijndaelManaged();
            System.Security.Cryptography.MD5CryptoServiceProvider hashAes = new System.Security.Cryptography.MD5CryptoServiceProvider();
            try
            {
                byte[] hash = new byte[32];
                byte[] temp = hashAes.ComputeHash(System.Text.Encoding.UTF8.GetBytes(pass));
                Array.Copy(temp, 0, hash, 0, 16);
                Array.Copy(temp, 0, hash, 15, 16);
                aes.Key = hash;
                aes.Mode = System.Security.Cryptography.CipherMode.ECB;
                System.Security.Cryptography.ICryptoTransform DESEncrypter = aes.CreateEncryptor();
                byte[] Buffer = System.Text.Encoding.UTF8.GetBytes(input);
                var encrypted = Convert.ToBase64String(DESEncrypter.TransformFinalBlock(Buffer, 0, Buffer.Length));
                return encrypted;
            }
            catch (Exception )
            {
                return "";
            }
        }

   
        public static string AesDecrypt(string input, string pass)
        {
            if (string.IsNullOrEmpty(input) == false)
            {
                System.Security.Cryptography.RijndaelManaged AES = new System.Security.Cryptography.RijndaelManaged();
                System.Security.Cryptography.MD5CryptoServiceProvider Hash_AES = new System.Security.Cryptography.MD5CryptoServiceProvider();
                try
                {
                    byte[] hash = new byte[32];
                    byte[] temp = Hash_AES.ComputeHash(System.Text.Encoding.UTF8.GetBytes(pass));
                    Array.Copy(temp, 0, hash, 0, 16);
                    Array.Copy(temp, 0, hash, 15, 16);
                    AES.Key = hash;
                    AES.Mode = System.Security.Cryptography.CipherMode.ECB;
                    System.Security.Cryptography.ICryptoTransform DESDecrypter = AES.CreateDecryptor();
                    byte[] buffer = Convert.FromBase64String(input);
                    var decrypted = System.Text.Encoding.UTF8.GetString(DESDecrypter.TransformFinalBlock(buffer, 0, buffer.Length));
                    return decrypted;
                }
                catch (Exception )
                {
                    return "";
                }
            }
            else
                return "";
        }


        /// <summary>
        /// Encrypts the properties marked with MustEncrypt of a class which implements IEncryptableClass.
        /// </summary>
        /// <param name="ob">The ob.</param>
        /// <param name="EncryptKey">The encrypt key.</param>
        public static void EncryptProperties(this IEncryptableClass ob, string EncryptKey)
        {
            var _properties = ob.GetType().GetProperties();
            foreach (var _property in _properties)
            {
                var _propertyAttribute = Attribute.GetCustomAttribute(_property, typeof(MustEncrypt));
                if (_propertyAttribute != null)
                    if (_property.GetValue(ob) != null)
                    {
 _property.SetValue(ob, AesEncrypt(_property.GetValue(ob).ToString(), EncryptKey));
                    }
            }
        }


        /// <summary>
        /// Decrypts the properties marked with MustEncrypt of a class which implements IEncryptableClass.
        /// </summary>
        /// <param name="ob">The ob.</param>
        /// <param name="EncryptKey">The encrypt key.</param>
        public static void DecryptProperties(this IEncryptableClass ob, String EncryptKey)
        {
            var _properties = ob.GetType().GetProperties();
            foreach (var _property in _properties)
            {
                var _propertyAttribute = Attribute.GetCustomAttribute(_property, typeof(MustEncrypt));
                if (_propertyAttribute != null)
                    if (_property.GetValue(ob) != null)
                    {
 _property.SetValue(ob, AesDecrypt(_property.GetValue(ob).ToString(), EncryptKey));
                    }                   
            }
        }
    }
}
