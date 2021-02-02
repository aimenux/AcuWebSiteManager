using PX.Data;

using System.Collections.Generic;

namespace PX.Objects.Common.DataIntegrity
{
	public class InconsistencyCode : ILabelProvider
	{
		public const string UnreleasedDocumentHasGlTransactions = "UNRELDOCHASGL";
		public const string ReleasedDocumentHasNoGlTransactions = "RELDOCHASNOGL";
		public const string BatchTotalNotEqualToTransactionTotal = "BATCHTRANTOTALMISMATCH";
		public const string ReleasedDocumentHasUnreleasedApplications = "RELDOCUNRELADJUST";
		public const string UnreleasedDocumentHasReleasedApplications = "UNRELDOCRELADJUST";
		public const string DocumentTotalsWrongPrecision = "DOCTOTALSPRECISION";
		public const string DocumentNegativeBalance = "DOCNEGATIVEBALANCE";

		public IEnumerable<ValueLabelPair> ValueLabelPairs => new ValueLabelList
		{
			{ UnreleasedDocumentHasGlTransactions, Messages.DataIntegrityGLBatchExistsForUnreleasedDocument },
			{ ReleasedDocumentHasNoGlTransactions, Messages.DataIntegrityGLBatchNotExistsForReleasedDocument },
			{ BatchTotalNotEqualToTransactionTotal, Messages.DataIntegrityGLBatchSumsNotEqualGLTransSums },
			{ ReleasedDocumentHasUnreleasedApplications, Messages.DataIntegrityReleasedDocumentWithUnreleasedApplications },
			{ UnreleasedDocumentHasReleasedApplications, Messages.DataIntegrityUnreleasedDocumentWithReleasedApplications },
			{ DocumentTotalsWrongPrecision, Messages.DataIntegrityDocumentTotalsHaveLargerPrecisionThanCurrency },
			{ DocumentNegativeBalance, Messages.DataIntegrityDocumentHasNegativeBalance },
		};

		public class unreleasedDocumentHasGlTransactions : PX.Data.BQL.BqlString.Constant<unreleasedDocumentHasGlTransactions>
		{
			public unreleasedDocumentHasGlTransactions() 
				: base(UnreleasedDocumentHasGlTransactions)
			{ }
		}

		public class releasedDocumentHasNoGlTransactions : PX.Data.BQL.BqlString.Constant<releasedDocumentHasNoGlTransactions>
		{
			public releasedDocumentHasNoGlTransactions()
				: base(ReleasedDocumentHasNoGlTransactions)
			{ }
		}

		public class batchTotalNotEqualToTransactionTotal : PX.Data.BQL.BqlString.Constant<batchTotalNotEqualToTransactionTotal>
		{
			public batchTotalNotEqualToTransactionTotal()
				: base(BatchTotalNotEqualToTransactionTotal)
			{ }
		}

		public class releasedDocumentHasUnreleasedApplications : PX.Data.BQL.BqlString.Constant<releasedDocumentHasUnreleasedApplications>
		{
			public releasedDocumentHasUnreleasedApplications()
				: base(ReleasedDocumentHasUnreleasedApplications)
			{ }
		}

		public class unreleasedDocumentHasReleasedApplications : PX.Data.BQL.BqlString.Constant<unreleasedDocumentHasReleasedApplications>
		{
			public unreleasedDocumentHasReleasedApplications()
				: base(UnreleasedDocumentHasReleasedApplications)
			{ }
		}

		public class documentTotalsWrongPrecision : PX.Data.BQL.BqlString.Constant<documentTotalsWrongPrecision>
		{
			public documentTotalsWrongPrecision()
				: base(DocumentTotalsWrongPrecision)
			{ }
		}

		public class documentNegativeBalance : PX.Data.BQL.BqlString.Constant<documentNegativeBalance>
		{
			public documentNegativeBalance()
				: base(DocumentNegativeBalance)
			{ }
		}
	}
}
