using System.Collections;
using System.Linq;
using PX.Data;
using PX.Objects.AR;
using PX.Objects.CS;
using PX.Objects.Common.Discount;
using System;
using System.Collections.Generic;

namespace PX.Objects.AP
{
    public class APDiscountMaint : PXGraph<APDiscountMaint, Vendor>
    {
        public PXSelect<Vendor> Filter;
        public PXSelect<Vendor, Where<Vendor.bAccountID, Equal<Current<Vendor.bAccountID>>>> CurrentVendor;
        public PXSelect<APDiscount, Where<APDiscount.bAccountID, Equal<Current<Vendor.bAccountID>>>> CurrentDiscounts;
		public PXSelect<VendorDiscountSequence> DiscountSequences;

        public APDiscountMaint()
        {
            this.Filter.Cache.AllowInsert = false;
            this.Filter.Cache.AllowDelete = false;

			foreach (var childType in DiscountChildTypes())
				Views.Caches.Add(childType);

			PXParentAttribute.SetLeaveChildren<DiscountSequenceDetail.discountSequenceID>(this.Caches<DiscountSequenceDetail>(), null, false);
		}

		protected virtual IEnumerable<Type> DiscountChildTypes()
		{
			yield return typeof(DiscountSequence);
			yield return typeof(APDiscountVendor);
			yield return typeof(APDiscountLocation);
			yield return typeof(DiscountInventoryPriceClass);
			yield return typeof(DiscountItem);
			yield return typeof(DiscountSequenceDetail);
		}

        public PXAction<Vendor> viewAPDiscountSequence;
        [PXUIField(DisplayName = Messages.ViewAPDiscountSequence, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = false)]
        [PXLookupButton]
        public virtual IEnumerable ViewAPDiscountSequence(PXAdapter adapter)
        {
            if (this.Filter.Current != null && this.CurrentDiscounts.Current != null)
            {
                if (this.Filter.Current.BAccountID != null && this.CurrentDiscounts.Current.DiscountID != null)
                {
                    APDiscountSequenceMaint apDiscountSequence = PXGraph.CreateInstance<APDiscountSequenceMaint>();
                    apDiscountSequence.Sequence.Current = new VendorDiscountSequence();
                    apDiscountSequence.Sequence.Current.VendorID = this.Filter.Current.BAccountID;
                    apDiscountSequence.Sequence.Current.DiscountID = this.CurrentDiscounts.Current.DiscountID;
                    throw new PXRedirectRequiredException(apDiscountSequence, Messages.ViewAPDiscountSequence);
                }
            }
            return adapter.Get();
        }

        protected virtual void Vendor_RowSelecting(PXCache sender, PXRowSelectingEventArgs e)
        {
            if (e.Row != null)
            {
                if (((Vendor)e.Row).LineDiscountTarget == null)
                    Filter.Cache.SetDefaultExt<Vendor.lineDiscountTarget>(e.Row);
                if (((Vendor)e.Row).IgnoreConfiguredDiscounts == null)
                    Filter.Cache.SetDefaultExt<Vendor.ignoreConfiguredDiscounts>(e.Row);
            }
        }

        protected virtual void APDiscount_ApplicableTo_FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
        {
            APDiscount row = e.Row as APDiscount;
            string[] allowedValues;
            string[] allowedLabels;

            if (row != null)
            {
                if (row.Type == DiscountType.Document)
                {
                    allowedValues = new[] {DiscountTarget.Vendor};
                    allowedLabels = new[] {Messages.VendorUnconditional}
                        .Select(PXMessages.LocalizeNoPrefix)
                        .ToArray();

                    e.ReturnState = PXStringState.CreateInstance(
						e.ReturnValue, 
						1, 
						false, 
						nameof(APDiscount.ApplicableTo), 
						false, 
						1, 
						null,
						allowedValues,
						allowedLabels,
						true, 
						DiscountTarget.Unconditional);

					return;
                }
            }

            if (!PXAccess.FeatureInstalled<FeaturesSet.accountLocations>())
            {
                allowedValues = new[]
                {
                    DiscountTarget.VendorAndInventory,
                    DiscountTarget.VendorAndInventoryPrice,
                    DiscountTarget.Vendor
                };
                allowedLabels = new[]
                    {
                        Messages.VendorAndInventory,
                        Messages.VendorInventoryPrice,
                        Messages.VendorUnconditional
                    }
                    .Select(PXMessages.LocalizeNoPrefix)
                    .ToArray();

                e.ReturnState = PXStringState.CreateInstance(
					e.ReturnValue, 
					1, 
					false, 
					nameof(APDiscount.ApplicableTo), 
					false, 
					1, 
					null,
                    allowedValues, 
                    allowedLabels,
					true, 
					DiscountTarget.Inventory);

                return;
            }

			allowedValues = new[]
			{
				DiscountTarget.VendorAndInventory,
				DiscountTarget.VendorAndInventoryPrice,
				DiscountTarget.VendorLocation,
				DiscountTarget.VendorLocationAndInventory,
				DiscountTarget.Vendor
			};
			allowedLabels = new[]
			{
				Messages.VendorAndInventory,
				Messages.VendorInventoryPrice,
				Messages.Vendor_Location,
				Messages.VendorLocationaAndInventory,
				Messages.VendorUnconditional
			}
			.Select(PXMessages.LocalizeNoPrefix)
			.ToArray();

			e.ReturnState = PXStringState.CreateInstance(
							   e.ReturnValue,
							   1,
							   false,
							   nameof(APDiscount.ApplicableTo),
							   false,
							   1,
							   null,
							   allowedValues,
							   allowedLabels,
							   true,
							   DiscountTarget.Inventory);
		}


