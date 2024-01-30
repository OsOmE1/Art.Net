// Copyright (c) 2024 OsOmE1 - https://github.com/OsOmE1 - https://github.com/OsOmE1/Art.Net

using System;
using System.IO;
using static ArtNet.Attributes;
using ArtNet.IO;
using ArtNet.Packets.Codes;
using ArtNet.Bin2Object;

namespace ArtNet.Packets;

[OpCode(OpCode = OpCodes.OpIpProgReply)]
public class ArtIpProgReply() : ArtNetPacket(OpCodes.OpIpProgReply)
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
    /// Pad length to match ArtPoll.
    /// </summary>
    [ArrayLength(FixedSize = 4)]
    public byte[] Filler;
    /// <summary>
    /// IP Address of Node.
    /// </summary>
    [ArrayLength(FixedSize = 4)]
    public byte[] ProgIp;
    /// <summary>
    /// Subnet mask of Node.
    /// </summary>
    [ArrayLength(FixedSize = 4)]
    public byte[] ProgSm;
    [Obsolete("(Deprecated)")]
    public byte[] ProgPort;
    /// <remarks>
    /// Refer to the ArtNet User Guide on how to use.
    /// <see href="https://artisticlicence.com/WebSiteMaster/User%20Guides/art-net.pdf#page=40"></see>
    /// </remarks>
    public byte Status;
    /// <summary>
    /// Transmit as zero, receivers don’t test.
    /// </summary>
    public byte Spare2;
    /// <summary>
    /// Default Gateway of Node.
    /// </summary>
    [ArrayLength(FixedSize = 4)]
    public byte[] ProgDg;
    /// <summary>
    /// Transmit as zero, receivers don’t test.
    /// </summary>
    [ArrayLength(FixedSize = 2)]
    public byte[] Spare78;

    public new static ArtIpProgReply FromData(ArtNetData data)
    {
        var stream = new MemoryStream(data.Buffer);
        var reader = new BinaryObjectReader(stream)
        {
            Position = 10
        };

        var packet = reader.ReadObject<ArtIpProgReply>();

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
        return stream.ToArray();
    }
}