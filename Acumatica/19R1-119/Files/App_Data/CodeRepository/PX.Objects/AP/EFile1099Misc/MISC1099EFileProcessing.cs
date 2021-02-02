using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Data;
using PX.Objects.CR;
using System.Collections;
using System.IO;
using PX.Api;
using PX.Common;
using PX.Objects.AP.Overrides.APDocumentRelease;
using PX.Objects.CS;
using PX.Objects.CS.DAC;
using PX.Objects.GL;
using PX.Objects.GL.DAC;

namespace PX.Objects.AP
{
	public class MISC1099EFileProcessing : PXGraph<MISC1099EFileProcessing>
	{
		#region Declaration
		public PXCancel<MISC1099EFileFilter> Cancel;

		public PXFilter<MISC1099EFileFilter> Filter;

		[PXFilterable]
		public PXFilteredProcessingOrderBy<MISC1099EFileProcessingInfo, MISC1099EFileFilter, OrderBy<Asc<MISC1099EFileProcessingInfo.payerBranchID, Asc<MISC1099EFileProcessingInfo.vendorID>>>> Records;

		public PXSelectJoin<Organization,
			LeftJoin<OrganizationBAccount, 
				On<Organization.bAccountID, Equal<OrganizationBAccount.bAccountID>>,
			LeftJoin<Contact, 
				On<OrganizationBAccount.bAccountID, Equal<Contact.bAccountID>,
					And<OrganizationBAccount.defContactID, Equal<Contact.contactID>>>,
			LeftJoin<Address, 
				On<OrganizationBAccount.bAccountID, Equal<Address.bAccountID>,
					And<OrganizationBAccount.defAddressID, Equal<Address.addressID>>>,
			LeftJoin<LocationExtAddress, 
				On<OrganizationBAccount.bAccountID, Equal<LocationExtAddress.locationBAccountID>,
					And<OrganizationBAccount.defLocationID, Equal<LocationExtAddress.locationID>>>>>>>,
			Where<Organization.organizationID, Equal<Optional<MISC1099EFileFilter.organizationID>>>> Payer;

		private int RecordCounter;

		protected int?[] MarkedOrganizations
		{
			get
			{
				if (Filter.Current != null && Filter.Current.OrganizationID != null)
				{
					if (Filter.Current.Include == MISC1099EFileFilter.include.AllMarkedOrganizations)
					{
						return PXSelectorAttribute.SelectAll<MISC1099EFileFilter.organizationID>(Filter.Cache, Filter.Current)
							.RowCast<Organization>()
							.Select(o => o.OrganizationID)
							.ToArray();
					}
					return new[] { Filter.Current.OrganizationID };
				}
				return new int?[] { };
			}
		}

		#endregion
		public IEnumerable records()
		{
			this.Caches<MISC1099EFileProcessingInfoRaw>().Clear();
			this.Caches<MISC1099EFileProcessingInfoRaw>().ClearQueryCache();

			if (Filter.Current?.OrganizationID == null) yield break;

			using (new PXReadBranchRestrictedScope(MarkedOrganizations, null))
			{
				// TODO: Workaround awaiting AC-64107
				// -
				IEnumerable<MISC1099EFileProcessingInfo> list = PXSelect<
					MISC1099EFileProcessingInfoRaw,
					Where<
						MISC1099EFileProcessingInfoRaw.finYear, Equal<Current<MISC1099EFileFilter.finYear>>,
						And<Current<MISC1099EFileFilter.organizationID>, IsNotNull>>>
					.Select(this)
					.RowCast<MISC1099EFileProcessingInfoRaw>()
					.GroupBy(rawInfo => new { rawInfo.PayerOrganizationID, rawInfo.VendorID })
					.Select(group => new MISC1099EFileProcessingInfo
					{
						BranchID = group.First().BranchID,
						PayerOrganizationID = group.Key.PayerOrganizationID,
						BoxNbr = group.First().BoxNbr,
						FinYear = group.First().FinYear,
						VendorID = group.Key.VendorID,
						VAcctCD = group.First().VAcctCD,
						VAcctName = group.First().VAcctName,
						LTaxRegistrationID = group.First().LTaxRegistrationID,
						HistAmt = group.Sum(h => h.HistAmt)
					})
					.ToArray();

				foreach (MISC1099EFileProcessingInfo record in list)
				{
					MISC1099EFileProcessingInfo located = Records.Cache.Locate(record) as MISC1099EFileProcessingInfo;
					yield return (located ?? Records.Cache.Insert(record));
				}
				//There is "masterBranch is not null" here because select parameters must be changed after changing masterBranch. 
				//If select parametrs is unchanged since last use this select will not be executed and return previous result.
			}
		}

