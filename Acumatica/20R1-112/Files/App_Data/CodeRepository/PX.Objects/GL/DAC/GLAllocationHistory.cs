namespace PX.Objects.GL
{
	using System;
	using PX.Data;
	
	[System.SerializableAttribute()]
	[PXCacheName(Messages.GLAllocationHistory)]
	public partial class GLAllocationHistory : PX.Data.IBqlTable
	{
		#region GLAllocationID
		public abstract class gLAllocationID : PX.Data.BQL.BqlString.Field<gLAllocationID> { }
		protected String _GLAllocationID;
		[PXDBString(15, IsUnicode = true, IsKey = true)]
		[PXDefault()]
		public virtual String GLAllocationID
		{
			get
			{
				return this._GLAllocationID;
			}
			set
			{
				this._GLAllocationID = value;
			}
		}
		#endregion
		#region Module
		public abstract class module : PX.Data.BQL.BqlString.Field<module> { }
		protected String _Module;
		[PXDBString(2, IsKey = true, IsFixed = true)]
		[PXDBDefault(typeof(Batch))]
		public virtual String Module
		{
			get
			{
				return this._Module;
			}
			set
			{
				this._Module = value;
			}
		}
		#endregion
		#region BatchNbr
		public abstract class batchNbr : PX.Data.BQL.BqlString.Field<batchNbr> { }
		protected String _BatchNbr;
		[PXDBString(15, IsUnicode = true, IsKey = true)]
		[PXDBDefault(typeof(Batch))]
		[PXParent(typeof(Select<Batch, Where<Batch.batchNbr, Equal<Current<GLAllocationHistory.batchNbr>>,And<Batch.module,Equal<Current<GLAllocationHistory.module>>>>>))]
		public virtual String BatchNbr
		{
			get
			{
				return this._BatchNbr;
			}
			set
			{
				this._BatchNbr = value;
			}
		}
		#endregion
	}
}
