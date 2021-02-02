using System;
using System.Collections;
using PX.Data.EP;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.CT;
using PX.Objects.PM;
using PX.Data;
using PX.TM;
using System.Linq;
using PX.Objects.GL;

namespace PX.Objects.EP
{
	public class EmployeeActivitiesApprove : PXGraph<EmployeeActivitiesApprove>
	{
		#region Selects

		[PXHidden]
		public PXSelect<ContractEx> dummyContract;
		[PXHidden]
		public PXSelect<CRCase> dummyCase;

		public PXFilter<EPActivityFilter> Filter;

		public PXSelectJoin<EPActivityApprove,
			LeftJoin<CRActivityLink,
				On<CRActivityLink.noteID, Equal<EPActivityApprove.refNoteID>>,
			LeftJoin<EPEarningType,
				On<EPEarningType.typeCD, Equal<EPActivityApprove.earningTypeID>>,
			LeftJoin<PMProject,
				On<PMProject.contractID, Equal<EPActivityApprove.projectID>>,
			LeftJoin<CRCase,
				On<CRCase.noteID, Equal<CRActivityLink.refNoteID>>,
			LeftJoin<ContractEx,
				On<CRCase.contractID, Equal<ContractEx.contractID>>>>>>>,
			Where2<
				Where<EPActivityApprove.approverID, Equal<Current<EPActivityFilter.approverID>>,
					Or<PMProject.approverID, Equal<Current<EPActivityFilter.approverID>>>>,
				And<EPActivityApprove.approverID, IsNotNull,
					And2<Where<EPActivityApprove.approvalStatus, NotEqual<ActivityStatusListAttribute.canceled>,
						    And<EPActivityApprove.approvalStatus, NotEqual<ActivityStatusListAttribute.completed>,
							And<EPActivityApprove.approvalStatus, NotEqual<ActivityStatusListAttribute.open>,
							And<EPActivityApprove.released, NotEqual<True>>>>>,
						And<EPActivityApprove.date, Less<Add<Current<EPActivityFilter.tillDate>, int1>>,
							And2<Where<EPActivityApprove.date, GreaterEqual<Current<EPActivityFilter.fromDate>>,
									Or<Current<EPActivityFilter.fromDate>, IsNull>>,
								And2<Where<EPActivityApprove.projectID, Equal<Current<EPActivityFilter.projectID>>,
										Or<Current<EPActivityFilter.projectID>, IsNull>>,
									And2<Where<EPActivityApprove.projectTaskID, Equal<Current<EPActivityFilter.projectTaskID>>,
											Or<Current<EPActivityFilter.projectTaskID>, IsNull>>,
										And2<Where<EPActivityApprove.ownerID, Equal<Current<EPActivityFilter.employeeID>>,
												Or<Current<EPActivityFilter.employeeID>, IsNull>>,
											And<EPActivityApprove.released, Equal<False>,
											And<EPActivityApprove.trackTime, Equal<True>>>>>>>>>>>,
			OrderBy<Desc<EPActivityApprove.date>>> Activity;

		#endregion

		public EmployeeActivitiesApprove()
		{
			//NO CRM Mode
			PXUIFieldAttribute.SetVisible<EPActivityApprove.contractID>(Activity.Cache, null, !PXAccess.FeatureInstalled<FeaturesSet.customerModule>());
			PXUIFieldAttribute.SetVisible<ContractEx.contractCD>(dummyContract.Cache, null, PXAccess.FeatureInstalled<FeaturesSet.customerModule>());
			PXUIFieldAttribute.SetVisible<CRCase.caseCD>(dummyCase.Cache, null, PXAccess.FeatureInstalled<FeaturesSet.customerModule>());

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
		}

		#region Actions

		public PXSave<EPActivityFilter> Save;
		public PXCancel<EPActivityFilter> Cancel;
		public PXAction<EPActivityFilter> viewDetails;
		public PXAction<EPActivityFilter> approveAll;
		public PXAction<EPActivityFilter> rejectAll;
		public PXAction<EPActivityFilter> viewCase;
		public PXAction<EPActivityFilter> viewContract;

