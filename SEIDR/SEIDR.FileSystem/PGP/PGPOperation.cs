using System;
using System.ComponentModel;

namespace SEIDR.FileSystem.PGP
{
    public enum PGPOperation
    {
        /// <summary>
        /// PGP key file generate operation.
        /// </summary>
        GenerateKey = 0,
        /// <summary>
        /// PGP Encryp operation.
        /// </summary>
        Encrypt = 1,
        /// <summary>
        /// PGP Decrypt operation.
        /// </summary>
        Decrypt = 2,
        //[Description("Sign the File with the private key, and Encrypt the file with the public key")]
        /// <summary>
        /// PGP Sign and encrypt operation.
        /// </summary>
        SignAndEncrypt = 3,
        /// <summary>
        /// PGP Sign operation.
        /// </summary>
        Sign = 4,
    }
}
