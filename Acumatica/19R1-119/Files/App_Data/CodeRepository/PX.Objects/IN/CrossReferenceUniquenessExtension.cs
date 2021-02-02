using System;
using System.Linq;
using PX.Common;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.AR;
using PX.Objects.CS;

namespace PX.Objects.IN
{
	public class CrossReferenceUniquenessAttribute : PXCheckUnique
	{
		public CrossReferenceUniquenessAttribute(params Type[] fields) : base(fields)
		{
			UniqueKeyIsPartOfPrimaryKey = true;
			IgnoreDuplicatesOnCopyPaste = true;
			ErrorMessage = Messages.AnotherItemAlreadyHasTheReferenceToThisAlternateID;
		}

		protected override String PrepareMessage(PXCache cache, Object currentRow, Object duplicateRow)
		{
			var xref = (INItemXRef)duplicateRow;
			var item = (InventoryItem)PXSelectorAttribute.Select<INItemXRef.inventoryID>(cache, xref);

			if (PXAccess.FeatureInstalled<FeaturesSet.subItem>() && item.StkItem == true)
			{
				var subitem = (INSubItem) PXSelectorAttribute.Select<INItemXRef.subItemID>(cache, xref);
				if (string.IsNullOrEmpty(subitem?.SubItemCD) == false)
					return PXMessages.LocalizeFormatNoPrefix(Messages.AnotherItemAlreadyHasTheReferenceToThisAlternateID, item.InventoryCD.TrimEnd(), subitem.SubItemCD);
			}

			return PXMessages.LocalizeFormatNoPrefix(Messages.AnotherItemAlreadyHasTheReferenceToThisAlternateID, item.InventoryCD.TrimEnd());
		}

		protected override Boolean CanClearError(String errorText)
		{
			return PXMessages
				.LocalizeFormatNoPrefix(Messages.AnotherItemAlreadyHasTheReferenceToThisAlternateID)
				.Split(new [] {"{0}"}, StringSplitOptions.None)
				.All(errorText.Contains);
		}

		[PXLocalizable]
		public static class Messages
		{
			public const string AnotherItemAlreadyHasTheReferenceToThisAlternateID = "The record cannot be saved because the alternate ID specified for the selected item is already assigned to another inventory item ({0}). Please specify another alternate ID.";
			public const string AnotherSubItemAlreadyHasTheReferenceToThisAlternateID = "The record cannot be saved because the alternate ID specified for the selected item is already assigned to another inventory item ({0}, subitem: {1}). Please specify another alternate ID.";
		}
	}

	/// <summary>
	/// Extension that enforces uniqueness of a newly entered Alternate ID for an inventory item <see cref="InventoryItem"/>
	/// </summary>
	[Serializable]
	public class CrossReferenceUniqueness<TGraph> : PXGraphExtension<TGraph> where TGraph : PXGraph
	{
		protected static bool IsActiveImpl() => PXAccess.FeatureInstalled<FeaturesSet.crossReferenceUniqueness>();

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[CrossReferenceUniqueness(typeof(INItemXRef.bAccountID), typeof(INItemXRef.alternateType))]
		protected void INItemXRef_AlternateID_CacheAttached(PXCache sender) { }
	}

	[Serializable]
	public class CrossReferenceUniquenessForStockItemExtension : CrossReferenceUniqueness<InventoryItemMaint>
	{
		public static bool IsActive() => IsActiveImpl();
	}

	[Serializable]
	public class CrossReferenceUniquenessForNonStockItemExtension : CrossReferenceUniqueness<NonStockItemMaint>
	{
		public static bool IsActive() => IsActiveImpl();
	}


	/// <summary>
	/// Extension that enforces uniqueness of a newly entered Alternate ID for an inventory item <see cref="InventoryItem"/>
	/// </summary>
	[Serializable]
	public class CrossReferenceUniquenessPriceWorksheet<TGraph> : PXGraphExtension<TGraph> where TGraph : PXGraph
	{
		protected static bool IsActiveImpl() => PXAccess.FeatureInstalled<FeaturesSet.crossReferenceUniqueness>();

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[CrossReferenceUniqueness(typeof(INNonStockItemXRef.bAccountID), typeof(INNonStockItemXRef.alternateType))]
		protected void INNonStockItemXRefHandler(Events.CacheAttached<INNonStockItemXRef.alternateID> e) { }

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[CrossReferenceUniqueness(typeof(INStockItemXRef.bAccountID), typeof(INStockItemXRef.alternateType))]
		protected void INStockItemXRefHandler(Events.CacheAttached<INStockItemXRef.alternateID> e) { }
	}

	[Serializable]
	public class CrossReferenceUniquenessForARPriceWorksheetExtension : CrossReferenceUniquenessPriceWorksheet<ARPriceWorksheetMaint>
	{
		public static bool IsActive() => IsActiveImpl();
	}

	[Serializable]
	public class CrossReferenceUniquenessForAPPriceWorksheetExtension : CrossReferenceUniquenessPriceWorksheet<APPriceWorksheetMaint>
	{
		public static bool IsActive() => IsActiveImpl();
	}
}