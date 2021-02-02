using System;
using System.Collections;

using PX.Data;
using PX.Objects.CS;
using PX.Objects.GL.FinPeriods;
using PX.Objects.GL.GraphBaseExtensions;
using PX.Objects.GL.DAC;
using PX.Objects.GL.Attributes;
using PX.Objects.Common.Tools;
using PX.Objects.Common.Extensions;
using PX.Objects.GL.FinPeriods.TableDefinition;
using System.Collections.Generic;
using System.Linq;
using PX.Objects.Common;

namespace PX.Objects.GL
{
	public class OrganizationFinPeriodMaint : PXGraph<OrganizationFinPeriodMaint>, IFinPeriodMaintenanceGraph
	{
        #region Types

        [Serializable]
        public class NewOrganizationCalendarParameters : IBqlTable
        {
            #region OrganizationID
            public abstract class organizationID : IBqlField { }

            [PXUIField(DisplayName = "Organization", Enabled = false)]
            [Organization]
            public virtual int? OrganizationID { get; set; }
            #endregion

            #region StartYear
            public abstract class startYear : IBqlField { }

            /// <summary>
            /// The number of the first financial year.
            /// </summary>
            [PXString(4, IsFixed = true)]
            [PXDefault(typeof(Search3<
                MasterFinYear.year,
                OrderBy<
                    Asc<MasterFinYear.year>>>))]
            [PXUIField(DisplayName = "First Financial Year")]
            [PXSelector(
                typeof(Search<MasterFinYear.year>),
                typeof(MasterFinYear.year))]
            public virtual string StartYear { get; set; }
            #endregion

            #region StartMasterFinPeriodID
            public abstract class startMasterFinPeriodID : IBqlField { }

            /// <summary>
            /// The start master period ID of the first year.
            /// </summary>
            [PXString]
            [PXUIField(DisplayName = "Master Period ID", Required = true)]
            [PXDefault]
            [FinPeriodSelector(
                searchType: typeof(Search<
                    FinPeriod.finPeriodID,
                    Where<FinPeriod.finYear, GreaterEqual<Sub<Current<NewOrganizationCalendarParameters.startYear>, int1>>,
                        And<FinPeriod.finYear, LessEqual<Current<NewOrganizationCalendarParameters.startYear>>,
                        And<FinPeriod.startDate, NotEqual<FinPeriod.endDate>>>>>),
                sourceType: null,
                fieldList: new Type[]
                {
                    typeof(FinPeriod.startDate),
                    typeof(FinPeriod.finPeriodID)
                },
                masterPeriodBasedOnOrganizationPeriods: false)]
            public virtual string StartMasterFinPeriodID { get; set; }
            #endregion

            #region StartDate
            public abstract class startDate : IBqlField { }

            /// <summary>
            /// The start date of master period ID of the first year.
            /// </summary>
            [PXDate]
            [PXDefault]
            [PXUIField(DisplayName = "Start Date", Enabled = false, Required = true)]
            public virtual DateTime? StartDate { get; set; }
            #endregion
        }

        [Serializable]
        public class FinYearKey : IBqlTable
        {
            #region OrganizationID
            public abstract class organizationID : IBqlField { }

            [PXInt]
            public virtual int? OrganizationID { get; set; }
            #endregion

            #region Year
            public abstract class year : IBqlField { }

            [PXString]
            public virtual string Year { get; set; }
            #endregion
        }

        public class OrganizationFinPeriodStatusActionsGraphExtension : FinPeriodStatusActionsGraphBaseExtension<OrganizationFinPeriodMaint, OrganizationFinYear> { }

        public class GenerateOrganizationCalendarExtension : GenerateCalendarExtensionBase<OrganizationFinPeriodMaint, OrganizationFinYear>
        {
            public static bool IsActive()
            {
                return PXAccess.FeatureInstalled<FeaturesSet.multipleCalendarsSupport>();
            }
        }

        #endregion

        public PXSelect<
			OrganizationFinYear,
			Where<OrganizationFinYear.organizationID, Equal<Optional<OrganizationFinYear.organizationID>>, 
				And<MatchWithOrganization<OrganizationFinYear.organizationID>>>>
			OrgFinYear;

