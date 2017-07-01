/****************************************************************************/
/*                                                                          */
/*    The Mondo Libraries  							                        */
/*                                                                          */
/*        Namespace: Mondo.Common							                */
/*             File: Utility.cs										        */
/*        Class(es): Utility										        */
/*          Purpose: Utility for global functions                           */
/*                                                                          */
/*  Original Author: Jim Lightfoot                                          */
/*    Creation Date: 19 Sep 2001                                            */
/*                                                                          */
/*   Copyright (c) 2001-2016 - Jim Lightfoot, All rights reserved           */
/*                                                                          */
/*  Licensed under the MIT license:                                         */
/*    http://www.opensource.org/licenses/mit-license.php                    */
/*                                                                          */
/****************************************************************************/

using System;
using System.Diagnostics;
using System.IO;
using System.Xml;
using System.Xml.XPath;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Runtime.InteropServices;

namespace Mondo.Common
{
    /****************************************************************************/
    /****************************************************************************/
    public class Pair<T>
    {
        public T First;
        public T Second;

        /****************************************************************************/
        public Pair(T obj1, T obj2)
        {
            First = obj1;
            Second = obj2;
        }
    }

    /****************************************************************************/
    /****************************************************************************/
    public sealed class cDebug
    {
        /****************************************************************************/
        [Conditional("DEBUG")]
        public static void Notify(string strCategory, string strMessage)
        {
            EventLog.Error(strCategory, strMessage);
        }

        /****************************************************************************/
        [Conditional("DEBUG")]
        public static void Capture(Exception e)
        {
            // Do nothing
        }

        /****************************************************************************/
        [Conditional("DEBUG")]
        public static void ToFile(string strData, string strFileName)
        {
            try
            {
                DiskFile.ToFile(strData, "c:\\Documents\\Development\\Output\\" + strFileName);
            }
            catch(Exception ex)
            {
                cDebug.Capture(ex);
                //ErrorHandler.Handle(new Exception("Unable to write debug file", ex));
            }
        }

        /****************************************************************************/
        [Conditional("DEBUG")]
        public static void ToFile(byte[] aData, string strFileName)
        {
            try
            {
                DiskFile.ToFile(aData, "c:\\Documents\\Development\\Output\\" + strFileName);
            }
            catch(Exception ex)
            {
                cDebug.Capture(ex);
                //ErrorHandler.Handle(new Exception("Unable to write debug file", ex));
            }
        }

        /****************************************************************************/
        [Conditional("DEBUG")]
        public static void Append(ref string strAppendTo, string strText)
        {
            strAppendTo += strText;
        }

        /****************************************************************************/
        [Conditional("DEBUG")]
        public static void Prepend(ref string strAppendTo, string strText)
        {
            strAppendTo = strText + strAppendTo;
        }
    }

    /****************************************************************************/
    /****************************************************************************/
    public sealed class DebugConsole
    {
        /****************************************************************************/
        [Conditional("DEBUG")]
        public static void WriteLine(string strMessage)
        {
            Console.WriteLine(strMessage);
        }
    }

    /****************************************************************************/
    /****************************************************************************/
    public class ValueOutOfRangeException : Exception {}
    public class NotFoundException        : Exception {}

    /****************************************************************************/
    /****************************************************************************/
    public static class UtilityExtensions
    {
        private static DateTime _daydexBase = new DateTime(1900, 1, 1);

        /****************************************************************************/
        public static int Age(this DateTime dtBirth, DateTime dtCurrent)
        {
            return((int)Math.Floor((dtCurrent - dtBirth).TotalHours / (double)8766));
        }        

        /*****************************************************************************/
        public static int Daydex(this DateTime dtValue)
        {
            if(dtValue == DateTime.MinValue)
                return(0);

            if(dtValue == DateTime.MaxValue)
                return(int.MaxValue);

            return((dtValue.Date - _daydexBase).Days);
        }

        /*****************************************************************************/
        public static DateTime FromDaydex(this int value)
        {
            if(value <= 0)
                return DateTime.MinValue;

            if(value == int.MaxValue)
                return DateTime.MaxValue;

            return _daydexBase.AddDays(value);
        }

