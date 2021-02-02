using System;
using System.Collections;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.CT;
using PX.Objects.IN;
using PX.Objects.PM;
using PX.Data;
using System.Linq;
using System.Collections.Generic;


namespace PX.Objects.EP
{
	public class EmployeeActivitiesRelease : PXGraph<EmployeeActivitiesRelease>
	{
		public EmployeeActivitiesRelease()
		{
			if (PXSelect<EPSetup>.SelectSingleBound(this, null, null) == null)
				throw new PXSetupNotEnteredException(ErrorMessages.SetupNotEntered, typeof(EPSetup), PXMessages.LocalizeNoPrefix(Messages.EPSetup));

			//NO CRM Mode
			PXUIFieldAttribute.SetVisible<EPActivityApprove.contractID>(Activity.Cache, null, !PXAccess.FeatureInstalled<FeaturesSet.customerModule>());
			PXUIFieldAttribute.SetVisible<ContractEx.contractCD>(dummyContract.Cache, null, PXAccess.FeatureInstalled<FeaturesSet.customerModule>());

			if (!PXAccess.FeatureInstalled<FeaturesSet.timeReportingModule>())
			{
				PXUIFieldAttribute.SetVisible(Activity.Cache, typeof(EPActivityApprove.timeSpent).Name, false);
				PXUIFieldAttribute.SetVisible(Activity.Cache, typeof(EPActivityApprove.timeBillable).Name, false);
				PXUIFieldAttribute.SetVisible(Activity.Cache, typeof(EPActivityApprove.isBillable).Name, false);

				PXUIFieldAttribute.SetVisible<EPActivityFilter.regularOvertime>(Filter.Cache, null, false);
				PXUIFieldAttribute.SetVisible<EPActivityFilter.regularTime>(Filter.Cache, null, false);
				PXUIFieldAttribute.SetVisible<EPActivityFilter.regularTotal>(Filter.Cache, null, false);
				PXUIFieldAttribute.SetVisible<EPActivityFilter.billableOvertime>(Filter.Cache, null, false);
				PXUIFieldAttribute.SetVisible<EPActivityFilter.billableTime>(Filter.Cache, null, false);
				PXUIFieldAttribute.SetVisible<EPActivityFilter.billableTotal>(Filter.Cache, null, false);
			}

			if (!PXAccess.FeatureInstalled<FeaturesSet.projectModule>())
			{
				PXUIFieldAttribute.SetVisible(Activity.Cache, typeof(EPActivityApprove.projectTaskID).Name, false);
				PXUIFieldAttribute.SetVisible<EPActivityFilter.projectTaskID>(Filter.Cache, null, false);
			}

			if (!PXAccess.FeatureInstalled<FeaturesSet.projectModule>())
			{
				PXUIFieldAttribute.SetVisible(Activity.Cache, typeof(EPActivityApprove.projectTaskID).Name, false);
				PXUIFieldAttribute.SetVisible<EPActivityFilter.projectTaskID>(Filter.Cache, null, false);
				PXUIFieldAttribute.SetVisible<EPActivityApprove.approvedDate>(Activity.Cache, null, false);
				PXUIFieldAttribute.SetVisible<EPActivityApprove.approverID>(Activity.Cache, null, false);
			}

			bool contractInstalled = PXAccess.FeatureInstalled<CS.FeaturesSet.contractManagement>();
			if (!contractInstalled)
			{
				PXUIFieldAttribute.SetVisible<CT.Contract.contractCD>(dummyContract.Cache, null, false);
				PXUIFieldAttribute.SetVisible<EPActivityFilter.contractID>(Filter.Cache, null, false);
			}
			if (!PXAccess.FeatureInstalled<FeaturesSet.projectModule>())
			{
				PXUIFieldAttribute.SetVisible(Activity.Cache, typeof(EPActivityApprove.projectTaskID).Name, false);
				PXUIFieldAttribute.SetVisible<EPActivityFilter.projectTaskID>(Activity.Cache, null, false);
			}

			Activity.SetProcessDelegate(ReleaseActivities);
			Activity.SetSelected<EPActivityApprove.selected>();
		}

