using System;
using System.Collections.Generic;
using System.Linq;
using PX.Common;
using PX.Data;
using PX.Objects.Common;
using PX.Objects.Common.Abstractions.Periods;
using PX.Objects.Common.Extensions;
using PX.Objects.GL.Descriptor;

namespace PX.Objects.GL
{
    public abstract class OrganizationDependedPeriodIDAttribute : PeriodIDAttribute, IPXFieldDefaultingSubscriber
    {
        #region State

        public override Type SearchType
        {
            get { return base.SearchType; }
            set
            {
                base.SearchType = value;

                if (FilterByOrganizationID)
                {
                    SearchTypeRestrictedByOrganization = _searchType != null
                        ? GetQueryWithRestrictionByOrganization(_searchType)
                        : null;
                }
            }
        }

        public Type SearchTypeRestrictedByOrganization { get; set; }

        public override Type DefaultType
        {
            get { return base.DefaultType; }
            set
            {
                base.DefaultType = value;

                if (FilterByOrganizationID)
                {
                    DefaultTypeRestrictedByOrganization = _defaultType != null
                        ? GetQueryWithRestrictionByOrganization(_defaultType)
                        : null;
                }
            }
        }

        public Type DefaultTypeRestrictedByOrganization { get; set; }

        public bool RedefaultOrRevalidateOnOrganizationSourceUpdated { get; set; }

        public bool FilterByOrganizationID { get; set; }
        public bool UseMasterOrganizationIDByDefault { get; set; }

        public IPeriodKeyProvider<
                OrganizationDependedPeriodKey,
                PeriodKeyProviderBase.SourcesSpecificationCollectionBase>
            PeriodKeyProvider
        { get; set; }

        #endregion

        #region Ctor
        public OrganizationDependedPeriodIDAttribute(
            Type dateType = null,
            Type searchByDateType = null,
            Type defaultType = null,
            bool redefaultOrRevalidateOnOrganizationSourceUpdated = true,
            bool useMasterOrganizationIDByDefault = false,
            bool filterByOrganizationID = true,
            bool redefaultOnDateChanged = true)
            : base(
                sourceType: dateType,
                searchType: searchByDateType,
                defaultType: defaultType,
                redefaultOnDateChanged: redefaultOnDateChanged)
        {
            FilterByOrganizationID = filterByOrganizationID;

            DefaultType = DefaultType;
            SearchType = SearchType;

            UseMasterOrganizationIDByDefault = useMasterOrganizationIDByDefault;

            RedefaultOrRevalidateOnOrganizationSourceUpdated = redefaultOrRevalidateOnOrganizationSourceUpdated;
        }

        public CalendarOrganizationIDProvider.SourceSpecificationItem GetMainSpecificationItem(PXCache cache, object row)
        {
            return PeriodKeyProvider.GetMainSourceSpecificationItem(cache, row);
        }

        public CalendarOrganizationIDProvider.SourceSpecificationItem MainSpecificationItem
        {
            get { return GetMainSpecificationItem(null, null); }
        }

        public Type BranchSourceType
        {
            get{ return GetMainSpecificationItem(null, null)?.BranchSourceType; }
        }

        public Type BranchSourceFormulaType
        {
            get { return GetMainSpecificationItem(null, null)?.BranchSourceFormulaType; }
        }

        public Type OrganizationSourceType
        {
            get { return GetMainSpecificationItem(null, null)?.OrganizationSourceType; }
        }

        #endregion

        #region Initialization

        public override void CacheAttached(PXCache sender)
        {
            base.CacheAttached(sender);

            sender.Graph.RowPersisting.AddHandler(sender.GetItemType(), RowPersisting);

            if (PeriodKeyProvider != null)
            {
                HashSet<PXCache> subscribedCaches = new HashSet<PXCache>();

                foreach (CalendarOrganizationIDProvider.SourceSpecificationItem sourceSpecification in PeriodKeyProvider.GetSourceSpecificationItems(sender, null))
                {
                    SubscribeForSourceSpecificationItem(sender, subscribedCaches, sourceSpecification);
                }
            }
        }

        public virtual void SubscribeForSourceSpecificationItem(PXCache sender, HashSet<PXCache> subscribedCaches, CalendarOrganizationIDProvider.SourceSpecificationItem sourceSpecification)
        {
            if (sourceSpecification.IsAnySourceSpecified)
            {
                if (!subscribedCaches.Contains(sender))
                {
                    SubscribeForSourceSpecificationItemImpl(sender, subscribedCaches, sourceSpecification);

                    subscribedCaches.Add(sender);
                }
            }
        }

