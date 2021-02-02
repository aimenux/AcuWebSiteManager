using System;
using System.Runtime.Serialization;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using PX.Data;
using PX.Data.SQLTree;
using PX.Objects.GL;
using PX.Objects.GL.DAC;
using PX.Objects.CS;
using PX.Objects.EP;
using PX.Objects.GL.Descriptor;
using PX.Objects.GL.FinPeriods.TableDefinition;
using PX.Objects.IN;
using PX.Objects.Common;
using PX.Objects.Common.Abstractions.Periods;
using PX.Objects.Common.Extensions;
using PX.Objects.FA.Descriptor;
using PX.Objects.GL.Attributes;

namespace PX.Objects.FA
{
	public class PXHashSet<T> : HashSet<T>
		where T : class, IBqlTable
	{
		public PXHashSet(PXGraph graph)
			: base(new Comparer<T>(graph))
		{
		}

		public List<T> ToList()
		{
			return new List<T>(this);
		}

		public class Comparer<TT> : IEqualityComparer<TT>
			where TT : T
		{
			protected PXCache _cache;
			public Comparer(PXGraph graph)
			{
				_cache = graph.Caches[typeof(TT)];
			}

			public bool Equals(TT a, TT b)
			{
				return _cache.ObjectsEqual(a, b);
			}

			public int GetHashCode(TT a)
			{
				return _cache.GetObjectHashCode(a);
			}
		}
	}

	public sealed class RowExt<Field> : IBqlOperand, IBqlCreator
	where Field : IBqlField
	{
		public bool AppendExpression(ref SQLExpression exp, PXGraph graph, BqlCommandInfo info, BqlCommand.Selection selection)
		{
			info.Fields?.Add(typeof(Field));
			return true;
		}

		public void Verify(PXCache cache, object item, List<object> pars, ref bool? result, ref object value)
		{
			(item as BqlFormula.ItemContainer)?.InvolvedFields.Add(typeof(Field));
			object fs = cache.GetValueExt(BqlFormula.ItemContainer.Unwrap(item), typeof(Field).Name);
			value = fs is PXFieldState ? ((PXFieldState)fs).Value : fs;
		}
	}

	public class DefaultValue<Field> : IBqlCreator, IBqlOperand
	where Field : IBqlField
	{
		#region IBqlCreator Members

		public bool AppendExpression(ref SQLExpression exp, PXGraph graph, BqlCommandInfo info, BqlCommand.Selection selection)
		{
			return false;
		}

		public void Verify(PXCache cache, object item, System.Collections.Generic.List<object> pars, ref bool? result, ref object value)
		{
			PXCache c = cache.Graph.Caches[BqlCommand.GetItemType(typeof(Field))];
			object row = null;
			if(c.GetItemType().IsAssignableFrom(cache.GetItemType()))
			{
				row = BqlFormula.ItemContainer.Unwrap(item);
			}
			c.RaiseFieldDefaulting<Field>(row, out value);
		}

		#endregion
	}


	#region PercentDBDecimalAttribute
	public class PercentDBDecimalAttribute : PXDBDecimalAttribute
	{
		protected decimal _Factor = 100m;
		protected int? _RoundPrecision;
		protected Rounding _RoundType = Rounding.Round;

		public enum Rounding
		{
			Round,
			Truncate
		}

		public double Factor
		{
			get { return (double)_Factor; }
			set { _Factor = (decimal)value; }
		}

		public int RoundPrecision
		{
			get { return _RoundPrecision ?? 4; }
			set { _RoundPrecision = value; }
		}

		public Rounding RoundType
		{
			get { return _RoundType; }
			set { _RoundType = value; }
		}

		public PercentDBDecimalAttribute()
			: base(4)
		{
			MinValue = -99999.0;
			MaxValue = 99999.0;
		}
		public PercentDBDecimalAttribute(int precision)
			: base(precision)
		{
			MinValue = -99999.0;
			MaxValue = 99999.0;
		}
		public override void FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			base.FieldUpdating(sender, e);
			if (e.NewValue != null)
			{
				switch (RoundType)
				{
					case Rounding.Round:
						e.NewValue = ((decimal)e.NewValue) / _Factor;
						if (_RoundPrecision != null)
						{
							e.NewValue = Math.Round((decimal)e.NewValue, (int)_RoundPrecision);
						}
						break;
					case Rounding.Truncate:
						int precDigs = _RoundPrecision ?? 4;
						decimal prec = (decimal)Math.Pow(10, precDigs);
						e.NewValue = Math.Truncate(prec * ((decimal)e.NewValue) / _Factor) / prec;
						break;
				}
			}
		}

