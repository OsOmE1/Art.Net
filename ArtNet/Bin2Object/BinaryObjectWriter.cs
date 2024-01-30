// Copyright (c) 2024 OsOmE1 - https://github.com/OsOmE1 - https://github.com/OsOmE1/Art.Net

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ArtNet.Bin2Object;

public class BinaryObjectWriter : BinaryWriter
{
    // Generic method cache to dramatically speed up repeated calls to WriteObject<T> with the same T
    private readonly Dictionary<string, MethodInfo> _writeObjectGenericCache = new();

    // VersionAttribute cache to dramatically speed up repeated calls to ReadObject<T> with the same T
    private readonly Dictionary<Type, Dictionary<FieldInfo, List<(double Min, double Max)>>> _writeObjectVersionCache = new();

    // Thread synchronization objects (for thread safety)
    private object _writeLock = new();

    // Initialization
    public BinaryObjectWriter(Stream stream, Endianness endianness = Endianness.Little, bool leaveOpen = false)
        : base(stream, Encoding.Default, leaveOpen)
        => Endianness = endianness;

    // Position in the stream
    public long Position
    {
        get => BaseStream.Position;
        set => BaseStream.Position = value;
    }

    // Allows you to specify types which should be written as different types to the stream
    // Key: type in object; Value: type in stream
    public Dictionary<Type, Type> PrimitiveMappings { get; } = new();

    public Endianness Endianness { get; set; }

    public double Version { get; set; } = 1;

    public Encoding Encoding { get; set; } = Encoding.UTF8;

    public void WriteEndianBytes(byte[] bytes) =>
        Write(Endianness == Endianness.Little ? bytes : bytes.Reverse().ToArray());

    public override void Write(long int64) => WriteEndianBytes(BitConverter.GetBytes(int64));

    public override void Write(ulong uint64) => WriteEndianBytes(BitConverter.GetBytes(uint64));

    public override void Write(int int32) => WriteEndianBytes(BitConverter.GetBytes(int32));

    public override void Write(uint uint32) => WriteEndianBytes(BitConverter.GetBytes(uint32));

    public override void Write(short int16) => WriteEndianBytes(BitConverter.GetBytes(int16));

    public override void Write(ushort uint16) => WriteEndianBytes(BitConverter.GetBytes(uint16));

    public void Write(long addr, byte[] bytes)
    {
        lock (_writeLock)
        {
            Position = addr;
            WriteEndianBytes(bytes);
        }
    }

    public void Write(long addr, long int64)
    {
        lock (_writeLock)
        {
            Position = addr;
            Write(int64);
        }
    }

    public void Write(long addr, ulong uint64)
    {
        lock (_writeLock)
        {
            Position = addr;
            Write(uint64);
        }
    }

    public void Write(long addr, int int32)
    {
        lock (_writeLock)
        {
            Position = addr;
            Write(int32);
        }
    }

    public void Write(long addr, uint uint32)
    {
        lock (_writeLock)
        {
            Position = addr;
            Write(uint32);
        }
    }

    public void Write(long addr, short int16)
    {
        lock (_writeLock)
        {
            Position = addr;
            Write(int16);
        }
    }

    public void Write(long addr, ushort uint16)
    {
        lock (_writeLock)
        {
            Position = addr;
            Write(uint16);
        }
    }

    public void Write(long addr, byte value)
    {
        lock (_writeLock)
        {
            Position = addr;
            Write(value);
        }
    }

    public void Write(long addr, bool value)
    {
        lock (_writeLock)
        {
            Position = addr;
            Write(value);
        }
    }

    public void WriteObjectAtAddr<T>(long addr, T obj)
    {
        lock (_writeLock)
        {
            Position = addr;
            WriteObject(obj);
        }
    }

