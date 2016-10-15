/****************************************************************************/
/*                                                                          */
/*    The Mondo Libraries  							                        */
/*                                                                          */
/*        Namespace: Mondo.Common							                */
/*             File: Email.cs								                */
/*        Class(es): Email				                                    */
/*          Purpose: An email utility class                                 */
/*                                                                          */
/*  Original Author: Jim Lightfoot                                          */
/*    Creation Date: 12 Nov 2005                                            */
/*                                                                          */
/*   Copyright (c) 2005-2016 - Jim Lightfoot, All rights reserved           */
/*                                                                          */
/*  Licensed under the MIT license:                                         */
/*    http://www.opensource.org/licenses/mit-license.php                    */
/*                                                                          */
/****************************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Diagnostics;

namespace Mondo.Common
{
    /****************************************************************************/
    /****************************************************************************/
    public interface IEmailServer : IDisposable
    {
        void Send(Email email);
    }

    /****************************************************************************/
    /****************************************************************************/
    public class SmtpServer : IEmailServer
    {
        /****************************************************************************/
        public SmtpServer(string strServer) 
        {
            //m_objClient = new SmtpClient(strServer);
        }

        /****************************************************************************/
        public SmtpServer() 
        {
            /*
             *   <system.net>
                    <mailSettings>
                      <smtp>
                        <network port="25" host="localhost" />
                            ...or...
                        <network port="25" host="mail.companyname.com" />
                      </smtp>
                    </mailSettings>
                  </system.net>
             */
        }       

        /****************************************************************************/
        public SmtpServer(string strServer, int iPort, string strUserName, string strPassword) : this(strServer)
        {
           // m_objClient.Credentials = new NetworkCredential(strUserName, strPassword);

          //  if(iPort != 0)
           //     m_objClient.Port = iPort;
        }

        /****************************************************************************/
        protected SmtpServer(SmtpClient client)
        {
          //  m_objClient = client;

         //   if(m_objClient.Host.ToLower().Contains("local"))
          //      m_objClient.DeliveryMethod = SmtpDeliveryMethod.PickupDirectoryFromIis;
        }

        /****************************************************************************/
        public void Send(Email email)
        {
            using(SmtpClient client = new SmtpClient())
                client.Send(email.InternalMessage);
        }

        /****************************************************************************/
        public void Dispose()
        {
            //if(m_objClient != null)
            { 
            //    m_objClient.Dispose();
             //   m_objClient = null;
            }
        }
    }

    /****************************************************************************/
    /****************************************************************************/
    public class Email : IDisposable
    {
        private readonly MailMessage  m_objMessage = new MailMessage();
        private readonly IEmailServer m_mailServer;
        private readonly bool         m_disposeMailServer;

        /****************************************************************************/
        public Email(IEmailServer mailServer = null)
        {
            if(mailServer == null)
            {
                m_mailServer = new SmtpServer();
                m_disposeMailServer = true;
            }
            else
            {
                m_mailServer = mailServer;
                m_disposeMailServer = false;
            }
        }

        /****************************************************************************/
        /****************************************************************************/
        public class InvalidEmail : Exception
        {
            /****************************************************************************/
            public InvalidEmail(string msg) : base(msg)
            {
            }
        }

        /****************************************************************************/
        public void Dispose()
        {
            try
            {
                m_objMessage.Dispose();
            }
            catch
            {
            }

            try
            {
                if(m_disposeMailServer && m_mailServer != null)
                    m_mailServer.Dispose();
            }
            catch
            {
            }
        }

        /****************************************************************************/
        internal MailMessage InternalMessage
        {
            get { return(m_objMessage); }
        }

        /****************************************************************************/
        public string From       
        {
            get
            {
                return(m_objMessage.From.Address);
            }

            set
            {
                string strDisplay = "";
                string strEmail   = ExtractAddress(value, ref strDisplay);

                if(strDisplay != "")
                {
                    m_objMessage.From = new MailAddress(strEmail, strDisplay);
                    return;
                }

                m_objMessage.From = new MailAddress(strEmail);
            }
        }

        /****************************************************************************/
        public static string ExtractAddress(string strValue, ref string strDisplay)
        {        
            string strEmail = strValue.Trim();
            strDisplay = "";
                
            if(!SplitAddress(ref strEmail, ref strDisplay, "[", "]"))
                SplitAddress(ref strEmail, ref strDisplay, "<", ">");

            strEmail = strEmail.Replace("\"", "");

            return(strEmail);
        }

        /****************************************************************************/
        public string To       
        {
            get
            {
                return(new StringList(m_objMessage.To).Pack("; "));
            }

            set
            {
                m_objMessage.To.Clear();

                AddRecipient(value);
            }
        }

        /****************************************************************************/
        public string CC
        {
            get
            {
                return(new StringList(m_objMessage.CC).Pack("; "));
            }
        }

        /****************************************************************************/
        public string BCC
        {
            get
            {
                return(new StringList(m_objMessage.Bcc).Pack("; "));
            }
        }

        /****************************************************************************/
        public void AddRecipient(string strRecipientList)       
        {
            _AddRecipient(strRecipientList, m_objMessage.To);
        }

        /****************************************************************************/
        public void AddCC(string strRecipientList)       
        {
            _AddRecipient(strRecipientList, m_objMessage.CC);
        }

        /****************************************************************************/
        public void AddBCC(string strRecipientList)       
        {
            _AddRecipient(strRecipientList, m_objMessage.Bcc);
        }

        /****************************************************************************/
        public IEnumerable Recipients       
        {
            set
            {
                AddRecipients(value);
            }
        }

        /****************************************************************************/
        public void AddRecipients(IEnumerable aRecipients)       
        {
            foreach(object objRecipient in aRecipients)
                AddRecipient(objRecipient.ToString());

            return;
        }

        /****************************************************************************/
        public string Subject     
        {
            get {return(m_objMessage.Subject);}
            set {m_objMessage.Subject = value;}
        }

        /****************************************************************************/
        public string Body
        {
            get {return(m_objMessage.Body);}
            set {m_objMessage.Body = value;}
        }

        /****************************************************************************/
        public bool IsBodyHtml  
        {
            get {return(m_objMessage.IsBodyHtml);}
            set { m_objMessage.IsBodyHtml = value; }
        }
        public AttachmentCollection Attachments {get{return(m_objMessage.Attachments);}}

        /****************************************************************************/
        public void AddAttachment(Stream objStream, string strName)      
        {
            Attachment objAttachment = new Attachment(objStream, strName, System.Net.Mime.MediaTypeNames.Application.Octet);

            this.Attachments.Add(objAttachment);

            return;
        }

        /****************************************************************************/
        public string Description     
        {
            get
            {
                StringList   aTo = new StringList();
                StringBuilder sb = new StringBuilder();

                foreach(MailAddress objAddress in m_objMessage.To)
                    aTo.Add(DisplayName(objAddress));

                sb.Append("From: ");
                sb.AppendLine(DisplayName(m_objMessage.From));
                sb.Append("To: ");
                sb.AppendLine(aTo.Pack(", "));
                sb.Append("Subject: ");
                sb.AppendLine(m_objMessage.Subject);
                sb.AppendLine("");
                sb.AppendLine(m_objMessage.Body);

                return(sb.ToString());
            }
        }

        /****************************************************************************/
        public void Send()
        {
            this.m_mailServer.Send(this);
        }

        /****************************************************************************/
        public static bool IsValidAddress(string strRawEmail)
        {
            string strEmail = strRawEmail.Normalized();

            if(strEmail.Length < 5 || strEmail.Length > 254)
                return(false);

            int iLength = strEmail.Length;
            int iAtIndex = strEmail.IndexOf("@");

            if(iAtIndex < 1 || iAtIndex > iLength - 5)
                return(false);

            // Only one "@" allowed
            if(strEmail.Replace("@", "").Length != iLength-1)
                return(false);

            if(strEmail.IndexOf("..") != -1)
                return(false);

            StringList aParts = new StringList(strEmail, "@", true);

            if(aParts.Count != 2)
                return(false);

            if(aParts[0].Length > 64)
                return(false);

            int iDotIndex = strEmail.IndexOf(".");

            if(iDotIndex < 1 || (iDotIndex == iLength - 1))
                return(false);

            int iHypens = strEmail.IndexOf("--");

            if(iHypens != -1)
                return(false);

            if(!IsAllowed(aParts[0], ".!#$%&'*+-/=?^_`{|}~"))
                return(false);

            if(!IsAllowed(aParts[1], ".-"))
                return(false);

            try
            {
                new MailAddress(strEmail);
            }
            catch
            {
                return(false);
            }

            return(true);
        }

        #region Private Methods

        /****************************************************************************/
        private static bool SplitAddress(ref string strEmail, ref string strDisplay, string strBegin, string strEnd)
        {        
            if(strEmail.Contains(strEnd))
            {
                strDisplay = strEmail.StripAfter(strBegin).Replace(strBegin, "").Replace(strEnd, "").Trim();
                strEmail   = strEmail.StripUpTo(strBegin).Replace(strBegin, "").Replace(strEnd, "").Trim();

                return(true);
            }

            return(false);
        }

        /****************************************************************************/
        private static string DisplayName(MailAddress objAddress)
        {
            if(objAddress != null)
            {
                string strAddress = objAddress.DisplayName;

                if(strAddress == "")
                    strAddress = objAddress.Address;

                return(strAddress);
            }

            return("");
        }

        /****************************************************************************/
        private static void _AddRecipient(string strRecipientList, MailAddressCollection aAddresses)       
        {
            string strOverride = Config.Get("EmailOverride");

            if(strOverride != "")
            {
                if(aAddresses.Count == 0)
                    aAddresses.Add(new MailAddress(strOverride));
            }
            else
            { 
                StringList aRecipients = new StringList(strRecipientList.Normalized().Replace(",", ";").Replace(" ", ";").Replace("\r\n", ";").Replace("\r", ";").Replace("\n", ";"), ";", true);

                foreach(string strRecipient in aRecipients)
                {
                    MailAddress objAddress = null;

                    try
                    {
                        objAddress = new MailAddress(strRecipient);
                    }
                    catch(Exception ex)
                    {
                        throw new Exception("Invalid email address: " + strRecipient, ex);
                    }

                    aAddresses.Add(objAddress);
                }
            }

            return;
        }

        /****************************************************************************/
        private static bool IsAllowed(string strValue, string strAllowable)
        {
            int iIndex = 0;

            foreach(char chValue in strValue)
            {
                if(char.IsLetterOrDigit(chValue))
                    return(true);

                if(iIndex != 0)
                    return(false);

                foreach(char chAllowed in strAllowable)
                {
                    if(chValue == chAllowed)
                        goto OK;
                }

                return(false);

               OK:
                ++iIndex;
                continue;
            }

            return(true);
        }

        #endregion
    }
}
