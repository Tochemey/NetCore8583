# NetCore8583

_NetCore8583 is considered feature complete and mature.
No future feature development is planned, though bugs and security issues are fixed._

[![GitHub Workflow Status (branch)](https://img.shields.io/github/actions/workflow/status/Tochemey/NetCore8583/ci.yml?branch=main&style=flat-square)](https://github.com/Tochemey/NetCore8583/blob/main/.github/workflows/ci.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg?style=flat-square)](https://opensource.org/licenses/MIT)
[![Nuget](https://img.shields.io/nuget/v/NetCore8583?style=flat-square)](https://www.nuget.org/packages/NetCore8583/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/NetCore8583?style=flat-square)](https://www.nuget.org/packages/NetCore8583/)
[![Stability: Maintenance](https://masterminds.github.io/stability/maintenance.svg)](https://masterminds.github.io/stability/maintenance.html)

## 📖 Introduction

NetCore8583 is a dotnet core implementation of the ISO 8583 protocol.

NetCore8583 is a library that helps parse/read and generate ISO 8583 messages. It does not handle sending or reading them over a network connection, but it does parse the data you have read and can generate the data you need to write over a network connection.

## 🏦 ISO 8583 overview

ISO8583 is a message format used for credit card transactions, banking and other commercial interaction between different systems. It has an ASCII variant and a binary one, and it is somewhat convoluted and difficult to implement.

The main format of the ISO8583 is something like this:

| ISO header (optional) | Message Type | primary bitmap | secondary bitmap (optional) | data fields |
| --------------------- | ------------ | -------------- | --------------------------- | ----------- |

The ISO header is a string containing some code that can vary according to the message type.

The message type is a number expressed as 4 hex digits (or 2 bytes when using binary format).

The bitmap is 64 bits long and it is encoded as 16 hex characters or as 8 bytes when using binary format. Every bit that is set in the bitmap indicates that the corresponding field is present. If the first bit is set, then field 1 is present, and so on.

The fields in the message are numbered from 1 to 64. Field 1 is the secondary bitmap, if present. The secondary bitmap allows for the message to have fields from 65 to 128.

Wikipedia has [a very good article](http://en.wikipedia.org/wiki/ISO_8583) on the whole specification.

## 📦 Usage

The library is available on nuget package. You can get it via:

```bash
dotnet add package NetCore8583
```

## 💬 Support

One can use the following channel to report a bug or discuss a feature or an enhancement:

- [Issue Tracker](https://github.com/Tochemey/NetCore8583/issues)
- [Discussions](https://github.com/Tochemey/NetCore8583/discussions)

If you find this library very useful in your day job, kindly show some love by starring it.

## ⚙️ How does NetCore8583 work?

NetCore8583 offers a [`MessageFactory`](./NetCore8583/MessageFactory.cs), which once properly configured, can create different message types with some values predefined, and can also parse a byte array to create an ISO message. Messages are represented by [`IsoMessage`](./NetCore8583/IsoMessage.cs) objects, which store [`IsoValue`](./NetCore8583/IsoValue.cs) instances for their data fields. You can work with the [`IsoValue`](./NetCore8583/IsoValue.cs) or use the convenience methods of [`IsoMessage`](./NetCore8583/IsoMessage.cs) to work directly with the stored values.

### 🏗️ MessageFactory and IsoMessage classes

These are the two main classes you need to use to work with ISO8583 messages. An [`IsoMessage`](./NetCore8583/IsoMessage.cs) can be encoded into a signed byte array. You can set and get the values for each field in an [`IsoMessage`](./NetCore8583/IsoMessage.cs), and it will adjust itself to use a secondary bitmap if necessary. An [`IsoMessage`](./NetCore8583/IsoMessage.cs) has settings to encode itself in binary or ASCII, to use a secondary bitmap even if it's not necessary, and it can have its own ISO header.

However, it can be cumbersome to programmatically create [`IsoMessage`](./NetCore8583/IsoMessage.cs) all the time. The MessageFactory is a big aid in creating [`IsoMessage`](./NetCore8583/IsoMessage.cs) with predefined values; also, it can set the date and the trace number in each new message.

- There is an extension method that helps switch between signed byte array and unsigned byte array.

#### 🔧 How to configure the MessageFactory

There are five main things you need to configure in a [`MessageFactory`](./NetCore8583/MessageFactory.cs): ISO headers, message templates, parsing templates, TraceNumberGenerator, and custom field encoders.

- **Iso headers**: ISO headers are strings that are associated with a message type. Whenever you ask the message factory to create an [`IsoMessage`](./NetCore8583/IsoMessage.cs), it will pass the corresponding ISO header (if present) to the new message.

- **Message Templates**: A message template is an [`IsoMessage`](./NetCore8583/IsoMessage.cs) itself; the [`MessageFactory`](./NetCore8583/MessageFactory.cs) can have a template for each message type it needs to create. When it creates a message and it has a template for that message type, it copies the fields from the template to the new message before returning it.

- **Parsing Templates**: A parsing template is a map containing [`FieldParseInfo`](./NetCore8583/Parse/FieldParseInfo.cs) objects as values and the field numbers as the keys. A [`FieldParseInfo`](./NetCore8583/Parse/FieldParseInfo.cs) object contains an [`IsoType`](./NetCore8583/IsoType.cs) and an optional length; with this information and the field number, the [`MessageFactory`](./NetCore8583/MessageFactory.cs) can parse incoming messages, first analyzing the message type and then using the parsing template for that type; when parsing a message, the [`MessageFactory`](./NetCore8583/MessageFactory.cs) only parses the fields that are specified in the message's bitmap. For example if the bitmap specifies field 4, the factory will get the [`FieldParseInfo`](./NetCore8583/Parse/FieldParseInfo.cs) stored in the map under key 4, and will attempt to parse the field according to the type and length specified by the [`FieldParseInfo`](./NetCore8583/Parse/FieldParseInfo.cs).
  A message does not need to contain all the fields specified in a parsing template, but a parsing template must contain all the fields specified in the bitmap of a message, or the MessageFactory won't be able to parse it because it has no way of knowing how it should parse that field (and also all subsequent fields).

- **ITraceNumberGenerator**: When creating new messages, they usually need to have a unique trace number, contained in field 11. Also, they usually need to have the date they were created (or the date the transaction was originated) in field 7. The [`MessageFactory`](./NetCore8583/MessageFactory.cs) can automatically set the current date on all new messages, you just need to set the assignDate property to true. And it can also assign a new trace number to each message it creates, but for this it needs a TraceNumberGenerator.
  The [`ITraceNumberGenerator`](./NetCore8583/ITraceNumberGenerator.cs) interface defines a `nextTrace()` method, which must return a new trace number between 1 and 999999. It needs to be cyclic, so it returns 1 again after returning 999999. And usually, it needs to be thread-safe.
  NetCore8583 only defines the interface; in production environments you will usually need to implement your own TraceNumberGenerator, getting the new trace number from a sequence in a database or some similar mechanism. As an example, the library includes the [`SimpleTraceGenerator`](./NetCore8583/Tracer/SimpleTraceGenerator.cs), which simply increments an in-memory value.

- **Custom fields encoders**: Certain implementations of ISO8583 specify fields which contain many subfields. If you only handle strings in those fields, you'll have to encode all those values before storing them in an [`IsoMessage`](./NetCore8583/IsoMessage.cs), and decode them when you get them from an IsoMessage.
  In these cases you can implement a [`CustomField`](./NetCore8583/ICustomField.cs), which is an interface that defines two methods, one for encoding an object into a String and another for decoding an object from a String. You can pass the [`MessageFactory`](./NetCore8583/MessageFactory.cs) a [`CustomField`](./NetCore8583/ICustomField.cs) for every field where you want to store custom values, so that parsed messages will return the objects decoded by the [`CustomField`](./NetCore8583/ICustomField.cs) instead of just strings; and when you set a value in an [`IsoMessage`](./NetCore8583/IsoMessage.cs), you can specify the CustomField to be used to encode the value as a String

#### 🔌 Custom Field encoders

Sometimes there are fields that contain several sub-fields or separate pieces of data. NetCore8583 will only parse the field for you, but you still have to parse those pieces of data from the field when you parse a message, and/or encode several pieces of data into a field when creating a message.

NetCore8583 can help with this process, by means of the custom field encoders. To use this feature, first you need to implement the ICustomField interface. You can see how it is done in the following test classes **_TestParsing.cs_** and **_TestIsoMessage.cs_** using the **_CustomField48.cs_** class.

### 🛠️ Easy way to configure ISO 8583 messages templates

There are two ways to configure message templates and parsing templates:

1. **XML configuration** -- declare templates, headers, and parse guides in a XML file and load it into the [`MessageFactory`](./NetCore8583/MessageFactory.cs).
2. **Programmatic configuration (Builder API)** -- use the fluent [`MessageFactoryBuilder`](./NetCore8583/Builder/MessageFactoryBuilder.cs) to configure everything in code, with full support for inheritance, composite fields, and all factory settings.

### 📄 XML configuration

The [`MessageFactory`](./NetCore8583/MessageFactory.cs) can read a XML file to setup message templates, ISO headers by type and parsing templates, which are the most cumbersome parts to configure programmatically.
There are three types of main elements that you need to specify in the config file: header, template, and parse. All these must be contained in a single `n8583-config` element.

#### 🏷️ Header

Specify a header element for every type of message that needs an ISO header. Only one per message type:

```xml
    <header type="0200">ISO015000050</header>
    <header type="0400">ISO015000050</header>
```

You can define a header as a reference to another header:

```xml
    <header type="0800" ref="0200" />
```

The header for 0800 messages will be the same as the header for 0200 messages.

#### 📋 Template Element

Each template element defines a message template, with the message type and the fields that the template should include. Every new message of that type that the [`MessageFactory`](./NetCore8583/MessageFactory.cs) creates will contain those same values, so this is very useful for defining fixed values, which will be the same in every message. Only one template per type.

```xml
    <template type="0200">
        <field num="3" type="NUMERIC" length="6">650000</field>
        <field num="32" type="LLVAR">456</field>
        <field num="35" type="LLVAR">4591700012340000=</field>
        <field num="43" type="ALPHA" length="40">Fixed-width data</field>
        <field num="48" type="LLLVAR">Life, the Universe, and Everything|42</field>
        <field num="49" type="ALPHA" length="3">840</field>
        <field num="60" type="LLLVAR">B456PRO1+000</field>
        <field num="61" type="LLLVAR">This field can have a value up to 999 characters long.</field>
        <field num="100" type="LLVAR">999</field>
        <field num="102" type="LLVAR">ABCD</field>
    </template>
```

You can define a template as extending another template, so that it includes all the fields from the referenced template as well as any new fields defined in it, as well as excluding fields from the referenced template:

```xml
    <template type="0400" extends="0200">
        <field num="90" type="ALPHA" length="42">Bla bla</field>
        <field num="102" type="exclude" />
    </template>
```

In the above example, the template for message type 0400 will include all fields defined in the template for message type 0200 except field 102, and will additionally include field 90.

#### 🔍 Parse Element

Each parse element defines a parsing template for a message type. It must include all the fields that an incoming message can contain, each field with its type and length (if needed). Only `ALPHA` and `NUMERIC` types need to have a length specified. The other types either have a fixed length, or have their length specified as part of the field (`LLVAR` and `LLLVAR`).

```xml
    <parse type="0210">
        <field num="3" type="NUMERIC" length="6" />
        <field num="4" type="AMOUNT" />
        <field num="7" type="DATE10" />
        <field num="11" type="NUMERIC" length="6" />
        <field num="12" type="TIME" />
        <field num="13" type="DATE4" />
        <field num="15" type="DATE4" />
        <field num="17" type="DATE_EXP" />
        <field num="32" type="LLVAR" />
        <field num="35" type="LLVAR" />
        <field num="37" type="NUMERIC" length="12" />
        <field num="38" type="NUMERIC" length="6" />
        <field num="39" type="NUMERIC" length="2" />
        <field num="41" type="ALPHA" length="16" />
        <field num="43" type="ALPHA" length="40" />
        <field num="48" type="LLLVAR" />
        <field num="49" type="ALPHA" length="3" />
        <field num="60" type="LLLVAR" />
        <field num="61" type="LLLVAR" />
        <field num="70" type="ALPHA" length="3" />
        <field num="100" type="LLVAR" />
        <field num="102" type="LLVAR" />
        <field num="126" type="LLLVAR" />
    </parse>
```

As with message templates, you can define parsing guides that extend other parsing guides:

```xml
    <parse type="0410" extends="0210">
        <field num="90" type="ALPHA" length="42" />
        <field num="102" type="exclude" />
    </parse>
```

#### 🧩 Composite Fields

Another feature is the [`CompositeField`](./NetCore8583/Codecs/CompositeField.cs). This is a [`CustomField`](./NetCore8583/ICustomField.cs) that acts as a container for several [`IsoValue`](./NetCore8583/IsoValue.cs), and it can be configured in the parsing guide of a message type:

```xml
    <parse type="0410">
        <field num="125" type="LLLVAR">
            <field num="1" type="ALPHA" length="5" />
            <field num="2" type="LLVAR" />
            <field num="3" type="NUMERIC" length="6" />
            <field num="4" type="ALPHA" length="2" />
        </field>
    </parse>
```

In the above example, when a message with type 0410 is parsed, the value for field 125 will be a [`CompositeField`](./NetCore8583/Codecs/CompositeField.cs) and you can obtain the subfields via **_GetField()_** or **_GetObjectValue()_**. The num attribute of the subfields is ignored.

This means that you can do this via code:

```c#
    //Assuming original data for field 125 is "018one  03two123456OK"
    CompositeField f = message.GetObjectValue(125);
    string sub1 = (string)f.GetObjectValue(0); //"one  "
    string sub2 = (string)f.GetObjectValue(1); //"two"
    string sub3 = (string)f.GetObjectValue(2); //"123456"
    string sub4 = (string)f.GetObjectValue(3); //"OK"
```

You can also create a [`CompositeField`](./NetCore8583/Codecs/CompositeField.cs), store several subfields inside it, and store it in any field inside an [`IsoMessage`](./NetCore8583/IsoMessage.cs), specifying the same instance as the [`CustomField`](./NetCore8583/ICustomField.cs):

```c#
    CompositeField f = new CompositeField().AddValue(new IsoValue(IsoType.ALPHA, "one", 5))
    .AddValue(new IsoValue(IsoType.LLVAR, "two"))
    .AddValue(new IsoValue(IsoType.NUMERIC, 123l, 6))
    .AddValue(new IsoValue(IsoType.ALPHA, "OK", 2));
    message.SetValue(125, f, f, IsoType.LLLVAR, 0);
```

When the message is encoded, field 125 will be "018one 03two000123OK".

### 🧱 Programmatic Configuration (Builder API)

If you prefer to configure the [`MessageFactory`](./NetCore8583/MessageFactory.cs) entirely in code -- without any XML files -- you can use the [`MessageFactoryBuilder`](./NetCore8583/Builder/MessageFactoryBuilder.cs). It provides a fluent API that supports all the same features as XML configuration: headers, templates, parse maps, inheritance, composite fields, and custom field encoders.

```c#
using NetCore8583.Builder;

var factory = new MessageFactoryBuilder<IsoMessage>()
    .WithEncoding(Encoding.UTF8)
    .WithBinaryMessages()
    .WithBinaryBitmap()
    // ... more factory settings
    .Build();
```

#### 🏷️ Headers

Set ASCII headers per message type, or reuse one via `WithHeaderRef`:

```c#
var factory = new MessageFactoryBuilder<IsoMessage>()
    .WithHeader(0x0200, "ISO015000050")
    .WithHeader(0x0210, "ISO015000055")
    .WithHeaderRef(0x0400, 0x0200)        // 0400 reuses the 0200 header
    .WithBinaryHeader(0x0280, new byte[] { 0xFF, 0xFF, 0xFF, 0xFF })
    .Build();
```

#### 📋 Templates

Define message templates with default field values. Fixed-length types (`ALPHA`, `NUMERIC`, `BINARY`) require a length parameter; variable-length types (`LLVAR`, `LLLVAR`, etc.) and date/time types do not:

```c#
var factory = new MessageFactoryBuilder<IsoMessage>()
    .WithTemplate(0x0200, t => t
        .Field(3,   IsoType.NUMERIC, "650000", 6)
        .Field(32,  IsoType.LLVAR,   "456")
        .Field(43,  IsoType.ALPHA,   "Fixed-width data", 40)
        .Field(48,  IsoType.LLLVAR,  "Life, the Universe, and Everything|42")
        .Field(49,  IsoType.ALPHA,   "484", 3)
        .Field(102, IsoType.LLVAR,   "ABCD"))
    .Build();
```

#### 🔗 Template Inheritance

A template can inherit all fields from another template using `Extends`, then add new fields or remove inherited ones with `Exclude`. This is the programmatic equivalent of the XML `extends` attribute:

```c#
var factory = new MessageFactoryBuilder<IsoMessage>()
    .WithTemplate(0x0200, t => t
        .Field(3,   IsoType.NUMERIC, "650000", 6)
        .Field(49,  IsoType.ALPHA,   "484", 3)
        .Field(102, IsoType.LLVAR,   "ABCD"))
    .WithTemplate(0x0400, t => t
        .Extends(0x0200)                           // inherits fields 3, 49, 102
        .Field(90, IsoType.ALPHA, "BLA", 42)       // adds field 90
        .Exclude(102))                             // removes field 102
    .Build();
```

> **Note:** The base template must be defined **before** the template that extends it.

#### 🔍 Parse Maps

Parse maps define how incoming messages are parsed. Each field specifies its `IsoType` and, for fixed-length types, its length:

```c#
var factory = new MessageFactoryBuilder<IsoMessage>()
    .WithParseMap(0x0200, p => p
        .Field(3,  IsoType.NUMERIC, 6)
        .Field(4,  IsoType.AMOUNT)
        .Field(7,  IsoType.DATE10)
        .Field(11, IsoType.NUMERIC, 6)
        .Field(12, IsoType.TIME)
        .Field(32, IsoType.LLVAR)
        .Field(41, IsoType.ALPHA, 8)
        .Field(49, IsoType.ALPHA, 3))
    .Build();
```

#### 🔗 Parse Map Inheritance

Just like templates, parse maps support `Extends` and `Exclude`:

```c#
var factory = new MessageFactoryBuilder<IsoMessage>()
    .WithParseMap(0x0200, p => p
        .Field(3,  IsoType.NUMERIC, 6)
        .Field(11, IsoType.NUMERIC, 6)
        .Field(12, IsoType.TIME)
        .Field(41, IsoType.ALPHA, 8)
        .Field(49, IsoType.ALPHA, 3))
    .WithParseMap(0x0210, p => p
        .Extends(0x0200)                   // inherits all 0200 fields
        .Field(39, IsoType.ALPHA, 2)       // adds field 39
        .Field(62, IsoType.LLLVAR))        // adds field 62
    .WithParseMap(0x0400, p => p
        .Extends(0x0200)                   // inherits all 0200 fields
        .Field(62, IsoType.LLLVAR))        // adds field 62
    .WithParseMap(0x0410, p => p
        .Extends(0x0400)                   // multi-level: inherits from 0400
        .Field(39, IsoType.ALPHA, 2)
        .Exclude(12))                      // removes field 12
    .Build();
```

> **Note:** The base parse map must be defined **before** the parse map that extends it.

#### 🧩 Composite Fields

Composite fields (fields that contain multiple subfields) are supported in both templates and parse maps.

**In a template** -- use `CompositeField` with `SubField` to define the subfield values:

```c#
var factory = new MessageFactoryBuilder<IsoMessage>()
    .WithTemplate(0x0100, t => t
        .CompositeField(10, IsoType.LLLVAR, cf => cf
            .SubField(IsoType.ALPHA,   "abcde", 5)
            .SubField(IsoType.LLVAR,   "llvar")
            .SubField(IsoType.NUMERIC, "12345", 5)
            .SubField(IsoType.ALPHA,   "X", 1)))
    .Build();

var m = factory.NewMessage(0x0100);
var f = (CompositeField)m.GetObjectValue(10);
string sub1 = (string)f.GetObjectValue(0); // "abcde"
string sub2 = (string)f.GetObjectValue(1); // "llvar"
```

**In a parse map** -- use `CompositeField` with `SubParser` to define the subfield parsers:

```c#
var factory = new MessageFactoryBuilder<IsoMessage>()
    .WithParseMap(0x0100, p => p
        .CompositeField(10, IsoType.LLLVAR, cf => cf
            .SubParser(IsoType.ALPHA, 5)
            .SubParser(IsoType.LLVAR)
            .SubParser(IsoType.NUMERIC, 5)
            .SubParser(IsoType.ALPHA, 1)))
    .Build();
```

#### 🔌 Custom Field Encoders

Register custom field encoders/decoders for fields that need special handling:

```c#
var factory = new MessageFactoryBuilder<IsoMessage>()
    .WithCustomField(48, new CustomField48())
    .WithTemplate(0x0200, t => t
        .Field(48, IsoType.LLLVAR, myObject, new CustomField48()))
    .Build();
```

#### ⚙️ Factory Settings

All [`MessageFactory`](./NetCore8583/MessageFactory.cs) properties can be set through the builder:

| Method                                      | Description                                       |
| ------------------------------------------- | ------------------------------------------------- |
| `WithEncoding(Encoding)`                    | Character encoding for messages and fields        |
| `WithForceStringEncoding()`                 | Force string encoding for variable-length headers |
| `WithRadix(int)`                            | Radix for length headers (default: 10)            |
| `WithBinaryMessages()`                      | Use binary message encoding                       |
| `WithBinaryBitmap()`                        | Use binary bitmap encoding                        |
| `WithEnforceSecondBitmap()`                 | Always write the secondary bitmap                 |
| `WithEtx(int)`                              | End-of-text character (-1 = none)                 |
| `WithIgnoreLast()`                          | Ignore last byte when parsing                     |
| `WithAssignDate()`                          | Auto-set field 7 with current date                |
| `WithTraceGenerator(ITraceNumberGenerator)` | Auto-set field 11 with trace numbers              |

#### 🚀 Complete Example

Below is a complete example that configures headers, templates with inheritance, and parse maps with multi-level inheritance -- equivalent to what you would typically set up via XML:

```c#
var factory = new MessageFactoryBuilder<IsoMessage>()
    .WithEncoding(Encoding.UTF8)
    // Headers
    .WithHeader(0x0200, "ISO015000050")
    .WithHeader(0x0210, "ISO015000055")
    .WithHeaderRef(0x0400, 0x0200)
    .WithHeader(0x0800, "ISO015000015")
    .WithHeaderRef(0x0810, 0x0800)
    // Templates
    .WithTemplate(0x0200, t => t
        .Field(3,   IsoType.NUMERIC, "650000", 6)
        .Field(32,  IsoType.LLVAR,   "456")
        .Field(35,  IsoType.LLVAR,   "4591700012340000=")
        .Field(43,  IsoType.ALPHA,   "Fixed-width data", 40)
        .Field(49,  IsoType.ALPHA,   "484", 3)
        .Field(100, IsoType.LLVAR,   "999")
        .Field(102, IsoType.LLVAR,   "ABCD"))
    .WithTemplate(0x0400, t => t
        .Extends(0x0200)
        .Field(90, IsoType.ALPHA, "BLA", 42)
        .Exclude(102))
    // Parse maps
    .WithParseMap(0x0800, p => p
        .Field(3,  IsoType.ALPHA, 6)
        .Field(12, IsoType.DATE4)
        .Field(17, IsoType.DATE4))
    .WithParseMap(0x0810, p => p
        .Extends(0x0800)
        .Exclude(17)
        .Field(39, IsoType.ALPHA, 2))
    .Build();

// Create a new message from the template
var request = factory.NewMessage(0x0200);

// Parse an incoming message
var parsed = factory.ParseMessage(data, 0);

// Create a response
var response = factory.CreateResponse(request);
```

## 📚 Resources

- [ISO 8583](http://en.wikipedia.org/wiki/ISO_8583)
