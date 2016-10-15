/****************************************************************************/
/*                                                                          */
/*    The Mondo Libraries  							                        */
/*                                                                          */
/*        Namespace: Mondo.Common							                */
/*             File: IsolatedStorage.cs							            */
/*        Class(es): IsolatedStorage				                        */
/*          Purpose: Read and write to isolated storage                     */
/*                                                                          */
/*  Original Author: Jim Lightfoot                                          */
/*    Creation Date: 20 Apr 2014                                            */
/*                                                                          */
/*   Copyright (c) 2014 - Tenth Generation Software, LLC                    */
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
using System.IO.IsolatedStorage;

using Mondo.Xml;

namespace Mondo.Common
{
    /****************************************************************************/
    /****************************************************************************/
    public class IsolatedStorage : FileSpec
    {
        /****************************************************************************/
        public IsolatedStorage(string strPath) : base(strPath.Replace("/", "\\"), "\\")
        {
        }

        /****************************************************************************/
        public bool CopySubFolders 
        {
            get;
            set;
        }

        /****************************************************************************/
        public override DateTime GetFileDate(string strFileName)
        {
            using(IStore isoStore = CreateStore())
            {
                return(isoStore.Storage.GetCreationTime(strFileName).LocalDateTime);
            }
        }

        /****************************************************************************/
        public override DateTime GetModifiedDate(string strFileName)
        {
            return(GetFileDate(strFileName));
        }

        /****************************************************************************/
        public override DateTime FileDate
        {
            get {return(GetFileDate(this.FullPath));}
        }

        /****************************************************************************/
        public override long FileSize
        {
            get {return(0);}
        }

        /****************************************************************************/
        public override FileSpec CreateSingleFileSpec(string strFileName)
        {
            string strFolder = Path.GetDirectoryName(strFileName);

            if(strFolder != "")
                return(new IsolatedStorage(strFileName));

            return(new IsolatedStorage(this.DirectoryName + Path.GetFileName(strFileName)));
        }

        /****************************************************************************/
        public override Stream GetStream(string strFileName)
        {
            using(IStore isoStore = CreateStore())
            {
                return(isoStore.Storage.OpenFile(strFileName, FileMode.Open, FileAccess.Read, FileShare.None));
            }
        }

        /****************************************************************************/
        public override bool FileExists(string strFileName)
        {
            using(IStore isoStore = CreateStore())
            {
                return(isoStore.Storage.FileExists(strFileName));
            }
        }

        /****************************************************************************/
        public override bool FolderExists()
        {
            using(IStore isoStore = CreateStore())
            {
                return(isoStore.Storage.DirectoryExists(this.DirectoryName));
            }
        }

        /****************************************************************************/
        public override void CopyTo(FileSpec objDestination, bool bOverwrite)
        {
            if(objDestination is IsolatedStorage)
                CopyToFile(objDestination as IsolatedStorage, bOverwrite);
            else
                base.CopyTo(objDestination, bOverwrite);
        }

        /****************************************************************************/
        public override void DropData(string strDestFileName, object objData, bool bOverwrite)
        {
            try
            {
                using(IStore isoStore = CreateStore())
                {
                    using(Stream stream = isoStore.Storage.OpenFile(this.DirectoryName + strDestFileName, bOverwrite ? FileMode.Create : FileMode.CreateNew, FileAccess.Write, FileShare.None))
                    {
                        if(objData is byte[])
                        {
                            byte[] pData = objData as byte[];

                            stream.Write(pData, 0, pData.Length);
                        }
                        else if(objData is Stream)
                            stream.Write(objData as Stream);
                        else
                        {
                            using(Stream objSource = Utility.StringToStream(objData.ToString()))
                            {
                                stream.Write(objSource);
                            }
                        }
                    }
                }
            }
            catch(DirectoryNotFoundException)
            {
                if(EnsurePathExists(this.DirectoryName + strDestFileName))
                    DropData(strDestFileName, objData, bOverwrite);
                else
                    throw;
            }
        }

        /****************************************************************************/
        public bool EnsurePathExists(string strFileName)
        {
            using(IStore isoStore = CreateStore())
            {
                strFileName = System.IO.Path.GetDirectoryName(strFileName);

                if(!isoStore.Storage.DirectoryExists(strFileName))
                {
                    isoStore.Storage.CreateDirectory(strFileName);
                    return(true);
                }
            }

            return(false);
        }

        /****************************************************************************/
        public override void DropFile(FileSpec objSpec, string strFileName, bool bOverwrite)
        {
            // Do nothing because this will never get called
        }

