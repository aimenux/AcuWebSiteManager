using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Data;
using PX.Objects.CS;
using PX.Objects.AP;
using System.Collections;
using PX.Objects.PO;
using PX.Objects.CM;
using PX.Objects.CR;
using PX.TM;
using PX.Objects.AR;

namespace PX.Objects.RQ
{
	
    [Serializable]
	public partial class RQBiddingState : IBqlTable
	{
		#region SingleMode
		public abstract class singleMode : PX.Data.BQL.BqlBool.Field<singleMode> { }
		protected bool? _SingleMode = false;
		[PXBool]
		[PXDefault(false)]
		public virtual bool? SingleMode
		{
			get
			{
				return _SingleMode;
			}
			set
			{
				_SingleMode = value;
			}
		}
		#endregion
		#region ReqNbr
		public abstract class reqNbr : PX.Data.BQL.BqlString.Field<reqNbr> { }
		protected String _ReqNbr;
		[PXDBString(15, IsUnicode = true)]		
		public virtual String ReqNbr
		{
			get
			{
				return this._ReqNbr;
			}
			set
			{
				this._ReqNbr = value;
			}
		}
		#endregion
		#region LineNbr
		public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }
		protected int? _LineNbr;
		[PXInt]		
		public virtual int? LineNbr
		{
			get
			{
				return this._LineNbr;
			}
			set
			{
				this._LineNbr = value;
			}
		}
		#endregion
	}

	[PX.Objects.GL.TableAndChartDashboardType]
	public class RQBiddingProcess : PXGraph<RQBiddingProcess>
	{
		public PXSave<RQRequisition> Save;
		public PXCancel<RQRequisition> Cancel;
		public PXFirst<RQRequisition> First;
		public PXPrevious<RQRequisition> Previous;
		public PXNext<RQRequisition> Next;
		public PXLast<RQRequisition> Last;

		public PXFilter<RQBiddingState> State;

		public PXSelectJoin<RQRequisition,
			LeftJoinSingleTable<Customer, 
				On<Customer.bAccountID, Equal<RQRequisition.customerID>>,
			LeftJoinSingleTable<Vendor, 
				On<Vendor.bAccountID, Equal<RQRequisition.vendorID>>>>,
			Where2<
				Where<Customer.bAccountID, IsNull,
					Or<Match<Customer, Current<AccessInfo.userName>>>>,
				And2<
					Where<Vendor.bAccountID, IsNull,
						Or<Match<Vendor, Current<AccessInfo.userName>>>>,
					And<Where<RQRequisition.status, Equal<RQRequisitionStatus.bidding>,
						Or<RQRequisition.status, Equal<RQRequisitionStatus.closed>,
						Or<RQRequisition.status, Equal<RQRequisitionStatus.open>,
						Or<RQRequisition.status, Equal<RQRequisitionStatus.pendingQuotation>,
						Or<RQRequisition.status, Equal<RQRequisitionStatus.released>>>>>>>>>> Document;
		
		public PXSelect<Vendor,
				 Where<Vendor.bAccountID, Equal<Current<RQBiddingVendor.vendorID>>>> Vendor;

		public PXSelectJoin<RQBiddingVendor,
			       LeftJoin<Location, 
				         On<Location.bAccountID, Equal<RQBiddingVendor.vendorID>,
				        And<Location.locationID, Equal<RQBiddingVendor.vendorLocationID>>>>,
			          Where<RQBiddingVendor.reqNbr, Equal<Optional<RQRequisition.reqNbr>>>> Vendors;

		public PXSelect<RQBiddingVendor,
			      Where<RQBiddingVendor.reqNbr, Equal<Required<RQRequisition.reqNbr>>,
				    And<RQBiddingVendor.vendorID, Equal<Required<RQBiddingVendor.vendorID>>,
				    And<RQBiddingVendor.vendorLocationID, Equal<Required<RQBiddingVendor.vendorLocationID>>>>>> ChoosenVendor;

		public PXSelect<PORemitAddress, Where<PORemitAddress.addressID, Equal<Current<RQBiddingVendor.remitAddressID>>>> Remit_Address;
		public PXSelect<PORemitContact, Where<PORemitContact.contactID, Equal<Current<RQBiddingVendor.remitContactID>>>> Remit_Contact;

		public PXSelect<RQRequisitionLine, Where<RQRequisitionLine.reqNbr, Equal<Optional<RQRequisition.reqNbr>>>> Lines;

		public PXSelectJoin<RQBidding,
			    LeftJoin<RQBiddingVendor,
				    On<RQBiddingVendor.reqNbr, Equal<RQBidding.reqNbr>,
				    And<RQBiddingVendor.vendorID, Equal<RQBidding.vendorID>,
				    And<RQBiddingVendor.vendorLocationID, Equal<RQBidding.vendorLocationID>>>>,
			    LeftJoin<Location, 
				    On<Location.bAccountID, Equal<RQBidding.vendorID>,
				    And<Location.locationID, Equal<RQBidding.vendorLocationID>>>>>,
			    Where<RQBidding.reqNbr, Equal<Argument<string>>,
				    And<RQBidding.lineNbr, Equal<Argument<int?>>>>> 
			Bidding;

		public PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Optional<RQBiddingVendor.curyInfoID>>>> currencyinfo;

		public ToggleCurrency<RQRequisition> CurrencyView;
		public CMSetupSelect cmsetup;

		#region Redefine attributes
		[PXDBString(15, IsUnicode = true, IsKey = true, InputMask = ">CCCCCCCCCCCCCCC")]
		[PXDefault()]
		[PXUIField(DisplayName = "Ref. Nbr.", Visibility = PXUIVisibility.SelectorVisible)]
		[AutoNumber(typeof(RQSetup.requisitionNumberingID), typeof(RQRequisition.orderDate))]
		[PXSelectorAttribute(
			typeof(Search<RQRequisition.reqNbr,
				    Where<RQRequisition.status, Equal<RQRequisitionStatus.bidding>,
					   Or<RQRequisition.status, Equal<RQRequisitionStatus.open>,
					   Or<RQRequisition.status, Equal<RQRequisitionStatus.closed>,
					   Or<RQRequisition.status, Equal<RQRequisitionStatus.pendingQuotation>,
					   Or<RQRequisition.status, Equal<RQRequisitionStatus.released>>>>>>>),
			typeof(RQRequisition.status),
			typeof(RQRequisition.priority),
			typeof(RQRequisition.orderDate),
			Filterable = true)]
		protected virtual void RQRequisition_ReqNbr_CacheAttached(PXCache sender)
		{			
		}

		[PXDBDate()]
		[PXUIField(DisplayName = "Entry Date", Visibility = PXUIVisibility.SelectorVisible)]
		protected virtual void RQBiddingVendor_EntryDate_CacheAttached(PXCache sender)
		{
		}
		#endregion

		public virtual IEnumerable bidding(
			[PXDBString]
			string reqNbr,
			[PXDBInt]
			int? lineNbr)
		{
			State.Current.ReqNbr = reqNbr ?? (this.Lines.Current != null ? this.Lines.Current.ReqNbr : null);
			State.Current.LineNbr = lineNbr ?? (this.Lines.Current != null ? this.Lines.Current.LineNbr : null);

			foreach (PXResult<RQBidding, RQBiddingVendor, Location> item in 
			            PXSelectJoin<RQBidding,
				            LeftJoin<RQBiddingVendor, 
					            On<RQBiddingVendor.reqNbr, Equal<RQBidding.reqNbr>,
					            And<RQBiddingVendor.vendorID, Equal<RQBidding.vendorID>,
					            And<RQBiddingVendor.vendorLocationID, Equal<RQBidding.vendorLocationID>>>>,
				            LeftJoin<Location, 
					            On<Location.bAccountID, Equal<RQBidding.vendorID>,
					            And<Location.locationID, Equal<RQBidding.vendorLocationID>>>>>,
				            Where<RQBidding.reqNbr, Equal<Required<RQRequisitionLine.reqNbr>>,
					            And<RQBidding.lineNbr, Equal<Required<RQRequisitionLine.lineNbr>>>>>
				            .Select(this, State.Current.ReqNbr, State.Current.LineNbr))
			{
				RQBidding bidding = item;				
				bidding.Selected =
					bidding.OrderQty > 0 || (bidding.VendorID == Document.Current.VendorID && bidding.VendorLocationID == Document.Current.VendorLocationID);

				yield return item;
			}
		}

		public PXAction<RQRequisition> ChooseVendor;
		[PXButton]
		[PXUIField(DisplayName = Messages.ChooseVendor)]
		public virtual IEnumerable chooseVendor(PXAdapter adapter)
		{
				RQBiddingVendor vendor = PXCache<RQBiddingVendor>.CreateCopy(this.Vendors.Current);

				foreach (RQRequisition item in adapter.Get<RQRequisition>())
				{
					if (vendor != null)
					{
						bool errors = false;

						foreach (RQRequisitionLine line in this.Lines.View.SelectMultiBound(new object[] { item }))
						{
							RQRequisitionLine l = (RQRequisitionLine)this.Lines.Cache.CreateCopy(line);

							RQBidding bidding = PXSelect<RQBidding,
									Where<RQBidding.reqNbr, Equal<Current<RQRequisitionLine.reqNbr>>,
										And<RQBidding.lineNbr, Equal<Current<RQRequisitionLine.lineNbr>>,
										And<RQBidding.vendorID, Equal<Current<RQBiddingVendor.vendorID>>,
										And<RQBidding.vendorLocationID, Equal<Current<RQBiddingVendor.vendorLocationID>>>>>>>
									.SelectSingleBound(this, new object[] { l, vendor });

							if (bidding == null)
							{
								this.Lines.Cache.RaiseExceptionHandling<RQRequisitionLine.orderQty>(line, line.OrderQty, new PXException(Messages.BiddingEmpty));
								errors = true;
							}
                            else if (bidding.MinQty != 0 || bidding.QuoteQty != 0)
							{
							    
								if (bidding.MinQty > line.OrderQty)
								{
									this.Lines.Cache.RaiseExceptionHandling<RQRequisitionLine.orderQty>(line, line.OrderQty, new PXException(Messages.OrderQtyLessMinQty));
									errors = true;
								}

								if (bidding.QuoteQty < line.OrderQty)
								{
									this.Lines.Cache.RaiseExceptionHandling<RQRequisitionLine.orderQty>(line, line.OrderQty, new PXException(Messages.OrderQtyMoreQuoteQty));
									errors = true;
								}
							}														
						}

						if (errors)
							throw new PXException(Messages.ChooseVendorError);

						foreach (RQBidding bid in 
                               PXSelect<RQBidding,
						          Where<RQBidding.reqNbr, Equal<Required<RQBidding.reqNbr>>,
							        And<RQBidding.orderQty, Greater<Required<RQBidding.orderQty>>>>>
                                .Select(this, item.ReqNbr, 0m))
							{
								RQBidding upd = (RQBidding)this.Bidding.Cache.CreateCopy(bid);
								upd.OrderQty = 0;
								this.Bidding.Update(upd);
							}

						yield return DoChooseVendor(item, vendor);
					}
					else
						yield return item;
				}
		}

		protected virtual void CopyUnitCost(RQRequisitionLine line, RQBidding bidding)
		{
			if ((bidding.MinQty == 0 && bidding.QuoteQty == 0) ||
				(bidding.MinQty <= line.OrderQty && bidding.QuoteQty >= line.OrderQty))
			{
				RQRequisitionLine lineCopy = (RQRequisitionLine) Lines.Cache.CreateCopy(line);

				decimal unitCost;
				PXCurrencyAttribute.CuryConvCury<RQRequisitionLine.curyInfoID>(Lines.Cache, lineCopy, bidding.QuoteUnitCost ?? 0, out unitCost, true);
				lineCopy.CuryEstUnitCost = unitCost;

				Lines.Update(lineCopy);
			}
		}

		private RQRequisition DoChooseVendor(RQRequisition item, RQBiddingVendor vendor)
		{
			if (vendor.RemitContactID == null)
				PORemitContactAttribute.DefaultRecord<RQBiddingVendor.remitContactID>(Caches[typeof(RQBiddingVendor)], vendor);

			if (vendor.RemitAddressID == null)
				PORemitAddressAttribute.DefaultRecord<RQBiddingVendor.remitAddressID>(Caches[typeof(RQBiddingVendor)], vendor);

			RQRequisition upd = (RQRequisition)this.Document.Cache.CreateCopy(item);
			upd.VendorID = vendor.VendorID;
			upd.VendorLocationID = vendor.VendorLocationID;
			upd.RemitAddressID = vendor.RemitAddressID;
			upd.RemitContactID = vendor.RemitContactID;
			upd.ShipVia = vendor.ShipVia;
			upd.FOBPoint = vendor.FOBPoint;

			upd = PXCache <RQRequisition>.CreateCopy(this.Document.Update(upd));

			if (upd.CustomerID == null && PXAccess.FeatureInstalled<FeaturesSet.multicurrency>())
			{
				CurrencyInfo current = currencyinfo.SelectWindowed(0, 1,vendor.CuryInfoID);
				CurrencyInfo info = currencyinfo.SelectWindowed(0, 1, upd.CuryInfoID);

				info = PXCache<CurrencyInfo>.CreateCopy(info);				
				bool update = false;

				if (current.CuryID != info.CuryID)
				{
					info.CuryID = current.CuryID;
					update = true;
				}

				if (current.CuryRateTypeID != info.CuryRateTypeID)
				{
					info.CuryRateTypeID = current.CuryRateTypeID;
					update = true;
				}
				
				if (update)
				{
                    try
                    {
                        PXDBCurrencyAttribute.SetBaseCalc<RQRequisitionLine.curyEstUnitCost>(this.Lines.Cache, null, false);
                        currencyinfo.Update(info);
                        upd.CuryID = info.CuryID;
                    }
                    finally
                    {
                        PXDBCurrencyAttribute.SetBaseCalc<RQRequisitionLine.curyEstUnitCost>(this.Lines.Cache, null, true);
                    }

                    string message = PXUIFieldAttribute.GetError<CurrencyInfo.curyEffDate>(currencyinfo.Cache, info);

					if (string.IsNullOrEmpty(message) == false)
					{
						this.Document.Cache.RaiseExceptionHandling<RQRequisition.orderDate>(item, item.OrderDate,
						                                   new PXSetPropertyException(message, PXErrorLevel.Warning));
					}

					upd = this.Document.Update(upd);					
				}
			}

            foreach (RQRequisitionLine line in this.Lines.View.SelectMultiBound(new object[] { upd }))
            {
                RQBidding bidding = 
                       PXSelect<RQBidding,
                          Where<RQBidding.reqNbr, Equal<Current<RQRequisitionLine.reqNbr>>,
                            And<RQBidding.lineNbr, Equal<Current<RQRequisitionLine.lineNbr>>,
                            And<RQBidding.vendorID, Equal<Current<RQBiddingVendor.vendorID>>,
                            And<RQBidding.vendorLocationID, Equal<Current<RQBiddingVendor.vendorLocationID>>>>>>>
                        .SelectSingleBound(this, new object[] { line, vendor });

				if (bidding != null)
					CopyUnitCost(line, bidding);
            }

			return this.Document.Search<RQRequisition.reqNbr>(upd.ReqNbr);
		}


		public PXAction<RQRequisition> VendorInfo;

		[PXButton]
		[PXUIField(DisplayName = Messages.VendorInfo)]
		public virtual IEnumerable vendorInfo(PXAdapter adapter)
		{
			foreach (RQRequisition item in adapter.Get<RQRequisition>())
			{
				if (this.Vendors.Current != null)
				{
					this.Vendor.ClearDialog();
					this.Vendor.AskExt();
				}	
                
				yield return item;
			}
		}
	
		public PXAction<RQRequisition> Process;

		[PXProcessButton]
		[PXUIField(DisplayName = Messages.BiddingComplete)]
		public virtual IEnumerable process(PXAdapter adapter)
		{
			RQRequisitionEntry graph = PXGraph.CreateInstance<RQRequisitionEntry>();

			foreach (RQRequisition rec in adapter.Get<RQRequisition>())
			{
				RQRequisition item = rec;
				this.Document.Current = item;				
				bool hasError = false;

				if (item.Splittable == true)
				{
					if (item.VendorID == null || item.VendorLocationID == null)
					{
						int? vendorID = null, vendorLocationID = null;

						Dictionary<int?, List<RQBidding>> biddings = new Dictionary<int?, List<RQBidding>>();

						foreach (RQBidding b in 
                            PXSelect<RQBidding,
							   Where<RQBidding.reqNbr, Equal<Required<RQBidding.reqNbr>>,
								 And<RQBidding.orderQty, Greater<CS.decimal0>>>>.Select(this, item.ReqNbr))
						{
							List<RQBidding> linebidding;

							if (!biddings.TryGetValue(b.LineNbr, out linebidding))
							{
								biddings[b.LineNbr] = linebidding = new List<RQBidding>();
							}

							linebidding.Add(b);
						}

						foreach (RQRequisitionLine line in Lines.Select(item.ReqNbr))
						{
							if (line.OrderQty > line.BiddingQty)
							{
								Lines.Cache.RaiseExceptionHandling<RQRequisitionLine.biddingQty>(line, line.BiddingQty,
									                                              new PXSetPropertyException(Messages.LineBiddingNotComplete));
								hasError = true;
								vendorLocationID = -1;
								break;
							}

							List<RQBidding> linebidding;

							if (biddings.TryGetValue(line.LineNbr, out linebidding))
							{
								if (linebidding.Count == 1)
								{
									CopyUnitCost(line, linebidding[0]);
								}

                                foreach (RQBidding b in linebidding.Where(bidding => bidding.OrderQty > 0))
                                {
                                    if (vendorLocationID == null)
                                    {
                                        vendorID = b.VendorID;
                                        vendorLocationID = b.VendorLocationID;
                                    }

                                    if (vendorLocationID > 0 && vendorLocationID != b.VendorLocationID)
                                    {
                                        vendorLocationID = -1;
                                        break;
                                    }
                                }
							}
						}

						if (vendorLocationID > 0)
						{
							RQBiddingVendor vendor = this.ChoosenVendor.SelectWindowed(0, 1, item.ReqNbr, vendorID, vendorLocationID);

							if (vendor != null)
								DoChooseVendor(item, vendor);
						}
					}
				}
				else if (item.VendorID == null)
				{
					Document.Cache.RaiseExceptionHandling<RQRequisition.vendorID>(item, null, new PXSetPropertyException(Messages.VendorIsNull));
					hasError = true;
				}
				else if(item.VendorLocationID == null)
				{
					Document.Cache.RaiseExceptionHandling<RQRequisition.vendorLocationID>(item, null, 
                                                                                          new PXSetPropertyException(Messages.VendorLocationIsNull));
					hasError = true;
				}
				//this.Save.Press();
	
				if(!hasError)
				{
					RQRequisition source = PXCache<RQRequisition>.CreateCopy(graph.Document.Search<RQRequisition.reqNbr>(item.ReqNbr));
					RQRequisition upd = PXCache<RQRequisition>.CreateCopy(source);
					upd.BiddingComplete = true;

					graph.Document.Update(upd);

					upd = graph.Document.Search<RQRequisition.reqNbr>(upd.ReqNbr);					
					item = PXCache<RQRequisition>.CreateCopy(item);

					foreach (var field in graph.Document.Cache.Fields)
					{
						if(!object.Equals(graph.Document.Cache.GetValue(source, field), graph.Document.Cache.GetValue(upd,field)))
							this.Document.Cache.SetValue(item, field, graph.Document.Cache.GetValue(upd, field));
					}

					yield return this.Document.Cache.Update(item);					
				}
				else
					yield return item;
			}
			
		}	

		public PXAction<RQRequisition> UpdateResult;

		[PXButton(/*ImageKey="CheckOut" */)]
		[PXUIField(DisplayName = Messages.UpdateResult)]
		public virtual IEnumerable updateResult(PXAdapter adapter)
		{
			foreach (RQRequisition requisition in adapter.Get<RQRequisition>())
			{
				RQRequisition result = requisition;
				RQBiddingVendor choosenVendor = null;

                if (requisition.Splittable == true)
                {
                    if (requisition.VendorID == null || requisition.VendorLocationID == null)
                    {
                        foreach (RQRequisitionLine line in this.Lines.Select(requisition.ReqNbr))
                        {
                            if (line.OrderQty > line.BiddingQty)
                            {
                                decimal openQty = line.OrderQty.GetValueOrDefault() - line.BiddingQty.GetValueOrDefault();
                                List<RQBidding> linebidding = new List<RQBidding>();

                                foreach (RQBidding bid in 
                                    PXSelect<RQBidding,
                                    	Where<RQBidding.reqNbr, Equal<Required<RQBidding.reqNbr>>,
                                    		And<RQBidding.lineNbr, Equal<Required<RQBidding.lineNbr>>,
                                    		And<RQBidding.quoteUnitCost, Greater<decimal0>>>>,
                                    	OrderBy<
                                    		Asc<RQBidding.quoteUnitCost>>>
                                    	.Select(this, line.ReqNbr, line.LineNbr))
                                {
                                    decimal quoteQty = bid.QuoteQty == 0
                                        ? decimal.MaxValue
                                        : bid.QuoteQty.GetValueOrDefault() - bid.OrderQty.GetValueOrDefault();

                                    if (quoteQty > 0)
                                    {
                                        decimal qty = quoteQty > openQty ? openQty : quoteQty;

                                        if (qty >= bid.MinQty)
                                        {
                                            if (choosenVendor == null)
                                            {
                                                choosenVendor = new RQBiddingVendor()
                                                {
                                                    VendorID = bid.VendorID,
                                                    VendorLocationID = bid.VendorLocationID
                                                };
                                            }
                                            else if (choosenVendor.VendorID != bid.VendorID || choosenVendor.VendorLocationID != bid.VendorLocationID)
                                            {
                                                choosenVendor.VendorID = null;
                                                choosenVendor.VendorLocationID = null;
                                            }

                                            RQBidding upd = (RQBidding)this.Bidding.Cache.CreateCopy(bid);
                                            upd.OrderQty += qty;
                                            this.Bidding.Update(upd);

                                            linebidding.Add(bid);

                                            openQty -= qty;
                                        }
                                    }

                                    if (openQty <= 0)
                                        break;
                                }

                                if (linebidding.Count == 1)
                                {
                                    CopyUnitCost(line, linebidding[0]);
                                }

                                if (openQty > 0)
                                {
                                    Lines.Cache.RaiseExceptionHandling<RQRequisitionLine.biddingQty>(line, line.BiddingQty,
                                        new PXSetPropertyException(Messages.LineBiddingNotComplete, PXErrorLevel.Warning));
                                }
                            }
                        }
                    }

                    if (choosenVendor != null && choosenVendor.VendorID == null)
						choosenVendor = null;
				}
				else
				{
					decimal choosenTotal  = decimal.MaxValue;

					foreach (PXResult<RQBiddingRequisitionLine, RQBiddingVendor, RQBidding> rec in
							PXSelectJoinGroupBy<RQBiddingRequisitionLine,
								InnerJoin<RQBiddingVendor,
									On<RQBiddingVendor.reqNbr, Equal<RQBiddingRequisitionLine.reqNbr>>,
								LeftJoin<RQBidding,
									On<RQBidding.reqNbr, Equal<RQBiddingRequisitionLine.reqNbr>,
									And<RQBidding.lineNbr, Equal<RQBiddingRequisitionLine.lineNbr>,
									And<RQBidding.vendorID, Equal<RQBiddingVendor.vendorID>,
									And<RQBidding.vendorLocationID, Equal<RQBiddingVendor.vendorLocationID>,
									And<RQBidding.minQty, LessEqual<RQBiddingRequisitionLine.orderQty>,
									And<Where<RQBidding.quoteQty, Equal<CS.decimal0>,
										Or<RQBidding.quoteQty, GreaterEqual<RQBiddingRequisitionLine.orderQty>>>>>>>>>>>,
								Where<RQRequisitionLine.reqNbr, Equal<Required<RQRequisitionLine.reqNbr>>>,
								Aggregate<
									GroupBy<RQBiddingVendor.vendorID,
									GroupBy<RQBiddingVendor.vendorLocationID,									
										Sum<RQBiddingRequisitionLine.quoteCost,
										Sum<RQBiddingRequisitionLine.lineNbr,
										Sum<RQBidding.lineNbr>>>>>>>
								.Select(this, requisition.ReqNbr))
						{
							RQBiddingRequisitionLine line = rec;
							RQBiddingVendor vendor = rec;
							RQBidding bidding = rec;

							if(line.QuoteCost == null || line.LineNbr != bidding.LineNbr || line.QuoteCost > choosenTotal ) 
								continue;

							choosenTotal = line.QuoteCost.Value;
							choosenVendor = vendor;
						}				
				}

				if (choosenVendor != null)
					result = DoChooseVendor(result, choosenVendor);	
                
				yield return result;
			}
		}

		public PXAction<RQRequisition> ClearResult;
		[PXButton(/*ImageKey = PX.Web.UI.Sprite.Main.UndoCheckOut*/)]
		[PXUIField(DisplayName = Messages.ClearResult)]
		public virtual IEnumerable clearResult(PXAdapter adapter)
		{
			foreach (RQRequisition item in adapter.Get<RQRequisition>())
			{
				RQRequisition result = item;

				foreach (RQBidding bid in 
                    PXSelect<RQBidding,
                       Where<RQBidding.reqNbr, Equal<Required<RQBidding.reqNbr>>,
                    	 And<RQBidding.orderQty, Greater<Required<RQBidding.orderQty>>>>>
                    .Select(this, item.ReqNbr, 0m))
				{
					RQBidding upd = (RQBidding)this.Bidding.Cache.CreateCopy(bid);
					upd.OrderQty = 0;
					this.Bidding.Update(upd);
				}

				if (item.VendorLocationID != null)
				{
					RQRequisition upd = (RQRequisition)this.Document.Cache.CreateCopy(item);
					upd.VendorID = null;
					upd.VendorLocationID = null;
					upd.RemitAddressID = null;
					upd.RemitContactID = null;
					upd.ShipVia = null;
					upd.FOBPoint = null;
					result = this.Document.Update(upd);					
				}

				yield return result;
			}
		}

		public RQBiddingProcess()
		{			
			this.Document.Cache.AllowDelete = false;
			
			PXUIFieldAttribute.SetEnabled(this.Document.Cache, null, false);
			PXUIFieldAttribute.SetEnabled<RQRequisition.reqNbr>(this.Document.Cache, null, true);
			PXUIFieldAttribute.SetEnabled<RQRequisition.vendorID>(this.Document.Cache, null, true);
			PXUIFieldAttribute.SetEnabled<RQRequisition.vendorLocationID>(this.Document.Cache, null, true);
			PXUIFieldAttribute.SetEnabled<RQRequisition.vendorRefNbr>(this.Document.Cache, null, true);
			PXUIFieldAttribute.SetEnabled<RQRequisition.splittable>(this.Document.Cache, null, true);

			this.Lines.Cache.AllowInsert = false;			
			this.Lines.Cache.AllowDelete = false;			
		}

		protected virtual void RQRequisition_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			RQRequisition row = (RQRequisition)e.Row;

			if (row == null)
                return;

			bool splitable = row.Splittable == true;
			PXUIFieldAttribute.SetVisible<RQRequisition.curyID>(sender, null, PXAccess.FeatureInstalled<FeaturesSet.multicurrency>());
			PXUIFieldAttribute.SetVisible<RQRequisitionLine.biddingQty>(Lines.Cache, null, splitable);
			PXUIFieldAttribute.SetVisible<RQBidding.orderQty>(Bidding.Cache, null, splitable);			

			PXUIFieldAttribute.SetEnabled<RQBidding.selected>(Bidding.Cache, null, splitable);
			PXUIFieldAttribute.SetEnabled<RQRequisition.reqNbr>(sender, null, !(State.Current.SingleMode == true));
			PXUIFieldAttribute.SetEnabled<RQRequisition.vendorID>(sender, row, false);
			PXUIFieldAttribute.SetEnabled<RQRequisition.vendorLocationID>(sender, row, false);

			if (row.Status == RQRequisitionStatus.Bidding)
			{
				PXUIFieldAttribute.SetEnabled<RQRequisition.vendorRefNbr>(sender, row, true);
				PXUIFieldAttribute.SetEnabled<RQRequisition.splittable>(sender, row, true);
			}
			else
			{
				PXUIFieldAttribute.SetEnabled(sender, row, false);
				PXUIFieldAttribute.SetEnabled<RQRequisition.reqNbr>(sender, row, true);
			}

			this.Process.SetEnabled(row.Status == RQRequisitionStatus.Bidding);
			this.ChooseVendor.SetEnabled(row.Status == RQRequisitionStatus.Bidding);
			this.UpdateResult.SetEnabled(row.Status == RQRequisitionStatus.Bidding);
			this.ClearResult.SetEnabled(row.Status == RQRequisitionStatus.Bidding);

			Document.Cache.AllowInsert =				
			Document.Cache.AllowDelete = false;
			Document.Cache.AllowUpdate = true;

			Bidding.Cache.AllowInsert =
			Bidding.Cache.AllowUpdate =
			Bidding.Cache.AllowDelete =

			Vendors.Cache.AllowInsert =
			Vendors.Cache.AllowUpdate =
			Vendors.Cache.AllowDelete = 
				row.Status == RQRequisitionStatus.Bidding;			
			
		}

		protected virtual void RQBidding_Selected_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			RQBidding row = (RQBidding)e.Row;

			if(row.Selected == true && row.OrderQty.GetValueOrDefault() == 0)
			{
				RQRequisitionLine line = 
                    PXSelect<RQRequisitionLine,
                    	Where<RQRequisitionLine.reqNbr, Equal<Required<RQRequisitionLine.reqNbr>>,
                    		And<RQRequisitionLine.lineNbr, Equal<Required<RQRequisitionLine.lineNbr>>>>>
                    	.Select(this, row.ReqNbr, row.LineNbr);

                decimal open = line.OrderQty.GetValueOrDefault() - line.BiddingQty.GetValueOrDefault();

                if (open > 0)				
					sender.SetValueExt<RQBidding.orderQty>(row, open);									
				
			}

			if (row.Selected == false)
				row.OrderQty = 0;
		}
		protected virtual void RQBidding_OrderQty_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			RQBidding row = (RQBidding)e.Row;

			if (row == null)
                return;

            RQRequisitionLine line = 
                PXSelect<RQRequisitionLine,
                	Where<RQRequisitionLine.reqNbr, Equal<Required<RQRequisitionLine.reqNbr>>,
                		And<RQRequisitionLine.lineNbr, Equal<Required<RQRequisitionLine.lineNbr>>>>>
                	.Select(this, row.ReqNbr, row.LineNbr);

			if (line == null)
                return;

			decimal delta = ((decimal?)e.NewValue).GetValueOrDefault() - row.OrderQty.GetValueOrDefault();

			if (delta > 0 && line.OrderQty - line.BiddingQty < delta)
			{
				sender.RaiseExceptionHandling<RQBidding.orderQty>(row, e.NewValue,
					new PXSetPropertyException(Messages.OrderQtyInsuff, PXErrorLevel.Warning));

				e.NewValue = row.OrderQty + line.OrderQty - line.BiddingQty;
			}

			if((decimal?)e.NewValue > 0)
				row.Selected = true;

		    if ((decimal?) e.NewValue != 0 && (row.MinQty != 0 || row.QuoteQty != 0))
		    {
		        if (row.MinQty > (decimal?) e.NewValue)
		        {
		            sender.RaiseExceptionHandling<RQBidding.orderQty>(row, e.NewValue,
		                new PXSetPropertyException(Messages.OrderQtyLessMinQty, PXErrorLevel.Warning));
		        }

		        if (row.QuoteQty < (decimal?) e.NewValue) 
		        {
		            sender.RaiseExceptionHandling<RQBidding.orderQty>(row, e.NewValue,
		                new PXSetPropertyException(Messages.OrderQtyMoreQuoteQty, PXErrorLevel.Warning));
		        }
		    }

		}
		protected virtual void RQBidding_VendorLocationID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			sender.SetDefaultExt<RQBidding.quoteUnitCost>(e.Row);
		}

        protected virtual void RQBidding_CuryQuoteUnitCost_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            RQBidding bidding = e.Row as RQBidding;

            if (bidding == null)
                return;

            RQRequisitionLine line =
                PXSelect<RQRequisitionLine,
                   Where<RQRequisitionLine.reqNbr, Equal<Required<RQRequisitionLine.reqNbr>>,
                     And<RQRequisitionLine.lineNbr, Equal<Required<RQRequisitionLine.reqNbr>>>>>
                .Select(this,
                        bidding.ReqNbr ?? this.State.Current.ReqNbr,
                        bidding.LineNbr ?? this.State.Current.LineNbr);

            Vendor vendor = PXSelect<Vendor, 
            	               Where<Vendor.bAccountID, Equal<Required<Vendor.bAccountID>>>>
            	            .Select(this, bidding.VendorID);

            if (vendor != null && line?.InventoryID != null && bidding.CuryInfoID != null)
            {
				POItemCostManager.ItemCost cost =
				  POItemCostManager.Fetch(this, bidding.VendorID, bidding.VendorLocationID, docDate: null, curyID: vendor.CuryID,
										  inventoryID: line.InventoryID, subItemID: line.SubItemID, siteID: null, uom: line.UOM);

				e.NewValue = cost.Convert<RQRequisitionLine.inventoryID, RQBidding.curyInfoID>(sender.Graph, line, bidding, line.UOM);
				e.Cancel = true;
            }
        }

        protected virtual void RQBidding_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
        {
            RQBidding row = (RQBidding)e.Row;

            if (row == null)
                return;

            if (this.Document.Current == null || !this.Document.Current.Splittable.GetValueOrDefault())
            {
                PXUIFieldAttribute.SetEnabled<RQBidding.orderQty>(sender, null, false);
                row.OrderQty = 0;
            }
            else
                PXUIFieldAttribute.SetEnabled<RQBidding.orderQty>(sender, null, true);
        }

        protected virtual void RQBidding_RowInserting(PXCache sender, PXRowInsertingEventArgs e)
        {
            RQBidding row = (RQBidding)e.Row;

            if (row == null)
                return;

            row.LineNbr = this.State.Current.LineNbr;
            row.ReqNbr = this.State.Current.ReqNbr;
            e.Cancel = !ValidateBiddingDuplicates(sender, row, null);

            UpdateVendor(sender, row);

            RQBiddingVendor vendor = ChoosenVendor.Select(row.ReqNbr, row.VendorID, row.VendorLocationID);

            if (vendor != null)
            {
                row.CuryInfoID = vendor.CuryInfoID;
            }
        }

		protected virtual void RQBidding_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			RQBidding row = (RQBidding)e.Row;

			if (row != null)
			{
				RQBidding upd = PXCache<RQBidding>.CreateCopy(row);				
				sender.SetDefaultExt<RQBidding.curyQuoteUnitCost>(upd);
				upd = this.Bidding.Update(upd);
			}
		}

		protected virtual void RQBidding_RowUpdating(PXCache sender, PXRowUpdatingEventArgs e)
		{
			RQBidding row = (RQBidding)e.Row;
			RQBidding newRow = (RQBidding)e.NewRow;

			if (row != null && newRow != null && row != newRow &&
			   (row.VendorID != newRow.VendorID || row.VendorLocationID != newRow.VendorLocationID))
			{
				e.Cancel = !ValidateBiddingDuplicates(sender, newRow, row);
			}

			if (e.Cancel != true)
			{
				if (newRow.VendorID != null && newRow.VendorLocationID == null && !PXAccess.FeatureInstalled<FeaturesSet.accountLocations>())
				{
					sender.SetDefaultExt<RQBidding.vendorLocationID>(newRow);
				}

				UpdateVendor(sender, newRow);

				if (row.VendorID != newRow.VendorID || row.VendorLocationID != newRow.VendorLocationID)
				{
					RQBiddingVendor vendor = ChoosenVendor.Select(row.ReqNbr, newRow.VendorID, newRow.VendorLocationID);

					if (vendor != null)
					{
						newRow.CuryInfoID = vendor.CuryInfoID;
						sender.SetDefaultExt<RQBidding.curyQuoteUnitCost>(newRow);
					}
				}
			}
		}

		protected virtual void RQBiddingVendor_RowInserting(PXCache sender, PXRowInsertingEventArgs e)
		{
			RQBiddingVendor row = (RQBiddingVendor)e.Row;

			if (row != null)
			{
				PORemitAddressAttribute.DefaultRecord<RQBiddingVendor.remitAddressID>(sender, e.Row);
				PORemitContactAttribute.DefaultRecord<RQBiddingVendor.remitContactID>(sender, e.Row);

                e.Cancel = !ValidateBiddingVendorDuplicates(sender, row, null);				
			}
		}

		protected virtual void RQBiddingVendor_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			RQBiddingVendor row = (RQBiddingVendor)e.Row;

			if (row == null)
                return;

            Vendor v = PXSelect<Vendor, 
            	          Where<Vendor.bAccountID, Equal<Current<RQBiddingVendor.vendorID>>>>
            	       .SelectSingleBound(this, new object[] { row });
		}

        protected virtual void RQBiddingVendor_RowUpdating(PXCache sender, PXRowUpdatingEventArgs e)
        {
            RQBiddingVendor row = (RQBiddingVendor)e.Row;
            RQBiddingVendor newRow = (RQBiddingVendor)e.NewRow;

            if (row != null && newRow != null && row != newRow &&
               (row.VendorID != newRow.VendorID || row.VendorLocationID != newRow.VendorLocationID))
            {
                e.Cancel = !ValidateBiddingVendorDuplicates(sender, newRow, row);
            }
        }
		protected virtual void RQBiddingVendor_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			RQBiddingVendor row = (RQBiddingVendor)e.Row;

			if(row == null)
                return;

            RQBidding b =
			PXSelect<RQBidding,
				Where<RQBidding.reqNbr, Equal<Current<RQBiddingVendor.reqNbr>>,
					And<RQBidding.vendorID, Equal<Current<RQBiddingVendor.vendorID>>,
					And<RQBidding.vendorLocationID, Equal<Current<RQBiddingVendor.vendorLocationID>>>>>>
				.SelectSingleBound(this,new object[]{row});

			PXUIFieldAttribute.SetEnabled<RQBiddingVendor.vendorID>(sender, row, b == null);
			PXUIFieldAttribute.SetEnabled<RQBiddingVendor.vendorLocationID>(sender, row, b == null);
			PXUIFieldAttribute.SetEnabled<RQBiddingVendor.curyID>(sender, row, b == null);
			PXUIFieldAttribute.SetEnabled<RQBiddingVendor.curyInfoID>(sender, row, b == null);
		}

		protected virtual void RQRequisition_VendorID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{			
			sender.SetDefaultExt<RQRequisition.vendorLocationID>(e.Row);			
		}
		
		protected virtual void RQBiddingVendor_VendorID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			RQBiddingVendor row = (RQBiddingVendor)e.Row;

			if (PXAccess.FeatureInstalled<FeaturesSet.multicurrency>() && row != null)
			{
				Vendor.Current = (Vendor)Vendor.View.SelectSingleBound(new object[] { e.Row });
				CurrencyInfo info = CurrencyInfoAttribute.SetDefaults<RQBiddingVendor.curyInfoID>(sender, e.Row);				
				sender.SetDefaultExt<RQBiddingVendor.curyID>(e.Row);
			}
		}
		protected virtual void RQBiddingVendor_VendorLocationID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			PORemitAddressAttribute.DefaultRecord<RQBiddingVendor.remitAddressID>(sender, e.Row);
			PORemitContactAttribute.DefaultRecord<RQBiddingVendor.remitContactID>(sender, e.Row);					
		}

		#region CurrencyInfo events
		protected virtual void CurrencyInfo_CuryID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (PXAccess.FeatureInstalled<FeaturesSet.multicurrency>() && 
				Vendor.Current != null &&
			    !string.IsNullOrEmpty(Vendor.Current.CuryID))
			{
				e.NewValue = Vendor.Current.CuryID;
				e.Cancel = true;
			}
		}

		protected virtual void CurrencyInfo_CuryRateTypeID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (PXAccess.FeatureInstalled<FeaturesSet.multicurrency>() && Vendor.Current != null)
			{
				e.NewValue = Vendor.Current.CuryRateTypeID ?? cmsetup.Current.APRateTypeDflt;
				e.Cancel = true;			
			}
		}

		protected virtual void CurrencyInfo_CuryEffDate_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (Document.Cache.Current != null)
			{
				e.NewValue = ((RQRequisition)Document.Cache.Current).OrderDate;
				e.Cancel = true;
			}
		}

        protected virtual void CurrencyInfo_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
        {
            CurrencyInfo info = e.Row as CurrencyInfo;

            if (info == null)
                return;

            bool curyEnabled = info.IsReadOnly != true && this.Bidding.Cache.AllowUpdate;
            bool curyRateEnabled = info.AllowUpdate(this.Bidding.Cache);

            RQRequisition doc = Document.Current;

            if (doc != null)
            {
                PXResult<RQBiddingVendor, Vendor> v = 
                    (PXResult<RQBiddingVendor, Vendor>)
                    PXSelectJoin<RQBiddingVendor,
                    	InnerJoin<Vendor, 
                    		On<RQBiddingVendor.vendorID, Equal<Vendor.bAccountID>>>,
                    	Where<RQBiddingVendor.curyInfoID, Equal<Required<RQBiddingVendor.curyInfoID>>>>
                    .SelectWindowed(this, 0, 1, info.CuryInfoID);

                Vendor vendor = v != null ? (Vendor)v : null;

                if (curyRateEnabled)
                    curyEnabled = vendor != null && vendor.AllowOverrideCury == true;

                if (curyRateEnabled)
                    curyRateEnabled = vendor != null && vendor.AllowOverrideRate == true;
            }

            PXUIFieldAttribute.SetEnabled<CurrencyInfo.curyID>(sender, info, curyEnabled);
            PXUIFieldAttribute.SetEnabled<CurrencyInfo.curyRateTypeID>(sender, info, curyRateEnabled);
            PXUIFieldAttribute.SetEnabled<CurrencyInfo.curyEffDate>(sender, info, curyRateEnabled);
            PXUIFieldAttribute.SetEnabled<CurrencyInfo.sampleCuryRate>(sender, info, curyRateEnabled);
            PXUIFieldAttribute.SetEnabled<CurrencyInfo.sampleRecipRate>(sender, info, curyRateEnabled);
        }
		#endregion

		private bool ValidateBiddingVendorDuplicates(PXCache sender, RQBiddingVendor row, RQBiddingVendor oldRow)
		{
            if (row.VendorLocationID != null)
            {
                foreach (RQBiddingVendor sibling in Vendors.Select(row.ReqNbr ?? this.State.Current.ReqNbr))
                {
                    if (sibling.VendorID == row.VendorID && sibling.VendorLocationID == row.VendorLocationID && row.LineID != sibling.LineID)
                    {
                        if (oldRow == null || oldRow.VendorID != row.VendorID)
                        {
                            sender.RaiseExceptionHandling<RQBiddingVendor.vendorID>(row, row.VendorID, 
                                new PXSetPropertyException(ErrorMessages.DuplicateEntryAdded));
                        }

                        if (oldRow == null || oldRow.VendorLocationID != row.VendorLocationID)
                        {
                            sender.RaiseExceptionHandling<RQBiddingVendor.vendorLocationID>(row, row.VendorLocationID, 
                                new PXSetPropertyException(ErrorMessages.DuplicateEntryAdded));
                        }

                        return false;
                    }
                }
            }

			PXUIFieldAttribute.SetError<RQBiddingVendor.vendorID>(sender, row, null);
			PXUIFieldAttribute.SetError<RQBiddingVendor.vendorLocationID>(sender, row, null);
			return true;
		}

		private bool ValidateBiddingDuplicates(PXCache sender, RQBidding row, RQBidding oldRow)
		{
            if (row.VendorLocationID != null)
            {
                foreach (RQBidding sibling in Bidding.Select(row.ReqNbr ?? this.State.Current.ReqNbr,
                                                             row.LineNbr ?? this.State.Current.LineNbr))
                {
                    if (sibling.VendorID == row.VendorID && sibling.VendorLocationID == row.VendorLocationID && row.LineID != sibling.LineID)
                    {
                        if (oldRow == null || oldRow.VendorID != row.VendorID)
                        {
                            sender.RaiseExceptionHandling<RQBidding.vendorID>(row, row.VendorID, 
                                new PXSetPropertyException(ErrorMessages.DuplicateEntryAdded));
                        }

                        if (oldRow == null || oldRow.VendorLocationID != row.VendorLocationID)
                        {
                            sender.RaiseExceptionHandling<RQBidding.vendorLocationID>(row, row.VendorLocationID, 
                                new PXSetPropertyException(ErrorMessages.DuplicateEntryAdded));
                        }

                        return false;
                    }
                }
            }

			PXUIFieldAttribute.SetError<RQBidding.vendorID>(sender, row, null);
			PXUIFieldAttribute.SetError<RQBidding.vendorLocationID>(sender, row, null);
			return true;
		}

		private void UpdateVendor(PXCache sender, RQBidding row)
		{
			if (row.VendorID == null || row.VendorLocationID == null)
                return;

			RQBiddingVendor bidder =
						PXSelect<RQBiddingVendor,
							Where<RQBiddingVendor.reqNbr, Equal<Required<RQBiddingVendor.reqNbr>>,
								And<RQBiddingVendor.vendorID, Equal<Required<RQBiddingVendor.vendorID>>,
								And<RQBiddingVendor.vendorLocationID, Equal<Required<RQBiddingVendor.vendorLocationID>>>>>>
							.SelectWindowed(this, 0, 1, row.ReqNbr, row.VendorID, row.VendorLocationID);

			if (bidder == null)
			{
				bidder = new RQBiddingVendor();
				bidder.ReqNbr = row.ReqNbr;

				bidder = PXCache<RQBiddingVendor>.CreateCopy(this.Vendors.Insert(bidder));

				bidder.VendorID = row.VendorID;
				bidder.VendorLocationID = row.VendorLocationID;
				bidder.EntryDate = this.Accessinfo.BusinessDate;

				this.Vendors.Update(bidder);
			}
			else if (bidder.EntryDate == null)
			{
				bidder.EntryDate = this.Accessinfo.BusinessDate;
				this.Vendors.Update(bidder);
			}
		}
	}
}
