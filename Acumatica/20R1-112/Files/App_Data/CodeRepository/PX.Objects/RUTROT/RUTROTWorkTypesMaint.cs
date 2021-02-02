using System;
using System.Collections;
using System.Linq;
using PX.Data;
using PX.Objects.AR;
using PX.Objects.IN;
using PX.Objects.SO;
using PX.Web.UI;

namespace PX.Objects.RUTROT
{
	public class RUTROTWorkTypesMaint : PXGraph<RUTROTWorkTypesMaint>
	{
		[Serializable]
		public class Filter : IBqlTable
		{
			#region RUTROTType
			public abstract class rUTROTType : PX.Data.BQL.BqlString.Field<rUTROTType> { }
			[PXString(1)]
			[RUTROTTypes.List]
			[PXDefault(RUTROTTypes.RUT)]
			[PXUIField(DisplayName = "Type")]
			public virtual string RUTROTType
			{
				get;
				set;
			}
			#endregion
		}

		public PXSelect<RUTROTSetup> RUTROTSetupView;
		public PXFilter<Filter> FilterView;
		[PXImport(typeof(RUTROTSetup))]
		public PXSelect<RUTROTWorkType, 
			Where<RUTROTWorkType.rUTROTType, Equal<Current<Filter.rUTROTType>>>, 
			OrderBy<Asc<RUTROTWorkType.position>>> WorkTypes;

		#region Buttons
		public PXSave<RUTROTSetup> Save;
		public PXCancel<RUTROTSetup> Cancel;
		public PXAction<RUTROTSetup> MoveDown;
		[PXButton]
		[PXUIField(DisplayName = ActionsMessages.RowDown, MapEnableRights = PXCacheRights.Update, Visible = true)]
		public IEnumerable moveDown(PXAdapter adapter)
		{
			RUTROTWorkType currentRow = WorkTypes.Current;
			if (currentRow != null)
			{
				RUTROTWorkType nextRow = PXSelect<RUTROTWorkType,
					Where<RUTROTWorkType.rUTROTType, Equal<Required<RUTROTWorkType.rUTROTType>>,
					And<RUTROTWorkType.position, Greater<Required<RUTROTWorkType.position>>>>,
					OrderBy<Asc<RUTROTWorkType.position>>>.SelectWindowed(this, 0, 1, currentRow.RUTROTType, currentRow.Position);
				if (nextRow != null)
				{
					int? tempPosition = nextRow.Position;
					nextRow.Position = null;
					WorkTypes.Update(nextRow);
					nextRow.Position = currentRow.Position;
					currentRow.Position = tempPosition;
					currentRow = WorkTypes.Update(currentRow);
					WorkTypes.Update(nextRow);
					WorkTypes.Cache.ActiveRow = currentRow;
				}
			}
			return adapter.Get();
		}

		public PXAction<RUTROTSetup> MoveUp;
		[PXButton]
		[PXUIField(DisplayName = ActionsMessages.RowUp, MapEnableRights = PXCacheRights.Update, Visible = true)]
		public IEnumerable moveUp(PXAdapter adapter)
		{
			RUTROTWorkType currentRow = WorkTypes.Current;
			if (currentRow != null)
			{
				RUTROTWorkType prevRow = PXSelect<RUTROTWorkType,
					Where<RUTROTWorkType.rUTROTType, Equal<Required<RUTROTWorkType.rUTROTType>>,
					And<RUTROTWorkType.position, Less<Required<RUTROTWorkType.position>>>>,
					OrderBy<Desc<RUTROTWorkType.position>>>.SelectWindowed(this, 0, 1, currentRow.RUTROTType, currentRow.Position);
				if (prevRow != null)
				{
					int? tempPosition = prevRow.Position;
					prevRow.Position = null;
					WorkTypes.Update(prevRow);
					prevRow.Position = currentRow.Position;
					currentRow.Position = tempPosition;
					currentRow = WorkTypes.Update(currentRow);
					WorkTypes.Update(prevRow);
					WorkTypes.Cache.ActiveRow = currentRow;
				}
			}
			return adapter.Get();
		}
		#endregion

		protected virtual IEnumerable workTypes()
		{
			return PXSelect<RUTROTWorkType, 
				Where<RUTROTWorkType.rUTROTType, Equal<Current<Filter.rUTROTType>>>, 
				OrderBy<Asc<RUTROTWorkType.position>>>.Select(this);
		}

		#region Events

		protected virtual void RUTROTWorkType_StartDate_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			RUTROTWorkType row = (RUTROTWorkType)e.Row;
			if (row == null || row.EndDate == null)
			{
				return;
			}
			DateTime? startDate = (DateTime?)e.NewValue;
			if (startDate == null)
			{
				return;
			}
			if (startDate > row.EndDate)
			{
				throw new PXException(CR.Messages.EndDateLessThanStartDate);
			}
		}

		protected virtual void RUTROTWorkType_EndDate_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			RUTROTWorkType row = (RUTROTWorkType)e.Row;
			if (row == null || row.StartDate == null)
			{
				return;
			}
			DateTime? endDate = (DateTime?)e.NewValue;
			if (endDate == null)
			{
				return;
			}
			if (row.StartDate > endDate)
			{
				throw new PXException(CR.Messages.EndDateLessThanStartDate);
			}
		}

		protected virtual void RUTROTWorkType_Position_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			RUTROTWorkType row = (RUTROTWorkType)e.Row;
			if (row == null)
			{
				return;
			}
			RUTROTWorkType latest = PXSelectOrderBy<RUTROTWorkType, OrderBy<Desc<RUTROTWorkType.position>>>.SelectWindowed(this, 0, 1);
			e.NewValue = latest.Position + 1;
		}

		protected virtual void RUTROTWorkType_RUTROTType_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (!string.IsNullOrEmpty(FilterView.Current?.RUTROTType))
			{
				e.NewValue = FilterView.Current.RUTROTType;
			}
		}

		protected virtual void RUTROTWorkType_RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
		{
			RUTROTWorkType workType = (RUTROTWorkType)e.Row;
			if (workType != null && workType.WorkTypeID != null)
			{
				if (PXSelect<ARTran, 
					Where<ARTranRUTROT.rUTROTWorkTypeID, Equal<Required<RUTROTWorkType.workTypeID>>>>.Select(this, workType.WorkTypeID).Any())
				{
					throw new PXException(RUTROTMessages.CannotDeleteWorkType);
				}

				if (PXSelect<SOLine, 
					Where<SOLineRUTROT.rUTROTWorkTypeID, Equal<Required<RUTROTWorkType.workTypeID>>>>.Select(this, workType.WorkTypeID).Any())
				{
					throw new PXException(RUTROTMessages.CannotDeleteWorkType);
				}

				if (PXSelect<InventoryItem, 
					Where<InventoryItemRUTROT.rUTROTWorkTypeID, Equal<Required<RUTROTWorkType.workTypeID>>>>.Select(this, workType.WorkTypeID).Any())
				{
					throw new PXException(RUTROTMessages.CannotDeleteWorkType);
				}
			}
		}
		#endregion
	}
}
