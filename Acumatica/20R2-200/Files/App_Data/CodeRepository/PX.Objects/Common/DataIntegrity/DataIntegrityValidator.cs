using System;
using System.Collections.Generic;
using System.Linq;

using PX.Common.Serialization;
using PX.Data;

using PX.Objects.GL;
using PX.Objects.Common.Extensions;
using PX.Objects.AR;
using PX.Objects.AP;
using PX.Objects.CM;
using System.Text;

namespace PX.Objects.Common.DataIntegrity
{
	public class DataIntegrityValidator<TRegister>
		where TRegister : CM.IRegister, IBalance
	{
		private class RecordContextInfo : Tuple<PXCache, object>
		{
			public PXCache Cache => Item1;

			public object Record => Item2;

			public string FullDescription => Cache.GetFullDescription(Record);

			public RecordContextInfo(PXCache cache, object record)
				: base(cache, record)
			{ }
		}

		private class InconsistencyError : Tuple<DataIntegrityException, IEnumerable<RecordContextInfo>>
		{
			public DataIntegrityException IntegrityException => Item1;

			public IEnumerable<RecordContextInfo> ContextData => Item2;

			public string ErrorCode => IntegrityException.InconsistencyCode;

			public InconsistencyError(DataIntegrityException exception, IEnumerable<RecordContextInfo> contextData) 
				: base(exception, contextData)
			{ }
		}

		private class InconsistencyError<TInconsistency> : InconsistencyError
			where TInconsistency : IConstant<string>, IBqlOperand, new()
		{
			public InconsistencyError(params RecordContextInfo[] contextData) 
				: base(
					  new DataIntegrityException(new TInconsistency().Value as string, GetLabel.For<TInconsistency>()), 
					  contextData)
			{ }
		}

		private PXSelectBase<GLTran> _selectGLTran;
		private PXGraph _graph;
		private PXCache _docCache;
		private TRegister _doc;
		private string _module;
		private int? _referenceID;
		private bool? _released;
		private string _inconsistencyHandlingMode;
		private ICollection<InconsistencyError> _errors;

		public DataIntegrityValidator(
			PXGraph graph,
			PXCache docCache,
			TRegister doc,
			string module,
			int? referenceID,
			bool? released,
			string inconsistencyHandlingMode)
		{
			_graph = graph;
			_docCache = docCache;
			_doc = doc;
			_module = module;
			_referenceID = referenceID;
			_released = released;
			_inconsistencyHandlingMode = inconsistencyHandlingMode;
			_errors = new List<InconsistencyError>();

			_selectGLTran = new PXSelect<GLTran,
				Where<GLTran.module, Equal<Required<GLTran.module>>,
					And<GLTran.tranType, Equal<Required<GLTran.tranType>>,
					And<GLTran.refNbr, Equal<Required<GLTran.refNbr>>,
					And<GLTran.referenceID, Equal<Required<GLTran.referenceID>>>>>>>(graph);
		}

		public void Commit()
		{
			if (_inconsistencyHandlingMode == InconsistencyHandlingMode.None || !_errors.Any())
			{
				return;
			}

			foreach (InconsistencyError error in _errors)
			{
				foreach (RecordContextInfo contextInfo in error.ContextData)
				{
					PXTrace.WriteInformation(contextInfo.Cache.GetFullDescription(contextInfo.Record));
				}

				PXTrace.WriteInformation($"{error.ErrorCode} {error.IntegrityException.Message}");				
			}

			if (_inconsistencyHandlingMode == InconsistencyHandlingMode.Prevent)
			{
				DataIntegrityException firstError = _errors.First().IntegrityException;

				throw new DataIntegrityException(
					firstError.InconsistencyCode,
					Messages.DataIntegrityErrorDuringProcessingFormat,
					firstError.Message);
			}

			if (_inconsistencyHandlingMode == InconsistencyHandlingMode.Log)
			{
				DateTime now = DateTime.UtcNow;

				foreach (InconsistencyError error in _errors)
				{
					string context = error.ContextData.Any()
						? string.Join("\r\n",
							error.ContextData.Select(contextInfo => contextInfo.Cache.ToXml(contextInfo.Record)))
						: string.Empty;
					var errorMsg = $"Error message: {error.IntegrityException.Message}; Date: {DateTime.Now}; Screen: {_graph.Accessinfo.ScreenID}; Context: {context}; InconsistencyCode: {error.IntegrityException.InconsistencyCode}";
					PXTrace.WriteError(errorMsg);
				}
			}
		}

		private bool IsSkipCheck(bool checkLevelDisableFlag)
			=> checkLevelDisableFlag || _inconsistencyHandlingMode == InconsistencyHandlingMode.None;

		///<summary> 
		/// Finding inconsistency between GL module and document.
		/// Run this method at start point of the "Release" process.
		/// Validating case:
		/// Unreleased document shouldn't have GL Batch before/after the "Release" process.
		///</summary> 
		public DataIntegrityValidator<TRegister> CheckTransactionsExistenceForUnreleasedDocument(bool disableCheck = false)
		{
			if (IsSkipCheck(disableCheck)) return this;

				GLTran tran;

				if (_released != true &&
					(tran = _selectGLTran.SelectSingle(_module, _doc.DocType, _doc.RefNbr, _referenceID)) != null)
				{
				_errors.Add(new InconsistencyError<InconsistencyCode.unreleasedDocumentHasGlTransactions>(
					new RecordContextInfo(_docCache, _doc),
					new RecordContextInfo(_selectGLTran.Cache, tran)));
			}

			return this;
		}

		///<summary> 
		/// Finding inconsistency between GL Batch and its transactions.
		/// Run this method at end point of the "Release" process.
		/// Validating case:
		/// The document should always have GL Batch after the "Release" process.
		///</summary>
		public DataIntegrityValidator<TRegister> CheckTransactionsExistenceForReleasedDocument(bool disableCheck = false)
		{
			if (IsSkipCheck(disableCheck)) return this;

				GLTran tran;

				if (_released == true &&
					(tran = _selectGLTran.SelectSingle(_module, _doc.DocType, _doc.RefNbr, _referenceID)) == null)
				{
				_errors.Add(new InconsistencyError<InconsistencyCode.releasedDocumentHasNoGlTransactions>(
					new RecordContextInfo(_docCache, _doc),
					new RecordContextInfo(_selectGLTran.Cache, tran)));
			}

			return this;
		}

		///<summary> 
		/// Finding inconsistency between GL Batch and its transactions.
		/// Run this method at end point of the "Release" process.
		/// Validating case:
		/// Debit and credit sums for GL Batch and its transactions should always 
		/// be the same after the "Release" process.
		///</summary>
		public DataIntegrityValidator<TRegister> CheckBatchAndTransactionsSumsForDocument(bool disableCheck = false)
		{
			if (IsSkipCheck(disableCheck)) return this;

			PXSelectBase<Batch> selectBatchAndTrans = new PXSelectJoin<
				Batch,
					InnerJoin<GLTran, 
						On<GLTran.module, Equal<Batch.module>,
					And<GLTran.batchNbr, Equal<Batch.batchNbr>>>>,
				Where<
					Batch.module, Equal<Required<Batch.module>>,
					And<Batch.batchNbr, Equal<Required<Batch.batchNbr>>>>>(_graph);

				foreach (string batchNbr in _selectGLTran
					.Select(_module, _doc.DocType, _doc.RefNbr, _referenceID).AsEnumerable()
					.Select(r => (GLTran)r)
					.Where(tran => tran.Posted != true)
					.GroupBy(tran => tran.BatchNbr)
					.Select(group => group.Key))
				{
					var set = selectBatchAndTrans.Select(_module, batchNbr).ToList();
					Batch batch = (Batch)set.FirstOrDefault();

					var result = set
						.Cast<PXResult<Batch, GLTran>>()
						.Select(item => new
						{
							DebitTotal = ((GLTran)item).DebitAmt,
							CreditTotal = ((GLTran)item).CreditAmt,
							CuryDebitTotal = ((GLTran)item).CuryDebitAmt,
							CuryCreditTotal = ((GLTran)item).CuryCreditAmt
						})
						.Aggregate((prev, next) => new
						{
							DebitTotal = prev.DebitTotal + next.DebitTotal,
							CreditTotal = prev.CreditTotal + next.CreditTotal,
							CuryDebitTotal = prev.CuryDebitTotal + next.CuryDebitTotal,
							CuryCreditTotal = prev.CuryCreditTotal + next.CuryCreditTotal
						});

				if (batch.DebitTotal != result.DebitTotal ||
						batch.CreditTotal != result.CreditTotal ||
						batch.CuryDebitTotal != result.CuryDebitTotal ||
						batch.CuryCreditTotal != result.CuryCreditTotal)
					{
					_errors.Add(
						new InconsistencyError<InconsistencyCode.batchTotalNotEqualToTransactionTotal>(
							new RecordContextInfo(_docCache, _doc),
							new RecordContextInfo(selectBatchAndTrans.Cache, batch)));
				}
			}

			return this;
		}

		///<summary> 
		/// Finding inconsistency between document and its applications.
		/// Run this method at end point of the "Release" process.
		/// Validating case:
		/// Released flag for document and its applications should always 
		/// be the same after the "Release" process.
		///</summary> 
		public DataIntegrityValidator<TRegister> CheckApplicationsReleasedForDocument<TAdjust, TAdjgDocType, TAdjgRefNbr, TReleased>(bool disableCheck = false)
			where TAdjust : class, IBqlTable, new()
			where TAdjgDocType : IBqlField
			where TAdjgRefNbr : IBqlField
			where TReleased : IBqlField
		{
			if (IsSkipCheck(disableCheck)) return this;

			PXSelectBase<TAdjust> selectTAdjust = new PXSelect<
				TAdjust,
				Where<
					TAdjgDocType, Equal<Required<TAdjgDocType>>,
					And<TAdjgRefNbr, Equal<Required<TAdjgRefNbr>>,
					And<TReleased, NotEqual<Required<TReleased>>>>>>(_graph);

				TAdjust adj = selectTAdjust.SelectSingle(_doc.DocType, _doc.RefNbr, _released);

				if (adj != null)
				{
					PXTrace.WriteInformation(_docCache.GetFullDescription(_doc));
					PXTrace.WriteInformation(selectTAdjust.Cache.GetFullDescription(adj));

				if (_released == true)
				{
					_errors.Add(new InconsistencyError<InconsistencyCode.releasedDocumentHasUnreleasedApplications>(
						new RecordContextInfo(_docCache, _doc),
						new RecordContextInfo(selectTAdjust.Cache, adj)));
				}
				else
				{
					_errors.Add(new InconsistencyError<InconsistencyCode.unreleasedDocumentHasReleasedApplications>(
						new RecordContextInfo(_docCache, _doc),
						new RecordContextInfo(selectTAdjust.Cache, adj)));
				}
			}

			return this;
		}

		public DataIntegrityValidator<TRegister> CheckDocumentHasNonNegativeBalance(bool disableCheck = false)
		{
			if (IsSkipCheck(disableCheck)
				|| ARDocType.HasNegativeAmount(_doc.DocType) == true
				|| APDocType.HasNegativeAmount(_doc.DocType) == true)
			{
				return this;
			}

			if (_doc.DocBal < 0m || _doc.CuryDocBal < 0m)
			{
				_errors.Add(new InconsistencyError<InconsistencyCode.documentNegativeBalance>(
					new RecordContextInfo(_docCache, _doc)));
	}

			return this;
		}

		public DataIntegrityValidator<TRegister> CheckDocumentTotalsConformToCurrencyPrecision(bool disableCheck = false)
		{
			if (IsSkipCheck(disableCheck)) return this;

			Company company = PXSetup<Company>.Select(_graph);

			Currency baseCurrency = PXSelect<
				Currency, 
				Where<
					Currency.curyID, Equal<Required<Currency.curyID>>>>
				.Select(_graph, company.BaseCuryID);

			Currency documentCurrency = _doc.CuryID == baseCurrency.CuryID
				? baseCurrency
				: PXSelect<
					Currency,
					Where<
						Currency.curyID, Equal<Required<Currency.curyID>>>>
					.Select(_graph, _doc.CuryID);

			short baseCurrencyDecimalPlaces = baseCurrency.DecimalPlaces ?? 0;
			short documentCurrencyDecimalPlaces = documentCurrency.DecimalPlaces ?? 0;

			decimal documentBalance = _doc.DocBal ?? 0m;
			decimal documentBalanceCurrency = _doc.CuryDocBal ?? 0m;

			decimal documentOriginalAmount = _doc.OrigDocAmt ?? 0m;
			decimal documentOriginalAmountCurrency = _doc.CuryOrigDocAmt ?? 0m;

			if (documentBalance != Math.Round(documentBalance, baseCurrencyDecimalPlaces)
				|| documentOriginalAmount != Math.Round(documentOriginalAmount, baseCurrencyDecimalPlaces)
				|| documentBalanceCurrency != Math.Round(documentBalanceCurrency, documentCurrencyDecimalPlaces)
				|| documentOriginalAmountCurrency != Math.Round(documentOriginalAmountCurrency, documentCurrencyDecimalPlaces))
			{
				_errors.Add(new InconsistencyError<InconsistencyCode.documentTotalsWrongPrecision>(
					new RecordContextInfo(_docCache, _doc)));
			}

			return this;
		}
	}
}
