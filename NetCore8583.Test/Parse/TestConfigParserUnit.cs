using System;
using System.IO;
using System.Text;
using NetCore8583.Codecs;
using NetCore8583.Extensions;
using NetCore8583.Parse;
using Xunit;

namespace NetCore8583.Test.Parse
{
    /// <summary>
    /// Unit tests for <see cref="ConfigParser"/> focusing on:
    /// - <c>ConfigureFromClasspathConfig</c>: file-path-based configuration
    /// - <c>ConfigureFromDefault</c>: loading the default "n8583.xml"
    /// - <c>CreateDefault</c>: factory creation from default config
    /// - Error paths: missing files, invalid XML, malformed message types
    /// </summary>
    public class TestConfigParserUnit
    {
        // ═══════════════════════════════════════════════════════════════════════
        // ConfigureFromClasspathConfig
        // ═══════════════════════════════════════════════════════════════════════

        [Fact]
        public void ConfigureFromClasspathConfig_ValidConfig_ConfiguresHeaders()
        {
            var mfact = new MessageFactory<IsoMessage>();
            ConfigParser.ConfigureFromClasspathConfig(mfact, "/Resources/config.xml");

            Assert.NotNull(mfact.GetIsoHeader(0x800));
            Assert.NotNull(mfact.GetIsoHeader(0x810));
        }

        [Fact]
        public void ConfigureFromClasspathConfig_ValidConfig_ConfiguresTemplates()
        {
            var mfact = new MessageFactory<IsoMessage>();
            ConfigParser.ConfigureFromClasspathConfig(mfact, "/Resources/config.xml");

            var m200 = mfact.GetMessageTemplate(0x200);
            Assert.NotNull(m200);
            Assert.True(m200.HasField(3));
            Assert.True(m200.HasField(32));
            Assert.True(m200.HasField(49));
        }

        [Fact]
        public void ConfigureFromClasspathConfig_ValidConfig_ConfiguresParseGuides()
        {
            var mfact = new MessageFactory<IsoMessage>();
            ConfigParser.ConfigureFromClasspathConfig(mfact, "/Resources/config.xml");

            // Parse a 0800 message with known fields
            var s800 = "0800201080000000000012345611251125";
            var m = mfact.ParseMessage(s800.GetSignedBytes(), 0);
            Assert.NotNull(m);
            Assert.True(m.HasField(3));
            Assert.True(m.HasField(12));
            Assert.True(m.HasField(17));
        }

        [Fact]
        public void ConfigureFromClasspathConfig_ExtendsTemplate_InheritsFields()
        {
            var mfact = new MessageFactory<IsoMessage>();
            ConfigParser.ConfigureFromClasspathConfig(mfact, "/Resources/config.xml");

            var m200 = mfact.GetMessageTemplate(0x200);
            var m400 = mfact.GetMessageTemplate(0x400);
            Assert.NotNull(m400);

            // 0400 extends 0200 — inherited fields should match
            for (var i = 2; i < 89; i++)
            {
                var v = m200.GetField(i);
                if (v == null)
                    Assert.False(m400.HasField(i));
                else
                    Assert.True(m400.HasField(i));
            }
        }

        [Fact]
        public void ConfigureFromClasspathConfig_ExtendsTemplate_ExcludesFields()
        {
            var mfact = new MessageFactory<IsoMessage>();
            ConfigParser.ConfigureFromClasspathConfig(mfact, "/Resources/config.xml");

            var m200 = mfact.GetMessageTemplate(0x200);
            var m400 = mfact.GetMessageTemplate(0x400);

            // field 102 is excluded in 0400 but present in 0200
            Assert.True(m200.HasField(102));
            Assert.False(m400.HasField(102));

            // field 90 is added only in 0400
            Assert.False(m200.HasField(90));
            Assert.True(m400.HasField(90));
        }

