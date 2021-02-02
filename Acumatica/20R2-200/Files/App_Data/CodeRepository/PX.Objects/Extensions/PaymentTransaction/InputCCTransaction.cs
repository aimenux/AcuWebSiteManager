using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;
using PX.Objects.AR;

namespace PX.Objects.Extensions.PaymentTransaction
{
	[PXHidden]
	public class InputCCTransaction : IBqlTable
	{
		#region TranType
		public abstract class tranType : PX.Data.BQL.BqlString.Field<tranType> { }
		[PXString(IsUnicode = true)]
		[TranTypeList]
		[PXUIField(DisplayName = "Transaction type", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual string TranType { get; set; }
		#endregion

		#region PCTranNumber
		public abstract class pCTranNumber : PX.Data.BQL.BqlString.Field<pCTranNumber> { }
		[PXString(50, IsUnicode = true)]
		[PXDefault]
		public virtual string PCTranNumber { get; set; }
		#endregion

		#region AuthNumber
		public abstract class authNumber : PX.Data.BQL.BqlString.Field<authNumber> { }
		[PXString(50, IsUnicode = true)]
		[PXDefault]
		[PXUIField(DisplayName = "Proc. Center Auth. Nbr.")]
		public virtual string AuthNumber { get; set; }
		#endregion

		#region TransactionDate
		public abstract class tranDate : PX.Data.BQL.BqlString.Field<tranDate> { }
		[PXDate]
		[PXUIField(DisplayName = "Transaction Date")]
		public virtual DateTime? TranDate { get; set; }
		#endregion

		#region ExtProfileId
		public abstract class extProfileId : PX.Data.BQL.BqlString.Field<extProfileId> { }
		[PXString(255, IsUnicode = true)]
		public virtual string ExtProfileId { get; set; }
		#endregion

		#region NeedSync
		public abstract class needValidation : PX.Data.BQL.BqlBool.Field<needValidation> { }
		[PXBool]
		[PXDefault(false)]
		public virtual bool? NeedValidation { get; set; }
		#endregion
	}
}
