using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Data;
using System.Collections;
using System.Diagnostics;

namespace PX.Objects.TX
{

	public class TaxExplorer : PXGraph<TaxExplorer>
	{
		public PXFilter<TaxBuilderFilter> Filter;
		public PXSelect<TaxRecord> TaxRecords;
		public PXSelect<ZoneRecord> ZoneRecords;
		public PXSelectJoin<ZoneDetailRecord, InnerJoin<TaxRecord, On<ZoneDetailRecord.taxID, Equal<TaxRecord.taxID>>>> ZoneDetailRecords;

		private TaxBuilder.Result result;

		public virtual IEnumerable taxRecords()
		{
			TaxBuilderFilter filter = Filter.Current;
			if (filter == null)
			{
				yield break;
			}

			if (filter.State == null)
			{
				yield break;
			}

			bool found = false;
			foreach (TaxRecord item in TaxRecords.Cache.Inserted)
			{
				found = true;
				yield return item;
			}
			if (found)
				yield break;

			foreach (TaxRecord record in Taxes)
			{
				TaxRecords.Cache.SetStatus(record, PXEntryStatus.Inserted);
				yield return record;
			}
		}

		public virtual IEnumerable zoneRecords()
		{
			TaxBuilderFilter filter = Filter.Current;
			if (filter == null)
			{
				yield break;
			}

			if (filter.State == null)
			{
				yield break;
			}

			bool found = false;
			foreach (ZoneRecord item in ZoneRecords.Cache.Inserted)
			{
				found = true;
				yield return item;
			}
			if (found)
				yield break;

			foreach (ZoneRecord record in Zones)
			{
				ZoneRecords.Cache.SetStatus(record, PXEntryStatus.Inserted);
				yield return record;
			}
		}

		public virtual IEnumerable zoneDetailRecords(string zoneID)
		{
			TaxBuilderFilter filter = Filter.Current;
			if (filter == null)
			{
				yield break;
			}

			if (filter.State == null)
			{
				yield break;
			}

			if (string.IsNullOrEmpty(zoneID))
			{
				yield break;
			}

			bool found = false;
			foreach (ZoneDetailRecord item in ZoneDetailRecords.Cache.Inserted)
			{
				if (item.ZoneID == zoneID)
				{

					TaxRecord tx = (from t in Taxes
									where t.TaxID == item.TaxID
									select t).Single();

					PXResult<ZoneDetailRecord, TaxRecord> res = new PXResult<ZoneDetailRecord, TaxRecord>(item, tx);

					found = true;
					yield return res;
				}
			}
			if (found)
				yield break;

			foreach (ZoneDetailRecord record in ZoneDetails)
			{
				if (record.ZoneID == zoneID)
				{
					ZoneDetailRecords.Cache.SetStatus(record, PXEntryStatus.Inserted);

					TaxRecord tx = (from t in Taxes
									where t.TaxID == record.TaxID
									select t).Single();

					PXResult<ZoneDetailRecord, TaxRecord> res = new PXResult<ZoneDetailRecord, TaxRecord>(record, tx);


					yield return res;
				}
			}
		}

		public IList<TaxRecord> Taxes
		{
			get
			{
				if (result == null)
				{
					result = TaxBuilderEngine.Execute(this, Filter.Current.State);
				}

				return result.Taxes;
			}
		}

		public IList<ZoneRecord> Zones
		{
			get
			{
				if (result == null)
				{
					result = TaxBuilderEngine.Execute(this, Filter.Current.State);
				}

				return result.Zones;
			}
		}

		public IList<ZoneDetailRecord> ZoneDetails
		{
			get
			{
				if (result == null)
				{
					result = TaxBuilderEngine.Execute(this, Filter.Current.State);
				}

				return result.ZoneDetails;
			}
		}


