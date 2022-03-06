using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SEIDR.JobBase;

namespace JobUnitTest
{
    public class ContextJobTestBase<T, C>: JobTestBase<T> 
        where T:ContextJobBase<C>
        where C:BaseContext, new()
    {
        public C MyContext { get; private set; }

        protected override void Init()
        {
            base.Init();
            MyContext = new C();
            MyContext.Init(_Executor, _TestExecution);
            BasicLocalFileHelper.ClearWorkingDirectory(MyContext);
        }

        public void ProcessTest()
        {
            try
            {
                _JOB.Process(MyContext);
            }
            catch (Exception ex)
            {
                MyContext.LogError("Context Job Test Base Error", ex);
            }
            finally
            {
                BasicLocalFileHelper.ClearWorkingDirectory(MyContext);
            }

        }
        public BasicLocalFileHelper GetLocalFile()
        {
            return MyContext.GetLocalFile();
        }

        public BasicLocalFileHelper GetLocalFile(string originalFile)
        {
            return MyContext.GetLocalFile(originalFile);
        }

        public DirectoryInfo ClearLocalWorkingDirectory()
        {
            var dir = new DirectoryInfo(BasicLocalFileHelper.DefaultWorkingDirectory);
            if (dir.Exists)
            {
                foreach (var fi in dir.GetFiles())
                {
                    fi.Delete();
                }
            }
            return dir;
        }

        public DirectoryInfo ClearLocalWorkingDirectory<F>(F fileHelper) where F : BasicLocalFileHelper
        {
            var dir = new DirectoryInfo(fileHelper.WorkDirectory);
            if (dir.Exists)
            {
                foreach (var fi in dir.GetFiles())
                {
                    fi.Delete();
                }
            }
            return dir;
        }
        [TestCleanup]
        public void Cleanup()
        {
            BasicLocalFileHelper.ClearWorkingDirectory(MyContext);
        }
    }
}
