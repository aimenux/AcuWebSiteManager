using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PX.Common;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.AR;
using PX.Objects.CN.Compliance.CL.DAC;
using PX.Objects.CN.Compliance.CL.Services;
using PX.Objects.CN.ProjectAccounting.PM.CacheExtensions;
using PX.Objects.CN.ProjectAccounting.PM.Descriptor;
using PX.Objects.CS;
using PX.Objects.PM;
using PX.SM;

namespace PX.Objects.CN.Compliance.PM.GraphExtensions
{
    public class ProjectTaskEntryExt : PXGraphExtension<ProjectTaskEntry>
    {
	    public static bool IsActive()
	    {
		    return PXAccess.FeatureInstalled<FeaturesSet.construction>();
	    }

		[PXCopyPasteHiddenView]
        [PXViewDetailsButton(typeof(Customer), typeof(Select<Customer,
            Where<Customer.bAccountID, Equal<Current<ComplianceDocument.customerID>>>>))]
        [PXViewDetailsButton(typeof(Vendor), typeof(Select<Vendor,
            Where<Vendor.bAccountID, Equal<Current<ComplianceDocument.vendorID>>>>))]
        [PXViewDetailsButton(typeof(Vendor), typeof(Select<Vendor,
            Where<Vendor.bAccountID, Equal<Current<ComplianceDocument.secondaryVendorID>>>>))]
        [PXViewDetailsButton(typeof(Vendor), typeof(Select<Vendor,
            Where<Vendor.bAccountID, Equal<Current<ComplianceDocument.jointVendorInternalId>>>>))]
        [PXViewDetailsButton(typeof(PMProject), typeof(Select<PMProject,
            Where<PMProject.contractID, Equal<Current<ComplianceDocument.projectID>>>>))]
        [PXViewDetailsButton(typeof(PMTask), typeof(Select<PMTask,
            Where<PMTask.taskID, Equal<Current<ComplianceDocument.costTaskID>>>>))]
        [PXViewDetailsButton(typeof(PMTask), typeof(Select<PMTask,
            Where<PMTask.taskID, Equal<Current<ComplianceDocument.revenueTaskID>>>>))]
        [PXViewDetailsButton(typeof(PMCostCode), typeof(Select<PMCostCode,
            Where<PMCostCode.costCodeID, Equal<Current<ComplianceDocument.costCodeID>>>>))]
        public PXSelect<ComplianceDocument,
            Where<ComplianceDocument.costTaskID, Equal<Current<PMTask.taskID>>,
                Or<ComplianceDocument.revenueTaskID, Equal<Current<PMTask.taskID>>>>> ComplianceDocuments;

        public PXSelect<CSAttributeGroup,
                Where<CSAttributeGroup.entityType, Equal<ComplianceDocument.typeName>,
                    And<CSAttributeGroup.entityClassID, Equal<ComplianceDocument.complianceClassId>>>>
            ComplianceAttributeGroups;

        public PXSelect<ComplianceAnswer> ComplianceAnswers;

        public PXSetup<LienWaiverSetup> LienWaiverSetup;

        private ComplianceDocumentService service;

        public override void Initialize()
        {
            base.Initialize();
            ValidateComplianceSetup();
            service = new ComplianceDocumentService(Base, ComplianceAttributeGroups, ComplianceDocuments,
                nameof(ComplianceDocuments));
            service.GenerateColumns(ComplianceDocuments.Cache, nameof(ComplianceAnswers));
            service.AddExpirationDateEventHandlers();
            ComplianceDocumentFieldVisibilitySetter.HideFieldsForProjectTask(ComplianceDocuments.Cache);
        }

        private void ValidateComplianceSetup()
        {
            if (LienWaiverSetup.Current == null)
                throw new PXSetupNotEnteredException<LienWaiverSetup>();
        }

        public IEnumerable complianceDocuments()
        {
            var documents = GetComplianceDocuments().ToList();
            service.ValidateComplianceDocuments(null, documents, ComplianceDocuments.Cache);
            return documents;
        }

        protected virtual void PmTask_RowSelected(PXCache cache, PXRowSelectedEventArgs args,
            PXRowSelected baseHandler)
        {
            if (!(args.Row is PMTask))
            {
                return;
            }
            baseHandler(cache, args);
            ComplianceDocuments.Select();
            ComplianceDocuments.AllowInsert = !Base.Task.Cache.Inserted.Any_();
        }

        protected virtual void _(Events.RowSelecting<PMTask> args)
        {
            var documents = GetComplianceDocuments();
            service.ValidateComplianceDocuments(args.Cache, documents, ComplianceDocuments.Cache);
        }

        protected virtual void _(Events.RowDeleted<PMTask> args)
        {
            var task = args.Row;
            if (task == null)
            {
                return;
            }
            var documents = GetComplianceDocuments();
            foreach (var document in documents)
            {
                if (document.CostTaskID == task.TaskID)
                {
                    document.CostTaskID = null;
                }
                if (document.RevenueTaskID == task.TaskID)
                {
                    document.RevenueTaskID = null;
                }
                ComplianceDocuments.Update(document);
            }
        }

        protected virtual void _(Events.RowSelected<ComplianceDocument> args)
        {
            service.UpdateExpirationIndicator(args.Row);
        }

        protected virtual void _(Events.RowInserting<ComplianceDocument> args)
        {
            var task = Base.Task.Current;
            if (task == null)
            {
                return;
            }
            var complianceDocument = args.Row;
            if (complianceDocument == null)
            {
                return;
            }
            FillProjectTaskInfo(complianceDocument, task);
        }

        private void FillProjectTaskInfo(ComplianceDocument complianceDocument, PMTask task)
        {
            var type = task.Type;
            if (type != null)
            {
                if (type == ProjectTaskType.Cost || type == ProjectTaskType.CostRevenue)
                {
                    complianceDocument.CostTaskID = task.TaskID;
                }
                if (type == ProjectTaskType.Revenue || type == ProjectTaskType.CostRevenue)
                {
                    complianceDocument.RevenueTaskID = task.TaskID;
                }
            }
            complianceDocument.ProjectID = task.ProjectID;
            complianceDocument.CustomerID = task.CustomerID;
            complianceDocument.CustomerName = GetCustomerName(complianceDocument.CustomerID);
        }

        private IEnumerable<ComplianceDocument> GetComplianceDocuments()
        {
            if (Base.Task.Current != null)
            {
                using (new PXConnectionScope())
                {
                    return new PXSelect<ComplianceDocument,
                        Where<ComplianceDocument.costTaskID,
                            Equal<Required<PMTask.taskID>>,
                            Or<ComplianceDocument.revenueTaskID, Equal<Required<PMTask.taskID>>>>>(Base)
                        .Select(Base.Task.Current.TaskID, Base.Task.Current.TaskID).FirstTableItems.ToList();
                }
            }
            return new PXResultset<ComplianceDocument>().FirstTableItems.ToList();
        }

        private string GetCustomerName(int? customerId)
        {
            if (!customerId.HasValue)
            {
                return null;
            }
            Customer customer = PXSelect<Customer,
                Where<Customer.bAccountID, Equal<Required<Customer.bAccountID>>>>.Select(Base, customerId);
            return customer?.AcctName;
        }
    }
}