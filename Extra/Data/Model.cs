/****************************************************************************/
/*                                                                          */
/*    The Mondo Libraries  							                        */
/*                                                                          */
/*        Namespace: Mondo.Common							                */
/*             File: Model.cs					    		                */
/*        Class(es): Model				         		                    */
/*          Purpose: Generic data item                                      */
/*                                                                          */
/*  Original Author: Jim Lightfoot                                          */
/*    Creation Date: 28 Sep 2003                                            */
/*                                                                          */
/*   Copyright (c) 2003-2007 - Jim Lightfoot, All rights reserved           */
/*                                                                          */
/****************************************************************************/

using System;
using System.Xml;
using System.Collections;
using System.Collections.Generic;

namespace Mondo.Common
{
    /****************************************************************************/
    /****************************************************************************/
    public interface IDataObject : IModel
    {
        string      GetAttributeValue(string idAttribute);
        XmlElement  GetAttributeElement(string idAttribute);

        void        SetAttributeValue(string idAttribute, object objValue);
        void        SetAttributeDefault(string idAttribute);
        void        SetSubAttributeValue(string idAttribute, string idSubAttribute, object objValue);

        XmlNode     Xml         {get;}
    }

    /****************************************************************************/
    /****************************************************************************/
    public interface INamedObject
    {
        string Name {get; set;}
    }
        
    /****************************************************************************/
    /****************************************************************************/
    public interface ITitledObject
    {
        string Title {get; set;}
    }
        
    /****************************************************************************/
    /****************************************************************************/
    public class NamedObject : INamedObject
    {
        private string m_strName = "";

        /****************************************************************************/
        public NamedObject()
        {
        }

        /****************************************************************************/
        public NamedObject(string strName) 
        {
            m_strName = strName;
        }

        /****************************************************************************/
        public virtual string Name 
        {
            get {return(m_strName);} 
            set {m_strName = value;}
        }

        /****************************************************************************/
        public override string ToString()
        {
            return(this.Name);
        }
    }
        
    /****************************************************************************/
    /****************************************************************************/
    public interface IDescription
    {
        string Description {get;}
    }
        
    /****************************************************************************/
    /****************************************************************************/
    public interface IFilter
    {
        bool Matches(object objCheck);
    }

    /****************************************************************************/
    /****************************************************************************/
    public interface IFilter<T>
    {
        bool Matches(T objCheck);
    }

    /****************************************************************************/
    /****************************************************************************/
    public interface IDataContainer
    {
        IDataObject CurrentObject {get;set;}
    }

    /****************************************************************************/
    /****************************************************************************/
    public interface IConditional
    {
        bool Matches(object objSource);
    }

    /****************************************************************************/
    /****************************************************************************/
    public interface IConditionalItem
    {
        bool CanShow(Common.IDataObject objItem);
    }

    /****************************************************************************/
    /****************************************************************************/
	public interface IDocumentContainer
    {
        string DocName   {get; set;}
        string DocPath   {get;}
    }

    /****************************************************************************/
    /****************************************************************************/
    public interface INode 
    {
        string NodeName      {get;}
    }

    /****************************************************************************/
    /****************************************************************************/
    public class UpdateArgs
    {
        public static UpdateArgs Empty = new UpdateArgs();

        /****************************************************************************/
        public UpdateArgs() : this(true)
        {
        }

        /****************************************************************************/
        public UpdateArgs(bool bUp)
        {
            this.Up = bUp;
        }

        /****************************************************************************/
        public bool Up
        {
            get;
            private set;
        }
    }

    /****************************************************************************/
    /****************************************************************************/
    public interface IEditor
    {
        void OnUpdateData(UpdateArgs e);
        void RetrieveData();
    }

    /****************************************************************************/
    /****************************************************************************/
     public interface ICommandable
    {
        void DoCommand(string strCommand, object objData);
    }

    /****************************************************************************/
	/****************************************************************************/
	public interface IConfiguration
	{
		string Get(string idValue);
	}

    /****************************************************************************/
    /****************************************************************************/
    public abstract class XmlLoadable
    {
        /****************************************************************************/
        public XmlLoadable()
        {
        }

        /****************************************************************************/
        public virtual void Load(XmlTextReader objReader)
        {
            bool bEmpty = objReader.IsEmptyElement;

            ReadAttributes(objReader);

            if(!bEmpty)
            {              
                while(objReader.Read())
                {
                    switch(objReader.NodeType)
                    {
                        case XmlNodeType.Element:  
                        {
                            //if(!objReader.IsEmptyElement) 
                                LoadElement(objReader);  
   
                            break;
                        }

                        case XmlNodeType.CDATA:
                        case XmlNodeType.Text:              
                            LoadInnerText(objReader.Value); 
                            break;

                        case XmlNodeType.EndElement:    
                            return;

                        default:                        
                            break;
                    }
                }
            }
        }

