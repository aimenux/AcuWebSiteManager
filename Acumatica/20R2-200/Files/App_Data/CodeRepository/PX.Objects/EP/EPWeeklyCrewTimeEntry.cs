using PX.Api;
using PX.Common;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Data.EP;
using PX.Objects.Common.Extensions;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.PM;
using PX.SM;
using PX.TM;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PX.Objects.EP
{
	public class EPWeeklyCrewTimeEntry : PXGraph<EPWeeklyCrewTimeEntry, EPWeeklyCrewTimeActivity>, PXImportAttribute.IPXPrepareItems
	{
		#region Data Views and delegates

		public SelectFrom<EPWeeklyCrewTimeActivity>.View Document;
		//At least 1 dataview with PMTimeActivity is absolutely needed so PXFomulas/PXParent creation from PMTimeActivity DAC gets triggered
		public SelectFrom<PMTimeActivity> PMTimeActivityDummyView;
		public SelectFrom<EPCompanyTreeMember>
			.Where<EPCompanyTreeMember.workGroupID.IsEqual<EPWeeklyCrewTimeActivity.workgroupID.FromCurrent>
				.And<EPCompanyTreeMember.active.IsEqual<True>>>.View OriginalGroupMembers;

		[PXImport]
		public SelectFrom<EPActivityApprove>
			.InnerJoin<EPEmployee>.On<EPEmployee.defContactID.IsEqual<EPActivityApprove.ownerID>>
			.InnerJoin<EPEarningType>.On<EPEarningType.typeCD.IsEqual<EPActivityApprove.earningTypeID>>
			.Where<EPActivityApprove.workgroupID.IsEqual<EPWeeklyCrewTimeActivity.workgroupID.FromCurrent>
				.And<Brackets<EPActivityApprove.weekID.IsEqual<EPWeeklyCrewTimeActivity.week.FromCurrent>
					.Or<EPActivityApprove.weekID.IsNull>>>>.View TimeActivities;
		public virtual IEnumerable timeActivities()
		{
			IEnumerable<object> returnedList = new List<object>();
			if (Document.Current.WorkgroupID == null || Document.Current.Week == null)
			{
				return returnedList;
			}

			BqlCommand cmd = TimeActivities.View.BqlSelect;
			if (Filter.Current.ProjectID != null)
			{
				cmd = cmd.WhereAnd<Where<EPActivityApprove.projectID, Equal<Current<EPWeeklyCrewTimeActivityFilter.projectID>>>>();
			}
			if (Filter.Current.ProjectTaskID != null)
			{
				cmd = cmd.WhereAnd<Where<EPActivityApprove.projectTaskID, Equal<Current<EPWeeklyCrewTimeActivityFilter.projectTaskID>>>>();
			}

			int startRow = 0;
			int totalRows = 0;
			returnedList = new PXView(this, false, cmd).Select(PXView.Currents, PXView.Parameters, PXView.Searches, PXView.SortColumns, PXView.Descendings,
									TimeActivities.View.GetExternalFilters(), ref startRow, 0, ref totalRows);
			if (Filter.Current.Day != null)
			{
				returnedList = returnedList.Cast<PXResult<EPActivityApprove, EPEmployee, EPEarningType>>().Where(x => ((EPActivityApprove)x).DayOfWeek == Filter.Current.Day);
			}

			Filter.Current.RegularTime = 0;
			Filter.Current.Overtime = 0;
			Filter.Current.BillableTime = 0;
			Filter.Current.BillableOvertime = 0;
			foreach (PXResult<EPActivityApprove, EPEmployee, EPEarningType> record in returnedList)
			{
				var activity = (EPActivityApprove)record;
				var earningType = (EPEarningType)record;

				Filter.Current.RegularTime += earningType.IsOvertime == true ? 0 : activity.TimeSpent.GetValueOrDefault();
				Filter.Current.BillableTime += earningType.IsOvertime == true ? 0 : activity.TimeBillable.GetValueOrDefault();
				Filter.Current.Overtime += activity.OvertimeSpent.GetValueOrDefault();
				Filter.Current.BillableOvertime += activity.OvertimeBillable.GetValueOrDefault();
			}

			return returnedList;
		}

		public SelectFrom<EPTimeActivitiesSummary>
			.Where<EPTimeActivitiesSummary.workgroupID.IsEqual<EPWeeklyCrewTimeActivity.workgroupID.FromCurrent>
				.And<EPTimeActivitiesSummary.week.IsEqual<EPWeeklyCrewTimeActivity.week.FromCurrent>>>.View WorkgroupTimeSummary;
		public virtual IEnumerable workgroupTimeSummary()
		{
			IEnumerable<EPTimeActivitiesSummary> returnedList = new List<EPTimeActivitiesSummary>();
			if (Document.Current.WorkgroupID == null || Document.Current.Week == null)
			{
				return returnedList;
			}

			// Gets activities marked with the selected workgroup
			BqlCommand cmd = WorkgroupTimeSummary.View.BqlSelect;
			returnedList = new PXView(this, false, cmd).SelectMulti().Select(x => (EPTimeActivitiesSummary)x);

			// Inserts group members without activities
			IEnumerable<EPCompanyTreeMember> groupMembers = OriginalGroupMembers.Select().FirstTableItems;
			int totalWithoutActivities = returnedList.Count(x => x.IsWithoutActivities == true);
			bool wasDirty = WorkgroupTimeSummary.Cache.IsDirty;
			foreach (EPCompanyTreeMember member in groupMembers.Where(x => !returnedList.Any(y => x.ContactID == y.ContactID)))
			{
				var summary = new EPTimeActivitiesSummary();
				summary.ContactID = member.ContactID;
				summary = WorkgroupTimeSummary.Insert(summary);
				returnedList = returnedList.Append(summary);
				totalWithoutActivities++;
			}
			WorkgroupTimeSummary.Cache.IsDirty = wasDirty;

			Filter.Current.TotalWorkgroupMembers = returnedList.Count();
			Filter.Current.TotalWorkgroupMembersWithActivities = Filter.Current.TotalWorkgroupMembers - totalWithoutActivities;
			return returnedList;
		}

		public SelectFrom<EPTimeActivitiesSummary>
			.LeftJoin<EPCompanyTreeMember>.On<EPTimeActivitiesSummary.contactID.IsEqual<EPCompanyTreeMember.contactID>>
			.Where<EPTimeActivitiesSummary.workgroupID.IsEqual<EPWeeklyCrewTimeActivity.workgroupID.FromCurrent>
				.And<EPTimeActivitiesSummary.week.IsEqual<EPWeeklyCrewTimeActivity.week.FromCurrent>>>.View CompanyTreeMembers;
		public virtual IEnumerable companyTreeMembers()
		{
			BqlCommand cmd = CompanyTreeMembers.View.BqlSelect;
			if (Filter.Current.ShowAllMembers == false)
			{
				cmd = cmd.WhereAnd<Where<EPTimeActivitiesSummary.status, Equal<WorkgroupMemberStatusAttribute.permanentActive>,
					Or<EPTimeActivitiesSummary.status, Equal<WorkgroupMemberStatusAttribute.temporaryActive>>>>();
			}

			List<object> results = new PXView(this, false, cmd).SelectMulti();
			return results.Cast<PXResult<EPTimeActivitiesSummary, EPCompanyTreeMember>>().Distinct(x => ((EPTimeActivitiesSummary)x).ContactID);
		}

		public PXFilter<EPWeeklyCrewTimeActivityFilter> Filter;
		/// Using EPActivityApprove2 to have a different cache than <see cref="TimeActivities" /> data view.
		public SelectFrom<EPActivityApprove2>.View BulkEntryTimeActivities;
		public IEnumerable bulkEntryTimeActivities()
		{
			return Caches[typeof(EPActivityApprove2)].Inserted;
		}

		public void _(Events.RowPersisting<EPActivityApprove2> e)
		{
			e.Cancel = true;
		}

		#endregion Data Views and delegates

		#region Actions

		public PXAction<EPWeeklyCrewTimeActivity> EnterBulkTime;
		[PXButton]
		[PXUIField(DisplayName = "Enter Bulk Time", Enabled = true)]
		public virtual void enterBulkTime()
		{
			var dialogResult = CompanyTreeMembers.AskExt();
			if (dialogResult == WebDialogResult.OK)
			{
				insertForBulkTimeEntry();
			}
		}

		public PXAction<EPWeeklyCrewTimeActivity> InsertForBulkTimeEntry;
		[PXButton]
		[PXUIField(DisplayName = "Add")]
		public virtual void insertForBulkTimeEntry()
		{
			IEnumerable<EPTimeActivitiesSummary> selectedMembers = CompanyTreeMembers.Select().FirstTableItems.Where(x => x.Selected == true);
			foreach (EPActivityApprove activity in BulkEntryTimeActivities.Select())
			{
				foreach (EPTimeActivitiesSummary contact in selectedMembers)
				{
					var copy = BulkEntryTimeActivities.Cache.CreateCopy(activity) as EPActivityApprove;
					copy.OwnerID = contact.ContactID;
					copy.NoteID = null;
					copy = TimeActivities.Insert(copy);
				}
			}

			Caches[typeof(EPActivityApprove2)].Clear();
		}

		public PXAction<EPWeeklyCrewTimeActivity> CopySelectedActivity;
		[PXButton]
		[PXUIField(DisplayName = "Copy Selected Line")]
		public virtual void copySelectedActivity()
		{
			EPActivityApprove copy = CopyActivity(TimeActivities.Current);
			copy.Hold = true;
			TimeActivities.Insert(copy);
		}

		public PXAction<EPWeeklyCrewTimeActivity> LoadLastWeekActivities;
		[PXButton]
		[PXUIField(DisplayName = "Load activities from previous week")]
		public virtual void loadLastWeekActivities()
		{
			if (Document.Current.WorkgroupID != null && Document.Current.Week != null)
			{
				foreach (EPActivityApprove activity in SelectFrom<EPActivityApprove>
						.Where<EPActivityApprove.workgroupID.IsEqual<EPWeeklyCrewTimeActivity.workgroupID.FromCurrent>
							.And<EPActivityApprove.weekID.IsEqual<P.AsInt>>>.View.Select(this, GetLastWeekID()))
				{
					EPActivityApprove copy = CopyActivity(activity);
					copy.Date = copy.Date.Value.Date.AddDays(7);
					copy.WeekID = Document.Current.Week.Value;
					copy.Hold = true;
					TimeActivities.Insert(copy);
				}
			}
		}

		public PXAction<EPWeeklyCrewTimeActivity> LoadLastWeekMembers;
		[PXButton]
		[PXUIField(DisplayName = "Load workgroup from previous week")]
		public virtual void loadLastWeekMembers()
		{
			if (Document.Current.WorkgroupID != null && Document.Current.Week != null)
			{
				HashSet<int?> currentMembers = WorkgroupTimeSummary.Select().FirstTableItems.Select(x => x.ContactID).ToHashSet();
				foreach (EPTimeActivitiesSummary summary in SelectFrom<EPTimeActivitiesSummary>
						.Where<EPTimeActivitiesSummary.workgroupID.IsEqual<EPWeeklyCrewTimeActivity.workgroupID.FromCurrent>
							.And<EPTimeActivitiesSummary.week.IsEqual<P.AsInt>>>.View.Select(this, GetLastWeekID()))
				{
					if (!currentMembers.Contains(summary.ContactID))
					{
						var newMember = new EPTimeActivitiesSummary();
						newMember.ContactID = summary.ContactID;
						newMember.Week = Document.Current.Week.Value;
						newMember.WorkgroupID = Document.Current.WorkgroupID.Value;
						WorkgroupTimeSummary.Insert(newMember);
					}
				}
			}
		}

		public PXAction<EPWeeklyCrewTimeActivity> DeleteMember;
		[PXButton]
		[PXUIField]
		public virtual void deleteMember()
		{
			WorkgroupTimeSummary.Delete(WorkgroupTimeSummary.Current);
		}

		public PXAction<EPWeeklyCrewTimeActivity> CompleteAllActivities;
		[PXButton]
		[PXUIField(DisplayName = "Complete Activities")]
		public virtual void completeAllActivities()
		{
			foreach (EPActivityApprove activity in TimeActivities.Select().FirstTableItems.Where(x => x.ApprovalStatus == ActivityStatusAttribute.Open))
			{
				activity.Hold = false;
				activity.ApprovalStatus = ActivityStatusAttribute.Completed;
				TimeActivities.Update(activity); 
			}
		}

		#endregion Actions

		#region CacheAttached

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXUIField(DisplayName = "Employee")]
		public virtual void _(Events.CacheAttached<EPActivityApprove.ownerID> e) { }

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXDefault(typeof(EPWeeklyCrewTimeActivity.workgroupID))]
		public virtual void _(Events.CacheAttached<EPActivityApprove.workgroupID> e) { }

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXDefault(typeof(Search<PMProject.contractID, Where<PMProject.nonProject, Equal<True>>>))]
		public virtual void _(Events.CacheAttached<EPActivityApprove.projectID> e) { }

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXCustomizeBaseAttribute(typeof(EPActivityDefaultWeekAttribute), nameof(EPActivityDefaultWeekAttribute.PersistingCheck), PXPersistingCheck.Null)]
		public virtual void _(Events.CacheAttached<EPActivityApprove.weekID> e) { }

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXDefault(PersistingCheck = PXPersistingCheck.Null)]
		public virtual void _(Events.CacheAttached<EPActivityApprove.date> e) { }

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXRemoveBaseAttribute(typeof(PXSelectorAttribute))]
		public virtual void _(Events.CacheAttached<EPActivityApprove.summary> e) { }

		#endregion CacheAttached

		#region Events

		public virtual void _(Events.FieldVerifying<EPActivityApprove.date> e)
		{
			PXWeekSelector2Attribute.WeekInfo weekInfo = PXWeekSelector2Attribute.GetWeekInfo(this, Document.Current.Week.Value);
			if (!weekInfo.IsValid((DateTime)e.NewValue))
			{
				e.NewValue = null;
			}
		}

		public virtual void _(Events.RowSelected<EPActivityApprove> e)
		{
			PXUIFieldAttribute.SetError<EPActivityApprove.date>(e.Cache, e.Row, e.Row.Date == null ? string.Format(ErrorMessages.FieldIsEmpty, Messages.DateTimeField) : null);
			PXUIFieldAttribute.SetEnabled(e.Cache, e.Row, e.Row.ApprovalStatus == ActivityStatusListAttribute.Open);
			e.Cache.Adjust<PXUIFieldAttribute>().ForAllFields(field => field.Enabled = e.Row.ApprovalStatus == ActivityStatusListAttribute.Open)
				.For<EPActivityApprove.approvalStatus>(field => field.Enabled = false)
				.For<EPActivityApprove.hold>(field => field.Enabled = e.Row.ApprovalStatus == ActivityStatusListAttribute.Open
					|| e.Row.ApprovalStatus == ActivityStatusListAttribute.Completed);
		}

		public virtual void _(Events.FieldDefaulting<EPActivityApprove.projectID> e)
		{
			if (Filter.Current.ProjectID != null)
			{
				e.NewValue = Filter.Current.ProjectID;
			}
		}

		public virtual void _(Events.FieldDefaulting<EPActivityApprove.projectTaskID> e)
		{
			if (Filter.Current.ProjectTaskID != null)
			{
				e.NewValue = Filter.Current.ProjectTaskID;
			}
		}

		public virtual void _(Events.FieldSelecting<EPActivityApprove.hold> e)
		{
			EPActivityApprove row = (EPActivityApprove)e.Row;
			if (row != null)
			{
				e.ReturnValue = row.ApprovalStatus == ActivityStatusListAttribute.Open;
			}
		}

		public virtual void _(Events.RowInserted<EPWeeklyCrewTimeActivity> e)
		{
			if (Document.Current.WorkgroupID != null)
			{
				var lastDocument = SelectFrom<EPWeeklyCrewTimeActivity>.Where<EPWeeklyCrewTimeActivity.workgroupID.IsEqual<EPWeeklyCrewTimeActivity.workgroupID.FromCurrent>>
					.AggregateTo<Max<EPWeeklyCrewTimeActivity.week>>.View.Select(this).TopFirst;
				if (lastDocument?.Week != null)
				{
					e.Row.Week = PXWeekSelector2Attribute.GetNextWeekID(this, lastDocument.Week.Value);
				}
			}
		}

		public virtual void _(Events.RowSelected<EPTimeActivitiesSummary> e)
		{
			if (e.Row != null)
			{
				PXUIFieldAttribute.SetWarning<EPTimeActivitiesSummary.status>(e.Cache, e.Row, e.Row.IsMemberActive == false && e.Row.IsWithoutActivities == false ? Messages.InactiveMemberTimeEntry : null);
				PXUIFieldAttribute.SetWarning<EPTimeActivitiesSummary.totalRegularTime>(e.Cache, e.Row, e.Row.IsWithoutActivities == true ? Messages.WithoutActivities : null);
			}
		}

		#endregion Events

		#region Helpers

		private EPActivityApprove CopyActivity(EPActivityApprove activity)
		{
			var copy = TimeActivities.Cache.CreateCopy(activity) as EPActivityApprove;
			copy.NoteID = null;
			return copy;
		}

		private int GetLastWeekID()
		{
			DateTime startDate = PXWeekSelector2Attribute.GetWeekStartDate(this, Document.Current.Week.Value);
			return PXWeekSelector2Attribute.GetWeekID(this, startDate.AddDays(-1));
		}

		#endregion Helpers

		#region PXImportAttribute.IPXPrepareItems Implementation

		public virtual bool PrepareImportRow(string viewName, IDictionary keys, IDictionary values)
		{
			if (viewName == nameof(TimeActivities) && values[nameof(PMTimeActivity.OwnerID)] != null)
			{
				Contact contact = SelectFrom<Contact>.Where<Contact.displayName.IsEqual<P.AsString>>.View.Select(this, values[nameof(PMTimeActivity.OwnerID)].ToString()).TopFirst;
				values[nameof(PMTimeActivity.OwnerID)] = contact.ContactID;
			}

			return true;
		}

		public virtual bool RowImporting(string viewName, object row)
		{
			return true;
		}

		public virtual bool RowImported(string viewName, object row, object oldRow)
		{
			return true;
		}

		public virtual void PrepareItems(string viewName, IEnumerable items)
		{
			return;
		}

		#endregion PXImportAttribute.IPXPrepareItems Implementation
	}

	/// <summary>
	/// Required to duplicate EPActivityApprove cache for Bulk Time Entry popup.
	/// </summary>
	[PXHidden]
	public class EPActivityApprove2 : EPActivityApprove
	{
		#region NoteID
		public new abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
		[PXDBGuid(true, IsKey = true)]
		public override Guid? NoteID { get; set; }
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
		[PXDefault(typeof(Search<PMProject.contractID, Where<PMProject.nonProject, Equal<True>>>))]
		public override Int32? ProjectID { get; set; }
		#endregion

		#region Summary
		public abstract class summary : PX.Data.BQL.BqlString.Field<summary> { }

		[PXDBString(Common.Constants.TranDescLength, InputMask = "", IsUnicode = true)]
		[PXDefault]
		[PXUIField(DisplayName = "Summary", Visibility = PXUIVisibility.SelectorVisible)]
		[PXFieldDescription]
		public override string Summary { get; set; }
		#endregion
	}
}