        /****************************************************************************/
        public static string ToShortString(this bool bValue)
        {
             return(bValue ? "1" : "0");
        }        

        /****************************************************************************/
        public static string ToLongString(this bool bValue)
        {
            return(bValue ? "True" : "False");
        }        

        /****************************************************************************/
        public static long ValidateID(this long val)
        {
            if(val <= 0)
              throw new ValueOutOfRangeException();

            return val;
        }        

        /****************************************************************************/
        public static string ToGuidString(this Guid idValue)
        {
            return(Utility.ToGuidString(idValue));
        }        

        /****************************************************************************/
        public static short ToBit(this bool bValue)
        {
            return((short)(bValue ? 1 : 0));
        }        

        /****************************************************************************/
        public static byte[] ToByteArray(this Stream objFrom)
        {
            return(Utility.StreamToStream(objFrom).ToArray());
        }        

        /****************************************************************************/
        public static bool TestFlag(this int iFlags, int iFlag)
        {
            return(Flag.Test(iFlags, iFlag));
        }        

        /****************************************************************************/
        public static MemoryStream ToStream(this byte[] aBytes)
        {
            MemoryStream objStream = new MemoryStream();   
       
            objStream.Write(aBytes, 0, aBytes.Length);
            objStream.Position = 0;

            return(objStream);
        }

        /****************************************************************************/
        public static void Clear(this byte[] aBytes)
        {
            int nBytes = aBytes.Length;
       
            for(int i = 0; i < nBytes; ++i)
                aBytes[i] = 0;

            return;
        }

        /****************************************************************************/
        public static void Clear(this char[] aChars)
        {
            int nChars = aChars.Length;
       
            for(int i = 0; i < nChars; ++i)
                aChars[i] = '\0';

            return;
        }

        /****************************************************************************/
        public static byte[] DeepClone(this byte[] aBytes)
        {
            byte[] aNew = new byte[aBytes.Length];
       
            aBytes.CopyTo(aNew, 0);

            return(aNew);
        }

        /****************************************************************************/
        public static byte[] DeepClone(this byte[] aBytes, int srcOffset, int iLength)
        {
            if(iLength == -1)
                iLength = aBytes.Length - srcOffset;

            byte[] aNew = new byte[iLength];
       
            Buffer.BlockCopy(aBytes, srcOffset, aNew, 0, iLength);

            return(aNew);
        }

        /****************************************************************************/
        public static bool IsEmptyList<T>(this IEnumerable<T> list)
        {
            foreach(T obj in list)
                return(false);

            return(true);
        }
    }    

    /****************************************************************************/
    /****************************************************************************/
    public static class StreamExtensions
    {
        private const int kBufferSize = 512 * 1024; // 512k

        /****************************************************************************/
        public static void Write(this Stream objDestination, Stream objSource)
        {
            byte[] aBuffer = new byte[kBufferSize];
            int    nRead   = 0;

            do
            {
                nRead = objSource.Read(aBuffer, 0, kBufferSize);

                if(nRead > 0)
                    objDestination.Write(aBuffer, 0, nRead);
            } 
            while(nRead > 0);

            objDestination.Flush();
        }
    }    

    /****************************************************************************/
    /****************************************************************************/
    public sealed class Utility
    {
        /****************************************************************************/
        public static Guid NullGuid
        {
            get
            {
                return(Guid.Empty);
            }
        }

        /****************************************************************************/
        public static int GuidLength
        {
            get
            {
                return(Guid.Empty.ToString().Length);
            }
        }

        /****************************************************************************/
        public static bool IsEven(uint uValue)
        {
            return(!IsOdd(uValue));
        }

        /****************************************************************************/
        public static bool IsEven(int iValue)
        {
            return(!IsOdd(iValue));
        }

        /****************************************************************************/
        public static bool IsOdd(uint uValue)
        {
            return(Flag.Test(uValue, 1));
        }

        /****************************************************************************/
        public static bool IsOdd(int iValue)
        {
            return(Flag.Test(iValue, 1));
        }

        /****************************************************************************/
        public static uint ClearFlag(uint uFlags, uint uFlag)
        {
            return(Flag.Clear(uFlags, uFlag));
        }

