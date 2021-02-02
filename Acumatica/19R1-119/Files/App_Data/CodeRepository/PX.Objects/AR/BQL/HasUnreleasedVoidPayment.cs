using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

using PX.Data;
using PX.Data.SQLTree;
using PX.Objects.Common;
using PX.Objects.AR.Standalone;

namespace PX.Objects.AR.BQL
{
    /// <summary>
    /// A predicate that returns <c>true</c> if and only if the payment defined
    /// by its key fields (document type and reference number) has an unreleased 
    /// void payment. This may be needed to exclude such payments from processing
    /// to prevent creating unnecessary applications, see e.g. the
    /// <see cref="ARAutoApplyPayments"/> graph.
    /// </summary>
    public class HasUnreleasedVoidPayment<TDocTypeField, TRefNbrField> : IBqlUnary
		where TDocTypeField : IBqlOperand
		where TRefNbrField : IBqlOperand
    {
		private readonly IBqlCreator exists = new Exists<Select<
			ARRegisterAlias2,
				Where<
					ARRegisterAlias2.docType, Equal<Switch<Case<Where<TDocTypeField, Equal<ARDocType.refund>>, ARDocType.voidRefund>, ARDocType.voidPayment>>,
					And<ARRegisterAlias2.docType, NotEqual<TDocTypeField>,
					And<ARRegisterAlias2.refNbr, Equal<TRefNbrField>,
					And<ARRegisterAlias2.released, NotEqual<True>>>>>>>();

		public bool AppendExpression(ref SQLExpression exp, PXGraph graph, BqlCommandInfo info, BqlCommand.Selection selection)
			=> exists.AppendExpression(ref exp, graph, info, selection);

		private string GetFieldName<T>()
		{
			if (typeof(IBqlField).IsAssignableFrom(typeof(T)))
			{
				return typeof(T).Name;
			}
			else if (typeof(IBqlParameter).IsAssignableFrom(typeof(T)))
			{
				return (Activator.CreateInstance<T>() as IBqlParameter).GetReferencedType().Name;
			}
			else
				return null;
		}

        public void Verify(PXCache cache, object item, List<object> pars, ref bool? result, ref object value)
        {
			string docType = cache.GetValue(item, GetFieldName<TDocTypeField>()) as string;
			string refNbr = cache.GetValue(item, GetFieldName<TRefNbrField>()) as string;

			value = result = Select(cache.Graph, ARPaymentType.GetVoidingARDocType(docType), docType, refNbr) != null;
        }

        public static bool Verify(PXGraph graph, ARRegister payment)
        {
            bool? result = null;
            object value = null;

            new HasUnreleasedVoidPayment<ARRegister.docType, ARRegister.refNbr>().Verify(
                graph.Caches[payment.GetType()],
                payment,
                new List<object>(0),
                ref result,
                ref value);

            return result == true;
        }

		public static ARRegister Select(PXGraph graph, ARRegister payment)
		{
			if (payment == null || payment.RefNbr == null || payment.DocType == null)
			{
				return null;
			}
			return Select(graph, ARPaymentType.GetVoidingARDocType(payment.DocType), payment.DocType, payment.RefNbr);
		}

		[Obsolete("The method is obsolete. It will be removed in 2019R1. Please use Select(PXGraph , string , string , string ) instead.")]
		public static ARRegister Select(PXGraph graph, string docType, string refNbr)
			=> Select(graph, ARDocType.VoidPayment, docType, refNbr);

		public static ARRegister Select(PXGraph graph, string voidDocType, string docType, string refNbr)
            => PXSelect<
                ARRegisterAlias2,
                Where<
                    ARRegisterAlias2.docType, Equal<Required<ARRegister.docType>>,
                    And<ARRegisterAlias2.docType, NotEqual<Required<ARRegister.docType>>,
                    And<ARRegisterAlias2.refNbr, Equal<Required<ARRegister.refNbr>>,
                    And<ARRegisterAlias2.released, NotEqual<True>>>>>>
                .SelectWindowed(graph, 0, 1, voidDocType, docType, refNbr)
                .RowCast<ARRegisterAlias2>()
                .FirstOrDefault();
    }
}
