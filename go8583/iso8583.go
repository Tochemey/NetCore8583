package iso8583

import (
	"encoding/hex"
	"fmt"
	"sort"
	"strconv"
	"strings"
)

// IsoType represents the type of a field in an ISO8583 message.
type IsoType int

const (
	Numeric IsoType = iota
	Alpha
	LLVAR
	LLLVAR
	DATE14
	DATE10
	DATE4
	DATE_EXP
	TIME
	AMOUNT
	BINARY
	LLBIN
	LLLBIN
	LLLLVAR
	LLLLBIN
	DATE12
	DATE6
)

// IsoValue represents the value of a field along with its type and length.
type IsoValue struct {
	Type   IsoType
	Value  string
	Length int // used for fixed length types
}

// NewValue creates a new IsoValue.
func NewValue(t IsoType, v string, length int) IsoValue {
	return IsoValue{Type: t, Value: v, Length: length}
}

func formatNumeric(v string, length int) (string, error) {
	if length <= 0 {
		return "", fmt.Errorf("length required")
	}
	if len(v) > length {
		return v[len(v)-length:], nil
	}
	return fmt.Sprintf("%0*s", length, v), nil
}

// encode converts the IsoValue to its string representation ready to be appended
// to an ISO8583 message.
func (iv IsoValue) encode() (string, error) {
	switch iv.Type {
	case Numeric:
		if iv.Length <= 0 {
			return "", fmt.Errorf("numeric type requires length")
		}
		if len(iv.Value) > iv.Length {
			return iv.Value[len(iv.Value)-iv.Length:], nil
		}
		return fmt.Sprintf("%0*s", iv.Length, iv.Value), nil
	case Alpha:
		if iv.Length <= 0 {
			return "", fmt.Errorf("alpha type requires length")
		}
		if len(iv.Value) > iv.Length {
			return iv.Value[:iv.Length], nil
		}
		return fmt.Sprintf("%-*s", iv.Length, iv.Value), nil
	case LLVAR:
		l := len(iv.Value)
		if l > 99 {
			return "", fmt.Errorf("llvar too long")
		}
		return fmt.Sprintf("%02d%s", l, iv.Value), nil
	case LLLVAR:
		l := len(iv.Value)
		if l > 999 {
			return "", fmt.Errorf("lllvar too long")
		}
		return fmt.Sprintf("%03d%s", l, iv.Value), nil
	case DATE14:
		return formatNumeric(iv.Value, 14)
	case DATE12:
		return formatNumeric(iv.Value, 12)
	case DATE10:
		return formatNumeric(iv.Value, 10)
	case DATE6:
		return formatNumeric(iv.Value, 6)
	case DATE4, DATE_EXP:
		return formatNumeric(iv.Value, 4)
	case TIME:
		return formatNumeric(iv.Value, 6)
	case AMOUNT:
		return formatNumeric(iv.Value, 12)
	case BINARY:
		if iv.Length <= 0 {
			return "", fmt.Errorf("binary type requires length")
		}
		if len(iv.Value) > iv.Length {
			return iv.Value[:iv.Length], nil
		}
		if len(iv.Value) < iv.Length {
			return iv.Value + strings.Repeat("0", iv.Length-len(iv.Value)), nil
		}
		return iv.Value, nil
	case LLBIN:
		l := len(iv.Value)
		if l > 99 {
			return "", fmt.Errorf("llbin too long")
		}
		return fmt.Sprintf("%02d%s", l, iv.Value), nil
	case LLLBIN:
		l := len(iv.Value)
		if l > 999 {
			return "", fmt.Errorf("lllbin too long")
		}
		return fmt.Sprintf("%03d%s", l, iv.Value), nil
	case LLLLVAR:
		l := len(iv.Value)
		if l > 9999 {
			return "", fmt.Errorf("llllvar too long")
		}
		return fmt.Sprintf("%04d%s", l, iv.Value), nil
	case LLLLBIN:
		l := len(iv.Value)
		if l > 9999 {
			return "", fmt.Errorf("llllbin too long")
		}
		return fmt.Sprintf("%04d%s", l, iv.Value), nil
	default:
		return "", fmt.Errorf("unsupported type %d", iv.Type)
	}
}

// FieldSpec describes how to parse a field when decoding messages.
type FieldSpec struct {
	Type   IsoType
	Length int // only for fixed length types
}

// IsoMessage represents an ISO8583 message.
type IsoMessage struct {
	Mti    string
	Fields map[int]IsoValue
}

// NewMessage creates a new IsoMessage.
func NewMessage(mti string) *IsoMessage {
	return &IsoMessage{Mti: mti, Fields: make(map[int]IsoValue)}
}

// SetField sets the field value.
func (m *IsoMessage) SetField(field int, v IsoValue) {
	if m.Fields == nil {
		m.Fields = make(map[int]IsoValue)
	}
	m.Fields[field] = v
}

// GetField retrieves a field value.
func (m *IsoMessage) GetField(field int) (IsoValue, bool) {
	v, ok := m.Fields[field]
	return v, ok
}

// Pack encodes the message into its byte representation.
func (m *IsoMessage) Pack() ([]byte, error) {
	maxField := 0
	for f := range m.Fields {
		if f < 2 || f > 128 {
			return nil, fmt.Errorf("field %d not supported", f)
		}
		if f > maxField {
			maxField = f
		}
	}

	bitmapLen := 8
	if maxField > 64 {
		bitmapLen = 16
	}
	bitmap := make([]byte, bitmapLen)
	for f := range m.Fields {
		idx := (f - 1) / 8
		bit := byte(1 << (7 - ((f - 1) % 8)))
		bitmap[idx] |= bit
	}
	if bitmapLen == 16 {
		bitmap[0] |= 0x80 // secondary bitmap indicator
	}

	var bmp strings.Builder
	for _, b := range bitmap {
		fmt.Fprintf(&bmp, "%02X", b)
	}

	var sb strings.Builder
	sb.WriteString(m.Mti)
	sb.WriteString(bmp.String())

	keys := make([]int, 0, len(m.Fields))
	for k := range m.Fields {
		keys = append(keys, k)
	}
	sort.Ints(keys)
	for _, k := range keys {
		enc, err := m.Fields[k].encode()
		if err != nil {
			return nil, err
		}
		sb.WriteString(enc)
	}

	return []byte(sb.String()), nil
}

