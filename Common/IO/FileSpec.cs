/****************************************************************************/
/*                                                                          */
/*    The Mondo Libraries  							                        */
/*                                                                          */
/*        Namespace: Mondo.Common							                */
/*             File: FileSpec.cs								            */
/*        Class(es): FileSpec				                                */
/*          Purpose: A file specification                                   */
/*                                                                          */
/*  Original Author: Jim Lightfoot                                          */
/*    Creation Date: 12 Nov 2005                                            */
/*                                                                          */
/*   Copyright (c) 2005 - Jim Lightfoot, All rights reserved                */
/*                                                                          */
/*  Licensed under the MIT license:                                         */
/*    http://www.opensource.org/licenses/mit-license.php                    */
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
    public interface IProgress
    {
        void SetProgress(string strItem);
        void AddProgressCount(int nItems);
        void ClearProgressCount();
    }

    /****************************************************************************/
    /****************************************************************************/
    public interface IPath
    {
        string FullPath      {get;}
    }

    /****************************************************************************/
    /****************************************************************************/
    public interface IStorage 
    {
        string LoadFile(string strFileName);
        byte[] GetFile(string strFileName);
        void   DropData(string strDestFileName, object objData, bool bOverwrite);
    }

    /****************************************************************************/
    /****************************************************************************/
    public abstract class FileSpec : IPath, IStorage
    {      
        private string    m_strFolder    = "";
        private string    m_strFileName  = "";
        private bool      m_bSingleFile  = false;
        private string    m_strSeparator = "";
        private IProgress m_objProgress  = null;
        
        /****************************************************************************/
        protected FileSpec(string strPath, string strSeparator)
        {
            m_strSeparator = strSeparator;

            string strRevisedPath = strPath.Trim();
            string strExtension   = Path.GetExtension(strPath);

            if(strExtension == "")
            {
                strRevisedPath = strRevisedPath.EnsureLastChar(m_strSeparator);
                strRevisedPath += "*.*";
            }

            m_strFolder   = Path.GetDirectoryName(strRevisedPath);
            m_strFolder   = m_strFolder.EnsureLastChar(m_strSeparator);
            m_strFileName = Path.GetFileName(strRevisedPath);

            if(strSeparator == "\\")
                m_strFolder = m_strFolder.Replace("//", "\\\\");
            else
                m_strFolder = m_strFolder.Replace("\\\\", "//").Replace("\\", "/");

            m_bSingleFile = !m_strFileName.Contains("*") && !m_strFileName.Contains("?");
       }

        /****************************************************************************/
        public static FileSpec Create(string strPath)
        {
           // if(strPath.ToLower().StartsWith("ftp"))
           //     return(new FtpFileSpec(strPath));

            return(new DiskFileSpec(strPath));
        }

        /****************************************************************************/
        public virtual string   DirectoryName  {get{return(m_strFolder);}}
        public string           FileName       {get{return(m_strFileName);}}
        public string           FullPath       {get{return(m_strFolder + m_strFileName);}}
        public bool             SingleFile     {get{return(m_bSingleFile);}}
        public IProgress        Progress       {get{return(m_objProgress);} set {m_objProgress = value;}}

        /****************************************************************************/
        public virtual bool IsRootFolder
        {
            get
            {
                string strFolder = Path.GetDirectoryName(this.DirectoryName).ToLower();

                return(strFolder == Directory.GetDirectoryRoot(strFolder));
            }
        }

        /****************************************************************************/
        public virtual void CopyTo(FileSpec objDestination, bool bOverwrite)
        {
            IEnumerable aFiles = this.GetFiles(false);

            foreach(string strSourceFilePath in aFiles)
            {
                if(m_objProgress != null)
                    m_objProgress.SetProgress(strSourceFilePath);

               objDestination.DropFile(this, strSourceFilePath, bOverwrite);
            }

            return;
        }

        /****************************************************************************/
        public virtual bool FileExists()
        {
            if(this.SingleFile)
                return(FileExists(this.FullPath));

            return(true);
        }

        /****************************************************************************/
        public virtual bool FolderExists()
        {
            return(true);
        }

        /****************************************************************************/
        public abstract ICollection         GetFiles(bool bSubFolders);
        public abstract ICollection         GetFolders(bool bSubFolders);
        public abstract bool                FileExists(string strFileName);
        public abstract void                DropFile(FileSpec objSpec, string strFileName, bool bOverwrite);
        public abstract void                DropData(string strDestFileName, object objData, bool bOverwrite);
        public abstract void                DeleteAll();
        public abstract void                Delete(string strFileName);
        public abstract void                DeleteFolder();
        public abstract byte[]              GetFile(string strFileName);
        public abstract string              LoadFile(string strFileName);
        public abstract DateTime            GetFileDate(string strFileName);
        public abstract DateTime            GetModifiedDate(string strFileName);
        public abstract DateTime            FileDate    {get;}
        public abstract long                FileSize    {get;}
        public abstract Stream              GetStream(string strFileName);
        public abstract FileSpec            CreateSingleFileSpec(string strFileName);
        
        /****************************************************************************/
        public virtual IComparer<string> ModificationDateComparer
        {
            get {return(new CompareModificationDates(this));}
        }

        /****************************************************************************/
        /****************************************************************************/
        public class CompareModificationDates : IComparer<string>
        {
            private FileSpec m_objSpec;

            /****************************************************************************/
            public CompareModificationDates(FileSpec objSpec)
            {
                m_objSpec = objSpec;
            }
        
            /****************************************************************************/
            public int Compare(string x, string y)
            {
 	            return(m_objSpec.GetModifiedDate(x).CompareTo(m_objSpec.GetModifiedDate(y)));
            }
        }
    }

    /****************************************************************************/
    /****************************************************************************/
    public class DiskFileSpec : FileSpec
    {
        private bool m_bSubFolders = true;
        private Dictionary<string, string> m_aExcludeExtensions = new Dictionary<string, string>(7);

        /****************************************************************************/
        public DiskFileSpec(string strPath) : base(strPath.Replace("/", "\\"), "\\")
        {
        }

        /****************************************************************************/
        public int FileCount
        {
            get
            {
                // ??? temp (doesn't include subfolders)
                return(this.GetFiles(false).Count);
            }
        }
        
        /****************************************************************************/
        public bool CopySubFolders 
        {
            get {return(m_bSubFolders);}
            set {m_bSubFolders = value;}
        }

        /****************************************************************************/
        public override DateTime GetFileDate(string strFileName)
        {
            return(File.GetCreationTime(strFileName));
        }

        /****************************************************************************/
        public override DateTime GetModifiedDate(string strFileName)
        {
            return(File.GetLastWriteTime(strFileName));
        }

        /****************************************************************************/
        public override DateTime FileDate
        {
            get {return(File.GetCreationTime(this.FullPath));}
        }

        /****************************************************************************/
        public override long FileSize
        {
            get 
            {
                try
                {
                    return(new FileInfo(this.FullPath).Length);
                }
                catch
                {
                    return(0);
                }
            }
        }

        /****************************************************************************/
        public override FileSpec CreateSingleFileSpec(string strFileName)
        {
            string strFolder = Path.GetDirectoryName(strFileName);

            if(strFolder != "")
                return(new DiskFileSpec(strFileName));

            return(new DiskFileSpec(this.DirectoryName + Path.GetFileName(strFileName)));
        }

        /****************************************************************************/
        public override Stream GetStream(string strFileName)
        {
            return(File.OpenRead(strFileName));
        }

        /****************************************************************************/
        public override bool FileExists(string strFileName)
        {
            return(File.Exists(strFileName));
        }

        /****************************************************************************/
        public override bool FolderExists()
        {
            return(Directory.Exists(this.DirectoryName));
        }

        /****************************************************************************/
        public override void CopyTo(FileSpec objDestination, bool bOverwrite)
        {
            if(objDestination is DiskFileSpec)
                CopyToFile(objDestination as DiskFileSpec, bOverwrite);
            else
                base.CopyTo(objDestination, bOverwrite);
        }

        /****************************************************************************/
        public void ExcludeExtension(string strExtension)
        {
            m_aExcludeExtensions.Add(strExtension.ToLower().EnsureStartsWith("."), "");
        }

        /****************************************************************************/
        public override void DropData(string strDestFileName, object objData, bool bOverwrite)
        {
            if(objData is byte[])
                DiskFile.ToFile(objData as byte[], this.DirectoryName + strDestFileName, bOverwrite);
            else if(objData is Stream)
                DiskFile.ToFile(objData as Stream, this.DirectoryName + strDestFileName, bOverwrite);
            else
                DiskFile.ToFile(objData.ToString(), this.DirectoryName + strDestFileName, bOverwrite);
        }

        /****************************************************************************/
        public override void DropFile(FileSpec objSpec, string strFileName, bool bOverwrite)
        {
            // Do nothing because this will never get called
        }

        /****************************************************************************/
        public override byte[] GetFile(string strFileName)
        {
            if(!strFileName.Contains(Path.DirectorySeparatorChar.ToString()))
                strFileName = this.DirectoryName + strFileName;

            return(DiskFile.LoadBytes(strFileName));
        }

        /****************************************************************************/
        public override string LoadFile(string strFileName)
        {
            if(!strFileName.Contains(Path.DirectorySeparatorChar.ToString()))
                strFileName = this.DirectoryName + strFileName;

            return(DiskFile.LoadFile(strFileName));
        }

        /****************************************************************************/
        public override void Delete(string strFileName)
        {
            try
            {
                File.Delete(strFileName);

              #if DEBUG
                Console.WriteLine("Deleted " + strFileName);
              #endif
            }
            catch(Exception ex)
            {
                cDebug.Capture(ex);

                throw;
            }
        }

        /****************************************************************************/
        public override void DeleteFolder()
        {
            Directory.Delete(this.DirectoryName);
        }

        /****************************************************************************/
        public override void DeleteAll()
        {
            IEnumerable aFiles = this.GetFiles(false);

            foreach(string strFileName in aFiles)
                Delete(strFileName);

            return;
        }

        /****************************************************************************/
        public void DeleteAllAsync()
        {
            IEnumerable aFiles = this.GetFiles(false);

            foreach(string strFileName in aFiles)
                AsyncDeleteFile(strFileName);

            return;
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
        private bool Excluded(string strFileName)
        {
            if(m_aExcludeExtensions.Count > 0)
            {
                string strExtension = Path.GetExtension(strFileName).ToLower();

                return(m_aExcludeExtensions.ContainsKey(strExtension));
            }

            return(false);
        }

        /****************************************************************************/
        private void CopyToFile(DiskFileSpec objDestination, bool bOverwrite)
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

                    DiskFile.CopyFile(strSource, strDestination, bOverwrite);
                }
                else
                {
                    IEnumerable aFiles = this.GetFiles(false);

                    foreach(string strSourceFilePath in aFiles)
                    {
                        string strFileName = Path.GetFileName(strSourceFilePath);

                        if(!Excluded(strFileName))
                        {
                            if(this.Progress != null)
                                this.Progress.SetProgress(strSourceFilePath);

                            DiskFile.CopyFile(strSourceFilePath, objDestination.DirectoryName + strFileName, bOverwrite);
                        }
                    }

                    if(this.CopySubFolders)
                    {
                        string[] aSubFolders = Directory.GetDirectories(this.DirectoryName);

                        foreach(string strSubFolder in aSubFolders)
                        {
                            DiskFileSpec objFrom = new DiskFileSpec(strSubFolder);
                            int          iIndex  = strSubFolder.LastIndexOf("\\");
                            DiskFileSpec objTo   = null;

                            objFrom.Progress = this.Progress;

                            if(iIndex != -1)
                                objTo = new DiskFileSpec(objDestination.DirectoryName + strSubFolder.Remove(0, iIndex + 1));

                            objFrom.CopySubFolders = true;
                            objFrom.m_aExcludeExtensions = this.m_aExcludeExtensions;
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

            return(Directory.GetFiles(this.DirectoryName, this.FileName, bSubFolders ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly));
        }

        /****************************************************************************/
        public override ICollection GetFolders(bool bSubFolders)
        {
            return(Directory.GetDirectories(this.DirectoryName, "*.*", bSubFolders ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly));
        }
    }
}
