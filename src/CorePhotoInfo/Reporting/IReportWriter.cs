using System.IO;

namespace CorePhotoInfo.Reporting
{
    public interface IReportWriter
    {
        void WriteHeader(string format, params object[] arg);
        void WriteSubheader(string format, params object[] arg);
        void WriteLine(string format, params object[] arg);
        void WriteError(string format, params object[] arg);
        void WriteImage(FileInfo imageFile);
    }
}