        /****************************************************************************/
        protected virtual void LoadInnerText(string strText)
        {
            // Do nothing in base class
        }

        /****************************************************************************/
        public virtual void SetAttribute(string strAttrName, string strValue)
        {
            // Base class does nothing
        }

        /****************************************************************************/
        protected virtual void ReadAttributes(XmlTextReader objReader)
        {
            BaseReadAttributes(objReader);      
        }

        /****************************************************************************/
        protected void BaseReadAttributes(XmlTextReader objReader)
        {
            bool bReadAttribute = objReader.MoveToFirstAttribute();

            while(bReadAttribute)
            {
                SetAttribute(objReader.Name, objReader.Value);
                bReadAttribute = objReader.MoveToNextAttribute();
            }           
        }

        /****************************************************************************/
        public virtual void LoadElement(XmlTextReader objReader)
        {
            switch(objReader.Name)
            {
                case "tenthgeneration":
                case "objects":
                    Load(objReader);
                    break;

                default:
                    LoadUnknown(objReader);
                    break;
            }
        }

        /****************************************************************************/
        public static string ReadInnerText(XmlTextReader objReader)
        {
            string strValue = "";

            if(!objReader.IsEmptyElement)
            {
                while(objReader.Read())
                {
                    switch(objReader.NodeType)
                    {
                        case XmlNodeType.Element:           LoadUnknown(objReader);     break;
                        case XmlNodeType.Text:              strValue = objReader.Value; break;
                        case XmlNodeType.CDATA:             strValue = objReader.Value; break;
                        case XmlNodeType.EndElement:                                    return(strValue);
                        default:                                                        break;
                    }
                }
            }

            return(strValue);
        }

        /****************************************************************************/
        public static void LoadUnknown(XmlTextReader objReader)
        {
            if(!objReader.IsEmptyElement)
            {
                while(objReader.Read())
                {
                    switch(objReader.NodeType)
                    {
                        case XmlNodeType.Element:           LoadUnknown(objReader);     break;
                        case XmlNodeType.Text:                                          break;
                        case XmlNodeType.CDATA:                                         break;
                        case XmlNodeType.EndElement:                                    return;
                        default:                                                        break;
                    }
                }
            }
        }
    }

    /****************************************************************************/
    /****************************************************************************/
    public abstract class DelegateXmlLoader : XmlLoadable
    {
        protected XmlLoadable m_objDelegate = null;
        private   Hashtable   m_htAttributes = new Hashtable(17);

        /****************************************************************************/
        public DelegateXmlLoader()
        {
        }

        /****************************************************************************/
        protected override void ReadAttributes(XmlTextReader objReader)
        {
            base.ReadAttributes(objReader);

            m_objDelegate = CreateDelegate();

            foreach(DictionaryEntry objEntry in m_htAttributes)
                m_objDelegate.SetAttribute(objEntry.Key.ToString(), objEntry.Value.ToString());
        }

        /****************************************************************************/
        public override void LoadElement(XmlTextReader objReader)
        {
            m_objDelegate.LoadElement(objReader);
        }

        /****************************************************************************/
        public override void SetAttribute(string strAttrName, string strValue)
        {
            m_htAttributes.Add(strAttrName, strValue);
        }

        /****************************************************************************/
        public Hashtable   Attributes       {get{return(m_htAttributes);}}

        /****************************************************************************/
        public string GetAttribute(string strName)
        {
            try
            {
                return(this.Attributes[strName].ToString());
            }
            catch
            {
                return("");
            }
        }

        /****************************************************************************/
        public int GetAttributeInt(string strName)
        {
            return(Utility.ToInt(GetAttribute(strName)));
        }

        /****************************************************************************/
        public bool GetAttributeBool(string strName)
        {
            return(Utility.IsTrue(GetAttribute(strName)));
        }

        /****************************************************************************/
        protected abstract XmlLoadable CreateDelegate();
    }

    /****************************************************************************/
    /****************************************************************************/
    public class PassThruXmlLoader : DelegateXmlLoader
    {
        /****************************************************************************/
        public PassThruXmlLoader(XmlLoadable objParent)
        {
            m_objDelegate = objParent;
        }

        /****************************************************************************/
        public static void Load(XmlTextReader objReader, XmlLoadable objParent)
        {
            PassThruXmlLoader objLoader = new PassThruXmlLoader(objParent);

            objLoader.Load(objReader);
        }

        /****************************************************************************/
        protected override void ReadAttributes(XmlTextReader objReader)
        {
            BaseReadAttributes(objReader);      
        }

