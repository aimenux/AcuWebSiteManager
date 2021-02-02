using System;
using System.Collections;
using PX.Data;
using PX.Data.EP;
using PX.Objects.AR;
using PX.Objects.CR;
using PX.Objects.GL;
using PX.SM;
using PX.TM;

namespace PX.Objects.EP
{
	[TableDashboardType]
	[EPCalendarDashboardGraph(typeof(EPCalendarEnq), "Filter", "Events")]
	public class EPEventEnq : PXGraph<EPEventEnq>
    {

        #region EventFilter

        [Serializable]
		[PXHidden]
		public partial class EventFilter : IBqlTable
        {
            #region Current Owner

            [PXDBGuid]
		    [CR.CRCurrentOwnerID]
		    public virtual Guid? CurrentOwnerID { get; set; }

		    #endregion

            #region OwnerID

            public abstract class ownerID : PX.Data.BQL.BqlGuid.Field<ownerID> { }
            protected Guid? _OwnerID;
            [PXDBGuid]
            [PXUIField(DisplayName = "Employee")]
            [PXSubordinateOwnerSelector]
            public virtual Guid? OwnerID
            {
                get {  return _OwnerID ?? CurrentOwnerID;  }
                set { _OwnerID = value; }
            }

            #endregion

        }

        #endregion

        #region Ctor
	    public EPEventEnq()
	    {           
	    }
        #endregion
        #region Selects

	    public PXSelect<BAccount> BAccountSelect;
	    public PXSelect<Customer> CustomerSelect;
	    public PXSelect<EPEmployee> EmployeeSelect;

        public PXFilter<EventFilter> Filter;

		[PXFilterable]
		[PXViewDetailsButton(typeof(EventFilter), OnClosingPopup = PXSpecialButtonType.Refresh)]
		public PXSelectReadonly3<CRActivity,
			LeftJoin<CRReminder,
				On<CRReminder.refNoteID, Equal<CRActivity.noteID>>>,
			OrderBy<Asc<CRActivity.startDate,
				Desc<CRActivity.priority>>>>
			Events;
		#endregion
		
		#region Actions

		public PXCancel<EventFilter> Cancel;

        public PXAction<EventFilter> AddNew;
		[PXUIField(DisplayName = "")]
        [PXInsertButton(Tooltip = Messages.AddEvent)]
        public virtual void addNew()
		{
			EPEventMaint graph = CreateInstance<EPEventMaint>();
			graph.Events.Insert();
		    graph.Events.Cache.IsDirty = false;
            PXRedirectHelper.TryRedirect(graph, PXRedirectHelper.WindowMode.InlineWindow);
		}

		public PXAction<EventFilter> DoubleClick;
		[PXUIField(DisplayName = "", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = false)]
		public virtual IEnumerable doubleClick(PXAdapter adapter)
		{
			return viewEvent(adapter);
		}

		public PXAction<EventFilter> EditDetail;
        [PXUIField(DisplayName = "")]
        [PXEditDetailButton(OnClosingPopup = PXSpecialButtonType.Refresh)]
        public virtual void editDetail()
        {
            var row = Events.Current;
            if (row == null) return;

            var graph = PXGraph.CreateInstance<EPEventMaint>();
            graph.Events.Current = row;
            PXRedirectHelper.TryRedirect(graph, PXRedirectHelper.WindowMode.InlineWindow);
        }

		public PXAction<EventFilter> ViewEntity;
		[PXUIField(DisplayName = Messages.ViewEntity, Visible = false)]
		[PXLookupButton(Tooltip = Messages.ttipViewEntity)]
		protected void viewEntity()
		{
			var row = Events.Current;
			if (row == null) return;

			new EntityHelper(this).NavigateToRow(row.RefNoteID, PXRedirectHelper.WindowMode.New);
		}

