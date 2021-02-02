using System;
using System.Collections.Generic;
using System.Text;

using PX.Data;
using System.Linq;
using PX.Data.SQLTree;

namespace PX.Objects.AP.BQL
{
	/// <summary>
	/// A predicate that returns <c>true</c> if and only if the
	/// <see cref="APRegister"/> descendant record (as determined
	/// by the pair of document type and reference number fields)
	/// has any lines linked to a PO bill.
	/// </summary>
	public class IsPOLinked<TDocTypeField, TRefNbrField> : IBqlUnary
		where TDocTypeField : IBqlField
		where TRefNbrField : IBqlField
	{
		private readonly IBqlCreator exists = new Exists<Select<
			APTran,
			Where<
				APTran.tranType, Equal<TDocTypeField>,
				And<APTran.refNbr, Equal<TRefNbrField>,
				And<Where<
					APTran.pONbr, IsNotNull,
					Or<APTran.pOLineNbr, IsNotNull,
					Or<APTran.receiptNbr, IsNotNull,
					Or<APTran.receiptLineNbr, IsNotNull>>>>>>>>>();

		public bool AppendExpression(ref SQLExpression exp, PXGraph graph, BqlCommandInfo info,
			BqlCommand.Selection selection)
			=> exists.AppendExpression(ref exp, graph, info, selection);

		public void Verify(PXCache cache, object item, List<object> pars, ref bool? result, ref object value)
		{
			string docType = cache.GetValue<TDocTypeField>(item) as string;
			string refNbr = cache.GetValue<TRefNbrField>(item) as string;

			value = result = PXSelect<
				APTran,
				Where<
					APTran.tranType, Equal<Required<APTran.tranType>>,
					And<APTran.refNbr, Equal<Required<APTran.refNbr>>,
					And<Where<
						APTran.pONbr, IsNotNull,
						Or<APTran.pOLineNbr, IsNotNull,
						Or<APTran.receiptNbr, IsNotNull,
						Or<APTran.receiptLineNbr, IsNotNull>>>>>>>>
				.SelectWindowed(cache.Graph, 0, 1, docType, refNbr)
				.RowCast<APTran>()
				.Any();
		}
				
		public static bool Verify(PXGraph graph, APRegister document)
		{
			bool? result = null;
			object value = null;

			new IsPOLinked<TDocTypeField, TRefNbrField>().Verify(
				graph.Caches[typeof(APRegister)],
				document,
				new List<object>(),
				ref result,
				ref value);

			return result == true;
		}
	}
}
