using System.Collections.Generic;
using PX.Data;
using PX.Data.BQL;
using PX.Data.SQLTree;

namespace PX.Objects.CR
{
	interface IBqlEntryStatus :
		IBqlDataType,
		IBqlEquitable,
		IBqlCastableTo<IBqlEntryStatus>
	{ }
	abstract class BqlEntryStatus : BqlType<IBqlEntryStatus, PXEntryStatus> { private BqlEntryStatus() { } }

	class EntryStatus: BqlEntryStatus.Operand<EntryStatus>, IBqlCreator
	{
		public void Verify(PXCache cache, object item, List<object> pars, ref bool? result, ref object value)
		{
			value = cache.InternalCurrent != null ? cache.GetStatus(cache.InternalCurrent) : PXEntryStatus.Notchanged;
		}

		public bool AppendExpression(ref SQLExpression exp, PXGraph graph, BqlCommandInfo info, BqlCommand.Selection selection)
		{
			return true;
		}

		public sealed class inserted : BqlEntryStatus.Constant<inserted>
		{
			public inserted() : base(PXEntryStatus.Inserted) { }
		}
		public sealed class updated : BqlEntryStatus.Constant<updated>
		{
			public updated() : base(PXEntryStatus.Updated) { }
		}
		public sealed class deleted : BqlEntryStatus.Constant<deleted>
		{
			public deleted() : base(PXEntryStatus.Deleted) { }
		}
		public sealed class notchanged : BqlEntryStatus.Constant<notchanged>
		{
			public notchanged() : base(PXEntryStatus.Notchanged) { }
		}
	}
}
