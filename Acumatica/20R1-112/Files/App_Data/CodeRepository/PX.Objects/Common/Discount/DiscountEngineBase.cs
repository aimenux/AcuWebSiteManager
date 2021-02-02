using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using PX.Data;

using PX.Objects.AR;
using PX.Objects.AP;
using PX.Objects.IN;
using PX.Objects.CS;
using PX.Objects.CR;
using PX.Objects.CM;
using PX.Common;
using PX.Data.BQL;
using PX.Objects.Common.Discount.Mappers;
using PX.Objects.Common.Extensions;

namespace PX.Objects.Common.Discount
{
	public abstract class DiscountEngine : PXGraph<DiscountEngine>
	{
		#region Constants
		private const string InventoryPriceClassesSlotName = "CachedDEInventoryPriceClasses";
		private const string CustomerPriceClassIDSlotName = "CachedDECustomerPriceClassID";
		#endregion

		#region DiscountDetailLine-related functions

		/// <summary>
		/// Returns single best discount details line for a given Entities set
		/// </summary>
		/// <param name="cache">Cache</param>
		/// <param name="dline">Discount-related fields</param>
		/// <param name="discountType">Line or Document</param>
		protected virtual DiscountDetailLine SelectBestDiscount(
			PXCache cache,
			DiscountLineFields dLine,
			HashSet<KeyValuePair<object, string>> entities,
			string discountType,
			decimal curyAmount,
			decimal quantity,
			DateTime date)
		{
			GetDiscountTypes();
			return entities != null
				? SelectApplicableDiscounts(cache, dLine, SelectApplicableEntityDiscounts(entities, discountType, true), curyAmount, quantity, discountType, date).FirstOrDefault()
				: new DiscountDetailLine();
		}

		/// <summary>
		/// Returns single DiscountDetails line on a given DiscountSequenceKey
		/// </summary>
		/// <param name="discountSequence">Applicable Discount Sequence</param>
		/// <param name="curyDiscountableAmount">Discountable amount</param>
		/// <param name="discountableQuantity">Discountable quantity</param>
		/// <param name="discountType">Discount type: line, group or document</param>
		protected virtual DiscountDetailLine SelectApplicableDiscount(
			PXCache cache,
			DiscountLineFields dLine,
			DiscountSequenceKey discountSequence,
			decimal curyDiscountableAmount,
			decimal discountableQuantity,
			string discountType,
			DateTime date)
			=> SelectApplicableDiscounts(cache, dLine, new HashSet<DiscountSequenceKey> {discountSequence}, curyDiscountableAmount, discountableQuantity, discountType, date).FirstOrDefault();

		/// <summary>
		/// Returns single DiscountDetails line. Accepts HashSet of DiscountSequenceKey
		/// </summary>
		/// <param name="discountSequences">Applicable Discount Sequences</param>
		/// <param name="curyDiscountableAmount">Discountable amount</param>
		/// <param name="discountableQuantity">Discountable quantity</param>
		/// <param name="discountType">Discount type: line, group or document</param>
		protected virtual DiscountDetailLine SelectApplicableDiscount(
			PXCache cache,
			DiscountLineFields dLine,
			HashSet<DiscountSequenceKey> discountSequences,
			decimal curyDiscountableAmount,
			decimal discountableQuantity,
			string discountType,
			DateTime date)
			=> SelectApplicableDiscounts(cache, dLine, discountSequences, curyDiscountableAmount, discountableQuantity, discountType, date).FirstOrDefault();

		/// <summary>
		/// Returns all Discount Sequences applicable to a given entities set.
		/// </summary>
		/// <param name="entities">Entities dictionary</param>
		/// <param name="discountType">Line, Group or Document</param>
		/// <returns></returns>
		protected virtual HashSet<DiscountSequenceKey> SelectApplicableEntityDiscounts(
			HashSet<KeyValuePair<object, string>> entities,
			string discountType,
			bool skipManual)
		{
			ConcurrentDictionary<string, DiscountCode> cachedDiscountTypes = GetCachedDiscountCodes();
			var applicableDiscounts = new HashSet<DiscountSequenceKey>();
			var allFoundDiscounts = new HashSet<DiscountSequenceKey>();
			var applicableDiscountsByEntity = new Dictionary<ApplicableToCombination, ImmutableHashSet<DiscountSequenceKey>>();

			int? vendorID = (int?)entities.LastOrDefault(e => e.Value == DiscountTarget.Vendor).Key;
			bool isARDiscount = vendorID == null;

			foreach (KeyValuePair<object, string> entity in entities)
			{
				var applicableEntityDiscounts = ImmutableHashSet.Create<DiscountSequenceKey>();

				if (isARDiscount && PXAccess.FeatureInstalled<FeaturesSet.customerDiscounts>())
				{
					applicableEntityDiscounts = GetApplicableEntityARDiscounts(entity);
				}
				else if (PXAccess.FeatureInstalled<FeaturesSet.vendorDiscounts>())
				{
					applicableEntityDiscounts = GetApplicableEntityAPDiscounts(entity, vendorID);
				}

				if (applicableEntityDiscounts.Count != 0)
				{
					ApplicableToCombination applicableCombination = SetApplicableToCombination(entity.Value);
					if (applicableDiscountsByEntity.ContainsKey(applicableCombination))
					{
						applicableDiscountsByEntity[applicableCombination] = applicableDiscountsByEntity[applicableCombination].Concat(applicableEntityDiscounts).ToImmutableHashSet();
					}
					else
					{
						applicableDiscountsByEntity.Add(applicableCombination, applicableEntityDiscounts);
					}
					allFoundDiscounts.AddRange(applicableEntityDiscounts);
				}
			}

			//add all applicable unconditional discounts
			if (isARDiscount)
				applicableDiscounts.AddRange(
					GetUnconditionalDiscountsByType(discountType).Where(ud => !skipManual || cachedDiscountTypes[ud.DiscountID].IsManual != true));

			//searching for correct entity discounts
			foreach (DiscountSequenceKey discountSequence in allFoundDiscounts)
			{
				if (cachedDiscountTypes.ContainsKey(discountSequence.DiscountID)
					&& cachedDiscountTypes[discountSequence.DiscountID].Type == discountType
					&& (!skipManual || !cachedDiscountTypes[discountSequence.DiscountID].IsManual)
					&& (isARDiscount && !cachedDiscountTypes[discountSequence.DiscountID].IsVendorDiscount
						|| !isARDiscount && cachedDiscountTypes[discountSequence.DiscountID].IsVendorDiscount))
				{
					ApplicableToCombination combinedApplicableTo = ApplicableToCombination.None;
					bool correctDiscount = true;

					foreach (KeyValuePair<ApplicableToCombination, ImmutableHashSet<DiscountSequenceKey>> singleEntity in applicableDiscountsByEntity)
					{
						if ((singleEntity.Key & cachedDiscountTypes[discountSequence.DiscountID].ApplicableToEnum) != ApplicableToCombination.None)
						{
							if (!singleEntity.Value.Contains(discountSequence))
							{
								correctDiscount = false;
								break;
							}
							combinedApplicableTo = combinedApplicableTo | singleEntity.Key;
						}
					}

					if (combinedApplicableTo == cachedDiscountTypes[discountSequence.DiscountID].ApplicableToEnum && correctDiscount)
						applicableDiscounts.Add(discountSequence);
				}
			}
			return applicableDiscounts;
		}

