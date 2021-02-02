using System;
using System.Collections.Generic;
using System.Text;
using PX.Data;
using PX.Objects.CS;
using System.Collections;
using PX.Objects.IN;
using PX.Objects.GL;
using System.Diagnostics;
using System.Linq;
using PX.Common;
using PX.Objects.Common.Discount;

namespace PX.Objects.AR
{
    [Serializable]
	public class ARDiscountSequenceMaint : PXGraph<ARDiscountSequenceMaint>
	{
        public ARDiscountSequenceMaint()
        {
            PXDBDefaultAttribute.SetDefaultForInsert<DiscountSequence.discountSequenceID>(Sequence.Cache, null, false);
        }

		#region Selects/Views
				
		public PXSelect<ARDiscountEx, Where<ARDiscountEx.discountID, Equal<Current<DiscountSequence.discountID>>>> Discount;
        public PXSelectJoin<DiscountSequence, InnerJoin<ARDiscount, On<DiscountSequence.discountID, Equal<ARDiscount.discountID>>>> Sequence;

        public PXSelect<DiscountSequence, Where<DiscountSequence.discountID, Equal<Current<DiscountSequence.discountID>>, 
            And<DiscountSequence.discountSequenceID, Equal<Current<DiscountSequence.discountSequenceID>>>>> CurrentSequence;

        public PXSelect<DiscountDetail, Where<DiscountDetail.discountID, Equal<Current<DiscountSequence.discountID>>,
            And<DiscountDetail.discountSequenceID, Equal<Current<DiscountSequence.discountSequenceID>>>>, OrderBy<Asc<DiscountDetail.quantity, Asc<DiscountDetail.amount>>>> Details;

        public PXSelect<DiscountSequenceDetail, Where<DiscountSequenceDetail.discountID, Equal<Current<DiscountSequence.discountID>>,
            And<DiscountSequenceDetail.discountSequenceID, Equal<Current<DiscountSequence.discountSequenceID>>>>, OrderBy<Asc<DiscountSequenceDetail.quantity, Asc<DiscountSequenceDetail.amount>>>> SequenceDetails;

		[PXImport(typeof(DiscountSequence))]
		public PXSelectJoin<DiscountItem,
			InnerJoin<InventoryItem, On<DiscountItem.inventoryID, Equal<InventoryItem.inventoryID>>>,
			Where<DiscountItem.discountID, Equal<Current<DiscountSequence.discountID>>,
			And<DiscountItem.discountSequenceID, Equal<Current<DiscountSequence.discountSequenceID>>>>> Items;
		
		[PXImport(typeof(DiscountSequence))]
		public PXSelectJoin<DiscountCustomer,
		    InnerJoin<Customer, On<DiscountCustomer.customerID, Equal<Customer.bAccountID>>>,
		    Where<DiscountCustomer.discountID, Equal<Current<DiscountSequence.discountID>>,
		    And<DiscountCustomer.discountSequenceID, Equal<Current<DiscountSequence.discountSequenceID>>>>> Customers;

		public PXSelectJoin<DiscountCustomerPriceClass,
			InnerJoin<ARPriceClass, On<DiscountCustomerPriceClass.customerPriceClassID, Equal<ARPriceClass.priceClassID>>>,
			Where<DiscountCustomerPriceClass.discountID, Equal<Current<DiscountSequence.discountID>>,
			And<DiscountCustomerPriceClass.discountSequenceID, Equal<Current<DiscountSequence.discountSequenceID>>>>> CustomerPriceClasses;

		public PXSelectJoin<DiscountInventoryPriceClass,
			InnerJoin<INPriceClass, On<DiscountInventoryPriceClass.inventoryPriceClassID, Equal<INPriceClass.priceClassID>>>,
			Where<DiscountInventoryPriceClass.discountID, Equal<Current<DiscountSequence.discountID>>,
			And<DiscountInventoryPriceClass.discountSequenceID, Equal<Current<DiscountSequence.discountSequenceID>>>>> InventoryPriceClasses;

        public PXSelectJoin<DiscountBranch,
            InnerJoin<Branch, On<DiscountBranch.branchID, Equal<Branch.branchID>>>,
            Where<DiscountBranch.discountID, Equal<Current<DiscountSequence.discountID>>,
            And<DiscountBranch.discountSequenceID, Equal<Current<DiscountSequence.discountSequenceID>>>>> Branches;

        public PXSelectJoin<DiscountSite,
            InnerJoin<INSite, On<DiscountSite.siteID, Equal<INSite.siteID>>>,
            Where<DiscountSite.discountID, Equal<Current<DiscountSequence.discountID>>,
            And<DiscountSite.discountSequenceID, Equal<Current<DiscountSequence.discountSequenceID>>>>> Sites;

		public PXFilter<UpdateSettingsFilter> UpdateSettings;

		#endregion
			
		#region Buttons/Actions

		public PXSave<DiscountSequence> Save;
		[PXCancelButton]
		[PXUIField(DisplayName = ActionsMessages.Cancel, MapEnableRights = PXCacheRights.Select)]
		protected virtual IEnumerable Cancel(PXAdapter a)
		{
			DiscountSequence current = null;
			string discountID = null;
			string sequenceID = null;

			#region Extract Keys
			if (a.Searches != null)
			{
				if (a.Searches.Length > 0)
					discountID = (string)a.Searches[0];
				if (a.Searches.Length > 1)
					sequenceID = (string)a.Searches[1];
			}
			#endregion

			DiscountSequence seq = PXSelect<DiscountSequence, 
				Where<DiscountSequence.discountSequenceID, Equal<Required<DiscountSequence.discountSequenceID>>,
				And<DiscountSequence.discountID, Equal<Required<DiscountSequence.discountID>>>>>.Select(this, sequenceID, discountID);

			ARDiscount discount = PXSelect<ARDiscount, Where<ARDiscount.discountID, Equal<Required<ARDiscount.discountID>>>>.Select(this, discountID);

			bool insertNewSequence = false;
			if (seq == null)
			{
                if (a.Searches != null && a.Searches.Length > 1)
                {
                    a.Searches[1] = null;
                }
				insertNewSequence = true;
			}

			if (Discount.Current != null && Discount.Current.DiscountID != discountID)
			{
				sequenceID = null;
			}

            foreach (PXResult<DiscountSequence, ARDiscount> headerCanceled in (new PXCancel<DiscountSequence>(this, "Cancel")).Press(a))
			{
                current = (DiscountSequence)headerCanceled;
			}

			if (insertNewSequence)
			{
				Sequence.Cache.Remove(current);

				DiscountSequence newSeq = new DiscountSequence();
				newSeq.DiscountID = discountID;

				if (discount != null)
				{
					newSeq.Description = discount.Description;
				}

				current = Sequence.Insert(newSeq);
				Sequence.Cache.IsDirty = false;

				if (discount != null)
				{
					if (discount.IsAutoNumber == false)
						current.DiscountSequenceID = sequenceID;
					else
						current.DiscountSequenceID = PXMessages.LocalizeNoPrefix(AP.Messages.NewKey);

					Sequence.Cache.Normalize();
				}
			}
			
			yield return current;
		}

		public PXAction<DiscountSequence> cancel;
		public PXInsert<DiscountSequence> Insert;
		public PXDelete<DiscountSequence> Delete;
		public PXFirst<DiscountSequence> First;
		public PXPrevious<DiscountSequence> Prev;
		public PXNext<DiscountSequence> Next;
		public PXLast<DiscountSequence> Last;

