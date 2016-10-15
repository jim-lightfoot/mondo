/****************************************************************************/
/*                                                                          */
/*    The Mondo Libraries  							                        */
/*                                                                          */
/*        Namespace: Mondo.Business							                */
/*             File: MemberAttribute.cs						                */
/*        Class(es): MemberAttribute							            */
/*          Purpose: A class data member                                    */
/*                                                                          */
/*  Original Author: Jim Lightfoot                                          */
/*    Creation Date: 11 Apr 2008                                            */
/*                                                                          */
/*   Copyright (c) 2008-2010 - Tenth Generation Software, LLC               */
/*                               All rights reserved                        */
/*                                                                          */
/****************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;

using Mondo.Common;

namespace Mondo.Business
{
    /****************************************************************************/
    /****************************************************************************/
    public enum PersistType
    {
        Normal      = 0,
        ReadOnly    = 1,  // Attributes that are read from db but not written to
        Transient   = 2,  // Attributes that are set by app code but handled in a custom way in biz objects (not read from the db)
        Declarative = 3   // Attributes that are set when created (INSERT) only
    }

    /****************************************************************************/
    /****************************************************************************/
     public interface IPersistAttribute
    {
        bool        Modified    { get; }
        short       ModifiedBit { get; }
        bool        ReadOnly    { get; }
        Type        ValueType   { get; }
        bool        IsNull      { get; }
        PersistType PersistType { get; }

        void        SetValue(IDataObjectSource objSource, string idField);
    }

    /****************************************************************************/
    /****************************************************************************/
    public class MemberAttribute<T> : IPersistAttribute, IComparable, IComparable<T>, IComparable<MemberAttribute<T>> where T : IComparable
    {
        private T           m_Value;
        private bool        m_bModified;
        private PersistType m_eType;

        /****************************************************************************/
        public MemberAttribute(T initValue)
        {
            m_Value     = initValue;
            m_bModified = false;
            m_eType     = PersistType.Normal;
        }

        /****************************************************************************/
        public MemberAttribute(T initValue, PersistType eType)
        {
            m_Value     = initValue;
            m_bModified = false;
            m_eType     = eType;
        }

        /****************************************************************************/
        public PersistType PersistType
        {
            get {return(m_eType); }
        }

        /****************************************************************************/
        public bool Modified
        {
            get {return(m_bModified); }
        }

        /****************************************************************************/
        public short ModifiedBit
        {
            get {return(m_bModified.ToBit()); }
        }

        /****************************************************************************/
        public bool ReadOnly
        {
            get {return(m_eType == PersistType.ReadOnly); }
        }

        /****************************************************************************/
        public virtual bool IsNull      
        {
            get {return(false);}
        }

        /****************************************************************************/
        public T Value
        {
            get 
            {
                return(m_Value);
            }

            set
            {
                if(!value.Equals(m_Value))
                {
                    m_Value = value;
                    m_bModified = true;
                }
            }
        }

        /****************************************************************************/
        public void SetValue(IDataObjectSource objSource, string idField)
        {
            string strValue = objSource.GetString(idField);

            try
            {
                m_Value = (T)Utility.Convert(strValue, typeof(T));
            }
            catch(Exception ex)
            {
                if(ex.Message.ToLower().Contains("not in correct format"))
                    throw new Exception("Mismatched database and business object types.", ex);

                throw;
            }
        }

        /****************************************************************************/
        public Type ValueType
        {
            get 
            {
                return(typeof(T));
            }
        }

        /****************************************************************************/
        public override string ToString()
        {
            return(m_Value.ToString());
        }

        /****************************************************************************/
        public override int GetHashCode()
        {
            return(m_Value.GetHashCode());
        }

        /****************************************************************************/
        public override bool Equals(object obj)
        {
            if(obj is MemberAttribute<T>)
                return(m_Value.Equals((obj as MemberAttribute<T>).m_Value));

            return(m_Value.Equals((T)obj));
        }

        #region Overloaded Operators

        /****************************************************************************
         * MemberAttribute<int> iCount = new MemberAttribute<int>();
         * 
         * int nValue = iCount;
         ****************************************************************************/
        public static implicit operator T(MemberAttribute<T> v)
        {
            return(v.m_Value);
        }

        /****************************************************************************/
        public static bool operator ==(MemberAttribute<T> v1, MemberAttribute<T> v2)
        {
            return(v1.CompareTo(v2) == 0);
        }

        /****************************************************************************/
        public static bool operator !=(MemberAttribute<T> v1, MemberAttribute<T> v2)
        {
            return(v1.CompareTo(v2) != 0);
        }

        /****************************************************************************/
        public static bool operator >=(MemberAttribute<T> v1, MemberAttribute<T> v2)
        {
            return(v1.CompareTo(v2) >= 0);
        }

        /****************************************************************************/
        public static bool operator <=(MemberAttribute<T> v1, MemberAttribute<T> v2)
        {
            return(v1.CompareTo(v2) <= 0);
        }

        /****************************************************************************/
        public static bool operator >(MemberAttribute<T> v1, MemberAttribute<T> v2)
        {
            return(v1.CompareTo(v2) > 0);
        }

        /****************************************************************************/
        public static bool operator <(MemberAttribute<T> v1, MemberAttribute<T> v2)
        {
            return(v1.CompareTo(v2) < 0);
        }

        #endregion

        #region IComparable Members

        /****************************************************************************/
        public int CompareTo(object obj)
        {
            if(obj is MemberAttribute<T>)
                return(CompareTo(obj as MemberAttribute<T>));

            return(CompareTo((T)obj));
        }

        #endregion

        #region IComparable<T> Members

        /****************************************************************************/
        public int CompareTo(T other)
        {
            return(m_Value.CompareTo(other));
        }

        #endregion

        #region IComparable<MemberAttribute<T>> Members

        /****************************************************************************/
        public int CompareTo(MemberAttribute<T> other)
        {
            return(CompareTo(other.m_Value));
        }

        #endregion
    }

    /****************************************************************************/
    /****************************************************************************/
    public class MemberAttributeInt : MemberAttribute<int>
    {
        private bool m_bZeroIsNull = false;

        /****************************************************************************/
        public MemberAttributeInt(int initValue) : base(initValue)
        {
        }

        /****************************************************************************/
        public MemberAttributeInt(int initValue, PersistType eType) : base(initValue, eType)
        {
        }

        /****************************************************************************/
        public MemberAttributeInt(int initValue, PersistType eType, bool bZeroIsNull) : base(initValue, eType)
        {
            m_bZeroIsNull = bZeroIsNull;
        }

        /****************************************************************************/
        public bool ZeroIsNull {get{return(m_bZeroIsNull);}}

        /****************************************************************************/
        public override bool IsNull
        {
            get
            {
                return(ZeroIsNull && (this.Value == 0));
            }
        }
    }

    /****************************************************************************/
    /****************************************************************************/
    public class MemberAttributeGuid : MemberAttribute<Guid>
    {
        /****************************************************************************/
        public MemberAttributeGuid(Guid initValue) : base(initValue)
        {
        }

        /****************************************************************************/
        public MemberAttributeGuid(Guid initValue, PersistType eType) : base(initValue, eType)
        {
        }

        /****************************************************************************/
        public override bool IsNull
        {
            get
            {
                return(this.Value == Guid.Empty);
            }
        }
    }

    /****************************************************************************/
    /****************************************************************************/
    public class MemberAttributeLong : MemberAttribute<long>
    {
        private bool m_bZeroIsNull = false;

        /****************************************************************************/
        public MemberAttributeLong(long initValue) : base(initValue)
        {
        }

        /****************************************************************************/
        public MemberAttributeLong(long initValue, PersistType eType) : base(initValue, eType)
        {
        }

        /****************************************************************************/
        public MemberAttributeLong(long initValue, PersistType eType, bool bZeroIsNull) : base(initValue, eType)
        {
            m_bZeroIsNull = bZeroIsNull;
        }

        /****************************************************************************/
        public bool ZeroIsNull {get{return(m_bZeroIsNull);}}

        /****************************************************************************/
        public override bool IsNull
        {
            get
            {
                return(ZeroIsNull && (this.Value == 0));
            }
        }
    }

    /****************************************************************************/
    /****************************************************************************/
    public class MemberAttributeDecimal : MemberAttribute<decimal>
    {
        private bool m_bMinIsNull = false;
        
        /****************************************************************************/
        public MemberAttributeDecimal(decimal initValue) : base(initValue)
        {
        }

        /****************************************************************************/
        public MemberAttributeDecimal(decimal initValue, PersistType eType) : base(initValue, eType)
        {
        }

        /****************************************************************************/
        public MemberAttributeDecimal(decimal initValue, PersistType eType, bool bMinIsNull) : base(initValue, eType)
        {
            m_bMinIsNull = bMinIsNull;
        }

        /****************************************************************************/
        public MemberAttributeDecimal(decimal initValue, bool bMinIsNull) : base(initValue)
        {
            m_bMinIsNull = bMinIsNull;
        }

        /****************************************************************************/
        public bool MinIsNull {get{return(m_bMinIsNull);}}

        /****************************************************************************/
        public override bool IsNull
        {
            get
            {
                return(MinIsNull && (this.Value == decimal.MinValue));
            }
        }
    }

    /****************************************************************************/
    /****************************************************************************/
    public class Persist : Attribute
    {
        private string m_strColumnName;

        /****************************************************************************/
        public Persist() : this("")
        {
        }

        /****************************************************************************/
        public Persist(string strName)
        {
            m_strColumnName = strName;
        }

        /****************************************************************************/
        public string ColumnName
        {
            get { return(m_strColumnName); }
        }
    }
}
