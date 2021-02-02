using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;
using PX.Objects.AR.CCPaymentProcessing.Interfaces;
namespace PX.Objects.Extensions.PaymentTransaction
{
	public partial class InputPaymentInfo : IBqlTable,ICCManualInputPaymentInfo
	{
		#region PCTranNumber
		public abstract class pCTranNumber : PX.Data.IBqlField
		{
		}
		protected String _PCTranNumber;
		[PXDBString(50, IsUnicode = true)]
		[PXDefault()]
		[PXUIField(DisplayName = "Proc. Center Tran. Nbr.", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String PCTranNumber
		{
			get
			{
				return this._PCTranNumber;
			}
			set
			{
				this._PCTranNumber = value;
			}
		}
		#endregion
		#region AuthNumber
		public abstract class authNumber : PX.Data.IBqlField
		{
		}
		protected String _AuthNumber;
		[PXDBString(50, IsUnicode = true)]
		[PXDefault()]
		[PXUIField(DisplayName = "Proc. Center Auth. Nbr.")]
		public virtual String AuthNumber
		{
			get
			{
				return this._AuthNumber;
			}
			set
			{
				this._AuthNumber = value;
			}
		}
		#endregion
	}
}
