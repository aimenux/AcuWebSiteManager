using PX.Data;
namespace PX.Objects.PR
{
	public class PRPayGroupMaint : PXGraph<PRPayGroupMaint>
	{
		public PXSavePerRow<PRPayGroup> Save;
		public PXCancel<PRPayGroup> Cancel;

		public PXSelect<PRPayGroup> PayGroup;

		#region Data Members for deleting by PXParentAttribute
		public PXSelect<PRPayGroupYearSetup> YearSetup;
		public PXSelect<PRPayGroupYear> Years;
		public PXSelect<PRPayGroupPeriodSetup> PeriodSetup;
		#endregion

		#region Calendar Button

		public PXAction<PRPayGroup> ShowCalendar;
		[PXUIField(DisplayName = FA.Messages.Calendar, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		protected virtual void showCalendar()
		{
			if (string.IsNullOrWhiteSpace(PayGroup.Current?.PayGroupID))
				return;

			PRPayGroupYearSetupMaint graph = CreateInstance<PRPayGroupYearSetupMaint>();
			graph.FiscalYearSetup.Current = graph.FiscalYearSetup.Search<PRPayGroupYearSetup.payGroupID>(PayGroup.Current.PayGroupID);
			if (graph.FiscalYearSetup.Current == null)
			{
				PRPayGroupYearSetup calendar = new PRPayGroupYearSetup { PayGroupID = PayGroup.Current.PayGroupID };
				graph.FiscalYearSetup.Cache.SetDefaultExt<PRPayGroupYearSetup.periodType>(calendar);
				graph.FiscalYearSetup.Cache.Insert(calendar);
				graph.FiscalYearSetup.Cache.IsDirty = false;
			}
			throw new PXRedirectRequiredException(graph, FA.Messages.Calendar);
		}

		#endregion

		#region PayGroup Event Handlers

		protected virtual void PRPayGroup_IsDefault_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			PRPayGroup payGroup = (PRPayGroup)e.Row;
			if (payGroup == null || !(payGroup.IsDefault ?? false)) return;

			foreach (PRPayGroup other in PXSelect<PRPayGroup, Where<PRPayGroup.payGroupID, NotEqual<Current<PRPayGroup.payGroupID>>>>.SelectMultiBound(this, new object[] { payGroup }))
			{
				other.IsDefault = false;
				PayGroup.Update(other);
			}
			PayGroup.View.RequestRefresh();
		}

		#endregion
	}
}