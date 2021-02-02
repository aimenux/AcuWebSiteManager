using System;
using PX.Data;

namespace PX.Objects.PM
{
	[PXHidden]
	[Serializable]
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public partial class PMUnbilledDailySummary : IBqlTable
	{
		#region ProjectID
		public abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }
		protected Int32? _ProjectID;
		[PXDBInt(IsKey = true)]
		[PXDefault()]
		public virtual Int32? ProjectID
		{
			get
			{
				return this._ProjectID;
			}
			set
			{
				this._ProjectID = value;
			}
		}
		#endregion
		#region TaskID
		public abstract class taskID : PX.Data.BQL.BqlInt.Field<taskID> { }
		protected Int32? _TaskID;
		[PXDBInt(IsKey = true)]
		[PXDefault()]
		public virtual Int32? TaskID
		{
			get
			{
				return this._TaskID;
			}
			set
			{
				this._TaskID = value;
			}
		}
		#endregion
		#region AccountGroupID
		public abstract class accountGroupID : PX.Data.BQL.BqlInt.Field<accountGroupID> { }
		protected Int32? _AccountGroupID;
		[PXDBInt(IsKey = true)]
		[PXDefault()]
		public virtual Int32? AccountGroupID
		{
			get
			{
				return this._AccountGroupID;
			}
			set
			{
				this._AccountGroupID = value;
			}
		}
		#endregion
		#region Date
		public abstract class date : PX.Data.BQL.BqlDateTime.Field<date> { }
		protected DateTime? _Date;
		[PXDBDate(IsKey=true)]
		[PXDefault]
		public virtual DateTime? Date
		{
			get
			{
				return this._Date;
			}
			set
			{
				this._Date = value;
			}
		}
		#endregion
		#region Billable
		public abstract class billable : PX.Data.BQL.BqlInt.Field<billable> { }
		protected int? _Billable;
		[PXDefault(0)]
		[PXDBInt]
		public virtual int? Billable
		{
			get
			{
				return this._Billable;
			}
			set
			{
				this._Billable = value;
			}
		}
		#endregion
		#region NonBillable
		public abstract class nonBillable : PX.Data.BQL.BqlInt.Field<nonBillable> { }
		protected int? _NonBillable;
		[PXDefault(0)]
		[PXDBInt]
		public virtual int? NonBillable
		{
			get
			{
				return this._NonBillable;
			}
			set
			{
				this._NonBillable = value;
			}
		}
		#endregion
		
		#region tstamp
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }
		protected Byte[] _tstamp;
		[PXDBTimestamp()]
		public virtual Byte[] tstamp
		{
			get
			{
				return this._tstamp;
			}
			set
			{
				this._tstamp = value;
			}
		}
		#endregion
	}

	public class PMUnbilledDailySummaryAccumAttribute : PXAccumulatorAttribute
	{
		public PMUnbilledDailySummaryAccumAttribute()
		{
			base._SingleRecord = true;
		}
		protected override bool PrepareInsert(PXCache sender, object row, PXAccumulatorCollection columns)
		{
			if (!base.PrepareInsert(sender, row, columns))
			{
				return false;
			}
					
			PMUnbilledDailySummaryAccum item = (PMUnbilledDailySummaryAccum)row;
			columns.Update<PMUnbilledDailySummaryAccum.billable>(item.Billable, PXDataFieldAssign.AssignBehavior.Summarize);
			columns.Update<PMUnbilledDailySummaryAccum.nonBillable>(item.NonBillable, PXDataFieldAssign.AssignBehavior.Summarize);
			
			return true;
		}
	}

	[PXHidden]
	[Serializable]
	[PMUnbilledDailySummaryAccum]
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public partial class PMUnbilledDailySummaryAccum : PMUnbilledDailySummary
	{
		public new abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }
		public new abstract class taskID : PX.Data.BQL.BqlInt.Field<taskID> { }
		public new abstract class accountGroupID : PX.Data.BQL.BqlInt.Field<accountGroupID> { }
		public new abstract class date : PX.Data.BQL.BqlDateTime.Field<date> { }
		public new abstract class billable : PX.Data.BQL.BqlInt.Field<billable> { }
		public new abstract class nonBillable : PX.Data.BQL.BqlInt.Field<nonBillable> { }
	}
}
