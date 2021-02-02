using System;
using PX.Common;
using PX.Data;
using PX.Data.EP;
using PX.Objects.CT;
using PX.Objects.EP;

namespace PX.Objects.CR
{
	#region CREmailCaseActivityMaint

	[PXGraphName(Messages.CREmailCaseActivityMaint, typeof(CRActivity))]
	public class CREmailCaseActivityMaint
		: CREmailActivityMaintBase<CRActivityMaint, EPActivity, EPActivity.taskID, EPActivity.refNoteID, EPActivity.type>
	{
		public CREmailCaseActivityMaint()
				{
			CRCaseActivityHelper<EPActivity.refNoteID, EPActivity.owner>.Attach(this);
		}

		protected override int? ClassID
		{
			get { return CRActivityClass.CRCaseActivity; }
		}

		protected override void CorrectAddresses(EPActivity message)
		{
			base.CorrectAddresses(message);

			PX.Common.Mail.Mailbox mailFrom;
			if (message != null && !string.IsNullOrEmpty(message.MailFrom) &&
				PX.Common.Mail.Mailbox.TryParse(message.MailFrom, out mailFrom))
			{
				var setup = PXSelectReadonly<CRSetup>.Select(this);
				if (setup == null || setup.Count == 0 ||
					((CRSetup)setup[0][typeof(CRSetup)]).ShowAuthorForCaseEmail != true)
				{
					message.MailFrom = PX.Common.Mail.Mailbox.Create(mailFrom.DisplayName, message.MailAccount);
					Message.Cache.Update(message);
				}
			}
		}
	}

	#endregion

	#region CRCaseActivityMaint

	[PXGraphName(Messages.CRCaseActivityMaint, typeof(EPActivity))]
	public class CRCaseActivityMaint : CRActivityMaintBase<CRCaseActivityMaint, EPActivity>
	{
		public CRCaseActivityMaint()
		{
			CRCaseActivityHelper<EPActivity.refNoteID, EPActivity.owner>.Attach(this);
		}

		protected override int? ClassID
		{
			get { return CRActivityClass.CRCaseActivity; }
		}
	}

	#endregion

	#region CRCaseActivityHelper<TableRefNoteID, TableOwnerID>

	public class CRCaseActivityHelper<TableRefNoteID, TableOwnerID> 
		where TableRefNoteID : IBqlField
		where TableOwnerID : IBqlField
	{
		protected readonly PXGraph _Graph;

		public readonly PXSelectBase<CRCase> CurrentCase;

		public readonly PXSelectBase<Contract> CurrentContract;

		public readonly PXSelectBase<CRCaseClass> CurrentCaseClass;

		public readonly PXSelectBase<EPEmployee> CurrentEmployer;

		public readonly PXSelectBase<Location> CurrentCustomerLocation;

		#region Ctor

		protected CRCaseActivityHelper(PXGraph graph)
		{
			_Graph = graph;

			CurrentCase = new PXSetup<CRCase, 
				Where<CRCase.noteID, Equal<Current<TableRefNoteID>>>>(graph);

			CurrentContract = new PXSetup<Contract, 
				Where<Contract.contractID, Equal<Current<CRCase.contractID>>>>(graph);

			CurrentCaseClass = new PXSetup<CRCaseClass, 
				Where<CRCaseClass.caseClassID, Equal<Current<CRCase.caseClassID>>>>(graph);

			CurrentEmployer = new PXSetup<EPEmployee,
				Where<EPEmployee.userID, Equal<Current<TableOwnerID>>>>(graph);

			CurrentCustomerLocation = new PXSetup<Location,
				Where<Location.bAccountID, Equal<Current<CRCase.customerID>>,
					And<Location.locationID, Equal<Current<CRCase.locationID>>>>>(graph);
		}

		public static CRCaseActivityHelper<TableRefNoteID, TableOwnerID> Attach(PXGraph graph)
		{
			var res = new CRCaseActivityHelper<TableRefNoteID, TableOwnerID>(graph);

			graph.RowInserting.AddHandler<EPActivity>(res.ActivityRowInserting);
			graph.RowInserted.AddHandler<EPActivity>(res.ActivityRowInserted);
			graph.RowSelected.AddHandler<EPActivity>(res.ActivityRowSelected);
			graph.RowUpdated.AddHandler<EPActivity>(res.ActivityRowUpdated);

			return res;
		}
		#endregion

		#region Event Handlers

		protected virtual void ActivityRowInserting(PXCache sender, PXRowInsertingEventArgs e)
		{
			var item = e.Row as EPActivity;
			if (item == null) return;

			item.RefNoteID = (long?)GetCurrent<TableRefNoteID>();
			item.Owner = (Guid?)GetCurrent<TableOwnerID>();

			if (CurrentCaseClass.Current != null && CurrentCaseClass.Current.IsBillable == true)
				item.IsBillable = true;

			var allowChangeBillableTime = CurrentCase.Current == null || CurrentCase.Current.Released != true;
			if (!allowChangeBillableTime) item.IsBillable = false;
		}

		protected virtual void ActivityRowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			var item = e.Row as EPActivity;
			if (item == null) return;

			SetOvertimeSpent(item);
			SetTimeBillable(item);
		}

