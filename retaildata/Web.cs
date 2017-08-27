using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace retaildata
{
    public class Web : Nancy.NancyModule
    {
        public static Workers workers;

        public Web()
        {
            Get["/upload"] = parameters => {
                return View["../../views/upload.html", new Object()];
            };
            Get["/status"] = parameters => workers.GetStatus();
            Post["/uploadcomplete"] = parameters =>
            {
                var email = Request.Form["email"];
                var outputFiles = new List<string>();
                foreach (var file in Request.Files)
                {
                    var newFileNameBase = Guid.NewGuid().ToString();
                    var origFileName = file.Name;
                    var newFileName = newFileNameBase + origFileName.Substring(origFileName.LastIndexOf('.'), origFileName.Length - origFileName.LastIndexOf('.'));
                    outputFiles.Add(newFileName);
                    File.WriteAllText(Path.Combine(Workers.UPLOAD_PATH, newFileNameBase + ".txt"),
                        "Orig name|" + file.Name + Environment.NewLine +
                        "Email|" + email + Environment.NewLine +
                        "Content-type|" + file.ContentType);
                    using (var outputStream = new FileStream(Path.Combine(Workers.UPLOAD_PATH, newFileName + ".bin"), FileMode.Create))
                        file.Value.CopyTo(outputStream);
                }
                return View["../../views/upload_complete.html", new { OutputFiles = outputFiles }];
            };
        }
    }
}