		protected virtual void TaxBuilderFilter_State_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			TaxBuilderFilter row = e.Row as TaxBuilderFilter;
			if (row != null)
			{
				result = null;
				TaxRecords.Cache.Clear();
				ZoneRecords.Cache.Clear();
				ZoneDetailRecords.Cache.Clear();
			}
		}


	}

	[Serializable]
	public partial class TaxBuilderFilter : IBqlTable
	{
		#region State
		public abstract class state : PX.Data.BQL.BqlString.Field<state> { }
		protected String _State;
		[PXString(2, IsFixed = true)]
		[PXSelector(typeof(TXImportState.stateCode))]
		[PXUIField(DisplayName = "State")]
		public virtual String State
		{
			get
			{
				return this._State;
			}
			set
			{
				this._State = value;
			}
		}
		#endregion
	}

	
	[Serializable]
	[DebuggerDisplay("{TaxID}={Rate}")]
    public partial class TaxRecord : IBqlTable
    {            
        #region TaxID
        public abstract class taxID : PX.Data.BQL.BqlString.Field<taxID> { }
        protected string _TaxID;
		[PXString(Tax.taxID.Length, IsKey = true, IsUnicode = true)]
        [PXUIField(DisplayName = "Tax ID")]
        public virtual string TaxID
        {
            get
            {
                return this._TaxID;
            }
            set
            {
                this._TaxID = value;
            }
        }
        #endregion
		#region Description
        public abstract class description : PX.Data.BQL.BqlString.Field<description> { }
        protected string _Description;
        [PXString(60, IsUnicode=true)]
        [PXUIField(DisplayName = "Description")]
        public virtual string Description
        {
            get
            {
                return this._Description;
            }
            set
            {
                this._Description = value;
            }
        }
        #endregion
		#region Rate
        public abstract class rate : PX.Data.BQL.BqlDecimal.Field<rate> { }
        protected decimal? _Rate;
        [PXDecimal(5)]
        [PXUIField(DisplayName = "Rate")]
        public virtual decimal? Rate
        {
            get
            {
                return this._Rate;
            }
            set
            {
                this._Rate = value;
            }
        }
        #endregion
		#region EffectiveDate
		public abstract class effectiveDate : PX.Data.BQL.BqlDateTime.Field<effectiveDate> { }
		protected DateTime? _EffectiveDate;
		[PXDate()]
		[PXUIField(DisplayName = "Effective Date", Visibility = PXUIVisibility.Visible)]
		public virtual DateTime? EffectiveDate
		{
			get
			{
				return this._EffectiveDate;
			}
			set
			{
				this._EffectiveDate = value;
			}
		}
		#endregion
		#region PreviousRate
        public abstract class previousRate : PX.Data.BQL.BqlDecimal.Field<previousRate> { }
        protected decimal? _PreviousRate;
        [PXDecimal(5)]
        [PXUIField(DisplayName = "Previous Rate")]
        public virtual decimal? PreviousRate
        {
            get
            {
                return this._PreviousRate;
            }
            set
            {
                this._PreviousRate = value;
            }
        }
        #endregion
		#region TaxableMax
		public abstract class taxableMax : PX.Data.BQL.BqlDecimal.Field<taxableMax> { }
		protected decimal? _TaxableMax;
		[PXDecimal(4)]
		[PXUIField(DisplayName = "Taxable Max.")]
		public virtual decimal? TaxableMax
		{
			get
			{
				return this._TaxableMax;
			}
			set
			{
				this._TaxableMax = value;
			}
		}
		#endregion
		#region RateOverMax
		public abstract class rateOverMax : PX.Data.BQL.BqlDecimal.Field<rateOverMax> { }
		protected decimal? _RateOverMax;
		[PXDecimal(5)]
		[PXUIField(DisplayName = "Rate Over Max.")]
		public virtual decimal? RateOverMax
		{
			get
			{
				return this._RateOverMax;
			}
			set
			{
				this._RateOverMax = value;
			}
		}
		#endregion

		public string CountyCode { get; set; }
		public string CityCode { get; set; }

		public string CountyName { get; set; }
		public string CityName { get; set; }
		
		#region IsTaxable
		public abstract class isTaxable : PX.Data.BQL.BqlBool.Field<isTaxable> { }
		protected bool? _IsTaxable;
		[PXBool()]
		[PXUIField(DisplayName = "Taxable")]
		public virtual bool? IsTaxable
		{
			get
			{
				return this._IsTaxable;
			}
			set
			{
				this._IsTaxable = value;
			}
		}
		#endregion
		#region IsFreight
		public abstract class isFreight : PX.Data.BQL.BqlBool.Field<isFreight> { }
		protected bool? _IsFreight;
		[PXBool()]
		[PXUIField(DisplayName = "Freight")]
		public virtual bool? IsFreight
		{
			get
			{
				return this._IsFreight;
			}
			set
			{
				this._IsFreight = value;
			}
		}
		#endregion
		#region IsService
		public abstract class isService : PX.Data.BQL.BqlBool.Field<isService> { }
		protected bool? _IsService;
		[PXBool()]
		[PXUIField(DisplayName = "Service")]
		public virtual bool? IsService
		{
			get
			{
				return this._IsService;
			}
			set
			{
				this._IsService = value;
			}
		}
		#endregion
		#region IsLabor
		public abstract class isLabor : PX.Data.BQL.BqlBool.Field<isLabor> { }
		protected bool? _IsLabor;
		[PXBool()]
		[PXUIField(DisplayName = "Labor")]
		public virtual bool? IsLabor
		{
			get
			{
				return this._IsLabor;
			}
			set
			{
				this._IsLabor = value;
			}
		}
		#endregion
    }

	[Serializable]
	[DebuggerDisplay("{ZoneID}-{Description}")]
    public partial class ZoneRecord : IBqlTable
    {            
        #region ZoneID
        public abstract class zoneID : PX.Data.BQL.BqlString.Field<zoneID> { }
        protected string _ZoneID;
        [PXString(10, IsKey=true, IsUnicode=true)]
        [PXUIField(DisplayName = "Zone ID")]
        public virtual string ZoneID
        {
            get
            {
                return this._ZoneID;
            }
            set
            {
                this._ZoneID = value;
            }
        }
        #endregion
		#region Description
        public abstract class description : PX.Data.BQL.BqlString.Field<description> { }
        protected string _Description;
        [PXString(60, IsUnicode=true)]
        [PXUIField(DisplayName = "Description")]
        public virtual string Description
        {
            get
            {
                return this._Description;
            }
            set
            {
                this._Description = value;
            }
        }
        #endregion
		#region CombinedRate
		public abstract class combinedRate : PX.Data.BQL.BqlDecimal.Field<combinedRate> { }
		protected decimal? _CombinedRate;
		[PXDecimal(5)]
		[PXUIField(DisplayName = "Combined Rate")]
		public virtual decimal? CombinedRate
		{
			get
			{
				return this._CombinedRate;
			}
			set
			{
				this._CombinedRate = value;
			}
		}
		#endregion
	}

	[Serializable]
	[DebuggerDisplay("{ZoneID}-{TaxID}")]
	public partial class ZoneDetailRecord : IBqlTable
	{
		#region ZoneID
		public abstract class zoneID : PX.Data.BQL.BqlString.Field<zoneID> { }
		protected string _ZoneID;
		[PXString(10, IsKey = true, IsUnicode = true)]
		[PXUIField(DisplayName = "Zone ID")]
		public virtual string ZoneID
		{
			get
			{
				return this._ZoneID;
			}
			set
			{
				this._ZoneID = value;
			}
		}
		#endregion
		#region TaxID
		public abstract class taxID : PX.Data.BQL.BqlString.Field<taxID> { }
		protected string _TaxID;
		[PXString(Tax.taxID.Length, IsKey = true, IsUnicode = true)]
		[PXUIField(DisplayName = "Tax ID")]
		public virtual string TaxID
		{
			get
			{
				return this._TaxID;
			}
			set
			{
				this._TaxID = value;
			}
		}
		#endregion
	}

	[Serializable]
	[DebuggerDisplay("{ZoneID}-{ZipCode}")]
    [PXHidden]
	public partial class ZoneZipRecord : IBqlTable
	{
		#region ZoneID
		public abstract class zoneID : PX.Data.BQL.BqlString.Field<zoneID> { }
		protected string _ZoneID;
		[PXString(10, IsKey = true, IsUnicode = true)]
		[PXUIField(DisplayName = "Zone ID")]
		public virtual string ZoneID
		{
			get
			{
				return this._ZoneID;
			}
			set
			{
				this._ZoneID = value;
			}
		}
		#endregion
		#region ZipCode
		public abstract class zipCode : PX.Data.BQL.BqlString.Field<zipCode> { }
		protected string _ZipCode;
		[PXString(10, IsKey = true, IsUnicode = true)]
		[PXUIField(DisplayName = "Zip Code")]
		public virtual string ZipCode
		{
			get
			{
				return this._ZipCode;
			}
			set
			{
				this._ZipCode = value;
			}
		}
		#endregion
	}

	[Serializable]
	[DebuggerDisplay("{ZoneID}-{ZipCode} [{ZipMin}-{ZipMax}]")]
    [PXHidden]
	public partial class ZoneZipPlusRecord : IBqlTable
	{
		#region ZoneID
		public abstract class zoneID : PX.Data.BQL.BqlString.Field<zoneID> { }
		protected string _ZoneID;
		[PXString(10, IsKey = true, IsUnicode = true)]
		[PXUIField(DisplayName = "Zone ID")]
		public virtual string ZoneID
		{
			get
			{
				return this._ZoneID;
			}
			set
			{
				this._ZoneID = value;
			}
		}
		#endregion
		#region ZipCode
		public abstract class zipCode : PX.Data.BQL.BqlString.Field<zipCode> { }
		protected string _ZipCode;
		[PXString(10, IsKey = true, IsUnicode = true)]
		[PXUIField(DisplayName = "Zip Code")]
		public virtual string ZipCode
		{
			get
			{
				return this._ZipCode;
			}
			set
			{
				this._ZipCode = value;
			}
		}
		#endregion
		#region ZipMin
		public abstract class zipMin : PX.Data.BQL.BqlInt.Field<zipMin> { }
		protected Int32? _ZipMin;
		[PXInt(IsKey = true)]
		[PXDefault()]
		public virtual Int32? ZipMin
		{
			get
			{
				return this._ZipMin;
			}
			set
			{
				this._ZipMin = value;
			}
		}
		#endregion
		#region ZipMax
		public abstract class zipMax : PX.Data.BQL.BqlInt.Field<zipMax> { }
		protected Int32? _ZipMax;
		[PXInt()]
		[PXDefault()]
		public virtual Int32? ZipMax
		{
			get
			{
				return this._ZipMax;
			}
			set
			{
				this._ZipMax = value;
			}
		}
		#endregion
	}
}
