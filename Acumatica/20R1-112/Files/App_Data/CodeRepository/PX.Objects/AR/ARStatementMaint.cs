using System;
using System.Collections;
using System.Linq;

using PX.Data;

using PX.Objects.Common.Aging;

namespace PX.Objects.AR
{
	public class ARStatementMaint : PXGraph<ARStatementMaint, ARStatementCycle>
	{
		public PXSelect<ARStatementCycle> ARStatementCycleRecord;
		public PXAction<ARStatementCycle> RecreateLast;

		[PXUIField(DisplayName = Messages.RegenerateLastStatement, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXProcessButton]
		public virtual IEnumerable recreateLast(PXAdapter adapter)
		{
			ARStatementCycle row = ARStatementCycleRecord.Current;
			if(row != null)
			{
				if (ARStatementProcess.CheckForUnprocessedPPD(this, row.StatementCycleId, row.LastStmtDate, null))
				{
					throw new PXSetPropertyException(Messages.UnprocessedPPDExists);
				}
			
				if (row.LastStmtDate != null)
				{
					PXLongOperation.StartOperation(this, delegate() { StatementCycleProcessBO.RegenerateLastStatement(CreateInstance<StatementCycleProcessBO>(), row); });
				}
			}

			return adapter.Get();
		}
		public virtual void ARStatementCycle_Day01_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
		{
			ARStatementCycle row = (ARStatementCycle)e.Row;
			if (row != null && row.PrepareOn == ARStatementScheduleType.TwiceAMonth)
			{
				if (!IsCorrectDayOfMonth((Int16?)e.NewValue))
				{
					throw new PXSetPropertyException<ARStatementCycle.day01>(Messages.StatementCycleDayEmpty);
				} 
				if (IsInCorrectForSomeMonth((Int16?)e.NewValue))
				{
					cache.RaiseExceptionHandling<ARStatementCycle.day01>(e.Row, e.NewValue, new PXSetPropertyException(Messages.StatementCycleDayIncorrect, PXErrorLevel.Warning, e.NewValue));
				}
			}
		}
		public virtual void ARStatementCycle_Day00_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
		{
			ARStatementCycle row = (ARStatementCycle)e.Row;
			if (row != null && (row.PrepareOn == ARStatementScheduleType.TwiceAMonth || row.PrepareOn == ARStatementScheduleType.FixedDayOfMonth))
			{
				if (!IsCorrectDayOfMonth((Int16?)e.NewValue))
				{
					throw new PXSetPropertyException<ARStatementCycle.day00>(Messages.StatementCycleDayEmpty);
				}
				if(IsInCorrectForSomeMonth((Int16?)e.NewValue))
				{
					cache.RaiseExceptionHandling<ARStatementCycle.day00>(e.Row,e.NewValue, new PXSetPropertyException(Messages.StatementCycleDayIncorrect, PXErrorLevel.Warning, e.NewValue));
				}
			}
		}

		protected virtual void ARStatementCycle_AgeDays00_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			ARStatementCycle statementCycle = e.Row as ARStatementCycle;

			if (statementCycle == null) return;

			if (e.ExternalCall == true
				&& statementCycle.AgeDays00 != 0
				&& statementCycle.AgeDays01 == 0
				&& statementCycle.AgeDays02 == 0
				&& statementCycle.AgeMsgCurrent == null
				&& statementCycle.AgeMsg00 == null
				&& statementCycle.AgeMsg01 == null
				&& statementCycle.AgeMsg02 == null
				&& statementCycle.AgeMsg03 == null)
			{
				FillBucketBoundaries(cache, statementCycle, statementCycle.AgeDays00.Value);
				FillBucketDescriptions(cache, statementCycle);
			}
		}

		private static void FillBucketBoundaries(PXCache cache, ARStatementCycle statementCycle, short bucketInterval)
		{
			cache.SetValueExt<ARStatementCycle.ageDays01>(statementCycle, (short)(bucketInterval * 2));
			cache.SetValueExt<ARStatementCycle.ageDays02>(statementCycle, (short)(bucketInterval * 3));
		}

