using PX.Data;

namespace PX.Objects.FA
{
	public static class FixedAssetClassExtension
	{
		/// <summary>
		/// Defines that the fixed asset has been converted from the purchase.
		/// Such fixed asset has the unreleased R+ transactions and does not have released P+ transaction
		/// </summary>
		public static bool IsConvertedFromAP(this FixedAsset asset, PXGraph graph)
		{
			return FAInnerStateDescriptor.IsConvertedFromAP(asset.AssetID, graph);
		}

		/// <summary>
		/// Defines that the fixed asset will be transferred.
		/// Such fixed asset has the unreleased TP transactions.
		/// </summary>
		public static bool WillBeTransferred(this FixedAsset asset, PXGraph graph)
		{
			return FAInnerStateDescriptor.WillBeTransferred(asset.AssetID, graph);
		}

		/// <summary>
		/// Defines that the fixed asset is acquired.
		/// Such fixed asset has the reased R+ transactions.
		/// </summary>
		public static bool IsAcquired(int? assetID, PXGraph graph)
		{
			return FAInnerStateDescriptor.IsAcquired(assetID, graph);
		}

	}
}
