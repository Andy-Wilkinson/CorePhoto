using System;
using System.IO;

namespace CorePhotoInfo.Reporting
{
    public class ConsoleReportWriter : IReportWriter
    {
        private static ConsoleColor ErrorColor = ConsoleColor.Red;
        private static ConsoleColor HeaderColor = ConsoleColor.DarkRed;
        private static ConsoleColor SubheaderColor = ConsoleColor.Yellow;
        private static ConsoleColor ImageColor = ConsoleColor.Green;

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

        public void WriteError(string format, params object[] arg)
        {
            var str = string.Format(format, arg);
            Console.ForegroundColor = ErrorColor;
            Console.WriteLine(str);
            Console.ResetColor();
        }

        public void WriteHeader(string str, ConsoleColor color, char underlineChar)
        {
            var underlineStr = new string(underlineChar, str.Length);

            Console.ForegroundColor = color;
            Console.WriteLine(str);
            Console.WriteLine(underlineStr);
            Console.ResetColor();
        }

        public void WriteImage(FileInfo imageFile)
        {
            Console.ForegroundColor = ImageColor;
            Console.WriteLine($"Output Image '{imageFile.Name}'");
            Console.ResetColor();
        }
    }
}