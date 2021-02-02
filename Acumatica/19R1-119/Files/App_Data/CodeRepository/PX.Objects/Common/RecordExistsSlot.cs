using PX.Common;
using PX.Data;

namespace PX.Objects.Common
{
	[PXInternalUseOnly]
	public class RecordExistsSlot<Table, KeyField, Where> : IPrefetchable
		where Table : class, IBqlTable, new()
		where KeyField : IBqlField
		where Where : IBqlWhere, new()
	{
		public bool RecordExists { get; private set; }

		public void Prefetch()
		{
			var tempGraph = PXGraph.CreateInstance<PXGraph>();

			PXSelectBase<Table> select = CreateSelect(tempGraph);

			using (new PXFieldScope(select.View, typeof(KeyField)))
				RecordExists = ((Table)select.SelectSingle()) != null;
		}

		protected virtual PXSelectBase<Table> CreateSelect(PXGraph tempGraph)
			=> new PXSelectReadonly<Table, Where>(tempGraph);

		public static bool IsRowsExists()
		{
			string slotName = typeof(RecordExistsSlot<Table, KeyField, Where>).ToString();
			var slot = PXDatabase.GetSlot<RecordExistsSlot<Table, KeyField, Where>>(slotName, typeof(Table));

			return slot.RecordExists;
		}
	}

	public class RecordExistsSlot<Table, KeyField> : RecordExistsSlot<Table, KeyField, Where<True, Equal<True>>>
		where Table : class, IBqlTable, new()
		where KeyField : IBqlField
	{
		protected override PXSelectBase<Table> CreateSelect(PXGraph tempGraph)
			=> new PXSelectReadonly<Table>(tempGraph);
	}
}
