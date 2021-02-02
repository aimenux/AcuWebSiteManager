using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Serialization;

using PX.Common;
using PX.Data;
using PX.Objects.Common;
using PX.Objects.AR;
using PX.Objects.AP;
using PX.Objects.CS;
using PX.Objects.CM;
using PX.Objects.CA;
using PX.Objects.Common.Abstractions.Periods;
using PX.Objects.Common.Bql;
using PX.Objects.Common.Extensions;
using PX.Objects.Common.GraphExtensions;
using PX.Objects.Common.GraphExtensions.Abstract;
using PX.Objects.CR;
using PX.Objects.GL.DAC;
using PX.Objects.GL.FinPeriods.TableDefinition;
using PX.Objects.GL.Descriptor;
using PX.Objects.GL.Exceptions;
using PX.Objects.GL.FinPeriods;
using PX.Objects.TX;

namespace PX.Objects.GL
{
    public class DrCr
    {
        public const string Debit = "D";
        public const string Credit = "C";

		public class debit : PX.Data.BQL.BqlString.Constant<debit>
		{
			public debit()
				: base(Debit) {  }
		}

		public class credit : PX.Data.BQL.BqlString.Constant<credit>
		{
			public credit()
				: base(Credit) {  }
		}
	}

    public class TranDateOutOfRangeException : PXSetPropertyException
    {
        public TranDateOutOfRangeException(DateTime? date, string organizationCD)
            : base(Messages.TranDateOutOfRange, date?.ToShortDateString(), organizationCD)
        {
        }


