using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PX.Common;
using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.PM;
using Branch = PX.Objects.GL.Branch;

namespace PX.Objects.IN
{
	public class INSiteMaint : PXGraph<INSiteMaint, INSite>
	{
		#region Views
		public PXSelect<BAccount> _bAccount;
		public PXSelect<INSite,
			Where<INSite.siteID, IsNotNull,
			And<INSite.siteID, NotEqual<SiteAttribute.transitSiteID>,
			And<Match<Current<AccessInfo.userName>>>>>> site; //TODO: it is workaround AC-56410
		public PXSelect<INSite, Where<INSite.siteID, Equal<Current<INSite.siteID>>>> siteaccounts;

		[PXFilterable]
		[PXImport(typeof(INSite))]
		public PXSelect<INLocation, Where<INLocation.siteID, Equal<Current<INSite.siteID>>>> location;
		[PXFilterable]
		[PXImport(typeof(INSite))]
		public PXSelect<INCart, Where<INCart.siteID, Equal<Current<INSite.siteID>>>> carts;
		[PXFilterable]
		[PXImport(typeof(INSite))]
		public SelectFrom<INTote>.Where<INTote.siteID.IsEqual<INSite.siteID.FromCurrent>>.View totes;
		public SelectFrom<INTote>.Where<INTote.siteID.IsEqual<INSite.siteID.FromCurrent>.And<INTote.assignedCartID.IsEqual<INCart.cartID.FromCurrent>>>.View totesInCart;
		public PXSetup<Branch, Where<Branch.branchID, Equal<Optional<INSite.branchID>>>> branch;
		public PXSelect<Address,
			Where<Address.bAccountID, Equal<Current<Branch.bAccountID>>,
					And<Address.addressID, Equal<Current<INSite.addressID>>>>> Address;
		public PXSelect<Contact,
			Where<Contact.bAccountID, Equal<Current<Branch.bAccountID>>,
					And<Contact.contactID, Equal<Current<INSite.contactID>>>>> Contact;
		public PXSelect<INItemSite, Where<INItemSite.siteID, Equal<Current<INSite.siteID>>>> itemsiterecords;
		public PXSetup<INSetup> insetup;
		#endregion

