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
    /****************************************************************************
       
        using(AsyncCrypto c = new AsyncCrypto())
        {
            string strEncrypted = "";

            using(AsyncCrypto c2 = new AsyncCrypto(c.PublicKey))
            {
                strEncrypted = c2.Encrypt("Zoomla.com is cool!");
            }

            return(c.Decrypt(strEncrypted));
        }
       
     ****************************************************************************/
    public class AsyncCrypto : IDisposable
    {
        private readonly RSACryptoServiceProvider m_objCrypto;
        private const int KeySize = 4096;

        /****************************************************************************/
        public AsyncCrypto()
        {
            m_objCrypto = new RSACryptoServiceProvider(KeySize);
        }

        /****************************************************************************/
        public AsyncCrypto(string strPublicKey) : this()
        {
            BinaryList aParts = new BinaryList(strPublicKey);

            if(aParts.Count != 2)
                throw new InvalidPublicKey();

            try
            {              
                RSAParameters p = new RSAParameters();

                p.Exponent = aParts[0];
                p.Modulus  = aParts[1];

                m_objCrypto.ImportParameters(p);
            }
            catch(Exception ex)
            {
                throw new InvalidPublicKey(ex);
            }
        }

        /****************************************************************************/
        public string PublicKey
        {
            get
            {
                RSAParameters p     = m_objCrypto.ExportParameters(false);
                BinaryList    aList = new BinaryList();

                aList.Add(p.Exponent);
                aList.Add(p.Modulus);

                return(aList.ToString());
            }
        }

        /****************************************************************************/
        public byte[] Encrypt(byte[] aData)
        {
           return(m_objCrypto.Encrypt(aData, false));
        }

        /****************************************************************************/
        public static byte[] Encrypt(byte[] aData, string strPublicKey)
        {
            using(AsyncCrypto c = new AsyncCrypto(strPublicKey))
            {
                return(c.Encrypt(aData));
            }
        }

        /****************************************************************************/
        public static string EncryptToString(byte[] aData, string strPublicKey)
        {
            aData = AsyncCrypto.Encrypt(aData, strPublicKey);

            string strData = aData.ToBase64String();

            return(strData);
        }

        /****************************************************************************/
        public string Encrypt(string strData)
        {
            UnicodeEncoding e = new UnicodeEncoding();

            return(Encrypt(e.GetBytes(strData)).ToBase64String());
        }

        /****************************************************************************/
        public byte[] Decrypt(byte[] aEncrypted)
        {
            return(m_objCrypto.Decrypt(aEncrypted, false));
        }

        /****************************************************************************/
        public string Decrypt(string strEncrypted)
        {
            UnicodeEncoding e = new UnicodeEncoding();

            return(e.GetString(Decrypt(strEncrypted.FromBase64String())));
        }

        /****************************************************************************/
        /****************************************************************************/
        public class InvalidPublicKey : Exception
        {
            /****************************************************************************/
            public InvalidPublicKey() : base("Invalid Public Key") 
            {
            }

            /****************************************************************************/
            public InvalidPublicKey(Exception ex) : base("Invalid Public Key", ex) 
            {
            }
        }

        /****************************************************************************/
        public void Dispose()
        {
            m_objCrypto.Dispose();
        }
    }

}