		public MISC1099EFileProcessing()
		{
			Records.SetProcessTooltip(Messages.EFiling1099SelectedVendorsTooltip);
			Records.SetProcessAllTooltip(Messages.EFiling1099AllVendorsTooltip);
		}


		protected virtual void MISC1099EFileFilter_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			MISC1099EFileFilter oldRow = (MISC1099EFileFilter)e.OldRow;
			MISC1099EFileFilter newRow = (MISC1099EFileFilter)e.Row;
			if (oldRow.FinYear != newRow.FinYear || 
				oldRow.OrganizationID != newRow.OrganizationID || 
				oldRow.Box7 != newRow.Box7)
			{
				Records.Cache.Clear();
			}
		}

	    protected virtual void MISC1099EFileFilter_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			MISC1099EFileFilter rowfilter = e.Row as MISC1099EFileFilter;
			if (rowfilter == null) return;

			Records.SetProcessDelegate(
				delegate (List<MISC1099EFileProcessingInfo> list)
				{
					MISC1099EFileProcessing graph = CreateInstance<MISC1099EFileProcessing>();
					graph.Process(list, rowfilter);
				});

			if (rowfilter.Include == MISC1099EFileFilter.include.AllMarkedOrganizations)
			{
				string unmarkedOrganizations = PXSelectJoinGroupBy<Organization,
					InnerJoin<Branch,
						On<Organization.organizationID, Equal<Branch.organizationID>>>,
					Where2<Where<Organization.reporting1099, NotEqual<True>, Or<Organization.reporting1099, IsNull>>,
						And<MatchWithBranch<Branch.branchID>>>,
					Aggregate<GroupBy<Organization.organizationID>>>.SelectWindowed(this, 0, 10)
					.RowCast<Organization>()
					.Select(organization => organization.OrganizationCD)
					.JoinToString(", ");

				sender.RaiseExceptionHandling<MISC1099EFileFilter.include>(rowfilter, rowfilter.Include,
					!string.IsNullOrEmpty(unmarkedOrganizations)
						? new PXSetPropertyException(Messages.Unefiled1099Organizations, PXErrorLevel.Warning, unmarkedOrganizations)
						: null);
			}
			else
			{
				sender.RaiseExceptionHandling<MISC1099EFileFilter.include>(rowfilter, rowfilter.Include, null);
			}
		}

		public void Process(List<MISC1099EFileProcessingInfo> records, MISC1099EFileFilter filter)
		{
			using (new PXReadBranchRestrictedScope(MarkedOrganizations, null, requireAccessForAllSpecified:true))
			{
				using (MemoryStream stream = new MemoryStream())
				{
					using (StreamWriter sw = new StreamWriter(stream, Encoding.Unicode))
					{
						BranchMaint graph = CreateInstance<BranchMaint>();

						foreach (PXResult<Organization, OrganizationBAccount, Contact, Address, LocationExtAddress> transmitter in Payer.Select(filter.OrganizationID))
						{
							TransmitterTRecord trecord = CreateTransmitterRecord(transmitter, transmitter, transmitter, transmitter, filter, 0);
							List<object> data1099Misc = new List<object> { trecord };

							List<IGrouping<int?, MISC1099EFileProcessingInfo>> groups = records.GroupBy(rec => rec.PayerOrganizationID).ToList();
							foreach (IGrouping<int?, MISC1099EFileProcessingInfo> group in groups)
							{
								foreach (PXResult<Organization, OrganizationBAccount, Contact, Address, LocationExtAddress> payer in Payer.Select(@group.Key))
								{
									Contact rowShipContact = PXSelectJoin<Contact,
										InnerJoin<Location, On<Contact.bAccountID, Equal<Location.bAccountID>,
											And<Contact.contactID, Equal<Location.defContactID>>>>,
										Where<Location.bAccountID, Equal<Required<BAccount.bAccountID>>,
											And<Location.locationID, Equal<Required<BAccount.defLocationID>>>>>
											.Select(graph, ((OrganizationBAccount)payer).BAccountID, ((OrganizationBAccount)payer).DefLocationID);

									data1099Misc.Add(CreatePayerARecord(payer, payer, payer, payer, payer, rowShipContact, filter));

									List<PayeeRecordB> payeeRecs = new List<PayeeRecordB>();
									foreach (MISC1099EFileProcessingInfo rec in @group)
									{
										PXProcessing<MISC1099EFileProcessingInfo>.SetCurrentItem(rec);
										payeeRecs.Add(CreatePayeeBRecord(graph, payer, rec, filter));
										PXProcessing<MISC1099EFileProcessingInfo>.SetProcessed();
									}
									payeeRecs = payeeRecs.WhereNotNull().ToList();
									trecord.TotalNumberofPayees = payeeRecs.Count.ToString();
									data1099Misc.AddRange(payeeRecs);
									data1099Misc.Add(CreateEndOfPayerRecordC(payeeRecs));

									//If combined State Filer then only generate K Record.
									if (((Organization)payer).CFSFiler == true)
									{
										data1099Misc.AddRange(payeeRecs
											.Where(x => !string.IsNullOrWhiteSpace(x.PayeeState))
											.GroupBy(x => x.PayeeState.Trim(), StringComparer.CurrentCultureIgnoreCase)
											.Select(y => CreateStateTotalsRecordK(y.ToList()))
											.Where(kRecord => kRecord != null));
									}
								}
							}

							data1099Misc.Add(CreateEndOfTransmissionRecordF(groups.Count, records.Count));

							//Write to file
							FixedLengthFile flatFile = new FixedLengthFile();

							flatFile.WriteToFile(data1099Misc, sw);
							sw.Flush();

							const string path = "1099-MISC.txt";
							PX.SM.FileInfo info = new PX.SM.FileInfo(path, null, stream.ToArray());

							throw new PXRedirectToFileException(info, true);
						}
					}
				}
			}
		}

		public TransmitterTRecord CreateTransmitterRecord(OrganizationBAccount orgBAccount, 
															Organization organization,
															Contact rowMainContact,
															Address rowMainAddress,
															MISC1099EFileFilter filter,
															int totalPayeeB)
		{
			return new TransmitterTRecord
			{
				RecordType = "T",
				PaymentYear = filter.FinYear,
				PriorYearDataIndicator = filter.IsPriorYear == true ? "P" : string.Empty,
				TransmitterTIN = orgBAccount.TaxRegistrationID,
				TransmitterControlCode = organization.TCC,
				Blank1 = string.Empty,
				TestFileIndicator = filter.IsTestMode == true ? "T" : string.Empty,
				ForeignEntityIndicator = organization.ForeignEntity == true ? "1" : string.Empty,

				TransmitterName = rowMainContact.FullName.Trim(),
				CompanyName = rowMainContact.FullName.Trim(),

				CompanyMailingAddress = string.Concat(rowMainAddress.AddressLine1, rowMainAddress.AddressLine2),
				CompanyCity = rowMainAddress.City,
				CompanyState = rowMainAddress.State,
				CompanyZipCode = rowMainAddress.PostalCode,
				Blank2 = string.Empty,
				//Setup at the end - dependent of Payee B records
				TotalNumberofPayees = totalPayeeB.ToString(),
				ContactName = organization.ContactName,
				ContactTelephoneAndExt = organization.CTelNumber,
				ContactEmailAddress = organization.CEmail,
				Blank3 = string.Empty,
				RecordSequenceNumber = (++RecordCounter).ToString(),
				Blank4 = string.Empty,

				VendorIndicator = "V",
				VendorName = TRecordVendorInfo.VendorName,
				VendorMailingAddress = TRecordVendorInfo.VendorMailingAddress,
				VendorCity = TRecordVendorInfo.VendorCity,
				VendorState = TRecordVendorInfo.VendorState,
				VendorZipCode = TRecordVendorInfo.VendorZipCode,
				VendorContactName = TRecordVendorInfo.VendorContactName,
				VendorContactTelephoneAndExt = TRecordVendorInfo.VendorContactTelephoneAndExt,

				Blank5 = string.Empty,

				#region Check - Vendor or Branch?
				VendorForeignEntityIndicator = TRecordVendorInfo.VendorForeignEntityIndicator,
				#endregion

				Blank6 = string.Empty,
				Blank7 = string.Empty,
			};
		}

		public PayerRecordA CreatePayerARecord(OrganizationBAccount orgBAccount,
												Organization organization,
												Contact rowMainContact,
												Address rowMainAddress,
												LocationExtAddress rowShipInfo,
												Contact rowShipContact,
												MISC1099EFileFilter filter)
		{
			string companyName1 = rowMainContact.FullName.Trim();
			string companyName2 = string.Empty;
			if (companyName1.Length > 40)
			{
				companyName2 = companyName1.Substring(40);
				companyName1 = companyName1.Substring(0, 40);
			}
			return new PayerRecordA
			{
				RecordType = "A",
				PaymentYear = filter.FinYear,
				CombinedFederalORStateFiler = organization.CFSFiler == true ? "1" : string.Empty,
				Blank1 = string.Empty,
				PayerTaxpayerIdentificationNumberTIN = orgBAccount.TaxRegistrationID,

				PayerNameControl = organization.NameControl,

				LastFilingIndicator = filter.IsLastFiling == true ? "1" : string.Empty,

				TypeofReturn = "A",
				AmountCodes = (filter.ReportingDirectSalesOnly == true) ? "1" : "12345678ABCDE",

				Blank2 = string.Empty,
				ForeignEntityIndicator = organization.ForeignEntity == true ? "1" : string.Empty,
				FirstPayerNameLine = companyName1,
				SecondPayerNameLine = companyName2,

				#region Check with Gabriel, we need Transfer Agent or no
				TransferAgentIndicator = "0",
				#endregion

				PayerShippingAddress = string.Concat(rowShipInfo.AddressLine1, rowShipInfo.AddressLine2),
				PayerCity = rowShipInfo.City,
				PayerState = rowShipInfo.State,
				PayerZipCode = rowShipInfo.PostalCode,

				PayerTelephoneAndExt = rowShipContact.Phone1,

				Blank3 = string.Empty,
				RecordSequenceNumber = (++RecordCounter).ToString(),
				Blank4 = string.Empty,
				Blank5 = string.Empty
			};
		}

		public PayeeRecordB CreatePayeeBRecord(BranchMaint graph, Organization organization, MISC1099EFileProcessingInfo Record1099, MISC1099EFileFilter filter)
		{
			PayeeRecordB bRecord;
			graph.Caches<AP1099History>().ClearQueryCache();
			using (new PXReadBranchRestrictedScope(new[] { Record1099.PayerOrganizationID }, null, requireAccessForAllSpecified:true))
			{
				VendorR rowVendor = PXSelect<VendorR, Where<VendorR.bAccountID, Equal<Required<VendorR.bAccountID>>>>.Select(graph, Record1099.VendorID);

				Contact rowVendorContact = PXSelect<Contact,
					Where<Contact.bAccountID, Equal<Required<BAccount.bAccountID>>,
						And<Contact.contactID, Equal<Required<BAccount.defContactID>>>>>.Select(graph, rowVendor.BAccountID, rowVendor.DefContactID);
				Address rowVendorAddress = PXSelect<Address,
					Where<Address.bAccountID, Equal<Required<BAccount.bAccountID>>,
						And<Address.addressID, Equal<Required<BAccount.defAddressID>>>>>.Select(graph, rowVendor.BAccountID, rowVendor.DefAddressID);

				LocationExtAddress rowVendorShipInfo = PXSelect<LocationExtAddress,
					Where<LocationExtAddress.locationBAccountID, Equal<Required<BAccount.bAccountID>>,
						And<LocationExtAddress.locationID, Equal<Required<BAccount.defLocationID>>>>>.Select(graph, rowVendor.BAccountID, rowVendor.DefLocationID);

				List<AP1099History> amtList1099 = PXSelectJoinGroupBy<AP1099History,
					InnerJoin<AP1099Box, On<AP1099History.boxNbr, Equal<AP1099Box.boxNbr>>>,
					Where<AP1099History.vendorID, Equal<Required<AP1099History.vendorID>>,
						And<AP1099History.finYear, Equal<Required<AP1099History.finYear>>,
						And<Where<
							Required<MISC1099EFileFilter.box7>, Equal<MISC1099EFileFilter.box7.box7All>,
							Or<Required<MISC1099EFileFilter.box7>, Equal<MISC1099EFileFilter.box7.box7Equal>,
								And<AP1099History.boxNbr, Equal<MISC1099EFileFilter.box7.box7Nbr>,
							Or<Required<MISC1099EFileFilter.box7>, Equal<MISC1099EFileFilter.box7.box7NotEqual>,
								And<AP1099History.boxNbr, NotEqual<MISC1099EFileFilter.box7.box7Nbr>>>>>>>>>,
					Aggregate<
						GroupBy<AP1099History.boxNbr,
						Sum<AP1099History.histAmt>>>>
						.Select(graph, Record1099.VendorID, filter.FinYear, filter.Box7, filter.Box7, filter.Box7).AsEnumerable()
						.Where(res => res.GetItem<AP1099History>().HistAmt >= res.GetItem<AP1099Box>().MinReportAmt)
						.RowCast<AP1099History>()
						.ToList();

				if ((amtList1099.Sum(hist => hist.HistAmt) ?? 0m) == 0m) return null;

				bRecord = new PayeeRecordB
				{
					RecordType = "B",
					PaymentYear = filter.FinYear,

					//ALWAYS G since we have one Payee record per file.
					CorrectedReturnIndicator = filter.IsCorrectionReturn == true ? "G" : string.Empty,

					NameControl = string.Empty,

					#region confirmed with Gabriel - ALWAYS Business
					TypeOfTIN = "1",
					#endregion

					PayerTaxpayerIdentificationNumberTIN = rowVendorShipInfo.TaxRegistrationID,

					PayerAccountNumberForPayee = rowVendor.AcctCD,

					#region Check with Gabriel, not sure about this
					PayerOfficeCode = string.Empty,
					#endregion

					Blank1 = string.Empty,

					PaymentAmount1 = filter.ReportingDirectSalesOnly == true ? 0m : Math.Round((amtList1099.FirstOrDefault(v => (v != null && v.BoxNbr == 1)) ?? new AP1099Hist { HistAmt = 0m }).HistAmt ?? 0m, 2),
					PaymentAmount2 = filter.ReportingDirectSalesOnly == true ? 0m : Math.Round((amtList1099.FirstOrDefault(v => (v != null && v.BoxNbr == 2)) ?? new AP1099Hist { HistAmt = 0m }).HistAmt ?? 0m, 2),
					PaymentAmount3 = filter.ReportingDirectSalesOnly == true ? 0m : Math.Round((amtList1099.FirstOrDefault(v => (v != null && v.BoxNbr == 3)) ?? new AP1099Hist { HistAmt = 0m }).HistAmt ?? 0m, 2),
					PaymentAmount4 = filter.ReportingDirectSalesOnly == true ? 0m : Math.Round((amtList1099.FirstOrDefault(v => (v != null && v.BoxNbr == 4)) ?? new AP1099Hist { HistAmt = 0m }).HistAmt ?? 0m, 2),
					PaymentAmount5 = filter.ReportingDirectSalesOnly == true ? 0m : Math.Round((amtList1099.FirstOrDefault(v => (v != null && v.BoxNbr == 5)) ?? new AP1099Hist { HistAmt = 0m }).HistAmt ?? 0m, 2),
					PaymentAmount6 = filter.ReportingDirectSalesOnly == true ? 0m : Math.Round((amtList1099.FirstOrDefault(v => (v != null && v.BoxNbr == 6)) ?? new AP1099Hist { HistAmt = 0m }).HistAmt ?? 0m, 2),
					PaymentAmount7 = filter.ReportingDirectSalesOnly == true ? 0m : Math.Round((amtList1099.FirstOrDefault(v => (v != null && v.BoxNbr == 7)) ?? new AP1099Hist { HistAmt = 0m }).HistAmt ?? 0m, 2),
					PaymentAmount8 = filter.ReportingDirectSalesOnly == true ? 0m : Math.Round((amtList1099.FirstOrDefault(v => (v != null && v.BoxNbr == 8)) ?? new AP1099Hist { HistAmt = 0m }).HistAmt ?? 0m, 2),
					//
					//Need Box 11???
					PaymentAmount9 = filter.ReportingDirectSalesOnly == true ? 0m : Math.Round((amtList1099.FirstOrDefault(v => (v != null && v.BoxNbr == 9)) ?? new AP1099Hist { HistAmt = 0m }).HistAmt ?? 0m, 2),
					//
					PaymentAmountA = filter.ReportingDirectSalesOnly == true ? 0m : Math.Round((amtList1099.FirstOrDefault(v => (v != null && v.BoxNbr == 10)) ?? new AP1099Hist { HistAmt = 0m }).HistAmt ?? 0m, 2),
					PaymentAmountB = filter.ReportingDirectSalesOnly == true ? 0m : Math.Round((amtList1099.FirstOrDefault(v => (v != null && v.BoxNbr == 13)) ?? new AP1099Hist { HistAmt = 0m }).HistAmt ?? 0m, 2),
					PaymentAmountC = filter.ReportingDirectSalesOnly == true ? 0m : Math.Round((amtList1099.FirstOrDefault(v => (v != null && v.BoxNbr == 14)) ?? new AP1099Hist { HistAmt = 0m }).HistAmt ?? 0m, 2),
					Payment = filter.ReportingDirectSalesOnly == true ? 0m : Math.Round((amtList1099.FirstOrDefault(v => (v != null && v.BoxNbr == 151)) ?? new AP1099Hist { HistAmt = 0m }).HistAmt ?? 0m, 2),
					PaymentAmountE = filter.ReportingDirectSalesOnly == true ? 0m : Math.Round((amtList1099.FirstOrDefault(v => (v != null && v.BoxNbr == 152)) ?? new AP1099Hist { HistAmt = 0m }).HistAmt ?? 0m, 2),
					PaymentAmountF = 0m,
					PaymentAmountG = 0m,

					ForeignCountryIndicator = rowVendor.ForeignEntity == true ? "1" : string.Empty,
					PayeeNameLine = rowVendorContact.FullName,

					Blank2 = string.Empty,

					PayeeMailingAddress = string.Concat(rowVendorAddress.AddressLine1, rowVendorAddress.AddressLine2),

					Blank3 = string.Empty,

					PayeeCity = rowVendorAddress.City,
					PayeeState = rowVendorAddress.State,
					PayeeZipCode = rowVendorAddress.PostalCode,

					Blank4 = string.Empty,

					RecordSequenceNumber = (++RecordCounter).ToString(),

					Blank5 = string.Empty,

					#region Confirmed with Gabriel, Skip for now
					SecondTINNotice = string.Empty,
					#endregion

					Blank6 = string.Empty,

					#region Check - Dependent on Box 9 - check in 3rd party
					DirectSalesIndicator = GetDirectSaleIndicator(graph, Record1099.VendorID.Value, filter.FinYear),
					#endregion

					FATCA = rowVendor.FATCA == true ? "1" : string.Empty,

					Blank7 = string.Empty,

					#region Confirmed with Gabriel, skip for now
					SpecialDataEntries = string.Empty,
					StateIncomeTaxWithheld = string.Empty,
					LocalIncomeTaxWithheld = string.Empty,
					#endregion

					CombineFederalOrStateCode = organization.CFSFiler == true ? GetCombinedFederalOrStateCode(rowVendorAddress.State) : string.Empty,

					Blank8 = string.Empty,
				};
			}
			return bRecord;
		}

		public EndOfPayerRecordC CreateEndOfPayerRecordC(List<PayeeRecordB> listPayeeB)
		{
			return new EndOfPayerRecordC
			{
				RecordType = "C",
				// At the End, Total# of B Records
				NumberOfPayees = Convert.ToString(listPayeeB.Count),
				Blank1 = string.Empty,

				#region Totals
				ControlTotal1 = Math.Round(listPayeeB.Sum(brec => brec.PaymentAmount1), 2),
				ControlTotal2 = Math.Round(listPayeeB.Sum(brec => brec.PaymentAmount2), 2),
				ControlTotal3 = Math.Round(listPayeeB.Sum(brec => brec.PaymentAmount3), 2),
				ControlTotal4 = Math.Round(listPayeeB.Sum(brec => brec.PaymentAmount4), 2),
				ControlTotal5 = Math.Round(listPayeeB.Sum(brec => brec.PaymentAmount5), 2),
				ControlTotal6 = Math.Round(listPayeeB.Sum(brec => brec.PaymentAmount6), 2),
				ControlTotal7 = Math.Round(listPayeeB.Sum(brec => brec.PaymentAmount7), 2),
				ControlTotal8 = Math.Round(listPayeeB.Sum(brec => brec.PaymentAmount8), 2),
				ControlTotal9 = Math.Round(listPayeeB.Sum(brec => brec.PaymentAmount9), 2),
				ControlTotalA = Math.Round(listPayeeB.Sum(brec => brec.PaymentAmountA), 2),
				ControlTotalB = Math.Round(listPayeeB.Sum(brec => brec.PaymentAmountB), 2),
				ControlTotalC = Math.Round(listPayeeB.Sum(brec => brec.PaymentAmountC), 2),
				ControlTotalD = Math.Round(listPayeeB.Sum(brec => brec.Payment), 2),
				ControlTotalE = Math.Round(listPayeeB.Sum(brec => brec.PaymentAmountE), 2),
				ControlTotalF = Math.Round(listPayeeB.Sum(brec => brec.PaymentAmountF), 2),
				ControlTotalG = Math.Round(listPayeeB.Sum(brec => brec.PaymentAmountG), 2),
				#endregion

				Blank2 = string.Empty,
				RecordSequenceNumber = (++RecordCounter).ToString(),
				Blank3 = string.Empty,
				Blank4 = string.Empty
			};
		}

		public StateTotalsRecordK CreateStateTotalsRecordK(List<PayeeRecordB> listPayeeB)
		{
			if (listPayeeB == null) return null;

			string stateInfo = (listPayeeB.FirstOrDefault() ?? new PayeeRecordB()).PayeeState;
			string CSFCCode = GetCombinedFederalOrStateCode(stateInfo);

			//Do not include K Record if State do not participate in CF/SF Program
			if (string.IsNullOrEmpty(CSFCCode)) return null;

			return new StateTotalsRecordK
			{
				RecordType = "K",

				NumberOfPayees = Convert.ToString(listPayeeB.Count),
				Blank1 = string.Empty,

				#region Totals
				ControlTotal1 = Math.Round(listPayeeB.Sum(brec => brec.PaymentAmount1), 2),
				ControlTotal2 = Math.Round(listPayeeB.Sum(brec => brec.PaymentAmount2), 2),
				ControlTotal3 = Math.Round(listPayeeB.Sum(brec => brec.PaymentAmount3), 2),
				ControlTotal4 = Math.Round(listPayeeB.Sum(brec => brec.PaymentAmount4), 2),
				ControlTotal5 = Math.Round(listPayeeB.Sum(brec => brec.PaymentAmount5), 2),
				ControlTotal6 = Math.Round(listPayeeB.Sum(brec => brec.PaymentAmount6), 2),
				ControlTotal7 = Math.Round(listPayeeB.Sum(brec => brec.PaymentAmount7), 2),
				ControlTotal8 = Math.Round(listPayeeB.Sum(brec => brec.PaymentAmount8), 2),
				ControlTotal9 = Math.Round(listPayeeB.Sum(brec => brec.PaymentAmount9), 2),
				ControlTotalA = Math.Round(listPayeeB.Sum(brec => brec.PaymentAmountA), 2),
				ControlTotalB = Math.Round(listPayeeB.Sum(brec => brec.PaymentAmountB), 2),
				ControlTotalC = Math.Round(listPayeeB.Sum(brec => brec.PaymentAmountC), 2),
				ControlTotalD = Math.Round(listPayeeB.Sum(brec => brec.Payment), 2),
				ControlTotalE = Math.Round(listPayeeB.Sum(brec => brec.PaymentAmountE), 2),
				ControlTotalF = Math.Round(listPayeeB.Sum(brec => brec.PaymentAmountF), 2),
				ControlTotalG = Math.Round(listPayeeB.Sum(brec => brec.PaymentAmountG), 2),
				#endregion

				Blank2 = string.Empty,
				RecordSequenceNumber = (++RecordCounter).ToString(),
				Blank3 = string.Empty,

				#region State and Local
				StateIncomeTaxWithheldTotal = 0m,
				LocalIncomeTaxWithheldTotal = 0m,
				#endregion

				Blank4 = string.Empty,

				//Check if new field needed
				CombinedFederalOrStateCode = CSFCCode,
				Blank5 = string.Empty
			};
		}

		public EndOfTransmissionRecordF CreateEndOfTransmissionRecordF(int totalPayerA, int totalPayeeB)
		{
			return new EndOfTransmissionRecordF()
			{
				RecordType = "F",
				// At the End, Total# of B Records
				NumberOfARecords = totalPayerA.ToString(),
				Zero1 = "0",

				Blank1 = string.Empty,

				TotalNumberOfPayees = totalPayeeB.ToString(),

				Blank2 = string.Empty,
				RecordSequenceNumber = (++RecordCounter).ToString(),

				Blank3 = string.Empty,
				Blank4 = string.Empty,
			};
		}

		public PXAction<MISC1099EFileFilter> View1099Summary;
		[PXUIField(DisplayName = "View 1099 Vendor History", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton(VisibleOnProcessingResults = true)]
		public virtual IEnumerable view1099Summary(PXAdapter adapter)
		{
			if (Records.Current != null)
			{
				AP1099DetailEnq graph = CreateInstance<AP1099DetailEnq>();
				graph.YearVendorHeader.Current.FinYear = Records.Current.FinYear;
				graph.YearVendorHeader.Current.VendorID = Records.Current.VendorID;
				graph.YearVendorHeader.Current.OrganizationID = Filter.Current.OrganizationID;
				throw new PXRedirectRequiredException(graph, true, "1099 Year Vendor History");
			}
			return adapter.Get();
		}

		private static string GetCombinedFederalOrStateCode(string stateAbbrCode)
		{
			stateAbbrCode = stateAbbrCode ?? string.Empty;
			switch (stateAbbrCode.Trim().ToUpper())
			{
				case "AL": return "01";
				case "AZ": return "04";
				case "AR": return "05";
				case "CA": return "06";
				case "CO": return "07";
				case "CT": return "08";
				case "DE": return "10";
				case "GA": return "13";
				case "HI": return "15";
				case "ID": return "16";
				case "IN": return "18";
				case "KS": return "20";
				case "LA": return "22";
				case "ME": return "23";
				case "MD": return "24";
				case "MA": return "25";
				case "MI": return "26";
				case "MN": return "27";
				case "MS": return "28";
				case "MO": return "29";
				case "MT": return "30"; 
				case "NE": return "31";
				case "NJ": return "34";
				case "NM": return "35";
				case "NC": return "37";
				case "ND": return "38";
				case "OH": return "39";
				case "OK": return "40";
				case "SC": return "45";
				case "WI": return "55";
				default: return string.Empty;
			}
		}

		private string GetDirectSaleIndicator(PXGraph graph, int VendorID, string FinYear)
		{
			using (new PXReadBranchRestrictedScope(MarkedOrganizations, null, requireAccessForAllSpecified:true))
			{
				foreach (PXResult<AP1099History, AP1099Box> dataRec in PXSelectJoinGroupBy<AP1099History,
					InnerJoin<AP1099Box, On<AP1099Box.boxNbr, Equal<AP1099History.boxNbr>>>,
					Where<AP1099History.vendorID, Equal<Required<AP1099History.vendorID>>,
						And<AP1099History.boxNbr, Equal<Required<AP1099History.boxNbr>>,
						And<AP1099History.finYear, Equal<Required<AP1099History.finYear>>>>>,
					Aggregate<GroupBy<AP1099History.boxNbr, Sum<AP1099History.histAmt>>>>.Select(graph, VendorID, 9, FinYear))
				{
					return (((AP1099History)dataRec).HistAmt >= ((AP1099Box)dataRec).MinReportAmt) ? "1" : string.Empty;
				}
			}
			return string.Empty;
		}
	}
}