		#region Buttons
		public PXAction<INSite> viewRestrictionGroups;
		[PXUIField(DisplayName = GL.Messages.ViewRestrictionGroups, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXLookupButton]
		public virtual IEnumerable ViewRestrictionGroups(PXAdapter adapter)
		{
			if (site.Current != null)
			{
				INAccessDetail graph = CreateInstance<INAccessDetail>();
				graph.Site.Current = graph.Site.Search<INSite.siteCD>(site.Current.SiteCD);
				throw new PXRedirectRequiredException(graph, false, "Restricted Groups");
			}
			return adapter.Get();
		}

		[PXCancelButton]
		[PXUIField(DisplayName = ActionsMessages.Cancel, MapEnableRights = PXCacheRights.Select)]
		protected new virtual IEnumerable Cancel(PXAdapter a)
		{
			int? siteID = getDefaultSiteID();
			if (PXAccess.FeatureInstalled<FeaturesSet.warehouse>() || siteID == null)
			{
				INSite current = null;
				foreach (INSite headerCanceled in (new PXCancel<INSite>(this, "Cancel")).Press(a))
				{
					current = headerCanceled;
				}
				yield return current;
			}
			else
				yield return (INSite)PXSelect<INSite, Where<INSite.siteID, Equal<Required<INSite.siteID>>, And<Match<Current<AccessInfo.userName>>>>>.Select(this, siteID);
		}

		public PXAction<INSite> iNLocationLabels;
		[PXUIField(DisplayName = Messages.INLocationLabels, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXLookupButton]
		public virtual IEnumerable INLocationLabels(PXAdapter adapter)
		{
			if (site.Current != null)
			{
				Dictionary<string, string> parameters = new Dictionary<string, string>();
				parameters["WarehouseID"] = site.Current.SiteCD;
				throw new PXReportRequiredException(parameters, "IN619000", Messages.INLocationLabels);
			}
			return adapter.Get();
		}

		public PXAction<INSite> validateAddresses;
		[PXUIField(DisplayName = CS.Messages.ValidateAddresses, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, FieldClass = CS.Messages.ValidateAddress)]
		[PXButton]
		public virtual IEnumerable ValidateAddresses(PXAdapter adapter)
		{
			INSite inSite = this.site.Current;
			if (inSite != null)
			{
				Address address = this.Address.Current;
				if (address != null && address.IsValidated == false)
				{
					PXAddressValidator.Validate<Address>(this, address, true);
				}
			}
			return adapter.Get();
		}

		public PXAction<INSite> viewOnMap;
		[PXUIField(DisplayName = CR.Messages.ViewOnMap, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable ViewOnMap(PXAdapter adapter)
		{
			BAccountUtility.ViewOnMap(this.Address.Current);
			return adapter.Get();
		}

		public PXAction<INSite> viewTotesInCart;
		[PXButton, PXUIField(DisplayName = "Assigned Totes", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		public virtual IEnumerable ViewTotesInCart(PXAdapter adapter)
		{
			totesInCart.AskExt();
			return adapter.Get();
		}
		
		#region MyButtons (MMK)
		public PXAction<INSite> action;
        [PXUIField(DisplayName = "Actions", MapEnableRights = PXCacheRights.Select)]
        [PXButton(SpecialType = PXSpecialButtonType.ActionsFolder)]
        protected virtual IEnumerable Action(PXAdapter adapter)
        {
            return adapter.Get();
        }

        public PXAction<INSite> report;
        [PXUIField(DisplayName = "Reports", MapEnableRights = PXCacheRights.Select)]
        [PXButton(SpecialType = PXSpecialButtonType.ReportsFolder)]
        protected virtual IEnumerable Report(PXAdapter adapter)
        {
            return adapter.Get();
        }

		public PXChangeID<INSite, INSite.siteCD> changeID;
        #endregion
		#endregion

		public INSiteMaint()
		{
            if (insetup.Current == null)
            {
                throw new PXSetupNotEnteredException(ErrorMessages.SetupNotEntered, typeof(INSetup), PXMessages.LocalizeNoPrefix(IN.Messages.INSetup));
            }

            if (!PXAccess.FeatureInstalled<FeaturesSet.warehouse>())
			{
				site.Cache.AllowInsert = getDefaultSiteID() == null;
				Next.SetVisible(false);
				Previous.SetVisible(false);
				Last.SetVisible(false);
				First.SetVisible(false);
			}

			PXUIFieldAttribute.SetVisible<INSite.pPVAcctID>(siteaccounts.Cache, null, true);
			PXUIFieldAttribute.SetVisible<INSite.pPVSubID>(siteaccounts.Cache, null, true);

			PXUIFieldAttribute.SetDisplayName<Contact.salutation>(Caches[typeof(Contact)], CR.Messages.Attention);
			PXUIFieldAttribute.SetDisplayName<INSite.overrideInvtAccSub>(siteaccounts.Cache, PXAccess.FeatureInstalled<FeaturesSet.subAccount>() ? Messages.OverrideInventoryAcctSub : Messages.OverrideInventoryAcct);

			PXUIFieldAttribute.SetEnabled<Contact.fullName>(Caches[typeof(Contact)], null);

			action.AddMenuAction(changeID);

			PXImportAttribute importAttribute = location.Attributes.Find(a => a is PXImportAttribute) as PXImportAttribute;
			importAttribute.MappingPropertiesInit += MappingPropertiesInit;
        }

		#region Cache Attached
		[PXDefault(typeof(Search<Branch.countryID, Where<Branch.branchID, Equal<Current<INSite.branchID>>>>))]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected virtual void Address_CountryID_CacheAttached(PXCache sender) { }

        [PXDBInt(IsKey = true)]
		[PXDBDefault(typeof(INSite.siteID), DefaultForInsert = true, DefaultForUpdate = true)]
		[PXParent(typeof(INLocation.FK.Site))]
		protected virtual void INLocation_SiteID_CacheAttached(PXCache sender) { }

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXUIVerify(typeof(Where<Selector<INSite.receiptLocationID, INLocation.active>, NotEqual<False>>), PXErrorLevel.Error, Messages.LocationIsNotActive, true)]
		protected virtual void INSite_receiptLocationID_CacheAttached(PXCache sender) { }

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXUIVerify(typeof(Where<Selector<INSite.shipLocationID, INLocation.active>, NotEqual<False>>), PXErrorLevel.Error, Messages.LocationIsNotActive, true)]
		protected virtual void INSite_shipLocationID_CacheAttached(PXCache sender) { }

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXUIVerify(typeof(Where<Selector<INSite.dropShipLocationID, INLocation.active>, NotEqual<False>>), PXErrorLevel.Error, Messages.LocationIsNotActive, true)]
		protected virtual void INSite_dropShipLocationID_CacheAttached(PXCache sender) { }

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXUIVerify(typeof(Where<Selector<INSite.returnLocationID, INLocation.active>, NotEqual<False>>), PXErrorLevel.Error, Messages.LocationIsNotActive, true)]
		protected virtual void INSite_returnLocationID_CacheAttached(PXCache sender) { } 
		#endregion

		#region Events
		#region INSite
		protected virtual void INSite_RowInserted(PXCache cache, PXRowInsertedEventArgs e)
		{
			try
			{
				var addr = new Address
				{
					BAccountID = (int?)branch.Cache.GetValue<Branch.bAccountID>(branch.Current)
				};
				addr = (Address)Address.Cache.Insert(addr);

				var cont = new Contact
				{
					BAccountID = (int?)branch.Cache.GetValue<Branch.bAccountID>(branch.Current),
					DefAddressID = addr.AddressID
				};
				cont = (Contact)Contact.Cache.Insert(cont);
			}
			finally
			{
				Address.Cache.IsDirty = false;
				Contact.Cache.IsDirty = false;
			}
		}

		protected virtual void INSite_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
        {
			if (!sender.ObjectsEqual<INSite.branchID>(e.Row, e.OldRow))
            {
				bool found = false;
				foreach (Address record in Address.Cache.Inserted)
                {
					record.BAccountID = (int?)branch.Cache.GetValue<Branch.bAccountID>(branch.Current);
					record.CountryID = (string)branch.Cache.GetValue<Branch.countryID>(branch.Current);
					found = true;
				}

				if (!found)
				{
					object old_branch = branch.View.SelectSingleBound(new object[] { e.OldRow });
					Address addr = (Address)Address.View.SelectSingleBound(new object[] { old_branch, e.OldRow }) ?? new Address();

					addr.BAccountID = (int?)branch.Cache.GetValue<Branch.bAccountID>(branch.Current);
					addr.CountryID = (string)branch.Cache.GetValue<Branch.countryID>(branch.Current);
					addr.AddressID = null;
					Address.Cache.Insert(addr);

				}
				else
				{
					Address.Cache.Normalize();
				}

				found = false;
				foreach (Contact cont in Contact.Cache.Inserted)
                    {
					cont.BAccountID = (int?)branch.Cache.GetValue<Branch.bAccountID>(branch.Current);
					cont.DefAddressID = null;
					foreach (Address record in Address.Cache.Inserted)
					{
						cont.DefAddressID = record.AddressID;
					}
					found = true;
                    }

				if (!found)
				{
					object old_branch = branch.View.SelectSingleBound(new object[] { e.OldRow });
					Contact cont = (Contact)Contact.View.SelectSingleBound(new object[] { old_branch, e.OldRow }) ?? new Contact();

					cont.BAccountID = (int?)branch.Cache.GetValue<Branch.bAccountID>(branch.Current);
					cont.DefAddressID = null;
					foreach (Address record in Address.Cache.Inserted)
					{
						cont.DefAddressID = record.AddressID;
					}
					cont.ContactID = null;
					Contact.Cache.Insert(cont);
				}
				else
				{
					Contact.Cache.Normalize();
				}
			}
			if (e.Row != null && e.OldRow != null && !PXAccess.FeatureInstalled<FeaturesSet.warehouse>() && !sender.ObjectsEqual<INSite.replenishmentClassID>(e.Row, e.OldRow))
			{
				string replenishmentClassID = ((INSite)e.Row).ReplenishmentClassID;
				if (replenishmentClassID != null)
				{
					foreach (INItemSite initemsite in PXSelect<INItemSite, Where<INItemSite.siteID, Equal<Current<INSite.siteID>>>>.Select(this))
					{
						initemsite.ReplenishmentClassID = replenishmentClassID;
						INItemSiteMaint.DefaultItemReplenishment(this, initemsite);
						INItemSiteMaint.DefaultSubItemReplenishment(this, initemsite);
						this.Caches[typeof(INItemSite)].Update(initemsite);
					}
				}
			}
                }

		protected virtual void INSite_RowUpdating(PXCache cache, PXRowUpdatingEventArgs e)
		{
			UpateSiteLocation<INSite.receiptLocationID, INSite.receiptLocationIDOverride>(cache, e);
			UpateSiteLocation<INSite.shipLocationID, INSite.shipLocationIDOverride>(cache, e);
            }

		protected virtual void INSite_RowDeleted(PXCache cache, PXRowDeletedEventArgs e)
		{
			Address.Cache.Delete(Address.Current);
			Contact.Cache.Delete(Contact.Current);
        }

		protected virtual void INSite_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			INAcctSubDefault.Required(sender, e);
			INSite row = (INSite)e.Row;
			if (row != null)
			{
				viewRestrictionGroups.SetEnabled(row.SiteCD != null);

				foreach (INLocation deletedLocation in location.Cache.Cached)
				{
					if (location.Cache.GetStatus(deletedLocation).IsIn(PXEntryStatus.Deleted, PXEntryStatus.InsertedDeleted))
					{
						if (deletedLocation.LocationID == row.ReceiptLocationID)
							sender.RaiseExceptionHandling<INSite.receiptLocationID>(row, deletedLocation.LocationCD, new PXSetPropertyException(ErrorMessages.ForeignRecordDeleted));
						if (deletedLocation.LocationID == row.ShipLocationID)
							sender.RaiseExceptionHandling<INSite.shipLocationID>(row, deletedLocation.LocationCD, new PXSetPropertyException(ErrorMessages.ForeignRecordDeleted));
						if (deletedLocation.LocationID == row.ReturnLocationID)
							sender.RaiseExceptionHandling<INSite.returnLocationID>(row, deletedLocation.LocationCD, new PXSetPropertyException(ErrorMessages.ForeignRecordDeleted));
						if (deletedLocation.LocationID == row.DropShipLocationID)
							sender.RaiseExceptionHandling<INSite.dropShipLocationID>(row, deletedLocation.LocationCD, new PXSetPropertyException(ErrorMessages.ForeignRecordDeleted));
					}
				}
			}
		}

		protected virtual void INSite_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			INAcctSubDefault.Required(sender, e);
			if (e.Operation.Command().IsIn(PXDBOperation.Insert, PXDBOperation.Update))
			{
				var site = (INSite)e.Row;
				if (site.OverrideInvtAccSub != true)
				{
					PXDefaultAttribute.SetPersistingCheck<INSite.invtAcctID>(sender, e.Row, PXPersistingCheck.Nothing);
					PXDefaultAttribute.SetPersistingCheck<INSite.invtSubID>(sender, e.Row, PXPersistingCheck.Nothing);
				}
				if (site.ReceiptLocationIDOverride == true || site.ShipLocationIDOverride == true)
				{
					List<PXDataFieldParam> prm = new List<PXDataFieldParam>();
					if (site.ReceiptLocationIDOverride == true)
						prm.Add(new PXDataFieldAssign(typeof(INItemSite.dfltReceiptLocationID).Name, PXDbType.Int, site.ReceiptLocationID));
					if (site.ShipLocationIDOverride == true)
						prm.Add(new PXDataFieldAssign(typeof(INItemSite.dfltShipLocationID).Name, PXDbType.Int, site.ShipLocationID));
					prm.Add(new PXDataFieldRestrict(typeof(INItemSite.siteID).Name, PXDbType.Int, site.SiteID));
					PXDatabase.Update<INItemSite>(prm.ToArray());
				}

				if (site.Active != true) 
				{
					bool cantDeactivateSite = (INRegister)
						PXSelect<INRegister,
						Where<INRegister.released, NotEqual<True>,
							And<Where<INRegister.siteID, Equal<Current<INSite.siteID>>,
							Or<INRegister.toSiteID, Equal<Current<INSite.siteID>>>>>>>
						.SelectSingleBound(this, new [] { e.Row }) != null;

					cantDeactivateSite = cantDeactivateSite || (INTran)
						PXSelect<INTran,
						Where<INTran.released, NotEqual<True>,
							And<Where<INTran.siteID, Equal<Current<INSite.siteID>>,
							Or<INTran.toSiteID, Equal<Current<INSite.siteID>>>>>>>
						.SelectSingleBound(this, new [] {e.Row}) != null;

					if (cantDeactivateSite)
						sender.RaiseExceptionHandling<INSite.active>(e.Row, null, new PXSetPropertyException(Messages.CantDeactivateSite));
					}
				}
		}

