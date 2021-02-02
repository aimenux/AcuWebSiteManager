using PX.Data;

namespace PX.Objects.PR
{
	public static class EarningDetailHelper
	{
		public static void CopySelectedEarningDetailRecord(PXCache cache)
		{
			PREarningDetail currentRecord = cache.Current as PREarningDetail;
			if (currentRecord?.AllowCopy != true)
				return;
			
			PREarningDetail newRecord = CreateEarningDetailCopy(cache, currentRecord);
			cache.Update(newRecord);
		}

		public static PREarningDetail CreateEarningDetailCopy(PXCache cache, PREarningDetail originalEarning)
		{
			// To ensure a true copy of all fields and avoid having fields defaulted in the Insert
			// or Update:
			// 1. Restore copy of original earning to new record
			// 2. Insert new record
			// 3. Restore copy of original record to the inserted new record again
			PREarningDetail copy = new PREarningDetail();
			cache.RestoreCopy(copy, originalEarning);
			copy.RecordID = null;
			copy = (PREarningDetail)cache.Insert(copy);

			int? copyRecordID = copy.RecordID;
			cache.RestoreCopy(copy, originalEarning);

			copy.RecordID = copyRecordID;
			return copy;
		}
	}
}
