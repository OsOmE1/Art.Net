using ArtNet.IO;
using ArtNet.Packets;
using ArtNet.Packets.Codes;
using NUnit.Framework;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;

namespace Tests;

[TestFixture]
public class Tests
{
    [Test]
    public void TestPacketSerialization()
    {
        var data = new ArtNetData();
        var pollReply = new ArtPollReply
        {
            Ip = IPAddress.Parse("127.0.0.1").GetAddressBytes(),
            Port = 0x1936,
            Mac = NetworkInterface.GetAllNetworkInterfaces()
                .Where(nic => nic.OperationalStatus == OperationalStatus.Up && nic.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                .Select(nic => nic.GetPhysicalAddress().GetAddressBytes())
                .FirstOrDefault(),

            GoodInput = [0x08, 0x08, 0x08, 0x08],
            GoodOutput = [0x80, 0x80, 0x80, 0x80],
            PortTypes = [0xc0, 0xc0, 0xc0, 0xc0],
            PortName = "PortName Test",
            LongName = "Art.Net LongName Test",
            EstaManLo = 0,
            VersInfoH = 6,
            VersInfoL = 9,
            SubSwitch = 0,
            OemHi = 0,
            Oem = 0xFF,
            UbeaVersion = 0,
            Status1 = 0xd2,
            SwMacro = 0,
            SwRemote = 0,
            Style = StyleCodes.StNode,
            NumPortsHi = 0,
            NumPortsLo = 4,
            Status2 = 0x08,
            BindIp = IPAddress.Parse("127.0.0.1").GetAddressBytes(),
            SwIn = [0x01, 0x02, 0x03, 0x04],
            SwOut = [0x01, 0x02, 0x03, 0x04],
            GoodOutputB = [0x80, 0x80, 0x80, 0x80],

            NodeReport = "NodeReport Test",
            Filler = new byte[168]
        };
        data.Buffer = pollReply.ToArray();

        ArtNetPacket altPacket = ArtNetPacket.FromData(data);
        Assert.AreEqual(altPacket.OpCode, pollReply.OpCode);

        var pollReplyPacket = altPacket.Cast<ArtPollReply>();
        Assert.AreEqual(pollReplyPacket.PortName, pollReply.PortName);
        Assert.AreEqual(pollReplyPacket.LongName, pollReply.LongName);

        Assert.AreEqual(pollReplyPacket.NodeReport, pollReply.NodeReport);

        Assert.Pass();
    }
}