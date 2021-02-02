using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

using PX.Data;
using PX.Data.SQLTree;
using PX.Objects.AP.Standalone;

namespace PX.Objects.AP.BQL
{
	/// <summary>
	/// A predicate that returns <c>true</c> if and only if the payment defined
	/// by its key fields (document type and reference number) has an unreleased 
	/// void payment. This may be needed to exclude such payments from processing
	/// to prevent creating unnecessary applications.
	/// </summary>
	public class HasUnreleasedVoidPayment<TDocTypeField, TRefNbrField> : IBqlUnary
		where TDocTypeField : IBqlField
		where TRefNbrField : IBqlField
	{
		private readonly IBqlCreator exists = new Exists<Select<
			APRegisterAlias2,
				Where<
					APRegisterAlias2.docType, Equal<Switch<Case<Where<TDocTypeField, Equal<APDocType.refund>>, APDocType.voidRefund>, APDocType.voidCheck>>,
					And<APRegisterAlias2.docType, NotEqual<TDocTypeField>,
					And<APRegisterAlias2.refNbr, Equal<TRefNbrField>,
					And<APRegisterAlias2.released, NotEqual<True>>>>>>>();

		public bool AppendExpression(ref SQLExpression exp, PXGraph graph, BqlCommandInfo info,
			BqlCommand.Selection selection)
			=> exists.AppendExpression(ref exp, graph, info, selection);
		
		public void Verify(PXCache cache, object item, List<object> pars, ref bool? result, ref object value)
		{
			string docType = cache.GetValue<TDocTypeField>(item) as string;
			string refNbr = cache.GetValue<TRefNbrField>(item) as string;

			value = result = Select(cache.Graph, APPaymentType.GetVoidingAPDocType(docType), docType, refNbr) != null;
		}

		public static bool Verify(PXGraph graph, APRegister payment)
		{
			bool? result = null;
			object value = null;

			new HasUnreleasedVoidPayment<APRegister.docType, APRegister.refNbr>().Verify(
				graph.Caches[payment.GetType()],
				payment,
				new List<object>(0),
				ref result,
				ref value);

			return result == true;
		}

		public static APRegister Select(PXGraph graph, APRegister payment)
		{
			if (payment == null || payment.RefNbr == null || payment.DocType == null)
			{
				return null;
			}
			return Select(graph, APPaymentType.GetVoidingAPDocType(payment.DocType), payment.DocType, payment.RefNbr);
		}

		[Obsolete("The method is obsolete. It will be removed in 2019R1. Please use Select(PXGraph , string , string , string ) instead.")]
		public static APRegister Select(PXGraph graph, string docType, string refNbr)
			=> Select(graph, APDocType.VoidCheck, docType, refNbr);

		public static APRegister Select(PXGraph graph, string voidDocType, string docType, string refNbr)
			=> PXSelect<
				APRegisterAlias2,
				Where<
					APRegisterAlias2.docType, Equal<Required<APRegister.docType>>,
					And<APRegisterAlias2.docType, NotEqual<Required<APRegister.docType>>,
					And<APRegisterAlias2.refNbr, Equal<Required<APRegister.refNbr>>,
					And<APRegisterAlias2.released, NotEqual<True>>>>>>
				.SelectWindowed(graph, 0, 1, voidDocType, docType, refNbr)
				.RowCast<APRegisterAlias2>()
				.FirstOrDefault();
	}
}
