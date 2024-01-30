// Copyright (c) 2024 OsOmE1 - https://github.com/OsOmE1 - https://github.com/OsOmE1/Art.Net

using System.IO;
using static ArtNet.Attributes;
using ArtNet.IO;
using ArtNet.Packets.Codes;
using ArtNet.Bin2Object;

namespace ArtNet.Packets;

/// <summary>
/// The ArtCommand packet is used to send property set style commands.
/// The packet can be unicast or broadcast, the decision being application specific.
/// </summary>
[OpCode(OpCode = OpCodes.OpCommand)]
public class ArtCommand() : ArtNetPacket(OpCodes.OpCommand)
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
    /// The length of the text array below. High Byte
    /// </summary>
    public byte LengthHi;
    /// <summary>
    /// Low Byte.
    /// </summary>
    public byte LengthLo;
    /// <summary>
    /// ASCII text array, null terminated.
    /// Max length is 512 bytes including the null terminator.
    /// </summary>
    [ArrayLength(FieldName = "Length")]
    public string Data;

    public int Length
    {
        get => LengthLo | LengthHi << 8;
        set
        {
            LengthLo = (byte)(value & 0xFF);
            LengthHi = (byte)(value >> 8);
        }
    }

    public new static ArtCommand FromData(ArtNetData data)
    {
        var stream = new MemoryStream(data.Buffer);
        var reader = new BinaryObjectReader(stream)
        {
            Position = 10
        };

        var packet = reader.ReadObject<ArtCommand>();

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