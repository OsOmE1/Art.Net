// Copyright (c) 2024 OsOmE1 - https://github.com/OsOmE1 - https://github.com/OsOmE1/Art.Net

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using ArtNet.IO;
using ArtNet.Packets;

namespace ArtNet.Sockets;

public class ArtNetSocket() : Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp)
{
    // Constant according to the ArtNet 4 User Guide
    public const int Port = 0x1936;
    private bool _open;
    private IPAddress _localIp;
    private IPAddress _localSubnetMask;

    public DateTime LastPacket;

    public event UnhandledExceptionEventHandler UnhandledException;
    public event EventHandler<NewPacketEventArgs<ArtNetPacket>> NewPacket;
    private readonly List<CancellationTokenSource> _runningIntervals = [];

    public void Begin(IPAddress localIp, IPAddress localSubnetMask)
    {
        _localIp = localIp;
        _localSubnetMask = localSubnetMask;

        SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
        Bind(new IPEndPoint(localIp, Port));
        SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);
        _open = true;


        ReceiveArtNet();
    }

    public void ReceiveArtNet()
    {
        try
        {
            var localPort = (EndPoint)new IPEndPoint(IPAddress.Any, Port);
            var receiveState = new ArtNetData();
            BeginReceiveFrom(receiveState.Buffer, 0, receiveState.BufferSize, SocketFlags.None, ref localPort,
                OnRecieve, receiveState);
        }
        catch (Exception ex)
        {
            UnhandledException?.Invoke(this, new UnhandledExceptionEventArgs(
                new ApplicationException("An error occurred while trying to start receiving ArtNet.", ex), false));
        }
    }

    private void OnRecieve(IAsyncResult state)
    {
        EndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
        if (!_open)
            return;

        try
        {
            var receiveState = (ArtNetData)state.AsyncState;

            if (receiveState == null)
                // Packet was invalid continue receiving
                ReceiveArtNet();

            receiveState!.DataLength = EndReceiveFrom(state, ref remoteEndPoint);

            // Protect against UDP loopback where we recieve our own packets.
            if (LocalEndPoint != remoteEndPoint && receiveState.Valid)
            {
                LastPacket = DateTime.Now;
                ArtNetPacket packet = ArtNetPacket.FromData(receiveState);
                if (packet == null)
                    ReceiveArtNet();

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
    public void Send(ArtNetPacket packet) =>
        SendTo(packet.ToArray(), new IPEndPoint(GetBroadcastAddress(), Port));


    /// <summary>
    /// Broadcasts a <see cref="ArtNetPacket"/> at a certain interval
    /// </summary>
    public CancellationTokenSource SendWithInterval(ArtNetPacket packet, int ms)
    {
        CancellationTokenSource cts = new();
        new Thread(() =>
        {
            while (true)
            {
                if (cts.IsCancellationRequested)
                {
                    break;
                }
                Send(packet);
                Thread.Sleep(ms);
            }
        }).Start();
        _runningIntervals.Add(cts);
        return cts;
    }

    /// <summary>
    /// Unicasts a <see cref="ArtNetPacket"/> to a recipient
    /// </summary>
    public void SendToIp(ArtNetPacket packet, IPAddress ip) =>
        SendTo(packet.ToArray(), new IPEndPoint(ip, Port));


    /// <summary>
    /// Unicasts a <see cref="ArtNetPacket"/> to a recipient at a certain interval
    /// </summary>
    public CancellationTokenSource SendToIpWithInterval(ArtNetPacket packet, IPAddress ip, int ms)
    {
        CancellationTokenSource cts = new();
        new Thread(() =>
        {
            while (true)
            {
                if (cts.IsCancellationRequested)
                {
                    break;
                }
                SendToIp(packet, ip);
                Thread.Sleep(ms);
            }
        }).Start();
        _runningIntervals.Add(cts);
        return cts;
    }

    /// <summary>
    /// Gets broadcast address for current art-net socket
    /// </summary>
    /// <returns>The broadcast address as <see cref="IPAddress"/></returns>
    public IPAddress GetBroadcastAddress()
    {
        if (_localSubnetMask == null)
            return IPAddress.Broadcast;

        byte[] ipAddressBytes = _localIp.GetAddressBytes();
        byte[] subnetMaskBytes = _localSubnetMask.GetAddressBytes();

        if (ipAddressBytes.Length != subnetMaskBytes.Length)
        {
            throw new ArgumentException("Lengths of IP address and subnet mask do not match.");
        }

        var broadcastAddress = new byte[ipAddressBytes.Length];
        for (var i = 0; i < broadcastAddress.Length; i++)
            broadcastAddress[i] = (byte)(ipAddressBytes[i] | (subnetMaskBytes[i] ^ 255));

        return new IPAddress(broadcastAddress);
    }

    /// <summary>
    /// Closes the socket connection and releases all running intervals
    /// </summary>
    public void Close()
    {
        _open = false;
        foreach (CancellationTokenSource cts in _runningIntervals)
            cts.Cancel();

        base.Close();
    }
}