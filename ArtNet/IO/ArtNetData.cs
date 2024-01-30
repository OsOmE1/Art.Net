// Copyright (c) 2024 OsOmE1 - https://github.com/OsOmE1 - https://github.com/OsOmE1/Art.Net

using ArtNet.Bin2Object;

namespace ArtNet.IO;

public class ArtNetData
{
    [SkipBin2Object]
    public byte[] Buffer = new byte[1500];
    [SkipBin2Object]
    public int BufferSize = 1500;
    [SkipBin2Object]
    public int DataLength = 0;

    public bool Valid => DataLength > 12;

    public short OpCode => (short)(Buffer[8] | Buffer[9] << 8);
}