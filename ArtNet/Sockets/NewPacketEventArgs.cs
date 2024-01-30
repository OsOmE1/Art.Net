// Copyright (c) 2024 OsOmE1 - https://github.com/OsOmE1 - https://github.com/OsOmE1/Art.Net

using System;
using System.Net;

namespace ArtNet.Sockets;

public class NewPacketEventArgs<TPacketType>(IPEndPoint source, TPacketType packet) : EventArgs
{
    public IPEndPoint Source = source;
    public TPacketType Packet = packet;
}