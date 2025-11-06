using NoneBot.Common;
using NoneBot.Process;
using System;
using System.Runtime.InteropServices;

namespace NoneBot
{
    public class Program
    {
        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        const int SW_HIDE = 0;
        const int SW_SHOW = 5;

        /* https://aka.ms/dotnet-core-applaunch?missing_runtime=true&arch=x86&rid=win10-x86&apphost_version=6.0.27 */
        public static void Main(string[] args)
        {
            //var hWnd = GetConsoleWindow();
            //ShowWindow(hWnd, SW_HIDE);
            TSingleton<CProcess>.ms_hInstance.MainProcess();
        }
    }
}
