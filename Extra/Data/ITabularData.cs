/****************************************************************************/
/*                                                                          */
/*    The Mondo Libraries  							                        */
/*                                                                          */
/*        Namespace: Mondo.Common							                */
/*             File: ITabularData.cs						                */
/*        Class(es): ITabularData, IDataWatcher							    */
/*          Purpose: Data related interfaces                                */
/*                                                                          */
/*  Original Author: Jim Lightfoot                                          */
/*    Creation Date: 27 May 2006                                            */
/*                                                                          */
/*   Copyright (c) 2006 - Tenth Generation Software, LLC                    */
/*                          All rights reserved                             */
/*                                                                          */
/****************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;

namespace Mondo.Common
{
    /****************************************************************************/
    /****************************************************************************/
    public interface ITabularData
    {
        int    NumRows      {get;}
        int    NumColumns   {get;}

        object GetValue(int iRow, int iColumn);
        string GetDisplay(int iRow, int iColumn);
        void   SetValue(int iRow, int iColumn, object objValue);

        void   InsertRow(int iIndex);
        void   AddRow();
        void   DeleteRow(int iIndex);

        string ToXmlString();
    }

    /****************************************************************************/
    /****************************************************************************/
    public interface IDataWatcher
    {
        void OnValueChanged(string idData, object objValue);
    }

    /****************************************************************************/
    /****************************************************************************/
    public class DataWatcher : IDataWatcher
    {
        private List<IDataWatcher> m_aWatchers = null;

        /****************************************************************************/
        public DataWatcher()
        {
        }

        /****************************************************************************/
        public virtual void OnValueChanged(string idData, object objValue)
        {
            if(m_aWatchers != null)
            {
                foreach(IDataWatcher objWatcher in m_aWatchers)
                    objWatcher.OnValueChanged(idData, objValue);
            }
        }

        /****************************************************************************/
        public void SubscribeToChange(IDataWatcher objWatcher)
        {
            if(m_aWatchers == null)
                m_aWatchers = new List<IDataWatcher>();

            m_aWatchers.Add(objWatcher);
        }

        /****************************************************************************/
        public void UnSubscribeToChange(IDataWatcher objWatcher)
        {
            if(m_aWatchers != null)
                m_aWatchers.Remove(objWatcher);
        }
    }
}
