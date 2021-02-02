using PX.Data;
using PX.Objects.CS;

namespace PX.Objects.CN.Common.Services
{
	public class NumberingSequenceUsage : INumberingSequenceUsage
	{
		public void CheckForNumberingUsage<TSetup, TNumberingId>(Numbering numbering,
			PXGraph graph, string message)
			where TSetup : class, IBqlTable, new()
		{
			if (numbering == null)
			{
				return;
			}
			var setupSelect = new PXSelect<TSetup>(graph);
			var numberingTypeName = typeof(TNumberingId).Name;
			if (IsNumberingInUse(setupSelect, numberingTypeName, numbering))
			{
				var fieldName = PXUIFieldAttribute.GetDisplayName(setupSelect.Cache, numberingTypeName);
				throw new PXException(Messages.NumberingIsUsedFailedDeleteSetup, message, fieldName);
			}
		}

		private static bool IsNumberingInUse(PXSelectBase setupSelect, string numberingType,
			Numbering numbering)
		{
			var setup = setupSelect.View.SelectSingle(numbering.NumberingID);
			return setup != null && (string)setupSelect.Cache.GetValue(setup, numberingType) ==
			       numbering.NumberingID;
		}
	}
}