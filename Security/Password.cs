/****************************************************************************/
/*                                                                          */
/*    The Mondo Libraries  							                        */
/*                                                                          */
/*        Namespace: Mondo.Security							                */
/*             File: Password.cs					    		            */
/*        Class(es): Password				         		                */
/*          Purpose: Encodes a password                                     */
/*                                                                          */
/*  Original Author: Jim Lightfoot                                          */
/*    Creation Date: 3 Jun 2013                                             */
/*                                                                          */
/*   Copyright (c) 2013 - Tenth Generation Software, LLC                    */
/*                          All rights reserved                             */
/*                                                                          */
/****************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

using Mondo.Common;

namespace Mondo.Security
{
    /****************************************************************************/
    /****************************************************************************/
    public static class Password
    {
        public class PasswordTooShort : Exception {}

        /****************************************************************************/
        public static string Encode(string strSalt2, string strSalt, string strPassword, int iMinPasswordLength, int nIterations = 0)
        {
            if(strPassword.Length < iMinPasswordLength)
                throw new PasswordTooShort();

            string strSaltPasswordId = strSalt2 + strPassword;
            UTF8Encoding objEncoder = new UTF8Encoding();

            return(Crypto.Hash(strSaltPasswordId, objEncoder.GetBytes(strSalt), nIterations));
        }

        /****************************************************************************/
        public static string GenerateNew(int iLength)
        {
            return(GenerateNew(iLength, 1)[0]);
        }

        /****************************************************************************/
        public static List<string> GenerateNew(int iLength, int nPasswords)
        {
            const string validChars     = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ123456789012345678901234567890!@#$%^&*()_+-={}[]:<>?,./~";
            char[]       characterArray = validChars.ToArray();
            byte[]       bytes          = new byte[iLength * 8];
            List<string> aPasswords     = new List<string>();
            
            using(RNGCryptoServiceProvider crypto = new RNGCryptoServiceProvider())
            { 
                for(int i = 0; i < nPasswords; ++i)
                { 
                    bytes.Clear();

                    crypto.GetBytes(bytes);

                    char[] result = new char[iLength];

                    for(int j = 0; j < iLength; ++j)
                    {
                        ulong value = BitConverter.ToUInt64(bytes, j * 8);
                        result[j] = characterArray[value % (uint)characterArray.Length];
                    }

                    aPasswords.Add(new string(result));
                }
            }

            return(aPasswords);
        }
    }
}
