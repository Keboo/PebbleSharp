using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PebbleSharp.Core.Responses;

namespace PebbleSharp.Core.Tests.Responses
{
    [TestClass]
    public class ApplicationMessageResponseTests
    {
        [TestMethod]
        public void ItParsesResponsedataCorrectly()
        {
            var data = new byte[]
            {
                0x01, 0x0B, 0x6B, 0xF6, 0x21, 0x5B, 0xC9, 0x7F, 0x40, 0x9E, 0x8C, 0x31, 0x4F, 0x55, 0x65, 0x72, 0x22,
                0xB4, 0x01, 0x01, 0x00, 0x00, 0x00, 0x02, 0x04, 0x00, 0x01, 0x00, 0x00, 0x00
            };

            var response = new ApplicationMessageResponse();

            response.SetPayload( data );

            Assert.AreEqual( 1, response.ParsdData.Count );
            Assert.AreEqual( "1", response.ParsdData.Keys.Single() );
            Assert.AreEqual( 1u, response.ParsdData["1"] );
            var expectedUUID = new UUID( new byte[]
            {
                0x6B, 0xF6, 0x21, 0x5B, 
                0xC9, 0x7F, 0x40, 0x9E, 
                0x8C, 0x31, 0x4F, 0x55, 
                0x65, 0x72, 0x22, 0xB4
            } );
            Assert.AreEqual( expectedUUID, response.TargetUUID );
        }
    }
}