        /****************************************************************************/
        public static void WriteMessageFile(string strMessage, string strPath)
        {
            using(StreamWriter sw = new StreamWriter(strPath))
                sw.WriteLine(strMessage);
        }

        /****************************************************************************/
        public static string GetHashString(uint uValue)
        {
            string strCode = string.Format("{0:X}", uValue);

            strCode = strCode.PadLeft(8, '0');

            return(strCode);
        }

        /****************************************************************************/
        public static string GetHashString(ulong uValue)
        {
            string strCode = string.Format("{0:X}", uValue);

            strCode = strCode.PadLeft(16, '0');

            return(strCode);
        }

        /****************************************************************************/
        public static string GetHashString(object obj)
        {
            return(GetHashString((uint)obj.GetHashCode()));
        }
      
        /****************************************************************************/
        public static byte ToByte(object objValue)
        {
            return(ToByte(objValue, 0));
        }

        /****************************************************************************/
        public static byte ToByte(object objValue, byte bDefault)
        {
            if(objValue == null)
                return(bDefault);

            if(objValue.ToString().Trim().Length == 0)
                return(bDefault);

            return(byte.Parse(objValue.ToString()));
        }

        /****************************************************************************/
        public static int ToInt(object objValue)
        {
            return(ToInt(objValue, 0, false));
        }

        /****************************************************************************/
        public static T Convert<T>(object val, T defaultVal = default(T), bool bThrow = false) where T : struct
        {
            if(val == null)
                return(defaultVal);

            if(typeof(T) == typeof(bool))
                val = ToBoolString(val);

            if(typeof(T) == typeof(Guid)) 
                return((T)((object)ToGuid(val)));

            try
            { 
                return((T)System.Convert.ChangeType(val, typeof(T)));
            }
            catch(Exception ex)
            {
                if(bThrow)
                    throw ex;

                return(defaultVal);
            }
        }

        /****************************************************************************/
        public static object ConvertType(object objValue, System.Type objType)
        {
            if(objType == typeof(string))
                return(objValue.Normalized());

            if(objType == typeof(bool))
                return(Utility.ToBool(objValue));

            if(objType == typeof(int))
                return(Utility.ToInt(objValue));

            if(objType == typeof(long))
                return(Utility.ToLong(objValue));

            if(objType == typeof(short))
                return(Utility.ToShort(objValue));

            if(objType == typeof(uint))
                return(Utility.ToUint(objValue));

            if(objType == typeof(ushort))
                return(Utility.ToUShort(objValue));

            if(objType == typeof(ulong))
                return(Utility.ToULong(objValue));

            if(objType == typeof(decimal))
                return(Utility.ToDecimal(objValue));

            if(objType == typeof(float))
                return(Utility.ToFloat(objValue));

            if(objType == typeof(double))
                return(Utility.ToDouble(objValue));

            if(objType == typeof(DateTime))
                return(Utility.ToDateTime(objValue));

            if(objType == typeof(Guid))
                return(Utility.ToGuid(objValue));

            return(objValue);
        }

        /****************************************************************************/
        public static int ToInt(object objValue, bool bTrim)
        {
            return(ToInt(objValue, 0, bTrim));
        }

        /****************************************************************************/
        public static int ToInt(object objValue, int iDefault)
        {
            return(ToInt(objValue, iDefault, false));
        }

        /****************************************************************************/
        public static int ToInt(object objValue, int iDefault, bool bTrim)
        {
            if(objValue == null)
                return(iDefault);

            if(objValue is int)
                return((int)objValue);

            if(objValue is long)
                return(int.Parse(objValue.ToString()));

            decimal dValue = 0m;

            if(decimal.TryParse(objValue.ToString(), out dValue))
            {
                if(Math.Floor(dValue) == dValue)
                {
                    if(bTrim)
                    {
                        if(dValue <= (decimal)int.MinValue)
                            return(int.MinValue);

                        if(dValue >= (decimal)int.MaxValue)
                            return(int.MaxValue);
                    }

                    if(dValue >= (decimal)int.MinValue && dValue <= (decimal)int.MaxValue)
                        return((int)dValue);
                }

                // Just to get the right exception
                return(int.Parse(objValue.ToString()));
            }

            if(objValue is XPathNodeIterator)
            {
                XPathNodeIterator objIterator = objValue as XPathNodeIterator;
                string strValue = "";

                try
                {
                    objIterator.MoveNext();
                    strValue = objIterator.Current.Value;
                }
                catch
                {
                }

                return(Utility.ToInt(strValue));
            }

            // Blank string - return default
            if(objValue.ToString().Trim().Length == 0)
                return(iDefault);

            // Probably not an int at this point
            return(int.Parse(objValue.ToString()));
        }