		/// <summary>
		/// Returns best available discount for Line and Document discount types. Returns list of all applicable discounts for Group discount type. 
		/// </summary>
		/// <param name="discountSequences">Applicable Discount Sequences</param>
		/// <param name="curyDiscountableAmount">Discountable amount</param>
		/// <param name="discountableQuantity">Discountable quantity</param>
		/// <param name="discountType">Discount type: line, group or document</param>
		protected virtual List<DiscountDetailLine> SelectApplicableDiscounts(
			PXCache cache,
			DiscountLineFields dLine,
			HashSet<DiscountSequenceKey> discountSequences,
			decimal curyDiscountableAmount,
			decimal discountableQuantity,
			string discountType,
			DateTime date,
			bool ignoreCurrency = false)
		{
			if (cache == null)
				throw new ArgumentNullException(nameof(cache));

			if (!ignoreCurrency && dLine?.MappedLine != null
				&& dLine.Cache.GetValue(dLine.MappedLine, nameof(CurrencyInfo.curyInfoID)) != null)
				PXCurrencyAttribute.CuryConvBase(cache, dLine.MappedLine, curyDiscountableAmount, out curyDiscountableAmount);

			var discountsToReturn = new List<DiscountDetailLine>();

			if (discountType != DiscountType.Group)
			{
				Func<string, DiscountDetailLine> selectSingleBestDiscountBy =
					option => SelectSingleBestDiscount(cache, dLine, discountSequences.ToList(), curyDiscountableAmount, discountableQuantity, option, date);

				DiscountDetailLine bestAmountDiscount = selectSingleBestDiscountBy(DiscountOption.Amount);
				DiscountDetailLine bestPercentDiscount = selectSingleBestDiscountBy(DiscountOption.Percent);

				if (bestAmountDiscount.DiscountID != null && bestPercentDiscount.DiscountID != null)
				{
					discountsToReturn.Add(
						bestAmountDiscount.Discount < curyDiscountableAmount / 100 * bestPercentDiscount.Discount
							? bestPercentDiscount
							: bestAmountDiscount);
				}
				else
				{
					if (bestAmountDiscount.DiscountID != null && bestPercentDiscount.DiscountID == null)
						discountsToReturn.Add(bestAmountDiscount);
					if (bestAmountDiscount.DiscountID == null && bestPercentDiscount.DiscountID != null)
						discountsToReturn.Add(bestPercentDiscount);
				}
			}
			else
			{
				discountsToReturn.Add(
					SelectAllApplicableDiscounts(
						cache, dLine, discountSequences.ToList(), curyDiscountableAmount, discountableQuantity, date));
			}
			return discountsToReturn;
		}

		protected virtual DiscountDetailLine CreateDiscountDetails(PXCache cache, DiscountLineFields dLine, PXResult<DiscountSequence, DiscountSequenceDetail> discountResult, DateTime date)
		{
			DiscountSequenceDetail bestDiscountDetail = discountResult;
			DiscountSequence bestDiscountSequence = discountResult;
			if (bestDiscountDetail == null || bestDiscountSequence == null)
				return new DiscountDetailLine();

			int precision = CommonSetupDecPl.PrcCst;

			ConcurrentDictionary<string, DiscountCode> cachedDiscountTypes = GetCachedDiscountCodes();

			var newDiscountDetail =
				new DiscountDetailLine
				{
					DiscountID = bestDiscountDetail.DiscountID,
					DiscountSequenceID = bestDiscountDetail.DiscountSequenceID,
					Type = cachedDiscountTypes[bestDiscountDetail.DiscountID].Type,
					DiscountedFor = bestDiscountSequence.DiscountedFor,
					BreakBy = bestDiscountSequence.BreakBy
				};

			if (newDiscountDetail.DiscountedFor == DiscountOption.Amount && newDiscountDetail.Type != DiscountType.Line)
			{
				decimal discountAmount;
				PXCurrencyAttribute.CuryConvCury(cache, dLine.MappedLine, (decimal)bestDiscountDetail.Discount, out discountAmount, precision);
				newDiscountDetail.Discount = discountAmount;
			}
			else
			{
				newDiscountDetail.Discount = bestDiscountDetail.Discount;
			}

			if (bestDiscountSequence.BreakBy == BreakdownType.Quantity)
			{
				newDiscountDetail.AmountFrom = bestDiscountDetail.Quantity;
				newDiscountDetail.AmountTo = bestDiscountDetail.QuantityTo;
			}
			else
			{
				if (dLine != null)
				{
					decimal curyAmount;
					PXCurrencyAttribute.CuryConvCury(cache, dLine.MappedLine, bestDiscountDetail.Amount ?? 0m, out curyAmount, precision);
					newDiscountDetail.AmountFrom = curyAmount;
					if (bestDiscountDetail.AmountTo != null)
					{
						decimal curyAmountTo;
						PXCurrencyAttribute.CuryConvCury(cache, dLine.MappedLine, bestDiscountDetail.AmountTo ?? 0m, out curyAmountTo, precision);
						newDiscountDetail.AmountTo = curyAmountTo;
					}
					else
					{
						newDiscountDetail.AmountTo = null;
					}
				}
				else
				{
					newDiscountDetail.AmountFrom = bestDiscountDetail.Amount;
					newDiscountDetail.AmountTo = bestDiscountDetail.AmountTo;
				}
			}

			if (bestDiscountSequence.DiscountedFor == DiscountOption.FreeItem)
			{
				newDiscountDetail.freeItemQty = bestDiscountDetail.FreeItemQty;
				newDiscountDetail.freeItemID = bestDiscountSequence.IsPromotion == true || bestDiscountDetail.LastDate <= date
					? bestDiscountSequence.FreeItemID
					: bestDiscountSequence.LastFreeItemID;
			}

			newDiscountDetail.Prorate = bestDiscountSequence.Prorate;
			return newDiscountDetail;
		}

		#endregion

		#region Discount codes and Entity discount caches

		#region Discount Codes Cache
		protected static DCCache DiscountCodesCache
		{
			get
			{
				DCCache codes = 
					PXContext.GetSlot<DCCache>() ?? 
					PXContext.SetSlot(
						PXDatabase.GetSlot<DCCache>(
							typeof(DiscountCode).Name, 
							typeof(ConcurrentDictionary<string, DiscountCode>), typeof(ARDiscount), typeof(APDiscount)));
				return codes;
			}
		}

		protected class DCCache : IPrefetchable
		{
			public ConcurrentDictionary<string, DiscountCode> cachedDiscountCodes { get; private set; }

			public void Prefetch()
			{
				cachedDiscountCodes = new ConcurrentDictionary<string, DiscountCode>();
				StoreARDiscounts();
				StoreAPDiscounts();
			}

			private void StoreARDiscounts()
			{
				foreach (PXDataRecord discountType in PXDatabase.SelectMulti<ARDiscount>(
					new PXDataField<ARDiscount.discountID>(),
					new PXDataField<ARDiscount.type>(),
					new PXDataField<ARDiscount.isManual>(),
					new PXDataField<ARDiscount.excludeFromDiscountableAmt>(),
					new PXDataField<ARDiscount.skipDocumentDiscounts>(),
					new PXDataField<ARDiscount.applicableTo>()))
				{
					var type = new DiscountCode
								{
									IsVendorDiscount = false,
									Type = discountType.GetString(1),
									IsManual = (bool)discountType.GetBoolean(2),
									ExcludeFromDiscountableAmt = (bool)discountType.GetBoolean(3),
									SkipDocumentDiscounts = (bool)discountType.GetBoolean(4),
									ApplicableToEnum = SetApplicableToCombination(discountType.GetString(5))
								};
					cachedDiscountCodes.GetOrAdd(discountType.GetString(0), type);
				}
			}

			private void StoreAPDiscounts()
			{
				foreach (PXDataRecord discountType in PXDatabase.SelectMulti<APDiscount>(
					new PXDataField<APDiscount.bAccountID>(),
					new PXDataField<APDiscount.discountID>(),
					new PXDataField<APDiscount.type>(),
					new PXDataField<APDiscount.isManual>(),
					new PXDataField<APDiscount.excludeFromDiscountableAmt>(),
					new PXDataField<APDiscount.skipDocumentDiscounts>(),
					new PXDataField<APDiscount.applicableTo>()))
				{
					var type = new DiscountCode
								{
									IsVendorDiscount = true,
									VendorID = discountType.GetInt32(0),
									Type = discountType.GetString(2),
									IsManual = (bool) discountType.GetBoolean(3),
									ExcludeFromDiscountableAmt = (bool) discountType.GetBoolean(4),
									SkipDocumentDiscounts = (bool) discountType.GetBoolean(5),
									ApplicableToEnum = SetApplicableToCombination(discountType.GetString(6))
								};
					cachedDiscountCodes.GetOrAdd(discountType.GetString(1), type);
				}
			}
		}
		#endregion

