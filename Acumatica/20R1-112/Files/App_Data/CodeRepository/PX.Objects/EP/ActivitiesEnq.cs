using System.Collections;
using PX.Data;
using PX.Objects.CR;
using PX.SM;
using PX.TM;

namespace PX.Objects.EP
{
	[DashboardType((int)DashboardTypeAttribute.Type.Default, GL.TableAndChartDashboardTypeAttribute._AMCHARTS_DASHBOART_TYPE)]
	public class ActivitiesEnq : PXGraph<ActivitiesEnq>
	{
		#region Selects
		[PXHidden]
		public PXSelect<BAccount> _baccount;
		[PXHidden]
		public PXSelect<AR.Customer> _customer;
		[PXHidden]
		public PXSelect<AP.Vendor> _vendor;

		[PXViewName(Messages.Activities)]
		[PXFilterable]
		[PXViewDetailsButton(typeof(CR.OwnedFilter))]
		public PXSelectReadonly<CRPMTimeActivity,
		Where<CRPMTimeActivity.classID, Equal<CRActivityClass.activity>, And<
			Where<CRPMTimeActivity.workgroupID, Owned<CurrentValue<AccessInfo.userID>>,
				Or2<Where<CRPMTimeActivity.workgroupID, IsNull, And<CRPMTimeActivity.ownerID, OwnedUser<CurrentValue<AccessInfo.userID>>>>,
				Or<CRPMTimeActivity.ownerID, Equal<Current<AccessInfo.userID>>,
				Or<CRPMTimeActivity.createdByID, Equal<Current<AccessInfo.userID>>>>>>>>,
		OrderBy<Desc<CRPMTimeActivity.endDate,
			Desc<CRPMTimeActivity.priority,
			Desc<CRPMTimeActivity.startDate>>>>>
			Activities;

		public PXSetup<EPSetup> epsetup; 
		#endregion

		#region Ctors

		public ActivitiesEnq()
		{
			PXUIFieldAttribute.SetDisplayName<CRPMTimeActivity.startDate>(Activities.Cache, Messages.Date);
			PXUIFieldAttribute.SetDisplayName<CRPMTimeActivity.endDate>(Activities.Cache, Messages.CompletedAt);

			var activityCache = Caches[typeof(CRPMTimeActivity)];
			PXUIFieldAttribute.SetVisible(activityCache, null, false);
			PXUIFieldAttribute.SetVisible<CRPMTimeActivity.subject>(activityCache, null);
			PXUIFieldAttribute.SetVisible<CRPMTimeActivity.uistatus>(activityCache, null);
			PXUIFieldAttribute.SetVisible<CRPMTimeActivity.startDate>(activityCache, null);
			PXUIFieldAttribute.SetVisible<CRPMTimeActivity.timeSpent>(activityCache, null);
			PXUIFieldAttribute.SetVisible<CRPMTimeActivity.overtimeSpent>(activityCache, null);
			PXUIFieldAttribute.SetVisible<CRPMTimeActivity.source>(activityCache, null);

			var isPmVisible = PM.ProjectAttribute.IsPMVisible( GL.BatchModule.TA);
			PXUIFieldAttribute.SetVisible<CRPMTimeActivity.projectID>(Activities.Cache, null, isPmVisible);
			PXUIFieldAttribute.SetVisible<CRPMTimeActivity.projectTaskID>(Activities.Cache, null, isPmVisible);
		}
		
		#endregion
		
		#region Actions

		public PXAction<CRPMTimeActivity> CreateNew;
		[PXUIField(DisplayName = "")]
		[PXInsertButton(Tooltip = Messages.AddActivityTooltip)]
		protected virtual IEnumerable createNew(PXAdapter adapter)
		{
			EPSetup setup = epsetup.Current;
			new ActivityService().CreateActivity(null, setup.DefaultActivityType, PXRedirectHelper.WindowMode.InlineWindow);
			return adapter.Get();
		}


		public PXAction<CRPMTimeActivity> ViewOwner;
		[PXUIField(DisplayName = Messages.ViewOwner, Visible = false)]
		[PXLookupButton(Tooltip = Messages.ttipViewOwner)]
		protected virtual IEnumerable viewOwner(PXAdapter adapter)
		{
			var current = Activities.Current;
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

		public PXAction<CRPMTimeActivity> ViewEntity;
		[PXUIField(DisplayName = Messages.ViewEntity, Visible = false)]
		[PXLookupButton(Tooltip = Messages.ttipViewEntity, OnClosingPopup = PXSpecialButtonType.Refresh)]
		protected virtual void viewEntity()
		{
            var row = Activities.Current;
            if (row == null) return;

            new EntityHelper(this).NavigateToRow(row.RefNoteID, PXRedirectHelper.WindowMode.New);
		}

        public PXAction<CRPMTimeActivity> ViewActivity;
        [PXUIField(DisplayName = Messages.ViewEntity, Visible = false)]
        protected virtual IEnumerable viewActivity(PXAdapter adapter)
        {
            var row = Activities.Current;
            if (row != null)
            {
                var graph = PXGraph.CreateInstance <CRActivityMaint>();
                graph.Activities.Current = graph.Activities.Search<CRPMTimeActivity.noteID>(row.NoteID);
                if (graph.Activities.Current != null)
                    PXRedirectHelper.TryRedirect(graph, PXRedirectHelper.WindowMode.InlineWindow);
            }
            return adapter.Get();
        }

		public PXAction<CRPMTimeActivity> DoubleClick;
		[PXUIField(DisplayName = "", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = false)]
		public virtual IEnumerable doubleClick(PXAdapter adapter)
		{
			return viewActivity(adapter);
		}

		#endregion

		#region Event Handlers

		[PXUIField(DisplayName = "Class", Visible = false)]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected virtual void CRPMTimeActivity_ClassID_CacheAttached(PXCache sender) { }
		
		[PXUIField(DisplayName = "Type", Required = true, Visible = false)]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected virtual void CRPMTimeActivity_Type_CacheAttached(PXCache sender) { }

		[PM.Project(FieldClass = PM.ProjectAttribute.DimensionName, BqlField = typeof(PMTimeActivity.projectID))]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		public virtual void CRPMTimeActivity_ProjectID_CacheAttached(PXCache sender) { }

		#endregion
	}
}
