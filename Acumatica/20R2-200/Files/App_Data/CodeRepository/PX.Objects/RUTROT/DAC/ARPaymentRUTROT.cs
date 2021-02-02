using System;
using PX.Data;
using PX.Objects.AR;

namespace PX.Objects.RUTROT
{
	[Serializable]
	public partial class ARPaymentRUTROT : PXCacheExtension<ARPayment>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<CS.FeaturesSet.rutRotDeduction>();
		}
		#region IsRUTROTPayment
		public abstract class isRUTROTPayment : PX.Data.BQL.BqlBool.Field<isRUTROTPayment> { }
		/// <summary>
		/// Specifies (if set to <c>true</c>) that the payment is created for a 
		/// claimed invoice and contains the amount that is paid by Skatteverket.
		/// This field is relevant only if 
		/// <see cref="CS.FeaturesSet.RutRotDeduction">ROT and RUT Deduction</see> feature is enabled.
		/// </summary>
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "ROT or RUT payment", FieldClass = RUTROTMessages.FieldClass, Visible = false)]
		public virtual bool? IsRUTROTPayment
		{
			get;
			set;
		}
		#endregion
	}
}