		public PXSelect<
			OrganizationFinPeriod,
			Where<OrganizationFinPeriod.organizationID, Equal<Optional<OrganizationFinYear.organizationID>>,
				And<OrganizationFinPeriod.finYear, Equal<Optional<OrganizationFinYear.year>>>>,
			OrderBy<
				Asc<OrganizationFinPeriod.periodNbr>>>
			OrgFinPeriods;

		public PXSelectJoin<
			OrganizationFinYear,
			InnerJoin<Organization,
				On<OrganizationFinYear.organizationID, Equal<Organization.organizationID>>>,
			Where<Organization.organizationCD, Equal<Required<Organization.organizationCD>>>,
			OrderBy<
				Desc<OrganizationFinYear.year>>>
			LastOrganizationYear;

		public PXFilter<NewOrganizationCalendarParameters> NewCalendarParams;

		public PXSetup<FinYearSetup> YearSetup;
		public PXFilter<FinYearKey> StoredYearKey;

		public OrganizationFinPeriodMaint()
		{
		    OrgFinYear.Cache.AllowInsert =
		    OrgFinYear.Cache.AllowUpdate = false;

			OrgFinPeriods.Cache.AllowInsert =
			OrgFinPeriods.Cache.AllowUpdate =
			OrgFinPeriods.Cache.AllowDelete = false;

			PXUIFieldAttribute.SetVisible<OrganizationFinPeriod.iNClosed>(OrgFinPeriods.Cache, null, PXAccess.FeatureInstalled<FeaturesSet.inventory>());
			PXUIFieldAttribute.SetVisible<OrganizationFinPeriod.fAClosed>(OrgFinPeriods.Cache, null, PXAccess.FeatureInstalled<FeaturesSet.fixedAsset>());

			FinYearSetup yearSetup = YearSetup.Current;
		}

		public PXAction<OrganizationFinYear> cancel;
		public PXDelete<OrganizationFinYear> Delete;
		public PXFirst<OrganizationFinYear> First;
		public PXPrevious<OrganizationFinYear> Previous;
		public PXNext<OrganizationFinYear> Next;
		public PXLast<OrganizationFinYear> Last;

		[InjectDependency]
		public IFinPeriodRepository FinPeriodRepository { get; set; }

		[InjectDependency]
		public IFinPeriodUtils FinPeriodUtils { get; set; }

		[PXCancelButton]
		[PXUIField(MapEnableRights = PXCacheRights.Select)]
		protected virtual IEnumerable Cancel(PXAdapter a)
		{
			string organizationCD;
			string yearNumber;
			if (NewCalendarParams.View.Answer == WebDialogResult.None) // First outer call
			{
				FinYearKey storedYearKey = new FinYearKey
				{
					OrganizationID = OrgFinYear.Current?.OrganizationID,
					Year = OrgFinYear.Current?.Year
				};

				#region Deliberate Technomagic. Copy-pasted fron base PXCancel instead invocation
				Clear();
				SelectTimeStamp();
				#endregion

				StoredYearKey.Cache.Clear();
				StoredYearKey.Insert(storedYearKey);
				StoredYearKey.Cache.IsDirty = false;

				organizationCD = (string)a.Searches.GetSearchValueByPosition(0);
				if (organizationCD == null)
				{
					yield break;
				}
				yearNumber = (string)a.Searches.GetSearchValueByPosition(1);
			}
			else // Second call after SmartPanel closing
			{
				object extValue = NewCalendarParams.Cache.GetValueExt<NewOrganizationCalendarParameters.organizationID>(NewCalendarParams.Current);
				PXFieldState state = extValue as PXFieldState;
				organizationCD = state != null ? (string)state.Value : (string)extValue;
				yearNumber = NewCalendarParams.Current.StartYear;
			}

			OrganizationFinYear targetYear = PXSelectJoin<
				OrganizationFinYear,
				InnerJoin<Organization,
					On<OrganizationFinYear.organizationID, Equal<Organization.organizationID>>>,
				Where<Organization.organizationCD, Equal<Required<Organization.organizationCD>>,
					And<OrganizationFinYear.year, Equal<Required<OrganizationFinYear.year>>>>>
				.SelectSingleBound(this, new object[] { }, organizationCD, yearNumber);

			OrganizationFinYear returnYear = null;
			if (targetYear == null)
			{
				OrganizationFinYear lastYear = LastOrganizationYear.SelectSingle(organizationCD);

				if(lastYear == null)
				{
					if(NewCalendarParams.AskExtFullyValid((graph, viewName) =>
					{
						NewCalendarParams.Current.OrganizationID = PXAccess.GetOrganizationID(organizationCD);
					},
						DialogAnswerType.Positive))
					{
						OrganizationFinYear generatedFinYear;
						using (PXTransactionScope ts = new PXTransactionScope())
						{
							generatedFinYear = GenerateSingleOrganizationFinYear(
								(int)NewCalendarParams.Current.OrganizationID,
								NewCalendarParams.Current.StartYear,
								NewCalendarParams.Current.StartMasterFinPeriodID);
							Actions.PressSave();
							ts.Complete();
						}
						returnYear = generatedFinYear;
					}
					else
					{
						OrganizationFinYear storedYear = FinPeriodRepository.FindOrganizationFinYearByID(StoredYearKey.Current?.OrganizationID, StoredYearKey.Current?.Year);
						returnYear = storedYear;
					}
				}
				else
				{
					returnYear = lastYear;
				}
			}
			else
			{
				returnYear = targetYear;
			}

			if (returnYear != null)
			{
				yield return returnYear;
			}
			yield break;
		}

