using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SimpleApp
{
    public class MessageHelper
    {
        public const int WM_COPYDATA = 0x004A;
    }


    [StructLayout(LayoutKind.Sequential)]
    public struct CopyDataStruct
    {

        public IntPtr dwData;

        public int cbData;//字符串长度

        [MarshalAs(UnmanagedType.LPStr)]
        public string lpData;//字符串
    }
}
