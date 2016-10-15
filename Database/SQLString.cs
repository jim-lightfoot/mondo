/****************************************************************************/
/*                                                                          */
/*    The Mondo Libraries  							                        */
/*                                                                          */
/*        Namespace: Mondo.Database							                */
/*             File: SQLString.cs						                    */
/*        Class(es): SQLString							                    */
/*          Purpose: Build up a SQL statement                               */
/*                                                                          */
/*  Original Author: Jim Lightfoot                                          */
/*    Creation Date: 8 Jul 2005                                             */
/*                                                                          */
/*   Copyright (c) 2005-2011 - Tenth Generation Software, LLC               */
/*                          All rights reserved                             */
/*                                                                          */
/****************************************************************************/

using System;
using System.Text;
using Mondo.Common;

namespace Mondo.Database
{
    /****************************************************************************/
    /****************************************************************************/
    /// <summary>
	/// Dynamically build a SQL statement
	/// </summary>
	public class SQLString
	{
        private StringBuilder m_objBuilder = new StringBuilder();
        private bool m_bTransaction = false;

        /****************************************************************************/
        public SQLString()
		{
            Begin();
		}

        /****************************************************************************/
        public SQLString(bool bTransaction)
		{
            Begin();

            if(bTransaction)
                BeginTransaction();
		}

        /****************************************************************************/
        public void Append(string strValue)
        {
            if(m_bTransaction)
            {
                m_objBuilder.Append("IF(@@ERROR = 0)\r\n");
                Begin();
            }

            m_objBuilder.Append(strValue);

            if(m_bTransaction)
                End();
        }

        /****************************************************************************/
        public void Begin()
        {
            m_objBuilder.Append("  BEGIN\r\n");
        }

        /****************************************************************************/
        public void End()
        {
            m_objBuilder.Append("  END\r\n");
        }

        /****************************************************************************/
        public void BeginTransaction()
        {
            m_objBuilder.Append("BEGIN TRANSACTION\r\n");
            m_bTransaction = true;
        }

        /****************************************************************************/
        public void EndTransaction()
        {
            if(m_bTransaction)
            {
                m_objBuilder.Append(@"IF(@@ERROR = 0)
                                        COMMIT TRANSACTION
                                      ELSE
                                        ROLLBACK TRANSACTION" + "\r\n");

                m_bTransaction = false;
            }
        }

        /****************************************************************************/
        public override string ToString()
        {
            EndTransaction();
            End();

            return(m_objBuilder.ToString());
        }
	}
}