        /****************************************************************************/
        protected override XmlLoadable CreateDelegate()
        {
            return(m_objDelegate);
        }
    }

    /****************************************************************************/
    /****************************************************************************/
    public abstract class XmlDocLoadable
    {
        /****************************************************************************/
        public XmlDocLoadable()
        {
        }

        /****************************************************************************/
        public virtual void Load(XmlNode xmlData)
        {
            LoadAttributes(xmlData);
            LoadChildren(xmlData);
        }

        /****************************************************************************/
        protected void LoadAttributes(XmlNode xmlData)
        {
            XmlAttributeCollection aAttributes = xmlData.Attributes;

            if(aAttributes != null)
                foreach(XmlAttribute objAttribute in aAttributes)
                    LoadAttribute(objAttribute.Name, objAttribute.Value);
        }

        /****************************************************************************/
        protected virtual void LoadAttribute(string strName, string strValue)
        {
            // Base class does nothing
        }

        /****************************************************************************/
        protected void LoadChildren(XmlNode xmlData)
        {
            XmlNodeList aChildren = xmlData.ChildNodes;

            foreach(XmlNode xmlChild in aChildren)
            {
                switch(xmlChild.NodeType)
                {
                    case XmlNodeType.Text:
                    {
                        LoadText(xmlChild);
                        break;
                    }

                    case XmlNodeType.Element:
                        LoadChild(xmlChild, xmlChild.LocalName);
                        break;
                }
            }

            return;
        }

        /****************************************************************************/
        protected void LoadChildren(XmlNode xmlData, string strXPath)
        {
            if(strXPath == "*")
                LoadChildren(xmlData);
            else
            {
                XmlNodeList aChildren = xmlData.SelectNodes(strXPath);

                foreach(XmlElement xmlChild in aChildren)
                    LoadChild(xmlChild, xmlChild.LocalName);
            }

            return;
        }

        /****************************************************************************/
        protected virtual void LoadChild(XmlNode xmlChild, string strName)
        {
            // do nothing in base class
        }

        /****************************************************************************/
        protected virtual void LoadText(XmlNode xmlText)
        {
            // Base class does nothing
        }
    }

    /****************************************************************************/
    /****************************************************************************/
    public abstract class DataModel : IModel
    {
        private bool                    m_bModified = false;
        private bool                    m_bReadOnly = false;
        private IModel                  m_objParent;
        private readonly List<IEditor>  m_aEditors  = new List<IEditor>();

        /****************************************************************************/
        public DataModel()
        {
            m_objParent = null;
        }

        /****************************************************************************/
        public DataModel(IModel objParent)
        {
            m_objParent = objParent;
        }

        /****************************************************************************/
        public virtual void Save()
        {
            m_bModified = false;
        }

        /****************************************************************************/
        public IModel        Parent     {get{return(m_objParent);} set{m_objParent = value;}}
        public bool          ReadOnly   {get{return(m_bReadOnly);} set{m_bReadOnly = value;}}
        public List<IEditor> Editors    {get{return(m_aEditors);}}
  
        /****************************************************************************/
        public virtual void OnUpdate()
        {
           UpdateArgs e = new UpdateArgs(false);

            foreach(IEditor objEditor in m_aEditors)
                objEditor.OnUpdateData(e);
        }

        /****************************************************************************/
        public bool Modified
        {
            get
            {
                return(m_bModified);
            }

            set
            {
                if(value)
                {
                    m_bModified = true;
                    OnUpdate();

                    if(m_objParent != null)
                        m_objParent.Modified = true;
                }
                else
                    m_bModified = false;
            }
        }

        /****************************************************************************/
        public virtual void OnBeforeSave()
        {
        }

        /****************************************************************************/
        public virtual void OnAfterSave()
        {
            this.Modified = false;
        }

        /****************************************************************************/
        protected bool Modify<T>(ref T objField, T objValue)
        {
            if(objField is IComparable)
            {
                if((objField as IComparable).CompareTo(objValue) == 0)
                    return(false);
            }

            objField = objValue;
            Modified = true;
            OnUpdate();
            return(true);
        }
    }
    
    /****************************************************************************/
    /****************************************************************************/
    public abstract class Model : XmlDocLoadable, IModel
    {
        private bool          m_bModified = false;
        private bool          m_bReadOnly = false;
        private IModel        m_objParent;
        private readonly List<IEditor> m_aEditors = new List<IEditor>();

        /****************************************************************************/
        public Model()
        {
            m_objParent = null;
        }

        /****************************************************************************/
        public Model(IModel objParent)
        {
            m_objParent = objParent;
        }