		[PXUIField(DisplayName = "")]
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

		[PXUIField(DisplayName = Messages.ApproveAll)]
		[PXButton()]
		public virtual IEnumerable ApproveAll(PXAdapter adapter)
		{
			if (Activity.Current == null || Filter.View.Ask(Messages.ApproveAllConfirmation, MessageButtons.YesNo) != WebDialogResult.Yes)
			{
				return adapter.Get();
			}

			foreach (EPActivityApprove item in Activity.Select())
			{
				item.IsApproved = true;
				item.IsReject = false;
				Activity.Cache.Update(item);
			}
			Persist();
			return adapter.Get();
		}

		[PXUIField(DisplayName = Messages.RejectAll)]
		[PXButton()]
		public virtual IEnumerable RejectAll(PXAdapter adapter)
		{
			if (Activity.Current == null || Filter.View.Ask(Messages.RejectAllConfirmation, MessageButtons.YesNo) != WebDialogResult.Yes)
			{
				return adapter.Get();
			}

			foreach (EPActivityApprove item in Activity.Select())
			{
				item.IsApproved = false;
				item.IsReject = true;
				Activity.Cache.Update(item);
			}
			Persist();
			return adapter.Get();
		}

		[PXButton()]
		[PXUIField(Visible = false)]
		public virtual IEnumerable ViewCase(PXAdapter adapter)
		{
			CRActivityLink row = (PXResultset<EPActivityApprove, CRActivityLink>)Activity.Search<EPActivityApprove.noteID>(Activity.Current?.NoteID);
			if (row != null)
			{
				CRCase caseRow = PXSelect<CRCase>.Search<CRCase.noteID>(this, row.RefNoteID);
				if (caseRow != null)
					PXRedirectHelper.TryRedirect(this, caseRow, PXRedirectHelper.WindowMode.NewWindow);
			}
			return adapter.Get();
		}

		[PXButton()]
		[PXUIField(Visible = false)]
		public virtual IEnumerable ViewContract(PXAdapter adapter)
		{
			var row = (PXResultset<EPActivityApprove, CRActivityLink>)Activity.Search<EPActivityApprove.noteID>(Activity.Current?.NoteID);

			var activityLink = (CRActivityLink) row;
			var actApr = (EPActivityApprove) row;

			if (row != null)
			{
				Contract contractRow;
				if (actApr.ContractID != null)
				{
					contractRow = PXSelect<Contract, 
						Where<Contract.contractID, Equal<Required<Contract.contractID>>>>.Select(this, actApr.ContractID);
				}
				else
				{
					contractRow = PXSelectJoin<Contract, 
						InnerJoin<CRCase, 
							On<CRCase.contractID, Equal<Contract.contractID>>>, 
						Where<CRCase.noteID, Equal<Required<CRActivityLink.refNoteID>>>>.Select(this, activityLink.RefNoteID);
				}


				if (contractRow != null)
					PXRedirectHelper.TryRedirect(this, contractRow, PXRedirectHelper.WindowMode.NewWindow);
			}
			return adapter.Get();
		}

		#endregion

		#region Event handlers

		[PXUIEnabled(typeof(EPActivityApprove.isBillable))]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected virtual void EPActivityApprove_TimeBillable_CacheAttached(PXCache sender) { }

		protected virtual void EPActivityFilter_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			EPActivityFilter row = (EPActivityFilter)e.Row;
			if (row != null)
			{
				row.BillableTime = 0;
				row.BillableOvertime = 0;
				row.BillableTotal = 0;
				row.RegularTime = 0;
				row.RegularOvertime = 0;
				row.RegularTotal = 0;
				foreach (PXResult<EPActivityApprove, CRActivityLink, EPEarningType> item in Activity.Select())
				{
					EPActivityApprove rowActivity = (EPActivityApprove)item;
					EPEarningType rowEarningType = (EPEarningType)item;

					if (rowEarningType.IsOvertime == true)
					{
						row.RegularOvertime += rowActivity.TimeSpent.GetValueOrDefault(0);
						row.BillableOvertime += rowActivity.TimeBillable.GetValueOrDefault(0);
					}
					else
					{
						row.RegularTime += rowActivity.TimeSpent.GetValueOrDefault(0);
						row.BillableTime += rowActivity.TimeBillable.GetValueOrDefault(0);
					}

					row.BillableTotal = row.BillableTime + row.BillableOvertime;
					row.RegularTotal = row.RegularTime + row.RegularOvertime;
				}
			}
		}

