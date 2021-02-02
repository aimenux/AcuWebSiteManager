using System;
using PX.Data;
using PX.Objects.CM;
using PX.Objects.CR;
using PX.Objects.GL;
using PX.TM;


namespace PX.Objects.CA
{
    /// <summary>
    /// This entity links CABatch and APPayment.
    /// </summary>
    [PXCacheName(Messages.CABatchDetail)]
    [Serializable]
	public partial class CABatchDetail : IBqlTable
	{
		#region BatchNbr
		public abstract class batchNbr : PX.Data.BQL.BqlString.Field<batchNbr> { }

        /// <summary>
        /// This field is the key field.
        /// Corresponds to the <see cref="CABatch.BatchNbr"/> field.
        /// </summary>
		[PXDBString(15, IsKey = true, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
		[PXDBDefault(typeof(CABatch.batchNbr))]
		[PXParent(typeof(Select<CABatch, Where<CABatch.batchNbr, Equal<Current<CABatchDetail.batchNbr>>>>))]

		public virtual string BatchNbr
		{
			get;
			set;
		}
		#endregion
		#region OrigModule
		public abstract class origModule : PX.Data.BQL.BqlString.Field<origModule> { }

		/// <summary>
		/// This field is a part of the compound key of the document.
		/// It is either equals to <see cref="GL.BatchModule.AP"/> or <see cref="GL.BatchModule.PR"/>in current implementation.
		/// Potentially it may be equal to <see cref="GL.BatchModule.AR"/>.
		/// </summary>
		[PXDBString(2, IsFixed = true, IsKey = true)]
		[PXDefault(GL.BatchModule.AP)]
		[PXStringList(new string[] { GL.BatchModule.AP, GL.BatchModule.AR, GL.BatchModule.PR }, new string[] { BatchModule.AP, BatchModule.AR, BatchModule.PR })]
		[PXUIField(DisplayName = "Module", Enabled = false)]
		public virtual string OrigModule
		{
			get;
			set;
		}
		#endregion
		#region OrigDocType
		public abstract class origDocType : PX.Data.BQL.BqlString.Field<origDocType> { }

        /// <summary>
        /// The type of payment document.
        /// This field is a part of the compound key of the document.
        /// Corresponds to the <see cref="PX.Objects.AP.APRegister.DocType"/> field and the <see cref="PX.Objects.AP.APPayment.DocType"/> field.
        /// </summary>
		[PXDBString(3, IsFixed = true, IsKey = true)]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Doc. Type")]
		public virtual string OrigDocType
		{
			get;
			set;
		}
		#endregion
		#region OrigRefNbr
		public abstract class origRefNbr : PX.Data.BQL.BqlString.Field<origRefNbr> { }

        /// <summary>
        /// The payment's reference number.
        /// This number is a link to payment document on the Checks and Payments (AP302000) form.
        /// This field is a part of the compound key of the document.
        /// Corresponds to the <see cref="PX.Objects.AP.APRegister.RefNbr"/> field and the <see cref="PX.Objects.AP.APPayment.RefNbr"/> field.
        /// </summary>
		[PXDBString(15, IsUnicode = true, IsKey = true)]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Reference Nbr.")]
		public virtual string OrigRefNbr
		{
			get;
			set;
		}
		#endregion
		#region OrigLineNbr
		/// <summary>
		/// Key field used to differentiate between Direct Deposit splits in PR module. For other modules, it isn't necessary and defaults to 0.
		/// </summary>
		[PXDBInt(IsKey = true)]
		[PXUIField(DisplayName = "Line Nbr.", Visible = false)]
		[PXDefault(0)]
		public virtual int? OrigLineNbr { get; set; }
		public abstract class origLineNbr : PX.Data.BQL.BqlInt.Field<origLineNbr> 
		{
			public const int DefaultValue = 0;
		}
		#endregion

		public virtual void Copy(AP.APPayment payment)
		{
			this.OrigRefNbr = payment.RefNbr;
			this.OrigDocType = payment.DocType;
			this.OrigModule = GL.BatchModule.AP;
		}

		public virtual void Copy(AR.ARPayment payment)
		{
			this.OrigRefNbr = payment.RefNbr;
			this.OrigDocType = payment.DocType;
			this.OrigModule = GL.BatchModule.AR;
		}
	}
}
