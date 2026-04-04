// MIT License
//
// Copyright (c) 2020 - 2026 Arsene Tochemey Gandote
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
using System.Collections.Frozen;
using System.Collections.Generic;

namespace NetCore8583.Tlv
{
    /// <summary>
    /// Built-in dictionary of common EMV tags with human-readable descriptions.
    /// Tag identifiers are stored as uppercase hex strings.
    /// <para>
    /// Descriptions are sourced from the <b>EMV v4.3 Specification</b>
    /// (EMV Book 3 -- Application Specification, and EMV Book 1 -- Application Independent ICC to Terminal
    /// Interface Requirements), published by EMVCo.
    /// ISO 7816-4 tags (e.g. 70, 77, 84) follow their definitions in ISO/IEC 7816-4:2020.
    /// </para>
    /// </summary>
    public static class EmvTags
    {
        /// <summary>The EMV specification version these tag descriptions are sourced from.</summary>
        public const string SpecVersion = "EMV v4.3";

        /// <summary>Frozen lookup of tag hex string to description, covering the most commonly used EMV tags per <see cref="SpecVersion"/>.</summary>
        public static readonly FrozenDictionary<string, string> Descriptions =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                // Core DE 55 tags (EMV Book 3, Annex A)
                ["5F2A"] = "Transaction Currency Code",
                ["82"] = "Application Interchange Profile",
                ["84"] = "Dedicated File (DF) Name",
                ["95"] = "Terminal Verification Results",
                ["9A"] = "Transaction Date",
                ["9C"] = "Transaction Type",
                ["9F03"] = "Amount, Other (Numeric)",
                ["9F09"] = "Application Version Number (Terminal)",
                ["9F10"] = "Issuer Application Data",
                ["9F1A"] = "Terminal Country Code",
                ["9F26"] = "Application Cryptogram",
                ["9F27"] = "Cryptogram Information Data",
                ["9F33"] = "Terminal Capabilities",
                ["9F34"] = "Cardholder Verification Method (CVM) Results",
                ["9F35"] = "Terminal Type",
                ["9F36"] = "Application Transaction Counter (ATC)",
                ["9F37"] = "Unpredictable Number",

                // Card data (EMV Book 3)
                ["50"] = "Application Label",
                ["57"] = "Track 2 Equivalent Data",
                ["5A"] = "Application Primary Account Number (PAN)",
                ["5F24"] = "Application Expiration Date",
                ["5F25"] = "Application Effective Date",
                ["5F28"] = "Issuer Country Code",
                ["5F34"] = "Application PAN Sequence Number",

                // Transaction / terminal (EMV Book 3)
                ["9F02"] = "Amount, Authorised (Numeric)",
                ["9F06"] = "Application Identifier (AID) - Terminal",
                ["9F07"] = "Application Usage Control",
                ["9F08"] = "Application Version Number (Card)",
                ["9F0D"] = "Issuer Action Code - Default",
                ["9F0E"] = "Issuer Action Code - Denial",
                ["9F0F"] = "Issuer Action Code - Online",
                ["9F17"] = "PIN Try Counter",
                ["9F1E"] = "Interface Device (IFD) Serial Number",
                ["9F21"] = "Transaction Time",
                ["9F41"] = "Transaction Sequence Counter",

                // Templates and structural (ISO 7816-4 / EMV Book 1)
                ["70"] = "EMV Proprietary Template",
                ["77"] = "Response Message Template Format 2",
                ["80"] = "Response Message Template Format 1",
                ["94"] = "Application File Locator (AFL)",
            }.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);

        /// <summary>Looks up a human-readable description for the given tag hex string.</summary>
        /// <param name="tag">Tag hex string (e.g. "9F26").</param>
        /// <returns>The description if found; otherwise null.</returns>
        public static string GetDescription(string tag) =>
            Descriptions.TryGetValue(tag, out var desc) ? desc : null;
    }
}
