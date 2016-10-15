/****************************************************************************/
/*                                                                          */
/*    The Mondo Libraries  							                        */
/*                                                                          */
/*        Namespace: Mondo.Security							                */
/*             File: CryptoExtensions.cs					    		    */
/*        Class(es): CryptoExtensions				         		        */
/*          Purpose: Provides encryption related extensions                 */
/*                                                                          */
/*  Original Author: Jim Lightfoot                                          */
/*    Creation Date: 11 May 2014                                            */
/*                                                                          */
/*   Copyright (c) 2014 - Tenth Generation Software, LLC                    */
/*                          All rights reserved                             */
/*                                                                          */
/****************************************************************************/

using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Security;
using System.Security.Cryptography;
using System.Runtime.InteropServices;
using Mondo.Common;

namespace Mondo.Security
{
    /****************************************************************************/
    /****************************************************************************/
	public static class CryptoExtensions
	{
        private const int kSaltSize = 32;

        /****************************************************************************/
        public static string Protect(this string strData) 
        {        
            byte[] aData = Encoding.UTF8.GetBytes(strData);

            return(Crypto.Protect(aData, false).ToBase64String());
        }

        /****************************************************************************/
        public static string Unprotect(this string strEncrypted) 
        {        
            byte[] aEncrypted   = strEncrypted.FromBase64String();
            byte[] aDecrypted   = Crypto.Unprotect(aEncrypted, false);
            string strDecrypted = Encoding.UTF8.GetString(aDecrypted);

            return(strDecrypted);
        }

        /****************************************************************************/
        public static byte[] ToSalted(this byte[] aData)
        {
            byte[] aSalt = null;

            return(aData.ToSalted(out aSalt));
        }

        /****************************************************************************/
        public static byte[] ToSalted(this byte[] aData, out byte[] aSalt)
        {
            aSalt = Crypto.GenerateSalt(kSaltSize);

            byte[] aSalted = new byte[kSaltSize + aData.Length];

            aSalt.CopyTo(aSalted, 0);
            aData.CopyTo(aSalted, kSaltSize);

            return(aSalted);
        }

        /****************************************************************************/
        public static byte[] FromSalted(this byte[] aData)
        {
            byte[] aSalt = null;

            return(aData.FromSalted(out aSalt));
        }

        /****************************************************************************/
        public static byte[] FromSalted(this byte[] aData, out byte[] aSalt)
        {
            int    iLength   = aData.Length - kSaltSize;
            byte[] aUnSalted = aData.DeepClone(kSaltSize, iLength);

            aSalt = aData.DeepClone(0, kSaltSize);

            return(aUnSalted);
        }

        #region Encrypt/Decrypt Methods

        #region Encrypt

        /****************************************************************************/
        public static string Encrypt(this IEncryptor encryptor, string strData)
        {
            if(strData.Normalized() == "")
                return("");

            return(encryptor.Encrypt(Encoding.UTF8.GetBytes(strData)).ToBase64String());
        }

        /****************************************************************************/
        public static string EncryptToString(this IEncryptor encryptor, byte[] aData)
        {
            if(aData == null || aData.Length == 0)
                return("");

            return(Convert.ToBase64String(encryptor.Encrypt(aData)));
        }

        /****************************************************************************/
        public static string EncodeMoney(this IEncryptor encryptor, long iAuthenticator, string strValue)
        {
            if(strValue.Normalized() == "")
                return("");

            decimal dValue = Utility.ToCurrency(strValue);

            return(encryptor.Encrypt(iAuthenticator, dValue.ToString()));
        }

        private const string kSeparator = "_&8#kV_";

        /****************************************************************************/
        public static string Encrypt(this IEncryptor encryptor, long iAuthenticator, string strValue)
        {
            if(strValue.Normalized() == "")
                return("");

            return(encryptor.Encrypt(iAuthenticator.ToString() + kSeparator + strValue));
        }

