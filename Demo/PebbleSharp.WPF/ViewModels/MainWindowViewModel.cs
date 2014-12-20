using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
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
                try
                {
                    var pebbles = PebbleNet45.DetectPebbles();
                    _pebbleDevices = new List<PebbleViewModel>( pebbles.Select( x => new PebbleViewModel( x ) ) );
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                
            }
        }

        public ICollectionView PebbleDevices
        {
            get { return CollectionViewSource.GetDefaultView( _pebbleDevices ); }
        }
    }
}