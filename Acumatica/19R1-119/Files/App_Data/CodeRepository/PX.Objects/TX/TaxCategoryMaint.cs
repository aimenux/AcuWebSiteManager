using System;
using PX.Data;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using PX.Objects.AP;
using PX.Data.Api.Export;

namespace PX.Objects.TX
{
	[NonOptimizable(new Type[] { typeof(Tax.taxType), typeof(Tax.taxApplyTermsDisc) }, IgnoreOptimizationBehavior = true)]
	public class TaxCategoryMaint : PXGraph<TaxCategoryMaint, TaxCategory>
	{
		public PXSelect<TaxCategory> TxCategory;
		public PXSelectJoin<TaxCategoryDet,InnerJoin<Tax, On<TaxCategoryDet.taxID,Equal<Tax.taxID>>>, Where<TaxCategoryDet.taxCategoryID, Equal<Current<TaxCategory.taxCategoryID>>>> Details;
        public PXSelect<TaxCategoryDet> TxCategoryDet;

		public TaxCategoryMaint()
		{
			if (Company.Current.BAccountID.HasValue == false)
			{
                throw new PXSetupNotEnteredException(ErrorMessages.SetupNotEntered, typeof(GL.Branch), PXMessages.LocalizeNoPrefix(CS.Messages.BranchMaint));
			}
		}
		public PXSetup<GL.Branch> Company;


		protected virtual void TaxCategory_RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
		{
			TaxCategory row = e.Row as TaxCategory;
			if (row != null)
			{
				PXSelectorAttribute.CheckAndRaiseForeignKeyException(sender, e.Row, typeof(AP.APTran.taxCategoryID), typeof(Search<AP.APTran.taxCategoryID, Where<AP.APTran.released, Equal<False>>>));
				PXSelectorAttribute.CheckAndRaiseForeignKeyException(sender, e.Row, typeof(AR.ARTran.taxCategoryID), typeof(Search<AR.ARTran.taxCategoryID, Where<AR.ARTran.released, Equal<False>>>));
				PXSelectorAttribute.CheckAndRaiseForeignKeyException(sender, e.Row, typeof(AR.ARFinCharge.taxCategoryID));
				PXSelectorAttribute.CheckAndRaiseForeignKeyException(sender, e.Row, typeof(CA.CASplit.taxCategoryID), typeof(Search2<CA.CASplit.taxCategoryID, InnerJoin<CA.CAAdj, 
					On<CA.CASplit.adjRefNbr, Equal<CA.CAAdj.adjRefNbr>>>, Where<CA.CAAdj.released, Equal<False>>>));
				PXSelectorAttribute.CheckAndRaiseForeignKeyException(sender, e.Row, typeof(EP.EPExpenseClaimDetails.taxCategoryID));
				PXSelectorAttribute.CheckAndRaiseForeignKeyException(sender, e.Row, typeof(PO.POLine.taxCategoryID));
				PXSelectorAttribute.CheckAndRaiseForeignKeyException(sender, e.Row, typeof(SO.SOLine.taxCategoryID));
				PXSelectorAttribute.CheckAndRaiseForeignKeyException(sender, e.Row, typeof(SO.SOOrder.freightTaxCategoryID));
				PXSelectorAttribute.CheckAndRaiseForeignKeyException(sender, e.Row, typeof(CR.CROpportunityProducts.taxCategoryID));
				PXSelectorAttribute.CheckAndRaiseForeignKeyException(sender, e.Row, typeof(IN.INItemClass.taxCategoryID));
				PXSelectorAttribute.CheckAndRaiseForeignKeyException(sender, e.Row, typeof(IN.InventoryItem.taxCategoryID));
			}
		}

		protected virtual void TaxCategoryDet_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			TaxCategoryDet row = e.Row as TaxCategoryDet;
			if (row != null)
			{
				PXUIFieldAttribute.SetEnabled<TaxCategoryDet.taxID>(sender, row, string.IsNullOrEmpty(row.TaxID));
			}
		}

		protected virtual void TaxCategoryDet_TaxID_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
		{
			TaxCategoryDet tax = (TaxCategoryDet)e.Row;
			string taxId = (string)e.NewValue;
			if (tax.TaxID != taxId)
			{
				foreach (TaxCategoryDet iTax in this.Details.Select())
				{
					if (iTax.TaxID == taxId)
					{
						e.Cancel = true;
						throw new PXSetPropertyException(Messages.TaxAlreadyInList);
					}
				}
			}
		}



		/*
		 * Add Later. Currently, there is no constraints in the database.
		public override void Persist()
		{
			try
			{
				base.Persist();
			}
			catch (PXDatabaseException e)
			{
				if (e.Message.IndexOf("DELETE") != -1 && e.Message.IndexOf("constraint 'Segment_SegmentValue_FK1'") != -1)
				{
					throw new Exception("Segment '" + e.Keys[1] + "' has values and cannot be deleted.");
				}
				else
				{
					throw;
				}
			}
		}
		*/
	}
}
