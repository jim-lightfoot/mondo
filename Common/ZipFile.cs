/****************************************************************************/
/*                                                                          */
/*           Module: TenthGeneration.Common								    */
/*             File: ZipFile.cs										        */
/*        Class(es): ZipFile										        */
/*          Purpose: Creates, opens and modifies zip files                  */
/*                                                                          */
/*  Original Author: Jim Lightfoot                                          */
/*    Creation Date: 17 Mar 2010                                            */
/*                                                                          */
/*   Copyright (c) 2010 - Tenth Generation Software                         */
/*                          All rights reserved                             */
/*                                                                          */
/****************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Ionic.Zip;

namespace TenthGeneration.Common
{
    /****************************************************************************/
    /****************************************************************************/
    public class SaveProgressEventArgs : EventArgs
    {
        private int m_iSoFar; 
        private int m_iTotal;

        /****************************************************************************/
        public SaveProgressEventArgs(int iSoFar, int iTotal)
        {
            m_iSoFar = iSoFar; 
            m_iTotal = iTotal;
        }

        /****************************************************************************/
        public int SoFar    {get{return(m_iSoFar);}}
        public int Total    {get{return(m_iTotal);}}
    }

    /****************************************************************************/
    /****************************************************************************/
    public class ZipFile : IDisposable
    {
        private Ionic.Zip.ZipFile m_objZip;
        private string            m_strPath;
        private bool              m_bSaved = false;

        public event EventHandler<Ionic.Zip.SaveProgressEventArgs> SaveProgress;

        /****************************************************************************/
        public ZipFile(string strPath)
        {
            m_objZip = new Ionic.Zip.ZipFile();
            m_strPath = strPath;
            m_objZip.CompressionLevel = Ionic.Zlib.CompressionLevel.BestCompression;

            m_objZip.SaveProgress += new EventHandler<Ionic.Zip.SaveProgressEventArgs>(this._SaveProgress);
        }

        /****************************************************************************/
        private void _SaveProgress(object sender, Ionic.Zip.SaveProgressEventArgs e)
        {
            if(this.SaveProgress != null)
                SaveProgress(new TenthGeneration.Common.ZipFile.SaveProgressEventArgs(e.EntriesSaved, e.EntriesTotal));
        }
   
        /****************************************************************************/
        public void AddFile(string strPath)
        {
            m_objZip.AddFile(strPath);
        }

        /****************************************************************************/
        public void Save()
        {
            if(!m_bSaved)
            {
                m_objZip.Save(m_strPath);
                Save();
            }
        }

        /****************************************************************************/
        public void Dispose()
        {
            try
            {
                Save();
            }
            catch
            {
            }

            try
            {
                if(m_objZip != null)
                    m_objZip.Dispose();
            }
            catch(Exception ex)
            {
                cDebug.Capture(ex);
            }
        }
    }
}
