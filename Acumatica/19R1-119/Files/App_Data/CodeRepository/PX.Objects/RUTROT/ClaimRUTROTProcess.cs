using System;
using System.Collections.Generic;
using System.Linq;
using PX.Data;
using PX.Objects.AR;
using PX.Objects.CM;
using PX.SM;

namespace PX.Objects.RUTROT
{
	public class ClaimRUTROTProcess : PXGraph<ClaimRUTROTProcess>
	{
		#region Selects
		public PXSave<RUTROT> Save;

		public PXSelectReadonly<RUTROT> Rutrots;

		public PXSelect<ARInvoice,
				Where<ARInvoice.docType, Equal<Required<RUTROT.docType>>,
					And<ARInvoice.refNbr, Equal<Required<RUTROT.refNbr>>>>> Documents;

		public PXSelect<ARTran,
				Where<ARTran.tranType, Equal<Required<ARInvoice.docType>>,
					And<ARTran.refNbr, Equal<Required<ARInvoice.refNbr>>>>> Lines;

		public PXSelect<RUTROTDistribution,
				Where<RUTROTDistribution.docType, Equal<Required<ARInvoice.docType>>,
					And<RUTROTDistribution.refNbr, Equal<Required<ARInvoice.refNbr>>>>> Distribution;

		public PXSelectJoin<ARPayment,
				InnerJoin<ARAdjust, 
					On<ARPayment.docType, Equal<ARAdjust.adjgDocType>, And<ARPayment.refNbr, Equal<ARAdjust.adjgRefNbr>>>,
				InnerJoin<ARInvoice, 
					On<ARAdjust.adjdDocType, Equal<ARInvoice.docType>, And<ARAdjust.adjdRefNbr, Equal<ARInvoice.refNbr>>>>>,
				Where<ARInvoice.docType, Equal<Required<ARInvoice.docType>>,
					And<ARInvoice.refNbr, Equal<Required<ARInvoice.refNbr>>,
					And<ARPayment.released, Equal<True>,
					And<ARPayment.docType, Equal<ARDocType.payment>>>>>> Payments;

		public PXSelect<GL.Branch, Where<GL.Branch.branchID, Equal<Required<GL.Branch.branchID>>>> CurrentBranch;

		#endregion

		#region Internal Types

		public class DocumentDetails
		{
			public int ClaimNbr
			{
				get;
				set;
			}

			public ARInvoice Document
			{
				get;
				set;
			}

			public RUTROT Rutrot
			{
				get;
				set;
			}

			public IEnumerable<RUTROTDistribution> Distribution
			{
				get;
				set;
			}

			public IEnumerable<ARTran> Lines
			{
				get;
				set;
			}

			public IEnumerable<Tuple<ARPayment, ARAdjust>> Payments
			{
				get;
				set;
			}

			public DistributionRounding Distributor
			{
				get;
				set;
			}

			private List<decimal> _payDistribution;

			public IEnumerable<decimal> PaymentDistribution
			{
				get
				{
					if (_payDistribution == null)
					{
						_payDistribution = DistributePayment();
					}
					return _payDistribution;
				}
			}

			private List<decimal> DistributePayment()
			{
				decimal totalClaim = Rutrot.CuryDistributedAmt ?? 1.0m;
				var shares = Distribution.Select(d => (d.CuryAmount ?? 0.0m) / totalClaim);
				decimal pct = 0.01m * (Rutrot.DeductionPct.Value);

				decimal rrToPay = (1.0m - pct) * (Rutrot.CuryTotalAmt ?? 0.0m) / pct;

				decimal relevantPay = Math.Min(TotalPay, rrToPay);

				return Distributor.DistributeInShares(relevantPay, shares).ToList();
			}

			public decimal TotalPay
			{
				get
				{
					if (Payments.Select(p => p.Item1.CuryID).All(c => c == Document.CuryID))
					{
						return Payments.Sum(p => p.Item2.CuryAdjgAmt ?? 0.0m);
					}
					else
					{
						return Payments.Sum(p => p.Item2.CuryAdjdAmt ?? 0.0m);
					}
				}
			}
		}
		#endregion

		public void BalanceDocuments(IEnumerable<RUTROT> documents)
		{
			var extDocuments = documents.Select(GetExtendedDoc);

			BalanceDocuments(extDocuments);
		}


