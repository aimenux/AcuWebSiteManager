using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using PX.Data;
using PX.Data.SQLTree;
using PX.Objects.CS;

namespace PX.Objects.GL.BQL
{
	/// <summary>
	/// A BQL predicate returning <c>true</c> if and only if there exists a 
	/// <see cref="GLVoucher">journal voucher</see> referencing the entity's
	/// note ID field by its <see cref="GLVoucher.RefNoteID"/> field.
	/// </summary>
	public class ExistsJournalVoucher<TNoteIDField> : IBqlUnary
		where TNoteIDField : IBqlField
	{
		private readonly IBqlCreator exists = new Exists<Select<
			GLVoucher,
			Where2<
				FeatureInstalled<FeaturesSet.gLWorkBooks>,
				And<TNoteIDField, Equal<GLVoucher.refNoteID>>>>>();

		public bool AppendExpression(ref SQLExpression exp, PXGraph graph, BqlCommandInfo info, BqlCommand.Selection selection)
			=> exists.AppendExpression(ref exp, graph, info, selection);
	
		public void Verify(PXCache cache, object item, List<object> pars, ref bool? result, ref object value)
		{
			Guid? noteID = cache.GetValue<TNoteIDField>(item) as Guid?;

			value = result = PXSelect<
				GLVoucher,
				Where2<
					FeatureInstalled<FeaturesSet.gLWorkBooks>,
					And<GLVoucher.refNoteID, Equal<Required<GLVoucher.refNoteID>>>>>
				.SelectWindowed(cache.Graph, 0, 1, noteID)
				.RowCast<GLVoucher>()
				.Any();
		}
	}
}
