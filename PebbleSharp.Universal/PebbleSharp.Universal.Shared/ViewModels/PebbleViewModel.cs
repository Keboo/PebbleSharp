using System.Threading.Tasks;

namespace PebbleSharp.Universal.Universal.ViewModels
{
    public abstract class PebbleViewModel : ViewModelBase
    {
        public Core.Pebble Pebble { get; set; }

        public abstract Task RefreshAsync();
    }
}