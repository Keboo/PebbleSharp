using Microsoft.VisualStudio.TestTools.UnitTesting;
using PebbleSharp.Core.Bundles;
using PebbleSharp.Core.Tests.Resources;

namespace PebbleSharp.Core.Tests.AbstractTests
{
    public abstract class BaseCrc32Tests
    {
        protected abstract IZip GetZip();

        public abstract void GeneratesCorrectChecksumForApp();

        public abstract void GeneratesCorrectChecksumForFirmware();

        protected void RunGeneratesCorrectChecksumForApp()
        {
            var bundle = new AppBundle();
            bundle.Load(ResourceManager.GetAppBundle(), GetZip());

            Assert.AreEqual(bundle.Manifest.Application.CRC, Crc32.Calculate(bundle.App));
        }

        protected void RunGeneratesCorrectChecksumForFirmware()
        {
            var bundle = new FirmwareBundle();
            bundle.Load(ResourceManager.GetFirmwareBundle(), GetZip());

            Assert.AreEqual(bundle.Manifest.Firmware.CRC, Crc32.Calculate(bundle.Firmware));
        }
    }
}
