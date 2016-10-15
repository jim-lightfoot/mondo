/****************************************************************************/
/*                                                                          */
/*    The Mondo Libraries  							                        */
/*                                                                          */
/*        Namespace: Mondo.Common							                */
/*             File: IService.cs									        */
/*        Class(es): IService										        */
/*          Purpose: Generic interface for a service                        */
/*                                                                          */
/*  Original Author: Jim Lightfoot                                          */
/*    Creation Date: 22 Aug 2011                                            */
/*                                                                          */
/*   Copyright (c) 2011 - Jim Lightfoot, All rights reserved                */
/*                                                                          */
/*  Licensed under the MIT license:                                         */
/*    http://www.opensource.org/licenses/mit-license.php                    */
/*                                                                          */
/****************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;

namespace Mondo.Common
{
    /****************************************************************************/
    /****************************************************************************/
    public interface IService
    {
        void Start();
        void Stop();
    }

    /****************************************************************************/
    /****************************************************************************/
    public interface ITask
    {
        void Run();
    }  
}
