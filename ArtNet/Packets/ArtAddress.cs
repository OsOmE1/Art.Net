﻿// Copyright (c) 2024 OsOmE1 - https://github.com/OsOmE1 - https://github.com/OsOmE1/Art.Net

using System.IO;
using static ArtNet.Attributes;
using ArtNet.IO;
using ArtNet.Packets.Codes;
using ArtNet.Bin2Object;

namespace ArtNet.Packets;

/// <summary>
/// A Controller or monitoring device on the network can reprogram numerous controls of a node remotely.
/// This, for example, would allow the lighting console to re-route DMX512 data at remote locations.
/// This is achieved by sending an <see cref="ArtAddress"/> packet to the Node’s IP address. (The IP address is returned in the <see cref="ArtPoll"/> packet).
/// The node replies with an <see cref="ArtPollReply"/> packet.
/// </summary>
[OpCode(OpCode = OpCodes.OpAddress)]
public class ArtAddress() : ArtNetPacket(OpCodes.OpAddress)
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
    /// Bits 14-8 of the 15 bit Port-Address are encoded into the bottom 7 bits of this field.
    /// This is used in combination with SubSwitch and SwIn[] or SwOut[] to produce the full universe address.
    /// This value is ignored unless bit 7 is high.i.e.to program a value 0x07, send the value as 0x87.
    /// Send 0x00 to reset this value to the physical switch setting.
    /// <para>Use value 0x7f for no change</para>
    /// </summary>
    public byte NetSwitch;
    /// <summary>
    /// The BindIndex defines the bound node which originated this packet and is used to uniquely identify the bound node when identical IP addresses are in use.
    /// This number represents the order of bound devices.
    /// A lower number means closer to root device.
    /// <para>A value of 1 means root device.</para>
    /// </summary>
    public byte BindIndex;
    /// <summary>
    /// The array represents a null terminated port name for port of this Node.
    /// The Controller uses the ArtAddress packet to program this string.
    /// Max length is 17 characters plus the null.
    /// The Node will ignore this value if the string is null.
    /// <para>This is a fixed length field, although the string it contains can be shorter than the field.</para>
    /// </summary>
    [String(FixedSize = 18)]
    public string PortName;
    /// <summary>
    /// The array represents a null terminated long name for the Node.
    /// The Controller uses the ArtAddress packet to program this string.
    /// Max length is 63 characters plus the null.
    /// The Node will ignore this value if the string is null.
    /// <para>This is a fixed length field, although the string it contains can be shorter than the field.</para>
    /// </summary>
    [String(FixedSize = 64)]
    public string LongName;
    /// <summary>
    /// Bits 3-0 of the 15 bit Port-Address for a given input port are encoded into the bottom 4 bits of this field.
    /// This is used in combination with NetSwitch and SubSwitch to produce the full universe address.
    /// This value is ignored unless bit 7 is high.i.e.to program a value 0x07, send the value as 0x87.
    /// <para>Send 0x00 to reset this value to the physical switch setting.</para>
    /// <para>Use value 0x7f for no change.</para>
    /// </summary>
    [ArrayLength(FixedSize = 4)]
    public byte[] SwIn;
    /// <summary>
    /// Bits 3-0 of the 15 bit Port-Address for a given output port are encoded into the bottom 4 bits of this field.
    /// This is used in combination with NetSwitch and SubSwitch to produce the full universe address.
    /// This value is ignored unless bit 7 is high.i.e.to program a value 0x07, send the value as 0x87.
    /// <para>Send 0x00 to reset this value to the physical switch setting.</para>
    /// <para>Use value 0x7f for no change.</para>
    /// </summary>
    [ArrayLength(FixedSize = 4)]
    public byte[] SwOut;
    /// <summary>
    /// Bits 7-4 of the 15 bit Port-Address are encoded into the bottom 4 bits of this field.
    /// This is used in combination with NetSwitch and SwIn[] or SwOut[] to produce the full universe address.
    /// This value is ignored unless bit 7 is high.i.e.to program a value 0x07, send the value as 0x87.
    /// <para>Send 0x00 to reset this value to the physical switch setting.</para>
    /// <para>Use value 0x7f for no change.</para>
    /// </summary>
    public byte SubSwitch;
    /// <summary>
    /// Sets the sACN Priority field for sACN generated on all 4 ports encoded into this packet.
    /// A value of 255 represents no change.
    /// Values of 0 to 200 inclusive are valid.
    /// </summary>
    public byte AcnPriority;
    /// <summary>
    /// Node configuration commands:
    /// </summary>
    /// <remarks>
    /// Refer to the ArtNet <see href="https://artisticlicence.com/WebSiteMaster/User%20Guides/art-net.pdf#page=43">User Guide</see> on how to use.
    /// </remarks>
    public AddressCommands Command;

    public new static ArtAddress FromData(ArtNetData data)
    {
        var stream = new MemoryStream(data.Buffer);
        var reader = new BinaryObjectReader(stream)
        {
            Position = 10
        };

        var packet = reader.ReadObject<ArtAddress>();

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