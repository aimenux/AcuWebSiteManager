using PX.Data;
using System;

namespace PX.Objects.IN.Matrix.DAC.Accumulators
{
	[PXHidden]
	[TemplateItemLastModifiedUpdate.Accumulator(BqlTable = typeof(InventoryItem))]
	public class TemplateItemLastModifiedUpdate : IBqlTable
	{
		#region InventoryID
		public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
		[PXDBInt(IsKey = true)]
		public virtual Int32? InventoryID
		{
			get;
			set;
		}
		#endregion
		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
		[PXDBLastModifiedDateTime]
		public virtual DateTime? LastModifiedDateTime
		{
			get;
			set;
		}
		#endregion
		#region LastModifiedByID
		public abstract class lastModifiedByID : Data.BQL.BqlGuid.Field<lastModifiedByID> { }
		[PXDBLastModifiedByID]
		public virtual Guid? LastModifiedByID
		{
			get;
			set;
		}
		#endregion
		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
		[PXDBLastModifiedByScreenID]
		public virtual string LastModifiedByScreenID
		{
			get;
			set;
		}
		#endregion
		#region tstamp
		public abstract class Tstamp : Data.BQL.BqlByteArray.Field<Tstamp> { }
		[PXDBTimestamp]
		public virtual byte[] tstamp
		{
			get;
			set;
		}
		#endregion

		public class AccumulatorAttribute : PXAccumulatorAttribute
		{
			public AccumulatorAttribute()
			{
				this.SingleRecord = true;
			}

			protected override bool PrepareInsert(PXCache sender, object row, PXAccumulatorCollection columns)
			{
				if (!base.PrepareInsert(sender, row, columns))
				{
					return false;
				}

				var templateItem = (TemplateItemLastModifiedUpdate)row;

				columns.UpdateOnly = true;
				columns.Update<TemplateItemLastModifiedUpdate.lastModifiedByID>(templateItem.LastModifiedByID, PXDataFieldAssign.AssignBehavior.Replace);
				columns.Update<TemplateItemLastModifiedUpdate.lastModifiedDateTime>(templateItem.LastModifiedDateTime, PXDataFieldAssign.AssignBehavior.Replace);
				columns.Update<TemplateItemLastModifiedUpdate.lastModifiedByScreenID>(templateItem.LastModifiedByScreenID, PXDataFieldAssign.AssignBehavior.Replace);

				return true;
			}
		}
	}
}