        public TranDateOutOfRangeException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            PXReflectionSerializer.RestoreObjectProps(this, info);
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            PXReflectionSerializer.GetObjectData(this, info);
            base.GetObjectData(info, context);
        }
    }

    public class FiscalPeriodClosedException : PXSetPropertyException
    {
        public FiscalPeriodClosedException(string message, PXErrorLevel errorLevel = PXErrorLevel.Error)
            : base(message, errorLevel)
        {
        }

        public FiscalPeriodClosedException(string finPeriodID, string organizationCD)
            : this(finPeriodID, organizationCD, PXErrorLevel.Error)
        {
        }

        public FiscalPeriodClosedException(string finPeriodID, string organizationCD, PXErrorLevel errorLevel)
            : this(finPeriodID, organizationCD, errorLevel, Messages.FinPeriodIsClosedInCompany)
        {
        }

        public FiscalPeriodClosedException(string finPeriodID, string organizationCD, string errorMessageFormat)
            : this(finPeriodID, organizationCD, PXErrorLevel.Error, errorMessageFormat)
        {
        }

        public FiscalPeriodClosedException(string finPeriodID, string organizationCD, PXErrorLevel errorLevel,
            string errorMessageFormat)
            : base(errorMessageFormat, errorLevel, FinPeriodIDAttribute.FormatForError(finPeriodID), organizationCD)
        {
        }

        public FiscalPeriodClosedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            PXReflectionSerializer.RestoreObjectProps(this, info);
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            PXReflectionSerializer.GetObjectData(this, info);
            base.GetObjectData(info, context);
        }
    }

    public class FiscalPeriodInactiveException : PXSetPropertyException
    {
        public FiscalPeriodInactiveException(string message, PXErrorLevel errorLevel = PXErrorLevel.Error)
            : base(message, errorLevel)
        {
        }

        public FiscalPeriodInactiveException(string finPeriodID, string organizationCD)
            : this(finPeriodID, organizationCD, PXErrorLevel.Error)
        {
        }

        public FiscalPeriodInactiveException(string finPeriodID, string organizationCD, PXErrorLevel errorLevel)
            : base(Messages.FinPeriodIsInactiveInCompany, errorLevel, FinPeriodIDAttribute.FormatForError(finPeriodID),
                organizationCD)
        {
        }


        public FiscalPeriodInactiveException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            PXReflectionSerializer.RestoreObjectProps(this, info);
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            PXReflectionSerializer.GetObjectData(this, info);
            base.GetObjectData(info, context);
        }
    }

    public class FiscalPeriodLockedException : PXSetPropertyException
    {
        public FiscalPeriodLockedException(string message, PXErrorLevel errorLevel = PXErrorLevel.Error)
            : base(message, errorLevel)
        {
        }

        public FiscalPeriodLockedException(string finPeriodID, string organizationCD)
            : this(finPeriodID, organizationCD, PXErrorLevel.Error)
        {
        }

        public FiscalPeriodLockedException(string finPeriodID, string organizationCD, PXErrorLevel errorLevel)
            : base(Messages.FinPeriodIsLockedInCompany, errorLevel, FinPeriodIDAttribute.FormatForError(finPeriodID),
                organizationCD)
        {
        }


        public FiscalPeriodLockedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            PXReflectionSerializer.RestoreObjectProps(this, info);
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            PXReflectionSerializer.GetObjectData(this, info);
            base.GetObjectData(info, context);
        }
    }

    public class FiscalPeriodInvalidException : PXSetPropertyException
    {
        public FiscalPeriodInvalidException(string FinPeriodID)
            : this(FinPeriodID, PXErrorLevel.Error)
        {
        }

        public FiscalPeriodInvalidException(string FinPeriodID, PXErrorLevel errorLevel)
            : base(Messages.FiscalPeriodInvalid, errorLevel, FinPeriodIDAttribute.FormatForError(FinPeriodID))
        {
        }


        public FiscalPeriodInvalidException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            PXReflectionSerializer.RestoreObjectProps(this, info);
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            PXReflectionSerializer.GetObjectData(this, info);
            base.GetObjectData(info, context);
        }
    }

    /// <summary>
    /// Implements Formatting for FinPeriod field.
    /// FinPeriod is stored and dispalyed as a string but in different formats. 
    /// This Attribute handles the conversion of one format into another.
    /// </summary>
    public class FinPeriodIDFormattingAttribute : PXEventSubscriberAttribute, IPXFieldSelectingSubscriber,
        IPXFieldUpdatingSubscriber
    {
        protected const string CS_DISPLAY_MASK = "##-####";

		public string DisplayMask { get; set; } = CS_DISPLAY_MASK;

		#region Implementation

		public virtual void FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
        {
            e.NewValue = FormatForStoring(e.NewValue as string);
        }

        public virtual void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
        {
            string perPost = e.ReturnValue as string;
            if (perPost != null &&
                (e.Row == null || object.Equals(e.ReturnValue, sender.GetValue(e.Row, _FieldOrdinal))))
            {
                e.ReturnValue = FormatForDisplay(perPost);
            }

            if (_AttributeLevel == PXAttributeLevel.Item || e.IsAltered)
            {
                e.ReturnState = PXStringState.CreateInstance(e.ReturnState, FinPeriodUtils.FULL_LENGHT, null,
                    _FieldName, null, null, DisplayMask, null, null, null, null);
            }
        }

		#endregion

		public static string FormatForError(string period)
		{
			return FormatForError(period, CS_DISPLAY_MASK);
		}

		public static string FormatForError(string period, string displayMask)
        {
            return Mask.Format(displayMask, FormatForDisplay(period));
        }

        protected static string FixedLength(string period)
        {
            if (period == null)
            {
                return null;
            }
            else if (period.Length >= FinPeriodUtils.FULL_LENGHT)
            {
                return period.Substring(0, FinPeriodUtils.FULL_LENGHT);
            }
            else
            {
                return period.PadRight(FinPeriodUtils.FULL_LENGHT);
            }
        }

        public static string FormatForDisplay(string period)
        {
            return FormatForDisplayInt(FixedLength(period));
        }

        protected static string FormatForDisplayInt(string period)
        {
            return string.IsNullOrEmpty(period)
                ? null
                : $"{FinPeriodUtils.PeriodInYear(period)}{FinPeriodUtils.FiscalYear(period)}";
        }

        public static string FormatForStoring(string period)
        {
            return FormatForStoringInt(FixedLength(period));
        }

        public static string FormatForStoringNoTrim(string period)
        {
            period = FixedLength(period);
            return string.IsNullOrEmpty(period)
                ? null
                : $"{period.Substring(FinPeriodUtils.PERIOD_LENGTH, FinPeriodUtils.YEAR_LENGTH)}{period.Substring(0, FinPeriodUtils.PERIOD_LENGTH)}";
        }

        protected static string FormatForStoringInt(string period)
        {
            string substr = period?.Trim();
            if (!string.IsNullOrEmpty(substr) && substr.Length < 6)
                return substr;
            else
                return string.IsNullOrEmpty(period)
                    ? null
                    : $"{period.Substring(FinPeriodUtils.PERIOD_LENGTH, FinPeriodUtils.YEAR_LENGTH)}{period.Substring(0, FinPeriodUtils.PERIOD_LENGTH)}";
        }

        protected static string FormatPeriod(string period)
        {
            return FormatForDisplay(period);
        }

        protected static string UnFormatPeriod(string period)
        {
            return FormatForStoring(period);
        }
    }

    public class PXFinPeriodException : PXException
    {
        public PXFinPeriodException()
            : base(Messages.NoPeriodsDefined)
        {
        }

        public PXFinPeriodException(string message)
            : base(message)
        {
        }

        public PXFinPeriodException(string format, params object[] args)
            : base(format, args)
        {
        }

        public PXFinPeriodException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            PXReflectionSerializer.RestoreObjectProps(this, info);
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            PXReflectionSerializer.GetObjectData(this, info);
            base.GetObjectData(info, context);
        }
    }

    public class PeriodIDAttribute : PXAggregateAttribute, IPXCommandPreparingSubscriber, IPXRowSelectingSubscriber
    {
        #region Types

        public class PeriodResult
        {
            public string PeriodID;
            public DateTime? StartDate;
            public DateTime? EndDate;
            public static implicit operator string(PeriodResult p)
            {
                return p.PeriodID;
            }
        }

        #endregion

        #region State

        /// <param name="object[]">External currents. First item - e.Row</param>
        /// <param name="DateTime?">Source date</param>
        /// <param name="int?">Calendar OrganizationID</param>
        /// <param name="object[]">Result collection</param>
        public Func<QueryParams, object[], List<object>, object[]> DefaultingQueryParametersDelegate { get; set; }

        protected Type _sourceType;

        protected Type _sourceFieldType;
        public Type SourceFieldType
        {
            get { return _sourceFieldType; }
            set
            {
                _sourceFieldType = value;

                _sourceType = BqlCommand.GetItemType(_sourceFieldType);
                _sourceField = _sourceFieldType.Name;
            }
        }

        public Type SourceType
        {
            get
            {
                return _sourceType;
            }
            set
            {
                _sourceType = value;
            }
        }

        protected Type _searchType;
        protected string _sourceField;
        protected DateTime _sourceDate;

        public virtual Type SearchType
        {
            get { return _searchType; }
            set { _searchType = value; }
        }

        protected Type _defaultType;

        public virtual Type DefaultType
        {
            get { return _defaultType; }
            set { _defaultType = value; }
        }

        protected bool _IsDBField = true;
        public bool IsDBField
        {
            get
            {
                return this._IsDBField;
            }
            set
            {
                this._IsDBField = value;
            }
        }

        public bool RedefaultOnDateChanged { get; set; }

        #region Proxy fields

        public bool IsKey
        {
            get
            {
                return ((PXDBStringAttribute)_Attributes[0]).IsKey;
            }
            set
            {
                ((PXDBStringAttribute)_Attributes[0]).IsKey = value;
            }
        }

        public new string FieldName
        {
            get
            {
                return ((PXDBStringAttribute)_Attributes[0]).FieldName;
            }
            set
            {
                ((PXDBStringAttribute)_Attributes[0]).FieldName = value;
            }
        }

        public Type BqlField
        {
            get
            {
                return ((PXDBFieldAttribute)_Attributes[0]).BqlField;
            }
            set
            {
                ((PXDBFieldAttribute)_Attributes[0]).BqlField = value;
                BqlTable = ((PXDBFieldAttribute)_Attributes[0]).BqlTable;
            }
        }

		public string DisplayMask
		{
			get
			{
				return _Attributes.OfType<FinPeriodIDFormattingAttribute>().FirstOrDefault()?.DisplayMask;
			}
			set
			{
				FinPeriodIDFormattingAttribute attr = _Attributes.OfType<FinPeriodIDFormattingAttribute>().FirstOrDefault();
				if (attr != null)
				{
					attr.DisplayMask = value;
				}
			}
		}

        #endregion

        #endregion

        #region Ctor & Setup

        public PeriodIDAttribute(
            Type sourceType = null,
            Type searchType = null,
            Type defaultType = null,
            bool redefaultOnDateChanged = true)
        {
            RedefaultOnDateChanged = redefaultOnDateChanged;

            if (sourceType != null)
            {
                _sourceType = BqlCommand.GetItemType(sourceType);

                SearchType = searchType;

                SourceFieldType = sourceType;
            }

            DefaultType = defaultType ?? searchType;

            _Attributes = new AggregatedAttributesCollection
            {
                new PXDBStringAttribute(FinPeriodUtils.FULL_LENGHT) {IsFixed = true},
                new FinPeriodIDFormattingAttribute()
            };
        }

        public override void GetSubscriber<ISubscriber>(List<ISubscriber> subscibers)
        {
            if ((typeof(ISubscriber) == typeof(IPXCommandPreparingSubscriber)
                 || typeof(ISubscriber) == typeof(IPXRowSelectingSubscriber)))
            {
                if (_IsDBField == false)
                {
                    subscibers.Add(this as ISubscriber);
                }
                else
                {
                    base.GetSubscriber<ISubscriber>(subscibers);
                    subscibers.Remove(this as ISubscriber);
                }
            }
            else
            {
                base.GetSubscriber<ISubscriber>(subscibers);
            }
        }

        public override void CacheAttached(PXCache sender)
        {
            base.CacheAttached(sender);

            if (SourceType != null)
            {
                PXCache dateCache = sender.Graph.Caches[BqlCommand.GetItemType(SourceFieldType)];

                if (dateCache.GetItemType().IsAssignableFrom(sender.GetItemType())
                    || sender.GetItemType().IsAssignableFrom(dateCache.GetItemType()))
                {
                    sender.Graph.FieldUpdated.AddHandler(_sourceType, _sourceField, DateSourceFieldUpdated);
                }
            }
        }

        protected virtual Type GetExecutableDefaultType() => DefaultType;

        #endregion

        #region Event Handlers

        public virtual void CommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
        {
            e.Expr = null;
            e.Cancel = true;
        }

        public virtual void RowSelecting(PXCache sender, PXRowSelectingEventArgs e)
        {
            sender.SetValue(e.Row, _FieldOrdinal, null);
        }

        protected void DateSourceFieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            if (RedefaultOnDateChanged)
            {
                RedefaultPeriodID(cache, e.Row);
            }
        }

        #endregion

        #region Methods

        protected virtual DateTime? GetSourceDate(PXCache cache, object row)
        {
            if (SourceFieldType == null)
                return null;

            PXCache dateCache = cache.Graph.Caches[BqlCommand.GetItemType(SourceFieldType)];

            return dateCache.GetItemType().IsAssignableFrom(row.GetType())
                ? (DateTime?)dateCache.GetValue(row, _sourceField)
                : (DateTime?)dateCache.GetValue(dateCache.Current, _sourceField);
        }

        protected virtual bool IsSourcesValuesDefined(PXCache cache, object row)
        {
            return SourceFieldType == null || GetSourceDate(cache, row) != null;
        }

        protected virtual object[] GetPeriodKey(PXCache cache, object row)
        {
            return null;
        }

        protected virtual QueryParams GetQueryParams(PXCache cache, object row)
        {
            return new QueryParams()
            {
                SourceDate = GetSourceDate(cache, row)
            };
        }

        protected virtual void SetPeriodIDBySources(PXCache cache, object row)
        {
            if (IsSourcesValuesDefined(cache, row))
            {
                QueryParams queryParams = GetQueryParams(cache, row);

                PeriodResult result = GetPeriod(cache.Graph,
                    GetExecutableDefaultType(),
                    queryParams,
                    GetPeriodKey(cache, row),
                    row.SingleToListOrNull(),
                    DefaultingQueryParametersDelegate);

                object newValue = (string)result;
                bool haspending = false;
                try
                {
                    object pendingValue = cache.GetValuePending(row, _FieldName);
                    if (pendingValue != null && pendingValue != PXCache.NotSetValue)
                    {
                        object period = UnFormatPeriod((string)pendingValue);

                        // We should call field verifying only when source field
                        // has been updated from the UI to prevent unexpected
                        // financial period value change. In such cases it is
                        // possible to set a new value without required validations
                        // which should be done inside the related attributes, see
                        // AC-106058 for details.
                        // 
                        if (!cache.Graph.UnattendedMode)
                        {
                            object rowCopy = cache.CreateCopy(row);

                            cache.RaiseFieldVerifying(_FieldName, row, ref period);

                            period = UnFormatPeriod((string)pendingValue);

                            cache.SetValue(rowCopy, _FieldName, period);

                            cache.RaiseRowUpdating(row, rowCopy);

                            if (cache.GetAttributesReadonly(rowCopy, _FieldName)
                                .OfType<IPXInterfaceField>()
                                .Any(a => a.ErrorLevel == PXErrorLevel.Error || a.ErrorLevel == PXErrorLevel.RowError))
                            {
                                cache.SetValuePending(row, _FieldName, newValue);
                            }
                        }

                        haspending = true;
                    }
                }
                catch (PXSetPropertyException)
                {
                    cache.SetValuePending(row, _FieldName, newValue);
                }
                finally
                {
                    if (cache.HasAttributes(row))
                    {
                        cache.RaiseExceptionHandling(_FieldName, row, null, null);
                    }

                    //this will happen only if FirstOpenPeriod is set from OpenPeriod
                    if (!haspending && result.StartDate > queryParams.SourceDate)
                    {
                        cache.SetValueExt(row, _sourceField, result.StartDate);
                    }
                    else
                    {
                        cache.SetDefaultExt(row, _FieldName, newValue);
                    }
                }
            }
            else
            {
                cache.SetValueExt(row, _FieldName, null);
            }
        }

        protected virtual void RedefaultPeriodID(PXCache cache, object row)
        {
            SetPeriodIDBySources(cache, row);            
        }

        public class QueryParams : IBqlTable
        {
            public abstract class sourceDate : IBqlField { }

            [PXDBDate]
            public DateTime? SourceDate { get; set; }
        }

        public static PeriodResult GetPeriod(PXGraph graph,
            Type searchType,
            QueryParams queryParams,
            object[] periodKey,
            List<object> currents = null,
            Func<QueryParams, object[], List<object>, object[]> getParametersDelegate = null,
            Boolean applyFormat = true)
        {
            BqlCommand select = BqlCommand.CreateInstance(searchType) ?? throw new ArgumentNullException("BqlCommand.CreateInstance(searchType)");

            Type sourceType = BqlCommand.GetItemType(((IBqlSearch)select).GetField());
            string sourceField = ((IBqlSearch)select).GetField().Name;

            object[] parameters = null;

            if (getParametersDelegate != null)
            {
                parameters = getParametersDelegate(queryParams, periodKey, currents);
            }
            else
            {
                List<object> paramList = new List<object>();

                paramList.AddRange(periodKey);

                parameters = paramList.ToArray();
            }

            List<object> queryCurrents = new List<object>() {queryParams};

            if (currents != null)
            {
                queryCurrents.AddRange(currents);
            }

            PXView view = graph.TypedViews.GetView(select, false);
            int startRow = 0;
            int totalRows = 0;
            List<object> source = view.Select(
                queryCurrents.ToArray(),
                parameters,
                null,
                null,
                null,
                null,
                ref startRow,
                1,
                ref totalRows);
            if (source != null && source.Count > 0)
            {
                object item = source[source.Count - 1];
                if (item != null && item is PXResult)
                {
                    item = ((PXResult)item)[sourceType];
                }

                string result = (string)graph.Caches[sourceType].GetValue(item, sourceField);
                if (applyFormat && result != null && result.Length == 6)
                {
                    result = FormatForDisplay(result);
                }

                if (item as IPeriod != null)
                {
                    return new PeriodResult { PeriodID = result, StartDate = (item as IPeriod).StartDate, EndDate = (item as IPeriod).EndDate };
                }
            }
            return new PeriodResult();
        }

        public virtual void GetFields(PXCache sender, object row)
        {
            _sourceDate = DateTime.MinValue;
            if (_sourceType != null)
            {
                _sourceDate = (DateTime)(PXView.FieldGetValue(sender, row, _sourceType, _sourceField) ?? DateTime.MinValue);
            }
        }

        public virtual DateTime GetDate(PXCache sender, object row)
        {
            GetFields(sender, row);
            return _sourceDate;
        }

        #endregion

        #region Formatting Helpers

        public static string FormatPeriod(string period)
        {
            return FormatForDisplay(period);
        }

        /// <summary>
        /// Format Period to string that can be used in an error message.
        /// </summary>
        public static string FormatForError(string period)
        {
            return FinPeriodIDFormattingAttribute.FormatForError(period);
        }

        /// <summary>
        /// Format Period to string that can be displayed in the control.
        /// </summary>
        /// <param name="period">Period in database format</param>
        public static string FormatForDisplay(string period)
        {
            return FinPeriodIDFormattingAttribute.FormatForDisplay(period);
        }

        /// <summary>
        /// Format period to database format
        /// </summary>
        /// <param name="period">Period in display format</param>
        /// <returns></returns>
        public static string UnFormatPeriod(string period)
        {
            return FinPeriodIDFormattingAttribute.FormatForStoring(period);
        }

        #endregion
    }

	/// <summary>
	/// Selector for FinPeriod. Extends <see cref="FinPeriodIDAttribute"/>.
	/// Displays all available fin Periods.
	/// </summary>
	public class FinPeriodSelectorAttribute : FinPeriodIDAttribute, IPXFieldVerifyingSubscriber
    {
		public Type OrigSelectorSearchType { get; private set; }

		#region Ctor
		public FinPeriodSelectorAttribute()
			: this(null)
		{
		}

		public FinPeriodSelectorAttribute(Type sourceType)
			: this(null, sourceType)
		{
		}

        public FinPeriodSelectorAttribute(
            Type searchType,
            Type sourceType,
            Type branchSourceType = null,
            Type branchSourceFormulaType = null,
            Type organizationSourceType = null,
            Type useMasterCalendarSourceType = null,
            Type defaultType = null,
            bool redefaultOrRevalidateOnOrganizationSourceUpdated = true,
            bool takeBranchForSelectorFromQueryParams = false,
            bool takeOrganizationForSelectorFromQueryParams = false,
            bool useMasterOrganizationIDByDefault = false,
            bool masterPeriodBasedOnOrganizationPeriods = true,
            SelectionModesWithRestrictions selectionModeWithRestrictions = SelectionModesWithRestrictions.Undefined,
            Type[] sourceSpecificationTypes = null,
            Type[] fieldList = null,
            Type masterFinPeriodIDType = null,
            bool redefaultOnDateChanged = true)
            : base(
                masterFinPeriodIDType: masterFinPeriodIDType,
                sourceType: sourceType,
                branchSourceType: branchSourceType,
                branchSourceFormulaType: branchSourceFormulaType,
                organizationSourceType: organizationSourceType,
                useMasterCalendarSourceType: useMasterCalendarSourceType,
                defaultType: defaultType,
                redefaultOrRevalidateOnOrganizationSourceUpdated: redefaultOrRevalidateOnOrganizationSourceUpdated,
                useMasterOrganizationIDByDefault: useMasterOrganizationIDByDefault,
                sourceSpecificationTypes: sourceSpecificationTypes,
                redefaultOnDateChanged: redefaultOnDateChanged)
        {
            Type[] fields;

            SelectionModeWithRestrictions = selectionModeWithRestrictions;

            if (fieldList != null && fieldList.Length > 0)
            {
                fields = fieldList;
            }
            else
            {
                fields = new Type[]
                {
                    typeof(FinPeriod.finPeriodID),
                    typeof(FinPeriod.descr),
                    typeof(FinPeriod.startDateUI),
                    typeof(FinPeriod.endDateUI)
                };
            }

            OrigSelectorSearchType = searchType;

            if (OrigSelectorSearchType == null)
            {
                OrigSelectorSearchType = GetDefaultSearchType();
            }

            if (OrigSelectorSearchType == null || !typeof(IBqlSearch).IsAssignableFrom(OrigSelectorSearchType))
            {
                throw new PXArgumentException("search", ErrorMessages.ArgumentException);
            }

            Type searchTypeDefinition = OrigSelectorSearchType.GetGenericTypeDefinition();
            Type[] searchArgs = OrigSelectorSearchType.GetGenericArguments();

            Type cmd;
            //compatibility: if WhereType = null then Desc<> else Asc<>
            if (searchTypeDefinition == typeof(Search<>))
            {
                cmd = BqlCommand.Compose(
                    typeof(Search3<,>),
                    typeof(FinPeriod.finPeriodID),
                    typeof(OrderBy<Desc<FinPeriod.finPeriodID>>));
            }
            else if (searchTypeDefinition == typeof(Search<,>))
            {
                cmd = BqlCommand.Compose(
                    typeof(Search<,,>),
                    typeof(FinPeriod.finPeriodID),
                    searchArgs[1],
                    typeof(OrderBy<Asc<FinPeriod.finPeriodID>>));
            }
            else if (searchTypeDefinition == typeof(Search<,,>))
            {
                cmd = OrigSelectorSearchType;
            }

            else if (searchTypeDefinition == typeof(Search2<,>))
            {
                cmd = BqlCommand.Compose(
                    typeof(Search3<,,>),
                    typeof(FinPeriod.finPeriodID),
                    searchArgs[1],
                    typeof(OrderBy<Desc<FinPeriod.finPeriodID>>));
            }
            else if (searchTypeDefinition == typeof(Search2<,,>))
            {
                cmd = BqlCommand.Compose(
                    typeof(Search2<,,,>),
                    typeof(FinPeriod.finPeriodID),
                    searchArgs[1],
                    searchArgs[2],
                    typeof(OrderBy<Asc<FinPeriod.finPeriodID>>));
            }
            else if (searchTypeDefinition == typeof(Search2<,,,>))
            {
                cmd = OrigSelectorSearchType;
            }
            else if (searchTypeDefinition == typeof(Search3<,>))
            {
                cmd = OrigSelectorSearchType;
            }
            else if (searchTypeDefinition == typeof(Search3<,,>))
            {
                cmd = OrigSelectorSearchType;
            }
            else if (searchTypeDefinition == typeof(Search5<,,>))
            {
                cmd = OrigSelectorSearchType;
            }
            else if (searchTypeDefinition == typeof(Search5<,,,>))
            {
                cmd = OrigSelectorSearchType;
            }
            else
            {
                throw new PXArgumentException("search", ErrorMessages.ArgumentException);
            }

            PXSelectorAttribute selectorAttribute =
                searchArgs[0]?.DeclaringType == typeof(FinPeriod)
                    ? new GenericFinPeriodSelectorAttribute(
                        cmd,
                        () => CalendarOrganizationIDProvider,
                        takeBranchForSelectorFromQueryParams,
                        takeOrganizationForSelectorFromQueryParams,
                        masterPeriodBasedOnOrganizationPeriods,
                        SelectionModeWithRestrictions,
                        fields)
                    {
                        CustomMessageElementDoesntExist = Messages.FinPeriodForBranchOrCompanyDoesNotExist
                    }
                    : new PXSelectorAttribute(cmd, fields)
                    {
                        CustomMessageElementDoesntExist = Messages.FinPeriodForBranchOrCompanyDoesNotExist
                    };

            _Attributes.Add(selectorAttribute);
        }

        #endregion

		#region Initialization
		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);
			Selector.SelectorMode |= PXSelectorMode.NoAutocomplete;
		}

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
                _Attributes.ForEach(_ => (_ as IPXFieldVerifyingSubscriber)?.FieldVerifying(sender, e));
            }
            catch (PXSetPropertyException)
            {
                e.NewValue = FormatForDisplay((string)e.NewValue);
                throw;
            }
        }

        protected virtual Type GetDefaultSearchType()
		{
			return typeof(Search3<FinPeriod.finPeriodID, OrderBy<Desc<FinPeriod.finPeriodID>>>);
		}
				
		#endregion

		protected PXSelectorAttribute Selector
		{
			get { return _Attributes.OfType<PXSelectorAttribute>().First(); }
		}

		public Type DescriptionField
		{
			get { return Selector.DescriptionField; }
			set { Selector.DescriptionField = value; }
		}

		public PXSelectorMode SelectorMode
		{
			get { return Selector.SelectorMode; }
			set { Selector.SelectorMode = value; }
		}

        public SelectionModesWithRestrictions SelectionModeWithRestrictions { get; set; }

        public enum SelectionModesWithRestrictions
        {
			Undefined,
            Any,
            All
        }
	}

	public class FinPeriodNonLockedSelectorAttribute : FinPeriodSelectorAttribute, IPXFieldVerifyingSubscriber
    {
		#region Ctor

		public FinPeriodNonLockedSelectorAttribute()
			:this(null)

		{									
		}

		public FinPeriodNonLockedSelectorAttribute(Type sourceType)
			:base(typeof(Search5<MasterFinPeriod.finPeriodID,
					LeftJoin<FinPeriod, On<FinPeriod.finPeriodID, Equal<MasterFinPeriod.finPeriodID>,
						And<FinPeriod.organizationID, NotEqual<FinPeriod.organizationID.masterValue>,
							And<FinPeriod.status, Equal<FinPeriod.status.locked>>>>>,
					Where<MasterFinPeriod.startDate, NotEqual<MasterFinPeriod.endDate>>,
					Aggregate<GroupBy<MasterFinPeriod.finPeriodID>>>),
				 sourceType,
				fieldList: new Type[] {
					typeof(MasterFinPeriod.finPeriodID),
					typeof(MasterFinPeriod.descr),
					typeof(MasterFinPeriod.startDateUI),
					typeof(MasterFinPeriod.endDateUI)
				})
		{
			_Attributes.Add( new PXDefaultAttribute(
				typeof(Search5<MasterFinPeriod.finPeriodID,
					LeftJoin<FinPeriod, On<FinPeriod.finPeriodID, Equal<MasterFinPeriod.finPeriodID>,
						And<FinPeriod.organizationID, NotEqual<FinPeriod.organizationID.masterValue>,
							And<FinPeriod.status, Equal<FinPeriod.status.locked>>>>>,
					Where<FinPeriod.finPeriodID, IsNull,
						And<MasterFinPeriod.startDate, NotEqual<MasterFinPeriod.endDate>>>,
					Aggregate<GroupBy<MasterFinPeriod.finPeriodID>>,
					OrderBy<Asc<MasterFinPeriod.finPeriodID>>>)
				) );
			_Attributes.Add(new PXRestrictorAttribute(typeof(Where<FinPeriod.finPeriodID, IsNull>), Messages.FinPeriodLocked, typeof(MasterFinPeriod.finPeriodID)));
		}
		#endregion

		#region Implementation
		public override void FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
		    base.FieldVerifying(sender, e);

			if (string.IsNullOrEmpty(e.NewValue?.ToString()))
			{
				throw new PXSetPropertyException(Messages.Prefix + ": " + Messages.ProcessingRequireFinPeriodID, "finPeriodID");
			}
		}
		#endregion
	}

	public enum PeriodValidation
	{
		Nothing,
		DefaultUpdate,
		DefaultSelectUpdate,
	}

	/// <summary>
	/// FinPeriod selector that extends <see cref="FinPeriodSelectorAttribute"/>. 
	/// Displays and accepts only Open Fin Periods. 
	/// When Date is supplied through SourceType parameter FinPeriod is defaulted with the FinPeriod for the given date.
	/// </summary>
	[PXAttributeFamily(typeof(OpenPeriodAttribute))]
	public class OpenPeriodAttribute : FinPeriodSelectorAttribute, IPXFieldSelectingSubscriber, IPXRowSelectedSubscriber, IPXFieldUpdatedSubscriber
    {
		#region State
		protected PeriodValidation _ValidatePeriod = PeriodValidation.DefaultUpdate;

		/// <summary>
		/// Gets or sets how the Period validation logic is handled.
		/// </summary>
		public PeriodValidation ValidatePeriod
		{
			get
			{
				return _ValidatePeriod;
			}
			set
			{
				_ValidatePeriod = value;
			}
		}

		protected bool _throwErrorExternal = false;

		public bool ThrowErrorExternal
		{
			get
			{
				return _throwErrorExternal;
			}
			set
			{
				_throwErrorExternal = value;
			}
		}

		protected BqlCommand GetSuitableFinPeriodCountInCompaniesCmd { get; set; }

		#endregion

		#region Ctor
		public OpenPeriodAttribute(Type SourceType)
			: this(null, SourceType)
		{
		}

		public OpenPeriodAttribute()
			: this(null)
		{
		}

		public OpenPeriodAttribute(
				Type searchType, 
				Type sourceType,
				Type branchSourceType = null,
				Type branchSourceFormulaType = null,
				Type organizationSourceType = null,
				Type useMasterCalendarSourceType = null,
				Type defaultType = null,
				bool redefaultOrRevalidateOnOrganizationSourceUpdated = true,
				bool takeBranchForSelectorFromQueryParams = false,
				bool takeOrganizationForSelectorFromQueryParams = false,
				bool useMasterOrganizationIDByDefault = false,
				bool masterPeriodBasedOnOrganizationPeriods = true,
				SelectionModesWithRestrictions selectionModeWithRestrictions = SelectionModesWithRestrictions.Undefined,
				Type[] sourceSpecificationTypes = null,
		        Type masterFinPeriodIDType = null,
		        bool redefaultOnDateChanged = true)
			: base(
			    searchType, 
				sourceType,
			    masterFinPeriodIDType: masterFinPeriodIDType,
                branchSourceType: branchSourceType,
				branchSourceFormulaType: branchSourceFormulaType,
				organizationSourceType: organizationSourceType,
				useMasterCalendarSourceType: useMasterCalendarSourceType,
				defaultType: defaultType,
				redefaultOrRevalidateOnOrganizationSourceUpdated: redefaultOrRevalidateOnOrganizationSourceUpdated,
				takeBranchForSelectorFromQueryParams: takeBranchForSelectorFromQueryParams,
				takeOrganizationForSelectorFromQueryParams: takeOrganizationForSelectorFromQueryParams,
				useMasterOrganizationIDByDefault: useMasterOrganizationIDByDefault,
				masterPeriodBasedOnOrganizationPeriods: masterPeriodBasedOnOrganizationPeriods,
				selectionModeWithRestrictions: selectionModeWithRestrictions,
				sourceSpecificationTypes: sourceSpecificationTypes,
			    redefaultOnDateChanged: redefaultOnDateChanged)
		{
			GetSuitableFinPeriodCountInCompaniesCmd = GenerateGetSuitableFinPeriodCountInCompaniesCmd(OrigSelectorSearchType);
		}

		protected virtual int? GetSuitableFinPeriodCountInCompanies(PXGraph graph, int?[] organizationIDs, string finPeriodID)
		{
			PXView view = new PXView(graph, true, GetSuitableFinPeriodCountInCompaniesCmd);

			List<object> data = view.SelectMulti(organizationIDs, finPeriodID);

			if (data != null && data.Any())
			{
				return ((PXResult) data[0]).RowCount ?? 0;
		}

			return 0;
		}
		
		protected virtual BqlCommand GenerateGetSuitableFinPeriodCountInCompaniesCmd(Type queryType)
		{
			BqlCommand cmd = BqlCommand.CreateInstance(queryType);

			cmd = cmd.WhereAnd<Where<FinPeriod.organizationID, In<Required<FinPeriod.organizationID>>,
										And<FinPeriod.finPeriodID, Equal<Required<FinPeriod.finPeriodID>>>>>();

			cmd = cmd.AggregateNew<Aggregate<GroupBy<FinPeriod.finPeriodID,
												Count<FinPeriod.organizationID>>>>();

			return cmd;
		}

		#endregion


		#region Runtime
		public static void SetValidatePeriod<Field>(PXCache cache, object data, PeriodValidation isValidatePeriod)
			where Field : IBqlField
		{
			if (data == null)
			{
				cache.SetAltered<Field>(true);
			}
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes<Field>(data))
			{
				if (attr is OpenPeriodAttribute)
				{
					((OpenPeriodAttribute)attr).ValidatePeriod = isValidatePeriod;
				}
			}
		}

		public static void SetValidatePeriod(PXCache cache, object data, string name, PeriodValidation isValidatePeriod)
		{
			if (data == null)
			{
				cache.SetAltered(name, true);
			}
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes(data, name))
			{
				if (attr is OpenPeriodAttribute)
				{
					((OpenPeriodAttribute)attr).ValidatePeriod = isValidatePeriod;
				}
			}
		}
		#endregion

		#region Initialization
		public override void GetSubscriber<ISubscriber>(List<ISubscriber> subscribers)
		{
			if (typeof(ISubscriber) == typeof(IPXFieldVerifyingSubscriber)
				|| typeof(ISubscriber) == typeof(IPXFieldDefaultingSubscriber)
				|| typeof(ISubscriber) == typeof(IPXFieldSelectingSubscriber)
				)
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

		protected override Type GetDefaultSearchType()
		{
			return typeof(Search<FinPeriod.finPeriodID, Where<FinPeriod.status, Equal<FinPeriod.status.open>>>);
		}

		public override void FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
		    if (_ValidatePeriod != PeriodValidation.Nothing)
		    {
		        OpenPeriodVerifying(sender, e);
		    }
		    else
		    {
		        base.FieldVerifying(sender, e);
		    }
        }

		public override void FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (_ValidatePeriod != PeriodValidation.Nothing)
			{
				OpenPeriodDefaulting(sender, e);
			}
			else
			{
				base.FieldDefaulting(sender, e);
			}
		}

		public virtual void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			((IPXFieldSelectingSubscriber)_Attributes[1]).FieldSelecting(sender, e);
			((IPXFieldSelectingSubscriber)_Attributes[2]).FieldSelecting(sender, e);

			if (e.ReturnState != null && e.ReturnState is PXStringState)
			{
				e.ReturnState = PXStringState.CreateInstance(e.ReturnState, ((PXStringState)e.ReturnState).Length, null, _FieldName, null, 1, ((PXStringState)e.ReturnState).InputMask, null, null, null, null);
			}
		}

		public virtual void FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			object val = sender.GetValueExt(e.Row, _FieldName);
			if (val is PXFieldState)
			{
				val = ((PXFieldState)val).Value;
			}

			string errval = UnFormatPeriod((string)val);
			if (errval != null && !errval.Equals(sender.GetValue(e.Row, _FieldName)))
			{
				PXUIFieldAttribute.SetError(sender, e.Row, _FieldName, null, null);
			}

		}

		public virtual void OpenPeriodDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
            base.FieldDefaulting(sender, e);

		    if (e.NewValue == null)
		    {
		        return;
		    }

		    try
		    {
		        IsValidPeriod(sender, e.Row, UnFormatPeriod((string)e.NewValue));
		    }
		    catch (PXSetPropertyException ex)
		    {
		        if (ThrowErrorExternal == true)
		        {
		            throw ex;
		        }

		        if (e.Row != null)
		        {
		            sender.RaiseExceptionHandling(_FieldName, e.Row, e.NewValue, ex);
		        }
		        e.NewValue = null;
		    }
        }

		public static void SetThrowErrorExternal<Field>(PXCache cache, bool throwErrorExternal)
			where Field : IBqlField
		{
			cache.GetAttributes<Field>().OfType<AROpenPeriodAttribute>().ForEach(attr => attr.ThrowErrorExternal = throwErrorExternal);
		}

		public virtual void OpenPeriodVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			string NewValue = e.NewValue as string;
			string OldValue = (string)sender.GetValue(e.Row, _FieldName);

			try
			{
				IsValidPeriod(sender, e.Row, e.NewValue);
			}
			catch (PXSetPropertyException)
			{
				e.NewValue = FormatForDisplay((string)e.NewValue);
				throw;
			}
			IsCurrentPeriod(sender, e.Row, e.NewValue);

		}

		protected virtual void IsCurrentPeriod(PXCache sender, object row, object value)
		{
			GetFields(sender, row);
			if (_ValidatePeriod != PeriodValidation.Nothing && _sourceDate != DateTime.MinValue)
			{
				int? calendarOrganizationID = CalendarOrganizationIDProvider.GetCalendarOrganizationID(sender.Graph, sender, row);

				string finPeriodIDFromDate = GetPeriod(
				    sender.Graph, 
					SearchTypeRestrictedByOrganization ?? SearchType, 
					new QueryParams(){SourceDate = _sourceDate },
					calendarOrganizationID.SingleToObjectArray(dontCreateForNull: false),
					row.SingleToListOrNull(),
					applyFormat: false);
				string userPeriod = (string)value;
				if (!object.Equals(finPeriodIDFromDate, userPeriod))
				{
					//check if user entered adjustment period
					PXResult<FinPeriod, FinYear> record = PXSelectJoin<FinPeriod, 
						InnerJoin<FinYear, 
							On<FinYear.year, Equal<FinPeriod.finYear>,
								And<FinYear.organizationID, Equal<FinPeriod.organizationID>>>>, 
						Where<FinPeriod.finPeriodID, Equal<Required<FinPeriod.finPeriodID>>>, 
						OrderBy<Asc<FinPeriod.finPeriodID>>>
						.SelectSingleBound(sender.Graph, null, userPeriod)
						.AsEnumerable()
						.Cast<PXResult<FinPeriod, FinYear>>()
						.SingleOrDefault();
					FinPeriod period = record;
					FinYear year = record;

					if (period != null 
						&& period.StartDate == period.EndDate
						&& FinPeriodUtils.FiscalYear(finPeriodIDFromDate) == period.FinYear)
					{
						return;
					}

					if (PXUIFieldAttribute.GetError(sender, row, _FieldName) == null)
					{
						PXUIFieldAttribute.SetWarning(sender, row, _FieldName, Messages.FiscalPeriodNotCurrent);
					}
				}
			}
		}

		public virtual void RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			if (sender.Graph.GetType() != typeof(PXGraph) && sender.Graph.GetType() != typeof(PXGenericInqGrph) && !sender.Graph.IsImport)
			{
				//will be called after graph event
				//GetFields(sender, e.Row);
				if (_AttributeLevel == PXAttributeLevel.Item)
				{
					IsDirty = true;
					_Attributes[1].IsDirty = true;
				}
				sender.Graph.RowSelected.AddHandler(sender.GetItemType(), new DynamicRowSelected(Document_RowSelected).RowSelected);
			}
		}

		private class DynamicRowSelected
		{
			private PXRowSelected _del;

			public DynamicRowSelected(PXRowSelected del)
			{
				_del = del;
			}
			public void RowSelected(PXCache sender, PXRowSelectedEventArgs e)
			{
				try
				{
					_del(sender, e);
				}
				finally
				{
					sender.Graph.RowSelected.RemoveHandler(sender.GetItemType(), RowSelected);
				}
			}
		}

		private PeriodValidation GetValidatePeriod(PXCache cache, object Row)
		{
			foreach (OpenPeriodAttribute attr in cache.GetAttributesReadonly(Row, _FieldName).OfType<OpenPeriodAttribute>())
			{
				return attr._ValidatePeriod;
			}
			return PeriodValidation.Nothing;
		}

		public virtual void Document_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			if (GetValidatePeriod(sender, e.Row) == PeriodValidation.DefaultSelectUpdate)
			{
				object oldState = sender.GetValueExt(e.Row, _FieldName);
				if (oldState is PXFieldState)
				{
					oldState = ((PXFieldState)oldState).Value;
				}
				string OldValue = UnFormatPeriod(oldState as string);

				try
				{
					if (sender.AllowDelete && !string.IsNullOrEmpty(OldValue))
					{
						IsValidPeriod(sender, e.Row, OldValue);
						IsCurrentPeriod(sender, e.Row, OldValue);
					}
				}
				catch (PXSetPropertyException ex)
				{
					sender.RaiseExceptionHandling(_FieldName, e.Row, FormatForDisplay(OldValue), ex);
				}
			}
		}

		public override void RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
            base.RowPersisting(sender, e);

			string NewValue = (string)sender.GetValue(e.Row, _FieldName);

			try
			{
			    bool validatePeriod = (e.Operation & PXDBOperation.Command) == PXDBOperation.Insert && _ValidatePeriod != PeriodValidation.Nothing;

                if (validatePeriod || (e.Operation & PXDBOperation.Command) == PXDBOperation.Update && sender.AllowDelete && _ValidatePeriod == PeriodValidation.DefaultSelectUpdate)
                {
                    object origRow = sender.GetOriginal(e.Row);

                    OrganizationDependedPeriodKey origPeriodKey = GetFullKey(sender, origRow);
                    OrganizationDependedPeriodKey periodKey = GetFullKey( sender, e.Row);

                    if (origPeriodKey.OrganizationID != periodKey.OrganizationID
                        || origPeriodKey.PeriodID != periodKey.PeriodID)
                    {
                        IsValidPeriod(sender, e.Row, NewValue);

                        GetFields(sender, e.Row);
                        if (_sourceDate != DateTime.MinValue && string.IsNullOrEmpty(NewValue))
                        {
                            if (sender.RaiseExceptionHandling(_FieldName, e.Row, null, new PXSetPropertyKeepPreviousException(PXMessages.LocalizeFormat(ErrorMessages.FieldIsEmpty, _FieldName))))
                            {
                                throw new PXRowPersistingException(_FieldName, null, ErrorMessages.FieldIsEmpty, _FieldName);
                            }
                        }
                    }
				}
			}
			catch (PXSetPropertyException ex)
			{
				if (sender.RaiseExceptionHandling(_FieldName, e.Row, FormatForDisplay(NewValue), ex))
				{
					throw new PXRowPersistingException(_FieldName, FormatForDisplay(NewValue), ex.Message);
				}
			}
		}

		public virtual void IsValidPeriod(PXCache sender, object Row, object NewValue)
		{
			if (NewValue != null && _ValidatePeriod != PeriodValidation.Nothing)
			{
				ValidateFinPeriodID(sender, Row, (string)NewValue);
            }
        }

		protected virtual void ValidateFinPeriodID(PXCache sender, object row, string finPeriodID)
		{
		    int? calendarOrganizationID = CalendarOrganizationIDProvider.GetCalendarOrganizationID(sender.Graph, sender, row);

		    if (calendarOrganizationID == null)
		    {
		        throw new PXSetPropertyException(Messages.FinPeriodCanNotBeSpecified);
		    }

		    if (calendarOrganizationID == FinPeriod.organizationID.MasterValue)
		    {
		        FinPeriod period = sender.Graph.GetService<IFinPeriodRepository>()
		            .FindByID(calendarOrganizationID, finPeriodID);

		        if (period == null)
		        {
					throw new PXSetPropertyException(Messages.MasterFinPeriodDoesNotExist, FormatForError(finPeriodID));
		        }
            }

			ValidateFinPeriodsStatus(sender, row, finPeriodID, CalendarOrganizationIDProvider);
		}

		protected virtual void ValidateFinPeriodsStatus(PXCache sender, object row, string finPeriodID, ICalendarOrganizationIDProvider calendarOrganizationIDProvider)
		{
			CalendarOrganizationIDProvider.PeriodKeyWithSourceValuesCollection keyWithSourceValues =
				calendarOrganizationIDProvider.GetKeysWithBasisOrganizationIDs(sender.Graph, sender, row);

			int?[] organizationIDs = keyWithSourceValues.ConsolidatedOrganizationIDs.ToArray();

			if (SelectionModeWithRestrictions == SelectionModesWithRestrictions.All
			    || SelectionModeWithRestrictions == SelectionModesWithRestrictions.Undefined
					&& calendarOrganizationIDProvider.GetCalendarOrganizationID(sender.Graph, sender, row) != FinPeriod.organizationID.MasterValue)
			{
				ValidateFinPeriodsStatusForAll(sender, row, finPeriodID, organizationIDs);
			}
			else
			{
				ValidateFinPeriodsStatusForAny(sender, row, finPeriodID, organizationIDs);
			}
		}

		private void ValidateFinPeriodsStatusForAny(PXCache sender, object row, string finPeriodID, int?[] organizationIDs)
		{
			int? availablePeriodCount;

			int? openPeriodCount = GetSuitableFinPeriodCountInCompanies(sender.Graph, organizationIDs, finPeriodID);

			IFinPeriodUtils periodUtils = sender.Graph.GetService<IFinPeriodUtils>();

			if (periodUtils.CanPostToClosedPeriod())
			{
				availablePeriodCount = PXSelectGroupBy<OrganizationFinPeriod,
												Where<OrganizationFinPeriod.status, NotEqual<FinPeriod.status.locked>,
													And<OrganizationFinPeriod.status, NotEqual<FinPeriod.status.inactive>,
													And<OrganizationFinPeriod.organizationID, In<Required<OrganizationFinPeriod.organizationID>>,
													And<OrganizationFinPeriod.finPeriodID, Equal<Required<OrganizationFinPeriod.finPeriodID>>>>>>,
												Aggregate<GroupBy<OrganizationFinPeriod.finPeriodID,
															Count<OrganizationFinPeriod.organizationID>>>>
												.Select(sender.Graph, organizationIDs, finPeriodID)
												.RowCount;

			}
			else
			{
				availablePeriodCount = openPeriodCount;
			}

			if (availablePeriodCount == 0)
			{
				throw new PXSetPropertyException(Messages.FinPeriodCannotBeUsedForProcessing, FormatForError(finPeriodID));
			}

			ProcessingResult validationResult = new ProcessingResult();

			if (availablePeriodCount != organizationIDs.Length)
			{
				validationResult.AddMessage(PXErrorLevel.Warning, Messages.FinPeriodCannotBeUsedForProcessingForAtLeastOneCompany, FormatForError(finPeriodID));
			}

			if (availablePeriodCount != openPeriodCount 
			    && openPeriodCount != organizationIDs.Length)
			{
				validationResult.AddMessage(PXErrorLevel.Warning, Messages.FinPeriodIsClosedInAtLeastOneCompany, FormatForError(finPeriodID));
		}

			if (validationResult.HasWarningOrError)
			{
				if (PXUIFieldAttribute.GetError(sender, row, _FieldName) == null)
		{
					PXUIFieldAttribute.SetWarning(sender, row, _FieldName, validationResult.GeneralMessage);
				}
			}
		}

		private void ValidateFinPeriodsStatusForAll(PXCache sender, object row, string finPeriodID, int?[] organizationIDs)
			{
			List<FinPeriod> finPeriods = 
				PXSelect<FinPeriod,
					Where<FinPeriod.finPeriodID, Equal<Required<FinPeriod.finPeriodID>>,
						And<FinPeriod.organizationID, In<Required<FinPeriod.organizationID>>>>>
					.Select(sender.Graph, finPeriodID, organizationIDs)
					.RowCast<FinPeriod>()
					.ToList();

			PeriodValidationResult validationResult = new PeriodValidationResult();

			if (finPeriods.Count != organizationIDs.Length)
				{
				IEnumerable<int?> unexistingForOrganizationIDs =
					organizationIDs.Except(finPeriods.Select(period => period.OrganizationID));

				validationResult.AddMessage(
					PXErrorLevel.Error,
					ExceptionType.Custom,
					Messages.FinPeriodDoesNotExistForCompanies,
					FinPeriodIDFormattingAttribute.FormatForError(finPeriodID),
					unexistingForOrganizationIDs.Select(PXAccess.GetOrganizationCD).ToArray().JoinIntoStringForMessageNoQuotes(5));
				}

			foreach (FinPeriod finPeriod in finPeriods)
				{
				PeriodValidationResult statusValidationResult = ValidateOrganizationFinPeriodStatus(sender, row, finPeriod);

				validationResult.Aggregate(statusValidationResult);
				}

			if (validationResult.HasWarningOrError)
				{
				ExceptionType maxExceptionType = validationResult.Messages.Max(mes => mes.ExceptionType);
				PXErrorLevel maxErrorLevel = validationResult.MaxErrorLevel;

				PXSetPropertyException finPeriodEx =
					maxExceptionType == ExceptionType.Locked ? new FiscalPeriodLockedException(validationResult.GeneralMessage, maxErrorLevel) :
					maxExceptionType == ExceptionType.Inactive ? new FiscalPeriodInactiveException(validationResult.GeneralMessage, maxErrorLevel) :
					maxExceptionType == ExceptionType.Closed ? new FiscalPeriodClosedException(validationResult.GeneralMessage, maxErrorLevel) :
					new PXSetPropertyException(validationResult.GeneralMessage, maxErrorLevel);

				if (validationResult.MaxErrorLevel <= PXErrorLevel.Warning)
					{
					sender.RaiseExceptionHandling(_FieldName, row, FormatForDisplay(finPeriodID), finPeriodEx);
					}
					else
					{
					finPeriodEx.ErrorValue = FormatForDisplay(finPeriodID);
					throw finPeriodEx;
				}
					}
				}

		protected virtual PeriodValidationResult ValidateOrganizationFinPeriodStatus(PXCache sender, object row, FinPeriod finPeriod)
		{
			PeriodValidationResult validationResult = new PeriodValidationResult();

			if (finPeriod.Status == FinPeriod.status.Locked)
			{
				validationResult.AddMessage(
					PXErrorLevel.Error,
					ExceptionType.Locked,
					Messages.FinPeriodIsLockedInCompany,
					FormatForError(finPeriod.FinPeriodID),
					PXAccess.GetOrganizationCD(finPeriod.OrganizationID));

				return validationResult;
			}

			if (finPeriod.Status == FinPeriod.status.Inactive)
			{
				validationResult.AddMessage(
					PXErrorLevel.Error,
					ExceptionType.Inactive,
					Messages.FinPeriodIsInactiveInCompany,
					FormatForError(finPeriod.FinPeriodID),
					PXAccess.GetOrganizationCD(finPeriod.OrganizationID));

				return validationResult;
			}

			if (finPeriod.Status == FinPeriod.status.Closed)
			{
				return HandleErrorThatPeriodIsClosed(sender, finPeriod);
			}

			return validationResult;
		}

		protected PeriodValidationResult HandleErrorThatPeriodIsClosed(
			PXCache sender,
			FinPeriod finPeriod,
			PXErrorLevel errorLevel = PXErrorLevel.Error,
			string errorMessage = Messages.FinPeriodIsClosedInCompany)
		{
			PeriodValidationResult validationResult = new PeriodValidationResult();

			validationResult.AddMessage(
				sender.Graph.GetService<IFinPeriodUtils>().CanPostToClosedPeriod() ? PXErrorLevel.Warning : errorLevel,
				ExceptionType.Closed,
				errorMessage,
				FormatForError(finPeriod.FinPeriodID),
				PXAccess.GetOrganizationCD(finPeriod.OrganizationID));

			return validationResult;

		}

		protected enum ExceptionType
		{
			Custom,
			Closed,
			Inactive,
			Locked,
		}

		protected class PeriodValidationMessage : ProcessingResultMessage
		{
			public ExceptionType ExceptionType { get; set; }

			public PeriodValidationMessage(PXErrorLevel errorLevel, ExceptionType exceptionType, string text)
				: base(errorLevel, text)
			{
				ExceptionType = exceptionType;
			}
		}

		protected class PeriodValidationResult : ProcessingResultBase<PeriodValidationResult, object, PeriodValidationMessage>
		{
			public void AddMessage(PXErrorLevel errorLevel, ExceptionType exceptionType, string message, params object[] args)
			{
				_messages.Add(new PeriodValidationMessage(errorLevel, exceptionType, PXMessages.LocalizeFormatNoPrefix(message, args)));
			}

			public void AddMessage(PXErrorLevel errorLevel, ExceptionType exceptionType, string message)
			{
				_messages.Add(new PeriodValidationMessage(errorLevel, exceptionType, PXMessages.LocalizeNoPrefix(message)));
			}

			public override void AddMessage(PXErrorLevel errorLevel, string message, params object[] args)
			{
				AddMessage(errorLevel, ExceptionType.Custom, message, args);
			}

			public override void AddMessage(PXErrorLevel errorLevel, string message)
			{
				AddMessage(errorLevel, ExceptionType.Custom, message);
			}
		}
		#endregion
	}

	/// <summary>
	/// FinPeriod selector that extends <see cref="FinPeriodSelectorAttribute"/>. 
	/// Displays and accepts only Closed Fin Periods. 
	/// When Date is supplied through aSourceType parameter FinPeriod is defaulted with the FinPeriod for the given date.
	/// </summary>
	public class ClosedPeriodAttribute : FinPeriodSelectorAttribute
	{
		public ClosedPeriodAttribute(Type aSourceType)
			: this(null, aSourceType)
		{
		}

		public ClosedPeriodAttribute()
			: this(null)
		{
		}

		public ClosedPeriodAttribute(Type searchType,
									Type sourceType,
									Type branchSourceType = null,
									Type branchSourceFormulaType = null,
									Type organizationSourceType = null,
									Type useMasterCalendarSourceType = null,
									Type defaultType = null,
									bool redefaultOrRevalidateOnOrganizationSourceUpdated = true,
		                            bool useMasterOrganizationIDByDefault = false)
			: base(searchType ?? typeof(Search<
				FinPeriod.finPeriodID,
				Where<FinPeriod.status, Equal<FinPeriod.status.open>,
					Or<FinPeriod.status, Equal<FinPeriod.status.closed>>>>),
				sourceType,
				branchSourceType: branchSourceType,
				branchSourceFormulaType: branchSourceFormulaType,
				organizationSourceType: organizationSourceType,
				useMasterCalendarSourceType: useMasterCalendarSourceType,
				defaultType: defaultType,
				redefaultOrRevalidateOnOrganizationSourceUpdated:redefaultOrRevalidateOnOrganizationSourceUpdated,
			    useMasterOrganizationIDByDefault: useMasterOrganizationIDByDefault)
		{
		}
	}

	/// <summary>
	/// FinPeriod selector that extends <see cref="FinPeriodSelectorAttribute"/>. 
	/// Displays any periods (active, closed, etc), maybe filtered. 
	/// When Date is supplied through aSourceType parameter FinPeriod is defaulted with the FinPeriod for the given date.
	/// Default columns list includes 'Active' and  'Closed in GL' columns
	/// </summary>
	public class AnyPeriodFilterableAttribute : FinPeriodSelectorAttribute
	{
		protected int _SelAttrIndex = -1;
		public AnyPeriodFilterableAttribute(Type aSourceType)
			: this(null, aSourceType)
		{

		}

		public AnyPeriodFilterableAttribute(Type sourceType, Type[] fieldList)
			: this(null, sourceType, fieldList)
		{
		}

		public AnyPeriodFilterableAttribute(Type searchType, Type sourceType, Type[] fieldList)
			: this(searchType, sourceType, null, null, null, null, null, true, fieldList)
		{ }

		public AnyPeriodFilterableAttribute(Type searchType, 
											Type sourceType, 
											Type branchSourceType = null,
											Type branchSourceFormulaType = null,
											Type organizationSourceType = null,
											Type useMasterCalendarSourceType = null,
											Type defaultType = null,
											bool redefaultOrRevalidateOnOrganizationSourceUpdated = true,
											Type[] fieldList = null,
		                                    Type masterFinPeriodIDType = null)
			: base(searchType,
					sourceType,
					branchSourceType:branchSourceType,
					branchSourceFormulaType:branchSourceFormulaType,
					organizationSourceType:organizationSourceType,
					useMasterCalendarSourceType:useMasterCalendarSourceType,
					defaultType:defaultType,
					redefaultOrRevalidateOnOrganizationSourceUpdated:redefaultOrRevalidateOnOrganizationSourceUpdated,
					fieldList:fieldList,
			        masterFinPeriodIDType: masterFinPeriodIDType)
		{
			this.Initialize();
			this.Filterable = true;
		}

		public AnyPeriodFilterableAttribute()
			: this(null)
		{
		}

		public virtual bool Filterable
		{
			get
			{
				return (_SelAttrIndex == -1) ? false : ((PXSelectorAttribute)_Attributes[_SelAttrIndex]).Filterable;
			}
			set
			{
				if (_SelAttrIndex != -1)
					((PXSelectorAttribute)_Attributes[_SelAttrIndex]).Filterable = value;
			}
		}
		protected virtual void Initialize()
		{
			this._SelAttrIndex = -1;
			foreach (PXEventSubscriberAttribute attr in _Attributes)
			{
				if (attr is PXSelectorAttribute && _SelAttrIndex < 0)
				{
					_SelAttrIndex = _Attributes.IndexOf(attr);
				}				
			}
		}
	}

	/// <summary>
	/// This is a Generic Attribute that Aggregates other attributes and exposes there public properties.
	/// The Attributes aggregated can be of the following types:
	/// - DBFieldAttribute such as PXBDInt, PXDBString, etc.
	/// - PXUIFieldAttribute
	/// - PXSelectorAttribute
	/// - PXDefaultAttribute
	/// </summary>
	public class AcctSubAttribute : PXAggregateAttribute, IPXInterfaceField, IPXCommandPreparingSubscriber, IPXRowSelectingSubscriber
	{
		protected int _DBAttrIndex = -1;
		protected int _NonDBAttrIndex = -1;
		protected int _UIAttrIndex = -1;
		protected int _SelAttrIndex = -1;
		protected int _DefAttrIndex = -1;

		protected PXDBFieldAttribute DBAttribute => _DBAttrIndex == -1 ? null : (PXDBFieldAttribute)_Attributes[_DBAttrIndex];
		protected PXEventSubscriberAttribute NonDBAttribute => _NonDBAttrIndex == -1 ? null : _Attributes[_NonDBAttrIndex];
		protected PXUIFieldAttribute UIAttribute => _UIAttrIndex == -1 ? null : (PXUIFieldAttribute) _Attributes[_UIAttrIndex];
		protected PXDimensionSelectorAttribute SelectorAttribute => _SelAttrIndex == -1 ? null : (PXDimensionSelectorAttribute)_Attributes[_SelAttrIndex];
		protected PXDefaultAttribute DefaultAttribute => _DefAttrIndex == -1 ? null : (PXDefaultAttribute)_Attributes[_DefAttrIndex];

		protected virtual void Initialize()
		{
			_DBAttrIndex = -1;
			_NonDBAttrIndex = -1;
			_UIAttrIndex = -1;
			_SelAttrIndex = -1;
			_DefAttrIndex = -1;

			foreach (PXEventSubscriberAttribute attr in _Attributes)
			{
				if (attr is PXDBFieldAttribute)
				{
					_DBAttrIndex = _Attributes.IndexOf(attr);
					foreach (PXEventSubscriberAttribute sibling in _Attributes)
					{
						if (!object.ReferenceEquals(attr, sibling) && PXAttributeFamilyAttribute.IsSameFamily(attr.GetType(), sibling.GetType()))
						{
							_NonDBAttrIndex = _Attributes.IndexOf(sibling);
							break;
						}
					}
				}
				if (attr is PXUIFieldAttribute)
				{
					_UIAttrIndex = _Attributes.IndexOf(attr);
				}
				if (attr is PXDimensionSelectorAttribute)
				{
					_SelAttrIndex = _Attributes.IndexOf(attr);
				}
				if (attr is PXSelectorAttribute && _SelAttrIndex < 0)
				{
					_SelAttrIndex = _Attributes.IndexOf(attr);
				}
				if (attr is PXDefaultAttribute)
				{
					_DefAttrIndex = _Attributes.IndexOf(attr);
				}
			} 
		}

		public AcctSubAttribute()
		{
			Initialize();
			this.Filterable = true;
		}

		public bool IsDBField { get; set; } = true;

		#region DBAttribute delagation
		public new string FieldName
		{
			get { return DBAttribute?.FieldName; }
			set { DBAttribute.FieldName = value; }
			}

		public bool IsKey
			{
			get { return DBAttribute?.IsKey ?? false; }
			set { DBAttribute.IsKey = value; }
		}

		public bool IsFixed
			{
			get { return ((PXDBStringAttribute) DBAttribute)?.IsFixed ?? false; }
			set
			{
				((PXDBStringAttribute) DBAttribute).IsFixed = value;
				if (NonDBAttribute != null)
					((PXStringAttribute) NonDBAttribute).IsFixed = value;
			}
		}

		public Type BqlField
			{
			get { return DBAttribute?.BqlField; }
			set
			{
				DBAttribute.BqlField = value;
				BqlTable = DBAttribute.BqlTable;
			}
		}

		/// <summary>
		/// Allows to control validation process.
		/// </summary>
		public virtual bool ValidateValue
		{
			get { return SelectorAttribute.ValidateValue; }
			set { SelectorAttribute.ValidateValue = value; }
		}
		#endregion

		#region UIAttribute delagation
		public PXUIVisibility Visibility
		{
			get { return UIAttribute?.Visibility ?? PXUIVisibility.Undefined; }
			set
			{
				if (UIAttribute != null)
					UIAttribute.Visibility = value;
			}
		}

		public bool Visible
		{
			get { return UIAttribute?.Visible ?? true; }
			set
			{
				if (UIAttribute != null)
					UIAttribute.Visible = value;
			}
		}

		public bool Enabled
		{
			get { return UIAttribute?.Enabled ?? true; }
			set
			{
				if (UIAttribute != null)
					UIAttribute.Enabled = value;
			}
		}

		public string DisplayName
		{
			get { return UIAttribute?.DisplayName; }
			set
			{
				if (UIAttribute != null)
					UIAttribute.DisplayName = value;
			}
		}

		public string FieldClass
		{
			get { return UIAttribute?.FieldClass; }
			set
			{
				if (UIAttribute != null)
					UIAttribute.FieldClass = value;
			}
		}

		public bool Required
		{
			get { return UIAttribute?.Required ?? false; }
			set
			{
				if (UIAttribute != null)
					UIAttribute.Required = value;
			}
		}

		public virtual int TabOrder
		{
			get { return UIAttribute?.TabOrder ?? _FieldOrdinal; }
			set
			{
				if (UIAttribute != null)
					UIAttribute.TabOrder = value;
			}
		}

		public virtual PXErrorHandling ErrorHandling
			{
			get { return UIAttribute?.ErrorHandling ?? PXErrorHandling.WhenVisible; }
			set
			{
				if (UIAttribute != null)
					UIAttribute.ErrorHandling = value;
			}
		}
		#endregion

		#region SelectorAttribute delagation
		public virtual Type DescriptionField
			{
			get { return SelectorAttribute?.DescriptionField; }
			set
			{
				if (SelectorAttribute != null)
					SelectorAttribute.DescriptionField = value;
			}
		}

		public virtual bool DirtyRead
		{
			get { return SelectorAttribute?.DirtyRead ?? false; }
			set
			{
				if (SelectorAttribute != null)
					SelectorAttribute.DirtyRead = value;
			}
		}

		public virtual bool CacheGlobal
		{
			get { return SelectorAttribute?.CacheGlobal ?? false; }
			set
			{
				if (SelectorAttribute != null)
					SelectorAttribute.CacheGlobal = value;
			}
		}

		public virtual bool ValidComboRequired
		{
			get { return SelectorAttribute?.ValidComboRequired ?? false; }
			set
			{
				if (SelectorAttribute != null)
					SelectorAttribute.ValidComboRequired = value;
			}
		}

		public virtual bool Filterable
		{
			get { return SelectorAttribute?.Filterable ?? false; }
			set
			{
				if (SelectorAttribute != null)
					SelectorAttribute.Filterable = value;
			}
		}
		#endregion

		#region DefaultAttribute delagation
		public virtual PXPersistingCheck PersistingCheck
		{
			get { return DefaultAttribute?.PersistingCheck ?? PXPersistingCheck.Nothing; }
			set
			{
				if (DefaultAttribute != null)
					DefaultAttribute.PersistingCheck = value;
			}
		}
		#endregion

		#region Implementation

		public virtual void CommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
		{
			e.Expr = new Data.SQLTree.SQLConst(string.Empty);
			e.Cancel = true;
		}

	

		public virtual void RowSelecting(PXCache sender, PXRowSelectingEventArgs e)
		{
			sender.SetValue(e.Row, _FieldOrdinal, null);
		}


		#endregion

		#region Initialization

		public override void GetSubscriber<ISubscriber>(List<ISubscriber> subscribers)
		{
			if (typeof(ISubscriber) == typeof(IPXCommandPreparingSubscriber)
				|| typeof(ISubscriber) == typeof(IPXRowSelectingSubscriber))
			{
				if (IsDBField == false)
				{
					if (NonDBAttribute == null)
					{
						subscribers.Add(this as ISubscriber);
					}
					else
					{
						if (typeof(ISubscriber) == typeof(IPXRowSelectingSubscriber))
						{
							subscribers.Add(this as ISubscriber);
						}
						else
						{
							NonDBAttribute.GetSubscriber(subscribers);
						}
					}

					for (int i = 0; i < _Attributes.Count; i++)
					{
						if (i != _DBAttrIndex && i != _NonDBAttrIndex)
						{
							_Attributes[i].GetSubscriber(subscribers);
						}
					}
				}
				else
				{
					base.GetSubscriber(subscribers);

					if (NonDBAttribute != null)
					{
						subscribers.Remove(NonDBAttribute as ISubscriber);
					}

					subscribers.Remove(this as ISubscriber);
				}
			}
			else
			{
				base.GetSubscriber(subscribers);

				if (NonDBAttribute != null)
				{
					subscribers.Remove(NonDBAttribute as ISubscriber);
				}
			}
		}
		#endregion

		#region IPXInterfaceField Members
		private IPXInterfaceField PXInterfaceField => UIAttribute;

		public string ErrorText
		{
			get { return PXInterfaceField?.ErrorText; }
			set
			{
				if (PXInterfaceField != null)
					PXInterfaceField.ErrorText = value;
			}
		}

		public object ErrorValue
		{
			get { return PXInterfaceField?.ErrorValue; }
			set
			{
				if (PXInterfaceField != null)
					PXInterfaceField.ErrorValue = value;
			}			
		}

		public PXErrorLevel ErrorLevel
		{
			get { return PXInterfaceField?.ErrorLevel ?? PXErrorLevel.Undefined; }
			set
			{
				if (PXInterfaceField != null)
					PXInterfaceField.ErrorLevel = value;
		}
		}

		public PXCacheRights MapEnableRights
		{
			get { return PXInterfaceField?.MapEnableRights ?? PXCacheRights.Select; }
			set
			{
				if (PXInterfaceField != null)
					PXInterfaceField.MapEnableRights = value;
			}
		}

		public PXCacheRights MapViewRights
		{
			get { return PXInterfaceField?.MapViewRights ?? PXCacheRights.Select; }
			set
			{
				if (PXInterfaceField != null)
					PXInterfaceField.MapViewRights = value;
			}
		}

		public bool ViewRights => PXInterfaceField?.ViewRights ?? true;

		public void ForceEnabled() => PXInterfaceField?.ForceEnabled();

		#endregion

	}

	/// <summary>
	/// Branch Field.
	/// </summary>
	/// <remarks>In case your DAC  supports multiple branches add this attribute to the Branch field of your DAC.</remarks>
	public class BranchAttribute : BranchBaseAttribute
	{
		public BranchAttribute(
		    Type sourceType = null, 
		    Type searchType = null, 
		    bool addDefaultAttribute = true, 
		    bool onlyActive = true,
		    bool useDefaulting = true)
            : base(sourceType, searchType, addDefaultAttribute, onlyActive, useDefaulting)
		{
		}
	}

	[PXDBInt()]
	[PXInt]
	[PXUIField(DisplayName = "Branch", FieldClass = _FieldClass)]
	public abstract class BranchBaseAttribute : AcctSubAttribute, IPXFieldSelectingSubscriber
	{
		public const string _FieldClass = "BRANCH";
		public const string _DimensionName = "BRANCH";
		private bool _IsDetail = true;
		private bool _Suppress = false;

		public bool IsDetail
		{
			get
			{
				return this._IsDetail;
			}
			set
			{
				this._IsDetail = value;
			}
		}

		public bool IsEnabledWhenOneBranchIsAccessible { get; set; }

		protected BranchBaseAttribute(
		    Type sourceType, 
		    Type searchType = null, 
		    bool addDefaultAttribute = true, 
		    bool onlyActive = false,
		    bool useDefaulting = true)
			: base()
		{
		    if (sourceType == null && useDefaulting)
		    {
		        sourceType = GetDefaultSourceType();
		    }

		    if (searchType == null)
		    {
		        searchType = typeof(Search2<Branch.branchID,
		            InnerJoin<Organization,
		                On<Branch.organizationID, Equal<Organization.organizationID>>>,
		            Where<MatchWithBranch<Branch.branchID>>>);

		    }
		    if (addDefaultAttribute)
		    {
		        _Attributes.Add(sourceType != null ? new PXDefaultAttribute(sourceType) : new PXDefaultAttribute());
		    }

            if (onlyActive)
			{
				_Attributes.Add(new PXRestrictorAttribute(typeof(Where<Branch.active, Equal<True>>), Messages.BranchInactive));
			}

			if (sourceType == null || !typeof(IBqlField).IsAssignableFrom(sourceType) || sourceType == typeof(AccessInfo.branchID))
			{
				IsDetail = false;
			}

			if (IsDetail)
			{
				_Attributes.Add(new InterBranchRestrictorAttribute(BqlCommand.Compose(
						typeof(Where<>),
						typeof(SameOrganizationBranch<,>),
						typeof(Branch.branchID),
						typeof(Current<>), sourceType)));
			}

			PXDimensionSelectorAttribute attr = 
				new PXDimensionSelectorAttribute(_DimensionName, 
													searchType, 
													typeof(Branch.branchCD), 
													typeof(Branch.branchCD), typeof(Branch.acctName), typeof(Branch.ledgerID), typeof(Organization.organizationName), typeof(Branch.defaultPrinterID));
			attr.ValidComboRequired = true;
			attr.DescriptionField = typeof(Branch.acctName);
			_Attributes.Add(attr);
			_SelAttrIndex = _Attributes.Count - 1;

			Initialize();
		}

	    protected virtual Type GetDefaultSourceType()
	    {
	        return typeof(AccessInfo.branchID);
	    }

		public bool Suppress()
		{
			object[] ids = PXAccess.GetBranches();

			return (ids == null || ids.Length <= 1) && !IsEnabledWhenOneBranchIsAccessible;
		}

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);

			_Suppress = Suppress();
		}

		public override void GetSubscriber<ISubscriber>(List<ISubscriber> subscribers)
		{
			base.GetSubscriber<ISubscriber>(subscribers);
			if (typeof(ISubscriber) == typeof(IPXFieldSelectingSubscriber))
			{
				subscribers.Remove(this as ISubscriber);
				subscribers.Add(this as ISubscriber);
			}
		}

		public virtual void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			if (_Suppress && e.ReturnState is PXFieldState)
			{
				PXFieldState state = (PXFieldState)e.ReturnState;

				state.Enabled = false;
				if (_IsDetail)
				{
					state.Visible = false;
					state.Visibility = PXUIVisibility.Invisible;
				}
			}
		}
	}

	/// <summary>
	/// Base Attribute for AccountCD field. Aggregates PXFieldAttribute, PXUIFieldAttribute and PXDimensionAttribute.
	/// PXDimensionAttribute selector has no restrictions and returns all records.
	/// </summary>
	[PXDBString(10, IsUnicode = true, InputMask = "")]
	[PXUIField(DisplayName = "Account", Visibility = PXUIVisibility.Visible)]
	public sealed class AccountRawAttribute : AcctSubAttribute
	{
		private string _DimensionName = "ACCOUNT";

		public AccountRawAttribute()
			: base()
		{
			PXDimensionAttribute attr = new PXDimensionAttribute(_DimensionName);
			attr.ValidComboRequired = false;
			_Attributes.Add(attr);
			_SelAttrIndex = _Attributes.Count - 1;
		}
	}

	/// <summary>
	/// Base Attribute for SubCD field. Aggregates PXFieldAttribute, PXUIFieldAttribute and PXDimensionAttribute.
	/// PXDimensionAttribute selector has no restrictions and returns all records.
	/// </summary>
	[PXDBString(30, IsUnicode = true, InputMask = "")]
	[PXUIField(DisplayName = "Subaccount", Visibility = PXUIVisibility.Visible, FieldClass = _DimensionName)]
	public class SubAccountRawAttribute : AcctSubAttribute
	{
		protected const string _DimensionName = "SUBACCOUNT";

		public SubAccountRawAttribute()
			: base()
		{
			PXDimensionAttribute attr = new PXDimensionAttribute(_DimensionName);
			attr.ValidComboRequired = false;
			_Attributes.Add(attr);
			_SelAttrIndex = _Attributes.Count - 1;			
		}

		public SubAccountRawAttribute(PX.Data.DimensionLookupMode lookupMode)
			: base()
		{
			PXDimensionValueLookupModeAttribute attr = new PXDimensionValueLookupModeAttribute(_DimensionName, lookupMode);
			attr.ValidComboRequired = false;
			_Attributes.Add(attr);
			_SelAttrIndex = _Attributes.Count - 1;
		}

		protected bool _SuppressValidation = false;
		public bool SuppressValidation
		{
			get
			{
				return this._SuppressValidation;
			}
			set
			{
				this._SuppressValidation = value;
			}
		}
		public override void GetSubscriber<ISubscriber>(List<ISubscriber> subscribers) 
		{
			base.GetSubscriber<ISubscriber>(subscribers);
			if (this._SuppressValidation)
			{
				if (typeof(ISubscriber) == typeof(IPXFieldVerifyingSubscriber) && (_SelAttrIndex >= 0))
				{
					subscribers.Remove(_Attributes[_SelAttrIndex] as ISubscriber);
				}
			}
		} 
	}

	/// <summary>
	/// Attribute suppress the dimension's lookup mode
	/// </summary>
	public sealed class PXDimensionValueLookupModeAttribute : PXDimensionAttribute
	{

		public PX.Data.DimensionLookupMode LookupMode
		{
			get;
			set;
		}
		public PXDimensionValueLookupModeAttribute(string dimension, PX.Data.DimensionLookupMode lookupMode) : base(dimension)
		{
			LookupMode = lookupMode;
		}
		/// <exclude/>
		public override void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			if (_AttributeLevel == PXAttributeLevel.Item || e.IsAltered)
			{
				e.ReturnState = PXSegmentedState.CreateInstance(e.ReturnState, _FieldName, _Definition != null && _Definition.Dimensions.ContainsKey(_Dimension) ? _Definition.Dimensions[_Dimension] : new PXSegment[0],
					!(e.ReturnState is PXFieldState) || String.IsNullOrEmpty(((PXFieldState)e.ReturnState).ViewName) ? "_" + _Dimension + "_Segments_" : null,
					LookupMode,
					ValidComboRequired, _Wildcard);
				((PXSegmentedState)e.ReturnState).IsUnicode = true;
				((PXSegmentedState)e.ReturnState).DescriptionName = typeof(SegmentValue.descr).Name;
			}
		}
	}

	/// <summary>
	/// Base Attribute for SubCD field. Aggregates PXFieldAttribute, PXUIFieldAttribute and PXDimensionAttribute.
	/// PXDimensionAttribute selector returns only records that are visible for the current user.
	/// </summary>
	[PXDBString(30, InputMask = "")]
	[PXUIField(DisplayName = "Subaccount", Visibility = PXUIVisibility.Visible, FieldClass = SubAccountAttribute.DimensionName)]
	public sealed class SubAccountRestrictedRawAttribute : SubAccountRawAttribute, IPXFieldSelectingSubscriber
	{
		public SubAccountRestrictedRawAttribute()
			:base(Data.DimensionLookupMode.BySegmentsAndAllAvailableSegmentValues)
		{
		}

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);
			PX.SM.Users user = PXSelect<PX.SM.Users,
				Where<PX.SM.Users.username, Equal<Current<AccessInfo.userName>>>>
				.Select(sender.Graph);
			if (user != null && user.GroupMask != null)
			{
				((PXDimensionValueLookupModeAttribute)_Attributes[_SelAttrIndex]).Restrictions = new GroupHelper.ParamsPair[][] { GroupHelper.GetParams(typeof(PX.SM.Users), typeof(CS.SegmentValue), user.GroupMask) };
			}
			sender.Graph.Views["_" + _DimensionName + "_RestrictedSegments_"] =
				new PXView(sender.Graph, true, new Select<PXDimensionValueLookupModeAttribute.SegmentValue>(), (PXSelectDelegate<short?, string>)((PXDimensionValueLookupModeAttribute)_Attributes[_SelAttrIndex]).SegmentSelect);
		}
		public void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			if (e.ReturnState is PXFieldState)
			{
				((PXFieldState)e.ReturnState).ViewName = "_" + _DimensionName + "_RestrictedSegments_";
			}
		}
		public override void GetSubscriber<ISubscriber>(List<ISubscriber> subscribers)
		{
			base.GetSubscriber<ISubscriber>(subscribers);
			if (typeof(ISubscriber) == typeof(IPXFieldSelectingSubscriber))
			{
				subscribers.Remove(this as ISubscriber);
				subscribers.Add(this as ISubscriber);
			}			
		}
		
	}

    [PXInt]
		[PXUIField(DisplayName = "Account", Visibility = PXUIVisibility.Visible, FieldClass = DimensionName)]
	public class UnboundAccountAttribute : AcctSubAttribute
	{
		public const string DimensionName = "ACCOUNT";

		public UnboundAccountAttribute()
			: base()
		{
			Type SearchType = typeof(Search<Account.accountID,
				Where2<Match<Current<AccessInfo.userName>>,
				And<Where<Current<GLSetup.ytdNetIncAccountID>, IsNull,
				Or<Account.accountID, NotEqual<Current<GLSetup.ytdNetIncAccountID>>>>>>>);
			PXDimensionSelectorAttribute attr = new PXDimensionSelectorAttribute(DimensionName, SearchType, typeof(Account.accountCD));
			attr.CacheGlobal = true;
			_Attributes.Add(attr);
			_SelAttrIndex = _Attributes.Count - 1;
			DescriptionField = typeof(Account.description);
			this.Filterable = true;
		}
	}	
	

	/// <summary>
	/// Represents Account Field
	/// The Selector will return all accounts.
	/// This attribute also tracks currency (which is supplied as curyField parameter) for the Account and
	/// raises an error in case Denominated GL Account currency is different from transaction currency.
	/// </summary>
	[PXRestrictor(typeof(Where<True, Equal<True>>), Messages.AccountInactive, ReplaceInherited = true)]
	public class AccountAnyAttribute : AccountAttribute
	{		
		public AccountAnyAttribute()
			: base(null) 
		{
			this.Filterable = true;
		}

		public override void Verify(PXCache sender, Account item, object row)
		{
		}
	}

	/// <summary>
	/// Represents Account Field.
	/// The Selector will return all accounts except 'YTD Net Income' account.
	/// This attribute also tracks currency (which is supplied as curyField parameter) for the Account and
	/// raises an error in case Denominated GL Account currency is different from transaction currency.
	/// </summary>
	[PXUIField(DisplayName = "Account", Visibility = PXUIVisibility.Visible, FieldClass = DimensionName)]	
	[PXDBInt]
	[PXInt]
	[PXRestrictor(typeof(Where<Account.active, Equal<True>>), Messages.AccountInactive)]
	[PXRestrictor(typeof(Where<Where<Current<GLSetup.ytdNetIncAccountID>, IsNull,
							Or<Account.accountID, NotEqual<Current<GLSetup.ytdNetIncAccountID>>>>>), Messages.YTDNetIncomeSelected)]
	public class AccountAttribute : AcctSubAttribute, IPXFieldVerifyingSubscriber, IPXRowPersistingSubscriber
	{	
		public class CuryException : PXSetPropertyException
		{
			public CuryException()
				: base(Messages.AccountCuryNotTransactionCury)
			{ 
			}

			public CuryException(SerializationInfo info, StreamingContext context)
				: base(info, context)
			{
				PXReflectionSerializer.RestoreObjectProps(this, info);
			}

			public override void GetObjectData(SerializationInfo info, StreamingContext context)
			{
				PXReflectionSerializer.GetObjectData(this, info);
				base.GetObjectData(info, context);
			}


		}


		public const string DimensionName = "ACCOUNT";

		private const string glSetup = "_GLSetup";

		public class dimensionName : PX.Data.BQL.BqlString.Constant<dimensionName>
		{
			public dimensionName() : base(DimensionName) { ;}
		}

		public bool SuppressCurrencyValidation
		{
			get { return _SuppressCurrencyValidation; }
			set { _SuppressCurrencyValidation = value; }
		}

		private bool _SuppressCurrencyValidation;

		public Type  LedgerID
		{
			get { return _ledgerID; }
			set { _ledgerID = value; }
		}

		private string _ControlAccountForModule;
		public string ControlAccountForModule
		{
			get
			{
				return _ControlAccountForModule;
			}
			set
			{
				_ControlAccountForModule = value;
				if (value != null)
				{
					var restrictor = new PXRestrictorAttribute(typeof(Where<Account.isCashAccount, Equal<False>>), null);
					_Attributes.Add(restrictor);
				}
			}
		}

		/// <summary>
		/// Field, that returns module name, that is allowed to be control module for account, when AvoidControlAccounts is set to TRUE.
		/// </summary>
		public Type AllowControlAccountForModuleField { get; set; }

		public bool AvoidControlAccounts { get; set; }

		private Type _ledgerID;
		private Type _branchID;
		
		private Type _curyKeyField;
		
		public AccountAttribute()
			: this(null)
		{			
		}

		public AccountAttribute(Type branchID)
			: this(branchID, typeof(Search<Account.accountID, Where<Match<Current<AccessInfo.userName>>>>))
		{			
		}

		public AccountAttribute(Type branchID, Type SearchType)
		{
			if(SearchType == null)
			{
				throw new PXArgumentException("SearchType", ErrorMessages.ArgumentNullException);
			}

			if (branchID != null)
			{
				_branchID = branchID; 

				List<Type> items = new List<Type> { SearchType.GetGenericTypeDefinition() };
				items.AddRange(SearchType.GetGenericArguments());
				for (int i = 0; i < items.Count; i++)
				{
					if (typeof(IBqlWhere).IsAssignableFrom(items[i]))
					{
						items[i] = BqlCommand.Compose(
							typeof(Where2<,>),
                            typeof(Where<,,>),
                            typeof(Account.isCashAccount), typeof(Equal<True>), typeof(Or<>),
							typeof(Match<>), typeof(Optional<>), branchID,
							typeof(And<>),
							items[i]);
						SearchType = BqlCommand.Compose(items.ToArray());
					}
				}
			}

			PXDimensionSelectorAttribute attr = new PXDimensionSelectorAttribute(DimensionName, SearchType, typeof(Account.accountCD));
			attr.CacheGlobal = true;
			attr.DescriptionField = typeof(Account.description);
			_Attributes.Add(attr);
			_SelAttrIndex = _Attributes.Count - 1;
			this.Filterable = true;
		}

		private static Type SearchKeyField(PXCache sender)
		{
			return sender.GetBqlField("CuryInfoID");
		}

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);
			sender.Graph.FieldSelecting.AddHandler(sender.GetItemType(), _FieldName, FieldSelecting);				
			_curyKeyField = (SuppressCurrencyValidation) ? null : SearchKeyField(sender);

			if (!PXAccess.FeatureInstalled<FeaturesSet.financialModule>())
			{
				PXDimensionSelectorAttribute.SetValidCombo(sender, _FieldName, false);
				if (!sender.Graph.Views.Caches.Contains(typeof(Account)))
					sender.Graph.Views.Caches.Add(typeof(Account));

				sender.Graph.FieldDefaulting.AddHandler(sender.GetItemType(), _FieldName, Feature_FieldDefaulting);
			}
			else
			{
				if (_curyKeyField != null)
				{
					sender.Graph.RowUpdated.AddHandler<CurrencyInfo>(CurrencyInfoRowUpdated);
				}

				if (_branchID != null)
					sender.Graph.FieldUpdated.AddHandler(BqlCommand.GetItemType(_branchID), _branchID.Name, BranchFieldUpdated);


				if (((PXDimensionSelectorAttribute) _Attributes[_SelAttrIndex]).CacheGlobal == false)
				{
					sender.Graph.FieldUpdating.AddHandler(sender.GetItemType(), _FieldName, FieldUpdating);
					sender.Graph.FieldUpdating.RemoveHandler(sender.GetItemType(), _FieldName, ((PXDimensionSelectorAttribute) _Attributes[_SelAttrIndex]).FieldUpdating);
				}
			}
		}

		public virtual void Feature_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (!e.Cancel)
			{
				if (this.Definitions.DefaultAccountID == null)
				{					
					object newValue = Account.Default;
					sender.RaiseFieldUpdating(_FieldName, e.Row, ref newValue);
					e.NewValue = newValue;										
				}
				else
				{
					e.NewValue = this.Definitions.DefaultAccountID;
				}

				e.Cancel = true;
			}
		}

		public virtual void Verify(PXCache sender, Account item, object row)
		{
			if (_curyKeyField != null && sender.Fields.Contains(typeof(CurrencyInfo.curyID).Name))
			{
				if (item != null && item.CuryID != null)
				{
					string CuryID;
					using (new PXCuryViewStateScope(sender.Graph))
					{
						object value = sender.GetValueExt(row, typeof(CurrencyInfo.curyID).Name);
						CuryID = PXFieldState.UnwrapValue(value)?.ToString()?.TrimEnd();
					}

					if (!object.Equals(item.CuryID, CuryID))
					{
						string Ledger_BalanceType = "A";

						if (_ledgerID != null)
						{
							int ledgerID = (int)PXView.FieldGetValue(sender, row, BqlCommand.GetItemType(_ledgerID), _ledgerID.Name);
							Ledger ledger = PXSelect<Ledger, Where<Ledger.ledgerID, Equal<Required<Ledger.ledgerID>>>>.Select(sender.Graph, ledgerID);

							if (ledger != null)
							{
								Ledger_BalanceType = ledger.BalanceType;
							}
						}

						if (Ledger_BalanceType != "R")
						{
							throw new CuryException();
						}
					}
				}
			}
		}

		#region Control Account rules
		private void VerifyControlAccountRules(PXCache sender, EventArgs e)
		{
			var verifying = e as PXFieldVerifyingEventArgs;
			var updated = e as PXFieldUpdatedEventArgs;
			var persisting = e as PXRowPersistingEventArgs;
			var rowselected = e as PXRowSelectedEventArgs;
			var row = verifying?.Row ?? updated?.Row ?? persisting?.Row ?? rowselected?.Row;

			if (ControlAccountForModule != null)
			{
				VerifyControlAccount(sender, _FieldName, e, ControlAccountForModule);
			}
			else if (AvoidControlAccounts)
			{
				var account = GetAccount(sender, _FieldName, e);
				if (account == null)
					return;

				if (AllowControlAccountForModuleField != null)
				{
					var value = sender.GetValueExt(row, AllowControlAccountForModuleField.Name);
					var allowedModule = (value is PXFieldState) ? (string)((PXFieldState)value).Value : (string)value;

					if (!string.IsNullOrEmpty(allowedModule))
					{
						if (allowedModule == ControlAccountModule.ANY || account.ControlAccountModule == allowedModule)
							return;
					}
				}

				VerifyAccountIsNotControl(sender, _FieldName, e, account);
			}
		}

		public static void VerifyControlAccount<T>(PXCache cache, EventArgs e, string expectedModule, Account account = null) where T : IBqlField
		{
			VerifyControlAccount(cache, typeof(T).Name, e, expectedModule, account);
		}
		public static void VerifyControlAccount(PXCache cache, string fieldName, EventArgs e, string expectedModule, Account account = null)
		{
			if (cache == null || e == null || SkipControlAccountValidation(e)) return;

			var verifying = e as PXFieldVerifyingEventArgs;
			var updated = e as PXFieldUpdatedEventArgs;
			var persisting = e as PXRowPersistingEventArgs;
			var rowselected = e as PXRowSelectedEventArgs;

			var row = verifying?.Row ?? updated?.Row ?? persisting?.Row ?? rowselected?.Row;
			if (account == null)
			{
				var accountID = (verifying != null) ? verifying.NewValue : cache.GetValue(row, fieldName);
				if (accountID == null) return;
				account = (Account)PXSelectorAttribute.Select(cache, row, fieldName, accountID) ?? (Account)PXSelect<Account>.Search<Account.accountCD>(cache.Graph, accountID);
			}
			if (account == null) return;

			try
			{
				VerifyControlAccount(account, expectedModule);
			}
			catch (PXSetPropertyException ex)
			{
				cache.RaiseExceptionHandling(fieldName, row, account.AccountCD, ex);

				if (ex.ErrorLevel >= PXErrorLevel.Error && verifying != null)
				{
					verifying.Cancel = true;
				}
				else if (ex.ErrorLevel >= PXErrorLevel.Error && persisting != null)
				{
					persisting.Cancel = true;
					throw ex;
				}
			}
		}
		public static void VerifyControlAccount(Account account, string expectedModule)
		{
			if (account.IsCashAccount == true)
			{
				throw new PXSetPropertyException(CA.Messages.CashAccountCanNotBeUsedAsControl, PXErrorLevel.Error, account.AccountCD, account.ControlAccountModule);
			}
			else if (account.ControlAccountModule == null)
			{
				throw new PXSetPropertyException(Messages.AccountIsNotControlForModule, PXErrorLevel.Warning, account.AccountCD, expectedModule);
			}
			else if (account.ControlAccountModule != expectedModule)
			{
				throw new PXSetPropertyException(Messages.AccountIsControlForAnotherModule, PXErrorLevel.Error, account.AccountCD, account.ControlAccountModule, expectedModule);
			}
		}

		public static void VerifyAccountIsNotControl<T>(PXCache cache, EventArgs e, Account account = null, bool throwOnVerifying = false) where T : IBqlField
		{
			VerifyAccountIsNotControl(cache, typeof(T).Name, e, account, throwOnVerifying);
		}
		public static void VerifyAccountIsNotControl(PXCache cache, string fieldName, EventArgs e, Account account = null, bool throwOnVerifying = false)
		{
			if (cache == null || e == null || SkipControlAccountValidation(e)) return;

			var verifying = e as PXFieldVerifyingEventArgs;
			var updated = e as PXFieldUpdatedEventArgs;
			var persisting = e as PXRowPersistingEventArgs;
			var rowselected = e as PXRowSelectedEventArgs;

			var row = verifying?.Row ?? updated?.Row ?? persisting?.Row ?? rowselected?.Row;
			if (account == null)
			{
				var accountID = (verifying != null) ? verifying.NewValue : cache.GetValue(row, fieldName);
				if (accountID == null) return;
				account = (Account)PXSelectorAttribute.Select(cache, row, fieldName, accountID) ?? (Account)PXSelect<Account>.Search<Account.accountCD>(cache.Graph, accountID);
			}
			if (account == null) return;

			try
			{
				VerifyAccountIsNotControl(account);
			}
			catch (PXSetPropertyException ex)
			{
				cache.RaiseExceptionHandling(fieldName, row, account.AccountCD, ex);

				if (ex.ErrorLevel >= PXErrorLevel.Error && verifying != null)
				{
					verifying.Cancel = true;
					if (throwOnVerifying)
					{
						var state = cache.GetStateExt(row, fieldName) as PXFieldState;
						if (state != null)
							verifying.NewValue = state.Value;
						throw ex;
					}
				}
				else if (ex.ErrorLevel >= PXErrorLevel.Error && persisting != null)
				{
					persisting.Cancel = true;
					throw ex;
				}
			}
		}
		public static void VerifyAccountIsNotControl(Account account)
		{
			if (account?.ControlAccountModule == null) return;

			if (account.AllowManualEntry == true)
			{
				throw new PXSetPropertyException(Messages.AccountIsControlPostedAllow, PXErrorLevel.Warning, account.AccountCD, account.ControlAccountModule);
			}
			else
			{
				throw new PXSetPropertyException(Messages.AccountIsControlPostedDisable, PXErrorLevel.Error, account.AccountCD, account.ControlAccountModule);
			}
		}

		private static bool SkipControlAccountValidation(EventArgs e)
		{
			var verifying = e as PXFieldVerifyingEventArgs;
			var updated = e as PXFieldUpdatedEventArgs;
			var persisting = e as PXRowPersistingEventArgs;
			var rowselected = e as PXRowSelectedEventArgs;

			return
				verifying == null && updated == null && persisting == null && rowselected == null ||
				verifying?.Cancel == true ||
				persisting?.Cancel == true ||
				persisting != null && persisting.Operation != PXDBOperation.Insert && persisting.Operation != PXDBOperation.Update;
		}

		public static Account GetAccount(PXCache cache, string fieldName, EventArgs e)
		{
			if (cache == null || e == null)
				return null;

			var verifying = e as PXFieldVerifyingEventArgs;
			var updated = e as PXFieldUpdatedEventArgs;
			var persisting = e as PXRowPersistingEventArgs;
			var rowselected = e as PXRowSelectedEventArgs;

			var row = verifying?.Row ?? updated?.Row ?? persisting?.Row ?? rowselected?.Row;

			var accountID = (verifying != null) ? verifying.NewValue : cache.GetValue(row, fieldName);
			if (accountID == null)
				return null;

			var account =
				(Account)PXSelectorAttribute.Select(cache, row, fieldName, accountID) ??
				(Account)PXSelect<Account>.Search<Account.accountCD>(cache.Graph, accountID);
			return account;
		}
		#endregion

		public virtual void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			PXView glSetupView = new PXView(sender.Graph, false, BqlCommand.CreateInstance(typeof(Select<GLSetup>)));
			if (!sender.Graph.Views.ContainsKey(glSetup))
				sender.Graph.Views.Add(glSetup, glSetupView);
			if (glSetupView.Cache.Current == null)
				glSetupView.Cache.Current = (GLSetup)PXSelect<GLSetup>.SelectWindowed(sender.Graph, 0, 1);
		}

		public virtual void FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			if (!e.Cancel && e.NewValue != null)
			{
				PXFieldUpdating fu = ((PXDimensionSelectorAttribute)_Attributes[_SelAttrIndex]).GetAttribute<PXDimensionAttribute>().FieldUpdating;
				fu(sender, e);
				e.Cancel = false;

				Account item = PXSelect<Account, Where2<Match<Current<AccessInfo.userName>>, And<Account.accountCD, Equal<Required<Account.accountCD>>>>>.Select(sender.Graph, e.NewValue);

				if (item == null)
				{
					fu = ((PXDimensionSelectorAttribute)_Attributes[_SelAttrIndex]).GetAttribute<PXSelectorAttribute>().SubstituteKeyFieldUpdating;
					fu(sender, e);
				}
				else
				{
					e.NewValue = item.AccountID;
				}
			}
		}

		private void CheckData(PXCache cache, object data)
		{
			object accountID = cache.GetValue(data, _FieldName);
			if (accountID != null)
			{
				cache.RaiseExceptionHandling(_FieldName, data, null, null);
				try
				{
					Account item = (Account)PXSelectorAttribute.Select(cache, data, _FieldName);
					if (item != null)
					{
						Verify(cache, item, data);
					}
				}
				catch (CuryException ee)
				{
					object val = cache.GetValueExt(data, FieldName);
					if (val is PXFieldState)
					{
						val = ((PXFieldState)val).Value;
					}
					cache.RaiseExceptionHandling(_FieldName, data, val, ee);
				}
				catch (PXSetPropertyException)
				{ }
			}
		}

		private void CurrencyInfoRowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			if (!sender.ObjectsEqual<CurrencyInfo.curyID>(e.Row, e.OldRow))
		{
			PXView siblings = CurrencyInfoAttribute.GetView(sender.Graph, BqlCommand.GetItemType(_curyKeyField), _curyKeyField);
			if (siblings != null)
			{
				PXCache cache = siblings.Cache;
				foreach (object data in siblings.SelectMultiBound(new object[] { e.Row }))
				{
					CheckData(cache, data);
				}
			}			
		}
		}

		protected virtual void BranchFieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			PXFieldState state = (PXFieldState)sender.GetValueExt(e.Row, _FieldName);
			if (state != null && state.Value != null)
			{
				Account item = (Account)PXSelectorAttribute.Select(sender, e.Row, _FieldName);
				if (item == null || item.IsCashAccount == false)
				{
					sender.SetValue(e.Row, _FieldName, null);
					sender.SetValueExt(e.Row, _FieldName, state.Value);
				}
			}
		}

		public virtual void FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			VerifyControlAccountRules(sender, e);

			if (!e.Cancel && e.NewValue != null)
			{
				//Account item = PXSelect<Account, Where2<Match<Current<AccessInfo.userName>>, And<Account.accountID, Equal<Required<Account.accountID>>>>>.Select(sender.Graph, e.NewValue);
				Account item = (Account)PXSelectorAttribute.Select(sender, e.Row, _FieldName, e.NewValue);

				if (item != null && !sender.Graph.UnattendedMode)
				{
					try
					{
						Verify(sender, item, e.Row);
					}
					catch (PXSetPropertyException)
					{
						e.NewValue = item.AccountCD;
						e.Cancel = true;
						throw;
					}
				}
			}

			if (_UIAttrIndex != -1)
			{
				((IPXFieldVerifyingSubscriber)_Attributes[_UIAttrIndex]).FieldVerifying(sender, e);
			}
		}

		public virtual void RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			VerifyControlAccountRules(sender, e);

			if (!e.Cancel && _curyKeyField != null && ((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert || (e.Operation & PXDBOperation.Command) == PXDBOperation.Update))
			{
				CheckData(sender, e.Row);
			}
		}

		#region Default SiteID
		protected virtual Definition Definitions
		{
			get
			{
				Definition defs = PX.Common.PXContext.GetSlot<Definition>();
				if (defs == null)
				{
					defs = PX.Common.PXContext.SetSlot<Definition>(PXDatabase.GetSlot<Definition>("Account.Definition", typeof(Account)));
				}
				return defs;
			}
		}

		protected class Definition : IPrefetchable
		{
			private int? _DefaultAccountID;
			public int? DefaultAccountID
			{
				get { return _DefaultAccountID; }
			}

			public void Prefetch()
			{
				using (PXDataRecord record = PXDatabase.SelectSingle<Account>(
					new PXDataField<Account.accountID>(),
					new PXDataFieldOrder<Account.accountID>()))
				{
					_DefaultAccountID = null;
					if (record != null)
						_DefaultAccountID = record.GetInt32(0);
				}
			}
		}
		#endregion

		public static Account GetAccount(PXGraph graph, int? accountID)
		{
			if (accountID == null)
				throw new ArgumentNullException("accountID");

			var account = (Account)PXSelect<Account,
								Where<Account.accountID, Equal<Required<Account.accountID>>>>
								.Select(graph, accountID);

			if (account == null)
			{
				throw new PXException(Messages._0_WithID_1_DoesNotExists, EntityHelper.GetFriendlyEntityName(typeof(Account)),
					accountID);
			}

			return account;
		}
	}
	[PXRestrictor(typeof(Where<Account.curyID, IsNull, And<Account.isCashAccount, Equal<boolFalse>>>), Messages.AccountCanNotBeDenominated)]
	public class PXNonCashAccountAttribute : AccountAttribute 
	{}

	public class AvoidControlAccountsAttribute : PXEventSubscriberAttribute, IPXFieldVerifyingSubscriber, IPXRowPersistingSubscriber
	{
		public virtual void FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if (e.Row != null && !e.Cancel && e.NewValue != null)
			{
				AccountAttribute.VerifyAccountIsNotControl(sender, _FieldName, e);
			}
		}

		public virtual void RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			if (e.Row != null && !e.Cancel)
			{
				AccountAttribute.VerifyAccountIsNotControl(sender, _FieldName, e);
			}
		}
	}

	[Obsolete("Class to be removed in 6.0")]
	[PXInt()]
	[PXUIField(DisplayName = "Subaccount", Visibility = PXUIVisibility.Visible, FieldClass = DimensionName)]
	public sealed class UnboundSubAccountAttribute : AcctSubAttribute
	{
		public const string DimensionName = "SUBACCOUNT";
		private Type _AccountType;

		public UnboundSubAccountAttribute()
			: base()
		{
			Type SearchType = typeof(Search<Sub.subID, Where<Match<Current<AccessInfo.userName>>>>);
			PXDimensionSelectorAttribute attr = new PXDimensionSelectorAttribute(DimensionName, SearchType, typeof(Sub.subCD));
			attr.CacheGlobal = true;
			_Attributes.Add(attr);
			_SelAttrIndex = _Attributes.Count - 1;
			this.Filterable = true;
		}

		public UnboundSubAccountAttribute(Type AccountType)
			: base()
		{
			_AccountType = AccountType;
			Type SearchType = BqlCommand.Compose(
				typeof(Search<,>),
				typeof(Sub.subID),
				typeof(Where2<,>),
				typeof(Match<>),
				typeof(Optional<>),
				AccountType,
				typeof(And<>),
				typeof(Match<>),
				typeof(Current<AccessInfo.userName>)
				);
			PXDimensionSelectorAttribute attr = new PXDimensionSelectorAttribute(DimensionName, SearchType, typeof(Sub.subCD));
			attr.CacheGlobal = true;
			_Attributes.Add(attr);
			_SelAttrIndex = _Attributes.Count - 1;
			this.Filterable = true;
		}

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);
			sender.Graph.CommandPreparing.AddHandler(sender.GetItemType(), _AccountType.Name, Account_CommandPreparing);
		}

		public void Account_CommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
		{
			//required for PXView.GetResult()
			if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Select)
			{
				e.DataType = PXDbType.Int;
				e.DataValue = e.Value;
				e.DataLength = 4;
			}
		}
	}

	/// <summary>
	/// Represents Subaccount Field
	/// Subaccount field usually exists in pair with Account field. Use AccountType argument to specify the respective Account field.
	/// </summary>
	[PXDBInt]
	[PXInt]
	[PXUIField(DisplayName = "Subaccount", Visibility = PXUIVisibility.Visible, FieldClass = DimensionName)]
	[PXRestrictor(typeof(Where<Sub.active, Equal<True>>), Messages.SubaccountInactive, typeof(Sub.subCD))]
	public class SubAccountAttribute : AcctSubAttribute, IPXRowInsertingSubscriber
	{
		#region State

		public const string DimensionName = "SUBACCOUNT";

		public class dimensionName : PX.Data.BQL.BqlString.Constant<dimensionName>
		{
			public dimensionName() : base(DimensionName) {}
		}
		private readonly Type _branchID;
		private readonly Type _accounType;
		#endregion

		#region Ctor

		public SubAccountAttribute() : this(null) {}

		public SubAccountAttribute(Type AccountType) : this(AccountType, null) {}

		public SubAccountAttribute(Type AccountType, Type BranchType, bool AccountAndBranchRequired=false)
		{
			_branchID = BranchType;
			_accounType = AccountType;

			BqlCommand search = BqlCommand.CreateInstance(typeof(Search<Sub.subID, Where<Match<Current<AccessInfo.userName>>>>));
			if (AccountType != null)
			{
				search = search.WhereAnd(GetIsNullAndMatchWhere(AccountType, AccountAndBranchRequired));

				if (BranchType != null)
				{
					search = search.WhereAnd(GetIsNullAndMatchWhere(BranchType, AccountAndBranchRequired));
				}
			}

			PXDimensionSelectorAttribute attr = new PXDimensionSelectorAttribute(DimensionName, search.GetType(), typeof (Sub.subCD))
			{
				CacheGlobal = true,
				DescriptionField = typeof (Sub.description)
			};
			_Attributes.Add(attr);
			_SelAttrIndex = _Attributes.Count - 1;
			Filterable = true;
		}

		private static Type GetIsNullAndMatchWhere(Type entityType, bool IsRequired)
		{
			Type where = BqlCommand.Compose(typeof(Where<>), typeof(Match<>), typeof(Optional<>), entityType);
			if (!IsRequired)
			{
				where = BqlCommand.Compose(typeof(Where<,,>), typeof(Optional<>), entityType, typeof(IsNull), typeof(Or<>), where);
			}
			return where;
		}

		#endregion

		#region Runtime

		public override void CacheAttached(PXCache sender)
		{
			if (!PXAccess.FeatureInstalled<FeaturesSet.subAccount>())
			{
				((PXDimensionSelectorAttribute)this._Attributes[_Attributes.Count - 1]).ValidComboRequired = false;
				sender.Graph.FieldDefaulting.AddHandler(sender.GetItemType(), _FieldName, FieldDefaulting);
			}

			base.CacheAttached(sender);
			if (_branchID != null)
				sender.Graph.FieldUpdated.AddHandler(BqlCommand.GetItemType(_branchID), _branchID.Name, RelatedFieldUpdated);
			if (_accounType != null)
				sender.Graph.FieldUpdated.AddHandler(BqlCommand.GetItemType(_accounType), _accounType.Name, RelatedFieldUpdated);
			//should execute before PXDimensionSelector RowPersisting()
			sender.Graph.RowPersisting.AddHandler(sender.GetItemType(), RowPersisting);
		}

		#endregion

		#region Implementation

		public virtual void RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			int? KeyToAbort = (int?)sender.GetValue(e.Row, _FieldOrdinal);

			if (((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert || (e.Operation & PXDBOperation.Command) == PXDBOperation.Update) &&
					KeyToAbort == null && !PXAccess.FeatureInstalled<FeaturesSet.subAccount>())
			{
				KeyToAbort = GetDefaultSubID(sender, e.Row);
				sender.SetValue(e.Row, _FieldName, KeyToAbort);
				PXUIFieldAttribute.SetError(sender, e.Row, _FieldName, null);
			}

			if (((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert ||
				 (e.Operation & PXDBOperation.Command) == PXDBOperation.Update) && KeyToAbort < 0)
			{
				PXCache cache = sender.Graph.Caches[typeof(Sub)];
				PXSelectBase<Sub> cmd = new PXSelectReadonly<Sub, Where<Sub.subCD, Equal<Current<Sub.subCD>>>>(sender.Graph);
				Sub persisteditem = null;

				foreach (Sub item in cache.Inserted)
				{
					if (object.Equals(item.SubID, KeyToAbort))
					{
						if ((persisteditem = (Sub)cmd.View.SelectSingleBound(new object[] { item })) != null)
						{
							//place in _JustPersisted dictionary
							cache.RaiseRowPersisting(item, PXDBOperation.Insert);
							cache.RaiseRowPersisted(persisteditem, PXDBOperation.Insert, PXTranStatus.Open, null);

							persisteditem = item;
							break;
						}
					}
				}

				if (persisteditem != null)
				{
					cache.SetStatus(persisteditem, PXEntryStatus.Notchanged);
					cache.Remove(persisteditem);
				}
			}
		}

		protected virtual void RelatedFieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			SetSubAccount(sender, e.Row);
		}

		public virtual void RowInserting(PXCache sender, PXRowInsertingEventArgs e)
		{
			SetSubAccount(sender, e.Row);
		}

		public virtual void FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (!e.Cancel)
			{
				e.NewValue = GetDefaultSubID(sender, e.Row);
				e.Cancel = true;
			}
		}

		#endregion

		private void SetSubAccount(PXCache sender, object row)
		{
			PXFieldState state = (PXFieldState)sender.GetValueExt(row, _FieldName);
			if (state != null && state.Value != null)
			{
				sender.SetValue(row, _FieldName, null);
				sender.SetValueExt(row, _FieldName, state.Value);

				// in case of an error while setting Subaccount just reset error state
				state = (PXFieldState)sender.GetValueExt(row, _FieldName);
				if (sender.GetValue(row, _FieldName) == null && state != null && state.ErrorLevel >= PXErrorLevel.Error)
				{
					sender.RaiseExceptionHandling(_FieldName, row, null, null);
				}
			}
		}

		#region Default SubID
		private int? GetDefaultSubID(PXCache sender, object row)
		{
			if (Definitions.DefaultSubID == null)
			{
				object newValue = "0";
				sender.RaiseFieldUpdating(_FieldName, row, ref newValue);
				return (int?)newValue;
			}
			return Definitions.DefaultSubID;
		}

		protected static Definition Definitions
		{
			get
			{
				Definition defs = PX.Common.PXContext.GetSlot<Definition>();
				if (defs == null)
				{
					defs = PX.Common.PXContext.SetSlot<Definition>(PXDatabase.GetSlot<Definition>(typeof(Definition).FullName, typeof(Sub)));
				}
				return defs;
			}
		}

		protected class Definition : IPrefetchable
		{
			private int? _DefaultSubID;
			public int? DefaultSubID
			{
				get { return _DefaultSubID; }
			}

			public void Prefetch()
			{
				_DefaultSubID = null;

				using (PXDataRecord record = PXDatabase.SelectSingle<Sub>(
					new PXDataField<Sub.subID>(),
					new PXDataFieldOrder<Sub.subID>()))
				{
					if (record != null)
						_DefaultSubID = record.GetInt32(0);
				}
			}
		}

		#endregion

		public static Sub GetSubaccount(PXGraph graph, int? subID)
		{
			if (subID == null)
				throw new ArgumentNullException("subID");

			var subaccount = (Sub)PXSelect<Sub,
								Where<Sub.subID, Equal<Required<Sub.subID>>>>
								.Select(graph, subID);

			if (subaccount == null)
			{
				throw new PXException(Messages._0_WithID_1_DoesNotExists, EntityHelper.GetFriendlyEntityName(typeof(Sub)),
					subID);
			}

			return subaccount;
		}

		/// <summary>
		/// Returns deafult subID if default subaccount exists, else returns null.
		/// </summary>
		public static int? TryGetDefaultSubID()
		{
			return Definitions.DefaultSubID;
		}
	}

	public class ControlAccountModule
	{
		public const string ANY = "ANY";
		public const string AP = "AP";
		public const string AR = "AR";
		public const string TX = "TX";
		public const string IN = "IN";
		public const string SO = "SO";
		public const string PO = "PO";
		public const string FA = "FA";
		public const string DR = "DR";

		public sealed class any : PX.Data.BQL.BqlString.Constant<any> { public any() : base(ANY) { } }
		public sealed class aP : PX.Data.BQL.BqlString.Constant<aP> { public aP() : base(AP) { } }
		public sealed class aR : PX.Data.BQL.BqlString.Constant<aR> { public aR() : base(AR) { } }
		public sealed class tX : PX.Data.BQL.BqlString.Constant<tX> { public tX() : base(TX) { } }
		public sealed class iN : PX.Data.BQL.BqlString.Constant<iN> { public iN() : base(IN) { } }
		public sealed class sO : PX.Data.BQL.BqlString.Constant<sO> { public sO() : base(SO) { } }
		public sealed class pO : PX.Data.BQL.BqlString.Constant<pO> { public pO() : base(PO) { } }
		public sealed class fA : PX.Data.BQL.BqlString.Constant<fA> { public fA() : base(FA) { } }
		public sealed class dR : PX.Data.BQL.BqlString.Constant<dR> { public dR() : base(DR) { } }

		public class ListAttribute : PXStringListAttribute, IPXFieldSelectingSubscriber, IPXFieldVerifyingSubscriber
		{
			protected static string[] values = new string[] { string.Empty, AP, AR, TX, IN, SO, PO, FA, DR };
			protected static string[] labels = new string[] { string.Empty, AP, AR, TX, IN, SO, PO, FA, DR };

			public ListAttribute() : base(values, labels) { }

			public override void FieldSelecting(PXCache cache, PXFieldSelectingEventArgs e)
			{
				base.FieldSelecting(cache, e);

				var list = new List<string>() { string.Empty, AP, AR, TX };

				if (PXAccess.FeatureInstalled<PX.Objects.CS.FeaturesSet.fixedAsset>())
					list.Add(FA);

				if (PXAccess.FeatureInstalled<PX.Objects.CS.FeaturesSet.defferedRevenue>())
					list.Add(DR);

				if (PXAccess.FeatureInstalled<PX.Objects.CS.FeaturesSet.distributionModule>())
				{
					list.Add(IN);
					list.Add(PO);
					list.Add(SO);
				}
				var newAllowedLabels = new string[list.Count];
				for (int i = 0; i < _AllowedValues.Length; i++)
				{
					if (list.Contains(_AllowedValues[i]))
					{
						newAllowedLabels[list.IndexOf(_AllowedValues[i])] = _AllowedLabels[i];
					}
				}
				var values = list.ToArray();
				var labels = newAllowedLabels;

				e.ReturnState = (PXStringState)PXStringState.CreateInstance(e.ReturnState, null, null, _FieldName, null, -1, null, values, labels, true, null, _NeutralAllowedLabels);
			}

			public virtual void FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
			{
				var module = (string)e.NewValue;

				if (
					module == FA && !PXAccess.FeatureInstalled<PX.Objects.CS.FeaturesSet.fixedAsset>() ||
					module == DR && !PXAccess.FeatureInstalled<PX.Objects.CS.FeaturesSet.defferedRevenue>() ||
					module.IsIn(IN, PO, SO) && !PXAccess.FeatureInstalled<PX.Objects.CS.FeaturesSet.distributionModule>()
					)
				{
					e.NewValue = null;
				}
			}
		}
	}

	[PXDBString(10, IsUnicode = true, InputMask = "")]
	[PXUIField(DisplayName = "Account", Visibility = PXUIVisibility.Visible)]
	public sealed class AccountCDWildcardAttribute : PXDimensionWildcardAttribute 
	{        
		private int _UIAttrIndex = -1;
		
		private void Initialize()
		{            
			_UIAttrIndex = -1;
			foreach (PXEventSubscriberAttribute attr in _Attributes)
			{                
				if (attr is PXUIFieldAttribute)
				{
					_UIAttrIndex = _Attributes.IndexOf(attr);
				}               
			}
		}
		private const string _DimensionName = "ACCOUNT";
		
		public AccountCDWildcardAttribute()
			: base(_DimensionName,typeof(Account.accountCD))
		{
			Initialize();	
		}
		public AccountCDWildcardAttribute(Type aSearchType)
			: base(_DimensionName, aSearchType)
		{
			Initialize();
		}

		public string DisplayName
		{
			get
			{
				return (_UIAttrIndex == -1) ? null : ((PXUIFieldAttribute)_Attributes[_UIAttrIndex]).DisplayName;
			}
			set
			{
				if (_UIAttrIndex != -1)
					((PXUIFieldAttribute)_Attributes[_UIAttrIndex]).DisplayName = value;
			}
		}

	}


	[PXDBString(30, IsUnicode = true, InputMask = "")]
	[PXUIField(DisplayName = "Subaccount", Visibility = PXUIVisibility.Visible, FieldClass = _DimensionName)]
	public sealed class SubCDWildcardAttribute : PXDimensionWildcardAttribute
	{
		private int _UIAttrIndex = -1;

		private void Initialize()
		{
			_UIAttrIndex = -1;
			foreach (PXEventSubscriberAttribute attr in _Attributes)
			{
				if (attr is PXUIFieldAttribute)
				{
					_UIAttrIndex = _Attributes.IndexOf(attr);
				}
			}
		}
		private const string _DimensionName = "SUBACCOUNT";
		public SubCDWildcardAttribute()
			: base(_DimensionName, typeof(Sub.subCD))
		{
			int dimensionSelIndex = this._Attributes.Count - 2;
			PXDimensionAttribute attr = this._Attributes[dimensionSelIndex] as PXDimensionAttribute; 
			attr.ValidComboRequired = false;
			Initialize();				
		}
		public string DisplayName
		{
			get
			{
				return (_UIAttrIndex == -1) ? null : ((PXUIFieldAttribute)_Attributes[_UIAttrIndex]).DisplayName;
			}
			set
			{
				if (_UIAttrIndex != -1)
					((PXUIFieldAttribute)_Attributes[_UIAttrIndex]).DisplayName = value;
			}
		}

		public override void CacheAttached(PXCache sender)
		{
			if (!PXAccess.FeatureInstalled<FeaturesSet.subAccount>())
			{
				sender.Graph.FieldDefaulting.AddHandler(sender.GetItemType(), _FieldName, FieldDefaulting);
			}
			base.CacheAttached(sender);
		}
		
		public void FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (!e.Cancel)
			{
				e.NewValue = GetDefaultSubID(sender, e.Row);
				e.Cancel = true;
			}
		}


		#region Default SubID
		private string GetDefaultSubID(PXCache sender, object row)
		{
			if (this.Definitions.DefaultSubCD == null)
			{
				object newValue = "0";
				sender.RaiseFieldUpdating(_FieldName, row, ref newValue);
				return (string)newValue;
			}
			return this.Definitions.DefaultSubCD;
		}

		private Definition Definitions
		{
			get
			{
				Definition defs = PX.Common.PXContext.GetSlot<Definition>();
				if (defs == null)
				{
					defs = PX.Common.PXContext.SetSlot<Definition>(PXDatabase.GetSlot<Definition>(typeof(Definition).FullName, typeof(Sub)));
				}
				return defs;
			}
		}

		private class Definition : IPrefetchable
		{
			private string _DefaultSubCD;
			public string DefaultSubCD
			{
				get { return _DefaultSubCD; }
			}

			public void Prefetch()
			{
				_DefaultSubCD = null;

				using (PXDataRecord record = PXDatabase.SelectSingle<Sub>(
					new PXDataField<Sub.subCD>(),
					new PXDataFieldOrder<Sub.subCD>()))
				{
					if (record != null)
						_DefaultSubCD = record.GetString(0);
				}
			}
		}

		#endregion
	}

	/// <summary>
	/// Specialized for the GLTran version of the <see cref="CashTranIDAttribute"/><br/>
	/// Since CATran created from the source row, it may be used only the fields <br/>
	/// of GLTran compatible DAC. <br/>
	/// The main purpuse of the attribute - to create CATran <br/>
	/// for the source row and provide CATran and source synchronization on persisting.<br/>
	/// CATran cache must exists in the calling Graph.<br/>
	/// </summary>
	public class GLCashTranIDAttribute : CashTranIDAttribute, IPXRowInsertingSubscriber, IPXRowUpdatingSubscriber
	{

		protected bool	 _IsIntegrityCheck = false;
		protected bool _CATranValidation = false;

		protected string _LedgerNotActual = "N";
		protected string _LedgerActual = "A";

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);
			Type ChildType = sender.GetItemType();

			if (sender.Graph.Views.Caches.Contains(ChildType) == false)
			{
				sender.Graph.Views.Caches.Add(ChildType);
			}

			if (sender.Graph.Views.Caches.Contains(typeof(CATran)) == false)
			{
				sender.Graph.Views.Caches.Add(typeof(CATran));
			}
		}

		public static CATran DefaultValues<Field>(PXCache sender, object data)
			where Field : IBqlField
		{
			foreach (PXEventSubscriberAttribute attr in sender.GetAttributes<Field>(data))
			{
				if (attr is GLCashTranIDAttribute)
				{
					((GLCashTranIDAttribute)attr)._IsIntegrityCheck = true;
					((GLCashTranIDAttribute) attr)._CATranValidation = true;
					CATran result = ((GLCashTranIDAttribute)attr).DefaultValues(sender, new CATran(), data);
					((GLCashTranIDAttribute) attr)._CATranValidation = false;
					return result;
				}
			}
			return null;
		}

		public static CATran DefaultValues(PXCache sender, object data)
		{
			GLCashTranIDAttribute attr = new GLCashTranIDAttribute();
			attr._IsIntegrityCheck = true;
			return attr.DefaultValues(sender, new CATran(), data);
		}

		public override void RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			if (_IsIntegrityCheck == false)
			{
				base.RowPersisting(sender, e);
			}
		}

		public override void RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
		{
			if (_IsIntegrityCheck == false)
			{
				base.RowPersisted(sender, e);
			}
		}

		public virtual void RowUpdating(PXCache sender, PXRowUpdatingEventArgs e)
		{
			GLTran tran = (GLTran)e.NewRow;
			int? cashAccountID;
			if (CheckGLTranCashAcc(sender.Graph, tran, out cashAccountID) == false)
			{
				Branch branch = (Branch)PXSelectorAttribute.Select<GLTran.branchID>(sender, tran);
				Account account = (Account)PXSelectorAttribute.Select<GLTran.accountID>(sender, tran);
				Sub sub = (Sub)PXSelectorAttribute.Select<GLTran.subID>(sender, tran);				
				if (e.ExternalCall)
				{
					sender.RaiseExceptionHandling<GLTran.subID>(tran, sub.SubCD,
						new PXSetPropertyException(Messages.CashAccountDoesNotExist, branch.BranchCD, account.AccountCD, sub.SubCD));
					e.Cancel = true;
				}
				else
				{
					throw new PXSetPropertyException(Messages.CashAccountDoesNotExist, branch.BranchCD, account.AccountCD, sub.SubCD);
				}
			} 
		}

		public virtual void RowInserting(PXCache sender, PXRowInsertingEventArgs e)
		{
			GLTran tran = (GLTran)e.Row;
			int? cashAccountID;
			if (CheckGLTranCashAcc(sender.Graph, tran, out cashAccountID) == false)
			{
				Branch branch = (Branch)PXSelectorAttribute.Select<GLTran.branchID>(sender, tran);
				Account account = (Account)PXSelectorAttribute.Select<GLTran.accountID>(sender, tran);
				Sub sub = (Sub)PXSelectorAttribute.Select<GLTran.subID>(sender, tran);
				if (e.ExternalCall)
				{
					sender.RaiseExceptionHandling<GLTran.subID>(tran, sub.SubCD,
						new PXSetPropertyException(Messages.CashAccountDoesNotExist, branch.BranchCD, account.AccountCD, sub.SubCD));
					e.Cancel = true;
				}
				else
				{
					throw new PXSetPropertyException(Messages.CashAccountDoesNotExist, branch.BranchCD, account.AccountCD, sub.SubCD);
				}
			}
		}

		public static bool? CheckGLTranCashAcc(PXGraph graph, GLTran tran, out int? cashAccountID)
		{
			cashAccountID = null;
			Account acc = PXSelect<Account, Where<Account.accountID, Equal<Required<Account.accountID>>>>.Select(graph, tran.AccountID);
			if (acc != null && acc.IsCashAccount == true && tran.SubID != null && tran.BranchID != null)
			{
				CashAccount cashAcc = PXSelect<CashAccount, Where<CashAccount.accountID, Equal<Required<CashAccount.accountID>>, Data.And<CashAccount.subID, Equal<Required<CashAccount.subID>>, And<CashAccount.branchID,
					Equal<Required<CashAccount.branchID>>>>>>.Select(graph, tran.AccountID, tran.SubID, tran.BranchID);
				
				if (cashAcc == null)
				{
					return false;
				}
				else
				{
					cashAccountID = cashAcc.CashAccountID;
					return true;
				}
			}
			return null;
		}

		public static bool CheckDocTypeAmounts(string Module, string DocType, decimal CuryDocAmount, decimal CuryDebitAmount, decimal CuryCreditAmount)
		{
			switch (Module)
			{
				case BatchModule.AR:
					switch (DocType)
					{
						case ARDocType.Payment:
						case ARDocType.CashSale:
						case ARDocType.Prepayment:
							//Debit
							return Math.Abs(CuryDocAmount) == CuryDebitAmount;
						case ARDocType.VoidPayment:
						case ARDocType.Refund:
						case ARDocType.CashReturn:
							//Credit
							return Math.Abs(CuryDocAmount) == CuryCreditAmount;
						default:
							return false;
					}
				case BatchModule.AP:
					switch (DocType)
					{
						case APDocType.Check:
						case APDocType.Prepayment:
						case APDocType.QuickCheck:
							//Credit
							return Math.Abs(CuryDocAmount) == CuryCreditAmount;
						case APDocType.VoidCheck:
						case APDocType.Refund:
						case APDocType.VoidQuickCheck:
							//Debit
							return Math.Abs(CuryDocAmount) == CuryDebitAmount;
						default:
							return false;
					}
			}
			return false;
		}	

		public override CATran DefaultValues(PXCache sender, CATran catran_Row, object orig_Row)
		{
			return DefaultValues<Batch, Batch.module, Batch.batchNbr>(sender, catran_Row, orig_Row);
		}

		protected CATran DefaultValues<TBatch, TModule, TBatchNbr>(PXCache sender, CATran catran_Row, object orig_Row) 
				where TBatch : Batch, IBqlTable, new()
				where TModule : IBqlField
				where TBatchNbr : IBqlField
		{
			GLTran parentTran = (GLTran)orig_Row;

			TBatch batch = PXSelect<TBatch,
									Where<TModule, Equal<Required<GLTran.module>>,
									 And<TBatchNbr, Equal<Required<GLTran.batchNbr>>>>>.Select(sender.Graph, parentTran.Module, parentTran.BatchNbr); ///Current

			if (batch.Scheduled == true || batch.Voided == true)
			{
				return null;
			}

			bool LedgerNotActual = false;
			if (String.IsNullOrWhiteSpace(parentTran.LedgerBalanceType))
			{
				Ledger setup = (Ledger)PXSelect<Ledger, Where<Ledger.ledgerID, Equal<Required<Ledger.ledgerID>>,
					And<Ledger.balanceType, Equal<LedgerBalanceType.actual>>>>.Select(sender.Graph, parentTran.LedgerID);
				LedgerNotActual = (setup == null);
			}
			else
			{
				LedgerNotActual = (parentTran.LedgerBalanceType == _LedgerNotActual);
			}
			int? cashAccountID;
			bool? CashAccChecked = CheckGLTranCashAcc(sender.Graph, parentTran, out cashAccountID);

			if (CashAccChecked == null || LedgerNotActual || parentTran.Module == BatchModule.CM)
			{
				return null;
			}
			else
			{
				if (CashAccChecked == false)
				{
					Branch branch = (Branch)PXSelectorAttribute.Select<GLTran.branchID>(sender, parentTran);
					Account account = (Account)PXSelectorAttribute.Select<GLTran.accountID>(sender, parentTran);
					Sub sub = (Sub)PXSelectorAttribute.Select<GLTran.subID>(sender, parentTran);
					sender.RaiseExceptionHandling<GLTran.subID>(parentTran, sub.SubCD, new PXSetPropertyException(Messages.CashAccountDoesNotExist,
						branch.BranchCD, account.AccountCD, sub.SubCD));
					return null;
				}
			}
			
			if (parentTran.Module == BatchModule.GL || catran_Row.TranID == null)
			{
				if (catran_Row.TranID == null)
				{
					string origTranType = GLTranType.GLEntry;
					string origRefNbr = parentTran.BatchNbr;
					int? origLineNbr = parentTran.LineNbr;
					string extRefNbr = parentTran.RefNbr;
					long? caTranId = null;
					CashAccount cashAcct;
					if (_CATranValidation)
					{
						switch (parentTran.Module)
						{
							case BatchModule.AR:
								ARPayment arPayment = PXSelect<ARPayment, Where<ARPayment.docType, Equal<Required<ARPayment.docType>>,
									And<ARPayment.refNbr, Equal<Required<ARPayment.refNbr>>>>>.Select(sender.Graph, parentTran.TranType, parentTran.RefNbr);
								cashAcct = PXSelectorAttribute.Select<ARPayment.cashAccountID>(sender.Graph.Caches[typeof(ARPayment)], arPayment) as CashAccount;
								if (arPayment != null && cashAcct != null && cashAcct.AccountID == parentTran.AccountID
									&& cashAcct.SubID == parentTran.SubID 
									&& CheckDocTypeAmounts(parentTran.Module, parentTran.TranType, arPayment.CuryOrigDocAmt.Value,
										parentTran.CuryDebitAmt.Value, parentTran.CuryCreditAmt.Value))
								{
									origTranType = arPayment.DocType;
									origRefNbr = arPayment.RefNbr;
									origLineNbr = null;
									extRefNbr = arPayment.ExtRefNbr;
									caTranId = arPayment.CATranID;
								}
								break;
							case BatchModule.AP:
								APPayment apPayment = PXSelect<APPayment, Where<APPayment.docType, Equal<Required<APPayment.docType>>,
									And<APPayment.refNbr, Equal<Required<APPayment.refNbr>>>>>.Select(sender.Graph, parentTran.TranType, parentTran.RefNbr);
								cashAcct = PXSelectorAttribute.Select<APPayment.cashAccountID>(sender.Graph.Caches[typeof(APPayment)], apPayment) as CashAccount;
								if (apPayment != null && cashAcct != null && cashAcct.AccountID == parentTran.AccountID
									&& cashAcct.SubID == parentTran.SubID
									&& CheckDocTypeAmounts(parentTran.Module, parentTran.TranType, apPayment.CuryOrigDocAmt.Value,
										parentTran.CuryDebitAmt.Value, parentTran.CuryCreditAmt.Value))
								{
									origTranType = apPayment.DocType;
									origRefNbr = apPayment.RefNbr;
									origLineNbr = null;
									extRefNbr = apPayment.ExtRefNbr;
									caTranId = apPayment.CATranID;
								}
								break;
						}

						if (caTranId != null)
						{
							CATran catran = PXSelect<CATran, Where<CATran.tranID, Equal<Required<CATran.tranID>>>>.Select(sender.Graph, caTranId);
							if (catran != null)
							{
								catran_Row = catran;
							}
							else
							{
								caTranId = null;
							}
						}

						if (caTranId == null)
						{
							decimal? amt;
							string DrCr;
							if (parentTran.CuryDebitAmt - parentTran.CuryCreditAmt >= 0)
							{
								amt = parentTran.CuryDebitAmt - parentTran.CuryCreditAmt;
								DrCr = "D";
							}
							else
							{
								amt = parentTran.CuryDebitAmt - parentTran.CuryCreditAmt;
								DrCr = "C";
							}
							PXResultset<CATran> catranList = PXSelect<CATran, Where<CATran.origModule, Equal<Required<CATran.origModule>>, Data.And<CATran.origTranType, Equal<Required<CATran.origTranType>>, Data.And<CATran.origRefNbr, Equal<Required<CATran.origRefNbr>>, Data.And<CATran.cashAccountID, Equal<Required<CATran.cashAccountID>>, Data.And<CATran.curyTranAmt, Equal<Required<CATran.curyTranAmt>>, And<CATran.drCr, Equal<Required<CATran.drCr>>>>>>>>>.Select(sender.Graph, parentTran.Module,
										origTranType, origRefNbr, cashAccountID, amt, DrCr);
							foreach (CATran catran in catranList)
							{
								GLTran linkedTran = PXSelect<GLTran, Where<GLTran.module, Equal<Required<GLTran.module>>, Data.And<GLTran.batchNbr, Equal<Required<GLTran.batchNbr>>,
										And<GLTran.cATranID, Equal<Required<GLTran.cATranID>>>>>>.Select(sender.Graph, parentTran.Module, parentTran.BatchNbr, catran.TranID);
								if (linkedTran == null)
								{
									catran_Row = catran;
									break;
								}
							}
						}
					}

					if (catran_Row.TranID == null)
					{
						catran_Row.OrigModule = parentTran.Module;
						catran_Row.OrigTranType = origTranType;
						catran_Row.OrigRefNbr = origRefNbr;
						catran_Row.OrigLineNbr = origLineNbr;
						catran_Row.ExtRefNbr = extRefNbr;
					}
					else
					{
						//make a copy for accumulator attribute
						catran_Row = PXCache<CATran>.CreateCopy(catran_Row);
					}
				}
				else if (catran_Row.TranID < 0)
				{
					catran_Row.OrigModule = parentTran.Module;
					catran_Row.OrigTranType = GLTranType.GLEntry;
					catran_Row.OrigRefNbr = parentTran.BatchNbr;
					catran_Row.ExtRefNbr = parentTran.RefNbr;
					catran_Row.OrigLineNbr = parentTran.LineNbr;
				}

				if (!_CATranValidation || catran_Row.TranID == null || catran_Row.TranID < 0)
				{
					SetAmounts(catran_Row, parentTran);
					if (catran_Row.CuryTranAmt >= 0)
					{
						catran_Row.DrCr = DrCr.Debit;
					}
					else
					{
						catran_Row.DrCr = DrCr.Credit;
					}
					CashAccount cashacc = PXSelect<CashAccount, Where<CashAccount.cashAccountID, Equal<Required<CashAccount.cashAccountID>>>>.Select(sender.Graph, cashAccountID);

					catran_Row.CashAccountID = cashacc.CashAccountID;
					catran_Row.CuryInfoID = parentTran.CuryInfoID;
					catran_Row.TranDate = parentTran.TranDate;
					catran_Row.TranDesc = parentTran.TranDesc;
				    SetPeriodsByMaster(sender, catran_Row, parentTran.TranPeriodID);
                    catran_Row.ReferenceID = parentTran.ReferenceID;
					catran_Row.Released = parentTran.Released;
					catran_Row.Posted = parentTran.Posted;
					catran_Row.Hold = false;
					catran_Row.BatchNbr = parentTran.BatchNbr;
					catran_Row.ExtRefNbr = parentTran.RefNbr;


					if (cashacc != null && cashacc.Reconcile == false)
					{
						catran_Row.Cleared = true;
						catran_Row.ClearDate = catran_Row.TranDate;
					}
				}
			}
			else
			{
				if ((parentTran.Released == true) && (catran_Row != null) && (catran_Row.TranID != null))
				{
					catran_Row.Released	= parentTran.Released;
					catran_Row.Posted	= parentTran.Posted;
					catran_Row.Hold		= false;
					catran_Row.BatchNbr = parentTran.BatchNbr;
					SetAmounts(catran_Row, parentTran);

					SetCleared(catran_Row, GetCashAccount(catran_Row, sender.Graph));
				}
				else
				{
					return null;
				}
			}
			return catran_Row;
		}

		private static void SetAmounts(CATran caTran, GLTran glTran)
		{
			caTran.CuryTranAmt = glTran.CuryDebitAmt - glTran.CuryCreditAmt;
			caTran.TranAmt = glTran.DebitAmt - glTran.CreditAmt;
		}

		/// <returns>
		/// <c>true</c>, if the <see cref="Batch"/> record, to which the current <see cref="GLTran"/>
		/// row belongs, is part of a recurring schedule, and a default value otherwise.
		/// </returns>
		protected override bool NeedPreventCashTransactionCreation(PXCache sender, object row)
		{
			GLTran transaction = row as GLTran;

			if (transaction?.Module == null || 
				transaction.BatchNbr == null)
			{
				return base.NeedPreventCashTransactionCreation(sender, row);
			}

			PXCache batchCache = sender.Graph.Caches[typeof(Batch)];

			// Try searching for the referenced Batch record in the 
			// Current property first.
			// -
			Batch parentBatch = batchCache.Current as Batch;

			if (parentBatch?.Module == transaction.Module &&
				parentBatch?.BatchNbr == transaction.BatchNbr)
			{
				return parentBatch?.Scheduled == true;
			}

			// Failing that, try to locate the Batch record in the cache.
			// -
			parentBatch = batchCache.CreateInstance() as Batch;

			if (parentBatch != null)
			{
				parentBatch.Module = transaction.Module;
				parentBatch.BatchNbr = transaction.BatchNbr;

				parentBatch = batchCache.Locate(parentBatch) as Batch;

				if (parentBatch != null)
				{
					return parentBatch.Scheduled == true;
				}
			}

			// Failing that, read the Batch record from the database.
			// -
			parentBatch = PXSelect<
				Batch,
				Where<
					Batch.module, Equal<Required<Batch.module>>,
					And<Batch.batchNbr, Equal<Required<Batch.batchNbr>>>>>
				.Select(sender.Graph, transaction.Module, transaction.BatchNbr);

			if (parentBatch != null)
			{
				return parentBatch.Scheduled == true;
			}

			return base.NeedPreventCashTransactionCreation(sender, row);
		}
	}

	#region TrialBalanceImportStatusAttribute

	/// <summary>
	/// List Attrubute with the following values : New, Valid, Duplicate, Error.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
	public sealed class TrialBalanceImportStatusAttribute : PXIntListAttribute
	{
		public const int NEW = 0;
		public const int VALID = 1;
		public const int DUPLICATE = 2;
		public const int ERROR = 3;

		public TrialBalanceImportStatusAttribute()
			: base(new int[] { NEW, VALID, DUPLICATE, ERROR }, 
				   new string[] { Messages.New, Messages.Valid, Messages.Duplicate, Messages.Error })
		{
		}
	}

	#endregion

	#region TrialBalanceImportMapStatusAttribute

	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
	public sealed class TrialBalanceImportMapStatusAttribute : PXIntListAttribute
	{
		public const int HOLD = 0;
		public const int BALANCED = 1;
		public const int RELEASED = 2;

		public TrialBalanceImportMapStatusAttribute()
			: base(new int[] { HOLD, BALANCED, RELEASED }, 
				   new string[] { Messages.Hold, Messages.Balanced, Messages.Released })
		{
		}
	}

	#endregion

	#region PersistErrorAttribute

	/// <summary>
	/// Maps the errors from one field to another. Whenever an error is raised on the Source field it is transfered to
	/// the target field and gets displayed on the targer field.
	/// To use this attribute decorate the Source field with this attribute and pass Taget field an argument in the constructor of this attrubute.
	/// </summary>
	/// <example>
	/// [PersistError(typeof(GLTrialBalanceImportDetails.importAccountCDError))]
	/// </example>
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
	public sealed class PersistErrorAttribute : PXEventSubscriberAttribute, IPXFieldSelectingSubscriber
	{
		private readonly string _errorFieldName;

		public PersistErrorAttribute(Type errorField)
		{
			_errorFieldName = errorField.Name;
		}

		public void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			if (e.Row != null)
			{
				string error = sender.GetValue(e.Row, _errorFieldName) as string;
				if (!string.IsNullOrEmpty(error))
				{
					SetError(sender, e.Row, error);
					if (e.ReturnState != null) 
						e.ReturnState = PXFieldState.CreateInstance(e.ReturnState, null, null, null,
							null, null, null, null, _FieldName, null, null, error, PXErrorLevel.Error,
							null, null, null, PXUIVisibility.Undefined, null, null, null);
				}
			}
		}

		public static void SetError(PXCache sender, object data, string fieldName, string error)
		{
			PersistErrorAttribute attribute = GetAttribute(sender, fieldName);
			if (attribute != null)
			{
				sender.SetValue(data, attribute._errorFieldName, error);
				attribute.SetError(sender, data, error);
			}
		}

		public static void ClearError(PXCache sender, object data, string fieldName)
		{
			PersistErrorAttribute attribute = GetAttribute(sender, fieldName);
			if (attribute != null) sender.SetValue(data, attribute._errorFieldName, null);
		}

		private void SetError(PXCache sender, object data, string error)
		{
			object value = sender.GetValue(data, _FieldOrdinal);
			PXUIFieldAttribute.SetError(sender, data, _FieldName, error,
										value == null ? null : value.ToString());
		}

		private static PersistErrorAttribute GetAttribute(PXCache sender, string fieldName)
		{
			foreach (PXEventSubscriberAttribute attribute in sender.GetAttributes(fieldName))
			{
				PersistErrorAttribute castAttribute = attribute as PersistErrorAttribute;
				if (castAttribute != null) return castAttribute;
			}
			return null;
		}
	}

	#endregion

	#region TableAndChartDashboardTypeAttribute

	/// <summary>
	/// Graphs decorated with the given attribute will expose there primary View as a source for both Dashboard Table and Dashbprd Chart Controls.
	/// Usually an Inquiry Graph is decorated with this attribute.
	/// </summary>
	/// <example>
	/// [DashboardType(PX.TM.OwnedFilter.DASHBOARD_TYPE, GL.TableAndChartDashboardTypeAttribute._AMCHARTS_DASHBOART_TYPE)]
	/// </example>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
	public sealed class TableAndChartDashboardTypeAttribute : DashboardTypeAttribute
	{
		public const int _AMCHARTS_DASHBOART_TYPE = 20;

		public TableAndChartDashboardTypeAttribute()
			: base((int)Type.Default, (int)Type.Chart, _AMCHARTS_DASHBOART_TYPE)
		{
			
		}
	}

	#endregion

	#region TableDashboardTypeAttribute

	/// <summary>
	/// Graphs decorated with the given attribute will expose there primary View as a source for Dashboard Table Controls.
	/// Usually an Inquiry Graph is decorated with this attribute.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
	public sealed class TableDashboardTypeAttribute : DashboardTypeAttribute
	{
		public TableDashboardTypeAttribute()
			: base((int)Type.Default)
		{
			
		}
	}

	#endregion

	public class GLTaxAttribute : TX.TaxAttribute
	{
		#region CuryInclTaxTotal
		protected string _CuryInclTaxTotal = "CuryTaxTotal";
		protected string _CuryTranTotal = "CuryTranTotal";
		public Type CuryInclTaxTotal
		{
			set
			{
				_CuryInclTaxTotal = value.Name;
			}
			get
			{
				return null;
			}
		}

		public Type CuryTranTotal
		{
			set
			{
				_CuryTranTotal = value.Name;
			}
			get
			{
				return null;
			}
		}
		#endregion

		public GLTaxAttribute(Type ParentType, Type TaxType, Type TaxSumType)
			: base(ParentType, TaxType, TaxSumType)
		{
			this.CuryDocBal = null;
			this.CuryLineTotal = typeof(GLTranDoc.curyTranAmt); //Used only for reading
			this.CuryTaxTotal = typeof(GLTranDoc.curyTaxAmt);
			this.CuryInclTaxTotal = typeof(GLTranDoc.curyInclTaxAmt);
			this.DocDate = typeof(GLTranDoc.tranDate);
			this.CuryTranAmt = typeof(GLTranDoc.curyTranAmt);

			//this._Attributes.Add(new PXUnboundFormulaAttribute(typeof(Switch<Case<Where<GLTranDoc.debitAccountID, IsNotNull>, Add<GLTranDoc.curyTranAmt, Sub<GLTranDoc.curyTaxAmt, GLTranDoc.curyInclTaxAmt>>>, Sub<GLTranDoc.curyTaxAmt, GLTranDoc.curyInclTaxAmt>>), typeof(SumCalc<GLDocBatch.curyDebitTotal>)));
			//this._Attributes.Add(new PXUnboundFormulaAttribute(typeof(Switch<Case<Where<GLTranDoc.creditAccountID, IsNotNull>, Add<GLTranDoc.curyTranAmt, Sub<GLTranDoc.curyTaxAmt, GLTranDoc.curyInclTaxAmt>>>, Sub<GLTranDoc.curyTaxAmt, GLTranDoc.curyInclTaxAmt>>), typeof(SumCalc<GLDocBatch.curyCreditTotal>)));

			this._Attributes.Add(new PXUnboundFormulaAttribute(typeof(Switch<Case<Where<GLTranDoc.debitAccountID, IsNotNull>, GLTranDoc.curyTranTotal>, Sub<GLTranDoc.curyTaxAmt, GLTranDoc.curyInclTaxAmt>>), typeof(SumCalc<GLDocBatch.curyDebitTotal>)));
			this._Attributes.Add(new PXUnboundFormulaAttribute(typeof(Switch<Case<Where<GLTranDoc.creditAccountID, IsNotNull>, GLTranDoc.curyTranTotal>, Sub<GLTranDoc.curyTaxAmt, GLTranDoc.curyInclTaxAmt>>), typeof(SumCalc<GLDocBatch.curyCreditTotal>)));


			//this._Attributes.Add(new PXUnboundFormulaAttribute(typeof(Switch<Case<Where<GLTranDoc.debitAccountID, IsNotNull>, Switch<Case<Where<GLTranDoc.parentLineNbr, IsNull>, GLTranDoc.curyTranTotal>, GLTranDoc.curyTranAmt>>, Sub<GLTranDoc.curyTaxAmt, GLTranDoc.curyInclTaxAmt>>), typeof(SumCalc<GLDocBatch.curyDebitTotal>)));
			//this._Attributes.Add(new PXUnboundFormulaAttribute(typeof(Switch<Case<Where<GLTranDoc.creditAccountID, IsNotNull>, Switch<Case<Where<GLTranDoc.parentLineNbr, IsNull>, GLTranDoc.curyTranTotal>, GLTranDoc.curyTranAmt>>, Sub<GLTranDoc.curyTaxAmt, GLTranDoc.curyInclTaxAmt>>), typeof(SumCalc<GLDocBatch.curyCreditTotal>)));

			//this._Attributes.Add(new PXUnboundFormulaAttribute(typeof(Switch<Case<Where<GLTranDoc.debitAccountID, IsNotNull, And<GLTranDoc.parentLineNbr, IsNull>>, GLTranDoc.curyTranTotal,
			//                                    Case<Where<GLTranDoc.debitAccountID, IsNull, And<GLTranDoc.parentLineNbr, IsNull>>,Sub<GLTranDoc.curyTaxAmt, GLTranDoc.curyInclTaxAmt>,
			//                                    Case<Where<GLTranDoc.debitAccountID, IsNotNull, And<GLTranDoc.parentLineNbr, IsNotNull>>, GLTranDoc.curyTranAmt>>>, decimal0>), typeof(SumCalc<GLDocBatch.curyDebitTotal>)));
			//this._Attributes.Add(new PXUnboundFormulaAttribute(typeof(Switch<Case<Where<GLTranDoc.creditAccountID, IsNotNull, And<GLTranDoc.parentLineNbr, IsNull>>, GLTranDoc.curyTranTotal,
			//                                    Case<Where<GLTranDoc.creditAccountID, IsNull, And<GLTranDoc.parentLineNbr, IsNull>>, Sub<GLTranDoc.curyTaxAmt, GLTranDoc.curyInclTaxAmt>,
			//                                    Case<Where<GLTranDoc.creditAccountID, IsNotNull, And<GLTranDoc.parentLineNbr, IsNotNull>>, GLTranDoc.curyTranAmt>>>, decimal0>), typeof(SumCalc<GLDocBatch.curyCreditTotal>)));
			////this._Attributes.Add(new PXUnboundFormulaAttribute(typeof(Switch<Case<Where<GLTranDoc.creditAccountID, IsNotNull, And<GLTranDoc.parentLineNbr, IsNull>>, GLTranDoc.curyTranTotal, Case<Where<GLTranDoc.creditAccountID, IsNotNull, And<GLTranDoc.parentLineNbr, IsNotNull>>, GLTranDoc.curyTranAmt>>, Sub<GLTranDoc.curyTaxAmt, GLTranDoc.curyInclTaxAmt>>), typeof(SumCalc<GLDocBatch.curyCreditTotal>)));
			
			//this._Attributes.Add(new PXUnboundFormulaAttribute(typeof(Switch<Case<Where<GLTranDoc.parentLineNbr, IsNotNull>, Switch<Case<Where<GLTranDoc.debitAccountID, IsNotNull>, Add<GLTranDoc.curyTranAmt, Sub<GLTranDoc.curyTaxAmt, GLTranDoc.curyInclTaxAmt>>>, Sub<GLTranDoc.curyTaxAmt, GLTranDoc.curyInclTaxAmt>>>,
			//							Switch<Case<Where<GLTranDoc.debitAccountID, IsNotNull>, GLTranDoc.curyTranTotal>, Sub<GLTranDoc.curyTaxAmt, GLTranDoc.curyInclTaxAmt>>>), typeof(SumCalc<GLDocBatch.curyDebitTotal>)));

			//this._Attributes.Add(new PXUnboundFormulaAttribute(typeof(Switch<Case<Where<GLTranDoc.parentLineNbr, IsNotNull>, Switch<Case<Where<GLTranDoc.creditAccountID, IsNotNull>, Add<GLTranDoc.curyTranAmt, Sub<GLTranDoc.curyTaxAmt, GLTranDoc.curyInclTaxAmt>>>, Sub<GLTranDoc.curyTaxAmt, GLTranDoc.curyInclTaxAmt>>>,
			//							Switch<Case<Where<GLTranDoc.creditAccountID, IsNotNull>, GLTranDoc.curyTranTotal>, Sub<GLTranDoc.curyTaxAmt, GLTranDoc.curyInclTaxAmt>>>), typeof(SumCalc<GLDocBatch.curyCreditTotal>)));
			
			
		}

		protected override decimal CalcLineTotal(PXCache sender, object row)
		{
			return (decimal?)ParentGetValue(sender.Graph, _CuryLineTotal) ?? 0m; //It's only readed, not updated
		}

		protected override List<object> SelectTaxes<Where>(PXGraph graph, object row, PXTaxCheck taxchk, params object[] parameters)
		{
			IComparer<Tax> taxByCalculationLevelComparer = GetTaxByCalculationLevelComparer();
			taxByCalculationLevelComparer.ThrowOnNull(nameof(taxByCalculationLevelComparer));

			Dictionary<string, PXResult<Tax, TaxRev>> tail = new Dictionary<string, PXResult<Tax, TaxRev>>();
			PXSelectBase<Tax> taxRevSelect = null;
			GLTranDoc parentrow =(GLTranDoc)(_ParentRow ?? row);

			if (parentrow != null && (parentrow.TranModule == GL.BatchModule.AP))
			{
				taxRevSelect = new PXSelectReadonly2<Tax,
											LeftJoin<TaxRev, On<TaxRev.taxID, Equal<Tax.taxID>, And<TaxRev.outdated, Equal<boolFalse>,
												And2<
												Where<TaxRev.taxType, Equal<TaxType.purchase>, And<Tax.reverseTax, Equal<boolFalse>, 
													Or<TaxRev.taxType, Equal<TaxType.sales>, And<Where<Tax.reverseTax, Equal<boolTrue>, 
													Or<Tax.taxType, Equal<CSTaxType.use>, 
													Or<Tax.taxType, Equal<CSTaxType.withholding>>>>>>>>, 
												And<Current<GLTranDoc.tranDate>, Between<TaxRev.startDate, TaxRev.endDate>>>>>>,
												Where>(graph);
			}
			if(parentrow != null && (parentrow.TranModule == GL.BatchModule.AR))
			{
				taxRevSelect = new PXSelectReadonly2<Tax,
				LeftJoin<TaxRev, On<TaxRev.taxID, Equal<Tax.taxID>, Data.And<TaxRev.outdated, Equal<boolFalse>, Data.And<TaxRev.taxType, Equal<TaxType.sales>, Data.And<Tax.taxType, NotEqual<CSTaxType.withholding>, Data.And<Tax.taxType, NotEqual<CSTaxType.use>, Data.And<Tax.reverseTax, Equal<boolFalse>, And<Current<GLTranDoc.tranDate>, Between<TaxRev.startDate, TaxRev.endDate>>>>>>>>>,
						Where>(graph);
			}
			if (parentrow != null && (parentrow.TranModule == GL.BatchModule.CA))
			{
				taxRevSelect = new PXSelectReadonly2<Tax,
				LeftJoin<TaxRev, On<TaxRev.taxID, Equal<Tax.taxID>, Data.And<TaxRev.outdated, Equal<boolFalse>,
					And2<Where<Current<GLTranDoc.cADrCr>, Equal<CADrCr.cACredit>, Data.And<TaxRev.taxType, Equal<TaxType.purchase>, Data.And<Tax.reverseTax, Equal<boolFalse>, Data.Or<Current<GLTranDoc.cADrCr>, Equal<CADrCr.cACredit>, Data.And<TaxRev.taxType, Equal<TaxType.sales>, Data.And<Tax.reverseTax, Equal<boolTrue>, Data.Or<Current<GLTranDoc.cADrCr>, Equal<CADrCr.cADebit>, Data.And<TaxRev.taxType, Equal<TaxType.sales>, Data.And<Tax.reverseTax, Equal<boolFalse>, Data.And<Tax.taxType, NotEqual<CSTaxType.withholding>, And<Tax.taxType, NotEqual<CSTaxType.use>>>>>>>>>>>>, And<Current<GLTranDoc.tranDate>, Between<TaxRev.startDate, TaxRev.endDate>>>>>>,
					Where>(graph);
			}

			if (taxRevSelect != null)
			{
				foreach (PXResult<Tax, TaxRev> record in taxRevSelect.View.SelectMultiBound(new object[] { row }, parameters))
				{
					tail[((Tax)record).TaxID] = record;
				}
			}
			List<object> ret = new List<object>();
			
			switch (taxchk)
			{
				case PXTaxCheck.Line:
					foreach (GLTax record in PXSelect<GLTax,
						Where<GLTax.module, Equal<Current<GLTranDoc.module>>, Data.And<GLTax.batchNbr, Equal<Current<GLTranDoc.batchNbr>>, Data.And<GLTax.detailType, Equal<GLTaxDetailType.lineTax>, And<GLTax.lineNbr, Equal<Current<GLTranDoc.lineNbr>>>>>>>
						.SelectMultiBound(graph, new object[] { row }))
					{
						if (tail.TryGetValue(record.TaxID, out PXResult<Tax, TaxRev> line))
						{
							int idx;
							for (idx = ret.Count;
								(idx > 0) && taxByCalculationLevelComparer.Compare((PXResult<GLTax, Tax, TaxRev>)ret[idx - 1], line) > 0;
								idx--) ;

							ret.Insert(idx, new PXResult<GLTax, Tax, TaxRev>(record, (Tax)line, (TaxRev)line));
						}
					}
					return ret;

				case PXTaxCheck.RecalcLine:
					{
						object[] details = PXSelect<GLTranDoc, Where<GLTranDoc.module, Equal<Current<GLTranDoc.module>>, Data.And<GLTranDoc.batchNbr, Equal<Current<GLTranDoc.batchNbr>>,
														And<GLTranDoc.parentLineNbr, Equal<Current<GLTranDoc.lineNbr>>>>>>.SelectMultiBound(graph, new object[] { _ParentRow }).ToArray();
						HashSet<Int32?> lines = new HashSet<Int32?>(Array.ConvertAll(details, a => PXResult.Unwrap<GLTranDoc>(a).LineNbr));

						if (row == null && _ParentRow == null) return ret;

						lines.Add(((GLTranDoc)row ?? (GLTranDoc)_ParentRow).LineNbr);

						foreach (GLTax record in PXSelect<GLTax,
							Where<GLTax.module, Equal<Current<GLTranDoc.module>>, Data.And<GLTax.batchNbr, Equal<Current<GLTranDoc.batchNbr>>,
							And<GLTax.detailType, Equal<GLTaxDetailType.lineTax>>>>>
							.SelectMultiBound(graph, new object[] { row }))
						{
							if (lines.Contains(record.LineNbr) && tail.TryGetValue(record.TaxID, out PXResult<Tax, TaxRev> line))
							{
								int idx;
								for (idx = ret.Count;
									(idx > 0)
									&& ((GLTax)(PXResult<GLTax, Tax, TaxRev>)ret[idx - 1]).LineNbr == record.LineNbr
									&& taxByCalculationLevelComparer.Compare((PXResult<GLTax, Tax, TaxRev>)ret[idx - 1], line) > 0;
									idx--) ;

								ret.Insert(idx, new PXResult<GLTax, Tax, TaxRev>(record, (Tax)line, (TaxRev)line));
							}
						}

						return ret;
					}
					case PXTaxCheck.RecalcTotals:					
					{
						GLTranDoc parent = (GLTranDoc)_ParentRow;

						if (parent == null) return ret;

						foreach (GLTaxTran record in PXSelect<GLTaxTran,
							Where<GLTaxTran.module, Equal<Current<GLTranDoc.module>>, Data.And<GLTaxTran.batchNbr, Equal<Current<GLTranDoc.batchNbr>>,
								And<GLTaxTran.lineNbr, Equal<Current<GLTranDoc.lineNbr>>>>>>
							.SelectMultiBound(graph, new object[] { parent }))
						{
							if (record.TaxID != null && tail.TryGetValue(record.TaxID, out PXResult<Tax, TaxRev> line))
							{
								int idx;
								for (idx = ret.Count;
									(idx > 0) && taxByCalculationLevelComparer.Compare((PXResult<GLTaxTran, Tax, TaxRev>)ret[idx - 1], line) > 0;
									idx--) ;

								ret.Insert(idx, new PXResult<GLTaxTran, Tax, TaxRev>(record, (Tax)line, (TaxRev)line));
							}
						}
						return ret;
					}				
				default:
					return ret;
			}
		}

		protected override IEnumerable<ITaxDetail> ManualTaxes(PXCache sender, object row)
		{
			List<ITaxDetail> ret = new List<ITaxDetail>();

			foreach (PXResult r in SelectTaxes(sender, row, PXTaxCheck.RecalcTotals))
			{
				ret.Add((ITaxDetail)r[0]);
			}
			return ret;
		}

		public override void CacheAttached(PXCache sender)
		{
			if (this.EnableTaxCalcOn(sender.Graph))
			{
				base.CacheAttached(sender);
				sender.Graph.RowInserting.AddHandler(_TaxSumType, Tax_RowInserting);
			}
			else
			{
				this.TaxCalc = TaxCalc.NoCalc;
			}
		}

		virtual protected bool EnableTaxCalcOn(PXGraph aGraph)
		{
			return (aGraph is JournalWithSubEntry);
		}

		protected override void Tax_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			if ((_TaxCalc != TaxCalc.NoCalc && e.ExternalCall || _TaxCalc == TaxCalc.ManualCalc))
			{
				if (!sender.ObjectsEqual<GLTaxTran.curyTaxAmt>(e.OldRow,e.Row))
				{
					_ParentRow = TaxParentAttribute.SelectParent(sender, e.Row, typeof(GLTranDoc));

					if (_ParentRow != null)
					{
						_ParentRow = PXCache<GLTranDoc>.CreateCopy((GLTranDoc)_ParentRow);
					}

					CalcTotals(sender.Graph.Caches[_ChildType], null, false);

					sender.Graph.Caches[_ParentType].Update(_ParentRow);
				}
			}
		}

		protected virtual void Tax_RowInserting(PXCache sender, PXRowInsertingEventArgs e)
		{
			if (e.ExternalCall)
			{
				throw new PXSetPropertyException(Messages.TaxDetailCanNotBeInsertedManully);				
				//((GLTax)e.Row).DetailType = GLTaxDetailType.LineTax;
			}
		}

		protected override void Tax_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			bool atleastonematched = false;
			if ((_TaxCalc != TaxCalc.NoCalc && e.ExternalCall || _TaxCalc == TaxCalc.ManualCalc))
			{
				PXCache cache = sender.Graph.Caches[_ChildType];
				PXCache taxcache = sender.Graph.Caches[_TaxType];

				List<object> details = TaxParentAttribute.ChildSelect(cache, e.Row, _ParentType);
				foreach (object det in details)
				{
					ITaxDetail taxzonedet = MatchesCategory(cache, det, (ITaxDetail)e.Row);
					AddOneTax(taxcache, det, taxzonedet);
					if (taxzonedet != null)
					{
						atleastonematched = true;
					}
				}
				_NoSumTotals = (_TaxCalc == TaxCalc.ManualCalc && e.ExternalCall == false);
				CalcTaxes(cache, null);
				_NoSumTotals = false;

				if (!atleastonematched)
				{
					sender.RaiseExceptionHandling("TaxID", e.Row, ((TaxDetail)e.Row).TaxID, new PXSetPropertyException(TX.Messages.NoLinesMatchTax, PXErrorLevel.RowError));
				}
			}
		}

		protected override void Tax_RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
			if ((_TaxCalc != TaxCalc.NoCalc && e.ExternalCall || _TaxCalc == TaxCalc.ManualCalc))
			{
				PXCache cache = sender.Graph.Caches[_ChildType];
				PXCache taxcache = sender.Graph.Caches[_TaxType];

				List<object> details = TaxParentAttribute.ChildSelect(cache, e.Row, _ParentType);
				foreach (object det in details)
				{
					DelOneTax(taxcache, det, e.Row);
				}
				CalcTaxes(cache, null);
			}
		}

		[Obsolete("Method to be removed in 6.0")]
		protected virtual void CuryTranAmount_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e) 
		{
			GLTranDoc row = (GLTranDoc) e.Row;
			if (row.IsChildTran && row.CuryTranTotal != decimal.Zero) 
			{
				GLTranDoc parent = PXSelect<GLTranDoc, Where<GLTranDoc.module, Equal<Required<GLTranDoc.module>>, Data.And<GLTranDoc.batchNbr, Equal<Required<GLTranDoc.batchNbr>>,
											And<GLTranDoc.lineNbr, Equal<Required<GLTranDoc.lineNbr>>>>>>.Select(sender.Graph, row.Module, row.BatchNbr, row.ParentLineNbr);
				e.NewValue = CalcTaxableFromTotalAmount(sender, e.Row, parent.TaxZoneID, row.TaxCategoryID, parent.TranDate.Value, row.CuryTranTotal.Value);
				e.Cancel = true;
			}
		}

		protected override void CalcDocTotals(
			PXCache sender, 
			object row, 
			decimal CuryTaxTotal, 
			decimal CuryInclTaxTotal, 
			decimal CuryWhTaxTotal,
			decimal CuryTaxDiscountTotal)
		{
			base.CalcDocTotals(sender, row, CuryTaxTotal, CuryInclTaxTotal, CuryWhTaxTotal, CuryTaxDiscountTotal);

			if (ParentGetStatus(sender.Graph) != PXEntryStatus.Deleted)
			{
				decimal doc_CuryWhTaxTotal = (decimal)(ParentGetValue(sender.Graph, _CuryWhTaxTotal) ?? 0m);

				if (object.Equals(CuryWhTaxTotal, doc_CuryWhTaxTotal) == false)
				{
					ParentSetValue(sender.Graph, _CuryWhTaxTotal, CuryWhTaxTotal);
				}
				decimal doc_CuryInclTaxTotal = (decimal)(ParentGetValue(sender.Graph, _CuryInclTaxTotal) ?? Decimal.Zero);
				if (object.Equals(CuryInclTaxTotal, doc_CuryInclTaxTotal) == false)
				{
					ParentSetValue(sender.Graph, _CuryInclTaxTotal, CuryInclTaxTotal);
				}
			}
		}

		protected bool _InternalCall = false;

		public override void RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			GLTranDoc doc = (GLTranDoc)e.Row;
			if (doc.ParentLineNbr != null)
			{
				_ParentRow = PXParentAttribute.SelectParent(sender, e.Row, typeof(GLTranDoc));

				if (_ParentRow != null)
				{
					_ParentRow = PXCache<GLTranDoc>.CreateCopy((GLTranDoc)_ParentRow);
				}
				if (doc.CuryTranTotal != doc.CuryTranAmt)
				{
					doc.CuryTranAmt = doc.CuryTranTotal;
				}
				base.RowInserted(sender, e);
				Object current = sender.Current;
				sender.Update(_ParentRow);
				sender.Current = current;
			}
			else
			{
				_ParentRow = doc;
				base.RowInserted(sender, e);
				if (doc.IsChildTran==true)
				{
					doc.CuryTranAmt = this.CalcTaxableFromTotal(sender, doc, doc.CuryTranTotal.Value); 
				}
				else if(doc.Split==false)
				{
					doc.CuryTranAmt = this.CalcTaxableFromTotal(sender, doc, doc.CuryTranTotal.Value);
					CalcTaxes(sender, doc, PXTaxCheck.Line);
				}
			}
		}

		public override void RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			GLTranDoc doc = (GLTranDoc)e.Row;
			if (doc.ParentLineNbr != null)
			{
				_ParentRow = PXParentAttribute.SelectParent(sender, e.Row, typeof(GLTranDoc));
				if (_ParentRow != null)
				{
					_ParentRow = PXCache<GLTranDoc>.CreateCopy((GLTranDoc)_ParentRow);
				}

				if (doc.CuryTranTotal != ((GLTranDoc)e.OldRow).CuryTranTotal)
				{
					doc.CuryTranAmt = doc.CuryTranTotal;
				}
				base.RowUpdated(sender, e);
				Object current = sender.Current;
				sender.Update(_ParentRow);
				sender.Current = current;
			}
			else
			{
				_ParentRow = doc;
				GLTranDoc oldRow = ((GLTranDoc)e.OldRow);
				if (doc.Split == false && doc.CuryTranTotal != oldRow.CuryTranTotal) 
				{
					doc.CuryTranAmt = this.CalcTaxableFromTotal(sender, doc, doc.CuryTranTotal.Value);
				}				
				GLTranDoc copy = PXCache<GLTranDoc>.CreateCopy((GLTranDoc)_ParentRow);
				base.RowUpdated(sender, e);
				if ((doc.TaxCategoryID != oldRow.TaxCategoryID || doc.TaxZoneID != oldRow.TaxZoneID || doc.TranDate != oldRow.TranDate || doc.Split != oldRow.Split || doc.TermsID != oldRow.TermsID) && (doc.Split == false)) 
				{
					
					doc.CuryTranAmt = this.CalcTaxableFromTotal(sender, doc, doc.CuryTranTotal.Value);
					CalcTaxes(sender, doc, PXTaxCheck.Line);
					
				}
				if (_TaxCalc != TaxCalc.NoCalc && _TaxCalc != TaxCalc.ManualLineCalc)
				{
					PXRowUpdatedEventArgs e1 = new PXRowUpdatedEventArgs(e.Row, copy, e.ExternalCall);
					for (int i = 0; i < _Attributes.Count; i++)
					{
						if (_Attributes[i] is IPXRowUpdatedSubscriber)
						{
							((IPXRowUpdatedSubscriber)_Attributes[i]).RowUpdated(sender, e1);
						}
					}
				}				
			}
		}

		public override void RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
			GLTranDoc doc = (GLTranDoc)e.Row;
			if (doc.ParentLineNbr != null)
			{
				_ParentRow = PXParentAttribute.SelectParent(sender, e.Row, typeof(GLTranDoc));

				if (_ParentRow != null)
				{
					_ParentRow = PXCache<GLTranDoc>.CreateCopy((GLTranDoc)_ParentRow);
				}
				base.RowDeleted(sender, e);
				Object current = sender.Current;
				sender.Update(_ParentRow);
				sender.Current = current;
			}
			else
			{
				_ParentRow = doc;
				base.RowDeleted(sender, e);
			}
		}

		protected override List<object> ChildSelect(PXCache cache, object data)
		{
			GLTranDoc doc = (GLTranDoc)data;
			if (doc.Split == false && doc.ParentLineNbr == null)
			{
				return new List<object> { data };
			}
			else
			{
				return base.ChildSelect(cache, data);
			}
		}

		public Decimal CalcTaxableFromTotal(PXCache cache, object row, Decimal aCuryTotal) 
		{
			Decimal result = decimal.Zero;
			List<object> taxes = SelectTaxes<Where<True,Equal<True>>>(cache.Graph, row, PXTaxCheck.Line);
			Terms terms = this.SelectTerms(cache.Graph);
			Decimal rate = Decimal.Zero;
			bool haveTaxes = false;
			bool haveTaxOnTax = false;
			foreach (PXResult<GLTax, Tax, TaxRev> iRes in taxes)
			{
				Tax tax = iRes;
				TaxRev taxRev = iRes;
				Decimal multiplier = tax.ReverseTax == true ? Decimal.MinusOne : Decimal.One;
				if (tax.TaxCalcLevel == CSTaxCalcLevel.Inclusive) continue;
				if (tax.TaxCalcLevel == CSTaxCalcLevel.CalcOnItemAmt)
				{
					haveTaxes = true;
					decimal termsFactor = Decimal.Zero;
					if(tax.TaxApplyTermsDisc == CSTaxTermsDiscount.ToTaxableAmount) 
						termsFactor = ((decimal)(terms.DiscPercent ?? 0m) / 100m);
					rate += multiplier * taxRev.TaxRate.Value * (1m - termsFactor);
				}
				if (tax.TaxCalcLevel == CSTaxCalcLevel.CalcOnItemAmtPlusTaxAmt && haveTaxes)
				{
					haveTaxOnTax = true;
					break;
				}
			}

			if (haveTaxOnTax)
				throw new PXException("Taxable amount can not be calculated - tax on taxes are defined");

			result = PXDBCurrencyAttribute.RoundCury(cache, row, aCuryTotal / (1 + rate / 100));
			Decimal curyTaxTotal = decimal.Zero;
			foreach (PXResult<GLTax, Tax, TaxRev> iRes in taxes)			
			{
				Tax tax = iRes;
				TaxRev taxRev = iRes;
				Decimal multiplier = tax.ReverseTax == true ? Decimal.MinusOne : Decimal.One;
				if (tax.TaxCalcLevel == CSTaxCalcLevel.Inclusive) continue;
				if (tax.TaxCalcLevel == CSTaxCalcLevel.CalcOnItemAmt)
				{
					decimal termsFactor = Decimal.Zero;
					if (tax.TaxApplyTermsDisc == CSTaxTermsDiscount.ToTaxableAmount)
						termsFactor = ((decimal)(terms.DiscPercent ?? 0m) / 100m);					
					Decimal curyTaxAmt = multiplier * (result *(1 - termsFactor) * taxRev.TaxRate / 100m)?? Decimal.Zero;
					if (tax.TaxApplyTermsDisc == CSTaxTermsDiscount.ToTaxAmount) 
					{
						curyTaxAmt *= (1 - (decimal)(terms.DiscPercent ?? 0m) / 100m);
					}
				    if (tax.TaxType != CSTaxType.Use)
				    {
					curyTaxTotal += PXDBCurrencyAttribute.RoundCury(cache, row, curyTaxAmt);
				}
			}
			}
			result = aCuryTotal - curyTaxTotal;
			return result;		
		}
	}

	#region generalLedgerModule

	public sealed class generalLedgerModule : PX.Data.BQL.BqlString.Constant<generalLedgerModule>
	{
		public generalLedgerModule() : base(typeof(generalLedgerModule).Namespace){}
	}

	#endregion

	#region accountType
	public sealed class accountType : PX.Data.BQL.BqlString.Constant<accountType>
	{
		public accountType()
			: base(typeof(PX.Objects.GL.Account).FullName)
		{
		}
	}
	#endregion
	#region subType
	public sealed class subType : PX.Data.BQL.BqlString.Constant<subType>
	{
		public subType()
			: base(typeof(PX.Objects.GL.Sub).FullName)
		{
		}
	}
	#endregion

	#region budgetType
	public sealed class budgetType : PX.Data.BQL.BqlString.Constant<budgetType>
	{
		public budgetType()
			: base(typeof(PX.Objects.GL.GLBudgetLine).FullName)
		{
		}
	}
	#endregion

	#region printerType
	public sealed class printerType : PX.Data.BQL.BqlString.Constant<printerType>
	{
		public printerType()
			: base(typeof(PX.SM.SMPrinter).FullName)
		{
		}
	}
	#endregion

	#region branchType
	public sealed class branchType : PX.Data.BQL.BqlString.Constant<branchType>
	{
		public branchType()
			: base(typeof(PX.Objects.GL.Branch).FullName)
		{
		}
	}
	#endregion

	public sealed class RunningFlagScope<KeyGraphType> : IDisposable where KeyGraphType : PXGraph
	{
		private class BoolWrapper
		{
			public bool Value { get; set; }
			public BoolWrapper () { this.Value = false; }
		}

		private static string _SLOT_KEY = string.Format("{0}_Running", typeof(KeyGraphType).Name);

		public RunningFlagScope()
		{
			BoolWrapper val = PXDatabase.GetSlot<BoolWrapper>(_SLOT_KEY);
			val.Value = true;
		}

		public void Dispose()
		{
			PXDatabase.ResetSlot<BoolWrapper>(_SLOT_KEY);
		}

		public static bool IsRunning
		{
			get
			{
				return PXDatabase.GetSlot<BoolWrapper>(_SLOT_KEY).Value;
			}
		}
	}

	public class PXRequiredExprAttribute : PXEventSubscriberAttribute
	{
		protected IBqlWhere _Where;

		public PXRequiredExprAttribute(Type where)
		{
			if (where == null)
			{
				throw new PXArgumentException("where", ErrorMessages.ArgumentNullException);
			}
			else if (typeof(IBqlWhere).IsAssignableFrom(where))
			{
				_Where = (IBqlWhere)Activator.CreateInstance(where);
			}
			else
			{
				throw new PXArgumentException("where", ErrorMessages.ArgumentException);
			}

		}

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);
			sender.Graph.RowPersisting.AddHandler(sender.GetItemType(), RowPersisting);
		}

		public void RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			if (sender.GetStatus(e.Row) == PXEntryStatus.Updated || sender.GetStatus(e.Row) == PXEntryStatus.Inserted)
			{
				bool? result = null;
				object value = null;
				_Where.Verify(sender, e.Row, new List<object>(), ref result, ref value);
				PXDefaultAttribute.SetPersistingCheck(sender, FieldName, e.Row, result.HasValue && result.Value ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);
			}
		}
	}
}



