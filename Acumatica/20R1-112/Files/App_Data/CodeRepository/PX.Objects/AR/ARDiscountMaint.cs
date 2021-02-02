using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PX.Data;
using PX.Objects.CS;
using PX.Objects.Common.Discount;

namespace PX.Objects.AR
{
	public class ARDiscountMaint : PXGraph<SO.SOSetupMaint>
	{
		#region Selects/Views

		public PXSelect<ARDiscount> Document;

		[Obsolete(Common.Messages.WillBeRemovedInAcumatica2020R2)]
		public PXSelect<DiscountSequence> DiscountSequences;

		#endregion

		public ARDiscountMaint()
		{
			PXCache cache;
			foreach (var type in DiscountChildTypes())
				cache = Caches[type];

			PXParentAttribute.SetLeaveChildren<DiscountSequenceDetail.discountSequenceID>(this.Caches<DiscountSequenceDetail>(), null, false);
			PXParentAttribute.SetLeaveChildren<DiscountSequence.discountID>(this.Caches<DiscountSequence>(), null, true);
		}

		protected virtual IEnumerable<Type> DiscountChildTypes()
		{
			yield return typeof(DiscountSequence);
			yield return typeof(DiscountSite);
			yield return typeof(DiscountItem);
			yield return typeof(DiscountBranch);
			yield return typeof(DiscountCustomer);
			yield return typeof(DiscountCustomerPriceClass);
			yield return typeof(DiscountInventoryPriceClass);
			yield return typeof(DiscountSequenceDetail);
		}

		public PXSavePerRow<ARDiscount> Save;
		public PXCancel<ARDiscount> Cancel;

        public PXAction<ARDiscount> viewARDiscountSequence;
        [PXUIField(DisplayName = AR.Messages.ViewARDiscountSequence, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = false)]
        [PXLookupButton]
        public virtual IEnumerable ViewARDiscountSequence(PXAdapter adapter)
        {
            if (this.Document.Current != null)
            {
                if (this.Document.Current.DiscountID != null)
                {
                    ARDiscountSequenceMaint arDiscountSequence = PXGraph.CreateInstance<ARDiscountSequenceMaint>();
                    arDiscountSequence.Sequence.Current = new DiscountSequence();
                    arDiscountSequence.Sequence.Current.DiscountID = this.Document.Current.DiscountID;
                    throw new PXRedirectRequiredException(arDiscountSequence, AR.Messages.ViewARDiscountSequence);
                }
            }
            return adapter.Get();
        }

		protected virtual void ARDiscount_ApplicableTo_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			ARDiscount row = e.Row as ARDiscount;
			if (row != null )
			{
				DiscountSequence firstSeq = PXSelect<DiscountSequence, Where<DiscountSequence.discountID, Equal<Required<DiscountSequence.discountID>>>>.SelectWindowed(this, 0, 1, row.DiscountID);
				if (firstSeq != null)
				{
					e.Cancel = true;
					throw new PXSetPropertyException(Messages.SequenceExistsApplicableTo);
				}
			}
		}

