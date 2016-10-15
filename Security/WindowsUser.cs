/****************************************************************************/
/*                                                                          */
/*           Module: Mondo.Security         				                */
/*             File: WindowsUser.cs								            */
/*        Class(es): WindowsUser								  	        */
/*          Purpose: A Windows User                                         */
/*                                                                          */
/*  Original Author: Jim Lightfoot                                          */
/*    Creation Date: 30 Jun 2013                                            */
/*                                                                          */
/*   Copyright (c) 2013 - Zoomla Corp,  All rights reserved                 */
/*                                                                          */
/****************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Principal;
using System.Runtime.InteropServices;

using Mondo.Common;

namespace Mondo.Security
{
    /****************************************************************************/
    /****************************************************************************/
    public class WindowsUser : Openable
    {
        private static Stack<string> s_aUserNames = new Stack<string>();
        private readonly Stack<WindowsImpersonationContext> m_aTokens = new Stack<WindowsImpersonationContext>();

        /****************************************************************************/
        public WindowsUser(string strUserName, string strPassword) : this(Environment.MachineName, strUserName, strPassword)
        {
        }

        /****************************************************************************/
        public WindowsUser(string strDomainName, string strUserName, string strPassword)
        {
            this.DomainName = strDomainName;
            this.UserName   = strUserName;
            this.Password   = strPassword;
        }

        /****************************************************************************/
        public string DomainName
        {
            get; private set;
        }

        /****************************************************************************/
        public string UserName
        {
            get; private set;
        }

        /****************************************************************************/
        protected string Password
        {
            get; set;
        }

        /****************************************************************************/
        public static string CurrentUserName 
        {
            get
            {
                if(s_aUserNames.Count == 0)
                    return("");

                return(s_aUserNames.Peek());
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct PROFILEINFO
        {
            ///
            /// Specifies the size of the structure, in bytes.
            ///
            public int dwSize;
    
            ///
            /// This member can be one of the following flags: 
            /// PI_NOUI or PI_APPLYPOLICY
            ///
            public int dwFlags;
    
            ///
            /// Pointer to the name of the user.
            /// This member is used as the base name of the directory 
            /// in which to store a new profile.
            ///
            public string lpUserName;
    
            ///
            /// Pointer to the roaming user profile path.
            /// If the user does not have a roaming profile, this member can be NULL.
            ///
            public string lpProfilePath;
    
            ///
            /// Pointer to the default user profile path. This member can be NULL.
            ///
            public string lpDefaultPath;
    
            ///
            /// Pointer to the name of the validating domain controller, in NetBIOS format.
            /// If this member is NULL, the Windows NT 4.0-style policy will not be applied.
            ///
            public string lpServerName;
    
            ///
            /// Pointer to the path of the Windows NT 4.0-style policy file. 
            /// This member can be NULL.
            ///
            public string lpPolicyPath;
    
            ///
            /// Handle to the HKEY_CURRENT_USER registry key.
            ///
            public IntPtr hProfile;
        }

        /****************************************************************************/
        [DllImport("advapi32.dll")]
        private static extern bool LogonUser(string lpszUserName, string lpszDomain, string lpszPassword, int dwLogonType,int dwLogonProvider, ref IntPtr phToken);

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int DuplicateToken(IntPtr hToken, int impersonationLevel, ref IntPtr hNewToken);

        [DllImport("userenv.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern bool LoadUserProfile(IntPtr hToken, ref PROFILEINFO lpProfileInfo);
        
        /****************************************************************************/
        public override void Open()
        {
            if(s_aUserNames.Count > 0 && s_aUserNames.Peek() == this.UserName)
            {
                m_aTokens.Push(null);
                base.Open();
                return;
            }

            IntPtr    iToken                = IntPtr.Zero;
            IntPtr    tokenDuplicate        = IntPtr.Zero;
            const int SecurityImpersonation = 2;

            if(LogonUser(this.UserName, this.DomainName, this.Password, 3, 0, ref iToken))
            {
                if(DuplicateToken(iToken, SecurityImpersonation, ref tokenDuplicate) != 0)
                {
                    m_aTokens.Push(WindowsIdentity.Impersonate(iToken));

                    s_aUserNames.Push(this.UserName);

                    #region LoadUserProfile

                    // Load user profile
                    PROFILEINFO profileInfo = new PROFILEINFO();

                    profileInfo.dwSize     = Marshal.SizeOf(profileInfo);
                    profileInfo.lpUserName = this.UserName;
                    profileInfo.dwFlags    = 1;

                    Boolean loadSuccess = LoadUserProfile(tokenDuplicate, ref profileInfo);

                    if (!loadSuccess)
                        throw new Exception("LoadUserProfile() failed with error code: " + Marshal.GetLastWin32Error());

                    if (profileInfo.hProfile == IntPtr.Zero)
                        throw new Exception("LoadUserProfile() failed - HKCU handle was not loaded. Error code: " + Marshal.GetLastWin32Error());

                    #endregion

                    base.Open();
                }
            }
            else
                throw new Exception("Invalid password");
        }

        /****************************************************************************/
        public override void Close()
        {
            base.Close();

            WindowsImpersonationContext objContext = m_aTokens.Pop();

            if(objContext != null)
            {
                objContext.Undo();
                objContext.Dispose();
                objContext = null;
                s_aUserNames.Pop();
            }
        }
    }
}
