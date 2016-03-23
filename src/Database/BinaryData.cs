/****************************************************************************/
/*                                                                          */
/*    The Mondo Libraries  							                        */
/*                                                                          */
/*        Namespace: Mondo.Database							                */
/*             File: BinaryData.cs								            */
/*        Class(es): BinaryData									            */
/*          Purpose: Class to store binary information in a database        */
/*                                                                          */
/*  Original Author: Jim Lightfoot                                          */
/*    Creation Date: 3 Jul 2003                                             */
/*                                                                          */
/*   Copyright (c) 2003-2016 - Jim Lightfoot, All rights reserved           */
/*                                                                          */
/*  Licensed under the MIT license:                                         */
/*    http://www.opensource.org/licenses/mit-license.php                    */
/*                                                                          */
/****************************************************************************/

using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlTypes;
using System.Data.SqlClient;
using System.IO;
using Mondo.Common;

namespace Mondo.Database
{
    /****************************************************************************/
    /****************************************************************************/
    public class BinaryData
	{
        public const int kBufferSize = 65536; 

        /****************************************************************************/
        private BinaryData()
		{
		}

        /****************************************************************************/
        /****************************************************************************/
        public class Empty : Exception
        {
            public Empty() : base("Binary data column(s) are empty") {}
        }
        
        /****************************************************************************/
        public static byte[] Read(Database objDatabase, string strSelect)
        {
            using(DbCommand objCommand = objDatabase.MakeSelectCommand(strSelect))
            {
                return(Read(objDatabase, objCommand));
            }
        }

        /****************************************************************************/
        public static byte[] Read(Database objDatabase, StoredProc sp)
        {
            return(Read(objDatabase, sp.Command));
        }

        /****************************************************************************/
        public static byte[] Read(Database objDatabase, DbCommand cmd)
        {
            using(MemoryStream objStream = new MemoryStream())
            {
                WriteTilesToStream(objDatabase, cmd, objStream);

                if(objStream.Length == 0)
                    throw new Empty();

                return(objStream.ToArray());
            }
        }

        #region Private Methods

        /****************************************************************************/
        private static void WriteTilesToStream(Database objDatabase, DbCommand cmd, Stream objStream)
        {
            try
            {
                using(Acquire o = new Acquire(objDatabase))
                {
                    using(DbDataReader objReader = objDatabase.ExecuteSelect(cmd))
                    {
                        if(objReader != null)
                        {
                            byte[] objBuffer = new byte[kBufferSize];
                            long   iOffset   = 0;

                            while(objReader.Read())
                            {
                                long iRead = objReader.GetBytes(0, iOffset, objBuffer, 0, kBufferSize); 

                                if(iRead == 0)
                                    break;

                                objStream.Write(objBuffer, 0, (int)iRead);

                                iOffset += iRead;
                            }
                        }
                    }
                }
            }
            catch(Exception)
            {
                // Do nothing. Stream will just be empty
            }

            if(objStream.CanSeek)
                objStream.Seek(0, SeekOrigin.Begin);

            return; 
        }
      
        #endregion
    }
}
