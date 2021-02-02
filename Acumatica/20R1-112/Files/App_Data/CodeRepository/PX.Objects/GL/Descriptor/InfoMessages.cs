using PX.Common;

namespace PX.Objects.GL
{
	[PXLocalizable]
	public static class InfoMessages
	{
		public const string SomeTransactionsCannotBeReclassified = "Some transactions that match the specified selection criteria cannot be reclassified. These transactions will not be loaded.";
		public const string NoReclassifiableTransactionsHaveBeenFoundToMatchTheCriteria = "No transactions, for which the reclassification can be performed, have been found to match the specified criteria.";
		public const string NoReclassifiableTransactionsHaveBeenSelected = "No transactions, for which the reclassification can be performed, have been selected.";
		public const string SomeTransactionsOfTheBatchCannotBeReclassified = "Some transactions of the batch cannot be reclassified. These transactions will not be loaded.";
		public const string NoReclassifiableTransactionsHaveBeenFoundInTheBatch = "No transactions, for which the reclassification can be performed, have been found in the batch.";
		public const string TransactionsListedOnTheFormIfAnyWillBeRemoved = "Transactions listed on the form (if any) will be removed. New transactions that match the selection criteria will be loaded. Do you want to continue?";
	}
}
