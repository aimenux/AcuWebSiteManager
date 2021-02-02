using System;
using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.IN;

namespace PX.Objects.CR
{
	[Serializable]
	[PXCacheName(Messages.CaseClassLabor)]
	public partial class CRCaseClassLaborMatrix : PX.Data.IBqlTable
	{

		#region CaseClassID
		public abstract class caseClassID : PX.Data.BQL.BqlString.Field<caseClassID> { }
		protected String _CaseClassID;
		[PXDBString(10, IsUnicode = true, IsKey = true)]
		[PXDBDefault(typeof(CRCaseClass.caseClassID))]
		[PXParent(typeof(Select<CRCaseClass, Where<Current<CRCaseClassLaborMatrix.caseClassID>, Equal<CRCaseClass.caseClassID>>>))]
		public virtual String CaseClassID
		{
			get
			{
				return this._CaseClassID;
			}
			set
			{
				this._CaseClassID = value;
			}
		}
		#endregion

		#region EarningType
		public abstract class earningType : PX.Data.BQL.BqlString.Field<earningType> { }
		protected string _EarningType;
		[PXDBString(2, IsFixed = true, IsKey = true, IsUnicode = false, InputMask = ">LL")]
		[PXDefault()]
		[PXRestrictor(typeof(Where<EP.EPEarningType.isActive, Equal<True>>), EP.Messages.EarningTypeInactive, typeof(EP.EPEarningType.typeCD))]
		[PXSelector(typeof(EP.EPEarningType.typeCD))]
		[PXUIField(DisplayName = "Earning Type")]
		public virtual string EarningType
		{
			get
			{
				return this._EarningType;
			}
			set
			{
				this._EarningType = value;
			}

		}
		#endregion

		#region LabourItemID
		public abstract class labourItemID : PX.Data.BQL.BqlInt.Field<labourItemID> { }
		protected Int32? _LabourItemID;
		[PXDBInt()]
		[PXDefault()]
		[PXUIField(DisplayName = "Labor Item")]
		[PXDimensionSelector(InventoryAttribute.DimensionName, typeof(Search<InventoryItem.inventoryID, Where<InventoryItem.itemType, Equal<INItemTypes.laborItem>, And<InventoryItem.itemStatus, NotEqual<InventoryItemStatus.unknown>, And<InventoryItem.isTemplate, Equal<False>, And<Match<Current<AccessInfo.userName>>>>>>>), typeof(InventoryItem.inventoryCD))]
		[PXForeignReference(typeof(Field<labourItemID>.IsRelatedTo<InventoryItem.inventoryID>))]
		public virtual Int32? LabourItemID
		{
			get
			{
				return this._LabourItemID;
			}
			set
			{
				this._LabourItemID = value;
			}
		}
		#endregion

		#region System Columns
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
		#region CreatedByID
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
		protected Guid? _CreatedByID;
		[PXDBCreatedByID()]
		public virtual Guid? CreatedByID
		{
			get
			{
				return this._CreatedByID;
			}
			set
			{
				this._CreatedByID = value;
			}
		}
		#endregion
		#region CreatedByScreenID
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }
		protected String _CreatedByScreenID;
		[PXDBCreatedByScreenID()]
		public virtual String CreatedByScreenID
		{
			get
			{
				return this._CreatedByScreenID;
			}
			set
			{
				this._CreatedByScreenID = value;
			}
		}
		#endregion
		#region CreatedDateTime
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
		protected DateTime? _CreatedDateTime;
		[PXDBCreatedDateTime()]
		public virtual DateTime? CreatedDateTime
		{
			get
			{
				return this._CreatedDateTime;
			}
			set
			{
				this._CreatedDateTime = value;
			}
		}
		#endregion
		#region LastModifiedByID
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
		protected Guid? _LastModifiedByID;
		[PXDBLastModifiedByID()]
		public virtual Guid? LastModifiedByID
		{
			get
			{
				return this._LastModifiedByID;
			}
			set
			{
				this._LastModifiedByID = value;
			}
		}
		#endregion
		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
		protected String _LastModifiedByScreenID;
		[PXDBLastModifiedByScreenID()]
		public virtual String LastModifiedByScreenID
		{
			get
			{
				return this._LastModifiedByScreenID;
			}
			set
			{
				this._LastModifiedByScreenID = value;
			}
		}
		#endregion
		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
		protected DateTime? _LastModifiedDateTime;
		[PXDBLastModifiedDateTime()]
		public virtual DateTime? LastModifiedDateTime
		{
			get
			{
				return this._LastModifiedDateTime;
			}
			set
			{
				this._LastModifiedDateTime = value;
			}
		}
		#endregion
		#endregion

		public static int? GetLaborClassID(PXGraph graph, string caseClassID, string earningTypeID)
		{
			CRCaseClassLaborMatrix matrix =
				PXSelect<CRCaseClassLaborMatrix
					, Where<
						CRCaseClassLaborMatrix.caseClassID, Equal<Required<CRCaseClass.caseClassID>>
						, And<CRCaseClassLaborMatrix.earningType, Equal<Required<CRCaseClassLaborMatrix.earningType>>>
						>
					>.Select(graph, new object[] { caseClassID, earningTypeID });
			return matrix != null ? matrix.LabourItemID: null;
		}

	}
}
