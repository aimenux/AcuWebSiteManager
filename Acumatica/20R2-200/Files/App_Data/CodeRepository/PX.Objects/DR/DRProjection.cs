using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Data;
using System.Collections;
using PX.Objects.IN;

namespace PX.Objects.DR
{
	public class DRProjection : PXGraph<DRProjection>
	{
		public PXCancel<ScheduleProjectionFilter> Cancel;
		public PXFilter<ScheduleProjectionFilter> Filter;
		public PXFilteredProcessing<DRScheduleDetail, ScheduleProjectionFilter> Items;
		public PXSetup<DRSetup> Setup;

		protected virtual IEnumerable items()
		{
			ScheduleProjectionFilter filter = Filter.Current;

			PXSelectBase<DRScheduleDetail> select = new PXSelectJoin<DRScheduleDetail,
				InnerJoin<DRSchedule, On<DRScheduleDetail.scheduleID, Equal<DRSchedule.scheduleID>>,
				InnerJoin<DRDeferredCode, On<DRDeferredCode.deferredCodeID, Equal<DRScheduleDetail.defCode>>>>,
				Where<DRDeferredCode.method, Equal<DeferredMethodType.cashReceipt>>>(this);

			if (!string.IsNullOrEmpty(filter.DeferredCode))
			{
				select.WhereAnd<Where<DRScheduleDetail.defCode, Equal<Current<ScheduleProjectionFilter.deferredCode>>>>();
			}

			return select.Select();
		}

		public DRProjection()
		{
			DRSetup setup = Setup.Current;
			Items.SetSelected<DRScheduleDetail.selected>();
		}

		#region EventHandlers

		protected virtual void ScheduleProjectionFilter_RowUpdated(PXCache cache, PXRowUpdatedEventArgs e)
		{
			Items.Cache.Clear();
		}

		protected virtual void ScheduleProjectionFilter_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			ScheduleProjectionFilter filter = Filter.Current;

			Items.SetProcessDelegate(RunProjection);
		}

		#endregion

		public static void RunProjection(List<DRScheduleDetail> items)
		{
			
		}
		
		#region Local Types
		[Serializable]
		public partial class ScheduleProjectionFilter : IBqlTable
		{
			#region DeferredCode
			public abstract class deferredCode : PX.Data.BQL.BqlString.Field<deferredCode> { }
			protected String _DeferredCode;
			[PXDBString(10, IsUnicode = true, InputMask = ">aaaaaaaaaa")]
			[PXUIField(DisplayName = "Deferral Code")]
			[PXSelector(typeof(Search<DRDeferredCode.deferredCodeID, Where<DRDeferredCode.method, Equal<DeferredMethodType.cashReceipt>>>))]
			public virtual String DeferredCode
			{
				get
				{
					return this._DeferredCode;
				}
				set
				{
					this._DeferredCode = value;
				}
			}
			#endregion
		}
				
		#endregion
	}
}