		[PXCustomizeBaseAttributeAttribute(typeof(PXUIFieldAttribute), nameof(PXUIFieldAttribute.DisplayName), "Case Status")]
		protected virtual void _(Events.CacheAttached<CRCase.status> e) {}

		public PXCancel<EPActivityFilter> Cancel;
		public PXAction<EPActivityFilter> viewDetails;

		#region Selects
		[PXHidden]
		public PXSelect<CRCase> dummyCase;
		[PXHidden]
		public PXSelect<ContractEx> dummyContract;
		public PXFilter<EPActivityFilter> Filter;
		
		public PXFilteredProcessingJoin<
			EPActivityApprove,
			EPActivityFilter,
			LeftJoin<EPEarningType,
				On<EPEarningType.typeCD, Equal<EPActivityApprove.earningTypeID>>,
			InnerJoin<EPEmployee,
				On<EPEmployee.userID, Equal<EPActivityApprove.ownerID>,
				And<Where<EPEmployee.timeCardRequired, NotEqual<True>, Or<EPEmployee.timeCardRequired, IsNull>>>>,
			LeftJoin<CRActivityLink,
				On<CRActivityLink.noteID, Equal<EPActivityApprove.refNoteID>>,
			LeftJoin<CRCase,
				On<CRCase.noteID, Equal<CRActivityLink.refNoteID>>,
			LeftJoin<CRCaseClass,
				On<CRCaseClass.caseClassID, Equal<CRCase.caseClassID>>,
			LeftJoin<ContractEx,
				On<CRCase.contractID, Equal<ContractEx.contractID>>>>>>>>,
			Where2<
				Where2<
					Where<EPActivityApprove.approvalStatus, Equal<ActivityStatusListAttribute.completed>,
						And<EPActivityApprove.approverID, IsNull>>,
					Or<Where<EPActivityApprove.approvalStatus, Equal<ActivityStatusListAttribute.approved>,
						And<EPActivityApprove.approverID, IsNotNull>>>>,
				And<EPActivityApprove.date, Less<Current<EPActivityFilter.tillDatePlusOne>>,
				And2<Where<EPActivityApprove.date, GreaterEqual<Current<EPActivityFilter.fromDate>>,
					Or<Current<EPActivityFilter.fromDate>, IsNull>>,
				And2<Where<EPActivityApprove.projectID, Equal<Current<EPActivityFilter.projectID>>,
					Or<Current<EPActivityFilter.projectID>, IsNull>>,
				And2<Where<EPActivityApprove.projectTaskID, Equal<Current<EPActivityFilter.projectTaskID>>,
					Or<Current<EPActivityFilter.projectTaskID>, IsNull>>,
				And2<Where<ContractEx.contractID, Equal<Current<EPActivityFilter.contractID>>,
					Or<Current<EPActivityFilter.contractID>, IsNull,
					Or<EPActivityApprove.contractID, Equal<Current<EPActivityFilter.contractID>>>>>,
				And<EPActivityApprove.projectID, IsNotNull,
				And<EPActivityApprove.released, NotEqual<True>,
				And<EPActivityApprove.trackTime, Equal<True>,
				And<EPActivityApprove.origNoteID, IsNull,
				And2<Where<EPActivityApprove.ownerID, Equal<Current<EPActivityFilter.employeeID>>,
					Or<Current<EPActivityFilter.employeeID>, IsNull>>,
				And<Where<CRCaseClass.perItemBilling, Equal<BillingTypeListAttribute.perActivity>,
				Or<CRCaseClass.caseClassID, IsNull, Or<CRCaseClass.isBillable, Equal<False>>>>>>>>>>>>>>>>,
			OrderBy<
				Desc<EPActivityApprove.date>>> 
			Activity;

