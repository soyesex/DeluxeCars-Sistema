using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeluxeCarsDesktop.ViewModel.ColumnView
{
    public class ColumnViewModel : ViewModelBase
    {
        private bool _isVisible = true;
        public string Header { get; set; }
        public bool IsVisible
        {
            get => _isVisible;
            set => SetProperty(ref _isVisible, value);
        }
    }
}
