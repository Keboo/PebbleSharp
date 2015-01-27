using System;

namespace PebbleCmd
{
    public class Menu
    {
        private readonly string[] _Options;

        public Menu( params string[] options )
        {
            if ( options == null || options.Length == 0 )
            {
                throw new InvalidOperationException( "Menu options must be specified" );
            }
            _Options = options;
        }

        public int ShowMenu()
        {
            for ( int selection = 0; selection == 0; selection = 0 )
            {
                for ( int i = 0; i < _Options.Length; i++ )
                {
                    Console.WriteLine( ( i + 1 ) + ") " + _Options[i] );
                }

                if ( int.TryParse( Console.ReadLine(), out selection ) &&
                    selection > 0 && selection <= _Options.Length )
                {
                    return selection - 1;
                }
            }
            return -1;
        }
    }
}