		public void BalanceDocuments(IEnumerable<DocumentDetails> documents)
		{
			ARInvoiceEntry graph = PXGraph.CreateInstance<ARInvoiceEntry>();

			foreach (DocumentDetails details in documents)
			{
				var invoice = (ARInvoice)graph.Document.Cache.CreateCopy(details.Document);

				using (PXTransactionScope ts = new PXTransactionScope())
				{
					RUTROTHelper.BalanceARInvoiceRUTROT(graph, invoice, rutrot: details.Rutrot);

					ts.Complete();
				}
			}
		}

		public void ClaimDocuments(IEnumerable<RUTROT> documents)
		{
			var extDocuments = documents.Select(GetExtendedDoc);
			ValidateExported(extDocuments);

			string filename = CreateFilename();

			if (RUTROTHelper.IsNeedBalancing(this, RUTROTBalanceOn.Claim))
			{
				BalanceDocuments(extDocuments);
			}

			foreach (var d in extDocuments)
			{
				ClaimDocument(d.Rutrot, filename);
			}

			this.Save.Press();

			var file = ExportDocuments(extDocuments, filename);

			throw new PXRedirectToFileException(file, true);
		}

		public void ExportDocuments(IEnumerable<RUTROT> documents)
		{
			var extDocuments = documents.Select(GetExtendedDoc);
			ValidateExported(extDocuments);

			string filename = CreateFilename();
			var file = ExportDocuments(extDocuments, filename);

			throw new PXRedirectToFileException(file, true);
		}

		public void ClaimDocument(RUTROT rutrot, string filename)
		{
			rutrot.IsClaimed = true;
			Rutrots.Cache.Update(rutrot);
		}

		#region Export
		private string CreateFilename()
		{
			string name = string.Format("ROT & RUT Export {0:000}{1:yy-hhmmss}.xml", DateTime.Now.DayOfYear, DateTime.Now);
			return string.Join("", name.Where(c => !"\\/:".Contains(c)));
		}

		private int GetClaimNumber()
		{
			var branch = CurrentBranch.SelectSingle(PXAccess.GetBranchID());
			if (branch == null)
			{
				return 0;
			}
			return PXCache<GL.Branch>.GetExtension<BranchRUTROT>(branch).RUTROTClaimNextRefNbr ?? 0;
		}

		private void UpdateClaimNumber()
		{
			var branch = CurrentBranch.SelectSingle(PXAccess.GetBranchID());
			if (branch == null)
				return;

			PXCache<GL.Branch>.GetExtension<BranchRUTROT>(branch).RUTROTClaimNextRefNbr += 1;
			CurrentBranch.Cache.Update(branch);
		}

		private void UpdateInvoiceExportNumbers(int exportNbr, IEnumerable<RUTROT> documents)
		{
			foreach (var doc in documents)
			{
				var copy = (RUTROT)Rutrots.Cache.CreateCopy(doc);
				copy.ExportRefNbr = exportNbr;
				Rutrots.Cache.Update(copy);
			}
		}

		private FileInfo ExportDocuments(IEnumerable<DocumentDetails> documents, string filename)
		{
			var file = new FileInfo(Guid.NewGuid(), filename, null, CreateExportContents(documents));

			var saver = PXGraph.CreateInstance<PX.SM.UploadFileMaintenance>();
			if (saver.SaveFile(file) || file.UID == null)
			{
				AttachFileToDocuments(file.UID.Value, documents.Select(d => d.Document));
			}
			else
			{
				throw new PXException(RUTROTMessages.FailedToExport);
			}

			foreach (var d in documents)
			{
				d.Rutrot.ClaimDate = DateTime.Today;
				d.Rutrot.ClaimFileName = filename;

				Rutrots.Cache.Update(d.Rutrot);
			}

			UpdateInvoiceExportNumbers(documents.First().ClaimNbr, documents.Select(d => d.Rutrot));
			UpdateClaimNumber();

			this.Save.Press();

			return file;
		}

