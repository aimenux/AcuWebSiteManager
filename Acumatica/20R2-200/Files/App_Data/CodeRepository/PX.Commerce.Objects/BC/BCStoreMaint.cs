using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Api;
using PX.Data;
using PX.Commerce.Core;
using PX.Objects.AR;
using PX.Objects.CS;
using PX.Data.PushNotifications;

namespace PX.Commerce.Objects
{
	public abstract class BCStoreMaint : BCBindingMaint
	{
		public PXSelect<BCBindingExt, Where<BCBindingExt.bindingID, Equal<Current<BCBinding.bindingID>>>> CurrentStore;

		[PXCopyPasteHiddenView]
		public PXSelect<BCLocations, Where<BCLocations.bindingID, Equal<Current<BCBinding.bindingID>>>> Locations;

		public BCStoreMaint()
		{
			//this is needed to navigate GiftCertificate to the Non Stock item. Without this code, it will go to stock item if the field is empty.
			FieldDefaulting.AddHandler<PX.Objects.IN.InventoryItem.stkItem>(delegate (PXCache cache, PXFieldDefaultingEventArgs e) { e.NewValue = false; });
		}

		public override void Clear()
		{
			base.Clear();
		}

		#region BCBinding Events
		[PXMergeAttributes(Method = MergeMethod.Append)]
		[BCSettingsChecker(new string[] { BCEntitiesAttribute.Order })]
		public virtual void BCBinding_BranchID_CacheAttached(PXCache sender) { }
		public virtual void _(Events.FieldDefaulting<BCBindingExt, BCBindingExt.taxSubstitutionListID> e)
		{
			BCBindingExt row = e.Row;

			if (row != null && base.CurrentBinding.Current.ConnectorType != null)
			{
				e.NewValue = CurrentBinding.Current.ConnectorType + BCObjectsConstants.TaxCodes;
			}
		}
		public virtual void _(Events.FieldDefaulting<BCBindingExt, BCBindingExt.taxCategorySubstitutionListID> e)
		{
			BCBindingExt row = e.Row;

			if (row != null && base.CurrentBinding.Current.ConnectorType != null)
			{
				e.NewValue = CurrentBinding.Current.ConnectorType + BCObjectsConstants.TaxClasses;
			}
		}
		public virtual void _(Events.RowSelected<BCBinding> e)
		{
			BCBinding row = e.Row as BCBinding;
			if (row == null) return;

			PXUIFieldAttribute.SetEnabled<BCBinding.isDefault>(e.Cache, e.Row, row.IsActive == true);
		}
		public virtual void _(Events.RowSelected<BCBindingExt> e)
		{
			BCBindingExt row = e.Row as BCBindingExt;
			if (row == null) return;

			Locations.AllowSelect = Locations.AllowInsert = Locations.AllowDelete = Locations.AllowUpdate = row.WarehouseMode == BCWarehouseModeAttribute.SpecificWarehouse;

			foreach (BCEntity entity in Entities.Select())
			{
				foreach (string fieldName in e.Cache.BqlFields.Select(i => i.Name))
				{
					foreach (PXEventSubscriberAttribute attr in e.Cache.GetAttributes(row, fieldName))
					{
						if (attr is BCSettingsCheckerAttribute && entity.IsActive == true && ((BCSettingsCheckerAttribute)attr).EntityApplied(entity.EntityType))
							((BCSettingsCheckerAttribute)attr).SetMandatory();
					}
				}
				MultiCurrency.Cache.AllowSelect = PXAccess.FeatureInstalled<PX.Objects.CS.FeaturesSet.multicurrency>();
			}
			foreach (var field in e.Cache.BqlFields)
			{
				foreach (PXEventSubscriberAttribute attr in e.Cache.GetAttributes(row, field.Name))
				{
					if (attr is BCSettingsCheckerAttribute)
					{
						PXDefaultAttribute.SetPersistingCheck(e.Cache, field.Name, row, ((BCSettingsCheckerAttribute)attr).FieldRequired() ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);
					}
					else if (attr is SalesCategoriesAttribute)
					{
						PXEventSubscriberAttribute settingChecker = e.Cache.GetAttributes(row, field.Name).FirstOrDefault(i => i is BCSettingsCheckerAttribute);
						if (settingChecker != null)
						{
							PXDefaultAttribute.SetPersistingCheck(e.Cache, field.Name, row, ((BCSettingsCheckerAttribute)settingChecker).FieldRequired() ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);
							SalesCategoriesAttribute.SetCheck(e.Cache, field.Name, row, ((BCSettingsCheckerAttribute)settingChecker).FieldRequired());
						}
					}
				}
			}

			if (HasSynchronizedRecord())
			{
				BCBindingExt original = e.Cache.GetOriginal(e.Row) as BCBindingExt;

				if (original != null)
				{
					if (row.Availability != original.Availability)
						PXUIFieldAttribute.SetWarning<BCBindingExt.availability>(e.Cache, e.Row, BCMessages.AvalabilityChangesWarning);
					else
						PXUIFieldAttribute.SetWarning<BCBindingExt.availability>(e.Cache, e.Row, null);
					if (row.AvailabilityCalcRule != original.AvailabilityCalcRule)
						PXUIFieldAttribute.SetWarning<BCBindingExt.availabilityCalcRule>(e.Cache, e.Row, BCMessages.AvalabilityChangesWarning);
					else
						PXUIFieldAttribute.SetWarning<BCBindingExt.availabilityCalcRule>(e.Cache, e.Row, null);
					if (row.NotAvailMode != original.NotAvailMode)
						PXUIFieldAttribute.SetWarning<BCBindingExt.notAvailMode>(e.Cache, e.Row, BCMessages.AvalabilityChangesWarning);
					else
						PXUIFieldAttribute.SetWarning<BCBindingExt.notAvailMode>(e.Cache, e.Row, null);
					if (IsWarehouseUpdated() || row.WarehouseMode != original.WarehouseMode)
						PXUIFieldAttribute.SetWarning<BCBindingExt.warehouseMode>(e.Cache, e.Row, BCMessages.AvalabilityChangesWarning);
					else
						PXUIFieldAttribute.SetWarning<BCBindingExt.warehouseMode>(e.Cache, e.Row, null);
				}
			}

		}

