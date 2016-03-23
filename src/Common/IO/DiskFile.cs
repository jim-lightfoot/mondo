/****************************************************************************/
/*                                                                          */
/*    The Mondo Libraries  							                        */
/*                                                                          */
/*        Namespace: Mondo.Common							                */
/*             File: DiskFile.cs										    */
/*        Class(es): DiskFile										        */
/*          Purpose: Functions to read and write files                      */
/*                                                                          */
/*  Original Author: Jim Lightfoot                                          */
/*    Creation Date: 3 Aug 2008                                             */
/*                                                                          */
/*   Copyright (c) 2008 - Jim Lightfoot, All rights reserved                */
/*                                                                          */
/*  Licensed under the MIT license:                                         */
/*    http://www.opensource.org/licenses/mit-license.php                    */
/*                                                                          */
/****************************************************************************/

using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Collections.Specialized;

namespace Mondo.Common
{
    /****************************************************************************/
    /****************************************************************************/
    public sealed class DiskFile
    {      
        private const int kBufferSize = 512 * 1024; // 512k

        /****************************************************************************/
        public static bool EnsurePathExists(string strFileName)
        {
            FileInfo objDstFile = new FileInfo(strFileName);

            if(!objDstFile.Directory.Exists)
            {
                objDstFile.Directory.Create();
                return(true);
            }

            return(false);
        }

        /****************************************************************************/
        public static string DirectorySeparatorChar
        {
            get {return(System.IO.Path.DirectorySeparatorChar.ToString());}
        }

        /****************************************************************************/
        public static void CopyFile(string strFromFileName, string strToFileName)
        {
            CopyFile(strFromFileName, strToFileName, true);
        }
        
        /****************************************************************************/
        public static void CopyFile(string strFromFileName, string strToFileName, bool bOverwrite)
        {
            try
            {
                File.Copy(strFromFileName, strToFileName, bOverwrite);
            }
            catch(DirectoryNotFoundException)
            {
                if(EnsurePathExists(strToFileName))
                    CopyFile(strFromFileName, strToFileName, bOverwrite);
                else
                    throw;
            }
        }
        
        /****************************************************************************/
        public static void ToFile(byte[] pData, string strFileName)
        {
            ToFile(pData, strFileName, true);
        }

        /****************************************************************************/
        public static void ToFile(byte[] pData, string strFileName, bool bOverwrite)
        {
            try
            {
                using(FileStream fs = new FileStream(strFileName, bOverwrite ? FileMode.Create : FileMode.CreateNew, FileAccess.Write, FileShare.None))
                {
                    fs.Write(pData, 0, pData.Length);
                }
            }
            catch(DirectoryNotFoundException)
            {
                if(EnsurePathExists(strFileName))
                    ToFile(pData, strFileName, bOverwrite);
                else
                    throw;
            }
        }
              
        /****************************************************************************/
        public static void ToFile(string strData, string strFileName)
        {
            ToFile(strData, strFileName, true);
        }

        /****************************************************************************/
        public static void ToFile(string strData, string strFileName, bool bOverwrite)
        {
            try
            {
                using(FileStream fs = new FileStream(strFileName, bOverwrite ? FileMode.Create : FileMode.CreateNew, FileAccess.Write, FileShare.None))
                {
                    using(StreamWriter objWriter = new StreamWriter(fs))
                    {
                        try
                        {
                            objWriter.Write(strData);
                        }
                        finally
                        {
                            objWriter.Close();
                        }
                    }
                }
            }
            catch(DirectoryNotFoundException)
            {
                if(EnsurePathExists(strFileName))
                    ToFile(strData, strFileName, bOverwrite);
                else
                    throw;
            }
            catch(Exception)
            {
                throw;
            }
        }

        /****************************************************************************/
        public static void ToFile(Stream objStream, string strFileName)
        {
            ToFile(objStream, strFileName, true);
        }
        
        /****************************************************************************/
        public static void ToFile(Stream objStream, string strFileName, bool bOverwrite)
        {
            try
            {
                using(FileStream fs = new FileStream(strFileName, bOverwrite ? FileMode.Create : FileMode.CreateNew, FileAccess.Write, FileShare.None))
                {
                    fs.Write(objStream);
                }
            }
            catch(DirectoryNotFoundException)
            {
                if(EnsurePathExists(strFileName))
                    ToFile(objStream, strFileName, bOverwrite);
                else
                    throw;
            }
        }
        
        /****************************************************************************/
        public static Stream LoadStream(string strFileName)
        {
            return(File.OpenRead(strFileName));
        }
        
        /****************************************************************************/
        public static MemoryStream LoadFileStream(string strFileName)
        {
            MemoryStream objMemory = new MemoryStream();

            try
            {
                byte[] aBuffer = new byte[kBufferSize];

                using(FileStream fs = File.OpenRead(strFileName))
                {
                    int iRead = 0;

                    do 
                    {
                        iRead = fs.Read(aBuffer, 0, kBufferSize);

                        objMemory.Write(aBuffer, 0, iRead);
                    }
                    while(iRead > 0);
                }

                objMemory.Position = 0;

                return(objMemory);
            }
            catch(Exception ex)
            {
                objMemory.Dispose();
                throw ex;
            }
        }

        /****************************************************************************/
        public static string LoadFile(string strFileName)
        {
            return(LoadFile(strFileName, false));
        }
        
        /****************************************************************************/
        public static string LoadFile(string strFileName, bool bThrow)
        {
            try
            {
                using(StreamReader fs = File.OpenText(strFileName))
                {
                    return(fs.ReadToEnd());
                }
            }
            catch(Exception ex)
            {
                cDebug.Capture(ex);

                if(bThrow)
                    throw;
            }

            return("");
        }
        
        /****************************************************************************/
        public static byte[] LoadBytes(string strFileName)
        {
            using(FileStream fs = File.OpenRead(strFileName))
            {
                byte[] aBuffer = new byte[fs.Length];

                fs.Read(aBuffer, 0, (int)fs.Length);

                return(aBuffer);
            }
        }

        /*************************************************************************/
        public static void AsyncDeleteFile(string strFileName)
        {
            ThreadPool.QueueUserWorkItem(new WaitCallback(_AsyncDeleteFile), strFileName);
        }

        /*************************************************************************/
        public static void _AsyncDeleteFile(object state)
        {
            try
            {
                string strFileName = state.ToString();

                if(File.Exists(strFileName))
                {
                    int nTrys = 0;

                    while(++nTrys <= 20)
                    {
                        try
                        {
                            File.Delete(strFileName);
                            return;
                        }
                        catch
                        {
                        }

                        System.Threading.Thread.Sleep(100);
                    }
                }
            }
            catch
            {
            }
        }

        /****************************************************************************/
        private DiskFile()
        {
        }       
    }
}
