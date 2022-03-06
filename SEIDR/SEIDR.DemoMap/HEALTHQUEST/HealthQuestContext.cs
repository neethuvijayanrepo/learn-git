using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SEIDR.DemoMap.BaseImplementation;
using SEIDR.JobBase;

namespace SEIDR.DemoMap.HEALTHQUEST
{
    public class HealthQuestContext : ContextObjectBase
    {
        /*
        
        Nothing special to this class, it's functionally the same as the BasicContext
            
        However, it could be modified to include properties that need to be tracked throughout the file for HealthQuest implementations.
        E.g., if we needed to track some sort of running total somewhere for some reason.

        It would be equivalent to the public variables in an SSIS script (as opposed to the local variables inside of a method).
        We could: 
            * initialize these variables during the setup method
            * use/modify them during transformations
            * also check them during our final validation call.

         */
        private object xrLock = new object();
        Doc.DocWriter _xrFile;
        public override void Init(MappingContext context, long RecordCount, DemoMapJobConfiguration settings, IJobExecutor executor)
        {
            base.Init(context, RecordCount, settings, executor);
            string baseFileName = System.IO.Path.GetFileNameWithoutExtension(context.FilePath);
            //Note: not really sure that we actually want an XR file...
            var xr = new Doc.DocMetaData(settings.OutputFolder, baseFileName + "_XR.CYM", "XR");
            xr.SetDelimiter('|');
            xr.AddDelimitedColumns("AccountNumber", "Ins1_EstimatedAmountDue");
            _xrFile = new Doc.DocWriter(xr);
        }

        public void AddXRRecord(Account a)
        {
            lock (xrLock)
            {
                _xrFile.AddRecord<Doc.DocRecord>(a);
            }
        }
        public override void DoCleanup()
        {
            lock (xrLock)
            {
            	_xrFile.Dispose();
            }
        }

    }
}
