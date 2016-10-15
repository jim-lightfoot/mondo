/****************************************************************************/
/*                                                                          */
/*    The Mondo Libraries  							                        */
/*                                                                          */
/*        Namespace: Mondo.Xml							                    */
/*             File: XmlObject.cs									        */
/*        Class(es): XmlObject								                */
/*          Purpose: Base class for objects whose underlying data is        */
/*                      stored in xml                                       */
/*                                                                          */
/*  Original Author: Jim Lightfoot                                          */
/*    Creation Date: 1 Aug 2008                                             */
/*                                                                          */
/*   Copyright (c) 2008-2016 - Jim Lightfoot, All rights reserved           */
/*                                                                          */
/*  Licensed under the MIT license:                                         */
/*    http://www.opensource.org/licenses/mit-license.php                    */
/*                                                                          */
/****************************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Text;
using Mondo.Common;

namespace Mondo.Xml
{
    /****************************************************************************/
    /****************************************************************************/
    public class XmlObject 
    {
        private XmlNode m_xmlData;

        /****************************************************************************/
        public XmlObject(XmlNode xmlData) 
        {
            m_xmlData = xmlData;
        }

        /****************************************************************************/
        public XmlNode Xml
        {
            get {return(m_xmlData);}
        }

        /****************************************************************************/
        protected virtual void OnModified()
        {
            // Do nothing in base class
        }

        /****************************************************************************/
        public string GetAttribute(string idAttribute)
        {
            return(this.Xml.GetAttribute(idAttribute));
        }

        /****************************************************************************/
        public string GetChildText(string idChild)
        {
            return(this.Xml.GetChildText(idChild));
        }

        /****************************************************************************/
        public bool GetAttributeBool(string idAttribute)
        {
            return(Utility.ToBool(this.Xml.GetAttribute(idAttribute)));
        }

        /****************************************************************************/
        public int GetAttributeInt(string idAttribute)
        {
            return(Utility.ToInt(this.Xml.GetAttribute(idAttribute)));
        }

        /****************************************************************************/
        public virtual bool SetAttribute(string idAttribute, string strValue)
        {
            string strCurrentValue = GetAttribute(idAttribute);

            strValue = strValue.Normalized();

            if(strCurrentValue != strValue)
            {
                this.Xml.SetAttribute(idAttribute, strValue);
                OnModified();
                return(true);
            }

            return(false);
        }

        /****************************************************************************/
        public bool SetAttribute(string idAttribute, bool bValue)
        {
            return(SetAttribute(idAttribute, bValue ? "1" : "0"));
        }

        /****************************************************************************/
        public bool SetAttribute(string idAttribute, int iValue)
        {
            return(SetAttribute(idAttribute, iValue.ToString()));
        }

        /****************************************************************************/
        public virtual bool SetChildText(string idChild, string strValue)
        {
            string strCurrentValue = GetChildText(idChild);

            strValue = strValue.Normalized();

            if(strCurrentValue != strValue)
            {
                this.Xml.SetChildText(idChild, strValue);
                OnModified();
                return(true);
            }

            return(false);
        }

        /****************************************************************************/
        public bool SetChildText(string idChild, bool bValue)
        {
            return(SetChildText(idChild, bValue ? "1" : "0"));
        }

        /****************************************************************************/
        public bool SetChildText(string idChild, int iValue)
        {
            return(SetChildText(idChild, iValue.ToString()));
        }

        /****************************************************************************/
        public virtual bool RemoveChildren(string xPath)
        {
            if(this.Xml.RemoveChildren(xPath))
            {
                OnModified();
                return(true);
            }

            return(false);
        }
    }
}