		#region Validation
		public void ValidateExported(IEnumerable<DocumentDetails> documents)
		{
			if (!documents.Any())
			{
				throw new PXException(RUTROTMessages.NoDocumentsSelected);
			}

			int buyersCount = documents.SelectMany(d => d.Distribution.Select(r => r.PersonalID)).Distinct().Count();

			if (buyersCount < 1)
			{
				throw new PXException(RUTROTMessages.AtLeastOneBuyerMustBeMentioned);
			}

			if (buyersCount > 100)
			{
				throw new PXException(RUTROTMessages.NoMoreThan100Buyers);
			}

			ValidateDates(documents);
			ValidateAmounts(documents);
			ValidateWorkTypes(documents);
		}

		private void ValidateDates(IEnumerable<DocumentDetails> documents)
		{
			var invoiceDates = documents.Select(d => d.Document.DocDate).Where(date => date != null).Select(date => date.Value);
			int minYear = Accessinfo.BusinessDate.Value.Year - 1;

			bool hasIncorrectDates = false;

			for (int i = 0; i < documents.Count(); i++)
			{
				var inv = documents.ElementAt(i).Document;
				if (inv.DocDate == null)
				{
					hasIncorrectDates = true;
					PXProcessing.SetError(i, new PXException(RUTROTMessages.DateShouldBeSpecifiedOnDocument));
				}

				if (inv.DocDate.Value.Year < minYear)
				{
					hasIncorrectDates = true;
					PXProcessing.SetError(i, new PXException(RUTROTMessages.DocumentDateIsBelowAllowed, new DateTime(minYear, 1, 1)));
				}
			}

			if (hasIncorrectDates)
			{
				throw new PXException(RUTROTMessages.SomeDocumentDatesIncorrect);
			}

			if (invoiceDates.Select(d => d.Year).Distinct().Count() > 1)
			{
				throw new PXException(RUTROTMessages.AllDocumentsMustBeSameYear);
			}

			bool hasTooLatePayments = false;

			for (int i = 0; i < documents.Count(); i++)
			{
				foreach (var pay in documents.ElementAt(i).Payments.Select(p => p.Item2))
				{
					if (pay.AdjgDocDate > Accessinfo.BusinessDate)
					{
						hasTooLatePayments = true;
						PXProcessing.SetError(i, new PXException(RUTROTMessages.PaymentDatesMustNotExceedClaimDate));
						break;
					}
				}
			}

			if (hasTooLatePayments)
			{
				throw new PXException(RUTROTMessages.PaymentDatesMustNotExceedClaimDate);
			}
		}

		private void ValidateAmounts(IEnumerable<DocumentDetails> documents)
		{
			bool hasAmountProblems = false;

			for (int i = 0; i < documents.Count(); i++)
			{
				decimal paid = documents.ElementAt(i).TotalPay;
				decimal claim = documents.ElementAt(i).Rutrot.CuryDistributedAmt ?? 0.0m;

				if (claim + paid - (documents.ElementAt(i).Document.CuryOrigDocAmt ?? 0.0m) > 0)
				{
					hasAmountProblems = true;
					PXProcessing.SetError(i, new PXException(RUTROTMessages.ClaimedPaidMustNotExceedTotal));
				}

				foreach (RUTROTDistribution distribution in documents.ElementAt(i).Distribution)
				{
					if (distribution.CuryAmount > distribution.CuryAllowance)
					{
						hasAmountProblems = true;
						PXProcessing.SetError(i, new PXException(RUTROTMessages.ClaimedTotalTooMuch));
					}
				}
			}

			if (hasAmountProblems)
			{
				throw new PXException(RUTROTMessages.SomeAmountsIncorrect);
			}
		}

		public void ValidateWorkTypes(IEnumerable<DocumentDetails> documents)
		{
			Dictionary<int, RUTROTWorkType> availableWorkTypes = GetAvailableWorkTypes(this, this.Accessinfo.BusinessDate);
			bool haveUnavailableWorkTypes = false;
			for (int i = 0; i < documents.Count(); i++)
			{
				foreach (ARTran tran in documents.ElementAt(i).Lines)
				{
					ARTranRUTROT tranRR = RUTROTHelper.GetExtensionNullable<ARTran, ARTranRUTROT>(tran);
					if (tranRR?.IsRUTROTDeductible == true &&
						(tranRR.RUTROTItemType == RUTROTItemTypes.MaterialCost || tranRR.RUTROTItemType == RUTROTItemTypes.Service))
					{
						if (!availableWorkTypes.ContainsKey(tranRR.RUTROTWorkTypeID.Value))
						{
							haveUnavailableWorkTypes = true;
							PXProcessing.SetError(i, new PXException(RUTROTMessages.CannotClaimWorkType));
						}
					}
				}
			}
			if (haveUnavailableWorkTypes)
			{
				throw new PXException(RUTROTMessages.CannotClaimWorkType);
			}
		}
#endregion

