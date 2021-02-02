using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PX.Data;
using PX.Objects.AR;
using PX.Objects.DR;
using PX.Objects.CM;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.IN;

namespace PX.Objects.RUTROT
{
	public class ARInvoiceEntryRUTROT : PXGraphExtension<ARInvoiceEntry>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.rutRotDeduction>();
		}

		public RUTROTManager<ARInvoice, ARTran, ARInvoiceRUTROT, ARTranRUTROT> RRManager;

		public override void Initialize()
		{
			base.Initialize();
			RRManager = new RUTROTManager<ARInvoice, ARTran, ARInvoiceRUTROT, ARTranRUTROT>(
				this.Base, Rutrots, RRDistribution, Base.Document, Base.Transactions, Base.currencyinfo);
		}		

		public PXSelect<RUTROT, Where<RUTROT.docType, Equal<Current<ARInvoice.docType>>,
			And<RUTROT.refNbr, Equal<Current<ARInvoice.refNbr>>>>> Rutrots;

		public PXSelect<RUTROTDistribution,
					Where<RUTROTDistribution.docType, Equal<Current<RUTROT.docType>>,
					And<RUTROTDistribution.refNbr, Equal<Current<RUTROT.refNbr>>>>> RRDistribution;

        public PXAction<ARInvoice> release;

        [PXUIField(DisplayName = "Release", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
        [PXProcessButton]
        public IEnumerable Release(PXAdapter adapter)
        {
            if (RUTROTHelper.IsNeedBalancing(Base, Base.Document.Current, RUTROTBalanceOn.Release))
            {
                Base.Save.Press();
                ARInvoice doc = Base.Document.Current;

                PXLongOperation.StartOperation(Base, delegate ()
                {
                    RUTROT rutrot = PXSelect<RUTROT, Where<RUTROT.refNbr, Equal<Required<ARInvoice.refNbr>>,
                            And<RUTROT.docType, Equal<Required<ARInvoice.docType>>>>>.Select(Base, doc.RefNbr, doc.DocType);

                    ARInvoiceEntry invoiceEntryGraph = PXGraph.CreateInstance<ARInvoiceEntry>();

                    RUTROTHelper.BalanceARInvoiceRUTROT(invoiceEntryGraph, doc, OnRelease: true, rutrot: rutrot);

                    RUTROTHelper.CreateAdjustment(invoiceEntryGraph, doc, rutrot);

                    Base.ReleaseProcess(new List<ARRegister> { doc });
                });
                return new List<ARInvoice> { Base.Document.Current };
            }
            else
            {
               return Base.Release(adapter);
            }

        }

        public delegate void ReverseInvoiceProcDelegate(ARRegister doc, ReverseInvoiceArgs reverseArgs);
        [PXOverride]
        public virtual void ReverseInvoiceProc(ARRegister doc, ReverseInvoiceArgs reverseArgs, ReverseInvoiceProcDelegate baseMethod)
        {
            baseMethod(doc, reverseArgs);

            ARInvoice reverce_invoice = Base.Document.Current;

            foreach (RUTROTDistribution rutrotDetail in PXSelect<RUTROTDistribution, 
				Where<RUTROTDistribution.docType, Equal<Required<ARRegister.docType>>,
                And<RUTROTDistribution.refNbr, Equal<Required<ARRegister.refNbr>>>>>.Select(Base, doc.DocType, doc.RefNbr))
            {
                RUTROTDistribution new_detail = (RUTROTDistribution)Base.RRDistribution.Cache.CreateCopy(rutrotDetail);
                new_detail.RefNbr = reverce_invoice.RefNbr;
                new_detail.DocType = reverce_invoice.DocType;
                Base.RRDistribution.Insert(new_detail);
            }
        }

        #region methods overrides
        [PXOverride]
	    public virtual void ARInvoiceCreated(ARInvoice invoice, ARRegister doc, ARInvoiceEntry.ARInvoiceCreatedDelegate baseMethod)
	    {
            baseMethod(invoice, doc);

            RUTROT rutrot = PXSelect<RUTROT, Where<RUTROT.docType, Equal<Required<ARRegister.docType>>,
                                And<RUTROT.refNbr, Equal<Required<ARRegister.refNbr>>>>>.Select(Base, doc.DocType, doc.RefNbr);

            rutrot = RUTROTHelper.CreateCopy(Base.Rutrots.Cache, rutrot, invoice.DocType, invoice.RefNbr);
            rutrot = Base.Rutrots.Update(rutrot);
        }
        #endregion
        #region redirectors
        public PXAction<ARInvoice> ViewCreditMemo;

        [PXUIField(Visible = false, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXLookupButton]
        protected virtual IEnumerable viewCreditMemo(PXAdapter adapter)
        {
            ARInvoice creditMemo = PXSelectJoin<ARInvoice,
                InnerJoin<RUTROT, On<RUTROT.balancingCreditMemoDocType, Equal<ARInvoice.docType>,
                                And<RUTROT.balancingCreditMemoRefNbr, Equal<ARInvoice.refNbr>>>>,
                Where<RUTROT.docType, Equal<Required<RUTROT.docType>>,
                And<RUTROT.refNbr, Equal<Required<RUTROT.refNbr>>>>>.Select(Base, Rutrots.Current.DocType, Rutrots.Current.RefNbr);

            Base.Document.Current = creditMemo;

            throw new PXRedirectRequiredException(Base, true, AR.Messages.Document) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
        }

        public PXAction<ARInvoice> ViewDebitMemo;

        [PXUIField(DisplayName = "", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = false)]
        [PXEditDetailButton]
        public virtual IEnumerable viewDebitMemo(PXAdapter adapter)
        {
            ARInvoice debitMemo = PXSelectJoin<ARInvoice,
                InnerJoin<RUTROT, On<RUTROT.balancingDebitMemoDocType, Equal<ARInvoice.docType>,
                                And<RUTROT.balancingDebitMemoRefNbr, Equal<ARInvoice.refNbr>>>>,
                Where<RUTROT.docType, Equal<Required<RUTROT.docType>>,
                And<RUTROT.refNbr, Equal<Required<RUTROT.refNbr>>>>>.Select(Base, Rutrots.Current.DocType, Rutrots.Current.RefNbr);

            Base.Document.Current = debitMemo;

            throw new PXRedirectRequiredException(Base, true, AR.Messages.Document) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
        }
        #endregion
        #region Events
        #region ARInvoice
        protected virtual void ARInvoice_BranchID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			RRManager.UncheckRUTROTIfProhibited(RUTROTHelper.GetExtensionNullable<ARInvoice, ARInvoiceRUTROT>(e.Row as ARInvoice), 
				Base.CurrentBranch.SelectSingle(((ARInvoice)e.Row).BranchID));
		}

        protected virtual void ARInvoice_CuryID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            var row = (ARInvoice)e.Row;

            if(row == null)
            {
                return;
            }

            ARInvoiceRUTROT doc = RUTROTHelper.GetExtensionNullable<ARInvoice, ARInvoiceRUTROT>(row);

            if (!RUTROTHelper.CurrenciesMatch(RUTROTHelper.GetBranchRUTROT(Base), doc))
            {
				doc.IsRUTROTDeductible = false;
			}
        }

		protected virtual void ARInvoice_CustomerID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			RRManager.UncheckRUTROTIfProhibited(RUTROTHelper.GetExtensionNullable<ARInvoice, ARInvoiceRUTROT>(e.Row as ARInvoice), 
				Base.CurrentBranch.SelectSingle(((ARInvoice)e.Row).BranchID));
		}
		
		protected virtual void ARInvoice_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
            ARInvoiceRUTROT doc = RUTROTHelper.GetExtensionNullable<ARInvoice, ARInvoiceRUTROT>(e.Row as ARInvoice);

            if (doc == null)
		    {
		        return;
		    }

			RRManager.Update((ARInvoice)e.Row);
			UpdateLinesControls(doc.IsRUTROTDeductible == true);
		}
        #endregion
        #region ARTran
        public virtual void ARTran_InventoryID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			RRManager.UpdateTranDeductibleFromInventory(RUTROTHelper.GetExtensionNullable<ARTran, ARTranRUTROT>((ARTran)e.Row), 
				RUTROTHelper.GetExtensionNullable<ARInvoice, ARInvoiceRUTROT>(Base.Document.Current));
		}

		protected virtual void ARTran_RUTROTType_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			ARTranRUTROT row = RUTROTHelper.GetExtensionNullable<ARTran, ARTranRUTROT>((ARTran)e.Row);
			if (row != null)
			{
				row.RUTROTWorkTypeID = null;
			}
		}

		protected virtual void ARTran_RUTROTItemType_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			ARTran row = (ARTran)e.Row;
			ARTranRUTROT rowRR = RUTROTHelper.GetExtensionNullable<ARTran, ARTranRUTROT>(row);

		    if (row == null)
		    {
		        return;
		    }

		    PXUIFieldAttribute.SetEnabled<ARTranRUTROT.rUTROTWorkTypeID>(sender, row, rowRR.RUTROTItemType != RUTROTItemTypes.OtherCost);

		    if (rowRR.RUTROTItemType == RUTROTItemTypes.OtherCost)
		    {
		        sender.SetValueExt<ARTranRUTROT.rUTROTWorkTypeID>(row, null);
		    }
		}

		protected virtual void ARTran_InventoryID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if (!e.ExternalCall)
			{
				e.Cancel = true;
			}
			ARTran tran = (ARTran)e.Row;
			if (tran == null)
				return;
			if (Base.Document.Current == null)
				return;
			if (e.NewValue == null)
				return;

			string value = Rutrots.Current?.RUTROTType;
			InventoryItem item = (InventoryItem)Base.InventoryItem.Select((int)e.NewValue);
			InventoryItemRUTROT itemRR = RUTROTHelper.GetExtensionNullable<InventoryItem, InventoryItemRUTROT>(item);
			if (!RUTROTHelper.IsItemMatchRUTROTType(value, item, itemRR, itemRR?.IsRUTROTDeductible == true))
			{
				sender.RaiseExceptionHandling<ARTran.inventoryID>(tran, 
					item.InventoryCD, 
					new PXSetPropertyException<ARTran.inventoryID>(RUTROTMessages.LineDoesNotMatchDoc));
				e.NewValue = item.InventoryCD;
				throw new PXSetPropertyException<ARTran.inventoryID>(RUTROTMessages.LineDoesNotMatchDoc);
			}
		}

		public virtual void ARTran_IsRUTROTDeductible_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			ARTran tran = (ARTran)e.Row;
			if (tran == null)
				return;
			if (Base.Document.Current == null)
				return;
			if (e.NewValue == null || (bool)e.NewValue == false)
				return;

			string value = Rutrots.Current?.RUTROTType;
			InventoryItem item = (InventoryItem)Base.InventoryItem.Select(tran.InventoryID);
			InventoryItemRUTROT itemRR = RUTROTHelper.GetExtensionNullable<InventoryItem, InventoryItemRUTROT>(item);
			if (!RUTROTHelper.IsItemMatchRUTROTType(value, item, itemRR, (bool)e.NewValue))
			{
				sender.RaiseExceptionHandling<ARTranRUTROT.isRUTROTDeductible>(tran, 
					false, 
					new PXSetPropertyException<ARTranRUTROT.isRUTROTDeductible>(RUTROTMessages.LineDoesNotMatchDoc));
				e.NewValue = false;
				throw new PXSetPropertyException<ARTranRUTROT.isRUTROTDeductible>(RUTROTMessages.LineDoesNotMatchDoc);
			}
		}

		protected virtual void ARTran_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			if (e.Row == null)
				return;
			ARTran row = e.Row as ARTran;
			ARTranRUTROT rowRR = RUTROTHelper.GetExtensionNullable<ARTran, ARTranRUTROT>(row);
			RUTROTWorkType workType = PXSelect<RUTROTWorkType, 
				Where<RUTROTWorkType.workTypeID, Equal<Required<RUTROTWorkType.workTypeID>>>>.Select(this.Base, rowRR.RUTROTWorkTypeID);
			if (rowRR?.IsRUTROTDeductible == true && Base.Document.Current.Released != true && rowRR?.RUTROTItemType != RUTROTItemTypes.OtherCost
				&& Base.Document.Current.Voided != true && !RUTROTHelper.IsUpToDateWorkType(workType, Base.Document.Current.DocDate ?? DateTime.Now))
			{
				sender.RaiseExceptionHandling<ARTranRUTROT.rUTROTWorkTypeID>(row, 
					workType?.Description, 
					new PXSetPropertyException(RUTROTMessages.ObsoleteWorkType));
			}
			else
			{
				sender.RaiseExceptionHandling<ARTranRUTROT.rUTROTWorkTypeID>(row, workType?.Description, null);
			}
        }

        protected virtual void ARTran_RowInserting(PXCache sender, PXRowInsertingEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            ARTran row = e.Row as ARTran;
            ARTranRUTROT rowRR = RUTROTHelper.GetExtensionNullable<ARTran, ARTranRUTROT>(row);

            rowRR.CuryRUTROTTaxAmountDeductible = decimal.Zero;
        }

        /// <summary>
        /// It`s a work around for the <see cref="RUTROT.curyTotalAmt"/> field, 
        /// which was calculated incorrectly when the document existed the group discount.
        /// </summary>
        protected virtual void ARTran_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
        {
            if (e.Row == null || Rutrots.Current == null)
            {
                return;
            }

            PXFormulaAttribute.CalcAggregate<ARTranRUTROT.curyRUTROTAvailableAmt>(Base.Transactions.Cache, Rutrots.Current);
        }

        protected virtual void ARTran_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
        {
            ARTran row = (ARTran)e.Row;
            ARTranRUTROT rowRR = RUTROTHelper.GetExtensionNullable<ARTran, ARTranRUTROT>(row);

            if (row == null || rowRR?.IsRUTROTDeductible != true)
            {
                return;
            }

            bool isWorkTypeRequired = rowRR.RUTROTItemType == RUTROTItemTypes.MaterialCost || rowRR.RUTROTItemType == RUTROTItemTypes.Service;

            if (isWorkTypeRequired && rowRR.RUTROTWorkTypeID == null && sender.RaiseExceptionHandling<ARTranRUTROT.rUTROTWorkTypeID>(e.Row, rowRR.RUTROTWorkTypeID, new PXSetPropertyException(ErrorMessages.FieldIsEmpty)))
            {
                throw new PXRowPersistingException(typeof(ARTranRUTROT.rUTROTWorkTypeID).Name, rowRR.RUTROTWorkTypeID, ErrorMessages.FieldIsEmpty);
            }
        }
        #endregion
        #endregion

        private void UpdateLinesControls(bool showSection)
		{
			PXUIFieldAttribute.SetVisible<ARTranRUTROT.isRUTROTDeductible>(Base.Transactions.Cache, null, showSection);
			PXUIFieldAttribute.SetEnabled<ARTranRUTROT.isRUTROTDeductible>(Base.Transactions.Cache, null, showSection);
			PXUIFieldAttribute.SetVisible<ARTranRUTROT.rUTROTItemType>(Base.Transactions.Cache, null, showSection);
			PXUIFieldAttribute.SetEnabled<ARTranRUTROT.rUTROTItemType>(Base.Transactions.Cache, null, showSection);
			PXUIFieldAttribute.SetVisible<ARTranRUTROT.rUTROTWorkTypeID>(Base.Transactions.Cache, null, showSection);
			PXUIFieldAttribute.SetVisible<ARTranRUTROT.curyRUTROTAvailableAmt>(Base.Transactions.Cache, null, showSection);
		}
    }
}
