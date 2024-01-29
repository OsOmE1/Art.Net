﻿using System;
using System.IO;
using static ArtNet.Attributes;
using ArtNet.IO;
using ArtNet.Packets.Codes;
using NoisyCowStudios.Bin2Object;

namespace ArtNet.Packets
{
    /// <summary>
    /// The ArtDataRequest packet is used to request data such as product URL
    /// The ArtDataRequest packet is unicast by a Controller to a Node.
    /// If the Node supports this feature it will respond by unicasting an ArtDataReply.
    /// In all scenarios, the ArtDataReply is unicast to the IP address of the sender
    /// </summary>
    [OpCode(OpCode = OpCodes.OpDataRequest)]
    public class ArtDataRequest : ArtNetPacket
    {
        /// <summary>
        /// High byte of the Art-Net protocol revision number.
        /// </summary>
        public byte ProtVerHi;
        /// <summary>
        /// Low byte of the Art-Net protocol revision number.
        /// Controllers should ignore communication with nodes using a protocol version lower than current version. 
        /// </summary>
        /// <value> 14 </value>
        public byte ProtVerLo;
        /// <summary>
        /// The ESTA manufacturer code.
        /// These codes are used to represent equipment manufacturer.
        /// They are assigned by ESTA.
        /// This field can be interpreted as two ASCII bytes representing the manufacturer initials.
        /// </summary>
        public byte EstaManHi;
        /// <summary>
        /// Hi byte of above
        /// </summary>
        public byte EstaManLo;
        /// <summary>
        /// The high byte of the Oem code.
        /// </summary>
        public byte OemHi;
        /// <summary>
        /// The low byte of the Oem code.
        /// The Oem code uniquely identifies the product sending this packet.
        /// </summary>
        public byte OemLo;
        /// <summary>
        /// The data being requested. Hi byte. See DataRequestCodes
        /// </summary>
        public byte RequestHi;
        /// <summary>
        /// The data being requested. Lo byte.
        /// </summary>
        public byte RequestLo;
        /// <summary>
        /// Transmit as zero, receivers don’t test.
        /// </summary>
        [ArrayLength(FixedSize = 22)]
        public string Spare;

        public int EstaMan
        {
            get => EstaManLo | EstaManHi << 8;
            set
            {
                EstaManLo = (byte)(value & 0xFF);
                EstaManHi = (byte)(value >> 8);
            }
        }

        public int Oem
        {
            get => OemLo | OemHi << 8;
            set
            {
                OemLo = (byte)(value & 0xFF);
                OemHi = (byte)(value >> 8);
            }
        }

        public DataRequestCodes Request
        {
            get => (DataRequestCodes)(RequestLo | RequestHi << 8);
            set
            {
                RequestHi = (byte)((ushort)value & 0xFF);
                RequestLo = (byte)((ushort)value >> 8);
            }
        }

        public ArtDataRequest() : base(OpCodes.OpCommand)
        {

        }

        public static new ArtDataRequest FromData(ArtNetData data)
        {
            var stream = new MemoryStream(data.Buffer);
            var reader = new BinaryObjectReader(stream)
            {
                Position = 10
            };

            ArtDataRequest packet = reader.ReadObject<ArtDataRequest>();

            packet.PacketData = data;

            return packet;
        }

        public override byte[] ToArray()
        {
            var stream = new MemoryStream();
            var writer = new BinaryObjectWriter(stream);
            writer.Write(ID);
            writer.Write((short)OpCode);

            writer.WriteObject(this);
            return stream.ToArray();
        }
    }
}