using ArtNet.Packets;
using ArtNet.Packets.Codes;
using System;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using ArtNet.Sockets;

var artnet = new ArtNetSocket
{
    EnableBroadcast = true
};
IPAddress localIp = Dns.GetHostEntry(Dns.GetHostName()).AddressList
    .FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork);
if (localIp == null)
    throw new Exception("could not find suitable ip");

Console.WriteLine(artnet.GetBroadcastAddress());
Console.WriteLine(localIp.ToString());

artnet.Begin(localIp, IPAddress.Parse("255.255.255.0"));
artnet.NewPacket += (_, e) =>
{
    Console.WriteLine(e.Packet.ToString());
    switch (e.Packet.OpCode)
    {
        case OpCodes.OpDmx:
        {
            ArtDmx dmx = ArtDmx.FromData(e.Packet.PacketData);
            Console.WriteLine(dmx.ToString());
            break;
        }
        case OpCodes.OpPoll:
        {
            var pollReply = new ArtPollReply
            {
                Ip = localIp.GetAddressBytes(),
                Port = ArtNetSocket.Port,
                Mac = NetworkInterface.GetAllNetworkInterfaces()
                    .Where(nic => nic.OperationalStatus == OperationalStatus.Up && nic.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                    .Select(nic => nic.GetPhysicalAddress().GetAddressBytes())
                    .FirstOrDefault(),

                GoodInput = [0x08, 0x08, 0x08, 0x08],
                GoodOutput = [0x80, 0x80, 0x80, 0x80],
                PortTypes = [0xc0, 0xc0, 0xc0, 0xc0],
                PortName = "Art.Net\0",
                LongName = "A c# Art-Net 4 Library\0",
                VersInfoH = 6,
                VersInfoL = 9,
                Oem = 0xFFFF,
                Status1 = 0xd2,
                Style = (byte)StyleCodes.StNode,
                NumPortsLo = 4,
                Status2 = 0x08,
                BindIp = localIp.GetAddressBytes(),
                SwIn = [0x01, 0x02, 0x03, 0x04],
                SwOut = [0x01, 0x02, 0x03, 0x04],
                GoodOutputB = [0x80, 0x80, 0x80, 0x80],

                NodeReport = "Up and running\0",
                Filler = new byte[168]
            };

            artnet.Send(pollReply);
            Console.WriteLine(pollReply.ToString());
            break;
        }
        default:
            throw new ArgumentOutOfRangeException();
    }
};
var artPoll = new ArtPoll
{
    ProtVer = 14,
    Priority = PriorityCodes.DpLow
};

artnet.SendWithInterval(artPoll, 3000);

Console.ReadLine();
artnet.Close();