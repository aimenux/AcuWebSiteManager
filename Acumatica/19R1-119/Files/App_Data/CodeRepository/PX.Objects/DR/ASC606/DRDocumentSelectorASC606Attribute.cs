using System;
using System.Collections;
using System.Linq;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.AR;
using PX.Objects.CR;
using PX.Objects.GL;


namespace PX.Objects.DR
{
	public class DRDocumentSelectorASC606Attribute : DRDocumentSelectorAttribute
	{
		public DRDocumentSelectorASC606Attribute(Type moduleField, Type docTypeField, Type businessAccountField = null) 
			: base(moduleField, docTypeField, businessAccountField) { }

		protected override IEnumerable GetRecords()
		{
			PXCache cache = this._Graph.Caches[BqlCommand.GetItemType(_moduleField)];

			if (cache.Current == null) yield break;

			string docType = (string)cache.GetValue(cache.Current, _docTypeField.Name);
			string module = (string)cache.GetValue(cache.Current, _moduleField.Name);

			bool excludeUnreleased = cache
				.GetAttributesReadonly(cache.Current, _FieldName)
				.OfType<DRDocumentSelectorAttribute>()
				.First()
				.ExcludeUnreleased;

			int? businessAccountID = null;

			if (_businessAccountField != null)
			{
				businessAccountID =
					(int?)cache.GetValue(cache.Current, _businessAccountField.Name);
			}

			if (module == BatchModule.AR)
			{
				PXSelectBase<ARInvoice> relevantInvoices = new PXSelectJoin<
					ARInvoice,
						InnerJoin<BAccount, On<BAccount.bAccountID, Equal<ARInvoice.customerID>>>,
					Where<ARInvoice.docType, Equal<Required<ARInvoice.docType>>,
						And<Where<ARInvoice.drSchedCntr, Less<ARInvoice.lineCntr>, Or<ARInvoice.drSchedCntr, IsNull>>>>>
					(this._Graph);

				if (excludeUnreleased)
				{
					relevantInvoices.WhereAnd<Where<ARInvoice.released, Equal<True>>>();
				}

				object[] queryParameters;

				if (businessAccountID.HasValue)
				{
					relevantInvoices.WhereAnd<
						Where<ARInvoice.customerID, Equal<Required<ARInvoice.customerID>>>>();

					queryParameters = new object[] { docType, businessAccountID };
				}
				else
				{
					queryParameters = new object[] { docType };
				}

				foreach (PXResult<ARInvoice, BAccount> result in relevantInvoices.Select(queryParameters))
				{
					ARInvoice arInvoice = result;
					BAccount customer = result;

					string status = null;

					ARDocStatus.ListAttribute documentStatusListAttribute =
						new ARDocStatus.ListAttribute();

					if (documentStatusListAttribute.ValueLabelDic.ContainsKey(arInvoice.Status))
					{
						status = documentStatusListAttribute.ValueLabelDic[arInvoice.Status];
					}

					DRDocumentRecord record = new DRDocumentRecord
					{
						BAccountCD = customer.AcctCD,
						RefNbr = arInvoice.RefNbr,
						Status = status,
						FinPeriodID = arInvoice.FinPeriodID,
						DocType = arInvoice.DocType,
						DocDate = arInvoice.DocDate,
						LocationID = arInvoice.CustomerLocationID,
						CuryOrigDocAmt = arInvoice.CuryOrigDocAmt,
						CuryID = arInvoice.CuryID
					};

					yield return record;
				}
			}
			else
			{
				foreach(var item in base.GetRecords())
				{
					yield return item;
				}
			}
		}

		public override void FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if (e.Row == null || e.NewValue == null) return;

			string module = (string)sender.GetValue(e.Row, _moduleField.Name);
			string documentType = (string)sender.GetValue(e.Row, _docTypeField.Name);

			BqlCommand relevantDocumentsSelect;

			if (module == BatchModule.AR)
			{
				relevantDocumentsSelect = new Select<
					ARRegister,
					Where<
						ARRegister.docType, Equal<Required<ARRegister.docType>>,
						And<ARRegister.refNbr, Equal<Required<ARRegister.refNbr>>,
						And<Where<ARInvoice.drSchedCntr, Less<ARInvoice.lineCntr>, Or<ARInvoice.drSchedCntr, IsNull>>>>>>();

				if (ExcludeUnreleased)
				{
					relevantDocumentsSelect =
						relevantDocumentsSelect.WhereAnd<Where<ARRegister.released, Equal<True>>>();
				}
			}
			else
			{
				base.FieldVerifying(sender, e);
				return;
			}

			PXView relevantDocuments = new PXView(_Graph, true, relevantDocumentsSelect);

			if (relevantDocuments.SelectSingle(documentType, e.NewValue) == null)
			{
				throwNoItem(
					restricted: null,
					external: true,
					value: e.NewValue);
			}

			
		}
	}
}