    public void WriteObject<T>(T obj)
    {
        Type type = typeof(T);
        TypeInfo ti = type.GetTypeInfo();

        if (ti.IsPrimitive)
        {
            // Checked for mapped primitive types
            if (PrimitiveMappings.Where(m => m.Key.GetTypeInfo().Name == type.Name).Select(m => m.Value)
                .FirstOrDefault() is { } mapping)
            {
                MethodInfo mappedWriter = (GetType()
                    .GetMethods()
                    .Where(m => m.Name == "Write" && m.GetParameters()[0].ParameterType == mapping &&
                                m.ReturnType == typeof(void))).FirstOrDefault();
                mappedWriter?.Invoke(this, new object[] { obj });
                return;
            }

            // Unmapped primitive
            switch (obj)
            {
                case long v:
                    Write(v);
                    break;
                case ulong v:
                    Write(v);
                    break;
                case int v:
                    Write(v);
                    break;
                case uint v:
                    Write(v);
                    break;
                case short v:
                    Write(v);
                    break;
                case ushort v:
                    Write(v);
                    break;
                case byte v:
                    Write(v);
                    break;
                case bool v:
                    Write(v);
                    break;
                default:
                    throw new ArgumentException("Unsupported primitive type specified: " + type.FullName);
            }
            return;
        }

        // First time caching
        if (!_writeObjectVersionCache.TryGetValue(type, out var value))
        {
            var fields = type.GetFields()
                .ToDictionary(i => i, i => i.GetCustomAttribute<SkipBin2ObjectAttribute>(false) != null
                    ? new List<(double, double)> { (-2, -2) }
                    : i.GetCustomAttributes<VersionAttribute>(false).Select(v => (v.Min, v.Max)).ToList());
            value = fields;
            _writeObjectVersionCache.Add(type, value);
        }

        foreach ((FieldInfo i, var versions) in value)
        {
            // Only process fields for our selected object versioning (always process if none supplied)
            if (versions.Count != 0 &&
                !versions.Any(v => (v.Min <= Version || v.Min == -1) && (v.Max >= Version || v.Max == -1)))
                continue;

            // String
            if (i.FieldType == typeof(string))
            {
                var attr = i.GetCustomAttribute<StringAttribute>(false);

                // No String attribute? Use a null-terminated string by default
                if (attr == null || attr.IsNullTerminated)
                    WriteNullTerminatedString((string)i.GetValue(obj));
                else
                {
                    if (attr.FixedSize <= 0)
                        throw new ArgumentException($"String attribute for array field {i.Name} configuration invalid");
                    WriteFixedLengthString((string)i.GetValue(obj), attr.FixedSize);
                }
            }

            // Array
            else if (i.FieldType.IsArray)
            {
                ArrayLengthAttribute attr = i.GetCustomAttribute<ArrayLengthAttribute>(false) ??
                                            throw new InvalidOperationException(
                                                $"Array field {i.Name} must have ArrayLength attribute");

                int lengthPrimitive;

                if (attr.FieldName != null)
                {
                    FieldInfo field = type.GetField(attr.FieldName);
                    if (field != null)
                        lengthPrimitive = Convert.ToInt32(field.GetValue(obj));
                    else
                    {
                        PropertyInfo property = type.GetProperty(attr.FieldName) ??
                                                throw new ArgumentException(
                                                    $"Array field {i.Name} has invalid FieldName in ArrayLength attribute");
                        lengthPrimitive = Convert.ToInt32(property.GetValue(obj));
                    }
                }
                else if (attr.FixedSize > 0)
                {
                    lengthPrimitive = attr.FixedSize;
                }
                else
                {
                    throw new ArgumentException($"ArrayLength attribute for array field " + i.Name + " configuration invalid");
                }

                object arr = i.GetValue(obj);
                if (arr == null && lengthPrimitive != 0)
                    arr = Array.CreateInstance(i.FieldType.GetElementType(), lengthPrimitive);


                MethodInfo us = GetType().GetMethods().Single(m =>
                    m.Name == "WriteArray" && m.GetParameters().Length == 1 && m.IsGenericMethodDefinition);
                MethodInfo mi2 = us.MakeGenericMethod(i.FieldType.GetElementType());
                mi2.Invoke(this, [arr]);
            }

            // Primitive type
            // This is unnecessary but saves on many generic Invoke calls which are really slow
            else if (i.FieldType.IsPrimitive)
            {
                // Checked for mapped primitive types
                if (PrimitiveMappings.Where(m => m.Key.GetTypeInfo().Name == i.FieldType.Name).Select(m => m.Value).FirstOrDefault() is Type mapping)
                {
                    MethodInfo mappedWriter = (GetType()
                        .GetMethods()
                        .Where(m => m.Name == "Write" && m.GetParameters()[0].ParameterType == mapping &&
                                    m.ReturnType == typeof(void))).FirstOrDefault();
                    mappedWriter?.Invoke(this, [Convert.ChangeType(i.GetValue(obj), mapping)]);
                }
                else
                {
                    // Unmapped primitive type
                    switch (i.GetValue(obj))
                    {
                        case long v:
                            Write(v);
                            break;
                        case ulong v:
                            Write(v);
                            break;
                        case int v:
                            Write(v);
                            break;
                        case uint v:
                            Write(v);
                            break;
                        case short v:
                            Write(v);
                            break;
                        case ushort v:
                            Write(v);
                            break;
                        case byte v:
                            Write(v);
                            break;
                        case bool v:
                            Write(v);
                            break;
                        default:
                            throw new ArgumentException("Unsupported primitive type specified: " + type.FullName);
                    }
                }
            }
            else if (i.FieldType.IsEnum)
            {
                Type underlyingType = Enum.GetUnderlyingType(i.FieldType);
                object o = i.GetValue(obj);

                if (o == null)
                    return;

                if (underlyingType == typeof(long))
                    Write((long)o);
                else if (underlyingType == typeof(ulong))
                    Write((ulong)o);
                else if (underlyingType == typeof(int))
                    Write((int)o);
                else if (underlyingType == typeof(uint))
                    Write((uint)o);
                else if (underlyingType == typeof(short))
                    Write((short)o);
                else if (underlyingType == typeof(ushort))
                    Write((ushort)o);
                else if (underlyingType == typeof(byte))
                    Write((byte)o);
                else if (underlyingType == typeof(sbyte))
                    Write((sbyte)o);
                else
                    throw new ArgumentException("Unsupported primitive type specified: " + type.FullName);
            }
            // Object
            else
            {
                if (!_writeObjectGenericCache.TryGetValue(i.FieldType.FullName, out MethodInfo mi2))
                {
                    MethodInfo us = GetType().GetMethods()
                        .Single(m => m.Name == "WriteObject" && m.IsGenericMethodDefinition);
                    mi2 = us.MakeGenericMethod(i.FieldType);
                    _writeObjectGenericCache.Add(i.FieldType.FullName, mi2);
                }
                mi2.Invoke(this, new[] { i.GetValue(obj) });
            }
        }
    }

