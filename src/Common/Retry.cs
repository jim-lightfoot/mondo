/****************************************************************************/
/*                                                                          */
/*    The Mondo Libraries  							                        */
/*                                                                          */
/*        Namespace: Mondo.Common							                */
/*             File: Retry.cs										        */
/*        Class(es): RetryPolicy, Retry								        */
/*          Purpose: Retry a task until it succeeds                         */
/*                                                                          */
/*  Original Author: Jim Lightfoot                                          */
/*    Creation Date: 11 Jan 2015                                            */
/*                                                                          */
/*   Copyright (c) 2015-2016 - Jim Lightfoot, All rights reserved           */
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

namespace Mondo.Common
{    
    /*************************************************************************/
    /*************************************************************************/
    public class RetryPolicy
    {
        public const int kDefault = -1;

        /*************************************************************************/
        public RetryPolicy(int iMaxRetries = kDefault, int iStartRetryWait = kDefault, double dRetryWaitIncrementFactor = kDefault)
        {
            this.MaxRetries               = iMaxRetries               == kDefault ? 4 : iMaxRetries;
            this.RetryWait                = iStartRetryWait           == kDefault ? 50 : iStartRetryWait;
            this.RetryWaitIncrementFactor = dRetryWaitIncrementFactor == kDefault ? 2d : dRetryWaitIncrementFactor;
        }

        /*************************************************************************/
        private int    m_iMaxRetries;
        private int    m_iRetryWait;
        private double m_dWaitIncrementFactor;

        /*************************************************************************/
        public int MaxRetries           
        { 
            get { return(m_iMaxRetries); }
            set { m_iMaxRetries = Math.Min(Math.Max(value, 1), 12);} 
        }

        /*************************************************************************/
        public int RetryWait           
        { 
            get { return(m_iRetryWait); }
            set { m_iRetryWait = Math.Min(Math.Max(value, 20), 400);} 
        }

        /*************************************************************************/
        public double RetryWaitIncrementFactor           
        { 
            get { return(m_dWaitIncrementFactor); }
            set { m_dWaitIncrementFactor = Math.Min(Math.Max(value, 1), 4);} 
        }

        /*************************************************************************/
        public virtual bool ShouldRetry(Exception ex)           
        { 
            return(true);
        }
    }

    /*************************************************************************/
    /*************************************************************************/
    public static class Retry
    {
        /*************************************************************************/
        public static void Run(Action fnAction, RetryPolicy policy)
        {        
            Exception exLog      = null;
            int       iRetryWait = policy.RetryWait;
            int       nRetries   = policy.MaxRetries;

            while(nRetries-- > 0)
            { 
                try
                {
                    fnAction();

                    return;
                }
                catch(Exception ex)
                {
                    if(!policy.ShouldRetry(ex))
                        throw;

                    exLog = ex;
                    Thread.Sleep(iRetryWait);

                    iRetryWait = (int)(iRetryWait * policy.RetryWaitIncrementFactor);
                }
            }

            throw exLog;
        }
        
        /*************************************************************************/
        public static void Run(Action fnAction, int iMaxRetries = RetryPolicy.kDefault, int iStartRetryWait = RetryPolicy.kDefault, double dRetryWaitIncrementFactor = RetryPolicy.kDefault)
        {
            Run(fnAction, new RetryPolicy(iMaxRetries, iStartRetryWait, dRetryWaitIncrementFactor));
        }

        /*************************************************************************/
        public static async Task RunAsync(Func<Task> fnAction, RetryPolicy policy)
        {        
            Exception exLog      = null;
            int       iRetryWait = policy.RetryWait;
            int       nRetries   = policy.MaxRetries;

            while(nRetries-- > 0)
            { 
                try
                {
                    await fnAction();

                    return;
                }
                catch(Exception ex)
                {
                    if(!policy.ShouldRetry(ex))
                        throw;

                    exLog = ex;
                }

                await Task.Delay(iRetryWait);

                iRetryWait = (int)(iRetryWait * policy.RetryWaitIncrementFactor);
            }

            throw exLog;
        }
        
        /*************************************************************************/
        public static async Task RunAsync(Func<Task> fnAction, int iMaxRetries = RetryPolicy.kDefault, int iStartRetryWait = RetryPolicy.kDefault, double dRetryWaitIncrementFactor = RetryPolicy.kDefault)
        {
            await RunAsync(fnAction, new RetryPolicy(iMaxRetries, iStartRetryWait, dRetryWaitIncrementFactor));
        }
   }
}
