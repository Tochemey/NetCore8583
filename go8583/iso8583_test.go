package iso8583

import "testing"

// Test packing and parsing of a simple ISO8583 message.
func TestPackAndParse(t *testing.T) {
	msg := NewMessage("0200")
	msg.SetField(3, NewValue(Numeric, "650000", 6))
	msg.SetField(4, NewValue(AMOUNT, "1000", 12))
	msg.SetField(7, NewValue(DATE10, "1234567890", 10))
	msg.SetField(48, NewValue(LLLVAR, "DATA", 0))

	packed, err := msg.Pack()
	if err != nil {
		t.Fatalf("pack failed: %v", err)
	}

	expected := "020032000000000100006500000000000010001234567890004DATA"
	if string(packed) != expected {
		t.Fatalf("unexpected packed message: %s", string(packed))
	}

	specs := map[int]FieldSpec{
		3:  {Type: Numeric, Length: 6},
		4:  {Type: AMOUNT, Length: 12},
		7:  {Type: DATE10, Length: 10},
		48: {Type: LLLVAR},
	}

	parsed, err := Parse(packed, specs)
	if err != nil {
		t.Fatalf("parse failed: %v", err)
	}

	if parsed.Mti != "0200" {
		t.Fatalf("unexpected MTI: %s", parsed.Mti)
	}

	if v, ok := parsed.GetField(3); !ok || v.Value != "650000" {
		t.Fatalf("unexpected field 3: %+v", v)
	}
	if v, ok := parsed.GetField(4); !ok || v.Value != "000000001000" {
		t.Fatalf("unexpected field 4: %+v", v)
	}
	if v, ok := parsed.GetField(7); !ok || v.Value != "1234567890" {
		t.Fatalf("unexpected field 7: %+v", v)
	}
	if v, ok := parsed.GetField(48); !ok || v.Value != "DATA" {
		t.Fatalf("unexpected field 48: %+v", v)
	}
}

// Test encoding and parsing of a binary variable-length field.
func TestLLBIN(t *testing.T) {
	msg := NewMessage("0200")
	msg.SetField(63, NewValue(LLBIN, "A1B2C3", 0))

	packed, err := msg.Pack()
	if err != nil {
		t.Fatalf("pack failed: %v", err)
	}

	specs := map[int]FieldSpec{
		63: {Type: LLBIN},
	}

	parsed, err := Parse(packed, specs)
	if err != nil {
		t.Fatalf("parse failed: %v", err)
	}

	if v, ok := parsed.GetField(63); !ok || v.Value != "A1B2C3" {
		t.Fatalf("unexpected field 63: %+v", v)
	}
}
