using System;
using PX.Data;
using PX.Objects.GL;

namespace PX.Objects.FA.DAC
{
	[PXProjection(typeof(Select5<
		FixedAsset,
		InnerJoin<Branch, On<FixedAsset.branchID, Equal<Branch.branchID>>,
		InnerJoin<FALocationHistory, 
			On<FALocationHistory.assetID, Equal<FixedAsset.assetID>, 
			And<FixedAsset.recordType, Equal<FARecordType.assetType>>>,
		InnerJoin<FABook, 
			On<FABook.updateGL, Equal<True>>,
		InnerJoin<FABookPeriod, 
			On<FABookPeriod.bookID, Equal<FABook.bookID>, 
			And<FABookPeriod.organizationID, Equal<Branch.organizationID>,
			And<FALocationHistory.periodID, LessEqual<FABookPeriod.finPeriodID>>>>>>>>,
		Aggregate<
			GroupBy<FALocationHistory.assetID, 
			GroupBy<FABookPeriod.finPeriodID, 
			Max<FALocationHistory.periodID, 
			Max<FALocationHistory.revisionID>>>>>>))]
	[Serializable]
	[PXCacheName(Messages.FALocationHistoryByPeriod)]
	public partial class FALocationHistoryByPeriod : IBqlTable
	{
		#region AssetID
		public abstract class assetID : PX.Data.BQL.BqlInt.Field<assetID> { }
		protected int? _AssetID;
		[PXDBInt(IsKey = true, BqlField = typeof(FixedAsset.assetID))]
		[PXDefault()]
		public virtual int? AssetID
		{
			get
			{
				return this._AssetID;
			}
			set
			{
				this._AssetID = value;
			}
		}
		#endregion
		#region PeriodID
		public abstract class periodID : PX.Data.BQL.BqlString.Field<periodID> { }
		protected string _PeriodID;
		[FABookPeriodID(
			assetSourceType: typeof(assetID),
			BqlField = typeof(FABookPeriod.finPeriodID))]
		[PXDefault]
		public virtual string PeriodID
		{
			get
			{
				return this._PeriodID;
			}
			set
			{
				this._PeriodID = value;
			}
		}
		#endregion
		#region LastPeriodID
		public abstract class lastPeriodID : PX.Data.BQL.BqlString.Field<lastPeriodID> { }
		protected string _LastPeriodID;
		[FABookPeriodID(
			assetSourceType: typeof(assetID),
			BqlField = typeof(FALocationHistory.periodID))]
		[PXDefault]
		public virtual string LastPeriodID
		{
			get
			{
				return this._LastPeriodID;
			}
			set
			{
				this._LastPeriodID = value;
			}
		}
		#endregion
		#region LastRevisionID
		public abstract class lastRevisionID : PX.Data.BQL.BqlInt.Field<lastRevisionID> { }
		protected int? _LastRevisionID;
		[PXDBInt(IsKey = true, BqlField = typeof(FALocationHistory.revisionID))]
		[PXDefault(0)]
		public virtual int? LastRevisionID
		{
			get
			{
				return this._LastRevisionID;
			}
			set
			{
				this._LastRevisionID = value;
			}
		}
		#endregion
	}
}