		public PXSelectJoinGroupBy<EPActivityApprove,
			InnerJoin<EPEmployee,
				On<EPEmployee.userID, Equal<EPActivityApprove.ownerID>,
				And<Where<EPEmployee.timeCardRequired, NotEqual<True>, Or<EPEmployee.timeCardRequired, IsNull>>>>,
			LeftJoin<CRActivityLink,
				On<CRActivityLink.noteID, Equal<EPActivityApprove.refNoteID>>,
			LeftJoin<CRCase,
				On<CRCase.noteID, Equal<CRActivityLink.refNoteID>>,
			LeftJoin<CRCaseClass,
				On<CRCaseClass.caseClassID, Equal<CRCase.caseClassID>>,
			LeftJoin<ContractEx,
				On<CRCase.contractID, Equal<ContractEx.contractID>>>>>>>,
			Where2<
				Where2<
					Where<EPActivityApprove.approvalStatus, Equal<ActivityStatusListAttribute.completed>,
						And<EPActivityApprove.approverID, IsNull>>,
					Or<Where<EPActivityApprove.approvalStatus, Equal<ActivityStatusListAttribute.approved>,
						And<EPActivityApprove.approverID, IsNotNull>>>>,
				And<EPActivityApprove.date, Less<Current<EPActivityFilter.tillDatePlusOne>>,
				And2<Where<EPActivityApprove.date, GreaterEqual<Current<EPActivityFilter.fromDate>>,
					Or<Current<EPActivityFilter.fromDate>, IsNull>>,
				And2<Where<EPActivityApprove.projectID, Equal<Current<EPActivityFilter.projectID>>,
					Or<Current<EPActivityFilter.projectID>, IsNull>>,
				And2<Where<EPActivityApprove.projectTaskID, Equal<Current<EPActivityFilter.projectTaskID>>,
					Or<Current<EPActivityFilter.projectTaskID>, IsNull>>,
				And2<Where<ContractEx.contractID, Equal<Current<EPActivityFilter.contractID>>,
					Or<Current<EPActivityFilter.contractID>, IsNull,
					Or<EPActivityApprove.contractID, Equal<Current<EPActivityFilter.contractID>>>>>,
				And<EPActivityApprove.projectID, IsNotNull,
				And<EPActivityApprove.released, NotEqual<True>,
				And<EPActivityApprove.trackTime, Equal<True>,
				And<EPActivityApprove.origNoteID, IsNull,
				And2<Where<EPActivityApprove.ownerID, Equal<Current<EPActivityFilter.employeeID>>,
					Or<Current<EPActivityFilter.employeeID>, IsNull>>,
				And<Where<CRCaseClass.perItemBilling, Equal<BillingTypeListAttribute.perActivity>,
				Or<CRCaseClass.caseClassID, IsNull, Or<CRCaseClass.isBillable, Equal<False>>>>>>>>>>>>>>>>,
			Aggregate<Sum<EPActivityApprove.timeSpent, Sum<EPActivityApprove.overtimeSpent, Sum<EPActivityApprove.timeBillable, Sum<EPActivityApprove.overtimeBillable>>>>>> Totals;

		#endregion

		#region Actions
		
		[PXUIField(DisplayName ="")]
		[PXEditDetailButton]
		public virtual IEnumerable ViewDetails(PXAdapter adapter)
		{
			var row = Activity.Current;
			if (row != null)
			{
				PXRedirectHelper.TryRedirect(this, row, PXRedirectHelper.WindowMode.NewWindow);
			}
			return adapter.Get();
		}

