/****************************************************************************/
/*                                                                          */
/*    The Mondo Libraries  							                        */
/*                                                                          */
/*        Namespace: Mondo.Common							                */
/*             File: IDataClient.cs				                            */
/*        Class(es): IDataClient, DataClientCollection, IDataServer         */
/*          Purpose: An object that wants to be notified when some data is  */
/*                      modified                                            */
/*                                                                          */
/*  Original Author: Jim Lightfoot                                          */
/*    Creation Date: 30 Jan 2006                                            */
/*                                                                          */
/*   Copyright (c) 2003-2007 - Tenth Generation Software, LLC               */
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
    public interface IDataClient
    {
        void OnUpdateData(string strContext);
    }

    /****************************************************************************/
    /****************************************************************************/
    public interface IDataServer
    {
        void AddClient(IDataClient objClient);
        void NotifyClients(string strContext);
    }

    /****************************************************************************/
    /****************************************************************************/
    public class DataClientCollection : List<IDataClient>, IDataServer
    {
        /****************************************************************************/
        public DataClientCollection()
        {
        }

        /****************************************************************************/
        public void AddClient(IDataClient objNew)
        {
            foreach(IDataClient objClient in this)
                if(objClient == objNew)
                    return;

            this.Add(objNew);
        }

        /****************************************************************************/
        public void NotifyClients(string strContext)
        {
            foreach(IDataClient objClient in this)
                objClient.OnUpdateData(strContext);
        }
    }
}
