using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Data;
using PX.Objects.AR;
using PX.Objects.PO;
using PX.Objects.AP;
using System.Collections;
using PX.Objects.CR;
using PX.Objects.CM;
using PX.Objects.CS;

namespace PX.Objects.RQ
{
	public class RQBiddingEntry : PXGraph<RQBiddingEntry>
	{
		public PXSave<RQBiddingVendor> Save;
		public PXAction<RQBiddingVendor> cancel;
		public PXBiddingInsert<RQBiddingVendor> Insert;				
		public PXDelete<RQBiddingVendor> Delete;
		public PXFirst<RQBiddingVendor> First;						
		public PXAction<RQBiddingVendor> previous;

		[PXUIField(DisplayName = ActionsMessages.Previous, MapEnableRights = PXCacheRights.Select)]
		[PXPreviousButton]
		protected virtual IEnumerable Previous(PXAdapter adapter)
		{
			foreach (PXResult<RQBiddingVendor, RQRequisition> item in new PXPrevious<RQBiddingVendor>(this, "Previous").Press(adapter))
			{
				if (Vendor.Cache.GetStatus((RQBiddingVendor)item) == PXEntryStatus.Inserted && adapter.Searches != null)
				{
					adapter.Searches = null;
					foreach (PXResult<RQBiddingVendor, RQRequisition> inserted in Insert.Press(adapter))
					{
						yield return inserted;
					}
				}
				else
				{
					yield return item;
				}
			}
		}

		public PXAction<RQBiddingVendor> next;
		[PXUIField(DisplayName = ActionsMessages.Next, MapEnableRights = PXCacheRights.Select)]
		[PXNextButton]
		protected virtual IEnumerable Next(PXAdapter adapter)
		{
			foreach (PXResult<RQBiddingVendor, RQRequisition> item in new PXNext<RQBiddingVendor>(this, "Next").Press(adapter))
			{
				if (Vendor.Cache.GetStatus((RQBiddingVendor)item) == PXEntryStatus.Inserted && adapter.Searches != null)
				{
					adapter.Searches = null;
					foreach (PXResult<RQBiddingVendor, RQRequisition> inserted in Insert.Press(adapter))
					{
						yield return inserted;
					}
				}
				else
				{
					yield return item;
				}
			}
		}

		public PXLast<RQBiddingVendor> Last;

		#region Additional Buttons
		public PXAction<RQRequest> validateAddresses;
		[PXUIField(DisplayName = CS.Messages.ValidateAddresses, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, FieldClass = CS.Messages.ValidateAddress)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.Process)]
		public virtual IEnumerable ValidateAddresses(PXAdapter adapter)
		{
			foreach (RQRequest current in adapter.Get<RQRequest>())
			{
				if (current != null)
				{
					PORemitAddress address = this.Remit_Address.Select();
					if (address != null && address.IsDefaultAddress == false && address.IsValidated == false)
					{
						PXAddressValidator.Validate<PORemitAddress>(this, address, true);
					}
				}
				yield return current;
			}
		} 
		#endregion

		#region Cache Attached
		
		#region RQBiddingVendor
		[PXDBIdentity]
		protected virtual void RQBiddingVendor_LineID_CacheAttached(PXCache sender)
		{
		}

		[PXDBString(15, IsUnicode = true, InputMask = "", IsKey = true)]
		[PXDefault("", PersistingCheck = PXPersistingCheck.NullOrBlank)]
		[PXSelectorAttribute(
			typeof(Search<RQRequisition.reqNbr,
			Where<RQRequisition.status, Equal<RQRequisitionStatus.bidding>>>),
			typeof(RQRequisition.status),
			typeof(RQRequisition.employeeID),
			typeof(RQRequisition.vendorID),
			Filterable = true)]
		[PXUIField(DisplayName = "Requisition")]
		protected virtual void RQBiddingVendor_ReqNbr_CacheAttached(PXCache sender)
		{
		}		

