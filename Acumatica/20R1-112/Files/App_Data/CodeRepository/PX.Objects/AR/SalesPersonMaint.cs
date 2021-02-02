using System;
using System.Collections.Generic;
using System.Collections;
using PX.Data;
using PX.Objects.CR;

namespace PX.Objects.AR
{
	public class SalesPersonMaint : PXGraph<SalesPersonMaint, SalesPerson>
	{
		#region Ctor+ Public Members
		public SalesPersonMaint() 
		{
			ARSetup setup = ARSetup.Current;
			this.CommissionsHistory.Cache.AllowInsert = false;
			this.CommissionsHistory.Cache.AllowDelete= false;
			this.CommissionsHistory.Cache.AllowUpdate = false;
			PXUIFieldAttribute.SetEnabled<CustSalesPeople.locationID>(this.SPCustomers.Cache, null, false);

			PXUIFieldAttribute.SetDisplayName<Contact.salutation>(Caches[typeof(Contact)], CR.Messages.Attention);
		}


		public PXSelect<SalesPerson> Salesperson;

		public PXSelect<SalesPerson, Where<SalesPerson.salesPersonID, Equal<Current<SalesPerson.salesPersonID>>>> SalespersonCurrent;
		public PXSelectJoin<CustSalesPeople,
			InnerJoinSingleTable<Customer, On<Customer.bAccountID, Equal<CustSalesPeople.bAccountID>>>,
			Where<CustSalesPeople.salesPersonID, Equal<Current<SalesPerson.salesPersonID>>,
			And<Match<Customer, Current<AccessInfo.userName>>>>> SPCustomers;
		public PXSelectGroupBy<ARSPCommnHistory, Where<ARSPCommnHistory.salesPersonID, Equal<Current<SalesPerson.salesPersonID>>>,
							   Aggregate<Sum<ARSPCommnHistory.commnAmt, 
										 Sum<ARSPCommnHistory.commnblAmt,
										 GroupBy<ARSPCommnHistory.commnPeriod, Max<ARSPCommnHistory.pRProcessedDate>>>>>> CommissionsHistory;

		public PXSetup<ARSetup> ARSetup;
		#endregion
		#region Sub-screen Navigation Buttons
		public PXAction<SalesPerson> viewDetails;
		[PXUIField(DisplayName = "View Details", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.DataEntry)]
		public virtual IEnumerable ViewDetails(PXAdapter adapter)
		{
			if (this.CommissionsHistory.Current != null)
			{
				ARSPCommnHistory current = this.CommissionsHistory.Current;
				ARSPCommissionDocEnq graph = PXGraph.CreateInstance<ARSPCommissionDocEnq>();
				SPDocFilter filter = graph.Filter.Current;
				filter.SalesPersonID = current.SalesPersonID;
				filter.CommnPeriod = current.CommnPeriod;
				graph.Filter.Update(filter);
				throw new PXRedirectRequiredException(graph, "Document");
			}
			return adapter.Get();
		}
		#endregion
		#region SalesPerson Events
		protected virtual void SalesPerson_RowDeleting(PXCache cache, PXRowDeletingEventArgs e)
		{
			SalesPerson row = (SalesPerson)e.Row;
			PXSelectBase<ARSalesPerTran> sel = new PXSelect<ARSalesPerTran, Where<ARSalesPerTran.salespersonID, Equal<Required<ARSalesPerTran.salespersonID>>>>(this);
			ARSalesPerTran tran = (ARSalesPerTran)sel.View.SelectSingle(row.SalesPersonID);
			if (tran != null)
			{
				throw new PXException(Messages.SalesPersonWithHistoryMayNotBeDeleted);
			}
		}
		#endregion	
        #region CustSalesPeople Events
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXCustomizeBaseAttribute(typeof(PXUIFieldAttribute), nameof(PXUIFieldAttribute.Enabled), false)]
		protected virtual void CustSalesPeople_IsDefault_CacheAttached(PXCache cache)
		{ }

        protected virtual void CustSalesPeople_RowInserting(PXCache cache, PXRowInsertingEventArgs e) 
		{
			CustSalesPeople row = (CustSalesPeople)e.Row;
			if (row != null && row.BAccountID.HasValue)
			{
				List<CustSalesPeople> current = new List<CustSalesPeople>();
				bool duplicated = false;
				foreach (CustSalesPeople iSP in this.SPCustomers.Select())
				{
					if (row.BAccountID == iSP.BAccountID)
					{
						current.Add(iSP);
						if (row.LocationID == iSP.LocationID)
							duplicated = true;
					}
				}
				if (duplicated)
				{
					Location freeLocation = null;
					PXSelectBase<Location> sel = new PXSelect<Location, Where<Location.bAccountID, Equal<Required<Location.bAccountID>>>>(this);
					foreach (Location iLoc in sel.Select(row.BAccountID))
					{
						bool found = current.Exists(new Predicate<CustSalesPeople>(delegate(CustSalesPeople op) { return (op.LocationID == iLoc.LocationID); }));
						if (!found)
						{
							freeLocation = iLoc;
							break;
						}
					}
					if (freeLocation != null)
					{
						row.LocationID = freeLocation.LocationID;
					}
					else
					{
						throw new PXException(Messages.AllCustomerLocationsAreAdded);
					}
				}
			}
		}

		#endregion
	}
}
