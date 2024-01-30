// Copyright (c) 2024 OsOmE1 - https://github.com/OsOmE1 - https://github.com/OsOmE1/Art.Net

using System;
using System.Globalization;

namespace ArtNet.Rdm;

/// <summary>
/// Device UId for RDM
/// </summary>
public class UId(ushort manufacturerId, uint deviceId)
{
    /// <summary>
    /// The manufacturer id of the device
    /// </summary>
    public readonly ushort ManufacturerId = manufacturerId;

    /// <summary>
    /// The id of the device
    /// </summary>
    public readonly uint DeviceId = deviceId;

    public override string ToString() =>
        $"{ManufacturerId:X4}:{DeviceId:X8}";

    public static UId Parse(string value)
    {
        string[] parts = value.Split(':');
        if (parts.Length < 2)
            throw new ArgumentException($"invalid UId: {value}");
        return new UId(ushort.Parse(parts[0], NumberStyles.HexNumber), uint.Parse(parts[1], NumberStyles.HexNumber));
    }

    public override int GetHashCode() => ManufacturerId.GetHashCode() + DeviceId.GetHashCode();

    public override bool Equals(object obj)
    {
        var uid = obj as UId;
        return uid != null && uid.ManufacturerId.Equals(ManufacturerId) && uid.DeviceId.Equals(DeviceId);
    }
}