		public PXAction<EPActivityFilter> viewCase;
		[PXButton()]
		[PXUIField(Visible = false)]
		public virtual IEnumerable ViewCase(PXAdapter adapter)
		{
			var row = Activity.Current;

			var apRow = (EPActivityApprove)row;

			if (row != null && row.RefNoteID != null)
			{
				CRCase caseRow = PXSelectJoin<CRCase,
					InnerJoin<CRActivityLink,
						On<CRActivityLink.refNoteID, Equal<CRCase.noteID>>>,
					Where<CRActivityLink.noteID, Equal<Required<EPActivityApprove.refNoteID>>>>.Select(this, apRow.RefNoteID);

				if (caseRow != null)
					PXRedirectHelper.TryRedirect(this, caseRow, PXRedirectHelper.WindowMode.NewWindow);
			}
			return adapter.Get();
		}

		public PXAction<EPActivityFilter> viewContract;
		[PXButton()]
		[PXUIField(Visible = false)]
		public virtual IEnumerable ViewContract(PXAdapter adapter)
		{
			var row = Activity.Current;

			var apRow = (EPActivityApprove)row;

			if (apRow != null)
			{
				Contract contractRow = null;

				if (apRow.ContractID != null)
				{
					contractRow = PXSelect<Contract,
						Where<Contract.contractID, Equal<Required<Contract.contractID>>>>.Select(this, apRow.ContractID);
				}
				else
				{
					contractRow = PXSelectJoin<Contract,
						InnerJoin<CRCase,
							On<CRCase.contractID, Equal<Contract.contractID>>,
						InnerJoin<CRActivityLink, 
							On<CRActivityLink.refNoteID, Equal<CRCase.noteID>>>>,
						Where<CRActivityLink.noteID, Equal<Required<EPActivityApprove.refNoteID>>>>.Select(this, apRow.RefNoteID);	
				}

				
				if (contractRow != null)
					PXRedirectHelper.TryRedirect(this, contractRow, PXRedirectHelper.WindowMode.NewWindow);
			}
			return adapter.Get();
		}
		#endregion

		#region Event handlers

		protected virtual void EPActivityFilter_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			EPActivityApprove totals = Totals.Select();
			EPActivityFilter row = (EPActivityFilter)e.Row;
			if (totals != null && row != null)
			{
				row.BillableTime = totals.TimeBillable - totals.OvertimeBillable;
				row.BillableOvertime = totals.OvertimeBillable;
				row.BillableTotal = totals.TimeBillable;
				row.RegularTime = totals.TimeSpent - totals.OvertimeSpent;
				row.RegularOvertime = totals.OvertimeSpent;
				row.RegularTotal = totals.TimeSpent;
			}
		}