        [Fact]
        public void ConfigureFromClasspathConfig_BinaryHeader_IsSetCorrectly()
        {
            var mfact = new MessageFactory<IsoMessage>();
            ConfigParser.ConfigureFromClasspathConfig(mfact, "/Resources/config.xml");

            // 0280 has a binary header (binary="true" with "ffffffff")
            var binHeader = mfact.GetBinaryIsoHeader(0x280);
            Assert.NotNull(binHeader);
            Assert.Equal(4, binHeader.Length);
            Assert.Equal(unchecked((sbyte) 0xff), binHeader[0]);
        }

        [Fact]
        public void ConfigureFromClasspathConfig_ExtendsParseGuide_InheritsAndExcludesFields()
        {
            var mfact = new MessageFactory<IsoMessage>();
            ConfigParser.ConfigureFromClasspathConfig(mfact, "/Resources/config.xml");

            var s810 = "08102010000002000000123456112500";
            var m = mfact.ParseMessage(s810.GetSignedBytes(), 0);
            Assert.NotNull(m);
            // 0810 extends 0800 but excludes field 17 and adds field 39
            Assert.False(m.HasField(17));
            Assert.True(m.HasField(39));
        }

        [Fact]
        public void ConfigureFromClasspathConfig_FileNotFound_ThrowsIoException()
        {
            var mfact = new MessageFactory<IsoMessage>();
            Assert.Throws<IOException>(() =>
                ConfigParser.ConfigureFromClasspathConfig(mfact, "/Resources/nonexistent.xml"));
        }

        [Fact]
        public void ConfigureFromClasspathConfig_CanBeCalledMultipleTimes()
        {
            // Calling configure multiple times should not throw
            var mfact = new MessageFactory<IsoMessage>();
            ConfigParser.ConfigureFromClasspathConfig(mfact, "/Resources/config.xml");
            ConfigParser.ConfigureFromClasspathConfig(mfact, "/Resources/config.xml");
            Assert.NotNull(mfact.GetIsoHeader(0x800));
        }

        // ═══════════════════════════════════════════════════════════════════════
        // ConfigureFromDefault
        // ═══════════════════════════════════════════════════════════════════════

        [Fact]
        public void ConfigureFromDefault_WhenFileNotFound_ThrowsFileNotFoundException()
        {
            // "n8583.xml" does not exist in the test output directory
            var mfact = new MessageFactory<IsoMessage>();
            Assert.Throws<FileNotFoundException>(() => ConfigParser.ConfigureFromDefault(mfact));
        }

        // ═══════════════════════════════════════════════════════════════════════
        // CreateDefault
        // ═══════════════════════════════════════════════════════════════════════

        [Fact]
        public void CreateDefault_WhenFileNotFound_Throws()
        {
            // Same as ConfigureFromDefault — n8583.xml doesn't exist
            Assert.ThrowsAny<Exception>(() => ConfigParser.CreateDefault());
        }

        // ═══════════════════════════════════════════════════════════════════════
        // Composite XML configs (reuse of composites.xml)
        // ═══════════════════════════════════════════════════════════════════════

        [Fact]
        public void ConfigureFromClasspathConfig_CompositeConfig_ParsesCompositeFields()
        {
            var mfact = new MessageFactory<IsoMessage>();
            ConfigParser.ConfigureFromClasspathConfig(mfact, "/Resources/composites.xml");

            var m = mfact.ParseMessage("01000040000000000000016one  03two12345.".GetSignedBytes(), 0);
            Assert.NotNull(m);
            var f = (CompositeField) m.GetObjectValue(10);
            Assert.NotNull(f);
            Assert.Equal(4, f.Values.Count);
        }

        [Fact]
        public void ConfigureFromClasspathConfig_IssueConfig_ParsesExtendedParseGuide()
        {
            var mfact = new MessageFactory<IsoMessage>();
            ConfigParser.ConfigureFromClasspathConfig(mfact, "/Resources/issue34.xml");

            var m200 = "0200422000000880800001X1231235959123456101010202020TERMINAL484";
            var m = mfact.ParseMessage(m200.GetSignedBytes(), 0);
            Assert.NotNull(m);
            Assert.Equal("X", m.GetObjectValue(2));
            Assert.Equal("123456", m.GetObjectValue(11));
        }
    }
}
