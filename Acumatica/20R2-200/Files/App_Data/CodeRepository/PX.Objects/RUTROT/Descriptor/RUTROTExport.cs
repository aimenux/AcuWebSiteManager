using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using PX.Data;
using PX.Objects.AR;

namespace PX.Objects.RUTROT
{
	public class RUTExport
	{
		public RUTExport(PXGraph graph, Dictionary<int, RUTROTWorkType> availableWorkTypes)
		{
			this.availableWorkTypes = availableWorkTypes;

			RUTROTSetup preferences = PXSelect<RUTROTSetup>.Select(graph);

			NsMain = preferences.NsMain;
			NsHtko = preferences.NsHtko;
			NsXsi = preferences.NsXsi;
			SchemaLocation = preferences.SchemaLocation;
		}
		#region Elements
		protected const string PrefixHtko = "htko";

		protected const string Root = "HtAnsokan";
		protected const string CompanyID = "NamnPaBegaran";
		protected const string ClaimsSection = "HushallBegaran";
		protected const string Claim = "Arenden";
		protected const string Buyer = "Kopare";

		protected const string PayDate = "BetalningsDatum";
		protected const string PaidAmt = "BetaltBelopp";
		protected const string ClaimedAmt = "BegartBelopp";
		protected const string InvoiceNbr = "FakturaNr";

		protected const string WorkPrice = "PrisForArbete";
		protected const string OtherCost = "Ovrigkostnad";
		protected const string MaterialCost = "Materialkostnad";
		protected const string HoursAmt = "AntalTimmar";

		protected const string DoneWork = "UtfortArbete";

		protected readonly string NsMain;
		protected readonly string NsHtko;
		protected readonly string NsXsi;
		protected readonly string SchemaLocation;

		protected Dictionary<int, RUTROTWorkType> availableWorkTypes;

		#endregion

		protected XmlWriter Writer
		{
			get;
			set;
		}

		protected void WriteStartHtko(string element)
		{
			Writer.WriteStartElement(PrefixHtko, element, NsHtko);
		}

		protected void WriteHtko(string element, string value)
		{
			if (!string.IsNullOrEmpty(value))
			{
				Writer.WriteElementString(PrefixHtko, element, NsHtko, value);
			}
		}

		protected void WriteRoot()
		{
			Writer.WriteStartElement("n1", "Begaran", NsMain);
			Writer.WriteAttributeString("xmlns", "xsi", null, NsXsi);
			Writer.WriteAttributeString("xmlns", "n1", null, NsMain);
			Writer.WriteAttributeString("xmlns", PrefixHtko, null, NsHtko);
			Writer.WriteAttributeString("xsi", "schemaLocation", null, SchemaLocation);
		}

		protected virtual string ClaimsSectionElement { get { return ClaimsSection; } }

		protected void WriteClaimsStart(string claimId)
		{
			WriteHtko(CompanyID, claimId);
			WriteStartHtko(ClaimsSectionElement);
		}

		protected void WriteEnd()
		{
			Writer.WriteEndElement();
			Writer.WriteEndElement();
		}

		protected string AmountString(decimal value)
		{
			return value.ToString("0.");
		}

		protected virtual void WriteSpecialPersonClaimInfo(RUTExportItem item)
		{
		}

		protected void WriteWorkType(string workType, int hoursAmt, decimal materialCost)
		{
			WriteStartHtko(workType);
			WriteHtko(HoursAmt, hoursAmt.ToString());
			WriteHtko(MaterialCost, AmountString(materialCost));
			Writer.WriteEndElement();
		}

		protected void WritePersonClaim(RUTExportItem item)
		{
			WriteStartHtko(Claim);

			WriteHtko(Buyer, item.PersonID);
			WriteHtko(PayDate, item.PayDate.ToString("yyyy-MM-dd"));
			//PrisForArbete
			WriteHtko(WorkPrice, AmountString(item.WorkPrice));
			WriteHtko(PaidAmt, AmountString(item.PaidAmt));
			WriteHtko(ClaimedAmt, AmountString(item.ClaimedAmt));
			if (!string.IsNullOrWhiteSpace(item.InvoiceNbr))
			{
				WriteHtko(InvoiceNbr, item.InvoiceNbr);
			}
			//Ovrigkostnad
			WriteHtko(OtherCost, AmountString(item.OtherCost));
			WriteSpecialPersonClaimInfo(item);
			//UtfortArbete
			WriteDoneWork(item);

			Writer.WriteEndElement();
		}