	    protected virtual void NewOrganizationCalendarParameters_StartMasterFinPeriodID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
	    {
	        NewOrganizationCalendarParameters row = (NewOrganizationCalendarParameters) e.Row;

	        row.StartDate = FinPeriodRepository
	            .FindByID(FinPeriod.organizationID.MasterValue, row.StartMasterFinPeriodID)
	            ?.StartDate;

	    }

        protected virtual void _(Events.RowUpdated<OrganizationFinYear> e)
		{
			e.Cache.IsDirty = false;
		}

	    protected virtual void _(Events.RowDeleting<OrganizationFinYear> e)
	    {
			VerifyOrganizationFinYearForDelete(e.Row);
		}

		protected virtual void VerifyOrganizationFinYearForDelete(OrganizationFinYear organizationFinYear)
		{
			if (!IsFirstOrLastOrganizationFinYear(organizationFinYear))
			{
				throw new PXException(Messages.OnlyFirstOrLastYearCanBeDeleted);
			}

			if (IsOrganizationFinYearUsed(organizationFinYear))
			{
				throw new PXException(Messages.PeriodAlreadyUsed);
			}
		}

		private bool IsFirstOrLastOrganizationFinYear(OrganizationFinYear organizationFinYear)
		{
			FinYear firstFinYear = FinPeriodRepository.FindFirstYear(organizationFinYear.OrganizationID, clearQueryCache: true);

			FinYear lastFinYear = FinPeriodRepository.FindLastYear(organizationFinYear.OrganizationID, clearQueryCache: true);

			return organizationFinYear.Year == firstFinYear.Year || 
				organizationFinYear.Year == lastFinYear.Year;
		}

		protected bool IsOrganizationFinYearUsed(OrganizationFinYear organizationFinYear)
		{
			return OrgFinPeriods
				.Select(organizationFinYear.OrganizationID, organizationFinYear.Year)
				.RowCast<OrganizationFinPeriod>()
				.Any(period => period.Status == FinPeriod.status.Closed
					|| period.Status == FinPeriod.status.Locked
					|| period.DateLocked == true)
				|| IsOrganizationFinYearReferenced(organizationFinYear);
		}

		private BqlCommand referencingByTranQuery = null;
		protected BqlCommand ReferencingByTranQuery => referencingByTranQuery =
			referencingByTranQuery ??
			PXSelectJoin<
				GLTran,
				InnerJoin<Branch,
					On<GLTran.branchID, Equal<Branch.branchID>>,
				InnerJoin<OrganizationFinPeriod,
					On<Branch.organizationID, Equal<OrganizationFinPeriod.organizationID>,
					And<GLTran.finPeriodID, Equal<OrganizationFinPeriod.finPeriodID>>>>>>
			.GetCommand();