		protected virtual void ARDiscount_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			ARDiscount row = e.Row as ARDiscount;
			if (row != null)
			{
                if (e.Operation == PXDBOperation.Insert || e.Operation == PXDBOperation.Update)
				{
                    if (PXSelectReadonly<AP.APDiscount, Where<AP.APDiscount.discountID, Equal<Required<AP.APDiscount.discountID>>>>.Select(this, row.DiscountID).Count != 0)
                    {
                        sender.RaiseExceptionHandling<ARDiscount.discountID>(row, row.DiscountID, new PXSetPropertyException(Messages.DiscountCodeAlreadyExistAP, PXErrorLevel.Error));
                    }
                    if ((row.Type == DiscountType.Line || row.Type == DiscountType.Document) && row.SkipDocumentDiscounts == true) row.SkipDocumentDiscounts = false;
                    if ((row.Type == DiscountType.Group || row.Type == DiscountType.Document) && row.ExcludeFromDiscountableAmt == true) row.ExcludeFromDiscountableAmt = false;
				}
			}
		}

        protected virtual void ARDiscount_RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
        {
			if((e.Operation & PXDBOperation.Delete) == PXDBOperation.Delete && e.TranStatus == PXTranStatus.Open)
				RemoveChildReferences((ARDiscount)e.Row);

            DiscountEngine.GetDiscountTypes(true);
        }

		protected virtual void RemoveChildReferences(ARDiscount discount)
		{
			void clearChildCaches()
			{
				foreach (var type in DiscountChildTypes())
					Caches[type].Clear();
			};

			clearChildCaches();

			try
			{
				PXParentAttribute.SetLeaveChildren<DiscountSequence.discountID>(this.Caches<DiscountSequence>(), null, false);

				Document.Cache.RaiseRowDeleted(discount);

				foreach (var type in DiscountChildTypes())
					Persist(type, PXDBOperation.Delete);

				clearChildCaches();
			}
			finally
			{
				PXParentAttribute.SetLeaveChildren<DiscountSequence.discountID>(this.Caches<DiscountSequence>(), null, true);
			}
		}

		protected virtual void ARDiscount_Type_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			ARDiscount discount = e.Row as ARDiscount;

			if (discount == null) return;

			DiscountSequence sequence = PXSelect<
				DiscountSequence, 
				Where<
					DiscountSequence.discountID, Equal<Required<DiscountSequence.discountID>>>>
				.SelectWindowed(this, 0, 1, discount.DiscountID);

			if (sequence != null)
			{
				discount.Type = (string)e.OldValue;
				sender.RaiseExceptionHandling<ARDiscount.type>(discount, e.OldValue, new PXSetPropertyException(Messages.DiscountTypeCannotChanged));
			}
			else if (
				discount.Type == DiscountType.Document &&
				!GetAllowedDiscountTargetsForDocumentDiscountType().Item1.Contains(discount.ApplicableTo))
			{
				sender.SetValueExt<ARDiscount.applicableTo>(discount, DiscountTarget.Customer);
			}
		}

		protected virtual void ARDiscount_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			ARDiscount row = e.Row as ARDiscount;
			if (row != null)
			{
				if (row.Type != DiscountType.Line)
				{
					sender.SetValueExt<ARDiscount.isAppliedToDR>(row, false);
				}
			}
		}

		protected virtual void ARDiscount_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			ARDiscount row = e.Row as ARDiscount;
			if (row != null)
			{
                PXUIFieldAttribute.SetEnabled<ARDiscount.excludeFromDiscountableAmt>(sender, row, row.Type == DiscountType.Line);
                PXUIFieldAttribute.SetEnabled<ARDiscount.skipDocumentDiscounts>(sender, row, row.Type == DiscountType.Group);
				PXUIFieldAttribute.SetEnabled<ARDiscount.isAppliedToDR>(sender, row, row.Type == DiscountType.Line);
				PXStringListAttribute.SetList<ARDiscount.type>(sender, null, new Tuple<string, string>[] {
					new Tuple<string, string> (DiscountType.Line, AR.Messages.Line),
					new Tuple<string, string> (DiscountType.Group, AR.Messages.Group),
					new Tuple<string, string> (DiscountType.Document, AR.Messages.Document) });
			}
		}

		private static Tuple<List<string>, List<string>> GetAllowedDiscountTargetsForDocumentDiscountType()
		{
			List<string> values = new List<string>();
			List<string> labels = new List<string>();

			values.Add(DiscountTarget.Customer);
			labels.Add(Messages.Customer);

			if (PXAccess.FeatureInstalled<FeaturesSet.branch>())
			{
				values.Add(DiscountTarget.CustomerAndBranch);
				labels.Add(Messages.CustomerAndBranch);
			}

			values.Add(DiscountTarget.CustomerPrice);
			labels.Add(Messages.CustomerPrice);

			if (PXAccess.FeatureInstalled<FeaturesSet.branch>())
			{
				values.Add(DiscountTarget.CustomerPriceAndBranch);
				labels.Add(Messages.CustomerPriceAndBranch);
			}

			values.Add(DiscountTarget.Unconditional);
			labels.Add(Messages.Unconditional);

			return Tuple.Create(values, labels);
		}

		protected virtual void ARDiscount_ApplicableTo_FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			ARDiscount row = e.Row as ARDiscount;

			List<string> values = new List<string>();
			List<string> labels = new List<string>();

			if (row != null)
			{
                if (row.Type == DiscountType.Document)
                {
	                var valuesAndLabels = GetAllowedDiscountTargetsForDocumentDiscountType();

	                values = valuesAndLabels.Item1;
	                labels = valuesAndLabels.Item2;

                    e.ReturnState = PXStringState.CreateInstance(
						e.ReturnValue, 
						1, 
						false, 
						nameof(ARDiscount.ApplicableTo), 
						false, 
						1, 
						null, 
						values.ToArray(), 
						labels.ToArray().Select(PXMessages.LocalizeNoPrefix).ToArray(), 
						true,
						DiscountTarget.Customer);

                    return;
                }
			}

			values.AddRange(new string[] { DiscountTarget.Customer, DiscountTarget.Inventory, DiscountTarget.InventoryPrice, DiscountTarget.CustomerAndInventory, DiscountTarget.CustomerAndInventoryPrice, DiscountTarget.CustomerPrice, DiscountTarget.CustomerPriceAndInventory, DiscountTarget.CustomerPriceAndInventoryPrice});
			labels.AddRange(new string[] { Messages.Customer, Messages.Discount_Inventory, Messages.InventoryPrice, Messages.CustomerAndInventory, Messages.CustomerAndInventoryPrice, Messages.CustomerPrice, Messages.CustomerPriceAndInventory, Messages.CustomerPriceAndInventoryPrice});
			if (PXAccess.FeatureInstalled<FeaturesSet.warehouse>())
			{
				values.AddRange(new string[] { DiscountTarget.Warehouse, DiscountTarget.WarehouseAndInventory, DiscountTarget.WarehouseAndCustomer, DiscountTarget.WarehouseAndInventoryPrice, DiscountTarget.WarehouseAndCustomerPrice });
				labels.AddRange(new string[] { Messages.Warehouse, Messages.WarehouseAndInventory, Messages.WarehouseAndCustomer, Messages.WarehouseAndInventoryPrice, Messages.WarehouseAndCustomerPrice });
			}
			if (PXAccess.FeatureInstalled<FeaturesSet.branch>())
			{
				values.Add(DiscountTarget.Branch);
				labels.Add(Messages.Branch);
			}
			values.Add(DiscountTarget.Unconditional );
			labels.Add(Messages.Unconditional);

			e.ReturnState = PXStringState.CreateInstance(e.ReturnValue, 1, false, "ApplicableTo", false, 1, null, values.ToArray(), labels.ToArray().Select(l => PXMessages.LocalizeNoPrefix(l)).ToArray(), true, DiscountTarget.Inventory);
		}
	}
}