// Parse decodes the provided data into an IsoMessage using the supplied specs.
func Parse(data []byte, specs map[int]FieldSpec) (*IsoMessage, error) {
	if len(data) < 20 {
		return nil, fmt.Errorf("data too short")
	}
	mti := string(data[:4])
	bmpHex := string(data[4:20])
	bmpBytes, err := hex.DecodeString(bmpHex)
	if err != nil {
		return nil, err
	}

	pos := 20
	// check for secondary bitmap
	if bmpBytes[0]&0x80 != 0 {
		if len(data) < 36 {
			return nil, fmt.Errorf("data too short for secondary bitmap")
		}
		bmpHex2 := string(data[20:36])
		bmpBytes2, err := hex.DecodeString(bmpHex2)
		if err != nil {
			return nil, err
		}
		bmpBytes = append(bmpBytes, bmpBytes2...)
		pos = 36
	}

	msg := NewMessage(mti)
	totalFields := len(bmpBytes) * 8
	for field := 2; field <= totalFields; field++ {
		idx := (field - 1) / 8
		bit := byte(1 << (7 - ((field - 1) % 8)))
		if bmpBytes[idx]&bit == 0 {
			continue
		}
		spec, ok := specs[field]
		if !ok {
			return nil, fmt.Errorf("no spec for field %d", field)
		}
		switch spec.Type {
		case Numeric, Alpha, DATE14, DATE12, DATE10, DATE6, DATE4, DATE_EXP, TIME, AMOUNT, BINARY:
			end := pos + spec.Length
			if end > len(data) {
				return nil, fmt.Errorf("insufficient data for field %d", field)
			}
			val := string(data[pos:end])
			pos = end
			if spec.Type == Alpha {
				val = strings.TrimRight(val, " ")
			}
			msg.SetField(field, IsoValue{Type: spec.Type, Value: val, Length: spec.Length})
		case LLVAR:
			if pos+2 > len(data) {
				return nil, fmt.Errorf("insufficient data for llvar length field %d", field)
			}
			l, err := strconv.Atoi(string(data[pos : pos+2]))
			if err != nil {
				return nil, err
			}
			pos += 2
			end := pos + l
			if end > len(data) {
				return nil, fmt.Errorf("insufficient data for field %d", field)
			}
			val := string(data[pos:end])
			pos = end
			msg.SetField(field, IsoValue{Type: spec.Type, Value: val})
		case LLLVAR:
			if pos+3 > len(data) {
				return nil, fmt.Errorf("insufficient data for lllvar length field %d", field)
			}
			l, err := strconv.Atoi(string(data[pos : pos+3]))
			if err != nil {
				return nil, err
			}
			pos += 3
			end := pos + l
			if end > len(data) {
				return nil, fmt.Errorf("insufficient data for field %d", field)
			}
			val := string(data[pos:end])
			pos = end
			msg.SetField(field, IsoValue{Type: spec.Type, Value: val})
		case LLBIN:
			if pos+2 > len(data) {
				return nil, fmt.Errorf("insufficient data for llbin length field %d", field)
			}
			l, err := strconv.Atoi(string(data[pos : pos+2]))
			if err != nil {
				return nil, err
			}
			pos += 2
			end := pos + l
			if end > len(data) {
				return nil, fmt.Errorf("insufficient data for field %d", field)
			}
			val := string(data[pos:end])
			pos = end
			msg.SetField(field, IsoValue{Type: spec.Type, Value: val})
		case LLLBIN:
			if pos+3 > len(data) {
				return nil, fmt.Errorf("insufficient data for lllbin length field %d", field)
			}
			l, err := strconv.Atoi(string(data[pos : pos+3]))
			if err != nil {
				return nil, err
			}
			pos += 3
			end := pos + l
			if end > len(data) {
				return nil, fmt.Errorf("insufficient data for field %d", field)
			}
			val := string(data[pos:end])
			pos = end
			msg.SetField(field, IsoValue{Type: spec.Type, Value: val})
		case LLLLVAR:
			if pos+4 > len(data) {
				return nil, fmt.Errorf("insufficient data for llllvar length field %d", field)
			}
			l, err := strconv.Atoi(string(data[pos : pos+4]))
			if err != nil {
				return nil, err
			}
			pos += 4
			end := pos + l
			if end > len(data) {
				return nil, fmt.Errorf("insufficient data for field %d", field)
			}
			val := string(data[pos:end])
			pos = end
			msg.SetField(field, IsoValue{Type: spec.Type, Value: val})
		case LLLLBIN:
			if pos+4 > len(data) {
				return nil, fmt.Errorf("insufficient data for llllbin length field %d", field)
			}
			l, err := strconv.Atoi(string(data[pos : pos+4]))
			if err != nil {
				return nil, err
			}
			pos += 4
			end := pos + l
			if end > len(data) {
				return nil, fmt.Errorf("insufficient data for field %d", field)
			}
			val := string(data[pos:end])
			pos = end
			msg.SetField(field, IsoValue{Type: spec.Type, Value: val})
		default:
			return nil, fmt.Errorf("unsupported type for field %d", field)
		}
	}

	return msg, nil
}