		#region VendorID
		[PXDefault]
		[VendorNonEmployeeActive(typeof(Search2<BAccountR.bAccountID,
			LeftJoin<RQBiddingVendor,
						On<RQBiddingVendor.reqNbr, Equal<Current<RQBiddingVendor.reqNbr>>,
						And<RQBiddingVendor.vendorID, Equal<BAccountR.bAccountID>>>>>), 
						Visibility = PXUIVisibility.SelectorVisible, DescriptionField = typeof(Vendor.acctName), CacheGlobal = true, Filterable = true, IsKey = true)]
		[PXRestrictor(typeof(Where<RQBiddingVendor.reqNbr, IsNotNull>), Messages.VendorNotInBidding)]
		protected virtual void RQBiddingVendor_VendorID_CacheAttached(PXCache sender)
		{
		}
		#endregion
		#region VendorLocationID
		[LocationID(typeof(Where<Location.bAccountID, Equal<Current<RQBiddingVendor.vendorID>>>), DescriptionField = typeof(Location.descr), Visibility = PXUIVisibility.SelectorVisible, IsKey = true)]			
		protected virtual void RQBiddingVendor_VendorLocationID_CacheAttached(PXCache sender)
		{			
		}
		#endregion		
		#endregion
		
		#endregion

		public PXSelectJoin<RQBiddingVendor,
			InnerJoin<RQRequisition, On<RQRequisition.reqNbr, Equal<RQBiddingVendor.reqNbr>>,
			 LeftJoinSingleTable<Vendor, On<Vendor.bAccountID, Equal<RQBiddingVendor.vendorID>>,
			 LeftJoinSingleTable<Customer, On<Customer.bAccountID, Equal<RQRequisition.customerID>>>>>,			
			Where<RQRequisition.status, Equal<RQRequisitionStatus.bidding>,
			And2<Where<Vendor.bAccountID, IsNull, 
			       Or<Match<Vendor,Current<AccessInfo.userName>>>>,
			And<Where<Customer.bAccountID, IsNull, 
			       Or<Match<Customer,Current<AccessInfo.userName>>>>>>>> Vendor;

		public PXSetup<Vendor,
		 Where<Vendor.bAccountID, Equal<Current<RQBiddingVendor.vendorID>>>> BVendor;
		
		public PXSelect<RQBiddingVendor,
			Where<RQBiddingVendor.lineID, Equal<Current<RQBiddingVendor.lineID>>>> CurrentDocument;

		public PXSelect<RQRequisitionLineBidding,			
			Where<RQRequisitionLineBidding.reqNbr, Equal<Current<RQBiddingVendor.reqNbr>>>> Lines;

		public PXSelect<RQBidding,
			Where<RQBidding.reqNbr, Equal<Current<RQBiddingVendor.reqNbr>>,
			And<RQBidding.vendorID, Equal<Current<RQBiddingVendor.vendorID>>,
			And<RQBidding.vendorLocationID, Equal<Current<RQBiddingVendor.vendorLocationID>>>>>> Bidding;

		public PXSelect<PORemitAddress, Where<PORemitAddress.addressID, Equal<Current<RQBiddingVendor.remitAddressID>>>> Remit_Address;
		public PXSelect<PORemitContact, Where<PORemitContact.contactID, Equal<Current<RQBiddingVendor.remitContactID>>>> Remit_Contact;
		
		public PXSelect<RQRequisitionLine> rqline;
		public PXSelect<RQRequisition> rq;
		public PXSelect<RQRequestLine> reqline;
		public PXSelect<RQRequest> req;
		public CMSetupSelect cmsetup;
		public PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Current<RQBiddingVendor.curyInfoID>>>> currencyinfo;
		public ToggleCurrency<RQBiddingVendor> CurrencyView;

		public RQBiddingEntry()
		{			
			this.Lines.Cache.AllowInsert = this.Lines.Cache.AllowDelete = false;
			PXUIFieldAttribute.SetEnabled(this.Lines.Cache, null, false);
			PXUIFieldAttribute.SetEnabled<RQRequisitionLineBidding.minQty>(this.Lines.Cache, null, true);
			PXUIFieldAttribute.SetEnabled<RQRequisitionLineBidding.quoteNumber>(this.Lines.Cache, null, true);
			PXUIFieldAttribute.SetEnabled<RQRequisitionLineBidding.quoteQty>(this.Lines.Cache, null, true);
			PXUIFieldAttribute.SetEnabled<RQRequisitionLineBidding.curyQuoteUnitCost>(this.Lines.Cache, null, true);
		}