		public PXAction<DiscountSequence> updateDiscounts;
		[PXUIField(DisplayName = "Update Discounts", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Select, Enabled = true)]
		[PXProcessButton]
        public virtual IEnumerable UpdateDiscounts(PXAdapter adapter)
		{
			if (Sequence.Current != null)
			{
			    var result = UpdateSettings.AskExt();
			    if (result == WebDialogResult.OK || (IsContractBasedAPI && result == WebDialogResult.Yes))
                {
                    Save.Press();
                    ARUpdateDiscounts.UpdateDiscount(Sequence.Current.DiscountID, Sequence.Current.DiscountSequenceID, UpdateSettings.Current.FilterDate);
                    Sequence.Current.tstamp = PXDatabase.SelectTimeStamp();
                    Save.Press();
					SelectTimeStamp();
                    Details.Cache.Clear();
                    Details.Cache.ClearQueryCacheObsolete();
                    CurrentSequence.Cache.Clear();
                    CurrentSequence.Cache.ClearQueryCacheObsolete();
                    //PXLongOperation.StartOperation(this, delegate() { SOUpdateDiscounts.UpdateDiscount(Sequence.Current.DiscountID, Sequence.Current.DiscountSequenceID, UpdateSettings.Current.FilterDate); });
                }
			}

			return adapter.Get();
		}

		#endregion

		#region Event Handlers

		protected virtual void DiscountDetail_StartDate_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			DiscountDetail row = e.Row as DiscountDetail;
			if (row != null)
			{
				if (Sequence.Current != null && Sequence.Current.StartDate != null)
					e.NewValue = Sequence.Current.StartDate;
				else
					e.NewValue = Accessinfo.BusinessDate;
			}
		}