		protected virtual void EPActivityFilter_FromDate_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			EPActivityFilter row = (EPActivityFilter) e.Row;
			if (row != null && e.ExternalCall && row.FromDate > row.TillDate)
				row.TillDate = row.FromDate;
		}

		protected virtual void EPActivityFilter_TillDate_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			EPActivityFilter row = (EPActivityFilter) e.Row;
			if (row != null && e.ExternalCall && row.FromDate != null && row.FromDate > row.TillDate)
				row.FromDate = row.TillDate;
		}
		
		#endregion

		#region Filter

		[Serializable]
		[PXHidden]
		public class EPActivityFilter : CR.OwnedFilter
		{

			#region EmployeeID

			public abstract class employeeID : PX.Data.BQL.BqlGuid.Field<employeeID> { }

			[PXDBGuid]
			[PXUIField(DisplayName = "Employee")]
			[PXSubordinateAndWingmenOwnerSelector]
			public virtual Guid? EmployeeID { set; get; }

			#endregion

			#region FromDate

			public abstract class fromDate : PX.Data.BQL.BqlDateTime.Field<fromDate> { }

			[PXDBDateAndTime(DisplayMask = "d", PreserveTime = true, UseTimeZone = true)]
			[PXUIField(DisplayName = "From Date", Visibility = PXUIVisibility.Visible)]
			public virtual DateTime? FromDate { set; get; }

			#endregion

			#region TillDate

			public abstract class tillDate : PX.Data.BQL.BqlDateTime.Field<tillDate> { }

			[PXDBDateAndTime(DisplayMask = "d", PreserveTime = true, UseTimeZone = true)]
			[BusinessDateTimeDefault]
			[PXUIField(DisplayName = "Until Date", Visibility = PXUIVisibility.Visible)]
			public virtual DateTime? TillDate { set; get; }

			#endregion

			#region TillDatePlusOne

			public abstract class tillDatePlusOne : PX.Data.BQL.BqlDateTime.Field<tillDatePlusOne> { }

			[PXDate()]
			public virtual DateTime? TillDatePlusOne
			{
				get { return TillDate.GetValueOrDefault(DateTime.Now).AddDays(1);}
			}

			#endregion

			#region ProjectID

			public abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }

			[EPProject(typeof(ownerID), DisplayName = PM.Messages.Project)]
			public virtual Int32? ProjectID { set; get; }

			#endregion

			#region ProjectTaskID

			public abstract class projectTaskID : PX.Data.BQL.BqlInt.Field<projectTaskID> { }

			[PXDefault(typeof(Search<PMTask.taskID, Where<PMTask.projectID, Equal<Current<projectID>>, And<PMTask.isDefault, Equal<True>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
			[ProjectTask(typeof (EPActivityFilter.projectID))]
			public virtual Int32? ProjectTaskID { set; get; }

			#endregion

			#region ContractID

			public abstract class contractID : PX.Data.BQL.BqlInt.Field<contractID> { }

			[PXDBInt]
			[PXUIField(DisplayName = "Contract")]
			[PXDimensionSelector(ContractAttribute.DimensionName, typeof(Search2<ContractExEx.contractID,
					LeftJoin<ContractBillingSchedule, On<ContractExEx.contractID, Equal<ContractBillingSchedule.contractID>>>,
				Where<ContractExEx.baseType, Equal<CTPRType.contract>>,
				OrderBy<Desc<ContractExEx.contractCD>>>),
				typeof(ContractExEx.contractCD), DescriptionField = typeof(ContractExEx.description), Filterable = true)]
			public virtual Int32? ContractID { set; get; }

			#endregion

			#region Total

			#region Regular

			public abstract class regularTime : PX.Data.BQL.BqlInt.Field<regularTime> { }

			[PXInt]
			[PXUIField(DisplayName = "Time Spent", Visibility = PXUIVisibility.Visible, Enabled = false)]
			public virtual Int32? RegularTime { set; get; }

			#endregion

			#region RegularOvertime

			public abstract class regularOvertime : PX.Data.BQL.BqlInt.Field<regularOvertime> { }

			[PXInt]
			[PXUIField(DisplayName = "Regular Overtime", Visibility = PXUIVisibility.Visible, Enabled = false)]
			public virtual Int32? RegularOvertime { set; get; }

			#endregion

			#region RegularTotal

			public abstract class regularTotal : PX.Data.BQL.BqlInt.Field<regularTotal> { }

			[PXInt]
			[PXUIField(DisplayName = "Regular Total", Visibility = PXUIVisibility.Visible, Enabled = false)]
			public virtual Int32? RegularTotal { set; get; }

			#endregion

			#region BillableTime

			public abstract class billableTime : PX.Data.BQL.BqlInt.Field<billableTime> { }

			[PXInt]
			[PXUIField(DisplayName = "Billable", Visibility = PXUIVisibility.Visible, Enabled = false)]
			public virtual Int32? BillableTime { set; get; }

			#endregion

			#region BillableOvertime

			public abstract class billableOvertime : PX.Data.BQL.BqlInt.Field<billableOvertime> { }

			[PXInt]
			[PXUIField(DisplayName = "Billable Overtime", Visibility = PXUIVisibility.Visible, Enabled = false)]
			public virtual Int32? BillableOvertime { set; get; }

			#endregion

			#region BillableTotal

			public abstract class billableTotal : PX.Data.BQL.BqlInt.Field<billableTotal> { }

			[PXInt]
			[PXUIField(DisplayName = "Billable Total", Visibility = PXUIVisibility.Visible, Enabled = false)]
			public virtual Int32? BillableTotal { set; get; }

			#endregion

			#endregion

		}

		#endregion

		
		protected static void ReleaseActivities(List<EPActivityApprove> activities)
		{
			RegisterEntry registerEntry = (RegisterEntry)PXGraph.CreateInstance(typeof(RegisterEntry));
			bool activityAdded = false;
			bool contractUsageError = false;
			bool costTransError = false;

			using (PXTransactionScope ts = new PXTransactionScope())
			{

				List<EPActivityApprove> useActivities = RecordContractUsage(activities, "Contract-Usage");
				if (useActivities.Count != activities.Count)
				{
					contractUsageError = true;
				}

				if (!RecordCostTrans(registerEntry, useActivities, out activityAdded))
				{
					costTransError = true;
				}

				ts.Complete();
			}

			if (contractUsageError)
			{
				throw new PXException(Messages.FailedCreateContractUsageTransactions);
			}

			EPSetup setup = PXSelect<EPSetup>.Select(registerEntry);

			if (activityAdded && setup != null && setup.AutomaticReleasePM == true)
			{
				PX.Objects.PM.RegisterRelease.Release(registerEntry.Document.Current);
			}
			
			if (costTransError)
			{
				throw new PXException(Messages.FailedCreateCostTransactions);
			}
		}

        public static List<EPActivityApprove> RecordContractUsage(List<EPActivityApprove> activities, string description)
        {
            RegisterEntry registerEntry = CreateInstance<RegisterEntry>();
            registerEntry.FieldVerifying.AddHandler<PMTran.projectID>((PXCache sender, PXFieldVerifyingEventArgs e) => { e.Cancel = true; });
            registerEntry.FieldVerifying.AddHandler<PMTran.inventoryID>((PXCache sender, PXFieldVerifyingEventArgs e) => { e.Cancel = true; });//restriction should be applicable only for budgeting.
            registerEntry.Document.Cache.Insert();
            registerEntry.Document.Current.Description = description;
            registerEntry.Document.Current.Released = true;
			registerEntry.Document.Current.Status = PMRegister.status.Released;
			registerEntry.Views.Caches.Add(typeof(EPActivityApprove));
			PXCache activityCache = registerEntry.Caches<EPActivityApprove>();
			List<EPActivityApprove> useActivities = new List<EPActivityApprove>();

			for (int i = 0; i < activities.Count; i++)
            {
                EPActivityApprove activity = activities[i];
                try
                {
                    PMTran usage = null;
                    if (activity.ContractID != null)
	                {
		                //Contract without CRM mode
						usage = CreateContractUsage(registerEntry, activity.ContractID, activity, activity.TimeBillable.GetValueOrDefault());
						activity.Billed = true;
						activityCache.Update(activity);
	                }
					else if (activity.RefNoteID != null && 
						PXSelectJoin<CRCase,
						InnerJoin<CRActivityLink, 
							On<CRActivityLink.refNoteID, Equal<CRCase.noteID>>>,  
						Where<CRActivityLink.noteID, Equal<Required<PMTimeActivity.refNoteID>>>>.Select(registerEntry, activity.RefNoteID).Count == 1)
                    {
                        //Add Contract-Usage
						usage = registerEntry.CreateContractUsage(activity, activity.TimeBillable.GetValueOrDefault());
						activity.Billed = true;
						activityCache.Update(activity);
                    }
                    if (usage != null)
                    {
                        UsageMaint.AddUsage(activityCache, usage.ProjectID, usage.InventoryID, usage.BillableQty ?? 0m, usage.UOM);
                    }

					useActivities.Add(activity);
                }
                catch (Exception e)
                {
                    PXProcessing<EPActivityApprove>.SetError(i, e is PXOuterException ? e.Message + "\r\n" + String.Join("\r\n", ((PXOuterException)e).InnerMessages) : e.Message);
                }
				
            }

				if (registerEntry.Transactions.Cache.IsInsertedUpdatedDeleted)
				{
					registerEntry.Save.Press();
				}
				else
				{
					activityCache.Persist(PXDBOperation.Update);
				}

			return useActivities;

        }

        public static bool RecordCostTrans(RegisterEntry registerEntry, List<EPActivityApprove> activities, out bool activityAdded)
        {
			registerEntry.FieldVerifying.AddHandler<PMTran.inventoryID>((PXCache sender, PXFieldVerifyingEventArgs e) => { e.Cancel = true; });//restriction should be applicable only for budgeting.
			EmployeeCostEngine costEngine = new EmployeeCostEngine(registerEntry);

            registerEntry.Views.Caches.Add(typeof(EPActivityApprove));
            PXCache activityCache = registerEntry.Caches<EPActivityApprove>();			

			registerEntry.Document.Cache.Insert();
            bool success = true;
	        activityAdded = false;
            for (int i = 0; i < activities.Count; i++)
            {
				//Check the UNMERGED state of Released (it could be set to true just now by the calling code):
				EPActivityApprove activity = PXSelect<EPActivityApprove>.Search<EPActivityApprove.noteID>(registerEntry, activities[i].NoteID);

				if (activity.Released == true)//activity can be released to PM via Timecard prior to releasing the case.
                    continue;
                
                try
                {
					if (activity.ProjectTaskID != null)//cost transactions are created only if project is set.
					{
						EPEmployee employee = PXSelect<EPEmployee>.Search<EPEmployee.userID>(registerEntry, activity.OwnerID);
						activity.LabourItemID = costEngine.GetLaborClass(activity);
						if (activity.LabourItemID == null)
							throw new PXException(Messages.CannotFindLabor, employee.AcctName);
						activityCache.Update(activity);

						var cost = costEngine.CalculateEmployeeCost(activity.TimeCardCD, activity.EarningTypeID, activity.LabourItemID, activity.ProjectID, activity.ProjectTaskID, activity.CertifiedJob, activity.UnionID, employee.BAccountID, activity.Date.Value);
						if (cost == null)
						{
							throw new PXException(Messages.EmployeeCostRateNotFound);
						}
						EPSetup epsetup = PXSelect<EPSetup>.Select(registerEntry);
						if (EPSetupMaint.GetPostingOption(registerEntry, epsetup, employee.BAccountID) != EPPostOptions.DoNotPost)
							registerEntry.CreateTransaction(activity, employee.BAccountID, activity.Date.Value, activity.TimeSpent, activity.TimeBillable, cost.Rate, cost.OvertimeMultiplier);
						activity.EmployeeRate = cost.Rate;
						activityAdded = true;
					}
					
                    activity.Released = true;
                    activity.ApprovalStatus = ActivityStatusAttribute.Released;                    
                    if (activity.RefNoteID != null)
		                PXUpdate<
							Set<CRActivity.isLocked, True>, 
							CRActivity,
			                Where<CRActivity.noteID, Equal<Required<CRActivity.noteID>>>>
			                .Update(registerEntry, activity.RefNoteID);							
                    activityCache.Update(activity);
                }
                catch (Exception e)
                {
                    PXProcessing<EPActivityApprove>.SetError(i, e is PXOuterException ? e.Message + "\r\n" + String.Join("\r\n", ((PXOuterException)e).InnerMessages) : e.Message);
                    success = false;
                }
            }
			
	            if (activityAdded)
	            {
		            registerEntry.Save.Press();
	            }
	            else
	            {
		            activityCache.Persist(PXDBOperation.Update);
					registerEntry.SelectTimeStamp();
                }

            return success;
        }

		///NO CRM Mode
		public static PMTran CreateContractUsage(RegisterEntry graph, int? contractID, PMTimeActivity timeActivity, int billableMinutes)
		{
			if (timeActivity.ApprovalStatus == ActivityStatusListAttribute.Canceled)
				return null;

			if (timeActivity.IsBillable != true)
				return null;

			Contract contract = PXSelect<Contract,
				Where<Contract.contractID, Equal<Required<Contract.contractID>>>>.Select(graph, contractID);

			if (contract == null)
				return null;//activity has no contract and will be billed through Project using the cost-transaction. Contract-Usage is not created in this case. 

			int? laborItemID = GetContractLaborClassID(graph, contractID, timeActivity); ;

			if (laborItemID == null)
			{
				EPEmployee employeeSettings = PXSelect<EPEmployee,
					Where<EPEmployee.userID, Equal<Required<EPEmployee.userID>>>>.Select(graph, timeActivity.OwnerID);

				if (employeeSettings != null)
				{
					laborItemID =
						EPEmployeeClassLaborMatrix.GetLaborClassID(graph, employeeSettings.BAccountID, timeActivity.EarningTypeID) ??
						employeeSettings.LabourItemID;
				}
			}

			InventoryItem laborItem = PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>.Select(graph, laborItemID);

			if (laborItem == null)
			{
				throw new PXException(PX.Objects.CR.Messages.LaborNotConfigured);

			}

			//save the sign of the value and do the rounding against absolute value.
			//reuse sign later when setting value to resulting transaction.
			int sign = billableMinutes < 0 ? -1 : 1;
			billableMinutes = Math.Abs(billableMinutes);


			if (billableMinutes > 0)
			{
				PMTran newLabourTran = new PMTran();
				newLabourTran.ProjectID = contractID;
				newLabourTran.InventoryID = laborItem.InventoryID;
				newLabourTran.AccountGroupID = contract.ContractAccountGroup;
				newLabourTran.OrigRefID = timeActivity.NoteID;
				newLabourTran.BAccountID = contract.CustomerID;
				newLabourTran.LocationID = contract.LocationID;
				newLabourTran.Description = timeActivity.Summary;
				newLabourTran.StartDate = timeActivity.Date;
				newLabourTran.EndDate = timeActivity.Date;
				newLabourTran.Date = timeActivity.Date;
				newLabourTran.UOM = laborItem.SalesUnit;
				newLabourTran.Qty = sign * Convert.ToDecimal(TimeSpan.FromMinutes(billableMinutes).TotalHours);
				newLabourTran.BillableQty = newLabourTran.Qty;
				newLabourTran.Released = true;
				newLabourTran.Allocated = true;
				newLabourTran.IsQtyOnly = true;
				newLabourTran.BillingID = contract.BillingID;
				return graph.Transactions.Insert(newLabourTran);
			}
			else
			{
				return null;
			}

		}

		///NO CRM Mode
		public static int? GetContractLaborClassID(PXGraph graph, int? contractID, PMTimeActivity activity)
		{
			EPContractRate matrix =
				PXSelectJoin<
					EPContractRate,
					InnerJoin<EPEmployee,
						On<EPContractRate.employeeID, Equal<EPEmployee.bAccountID>>>,
					Where<EPContractRate.contractID, Equal<Required<EPContractRate.contractID>>,
						And<EPContractRate.earningType, Equal<Required<CRPMTimeActivity.earningTypeID>>,
						And<EPEmployee.userID, Equal<Required<CRPMTimeActivity.ownerID>>>>>>.Select(graph, contractID, activity.EarningTypeID, activity.OwnerID);
			if (matrix == null)
				matrix =
					PXSelect<
						EPContractRate,
						Where<EPContractRate.contractID, Equal<Required<EPContractRate.contractID>>,
							And<EPContractRate.earningType, Equal<Required<CRPMTimeActivity.earningTypeID>>,
							And<EPContractRate.employeeID, IsNull>>>>.Select(graph, contractID, activity.EarningTypeID);
			if (matrix != null)
				return matrix.LabourItemID;
			else
				return null;
		}
	}
}

