using System;
using PX.Objects.IN;

namespace PX.Objects.GL.ConsolidationImport
{
	public class SubOffImportSubaccountMapper : IImportSubaccountMapper
	{
		private readonly Func<int?> _tryGetDefaultSubID;

		public SubOffImportSubaccountMapper(Func<int?> tryGetDefaultSubID)
		{
			_tryGetDefaultSubID = tryGetDefaultSubID;
		}

		public Sub.Keys GetMappedSubaccountKeys(string subaccountCD)
		{
			return new Sub.Keys()
			{
				SubCD = string.Empty,
				SubID = _tryGetDefaultSubID()
			};
		}
	}
}
