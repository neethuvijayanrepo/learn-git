#define SINGLE
#undef SINGLE

using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SEIDR.JobBase;
using SEIDR.MetrixProcessing.Invoice.Physician;

namespace SEIDR.MetrixProcessing.Invoice
{
    [IJobMetaData(nameof(PhysicianInvoiceJob), nameof(Invoice), "Physician Invoice Preview Generation",
                  false, true, null)]
    [SuppressMessage("ReSharper", "AccessToDisposedClosure")]
    public class PhysicianInvoiceJob : ContextJobBase<InvoicingContext>
    {
        public DataTable CheckProjectsPendingInvoicePreview(DataBase.DatabaseManager metrix)
        {
            var ds = metrix.Execute("APP.[usp_Project_sl_InvoicePreviewGeneration]");
            if (ds == null || ds.Tables.Count == 0)
                return null;
            return ds.Tables[0];
        }

        public override void Process(InvoicingContext context)
        {
            if (context.Execution.ProjectID == null)
            {
                var dt = CheckProjectsPendingInvoicePreview(context.Metrix);
                if (dt == null)
                {
                    context.LogInfo("No Invoiceable projects identified.");
                }
                else
                {
                    ChildExecutionInfo child = new ChildExecutionInfo(context, true);
                    foreach (DataRow row in dt.Rows)
                    {
                        child.ProjectID = Convert.ToInt32(row[nameof(ChildExecutionInfo.ProjectID)]);
                        child.OrganizationID = Convert.ToInt32(row[nameof(ChildExecutionInfo.OrganizationID)]);
                        child.ProcessingDate = (DateTime) row["InvoiceDate"];
                        child.ContinueToNextStep = false; //Reset to step # 1.
                        RegisterChildExecution(child, context);
                    }
                }
                // Alternatively in future - set child execution to a different branch.
                // Job doesn't need to decide whether or not the current execution is allowed to continue then.. (can still allow that as a safety of course, though, depending on what is being done)
                context.SetStatus(InvoiceResultCode.PC);
                return;
            }

            context.LoadOverrides();
            using (InvoicePreviewGenerator pvg = new InvoicePreviewGenerator(context))
            {
                pvg.Init();
                //Note: waiting for task to finish before the pvg using block is ended.
                using (Task tp = Task.Run(() => pvg.Process()))
                using (Task te1 = Task.Run(() => ProcessGeneratorQueue(context, pvg, tp)))
                {
#if SINGLE
                    {
                        var toWait = new[]
                        {
                            tp, te1
                        };
#else
                    /*

                    Performance: 500k transaction/line items:
                    4 tasks: ~6 mins
                    5 tasks: ~5 mins
                    3 tasks: ~4 mins
                    2 tasks: ~4 mins. Tasks may not necessarily mean more threads, though, depending on resources.
                     */

                    using (Task te2 = Task.Run(() => ProcessGeneratorQueue(context, pvg, tp)))
                    {
                        var toWait = new[]
                        {
                            tp, te1, te2, 
                        };
#endif

                        Task.WaitAll(toWait);
                        pvg.CheckBulkCopy();
                    }
                }
            }
        }

        void ProcessGeneratorQueue(InvoicingContext context,InvoicePreviewGenerator generator,  Task parentTask)
        {
            object statusLock = context.GetSyncObject();
            EncounterContainer ec;
            bool start;
            while ((start = generator.Encounters.TryTake(out ec)) || parentTask.Status == TaskStatus.Running)
            {
                if (!start)
                {
                    context.CheckResetEvent(true);
                    continue;
                }
                bool success = ec.DoWork();
                if (!success)
                {
                    lock (statusLock)
                    {
                        context.LogError("Issue during bulk insert check for EncounterID " + ec.EncounterID);
                    }
                    break;
                }
            }

            lock (statusLock)
            {
                context.LogInfo("Finished an Encounter loop task..");
            }
        }
    }
}