		public virtual void _(Events.RowUpdated<BCBinding> e)
		{
			CurrentStore.Update(CurrentStore?.Current ?? CurrentStore.Select() ?? new BCBindingExt());
		}


		public virtual void _(Events.RowInserted<BCBinding> e)
		{
			bool dirty = CurrentStore.Cache.IsDirty;
			CurrentStore.Insert();
			CurrentStore.Cache.IsDirty = dirty;
		}
		public virtual void _(Events.RowPersisting<BCBinding> e)
		{
			BCBinding row = e.Row as BCBinding;
			if (row == null) return;

			Boolean anyError = false;
			string errorMessage = null;
			foreach (BCEntity entity in Entities.Select())
			{
				if (entity.EntityType == BCEntitiesAttribute.Payment)
				{
					foreach (BCPaymentMethods item in PaymentMethods.Select())
					{
						if (item.Active == true)
						{
							if (item.CashAccountID == null || string.IsNullOrEmpty(item.PaymentMethodID))
							{
								anyError = true;
								errorMessage = PX.Commerce.Core.BCMessages.PaymentMethodRequired;

								PaymentMethods.Cache.RaiseExceptionHandling<BCPaymentMethods.cashAccountID>(item, null, new PXSetPropertyException(errorMessage, PXErrorLevel.RowError));
							}

						}
					}
					if (anyError) throw new PXSetPropertyException<BCPaymentMethods.cashAccountID>(errorMessage);
					break;
				}
			}
		}

		public virtual void _(Events.FieldUpdated<BCBinding, BCBinding.branchID> e)
		{
			var row = e.Row;
			if (row == null || e.OldValue == null) return;
			foreach (BCEntity entity in Entities.Select())
			{
				if (entity.EntityType == BCEntitiesAttribute.Payment)
				{
					bool found = false;
					foreach (BCPaymentMethods item in PaymentMethods.Select())
					{
						if (item.Active == true || !string.IsNullOrEmpty(item.PaymentMethodID) || item.CashAccountID != null)
						{
							found = true;
							break;
						}
					}
					if (found)
					{
						if (PaymentMethods.Ask(BCCaptions.Stores, BCMessages.BranchChangeConfirmation, MessageButtons.YesNo) == WebDialogResult.Yes)
						{
							foreach (BCPaymentMethods item in PaymentMethods.Select())
							{
								item.PaymentMethodID = null;
								item.CuryID = null;
								item.CashAccountID = null;
								item.ReleasePayments = false;
								item.Active = false;
								foreach (BCMultiCurrencyPaymentMethod item1 in MultiCurrency.Select())
								{
									MultiCurrency.Delete(item1);
								}
								PaymentMethods.Cache.Update(item);
							}
						}
						else
							row.BranchID = e.OldValue.ToInt();
					}

					break;
				}

			}
		}
		public virtual void _(Events.FieldUpdated<BCBindingExt, BCBindingExt.warehouseMode> e)
		{
			if (e.Row.WarehouseMode == BCWarehouseModeAttribute.AllWarehouse)
			{
				foreach (BCLocations loc in Locations.Select())
				{
					Locations.Delete(loc);
				}
			}
			Locations.View.RequestRefresh();
		}

