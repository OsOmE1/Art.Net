// Copyright (c) 2024 OsOmE1 - https://github.com/OsOmE1 - https://github.com/OsOmE1/Art.Net

using System.IO;
using static ArtNet.Attributes;
using ArtNet.IO;
using ArtNet.Packets.Codes;
using ArtNet.Bin2Object;

namespace ArtNet.Packets;

/// <summary>
/// The ArtTodControl packet is used to send RDM control parameters over Art-Net.
/// The response is ArtTodData.
/// </summary>
[OpCode(OpCode = OpCodes.OpTodControl)]
public class ArtTodControl() : ArtNetPacket(OpCodes.OpTodControl)
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
    [ArrayLength(FixedSize = 2)]
    public byte[] Filler;
    /// <summary>
    /// Transmit as zero, receivers don’t test.
    /// </summary>
    [ArrayLength(FixedSize = 7)]
    public byte[] Spare;
    /// <summary>
    /// The top 7 bits of the Port-Address of the Output Gateway DMX Port that should action this command.
    /// </summary>
    public byte Net;
    /// <summary>
    /// Defines the packet contents.
    /// </summary>
    public TodControlCodes Command;
    /// <summary>
    /// The low byte of the 15 bit Port-Address of the DMX Port that should action this command.
    /// </summary>
    public byte Address;

    public new static ArtTodControl FromData(ArtNetData data)
    {
        var stream = new MemoryStream(data.Buffer);
        var reader = new BinaryObjectReader(stream)
        {
            Position = 10
        };

        var packet = reader.ReadObject<ArtTodControl>();

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