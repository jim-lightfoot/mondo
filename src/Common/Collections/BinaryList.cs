/****************************************************************************/
/*                                                                          */
/*    The Mondo Libraries  							                        */
/*                                                                          */
/*        Namespace: Mondo.Common							                */
/*             File: BinaryList.cs										    */
/*        Class(es): BinaryList										        */
/*          Purpose: A collection of strings                                */
/*                                                                          */
/*  Original Author: Jim Lightfoot                                          */
/*    Creation Date: 12 Sep 2001                                            */
/*                                                                          */
/*   Copyright (c) 2001-2008 - Jim Lightfoot, All rights reserved           */
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
    /// A list of byte arrays
    /// </summary>
    public class BinaryList : List< byte[] >, IDisposable
    {
        private const string Spacer = "__R*(U@Ld]*TkSe8aM__";

        /****************************************************************************/
        public BinaryList()
        {
        }

        /****************************************************************************
         * Takes a binary list that has been converted to Base64 strings and 
         * concatenated with the spacer (see ToString())
         ****************************************************************************/
        public BinaryList(string strList)
        {
            StringList aParts = new StringList(strList, Spacer, true);

            foreach(string strPart in aParts)
                this.Add(strPart.FromBase64String());
        }

        /****************************************************************************
         * Takes a binary list that has been converted to Base64 strings and 
         * concatenated with the spacer and then converted to a byte array (see ToArray())
         ****************************************************************************/
        public BinaryList(byte[] aList) : this(Encoding.ASCII.GetString(aList))
        {
        }

        /****************************************************************************/
        public new byte[] ToArray()
        {
            return(Encoding.ASCII.GetBytes(ToString()));
        }

        /****************************************************************************/
        public override string ToString()
        {
            StringList aList = new StringList();

            foreach(byte[] aItem in this)
                aList.Add(aItem.ToBase64String());

            return(aList.Pack(Spacer));
        }

        /****************************************************************************/
        public void Dispose()
        {
            foreach(byte[] aData in this)
                aData.Clear();

            this.Clear();
        }
    }
}