		private static void FillBucketDescriptions(PXCache cache, ARStatementCycle statementCycle)
		{
			string[] bucketDescriptions = AgingEngine.GetDayAgingBucketDescriptions(
				AgingDirection.Backwards,
				new int[]
				{
					0,
					statementCycle.AgeDays00.Value,
					statementCycle.AgeDays01.Value,
					statementCycle.AgeDays02.Value,
				},
				false)
				.ToArray();

			cache.SetValueExt<ARStatementCycle.ageMsgCurrent>(statementCycle, bucketDescriptions[0]);
			cache.SetValueExt<ARStatementCycle.ageMsg00>(statementCycle, bucketDescriptions[1]);
			cache.SetValueExt<ARStatementCycle.ageMsg01>(statementCycle, bucketDescriptions[2]);
			cache.SetValueExt<ARStatementCycle.ageMsg02>(statementCycle, bucketDescriptions[3]);
			cache.SetValueExt<ARStatementCycle.ageMsg03>(statementCycle, bucketDescriptions[4]);
		}

		private static bool IsCorrectDayOfMonth(short? day)
		{
			return day != null && day > 0 && day <= 31;
		}

		private static bool IsInCorrectForSomeMonth(short? day)
		{
			return day != null && day > 28;
		}

		public virtual void ARStatementCycle_PrepareOn_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			ARStatementCycle row = (ARStatementCycle)e.Row;
			if (row.PrepareOn == ARStatementScheduleType.EndOfMonth)
			{
				row.Day00 = null;
				row.Day01 = null;
			}
			else if (row.PrepareOn == ARStatementScheduleType.FixedDayOfMonth)
			{
				row.Day01 = null;
				if (!IsCorrectDayOfMonth(row.Day00))
				{
					row.Day00 = 1;
				}
			}
			else if (row.PrepareOn == ARStatementScheduleType.TwiceAMonth)
			{
				if (!IsCorrectDayOfMonth(row.Day00))
				{
					row.Day00 = 1;
				}
				if (!IsCorrectDayOfMonth(row.Day01))
				{
					row.Day01 = 1;
				}
			}
		}

		protected virtual void ARStatementCycle_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			if (e.Row == null) return;

			ARStatementCycle row = (ARStatementCycle)e.Row;
			PXUIFieldAttribute.SetEnabled<ARStatementCycle.day00>(cache, null, (row.PrepareOn == ARStatementScheduleType.FixedDayOfMonth || row.PrepareOn == ARStatementScheduleType.TwiceAMonth));
			PXUIFieldAttribute.SetEnabled<ARStatementCycle.day01>(cache, null, (row.PrepareOn == ARStatementScheduleType.TwiceAMonth));
			PXUIFieldAttribute.SetEnabled<ARStatementCycle.finChargeID>(cache, null, (row.FinChargeApply??false));
			