		protected virtual IEnumerable lines()
		{
			if (Vendor.Current == null || Vendor.Current.VendorLocationID == null)
				yield break;

			using (ReadOnlyScope scope = new ReadOnlyScope(Lines.Cache, Bidding.Cache, rqline.Cache, rq.Cache))
			{
				PXResultset<RQRequisitionLineBidding> list =
					PXSelectJoin<RQRequisitionLineBidding,
						LeftJoin<RQBidding,
									On<RQBidding.reqNbr, Equal<RQRequisitionLineBidding.reqNbr>,
								 And<RQBidding.lineNbr, Equal<RQRequisitionLineBidding.lineNbr>,
								And<RQBidding.vendorID, Equal<Current<RQBiddingVendor.vendorID>>,
								And<RQBidding.vendorLocationID, Equal<Current<RQBiddingVendor.vendorLocationID>>>>>>>,
						Where<RQRequisitionLineBidding.reqNbr, Equal<Current<RQBiddingVendor.reqNbr>>>>
						.Select(this);

				foreach (PXResult<RQRequisitionLineBidding, RQBidding> item in list)
				{
					RQRequisitionLineBidding result = PrepareRQRequisitionLineBiddingInViewDelegate(item);
                    yield return result;
				}
			}
		}

		/// <summary>
		/// Prepare <see cref="RQRequisitionLineBidding"/> in view delegate. This is an extension point used by Lexware PriceUnit customization.
		/// </summary>
		protected virtual RQRequisitionLineBidding PrepareRQRequisitionLineBiddingInViewDelegate(PXResult<RQRequisitionLineBidding, RQBidding> item)
		{
			RQRequisitionLineBidding rqLineBidding = item;
			if (Lines.Cache.GetStatus(rqLineBidding) != PXEntryStatus.Updated)
			{
				RQBidding bidding = item;
				FillRequisitionLineBiddingPropertiesInViewDelegate(rqLineBidding, bidding);

				if (bidding.LineID == null)
					Lines.Cache.Update(rqLineBidding);
				else
					Lines.Cache.MarkUpdated(rqLineBidding);
			}
			return rqLineBidding;
		}

        /// <summary>
        /// Fill <see cref="RQRequisitionLineBidding"/> properties from <see cref="RQBidding"/> in view delegate. This is an extension point used by Lexware PriceUnit customization.
        /// </summary>
        /// <param name="rqLineBidding">The line bidding.</param>
        /// <param name="bidding">The bidding.</param>
        protected virtual void FillRequisitionLineBiddingPropertiesInViewDelegate(RQRequisitionLineBidding rqLineBidding, RQBidding bidding)
        {
            rqLineBidding.QuoteNumber = bidding.QuoteNumber;
            rqLineBidding.QuoteQty = bidding.QuoteQty ?? 0m;
            rqLineBidding.CuryInfoID = Vendor.Current.CuryInfoID;
            rqLineBidding.CuryQuoteUnitCost = bidding.CuryQuoteUnitCost ?? 0m;
            rqLineBidding.QuoteUnitCost = bidding.QuoteUnitCost ?? 0m;
            rqLineBidding.CuryQuoteExtCost = bidding.CuryQuoteExtCost ?? 0m;
            rqLineBidding.QuoteExtCost = bidding.QuoteExtCost ?? 0m;
            rqLineBidding.MinQty = bidding.MinQty ?? 0m;

            if (bidding.CuryQuoteUnitCost == null && rqLineBidding.InventoryID != null)
            {
                string bidVendorCuryID = (string) Vendor.GetValueExt<RQBiddingVendor.curyID>(Vendor.Current);

                POItemCostManager.ItemCost cost = POItemCostManager.Fetch(this, Vendor.Current.VendorID, Vendor.Current.VendorLocationID, 
                                                                          docDate: null, 
                                                                          curyID: bidVendorCuryID,
                                                                          inventoryID: rqLineBidding.InventoryID,
                                                                          subItemID: rqLineBidding.SubItemID, 
                                                                          siteID: null, 
                                                                          uom: rqLineBidding.UOM);
                rqLineBidding.CuryQuoteUnitCost =
                    cost.Convert<RQRequisitionLineBidding.inventoryID, RQRequisitionLineBidding.curyInfoID>(this, rqLineBidding, rqLineBidding.UOM);
            }

            if (rqLineBidding.CuryQuoteUnitCost == null)
            {
                rqLineBidding.CuryQuoteUnitCost = 0m;
            }
        }
		
