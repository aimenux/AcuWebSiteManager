using System;
using System.Collections.Generic;

namespace PX.Objects.Common.Discount
{
	/// <summary>
	/// Used for calculation of intersections of discounts. Every new value should be power of two.
	/// </summary>
	[Flags]
	public enum ApplicableToCombination
	{
		None = 0,
		Customer = 1,
		InventoryItem = 2,
		CustomerPriceClass = 4,
		InventoryPriceClass = 8,
		Vendor = 16,
		Warehouse = 32,
		Branch = 64,
		Location = 128,
		Unconditional = 256
	}

	/// <summary>
	/// Stores Discount Codes. Both AP and AR. IsVendorDiscount should be true for AP discounts.
	/// </summary>
	public struct DiscountCode
	{
		public bool IsVendorDiscount;
		public int? VendorID;
		public string Type;
		public ApplicableToCombination ApplicableToEnum;
		public bool IsManual;
		public bool ExcludeFromDiscountableAmt;
		public bool SkipDocumentDiscounts;
	}

	/// <summary>
	/// Stores discount sequence keys
	/// </summary>
	public class DiscountSequenceKey
	{
		public string DiscountID { get; }
		public string DiscountSequenceID { get; }
		public decimal? CuryDiscountableAmount { get; set; }
		public decimal? DiscountableQuantity { get; set; }

		internal DiscountSequenceKey(string discountID, string discountSequenceID)
		{
			this.DiscountID = discountID;
			this.DiscountSequenceID = discountSequenceID;
			this.CuryDiscountableAmount = 0m;
			this.DiscountableQuantity = 0m;
		}

		public override bool Equals(object obj)
		{
			if (obj == null || GetType() != obj.GetType())
				return false;

			DiscountSequenceKey newSequenceKey = obj as DiscountSequenceKey;
			return newSequenceKey.DiscountID == this.DiscountID && newSequenceKey.DiscountSequenceID == this.DiscountSequenceID;
		}

		public override int GetHashCode()
		{
			int hashCode = 17;
			hashCode = (hashCode * 11) + DiscountID?.GetHashCode() ?? 0;
			hashCode = (hashCode * 11) + DiscountSequenceID?.GetHashCode() ?? 0;
			return hashCode;
		}

		public class DiscountSequenceKeyComparer : IEqualityComparer<DiscountSequenceKey>
		{
			public bool Equals(DiscountSequenceKey discountSequenceKey1, DiscountSequenceKey discountSequenceKey2)
			{
				return discountSequenceKey1.DiscountID == discountSequenceKey2.DiscountID && discountSequenceKey1.DiscountSequenceID == discountSequenceKey2.DiscountSequenceID;
			}

			public int GetHashCode(DiscountSequenceKey discountSequenceKey)
			{
				int hashCode = 17;
				hashCode = (hashCode * 11) + discountSequenceKey.DiscountID?.GetHashCode() ?? 0;
				hashCode = (hashCode * 11) + discountSequenceKey.DiscountSequenceID?.GetHashCode() ?? 0;
				return hashCode;
			}
		}
	}

	/// <summary>
	/// Stores Discount Details lines combined with Discount Code and Discount Sequence fields
	/// </summary>
	public struct DiscountDetailLine
	{
		public string DiscountID;
		public string DiscountSequenceID;
		public ApplicableToCombination ApplicableToCombined;
		public string Type; // L, G or P
		public string DiscountedFor; // A or P or F
		public string BreakBy;
		public decimal? AmountFrom; //Amount or Quantity
		public decimal? AmountTo; //Amount or Quantity
		public decimal? Discount; //Amount or Percent
		public int? freeItemID;
		public decimal? freeItemQty;
		public bool? Prorate;
		public string ExtDiscCode;
		public string Description;
	}

	/// <summary>
	/// Used in group discounts only
	/// </summary>
	public struct DiscountableValues
	{
		public decimal? CuryDiscountableAmount;
		public decimal? DiscountableQuantity;
	}

	public struct DiscountResult
	{
		/// <summary>
		/// Gets Discount. Its either a Percent or an Amount. Check <see cref="IsAmount"/> property.
		/// </summary>
		public decimal? Discount { get; }

		/// <summary>
		/// Returns true if Discount is an Amount; otherwise false.
		/// </summary>
		public bool IsAmount { get; }

		/// <summary>
		/// Returns True if No Discount was found.
		/// </summary>
		public bool IsEmpty => Discount == null || Discount == 0m;

		internal DiscountResult(decimal? discount, bool isAmount)
		{
			this.Discount = discount;
			this.IsAmount = isAmount;
		}
	}

	public struct UnitPriceVal
	{
		public decimal? CuryUnitPrice;
		public bool isBAccountSpecific;
		public bool isPriceClassSpecific;
		public bool isPromotional;
		public bool skipLineDiscount;

		public UnitPriceVal(bool skipLineDiscount)
		{
			this.CuryUnitPrice = 0m;
			this.isBAccountSpecific = false;
			this.isPriceClassSpecific = false;
			this.isPromotional = false;
			this.skipLineDiscount = skipLineDiscount;
		}
	}
}