/****************************************************************************/
/*                                                                          */
/*    The Mondo Libraries  							                        */
/*                                                                          */
/*        Namespace: Mondo.Common							                */
/*             File: CachedExceptionHandler.cs							    */
/*        Class(es): CachedExceptionHandler                                 */
/*          Purpose: Caches exceptions and coalesces duplicate messages to  */
/*                      be sent at certain intervals so as not to flood     */
/*                      email systems.                                      */
/*                                                                          */
/*  Original Author: Jim Lightfoot                                          */
/*    Creation Date: 24 Aug 2008                                            */
/*                                                                          */
/*   Copyright (c) 2008 - Jim Lightfoot, All rights reserved                */
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
        private readonly Dictionary<string, Error> m_aExceptions = new Dictionary<string,Error>(137);
        
        private const int kExpiration = 60;  // Expires cached error after "n" minutes
        private const int kSendNotice = 5;   // Sends error message every "n" minutes

        private readonly object m_objLock = new System.Object();
        
        /*********************************************************************/
        internal ExceptionCache()
        {
        }
        
        /*********************************************************************/
        internal bool Add(ref Exception objException)
        {
          #if xDEBUG
            return(true);
          #else
            string strKey = ToHash(objException);

            lock(m_objLock)
            {
                if(m_aExceptions.ContainsKey(strKey))
                {
                    Error    objError = m_aExceptions[strKey];
                    DateTime dtNow    = DateTime.Now;
                    
                    // If the cached error is more than 1 hour old then remove it and start over
                    if(objError.Created < dtNow.AddMinutes(-kExpiration))
                    {
                        m_aExceptions.Remove(strKey);
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
                
                m_aExceptions.Add(strKey, objNewError);
            }
          #endif

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
            private Exception m_objException;
            private int       m_iCount = 1;
            private DateTime  m_dtCreated;
            private DateTime  m_dtLastSent;

            /*********************************************************************/
            internal Error(Exception ex)
            {
                m_objException = ex;
                m_dtCreated    = m_dtLastSent = DateTime.Now;
            }
            
            /*********************************************************************/
            internal Exception Exception
            {
              get {return(m_objException);}
            }
            
            /*********************************************************************/
            internal DateTime Created
            {
                get {return(m_dtCreated);}
            }
            
            /*********************************************************************/
            internal int Count
            {
                get {return(m_iCount);}
            }
            
            /*********************************************************************/
            internal DateTime LastSent
            {
                set {m_dtLastSent = value;}
                get {return(m_dtLastSent);}
            }
            
            /*********************************************************************/
            internal void IncrementCount()
            {
                ++m_iCount;
            }
            
            /*********************************************************************/
            internal void ResetCount()
            {
                m_iCount = 0;
            }
        }       
    }
}
