using System;
using System.Collections.Generic;
using PX.Data;
using PX.Objects.Common;

namespace PX.Objects.CM
{
	[Serializable]
	public class CMReportTranType : PX.Data.IBqlTable
	{
		#region TranType

		public class tranType : PX.Data.BQL.BqlString.Field<tranType>, ILabelProvider
		{
			public const string Revalue = "REV";

			public IEnumerable<ValueLabelPair> ValueLabelPairs => new ValueLabelList
			{
				{Revalue, Messages.Revalue},
			};

			public class revalue : PX.Data.BQL.BqlString.Constant<revalue>
			{
				public revalue() : base(Revalue) { }
			}
		}

		[PXString(3, IsFixed = true)]
		[LabelList(typeof(tranType))]
		public virtual string TranType { get; set; }
		#endregion
	}
}
