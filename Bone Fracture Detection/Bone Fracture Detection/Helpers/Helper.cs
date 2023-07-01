using DinkToPdf;
using MimeKit;
using MimeKit.Text;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

namespace Bone_Fracture_Detection.Helpers
{
    internal class Helper
    {
        #region Declarations
        private static object lockerFile = new Object();
        private string fromAddress = "bonedetection@gmail.com";
        private string fromPassword = "qcgpxsnvopmpxomk";
        private string fromAddressTitle = "Bone Detection";

        public Helper(){ }
        #endregion

        #region Methods
        public string runBatFile()
        {
            try
            {
                var workingDirectory = "D:\\FYP";

                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        WorkingDirectory = workingDirectory + "\\",
                        FileName = Path.Combine(workingDirectory, "run_prediction.bat"),
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true
                    }
                };


                var result = -5;

                List<string> outputResult = new List<string>();
                string logFileDestination = "D:\\FYP\\Bone Fracture Detection\\Bone Fracture Detection\\log\\";

                if (Directory.Exists(logFileDestination))
                {
                    Directory.CreateDirectory(logFileDestination);
                }

                string logFilePath = Path.Combine(workingDirectory, "outputLog.txt");

                if (!System.IO.File.Exists(logFilePath))
                {
                    var myFile = System.IO.File.Create(logFilePath);
                    myFile.Close();
                }

                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.OutputDataReceived += (sender, args) =>
                {
                    if (!string.IsNullOrEmpty(args.Data))
                    {
                        outputResult.Add(args.Data);
                        writeToFile(args.Data + " \n", logFilePath);
                    }
                };
                process.ErrorDataReceived += (sender, args) =>
                {
                    if (!string.IsNullOrEmpty(args.Data))
                    {
                        outputResult.Add("Error : " + args.Data);
                        writeToFile("Error : " + args.Data + " \n", logFilePath);
                    }
                };

                process.Start();
                writeToFile("Process Output Started (" + DateTime.Now.ToString() + ") : " + " \n", logFilePath);
                process.BeginErrorReadLine();
                process.BeginOutputReadLine();
                process.WaitForExit();
                result = process.ExitCode;

