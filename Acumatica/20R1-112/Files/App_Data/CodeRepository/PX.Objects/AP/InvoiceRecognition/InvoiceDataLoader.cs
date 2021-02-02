using PX.CloudServices.DocumentRecognition;
using PX.CloudServices.DocumentRecognition.InvoiceRecognition;
using PX.Common;
using PX.Data;
using System;
using System.Collections.Generic;

namespace PX.Objects.AP.InvoiceRecognition
{
	public class InvoiceDataLoader : DocumentDataLoader
	{
		private readonly Document _recognizedInvoice;
		private readonly PXCache _documentCache;
		private readonly PXCache _transactionCache;

		public InvoiceDataLoader(InvoiceRecognitionResult recognitionResult, PXCache documentCache, PXCache transactionCache)
		{
			recognitionResult.ThrowOnNull(nameof(recognitionResult));
			documentCache.ThrowOnNull(nameof(documentCache));
			transactionCache.ThrowOnNull(nameof(transactionCache));

			_recognizedInvoice = recognitionResult?.Documents?.Count > 0 ?
				recognitionResult.Documents[0] :
				null;
			_documentCache = documentCache;
			_transactionCache = transactionCache;
		}

		public void Load(APInvoice documentRow)
		{
			documentRow.ThrowOnNull(nameof(documentRow));

			if (_recognizedInvoice == null)
			{
				return;
			}

			var documentFields = _recognizedInvoice.Fields;
			if (documentFields != null)
			{
				LoadDocument(documentRow, documentFields);
			}

			var details = _recognizedInvoice.Details?.Value;
			if (details != null)
			{
				LoadTransactions(details);
			}
		}

		private void LoadDocument(APInvoice documentRow, PrimaryFields primaryFields)
		{
			SetFieldExtValue<string, string>(primaryFields.VendorId, _documentCache, documentRow, nameof(documentRow.VendorID));
			SetFieldExtValue<string, DateTime?>(primaryFields.Date, _documentCache, documentRow, nameof(documentRow.DocDate));
			SetFieldExtValue<decimal?, decimal?>(primaryFields.CuryOrigDocAmt, _documentCache, documentRow, nameof(documentRow.CuryOrigDocAmt));
		}

		private void LoadTransactions(IList<DetailsValue> details)
		{
			foreach (var d in details)
			{
				var transactionFields = d?.Fields;
				if (transactionFields == null)
				{
					continue;
				}

				var transactionRow = _transactionCache.Insert();

				SetFieldExtValue<int?, int?>(transactionFields.LineNbr, _transactionCache, transactionRow, nameof(APTran.LineNbr));
				SetFieldExtValue<string, string>(transactionFields.VendorItemID, _transactionCache, transactionRow, nameof(APTran.InventoryID));
				SetFieldExtValue<string, string>(transactionFields.Description, _transactionCache, transactionRow, nameof(APTran.TranDesc));
				SetFieldExtValue<string, string>(transactionFields.UOM, _transactionCache, transactionRow, nameof(APTran.UOM));
				SetFieldExtValue<decimal?, decimal?>(transactionFields.Qty, _transactionCache, transactionRow, nameof(APTran.Qty));
				SetFieldExtValue<decimal?, decimal?>(transactionFields.UnitPrice, _transactionCache, transactionRow, nameof(APTran.CuryUnitCost));
			}
		}
	}
}
