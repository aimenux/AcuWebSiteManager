using System;
using System.Collections.Generic;
using PX.Data;
using PX.Objects.Common;


namespace PX.Objects.GL
{
	public class GLTranType : ILabelProvider
	{
		protected static readonly IEnumerable<ValueLabelPair> ValueLabelList = new ValueLabelList
		{
			{ GLEntry, CA.Messages.GLEntry}
		};

		public IEnumerable<ValueLabelPair> ValueLabelPairs => ValueLabelList;

		public const string GLEntry = "GLE";

		public class gLEntry : PX.Data.BQL.BqlString.Constant<gLEntry>
		{
			public gLEntry() : base(GLEntry) { }
		}
	}
}