		protected virtual void INSite_ReceiptLocationID_ExceptionHandling(PXCache sender, PXExceptionHandlingEventArgs e)
		{
			if (IsImport)
			{
				_WrongLocations[0] = e.NewValue as string;
				e.Cancel = true;
			}
		}

		protected virtual void INSite_ReturnLocationID_ExceptionHandling(PXCache sender, PXExceptionHandlingEventArgs e)
		{
			if (IsImport)
			{
				_WrongLocations[1] = e.NewValue as string;
				e.Cancel = true;
			}
		}

		protected virtual void INSite_DropShipLocationID_ExceptionHandling(PXCache sender, PXExceptionHandlingEventArgs e)
		{
			if (IsImport)
			{
				_WrongLocations[2] = e.NewValue as string;
				e.Cancel = true;
			}
		}

		protected virtual void INSite_ShipLocationID_ExceptionHandling(PXCache sender, PXExceptionHandlingEventArgs e)
		{
			if (IsImport)
			{
				_WrongLocations[3] = e.NewValue as string;
				e.Cancel = true;
			}
		}

		protected string[] _WrongLocations = { null, null, null, null };
		#endregion

		#region INLocation
		protected virtual void INLocation_RowInserting(PXCache sender, PXRowInsertingEventArgs e)
        {
			var row = (INLocation)e.Row;
            if (row != null)
            {
				var located = (INLocation)sender.Locate(row);
                if (located != null)
                    row.LocationID = located.LocationID;
                }
            }

