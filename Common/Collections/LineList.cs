/****************************************************************************/
/*                                                                          */
/*    The Mondo Libraries  							                        */
/*                                                                          */
/*        Namespace: Mondo.Common							                */
/*             File: LineList.cs										    */
/*        Class(es): LineList										        */
/*          Purpose: A list of lines in a text file                         */
/*                                                                          */
/*  Original Author: Jim Lightfoot                                          */
/*    Creation Date: 2 Mar 2014                                             */
/*                                                                          */
/*   Copyright (c) 2014-2016 - Jim Lightfoot, All rights reserved           */
/*                                                                          */
/*  Licensed under the MIT license:                                         */
/*    http://www.opensource.org/licenses/mit-license.php                    */
/*                                                                          */
/****************************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Xml;
using System.Text;
using Mondo.Xml;

namespace Mondo.Common
{
    /****************************************************************************/
    /****************************************************************************/
    /// <summary>
    /// A list of lines in a text file
    /// </summary>
    public class LineList : StringList
    {
        private const string kSeparator = "__H8KdWE*(LK@__";

        public delegate string ProcessLine(string strLine);

        /****************************************************************************/
        public LineList(string strList)
        {
            string strFile = strList;

            strFile = strFile.Replace("\r\n", kSeparator);
            strFile = strFile.Replace("\r", kSeparator);
            strFile = strFile.Replace("\n", kSeparator);

            this.Parse(strFile, kSeparator, false, false);
        }

        /****************************************************************************/
        public void Process(ProcessLine fncProcess)
        {
            foreach(string strLine in this)
                fncProcess(strLine);

            return;
        }
    }
}