		protected virtual byte[] CreateExportContents(IEnumerable<DocumentDetails> documents)
		{
			bool isRut = documents.First().Rutrot.RUTROTType == RUTROTTypes.RUT;
			Dictionary<int, RUTROTWorkType> availableWorkTypes = GetAvailableWorkTypes(this, this.Accessinfo.BusinessDate);

			var exporter = isRut ? new RUTExport(this, availableWorkTypes) : new ROTExport(this, availableWorkTypes);

			return exporter.Export(documents);
		}

		private Dictionary<int, RUTROTWorkType> GetAvailableWorkTypes(PXGraph graph, DateTime? date)
		{
			if (date == null)
			{
				return null;
			}
			Dictionary<int, RUTROTWorkType> availableWorkTypes = new Dictionary<int, RUTROTWorkType>();
			foreach (RUTROTWorkType workType in PXSelectOrderBy<RUTROTWorkType, 
				OrderBy<Asc<RUTROTWorkType.rUTROTType, Asc<RUTROTWorkType.position>>>>.Select(graph))
			{
				if (RUTROTHelper.IsUpToDateWorkType(workType, date.Value))
				{
					availableWorkTypes.Add(workType.WorkTypeID.Value, workType);
				}
			}
			return availableWorkTypes;
		}

		private void AttachFileToDocuments(Guid fileId, IEnumerable<ARInvoice> documents)
		{
			var fcache = this.Caches[typeof(NoteDoc)];
			foreach (var d in documents)
			{
				var fileNote = new NoteDoc { NoteID = d.NoteID, FileID = fileId };
				fcache.Insert(fileNote);

				this.Persist(typeof(NoteDoc), PXDBOperation.Insert);
			}
		}
#endregion

#region Getting Doc Details
		private DocumentDetails GetExtendedDoc(RUTROT inv)
		{
			ARInvoice origInv = Documents.Select(inv.DocType, inv.RefNbr);

			return new DocumentDetails
			{
				ClaimNbr = GetClaimNumber(),
				Document = origInv,
				Rutrot = inv,
				Distribution = Distribution.Select(inv.DocType, inv.RefNbr).Select(r => (RUTROTDistribution)r).ToList(),
				Lines = GetLines(origInv),
				Payments = GetPayments(origInv),
				Distributor = GetDistributor(origInv.CuryInfoID)
			};
		}

		private IEnumerable<ARTran> GetLines(ARInvoice invoice)
		{
			var trans = Lines.Select(invoice.DocType, invoice.RefNbr).Select(r => (ARTran)r);
			return trans.ToList();
		}

		private IEnumerable<Tuple<ARPayment, ARAdjust>> GetPayments(ARInvoice invoice)
		{
			var payments = Payments.Select(invoice.DocType, invoice.RefNbr).AsEnumerable();
			return payments.Select(p => new Tuple<ARPayment, ARAdjust>((ARPayment)p, p.GetItem<ARAdjust>())).ToList();
		}
#endregion

		private DistributionRounding GetDistributor(long? curyInfoID)
		{
			var arsetup = new PXSelect<ARSetup>(this).SelectSingle();
			var curyInfo = new PXSelect<CurrencyInfo, 
				Where<CurrencyInfo.curyInfoID, Equal<Required<ARInvoice.curyInfoID>>>>(this).SelectSingle(curyInfoID);

			if (curyInfo == null)
			{
				return new DistributionRounding(arsetup, PXAccess.FeatureInstalled<PX.Objects.CS.FeaturesSet.invoiceRounding>())
				{
					CuryPlaces = 0,
					PreventOverflow = true
				};
			}
			else
			{
				return new DistributionRounding(arsetup, PXAccess.FeatureInstalled<PX.Objects.CS.FeaturesSet.invoiceRounding>())
				{
					CuryPlaces = curyInfo.CuryPrecision ?? 0,
					PreventOverflow = true
				};
			}
		}
	}
}
