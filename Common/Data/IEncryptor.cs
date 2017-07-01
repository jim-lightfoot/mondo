/****************************************************************************/
/*                                                                          */
/*    The Mondo Libraries  							                        */
/*                                                                          */
/*        Namespace: Mondo.Common							                */
/*             File: IEncryptor.cs					    		            */
/*        Class(es): IEncryptor				         		                */
/*          Purpose: Interface for encryption and decryption                */
/*                                                                          */
/*  Original Author: Jim Lightfoot                                          */
/*    Creation Date: 5 Jul 2015                                             */
/*                                                                          */
/*   Copyright (c) 2005-2017 - Jim Lightfoot, All rights reserved           */
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

namespace Mondo.Common
{
    /****************************************************************************/
    /****************************************************************************/
	public interface IEncryptor 
	{
        byte[] Encrypt(byte[] aData);
        byte[] Decrypt(byte[] aEncrypted, int offset = 0, int length = -1);
    }
}
