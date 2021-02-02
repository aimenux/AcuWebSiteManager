using System;
using System.Collections.Generic;
using PX.Common;
using PX.Data;
using PX.Objects.CS;
using System.Collections;
using PX.Objects.GL;
using PX.Objects.IN;
using PX.Objects.CT;
using PX.Objects.CR;

namespace PX.Objects.PM
{
	public class TemplateGlobalTaskMaint : PXGraph<TemplateGlobalTaskMaint>
	{
		#region DAC Attributes Override

        #region PMTask
        [PXDBInt(IsKey = true)]
        [PXParent(typeof(Select<PMProject, Where<PMProject.contractID, Equal<Current<PMTask.projectID>>>>))]
        [PXDefault(typeof(Search<PMProject.contractID, Where<PMProject.nonProject, Equal<True>>>))]
        protected virtual void PMTask_ProjectID_CacheAttached(PXCache sender)
        {
        }

        [PXDimensionSelector(ProjectTaskAttribute.DimensionName,
            typeof(Search<PMTask.taskCD, Where<PMTask.projectID, Equal<Current<PMTask.projectID>>>>),
            typeof(PMTask.taskCD),
            typeof(PMTask.taskCD), typeof(PMTask.locationID), typeof(PMTask.description), typeof(PMTask.status), DescriptionField = typeof(PMTask.description))]
		[PXDBString(30, IsUnicode = true, IsKey = true, InputMask = "")]
        [PXDefault]
        [PXUIField(DisplayName = "Task ID", Visibility = PXUIVisibility.SelectorVisible)]
        protected virtual void PMTask_TaskCD_CacheAttached(PXCache sender)
        {
        }

        [PXDBString(1, IsFixed = true)]
        [PXDefault(ProjectTaskStatus.Active)]
        [PXUIField(DisplayName = "Status", Visibility = PXUIVisibility.Invisible, Visible = false)]
        protected virtual void PMTask_Status_CacheAttached(PXCache sender)
        {
        }

		[PXDBBool()]
		[PXDefault(typeof(Search<PMSetup.visibleInGL>))]
		[PXUIField(DisplayName = "GL")]
		protected virtual void PMTask_VisibleInGL_CacheAttached(PXCache sender){}

		[PXDBBool()]
		[PXDefault(typeof(Search<PMSetup.visibleInAP>))]
		[PXUIField(DisplayName = "AP")]
		protected virtual void PMTask_VisibleInAP_CacheAttached(PXCache sender) { }

		[PXDBBool()]
		[PXDefault(typeof(Search<PMSetup.visibleInAR>))]
		[PXUIField(DisplayName = "AR")]
		protected virtual void PMTask_VisibleInAR_CacheAttached(PXCache sender) { }

		[PXDBBool()]
		[PXDefault(typeof(Search<PMSetup.visibleInCA>))]
		[PXUIField(DisplayName = "CA")]
		protected virtual void PMTask_VisibleInCA_CacheAttached(PXCache sender) { }

		[PXDBBool()]
		[PXDefault(typeof(Search<PMSetup.visibleInCR>))]
		[PXUIField(DisplayName = "CRM")]
		protected virtual void PMTask_VisibleInCR_CacheAttached(PXCache sender) { }

		[PXDBBool()]
		[PXDefault(typeof(Search<PMSetup.visibleInTA>))]
		[PXUIField(DisplayName = "Time Entries")]
		protected virtual void PMTask_VisibleInTA_CacheAttached(PXCache sender) { }

		[PXDBBool()]
		[PXDefault(typeof(Search<PMSetup.visibleInEA>))]
		[PXUIField(DisplayName = "Expenses")]
		protected virtual void PMTask_VisibleInEA_CacheAttached(PXCache sender) { }

		[PXDBBool()]
		[PXDefault(typeof(Search<PMSetup.visibleInIN>))]
		[PXUIField(DisplayName = "IN")]
		protected virtual void PMTask_VisibleInIN_CacheAttached(PXCache sender) { }

		[PXDBBool()]
		[PXDefault(typeof(Search<PMSetup.visibleInSO>))]
		[PXUIField(DisplayName = "SO")]
		protected virtual void PMTask_VisibleInSO_CacheAttached(PXCache sender) { }

		[PXDBBool()]
		[PXDefault(typeof(Search<PMSetup.visibleInPO>))]
		[PXUIField(DisplayName = "PO")]
		protected virtual void PMTask_VisibleInPO_CacheAttached(PXCache sender) { }
		
		[PXDBString(PMRateTable.rateTableID.Length, IsUnicode = true)]
		[PXUIField(DisplayName = "Rate Table")]
		[PXSelector(typeof(PMRateTable.rateTableID), DescriptionField = typeof(PMRateTable.description))]
		protected virtual void PMTask_RateTableID_CacheAttached(PXCache sender) { }

		#endregion

		#region PMBudget
		
        [PXDefault]
        [AccountGroupAttribute(IsKey=true)]
        protected virtual void PMBudget_AccountGroupID_CacheAttached(PXCache sender)
        {
        }

        [PXDefault(typeof(PMTask.projectID))]
        [PXDBInt(IsKey = true)]
        protected virtual void PMBudget_ProjectID_CacheAttached(PXCache sender)
        {
        }

        [PXDBDefault(typeof(PMTask.taskID))]
        [PXDBInt(IsKey = true)]
        [PXParent(typeof(Select<PMTask, Where<PMTask.projectID, Equal<Current<PM.PMBudget.projectID>>, And<PMTask.taskID, Equal<Current<PM.PMBudget.projectTaskID>>>>>))]
        protected virtual void PMBudget_ProjectTaskID_CacheAttached(PXCache sender)
        {
        }
		#endregion

