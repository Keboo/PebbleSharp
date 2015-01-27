namespace PebbleSharp.Core.Responses
{
    [Endpoint( Endpoint.ApplicationMessage )]
    public class ApplicationMessageResponse : ResponseBase
    {
        protected override void Load(byte[] payload)
        {
            
        }
    }
}