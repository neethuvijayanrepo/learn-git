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
    /// <summary>
    /// Test Path parsing of various file name paths. Files should not need to actually exist.
    /// </summary>
    [TestClass]
    public class RegistrationFileTest
    {
        
        [TestMethod]
        public void Test835()
        {
            string filePath = @"\\Sdsrv031.cymetrix.com\is\DATA\_SourceFiles\UAB\HQ_Hosp\SEIDR_835_SYNC_TEST\NDS_10118004.RMT";

            JobProfile jp = JobProfile.GetSample(40, FileDateMask: "*_<MM><DD><YY>*");
            TestPath(filePath, jp, new DateTime(2018, 10, 11), false);
        }
        [TestMethod]
        public void Test835_2()
        {
            string filePath = @"\\Sdsrv031.cymetrix.com\is\DATA\_SourceFiles\UAB\HQ_Hosp\SEIDR_835_SYNC_TEST\MCARE_10121802.RMT";
            JobProfile jp = JobProfile.GetSample(40, FileDateMask: "*_<MM><DD><YY>*");
            TestPath(filePath, jp, new DateTime(2018, 10, 12), true);
        }
        [TestMethod]
        public void Test835_NoWildcardMask()
        {
            string filePath = @"\\Sdsrv031.cymetrix.com\is\DATA\_SourceFiles\UAB\HQ_Hosp\SEIDR_835_SYNC_TEST\MCARE_10121802.RMT";
            JobProfile jp = JobProfile.GetSample(40, FileDateMask: "<MM><DD><YY>"); //No wildcards
            TestPath(filePath, jp, new DateTime(2018, 10, 12), true);
        }


        [TestMethod]
        public void Test835_3()
        {
            string filePath = @"\\Sdsrv031.cymetrix.com\is\DATA\_SourceFiles\UAB\HQ_Hosp\SEIDR_835_SYNC_TEST\MCARE_10121802.RMT";
            JobProfile jp = JobProfile.GetSample(40, FileDateMask: "MCare_<MM><DD><YY>*"); //case doesn't match
            TestPath(filePath, jp, new DateTime(2018, 10, 12), true);
        }
        [TestMethod]
        public void Test835_4()
        {
            string filePath = @"\\Sdsrv031.cymetrix.com\is\DATA\_SourceFiles\UAB\HQ_Hosp\SEIDR_835_SYNC_TEST\MCARE_10121802.RMT";
            JobProfile jp = JobProfile.GetSample(40, FileDateMask: "MCCCCCare_<MM><DD><YY>*"); //DateMask too many Cs
            TestPath(filePath, jp, new DateTime(2018, 10, 12), false);
        }
        [TestMethod]
        public void TestNameParse_Offset()
        {
            string filepath = "1401_6_16_AppServiceUser_20181120210144871.csv";
            string mask = "1401_6_16_AppServiceUser_<YYYY><MM><DD><0YYYY0MM+1DD>*";
            DateTime refdate = new DateTime(2018, 11, 1);
            DateTime expected = new DateTime(2018, 11, 21);
            Assert.IsTrue(SEIDR.Doc.DocExtensions.ParseDateRegex(filepath, mask, ref refdate));
            Assert.AreEqual(expected, refdate);
        }

        [TestMethod]
        public void TestNameParse_Case()
        {
            string filepath = "1401_6_16_AppServiceUser_20181120210144871.csv";
            string mask = "1401_6_16_aPPsERVICEuSER_<YYYY><MM><DD><0YYYY0MM+1DD>*";
            DateTime refdate = new DateTime(2018, 11, 1);
            DateTime expected = new DateTime(2018, 11, 21);
            Assert.IsTrue(SEIDR.Doc.DocExtensions.ParseDateRegex(filepath, mask, ref refdate));
            Assert.AreEqual(expected, refdate);

        }

        private void TestPath(string FilePath, JobProfile register, DateTime expectedProcessingDate, bool ValidMatch)
        {
            FileInfo f = new FileInfo(FilePath);
            if (f.Exists)
            {
                RegistrationFile rf = new RegistrationFile(register, f);
                if (ValidMatch)
                    Assert.AreEqual(expectedProcessingDate, rf.FileDate);
                else
                    Assert.AreEqual(f.CreationTime.Date, rf.FileDate);
            }
            else //Test file no longer exists, just check actual post parsing versus expected if ValidMatch. Also success of parsing should match ValidMatch.
            {
                DateTime actual = new DateTime(1, 1, 1);

                Assert.AreEqual(ValidMatch, SEIDR.Doc.DocExtensions.ParseDateRegex(Path.GetFileName(FilePath), register.FileDateMask, ref actual));
                if(ValidMatch)
                    Assert.AreEqual(expectedProcessingDate, actual);                
            }
        }


    }
}
