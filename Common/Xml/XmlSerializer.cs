/****************************************************************************/
/*                                                                          */
/*    The Mondo Libraries  							                        */
/*                                                                          */
/*        Namespace: Mondo.Xml							                    */
/*             File: XmlSerializer.cs									    */
/*        Class(es): XmlSerializer								            */
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
	public interface IXmlSerialize 
    {
        void Serialize(cXMLWriter objWriter);
        void Deserialize(XmlNode xmlItem);
    }

    /****************************************************************************/
    /****************************************************************************/
	public class XmlSerializer
    {
        /****************************************************************************/
	    public static XmlDocument Serialize(object objData)
        {
            cXMLWriter objWriter = new cXMLWriter();

            using(objWriter.Acquire)
            {
                Serialize(objWriter, objData);
            }

            XmlDocument xmlResults = objWriter.Xml;

            //DebugConsole.WriteLine(xmlResults.OuterXml);

            return(xmlResults);
        }

        /****************************************************************************/
	    public static void Deserialize(XmlNode xmlData, object objInstance)
        {
            if(xmlData is XmlDocument)
                xmlData = XmlDoc.RootElement(xmlData as XmlDocument);

            DeserializeChildren(xmlData, objInstance);

            if(objInstance is IXmlSerialize)
                (objInstance as IXmlSerialize).Deserialize(xmlData);
        }

        /****************************************************************************/
	    public static void Serialize(cXMLWriter objWriter, object objInstance, XmlSerialize objAttribute)
        {
            Type   objType        = objInstance.GetType();
            string strElementName = objType.Name;

            if(objAttribute != null && objAttribute.ElementName != "")
                strElementName = objAttribute.ElementName;

            using(XmlElementWriter w = new XmlElementWriter(objWriter, strElementName))
            {
                SerializeChildren(objWriter, objInstance);
            }

            return;
        }

        /****************************************************************************/
	    private static void Serialize(cXMLWriter objWriter, object objInstance)
        {
            if(objInstance is IXmlSerialize)
            {
                (objInstance as IXmlSerialize).Serialize(objWriter);
                return;
            }

            Type         objType      = objInstance.GetType();
            XmlSerialize objAttribute = XmlSerialize.GetSerializeAttribute(objType);

            Serialize(objWriter, objInstance, objAttribute);

            return;
        }

        /****************************************************************************/
	    /****************************************************************************/
	    private class SerializeProperty
        {
            public XmlSerialize Attribute;
            public PropertyInfo Property;

            /****************************************************************************/
	        public SerializeProperty(PropertyInfo prop, XmlSerialize attr)
            {
                Attribute = attr;
                Property = prop;
            }
        }

        /****************************************************************************/
	    private static void SerializeChildren(cXMLWriter objWriter, object objInstance)
        {
            Type                     objType     = objInstance.GetType();
            PropertyInfo[]           aProperties = objType.GetProperties(); // ??? TODO: Let's revisit the filter some other time
            List<SerializeProperty>  aAttributes = new List<SerializeProperty>();
            List<SerializeProperty>  aLists      = new List<SerializeProperty>();
            List<SerializeProperty>  aElements   = new List<SerializeProperty>();

            // Gather all the properties that have an XmlSerialize attribute
            foreach(PropertyInfo objProperty in aProperties)
            {
                XmlSerialize objAttribute = XmlSerialize.GetSerializeAttribute(objProperty);

                if(objAttribute != null)
                {
                    SerializeProperty objProp = new SerializeProperty(objProperty, objAttribute);

                    // Put into the right bucket
                    if(objAttribute is XmlAttributeSerialize)
                        aAttributes.Add(objProp);
                    else if(objAttribute is XmlSerializeList)
                        aLists.Add(objProp);
                    else
                        aElements.Add(objProp);
                }
            }

            // Write out the xml attributes first
            foreach(SerializeProperty objProperty in aAttributes)
                objProperty.Attribute.SerializeProperty(objWriter, objInstance, objProperty.Property);

            // Now write out all of the elements
            foreach(SerializeProperty objProperty in aElements)
                objProperty.Attribute.SerializeProperty(objWriter, objInstance, objProperty.Property);

            // Now write out all of the lists
            foreach(SerializeProperty objProperty in aLists)
                objProperty.Attribute.SerializeProperty(objWriter, objInstance, objProperty.Property);

            return;
        }

        /****************************************************************************/
	    private static void DeserializeChildren(XmlNode xmlData, object objInstance)
        {
            Type           objType     = objInstance.GetType();
            PropertyInfo[] aProperties = objType.GetProperties(); // ??? TODO: Let's revisit the filter some other time
 
            foreach(PropertyInfo objProperty in aProperties)
            {
                try
                {
                    XmlSerialize objAttribute = XmlSerialize.GetSerializeAttribute(objProperty);

                    if(objAttribute != null)
                        objAttribute.DeserializeProperty(xmlData, objInstance, objProperty);
                }
                catch(Exception ex)
                {
                    cDebug.Capture(ex);
                }
            }

            return;
        }
    }
}
