﻿// Copyright (c) 2024 OsOmE1 - https://github.com/OsOmE1 - https://github.com/OsOmE1/Art.Net
// Copyright (c) 2024 Quphoria - http://quphoria.co.uk - https://github.com/OsOmE1/Art.Net

using System;
using System.IO;
using static ArtNet.Attributes;
using ArtNet.IO;
using ArtNet.Packets.Codes;
using ArtNet.Bin2Object;
using System.Buffers.Binary;

namespace ArtNet.Packets;

public struct TalkToMe
{
    public bool Vlc;
    /// <summary>
    /// <para>true = Diagnostics messages are unicast.</para>
    /// <para>false = Diagnostics messages are broadcast.</para>
    /// </summary>
    public bool DiagCast;
    public bool SendDiagnostics;
    /// <summary>
    /// <para>true = Send ArtPollReply whenever Node conditions change. This selection allows the Controller to be informed of changes without the need to continuously poll.</para>
    /// <para>false = Only send ArtPollReply in response to an ArtPoll or ArtAddress.</para>
    /// </summary>
    public bool ReplyOnChange;
}

[OpCode(OpCode = OpCodes.OpPoll)]
public class ArtPoll() : ArtNetPacket(OpCodes.OpPoll)
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
    /// Set behaviour of Node
    /// </summary>
    [SkipBin2Object]
    public TalkToMe TalkToMe;
    /// <summary>
    /// Holds the Flags stored in TalkToMe
    /// </summary>
    public byte Flags;
    /// <summary>
    /// The lowest priority of diagnostics message that should be sent. See <see cref="PriorityCodes"/>.
    /// </summary>
    public PriorityCodes Priority;
    /// <summary>
    /// Top of the range of Port-Addresses to be tested if Targeted Mode is active.
    /// </summary>
    /// <remark>This is optional, consider zero if packet is too short</remarK>
    [SkipBin2Object]
    public int AddressTop;
    /// <summary>
    /// Bottom of the range of Port-Addresses to be tested if Targeted Mode is active.
    /// </summary>
    /// <remark>This is optional, consider zero if packet is too short</remarK>
    [SkipBin2Object]
    public int AddressBottom;
    /// <summary>
    /// The ESTA Manufacturer Code is assigned by ESTA and uniquely identifies the manufacturer that generated this packet.
    /// </summary>
    [SkipBin2Object]
    public int EstaMan;
    /// <summary>
    /// The Oem code uniquely identifies the product sending this packet.
    /// </summary>
    [SkipBin2Object]
    public int Oem;

    public int ProtVer
    {
        get => ProtVerLo | ProtVerHi << 8;
        set
        {
            ProtVerLo = (byte)(value & 0xFF);
            ProtVerHi = (byte)(value >> 8);
        }
    }

    public static new ArtPoll FromData(ArtNetData data)
    {
        var stream = new MemoryStream(data.Buffer);
        var reader = new BinaryObjectReader(stream)
        {
            Position = 10,
        };

        var packet = reader.ReadObject<ArtPoll>();
        packet.TalkToMe = new TalkToMe
        {
            Vlc = (packet.Flags & (1 << 4)) > 0,
            DiagCast = (packet.Flags & (1 << 3)) > 0,
            SendDiagnostics = (packet.Flags & (1 << 2)) > 0,
            ReplyOnChange = (packet.Flags & (1 << 1)) > 0,
        };

        // read optional bytes
        // ReadUint16 reads in little endian, convert to big
        if (stream.Length - reader.Position >= 2)
            packet.AddressTop = BinaryPrimitives.ReverseEndianness(reader.ReadUInt16());
        if (stream.Length - reader.Position >= 2)
            packet.AddressBottom = BinaryPrimitives.ReverseEndianness(reader.ReadUInt16());
        if (stream.Length - reader.Position >= 2)
            packet.EstaMan = BinaryPrimitives.ReverseEndianness(reader.ReadUInt16());
        if (stream.Length - reader.Position >= 2)
            packet.Oem = BinaryPrimitives.ReverseEndianness(reader.ReadUInt16());
        packet.PacketData = data;
        return packet;
    }


    public override byte[] ToArray()
    {
        Flags = 0;
        if (TalkToMe.Vlc)
            Flags |= 1 << 4;
        if (TalkToMe.DiagCast)
            Flags |= 1 << 3;
        if (TalkToMe.SendDiagnostics)
            Flags |= 1 << 2;
        if (TalkToMe.ReplyOnChange)
            Flags |= 1 << 1;

        var stream = new MemoryStream();
        var writer = new BinaryObjectWriter(stream);
        writer.Write(Id);
        writer.Write((short)OpCode);

        writer.WriteObject(this);

        // write optional extra bytes
        writer.Write((byte)((AddressTop >> 8) & 0xff));
        writer.Write((byte)(AddressTop & 0xff));
        writer.Write((byte)((AddressBottom >> 8) & 0xff));
        writer.Write((byte)(AddressBottom & 0xff));
        writer.Write((byte)((EstaMan >> 8) & 0xff));
        writer.Write((byte)(EstaMan & 0xff));
        writer.Write((byte)((Oem >> 8) & 0xff));
        writer.Write((byte)(Oem & 0xff));

        return stream.ToArray();
    }

    public override string ToString() =>
        base.ToString() + $"ProtVerHi: {ProtVerHi}\nProtVerLo: {ProtVerLo}\n" +
        "TalkToMe:\n" +
        $"\tVLC: {(TalkToMe.Vlc ? "true" : "false")}\n" +
        $"\tDiagCast: {(TalkToMe.DiagCast ? "unicast" : "broadcast")}\n" +
        $"\tSendDiagnostics: {(TalkToMe.SendDiagnostics ? "true" : "false")}\n" +
        $"\tReplyOnChange: {(TalkToMe.ReplyOnChange ? "true" : "false")}\n" +
        $"Priority: {Enum.GetName(typeof(PriorityCodes), Priority)}({Priority:X})\n";
}