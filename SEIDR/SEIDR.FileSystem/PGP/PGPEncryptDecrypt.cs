using Org.BouncyCastle.Bcpg;
using Org.BouncyCastle.Bcpg.OpenPgp;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEIDR.FileSystem
{
    public class PGPEncryptDecrypt
    {

        #region Public Region

        public static void GenerateKey(string username, string password, string keyStoreUrl)
        {

            IAsymmetricCipherKeyPairGenerator kpg = GeneratorUtilities.GetKeyPairGenerator("RSA");
            // new RsaKeyPairGenerator();

            kpg.Init(new RsaKeyGenerationParameters(BigInteger.ValueOf(0x13), new SecureRandom(), 1024, 8));
            AsymmetricCipherKeyPair kp = kpg.GenerateKeyPair();
            FileStream out1 = new FileInfo(string.Format("{0}_PrivateKey.txt", keyStoreUrl)).OpenWrite();
            FileStream out2 = new FileInfo(string.Format("{0}_PublicKey.txt", keyStoreUrl)).OpenWrite();
            ExportKeyPair(out1, out2, kp.Public, kp.Private, PublicKeyAlgorithmTag.RsaGeneral, SymmetricKeyAlgorithmTag.Cast5, username, password.ToCharArray(), true);

            out1.Close();
            out2.Close();

        }

        public static void Encrypt(string filePath, string publicKeyFile, string OutputFilePath)
        {

            Stream keyIn, fos;
            
            keyIn = File.OpenRead(publicKeyFile);
            fos = File.Create(OutputFilePath);
            EncryptFile(fos, filePath, ReadPublicKey(keyIn), true, true);
            keyIn.Close();
            fos.Close();
        }

        public static void Decrypt(string filePath, string privateKeyFile, string passPhrase, string pathToSaveFile)
        {
            Stream fin = File.OpenRead(filePath);
            Stream keyIn = File.OpenRead(privateKeyFile);
            DecryptFile(fin, keyIn, passPhrase.ToCharArray(), pathToSaveFile);
            fin.Close();
            keyIn.Close();

        }

        public static void SignAndEncrypt(string actualFileName, string embeddedFileName, string publicKeyFile, string OutputFileName,
            string password, bool armor, bool withIntegrityCheck)
        {
            long keyId = 0;
            //Stream fin = File.OpenRead(actualFileName);
            Stream fin = File.OpenRead(embeddedFileName);
            //Stream fin = File.OpenRead(publicKeyFile);
            Stream keyIn = File.OpenRead(publicKeyFile);
            
            fin = PgpUtilities.GetDecoderStream(fin);
            string fstr = fin.ToString();
            PgpSecretKeyRingBundle pgpSecBundle = new PgpSecretKeyRingBundle(fin);
            foreach (PgpKeyRing kr in pgpSecBundle.GetKeyRings())
            {
                if (kr is PgpSecretKeyRing)
                {
                    PgpSecretKey key = ((PgpSecretKeyRing)kr).GetSecretKey();
                    keyId = key.KeyId;
                    break;
                }
            }

            //PgpSecretKeyRing skr = new PgpSecretKeyRing(fin);
            //PgpSecretKey key = skr.GetSecretKey();
            
            /*
            PgpObjectFactory pgpF = new PgpObjectFactory(fin);
            
            PgpEncryptedDataList enc;
            PgpObject o = pgpF.NextPgpObject();
            // the first object might be a PGP marker packet.
            if (o is PgpEncryptedDataList)
            {
                enc = (PgpEncryptedDataList)o;
            }
            else
            {
                enc = (PgpEncryptedDataList)pgpF.NextPgpObject();
            }

            foreach (PgpPublicKeyEncryptedData pked in enc.GetEncryptedDataObjects())
            {
                keyId = pked.KeyId;
                break;
            }
            */

            //var publickKeyData = ReadPublicKey(keyIn);
            SignAndEncryptFile(actualFileName, embeddedFileName, keyIn, keyId, OutputFileName, password.ToCharArray(), armor, withIntegrityCheck, ReadPublicKey(keyIn));

            fin.Close();
            keyIn.Close();
        }

        public static void SignAndEncryptFile(string actualFileName, string embeddedFileName, Stream keyIn, long keyId, string OutputFileName,
            char[] password, bool armor, bool withIntegrityCheck, PgpPublicKey encKey)
        {
            const int BUFFER_SIZE = 1 << 16; // should always be power of 2 
            Stream outputStream = File.Open(OutputFileName, FileMode.Create);
            if (armor)
                outputStream = new ArmoredOutputStream(outputStream); // Init encrypted data generator 

            PgpEncryptedDataGenerator encryptedDataGenerator = new PgpEncryptedDataGenerator(SymmetricKeyAlgorithmTag.Cast5, withIntegrityCheck, new SecureRandom());
            encryptedDataGenerator.AddMethod(encKey);
            Stream encryptedOut = encryptedDataGenerator.Open(outputStream, new byte[BUFFER_SIZE]); // Init compression
            PgpCompressedDataGenerator compressedDataGenerator = new PgpCompressedDataGenerator(CompressionAlgorithmTag.Zip);
            Stream compressedOut = compressedDataGenerator.Open(encryptedOut); // Init signature 

            PgpSecretKey pgpSecKey = null;
            Stream fin = File.OpenRead(embeddedFileName);
            fin = PgpUtilities.GetDecoderStream(fin);
            string fstr = fin.ToString();
            PgpSecretKeyRingBundle pgpSecBundle = new PgpSecretKeyRingBundle(fin);
            foreach (PgpKeyRing kr in pgpSecBundle.GetKeyRings())
            {
                if (kr is PgpSecretKeyRing)
                {
                    PgpSecretKey key = ((PgpSecretKeyRing)kr).GetSecretKey();
                    //keyId = key.KeyId;
                    pgpSecKey = key;
                    break;
                }
            }


            /*
            PgpSecretKeyRingBundle pgpSecBundle = new PgpSecretKeyRingBundle(PgpUtilities.GetDecoderStream(keyIn));
            PgpSecretKey pgpSecKey = pgpSecBundle.GetSecretKey(keyId);
            */
            if (pgpSecKey == null)
                throw new ArgumentException(keyId.ToString("X") + " could not be found in specified key ring bundle.", "keyId");
            PgpPrivateKey pgpPrivKey = pgpSecKey.ExtractPrivateKey(password);
            PgpSignatureGenerator signatureGenerator = new PgpSignatureGenerator(pgpSecKey.PublicKey.Algorithm, HashAlgorithmTag.Sha1);
            signatureGenerator.InitSign(PgpSignature.BinaryDocument, pgpPrivKey);
            foreach (string userId in pgpSecKey.PublicKey.GetUserIds())
            {
                PgpSignatureSubpacketGenerator spGen = new PgpSignatureSubpacketGenerator();
                spGen.SetSignerUserId(false, userId);
                signatureGenerator.SetHashedSubpackets(spGen.Generate()); // Just the first one! 
                break;
            }
            signatureGenerator.GenerateOnePassVersion(false).Encode(compressedOut); // Create the Literal Data generator output stream
            PgpLiteralDataGenerator literalDataGenerator = new PgpLiteralDataGenerator();
            FileInfo embeddedFile = new FileInfo(embeddedFileName);
            FileInfo actualFile = new FileInfo(actualFileName); // TODO: Use lastwritetime from source file 
            Stream literalOut = literalDataGenerator.Open(compressedOut, PgpLiteralData.Binary, embeddedFile.Name, actualFile.LastWriteTime, new byte[BUFFER_SIZE]); // Open the input file 
            FileStream inputStream = actualFile.OpenRead();
            byte[] buf = new byte[BUFFER_SIZE];
            int len;
            while ((len = inputStream.Read(buf, 0, buf.Length)) > 0)
            {
                literalOut.Write(buf, 0, len);
                signatureGenerator.Update(buf, 0, len);
            }

            literalOut.Close();
            literalDataGenerator.Close();
            signatureGenerator.Generate().Encode(compressedOut);
            compressedOut.Close();
            compressedDataGenerator.Close();
            encryptedOut.Close();
            encryptedDataGenerator.Close();
            inputStream.Close();

            if (armor)
                outputStream.Close();
        }

        #endregion

        #region Private Region

        /**
        * A simple routine that opens a key ring file and loads the first available key suitable for
        * encryption.
        * @param in
        * @return
        * @m_out
*/
        private static PgpPublicKey ReadPublicKey(Stream inputStream)
        {
            inputStream = PgpUtilities.GetDecoderStream(inputStream);
            PgpPublicKeyRingBundle pgpPub = new PgpPublicKeyRingBundle(inputStream);

            // we just loop through the collection till we find a key suitable for encryption, in the real
            // world you would probably want to be a bit smarter about this.
            // iterate through the key rings.
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

        /**

        * Search a secret key ring collection for a secret key corresponding to
        * keyId if it exists.
        * @param pgpSec a secret key ring collection.
        * @param keyId keyId we want.
        * @param pass passphrase to decrypt secret key with.
        * @return

*/
        private static PgpPrivateKey FindSecretKey(PgpSecretKeyRingBundle pgpSec, long keyId, char[] pass)
        {
            PgpSecretKey pgpSecKey = pgpSec.GetSecretKey(keyId);
            if (pgpSecKey == null)
            {
                return null;
            }
            return pgpSecKey.ExtractPrivateKey(pass);
        }

        /**

        * decrypt the passed in message stream*/
        private static void DecryptFile(Stream inputStream, Stream keyIn, char[] passwd, string pathToSaveFile)
        {
            inputStream = PgpUtilities.GetDecoderStream(inputStream);
            try
            {
                PgpObjectFactory pgpF = new PgpObjectFactory(inputStream);
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
                PgpSecretKeyRingBundle pgpSec = new PgpSecretKeyRingBundle(PgpUtilities.GetDecoderStream(keyIn));

                foreach (PgpPublicKeyEncryptedData pked in enc.GetEncryptedDataObjects())
                {
                    sKey = FindSecretKey(pgpSec, pked.KeyId, passwd);
                    if (sKey != null)
                    {
                        pbe = pked;
                        break;
                    }
                }

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
                }

                if (message is PgpLiteralData)
                {

                    PgpLiteralData ld = (PgpLiteralData)message;

                    Stream fOut = File.Create(pathToSaveFile);
                    Stream unc = ld.GetInputStream();
                    Streams.PipeAll(unc, fOut);
                    fOut.Close();
                }
                else if (message is PgpOnePassSignatureList)
                {
                    throw new PgpException("encrypted message contains a signed message – not literal data.");
                }
                else
                {
                    throw new PgpException("message is not a simple encrypted file – type unknown.");
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
            }
            catch (PgpException e)
            {
                Exception underlyingException = e.InnerException;
            }
        }
        private static void EncryptFile(Stream outputStream, string fileName, PgpPublicKey encKey, bool armor, bool withIntegrityCheck)
        {
            if (armor)
            {
                outputStream = new ArmoredOutputStream(outputStream);
            }
            try
            {

                MemoryStream bOut = new MemoryStream();
                PgpCompressedDataGenerator comData = new PgpCompressedDataGenerator(CompressionAlgorithmTag.Zip);
                PgpUtilities.WriteFileToLiteralData(comData.Open(bOut),
                PgpLiteralData.Binary,
                new FileInfo(fileName));
                comData.Close();
                PgpEncryptedDataGenerator cPk = new PgpEncryptedDataGenerator(SymmetricKeyAlgorithmTag.Cast5, withIntegrityCheck, new SecureRandom());

                cPk.AddMethod(encKey);
                byte[] bytes = bOut.ToArray();
                Stream cOut = cPk.Open(outputStream, bytes.Length);
                cOut.Write(bytes, 0, bytes.Length);
                cOut.Close();

                if (armor)
                {
                    outputStream.Close();
                }

            }

            catch (PgpException e)
            {
                Exception underlyingException = e.InnerException;
                if (underlyingException != null)
                {
                    //(underlyingException.Message);
                    //(underlyingException.StackTrace);
                }

            }

        }

        private static void ExportKeyPair(
        Stream secretOut,
        Stream publicOut,
        AsymmetricKeyParameter publicKey,
        AsymmetricKeyParameter privateKey,
        PublicKeyAlgorithmTag PublicKeyAlgorithmTag,
        SymmetricKeyAlgorithmTag SymmetricKeyAlgorithmTag,
        string identity,
        char[] passPhrase,
        bool armor)
        {
            if (armor)
            {
                secretOut = new ArmoredOutputStream(secretOut);
            }

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
            // ,"BC"
            );

            secretKey.Encode(secretOut);
            secretOut.Close();

            if (armor)
            {
                publicOut = new ArmoredOutputStream(publicOut);
            }

            PgpPublicKey key = secretKey.PublicKey;
            key.Encode(publicOut);
            publicOut.Close();
        }

    }

    #endregion
}