		#region Discount Entities Cache

		protected static void UpdateEntityCache() => PXContext.SetSlot(PXDatabase.GetSlot<DECache>(typeof(DiscountSequence).Name, typeof(DiscountSequence)));

		protected class DECache : IPrefetchable
		{
			public void Prefetch() => ClearAllEntityCaches();

			private static void ClearAllEntityCaches()
			{
				SelectUnconditionalDiscounts(true);
				ClearAllEntityDiscounts<DiscountCustomer, int>();
				ClearAllEntityDiscounts<DiscountItem, int>();
				ClearAllEntityDiscounts<DiscountCustomerPriceClass, string>();
				ClearAllEntityDiscounts<DiscountInventoryPriceClass, string>();
				ClearAllEntityDiscounts<DiscountBranch, int>();
				ClearAllEntityDiscounts<DiscountSite, int>();
				ClearAllEntityDiscounts<APDiscountVendor, int>();
				ClearAllEntityDiscountsTwoKeys<APDiscountLocation, int, int>();
			}

			private static void ClearEntityCaches(PXCache cache, string discountID, string discountSequenceID, string applicableTo, int? vendorID = null)
			{
				SelectUnconditionalDiscounts(true);

				ApplicableToCombination applicable = SetApplicableToCombination(applicableTo);

				if (applicable.HasFlag(ApplicableToCombination.Customer))
					ClearDiscountCustomers(cache.Graph, discountID, discountSequenceID);

				if (applicable.HasFlag(ApplicableToCombination.InventoryItem))
					ClearDiscountItems(cache.Graph, discountID, discountSequenceID);

				if (applicable.HasFlag(ApplicableToCombination.CustomerPriceClass))
					ClearDiscountCustomerPriceClasses(cache.Graph, discountID, discountSequenceID);

				if (applicable.HasFlag(ApplicableToCombination.InventoryPriceClass))
					ClearDiscountInventoryPriceClasses(cache.Graph, discountID, discountSequenceID);

				if (applicable.HasFlag(ApplicableToCombination.Branch))
					ClearDiscountBranches(cache.Graph, discountID, discountSequenceID);

				if (applicable.HasFlag(ApplicableToCombination.Warehouse))
					ClearDiscountSites(cache.Graph, discountID, discountSequenceID);

				if (vendorID != null && applicable.HasFlag(ApplicableToCombination.Vendor))
					ClearDiscountVendors(cache.Graph, discountID, discountSequenceID);

				if (vendorID != null && applicable.HasFlag(ApplicableToCombination.Location))
					ClearDiscountLocations(cache.Graph, discountID, discountSequenceID);
			}

			private static void ClearDiscountLocations(PXGraph graph, String discountID, String discountSequenceID)
			{
				var discountLocations =
					PXSelect<APDiscountLocation,
					Where<APDiscountLocation.discountID, Equal<Required<APDiscountLocation.discountID>>,
						And<APDiscountLocation.discountSequenceID, Equal<Required<APDiscountLocation.discountSequenceID>>>>>
					.Select(graph, discountID, discountSequenceID);

				foreach (APDiscountLocation entity in discountLocations)
					ClearEntityDiscountsTwoKeys<APDiscountLocation, int, int>((int)entity.VendorID, (int)entity.LocationID);
			}

			private static void ClearDiscountVendors(PXGraph graph, String discountID, String discountSequenceID)
			{
				var discountVendors =
					PXSelect<APDiscountVendor,
					Where<APDiscountVendor.discountID, Equal<Required<APDiscountVendor.discountID>>,
						And<APDiscountVendor.discountSequenceID, Equal<Required<APDiscountVendor.discountSequenceID>>>>>
					.Select(graph, discountID, discountSequenceID);

				foreach (APDiscountVendor entity in discountVendors)
					ClearEntityDiscounts<APDiscountVendor, int>((int)entity.VendorID);
			}

			private static void ClearDiscountSites(PXGraph graph, String discountID, String discountSequenceID)
			{
				var discountSites =
					PXSelect<DiscountSite,
					Where<DiscountSite.discountID, Equal<Required<DiscountSite.discountID>>,
						And<DiscountSite.discountSequenceID, Equal<Required<DiscountSite.discountSequenceID>>>>>
					.Select(graph, discountID, discountSequenceID);

				foreach (DiscountSite entity in discountSites)
					ClearEntityDiscounts<DiscountSite, int>((int)entity.SiteID);
			}

			private static void ClearDiscountBranches(PXGraph graph, String discountID, String discountSequenceID)
			{
				var discountBranches =
					PXSelect<DiscountBranch,
					Where<DiscountBranch.discountID, Equal<Required<DiscountBranch.discountID>>,
						And<DiscountBranch.discountSequenceID, Equal<Required<DiscountBranch.discountSequenceID>>>>>
					.Select(graph, discountID, discountSequenceID);

				foreach (DiscountBranch entity in discountBranches)
					ClearEntityDiscounts<DiscountBranch, int>((int)entity.BranchID);
			}

			private static void ClearDiscountInventoryPriceClasses(PXGraph graph, String discountID, String discountSequenceID)
			{
				var discountInventoryPriceClasses =
					PXSelect<DiscountInventoryPriceClass,
					Where<DiscountInventoryPriceClass.discountID, Equal<Required<DiscountInventoryPriceClass.discountID>>,
						And<DiscountInventoryPriceClass.discountSequenceID, Equal<Required<DiscountInventoryPriceClass.discountSequenceID>>>>>
					.Select(graph, discountID, discountSequenceID);

				foreach (DiscountInventoryPriceClass entity in discountInventoryPriceClasses)
					ClearEntityDiscounts<DiscountInventoryPriceClass, string>(entity.InventoryPriceClassID);
			}

			private static void ClearDiscountCustomerPriceClasses(PXGraph graph, String discountID, String discountSequenceID)
			{
				var discountCustomerPriceClasses =
					PXSelect<DiscountCustomerPriceClass,
					Where<DiscountCustomerPriceClass.discountID, Equal<Required<DiscountCustomerPriceClass.discountID>>,
						And<DiscountCustomerPriceClass.discountSequenceID, Equal<Required<DiscountCustomerPriceClass.discountSequenceID>>>>>
					.Select(graph, discountID, discountSequenceID);

				foreach (DiscountCustomerPriceClass entity in discountCustomerPriceClasses)
					ClearEntityDiscounts<DiscountCustomerPriceClass, string>(entity.CustomerPriceClassID);
			}

			private static void ClearDiscountItems(PXGraph graph, String discountID, String discountSequenceID)
			{
				var discountItems =
					PXSelect<DiscountItem,
					Where<DiscountItem.discountID, Equal<Required<DiscountItem.discountID>>,
						And<DiscountItem.discountSequenceID, Equal<Required<DiscountItem.discountSequenceID>>>>>
					.Select(graph, discountID, discountSequenceID);

				foreach (DiscountItem entity in discountItems)
					ClearEntityDiscounts<DiscountItem, int>((int)entity.InventoryID);
			}

			private static void ClearDiscountCustomers(PXGraph graph, String discountID, String discountSequenceID)
			{
				var discountCustomers =
					PXSelect<DiscountCustomer,
					Where<DiscountCustomer.discountID, Equal<Required<DiscountCustomer.discountID>>,
						And<DiscountCustomer.discountSequenceID, Equal<Required<DiscountCustomer.discountSequenceID>>>>>
					.Select(graph, discountID, discountSequenceID);

				foreach (DiscountCustomer entity in discountCustomers)
					ClearEntityDiscounts<DiscountCustomer, int>((int)entity.CustomerID);
			}