		private BqlCommand referencingByBatchQuery = null;
		protected BqlCommand ReferencingByBatchQuery => referencingByBatchQuery =
			referencingByBatchQuery ??
			PXSelectJoin<
				Batch,
				InnerJoin<Branch,
					On<Batch.branchID, Equal<Branch.branchID>>,
				InnerJoin<OrganizationFinPeriod,
					On<Branch.organizationID, Equal<OrganizationFinPeriod.organizationID>,
					And<Batch.finPeriodID, Equal<OrganizationFinPeriod.finPeriodID>>>>>>
			.GetCommand();

		private bool IsOrganizationFinYearReferenced(OrganizationFinYear organizationFinYear)
		{
			using (new PXConnectionScope())
			{
				GLTran tran = ReferencingByTranQuery
				.WhereNew<Where<OrganizationFinPeriod.organizationID, Equal<Required<OrganizationFinYear.organizationID>>,
					And<OrganizationFinPeriod.finYear, Equal<Required<OrganizationFinYear.year>>>>>()
				.SelectSingleReadonly<GLTran>(this, organizationFinYear.OrganizationID, organizationFinYear.Year);

				Batch batch = ReferencingByBatchQuery
				.WhereNew<Where<OrganizationFinPeriod.organizationID, Equal<Required<OrganizationFinYear.organizationID>>,
					And<OrganizationFinPeriod.finYear, Equal<Required<OrganizationFinYear.year>>>>>()
				.SelectSingleReadonly<Batch>(this, organizationFinYear.OrganizationID, organizationFinYear.Year);

				return batch != null || tran != null;
			}
		}

		protected virtual OrganizationFinYear GenerateSingleOrganizationFinYear(int organizationID, string startYearNumber, string startMasterFinPeriodID)
		{
			MasterFinYear startYear = FinPeriodRepository.FindMasterFinYearByID(startYearNumber);
			MasterFinPeriod startMasterFinPeriod = FinPeriodRepository.FindMasterFinPeriodByID(startMasterFinPeriodID);

			return GenerateSingleOrganizationFinYear(organizationID, startYear, startMasterFinPeriod);
		}

		/// <summary>
		/// TODO: Share function in <see cref="OrganizationMaint.CreateOrganizationCalendar"/> function
		/// </summary>
		/// <param name="organizationID"></param>
		/// <param name="masterFinPeriod"></param>
		/// <returns></returns>
		protected virtual OrganizationFinPeriod CopyOrganizationFinPeriodFromMaster(
		    int organizationID,
		    MasterFinPeriod masterFinPeriod,
		    FinPeriod orgFinPeriodStatusSource,
		    string yearNumber = null, 
		    string periodNumber = null)
		{
			bool isCentralizedManagement = PXAccess.FeatureInstalled<FeaturesSet.centralizedPeriodsManagement>();
			string organizationFinPeriodID = FinPeriodUtils.ComposeFinPeriodID(yearNumber, periodNumber) ?? masterFinPeriod.FinPeriodID;

            OrganizationFinPeriod orgFinPeriod = new OrganizationFinPeriod
			{
				OrganizationID = organizationID,
				FinPeriodID = organizationFinPeriodID,
				MasterFinPeriodID = masterFinPeriod.FinPeriodID,
				FinYear = yearNumber ?? masterFinPeriod.FinYear,
				PeriodNbr = periodNumber ?? masterFinPeriod.PeriodNbr,
				Custom = masterFinPeriod.Custom,
				DateLocked = masterFinPeriod.DateLocked,
				StartDate = masterFinPeriod.StartDate,
				EndDate = masterFinPeriod.EndDate,

				Status = isCentralizedManagement ? masterFinPeriod.Status : orgFinPeriodStatusSource != null ? orgFinPeriodStatusSource.Status : FinPeriod.status.Inactive,
				ARClosed = isCentralizedManagement ? masterFinPeriod.ARClosed : orgFinPeriodStatusSource != null ? orgFinPeriodStatusSource.ARClosed : false,
				APClosed = isCentralizedManagement ? masterFinPeriod.APClosed : orgFinPeriodStatusSource != null ? orgFinPeriodStatusSource.APClosed : false,
				FAClosed = isCentralizedManagement ? masterFinPeriod.FAClosed : orgFinPeriodStatusSource != null ? orgFinPeriodStatusSource.FAClosed : false,
				CAClosed = isCentralizedManagement ? masterFinPeriod.CAClosed : orgFinPeriodStatusSource != null ? orgFinPeriodStatusSource.CAClosed : false,
				INClosed = isCentralizedManagement ? masterFinPeriod.INClosed : orgFinPeriodStatusSource != null ? orgFinPeriodStatusSource.INClosed : false,

				Descr = masterFinPeriod.Descr,
			};   

            PXDBLocalizableStringAttribute.CopyTranslations<MasterFinPeriod.descr, OrganizationFinPeriod.descr>(this, masterFinPeriod, orgFinPeriod);

            return orgFinPeriod;
        }

