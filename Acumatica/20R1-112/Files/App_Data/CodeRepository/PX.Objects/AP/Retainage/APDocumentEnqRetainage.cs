using System;

using PX.Data;
using PX.Objects.CS;

namespace PX.Objects.AP
{
	[Serializable]
	public class APDocumentEnqRetainage : PXGraphExtension<APDocumentEnq>
	{
		[Serializable]
		[PXHidden]
		public class APRegisterOrig : APRegister
		{
			#region DocType
			public new abstract class docType : PX.Data.BQL.BqlString.Field<docType> { }
			#endregion
			#region RefNbr
			public new abstract class refNbr : IBqlField { }
			#endregion
			#region BranchID
			public new abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
			#endregion
			#region Released
			public new abstract class released : PX.Data.BQL.BqlBool.Field<released> { }
			#endregion
			#region OrigDocType
			public new abstract class origDocType : PX.Data.BQL.BqlString.Field<origDocType> { }
			#endregion
			#region OrigRefNbr
			public new abstract class origRefNbr : IBqlField { }
			#endregion
		}

		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.retainage>();
		}

		#region Cache Attached Events

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXCustomizeBaseAttribute(typeof(PXUIFieldAttribute), nameof(PXUIFieldAttribute.DisplayName), "Currency Original Retainage")]
		protected virtual void APDocumentResult_CuryRetainageTotal_CacheAttached(PXCache sender) { }
		
		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXCustomizeBaseAttribute(typeof(PXUIFieldAttribute), nameof(PXUIFieldAttribute.DisplayName), "Currency Unreleased Retainage")]
		protected virtual void APDocumentResult_CuryRetainageUnreleasedAmt_CacheAttached(PXCache sender) { }
		
		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXCustomizeBaseAttribute(typeof(PXUIFieldAttribute), nameof(PXUIFieldAttribute.DisplayName), "Currency Total Amount")]
		protected virtual void APDocumentResult_CuryOrigDocAmtWithRetainageTotal_CacheAttached(PXCache sender) { }

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXUIField(DisplayName = "Retainage", Visible = false)]
		[PXDBCalced(typeof(
						Switch<Case<Where<
						Exists<Select<
							APRegisterOrig,
							Where<APRegisterOrig.origDocType, Equal<APDocumentEnq.APDocumentResult.docType>,
								And<APRegisterOrig.origRefNbr, Equal<APDocumentEnq.APDocumentResult.refNbr>,
								And<APRegisterOrig.branchID, Equal<APDocumentEnq.APDocumentResult.branchID>,
								And<APRegisterOrig.released, Equal<True>>>>>>>>,
					True>, False>),
			typeof(bool))]
		protected virtual void APDocumentResult_Retainage_CacheAttached(PXCache sender) { }
		#endregion
	}
}