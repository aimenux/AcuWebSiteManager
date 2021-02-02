using System;
using PX.Data;
using PX.Objects.CS;
using PX.Objects.DR;

namespace PX.Objects.CT
{
	[System.SerializableAttribute()]
    [PXHidden]
	public partial class ContractTask : PX.Data.IBqlTable
	{
		#region ContractID
		public abstract class contractID : PX.Data.BQL.BqlInt.Field<contractID> { }
		protected Int32? _ContractID;
		[PXDBInt(IsKey = true)]
		[PXDBDefault(typeof(Contract.contractID))]
		public virtual Int32? ContractID
		{
			get
			{
				return this._ContractID;
			}
			set
			{
				this._ContractID = value;
			}
		}
		#endregion
		#region TaskID
		public abstract class taskID : PX.Data.BQL.BqlInt.Field<taskID> { }
		protected Int32? _TaskID;
		[PXDBInt()]
		[PXLineNbr(typeof(Contract))]
		[PXParent(typeof(Select<Contract, Where<Contract.contractID, Equal<Current<ContractTask.contractID>>>>), LeaveChildren = true)]
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
		#region TaskCD
		public abstract class taskCD : PX.Data.BQL.BqlString.Field<taskCD> { }
		protected String _TaskCD;
		[PXDBString(30, IsUnicode = true, IsKey = true, InputMask = "")]
		[PXUIField(DisplayName = "Task ID", Visibility = PXUIVisibility.SelectorVisible)]
		[PXDefault()]
		public virtual String TaskCD
		{
			get
			{
				return this._TaskCD;
			}
			set
			{
				this._TaskCD = value;
			}
		}
		#endregion
		#region TaskDescr
		public abstract class taskDescr : PX.Data.BQL.BqlString.Field<taskDescr> { }
		protected String _TaskDescr;
		[PXDBString(60, IsUnicode = true)]
		[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String TaskDescr
		{
			get
			{
				return this._TaskDescr;
			}
			set
			{
				this._TaskDescr = value;
			}
		}
		#endregion
		#region DeferredCode
		public abstract class deferredCode : PX.Data.BQL.BqlString.Field<deferredCode> { }
		protected String _DeferredCode;
		[PXDBString(10, IsUnicode = true)]
		[PXSelector(typeof(Search<DRDeferredCode.deferredCodeID>))]
		[PXUIField(DisplayName = "Deferral Code")]
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
		#region TaskProgress
		public abstract class taskProgress : PX.Data.BQL.BqlDecimal.Field<taskProgress> { }
		protected Decimal? _TaskProgress;
		[PXDBDecimal(2)]
		[PXDefault(TypeCode.Decimal,"0.0")]
		[PXUIField(DisplayName = "Progress, (%%)")]
		public virtual Decimal? TaskProgress
		{
			get
			{
				return this._TaskProgress;
			}
			set
			{
				this._TaskProgress = value;
			}
		}
		#endregion
		#region RecognitionMethod
		public abstract class recognitionMethod : PX.Data.BQL.BqlString.Field<recognitionMethod> { }
		protected String _RecognitionMethod;
		[PXDBString(1, IsFixed = true)]
		[PXUIField(DisplayName = "Recognition Method")]
		[PXDefault("C")]
		[PXStringList(new string[] { "P", "C"}, new string[] { "On Progress", "On Completion"})]
		public virtual String RecognitionMethod
		{
			get
			{
				return this._RecognitionMethod;
			}
			set
			{
				this._RecognitionMethod = value;
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
}
