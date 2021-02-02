namespace PX.Objects.AP
{
	public class AdjustmentGroupKey
	{
		public class AdjustmentType
		{
			public const string APAdjustment = "A";
			public const string POAdjustment = "P";
			public const string OutstandingBalance = "T";

			public class aPAdjustment : PX.Data.BQL.BqlString.Constant<aPAdjustment>
			{
				public aPAdjustment() : base(APAdjustment) {}
			}
			public class pOAdjustment : PX.Data.BQL.BqlString.Constant<pOAdjustment>
			{
				public pOAdjustment() : base(POAdjustment) { }
			}

			public class outstandingBalance : PX.Data.BQL.BqlString.Constant<outstandingBalance>
			{
				public outstandingBalance() : base(OutstandingBalance) { }
			}

		}

		public string Source { get; set; }
		public string AdjdDocType { get; set; }
		public string AdjdRefNbr { get; set; }
		public long? AdjdCuryInfoID { get; set; }

		public override int GetHashCode() => (Source, AdjdDocType, AdjdRefNbr).GetHashCode();

		public override bool Equals(object obj)
		{
			if (obj is AdjustmentGroupKey key)
				return Source == key.Source && AdjdDocType == key.AdjdDocType && AdjdRefNbr == key.AdjdRefNbr;

			return base.Equals(obj);
		}
	}
}