    public void WriteArray<T>(long addr, T[] array)
    {
        lock (_writeLock)
        {
            Position = addr;
            WriteArray(array);
        }
    }

    public void WriteArray<T>(IEnumerable<T> array)
    {
        foreach (T t in array)
            WriteObject(t);
    }

    public void WriteNullTerminatedString(long addr, string str, Encoding encoding = null)
    {
        lock (_writeLock)
        {
            Position = addr;
            WriteNullTerminatedString(str, encoding);
        }
    }

    public void WriteNullTerminatedString(string str, Encoding encoding = null) =>
        WriteFixedLengthString(str, str.Length + 1, encoding);

    // The difference between this and BinaryWriter.Write(string) is that the latter adds a length prefix before the string
    public void WriteFixedLengthString(long addr, string str, int size = -1, Encoding encoding = null)
    {
        lock (_writeLock)
        {
            Position = addr;
            WriteFixedLengthString(str, size, encoding);
        }
    }

    public void WriteFixedLengthString(string str, int size = -1, Encoding encoding = null)
    {
        if (str.Length > size && size != -1)
            throw new ArgumentException("String cannot be longer than fixed length");

        byte[] bytes = encoding?.GetBytes(str) ?? Encoding.GetBytes(str);
        Write(bytes);

        if (size == -1) return;
        for (int padding = str.Length; padding < size; padding++)
            Write((byte)0);
    }
}