        /****************************************************************************/
        public override byte[] GetFile(string strFileName)
        {
            using(IStore isoStore = CreateStore())
            {
                using(Stream stream = isoStore.Storage.OpenFile(this.DirectoryName + strFileName, FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    byte[] aBuffer = new byte[stream.Length];

                    stream.Read(aBuffer, 0, (int)stream.Length);

                    return(aBuffer);
                }
            }
        }

        /****************************************************************************/
        public override string LoadFile(string strFileName)
        {
            using(IStore isoStore = CreateStore())
            {
                using(Stream stream = isoStore.Storage.OpenFile(this.DirectoryName + strFileName, FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    return(Utility.StreamToString(stream));
                }
            }
        }

        /****************************************************************************/
        public override void Delete(string strFileName)
        {
            using(IStore isoStore = CreateStore())
            {
                isoStore.Storage.DeleteFile(strFileName);
            }
        }

        /****************************************************************************/
        public override void DeleteFolder()
        {
            using(IStore isoStore = CreateStore())
            {
                isoStore.Storage.DeleteDirectory(this.DirectoryName);
            }
        }

        /****************************************************************************/
        public override void DeleteAll()
        {
            IEnumerable aFiles = this.GetFiles(false);

            using(IStore isoStore = CreateStore())
            {
                foreach(string strFileName in aFiles)
                    isoStore.Storage.DeleteFile(strFileName);
            }

            return;
        }

        /****************************************************************************/
        private void CopyToFile(IsolatedStorage objDestination, bool bOverwrite)
        {
            if(this.DirectoryName != objDestination.DirectoryName)
            {
                if(this.SingleFile)
                {
                    string strSource      = this.DirectoryName + this.FileName;
                    string strDestination = objDestination.FileName;

                    if(strDestination == "*.*")
                        strDestination = this.FileName;

                    strDestination = objDestination.DirectoryName + strDestination;

                    if(this.Progress != null)
                        this.Progress.SetProgress(strSource);

                    using(IStore isoStore = CreateStore())
                    {
                        isoStore.Storage.CopyFile(strSource, strDestination, bOverwrite);
                    }
                }
                else
                {
                    IEnumerable aFiles = this.GetFiles(false);

                    using(IStore isoStore = CreateStore())
                    {
                        foreach(string strSourceFilePath in aFiles)
                        {
                            string strFileName = Path.GetFileName(strSourceFilePath);

                            if(this.Progress != null)
                                this.Progress.SetProgress(strSourceFilePath);

                            isoStore.Storage.CopyFile(strSourceFilePath, objDestination.DirectoryName + strFileName, bOverwrite);
                        }
                    }

                    if(this.CopySubFolders)
                    {
                        string[] aSubFolders = Directory.GetDirectories(this.DirectoryName);

                        foreach(string strSubFolder in aSubFolders)
                        {
                            IsolatedStorage objFrom = new IsolatedStorage(strSubFolder);
                            int             iIndex  = strSubFolder.LastIndexOf("\\");
                            IsolatedStorage objTo   = null;

                            objFrom.Progress = this.Progress;

                            if(iIndex != -1)
                                objTo = new IsolatedStorage(objDestination.DirectoryName + strSubFolder.Remove(0, iIndex + 1));

                            objFrom.CopySubFolders = true;
                            objFrom.CopyTo(objTo, bOverwrite);
                        }
                    }
                }
            }
        }

        /****************************************************************************/
        public override ICollection GetFiles(bool bSubFolders)
        {
            if(this.SingleFile)
            {
                List<string> aFiles = new List<string>();
                
                aFiles.Add(this.DirectoryName + this.FileName);

                return(aFiles);
            }

            using(IStore isoStore = CreateStore())
            {
                return(new StringList(isoStore.Storage.GetFileNames()));
           }
        }

        /****************************************************************************/
        public override ICollection GetFolders(bool bSubFolders)
        {
            using(IStore isoStore = CreateStore())
            {
                return(new StringList(isoStore.Storage.GetDirectoryNames()));
            }
        }

        /****************************************************************************/
        protected virtual IStore CreateStore()
        {
            return(new Store());
        }

        /****************************************************************************/
        /****************************************************************************/
        protected interface IStore : IDisposable
        {
            IsolatedStorageFile Storage {get;}
        }

        /****************************************************************************/
        /****************************************************************************/
        private class Store : IStore
        {
            private readonly IsolatedStorageFile m_objStorage;

            /****************************************************************************/
            internal Store()
            {
                m_objStorage = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Domain | IsolatedStorageScope.Assembly, null, null);
            }

            /****************************************************************************/
            public IsolatedStorageFile Storage
            {
                get { return(m_objStorage); }
            }
        
            /****************************************************************************/
            public void Dispose()
            {
                try 
                { 
 	                m_objStorage.Close();
                }
                catch
                {
                }

                m_objStorage.Dispose();
            }
        }
    }
}