			private static void ClearAllEntityDiscounts<TTable, TEntityType>()
				where TTable : IBqlTable
			{
				var cachedDiscountEntities =
					PXDatabase.GetSlot<ConcurrentDictionary<TEntityType, ImmutableHashSet<DiscountSequenceKey>>>(
						typeof(TTable).Name,
						typeof(ConcurrentDictionary<TEntityType, ImmutableHashSet<DiscountSequenceKey>>));
				cachedDiscountEntities.Clear();
			}

			private static void ClearAllEntityDiscountsTwoKeys<TTable, TEntityType1, TEntityType2>()
				where TTable : IBqlTable
			{
				var cachedDiscountEntities =
					PXDatabase.GetSlot<ConcurrentDictionary<Tuple<TEntityType1, TEntityType2>, ImmutableHashSet<DiscountSequenceKey>>>(
						typeof(TTable).Name,
						typeof(ConcurrentDictionary<Tuple<TEntityType1, TEntityType2>, ImmutableHashSet<DiscountSequenceKey>>));
				cachedDiscountEntities.Clear();
			}

			private static void ClearEntityDiscounts<TTable, TEntityType>(TEntityType entityID)
				where TTable : IBqlTable
			{
				var cachedDiscountEntities =
					PXDatabase.GetSlot<ConcurrentDictionary<TEntityType, ImmutableHashSet<DiscountSequenceKey>>>(
						typeof(TTable).Name,
						typeof(ConcurrentDictionary<TEntityType, ImmutableHashSet<DiscountSequenceKey>>));
				if (cachedDiscountEntities.ContainsKey(entityID))
					cachedDiscountEntities.Remove(entityID);
			}

			private static void ClearEntityDiscountsTwoKeys<TTable, TEntityType1, TEntityType2>(TEntityType1 entityID1, TEntityType2 entityID2)
				where TTable : IBqlTable
			{
				var cachedDiscountEntities =
					PXDatabase.GetSlot<ConcurrentDictionary<Tuple<TEntityType1, TEntityType2>, ImmutableHashSet<DiscountSequenceKey>>>(
						typeof(TTable).Name,
						typeof(ConcurrentDictionary<Tuple<TEntityType1, TEntityType2>, ImmutableHashSet<DiscountSequenceKey>>));
				var entityKey = Tuple.Create(entityID1, entityID2);
				if (cachedDiscountEntities.ContainsKey(entityKey))
					cachedDiscountEntities.Remove(entityKey);
			}
		}

		#endregion

		/// <summary>
		/// Returns dictionary of cached discount codes. Dictionary key is DiscountID
		/// </summary>
		protected static ConcurrentDictionary<string, DiscountCode> GetCachedDiscountCodes()
		{
			ConcurrentDictionary<string, DiscountCode> cachedDiscountCodes = DiscountCodesCache.cachedDiscountCodes;
			if (cachedDiscountCodes.Count == 0)
				GetDiscountTypes(false);
			return cachedDiscountCodes;
		}

		/// <summary>
		/// Collects all discount types and unconditional AR discounts
		/// </summary>
		/// <param name="clearCache">Set to true to clear discount types cache and recreate it.</param>
		public static void GetDiscountTypes(bool clearCache = false) => SelectUnconditionalDiscounts(clearCache);

		private HashSet<DiscountSequenceKey> GetUnconditionalDiscountsByType(string type)
		{
			var cachedDiscountEntities = CachedUnconditionalDiscounts;
			if (cachedDiscountEntities.ContainsKey(AR.Messages.Unconditional) == false)
				return new HashSet<DiscountSequenceKey>();

			var unconditionalDiscounts = new HashSet<DiscountSequenceKey>();
			foreach (DiscountSequenceKey discountSequence in cachedDiscountEntities[AR.Messages.Unconditional])
			{
				if (GetCachedDiscountCodes().ContainsKey(discountSequence.DiscountID))
				{
					if (GetCachedDiscountCodes()[discountSequence.DiscountID].Type == type)
						unconditionalDiscounts.Add(discountSequence);
				}
				else
				{
					SelectUnconditionalDiscounts(true);
					break;
				}
			}
			return unconditionalDiscounts;
		}

		private static HashSet<DiscountSequenceKey> SelectUnconditionalDiscounts(bool clearCache)
		{
			var cachedDiscountEntities = CachedUnconditionalDiscounts;
			if (clearCache)
				cachedDiscountEntities.Clear();

			if (!cachedDiscountEntities.ContainsKey(AR.Messages.Unconditional))
			{
				cachedDiscountEntities.GetOrAdd(AR.Messages.Unconditional, ImmutableHashSet.Create<DiscountSequenceKey>);
				IEnumerable<DiscountSequenceKey> unconditionalDiscounts =
					PXSelectJoin<ARDiscount,
					InnerJoin<DiscountSequence, On<DiscountSequence.discountID, Equal<ARDiscount.discountID>>>,
					Where<ARDiscount.applicableTo, Equal<DiscountTarget.unconditional>>>
					.Select(PXGraph.CreateInstance<PXGraph>())
					.RowCast<DiscountSequence>()
					.Select(d => new DiscountSequenceKey(d.DiscountID, d.DiscountSequenceID));
				cachedDiscountEntities[AR.Messages.Unconditional] = cachedDiscountEntities[AR.Messages.Unconditional].Concat(unconditionalDiscounts).ToImmutableHashSet();
			}
			return new HashSet<DiscountSequenceKey>(cachedDiscountEntities[AR.Messages.Unconditional]); // Uses ordinal hash-set for backward compatibility
		}

		private static ConcurrentDictionary<Object, ImmutableHashSet<DiscountSequenceKey>> CachedUnconditionalDiscounts
		{
			get
			{
				return PXDatabase.GetSlot<ConcurrentDictionary<object, ImmutableHashSet<DiscountSequenceKey>>>(
					AR.Messages.Unconditional,
					typeof(ConcurrentDictionary<object, ImmutableHashSet<DiscountSequenceKey>>));
			}
		}

		#endregion

		#region Entity-specific functions
		private static readonly IReadOnlyDictionary<string, ApplicableToCombination> DiscountTargetToApplicableMap =
			new Dictionary<string, ApplicableToCombination>
			{
				[DiscountTarget.Customer] = ApplicableToCombination.Customer,
				[DiscountTarget.Inventory] = ApplicableToCombination.InventoryItem,
				[DiscountTarget.CustomerPrice] = ApplicableToCombination.CustomerPriceClass,
				[DiscountTarget.InventoryPrice] = ApplicableToCombination.InventoryPriceClass,
				[DiscountTarget.CustomerAndInventory] = ApplicableToCombination.Customer | ApplicableToCombination.InventoryItem,
				[DiscountTarget.CustomerAndInventoryPrice] = ApplicableToCombination.Customer | ApplicableToCombination.InventoryPriceClass,
				[DiscountTarget.CustomerPriceAndInventory] = ApplicableToCombination.CustomerPriceClass | ApplicableToCombination.InventoryItem,
				[DiscountTarget.CustomerPriceAndBranch] = ApplicableToCombination.CustomerPriceClass | ApplicableToCombination.Branch,
				[DiscountTarget.CustomerPriceAndInventoryPrice] = ApplicableToCombination.CustomerPriceClass | ApplicableToCombination.InventoryPriceClass,
				[DiscountTarget.CustomerAndBranch] = ApplicableToCombination.Customer | ApplicableToCombination.Branch,
				[DiscountTarget.Warehouse] = ApplicableToCombination.Warehouse,
				[DiscountTarget.WarehouseAndCustomer] = ApplicableToCombination.Warehouse | ApplicableToCombination.Customer,
				[DiscountTarget.WarehouseAndCustomerPrice] = ApplicableToCombination.Warehouse | ApplicableToCombination.CustomerPriceClass,
				[DiscountTarget.WarehouseAndInventory] = ApplicableToCombination.Warehouse | ApplicableToCombination.InventoryItem,
				[DiscountTarget.WarehouseAndInventoryPrice] = ApplicableToCombination.Warehouse | ApplicableToCombination.InventoryPriceClass,
				[DiscountTarget.Branch] = ApplicableToCombination.Branch,
				[DiscountTarget.Vendor] = ApplicableToCombination.Vendor, //Unconditional for Vendor
				[DiscountTarget.VendorAndInventory] = ApplicableToCombination.Vendor | ApplicableToCombination.InventoryItem,
				[DiscountTarget.VendorAndInventoryPrice] = ApplicableToCombination.Vendor | ApplicableToCombination.InventoryPriceClass,
				[DiscountTarget.VendorLocation] = ApplicableToCombination.Location,
				[DiscountTarget.VendorLocationAndInventory] = ApplicableToCombination.Location | ApplicableToCombination.InventoryItem,
				[DiscountTarget.Unconditional] = ApplicableToCombination.Unconditional,
			};

