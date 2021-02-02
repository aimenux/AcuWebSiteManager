using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;
using PX.Objects.AR;
using PX.Objects.AR.CCPaymentProcessing.Interfaces;
using PX.Objects.AR.CCPaymentProcessing.Common;

namespace PX.Objects.Extensions.PaymentTransaction
{
	public partial class InputPaymentInfo : IBqlTable,ICCManualInputPaymentInfo
	{
		#region PCTranNumber
		public abstract class pCTranNumber : PX.Data.BQL.BqlString.Field<pCTranNumber> { }
		[PXDBString(50, IsUnicode = true)]
		[PXDefault()]
		[PXUIField(DisplayName = "Proc. Center Tran. Nbr.", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual string PCTranNumber { get; set; }
		#endregion

		#region AuthNumber
		public abstract class authNumber : PX.Data.BQL.BqlString.Field<authNumber> { }
		[PXDBString(50, IsUnicode = true)]
		[PXDefault()]
		[PXUIField(DisplayName = "Proc. Center Auth. Nbr.")]
		public virtual string AuthNumber{ get; set; }
		#endregion
	}
}
