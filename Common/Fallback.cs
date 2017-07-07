/****************************************************************************/
/*                                                                          */
/*    The Mondo Libraries  							                        */
/*                                                                          */
/*        Namespace: Mondo.Common							                */
/*             File: Fallback.cs								            */
/*        Class(es): Fallback								                */
/*          Purpose: Runs thru a list of params and calls a given callback  */
/*                      with each one until one succeeds                    */
/*                                                                          */
/*  Original Author: Jim Lightfoot                                          */
/*    Creation Date: 14 Sep 2016                                            */
/*                                                                          */
/*   Copyright (c) 2016-2017 - Jim Lightfoot, All rights reserved           */
/*                                                                          */
/*  Licensed under the MIT license:                                         */
/*    http://www.opensource.org/licenses/mit-license.php                    */
/*                                                                          */
/****************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mondo.Common
{
    /****************************************************************************/
    public static class Fallback
    {
        /****************************************************************************/
        public static T Run<L, T>(IEnumerable<L> paramList, Func<L, T> fn)
        {
            Exception exception = null;

            foreach(var item in paramList)
            {
                try
                {
                    return fn(item);
                }
                catch (Exception ex)
                {
                    exception = ex;
                }
            }

            throw exception;
        }
    }
}