        /****************************************************************************/
        public static short ToShort(object objValue)
        {
            return(ToShort(objValue, 0));
        }

        /****************************************************************************/
        public static short ToShort(object objValue, short iDefault)
        {
            if(objValue == null)
                return(iDefault);

            if(objValue.ToString().Trim().Length == 0)
                return(iDefault);

            return(short.Parse(objValue.ToString()));
        }

        /****************************************************************************/
        public static ushort ToUShort(object objValue)
        {
            return(ToUShort(objValue, 0));
        }

        /****************************************************************************/
        public static ushort ToUShort(object objValue, ushort iDefault)
        {
            if(objValue == null)
                return(iDefault);

            if(objValue.ToString().Trim().Length == 0)
                return(iDefault);

            return(ushort.Parse(objValue.ToString()));
        }

        /****************************************************************************/
        public static long ToLong(object objValue)
        {
            return(ToLong(objValue, 0));
        }

        /****************************************************************************/
        public static long ToLong(object objValue, long iDefault)
        {
            if(objValue == null)
                return(iDefault);

            if(objValue.ToString().Trim().Length == 0)
                return(iDefault);

            return(long.Parse(objValue.ToString()));
        }

        /****************************************************************************/
        public static ulong ToULong(object objValue)
        {
            return(ToULong(objValue, 0));
        }

        /****************************************************************************/
        public static ulong ToULong(object objValue, ulong iDefault)
        {
            if(objValue == null)
                return(iDefault);

            if(objValue.ToString().Trim().Length == 0)
                return(iDefault);

            return(ulong.Parse(objValue.ToString()));
        }

        /****************************************************************************/
        /// <summary>
        /// Convert the given value into a boolean
        /// </summary>
        /// <param name="objValue">Value to convert</param>
        /// <returns></returns>
        public static bool ToBool(object objValue)
        {
            return(ToBool(objValue, false));
        }

        /****************************************************************************/
        /// <summary>
        /// Convert the given value into a boolean
        /// </summary>
        /// <param name="objValue">Value to convert</param>
        /// <returns></returns>
        private static string ToBoolString(object objValue)
        {
            try
            {
                if(objValue == null)
                    return("");

                if(objValue is bool)
                    return((bool)objValue ? "true" : "false");

                string strValue = objValue.ToString().ToLower();
                long iValue = 0;

                if(long.TryParse(strValue, out iValue) && iValue > 0)
                    return("true");

                switch(strValue)
                {
                    case "x":       
                    case "y":       
                    case "yes":     
                    case "t":       
                    case "checked":       
                    case "true":    return("true");

                    case "":       
                    case "n":       
                    case "no":     
                    case "f":       
                    case "false":    
                    case "0":       return("false");

                    default:        return("");
                }
            }
            catch
            {
                return("");
            }
        }

        /****************************************************************************/
        /// <summary>
        /// Convert the given value into a boolean
        /// </summary>
        /// <param name="objValue">Value to convert</param>
        /// <returns></returns>
        public static bool ToBool(object objValue, bool bDefault)
        {
            try
            {
                if(objValue == null)
                    return(bDefault);

                if(objValue is bool)
                    return((bool)objValue);

                string strValue = objValue.ToString().ToLower();
                long iValue = 0;

                if(long.TryParse(strValue, out iValue) && iValue > 0)
                    return(true);

                switch(strValue)
                {
                    case "x":       
                    case "y":       
                    case "yes":     
                    case "t":       
                    case "checked":       
                    case "true":    return(true);

                    case "":       
                    case "n":       
                    case "no":     
                    case "f":       
                    case "false":    
                    case "0":       return(false);

                    default:        return(bDefault);
                }
            }
            catch
            {
                return(bDefault);
            }
        }