		protected virtual void WriteDoneWork(RUTExportItem item)
		{
			WriteStartHtko(DoneWork);
			foreach (RUTROTWorkType workType in availableWorkTypes.Values)
			{
				if (workType.RUTROTType == RUTROTTypes.RUT)
				{
					WriteWorkType(workType.XMLTag, item.HoursAmt[workType.WorkTypeID ?? 0], item.MaterialCost[workType.WorkTypeID ?? 0]);
				}
			}
			Writer.WriteEndElement();
		}

		public byte[] Export(IEnumerable<ClaimRUTROTProcess.DocumentDetails> documents)
		{
			using (var stream = new MemoryStream())
			{
				using (Writer = XmlWriter.Create(stream, new XmlWriterSettings { Indent = true }))
				{
					WriteRoot();

					var claims = RUTExportItem.Create(documents, availableWorkTypes);

					WriteClaimsStart(documents.First().ClaimNbr.ToString());

					foreach (var c in claims)
					{
						WritePersonClaim(c);
					}

					WriteEnd();

					Writer.Flush();
					return stream.ToArray();
				}
			}
		}
	}

	public class ROTExport : RUTExport
	{
		public ROTExport(PXGraph graph, Dictionary<int, RUTROTWorkType> availableWorkTypes) : base(graph, availableWorkTypes)
		{ }
		#region Elements
		protected const string ROTClaimSection = "RotBegaran";
		protected const string ApartmentID = "LagenhetsNr";
		protected const string EstateID = "Fastighetsbeteckning";
		protected const string OrganizationNbr = "BrfOrgNr";

		#endregion

		protected const string _OrgNbrPrefix = "16";

		protected virtual string OrgNbrPrefix
		{
			get { return _OrgNbrPrefix; }
		}

		protected override string ClaimsSectionElement
		{
			get { return ROTClaimSection; }
		}

		protected override void WriteSpecialPersonClaimInfo(RUTExportItem item)
		{
			WriteHtko(EstateID, item.ROTEstate);
			WriteHtko(ApartmentID, item.ROTAppartment);
			WriteHtko(OrganizationNbr, OrgNbrPrefix + item.ROTOrganizationNbr);
		}

		protected override void WriteDoneWork(RUTExportItem item)
		{
			WriteStartHtko(DoneWork);
			foreach (RUTROTWorkType workType in availableWorkTypes.Values)
			{
				if (workType.RUTROTType == RUTROTTypes.ROT)
				{
					WriteWorkType(workType.XMLTag, item.HoursAmt[workType.WorkTypeID], item.MaterialCost[workType.WorkTypeID]);
				}
			}
			Writer.WriteEndElement();
		}

		protected bool IsForProperty(ARInvoice invoice, RUTROT rutrot)
		{
			return rutrot.ROTOrganizationNbr == null;
		}
	}

	public class RUTExportItem
	{
		public string PersonID
		{
			get;
			set;
		}

		public decimal PaidAmt
		{
			get;
			set;
		}

		public decimal ClaimedAmt
		{
			get;
			set;
		}

		public DateTime PayDate
		{
			get;
			set;
		}

		public string InvoiceNbr
		{
			get;
			set;
		}

		public decimal WorkPrice
		{
			get;
			set;
		}

		public decimal OtherCost
		{
			get;
			set;
		}

		public string ROTEstate
		{
			get;
			set;
		}

		public string ROTAppartment
		{
			get;
			set;
		}

		public string ROTOrganizationNbr
		{
			get;
			set;
		}

		public Dictionary<int?, int> HoursAmt
		{
			get;
			set;
		}

