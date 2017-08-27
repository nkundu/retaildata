using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace retaildata
{
    public class Workers
    {
        public const string UPLOAD_PATH = "uploads";
        private const string PROCESSED_PATH = "processed";

        private bool parserIdle = true;

        public Workers()
        {
            new Thread(TakeFiles).Start();
        }

        public string GetStatus()
        {
            return "good";
        }

        private void TakeFiles()
        {
            Directory.CreateDirectory(UPLOAD_PATH);
            while (true)
            {
                var files = Directory.GetFiles(UPLOAD_PATH);
                if (files.Length > 0)
                {
                    ProcessFile(files[0]);
                }
                Thread.Sleep(1000);
            }
        }

        private void ProcessFile(string path)
        {
            FileInfo fi = new FileInfo(path);
            DirectoryInfo di = fi.Directory;
        }
    }
}
