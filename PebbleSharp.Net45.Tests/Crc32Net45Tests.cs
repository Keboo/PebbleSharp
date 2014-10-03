using Microsoft.VisualStudio.TestTools.UnitTesting;
using PebbleSharp.Core;
using PebbleSharp.Core.Tests.AbstractTests;

namespace PebbleSharp.Net45.Tests
{
    [TestClass]
    public class Crc32Net45Tests : BaseCrc32Tests
    {
        protected override IZip GetZip()
        {
            return new Zip();
        }

        [TestMethod]
        [TestCategory( "NET 4.5" )]
        public override void GeneratesCorrectChecksumForApp()
        {
            RunGeneratesCorrectChecksumForApp();
        }

        [TestMethod]
        [TestCategory( "NET 4.5" )]
        public override void GeneratesCorrectChecksumForFirmware()
        {
            RunGeneratesCorrectChecksumForFirmware();
        }
    }
}
