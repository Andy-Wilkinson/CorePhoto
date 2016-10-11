using System;

namespace CorePhoto
{
    public class ImageFormatException : Exception
    {
        public ImageFormatException() { }
        public ImageFormatException(string message) : base(message) { }
        public ImageFormatException(string message, System.Exception inner) : base(message, inner) { }
    }
}