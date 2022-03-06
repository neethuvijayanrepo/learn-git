using System;
using System.Collections.Generic;
using System.IO;
using JobUnitTest;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SEIDR.FileSystem.FileConcatenation;

namespace JobUnitTest.FileSystem.FileMerge
{
    [TestClass]
    public class FileMergeJobTest: ContextJobTestBase<FileMergeJob, SEIDR.FileSystem.FileSystemContext>
    {
        [TestMethod]
        public void TestMerge()
        {
            
            int leftCount = 2;
            int rightCount = 3;
            #region Array initialization
            bool[,] leftJoins = new bool[leftCount + 1, rightCount + 1];
            bool[] caseSensitive = new bool[leftCount + 1];
            const string DEFAULT_KEY = "AccountNumber";
            string[] keyLeft1 = new string[leftCount + 1];
            string[] keyRight1 = new string[rightCount + 1];
            string[,] keyLeft2 = new string[leftCount + 1, rightCount + 1];
            string[,] keyRight2 = new string[rightCount + 1, rightCount + 1];
            string[,] keyLeft3 = new string[leftCount + 1, rightCount + 1];
            string[,] keyRight3 = new string[rightCount + 1, rightCount + 1];
            bool[,] preSorted = new bool[leftCount + 1, rightCount + 1];

            bool[,] removeDuplicateColumns = new bool[leftCount+1, rightCount+1];
            bool[,] removeExtraMergeColumns = new bool[leftCount + 1, rightCount + 1];
            #endregion

            caseSensitive[1] = true;
            keyRight1[1] = "Account No";
            keyRight1[2] = "Account No";
            keyRight1[3] = "Account";
            //leftJoins[1, 3] = true;
            keyLeft2[2, 2] = "Facility"; //Multi key needs a bit more control, so two dimension
            leftJoins[2, 1] = true;

            leftJoins[1, 3] = true;
            keyLeft2[1, 3] = " Facility";
            keyRight2[1, 3] = "Facility";


            for (int i = 1; i <= leftCount; i++)
            {
                var left = SetExecutionTestFile($"MergeFileLeft{i}.txt", nameof(FileSystem), nameof(FileMerge));
                Assert.IsNotNull(left);
                Assert.IsNotNull(left.DirectoryName);
                for (int j = 1; j <= rightCount; j++)
                {
                    var right = GetTestFile($"MergeFileRight{j}.txt", nameof(FileSystem), nameof(FileMerge));
                    FileMergeJobSettings s = new FileMergeJobSettings
                    {
                        MergeFile = right.FullName,
                        InnerJoin = !leftJoins[i, j],
                        CaseSensitive = caseSensitive[i],

                        LeftKey1 = keyLeft1[i] ?? DEFAULT_KEY,
                        LeftKey2 = keyLeft2[i, j],
                        LeftKey3 = keyLeft3[i, j],
                        RightKey1 = keyRight1[j],
                        RightKey2 = keyRight2[i, j],
                        RightKey3 = keyRight3[i, j],

                        LeftInputHasHeader = true,
                        RightInputHasHeader = true,

                        OutputFilePath = Path.Combine(left.DirectoryName, $"MergeFileOutput_{i}_{j}_Actual.txt"),
                        Overwrite = true,
                        PreSorted = preSorted[i, j],
                        IncludeHeader = true,

                        RemoveDuplicateColumns = removeDuplicateColumns[i,j],
                        RemoveExtraMergeColumns = removeExtraMergeColumns[i,j]
                    };
                    try
                    {
                        _JOB.DoMerge(MyContext, s);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine(ex);
                        System.Diagnostics.Debug.WriteLine($"File Combo: {i}, {j}. Output Path: '{s.OutputFilePath}'.");
                        Assert.Fail();
                    }

                    Assert.IsTrue(MyContext.Success);
                    MyContext.WorkingFile.Finish();
                    FileInfo a = new FileInfo(s.OutputFilePath);
                    var expected = GetTestFile($"MergeFileOutput_{i}_{j}.txt", nameof(FileSystem), nameof(FileMerge));
                    AssertFileContent(expected, a);
                    _TestExecution.FilePath = left.FullName; //Reset file path, because otherwise we'll be looking at the output.
                }
            }


        }


        [TestMethod]
        public void MergeClientTest()
        {
            string dir = @"\\ncimtxfls01\sourcefiles_uat\Watsonville\TestFiles\";
            _TestExecution.FilePath = @"\\ncimtxfls01\sourcefiles_uat\Watsonville\TestFiles\NAV_ARMAST.csv";
            FileMergeJobSettings s = new FileMergeJobSettings
            {
                MergeFile = @"\\ncimtxfls01\sourcefiles_uat\Watsonville\TestFiles\NAV_DEMO.csv",
                InnerJoin = true,
                CaseSensitive = false,

                LeftKey1 = "PATIENT/ENCOUNTER NUMBER",

                LeftInputHasHeader = true,
                RightInputHasHeader = true,

                OutputFilePath = Path.Combine(dir, $"NAV_DEMO+ARMAST.csv"),
                Overwrite = true,
                PreSorted = false,
                IncludeHeader = true,

                RemoveDuplicateColumns = true,
                RemoveExtraMergeColumns = true,

                //TEXT_QUALIFIER = "\"",
                KeepDelimiter = false
            };
       
            _JOB.DoMerge(MyContext, s);
            if (MyContext.WorkingFile.Working)
                MyContext.WorkingFile.Finish();

            s.MergeFile = @"\\ncimtxfls01\sourcefiles_uat\Watsonville\TestFiles\NAV_PERSON.csv";
            s.LeftKey1 = "MRN";
            s.RightKey1 = s.LeftKey1;
            s.OutputFilePath = Path.Combine(dir, "NAV_DemographicsFull.csv");

            _JOB.DoMerge(MyContext, s);
            if (MyContext.WorkingFile.Working)
                MyContext.WorkingFile.Finish();
        }

    }
}