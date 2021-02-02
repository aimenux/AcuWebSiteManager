using System;
using System.Collections;
using System.Collections.Generic;
using CommonServiceLocator;
using PX.SM;
using PX.Data;
using PX.Objects.GL;
using PX.Objects.Extensions.MultiCurrency;

namespace PX.Objects.CM.Extensions
{
	/// <summary>
	/// Stores currency and exchange rate information for a particular document or transaction.
	/// Usually, there is an individual record of this type for each document or transaction that involves monetary amounts and supports multi-currency.
	/// The documents store a link to their instance of CurrencyInfo in the CuryInfoID field, such as <see cref="GLTran.CuryInfoID"/>.
	/// The exchange rate data for objects of this type is either entered by user or obtained from the <see cref="CurrencyRate"/> records.
	/// Records of this type are either created automatically by the <see cref="CurrencyInfoAttribute"/> or 
	/// explicitly inserted by the application code (such as in <see cref="AR.ARReleaseProcess"/>).
	/// User must not be aware of existence of these records.
	/// </summary>
	[Serializable]
	[ForceSaveInDash(false)]
	[PXCacheName(CM.Messages.CurrencyInfo)]
	public partial class CurrencyInfo : PX.Data.IBqlTable
	{
		#region CuryInfoID
		public abstract class curyInfoID : PX.Data.BQL.BqlLong.Field<curyInfoID> { }
		/// <summary>
		/// Key field. Database identity.
		/// Unique identifier of the Currency Info object.
		/// </summary>
		[PXDBLongIdentity(IsKey = true)]
		[PXUIField(Visible = false)]
		[PXDependsOnFields(typeof(CurrencyInfo.curyID), typeof(CurrencyInfo.baseCuryID), typeof(CurrencyInfo.sampleCuryRate))]
		public virtual Int64? CuryInfoID
		{
			get;
			set;
		}
		#endregion
		#region ModuleCode
		public abstract class moduleCode : PX.Data.BQL.BqlString.Field<moduleCode> { }
		/// <summary>
		/// Identifier of the module, to which the Currency Info object belongs.
		/// The value of this field affects the choice of the default <see cref="CuryRateTypeID">Rate Type</see>:
		/// for <c>"CA"</c> the Rate Type is taken from the <see cref="CMSetup.CARateTypeDflt" />,
		/// for <c>"AP"</c> the Rate Type is taken from the <see cref="CMSetup.APRateTypeDflt" />,
		/// for <c>"AR"</c> the Rate Type is taken from the <see cref="CMSetup.ARRateTypeDflt" />,
		/// for <c>"GL"</c> the Rate Type is taken from the <see cref="CMSetup.GLRateTypeDflt" />.
		/// </summary>
		public virtual string ModuleCode
		{
			get;
			set;
		}
		#endregion
		#region IsReadOnly
		/// <summary>
		/// When set to <c>true</c>, the system won't allow user to change the fields of this object.
		/// </summary>
		public virtual bool? IsReadOnly
		{
			get;
			set;
		}
		#endregion
		#region BaseCalc
		public abstract class baseCalc : PX.Data.BQL.BqlBool.Field<baseCalc> { }
		/// <summary>
		/// When <c>true</c>, indicates that the system must calculate the amounts in the
		/// <see cref="BaseCuryID">base currency</see> for the related document or transaction.
		/// Otherwise the changes in the amounts expressed in the <see cref="CuryID">currency of the document</see>
		/// won't result in an update to the amounts in base currency.
		/// </summary>
		/// <value>
		/// Defaults to <c>true</c>.
		/// </value>
		[PXDBBool]
		[PXDefault(true)]
		public virtual bool? BaseCalc
		{
			get;
			set;
		}
		#endregion
		#region BaseCuryID
		public abstract class baseCuryID : PX.Data.BQL.BqlString.Field<baseCuryID> { }
		/// <summary>
		/// Identifier of the base <see cref="Currency"/>.
		/// </summary>
		/// <value>
		/// Defaults to the <see cref="Company.BaseCuryID">base currency of the company</see>.
		/// </value>
		[PXDBString(5, IsUnicode = true)]
		[PXDefault]
		[PXUIField(DisplayName = "Base Currency ID")]
		public virtual String BaseCuryID
		{
			get;
			set;
		}
		#endregion
		#region CuryID
		protected class CuryIDSelectorAttribute : PXCustomSelectorAttribute
		{
			public CuryIDSelectorAttribute()
				: base(typeof(Currency.curyID))
			{
			}
			public virtual IEnumerable GetRecords()
			{
				foreach (IPXCurrency c in ServiceLocator.Current.GetInstance<Func<PXGraph, IPXCurrencyService>>()(_Graph).Currencies())
				{
					if (c is Currency)
					{
						yield return c; 
					}
					else
					{
						yield return new Currency { CuryID = c.CuryID, Description = c.Description };
					}
				}
			}
		}
		public class CuryIDStringAttribute : PXDBStringAttribute
		{
			protected Dictionary<long, string> _Matches;
			public static Dictionary<long, string> GetMatchesDictionary(PXCache sender)
			{
				foreach (PXEventSubscriberAttribute attr in sender.Graph.Caches[typeof(CurrencyInfo)].GetAttributesReadonly<curyID>())
				{
					if (attr is CuryIDStringAttribute)
					{
						return ((CuryIDStringAttribute)attr)._Matches;
					}
				}
				return null;
			}
			public CuryIDStringAttribute()
				: base(5)
			{
				IsUnicode = true;
			}
			public override void RowSelecting(PXCache sender, PXRowSelectingEventArgs e)
			{
				base.RowSelecting(sender, e);
				if (_Matches != null)
				{
					CurrencyInfo info = (CurrencyInfo)e.Row;
					if (info != null && info.CuryInfoID != null && !String.IsNullOrEmpty(info.CuryID))
					{
						_Matches[(long)info.CuryInfoID] = info.CuryID;
					}
				}
			}
			public override void CacheAttached(PXCache sender)
			{
				base.CacheAttached(sender);
				if (sender.Graph.FindImplementation<IPXCurrencyHelper>() == null)
				{
					_Matches = new Dictionary<long, string>();
				}
			}
		}
		public abstract class curyID : PX.Data.BQL.BqlString.Field<curyID> { }
		/// <summary>
		/// Identifier of the <see cref="Currency"/> of this Currency Info object.
		/// </summary>
		/// <value>
		/// Defaults to the <see cref="Company.BaseCuryID">base currency of the company</see>.
		/// </value>
		[CuryIDString]
		[PXDefault]
		[PXUIField(DisplayName = "Currency", ErrorHandling = PXErrorHandling.Never)]
		[CuryIDSelector]
		public virtual String CuryID
		{
			get;
			set;
		}
		#endregion
		#region DisplayCuryID
		public abstract class displayCuryID : PX.Data.BQL.BqlString.Field<displayCuryID> { }
		/// <summary>
		/// The read-only property providing the <see cref="CuryID">Currency</see> for display in the User Interface.
		/// </summary>
		[PXString(5, IsUnicode = true)]
		[PXUIField(DisplayName = "Currency ID")]
		[PXDependsOnFields(typeof(curyID))]
		public virtual String DisplayCuryID
		{
			get { return CuryID; }
			set { }
		}
		#endregion
		#region CuryRateTypeID
		protected class CuryRateTypeIDSelectorAttribute : PXCustomSelectorAttribute
		{
			public CuryRateTypeIDSelectorAttribute()
				: base(typeof(CurrencyRateType.curyRateTypeID))
			{
			}
			public virtual IEnumerable GetRecords()
			{
				foreach (IPXCurrencyRateType c in ServiceLocator.Current.GetInstance<Func<PXGraph, IPXCurrencyService>>()(_Graph).CurrencyRateTypes())
				{
					if (c is CurrencyRateType)
					{
						yield return c;
					}
					else
					{
						yield return new CurrencyRateType { CuryRateTypeID = c.CuryRateTypeID, Descr = c.Descr };
					}
				}
			}
		}
		public abstract class curyRateTypeID : PX.Data.BQL.BqlString.Field<curyRateTypeID> { }
		/// <summary>
		/// The identifier of the <see cref="CurrencyRateType">Rate Type</see> associated with this object. 
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="CurrencyRateType.CuryRateTypeID"/> field.
		/// </value>
		[PXDBString(6, IsUnicode = true)]
		[CuryRateTypeIDSelector]
		[PXUIField(DisplayName = "Curr. Rate Type ID")]
		public virtual String CuryRateTypeID
		{
			get;
			set;
		}
		#endregion
		#region CuryEffDate
		public abstract class curyEffDate : PX.Data.BQL.BqlDateTime.Field<curyEffDate> { }
		/// <summary>
		/// The date, starting from which the specified <see cref="CuryRate">rate</see> is considered current.
		/// </summary>
		/// <value>
		/// Defaults to the <see cref="AccessInfo.BusinessDate">current business date</see>.
		/// </value>
		[PXDBDate]
		[PXDefault(typeof(AccessInfo.businessDate))]
		[PXUIField(DisplayName = "Effective Date")]
		public virtual DateTime? CuryEffDate
		{
			get;
			set;
		}
		#endregion
		#region CuryMultDiv
		public abstract class curyMultDiv : PX.Data.BQL.BqlString.Field<curyMultDiv> { }
		/// <summary>
		/// The operation required for currency conversion: Divide or Multiply.
		/// </summary>
		/// <value>
		/// Allowed values are:
		/// <c>"M"</c> - Multiply,
		/// <c>"D"</c> - Divide.
		/// Defaults to <c>"M"</c>.
		/// </value>
		[PXDBString(1, IsFixed = true)]
		[PXDefault(CuryMultDivType.Mult, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Mult Div")]
		public virtual String CuryMultDiv
		{
			get;
			set;
		}
		#endregion
		#region CuryRate
		public abstract class curyRate : PX.Data.BQL.BqlDecimal.Field<curyRate> { }
		/// <summary>
		/// The currency rate. For the purposes of conversion the value of this field is used
		/// together with the operation selected in the <see cref="CuryMultDiv"/> field.
		/// </summary>
		/// <value>
		/// Defaults to <c>1.0</c>.
		/// </value>
		[PXDBDecimal(8)]
		[PXDefault(TypeCode.Decimal, "1.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Decimal? CuryRate
		{
			get;
			set;
		}
		#endregion
		#region RecipRate
		public abstract class recipRate : PX.Data.BQL.BqlDecimal.Field<recipRate> { }
		/// <summary>
		/// The inverse of the <see cref="CuryRate">exchange rate</see>, which is calculated automatically.
		/// </summary>
		[PXDBDecimal(8)]
		[PXDefault(TypeCode.Decimal, "1.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Decimal? RecipRate
		{
			get;
			set;
		}
		#endregion
		#region SampleCuryRate
		public abstract class sampleCuryRate : PX.Data.BQL.BqlDecimal.Field<sampleCuryRate> { }
		/// <summary>
		/// The exchange rate used for calculations and determined by the values of
		/// the <see cref="CuryMultDiv"/>, <see cref="CuryRate"/> and <see cref="RecipRate"/> fields.
		/// </summary>
		[PXDecimal(8)]
		[PXUIField(DisplayName = "Curr. Rate")]
		[PXDefault]
		public virtual Decimal? SampleCuryRate
		{
			[PXDependsOnFields(typeof(curyMultDiv), typeof(curyRate), typeof(recipRate))]
			get
			{
				return (this.CuryMultDiv == "M") ? this.CuryRate : this.RecipRate;
			}
			set
			{
				if (this.CuryMultDiv == "M")
				{
					this.CuryRate = value;
				}
				else
				{
					this.RecipRate = value;
				}
			}
		}
		#endregion
		#region SampleRecipRate
		public abstract class sampleRecipRate : PX.Data.BQL.BqlDecimal.Field<sampleRecipRate> { }

		/// <summary>
		/// The inverse of the <see cref="SampleCuryRate"/>. This value is also determined by the values of
		/// the <see cref="CuryMultDiv"/>, <see cref="CuryRate"/> and <see cref="RecipRate"/> fields.
		/// </summary>
		[PXDecimal(8)]
		[PXUIField(DisplayName = "Reciprocal Rate")]
		public virtual Decimal? SampleRecipRate
		{
			[PXDependsOnFields(typeof(curyMultDiv), typeof(recipRate), typeof(curyRate))]
			get
			{
				return (this.CuryMultDiv == "M") ? this.RecipRate : this.CuryRate;
			}
			set
			{
				if (this.CuryMultDiv == "M")
				{
					this.RecipRate = value;
				}
				else
				{
					this.CuryRate = value;
				}
			}
		}
		#endregion
		#region CuryPrecision
		public abstract class curyPrecision : PX.Data.BQL.BqlShort.Field<curyPrecision> { }
		/// <summary>
		/// The number of digits after the decimal point used in operations with the amounts
		/// associated with this Currency Info object and expressed in its <see cref="CuryID">currency</see>.
		/// </summary>
		/// <value>
		/// The value of this field is taken from the <see cref="CurrencyList.DecimalPlaces"/> field of the record associated with the
		/// <see cref="CuryID">currency</see> of this object.
		/// </value>
		[PXShort]
		public virtual short? CuryPrecision
		{
			get;
			set;
		}
		#endregion
		#region BasePrecision
		public abstract class basePrecision : PX.Data.BQL.BqlShort.Field<basePrecision> { }
		/// <summary>
		/// The number of digits after the decimal point used in operations with the amounts
		/// associated with this Currency Info object and expressed in its <see cref="BaseCuryID">base currency</see>.
		/// </summary>
		/// <value>
		/// The value of this field is taken from the <see cref="CurrencyList.DecimalPlaces"/> field of the record associated with the
		/// <see cref="BaseCuryID">base currency</see> of this object.
		/// </value>
		[PXShort]
		public virtual short? BasePrecision
		{
			get;
			set;
		}
		#endregion
		#region tstamp
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }
		[PXDBTimestamp]
		public virtual Byte[] tstamp
		{
			get;
			set;
		}
		#endregion
		#region Compatibility Conversion
		public CM.CurrencyInfo GetCM()
		{
			return new CM.CurrencyInfo
			{
				BaseCalc = BaseCalc,
				BaseCuryID = BaseCuryID,
				BasePrecision = BasePrecision,
				CuryEffDate = CuryEffDate,
				CuryID = CuryID,
				CuryInfoID = CuryInfoID,
				CuryMultDiv = CuryMultDiv,
				CuryPrecision = CuryPrecision,
				CuryRate = CuryRate,
				CuryRateTypeID = CuryRateTypeID,
				DisplayCuryID = DisplayCuryID,
				IsReadOnly = IsReadOnly,
				ModuleCode = ModuleCode,
				RecipRate = RecipRate,
				SampleCuryRate = SampleCuryRate,
				SampleRecipRate = SampleRecipRate,
				tstamp = tstamp
			};
		}
		public static CurrencyInfo GetEX(CM.CurrencyInfo info)
		{
			return new CurrencyInfo
			{
				BaseCalc = info.BaseCalc,
				BaseCuryID = info.BaseCuryID,
				BasePrecision = info.BasePrecision,
				CuryEffDate = info.CuryEffDate,
				CuryID = info.CuryID,
				CuryInfoID = info.CuryInfoID,
				CuryMultDiv = info.CuryMultDiv,
				CuryPrecision = info.CuryPrecision,
				CuryRate = info.CuryRate,
				CuryRateTypeID = info.CuryRateTypeID,
				DisplayCuryID = info.DisplayCuryID,
				IsReadOnly = info.IsReadOnly,
				ModuleCode = info.ModuleCode,
				RecipRate = info.RecipRate,
				SampleCuryRate = info.SampleCuryRate,
				SampleRecipRate = info.SampleRecipRate,
				tstamp = info.tstamp
			};
		}
		#endregion
	}
}
