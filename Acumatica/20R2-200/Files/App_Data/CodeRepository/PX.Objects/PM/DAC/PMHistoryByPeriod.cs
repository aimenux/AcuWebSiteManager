using PX.Objects.CM;
using PX.Objects.IN;
using PX.SM;
using System;
using PX.Data;
using PX.Objects.GL;
using PX.Objects.GL.FinPeriods;

namespace PX.Objects.PM
{
	
	[System.SerializableAttribute()]
	[PXProjection(typeof(Select5<PMHistory,
		InnerJoin<MasterFinPeriod, On<MasterFinPeriod.finPeriodID, GreaterEqual<PMHistory.periodID>>>,
	   Aggregate<
	   GroupBy<PMHistory.projectID,
	   GroupBy<PMHistory.projectTaskID,
	   GroupBy<PMHistory.accountGroupID,
	   GroupBy<PMHistory.inventoryID,
		GroupBy<PMHistory.costCodeID,
	   Max<PMHistory.periodID,
		GroupBy<MasterFinPeriod.finPeriodID
		>>>>>>>>>))]
    [PXHidden]
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public class PMHistoryByPeriod : IBqlTable
	{
		#region ProjectID
		public abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }
		protected Int32? _ProjectID;
		[PXDBInt(IsKey = true, BqlField = typeof(PMHistory.projectID))]
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
		#region ProjectTaskID
		public abstract class projectTaskID : PX.Data.BQL.BqlInt.Field<projectTaskID> { }
		protected Int32? _ProjectTaskID;
		[PXDBInt(IsKey = true, BqlField = typeof(PMHistory.projectTaskID))]
		[PXDefault()]
		public virtual Int32? ProjectTaskID
		{
			get
			{
				return this._ProjectTaskID;
			}
			set
			{
				this._ProjectTaskID = value;
			}
		}
		#endregion
		#region AccountGroupID
		public abstract class accountGroupID : PX.Data.BQL.BqlInt.Field<accountGroupID> { }
		protected Int32? _AccountGroupID;
		[PXDBInt(IsKey = true, BqlField = typeof(PMHistory.accountGroupID))]
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
		#region InventoryID
		public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
		protected Int32? _InventoryID;
		[PXDBInt(IsKey = true, BqlField = typeof(PMHistory.inventoryID))]
		[PXDefault()]
		public virtual Int32? InventoryID
		{
			get
			{
				return this._InventoryID;
			}
			set
			{
				this._InventoryID = value;
			}
		}
		#endregion
		#region CostCodeID
		public abstract class costCodeID : PX.Data.BQL.BqlInt.Field<costCodeID> { }
		protected Int32? _CostCodeID;
		[PXDBInt(IsKey = true, BqlField = typeof(PMHistory.costCodeID))]
		[PXDefault()]
		public virtual Int32? CostCodeID
		{
			get
			{
				return this._CostCodeID;
			}
			set
			{
				this._CostCodeID = value;
			}
		}
		#endregion
		#region PeriodID
		public abstract class periodID : PX.Data.BQL.BqlString.Field<periodID> { }
		protected String _PeriodID;
		[GL.FinPeriodID(IsKey = true, BqlField = typeof(PMHistory.projectID))]
		[PXDefault]
		[PXUIField(DisplayName = "Financial Period", Visibility = PXUIVisibility.Invisible)]
		[PXSelector(typeof(MasterFinPeriod.finPeriodID), DescriptionField = typeof(MasterFinPeriod.descr))]
		public virtual String PeriodID
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
	}
}