using System;
using System.Net;
using System.Net.Sockets;
using ArtNet.IO;
using ArtNet.Packets;

namespace ArtNet.Sockets
{
    public class ArtNetSocket : Socket
    {
        // Constant according to the ArtNet 4 User Guide
        public int Port = 0x1936;
        private bool open = false;
        private IPAddress localIp;
        private IPAddress localSubnetMask;

        public DateTime LastPacket;

        public event UnhandledExceptionEventHandler UnhandledException;
        public event EventHandler<NewPacketEventArgs<ArtNetPacket>> NewPacket;

        public ArtNetSocket() : base(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp) { }

        public void Begin(IPAddress localIp, IPAddress localSubnetMask)
        {
            this.localIp = localIp;
            this.localSubnetMask = localSubnetMask;

            SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            Bind(new IPEndPoint(localIp, Port));
            SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);
            open = true;


            ReceiveArtNet();
        }

        public void ReceiveArtNet()
        {
            try
            {
                var localPort = (EndPoint)new IPEndPoint(IPAddress.Any, Port);
                var recieveState = new ArtNetData();
                BeginReceiveFrom(recieveState.Buffer, 0, recieveState.BufferSize, SocketFlags.None, ref localPort, new AsyncCallback(OnRecieve), recieveState);
            }
            catch (Exception ex)
            {
                UnhandledException?.Invoke(this, new UnhandledExceptionEventArgs(
                    new ApplicationException("An error ocurred while trying to start recieving ArtNet.", ex), false));
            }
        }

        private void OnRecieve(IAsyncResult state)
        {
            EndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
            if (!open)
            {
                return;
            }

            try
            {
                ArtNetData recieveState = (ArtNetData)(state.AsyncState);

                if (recieveState == null)
                {
                    // Packet was invalid continue receiving
                    ReceiveArtNet();
                }
                recieveState.DataLength = EndReceiveFrom(state, ref remoteEndPoint);

                // Protect against UDP loopback where we recieve our own packets.
                if (LocalEndPoint != remoteEndPoint && recieveState.Valid)
                {
                    LastPacket = DateTime.Now;
                    var packet = ArtNetPacket.FromData(recieveState);
                    if (packet == null)
                    {
                        ReceiveArtNet();
                    }

                    NewPacket?.Invoke(this, new NewPacketEventArgs<ArtNetPacket>((IPEndPoint)remoteEndPoint, packet));
                }
            }
            catch (Exception ex)
            {
                UnhandledException?.Invoke(this, new UnhandledExceptionEventArgs(ex, false));
            }
            ReceiveArtNet();
        }

        /// <summary>
        /// Broadcasts a <see cref="ArtNetPacket"/>
        /// </summary>
        public void Send(ArtNetPacket packet)
        {
            SendTo(packet.ToArray(), new IPEndPoint(GetBroadcastAddress(), Port));
        }

        /// <summary>
        /// Sends a <see cref="ArtNetPacket"/> to a recipient
        /// </summary>
        public void SendToIp(ArtNetPacket packet, IPAddress ip)
        {
            SendTo(packet.ToArray(), new IPEndPoint(ip, Port));
        }

        /// <summary>
        /// Gets broadcast address for current art-net socket
        /// </summary>
        /// <returns>The broadcast address as <see cref="IPAddress"/></returns>
        public IPAddress GetBroadcastAddress()
        {
            if (localSubnetMask == null)
                return IPAddress.Broadcast;

            byte[] ipAdressBytes = localIp.GetAddressBytes();
            byte[] subnetMaskBytes = localSubnetMask.GetAddressBytes();

            if (ipAdressBytes.Length != subnetMaskBytes.Length)
            {
                throw new ArgumentException("Lengths of IP address and subnet mask do not match.");
            }

            byte[] broadcastAddress = new byte[ipAdressBytes.Length];
            for (int i = 0; i < broadcastAddress.Length; i++)
            {
                broadcastAddress[i] = (byte)(ipAdressBytes[i] | (subnetMaskBytes[i] ^ 255));
            }
            return new IPAddress(broadcastAddress);
        }
    }
}
