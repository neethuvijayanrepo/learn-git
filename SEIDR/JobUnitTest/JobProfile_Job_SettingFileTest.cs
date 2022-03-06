using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JobUnitTest
{
    [TestClass]
    public class JobProfile_Job_SettingFileTest:TestBase
    {
        public const string SETTING_CONTENT = @"blaeafjaefa
            afafjejoa;fj
esfjaoeif hi
random string of letter
LORUS IPSUM

Lorem ipsum dolor sit amet, consectetur adipiscing elit. Vestibulum rhoncus, mi a pretium maximus, neque libero ultrices elit, et cursus diam risus non neque. Integer id massa iaculis, aliquet nunc quis, iaculis ante. Suspendisse eget magna quis ante lacinia pellentesque. Fusce quis ex sed dolor tincidunt tincidunt id non ex. Morbi finibus odio a ligula scelerisque, dictum ultrices elit commodo. Vestibulum eget mattis metus. Etiam turpis lorem, tristique blandit augue sit amet, dignissim lacinia justo. Quisque tempus diam erat, ultrices congue nisi laoreet a. Fusce at urna diam. Sed aliquet tristique leo quis lacinia. Fusce commodo quis lorem at tincidunt. Integer sollicitudin sollicitudin luctus. Nullam mattis aliquam ligula, non malesuada purus venenatis fringilla. Etiam ultrices ullamcorper tellus, at maximus sem euismod non.

Maecenas turpis urna, placerat vitae sem quis, faucibus consectetur leo. Suspendisse pretium libero non tristique congue. Cras imperdiet ac mauris et posuere. Curabitur urna ex, posuere cursus facilisis sit amet, porta et eros. Vestibulum aliquet lacus sed molestie pulvinar. Nulla aliquam diam nec magna venenatis tempus. Nulla at tellus eget nisi suscipit pharetra. Etiam leo dui, porttitor sed purus nec, malesuada malesuada felis. Cras ac eros nec turpis bibendum luctus. Proin bibendum augue vel elementum tempus. Nunc at lacus ut mauris elementum malesuada vel non leo. Donec vehicula turpis a mauris tempus, et vestibulum purus posuere. Vestibulum diam arcu, posuere a vehicula sed, pretium eu ante. Nunc vel nunc bibendum, ultricies turpis eget, ullamcorper purus.

Vestibulum sollicitudin odio nec tellus consequat, in pharetra nisi volutpat. Morbi condimentum nibh in lacus hendrerit, ac rhoncus diam sagittis. Suspendisse aliquet lorem eget metus scelerisque molestie. Proin efficitur auctor mi, ut vulputate tellus tincidunt et. Duis porttitor velit suscipit, porta justo et, facilisis lacus. Proin interdum, nisl id sagittis egestas, enim tortor placerat justo, quis ultrices sapien arcu viverra metus. Vivamus fermentum vitae diam in rutrum. Nam in dolor a metus vestibulum dapibus nec non nisl. Fusce hendrerit lorem et lorem egestas, at venenatis ex auctor. Sed ex purus, bibendum sit amet aliquet at, volutpat ut orci. Fusce sit amet pharetra tortor. Donec congue, neque quis consectetur tempus, arcu turpis semper ante, eget tincidunt sem tortor congue lorem. Mauris volutpat viverra justo, in iaculis mi auctor eu. Nulla facilisi. Donec laoreet tortor in malesuada eleifend. Aenean dui augue, consequat vitae pretium nec, volutpat quis justo.

Ut ex ligula, blandit at sem in, aliquet fringilla massa. Quisque imperdiet odio vitae molestie dapibus. In ac ultrices leo, a venenatis purus. Ut mauris nunc, iaculis vitae volutpat ut, venenatis sit amet dolor. Ut maximus leo et posuere euismod. In hendrerit finibus metus. Etiam rutrum ipsum vitae mi tempus viverra. Quisque elementum congue arcu, sit amet ultricies libero lacinia ac. Donec sit amet dictum leo, id hendrerit arcu. Integer sodales dolor sit amet felis varius, nec faucibus enim convallis.

Quisque turpis ligula, mollis in egestas eu, scelerisque imperdiet est. Interdum et malesuada fames ac ante ipsum primis in faucibus. Sed in eleifend nulla. Maecenas urna lectus, efficitur vel ligula vel, condimentum iaculis justo. Curabitur et nulla lorem. Nulla efficitur vitae ligula eget aliquam. Proin at commodo purus, id auctor enim.


";
        const string TEST_FILE_NAME = "Test.Txt";

        [TestMethod]
        public void TestGettingConfiguration()
        {
            CheckUserKey("SETTINGS", "User Settings Test");
            try
            {
                CheckJobMetaData<SEIDR.FileSystem.FileConversion.FixWidthConversionJob>();
                SEIDR.JobBase.JobProfile_Job_SettingsFile sf;
                using (var h = _Manager.GetBasicHelper(true))
                {
                    h.BeginTran();
                    var profile = CreateProfile("Setting file test", "SETTINGS", h);
                    var stepID = SetStep<SEIDR.FileSystem.FileConversion.FixWidthConversionJob>(profile, h, 1, "TEST");
                    string insertCommand = @"IF NOT EXISTS(SELECT null FROM SEIDR.JobProfile_Job_SettingsFile WHERE JobProfile_JobID = " + stepID + @")
BEGIN
    INSERT INTO SEIDR.JobProfile_Job_SettingsFile(JobProfile_JobID, SettingsFilePath)
    VALUES(" + stepID + ", '" + _TestExecution.FilePath + @"')
END
ELSE
BEGIN
    UPDATE SEIDR.JobProfile_Job_SettingsFile
    SET SettingsFilePath = '" + _TestExecution.FilePath + @"'
    WHERE JobProfile_JobID = " + stepID + @"
END";
                    _Manager.ExecuteTextNonQuery(insertCommand);
                    h.Procedure = "usp_JobProfile_Job_SettingsFile_ss"; //With a helper model, need to specify the procedure for select single
                    sf = _Manager.SelectSingle<SEIDR.JobBase.JobProfile_Job_SettingsFile>(h);

                    if(h.HasOpenTran)
                        h.RollbackTran();
                }
                Assert.AreEqual(_TestExecution.FilePath, sf.SettingsFilePath);
                string content = System.IO.File.ReadAllText(sf.SettingsFilePath);
                Assert.AreEqual(SETTING_CONTENT, content);
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                Assert.Fail();
            }   
        }

        protected override void Init()
        {
            base.Init();
            PrepRootDirectory(false);
            PrepSubfolder("SettingsFile", true);
            _TestExecution.SetFileInfo( CreateFile(TEST_FILE_NAME, SETTING_CONTENT));
        }
    }
}
