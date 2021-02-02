using PX.Objects.CS;
using PX.SM;

namespace PX.Objects.CM
{
	using System;
	using PX.Data;
	using PX.Objects.GL;

	/// <summary>
	/// Stores currency and exchange rate information for a particular document or transaction.
	/// Usually, there is an individual record of this type for each document or transaction that involves monetary amounts and supports multi-currency.
	/// The documents store a link to their instance of CurrencyInfo in the CuryInfoID field, such as <see cref="GLTran.CuryInfoID"/>.
	/// The exchange rate data for objects of this type is either entered by user or obtained from the <see cref="CurrencyRate"/> records.
	/// Records of this type are either created automatically by the <see cref="CurrencyInfoAttribute"/> or 
	/// explicitly inserted by the application code (such as in <see cref="AR.ARReleaseProcess"/>).
	/// User must not be aware of existence of these records.
	/// </summary>
	[System.SerializableAttribute()]
	[ForceSaveInDash(false)]
    [PXCacheName(Messages.CurrencyInfo)]
	public partial class CurrencyInfo : PX.Data.IBqlTable
	{
		private CMSetup getCMSetup(PXCache cache)
		{
			CMSetup CMSetup = (CMSetup)cache.Graph.Caches[typeof(CMSetup)].Current;
			if (CMSetup == null)
			{
				CMSetup = PXSelectReadonly<CMSetup>.Select(cache.Graph);
			}
			return CMSetup;
		}
		private CurrencyRate getCuryRate(PXCache cache)
		{
			if (string.Equals(this.CuryID, this.BaseCuryID, StringComparison.OrdinalIgnoreCase))
			{
				return null;
			}

			return PXSelectReadonly<CurrencyRate,
							Where<CurrencyRate.toCuryID, Equal<Required<CurrencyInfo.baseCuryID>>,
							And<CurrencyRate.fromCuryID, Equal<Required<CurrencyInfo.curyID>>,
							And<CurrencyRate.curyRateType, Equal<Required<CurrencyInfo.curyRateTypeID>>,
							And<CurrencyRate.curyEffDate, LessEqual<Required<CurrencyInfo.curyEffDate>>>>>>,
							OrderBy<Desc<CurrencyRate.curyEffDate>>>.SelectWindowed(cache.Graph, 0, 1, BaseCuryID, CuryID, CuryRateTypeID, CuryEffDate);
		}
		public virtual void SetCuryEffDate(PXCache sender, object value)
		{
			sender.SetValue<CurrencyInfo.curyEffDate>(this, value);
			defaultCuryRate(sender, false);
		}

		public virtual bool AllowUpdate(PXCache sender)
		{
			if (sender != null && !sender.AllowUpdate && !sender.AllowDelete) return false;
			return !(this.IsReadOnly == true || (this.CuryID == this.BaseCuryID));
		}

		private void SetDefaultEffDate(PXCache cache)
		{
			object newValue;
			if (cache.RaiseFieldDefaulting<CurrencyInfo.curyEffDate>(this, out newValue))
			{
				cache.RaiseFieldUpdating<CurrencyInfo.curyEffDate>(this, ref newValue);
			}
			this.CuryEffDate = (DateTime?)newValue;
		}