                if (result == 0)
                {
                    return "success";
                }
                else
                {
                    return string.Join(" \n", outputResult);
                }
            }
            catch (Exception e)
            {
                throw;
            }
            
        }

        public bool sendEmail(string email)
        {
            bool isSuccess = true;

            try
            {
                string smtpServer = "smtp.gmail.com";
                int smtpPortNumber = 587;

                var mimeMessage = new MimeMessage();
                mimeMessage.Subject = "Bone Fracture Report";
                mimeMessage.From.Add(new MailboxAddress(fromAddressTitle, fromAddress));
                mimeMessage.To.Add(new MailboxAddress("", email));

                var bodyBuilder  = new BodyBuilder()
                {
                    HtmlBody = "Hello,<br><br>" +
                    "Please find attached the bone fracture report <br><br>" +
                    "Regards,<br>" +
                    "Bone Fracture Detection Admin"
                };

                bodyBuilder.Attachments.Add("D:\\FYP\\Bone Fracture Detection\\Bone Fracture Detection\\Template\\BoneFractureReport.pdf");

                mimeMessage.Body = bodyBuilder.ToMessageBody();
                using (var client = new MailKit.Net.Smtp.SmtpClient())
                {
                    client.Connect(smtpServer, smtpPortNumber, false);
                    // Note: only needed if the SMTP server requires authentication  
                    // Error 5.5.1 Authentication   
                    client.Authenticate(fromAddress, fromPassword);
                    client.Send(mimeMessage);
                    client.Disconnect(true);
                }

                System.IO.File.Delete("D:\\FYP\\Bone Fracture Detection\\Bone Fracture Detection\\Template\\reportTemplate.html");
                System.IO.File.Delete("D:\\FYP\\Bone Fracture Detection\\Bone Fracture Detection\\Template\\BoneFractureReport.pdf");
                if (Directory.Exists("D:\\FYP\\runs"))
                {
                    Directory.Delete("D:\\FYP\\runs", true);
                }
                return isSuccess;

            }
            catch (Exception ex)
            {
                isSuccess = false;
                throw ex;
            }   
        }

        public void fillReportInformations(Dictionary<string,string> predictions,string firstname,string lastname)
        {
            string txtFilePath = "D:\\FYP\\Bone Fracture Detection\\Bone Fracture Detection\\Template\\reportTemplate.txt";
            string txtFileData = System.IO.File.ReadAllText(txtFilePath);
            string htmlFilePath = Path.ChangeExtension(txtFilePath, "html");

            txtFileData = txtFileData.Replace("%%firstname%%",firstname);
            txtFileData = txtFileData.Replace("%%lastname%%",lastname);
            txtFileData = txtFileData.Replace("%%binary%%", predictions["BINARY"]);
            txtFileData = txtFileData.Replace("%%advice%%", predictions["ADVICE"]);
            txtFileData = txtFileData.Replace("%%multi%%", ( predictions.ContainsKey("CLASS") ? predictions["CLASS"] : "" ));
            if (predictions["BINARY"].ToUpper() == "POSITIVE")
            {
                txtFileData = txtFileData.Replace("%%imagePath%%", "D:\\FYP\\runs\\detect\\predict\\image1.png");
            }
            else
            {
                txtFileData = txtFileData.Replace("%%imagePath%%", "D:\\FYP\\image_to_predict\\image1.png");
            }

            using (StreamWriter outputFile = new StreamWriter(htmlFilePath, false))
            {
                outputFile.WriteLine(txtFileData);
                outputFile.Close();
            }

            convertHtmlToPdf(htmlFilePath);

        }

        public void convertHtmlToPdf(string filePath)
        {
            string htmlFile = filePath;
            //string fileName = Path.GetFileNameWithoutExtension(filePath);
            string pdfFile = Path.Combine(Path.GetDirectoryName(filePath),"BoneFractureReport.pdf");

            CustomAssemblyLoadContext context = new CustomAssemblyLoadContext();
            string libPath = "D:\\FYP\\Bone Fracture Detection\\Bone Fracture Detection\\Libraries\\libwkhtmltox.dll";
            context.LoadUnmanagedLibrary(libPath);

            var document = new HtmlToPdfDocument()
            {
                GlobalSettings =
                {
                    ColorMode = ColorMode.Color,
                    Orientation = Orientation.Portrait,
                    PaperSize = PaperKind.A4,
                    Margins = new MarginSettings() { Top = default, Left = default, Right = default, Bottom = default }
                },
                Objects =
                {
                    new ObjectSettings()
                    {
                        HtmlContent = System.IO.File.ReadAllText(htmlFile),
                        WebSettings =
                        {
                            DefaultEncoding = "utf-8",
                            LoadImages = true
                        },
                    }
                }
            };

            var converter = new BasicConverter(new PdfTools());
            var pdf = converter.Convert(document);
            System.IO.File.WriteAllBytes(pdfFile, pdf);
        }

        public Dictionary<string,string> healthAdviceDescription(Dictionary<string,string> predictions)
        {
            string advice = "";

            if (predictions["BINARY"].ToUpper().Equals("NEGATIVE"))
            {
                advice = "There is no fracture you are safe";
            }


            if (predictions["BINARY"].ToUpper().Equals("POSITIVE") && int.Parse(predictions["BOXES"]) == 0)
            {
                advice = "It's not very clear if you have a fracture or not. You Should see a doctor";
            }

            if (predictions["BINARY"].ToUpper().Equals("POSITIVE") && int.Parse(predictions["BOXES"]) > 0)
            {
                advice = "You have a fracture in your " + predictions["CLASS"]+ ". You should see a doctor";
            }

            predictions.Add("ADVICE", advice);
            return predictions;
        }

        private void writeToFile(string text, string filePath)
        {
            try
            {
                lock (lockerFile)
                {
                    using (FileStream file = new FileStream(filePath, FileMode.Append, FileAccess.Write, FileShare.Read))
                    using (StreamWriter writer = new StreamWriter(file))
                    {
                        writer.WriteLine(text);
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }

        }


        #endregion

        #region Internal class
        internal class CustomAssemblyLoadContext : AssemblyLoadContext
        {
            public IntPtr LoadUnmanagedLibrary(string absolutePath)
            {
                return LoadUnmanagedDll(absolutePath);
            }

            protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
            {
                return LoadUnmanagedDllFromPath(unmanagedDllName);
            }
        }

        #endregion

    }
}
