using System;
using System.Collections.Generic;
using System.Linq;
using PX.Common;
using PX.Data;
using PX.Objects.Common;
using PX.Objects.Common.Abstractions.Periods;
using PX.Objects.Common.Extensions;
using PX.Objects.CS;
using PX.Objects.GL.Descriptor;
using PX.Objects.GL.FinPeriods;
using PX.Objects.GL.FinPeriods.TableDefinition;

namespace PX.Objects.GL
{
    /// <summary>
	/// Attribute describes FinPeriod Field.
	/// This attribute contains static Util functions.
	/// </summary>
	public class FinPeriodIDAttribute : OrganizationDependedPeriodIDAttribute
    {
        #region Types

        public class ValidationResult
        {
            public HashSet<int?> OrganizationIDsWithErrors;
            public HashSet<int?> BranchIDsWithErrors;
            public List<CalendarOrganizationIDProvider.KeyWithSourceValues> BranchValuesWithErrors;

            public bool HasErrors => BranchValuesWithErrors.Any();

            public ValidationResult()
            {
                OrganizationIDsWithErrors = new HashSet<int?>();
                BranchIDsWithErrors = new HashSet<int?>();
                BranchValuesWithErrors = new List<CalendarOrganizationIDProvider.KeyWithSourceValues>();
            }

            public List<string> GetOrganizationCDsWithErrors()
            {
                return OrganizationIDsWithErrors.Select(PXAccess.GetOrganizationCD).ToList();
            }

            public List<string> GetBranchCDsWithErrors()
            {
                return BranchIDsWithErrors.Select(PXAccess.GetBranchCD).ToList();
            }
        }

        public enum HeaderFindingModes
        {
            Current,
            Parent
        }

        #endregion

        public HeaderFindingModes HeaderFindingMode { get; set; }

        public bool IsHeader { get; set; }

        public bool CalculatePeriodByHeader { get; set; }

        public bool AutoCalculateMasterPeriod { get; set; }

        public Type MasterFinPeriodIDType { get; set; }

        public Type HeaderMasterFinPeriodIDType { get; set; }

        public Type UseMasterCalendarSourceType { get; set; }

        public ICalendarOrganizationIDProvider CalendarOrganizationIDProvider { get; protected set; }

		[InjectDependency]
		public IFinPeriodRepository FinPeriodRepository { get; set; }

        #region Ctor & Setup
        public FinPeriodIDAttribute(
            Type sourceType = null,
            Type branchSourceType = null,
            Type branchSourceFormulaType = null,
            Type organizationSourceType = null,
            Type useMasterCalendarSourceType = null,
            Type defaultType = null,
            bool redefaultOrRevalidateOnOrganizationSourceUpdated = true,
            bool useMasterOrganizationIDByDefault = false,
            Type[] sourceSpecificationTypes = null,
            Type masterFinPeriodIDType = null,
            Type headerMasterFinPeriodIDType = null,
            bool filterByOrganizationID = true,
            bool redefaultOnDateChanged = true)
            : base(
                dateType: sourceType,
                searchByDateType: typeof(Search<FinPeriod.finPeriodID,
                                            Where<FinPeriod.startDate, LessEqual<Current2<QueryParams.sourceDate>>,
                                                    And<FinPeriod.endDate, Greater<Current2<QueryParams.sourceDate>>>>>),
                defaultType: defaultType,
                redefaultOrRevalidateOnOrganizationSourceUpdated: redefaultOrRevalidateOnOrganizationSourceUpdated,
                filterByOrganizationID: true,
                useMasterOrganizationIDByDefault: useMasterOrganizationIDByDefault,
                redefaultOnDateChanged: redefaultOnDateChanged)
        {
            HeaderFindingMode = HeaderFindingModes.Current;
            CalculatePeriodByHeader = true;
            AutoCalculateMasterPeriod = true;

            MasterFinPeriodIDType = masterFinPeriodIDType;

            HeaderMasterFinPeriodIDType = headerMasterFinPeriodIDType;

            FilterByOrganizationID = filterByOrganizationID;
            UseMasterOrganizationIDByDefault = useMasterOrganizationIDByDefault;

            UseMasterCalendarSourceType = useMasterCalendarSourceType;

            RedefaultOrRevalidateOnOrganizationSourceUpdated = redefaultOrRevalidateOnOrganizationSourceUpdated;

            PeriodKeyProvider =
            CalendarOrganizationIDProvider = new CalendarOrganizationIDProvider(
                new PeriodKeyProviderBase.SourcesSpecificationCollection()
                {
                    SpecificationItems = new PeriodKeyProviderBase.SourceSpecificationItem()
                    {
                        BranchSourceType = branchSourceType,
                        BranchSourceFormulaType = branchSourceFormulaType,
                        OrganizationSourceType = organizationSourceType,
                    }.SingleToList()
                },
                sourceSpecificationTypes: sourceSpecificationTypes,
                useMasterCalendarSourceType: useMasterCalendarSourceType,
                useMasterOrganizationIDByDefault: useMasterOrganizationIDByDefault);
        }