		private static ApplicableToCombination SetApplicableToCombination(string applicableTo)
		{
			ApplicableToCombination applicableToResult;
			DiscountTargetToApplicableMap.TryGetValue(applicableTo, out applicableToResult);
			return applicableToResult;
		}

		private ImmutableHashSet<DiscountSequenceKey> GetApplicableEntityARDiscounts(KeyValuePair<object, string> entity)
		{
			switch (entity.Value)
			{
				case DiscountTarget.Customer:
					return SelectEntityDiscounts<
							DiscountCustomer,
							DiscountCustomer.discountID,
							DiscountCustomer.discountSequenceID,
							DiscountCustomer.customerID, int>(
							(int) entity.Key);
				case DiscountTarget.Inventory:
					return SelectEntityDiscounts<
							DiscountItem,
							DiscountItem.discountID,
							DiscountItem.discountSequenceID,
							DiscountItem.inventoryID, int>(
							(int) entity.Key);
				case DiscountTarget.CustomerPrice:
					return SelectEntityDiscounts<
							DiscountCustomerPriceClass,
							DiscountCustomerPriceClass.discountID,
							DiscountCustomerPriceClass.discountSequenceID,
							DiscountCustomerPriceClass.customerPriceClassID, string>(
							(string) entity.Key);
				case DiscountTarget.InventoryPrice:
					return SelectEntityDiscounts<
							DiscountInventoryPriceClass,
							DiscountInventoryPriceClass.discountID,
							DiscountInventoryPriceClass.discountSequenceID,
							DiscountInventoryPriceClass.inventoryPriceClassID, string>(
							(string) entity.Key);
				case DiscountTarget.Branch:
					return SelectEntityDiscounts<
							DiscountBranch,
							DiscountBranch.discountID,
							DiscountBranch.discountSequenceID,
							DiscountBranch.branchID, int>(
							(int) entity.Key);
				case DiscountTarget.Warehouse:
					return SelectEntityDiscounts<
							DiscountSite,
							DiscountSite.discountID,
							DiscountSite.discountSequenceID,
							DiscountSite.siteID, int>(
							(int) entity.Key);
				default:
					return ImmutableHashSet.Create<DiscountSequenceKey>();
			}
		}

		private ImmutableHashSet<DiscountSequenceKey> GetApplicableEntityAPDiscounts(KeyValuePair<object, string> entity, int? vendorID)
		{
			switch (entity.Value)
			{
				case DiscountTarget.Inventory:
					return SelectEntityDiscounts<
						DiscountItem,
						DiscountItem.discountID,
						DiscountItem.discountSequenceID,
						DiscountItem.inventoryID, int>(
						(int) entity.Key);
				case DiscountTarget.Vendor:
					return SelectEntityDiscounts<
						APDiscountVendor,
						APDiscountVendor.discountID,
						APDiscountVendor.discountSequenceID,
						APDiscountVendor.vendorID, int>(
						(int) entity.Key);
				case DiscountTarget.InventoryPrice:
					return SelectEntityDiscounts<
						DiscountInventoryPriceClass,
						DiscountInventoryPriceClass.discountID,
						DiscountInventoryPriceClass.discountSequenceID,
						DiscountInventoryPriceClass.inventoryPriceClassID, string>(
						(string) entity.Key);
				case DiscountTarget.VendorLocation:
					return vendorID == null
						? ImmutableHashSet.Create<DiscountSequenceKey>()
						: SelectEntityDiscounts<
							APDiscountLocation,
							APDiscountLocation.discountID,
							APDiscountLocation.discountSequenceID,
							APDiscountLocation.vendorID, int,
							APDiscountLocation.locationID, int>(
							(int) vendorID, (int) entity.Key);
				default:
					return ImmutableHashSet.Create<DiscountSequenceKey>();
			}
		}



		/// <summary>
		/// Removes entity from the list of cached Inventory ID to Inventory Price Class correlations 
		/// </summary>
		public static void RemoveFromCachedInventoryPriceClasses(int? inventoryID)
		{
			int invID = inventoryID ?? 0;
			var cachedInventoryPriceClasses = PXDatabase.GetSlot<ConcurrentDictionary<int, string>>(InventoryPriceClassesSlotName);
			if (cachedInventoryPriceClasses.ContainsKey(invID))
				cachedInventoryPriceClasses.Remove(invID);
		}

		protected static string GetInventoryPriceClassID<TLine>(PXCache cache, TLine line, int? inventoryID)
			where TLine : class, IBqlTable, new()
		{
			if (inventoryID == null)
				return null;

			int invID = inventoryID.Value;

			var cachedInventoryPriceClasses = PXDatabase.GetSlot<ConcurrentDictionary<int, string>>(InventoryPriceClassesSlotName);
			if (cachedInventoryPriceClasses.ContainsKey(invID))
				return cachedInventoryPriceClasses[invID];

			if (cachedInventoryPriceClasses.Count >= 5000)
				cachedInventoryPriceClasses.Clear();

			InventoryItem item =
				(InventoryItem)PXSelectorAttribute.Select<InventoryItem.inventoryID>(cache, line)
				?? PXSelectReadonly<InventoryItem, Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>
					.Select(cache.Graph, inventoryID);

			if (item != null)
				return cachedInventoryPriceClasses.GetOrAdd(invID, item.PriceClassID);

			return null;
		}


		/// <summary>
		/// Removes entity from the list of cached Customer ID to Customer Price Class correlations 
		/// </summary>
		public static void RemoveFromCachedCustomerPriceClasses(int? bAccountID)
		{
			int bAcctID = bAccountID ?? 0;
			var cachedCustomerPriceClasses = PXDatabase.GetSlot<ConcurrentDictionary<Tuple<int, int>, string>>(CustomerPriceClassIDSlotName);
			foreach (var key in cachedCustomerPriceClasses.Keys.Where(k => k.Item1 == bAcctID))
				cachedCustomerPriceClasses.Remove(key);
		}

		protected static string GetCustomerPriceClassID(PXCache cache, int? bAccountID, int? locationID)
		{
			if (bAccountID == null || locationID == null)
				return null;

			int bAcctID = bAccountID.Value;
			int locID = locationID.Value;
			var key = Tuple.Create(bAcctID, locID);

			var cachedCustomerPriceClasses = PXDatabase.GetSlot<ConcurrentDictionary<Tuple<int, int>, string>>(CustomerPriceClassIDSlotName);
			if (cachedCustomerPriceClasses.ContainsKey(key))
				return cachedCustomerPriceClasses[key];

			if (cachedCustomerPriceClasses.Count >= 5000)
				cachedCustomerPriceClasses.Clear();

			Location location =
				PXSelectReadonly<Location,
						Where<Location.bAccountID, Equal<Required<Location.bAccountID>>,
							And<Location.locationID, Equal<Required<Location.locationID>>>>>
					.Select(cache.Graph, bAccountID, locationID);

			if (location != null)
				return cachedCustomerPriceClasses.GetOrAdd(key, location.CPriceClassID);

			return null;
		}

