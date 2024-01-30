// Copyright (c) 2024 OsOmE1 - https://github.com/OsOmE1 - https://github.com/OsOmE1/Art.Net
// Copyright (c) 2024 Quphoria - http://quphoria.co.uk - https://github.com/OsOmE1/Art.Net

using System;
using System.IO;
using static ArtNet.Attributes;
using ArtNet.IO;
using ArtNet.Packets.Codes;
using ArtNet.Bin2Object;

namespace ArtNet.Packets;

public struct IpProgCommand
{
    /// <summary>
    /// Set to enable any programming
    /// </summary>
    public bool EnableAnyProgramming;
    /// <summary>
    /// Set to enable DHCP (if set ignore lower bits).
    /// </summary>
    public bool EnableDhcp;
    /// <summary>
    /// Set to program the default gateway address
    /// </summary>
    public bool ProgramDefaultGateway;
    /// <summary>
    /// Set to return all three parameters to default
    /// </summary>
    public bool ReturnToDefault;
    /// <summary>
    /// Set to program the IP address
    /// </summary>
    public bool ProgramIpAddress;
    /// <summary>
    /// Set to program the subnet mask
    /// </summary>
    public bool ProgramSubnetMask;
    /// <summary>
    /// Set to program the art-net port (Deprecated)
    /// </summary>
    public bool ProgramPort;
}

[OpCode(OpCode = OpCodes.OpIpProg)]
public class ArtIpProg() : ArtNetPacket(OpCodes.OpIpProg)
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
    /// Defines the how this packet is processed. If all bits are clear, this is an enquiry only.
    /// </summary>
    [SkipBin2Object]
    public IpProgCommand IpProgCommand;
    /// <summary>
    /// Holds the flags stored in IpProgCommand
    /// </summary>
    public byte Command;
    /// <summary>
    /// Set to zero. Pads data structure for word alignment
    /// </summary>
    public byte Filler4;
    /// <summary>
    /// IP Address to be programmed into Node if enabled by Command Field
    /// </summary>
    [ArrayLength(FixedSize = 4)]
    public byte[] ProgIp;
    /// <summary>
    /// Subnet mask to be programmed into Node if enabled by Command Field
    /// </summary>
    [ArrayLength(FixedSize = 4)]
    public byte[] ProgSm;
    /// <summary>
    /// Artnet port to be programmed into Node if enabled by Command Field (Deprecated)
    /// </summary>
    [Obsolete("(Deprecated)")]
    [ArrayLength(FixedSize = 2)]
    public byte[] ProgPort;
    /// <summary>
    /// Default Gateway to be programmed into Node if enabled by Command Field
    /// </summary>
    [ArrayLength(FixedSize = 4)]
    public byte[] ProgDg;
    /// <summary>
    /// Transmit as zero, receivers don’t test.
    /// </summary>
    [ArrayLength(FixedSize = 4)]
    public byte[] Spare;

    public new static ArtIpProg FromData(ArtNetData data)
    {
        var stream = new MemoryStream(data.Buffer);
        var reader = new BinaryObjectReader(stream)
        {
            Position = 10
        };

        var packet = reader.ReadObject<ArtIpProg>();
        packet.IpProgCommand = new IpProgCommand
        {
            EnableAnyProgramming = (packet.Command & (1 << 7)) > 0,
            EnableDhcp = (packet.Command & (1 << 6)) > 0,
            ProgramDefaultGateway = (packet.Command & (1 << 4)) > 0,
            ReturnToDefault = (packet.Command & (1 << 3)) > 0,
            ProgramIpAddress = (packet.Command & (1 << 2)) > 0,
            ProgramSubnetMask = (packet.Command & (1 << 1)) > 0,
            ProgramPort = (packet.Command & (1 << 0)) > 0,
        };

        packet.PacketData = data;

        return packet;
    }

    public override byte[] ToArray()
    {
        Command = 0;
        if (IpProgCommand.EnableAnyProgramming)
            Command |= 1 << 7;
        if (IpProgCommand.EnableDhcp)
            Command |= 1 << 6;
        if (IpProgCommand.ProgramDefaultGateway)
            Command |= 1 << 4;
        if (IpProgCommand.ReturnToDefault)
            Command |= 1 << 3;
        if (IpProgCommand.ProgramIpAddress)
            Command |= 1 << 2;
        if (IpProgCommand.ProgramSubnetMask)
            Command |= 1 << 1;
        if (IpProgCommand.ProgramPort)
            Command |= 1 << 0;

        var stream = new MemoryStream();
        var writer = new BinaryObjectWriter(stream);
        writer.Write(Id);
        writer.Write((short)OpCode);

        writer.WriteObject(this);
        return stream.ToArray();
    }
}