using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Encryptor
{
   public interface IAesEncryptor:IEncryptor 
    {
        string AesEncrypt(string input);
        string AesDecrypt(string input);
        void EncryptProperties(IEncryptableClass  ob);
        void Decryptproperties(IEncryptableClass ob);
    }
}
