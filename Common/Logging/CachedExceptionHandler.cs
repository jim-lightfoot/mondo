/****************************************************************************/
/*                                                                          */
/*    The Mondo Libraries  						    */
/*                                                                          */
/*        Namespace: Mondo.Common					    */
/*             File: CachedExceptionHandler.cs				    */
/*        Class(es): CachedExceptionHandler                                 */
/*          Purpose: Caches exceptions and coalesces duplicate messages to  */
/*                      be sent at certain intervals so as not to flood     */
/*                      loggin systems.                                     */
/*                                                                          */
/*  Original Author: Jim Lightfoot                                          */
/*    Creation Date: 24 Aug 2008                                            */
/*                                                                          */
/*   Copyright (c) 2008-2017 - Jim Lightfoot, All rights reserved           */
/*                                                                          */
/*  Licensed under the MIT license:                                         */
/*    http://www.opensource.org/licenses/mit-license.php                    */
/*                                                                          */
/****************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using Mondo.Common;

namespace Mondo.Common
{
    /*********************************************************************/
    /*********************************************************************/
    public interface IExceptionHandler 
    {
        string FormatMessage(XmlDocument xmlMessage, string strAppName);
        void SendMessage(string strMessage);
    }

    /*********************************************************************/
    /*********************************************************************/
    internal abstract class ExceptionHandler : IExceptionHandler
    {
        /*********************************************************************/
        internal ExceptionHandler()
        {
        }

        /*********************************************************************/
        public abstract string FormatMessage(XmlDocument xmlMessage, string strAppName);

        /*********************************************************************/
        public abstract void SendMessage(string strMessage);
    }

    /*********************************************************************/
    /*********************************************************************/
    internal class ExceptionCache 
    {
        private readonly Dictionary<string, Error> _aExceptions = new Dictionary<string,Error>(137);
        
        private const int kExpiration = 60;  // Expires cached error after "n" minutes
        private const int kSendNotice = 5;   // Sends error message every "n" minutes

        private readonly object _objLock = new System.Object();
        
        /*********************************************************************/
        internal ExceptionCache()
        {
        }
        
        /*********************************************************************/
        internal bool Add(ref Exception objException)
        {
            string strKey = ToHash(objException);

            lock(_objLock)
            {
                if(_aExceptions.ContainsKey(strKey))
                {
                    Error    objError = _aExceptions[strKey];
                    DateTime dtNow    = DateTime.Now;
                    
                    // If the cached error is more than 1 hour old then remove it and start over
                    if(objError.Created < dtNow.AddMinutes(-kExpiration))
                    {
                        _aExceptions.Remove(strKey);
                    }
                    // If it's been more than 5 minutes since we last sent the error then send it again
                    else if(objError.LastSent < dtNow.AddMinutes(-kSendNotice))                  
                    {
                        objError.IncrementCount();
                        
                        // Send error with count
                        TimeSpan tsDiff     = dtNow - objError.Created;
                        string   strMessage = string.Format("The following errors were received {0} times in the last {1} minutes", objError.Count, (int)Math.Floor(tsDiff.TotalMinutes));
                        
                        objException = new Exception(strMessage, objException);
                        objError.LastSent = dtNow;
                        
                        return(true);
                    }
                    else
                    {
                        objError.IncrementCount();
                        return(false);
                    }                        
                }

                Error objNewError = new Error(objException);
                
                _aExceptions.Add(strKey, objNewError);
            }

            return(true);
        }
        
        /*********************************************************************/
        private static string ToHash(Exception objException)
        {
            StringBuilder sbKey    = new StringBuilder();
            Exception     objInner = objException;
            
            while(objInner != null)
            {
                sbKey.Append(objInner.Message.ToLower().Replace(" ", ""));
                
                if(objInner.InnerException == null && objInner.Source != null)
                    sbKey.Append(objInner.Source.ToLower().Replace(" ", ""));
                    
                objInner = objInner.InnerException;
            }
            
            return(sbKey.ToString().GetHashCode().ToString());
        }

        /*********************************************************************/
        /*********************************************************************/
        private class Error
        {
            private Exception _objException;
            private int       _iCount = 1;
            private DateTime  _dtCreated;
            private DateTime  _dtLastSent;

            /*********************************************************************/
            internal Error(Exception ex)
            {
                _objException = ex;
                _dtCreated    = _dtLastSent = DateTime.Now;
            }
            
            /*********************************************************************/
            internal Exception Exception
            {
              get {return(_objException);}
            }
            
            /*********************************************************************/
            internal DateTime Created
            {
                get {return(_dtCreated);}
            }
            
            /*********************************************************************/
            internal int Count
            {
                get {return(_iCount);}
            }
            
            /*********************************************************************/
            internal DateTime LastSent
            {
                set {_dtLastSent = value;}
                get {return(_dtLastSent);}
            }
            
            /*********************************************************************/
            internal void IncrementCount()
            {
                ++_iCount;
            }
            
            /*********************************************************************/
            internal void ResetCount()
            {
                _iCount = 0;
            }
        }       
    }
}