		private void defaultCuryRate(PXCache cache)
		{
			defaultCuryRate(cache, true);
		}
		private void defaultCuryRate(PXCache cache, bool ForceDefault)
		{
			CurrencyRate rate = getCuryRate(cache);
			if (rate != null)
			{
				DateTime? UserCuryEffDate = CuryEffDate;

				CuryEffDate = rate.CuryEffDate;
				CuryRate = Math.Round((decimal)rate.CuryRate, 8);
				CuryMultDiv = rate.CuryMultDiv;
				RecipRate = Math.Round((decimal)rate.RateReciprocal, 8);

				if (rate.CuryEffDate < UserCuryEffDate)
				{
					CurrencyRateType ratetype = (CurrencyRateType)PXSelectorAttribute.Select<CurrencyInfo.curyRateTypeID>(cache, this);
					if (ratetype != null && ratetype.RateEffDays > 0 && ((TimeSpan)(UserCuryEffDate - rate.CuryEffDate)).Days >= ratetype.RateEffDays)
					{
						throw new PXRateIsNotDefinedForThisDateException(rate.CuryRateType, rate.FromCuryID, rate.ToCuryID, (DateTime)UserCuryEffDate);
					}
				}
			}
			else if (ForceDefault)
			{
				if (string.Equals(this.CuryID, this.BaseCuryID, StringComparison.OrdinalIgnoreCase))
				{
					bool dirty = cache.IsDirty;
					CurrencyInfo dflt = new CurrencyInfo();
					cache.SetDefaultExt<CurrencyInfo.curyRate>(dflt);
					cache.SetDefaultExt<CurrencyInfo.curyMultDiv>(dflt);
					cache.SetDefaultExt<CurrencyInfo.recipRate>(dflt);
					CuryRate = Math.Round((decimal)dflt.CuryRate, 8);
					CuryMultDiv = dflt.CuryMultDiv;
					RecipRate = Math.Round((decimal)dflt.RecipRate, 8);
					cache.IsDirty = dirty;
				}
				else if (this._CuryRateTypeID == null || this._CuryEffDate == null)
				{
					this.CuryRate = null;
					this.RecipRate = null;
					this.CuryMultDiv = "M"; 
				}
				else
				{
					this.CuryRate = null;
					this.RecipRate = null;
					this.CuryMultDiv = "M";
					throw new PXSetPropertyException(Messages.RateNotFound, PXErrorLevel.Warning);
				}
			}
		}
		public virtual bool CheckRateVariance(PXCache cache)
		{
			CMSetup CMSetup = getCMSetup(cache);
			if (CMSetup != null && CMSetup.RateVarianceWarn == true && CMSetup.RateVariance != 0)
			{
				CurrencyRate rate = getCuryRate(cache);
				if (rate != null && rate.CuryRate != null && rate.CuryRate != 0)
				{
					decimal Variance;
					if (rate.CuryMultDiv == CuryMultDiv || CuryRate == 0)
					{
						Variance = 100 * ((decimal)CuryRate - (decimal)rate.CuryRate) / (decimal)rate.CuryRate;
					}
					else
					{
						Variance = 100 * (1 / (decimal)CuryRate - (decimal)rate.CuryRate) / (decimal)rate.CuryRate;
					}
					if (Math.Abs(Variance) > CMSetup.RateVariance)
					{
						return true;
					}
				}
			}
			return false;
		}
		private sealed class CuryInfoIDAttribute : PXEventSubscriberAttribute
		{
			public override void CacheAttached(PXCache sender)
			{
				base.CacheAttached(sender);
				sender.Graph.CommandPreparing.AddHandler(sender.GetItemType(), _FieldName, Parameter_CommandPreparing);
			}

			public void Parameter_CommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
			{
				long? Key;
				if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Select && (e.Operation & PXDBOperation.Option) != PXDBOperation.External &&
					(e.Operation & PXDBOperation.Option) != PXDBOperation.ReadOnly && e.Row == null && (Key = e.Value as long?) != null)
				{
					if (Key < 0L)
					{
						e.DataValue = null;
						e.Cancel = true;
					}
				}
			} 
		}

