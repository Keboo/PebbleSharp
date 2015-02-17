using System;
using System.Diagnostics;

namespace PebbleSharp.Core.Responses
{
    [Endpoint( Endpoint.DataLog )]
    public class DataLogResponse : ResponseBase
    {
        protected override void Load( byte[] payload )
        {
            Debug.WriteLine( "DataLog payload: " + BitConverter.ToString( payload ) );
            //TODO:
        }
    }
}