        /****************************************************************************/
        public static bool IsTrue(string strValue)
        {
            return(ToBool(strValue));
        }

        /****************************************************************************/
        public static bool IsGuid(string strValue)
        {
            Guid guidValue = Guid.Empty;

            return(Guid.TryParse(strValue, out guidValue));
        }
              
        /****************************************************************************/
        public static Guid ToGuid(object objValue)
        {
            if(objValue == null)
                return(Guid.Empty);

            if(objValue is Guid)
                return((Guid)objValue);

            string strValue = objValue.ToString().Trim();

            if(strValue == "")
                return(Guid.Empty);

            Guid guidValue = Guid.Empty;

            Guid.TryParse(strValue, out guidValue);

            return(guidValue);
        }
        
        /****************************************************************************/
        public static string ToGuidString(object objValue)
        {
            if(objValue == null)
                return(NullGuid.ToString());
 
            string strValue = objValue.ToString().Trim();

            if(strValue == "")
                return(NullGuid.ToString());

            return(strValue.ToUpper());
        }

        /****************************************************************************/
        /// <summary>
        /// Converts an object to an decimal. If the string is not a valid decimal it will return zero.
        /// </summary>
        /// <param name="objValue">The object to convert</param>
        /// <returns>The resulting value</returns>
        public static decimal ToDecimal(object objValue)
        {
            return(ToDecimal(objValue, 0M));
        }

        /****************************************************************************/
        /// <summary>
        /// Converts an object to an decimal. If the string is not a valid decimal it will return the default value.
        /// </summary>
        /// <param name="objValue">The object to convert</param>
        /// <param name="iDefault">The default value to use if the object is not a valid decimal</param>
        /// <returns>The resulting value</returns>
        public static decimal ToDecimal(object objValue, decimal iDefault)
        {
            try
            {
                if(objValue != null)
                    return(decimal.Parse(objValue.ToString()));
            }
            catch
            {
            }

            return(iDefault);
        }

        /****************************************************************************/
        /// <summary>
        /// Converts an object to an double. If the string is not a valid double it will return zero.
        /// </summary>
        /// <param name="objValue">The object to convert</param>
        /// <returns>The resulting value</returns>
        public static double ToDouble(object objValue)
        {
            return(ToDouble(objValue, 0d));
        }

        /****************************************************************************/
        /// <summary>
        /// Converts an object to an double. If the string is not a valid double it will return the default value.
        /// </summary>
        /// <param name="objValue">The object to convert</param>
        /// <param name="iDefault">The default value to use if the object is not a valid decimal</param>
        /// <returns>The resulting value</returns>
        public static double ToDouble(object objValue, double dDefault)
        {
            try
            {
                return(double.Parse(objValue.ToString()));
            }
            catch
            {
                return(dDefault);
            }
        }

        /****************************************************************************/
        /// <summary>
        /// Converts an object to a DateTime. If the string is not a valid date it will return DateTime.MinValue.
        /// </summary>
        /// <param name="objValue">The object to convert</param>
        /// <returns>The resulting integer</returns>
        public static DateTime ToDateTime(object objValue)
        {
            try
            {
                if(objValue != null)
                {
                    if(objValue is DateTime)
                        return((DateTime)objValue);

                    return(DateTime.Parse(objValue.ToString()));
                }
            }
            catch
            {
            }

            return DateTime.MinValue;
        }

        /*****************************************************************************/
        public static int ToDaydex(object objValue)
        {
            try
            {
                return Utility.ToDateTime(objValue).Daydex();
            }
            catch
            { 
                return 0;
            }
        }

        /*****************************************************************************/
        public static DateTime FromDaydex(object value)
        {
            try
            {
                int daydex = Utility.ToInt(value, 0);

                return daydex.FromDaydex();
            }
            catch
            {
                return DateTime.MinValue;
            }
        }

