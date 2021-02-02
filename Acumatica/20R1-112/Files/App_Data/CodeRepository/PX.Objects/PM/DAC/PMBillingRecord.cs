namespace PX.Objects.PM
{
	using System;
	using PX.Data;

	[PXCacheName(Messages.ProjectBillingRecord)]
	[Serializable]
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public partial class PMBillingRecord : IBqlTable
	{
		#region ProjectID
		public abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }
		[PXDBInt(IsKey = true)]
		[PXDefault(typeof(PMProject.contractID))]
		[PXUIField(DisplayName = "Project ID")]
		public virtual Int32? ProjectID
		{
			get; set;
		}
		#endregion
		#region RecordID
		public abstract class recordID : PX.Data.BQL.BqlInt.Field<recordID> { }
		[PXDefault(typeof(PMProject.billingLineCntr))]
		[PXDBInt(IsKey = true)]
		[PXUIField(DisplayName = "Billing Number")]
		public virtual Int32? RecordID
		{
			get;set;
		}
		#endregion
		
		#region BillingTag
		public abstract class billingTag : PX.Data.BQL.BqlString.Field<billingTag> { }
		[PXDefault("P")]
		[PXDBString(30, IsKey = true, IsUnicode = true)]
		public virtual String BillingTag
		{
			get; set;
		}
		#endregion
		#region Date
		public abstract class date : PX.Data.BQL.BqlDateTime.Field<date> { }
		protected DateTime? _Date;
		[PXDBDate]
		[PXDefault(typeof(AccessInfo.businessDate))]
		[PXUIField(DisplayName = "Pro Forma Date", Visibility = PXUIVisibility.SelectorVisible, Required = true)]
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
		#region ProformaRefNbr
		public abstract class proformaRefNbr : PX.Data.BQL.BqlString.Field<proformaRefNbr> { }
		[PXSelector(typeof(Search<PMProforma.refNbr, Where<PMProforma.projectID, Equal<Current<PMBillingRecord.projectID>>>>))]
		[PXDBString(PMProforma.refNbr.Length, IsUnicode = true)]
		[PXUIField(DisplayName = "Pro Forma Reference Nbr.")]
		public virtual String ProformaRefNbr
		{
			get; set;
		}
		#endregion
		#region ARDocType
		public abstract class aRDocType : PX.Data.BQL.BqlString.Field<aRDocType> { }
		protected String _ARDocType;
		[PXUIField(DisplayName = "AR Doc. Type")]
		[AR.ARInvoiceType.List()]
		[PXDBString(3, IsFixed = true)]
		public virtual String ARDocType
		{
			get
			{
				return this._ARDocType;
			}
			set
			{
				this._ARDocType = value;
			}
		}
		#endregion
		#region ARRefNbr
		public abstract class aRRefNbr : PX.Data.BQL.BqlString.Field<aRRefNbr> { }
		protected String _ARRefNbr;
		[PXDBString(15, IsUnicode = true)]
		[PXUIField(DisplayName = "AR Reference Nbr.")]
		[PXSelector(typeof(Search<PX.Objects.AR.ARInvoice.refNbr>))]
		public virtual String ARRefNbr
		{
			get
			{
				return this._ARRefNbr;
			}
			set
			{
				this._ARRefNbr = value;
			}
		}
		#endregion
		#region SortOrder
		public abstract class sortOrder : PX.Data.BQL.BqlInt.Field<sortOrder> { }
		protected Int32? _SortOrder;
		[PXInt]
		public virtual Int32? SortOrder
		{
			get
			{
				return this._SortOrder ?? RecordID;
			}
			set
			{
				this._SortOrder = value;
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

		#region RecordNumber
		public abstract class recordNumber : PX.Data.BQL.BqlInt.Field<recordNumber> { }
		
		[PXInt]
		[PXUIField(DisplayName = "Billing Number", Visible = false)]
		public virtual Int32? RecordNumber
		{
			get { return RecordID < 0 ? null : RecordID; }
		}
		#endregion
	}

	[PXBreakInheritance]
	[PXHidden]
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public class PMBillingRecordEx : PMBillingRecord
	{
		public new abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }

		public new abstract class recordID : PX.Data.BQL.BqlInt.Field<recordID> { }

		public new abstract class billingTag : PX.Data.BQL.BqlString.Field<billingTag> { }

		public new abstract class proformaRefNbr : PX.Data.BQL.BqlString.Field<proformaRefNbr> { }

		public new abstract class aRDocType : PX.Data.BQL.BqlString.Field<aRDocType> { }
		public new abstract class aRRefNbr : PX.Data.BQL.BqlString.Field<aRRefNbr> { }



	}
}