		protected virtual OrganizationFinPeriod GenerateAdjustmentOrganizationFinPeriod(int organizationID, OrganizationFinPeriod prevFinPeriod)
		{
			(string masterYearNumber, string masterPeriodNumber) = FinPeriodUtils.ParseFinPeriodID(prevFinPeriod.FinPeriodID);
			MasterFinYear masterFinYear = FinPeriodRepository.FindMasterFinYearByID(masterYearNumber, clearQueryCache: true);
			string adjustmentMasterFinPeriodID = $"{masterYearNumber:0000}{masterFinYear.FinPeriods:00}";

			(string yearNumber, string periodNumber) = FinPeriodUtils.ParseFinPeriodID(prevFinPeriod.FinPeriodID);
			periodNumber = $"{int.Parse(periodNumber) + 1:00}";
			OrganizationFinPeriod orgFinPeriod = new OrganizationFinPeriod
			{
				OrganizationID = organizationID,
				FinPeriodID = FinPeriodUtils.ComposeFinPeriodID(yearNumber, periodNumber),
				MasterFinPeriodID = adjustmentMasterFinPeriodID,
				FinYear = yearNumber,
				PeriodNbr = periodNumber,
				Custom = prevFinPeriod.Custom,
				DateLocked = prevFinPeriod.DateLocked,
				StartDate = prevFinPeriod.EndDate,
				EndDate = prevFinPeriod.EndDate,

				Status = prevFinPeriod.Status,
				ARClosed = prevFinPeriod.ARClosed,
				APClosed = prevFinPeriod.APClosed,
				FAClosed = prevFinPeriod.FAClosed,
				CAClosed = prevFinPeriod.CAClosed,
				INClosed = prevFinPeriod.INClosed,

				Descr = Messages.AdjustmentPeriod,
			};

			MasterFinPeriod masterFinPeriod = FinPeriodRepository.FindMasterFinPeriodByID(adjustmentMasterFinPeriodID);

			PXDBLocalizableStringAttribute.CopyTranslations<MasterFinPeriod.descr, OrganizationFinPeriod.descr>(this, masterFinPeriod, orgFinPeriod);

			return orgFinPeriod;
		}

		MasterFinPeriodMaint masterCalendarGraph = null;
		MasterFinPeriodMaint MasterCalendarGraph => masterCalendarGraph ?? (masterCalendarGraph = CreateInstance<MasterFinPeriodMaint>());