		protected virtual void RQRequisitionLineBidding_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			e.Cancel = true;
		}

		public override void Persist()
		{
			base.Persist();
			Lines.Cache.Clear();
		}

		protected virtual void RQRequisitionLineBidding_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			this.Vendor.Cache.MarkUpdated(this.Vendor.Current);

			RQRequisitionLineBidding newRow = (RQRequisitionLineBidding)e.Row;
			RQRequisitionLineBidding oldRow = (RQRequisitionLineBidding)e.OldRow;

			if (newRow.MinQty != oldRow.MinQty || newRow.QuoteUnitCost != oldRow.QuoteUnitCost ||
				newRow.QuoteQty != oldRow.QuoteQty || newRow.QuoteNumber != oldRow.QuoteNumber)
			{
				RQBidding bidding = GetRQBiddingOnRequisitionLineBiddingRowUpdatedEvent(newRow);
				Bidding.Update(bidding);
			}
		}

		protected virtual RQBidding GetRQBiddingOnRequisitionLineBiddingRowUpdatedEvent(RQRequisitionLineBidding updatedRQLineBidding)
		{
			RQBidding bidding =
					PXSelect<RQBidding,
					Where<RQBidding.reqNbr, Equal<Required<RQBidding.reqNbr>>,
						And<RQBidding.lineNbr, Equal<Required<RQBidding.lineNbr>>,
						And<RQBidding.vendorID, Equal<Required<RQBidding.vendorID>>,
						And<RQBidding.vendorLocationID, Equal<Required<RQBidding.vendorLocationID>>>>>>>.SelectWindowed(
						this, 0, 1,
					Vendor.Current.ReqNbr,
					updatedRQLineBidding.LineNbr,
					Vendor.Current.VendorID,
					Vendor.Current.VendorLocationID);

			if (bidding == null)
			{
				bidding = new RQBidding
				{
					VendorID = Vendor.Current.VendorID,
					VendorLocationID = Vendor.Current.VendorLocationID,
					ReqNbr = Vendor.Current.ReqNbr,
					CuryInfoID = Vendor.Current.CuryInfoID,
					LineNbr = updatedRQLineBidding.LineNbr
				};
				bidding = Bidding.Insert(bidding);
			}

			bidding = (RQBidding)Bidding.Cache.CreateCopy(bidding);

			bidding.QuoteQty = updatedRQLineBidding.QuoteQty;
			bidding.QuoteNumber = updatedRQLineBidding.QuoteNumber;
			bidding.QuoteUnitCost = updatedRQLineBidding.QuoteUnitCost;
			bidding.CuryQuoteUnitCost = updatedRQLineBidding.CuryQuoteUnitCost;
			bidding.MinQty = updatedRQLineBidding.MinQty;

			return bidding;
		}

        protected virtual void RQBiddingVendor_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			RQBiddingVendor row = (RQBiddingVendor)e.Row;

			if (row == null)
                return;

			if(row.EntryDate == null)
				row.EntryDate = sender.Graph.Accessinfo.BusinessDate;

			if (BVendor.Current != null && BVendor.Current.AllowOverrideCury == true)
			{
				RQBidding bidding = (RQBidding)this.Bidding.View.SelectSingleBound(new object[] { e.Row });
				PXUIFieldAttribute.SetEnabled<RQBiddingVendor.curyID>(sender, e.Row, bidding == null);
			}
			else
				PXUIFieldAttribute.SetEnabled<RQBiddingVendor.curyID>(sender, e.Row, false);

			PXUIFieldAttribute.SetEnabled<RQBiddingVendor.curyTotalQuoteExtCost>(sender, e.Row, false);
			PXUIFieldAttribute.SetEnabled<RQBiddingVendor.totalQuoteExtCost>(sender, e.Row, false);
			PXUIFieldAttribute.SetEnabled<RQBiddingVendor.totalQuoteQty>(sender, e.Row, false);
			
			PORemitAddress remitAddress = this.Remit_Address.Select();
			bool enableAddressValidation = (remitAddress != null && remitAddress.IsDefaultAddress == false && remitAddress.IsValidated == false);
			this.validateAddresses.SetEnabled(enableAddressValidation);
		}
		
		protected virtual void RQBiddingVendor_RowInserting(PXCache sender, PXRowInsertingEventArgs e)
		{
			RQBiddingVendor row = (RQBiddingVendor)e.Row;

			if (row != null)
			{		
				PORemitAddressAttribute.DefaultRecord<RQBiddingVendor.remitAddressID>(sender, e.Row);
				PORemitContactAttribute.DefaultRecord<RQBiddingVendor.remitContactID>(sender, e.Row);					
			}

			BVendor.Current = (Vendor)BVendor.View.SelectSingleBound(new object[] { e.Row });
		}

		protected virtual void RQBiddingVendor_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			RQBiddingVendor row = (RQBiddingVendor)e.Row;

			if (row != null)
			{
				if (PXAccess.FeatureInstalled<FeaturesSet.multicurrency>() && row.ReqNbr != null && row.VendorID != null)
				{
					BVendor.Current = (Vendor) BVendor.View.SelectSingleBound(new object[] {e.Row});
					CurrencyInfo info = CurrencyInfoAttribute.SetDefaults<RQBiddingVendor.curyInfoID>(sender, e.Row);
					sender.SetDefaultExt<RQBiddingVendor.curyID>(e.Row);
				}
			}
		}

		#region CurrencyInfo events
		protected virtual void CurrencyInfo_CuryID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (PXAccess.FeatureInstalled<FeaturesSet.multicurrency>())
			{
				Vendor v = BVendor.Current;
				if (v != null && !string.IsNullOrEmpty(v.CuryID))
				{
					e.NewValue = v.CuryID;
					e.Cancel = true;
				}
			}
		}

		protected virtual void CurrencyInfo_CuryRateTypeID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (PXAccess.FeatureInstalled<FeaturesSet.multicurrency>())
			{
				Vendor v = BVendor.Current;
				if (v != null)
				{
					e.NewValue = v.CuryRateTypeID ?? cmsetup.Current.APRateTypeDflt;
					e.Cancel = true;
				}
			}
		}

		protected virtual void CurrencyInfo_CuryEffDate_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (Vendor.Cache.Current != null)
			{
				e.NewValue = ((RQBiddingVendor)Vendor.Cache.Current).EntryDate;
				e.Cancel = true;
			}
		}

		protected virtual void CurrencyInfo_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			CurrencyInfo info = e.Row as CurrencyInfo;
			if (info != null)
			{
				bool curyenabled = info.IsReadOnly != true;
				bool curyrateenabled = info.AllowUpdate(this.Bidding.Cache);

				if (BVendor.Current != null)
				{
					if (BVendor.Current.AllowOverrideCury != true)
						curyenabled = false;

					if (BVendor.Current.AllowOverrideRate != true)
						curyrateenabled = false;
				}
				
				PXUIFieldAttribute.SetEnabled<CurrencyInfo.curyID>(sender, info, curyenabled);
				PXUIFieldAttribute.SetEnabled<CurrencyInfo.curyRateTypeID>(sender, info, curyrateenabled);
				PXUIFieldAttribute.SetEnabled<CurrencyInfo.curyEffDate>(sender, info, curyrateenabled);
				PXUIFieldAttribute.SetEnabled<CurrencyInfo.sampleCuryRate>(sender, info, curyrateenabled);
				PXUIFieldAttribute.SetEnabled<CurrencyInfo.sampleRecipRate>(sender, info, curyrateenabled);
			}
		}
		#endregion
		[PXCancelButton]
		[PXUIField(DisplayName = ActionsMessages.Cancel, MapEnableRights = PXCacheRights.Select)]
		protected virtual IEnumerable Cancel(PXAdapter a)
		{
			if (a.Searches != null && a.Searches.Length == 3)
			{
				if (this.Vendor.Current != null && this.Vendor.Current.ReqNbr != (string)a.Searches[0])
				{
					PXResult<RQBiddingVendor,Vendor, Location> res = (PXResult<RQBiddingVendor,Vendor, Location>)
						PXSelectJoin<RQBiddingVendor,						
						InnerJoin<Vendor, On<Vendor.bAccountID, Equal<RQBiddingVendor.vendorID>>,
						InnerJoin<Location, On<Location.bAccountID, Equal<Vendor.bAccountID>, And<Location.locationID, Equal<Vendor.defLocationID>>>>>,
						Where<RQBiddingVendor.reqNbr, Equal<Required<RQBiddingVendor.reqNbr>>,
							And<Match<Vendor,Current<AccessInfo.userName>>>>>
						.Select(this, (string)a.Searches[0]);

					Vendor vendor = null;  
					Location location = null;
					if (res != null)
					{
						vendor = res;
						location = res;
					}

					if (vendor == null || vendor.BAccountID == null)
					{
						a.Searches[1] = null;
						a.Searches[2] = null;
					}
					else
					{
						a.Searches[1] = vendor.AcctCD;
						a.Searches[2] = location.LocationCD;
					}
				}
				else
				{
					if (a.Searches[1] != null && a.Searches[2] != null)
					{
						Location loc = PXSelectJoin<Location,
							InnerJoin<Vendor,
										 On<Vendor.bAccountID, Equal<Location.bAccountID>>>,
							Where<Vendor.acctCD, Equal<Required<Vendor.acctCD>>,
							  And<Location.locationCD, Equal<Required<Location.locationCD>>,
								And<Match<Vendor, Current<AccessInfo.userName>>>>>>
								.SelectWindowed(this, 0, 1, a.Searches[1], a.Searches[2]);

						if(loc == null)
							a.Searches[2] = null;						
					}

					if (a.Searches[1] != null && a.Searches[2] == null)
					{					
							Location loc = PXSelectJoin<Location,
								InnerJoin<Vendor, 
											 On<Vendor.bAccountID, Equal<Location.bAccountID>, And<Vendor.defLocationID, Equal<Location.locationID>>>>,
								Where<Vendor.acctCD,Equal<Required<Vendor.acctCD>>,
									And<Match<Vendor,Current<AccessInfo.userName>>>>>
									.SelectWindowed(this, 0, 1, a.Searches[1]);

							if (loc != null)
								a.Searches[2] = loc.LocationCD;					
					}	
				}
				
			}

			foreach (object e in (new PXCancel<RQBiddingVendor>(this, "Cancel")).Press(a))							
				yield return e;			
		}		

		public class PXBiddingInsert<TNode> : PXInsert<TNode>
		where TNode : RQBiddingVendor, new()
		{
			public PXBiddingInsert(PXGraph graph, string name) : base(graph, name)
			{
			}

			[PXInsertButton]
			[PXUIField(DisplayName = "Insert", MapEnableRights = PXCacheRights.Insert, MapViewRights = PXCacheRights.Insert)]
			protected override IEnumerable Handler(PXAdapter adapter)
			{
				List<object> items = new List<object>();
                
				foreach(object item in base.Handler(adapter))
				{
					RQBiddingVendor b = item is PXResult 
                        ? (RQBiddingVendor)((PXResult)item)[0] 
                        : (RQBiddingVendor)item;

					b.VendorID = null;
					b.VendorLocationID = null;					
					items.Add(item);

					foreach (Type type in _Graph.Caches.Keys)
						_Graph.Caches[type].IsDirty = false;					
				}

				return items;
			}
		}
	}
}
