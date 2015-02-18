using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PebbleSharp.Core;
using PebbleSharp.Core.Responses;

namespace PebbleSharp.Net45.Tests
{
    [TestClass]
    public class IntegrationTests
    {
        private static Pebble _Pebble;

        [ClassInitialize]
        public static void Startup( TestContext context )
        {
            _Pebble = PebbleNet45.DetectPebbles().SingleOrDefault();
            Assert.IsNotNull( _Pebble, "Could not find pebble" );
            _Pebble.ConnectAsync().Wait();
        }

        [ClassCleanup]
        public static void Cleanup()
        {
            _Pebble.Disconnect();
        }

        [TestMethod]
        public async Task CanGetCurrentTime()
        {
            TimeResponse response = await _Pebble.GetTimeAsync();
            AssertSuccessfulResult( response );
        }

        [TestMethod]
        public async Task CanSetTime()
        {
            await _Pebble.SetTimeAsync( DateTime.Now );
            TimeResponse respnse = await _Pebble.GetTimeAsync();
            AssertSuccessfulResult( respnse );
            Assert.IsTrue( DateTime.Now - respnse.Time < TimeSpan.FromSeconds( 10 ) );
        }

        [TestMethod]
        public async Task CanGetFirmwareInfo()
        {
            FirmwareVersionResponse firmwareResponse = await _Pebble.GetFirmwareVersionAsync();
            AssertSuccessfulResult( firmwareResponse );
            Assert.IsNotNull( firmwareResponse.Firmware );
            Assert.IsNotNull( firmwareResponse.RecoveryFirmware );
        }

        // ReSharper disable once UnusedParameter.Local
        private static void AssertSuccessfulResult<T>( T response )
            where T : ResponseBase
        {
            Assert.IsNotNull( response, string.Format( "{0} was null", typeof( T ).Name ) );
            Assert.IsTrue( response.Success, string.Concat( Environment.NewLine,
                string.Format( "{0} failed", typeof( T ).Name ),
                response.ErrorMessage,
                response.ErrorDetails ) );
        }
    }
}