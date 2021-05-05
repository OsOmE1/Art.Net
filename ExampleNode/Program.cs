using ArtNet.IO;
using ArtNet.Packets;
using ArtNet.Packets.Codes;
using System;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;

namespace Example
{
    class Program
    {
        static void Main(string[] args)
        {
            var artnet = new ArtNet.Sockets.ArtNetSocket
            {
                EnableBroadcast = true
            };

            Console.WriteLine(artnet.GetBroadcastAddress());
            artnet.Begin(IPAddress.Parse("127.0.0.1"), IPAddress.Parse("255.255.255.0"));
            artnet.NewPacket += (object sender, ArtNet.Sockets.NewPacketEventArgs<ArtNetPacket> e) =>
            {
                Console.WriteLine(e.Packet.ToString());
                if (e.Packet.OpCode == OpCodes.OpDmx)
                {
                    ArtDmx dmx = ArtDmx.FromData(e.Packet.PacketData);
                    Console.WriteLine(dmx.ToString());
                }
                if (e.Packet.OpCode == OpCodes.OpPoll)
                {
                    var pollReply = new ArtPollReply
                    {
                        IP = IPAddress.Parse("127.0.0.1").GetAddressBytes(),
                        Port = (short)artnet.Port,
                        Mac = NetworkInterface.GetAllNetworkInterfaces()
                            .Where(nic => nic.OperationalStatus == OperationalStatus.Up && nic.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                            .Select(nic => nic.GetPhysicalAddress().GetAddressBytes())
                            .FirstOrDefault(),

                        GoodInput = new byte[] { 0x08, 0x08, 0x08, 0x08 },
                        GoodOutput = new byte[] { 0x80, 0x80, 0x80, 0x80 },
                        PortTypes = new byte[] { 0xc0, 0xc0, 0xc0, 0xc0 },
                        ShortName = "Art.Net\0",
                        LongName = "A c# Art-Net 4 Library\0",
                        EstaManHi = 0,
                        EstaManLo = 0,
                        VersInfoH = 6,
                        VersInfoL = 9,
                        SubSwitch = 0,
                        OemHi = 0,
                        OemLo = 0,
                        UbeaVersion = 0,
                        Status1 = 0xd2,
                        SwMacro = 0,
                        SwRemote = 0,
                        Style = (byte)StyleCodes.StNode,
                        NumPortsHi = 0,
                        NumPortsLo = 4,
                        Status2 = 0x08,
                        BindIp = IPAddress.Parse("127.0.0.1").GetAddressBytes(),
                        SwIn = new byte[] { 0x01, 0x02, 0x03, 0x04 },
                        SwOut = new byte[] { 0x01, 0x02, 0x03, 0x04 },
                        GoodOutput2 = new byte[] { 0x80, 0x80, 0x80, 0x80 },

                        NodeReport = "Up and running\0",
                        Filler = new byte[168]
                    };
                    pollReply.Oem = 0xFFFF;

                    artnet.Send(pollReply);
                    Console.WriteLine(pollReply.ToString());
                }

            };
            var todRequest = new ArtTodRequest
            {
                ProtVerHi = 0,
                ProtVerLo = 0,
                Filler = 0,
                Spare = new byte[7],
                Net = 0x00,
                Command = 0x00,
                Address = new byte[32],
            };

            todRequest.ProtVer = 14;
            artnet.Send(todRequest);

            Console.ReadLine();
        }
    }
}
