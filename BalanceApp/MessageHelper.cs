using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace BalanceApp
{
    public class MessageHelper
    {
        public const int WM_COPYDATA = 0x004A;

        [DllImport("User32.dll", EntryPoint = "SendMessage")]
        private static extern int SendMessage
        (
            IntPtr hWnd,                   //目标窗体句柄
            int Msg,                       //WM_COPYDATA
            int wParam,                    //自定义数值
            ref CopyDataStruct lParam     //结构体
        );

        [DllImport("User32.dll", EntryPoint = "FindWindow")]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        public static void SendMessage(string windowName, string strMsg)
        {

            if (strMsg == null) return;

            IntPtr hwnd = FindWindow(null, windowName);

            if (hwnd != IntPtr.Zero)
            {
                CopyDataStruct cds;

                cds.dwData = IntPtr.Zero;
                cds.lpData = strMsg;

                cds.cbData = System.Text.Encoding.Default.GetBytes(strMsg).Length + 1;

                int fromWindowHandler = 0;
                SendMessage(hwnd, WM_COPYDATA, fromWindowHandler, ref cds);
            }
        }

        public static void SendMessageByProcess(string processName, string strMsg)
        {
            if (strMsg == null) return;
            var process = Process.GetProcessesByName(processName);
            if (process.FirstOrDefault() == null) return;
            var hwnd = process.FirstOrDefault().MainWindowHandle;
            if (hwnd == IntPtr.Zero) return;

            if (hwnd != IntPtr.Zero)
            {
                CopyDataStruct cds;

                cds.dwData = IntPtr.Zero;
                cds.lpData = strMsg;

                cds.cbData = System.Text.Encoding.Default.GetBytes(strMsg).Length + 1;

                int fromWindowHandler = 0;
                SendMessage(hwnd, WM_COPYDATA, fromWindowHandler, ref cds);

            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct CopyDataStruct
        {
            public IntPtr dwData;
            public int cbData;

            [MarshalAs(UnmanagedType.LPStr)]
            public string lpData;
        }
    }
}