		private object GetCurrent<TField>()
		{
			var table = typeof(TField).DeclaringType;
			var cache = _Graph.Caches[table];
			var row = cache.Current;
			var field = cache.GetField(typeof(TField));
			return cache.GetValue(row, field);
		}

		protected virtual void ActivityRowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			var item = e.Row as EPActivity;
			if (item == null) return;

			var allowOverrideBillable = CurrentCaseClass.Current == null || CurrentCaseClass.Current.AllowOverrideBillable == true;
			var allowChangeBillableTime = CurrentCase.Current == null || CurrentCase.Current.Released != true;
			PXUIFieldAttribute.SetEnabled<EPActivity.isBillable>(sender, item, allowChangeBillableTime);
			PXUIFieldAttribute.SetEnabled<EPActivity.timeBillable>(sender, item,
				allowOverrideBillable && allowChangeBillableTime && (item.IsBillable ?? false));
			PXUIFieldAttribute.SetEnabled<EPActivity.overtimeBillable>(sender, item,
				allowOverrideBillable && allowChangeBillableTime && (item.IsBillable ?? false));
			PXUIFieldAttribute.SetEnabled<EPActivity.overtimeSpent>(sender, item, false);

			if (item.Billed == true) PXUIFieldAttribute.SetEnabled(sender, item, false);
		}

		protected virtual void ActivityRowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			var item = e.Row as EPActivity;
			var oldItem = e.OldRow as EPActivity;
			if (item == null || oldItem == null) return;

			SetOvertimeSpent((EPActivity)e.Row);

			if (item.TimeBillable == oldItem.TimeBillable && item.OvertimeBillable == oldItem.OvertimeBillable && 
				(item.StartDate != oldItem.StartDate || item.IsBillable != oldItem.IsBillable || 
				item.TimeSpent != oldItem.TimeSpent || item.OvertimeSpent != oldItem.OvertimeSpent))
			{
				SetTimeBillable((EPActivity)e.Row);
			}

			if (item.TimeBillable > item.TimeSpent)
				sender.RaiseExceptionHandling<EPActivity.timeBillable>(e.Row, item.TimeBillable,
					new PXSetPropertyException(Messages.BillableTimeCannotBeGreaterThanTimeSpent));
			if (item.OvertimeBillable > item.OvertimeSpent)
				sender.RaiseExceptionHandling<EPActivity.overtimeBillable>(e.Row, item.OvertimeBillable,
					new PXSetPropertyException(Messages.OvertimeBillableCannotBeGreaterThanOvertimeSpent));
		}

		#endregion

		#region Public Methods

		public static void SetTimeBillable(EPActivity item)
		{
			if (item == null || item.Billed == true) return;

			if (item.IsBillable == true)
			{
				item.TimeBillable = item.TimeSpent != null && item.TimeSpent >= 0 ? item.TimeSpent : 0;
				item.OvertimeBillable = item.OvertimeSpent != null && item.OvertimeSpent >= 0 ? item.OvertimeSpent : 0;
			}
			else
			{
				item.TimeBillable = null;
				item.OvertimeBillable = null;
			}
		}

		#endregion

		#region Protected Methods

		protected virtual void SetOvertimeSpent(EPActivity item)
		{
			if (item == null) return;

			if (item.EndDate == null || item.StartDate == null)
				item.OvertimeSpent = 0;
			else
			try
			{
				var overtimespent = CalculateOvertime((DateTime)item.StartDate, (DateTime)item.EndDate);
				item.OvertimeSpent = (int)overtimespent.TotalMinutes;
			}
			catch (ArgumentOutOfRangeException)
			{
					item.OvertimeSpent = 0;
			}
		}

		protected virtual TimeSpan CalculateOvertime(DateTime start, DateTime end)
		{
			var calendarId = CalendarID;
			return calendarId == null ? new TimeSpan() : CalendarHelper.CalculateOvertime(_Graph, start, end, calendarId);
		}

		protected virtual string CalendarID
		{
			get
			{
				if (CurrentContract.Current != null && !string.IsNullOrEmpty(CurrentContract.Current.CalendarID))
					return CurrentContract.Current.CalendarID;
				if (CurrentCaseClass.Current != null && !string.IsNullOrEmpty(CurrentCaseClass.Current.CalendarID))
					return CurrentCaseClass.Current.CalendarID;
				if (CurrentEmployer.Current != null && !string.IsNullOrEmpty(CurrentEmployer.Current.CalendarID))
					return CurrentEmployer.Current.CalendarID;
				if (CurrentCustomerLocation.Current != null && !string.IsNullOrEmpty(CurrentCustomerLocation.Current.CCalendarID))
					return CurrentCustomerLocation.Current.CCalendarID;
				return null;
			}
		}

		#endregion
	}

	#endregion
}
