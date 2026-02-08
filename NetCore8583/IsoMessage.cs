using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using NetCore8583.Extensions;

namespace NetCore8583
{
  /// <summary>
  /// Represents an ISO 8583 financial message with a type, optional header, bitmap, and up to 128 fields (fields 2–128; field 1 is the secondary bitmap).
  /// Supports both ASCII and binary encoding, configurable encoding, and optional ETX terminator.
  /// </summary>
  public class IsoMessage
  {
    private static readonly sbyte[] Hex =
    {
      (sbyte)'0',
      (sbyte)'1',
      (sbyte)'2',
      (sbyte)'3',
      (sbyte)'4',
      (sbyte)'5',
      (sbyte)'6',
      (sbyte)'7',
      (sbyte)'8',
      (sbyte)'9',
      (sbyte)'A',
      (sbyte)'B',
      (sbyte)'C',
      (sbyte)'D',
      (sbyte)'E',
      (sbyte)'F'
    };

    /// <summary>
    ///   This is where values are stored
    /// </summary>
    private readonly IsoValue[] _fields = new IsoValue[129];

    /// <summary>Initializes a new ISO message with no header.</summary>
    public IsoMessage()
    {
    }

    /// <summary>Initializes a new ISO message with the given ASCII header.</summary>
    /// <param name="header">The ISO header string (e.g. TPDU or application header).</param>
    public IsoMessage(string header) => IsoHeader = header;

    /// <summary>Initializes a new ISO message with the given binary header.</summary>
    /// <param name="binaryHeader">The raw binary ISO header bytes.</param>
    public IsoMessage(sbyte[] binaryHeader) => BinIsoHeader = binaryHeader;

    /// <summary>When true, field values (e.g. bitmap) are encoded as strings using the message encoding instead of raw bytes.</summary>
    public bool ForceStringEncoding { get; set; } = false;

    /// <summary>Optional binary ISO header; used when the message uses a binary header instead of <see cref="IsoHeader"/>.</summary>
    public sbyte[] BinIsoHeader { get; set; }

    /// <summary>
    ///   Stores the optional ISO header.
    /// </summary>
    public string IsoHeader { get; set; }

    /// <summary>
    ///   Message Type
    /// </summary>
    public int Type { get; set; }

    /// <summary>
    ///   Indicates if the message is binary-coded.
    /// </summary>
    public bool Binary { get; set; }

    /// <summary>
    ///   Sets the ETX character, which is sent at the end of the message as a terminator.
    ///   Default is -1, which means no terminator is sent.
    /// </summary>
    public int Etx { get; set; } = -1;

    /// <summary>
    ///   Flag to enforce secondary bitmap even if empty.
    /// </summary>
    public bool EnforceSecondBitmap { get; set; }

    /// <summary>When true, the bitmap is written in binary form (8 bytes for 64 fields, 16 for 128) instead of ASCII hex.</summary>
    public bool BinBitmap { get; set; }

    /// <summary>Character encoding used for string fields and ASCII headers. Default is <see cref="Encoding.Default"/>.</summary>
    public Encoding Encoding { get; set; } = Encoding.Default;

    /// <summary>
    ///   Returns the stored object value in a specified field. Fields
    ///   are represented by IsoValues which contain objects so this
    ///   method can return the contained objects directly.
    /// </summary>
    /// <param name="field">The field number (2 to 128)</param>
    /// <returns>The stored object value in that field, or null if the message does not have the field.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public object GetObjectValue(int field)
    {
      var v = _fields[field];
      return v.Value;
    }

    /// <summary>
    ///   Returns the IsoValue used in a field to contain an object.
    /// </summary>
    /// <param name="field">The field index (2 to 128).</param>
    /// <returns>The IsoValue for the specified field.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IsoValue GetField(int field) => _fields[field];

    /// <summary>
    /// Stores the field at the specified index. The first data field is index 2 (index 1 is the secondary bitmap).
    /// </summary>
    /// <param name="index">The field index (2 to 128).</param>
    /// <param name="field">The <see cref="IsoValue"/> to store; can be null to clear the field.</param>
    /// <returns>The receiver (useful for setting several fields in sequence).</returns>
    public IsoMessage SetField(int index,
      IsoValue field)
    {
      if (index is < 2 or > 128) throw new IndexOutOfRangeException("Field index must be between 2 and 128");
      if (field != null) field.Encoding = Encoding;
      _fields[index] = field;
      return this;
    }

