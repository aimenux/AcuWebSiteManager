using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Data;
using PX.Objects.RQ;

namespace PX.Objects.TX
{   
    [Serializable]
	public class TaxByZipEnq : PXGraph<TaxByZipEnq>
	{
        [Serializable]
		public partial class TaxFilter : IBqlTable
		{
			#region TaxDate
			public abstract class taxDate : PX.Data.BQL.BqlDateTime.Field<taxDate> { }
			protected DateTime? _TaxDate ;
			[PXDBDate()]
			[PXDefault(typeof(AccessInfo.businessDate))]
			[PXUIField(DisplayName = "Tax Date", Visibility = PXUIVisibility.Visible)]
			public virtual DateTime? TaxDate
			{
				get
				{
					return this._TaxDate;
				}
				set
				{
					this._TaxDate = value;
				}
			}
			#endregion
			#region TaxZoneID
			public abstract class taxZoneID : PX.Data.BQL.BqlString.Field<taxZoneID> { }
			protected String _TaxZoneID;
			[PXDBString(10, IsUnicode = true)]
			[PXUIField(DisplayName = "Tax Zone ID", Visibility = PXUIVisibility.SelectorVisible)]
			[PXSelector(typeof(Search3<TaxZone.taxZoneID, OrderBy<Asc<TaxZone.taxZoneID>>>), CacheGlobal = true)]
			public virtual String TaxZoneID
			{
				get
				{
					return this._TaxZoneID;
				}
				set
				{
					this._TaxZoneID = value;
				}
			}
			#endregion
			#region TaxID
			public abstract class taxID : PX.Data.BQL.BqlString.Field<taxID> { }
			protected String _TaxID;
			[PXDBString(Tax.taxID.Length, IsUnicode = true)]
			[PXUIField(DisplayName = "Tax ID", Visibility = PXUIVisibility.SelectorVisible)]
			[PXSelector(typeof(Tax.taxID), DescriptionField = typeof(Tax.descr), CacheGlobal = true)]
			public virtual String TaxID
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

		public PXCancel<TaxFilter> Cancel;
		public PXFilter<TaxFilter> Filter;
		public PXSelect<TaxZoneDetails> Records;

		public override System.Collections.IEnumerable ExecuteSelect(string viewName, object[] parameters, object[] searches, string[] sortcolumns, bool[] descendings, PXFilterRow[] filters, ref int startRow, int maximumRows, ref int totalRows)
		{
			return base.ExecuteSelect(viewName, parameters, searches, sortcolumns, descendings, filters, ref startRow, maximumRows, ref totalRows);
		}
		public TaxByZipEnq()
		{
			Records.WhereAndCurrent<TaxFilter>();
		}

		[PXProjection(typeof(Select2<TaxZoneDet,
			InnerJoin<Tax, On<Tax.taxID, Equal<TaxZoneDet.taxID>>,
			InnerJoin<TaxZoneZip, On<TaxZoneZip.taxZoneID, Equal<TaxZoneDet.taxZoneID>>>>>))]
        [Serializable]
		public partial class TaxZoneDetails : TaxZoneDet
		{
			#region ZipCode
			public abstract class zipCode : PX.Data.BQL.BqlString.Field<zipCode> { }
			protected String _ZipCode;
			[PXUIField(DisplayName = "Zip Code")]
			[PXDBString(9, IsKey = true, BqlField = typeof(TaxZoneZip.zipCode))]
			public virtual String ZipCode
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
			#region Descr
			public abstract class descr : PX.Data.BQL.BqlString.Field<descr> { }
			protected String _Descr;
			[PXDBString(60, IsUnicode = true, BqlField = typeof(Tax.descr))]
			[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
			[PX.Data.EP.PXFieldDescription]
			public virtual String Descr
			{
				get
				{
					return this._Descr;
				}
				set
				{
					this._Descr = value;
				}
			}
			#endregion
			#region TaxRate
			public abstract class taxRate : PX.Data.BQL.BqlDecimal.Field<taxRate> { }
			protected Decimal? _TaxRate;
			[PXDecimal(6)]
			[PXDefault(TypeCode.Decimal, "0.0")]
			[PXUIField(DisplayName = "Tax Rate")]
			[PXDBScalar(typeof(Search<TaxRev.taxRate,
				Where<TaxRev.taxID, Equal<TaxZoneDet.taxID>,
					And<CurrentValue<TaxFilter.taxDate>, Between<TaxRev.startDate, TaxRev.endDate>>>>))]
			public virtual Decimal? TaxRate
			{
				get
				{
					return this._TaxRate;
				}
				set
				{
					this._TaxRate = value;
				}
			}
			#endregion
		}
	}
}
