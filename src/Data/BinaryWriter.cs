/****************************************************************************/
/*                                                                          */
/*    The Mondo Libraries  							                        */
/*                                                                          */
/*           Module: Mondo.Common								            */
/*             File: BinaryWriter.cs										*/
/*        Class(es): BinaryWriter, BinaryReader							    */
/*          Purpose: Reads and Writes binary files                          */
/*                                                                          */
/*  Original Author: Jim Lightfoot                                          */
/*    Creation Date: 22 Aug 2011                                            */
/*                                                                          */
/*   Copyright (c) 2011-2016 - Jim Lightfoot, All rights reserved           */
/*                                                                          */
/*  Licensed under the MIT license:                                         */
/*    http://www.opensource.org/licenses/mit-license.php                    */
/*                                                                          */
/****************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Mondo.Common
{
    /****************************************************************************/
    /****************************************************************************/
    public class BinaryWriter : System.IO.BinaryWriter
    {
        private bool m_bLittleEndian = true;

        /****************************************************************************/
        public BinaryWriter(Stream objStream) : this(objStream, true)
        {
        }

        /****************************************************************************/
        public BinaryWriter(Stream objStream, bool bLittleEndian) : base(objStream)
        {
            m_bLittleEndian = bLittleEndian;
        }

        /****************************************************************************/
        public override void Write(ushort value)
        {
            if(m_bLittleEndian)
                base.Write(value);
            else
            {
                base.Write((byte)((value & 0xFF00) >> 8));
                base.Write((byte)(value & 0x00FF));
            }
        }

        /****************************************************************************/
        public override void Write(short value)
        {
            if(m_bLittleEndian)
                base.Write(value);
            else
            {
                base.Write((byte)((value & 0xFF00) >> 8));
                base.Write((byte)(value & 0x00FF));
            }
        }

        /****************************************************************************/
        public override void Write(int value)
        {
            if(m_bLittleEndian)
                base.Write(value);
            else
            {
                base.Write((byte)((value & 0xFF000000) >> 24));
                base.Write((byte)((value & 0x00FF0000) >> 16));
                base.Write((byte)((value & 0x0000FF00) >> 8));
                base.Write((byte)(value & 0x000000FF));
            }
        }

        /****************************************************************************/
        public override void Write(uint value)
        {
            if(m_bLittleEndian)
                base.Write(value);
            else
            {
                base.Write((byte)((value & 0xFF000000) >> 24));
                base.Write((byte)((value & 0x00FF0000) >> 16));
                base.Write((byte)((value & 0x0000FF00) >> 8));
                base.Write((byte)(value & 0x000000FF));
            }
        }
    }

    /****************************************************************************/
    /****************************************************************************/
    public class BinaryReader : System.IO.BinaryReader
    {
        private bool m_bLittleEndian = true;

        /****************************************************************************/
        public BinaryReader(Stream objStream) : this(objStream, true)
        {
        }

        /****************************************************************************/
        public BinaryReader(Stream objStream, bool bLittleEndian) : base(objStream)
        {
            m_bLittleEndian = bLittleEndian;
        }

        /****************************************************************************/
        public override short ReadInt16()
        {
            if(m_bLittleEndian)
                return(base.ReadInt16());

            int b1 = (int)ReadByte();
            int b2 = (int)ReadByte();
            
            return((short)((b1 << 8) | b2));
        }

        /****************************************************************************/
        public override ushort ReadUInt16()
        {
            if(m_bLittleEndian)
                return(base.ReadUInt16());

            uint b1 = (uint)ReadByte();
            uint b2 = (uint)ReadByte();
            
            return((ushort)((b1 << 8) | b2));
        }

        /****************************************************************************/
        public override int ReadInt32()
        {
            if(m_bLittleEndian)
                return(base.ReadInt32());

            int  b1 = (int)ReadByte();
            int  b2 = (int)ReadByte();
            int  b3 = (int)ReadByte();
            int  b4 = (int)ReadByte();

            return((b1 << 24) | (b2 << 16) | (b3 << 8) | b4);
        }

        /****************************************************************************/
        public override uint ReadUInt32()
        {
            if(m_bLittleEndian)
                return(base.ReadUInt32());

            uint  b1 = (uint)ReadByte();
            uint  b2 = (uint)ReadByte();
            uint  b3 = (uint)ReadByte();
            uint  b4 = (uint)ReadByte();

            return((b1 << 24) | (b2 << 16) | (b3 << 8) | b4);
        }

        /****************************************************************************/
        public override long ReadInt64()
        {
            if(m_bLittleEndian)
                return(base.ReadInt64());

            int  b1 = (int)ReadByte();
            int  b2 = (int)ReadByte();
            int  b3 = (int)ReadByte();
            int  b4 = (int)ReadByte();
            int  b5 = (int)ReadByte();
            int  b6 = (int)ReadByte();
            int  b7 = (int)ReadByte();
            int  b8 = (int)ReadByte();

            return((b1 << 56) | (b2 << 48) | (b3 << 40) | (b4 << 32) | (b5 << 24) | (b6 << 16) | (b7 << 8) | b8);
        }

        /****************************************************************************/
        public override ulong ReadUInt64()
        {
            if(m_bLittleEndian)
                return(base.ReadUInt64());

            uint  b1 = (uint)ReadByte();
            uint  b2 = (uint)ReadByte();
            uint  b3 = (uint)ReadByte();
            uint  b4 = (uint)ReadByte();
            uint  b5 = (uint)ReadByte();
            uint  b6 = (uint)ReadByte();
            uint  b7 = (uint)ReadByte();
            uint  b8 = (uint)ReadByte();

            return((b1 << 56) | (b2 << 48) | (b3 << 40) | (b4 << 32) | (b5 << 24) | (b6 << 16) | (b7 << 8) | b8);
        }

    }
}