		private sealed class CuryRateTypeIDAttribute : PXEventSubscriberAttribute, IPXFieldDefaultingSubscriber, IPXFieldVerifyingSubscriber, IPXFieldUpdatedSubscriber
		{
			public void FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
			{
				CurrencyInfo info = e.Row as CurrencyInfo;
				if (info != null && !String.IsNullOrEmpty(info.ModuleCode))
				{
					CMSetup CMSetup = null;
					if (PXAccess.FeatureInstalled<FeaturesSet.multicurrency>())
					{
						CMSetup = info.getCMSetup(sender);
					}
					
					if (CMSetup != null)
					{
						string rateType;
						switch (info.ModuleCode)
						{
							case BatchModule.PM:
								rateType = CMSetup.PMRateTypeDflt;
								break;
							case BatchModule.CA:
								rateType = CMSetup.CARateTypeDflt;
								break;
							case BatchModule.AP:
								rateType = CMSetup.APRateTypeDflt;
								break;
							case BatchModule.AR:
								rateType = CMSetup.ARRateTypeDflt;
								break;
							case BatchModule.GL:
								rateType = CMSetup.GLRateTypeDflt;
								break;
							default:
								rateType = null;
								break;
						}
						if (!string.IsNullOrEmpty(rateType))
						{
							e.NewValue = rateType;
						}
					}
				}
			}
			public void FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
			{
                if (!PXAccess.FeatureInstalled<CS.FeaturesSet.multicurrency>())
                {
                    e.Cancel = true;
                }
			}
			public void FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
			{
				CurrencyInfo info = e.Row as CurrencyInfo;
				if (info != null)
				{
					//reset effective date to document date first
					info.SetDefaultEffDate(sender);
					try
					{
						info.defaultCuryRate(sender);
					}
					catch (PXSetPropertyException ex)
					{
						if (e.ExternalCall)
						{
							sender.RaiseExceptionHandling(_FieldName, e.Row, sender.GetValue(e.Row, _FieldOrdinal), ex);
						}
					}
				}
			}
		}
		private sealed class CuryEffDateAttribute : PXEventSubscriberAttribute, IPXFieldUpdatedSubscriber
		{
			public void FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
			{
				CurrencyInfo info = e.Row as CurrencyInfo;
				if (info != null)
				{
					try
					{
						info.defaultCuryRate(sender);
					}
					catch (PXSetPropertyException ex)
					{
						sender.RaiseExceptionHandling(_FieldName, e.Row, sender.GetValue(e.Row, _FieldOrdinal), ex);
					}
				}
			}
		}
		public sealed class CuryIDAttribute : PXEventSubscriberAttribute, IPXFieldUpdatedSubscriber, IPXRowSelectedSubscriber, IPXRowUpdatingSubscriber, IPXRowUpdatedSubscriber, IPXFieldVerifyingSubscriber
		{
			public void FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
			{
				CurrencyInfo info = e.Row as CurrencyInfo;
				if (info != null)
				{
					//reset effective date to document date first
					info.SetDefaultEffDate(sender);
					try
					{
						info.defaultCuryRate(sender);
					}
					catch (PXSetPropertyException ex)
					{
						sender.RaiseExceptionHandling(_FieldName, e.Row, sender.GetValue(e.Row, _FieldOrdinal), ex);
					}
					info.CuryPrecision = null;
				}
			}
			public void RowSelected(PXCache sender, PXRowSelectedEventArgs e)
			{
				CurrencyInfo info = e.Row as CurrencyInfo;
				if (info != null)
				{
					bool disabled = info.IsReadOnly == true || (info.CuryID == info.BaseCuryID);
					PXUIFieldAttribute.SetEnabled<CurrencyInfo.curyMultDiv>(sender, info, !disabled);
					PXUIFieldAttribute.SetEnabled<CurrencyInfo.sampleCuryRate>(sender, info, !disabled);
					PXUIFieldAttribute.SetEnabled<CurrencyInfo.sampleRecipRate>(sender, info, !disabled);
					PXUIFieldAttribute.SetEnabled<CurrencyInfo.curyRateTypeID>(sender, info, !disabled);
					PXUIFieldAttribute.SetEnabled<CurrencyInfo.curyEffDate>(sender, info, !disabled);
					PXUIFieldAttribute.SetEnabled<CurrencyInfo.baseCuryID>(sender, info, false);
					PXUIFieldAttribute.SetEnabled<CurrencyInfo.displayCuryID>(sender, info, false);
					PXUIFieldAttribute.SetEnabled<CurrencyInfo.curyID>(sender, info, true);
				}
			}
			private bool? currencyInfoDirty = null;
			public void RowUpdating(PXCache sender, PXRowUpdatingEventArgs e)
			{
				CurrencyInfo info = e.Row as CurrencyInfo;
				if (info != null && info._IsReadOnly == true)
				{
					e.Cancel = true;
				}
				else
				{
					currencyInfoDirty = sender.IsDirty;
				}
			}
			public void RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
			{
				CurrencyInfo info = e.Row as CurrencyInfo;
				if (info != null)
				{
					CurrencyInfo old = e.OldRow as CurrencyInfo;
					if (old != null && (String.IsNullOrEmpty(info.CuryID) || String.IsNullOrEmpty(info.BaseCuryID)))
					{
						info.BaseCuryID = old.BaseCuryID;
						info.CuryID = old.CuryID;
					}
					if (currencyInfoDirty == false
						&& info.CuryID == old.CuryID
						&& info.CuryRateTypeID == old.CuryRateTypeID
						&& info.CuryEffDate == old.CuryEffDate
						&& info.CuryMultDiv == old.CuryMultDiv
						&& info.CuryRate == old.CuryRate)
					{
						sender.IsDirty = false;
						currencyInfoDirty = null;
					}
				}
			}
			public void FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
			{
				CurrencyInfo info = e.Row as CurrencyInfo;
				if (info != null)
				{
					CMSetup CMSetup = info.getCMSetup(sender);
					//if (CMSetup != null && (bool)CMSetup.MCActivated)
                    //{

                    //    if (info.ModuleCode == "AP" && CMSetup.APCuryOverride != true ||
                    //        info.ModuleCode == "AR" && CMSetup.ARCuryOverride != true)
                    //    {
                    //        object newValue;
                    //        sender.RaiseFieldDefaulting<CurrencyInfo.curyID>(e.Row, out newValue);
                    //        if (!object.Equals(newValue, e.NewValue))
                    //        {
                    //            throw new PXSetPropertyException(Messages.CuryIDCannotBeChanged, "CuryID");
                    //        }
                    //    }
                    //}
				}
			}
		}