		protected virtual void INLocation_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			string cd;
			var loc = (INLocation)e.Row;
			if (site.Current != null && IsImport && (cd = loc.LocationCD) != null)
			{
				if (_WrongLocations[0] == cd)
				{
					site.Current.ReceiptLocationID = loc.LocationID;
					_WrongLocations[0] = null;
				}
				if (_WrongLocations[1] == cd)
				{
					site.Current.ReturnLocationID = loc.LocationID;
					_WrongLocations[1] = null;
				}
				if (_WrongLocations[2] == cd)
				{
					site.Current.DropShipLocationID = loc.LocationID;
					_WrongLocations[2] = null;
				}
				if (_WrongLocations[3] == cd)
				{
					site.Current.ShipLocationID = loc.LocationID;
					_WrongLocations[3] = null;
				}
			}
		}

		protected virtual void INLocation_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			var row = (INLocation)e.Row;
			if (row == null) return;

			if(row.PrimaryItemID != null && InventoryItem.PK.Find(this, row.PrimaryItemID) == null)
				PXUIFieldAttribute.SetWarning<INLocation.primaryItemID>(sender, row, Messages.ItemWasDeleted);
			PXUIFieldAttribute.SetEnabled<INLocation.isCosted>(sender, row, row.ProjectID == null);

