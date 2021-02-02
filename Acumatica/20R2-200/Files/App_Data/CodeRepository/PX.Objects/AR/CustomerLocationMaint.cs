using System;
using PX.Data;
using PX.Objects.CR;
using PX.Objects.Common.Discount;
using System.Collections;
using PX.Objects.CR.Extensions.Relational;

namespace PX.Objects.AR
{
	public class CustomerLocationMaint : LocationMaint
	{
		#region Views

		public PXSelect<
				Customer,
			Where<
				Customer.bAccountID, Equal<Current<Location.bAccountID>>,
				And<Customer.bAccountID, IsNotNull,
				And<Match<Customer, Current<AccessInfo.userName>>>>>>
			Customer;

		#endregion

		#region ctor

		public CustomerLocationMaint() : base()
		{
			Location.Join<LeftJoin<Customer, On<Customer.bAccountID, Equal<Location.bAccountID>>>>();

			Location.WhereAnd<Where<
				Customer.bAccountID, Equal<Location.bAccountID>,
				And<Customer.bAccountID, IsNotNull,
				And<Match<Customer, Current<AccessInfo.userName>>>>>>();

			Location.WhereAnd<Where<Location.locType, Equal<LocTypeList.customerLoc>,
				Or<Location.locType, Equal<LocTypeList.combinedLoc>>>>();

			MenuAction.AddMenuAction(ViewAccountLocation);
			MenuAction.AddMenuAction(validateAddresses);
			Actions.Move(nameof(Delete), nameof(CopyPaste), true);
		}

		#endregion

		#region Buttons

		public PXCopyPasteAction<Location> CopyPaste;
		public PXMenuAction<Location> MenuAction;


		[PXUIField(DisplayName = ActionsMessages.Cancel, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXCancelButton]
		protected override IEnumerable Cancel(PXAdapter adapter)
		{
			int? acctid = Location.Current != null ? Location.Current.BAccountID : null;
			foreach (PXResult<Location, BAccount> loc in (new PXCancel<Location>(this, "Cancel")).Press(adapter))
			{
				Location location = loc;
				if (!IsImport && Location.Cache.GetStatus(location) == PXEntryStatus.Inserted
						&& (acctid != location.BAccountID || string.IsNullOrEmpty(location.LocationCD)))
				{
					foreach (PXResult<Location, BAccount> first in First.Press(adapter))
					{
						return new object[] { first };
					}
					location.LocationCD = null;
					return new object[] { loc };
				}
				else
				{
					return new object[] { loc };
				}
			}
			return new object[0];
		}

		[PXUIField(DisplayName = ActionsMessages.Previous, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXPreviousButton]
		protected override IEnumerable Previous(PXAdapter adapter)
		{
			foreach (PXResult<Location, BAccount> loc in (new PXPrevious<Location>(this, "Prev")).Press(adapter))
			{
				if (Location.Cache.GetStatus((Location)loc) == PXEntryStatus.Inserted)
				{
					return Last.Press(adapter);
				}
				else
				{
					return new object[] { loc };
				}
			}
			return new object[0];
		}

		[PXUIField(DisplayName = ActionsMessages.Next, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXNextButton]
		protected override IEnumerable Next(PXAdapter adapter)
		{
			foreach (PXResult<Location, BAccount> loc in (new PXNext<Location>(this, "Next")).Press(adapter))
			{
				if (Location.Cache.GetStatus((Location)loc) == PXEntryStatus.Inserted)
				{
					return First.Press(adapter);
				}
				else
				{
					return new object[] { loc };
				}
			}
			return new object[0];
		}

		public PXAction<BAccount> ViewAccountLocation;
		[PXUIField(DisplayName = "View Account Location", Visible = false)]
		[PXButton]
		protected virtual void viewAccountLocation()
		{
			if (Location.Current is Location location)
			{
				var graph = PXGraph.CreateInstance<AccountLocationMaint>();
				graph.Location.Current = location;
				PXRedirectHelper.TryRedirect(graph, PXRedirectHelper.WindowMode.Same);
			}
		}


		#endregion

		#region Events

		protected override void Location_RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
		{
			base.Location_RowPersisted(sender, e);

			if (e.TranStatus == PXTranStatus.Completed)
				DiscountEngine.RemoveFromCachedCustomerPriceClasses(((Location)e.Row).BAccountID);
		}

		#endregion

		#region Methods

		[PXDefault(typeof(Location.bAccountID))]
		[Customer(typeof(Search<Customer.bAccountID,
			Where<Where<
				Customer.type, Equal<BAccountType.customerType>,
				Or<Customer.type, Equal<BAccountType.prospectType>, 
				Or<Customer.type, Equal<BAccountType.combinedType>>>>>>), IsKey = true, TabOrder = 0)]
		[PXParent(typeof(Select<BAccount, Where<BAccount.bAccountID, Equal<Current<Location.bAccountID>>>>))]
		protected override void _(Events.CacheAttached<Location.bAccountID> e) { }

		[PXUIField(DisplayName = "Default Branch")]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected virtual void _(Events.CacheAttached<Location.cBranchID> e) { }

		protected override void EstablishVTaxZoneRule(Action<Type, bool> setCheck)
		{
			setCheck(typeof(Location.vTaxZoneID), false);
		}

		#endregion

		#region Extensions

		/// <exclude/>
		// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
		public class LocationBAccountSharedContactOverrideGraphExt : SharedChildOverrideGraphExt<CustomerLocationMaint, LocationBAccountSharedContactOverrideGraphExt>
		{
			#region Initialization 

			protected override DocumentMapping GetDocumentMapping()
			{
				return new DocumentMapping(typeof(Location))
				{
					RelatedID = typeof(Location.bAccountID),
					ChildID = typeof(Location.defContactID),
					IsOverrideRelated = typeof(Location.overrideContact)
				};
			}

			protected override RelatedMapping GetRelatedMapping()
			{
				return new RelatedMapping(typeof(Customer))
				{
					RelatedID = typeof(Customer.bAccountID),
					ChildID = typeof(Customer.defContactID)
				};
			}

			protected override ChildMapping GetChildMapping()
			{
				return new ChildMapping(typeof(Contact))
				{
					ChildID = typeof(Contact.contactID),
					RelatedID = typeof(Contact.bAccountID),
				};
			}

			#endregion
		}

		/// <exclude/>
		// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
		public class LocationBAccountSharedAddressOverrideGraphExt : SharedChildOverrideGraphExt<CustomerLocationMaint, LocationBAccountSharedAddressOverrideGraphExt>
		{
			#region Initialization 

			protected override DocumentMapping GetDocumentMapping()
			{
				return new DocumentMapping(typeof(Location))
				{
					RelatedID = typeof(Location.bAccountID),
					ChildID = typeof(Location.defAddressID),
					IsOverrideRelated = typeof(Location.overrideAddress)
				};
			}

			protected override RelatedMapping GetRelatedMapping()
			{
				return new RelatedMapping(typeof(Customer))
				{
					RelatedID = typeof(Customer.bAccountID),
					ChildID = typeof(Customer.defAddressID)
				};
			}

			protected override ChildMapping GetChildMapping()
			{
				return new ChildMapping(typeof(Address))
				{
					ChildID = typeof(Address.addressID),
					RelatedID = typeof(Address.bAccountID),
				};
			}

			#endregion
		}

		#endregion
	}
}
