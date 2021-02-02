using PX.Data;
using PX.Objects.GL;
using PX.Objects.IN;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static PX.Objects.IN.INReleaseProcess;

namespace PX.Objects.PM.GraphExtensions
{
    public class INReleaseProcessExt : PXGraphExtension<INReleaseProcess>
    {
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<CS.FeaturesSet.projectAccounting>();
        }

        [PXOverride]
        public virtual GLTran InsertGLSalesDebit(JournalEntry je, GLTran tran, GLTranInsertionContext context, Func<JournalEntry, GLTran, GLTranInsertionContext, GLTran> baseMethod)
        {
            if (CopyProjectFromLine(tran, context))
            {
                tran.ProjectID = context.INTran.ProjectID;
                tran.TaskID = context.INTran.TaskID;
                tran.CostCodeID = context.INTran.CostCodeID;
            }

            return baseMethod(je, tran, context);
        }

        [PXOverride]
        public virtual GLTran InsertGLSalesCredit(JournalEntry je, GLTran tran, GLTranInsertionContext context, Func<JournalEntry, GLTran, GLTranInsertionContext, GLTran> baseMethod)
        {
            if (CopyProjectFromLine(tran, context))
            {
                tran.ProjectID = context.INTran.ProjectID;
                tran.TaskID = context.INTran.TaskID;
                tran.CostCodeID = context.INTran.CostCodeID;
            }

            return baseMethod(je, tran, context);
        }

        [PXOverride]
        public virtual GLTran InsertGLNonStockCostDebit(JournalEntry je, GLTran tran, GLTranInsertionContext context, Func<JournalEntry, GLTran, GLTranInsertionContext, GLTran> baseMethod)
        {
            if (context.INTran.AccrueCost != true && CopyProjectFromLine(tran, context))
            {
                tran.ProjectID = context.INTran.ProjectID;
                tran.TaskID = context.INTran.TaskID;
                tran.CostCodeID = context.INTran.CostCodeID;
            }

            return baseMethod(je, tran, context);
        }

        [PXOverride]
        public virtual GLTran InsertGLNonStockCostCredit(JournalEntry je, GLTran tran, GLTranInsertionContext context, Func<JournalEntry, GLTran, GLTranInsertionContext, GLTran> baseMethod)
        {
            if (CopyProjectFromLine(tran, context))
            {
                tran.ProjectID = context.INTran.ProjectID;
                tran.TaskID = context.INTran.TaskID;
                tran.CostCodeID = context.INTran.CostCodeID;
            }

            return baseMethod(je, tran, context);
        }

        [PXOverride]
        public virtual GLTran InsertGLCostsDebit(JournalEntry je, GLTran tran, GLTranInsertionContext context, Func<JournalEntry, GLTran, GLTranInsertionContext, GLTran> baseMethod)
        {
            int? locProjectID;
            int? locTaskID = null;
            if (context.Location != null && context.Location.ProjectID != null)//can be null if Adjustment
            {
                locProjectID = context.Location.ProjectID;
                locTaskID = context.Location.TaskID;

                if (locTaskID == null)//Location with ProjectTask WildCard
                {
                    if (context.Location.ProjectID == context.INTran.ProjectID)
                    {
                        locTaskID = context.INTran.TaskID;
                    }
                    else
                    {
                        //substitute with any task from the project.
                        PMTask task = PXSelect<PMTask, Where<PMTask.projectID, Equal<Required<PMTask.projectID>>,
                            And<PMTask.visibleInIN, Equal<True>, And<PMTask.isActive, Equal<True>>>>>.Select(Base, context.Location.ProjectID);
                        if (task != null)
                        {
                            locTaskID = task.TaskID;
                        }
                    }
                }

            }
            else
            {
                locProjectID = PM.ProjectDefaultAttribute.NonProject();
            }

            if (context.TranCost.TranType == INTranType.Adjustment || context.TranCost.TranType == INTranType.Transfer)
            {
                tran.ProjectID = locProjectID;
                tran.TaskID = locTaskID;
            }
            else
            {
                tran.ProjectID = context.INTran.ProjectID ?? locProjectID;
                tran.TaskID = context.INTran.TaskID ?? locTaskID;
            }
            tran.CostCodeID = context.INTran.CostCodeID;
            return baseMethod(je, tran, context);
        }

        [PXOverride]
        public virtual GLTran InsertGLCostsCredit(JournalEntry je, GLTran tran, GLTranInsertionContext context, Func<JournalEntry, GLTran, GLTranInsertionContext, GLTran> baseMethod)
        {
            if (CopyProjectFromLine(tran, context))
            {
                tran.ProjectID = context.INTran.ProjectID;
                tran.TaskID = context.INTran.TaskID;
                tran.CostCodeID = context.INTran.CostCodeID;
            }
            return baseMethod(je, tran, context);
        }

        [PXOverride]
        public virtual GLTran InsertGLCostsOversold(JournalEntry je, GLTran tran, GLTranInsertionContext context, Func<JournalEntry, GLTran, GLTranInsertionContext, GLTran> baseMethod)
        {
            if (context.TranCost.TranType == INTranType.Transfer)
            {
                //GIT always to Non-Project.
                tran.ProjectID = context.TranCost.COGSAcctID == null ? PM.ProjectDefaultAttribute.NonProject() : context.INTran.ProjectID;
                tran.TaskID = context.TranCost.COGSAcctID == null ? null : context.INTran.TaskID;
                tran.CostCodeID = context.TranCost.COGSAcctID == null ? null : context.INTran.CostCodeID;
            }
            else
            {
                if (CopyProjectFromLine(tran, context))
                {
                    tran.ProjectID = context.INTran.ProjectID;
                    tran.TaskID = context.INTran.TaskID;
                    tran.CostCodeID = context.INTran.CostCodeID;
                }
            }
            return baseMethod(je, tran, context);
        }

        [PXOverride]
        public virtual GLTran InsertGLCostsVarianceCredit(JournalEntry je, GLTran tran, GLTranInsertionContext context, Func<JournalEntry, GLTran, GLTranInsertionContext, GLTran> baseMethod)
        {
            if (CopyProjectFromLine(tran, context))
            {
                tran.ProjectID = context.INTran.ProjectID;
                tran.TaskID = context.INTran.TaskID;
                tran.CostCodeID = context.INTran.CostCodeID;
            }
            return baseMethod(je, tran, context);
        }

        [PXOverride]
        public virtual GLTran InsertGLCostsVarianceDebit(JournalEntry je, GLTran tran, GLTranInsertionContext context, Func<JournalEntry, GLTran, GLTranInsertionContext, GLTran> baseMethod)
        {
            if (CopyProjectFromLine(tran, context))
            {
                tran.ProjectID = context.INTran.ProjectID;
                tran.TaskID = context.INTran.TaskID;
                tran.CostCodeID = context.INTran.CostCodeID;
            }
            return baseMethod(je, tran, context);
        }

        protected virtual bool CopyProjectFromLine(GLTran tran, GLTranInsertionContext context)
        {
            Account account = Account.PK.Find(Base, tran.AccountID);
            if (account != null)
            {
                return account.AccountGroupID != null;
            }

            return true;
        }
    }
}
