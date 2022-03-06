using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.IO;
using SEIDR;

namespace FixedWidthConverter
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            //Open GUI
            if (args.Length < 2)
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                FixWidthConverterForm fwc;
                if (args.Length == 1 && !args[0].ToLower().EndsWith("fwcs"))
                    fwc = new FixWidthConverterForm(args[0]);
                else if (args.Length == 1)
                {
                    fwc = new FixWidthConverterForm();
                    fwc.InitSettingsFile(args[0]);
                }
                else
                {
                    fwc = new FixWidthConverterForm();
                    if (System.Deployment.Application.ApplicationDeployment.IsNetworkDeployed)
                    {
                        string[] activationData = AppDomain.CurrentDomain.SetupInformation.ActivationArguments.ActivationData;
                        if (activationData != null && activationData.Length > 0)
                        {
                            string[] args2 = activationData[0].Split(new char[] { ',' });
                            if (args2.Length > 0 && !string.IsNullOrWhiteSpace(args2[0]))
                            {
                                //File.AppendAllText(@"C:\temp\FixedWidthLog.txt", "ARGS2: " + args2[0] + Environment.NewLine);
                                if(args2[0].StartsWith(@"file:///"))
                                {
                                    string settings = args2[0].Substring("file:///".Length);
                                    settings = Uri.UnescapeDataString(settings);
                                    //File.AppendAllText(@"C:\temp\FixedWidthLog.txt", settings + Environment.NewLine);
                                    fwc.InitSettingsFile(settings);
                                }                                
                            }
                        }
                    }
                }
                Application.Run(fwc);
            }//Else: Don't use GUI, just run the program.
            else
            {
                LikeExpressions LIKE = new LikeExpressions();
                if(!File.Exists(args[1]))
                    throw new Exception("Setting file does not exist.");
                if(File.Exists(args[0]))
                    FixWidthConverter.construct(args[0], args[1]).ConvertFile();
                else if (Directory.Exists(args[0]))
                {
                    DirectoryInfo di = new DirectoryInfo(args[0]);
                    string filter = Path.GetFileNameWithoutExtension(args[1]);
                    FileInfo[] fList = di.GetFiles();
                    foreach (var f in fList)
                    {
                        string check = System.Text.RegularExpressions.Regex.Replace(f.Name, "[ 0-9]+", "");
                        if (LIKE.Compare(check, filter))
                        {
                            try
                            {
                                FixWidthConverter.construct(f.FullName, args[1]).ConvertFile();
                            }
                            catch { }
                        }
                    }
                }
            }
        }
    }
}
