/****************************************************************************/
/*                                                                          */
/*    The Mondo Libraries  							                        */
/*                                                                          */
/*        Namespace: Mondo.Security							                */
/*             File: Crypto.cs					    		                */
/*        Class(es): Crypto				         		                    */
/*          Purpose: Provides data encryption and decryption                */
/*                                                                          */
/*  Original Author: Jim Lightfoot                                          */
/*    Creation Date: 5 Jul 2003                                             */
/*                                                                          */
/*   Copyright (c) 2003-2017 - Jim Lightfoot, All rights reserved           */
/*                                                                          */
/*  Licensed under the MIT license:                                         */
/*    http://www.opensource.org/licenses/mit-license.php                    */
/*                                                                          */
/****************************************************************************/

using System;
using System.IO;
using System.Text;
using System.Security;
using System.Security.Cryptography;
using System.Runtime.InteropServices;
using Mondo.Common;

namespace Mondo.Security
{
    /****************************************************************************/
    /****************************************************************************/
	public class Crypto : IEncryptor, IDisposable
	{
        private readonly byte[] _key;
        private readonly byte[] _IV;

      #if DEBUG
        private readonly string _strIV;
        private readonly string _strKey;
      #endif

		/****************************************************************************/
        public Crypto(byte[] key, byte[] IV)
		{
            _key = key.DeepClone();
            _IV  = IV.DeepClone();
      #if DEBUG
            _strIV = _IV.ToBase64String();
            _strKey = _key.ToBase64String();
      #endif
		}

		/****************************************************************************/
        public Crypto(byte[] key_iv)
		{
            using(BinaryList aList = new BinaryList(key_iv))
            { 
                _key = aList[0].DeepClone();
                _IV  = aList[1].DeepClone();
            }
      #if DEBUG
            _strIV = _IV.ToBase64String();
            _strKey = _key.ToBase64String();
      #endif
		}

        #if DEBUG

		/****************************************************************************/
        public byte[] Key
		{
            get
            {
                return(_key);
            }
		}

        #endif

     	/****************************************************************************/
        public static Crypto GenerateNew()
		{
            using(RijndaelManaged objAlgo = new RijndaelManaged())
            {
                objAlgo.GenerateKey();
                objAlgo.GenerateIV();

                return(new Crypto(objAlgo.Key, objAlgo.IV));
            }
        }

        /*************************************************************************/
        public void Dispose()
        {
            _key.Clear();
            _IV.Clear();
        }
        
        /****************************************************************************/
        public byte[] ToArray()
        {
            using(BinaryList aList = new BinaryList())
            { 
                aList.Add(_key);
                aList.Add(_IV);

                return(aList.ToArray());
            }
        }

        #region GenerateKey

        /****************************************************************************/
        public static void GenerateNewKey(SymmetricAlgorithm algorithm)
        {
            algorithm.GenerateKey();
            algorithm.GenerateIV();

            string strWrite = "byte[] key  = {";

            foreach(byte b in algorithm.Key)
                strWrite += b.ToString() + ", ";

            strWrite = strWrite.Substring(0, strWrite.Length - 2) + "};\r\n";

            strWrite += "byte[] IV  = {";

            foreach(byte c in algorithm.IV)
                strWrite += c.ToString() + ", ";

            strWrite = strWrite.Substring(0, strWrite.Length - 2) + "};\r\n";

            strWrite += "\r\n\r\n";
            strWrite += Convert.ToBase64String(algorithm.Key) + "\r\n";
            strWrite += Convert.ToBase64String(algorithm.IV) + "\r\n";

            cDebug.ToFile(strWrite, "Common.Crypto\\keys.txt");
        }

        /****************************************************************************/
        public static string[] GenerateKey()
        {
            using(RijndaelManaged crypto = new RijndaelManaged())
            {
                return(GenerateKey(crypto));
            }
        }

        /****************************************************************************/
        public static string[] GenerateKey(SymmetricAlgorithm algorithm)
        {
            algorithm.GenerateKey();
            algorithm.GenerateIV();

            string[] aParts = new string[2];

            aParts[0] = Convert.ToBase64String(algorithm.Key);
            aParts[1] = Convert.ToBase64String(algorithm.IV);

            return(aParts);
        }

        /****************************************************************************/
        public static byte[] GenerateKey(out byte[] aIV)
        {
            using(RijndaelManaged crypto = new RijndaelManaged())
            {
                crypto.GenerateKey();
                crypto.GenerateIV();

                aIV  = crypto.IV;
                return(crypto.Key);
            }
        }

        #endregion

