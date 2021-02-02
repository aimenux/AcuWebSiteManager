using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Data;
using System.Collections;
using PX.Objects.CS;
using System.Globalization;

namespace PX.Objects.TX
{
	[Obsolete(Common.Messages.ClassIsObsoleteRemoveInAcumatica2020R1)]
	public class TaxImport : PXGraph<TaxImport>
	{
		public PXCancel<TXImportState> Cancel;
        public PXProcessing<TXImportState> Items;

		public PXAction<TXImportState> viewData;
		[PXUIField(DisplayName = "View Data")]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.Inquiry)]
		public virtual void ViewData()
		{
			if (Items.Current != null)
			{
				TaxExplorer target = CreateInstance<TaxExplorer>();
				target.Clear();
				target.Filter.Current.State = Items.Current.StateCode;

				throw new PXRedirectRequiredException(target, "View Data");
			}
		}
		        
		public TaxImport()
        {
			Items.SetProcessDelegate<TaxImportProcess>(TaxImportProcess.Process);
        }
		
	}

	public class TaxImportProcess : PXGraph<TaxImportProcess>
	{
        #region Type Override events
        #region  TaxZoneDet
        #region TaxZoneID

        [PXDBString(10, IsUnicode = true, IsKey = true)]
        protected virtual void TaxZoneDet_TaxZoneID_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #region TaxID

        [PXDBString(Tax.taxID.Length, IsUnicode = true, IsKey = true)]
        [PXDefault()]
        protected virtual void TaxZoneDet_TaxID_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #region IsImported

        [PXDBBool()]
        [PXDefault(true)]
        protected virtual void TaxZoneDet_IsImported_CacheAttached(PXCache sender)
        {
        }
        #endregion       
        #endregion
        #endregion
        public PXSelect<Tax> Taxes;
		public PXSelect<TaxRev> Revisions;
		public PXSelect<TaxZone> TaxZones;
		public PXSelect<TaxZoneDet> TaxZoneDetails;
		public PXSelect<TaxCategoryDet> TaxCategoryDetails;
		public PXSelect<TaxZoneZip> TaxZoneZip;

		private const int SalesTaxBucketID = 1;

		public static void Process(TaxImportProcess graph, TXImportState item)
		{
			TaxBuilder.Result result = TaxBuilderEngine.Execute(graph, item.StateCode);

			List<Tax> taxes;
			List<KeyValuePair<Tax, List<TaxRev>>> revisions;
			List<TaxZone> zones;
			List<TaxZoneDet> zoneDetails;
			List<TaxCategoryDet> categoryDetails;
			List<TaxZoneZip> zoneZips;

			Translate(graph, item, result, out taxes, out revisions, out zones, out zoneDetails, out categoryDetails, out zoneZips);

			//Save to TX module:

			foreach (TaxZone zone in zones)
			{
				TaxZone existing = PXSelect<TaxZone, Where<TaxZone.taxZoneID, Equal<Required<TaxZone.taxZoneID>>>>.Select(graph, zone.TaxZoneID);

				if (existing == null)
				{
					graph.TaxZones.Insert(zone);
				}

				PXSelectBase<TaxZoneZip> selectZips = new PXSelect<TaxZoneZip, Where<TaxZoneZip.taxZoneID, Equal<Required<TaxZoneZip.taxZoneID>>>>(graph);
				foreach (TaxZoneZip z in selectZips.Select(zone.TaxZoneID))
				{
					graph.TaxZoneZip.Delete(z);
				}
			}

			foreach (TaxZoneZip zip in zoneZips)
			{
				graph.TaxZoneZip.Insert(zip);
			}

			foreach (Tax tax in taxes)
			{
				Tax existing = PXSelect<Tax, Where<Tax.taxID, Equal<Required<Tax.taxID>>>>.Select(graph, tax.TaxID);
				if (existing == null)
				{
					graph.Taxes.Insert(tax);
				}
			}

			foreach (KeyValuePair<Tax, List<TaxRev>> kv in revisions)
			{
				PXResultset<TaxRev> existingRevisions = PXSelect<TaxRev, Where<TaxRev.taxID, Equal<Required<TaxRev.taxID>>>>.Select(graph, kv.Key.TaxID);
							

				if (existingRevisions.Count == 0)
				{
					foreach (TaxRev revision in kv.Value)
						graph.Revisions.Insert(revision);
				}
				else
				{
					foreach (TaxRev revision in kv.Value)
					{
						bool skip = false;
						foreach (TaxRev existing in existingRevisions)
						{
							if ( graph.Revisions.Cache.ObjectsEqual<TaxRev.startDate, TaxRev.endDate, TaxRev.taxRate>(revision, existing))
							{
								skip = true;
								break;
							}
						}

						if ( !skip )
							graph.Revisions.Insert(revision);
					}
				}
			}

			foreach (TaxCategoryDet category in categoryDetails)
			{
				TaxCategoryDet existing = PXSelect<TaxCategoryDet, Where<TaxCategoryDet.taxID, Equal<Required<TaxCategoryDet.taxID>>,
					And<TaxCategoryDet.taxCategoryID, Equal<Required<TaxCategoryDet.taxCategoryID>>>>>.Select(graph, category.TaxID, category.TaxCategoryID);

				if ( existing == null )
					graph.TaxCategoryDetails.Insert(category);
			}
						
			foreach (TaxZoneDet zd in zoneDetails)
			{
				TaxZoneDet existing = PXSelect<TaxZoneDet, Where<TaxZoneDet.taxID, Equal<Required<TaxZoneDet.taxID>>,
					And<TaxZoneDet.taxZoneID, Equal<Required<TaxZoneDet.taxZoneID>>>>>.Select(graph, zd.TaxID, zd.TaxZoneID);

				if (existing == null)
					graph.TaxZoneDetails.Insert(zd);
			}

			graph.Actions.PressSave();
		}


		private static void Translate(TaxImportProcess graph, TXImportState item, TaxBuilder.Result result, out List<Tax> taxes,
			out List<KeyValuePair<Tax, List<TaxRev>>> revisions, out List<TaxZone> zones, out List<TaxZoneDet> zoneDetails, out List<TaxCategoryDet> categoryDetails, out List<TaxZoneZip> zoneZips)
		{
			taxes = new List<Tax>(result.Taxes.Count);
			revisions = new List<KeyValuePair<Tax,List<TaxRev>>>(result.Taxes.Count);
			zones = new List<TaxZone>(result.Zones.Count);
			zoneDetails = new List<TaxZoneDet>(result.ZoneDetails.Count);
			categoryDetails = new List<TaxCategoryDet>(result.Taxes.Count * 2);
			zoneZips = new List<TaxZoneZip>(result.ZoneZips.Count);

			TXImportSettings settings = PXSetup<TXImportSettings>.Select(graph);

			if (string.IsNullOrEmpty(settings.TaxableCategoryID))
			{
				throw new PXException(Messages.TaxableCategoryIDIsNotSet);
			}

			foreach (ZoneRecord zr in result.Zones)
			{
				TaxZone taxZone = new TaxZone();
				taxZone.TaxZoneID = zr.ZoneID;
				taxZone.Descr = zr.Description;

				if (zr.CombinedRate > 0)
					taxZone.DfltTaxCategoryID = settings.TaxableCategoryID;

				taxZone.IsImported = true;
				zones.Add(taxZone);
			}

			foreach (TaxRecord tr in result.Taxes)
			{
				Tax tax = new Tax();
				tax.TaxID = tr.TaxID;
				tax.TaxType = CSTaxType.Sales;
				tax.Descr = tr.Description;
				tax.IsImported = true;
				tax.SalesTaxAcctID = item.AccountID;
				tax.SalesTaxSubID = item.SubID;
				tax.TaxCalcType = CSTaxCalcType.Doc;
				tax.TaxCalcLevel = CSTaxCalcLevel.CalcOnItemAmt;
				taxes.Add(tax);

				revisions.Add( new KeyValuePair<Tax,List<TaxRev>>(tax, GetRevisions(tr)));
				categoryDetails.AddRange(GetCategories(graph, settings, tr));
			}

			foreach (ZoneDetailRecord zd in result.ZoneDetails)
			{
				TaxZoneDet zoneDetail = new TaxZoneDet();
				zoneDetail.TaxID = zd.TaxID;
				zoneDetail.TaxZoneID = zd.ZoneID;
				zoneDetail.IsImported = true;
				zoneDetails.Add(zoneDetail);
			}

			foreach (IList<ZoneZipPlusRecord> zr in result.ZoneZipPlus.Values)
			{
				foreach (ZoneZipPlusRecord zzp in zr)
				{
					TaxZoneZip tzzp = new TaxZoneZip();
					tzzp.TaxZoneID = zzp.ZoneID;
					tzzp.ZipCode = zzp.ZipCode;
					tzzp.ZipMin = zzp.ZipMin;
					tzzp.ZipMax = zzp.ZipMax;
					
					zoneZips.Add(tzzp);
				}
			}

		}


		private static List<TaxCategoryDet> GetCategories(TaxImportProcess graph, TXImportSettings settings, TaxRecord tr)
		{
			List<TaxCategoryDet> list = new List<TaxCategoryDet>();

			TaxCategory taxable = PXSelect<TaxCategory, Where<TaxCategory.taxCategoryID, Equal<Required<TaxCategory.taxCategoryID>>>>.Select(graph, settings.TaxableCategoryID);
			if (taxable != null)
			{
				bool insert = false;

				if (taxable.TaxCatFlag != true)
				{
					insert = tr.IsTaxable == true;
				}
				else
				{
					insert = tr.IsTaxable != true;
				}
				
				if (insert)
				{
					TaxCategoryDet tcd = new TaxCategoryDet();
					tcd.TaxID = tr.TaxID;
					tcd.TaxCategoryID = settings.TaxableCategoryID;
					tcd.IsImported = true;
					list.Add(tcd);
				}
			}

			TaxCategory freight = PXSelect<TaxCategory, Where<TaxCategory.taxCategoryID, Equal<Required<TaxCategory.taxCategoryID>>>>.Select(graph, settings.FreightCategoryID);
			if (freight != null)
			{
				bool insert = false;

				if (freight.TaxCatFlag != true)
				{
					insert = tr.IsFreight == true;
				}
				else
				{
					insert = tr.IsFreight != true;
				}

				if (insert)
				{
					TaxCategoryDet tcd = new TaxCategoryDet();
					tcd.TaxID = tr.TaxID;
					tcd.TaxCategoryID = settings.FreightCategoryID;
					tcd.IsImported = true;
					list.Add(tcd);
				}
			}

			TaxCategory service = PXSelect<TaxCategory, Where<TaxCategory.taxCategoryID, Equal<Required<TaxCategory.taxCategoryID>>>>.Select(graph, settings.ServiceCategoryID);
			if (service != null)
			{
				bool insert = false;

				if (service.TaxCatFlag != true)
				{
					insert = tr.IsService == true;
				}
				else
				{
					insert = tr.IsService != true;
				}

				if (insert)
				{
					TaxCategoryDet tcd = new TaxCategoryDet();
					tcd.TaxID = tr.TaxID;
					tcd.TaxCategoryID = settings.ServiceCategoryID;
					tcd.IsImported = true;
					list.Add(tcd);
				}
			}

			TaxCategory labor = PXSelect<TaxCategory, Where<TaxCategory.taxCategoryID, Equal<Required<TaxCategory.taxCategoryID>>>>.Select(graph, settings.LaborCategoryID);
			if (labor != null)
			{
				bool insert = false;

				if (labor.TaxCatFlag != true)
				{
					insert = tr.IsLabor == true;
				}
				else
				{
					insert = tr.IsLabor != true;
				}

				if (insert)
				{
					TaxCategoryDet tcd = new TaxCategoryDet();
					tcd.TaxID = tr.TaxID;
					tcd.TaxCategoryID = settings.LaborCategoryID;
					tcd.IsImported = true;
					list.Add(tcd);
				}
			}


			return list;
		}

		private static List<TaxRev> GetRevisions(TaxRecord tr)
		{
			List<TaxRev> list = new List<TaxRev>();

			//TODO:
			//split into 2 revisions if rateovermax is specified;
			//split into 2 revisions if previosRate is specified;
			int cx = 1;

			//outdated:
			if (tr.PreviousRate > 0 && tr.EffectiveDate != null)
			{
				TaxRev rev1 = new TaxRev();
				rev1.TaxID = tr.TaxID;
				rev1.RevisionID = cx++;
				rev1.IsImported = true;
				rev1.Outdated = true;
				rev1.TaxBucketID = SalesTaxBucketID;
				rev1.StartDate = DateTime.Parse(TaxRev.DefaultStartDate, CultureInfo.InvariantCulture);
				rev1.EndDate = tr.EffectiveDate ?? DateTime.Parse(TaxRev.DefaultEndDate, CultureInfo.InvariantCulture);
				rev1.TaxType = CSTaxType.Sales;
				rev1.TaxRate = tr.Rate * 100;
				list.Add(rev1);
			}

			//current:
			if (tr.TaxableMax != null && tr.TaxableMax > 0)
			{
				//split into 2:
				TaxRev rev1 = new TaxRev();
				rev1.TaxID = tr.TaxID;
				rev1.RevisionID = cx++;
				rev1.IsImported = true;
				rev1.Outdated = false;
				rev1.TaxBucketID = SalesTaxBucketID;
				rev1.StartDate = tr.EffectiveDate ?? DateTime.Parse(TaxRev.DefaultStartDate, CultureInfo.InvariantCulture);
				rev1.EndDate = tr.EffectiveDate ?? DateTime.Parse(TaxRev.DefaultEndDate, CultureInfo.InvariantCulture);
				rev1.TaxType = CSTaxType.Sales;
				rev1.TaxRate = tr.Rate * 100;
				rev1.TaxableMax = tr.TaxableMax;
				list.Add(rev1);

				TaxRev rev2 = new TaxRev();
				rev2.TaxID = tr.TaxID;
				rev2.RevisionID = cx++;
				rev2.IsImported = true;
				rev2.Outdated = false;
				rev2.TaxBucketID = SalesTaxBucketID;
				rev2.StartDate = tr.EffectiveDate ?? DateTime.Parse(TaxRev.DefaultStartDate, CultureInfo.InvariantCulture);
				rev2.EndDate = tr.EffectiveDate ?? DateTime.Parse(TaxRev.DefaultEndDate, CultureInfo.InvariantCulture);
				rev2.TaxType = CSTaxType.Sales;
				rev2.TaxRate = tr.RateOverMax * 100;
				rev2.TaxableMin = tr.TaxableMax;
				list.Add(rev2);
			}
			else
			{
				TaxRev rev1 = new TaxRev();
				rev1.TaxID = tr.TaxID;
				rev1.RevisionID = cx++;
				rev1.IsImported = true;
				rev1.Outdated = false;
				rev1.TaxBucketID = SalesTaxBucketID;
				rev1.StartDate = tr.EffectiveDate ?? DateTime.Parse(TaxRev.DefaultStartDate, CultureInfo.InvariantCulture);
				rev1.EndDate = DateTime.Parse(TaxRev.DefaultEndDate, CultureInfo.InvariantCulture);
				rev1.TaxType = CSTaxType.Sales;
				rev1.TaxRate = tr.Rate * 100;
				list.Add(rev1);
			}
			return list;
		}		
	}
}
