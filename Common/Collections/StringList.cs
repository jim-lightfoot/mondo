/****************************************************************************/
/*                                                                          */
/*    The Mondo Libraries  							                        */
/*                                                                          */
/*        Namespace: Mondo.Common							                */
/*             File: StringList.cs										    */
/*        Class(es): StringList										        */
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
    /// A list of strings
    /// </summary>
    public class StringList : List<string>
    {
        /****************************************************************************/
        public StringList()
        {
        }

        /****************************************************************************/
        public StringList(IEnumerable aItems, bool bRemoveEmptys)
        {
            Add(aItems, bRemoveEmptys);
        }

        /****************************************************************************/
        public StringList(Exception ex) 
        {
            while(ex != null)
            {
                this.Add(ex.Message);
                ex = ex.InnerException;
            }
        }

        /****************************************************************************/
        public StringList(IEnumerable aItems) : this(aItems, false)
        {
        }

        /****************************************************************************/
        public StringList(string strList, string strSeparator, bool bRemoveEmptys, bool trim = true)
        {
            Parse(strList, strSeparator, bRemoveEmptys, trim);
        }

        /****************************************************************************/
        protected void Parse(string strList, string strSeparator, bool bRemoveEmptys, bool bTrim = true)
        {
           string[] aParts = strList.Split(new string[] {strSeparator}, bRemoveEmptys ? StringSplitOptions.RemoveEmptyEntries : StringSplitOptions.None);

            foreach(string strPart in aParts)
                Add(strPart, false, bTrim);

            return;
        }

        /****************************************************************************/
        public StringList(string strList, string[] aSeparators, bool bRemoveEmptys)
        {
            string[] aParts = strList.Split(aSeparators, bRemoveEmptys ? StringSplitOptions.RemoveEmptyEntries : StringSplitOptions.None);

            foreach(string strPart in aParts)
                if(strPart.Trim() != "")
                    Add(strPart.Trim());

            return;
        }

        /****************************************************************************/
        public StringList(XmlNode xmlParent, string xPath, string strAttributeName)
        {
            XmlNodeList aNodes = xmlParent.SelectNodes(xPath);

            foreach(XmlNode xmlNode in aNodes)
                Add(xmlNode.GetAttribute(strAttributeName));
        }

        /****************************************************************************/
        public StringList(XmlNode xmlParent, string xPath) : this(xmlParent.SelectNodes(xPath))
        {
        }

        /****************************************************************************/
        public StringList(XmlNodeList aNodes)
        {
            foreach(XmlNode xmlNode in aNodes)
                Add(xmlNode.InnerText.Trim());
        }

        /****************************************************************************/
        public void Add(object obj)
        {
            Add(obj, false);
        }

        /****************************************************************************/
        public void Add(object obj, bool bRemoveEmptys)
        {
            Add(obj, bRemoveEmptys, true);
        }

        /****************************************************************************/
        public void Add(object obj, bool bRemoveEmptys, bool bTrim)
        {
            string strValue = "";

            if(obj is string)
                strValue = bTrim ? obj.Normalized() : obj.ToString();
            else if(obj is IEnumerable)
            {
                IEnumerable aItems = obj as IEnumerable;

                foreach(object objItem in aItems)
                    Add(objItem.Normalized(), bRemoveEmptys);

                return;
            }
            else
                strValue = bTrim ? obj.Normalized() : obj.ToString();

            if(!bRemoveEmptys || strValue != "")
                base.Add(strValue);

            return;
        }

        /****************************************************************************/
        public static string Pack(IEnumerable aItems, string strSeparator)
        {
            StringList aList = new StringList(aItems);

            return(aList.Pack(strSeparator));
        }

        /****************************************************************************/
        public static string Pack(IEnumerable aItems, string strFormat, string strSeparator)
        {
            StringList aList = new StringList(aItems);

            return(aList.Pack(strFormat, strSeparator));
        }

        /****************************************************************************/
        /// <summary>
        /// Packs the given list into a single string separating them by the given separator.
        /// </summary>
        /// <param name="aItems">A list of string to pack</param>
        /// <param name="strSeparator">A string that will separate the values</param>
        /// <param name="bRemoveEmptys">Removes any empty strings if true</param>
        /// <returns>A string with the packed values</returns>
        public static string Pack(IEnumerable aItems, string strSeparator, bool bRemoveEmptys)
        {
            StringList aList = new StringList(aItems, bRemoveEmptys);

            return(aList.Pack(strSeparator));
        }

        /****************************************************************************/
        public string Pack(string strFormat, string strSeparator)
        {
            StringBuilder sb = new StringBuilder();

            foreach(string strAdd in this)
            {
                if(sb.Length != 0)
                    sb.Append(strSeparator);

                if(strAdd == "")
                {
                    string temp = string.Format(strFormat, "__789sdfJK^%$#");

                    temp = temp.Replace("__789sdfJK^%$#", "");
                    sb.Append(temp);
                }
                else
                    sb.AppendFormat(strFormat, strAdd);
            }
   
            return(sb.ToString());
        }

        /****************************************************************************/
        public void Pack(StringBuilder sb, string strFormat, string strSeparator)
        {
            foreach(string strAdd in this)
            {
                if(sb.Length != 0)
                    sb.Append(strSeparator);

                sb.AppendFormat(strFormat, strAdd);
            }
   
            return;
        }

        /****************************************************************************/
        public string Pack(string strSeparator)
        {
            StringBuilder sb = new StringBuilder();

            foreach(string strAdd in this)
            {
                if(sb.Length != 0)
                    sb.Append(strSeparator);

                sb.Append(strAdd);
            }
   
            return(sb.ToString());
        }

        /****************************************************************************/
        public static string GetPart(IList<string> aParts, int iIndex)
        {
            if(iIndex >= aParts.Count)
                return("");

            return(aParts[iIndex]);
        }

        /****************************************************************************/
        public static int GetPartInt(IList<string> aParts, int iIndex)
        {
            return(Utility.ToInt(GetPart(aParts, iIndex)));
        }

        /****************************************************************************/
        public static List<string> GetTextRows(string strFileData, bool bRemoveEmptys)
        {
            const string kRandomChars1 = "*()@Kldwkld30p2elk;dl9sdlJKL^%0p";
            const string kRandomChars2 = "p90e23iop#w2ep0-d0p9-pioswd()*$@";
            
            strFileData = strFileData.Replace("\r\n",        kRandomChars1);
            strFileData = strFileData.Replace("\r",          kRandomChars2);
            strFileData = strFileData.Replace("\n",          "\r\n");
            strFileData = strFileData.Replace(kRandomChars1, "\r\n");
            strFileData = strFileData.Replace(kRandomChars2, "\r\n");

            return(StringList.ParseString(strFileData, "\r\n", bRemoveEmptys));
        }

        /****************************************************************************/
        /// <summary>
        /// Takes the given string  containing a list of values separated by the given 
        /// separator and returns a List<string>, removing emptys if requested.
        /// </summary>
        /// <param name="strList">A string containing the list</param>
        /// <param name="strSeparator">A string that separates the values</param>
        /// <param name="bRemoveEmptys">Removes any empty string if true</param>
        /// <returns>A StringCollection containing the list of strings</returns>
        public static List<string> ParseString(string strList, string strSeparator, bool bRemoveEmptys)
        {
            List<string> aReturn = new List<string>();

            if(strList != null && strList.Length > 0)
            {
                int iSepLen = strSeparator.Length;
                string strTemp = strList;

                while(true)
                {
                    strTemp = strTemp.Trim();

                    int iFind = strTemp.IndexOf(strSeparator);

                    if(iFind == -1)
                    {
                        if(!bRemoveEmptys || strTemp != "")
                            aReturn.Add(strTemp);

                        break;
                    }

                    string strAdd = strTemp.Substring(0, iFind).Trim();

                    if(!bRemoveEmptys || strAdd != "")
                        aReturn.Add(strAdd);

                    strTemp = strTemp.Remove(0, iFind + iSepLen);
                }
            }

            return (aReturn);
        }
    }

    /****************************************************************************/
    /****************************************************************************/
    /// <summary>
    /// A dictionary of strings
    /// </summary>
    public class StringDictionary : Dictionary<string, string>
    {
        /****************************************************************************/
        public StringDictionary()
        {
        }

        /****************************************************************************/
        public StringDictionary(string strList, string strSeparator)
        {
            string[] aStrings = strList.Split(new string[] {strSeparator}, StringSplitOptions.RemoveEmptyEntries);

            foreach(string strValue in aStrings)
             {
                string strNew = strValue.Normalized();

                this.Add(strNew, strNew);
             }
        }

        /****************************************************************************/
        public StringDictionary(string strList, string strSeparator, string strSeparator2)
        {
            string[] aStrings = strList.Split(new string[] {strSeparator}, StringSplitOptions.RemoveEmptyEntries);

            foreach(string strValue in aStrings)
             {
                string[] aPair = strValue.Split(new string[] {strSeparator2}, StringSplitOptions.None);

                this.Add(aPair[0].Trim(), aPair[1].Trim());
             }
        }

        /****************************************************************************/
        public StringDictionary(IEnumerable aItems)
        {
             foreach(object objValue in aItems)
             {
                string strValue = objValue.Normalized();

                this.Add(strValue, strValue);
             }
        }
    }
}