        /****************************************************************************/
        public static decimal ToCurrency(object objValue)
        {
            if(!objValue.IsEmpty())
            {
                try
                {
                    string strCurrency = objValue.Normalized();

                    if(strCurrency == "")
                        return(0);

                    string strNew = "";

                    foreach(char chCheck in strCurrency)
                        if(char.IsDigit(chCheck) || chCheck == '.' || chCheck == ',' || chCheck == '-' || chCheck == '(' || chCheck == ')')
                            strNew += chCheck;

                    return(decimal.Parse(strNew, System.Globalization.NumberStyles.Currency));
                }
                catch
                {
                }
            }

            return(0M);
        }
 
        /****************************************************************************/
        /// <summary>
        /// Converts an object to an float. If the string is not a valid float it will return zero.
        /// </summary>
        /// <param name="objValue">The object to convert</param>
        /// <returns>The resulting float</returns>
        public static float ToFloat(object objValue)
        {
            return(ToFloat(objValue, 0f));
        }

        /****************************************************************************/
        /// <summary>
        /// Converts an object to an float. If the string is not a valid float it will return the default value.
        /// </summary>
        /// <param name="objValue">The object to convert</param>
        /// <param name="iDefault">The default value to use if the object is not a valid integer</param>
        /// <returns>The resulting float</returns>
        public static float ToFloat(object objValue, float fDefault)
        {
            try
            {
                return(float.Parse(objValue.ToString()));
            }
            catch
            {
                return(fDefault);
            }
        }

        /****************************************************************************/
        /// <summary>
        /// Converts an object to an float. If the string is not a valid float it will return the default value.
        /// </summary>
        /// <param name="objValue">The object to convert</param>
        /// <param name="bThrow">Indicates whether to throw an exception if a parsing error occurs</param>
        /// <returns>The resulting float</returns>
        public static float ToFloat(object objValue, bool bThrow)
        {
            try
            {
                if(objValue == null)
                    return(0f);

                if(objValue.ToString().Trim() == "")
                    return(0f);

                return(float.Parse(objValue.ToString()));
            }
            catch
            {
                if(bThrow)
                    throw;

                return(0f);
            }
        }

        /****************************************************************************/
        public static uint ToUint(string strValue)
        {
            if(strValue != null && strValue.Trim() != "")
            {
                try
                {
                    return(uint.Parse(strValue));
                }
                catch
                {
                }
            }

            return(0);
        }
        
        /****************************************************************************/
        /// <summary>
        /// Converts an object to an uint. If the string is not a valid integer it will return zero.
        /// </summary>
        /// <param name="objValue">The object to convert</param>
        /// <returns>The resulting value</returns>
        public static uint ToUint(object objValue)
        {
            return(ToUint(objValue, 0));
        }

        /****************************************************************************/
        /// <summary>
        /// Converts an object to an uint. If the string is not a valid integer it will return the default value.
        /// </summary>
        /// <param name="objValue">The object to convert</param>
        /// <param name="iDefault">The default value to use if the object is not a valid uint</param>
        /// <returns>The resulting value</returns>
        public static uint ToUint(object objValue, uint iDefault)
        {
            try
            {
                return (uint.Parse(objValue.ToString()));
            }
            catch
            {
                return (iDefault);
            }
        }

        /****************************************************************************/
        public static string RemoveUpTo(string strValue, string strRemove)
        {
            int iIndex = strValue.IndexOf(strRemove);

            if(iIndex != -1)
                return(strValue.Remove(0, iIndex + strRemove.Length));
            
            return(strValue);
        }
        
        /****************************************************************************/
        public static string Pack(StringCollection aStrings)
        {
            string strPacked = "";

            foreach(string str in aStrings)
            {
                if(strPacked.Length > 0)
                    strPacked += "//";

                strPacked += str;
            }
   
            return(strPacked);
        }

        /****************************************************************************/
        public static IComparable ToComparable(object objComparable)
        {
            if(objComparable is IComparable)
                return(objComparable as IComparable);
            
            return(objComparable.ToString());
        }