		public virtual void _(Events.FieldUpdated<BCLocations, BCLocations.locationID> e)
		{
			checkWarehouseLocation();
		}
		public virtual void _(Events.RowPersisting<BCLocations> e)
		{
			checkWarehouseLocation();
		}

		public override void _(Events.FieldUpdated<BCEntity, BCEntity.isActive> e)
		{
			base._(e);

			BCEntity row = e.Row as BCEntity;
			if (row == null || e.NewValue == null || e.Row?.EntityType != BCEntitiesAttribute.Payment) return;

			if (((bool)e.NewValue) == true)
			{
				PaymentMethods.Select();
			}
		}
		
		#endregion

		#region PaymentMethods
		[PXCopyPasteHiddenView]
		public PXSelect<BCPaymentMethods, Where<BCPaymentMethods.bindingID, Equal<Current<BCBinding.bindingID>>>> PaymentMethods;
		[PXCopyPasteHiddenView]
		public PXSelect<BCBigCommercePayment> BigCommerceMethods;
		[PXCopyPasteHiddenView]
		public PXSelect<BCMultiCurrencyPaymentMethod, Where<BCMultiCurrencyPaymentMethod.bindingID, Equal<Current<BCPaymentMethods.bindingID>>,
			And<BCMultiCurrencyPaymentMethod.paymentMappingID, Equal<Current<BCPaymentMethods.paymentMappingID>>>
			>> MultiCurrency;

		public IEnumerable bigCommerceMethods()
		{
			BCBinding binding = Bindings.Current;
			if (binding == null || binding.BindingID == null || binding.BindingID <= 0) yield break;

			Boolean anyFound = false;
			foreach (BCBigCommercePayment payment in BigCommerceMethods.Cache.Cached)
			{
				anyFound = true;
				yield return payment;
			}

			if (!anyFound)
			{
				var paymentMethods = ConnectorHelper.GetConnector(binding.ConnectorType)?.GetExternalInfo<IPaymentMethod>(BCObjectsConstants.BCPayment, binding.BindingID);
				if (paymentMethods == null) yield break;

				Boolean lastDirty = BigCommerceMethods.Cache.IsDirty;
				
				foreach (var method in paymentMethods)
				{
					if (!string.IsNullOrEmpty(method.Name))
					{
						BCBigCommercePayment inserted = BigCommerceMethods.Insert(new BCBigCommercePayment() { CreatePaymentfromOrder = method.CreatePaymentfromOrder, Name = method.Name });
						BigCommerceMethods.Cache.SetStatus(inserted, PXEntryStatus.Held);
						yield return inserted;
					}
				}
				BigCommerceMethods.Cache.IsDirty = lastDirty;
			}
		}

		public IEnumerable paymentMethods()
		{
			BCBinding binding = Bindings.Current;
			if (binding == null || binding.BindingID == null || binding.BindingID <= 0) yield break;
			foreach (BCEntity entity in Entities.Select())
			{
				if (entity.EntityType == BCEntitiesAttribute.Payment)
				{
					if (entity.IsActive != true) yield break;
					else break;
				}
			}
			bool lastDirty = this.PaymentMethods.Cache.IsDirty;
			PXView view = new PXView(this, false, PaymentMethods.View.BqlSelect);
			List<BCPaymentMethods> stored = view.SelectMulti().Select(r => (BCPaymentMethods)r).ToList();
			foreach (BCBigCommercePayment paymentMethod in BigCommerceMethods.Select())
			{
				var matchedResult = stored.FirstOrDefault(x => x.StorePaymentMethod != null && x.StorePaymentMethod.ToUpper().Equals(paymentMethod.Name.ToUpper()));
				if (matchedResult == null)
				{
					BCPaymentMethods entry = new BCPaymentMethods()
					{
						CreatePaymentFromOrder = paymentMethod.CreatePaymentfromOrder,
						StorePaymentMethod = paymentMethod.Name,
						BindingID = binding.BindingID
					};
					entry = PaymentMethods.Insert(entry);

					if (entry != null) yield return entry;
				}
			}
			foreach (BCPaymentMethods result in stored)
			{
				yield return result;
			}
			this.PaymentMethods.Cache.IsDirty = lastDirty;
		}

