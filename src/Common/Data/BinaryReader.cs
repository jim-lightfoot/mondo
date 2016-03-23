/****************************************************************************/
/*                                                                          */
/*    The Mondo Libraries  							                        */
/*                                                                          */
/*        Namespace: Mondo.Common							                */
/*             File: BinaryReader.cs										*/
/*        Class(es): BinaryReader										    */
/*          Purpose: Reads values from a byte array                         */
/*                                                                          */
/*  Original Author: Jim Lightfoot                                          */
/*    Creation Date: 21 May 2013                                            */
/*                                                                          */
/*   Copyright (c) 2013 - Tenth Generation Software, LLC                    */
/*                              All rights reserved                         */
/*                                                                          */
/****************************************************************************/

using System;

namespace Mondo.Common
{
    /****************************************************************************/
    /****************************************************************************/
    public class BinaryReader
    {
        private int             m_iCursor = 0;
        private readonly byte[] m_aData;
        private readonly int    m_iLength;

        /****************************************************************************/
        public BinaryReader(byte[] aData)
        {
            if(aData == null)
                throw new Empty();

            m_aData   = aData;
            m_iLength = aData.Length;

            if(m_iLength == 0)
                throw new Empty();
        }

        /****************************************************************************/
        public bool End
        {
            get
            {
                return(m_iCursor == m_iLength);
            }
        }

        /****************************************************************************/
        public class Empty : Exception {}
        public class OutOfRangeException : Exception {}
        public class ArgumentOutOfRangeException : Exception {}

        /****************************************************************************/ 
        public int ReadInt()
        {
            int  b1 = ReadByteAsInt();
            int  b2 = ReadByteAsInt();
            int  b3 = ReadByteAsInt();
            int  b4 = ReadByteAsInt();

            return((b1 << 24) | (b2 << 16) | (b3 << 8) | b4);
        }

        /****************************************************************************/ 
        public int ReadInt(int nBytes)
        {
            if(nBytes > 4 || nBytes < 1)
                throw new ArgumentOutOfRangeException();

            int  b1 = ReadByteAsInt();
            int  b2 = nBytes < 2 ? 0 : ReadByteAsInt();
            int  b3 = nBytes < 3 ? 0 : ReadByteAsInt();
            int  b4 = nBytes < 4 ? 0 : ReadByteAsInt();

            return((b1 << 24) | (b2 << 16) | (b3 << 8) | b4);
        }

        /****************************************************************************/ 
        public long ReadLong()
        {
            long i1 = (long)ReadInt();
            long i2 = (long)ReadInt();

            return((i1 << 32) | i2);
        }

        /****************************************************************************/ 
        public long ReadLong(int nBytes)
        {
            if(nBytes <= 4)
                return((long)ReadInt(nBytes));

            long i1 = (long)ReadInt();
            long i2 = (long)ReadInt(nBytes - 4);

            return((i1 << 32) | i2);
        }

        /****************************************************************************/ 
        public byte ReadByte()
        {
            try
            {
                return(m_aData[m_iCursor++]);
            }
            catch(Exception ex)
            {
                throw new OutOfRangeException();
            }
        }

        /****************************************************************************/ 
        public ushort ReadUShort()
        {
            int iTop    = ReadByteAsInt();
            int iBottom = ReadByteAsInt();

            return((ushort)((iTop << 8) | iBottom));
        }

        /****************************************************************************/ 
        public int ReadByteAsInt()
        {
            return((int)ReadByte());
        }

        /****************************************************************************/ 
        public int ReadWordAsInt()
        {
            return((int)ReadUShort());
        }

        /****************************************************************************/ 
        public short ReadWordAsShort()
        {
            int   iTop    = (int)ReadByte();
            int   iBottom = (int)ReadByte();
            
            return((short)((iTop << 8) | iBottom));
        }

        /****************************************************************************/ 
        public double ReadWordAsDouble()
        {
            short iValue = ReadWordAsShort();

            return(((double)iValue) / 10d);
        }

        /****************************************************************************/ 
        public double ReadByteAsDouble()
        {
            byte  byValue  = ReadByte();
            sbyte sValue   = (sbyte)byValue;
            short iValue   = (short)sValue;

            return(((double)iValue) / 10d);
        }

        /****************************************************************************/ 
        public double ReadUByteAsDouble()
        {
            byte  byValue  = ReadByte();
            short iValue   = (short)byValue;

            return(((double)iValue) / 10d);
        }
    }
}
