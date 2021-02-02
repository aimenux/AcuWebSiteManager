using System;
using System.Collections;
using System.Collections.Generic;
using PX.SM;
using PX.Data;
using PX.Objects.AR;
using System.Linq;
using PX.Objects.Common.Discount;
using PX.Data.ReferentialIntegrity.Attributes;

namespace PX.Objects.AP
{

    public class APDiscountSequenceMaint : PXGraph<APDiscountSequenceMaint>
    {
        public APDiscountSequenceMaint()
        {
            PXDBDefaultAttribute.SetDefaultForInsert<DiscountSequence.discountSequenceID>(Sequence.Cache, null, false);
        }

        #region Selects/Views
        public PXSelect<APDiscountEx, Where<APDiscountEx.discountID, Equal<Current<VendorDiscountSequence.discountID>>>> Discount;

        public PXSelect<VendorDiscountSequence> Sequence;

        public PXSelect<DiscountDetail, Where<DiscountDetail.discountID, Equal<Current<VendorDiscountSequence.discountID>>,
            And<DiscountDetail.discountSequenceID, Equal<Current<VendorDiscountSequence.discountSequenceID>>>>, OrderBy<Asc<DiscountDetail.quantity, Asc<DiscountDetail.amount>>>> Details;

        public PXSelect<DiscountSequenceDetail, Where<DiscountSequenceDetail.discountID, Equal<Current<VendorDiscountSequence.discountID>>,
            And<DiscountSequenceDetail.discountSequenceID, Equal<Current<VendorDiscountSequence.discountSequenceID>>>>, OrderBy<Asc<DiscountSequenceDetail.quantity, Asc<DiscountSequenceDetail.amount>>>> SequenceDetails;

        [PXImport(typeof(VendorDiscountSequence))]
        public PXSelectJoin<DiscountItem,
            InnerJoin<IN.InventoryItem, On<IN.InventoryItem.inventoryID, Equal<DiscountItem.inventoryID>>>,
            Where<DiscountItem.discountID, Equal<Current<VendorDiscountSequence.discountID>>,
            And<DiscountItem.discountSequenceID, Equal<Current<VendorDiscountSequence.discountSequenceID>>>>> Items;

        public PXSelectJoin<DiscountInventoryPriceClass,
            InnerJoin<IN.INPriceClass, On<DiscountInventoryPriceClass.inventoryPriceClassID, Equal<IN.INPriceClass.priceClassID>>>,
            Where<DiscountInventoryPriceClass.discountID, Equal<Current<VendorDiscountSequence.discountID>>,
            And<DiscountInventoryPriceClass.discountSequenceID, Equal<Current<VendorDiscountSequence.discountSequenceID>>>>> InventoryPriceClasses;

        [PXImport(typeof(VendorDiscountSequence))]
        public PXSelectJoin<APDiscountLocation,
            InnerJoin<CR.Location, On<CR.Location.locationID, Equal<APDiscountLocation.locationID>>>,
            Where<APDiscountLocation.discountID, Equal<Current<VendorDiscountSequence.discountID>>,
            And<APDiscountLocation.discountSequenceID, Equal<Current<VendorDiscountSequence.discountSequenceID>>, And<APDiscountLocation.vendorID, Equal<Current<VendorDiscountSequence.vendorID>>>>>> Locations;

        public PXSelect<APDiscountVendor> Vendors;

        public PXFilter<UpdateSettingsFilter> UpdateSettings;

        #endregion

        #region Buttons/Actions

        public PXSave<VendorDiscountSequence> Save;

        [PXCancelButton]
        [PXUIField(DisplayName = ActionsMessages.Cancel, MapEnableRights = PXCacheRights.Select)]
        protected virtual IEnumerable Cancel(PXAdapter a)
        {
            VendorDiscountSequence current = null;
            string discountID = null;
            string sequenceID = null;
            string vendorID = null;
			#region Extract Keys
			if (a.Searches != null)
                {
                    if (a.Searches.Length > 0)
                        vendorID = (string)a.Searches[0];
                    if (a.Searches.Length > 1)
                        discountID = (string)a.Searches[1];
                    if (a.Searches.Length > 2)
                        sequenceID = (string)a.Searches[2];
                }
                #endregion

                APDiscount seq1 = PXSelectJoin<APDiscount, InnerJoin<Vendor, On<Vendor.bAccountID, Equal<APDiscount.bAccountID>>>,
                   Where<Vendor.acctCD, Equal<Required<Vendor.acctCD>>,
                   And<APDiscount.discountID, Equal<Required<APDiscount.discountID>>>>>.Select(this, vendorID, discountID);
                if (seq1 == null)
                {
                    if (a.Searches != null && a.Searches.Length > 1)
                    {
                        a.Searches[1] = null;
                        discountID = null;
                    }
                    if (a.Searches != null && a.Searches.Length > 2)
                        a.Searches[2] = null;
                }

                VendorDiscountSequence seq = PXSelect<VendorDiscountSequence,
                    Where<VendorDiscountSequence.discountSequenceID, Equal<Required<VendorDiscountSequence.discountSequenceID>>,
                    And<VendorDiscountSequence.discountID, Equal<Required<VendorDiscountSequence.discountID>>>>>.Select(this, sequenceID, discountID);

                APDiscount discount = PXSelect<APDiscount, Where<APDiscount.discountID, Equal<Required<APDiscount.discountID>>>>.Select(this, discountID);

                bool insertNewSequence = false;
                if (seq == null)
                {
                    if (a.Searches != null && a.Searches.Length > 2)
                    {
                        a.Searches[2] = null;
                    }
                    if (discountID != null)
                        insertNewSequence = true;
                }

                if (Discount.Current != null && Discount.Current.DiscountID != discountID)
                {
                    sequenceID = null;
                }

                foreach (VendorDiscountSequence headerCanceled in (new PXCancel<VendorDiscountSequence>(this, "Cancel")).Press(a))
                {
                    current = headerCanceled;
                }

                if (insertNewSequence)
                {
                    Sequence.Cache.Remove(current);

                    VendorDiscountSequence newSeq = new VendorDiscountSequence();
                    newSeq.DiscountID = discountID;

                    Vendor ven = PXSelect<Vendor, Where<Vendor.acctCD, Equal<Required<Vendor.acctCD>>>>.SelectWindowed(this, 0, 1, vendorID);

                    newSeq.VendorID = ven.BAccountID;

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
                            current.DiscountSequenceID = PXMessages.LocalizeNoPrefix(Messages.NewKey);

                        Sequence.Cache.Normalize();
                    }
                }
                if (seq != null && seq.Description != null)
                    current.Description = seq.Description;

                yield return current;
            
        }