		public virtual void _(Events.FieldUpdated<BCPaymentMethods, BCPaymentMethods.paymentMethodID> e)
		{
			var row = e.Row;
			if (row == null || e.NewValue == null) return;
			foreach (BCMultiCurrencyPaymentMethod item in MultiCurrency.Select())
			{
				MultiCurrency.Delete(item);
			}
			row.CuryID = null;
			row.CashAccountID = null;
			row.ProcessingCenterID = null;
		}
		public virtual void _(Events.RowSelected<BCPaymentMethods> e)
		{
			if (e.Row == null) return;

			PXUIFieldAttribute.SetEnabled<BCPaymentMethods.releasePayments>(e.Cache, e.Row, e.Row?.ProcessingCenterID == null);
		}
		public virtual void _(Events.FieldUpdated<BCPaymentMethods, BCPaymentMethods.processingCenterID> e)
		{
			e.Row.ReleasePayments = false;
		}
		public virtual void BCMultiCurrencyPaymentMethod_RowInserting(PXCache sender, PXRowInsertingEventArgs e)
		{
			var row = (BCMultiCurrencyPaymentMethod)e.Row;
			if (row == null) return;

			foreach (BCMultiCurrencyPaymentMethod item in MultiCurrency.Select())
			{
				if (row.CuryID?.ToString().Trim() == item?.CuryID?.ToString().Trim())
				{
					sender.RaiseExceptionHandling<BCMultiCurrencyPaymentMethod.curyID>(row, null, new PXSetPropertyException(PX.Commerce.Core.BCMessages.DuplicateCurrency, PXErrorLevel.RowError));
					e.Cancel = true;
				}
			}
		}
		#endregion

		#region Shipping Mapping
		[PXCopyPasteHiddenView]
		public PXSelect<BCShippingMappings, Where<BCShippingMappings.bindingID, Equal<Current<BCBinding.bindingID>>>, OrderBy<Asc<BCShippingMappings.shippingZone, Asc<BCShippingMappings.shippingZone>>>> ShippingMappings;
		[PXCopyPasteHiddenView]
		public PXSelect<BCShippingZones, Where<BCShippingMappings.bindingID, Equal<Current<BCBinding.bindingID>>>> ShippingZones;

		public IEnumerable shippingZones()
		{
			BCBinding binding = Bindings.Current;
			if (binding == null || binding.BindingID == null || binding.BindingID <= 0)
				yield break;
			Boolean anyFound = false;
			foreach (BCShippingZones zone in ShippingZones.Cache.Cached)
			{
				anyFound = true;
				yield return zone;
			}

			Boolean lastDirty = ShippingZones.Cache.IsDirty;
			if (!anyFound)
			{
				IEnumerable<IShippingZone> zones = ConnectorHelper.GetConnector(binding.ConnectorType)?.GetExternalInfo<IShippingZone>(BCObjectsConstants.BCShippingZone, binding.BindingID);
				if (zones != null && zones.Count() > 0)
				{
					foreach (var zone in zones)
					{
						if (zone.ShippingMethods != null && zone.ShippingMethods.Count > 0 && zone.Enabled == true)
						{
							foreach (var method in zone.ShippingMethods)
							{
								if (method?.Enabled == true)
									yield return AddShippingZoneItem(binding.BindingID, zone.Name, method.Name);
							}
						}
					}
				}
			}
			ShippingZones.Cache.IsDirty = lastDirty;
			yield break;
		}

		public IEnumerable shippingMappings()
		{
			PXView pxView = new PXView(this, false, ShippingMappings.View.BqlSelect);
			List<BCShippingMappings> shippingMappingList = pxView.SelectMulti().Select(r => { var item = (BCShippingMappings)r; return item; }).ToList();

			foreach (BCShippingZones zone in ShippingZones.Select())
			{
				AddShippingMappingItem(shippingMappingList, zone);
			}

			return shippingMappingList ?? new List<BCShippingMappings>();

		}