        public virtual void SubscribeForSourceSpecificationItemImpl(PXCache sender, HashSet<PXCache> subscribedCaches,
            CalendarOrganizationIDProvider.SourceSpecificationItem sourceSpecification)
        {
            if (!sender.Graph.IsImport && !sender.Graph.IsContractBasedAPI)
            {
                sender.Graph.RowUpdating.AddHandler(sender.GetItemType(), RowUpdating);
            }

            sender.Graph.RowUpdated.AddHandler(sender.GetItemType(), RowUpdated);
        }

        protected abstract Type GetQueryWithRestrictionByOrganization(Type bqlQueryType);

        protected override Type GetExecutableDefaultType() => DefaultTypeRestrictedByOrganization ?? DefaultType;

        #endregion

        #region Implementation

        protected override bool IsSourcesValuesDefined(PXCache cache, object row)
        {
            return base.IsSourcesValuesDefined(cache, row)
                   && PeriodKeyProvider.IsKeyDefined(cache.Graph, cache, row);
        }

        protected override object[] GetPeriodKey(PXCache cache, object row)
        {
            return PeriodKeyProvider.GetKeyAsArrayOfObjects(cache.Graph, cache, row);
        }

        #region Handlers

        public virtual void FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            GetFields(sender, e.Row);

			Type defaultType = DefaultTypeRestrictedByOrganization ?? DefaultType;

			if (defaultType != null && (_sourceDate != DateTime.MinValue ||
                (SourceType != null && (BranchSourceType != null || OrganizationSourceType != null))))
            {
                e.NewValue = (string)GetPeriod(sender.Graph,
                    defaultType,
                    new QueryParams(){SourceDate = _sourceDate == DateTime.MinValue ? (DateTime?)null : _sourceDate }, 
                    PeriodKeyProvider.GetKeyAsArrayOfObjects(sender.Graph, sender, e.Row),
                    e.Row.SingleToListOrNull(),
                    DefaultingQueryParametersDelegate);
            }
        }

        protected virtual void RowUpdating(PXCache cache, PXRowUpdatingEventArgs e)
        {
            if (cache.Graph.UnattendedMode)
                return;

            ValidatePeriodAndSources(cache, e.Row, e.NewRow, e.ExternalCall);
        }

        protected virtual void ValidatePeriodAndSources(PXCache cache, object oldRow, object newRow, bool externalCall)
        {
            if (PeriodSourceFieldsEqual(cache, oldRow, newRow))
                return;

            ValidatePeriodAndSourcesImpl(cache, oldRow, newRow, externalCall);
        }

        protected abstract void ValidatePeriodAndSourcesImpl(PXCache cache, object oldRow, object newRow,
            bool externalCall);

        protected virtual void RowUpdated(PXCache cache, PXRowUpdatedEventArgs e)
        {
            if (PeriodSourceFieldsEqual(cache, e.OldRow, e.Row))
                return;

            RowUpdatedImpl(cache, e);
        }

        protected virtual void RowUpdatedImpl(PXCache cache, PXRowUpdatedEventArgs e)
        {
            if (RedefaultOrRevalidateOnOrganizationSourceUpdated)
            {
                object value = null;
                object errorValue = null;
                bool hasError = false;

                foreach (PXEventSubscriberAttribute attribute in cache.GetAttributesReadonly(e.Row, _FieldName))
                {
                    IPXInterfaceField uiFieldAttribute = attribute as IPXInterfaceField;

                    if (uiFieldAttribute != null)
                    {
                        hasError = uiFieldAttribute.ErrorLevel == PXErrorLevel.Error || uiFieldAttribute.ErrorLevel == PXErrorLevel.RowError;

                        if (hasError
                            || uiFieldAttribute.ErrorLevel == PXErrorLevel.Warning
                            || uiFieldAttribute.ErrorLevel == PXErrorLevel.RowWarning)
                        {
                            errorValue = uiFieldAttribute.ErrorValue;

                            value = hasError
                                ? errorValue
                                : FormatForDisplay((string)cache.GetValue(e.Row, _FieldName));

                            cache.RaiseExceptionHandling(_FieldName, e.Row, value, null);
                        }
                    }
                }

                OrganizationDependedPeriodKey newPeriodKey = GetFullKey(cache, e.Row);

                OrganizationDependedPeriodKey oldPeriodKey = GetFullKey(cache, e.OldRow);

                if (ShouldExecuteRedefaultFinPeriodIDonRowUpdated(errorValue, hasError, newPeriodKey, oldPeriodKey))
                {
                    RedefaultPeriodID(cache, e.Row);
                }
                else if (!newPeriodKey.IsNotPeriodPartsEqual(oldPeriodKey)
                         && oldPeriodKey.PeriodID != null
                         && !cache.Graph.IsContractBasedAPI
                         && !cache.Graph.UnattendedMode
                         && !cache.Graph.IsImport
                         && !cache.Graph.IsExport)
                {
                    OrganizationDependedPeriodKey basePeriodForMapping = GetFullKey(cache, e.OldRow);

                    if (hasError)
                    {
                        basePeriodForMapping.PeriodID = UnFormatPeriod((string)errorValue);
                    }
                    else if (oldPeriodKey.PeriodID != newPeriodKey.PeriodID)
                    {
                        basePeriodForMapping.PeriodID = newPeriodKey.PeriodID;
                    }

                    string mappedPeriod = GetMappedPeriodID(cache, newPeriodKey, basePeriodForMapping);

                    cache.SetValueExt(e.Row, _FieldName, FormatForDisplay(mappedPeriod));
                }
                else
                {
                    cache.SetValueExt(e.Row, _FieldName, FormatForDisplay(newPeriodKey.PeriodID));
                }
            }
        }