        /****************************************************************************/
        public virtual void Save()
        {
            m_bModified = false;
        }

        /****************************************************************************/
        public IModel        Parent     {get{return(m_objParent);} set{m_objParent = value;}}
        public bool          ReadOnly   {get{return(m_bReadOnly);} set{m_bReadOnly = value;}}
        public List<IEditor> Editors    {get{return(m_aEditors);}}
  
        /****************************************************************************/
        public virtual void OnUpdate()
        {
            UpdateArgs e = new UpdateArgs();

            foreach(IEditor objEditor in m_aEditors)
                objEditor.OnUpdateData(e);
        }

        /****************************************************************************/
        public override void Load(XmlNode xmlData)
        {
            base.Load(xmlData);
            this.m_bModified = false;
        }

        /****************************************************************************/
        public bool Modified
        {
            get
            {
                return(m_bModified);
            }

            set
            {
                if(value)
                {
                    m_bModified = true;
                    OnUpdate();

                    if(m_objParent != null)
                        m_objParent.Modified = true;
                }
                else
                    m_bModified = false;
            }
        }

        /****************************************************************************/
        protected bool Modify(ref string strField, string strValue)
        {
            strValue = strValue.Trim();

            if(strField != strValue)
            {
                strField = strValue;
                Modified = true;
                return(true);
            }

            return(false);
        }

        /****************************************************************************/
        protected bool Modify(ref bool bField, bool bValue)
        {
            if(bField != bValue)
            {
                bField = bValue;
                Modified = true;
                OnUpdate();
                return(true);
            }

            return(false);
        }

        /****************************************************************************/
        protected bool Modify(ref Guid guidField, Guid guidValue)
        {
            if(guidField != guidValue)
            {
                guidField = guidValue;
                Modified = true;
                OnUpdate();
                return(true);
            }

            return(false);
        }

        /****************************************************************************/
        protected bool Modify(ref int iField, int iValue)
        {
            if(iField != iValue)
            {
                iField = iValue;
                Modified = true;
                OnUpdate();
                return(true);
            }

            return(false);
        }


        public void OnBeforeSave()
        {
            
        }

        public void OnAfterSave()
        {
            
        }
    }
    
    /****************************************************************************/
    /****************************************************************************/
    public abstract class XmlModel : XmlLoadable
    {
        private bool     m_bModified = false;
        private bool     m_bReadOnly = false;
        private XmlModel m_objParent;

        /****************************************************************************/
        public XmlModel()
        {
            m_objParent = null;
        }

        /****************************************************************************/
        public XmlModel(XmlModel objParent)
        {
            m_objParent = objParent;
        }

        /****************************************************************************/
        public virtual void Save()
        {
            m_bModified = false;
        }

        /****************************************************************************/
        public XmlModel Parent            {get{return(m_objParent);} set{m_objParent = value;}}
        public bool     ReadOnly          {get{return(m_bReadOnly);} set{m_bReadOnly = value;}}

        /****************************************************************************/
        public virtual void OnUpdate()
        {
            // Do nothing in base class
        }

        /****************************************************************************/
        public override void Load(XmlTextReader objReader)
        {
            base.Load(objReader);
            m_bModified = false;
        }

        /****************************************************************************/
        public bool Modified
        {
            get
            {
                return(m_bModified);
            }

            set
            {
                if(value)
                {
                    m_bModified = true;

                    if(m_objParent != null)
                        m_objParent.Modified = true;
                }
                else
                    m_bModified = false;
            }
        }

        /****************************************************************************/
        protected bool Modify(ref string strField, string strValue)
        {
            strValue = strValue.Trim();

            if(strField != strValue)
            {
                strField = strValue;
                Modified = true;
                OnUpdate();
                return(true);
            }

            return(false);
        }

        /****************************************************************************/
        protected bool Modify(ref bool bField, bool bValue)
        {
            if(bField != bValue)
            {
                bField = bValue;
                Modified = true;
                OnUpdate();
                return(true);
            }

            return(false);
        }

        /****************************************************************************/
        protected bool Modify(ref int iField, int iValue)
        {
            if(iField != iValue)
            {
                iField = iValue;
                Modified = true;
                OnUpdate();
                return(true);
            }

            return(false);
        }

        /****************************************************************************/
        protected bool Modify(ref long iField, long iValue)
        {
            if(iField != iValue)
            {
                iField = iValue;
                Modified = true;
                OnUpdate();
                return(true);
            }

            return(false);
        }

        /****************************************************************************/
        protected bool Modify(ref Guid idField, Guid idValue)
        {
            if(idField != idValue)
            {
                idField = idValue;
                Modified = true;
                OnUpdate();
                return(true);
            }

            return(false);
        }
    }
}