			bool warn = ShowWarningProjectLowestPickPriority(row);
			PXUIFieldAttribute.SetWarning<INLocation.pickPriority>(sender, row, warn ? Messages.LocationWithProjectLowestPickPriority : null);
		}

		protected virtual void INLocation_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			var row = (INLocation)e.Row;
			if (row == null) return;

			if ((e.Operation & PXDBOperation.Command) != PXDBOperation.Delete && row.ProjectID != null && row.TaskID == null && row.Active == true)
			{
				INLocation anotherWildcardLocation =
					PXSelect<INLocation,
					Where<INLocation.locationID, NotEqual<Required<INLocation.locationID>>,
						And<INLocation.projectID, Equal<Required<INLocation.projectID>>,
						And<INLocation.taskID, IsNull,
						And<INLocation.active, Equal<True>>>>>>
					.Select(this, row.LocationID, row.ProjectID);

				if (anotherWildcardLocation != null)
				{
					PMProject project = PXSelect<PMProject, Where<PMProject.contractID, Equal<Required<PMProject.contractID>>>>.Select(this, row.ProjectID);
					INSite warehouse = INSite.PK.Find(this, anotherWildcardLocation.SiteID);
					if (sender.RaiseExceptionHandling<INLocation.projectID>(e.Row, project.ContractCD, new PXSetPropertyException(Messages.ProjectWildcardLocationIsUsedIn, PXErrorLevel.Error, warehouse.SiteCD, anotherWildcardLocation.LocationCD)))
					{
						throw new PXRowPersistingException(PXDataUtils.FieldName<INLocation.projectID>(), row.ProjectID, Messages.ProjectWildcardLocationIsUsedIn, warehouse.SiteCD, anotherWildcardLocation.LocationCD);
					}
				}
			}
			if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Delete)
			{
				INItemSite itemSite =
					PXSelect<INItemSite,
					Where<INItemSite.siteID, Equal<Current<INSite.siteID>>,
						And<Where<
							INItemSite.dfltReceiptLocationID, Equal<Required<INItemSite.dfltReceiptLocationID>>,
							Or<INItemSite.dfltShipLocationID, Equal<Required<INItemSite.dfltShipLocationID>>>>>>>
					.Select(this, row.LocationID, row.LocationID);

				if (itemSite != null)
				{
					InventoryItem initem = InventoryItem.PK.Find(this, itemSite.InventoryID) ?? new InventoryItem();
					if (sender.RaiseExceptionHandling<INLocation.locationID>(e.Row, row.LocationCD, new PXSetPropertyException(Messages.LocationInUseInItemWarehouseDetails, PXErrorLevel.Error, row.LocationCD.TrimEnd(), initem.InventoryCD.TrimEnd())))
					{
						throw new PXRowPersistingException(PXDataUtils.FieldName<INLocation.locationID>(), row.LocationID, Messages.LocationInUseInItemWarehouseDetails, row.LocationCD.TrimEnd(), initem.InventoryCD.TrimEnd());
					}
				}

				INPIClassLocation piLocation =
					PXSelectJoin<INPIClassLocation,
					InnerJoin<INPIClass, On<INPIClassLocation.FK.PIClass>>,
					Where<INPIClass.siteID, Equal<Current<INSite.siteID>>,
						And<INPIClassLocation.locationID, Equal<Required<INPIClassLocation.locationID>>>>>
					.Select(this, row.LocationID);

				if (piLocation != null)
				{
					if (sender.RaiseExceptionHandling<INLocation.locationID>(e.Row, row.LocationCD, new PXSetPropertyException(Messages.LocationInUseInPIType, PXErrorLevel.Error, row.LocationCD.TrimEnd(), piLocation.PIClassID.TrimEnd())))
					{
						throw new PXRowPersistingException(PXDataUtils.FieldName<INLocation.locationID>(), row.LocationID, Messages.LocationInUseInPIType, row.LocationCD.TrimEnd(), piLocation.PIClassID.TrimEnd());
					}
				}
			}
		}

		protected virtual void INLocation_ProjectID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			var row = (INLocation)e.Row;
			if (row?.ProjectID != null)
				sender.SetValueExt<INLocation.isCosted>(row, true);
		}

		protected virtual void INLocation_ProjectID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			///TODO: Redo this using Plans and Status tables once we have them in version 7.0

			var row = (INLocation)e.Row;
			if (row == null) return;

			PO.POReceiptLine unreleasedPO =
				PXSelect<PO.POReceiptLine,
				Where<PO.POReceiptLine.projectID, Equal<Required<PO.POReceiptLine.projectID>>,
					And<PO.POReceiptLine.released, Equal<False>,
					And<PO.POReceiptLine.locationID, Equal<Required<INLocation.locationID>>>>>>
				.SelectWindowed(this, 0, 1, row.ProjectID, row.LocationID);

			if (unreleasedPO != null)
			{
				PMProject project = PXSelect<PMProject, Where<PMProject.contractID, Equal<Required<PMProject.contractID>>>>.Select(this, row.ProjectID ?? e.NewValue);
				if (project != null)
					e.NewValue = project.ContractCD;

				throw new PXSetPropertyException(Messages.ProjectUsedInPO);
			}

			SO.SOShipLine unreleasedSO =
				PXSelect<SO.SOShipLine,
				Where<SO.SOShipLine.projectID, Equal<Required<SO.SOShipLine.projectID>>,
					And<SO.SOShipLine.released, Equal<False>,
					And<SO.SOShipLine.locationID, Equal<Required<INLocation.locationID>>>>>>
				.SelectWindowed(this, 0, 1, row.ProjectID, row.LocationID);

			if (unreleasedSO != null)
			{
				PMProject project = PXSelect<PMProject, Where<PMProject.contractID, Equal<Required<PMProject.contractID>>>>.Select(this, row.ProjectID ?? e.NewValue);
				if (project != null)
					e.NewValue = project.ContractCD;

				throw new PXSetPropertyException(Messages.ProjectUsedInSO);
			}

			INLocationStatus locationStatus =
				PXSelect<INLocationStatus,
				Where<INLocationStatus.siteID, Equal<Required<INLocationStatus.siteID>>,
					And<INLocationStatus.locationID, Equal<Required<INLocationStatus.locationID>>,
					And<INLocationStatus.qtyOnHand, NotEqual<decimal0>>>>>
				.SelectWindowed(this, 0, 1, row.SiteID, row.LocationID);

			if (locationStatus != null)
			{
				PMProject project = PXSelect<PMProject, Where<PMProject.contractID, Equal<Required<PMProject.contractID>>>>.Select(this, row.ProjectID ?? e.NewValue);
				if (project != null)
					e.NewValue = project.ContractCD;

				throw new PXSetPropertyException(Messages.ProjectUsedInIN);
			}

		}

		protected virtual void INLocation_TaskID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			///TODO: Redo this using Plans and Status tables once we have them in version 7.0

			var row = (INLocation)e.Row;
			if (row == null) return;
			
			PO.POReceiptLine unreleasedPO =
				PXSelect<PO.POReceiptLine,
				Where<PO.POReceiptLine.taskID, Equal<Required<PO.POReceiptLine.taskID>>,
					And<PO.POReceiptLine.released, Equal<False>,
					And<PO.POReceiptLine.locationID, Equal<Required<INLocation.locationID>>>>>>
				.SelectWindowed(this, 0, 1, row.TaskID, row.LocationID);

			if (unreleasedPO != null)
			{
				PMTask task = PXSelect<PMTask, Where<PMTask.taskID, Equal<Required<PMTask.taskID>>>>.Select(this, row.TaskID ?? e.NewValue);
				if (task != null)
					e.NewValue = task.TaskCD;
				
				throw new PXSetPropertyException(Messages.TaskUsedInPO);
			}

			SO.SOShipLine unreleasedSO =
				PXSelect<SO.SOShipLine,
				Where<SO.SOShipLine.taskID, Equal<Required<SO.SOShipLine.taskID>>,
					And<SO.SOShipLine.released, Equal<False>,
					And<SO.SOShipLine.locationID, Equal<Required<INLocation.locationID>>>>>>
				.SelectWindowed(this, 0, 1, row.TaskID, row.LocationID);

			if (unreleasedSO != null)
			{
				PMTask task = PXSelect<PMTask, Where<PMTask.taskID, Equal<Required<PMTask.taskID>>>>.Select(this, row.TaskID ?? e.NewValue);
				if (task != null)
					e.NewValue = task.TaskCD;

				throw new PXSetPropertyException(Messages.TaskUsedInSO);
			}

			INLocationStatus locationStatus =
				PXSelect<INLocationStatus,
				Where<INLocationStatus.siteID, Equal<Required<INLocationStatus.siteID>>,
					And<INLocationStatus.locationID, Equal<Required<INLocationStatus.locationID>>,
					And<INLocationStatus.qtyOnHand, NotEqual<decimal0>>>>>
				.SelectWindowed(this, 0, 1, row.SiteID, row.LocationID);

			if (locationStatus != null)
			{
				PMTask task = PXSelect<PMTask, Where<PMTask.taskID, Equal<Required<PMTask.taskID>>>>.Select(this, row.TaskID ?? e.NewValue);
				if (task != null)
					e.NewValue = task.TaskCD;

				throw new PXSetPropertyException(Messages.TaskUsedInIN);
			} 
		}

		protected virtual void INLocation_IsCosted_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if (e.Row == null) return;

			bool enable;
			if ((bool?)e.NewValue == true)
			{
				INLocationStatus status =
				PXSelect<INLocationStatus,
					Where<INLocationStatus.siteID, Equal<Current<INLocation.siteID>>,
						And<INLocationStatus.locationID, Equal<Current<INLocation.locationID>>,
						And<INLocationStatus.qtyOnHand, NotEqual<decimal0>>>>>
					.SelectSingleBound(this, new[] { e.Row });
							enable = status == null;

			}
			else
			{
				INCostStatus status =
				PXSelect<INCostStatus,
					Where<INCostStatus.costSiteID, Equal<Current<INLocation.locationID>>,
						And<INCostStatus.qtyOnHand, Greater<decimal0>>>>
					.SelectSingleBound(this, new[] { e.Row });
							enable = status == null;
			}

			if (!enable)
				throw new PXSetPropertyException(Messages.LocationCostedWarning, PXErrorLevel.Error);

			if ((bool?)e.NewValue == true)
				sender.RaiseExceptionHandling<INLocation.isCosted>(e.Row, true, new PXSetPropertyException(Messages.LocationCostedSetWarning, PXErrorLevel.RowWarning));
		}
		#endregion
		
		#region Address
		protected virtual void Address_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			var addr = (Address)e.Row;
			if (addr != null)
				site.Current.AddressID = addr.AddressID;
			}

		protected virtual void Address_BAccountID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (branch.Current != null)
			{
				e.NewValue = (int?)branch.Cache.GetValue<Branch.bAccountID>(branch.Current);
				e.Cancel = true;
			}
		}

		protected virtual void Address_CountryID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			var address = (Address)e.Row;
			address.State = null;
			address.PostalCode = null;
		}
		#endregion

		#region Contact
		protected virtual void Contact_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			var cont = (Contact)e.Row;
			if (cont != null)
				site.Current.ContactID = cont.ContactID;
		}
		
		protected virtual void Contact_BAccountID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (branch.Current != null)
			{
				e.NewValue = (int?)branch.Cache.GetValue<Branch.bAccountID>(branch.Current);
				e.Cancel = true;
			}
		}

		protected virtual void Contact_DefAddressID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			e.Cancel = true;
		}

		protected virtual void Contact_ContactType_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
			{
			e.NewValue = ContactTypesAttribute.BAccountProperty;
		}
		#endregion
		#endregion


		public override void Persist()
		{
			using (var ts = new PXTransactionScope())
			{
				foreach (INSite record in site.Cache.Deleted)
				{
					PXDatabase.Delete<INSiteStatus>(
						new PXDataFieldRestrict<INSiteStatus.siteID>(PXDbType.Int, 4, record.SiteID, PXComp.EQ),
						new PXDataFieldRestrict<INSiteStatus.qtyOnHand>(PXDbType.Decimal, 8, 0m, PXComp.EQ),
						new PXDataFieldRestrict<INSiteStatus.qtyAvail>(PXDbType.Decimal, 8, 0m, PXComp.EQ));

					PXDatabase.Delete<INLocationStatus>(
						new PXDataFieldRestrict<INLocationStatus.siteID>(PXDbType.Int, 4, record.SiteID, PXComp.EQ),
						new PXDataFieldRestrict<INLocationStatus.qtyOnHand>(PXDbType.Decimal, 8, 0m, PXComp.EQ),
						new PXDataFieldRestrict<INLocationStatus.qtyAvail>(PXDbType.Decimal, 8, 0m, PXComp.EQ));

					PXDatabase.Delete<INLotSerialStatus>(
						new PXDataFieldRestrict<INLotSerialStatus.siteID>(PXDbType.Int, 4, record.SiteID, PXComp.EQ),
						new PXDataFieldRestrict<INLotSerialStatus.qtyOnHand>(PXDbType.Decimal, 8, 0m, PXComp.EQ),
						new PXDataFieldRestrict<INLotSerialStatus.qtyAvail>(PXDbType.Decimal, 8, 0m, PXComp.EQ));

					InventoryItem item =
						PXSelectReadonly2<InventoryItem,
							InnerJoin<INSiteStatus, On<INSiteStatus.FK.InventoryItem>>,
							Where<INSiteStatus.siteID, Equal<Required<INSiteStatus.siteID>>>>
						.SelectWindowed(this, 0, 1, record.SiteID);

					if (item?.InventoryCD != null)
						throw new PXRowPersistingException(typeof(INSite.siteCD).Name, record, Messages.SiteUsageDeleted, item.InventoryCD.TrimEnd());
				}

				ts.Complete();
				}

			base.Persist();
			location.Cache.Clear();
			}

		public override void Clear()
					{
			base.Clear();
			_WrongLocations[0] = null;
			_WrongLocations[1] = null;
			_WrongLocations[2] = null;
			_WrongLocations[3] = null;
		}
		
		protected virtual bool ShowWarningProjectLowestPickPriority(INLocation row)
		{
			if (row.ProjectID == null)
				return false;

			return location.Select().RowCast<INLocation>().Any(
				l => l.Active == true
					 && l.ProjectID == null
					 && l.PickPriority >= row.PickPriority);
		}

		private void MappingPropertiesInit(object sender, PXImportAttribute.MappingPropertiesInitEventArgs e)
		{
			string isCostedFieldName = location.Cache.GetField(typeof(INLocation.isCosted));
			if (!e.Names.Contains(isCostedFieldName))
			{
				e.Names.Add(isCostedFieldName);
				e.DisplayNames.Add(PXUIFieldAttribute.GetDisplayName<INLocation.taskID>(location.Cache));
			}

			string projectTaskFieldName = location.Cache.GetField(typeof(INLocation.taskID));
			if (!e.Names.Contains(projectTaskFieldName))
			{
				e.Names.Add(projectTaskFieldName);
				e.DisplayNames.Add(PXUIFieldAttribute.GetDisplayName<INLocation.taskID>(location.Cache));
			}

		}

		protected void UpateSiteLocation<Field, FieldResult>(PXCache cache, PXRowUpdatingEventArgs e)
			where Field : IBqlField
			where FieldResult : IBqlField
		{
			int? newValue = (int?)cache.GetValue<Field>(e.NewRow);
			int? value = (int?)cache.GetValue<Field>(e.Row);
			if (value != newValue && e.ExternalCall)
			{
				INItemSite itemsite = PXSelect<INItemSite, Where<INItemSite.siteID, Equal<Required<INItemSite.siteID>>>>.SelectWindowed(this, 0, 1, cache.GetValue<INSite.siteID>(e.Row));
				if (itemsite != null && site.Ask(Messages.Warning, Messages.SiteLocationOverride, MessageButtons.YesNo) == WebDialogResult.Yes)
					cache.SetValue<FieldResult>(e.NewRow, true);
				else
					cache.SetValue<FieldResult>(e.NewRow, false);
			}
		}

		protected int? getDefaultSiteID()
		{
			using (PXDataRecord record = PXDatabase.SelectSingle<INSite>(
				new PXDataField<INSite.siteID>(),
				new PXDataFieldOrder<INSite.siteID>()))
			{
				if (record != null)
					return record.GetInt32(0);
			}
			return null;
		}
	}
}
