using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Data;
using System.Diagnostics;
using PX.Objects.CS;
using System.Text.RegularExpressions;

namespace PX.Objects.TX
{
	public class TaxBuilderEngine
	{
		public static TaxBuilder.Result Execute(PXGraph graph, string stateCode)
		{
			PXSelectBase<TXImportFileData> select = new PXSelect<TXImportFileData, Where<TXImportFileData.stateCode, Equal<Required<TXImportFileData.stateCode>>>>(graph);
			PXSelectBase<TXImportZipFileData> zipSelect = new PXSelect<TXImportZipFileData,
				Where<TXImportZipFileData.stateCode, Equal<Required<TXImportZipFileData.stateCode>>,
				And<Where2<Where<TXImportZipFileData.plus4PortionOfZipCode, NotEqual<ZipMin>>,
				Or<TXImportZipFileData.plus4PortionOfZipCode2, NotEqual<ZipMax>>>>>>(graph);

			PXResultset<TXImportFileData> resultSet = select.Select(stateCode);
			PXResultset<TXImportZipFileData> resultSetZip = zipSelect.Select(stateCode);
			List<TXImportFileData> data = new List<TXImportFileData>(resultSet.Count);
			foreach (TXImportFileData row in resultSet)
			{
				data.Add(row);
			}

			List<TXImportZipFileData> zipData = new List<TXImportZipFileData>(resultSetZip.Count);
			foreach (TXImportZipFileData row in resultSetZip)
			{
				zipData.Add(row);
			}

			TaxBuilder builder = null;

			switch (stateCode)
			{
				default:
					builder = new GenericTaxBuilder(stateCode, data, zipData);
					break;
			}

			if (builder != null)
			{
				return builder.Execute();
			}
			else
			{
				throw new NotImplementedException();
			}
		}

		public class ZipMin : PX.Data.BQL.BqlInt.Constant<ZipMin>
		{
			public ZipMin() : base(1) { }
		}

		public class ZipMax : PX.Data.BQL.BqlInt.Constant<ZipMax>
		{
			public ZipMax() : base(9999) { }
		}
		
		public static string GetTaxZoneByZip(PXGraph graph, string zipCode)
		{
			if (graph == null)
				throw new ArgumentNullException("graph");

			if (string.IsNullOrEmpty(zipCode))
				throw new ArgumentNullException("zipCode");

			PXResultset<TaxZoneZip> resultset = null;

			bool isZipPlusFour = false;
			if (zipCode.Length == 10)
			{
				Regex zipPlusFourDigits = new Regex(@"^\w{5}-\d{4}\b"); //five letters or digits dash four digits with total length of 10 symbols
				isZipPlusFour = zipPlusFourDigits.IsMatch(zipCode);
			}

			if (isZipPlusFour)
			{
				string zip = zipCode.Substring(0, 5);
				string zipPlus = zipCode.Substring(6);

				int plus;

				if (int.TryParse(zipPlus, out plus))
				{
					resultset = PXSelect<TaxZoneZip, Where<TaxZoneZip.zipCode, Equal<Required<TaxZoneZip.zipCode>>,
					And<Required<TaxZoneZip.zipCode>, Between<TaxZoneZip.zipMin, TaxZoneZip.zipMax>>>>.Select(graph, zip, plus);
				}
				else
				{
					PXTrace.WriteWarning("Failed to extract a valid zip+4 code (XXXXX-XXXX) from the following: {0}", zipCode);
					return null;
				}
			}
			else
			{
				resultset = PXSelect<TaxZoneZip, Where<TaxZoneZip.zipCode, Equal<Required<TaxZoneZip.zipCode>>>>.Select(graph, zipCode);
			}


			if (resultset.Count == 0)
			{
				PXTrace.WriteWarning("Failed to find record in TaxZoneZip for the given zip code: {0}", zipCode);
				return null;
			}
			else if (resultset.Count > 1)
			{
				PXTrace.WriteWarning("{0} records returned from TaxZoneZip for the given zip code: {1}. Only first will be used.", resultset.Count, zipCode);

				return ((TaxZoneZip)resultset).TaxZoneID;
			}
			else
			{
				return ((TaxZoneZip)resultset).TaxZoneID;
			}

		}

	}

	public abstract class TaxBuilder
	{
		protected List<TXImportFileData> data;
		protected List<TXImportZipFileData> zipData;

