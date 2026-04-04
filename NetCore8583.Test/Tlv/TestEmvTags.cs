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

using NetCore8583.Tlv;
using Xunit;

namespace NetCore8583.Test.Tlv
{
    public class TestEmvTags
    {
        [Theory]
        [InlineData("9F26", "Application Cryptogram")]
        [InlineData("9F27", "Cryptogram Information Data")]
        [InlineData("9F10", "Issuer Application Data")]
        [InlineData("9F37", "Unpredictable Number")]
        [InlineData("9F36", "Application Transaction Counter (ATC)")]
        [InlineData("95", "Terminal Verification Results")]
        [InlineData("9A", "Transaction Date")]
        [InlineData("9C", "Transaction Type")]
        [InlineData("5F2A", "Transaction Currency Code")]
        [InlineData("82", "Application Interchange Profile")]
        [InlineData("9F1A", "Terminal Country Code")]
        [InlineData("9F03", "Amount, Other (Numeric)")]
        [InlineData("9F33", "Terminal Capabilities")]
        [InlineData("9F34", "Cardholder Verification Method (CVM) Results")]
        [InlineData("9F35", "Terminal Type")]
        [InlineData("9F09", "Application Version Number (Terminal)")]
        [InlineData("84", "Dedicated File (DF) Name")]
        public void KnownTagsHaveDescriptions(string tag, string expectedDescription)
        {
            Assert.Equal(expectedDescription, EmvTags.GetDescription(tag));
        }

        [Fact]
        public void CaseInsensitiveLookup()
        {
            Assert.Equal("Application Cryptogram", EmvTags.GetDescription("9f26"));
            Assert.Equal("Application Cryptogram", EmvTags.GetDescription("9F26"));
        }

        [Fact]
        public void UnknownTagReturnsNull()
        {
            Assert.Null(EmvTags.GetDescription("FF99"));
        }

        [Fact]
        public void DictionaryContainsAllRequiredTags()
        {
            string[] requiredTags =
            {
                "9F26", "9F27", "9F10", "9F37", "9F36",
                "95", "9A", "9C", "5F2A", "82",
                "9F1A", "9F03", "9F33", "9F34", "9F35",
                "9F09", "84"
            };

            foreach (var tag in requiredTags)
            {
                Assert.True(EmvTags.Descriptions.ContainsKey(tag),
                    $"Tag {tag} should be in the EMV dictionary.");
            }
        }
    }
}
