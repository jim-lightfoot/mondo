/****************************************************************************/
/*                                                                          */
/*    The Mondo Libraries  							                        */
/*                                                                          */
/*        Namespace: Mondo.Common							                */
/*             File: IMessageQueue.cs										*/
/*        Class(es): IMessageQueue				  				            */
/*          Purpose: Interface for message queues                           */
/*                                                                          */
/*  Original Author: Jim Lightfoot                                          */
/*    Creation Date: 8 Nov 2015                                             */
/*                                                                          */
/*   Copyright (c) 2015 - Jim Lightfoot,  All rights reserved               */
/*                                                                          */
/*  Licensed under the MIT license:                                         */
/*    http://www.opensource.org/licenses/mit-license.php                    */
/*                                                                          */
/****************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace Mondo.Common
{
    /****************************************************************************/
    /****************************************************************************/
    public interface IMessageQueue
    {
        void SendMessage(string message);
        void Clear();

        Task ProcessMessagesAsync(IMessageProcessor processor, CancellationToken cancelToken);
        void ProcessMessages(IMessageProcessor processor, CancellationToken cancelToken);
    }

    /****************************************************************************/
    /****************************************************************************/
    public interface IMessageBatch : IDisposable
    {
        string Id { get; }
    }

    /****************************************************************************/
    /****************************************************************************/
    public interface IMessageProcessor
    {
        void ProcessMessage(string message, IMessageBatch batch);

        bool HandleMessageError(Exception ex, string message);

        IMessageBatch StartBatch();
    }
}