		public override void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			base.FieldSelecting(sender, e);
			if (e.ReturnValue != null && (!(e.ReturnState is PXFieldState) || String.IsNullOrEmpty(((PXFieldState)e.ReturnState).Error)))
			{
				switch (RoundType)
				{
					case Rounding.Round:
						e.ReturnValue = ((decimal)e.ReturnValue) * _Factor;
						break;
					case Rounding.Truncate:
						decimal prec = (decimal)Math.Pow(10, _Precision ?? 4);
						e.ReturnValue = Math.Ceiling(prec * ((decimal)e.ReturnValue) * _Factor) / prec;
						break;
				}
			}
		}
	}
	#endregion

	#region PercentTotalDBDecimalAttribute
	public class PercentTotalDBDecimalAttribute : PercentDBDecimalAttribute
	{
		#region state
		protected Type _MapErrorTo;
		protected PXPersistingCheck _PersistingCheck = PXPersistingCheck.Null;
		public virtual PXPersistingCheck PersistingCheck
		{
			get
			{
				return _PersistingCheck;
			}
			set
			{
				_PersistingCheck = value;
			}
		}
		public virtual Type MapErrorTo
		{
			get
			{
				return _MapErrorTo;
			}
			set
			{
				_MapErrorTo = value;
			}
		}
		#endregion

		public PercentTotalDBDecimalAttribute()
		{
			MinValue = 0.0;
			MaxValue = 99999.0;
		}
		public override void RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			object val;
			if (_PersistingCheck != PXPersistingCheck.Nothing &&
				((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert ||
				(e.Operation & PXDBOperation.Command) == PXDBOperation.Update) &&
				(val = sender.GetValue(e.Row, _FieldOrdinal)) != null && (decimal)val != 1m)
			{
				val = (decimal)val * 100m;
				if (_MapErrorTo == null)
				{
					if (sender.RaiseExceptionHandling(_FieldName, e.Row, val, new PXSetPropertyException(PXMessages.LocalizeFormat(Messages.WrongValue, _FieldName))))
					{
						throw new PXRowPersistingException(_FieldName, null, Messages.WrongValue, _FieldName);
					}
				}
				else
				{
					string name = _MapErrorTo.Name;
					name = char.ToUpper(name[0]) + name.Substring(1);
					val = sender.GetValueExt(e.Row, name);
					if (val is PXFieldState)
					{
						val = ((PXFieldState)val).Value;
					}
					if (sender.RaiseExceptionHandling(name, e.Row, val, new PXSetPropertyException(PXMessages.LocalizeFormat(Messages.WrongValue, name, _FieldName))))
					{
						throw new PXRowPersistingException(_FieldName, null, Messages.WrongValue, _FieldName);
					}
				}
			}
		}
	}
	#endregion

	#region FABookPeriodIDAttribute

	public class PXFABookPeriodException : PXException
	{
		public PXFABookPeriodException() : base(Messages.NoPeriodsDefined) { }

		public PXFABookPeriodException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{

		}

	}

	public class PXFABookCalendarException : PXException
	{
		public PXFABookCalendarException() : base(Messages.NoCalendarDefined) { }

		public PXFABookCalendarException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{

		}
	}

	[Flags]
	public enum ReportParametersFlag
	{
		None = 0,
		Organization = 1,
		Branch = 2,
		BAccount = 4,
		FixedAsset = 8,
		Book = 16,
	}

	public class FABookPeriodSelectorAttribute: FABookPeriodIDAttribute, IPXFieldVerifyingSubscriber
    {
        protected int _SelAttrIndex = -1;

        public FABookPeriodSelectorAttribute(
            Type selectorSearchType = null, 
            Type searchByDateType = null,
            Type defaultType = null,
            Type bookSourceType = null,
            bool isBookRequired = true,
            Type assetSourceType = null,
            Type dateType = null,
            Type branchSourceType = null,
            Type branchSourceFormulaType = null,
            Type organizationSourceType = null,
			Type[] fieldList = null,
			ReportParametersFlag reportParametersMask = ReportParametersFlag.None)
		: base(
            searchByDateType:searchByDateType,
            defaultType: defaultType,
            bookSourceType: bookSourceType,
            isBookRequired: isBookRequired,
            assetSourceType: assetSourceType,
            dateType: dateType,
            branchSourceType: branchSourceType,
            branchSourceFormulaType: branchSourceFormulaType,
            organizationSourceType: organizationSourceType)
        {

			DefaultType = GetCompleteDefaultType(defaultType, searchByDateType, selectorSearchType, dateType);

			_Attributes.Add(new GenericFABookPeriodSelectorAttribute(
				 selectorSearchType ?? GetDefaultSelectorSearchType(),
                FABookPeriodKeyProvider,
				reportParametersMask,
                fieldList ?? GetDefaultFieldList()));

            _SelAttrIndex = _Attributes.Count - 1;
        }

        protected virtual Type GetDefaultSelectorSearchType()
        {
            return typeof(Search<FABookPeriod.finPeriodID, Where<FABookPeriod.startDate, NotEqual<FABookPeriod.endDate>>>);
        }

        protected virtual Type[] GetDefaultFieldList()
        {
            return new[] { typeof(FABookPeriod.finPeriodID), typeof(FABookPeriod.descr) };
        }

		protected virtual Type GetCompleteDefaultType(Type defaultType, Type searchByDateType, Type selectorSearchType, Type dateType)
		{
			return defaultType ?? searchByDateType ?? GetSearchTypeRestrictedByDate(selectorSearchType, dateType);
		}

		protected virtual Type GetSearchTypeRestrictedByDate(Type selectorSearchType, Type dateType)
		{
			selectorSearchType = selectorSearchType ?? GetDefaultSelectorSearchType();
			if (dateType != null)
			{
				BqlCommand select = BqlCommand.CreateInstance(selectorSearchType) ?? throw new ArgumentNullException("BqlCommand.CreateInstance(selectorSearchType)");
				Type whereDate = BqlCommand.Compose(
					typeof(Where<,,>),
						typeof(FABookPeriod.startDate),
						typeof(LessEqual<>),
							typeof(Current<>),
								dateType,
						typeof(And<,>),
							typeof(FABookPeriod.endDate),
							typeof(Greater<>),
								typeof(Current<>),
									dateType);
				select = select.WhereAnd(whereDate);
				Type[] decomposed = BqlCommand.Decompose(select.GetSelectType());
				decomposed[0] = BqlHelper.SelectToSearch[decomposed[0]];
				decomposed[1] = typeof(FABookPeriod.finPeriodID);
				selectorSearchType = BqlCommand.Compose(decomposed);
			}
			return selectorSearchType;
		}

		#region Initialization
		public override void GetSubscriber<ISubscriber>(List<ISubscriber> subscribers)
        {
            if (typeof(ISubscriber) == typeof(IPXFieldVerifyingSubscriber))
            {
                subscribers.Add(this as ISubscriber);
            }
            else
            {
                base.GetSubscriber<ISubscriber>(subscribers);
            }
        }
        #endregion

        #region Implementation
        public virtual void FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
        {
            try
            {
                if (_SelAttrIndex != -1)
                    ((IPXFieldVerifyingSubscriber)_Attributes[_SelAttrIndex]).FieldVerifying(sender, e);
            }
            catch (PXSetPropertyException)
            {
                e.NewValue = FormatForDisplay((string)e.NewValue);
                throw;
            }
        }
        #endregion
    }


	public class FABookPeriodExistingInGLSelectorAttribute : FABookPeriodSelectorAttribute
	{
		public FABookPeriodExistingInGLSelectorAttribute(
			Type searchByDateType = null,
			Type defaultType = null,
			Type bookSourceType = null,
			bool isBookRequired = true,
			Type assetSourceType = null,
			Type dateType = null,
			Type branchSourceType = null,
			Type branchSourceFormulaType = null,
			Type organizationSourceType = null,
			Type[] fieldList = null): base(
				selectorSearchType: typeof(Search2<
					FABookPeriod.finPeriodID,
					InnerJoin<FABook, On<FABookPeriod.bookID, Equal<FABook.bookID>>,
					LeftJoin<FinPeriod,
						On<FABookPeriod.organizationID, Equal<FinPeriod.organizationID>,
							And<FABookPeriod.finPeriodID, Equal<FinPeriod.finPeriodID>,
							And<FABook.updateGL, Equal<True>>>>>>,
					Where<FABookPeriod.startDate, NotEqual<FABookPeriod.endDate>,
						And<Where<FinPeriod.finPeriodID, IsNotNull,
						Or<FABook.updateGL, NotEqual<True>>>>>>),
				defaultType: defaultType,
				bookSourceType: bookSourceType,
				isBookRequired: isBookRequired,
				assetSourceType: assetSourceType,
				dateType: dateType,
				branchSourceType: branchSourceType,
				branchSourceFormulaType: branchSourceFormulaType,
				organizationSourceType: organizationSourceType)
		{ }
	}

	public class FABookPeriodOpenInGLSelectorAttribute : FABookPeriodSelectorAttribute
	{
		public FABookPeriodOpenInGLSelectorAttribute(
			Type searchByDateType = null,
			Type defaultType = null,
			Type bookSourceType = null,
			bool isBookRequired = true,
			Type assetSourceType = null,
			Type dateType = null,
			Type branchSourceType = null,
			Type branchSourceFormulaType = null,
			Type organizationSourceType = null,
			Type[] fieldList = null) : base(
				selectorSearchType: typeof(Search2<
					FABookPeriod.finPeriodID,
					InnerJoin<FABook, On<FABookPeriod.bookID, Equal<FABook.bookID>>,
					LeftJoin<FinPeriod,
						On<FABookPeriod.organizationID, Equal<FinPeriod.organizationID>,
							And<FABookPeriod.finPeriodID, Equal<FinPeriod.finPeriodID>,
							And<FABook.updateGL, Equal<True>>>>>>,
					Where<FABookPeriod.startDate, NotEqual<FABookPeriod.endDate>,
						And<Where<FinPeriod.finPeriodID, IsNotNull,
						And<FinPeriod.fAClosed, NotEqual<True>,
							Or<FABook.updateGL, NotEqual<True>>>>>>>),
				defaultType: defaultType,
				bookSourceType: bookSourceType,
				isBookRequired: isBookRequired,
				assetSourceType: assetSourceType,
				dateType: dateType,
				branchSourceType: branchSourceType,
				branchSourceFormulaType: branchSourceFormulaType,
				organizationSourceType: organizationSourceType)
		{ }
	}

	public class FABookPeriodIDAttribute : OrganizationDependedPeriodIDAttribute
	{
	    public FABookPeriodKeyProvider FABookPeriodKeyProvider { get; set; }

	    public FABookPeriodKeyProvider.FASourceSpecificationItem FAMainSpecificationItem => 
	        (FABookPeriodKeyProvider.FASourceSpecificationItem) MainSpecificationItem;

	    #region Ctor & Setup

        public FABookPeriodIDAttribute(
	        Type bookSourceType = null,
	        Type dateType = null,
	        bool isBookRequired = true,
	        Type assetSourceType = null,        
	        Type branchSourceType = null,
	        Type branchSourceFormulaType = null,
	        Type organizationSourceType = null,
	        Type searchByDateType = null,
	        Type defaultType = null)
	        : base(
	            dateType: dateType,
	            searchByDateType: searchByDateType ?? typeof(Search<FABookPeriod.finPeriodID,
	                                  Where<FABookPeriod.startDate, LessEqual<Current2<QueryParams.sourceDate>>,
	                                      And<FABookPeriod.endDate, Greater<Current2<QueryParams.sourceDate>>>>>),
	            defaultType: defaultType
	        )
	    {
            PeriodKeyProvider =
	        FABookPeriodKeyProvider = new FABookPeriodKeyProvider(
	            new PeriodKeyProviderBase.SourcesSpecificationCollection<
	                FABookPeriodKeyProvider.FASourceSpecificationItem>()
	            {
	                SpecificationItems = new FABookPeriodKeyProvider.FASourceSpecificationItem()
	                {
	                    AssetSourceType = assetSourceType,
	                    BookSourceType = bookSourceType,
	                    IsBookRequired = isBookRequired,
	                    BranchSourceType = branchSourceType,
	                    BranchSourceFormulaType = branchSourceFormulaType,
	                    OrganizationSourceType = organizationSourceType,
	                }.SingleToList()
	            });
	    }

		protected override bool ShouldExecuteRedefaultFinPeriodIDonRowUpdated(
			object errorValue, 
			bool hasError,
			OrganizationDependedPeriodKey newPeriodKey, 
			OrganizationDependedPeriodKey oldPeriodKey)
		{
			var newKey = (FABookPeriod.Key)newPeriodKey;
			var oldKey = (FABookPeriod.Key)oldPeriodKey;

			return base.ShouldExecuteRedefaultFinPeriodIDonRowUpdated(errorValue, hasError, newKey, oldKey)
				|| oldKey.BookID != newKey.BookID;
		}


		protected override Type GetQueryWithRestrictionByOrganization(Type bqlQueryType)
	    {
	        return BqlCommand.CreateInstance(bqlQueryType)
	            .WhereAnd(typeof(Where<FABookPeriod.organizationID, Equal<Required<FABookPeriod.organizationID>>,
	            And<FABookPeriod.bookID, Equal<Required<FABookPeriod.bookID>>>>))
	            .GetType();
	    }

	    protected override void ValidatePeriodAndSourcesImpl(PXCache cache, object oldRow, object newRow, bool externalCall)
	    {
	        PeriodKeyProviderBase.KeyWithSourceValuesCollection<
	            FABookPeriodKeyProvider.FAKeyWithSourceValues,
	            FABookPeriodKeyProvider.FASourceSpecificationItem,
	            FABookPeriod.Key> newKeyWithSourceValues =
	            FABookPeriodKeyProvider.GetKeys(cache.Graph, cache, newRow);

	        PeriodKeyProviderBase.KeyWithSourceValuesCollection<
	            FABookPeriodKeyProvider.FAKeyWithSourceValues,
	            FABookPeriodKeyProvider.FASourceSpecificationItem,
	            FABookPeriod.Key> oldKeyWithSourceValues =
	            FABookPeriodKeyProvider.GetKeys(cache.Graph, cache, oldRow);

            FABookPeriod.Key newPeriodKey = newKeyWithSourceValues.ConsolidatedKey;

            newPeriodKey.PeriodID = (string)cache.GetValue(newRow, _FieldName);

	        if (!newPeriodKey.Defined)
	            return;

	        IFABookPeriodRepository periodRepository = cache.Graph.GetService<IFABookPeriodRepository>();

	        FABookPeriod period = periodRepository.FindByKey(newPeriodKey.BookID, newPeriodKey.OrganizationID,
	            newPeriodKey.PeriodID);

	        if (period == null)
	        {
                PXSetPropertyException exception = null;

	            FABook book = BookMaint.FindByID(cache.Graph, newPeriodKey.BookID);

	            if (book.UpdateGL == true)
	            {
	                exception = new PXSetPropertyException(PXMessages.LocalizeFormatNoPrefix(
	                    Messages.PeriodDoesNotExistForBookAndCompany,
	                    FormatForError(newPeriodKey.PeriodID),
	                    book.BookCode,
	                    PXAccess.GetOrganizationCD(newPeriodKey.OrganizationID)));

	                if (FAMainSpecificationItem.OrganizationSourceType != null
	                    && newKeyWithSourceValues.MainItem.SourceOrganizationIDs.First() != null
	                    && newKeyWithSourceValues.MainItem.SourceOrganizationIDs.First() != oldKeyWithSourceValues.MainItem.SourceOrganizationIDs.First())
	                {
	                    SetErrorAndResetToOldForField(
	                        cache,
	                        oldRow,
	                        newRow,
	                        FAMainSpecificationItem.OrganizationSourceType.Name,
	                        exception,
	                        externalCall);
	                }

	                if (FAMainSpecificationItem.BranchSourceType != null
	                    && newKeyWithSourceValues.MainItem.SourceBranchIDs.First() != null
	                    && newKeyWithSourceValues.MainItem.SourceBranchIDs.First() != oldKeyWithSourceValues.MainItem.SourceBranchIDs.First())
	                {
	                    SetErrorAndResetToOldForField(
	                        cache,
	                        oldRow,
	                        newRow,
	                        FAMainSpecificationItem.BranchSourceType.Name,
	                        exception,
	                        externalCall);
	                }

	                if (FAMainSpecificationItem.AssetSourceType != null
	                    && newKeyWithSourceValues.MainItem.SourceAssetIDs.First() != null
	                    && newKeyWithSourceValues.MainItem.SourceAssetIDs.First() != oldKeyWithSourceValues.MainItem.SourceAssetIDs.First())
	                {
	                    SetErrorAndResetToOldForField(
	                        cache,
	                        oldRow,
	                        newRow,
	                        FAMainSpecificationItem.AssetSourceType.Name,
	                        exception,
	                        externalCall);
	                }
                }
	            else
	            {
                    exception = new PXSetPropertyException(PXMessages.LocalizeFormatNoPrefix(
	                    Messages.PeriodDoesNotExistForBook,
	                    FormatForError(newPeriodKey.PeriodID)));
                }

                cache.RaiseExceptionHandling(
	                _FieldName,
	                newRow,
	                FormatForDisplay(newPeriodKey.PeriodID),
	                exception);

	            cache.SetValue(
	                newRow,
	                _FieldName,
	                cache.GetValue(oldRow, _FieldName));

	            if (FAMainSpecificationItem.BookSourceType != null
	                && newKeyWithSourceValues.MainItem.SourceBookIDs.First() != null
	                && newKeyWithSourceValues.MainItem.SourceBookIDs.First() != oldKeyWithSourceValues.MainItem.SourceBookIDs.First())
	            {
	                SetErrorAndResetToOldForField(
	                    cache,
	                    oldRow,
	                    newRow,
	                    FAMainSpecificationItem.BookSourceType.Name,
	                    exception,
	                    externalCall);
	            }
            }
        }

	    protected override string GetMappedPeriodID(PXCache cache, OrganizationDependedPeriodKey newPeriodKey,
	        OrganizationDependedPeriodKey oldPeriodKey)
	    {
	        IFABookPeriodRepository periodRepository = cache.Graph.GetService<IFABookPeriodRepository>();

	        FABookPeriod mappedPeriod = periodRepository.FindMappedPeriod((FABookPeriod.Key)oldPeriodKey, (FABookPeriod.Key)newPeriodKey);

	        return mappedPeriod?.FinPeriodID;
        }

	    #endregion
	}
	#endregion

	#region SubAccountMaskAttribute
	public class FAAcctSubDefault
	{
		public class CustomListAttribute : PXStringListAttribute
		{
			public string[] AllowedValues => _AllowedValues;
			public string[] AllowedLabels => _AllowedLabels;

			public CustomListAttribute(string[] AllowedValues, string[] AllowedLabels): base(AllowedValues, AllowedLabels) {}
		}

		public class ClassListAttribute : CustomListAttribute
		{
			public ClassListAttribute()
				: base(new[] { MaskAsset, MaskLocation, MaskDepartment, MaskClass },
						new[] { Messages.MaskAsset, Messages.MaskLocation, Messages.MaskDepartment, Messages.MaskClass }) { }
		}

		public const string MaskAsset = "A";
		public const string MaskLocation = "L";
		public const string MaskDepartment = "D";
		public const string MaskClass = "C";
	}

	/// <summary>
	/// A subaccount mask that is used to generate the subaccounts of the fixed asset.
	/// </summary>
	/// <value>The string mask for the SUBACCOUNT segmented key with the appropriate structure.
	/// The mask can contain the following mask symbols:
	/// <list type="bullet">
	/// <item> <term><c>A</c></term> <description>The subaccount source is a fixed asset, <see cref="FixedAsset.DisposalSubID"/></description> </item>
	/// <item> <term><c>C</c></term> <description>The subaccount source is a fixed asset class, <see cref="FAClass.DisposalSubID"/></description> </item>
	/// <item> <term><c>L</c></term> <description>The subaccount source is a location, <see cref="Location.CMPExpenseSubID"/></description> </item>
	/// <item> <term><c>D</c></term> <description>The subaccount source is a department, <see cref="EPDepartment.ExpenseSubID"/></description> </item>
	///</list>
	/// By default, the mask contains only "C" symbols (which means that the only subaccount source is a fixed asset class).
	/// </value>
	[PXDBString(30, IsUnicode = true, InputMask = "")]
	[PXUIField(DisplayName = "Subaccount Mask", Visibility = PXUIVisibility.Visible, FieldClass = _DimensionName)]
	public sealed class SubAccountMaskAttribute : AcctSubAttribute
	{
		private const string _DimensionName = "SUBACCOUNT";
		private const string _MaskName = "FASETUP";
		public SubAccountMaskAttribute()
		{
			PXDimensionMaskAttribute attr = new PXDimensionMaskAttribute(
				_DimensionName, 
				_MaskName, 
				FAAcctSubDefault.MaskClass,
				new FAAcctSubDefault.ClassListAttribute().AllowedValues, 
				new FAAcctSubDefault.ClassListAttribute().AllowedLabels)
			{
				ValidComboRequired = false
			};
			_Attributes.Add(attr);
			_SelAttrIndex = _Attributes.Count - 1;
		}

		public static string MakeSub<Field>(PXGraph graph, string mask, object[] sources, Type[] fields)
			where Field : IBqlField
		{
			try
			{
				return PXDimensionMaskAttribute.MakeSub<Field>(graph, mask, new FAAcctSubDefault.ClassListAttribute().AllowedValues, 3, sources);
			}
			catch (PXMaskArgumentException)
			{
				// default source subID is null
				return null;
			}
		}
	}

	#endregion

	#region RecoveryStartPeriod
	public class RecoveryStartPeriod<StartDate, BookID, DepreciationMethodID, AveragingConvention, MidMonthType, MidMonthDay> : BqlFormulaEvaluator<StartDate, BookID, DepreciationMethodID, AveragingConvention, MidMonthType, MidMonthDay>
		where StartDate : IBqlOperand
		where BookID : IBqlOperand
		where DepreciationMethodID : IBqlOperand
		where AveragingConvention : IBqlOperand
		where MidMonthType : IBqlOperand
		where MidMonthDay : IBqlOperand
	{
		public override object Evaluate(PXCache cache, object item, Dictionary<Type, object> pars)
		{
			try
			{
				return PeriodIDAttribute.FormatForDisplay(DeprCalcParameters.GetRecoveryStartPeriod(cache.Graph, (FABookBalance)item));
			}
			catch (PXException ex)
			{
				throw new PXSetPropertyException(ex.Message, PXErrorLevel.Error);
			}
			catch
			{
				return null;
			}
		}
	}
	#endregion

	#region OffsetBookDate
	public class OffsetBookDate<BookDate, BookID, AssetID, DepreciationMethodID, AveragingConvention, UsefulLife>
		: BqlFormulaEvaluator<BookDate, BookID, AssetID, DepreciationMethodID, AveragingConvention, UsefulLife>
		where BookDate : IBqlOperand
		where BookID : IBqlOperand
		where AssetID : IBqlOperand
		where DepreciationMethodID : IBqlOperand
		where AveragingConvention : IBqlOperand
		where UsefulLife : IBqlOperand
	{
		public override object Evaluate(PXCache cache, object item, Dictionary<Type, object> pars)
		{
			try
			{
				return DeprCalcParameters.GetRecoveryEndDate(cache.Graph, (FABookBalance)item);
			}
			catch (PXException ex)
			{
				throw new PXSetPropertyException(ex.Message, PXErrorLevel.Error);
			}
			catch
			{
				return null;
			}
		}
	}
	#endregion

	#region OffsetBookDateToPeriod
	public class OffsetBookDateToPeriod<BookDate, BookID, AssetID, DepreciationMethodID, AveragingConvention, UsefulLife>
		: OffsetBookDate<BookDate, BookID, AssetID, DepreciationMethodID, AveragingConvention, UsefulLife>, IBqlOperand
		where BookDate : IBqlOperand
		where BookID : IBqlOperand
		where AssetID : IBqlOperand
		where DepreciationMethodID : IBqlOperand
		where AveragingConvention : IBqlOperand
		where UsefulLife : IBqlOperand
	{
		public override object Evaluate(PXCache cache, object item, Dictionary<Type, object> pars)
		{
			object val = base.Evaluate(cache, item, pars);

			if (val == null)
			{
				return null;
			}

			DateTime? recoveryEndDate = (DateTime?)val;
			int? bookID = (int?)pars[typeof(BookID)];
			int? assetID = (int?)pars[typeof(AssetID)];
			return FABookPeriodIDAttribute.FormatForDisplay(cache.Graph.GetService<IFABookPeriodRepository>().GetFABookPeriodIDOfDate(recoveryEndDate, bookID, assetID, false));
		}
	}
	#endregion

	#region GetBonusRate
	public class GetBonusRate<Date, BonusID> : BqlFormulaEvaluator<Date, BonusID>
		where Date : IBqlOperand
		where BonusID : IBqlOperand
	{
		public override object Evaluate(PXCache cache, object item, Dictionary<Type, object> pars)
		{
			DateTime? date = (DateTime?)pars[typeof(Date)];
			int? bonusID = (int?)pars[typeof(BonusID)];

			if (bonusID == null || date == null)
			{
				return 0m;
			}

			FABonusDetails det = PXSelect<FABonusDetails, Where<FABonusDetails.bonusID, Equal<Required<FABonus.bonusID>>,
				And<FABonusDetails.startDate, LessEqual<Required<FABookBalance.deprFromDate>>,
				And<FABonusDetails.endDate, GreaterEqual<Required<FABookBalance.deprFromDate>>>>>>.Select(cache.Graph, bonusID, date, date);

			return det == null ? 0m : det.BonusPercent;
		}

	}
	#endregion

	#region FA Reports
	[Serializable]
	public partial class FABookPeriodSelection : IBqlTable
	{
		#region GLBookCD
		public abstract class gLBookCD : PX.Data.BQL.BqlString.Field<gLBookCD> { }
		protected string _GLBookCD;
		[PXString]
		[GLBookDefault]
		public virtual string GLBookCD
		{
			get
			{
				return _GLBookCD;
			}
			set
			{
				_GLBookCD = value;
			}
		}
		#endregion
		#region CurPeriodID
		public abstract class curPeriodID : PX.Data.BQL.BqlString.Field<curPeriodID> { }
		protected string _CurPeriodID;
		[PXString]
		[CurrentGLBookPeriodDefault]
		public virtual string CurPeriodID
		{
			get
			{
				return _CurPeriodID;
			}
			set
			{
				_CurPeriodID = value;
			}
		}
		#endregion
	}

	public class GLBookDefaultAttribute : PXDefaultAttribute
	{
		public override void FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			base.FieldDefaulting(sender, e);

			FABook book = PXSelect<FABook, Where<FABook.updateGL, Equal<True>>>.SelectSingleBound(sender.Graph, new object[0]);

			if (book != null)
			{
				e.NewValue = book.BookCode;
			}
		}
	}

	public class CurrentGLBookPeriodDefaultAttribute : PXDefaultAttribute
	{
		public override void FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			base.FieldDefaulting(sender, e);

			FABookPeriod period = PXSelectJoin<
				FABookPeriod, 
				LeftJoin<FABook, 
					On<FABookPeriod.bookID, Equal<FABook.bookID>>>,
				Where<FABookPeriod.startDate, LessEqual<Current<AccessInfo.businessDate>>, 
					And<FABookPeriod.endDate, Greater<Current<AccessInfo.businessDate>>, 
					And<FABook.updateGL, Equal<True>,
					And<FABookPeriod.organizationID, Equal<FinPeriod.organizationID.masterValue>>>>>>
				.SelectSingleBound(sender.Graph, new object[0]);

			if (period != null)
			{
				e.NewValue = FinPeriodIDFormattingAttribute.FormatForDisplay(period.FinPeriodID);
			}
		}
	}

	public class FABookPeriodReportParameters : IBqlTable
	{
		#region OrganizationID
		public abstract class organizationID : PX.Data.BQL.BqlInt.Field<organizationID> { }

		[Organization(
			false,
			typeof(Search2<Organization.organizationID,
				InnerJoin<Branch,
					On<Organization.organizationID, Equal<Branch.organizationID>>>,
				Where<Branch.branchID, Equal<Current<AccessInfo.branchID>>,
					And2<FeatureInstalled<FeaturesSet.branch>,
					And<MatchWithBranch<Branch.branchID>>>>>))]
		public int? OrganizationID { get; set; }
		#endregion

		#region BranchID
		public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }

		[BranchOfOrganization(
			typeof(organizationID),
			onlyActive: false,
			sourceType: typeof(Search2<Branch.branchID,
				InnerJoin<Organization,
					On<Branch.organizationID, Equal<Organization.organizationID>>,
				CrossJoin<FeaturesSet>>,
				Where<FeaturesSet.branch, Equal<True>,
					And<Organization.organizationType, NotEqual<OrganizationTypes.withoutBranches>,
					And<Branch.branchID, Equal<Current<AccessInfo.branchID>>>>>>))]
		public int? BranchID { get; set; }
		#endregion

		#region OrgBAccountID
		public abstract class orgBAccountID : PX.Data.BQL.BqlInt.Field<orgBAccountID> { }

		[OrganizationTree(typeof(organizationID), typeof(branchID), onlyActive: false)]
		public int? OrgBAccountID { get; set; }
		#endregion

		#region BookID
		public abstract class bookID : PX.Data.BQL.BqlInt.Field<bookID> { }
		[PXDBInt]
		[GLBookDefault]
		[PXSelector(typeof(FABook.bookID), SubstituteKey = typeof(FABook.bookCode), DescriptionField = typeof(FABook.description))]
		[PXUIField(DisplayName = "Book")]
		public virtual int? BookID
		{
			get;
			set;
		}
		#endregion

		#region CurentFABookPeriodIDByOrganizationBranchBook
		public abstract class currentFABookPeriodIDByOrganizationBranchBook : PX.Data.BQL.BqlString.Field<currentFABookPeriodIDByOrganizationBranchBook> { }

		[PXDBString]
		[FABookPeriodSelector
			(reportParametersMask: 
				ReportParametersFlag.Organization |
				ReportParametersFlag.Branch |
				ReportParametersFlag.Book,
			dateType: typeof(AccessInfo.businessDate),
			organizationSourceType: typeof(organizationID),
			branchSourceType: typeof(branchID),
			bookSourceType: typeof(bookID),
			isBookRequired: false)]
		public virtual string CurrentFABookPeriodIDByOrganizationBranchBook
		{
			get;
			set;
		}
		#endregion
		#region CurentFABookPeriodIDByOrganizationBook
		public abstract class currentFABookPeriodIDByOrganizationBook : PX.Data.BQL.BqlString.Field<currentFABookPeriodIDByOrganizationBook> { }

		[PXDBString]
		[FABookPeriodSelector
			(reportParametersMask:
				ReportParametersFlag.Organization |
				ReportParametersFlag.Book,
			dateType: typeof(AccessInfo.businessDate),
			organizationSourceType: typeof(organizationID),
			bookSourceType: typeof(bookID),
			isBookRequired: false)]
		public virtual string CurrentFABookPeriodIDByOrganizationBook
		{
			get;
			set;
		}
		#endregion
		#region CurentFABookPeriodIDByBranchBook
		public abstract class currentFABookPeriodIDByBranchBook : PX.Data.BQL.BqlString.Field<currentFABookPeriodIDByBranchBook> { }

		[PXDBString]
		[FABookPeriodSelector
			(reportParametersMask:
				ReportParametersFlag.Branch |
				ReportParametersFlag.Book,
			dateType: typeof(AccessInfo.businessDate),
			branchSourceType: typeof(branchID),
			bookSourceType: typeof(bookID),
			isBookRequired: false)]
		public virtual string CurrentFABookPeriodIDByBranchBook
		{
			get;
			set;
		}
		#endregion

		#region CurrentFABookPeriodIDByBAccountBook
		public abstract class currentFABookPeriodIDByBAccountBook : PX.Data.BQL.BqlString.Field<currentFABookPeriodIDByBAccountBook> { }

		[PXDBString]
		[FABookPeriodSelector
			(reportParametersMask:
				ReportParametersFlag.BAccount |
				ReportParametersFlag.Book,
			dateType: typeof(AccessInfo.businessDate),
			branchSourceType: typeof(branchID),
			bookSourceType: typeof(bookID),
			isBookRequired: false)]
		public virtual string CurrentFABookPeriodIDByBAccountBook
		{
			get;
			set;
		}
		#endregion
	}
	#endregion

}
