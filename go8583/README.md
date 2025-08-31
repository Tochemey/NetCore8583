# go8583

A lightweight Go port of the [NetCore8583](../) ISO 8583 message library. The module provides simple APIs to build and parse ASCII ISO 8583 messages.

## Installation

```bash
go get github.com/Tochemey/go8583
```

## Usage

```go
package main

import (
    "fmt"
    iso8583 "github.com/Tochemey/go8583"
)

func main() {
    // build a message
    m := iso8583.NewMessage("0200")
    m.SetField(3, iso8583.NewValue(iso8583.Numeric, "650000", 6))
    m.SetField(4, iso8583.NewValue(iso8583.AMOUNT, "1000", 12))
    m.SetField(48, iso8583.NewValue(iso8583.LLLVAR, "DATA", 0))

    packed, err := m.Pack()
    if err != nil {
        panic(err)
    }

    // parse the bytes back into a message
    specs := map[int]iso8583.FieldSpec{
        3:  {Type: iso8583.Numeric, Length: 6},
        4:  {Type: iso8583.AMOUNT, Length: 12},
        48: {Type: iso8583.LLLVAR},
    }
    parsed, err := iso8583.Parse(packed, specs)
    if err != nil {
        panic(err)
    }
    fmt.Println("Field 4:", parsed.GetField(4))
}
```

## Testing

Run the unit tests with:

```bash
go test ./...
```

These tests mirror the original .NET unit tests to ensure functional parity.