        public virtual OrganizationDependedPeriodKey GetFullKey(PXCache cache, object row)
        {
            OrganizationDependedPeriodKey key = PeriodKeyProvider.GetKey(cache.Graph, cache, row);
            key.PeriodID = (string)cache.GetValue(row, _FieldName);

            return key;
        }

        protected abstract string GetMappedPeriodID(PXCache cache, OrganizationDependedPeriodKey newPeriodKey,
            OrganizationDependedPeriodKey oldPeriodKey);

        public virtual void RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
        {
            if (BranchSourceType != null
                && (sender.Graph.IsImport || sender.Graph.IsContractBasedAPI || sender.Graph.UnattendedMode)
                && (e.Operation & PXDBOperation.Command).IsIn(PXDBOperation.Insert, PXDBOperation.Update))
            {
                ValidatePeriodAndSources(sender,
                    sender.GetOriginal(e.Row),
                    e.Row,
                    !sender.Graph.UnattendedMode || sender.Graph.IsContractBasedAPI);
            }
        }

        #endregion

        #region Help methods

        protected virtual bool ShouldExecuteRedefaultFinPeriodIDonRowUpdated(object errorValue,
            bool hasError,
            OrganizationDependedPeriodKey newPeriodKey,
            OrganizationDependedPeriodKey oldPeriodKey)
        {
            return errorValue == null && hasError
                    || newPeriodKey.PeriodID == null && oldPeriodKey.PeriodID == null
                    || newPeriodKey.IsMasterCalendar && !oldPeriodKey.IsMasterCalendar
                    || !newPeriodKey.IsMasterCalendar && oldPeriodKey.IsMasterCalendar;
        }

        protected void SetErrorAndResetToOldForField(PXCache cache, object oldRow, object newRow, string fieldName, PXSetPropertyException exception, bool externalCall)
        {
            object branchSourceNewValue = GetNewValueByIncomig(
                cache,
                newRow,
                fieldName,
                externalCall);

            cache.RaiseExceptionHandling(
                fieldName,
                newRow,
                branchSourceNewValue,
                exception);

            cache.SetValue(
                newRow,
                fieldName,
                cache.GetValue(oldRow, fieldName));
        }

        protected virtual bool PeriodSourceFieldsEqual(PXCache cache, object oldRow, object newRow)
        {
			if (oldRow != null && newRow == null
				|| oldRow == null && newRow != null)
				return false;

	        string newOrgFinPeriodID = (string)cache.GetValue(newRow, _FieldName);
	        string oldOrgFinPeriodID = (string)cache.GetValue(oldRow, _FieldName);

            return PeriodKeyProvider.IsKeySourceValuesEquals(cache, oldRow, newRow)
                   && newOrgFinPeriodID == oldOrgFinPeriodID;
        }

        protected object GetNewValueByIncomig(PXCache cache, object row, string fieldName, bool externalCall)
        {
            object incoming = null;

            if (externalCall)
            {
                incoming = cache.GetValuePending(row, fieldName);

                if (incoming != null)
                {
                    return incoming;
                }
            }

            try
            {
                incoming = cache.GetValueExt(row, fieldName);

                var state = incoming as PXFieldState;

                if (state != null)
                {
                    return state.Value;
                }

                if (incoming != null)
                {
                    return incoming;
                }
            }
            catch
            {
            }

            return cache.GetValue(row, fieldName);
        }

        #endregion

        #endregion
    }
}