		protected List<TaxRecord> taxes;
		protected List<ZoneRecord> zones;
		protected List<ZoneDetailRecord> zoneDetails;
		protected List<ZoneZipRecord> zoneZips;
		protected Dictionary<ZoneRecord, IList<ZoneZipPlusRecord>> zoneZipPlus;

		public TaxBuilder(List<TXImportFileData> data, List<TXImportZipFileData> zipData)
		{
			if (data == null)
				throw new ArgumentNullException("data");
			if (zipData == null)
				throw new ArgumentNullException("zipData");

			this.data = data;
			this.zipData = zipData;
		}

		public virtual Result Execute()
		{
			Stopwatch sw = new Stopwatch();
			sw.Start();
			ExecuteBuilder();
			sw.Stop();

			Debug.Print("{0} ExecuteAlgorithm in {1} millisec.", this, sw.ElapsedMilliseconds);

			return new Result(this.taxes, this.zones, this.zoneDetails, this.zoneZips, this.zoneZipPlus);
		}

		protected abstract void ExecuteBuilder();

		public static bool HasTax(TXImportFileData record)
		{
			return record.CombinedSalesTaxRate > 0;
		}

		public static bool IsOnlyState(TXImportFileData record)
		{
			return record.StateSalesTaxRate == record.CombinedSalesTaxRate;
		}

		public static bool IsOnlyStateAndCounty(TXImportFileData record)
		{
			return (record.StateSalesTaxRate + record.CountySalesTaxRate) == record.CombinedSalesTaxRate;
		}

		public static bool ContainsCityTax(TXImportFileData record)
		{
			if ((TotalCounty(record) + record.StateSalesTaxRate) == record.CombinedSalesTaxRate)
				return false;
			else
				return true;
		}

		public static bool ContainsDistrictTax(TXImportFileData record)
		{
			if ( TotalDistrict(record) > 0)
				return true;
			else
				return false;
		}


		public static decimal TotalCounty(TXImportFileData record)
		{
			decimal result = record.CountySalesTaxRate.Value;
			return result + TotalDistrict(record);
		}

		public static decimal TotalDistrict(TXImportFileData record)
		{
			decimal result = 0;

			if (record.TransitTaxIsCity == "C")
			{
				result += record.TransitSalesTaxRate.Value;
			}
			if (record.Other1TaxIsCity == "C")
			{
				result += record.Other1SalesTaxRate.Value;
			}
			if (record.Other2TaxIsCity == "C")
			{
				result += record.Other2SalesTaxRate.Value;
			}
			if (record.Other3TaxIsCity == "C")
			{
				result += record.Other3SalesTaxRate.Value;
			}
			if (record.Other4TaxIsCity == "C")
			{
				result += record.Other4SalesTaxRate.Value;
			}

			return result;
		}

		public static decimal TotalCity(TXImportFileData record)
		{
			decimal result = record.CitySalesTaxRate.Value;

			if (record.TransitTaxIsCity == "T")
			{
				result += record.TransitSalesTaxRate.Value;
			}
			if (record.Other1TaxIsCity == "T")
			{
				result += record.Other1SalesTaxRate.Value;
			}
			if (record.Other2TaxIsCity == "T")
			{
				result += record.Other2SalesTaxRate.Value;
			}
			if (record.Other3TaxIsCity == "T")
			{
				result += record.Other3SalesTaxRate.Value;
			}
			if (record.Other4TaxIsCity == "T")
			{
				result += record.Other4SalesTaxRate.Value;
			}

			return result;
		}

		protected virtual ZoneDetailRecord AppendZoneDetail(ZoneRecord zone, TaxRecord tax)
		{
			ZoneDetailRecord zd = new ZoneDetailRecord();
			zd.ZoneID = zone.ZoneID;
			zd.TaxID = tax.TaxID;

			zoneDetails.Add(zd);

			return zd;
		}

		protected virtual ZoneRecord AppendZone(string zoneID, string description)
		{
			ZoneRecord zr = new ZoneRecord();
			zr.ZoneID = zoneID;
			zr.Description = description;

			zones.Add(zr);

			return zr;
		}

		protected virtual TaxRecord AppendTax(string taxID, string description)
		{
			TaxRecord tr = new TaxRecord();
			tr.TaxID = taxID;
			tr.Description = description;


			taxes.Add(tr);

			return tr;
		}

