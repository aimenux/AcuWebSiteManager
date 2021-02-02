using System;
using System.Collections.Generic;
using System.Text;

using PX.Data;
using PX.Data.SQLTree;

namespace PX.Objects.AR.BQL
{
	/// <summary>
	/// A BQL function that returns <see cref="ARDocType.SignAmount(string)"/>
	/// </summary>
	public sealed class SignAmount<TDocType> : BqlFunction, IBqlOperand, IBqlCreator
		where TDocType : IBqlOperand
	{
		private IBqlCreator _source;

		/// <exclude/>
		public void Verify(PXCache cache, object item, List<object> pars, ref bool? result, ref object value)
		{
			if (!getValue<TDocType>(ref _source, cache, item, pars, ref result, out value) || value == null)
				return;
			value = ARDocType.SignAmount((string)value);
		}

		/// <exclude />
		public bool AppendExpression(ref SQLExpression exp, PXGraph graph, BqlCommandInfo info, BqlCommand.Selection selection) => true;
	}
}
