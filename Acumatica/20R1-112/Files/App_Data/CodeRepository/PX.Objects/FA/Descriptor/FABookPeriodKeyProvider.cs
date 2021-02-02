using System;
using System.Collections.Generic;
using System.Linq;
using PX.Data;
using PX.Objects.Common;
using PX.Objects.Common.Extensions;
using PX.Objects.CS;
using PX.Objects.GL.Descriptor;

namespace PX.Objects.FA.Descriptor
{
	public class FABookPeriodKeyProvider :
		PeriodKeyProviderBase<
			PeriodKeyProviderBase.SourcesSpecificationCollection<FABookPeriodKeyProvider.FASourceSpecificationItem>,
			FABookPeriodKeyProvider.FASourceSpecificationItem,
			PeriodKeyProviderBase.KeyWithSourceValuesCollection<
				FABookPeriodKeyProvider.FAKeyWithSourceValues, 
				FABookPeriodKeyProvider.FASourceSpecificationItem,
				FABookPeriod.Key>,
			FABookPeriodKeyProvider.FAKeyWithSourceValues,
			FABookPeriod.Key>
	{
		#region Types

		public class FASourceSpecificationItem : SourceSpecificationItem
		{
			public virtual Type BookSourceType { get; set; }

			public virtual Type AssetSourceType { get; set; }

			public bool IsBookRequired { get; set; }

		    public override bool IsAnySourceSpecified => base.IsAnySourceSpecified
		                                                 || AssetSourceType != null
		                                                 || BookSourceType != null;

		    protected override List<Type> BuildSourceFields(PXCache cache)
		    {
		        List<Type> fields = base.BuildSourceFields(cache);

		        if (BookSourceType != null)
		        {
		            fields.Add(BookSourceType);
		        }

		        if (AssetSourceType != null)
		        {
		            fields.Add(AssetSourceType);
		        }

		        return fields;
		    }

        }

		public class FAKeyWithSourceValues: KeyWithSourceValues<FASourceSpecificationItem, FABookPeriod.Key>
		{
			public virtual List<int?> SourceAssetIDs { get; set; }

			public virtual List<int?> SourceBookIDs { get; set; }

		    public override bool SourcesEqual(object otherObject)
		    {
		        var otherKeyWithSourceValues = (FAKeyWithSourceValues)otherObject;

		        return base.SourcesEqual(otherObject)
		               && SourceAssetIDs.OrderBy(v => v).SequenceEqual(otherKeyWithSourceValues.SourceAssetIDs.OrderBy(v => v))
		               && SourceBookIDs.OrderBy(v => v).SequenceEqual(otherKeyWithSourceValues.SourceBookIDs.OrderBy(v => v));
		    }
        }

		#endregion

		public override int? MasterValue => FABookPeriod.organizationID.NonPostingBookValue;

		public FABookPeriodKeyProvider(
			SourcesSpecificationCollection<FASourceSpecificationItem> sourcesSpecification = null,
			Type[] sourceSpecificationTypes = null,
			bool useMasterOrganizationIDByDefault = false)
			: base(
				sourcesSpecification: sourcesSpecification,
				sourceSpecificationTypes: sourceSpecificationTypes,
				useMasterOrganizationIDByDefault: useMasterOrganizationIDByDefault)
		{
		}

		protected class ParameterEvaluator<TEnum>
			where TEnum : struct, IConvertible
		{
			protected Dictionary<TEnum, object> Parameters = new Dictionary<TEnum, object>();
			public ParameterEvaluator(TEnum mask, object[] parameters)
			{
				if (!typeof(TEnum).IsEnum)
				{
					throw new ArgumentException("TEnum must be an enumerated type.");
				}

				Queue<object> queuedParams = new Queue<object>(parameters);
				foreach (TEnum flag in (TEnum[])Enum.GetValues(typeof(TEnum)))
				{
					if(((int)(object)flag & (int)(object)mask) > 0)
					{
						Parameters.Add(flag, queuedParams.Dequeue());
					}
				}
			}

			public object this[TEnum index]
			{
				get
				{
					Parameters.TryGetValue(index, out object value);
					return value;
				}
			}
		}

		public virtual FABookPeriod.Key GetKeyFromReportParameters(
		   PXGraph graph,
		   object[] parameters,
		   ReportParametersFlag reportParametersMask)
		{
			ParameterEvaluator<ReportParametersFlag> parameterEvaluator = new ParameterEvaluator<ReportParametersFlag>(reportParametersMask, parameters);

			HashSet<int?> branchIDs = new HashSet<int?>();
			int? branchID = (int?)parameterEvaluator[ReportParametersFlag.Branch];
			if (branchID != null)
			{
				branchIDs.Add(branchID);
			}
			branchIDs.AddRange(PXAccess.GetBranchIDsByBAccount((int?)parameterEvaluator[ReportParametersFlag.BAccount]).Cast<int?>());

			FAKeyWithSourceValues keyWithSourceValuesItem = EvaluateRawKey(graph,
				new FAKeyWithSourceValues()
				{
					SpecificationItem = CachedSourcesSpecification.MainSpecificationItem,
					SourceOrganizationIDs = ((int?)parameterEvaluator[ReportParametersFlag.Organization]).SingleToList(),
					SourceBranchIDs = branchIDs.ToList(),
					SourceAssetIDs = ((int?)parameterEvaluator[ReportParametersFlag.FixedAsset]).SingleToList(),
					SourceBookIDs = ((int?)parameterEvaluator[ReportParametersFlag.Book]).SingleToList()
				});

			if (keyWithSourceValuesItem.Key.OrganizationID == null && UseMasterOrganizationIDByDefault)
			{
				keyWithSourceValuesItem.Key.OrganizationID = MasterValue;
			}

			return keyWithSourceValuesItem.Key;
		}

		#region OrganizationID Pre-Calculation Services

		protected override FAKeyWithSourceValues CollectSourceValues(
			PXGraph graph, PXCache attributeCache, object extRow, FASourceSpecificationItem specificationItem)
		{
			if (specificationItem == null)
				return null;

			FAKeyWithSourceValues valuesCollectionItem =
				base.CollectSourceValues(graph, attributeCache, extRow, specificationItem);

			valuesCollectionItem.SourceAssetIDs =
				GetAssetIDsValueFromField(graph, attributeCache, extRow, specificationItem).SourceAssetIDs;

			valuesCollectionItem.SourceBookIDs =
				GetBookIDsValueFromField(graph, attributeCache, extRow, specificationItem).SourceBookIDs;

			return valuesCollectionItem;
		}

		protected override FAKeyWithSourceValues EvaluateRawKey(PXGraph graph,
			FAKeyWithSourceValues keyWithSourceValues)
		{
			if (keyWithSourceValues == null)
				return null;

			FABook book = BookMaint.FindByID(graph, keyWithSourceValues.SourceBookIDs.First());

			if (book == null)
			{
				if (keyWithSourceValues.SpecificationItem.IsBookRequired)
				{
					return keyWithSourceValues;
				}
				else
				{
					book = BookMaint.FindByBookMarker(graph, FABook.bookID.Markers.GLOrAnyBook);
				}
			}

			if (book == null)
			{
				return keyWithSourceValues;
			}

			keyWithSourceValues.Key.SetBookID(book);

			if (book.UpdateGL == true)
			{
				if (!PXAccess.FeatureInstalled<FeaturesSet.branch>())
					keyWithSourceValues.KeyOrganizationIDs = PXAccess.GetParentOrganizationID(PXAccess.GetBranchID()).SingleToList();
				else
				{
					keyWithSourceValues.KeyOrganizationIDs = keyWithSourceValues.SourceOrganizationIDs;

					if (IsIDsUndefined(keyWithSourceValues.KeyOrganizationIDs))
					{
						if (!IsIDsUndefined(keyWithSourceValues.SourceBranchIDs))
						{
							keyWithSourceValues.KeyOrganizationIDs =
								keyWithSourceValues.SourceBranchIDs
									.Select(branchID => PXAccess.GetParentOrganizationID(branchID))
									.ToList();
						}
						else
						{
							if (!IsIDsUndefined(keyWithSourceValues.SourceAssetIDs))
							{
								keyWithSourceValues.KeyOrganizationIDs =
									keyWithSourceValues.SourceAssetIDs
										.Select(assetID => PXAccess.GetParentOrganizationID(AssetMaint.FindByID(graph, assetID)?.BranchID))
										.ToList();
							}
							else if (!PXAccess.FeatureInstalled<FeaturesSet.multipleCalendarsSupport>())
							{
								keyWithSourceValues.KeyOrganizationIDs =
									((int?)FABookPeriod.organizationID.NonPostingBookValue).SingleToList();
							}
						}
					}
				}
			}
			else
			{
				keyWithSourceValues.KeyOrganizationIDs =
					((int?)FABookPeriod.organizationID.NonPostingBookValue).SingleToList();
			}

			keyWithSourceValues.Key.OrganizationID = keyWithSourceValues.KeyOrganizationIDs.First();

			return keyWithSourceValues;
		}

		#endregion

		#region Values Extraction

		public virtual FAKeyWithSourceValues GetAssetIDsValueFromField(
			PXGraph graph,
			PXCache attributeCache,
			object extRow,
			FASourceSpecificationItem sourceSpecification)
		{
			return GetIDsValueFromField(graph, attributeCache, extRow, sourceSpecification,
				sourceSpecification.AssetSourceType, (item, value) => item.SourceAssetIDs = value);
		}

		public virtual FAKeyWithSourceValues GetBookIDsValueFromField(
			PXGraph graph,
			PXCache attributeCache,
			object extRow,
			FASourceSpecificationItem sourceSpecification)
		{
			if (typeof(IBqlField).IsAssignableFrom(sourceSpecification.BookSourceType))
			{
				return GetIDsValueFromField(graph, attributeCache, extRow, sourceSpecification,
					sourceSpecification.BookSourceType, (item, value) => item.SourceBookIDs = value);
			}

			List<int?> bookIDs = new List<int?> { null };
			if (typeof(IBqlSearch).IsAssignableFrom(sourceSpecification.BookSourceType))
			{
				bookIDs = ((int?)((IBqlSearch)sourceSpecification.BookSourceType).SelectFirst(graph, extRow)).SingleToList();
			}

			return new FAKeyWithSourceValues()
			{
				SpecificationItem = sourceSpecification,
				SourceBookIDs = bookIDs
			};
		}

		#endregion
	}
}