        /****************************************************************************/
        public static byte[] Encrypt(byte[] aData, byte[] aPassword, byte[] aSalt)
        {
            byte[] aKey = null;
            byte[] aIV  = null;

            GenerateRC2Key(aPassword, aSalt, out aKey, out aIV);

            using(RC2CryptoServiceProvider algorithm = new RC2CryptoServiceProvider())
            {
                algorithm.Key = aKey;
                algorithm.IV  = aIV;

                // setup an RC2 object to encrypt with the derived key                              
                return(EncryptBytes(algorithm, aData));
            }
        }

        /****************************************************************************/
        public static string Encrypt(string strData, string password, byte[] aSalt)
        {
            byte[] aKey = null;
            byte[] aIV  = null;

            GenerateRC2Key(password, aSalt, out aKey, out aIV);

            using(RC2CryptoServiceProvider algorithm = new RC2CryptoServiceProvider())
            {
                algorithm.Key = aKey;
                algorithm.IV  = aIV;

                // setup an RC2 object to encrypt with the derived key                              
                return(InternalEncrypt(algorithm, strData));
            }
        }

        /****************************************************************************/
        private SymmetricAlgorithm CreateAlgorithm()
        {
            return(new RijndaelManaged {Key = _key, IV = _IV});
        }

        /****************************************************************************/
        public string Encrypt(string strData)
        {
            using(SymmetricAlgorithm algorithm = CreateAlgorithm())
            {
                return(InternalEncrypt(algorithm, strData));
            }
        }

        /****************************************************************************/
        public byte[] Encrypt(byte[] aData)
        {
            using(SymmetricAlgorithm algorithm = CreateAlgorithm())
            {
                return(EncryptBytes(algorithm, aData));
            }
        }

        /****************************************************************************/
        public static string Decrypt(string strData, string password, byte[] aSalt)
        {
            byte[] aKey = null;
            byte[] aIV  = null;

            GenerateRC2Key(password, aSalt, out aKey, out aIV);

            using(RC2CryptoServiceProvider algorithm = new RC2CryptoServiceProvider {Key = aKey, IV = aIV})
            {
                return(InternalDecrypt(algorithm, strData));
            }
        }

        /****************************************************************************/
        public static byte[] Decrypt(byte[] aData, byte[] aPassword, byte[] aSalt)
        {
            byte[] aKey = null;
            byte[] aIV  = null;

            GenerateRC2Key(aPassword, aSalt, out aKey, out aIV);

            using(RC2CryptoServiceProvider algorithm = new RC2CryptoServiceProvider {Key = aKey, IV = aIV})
            {
                return(DecryptBytes(algorithm, aData));
            }
        }

        /****************************************************************************/
        public byte[] Decrypt(byte[] aEncrypted, int offset = 0, int length = -1)
        {
            using(SymmetricAlgorithm algorithm = CreateAlgorithm())
            {
                return(DecryptBytes(algorithm, aEncrypted, offset, length));
            }
        }

        /****************************************************************************/
        public string Decrypt(string encrypted)
        {
            using(SymmetricAlgorithm algorithm = CreateAlgorithm())
            {
                return(InternalDecrypt(algorithm, encrypted));
            }
        }

        /****************************************************************************/
        public byte[] DecryptBytes(byte[] aData)
        {
            return(Decrypt(aData));
        }

        #region Password and Salt

        private const int kPasswordHashIterations = 5000;

        /****************************************************************************/
        public static string Hash(string password, byte[] aSalt, int nIterations = kPasswordHashIterations)
        {
            byte[] aHashBytes = HashBytes(password, aSalt, nIterations);

            return(Convert.ToBase64String(aHashBytes));
        }       

        /****************************************************************************/
        public static byte[] HashBytes(string password, byte[] aSalt, int nIterations = kPasswordHashIterations)
        {
            if(nIterations == 0)
                nIterations = kPasswordHashIterations;

            using(Rfc2898DeriveBytes objHasher = new Rfc2898DeriveBytes(password, aSalt, nIterations))
            {
                return objHasher.GetBytes(128);
            }
        }       

        /****************************************************************************/
        public static char[] Hash(byte[] aPassword, byte[] aSalt, int nIterations = kPasswordHashIterations)
        {
            using(Rfc2898DeriveBytes objHasher = new Rfc2898DeriveBytes(aPassword, aSalt, nIterations))
            {
                byte[] aHashBytes = objHasher.GetBytes(128);
                char[] outArray   = new char[aHashBytes.Length];

                Convert.ToBase64CharArray(aHashBytes, 0, aHashBytes.Length, outArray, 0);

                aHashBytes.Clear();

                return(outArray);
            }
        }       

        /****************************************************************************/
        public static string GenerateNewPassword(int size)
        {
            return(GenerateSaltString(size).Replace("=", ""));
        }

        /****************************************************************************/
        public static string GenerateSaltString(int size)
        {
            return(Convert.ToBase64String(GenerateSalt(size)));
        }