		#region PMRecurringItem
		[PXDefault(ResetUsageOption.Never)]
		[PXUIField(DisplayName = "Reset Usage", Visible = false)]
		[PXDBString(1, IsFixed = true)]
		[ResetUsageOption.ListForProject]
		protected virtual void ResetUsageCacheAttached(Events.CacheAttached<PMRecurringItem.resetUsage> e) { }
		#endregion

		#endregion

		#region Views/Selects

		public PXSelectJoin<PMTask, LeftJoin<PMProject, On<PMTask.projectID, Equal<PMProject.contractID>>>, Where<PMProject.nonProject, Equal<True>>> Task;
		public PXSelect<PMTask, Where<PMTask.taskID, Equal<Current<PMTask.taskID>>>> TaskProperties;
        public PXSelect<PMRecurringItem,
            Where<PMRecurringItem.projectID, Equal<Current<PMTask.projectID>>,
            And<PMRecurringItem.taskID, Equal<Current<PMTask.taskID>>>>> BillingItems;
        
		[PXImport(typeof(PMTask))]		
        public PXSelectJoin<PMBudget, LeftJoin<PMAccountGroup, On<PMBudget.accountGroupID, Equal<PMAccountGroup.groupID>>>, 
            Where<PMBudget.projectID, Equal<Current<PMTask.projectID>>, 
            And<PMBudget.projectTaskID, Equal<Current<PMTask.taskID>>>>> Budget;

		[PXViewName(Messages.TaskAnswers)]
		public CRAttributeList<PMTask> Answers;
		public PXSetup<PMSetup> Setup;
		public PXSetup<Company> CompanySetup;
        #endregion

        #region	Actions/Buttons

        public PXSave<PMTask> Save;
		public PXCancel<PMTask> Cancel;
		public PXInsert<PMTask> Insert;
		public PXDelete<PMTask> Delete;
		public PXFirst<PMTask> First;
		public PXPrevious<PMTask> previous;
		public PXNext<PMTask> next;
		public PXLast<PMTask> Last;
						
        #endregion		 

		public TemplateGlobalTaskMaint()
		{
			if (Setup.Current == null)
			{
				throw new PXException(Messages.SetupNotConfigured);
			}
		}

		#region Event Handlers

		protected virtual void _(Events.FieldDefaulting<PMBudget, PMBudget.costCodeID> e)
		{
			if (!CostCodeAttribute.UseCostCode())
			{
				e.NewValue = CostCodeAttribute.GetDefaultCostCode();
			}
		}

		protected virtual void _(Events.FieldDefaulting<PMBudget, PMBudget.inventoryID> e)
		{
			if (CostCodeAttribute.UseCostCode())
			{
				e.NewValue = PMInventorySelectorAttribute.EmptyInventoryID;
			}
		}

		protected virtual void _(Events.FieldUpdated<PMBudget, PMBudget.accountGroupID> e)
		{
			PMAccountGroup ag = PXSelect<PMAccountGroup, Where<PMAccountGroup.groupID, Equal<Required<PMAccountGroup.groupID>>>>.Select(this, e.Row.AccountGroupID);
			if (ag != null)
			{
				if (ag.IsExpense == true)
				{
					e.Row.Type = GL.AccountType.Expense;
				}
				else
				{
					e.Row.Type = ag.Type;
				}
			}
		}

		protected virtual void _(Events.RowSelected<PMTask> e)
		{
			if (e.Row != null)
			{
				PMProject prj = PXSelect<PMProject, Where<PMProject.contractID, Equal<Current<PMTask.projectID>>>>.SelectSingleBound(this, new object[] { e.Row });
                PXUIFieldAttribute.SetEnabled<PMTask.autoIncludeInPrj>(e.Cache, e.Row, prj != null && prj.NonProject != true);
			}
		}

		protected virtual void _(Events.RowSelected<PMBudget> e)
		{
			if (e.Row != null)
			{
				PXUIFieldAttribute.SetEnabled<PMBudget.isProduction>(e.Cache, e.Row, e.Row.Type == GL.AccountType.Expense);
			}
		}

        protected virtual void PMTask_CustomerID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
        {
            PMTask row = e.Row as PMTask;
            if(row == null) return;

            PMProject prj = PXSelect<PMProject, Where<PMProject.contractID, Equal<Current<PMTask.projectID>>>>.SelectSingleBound(this, new object[] { row });
            if(prj != null && prj.NonProject == true)
            {
                e.Cancel = true;
            }
        }

        
		protected virtual void PMTask_ProjectID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			PMTask row = e.Row as PMTask;
			if (row != null)
			{
				sender.SetDefaultExt<PMTask.customerID>(e.Row);
			}
		}

		protected virtual void _(Events.FieldUpdated<PMRecurringItem, PMRecurringItem.inventoryID> e)
		{
			e.Cache.SetDefaultExt<PMRecurringItem.description>(e.Row);
			e.Cache.SetDefaultExt<PMRecurringItem.uOM>(e.Row);
			e.Cache.SetDefaultExt<PMRecurringItem.amount>(e.Row);
		}


		protected virtual void _(Events.FieldDefaulting<PMRecurringItem, PMRecurringItem.amount> e)
		{
			if (e.Row == null) return;
			InventoryItem item = PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>.Select(this, e.Row.InventoryID);
			if (item != null)
			{
				e.NewValue = item.BasePrice;
			}
		}

		protected virtual void _(Events.RowSelected<PMRecurringItem> e)
		{			
            if (e.Row != null && Task.Current != null)
            {
                PXUIFieldAttribute.SetEnabled<PMRecurringItem.included>(e.Cache, e.Row, Task.Current.IsActive != true);
			}
        }

		#endregion
	}
}