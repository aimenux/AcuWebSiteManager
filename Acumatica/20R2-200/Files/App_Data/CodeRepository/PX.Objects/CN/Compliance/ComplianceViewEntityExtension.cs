using PX.Data;
using PX.Objects.AP;
using PX.Objects.AR;
using PX.Objects.CN.Compliance.CL.DAC;
using PX.Objects.PM;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.CN.Compliance
{
    public class ComplianceViewEntityExtension<Graph, PrimaryDac> : PXGraphExtension<Graph> 
        where Graph : PXGraph
        where PrimaryDac : class, IBqlTable, new()
    {
        public PXAction<PrimaryDac> complianceViewCustomer;
        [PXUIField(DisplayName = "View Customer", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton(VisibleOnDataSource = false, VisibleOnProcessingResults = false, PopupVisible = false)]
        public virtual IEnumerable ComplianceViewCustomer(PXAdapter adapter)
        {
            Customer entity = PXSelect<Customer, Where<Customer.bAccountID, Equal<Current<ComplianceDocument.customerID>>>>.Select(Base);
            if (entity != null)
            {
                var target = PXGraph.CreateInstance<CustomerMaint>();
                target.BAccount.Current = entity;
                throw new PXRedirectRequiredException(target, true, "redirect") { Mode = PXBaseRedirectException.WindowMode.NewWindow };
            }

            return adapter.Get();
        }

        public PXAction<PrimaryDac> complianceViewProject;
        [PXUIField(DisplayName = "View Project", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton(VisibleOnDataSource = false, VisibleOnProcessingResults = false, PopupVisible = false)]
        public virtual IEnumerable ComplianceViewProject(PXAdapter adapter)
        {
            PMProject entity = PXSelect<PMProject, Where<PMProject.contractID, Equal<Current<ComplianceDocument.projectID>>>>.Select(Base);
            if (entity != null)
            {
                var target = PXGraph.CreateInstance<ProjectEntry>();
                target.Project.Current = entity;
                throw new PXRedirectRequiredException(target, true, "redirect") { Mode = PXBaseRedirectException.WindowMode.NewWindow };
            }

            return adapter.Get();
        }

        public PXAction<PrimaryDac> complianceViewCostTask;
        [PXUIField(DisplayName = "View Cost Task", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton(VisibleOnDataSource = false, VisibleOnProcessingResults = false, PopupVisible = false)]
        public virtual IEnumerable ComplianceViewCostTask(PXAdapter adapter)
        {
            PMTask entity = PXSelect<PMTask, Where<PMTask.taskID, Equal<Current<ComplianceDocument.costTaskID>>>>.Select(Base);
            if (entity != null)
            {
                var target = PXGraph.CreateInstance<ProjectTaskEntry>();
                target.Task.Current = entity;
                throw new PXRedirectRequiredException(target, true, "redirect") { Mode = PXBaseRedirectException.WindowMode.NewWindow };
            }

            return adapter.Get();
        }

        public PXAction<PrimaryDac> complianceViewRevenueTask;
        [PXUIField(DisplayName = "View Revenue Task", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton(VisibleOnDataSource = false, VisibleOnProcessingResults = false, PopupVisible = false)]
        public virtual IEnumerable ComplianceViewRevenueTask(PXAdapter adapter)
        {
            PMTask entity = PXSelect<PMTask, Where<PMTask.taskID, Equal<Current<ComplianceDocument.revenueTaskID>>>>.Select(Base);
            if (entity != null)
            {
                var target = PXGraph.CreateInstance<ProjectTaskEntry>();
                target.Task.Current = entity;
                throw new PXRedirectRequiredException(target, true, "redirect") { Mode = PXBaseRedirectException.WindowMode.NewWindow };
            }

            return adapter.Get();
        }

        public PXAction<PrimaryDac> complianceViewCostCode;
        [PXUIField(DisplayName = "View Cost Code", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton(VisibleOnDataSource = false, VisibleOnProcessingResults = false, PopupVisible = false)]
        public virtual IEnumerable ComplianceViewCostCode(PXAdapter adapter)
        {
            PMCostCode entity = PXSelect<PMCostCode, Where<PMCostCode.costCodeID, Equal<Current<ComplianceDocument.costCodeID>>>>.Select(Base);
            if (entity != null)
            {
                var target = PXGraph.CreateInstance<CostCodeMaint>();
                target.Items.Current = entity;
                throw new PXRedirectRequiredException(target, true, "redirect") { Mode = PXBaseRedirectException.WindowMode.NewWindow };
            }

            return adapter.Get();
        }

        public PXAction<PrimaryDac> complianceViewVendor;
        [PXUIField(DisplayName = "View Vendor", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton(VisibleOnDataSource = false, VisibleOnProcessingResults = false, PopupVisible = false)]
        public virtual IEnumerable ComplianceViewVendor(PXAdapter adapter)
        {
            VendorR entity = PXSelect<VendorR, Where<VendorR.bAccountID, Equal<Current<ComplianceDocument.vendorID>>>>.Select(Base);
            if (entity != null)
            {
                var target = PXGraph.CreateInstance<PX.Objects.AP.VendorMaint>();
                target.BAccount.Current = entity;
                throw new PXRedirectRequiredException(target, true, "redirect") { Mode = PXBaseRedirectException.WindowMode.NewWindow };
            }

            return adapter.Get();
        }

        public PXAction<PrimaryDac> complianceViewSecondaryVendor;
        [PXUIField(DisplayName = "View Vendor", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton(VisibleOnDataSource = false, VisibleOnProcessingResults = false, PopupVisible = false)]
        public virtual IEnumerable ComplianceViewSecondaryVendor(PXAdapter adapter)
        {
            VendorR entity = PXSelect<VendorR, Where<VendorR.bAccountID, Equal<Current<ComplianceDocument.secondaryVendorID>>>>.Select(Base);
            if (entity != null)
            {
                var target = PXGraph.CreateInstance<PX.Objects.AP.VendorMaint>();
                target.BAccount.Current = entity;
                throw new PXRedirectRequiredException(target, true, "redirect") { Mode = PXBaseRedirectException.WindowMode.NewWindow };
            }

            return adapter.Get();
        }

        public PXAction<PrimaryDac> complianceViewJointVendor;
        [PXUIField(DisplayName = "View Vendor", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton(VisibleOnDataSource = false, VisibleOnProcessingResults = false, PopupVisible = false)]
        public virtual IEnumerable ComplianceViewJointVendor(PXAdapter adapter)
        {
            VendorR entity = PXSelect<VendorR, Where<VendorR.bAccountID, Equal<Current<ComplianceDocument.jointVendorInternalId>>>>.Select(Base);
            if (entity != null)
            {
                var target = PXGraph.CreateInstance<PX.Objects.AP.VendorMaint>();
                target.BAccount.Current = entity;
                throw new PXRedirectRequiredException(target, true, "redirect") { Mode = PXBaseRedirectException.WindowMode.NewWindow };
            }

            return adapter.Get();
        }

        
    }
}
