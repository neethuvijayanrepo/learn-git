using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SEIDR.FileSystem;
using SEIDR.JobBase;
using System.IO.Compression;

namespace JobUnitTest.Deployment
{
    [TestClass]
    public class DeploymentPrep : ContextJobTestBase<DeploymentPrepJob, DeploymentPrepContext>
    {
        DirectoryInfo GetChildDirectory(DirectoryInfo parent, params string[] childPaths)
        {
            string[] paths = new string[childPaths.Length + 1];
            paths[0] = parent.FullName;
            for (int i = 0; i < childPaths.Length; i++)
            {
                paths[i + 1] = childPaths[i];
            }
            return new DirectoryInfo(Path.Combine(paths));
        }

        public void CopyMatchesToFolder(DirectoryInfo target, IEnumerable<FileInfo> matches)
        {
            foreach (var f in matches)
            {
                f.CopyTo(Path.Combine(target.FullName, f.Name));
            }
        }

        public void checkSqlChangeSubFolders(DirectoryInfo checkFolder, DirectoryInfo targetRoot, DateTime checkTime, bool subDir)
        {
            bool skipBIA = checkFolder.Name.Equals(CHANGE_SCRIPTS, StringComparison.OrdinalIgnoreCase);
            var files = checkFolder.EnumerateFiles("*.sql", SearchOption.TopDirectoryOnly);
            if (skipBIA)
                files = files.Where(f => !f.Name.StartsWith("BIA-", StringComparison.OrdinalIgnoreCase));
            CopyMatchesToFolder(targetRoot, files.Where(f => f.LastWriteTime > checkTime));
            var folders = checkFolder.EnumerateDirectories().Where(d => d.LastWriteTime > checkTime);
            if (skipBIA)
                folders = folders.Where(f => !f.Name.StartsWith("BIA-", StringComparison.OrdinalIgnoreCase));
            foreach (var f in folders)
            {
                DirectoryInfo next = targetRoot;
                if (subDir)
                    next = targetRoot.CreateSubdirectory(f.Name);
                checkSqlChangeSubFolders(f, next, checkTime, subDir);
            }
        }

        void copyFolderStructure(DirectoryInfo source, DirectoryInfo target)
        {
            if (!target.Exists)
                target.Create();
            foreach (var subFolder in source.EnumerateDirectories("*", SearchOption.TopDirectoryOnly))
            {
                var targ2 = target.CreateSubdirectory(subFolder.Name);
                copyFolderStructure(subFolder, targ2);
            }
        }
        private const string ANDROMEDA_DATA = "Cymetrix.Andromeda.Data";
        private const string CHANGE_SCRIPTS = "Change Scripts"; //Still need to manually go through and identify data scripts and Andromeda_Staging data.
        private const string VIEWS = "Views";
        private const string FUNCTIONS = "Functions";
        private const string PROCEDURES = "Stored Procedures";
        private const string TRIGGERS = "Triggers";
        [TestMethod]
        public void PrepEnvironmentDeploymentZip()
        {

            var gitFolder = new DirectoryInfo(Environment.CurrentDirectory).Parent.Parent.Parent.Parent;
            bool UAT = gitFolder.Name.Contains("UAT", StringComparison.OrdinalIgnoreCase);
            var gitHead = gitFolder.GetDirectories(".git")[0].GetFiles("HEAD")[0];
            var changeTime = gitHead.CreationTime.AddMinutes(1); //Give a bit of a buffer for files brought in with latest pull.

            string deploymentRoot = $@"C:\Deployment\{(UAT ? "UAT" : "SQL")}_{DateTime.Today:yyyy_MM_dd}";
            string outputFolder = null; //if null, use parent directory. 
            DirectoryInfo di = new DirectoryInfo(deploymentRoot);
            if(!di.Exists)
            //if (!di.EnumerateFiles("*.sql", SearchOption.AllDirectories).Any())
            {
                var template = new DirectoryInfo($@"C:\Deployment\{(UAT ? "UAT" : "SQL")}_TEMPLATE");
                copyFolderStructure(template, di);
                di.Refresh();
                var Andromeda = di.EnumerateDirectories("*Andromeda").FirstOrDefault();
                var changeScripts = GetChildDirectory(Andromeda, "1. " + CHANGE_SCRIPTS);
                var source = GetChildDirectory(gitFolder, ANDROMEDA_DATA, CHANGE_SCRIPTS);
                checkSqlChangeSubFolders(source, changeScripts, changeTime, true);
                /*var files = source.EnumerateFiles("*.sql", SearchOption.TopDirectoryOnly).Where(f => f.LastWriteTime >= changeTime);
                CopyMatchesToFolder(changeScripts, files);
                */
                var views = GetChildDirectory(Andromeda, "2. " + VIEWS);
                source = GetChildDirectory(gitFolder, ANDROMEDA_DATA, VIEWS);
                checkSqlChangeSubFolders(source, views, changeTime, true);
                var functions = GetChildDirectory(Andromeda, "3. " + FUNCTIONS);
                source = GetChildDirectory(gitFolder, ANDROMEDA_DATA, FUNCTIONS);
                checkSqlChangeSubFolders(source, functions, changeTime, true);
                var procedures = GetChildDirectory(Andromeda, "4. " + PROCEDURES);
                source = GetChildDirectory(gitFolder, ANDROMEDA_DATA, PROCEDURES);
                checkSqlChangeSubFolders(source, procedures, changeTime, true);
                var triggers = GetChildDirectory(Andromeda, "5. " + TRIGGERS);
                source = GetChildDirectory(gitFolder, ANDROMEDA_DATA, TRIGGERS);
                checkSqlChangeSubFolders(source, triggers, changeTime, true);
                System.Diagnostics.Debug.WriteLine("REVIEW FILES COPIED");
                return;
            }

            _TestExecution.FilePath = deploymentRoot;
            ExecuteTest();
            di.Refresh();
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            if (string.IsNullOrEmpty(outputFolder) || outputFolder == deploymentRoot)
                outputFolder = di.Parent.FullName; //Don't put in the same folder that's being zipped up - can cause issues.

            string zipOut = Path.Combine(outputFolder, di.Name + ".ZIP");
            if (File.Exists(zipOut))
            {
                File.Delete(zipOut);
            }

            ZipFile.CreateFromDirectory(deploymentRoot, zipOut, CompressionLevel.Optimal, false);
        }
    }
}