			bool isRequired = row.FinChargeApply ?? false;
			PXDefaultAttribute.SetPersistingCheck<ARStatementCycle.finChargeID>(cache, row, isRequired ? PXPersistingCheck.Null : PXPersistingCheck.Nothing);
			PXUIFieldAttribute.SetRequired<ARStatementCycle.finChargeID>(cache, isRequired);
			PXUIFieldAttribute.SetEnabled<ARStatementCycle.requireFinChargeProcessing>(cache, null, (row.FinChargeApply ?? false));
		
		}
		protected virtual void ARStatementCycle_RowPersisting(PXCache cache, PXRowPersistingEventArgs e)
		{
			ARStatementCycle row = (ARStatementCycle)e.Row;

			if (row == null || e.Operation == PXDBOperation.Delete)
			{
				return;
			}

			if (row.PrepareOn == ARStatementScheduleType.TwiceAMonth || row.PrepareOn == ARStatementScheduleType.FixedDayOfMonth)
			{
				if (!IsCorrectDayOfMonth(row.Day00))
				{
					cache.RaiseExceptionHandling<ARStatementCycle.day00>(e.Row, row.Day00, new PXSetPropertyException(Messages.StatementCycleDayEmpty, PXErrorLevel.Error));
					throw new PXSetPropertyException<ARStatementCycle.day00>(Messages.StatementCycleDayEmpty);
				}
				if (IsInCorrectForSomeMonth(row.Day00))
				{
					cache.RaiseExceptionHandling<ARStatementCycle.day00>(e.Row, row.Day00, new PXSetPropertyException(Messages.StatementCycleDayIncorrect, PXErrorLevel.Warning, row.Day00));
				}
			}

			if (row.PrepareOn == ARStatementScheduleType.TwiceAMonth )
			{
				if (!IsCorrectDayOfMonth(row.Day01))
				{
					cache.RaiseExceptionHandling<ARStatementCycle.day01>(e.Row, row.Day01, new PXSetPropertyException(Messages.StatementCycleDayEmpty, PXErrorLevel.Error));
					throw new PXSetPropertyException<ARStatementCycle.day01>(Messages.StatementCycleDayEmpty);
				}
				if (IsInCorrectForSomeMonth(row.Day01))
				{
					cache.RaiseExceptionHandling<ARStatementCycle.day01>(e.Row, row.Day01, new PXSetPropertyException(Messages.StatementCycleDayIncorrect, PXErrorLevel.Warning, row.Day01));
				}
			}

			EnsureAgingBucketBoundariesConsistency(cache, row);
		}
		protected virtual void ARStatementCycle_RowDeleting(PXCache cache, PXRowDeletingEventArgs e)
		{
			if (e.Row == null) return;
			PXSelectorAttribute.CheckAndRaiseForeignKeyException(cache, e.Row, typeof(ARStatement.statementCycleId));
			PXSelectorAttribute.CheckAndRaiseForeignKeyException(cache, e.Row, typeof(Customer.statementCycleId));
			PXSelectorAttribute.CheckAndRaiseForeignKeyException(cache, e.Row, typeof(CustomerClass.statementCycleId));
		}

		protected virtual void ARStatementCycle_FinChargeApply_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			ARStatementCycle row = (ARStatementCycle)e.Row;
			if (!(row.FinChargeApply ?? false))
			{
				row.FinChargeID = null;
				row.RequireFinChargeProcessing = false;
			}
		}

		private static void EnsureAgingBucketBoundariesConsistency(PXCache cache, ARStatementCycle statementCycle)
		{
			// Bucket boundaries are neither used nor editable
			// when aging by financial periods, hence no need 
			// to control consistency.
			// -
			if (statementCycle.UseFinPeriodForAging == true) return;

			if (statementCycle.Bucket01LowerInclusiveBound > statementCycle.AgeDays00)
			{
				DisplayBucketBoundaryError<ARStatementCycle.ageDays00>(cache, statementCycle);
			}

			if (statementCycle.Bucket02LowerInclusiveBound > statementCycle.AgeDays01)
			{
				DisplayBucketBoundaryError<ARStatementCycle.ageDays01>(cache, statementCycle);
			}

			if (statementCycle.Bucket03LowerInclusiveBound > statementCycle.AgeDays02)
			{
				DisplayBucketBoundaryError<ARStatementCycle.ageDays02>(cache, statementCycle);
			}
		}

		private static void DisplayBucketBoundaryError<TField>(PXCache cache, ARStatementCycle statementCycle)
			where TField : IBqlField
		{
			cache.RaiseExceptionHandling<TField>(
				statementCycle,
				cache.GetValue(statementCycle, typeof(TField).Name),
				new PXSetPropertyException<TField>(
					Messages.EndDayOfAgingPeriodShouldNotBeEarlierThanStartDay));
		}
	}
}