		protected virtual BCShippingZones AddShippingZoneItem(int? bindingID, string shippingZone, string shippingMethod)
		{
			bool lastDirty = ShippingZones.Cache.IsDirty;

			BCShippingZones row = new BCShippingZones()
			{
				BindingID = bindingID,
				ShippingZone = shippingZone,
				ShippingMethod = shippingMethod
			};
			row = ShippingZones.Insert(row);

			ShippingZones.Cache.SetStatus(row, PXEntryStatus.Held);
			ShippingZones.Cache.IsDirty = lastDirty;

			return row;
		}
		protected virtual void AddShippingMappingItem(List<BCShippingMappings> shippingMappingList, BCShippingZones zone)
		{
			bool lastDirty = ShippingMappings.Cache.IsDirty;
			var existedItem = shippingMappingList.FirstOrDefault(x => x.ShippingZone?.ToUpper()==zone.ShippingZone?.ToUpper() && x.ShippingMethod.Equals(zone.ShippingMethod, StringComparison.OrdinalIgnoreCase));
			if (existedItem == null)
			{
				BCShippingMappings row = new BCShippingMappings()
				{
					BindingID = zone.BindingID,
					ShippingZone = zone.ShippingZone,
					ShippingMethod = zone.ShippingMethod,
				};
				row = ShippingMappings.Insert(row);
				shippingMappingList.Add(row);
			}
			else
			{
				existedItem.ShippingZone = zone.ShippingZone;
				existedItem.ShippingMethod = zone.ShippingMethod;
				ShippingMappings.Cache.Update(existedItem);
			}
			ShippingMappings.Cache.IsDirty = lastDirty;
		}

		#endregion

		#region Persist

		public override void Persist()
		{
			//Skip push notifications to remove extra extra messages pushed
			using (new SuppressPushNotificationsScope())
			{

				if (HasSynchronizedRecord())
				{
					BCBinding binding = Bindings.Current;
					BCBindingExt store = CurrentStore.Current;
					BCBindingExt original = CurrentStore.Cache.GetOriginal(store) as BCBindingExt;

					bool resync = false;
					if (IsFieldUpdated(CurrentStore.Cache, store, original, nameof(BCBindingExt.availability))
						|| IsFieldUpdated(CurrentStore.Cache, store, original, nameof(BCBindingExt.availabilityCalcRule))
						|| IsFieldUpdated(CurrentStore.Cache, store, original, nameof(BCBindingExt.notAvailMode))
						|| IsFieldUpdated(CurrentStore.Cache, store, original, nameof(BCBindingExt.warehouseMode))
						|| IsWarehouseUpdated())
						resync = true;

					base.Persist();

					if (resync)
					{
						PXLongOperation.StartOperation(this, delegate ()
						{
							BCEntityMaint.DoResetSync(binding.ConnectorType, binding.BindingID, BCEntitiesAttribute.ProductAvailability);
						});
					}
				}
				else
					base.Persist();
			}
		}
		#endregion

		#region Check Modifications
		protected bool IsFieldUpdated(PXCache cache, object row, object original, string field)
		{
			var oldValue = cache.GetValue(original, field);
			var newValue = cache.GetValue(row, field);
			if (oldValue != null && newValue != null)
				return oldValue.ToString() != newValue.ToString();
			return false;
		}

		protected bool IsWarehouseUpdated()
		{
			var originalLocation = Locations.Cache.GetOriginal(Locations.Current);
			if (originalLocation != null)
			{
				BCLocations newRow = (BCLocations)Locations.Current;
				BCLocations oldRow = (BCLocations)originalLocation;
				return oldRow.LocationID != newRow.LocationID || oldRow.SiteID != newRow.SiteID;

			}
			return Locations.Cache.IsInsertedUpdatedDeleted;
		}

		protected void checkWarehouseLocation()
		{
			foreach (var item in Locations.Select().Select(x => x.GetItem<BCLocations>()).ToLookup(x => x.SiteID))
			{
				if (item.Count() > 1 && (item.Any(i => i.LocationID == null) || item.GroupBy(g => g.LocationID).Where(x => x.Count() > 1).Any()))
					throw new PXException(BCObjectsMessages.DuplicateLocationRows);
			}
		}

		protected bool HasSynchronizedRecord()
		{
			BCSyncStatus status = PXSelect<BCSyncStatus,
											Where<BCSyncStatus.connectorType, Equal<Current<BCEntity.connectorType>>,
												And<BCSyncStatus.bindingID, Equal<Current<BCEntity.bindingID>>,
												And<BCSyncStatus.pendingSync, Equal<False>,
												And<BCSyncStatus.entityType, Equal<Required<BCEntity.entityType>>>>>>>.Select(this, BCEntitiesAttribute.ProductAvailability);
			return status != null;
		}
		#endregion
	}
}