﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeluxeCarsDesktop.Interfaces
{
    public interface ICloseable
    {
        Action CloseAction { get; set; }
    }
}