		#endregion

		#region Selects

		/// <summary>
		/// Returns single best available discount
		/// </summary>
		/// <param name="discountSequences">Applicable Discount Sequences</param>
		/// <param name="amount">Discountable amount</param>
		/// <param name="quantity">Discountable quantity</param>
		/// <param name="discountFor">Discounted for: amount, percent or free item</param>
		protected virtual DiscountDetailLine SelectSingleBestDiscount(PXCache cache, DiscountLineFields dLine, List<DiscountSequenceKey> discountSequences, decimal amount, decimal quantity, string discountFor, DateTime date)
		{
			if (discountSequences?.Any() != true)
				return new DiscountDetailLine();

			var bestDiscount = GetDiscounts(discountSequences, amount, quantity, discountFor, date, true).SingleOrDefault();
			DiscountSequence sequence = bestDiscount;
			return sequence == null || (sequence.BreakBy != BreakdownType.Amount || amount != 0m) && (sequence.BreakBy != BreakdownType.Quantity || quantity != 0m)
				? CreateDiscountDetails(cache, dLine, bestDiscount, date)
				: new DiscountDetailLine();
		}

		/// <summary>
		/// Returns list of all applicable discounts
		/// </summary>
		/// <param name="discountSequences">Applicable Discount Sequences</param>
		/// <param name="amount">Discountable amount</param>
		/// <param name="quantity">Discountable quantity</param>
		protected virtual List<DiscountDetailLine> SelectAllApplicableDiscounts(PXCache cache, DiscountLineFields dLine, List<DiscountSequenceKey> discountSequences, decimal amount, decimal quantity, DateTime date)
		{
			var applicableDiscountDetails = new List<DiscountDetailLine>();
			if (discountSequences?.Any() != true)
				return applicableDiscountDetails; // return null?

			var allApplicableDiscounts = GetDiscounts(discountSequences, amount, quantity, string.Empty, date, false);

			foreach (PXResult<DiscountSequence, DiscountSequenceDetail> bestDiscount in allApplicableDiscounts)
			{
				DiscountSequence sequence = bestDiscount;
				if (sequence == null || sequence.BreakBy == BreakdownType.Amount && amount == 0m || sequence.BreakBy == BreakdownType.Quantity && quantity == 0m)
					continue;

				int existingLineIndex = applicableDiscountDetails.FindIndex(
					x => x.DiscountID == sequence.DiscountID
						&& x.DiscountSequenceID == sequence.DiscountSequenceID
						&& x.DiscountedFor == sequence.DiscountedFor);

				DiscountDetailLine newLine = CreateDiscountDetails(cache, dLine, bestDiscount, date);
				if (existingLineIndex < 0)
					applicableDiscountDetails.Add(newLine);
				else if (applicableDiscountDetails[existingLineIndex].Discount < newLine.Discount)
					applicableDiscountDetails[existingLineIndex] = newLine;
			}
			return applicableDiscountDetails;
		}

		protected virtual List<PXResult<DiscountSequence, DiscountSequenceDetail>> GetDiscounts(
			List<DiscountSequenceKey> discountSequences,
			decimal amount,
			decimal quantity,
			string discountedFor,
			DateTime date,
			bool single)
		{
			if (discountSequences.Count == 0)
				return null;

			// Current discount - 14 of Required
			var requiredParams =
				new List<object>
				{
					BreakdownType.Amount,   // Required<DiscountSequence.breakBy>
					Math.Abs(amount),       // Required<DiscountSequenceDetail.amount>
					Math.Abs(amount),       // Required<DiscountSequenceDetail.amountTo>

					BreakdownType.Quantity, // Required<DiscountSequence.breakBy>
					Math.Abs(quantity),     // Required<DiscountSequenceDetail.quantity>
					Math.Abs(quantity),     // Required<DiscountSequenceDetail.quantityTo>
					
					BreakdownType.Amount,   // Required<DiscountSequence.breakBy>
					Math.Abs(amount),       // Required<DiscountSequenceDetail.amount>
					
					BreakdownType.Quantity, // Required<DiscountSequence.breakBy>
					Math.Abs(quantity),     // Required<DiscountSequenceDetail.quantity>

					date,                   // Required<DiscountSequenceDetail.lastDate>
					date,                   // Required<DiscountSequenceDetail.lastDate>
					date,                   // Required<DiscountSequence.startDate>
					date                    // Required<DiscountSequence.endDate>
				};

			var discountTypes = new List<Type>();
			if (discountedFor != string.Empty)
			{
				discountTypes.Add(typeof(And<,,>));
				discountTypes.Add(typeof(DiscountSequence.discountedFor));
				discountTypes.Add(typeof(Equal<Required<DiscountSequence.discountedFor>>));
				requiredParams.Add(discountedFor);
			}

			discountTypes.Add(typeof(And<>));
			discountTypes.Add(typeof(Where<,,>));

			for (int i = 0; i < discountSequences.Count; i++)
			{
				bool isLast = i == discountSequences.Count - 1;

				discountTypes.Add(typeof(DiscountSequenceDetail.discountID));
				discountTypes.Add(typeof(Equal<Required<DiscountSequenceDetail.discountID>>));
				discountTypes.Add(!isLast ? typeof(And<,,>) : typeof(And<,>));
				discountTypes.Add(typeof(DiscountSequenceDetail.discountSequenceID));
				discountTypes.Add(typeof(Equal<Required<DiscountSequenceDetail.discountSequenceID>>));

				requiredParams.Add(discountSequences[i].DiscountID);
				requiredParams.Add(discountSequences[i].DiscountSequenceID);

				if (!isLast)
					discountTypes.Add(typeof(Or<,,>));
			}

			BqlCommand command = BqlTemplate.OfCommand<
				Select2<DiscountSequence,
				LeftJoin<DiscountSequenceDetail,
					On<DiscountSequenceDetail.discountID, Equal<DiscountSequence.discountID>,
					And<DiscountSequenceDetail.discountSequenceID, Equal<DiscountSequence.discountSequenceID>>>>,
				Where2<
					// Current discount - 14 of Required
					Where<Where2<Where<
						DiscountSequence.breakBy, Equal<Required<DiscountSequence.breakBy>>,
						And<DiscountSequenceDetail.amount, LessEqual<Required<DiscountSequenceDetail.amount>>,
						And<DiscountSequenceDetail.amountTo, Greater<Required<DiscountSequenceDetail.amountTo>>,

						Or<DiscountSequence.breakBy, Equal<Required<DiscountSequence.breakBy>>,
						And<DiscountSequenceDetail.quantity, LessEqual<Required<DiscountSequenceDetail.quantity>>,
						And<DiscountSequenceDetail.quantityTo, Greater<Required<DiscountSequenceDetail.quantityTo>>,

						Or<Where2<Where<DiscountSequence.breakBy, Equal<Required<DiscountSequence.breakBy>>,
						And<Where2<Where<DiscountSequenceDetail.amountTo, IsNull,
						Or<DiscountSequenceDetail.amountTo, Equal<decimal0>>>,
						And<DiscountSequenceDetail.amount, LessEqual<Required<DiscountSequenceDetail.amount>>>>>>,

						Or<Where<DiscountSequence.breakBy, Equal<Required<DiscountSequence.breakBy>>,
						And<Where2<Where<DiscountSequenceDetail.quantityTo, IsNull,
						Or<DiscountSequenceDetail.quantityTo, Equal<decimal0>>>,
						And<DiscountSequenceDetail.quantity, LessEqual<Required<DiscountSequenceDetail.quantity>>>>>>>>>>>>>>>,

						And<Where<DiscountSequence.isActive, Equal<True>, And<DiscountSequenceDetail.isActive, Equal<True>,
						And<Where2<Where<DiscountSequence.isPromotion, Equal<False>,
						And<Where2<Where<DiscountSequenceDetail.lastDate, LessEqual<Required<DiscountSequenceDetail.lastDate>>,
							And<DiscountSequenceDetail.isLast, Equal<False>>>,
							Or<Where<DiscountSequenceDetail.lastDate, Greater<Required<DiscountSequenceDetail.lastDate>>,
							And<DiscountSequenceDetail.isLast, Equal<True>>>>>>>,
						Or<Where<DiscountSequence.isPromotion, Equal<True>,
						And<DiscountSequenceDetail.isLast, Equal<False>,
						And<DiscountSequence.startDate, LessEqual<Required<DiscountSequence.startDate>>,
						And<DiscountSequence.endDate, GreaterEqual<Required<DiscountSequence.endDate>>>>>>>>>>>>>>,
					BqlPlaceholder.A>, // Additional conditions placeholder
				OrderBy<Asc<DiscountSequenceDetail.isLast,
					Desc<DiscountSequenceDetail.discount>>>>>.Replace<BqlPlaceholder.A>(BqlCommand.Compose(discountTypes.ToArray())).ToCommand();

			var bestDiscountView = new PXView(PXGraph.CreateInstance<PXGraph>(), false, command);

			var result = new List<PXResult<DiscountSequence, DiscountSequenceDetail>>();
			if (single)
				result.Add((PXResult<DiscountSequence, DiscountSequenceDetail>)bestDiscountView.SelectSingle(requiredParams.ToArray()));
			else
				result.AddRange(bestDiscountView.SelectMulti(requiredParams.ToArray()).Cast<PXResult<DiscountSequence, DiscountSequenceDetail>>());

			return result;
		}

