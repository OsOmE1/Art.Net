// Copyright (c) 2024 OsOmE1 - https://github.com/OsOmE1 - https://github.com/OsOmE1/Art.Net

using System;
using ArtNet.Packets.Codes;

namespace ArtNet;

internal class Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class OpCodeAttribute : Attribute
    {
        public OpCodes OpCode { get; set; } = OpCodes.OpNone;
    }
}