		public sealed class BaseCuryIDAttribute : PXEventSubscriberAttribute, IPXFieldUpdatedSubscriber
		{
			public void FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
			{
				var info = e.Row as CurrencyInfo;
				if (info != null)
				{
					info.BasePrecision = null;
				}
			}
		}

		private sealed class CuryRateAttribute : PXEventSubscriberAttribute, IPXFieldUpdatedSubscriber
		{
			public void FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
			{
				CurrencyInfo info = e.Row as CurrencyInfo;

				if (info == null || !e.ExternalCall) return;

				decimal rate = Math.Round((decimal)info.SampleCuryRate, 8);

				bool hasCurrencyRateDefaulted = false;

				if (rate == 0)
				{
					try
					{
						info.defaultCuryRate(sender);
						hasCurrencyRateDefaulted = true;
					}
					catch (PXSetPropertyException)
					{
						rate = 1;
					}
				}

				if (!hasCurrencyRateDefaulted)
				{
					info.CuryRate = rate;
					info.RecipRate = Math.Round(1m / rate, 8);
					info.CuryMultDiv = CuryMultDivType.Mult;
				}

				if (info.CheckRateVariance(sender))
				{
					PXUIFieldAttribute.SetWarning<CurrencyInfo.sampleCuryRate>(
						sender, 
						e.Row, 
						Messages.RateVarianceExceeded);
				}
			}
		}
		private sealed class CuryRecipRateAttribute : PXEventSubscriberAttribute, IPXFieldUpdatedSubscriber
		{
			public void FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
			{
				CurrencyInfo info = e.Row as CurrencyInfo;
				if (info != null && e.ExternalCall)
				{
					decimal rate = Math.Round((decimal)info.SampleRecipRate, 8);
					if (rate == 0)
						rate = 1;
					info.CuryRate = rate;
					info.RecipRate = Math.Round((decimal)(1 / rate), 8);
					info.CuryMultDiv = "D";
					if (info.CheckRateVariance(sender))
					{
						PXUIFieldAttribute.SetWarning(sender, e.Row, "SampleRecipRate", Messages.RateVarianceExceeded);
					}
				}
			}
		}
		#region CuryInfoID
		public abstract class curyInfoID : PX.Data.BQL.BqlLong.Field<curyInfoID> { }
		protected Int64? _CuryInfoID;

