using System;
using System.Collections.Generic;
using System.IO;

using Org.BouncyCastle.Bcpg;
using Org.BouncyCastle.Bcpg.OpenPgp;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities.IO;

namespace SEIDR.FileSystem.PGP
{
    public class PGP
    {
        private delegate bool Process_Operation(ref ValidationError error);

        private PGPConfiguration config { get; set; }
        private PGPOperation operation { get; set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="config">PGP configuration</param>
        public PGP(PGPConfiguration config)
        {
            this.config = config;
            operation = (PGPOperation) config.PGPOperationID;
        }

        /// <summary>
        /// Perform processing of provided configuration.
        /// </summary>
        /// <param name="error">validation error</param>
        /// <returns>true if processing success, otherwice false</returns>
        public bool Process(ref ValidationError error)
        {
            return ((Process_Operation) Delegate.CreateDelegate(typeof(Process_Operation), this, operation.ToString())).Invoke(ref error);
        }

        /// <summary>
        /// Generate key pair (private and public).
        /// </summary>
        /// <param name="error">validation error</param>
        /// <returns>true if generation success, otherwice false</returns>
        private bool GenerateKey(ref ValidationError error)
        {
            // perform validation of configuration
            if (string.IsNullOrWhiteSpace(config.PrivateKeyFile))
            {
                error = ValidationError.PI;
                return false;
            }
            else
            {
                bool valid = true;
                string fileName = Path.GetFileName(config.PrivateKeyFile);
                if (!string.IsNullOrEmpty(fileName))
                {
                    string path = Path.GetDirectoryName(config.PrivateKeyFile);
                    valid = Directory.Exists(path);
                }
                else
                {
                    valid = false;
                }
                if (!valid)
                {
                    error = ValidationError.PI;
                    return false;
                }
            }

            if (string.IsNullOrWhiteSpace(config.PublicKeyFile))
            {
                error = ValidationError.PU;
                return false;
            }
            else
            {
                bool valid = true;
                string fileName = Path.GetFileName(config.PublicKeyFile);
                if (!string.IsNullOrEmpty(fileName))
                {
                    string path = Path.GetDirectoryName(config.PublicKeyFile);
                    valid = Directory.Exists(path);
                }
                else
                {
                    valid = false;
                }
                if (!valid)
                {
                    error = ValidationError.PU;
                    return false;
                }
            }

            IAsymmetricCipherKeyPairGenerator kpg = GeneratorUtilities.GetKeyPairGenerator("RSA");
            kpg.Init(new RsaKeyGenerationParameters(BigInteger.ValueOf(0x13), new SecureRandom(), 1024, 8));
            AsymmetricCipherKeyPair kp = kpg.GenerateKeyPair();

            char[] pass = string.IsNullOrEmpty(config.PassPhrase) ? new char[0] : config.PassPhrase.ToCharArray();
            string identity = string.IsNullOrEmpty(config.KeyIdentity) ? "" : config.KeyIdentity;
            GenerateKey(config.PrivateKeyFile, config.PublicKeyFile, kp.Public, kp.Private, PublicKeyAlgorithmTag.RsaGeneral, SymmetricKeyAlgorithmTag.Cast5, identity, pass);

            return true;
        }
        
        /// <summary>
        /// Encrypt.
        /// </summary>
        /// <param name="error">validation error</param>
        /// <returns>true if encrypt success, otherwice false</returns>
        private bool Encrypt(ref ValidationError error)
        {
            // perform validation of configuration
            if (string.IsNullOrWhiteSpace(config.PublicKeyFile))
            {
                error = ValidationError.PU;
                return false;
            }
            else if (!File.Exists(config.PublicKeyFile))
            {
                error = ValidationError.PU;
                return false;
            }

            if (string.IsNullOrWhiteSpace(config.SourcePath))
            {
                error = ValidationError.PS;
                return false;
            }
            else if (!Utility.IsPathExists(config.SourcePath))
            {
                    error = ValidationError.PS;
                    return false;
            }

            if (string.IsNullOrWhiteSpace(config.OutputPath))
            {
                error = ValidationError.PO;
                return false;
            }
            else if (!Utility.IsPathExists(config.OutputPath))
            {
                error = ValidationError.PO;
                return false;
            }

            string[] files = Utility.GetFiles(config.SourcePath, "*.txt");
            foreach (string file in files)
            {
                string outFile = ComposeOutFile(file, config.OutputPath, ".pgp", null);                
                EncryptFile(outFile, file, config.PublicKeyFile, true);
            }            

            return true;
        }

        public static string GetOutputFile(PGPConfiguration config)
        {
            string result = null;
            if (config != null)
            {                
                PGPOperation operation = (PGPOperation) config.PGPOperationID;
                result = operation == PGPOperation.Encrypt        ? ComposeOutFile(config.SourcePath, config.OutputPath, ".pgp", null) :
                         operation == PGPOperation.SignAndEncrypt ? ComposeOutFile(config.SourcePath, config.OutputPath, ".pgp", null) :
                         operation == PGPOperation.Decrypt        ? ComposeOutFile(config.SourcePath, config.OutputPath, null, ".pgp") :
                         operation == PGPOperation.Sign           ? ComposeOutFile(config.SourcePath, config.OutputPath, ".sgn", null) :
                                                                    null;

            }

            return result;
        }

        private bool Decrypt(ref ValidationError error)
        {
            // perform validation of configuration
            if (string.IsNullOrWhiteSpace(config.PrivateKeyFile))
            {
                error = ValidationError.PI;
                return false;
            }
            else if (!File.Exists(config.PrivateKeyFile))
            {
                error = ValidationError.PI;
                return false;
            }

            if (string.IsNullOrWhiteSpace(config.SourcePath))
            {
                error = ValidationError.PS;
                return false;
            }
            else if (!Utility.IsPathExists(config.SourcePath))
            {
                error = ValidationError.PS;
                return false;
            }

            if (string.IsNullOrWhiteSpace(config.OutputPath))
            {
                error = ValidationError.PO;
                return false;
            }
            else if (!Utility.IsPathExists(config.OutputPath))
            {
                error = ValidationError.PO;
                return false;
            }

            string[] files = Utility.GetFiles(config.SourcePath, "*.pgp");
            char[] pass = string.IsNullOrEmpty(config.PassPhrase) ? new char[0] : config.PassPhrase.ToCharArray();

            foreach (string file in files)
            {
                string outFile = ComposeOutFile(file, config.OutputPath, null, ".pgp");                
                DecryptFile(file, config.PrivateKeyFile, pass, outFile);
            }

            return true;
        }

        /// <summary>
        /// Sign and encrypt source file to output file using public and private keys.
        /// </summary>
        /// <param name="error">validation error</param>
        /// <returns>true if sign and encrypt success, otherwice false</returns>
        private bool SignAndEncrypt(ref ValidationError error)
        {
            // perform validation of configuration
            if (string.IsNullOrWhiteSpace(config.PrivateKeyFile))
            {
                error = ValidationError.PI;
                return false;
            }
            else if (!File.Exists(config.PrivateKeyFile))
            {
                error = ValidationError.PI;
                return false;
            }
            if (string.IsNullOrWhiteSpace(config.PublicKeyFile))
            {
                error = ValidationError.PU;
                return false;
            }
            else if (!File.Exists(config.PublicKeyFile))
            {
                error = ValidationError.PU;
                return false;
            }
            if (string.IsNullOrWhiteSpace(config.SourcePath))
            {
                error = ValidationError.PS;
                return false;
            }
            else if (!Utility.IsPathExists(config.SourcePath))
            {
                error = ValidationError.PS;
                return false;
            }
            if (string.IsNullOrWhiteSpace(config.OutputPath))
            {
                error = ValidationError.PO;
                return false;
            }
            else if (!Utility.IsPathExists(config.OutputPath))
            {
                error = ValidationError.PO;
                return false;
            }

            string[] files = Utility.GetFiles(config.SourcePath, "*.txt");
            char[] passPhrase = string.IsNullOrEmpty(config.PassPhrase) ? new char[0] : config.PassPhrase.ToCharArray();

            foreach (string file in files)
            {
                string outFile = ComposeOutFile(file, config.OutputPath, ".pgp", null);

                SignAndEncryptFile(file, config.PrivateKeyFile, config.PublicKeyFile, outFile, passPhrase, true);
            }

            return true;
        }

        private bool Sign(ref ValidationError error)
        {
            // perform validation of configuration
            if (string.IsNullOrWhiteSpace(config.PrivateKeyFile))
            {
                error = ValidationError.PI;
                return false;
            }
            else if (!File.Exists(config.PrivateKeyFile))
            {
                error = ValidationError.PI;
                return false;
            }
            if (string.IsNullOrWhiteSpace(config.SourcePath))
            {
                error = ValidationError.PS;
                return false;
            }
            else if (!Utility.IsPathExists(config.SourcePath))
            {
                error = ValidationError.PS;
                return false;
            }
            if (string.IsNullOrWhiteSpace(config.OutputPath))
            {
                error = ValidationError.PO;
                return false;
            }
            else if (!Utility.IsPathExists(config.OutputPath))
            {
                error = ValidationError.PO;
                return false;
            }

            string[] files = Utility.GetFiles(config.SourcePath, "*.txt");
            char[] passPhrase = string.IsNullOrEmpty(config.PassPhrase) ? new char[0] : config.PassPhrase.ToCharArray();

            foreach (string file in files)
            {
                string outFile = ComposeOutFile(file, config.OutputPath, ".sgn", null);

                SignFile(file, config.PrivateKeyFile, passPhrase, outFile);
            }

            return true;
        }


        /// <summary>
        /// Generate pair of keys public and private.
        /// </summary>
        /// <param name="privateKeyFile">generated private key file</param>
        /// <param name="publicKeyFile">generated public key file</param>
        /// <param name="publicKey">assymetric public key type</param>
        /// <param name="privateKey">assymetric private key file</param>
        /// <param name="PublicKeyAlgorithmTag">algorithm type for public key</param>
        /// <param name="SymmetricKeyAlgorithmTag">algorithm type for private key</param>
        /// <param name="identity">identity</param>
        /// <param name="passPhrase">pass phrase for public key and private key</param>
        static public void GenerateKey
        (
                string privateKeyFile,
                string publicKeyFile,
                AsymmetricKeyParameter publicKey,
                AsymmetricKeyParameter privateKey,
                PublicKeyAlgorithmTag PublicKeyAlgorithmTag,
                SymmetricKeyAlgorithmTag SymmetricKeyAlgorithmTag,
                string identity,
                char[] passPhrase
            )
        {
            using (FileStream privateKeyFileStream = new FileInfo(privateKeyFile).OpenWrite())
            {
                using (Stream privateKeyFileArmoredStream = new ArmoredOutputStream(privateKeyFileStream))
                {
                    using (FileStream publicKeyFileStream = new FileInfo(publicKeyFile).OpenWrite())
                    {
                        using (Stream publicKeyFileArmoredStream = new ArmoredOutputStream(publicKeyFileStream))
                        {
                            PgpSecretKey secretKey = new PgpSecretKey(
                                PgpSignature.DefaultCertification,
                                PublicKeyAlgorithmTag,
                                publicKey,
                                privateKey,
                                DateTime.Now,
                                identity,
                                SymmetricKeyAlgorithmTag,
                                passPhrase,
                                null,
                                null,
                                new SecureRandom()
                            );

                            secretKey.Encode(privateKeyFileArmoredStream);
                            PgpPublicKey key = secretKey.PublicKey;
                            key.Encode(publicKeyFileArmoredStream);

                            publicKeyFileArmoredStream.Close();
                        }

                        publicKeyFileStream.Close();
                    }

                    privateKeyFileArmoredStream.Close();
                }

                privateKeyFileStream.Close();
            }
        }

        /// <summary>
        /// Encrypt source file to outputfile using public key.
        /// </summary>
        /// <param name="outputFile">enrypted output file</param>
        /// <param name="sourceFile">source file to encrypt</param>
        /// <param name="publicKeyFile">public key file</param>
        /// <param name="withIntegrityCheck">flag integrity check</param>
        static public void EncryptFile(string outputFile, string sourceFile, string publicKeyFile, bool withIntegrityCheck)
        {
            using (FileStream outputFileStream = File.Create(outputFile))
            {
                using (Stream outputFileArmoredStream = new ArmoredOutputStream(outputFileStream))
                {
                    using (Stream publicKeyStream = File.OpenRead(publicKeyFile))
                    {
                        PgpPublicKey publicKey = ReadPublicKey(publicKeyStream);
                        using (MemoryStream bOut = new MemoryStream())
                        {
                            PgpCompressedDataGenerator comData = new PgpCompressedDataGenerator(CompressionAlgorithmTag.Zip);
                            PgpUtilities.WriteFileToLiteralData(comData.Open(bOut),
                            PgpLiteralData.Binary,
                            new FileInfo(sourceFile));
                            comData.Close();
                            PgpEncryptedDataGenerator cPk = new PgpEncryptedDataGenerator(SymmetricKeyAlgorithmTag.Cast5, withIntegrityCheck, new SecureRandom());

                            cPk.AddMethod(publicKey);
                            byte[] bytes = bOut.ToArray();
                            using (Stream cOut = cPk.Open(outputFileArmoredStream, bytes.Length))
                            {
                                cOut.Write(bytes, 0, bytes.Length);
                                cOut.Close();
                            }

                            bOut.Close();
                        }

                        publicKeyStream.Close();
                    }

                    outputFileArmoredStream.Close();
                }

                outputFileStream.Close();
            }

        }

        /// <summary>
        /// Decrypt source file to output file using private key and passphrase.
        /// </summary>
        /// <param name="sourceFile">source file to decrypt</param>
        /// <param name="privateKeyFile">private key file</param>
        /// <param name="passPhrase">passphrase</param>
        /// <param name="outputFilePath">decrypted output file </param>
        static public void DecryptFile(string sourceFile, string privateKeyFile, char[] passPhrase, string outputFilePath)
        {
            using (Stream sourceFileStream = File.OpenRead(sourceFile))
            {
                using (Stream sourceFileDecodedStream = PgpUtilities.GetDecoderStream(sourceFileStream))
                {
                    using (Stream privateKeyStream = File.OpenRead(privateKeyFile))
                    {
                        PgpObjectFactory pgpF = new PgpObjectFactory(sourceFileDecodedStream);
                        if (pgpF == null)
                        {
                            throw new ArgumentException(String.Format("Not expected format of source file '{0}' for decrypt.", sourceFile));
                        }

                        PgpEncryptedDataList enc;
                        PgpObject o = pgpF.NextPgpObject();
                        //
                        // the first object might be a PGP marker packet.
                        //

                        if (o == null)
                        {
                            throw new ArgumentException(String.Format("Not expected format of source file '{0}' for decrypt.", sourceFile));
                        }

                        if (o is PgpEncryptedDataList)
                        {
                            enc = (PgpEncryptedDataList)o;
                        }
                        else
                        {
                            enc = (PgpEncryptedDataList)pgpF.NextPgpObject();
                        }

                        if (enc == null)
                        {
                            throw new ArgumentException(String.Format("Not expected format of source file '{0}' for decrypt.", sourceFile));
                        }

                        //
                        // find the secret key
                        //

                        PgpPrivateKey sKey = null;
                        PgpPublicKeyEncryptedData pbe = null;
                        PgpSecretKeyRingBundle pgpSec = new PgpSecretKeyRingBundle(PgpUtilities.GetDecoderStream(privateKeyStream));
                        if (pgpSec == null)
                        {
                            throw new ArgumentException(String.Format("Not expected format of private key file '{0}' for decrypt.", privateKeyFile));
                        }
                        foreach (PgpPublicKeyEncryptedData pked in enc.GetEncryptedDataObjects())
                        {
                            sKey = FindSecretKey(pgpSec, pked.KeyId, passPhrase);
                            if (sKey != null)
                            {
                                pbe = pked;
                                break;
                            }
                        }

                        if (sKey == null)
                        {
                            throw new ArgumentException("Secret key for message not found.");
                        }

                        Stream clear = pbe.GetDataStream(sKey);
                        PgpObjectFactory plainFact = new PgpObjectFactory(clear);
                        PgpObject message = plainFact.NextPgpObject();

                        if (message is PgpCompressedData)
                        {
                            PgpCompressedData cData = (PgpCompressedData)message;
                            PgpObjectFactory pgpFact = new PgpObjectFactory(cData.GetDataStream());
                            message = pgpFact.NextPgpObject();

                            if (message is PgpOnePassSignatureList) //message signed, try to get literal data
                            {
                                message = pgpFact.NextPgpObject();
                            }
                        }

                        if (message is PgpLiteralData)
                        {
                            PgpLiteralData ld = (PgpLiteralData)message;
                            using (Stream outputFileStream = File.Create(outputFilePath))
                            {
                                using (Stream unc = ld.GetInputStream())
                                {
                                    Streams.PipeAll(unc, outputFileStream);
                                    unc.Close();
                                }
                                outputFileStream.Close();
                            }
                        }
                        else
                        {
                            throw new PgpException("Message is not a simple encrypted file – type unknown.");
                        }

                        if (pbe.IsIntegrityProtected())
                        {
                            if (!pbe.Verify())
                            {
                                //("message failed integrity check");
                            }
                            else
                            {
                                //("message integrity check passed");
                            }
                        }
                        else
                        {
                            //("no message integrity check");
                        }

                        privateKeyStream.Close();
                    }

                    sourceFileDecodedStream.Close();
                }

                sourceFileStream.Close();
            }
        }

        /// <summary>
        /// Sign and encrypt source file to output file using private and public keys and pass phrase. 
        /// </summary>
        /// <param name="sourceFile">source file to sign and encrypt</param>
        /// <param name="privateKeyFile">private key file to sign</param>
        /// <param name="publicKeyFile">public key file to encrypt</param>
        /// <param name="outputFile">signed and encrypted output file</param>
        /// <param name="passPhrase">pass phrase to sign</param>
        /// <param name="withIntegrityCheck">flag for entegrity check</param>
        public static void SignAndEncryptFile(string sourceFile, string privateKeyFile, string publicKeyFile, string outputFile,
            char[] passPhrase, bool withIntegrityCheck)
        {
            const int BUFFER_SIZE = 1 << 16; // should always be power of 2 
            using (FileStream outputFileStream = File.Open(outputFile, FileMode.Create))
            {
                using (Stream outputStream = new ArmoredOutputStream(outputFileStream))
                {
                    using (Stream publicKeyStream = File.OpenRead(publicKeyFile))
                    {
                        PgpPublicKey publicKey = ReadPublicKey(publicKeyStream);
                        PgpEncryptedDataGenerator encryptedDataGenerator = new PgpEncryptedDataGenerator(SymmetricKeyAlgorithmTag.Cast5, withIntegrityCheck, new SecureRandom());
                        encryptedDataGenerator.AddMethod(publicKey);
                        using (Stream encryptedOutStream = encryptedDataGenerator.Open(outputStream, new byte[BUFFER_SIZE]))
                        {
                            PgpCompressedDataGenerator compressedDataGenerator = new PgpCompressedDataGenerator(CompressionAlgorithmTag.Zip);
                            using (Stream compressedOutStream = compressedDataGenerator.Open(encryptedOutStream))
                            {
                                using (Stream privateKeyStream = PgpUtilities.GetDecoderStream(File.OpenRead(privateKeyFile)))
                                {
                                    PgpSecretKey pgpSecKey = null;
                                    PgpSecretKeyRingBundle pgpSecBundle = new PgpSecretKeyRingBundle(privateKeyStream);
                                    foreach (PgpKeyRing kr in pgpSecBundle.GetKeyRings())
                                    {
                                        if (kr is PgpSecretKeyRing)
                                        {
                                            PgpSecretKey key = ((PgpSecretKeyRing)kr).GetSecretKey();
                                            pgpSecKey = key;
                                            break;
                                        }
                                    }
                                    if (pgpSecKey == null)
                                        throw new ArgumentException("Secret Key could not be found in specified key ring bundle.");
                                    PgpPrivateKey pgpPrivKey = pgpSecKey.ExtractPrivateKey(passPhrase);
                                    PgpSignatureGenerator signatureGenerator = new PgpSignatureGenerator(pgpSecKey.PublicKey.Algorithm, HashAlgorithmTag.Sha1);
                                    signatureGenerator.InitSign(PgpSignature.BinaryDocument, pgpPrivKey);
                                    foreach (string userId in pgpSecKey.PublicKey.GetUserIds())
                                    {
                                        PgpSignatureSubpacketGenerator spGen = new PgpSignatureSubpacketGenerator();
                                        spGen.SetSignerUserId(false, userId);
                                        signatureGenerator.SetHashedSubpackets(spGen.Generate()); // Just the first one! 
                                        break;
                                    }
                                    signatureGenerator.GenerateOnePassVersion(false).Encode(compressedOutStream); // Create the Literal Data generator output stream
                                    PgpLiteralDataGenerator literalDataGenerator = new PgpLiteralDataGenerator();
                                    FileInfo privateKeyFileInfo = new FileInfo(privateKeyFile);
                                    FileInfo sourceFileInfo = new FileInfo(sourceFile); // TODO: Use lastwritetime from source file 
                                    using (Stream literalOutStream = literalDataGenerator.Open(compressedOutStream, PgpLiteralData.Binary, privateKeyFileInfo.Name, sourceFileInfo.LastWriteTime, new byte[BUFFER_SIZE]))
                                    {
                                        using (FileStream inputStream = sourceFileInfo.OpenRead())
                                        {
                                            byte[] buf = new byte[BUFFER_SIZE];
                                            int len;
                                            while ((len = inputStream.Read(buf, 0, buf.Length)) > 0)
                                            {
                                                literalOutStream.Write(buf, 0, len);
                                                signatureGenerator.Update(buf, 0, len);
                                            }
                                            literalDataGenerator.Close();
                                            signatureGenerator.Generate().Encode(compressedOutStream);
                                            compressedDataGenerator.Close();
                                            encryptedDataGenerator.Close();
                                            inputStream.Close();
                                        }

                                        literalOutStream.Close();
                                    }

                                    privateKeyStream.Close();
                                }

                                compressedOutStream.Close();
                            }

                            encryptedOutStream.Close();
                        }

                        publicKeyStream.Close();
                    }

                    outputStream.Close();
                }
            }
        }

        static public void SignFile(string sourceFileName, string privateKeyFile, char[] passPhrase, string outputFile)
        {
            using (Stream privateKeyStream = File.OpenRead(privateKeyFile))
            {
                PgpSecretKey pgpSec = ReadSigningSecretKey(privateKeyStream);
                PgpPrivateKey pgpPrivKey = null;

                pgpPrivKey = pgpSec.ExtractPrivateKey(passPhrase);

                PgpSignatureGenerator sGen = new PgpSignatureGenerator(pgpSec.PublicKey.Algorithm, HashAlgorithmTag.Sha1);

                sGen.InitSign(PgpSignature.BinaryDocument, pgpPrivKey);

                foreach (string userId in pgpSec.PublicKey.GetUserIds())
                {
                    PgpSignatureSubpacketGenerator spGen = new PgpSignatureSubpacketGenerator();

                    spGen.SetSignerUserId(false, userId);
                    sGen.SetHashedSubpackets(spGen.Generate());
                }

                CompressionAlgorithmTag compression = CompressionAlgorithmTag.Uncompressed;
                PgpCompressedDataGenerator cGen = new PgpCompressedDataGenerator(compression);

                using (FileStream outputFileStream = File.Open(outputFile, FileMode.Create))
                {
                    BcpgOutputStream bOut = new BcpgOutputStream(cGen.Open(outputFileStream));
                    sGen.GenerateOnePassVersion(false).Encode(bOut);

                    FileInfo file = new FileInfo(sourceFileName);
                    using (FileStream fIn = new FileStream(sourceFileName, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        PgpLiteralDataGenerator lGen = new PgpLiteralDataGenerator();
                        using (Stream lOut = lGen.Open(bOut, PgpLiteralData.Binary, file))
                        {
                            int ch = 0;
                            while ((ch = fIn.ReadByte()) >= 0)
                            {
                                lOut.WriteByte((byte)ch);
                                sGen.Update((byte)ch);
                            }

                            fIn.Close();
                            lGen.Close();
                            lOut.Close();
                        }

                        fIn.Close();
                    }
                    sGen.Generate().Encode(bOut);
                    cGen.Close();
                    outputFileStream.Close();
                }                
            }
        }

        /// <summary>
        /// Verify sign using public key, in the case we try to verify encrypted file, we need also private key to encrypt before verify.
        /// </summary>
        /// <param name="sourceFile">source signed file encrypted and signed or just signed </param>
        /// <param name="publicKeyFile">public key file to verify sign</param>
        /// <param name="privateKeyFile">private key file for encrypted file</param>
        /// <param name="passPhrase">pass phrase for private key</param>
        /// <returns>true if sign is valid, otherwice false</returns>
        public static bool VerifySignFile(string sourceFile, string publicKeyFile, string privateKeyFile, string passPhrase)
        {
            bool result = false;
            try
            {
                using (Stream sourceFileStream = File.OpenRead(sourceFile))
                {
                    using (Stream sourceFileDecoderStream = PgpUtilities.GetDecoderStream(sourceFileStream))
                    {
                        PgpOnePassSignature ops = null;

                        PgpObjectFactory pgpFact = new PgpObjectFactory(sourceFileDecoderStream);
                        PgpObject o = pgpFact.NextPgpObject();

                        if (o is PgpCompressedData)
                        {
                            PgpCompressedData c1 = (PgpCompressedData)o;

                            pgpFact = new PgpObjectFactory(c1.GetDataStream());

                            PgpOnePassSignatureList p1 = (PgpOnePassSignatureList)pgpFact.NextPgpObject();
                            ops = p1[0];

                            PgpLiteralData p2 = (PgpLiteralData)pgpFact.NextPgpObject();
                            Stream dIn = p2.GetInputStream();

                            using (Stream publicKeyFileStream = File.OpenRead(publicKeyFile))
                            {
                                using (Stream publicKeyFileDecoderStream = PgpUtilities.GetDecoderStream(publicKeyFileStream))
                                {
                                    PgpPublicKeyRingBundle pgpRing = new PgpPublicKeyRingBundle(publicKeyFileDecoderStream);
                                    PgpPublicKey key = pgpRing.GetPublicKey(ops.KeyId);
                                    if (key != null)
                                    {
                                        using (Stream fos = File.Create(p2.FileName))
                                        {
                                            ops.InitVerify(key);

                                            int ch;
                                            while ((ch = dIn.ReadByte()) >= 0)
                                            {
                                                ops.Update((byte)ch);
                                                fos.WriteByte((byte)ch);
                                            }
                                            fos.Close();

                                            PgpSignatureList p3 = (PgpSignatureList)pgpFact.NextPgpObject();
                                            PgpSignature firstSig = p3[0];

                                            result = ops.Verify(firstSig);
                                        }
                                    }
                                    publicKeyFileDecoderStream.Close();
                                }
                                publicKeyFileStream.Close();
                            }
                        }
                        else
                        {
                            using (Stream privateKeyStream = File.OpenRead(privateKeyFile))
                            {
                                PgpEncryptedDataList enc;

                                if (o is PgpEncryptedDataList)
                                {
                                    enc = (PgpEncryptedDataList)o;
                                }
                                else
                                {
                                    enc = (PgpEncryptedDataList)pgpFact.NextPgpObject();
                                }

                                PgpPrivateKey sKey = null;
                                PgpPublicKeyEncryptedData pbe = null;
                                PgpSecretKeyRingBundle pgpSec = new PgpSecretKeyRingBundle(PgpUtilities.GetDecoderStream(privateKeyStream));

                                char[] pass = string.IsNullOrEmpty(passPhrase) ? new char[0] : passPhrase.ToCharArray();
                                foreach (PgpPublicKeyEncryptedData pked in enc.GetEncryptedDataObjects())
                                {
                                    sKey = FindSecretKey(pgpSec, pked.KeyId, pass);
                                    if (sKey != null)
                                    {
                                        pbe = pked;
                                        break;
                                    }
                                }

                                if (sKey != null)
                                {
                                    Stream clear = pbe.GetDataStream(sKey);
                                    PgpObjectFactory plainFact = new PgpObjectFactory(clear);
                                    PgpObject message = plainFact.NextPgpObject();
                                    PgpOnePassSignatureList p1 = null;

                                    if (message is PgpCompressedData)
                                    {
                                        PgpCompressedData cData = (PgpCompressedData)message;
                                        PgpObjectFactory pgpF = new PgpObjectFactory(cData.GetDataStream());
                                        message = pgpF.NextPgpObject();

                                        if (message is PgpOnePassSignatureList) //message signed
                                        {
                                            p1 = (PgpOnePassSignatureList)message;
                                        }

                                        if (p1 != null)
                                        {
                                            ops = p1[0];

                                            PgpLiteralData p2 = (PgpLiteralData)pgpF.NextPgpObject();
                                            Stream dIn = p2.GetInputStream();

                                            using (Stream publicKeyFileStream = File.OpenRead(publicKeyFile))
                                            {
                                                using (Stream publicKeyFileDecoderStream = PgpUtilities.GetDecoderStream(publicKeyFileStream))
                                                {
                                                    PgpPublicKeyRingBundle pgpRing = new PgpPublicKeyRingBundle(publicKeyFileDecoderStream);
                                                    PgpPublicKey key = pgpRing.GetPublicKey(ops.KeyId);
                                                    if (key != null)
                                                    {
                                                        using (Stream fos = File.Create(p2.FileName))
                                                        {
                                                            ops.InitVerify(key);

                                                            int ch;
                                                            while ((ch = dIn.ReadByte()) >= 0)
                                                            {
                                                                ops.Update((byte)ch);
                                                                fos.WriteByte((byte)ch);
                                                            }
                                                            fos.Close();

                                                            PgpSignatureList p3 = (PgpSignatureList)pgpF.NextPgpObject();
                                                            PgpSignature firstSig = p3[0];

                                                            result = ops.Verify(firstSig);
                                                        }
                                                    }
                                                    publicKeyFileDecoderStream.Close();
                                                }
                                                publicKeyFileStream.Close();
                                            }
                                        }
                                    }
                                }
                                privateKeyStream.Close();
                            }
                        }
                    }
                    sourceFileStream.Close();
                }
            }
            catch
            {
                result = false;
            }

            return result;
        }


        static private PgpSecretKey ReadSigningSecretKey(Stream keyFileStream)
        {
            using (Stream keyFileDecoderStream = PgpUtilities.GetDecoderStream(keyFileStream))
            {
                PgpSecretKeyRingBundle pgpSec = new PgpSecretKeyRingBundle(keyFileDecoderStream);
                PgpSecretKey key = null;
                System.Collections.IEnumerator rIt = pgpSec.GetKeyRings().GetEnumerator();
                while (key == null && rIt.MoveNext())
                {
                    PgpSecretKeyRing kRing = (PgpSecretKeyRing)rIt.Current;
                    System.Collections.IEnumerator kIt = kRing.GetSecretKeys().GetEnumerator();
                    while (key == null && kIt.MoveNext())
                    {
                        PgpSecretKey k = (PgpSecretKey)kIt.Current;
                        if (k.IsSigningKey)
                            key = k;
                    }
                }

                if (key == null)
                    throw new Exception("Wrong private key - Can't find signing key in key ring.");
                else
                    return key;
            }
        }

        static private PgpPrivateKey FindSecretKey(PgpSecretKeyRingBundle pgpSec, long keyId, char[] pass)
        {
            PgpSecretKey pgpSecKey = pgpSec.GetSecretKey(keyId);
            if (pgpSecKey == null)
            {
                return null;
            }
            return pgpSecKey.ExtractPrivateKey(pass);
        }

        static private PgpPublicKey ReadPublicKey(Stream inputStream)
        {
            inputStream = PgpUtilities.GetDecoderStream(inputStream);
            PgpPublicKeyRingBundle pgpPub = new PgpPublicKeyRingBundle(inputStream);            
            foreach (PgpPublicKeyRing kRing in pgpPub.GetKeyRings())               
            {
                foreach (PgpPublicKey k in kRing.GetPublicKeys())
                {
                    if (k.IsEncryptionKey)
                    {
                        return k;
                    }
                }
            }
            throw new ArgumentException("Can't find encryption key in key ring.");
        }
        static private string ComposeOutFile(string sourceFile, string outputPath, string addString, string removeString)
        {
            string result = "";
            string outPath = "";

            if (!string.IsNullOrEmpty(sourceFile))
            {
                result = Path.GetFileName(sourceFile);
                if (!Path.HasExtension(result)) // handle path without filename
                {
                    result = "";
                }
                
                if (!string.IsNullOrEmpty(outputPath))
                {
                    outPath = Path.HasExtension(outputPath) ? Path.GetDirectoryName(outputPath) : Path.GetFullPath(outputPath);
                    outPath = Utility.RemoveTailSlash(outPath);
                }
                else
                {
                    outPath = Path.HasExtension(sourceFile) ? Path.GetDirectoryName(sourceFile) : Path.GetFullPath(sourceFile);
                    outPath = Utility.RemoveTailSlash(outPath);
                }


                if (!string.IsNullOrEmpty(removeString))
                {
                    result = result.Replace(removeString, "");
                }

                if (!string.IsNullOrEmpty(addString))
                {
                    if (string.IsNullOrEmpty(result))
                    {
                        result = "*.*";
                    }
                    result = String.Format("{0}{1}", result, addString);
                }
            }

            result = Path.Combine(outPath, result);

            return result;
        }

        /// <summary>
        /// Decrypt source file to output file using private key and passphrase.
        /// </summary>
        /// <param name="sourceFile">source file to decrypt</param>
        /// <param name="privateKeyFile">private key file</param>
        /// <param name="passPhrase">passphrase</param>
        /// <param name="outputFilePath">decrypted output file </param>
        static public bool VerifySignEncrypted(string sourceFile, string publicKeyFile)
        {
            bool result = false;
            using (Stream sourceFileStream = File.OpenRead(sourceFile))
            {
                using (Stream sourceFileDecodedStream = PgpUtilities.GetDecoderStream(sourceFileStream))
                {
                    using (Stream publicKeyStream = File.OpenRead(publicKeyFile))
                    {
                        PgpObjectFactory pgpF = new PgpObjectFactory(sourceFileDecodedStream);
                        PgpEncryptedDataList enc;
                        PgpObject o = pgpF.NextPgpObject();
                        //
                        // the first object might be a PGP marker packet.
                        //

                        if (o is PgpEncryptedDataList)
                        {
                            enc = (PgpEncryptedDataList)o;
                        }
                        else
                        {
                            enc = (PgpEncryptedDataList)pgpF.NextPgpObject();
                        }

                        //
                        // find the secret key
                        //

                        PgpPrivateKey sKey = null;
                        PgpPublicKeyEncryptedData pbe = null;
                        PgpPublicKeyRingBundle pgpSec = new PgpPublicKeyRingBundle(PgpUtilities.GetDecoderStream(publicKeyStream));
                        
                        /*
                        foreach (PgpPublicKeyEncryptedData pked in enc.GetEncryptedDataObjects())
                        {
                            sKey = FindSecretKey(pgpSec, pked.KeyId, new char[0]);
                            if (sKey != null)
                            {
                                pbe = pked;
                                break;
                            }
                        }
                        */
                        if (sKey == null)
                        {
                            throw new ArgumentException("secret key for message not found.");
                        }

                        Stream clear = pbe.GetDataStream(sKey);
                        PgpObjectFactory plainFact = new PgpObjectFactory(clear);
                        PgpObject message = plainFact.NextPgpObject();

                        if (message is PgpCompressedData)
                        {
                            PgpCompressedData cData = (PgpCompressedData)message;
                            PgpObjectFactory pgpFact = new PgpObjectFactory(cData.GetDataStream());
                            message = pgpFact.NextPgpObject();

                            if (message is PgpOnePassSignatureList) //message signed, try to get literal data
                            {
                                message = pgpFact.NextPgpObject();
                            }
                        }

                        publicKeyStream.Close();
                    }
                    sourceFileDecodedStream.Close();
                }

                sourceFileStream.Close();
            }
            return result;
        }

    }
}