		protected virtual void EPActivityFilter_FromDate_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			EPActivityFilter row = (EPActivityFilter)e.Row;
			if (row != null && e.ExternalCall && row.FromDate > row.TillDate)
				row.TillDate = row.FromDate;
		}

		protected virtual void EPActivityFilter_TillDate_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			EPActivityFilter row = (EPActivityFilter)e.Row;
			if (row != null && e.ExternalCall && row.FromDate != null && row.FromDate > row.TillDate)
				row.FromDate = row.TillDate;
		}

		protected virtual void EPActivityFilter_RowUpdating(PXCache sender, PXRowUpdatingEventArgs e)
		{
			if (Activity.Cache.IsDirty && Filter.View.Ask(ActionsMessages.ConfirmationMsg, MessageButtons.YesNo) != WebDialogResult.Yes)
			{
				e.Cancel = true;
			}
			else
			{
				Activity.Cache.Clear();
			}
		}

		protected virtual void EPActivityApprove_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			EPActivityApprove row = (EPActivityApprove)e.Row;
			if (row == null)
				return;

			PXUIFieldAttribute.SetEnabled(sender, row, false);
			PXUIFieldAttribute.SetEnabled<EPActivityApprove.isApproved>(sender, row, true);
			PXUIFieldAttribute.SetEnabled<EPActivityApprove.isReject>(sender, row, true);
		}

		protected virtual void EPActivityApprove_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			EPActivityApprove row = (EPActivityApprove) e.Row;
			if (row == null || e.Operation == PXDBOperation.Delete)
				return;
			if (row.IsApproved == true)
			{
				sender.SetValueExt<EPActivityApprove.approvalStatus>(row, ActivityStatusListAttribute.Approved);
				row.ApprovedDate = Accessinfo.BusinessDate;
			}
			else if (row.IsReject == true)
			{
				sender.SetValueExt<EPActivityApprove.approvalStatus>(row, ActivityStatusListAttribute.Rejected);
			}
			else if (row.ApprovalStatus == ActivityStatusListAttribute.Rejected || row.ApprovalStatus == ActivityStatusListAttribute.Approved)
			{
				sender.SetValueExt<EPActivityApprove.approvalStatus>(row, ActivityStatusListAttribute.PendingApproval);
			}
		}
	 
		#endregion

		public override void Persist()
		{
			var groups = Activity.Cache.Updated.Cast<EPActivityApprove>()
				.Where(a => a.IsReject == true && a.TimeCardCD != null)
				.GroupBy(a => a.TimeCardCD)
				.ToList();

			using (var ts = new PXTransactionScope())
			{
				if (groups.Count > 0)
				{
					TimeCardMaint maint = PXGraph.CreateInstance<TimeCardMaint>();

					foreach (var group in groups)
					{
						maint.Clear();
						maint.Document.Current = maint.Document.Search<EPTimeCard.timeCardCD>(group.Key);
						maint.Actions["Reject"].Press();
						maint.Persist();
					}
				}

				base.Persist();

				ts.Complete();
			}
		}

		

		#region Filter
		[Serializable]
		[PXHidden]
		public class EPActivityFilter : IBqlTable
		{

			#region ApproverID
			public abstract class approverID : PX.Data.BQL.BqlInt.Field<approverID> { }
			protected Int32? _ApproverID;
			[PXDBInt]
			[PXSubordinateAndWingmenSelector]
			[PXDefault(typeof(Search<EPEmployee.bAccountID, Where<EPEmployee.userID, Equal<Current<AccessInfo.userID>>>>))]
			[PXUIField(DisplayName = "Approver", Visibility = PXUIVisibility.SelectorVisible)]
			public virtual Int32? ApproverID { get; set; }
			#endregion

			#region EmployeeID
			public abstract class employeeID : PX.Data.BQL.BqlGuid.Field<employeeID> { }
			[PXDBGuid]
			[PXUIField(DisplayName = "Employee")]
			[PXSubordinateAndWingmenOwnerSelector]
			public virtual Guid? EmployeeID { set; get; }
			#endregion

			#region FromDate
			public abstract class fromDate : PX.Data.BQL.BqlDateTime.Field<fromDate> { }
			[PXDBDateAndTime(DisplayMask = "d", PreserveTime = true)]
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

			#region ProjectID
			public abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }
			[ProjectByApprover]
			[PXFormula(typeof(Default<approverID>))]
            public virtual Int32? ProjectID { set; get; }
			#endregion

			#region ProjectTaskID
			public abstract class projectTaskID : PX.Data.BQL.BqlInt.Field<projectTaskID> { }
			[PXDefault(typeof(Search<PMTask.taskID, Where<PMTask.projectID, Equal<Current<projectID>>, And<PMTask.isDefault, Equal<True>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
			[ProjectTask(typeof(projectID))]
			public virtual Int32? ProjectTaskID { set; get; }
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

		#region ProjectByApproverAttribute
		[PXDBInt()]
		[PXUIField(DisplayName = "Project", Visibility = PXUIVisibility.Visible)]
		public class ProjectByApproverAttribute : GL.AcctSubAttribute
		{
			public ProjectByApproverAttribute()
			{
				Type searchType = typeof(
					Search5<
						PMProject.contractID
						, LeftJoin<PMTask, On<PMTask.projectID, Equal<PMProject.contractID>, And<PMTask.approverID, Equal<Current<EPActivityFilter.approverID>>>>>
						, Where<
							PMProject.isActive, Equal<True>
							, And<Where<PMTask.taskID, IsNotNull, Or<PMProject.approverID, Equal<Current<EPActivityFilter.approverID>>>>>
							>
						, Aggregate<GroupBy<PMProject.contractID, GroupBy<PMProject.contractCD, GroupBy<PMProject.description, GroupBy<PMProject.status>>>>>
						, OrderBy<Asc<PMProject.contractCD>>
						>
						);

				PXDimensionSelectorAttribute select = new PXDimensionSelectorAttribute(ProjectAttribute.DimensionName, searchType, typeof(PMProject.contractCD),
				typeof(PMProject.contractCD), typeof(PMProject.description), typeof(PMProject.status));
				select.DescriptionField = typeof(PMProject.description);
				select.ValidComboRequired = true;
				select.CacheGlobal = true;

				_Attributes.Add(select);
				_SelAttrIndex = _Attributes.Count - 1;
			}

			public override void CacheAttached(PXCache sender)
			{
				base.CacheAttached(sender);
				/*Visible =*/
				Enabled = ProjectAttribute.IsPMVisible( GL.BatchModule.TA);
			}
		}

		#endregion   


	}
	
	[Serializable]
	public class EPActivityApprove : PMTimeActivity
	{
		#region Overrides
		public new abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
		public new abstract class parentTaskNoteID : PX.Data.BQL.BqlGuid.Field<parentTaskNoteID> { }
		public new abstract class summary : PX.Data.BQL.BqlString.Field<summary> { }
		public new abstract class timeSpent : PX.Data.BQL.BqlInt.Field<timeSpent> { }
		public new abstract class timeBillable : PX.Data.BQL.BqlInt.Field<timeBillable> { }
		public new abstract class ownerID : PX.Data.BQL.BqlGuid.Field<ownerID> { }
		#endregion

		#region TrackTime
		public new abstract class trackTime : PX.Data.BQL.BqlBool.Field<trackTime> { }

		[PXDBBool]
		[PXDefault(true)]
		public override bool? TrackTime { get; set; }
		#endregion

		#region Approve
		public abstract class isApproved : PX.Data.BQL.BqlBool.Field<isApproved> { }
		protected bool? _IsApproved;
		[PXBool]
		[PXUIField(DisplayName = "Approve")]
		public virtual bool? IsApproved
		{
			get
			{
				return _IsApproved ?? ApprovalStatus == ActivityStatusListAttribute.Approved;
			}
			set
			{
				_IsApproved = value;
				if (_IsApproved == true)
					_IsReject = false;
			}
		}
		#endregion

		#region IsReject
		public abstract class isReject : PX.Data.BQL.BqlBool.Field<isReject> { }
		protected bool? _IsReject;
		[PXBool]
		[PXUIField(DisplayName = "Reject")]
		public virtual bool? IsReject
		{
			get
			{
				return _IsReject ?? ApprovalStatus == ActivityStatusListAttribute.Rejected;
			}
			set
			{
				_IsReject = value;
				if (_IsReject == true)
					_IsApproved = false;
			}
		}

		#endregion
		
		#region ContractID
		public new abstract class contractID : PX.Data.BQL.BqlInt.Field<contractID> { }

		[PXDBInt(BqlField = typeof(PMTimeActivity.contractID))]
		[PXUIField(DisplayName = "Contract", Visible = false)]
		[PXSelector(typeof(Search2<ContractExEx.contractID,
				LeftJoin<ContractBillingSchedule, On<ContractExEx.contractID, Equal<ContractBillingSchedule.contractID>>>,
			Where<ContractExEx.baseType, Equal<CTPRType.contract>>,
			OrderBy<Desc<ContractExEx.contractCD>>>),
			DescriptionField = typeof(ContractExEx.description),
			SubstituteKey = typeof(ContractExEx.contractCD), Filterable = true)]
		[PXRestrictor(typeof(Where<ContractExEx.status, Equal<Contract.status.active>>), CR.Messages.ContractIsNotActive)]
		[PXRestrictor(typeof(Where<Current<AccessInfo.businessDate>, LessEqual<ContractExEx.graceDate>, Or<Contract.expireDate, IsNull>>), CR.Messages.ContractExpired)]
		[PXRestrictor(typeof(Where<Current<AccessInfo.businessDate>, GreaterEqual<ContractExEx.startDate>>), CR.Messages.ContractActivationDateInFuture, typeof(ContractExEx.startDate))]
		public override Int32? ContractID { set; get; }
        #endregion

        #region ProjectID
        public new abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }

        [EPActivityProjectDefault(typeof(isBillable))]
        [EPProject(typeof(ownerID), FieldClass = ProjectAttribute.DimensionName, BqlField = typeof(PMTimeActivity.projectID))]
        [PXFormula(typeof(
            Switch<
                Case<Where<Not<FeatureInstalled<FeaturesSet.projectModule>>>, DefaultValue<projectID>,
                Case<Where<isBillable, Equal<True>, And<Current2<projectID>, Equal<NonProject>>>, Null,
                Case<Where<isBillable, Equal<False>, And<Current2<projectID>, IsNull>>, DefaultValue<projectID>>>>,
            projectID>))]
        public override Int32? ProjectID { get; set; }
        #endregion

        #region ProjectTaskID
        public new abstract class projectTaskID : PX.Data.BQL.BqlInt.Field<projectTaskID> { }
        [PXDefault(typeof(Search<PMTask.taskID, Where<PMTask.projectID, Equal<Current<projectID>>, And<PMTask.isDefault, Equal<True>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
        [ProjectTask(typeof(projectID), BatchModule.TA, DisplayName = "Project Task")]
        [PXFormula(typeof(Switch<
            Case<Where<Current2<projectID>, Equal<NonProject>>, Null>,
            projectTaskID>))]

        /// <summary>
        /// Identifier of the <see cref="PX.Objects.PMTask.TaskID">TaskID</see>.
        /// </summary>
        public override int? ProjectTaskID { get; set; }
        #endregion

        #region Date
        public new abstract class date : PX.Data.BQL.BqlDateTime.Field<date> { }		
		[PXDBDateAndTime(BqlField = typeof(PMTimeActivity.date), DisplayNameDate = "Date", DisplayNameTime = "Time")]
		[PXUIField(DisplayName = "Date")]
		public override DateTime? Date { get; set; }
		#endregion

		#region WeekID
		public new abstract class weekID : PX.Data.BQL.BqlInt.Field<weekID> { }

		[PXDBInt(BqlField = typeof(PMTimeActivity.weekID))]
		[PXUIField(DisplayName = "Time Card Week", Enabled = false)]
		[PXWeekSelector2()]
		[PXFormula(typeof(Default<date, trackTime>))]
		[EPActivityDefaultWeek(typeof(date))]
		public override int? WeekID { get; set; }
		#endregion
		
		#region TimeCardCD
		public new abstract class timeCardCD : PX.Data.BQL.BqlString.Field<timeCardCD> { }

		[PXDBString(10, BqlField = typeof(PMTimeActivity.timeCardCD))]
		[PXUIField(DisplayName = "Time Card Ref.", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		public override string TimeCardCD { get; set; }
		#endregion

		#region ApprovalStatus
		public new abstract class approvalStatus : PX.Data.BQL.BqlString.Field<approvalStatus> { }

		[PXDBString(2, IsFixed = true, BqlField = typeof(PMTimeActivity.approvalStatus))]
		[ActivityStatusList]
		[PXUIField(DisplayName = "Status")]
		[PXDefault(ActivityStatusAttribute.Open, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXFormula(typeof(Switch<
			Case<Where<hold, IsNull, Or<hold, Equal<True>>>, ActivityStatusAttribute.open,			
			Case<Where<released, Equal<True>>, ActivityStatusAttribute.released,
			Case<Where<approverID, IsNotNull, And<hold, Equal<False>>>, 
				ActivityStatusAttribute.pendingApproval>>>,
			ActivityStatusAttribute.completed>))]
		public override string ApprovalStatus { get; set; }
		#endregion

		#region ApproverID
		public new abstract class approverID : PX.Data.BQL.BqlInt.Field<approverID> { }

		[PXDBInt(BqlField = typeof(PMTimeActivity.approverID))]
		[PXSelector(typeof(Search<EPEmployee.bAccountID>), SubstituteKey = typeof(EPEmployee.acctCD))]
		[PXFormula(typeof(
			Switch<
				Case<Where<Selector<projectTaskID, PMTask.approverID>, IsNotNull>, Selector<projectTaskID, PMTask.approverID>>, 
				Null>
			))]
		[PXUIField(DisplayName = "Approver", Visibility = PXUIVisibility.SelectorVisible)]
		public override Int32? ApproverID { get; set; }

		#endregion
		
		#region Hold
		public abstract class hold : PX.Data.BQL.BqlBool.Field<hold> { }

		[PXBool]
		[PXUIField(FieldName = "Hold", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual bool? Hold { get; set; }
		#endregion

		public new abstract class overtimeSpent : PX.Data.BQL.BqlInt.Field<overtimeSpent> { }
		public new abstract class overtimeBillable : PX.Data.BQL.BqlInt.Field<overtimeBillable> { }
	}

	//Used in the join. ContractEx__ContractCD must be visible only if CRM is ON.
	public class ContractEx : Contract
	{
		public new abstract class contractID : PX.Data.BQL.BqlInt.Field<contractID> { }

		public new abstract class contractCD : PX.Data.BQL.BqlString.Field<contractCD> { }
	}

	//Used in the selector and thus must Contract_CD be visible at all times
	public class ContractExEx : Contract
	{
		public new abstract class contractID : PX.Data.BQL.BqlInt.Field<contractID> { }

		public new abstract class baseType : PX.Data.BQL.BqlString.Field<baseType> { }

		public new abstract class contractCD : PX.Data.BQL.BqlString.Field<contractCD> { }

		[Obsolete(Common.InternalMessages.PropertyIsObsoleteAndWillBeRemoved2019R2)]
		public new abstract class isTemplate : PX.Data.BQL.BqlBool.Field<isTemplate> { }


		public new abstract class description : PX.Data.BQL.BqlString.Field<description> { }
	}

}