		public PXAction<EventFilter> ViewOwner;
		[PXUIField(DisplayName = Messages.ViewOwner, Visible = false)]
		[PXLookupButton(Tooltip = Messages.ttipViewOwner)]
		protected virtual IEnumerable viewOwner(PXAdapter adapter)
		{
			var current = Events.Current;
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

		public PXAction<EventFilter> ViewEvent;

		[PXUIField(DisplayName = Messages.Event, Visible = false)]
		public virtual IEnumerable viewEvent(PXAdapter adapter)
		{
			if (this.Events.Current != null)
			{
				var evnt = this.Events.Current;
				this.Persist();
				EPEventMaint graph = PXGraph.CreateInstance<PX.Objects.EP.EPEventMaint>();

				graph.Events.Current = (CRActivity)PXSelect<CRActivity,
					Where<CRActivity.noteID, Equal<Required<CRActivity.noteID>>>>.
					SelectSingleBound(this, null, evnt.NoteID);

				if (graph.Events.Current != null)
					PXRedirectHelper.TryRedirect(graph, PXRedirectHelper.WindowMode.InlineWindow);
			}
			return adapter.Get();
		}

		public PXAction<EventFilter> Complete;
		[PXUIField(DisplayName = Messages.CompleteEvent)]
		[PXButton(Tooltip = Messages.CompleteEvent)]
		protected virtual void complete()
		{
			CRActivity row = Events.Current;
			if (row == null) return;

			if(row.OwnerID != Accessinfo.UserID)
				throw new PXException(Messages.EventNonOwned, row.Subject);
			if (row.UIStatus == ActivityStatusListAttribute.Completed || row.UIStatus == ActivityStatusListAttribute.Canceled)
				throw new PXException(Messages.EventInStatus, row.Subject, new ActivityStatusListAttribute().ValueLabelDic[row.UIStatus]);

			PXLongOperation.StartOperation(this, delegate
			{
				EPEventMaint graph = CreateInstance<EPEventMaint>();
				graph.Events.Current = graph.Events.Search<CRActivity.noteID>(row.NoteID);
				graph.Complete.Press();
			});
		}

		public PXAction<EventFilter> CancelActivity;
		[PXUIField(DisplayName = Messages.CancelEvent)]
		[PXButton(Tooltip = Messages.CancelEvent)]
		protected virtual void cancelActivity()
		{
			CRActivity row = Events.Current;
			if (row == null) return;

			if (row.OwnerID != Accessinfo.UserID)
				throw new PXException(Messages.EventNonOwned, row.Subject);
			if (row.UIStatus == ActivityStatusListAttribute.Completed || row.UIStatus == ActivityStatusListAttribute.Canceled)
				throw new PXException(Messages.EventInStatus, row.Subject, new ActivityStatusListAttribute().ValueLabelDic[row.UIStatus]);

			PXLongOperation.StartOperation(this, delegate
			{
				EPEventMaint graph = CreateInstance<EPEventMaint>();
				graph.Events.Current = graph.Events.Search<CRActivity.noteID>(row.NoteID);
				graph.CancelActivity.Press();
			});
		}

		//TODO: need implementation
		/*public PXAction<EventFilter> ExportCalendar;
		[PXUIField(DisplayName = Messages.ExportCalendar)]
		[PXButton(Tooltip = Messages.ExportCalendarTooltip)]
		public IEnumerable exportCalendar(PXAdapter adapter)
		{
			var events = Events.Select().RowCast<CRActivity>();
			var calendar = (vCalendarIcs)VCalendarProcessor.CreateVCalendar(events);
			throw new EPIcsExportRedirectException(calendar);
		}

		public PXAction<EventFilter> ExportCard;
		[PXUIField(DisplayName = Messages.ExportCard)]
		[PXButton(Tooltip = Messages.ExportCardTooltip)]
		public IEnumerable exportCard(PXAdapter adapter)
		{
			if (Events.Current == null) return adapter.Get();
			var card = VCalendarProcessor.CreateVEvent(Events.Current);
			throw new EPIcsExportRedirectException(card);
		}*/

		#endregion

		#region Data Handlers
		protected virtual IEnumerable events()
		{
			BqlCommand command = new Select2<CRActivity,
				LeftJoin<EPAttendee, On<EPAttendee.userID, Equal<Current<AccessInfo.userID>>,
					And<EPAttendee.eventNoteID, Equal<CRActivity.noteID>>>,
				LeftJoin<EPView,
					On<EPView.noteID, Equal<CRActivity.noteID>,
						And<EPView.userID, Equal<Current<AccessInfo.userID>>>>,
				LeftJoin<CRReminder,
					On<CRReminder.refNoteID, Equal<CRActivity.noteID>>>>>,
				Where<CRActivity.classID, Equal<CRActivityClass.events>>,
				OrderBy<Desc<CRActivity.priority, Asc<CRActivity.startDate, Asc<CRActivity.endDate>>>>>();

			var filter = Filter.Current;
			if (filter.OwnerID != null)
			{
				command = new Select2<CRActivity,
				LeftJoin<EPAttendee, On<EPAttendee.userID, Equal<CurrentValue<EventFilter.ownerID>>,
					And<EPAttendee.eventNoteID, Equal<CRActivity.noteID>>>,
				LeftJoin<EPView,
					On<EPView.noteID, Equal<CRActivity.noteID>,
						And<EPView.userID, Equal<Current<AccessInfo.userID>>>>,
				LeftJoin<CRReminder,
					On<CRReminder.refNoteID, Equal<CRActivity.noteID>>>>>,
				Where2<Where<CRActivity.ownerID, Equal<CurrentValue<EventFilter.ownerID>>, Or<EPAttendee.userID, IsNotNull>>,
					And<Where<CRActivity.classID, Equal<CRActivityClass.events>>>>,
				OrderBy<Desc<CRActivity.priority, Asc<CRActivity.startDate, Asc<CRActivity.endDate>>>>>();
			}

			var view = new PXView(this, true, command);
			var startRow = PXView.StartRow;
			int totalRows = 0;
			var list = view.Select(PXView.Currents, PXView.Parameters, PXView.Searches, PXView.SortColumns, PXView.Descendings, PXView.Filters,
				ref startRow, PXView.MaximumRows, ref totalRows);
			PXView.StartRow = 0;
			return list;
		}

		public override bool IsDirty
		{
			get
			{
				return false;
			}
		}

		#endregion


		#region public Methods

		public void Import(byte[] content, string fileExtension)
		{
			//TODO: need implementation
			/*vCalendar calendar = new vCalendar();
			using (TextReader reader = new StreamReader(new MemoryStream(content)))
			{
				calendar.Read(reader);
			}
			foreach (vEvent item in calendar.Events)
			{
				var inserted = Events.Insert(CreateEvent(item));
				if (inserted.ClassID == null) inserted.ClassID = 1;
			}
			Save.Press();*/
		}

		/*private static CRActivity CreateEvent(vEvent card)
		{
			CRActivity epEvent = new CRActivity();
			epEvent.Subject = card.Summary;
			epEvent.Body = card.Description;
			epEvent.StartDate = card.StartDate.ToLocalTime();
			epEvent.EndDate = card.EndDate.ToLocalTime();
			//card.Category = "meeting";//epEvent
			epEvent.Location = card.Location;
			return epEvent;
		}*/

		#endregion
	}
}
