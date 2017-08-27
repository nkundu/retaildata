using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace retaildata
{
    public class Workers
    {
        public const string UPLOAD_PATH = "uploads";
        private const string PROCESSED_PATH = "processed";
        private DataManager dataManager;

        private bool parserIdle = true;

        public Workers(DataManager data)
        {
            dataManager = data;
            new Thread(TakeFiles).Start();
        }

        public string GetStatus()
        {
            return "good";
        }

        private void TakeFiles()
        {
            Directory.CreateDirectory(UPLOAD_PATH);
            Directory.CreateDirectory(PROCESSED_PATH);
            while (true)
            {
                var files = Directory.GetFiles(UPLOAD_PATH);
                if (files.Length > 0)
                {
                    foreach (var f in files)
                        if (ProcessFile(f))
                            break;
                } else
                {
                    var upc = dataManager.GetRandomUPC();
                    if (!dataManager.HasData(upc))
                        dataManager.GetData(upc);
                }
                Thread.Sleep(1000);
            }
        }

        private bool ProcessFile(string path)
        {
            if (!path.EndsWith(".bin"))
                return false;

            FileInfo fi = new FileInfo(path);
            DirectoryInfo di = fi.Directory;
            var baseName = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(fi.Name));
            var infoPath = Path.Combine(di.FullName, baseName + ".txt");
            var resultPath = Path.Combine(di.FullName, baseName + ".result.txt");
            try
            {
                var upcs = File.ReadAllLines(path);
                File.AppendAllText(resultPath, ProcessUpcs(upcs));

                File.Move(path, Path.Combine(PROCESSED_PATH, fi.Name));
                File.Move(infoPath, Path.Combine(PROCESSED_PATH, baseName + ".txt"));
                File.Move(resultPath, Path.Combine(PROCESSED_PATH, baseName + ".result.txt"));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return true;
        }

        private string ProcessUpcs(IEnumerable<string> upcs)
        {
            var result = new StringBuilder();
            foreach (var upc in upcs)
            {
                if (!IsValidGtin(upc))
                {
                    result.AppendLine(upc + " is not a valid UPC");
                    continue;
                }

                if (!dataManager.HasData(upc))
                    dataManager.GetData(upc);
            }

            return result.ToString();
        }

        public static bool IsValidGtin(string code)
        {
            if (code != (new Regex("[^0-9]")).Replace(code, ""))
            {
                // is not numeric
                return false;
            }
            // pad with zeros to lengthen to 14 digits
            switch (code.Length)
            {
                case 8:
                    code = "000000" + code;
                    break;
                case 12:
                    code = "00" + code;
                    break;
                case 13:
                    code = "0" + code;
                    break;
                case 14:
                    break;
                default:
                    // wrong number of digits
                    return false;
            }
            // calculate check digit
            int[] a = new int[13];
            a[0] = int.Parse(code[0].ToString()) * 3;
            a[1] = int.Parse(code[1].ToString());
            a[2] = int.Parse(code[2].ToString()) * 3;
            a[3] = int.Parse(code[3].ToString());
            a[4] = int.Parse(code[4].ToString()) * 3;
            a[5] = int.Parse(code[5].ToString());
            a[6] = int.Parse(code[6].ToString()) * 3;
            a[7] = int.Parse(code[7].ToString());
            a[8] = int.Parse(code[8].ToString()) * 3;
            a[9] = int.Parse(code[9].ToString());
            a[10] = int.Parse(code[10].ToString()) * 3;
            a[11] = int.Parse(code[11].ToString());
            a[12] = int.Parse(code[12].ToString()) * 3;
            int sum = a[0] + a[1] + a[2] + a[3] + a[4] + a[5] + a[6] + a[7] + a[8] + a[9] + a[10] + a[11] + a[12];
            int check = (10 - (sum % 10)) % 10;
            // evaluate check digit
            int last = int.Parse(code[13].ToString());
            return check == last;
        }
    }
}
