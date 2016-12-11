/****************************************************************************/
/*                                                                          */
/*    The Mondo Libraries  							                        */
/*                                                                          */
/*        Namespace: Mondo.Common							                */
/*             File: StringExtensions.cs								    */
/*        Class(es): StringExtensions								        */
/*          Purpose: String helper functions                                */
/*                                                                          */
/*  Original Author: Jim Lightfoot                                          */
/*    Creation Date: 12 Sep 2001                                            */
/*                                                                          */
/*   Copyright (c) 2001-2016 - Jim Lightfoot, All rights reserved           */
/*                                                                          */
/*  Licensed under the MIT license:                                         */
/*    http://www.opensource.org/licenses/mit-license.php                    */
/*                                                                          */
/****************************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Xml;
using System.Security;
using System.Text;

using Mondo.Xml;

namespace Mondo.Common
{
    /****************************************************************************/
    /****************************************************************************/
    public static class StringExtensions
    {
        private static string kReplace = ")*@LKD_)@oildas-";

        /****************************************************************************/
        public static string Normalized(this object objValue)
        {
            if(objValue == null)
                return("");

            return(objValue.ToString().Trim());
        }        
        
        /****************************************************************************/
        public static bool IsEmpty(this object objValue)
        {
            if(objValue == null)
                return(true);

            return(objValue.ToString().Trim().Length == 0);
        }

        #region StringBuilder 

        /****************************************************************************/
        public static StringBuilder AppendSpaces(this StringBuilder sb, int nSpaces)
        {
            if(nSpaces > 0)
                sb.Append("                                                                                                    ".Substring(0, nSpaces));

            return(sb);
        }        
        
        /****************************************************************************/
        public static void Append(this StringBuilder sb, string str1, string str2)
        {
            sb.Append(str1);
            sb.Append(str2);
        }

        /****************************************************************************/
        public static void Append(this StringBuilder sb, string str1, string str2, string str3)
        {
            sb.Append(str1);
            sb.Append(str2);
            sb.Append(str3);
        }

        /****************************************************************************/
        public static void Append(this StringBuilder sb, string str1, string str2, string str3, string str4)
        {
            sb.Append(str1);
            sb.Append(str2);
            sb.Append(str3);
            sb.Append(str4);
        }

        /****************************************************************************/
        public static void Append(this StringBuilder sb, string str1, string str2, string str3, string str4, string str5)
        {
            sb.Append(str1);
            sb.Append(str2);
            sb.Append(str3);
            sb.Append(str4);
            sb.Append(str5);
        }

        /****************************************************************************/
        public static void Append(this StringBuilder sb, string str1, string str2, string str3, string str4, string str5, string str6)
        {
            sb.Append(str1);
            sb.Append(str2);
            sb.Append(str3);
            sb.Append(str4);
            sb.Append(str5);
            sb.Append(str6);
        }

        #endregion

        /****************************************************************************/
        public static string NormalizeNewLines(this object objValue)
        {
            return(objValue.Normalized().Replace("\r\n", kReplace).Replace("\r", kReplace).Replace("\n", kReplace).Replace(kReplace, "\r\n"));
        }        
        
        /****************************************************************************/
        public static string NormalizeSpace(this object objValue)
        {
            if(objValue == null)
                return("");

            string strReturn = objValue.ToString().Trim();

            while(strReturn.Contains("  "))
                strReturn = strReturn.Replace("  ", " ");

            return(strReturn);
        }        
        
        /****************************************************************************/
        public static string Truncate(this string strValue, int nChars)
        {
            strValue = strValue.Normalized();

            if(strValue.Length <= nChars)
                return(strValue);

            return(strValue.Substring(0, nChars).Trim());
        }        
        
        /****************************************************************************/
        public static string EnsureLastChar(this string strValue, string strLast)
        {
            return(strValue.EnsureEndsWith(strLast));
        }

        /****************************************************************************/
        public static string EnsureStartsWith(this string strValue, char chFirst)
        {
            return(EnsureStartsWith(strValue, chFirst.ToString()));
        }

        /****************************************************************************/
        public static string EnsureStartsWith(this string strValue, string strFirst)
        {
            if(strValue.IndexOf(strFirst) == 0)
                return(strValue);

            return(strFirst + strValue);
        }

        /****************************************************************************/
        public static string EnsureNotStartsWith(this string val, string start)
        {
            if(!val.StartsWith(start))
                return(val);

            return(val.Substring(start.Length).Normalized().EnsureNotStartsWith(start));
        }

        /****************************************************************************/
        public static string EnsureEndsWith(this string strValue, string strLast)
        {
            if(strValue == null)
                strValue = "";

            if(strLast == null || strLast == "")
                return(strValue);

            if(strValue.EndsWith(strLast))
                return(strValue);

            return(strValue + strLast);
        }
        
        /****************************************************************************/
        public static string EnsureEndsWith(this string strValue, char chLast)
        {
            return(EnsureEndsWith(strValue, chLast.ToString()));
        }

        /****************************************************************************/
        public static string EnsureEndsWithSeparator(this string strValue)
        {
            return(EnsureEndsWith(strValue, System.IO.Path.DirectorySeparatorChar));
        }
        
        /****************************************************************************/
        public static string EnsureNotEndsWithSeparator(this string strValue)
        {
            return(strValue.EnsureNotEndsWith(System.IO.Path.DirectorySeparatorChar.ToString()));
        }
        
        /****************************************************************************/
        public static string EnsureNotEndsWith(this string strValue, string strEnd)
        {
            if(string.IsNullOrWhiteSpace(strValue))
                return "";

            if(!strValue.EndsWith(strEnd))
              return strValue;

            return strValue.Substring(0, strValue.Length - strEnd.Length).Normalized().EnsureNotEndsWith(strEnd);
        }
        
        /****************************************************************************/
        public static string RemoveLastChar(this string strValue)
        {
            if(strValue == null)
                strValue = "";

            if(strValue == "")
                return(strValue);

            return(strValue.Substring(0, strValue.Length - 1)).Trim();
        }
        
        /****************************************************************************/
        public static string RemoveLast(this string strValue, string strRemove)
        {
            strValue = strValue.Normalized();

            if(strValue.EndsWith(strRemove, StringComparison.CurrentCultureIgnoreCase))
                return(strValue.Substring(0, strValue.Length - strRemove.Length)).Trim();

            return(strValue);
        }
        
        /****************************************************************************/
        public static string StripAfter(this string strValue, string strAfter)
        {
            strValue = strValue.Normalized();

            int iIndex = strValue.IndexOf(strAfter);

            if(iIndex != -1)
                strValue = strValue.Substring(0, iIndex);

            return(strValue.Trim());
        } 

        /****************************************************************************/
        public static string StripUpTo(this string strValue, string strAfter)
        {
            strValue = strValue.Normalized();

            int iIndex = strValue.IndexOf(strAfter);

            if(iIndex != -1)
                strValue = strValue.Substring(iIndex + strAfter.Length).Trim();

            return(strValue);
        }

        /****************************************************************************/
        public static string StripUpToLast(this string strValue, string strAfter)
        {
            int iIndex = strValue.LastIndexOf(strAfter);

            if(iIndex != -1)
                strValue = strValue.Substring(iIndex+strAfter.Length).Trim();

            return(strValue);
        }

        /****************************************************************************/
        public static string StripFromTo(this string strValue, string strFrom, string strTo)
        {
            int iFrom = strValue.IndexOf(strFrom);

            if(iFrom != -1)
            {
                int iTo = strValue.IndexOf(strTo, iFrom);

                if(iTo != -1)
                    return(strValue.Remove(iFrom, (iTo + strTo.Length) - iFrom));
            }
            return(strValue);
        }

        /****************************************************************************/
        public static string StripAfterLast(this string strValue, string strAfter, bool include = false)
        {
            strValue = strValue.Trim();

            int iIndex = strValue.LastIndexOf(strAfter);

            if(iIndex != -1)
                strValue = strValue.Substring(0, iIndex + (include ? 0 : strAfter.Length));

            return(strValue);
        }

        /****************************************************************************/
        public static  string ReadLine(this System.IO.BinaryReader objReader)
        {
            StringBuilder sbLine = new StringBuilder();
            char          chRead = '\0';

            try
            { 
                while(true)
                {
                    chRead = objReader.ReadChar();

                    if(chRead == '\r' || chRead == '\n')
                        break;

                    sbLine.Append(chRead);
                }
            
                if(chRead == '\r' && objReader.PeekChar() == '\n')
                    objReader.ReadChar();
            }
            catch(EndOfStreamException)
            {
            }

            return(sbLine.ToString());
        }

        /****************************************************************************/
        public static bool ContainsDigit(this string strValue)
        {
            foreach(char chValue in strValue)
                if(char.IsDigit(chValue))
                    return(true);

            return(false);
        }

        /****************************************************************************/
        public static bool ContainsPunctuation(this string strValue)
        {
            foreach(char chValue in strValue)
                if(char.IsPunctuation(chValue))
                    return(true);

            return(false);
        }

        /****************************************************************************/
        public static bool ContainsLetter(this string strValue)
        {
            foreach(char chValue in strValue)
                if(char.IsLetter(chValue))
                    return(true);

             return(false);
        }

        /****************************************************************************/
        public static string ToBase64String(this byte[] aData)
        {
            return(Convert.ToBase64String(aData));
        }

        /****************************************************************************/
        public static byte[] FromBase64String(this string strData)
        {
            return(Convert.FromBase64String(strData));
        }

        /****************************************************************************/
        public static SecureString ToSecureString(this string strValue)
        {
            int    iLength     = strValue.Length;
            char[] aCharacters = strValue.ToCharArray();

           SecureString ss = new SecureString();

            foreach(char ch in aCharacters)
                ss.AppendChar(ch);

            ss.MakeReadOnly();

            for(int i = 0; i < iLength; ++i)
                aCharacters[i] = (char)0;

            return(ss);
        }

        /****************************************************************************/
        public static byte[] GetBytes(this string str)
        {
            return(System.Text.UnicodeEncoding.Unicode.GetBytes(str));
        }

        /****************************************************************************/
        public static string FromBytes(this byte[] a)
        {
            return(System.Text.UnicodeEncoding.Unicode.GetString(a));
        }

        /****************************************************************************/
        public static SecureString ToSecureString(this byte[] aBytes)
        {
            int    iLength     = aBytes.Length;
            char[] aCharacters = new char[iLength];

            Convert.ToBase64CharArray(aBytes, 0, iLength, aCharacters, 0);

            SecureString ss = new SecureString();

            foreach(char ch in aCharacters)
                ss.AppendChar(ch);

            ss.MakeReadOnly();

            for(int i = 0; i < iLength; ++i)
                aCharacters[i] = (char)0;

            return(ss);
        }
         
        /****************************************************************************/
        public static int FindWhiteSpace(this string strValue, int iStart, bool bForwards)
        {
            int iLength = strValue.Length;
            
            if(bForwards)
            {
                while(iStart < iLength)
                {
                    if(char.IsWhiteSpace(strValue[iStart]))
                        return(iStart);
                        
                    ++iStart;
                }
            }
            else
            {
                while(iStart >= 0)
                {
                    if(char.IsWhiteSpace(strValue[iStart]))
                        return(iStart);
                        
                    --iStart;
                }                
            }
                        
            return(-1);
        }

        /****************************************************************************/
        /// <summary>
        /// Capitalizes all words in the given string.
        /// </summary>
        /// <param name="strText">The string to capitalize</param>
        /// <returns>The result</returns>
        public static string Capitalize(this string strText)
        {
            if(strText.Length > 1)
                return(strText.Substring(0, 1).ToUpper() + strText.Substring(1).ToLower());

            return(strText.ToUpper());
        }

        /****************************************************************************/
        public static string CapitalizeWords(this string strValue)
        {
            StringList aParts = new StringList(strValue.ToLower(), " ", true);
            int        nParts = aParts.Count;

            for(int i = 0; i < nParts; ++i)
                aParts[i] = Capitalize(aParts[i]);

            return(aParts.Pack(" "));
        }

        /****************************************************************************/
        public static Dictionary<string, string> FromJSONToDictionary(this string strValue)
        {
            Dictionary<string, string> dReturn = new Dictionary<string,string>();

            strValue = strValue.Trim().Substring(1).StripAfterLast("}", true).Trim();

            int index = strValue.IndexOf(":");

            while(index != -1)
            {
                string key = strValue.Substring(0, index).Replace("\"", "").Trim();

                strValue = strValue.Substring(index+1);

                index = strValue.IndexOf(",");

                if(index == -1)
                {
                    dReturn.Add(key, strValue.Trim());
                    break;
                }

                dReturn.Add(key, strValue.Substring(0, index).Trim());

                strValue = strValue.Substring(index+1);

                index = strValue.IndexOf(":");
            }

            return(dReturn);
        }

        /****************************************************************************/
        public static string FormatText(this string strValue, string strFormat)
        {
            StringBuilder sb = new StringBuilder();
            int           iIndex = 0;

            try
            { 
                foreach(char ch in strFormat)
                {
                    if(ch == '#')
                    {
                        sb.Append(strValue.Substring(iIndex++, 1));
                    }
                    else
                        sb.Append(ch);
                }
            }
            catch
            {
            }

            return(sb.ToString());
        }
    }
}
