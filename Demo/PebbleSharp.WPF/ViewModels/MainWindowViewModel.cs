using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using GalaSoft.MvvmLight;
using PebbleSharp.Net45;

namespace PebbleSharp.WPF.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly IList<PebbleViewModel> _pebbleDevices;

        public MainWindowViewModel()
        {
            if ( IsInDesignMode == false )
            {
                _pebbleDevices = new List<PebbleViewModel>( PebbleNet45.DetectPebbles().Select( x => new PebbleViewModel( x ) ) );
            }
        }

        public ICollectionView PebbleDevices
        {
            get { return CollectionViewSource.GetDefaultView( _pebbleDevices ); }
        }
    }
}