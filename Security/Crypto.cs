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
/*   Copyright (c) 2005 - Tenth Generation Software, LLC                    */
/*                          All rights reserved                             */
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
        private readonly byte[] m_key;
        private readonly byte[] m_IV;

      #if DEBUG
        private readonly string m_strIV;
        private readonly string m_strKey;
      #endif

		/****************************************************************************/
        public Crypto(byte[] key, byte[] IV)
		{
            m_key = key.DeepClone();
            m_IV  = IV.DeepClone();
      #if DEBUG
            m_strIV = m_IV.ToBase64String();
            m_strKey = m_key.ToBase64String();
      #endif
		}

		/****************************************************************************/
        public Crypto(byte[] key_iv)
		{
            using(BinaryList aList = new BinaryList(key_iv))
            { 
                m_key = aList[0].DeepClone();
                m_IV  = aList[1].DeepClone();
            }
      #if DEBUG
            m_strIV = m_IV.ToBase64String();
            m_strKey = m_key.ToBase64String();
      #endif
		}

        #if DEBUG
		/****************************************************************************/
        public byte[] Key
		{
            get
            {
                return(m_key);
            }
		}
        #endif

		/****************************************************************************/
        public Crypto(SecureString key_iv)
		{
            // ?????
            throw new Exception("Not implemented yet!");
		}

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
            m_key.Clear();
            m_IV.Clear();
        }
        
        /****************************************************************************/
        public byte[] ToArray()
        {
            using(BinaryList aList = new BinaryList())
            { 
                aList.Add(m_key);
                aList.Add(m_IV);

                return(aList.ToArray());
            }
        }

        #region GenerateKey

        /****************************************************************************/
        public static void GenerateNewKey(SymmetricAlgorithm objAlgorithm)
        {
            objAlgorithm.GenerateKey();
            objAlgorithm.GenerateIV();

            string strWrite = "byte[] key  = {";

            foreach(byte b in objAlgorithm.Key)
                strWrite += b.ToString() + ", ";

            strWrite = strWrite.Substring(0, strWrite.Length - 2) + "};\r\n";

            strWrite += "byte[] IV  = {";

            foreach(byte c in objAlgorithm.IV)
                strWrite += c.ToString() + ", ";

            strWrite = strWrite.Substring(0, strWrite.Length - 2) + "};\r\n";

            strWrite += "\r\n\r\n";
            strWrite += Convert.ToBase64String(objAlgorithm.Key) + "\r\n";
            strWrite += Convert.ToBase64String(objAlgorithm.IV) + "\r\n";

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
        public static string[] GenerateKey(SymmetricAlgorithm objAlgorithm)
        {
            objAlgorithm.GenerateKey();
            objAlgorithm.GenerateIV();

            string[] aParts = new string[2];

            aParts[0] = Convert.ToBase64String(objAlgorithm.Key);
            aParts[1] = Convert.ToBase64String(objAlgorithm.IV);

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

        private static byte[] s_aEntropy = {54, 173, 182, 64, 151, 114, 200, 6, 146, 127, 149, 103, 186, 39, 13, 178};

        /****************************************************************************/
        public static byte[] Protect(byte[] aData, bool bSalt = true)
        {
            try
            { 
                if(bSalt)
                    aData = aData.ToSalted();

 	            return(System.Security.Cryptography.ProtectedData.Protect(aData, s_aEntropy, System.Security.Cryptography.DataProtectionScope.CurrentUser));
            }
            catch(Exception ex)
            { 
                throw ex;
            }
        }

        /****************************************************************************/
        public static byte[] Unprotect(byte[] aData, bool bSalted = true)
        {
            try
            { 
 	            aData = System.Security.Cryptography.ProtectedData.Unprotect(aData, s_aEntropy, System.Security.Cryptography.DataProtectionScope.CurrentUser);

                if(bSalted)
                    aData = aData.FromSalted();

                return(aData);
            }
            catch(Exception ex)
            { 
                throw ex;
            }
        }

        /****************************************************************************/
        public static byte[] Encrypt(byte[] aData, byte[] aPassword, byte[] aSalt)
        {
            byte[] aKey = null;
            byte[] aIV  = null;

            GenerateRC2Key(aPassword, aSalt, out aKey, out aIV);

            using(RC2CryptoServiceProvider objAlgorithm = new RC2CryptoServiceProvider())
            {
                objAlgorithm.Key = aKey;
                objAlgorithm.IV  = aIV;

                // setup an RC2 object to encrypt with the derived key                              
                return(EncryptBytes(objAlgorithm, aData));
            }
        }

        /****************************************************************************/
        public static string Encrypt(string strData, string strPassword, byte[] aSalt)
        {
            byte[] aKey = null;
            byte[] aIV  = null;

            GenerateRC2Key(strPassword, aSalt, out aKey, out aIV);

            using(RC2CryptoServiceProvider objAlgorithm = new RC2CryptoServiceProvider())
            {
                objAlgorithm.Key = aKey;
                objAlgorithm.IV  = aIV;

                // setup an RC2 object to encrypt with the derived key                              
                return(InternalEncrypt(objAlgorithm, strData));
            }
        }

        /****************************************************************************/
        private SymmetricAlgorithm CreateAlgorithm()
        {
            return(new RijndaelManaged {Key = m_key, IV = m_IV});
        }

        /****************************************************************************/
        public string Encrypt(string strData)
        {
            using(SymmetricAlgorithm objAlgorithm = CreateAlgorithm())
            {
                return(InternalEncrypt(objAlgorithm, strData));
            }
        }

        /****************************************************************************/
        public byte[] Encrypt(byte[] aData)
        {
            using(SymmetricAlgorithm objAlgorithm = CreateAlgorithm())
            {
                return(EncryptBytes(objAlgorithm, aData));
            }
        }

        /****************************************************************************/
        public static string Decrypt(string strData, string strPassword, byte[] aSalt)
        {
            byte[] aKey = null;
            byte[] aIV  = null;

            GenerateRC2Key(strPassword, aSalt, out aKey, out aIV);

            using(RC2CryptoServiceProvider objAlgorithm = new RC2CryptoServiceProvider {Key = aKey, IV = aIV})
            {
                return(InternalDecrypt(objAlgorithm, strData));
            }
        }

        /****************************************************************************/
        public static byte[] Decrypt(byte[] aData, byte[] aPassword, byte[] aSalt)
        {
            byte[] aKey = null;
            byte[] aIV  = null;

            GenerateRC2Key(aPassword, aSalt, out aKey, out aIV);

            using(RC2CryptoServiceProvider objAlgorithm = new RC2CryptoServiceProvider {Key = aKey, IV = aIV})
            {
                return(DecryptBytes(objAlgorithm, aData));
            }
        }

        /****************************************************************************/
        public byte[] Decrypt(byte[] aEncrypted, int offset = 0, int length = -1)
        {
            using(SymmetricAlgorithm objAlgorithm = CreateAlgorithm())
            {
                return(DecryptBytes(objAlgorithm, aEncrypted, offset, length));
            }
        }

        /****************************************************************************/
        public string Decrypt(string strEncrypted)
        {
            using(SymmetricAlgorithm objAlgorithm = CreateAlgorithm())
            {
                return(InternalDecrypt(objAlgorithm, strEncrypted));
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
        public static string Hash(string strPassword, byte[] aSalt, int nIterations = kPasswordHashIterations)
        {
            if(nIterations == 0)
                nIterations = kPasswordHashIterations;

            using(Rfc2898DeriveBytes objHasher = new Rfc2898DeriveBytes(strPassword, aSalt, nIterations))
            {
                byte[] aHashBytes = objHasher.GetBytes(128);

                return(Convert.ToBase64String(aHashBytes));
            }
        }       

        /****************************************************************************/
        public static char[] Hash(byte[] aPassword, byte[] aSalt)
        {
            using(Rfc2898DeriveBytes objHasher = new Rfc2898DeriveBytes(aPassword, aSalt, kPasswordHashIterations))
            {
                byte[] aHashBytes = objHasher.GetBytes(128);
                char[] outArray   = new char[aHashBytes.Length];

                Convert.ToBase64CharArray(aHashBytes, 0, aHashBytes.Length, outArray, 0);

                aHashBytes.Clear();

                return(outArray);
            }
        }       

        /****************************************************************************/
        public static string GenerateNewPassword(int iSize)
        {
            return(GenerateSaltString(iSize).Replace("=", ""));
        }

        /****************************************************************************/
        public static string GenerateSaltString(int iSize)
        {
            return(Convert.ToBase64String(GenerateSalt(iSize)));
        }

        /****************************************************************************/
        public static byte[] GenerateSalt(int iSize)
        {
            using(RNGCryptoServiceProvider objCrypto = new RNGCryptoServiceProvider())
            {
                byte[] aBuffer = new byte[iSize];

                objCrypto.GetBytes(aBuffer);

                return(aBuffer);
            }
        }

        #endregion

        /****************************************************************************/
        public static byte[] XOR(byte[] a1, byte[] a2)
        {
            int iLength = a1.Length;
            byte[] aResults = new byte[a1.Length];

            for(int i = 0; i < iLength; ++i)
                aResults[i] = (byte)(a1[i] ^ a2[i]);

            return(aResults);
        }

        /****************************************************************************/
        public static byte[] XOR(byte[] a1, byte[] a2, byte[] a3, byte[] a4)
        {
            byte[] p1 = XOR(a1, a2);
            byte[] p2 = XOR(a3, a4);
            byte[] p3 = XOR(p1, p2);

            p1.Clear();
            p2.Clear();

            return(p3);
        }

        #region Private Methods

        /****************************************************************************/
        private static byte[] EncryptBytes(SymmetricAlgorithm objAlgorithm, byte[] aData)
        {
            // Get an encryptor.
            using(ICryptoTransform encryptor = objAlgorithm.CreateEncryptor())
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
        private static string InternalEncrypt(SymmetricAlgorithm objAlgorithm, string strData)
        {
            if(string.IsNullOrEmpty(strData))
                return("");

            // Get encrypted array of bytes.
            byte[] toEncrypt = Encoding.UTF8.GetBytes(strData); // ??? Encoding.ASCII.GetBytes(strData);
            byte[] encrypted = EncryptBytes(objAlgorithm, toEncrypt);

            string strReturn = Convert.ToBase64String(encrypted);

            toEncrypt.Clear();

            return(strReturn);
        }        
               
        /****************************************************************************/
        private static byte[] EncryptBytes(SymmetricAlgorithm objAlgorithm, SecureString strSecureData)
        {
            byte[] aData = null;
            IntPtr pData    = Marshal.SecureStringToBSTR(strSecureData);

            try
            {
                try
                {
                    string strData = Marshal.PtrToStringBSTR(pData); // ???

                    // Convert the data to a byte array.
                    aData = Encoding.UTF8.GetBytes(strData); // ??? Encoding.ASCII.GetBytes(strData);
                }
                finally
                {
                    Marshal.ZeroFreeBSTR(pData);
                }

               return(EncryptBytes(objAlgorithm, aData));
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
        private static string InternalDecrypt(SymmetricAlgorithm objAlgorithm, string strEncrypted)
        {
            if(string.IsNullOrEmpty(strEncrypted))
                return("");

            byte[] aEncrypted = Convert.FromBase64String(strEncrypted);
            byte[] aDecrypted = DecryptBytes(objAlgorithm, aEncrypted);
            string strResult  = System.Text.Encoding.UTF8.GetString(aDecrypted); // ???

            aDecrypted.Clear();

            while(strResult[strResult.Length-1] == '\0')
                strResult = strResult.Substring(0, strResult.Length-1);

            return(strResult);
        }

        /****************************************************************************/
        private static byte[] DecryptBytes(SymmetricAlgorithm objAlgorithm, string strEncrypted)
        {
            byte[] aEncrypted = Convert.FromBase64String(strEncrypted);

            return(DecryptBytes(objAlgorithm, aEncrypted));
        }

        /****************************************************************************/
        private static byte[] DecryptBytes(SymmetricAlgorithm objAlgorithm, byte[] aEncrypted, int offset = 0, int length = -1)
        {
            using(ICryptoTransform decryptor = objAlgorithm.CreateDecryptor())
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
        private static void GenerateRC2Key(string strPassword, byte[] aSalt, out byte[] aKey, out byte[] aIV)
        {
            using(Rfc2898DeriveBytes pwdGen = new Rfc2898DeriveBytes(strPassword, aSalt, 5000))
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