        public override void CacheAttached(PXCache sender)
        {
            base.CacheAttached(sender);


            if (MasterFinPeriodIDType != null)
            {
                sender.Graph.FieldDefaulting.AddHandler(sender.GetItemType(), MasterFinPeriodIDType.Name, MasterFinPeriodIDFieldDefaulting);
            }

            if (UseMasterCalendarSourceType != null)
            {
                if (OrganizationSourceType != null)
                {
                    sender.Graph.FieldUpdated.AddHandler(
                        BqlCommand.GetItemType(OrganizationSourceType),
                        OrganizationSourceType.Name,
                        CalendarOrganizationIDSourceFieldUpdated);
                }

                if (BranchSourceType != null)
                {
                    sender.Graph.FieldUpdated.AddHandler(
                        BqlCommand.GetItemType(BranchSourceType),
                        BranchSourceType.Name,
                        CalendarOrganizationIDSourceFieldUpdated);
                }
            }
        }

        public override void SubscribeForSourceSpecificationItemImpl(PXCache sender, HashSet<PXCache> subscribedCaches,
            CalendarOrganizationIDProvider.SourceSpecificationItem sourceSpecification)
        {
            base.SubscribeForSourceSpecificationItemImpl(sender, subscribedCaches, sourceSpecification);

            if (MasterFinPeriodIDType != null)
            {
                sender.Graph.RowInserted.AddHandler(sender.GetItemType(), RowInserted);
            }
        }

        protected override Type GetQueryWithRestrictionByOrganization(Type bqlQueryType)
        {
            return BqlCommand.CreateInstance(bqlQueryType)
                .WhereAnd(typeof(Where<FinPeriod.organizationID, Equal<Required<FinPeriod.organizationID>>>))
                .GetType();
        }

        #endregion


        #region  Handlers

        public override void FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            SetUseMasterCalendarValue(sender, e.Row);

            if (HeaderMasterFinPeriodIDType != null && CalculatePeriodByHeader)
            {
                int? calendarOrgID = CalendarOrganizationIDProvider.GetCalendarOrganizationID(sender.Graph, sender, e.Row);
				
                FinPeriod orgFinPeriod =
                    FinPeriodRepository.GetFinPeriodByMasterPeriodID(
                            calendarOrgID,
                            GetHeaderMasterFinPeriodID(sender, e.Row))
                        .Result;

                if (orgFinPeriod != null)
                {
                    e.NewValue = FormatForDisplay(orgFinPeriod.FinPeriodID);
                }
            }
            else
            {
                base.FieldDefaulting(sender, e);
            }
        }

        public virtual void MasterFinPeriodIDFieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            e.NewValue = FormatForDisplay(CalcMasterPeriodID(sender, e.Row));
        }

        protected virtual void CalendarOrganizationIDSourceFieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            SetUseMasterCalendarValue(cache, e.Row);
        }

        public void RowInserted(PXCache cache, PXRowInsertedEventArgs e)
        {
            if (AutoCalculateMasterPeriod)
            {
                SetMasterPeriodID(cache, e.Row);
            }
        }

        protected override void RowUpdatedImpl(PXCache cache, PXRowUpdatedEventArgs e)
        {
            base.RowUpdatedImpl(cache, e);

            if (AutoCalculateMasterPeriod)
            {
                SetMasterPeriodID(cache, e.Row);
            }
        }

        #endregion


        #region Implementation

        protected override bool PeriodSourceFieldsEqual(PXCache cache, object oldRow, object newRow)
        {
            bool res = base.PeriodSourceFieldsEqual(cache, oldRow, newRow);

            if (UseMasterCalendarSourceType != null)
            {
                res &= (bool?) cache.GetValue(newRow, UseMasterCalendarSourceType.Name) ==
                        (bool?) cache.GetValue(oldRow, UseMasterCalendarSourceType.Name);
            }

            return res;
        }

        protected void SetErrorAndResetForMainFields(PXCache cache, object oldRow, object newRow, int? oldCalendarID, int? newCalendarID, bool externalCall,
            PXSetPropertyException exception)
        {
            cache.RaiseExceptionHandling(
                _FieldName,
                newRow,
                FormatForDisplay((string)cache.GetValue(newRow, _FieldName)),
                exception);

            cache.SetValue(
                newRow,
                _FieldName,
                cache.GetValue(oldRow, _FieldName));

            if (oldCalendarID != newCalendarID)
            {
                SetErrorAndResetToOldForField(
                    cache,
                    oldRow,
                    newRow,
                    MainSpecificationItem.BranchSourceType.Name,
                    exception,
                    externalCall);

                if (MainSpecificationItem.OrganizationSourceType != null)
                {
                    SetErrorAndResetToOldForField(
                        cache,
                        oldRow,
                        newRow,
                        MainSpecificationItem.OrganizationSourceType.Name,
                        exception,
                        externalCall);
                }
            }
        }

        public static ValidationResult ValidateRowLevelSources(
        ICalendarOrganizationIDProvider calendarOrganizationIDProvider,
        PXCache cache,
        object row,
        Func<int?, bool?> /*<Calendar OrganizationID, result>*/ validationDelegate,
        bool skipMain = false)
        {
            ValidationResult result = new ValidationResult();

            List<CalendarOrganizationIDProvider.KeyWithSourceValues> branchValues =
                calendarOrganizationIDProvider.GetSourcesSpecification(cache, row).SpecificationItems
                    .Where(specification => !specification.IsMain || !skipMain)
                    .Select(specification => calendarOrganizationIDProvider.GetBranchIDsValueFromField(cache.Graph, cache, row, specification))
                    .ToList();

            foreach (CalendarOrganizationIDProvider.KeyWithSourceValues branchValue in branchValues)
            {
                bool hasError = false;

                foreach (int? branchID in branchValue.SourceBranchIDs)
                {
                    if (branchID == null)
                        continue;

                    bool branchError = false;
                    int? organizationID = PXAccess.GetParentOrganizationID(branchID);

                    if (!result.OrganizationIDsWithErrors.Contains(organizationID))
                    {
                        if (validationDelegate(organizationID) == false)
                        {
                            result.OrganizationIDsWithErrors.Add(organizationID);
                            branchError = true;
                            hasError = true;
                        }
                    }
                    else
                    {
                        branchError = true;
                        hasError = true;
                    }

                    if (branchError)
                    {
                        if (!result.BranchIDsWithErrors.Contains(branchID))
                        {
                            result.BranchIDsWithErrors.Add(branchID);
                        }
                    }
                }

                if (hasError)
                {
                    result.BranchValuesWithErrors.Add(branchValue);
                }
            }

            return result;
        }

        protected override string GetMappedPeriodID(PXCache cache, OrganizationDependedPeriodKey newPeriodKey,
            OrganizationDependedPeriodKey oldPeriodKey)
        {
            FinPeriod mappedFinPeriod = FinPeriodRepository.GetMappedPeriod(oldPeriodKey.OrganizationID, oldPeriodKey.PeriodID, newPeriodKey.OrganizationID);

            return mappedFinPeriod?.FinPeriodID;
        }

        protected override void ValidatePeriodAndSourcesImpl(PXCache cache, object oldRow, object newRow,
            bool externalCall)
        {
            int? newMainCalendarOrgID = CalendarOrganizationIDProvider.GetCalendarOrganizationID(cache.Graph, cache, newRow);
            int? oldMainCalendarOrgID = CalendarOrganizationIDProvider.GetCalendarOrganizationID(cache.Graph, cache, oldRow);

            if (HeaderMasterFinPeriodIDType != null && CalculatePeriodByHeader)
            {
                if (newMainCalendarOrgID != oldMainCalendarOrgID)
                {
                    string headerMasterPeriodID = GetHeaderMasterFinPeriodID(cache, newRow);

                    if (headerMasterPeriodID != null)
                    {
                        ProcessingResult<FinPeriod> procResult = FinPeriodRepository.GetFinPeriodByMasterPeriodID(newMainCalendarOrgID, headerMasterPeriodID);

                        if (!procResult.IsSuccess)
                        {
                            SetErrorAndResetToOldForField(
                                cache,
                                oldRow,
                                newRow,
                                CalendarOrganizationIDProvider.GetSourcesSpecification(cache, newRow).MainSpecificationItem.BranchSourceType.Name,
                                new PXSetPropertyException(procResult.GetGeneralMessage()),
                                externalCall);
                        }
                    }
                }
            }
            else
            {
                FinPeriod newMainOrgFinPeriod = GetAndValidateMainFinPeriod(cache, oldRow, newRow, externalCall);

                if (newMainOrgFinPeriod == null)
                    return;

                ValidateRelatedToMainFinPeriods(cache, oldRow, newRow, externalCall, newMainCalendarOrgID, oldMainCalendarOrgID, newMainOrgFinPeriod);

                ValidateNotMainRowLevelSources(cache, oldRow, newRow, externalCall, newMainOrgFinPeriod);
            }
        }

        private void ValidateNotMainRowLevelSources(PXCache cache, object oldRow, object newRow, bool externalCall, FinPeriod newMainOrgFinPeriod)
        {
            ValidationResult validationResult = ValidateRowLevelSources(
                CalendarOrganizationIDProvider,
                cache,
                newRow,
                organizationID => FinPeriodRepository
                    .GetFinPeriodByMasterPeriodID(organizationID, newMainOrgFinPeriod.MasterFinPeriodID).IsSuccess,
                skipMain: true);

            if (validationResult.HasErrors)
            {
                foreach (var branchValue in validationResult.BranchValuesWithErrors)
                {
                    if (branchValue.SpecificationItem.BranchSourceType != null
                        && (branchValue.SpecificationItem.BranchSourceFormulaType != null ||
                            PXAccess.FeatureInstalled<FeaturesSet.branch>()))
                    {
						PXCache branchCache = cache.Graph.Caches[BqlCommand.GetItemType(branchValue.SpecificationItem.BranchSourceType)];
						object newBranch = branchCache.GetItemType().IsAssignableFrom(newRow.GetType())
							? newRow
							: branchCache.Current;
						object oldBranch = branchCache.GetItemType().IsAssignableFrom(oldRow.GetType())
							? oldRow
							: branchCache.Current;

						string organizationCD =
                            PXAccess.GetOrganizationCD(
                                PXAccess.GetParentOrganizationID(branchValue.SourceBranchIDs.Single()));

                        var exception = new PXSetPropertyException(
                            Messages.RelatedFinPeriodForMasterDoesNotExistForCompany,
                            FormatForError(newMainOrgFinPeriod.MasterFinPeriodID),
                            organizationCD);

                        SetErrorAndResetToOldForField(
                            branchCache,
                            oldBranch,
                            newBranch,
                            branchValue.SpecificationItem.BranchSourceType.Name,
                            exception,
                            externalCall);
                    }
                }
            }
        }

        protected virtual FinPeriod GetAndValidateMainFinPeriod(PXCache cache, object oldRow, object newRow, bool externalCall)
        {
            int? newMainCalendarOrgID = CalendarOrganizationIDProvider.GetCalendarOrganizationID(cache.Graph, cache, newRow);
            int? oldMainCalendarOrgID = CalendarOrganizationIDProvider.GetCalendarOrganizationID(cache.Graph, cache, oldRow);
			
            string newMainOrgFinPeriodID = null;
            FinPeriod newMainOrgFinPeriod = null;

            newMainOrgFinPeriodID = (string)cache.GetValue(newRow, _FieldName);

            if (newMainOrgFinPeriodID == null)
                return null;

            newMainOrgFinPeriod = FinPeriodRepository.FindByID(newMainCalendarOrgID, newMainOrgFinPeriodID);

            if (newMainOrgFinPeriod == null)
            {
                string errorMessage = null;

                if (newMainCalendarOrgID == FinPeriod.organizationID.MasterValue)
                {
                    errorMessage = PXMessages.LocalizeFormatNoPrefix(Messages.MasterFinPeriodDoesNotExist,
                        FormatForError(newMainOrgFinPeriodID));
                }
                else
                {
                    errorMessage = PXMessages.LocalizeFormatNoPrefix(
                        Messages.FinPeriodDoesNotExistForCompany,
                        FormatForError(newMainOrgFinPeriodID),
                        PXAccess.GetOrganizationCD(newMainCalendarOrgID));
                }

                SetErrorAndResetForMainFields(cache, oldRow, newRow, oldMainCalendarOrgID, newMainCalendarOrgID, externalCall,
                    new PXSetPropertyException(errorMessage));

                return null;
            }

            return newMainOrgFinPeriod;
        }

        /// <summary>
        /// Check All Organizations & Set Error to FinPeriod Field
        /// </summary>
        private void ValidateRelatedToMainFinPeriods(PXCache cache, object oldRow, object newRow, bool externalCall,
            int? newMainCalendarOrgID, int? oldMainCalendarOrgID, FinPeriod newMainOrgFinPeriod)
        {
            string oldMainOrgFinPeriodID = (string)cache.GetValue(oldRow, _FieldName);

            FinPeriod oldMainOrgFinPeriod = FinPeriodRepository.FindByID(oldMainCalendarOrgID, oldMainOrgFinPeriodID);

            if (newMainOrgFinPeriod.MasterFinPeriodID != oldMainOrgFinPeriod?.MasterFinPeriodID)
            {
                HashSet<int?> usedOrgIDs = new HashSet<int?>();

                if (IsHeader)
                {
                    usedOrgIDs.AddRange(CalendarOrganizationIDProvider.GetDetailOrganizationIDs(cache.Graph));
                }

                if (!usedOrgIDs.Any())
                    return;

                ProcessingResult existenceValidationResult =
                    FinPeriodRepository.FinPeriodsForMasterExist(newMainOrgFinPeriod.MasterFinPeriodID, usedOrgIDs.ToArray());

                if (!existenceValidationResult.IsSuccess)
                {
                    SetErrorAndResetForMainFields(cache, oldRow, newRow, oldMainCalendarOrgID, newMainCalendarOrgID,
                        externalCall,
                        new PXSetPropertyException(existenceValidationResult.GeneralMessage));
                }
            }
        }

        protected override void RedefaultPeriodID(PXCache cache, object row)
        {
            if (HeaderMasterFinPeriodIDType != null)
            {
                if (CalculatePeriodByHeader)
                {
                    string newFinPeriodID = null;

                    int? calendarOrgID = CalendarOrganizationIDProvider.GetCalendarOrganizationID(cache.Graph, cache, row);

                    if (calendarOrgID != null)
                    {
                        FinPeriod orgFinPeriod =
                            FinPeriodRepository.GetFinPeriodByMasterPeriodID(
                                    calendarOrgID,
                                    GetHeaderMasterFinPeriodID(cache, row))
                                .Result;

                        if (orgFinPeriod != null)
                        {
                            newFinPeriodID = FormatForDisplay(orgFinPeriod.FinPeriodID);
                        }
                    }

                    cache.SetValueExt(row, _FieldName, newFinPeriodID);
                }
            }
            else
            {
                base.RedefaultPeriodID(cache, row);

                if (AutoCalculateMasterPeriod)
                {
                    SetMasterPeriodID(cache, row);
                }
            }
        }

        protected override bool ShouldExecuteRedefaultFinPeriodIDonRowUpdated(object errorValue, bool hasError,
            OrganizationDependedPeriodKey newPeriodKey, OrganizationDependedPeriodKey oldPeriodKey)
        {
            var newKey = (FinPeriod.Key)newPeriodKey;
            var oldKey = (FinPeriod.Key)oldPeriodKey;

            return base.ShouldExecuteRedefaultFinPeriodIDonRowUpdated(errorValue, hasError, newKey, oldKey)
                || HeaderMasterFinPeriodIDType != null && oldKey.OrganizationID != newKey.OrganizationID;
        }

        protected virtual void SetUseMasterCalendarValue(PXCache sender, object row)
        {
            if (UseMasterCalendarSourceType == null)
                return;

            bool useMasterCalendarValue =
                CalendarOrganizationIDProvider.GetBranchIDsValueFromField(sender.Graph, sender, row,
                        CalendarOrganizationIDProvider.GetSourcesSpecification(sender, row).MainSpecificationItem)
                    .SourceBranchIDs.FirstOrDefault() == null
                && CalendarOrganizationIDProvider.GetOrganizationIDsValueFromField(sender.Graph, sender, row,
                        CalendarOrganizationIDProvider.GetSourcesSpecification(sender, row).MainSpecificationItem)
                    .SourceOrganizationIDs.FirstOrDefault() == null;

            sender.SetValueExt(row,
                UseMasterCalendarSourceType.Name,
                useMasterCalendarValue);
        }

        public virtual string GetHeaderMasterFinPeriodID(PXCache cache, object row)
        {
            if (HeaderFindingMode == HeaderFindingModes.Parent)
            {
                object parentRow = PXParentAttribute.SelectParent(cache, row, BqlCommand.GetItemType(HeaderMasterFinPeriodIDType));

                return (string)BqlHelper.GetOperandValue(cache.Graph, parentRow, HeaderMasterFinPeriodIDType);
            }

            return (string)BqlHelper.GetCurrentValue(cache.Graph, HeaderMasterFinPeriodIDType);
        }

        public virtual void SetMasterPeriodID(PXCache cache, object row)
        {
            if (MasterFinPeriodIDType == null)
                return;

            string oldMasterFinPeriodID = (string)cache.GetValue(row, MasterFinPeriodIDType.Name);
            string newMasterFinPeriodID = CalcMasterPeriodID(cache, row);

            if (newMasterFinPeriodID != oldMasterFinPeriodID)
            {
                cache.SetValueExt(row, MasterFinPeriodIDType.Name, FormatForDisplay(newMasterFinPeriodID));
            }
        }

        public virtual void DefaultPeriods(PXCache cache, object row)
        {
            cache.SetDefaultExt(row, _FieldName);

            if (MasterFinPeriodIDType != null)
            {
                cache.SetDefaultExt(row, MasterFinPeriodIDType.Name);
            }
        }

        public static void SetMasterPeriodID<TField>(PXCache cache, object row)
            where TField : IBqlField
        {
            cache.GetAttributesReadonly<TField>(row)
                .OfType<FinPeriodIDAttribute>().First()
                .SetMasterPeriodID(cache, row);
        }

        public static void DefaultPeriods<TField>(PXCache cache, object row)
            where TField : IBqlField
        {
            cache.GetAttributesReadonly<TField>(row)
                .OfType<FinPeriodIDAttribute>().First()
                .DefaultPeriods(cache, row);
        }

        public virtual string CalcMasterPeriodID(PXCache cache, object row)
        {
            string finPeriodID;

            finPeriodID = (string)cache.GetValue(row, _FieldName);

            int? calendarOrgID = CalendarOrganizationIDProvider.GetCalendarOrganizationID(cache.Graph, cache, row);

            if (calendarOrgID == FinPeriod.organizationID.MasterValue)
                return finPeriodID;

            string masterFinPeriodID = null;

            if (finPeriodID != null)
            {
                FinPeriod orgFinPeriod = FinPeriodRepository.FindByID(calendarOrgID, finPeriodID);

                masterFinPeriodID = orgFinPeriod?.MasterFinPeriodID;
            }

            return masterFinPeriodID;
        }

        public static string CalcMasterPeriodID<TField>(PXCache cache, object row)
            where TField : IBqlField
        {
            return cache.GetAttributesReadonly<TField>(row)
                .OfType<FinPeriodIDAttribute>().First()
                .CalcMasterPeriodID(cache, row);
        }

        public virtual void SetPeriodsByMaster(PXCache cache, object row, string masterFinPeriodID)
        {
            cache.SetValue(row,
                _FieldName,
                CalcFinPeriodIDForMaster(cache, row, masterFinPeriodID));
			if(MasterFinPeriodIDType != null)
				cache.SetValue(row, MasterFinPeriodIDType.Name, masterFinPeriodID);
        }

        public static void SetPeriodsByMaster<TField>(PXCache cache, object row, string masterFinPeriodID)
            where TField : IBqlField
        {
            cache.GetAttributesReadonly<TField>(row)
                    .OfType<FinPeriodIDAttribute>().First()
                    .SetPeriodsByMaster(cache, row, masterFinPeriodID);
        }

        public virtual string CalcFinPeriodIDForMaster(PXCache cache, object row, string masterFinPeriodID)
        {
            return FinPeriodRepository
                .GetFinPeriodByMasterPeriodID(
                    CalendarOrganizationIDProvider.GetCalendarOrganizationID(cache.Graph, cache, row),
                    masterFinPeriodID)
                .Result
                ?.FinPeriodID;
        }

        public static string CalcFinPeriodIDForMaster<TField>(PXCache cache, object row, string masterFinPeriodID)
            where TField : IBqlField
        {
            return cache.GetAttributesReadonly<TField>(row)
                    .OfType<FinPeriodIDAttribute>().First()
                    .CalcFinPeriodIDForMaster(cache, row, masterFinPeriodID);
        }

        #endregion
    }
}
