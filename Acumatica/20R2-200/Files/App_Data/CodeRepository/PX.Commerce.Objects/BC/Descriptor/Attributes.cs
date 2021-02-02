using PX.Commerce.Core;
using PX.Data;
using PX.Objects.AR;
using PX.Objects.CS;
using PX.Objects.IN;
using PX.Objects.IN.RelatedItems;
using PX.Objects.SO;
using PX.Objects.TX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PX.Commerce.Objects
{
	#region BCItemVisibilityAttribute
	public class BCItemVisibilityAttribute : PXStringListAttribute
	{
		public const string Visible = "V";
		public const string Featured = "F";
		public const string Invisible = "I";

		public BCItemVisibilityAttribute() :
			base(
				new[] {
					Visible,
					Featured,
					Invisible,
				},
				new[]
				{
					BCCaptions.Visible,
					BCCaptions.Featured,
					BCCaptions.Invisible,
				})
		{
		}


	}
	#endregion
	#region BCItemVisibilityAttribute
	public class BCPostDiscountAttribute : PXStringListAttribute
	{
		public const string LineDiscount = "L";
		public const string DocumentDiscount = "D";

		public BCPostDiscountAttribute() :
			base(
				new[] {
					LineDiscount,
					DocumentDiscount,
				},
				new[]
				{
					BCObjectsMessages.LineDiscount,
					BCObjectsMessages.DocumentDiscount,
				})
		{
		}
	}
	#endregion
	#region BCItemFileTypeAttribute
	public class BCFileTypeAttribute : PXStringListAttribute
	{
		public const string Image = "I";
		public const string Video = "V";

		public BCFileTypeAttribute() :
			base(
				new[] {
					Image,
					Video,
				},
				new[]
				{
					BCCaptions.Image,
					BCCaptions.Video,
				})
		{
		}
	}
	#endregion

	#region BCAvailabilityLevelsAttribute
	public class BCAvailabilityLevelsAttribute : PXStringListAttribute
	{
		public const string Available = "A";
		public const string AvailableForShipping = "S";
		public const string OnHand = "H";

		public BCAvailabilityLevelsAttribute() :
			base(
				new[] {
					Available,
					AvailableForShipping,
					OnHand,
				},
				new[]
				{
					BCCaptions.Available,
					BCCaptions.AvailableForShipping,
					BCCaptions.OnHand,
				})
		{

		}

		public sealed class available : PX.Data.BQL.BqlString.Constant<available>
		{
			public available() : base(Available)
			{
			}
		}
		public sealed class availableForShipping : PX.Data.BQL.BqlString.Constant<availableForShipping>
		{
			public availableForShipping() : base(AvailableForShipping)
			{
			}
		}
		public sealed class onHand : PX.Data.BQL.BqlString.Constant<onHand>
		{
			public onHand() : base(OnHand)
			{
			}
		}
	}
	#endregion
	#region BCWarehouseModeAttribute
	public class BCWarehouseModeAttribute : PXStringListAttribute
	{
		public const string AllWarehouse = "A";
		public const string SpecificWarehouse = "S";

		public BCWarehouseModeAttribute() :
				base(
					new[]
					{
						AllWarehouse,
						SpecificWarehouse},
					new[]
					{
						BCCaptions.AllWarehouse,
						BCCaptions.SpecificWarehouse
					})
		{ }
		public sealed class allWarehouse : PX.Data.BQL.BqlString.Constant<allWarehouse>
		{
			public allWarehouse() : base(AllWarehouse)
			{
			}
		}
		public sealed class specificWarehouse : PX.Data.BQL.BqlString.Constant<specificWarehouse>
		{
			public specificWarehouse() : base(SpecificWarehouse)
			{
			}
		}
	}
	#endregion

	#region BCSalesCategoriesExportAttribute
	public class BCSalesCategoriesExportAttribute : PXStringListAttribute
	{
		public const string DoNothing = "N";
		public const string ExportAsTags = "E";

		public BCSalesCategoriesExportAttribute() :
				base(
					new[]
					{
						DoNothing,
						ExportAsTags},
					new[]
					{
						BCCaptions.DoNothing,
						BCCaptions.CategoryAsTags
					})
		{ }
	}
	#endregion

	#region BCShopifyStorePlanAttribute
	public class BCShopifyStorePlanAttribute : PXStringListAttribute
	{
		public const string LitePlan = "LP";
		public const string BasicPlan = "BP";
		public const string NormalPlan = "NP";
		public const string AdvancedPlan = "AP";
		public const string PlusPlan = "PP";

		public BCShopifyStorePlanAttribute() :
				base(
					new[]
					{
						LitePlan,
						BasicPlan,
						NormalPlan,
						AdvancedPlan,
						PlusPlan},
					new[]
					{
						BCCaptions.ShopifyLitePlan,
						BCCaptions.ShopifyBasicPlan,
						BCCaptions.ShopifyNormalPlan,
						BCCaptions.ShopifyAdvancedPlan,
						BCCaptions.ShopifyPlusPlan
					})
		{ }
	}
	#endregion

	#region BCItemAvailabilityAttribute
	public class BCItemAvailabilities
	{
		public const string StoreDefault = "X";
		public const string AvailableTrack = "T";
		public const string AvailableSkip = "S";
		public const string PreOrder = "P";
		public const string Disabled = "D";

		public class List : PXStringListAttribute
		{
			public List() :
				base(
					new[] {
						AvailableTrack,
						AvailableSkip,
						PreOrder,
						Disabled,
					},
					new[]
					{
						BCCaptions.AvailableTrack,
						BCCaptions.AvailableSkip,
						BCCaptions.PreOrder,
						BCCaptions.Disabled,
					})
			{
			}
		}
		public class ListDef : PXStringListAttribute, IPXRowSelectedSubscriber
		{
			public ListDef() :
				base(
					new[] {
						StoreDefault,
						AvailableTrack,
						AvailableSkip,
						PreOrder,
						Disabled,
					},
					new[]
					{
						BCCaptions.StoreDefault,
						BCCaptions.AvailableTrack,
						BCCaptions.AvailableSkip,
						BCCaptions.PreOrder,
						BCCaptions.Disabled,
					})
			{
			}

			public void RowSelected(PXCache sender, PXRowSelectedEventArgs e)
			{
				InventoryItem row = e.Row as InventoryItem;

				if (row != null)
				{
					if (row.StkItem == false)
					{
						var list = new BCItemAvailabilities.NonStockAvailability().ValueLabelDic;

						PXStringListAttribute.SetList<BCInventoryItem.availability>(sender, row, list.Keys.ToArray(), list.Values.ToArray());
						sender.Adjust<PXUIFieldAttribute>(row).For<BCInventoryItem.notAvailMode>(fa => fa.Visible = false);


					}
					else
					{
						var list = new BCItemAvailabilities.ListDef().ValueLabelDic;

						PXStringListAttribute.SetList<BCInventoryItem.availability>(sender, row, list.Keys.ToArray(), list.Values.ToArray());
						sender.Adjust<PXUIFieldAttribute>(row).For<BCInventoryItem.notAvailMode>(fa => fa.Visible = true);
					}
				}
			}
		}
		public class NonStockAvailability : PXStringListAttribute
		{
			public NonStockAvailability() :
				base(
					new[] {
						StoreDefault,
						AvailableSkip,
						PreOrder,
						Disabled,
					},
					new[]
					{
						BCCaptions.StoreDefault,
						BCCaptions.AvailableSkip,
						BCCaptions.PreOrder,
						BCCaptions.Disabled,
					})
			{
			}
		}
		public static string Convert(String val)
		{
			switch (val)
			{
				case StoreDefault: return BCCaptions.StoreDefault;
				case AvailableTrack: return BCCaptions.AvailableTrack;
				case AvailableSkip: return BCCaptions.AvailableSkip;
				case PreOrder: return BCCaptions.PreOrder;
				case Disabled: return BCCaptions.Disabled;
				default: return null;
			}
		}

		public sealed class storeDefault : PX.Data.BQL.BqlString.Constant<storeDefault>
		{
			public storeDefault() : base(StoreDefault)
			{
			}
		}
		public sealed class availableTrack : PX.Data.BQL.BqlString.Constant<availableTrack>
		{
			public availableTrack() : base(AvailableTrack)
			{
			}
		}
		public sealed class availableSkip : PX.Data.BQL.BqlString.Constant<availableSkip>
		{
			public availableSkip() : base(AvailableSkip)
			{
			}
		}
		public sealed class preOrder : PX.Data.BQL.BqlString.Constant<preOrder>
		{
			public preOrder() : base(PreOrder)
			{
			}
		}
		public sealed class disabled : PX.Data.BQL.BqlString.Constant<disabled>
		{
			public disabled() : base(Disabled)
			{
			}
		}
	}
	#endregion
	#region BCItemNotAvailModeAttribute
	public class BCItemNotAvailModes
	{
		public const string StoreDefault = "X";
		public const string DoNothing = "N";
		public const string DisableItem = "D";
		public const string PreOrderItem = "P";

		public class List : PXStringListAttribute
		{
			public List() :
			base(
				new[] {
					DoNothing,
					DisableItem,
					PreOrderItem,
				},
				new[]
				{
					BCCaptions.DoNothing,
					BCCaptions.DisableItem,
					BCCaptions.PreOrderItem,
				})
			{
			}
		}
		public class ListDef : PXStringListAttribute
		{
			public ListDef() :
			base(
				new[] {
					StoreDefault,
					DoNothing,
					DisableItem,
					PreOrderItem,
				},
				new[]
				{
					BCCaptions.StoreDefault,
					BCCaptions.DoNothing,
					BCCaptions.DisableItem,
					BCCaptions.PreOrderItem,
				})
			{
			}
		}

		public static string Convert(String val)
		{
			switch (val)
			{
				case StoreDefault: return BCCaptions.StoreDefault;
				case DoNothing: return BCCaptions.DoNothing;
				case DisableItem: return BCCaptions.DisableItem;
				case PreOrderItem: return BCCaptions.PreOrderItem;
				default: return null;
			}
		}
	}
	#endregion

	#region BCDimensionMaskAttribute
	public class BCDimensionMaskAttribute : BCDimensionAttribute, IPXRowSelectedSubscriber, IPXRowSelectingSubscriber, IPXFieldDefaultingSubscriber, IPXFieldUpdatedSubscriber
	{
		protected Type NewNumbering;

		public BCDimensionMaskAttribute(String dimension, Type numbering)
			: base(dimension)
		{
			if (numbering == null) throw new ArgumentException("numbering");

			NewNumbering = numbering;
		}
		public override void CacheAttached(PXCache sender)
		{
			SetSegmentDelegate(new PXSelectDelegate<short?, string>(BCSegmentSelect));

			base.CacheAttached(sender);

			sender.Graph.FieldVerifying.AddHandler(_BqlTable, NewNumbering.Name, NumberingFieldVerifying);
		}

		public System.Collections.IEnumerable BCSegmentSelect([PXShort] short? segment, [PXString] string value)
		{
			if (segment == 0)
			{
				yield return new SegmentValue(new String('#', _Definition.Dimensions[_Dimension].Sum(s => s.Length)), "Auto Numbering", false);
			}
			if (segment > 0)
			{
				PXSegment seg = segment != null ? _Definition.Dimensions[_Dimension][segment.Value - 1] : _Definition.Dimensions[_Dimension].FirstOrDefault();
				if (!seg.Validate) yield return new SegmentValue(new String('#', seg.Length), "Auto Numbering", false);
			}

			foreach (SegmentValue segmentValue in base.SegmentSelect(segment, value))
				yield return segmentValue;
		}

		public override void RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			//Suppress Auto-Numbering
			//base.RowPersisting(sender, e);
		}
		public virtual void RowSelecting(PXCache sender, PXRowSelectingEventArgs e)
		{
			Boolean enabled = GetAutoNumbering(_Dimension) == null && GetSegments(_Dimension).Count() > 1;
			if (!enabled) sender.SetValue(e.Row, _FieldOrdinal, null);
		}
		public virtual void RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			Boolean enabled = /*GetAutoNumbering(_Dimension) == null && */GetSegments(_Dimension).Count() > 1;

			PXUIFieldAttribute.SetEnabled(sender, e.Row, _FieldName, enabled);
		}
		public override void FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			base.FieldVerifying(sender, e);

			//Validate Mask
			string mask = e.NewValue as String;
			if (mask == null) return;

			Int32 index = 0, count = 0;
			Int32 autoSegment = -1;
			foreach (PXSegment seg in GetSegments(_Dimension))
			{
				//Replace after merge
				short segmentID = (short)seg.GetType().InvokeMember("SegmentID", BindingFlags.GetField | BindingFlags.Public | BindingFlags.Instance, null, seg, new object[0]);

				if (mask.Length < index + seg.Length) throw new PXSetPropertyException(BCMessages.InvalidMaskLength);
				String part = mask.Substring(index, seg.Length);

				if (part.StartsWith("#"))
				{
					if (autoSegment >= 0) throw new PXSetPropertyException(BCMessages.MultipleAutoNumberSegments);
					autoSegment = segmentID;
				}
				else if (seg.Validate)
				{
					Dictionary<String, ValueDescr> dict = PXDatabaseGetSlot().Values[_Dimension][segmentID];
					if (!dict.ContainsKey(part)) throw new PXSetPropertyException(BCMessages.InvalidSegmentValue);
				}

				index += seg.Length;
				count++;
			}
			if (count > 1 && autoSegment < 0) throw new PXSetPropertyException(BCMessages.InvalidAutoNumberSegment);
		}
		public override void FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			base.FieldDefaulting(sender, e);

			string mask = String.Empty;

			foreach (PXSegment seg in GetSegments(_Dimension))
			{
				//Replace after merge
				short segmentID = (short)seg.GetType().InvokeMember("SegmentID", BindingFlags.GetField | BindingFlags.Public | BindingFlags.Instance, null, seg, new object[0]);
				bool autonumber = (bool)seg.GetType().InvokeMember("AutoNumber", BindingFlags.GetField | BindingFlags.Public | BindingFlags.Instance, null, seg, new object[0]);

				if (autonumber) mask += new String('#', seg.Length);
				else if (seg.Validate)
				{
					Dictionary<String, ValueDescr> dict = PXDatabaseGetSlot().Values[_Dimension][segmentID];
					mask += dict.FirstOrDefault().Key;
				}
				else mask += new String(' ', seg.Length);
			}

			e.NewValue = mask;
		}
		public virtual void FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			Object val = sender.GetValue(e.Row, NewNumbering.Name);
			sender.RaiseFieldVerifying(NewNumbering.Name, e.Row, ref val);
		}
		public virtual void NumberingFieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			base.FieldVerifying(sender, e);

			string numb = (string)e.NewValue;
			string mask = (string)sender.GetValue(e.Row, _FieldOrdinal);
            if (numb == null || mask == null)
                return;
			Int32 index = 0;
			Int32 autoSegmentLength = -1;
			foreach (PXSegment seg in GetSegments(_Dimension))
			{
				//bool autonumber = (bool)seg.GetType().InvokeMember("AutoNumber", BindingFlags.GetField | BindingFlags.Public | BindingFlags.Instance, null, seg, new object[0]);

				if ((mask != null && mask.Substring(index, seg.Length).StartsWith("#")))
				{
					autoSegmentLength = seg.Length;
				}

				index += seg.Length;
			}

			PX.Objects.CS.NumberingSequence seq = PX.Objects.CS.AutoNumberAttribute.GetNumberingSequence(numb, null, sender.Graph.Accessinfo.BusinessDate);
			if (autoSegmentLength > 0 && seq?.LastNbr?.Length != autoSegmentLength || seq?.LastNbr?.Length > index)
			{
				throw new PXSetPropertyException(BCMessages.InvalidNumberingLength);
			}
		}
	}
	#endregion
	#region BCCustomNumberingAttribute
	public class BCCustomNumberingAttribute : PXEventSubscriberAttribute, IPXRowPersistingSubscriber
	{
		protected String Dimension;
		protected Type Mask;
		protected Type Numbering;
		protected Type NumberingSelect;

		public BCCustomNumberingAttribute(String dimension, Type mask, Type numbering, Type select)
		{
			Dimension = dimension ?? throw new ArgumentException("dimension");
			Mask = mask ?? throw new ArgumentException("mask");
			Numbering = numbering ?? throw new ArgumentException("numbering");
			NumberingSelect = select ?? throw new ArgumentException("select");
		}

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);

			PXCache bindingCache = sender.Graph.Caches[BqlCommand.GetItemType(Mask)]; // Initialize cache in advance to allow DimensionSelector from GuesCustomer fire events on persisting
			sender.Graph.RowPersisting.AddHandler(_BqlTable, RowPersisting);
		}

		public void RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			if ((e.Operation & PXDBOperation.Command) != PXDBOperation.Insert)
				return;

			BCAPISyncScope.BCSyncScopeContext context = BCAPISyncScope.GetScoped();
			if (context == null) return;

			PXView view = new PXView(sender.Graph, true, BqlCommand.CreateInstance(NumberingSelect));
			Object store = view.SelectSingle(context.ConnectorType, context.Binding);

			String mask = (String)sender.Graph.Caches[BqlCommand.GetItemType(Mask)]
				.GetValue(store is PXResult ? (store as PXResult)[BqlCommand.GetItemType(Mask)] : store, Mask.Name);
			String numbering = (String)sender.Graph.Caches[BqlCommand.GetItemType(Numbering)]
				.GetValue(store is PXResult ? (store as PXResult)[BqlCommand.GetItemType(Mask)] : store, Numbering.Name);

			Int32 index = 0;
			Int32 segment = -1;
			for (int i = 0; i < BCDimensionMaskAttribute.GetSegments(Dimension).Count(); i++)
			{
				PXSegment seg = BCDimensionMaskAttribute.GetSegments(Dimension).ElementAt(i);

				if (mask == null || mask.Substring(index, seg.Length).StartsWith("#"))
				{
					segment = i + 1;
					break;
				}

				index += seg.Length;
			}

			if (mask != null) sender.SetValue(e.Row, _FieldOrdinal, mask);
			if (numbering != null) PXDimensionAttribute.SetCustomNumbering(sender, _FieldName, numbering, segment < 0 ? (int?)null : (int?)segment);
		}
	}
	#endregion

	#region BCAutoNumberAttribute
	public class BCAutoNumberAttribute : AutoNumberAttribute
	{
		public BCAutoNumberAttribute(Type setupField, Type dateField)
			: base(null, dateField, new string[] { }, new Type[] { setupField })
		{
		}

		public static void CheckAutoNumbering(PXGraph graph, string numberingID)
		{
			Numbering numbering = null;

			if (numberingID != null)
			{
				numbering = (Numbering)PXSelect<Numbering,
								Where<Numbering.numberingID, Equal<Required<Numbering.numberingID>>>>
								.Select(graph, numberingID);
			}

			if (numbering == null)
			{
				throw new PXSetPropertyException(PX.Objects.CS.Messages.NumberingIDNull);
			}

			if (numbering.UserNumbering == true)
			{
				throw new PXSetPropertyException(PX.Objects.CS.Messages.CantManualNumber, numbering.NumberingID);
			}
		}
	}
	#endregion
	#region SalesCategoriesAttribute
	public class SalesCategoriesAttribute : PXStringListAttribute
	{
		protected bool _Check = false;
		protected Tuple<String, String>[] _Values = new Tuple<string, string>[0];

		public SalesCategoriesAttribute() : base(new string[] { }, new string[] { })
		{
			MultiSelect = true;
		}

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);

			INCategory[] categories = PXSelect<INCategory>.Select(sender.Graph).Select(r => r.GetItem<INCategory>()).ToArray();
			_Values = categories.Select(i => Tuple.Create(i.CategoryID.ToString(), i.Description)).ToArray();

			_AllowedValues = _Values.Select(t => t.Item1).ToArray();
			_AllowedLabels = _Values.Select(t => t.Item2).ToArray();
		}

		public override void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			base.FieldSelecting(sender, e);

			if (_Check == true)
				((PXFieldState)e.ReturnState).Required = true;
			else
				((PXFieldState)e.ReturnState).Required = false;
		}

		public static void SetCheck(PXCache sender, String fieldName, Object row, Boolean value)
		{
			foreach (PXEventSubscriberAttribute attr in sender.GetAttributes(row, fieldName))
			{
				if (attr is SalesCategoriesAttribute)
				{
					((SalesCategoriesAttribute)attr)._Check = value;
				}
			}
		}
	}
	#endregion
	#region RelatedItemsAttribute
	public class RelatedItemsAttribute : PXStringListAttribute
	{
		public RelatedItemsAttribute() :
				base(
					new[] {
						CrossSell,
						Related,
						UpSell,
					},
					new[]
					{
						InventoryRelation.Desc.CrossSell,
						InventoryRelation.Desc.Related,
						InventoryRelation.Desc.UpSell,
					})
		{
			MultiSelect = true;
		}
		public sealed class crossSell : PX.Data.BQL.BqlString.Constant<crossSell>
		{
			public crossSell() : base(CrossSell)
			{
			}
		}
		public sealed class related : PX.Data.BQL.BqlString.Constant<related>
		{
			public related() : base(Related)
			{
			}
		}
		public sealed class upSell : PX.Data.BQL.BqlString.Constant<upSell>
		{
			public upSell() : base(UpSell)
			{
			}
		}
		public const string CrossSell = InventoryRelation.CrossSell;
		public const string Related = InventoryRelation.Related;
		public const string UpSell = InventoryRelation.UpSell;
	}
	#endregion
	#region BCSettingsCheckerAttribute
	public class BCSettingsCheckerAttribute : PXEventSubscriberAttribute
	{
		private bool _MakeMandatory = false;
		public string[] _AppliedEntities = new string[0];

		public BCSettingsCheckerAttribute(string[] appliedEntities)
		{
			_AppliedEntities = appliedEntities;
		}
		public bool EntityApplied(string entityCode)
		{
			return (_AppliedEntities.Contains(entityCode));
		}

		public void SetMandatory()
		{
			_MakeMandatory = true;
		}
		public bool FieldRequired()
		{
			return _MakeMandatory;
		}
	}
	#endregion

	#region BCTaxSyncAttribute
	public class BCTaxSyncAttribute : PXStringListAttribute
	{
		public const string NoSync = "N";
		public const string ManualTaxes = "M";
		public const string AutomaticTaxes = "A";

		public BCTaxSyncAttribute() :
				base(
					new[] {
						NoSync,
						ManualTaxes,
						AutomaticTaxes,
					},
					new[]
					{
						BCCaptions.NoSync,
						BCCaptions.ManualTaxes,
						BCCaptions.AutomaticTaxes,
					})
		{ }

		public sealed class noSync : PX.Data.BQL.BqlString.Constant<noSync>
		{
			public noSync() : base(NoSync)
			{
			}
		}
		public sealed class manualTaxes : PX.Data.BQL.BqlString.Constant<manualTaxes>
		{
			public manualTaxes() : base(ManualTaxes)
			{
			}
		}
		public sealed class automaticTaxes : PX.Data.BQL.BqlString.Constant<automaticTaxes>
		{
			public automaticTaxes() : base(AutomaticTaxes)
			{
			}
		}
	}
	#endregion
}
