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
using System.Threading.Tasks;

namespace Mondo.Common
{
    /****************************************************************************/
    /****************************************************************************/
    public interface IEmailServerFactory 
    {
        IEmailServer Create(string server = "", int iPort = 0, string userName = "", string password = "");
    }

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
        private SmtpClient _client = null;

        /****************************************************************************         
           // If no params
         
               <system.net>
                <mailSettings>
                    <smtp>
                    <network port="25" host="localhost" />
                        ...or...
                    <network port="25" host="mail.companyname.com" />
                    </smtp>
                </mailSettings>
                </system.net>
           
         ****************************************************************************/
        public SmtpServer(string server = "", int iPort = 0, string userName = "", string password = "") 
        {
            if(!string.IsNullOrWhiteSpace(server))
            {
                _client = new SmtpClient(server);

                if(!string.IsNullOrWhiteSpace(userName))
                    _client.Credentials = new NetworkCredential(userName, password);

                if(iPort != 0)
                    _client.Port = iPort;
            }
        }

        /****************************************************************************/
        public void Send(Email email)
        {
            if(_client != null)
                _client.Send(email.InternalMessage);
            else
            { 
                using(SmtpClient client = new SmtpClient())
                    client.Send(email.InternalMessage);
            }
        }

        /****************************************************************************/
        public void Dispose()
        {
            if(_client != null)
            { 
                _client.Dispose();
                _client = null;
            }
        }
    }

    /****************************************************************************/
    /****************************************************************************/
    public class Email : IDisposable
    {
        private readonly MailMessage  _message = new MailMessage();
        private readonly IEmailServer _mailServer;
        private readonly bool         _disposeMailServer;

        /****************************************************************************/
        public Email(IEmailServer mailServer = null)
        {
            if(mailServer == null)
            {
                _mailServer = new SmtpServer();
                _disposeMailServer = true;
            }
            else
            {
                _mailServer = mailServer;
                _disposeMailServer = false;
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
                _message.Dispose();
            }
            catch
            {
            }

            try
            {
                if(_disposeMailServer && _mailServer != null)
                    _mailServer.Dispose();
            }
            catch
            {
            }
        }

        /****************************************************************************/
        internal MailMessage InternalMessage
        {
            get { return(_message); }
        }

        /****************************************************************************/
        public string From       
        {
            get
            {
                return(_message.From.Address);
            }

            set
            {
                string strDisplay = "";
                string strEmail   = ExtractAddress(value, ref strDisplay);

                if(strDisplay != "")
                {
                    _message.From = new MailAddress(strEmail, strDisplay);
                    return;
                }

                _message.From = new MailAddress(strEmail);
            }
        }

        /****************************************************************************/
        public static string ExtractAddress(string value, ref string display)
        {        
            string emailAddress = value.Trim();
                   display = "";
                
            if(!SplitAddress(ref emailAddress, ref display, "[", "]"))
                SplitAddress(ref emailAddress, ref display, "<", ">");

            emailAddress = emailAddress.Replace("\"", "");

            return(emailAddress);
        }

        /****************************************************************************/
        public string To       
        {
            get
            {
                return(new StringList(_message.To).Pack("; "));
            }

            set
            {
                _message.To.Clear();

                AddRecipient(value);
            }
        }

        /****************************************************************************/
        public string CC
        {
            get
            {
                return(new StringList(_message.CC).Pack("; "));
            }
        }

        /****************************************************************************/
        public string BCC
        {
            get
            {
                return(new StringList(_message.Bcc).Pack("; "));
            }
        }

        /****************************************************************************/
        public void AddRecipient(string recipientList)       
        {
            _AddRecipient(recipientList, _message.To);
        }

        /****************************************************************************/
        public void AddCC(string recipientList)       
        {
            _AddRecipient(recipientList, _message.CC);
        }

        /****************************************************************************/
        public void AddBCC(string recipientList)       
        {
            _AddRecipient(recipientList, _message.Bcc);
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
            foreach(object recipient in aRecipients)
                AddRecipient(recipient.ToString());

            return;
        }

        /****************************************************************************/
        public string Subject     
        {
            get {return(_message.Subject);}
            set {_message.Subject = value;}
        }

        /****************************************************************************/
        public string Body
        {
            get {return(_message.Body);}
            set {_message.Body = value;}
        }

        /****************************************************************************/
        public bool IsBodyHtml  
        {
            get {return(_message.IsBodyHtml);}
            set { _message.IsBodyHtml = value; }
        }
        public AttachmentCollection Attachments {get{return(_message.Attachments);}}

        /****************************************************************************/
        public void AddAttachment(Stream objStream, string strName)      
        {
            Attachment attachment = new Attachment(objStream, strName, System.Net.Mime.MediaTypeNames.Application.Octet);

            this.Attachments.Add(attachment);

            return;
        }

        /****************************************************************************/
        public string Description     
        {
            get
            {
                StringList    aTo = new StringList();
                StringBuilder sb = new StringBuilder();

                foreach(MailAddress mailAddress in _message.To)
                    aTo.Add(DisplayName(mailAddress));

                sb.Append("From: ");
                sb.AppendLine(DisplayName(_message.From));
                sb.Append("To: ");
                sb.AppendLine(aTo.Pack(", "));
                sb.Append("Subject: ");
                sb.AppendLine(_message.Subject);
                sb.AppendLine("");
                sb.AppendLine(_message.Body);

                return(sb.ToString());
            }
        }

        /****************************************************************************/
        public void Send()
        {
            this._mailServer.Send(this);
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
        private static string DisplayName(MailAddress mailAddress)
        {
            if(mailAddress != null)
            {
                string strAddress = mailAddress.DisplayName;

                if(strAddress == "")
                    strAddress = mailAddress.Address;

                return(strAddress);
            }

            return("");
        }

        /****************************************************************************/
        private static void _AddRecipient(string recipientList, MailAddressCollection aAddresses)       
        {
            string strOverride = Config.Get("EmailOverride");

            if(strOverride != "")
            {
                if(aAddresses.Count == 0)
                    aAddresses.Add(new MailAddress(strOverride));
            }
            else
            { 
                StringList aRecipients = new StringList(recipientList.Normalized().Replace(",", ";").Replace(" ", ";").Replace("\r\n", ";").Replace("\r", ";").Replace("\n", ";"), ";", true);

                foreach(string strRecipient in aRecipients)
                {
                    MailAddress mailAddress = null;

                    try
                    {
                        mailAddress = new MailAddress(strRecipient);
                    }
                    catch(Exception ex)
                    {
                        throw new Exception("Invalid email address: " + strRecipient, ex);
                    }

                    aAddresses.Add(mailAddress);
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