        protected virtual void APDiscount_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
        {
            APDiscount row = e.Row as APDiscount;
            if (row != null)
            {
                if (e.Operation == PXDBOperation.Insert || e.Operation == PXDBOperation.Update)
                {
                    if (PXSelectReadonly<ARDiscount, Where<ARDiscount.discountID, Equal<Required<ARDiscount.discountID>>>>.Select(this, row.DiscountID).Count != 0)
                    {
                        sender.RaiseExceptionHandling<APDiscount.discountID>(row, row.DiscountID, new PXSetPropertyException(Messages.DiscountCodeAlreadyExistAR, PXErrorLevel.Error));
                    }
                    if ((row.Type == DiscountType.Line || row.Type == DiscountType.Document) && row.SkipDocumentDiscounts == true) row.SkipDocumentDiscounts = false;
                    if ((row.Type == DiscountType.Group || row.Type == DiscountType.Document) && row.ExcludeFromDiscountableAmt == true) row.ExcludeFromDiscountableAmt = false;
                }
                if (e.Operation == PXDBOperation.Insert)
                {
                    if (PXSelectReadonly<APDiscount, Where<APDiscount.discountID, Equal<Required<APDiscount.discountID>>>>.Select(this, row.DiscountID).Count != 0)
                    {
                        sender.RaiseExceptionHandling<APDiscount.discountID>(row, row.DiscountID, new PXSetPropertyException(Messages.DiscountCodeAlreadyExist, PXErrorLevel.Error));
                    }
                }
            }
        }

        protected virtual void APDiscount_RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
        {
            DiscountEngine.GetDiscountTypes(true);
        }

        protected virtual void APDiscount_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
        {
            APDiscount row = e.Row as APDiscount;
            if (row != null)
            {
                PXUIFieldAttribute.SetEnabled<APDiscount.excludeFromDiscountableAmt>(sender, row, row.Type == DiscountType.Line);
                PXUIFieldAttribute.SetEnabled<APDiscount.skipDocumentDiscounts>(sender, row, row.Type == DiscountType.Group);
				PXStringListAttribute.SetList<APDiscount.type>(sender, null, new Tuple<string, string>[] {
					new Tuple<string, string> (DiscountType.Line, AR.Messages.Line),
					new Tuple<string, string> (DiscountType.Group, AR.Messages.Group),
					new Tuple<string, string> (DiscountType.Document, AR.Messages.Document) });
			}
        }
        protected virtual void APDiscount_Type_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            APDiscount discount = e.Row as APDiscount;

	        if (discount == null) return;

            VendorDiscountSequence sequence = PXSelect<
				VendorDiscountSequence, 
				Where<
					VendorDiscountSequence.discountID, Equal<Required<VendorDiscountSequence.discountID>>>>
				.SelectWindowed(this, 0, 1, discount.DiscountID);

			if (sequence != null)
            {
                discount.Type = (string)e.OldValue;
                sender.RaiseExceptionHandling<APDiscount.type>(discount, e.OldValue, new PXSetPropertyException(AR.Messages.DiscountTypeCannotChanged));
            }
			else if (
				discount.Type == DiscountType.Document && 
				discount.ApplicableTo != DiscountTarget.Vendor)
			{
				sender.SetValueExt<APDiscount.applicableTo>(e.Row, DiscountTarget.Vendor);
			}
        }
    }
}