		protected virtual TaxRecord AppendTax(string taxID, string description, decimal? rate, DateTime? effectiveDate, decimal? previousRate)
		{
			TaxRecord tr = new TaxRecord();
			tr.TaxID = taxID;
			tr.Description = description;
			tr.Rate = rate;
			tr.EffectiveDate = effectiveDate;
			tr.PreviousRate = previousRate;

			taxes.Add(tr);

			return tr;
		}

		protected virtual void SetFlags(TaxRecord tax, TXImportFileData record)
		{
			tax.IsTaxable = record.CombinedSalesTaxRate > 0;
			tax.IsFreight = record.TaxFreight == "Y";
			tax.IsService = record.TaxServices == "Y";
			tax.IsLabor = record.TaxServices == "Y" || record.TaxServices == "S";
		}

		protected virtual void AppendZip(ZoneRecord zone, TXImportFileData t)
		{
			var zips = from z in zipData
					   where z.ZipCode == t.ZipCode && z.CountyName == t.CountyName
					   select z;

			bool added = false;
			foreach (TXImportZipFileData zz in zips)
			{
				ZoneZipPlusRecord pr = new ZoneZipPlusRecord();
				pr.ZoneID = zone.ZoneID;
				pr.ZipCode = t.ZipCode;
				pr.ZipMin = zz.Plus4PortionOfZipCode;
				pr.ZipMax = zz.Plus4PortionOfZipCode2;

				if (zoneZipPlus.ContainsKey(zone))
				{
					zoneZipPlus[zone].Add(pr);
				}
				else
				{
					List<ZoneZipPlusRecord> list = new List<ZoneZipPlusRecord>();
					list.Add(pr);
					zoneZipPlus.Add(zone, list);
				}

				added = true;
			}

			if (!added)
			{
				ZoneZipPlusRecord pr = new ZoneZipPlusRecord();
				pr.ZoneID = zone.ZoneID;
				pr.ZipCode = t.ZipCode;
				pr.ZipMin = 1;
				pr.ZipMax = 9999;

				if (zoneZipPlus.ContainsKey(zone))
				{
					zoneZipPlus[zone].Add(pr);
				}
				else
				{
					List<ZoneZipPlusRecord> list = new List<ZoneZipPlusRecord>();
					list.Add(pr);
					zoneZipPlus.Add(zone, list);
				}
			}
		}


		public class Result
		{
			public IList<TaxRecord> Taxes { get; private set; }
			public IList<ZoneRecord> Zones { get; private set; }
			public IList<ZoneDetailRecord> ZoneDetails { get; private set; }
			public IList<ZoneZipRecord> ZoneZips { get; private set; }
			public IDictionary<ZoneRecord, IList<ZoneZipPlusRecord>> ZoneZipPlus { get; private set; }

			public Result(IList<TaxRecord> taxes, IList<ZoneRecord> zones, IList<ZoneDetailRecord> zoneDetails, IList<ZoneZipRecord> zoneZips, IDictionary<ZoneRecord, IList<ZoneZipPlusRecord>> zoneZipPlus)
			{
				this.Taxes = taxes;
				this.Zones = zones;
				this.ZoneDetails = zoneDetails;
				this.ZoneZips = zoneZips;
				this.ZoneZipPlus = zoneZipPlus;
			}
		}
	}

	public class GenericTaxBuilder : TaxBuilder
	{
		protected string State { get; set; }

		public GenericTaxBuilder(string state, List<TXImportFileData> data, List<TXImportZipFileData> zipData)
			: base(data, zipData)
		{
			this.State = state;
			this.taxes = new List<TaxRecord>(200);
			this.zones = new List<ZoneRecord>(200);
			this.zoneDetails = new List<ZoneDetailRecord>(500);
			this.zoneZips = new List<ZoneZipRecord>(5000);
			this.zoneZipPlus = new Dictionary<ZoneRecord, IList<ZoneZipPlusRecord>>(500);
		}