        /****************************************************************************/
        public static byte[] GenerateSalt(int size)
        {
            using(RNGCryptoServiceProvider objCrypto = new RNGCryptoServiceProvider())
            {
                byte[] aBuffer = new byte[size];

                objCrypto.GetBytes(aBuffer);

                return(aBuffer);
            }
        }

        #endregion

        #region Private Methods

        /****************************************************************************/
        private static byte[] EncryptBytes(SymmetricAlgorithm algorithm, byte[] aData)
        {
            // Get an encryptor.
            using(ICryptoTransform encryptor = algorithm.CreateEncryptor())
            {
                // Encrypt the data.
                using(MemoryStream msEncrypt  = new MemoryStream())
                {
                    using(CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        // Write all data to the crypto stream and flush it.
                        csEncrypt.Write(aData, 0, aData.Length);
                        csEncrypt.FlushFinalBlock();

                        // Get encrypted array of bytes.
                        return(msEncrypt.ToArray());
                    }
                }
            }
        }        
               
        /****************************************************************************/
        private static string InternalEncrypt(SymmetricAlgorithm algorithm, string data)
        {
            if(string.IsNullOrEmpty(data))
                return("");

            // Get encrypted array of bytes.
            byte[] toEncrypt = Encoding.UTF8.GetBytes(data); 
            byte[] encrypted = EncryptBytes(algorithm, toEncrypt);

            string strReturn = Convert.ToBase64String(encrypted);

            toEncrypt.Clear();

            return(strReturn);
        }        
               
        /****************************************************************************/
        private static byte[] EncryptBytes(SymmetricAlgorithm algorithm, SecureString secureData)
        {
            byte[] aData = null;
            IntPtr pData    = Marshal.SecureStringToBSTR(secureData);

            try
            {
                try
                {
                    string strData = Marshal.PtrToStringBSTR(pData);

                    // Convert the data to a byte array.
                    aData = Encoding.UTF8.GetBytes(strData); 
                }
                finally
                {
                    Marshal.ZeroFreeBSTR(pData);
                }

               return(EncryptBytes(algorithm, aData));
            }
            finally
            {
                if(aData != null)
                {
                    aData.Clear();
                    aData = null;
                }
            }
         }        
        
        /****************************************************************************/
        private static string InternalDecrypt(SymmetricAlgorithm algorithm, string encrypted)
        {
            if(string.IsNullOrEmpty(encrypted))
                return("");

            byte[] aEncrypted = Convert.FromBase64String(encrypted);
            byte[] aDecrypted = DecryptBytes(algorithm, aEncrypted);
            string strResult  = System.Text.Encoding.UTF8.GetString(aDecrypted); 

            aDecrypted.Clear();

            while(strResult[strResult.Length-1] == '\0')
                strResult = strResult.Substring(0, strResult.Length-1);

            return(strResult);
        }

        /****************************************************************************/
        private static byte[] DecryptBytes(SymmetricAlgorithm algorithm, string encrypted)
        {
            byte[] aEncrypted = Convert.FromBase64String(encrypted);

            return(DecryptBytes(algorithm, aEncrypted));
        }

        /****************************************************************************/
        private static byte[] DecryptBytes(SymmetricAlgorithm algorithm, byte[] aEncrypted, int offset = 0, int length = -1)
        {
            using(ICryptoTransform decryptor = algorithm.CreateDecryptor())
            {
                // Now decrypt the encrypted data using the decryptor
                using(MemoryStream msDecrypt = new MemoryStream(aEncrypted))
                {
                    using(CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        byte[] fromEncrypt = new byte[aEncrypted.Length * 3];

                        // Read the data out of the crypto stream.
                        int iRead = csDecrypt.Read(fromEncrypt, 0, fromEncrypt.Length);

                        return(fromEncrypt.DeepClone(0, iRead));
                    }
                }
            }
        }

        /****************************************************************************/
        private static void GenerateRC2Key(string password, byte[] aSalt, out byte[] aKey, out byte[] aIV)
        {
            using(Rfc2898DeriveBytes pwdGen = new Rfc2898DeriveBytes(password, aSalt, 5000))
            {
                aKey = pwdGen.GetBytes(16);
                aIV  = pwdGen.GetBytes(8);
            }
        }

        /****************************************************************************/
        private static void GenerateRC2Key(byte[] aPassword, byte[] aSalt, out byte[] aKey, out byte[] aIV)
        {
            using(Rfc2898DeriveBytes pwdGen = new Rfc2898DeriveBytes(aPassword, aSalt, 5000))
            {
                aKey = pwdGen.GetBytes(16);
                aIV  = pwdGen.GetBytes(8);
            }
        }

        #endregion
    }
}
