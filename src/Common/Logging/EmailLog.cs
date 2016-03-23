/****************************************************************************/
/*                                                                          */
/*    The Mondo Libraries  							                        */
/*                                                                          */
/*        Namespace: Mondo.Common							                */
/*             File: ExceptionEmail.cs								        */
/*        Class(es): ExceptionEmail									        */
/*          Purpose: Send an exception email                                */
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
using System.Xml.Xsl;
using System.IO;
using System.Reflection;
using System.Diagnostics;
using System.Web;
using System.Net;
using System.Net.Mail;
using Mondo.Xml;
using Mondo.Xml.Xsl;

namespace Mondo.Common
{
    /*****************************************************************************/
    /*****************************************************************************/
    internal sealed class XsltResource
    {
        /****************************************************************************/
        private XsltResource()
        {
        }

        /****************************************************************************/
        internal static string GetString(string strName)
        {
            return(Utility.StreamToString(GetStream(strName)));
        }

        /****************************************************************************/
        internal static Stream GetStream(string strName)
        {
            System.IO.Stream objFile = Assembly.GetExecutingAssembly().GetManifestResourceStream("Mondo.Common.xslt." + strName);

            if(objFile == null)
                throw new ArgumentException("Resource not found: " + strName);

            return(objFile);
        }
    }    
    
    /****************************************************************************/
    /****************************************************************************/
    public abstract class TransformLog  
    {
        private XslCompiledTransform m_objTransform = null;

        /****************************************************************************/
        protected TransformLog(string strTransformName) 
        {
            string strXslt = XsltResource.GetString("ExceptionFormatters." + strTransformName + ".xslt");

            m_objTransform = Transform.FromString(strXslt);
        }

        /****************************************************************************/
        protected string FormatMessage(XmlDocument xmlMessage, string strAppName)
        {
            try
            {
                XsltArgumentList aArgs = new XsltArgumentList();

                aArgs.AddParam("APPNAME", "", strAppName);

                return(Transform.Run(m_objTransform, xmlMessage, aArgs));
            }
            catch(Exception ex)
            {
                cDebug.Capture(ex);
                return("");
            }
        }
    }

    /****************************************************************************/
    /****************************************************************************/
    public class EmailLog : TransformLog, ILog
    {
        private   string m_strAppName;
        protected string m_strFrom;
        protected string m_strTo;
        protected bool   IsHtml {get; set;}

         /****************************************************************************/
        public EmailLog(IConfig config = null) : base("html")
        {
            if(config == null)
                config = new AppConfig();

            m_strAppName = config.Get("ExceptionEmailAppName");
            m_strFrom    = config.Get("ExceptionEmailFrom");
            m_strTo      = config.Get("ExceptionEmailTo");
            IsHtml       = true;
       }

        /****************************************************************************/
        protected EmailLog(string strType) : base(strType)
        {
        }

        /****************************************************************************/
        private void SendMessage(string strMessage)
        {
            try
            {
                using(Email objEmail = new Email())
                {
                    objEmail.From       = m_strFrom;
                    objEmail.To         = m_strTo;
                    objEmail.Subject    = "An exception was thrown by " + m_strAppName;
                    objEmail.Body       = strMessage.Trim();
                    objEmail.IsBodyHtml = IsHtml;

                    objEmail.Send();
                }
            }
            catch(Exception ex)
            {
                cDebug.Capture(ex);
            }
        }

        /****************************************************************************/
        public void WriteError(Exception ex, Dictionary<string, string> properties)
        {
            XmlDocument xmlMessage = ex.ToXml();
            string      strMessage = FormatMessage(xmlMessage, m_strAppName);

            SendMessage(strMessage);
        }

        /****************************************************************************/
        public void WriteEvent(string eventName, Dictionary<string, string> properties, Dictionary<string, double> metrics)
        {
            // Don't send emails for these
        }

        /****************************************************************************/
        public void WriteMetric(string metricName, double value, Dictionary<string, string> properties)
        {
            // Don't send emails for these
        }

        /****************************************************************************/
        public void WriteTrace(string message, Log.SeverityLevel level, Dictionary<string, string> properties)
        {
            // Don't send emails for these
        }

        /****************************************************************************/
        public void WriteRequest(string name, DateTime startTime, TimeSpan duration, string responseCode, bool success)
        {
            // Don't send emails for these
        }
    }

    /*********************************************************************/
    /*********************************************************************/
    public static class ExceptionUtility
    {
        /*********************************************************************/
        public static XmlDocument ToXml(this Exception objException)
        {
            cXMLWriter objWriter = new cXMLWriter();
            Exception  objInner  = objException;

            using(objWriter.Acquire)
            {
                using(new XmlElementWriter(objWriter, "Exceptions"))
                {
                    while(objInner != null)
                    {
                        using(new XmlElementWriter(objWriter, "Exception"))
                        {
                            objWriter.WriteElementCDATA("Message",      objInner.Message);
                            objWriter.WriteElementString("Type",        objInner.GetType().ToString());
                            objWriter.WriteElementCDATA("Source",       objInner.Source.Normalized());
                            objWriter.WriteElementCDATA("StackTrace",   objInner.StackTrace.Normalized());
                        }

                        objInner = objInner.InnerException;
                    }

                    using(new XmlElementWriter(objWriter, "ServerInfo"))
                    {
                        objWriter.WriteElementString("Timestamp",       DateTime.Now.ToString());
                        objWriter.WriteElementString("MachineName",     Environment.MachineName.Normalized());
                        objWriter.WriteElementString("OS",              Environment.OSVersion.ToString());
                        objWriter.WriteElementString("ThreadIdentity",  Environment.UserName.Normalized());
                    }
                }
            }

            return(objWriter.Xml);
        }
    }
}