		protected override void ExecuteBuilder()
		{
			TaxRecord stateTax = null;
			TaxRecord zeroTax = null;
			ZoneRecord zeroZone = null;
			Dictionary<string, ZoneRecord> zoneDict = new Dictionary<string, ZoneRecord>();

			Dictionary<string, TaxRecord> districtTax = new Dictionary<string, TaxRecord>();
			Dictionary<string, TaxRecord> districtTransitTax = new Dictionary<string, TaxRecord>();
			Dictionary<string, TaxRecord> districtOther1Tax = new Dictionary<string, TaxRecord>();
			Dictionary<string, TaxRecord> districtOther2Tax = new Dictionary<string, TaxRecord>();
			Dictionary<string, TaxRecord> districtOther3Tax = new Dictionary<string, TaxRecord>();
			Dictionary<string, TaxRecord> districtOther4Tax = new Dictionary<string, TaxRecord>();
			Dictionary<string, TaxRecord> cityTax = new Dictionary<string, TaxRecord>();
			Dictionary<string, TaxRecord> cityTransitTax = new Dictionary<string, TaxRecord>();
			Dictionary<string, TaxRecord> cityOther1Tax = new Dictionary<string, TaxRecord>();
			Dictionary<string, TaxRecord> cityOther2Tax = new Dictionary<string, TaxRecord>();
			Dictionary<string, TaxRecord> cityOther3Tax = new Dictionary<string, TaxRecord>();
			Dictionary<string, TaxRecord> cityOther4Tax = new Dictionary<string, TaxRecord>();
			

			foreach (TXImportFileData t in data)
			{
				string zoneKey = State;
				List<TaxRecord> list = new List<TaxRecord>();

				if (!HasTax(t))
				{
					if (zeroTax == null)
					{
						zeroTax = AppendTax(State + "ZERO", "NO Tax", t.CombinedSalesTaxRate, t.CombinedSalesTaxRateEffectiveDate, t.CombinedSalesTaxPreviousRate);
						SetFlags(zeroTax, t);

						zeroZone = new ZoneRecord();
						zeroZone.ZoneID = "NO Tax";
						zeroZone.Description = "No Tax";
						zeroZone.CombinedRate = t.CombinedSalesTaxRate;
						zones.Add(zeroZone);

						AppendZoneDetail(zeroZone, zeroTax);
					}

					AppendZip(zeroZone, t);
				}
				else
				{
					if (stateTax == null)
					{
						stateTax = AppendTax(State + "STATE", State + " State Tax");
						stateTax.Rate = t.StateSalesTaxRate;
						stateTax.PreviousRate = t.StateSalesTaxPreviousRate;
						stateTax.EffectiveDate = t.StateSalesTaxRateEffectiveDate;
						SetFlags(stateTax, t);
					}

					if (t.StateSalesTaxRate > 0)
					{
						list.Add(stateTax);
					}

					#region County
					if (t.CountySalesTaxRate > 0 || t.CountySalesTaxPreviousRate > 0)
					{
						if ( !string.IsNullOrEmpty( t.SignatureCodeCounty ))
							zoneKey = t.SignatureCodeCounty;

						if (!districtTax.ContainsKey(t.SignatureCodeCounty))
						{
							string taxID = t.StateCode + t.SignatureCodeCounty;
							TaxRecord info = AppendTax(taxID, "County Tax " + t.CountyName);
							info.Rate = t.CountySalesTaxRate;
							info.PreviousRate = t.CountySalesTaxPreviousRate;
							info.EffectiveDate = t.CountySalesTaxRateEffectiveDate;
							SetFlags(info, t);
							districtTax.Add(t.SignatureCodeCounty, info);
							list.Add(info);

							info.CountyCode = t.SignatureCodeCounty;
							info.CountyName = t.CountyName;
						}
						else
						{
							list.Add(districtTax[t.SignatureCodeCounty]);
						}
					}

					if (t.TransitTaxIsCity == "C" && (t.TransitSalesTaxRate > 0 || t.TransitSalesTaxPreviousRate > 0))
					{
						if (!string.IsNullOrEmpty(t.SignatureCodeTransit))
							zoneKey = t.SignatureCodeTransit;

						if (!districtTransitTax.ContainsKey(t.SignatureCodeTransit))
						{
							string taxID = t.StateCode + "CT" + t.SignatureCodeTransit;
							TaxRecord info = AppendTax(taxID, "Transit Tax " + t.CityName);
							info.Rate = t.TransitSalesTaxRate;
							info.PreviousRate = t.TransitSalesTaxPreviousRate;
							info.EffectiveDate = t.TransitSalesTaxRateEffectiveDate;
							SetFlags(info, t);
							districtTransitTax.Add(t.SignatureCodeTransit, info);
							list.Add(info);

							info.CountyCode = t.SignatureCodeTransit;
							info.CountyName = t.CountyName;
						}
						else
						{
							list.Add(districtTransitTax[t.SignatureCodeTransit]);
						}
					}
					if (t.Other1TaxIsCity == "C" && (t.Other1SalesTaxRate > 0 || t.Other1SalesTaxPreviousRate > 0))
					{
						if (!string.IsNullOrEmpty(t.SignatureCodeOther1))
							zoneKey = t.SignatureCodeOther1;

						if (!districtOther1Tax.ContainsKey(t.SignatureCodeOther1))
						{
							string taxID = t.StateCode + "CO1" + t.SignatureCodeOther1;
							TaxRecord info = AppendTax(taxID, "County Other 1 Tax " + t.CountyName);
							info.Rate = t.Other1SalesTaxRate;
							info.PreviousRate = t.Other1SalesTaxPreviousRate;
							info.EffectiveDate = t.Other1SalesTaxRateEffectiveDate;
							SetFlags(info, t);
							districtOther1Tax.Add(t.SignatureCodeOther1, info);
							list.Add(info);

							info.CountyCode = t.SignatureCodeOther1;
							info.CountyName = t.CountyName;
						}
						else
						{
							list.Add(districtOther1Tax[t.SignatureCodeOther1]);
						}
					}
					if (t.Other2TaxIsCity == "C" && (t.Other2SalesTaxRate > 0 || t.Other2SalesTaxPreviousRate > 0))
					{
						if (!string.IsNullOrEmpty(t.SignatureCodeOther2))
							zoneKey = t.SignatureCodeOther2;

						if (!districtOther2Tax.ContainsKey(t.SignatureCodeOther2))
						{
							string taxID = t.StateCode + "CO2" + t.SignatureCodeOther2;
							TaxRecord info = AppendTax(taxID, "County Other 2 Tax " + t.CountyName);
							info.Rate = t.Other2SalesTaxRate;
							info.PreviousRate = t.Other2SalesTaxPreviousRate;
							info.EffectiveDate = t.Other2SalesTaxRateEffectiveDate;
							SetFlags(info, t);
							districtOther2Tax.Add(t.SignatureCodeOther2, info);
							list.Add(info);

							info.CountyCode = t.SignatureCodeOther2;
							info.CountyName = t.CountyName;
						}
						else
						{
							list.Add(districtOther2Tax[t.SignatureCodeOther2]);
						}
					}
					if (t.Other3TaxIsCity == "C" && (t.Other3SalesTaxRate > 0 || t.Other3SalesTaxPreviousRate > 0))
					{
						if (!string.IsNullOrEmpty(t.SignatureCodeOther3))
							zoneKey = t.SignatureCodeOther3;

						if (!districtOther3Tax.ContainsKey(t.SignatureCodeOther3))
						{
							string taxID = t.StateCode + "CO3" + t.SignatureCodeOther3;
							TaxRecord info = AppendTax(taxID, "County Other 3 Tax " + t.CountyName);
							info.Rate = t.Other3SalesTaxRate;
							info.PreviousRate = t.Other3SalesTaxPreviousRate;
							info.EffectiveDate = t.Other3SalesTaxRateEffectiveDate;
							SetFlags(info, t);
							districtOther3Tax.Add(t.SignatureCodeOther3, info);
							list.Add(info);

							info.CountyCode = t.SignatureCodeOther3;
							info.CountyName = t.CountyName;
						}
						else
						{
							list.Add(districtOther3Tax[t.SignatureCodeOther3]);
						}
					}

					if (t.Other4TaxIsCity == "C" && (t.Other4SalesTaxRate > 0 || t.Other4SalesTaxPreviousRate > 0))
					{
						if (!string.IsNullOrEmpty(t.SignatureCodeOther4))
							zoneKey = t.SignatureCodeOther4;

						if (!districtOther4Tax.ContainsKey(t.SignatureCodeOther4))
						{
							string taxID = t.StateCode + "CO4" + t.SignatureCodeOther4;
							TaxRecord info = AppendTax(taxID, "County Other 4 Tax " + t.CountyName);
							info.Rate = t.Other4SalesTaxRate;
							info.PreviousRate = t.Other4SalesTaxPreviousRate;
							info.EffectiveDate = t.Other4SalesTaxRateEffectiveDate;
							SetFlags(info, t);
							districtOther4Tax.Add(t.SignatureCodeOther4, info);
							list.Add(info);

							info.CountyCode = t.SignatureCodeOther4;
							info.CountyName = t.CountyName;
						}
						else
						{
							list.Add(districtOther4Tax[t.SignatureCodeOther4]);
						}
					} 
					#endregion

					#region City
					if (t.CitySalesTaxRate > 0 || t.CitySalesTaxPreviousRate > 0)
					{
						if (!string.IsNullOrEmpty(t.SignatureCodeCity))
							zoneKey = t.SignatureCodeCity;

						if (!cityTax.ContainsKey(t.SignatureCodeCity))
						{
							string taxID = t.StateCode + (t.SignatureCodeCity);
							TaxRecord info = AppendTax(taxID, "City Tax " + t.CityName);
							info.Rate = t.CitySalesTaxRate;
							info.PreviousRate = t.CitySalesTaxPreviousRate;
							info.EffectiveDate = t.CitySalesTaxRateEffectiveDate;
							SetFlags(info, t);
							cityTax.Add(t.SignatureCodeCity, info);
							list.Add(info);

							info.CityCode = t.SignatureCodeCity;
							info.CityName = t.CityName;
						}
						else
						{
							list.Add(cityTax[t.SignatureCodeCity]);
						}
					}

					if (t.TransitTaxIsCity == "T" && (t.TransitSalesTaxRate > 0 || t.TransitSalesTaxPreviousRate > 0))
					{
						if (!string.IsNullOrEmpty(t.SignatureCodeTransit))
							zoneKey = t.SignatureCodeTransit;

						if (!cityTransitTax.ContainsKey(t.SignatureCodeTransit))
						{
							string taxID = t.StateCode + "TT" + (t.SignatureCodeCounty);
							TaxRecord info = AppendTax(taxID, "City Transit Tax " + t.CityName);
							info.Rate = t.TransitSalesTaxRate;
							info.PreviousRate = t.TransitSalesTaxPreviousRate;
							info.EffectiveDate = t.TransitSalesTaxRateEffectiveDate;
							SetFlags(info, t);
							cityTransitTax.Add(t.SignatureCodeTransit, info);
							list.Add(info);

							info.CityCode = t.SignatureCodeTransit;
							info.CityName = t.CityName;
						}
						else
						{
							list.Add(cityTransitTax[t.SignatureCodeTransit]);
						}
					}
					if (t.Other1TaxIsCity == "T" && (t.Other1SalesTaxRate > 0 || t.Other1SalesTaxPreviousRate > 0))
					{
						if (!string.IsNullOrEmpty(t.SignatureCodeOther1))
							zoneKey = t.SignatureCodeOther1;

						if (!cityOther1Tax.ContainsKey(t.SignatureCodeOther1))
						{
							string taxID = t.StateCode + "TO1" + (t.SignatureCodeOther1);
							TaxRecord info = AppendTax(taxID, "City Other 1 Tax " + t.CityName);
							info.Rate = t.Other1SalesTaxRate;
							info.PreviousRate = t.Other1SalesTaxPreviousRate;
							info.EffectiveDate = t.Other1SalesTaxRateEffectiveDate;
							SetFlags(info, t);
							cityOther1Tax.Add(t.SignatureCodeOther1, info);
							list.Add(info);

							info.CityCode = t.SignatureCodeOther1;
							info.CityName = t.CityName;
						}
						else
						{
							list.Add(cityOther1Tax[t.SignatureCodeOther1]);
						}
					}
					if (t.Other2TaxIsCity == "T" && (t.Other2SalesTaxRate > 0 || t.Other2SalesTaxPreviousRate > 0))
					{
						if (!string.IsNullOrEmpty(t.SignatureCodeOther2))
							zoneKey = t.SignatureCodeOther2;
						
						if (!cityOther2Tax.ContainsKey(t.SignatureCodeOther2))
						{
							string taxID = t.StateCode + "TO2" + (t.SignatureCodeOther2);
							TaxRecord info = AppendTax(taxID, "City Other 2 Tax " + t.CityName);
							info.Rate = t.Other2SalesTaxRate;
							info.PreviousRate = t.Other2SalesTaxPreviousRate;
							info.EffectiveDate = t.Other2SalesTaxRateEffectiveDate;
							SetFlags(info, t);
							cityOther2Tax.Add(t.SignatureCodeOther2, info);
							list.Add(info);

							info.CityCode = t.SignatureCodeOther2;
							info.CityName = t.CityName;
						}
						else
						{
							list.Add(cityOther2Tax[t.SignatureCodeOther2]);
						}
					}
					if (t.Other3TaxIsCity == "T" && (t.Other3SalesTaxRate > 0 || t.Other3SalesTaxPreviousRate > 0))
					{
						if (!string.IsNullOrEmpty(t.SignatureCodeOther3))
							zoneKey = t.SignatureCodeOther3;
						
						if (!cityOther3Tax.ContainsKey(t.SignatureCodeOther3))
						{
							string taxID = t.StateCode + "TO3" + t.SignatureCodeOther3;
							TaxRecord info = AppendTax(taxID, "City Other 3 Tax " + t.CityName);
							info.Rate = t.Other3SalesTaxRate;
							info.PreviousRate = t.Other3SalesTaxPreviousRate;
							info.EffectiveDate = t.Other3SalesTaxRateEffectiveDate;
							SetFlags(info, t);
							cityOther3Tax.Add(t.SignatureCodeOther3, info);
							list.Add(info);

							info.CityCode = t.CityTaxCodeAssignedByState ?? t.OtherTaxCode1AssignedByState;
							info.CityName = t.CityName;
						}
						else
						{
							list.Add(cityOther3Tax[t.SignatureCodeOther3]);
						}
					}

					if (t.Other4TaxIsCity == "T" && (t.Other4SalesTaxRate > 0 || t.Other4SalesTaxPreviousRate > 0))
					{
						if (!string.IsNullOrEmpty(t.SignatureCodeOther4))
							zoneKey = t.SignatureCodeOther4;

						if (!cityOther4Tax.ContainsKey(t.SignatureCodeOther4))
						{
							string taxID = t.StateCode + "TO4" + t.SignatureCodeOther4;
							TaxRecord info = AppendTax(taxID, "City Other 4 Tax " + t.CityName);
							info.Rate = t.Other4SalesTaxRate;
							info.PreviousRate = t.Other4SalesTaxPreviousRate;
							info.EffectiveDate = t.Other4SalesTaxRateEffectiveDate;
							SetFlags(info, t);
							cityOther4Tax.Add(t.SignatureCodeOther4, info);
							list.Add(info);

							info.CityCode = t.SignatureCodeOther4;
							info.CityName = t.CityName;
						}
						else
						{
							list.Add(cityOther4Tax[t.SignatureCodeOther4]);
						}
					}
					#endregion


					ZoneRecord zr = null;
					if (!zoneDict.ContainsKey(zoneKey))
					{
						string countyZoneID = null;
						string cityZoneID = null;
						string cityName = null;
						string countyName = null;
						
						foreach (TaxRecord tr in list)
						{
							if (countyZoneID == null && !string.IsNullOrEmpty(tr.CountyCode))
							{
								countyZoneID = State + tr.CountyCode;
								countyName = tr.CountyName;
							}
							else if (countyZoneID != null && !string.IsNullOrEmpty(tr.CountyCode))
							{
								if (countyZoneID != (State + tr.CountyCode))
									Debug.Print(string.Format("CountyZoneID mismatch {0} <> {1}", countyZoneID, (State + tr.CountyCode)));
							}

							if (cityZoneID == null && !string.IsNullOrEmpty(tr.CityCode))
							{
								cityZoneID = State + tr.CityCode;
								cityName = tr.CityName;
							}
							else if (cityZoneID != null && !string.IsNullOrEmpty(tr.CityCode))
							{
								if (cityZoneID != (State + tr.CityCode))
									Debug.Print(string.Format("CityZoneID mismatch {0} <> {1}", cityZoneID, (State + tr.CityCode)));
							}
						}

						string zoneID = cityZoneID;
						string zoneDesc = countyName;

						if (zoneID == null)
							zoneID = countyZoneID;
						
						if ( !string.IsNullOrEmpty(cityName) )
							zoneDesc = cityName + " - " + countyName;

						
						if (zoneID == null)
							zoneID = zoneKey;

						if (zoneDesc == null)
							zoneDesc = State;
												
						zr = new ZoneRecord();
						zr.ZoneID = zoneID;
						zr.Description = zoneDesc;
						zr.CombinedRate = t.CombinedSalesTaxRate;
						zones.Add(zr);
						zoneDict.Add(zoneKey, zr);

						foreach (TaxRecord tr in list)
						{
							AppendZoneDetail(zr, tr);
						}
						
					}
					else
					{
						zr = zoneDict[zoneKey];
					}
					
					
					AppendZip(zr, t);
				}
			}
		}
	}

}
