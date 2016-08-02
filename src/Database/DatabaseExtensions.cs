/****************************************************************************/
/*                                                                          */
/*    The Mondo Libraries  							                        */
/*                                                                          */
/*        Namespace: Mondo.Database							                */
/*             File: DatabaseExtensions.cs								    */
/*        Class(es): DatabaseExtensions								        */
/*          Purpose: Database helper functions                              */
/*                                                                          */
/*  Original Author: Jim Lightfoot                                          */
/*    Creation Date: 1 Aug 2016                                             */
/*                                                                          */
/*   Copyright (c) 2016 - Jim Lightfoot, All rights reserved                */
/*                                                                          */
/*  Licensed under the MIT license:                                         */
/*    http://www.opensource.org/licenses/mit-license.php                    */
/*                                                                          */
/****************************************************************************/

using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;

using Mondo.Common;

namespace Mondo.Database
{
    /****************************************************************************/
    /****************************************************************************/
    public static class DatabaseExtensions
    {
        /****************************************************************************/
        public static IList<T> ToList<T>(this DataTable table, bool dispose = true) where T : new()
        {
            DataSourceList dataSource = DataSourceList.Create(table);

            IList<T> list = dataSource.ToList<T>();

            if(dispose)
              table.Dispose();

            return list;
        }        
        
    }
}
