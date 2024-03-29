﻿// Copyright (c) 2024 OsOmE1 - https://github.com/OsOmE1 - https://github.com/OsOmE1/Art.Net

using System.IO;
using static ArtNet.Attributes;
using ArtNet.IO;
using ArtNet.Packets.Codes;
using ArtNet.Bin2Object;

namespace ArtNet.Packets;

/// <summary>
/// The ArtRdm packet is used to send RDM control parameters over Art-Net.
/// </summary>
[OpCode(OpCode = OpCodes.OpRdm)]
public class ArtRdm : ArtNetPacket
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
    /// Art-Net Devices that only support RDM DRAFT V1.0 set field to 0x00.
    /// Devices that support RDM STANDARD V1.0 set field to 0x01.
    /// </summary>
    public byte RdmVer;
    /// <summary>
    /// Pad length to match ArtPoll.
    /// </summary>
    public byte Filler;
    /// <summary>
    /// Transmit as zero, receivers don’t test.
    /// </summary>
    [ArrayLength(FixedSize = 7)]
    public byte[] Spare;
    /// <summary>
    /// The top 7 bits of 15 bit Port-Address that should action this command.
    /// </summary>
    public byte Net;
    /// <summary>
    /// Defines the packet action.
    /// </summary>
    public RdmCodes Command;
    /// <summary>
    /// The low 8 bits of the Port-Address that should action this command.
    /// </summary>
    public byte Address;
    /// <summary>
    /// The RDM data packet excluding the DMX StartCode.
    /// <para>Variable length</para>
    /// </summary>
    [SkipBin2Object]
    public byte[] RdmPacket;

    public ArtRdm() : base(OpCodes.OpRdm)
    {

    }

    public new static ArtRdm FromData(ArtNetData data)
    {
        var stream = new MemoryStream(data.Buffer);
        var reader = new BinaryObjectReader(stream)
        {
            Position = 10
        };

        var packet = reader.ReadObject<ArtRdm>();

        packet.RdmPacket = reader.ReadBytes((int)(stream.Length - reader.Position));

        packet.PacketData = data;

        return packet;
    }

    public override byte[] ToArray()
    {
        var stream = new MemoryStream();
        var writer = new BinaryObjectWriter(stream);
        writer.Write(Id);
        writer.Write((short)OpCode);

        writer.WriteObject(this);

        writer.WriteEndianBytes(RdmPacket);
        return stream.ToArray();
    }
}