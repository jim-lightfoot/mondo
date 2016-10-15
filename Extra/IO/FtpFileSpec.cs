/****************************************************************************/
/*                                                                          */
/*    The Mondo Libraries  							                        */
/*                                                                          */
/*        Namespace: Mondo.Common							                */
/*             File: FtpFileSpec.cs								            */
/*        Class(es): FtpFileSpec				                            */
/*          Purpose: An ftp file specification                              */
/*                                                                          */
/*  Original Author: Jim Lightfoot                                          */
/*    Creation Date: 12 Nov 2005                                            */
/*                                                                          */
/*   Copyright (c) 2005 - Tenth Generation Software, LLC                    */
/*                          All rights reserved                             */
/*                                                                          */
/****************************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Net;
using System.IO;
using System.Threading;
using Mondo.Xml;

namespace Mondo.Common
{
    /****************************************************************************/
    /****************************************************************************/
    public class FtpFileSpec : FileSpec
    {       
        private string    m_strUserName = "";
        private string    m_strPassword = "";
        private WebClient m_objClient = null;

        /****************************************************************************/
        public FtpFileSpec(string strPath) : base(strPath.Replace("\\", "/"), "/")
        {
        }

        /****************************************************************************/
        public string UserName     {get{return(m_strUserName);} set{m_strUserName = value;}}
        public string Password     {get{return(m_strPassword);} set{m_strPassword = value;}}

        /****************************************************************************/
        public override string DirectoryName
        {
            get
            {
                return(NormalizePath(base.DirectoryName));
            }
        }

        /****************************************************************************/
        public override long FileSize
        {
            // ??? Implement some time in the future
            get { return(0); }
        }

        /****************************************************************************/
        public override bool IsRootFolder
        {
            get
            {
                return(false);
            }
        }

        /****************************************************************************/
        public override bool FileExists(string strFileName)
        {
            try
            {
                DateTime dtCreated = GetFileDate(strFileName);

                return(dtCreated != DateTime.MinValue);
            }
            catch
            {
                return(false);
            }
        }

        /****************************************************************************/
        /* Copies a disk file to this ftp location                                  */
        /****************************************************************************/
        public override void DropFile(FileSpec objSpec, string strFileName, bool bOverwrite)
        {
            WebClient objClient      = this.Client;
            string    strDestination = this.DirectoryName;
            
            if(this.SingleFile)
                strDestination += this.FileName;
            else
                strDestination += Path.GetFileName(strFileName);

            if(!bOverwrite && FileExists(strDestination))
                throw new IOException("File already exists!");

            if(objSpec == null || objSpec is DiskFileSpec)
                objClient.UploadFile(strDestination, strFileName);
            else
            {
            }
        }

        /****************************************************************************/
        /* Copies a disk file to this ftp location                                  */
        /****************************************************************************/
        public override void DropData(string strDestFileName, object objData, bool bOverwrite)
        {
            WebClient objClient      = this.Client;
            string    strDestination = this.DirectoryName;
            
            strDestination += strDestFileName;

            if(!bOverwrite && FileExists(strDestination))
                throw new IOException("File already exists!");

            if(objData is byte[])
                objClient.UploadData(strDestination, objData as byte[]);
            else if(objData is Stream)
            {
                using(MemoryStream objStream = Utility.StreamToStream(objData as Stream))
                {
                    objClient.UploadData(strDestination, objStream.ToArray());
                }
            }
            else
                objClient.UploadString(strDestination, objData.ToString());
        }

        /****************************************************************************/
        public override void Delete(string strFileName)
        {
            string         strDestination = this.DirectoryName;
            FtpWebRequest  objRequest     = CreateRequest(strDestination + strFileName, WebRequestMethods.Ftp.DeleteFile, false);
            FtpWebResponse objResponse    = objRequest.GetResponse() as FtpWebResponse;
            
            objResponse.Close();
        }

        /****************************************************************************/
        public override void DeleteFolder()
        {
            // ?????
        }

        /****************************************************************************/
        public override void DeleteAll()
        {
            // ??? probably a shortcut way of doing this!!
            ICollection aFiles = this.GetFiles(false);

            foreach(string strFileName in aFiles)
                Delete(strFileName);

            return;
        }

        /****************************************************************************/
        private WebClient Client
        {
            get
            {
                if(m_objClient == null)
                {
                    m_objClient = new WebClient();

                    if(m_strUserName == "")
                        m_objClient.UseDefaultCredentials = true;
                    else
                        m_objClient.Credentials = new NetworkCredential(m_strUserName, m_strPassword);
                }

                return(m_objClient);
            }
        }

        /****************************************************************************/
        public override void CopyTo(FileSpec objDestination, bool bOverwrite)
        {
            if(objDestination is DiskFileSpec)
                CopyToFile(objDestination as DiskFileSpec, bOverwrite);
        }

        /****************************************************************************/
        public override byte[] GetFile(string strFileName)
        {
            string strDestination = this.DirectoryName + strFileName;
            
            return(DownloadBytes(strDestination, WebRequestMethods.Ftp.DownloadFile));
        }

        /****************************************************************************/
        public override string LoadFile(string strFileName)
        {
          return(GetFile(strFileName).FromBytes());
        }

        /****************************************************************************/
        public override ICollection GetFiles(bool bSubFolders)
        {
            if(this.SingleFile)
            {
                List<string> aList = new List<string>();
                
                aList.Add(this.DirectoryName + this.FileName);
                
                return(aList);
            }

            string strDestination      = this.DirectoryName;
            string strDirectoryListing = Download(strDestination, WebRequestMethods.Ftp.ListDirectory);

            return(new StringList(strDirectoryListing, "\r\n", true));
        }

        /****************************************************************************/
        public override ICollection GetFolders(bool bSubFolders)
        {
            // ?????
            return(null);
        }

        /****************************************************************************/
        /* Copies ftp files to a disk location                                      */
        /****************************************************************************/
        private void CopyToFile(DiskFileSpec objDestination, bool bOverwrite)
        {
            string strDestination = objDestination.DirectoryName;

            if(this.SingleFile && objDestination.FileName != "*.*")
                strDestination += objDestination.FileName;
            else
                strDestination += this.FileName;
            
            using(Stream objStream = GetStream(this.FullPath))
            {
                DiskFile.ToFile(objStream, strDestination, bOverwrite);
            }
        }

        /****************************************************************************/
        private FtpWebRequest CreateRequest(string strPath, string strMethod, bool bUseBinary)
        {
            FtpWebRequest objRequest = WebRequest.Create(new Uri(strPath)) as FtpWebRequest;

            if(m_strUserName == "")
                objRequest.UseDefaultCredentials = true;
            else
                objRequest.Credentials = new NetworkCredential(m_strUserName, m_strPassword);

            objRequest.KeepAlive = false;
            objRequest.Method    = strMethod;

            if(bUseBinary)
                objRequest.UseBinary = true;

            return(objRequest);
        }

        /****************************************************************************/
        private string Download(string strPath, string strMethod)
        {
            FtpWebRequest  objRequest  = CreateRequest(strPath, strMethod, true);
            FtpWebResponse objResponse = objRequest.GetResponse() as FtpWebResponse;
            StreamReader   objReader   = new StreamReader(objResponse.GetResponseStream());
            
            try
            {
                return(objReader.ReadToEnd());
            }
            finally
            {
                objReader.Close();
            }
        }

        /****************************************************************************/
        public override FileSpec CreateSingleFileSpec(string strFileName)
        {
            FtpFileSpec objNew = new FtpFileSpec(this.DirectoryName + Path.GetFileName(strFileName));

            objNew.m_strUserName = this.m_strUserName;
            objNew.m_strPassword = this.m_strPassword;

            return(objNew);
        }

        /****************************************************************************/
        public override Stream GetStream(string strFileName)
        {
            return(GetFileStream(strFileName, WebRequestMethods.Ftp.DownloadFile));
        }

        /****************************************************************************/
        private Stream GetFileStream(string strFileName, string strMethod)
        {
            FtpWebRequest  objRequest  = CreateRequest(strFileName, strMethod, true);
            FtpWebResponse objResponse = objRequest.GetResponse() as FtpWebResponse;
            Stream         ftpStream   = objResponse.GetResponseStream();

            try
            {
                MemoryStream objStream = Utility.StreamToStream(ftpStream);

                return(objStream);
            }
            finally
            {
                ftpStream.Close();
            }
        }

        
        /****************************************************************************/
        private byte[] DownloadBytes(string strPath, string strMethod)
        {
            using(MemoryStream objStream = GetFileStream(strPath, strMethod) as MemoryStream)
            {
                return(objStream.ToArray());
            }
        }
        
        /****************************************************************************/
        private string NormalizePath(string strDestination)
        {
            strDestination = strDestination.Replace("\\", "/");
            
            if(!strDestination.StartsWith("ftp://"))
            {
                if(strDestination.StartsWith("ftp:/"))
                    strDestination = strDestination.Replace("ftp:/", "ftp://");
                else
                    strDestination = "ftp://" + strDestination;
            }
            
            strDestination = strDestination.EnsureLastChar("/");

            return(strDestination);
        }

        /****************************************************************************/
        public override DateTime FileDate
        {
            get
            {
                string strTimeStamp = Download(this.FullPath, WebRequestMethods.Ftp.GetDateTimestamp);

                return(DateTime.Parse(strTimeStamp));
            }
        }

        /****************************************************************************/
        public override DateTime GetFileDate(string strFileName)
        {
            string strDestination = this.DirectoryName;
            string strTimeStamp   = Download(strDestination + strFileName, WebRequestMethods.Ftp.GetDateTimestamp);

            return(DateTime.Parse(strTimeStamp));
        }

        /****************************************************************************/
        public override DateTime GetModifiedDate(string strFileName)
        {
            return(GetFileDate(strFileName));
        }
    }
}
