namespace PX.Objects.GL
{
	using System;
	using PX.Data;
    using PX.Data.EP;
	using System.Collections;
	using System.Collections.Generic;

    /// <summary>
    /// Represents Voucher Entry Code, used to create a document of GL, AP, AR or CA module as a part of a <see cref="GLDocBatch">GL Document Batch</see>.
    /// User can manage records of this type on the Voucher Entry Codes (GL.10.60.00) page of the system.
    /// </summary>
	[System.SerializableAttribute()]
	[PXCacheName(Messages.GLTranCode)]
	public partial class GLTranCode : PX.Data.IBqlTable
	{
		#region Module
		public abstract class module : PX.Data.BQL.BqlString.Field<module> { }
		protected String _Module;

        /// <summary>
        /// The code of the module where a document or transaction will be generated according to this entry code.
        /// </summary>
        /// <value>
        /// Allowed values are: <c>"GL"</c>, <c>"AP"</c>, <c>"AR"</c> and <c>"CA"</c>.
        /// </value>
		[PXDBString(2, IsKey = true, IsFixed = true)]
		[PXDefault()]
        [PXUIField(DisplayName = "Module", Visibility = PXUIVisibility.SelectorVisible)]
		[VoucherModule.List()]
        [PXFieldDescription]		
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
		#region TranType
		public abstract class tranType : PX.Data.BQL.BqlString.Field<tranType> { }
		protected String _TranType;

        /// <summary>
        /// The type of the document or transaction generated according to the code.
        /// </summary>
        /// <value>
        /// Allowed values set depends on the selected <see cref="Module"/>.
        /// </value>
		[PXDBString(3, IsKey = true, IsFixed = true)]        
		[PXDefault()]
        [PXUIField(DisplayName = "Module Tran. Type",Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String TranType
		{
			get
			{
				return this._TranType;
			}
			set
			{
				this._TranType = value;
			}
		}
		#endregion
		#region TranCode
		public abstract class tranCode : PX.Data.BQL.BqlString.Field<tranCode> { }
		protected String _TranCode;

        /// <summary>
        /// !REV!
        /// The unique voucher code for the selected <see cref="Module"/> and <see cref="TranType">Type</see> of the document.
        /// The code is selected by user on the Journal Vouchers (GL.30.40.00) screen (<see cref="GLTranCode.TranCode"/> field)
        /// when entering the lines of the batch and determines the module and type of the document or transaction
        /// to be created from the corresponding line of the document batch.
        /// Identifies the record of this DAC associated with a <see cref="GLTranCode">line</see> of a <see cref="GLDocBatch">document batch</see>.
        /// Only one code can be created for any combination of <see cref="Module"/> and <see cref="TranType">Document/Transaction Type</see>.
        /// </summary>
		[PXDBString(5, IsUnicode = true,InputMask = ">aaaaa")]
		[PXDefault()]
        [PXCheckUnique()]
        [PXUIField(DisplayName = "Unique Tran. Code", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String TranCode
		{
			get
			{
				return this._TranCode;
			}
			set
			{
				this._TranCode = value;
			}
		}
		#endregion
		#region Descr
		public abstract class descr : PX.Data.BQL.BqlString.Field<descr> { }
		protected String _Descr;

        /// <summary>
        /// Description of the entry code.
        /// </summary>
		[PXDBString(60, IsUnicode = true)]
        [PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String Descr
		{
			get
			{
				return this._Descr;
			}
			set
			{
				this._Descr = value;
			}
		}
		#endregion
        #region Active
        public abstract class active : PX.Data.BQL.BqlBool.Field<active> { }
        protected bool? _Active;

        /// <summary>
        /// Indicates whether the entry code is active.
        /// Only active codes can be used to create documents or transactions.
        /// </summary>
        /// <value>
        /// Defaults to <c>true</c>.
        /// If set to <c>false</c>, the entry code won't appear in the list of available codes for the <see cref="GLTranDoc.TranCode"/> field.
        /// </value>
        [PXDBBool]
        [PXDefault(true)]
        [PXUIField(DisplayName = "Active")]
        public virtual bool? Active
        {
            get
            {
                return _Active;
            }
            set
            {
                _Active = value;
            }
        }
        #endregion
	}

	public static class VoucherModule 
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(
					new string[] { GL.BatchModule.GL, GL.BatchModule.AP, GL.BatchModule.AR, GL.BatchModule.CA},
					new string[] { Messages.ModuleGL, Messages.ModuleAP, Messages.ModuleAR, Messages.ModuleCA }) { }
		}		
	}	
}
