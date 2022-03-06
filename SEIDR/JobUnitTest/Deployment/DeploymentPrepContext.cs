using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobUnitTest.Deployment
{
    public class DeploymentPrepContext: SEIDR.FileSystem.FileSystemContext
    {
        public readonly bool CopyToTraining;
        public readonly string TrainingRootFolder;

        public DeploymentPrepContext()
            :base()
        {
            if (!bool.TryParse(ConfigurationManager.AppSettings[nameof(CopyToTraining)], out CopyToTraining))
                CopyToTraining = false;
            TrainingRootFolder = ConfigurationManager.AppSettings[nameof(TrainingRootFolder)];
        }

    }
}