        public PXAction<VendorDiscountSequence> cancel;
        public PXInsert<VendorDiscountSequence> Insert;
        public PXDelete<VendorDiscountSequence> Delete;
        public PXFirst<VendorDiscountSequence> First;
        public PXPrevious<VendorDiscountSequence> Prev;
        public PXNext<VendorDiscountSequence> Next;
        public PXLast<VendorDiscountSequence> Last;

        public PXAction<VendorDiscountSequence> updateDiscounts;
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

                    //PXLongOperation.StartOperation(this, delegate() { SO.SOUpdateDiscounts.UpdateDiscount(Sequence.Current.DiscountID, Sequence.Current.DiscountSequenceID, UpdateSettings.Current.FilterDate); });
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

        protected virtual void VendorDiscountSequence_IsPromotion_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            VendorDiscountSequence row = e.Row as VendorDiscountSequence;
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
        protected virtual void VendorDiscountSequence_DiscountedFor_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            PXUIFieldAttribute.SetEnabled<VendorDiscountSequence.prorate>(sender, e.Row, ((VendorDiscountSequence)e.Row).DiscountedFor == DiscountOption.Amount);
        }

        protected virtual void VendorDiscountSequence_DiscountID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            VendorDiscountSequence row = e.Row as VendorDiscountSequence;
            APDiscount discount = PXSelect<APDiscount, Where<APDiscount.discountID, Equal<Required<APDiscount.discountID>>>>.Select(this, row.DiscountID);

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
                PXResult<DiscountDetail> prevDetailLine = PXSelect<DiscountDetail, Where<DiscountDetail.discountID, Equal<Current<VendorDiscountSequence.discountID>>,
                And<DiscountDetail.discountSequenceID, Equal<Current<VendorDiscountSequence.discountSequenceID>>, And<DiscountDetail.amount, Less<Required<DiscountDetail.amount>>, And<DiscountDetail.isActive, Equal<True>>>>>, OrderBy<Desc<DiscountDetail.amount>>>.SelectSingleBound(this, null, row.Amount);
                if (prevDetailLine != null)
                {
                    DiscountDetail prevLine = (DiscountDetail)prevDetailLine;
                    prevLine.AmountTo = row.Amount;
                    Details.Update(prevLine);
                }
                PXResult<DiscountDetail> nextDetailLine = PXSelect<DiscountDetail, Where<DiscountDetail.discountID, Equal<Current<VendorDiscountSequence.discountID>>,
                And<DiscountDetail.discountSequenceID, Equal<Current<VendorDiscountSequence.discountSequenceID>>, And<DiscountDetail.amount, Greater<Required<DiscountDetail.amount>>, And<DiscountDetail.isActive, Equal<True>>>>>, OrderBy<Asc<DiscountDetail.amount>>>.SelectSingleBound(this, null, row.Amount);
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
                PXResult<DiscountDetail> prevDetailLine = PXSelect<DiscountDetail, Where<DiscountDetail.discountID, Equal<Current<VendorDiscountSequence.discountID>>,
                And<DiscountDetail.discountSequenceID, Equal<Current<VendorDiscountSequence.discountSequenceID>>, And<DiscountDetail.quantity, Less<Required<DiscountDetail.quantity>>, And<DiscountDetail.isActive, Equal<True>>>>>, OrderBy<Desc<DiscountDetail.quantity>>>.SelectSingleBound(this, null, row.Quantity);
                if (prevDetailLine != null)
                {
                    DiscountDetail prevLine = (DiscountDetail)prevDetailLine;
                    prevLine.QuantityTo = row.Quantity;
                    Details.Update(prevLine);
                }
                PXResult<DiscountDetail> nextDetailLine = PXSelect<DiscountDetail, Where<DiscountDetail.discountID, Equal<Current<VendorDiscountSequence.discountID>>,
                And<DiscountDetail.discountSequenceID, Equal<Current<VendorDiscountSequence.discountSequenceID>>, And<DiscountDetail.quantity, Greater<Required<DiscountDetail.quantity>>, And<DiscountDetail.isActive, Equal<True>>>>>, OrderBy<Asc<DiscountDetail.quantity>>>.SelectSingleBound(this, null, row.Quantity);
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

        protected virtual void VendorDiscountSequence_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
        {
			VendorDiscountSequence row = e.Row as VendorDiscountSequence;
            if (row == null) return;

			Details.Cache.AllowInsert =
			Details.Cache.AllowUpdate =
			Details.Cache.AllowDelete = row.VendorID != null && row.DiscountID != null && row.DiscountSequenceID != null;

			Items.Cache.AllowInsert =
			Items.Cache.AllowUpdate =
			Items.Cache.AllowDelete = row.VendorID != null && row.DiscountID != null && row.DiscountSequenceID != null;

			InventoryPriceClasses.Cache.AllowInsert =
			InventoryPriceClasses.Cache.AllowUpdate =
			InventoryPriceClasses.Cache.AllowDelete = row.VendorID != null && row.DiscountID != null && row.DiscountSequenceID != null;

			Locations.Cache.AllowInsert =
			Locations.Cache.AllowUpdate =
			Locations.Cache.AllowDelete = row.VendorID != null && row.DiscountID != null && row.DiscountSequenceID != null;

            APDiscount discount = PXSelect<APDiscount, Where<APDiscount.discountID, Equal<Required<APDiscount.discountID>>>>.Select(this, row.DiscountID);
            SetControlsState(sender, row, discount);
            SetGridColumnsState(discount);

            Dictionary<string, string> allowed = new DiscountOption.ListAttribute().ValueLabelDic;
            allowed.Remove(DiscountOption.FreeItem);
            PXStringListAttribute.SetList<VendorDiscountSequence.discountedFor>(sender, row,
                                                           allowed.Keys.ToArray(),
                                                           allowed.Values.ToArray());
        }

        protected virtual void VendorDiscountSequence_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
        {
            VendorDiscountSequence row = e.Row as VendorDiscountSequence;

            if (row != null)
            {
                if (row.IsPromotion == true && row.EndDate == null)
                {
                    if (sender.RaiseExceptionHandling<VendorDiscountSequence.endDate>(e.Row, null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty)))
                    {
                        throw new PXRowPersistingException(typeof(VendorDiscountSequence.endDate).Name, null, ErrorMessages.FieldIsEmpty);
                    }
                }
                if (row.IsPromotion == true && row.EndDate != null && row.StartDate != null && row.EndDate < row.StartDate)
                {
					if (sender.RaiseExceptionHandling<DiscountSequence.endDate>(e.Row, row.EndDate, new PXSetPropertyException(AR.Messages.EffectiveDateExpirationDate)))
                    {
						throw new PXRowPersistingException(typeof(DiscountSequence.endDate).Name, row.EndDate, AR.Messages.EffectiveDateExpirationDate);
                    }
                }
            }

            if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert)
            {
                APDiscount discount = Discount.Current;
                if (discount != null)
                {
                    PXDBDefaultAttribute.SetDefaultForInsert<VendorDiscountSequence.discountSequenceID>(sender, e.Row, discount.IsAutoNumber == true);
                }
            }

        }

        protected virtual void VendorDiscountSequence_DiscountSequenceID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            if (e.Row != null)
            {
                APDiscount discount = PXSelect<APDiscount, Where<APDiscount.discountID, Equal<Required<VendorDiscountSequence.discountID>>>>.Select(this, ((VendorDiscountSequence)e.Row).DiscountID);
                if (discount != null && discount.IsAutoNumber == true)
                {
                    e.NewValue = PXMessages.LocalizeNoPrefix(Messages.NewKey);
                }
            }
        }
        
        protected virtual void DiscountDetail_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
        {
			DiscountDetail currentLine = e.Row as DiscountDetail;
			if (currentLine != null)
			{
				if (!sender.ObjectsEqual<DiscountDetail.isActive>(e.Row, e.OldRow))
				{
					//update hidden line with "last" values
					PXResult<DiscountSequenceDetail> LastDiscountDetailLine = PXSelect<DiscountSequenceDetail, Where<DiscountSequenceDetail.discountID, Equal<Current<VendorDiscountSequence.discountID>>,
						And<DiscountSequenceDetail.discountSequenceID, Equal<Current<VendorDiscountSequence.discountSequenceID>>, And<DiscountSequenceDetail.discountDetailsID, NotEqual<Required<DiscountSequenceDetail.discountDetailsID>>,
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
                PXResult<DiscountSequenceDetail> LastDiscountDetailLine = PXSelect<DiscountSequenceDetail, Where<DiscountSequenceDetail.discountID, Equal<Current<VendorDiscountSequence.discountID>>,
                    And<DiscountSequenceDetail.discountSequenceID, Equal<Current<VendorDiscountSequence.discountSequenceID>>, And<DiscountSequenceDetail.discountDetailsID, NotEqual<Required<DiscountSequenceDetail.discountDetailsID>>,
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
			return PXSelect<DiscountDetail, Where<DiscountDetail.discountID, Equal<Current<VendorDiscountSequence.discountID>>,
					And<DiscountDetail.discountSequenceID, Equal<Current<VendorDiscountSequence.discountSequenceID>>, And<DiscountDetail.quantity, Greater<Required<DiscountDetail.quantity>>, And<DiscountDetail.isActive, Equal<True>>>>>, OrderBy<Asc<DiscountDetail.quantity>>>.SelectSingleBound(this, null, currentLine.Quantity);
		}

		public virtual DiscountDetail GetPrevDiscountDetailLineQuantity(DiscountDetail currentLine)
		{
			return PXSelect<DiscountDetail, Where<DiscountDetail.discountID, Equal<Current<VendorDiscountSequence.discountID>>,
						And<DiscountDetail.discountSequenceID, Equal<Current<VendorDiscountSequence.discountSequenceID>>, And<DiscountDetail.quantity, Less<Required<DiscountDetail.quantity>>, And<DiscountDetail.isActive, Equal<True>>>>>, OrderBy<Desc<DiscountDetail.quantity>>>.SelectSingleBound(this, null, currentLine.Quantity);
		}

		public virtual DiscountDetail GetNextDiscountDetailLineAmount(DiscountDetail currentLine)
		{
			return PXSelect<DiscountDetail, Where<DiscountDetail.discountID, Equal<Current<VendorDiscountSequence.discountID>>,
					And<DiscountDetail.discountSequenceID, Equal<Current<VendorDiscountSequence.discountSequenceID>>, And<DiscountDetail.amount, Greater<Required<DiscountDetail.amount>>, And<DiscountDetail.isActive, Equal<True>>>>>, OrderBy<Asc<DiscountDetail.amount>>>.SelectSingleBound(this, null, currentLine.Amount);
		}

		public virtual DiscountDetail GetPrevDiscountDetailLineAmount(DiscountDetail currentLine)
		{
			return PXSelect<DiscountDetail, Where<DiscountDetail.discountID, Equal<Current<VendorDiscountSequence.discountID>>,
						And<DiscountDetail.discountSequenceID, Equal<Current<VendorDiscountSequence.discountSequenceID>>, And<DiscountDetail.amount, Less<Required<DiscountDetail.amount>>, And<DiscountDetail.isActive, Equal<True>>>>>, OrderBy<Desc<DiscountDetail.amount>>>.SelectSingleBound(this, null, currentLine.Amount);
        }

        #endregion

        #region Validation

        private bool RunVerification()
        {
            if (Discount.Current != null && Sequence.Current != null && Sequence.Current.IsActive == true)
            {
                switch (Discount.Current.ApplicableTo)
                {
                    case DiscountTarget.Unconditional:
                        return VerifyUnconditional();
                    case DiscountTarget.VendorAndInventory:
                        return VerifyItem();
                    case DiscountTarget.VendorAndInventoryPrice:
                        return VerifyInventoryPriceClass();
                    case DiscountTarget.VendorLocation:
                        return VerifyLocation();
                    case DiscountTarget.VendorLocationAndInventory:
                        return VerifyCombination_Location_Inventory();
                }
            }

            return true;
        }

        private bool VerifyUnconditional()
        {
            bool success = true;

            if (!IsUncoditionalValid())
            {
                success = false;
                Sequence.Cache.RaiseExceptionHandling<VendorDiscountSequence.discountSequenceID>(Sequence.Current, Sequence.Current.DiscountSequenceID, new PXSetPropertyException(AR.Messages.UnconditionalDiscUniqueConstraint, PXErrorLevel.Error));
            }

            return success;
        }

        private bool IsUncoditionalValid()
        {
            //Check for duplicates in other sequences that overlap with current:
            VendorDiscountSequence existing = null;

            if (Sequence.Current.IsPromotion == true)
            {
                #region Search duplicates in promotional sequences

                existing = PXSelectReadonly<VendorDiscountSequence,
                                    Where<VendorDiscountSequence.isActive, Equal<True>,
                                    And<VendorDiscountSequence.discountID, Equal<Current<VendorDiscountSequence.discountID>>,
                                    And<VendorDiscountSequence.discountSequenceID, NotEqual<Current<VendorDiscountSequence.discountSequenceID>>,
                                    And<VendorDiscountSequence.isPromotion, Equal<True>,
                                        And<Where2<Where<
                                                VendorDiscountSequence.startDate, Between<Current<VendorDiscountSequence.startDate>, Current<VendorDiscountSequence.endDate>>>,
                                                Or<VendorDiscountSequence.endDate, Between<Current<VendorDiscountSequence.startDate>, Current<VendorDiscountSequence.endDate>>>>>
                                        >>>>>.SelectWindowed(this, 0, 1);

                if (existing != null)
                {
                    return false;
                }

                #endregion
            }
            else
            {
                #region Search duplicates in NON promotional sequences

                existing = PXSelectReadonly<VendorDiscountSequence,
                                        Where<VendorDiscountSequence.isActive, Equal<True>,
                                        And<VendorDiscountSequence.discountID, Equal<Current<VendorDiscountSequence.discountID>>,
                                        And<VendorDiscountSequence.discountSequenceID, NotEqual<Current<VendorDiscountSequence.discountSequenceID>>,
                                        And<VendorDiscountSequence.isPromotion, Equal<False>>>>>>.SelectWindowed(this, 0, 1);

                if (existing != null)
                {
                    return false;
                }

                #endregion
            }

            return true;

        }       

        private bool VerifyItem()
        {
            bool success = true;

            foreach (PXResult<DiscountItem, IN.InventoryItem> row in Items.Select())
            {
                if (Items.Cache.GetStatus((DiscountItem)row) != PXEntryStatus.Deleted)
                {
                    if (!VerifyItem(((DiscountItem)row).InventoryID))
                    {
                        success = false;
                        Items.Cache.RaiseExceptionHandling<DiscountItem.inventoryID>((DiscountItem)row, ((IN.InventoryItem)row).InventoryCD, new PXSetPropertyException(AR.Messages.UniqueItemConstraint, PXErrorLevel.Error));
                    }
                }
            }

            return success;
        }

        private bool VerifyItem(int? inventoryID)
        {
            //Check for duplicates in other sequences that overlap with current:
            DiscountItem existing = null;

            if (Sequence.Current.IsPromotion == true)
            {
                #region Search duplicates in promotional sequences

                existing = PXSelectReadonly2<DiscountItem,
                                        InnerJoin<VendorDiscountSequence, On<DiscountItem.discountSequenceID, Equal<VendorDiscountSequence.discountSequenceID>,
                                            And<VendorDiscountSequence.discountSequenceID, NotEqual<Current<VendorDiscountSequence.discountSequenceID>>>>>,
                                        Where<DiscountItem.discountID, Equal<Current<VendorDiscountSequence.discountID>>,
                                        And<DiscountItem.inventoryID, Equal<Required<DiscountItem.inventoryID>>,
                                        And<VendorDiscountSequence.isActive, Equal<True>,
                                        And<VendorDiscountSequence.discountID, Equal<Current<VendorDiscountSequence.discountID>>,
                                        And<VendorDiscountSequence.isPromotion, Equal<True>,
                                        And<Where2<Where<
                                                VendorDiscountSequence.startDate, Between<Current<VendorDiscountSequence.startDate>, Current<VendorDiscountSequence.endDate>>>,
                                                Or<VendorDiscountSequence.endDate, Between<Current<VendorDiscountSequence.startDate>, Current<VendorDiscountSequence.endDate>>>>>
                                        >>>>>>.SelectWindowed(this, 0, 1, inventoryID);


                if (existing != null)
                {
                    return false;
                }

                #endregion
            }
            else
            {
                #region Search duplicates in NON promotional sequences

                existing = PXSelectReadonly2<DiscountItem,
                                        InnerJoin<VendorDiscountSequence, On<DiscountItem.discountSequenceID, Equal<VendorDiscountSequence.discountSequenceID>,
                                            And<VendorDiscountSequence.discountSequenceID, NotEqual<Current<VendorDiscountSequence.discountSequenceID>>>>>,
                                        Where<DiscountItem.discountID, Equal<Current<VendorDiscountSequence.discountID>>,
                                        And<DiscountItem.inventoryID, Equal<Required<DiscountItem.inventoryID>>,
                                        And<VendorDiscountSequence.isActive, Equal<True>,
                                        And<VendorDiscountSequence.discountID, Equal<Current<VendorDiscountSequence.discountID>>,
                                        And<VendorDiscountSequence.isPromotion, Equal<False>>>>>>>.SelectWindowed(this, 0, 1, inventoryID);

                if (existing != null)
                {
                    return false;
                }

                #endregion
            }

            return true;
        }

        private bool VerifyInventoryPriceClass()
        {
            bool success = true;

            foreach (DiscountInventoryPriceClass row in InventoryPriceClasses.Select())
            {
                if (InventoryPriceClasses.Cache.GetStatus(row) != PXEntryStatus.Deleted)
                {
                    if (!VerifyInventoryPriceClass(row.InventoryPriceClassID))
                    {
                        success = false;
                        InventoryPriceClasses.Cache.RaiseExceptionHandling<DiscountInventoryPriceClass.inventoryPriceClassID>(row, null, new PXSetPropertyException(AR.Messages.UniqueItemConstraint, PXErrorLevel.Error));
                    }
                }
            }

            return success;
        }

        private bool VerifyInventoryPriceClass(string priceClassID)
        {
            //Check for duplicates in other sequences that overlap with current:
            DiscountInventoryPriceClass existing = null;

            if (Sequence.Current.IsPromotion == true)
            {
                #region Search duplicates in promotional sequences

                existing = PXSelectReadonly2<DiscountInventoryPriceClass,
                                        InnerJoin<VendorDiscountSequence, On<DiscountInventoryPriceClass.discountSequenceID, Equal<VendorDiscountSequence.discountSequenceID>,
                                            And<VendorDiscountSequence.discountSequenceID, NotEqual<Current<VendorDiscountSequence.discountSequenceID>>>>>,
                                        Where<DiscountInventoryPriceClass.discountID, Equal<Current<VendorDiscountSequence.discountID>>,
                                        And<DiscountInventoryPriceClass.inventoryPriceClassID, Equal<Required<DiscountInventoryPriceClass.inventoryPriceClassID>>,
                                        And<VendorDiscountSequence.isActive, Equal<True>,
                                        And<VendorDiscountSequence.discountID, Equal<Current<VendorDiscountSequence.discountID>>,
                                        And<VendorDiscountSequence.isPromotion, Equal<True>,
                                        And<Where2<Where<
                                                VendorDiscountSequence.startDate, Between<Current<VendorDiscountSequence.startDate>, Current<VendorDiscountSequence.endDate>>>,
                                                Or<VendorDiscountSequence.endDate, Between<Current<VendorDiscountSequence.startDate>, Current<VendorDiscountSequence.endDate>>>>>
                                        >>>>>>.SelectWindowed(this, 0, 1, priceClassID);


                if (existing != null)
                {
                    return false;
                }

                #endregion
            }
            else
            {
                #region Search duplicates in NON promotional sequences

                existing = PXSelectReadonly2<DiscountInventoryPriceClass,
                                        InnerJoin<VendorDiscountSequence, On<DiscountInventoryPriceClass.discountSequenceID, Equal<VendorDiscountSequence.discountSequenceID>,
                                            And<VendorDiscountSequence.discountSequenceID, NotEqual<Current<VendorDiscountSequence.discountSequenceID>>>>>,
                                        Where<DiscountInventoryPriceClass.discountID, Equal<Current<VendorDiscountSequence.discountID>>,
                                        And<DiscountInventoryPriceClass.inventoryPriceClassID, Equal<Required<DiscountInventoryPriceClass.inventoryPriceClassID>>,
                                        And<VendorDiscountSequence.isActive, Equal<True>,
                                        And<VendorDiscountSequence.discountID, Equal<Current<VendorDiscountSequence.discountID>>,
                                        And<VendorDiscountSequence.isPromotion, Equal<False>>>>>>>.SelectWindowed(this, 0, 1, priceClassID);

                if (existing != null)
                {
                    return false;
                }

                #endregion
            }

            return true;
        }

        private bool VerifyLocation()
        {
            bool success = true;

            foreach (PXResult<APDiscountLocation, CR.Location> row in Locations.Select())
            {
                if (Locations.Cache.GetStatus((APDiscountLocation)row) != PXEntryStatus.Deleted)
                {
                    if (!VerifyLocation(((APDiscountLocation)row).LocationID))
                    {
                        success = false;
                        Locations.Cache.RaiseExceptionHandling<APDiscountLocation.locationID>((APDiscountLocation)row, ((CR.Location)row).LocationID, new PXSetPropertyException(AR.Messages.UniqueItemConstraint, PXErrorLevel.Error));
                    }
                }
            }

            return success;
        }

        private bool VerifyLocation(int? locationID)
        {
            //Check for duplicates in other sequences that overlap with current:
            APDiscountLocation existing = null;

            if (Sequence.Current.IsPromotion == true)
            {
                #region Search duplicates in promotional sequences

                existing = PXSelectReadonly2<APDiscountLocation,
                                        InnerJoin<VendorDiscountSequence, On<APDiscountLocation.discountSequenceID, Equal<VendorDiscountSequence.discountSequenceID>,
                                            And<VendorDiscountSequence.discountSequenceID, NotEqual<Current<VendorDiscountSequence.discountSequenceID>>>>>,
                                        Where<APDiscountLocation.discountID, Equal<Current<VendorDiscountSequence.discountID>>,And<APDiscountLocation.vendorID, Equal<Current<VendorDiscountSequence.vendorID>>,
                                        And<APDiscountLocation.locationID, Equal<Required<APDiscountLocation.locationID>>,
                                        And<VendorDiscountSequence.isActive, Equal<True>,
                                        And<VendorDiscountSequence.discountID, Equal<Current<VendorDiscountSequence.discountID>>,
                                        And<VendorDiscountSequence.isPromotion, Equal<True>,
                                        And<Where2<Where<
                                                VendorDiscountSequence.startDate, Between<Current<VendorDiscountSequence.startDate>, Current<VendorDiscountSequence.endDate>>>,
                                                Or<VendorDiscountSequence.endDate, Between<Current<VendorDiscountSequence.startDate>, Current<VendorDiscountSequence.endDate>>>>>>
                                        >>>>>>.SelectWindowed(this, 0, 1, locationID);


                if (existing != null)
                {
                    return false;
                }

                #endregion
            }
            else
            {
                #region Search duplicates in NON promotional sequences

                existing = PXSelectReadonly2<APDiscountLocation,
                                        InnerJoin<VendorDiscountSequence, On<APDiscountLocation.discountSequenceID, Equal<VendorDiscountSequence.discountSequenceID>,
                                            And<VendorDiscountSequence.discountSequenceID, NotEqual<Current<VendorDiscountSequence.discountSequenceID>>>>>,
                                        Where<APDiscountLocation.discountID, Equal<Current<VendorDiscountSequence.discountID>>,And<APDiscountLocation.vendorID, Equal<Current<VendorDiscountSequence.vendorID>>,
                                        And<APDiscountLocation.locationID, Equal<Required<APDiscountLocation.locationID>>,
                                        And<VendorDiscountSequence.isActive, Equal<True>,
                                        And<VendorDiscountSequence.discountID, Equal<Current<VendorDiscountSequence.discountID>>,
                                        And<VendorDiscountSequence.isPromotion, Equal<False>>>>>>>>.SelectWindowed(this, 0, 1, locationID);

                if (existing != null)
                {
                    return false;
                }

                #endregion
            }

            return true;
        }

        private bool VerifyCombination_Location_Inventory()
        {
            bool success = true;

            foreach (PXResult<DiscountItem, IN.InventoryItem> item in Items.Select())
            {
                if (Items.Cache.GetStatus((DiscountItem)item) != PXEntryStatus.Deleted)
                {
                    foreach (PXResult<APDiscountLocation, CR.Location> location in Locations.Select())
                    {
                        if (Locations.Cache.GetStatus((APDiscountLocation)location) != PXEntryStatus.Deleted)
                        {
                            if (!VerifyCombination_Location_Inventory(((APDiscountLocation)location).LocationID, ((APDiscountLocation)location).VendorID, ((DiscountItem)item).InventoryID))
                            {
                                success = false;
                                Locations.Cache.RaiseExceptionHandling<APDiscountLocation.locationID>((APDiscountLocation)location, ((CR.Location)location).LocationCD, new PXSetPropertyException(AR.Messages.UniqueItemConstraint, PXErrorLevel.Error));
                                Items.Cache.RaiseExceptionHandling<DiscountItem.inventoryID>((DiscountItem)item, ((IN.InventoryItem)item).InventoryCD, new PXSetPropertyException(AR.Messages.UniqueItemConstraint, PXErrorLevel.Error));
                            }
                        }
                    }
                }

            }

            return success;
        }

        private bool VerifyCombination_Location_Inventory(int? locationID,int? vendorID, int? itemID)
        {
            APDiscountLocation existing = null;

            if (Sequence.Current.IsPromotion == true)
            {
                #region Search duplicates in promotional sequences
                existing = PXSelectReadonly2<APDiscountLocation,
                InnerJoin<DiscountItem, On<APDiscountLocation.discountID, Equal<DiscountItem.discountID>,
                    And<APDiscountLocation.discountSequenceID, Equal<DiscountItem.discountSequenceID>>>,
                InnerJoin<DiscountSequence, On<APDiscountLocation.discountID, Equal<DiscountSequence.discountID>,
                    And<APDiscountLocation.discountSequenceID, Equal<DiscountSequence.discountSequenceID>,
                    And<DiscountSequence.discountSequenceID, NotEqual<Current<DiscountSequence.discountSequenceID>>>>>, 
                InnerJoin<APDiscountVendor, On<APDiscountLocation.vendorID, Equal<APDiscountVendor.vendorID>,
                    And<APDiscountLocation.discountID, Equal<APDiscountVendor.discountID>,
                    And<APDiscountLocation.discountSequenceID, Equal<APDiscountVendor.discountSequenceID>>>>>>>,

                Where<APDiscountLocation.locationID, Equal<Required<APDiscountLocation.locationID>>,
                And<APDiscountVendor.vendorID, Equal<Required<APDiscountVendor.vendorID>>,
                And<DiscountItem.inventoryID, Equal<Required<DiscountItem.inventoryID>>,
                And<DiscountSequence.isActive, Equal<True>,
                And<DiscountSequence.discountID, Equal<Current<DiscountSequence.discountID>>,
                And<DiscountSequence.isPromotion, Equal<True>,
                                                And<Where2<Where<
                                                        DiscountSequence.startDate, Between<Current<DiscountSequence.startDate>, Current<DiscountSequence.endDate>>>,
                                                        Or<DiscountSequence.endDate, Between<Current<DiscountSequence.startDate>, Current<DiscountSequence.endDate>>>>>
                                                >>>>>>>.SelectWindowed(this, 0, 1, locationID,vendorID, itemID);
                if (existing != null)
                {
                    return false;
                }

                #endregion
            }
            else
            {
                #region Search duplicates in NON promotional sequences

                existing = PXSelectReadonly2<APDiscountLocation,
                InnerJoin<DiscountItem, On<APDiscountLocation.discountID, Equal<DiscountItem.discountID>,
                    And<APDiscountLocation.discountSequenceID, Equal<DiscountItem.discountSequenceID>>>,
                InnerJoin<DiscountSequence, On<APDiscountLocation.discountID, Equal<DiscountSequence.discountID>,
                    And<APDiscountLocation.discountSequenceID, Equal<DiscountSequence.discountSequenceID>,
                    And<DiscountSequence.discountSequenceID, NotEqual<Current<DiscountSequence.discountSequenceID>>>>>,
                InnerJoin<APDiscountVendor, On<APDiscountLocation.vendorID, Equal<APDiscountVendor.vendorID>,
                    And<APDiscountLocation.discountID, Equal<APDiscountVendor.discountID>,
                    And<APDiscountLocation.discountSequenceID, Equal<APDiscountVendor.discountSequenceID>>>>>>>,

                Where<APDiscountLocation.locationID, Equal<Required<APDiscountLocation.locationID>>,
                And<APDiscountVendor.vendorID, Equal<Required<APDiscountVendor.vendorID>>,
                And<DiscountItem.inventoryID, Equal<Required<DiscountItem.inventoryID>>,
                And<DiscountSequence.isActive, Equal<True>,
                And<DiscountSequence.discountID, Equal<Current<DiscountSequence.discountID>>,
                And<DiscountSequence.discountID, Equal<Current<DiscountSequence.discountID>>,
                And<DiscountSequence.isPromotion, Equal<False>>>>>>>>>.SelectWindowed(this, 0, 1, locationID,vendorID, itemID);

                if (existing != null)
                {
                    return false;
                }

                #endregion
            }

            return true;
        }

       
        #endregion

        private void SetControlsState(PXCache sender, VendorDiscountSequence row, APDiscount discount)
        {
            if (row != null)
            {
                updateDiscounts.SetEnabled(!(sender.GetStatus(row) == PXEntryStatus.Inserted && discount != null && discount.IsAutoNumber == true) && row.IsPromotion != true);

                PXUIFieldAttribute.SetEnabled<VendorDiscountSequence.endDate>(sender, row, row.IsPromotion == true);
                PXUIFieldAttribute.SetRequired<VendorDiscountSequence.endDate>(sender, row.IsPromotion == true);
                PXUIFieldAttribute.SetEnabled<VendorDiscountSequence.startDate>(sender, row, row.IsPromotion == true);
                PXUIFieldAttribute.SetRequired<VendorDiscountSequence.startDate>(sender, row.IsPromotion == true);
                PXUIFieldAttribute.SetVisible<VendorDiscountSequence.endDate>(sender, row, row.IsPromotion == true);
                PXUIFieldAttribute.SetVisible<VendorDiscountSequence.startDate>(sender, row, row.IsPromotion == true);
                PXUIFieldAttribute.SetEnabled<VendorDiscountSequence.prorate>(sender,row, row.DiscountedFor == DiscountOption.Amount);

                PXUIFieldAttribute.SetEnabled<VendorDiscountSequence.breakBy>(sender, row, (discount != null && (discount.Type == DiscountType.Group || discount.Type == DiscountType.Line)));
            }
        }

        private void SetGridColumnsState(APDiscount discount)
        {
            if (Sequence.Current != null)
            {
                #region Show All Columns
                PXUIFieldAttribute.SetVisible<DiscountDetail.amountTo>(Details.Cache, null, true);
                PXUIFieldAttribute.SetVisible<DiscountDetail.amount>(Details.Cache, null, true);
                PXUIFieldAttribute.SetVisible<DiscountDetail.pendingAmount>(Details.Cache, null, true);
                PXUIFieldAttribute.SetVisible<DiscountDetail.lastAmount>(Details.Cache, null, true);
                PXUIFieldAttribute.SetVisible<DiscountDetail.quantityTo>(Details.Cache, null, true);
                PXUIFieldAttribute.SetVisible<DiscountDetail.quantity>(Details.Cache, null, true);
                PXUIFieldAttribute.SetVisible<DiscountDetail.pendingQuantity>(Details.Cache, null, true);
                PXUIFieldAttribute.SetVisible<DiscountDetail.lastQuantity>(Details.Cache, null, true);
                PXUIFieldAttribute.SetVisible<DiscountDetail.discount>(Details.Cache, null, true);
                PXUIFieldAttribute.SetVisible<DiscountDetail.lastDiscount>(Details.Cache, null, true);
                PXUIFieldAttribute.SetVisible<DiscountDetail.pendingDiscount>(Details.Cache, null, true);
                PXUIFieldAttribute.SetVisible<DiscountDetail.discountPercent>(Details.Cache, null, true);
                PXUIFieldAttribute.SetVisible<DiscountDetail.lastDiscountPercent>(Details.Cache, null, true);
                PXUIFieldAttribute.SetVisible<DiscountDetail.pendingDiscountPercent>(Details.Cache, null, true);
                PXUIFieldAttribute.SetVisible<DiscountDetail.freeItemQty>(Details.Cache, null, true);
                PXUIFieldAttribute.SetVisible<DiscountDetail.pendingFreeItemQty>(Details.Cache, null, true);
                PXUIFieldAttribute.SetVisible<DiscountDetail.lastFreeItemQty>(Details.Cache, null, true);

                PXUIFieldAttribute.SetVisible<DiscountItem.amount>(Items.Cache, null, true);
                PXUIFieldAttribute.SetVisible<DiscountItem.quantity>(Items.Cache, null, true);
                PXUIFieldAttribute.SetVisible<DiscountItem.uOM>(Items.Cache, null, true);

                #endregion

                #region Hide Selective Columns

                if (Sequence.Current.DiscountedFor != DiscountOption.FreeItem)
                {
                    PXUIFieldAttribute.SetVisible<DiscountDetail.freeItemQty>(Details.Cache, null, false);
                    PXUIFieldAttribute.SetVisible<DiscountDetail.lastFreeItemQty>(Details.Cache, null, false);
                    PXUIFieldAttribute.SetVisible<DiscountDetail.pendingFreeItemQty>(Details.Cache, null, false);
                }
                if (Sequence.Current.BreakBy == BreakdownType.Quantity)
                {
                    PXUIFieldAttribute.SetVisible<DiscountDetail.amount>(Details.Cache, null, false);
                    PXUIFieldAttribute.SetVisible<DiscountDetail.amountTo>(Details.Cache, null, false);
                    PXUIFieldAttribute.SetVisible<DiscountDetail.pendingAmount>(Details.Cache, null, false);
                    PXUIFieldAttribute.SetVisible<DiscountDetail.lastAmount>(Details.Cache, null, false);
                    PXUIFieldAttribute.SetVisible<DiscountItem.amount>(Items.Cache, null, false);
                }
                else
                {
                    PXUIFieldAttribute.SetVisible<DiscountDetail.quantity>(Details.Cache, null, false);
                    PXUIFieldAttribute.SetVisible<DiscountDetail.quantityTo>(Details.Cache, null, false);
                    PXUIFieldAttribute.SetVisible<DiscountDetail.pendingQuantity>(Details.Cache, null, false);
                    PXUIFieldAttribute.SetVisible<DiscountDetail.lastQuantity>(Details.Cache, null, false);

                    PXUIFieldAttribute.SetVisible<DiscountItem.quantity>(Items.Cache, null, false);
                    PXUIFieldAttribute.SetVisible<DiscountItem.uOM>(Items.Cache, null, false);
                }

                if (Sequence.Current.IsPromotion == true)
                {
                    PXUIFieldAttribute.SetVisible<DiscountDetail.lastAmount>(Details.Cache, null, false);
                    PXUIFieldAttribute.SetVisible<DiscountDetail.lastDiscount>(Details.Cache, null, false);
                    PXUIFieldAttribute.SetVisible<DiscountDetail.lastDiscountPercent>(Details.Cache, null, false);
                    PXUIFieldAttribute.SetVisible<DiscountDetail.lastFreeItemQty>(Details.Cache, null, false);
                    PXUIFieldAttribute.SetVisible<DiscountDetail.lastQuantity>(Details.Cache, null, false);
                    PXUIFieldAttribute.SetVisible<DiscountDetail.pendingAmount>(Details.Cache, null, false);
                    PXUIFieldAttribute.SetVisible<DiscountDetail.pendingDiscount>(Details.Cache, null, false);
                    PXUIFieldAttribute.SetVisible<DiscountDetail.pendingDiscountPercent>(Details.Cache, null, false);
                    PXUIFieldAttribute.SetVisible<DiscountDetail.pendingFreeItemQty>(Details.Cache, null, false);
                    PXUIFieldAttribute.SetVisible<DiscountDetail.pendingQuantity>(Details.Cache, null, false);
                    PXUIFieldAttribute.SetVisible<DiscountDetail.startDate>(Details.Cache, null, false);
                    PXUIFieldAttribute.SetVisible<DiscountDetail.lastDate>(Details.Cache, null, false);

                }

                if (Sequence.Current.DiscountedFor == DiscountOption.Amount)
                {
                    PXUIFieldAttribute.SetVisible<DiscountDetail.discountPercent>(Details.Cache, null, false);
                    PXUIFieldAttribute.SetVisible<DiscountDetail.lastDiscountPercent>(Details.Cache, null, false);
                    PXUIFieldAttribute.SetVisible<DiscountDetail.pendingDiscountPercent>(Details.Cache, null, false);
                }
                if (Sequence.Current.DiscountedFor == DiscountOption.Percent)
                {
                    PXUIFieldAttribute.SetVisible<DiscountDetail.discount>(Details.Cache, null, false);
                    PXUIFieldAttribute.SetVisible<DiscountDetail.lastDiscount>(Details.Cache, null, false);
                    PXUIFieldAttribute.SetVisible<DiscountDetail.pendingDiscount>(Details.Cache, null, false);
                }
                if (Sequence.Current.DiscountedFor == DiscountOption.FreeItem)
                {
                    PXUIFieldAttribute.SetVisible<DiscountDetail.discount>(Details.Cache, null, false);
                    PXUIFieldAttribute.SetVisible<DiscountDetail.lastDiscount>(Details.Cache, null, false);
                    PXUIFieldAttribute.SetVisible<DiscountDetail.pendingDiscount>(Details.Cache, null, false);
                    PXUIFieldAttribute.SetVisible<DiscountDetail.discountPercent>(Details.Cache, null, false);
                    PXUIFieldAttribute.SetVisible<DiscountDetail.lastDiscountPercent>(Details.Cache, null, false);
                    PXUIFieldAttribute.SetVisible<DiscountDetail.pendingDiscountPercent>(Details.Cache, null, false);
                }

                #endregion

                #region Enable Columns

                if (Sequence.Current.IsPromotion == true)
                {
                    PXUIFieldAttribute.SetEnabled<DiscountDetail.amount>(Details.Cache, null, true);
                    PXUIFieldAttribute.SetEnabled<DiscountDetail.quantity>(Details.Cache, null, true);
                    PXUIFieldAttribute.SetEnabled<DiscountDetail.discount>(Details.Cache, null, true);
                    PXUIFieldAttribute.SetEnabled<DiscountDetail.discountPercent>(Details.Cache, null, true);
                    PXUIFieldAttribute.SetEnabled<DiscountDetail.freeItemQty>(Details.Cache, null, true);
                }

                #endregion
            }
        }

        //TODO: reuse NetTools
        private static string IncNumber(string str, int count)
        {
            int i;
            bool j = true;
            int intcount = count;

            System.Text.StringBuilder bld = new System.Text.StringBuilder();
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
            if (!RunVerification())
            {
                throw new PXException(AR.Messages.DiscountsNotvalid);
            }

            APDiscountEx discount = PXSelect<APDiscountEx, Where<APDiscountEx.discountID, Equal<Current<VendorDiscountSequence.discountID>>>>.Select(this);

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
            APDiscountVendor ven = (APDiscountVendor)PXSelect<APDiscountVendor,
                                                        Where<APDiscountVendor.discountID, Equal<Current<VendorDiscountSequence.discountID>>,
                                                            And<APDiscountVendor.discountSequenceID, Equal<Current<VendorDiscountSequence.discountSequenceID>>,
                                                            And<APDiscountVendor.vendorID, Equal<Current<VendorDiscountSequence.vendorID>>>>>>.Select(this);
            if (ven == null&&Sequence.Current!=null) 
            {
                ven = new APDiscountVendor();
                ven.VendorID = Sequence.Current.VendorID;
                ven.DiscountID = Sequence.Current.DiscountID;
                ven.DiscountSequenceID = Sequence.Current.DiscountSequenceID;
                Vendors.Update(ven);
            }
            base.Persist();
        }

        #region Local Types

        [Serializable]
        [PXHidden]
        public partial class APDiscountEx : APDiscount
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
                    return this.ApplicableTo == DiscountTarget.VendorAndInventory
                    || this.ApplicableTo == DiscountTarget.VendorLocationAndInventory;
                }
                set
                {
                }
            }
            #endregion

            #region ShowLocations
            public abstract class showLocations : PX.Data.BQL.BqlBool.Field<showLocations> { }

            [PXBool()]
            [PXUIField(Visibility = PXUIVisibility.Visible)]
            public virtual Boolean? ShowLocations
            {
                [PXDependsOnFields(typeof(applicableTo))]
                get
                {
                    return this.ApplicableTo == DiscountTarget.VendorLocation
                    || this.ApplicableTo == DiscountTarget.VendorLocationAndInventory;
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
                    return this.ApplicableTo == DiscountTarget.VendorAndInventoryPrice;

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
    }

  
    [PXProjection(typeof(Select2<DiscountSequence, InnerJoin<APDiscount, On<DiscountSequence.discountID, Equal<APDiscount.discountID>>>>), new Type[] { typeof(DiscountSequence) })]
    [PXPrimaryGraph(typeof(APDiscountSequenceMaint))]
	[Serializable]
    public partial class VendorDiscountSequence : DiscountSequence
    {
        public abstract class vendorID : PX.Data.BQL.BqlInt.Field<vendorID> { }
        protected int? _VendorID;
        [Vendor( IsKey = true, BqlField = typeof(APDiscount.bAccountID))]
        public virtual int? VendorID
        {
            get
            {
                return this._VendorID;
            }
            set
            {
                this._VendorID = value;
            }
        }
        #region DiscountID
        public new abstract class discountID : PX.Data.BQL.BqlString.Field<discountID> { }
        [PXDBString(10, IsUnicode = true, IsKey = true, InputMask = ">aaaaaaaaaa")]
        [PXDefault()]
        [PXSelector(typeof(Search<APDiscount.discountID, Where<APDiscount.bAccountID, Equal<Current<VendorDiscountSequence.vendorID>>>>))]
        [PXUIField(DisplayName = "Discount Code", Visibility = PXUIVisibility.SelectorVisible)]
        [PXParent(typeof(Select<APDiscount, Where<APDiscount.discountID, Equal<Current<VendorDiscountSequence.discountID>>>>))]
		[PXReferentialIntegrityCheck]
        [PX.Data.EP.PXFieldDescription]
        public override String DiscountID
        {
            get
            {
                return this._DiscountID;
            }
            set
            {
                this._DiscountID = value;
            }
        }
        #endregion
        #region DiscountSequenceID
        public new abstract class discountSequenceID : PX.Data.BQL.BqlString.Field<discountSequenceID> { }
        [PXDBString(10, IsUnicode = true, IsKey = true, InputMask = ">CCCCCCCCCC")]
        [PXDBDefault(typeof(APDiscount.lastNumber), DefaultForUpdate = false)]
        [PXUIField(DisplayName = "Sequence", Visibility = PXUIVisibility.SelectorVisible, Required=true)]
        [PXSelector(typeof(Search<VendorDiscountSequence.discountSequenceID, Where<VendorDiscountSequence.discountID, Equal<Current<VendorDiscountSequence.discountID>>>>))]
        [PX.Data.EP.PXFieldDescription]
        public override String DiscountSequenceID
        {
            get
            {
                return this._DiscountSequenceID;
            }
            set
            {
                this._DiscountSequenceID = value;
            }
        }
        #endregion
        #region LineCntr
        public new abstract class lineCntr : PX.Data.BQL.BqlInt.Field<lineCntr> { }
        #endregion
        #region Description
        public new abstract class description : PX.Data.BQL.BqlString.Field<description> { }
        #endregion
        #region DiscountedFor
        public new abstract class discountedFor : PX.Data.BQL.BqlString.Field<discountedFor> { }
        #endregion
        #region BreakBy
        public new abstract class breakBy : PX.Data.BQL.BqlString.Field<breakBy> { }
        #endregion
        #region IsPromotion
        public new abstract class isPromotion : PX.Data.BQL.BqlBool.Field<isPromotion> { }
        #endregion
        #region IsActive
        public new abstract class isActive : PX.Data.BQL.BqlBool.Field<isActive> { }
        #endregion
        #region ProrateDiscounts
        public new abstract class prorate : PX.Data.BQL.BqlBool.Field<prorate> { }
        #endregion
        #region StartDate
        public new abstract class startDate : PX.Data.BQL.BqlDateTime.Field<startDate> { }
        #endregion
        #region EndDate
        public new abstract class endDate : PX.Data.BQL.BqlDateTime.Field<endDate> { }
        #endregion
        #region UpdateDate
        public new abstract class updateDate : PX.Data.BQL.BqlDateTime.Field<updateDate> { }
        #endregion
        #region FreeItemID
        public new abstract class freeItemID : PX.Data.BQL.BqlInt.Field<freeItemID> { }
        #endregion
        #region PendingFreeItemID
        public new abstract class pendingFreeItemID : PX.Data.BQL.BqlInt.Field<pendingFreeItemID> { }
        #endregion
        #region LastFreeItemID
        public new abstract class lastFreeItemID : PX.Data.BQL.BqlInt.Field<lastFreeItemID> { }
        #endregion
        #region NoteID
        public new abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
        #endregion

        #region System Columns
        #region tstamp
        public new abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }
        [PXDBTimestamp()]
        public override Byte[] tstamp
        {
            get
            {
                return this._tstamp;
            }
            set
            {
                this._tstamp = value;
            }
        }
        #endregion
        #region CreatedByID
        public new abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
        #endregion
        #region CreatedByScreenID
        public new abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }
        #endregion
        #region CreatedDateTime
        public new abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
        #endregion
        #region LastModifiedByID
        public new abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
        #endregion
        #region LastModifiedByScreenID
        public new abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
        #endregion
        #region LastModifiedDateTime
        public new abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
        #endregion
        #endregion
    }






}