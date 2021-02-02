using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using PX.Common;
using PX.CS;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.SM;
using PX.TM;
using PX.Data;
using System.Collections;

namespace PX.Objects.EP
{
	[DashboardType((int)DashboardTypeAttribute.Type.Task, GL.TableAndChartDashboardTypeAttribute._AMCHARTS_DASHBOART_TYPE)]
	public class EPTaskEnq : PXGraph<EPTaskEnq>
	{
		#region TaskFilter

		[Serializable]
		public partial class TaskFilter : PX.Objects.CR.OwnedFilter
		{
			#region OwnerID

			public new abstract class ownerID : PX.Data.BQL.BqlGuid.Field<ownerID> { }

			#endregion

			#region myOwner
			public new abstract class myOwner : PX.Data.BQL.BqlBool.Field<myOwner> { }

			#endregion

			#region myWorkGroup

			public new abstract class myWorkGroup : PX.Data.BQL.BqlBool.Field<myWorkGroup> { }

			#endregion

			#region workGroupID

			public new abstract class workGroupID : PX.Data.BQL.BqlInt.Field<workGroupID> { }

			#endregion

			#region currentOwnerID

			public new abstract class currentOwnerID : PX.Data.BQL.BqlGuid.Field<currentOwnerID> { }

			#endregion

			#region IsEscalated

			public abstract class isEscalated : PX.Data.BQL.BqlBool.Field<isEscalated> { }
			[PXDefault(false)]
			[PXBool]
			[PXUIField(DisplayName = "Escalated")]
			public virtual Boolean? IsEscalated { get; set; }

			#endregion

			#region IsFollowUp

			public abstract class isFollowUp : PX.Data.BQL.BqlBool.Field<isFollowUp> { }
			[PXDefault(false)]
			[PXBool]
			[PXUIField(DisplayName = "Follow Up")]
			public virtual Boolean? IsFollowUp { get; set; }

			#endregion
		}

		#endregion

		#region MyEPCompanyTree

		[Serializable]
        [PXHidden]
		[PXCacheName(Messages.Company)]
		public partial class MyEPCompanyTree : EPCompanyTree
		{
			public new abstract class workGroupID : PX.Data.BQL.BqlInt.Field<workGroupID> { }

			public new abstract class description : PX.Data.BQL.BqlString.Field<description> { }
		}

		#endregion

		#region Selects

		[PXHidden]
		public PXSelect<BAccount>
			BaseBAccount;

		[PXHidden]
		public PXSelect<BAccount2>
			BAccount2View;

		[PXHidden]
		public PXSelect<EPCompanyTree>
			CompanyTree;

		public PXFilter<TaskFilter>
			Filter;

		[PXFilterable]
        [PXViewDetailsButton(typeof(CRActivity), OnClosingPopup = PXSpecialButtonType.Refresh)]
		public PXSelectReadonly2<CRActivity, 
            LeftJoin<EPView,
					On<EPView.noteID, Equal<CRActivity.noteID>,
				   And<EPView.userID, Equal<Current<AccessInfo.userID>>>>,
			LeftJoin<CRReminder,
					On<CRReminder.refNoteID, Equal<CRActivity.noteID>>>>,
            Where<CRActivity.classID, Equal<CRActivityClass.task>, And<
                Where<CRActivity.workgroupID, Owned<CurrentValue<AccessInfo.userID>>,
                    Or2<Where<CRActivity.workgroupID, IsNull, And<CRActivity.ownerID, OwnedUser<CurrentValue<AccessInfo.userID>>>>,
                    Or<CRActivity.ownerID, Equal<Current<AccessInfo.userID>>,
                    Or<CRActivity.createdByID, Equal<Current<AccessInfo.userID>>>>>>>>, 
			OrderBy<Asc<CRActivity.endDate,
				Desc<CRActivity.priority,
				Asc<CRActivity.startDate>>>>>
			Tasks;

		#endregion

		#region Ctors

		public EPTaskEnq()
			: base()
		{
			PXUIFieldAttribute.SetDisplayName<MyEPCompanyTree.description>(Caches[typeof(MyEPCompanyTree)], Messages.WorkGroup);

			PXUIFieldAttribute.SetDisplayName<CRActivity.noteID>(Tasks.Cache, Messages.NoteID);
			PXUIFieldAttribute.SetDisplayName<CRActivity.endDate>(Tasks.Cache, CR.Messages.DueDate);

			PXUIFieldAttribute.SetVisible<CRActivity.noteID>(Tasks.Cache, null, true);
			PXUIFieldAttribute.SetVisible<CRActivity.subject>(Tasks.Cache, null, true);
			PXUIFieldAttribute.SetVisible<CRActivity.priority>(Tasks.Cache, null, true);
			PXUIFieldAttribute.SetVisible<CRActivity.uistatus>(Tasks.Cache, null, true);
			PXUIFieldAttribute.SetVisible<CRActivity.startDate>(Tasks.Cache, null, true);
			PXUIFieldAttribute.SetVisible<CRActivity.endDate>(Tasks.Cache, null, true);
			PXUIFieldAttribute.SetVisible<CRActivity.categoryID>(Tasks.Cache, null, false);
			PXUIFieldAttribute.SetVisible<CRActivity.percentCompletion>(Tasks.Cache, null, false);
			PXUIFieldAttribute.SetVisible<CRActivity.refNoteID>(Tasks.Cache, null, false);
        }

		#endregion