        /****************************************************************************/
        public static string ToString(byte[] pData)
        {
            try
            {
                ASCIIEncoding objEncoder = new ASCIIEncoding();
                string strValue = objEncoder.GetString(pData);
                int iIndex = 0;

                while(iIndex < strValue.Length)
                {
                    char chValue = strValue[iIndex];

                    // ??? This filters out characters it shouldn't
                    if(char.IsLetterOrDigit(chValue) || char.IsPunctuation(chValue) || char.IsWhiteSpace(chValue) || char.IsSeparator(chValue))
                    {
                        ++iIndex;
                        continue;
                    }

                    strValue = strValue.Remove(iIndex, 1);
                }

                return(strValue);
            }
            catch
            {
            }

            return("");
        }
                
        /****************************************************************************/
        public static MemoryStream StreamToStream(Stream objFrom)
        {
            if(objFrom is MemoryStream)
                return(objFrom as MemoryStream);

            const int    kBufferSize  = 65536;
            int          nRead        = 0;
            byte[]       aBuffer      = new byte[kBufferSize];
            MemoryStream objStream    = new MemoryStream();
            
            try
            {
                // Reset the stream
                if(objStream.CanSeek)
                    objStream.Seek(0, SeekOrigin.Begin);

                nRead = objFrom.Read(aBuffer, 0, kBufferSize);
                
                while(nRead > 0)
                {
                    objStream.Write(aBuffer, 0, nRead);
                    nRead = objFrom.Read(aBuffer, 0, kBufferSize);
                }
            }
            catch(Exception ex)
            {
                objStream.Dispose();
                objStream = null;

                throw ex;
            }
            finally
            {
                // Reset the source stream
                if(objFrom.CanSeek)
                    objFrom.Seek(0, SeekOrigin.Begin);
            }

            objStream.Position = 0;
            
            return(objStream);
        }

        /****************************************************************************/
        public static async Task<MemoryStream> StreamToStreamAsync(Stream objFrom)
        {
            if(objFrom is MemoryStream)
                return(objFrom as MemoryStream);

            const int    kBufferSize  = 65536;
            int          nRead        = 0;
            byte[]       aBuffer      = new byte[kBufferSize];
            MemoryStream objStream    = new MemoryStream();
            
            try
            {
                // Reset the stream
                if(objStream.CanSeek)
                    objStream.Seek(0, SeekOrigin.Begin);

                nRead = await objFrom.ReadAsync(aBuffer, 0, kBufferSize);
                
                while(nRead > 0)
                {
                    await objStream.WriteAsync(aBuffer, 0, nRead);
                    nRead = await objFrom.ReadAsync(aBuffer, 0, kBufferSize);
                }
            }
            catch(Exception ex)
            {
                objStream.Dispose();
                objStream = null;

                throw ex;
            }
            finally
            {
                // Reset the source stream
                if(objFrom.CanSeek)
                    objFrom.Seek(0, SeekOrigin.Begin);
            }

            objStream.Position = 0;
            
            return(objStream);
        }

