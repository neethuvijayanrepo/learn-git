using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestProject.Utilities.Cryptography.CryptoInterfaces
{
    public interface ITestProjectCryptoProvider
    {
        /// <summary>
        /// To encrypt string data
        /// </summary>
        /// <param name="queryString">The string which is to be encrypted</param>
        /// <returns>encrypted string</returns>
        string EncryptString(string queryString);

        /// <summary>
        /// To do string decryption.
        /// </summary>
        /// <param name="cryptoData">Encrypted data which is to be decrypted</param>
        /// <returns>Decrypted string</returns>
        string DecryptString(string cryptoData);
    }
}