    /// <summary>
    /// Sets multiple fields in one call from a dictionary of field index to <see cref="IsoValue"/>.
    /// </summary>
    /// <param name="values">Dictionary of field index (2–128) to value.</param>
    /// <returns>This message.</returns>
    public IsoMessage SetFields(Dictionary<int, IsoValue> values)
    {
      foreach (var (key, value) in values)
        SetField(key,
          value);
      return this;
    }

    /// <summary>
    ///   Sets the specified value in the specified field, creating an IsoValue internally.
    /// </summary>
    /// <param name="index">The field number (2 to 128).</param>
    /// <param name="value">The value to be stored.</param>
    /// <param name="encoder">An optional <see cref="ICustomField"/> to encode/decode the value.</param>
    /// <param name="t">The ISO type of the field.</param>
    /// <param name="length">The length for fixed-length types (ALPHA, NUMERIC, BINARY); ignored for variable-length types.</param>
    /// <returns>This message.</returns>
    public IsoMessage SetValue(int index,
      object value,
      ICustomField encoder,
      IsoType t,
      int length)
    {
      if (index is < 2 or > 128) throw new IndexOutOfRangeException("Field index must be between 2 and 128");
      if (value != null)
      {
        var v = t.NeedsLength()
          ? new IsoValue(t,
            value,
            length,
            encoder)
          : new IsoValue(t,
            value,
            encoder);
        v.Encoding = Encoding;
        _fields[index] = v;
      }
      else
      {
        _fields[index] = null;
      }

      return this;
    }

    /// <summary>
    ///   Sets the specified value in the specified field, creating an IsoValue internally.
    /// </summary>
    /// <param name="index">The field number (2 to 128).</param>
    /// <param name="value">The value to be stored.</param>
    /// <param name="t">The ISO type.</param>
    /// <param name="length">The length of the field, used for ALPHA and NUMERIC values only; ignored for other types.</param>
    /// <returns>This message.</returns>
    public IsoMessage SetValue(int index,
      object value,
      IsoType t,
      int length) =>
      SetValue(index,
        value,
        null,
        t,
        length);

    /// <summary>
    ///   A convenience method to set new values in fields that already contain values.
    ///   The field's type, length and custom encoder are taken from the current value.
    ///   This method can only be used with fields that have been previously set,
    ///   usually from a template in the MessageFactory.
    /// </summary>
    /// <param name="index">The field's index</param>
    /// <param name="value">The new value to be set in that field.</param>
    /// <returns>The message itself.</returns>
    public IsoMessage UpdateValue(int index,
      object value)
    {
      var current = GetField(index);
      if (current == null)
        throw new ArgumentException("Value-only field setter can only be used on existing fields");
      SetValue(index,
        value,
        current.Encoder,
        current.Type,
        current.Length);
      GetField(index).Encoding = current.Encoding;
      return this;
    }

    /// <summary>
    /// Returns true if the message has a value in the specified field.
    /// </summary>
    /// <param name="idx">The field number (2 to 128).</param>
    /// <returns>True if the field is set; otherwise false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool HasField(int idx) => _fields[idx] != null;

    /// <summary>
    ///   Writes a message to a stream, after writing the specified number of bytes indicating
    ///   the message's length. The message will first be written to an internal memory stream
    ///   which will then be dumped into the specified stream. This method flushes the stream
    ///   after the write. There are at most three write operations to the stream: one for the
    ///   length header, one for the message, and the last one with for the ETX.
    /// </summary>
    /// <param name="outs">The stream to write the message to.</param>
    /// <param name="lengthBytes">The size of the message length header. Valid ranges are 0 to 4.</param>
    public void Write(List<sbyte> outs,
      int lengthBytes)
    {
      if (lengthBytes > 4) throw new ArgumentException("The length header can have at most 4 bytes");

      var data = WriteData();

      if (lengthBytes > 0)
      {
        var len = data.Length;
        if (Etx > -1) len++;
        var buf = new sbyte[lengthBytes];
        var pos = 0;
        if (lengthBytes == 4)
        {
          buf[0] = (sbyte)((len & 0xff000000) >> 24);
          pos++;
        }

        if (lengthBytes > 2)
        {
          buf[pos] = (sbyte)((len & 0xff0000) >> 16);
          pos++;
        }

        if (lengthBytes > 1)
        {
          buf[pos] = (sbyte)((len & 0xff00) >> 8);
          pos++;
        }

        buf[pos] = (sbyte)(len & 0xff);
        outs.AddRange(buf);
      }


      outs.AddRange(data);

      //ETX
      if (Etx > -1) outs.Add((sbyte)Etx);
    }

