using System;
using System.Collections.Generic;

namespace PX.Objects.PR.AUF
{
	public class PimRecord : AufRecord
	{
		public PimRecord(string itemID) : base(AufRecordType.Pim)
		{
			Title = itemID;
			PimID = Math.Abs(itemID.GetHashCode());
		}

		public PimRecord(string itemID, int pimID) : base(AufRecordType.Pim)
		{
			Title = itemID;
			PimID = pimID;
		}

		public override string ToString()
		{
			object[] lineData =
			{
				Title,
				PimID,
				Description,
				AatrixTaxType,
				State,
				AufConstants.UnusedField,
				AccountNumber,
				AufConstants.UnusedField
			};

			return FormatLine(lineData);
		}

		public virtual string Title { get; set; }
		public virtual int PimID { get; set; }
		public virtual string Description { get; set; }
		public virtual int? AatrixTaxType { get; set; }
		public virtual string State { get; set; }
		public virtual string AccountNumber { get; set; }
	}

	public class PimRecordComparer : IEqualityComparer<PimRecord>
	{
		public bool Equals(PimRecord x, PimRecord y)
		{
			return x.PimID == y.PimID;
		}

		public int GetHashCode(PimRecord obj)
		{
			return obj.PimID;
		}
	}
}
