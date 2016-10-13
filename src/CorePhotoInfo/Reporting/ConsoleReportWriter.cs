using System;

namespace CorePhotoInfo.Reporting
{
    public class ConsoleReportWriter : IReportWriter
    {

        private static ConsoleColor HeaderColor = ConsoleColor.DarkRed;
        private static ConsoleColor SubheaderColor = ConsoleColor.Yellow;

        public void WriteHeader(string format, params object[] arg)
        {
            var str = string.Format(format, arg);
            WriteHeader(str, HeaderColor, '=');
        }

        public void WriteSubheader(string format, params object[] arg)
        {
            var str = string.Format(format, arg);
            WriteHeader(str, SubheaderColor, '-');
        }

        public void WriteLine(string format, params object[] arg)
        {
            var str = string.Format(format, arg);
            Console.WriteLine(str);
        }

        public void WriteHeader(string str, ConsoleColor color, char underlineChar)
        {
            var underlineStr = new string(underlineChar, str.Length);

            Console.ForegroundColor = color;
            Console.WriteLine(str);
            Console.WriteLine(underlineStr);
            Console.ResetColor();
        }
    }
}