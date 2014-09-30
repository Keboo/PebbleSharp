using System;
using PebbleSharp.Core;

namespace PebbleSharp.WPF.Messages
{
    public class PebbleConnected
    {
        private readonly Pebble _pebble;

        public PebbleConnected( Pebble pebble )
        {
            if (pebble == null) throw new ArgumentNullException("pebble");
            _pebble = pebble;
        }

        public Pebble Pebble
        {
            get { return _pebble; }
        }
    }
}