    /// <summary>
    /// Builds a 128-bit bitmap indicating which fields (2–128) are present; bit 0 is set if the secondary bitmap (fields 65–128) is used or enforced.
    /// </summary>
    /// <returns>A <see cref="Bitmap128"/> with bits set for present fields.</returns>
    private Bitmap128 CreateBitmapBitSet()
    {
      var bs = new Bitmap128();
      var hasSecondary = false;
      
      for (var i = 2; i < 129; i++)
      {
        if (_fields[i] != null)
        {
          if (i > 64)
          {
            hasSecondary = true;
          }
          bs.Set(i - 1, true);
        }
      }

      // Set bit 0 (field 1) if secondary bitmap is present or enforced
      if (EnforceSecondBitmap || hasSecondary)
      {
        bs.Set(0, true);
      }

      return bs;
    }

    /// <summary>
    ///   Writes the message to a memory stream and returns a byte array with the result.
    /// </summary>
    /// <returns>The encoded message as a signed byte array.</returns>
    public sbyte[] WriteData()
    {
      var sbyteList = new List<sbyte>();
      var stream = new MemoryStream();
      if (IsoHeader != null)
        try
        {
          sbyteList.AddRange(IsoHeader.GetSignedBytes(Encoding));
        }
        catch (IOException)
        {
          //should never happen, writing to a ByteArrayOutputStream
        }
      else if (BinIsoHeader != null)
        try
        {
          sbyteList.AddRange(BinIsoHeader);
        }
        catch (IOException)
        {
          //should never happen, writing to a ByteArrayOutputStream
        }

      //Message Type
      if (Binary)
      {
        sbyteList.Add((sbyte)((Type & 0xff00) >> 8));
        sbyteList.Add((sbyte)(Type & 0xff));
      }
      else
      {
        try
        {
          sbyteList.AddRange(Type.ToString("x4").GetSignedBytes(Encoding));
        }
        catch (IOException)
        {
          //should never happen, writing to a ByteArrayOutputStream
        }
      }

      //Bitmap
      var bits = CreateBitmapBitSet();
      var bitmapLength = bits.Get(0) ? 128 : 64; // Secondary bitmap present if bit 0 is set

      // Write bitmap to stream
      if (Binary || BinBitmap)
      {
        var pos = 128;
        var b = 0;
        for (var i = 0; i < bitmapLength; i++)
        {
          if (bits.Get(i)) b |= pos;
          pos >>= 1;
          if (pos != 0) continue;
          sbyteList.Add((sbyte)b);
          pos = 128;
          b = 0;
        }
      }
      else
      {
        var sbyteList2 = new List<sbyte>();
        if (ForceStringEncoding)
        {
          sbyteList2 = sbyteList;
          sbyteList = new List<sbyte>();
        }

        var pos = 0;
        var lim = bitmapLength / 4;
        for (var i = 0; i < lim; i++)
        {
          var nibble = 0;
          if (bits.Get(pos++)) nibble |= 8;
          if (bits.Get(pos++)) nibble |= 4;
          if (bits.Get(pos++)) nibble |= 2;
          if (bits.Get(pos++)) nibble |= 1;
          sbyteList.Add(Hex[nibble]);
        }

        if (ForceStringEncoding)
        {
          ReadOnlySpan<sbyte> bitmapSpan = sbyteList.ToArray();
          var hb = bitmapSpan.ToString(Encoding.Default);
          sbyteList = sbyteList2;
          try
          {
            sbyteList.AddRange(hb.GetSignedBytes(Encoding));
          }
          catch (IOException)
          {
            //never happen
          }
        }
      }

      var sbyteArray = sbyteList.ToArray();
      stream.Write(sbyteArray.AsUnsignedBytes().ToArray(), 0, sbyteArray.Length);

      //Fields
      for (var i = 2; i < 129; i++)
      {
        var v = _fields[i];
        if (v == null) continue;
        try
        {
          v.Write(stream,
            Binary,
            ForceStringEncoding);
        }
        catch (IOException)
        {
          //should never happen, writing to a ByteArrayOutputStream
        }
      }

      var resultBytes = stream.ToArray();
      var resultSigned = new sbyte[resultBytes.Length];
      resultBytes.AsSignedBytes().CopyTo(resultSigned);
      return resultSigned;
    }

