/****************************************************************************/
/*                                                                          */
/*    The Mondo Libraries  							                        */
/*                                                                          */
/*        Namespace: Mondo.Xml							                    */
/*             File: XmlSerialize.cs									    */
/*        Class(es): XmlSerialize								            */
/*          Purpose: Xml serialization                                      */
/*                                                                          */
/*  Original Author: Jim Lightfoot                                          */
/*    Creation Date: 27 Sep 2011                                            */
/*                                                                          */
/*   Copyright (c) 2011-2016 - Jim Lightfoot, All rights reserved           */
/*                                                                          */
/*  Licensed under the MIT license:                                         */
/*    http://www.opensource.org/licenses/mit-license.php                    */
/*                                                                          */
/****************************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Xml;
using System.Text;

using Mondo.Common;

namespace Mondo.Xml
{
    /****************************************************************************/
    /****************************************************************************/
	public class XmlSerialize : Attribute
    {
        private string m_strElementName;
        private bool   m_bForceType = false;
        protected Attributes m_eAttributes = Attributes.Normal;

        /****************************************************************************/
	    public enum Attributes
        {
            Normal = 0,
            SerializeOnly = 1,
            DeserializeOnly = 2
        }

        /****************************************************************************/
	    public XmlSerialize(string strElementName, Attributes eAttributes)
        {
            m_strElementName = strElementName;
            m_eAttributes    = eAttributes;
        }  

        /****************************************************************************/
	    public XmlSerialize(string strElementName) : this(strElementName, Attributes.Normal)
        {
        }  

        /****************************************************************************/
	    public XmlSerialize(Attributes eAttributes) : this("", eAttributes)
        {
        }  

        /****************************************************************************/
	    public XmlSerialize() : this("")
        {
        }  

        /****************************************************************************/
	    public XmlSerialize(bool bForceType) : this("")
        {
            m_bForceType = bForceType;
        }  

        /****************************************************************************/
	    public string ElementName
        {
            get {return(m_strElementName);}
        }   

        /****************************************************************************/
	    public virtual void SerializeProperty(cXMLWriter objWriter, object objInstance, PropertyInfo objProperty)
        {
            if(m_eAttributes != Attributes.DeserializeOnly)
            {
                string strElementName = this.ElementName;

                if(strElementName == "")
                    strElementName = objProperty.Name;

                string strValue = objProperty.GetValue(objInstance, null).Normalized();

                if(m_bForceType)
                {
                    using(new XmlElementWriter(objWriter, strElementName))
                    {
                        objWriter.WriteAttributeString("type", objProperty.PropertyType.ToString().ToLower().Replace("system.", ""));
                        objWriter.WriteString(strValue);
                    }
                }
                else
                    objWriter.WriteElementString(strElementName, strValue);
            }
        }

        /****************************************************************************/
	    public virtual void DeserializeProperty(XmlNode xmlData, object objInstance, PropertyInfo objProperty)
        {
            if(m_eAttributes != Attributes.SerializeOnly)
            {
                string strElementName = this.ElementName;

                if(strElementName == "")
                    strElementName = objProperty.Name;

                string strValue = xmlData.GetChildText(strElementName);
                object objValue = Utility.ConvertType(strValue, objProperty.PropertyType);
            
                objProperty.SetValue(objInstance, objValue, null);
            }
        }

        /****************************************************************************/
	    public static XmlSerialize GetSerializeAttribute(MemberInfo objMember)
        {
            IEnumerable aAttributes = objMember.GetCustomAttributes(true);

            foreach(Attribute objAttribute in aAttributes)
            {
                if(objAttribute is XmlSerialize)
                    return(objAttribute as XmlSerialize);
            }

            return(null);
        }
    }

    /****************************************************************************/
    /****************************************************************************/
	public class XmlAttributeSerialize : XmlSerialize
    {
        /****************************************************************************/
	    public XmlAttributeSerialize(string strAttributeName, Attributes eAttributes) : base(strAttributeName, eAttributes)
        {
        }  

        /****************************************************************************/
	    public XmlAttributeSerialize(string strAttributeName) : this(strAttributeName, Attributes.Normal)
        {
        }  

        /****************************************************************************/
	    public XmlAttributeSerialize(Attributes eAttributes) : this("", eAttributes)
        {
        }  

        /****************************************************************************/
	    public XmlAttributeSerialize() : this("")
        {
        }  

        /****************************************************************************/
	    public override void SerializeProperty(cXMLWriter objWriter, object objInstance, PropertyInfo objProperty)
        {
            if(m_eAttributes != Attributes.DeserializeOnly)
            {
                string strAttributeName = this.ElementName;

                if(strAttributeName == "")
                    strAttributeName = objProperty.Name;

                string strValue = objProperty.GetValue(objInstance, null).Normalized();

                objWriter.WriteAttributeString(strAttributeName, strValue);
            }
        }

        /****************************************************************************/
	    public override void DeserializeProperty(XmlNode xmlData, object objInstance, PropertyInfo objProperty)
        {
            if(m_eAttributes != Attributes.SerializeOnly)
            {
                string strAttributeName = this.ElementName;

                if(strAttributeName == "")
                    strAttributeName = objProperty.Name;

                string strValue = xmlData.GetAttribute(strAttributeName);
                object objValue = Utility.ConvertType(strValue, objProperty.PropertyType);
            
                objProperty.SetValue(objInstance, objValue, null);
            }
        }
    }

    /****************************************************************************/
    /****************************************************************************/
	public class XmlSerializeJSONList : XmlSerializeList
    {    
        /****************************************************************************/
	    public XmlSerializeJSONList(string strElementName) : base(strElementName)
        {
        }  

        /****************************************************************************/
	    public override void SerializeProperty(cXMLWriter objWriter, object objInstance, PropertyInfo objProperty)
        {
            IEnumerable aList = objProperty.GetValue(objInstance, null) as IEnumerable;

            using(XmlElementWriter w = new XmlElementWriter(objWriter, "List"))
            {
                objWriter.WriteAttributeString("name", this.ElementName);

                SerializeList(objWriter, aList);
            }
        }
    }

    /****************************************************************************/
    /****************************************************************************/
	public class XmlSerializeList : XmlSerialize
    {
        private bool m_bXmlSerializeOnly = false;

        /****************************************************************************/
	    public XmlSerializeList(string strElementName, bool bXmlSerializeOnly) : base(strElementName, Attributes.SerializeOnly)
        {
            m_bXmlSerializeOnly = bXmlSerializeOnly;
        }  

        /****************************************************************************/
	    public XmlSerializeList() : this("", false)
        {
        }  

        /****************************************************************************/
	    public XmlSerializeList(string strElementName) : this(strElementName, false)
        {
        }  

       /****************************************************************************/
	    public XmlSerializeList(bool bXmlSerializeOnly) : this("", bXmlSerializeOnly)
        {
        }  

        /****************************************************************************/
	    public bool XmlSerializeOnly
        {
            get {return(m_bXmlSerializeOnly);}
        }  

        /****************************************************************************/
	    public override void SerializeProperty(cXMLWriter objWriter, object objInstance, PropertyInfo objProperty)
        {
            IEnumerable aList = objProperty.GetValue(objInstance, null) as IEnumerable;

            if(this.ElementName != "")
            {
                using(XmlElementWriter w = new XmlElementWriter(objWriter, this.ElementName))
                {
                    SerializeList(objWriter, aList);
                }
            }
            else
                SerializeList(objWriter, aList);
        }

        /****************************************************************************/
	    public override void DeserializeProperty(XmlNode xmlData, object objInstance, PropertyInfo objProperty)
        {
            // Can't deserialize a list. Must do manually
        }

        /****************************************************************************/
	    protected void SerializeList(cXMLWriter objWriter, IEnumerable aList)
        {
            foreach(object objChild in aList)
            {
                if(objChild is IXmlSerialize)
                    (objChild as IXmlSerialize).Serialize(objWriter);
                else
                {
                    Type         objType      = objChild.GetType();
                    XmlSerialize objAttribute = XmlSerialize.GetSerializeAttribute(objType);

                    if(!this.XmlSerializeOnly || objAttribute != null)
                        XmlSerializer.Serialize(objWriter, objChild, objAttribute);
                }
            }
        }
    }
}