        /// <summary>
        /// Key field. Database identity.
        /// Unique identifier of the Currency Info object.
        /// </summary>
		[PXDBLongIdentity(IsKey = true)]
		[PXUIField(Visible = false)]
		[PXDependsOnFields(typeof(CurrencyInfo.curyID), typeof(CurrencyInfo.baseCuryID), typeof(CurrencyInfo.sampleCuryRate))]
		[CuryInfoIDAttribute]
		public virtual Int64? CuryInfoID
		{
			get
			{
				return this._CuryInfoID;
			}
			set
			{
				this._CuryInfoID = value;
			}
		}
		#endregion
		#region ModuleCode
		protected string _ModuleCode;

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
			get
			{
				return this._ModuleCode;
			}
			set
			{
				this._ModuleCode = value;
			}
		}
		#endregion
		#region IsReadOnly
		protected bool? _IsReadOnly;

        /// <summary>
        /// When set to <c>true</c>, the system won't allow user to change the fields of this object.
        /// </summary>
		public virtual bool? IsReadOnly
		{
			get
			{
				return this._IsReadOnly;
			}
			set
			{
				this._IsReadOnly = value;
			}
		}
		#endregion
		#region BaseCalc
		public abstract class baseCalc : PX.Data.BQL.BqlBool.Field<baseCalc> { }
		protected bool? _BaseCalc;

        /// <summary>
        /// When <c>true</c>, indicates that the system must calculate the amounts in the
        /// <see cref="BaseCuryID">base currency</see> for the related document or transaction.
        /// Otherwise the changes in the amounts expressed in the <see cref="CuryID">currency of the document</see>
        /// won't result in an update to the amounts in base currency.
        /// </summary>
        /// <value>
        /// Defaults to <c>true</c>.
        /// </value>
		[PXDBBool()]
		[PXDefault(true)]
		public virtual bool? BaseCalc
		{
			get
			{
				return this._BaseCalc;
			}
			set
			{
				this._BaseCalc = value;
			}
		}
		#endregion
		#region BaseCuryID
		public abstract class baseCuryID : PX.Data.BQL.BqlString.Field<baseCuryID> { }
		protected String _BaseCuryID;

        /// <summary>
        /// Identifier of the base <see cref="Currency"/>.
        /// </summary>
        /// <value>
        /// Defaults to the <see cref="Company.BaseCuryID">base currency of the company</see>.
        /// </value>
		[PXDBString(5, IsUnicode = true)]
		[PXDefault(typeof(Search<Company.baseCuryID>))]
		[PXUIField(DisplayName = "Base Currency ID")]
		[BaseCuryID]
		public virtual String BaseCuryID
		{
			get
			{
				return this._BaseCuryID;
			}
			set
			{
				this._BaseCuryID = value;
			}
		}
		#endregion
		#region CuryID
		public abstract class curyID : PX.Data.BQL.BqlString.Field<curyID> { }
		protected String _CuryID;

        /// <summary>
        /// Identifier of the <see cref="Currency"/> of this Currency Info object.
        /// </summary>
        /// <value>
        /// Defaults to the <see cref="Company.BaseCuryID">base currency of the company</see>.
        /// </value>
		[PXDBString(5, IsUnicode = true)]
		[PXDefault(typeof(Search<Company.baseCuryID>))]
		[PXUIField(DisplayName = "Currency", ErrorHandling = PXErrorHandling.Never)]
		[PXSelector(typeof(Currency.curyID))]
		[CuryID]
		public virtual String CuryID
		{
			get
			{
				return this._CuryID;
			}
			set
			{
				this._CuryID = value;
			}
		}
		#endregion
		#region DisplayCuryID
		public abstract class displayCuryID : PX.Data.BQL.BqlString.Field<displayCuryID> { }

        /// <summary>
        /// The read-only property providing the <see cref="CuryID">Currency</see> for display in the User Interface.
        /// </summary>
		[PXString(5, IsUnicode = true)]
		[PXUIField(DisplayName = "Currency ID")]
		public virtual String DisplayCuryID
		{
			get
			{
				return this._CuryID;
			}
			set
			{
				;
			}
		}
		#endregion
		#region CuryRateTypeID
		public abstract class curyRateTypeID : PX.Data.BQL.BqlString.Field<curyRateTypeID> { }
		protected String _CuryRateTypeID;

        /// <summary>
        /// The identifier of the <see cref="CurrencyRateType">Rate Type</see> associated with this object. 
        /// </summary>
        /// <value>
        /// Corresponds to the <see cref="CurrencyRateType.CuryRateTypeID"/> field.
        /// </value>
		[PXDBString(6, IsUnicode = true)]
		[CuryRateTypeID()]
		[PXSelector(typeof(CurrencyRateType.curyRateTypeID))]
		[PXUIField(DisplayName = "Curr. Rate Type ID")]
		public virtual String CuryRateTypeID
		{
			get
			{
				return this._CuryRateTypeID;
			}
			set
			{
				this._CuryRateTypeID = value;
			}
		}
		#endregion
		#region CuryEffDate
		public abstract class curyEffDate : PX.Data.BQL.BqlDateTime.Field<curyEffDate> { }
		protected DateTime? _CuryEffDate;

        /// <summary>
        /// The date, starting from which the specified <see cref="CuryRate">rate</see> is considered current.
        /// </summary>
        /// <value>
        /// Defaults to the <see cref="AccessInfo.BusinessDate">current business date</see>.
        /// </value>
		[PXDBDate()]
		[PXDefault(typeof(AccessInfo.businessDate))]
		[PXUIField(DisplayName="Effective Date")]
		[CuryEffDate()]
		public virtual DateTime? CuryEffDate
		{
			get
			{
				return this._CuryEffDate;
			}
			set
			{
				this._CuryEffDate = value;
			}
		}
		#endregion
		#region CuryMultDiv
		public abstract class curyMultDiv : PX.Data.BQL.BqlString.Field<curyMultDiv> { }
		protected String _CuryMultDiv;

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
		[PXUIField(DisplayName="Mult Div")]
		public virtual String CuryMultDiv
		{
			get
			{
				return this._CuryMultDiv;
			}
			set
			{
				this._CuryMultDiv = value;
			}
		}
		#endregion
		#region CuryRate
		public abstract class curyRate : PX.Data.BQL.BqlDecimal.Field<curyRate> { }
		protected Decimal? _CuryRate;

        /// <summary>
        /// The currency rate. For the purposes of conversion the value of this field is used
        /// together with the operation selected in the <see cref="CuryMultDiv"/> field.
        /// </summary>
        /// <value>
        /// Defaults to <c>1.0</c>.
        /// </value>
		[PXDBDecimal(8)]
		[PXDefault(TypeCode.Decimal,"1.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Decimal? CuryRate
		{
			get
			{
				return this._CuryRate;
			}
			set
			{
				this._CuryRate = value;
			}
		}
		#endregion
		#region RecipRate
		public abstract class recipRate : PX.Data.BQL.BqlDecimal.Field<recipRate> { }
		protected Decimal? _RecipRate;

        /// <summary>
        /// The inverse of the <see cref="CuryRate">exchange rate</see>, which is calculated automatically.
        /// </summary>
		[PXDBDecimal(8)]
        [PXDefault(TypeCode.Decimal, "1.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Decimal? RecipRate
		{
			get
			{
				return this._RecipRate;
			}
			set
			{
				this._RecipRate = value;
			}
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
        [PXDefault()]
		[CuryRate()]
		public virtual Decimal? SampleCuryRate
		{
			[PXDependsOnFields(typeof(curyMultDiv),typeof(curyRate),typeof(recipRate))]
			get
			{
				return (this._CuryMultDiv == "M") ? this._CuryRate : this._RecipRate;
			}
			set
			{
				if (this.CuryMultDiv == "M")
				{
					this._CuryRate = value;
				}
				else
				{
					this._RecipRate = value;
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
		[CuryRecipRate()]
		public virtual Decimal? SampleRecipRate
		{
			[PXDependsOnFields(typeof(curyMultDiv),typeof(recipRate),typeof(curyRate))]
			get
			{
				return (this._CuryMultDiv == "M") ? this._RecipRate : this._CuryRate;
			}
			set
			{
				if (this.CuryMultDiv == "M")
				{
					this._RecipRate = value;
				}
				else
				{
					this._CuryRate = value;
				}
			}
		}
		#endregion
		#region CuryPrecision
		public abstract class curyPrecision : PX.Data.BQL.BqlShort.Field<curyPrecision>
		{
			public const short Default = 2;
		}
		protected short? _CuryPrecision;

        /// <summary>
        /// The number of digits after the decimal point used in operations with the amounts
        /// associated with this Currency Info object and expressed in its <see cref="CuryID">currency</see>.
        /// </summary>
        /// <value>
        /// The value of this field is taken from the <see cref="CurrencyList.DecimalPlaces"/> field of the record associated with the
        /// <see cref="CuryID">currency</see> of this object.
        /// </value>
        [PXShort]
		[PXDefault(curyPrecision.Default, typeof(Search<CurrencyList.decimalPlaces, Where<CurrencyList.curyID, Equal<Current<CurrencyInfo.curyID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual short? CuryPrecision
		{
			get
			{
				return _CuryPrecision;
			}
			set
			{
				_CuryPrecision = value;
			}
		}
		#endregion
		#region BasePrecision
		public abstract class basePrecision : PX.Data.BQL.BqlShort.Field<basePrecision> { }
        protected short? _BasePrecision;

        /// <summary>
        /// The number of digits after the decimal point used in operations with the amounts
        /// associated with this Currency Info object and expressed in its <see cref="BaseCuryID">base currency</see>.
        /// </summary>
        /// <value>
        /// The value of this field is taken from the <see cref="CurrencyList.DecimalPlaces"/> field of the record associated with the
        /// <see cref="BaseCuryID">base currency</see> of this object.
        /// </value>
        [PXShort]
		[PXDefault((short)2, typeof(Search<CurrencyList.decimalPlaces, Where<CurrencyList.curyID, Equal<Current<CurrencyInfo.baseCuryID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual short? BasePrecision
		{
			get
			{
				return _BasePrecision;
			}
			set
			{
				_BasePrecision = value;
			}
		}
		#endregion
		#region tstamp
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }
		protected Byte[] _tstamp;
		[PXDBTimestamp()]
		public virtual Byte[] tstamp
		{
			get
			{
				return this._tstamp;
			}
			set
			{
				this._tstamp = value;
			}
		}
		#endregion
	}

	/// <summary>
	/// <exclude />
	/// </summary>
	[PXHidden]
	[Serializable]
	public class CurrencyInfo2 : CurrencyInfo
	{
		#region CuryInfoID
		public new abstract class curyInfoID : IBqlField { }
		#endregion
		#region CuryID
		public new abstract class curyID : IBqlField { }
		#endregion
		#region CuryRateTypeID
		public new abstract class curyRateTypeID : IBqlField { }
		#endregion
		#region CuryEffDate
		public new abstract class curyEffDate : IBqlField { }
		#endregion
		#region CuryMultDiv
		public new abstract class curyMultDiv : PX.Data.BQL.BqlString.Field<curyMultDiv> { }
		#endregion
		#region CuryRate
		public new abstract class curyRate : PX.Data.BQL.BqlDecimal.Field<curyRate> { }
		#endregion
		#region BaseCuryID
		public new abstract class baseCuryID : IBqlField { }
		#endregion
		#region RecipRate
		public new abstract class recipRate : PX.Data.BQL.BqlDecimal.Field<recipRate> { }
		#endregion
		#region BaseCalc
		public new abstract class baseCalc : PX.Data.BQL.BqlBool.Field<baseCalc> { }
		#endregion
		#region tstamp
		public new abstract class tstamp : IBqlField { }
		#endregion
	}

}
