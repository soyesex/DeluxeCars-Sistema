using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace DeluxeCarsDesktop.Utils
{
    public static class SecureStringExtensions
    {
        public static string Unsecure(this SecureString secureString)
        {
            if (secureString == null) return string.Empty;
            IntPtr ptr = System.Runtime.InteropServices.Marshal.SecureStringToGlobalAllocUnicode(secureString);
            try
            {
                return System.Runtime.InteropServices.Marshal.PtrToStringUni(ptr);
            }
            finally
            {
                System.Runtime.InteropServices.Marshal.ZeroFreeGlobalAllocUnicode(ptr);
            }
        }
    }
}