		protected Lazy<ARDiscountSequenceMaint> ARDiscountSequence = Lazy.By(PXGraph.CreateInstance<ARDiscountSequenceMaint>);
		protected virtual ImmutableHashSet<DiscountSequenceKey> SelectEntityDiscounts<TTable, TDiscountID, TDiscountSequenceID, TKeyField, TKeyType>(TKeyType entityID)
			where TTable : IBqlTable
			where TKeyField : IBqlField
			where TDiscountID : IBqlField
			where TDiscountSequenceID : IBqlField
		{
			var cachedDiscountEntities = 
				PXDatabase.GetSlot<ConcurrentDictionary<TKeyType, ImmutableHashSet<DiscountSequenceKey>>>(
					typeof(TTable).Name,
					typeof(ConcurrentDictionary<TKeyType, ImmutableHashSet<DiscountSequenceKey>>));

			if (!cachedDiscountEntities.ContainsKey(entityID))
			{
				BqlCommand select = 
					new Select2<DiscountSequence,
					InnerJoin<TTable, On<TDiscountID, Equal<DiscountSequence.discountID>, 
						And<TDiscountSequenceID, Equal<DiscountSequence.discountSequenceID>>>>,
					Where<DiscountSequence.isActive, Equal<True>, 
						And<TKeyField, Equal<Required<TKeyField>>>>>();

				PXView discountSequenceView = new PXView(ARDiscountSequence.Value, true, select);
				discountSequenceView.Cache.ClearQueryCache();

				cachedDiscountEntities[entityID] = discountSequenceView
						.SelectMulti(entityID)
						.RowCast<DiscountSequence>()
						.Select(seq => new DiscountSequenceKey(seq.DiscountID, seq.DiscountSequenceID))
						.ToImmutableHashSet();
			}
			return cachedDiscountEntities[entityID];
		}

		protected Lazy<APDiscountSequenceMaint> APDiscountSequence = Lazy.By(PXGraph.CreateInstance<APDiscountSequenceMaint>);
		protected virtual ImmutableHashSet<DiscountSequenceKey> SelectEntityDiscounts<TTable, TDiscountID, TDiscountSequenceID, TKeyField1, TKeyType1, TKeyField2, TKeyType2>(TKeyType1 entityID1, TKeyType2 entityID2)
			where TTable : IBqlTable
			where TKeyField1 : IBqlField
			where TKeyField2 : IBqlField
			where TDiscountID : IBqlField
			where TDiscountSequenceID : IBqlField
		{
			var cachedDiscountEntities = 
				PXDatabase.GetSlot<ConcurrentDictionary<Tuple<TKeyType1, TKeyType2>, ImmutableHashSet<DiscountSequenceKey>>>(
					typeof(TTable).Name,
					typeof(ConcurrentDictionary<Tuple<TKeyType1, TKeyType2>, ImmutableHashSet<DiscountSequenceKey>>));

			var key = Tuple.Create(entityID1, entityID2);
			if (!cachedDiscountEntities.ContainsKey(key))
			{
				BqlCommand select = 
					new Select2<DiscountSequence,
					InnerJoin<TTable, On<TDiscountID, Equal<DiscountSequence.discountID>, 
						And<TDiscountSequenceID, Equal<DiscountSequence.discountSequenceID>>>>,
					Where<DiscountSequence.isActive, Equal<True>, 
						And<TKeyField1, Equal<Required<TKeyField1>>,
						And<TKeyField2, Equal<Required<TKeyField2>>>>>>();

				PXView discountSequenceView = new PXView(APDiscountSequence.Value, true, select);
				discountSequenceView.Cache.ClearQueryCache();

				cachedDiscountEntities[key] = discountSequenceView
						.SelectMulti(key.Item1, key.Item2)
						.RowCast<DiscountSequence>()
						.Select(seq => new DiscountSequenceKey(seq.DiscountID, seq.DiscountSequenceID))
						.ToImmutableHashSet();
			}
			return cachedDiscountEntities[key];
		}

		protected virtual HashSet<DiscountSequenceKey> SelectDiscountSequences(string discountID)
		{
			return PXDatabase
				.SelectMulti<DiscountSequence>(
					new PXDataField<DiscountSequence.discountSequenceID>(),
					new PXDataFieldValue("DiscountID", PXDbType.NVarChar, discountID))
				.Select(ds => new DiscountSequenceKey(discountID, ds.GetString(0)))
				.ToHashSet();
		}

		protected virtual DiscountSequence SelectDiscountSequence(PXCache cache, string discountID, string discountSequenceID)
		{
			return PXSelect<DiscountSequence,
				Where<DiscountSequence.discountID, Equal<Required<DiscountSequence.discountID>>,
					And<DiscountSequence.discountSequenceID, Equal<Required<DiscountSequence.discountSequenceID>>>>>
				.Select(cache.Graph, discountID, discountSequenceID)
				.FirstOrDefault() ?? new DiscountSequence();
		}


		public virtual decimal GetDiscountLimit(PXCache cache, int? customerID, int? vendorID = null)
		{
			decimal discountLimit = 100m;
			if (customerID != null)
			{
				CustomerClass customerClass =
					PXSelectJoin<CustomerClass,
					LeftJoinSingleTable<Customer, On<Customer.customerClassID, Equal<CustomerClass.customerClassID>>>,
					Where<Customer.bAccountID, Equal<Required<Customer.bAccountID>>>>
					.SelectWindowed(cache.Graph, 0, 1, customerID);
				if (customerClass != null)
					discountLimit = customerClass.DiscountLimit ?? 100m;
			}
			else if (vendorID != null)
			{
				discountLimit = (decimal)Math.Pow((double)discountLimit, 3);
			}

			return discountLimit;
		}

		public virtual decimal GetTotalGroupAndDocumentDiscount<TDiscountDetail>(PXSelectBase<TDiscountDetail> discountDetails, bool docOnly = false)
			where TDiscountDetail : class, IBqlTable, IDiscountDetail, new()
		{
			return discountDetails
				.Select().RowCast<TDiscountDetail>()
				.Where(r => r.SkipDiscount != true && (docOnly && r.Type == DiscountType.Document || !docOnly))
				.Select(r => r.CuryDiscountAmt ?? 0).Sum();
		}

