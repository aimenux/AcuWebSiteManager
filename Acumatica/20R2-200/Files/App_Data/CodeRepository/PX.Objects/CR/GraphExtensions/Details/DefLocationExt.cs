using System;
using System.Collections;
using PX.Common;
using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Data.BQL;
using PX.Data.EP;
using PX.Objects.AP;
using PX.Objects.CS;
using PX.Objects.CT;
using PX.Objects.SO;
using PX.SM;
using PX.Objects.CR.DAC;
using PX.Objects.CA;
using PX.Objects.CR.MassProcess;
using PX.Data.MassProcess;
using PX.Objects.CR.Extensions;
using PX.Objects.CR.Extensions.CRDuplicateEntities;
using PX.Objects.CR.Extensions.CRCreateActions;
using System.Linq;
using System.Collections.Generic;
using PX.Objects.CR.Extensions.Relational;
using PX.CS.Contracts.Interfaces;
using PX.Objects.GDPR;
using CRLocation = PX.Objects.CR.Standalone.Location;
using PX.Objects.TX;
using PX.Objects.GL;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.IN;

namespace PX.Objects.CR.Extensions
{
	/// <summary>
	/// Extension that is used for selecting and defaulting the Default Address and Default Contact of the Business Account and it's inheritors.
	/// No Inserting of Contact and Address is implemented, as the Inserting is handled inside the <see cref="SharedChildOverrideGraphExt"/> graph extension.
	/// </summary>
	public abstract class DefLocationExt<TGraph, TDefContactAddress, TLocationDetails, TMaster, TBAccountID, TDefLocationID> : PXGraphExtension<TDefContactAddress, TLocationDetails, TGraph>
		where TGraph : PXGraph
		where TDefContactAddress : PXGraphExtension<TGraph>    // DefContactAddress graph ext
		where TLocationDetails : LocationDetailsExt<TGraph, TDefContactAddress, TMaster, TBAccountID>
		where TMaster : BAccount, IBqlTable, new()
		where TBAccountID : class, IBqlField
		where TDefLocationID : class, IBqlField
	{
		#region State

		protected LocationValidator locationValidator;

		public virtual List<Type> InitLocationFields => new List<Type>();

		#endregion

		#region Ctor

		public override void Initialize()
		{
			base.Initialize();

			locationValidator = new LocationValidator();
		}

		#endregion

		#region Views
		[PXHidden]
		public PXSelect<Location>
			BaseLocationDummy;

		[PXViewName(Messages.DeliverySettings)]
		public PXSelect<
				CRLocation,
			Where<
				CRLocation.bAccountID, Equal<Current<TBAccountID>>,
				And<CRLocation.locationID, Equal<Current<TDefLocationID>>>>>
			DefLocation;

		[PXViewName(Messages.DeliveryContact)]
		public PXSelect<
				Contact,
			Where<
				Contact.bAccountID, Equal<Current<CRLocation.bAccountID>>,
				And<Contact.contactID, Equal<Current<CRLocation.defContactID>>>>>
			DefLocationContact;

		[Api.Export.PXOptimizationBehavior(IgnoreBqlDelegate = true)]
		public virtual IEnumerable defLocationContact()
		{
			return SelectEntityByKey<Contact, Contact.contactID, CRLocation.defContactID, CRLocation.overrideContact>();
		}

		[PXViewName(Messages.DeliveryAddress)]
		public PXSelect<
				Address,
			Where<
				Address.bAccountID, Equal<Current<CRLocation.bAccountID>>,
				And<Address.addressID, Equal<Current<CRLocation.defAddressID>>>>>
			DefLocationAddress;

		[Api.Export.PXOptimizationBehavior(IgnoreBqlDelegate = true)]
		public virtual IEnumerable defLocationAddress()
		{
			return SelectEntityByKey<Address, Address.addressID, CRLocation.defAddressID, CRLocation.overrideAddress>();
		}

		#endregion

		#region Actions

		public PXAction<TMaster> ViewDefLocationAddressOnMap;
		[PXUIField(DisplayName = Messages.ViewOnMap, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton()]
		public virtual IEnumerable viewDefLocationAddressOnMap(PXAdapter adapter)
		{
			Address addr = (Address)this.DefLocationAddress.SelectSingle();
			BAccountUtility.ViewOnMap(addr);
			return adapter.Get();
		}

		public PXAction<TMaster> SetDefaultLocation;
		[PXUIField(DisplayName = Messages.SetDefault)]
		[PXButton]
		public virtual void setDefaultLocation()
		{
			var account = Base.Caches[typeof(TMaster)].Current as TMaster;
			if (account == null || account.BAccountID == null) return;

			var row = Base1.Locations.Current;
			if (row == null || row.LocationID == null) return;

			if (row.IsActive != true)
				throw new Exception(Messages.DefaultLocationCanNotBeNotActive);

			var currentDefault =
				(CRLocation)PXSelect<
						CRLocation,
					Where<
						CRLocation.bAccountID, Equal<Required<BAccount.bAccountID>>,
						And<CRLocation.locationID, Equal<Required<BAccount.defLocationID>>>>>
					.SelectSingleBound(Base, null, account.BAccountID, account.DefLocationID);

			if (currentDefault != null && DefLocation.Cache.GetStatus(currentDefault) == PXEntryStatus.Inserted)
				DefLocation.Cache.Delete(currentDefault);

			if (account.DefLocationID != row.LocationID)
				SetLocationAsDefault(row);

			account.DefLocationID = row.LocationID;

			Base.Caches[typeof(TMaster)].Update(account);
		}

		[PXOverride]
		public virtual bool DoValidateAddresses(ValidateAddressesDelegate baseDel)
		{
			bool res = baseDel();

			Address locAddress = this.DefLocationAddress.SelectSingle();
			if (locAddress != null && locAddress.IsValidated == false)
			{
				if (PXAddressValidator.Validate<Address>(Base, locAddress, true))
				{
					return res;
				}
			}

			return res;
		}

		#endregion

		#region Events

		#region CacheAttached

		[PXDBInt(IsKey = true)]
		[PXDBDefault(typeof(BAccount.bAccountID))]
		[PXMergeAttributes(Method = MergeMethod.Replace)]
		protected virtual void _(Events.CacheAttached<CRLocation.bAccountID> e) { }

		[PXDBChildIdentity(typeof(Contact.contactID))]
		[PXMergeAttributes(Method = MergeMethod.Append)]
		protected virtual void _(Events.CacheAttached<CRLocation.defContactID> e) { }

		[PXDBChildIdentity(typeof(Address.addressID))]
		[PXMergeAttributes(Method = MergeMethod.Append)]
		protected virtual void _(Events.CacheAttached<CRLocation.defAddressID> e) { }

		[PXDBInt]
		[PXMergeAttributes(Method = MergeMethod.Replace)]
		protected virtual void _(Events.CacheAttached<CRLocation.cARAccountLocationID> e) { }

		[PXDBInt]
		[PXMergeAttributes(Method = MergeMethod.Replace)]
		protected virtual void _(Events.CacheAttached<CRLocation.vAPAccountLocationID> e) { }

		[PXDBInt]
		[PXMergeAttributes(Method = MergeMethod.Replace)]
		protected virtual void _(Events.CacheAttached<CRLocation.vPaymentInfoLocationID> e) { }

		[PXShort]
		[PXMergeAttributes(Method = MergeMethod.Replace)]
		protected virtual void _(Events.CacheAttached<CRLocation.vSiteIDIsNull> e) { }

		[PXFormula(typeof(Switch<Case<Where<CRLocation.defAddressID, Equal<CurrentValue<BAccount.defAddressID>>>, False>, True>))]
		[PXMergeAttributes(Method = MergeMethod.Append)]
		protected virtual void _(Events.CacheAttached<CRLocation.overrideAddress> e) { }

		[PXFormula(typeof(Switch<Case<Where<CRLocation.defContactID, Equal<CurrentValue<BAccount.defContactID>>>, False>, True>))]
		[PXMergeAttributes(Method = MergeMethod.Append)]
		protected virtual void _(Events.CacheAttached<CRLocation.overrideContact> e) { }

		[PXFormula(typeof(Switch<Case<Where<CRLocation.defAddressID, Equal<CurrentValue<BAccount.defAddressID>>>, True>, False>))]
		[PXMergeAttributes(Method = MergeMethod.Append)]
		protected virtual void _(Events.CacheAttached<CRLocation.isAddressSameAsMain> e) { }

		[PXFormula(typeof(Switch<Case<Where<CRLocation.defContactID, Equal<CurrentValue<BAccount.defContactID>>>, True>, False>))]
		[PXMergeAttributes(Method = MergeMethod.Append)]
		protected virtual void _(Events.CacheAttached<CRLocation.isContactSameAsMain> e) { }

		[PXDBInt]
		[PXDBChildIdentity(typeof(CRLocation.locationID))]
		[PXMergeAttributes(Method = MergeMethod.Replace)]   // remove PXSelector from DAC
		protected virtual void _(Events.CacheAttached<TDefLocationID> e) { }

		#endregion

		#region Field-level

		protected virtual void _(Events.FieldDefaulting<CRLocation, CRLocation.vBranchID> e)
		{
			e.NewValue = null;
			e.Cancel = true;
		}

		protected virtual void _(Events.FieldDefaulting<CRLocation, CRLocation.vAPAccountLocationID> e) =>
			DefaultFromSame<CRLocation.locationID>(e.Args, e.Cache);

		protected virtual void _(Events.FieldDefaulting<CRLocation, CRLocation.cARAccountLocationID> e) =>
			DefaultFromSame<CRLocation.locationID>(e.Args, e.Cache);

		protected virtual void _(Events.FieldDefaulting<CRLocation, CRLocation.vPaymentInfoLocationID> e) =>
			DefaultFromSame<CRLocation.locationID>(e.Args, e.Cache);

		#endregion

		#region Row-level

		protected virtual void _(Events.RowSelected<CRLocation> e)
		{
			var row = e.Row as CRLocation;
			if (row == null) return;

			// Acuminator disable once PX1047 RowChangesInEventHandlersForbiddenForArgs [non-DB field, just for UI]
			row.IsDefault = false;

			var acct = Base.Caches[typeof(TMaster)].Current as TMaster;
			if (acct == null) return;

			if (row.LocationID == acct.DefLocationID)
			{
				// Acuminator disable once PX1047 RowChangesInEventHandlersForbiddenForArgs [non-DB field, just for UI]
				row.IsDefault = true;
			}
		}

		object _KeyToAbort = null;

		protected virtual void _(Events.RowPersisted<CRLocation> e)
		{
			if (e.Operation == PXDBOperation.Insert)
			{
				if (e.TranStatus == PXTranStatus.Open)
				{
					if ((int?)e.Cache.GetValue<CRLocation.vAPAccountLocationID>(e.Row) == null || (int?)e.Cache.GetValue<CRLocation.vAPAccountLocationID>(e.Row) < 0)
					{
						_KeyToAbort = e.Cache.GetValue<CRLocation.locationID>(e.Row);

						// Acuminator disable once PX1043 SavingChangesInEventHandlers [legacy]
						PXDatabase.Update<Location>(
							new PXDataFieldAssign(nameof(CRLocation.VAPAccountLocationID), _KeyToAbort),
							new PXDataFieldRestrict(nameof(CRLocation.LocationID), _KeyToAbort),
							PXDataFieldRestrict.OperationSwitchAllowed);

						e.Cache.SetValue<CRLocation.vAPAccountLocationID>(e.Row, _KeyToAbort);
					}

					if ((int?)e.Cache.GetValue<CRLocation.vPaymentInfoLocationID>(e.Row) == null || (int?)e.Cache.GetValue<CRLocation.vPaymentInfoLocationID>(e.Row) < 0)
					{
						_KeyToAbort = e.Cache.GetValue<CRLocation.locationID>(e.Row);

						// Acuminator disable once PX1043 SavingChangesInEventHandlers [legacy]
						PXDatabase.Update<CRLocation>(
							new PXDataFieldAssign(nameof(CRLocation.VPaymentInfoLocationID), _KeyToAbort),
							new PXDataFieldRestrict(nameof(CRLocation.LocationID), _KeyToAbort),
							PXDataFieldRestrict.OperationSwitchAllowed);

						e.Cache.SetValue<CRLocation.vPaymentInfoLocationID>(e.Row, _KeyToAbort);
					}

					if ((int?)e.Cache.GetValue<CRLocation.cARAccountLocationID>(e.Row) == null || (int?)e.Cache.GetValue<CRLocation.cARAccountLocationID>(e.Row) < 0)
					{
						_KeyToAbort = e.Cache.GetValue<CRLocation.locationID>(e.Row);

						// Acuminator disable once PX1043 SavingChangesInEventHandlers [legacy]
						PXDatabase.Update<CRLocation>(
							new PXDataFieldAssign(nameof(CRLocation.CARAccountLocationID), _KeyToAbort),
							new PXDataFieldRestrict(nameof(CRLocation.LocationID), _KeyToAbort),
							PXDataFieldRestrict.OperationSwitchAllowed);

						e.Cache.SetValue<CRLocation.cARAccountLocationID>(e.Row, _KeyToAbort);
					}
				}
				else
				{
					if (e.TranStatus == PXTranStatus.Aborted)
					{
						if (object.Equals(_KeyToAbort, e.Cache.GetValue<CRLocation.vAPAccountLocationID>(e.Row)))
						{
							e.Cache.SetValue<CRLocation.vAPAccountLocationID>(e.Row, null);
						}

						if (object.Equals(_KeyToAbort, e.Cache.GetValue<CRLocation.vPaymentInfoLocationID>(e.Row)))
						{
							e.Cache.SetValue<CRLocation.vPaymentInfoLocationID>(e.Row, null);
						}

						if (object.Equals(_KeyToAbort, e.Cache.GetValue<CRLocation.cARAccountLocationID>(e.Row)))
						{
							e.Cache.SetValue<CRLocation.cARAccountLocationID>(e.Row, null);
						}
					}
					_KeyToAbort = null;
				}
			}
		}
		
		protected virtual void _(Events.RowUpdated<TMaster> e)
		{
			if (e.Cache.ObjectsEqual<BAccount.acctCD>(e.Row, e.OldRow) || (e.OldRow as TMaster).AcctCD != null)
				return;

			InsertLocation(e.Cache, e.Row);
		}

		protected virtual void _(Events.RowInserted<TMaster> e, PXRowInserted del)
		{
			var row = (TMaster)e.Row;
			if (row == null)
				return;

			InsertLocation(e.Cache, row);

			del?.Invoke(e.Cache, e.Args);
		}

		#endregion

		#endregion

		#region Methods

		[PXOverride]
		public virtual void Persist(Action del)
		{
			var master = Base.Caches[typeof(TMaster)].Current as TMaster;
			if (master != null && Base.Caches[typeof(TMaster)].GetStatus(master) == PXEntryStatus.Updated)
			{
				bool errorsExist = false;

				foreach (CRLocation location in Base1.Locations.Select())
				{
					PXEntryStatus locationStatus = DefLocation.Cache.GetStatus(location);

					if (locationStatus != PXEntryStatus.Updated && locationStatus != PXEntryStatus.Inserted)
					{
						errorsExist |= !ValidateLocation(DefLocation.Cache, location);
					}
				}

				if (errorsExist)
				{
					throw new PXException(Common.Messages.RecordCanNotBeSaved);
				}
			}


			del();
		}

		public virtual void InsertLocation(PXCache cache, TMaster row)
		{
			// Inserting delivery locaiton record
			if (row.DefLocationID == null)
			{
				var locationOldDirty = DefLocation.Cache.IsDirty;
				var location = (CRLocation)DefLocation.Cache.CreateInstance();
				location.BAccountID = row.BAccountID;
				// CRLocation CD need to be formatted accorfing to segmented key mask prior inserting
				object cd = PXMessages.LocalizeNoPrefix(Messages.DefaultLocationCD);
				DefLocation.Cache.RaiseFieldUpdating<CRLocation.locationCD>(location, ref cd);
				location.LocationCD = (string)cd;

				location.LocType = LocTypeList.CompanyLoc;
				switch (row.Type)
				{
					case BAccountType.VendorType:
						location.LocType = LocTypeList.VendorLoc;
						break;
					case BAccountType.CustomerType:
						location.LocType = LocTypeList.CustomerLoc;
						break;
					case BAccountType.CombinedType:
						location.LocType = LocTypeList.CombinedLoc;
						break;
					case BAccountType.EmpCombinedType:
						location.LocType = LocTypeList.CustomerLoc;
						break;
					default:
						location.LocType = LocTypeList.CompanyLoc;
						break;
				}
				location.Descr = PXMessages.LocalizeNoPrefix(Messages.DefaultLocationDescription);
				location.IsDefault = true;
				location.DefAddressID = row.DefAddressID;
				location.OverrideAddress = false;
				location.DefContactID = row.DefContactID;
				location.OverrideContact = false;
				location = (CRLocation)DefLocation.Cache.Insert(location);

				//location.VDefAddressID = location.DefAddressID;
				//location.VDefContactID = location.DefContactID;
				//location.VRemitAddressID = location.DefAddressID;
				//location.VRemitContactID = location.DefContactID;

				InitLocation(location, location.LocType, false);

				location = (CRLocation)DefLocation.Cache.Update(location);

				cache.SetValue<BAccount.defLocationID>(row, location.LocationID);
				DefLocation.Cache.IsDirty = locationOldDirty;
			}
		}

		public virtual void InitLocation(IBqlTable location, string aLocationType, bool onlySetDefault)
		{
			if (location == null)
				return;

			PXCache cache = Base.Caches[location.GetType()];

			foreach (var field in InitLocationFields)
			{
				if (onlySetDefault || cache.GetValue(location, field.Name) == null)
					cache.SetDefaultExt(location, field.Name);
			}

			cache.SetValue<CRLocation.locType>(location, aLocationType);
		}

		public virtual bool ValidateLocation(PXCache cache, CRLocation location)
		{
			return true;
		}

		public virtual void SetLocationAsDefault(CRLocation row)
		{
			int? defLocationID = row.LocationID;
			CRLocation current = null;

			foreach (CRLocation other in PXParentAttribute.SelectSiblings(DefLocation.Cache, row))
			{
				if (other.LocationID == row.LocationID)
					continue;

				if (!object.Equals(other.LocationID, other.VAPAccountLocationID))
				{
					current = this.DefLocation.SelectSingle();

					other.VAPAccountLocationID = defLocationID;
					if (object.Equals(other, row) && current != null)
					{
						other.VAPAccountID = current.VAPAccountID;
						other.VAPSubID = current.VAPSubID;
						other.VRetainageAcctID = current.VRetainageAcctID;
						other.VRetainageSubID = current.VRetainageSubID;
					}

					DefLocation.Cache.MarkUpdated(other);
					DefLocation.Cache.IsDirty = true;
				}

				if (!object.Equals(other.LocationID, other.VPaymentInfoLocationID))
				{
					current = this.DefLocation.SelectSingle();

					other.VPaymentInfoLocationID = defLocationID;
					if (object.Equals(other, row) && current != null)
					{
						other.VCashAccountID = current.VCashAccountID;
						other.VPaymentMethodID = current.VPaymentMethodID;
						other.VPaymentLeadTime = current.VPaymentLeadTime;
						other.VSeparateCheck = current.VSeparateCheck;
						other.VRemitAddressID = current.VRemitAddressID;
						other.VRemitContactID = current.VRemitContactID;
					}

					DefLocation.Cache.MarkUpdated(other);
					DefLocation.Cache.IsDirty = true;
				}

				if (!object.Equals(other.LocationID, other.CARAccountLocationID))
				{
					current = this.DefLocation.SelectSingle();

					other.CARAccountLocationID = defLocationID;
					if (object.Equals(other, row) && current != null)
					{
						other.CARAccountID = current.CARAccountID;
						other.CARSubID = current.CARSubID;
						other.CRetainageAcctID = current.CRetainageAcctID;
						other.CRetainageSubID = current.CRetainageSubID;
					}

					DefLocation.Cache.MarkUpdated(other);
					DefLocation.Cache.IsDirty = true;
				}
			}
		}

		public virtual void DefaultFrom<TSourceField>(PXFieldDefaultingEventArgs e, PXCache source, bool allowNull = true, object nullValue = null)
			where TSourceField : IBqlField
		{
			if (source.Current != null && (allowNull || source.GetValue<TSourceField>(source.Current) != null))
			{
				e.NewValue = source.GetValue<TSourceField>(source.Current) ?? nullValue;
				e.Cancel = true;
			}
		}

		public virtual void DefaultFromSame<TSourceField>(PXFieldDefaultingEventArgs e, PXCache source, bool allowNull = true, object nullValue = null)
			where TSourceField : IBqlField
		{
			if (e.Row != null && (allowNull || source.GetValue<TSourceField>(e.Row) != null))
			{
				e.NewValue = source.GetValue<TSourceField>(e.Row) ?? nullValue;
				e.Cancel = true;
			}
		}

		public virtual IEnumerable SelectEntityByKey<TEntity, TEntityKey, TValueField, TOverrideField>()
			where TEntity : class, IBqlTable, new()
			where TEntityKey : IBqlField
			where TValueField : IBqlField
			where TOverrideField : IBqlField
		{
			var location = this.DefLocation.Current;

			if (location == null)
			{
				location = this.DefLocation.Current = this.DefLocation.SelectSingle();

				this.DefLocation.Cache.RaiseRowSelected(location);
			}

			if (location == null)
				yield break;

			foreach (var entity in SelectEntityByKey<TEntity, TEntityKey, TValueField, TOverrideField>(location))
			{
				yield return entity;
			}
		}

		public virtual IEnumerable SelectEntityByKey<TEntity, TEntityKey, TValueField, TOverrideField>(object valueObject)
			where TEntity : class, IBqlTable, new()
			where TEntityKey : IBqlField
			where TValueField : IBqlField
			where TOverrideField : IBqlField
		{
			if (valueObject == null)
				yield break;

			var entity = new PXSelect<
						TEntity,
					Where<
						TEntityKey, Equal<Required<TValueField>>>>(Base)
				.SelectSingle(Base.Caches[typeof(TValueField).DeclaringType].GetValue<TValueField>(valueObject));

			bool isOverride = (Base.Caches[typeof(TOverrideField).DeclaringType].GetValue<TOverrideField>(valueObject) as bool?) == true;

			if (isOverride)
			{
				// it's a self-sufficient entity, no need to clone for proper Enabling
				PXUIFieldAttribute.SetEnabled(Base.Caches<TEntity>(), entity, true);
			}
			else
			{
				// need to copy, because if you set Entity "disabled" - main Entity may be disabled too (if they are the same)
				entity = PXCache<TEntity>.CreateCopy(entity);

				PXUIFieldAttribute.SetEnabled(Base.Caches<TEntity>(), entity, false);
			}

			yield return entity;
		}

		#endregion

		#region Inner Classes

		public abstract class WithUIExtension : DefLocationExt<TGraph, TDefContactAddress, TLocationDetails, TMaster, TBAccountID, TDefLocationID>
		{
			#region Events

			#region CacheAttached

			[PXDBDefault(typeof(BAccount.bAccountID))]
			[PXMergeAttributes(Method = MergeMethod.Append)]
			protected virtual void _(Events.CacheAttached<CRLocation.bAccountID> e) { }

			[CS.LocationRaw(typeof(Where<Location.bAccountID, Equal<Current<CRLocation.bAccountID>>>), typeof(CRLocation.locationCD), IsKey = true, Visibility = PXUIVisibility.SelectorVisible, DisplayName = "Location ID")]
			[PXDefault(PersistingCheck = PXPersistingCheck.NullOrBlank)]
			[PXMergeAttributes(Method = MergeMethod.Replace)]
			protected virtual void _(Events.CacheAttached<CRLocation.locationCD> e) { }



			// Customer Location Properties

			[PXSelector(typeof(Search<TaxZone.taxZoneID>), DescriptionField = typeof(TaxZone.descr), CacheGlobal = true)]
			[PXMergeAttributes(Method = MergeMethod.Append)]
			protected virtual void _(Events.CacheAttached<CRLocation.cTaxZoneID> e) { }

			[PXSelector(typeof(Search<Carrier.carrierID>),
				typeof(Carrier.carrierID), typeof(Carrier.description), typeof(Carrier.isExternal), typeof(Carrier.confirmationRequired),
				CacheGlobal = true,
				DescriptionField = typeof(Carrier.description))]
			[PXMergeAttributes(Method = MergeMethod.Append)]
			protected virtual void _(Events.CacheAttached<CRLocation.cCarrierID> e) { }

			[PXSelector(typeof(Search<ShipTerms.shipTermsID>), CacheGlobal = true, DescriptionField = typeof(ShipTerms.description))]
			[PXMergeAttributes(Method = MergeMethod.Append)]
			protected virtual void _(Events.CacheAttached<CRLocation.cShipTermsID> e) { }

			[PXSelector(typeof(ShippingZone.zoneID), CacheGlobal = true, DescriptionField = typeof(ShippingZone.description))]
			[PXMergeAttributes(Method = MergeMethod.Append)]
			protected virtual void _(Events.CacheAttached<CRLocation.cShipZoneID> e) { }

			[PXSelector(typeof(FOBPoint.fOBPointID), CacheGlobal = true, DescriptionField = typeof(FOBPoint.description))]
			[PXMergeAttributes(Method = MergeMethod.Append)]
			protected virtual void _(Events.CacheAttached<CRLocation.cFOBPointID> e) { }

			[Branch(useDefaulting: false, IsDetail = false, PersistingCheck = PXPersistingCheck.Nothing, DisplayName = "Default Branch", IsEnabledWhenOneBranchIsAccessible = true)]
			[PXMergeAttributes(Method = MergeMethod.Replace)]
			protected virtual void _(Events.CacheAttached<CRLocation.cBranchID> e) { }

			[Account(DisplayName = "Sales Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description), Required = true, AvoidControlAccounts = true)]
			[PXMergeAttributes(Method = MergeMethod.Replace)]
			protected virtual void _(Events.CacheAttached<CRLocation.cSalesAcctID> e) { }

			[SubAccount(typeof(CRLocation.cSalesAcctID), DisplayName = "Sales Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description), Required = true)]
			[PXMergeAttributes(Method = MergeMethod.Replace)]
			protected virtual void _(Events.CacheAttached<CRLocation.cSalesSubID> e) { }

			[PXSelector(typeof(AR.ARPriceClass.priceClassID))]
			[PXMergeAttributes(Method = MergeMethod.Append)]
			protected virtual void _(Events.CacheAttached<CRLocation.cPriceClassID> e) { }

			[PXDimensionSelector(SiteAttribute.DimensionName, typeof(INSite.siteID), typeof(INSite.siteCD), DescriptionField = typeof(INSite.descr))]
			[PXRestrictor(typeof(Where<INSite.active, Equal<True>>), IN.Messages.InactiveWarehouse, typeof(INSite.siteCD))]
			[PXRestrictor(typeof(Where<INSite.siteID, NotEqual<SiteAttribute.transitSiteID>>), IN.Messages.TransitSiteIsNotAvailable)]
			[PXMergeAttributes(Method = MergeMethod.Append)]
			protected virtual void _(Events.CacheAttached<CRLocation.cSiteID> e) { }

			[Account(DisplayName = "Discount Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description), Required = false, AvoidControlAccounts = true)]
			[PXMergeAttributes(Method = MergeMethod.Replace)]
			protected virtual void _(Events.CacheAttached<CRLocation.cDiscountAcctID> e) { }

			[SubAccount(typeof(CRLocation.cDiscountAcctID), DisplayName = "Discount Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description), Required = false)]
			[PXMergeAttributes(Method = MergeMethod.Replace)]
			protected virtual void _(Events.CacheAttached<CRLocation.cDiscountSubID> e) { }

			[Account(DisplayName = "Retainage Receivable Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description), Required = false, ControlAccountForModule = ControlAccountModule.AR)]
			[PXMergeAttributes(Method = MergeMethod.Replace)]
			protected virtual void _(Events.CacheAttached<CRLocation.cRetainageAcctID> e) { }

			[SubAccount(typeof(CRLocation.cRetainageAcctID), DisplayName = "Retainage Receivable Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description), Required = false)]
			[PXMergeAttributes(Method = MergeMethod.Replace)]
			protected virtual void _(Events.CacheAttached<CRLocation.cRetainageSubID> e) { }

			[Account(DisplayName = "Freight Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description), Required = false, AvoidControlAccounts = true)]
			[PXMergeAttributes(Method = MergeMethod.Replace)]
			protected virtual void _(Events.CacheAttached<CRLocation.cFreightAcctID> e) { }

			[SubAccount(typeof(CRLocation.cFreightAcctID), DisplayName = "Freight Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description), Required = false)]
			[PXMergeAttributes(Method = MergeMethod.Replace)]
			protected virtual void _(Events.CacheAttached<CRLocation.cFreightSubID> e) { }

			[PXSelector(typeof(Search<CSCalendar.calendarID>), DescriptionField = typeof(CSCalendar.description))]
			[PXMergeAttributes(Method = MergeMethod.Append)]
			protected virtual void _(Events.CacheAttached<CRLocation.cCalendarID> e) { }

			[PM.Project(typeof(Where<PM.PMProject.customerID, Equal<Current<CRLocation.bAccountID>>>), DisplayName = "Default Project")]
			[PXMergeAttributes(Method = MergeMethod.Replace)]
			protected virtual void _(Events.CacheAttached<CRLocation.cDefProjectID> e) { }

			[Account(null, typeof(Search<Account.accountID,
				Where2<
					Match<Current<AccessInfo.userName>>,
					And<Account.active, Equal<True>,
					And<Where<Current<GLSetup.ytdNetIncAccountID>, IsNull,
						Or<Account.accountID, NotEqual<Current<GLSetup.ytdNetIncAccountID>>>>>>>>), DisplayName = "AR Account", Required = true, ControlAccountForModule = ControlAccountModule.AR)]
			[PXMergeAttributes(Method = MergeMethod.Replace)]
			protected virtual void _(Events.CacheAttached<CRLocation.cARAccountID> e) { }

			[SubAccount(typeof(CRLocation.cARAccountID), DisplayName = "AR Sub.", DescriptionField = typeof(Sub.description), Required = true)]
			[PXMergeAttributes(Method = MergeMethod.Replace)]
			protected virtual void _(Events.CacheAttached<CRLocation.cARSubID> e) { }



			// Vendor Location Properties

			[PXSelector(typeof(Search<TaxZone.taxZoneID>), CacheGlobal = true)]
			[PXMergeAttributes(Method = MergeMethod.Append)]
			protected virtual void _(Events.CacheAttached<CRLocation.vTaxZoneID> e) { }

			[PXSelector(typeof(Search<Carrier.carrierID>),
				typeof(Carrier.carrierID), typeof(Carrier.description), typeof(Carrier.isExternal), typeof(Carrier.confirmationRequired),
				CacheGlobal = true,
				DescriptionField = typeof(Carrier.description))]
			[PXMergeAttributes(Method = MergeMethod.Append)]
			protected virtual void _(Events.CacheAttached<CRLocation.vCarrierID> e) { }

			[PXSelector(typeof(Search<ShipTerms.shipTermsID>), CacheGlobal = true, DescriptionField = typeof(ShipTerms.description))]
			[PXMergeAttributes(Method = MergeMethod.Append)]
			protected virtual void _(Events.CacheAttached<CRLocation.vShipTermsID> e) { }

			[PXSelector(typeof(FOBPoint.fOBPointID), CacheGlobal = true, DescriptionField = typeof(FOBPoint.description))]
			[PXMergeAttributes(Method = MergeMethod.Append)]
			protected virtual void _(Events.CacheAttached<CRLocation.vFOBPointID> e) { }

			[Branch(useDefaulting: false, IsDetail = false, PersistingCheck = PXPersistingCheck.Nothing, DisplayName = "Default Branch", IsEnabledWhenOneBranchIsAccessible = true)]
			[PXMergeAttributes(Method = MergeMethod.Replace)]
			protected virtual void _(Events.CacheAttached<CRLocation.vBranchID> e) { }

			[Account(DisplayName = "Expense Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description), AvoidControlAccounts = true)]
			[PXMergeAttributes(Method = MergeMethod.Replace)]
			protected virtual void _(Events.CacheAttached<CRLocation.vExpenseAcctID> e) { }

			[SubAccount(typeof(CRLocation.vExpenseAcctID), DisplayName = "Expense Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
			[PXMergeAttributes(Method = MergeMethod.Replace)]
			protected virtual void _(Events.CacheAttached<CRLocation.vExpenseSubID> e) { }

			[Account(DisplayName = "Retainage Payable Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description), Required = false, ControlAccountForModule = ControlAccountModule.AP)]
			[PXMergeAttributes(Method = MergeMethod.Replace)]
			protected virtual void _(Events.CacheAttached<CRLocation.vRetainageAcctID> e) { }

			[SubAccount(typeof(CRLocation.vRetainageAcctID), DisplayName = "Retainage Payable Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description), Required = false)]
			[PXMergeAttributes(Method = MergeMethod.Replace)]
			protected virtual void _(Events.CacheAttached<CRLocation.vRetainageSubID> e) { }

			[Account(DisplayName = "Freight Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description), Required = false)]
			[PXMergeAttributes(Method = MergeMethod.Replace)]
			protected virtual void _(Events.CacheAttached<CRLocation.vFreightAcctID> e) { }

			[SubAccount(typeof(CRLocation.vFreightAcctID), DisplayName = "Freight Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description), Required = false)]
			[PXMergeAttributes(Method = MergeMethod.Replace)]
			protected virtual void _(Events.CacheAttached<CRLocation.vFreightSubID> e) { }

			[Account(DisplayName = "Discount Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description), Required = false, AvoidControlAccounts = true)]
			[PXMergeAttributes(Method = MergeMethod.Replace)]
			protected virtual void _(Events.CacheAttached<CRLocation.vDiscountAcctID> e) { }

			[SubAccount(typeof(CRLocation.vDiscountAcctID), DisplayName = "Discount Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description), Required = false)]
			[PXMergeAttributes(Method = MergeMethod.Replace)]
			protected virtual void _(Events.CacheAttached<CRLocation.vDiscountSubID> e) { }

			[PXDimensionSelector(SiteAttribute.DimensionName, typeof(INSite.siteID), typeof(INSite.siteCD), DescriptionField = typeof(INSite.descr))]
			[PXRestrictor(typeof(Where<INSite.active, Equal<True>>), IN.Messages.InactiveWarehouse, typeof(INSite.siteCD))]
			[PXRestrictor(typeof(Where<INSite.siteID, NotEqual<SiteAttribute.transitSiteID>>), IN.Messages.TransitSiteIsNotAvailable)]
			[PXMergeAttributes(Method = MergeMethod.Append)]
			protected virtual void _(Events.CacheAttached<CRLocation.vSiteID> e) { }

			[PM.Project(DisplayName = "Default Project")]
			[PXMergeAttributes(Method = MergeMethod.Replace)]
			protected virtual void _(Events.CacheAttached<CRLocation.vDefProjectID> e) { }

			[Account(null, typeof(Search<Account.accountID,
				Where2<
					Match<Current<AccessInfo.userName>>,
					And<Account.active, Equal<True>,
					And<Where<Current<GLSetup.ytdNetIncAccountID>, IsNull,
						Or<Account.accountID, NotEqual<Current<GLSetup.ytdNetIncAccountID>>>>>>>>), DisplayName = "AP Account", Required = true, ControlAccountForModule = ControlAccountModule.AP)]
			[PXMergeAttributes(Method = MergeMethod.Replace)]
			protected virtual void _(Events.CacheAttached<CRLocation.vAPAccountID> e) { }

			[SubAccount(typeof(CRLocation.vAPAccountID), DisplayName = "AP Sub.", DescriptionField = typeof(Sub.description), Required = true)]
			[PXMergeAttributes(Method = MergeMethod.Replace)]
			protected virtual void _(Events.CacheAttached<CRLocation.vAPSubID> e) { }

			[PXSelector(typeof(Search<PaymentMethod.paymentMethodID,
					Where<PaymentMethod.useForAP, Equal<True>,
						And<PaymentMethod.isActive, Equal<True>>>>),
				DescriptionField = typeof(PaymentMethod.descr))]
			[PXMergeAttributes(Method = MergeMethod.Append)]
			protected virtual void _(Events.CacheAttached<CRLocation.vPaymentMethodID> e) { }

			[CashAccount(typeof(Location.vBranchID), typeof(Search2<CashAccount.cashAccountID,
					InnerJoin<PaymentMethodAccount,
						On<PaymentMethodAccount.cashAccountID, Equal<CashAccount.cashAccountID>>>,
					Where2<
						Match<Current<AccessInfo.userName>>,
						And<CashAccount.clearingAccount, Equal<False>,
						And<PaymentMethodAccount.paymentMethodID, Equal<Current<Location.vPaymentMethodID>>,
						And<PaymentMethodAccount.useForAP, Equal<True>>>>>>),
				Visibility = PXUIVisibility.Visible)]
			[PXMergeAttributes(Method = MergeMethod.Replace)]
			protected virtual void _(Events.CacheAttached<CRLocation.vCashAccountID> e) { }



			//Company Location Properties

			[SubAccount(DisplayName = "Sales Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
			[PXMergeAttributes(Method = MergeMethod.Replace)]
			protected virtual void _(Events.CacheAttached<CRLocation.cMPSalesSubID> e) { }

			[SubAccount(DisplayName = "Expense Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
			[PXMergeAttributes(Method = MergeMethod.Replace)]
			protected virtual void _(Events.CacheAttached<CRLocation.cMPExpenseSubID> e) { }

			[SubAccount(DisplayName = "Freight Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
			[PXMergeAttributes(Method = MergeMethod.Replace)]
			protected virtual void _(Events.CacheAttached<CRLocation.cMPFreightSubID> e) { }

			[SubAccount(DisplayName = "Discount Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
			[PXMergeAttributes(Method = MergeMethod.Replace)]
			protected virtual void _(Events.CacheAttached<CRLocation.cMPDiscountSubID> e) { }

			[SubAccount(DisplayName = "Currency Gain/Loss Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
			[PXMergeAttributes(Method = MergeMethod.Replace)]
			protected virtual void _(Events.CacheAttached<CRLocation.cMPGainLossSubID> e) { }

			[PXDimensionSelector(SiteAttribute.DimensionName, typeof(INSite.siteID), typeof(INSite.siteCD), DescriptionField = typeof(INSite.descr))]
			[PXRestrictor(typeof(Where<INSite.active, Equal<True>>), IN.Messages.InactiveWarehouse, typeof(INSite.siteCD))]
			[PXMergeAttributes(Method = MergeMethod.Append)]
			protected virtual void _(Events.CacheAttached<CRLocation.cMPSiteID> e) { }

			#endregion

			#endregion
		}

		public abstract class WithCombinedTypeValidation : WithUIExtension
		{
			#region Events

			protected virtual void _(Events.RowDeleting<TMaster> e)
			{
				TMaster row = e.Row;
				if (row != null && (row.Type == BAccountType.CombinedType || row.IsBranch == true))
				{
					PXParentAttribute.SetLeaveChildren<Location.bAccountID>(this.BaseLocationDummy.Cache, null, true);
				}
			}

			#endregion
		}

		#endregion
	}
}