        /****************************************************************************/
        public static string EncryptDate(this IEncryptor encryptor, long iAuthenticator, DateTime dtValue)
        {
            Random   r         = new Random();
            int      iRandom   = r.Next(24 * 60 * 60 * 1000) - 1;
            DateTime dtEncrypt = dtValue.Date.AddMilliseconds(iRandom);
            long     iValue    = dtEncrypt.ToBinary();

            return(encryptor.Encrypt(iAuthenticator, iValue.ToString()));
        }

        #endregion

        #region Decrypt

        /****************************************************************************/
        public static SecureString DecryptToSecure(this IEncryptor encryptor, string strEncrypted)
        {
            if(strEncrypted.Normalized() == "")
                return(null);

            int    iLength      = 0;
            char[] chDecrypted  = encryptor.DecryptChars(strEncrypted, out iLength);

            try
            { 
                return(ToSecureString(chDecrypted, iLength));
            }
            finally 
            { 
                chDecrypted.Clear();
            }
        }

        /****************************************************************************/
        public static string Decrypt(this IEncryptor encryptor, string strEncrypted)
        {
            if(strEncrypted.Normalized() == "")
                return("");

            try 
            { 
                int    iLength      = 0;
                char[] chDecrypted  = encryptor.DecryptChars(strEncrypted, out iLength);
                string strDecrypted = new String(chDecrypted, 0, iLength);

                chDecrypted.Clear();

                return(strDecrypted.Replace(((char)6).ToString(), "")); // Was getting wierd char problems
            }
            catch
            {
                return(strEncrypted);
            }
        }

        /****************************************************************************/
        public static char[] DecryptChars(this IEncryptor encryptor, string strEncrypted, out int iLength)
        {
            byte[] aDecrypted  = encryptor.Decrypt(strEncrypted.FromBase64String());
            char[] chDecrypted = Encoding.UTF8.GetChars(aDecrypted);

            aDecrypted.Clear();

            iLength = 0;

            while(iLength < chDecrypted.Length && chDecrypted[iLength] != '\0')
                ++iLength;

            return(chDecrypted);
        }

        /****************************************************************************/
        public static byte[] DecryptBytes(this IEncryptor encryptor, string strEncrypted)
        {
            return(encryptor.Decrypt(Convert.FromBase64String(strEncrypted)));
        }

        /****************************************************************************/
        public static string Decrypt(this IEncryptor encryptor, long iAuthenticator, string strValue)
        {
            if(strValue.Normalized() == "")
                return("");

            try
            {
                string     strDecrypted = encryptor.Decrypt(strValue);
                StringList aParts       = new StringList(strDecrypted, kSeparator, false);

                if(aParts.Count == 2)
                {
                    long iCheckAuthenticator = 0L;

                    if(!long.TryParse(aParts[0], out iCheckAuthenticator))
                        return("");

                    if(iCheckAuthenticator != iAuthenticator)
                        return("");

                    return(aParts[1]);
                }
                else
                    return(strDecrypted); 
            }
            catch
            {
            }

            return(strValue);
        }

        /****************************************************************************/
        public static string DecryptDate(this IEncryptor encryptor, long iAuthenticator, string strValue)
        {
            string strDate = encryptor.Decrypt(iAuthenticator, strValue);
            long   iValue  = 0;

            if(!long.TryParse(strDate, out iValue))
                return(strDate);

            DateTime dtValue = DateTime.FromBinary(iValue);

            return(dtValue.ToString("yyyy-MM-dd"));
        }

        #endregion

        #endregion

        #region Private Methods

        /****************************************************************************/
        unsafe private static SecureString ToSecureString(char[] aChars, int iLength)
        {
            SecureString str;

            fixed(char* pChars = aChars)
            {
                str = new SecureString(pChars, iLength);
            }

            str.MakeReadOnly();

            return(str);
        }

        #endregion
    }
}