		#region Pricing part

		//Returns best ARSalesPrice
		protected virtual ARSalesPriceMaint.SalesPriceItem GetSalesPrice(PXCache cache,
			int? inventoryID, int? siteID, int? customerID, string customerPriceClassID, string curyID, string UOM, decimal? quantity, DateTime date,
			bool isBaseQty)
		{
			if(isBaseQty)
				quantity = INUnitAttribute.ConvertFromBase(cache, inventoryID, UOM, (int)quantity, INPrecision.QUANTITY);

			return ARSalesPriceMaint.SingleARSalesPriceMaint.FindSalesPrice(cache, customerPriceClassID, customerID, inventoryID, siteID, curyID, curyID, quantity, UOM, date, false);
		}

		#endregion

		#endregion

		#region NoDiscountDetail methods
		public static void SetLineDiscountOnly<TLine>(
			PXCache cache,
			TLine line,
			DiscountLineFields dLine,
			string discountID,
			decimal? unitPrice,
			decimal? extPrice,
			decimal? qty,
			int? locationID,
			int? customerID,
			string curyID,
			DateTime? date,
			int? branchID = null,
			int? inventoryID = null,
			bool needDiscountID = true)
			where TLine : class, IBqlTable, new()
		{
			DiscountEngineProvider.GetEngineFor<TLine, NoDiscountDetail>().SetLineDiscountOnlyImpl(
				cache,
				line,
				dLine,
				discountID,
				unitPrice,
				extPrice,
				qty,
				locationID,
				customerID,
				curyID,
				date,
				branchID,
				inventoryID,
				needDiscountID
			);
		}

		protected abstract void SetLineDiscountOnlyImpl(
			PXCache cache,
			object line,
			DiscountLineFields dLine,
			string discountID,
			decimal? unitPrice,
			decimal? extPrice,
			decimal? qty,
			int? locationID,
			int? customerID,
			string curyID,
			DateTime? date,
			int? branchID,
			int? inventoryID,
			bool needDiscountID);

		/// <summary>
		/// Fake discount detail table, used for calling DiscountEngine methods that do not need DiscountDetail
		/// </summary>
		[PXHidden]
		private class NoDiscountDetail : IBqlTable, IDiscountDetail
		{
			public Int32? RecordID { get; set; }
			public ushort? LineNbr { get; set; }
			public Boolean? SkipDiscount { get; set; }
			public String DiscountID { get; set; }
			public String DiscountSequenceID { get; set; }
			public String Type { get; set; }
			public Decimal? CuryDiscountableAmt { get; set; }
			public Decimal? DiscountableQty { get; set; }
			public Decimal? CuryDiscountAmt { get; set; }
			public Decimal? DiscountPct { get; set; }
			public Int32? FreeItemID { get; set; }
			public Decimal? FreeItemQty { get; set; }
			public Boolean? IsManual { get; set; }
			public Boolean? IsOrigDocDiscount { get; set; }
			public String ExtDiscCode { get; set; }
			public String Description { get; set; }
		} 
		#endregion

		public static bool ApplyQuantityDiscountByBaseUOMForAP(PXGraph graph) => ApplyQuantityDiscountByBaseUOM(graph).ForAP;
		public static bool ApplyQuantityDiscountByBaseUOMForAR(PXGraph graph) => ApplyQuantityDiscountByBaseUOM(graph).ForAR;

		internal static ApplyQuantityDiscountByBaseUOMOption ApplyQuantityDiscountByBaseUOM(PXGraph graph)
		{
			if (!PXAccess.FeatureInstalled<FeaturesSet.customerDiscounts>() && !PXAccess.FeatureInstalled<FeaturesSet.vendorDiscounts>())
				return new ApplyQuantityDiscountByBaseUOMOption(false, false);

			ARSetup arSetup = PXSelect<ARSetup>.SelectSingleBound(graph, null);
			APSetup apSetup = PXSelect<APSetup>.SelectSingleBound(graph, null);

			if (!PXAccess.FeatureInstalled<FeaturesSet.customerDiscounts>())
			{
				return new ApplyQuantityDiscountByBaseUOMOption(
					apSetup?.ApplyQuantityDiscountBy == ApplyQuantityDiscountType.BaseUOM,
					false);
			}

			if (!PXAccess.FeatureInstalled<FeaturesSet.vendorDiscounts>())
			{
				return new ApplyQuantityDiscountByBaseUOMOption(
					false,
					arSetup?.ApplyQuantityDiscountBy == ApplyQuantityDiscountType.BaseUOM);
			}

			return new ApplyQuantityDiscountByBaseUOMOption(
				apSetup?.ApplyQuantityDiscountBy == ApplyQuantityDiscountType.BaseUOM,
				arSetup?.ApplyQuantityDiscountBy == ApplyQuantityDiscountType.BaseUOM);
		}

		internal struct ApplyQuantityDiscountByBaseUOMOption
		{
			public readonly bool ForAP;
			public readonly bool ForAR;

			public ApplyQuantityDiscountByBaseUOMOption(bool forAP, bool forAR)
			{
				ForAP = forAP;
				ForAR = forAR;
			}
		}

		[Flags]
		public enum DiscountCalculationOptions
		{
			[Description("Calculate all Automatic discounts. Default state: set.")]
			CalculateAll = 0,
			[Description("Set this flag to disable free-item discounts. Default state: not set.")]
			DisableFreeItemDiscountsCalculation = 1,
			[Description("Set this flag to disable automatic Group and Document discounts calculation. Default state: not set.")]
			DisableGroupAndDocumentDiscounts = 2,
			[Description("Set this flag to disable automatic discount calculation. Discounts, that already present in the document and marked as Manual, will be kept in a valid state. Overrides DisableGroupAndDocumentDiscounts option. Default state: not set.")]
			DisableAllAutomaticDiscounts = 4,
			[Description("Set this flag to disable AR discount calculation. Default state: not set.")]
			DisableARDiscountsCalculation = 8,
			[Description("Set this flag to disable AP discount calculation. Default state: not set.")]
			DisableAPDiscountsCalculation = 16,
			[Description("Set this flag to disable price/cost calculation. Default state: set for AP")]
			DisablePriceCalculation = 32,
			[Description("Set this flag to enable automatic discount calculation on import. Default state: not set")]
			CalculateDiscountsFromImport = 64,
			[Description("Set this flag to enable optimization of Group and Document discount calculation procedure. Default state: not set")]
			EnableOptimizationOfGroupAndDocumentDiscountsCalculation = 128
		}

		public const DiscountCalculationOptions DefaultDiscountCalculationParameters =
			DiscountCalculationOptions.CalculateAll;
		
		/// <summary>
		/// Default option for AR. AP discounts disabled.
		/// </summary>
		public const DiscountCalculationOptions DefaultARDiscountCalculationParameters =
			DiscountCalculationOptions.CalculateAll | DiscountCalculationOptions.DisableAPDiscountsCalculation;
		
		/// <summary>
		/// Default option for AP. AR discounts and Free-item discounts disabled.
		/// </summary>
		public const DiscountCalculationOptions DefaultAPDiscountCalculationParameters =
			DiscountCalculationOptions.CalculateAll | DiscountCalculationOptions.DisableARDiscountsCalculation | DiscountCalculationOptions.DisableFreeItemDiscountsCalculation;
		
		protected static decimal RoundDiscountRate(decimal discountRate) => Math.Round(discountRate, 18, MidpointRounding.AwayFromZero);
	}

	public static class ConcurrentDictionaryEx
	{
		public static bool Remove<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> self, TKey key)
		{
			return ((IDictionary<TKey, TValue>)self).Remove(key);
		}
	}
}