		protected virtual OrganizationFinYear GenerateSingleOrganizationFinYear(int organizationID, MasterFinYear startMasterYear, MasterFinPeriod startMasterFinPeriod)
		{
			if(startMasterYear == null)
			{
				throw new ArgumentNullException(nameof(startMasterYear));
			}
			if (startMasterFinPeriod == null)
			{
				throw new ArgumentNullException(nameof(startMasterFinPeriod));
			}

			OrganizationFinYear newOrganizationFinYear = (OrganizationFinYear)this.Caches<OrganizationFinYear>().Insert(
				new OrganizationFinYear
				{
					OrganizationID = organizationID,
					Year = startMasterYear.Year,
					FinPeriods = startMasterYear.FinPeriods,
					StartMasterFinPeriodID = startMasterFinPeriod.FinPeriodID,
					StartDate = startMasterFinPeriod.StartDate,
				});

			short periodNumber = 1;
			MasterFinPeriod sourceMasterFinPeriod = startMasterFinPeriod;
			int periodsCountForCopy = (int)newOrganizationFinYear.FinPeriods;
			if(YearSetup.Current.HasAdjustmentPeriod == true)
			{
				periodsCountForCopy--;
			}
			OrganizationFinPeriod newOrganizationFinPeriod = null;

		    FinPeriod firstOrgFinPeriod = FinPeriodRepository.FindFirstPeriod(organizationID, clearQueryCache: true);

			while (periodNumber <= periodsCountForCopy)
			{
				newOrganizationFinPeriod = (OrganizationFinPeriod)this.Caches<OrganizationFinPeriod>().Insert(
					CopyOrganizationFinPeriodFromMaster(
					    organizationID, 
					    sourceMasterFinPeriod,
					    firstOrgFinPeriod != null && Convert.ToInt32(firstOrgFinPeriod.FinYear) > Convert.ToInt32(newOrganizationFinYear.Year) ? firstOrgFinPeriod : null,
                        newOrganizationFinYear.Year, 
					    $"{periodNumber:00}"));

				if (periodNumber < periodsCountForCopy) // no need to search for the next master period if last organization period is generated
				{
					string sourceMasterFinPeriodID = sourceMasterFinPeriod.FinPeriodID;
					while ((sourceMasterFinPeriod = FinPeriodRepository.FindNextNonAdjustmentMasterFinPeriod(sourceMasterFinPeriodID, clearQueryCache: true)) == null)
					{
						MasterCalendarGraph.Clear();
						MasterCalendarGraph.GenerateNextMasterFinYear();
					}
				}

				periodNumber++;
			}
			newOrganizationFinYear.EndDate = newOrganizationFinPeriod.EndDate;
			if (YearSetup.Current.HasAdjustmentPeriod == true)
			{
				this.Caches<OrganizationFinPeriod>().Insert(GenerateAdjustmentOrganizationFinPeriod(organizationID, newOrganizationFinPeriod));
			}
			
			return newOrganizationFinYear;
		}

		protected virtual OrganizationFinYear GenerateNextOrganizationFinYear(OrganizationFinYear year)
		{
			string generatedYearNumber = $"{int.Parse(year.Year) + 1:0000}";
			MasterFinYear masterFinYear;
			while ((masterFinYear = FinPeriodRepository.FindMasterFinYearByID(generatedYearNumber, clearQueryCache: true)) == null)
			{
				MasterCalendarGraph.Clear();
				MasterCalendarGraph.GenerateCalendar(
					FinPeriod.organizationID.MasterValue, 
					int.Parse(FinPeriodRepository.FindLastYear(FinPeriod.organizationID.MasterValue, clearQueryCache: true).Year), 
					int.Parse(generatedYearNumber));
			}

			short generatedFinPeriodsCount = (short)masterFinYear.FinPeriods;
			if (YearSetup.Current.HasAdjustmentPeriod == true)
			{
				generatedFinPeriodsCount--;
			}

			OrganizationFinPeriod lastNonAdjustmentOrgFinPeriod = this.FinPeriodRepository.FindLastNonAdjustmentOrganizationFinPeriodOfYear(year.OrganizationID, year.Year, clearQueryCache: true);
			int generatedMasterYearNumber = int.Parse(lastNonAdjustmentOrgFinPeriod.FinYear);

			List<MasterFinPeriod> masterFinPeriods;

			PXSelectBase<MasterFinPeriod> select = new PXSelectReadonly<
				MasterFinPeriod,
				Where<MasterFinPeriod.finPeriodID, Greater<Required<MasterFinPeriod.finPeriodID>>,
					And<MasterFinPeriod.startDate, NotEqual<MasterFinPeriod.endDate>>>,
				OrderBy<
					Asc<MasterFinPeriod.finPeriodID>>>(this);
			select.View.Clear();

			while ((masterFinPeriods = select
				.SelectWindowed(0, generatedFinPeriodsCount, lastNonAdjustmentOrgFinPeriod.MasterFinPeriodID)
				.RowCast<MasterFinPeriod>()
				.ToList()).Count < generatedFinPeriodsCount)
			{
				generatedMasterYearNumber++;
				MasterCalendarGraph.Clear();
				MasterCalendarGraph.GenerateCalendar(
					FinPeriod.organizationID.MasterValue, 
					int.Parse(FinPeriodRepository.FindLastYear(FinPeriod.organizationID.MasterValue, clearQueryCache: true).Year), 
					generatedMasterYearNumber);
				select.View.Clear();
			}

			MasterFinPeriod startMasterFinPeriod = masterFinPeriods.First();
			return GenerateSingleOrganizationFinYear((int)year.OrganizationID, masterFinYear, startMasterFinPeriod);
		}