        /****************************************************************************/
        public static string StreamToString(System.IO.Stream objBuffer)
        {
            StreamReader objReader = new StreamReader(objBuffer);   
       
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
        public static MemoryStream StringToStream(string strValue)
        {
            return(StringToStream(strValue, new ASCIIEncoding()));
        }

        /****************************************************************************/
        public static MemoryStream StringToStream(string strValue, Encoding objEncoder)
        {
            MemoryStream  objStream  = new MemoryStream();   
            byte[]        pData      = objEncoder.GetBytes(strValue);
       
            objStream.Write(pData, 0, pData.Length);
            objStream.Position = 0;

            return(objStream);
        }

        /****************************************************************************/
        public static void Dispose(object objData)
        {
            if(objData != null && objData is IDisposable)
            {
                try
                {
                    (objData as IDisposable).Dispose();
                }
                catch
                {
                }
            }
        }

        /****************************************************************************/
        public static void Swap(ref int v1, ref int v2)
        {
            int temp = v1;

            v1 = v2;
            v2 = temp;

            return;
        }

        /****************************************************************************/
        public static bool InException
        {
            get
            {
                return(Marshal.GetExceptionPointers() != IntPtr.Zero || Marshal.GetExceptionCode() != 0);             
            }
        }

        /****************************************************************************/
        public static decimal Pin(decimal dValue, decimal dMin, decimal dMax)
        {
            return((dValue < dMin) ? dMin : ((dValue > dMax) ? dMax : dValue));
        }

        /****************************************************************************/
        public static double Pin(double dValue, double dMin, double dMax)
        {
            return((dValue < dMin) ? dMin : ((dValue > dMax) ? dMax : dValue));
        }

        /****************************************************************************/
        public static int Pin(int iValue, int iMin, int iMax)
        {
            return((iValue < iMin) ? iMin : ((iValue > iMax) ? iMax : iValue));
        }

        /****************************************************************************/
        public static long Pin(long iValue, long iMin, long iMax)
        {
            return((iValue < iMin) ? iMin : ((iValue > iMax) ? iMax : iValue));
        }

        /****************************************************************************/
        public static short Pin(short iValue, short iMin, short iMax)
        {
            return((iValue < iMin) ? iMin : ((iValue > iMax) ? iMax : iValue));
        }

        #region Date/Time Functions

        /****************************************************************************/
        public static DateTime TruncateSeconds(DateTime dtValue)
        {
            return(new DateTime(dtValue.Year, dtValue.Month, dtValue.Day, dtValue.Hour, dtValue.Minute, 0));
        }

        /****************************************************************************/
        public static DateTime TruncateMinutes(DateTime dtValue)
        {
            return(new DateTime(dtValue.Year, dtValue.Month, dtValue.Day, dtValue.Hour, 0, 0));
        }

        #endregion

        /****************************************************************************/
        public static byte[] XOR(byte[] a1, byte[] a2)
        {
            int iLength = a1.Length;
            byte[] aResults = new byte[a1.Length];

            for(int i = 0; i < iLength; ++i)
                aResults[i] = (byte)(a1[i] ^ a2[i]);

            return(aResults);
        }

        /****************************************************************************/
        public static byte[] XOR(byte[] a1, byte[] a2, byte[] a3, byte[] a4)
        {
            byte[] p1 = XOR(a1, a2);
            byte[] p2 = XOR(a3, a4);
            byte[] p3 = XOR(p1, p2);

            p1.Clear();
            p2.Clear();

            return(p3);
        }

       /****************************************************************************/
        private Utility()
        {
        }       
    }

    /****************************************************************************/
    /****************************************************************************/
    public sealed class Flag
    {
        /****************************************************************************/
        public static bool Test(int uFlags, int uFlag)
        {
            return((uFlags & uFlag) == uFlag);
        }

        /****************************************************************************/
        public static bool Test(long uFlags, long uFlag)
        {
            return((uFlags & uFlag) == uFlag);
        }

        /****************************************************************************/
        public static bool Test(uint uFlags, uint uFlag)
        {
            return((uFlags & uFlag) == uFlag);
        }

        /****************************************************************************/
        public static bool Test(byte uFlags, byte uFlag)
        {
            return((uFlags & uFlag) == uFlag);
        }

        /****************************************************************************/
        public static uint Clear(uint uFlags, uint uFlag)
        {
            return(uFlags & (~uFlag));
        }

        /****************************************************************************/
        public static int Clear(int uFlags, int uFlag)
        {
            return(uFlags & (~uFlag));
        }

        /****************************************************************************/
        public static long Clear(long uFlags, long uFlag)
        {
            return(uFlags & (~uFlag));
        }

        /****************************************************************************/
        public static short Clear(short uFlags, short uFlag)
        {
            return((short)((int)uFlags & (~((int)uFlag))));
        }

        /****************************************************************************/
        public static uint Set(uint uFlags, uint uFlag, bool bSet)
        {
            return(Clear(uFlags, uFlag) | ((bSet ? uFlag : 0)));
        }
         
        /****************************************************************************/
        public static long Set(long uFlags, long uFlag, bool bSet)
        {
            return(Clear(uFlags, uFlag) | ((bSet ? uFlag : 0)));
        }
         
        /****************************************************************************/
        public static short Set(short sFlags, short sFlag, bool bSet)
        {
            long iFlags = sFlags;
            long iFlag = sFlag;

            return((short)Set(iFlags, iFlag, bSet));
        }
         
        /****************************************************************************/
        private Flag()
        {
        }
    }
}
