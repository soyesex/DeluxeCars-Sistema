using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeluxeCarsDesktop.Utils
{
    public class DisplayItem<T>
    {
        public T Value { get; set; }
        public string DisplayText { get; set; }
    }
}
