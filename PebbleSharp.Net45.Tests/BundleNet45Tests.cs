using Microsoft.VisualStudio.TestTools.UnitTesting;
using PebbleSharp.Core;
using PebbleSharp.Core.Tests.AbstractTests;

namespace PebbleSharp.Net45.Tests
{
    [TestClass]
    public class BundleNet45Tests : BasePebbleBundleTests
    {
        protected override IZip GetZip()
        {
            return new Zip();
        }

        [TestMethod]
        [TestCategory("NET 4.5")]
        public override void CanLoadInformationFromAppBundle()
        {
            RunCanLoadInformationFromAppBundle();
        }

        [TestMethod]
        [TestCategory( "NET 4.5" )]
        public override void CanLoadInformationFromFirmwareBundle()
        {
            RunCanLoadInformationFromFirmwareBundle();
        }
    }
}