		public Dictionary<int?, decimal> MaterialCost
		{
			get;
			set;
		}

		public static IEnumerable<RUTExportItem> Create(ClaimRUTROTProcess.DocumentDetails document, Dictionary<int, RUTROTWorkType> availableWorkTypes)
		{
			var items = new List<RUTExportItem>();

			RUTROT rowRR = document.Rutrot;
			DateTime payDate = document.Payments.Select(p => p.Item2.AdjgDocDate).Where(d => d != null).Max(d => d.Value);
			string invNbr = document.Document.RefNbr;
			decimal otherCost = rowRR.CuryOtherCost ?? 0m;
			decimal workPrice = rowRR.CuryWorkPrice ?? 0m;
			Dictionary<int?, decimal> materialCost = new Dictionary<int?, decimal>();
			Dictionary<int?, decimal> materialCostSumms = new Dictionary<int?, decimal>();
			Dictionary<int?, int> hoursAmt = new Dictionary<int?, int>();
			Dictionary<int?, int> hourSumms = new Dictionary<int?, int>();
			foreach (RUTROTWorkType workType in availableWorkTypes.Values)
			{
				materialCost.Add(workType.WorkTypeID, 0m);
				materialCostSumms.Add(workType.WorkTypeID, 0m);
				hoursAmt.Add(workType.WorkTypeID, 0);
				hourSumms.Add(workType.WorkTypeID, 0);
			}
			decimal claimedSumm = 0m;
			decimal paidSumm = 0m;
			decimal workPriceSumm = 0m;
			decimal otherCostSumm = 0m;
			decimal paidUnrounded = 0m;

			foreach (ARTran tran in document.Lines)
			{
				ARTranRUTROT tranRR = PXCache<ARTran>.GetExtension<ARTranRUTROT>(tran);
				try
				{
					if (tranRR.RUTROTItemType == RUTROTItemTypes.MaterialCost)
					{
						materialCost[tranRR.RUTROTWorkTypeID] += tranRR.CuryRUTROTTotal ?? 0m;
					}
					else if (tranRR.RUTROTItemType == RUTROTItemTypes.Service)
					{
						hoursAmt[tranRR.RUTROTWorkTypeID] += (int)(tran.Qty ?? 0);
					}
				}
				catch
				{
					throw new PXException(RUTROTMessages.CannotClaimWorkType);
				}
			}
			foreach (var dp in document.Distribution.Zip(document.PaymentDistribution, (d, p) => new { Distribution = d, Payment = p }))
			{
				decimal percent = (dp.Distribution.CuryAmount / rowRR.CuryTotalAmt) ?? 1m;
				RUTExportItem item = new RUTExportItem()
				{
					InvoiceNbr = invNbr,
					OtherCost = Math.Floor(otherCost * percent),
					WorkPrice = Math.Floor(workPrice * percent),
					MaterialCost = new Dictionary<int?, decimal>(),
					HoursAmt = new Dictionary<int?, int>(),
					PaidAmt = Math.Floor(dp.Payment),
					PayDate = payDate,
					PersonID = dp.Distribution.PersonalID,
					ClaimedAmt = Math.Floor(dp.Distribution.CuryAmount ?? 0.0m),
					ROTEstate = rowRR.ROTEstate,
					ROTAppartment = rowRR.ROTAppartment,
					ROTOrganizationNbr = rowRR.ROTOrganizationNbr
				};
				foreach (RUTROTWorkType workType in availableWorkTypes.Values)
				{
					item.MaterialCost.Add(workType.WorkTypeID, 0m);
					item.HoursAmt.Add(workType.WorkTypeID, 0);
				}
				foreach (RUTROTWorkType workType in availableWorkTypes.Values)
				{
					decimal hoursAmtCurrent = hoursAmt[workType.WorkTypeID] * percent;
					item.HoursAmt[workType.WorkTypeID] = hoursAmtCurrent < 1 && hoursAmtCurrent > 0 ? 1 : (int)Math.Round(hoursAmtCurrent);
					item.MaterialCost[workType.WorkTypeID] = Math.Floor(materialCost[workType.WorkTypeID] * percent);
					hourSumms[workType.WorkTypeID] += item.HoursAmt[workType.WorkTypeID];
					materialCostSumms[workType.WorkTypeID] += item.MaterialCost[workType.WorkTypeID];
				}
				claimedSumm += item.ClaimedAmt;
				paidSumm += item.PaidAmt;
				workPriceSumm += item.WorkPrice;
				otherCostSumm += item.OtherCost;
				paidUnrounded += dp.Payment;
				items.Add(item);
			}
			if (items.Count > 0)
			{
				decimal claimLeft = Math.Floor(rowRR.CuryTotalAmt ?? 0m) - claimedSumm;
				for (int i = 0; i < items.Count; i++)
				{
					if (Math.Round(claimLeft) == 0m)
					{
						break;
					}
					if (items[i].ClaimedAmt != 0)
					{
						items[i].ClaimedAmt += Math.Sign(claimLeft);
						claimLeft -= Math.Sign(claimLeft);
					}
				}
				decimal paidLeft = Math.Floor(paidUnrounded) - paidSumm;
				for (int i = 0; i < items.Count; i++)
				{
					if (Math.Round(paidLeft) == 0m)
					{
						break;
					}
					if (items[i].PaidAmt != 0)
					{
						items[i].PaidAmt += Math.Sign(paidLeft);
						paidLeft -= Math.Sign(paidLeft);
					}
				}
				decimal workPriceLeft = Math.Floor(workPrice) - workPriceSumm;
				for (int i = 0; i < items.Count; i++)
				{
					if (Math.Round(workPriceLeft) == 0m)
					{
						break;
					}
					if (items[i].WorkPrice != 0)
					{
						items[i].WorkPrice += Math.Sign(workPriceLeft);
						workPriceLeft -= Math.Sign(workPriceLeft);
					}
				}
				decimal otherCostLeft = Math.Floor(otherCost) - otherCostSumm;
				for (int i = 0; i < items.Count; i++)
				{
					if (Math.Round(otherCostLeft) == 0m)
					{
						break;
					}
					if (items[i].OtherCost != 0)
					{
						items[i].OtherCost += Math.Sign(otherCostLeft);
						otherCostLeft -= Math.Sign(otherCostLeft);
					}
				}
				foreach (RUTROTWorkType workType in availableWorkTypes.Values)
				{
					int hoursLeft = hoursAmt[workType.WorkTypeID] - hourSumms[workType.WorkTypeID];
					for (int j = 0; j < items.Count; j++)
					{
						if (hoursLeft == 0)
							break;
						if (items[j].HoursAmt[workType.WorkTypeID] != 0)
						{
							if (items[j].HoursAmt[workType.WorkTypeID] + Math.Sign(hoursLeft) >= 1)
							{
								items[j].HoursAmt[workType.WorkTypeID] += Math.Sign(hoursLeft);
								hoursLeft -= Math.Sign(hoursLeft);
							}
						}
					}
				}
				foreach (RUTROTWorkType workType in availableWorkTypes.Values)
				{
					decimal materialCostLeft = materialCost[workType.WorkTypeID] - materialCostSumms[workType.WorkTypeID];
					for (int j = 0; j < items.Count; j++)
					{
						if (materialCostLeft == 0)
							break;
						if (items[j].MaterialCost[workType.WorkTypeID] != 0)
						{
							if (items[j].MaterialCost[workType.WorkTypeID] + Math.Sign(materialCostLeft) >= 1)
							{
								items[j].MaterialCost[workType.WorkTypeID] += Math.Sign(materialCostLeft);
								materialCostLeft -= Math.Sign(materialCostLeft);
							}
						}
					}
				}
			}
			return items;
		}

		public static IEnumerable<RUTExportItem> Create(IEnumerable<ClaimRUTROTProcess.DocumentDetails> documents, Dictionary<int, RUTROTWorkType> availableWorkTypes)
		{
			return documents.SelectMany(d => Create(d, availableWorkTypes)).ToList().OrderBy(item => item.PersonID);
		}
	}
}
