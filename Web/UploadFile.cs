/****************************************************************************/
/*                                                                          */
/*    The Mondo Libraries  	                                                */
/*                                                                          */
/*      Namespace: Mondo.Web	                                            */
/*           File: UploadFile.cs                                            */
/*      Class(es): UploadFile                                               */
/*          Purpose: Loads the data from an uploaded file                   */
/*                                                                          */
/*  Original Author: Jim Lightfoot                                          */
/*    Creation Date: 29 Nov 2015                                            */
/*                                                                          */
/*   Copyright (c) 2015-2016 - Jim Lightfoot, All rights reserved           */
/*                                                                          */
/*  Licensed under the MIT license:                                         */
/*    http://www.opensource.org/licenses/mit-license.php                    */
/*                                                                          */
/****************************************************************************/

using System;
using System.IO;
using System.Text;
using System.Collections;

namespace Mondo.Web
{
    /// <summary>
    /// 
    /// </summary>
    public class UploadFile
    {
        private int       m_iTotalSize     = 0;
        private byte[]    m_pFileData      = null;
        private Guid      m_uUserId        = Guid.Empty;
        private Guid      m_uContainer     = Guid.Empty;
        private string    m_strFileName    = "";
        private string    m_strContentType = "";

        public int      Length          {get{return(m_iTotalSize);}}
        public byte[]   Data            {get{return(m_pFileData);}}
        public string   FileName        {get{return(m_strFileName);}}
        public string   ContentType     {get{return(m_strContentType);}}

        private const int kStart       = 1;
        private const int kFileName    = 4;
        private const int kContentType = 5;

        /****************************************************************************/
        public string GetText()
        {
            UTF8Encoding encoder = new UTF8Encoding();

            return(encoder.GetString(m_pFileData));
        }

        /****************************************************************************/
        private static string GetLine(byte[] pData, UTF8Encoding encoder, ref int iOffset)
        {
            int iTotalLength = pData.Length;
            int iCurrent     = iOffset;

            while(iCurrent < iTotalLength)
            {
                char chCurrent = (char)(pData[iCurrent]);

                if(chCurrent == '\r')
                {
                    if(iCurrent+1 < iTotalLength)
                    {
                        char chNext = (char)(pData[iCurrent+1]);

                        if(chNext == '\n')
                            ++iCurrent;
                    }

                    break;
                }

                ++iCurrent;
            }

            if(iCurrent == iOffset)
                return("<eol>");

            string strLine = encoder.GetString(pData, iOffset, iCurrent - iOffset + 1);

            iOffset = iCurrent + 1;

            return(strLine);
        }

        /****************************************************************************/
        public UploadFile(byte[] pData)
        {
            int          iRawSize   = pData.Length;
            UTF8Encoding encoder    = new UTF8Encoding();
            int          iType      = 0;

            int iOffset = 0;

            while(true)
            {
                string strLine = GetLine(pData, encoder, ref iOffset).Trim();

                if(strLine == "<eol>")
                    break;

                if(strLine == "")
                {
                    if(iType == kContentType)
                        break;

                    continue;
                }

                int iFind = strLine.IndexOf("----");

                // Is this is a content boundary?
                if(iFind != -1)
                {
                    iType = kStart;
                    continue;
                }
                
                iFind = strLine.IndexOf("Content-Disposition");

                // Is this is a content disposition line?
                if(iFind != -1)
                {
                    iFind = strLine.IndexOf("FILENAME");

                    if(iFind != -1)
                    {
                        iType = kFileName;

                        string strFind = "filename=\"";
                        iFind = strLine.IndexOf(strFind);

                        if(iFind != -1)
                        {
                            m_strFileName = strLine.Substring(iFind + strFind.Length).Trim();
                            m_strFileName = m_strFileName.Remove(m_strFileName.Length-1, 1); // Remove last char which should be a "

                            FileInfo objInfo = new FileInfo(m_strFileName);
                            m_strFileName = objInfo.Name;
                        }

                        continue;
                    }

                    iType = 0;
                    continue;
                }

                if(iType == kFileName)
                {
                    string strFind = "Content-Type:";

                    iFind = strLine.IndexOf(strFind);

                    if(iFind != -1)
                    {
                        iType = kContentType;
                        m_strContentType = strLine.Substring(iFind + strFind.Length).Trim();

                        continue;
                    }
                }
            }                  

            m_iTotalSize = iRawSize - iOffset;
            int i = 0;

            if(m_iTotalSize > 0)
            {
                m_pFileData = new byte[m_iTotalSize];

                try
                {
                    for(i = 0; i < m_iTotalSize; ++i)
                        m_pFileData[i] = pData[i + iOffset];
                }
                catch(Exception e)
                {
                    m_pFileData = null;
                    throw new Exception(e.Message + " m_iTotalSize = " + m_iTotalSize.ToString() + " i = " + i.ToString());
                }

                //cUtility.ToFile(m_pFileData, "d:\\SaveImage.jpg");
            }
        }
    }
}