		protected virtual OrganizationFinYear GeneratePreviousOrganizationFinYear(OrganizationFinYear year)
		{
			string generatedYearNumber = $"{int.Parse(year.Year)-1:0000}";
			MasterFinYear masterFinYear;
			while((masterFinYear = FinPeriodRepository.FindMasterFinYearByID(generatedYearNumber, clearQueryCache: true)) == null)
			{
				MasterCalendarGraph.Clear();
				MasterCalendarGraph.GenerateCalendar(
					FinPeriod.organizationID.MasterValue, 
					int.Parse(generatedYearNumber), 
					int.Parse(FinPeriodRepository.FindFirstYear(FinPeriod.organizationID.MasterValue, clearQueryCache: true).Year));
			}

			short generatedFinPeriodsCount = (short)masterFinYear.FinPeriods;
			if(YearSetup.Current.HasAdjustmentPeriod == true)
			{
				generatedFinPeriodsCount--;
			}

			int generatedMasterYearNumber = int.Parse(FinPeriods.FinPeriodUtils.FiscalYear(year.StartMasterFinPeriodID));

			List<MasterFinPeriod> masterFinPeriods;

			PXSelectBase<MasterFinPeriod> select = new PXSelectReadonly<MasterFinPeriod,
				Where<MasterFinPeriod.finPeriodID, Less<Required<MasterFinPeriod.finPeriodID>>,
					And<MasterFinPeriod.startDate, NotEqual<MasterFinPeriod.endDate>>>,
				OrderBy<
					Desc<MasterFinPeriod.finPeriodID>>>(this);
			select.View.Clear();

			while ((masterFinPeriods = select
				.SelectWindowed(0, generatedFinPeriodsCount, year.StartMasterFinPeriodID)
				.RowCast<MasterFinPeriod>()
				.ToList()).Count < generatedFinPeriodsCount)
			{
				generatedMasterYearNumber--;
				MasterCalendarGraph.Clear();
				MasterCalendarGraph.GenerateCalendar(
					FinPeriod.organizationID.MasterValue, 
					generatedMasterYearNumber, 
					int.Parse(FinPeriodRepository.FindFirstYear(FinPeriod.organizationID.MasterValue, clearQueryCache: true).Year));
				select.View.Clear();
			}

			MasterFinPeriod startMasterFinPeriod = masterFinPeriods.Last();
			return GenerateSingleOrganizationFinYear((int)year.OrganizationID, masterFinYear, startMasterFinPeriod);
		}

		public virtual void GenerateCalendar(int? organizationID, int fromYear, int toYear)
		{
			(int firstYearNumber, int lastYearNumber) = FinPeriodUtils.GetFirstLastYearForGeneration(organizationID, fromYear, toYear,
				clearQueryCache: true);

			OrganizationFinYear firstFinYear = FinPeriodRepository.FindOrganizationFinYearByID(organizationID, $"{firstYearNumber:0000}",
				clearQueryCache: true);
			OrganizationFinYear lastFinYear = FinPeriodRepository.FindOrganizationFinYearByID(organizationID, $"{lastYearNumber:0000}",
				clearQueryCache: true);

			OrganizationFinYear baseFinYear = firstFinYear;

			using (PXTransactionScope ts = new PXTransactionScope())
			{
				if (fromYear < firstYearNumber)
				{
					do
					{
						baseFinYear = GeneratePreviousOrganizationFinYear(baseFinYear);
						Actions.PressSave();
					}
					while (baseFinYear != null &&
						string.CompareOrdinal(baseFinYear.Year, $"{fromYear:0000}") > 0);
				}
				baseFinYear = lastFinYear;
				if (toYear > lastYearNumber)
				{
					do
					{
						baseFinYear = GenerateNextOrganizationFinYear(baseFinYear);
						Actions.PressSave();
					}
					while (baseFinYear != null &&
						string.CompareOrdinal(baseFinYear.Year, $"{toYear:0000}") < 0);
				}
				ts.Complete();
			}
		}
	}
}