    /// <summary>
    /// Builds a byte array with the message data, optionally prefixed by a length header and suffixed by ETX if configured.
    /// </summary>
    /// <param name="lengthBytes">Size of the length header (0–4 bytes).</param>
    /// <returns>The complete message bytes (length header + message + optional ETX).</returns>
    public sbyte[] WriteToBuffer(int lengthBytes)
    {
      if (lengthBytes > 4) throw new ArgumentException("The length header can have at most 4 bytes");
      var data = WriteData();
      var stream = new List<sbyte>(lengthBytes + data.Length + (Etx > -1 ? 1 : 0));
      if (lengthBytes > 0)
      {
        var len = data.Length;
        if (Etx > -1) len++;
        if (lengthBytes == 4) stream.Add((sbyte)((len & 0xff000000) >> 24));
        if (lengthBytes > 2) stream.Add((sbyte)((len & 0xff0000) >> 16));
        if (lengthBytes > 1) stream.Add((sbyte)((len & 0xff00) >> 8));
        stream.Add((sbyte)(len & 0xff));
      }

      stream.AddRange(data);

      //ETX
      if (Etx > -1) stream.Add((sbyte)Etx);

      return stream.ToArray();
    }

    /// <summary>
    /// Returns a human-readable string representation of the message (header, type, hex bitmap, and field values) for debugging.
    /// </summary>
    /// <returns>A debug string of the message content.</returns>
    public string DebugString()
    {
      var sb = new StringBuilder();
      if (IsoHeader != null) sb.Append(IsoHeader);
      else if (BinIsoHeader != null)
        sb.Append("[0x").Append(HexCodec.HexEncode(BinIsoHeader,
          0,
          BinIsoHeader.Length)).Append(']');
      sb.Append(Type.ToString("x4"));
      //Bitmap
      var bs = CreateBitmapBitSet();
      var bitmapLength = bs.Get(0) ? 128 : 64;
      var pos = 0;
      var lim = bitmapLength / 4;
      for (var i = 0; i < lim; i++)
      {
        var nibble = 0;
        if (bs.Get(pos++)) nibble |= 8;
        if (bs.Get(pos++)) nibble |= 4;
        if (bs.Get(pos++)) nibble |= 2;
        if (bs.Get(pos++)) nibble |= 1;
        var string0 = Hex.ToString(nibble,
          1,
          Encoding);
        sb.Append(string0);
      }

      //Fields
      for (var i = 2; i < 129; i++)
      {
        var v = _fields[i];
        if (v == null) continue;
        var desc = v.ToString();
        switch (v.Type)
        {
          case IsoType.LLBIN:
          case IsoType.LLVAR:
            sb.Append(desc.Length.ToString("D2"));
            break;
          case IsoType.LLLBIN:
          case IsoType.LLLVAR:
            sb.Append(desc.Length.ToString("D3"));
            break;
          case IsoType.LLLLBIN:
          case IsoType.LLLLVAR:
            sb.Append(desc.Length.ToString("D4"));
            break;
        }

        sb.Append(desc);
      }

      return sb.ToString();
    }

    /// <summary>
    /// Returns true if the message contains all of the specified fields.
    /// </summary>
    /// <param name="idx">One or more field numbers to check.</param>
    /// <returns>True if every specified field is set; otherwise false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool HasEveryField(params int[] idx)
    {
      foreach (var i in idx)
      {
        if (!HasField(i)) return false;
      }
      return true;
    }

    /// <summary>
    /// Returns true if the message contains at least one of the specified fields.
    /// </summary>
    /// <param name="idx">One or more field numbers to check.</param>
    /// <returns>True if any specified field is set; otherwise false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool HasAnyField(params int[] idx)
    {
      foreach (var i in idx)
      {
        if (HasField(i)) return true;
      }
      return false;
    }

    /// <summary>
    /// Copies the specified fields from the source message into this message. Fields not present in the source are skipped.
    /// </summary>
    /// <param name="source">The source ISO message to copy from.</param>
    /// <param name="fields">The field numbers (2–128) to copy.</param>
    public void CopyFieldsFrom(IsoMessage source, params int[] fields)
    {
      foreach (var field in fields)
      {
        var isoValue = source.GetField(field);
        if (isoValue == null) continue;
        SetValue(field, isoValue.Value, isoValue.Encoder, isoValue.Type, isoValue.Length);
      }
    }

    /// <summary>
    /// Removes the specified fields from the message (sets them to null).
    /// </summary>
    /// <param name="fields">The field numbers (2–128) to remove.</param>
    public void RemoveFields(params int[] fields)
    {
      foreach (var field in fields) SetField(field, null);
    }
  }
}