using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SEIDR.FileSystem;
using SEIDR.JobBase;
using SEIDR;

namespace JobUnitTest.Deployment
{
    [IJobMetaData(nameof(DeploymentPrepJob), nameof(JobUnitTest),
        "Prepare Deployment scripts with database using.",
        NeedsFilePath: false, AllowRetry: false, ConfigurationTable: null)]
    public class DeploymentPrepJob : ContextJobBase<DeploymentPrepContext>
    {
        public void CopyToTrainingSubDir(DirectoryInfo training, DirectoryInfo sub)
        {
            var dest = training.CreateSubdirectory(sub.Name);
            foreach (var file in sub.EnumerateFiles())
            {
                string destPath = Path.Combine(dest.FullName, file.Name);
                file.CopyTo(destPath, true);
            }

            foreach (var sub2 in sub.EnumerateDirectories())
            {
                CopyToTrainingSubDir(dest, sub2); //E.g., '~\TRAINING\1. ChangeScripts\', '~\Andromeda\1. ChangeScripts\MET-1234\'
            }
        }

        public override void Process(DeploymentPrepContext context)
        {
            string rootFolder = context.CurrentFilePath;

            DirectoryInfo di = new DirectoryInfo(rootFolder);
            if (!di.Exists)
            {
                context.SetStatus(ResultStatusCode.NS);
                return;
            }

            if (context.CopyToTraining)
            {
                var andromeda = di
                                .EnumerateDirectories()
                                .Where(d => d.Name.EndsWith(".Andromeda", StringComparison.OrdinalIgnoreCase));
                var source = andromeda.FirstOrDefault();
                if(source != null)
                {
                    var training = source.Parent.CreateSubdirectory(context.TrainingRootFolder);
                    foreach (var sub in source.EnumerateDirectories())
                    {
                        CopyToTrainingSubDir(training, sub);
                    }
                }

            }

            di.Refresh();
            List<DirectoryInfo> toDelete = new List<DirectoryInfo>();
            foreach (var sub in di.EnumerateDirectories())
            {

                int separator = sub.Name.IndexOf('.');
                if (separator < 0)
                    continue;
                var DB = sub.Name.Substring(separator + 1);
                var server = sub.Name.Substring(0, separator);

                var fileSet = sub.EnumerateFiles("*.*", SearchOption.AllDirectories);
                // ReSharper disable once PossibleMultipleEnumeration
                if (fileSet.UnderMaximumCount(0))
                {
                    toDelete.Add(sub);
                    continue;
                }

                // ReSharper disable once PossibleMultipleEnumeration
                foreach (var path in fileSet
                    .Where(fi => !fi.Name.EndsWith(".PREPPED.SQL", StringComparison.OrdinalIgnoreCase) 
                                 && fi.Extension.Equals(".sql", StringComparison.OrdinalIgnoreCase)))
                {
                    string originalContent = File.ReadAllText(path.FullName);
                    while (originalContent.StartsWith("USE") || originalContent[0].In('\r', '\n'))
                    {
                        int idx = originalContent.IndexOfAny(new char[] {'\r', '\n'});
                        if (idx < 0)
                            break;
                        originalContent = originalContent.Substring(idx + 1);
                    }
                    string content = $"USE [{DB}]{Environment.NewLine}GO{Environment.NewLine}{File.ReadAllText(path.FullName)}";
                    string dir = path.Directory.FullName;
                    string output = Path.Combine(dir, path.Name.Replace(path.Extension, ".PREPPED.SQL"));
                    File.WriteAllText(output, content);
                    File.Delete(path.FullName);
                }

                foreach (var subDir in sub.EnumerateDirectories("*", SearchOption.AllDirectories))
                {
                    if (subDir.GetFiles("*.*", SearchOption.AllDirectories).UnderMaximumCount(0))
                    {
                        try
                        {
                            subDir.Delete(true);
                        }
                        catch { continue; }
                    }
                }
            }

            foreach (var del in toDelete)
            {
                try
                {
                    del.Delete(true);
                }
                catch
                {
                    continue;
                }
            }

        }
    }
}