        protected virtual void DiscountSequence_IsPromotion_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            DiscountSequence row = e.Row as DiscountSequence;
            if (row != null)
            {
                if (row.IsPromotion == true)
                {
                    row.PendingFreeItemID = null;
                    row.LastFreeItemID = null;
                }
                else
                {
                    row.EndDate = null;
                }
            }
        }

		protected virtual void DiscountSequence_DiscountID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			DiscountSequence row = e.Row as DiscountSequence;
			ARDiscount discount = PXSelect<ARDiscount, Where<ARDiscount.discountID, Equal<Required<ARDiscount.discountID>>>>.Select(this, row.DiscountID);

			if (row != null && discount != null)
			{
				if (discount.Type == DiscountType.Document)
					row.BreakBy = BreakdownType.Amount;
			}
		}

        protected virtual void DiscountDetail_Amount_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            DiscountDetail row = e.Row as DiscountDetail;
            if (row != null && Sequence.Current.IsPromotion == true)
            {
                PXResult<DiscountDetail> prevDetailLine = PXSelect<DiscountDetail, Where<DiscountDetail.discountID, Equal<Current<DiscountSequence.discountID>>,
                And<DiscountDetail.discountSequenceID, Equal<Current<DiscountSequence.discountSequenceID>>, And<DiscountDetail.amount, Less<Required<DiscountDetail.amount>>, And<DiscountDetail.isActive, Equal<True>>>>>, OrderBy<Desc<DiscountDetail.amount>>>.SelectSingleBound(this, null, row.Amount);
                if (prevDetailLine != null)
                {
                    DiscountDetail prevLine = (DiscountDetail)prevDetailLine;
                    prevLine.AmountTo = row.Amount;
                    Details.Update(prevLine);
                }
                PXResult<DiscountDetail> nextDetailLine = PXSelect<DiscountDetail, Where<DiscountDetail.discountID, Equal<Current<DiscountSequence.discountID>>,
                And<DiscountDetail.discountSequenceID, Equal<Current<DiscountSequence.discountSequenceID>>, And<DiscountDetail.amount, Greater<Required<DiscountDetail.amount>>, And<DiscountDetail.isActive, Equal<True>>>>>, OrderBy<Asc<DiscountDetail.amount>>>.SelectSingleBound(this, null, row.Amount);
                if (nextDetailLine == null)
                {
                    row.AmountTo = null;
                }
                else
                {
                    DiscountDetail nextLine = (DiscountDetail)nextDetailLine;
                    row.AmountTo = nextLine.Amount;
                }
            }
        }

        protected virtual void DiscountDetail_Quantity_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            DiscountDetail row = e.Row as DiscountDetail;
            if (row != null && Sequence.Current.IsPromotion == true)
            {
                PXResult<DiscountDetail> prevDetailLine = PXSelect<DiscountDetail, Where<DiscountDetail.discountID, Equal<Current<DiscountSequence.discountID>>,
                And<DiscountDetail.discountSequenceID, Equal<Current<DiscountSequence.discountSequenceID>>, And<DiscountDetail.quantity, Less<Required<DiscountDetail.quantity>>, And<DiscountDetail.isActive, Equal<True>>>>>, OrderBy<Desc<DiscountDetail.quantity>>>.SelectSingleBound(this, null, row.Quantity);
                if (prevDetailLine != null)
                {
                    DiscountDetail prevLine = (DiscountDetail)prevDetailLine;
                    prevLine.QuantityTo = row.Quantity;
                    Details.Update(prevLine);
                }
                PXResult<DiscountDetail> nextDetailLine = PXSelect<DiscountDetail, Where<DiscountDetail.discountID, Equal<Current<DiscountSequence.discountID>>,
                And<DiscountDetail.discountSequenceID, Equal<Current<DiscountSequence.discountSequenceID>>, And<DiscountDetail.quantity, Greater<Required<DiscountDetail.quantity>>, And<DiscountDetail.isActive, Equal<True>>>>>, OrderBy<Asc<DiscountDetail.quantity>>>.SelectSingleBound(this, null, row.Quantity);
                if (nextDetailLine == null)
                {
                    row.QuantityTo = null;
                }
                else
                {
                    DiscountDetail nextLine = (DiscountDetail)nextDetailLine;
                    row.QuantityTo = nextLine.Quantity;
                }
            }
        }

		protected virtual void DiscountSequence_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
            if (e.Row == null) return;

            ARDiscount discount = PXSelect<ARDiscount, Where<ARDiscount.discountID, Equal<Required<ARDiscount.discountID>>>>.Select(this, ((DiscountSequence)e.Row).DiscountID);
			SetControlsState(sender, e.Row as DiscountSequence, discount);
			SetGridColumnsState(discount);

            if (discount != null && discount.Type != DiscountType.Group)
            {
                Dictionary<string, string> allowed = new DiscountOption.ListAttribute().ValueLabelDic;
                allowed.Remove(DiscountOption.FreeItem);
                PXStringListAttribute.SetList<DiscountSequence.discountedFor>(sender, e.Row,
                                                               allowed.Keys.ToArray(),
                                                               allowed.Values.ToArray());
            }
		}

        protected virtual void DiscountSequence_RowUpdating(PXCache sender, PXRowUpdatingEventArgs e)
        {
            DiscountSequence newRow = e.NewRow as DiscountSequence;
            if (newRow != null && newRow.IsActive == true && newRow.DiscountedFor == DiscountOption.FreeItem && newRow.FreeItemID == null)
            {
                newRow.IsActive = false;

                if (newRow.IsPromotion == true)
                {
                    CurrentSequence.Cache.RaiseExceptionHandling<DiscountSequence.freeItemID>(e.NewRow, newRow.FreeItemID,
                        new PXSetPropertyException(AR.Messages.FreeItemMayNotBeEmpty, PXErrorLevel.Warning));
                    CurrentSequence.Cache.RaiseExceptionHandling<DiscountSequence.isActive>(e.NewRow, newRow.IsActive,
                        new PXSetPropertyException(AR.Messages.FreeItemMayNotBeEmpty, PXErrorLevel.Warning));
                }
                else
                {
                    CurrentSequence.Cache.RaiseExceptionHandling<DiscountSequence.freeItemID>(e.NewRow, newRow.FreeItemID,
                        new PXSetPropertyException(AR.Messages.FreeItemMayNotBeEmptyPending, PXErrorLevel.Warning));
                    CurrentSequence.Cache.RaiseExceptionHandling<DiscountSequence.isActive>(e.NewRow, newRow.IsActive,
                        new PXSetPropertyException(AR.Messages.FreeItemMayNotBeEmptyPending, PXErrorLevel.Warning));
                }
            }
        }

		protected virtual void DiscountSequence_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			DiscountSequence row = e.Row as DiscountSequence;
			if (row != null)
			{
                if (row.DiscountSequenceID == null)
                {
                    throw new PXRowPersistingException(typeof(DiscountSequence.discountSequenceID).Name, null, ErrorMessages.FieldIsEmpty);
                }
                if (row.IsPromotion == true && row.EndDate == null)
				{
					if (sender.RaiseExceptionHandling<DiscountSequence.endDate>(e.Row, null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty)))
					{
						throw new PXRowPersistingException(typeof(DiscountSequence.endDate).Name, null, ErrorMessages.FieldIsEmpty);
					}
				}
                if (row.IsPromotion == true && row.EndDate != null && row.StartDate != null && row.EndDate < row.StartDate)
                {
					if (sender.RaiseExceptionHandling<DiscountSequence.endDate>(e.Row, row.EndDate, new PXSetPropertyException(AR.Messages.EffectiveDateExpirationDate)))
                    {
						throw new PXRowPersistingException(typeof(DiscountSequence.endDate).Name, row.EndDate, AR.Messages.EffectiveDateExpirationDate);
                    }
                }
                if (row.DiscountedFor == DiscountOption.FreeItem && row.IsActive == true && row.FreeItemID == null)
                {
                    row.IsActive = false;
                    if (row.IsPromotion == true)
                        CurrentSequence.Cache.RaiseExceptionHandling<DiscountSequence.freeItemID>(e.Row, row.FreeItemID,
                            new PXSetPropertyException(AR.Messages.FreeItemMayNotBeEmpty, PXErrorLevel.Error));
                    else
                        CurrentSequence.Cache.RaiseExceptionHandling<DiscountSequence.freeItemID>(e.Row, row.FreeItemID,
                            new PXSetPropertyException(AR.Messages.FreeItemMayNotBeEmptyPending, PXErrorLevel.Error));
                }
			}

			if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert)
			{
				ARDiscount discount = Discount.Current;
				if (discount != null)
				{
					PXDBDefaultAttribute.SetDefaultForInsert<DiscountSequence.discountSequenceID>(sender, e.Row, discount.IsAutoNumber == true);
				}
			}
		}

		protected virtual void DiscountSequence_DiscountSequenceID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (e.Row != null)
			{
				ARDiscount discount = PXSelect<ARDiscount, Where<ARDiscount.discountID, Equal<Required<DiscountSequence.discountID>>>>.Select(this, ((DiscountSequence)e.Row).DiscountID);
				if (discount != null && discount.IsAutoNumber == true)
				{
					e.NewValue = PXMessages.LocalizeNoPrefix(AP.Messages.NewKey);
				}
			}
		}
			
        //RowUpdated event handlers
		protected virtual void DiscountDetail_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			DiscountDetail currentLine = e.Row as DiscountDetail;
			if (currentLine != null)
			{
				if (!sender.ObjectsEqual<DiscountDetail.isActive>(e.Row, e.OldRow))
				{
					//update hidden line with "last" values
					PXResult<DiscountSequenceDetail> LastDiscountDetailLine = PXSelect<DiscountSequenceDetail, Where<DiscountSequenceDetail.discountID, Equal<Current<DiscountSequence.discountID>>,
						And<DiscountSequenceDetail.discountSequenceID, Equal<Current<DiscountSequence.discountSequenceID>>, And<DiscountSequenceDetail.discountDetailsID, NotEqual<Required<DiscountSequenceDetail.discountDetailsID>>,
							And<DiscountSequenceDetail.lineNbr, Equal<Required<DiscountSequenceDetail.lineNbr>>, And<DiscountSequenceDetail.isLast, Equal<True>>>>>>, OrderBy<Asc<DiscountSequenceDetail.quantity>>>.SelectSingleBound(this, null, currentLine.DiscountDetailsID, currentLine.LineNbr);
					if (LastDiscountDetailLine != null)
					{
						((DiscountSequenceDetail)LastDiscountDetailLine).IsActive = currentLine.IsActive;
						SequenceDetails.Update(LastDiscountDetailLine);
					}

					//update neighbours
					if (Sequence.Current.BreakBy == BreakdownType.Quantity)
					{
						DiscountDetail nextDetailLine = GetNextDiscountDetailLineQuantity(currentLine);
						DiscountDetail prevDetailLine = GetPrevDiscountDetailLineQuantity(currentLine);
						if (nextDetailLine == null)
						{

							if (prevDetailLine != null)
							{
								if (currentLine.IsActive == true)
								{
									currentLine.QuantityTo = null;
									prevDetailLine.QuantityTo = currentLine.Quantity;
									Details.Update(prevDetailLine);
								}
								else
								{
									prevDetailLine.QuantityTo = null;
									Details.Update(prevDetailLine);
								}
							}
							currentLine.QuantityTo = null;
						}
						else
						{
							if (prevDetailLine != null)
							{
								if (currentLine.IsActive == true)
								{
									prevDetailLine.QuantityTo = currentLine.Quantity;
									currentLine.QuantityTo = nextDetailLine.Quantity;
									Details.Update(prevDetailLine);
								}
								else
								{
									prevDetailLine.QuantityTo = nextDetailLine.Quantity;
									Details.Update(prevDetailLine);
								}
							}
							else
							{
								currentLine.QuantityTo = nextDetailLine.Quantity;
							}
						}
					}
					else
					{
						DiscountDetail nextDetailLine = GetNextDiscountDetailLineAmount(currentLine);
						DiscountDetail prevDetailLine = GetPrevDiscountDetailLineAmount(currentLine);
						if (nextDetailLine == null)
						{
							if (prevDetailLine != null)
							{
								if (currentLine.IsActive == true)
								{
									currentLine.AmountTo = null;
									prevDetailLine.AmountTo = currentLine.Amount;
									Details.Update(prevDetailLine);
								}
								else
								{
									prevDetailLine.AmountTo = null;
									Details.Update(prevDetailLine);
								}
							}
							currentLine.AmountTo = null;
						}
						else
						{
							if (prevDetailLine != null)
			{
								if (currentLine.IsActive == true)
								{
									prevDetailLine.AmountTo = currentLine.Amount;
									currentLine.AmountTo = nextDetailLine.Amount;
									Details.Update(prevDetailLine);
								}
								else
								{
									prevDetailLine.AmountTo = nextDetailLine.Amount;
									Details.Update(prevDetailLine);
								}
							}
							else
							{
								currentLine.AmountTo = nextDetailLine.Amount;
							}
						}
					}
				}

				if (!sender.ObjectsEqual<DiscountDetail.pendingQuantity, DiscountDetail.pendingAmount, DiscountDetail.pendingDiscountPercent, DiscountDetail.pendingFreeItemQty>(e.Row, e.OldRow))
				{
					if (currentLine.StartDate == null)
					{
						if (Sequence.Current != null && Sequence.Current.StartDate != null)
							currentLine.StartDate = Sequence.Current.StartDate;
						else
							currentLine.StartDate = Accessinfo.BusinessDate;
					}
				}
			}
		}

        protected virtual void DiscountDetail_RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
        {
            DiscountDetail row = e.Row as DiscountDetail;
            if (row != null)
            {
                PXResult<DiscountSequenceDetail> LastDiscountDetailLine = PXSelect<DiscountSequenceDetail, Where<DiscountSequenceDetail.discountID, Equal<Current<DiscountSequence.discountID>>,
                    And<DiscountSequenceDetail.discountSequenceID, Equal<Current<DiscountSequence.discountSequenceID>>, And<DiscountSequenceDetail.discountDetailsID, NotEqual<Required<DiscountSequenceDetail.discountDetailsID>>, 
                    And<DiscountSequenceDetail.lineNbr, Equal<Required<DiscountSequenceDetail.lineNbr>>, And<DiscountSequenceDetail.isLast, Equal<True>>>>>>, OrderBy<Asc<DiscountSequenceDetail.quantity>>>.SelectSingleBound(this, null, row.DiscountDetailsID, row.LineNbr);
                if (LastDiscountDetailLine != null)
                    SequenceDetails.Delete(LastDiscountDetailLine);
            }
        }

        protected virtual void DiscountDetail_RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
        {
            DiscountDetail row = e.Row as DiscountDetail;
            if (row != null)
            {
                if (Sequence.Current.BreakBy == BreakdownType.Quantity)
                {
					DiscountDetail nextDetailLine = GetNextDiscountDetailLineQuantity(row);
					DiscountDetail prevDetailLine = GetPrevDiscountDetailLineQuantity(row);
                        if (prevDetailLine != null)
                        {
						if (nextDetailLine == null)
						{
							prevDetailLine.QuantityTo = null;
							Details.Update(prevDetailLine);
                    }
                    else
                    {
							prevDetailLine.QuantityTo = nextDetailLine.Quantity;
							Details.Update(prevDetailLine);
                        }
                    }
                }
                else
                {
					DiscountDetail nextDetailLine = GetNextDiscountDetailLineAmount(row);
					DiscountDetail prevDetailLine = GetPrevDiscountDetailLineAmount(row);
                        if (prevDetailLine != null)
                        {
						if (nextDetailLine == null)
						{
							prevDetailLine.AmountTo = null;
							Details.Update(prevDetailLine);
                    }
                    else
                    {
							prevDetailLine.AmountTo = nextDetailLine.Amount;
							Details.Update(prevDetailLine);
						}
                        }
                    }
                }
            }

		public virtual DiscountDetail GetNextDiscountDetailLineQuantity(DiscountDetail currentLine)
		{
			return PXSelect<DiscountDetail, Where<DiscountDetail.discountID, Equal<Current<DiscountSequence.discountID>>,
					And<DiscountDetail.discountSequenceID, Equal<Current<DiscountSequence.discountSequenceID>>, And<DiscountDetail.quantity, Greater<Required<DiscountDetail.quantity>>, And<DiscountDetail.isActive, Equal<True>>>>>, OrderBy<Asc<DiscountDetail.quantity>>>.SelectSingleBound(this, null, currentLine.Quantity);
		}

		public virtual DiscountDetail GetPrevDiscountDetailLineQuantity(DiscountDetail currentLine)
		{
			return PXSelect<DiscountDetail, Where<DiscountDetail.discountID, Equal<Current<DiscountSequence.discountID>>,
						And<DiscountDetail.discountSequenceID, Equal<Current<DiscountSequence.discountSequenceID>>, And<DiscountDetail.quantity, Less<Required<DiscountDetail.quantity>>, And<DiscountDetail.isActive, Equal<True>>>>>, OrderBy<Desc<DiscountDetail.quantity>>>.SelectSingleBound(this, null, currentLine.Quantity);
		}

		public virtual DiscountDetail GetNextDiscountDetailLineAmount(DiscountDetail currentLine)
		{
			return PXSelect<DiscountDetail, Where<DiscountDetail.discountID, Equal<Current<DiscountSequence.discountID>>,
					And<DiscountDetail.discountSequenceID, Equal<Current<DiscountSequence.discountSequenceID>>, And<DiscountDetail.amount, Greater<Required<DiscountDetail.amount>>, And<DiscountDetail.isActive, Equal<True>>>>>, OrderBy<Asc<DiscountDetail.amount>>>.SelectSingleBound(this, null, currentLine.Amount);
		}

		public virtual DiscountDetail GetPrevDiscountDetailLineAmount(DiscountDetail currentLine)
		{
			return PXSelect<DiscountDetail, Where<DiscountDetail.discountID, Equal<Current<DiscountSequence.discountID>>,
						And<DiscountDetail.discountSequenceID, Equal<Current<DiscountSequence.discountSequenceID>>, And<DiscountDetail.amount, Less<Required<DiscountDetail.amount>>, And<DiscountDetail.isActive, Equal<True>>>>>, OrderBy<Desc<DiscountDetail.amount>>>.SelectSingleBound(this, null, currentLine.Amount);
        }

       	#endregion

		private void SetControlsState(PXCache sender, DiscountSequence row, ARDiscount discount)
		{
			if (row != null)
			{
                updateDiscounts.SetEnabled(!(sender.GetStatus(row) == PXEntryStatus.Inserted && discount != null && discount.IsAutoNumber == true) && row.IsPromotion != true);

                PXUIFieldAttribute.SetEnabled<DiscountSequence.endDate>(sender, row, row.IsPromotion == true);
				PXUIFieldAttribute.SetRequired<DiscountSequence.endDate>(sender, row.IsPromotion == true);
				PXUIFieldAttribute.SetRequired<DiscountSequence.startDate>(sender, row.IsPromotion == true);
                PXUIFieldAttribute.SetVisible<DiscountSequence.startDate>(sender, row, true);
                PXUIFieldAttribute.SetVisible<DiscountSequence.startDate>(sender, row, row.IsPromotion == true);
				PXUIFieldAttribute.SetVisible<DiscountSequence.endDate>(sender, row, row.IsPromotion == true);

				PXUIFieldAttribute.SetVisible<DiscountSequence.updateDate>(sender, row, row.IsPromotion == false);

                PXUIFieldAttribute.SetEnabled<DiscountSequence.breakBy>(sender, row, (discount != null && (discount.Type == DiscountType.Group || discount.Type == DiscountType.Line)));
                PXUIFieldAttribute.SetEnabled<DiscountSequence.pendingFreeItemID>(sender, row, row.DiscountedFor == DiscountOption.FreeItem && IsFreeItemApplicable(row.DiscountID));

                PXUIFieldAttribute.SetEnabled<DiscountSequence.freeItemID>(sender, row, row.IsPromotion == true);

                PXUIFieldAttribute.SetEnabled<DiscountSequence.prorate>(sender, row, row.DiscountedFor == DiscountOption.FreeItem || row.DiscountedFor == DiscountOption.Amount);
				PXUIFieldAttribute.SetVisible<DiscountSequence.pendingFreeItemID>(sender, row, row.IsPromotion != true);
				PXUIFieldAttribute.SetVisible<DiscountSequence.lastFreeItemID>(sender, row, row.IsPromotion != true);
			}
		}

		private void SetGridColumnsState(ARDiscount discount)
		{
			if (Sequence.Current == null) return;

			bool isPromotion = Sequence.Current.IsPromotion == true;

			bool discountByAmt = Sequence.Current.DiscountedFor == DiscountOption.Amount;
			bool discountByPct = Sequence.Current.DiscountedFor == DiscountOption.Percent;
			bool discountByItem = Sequence.Current.DiscountedFor == DiscountOption.FreeItem;

			bool breakByAmt = Sequence.Current.BreakBy == BreakdownType.Amount;
			bool breakByQty = Sequence.Current.BreakBy == BreakdownType.Quantity;

			PXUIFieldAttribute.SetVisible<DiscountItem.amount>(Items.Cache, null, breakByAmt);
			PXUIFieldAttribute.SetVisible<DiscountItem.quantity>(Items.Cache, null, breakByQty);
			PXUIFieldAttribute.SetVisible<DiscountItem.uOM>(Items.Cache, null, breakByQty);

			#region DiscountDetail Fields Visibility
			PXUIFieldAttribute.SetVisible(Details.Cache, null, false);

			PXUIFieldAttribute.SetVisible<DiscountDetail.discount>(Details.Cache, null, discountByAmt);
			PXUIFieldAttribute.SetVisible<DiscountDetail.lastDiscount>(Details.Cache, null, discountByAmt && !isPromotion);
			PXUIFieldAttribute.SetVisible<DiscountDetail.pendingDiscount>(Details.Cache, null, discountByAmt && !isPromotion);

			PXUIFieldAttribute.SetVisible<DiscountDetail.discountPercent>(Details.Cache, null, discountByPct);
			PXUIFieldAttribute.SetVisible<DiscountDetail.lastDiscountPercent>(Details.Cache, null, discountByPct && !isPromotion);
			PXUIFieldAttribute.SetVisible<DiscountDetail.pendingDiscountPercent>(Details.Cache, null, discountByPct && !isPromotion);

			PXUIFieldAttribute.SetVisible<DiscountDetail.freeItemQty>(Details.Cache, null, discountByItem);
			PXUIFieldAttribute.SetVisible<DiscountDetail.lastFreeItemQty>(Details.Cache, null, discountByItem && !isPromotion);
			PXUIFieldAttribute.SetVisible<DiscountDetail.pendingFreeItemQty>(Details.Cache, null, discountByItem && !isPromotion);

			PXUIFieldAttribute.SetVisible<DiscountDetail.amount>(Details.Cache, null, breakByAmt);
			PXUIFieldAttribute.SetVisible<DiscountDetail.lastAmount>(Details.Cache, null, breakByAmt && !isPromotion);
			PXUIFieldAttribute.SetVisible<DiscountDetail.pendingAmount>(Details.Cache, null, breakByAmt && !isPromotion);

			PXUIFieldAttribute.SetVisible<DiscountDetail.quantity>(Details.Cache, null, breakByQty);
			PXUIFieldAttribute.SetVisible<DiscountDetail.lastQuantity>(Details.Cache, null, breakByQty && !isPromotion);
			PXUIFieldAttribute.SetVisible<DiscountDetail.pendingQuantity>(Details.Cache, null, breakByQty && !isPromotion);

			PXUIFieldAttribute.SetVisible<DiscountDetail.startDate>(Details.Cache, null, !isPromotion);
			PXUIFieldAttribute.SetVisible<DiscountDetail.lastDate>(Details.Cache, null, !isPromotion); 
                #endregion

			#region DiscountDetail Fields Editability
			PXUIFieldAttribute.SetEnabled(Details.Cache, null, false);

			PXUIFieldAttribute.SetEnabled<DiscountDetail.isActive>(Details.Cache, null, true);

			PXUIFieldAttribute.SetEnabled<DiscountDetail.discount>(Details.Cache, null, discountByAmt && isPromotion);
			PXUIFieldAttribute.SetEnabled<DiscountDetail.pendingDiscount>(Details.Cache, null, discountByAmt && !isPromotion);

			PXUIFieldAttribute.SetEnabled<DiscountDetail.discountPercent>(Details.Cache, null, discountByPct && isPromotion);
			PXUIFieldAttribute.SetEnabled<DiscountDetail.pendingDiscountPercent>(Details.Cache, null, discountByPct && !isPromotion);

			PXUIFieldAttribute.SetEnabled<DiscountDetail.freeItemQty>(Details.Cache, null, discountByItem && isPromotion);
			PXUIFieldAttribute.SetEnabled<DiscountDetail.pendingFreeItemQty>(Details.Cache, null, discountByItem && !isPromotion);

			PXUIFieldAttribute.SetEnabled<DiscountDetail.amount>(Details.Cache, null, breakByAmt && isPromotion);
			PXUIFieldAttribute.SetEnabled<DiscountDetail.pendingAmount>(Details.Cache, null, breakByAmt && !isPromotion);

			PXUIFieldAttribute.SetEnabled<DiscountDetail.quantity>(Details.Cache, null, breakByQty && isPromotion);
			PXUIFieldAttribute.SetEnabled<DiscountDetail.pendingQuantity>(Details.Cache, null, breakByQty && !isPromotion);

			PXUIFieldAttribute.SetEnabled<DiscountDetail.startDate>(Details.Cache, null, !isPromotion); 
                #endregion
            }

		private bool IsFreeItemApplicable(string discountID)
		{
			ARDiscount discount = PXSelect<ARDiscount, Where<ARDiscount.discountID, Equal<Required<ARDiscount.discountID>>>>.Select(this, discountID);

			if (discount == null)
				return true;
			else
			{
                //Free items are valid for group discounts only for now
                if (discount.Type == DiscountType.Group)
				{
					return true;
				}
				else
				{
					return false;
				}
			}
		}
			
		//TODO: reuse NetTools
		internal static string IncNumber(string str, int count)
		{
			int i;
			bool j = true;
			int intcount = count;

			StringBuilder bld = new StringBuilder();
			for (i = str.Length; i > 0; i--)
			{
				string c = str.Substring(i - 1, 1);

				if (System.Text.RegularExpressions.Regex.IsMatch(c, "[^0-9]"))
				{
					j = false;
				}

				if (j && System.Text.RegularExpressions.Regex.IsMatch(c, "[0-9]"))
				{
					int digit = Convert.ToInt16(c);

					string s_count = Convert.ToString(intcount);
					int digit2 = Convert.ToInt16(s_count.Substring(s_count.Length - 1, 1));

					bld.Append((digit + digit2) % 10);

					intcount -= digit2;
					intcount += ((digit + digit2) - (digit + digit2) % 10);

					intcount /= 10;

					if (intcount == 0)
					{
						j = false;
					}
				}
				else
				{
					bld.Append(c);
				}
			}

			if (intcount != 0)
			{
				throw new ArithmeticException("");
			}

			char[] chars = bld.ToString().ToCharArray();
			Array.Reverse(chars);
			return new string(chars);
		}

        public override void Persist()
        {
            ARDiscountEx discount = PXSelect<ARDiscountEx, Where<ARDiscountEx.discountID, Equal<Current<DiscountSequence.discountID>>>>.Select(this);

            if (discount != null && Sequence.Current != null)
            {
                if (discount.IsAutoNumber == true && Sequence.Cache.GetStatus(Sequence.Current) == PXEntryStatus.Inserted)
                {
                    string lastNumber = string.IsNullOrEmpty(discount.LastNumber) ? string.Format("{0}0000", discount.DiscountID) : discount.LastNumber;

                    if (!char.IsDigit(lastNumber[lastNumber.Length - 1]))
                    {
                        lastNumber = string.Format("{0}0000", lastNumber);
                    }

                    discount.LastNumber = IncNumber(lastNumber, 1);
                    Discount.Update(discount);
                }
            }

            base.Persist();
        }

		#region Local Types

        [Serializable]
        public partial class ARDiscountEx : ARDiscount
        {
            #region DiscountID
            public new abstract class discountID : PX.Data.BQL.BqlString.Field<discountID> { }
            #endregion

            #region Tab VisibleExp Support

            #region ShowListOfItems
            public abstract class showListOfItems : PX.Data.BQL.BqlBool.Field<showListOfItems> { }

            [PXBool()]
            [PXUIField(Visibility = PXUIVisibility.Visible)]
            public virtual Boolean? ShowListOfItems
            {
                [PXDependsOnFields(typeof(applicableTo))]
                get
                {
                    return this.ApplicableTo == DiscountTarget.Inventory
                    || this.ApplicableTo == DiscountTarget.CustomerAndInventory
                    || this.ApplicableTo == DiscountTarget.CustomerPriceAndInventory
                    || this.ApplicableTo == DiscountTarget.WarehouseAndInventory;
                }
                set
                {
                }
            }
            #endregion

            #region ShowCustomers
            public abstract class showCustomers : PX.Data.BQL.BqlBool.Field<showCustomers> { }

            [PXBool()]
            [PXUIField(Visibility = PXUIVisibility.Visible)]
            public virtual Boolean? ShowCustomers
            {
                [PXDependsOnFields(typeof(applicableTo))]
                get
                {
                    return this.ApplicableTo == DiscountTarget.Customer
                        || this.ApplicableTo == DiscountTarget.CustomerAndInventory
                        || this.ApplicableTo == DiscountTarget.CustomerAndInventoryPrice
                        || this.ApplicableTo == DiscountTarget.WarehouseAndCustomer
                        || this.ApplicableTo == DiscountTarget.CustomerAndBranch;
                }
                set
                {
                }
            }
            #endregion

            #region ShowCustomerPriceClass
            public abstract class showCustomerPriceClass : PX.Data.BQL.BqlBool.Field<showCustomerPriceClass> { }

            [PXBool()]
            [PXUIField(Visibility = PXUIVisibility.Visible)]
            public virtual Boolean? ShowCustomerPriceClass
            {
                [PXDependsOnFields(typeof(applicableTo))]
                get
                {
                    return this.ApplicableTo == DiscountTarget.CustomerPrice
                        || this.ApplicableTo == DiscountTarget.CustomerPriceAndInventory
                        || this.ApplicableTo == DiscountTarget.CustomerPriceAndInventoryPrice
                        || this.ApplicableTo == DiscountTarget.WarehouseAndCustomerPrice
                        || this.ApplicableTo == DiscountTarget.CustomerPriceAndBranch;
                }
                set
                {
                }
            }
            #endregion

            #region ShowInventoryPriceClass
            public abstract class showInventoryPriceClass : PX.Data.BQL.BqlBool.Field<showInventoryPriceClass> { }

            [PXBool()]
            [PXUIField(Visibility = PXUIVisibility.Visible)]
            public virtual Boolean? ShowInventoryPriceClass
            {
                [PXDependsOnFields(typeof(applicableTo))]
                get
                {
                    return this.ApplicableTo == DiscountTarget.InventoryPrice
                        || this.ApplicableTo == DiscountTarget.CustomerAndInventoryPrice
                        || this.ApplicableTo == DiscountTarget.CustomerPriceAndInventoryPrice
                        || this.ApplicableTo == DiscountTarget.WarehouseAndInventoryPrice;

                }
                set
                {
                }
            }
            #endregion

            #region ShowBranches
            public abstract class showBranches : PX.Data.BQL.BqlBool.Field<showBranches> { }

            [PXBool()]
            [PXUIField(Visibility = PXUIVisibility.Visible)]
            public virtual Boolean? ShowBranches
            {
                [PXDependsOnFields(typeof(applicableTo))]
                get
                {
                    return this.ApplicableTo == DiscountTarget.Branch
                        || this.ApplicableTo == DiscountTarget.CustomerAndBranch
                        || this.ApplicableTo == DiscountTarget.CustomerPriceAndBranch;
                }
                set
                {
                }
            }
            #endregion

            #region ShowSites
            public abstract class showSites : PX.Data.BQL.BqlBool.Field<showSites> { }

            [PXBool()]
            [PXUIField(Visibility = PXUIVisibility.Visible)]
            public virtual Boolean? ShowSites
            {
                [PXDependsOnFields(typeof(applicableTo))]
                get
                {
                    return this.ApplicableTo == DiscountTarget.Warehouse
                        || this.ApplicableTo == DiscountTarget.WarehouseAndCustomer
                        || this.ApplicableTo == DiscountTarget.WarehouseAndCustomerPrice
                        || this.ApplicableTo == DiscountTarget.WarehouseAndInventory
                        || this.ApplicableTo == DiscountTarget.WarehouseAndInventoryPrice;
                }
                set
                {
                }
            }
            #endregion
            #endregion
        }
		
		[Serializable]
		public partial class UpdateSettingsFilter : IBqlTable
		{
			#region FilterDate
			public abstract class filterDate : PX.Data.BQL.BqlDateTime.Field<filterDate> { }
			protected DateTime? _FilterDate;
			[PXDefault(typeof(AccessInfo.businessDate))]
			[PXDate()]
			[PXUIField(DisplayName = "Filter Date", Required = true)]
			public virtual DateTime? FilterDate
			{
				get
				{
					return this._FilterDate;
				}
				set
				{
					this._FilterDate = value;
				}
			}
			#endregion
		}
		#endregion



		#region Well-known extensions
		public DiscountValidation DiscountValidationExt => GetExtension<DiscountValidation>();
		public class DiscountValidation : PXGraphExtension<ARDiscountSequenceMaint>
		{
			public static bool IsActive() => true;

			public virtual IReadOnlyDictionary<string, Func<bool>> DuplicatePredicatesMap { get; }

			public DiscountValidation()
			{
				DuplicatePredicatesMap = new Dictionary<string, Func<bool>>
				{
					[DiscountTarget.Unconditional] = HasUnconditionalDuplicates,
					[DiscountTarget.Customer] = HasCustomerDuplicates,
					[DiscountTarget.Inventory] = HasInventoryDuplicates,
					[DiscountTarget.CustomerPrice] = HasCustomerPriceClassDuplicates,
					[DiscountTarget.InventoryPrice] = HasInventoryPriceClassDuplicates,
					[DiscountTarget.CustomerAndInventory] = HasCustomerAndInventoryComboDuplicates,
					[DiscountTarget.CustomerAndInventoryPrice] = HasCustomerAndInventoryPriceClassComboDuplicates,
					[DiscountTarget.CustomerPriceAndInventory] = HasCustomerPriceClassAndInventoryComboDuplicates,
					[DiscountTarget.CustomerPriceAndInventoryPrice] = HasCustomerPriceClassAndInventoryPriceClassComboDuplicates,
					[DiscountTarget.Branch] = HasBranchDuplicates,
					[DiscountTarget.Warehouse] = HasWarehouseDuplicates,
					[DiscountTarget.WarehouseAndCustomer] = HasWarehouseAndCustomerComboDuplicates,
					[DiscountTarget.WarehouseAndCustomerPrice] = HasWarehouseAndCustomerPriceClassComboDuplicates,
					[DiscountTarget.WarehouseAndInventory] = HasWarehouseAndInventoryComboDuplicates,
					[DiscountTarget.WarehouseAndInventoryPrice] = HasWarehouseAndInventoryPriceClassComboDuplicates
				};
			}

			public override void Initialize()
			{
				base.Initialize();
				Base.OnBeforeCommit += OnBeforeCommit;
			}

			private void OnBeforeCommit(PXGraph g)
			{
				if (HasDuplicates())
					throw new PXException(Messages.DiscountsNotvalid);
			}

			public virtual bool HasDuplicates()
			{
				if (Base.Discount.Current != null && Base.Sequence.Current != null && Base.Sequence.Current.IsActive == true && DuplicatePredicatesMap.ContainsKey(Base.Discount.Current.ApplicableTo))
				{
					return DuplicatePredicatesMap[Base.Discount.Current.ApplicableTo].Invoke();
				}

				return false;
			}

			private bool HasUnconditionalDuplicates()
			{
				bool hasDuplicates = false;
				PXSelectBase<DiscountSequence> duplicatesSelect = GetDuplicatesSelectCommand();
				foreach (PXResult<DiscountSequence> duplicate in duplicatesSelect.SelectWindowed(0, 1))
				{
					hasDuplicates |= Invalidate<DiscountSequence, DiscountSequence.discountSequenceID>(Base.Sequence, duplicate, Messages.UnconditionalDiscUniqueConstraint);
				}

				return hasDuplicates;
			}

			public virtual bool HasCustomerDuplicates()
			{
				bool hasDuplicates = false;
				PXSelectBase<DiscountSequence> duplicatesSelect = GetDuplicatesSelectCommand().With(ApplyCustomer);
				foreach (PXResult<DiscountSequence, DiscountCustomer> duplicate in duplicatesSelect.Select())
				{
					hasDuplicates |= Invalidate<DiscountCustomer, DiscountCustomer.customerID>(Base.Customers, duplicate, Messages.UniqueItemConstraint);
				}

				return hasDuplicates;
			}

			public virtual bool HasInventoryDuplicates()
			{
				bool hasDuplicates = false;
				PXSelectBase<DiscountSequence> duplicatesSelect = GetDuplicatesSelectCommand().With(ApplyInventory);
				foreach (PXResult<DiscountSequence, DiscountItem> duplicate in duplicatesSelect.Select())
				{
					hasDuplicates |= Invalidate<DiscountItem, DiscountItem.inventoryID>(Base.Items, duplicate, Messages.UniqueItemConstraint);
				}

				return hasDuplicates;
			}

			public virtual bool HasCustomerPriceClassDuplicates()
			{
				bool hasDuplicatess = false;
				PXSelectBase<DiscountSequence> duplicatesSelect = GetDuplicatesSelectCommand().With(ApplyCustomerPriceClass);
				foreach (PXResult<DiscountSequence, DiscountCustomerPriceClass> duplicate in duplicatesSelect.Select())
				{
					hasDuplicatess |= Invalidate<DiscountCustomerPriceClass, DiscountCustomerPriceClass.customerPriceClassID>(Base.CustomerPriceClasses, duplicate, Messages.UniqueItemConstraint);
				}

				return hasDuplicatess;
			}

			public virtual bool HasInventoryPriceClassDuplicates()
			{
				bool hasDuplicates = false;
				PXSelectBase<DiscountSequence> duplicatesSelect = GetDuplicatesSelectCommand().With(ApplyInventoryPriceClass);
				foreach (PXResult<DiscountSequence, DiscountInventoryPriceClass> duplicate in duplicatesSelect.Select())
				{
					hasDuplicates |= Invalidate<DiscountInventoryPriceClass, DiscountInventoryPriceClass.inventoryPriceClassID>(Base.InventoryPriceClasses, duplicate, Messages.UniqueItemConstraint);
				}

				return hasDuplicates;
			}

			public virtual bool HasBranchDuplicates()
			{
				bool hasDuplicates = false;
				PXSelectBase<DiscountSequence> duplicatesSelect = GetDuplicatesSelectCommand().With(ApplyBranch);
				foreach (PXResult<DiscountSequence, DiscountBranch> duplicate in duplicatesSelect.Select())
				{
					hasDuplicates |= Invalidate<DiscountBranch, DiscountBranch.branchID>(Base.Branches, duplicate, Messages.UniqueBranchConstraint);
				}

				return hasDuplicates;
			}

			public virtual bool HasWarehouseDuplicates()
			{
				bool hasDuplicates = false;
				PXSelectBase<DiscountSequence> duplicatesSelect = GetDuplicatesSelectCommand().With(ApplyWarehouse);
				foreach (PXResult<DiscountSequence, DiscountSite> duplicate in duplicatesSelect.Select())
				{
					hasDuplicates |= Invalidate<DiscountSite, DiscountSite.siteID>(Base.Sites, duplicate, Messages.UniqueWarehouseConstraint);
				}

				return hasDuplicates;
			}

			public virtual bool HasCustomerAndInventoryComboDuplicates()
			{
				bool hasDuplicates = false;
				PXSelectBase<DiscountSequence> duplicatesSelect = GetDuplicatesSelectCommand().With(ApplyCustomer).With(ApplyInventory);
				foreach (PXResult<DiscountSequence, DiscountCustomer, DiscountItem> duplicate in duplicatesSelect.Select())
				{
					hasDuplicates |= Invalidate<DiscountCustomer, DiscountCustomer.customerID>(Base.Customers, duplicate, Messages.UniqueItemConstraint);
					hasDuplicates |= Invalidate<DiscountItem, DiscountItem.inventoryID>(Base.Items, duplicate, Messages.UniqueItemConstraint);
				}

				return hasDuplicates;
			}

			public virtual bool HasCustomerAndInventoryPriceClassComboDuplicates()
			{
				bool hasDuplicates = false;
				PXSelectBase<DiscountSequence> duplicatesSelect = GetDuplicatesSelectCommand().With(ApplyCustomer).With(ApplyInventoryPriceClass);
				foreach (PXResult<DiscountSequence, DiscountCustomer, DiscountInventoryPriceClass> duplicate in duplicatesSelect.Select())
				{
					hasDuplicates |= Invalidate<DiscountCustomer, DiscountCustomer.customerID>(Base.Customers, duplicate, Messages.UniqueItemConstraint);
					hasDuplicates |= Invalidate<DiscountInventoryPriceClass, DiscountInventoryPriceClass.inventoryPriceClassID>(Base.InventoryPriceClasses, duplicate, Messages.UniqueItemConstraint);
				}

				return hasDuplicates;
			}

			public virtual bool HasCustomerPriceClassAndInventoryComboDuplicates()
			{
				bool hasDuplicates = false;
				PXSelectBase<DiscountSequence> duplicatesSelect = GetDuplicatesSelectCommand().With(ApplyCustomerPriceClass).With(ApplyInventory);
				foreach (PXResult<DiscountSequence, DiscountCustomerPriceClass, DiscountItem> duplicate in duplicatesSelect.Select())
				{
					hasDuplicates |= Invalidate<DiscountCustomerPriceClass, DiscountCustomerPriceClass.customerPriceClassID>(Base.CustomerPriceClasses, duplicate, Messages.UniqueItemConstraint);
					hasDuplicates |= Invalidate<DiscountItem, DiscountItem.inventoryID>(Base.Items, duplicate, Messages.UniqueItemConstraint);
				}

				return hasDuplicates;
			}

			public virtual bool HasCustomerPriceClassAndInventoryPriceClassComboDuplicates()
			{
				bool hasDuplicates = false;
				PXSelectBase<DiscountSequence> duplicatesSelect = GetDuplicatesSelectCommand().With(ApplyCustomerPriceClass).With(ApplyInventoryPriceClass);
				foreach (PXResult<DiscountSequence, DiscountCustomerPriceClass, DiscountInventoryPriceClass> duplicate in duplicatesSelect.Select())
				{
					hasDuplicates |= Invalidate<DiscountCustomerPriceClass, DiscountCustomerPriceClass.customerPriceClassID>(Base.CustomerPriceClasses, duplicate, Messages.UniqueItemConstraint);
					hasDuplicates |= Invalidate<DiscountInventoryPriceClass, DiscountInventoryPriceClass.inventoryPriceClassID>(Base.InventoryPriceClasses, duplicate, Messages.UniqueItemConstraint);
				}

				return hasDuplicates;
			}

			public virtual bool HasWarehouseAndCustomerComboDuplicates()
			{
				bool hasDuplicates = false;
				PXSelectBase<DiscountSequence> duplicatesSelect = GetDuplicatesSelectCommand().With(ApplyWarehouse).With(ApplyCustomer);
				foreach (PXResult<DiscountSequence, DiscountSite, DiscountCustomer> duplicate in duplicatesSelect.Select())
				{
					hasDuplicates |= Invalidate<DiscountSite, DiscountSite.siteID>(Base.Sites, duplicate, Messages.UniqueWarehouseConstraint);
					hasDuplicates |= Invalidate<DiscountCustomer, DiscountCustomer.customerID>(Base.Customers, duplicate, Messages.UniqueItemConstraint);
				}

				return hasDuplicates;
			}

			public virtual bool HasWarehouseAndCustomerPriceClassComboDuplicates()
			{
				bool hasDuplicates = false;
				PXSelectBase<DiscountSequence> duplicatesSelect = GetDuplicatesSelectCommand().With(ApplyWarehouse).With(ApplyCustomerPriceClass);
				foreach (PXResult<DiscountSequence, DiscountSite, DiscountCustomerPriceClass> duplicate in duplicatesSelect.Select())
				{
					hasDuplicates |= Invalidate<DiscountSite, DiscountSite.siteID>(Base.Sites, duplicate, Messages.UniqueWarehouseConstraint);
					hasDuplicates |= Invalidate<DiscountCustomerPriceClass, DiscountCustomerPriceClass.customerPriceClassID>(Base.CustomerPriceClasses, duplicate, Messages.UniqueItemConstraint);
				}

				return hasDuplicates;
			}

			public virtual bool HasWarehouseAndInventoryComboDuplicates()
			{
				bool hasDuplicates = false;
				PXSelectBase<DiscountSequence> duplicatesSelect = GetDuplicatesSelectCommand().With(ApplyWarehouse).With(ApplyInventory);
				foreach (PXResult<DiscountSequence, DiscountSite, DiscountItem> duplicate in duplicatesSelect.Select())
				{
					hasDuplicates |= Invalidate<DiscountSite, DiscountSite.siteID>(Base.Sites, duplicate, Messages.UniqueWarehouseConstraint);
					hasDuplicates |= Invalidate<DiscountItem, DiscountItem.inventoryID>(Base.Items, duplicate, Messages.UniqueItemConstraint);
				}

				return hasDuplicates;
			}

			public virtual bool HasWarehouseAndInventoryPriceClassComboDuplicates()
			{
				bool hasDuplicates = false;
				PXSelectBase<DiscountSequence> duplicatesSelect = GetDuplicatesSelectCommand().With(ApplyWarehouse).With(ApplyInventoryPriceClass);
				foreach (PXResult<DiscountSequence, DiscountSite, DiscountInventoryPriceClass> duplicate in duplicatesSelect.Select())
				{
					hasDuplicates |= Invalidate<DiscountSite, DiscountSite.siteID>(Base.Sites, duplicate, Messages.UniqueWarehouseConstraint);
					hasDuplicates |= Invalidate<DiscountInventoryPriceClass, DiscountInventoryPriceClass.inventoryPriceClassID>(Base.InventoryPriceClasses, duplicate, Messages.UniqueItemConstraint);
				}

				return hasDuplicates;
			}

			public PXSelectBase<DiscountSequence> GetDuplicatesSelectCommand()
			{
				var cmd =
					new PXSelectReadonly<DiscountSequence,
					Where<DiscountSequence.isActive, Equal<True>,
						And<DiscountSequence.discountID, Equal<Current<DiscountSequence.discountID>>,
						And<DiscountSequence.discountSequenceID, NotEqual<Current<DiscountSequence.discountSequenceID>>>>>>
						(Base);

				if (Base.Sequence.Current.IsPromotion == true)
					cmd.WhereAnd<
						Where<DiscountSequence.isPromotion, Equal<True>,
						And<Where2<Where<
							DiscountSequence.startDate, Between<Current<DiscountSequence.startDate>, Current<DiscountSequence.endDate>>>,
							Or<DiscountSequence.endDate, Between<Current<DiscountSequence.startDate>, Current<DiscountSequence.endDate>>>>>>>();
				else
					cmd.WhereAnd<Where<DiscountSequence.isPromotion, Equal<False>>>();

				return cmd;
			}

			public PXSelectBase<DiscountSequence> ApplyCustomer(PXSelectBase<DiscountSequence> cmd)
			{
				cmd.Join<InnerJoin<DiscountCustomer, On<DiscountCustomer.DiscountSequenceFK>>>();
				cmd.WhereAnd<Where<DiscountCustomer.customerID, In2<Search<DiscountCustomer.customerID, Where<DiscountCustomer.DiscountSequenceFK.SameAsCurrent>>>>>();
				return cmd;
			}

			public PXSelectBase<DiscountSequence> ApplyInventory(PXSelectBase<DiscountSequence> cmd)
			{
				cmd.Join<InnerJoin<DiscountItem, On<DiscountItem.DiscountSequenceFK>>>();
				cmd.WhereAnd<Where<DiscountItem.inventoryID, In2<Search<DiscountItem.inventoryID, Where<DiscountItem.DiscountSequenceFK.SameAsCurrent>>>>>();
				return cmd;
			}

			public PXSelectBase<DiscountSequence> ApplyBranch(PXSelectBase<DiscountSequence> cmd)
			{
				cmd.Join<InnerJoin<DiscountBranch, On<DiscountBranch.DiscountSequenceFK>>>();
				cmd.WhereAnd<Where<DiscountBranch.branchID, In2<Search<DiscountBranch.branchID, Where<DiscountBranch.DiscountSequenceFK.SameAsCurrent>>>>>();
				return cmd;
			}

			public PXSelectBase<DiscountSequence> ApplyWarehouse(PXSelectBase<DiscountSequence> cmd)
			{
				cmd.Join<InnerJoin<DiscountSite, On<DiscountSite.DiscountSequenceFK>>>();
				cmd.WhereAnd<Where<DiscountSite.siteID, In2<Search<DiscountSite.siteID, Where<DiscountSite.DiscountSequenceFK.SameAsCurrent>>>>>();
				return cmd;
			}

			public PXSelectBase<DiscountSequence> ApplyCustomerPriceClass(PXSelectBase<DiscountSequence> cmd)
			{
				cmd.Join<InnerJoin<DiscountCustomerPriceClass, On<DiscountCustomerPriceClass.DiscountSequenceFK>>>();
				cmd.WhereAnd<Where<DiscountCustomerPriceClass.customerPriceClassID, In2<Search<DiscountCustomerPriceClass.customerPriceClassID, Where<DiscountCustomerPriceClass.DiscountSequenceFK.SameAsCurrent>>>>>();
				return cmd;
			}

			public PXSelectBase<DiscountSequence> ApplyInventoryPriceClass(PXSelectBase<DiscountSequence> cmd)
			{
				cmd.Join<InnerJoin<DiscountInventoryPriceClass, On<DiscountInventoryPriceClass.DiscountSequenceFK>>>();
				cmd.WhereAnd<Where<DiscountInventoryPriceClass.inventoryPriceClassID, In2<Search<DiscountInventoryPriceClass.inventoryPriceClassID, Where<DiscountInventoryPriceClass.DiscountSequenceFK.SameAsCurrent>>>>>();
				return cmd;
			}

			public bool Invalidate<TEntity, TKeyField>(PXSelectBase<TEntity> view, TEntity entity, string errorMsg)
				where TEntity : class, IBqlTable, new()
				where TKeyField : class, IBqlField
			{
				view.Cache.SetValue<DiscountSequence.discountSequenceID>(entity, Base.Sequence.Current.DiscountSequenceID);
				TEntity locatedEntity = (TEntity)view.Cache.Locate(entity) ?? view.Select().RowCast<TEntity>().FirstOrDefault(r => view.Cache.ObjectsEqual(r, entity));
				if (locatedEntity != null && view.Cache.GetStatus(locatedEntity) != PXEntryStatus.Deleted)
				{
					view.Cache.RaiseExceptionHandling<TKeyField>(
						locatedEntity,
						(string)(PXStringState)view.Cache.GetStateExt<TKeyField>(locatedEntity),
						new PXSetPropertyException(errorMsg, PXErrorLevel.Error));

					return true;
				}
		
				return false;
			}
		}
		#endregion
	}
}