		#region Data Handlers

		public override bool IsDirty
		{
			get
			{
				return false;
			}
		}

		#endregion

		#region Event Handlers

		[PXDBDate(UseTimeZone = true)]
		[PXFormula(typeof(TimeZoneNow))]
		[PXUIField(DisplayName = "Start Date")]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected virtual void CRActivity_StartDate_CacheAttached(PXCache sender){}

		[PXDBDate(UseTimeZone = true, PreserveTime = false, DisplayMask = "g")]
		[PXUIField(DisplayName = "End Time")]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected virtual void CRActivity_EndDate_CacheAttached(PXCache sender){}

	    #endregion

        #region Actions

        public PXCancel<CRActivity> Cancel;
        
        public PXAction<CRActivity> AddNew;
		[PXUIField(DisplayName = "")]
        [PXInsertButton(Tooltip = Messages.AddTask)]
		public virtual void addNew()
		{
			var graph = PXGraph.CreateInstance<CRTaskMaint>();
			graph.Tasks.Insert();
		    graph.Tasks.Cache.IsDirty = false;
			PXRedirectHelper.TryRedirect(graph, PXRedirectHelper.WindowMode.InlineWindow);
		}

		public PXAction<CRActivity> DoubleClick;
		[PXUIField(DisplayName = "", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = false)]
		public virtual IEnumerable doubleClick(PXAdapter adapter)
		{
			return viewTask(adapter);
		}

		public PXAction<CRActivity> EditDetail;
        [PXUIField(DisplayName = "")]
        [PXEditDetailButton(OnClosingPopup = PXSpecialButtonType.Refresh)]
        public virtual void editDetail()
        {
            var row = Tasks.Current;
            if (row == null) return;

            var graph = PXGraph.CreateInstance<CRTaskMaint>();
            graph.Tasks.Current = row;
            PXRedirectHelper.TryRedirect(graph, PXRedirectHelper.WindowMode.InlineWindow);
        }

        public PXAction<CRActivity> Complete;
        [PXUIField(DisplayName = PX.TM.Messages.CompleteTask)]
        [PXButton(Tooltip = PX.TM.Messages.CompleteTask, OnClosingPopup = PXSpecialButtonType.Refresh)]
        protected virtual void complete()
        {
            var row = Tasks.Current;
            if (row == null) return;
            if (row.UIStatus == ActivityStatusAttribute.Draft)
                throw new PXException(Messages.CompleteAndCancelNotAvailableForTask);
            var graph = PXGraph.CreateInstance<CRTaskMaint>();
            graph.Tasks.Current = graph.Tasks.Search<CRActivity.noteID>(row.NoteID);
            graph.Complete.PressButton();

            Tasks.Cache.ClearQueryCacheObsolete();
        }

        public PXAction<CRActivity> CancelActivity;
		[PXUIField(DisplayName = PX.TM.Messages.CancelTask)]
        [PXButton(Tooltip = PX.TM.Messages.CancelTask, OnClosingPopup = PXSpecialButtonType.Refresh)]
		protected virtual void cancelActivity()
		{
			var row = Tasks.Current;
			if (row == null) return;
            if (row.UIStatus == ActivityStatusAttribute.Draft)
                throw new PXException(Messages.CompleteAndCancelNotAvailableForTask);
			var graph = PXGraph.CreateInstance<CRTaskMaint>();
			graph.Tasks.Current = graph.Tasks.Search<CRActivity.noteID>(row.NoteID);
			graph.CancelActivity.PressButton();

            Tasks.Cache.ClearQueryCacheObsolete();
		}

        public PXAction<CRActivity> ViewEntity;
		[PXUIField(DisplayName = Messages.ViewEntity, Visible = false)]
        [PXLookupButton(Tooltip = Messages.ttipViewEntity, OnClosingPopup = PXSpecialButtonType.Refresh)]
		protected virtual void viewEntity()
		{
			var row = Tasks.Current;
			if (row == null) return;

			new EntityHelper(this).NavigateToRow(row.RefNoteID, PXRedirectHelper.WindowMode.New);
		}

        public PXAction<CRActivity> ViewOwner;
		[PXUIField(DisplayName = Messages.ViewOwner, Visible = false)]
        [PXLookupButton(Tooltip = Messages.ttipViewOwner, OnClosingPopup = PXSpecialButtonType.Refresh)]
		protected virtual IEnumerable viewOwner(PXAdapter adapter)
		{
			var current = Tasks.Current;
			if (current != null && current.OwnerID != null)
			{
				var employee = (EPEmployee)PXSelect<EPEmployee,
					Where<EPEmployee.userID, Equal<Required<EPEmployee.userID>>>>.
					Select(this, current.OwnerID);
				if (employee != null)
					PXRedirectHelper.TryRedirect(this, employee, PXRedirectHelper.WindowMode.NewWindow);

				var user = (Users)PXSelect<Users,
					Where<Users.pKID, Equal<Required<Users.pKID>>>>.
					Select(this, current.OwnerID);
				if (user != null)
					PXRedirectHelper.TryRedirect(this, user, PXRedirectHelper.WindowMode.NewWindow);
			}
			return adapter.Get();
		}

		public PXAction<CRActivity> ViewTask;

		[PXUIField(DisplayName = Messages.Task, Visible = false)]
		public virtual IEnumerable viewTask(PXAdapter adapter)
		{
			editDetail();
			return adapter.Get();
		}
		#endregion
	}
}
