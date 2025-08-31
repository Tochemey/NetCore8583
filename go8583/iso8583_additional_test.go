package iso8583

import "testing"

// Test packing and parsing with fields beyond primary bitmap
func TestSecondaryBitmap(t *testing.T) {
	msg := NewMessage("0200")
	msg.SetField(3, NewValue(Numeric, "650000", 6))
	msg.SetField(100, NewValue(LLVAR, "123", 0))
	msg.SetField(102, NewValue(LLVAR, "ABCD", 0))

	packed, err := msg.Pack()
	if err != nil {
		t.Fatalf("pack failed: %v", err)
	}
	if len(packed) < 36 { // MTI + 2 bitmaps
		t.Fatalf("packed message too short: %d", len(packed))
	}

	specs := map[int]FieldSpec{
		3:   {Type: Numeric, Length: 6},
		100: {Type: LLVAR},
		102: {Type: LLVAR},
	}
	parsed, err := Parse(packed, specs)
	if err != nil {
		t.Fatalf("parse failed: %v", err)
	}
	if v, ok := parsed.GetField(3); !ok || v.Value != "650000" {
		t.Fatalf("unexpected field 3: %+v", v)
	}
	if v, ok := parsed.GetField(100); !ok || v.Value != "123" {
		t.Fatalf("unexpected field 100: %+v", v)
	}
	if v, ok := parsed.GetField(102); !ok || v.Value != "ABCD" {
		t.Fatalf("unexpected field 102: %+v", v)
	}
}

// Test encoding and decoding of 4-digit variable length fields
func TestLLLLFields(t *testing.T) {
	msg := NewMessage("0200")
	msg.SetField(2, NewValue(LLLLVAR, "HELLO WORLD", 0))
	msg.SetField(3, NewValue(LLLLBIN, "A1B2C3", 0))

	packed, err := msg.Pack()
	if err != nil {
		t.Fatalf("pack failed: %v", err)
	}

	specs := map[int]FieldSpec{
		2: {Type: LLLLVAR},
		3: {Type: LLLLBIN},
	}
	parsed, err := Parse(packed, specs)
	if err != nil {
		t.Fatalf("parse failed: %v", err)
	}
	if v, ok := parsed.GetField(2); !ok || v.Value != "HELLO WORLD" {
		t.Fatalf("unexpected field 2: %+v", v)
	}
	if v, ok := parsed.GetField(3); !ok || v.Value != "A1B2C3" {
		t.Fatalf("unexpected field 3: %+v", v)
	}
}

// Test fixed-length binary field encoding
func TestBinaryField(t *testing.T) {
	msg := NewMessage("0600")
	msg.SetField(41, NewValue(BINARY, "ABCDEF", 8))

	packed, err := msg.Pack()
	if err != nil {
		t.Fatalf("pack failed: %v", err)
	}

	specs := map[int]FieldSpec{
		41: {Type: BINARY, Length: 8},
	}
	parsed, err := Parse(packed, specs)
	if err != nil {
		t.Fatalf("parse failed: %v", err)
	}
	if v, ok := parsed.GetField(41); !ok || v.Value != "ABCDEF00" {
		t.Fatalf("unexpected field 41: %+v", v)
	}
}

// Test encoding and parsing of LLLBIN field
func TestLLLBIN(t *testing.T) {
	msg := NewMessage("0200")
	msg.SetField(62, NewValue(LLLBIN, "ABCDEF", 0))

	packed, err := msg.Pack()
	if err != nil {
		t.Fatalf("pack failed: %v", err)
	}

	specs := map[int]FieldSpec{
		62: {Type: LLLBIN},
	}
	parsed, err := Parse(packed, specs)
	if err != nil {
		t.Fatalf("parse failed: %v", err)
	}
	if v, ok := parsed.GetField(62); !ok || v.Value != "ABCDEF" {
		t.Fatalf("unexpected field 62: %+v", v)
	}
}
