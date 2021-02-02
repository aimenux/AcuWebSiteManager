using PX.Data;

namespace PX.Objects.FA
{
	public class FAInnerStateDescriptor
	{
		public static bool FixedAssetHasTransactions(int? assetID, PXGraph graph, string tranType, bool isReleased, string origin = null)
		{
			PXSelectBase<FATran> bql = new PXSelectReadonly<FATran,
				Where<FATran.assetID, Equal<Required<FixedAsset.assetID>>,
					And<FATran.tranType, Equal<Required<FATran.tranType>>>>>(graph);

			if (isReleased)
			{
				bql.WhereAnd<Where<FATran.released, Equal<True>>>();
			}
			else
			{
				bql.WhereAnd<Where<FATran.released, NotEqual<True>>>();
			}

			if (origin != null)
			{
				bql.WhereAnd<Where<FATran.origin, Equal<Required<FATran.origin>>>>();
				return bql.SelectSingle(assetID, tranType, origin) != null;
			}
			else
			{
				return bql.SelectSingle(assetID, tranType) != null;
			}
		}

		/// <summary>
		/// Defines that the fixed asset has been converted from the purchase.
		/// Such fixed asset has the unreleased R+ transactions and does not have released P+ transaction.
		/// </summary>
		public static bool IsConvertedFromAP(int? assetID, PXGraph graph)
		{
			return FixedAssetHasTransactions(assetID, graph, tranType: FATran.tranType.ReconciliationPlus, isReleased: false)
			       && !FixedAssetHasTransactions(assetID, graph, tranType: FATran.tranType.PurchasingPlus, isReleased: true);
		}

		/// <summary>
		/// Defines that the fixed asset will be transferred.
		/// Such fixed asset has the unreleased TP transactions.
		/// </summary>
		public static bool WillBeTransferred(int? assetID, PXGraph graph)
		{
			return FixedAssetHasTransactions(assetID, graph, tranType: FATran.tranType.TransferPurchasing, isReleased: false) ||
				FixedAssetHasTransactions(assetID, graph, tranType: FATran.tranType.TransferDepreciation, isReleased: false);
		}

		/// <summary>
		/// Defines that the fixed asset is acquired.
		/// Such fixed asset has the reased R+ transactions.
		/// </summary>
		public static bool IsAcquired(int? assetID, PXGraph graph)
		{
			return FixedAssetHasTransactions(assetID, graph, tranType: FATran.tranType.PurchasingPlus, isReleased: true);
		}
	}
}
