using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PX.Common;
using PX.Data;
using PX.Objects.Common.Extensions;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.IN;
using PX.Objects.CT;
using PX.Objects.PM;
using PX.Objects.GL;
using System.Diagnostics;
using System.Globalization;
using PX.SM;
using PX.TM;
using PX.Web.UI;
using Branch = PX.Objects.GL.Branch;
using System.Text;
using PX.Objects.GL.FinPeriods.TableDefinition;
using PX.Objects.GL.FinPeriods;

namespace PX.Objects.EP
{
	[Serializable]
	public class TimeCardMaint : PXGraph<TimeCardMaint, EPTimeCard>, PXImportAttribute.IPXPrepareItems
	{
		#region DAC Overrides

		[PXDBDate()]
		[PXDefault(typeof(EPTimeCard.createdDateTime), PersistingCheck = PXPersistingCheck.Nothing)]
		protected virtual void EPApproval_DocDate_CacheAttached(PXCache sender)
		{
		}

		[PXDBInt()]
		[PXDefault(typeof(EPTimeCard.employeeID), PersistingCheck = PXPersistingCheck.Nothing)]
		protected virtual void EPApproval_BAccountID_CacheAttached(PXCache sender)
		{
		}

		[PXDBGuid()]
		[PXDefault(typeof(Search<CREmployee.userID,
				Where<CREmployee.userID, Equal<Current<AccessInfo.userID>>,
					And<Current<EPTimeCard.workgroupID>, IsNull>>>),
				PersistingCheck = PXPersistingCheck.Nothing)]
		protected virtual void EPApproval_DocumentOwnerID_CacheAttached(PXCache sender)
		{
		}

		[PXDBLong()]
		protected virtual void EPApproval_CuryInfoID_CacheAttached(PXCache sender)
		{
		}

		#endregion

		#region Selects

		//This view is required for 2 purposes - 1 so that correct cache is initialized ApproverEmployee -> EPEmployee
		//										 2 Employee object with all its properties can be used in the Assignment Rules.

		[PXViewName(Messages.Employee)]
		public PXSetup<EPEmployee, Where<EPEmployee.bAccountID, Equal<Current<EPTimeCard.employeeID>>>> Employee;

		[PXHidden]
		public PXSetup<EPSetup> EpSetup;

		[PXHidden]
		public PXSelect<EPEarningType> EarningTypes;

		[PXHidden]
		public PXSelect<BAccount> AccountBase;

		[PXViewName(Messages.TimeCardDocument)]
		public PXSelectJoin<EPTimeCard,
						InnerJoin<EPEmployee, On<EPEmployee.bAccountID, Equal<EPTimeCard.employeeID>>>,
						Where<EPTimeCard.createdByID, Equal<Current<AccessInfo.userID>>,
										Or<EPEmployee.userID, Equal<Current<AccessInfo.userID>>,
										Or<EPEmployee.userID, OwnedUser<Current<AccessInfo.userID>>,
										Or<EPTimeCard.noteID, Approver<Current<AccessInfo.userID>>,
										Or<EPTimeCard.employeeID, WingmanUser<Current<AccessInfo.userID>>>>>>>> Document;

		[PXHidden]
		public PXFilter<EP.CRActivityMaint.EPTempData> TempData;

		[PXImport(typeof(EPTimeCard))]
		[PXViewName(Messages.TimeCardSummary)]
		public PXSelect<EPTimeCardSummaryWithInfo, Where<EPTimeCardSummaryWithInfo.timeCardCD, Equal<Current<EPTimeCard.timeCardCD>>>, OrderBy<Asc<EPTimeCardSummaryWithInfo.lineNbr>>> Summary;

		protected Dictionary<string, SummaryRecord> sumList = new Dictionary<string, SummaryRecord>();
		protected Dictionary<string, EPTimeCardSummaryWithInfo> existingSummaryRowsByKeys = new Dictionary<string, EPTimeCardSummaryWithInfo>();
		protected Dictionary<int, EPTimeCardSummaryWithInfo> existingSummaryRowsByLineNbr = new Dictionary<int, EPTimeCardSummaryWithInfo>();
				
		public virtual IEnumerable summary()
		{
			EPTimeCard document = Document.Current;

			if (PXView.Currents != null)
				foreach (object current in PXView.Currents)
				{
					var currentDocument = current as EPTimeCard;
					if (currentDocument != null)
						document = currentDocument;
				}

			if (document == null)
				return new List<EPTimeCardSummaryWithInfo>();

			if (document.WeekID == null)
				return new List<EPTimeCardSummaryWithInfo>();

			return SelectSummaryRecords(document, true);
		}

		public virtual IList<EPTimeCardSummaryWithInfo> SelectSummaryRecords(EPTimeCard document, bool autocorrect)
		{
			if (document == null)
				throw new ArgumentNullException(nameof(document));

			/*
             * List all summary records that are in tthe database.
             * Add new ones based on the new activities added.
             * Remove the ones that are empty - no activities associated.
             * Update existing ones - new activities must be aggregated              
             */

			SelectExistingSummaryRecords(document.TimeCardCD);
			
			ProcessActivities(autocorrect);
			
			return BuildSummaryList(autocorrect);
		}

		private void SelectExistingSummaryRecords(string timecardCD)
		{
			existingSummaryRowsByKeys.Clear();
			existingSummaryRowsByLineNbr.Clear();
			PXSelectBase<EPTimeCardSummaryWithInfo> select = new PXSelect<EPTimeCardSummaryWithInfo,
				Where<EPTimeCardSummaryWithInfo.timeCardCD, Equal<Required<EPTimeCard.timeCardCD>>>, OrderBy<Asc<EPTimeCardSummaryWithInfo.lineNbr>>>(this);
			foreach (EPTimeCardSummaryWithInfo item in select.Select(timecardCD))
			{
				string key = GetSummaryKey(item);
				if (!existingSummaryRowsByKeys.ContainsKey(key))
					existingSummaryRowsByKeys.Add(key, item);
				existingSummaryRowsByLineNbr.Add(item.LineNbr.Value, item);
			}

		}

		private void ProcessActivities(bool autocorrect)
		{
			sumList.Clear();

			foreach (PXResult res in activities())
			{
				EPTimecardDetail activity = (EPTimecardDetail) res[typeof(EPTimecardDetail)];

				if (string.IsNullOrEmpty(activity.EarningTypeID))
					continue;
				string key = GetSummaryKey(activity);

				SummaryRecord record = null;
				EPTimeCardSummaryWithInfo summary = null;

				if (existingSummaryRowsByKeys.TryGetValue(key, out summary))
				{
					EPTimeCardSummaryWithInfo summaryByLineNbr = null;
					if (activity.SummaryLineNbr != null)
					{
						if (existingSummaryRowsByLineNbr.TryGetValue(activity.SummaryLineNbr.Value, out summaryByLineNbr))
						{
							if (GetSummaryKey(summary) == GetSummaryKey(summaryByLineNbr))
							{
								summary = summaryByLineNbr;
							}
							else if (autocorrect)
							{
								Activities.Cache.SetValue<EPTimecardDetail.summaryLineNbr>(activity, null);
								Activities.Cache.MarkUpdated(activity);
							}
						}
						else if (autocorrect)
						{
							Activities.Cache.SetValue<EPTimecardDetail.summaryLineNbr>(activity, null);
							Activities.Cache.MarkUpdated(activity);
						}
					}
				}



				if (summary != null)
				{
					if (sumList.ContainsKey(summary.LineNbr.ToString()))
					{
						record = sumList[summary.LineNbr.ToString()];
					}
				}
				else
				{
					if (sumList.ContainsKey(key))
					{
						record = sumList[key];
					}
				}

				if (activity.SummaryLineNbr != null)
				{
					if (summary != null)
					{
						if (record == null)
						{
							record = new SummaryRecord(summary);
							sumList.Add(record.Summary.LineNbr.ToString(), record);
						}
						record.LinkedDetails.Add(activity);
					}
					else
					{
						if (autocorrect)
						{
							Activities.Cache.SetValue<EPTimecardDetail.summaryLineNbr>(activity, null);
							Activities.Cache.MarkUpdated(activity);
						}

						if (record == null)
						{
							record = new SummaryRecord(null);
							sumList.Add(key, record);
						}
						record.SummaryKey = key;
						record.NotLinkedDetails.Add(activity);
					}
				}
				else
				{
					if (summary != null)
					{
						if (record == null)
						{
							record = new SummaryRecord(summary);
							sumList.Add(record.Summary.LineNbr.ToString(), record);
						}
					}
					else
					{
						if (autocorrect)
						{
							Activities.Cache.SetValue<EPTimecardDetail.summaryLineNbr>(activity, null);
							Activities.Cache.MarkUpdated(activity);
						}

						if (record == null)
						{
							record = new SummaryRecord(null);
							sumList.Add(key, record);
						}
						record.SummaryKey = key;
					}
					record.NotLinkedDetails.Add(activity);
				}
			}
		}

		private List<EPTimeCardSummaryWithInfo> BuildSummaryList(bool autocorrect)
		{
			List<EPTimeCardSummaryWithInfo> list = new List<EPTimeCardSummaryWithInfo>();

			foreach (EPTimeCardSummaryWithInfo item in existingSummaryRowsByLineNbr.Values)
			{
				if (sumList.ContainsKey(item.LineNbr.ToString()))
				{
					EPTimeCardSummaryWithInfo summary = sumList[item.LineNbr.ToString()].Summary;
					if (autocorrect)
					{
						bool hasChanges = false;

						EPTimeCardSummary keyItem = sumList[item.LineNbr.ToString()].SummariseDetails();
						if (summary.Mon.GetValueOrDefault() != keyItem.Mon.GetValueOrDefault())
						{
							hasChanges = true;
							summary.Mon = keyItem.Mon;
						}
						if (summary.Tue.GetValueOrDefault() != keyItem.Tue.GetValueOrDefault())
						{
							hasChanges = true;
							summary.Tue = keyItem.Tue;
						}
						if (summary.Wed.GetValueOrDefault() != keyItem.Wed.GetValueOrDefault())
						{
							hasChanges = true;
							summary.Wed = keyItem.Wed;
						}
						if (summary.Thu.GetValueOrDefault() != keyItem.Thu.GetValueOrDefault())
						{
							hasChanges = true;
							summary.Thu = keyItem.Thu;
						}
						if (summary.Fri.GetValueOrDefault() != keyItem.Fri.GetValueOrDefault())
						{
							hasChanges = true;
							summary.Fri = keyItem.Fri;
						}
						if (summary.Sat.GetValueOrDefault() != keyItem.Sat.GetValueOrDefault())
						{
							hasChanges = true;
							summary.Sat = keyItem.Sat;
						}
						if (summary.Sun.GetValueOrDefault() != keyItem.Sun.GetValueOrDefault())
						{
							hasChanges = true;
							summary.Sun = keyItem.Sun;
						}

						if (hasChanges && autocorrect)
							Summary.Cache.MarkUpdated(summary);
					}
					list.Add(summary);
				}
				else
				{
					if (Summary.Cache.GetStatus(item) == PXEntryStatus.Notchanged && autocorrect)
					{
						bool hasChanges = false;
						if (item.Mon.GetValueOrDefault() != 0)
						{
							item.Mon = 0;
							hasChanges = true;
						}
						if (item.Tue.GetValueOrDefault() != 0)
						{
							item.Tue = 0;
							hasChanges = true;
						}
						if (item.Wed.GetValueOrDefault() != 0)
						{
							item.Wed = 0;
							hasChanges = true;
						}
						if (item.Thu.GetValueOrDefault() != 0)
						{
							item.Thu = 0;
							hasChanges = true;
						}
						if (item.Fri.GetValueOrDefault() != 0)
						{
							item.Fri = 0;
							hasChanges = true;
						}
						if (item.Sat.GetValueOrDefault() != 0)
						{
							item.Sat = 0;
							hasChanges = true;
						}
						if (item.Sun.GetValueOrDefault() != 0)
						{
							item.Sun = 0;
							hasChanges = true;
						}
						if (hasChanges)
						{
							Summary.Cache.MarkUpdated(item);
						}
					}
					list.Add(item);
				}
			}

			foreach (SummaryRecord record in sumList.Values)
			{
				if (record.SummaryKey != null && autocorrect)
				{
					List<EPTimecardDetail> details = record.NotLinkedDetails;
					EPTimeCardSummaryWithInfo summary = null;
					//sw3.Restart();
					foreach (EPTimecardDetail detail in details)
					{
						if (detail.TimeSpent.GetValueOrDefault() != 0)
						{
							try
							{
								skipValidation = true;
								summary = AddToSummary(summary, detail);

							}
							catch (PXFieldProcessingException ex)
							{
								summary = null;
								if (ex.FieldName == "ParentNoteID")
								{
									Activities.Cache.RaiseExceptionHandling<EPTimecardDetail.parentTaskNoteID>
										(detail,
										detail.Summary, new PXSetPropertyException(Messages.SummaryTaskNotFound,
											PXErrorLevel.RowError));
								}
								else
								{
									Activities.Cache.RaiseExceptionHandling<EPTimecardDetail.summary>(detail, detail.Summary, new PXSetPropertyException(Messages.SummarySyncFailed
									+ Environment.NewLine + ex.MessageNoPrefix, PXErrorLevel.RowError));
								}
							}
							catch (PXSetPropertyException ex)
							{
								summary = null;
								Activities.Cache.RaiseExceptionHandling<EPTimecardDetail.summary>(detail, detail.Summary, new PXSetPropertyException(Messages.SummarySyncFailed
									+ Environment.NewLine + ex.MessageNoPrefix, PXErrorLevel.RowError));
							}
							finally
							{
								skipValidation = false;
							}
						}
					}
					//sw3.Stop();
					//Debug.Print("AddToSummary for {0} details in {1} ms.", details.Count, sw3.ElapsedMilliseconds);

					if (summary != null)
						list.Add(summary);
				}
			}

			return list;
		}

		[PXViewName(Messages.TimeCardDetail)]
		public PXSelect<EPTimecardDetail, Where<EPTimecardDetail.trackTime, Equal<True>>, OrderBy<Asc<EPTimecardDetail.date>>> Activities;

		[PXHidden]
		public PXSelect<EPTimecardDetail,
			Where<EPTimecardDetail.ownerID, Equal<Current<EPEmployee.userID>>,
				And<EPTimecardDetail.weekID, Equal<Current<EPTimeCard.weekId>>,
				And<EPTimecardDetail.trackTime, Equal<True>,
				And<EPTimecardDetail.approvalStatus, NotEqual<ActivityStatusListAttribute.canceled>,
				And<Where<EPTimecardDetail.timeCardCD, IsNull, Or<EPTimecardDetail.timeCardCD, Equal<Current<EPTimeCard.timeCardCD>>>>>>>>>,
			OrderBy<Asc<EPTimecardDetail.date>>> AllActivities;

		[PXHidden]
		public PXSelect<EPTimecardDetail,
			Where<EPTimecardDetail.timeCardCD, Equal<Current<EPTimeCard.timeCardCD>>>,
			OrderBy<Asc<EPTimecardDetail.date>>> TimecardActivities;

		public virtual IEnumerable activities()
		{
			EPTimeCard document = Document.Current;

			if (PXView.Currents != null)
				foreach (object current in PXView.Currents)
				{
					var currentDocument = current as EPTimeCard;
					if (currentDocument != null)
						document = currentDocument;
				}

			if (document == null)
				return new EPTimecardDetail[] { };

			if (document.WeekID == null)
				return new EPTimecardDetail[] { };

			if (!CanSelectAllDetailsByTimeCardCD(document))
			{
				return AllActivities.Select();
			}
			else
			{
				return TimecardActivities.Select();
			}
		}

		[PXCopyPasteHiddenFields(typeof(EPTimeCardItem.noteID), typeof(Note.noteText))]
		public PXSelect<EPTimeCardItem,
			Where<EPTimeCardItem.timeCardCD, Equal<Current<EPTimeCard.timeCardCD>>>,
			OrderBy<Asc<EPTimeCardItem.lineNbr>>> Items;

		public PXSelectJoin<EPTimecardTask,
			InnerJoin<CREmployee, On<CREmployee.userID, Equal<EPTimecardTask.ownerID>>>,
			Where<CREmployee.bAccountID, Equal<Current<EPTimeCard.employeeID>>, And<EPTimecardTask.classID, Equal<CRActivityClass.task>>>> Tasks;

		[PXViewName(Messages.Approval)]
		public EPApprovalAction<EPTimeCard, EPTimeCard.isApproved, EPTimeCard.isRejected> Approval;
		#endregion

		#region Flags
		/// <summary>
		/// When True activities billable field is not updated when created TC correction.
		/// </summary>
		protected bool isCreateCorrectionFlag = false;
		/// <summary>
		/// When True Summary records are not updated as a result of a detail row update.
		/// </summary>
		protected bool dontSyncSummary = false;
		/// <summary>
		/// When True detail row is not updated when a summary record is modified.
		/// </summary>
		protected bool dontSyncDetails = false;

		#endregion
		
		[InjectDependency]
		public IFinPeriodRepository FinPeriodRepository { get; set; }

		public TimeCardMaint()
		{
			if (EpSetup.Current.TimeCardNumberingID == null)
				throw new PXSetupNotEnteredException(ErrorMessages.SetupNotEntered, typeof(EPSetup), typeof(EPSetup).Name);

			PXUIFieldAttribute.SetEnabled<EPTimecardDetail.overtimeSpent>(Activities.Cache, null, !ShowActivityTime);

			PXDBDateAndTimeAttribute.SetTimeVisible<EPTimecardDetail.date>(Activities.Cache, null, ShowActivityTime);

			bool timeReportingInstalled = PXAccess.FeatureInstalled<CS.FeaturesSet.timeReportingModule>();
			if (!timeReportingInstalled)
			{
				preloadFromTasks.SetVisible(false);
				PXUIFieldAttribute.SetVisible<EPTimeCardSummary.parentNoteID>(Summary.Cache, null, false);
				PXUIFieldAttribute.SetVisible<EPTimecardDetail.approvalStatus>(Activities.Cache, null, false);
				createActivity.SetVisible(false);
			}
			bool projectInstalled = PXAccess.FeatureInstalled<CS.FeaturesSet.projectModule>();
			if (!projectInstalled)
			{
				PXUIFieldAttribute.SetVisible<EPTimecardDetail.approvalStatus>(Activities.Cache, null, false);
			}
			bool contractInstalled = PXAccess.FeatureInstalled<CS.FeaturesSet.contractManagement>();
			if (!contractInstalled)
			{
				PXUIFieldAttribute.SetVisible<ContractEx.contractCD>(Caches[typeof(ContractEx)], null, false);
			}
			bool crmInstalled = PXAccess.FeatureInstalled<CS.FeaturesSet.customerModule>();
			if (!crmInstalled)
			{
				PXUIFieldAttribute.SetVisible<CRCase.caseCD>(Caches[typeof(CRCase)], null, false);
			}

            var startDate_date_fieldName = typeof(EPTimecardDetail.date).Name + PXDBDateAndTimeAttribute.DATE_FIELD_POSTFIX;
            FieldUpdating.AddHandler(typeof(EPTimecardDetail), startDate_date_fieldName, EPTimecardDetail_StartDate_Date_FieldUpdating);
            FieldUpdated.AddHandler(typeof(EPTimecardDetail), startDate_date_fieldName, EPTimecardDetail_StartDate_Date_FieldUpdated);
            FieldVerifying.AddHandler(typeof(EPTimecardDetail), startDate_date_fieldName, EPTimecardDetail_StartDate_Date_FieldVerifying);
            var startDate_time_fieldName = typeof(EPTimecardDetail.date).Name + PXDBDateAndTimeAttribute.TIME_FIELD_POSTFIX;
            FieldUpdating.AddHandler(typeof(EPTimecardDetail), startDate_time_fieldName, EPTimecardDetail_StartDate_Time_FieldUpdating);
			
			Approval.StatusHandler =
				(item) =>
				{
					switch (Approval.GetResult(item))
					{
						case ApprovalResult.Approved:
							item.Status = EPTimeCardStatusAttribute.ApprovedStatus;
							break;

						case ApprovalResult.Rejected:
							item.Status = EPTimeCardStatusAttribute.RejectedStatus;
							break;

						case ApprovalResult.PendingApproval:
							item.Status = EPTimeCardStatusAttribute.OpenStatus;
							break;

						case ApprovalResult.Submitted:
							item.Status = EPTimeCardStatusAttribute.ReleasedStatus;
							break;
					}
				};

			PXDimensionAttribute.SuppressAutoNumbering<EPTimecardDetail.contractCD>(Activities.Cache, true);
		}
		
		public override bool CanClipboardCopyPaste() { return false; }

		public void CheckAllowedUser()
		{
			EPEmployee employeeByUserID = PXSelect<EPEmployee, Where<EPEmployee.userID, Equal<Current<AccessInfo.userID>>>>.Select(this);
			if (employeeByUserID == null && !ProxyIsActive)
			{
				if (IsExport || IsImport)
					throw new PXException(Messages.MustBeEmployee);
				else
					Redirector.Redirect(System.Web.HttpContext.Current, string.Format("~/Frames/Error.aspx?exceptionID={0}&typeID={1}", Messages.MustBeEmployee, "error"));
			}
		}

		public static IEnumerable QSelect(PXGraph graph, BqlCommand bqlCommand, object[] parameters)
		{
			var view = new PXView(graph, false, bqlCommand);
			var startRow = PXView.StartRow;
			int totalRows = 0;

			Stopwatch sw = new Stopwatch();
			sw.Start();

			var list = view.Select(PXView.Currents, parameters, PXView.Searches, PXView.SortColumns, PXView.Descendings, PXView.Filters,
								   ref startRow, PXView.MaximumRows, ref totalRows);

			sw.Stop();
			Debug.Print("activities.QSelect in {0} ms.", sw.ElapsedMilliseconds);

			PXView.StartRow = 0;
			return list;
		}

		public virtual string GetSummaryKey(EPTimecardDetail activity)
		{
			return string.Format("{0}.{1}.{2}.{3}.{4}.{5}.{6}.{7}.{8}.{9}.{10}.{11}", activity.EarningTypeID.ToUpper(),
													   activity.ProjectID.GetValueOrDefault(),
													   activity.ProjectTaskID.GetValueOrDefault(-1),
													   activity.IsBillable.GetValueOrDefault(),
													   activity.ParentTaskNoteID.GetValueOrDefault(Guid.Empty),
													   activity.JobID.GetValueOrDefault(-1),
													   activity.ShiftID.GetValueOrDefault(-1),
													   activity.CostCodeID.GetValueOrDefault(-1),
													   activity.UnionID,
													   activity.LabourItemID.GetValueOrDefault(-1),
													   activity.WorkCodeID,
													   activity.CertifiedJob.GetValueOrDefault());
		}

		public virtual string GetSummaryKey(EPTimeCardSummary summary)
		{
			return string.Format("{0}.{1}.{2}.{3}.{4}.{5}.{6}.{7}.{8}.{9}.{10}.{11}", summary.EarningType.ToUpper(),
													   summary.ProjectID.GetValueOrDefault(),
													   summary.ProjectTaskID.GetValueOrDefault(-1),
													   summary.IsBillable.GetValueOrDefault(),
													   summary.ParentNoteID.GetValueOrDefault(Guid.Empty),
													   summary.JobID.GetValueOrDefault(-1),
													   summary.ShiftID.GetValueOrDefault(-1),
													   summary.CostCodeID.GetValueOrDefault(-1),
													   summary.UnionID,
													   summary.LabourItemID.GetValueOrDefault(-1),
													   summary.WorkCodeID,
													   summary.CertifiedJob.GetValueOrDefault());
		}

		#region Setup Settings

		/// <summary>
		/// Gets the source for the generated PMTran.AccountID
		/// </summary>
		public string ExpenseAccountSource
		{
			get
			{
				string result = PM.PMExpenseAccountSource.InventoryItem;

				PMSetup setup = PXSelect<PMSetup>.Select(this);
				if (setup != null && !string.IsNullOrEmpty(setup.ExpenseAccountSource))
				{
					result = setup.ExpenseAccountSource;
				}

				return result;
			}
		}

		public string ExpenseSubMask
		{
			get
			{
				string result = null;

				PMSetup setup = PXSelect<PMSetup>.Select(this);
				if (setup != null && !string.IsNullOrEmpty(setup.ExpenseSubMask))
				{
					result = setup.ExpenseSubMask;
				}

				return result;
			}
		}

		public string ExpenseAccrualAccountSource
		{
			get
			{
				string result = PM.PMExpenseAccountSource.InventoryItem;

				PMSetup setup = PXSelect<PMSetup>.Select(this);
				if (setup != null && !string.IsNullOrEmpty(setup.ExpenseAccountSource))
				{
					result = setup.ExpenseAccrualAccountSource;
				}

				return result;
			}
		}

		public string ExpenseAccrualSubMask
		{
			get
			{
				string result = null;

				PMSetup setup = PXSelect<PMSetup>.Select(this);
				if (setup != null && !string.IsNullOrEmpty(setup.ExpenseAccrualSubMask))
				{
					result = setup.ExpenseAccrualSubMask;
				}

				return result;
			}
		}

		public string ActivityTimeUnit
		{
			get
			{
				string result = EPSetup.Minute;

				EPSetup setup = PXSelect<EPSetup>.Select(this);
				if (setup != null && !string.IsNullOrEmpty(setup.ActivityTimeUnit))
				{
					result = setup.ActivityTimeUnit;
				}

				return result;
			}
		}

		public string EmployeeRateUnit
		{
			get
			{
				string result = EPSetup.Hour;

				EPSetup setup = PXSelect<EPSetup>.Select(this);
				if (setup != null && !string.IsNullOrEmpty(setup.EmployeeRateUnit))
				{
					result = setup.EmployeeRateUnit;
				}

				return result;
			}
		}

		public bool ShowActivityTime
		{
			get
			{
				bool result = false;

				EPSetup setup = PXSelect<EPSetup>.Select(this);
				if (setup != null)
				{
					result = setup.RequireTimes == true;
				}

				return result;
			}
        }
        #endregion

		#region Actions

		public PXAction<EPTimeCard> viewActivity;
		[PXUIField(Visible = false)]
		[PXButton]
		public virtual IEnumerable ViewActivity(PXAdapter adapter)
		{
			if (Activities.Current != null)
			{
				this.Save.Press();

				PXRedirectHelper.TryRedirect(this, Activities.Current, PXRedirectHelper.WindowMode.NewWindow);
			}
			return adapter.Get();
		}

		public PXAction<EPTimeCard> createActivity;
		[PXUIField(DisplayName = Messages.AddActivity)]
		[PXButton(Tooltip = Messages.AddActivityTooltip,
			ImageKey = PX.Web.UI.Sprite.Main.DataEntry)]
		public virtual IEnumerable CreateActivity(PXAdapter adapter)
		{
			var doc = Document.Current;
			if (doc != null)
			{
				Guid? owner = null;
				EPEmployee emp = Employee.Select();
				if (emp != null)
					owner = emp.UserID;

				new ActivityService().CreateActivity(null, EpSetup.Current.DefaultActivityType, owner);
			}
			return adapter.Get();
		}

		public PXAction<EPTimeCard> action;
		[PXUIField(DisplayName = Messages.Actions)]
		[PXButton(SpecialType = PXSpecialButtonType.ActionsFolder)]
		public virtual IEnumerable Action(PXAdapter adapter)
		{
			return adapter.Get();
		}

		public PXAction<EPTimeCard> submit;

		[PXUIField(DisplayName = Messages.Submit)]
		[PXButton]
		public virtual void Submit()
		{
			OnSubmitClicked();
		}

		protected EmployeeCostEngine costEngine;
		public EmployeeCostEngine CostEngine
		{
			get
			{
				if (costEngine == null)
				{
					costEngine = CreateEmployeeCostEngine();
				}

				return costEngine;
			}
		}

		public virtual bool RequireApproved()
		{
			return false;
		}


		public virtual void OnSubmitClicked()
		{
			if (Document.Current == null) return;

			List<EPTimecardDetail> details = new List<EPTimecardDetail>();
			List<int?> details_laborItemID = new List<int?>();
			HashSet<ActivityValidationError> valiationErrors = new HashSet<ActivityValidationError>();

			EPEmployee employee = PXSelect<EPEmployee, Where<EPEmployee.bAccountID, Equal<Required<EPEmployee.bAccountID>>>>.Select(this, Document.Current.EmployeeID);
			foreach (PXResult<EPTimecardDetail> res in activities())
			{
				EPTimecardDetail act = (EPTimecardDetail)res;
				CRCase refCase = (CRCase)PXSelectorAttribute.Select<EPTimecardDetail.caseCD>(Activities.Cache, act, act.CaseCD);
				PMProject pr = (PMProject)PXSelectorAttribute.Select<EPTimecardDetail.projectID>(Activities.Cache, act, act.ProjectID);

				int? laborClassID = CostEngine.GetLaborClass(act, employee, refCase);
				if (act.Released != true)
				{
					if (laborClassID == null)
					{
						if (act.IsOvertimeCalc == true)
						{
							valiationErrors.Add(ActivityValidationError.OvertimeLaborClassNotSpecified);
						}
						else
						{
							valiationErrors.Add(ActivityValidationError.LaborClassNotSpecified);
						}
					}

					List<ActivityValidationError> activityErrors = ValidateActivityOnSubmit(act, pr);
					valiationErrors.AddRange(activityErrors);

					DisplayErrors(act, pr, activityErrors);
				}
				
				details_laborItemID.Add(laborClassID);
				details.Add(act);
			}

			if (valiationErrors.Count > 0)
			{
				StringBuilder sb = new StringBuilder();

				if (valiationErrors.Contains(ActivityValidationError.ActivityIsNotCompleted))
				{
					sb.AppendLine(PXMessages.LocalizeNoPrefix(Messages.HasOpenActivity));
				}

				if (valiationErrors.Contains(ActivityValidationError.ActivityIsNotApproved) && RequireApproved())
				{
					sb.AppendLine(PXMessages.LocalizeNoPrefix(Messages.HasPendingOrRejectedActivity));
				}

				if (valiationErrors.Contains(ActivityValidationError.ActivityIsRejected))
				{
					sb.AppendLine(PXMessages.LocalizeNoPrefix(Messages.HasPendingOrRejectedActivity));
				}

				if (valiationErrors.Contains(ActivityValidationError.ProjectIsNotActive) ||
					valiationErrors.Contains(ActivityValidationError.ProjectIsCompleted) ||
					valiationErrors.Contains(ActivityValidationError.ProjectTaskIsCancelled) ||
					valiationErrors.Contains(ActivityValidationError.ProjectTaskIsCompleted) ||
					valiationErrors.Contains(ActivityValidationError.ProjectTaskIsNotActive))
				{
					sb.AppendLine(PXMessages.LocalizeNoPrefix(Messages.HasInactiveProject));
				}

				if (valiationErrors.Contains(ActivityValidationError.LaborClassNotSpecified) ||
					valiationErrors.Contains(ActivityValidationError.OvertimeLaborClassNotSpecified))
				{
					sb.AppendLine(PXMessages.LocalizeFormatNoPrefix(Messages.CannotFindLabor, Employee.Current.AcctName));
				}

				string errorText = sb.ToString();

				if (!string.IsNullOrEmpty(errorText))
				{
					throw new PXException(errorText);
				}
			}

			RecalculateTotals(Document.Current, details);
			string errorMsg;
			if (!ValidateTotals(Document.Current, out errorMsg))
			{
				if (employee.HoursValidation == HoursValidationOption.Validate)
				{
					throw new PXException(PXMessages.LocalizeNoPrefix(Messages.TimecardIsNotValid) + " " + errorMsg);
				}
			}

			for (int i = 0; i < details.Count; i++)
			{
				bool updated = false;
				if (details[i].Released != true && details[i].LabourItemID != details_laborItemID[i])
				{
					Activities.Cache.SetValue<EPTimecardDetail.labourItemID>(details[i], details_laborItemID[i]);
					updated = true;
				}

				if (details[i].TimeCardCD != Document.Current.TimeCardCD)
				{
					Activities.Cache.SetValue<EPTimecardDetail.timeCardCD>(details[i], Document.Current.TimeCardCD);
					updated = true;
				}

				if (details[i].Released != true)
				{
					var laborCostRate = CostEngine.CalculateEmployeeCost(details[i].TimeCardCD, details[i].EarningTypeID, details[i].LabourItemID, details[i].ProjectID, details[i].ProjectTaskID, details[i].CertifiedJob, details[i].UnionID, Document.Current.EmployeeID, details[i].Date.Value);

					if (laborCostRate != null && laborCostRate.Rate != null && laborCostRate.Rate != details[i].EmployeeRate)
					{
						Activities.Cache.SetValue<EPTimecardDetail.employeeRate>(details[i], laborCostRate.Rate);
						updated = true;
					}
				}

				if (updated)
				{
					Activities.Cache.MarkUpdated(details[i]);
				}
			}

			if (EpSetup.Current.TimeCardAssignmentMapID != null)
			{
				Approval.Assign(Document.Current, EpSetup.Current.TimeCardAssignmentMapID, EpSetup.Current.TimeCardAssignmentNotificationID);
			}
			else
			{
				Document.Current.IsApproved = true;
			}
			Document.Current.IsHold = false;
			Document.Current.TimeSpent = Document.Current.TimeSpentCalc;
			Document.Current.OvertimeSpent = Document.Current.OvertimeSpentCalc;
			Document.Current.TimeBillable = Document.Current.TimeBillableCalc;
			Document.Current.OvertimeBillable = Document.Current.OvertimeBillableCalc;
			Document.Cache.SetStatus(Document.Current, PXEntryStatus.Updated);
		}

		public virtual List<ActivityValidationError> ValidateActivityOnSubmit(EPTimecardDetail act, PMProject pr)
		{
			List<ActivityValidationError> list = new List<ActivityValidationError>();

			switch (act.ApprovalStatus)
			{
				case ActivityStatusAttribute.Open:
					list.Add(ActivityValidationError.ActivityIsNotCompleted);
					break;

				case ActivityStatusAttribute.PendingApproval:
					list.Add(ActivityValidationError.ActivityIsNotApproved);
					break;

				case ActivityStatusAttribute.Rejected:
					list.Add(ActivityValidationError.ActivityIsRejected);

					break;
			}

			if (pr.IsActive != true)
			{
				list.Add(ActivityValidationError.ProjectIsNotActive);
			}

			if (pr.IsCompleted == true)
			{
				list.Add(ActivityValidationError.ProjectIsCompleted);
			}

			PMTask projectTask = (PMTask)PXSelectorAttribute.Select<PMTimeActivity.projectTaskID>(Activities.Cache, act);
			if (projectTask != null)
			{
				if (projectTask.IsCancelled == true)
				{
					list.Add(ActivityValidationError.ProjectTaskIsCancelled);
				}
				else if (projectTask.IsCompleted == true)
				{
					list.Add(ActivityValidationError.ProjectTaskIsCompleted);
				}
				else if (projectTask.IsActive != true)
				{
					list.Add(ActivityValidationError.ProjectTaskIsNotActive);
				}
			}

			return list;
		}

		public virtual void DisplayErrors(EPTimecardDetail act, PMProject pr, List<ActivityValidationError> errors)
		{
			PMTask projectTask = (PMTask)PXSelectorAttribute.Select<PMTimeActivity.projectTaskID>(Activities.Cache, act);
			foreach (ActivityValidationError error in errors)
			{
				switch (error)
				{
					case ActivityValidationError.ActivityIsNotCompleted:
						Activities.Cache.RaiseExceptionHandling<EPTimecardDetail.date>(act, null, new PXSetPropertyException(Messages.ActivityIsNotCompleted, PXErrorLevel.RowError));
						break;
					case ActivityValidationError.ActivityIsNotApproved:
						if (RequireApproved())
							Activities.Cache.RaiseExceptionHandling<EPTimecardDetail.date>(act, null, new PXSetPropertyException(Messages.ActivityIsNotApproved, PXErrorLevel.RowError));
						break;
					case ActivityValidationError.ProjectIsNotActive:
						Activities.Cache.RaiseExceptionHandling<EPTimecardDetail.projectID>(act, pr.ContractCD, new PXSetPropertyException(Messages.ProjectIsNotActive, PXErrorLevel.Error, pr.ContractCD));
						break;
					case ActivityValidationError.ProjectIsCompleted:
						Activities.Cache.RaiseExceptionHandling<EPTimecardDetail.projectID>(act, pr.ContractCD, new PXSetPropertyException(Messages.ProjectIsCompleted, PXErrorLevel.Error, pr.ContractCD));
						break;
					case ActivityValidationError.ProjectTaskIsCancelled:
						Activities.Cache.RaiseExceptionHandling<EPTimecardDetail.projectTaskID>(act, projectTask.TaskCD, new PXSetPropertyException(Messages.ProjectTaskIsCancelled, PXErrorLevel.Error, pr.ContractCD, projectTask.TaskCD));
						break;
					case ActivityValidationError.ProjectTaskIsCompleted:
						Activities.Cache.RaiseExceptionHandling<EPTimecardDetail.projectTaskID>(act, projectTask.TaskCD, new PXSetPropertyException(Messages.ProjectTaskIsCompleted, PXErrorLevel.Error, pr.ContractCD, projectTask.TaskCD));
						break;
					case ActivityValidationError.ProjectTaskIsNotActive:
						Activities.Cache.RaiseExceptionHandling<EPTimecardDetail.projectTaskID>(act, projectTask.TaskCD, new PXSetPropertyException(Messages.ProjectTaskIsNotActive, PXErrorLevel.Error, pr.ContractCD, projectTask.TaskCD));
						break;
					case ActivityValidationError.LaborClassNotSpecified:
						Activities.Cache.RaiseExceptionHandling<EPTimecardDetail.labourItemID>(act, null, new PXSetPropertyException(Messages.OvertimeLaborClassNotSpecified, PXErrorLevel.RowError));
						break;
					case ActivityValidationError.OvertimeLaborClassNotSpecified:
						Activities.Cache.RaiseExceptionHandling<EPTimecardDetail.labourItemID>(act, null, new PXSetPropertyException(Messages.LaborClassNotSpecified, PXErrorLevel.RowError));
						break;
				}
			}
		}

		public PXAction<EPTimeCard> edit;
		[PXUIField(DisplayName = Messages.PutOnHold)]
		[PXButton]
		public virtual void Edit()
		{
			if (Document.Current != null)
			{
				Document.Current.IsApproved = false;
				Document.Current.IsRejected = false;
				Document.Current.IsHold = true;
				Document.Current.TimeSpent = 0;
				Document.Current.OvertimeSpent = 0;
				Document.Current.TimeBillable = 0;
				Document.Current.OvertimeBillable = 0;
				Document.Cache.SetStatus(Document.Current, PXEntryStatus.Updated);

				PXSelectBase<EPApproval> select = new PXSelect<EPApproval, Where<EPApproval.refNoteID, Equal<Required<EPApproval.refNoteID>>>>(this);
				foreach (EPApproval approval in select.Select((Document.Current.NoteID)))
				{
					this.Caches[typeof(EPApproval)].Delete(approval);
				}

				foreach (EPTimecardDetail item in Activities.Select())
				{
					Activities.Cache.SetValue<EPTimecardDetail.timeCardCD>(item, null);
					Activities.Cache.SetStatus(item, PXEntryStatus.Updated);
				}
			}
		}

		public PXAction<EPTimeCard> release;
		[PXUIField(DisplayName = Messages.Release)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.Release)]
		public virtual IEnumerable Release(PXAdapter adapter)
		{
			List<EPTimeCard> list = new List<EPTimeCard>();
			foreach (var res in adapter.Get())
			{
				EPTimeCard doc = PXResult.Unwrap<EPTimeCard>(res);

				if (doc != null && doc.IsReleased != true)
					list.Add(doc);
			}

			if (list.Any())
			{
				Save.Press();
				PXLongOperation.StartOperation(this, delegate ()
				{
					var releaseGraph = (PM.RegisterEntry)PXGraph.CreateInstance(typeof(PM.RegisterEntry));
					var timecardGraph = PXGraph.CreateInstance<TimeCardMaint>();
					releaseGraph.Clear();

					foreach (EPTimeCard item in list)
					{
						releaseGraph.Clear();
						PXTimeStampScope.SetRecordComesFirst(typeof(EPTimeCard), true);

						PMSetup setup = PXSelect<PMSetup>.Select(releaseGraph);
						if (setup == null)
						{
							//Setup may be null because the PM module is not enabled. and yet we need to be able to generate PMTran for the Contract billing.
							releaseGraph.Setup.Insert();
						}

						using (PXTransactionScope ts = new PXTransactionScope())
						{
							timecardGraph.Clear();

							//as setcurrent doesn't place the item into the cache, set status is raised
							timecardGraph.Document.Current = item;
							timecardGraph.Document.Cache.SetStatus(item, PXEntryStatus.Notchanged);

							if (string.IsNullOrEmpty(item.OrigTimeCardCD))
							{
								timecardGraph.ProcessRegularTimecard(releaseGraph, item);
							}
							else
							{
								timecardGraph.ProcessCorrectingTimecard(releaseGraph, item);
							}

							if(EPSetupMaint.GetPostingOption(this, EpSetup.Current, item.EmployeeID) != EPPostOptions.DoNotPost)
							{
								releaseGraph.Save.Press();
							}
							
							item.Status = EPTimeCardStatusAttribute.ReleasedStatus;
							item.IsReleased = true;
							timecardGraph.Document.Update(item);

							timecardGraph.Save.Press();

							ts.Complete();
						}

						if (EpSetup.Current.AutomaticReleasePM == true)
						{
							PX.Objects.PM.RegisterRelease.Release(releaseGraph.Document.Current);
						}
					}
				});
			}
			else throw new PXException(Messages.AlreadyReleased);

			return list;
		}

		public PXAction<EPTimeCard> correct;
		[PXUIField(DisplayName = Messages.Correct)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.Release)]
		public virtual IEnumerable Correct(PXAdapter adapter)
		{
			if (Document.Current != null)
			{
				EPTimeCard source = GetLastCorrection(Document.Current);

				if (source.IsReleased != true)
					return new EPTimeCard[] { source };


				EPTimeCard newCard = (EPTimeCard)Document.Cache.CreateInstance();
				newCard.WeekID = source.WeekID;
				newCard.OrigTimeCardCD = source.TimeCardCD;
				newCard = Document.Insert(newCard);
				newCard.EmployeeID = source.EmployeeID;
				PXNoteAttribute.CopyNoteAndFiles(Document.Cache, source, Document.Cache, newCard, true, true);

				bool failed = false;

				Dictionary<string, TimeCardSummaryCopiedInfo> summaryDescriptions = new Dictionary<string, TimeCardSummaryCopiedInfo>(); 
				foreach (EPTimeCardSummary summary in Summary.View.SelectMultiBound(new object[] { source }))
				{
					string key = GetSummaryKey(summary);
					if (!summaryDescriptions.ContainsKey(key))
					{
						string note = PXNoteAttribute.GetNote(Summary.Cache, summary);
						var info = new TimeCardSummaryCopiedInfo(summary.Description, note);
						summaryDescriptions.Add(key, info);
					}
				}

				foreach (EPTimecardDetail act in TimecardActivities.View.SelectMultiBound(new object[] { source }))
				{
					EPTimecardDetail newActivity = PXCache<EPTimecardDetail>.CreateCopy(act);
					newActivity.Released = false;
					newActivity.Billed = false;
					newActivity.NoteID = null;
					newActivity.TimeCardCD = null;
					newActivity.TimeSheetCD = null;
					newActivity.OrigNoteID = act.NoteID; //relation between the original activity and the corrected one.
					newActivity.Date = act.Date;
					newActivity.Billed = false;
					newActivity.SummaryLineNbr = null;
					newActivity.NoteID = null;
					newActivity.ContractCD = null;
					isCreateCorrectionFlag = true;
					try
					{
						newActivity = Activities.Insert(newActivity);
					}
					catch (PXSetPropertyException ex)
					{
						failed = true;
						Activities.Cache.RaiseExceptionHandling<EPTimecardDetail.summary>(act, act.Summary, new PXSetPropertyException(ex.MessageNoPrefix, PXErrorLevel.RowError));
						continue;
					}
					newActivity.TrackTime = act.TrackTime; //copy as is.
					isCreateCorrectionFlag = false;
					newActivity.ApprovalStatus = ActivityStatusAttribute.Completed;
					newActivity.RefNoteID = act.NoteID == act.RefNoteID ? newActivity.NoteID : act.RefNoteID;
					newActivity.ContractCD = act.ContractCD;

					PXNoteAttribute.CopyNoteAndFiles(Activities.Cache, act, Activities.Cache, newActivity);

					Activities.Cache.SetValue<EPTimecardDetail.isCorrected>(act, true);
					Activities.Cache.SetStatus(act, PXEntryStatus.Updated);
				}

				if (failed)
				{
					throw new PXException(Messages.FailedToCreateCorrectionTC);
				}

				foreach (EPTimeCardItem item in Items.View.SelectMultiBound(new object[] { source }))
				{
					EPTimeCardItem record = Items.Insert();
					record.ProjectID = item.ProjectID;
					record.TaskID = item.TaskID;
					record.Description = item.Description;
					record.InventoryID = item.InventoryID;
					record.CostCodeID = item.CostCodeID;
					record.UOM = item.UOM;
					record.Mon = item.Mon;
					record.Tue = item.Tue;
					record.Wed = item.Wed;
					record.Thu = item.Thu;
					record.Fri = item.Fri;
					record.Sat = item.Sat;
					record.Sun = item.Sun;
					record.OrigLineNbr = item.LineNbr;//relation between the original activity and the corrected one.
				}

				foreach (EPTimeCardSummary summary in Summary.Select())
				{
					string key = GetSummaryKey(summary);
					if (summaryDescriptions.ContainsKey(key))
					{
						PXNoteAttribute.SetNote(Summary.Cache, summary, summaryDescriptions[key].Note);
						Summary.Cache.SetValue<EPTimeCardSummary.description>(summary, summaryDescriptions[key].Description);
					}
				}

				return new EPTimeCard[] { newCard };
			}

			return adapter.Get();
		}


		public PXAction<EPTimeCard> preloadFromTasks;
		[PXUIField(DisplayName = Messages.PreloadFromTasks)]
		[PXButton(Tooltip = Messages.PreloadFromTasksTooltip)]
		public virtual void PreloadFromTasks()
		{
			if (Tasks.AskExt() == WebDialogResult.OK)
			{
				foreach (EPTimecardTask task in Tasks.Cache.Updated)
				{
					if (task.Selected == true)
					{
						bool alreadyExists = false;
						foreach (EPTimeCardSummaryWithInfo existing in Summary.Select())
						{
							if (existing.ProjectID == task.ProjectID && existing.ProjectTaskID == task.ProjectTaskID
								&& existing.JobID == task.JobID && existing.ShiftID == task.ShiftID && existing.CostCodeID == task.CostCodeID)
							{
								alreadyExists = true;
								break;
							}
						}

						if (alreadyExists)
							continue;

						EPTimeCardSummaryWithInfo summary = (EPTimeCardSummaryWithInfo)Summary.Cache.CreateInstance();
						summary.ParentNoteID = task.NoteID;
						summary.ProjectID = task.ProjectID;
						summary.ProjectTaskID = task.ProjectTaskID;
						summary.IsBillable = task.IsBillable;
						summary.Description = task.Subject;
						summary.JobID = task.JobID;
						summary.ShiftID = task.ShiftID;
						summary.CostCodeID = task.CostCodeID;
						PMProject project = (PMProject)PXSelectorAttribute.Select<EPTimecardTask.projectID>(Tasks.Cache, task, task.ProjectID);
						if (project != null)
						{
							summary.ProjectManager = project.ApproverID;
						}
						PMTask projectTask = (PMTask)PXSelectorAttribute.Select<EPTimecardTask.projectTaskID>(Tasks.Cache, task, task.ProjectTaskID);
						if (projectTask != null)
						{
							summary.TaskApproverID = projectTask.ApproverID;
						}

						summary = Summary.Insert(summary);
					}
				}

				if (Document.Cache.GetStatus(Document.Current) != PXEntryStatus.Inserted)
					this.Save.Press();
			}
		}

		public PXAction<EPTimeCard> preloadFromPreviousTimecard;
		[PXUIField(DisplayName = Messages.PreloadFromPreviousTimecard)]
		[PXButton(Tooltip = Messages.PreloadFromPreviousTimecardTooltip)]
		public virtual void PreloadFromPreviousTimecard()
		{
			if (Document.Current == null)
				return;

			if (Document.Current.WeekID == null)
				return;

			EPEmployee employee =
				PXSelect<EPEmployee, Where<EPEmployee.bAccountID, Equal<Current<EPTimeCard.employeeID>>>>.Select(this);

			if (employee == null)
				return;

			EPTimeCard previous = PXSelect<EPTimeCard, Where<EPTimeCard.employeeID, Equal<Current<EPTimeCard.employeeID>>, And<EPTimeCard.weekId, Equal<Required<EPTimeCard.weekId>>>>>.Select(this, Document.Current.WeekID.Value - 1);

			if (previous == null)
			{
				var select = new PXSelect<EPTimeCard,
				Where<EPTimeCard.employeeID, Equal<Current<EPTimeCard.employeeID>>, And<EPTimeCard.weekId, NotEqual<Required<EPTimeCard.weekId>>>>,
				OrderBy<Desc<EPTimeCard.timeCardCD>>>(this);

				previous = select.SelectWindowed(0, 1, Document.Current.WeekID);
			}

			if (previous == null)
				return;

			PXSelectBase<EPTimeCardSummaryWithInfo> summarySelect = new PXSelect<EPTimeCardSummaryWithInfo, Where<EPTimeCardSummaryWithInfo.timeCardCD, Equal<Required<EPTimeCardSummaryWithInfo.timeCardCD>>>>(this);
			foreach (EPTimeCardSummaryWithInfo item in summarySelect.Select(previous.TimeCardCD))
			{
				if (item.EarningType != EpSetup.Current.HolidaysType &&
					item.EarningType != EpSetup.Current.VacationsType)
				{

					bool skip = false;
					foreach (EPTimeCardSummaryWithInfo existing in Summary.Select())
					{
						if (existing.ProjectID == item.ProjectID
							&& existing.ProjectTaskID == item.ProjectTaskID
							&& existing.JobID == item.JobID
							&& existing.ShiftID == item.ShiftID
							&& existing.EarningType == item.EarningType
							&& existing.CostCodeID == item.CostCodeID
							&& existing.UnionID == item.UnionID
							&& existing.LabourItemID == item.LabourItemID
							&& existing.WorkCodeID == item.WorkCodeID)
						{
							skip = true;
							break;
						}

						PMProject project = PXSelect<PMProject, Where<PMProject.contractID, Equal<Required<PMProject.contractID>>>>.Select(this, item.ProjectID);
						if (project.IsCompleted == true || project.IsCancelled == true || project.VisibleInTA != true)
						{
							skip = true;
						}

						if (item.ProjectTaskID != null)
						{
							PMTask task = PXSelect<PMTask, Where<PMTask.taskID, Equal<Required<PMTask.taskID>>>>.Select(this, item.ProjectTaskID);
							if (task.IsCompleted == true || task.IsCancelled == true || project.VisibleInTA != true)
							{
								skip = true;
							}
						}
					}

					if (skip)
						continue;

					EPTimeCardSummaryWithInfo summary = (EPTimeCardSummaryWithInfo)PXCache<EPTimeCardSummaryWithInfo>.CreateCopy(item);
					summary.TimeCardCD = null;
					summary.Description = null;
					summary.Mon = null;
					summary.Tue = null;
					summary.Wed = null;
					summary.Thu = null;
					summary.Fri = null;
					summary.Sat = null;
					summary.Sun = null;
					summary.NoteID = null;

					Summary.Insert(summary);
				}
			}

			if (Document.Cache.GetStatus(Document.Current) != PXEntryStatus.Inserted)
				this.Save.Press();
		}

		public PXAction<EPTimeCard> preloadHolidays;
		[PXUIField(DisplayName = Messages.PreloadHolidays)]
		[PXButton(Tooltip = Messages.PreloadHolidays)]
		public virtual void PreloadHolidays()
		{
			if (Document.Current == null)
				return;

			if (Document.Current.WeekStartDate == null)
				return;

			EPEmployee employee =
				PXSelect<EPEmployee, Where<EPEmployee.bAccountID, Equal<Current<EPTimeCard.employeeID>>>>.Select(this);

			if (employee == null)
				return;

			decimal? regularHours = CostEngine.GetEmployeeRegularHours(Document.Current.EmployeeID, Document.Current.WeekStartDate);
			int minutesPerday = (int)(regularHours * 60 / 5);
			EPTimeCardSummaryWithInfo summary = new EPTimeCardSummaryWithInfo();
			summary.EarningType = EpSetup.Current.HolidaysType;
			summary.IsBillable = false;
			summary.Description = PXMessages.LocalizeNoPrefix(Messages.HolidayDesc);
						
			for (int i = 0; i < 7; i++)
			{
				var day = Document.Current.WeekStartDate.Value.AddDays(i);
				if (CalendarHelper.IsHoliday(this, employee.CalendarID, day))
					summary.SetDayTime(day.DayOfWeek, minutesPerday);
			}
			
			if (summary.GetTimeTotal() > 0)
				Summary.Insert(summary);

			if (Document.Cache.GetStatus(Document.Current) != PXEntryStatus.Inserted)
				this.Save.Press();
		}

		public PXAction<EPTimeCard> normalizeTimecard;
		[PXUIField(DisplayName = Messages.NormalizeTimecard)]
		[PXButton(Tooltip = Messages.NormalizeTimecard)]
		public virtual void NormalizeTimecard()
		{
			if (Document.Current == null)
				return;

			if (!Summary.Cache.AllowInsert)
				return;

			decimal? regularHours = CostEngine.GetEmployeeRegularHours(Document.Current.EmployeeID, Document.Current.WeekStartDate);
			int minutesPerday = (int)(regularHours * 60 / 5);

			int monMinutes = 0;
			int tueMinutes = 0;
			int wedMinutes = 0;
			int thuMinutes = 0;
			int friMinutes = 0;
			int satMinutes = 0;
			int sunMinutes = 0;

			int? nonProjectID = PM.ProjectDefaultAttribute.NonProject();
			foreach (EPTimeCardSummaryWithInfo item in Summary.Select())
			{

				monMinutes += item.Mon.GetValueOrDefault();
				tueMinutes += item.Tue.GetValueOrDefault();
				wedMinutes += item.Wed.GetValueOrDefault();
				thuMinutes += item.Thu.GetValueOrDefault();
				friMinutes += item.Fri.GetValueOrDefault();
				satMinutes += item.Sat.GetValueOrDefault();
				sunMinutes += item.Sun.GetValueOrDefault();
			}

			int delta = (int)(regularHours * 60) - (monMinutes + tueMinutes + wedMinutes + thuMinutes + friMinutes + satMinutes + sunMinutes);

			if (delta <= 0)
				return;

			EPTimeCardSummaryWithInfo summary = new EPTimeCardSummaryWithInfo();
			summary.EarningType = EpSetup.Current.RegularHoursType;
			summary.IsBillable = false;
			summary.Description = PXMessages.LocalizeNoPrefix(Messages.NormalizationDesc);
			summary.ProjectID = ProjectDefaultAttribute.NonProject();
			try
			{
				dontSyncDetails = true;
				skipProjectDefaultingFromEarningType = true;
				summary = Summary.Insert(summary);
			}
			finally
			{
				dontSyncDetails = false;
				skipProjectDefaultingFromEarningType = false;
			}
			
			PX.Objects.EP.PXWeekSelector2Attribute.WeekInfo weekInfo = PXWeekSelector2Attribute.GetWeekInfo(this, Document.Current.WeekID.Value);

			isCreateCorrectionFlag = true;
			int monQuota = minutesPerday - monMinutes;
			if (monQuota > 0)
			{
				EPTimecardDetail detailNormalize_Mon = (EPTimecardDetail) Activities.Cache.CreateInstance();
				detailNormalize_Mon.Date = SetEmployeeTime(weekInfo.Mon.Date).Value;
				detailNormalize_Mon.TimeSpent = delta < monQuota ? delta : monQuota;
				detailNormalize_Mon.Summary = PXMessages.LocalizeNoPrefix(Messages.NormalizationDesc);
				detailNormalize_Mon.ProjectID = ProjectDefaultAttribute.NonProject();
				detailNormalize_Mon.IsBillable = false;
				if (summary != null) detailNormalize_Mon.SummaryLineNbr = summary.LineNbr;
				Activities.Insert(detailNormalize_Mon);
				delta = delta - monQuota;
			}

			if (delta >= 0)
			{
				int tueQuota = minutesPerday - tueMinutes;
				if (tueQuota > 0)
				{
					EPTimecardDetail detailNormalize_Tue = (EPTimecardDetail)Activities.Cache.CreateInstance();
					detailNormalize_Tue.Date = SetEmployeeTime(weekInfo.Tue.Date).Value;
					detailNormalize_Tue.TimeSpent = delta < tueQuota ? delta : tueQuota;
					detailNormalize_Tue.Summary = PXMessages.LocalizeNoPrefix(Messages.NormalizationDesc);
					detailNormalize_Tue.ProjectID = ProjectDefaultAttribute.NonProject();
					detailNormalize_Tue.IsBillable = false;
					if (summary != null) detailNormalize_Tue.SummaryLineNbr = summary.LineNbr;
					Activities.Insert(detailNormalize_Tue);
					delta = delta - tueQuota;
				}
			}

			if (delta >= 0)
			{
				int wedQuota = minutesPerday - wedMinutes;
				if (wedQuota > 0)
				{
					EPTimecardDetail detailNormalize_Wed = (EPTimecardDetail)Activities.Cache.CreateInstance();
					detailNormalize_Wed.Date = SetEmployeeTime(weekInfo.Wed.Date).Value;
					detailNormalize_Wed.TimeSpent = delta < wedQuota ? delta : wedQuota;
					detailNormalize_Wed.Summary = PXMessages.LocalizeNoPrefix(Messages.NormalizationDesc);
					detailNormalize_Wed.ProjectID = ProjectDefaultAttribute.NonProject();
					detailNormalize_Wed.IsBillable = false;
					if (summary != null) detailNormalize_Wed.SummaryLineNbr = summary.LineNbr;
					Activities.Insert(detailNormalize_Wed);
					delta = delta - wedQuota;
				}
			}

			if (delta >= 0)
			{
				int thuQuota = minutesPerday - thuMinutes;
				if (thuQuota > 0)
				{
					EPTimecardDetail detailNormalize_Thu = (EPTimecardDetail)Activities.Cache.CreateInstance();
					detailNormalize_Thu.Date = SetEmployeeTime(weekInfo.Thu.Date).Value;
					detailNormalize_Thu.TimeSpent = delta < thuQuota ? delta : thuQuota;
					detailNormalize_Thu.Summary = PXMessages.LocalizeNoPrefix(Messages.NormalizationDesc);
					detailNormalize_Thu.ProjectID = ProjectDefaultAttribute.NonProject();
					detailNormalize_Thu.IsBillable = false;
					if (summary != null) detailNormalize_Thu.SummaryLineNbr = summary.LineNbr;
					Activities.Insert(detailNormalize_Thu);
					delta = delta - thuQuota;
				}
			}

			if (delta >= 0)
			{
				int friQuota = minutesPerday - friMinutes;
				if (friQuota > 0)
				{
					EPTimecardDetail detailNormalize_Fri = (EPTimecardDetail)Activities.Cache.CreateInstance();
					detailNormalize_Fri.Date = SetEmployeeTime(weekInfo.Fri.Date).Value;
					detailNormalize_Fri.TimeSpent = delta < friQuota ? delta : friQuota;
					detailNormalize_Fri.Summary = PXMessages.LocalizeNoPrefix(Messages.NormalizationDesc);
					detailNormalize_Fri.ProjectID = ProjectDefaultAttribute.NonProject();
					detailNormalize_Fri.IsBillable = false;
					if (summary != null) detailNormalize_Fri.SummaryLineNbr = summary.LineNbr;
					Activities.Insert(detailNormalize_Fri);
					delta = delta - friQuota;
				}
			}
			isCreateCorrectionFlag = false;

			//by this time delta should be less then equal to zero
			Debug.Assert(delta <= 0);


			if (Document.Cache.GetStatus(Document.Current) != PXEntryStatus.Inserted)
				this.Save.Press();
		}

		public PXAction<EPTimeCard> View;
		[PXUIField(DisplayName = Messages.View)]
		[PXButton()]
		public virtual IEnumerable view(PXAdapter adapter)
		{
            var row = Activities.Current;
            if (row != null)
            {
				if (Document.Cache.GetStatus(Document.Current) == PXEntryStatus.Inserted)
				{
					throw new PXException(Messages.TimecardMustBeSaved);
				}
				if(AutomationView != null && Views[AutomationView] != null)
					PXAutomation.GetStep(this,
					  new object[] { Views[AutomationView].Cache.Current },
					  BqlCommand.CreateInstance(typeof(Select<>), Views[AutomationView].Cache.GetItemType())
				         );

				var result = new List<object>(Save.Press(adapter).Cast<object>());
                PXRedirectHelper.TryRedirect(this, row, PXRedirectHelper.WindowMode.NewWindow);
				return result;

            }
            return adapter.Get();
		}

		public PXAction<EPTimeCard> viewPMTran;
		[PXUIField(DisplayName = PM.Messages.ViewTransactions, FieldClass = ProjectAttribute.DimensionName)]
		[PXButton()]
		public virtual IEnumerable ViewPMTran(PXAdapter adapter)
		{
			if (Document.Current != null)
			{
				RegisterEntry graph = PXGraph.CreateInstance<RegisterEntry>();
				graph.Clear();
				PMRegister registr = PXSelect<PMRegister
					, Where<PMRegister.origDocType, Equal<PMOrigDocType.timeCard>
						, And<PMRegister.origNoteID, Equal<Current<EPTimeCard.noteID>>>>>.SelectSingleBound(this, new object[] { Document });
				if (registr == null)
				{
					adapter.View.Ask(Messages.TransactionNotExists, MessageButtons.OK);
				}
				else
				{
					graph.Document.Current = registr;
					throw new PXRedirectRequiredException(graph, PM.Messages.ViewTransactions);
				}
			}
			return adapter.Get();
		}

		#endregion

		#region Event Handlers

		protected virtual void EPTimecardDetail_EarningTypeID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			EPTimecardDetail row = (EPTimecardDetail)e.Row;
			if (row != null && e.NewValue == null)
			{
				e.Cancel = true;
				throw new PXSetPropertyException(ErrorMessages.FieldIsEmpty, typeof(EPTimecardDetail.earningTypeID).Name);
			}
		}

		protected virtual void EPTimeCardSummaryWithInfo_EarningType_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			EPTimeCardSummaryWithInfo row = (EPTimeCardSummaryWithInfo)e.Row;
			if (row != null && e.NewValue == null)
			{
				e.Cancel = true;
				throw new PXSetPropertyException(ErrorMessages.FieldIsEmpty, typeof(EPTimeCardSummary.earningType).Name);
			}
		}

		protected virtual void _(Events.FieldDefaulting<EPTimeCard, EPTimeCard.weekId> e)
		{
			if (e.Row != null)
			{
				e.NewValue = GetNextWeekID(e.Row.EmployeeID);
			}
		}

		protected virtual void EPTimeCard_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			var row = e.Row as EPTimeCard;
			var oldRow = e.OldRow as EPTimeCard;
			if (row == null || oldRow == null) return;

			var employeeChanged = row.EmployeeID != oldRow.EmployeeID;
			if (row.WeekID == null || employeeChanged)
			{
				try
				{
					sender.SetValueExt<EPTimeCard.weekId>(row, GetNextWeekID(row.EmployeeID));
				}
				catch (PXException exception)
				{
					sender.SetValueExt<EPTimeCard.weekId>(row, null);
					sender.RaiseExceptionHandling<EPTimeCard.weekId>(row, null, exception);
				}
			}

			if (employeeChanged)
			{
				Items.Cache.Clear();
				Activities.Cache.Clear();
				Summary.Cache.Clear();
				this.Clear(PXClearOption.ClearQueriesOnly);
			}
		}

		protected virtual void EPTimeCard_RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
		{
			var row = e.Row as EPTimeCard;
			if (row == null) return;

			foreach (EPTimeCardSummaryWithInfo summary in Summary.Select())
			{
				try
				{
					dontSyncDetails = true;
					Summary.Delete(summary);
				}
				finally
				{
					dontSyncDetails = false;
				}
			}

			try
			{
				dontSyncSummary = true;
				PXSelectBase<EPTimecardDetail> selectDetail = new PXSelectJoin<EPTimecardDetail,
					InnerJoin<CREmployee, On<CREmployee.userID, Equal<EPTimecardDetail.ownerID>>>,
					Where<CREmployee.bAccountID, Equal<Current<EPTimeCard.employeeID>>,
					And<EPTimecardDetail.weekID, Equal<Current<EPTimeCard.weekId>>,
					And<EPTimecardDetail.released, NotEqual<True>,
					And<EPTimecardDetail.isCorrected, NotEqual<True>,
					And2<Where<EPTimecardDetail.summaryLineNbr, IsNotNull, Or<EPTimecardDetail.origNoteID, IsNotNull>>
									, And<Where<EPTimecardDetail.timeCardCD, IsNull, Or<EPTimecardDetail.timeCardCD, Equal<Current<EPTimeCard.timeCardCD>>>>>>>>>>>(this);

				foreach (PXResult<EPTimecardDetail, CREmployee> res in selectDetail.Select())
				{
					EPTimecardDetail activity = (EPTimecardDetail)res;
					if (activity.OrigNoteID != null)
					{
						EPTimecardDetail originActivity = PXSelect<EPTimecardDetail>.Search<EPTimecardDetail.noteID>(this, activity.OrigNoteID);
						if (originActivity != null)
						{
							originActivity.IsCorrected = false;
							Activities.Cache.MarkUpdated(originActivity);
						}
					}
					Activities.Delete(activity);
				}

			}
			finally
			{
				dontSyncSummary = false;
			}

		}

		protected virtual void EPTimeCard_RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
			EPTimeCard row = (EPTimeCard)e.Row;
			EPTimeCard timeCard = PXSelect<EPTimeCard
								, Where<
									EPTimeCard.employeeID, Equal<Current<EPTimeCard.employeeID>>
										, And<EPTimeCard.weekId, Greater<Current<EPTimeCard.weekId>>
											, And<Where<EPTimeCard.timeCardCD, Equal<Current<EPTimeCard.origTimeCardCD>>, Or<Current<EPTimeCard.origTimeCardCD>, IsNull>>>
										>
									>
								>.SelectWindowed(this, 0, 1);
			if (timeCard != null && timeCard.TimeCardCD != row.OrigTimeCardCD)
			{
				throw new PXException(Messages.TimeCardNoDelete);
			}

		}

		protected virtual void _(Events.RowPersisting<EPTimeCard> e)
		{
			if (e.Operation == PXDBOperation.Insert && string.IsNullOrEmpty(e.Row.OrigTimeCardCD))
			{
				//Check for duplicate timecard for the given week:
				EPTimeCard duplicate = PXSelectReadonly<EPTimeCard, Where<EPTimeCard.employeeID, Equal<Required<EPTimeCard.employeeID>>,
					And<EPTimeCard.weekId, Equal<Required<EPTimeCard.weekId>>,
					And<EPTimeCard.timeCardCD, NotEqual<Required<EPTimeCard.timeCardCD>>>>>>.Select(this, e.Row.EmployeeID, e.Row.WeekID, e.Row.TimeCardCD);

				if (duplicate != null)
				{
					throw new PXException(Messages.DuplicateTimecard);
				}
			}
		}

		protected virtual void EPTimeCard_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			var row = e.Row as EPTimeCard;
			if (row == null) return;

			var isSubordinatedEmployee = row.EmployeeID == null ||
				PXSubordinateSelectorAttribute.IsSubordinated(this, row.EmployeeID);

			if (!isSubordinatedEmployee)
			{
				EPEmployee wingmen = PXSelect<EPEmployee, Where<EPEmployee.bAccountID, WingmanUser<Current<AccessInfo.userID>>>>.SelectSingleBound(this, null);
				isSubordinatedEmployee = wingmen != null;
			}

			if (!isSubordinatedEmployee)
			{
				PXUIFieldAttribute.SetEnabled(sender, row, false);
				PXUIFieldAttribute.SetEnabled<EPTimeCard.timeCardCD>(sender, row, true);
			}

			EPEmployee employee = PXSelect<EPEmployee, Where<EPEmployee.bAccountID, Equal<Current<EPTimeCard.employeeID>>>>.Select(this);
			if (employee != null)
			{
				PXTimeZoneInfo currentTimeZone = LocaleInfo.GetTimeZone();
				if (currentTimeZone != null)
				{
					UserPreferences pref = PXSelect<UserPreferences, Where<UserPreferences.userID, Equal<Required<UserPreferences.userID>>>>.Select(sender.Graph, employee.UserID);
					if (pref != null && !string.IsNullOrEmpty(pref.TimeZone))
					{
						if (currentTimeZone.Id != pref.TimeZone)
					{
						PXTimeZoneInfo empTimeZone = PXTimeZoneInfo.FindSystemTimeZoneById(pref.TimeZone);
							PXUIFieldAttribute.SetWarning<EPTimeCard.employeeID>(sender, e.Row, string.Format(PXMessages.LocalizeNoPrefix(Messages.CurrentUserTimeZoneDiffersFromEmployeeTimeZone), currentTimeZone.DisplayName, empTimeZone.DisplayName));
						}
					}
					else if (employee.CalendarID != null)
					{
						CSCalendar cal = PXSelect<CSCalendar, Where<CSCalendar.calendarID, Equal<Required<CSCalendar.calendarID>>>>.Select(this, employee.CalendarID);
						if (cal != null && !string.IsNullOrEmpty(cal.TimeZone) && currentTimeZone.Id != cal.TimeZone)
						{
							PXTimeZoneInfo empTimeZone = PXTimeZoneInfo.FindSystemTimeZoneById(cal.TimeZone);
							if (empTimeZone != null)
								PXUIFieldAttribute.SetWarning<EPTimeCard.employeeID>(sender, e.Row, string.Format(PXMessages.LocalizeNoPrefix(Messages.CurrentUserTimeZoneDiffersFromEmployeeTimeZone), currentTimeZone.DisplayName, empTimeZone.DisplayName));
						}
					}
				}
			}

			ValidateDisplayVSReportedTimeZones(row);

			Document.Cache.AllowDelete = isSubordinatedEmployee && row.IsReleased != true && row.Status == EPTimeCardStatusAttribute.HoldStatus;

			var allowEdit = isSubordinatedEmployee &&
				row.IsHold == true && row.IsApproved != true &&
				row.IsRejected != true && row.IsReleased != true &&
				row.WeekID != null;

			Activities.Cache.AllowInsert = allowEdit;
			Activities.Cache.AllowUpdate = allowEdit;
			Activities.Cache.AllowDelete = allowEdit;
			Summary.Cache.AllowInsert = allowEdit;
			Summary.Cache.AllowUpdate = allowEdit;
			Summary.Cache.AllowDelete = allowEdit;
			Items.Cache.AllowInsert = allowEdit;
			Items.Cache.AllowUpdate = allowEdit;
			Items.Cache.AllowDelete = allowEdit;
			preloadFromTasks.SetEnabled(allowEdit);
			preloadFromPreviousTimecard.SetEnabled(allowEdit);
			preloadHolidays.SetEnabled(allowEdit);
			normalizeTimecard.SetEnabled(allowEdit);
			viewPMTran.SetEnabled(row.IsReleased == true);

			var isInserted = sender.GetStatus(row) == PXEntryStatus.Inserted;
			createActivity.SetEnabled(allowEdit && !isInserted);
			submit.SetEnabled(!isInserted);

			if (row.EmployeeID != null)
			{
				if (Summary.Select().Count > 0 && Document.Cache.GetStatus(row) != PXEntryStatus.Inserted)
				{
					PXUIFieldAttribute.SetEnabled<EPTimeCard.employeeID>(sender, row, false);
				}
				else
				{
					PXUIFieldAttribute.SetEnabled<EPTimeCard.employeeID>(sender, row, allowEdit && row.OrigTimeCardCD == null);
				}
			}

			var isFirst = IsFirstTimeCard(row.EmployeeID);
			PXUIFieldAttribute.SetEnabled<EPTimeCard.weekId>(sender, row, isFirst && allowEdit);

			RecalculateTotals(row);
			string errorMsg;
			ValidateTotals(row, out errorMsg);
						
			if (row.WeekID != null)
			{
				PX.Objects.EP.PXWeekSelector2Attribute.WeekInfo weekInfo = PXWeekSelector2Attribute.GetWeekInfo(this, row.WeekID.Value);
				PXUIFieldAttribute.SetEnabled<EPTimeCardSummary.mon>(Summary.Cache, null, weekInfo.Mon.Enabled);
				PXUIFieldAttribute.SetEnabled<EPTimeCardSummary.tue>(Summary.Cache, null, weekInfo.Tue.Enabled);
				PXUIFieldAttribute.SetEnabled<EPTimeCardSummary.wed>(Summary.Cache, null, weekInfo.Wed.Enabled);
				PXUIFieldAttribute.SetEnabled<EPTimeCardSummary.thu>(Summary.Cache, null, weekInfo.Thu.Enabled);
				PXUIFieldAttribute.SetEnabled<EPTimeCardSummary.fri>(Summary.Cache, null, weekInfo.Fri.Enabled);
				PXUIFieldAttribute.SetEnabled<EPTimeCardSummary.sat>(Summary.Cache, null, weekInfo.Sat.Enabled);
				PXUIFieldAttribute.SetEnabled<EPTimeCardSummary.sun>(Summary.Cache, null, weekInfo.Sun.Enabled);
				PXUIFieldAttribute.SetEnabled<EPTimeCardItem.mon>(Items.Cache, null, weekInfo.Mon.Enabled);
				PXUIFieldAttribute.SetEnabled<EPTimeCardItem.tue>(Items.Cache, null, weekInfo.Tue.Enabled);
				PXUIFieldAttribute.SetEnabled<EPTimeCardItem.wed>(Items.Cache, null, weekInfo.Wed.Enabled);
				PXUIFieldAttribute.SetEnabled<EPTimeCardItem.thu>(Items.Cache, null, weekInfo.Thu.Enabled);
				PXUIFieldAttribute.SetEnabled<EPTimeCardItem.fri>(Items.Cache, null, weekInfo.Fri.Enabled);
				PXUIFieldAttribute.SetEnabled<EPTimeCardItem.sat>(Items.Cache, null, weekInfo.Sat.Enabled);
				PXUIFieldAttribute.SetEnabled<EPTimeCardItem.sun>(Items.Cache, null, weekInfo.Sun.Enabled);
			}
		}

		protected virtual void EPTimeCard_EmployeeID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			EPTimeCard row = e.Row as EPTimeCard;
			if (row != null && row.WeekID != null)
			{
				EPTimecardLite futureTimeCard = PXSelect<EPTimecardLite, Where<EPTimecardLite.employeeID, Equal<Current<EPTimeCard.employeeID>>, And<EPTimecardLite.weekId, Greater<Current<EPTimeCard.weekId>>>>>.Select(this);
				if (futureTimeCard != null)
				{
					throw new PXSetPropertyException(Messages.TimeCardInFutureExists);
				}

			}
		}

		protected virtual void EPTimeCard_IsApproved_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			EPTimeCard row = e.Row as EPTimeCard;
			if (row == null) return;

			if ((bool?)e.NewValue == true)
			{
				//all non zero activities must be approved before timecard can be approved.
				PXView detailsView = new PXView(this, false, Activities.View.BqlSelect);
				foreach (PXResult res in activities())
				{
					EPTimecardDetail act = (EPTimecardDetail)res[typeof(EPTimecardDetail)];

					if (act.Released == true)
						continue;

					if ((act.TimeSpent.GetValueOrDefault() != 0 || act.TimeBillable.GetValueOrDefault() != 0) &&
						act.ApprovalStatus != ActivityStatusListAttribute.Completed &&
						act.ApprovalStatus != ActivityStatusListAttribute.Approved &&
						act.ApprovalStatus != ActivityStatusListAttribute.Canceled)
					{
						e.Cancel = true;

						Activities.Cache.RaiseExceptionHandling<EPTimecardDetail.timeSpent>(act, act.TimeSpent,
																							new PXSetPropertyException(
																								Messages
																									.ActivityIsNotApproved,
																								PXErrorLevel.RowError));

						throw new PXException(Messages.OneOrMoreActivitiesAreNotApproved);
					}
				}
			}
		}


		protected virtual void EPTimeCardSummaryWithInfo_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			EPTimeCardSummaryWithInfo row = e.Row as EPTimeCardSummaryWithInfo;
			if (row == null) return;

			if (dontSyncDetails) return;

			string key = GetSummaryKey(row);
			SummaryRecord rec;
			SummaryRecord.SummaryRecordInfo info = new SummaryRecord.SummaryRecordInfo();
			if (sumList.TryGetValue(row.LineNbr.ToString(), out rec))
			{
				info = rec.GetInfo();
			}
			else if (sumList.TryGetValue(key, out rec))
			{
				info = rec.GetInfo();
			}

			bool isEnabled = true;

			if (row.ProjectID != null)
			{
				isEnabled = !info.HasManualDetails;
			}

			PXUIFieldAttribute.SetEnabled<EPTimeCardSummary.earningType>(sender, row, isEnabled);
			PXUIFieldAttribute.SetEnabled<EPTimeCardSummary.parentNoteID>(sender, row, isEnabled);
			PXUIFieldAttribute.SetEnabled<EPTimeCardSummary.projectID>(sender, row, isEnabled);
			PXUIFieldAttribute.SetEnabled<EPTimeCardSummary.projectTaskID>(sender, row, isEnabled);
			PXUIFieldAttribute.SetEnabled<EPTimeCardSummary.isBillable>(sender, row, isEnabled);

			if (info.ApprovalRequired)
			{
				if (info.HasCompleted || info.HasOpen)
					row.ApprovalStatus = ApprovalStatusListAttribute.PendingApproval;
				else if (info.HasApproved && info.HasRejected)
					row.ApprovalStatus = ApprovalStatusListAttribute.PartiallyApprove;
				else if (info.HasApproved)
					row.ApprovalStatus = ApprovalStatusListAttribute.Approved;
				else if (info.HasRejected)
					row.ApprovalStatus = ApprovalStatusListAttribute.Rejected;
				else
					row.ApprovalStatus = ApprovalStatusListAttribute.NotRequired;
			}
			else
				row.ApprovalStatus = ApprovalStatusListAttribute.NotRequired;

			if (info.Rate != null)
				row.EmployeeRate = info.Rate;

			PXUIFieldAttribute.SetWarning<EPTimeCardSummary.employeeRate>(sender, e.Row, null);
			if (info.ContainsMixedRates)
			{
				PXUIFieldAttribute.SetWarning<EPTimeCardSummary.employeeRate>(sender, e.Row, Messages.ContainsMixedRates);
			}
		}

		bool skipProjectDefaultingFromEarningType = false;
		protected virtual void EPTimeCardSummaryWithInfo_RowInserting(PXCache sender, PXRowInsertingEventArgs e)
		{
			EPTimeCardSummaryWithInfo row = e.Row as EPTimeCardSummaryWithInfo;
			if (row == null) return;

			EPEarningType earningType = PXSelect<EPEarningType, Where<EPEarningType.typeCD, Equal<Required<EPEarningType.typeCD>>>>.Select(this, row.EarningType);
			if (earningType != null && !skipProjectDefaultingFromEarningType)
			{
                if (!IsImportFromExcel)
                {
                    row.IsBillable = earningType.isBillable == true;
                }
                row.EarningType = earningType.TypeCD;
			
				if (earningType.ProjectID != null && (row.ProjectID == null || row.ProjectID == ProjectDefaultAttribute.NonProject()) &&
					PXSelectorAttribute.Select(sender, e.Row, sender.GetField(typeof(EPTimeCardSummary.projectID)), earningType.ProjectID) != null //available in selector
					)
				{
					row.ProjectID = earningType.ProjectID;
				}

				if (earningType.TaskID != null && row.ProjectTaskID == null && !ProjectDefaultAttribute.IsNonProject( row.ProjectID))
				{
					PMTask defTask = PXSelect<PMTask, Where<PMTask.projectID, Equal<Required<PMTask.projectID>>,
								And<PMTask.taskID, Equal<Required<PMTask.taskID>>>>>.Select(this, row.ProjectID, earningType.TaskID);
					if (defTask != null && defTask.VisibleInTA == true)
					{
						row.ProjectTaskID = earningType.TaskID;
					}
				}
			}

			if (row.ProjectID == null && e.ExternalCall)
			{
				EPTimeCardSummaryWithInfo previousRecord = PXSelect<EPTimeCardSummaryWithInfo,
					Where<EPTimeCardSummaryWithInfo.lineNbr, NotEqual<Current<EPTimeCardSummaryWithInfo.lineNbr>>,
						And<EPTimeCardSummaryWithInfo.timeCardCD, Equal<Current<EPTimeCardSummaryWithInfo.timeCardCD>>>>, OrderBy<Desc<EPTimeCardSummaryWithInfo.lineNbr>>>.SelectSingleBound(this, new object[] { row });

				if (previousRecord != null && !string.Equals(previousRecord.EarningType, row.EarningType, StringComparison.InvariantCultureIgnoreCase) && previousRecord.ProjectID != null)
				{
					row.ProjectID = previousRecord.ProjectID;
					row.ProjectTaskID = previousRecord.ProjectTaskID;
				}
			}

			if (row.ProjectID == null && earningType != null && earningType.TypeCD == EpSetup.Current.HolidaysType)
			{
				row.ProjectID = PM.ProjectDefaultAttribute.NonProject();
			}
		}

		protected virtual void EPTimeCardSummaryWithInfo_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			//Debug.Print("EPTimeCardSummary_RowInserted Start");
			Debug.Indent();
			try
			{
				EPTimeCardSummaryWithInfo row = e.Row as EPTimeCardSummaryWithInfo;
				if (row == null) return;
				if (dontSyncDetails) return;

				UpdateAdjustingActivities(row);
			}
			finally
			{
				Debug.Unindent();
				//Debug.Print("EPTimeCardSummary_RowInserted End");
			}
		}

		protected virtual void EPTimeCardSummaryWithInfo_RowUpdating(PXCache sender, PXRowUpdatingEventArgs e)
		{
			EPTimeCardSummaryWithInfo row = e.NewRow as EPTimeCardSummaryWithInfo;
			EPTimeCardSummaryWithInfo oldRow = e.Row as EPTimeCardSummaryWithInfo;

			EPEarningType earningType = PXSelect<EPEarningType, Where<EPEarningType.typeCD, Equal<Required<EPEarningType.typeCD>>>>.Select(this, row.EarningType);
			if (earningType != null)
			{
                if (row.IsBillable == null || row.EarningType != oldRow.EarningType)
                    row.IsBillable = earningType.isBillable == true;

				if (earningType.ProjectID != null)
				{
					if ((row.ProjectID == null || row.EarningType != oldRow.EarningType) &&
						PXSelectorAttribute.Select(sender, e.Row, sender.GetField(typeof(EPTimeCardSummary.projectID)), earningType.ProjectID) != null //available in selector
						)
					{
						row.ProjectID = earningType.ProjectID;
						if (ProjectDefaultAttribute.IsNonProject(row.ProjectID))
						{
							row.ProjectTaskID = null;
						}
					}
				}

				if (earningType.TaskID != null)
				{
					if (row.ProjectTaskID == null || row.EarningType != oldRow.EarningType)
					{
						if (row.ProjectID == earningType.ProjectID)
						{
							PMTask defTask = PXSelect<PMTask, Where<PMTask.projectID, Equal<Required<PMTask.projectID>>,
								And<PMTask.taskID, Equal<Required<PMTask.taskID>>>>>.Select(this, row.ProjectID, earningType.TaskID);
							if (defTask != null && defTask.VisibleInTA == true)
							{
								row.ProjectTaskID = earningType.TaskID;
							}
						}
					}
				}
			}
		}

		protected virtual void EPTimeCardSummaryWithInfo_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			//Debug.Print("EPTimeCardSummary_RowUpdated Start");
			Debug.Indent();
			try
			{
				EPTimeCardSummaryWithInfo row = e.Row as EPTimeCardSummaryWithInfo;
				EPTimeCardSummaryWithInfo oldRow = e.OldRow as EPTimeCardSummaryWithInfo;

				if (row == null) return;

				//Debug.Print("Old: {0}", oldRow);
				//Debug.Print("New: {0}", row);

				if (dontSyncDetails) return;

				if (oldRow.ProjectID != row.ProjectID || oldRow.ProjectTaskID != row.ProjectTaskID ||
					oldRow.IsBillable != row.IsBillable || oldRow.ParentNoteID != row.ParentNoteID ||
					oldRow.EarningType != row.EarningType || oldRow.JobID != row.JobID || oldRow.ShiftID != row.ShiftID || oldRow.CostCodeID != row.CostCodeID
					|| oldRow.UnionID != row.UnionID || oldRow.LabourItemID != row.LabourItemID || oldRow.WorkCodeID != row.WorkCodeID)
				{
					
					//It is assumed that this fields can be changed only for the auto activities. If atleast one manual activity is present these fields are not editable.
					//hence here we will just delete the existing auto details and recreate new one:

					List<EPTimecardDetail> list = GetDetails(oldRow, Document.Current, false);

					try
					{
						dontSyncSummary = true;
						foreach (EPTimecardDetail item in list)
						{
							if (item.SummaryLineNbr == row.LineNbr)
								Activities.Delete(item);
						}
					}
					finally
					{
						dontSyncSummary = false;
					}
				}

				UpdateAdjustingActivities(row, oldRow.Description == row.Description);
			}
			finally
			{
				Debug.Unindent();
				//Debug.Print("EPTimeCardSummary_RowUpdated End");
			}
		}

		protected virtual void EPTimeCardSummaryWithInfo_RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
		{
			//Debug.Print("EPTimeCardSummary_RowDeleting Start");
			Debug.Indent();
			try
			{
				EPTimeCardSummaryWithInfo row = e.Row as EPTimeCardSummaryWithInfo;
				if (row == null) return;

				if (dontSyncDetails)
					return;

				if (row.Mon == null && row.Tue == null && row.Wed == null && row.Thu == null && row.Fri == null &&
					row.Sat == null && row.Sun == null)
					return; //Grid on pressing <Enter> at the last field automatically insert new row then delets it. Allow to delete such a row without checking. 

				if (GetDetails(row, Document.Current, true).Count > 0)//exists a detail record
				{
					if (FindDuplicates(row).Count == 0)//this is the last row that the detail record is referencing
					{
						e.Cancel = true;
						throw new PXException(Messages.RecordCannotDeleted);
					}
				}

			}
			finally
			{
				Debug.Unindent();
				//Debug.Print("EPTimeCardSummary_RowDeleting End");
			}
		}

		protected virtual void EPTimeCardSummaryWithInfo_RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
			//Debug.Print("EPTimeCardSummary_RowDeleted Start");
			Debug.Indent();
			try
			{
				EPTimeCardSummaryWithInfo row = e.Row as EPTimeCardSummaryWithInfo;
				if (row == null) return;
				if (dontSyncDetails) return;

				UpdateAdjustingActivities(row);
			}
			finally
			{
				Debug.Unindent();
				//Debug.Print("EPTimeCardSummary_RowDeleted End");
			}
		}

		protected virtual void EPTimeCardSummaryWithInfo_ProjectID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			EPTimeCardSummaryWithInfo row = e.Row as EPTimeCardSummaryWithInfo;
			if (row == null) return;

			PMTask currentTask = PXSelect<PMTask, Where<PMTask.taskID, Equal<Required<PMTask.taskID>>>>.Select(this, row.ProjectTaskID);
			if (currentTask != null)
			{
				PMTask newTask = PXSelect<PMTask, Where<PMTask.projectID, Equal<Required<PMTask.projectID>>, And<PMTask.taskCD, Equal<Required<PMTask.taskCD>>>>>.Select(this, row.ProjectID, currentTask.TaskCD);
				if (newTask != null)
				{
					row.ProjectTaskID = newTask.TaskID;
				}
				else
				{
					row.ProjectTaskID = null;
				}
			}

			sender.SetDefaultExt<EPTimeCardSummary.unionID>(e.Row);
			sender.SetDefaultExt<EPTimeCardSummary.certifiedJob>(e.Row);
			sender.SetDefaultExt<EPTimeCardSummary.labourItemID>(e.Row);
			sender.SetDefaultExt<EPTimeCardSummary.employeeRate>(e.Row);
		}

		protected virtual void _(Events.FieldUpdated<TimeCardMaint.EPTimeCardSummaryWithInfo, TimeCardMaint.EPTimeCardSummaryWithInfo.projectID> e)
		{
			if (e.Row != null)
			{
				PMProject project = null;

				if (e.Row.ProjectID != null)
					project = (PMProject)PXSelectorAttribute.Select<EPTimeCardSummaryWithInfo.projectID>(e.Cache, e.Row, e.Row.ProjectID);

				if (project != null)
					e.Row.ProjectDescription = project.Description;
				else
					e.Row.ProjectDescription = null;
			}
		}

		protected virtual void _(Events.FieldUpdated<TimeCardMaint.EPTimeCardSummaryWithInfo, TimeCardMaint.EPTimeCardSummaryWithInfo.projectTaskID> e)
		{
			if (e.Row != null)
			{
				PMTask task = null;

				if (e.Row.ProjectTaskID != null)
					task = PXSelect<PMTask>.Search<PMTask.taskID>(this, e.Row.ProjectTaskID);

				if (task != null)
					e.Row.ProjectTaskDescription = task.Description;
				else
					e.Row.ProjectTaskDescription = null;

				e.Cache.SetDefaultExt<EPTimeCardSummaryWithInfo.employeeRate>(e.Row);
			}
		}

		protected bool skipValidation = false;

		protected virtual void EPTimeCardSummaryWithInfo_ProjectID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			EPTimeCardSummaryWithInfo row = (EPTimeCardSummaryWithInfo)e.Row;
			if (row == null) return;

			if (skipValidation)
				return;

			if (e.NewValue != null && e.NewValue is int)
			{
				PMProject project = (PMProject)PXSelectorAttribute.Select<EPTimeCardSummaryWithInfo.projectID>(sender, e.Row, e.NewValue);
				//PMProject project = PXSelect<PMProject>.Search<PMProject.contractID>(sender.Graph, e.NewValue);
				if (project != null)
				{
					if (project.IsCompleted == true)
					{
						var ex = new PXSetPropertyException(PM.Messages.ProjectIsCompleted, PXErrorLevel.Error);
						ex.ErrorValue = project.ContractCD;

						sender.RaiseExceptionHandling<EPTimeCardSummary.projectID>(row, e.NewValue, ex);
					}

					if (project.IsCancelled == true)
					{
						var ex = new PXSetPropertyException(PM.Messages.ProjectIsCanceled, PXErrorLevel.Error);
						ex.ErrorValue = project.ContractCD;

						sender.RaiseExceptionHandling<EPTimeCardSummary.projectID>(row, e.NewValue, ex);
					}
				}
			}
		}
		protected virtual void EPTimeCardSummaryWithInfo_ProjectTaskID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			EPTimeCardSummaryWithInfo row = (EPTimeCardSummaryWithInfo)e.Row;
			if (row == null) return;

			if (skipValidation)
				return;

			if (e.NewValue != null && e.NewValue is int)
			{
				PMTask task = PXSelect<PMTask>.Search<PMTask.taskID>(sender.Graph, e.NewValue);
				if (task != null)
				{
					if (task.IsCompleted == true)
					{
						var ex = new PXSetPropertyException(PM.Messages.ProjectTaskIsCompleted, PXErrorLevel.Error);
						ex.ErrorValue = task.TaskCD;

						sender.RaiseExceptionHandling<EPTimeCardSummary.projectTaskID>(row, e.NewValue, ex);
					}

					if (task.IsCancelled == true)
					{
						var ex = new PXSetPropertyException(PM.Messages.ProjectTaskIsCanceled, PXErrorLevel.Error);
						ex.ErrorValue = task.TaskCD;

						sender.RaiseExceptionHandling<EPTimeCardSummary.projectTaskID>(row, e.NewValue, ex);
					}
				}
			}
		}
		
		protected virtual void EPTimecardDetail_RowSelecting(PXCache sender, PXRowSelectingEventArgs e)
		{
			EPTimecardDetail row = e.Row as EPTimecardDetail;
			if (row != null)
			{
				using (new PXConnectionScope())
				{
					InitEarningType(row);
				}

				RecalculateFields(row);

				if (row.Date != null)
					row.Day = ((int)row.Date.Value.DayOfWeek).ToString();
			}
		}

		protected virtual void EPTimecardDetail_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			EPTimecardDetail row = e.Row as EPTimecardDetail;
			if (row != null)
			{
				if (row.Released == true)
					PXUIFieldAttribute.SetEnabled(sender, e.Row, false);
				else
				{
					PXUIFieldAttribute.SetEnabled<EPTimecardDetail.billableTimeCalc>(sender, e.Row, row.IsOvertimeCalc != true && row.IsBillable == true);
					PXUIFieldAttribute.SetEnabled<EPTimecardDetail.billableOvertimeCalc>(sender, e.Row, row.IsOvertimeCalc == true && row.IsBillable == true);
				}

				if (row.ApprovalStatus == ActivityStatusAttribute.Open && row.ApproverID != null)
				{
					PXUIFieldAttribute.SetWarning<EPTimecardDetail.approvalStatus>(sender, row, Messages.ActivityIsOpenAndCannotbeApproved);
				}

				ValidateProjectAndProjectTask(row);
			}
		}

		protected virtual void EPTimecardDetail_RowInserting(PXCache sender, PXRowInsertingEventArgs e)
		{
			EPTimecardDetail row = e.Row as EPTimecardDetail;
			if (row == null) return;

			if (row.OrigNoteID != null) return;//inserting correction transaction - do not init just insert it as is.

			EPTimeCard doc = Document.Current;

			if (doc == null || doc.WeekID == null || doc.EmployeeID == null)
			{
				e.Cancel = true;
				return;
			}


			EPEarningType earningType = InitEarningType(row);
			if (earningType != null)
			{
				row.EarningTypeID = earningType.TypeCD;
                if (!isCreateCorrectionFlag)
                    row.IsBillable = earningType.isBillable == true;
            }
			

			RecalculateFields(row);

			PXSelectReadonly<EPEmployee,
					Where<EPEmployee.bAccountID, Equal<Required<EPEmployee.bAccountID>>>>.Clear(this);

			EPEmployee employee = PXSelectReadonly<EPEmployee,
					Where<EPEmployee.bAccountID, Equal<Required<EPEmployee.bAccountID>>>>.
					Select(this, doc.EmployeeID);
			row.OwnerID = employee.With(_ => _.UserID);

			row.Billed = false;
			row.TrackTime = true;

			if (earningType != null && earningType.ProjectID != null && row.ProjectID == null &&
					PXSelectorAttribute.Select(sender, e.Row, sender.GetField(typeof(EPTimecardDetail.projectID)), earningType.ProjectID) != null //available in selector
					)
			{
				row.ProjectID = earningType.ProjectID;
			}

			if (earningType != null && earningType.TaskID != null && row.ProjectTaskID == null && !ProjectDefaultAttribute.IsNonProject( row.ProjectID))
			{
				PMTask defTask = PXSelect<PMTask, Where<PMTask.projectID, Equal<Required<PMTask.projectID>>,
								And<PMTask.taskID, Equal<Required<PMTask.taskID>>>>>.Select(this, row.ProjectID, earningType.TaskID);
				if (defTask != null && defTask.VisibleInTA == true)
				{
					row.ProjectTaskID = earningType.TaskID;
				}
			}

			if (row.IsBillable == true)
				row.TimeBillable = row.TimeSpent;


			row.ApprovalStatus = ActivityStatusListAttribute.Completed;

			if (row.Date == null)
			{
				row.Date = CRActivityMaint.GetNextActivityStartDate<EPTimecardDetail>(this, Activities.Select(), row, doc.WeekID, doc.WeekID, TempData.Cache, typeof(EP.CRActivityMaint.EPTempData.lastEnteredDate));
			}

			row.WeekID = doc.WeekID;

			row.Day = ((int)row.Date.Value.DayOfWeek).ToString();

			//autodefault to non-project if project module is off:
			if (row.ProjectID == null && !ProjectAttribute.IsPMVisible( BatchModule.TA))
				row.ProjectID = ProjectDefaultAttribute.NonProject();
		}

		protected virtual void EPTimecardDetail_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			EPTimecardDetail row = e.Row as EPTimecardDetail;
			if (row == null) return;

			EmployeeActivitiesEntry.UpdateReportedInTimeZoneIDIfNeeded(sender, row, null, row.Date);

			if (dontSyncSummary) return;

			EPTimeCardSummaryWithInfo summary = GetSummaryRecord(row);
			AddToSummary(summary, row);
		}

		protected virtual void EPTimecardDetail_RowUpdating(PXCache sender, PXRowUpdatingEventArgs e)
		{
			EPTimecardDetail row = e.NewRow as EPTimecardDetail;
			EPTimecardDetail oldRow = e.Row as EPTimecardDetail;
			if (row == null || row.Billed == true) return;

			EPTimeCard doc = Document.Current;
			if (doc == null || doc.WeekID == null)
			{
				e.Cancel = true;
				return;
			}

			if (!dontSyncSummary)
			{
				EPEarningType earningType = InitEarningType(row);
				row.EarningTypeID = earningType.TypeCD;
               
				if (row.IsBillable == null || row.EarningTypeID != oldRow.EarningTypeID)
					row.IsBillable = earningType.isBillable == true;

                if (earningType.ProjectID != null)
				{
					if ((row.ProjectID == null || row.EarningTypeID != oldRow.EarningTypeID) &&
						PXSelectorAttribute.Select(sender, e.Row, sender.GetField(typeof(EPTimecardDetail.projectID)), earningType.ProjectID) != null //available in selector
						)
					{
						row.ProjectID = earningType.ProjectID;
						if(ProjectDefaultAttribute.IsNonProject(row.ProjectID))
						{
							row.ProjectTaskID = null;
						}
					}
				}

				if (earningType.TaskID != null)
				{
					if (row.ProjectTaskID == null || row.EarningTypeID != oldRow.EarningTypeID)
					{
						if (row.ProjectID == earningType.ProjectID)
						{
							PMTask defTask = PXSelect<PMTask, Where<PMTask.projectID, Equal<Required<PMTask.projectID>>,
								And<PMTask.taskID, Equal<Required<PMTask.taskID>>>>>.Select(this, row.ProjectID, earningType.TaskID);
							if (defTask != null && defTask.VisibleInTA == true)
							{
								row.ProjectTaskID = earningType.TaskID;
							}
						}
					}
				}
			}

			if (row.ProjectID == null && row.TimeSpent.GetValueOrDefault() != 0)
				sender.RaiseExceptionHandling<EPTimecardDetail.projectID>(row, null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, typeof(EPTimecardDetail.projectID).Name));

			if (row.Date == null || PXWeekSelector2Attribute.GetWeekID(this, (DateTime)row.Date) != doc.WeekID)
			{
				row.Date = PXWeekSelector2Attribute.GetWeekStartDate(this, (int)doc.WeekID);
			}

			row.WeekID = doc.WeekID;
		}

		protected virtual void EPTimecardDetail_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			EPTimecardDetail row = e.Row as EPTimecardDetail;
			EPTimecardDetail oldRow = e.OldRow as EPTimecardDetail;
			if (row == null) return;

			EmployeeActivitiesEntry.UpdateReportedInTimeZoneIDIfNeeded(sender, row, oldRow.Date, row.Date);

			if (row.SummaryLineNbr != null)
			{
				EPTimeCardSummaryWithInfo summary = GetSummaryRecord(row);
				if (summary != null && (summary.IsBillable.GetValueOrDefault() != row.IsBillable.GetValueOrDefault() ||
					summary.EarningType != row.EarningTypeID ||
					summary.ProjectID != row.ProjectID ||
					summary.ProjectTaskID != row.ProjectTaskID ||
					summary.ParentNoteID != row.ParentTaskNoteID ||
					summary.JobID != row.JobID ||
					summary.ShiftID != row.ShiftID ||
					summary.CostCodeID != row.CostCodeID ||
					summary.UnionID != row.UnionID ||
					summary.LabourItemID != row.LabourItemID ||
					summary.WorkCodeID != row.WorkCodeID ||
					summary.CertifiedJob != row.CertifiedJob))
				{
					row.SummaryLineNbr = null;//summary and line are no longer linked with each other.
				}
			}

			if (row.EarningTypeID != oldRow.EarningTypeID)
				InitEarningType(row);

			if (row.Date.GetValueOrDefault() != oldRow.Date.GetValueOrDefault()
				|| row.EarningTypeID != oldRow.EarningTypeID
				|| row.ProjectID.GetValueOrDefault() != oldRow.ProjectID.GetValueOrDefault()
				|| row.ProjectTaskID.GetValueOrDefault() != oldRow.ProjectTaskID.GetValueOrDefault()
				|| row.CostCodeID != oldRow.CostCodeID
				|| row.UnionID != oldRow.UnionID
				|| row.LabourItemID != oldRow.LabourItemID
				|| row.CertifiedJob.GetValueOrDefault() != oldRow.CertifiedJob.GetValueOrDefault())
			{
				var cost = CostEngine.CalculateEmployeeCost(null, row.EarningTypeID, row.LabourItemID, row.ProjectID, row.ProjectTaskID, row.CertifiedJob, row.UnionID, Document.Current.EmployeeID, row.Date.Value);
				row.EmployeeRate = cost?.Rate;
				row.OvertimeMultiplierCalc = cost?.OvertimeMultiplier;
			}

			if (row.EarningTypeID != oldRow.EarningTypeID || row.TimeSpent.GetValueOrDefault() != oldRow.TimeSpent.GetValueOrDefault() ||
				row.IsBillable.GetValueOrDefault() != oldRow.IsBillable.GetValueOrDefault())
			{
				RecalculateFields(row);
			}

			if (row.Date.GetValueOrDefault() != oldRow.Date.GetValueOrDefault()
				|| row.EarningTypeID != oldRow.EarningTypeID
				|| row.ProjectID.GetValueOrDefault() != oldRow.ProjectID.GetValueOrDefault()
				|| row.ProjectTaskID.GetValueOrDefault() != oldRow.ProjectTaskID.GetValueOrDefault()
				|| row.TimeSpent.GetValueOrDefault() != oldRow.TimeSpent.GetValueOrDefault()
				|| row.IsBillable.GetValueOrDefault() != oldRow.IsBillable.GetValueOrDefault()
				|| row.TimeBillable.GetValueOrDefault() != oldRow.TimeBillable.GetValueOrDefault()
				|| row.JobID != oldRow.JobID
				|| row.CostCodeID != oldRow.CostCodeID
				|| row.UnionID != oldRow.UnionID
				|| row.LabourItemID != oldRow.LabourItemID
				|| row.WorkCodeID != oldRow.WorkCodeID
				|| row.CertifiedJob.GetValueOrDefault() != oldRow.CertifiedJob.GetValueOrDefault())
			{
				if (row.ApproverID != null)
				{
					row.ApprovalStatus = ActivityStatusListAttribute.PendingApproval;
				}
			}

			if (!dontSyncSummary)
			{
				EPTimeCardSummaryWithInfo summary = GetSummaryRecord(row);
				AddToSummary(summary, row);

				EPTimeCardSummaryWithInfo summaryOld = GetSummaryRecord(oldRow);
				SubtractFromSummary(summaryOld, oldRow);
			}
		}

		protected virtual void EPTimecardDetail_RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
		{
			//Debug.Print("EPTimecardDetail_RowDeleting Start");
			Debug.Indent();
			try
			{
				EPTimecardDetail row = e.Row as EPTimecardDetail;

				if (row != null)
				{
					if (row.Released == true)
					{
						throw new PXException(Messages.ActivityIsReleased);
					}
					if (Document.Current != null && !string.IsNullOrEmpty(Document.Current.OrigTimeCardCD) && ((EPTimecardDetail)e.Row).OrigNoteID != null
						&& Document.Cache.GetStatus(Document.Current) != PXEntryStatus.Deleted)
					{
						throw new PXException(Messages.CannotDeleteCorrectionActivity);
					}

					if (row.SummaryLineNbr == null)
					{
						if (!dontSyncSummary)
						{
							EPTimeCardSummaryWithInfo summary = GetSummaryRecord(row);
							SubtractFromSummary(summary, row);
						}
					}

				}
			}
			finally
			{
				Debug.Unindent();
				//Debug.Print("EPTimecardDetail_RowDeleting End");
			}
		}

		protected virtual void EPTimecardDetail_RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
			//Debug.Print("EPTimecardDetail_RowDeleted Start");
			Debug.Indent();
			try
			{
				EPTimecardDetail row = e.Row as EPTimecardDetail;

				if (row != null && row.SummaryLineNbr != null)//adjust activity is deleted
				{
					if (!dontSyncSummary)
					{
						EPTimeCardSummaryWithInfo summary = GetSummaryRecord(row);
						SubtractFromSummary(summary, row);
					}
				}
			}
			finally
			{
				Debug.Unindent();
				//Debug.Print("EPTimecardDetail_RowDeleted End");
			}
		}

		protected virtual void EPTimecardDetail_StartDate_FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			EPTimecardDetail row = e.Row as EPTimecardDetail;
			if (row == null) return;

			EPTimeCard doc = Document.Current;
			if (row.Date == null && doc != null && doc.WeekID != null && doc.EmployeeID != null)
			{
				DateTime? date = CRActivityMaint.GetNextActivityStartDate<EPTimecardDetail>(this, Activities.Select(), row, doc.WeekID, doc.WeekID, TempData.Cache, typeof(EP.CRActivityMaint.EPTempData.lastEnteredDate));

				sender.SetValueExt<EPTimecardDetail.date>(row, date);
			}
		}

		protected virtual void _(Events.FieldUpdated<EPTimecardDetail, EPTimecardDetail.date> e)
		{
			EmployeeActivitiesEntry.UpdateReportedInTimeZoneIDIfNeeded(e.Cache, e.Row, (DateTime?) e.OldValue, (DateTime?) e.NewValue);
		}

		private static string LocalizeDayOfWeek(DayOfWeek dayOfWeek)
		{
			string dayName;
			switch (dayOfWeek)
			{
				case DayOfWeek.Monday: dayName = Messages.Monday; break;
				case DayOfWeek.Tuesday: dayName = Messages.Tuesday; break;
				case DayOfWeek.Wednesday: dayName = Messages.Wednesday; break;
				case DayOfWeek.Thursday: dayName = Messages.Thursday; break;
				case DayOfWeek.Friday: dayName = Messages.Friday; break;
				case DayOfWeek.Saturday: dayName = Messages.Saturday; break;
				default: dayName = Messages.Sunday; break;
			}
			return PXMessages.LocalizeNoPrefix(dayName);
		}

		protected virtual void EPTimecardDetail_Day_FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			if (Document.Current == null || Document.Current.WeekID == null) return;
			PXWeekSelector2Attribute.WeekInfo weekInfo = PXWeekSelector2Attribute.GetWeekInfo(this, Document.Current.WeekID.Value);

			List<string> allowedValues = new List<string>();
			List<string> allowedLabels = new List<string>();

			if (weekInfo.Mon.Enabled)
			{
				allowedValues.Add("1");
                allowedLabels.Add(LocalizeDayOfWeek(DayOfWeek.Monday));
			}

			if (weekInfo.Tue.Enabled)
			{
				allowedValues.Add("2");
                allowedLabels.Add(LocalizeDayOfWeek(DayOfWeek.Tuesday));
			}

			if (weekInfo.Wed.Enabled)
			{
				allowedValues.Add("3");
                allowedLabels.Add(LocalizeDayOfWeek(DayOfWeek.Wednesday));
			}

			if (weekInfo.Thu.Enabled)
			{
				allowedValues.Add("4");
                allowedLabels.Add(LocalizeDayOfWeek(DayOfWeek.Thursday));
			}

			if (weekInfo.Fri.Enabled)
			{
				allowedValues.Add("5");
                allowedLabels.Add(LocalizeDayOfWeek(DayOfWeek.Friday));
			}

			if (weekInfo.Sat.Enabled)
			{
				allowedValues.Add("6");
                allowedLabels.Add(LocalizeDayOfWeek(DayOfWeek.Saturday));
			}

			if (weekInfo.Sun.Enabled)
			{
				allowedValues.Add("0");
                allowedLabels.Add(LocalizeDayOfWeek(DayOfWeek.Sunday));
			}

			e.ReturnState = PXStringState.CreateInstance(e.ReturnState, 1, false, typeof(EPTimecardDetail.day).Name, false, 1, null,
													allowedValues.ToArray(), allowedLabels.ToArray(), true, allowedValues[0]);
		}

		protected virtual void EPTimecardDetail_Day_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			EPTimecardDetail row = e.Row as EPTimecardDetail;
			if (row == null || e.ExternalCall != true) return;

			if (Document.Current != null)
			{
				PXWeekSelector2Attribute.WeekInfo weekInfo = PXWeekSelector2Attribute.GetWeekInfo(this, Document.Current.WeekID.Value);

				DayOfWeek val = (DayOfWeek)Enum.Parse(typeof(DayOfWeek), row.Day);

				switch (val)
				{
					case DayOfWeek.Monday:
						sender.SetValueExt<EPTimecardDetail.date>(row, CombineDateAndTime(weekInfo.Mon.Date, row.Date));
						break;
					case DayOfWeek.Tuesday:
						sender.SetValueExt<EPTimecardDetail.date>(row, CombineDateAndTime(weekInfo.Tue.Date, row.Date));
						break;
					case DayOfWeek.Wednesday:
						sender.SetValueExt<EPTimecardDetail.date>(row, CombineDateAndTime(weekInfo.Wed.Date, row.Date));
						break;
					case DayOfWeek.Thursday:
						sender.SetValueExt<EPTimecardDetail.date>(row, CombineDateAndTime(weekInfo.Thu.Date, row.Date));
						break;
					case DayOfWeek.Friday:
						sender.SetValueExt<EPTimecardDetail.date>(row, CombineDateAndTime(weekInfo.Fri.Date, row.Date));
						break;
					case DayOfWeek.Saturday:
						sender.SetValueExt<EPTimecardDetail.date>(row, CombineDateAndTime(weekInfo.Sat.Date, row.Date));
						break;
					case DayOfWeek.Sunday:
						sender.SetValueExt<EPTimecardDetail.date>(row, CombineDateAndTime(weekInfo.Sun.Date, row.Date));
						break;

				}
			}
		}

		private DateTime CombineDateAndTime(DateTime? datePart, DateTime? timePart)
		{
			return new DateTime(datePart.Value.Year, datePart.Value.Month, datePart.Value.Day, timePart.Value.Hour, timePart.Value.Minute, timePart.Value.Second);
		}

		protected virtual void EPTimecardDetail_StartDate_Date_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			EPTimecardDetail row = e.Row as EPTimecardDetail;
			if (row == null) return;

			int? weekId = row.WeekID ?? Document.Current.With(_ => _.WeekID);
			if (weekId == null) return;

			DateTime? newValue = null;
			DateTime valFromString;
			if (e.NewValue is string &&
				DateTime.TryParse((string)e.NewValue, sender.Graph.Culture, System.Globalization.DateTimeStyles.None, out valFromString))
			{
				newValue = valFromString;
			}
			if (e.NewValue is DateTime)
				newValue = (DateTime)e.NewValue;

			PXWeekSelector2Attribute.WeekInfo weekInfo = PXWeekSelector2Attribute.GetWeekInfo(this, Document.Current.WeekID.Value);
			if (newValue != null && !weekInfo.IsValid(((DateTime)newValue).Date))
			{
				throw new PXSetPropertyException(Messages.DateNotInWeek);
			}
		}

		protected virtual void EPTimecardDetail_OwnerID_FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			EPTimecardDetail row = e.Row as EPTimecardDetail;
			if (row == null) return;

			if (row.OwnerID == null)
			{
				EPEmployee employee = Document.Current.
					With(_ => PXSelectReadonly<EPEmployee,
					Where<EPEmployee.bAccountID, Equal<Required<EPEmployee.bAccountID>>>>.
					Select(this, _.EmployeeID));
				row.OwnerID = employee.With(_ => _.UserID);
			}
		}

		protected virtual void EPTimecardDetail_StartDate_Date_FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			var row = e.Row as EPTimecardDetail;
			if (row == null) return;

			DateTime? newValue = null;
			DateTime valFromString;
			if (e.NewValue is string &&
				DateTime.TryParse((string)e.NewValue, sender.Graph.Culture, System.Globalization.DateTimeStyles.None, out valFromString))
			{
				newValue = valFromString;
			}
			if (e.NewValue is DateTime)
				newValue = (DateTime)e.NewValue;

			sender.SetValuePending(e.Row, typeof(EPTimecardDetail.date).Name + "_oldValue", newValue);
		}

		protected virtual void EPTimecardDetail_StartDate_Time_FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			var row = e.Row as EPTimecardDetail;
			if (row == null) return;

			DateTime? newValue = null;
			DateTime valFromString;
			if (e.NewValue is string &&
				DateTime.TryParse((string)e.NewValue, sender.Graph.Culture, System.Globalization.DateTimeStyles.None, out valFromString))
			{
				newValue = valFromString;
			}
			if (e.NewValue is DateTime)
				newValue = (DateTime)e.NewValue;

			object valuePending = sender.GetValuePending(e.Row, typeof(EPTimecardDetail.date).Name + "_oldValue");
			if (valuePending != PXCache.NotSetValue) 
			{
				var oldValue = (DateTime?) valuePending;
				if (oldValue != null && newValue != null &&
					(int)oldValue.Value.TimeOfDay.TotalMinutes == (int)newValue.Value.TimeOfDay.TotalMinutes)
					{
						e.Cancel = true;
					}
			}
		}

		protected virtual void EPTimecardDetail_StartDate_Date_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			var row = e.Row as EPTimecardDetail;
			if (row == null) return;

			object valuePending = sender.GetValuePending(e.Row, typeof(EPTimecardDetail.date).Name + "_oldValue");
			if (valuePending != PXCache.NotSetValue)
			{
				var oldValue = (DateTime?) valuePending;
				var newValue = row.Date;
				if (newValue != null &&
					(oldValue == null ||
						oldValue != newValue && ((int)oldValue.Value.TimeOfDay.TotalMinutes == (int)newValue.Value.TimeOfDay.TotalMinutes)))
				{
					var calendarId = CRActivityMaint.GetCalendarID(this, row);
					if (!string.IsNullOrEmpty(calendarId))
					{
						DateTime start;
						DateTime end;
						CalendarHelper.CalculateStartEndTime(this, calendarId, newValue.Value.Date, out start, out end);
						sender.SetValueExt<EPTimecardDetail.date>(row, start);
					}
				}
			}

			row.Day = ((int)row.Date.Value.DayOfWeek).ToString();

			if (row != null && row.Date != null)
				TempData.Cache.SetValue<CRActivityMaint.EPTempData.lastEnteredDate>(TempData.Current, row.Date);
		}

		protected virtual void EPTimecardDetail_BillableTimeCalc_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			EPTimecardDetail row = e.Row as EPTimecardDetail;
			if (row == null) return;
			int? newBillableTime = (int?)e.NewValue;

			if (row.TimeSpent < newBillableTime)
			{
				throw new PXSetPropertyException(CR.Messages.BillableTimeCannotBeGreaterThanTimeSpent);
			}
		}

		protected virtual void EPTimecardDetail_UIStatus_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			e.Cancel = true;
		}

		protected virtual void EPTimecardDetail_BillableTimeCalc_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			EPTimecardDetail row = e.Row as EPTimecardDetail;
			if (row == null) return;
			row.TimeBillable = row.BillableTimeCalc;

		}

		protected virtual void EPTimecardDetail_BillableOvertimeCalc_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			EPTimecardDetail row = e.Row as EPTimecardDetail;
			if (row == null) return;
			int? newBillableTime = (int?)e.NewValue;

			if (row.TimeSpent < newBillableTime)
			{
				throw new PXSetPropertyException(CR.Messages.OvertimeBillableCannotBeGreaterThanOvertimeSpent);
			}
		}

		protected virtual void EPTimecardDetail_BillableOvertimeCalc_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			EPTimecardDetail row = e.Row as EPTimecardDetail;
			if (row == null) return;
			row.TimeBillable = row.BillableOvertimeCalc;

		}

		protected virtual void EPTimecardDetail_IsBillable_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			EPTimecardDetail row = e.Row as EPTimecardDetail;
			if (row == null) return;

			if (row.IsBillable == true && row.TimeBillable.GetValueOrDefault() == 0)
			{
				row.TimeBillable = row.TimeSpent;
			}
		}

		protected virtual void EPTimeCardItem_InventoryID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			sender.SetDefaultExt<EPTimeCardItem.uOM>(e.Row);
		}

		protected virtual void EPApproval_Details_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (this.Document.Current == null) return;

			e.NewValue = null;
			Type[] fields = new Type[]
			{
				typeof(EPTimeCard.weekDescription), typeof(EPTimeCard.timeSpentCalc), typeof(EPTimeCard.overtimeSpentCalc),typeof(EPTimeCard.timeBillableCalc), typeof(EPTimeCard.overtimeBillableCalc)
			};
			foreach (Type t in fields)
			{
				PXStringState strState = this.Document.Cache.GetValueExt(this.Document.Current, t.Name) as PXStringState;
				if (strState != null)
				{
					string value =
						strState.InputMask != null ? Mask.Format(strState.InputMask, strState) :
						strState.Value != null ? strState.Value.ToString() : null;

					if (!string.IsNullOrEmpty(value))
						e.NewValue += (e.NewValue != null ? ", " : string.Empty) + strState.DisplayName + "=" + value.Trim();
				}
			}
		}
						
		protected virtual void _(Events.FieldDefaulting<EPTimecardDetail, EPTimecardDetail.overtimeMultiplierCalc> e)
		{
			if (Document.Current != null)
			{
				if (e.Row.Date != null)
				{
					e.NewValue = CostEngine.CalculateEmployeeCost(null, e.Row.EarningTypeID, e.Row.LabourItemID, e.Row.ProjectID, e.Row.ProjectTaskID, e.Row.CertifiedJob, e.Row.UnionID, Document.Current.EmployeeID, e.Row.Date.Value)?.OvertimeMultiplier;
				}
			}
		}
		protected virtual void _(Events.FieldUpdated<EPTimecardDetail, EPTimecardDetail.costCodeID> e)
		{
			e.Cache.SetDefaultExt<EPTimecardDetail.workCodeID>(e.Row);
		}
		protected virtual void _(Events.FieldUpdated<EPTimecardDetail, EPTimecardDetail.projectID> e)
		{
			e.Cache.SetDefaultExt<EPTimecardDetail.unionID>(e.Row);
			e.Cache.SetDefaultExt<EPTimecardDetail.certifiedJob>(e.Row);
			e.Cache.SetDefaultExt<EPTimecardDetail.labourItemID>(e.Row);
			e.Cache.SetDefaultExt<EPTimecardDetail.employeeRate>(e.Row);
		}

		protected virtual void _(Events.FieldDefaulting<EPTimeCardSummaryWithInfo, EPTimeCardSummaryWithInfo.workCodeID> e)
		{
			if (e.Row.CostCodeID != null && e.Row.CostCodeID != CostCodeAttribute.GetDefaultCostCode())
			{
				PMCostCode costCode = PXSelect<PMCostCode, Where<PMCostCode.costCodeID, Equal<Required<PMCostCode.costCodeID>>>>.Select(this, e.Row.CostCodeID);
				if (costCode != null)
				{
					var select = new PXSelect<PMWorkCode, Where<PMWorkCode.costCodeFrom, LessEqual<Required<PMWorkCode.costCodeFrom>>,
						And<PMWorkCode.costCodeTo, GreaterEqual<Required<PMWorkCode.costCodeTo>>>>>(this);
					PMWorkCode code = select.Select(costCode.CostCodeCD, costCode.CostCodeCD);
					if (code != null)
					{
						e.NewValue = code.WorkCodeID;
					}
				}
			}
		}
		       
        protected virtual void _(Events.FieldUpdated<EPTimeCardSummaryWithInfo, EPTimeCardSummaryWithInfo.costCodeID> e)
		{
			e.Cache.SetDefaultExt<EPTimeCardSummaryWithInfo.workCodeID>(e.Row);
		}
		protected virtual void _(Events.FieldDefaulting<EPTimeCardSummaryWithInfo, EPTimeCardSummaryWithInfo.employeeRate> e)
		{
			if (Document.Current != null)
			{
				DateTime? weekStartDate = PXWeekSelector2Attribute.GetWeekStartDate(this, (int)Document.Current.WeekID);
				if (weekStartDate != null)
				{
					e.NewValue = CostEngine.CalculateEmployeeCost(null, e.Row.EarningType, e.Row.LabourItemID, e.Row.ProjectID, e.Row.ProjectTaskID, e.Row.CertifiedJob, e.Row.UnionID, Document.Current.EmployeeID, weekStartDate.Value)?.Rate;
				}
			}
		}
		protected virtual void _(Events.FieldUpdated<EPTimeCardSummaryWithInfo, EPTimeCardSummaryWithInfo.earningType> e)
		{
			if (!IsImportFromExcel)
				e.Cache.SetDefaultExt<EPTimeCardSummaryWithInfo.projectID>(e.Row);
			e.Cache.SetDefaultExt<EPTimeCardSummaryWithInfo.labourItemID>(e.Row);
			e.Cache.SetDefaultExt<EPTimeCardSummaryWithInfo.employeeRate>(e.Row);
		}
		
		protected virtual void _(Events.FieldUpdated<EPTimeCardSummaryWithInfo, EPTimeCardSummaryWithInfo.labourItemID> e)
		{
			e.Cache.SetDefaultExt<EPTimeCardSummaryWithInfo.employeeRate>(e.Row);
		}
		protected virtual void _(Events.FieldUpdated<EPTimeCardSummaryWithInfo, EPTimeCardSummaryWithInfo.certifiedJob> e)
		{
			e.Cache.SetDefaultExt<EPTimeCardSummaryWithInfo.employeeRate>(e.Row);
		}
		

		#endregion

		#region Protected Methods
		
		public virtual void ProcessRegularTimecard(PM.RegisterEntry releaseGraph, EPTimeCard timecard)
		{
			PXCache registerCache = releaseGraph.Document.Cache;
			PXCache tranCache = releaseGraph.Transactions.Cache;
			PM.PMRegister doc = (PM.PMRegister)registerCache.Insert();
			doc.OrigDocType = PMOrigDocType.Timecard;
			doc.OrigNoteID = timecard.NoteID;
			releaseGraph.FieldVerifying.AddHandler<PMTran.inventoryID>((PXCache sender, PXFieldVerifyingEventArgs e) => { e.Cancel = true; });//restriction should be applicable only for budgeting.

			EP.EPEmployee emp = PXSelect<EP.EPEmployee, Where<EP.EPEmployee.bAccountID, Equal<Required<EP.EPEmployee.bAccountID>>>>.Select(this, timecard.EmployeeID);
			if (emp != null)
			{
				doc.Description = string.Format("{0} - {1}", emp.AcctName, timecard.WeekID);
			}

			List<ReleasedActivity> released = new List<ReleasedActivity>();
			foreach (EPTimecardDetail act in Activities.Select())
			{
				if (act.Released == true)//activities can be released throught employee activity release screen (if TC is not required for an employee)
					continue;

				bool isBilled = false;

				if (act.ApprovalStatus == ActivityStatusListAttribute.PendingApproval)
				{ 
					throw new PXException(Messages.HasPendingOrRejectedActivityOnRelease);
				}

				if (act.ContractID != null)
				{
					//Contract without CRM mode
					PMTran contractUsageTran = EmployeeActivitiesRelease.CreateContractUsage(releaseGraph, act.ContractID, act, act.TimeBillable.GetValueOrDefault());
					if (contractUsageTran != null)
						isBilled = true;
				}
				else if (act.RefNoteID != null && PXSelectJoin<CRCase,
						InnerJoin<CRActivityLink,
							On<CRActivityLink.refNoteID, Equal<CRCase.noteID>>>,
						Where<CRActivityLink.noteID, Equal<Required<EPTimecardDetail.refNoteID>>>>.Select(releaseGraph, act.RefNoteID).Count == 1)
				{
					//Add Contract-Usage
					PMTran contractUsageTran = releaseGraph.CreateContractUsage(act, act.TimeBillable.GetValueOrDefault());
					if (contractUsageTran != null)
						isBilled = true;
				}

				var reportedDate = GetReportedDate(act);
				string postingOption = EPSetupMaint.GetPostingOption(this, EpSetup.Current, timecard.EmployeeID);
				var cost = CostEngine.CalculateEmployeeCost(act.TimeCardCD, act.EarningTypeID, act.LabourItemID, act.ProjectID, act.ProjectTaskID, act.CertifiedJob, act.UnionID, timecard.EmployeeID, reportedDate);
				if (cost == null && PXAccess.FeatureInstalled<FeaturesSet.projectAccounting>() && postingOption != EPPostOptions.DoNotPost && ProjectDefaultAttribute.IsProject(this, act.ProjectID))
				{
					throw new PXException(Messages.EmployeeCostRateNotFound);
				}

				if (cost != null)
				{
					act.EmployeeRate = cost.Rate;
				}

				if (postingOption != EPPostOptions.DoNotPost )
					releaseGraph.CreateTransaction(act, timecard.EmployeeID, act.Date.Value, act.TimeSpent, act.TimeBillable, cost?.Rate, cost?.OvertimeMultiplier);
				released.Add(new ReleasedActivity(act, act.EmployeeRate.GetValueOrDefault(), isBilled));
			}


			PXWeekSelector2Attribute.WeekInfo week = PXWeekSelector2Attribute.GetWeekInfo(this, timecard.WeekID.Value);

			foreach (EPTimeCardItem item in Items.View.SelectMultiBound(new object[] { timecard }))
			{
				#region Create Transactions By Days

				if (item.Sun.GetValueOrDefault() > 0)
				{
					CreateItemTransaction(item, timecard.EmployeeID, week.Sun.Date, item.Sun, tranCache);
				}

				if (item.Mon.GetValueOrDefault() > 0)
				{
					CreateItemTransaction(item, timecard.EmployeeID, week.Mon.Date, item.Mon, tranCache);
				}

				if (item.Tue.GetValueOrDefault() > 0)
				{
					CreateItemTransaction(item, timecard.EmployeeID, week.Tue.Date, item.Tue, tranCache);
				}

				if (item.Wed.GetValueOrDefault() > 0)
				{
					CreateItemTransaction(item, timecard.EmployeeID, week.Wed.Date, item.Wed, tranCache);
				}

				if (item.Thu.GetValueOrDefault() > 0)
				{
					CreateItemTransaction(item, timecard.EmployeeID, week.Thu.Date, item.Thu, tranCache);
				}

				if (item.Fri.GetValueOrDefault() > 0)
				{
					CreateItemTransaction(item, timecard.EmployeeID, week.Fri.Date, item.Fri, tranCache);
				}

				if (item.Sat.GetValueOrDefault() > 0)
				{
					CreateItemTransaction(item, timecard.EmployeeID, week.Sat.Date, item.Sat, tranCache);
				}

				#endregion
			}

			//Setting the released/billed status for all activities if and only if all the transaction were processed without an exception in the above loop.
			foreach (ReleasedActivity act in released)
			{
				Activities.Cache.SetValueExt<EPTimecardDetail.released>(act.Activity, true);
				Activities.Cache.SetValue<EPTimecardDetail.employeeRate>(act.Activity, act.Cost);
				if (act.IsBilled)
					Activities.Cache.SetValue<EPTimecardDetail.billed>(act.Activity, true);

				Activities.Cache.SetStatus(act.Activity, PXEntryStatus.Updated);
			}
		}

		protected DateTime GetReportedDate(PMTimeActivity activity)
		{
			var result = activity.Date.Value;
			PXTimeZoneInfo currentTimeZone = LocaleInfo.GetTimeZone();
			if (currentTimeZone != null && !string.IsNullOrEmpty(activity.ReportedInTimeZoneID) &&
				currentTimeZone.Id != activity.ReportedInTimeZoneID)
			{
				var utcDate = PXTimeZoneInfo.ConvertTimeToUtc(activity.Date.Value, currentTimeZone);
				PXTimeZoneInfo reportedTimeZone = PXTimeZoneInfo.FindSystemTimeZoneById(activity.ReportedInTimeZoneID);
				result = PXTimeZoneInfo.ConvertTimeFromUtc(utcDate, reportedTimeZone);
			}
			return result;
		}

		public virtual void ProcessCorrectingTimecard(PM.RegisterEntry releaseGraph, EPTimeCard timecard)
		{
			PXCache registerCache = releaseGraph.Document.Cache;
			PXCache tranCache = releaseGraph.Transactions.Cache;
			PM.PMRegister doc = (PM.PMRegister)registerCache.Insert();
			doc.OrigDocType = PMOrigDocType.Timecard;
			doc.OrigNoteID = timecard.NoteID;
			releaseGraph.FieldVerifying.AddHandler<PMTran.inventoryID>((PXCache sender, PXFieldVerifyingEventArgs e) => { e.Cancel = true; });//restriction should be applicable only for budgeting.

			EP.EPEmployee emp = PXSelect<EP.EPEmployee, Where<EP.EPEmployee.bAccountID, Equal<Required<EP.EPEmployee.bAccountID>>>>.Select(this, timecard.EmployeeID);
			if (emp != null)
			{
				doc.Description = PXMessages.LocalizeFormatNoPrefixNLA(Messages.Correction, emp.AcctName, timecard.WeekID);
			}

			List<ReleasedActivity> released = new List<ReleasedActivity>();
			string postingOption = EPSetupMaint.GetPostingOption(this, EpSetup.Current, timecard.EmployeeID);

			//process deleted activities:
			PXSelectBase<EPTimecardDetailOrig> selectDeletedActivities = new PXSelectJoin<
				EPTimecardDetailOrig,
			InnerJoin<CREmployee,
				On<CREmployee.userID, Equal<EPTimecardDetailOrig.ownerID>>,
			LeftJoin<EPTimecardDetailEx,
				On<EPTimecardDetailOrig.noteID, Equal<EPTimecardDetailEx.origNoteID>>>>,
					Where<CREmployee.bAccountID, Equal<Required<EPTimeCard.employeeID>>,
						And<EPTimecardDetailOrig.weekID, Equal<Required<EPTimeCard.weekId>>,
						And<EPTimecardDetailOrig.timeSheetCD, IsNull,
				And<EPTimecardDetailEx.noteID, IsNull,
				And<Where<EPTimecardDetailOrig.timeCardCD, IsNull,
					Or<EPTimecardDetailOrig.timeCardCD, Equal<Required<EPTimeCard.timeCardCD>>>>>>>>>,
					OrderBy<Asc<EPTimecardDetailOrig.date>>>(this);

			foreach (PXResult<EPTimecardDetailOrig, CREmployee, EPTimecardDetailEx> res in selectDeletedActivities.Select(timecard.EmployeeID, timecard.WeekID, timecard.OrigTimeCardCD))
			{
				EPTimecardDetailOrig orig = (EPTimecardDetailOrig)res;
				bool isBilled = false;
				var laborRate = CostEngine.CalculateEmployeeCost(orig.TimeCardCD, orig.EarningTypeID, orig.LabourItemID, orig.ProjectID, orig.ProjectTaskID, orig.CertifiedJob, orig.UnionID, timecard.EmployeeID, orig.Date.Value);
				if (laborRate == null && PXAccess.FeatureInstalled<FeaturesSet.projectAccounting>() && postingOption != EPPostOptions.DoNotPost && ProjectDefaultAttribute.IsProject(this, orig.ProjectID))
				{
					throw new PXException(Messages.EmployeeCostRateNotFound);
				}
				decimal? origCost = orig.EmployeeRate ?? laborRate?.Rate;

				if (postingOption != EPPostOptions.DoNotPost)
					releaseGraph.CreateTransaction(orig, timecard.EmployeeID, orig.Date.Value, -orig.TimeSpent, -orig.TimeBillable, origCost, laborRate?.OvertimeMultiplier);
				Activities.Cache.SetValueExt<EPTimecardDetail.released>(orig, true);
				Activities.Cache.SetValue<EPTimecardDetail.employeeRate>(orig, IN.PXDBPriceCostAttribute.Round((decimal)origCost));
				Activities.Cache.SetStatus(orig, PXEntryStatus.Updated);

				if (orig.RefNoteID != null)
				{
					CRCase crCase = PXSelectJoin<CRCase,
						InnerJoin<CRActivityLink,
							On<CRActivityLink.refNoteID, Equal<CRCase.noteID>>>,
						Where<CRActivityLink.noteID, Equal<Required<EPTimecardDetailOrig.refNoteID>>>>.Select(this, orig.RefNoteID);
					if (crCase != null && crCase.ContractID != null)
					{
						PMTran contractUsageTran = null;
						if (orig.IsBillable == true)
						{
							//Subtract Contract-Usage
							contractUsageTran = releaseGraph.CreateContractUsage(orig, -orig.TimeBillable.GetValueOrDefault());
						}

						if (contractUsageTran != null)
							isBilled = true;
					}
				}

				released.Add(new ReleasedActivity(orig, origCost.GetValueOrDefault(), isBilled));
			}

			//process new and modified activities:
			PXSelectBase<EPTimecardDetail> selectActivities = new PXSelectJoin<
				EPTimecardDetail,
			InnerJoin<CREmployee,
				On<CREmployee.userID, Equal<EPTimecardDetailOrig.ownerID>>,
			LeftJoin<EPTimecardDetailOrig,
				On<EPTimecardDetailOrig.noteID, Equal<EPTimecardDetail.origNoteID>>>>,
					Where<CREmployee.bAccountID, Equal<Required<EPTimeCard.employeeID>>,
						And<EPTimecardDetail.weekID, Equal<Required<EPTimeCard.weekId>>,
						And<EPTimecardDetail.timeSheetCD, IsNull,
				And<Where<EPTimecardDetail.timeCardCD, IsNull,
					Or<EPTimecardDetail.timeCardCD, Equal<Required<EPTimeCard.timeCardCD>>>>>>>>,
					OrderBy<Asc<EPTimecardDetail.date>>>(this);

			foreach (PXResult<EPTimecardDetail, CREmployee, EPTimecardDetailOrig> res in selectActivities.Select(timecard.EmployeeID, timecard.WeekID, timecard.TimeCardCD))
			{
				EPTimecardDetail act = (EPTimecardDetail)res;

				if (act.Released == true)//new added activities can be released throught employee activity release screen (if TC is not required for an employee)
					continue;
				bool isBilled = false;

				EPTimecardDetailOrig orig = (EPTimecardDetailOrig)res;
				Activities.Cache.RaiseRowSelected(act);

				decimal? origCost = null;
				var laborCost = CostEngine.CalculateEmployeeCost(act.TimeCardCD, act.EarningTypeID, act.LabourItemID, act.ProjectID, act.ProjectTaskID, act.CertifiedJob, act.UnionID, timecard.EmployeeID, act.Date.Value);
				if (laborCost == null && PXAccess.FeatureInstalled<FeaturesSet.projectAccounting>() && postingOption != EPPostOptions.DoNotPost && ProjectDefaultAttribute.IsProject(this, act.ProjectID))
				{
					throw new PXException(Messages.EmployeeCostRateNotFound);
				}

				decimal? cost = laborCost?.Rate ?? orig?.EmployeeRate;
				if (orig.NoteID != null)
					origCost = orig.EmployeeRate ?? CostEngine.CalculateEmployeeCost(orig.TimeCardCD, orig.EarningTypeID, orig.LabourItemID, orig.ProjectID, orig.ProjectTaskID, orig.CertifiedJob, orig.UnionID, timecard.EmployeeID, orig.Date.Value)?.Rate;


				int? time = act.TimeSpent;
				if (orig.NoteID != null)
				{
					time = ((act.TimeSpent ?? 0) - (orig.TimeSpent ?? 0));
				}

				int? timeBillable = act.TimeBillable;
				if (orig.NoteID != null)
				{
					timeBillable = ((act.TimeBillable ?? 0) - (orig.TimeBillable ?? 0));
				}


				if (act.RefNoteID != null)
				{
					CRCase crCase = PXSelectJoin<CRCase,
						InnerJoin<CRActivityLink,
							On<CRActivityLink.refNoteID, Equal<CRCase.noteID>>>,
						Where<CRActivityLink.noteID, Equal<Required<EPTimecardDetail.refNoteID>>>>.Select(this, act.RefNoteID);
					if (crCase != null && crCase.ContractID != null)
					{
						PMTran contractUsageTran = null;
						if (act.IsBillable == orig.IsBillable)
						{
							//Correct Contract-Usage
							contractUsageTran = releaseGraph.CreateContractUsage(act, timeBillable.GetValueOrDefault());
						}
						else
						{
							if (act.IsBillable == true)
							{
								//Add Usage
								contractUsageTran = releaseGraph.CreateContractUsage(act, timeBillable.GetValueOrDefault());
							}
							else
							{
								//Subtract Usage
								contractUsageTran = releaseGraph.CreateContractUsage(act, -timeBillable.GetValueOrDefault());
							}
						}

						if (contractUsageTran != null)
							isBilled = false;
					}
				}


				if (orig.NoteID == null || (act.ProjectID == orig.ProjectID && act.ProjectTaskID == orig.ProjectTaskID && act.CostCodeID == orig.CostCodeID
					&& act.UnionID == orig.UnionID && act.LabourItemID == orig.LabourItemID && act.WorkCodeID == orig.WorkCodeID && origCost == cost && act.IsBillable == orig.IsBillable))
				{
					if (postingOption != EPPostOptions.DoNotPost)
						releaseGraph.CreateTransaction(act, timecard.EmployeeID, act.Date.Value, time, timeBillable, cost, laborCost?.OvertimeMultiplier);
				}
				else
				{
					//delete previous:
					if (postingOption != EPPostOptions.DoNotPost)
						releaseGraph.CreateTransaction(orig, timecard.EmployeeID, orig.Date.Value, -orig.TimeSpent, -orig.TimeBillable, origCost, laborCost?.OvertimeMultiplier);

					//add new:
					if (postingOption != EPPostOptions.DoNotPost)
						releaseGraph.CreateTransaction(act, timecard.EmployeeID, act.Date.Value, act.TimeSpent, act.TimeBillable, cost, laborCost?.OvertimeMultiplier);
				}

				released.Add(new ReleasedActivity(act, IN.PXDBPriceCostAttribute.Round((decimal)cost), isBilled));
			}


			//Non-Stock Items:
			PXWeekSelector2Attribute.WeekInfo week = PXWeekSelector2Attribute.GetWeekInfo(this, timecard.WeekID.Value);
			//process deleted items
			PXSelectBase<EPTimeCardItemOrig> selectDeletedItems = new PXSelectJoin<EPTimeCardItemOrig,
				LeftJoin<EPTimeCardItemEx, On<EPTimeCardItemEx.timeCardCD, Equal<Current<EPTimeCard.timeCardCD>>,
					And<EPTimeCardItemEx.origLineNbr, Equal<EPTimeCardItemOrig.lineNbr>>>>,
				Where<EPTimeCardItemOrig.timeCardCD, Equal<Current<EPTimeCard.origTimeCardCD>>,
					And<EPTimeCardItemEx.timeCardCD, IsNull>>>(this);

			foreach (PXResult<EPTimeCardItemOrig, EPTimeCardItemEx> res in selectDeletedItems.View.SelectMultiBound(new object[] { timecard }))
			{
				EPTimeCardItemOrig orig = (EPTimeCardItemOrig)res;

				#region Create Transactions By Days

				if (orig.Sun.GetValueOrDefault() > 0)
				{
					CreateItemTransaction(orig, timecard.EmployeeID, week.Sun.Date, -orig.Sun, tranCache);

				}

				if (orig.Mon.GetValueOrDefault() > 0)
				{
					CreateItemTransaction(orig, timecard.EmployeeID, week.Mon.Date, -orig.Mon, tranCache);
				}

				if (orig.Tue.GetValueOrDefault() > 0)
				{
					CreateItemTransaction(orig, timecard.EmployeeID, week.Tue.Date, -orig.Tue, tranCache);
				}

				if (orig.Wed.GetValueOrDefault() > 0)
				{
					CreateItemTransaction(orig, timecard.EmployeeID, week.Wed.Date, -orig.Wed, tranCache);
				}

				if (orig.Thu.GetValueOrDefault() > 0)
				{
					CreateItemTransaction(orig, timecard.EmployeeID, week.Thu.Date, -orig.Thu, tranCache);
				}

				if (orig.Fri.GetValueOrDefault() > 0)
				{
					CreateItemTransaction(orig, timecard.EmployeeID, week.Fri.Date, -orig.Fri, tranCache);
				}

				if (orig.Sat.GetValueOrDefault() > 0)
				{
					CreateItemTransaction(orig, timecard.EmployeeID, week.Sat.Date, -orig.Sat, tranCache);
				}

				#endregion

			}

			//process added items
			PXSelectBase<EPTimeCardItem> selectAddedItems = new PXSelect<EPTimeCardItem,
				Where<EPTimeCardItem.timeCardCD, Equal<Current<EPTimeCard.timeCardCD>>,
					And<EPTimeCardItem.origLineNbr, IsNull>>>(this);
			foreach (EPTimeCardItem item in selectAddedItems.View.SelectMultiBound(new object[] { timecard }))
			{
				#region Create Transactions By Days

				if (item.Sun.GetValueOrDefault() > 0)
				{
					CreateItemTransaction(item, timecard.EmployeeID, week.Sun.Date, item.Sun, tranCache);
				}

				if (item.Mon.GetValueOrDefault() > 0)
				{
					CreateItemTransaction(item, timecard.EmployeeID, week.Mon.Date, item.Mon, tranCache);
				}

				if (item.Tue.GetValueOrDefault() > 0)
				{
					CreateItemTransaction(item, timecard.EmployeeID, week.Tue.Date, item.Tue, tranCache);
				}

				if (item.Wed.GetValueOrDefault() > 0)
				{
					CreateItemTransaction(item, timecard.EmployeeID, week.Wed.Date, item.Wed, tranCache);
				}

				if (item.Thu.GetValueOrDefault() > 0)
				{
					CreateItemTransaction(item, timecard.EmployeeID, week.Thu.Date, item.Thu, tranCache);
				}

				if (item.Fri.GetValueOrDefault() > 0)
				{
					CreateItemTransaction(item, timecard.EmployeeID, week.Fri.Date, item.Fri, tranCache);
				}

				if (item.Sat.GetValueOrDefault() > 0)
				{
					CreateItemTransaction(item, timecard.EmployeeID, week.Sat.Date, item.Sat, tranCache);
				}

				#endregion
			}

			//process modified records
			PXSelectBase<EPTimeCardItemOrig> selectModifiedItems = new PXSelectJoin<EPTimeCardItemOrig,
				LeftJoin<EPTimeCardItemEx, On<EPTimeCardItemEx.timeCardCD, Equal<Current<EPTimeCard.timeCardCD>>,
					And<EPTimeCardItemEx.origLineNbr, Equal<EPTimeCardItemOrig.lineNbr>>>>,
				Where<EPTimeCardItemOrig.timeCardCD, Equal<Current<EPTimeCard.origTimeCardCD>>,
					And<EPTimeCardItemEx.timeCardCD, IsNotNull>>>(this);
			foreach (PXResult<EPTimeCardItemOrig, EPTimeCardItemEx> res in selectModifiedItems.View.SelectMultiBound(new object[] { timecard }))
			{
				EPTimeCardItemOrig orig = (EPTimeCardItemOrig)res;
				EPTimeCardItemEx item = (EPTimeCardItemEx)res;

				#region Create Transactions By Days

				if (orig.ProjectID == item.ProjectID && orig.TaskID == item.TaskID && orig.InventoryID == item.InventoryID && orig.CostCodeID == item.CostCodeID)
				{
					if (item.Sun.GetValueOrDefault() - orig.Sun.GetValueOrDefault() != 0)
					{
						CreateItemTransaction(item, timecard.EmployeeID, week.Sun.Date, item.Sun.GetValueOrDefault() - orig.Sun.GetValueOrDefault(), tranCache);
					}
				}
				else
				{
					if (orig.Sun.GetValueOrDefault() != 0)
						CreateItemTransaction(orig, timecard.EmployeeID, week.Sun.Date, -orig.Sun.GetValueOrDefault(), tranCache);
					if (item.Sun.GetValueOrDefault() != 0)
						CreateItemTransaction(item, timecard.EmployeeID, week.Sun.Date, item.Sun.GetValueOrDefault(), tranCache);
				}

				if (orig.ProjectID == item.ProjectID && orig.TaskID == item.TaskID && orig.InventoryID == item.InventoryID && orig.CostCodeID == item.CostCodeID)
				{
					if (item.Mon.GetValueOrDefault() - orig.Mon.GetValueOrDefault() != 0)
					{
						CreateItemTransaction(item, timecard.EmployeeID, week.Mon.Date,
							item.Mon.GetValueOrDefault() - orig.Mon.GetValueOrDefault(), tranCache);
					}
				}
				else
				{
					if (orig.Mon.GetValueOrDefault() != 0)
						CreateItemTransaction(orig, timecard.EmployeeID, week.Mon.Date, -orig.Mon.GetValueOrDefault(), tranCache);
					if (item.Mon.GetValueOrDefault() != 0)
						CreateItemTransaction(item, timecard.EmployeeID, week.Mon.Date, item.Mon.GetValueOrDefault(), tranCache);
				}

				if (orig.ProjectID == item.ProjectID && orig.TaskID == item.TaskID && orig.InventoryID == item.InventoryID && orig.CostCodeID == item.CostCodeID)
				{
					if (item.Tue.GetValueOrDefault() - orig.Tue.GetValueOrDefault() != 0)
					{
						CreateItemTransaction(item, timecard.EmployeeID, week.Tue.Date,
							item.Tue.GetValueOrDefault() - orig.Tue.GetValueOrDefault(), tranCache);
					}
				}
				else
				{
					if (orig.Tue.GetValueOrDefault() != 0)
						CreateItemTransaction(orig, timecard.EmployeeID, week.Tue.Date, -orig.Tue.GetValueOrDefault(), tranCache);
					if (item.Tue.GetValueOrDefault() != 0)
						CreateItemTransaction(item, timecard.EmployeeID, week.Tue.Date, item.Tue.GetValueOrDefault(), tranCache);
				}

				if (orig.ProjectID == item.ProjectID && orig.TaskID == item.TaskID && orig.InventoryID == item.InventoryID && orig.CostCodeID == item.CostCodeID)
				{
					if (item.Wed.GetValueOrDefault() - orig.Wed.GetValueOrDefault() != 0)
					{
						CreateItemTransaction(item, timecard.EmployeeID, week.Wed.Date,
							item.Wed.GetValueOrDefault() - orig.Wed.GetValueOrDefault(), tranCache);
					}
				}
				else
				{
					if (orig.Wed.GetValueOrDefault() != 0)
						CreateItemTransaction(orig, timecard.EmployeeID, week.Wed.Date, -orig.Wed.GetValueOrDefault(), tranCache);
					if (item.Wed.GetValueOrDefault() != 0)
						CreateItemTransaction(item, timecard.EmployeeID, week.Wed.Date, item.Wed.GetValueOrDefault(), tranCache);
				}

				if (orig.ProjectID == item.ProjectID && orig.TaskID == item.TaskID && orig.InventoryID == item.InventoryID && orig.CostCodeID == item.CostCodeID)
				{
					if (item.Thu.GetValueOrDefault() - orig.Thu.GetValueOrDefault() != 0)
					{
						CreateItemTransaction(item, timecard.EmployeeID, week.Thu.Date,
							item.Thu.GetValueOrDefault() - orig.Thu.GetValueOrDefault(), tranCache);
					}
				}
				else
				{
					if (orig.Thu.GetValueOrDefault() != 0)
						CreateItemTransaction(orig, timecard.EmployeeID, week.Thu.Date, -orig.Thu.GetValueOrDefault(), tranCache);
					if (item.Thu.GetValueOrDefault() != 0)
						CreateItemTransaction(item, timecard.EmployeeID, week.Thu.Date, item.Thu.GetValueOrDefault(), tranCache);
				}

				if (orig.ProjectID == item.ProjectID && orig.TaskID == item.TaskID && orig.InventoryID == item.InventoryID && orig.CostCodeID == item.CostCodeID)
				{
					if (item.Fri.GetValueOrDefault() - orig.Fri.GetValueOrDefault() != 0)
					{
						CreateItemTransaction(item, timecard.EmployeeID, week.Fri.Date,
							item.Fri.GetValueOrDefault() - orig.Fri.GetValueOrDefault(), tranCache);
					}
				}
				else
				{
					if (orig.Fri.GetValueOrDefault() != 0)
						CreateItemTransaction(orig, timecard.EmployeeID, week.Fri.Date, -orig.Fri.GetValueOrDefault(), tranCache);
					if (item.Fri.GetValueOrDefault() != 0)
						CreateItemTransaction(item, timecard.EmployeeID, week.Fri.Date, item.Fri.GetValueOrDefault(), tranCache);
				}

				if (orig.ProjectID == item.ProjectID && orig.TaskID == item.TaskID && orig.InventoryID == item.InventoryID && orig.CostCodeID == item.CostCodeID)
				{
					if (item.Sat.GetValueOrDefault() - orig.Sat.GetValueOrDefault() != 0)
					{
						CreateItemTransaction(item, timecard.EmployeeID, week.Sat.Date,
							item.Sat.GetValueOrDefault() - orig.Sat.GetValueOrDefault(), tranCache);
					}
				}
				else
				{
					if (orig.Sat.GetValueOrDefault() != 0)
						CreateItemTransaction(orig, timecard.EmployeeID, week.Sat.Date, -orig.Sat.GetValueOrDefault(), tranCache);
					if (item.Sat.GetValueOrDefault() != 0)
						CreateItemTransaction(item, timecard.EmployeeID, week.Sat.Date, item.Sat.GetValueOrDefault(), tranCache);
				}

				#endregion

			}

			//Setting the released/billed status for all activities if and only if all the transaction were processed without an exception in the above loop.
			foreach (ReleasedActivity act in released)
			{
				Activities.Cache.SetValueExt<EPTimecardDetail.released>(act.Activity, true);
				Activities.Cache.SetValue<EPTimecardDetail.employeeRate>(act.Activity, act.Cost);
				if (act.IsBilled)
					Activities.Cache.SetValue<EPTimecardDetail.billed>(act.Activity, true);

				Activities.Cache.SetStatus(act.Activity, PXEntryStatus.Updated);
			}
		}

		public virtual EPEarningType InitEarningType(EPTimecardDetail row)
		{
			if (row == null)
				throw new ArgumentNullException();

			EPEarningType earningType = PXSelect<EPEarningType, Where<EPEarningType.typeCD, Equal<Required<EPEarningType.typeCD>>>>.Select(this, row.EarningTypeID);

			if (earningType != null && row.EarningTypeID != null && Document.Current != null && Document.Current.EmployeeID != null && row.Date != null)
			{
				row.IsOvertimeCalc = earningType.IsOvertime;
				row.OvertimeMultiplierCalc = CostEngine.GetOvertimeMultiplier(row.EarningTypeID, Document.Current.EmployeeID, row.LabourItemID, row.Date.Value);
			}

			return earningType;
		}

		public virtual void RecalculateFields(EPTimecardDetail row)
		{
			if (row == null)
				throw new ArgumentNullException();

			row.BillableTimeCalc = null;
			row.BillableOvertimeCalc = null;
			row.RegularTimeCalc = null;
			row.OverTimeCalc = null;

			if (row.IsOvertimeCalc == true)
			{
				row.OverTimeCalc = row.TimeSpent;
				row.OvertimeSpent = row.TimeSpent;
				if (row.IsBillable == true)
				{
					row.BillableOvertimeCalc = row.TimeBillable;
					row.OvertimeBillable = row.TimeBillable;
				}
			}
			else
			{
				row.RegularTimeCalc = row.TimeSpent;
				if (row.IsBillable == true)
				{
					row.BillableTimeCalc = row.TimeBillable;
				}
			}
		}
		
		public virtual EPTimeCardSummaryWithInfo GetSummaryRecord(EPTimecardDetail activity)
		{			
			foreach (EPTimeCardSummaryWithInfo summary in SelectSummaryRecords(Document.Current, false))
			{
				if (IsFit(summary, activity))
					return summary;
			}
			return null;
		}

		public virtual bool IsFit(EPTimeCardSummary summary, EPTimecardDetail activity)
		{
			if (!string.IsNullOrEmpty(activity.TimeCardCD) && activity.TimeCardCD != summary.TimeCardCD)
				return false;
			if (activity.SummaryLineNbr != null)
			{
				return summary.LineNbr == activity.SummaryLineNbr;
			}

			if (activity.ProjectID.GetValueOrDefault(ProjectDefaultAttribute.NonProject().GetValueOrDefault(0)) != summary.ProjectID.GetValueOrDefault(ProjectDefaultAttribute.NonProject().GetValueOrDefault(0))) return false;
			if (activity.ProjectTaskID != null || summary.ProjectTaskID != null)
			{
				if (activity.ProjectTaskID != summary.ProjectTaskID) return false;
			}
			if (activity.EarningTypeID != summary.EarningType) return false;
			if (activity.IsBillable.GetValueOrDefault() != summary.IsBillable.GetValueOrDefault()) return false;
			if (activity.CertifiedJob.GetValueOrDefault() != summary.CertifiedJob.GetValueOrDefault()) return false;
			if (activity.ParentTaskNoteID != null)
			{
				if (activity.ParentTaskNoteID != summary.ParentNoteID) return false;
			}
			else
			{
				if (summary.ParentNoteID != null) return false;
			}
			if (activity.JobID != null)
			{
				if (activity.JobID != summary.JobID) return false;
			}
			else
			{
				if (summary.JobID != null) return false;
			}
			if (activity.ShiftID != null)
			{
				if (activity.ShiftID != summary.ShiftID) return false;
			}
			else
			{
				if (summary.ShiftID != null) return false;
			}
			if (activity.JobID != null)
			{
				if (activity.JobID != summary.JobID) return false;
			}
			else
			{
				if (summary.JobID != null) return false;
			}
			if (activity.CostCodeID != null)
			{
				if (activity.CostCodeID != summary.CostCodeID) return false;
			}
			else
			{
				if (summary.CostCodeID != null) return false;
			}
			if (activity.UnionID != null)
			{
				if (activity.UnionID != summary.UnionID) return false;
			}
			else
			{
				if (summary.UnionID != null) return false;
			}
			if (activity.LabourItemID != null)
			{
				if (activity.LabourItemID != summary.LabourItemID) return false;
			}
			else
			{
				if (summary.LabourItemID != null) return false;
			}
			if (activity.WorkCodeID != null)
			{
				if (activity.WorkCodeID != summary.WorkCodeID) return false;
			}
			else
			{
				if (summary.WorkCodeID != null) return false;
			}
			return true;
		}

		public virtual bool IsDuplicate(EPTimeCardSummary summary, EPTimeCardSummary duplicate)
		{
			if (summary.LineNbr == duplicate.LineNbr) return false;
			if (duplicate.ProjectID.GetValueOrDefault(ProjectDefaultAttribute.NonProject().GetValueOrDefault(0)) != summary.ProjectID.GetValueOrDefault(ProjectDefaultAttribute.NonProject().GetValueOrDefault(0))) return false;
			if (duplicate.ProjectTaskID != null || summary.ProjectTaskID != null)
			{
				if (duplicate.ProjectTaskID != summary.ProjectTaskID) return false;
			}
			if (duplicate.EarningType != summary.EarningType) return false;
			if (duplicate.IsBillable.GetValueOrDefault() != summary.IsBillable.GetValueOrDefault()) return false;
			if (duplicate.CertifiedJob.GetValueOrDefault() != summary.CertifiedJob.GetValueOrDefault()) return false;
			if (duplicate.ParentNoteID != null)
			{
				if (duplicate.ParentNoteID != summary.ParentNoteID) return false;
			}
			else
			{
				if (summary.ParentNoteID != null) return false;
			}
			if (duplicate.JobID != null)
			{
				if (duplicate.JobID != summary.JobID) return false;
			}
			else
			{
				if (summary.JobID != null) return false;
			}
			if (duplicate.ShiftID != null)
			{
				if (duplicate.ShiftID != summary.ShiftID) return false;
			}
			else
			{
				if (summary.ShiftID != null) return false;
			}
			if (duplicate.JobID != null)
			{
				if (duplicate.JobID != summary.JobID) return false;
			}
			else
			{
				if (summary.JobID != null) return false;
			}
			if (duplicate.CostCodeID != null)
			{
				if (duplicate.CostCodeID != summary.CostCodeID) return false;
			}
			else
			{
				if (summary.CostCodeID != null) return false;
			}
			if (duplicate.UnionID != null)
			{
				if (duplicate.UnionID != summary.UnionID) return false;
			}
			else
			{
				if (summary.UnionID != null) return false;
			}
			if (duplicate.LabourItemID != null)
			{
				if (duplicate.LabourItemID != summary.LabourItemID) return false;
			}
			else
			{
				if (summary.LabourItemID != null) return false;
			}
			if (duplicate.WorkCodeID != null)
			{
				if (duplicate.WorkCodeID != summary.WorkCodeID) return false;
			}
			else
			{
				if (summary.WorkCodeID != null) return false;
			}
			return true;
		}

		public virtual EPTimeCardSummaryWithInfo AddToSummary(EPTimeCardSummaryWithInfo summary, PMTimeActivity activity)
		{
			return AddToSummary(summary, activity, 1);
		}

		public virtual void SubtractFromSummary(EPTimeCardSummaryWithInfo summary, PMTimeActivity activity)
		{
			summary = AddToSummary(summary, activity, -1);

			try
			{
				dontSyncDetails = true;
				if (summary != null && summary.TimeSpent.Value == 0)
				{
					Summary.Delete(summary);//cascadly will delete detail record through PXParent.
				}

			}
			finally
			{
				dontSyncDetails = false;
			}

		}

		public virtual EPTimeCardSummaryWithInfo AddToSummary(EPTimeCardSummaryWithInfo summary, PMTimeActivity activity, int mult)
		{
			if (activity == null)
				throw new ArgumentNullException();

			//Debug.Print("AddToSummary Mult:{0} Start", mult);
			//Debug.Indent();

			if (activity.TimeSpent.GetValueOrDefault() == 0)
			{
				Debug.Unindent();
				//Debug.Print("Activity is empty. Exiting AddToSummary");
				return null;
			}

			if (activity.ProjectID == null)
			{
				Debug.Unindent();
				//Debug.Print("Activity.ProjectID is empty. Exiting AddToSummary");
				return null;
			}

			if (activity.ProjectTaskID == null && !ProjectDefaultAttribute.IsNonProject( activity.ProjectID))
			{
				//do not add to summary with empty TaskID
				return null;
			}

			if (summary == null)
			{
				summary = (EPTimeCardSummaryWithInfo)Summary.Cache.CreateInstance();
				summary.EarningType = activity.EarningTypeID;
				summary.ParentNoteID = activity.ParentTaskNoteID;
				summary.ProjectID = activity.ProjectID;
				summary.ProjectTaskID = activity.ProjectTaskID;
				summary.IsBillable = activity.IsBillable;
				summary.JobID = activity.JobID;
				summary.ShiftID = activity.ShiftID;
				summary.CostCodeID = activity.CostCodeID;
				summary.UnionID = activity.UnionID;
				summary.LabourItemID = activity.LabourItemID;
				summary.WorkCodeID = activity.WorkCodeID;
				summary.CertifiedJob = activity.CertifiedJob;
				PMProject project = (PMProject) PXSelectorAttribute.Select<EPTimecardDetail.projectID>(Activities.Cache, activity, activity.ProjectID);
				if (project != null)
				{
					summary.ProjectManager = project.ApproverID;
				}
				PMTask projectTask = (PMTask)PXSelectorAttribute.Select<EPTimecardDetail.projectTaskID>(Activities.Cache, activity, activity.ProjectTaskID);
				if (projectTask != null)
				{
					summary.TaskApproverID = projectTask.ApproverID;
				}

				if (activity.ParentTaskNoteID != null)
				{
					CRActivity parentTask = PXSelect<CRActivity,
						Where<CRActivity.noteID, Equal<Required<PMTimeActivity.parentTaskNoteID>>>>
						.Select(this, activity.ParentTaskNoteID);

					if (parentTask != null)
						summary.Description = parentTask.Subject;
				}
				else
				{
					PMTask pmTask = PXSelect<PMTask,
						Where<PMTask.projectID, Equal<Required<PMTask.projectID>>,
							And<PMTask.noteID, Equal<Required<PMTask.noteID>>>>>
						.Select(this, activity.ProjectID, activity.ParentTaskNoteID);

					if (pmTask != null)
						summary.Description = pmTask.Description;
				}

				dontSyncDetails = true;
				try
				{
					summary = Summary.Insert(summary);
					if (summary == null) return null;
					summary.ProjectID = activity.ProjectID; //may have been overriden in rowInserting()
					summary.ProjectTaskID = activity.ProjectTaskID; //may have been overriden in rowInserting()				
					summary.CostCodeID = activity.CostCodeID;
					summary.UnionID = activity.UnionID;
					summary.LabourItemID = activity.LabourItemID;
					summary.WorkCodeID = activity.WorkCodeID;
					summary.CertifiedJob = activity.CertifiedJob;
					summary.IsBillable = activity.IsBillable;
				}
				finally
				{
					dontSyncDetails = false;
				}
			}

			AddActivityTimeToSummary(summary, activity, mult);
            Summary.Cache.MarkUpdated(summary);
            

			//Debug.Unindent();
			//Debug.Print("AddToSummary End");


			return summary;
		}

		public virtual void AddActivityTimeToSummary(EPTimeCardSummary summary, PMTimeActivity activity, int mult)
		{
			if (activity.TimeSpent != null && activity.Date != null)
			{
				switch (activity.Date.Value.DayOfWeek)
				{
					case DayOfWeek.Monday:
						summary.Mon = summary.Mon.GetValueOrDefault() + (mult * activity.TimeSpent.Value);
						break;
					case DayOfWeek.Tuesday:
						summary.Tue = summary.Tue.GetValueOrDefault() + (mult * activity.TimeSpent.Value);
						break;
					case DayOfWeek.Wednesday:
						summary.Wed = summary.Wed.GetValueOrDefault() + (mult * activity.TimeSpent.Value);
						break;
					case DayOfWeek.Thursday:
						summary.Thu = summary.Thu.GetValueOrDefault() + (mult * activity.TimeSpent.Value);
						break;
					case DayOfWeek.Friday:
						summary.Fri = summary.Fri.GetValueOrDefault() + (mult * activity.TimeSpent.Value);
						break;
					case DayOfWeek.Saturday:
						summary.Sat = summary.Sat.GetValueOrDefault() + (mult * activity.TimeSpent.Value);
						break;
					case DayOfWeek.Sunday:
						summary.Sun = summary.Sun.GetValueOrDefault() + (mult * activity.TimeSpent.Value);
						break;
				}
			}
		}

		public virtual void UpdateAdjustingActivities(EPTimeCardSummary summary)
		{
			UpdateAdjustingActivities(summary, false);
		}
		public virtual void UpdateAdjustingActivities(EPTimeCardSummary summary, bool skipDescriptionUpdate)
		{
			if (summary == null)
				throw new ArgumentNullException();

			EPTimeCard doc = PXSelect<EPTimeCard, Where<EPTimeCard.timeCardCD, Equal<Required<EPTimeCard.timeCardCD>>>>.Select(this, summary.TimeCardCD);
			if (doc == null)
				return;

			Dictionary<DayOfWeek, DayActivities> dict = GetActivities(summary, doc);

			PX.Objects.EP.PXWeekSelector2Attribute.WeekInfo weekInfo = PXWeekSelector2Attribute.GetWeekInfo(this, doc.WeekID.Value);

			if (weekInfo.Mon.Enabled)
				UpdateAdjustingActivities(summary, dict, DayOfWeek.Monday, SetEmployeeTime(weekInfo.Mon.Date).Value, skipDescriptionUpdate);
			if (weekInfo.Tue.Enabled)
				UpdateAdjustingActivities(summary, dict, DayOfWeek.Tuesday, SetEmployeeTime(weekInfo.Tue.Date).Value, skipDescriptionUpdate);
			if (weekInfo.Wed.Enabled)
				UpdateAdjustingActivities(summary, dict, DayOfWeek.Wednesday, SetEmployeeTime(weekInfo.Wed.Date).Value, skipDescriptionUpdate);
			if (weekInfo.Thu.Enabled)
				UpdateAdjustingActivities(summary, dict, DayOfWeek.Thursday, SetEmployeeTime(weekInfo.Thu.Date).Value, skipDescriptionUpdate);
			if (weekInfo.Fri.Enabled)
				UpdateAdjustingActivities(summary, dict, DayOfWeek.Friday, SetEmployeeTime(weekInfo.Fri.Date).Value, skipDescriptionUpdate);
			if (weekInfo.Sat.Enabled)
				UpdateAdjustingActivities(summary, dict, DayOfWeek.Saturday, SetEmployeeTime(weekInfo.Sat.Date).Value, skipDescriptionUpdate);
			if (weekInfo.Sun.Enabled)
				UpdateAdjustingActivities(summary, dict, DayOfWeek.Sunday, SetEmployeeTime(weekInfo.Sun.Date).Value, skipDescriptionUpdate);
		}

		public virtual DateTime? SetEmployeeTime(DateTime? date)
		{
			EPEmployee emp = Employee.Select();
			if (emp != null)
			{
				CSCalendar cal = PXSelect<CSCalendar, Where<CSCalendar.calendarID, Equal<Required<CSCalendar.calendarID>>>>.Select(this, emp.CalendarID);
				if (cal != null)
				{
					switch (date.Value.DayOfWeek)
					{
						case DayOfWeek.Monday: return PXDBDateAndTimeAttribute.CombineDateTime(date, cal.MonStartTime);
						case DayOfWeek.Tuesday: return PXDBDateAndTimeAttribute.CombineDateTime(date, cal.TueStartTime);
						case DayOfWeek.Wednesday: return PXDBDateAndTimeAttribute.CombineDateTime(date, cal.WedStartTime);
						case DayOfWeek.Thursday: return PXDBDateAndTimeAttribute.CombineDateTime(date, cal.ThuStartTime);
						case DayOfWeek.Friday: return PXDBDateAndTimeAttribute.CombineDateTime(date, cal.FriStartTime);
						case DayOfWeek.Saturday: return PXDBDateAndTimeAttribute.CombineDateTime(date, cal.SatStartTime);
						case DayOfWeek.Sunday: return PXDBDateAndTimeAttribute.CombineDateTime(date, cal.SunStartTime);
					}
				}
			}

			return PXDateAndTimeAttribute.CombineDateTime(date, new DateTime(2008, 1, 1, 9, 0, 0));//use default 9:00
		}


		public virtual void UpdateAdjustingActivities(EPTimeCardSummary summary, Dictionary<DayOfWeek, DayActivities> dict, DayOfWeek dayOfWeek, DateTime startDate, bool skipDescriptionUpdate)
		{
			if (summary == null)
				throw new ArgumentNullException("summary");

			if (dict == null)
				throw new ArgumentNullException("dict");

			//Debug.Print("UpdateAdjustingActivities for {0} Start", dayOfWeek);
			Debug.Indent();

			int summaryTimeTotal = 0;

			if (Summary.Cache.GetStatus(summary) != PXEntryStatus.Deleted && Summary.Cache.GetStatus(summary) != PXEntryStatus.InsertedDeleted)
				summaryTimeTotal = summary.GetTimeTotal(dayOfWeek).GetValueOrDefault();

			int dayTotal = 0;
			if (dict.ContainsKey(dayOfWeek))
			{
				dayTotal = dict[dayOfWeek].GetTotalTime(summary.LineNbr.Value);
			}

			EPTimecardDetail adjust = null;
			if (dict.ContainsKey(dayOfWeek))
			{
				adjust = dict[dayOfWeek].GetAdjustingActivity(summary.LineNbr.Value);
			}

			if (summaryTimeTotal != dayTotal)
			{
				if (adjust == null && summaryTimeTotal - dayTotal != 0)
				{
					dontSyncSummary = true;
					try
					{
						adjust = Activities.Insert();

						// CR
						Activities.Cache.SetValueExt<EPTimecardDetail.date>(adjust, startDate);
						if (!string.IsNullOrEmpty(summary.Description))
						{
							adjust.Summary = summary.Description;
						}
						else
						{
							adjust.Summary = string.Format(PXMessages.LocalizeNoPrefix(Messages.SummaryActivities), LocalizeDayOfWeek(dayOfWeek));
						}
						adjust.ParentTaskNoteID = summary.ParentNoteID;
						adjust.ApprovalStatus = ActivityStatusAttribute.Completed;

						// PM
						adjust.EarningTypeID = summary.EarningType;
						adjust.JobID = summary.JobID;
						adjust.ShiftID = summary.ShiftID;
						adjust.CostCodeID = summary.CostCodeID;
						adjust.UnionID = summary.UnionID;
						adjust.LabourItemID = summary.LabourItemID;
						adjust.WorkCodeID = summary.WorkCodeID;
						adjust.SummaryLineNbr = summary.LineNbr;
						adjust.Day = ((int)adjust.Date.Value.DayOfWeek).ToString();
						adjust.IsBillable = summary.IsBillable;
						adjust.CertifiedJob = summary.CertifiedJob;
						adjust.TimeSpent = (summaryTimeTotal - dayTotal);
						if (adjust.IsBillable == true)
							adjust.TimeBillable = adjust.TimeSpent;
						adjust.ProjectID = summary.ProjectID;
						adjust.ProjectTaskID = summary.ProjectTaskID;
						Activities.Cache.SetDefaultExt<EPTimecardDetail.approverID>(adjust);
						Activities.Cache.SetDefaultExt<EPTimecardDetail.approvalStatus>(adjust);
						var cost = CostEngine.CalculateEmployeeCost(null, adjust.EarningTypeID, adjust.LabourItemID, adjust.ProjectID, adjust.ProjectTaskID, adjust.CertifiedJob, adjust.UnionID, Document.Current.EmployeeID, adjust.Date.Value);
						adjust.EmployeeRate = cost?.Rate;
						adjust.OvertimeMultiplierCalc = cost?.OvertimeMultiplier;

						InitEarningType(adjust);
						RecalculateFields(adjust);
					}
					finally
					{
						dontSyncSummary = false;
					}
				}
				else if (adjust != null && summaryTimeTotal == 0 && adjust.SummaryLineNbr != null)//delete only adjusting activity that was added automatically on summary update.
				{
					dontSyncSummary = true;
					try
					{
						Activities.Delete(adjust);
					}
					finally
					{
						dontSyncSummary = false;
					}
				}
				else if (adjust != null)
				{
					adjust.TimeSpent = adjust.TimeSpent + summaryTimeTotal - dayTotal;
					if (!string.IsNullOrEmpty(summary.Description))
					{
						if (!skipDescriptionUpdate || string.IsNullOrEmpty(adjust.Summary))
							adjust.Summary = summary.Description;
					}
					adjust.IsBillable = summary.IsBillable;
					if (adjust.IsBillable == true)
						adjust.TimeBillable = adjust.TimeSpent;
					adjust.CertifiedJob = summary.CertifiedJob;
					RecalculateFields(adjust);
					adjust.ApprovalStatus = ApprovalStatusAttribute.PendingApproval;

					Activities.Cache.MarkUpdated(adjust);

				}
			}
			else
			{
				if (adjust != null)
				{
					if (!string.IsNullOrEmpty(summary.Description) && !skipDescriptionUpdate)
					{
						Activities.Cache.SetValue<EPTimecardDetail.summary>(adjust, summary.Description);
						Activities.Cache.MarkUpdated(adjust);
					}
				}
			}

			Debug.Unindent();
			//Debug.Print("UpdateAdjustingActivities for {0} End", dayOfWeek);
		}

		public virtual Dictionary<DayOfWeek, DayActivities> GetActivities(EPTimeCardSummary summary, EPTimeCard doc)
		{
			if (summary == null)
				throw new ArgumentNullException("summary");
			if (doc == null)
				throw new ArgumentNullException("doc");

			Dictionary<DayOfWeek, DayActivities> dict = new Dictionary<DayOfWeek, DayActivities>();

			List<EPTimeCardSummary> duplicates = FindDuplicates(summary);

			foreach (EPTimecardDetail activity in GetDetails(summary, doc, false))
			{
				bool duplicateFit = false;
				foreach(EPTimeCardSummary duplicate in duplicates)
				{
					if (IsFit(duplicate, activity))
					{
						duplicateFit = true;
						break;
					}
				}
				if (duplicateFit)
					continue;

				DayOfWeek day = activity.Date.Value.DayOfWeek;

				if (dict.ContainsKey(activity.Date.Value.DayOfWeek))
				{
					dict[activity.Date.Value.DayOfWeek].Activities.Add(activity);
				}
				else
				{
					DayActivities d = new DayActivities();
					d.Day = day;
					d.Activities.Add(activity);
					dict.Add(activity.Date.Value.DayOfWeek, d);
				}
			}

			return dict;
		}
		public virtual bool AreTheseConnected(EPTimeCardSummary summary, EPTimecardDetail detail)
		{
			if (detail.SummaryLineNbr != null)
			{
				return detail.SummaryLineNbr == summary.LineNbr;
			}
			if (summary.EarningType != detail.EarningTypeID)
				return false;
			if (summary.JobID != detail.JobID)
				return false;
			if (summary.ShiftID != detail.ShiftID)
				return false;
			if (summary.ProjectID != detail.ProjectID)
				return false;
			if (summary.ProjectTaskID != detail.ProjectTaskID)
				return false;
			if (summary.CostCodeID != detail.CostCodeID)
				return false;
			if (summary.ParentNoteID != null)
			{
				return summary.ParentNoteID == detail.ParentTaskNoteID;
			}
			return true;
		}

		public virtual List<EPTimeCardSummary> FindDuplicates(EPTimeCardSummary summary)
			{
			List<EPTimeCardSummary> duplicates = new List<EPTimeCardSummary>();
			foreach (EPTimeCardSummary duplicate in SelectSummaryRecords(Document.Current, false))
			{
				if (IsDuplicate(summary, duplicate))
					duplicates.Add(duplicate);
			}

			return duplicates;
		}

		public virtual List<EPTimecardDetail> GetDetails(EPTimeCardSummary summary, EPTimeCard doc, bool onlyManual)
		{	
			if (summary == null)
				throw new ArgumentNullException("summary");
			if (doc == null)
				throw new ArgumentNullException("doc");
			List<EPTimecardDetail> result = new List<EPTimecardDetail>();
			foreach (PXResult res in activities())
			{
				EPTimecardDetail detail = (EPTimecardDetail)res[typeof(EPTimecardDetail)];
			
				if (IsFit(summary, detail))
				{
					if (onlyManual && detail.SummaryLineNbr != null)
					{
						continue;
					}
					result.Add(detail);
				}
			}
			return result;
		}

		public virtual void RecalculateTotals(EPTimeCard timecard)
		{
			if (timecard == null)
				throw new ArgumentNullException();

			List<EPTimecardDetail> list = new List<EPTimecardDetail>();

			if (timecard.IsHold == true)
			{
				foreach (PXResult res in activities())
				{
					EPTimecardDetail detail = (EPTimecardDetail)res[typeof(EPTimecardDetail)];
					list.Add(detail);
				}
			}

			RecalculateTotals(timecard, list);
		}

		public virtual void RecalculateTotals(EPTimeCard timecard, List<EPTimecardDetail> details)
		{
			if (timecard == null)
				throw new ArgumentNullException("timecard");

			if (details == null)
				throw new ArgumentNullException("details");

            int regularTime = timecard.TimeSpent.GetValueOrDefault();
			int overtimeSpent = timecard.OvertimeSpent.GetValueOrDefault();
			int timeBillable = timecard.TimeBillable.GetValueOrDefault();
			int overtimeBillable = timecard.OvertimeBillable.GetValueOrDefault();

			if (timecard.IsHold == true)
			{
				regularTime = 0;
				overtimeSpent = 0;
				timeBillable = 0;
				overtimeBillable = 0;

				foreach (EPTimecardDetail detail in details)
				{
					regularTime += detail.RegularTimeCalc.GetValueOrDefault();
					timeBillable += detail.BillableTimeCalc.GetValueOrDefault();
					overtimeSpent += detail.OverTimeCalc.GetValueOrDefault();
					overtimeBillable += detail.BillableOvertimeCalc.GetValueOrDefault();
				}
			}
			
			timecard.TimeSpentCalc = regularTime;
			timecard.OvertimeSpentCalc = overtimeSpent;
			timecard.TotalSpentCalc = regularTime + overtimeSpent;

			timecard.TimeBillableCalc = timeBillable;
			timecard.OvertimeBillableCalc = overtimeBillable;
			timecard.TotalBillableCalc = timeBillable + overtimeBillable;
		}
				
		public virtual void ValidateProjectAndProjectTask(EPTimecardDetail timeCardDetail)
		{
			if (timeCardDetail != null && Document.Current != null && Document.Current.IsReleased != true)
			{
				string errProjectMsg = PXUIFieldAttribute.GetError<EPTimecardDetail.projectID>(Activities.Cache, timeCardDetail);
				if (!string.IsNullOrEmpty(errProjectMsg) && errProjectMsg.Equals(PXLocalizer.Localize(PM.Messages.ProjectExpired)))
				{
					PXUIFieldAttribute.SetError<EPTimecardDetail.projectID>(Activities.Cache, timeCardDetail, null);
				}
				
				if (timeCardDetail.ProjectID != null)
				{
					PMProject project = (PMProject) PXSelectorAttribute.Select<EPTimecardDetail.projectID>(Activities.Cache, timeCardDetail, timeCardDetail.ProjectID);
					if (project != null && timeCardDetail != null && project.ExpireDate != null && timeCardDetail.Date != null)
					{
						if (timeCardDetail.Date > project.ExpireDate)
						{
							Activities.Cache.RaiseExceptionHandling<EPTimecardDetail.projectID>(
									timeCardDetail, timeCardDetail.ProjectID,
									new PXSetPropertyException(PM.Messages.ProjectExpired,
									PXErrorLevel.Warning));
						}
					}
				}

				string errProjTaskMsg = PXUIFieldAttribute.GetError<EPTimecardDetail.projectTaskID>(Activities.Cache, timeCardDetail);
				if (!string.IsNullOrEmpty(errProjTaskMsg) && errProjTaskMsg.Equals(PXLocalizer.Localize(PM.Messages.ProjectTaskExpired)))
				{
					PXUIFieldAttribute.SetError<EPTimecardDetail.projectTaskID>(Activities.Cache, timeCardDetail, null);
				}

				if (timeCardDetail.ProjectTaskID != null)
				{
					PMTask projectTask = PXSelect<PMTask, Where<PMTask.taskID, Equal<Required<EPTimeCardSummary.projectTaskID>>>>.Select(this, timeCardDetail.ProjectTaskID);
					if (projectTask != null && timeCardDetail != null && projectTask.EndDate != null && timeCardDetail.Date != null)
					{
						if (timeCardDetail.Date > projectTask.EndDate)
						{
							Activities.Cache.RaiseExceptionHandling<EPTimecardDetail.projectTaskID>(
								timeCardDetail, timeCardDetail.ProjectTaskID,
								new PXSetPropertyException(PM.Messages.ProjectTaskExpired,
								PXErrorLevel.Warning));
						}					
					}
				}
			}
		}

		public virtual bool ValidateTotals(EPTimeCard timecard, out string errorMsg)
		{
			if (timecard == null)
				throw new ArgumentNullException();

			bool valid = true;
			errorMsg = null;

			EPEmployee employee = PXSelect<EPEmployee, Where<EPEmployee.bAccountID, Equal<Required<EPEmployee.bAccountID>>>>.Select(this, timecard.EmployeeID);
			if (employee == null || employee.HoursValidation == HoursValidationOption.None)
			{
				return true;//always valid
			}

			PXUIFieldAttribute.SetError<EPTimeCard.timeSpentCalc>(Document.Cache, timecard, null);
			PXUIFieldAttribute.SetError<EPTimeCard.overtimeSpentCalc>(Document.Cache, timecard, null);
			PXUIFieldAttribute.SetError<EPTimeCard.totalSpentCalc>(Document.Cache, timecard, null);

			DateTime date = Accessinfo.BusinessDate ?? DateTime.Now;
			bool isFullWeek = true;
			if (timecard.WeekID != null)
			{
				EPWeekRaw week = PXSelectorAttribute.Select<EPTimeCard.weekId>(Document.Cache, timecard) as EPWeekRaw;
				if (week != null)
				{
					date = week.StartDate.Value;
					isFullWeek = week.IsFullWeek.Value;
				}
			}

			decimal? regularHours = CostEngine.GetEmployeeRegularHours(timecard.EmployeeID, date);
			if (isFullWeek && (timecard.TimeSpentCalc > (regularHours * 60)))
			{
				valid = false;
				errorMsg = PXMessages.LocalizeFormatNoPrefixNLA(Messages.TotalTimeForWeekCannotExceedHours, regularHours);
				Document.Cache.RaiseExceptionHandling<EPTimeCard.timeSpentCalc>(timecard, timecard.TimeSpentCalc, new PXSetPropertyException(errorMsg, PXErrorLevel.Warning));
			}
			else if (isFullWeek && (timecard.TimeSpentCalc < (regularHours * 60)))
			{
				valid = false;

				if (timecard.OvertimeSpentCalc > 0)
				{
					errorMsg = PXMessages.LocalizeFormatNoPrefixNLA(Messages.OvertimeNotAllowed, regularHours);
					Document.Cache.RaiseExceptionHandling<EPTimeCard.overtimeSpentCalc>(timecard, timecard.OvertimeSpentCalc, new PXSetPropertyException(errorMsg, PXErrorLevel.Warning));
				}
				else
				{
					errorMsg = PXMessages.LocalizeFormatNoPrefixNLA(Messages.TimecradIsNotNormalized, regularHours);
					Document.Cache.RaiseExceptionHandling<EPTimeCard.totalSpentCalc>(timecard, timecard.TotalSpentCalc, new PXSetPropertyException(errorMsg, PXErrorLevel.Warning));
				}

			}

			return valid;
		}

		public virtual PM.PMTran CreateItemTransaction(EPTimeCardItem record, int? employeeID, DateTime? date, decimal? qty, PXCache tranCache)
		{
			InventoryItem nsItem = PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>.Select(this, record.InventoryID);
			if (nsItem == null)
			{
				throw new PXException(Messages.InventoryItemIsEmpty);
			}

			if (nsItem.InvtAcctID == null)
			{
				throw new PXException(Messages.ExpenseAccrualIsRequired, nsItem.InventoryCD.Trim());
			}

			if (nsItem.InvtSubID == null)
			{
				throw new PXException(Messages.ExpenseAccrualSubIsRequired, nsItem.InventoryCD.Trim());
			}

			Contract contract = PXSelect<Contract, Where<Contract.contractID, Equal<Required<Contract.contractID>>>>.Select(this, record.ProjectID);

			decimal? cost = 0m;

			int? accountID = nsItem.COGSAcctID;
			int? offsetaccountID = nsItem.InvtAcctID;
			int? accountGroupID = null;
			string subCD = null;
			string offsetSubCD = null;

			int? branchID = null;
			EP.EPEmployee emp = PXSelect<EP.EPEmployee, Where<EP.EPEmployee.bAccountID, Equal<Required<EP.EPEmployee.bAccountID>>>>.Select(this, employeeID);
			if (emp != null)
			{
				Branch branch = PXSelect<Branch, Where<Branch.bAccountID, Equal<Required<EPEmployee.parentBAccountID>>>>.Select(this, emp.ParentBAccountID);
				if (branch != null)
				{
					branchID = branch.BranchID;
				}
			}

			if (contract.BaseType == CT.CTPRType.Project)//contract do not record money only usage.
			{
				cost = nsItem.StdCost;

				if (contract.NonProject != true)
				{
					PMTask task = PXSelect<PMTask, Where<PMTask.projectID, Equal<Required<PMTask.projectID>>, And<PMTask.taskID, Equal<Required<PMTask.taskID>>>>>.Select(this, record.ProjectID, record.TaskID);

					#region Combine Account and Subaccount

					if (ExpenseAccountSource == PMAccountSource.Project)
					{
						if (contract.DefaultAccountID != null)
						{
							accountID = contract.DefaultAccountID;
							Account account = PXSelect<Account, Where<Account.accountID, Equal<Required<Account.accountID>>>>.Select(this, accountID);
							if (account.AccountGroupID == null)
							{
								throw new PXException(Messages.NoAccountGroupOnProject, account.AccountCD.Trim(), contract.ContractCD.Trim());
							}
							accountGroupID = account.AccountGroupID;
						}
						else
						{
							PXTrace.WriteWarning(Messages.NoDefualtAccountOnProject, contract.ContractCD.Trim());
						}
					}
					else if (ExpenseAccountSource == PMAccountSource.Task)
					{

						if (task.DefaultAccountID != null)
						{
							accountID = task.DefaultAccountID;
							Account account = PXSelect<Account, Where<Account.accountID, Equal<Required<Account.accountID>>>>.Select(this, accountID);
							if (account.AccountGroupID == null)
							{
								throw new PXException(Messages.NoAccountGroupOnTask, account.AccountCD.Trim(), contract.ContractCD.Trim(), task.TaskCD.Trim());
							}
							accountGroupID = account.AccountGroupID;
						}
						else
						{
							PXTrace.WriteWarning(Messages.NoDefualtAccountOnTask, contract.ContractCD.Trim(), task.TaskCD.Trim());
						}
					}
					else if (ExpenseAccountSource == PMAccountSource.Employee)
					{
						if (emp.ExpenseAcctID != null)
						{
							accountID = emp.ExpenseAcctID;
							Account account = PXSelect<Account, Where<Account.accountID, Equal<Required<Account.accountID>>>>.Select(this, accountID);
							if (account.AccountGroupID == null)
							{
								throw new PXException(Messages.NoAccountGroupOnEmployee, account.AccountCD, emp.AcctCD.Trim());
							}
							accountGroupID = account.AccountGroupID;
						}
						else
						{
							PXTrace.WriteWarning(Messages.NoExpenseAccountOnEmployee, emp.AcctCD.Trim());
						}
					}
					else
					{
						if (accountID == null)
						{
							throw new PXException(Messages.NoExpenseAccountOnInventory, nsItem.InventoryCD.Trim());
						}

						//defaults to InventoryItem.COGSAcctID
						Account account = PXSelect<Account, Where<Account.accountID, Equal<Required<Account.accountID>>>>.Select(this, accountID);
						if (account.AccountGroupID == null)
						{
							throw new PXException(Messages.NoAccountGroupOnInventory, account.AccountCD.Trim(), nsItem.InventoryCD.Trim());
						}
						accountGroupID = account.AccountGroupID;
					}


					if (accountGroupID == null)
					{
						//defaults to InventoryItem.COGSAcctID
						Account account = PXSelect<Account, Where<Account.accountID, Equal<Required<Account.accountID>>>>.Select(this, accountID);
						if (account.AccountGroupID == null)
						{
							throw new PXException(Messages.AccountGroupIsNotAssignedForAccount, account.AccountCD.Trim());
						}
						accountGroupID = account.AccountGroupID;
					}


					if (!string.IsNullOrEmpty(ExpenseSubMask))
					{
						if (ExpenseSubMask.Contains(PMAccountSource.InventoryItem) && nsItem.COGSSubID == null)
						{
							throw new PXException(Messages.NoExpenseSubOnInventory, nsItem.InventoryCD.Trim());
						}
						if (ExpenseSubMask.Contains(PMAccountSource.Project) && contract.DefaultSubID == null)
						{
							throw new PXException(Messages.NoExpenseSubOnProject, contract.ContractCD.Trim());
						}
						if (ExpenseSubMask.Contains(PMAccountSource.Task) && task.DefaultSubID == null)
						{
							throw new PXException(Messages.NoExpenseSubOnTask, contract.ContractCD.Trim(), task.TaskCD.Trim());
						}
						if (ExpenseSubMask.Contains(PMAccountSource.Employee) && emp.ExpenseSubID == null)
						{
							throw new PXException(Messages.NoExpenseSubOnEmployee, emp.AcctCD.Trim());
						}


						subCD = PM.SubAccountMaskAttribute.MakeSub<PMSetup.expenseSubMask>(this, ExpenseSubMask,
							new object[] { nsItem.COGSSubID, contract.DefaultSubID, task.DefaultSubID, emp.ExpenseSubID },
							new Type[] { typeof(InventoryItem.cOGSSubID), typeof(Contract.defaultSubID), typeof(PMTask.defaultSubID), typeof(EPEmployee.expenseSubID) });
					}

					#endregion

					#region Combine Accrual Account and Subaccount

					if (ExpenseAccrualAccountSource == PMAccountSource.Project)
					{
						if (contract.DefaultAccrualAccountID != null)
						{
							offsetaccountID = contract.DefaultAccrualAccountID;
						}
						else
						{
							PXTrace.WriteWarning(EP.Messages.NoDefualtAccrualAccountOnProject, contract.ContractCD.Trim());
						}
					}
					else if (ExpenseAccrualAccountSource == PMAccountSource.Task)
					{
						if (task.DefaultAccrualAccountID != null)
						{
							offsetaccountID = task.DefaultAccrualAccountID;
						}
						else
						{
							PXTrace.WriteWarning(EP.Messages.NoDefualtAccountOnTask, contract.ContractCD.Trim(), task.TaskCD.Trim());
						}
					}
					else
					{
						if (offsetaccountID == null)
						{
							throw new PXException(EP.Messages.NoAccrualExpenseAccountOnInventory, nsItem.InventoryCD.Trim());
						}
					}

					if (!string.IsNullOrEmpty(ExpenseAccrualSubMask))
					{
						if (ExpenseAccrualSubMask.Contains(PMAccountSource.InventoryItem) && nsItem.InvtSubID == null)
						{
							throw new PXException(EP.Messages.NoExpenseAccrualSubOnInventory, nsItem.InventoryCD.Trim());
						}
						if (ExpenseAccrualSubMask.Contains(PMAccountSource.Project) && contract.DefaultAccrualSubID == null)
						{
							throw new PXException(EP.Messages.NoExpenseAccrualSubOnProject, contract.ContractCD.Trim());
						}
						if (ExpenseAccrualSubMask.Contains(PMAccountSource.Task) && task.DefaultAccrualSubID == null)
						{
							throw new PXException(EP.Messages.NoExpenseAccrualSubOnTask, contract.ContractCD.Trim(), task.TaskCD.Trim());
						}
						if (ExpenseAccrualSubMask.Contains(PMAccountSource.Employee) && emp.ExpenseSubID == null)
						{
							throw new PXException(Messages.NoExpenseSubOnEmployee, emp.AcctCD.Trim());
						}

						offsetSubCD = PM.SubAccountMaskAttribute.MakeSub<PMSetup.expenseAccrualSubMask>(this, ExpenseAccrualSubMask,
							new object[] { nsItem.InvtSubID, contract.DefaultAccrualSubID, task.DefaultAccrualSubID, emp.ExpenseSubID },
							new Type[] { typeof(InventoryItem.invtSubID), typeof(Contract.defaultAccrualSubID), typeof(PMTask.defaultAccrualSubID), typeof(EPEmployee.expenseSubID) });
					}

					#endregion
				}
				else
				{
					//defaults to InventoryItem.COGSAcctID
					Account account = PXSelect<Account, Where<Account.accountID, Equal<Required<Account.accountID>>>>.Select(this, accountID);
					if (account.AccountGroupID != null)
					{
						accountGroupID = account.AccountGroupID;
					}
				}
			}

			int? subID = nsItem.COGSSubID;
			int? offsetSubID = nsItem.InvtSubID;
			EPSetup epsetup = PXSelect<EPSetup>.Select(this);
			if (epsetup != null && EPSetupMaint.GetPostingOption(this, epsetup, employeeID) == EPPostOptions.PostToOffBalance)
			{
				accountGroupID = epsetup.OffBalanceAccountGroupID;
				accountID = null;
				offsetaccountID = null;
				offsetSubID = null;
				subCD = null;
				subID = null;
			}

			PMTran tran = (PMTran)tranCache.Insert();
			tran.BranchID = branchID;
			tran.AccountID = accountID;
			if (string.IsNullOrEmpty(subCD))
				tran.SubID = subID;
			if (string.IsNullOrEmpty(offsetSubCD))
				tran.OffsetSubID = offsetSubID;
			tran.AccountGroupID = accountGroupID;
			tran.ProjectID = record.ProjectID;
			tran.TaskID = record.TaskID;
			tran.CostCodeID = record.CostCodeID;
			tran.InventoryID = record.InventoryID;
			tran.ResourceID = employeeID;
			tran.Date = date;
			FinPeriod finPeriod = FinPeriodRepository.GetFinPeriodByDate(tran.Date, PXAccess.GetParentOrganizationID(branchID));
			tran.FinPeriodID = finPeriod?.FinPeriodID;
			tran.Qty = qty;
			tran.UOM = record.UOM;
			tran.Billable = true;
			tran.BillableQty = tran.Qty;
			tran.TranCuryUnitRate = PXDBPriceCostAttribute.Round((decimal)cost);
			tran.Amount = null;
			tran.Description = record.Description;
			tran.OffsetAccountID = offsetaccountID;
			tran.IsQtyOnly = contract.BaseType == CTPRType.Contract;

			tran = (PMTran)tranCache.Update(tran);

			if (!string.IsNullOrEmpty(subCD))
				tranCache.SetValueExt<PMTran.subID>(tran, subCD);

			if (!string.IsNullOrEmpty(offsetSubCD))
				tranCache.SetValueExt<PMTran.offsetSubID>(tran, offsetSubCD);

			PXNoteAttribute.CopyNoteAndFiles(Items.Cache, record, tranCache, tran);

			return tran;
		}

		public virtual EmployeeCostEngine CreateEmployeeCostEngine()
		{
			return new EmployeeCostEngine(this);
		}

		protected PXCache CreateInstanceCache<TNode>(Type graphType)
			where TNode : IBqlTable
		{
			if (graphType != null)
			{
				var graph = PXGraph.CreateInstance(graphType);
				graph.Clear();
				foreach (Type type in graph.Views.Caches)
				{
					var cache = graph.Caches[type];
					if (typeof(TNode).IsAssignableFrom(cache.GetItemType()))
						return cache;
				}
			}
			return null;
		}

		public virtual bool IsFirstTimeCard(int? employeeID)
		{
			return employeeID == null ||
				PXSelectReadonly<EPTimecardLite,
					Where<EPTimecardLite.employeeID, Equal<Required<EPTimecardLite.employeeID>>>>.
				SelectWindowed(this, 0, 1, employeeID).Count == 0;
		}

		public virtual int? GetNextWeekID(int? employeeID)
		{
			var isFist = IsFirstTimeCard(employeeID);
			if (!isFist)
			{
				var lastCard = (EPTimecardLite)PXSelectReadonly<EPTimecardLite,
					Where<EPTimecardLite.employeeID, Equal<Required<EPTimecardLite.employeeID>>>,
					OrderBy<Desc<EPTimecardLite.weekId>>>.
					SelectWindowed(this, 0, 1, employeeID);
				if (lastCard != null && lastCard.WeekID != null)
				{
					return PXWeekSelector2Attribute.GetNextWeekID(this, (int)lastCard.WeekID);
				}
			}
			return Accessinfo.BusinessDate.With(_ => PXWeekSelector2Attribute.GetWeekID(this, _));
		}


		public virtual EPTimeCard GetLastCorrection(EPTimeCard source)
		{
			if (source.IsReleased == true)
			{
				EPTimeCard res = Document.Search<EPTimeCard.origTimeCardCD>(source.TimeCardCD);
				if (res != null)
					return GetLastCorrection(res);
			}
			return source;
		}

		public bool CanSelectAllDetailsByTimeCardCD(EPTimeCard document)
		{
			return !(document.IsHold == true && document.IsApproved != true &&
			         document.IsRejected != true && document.IsReleased != true);
		}

		public virtual void ValidateDisplayVSReportedTimeZones(EPTimeCard timeCard)
		{
			PXTimeZoneInfo currentTimeZone = LocaleInfo.GetTimeZone();
			int topCount = 2;
			PXResultset<EPTimecardDetail> queryResult = null;

			if (CanSelectAllDetailsByTimeCardCD(Document.Current))
			{
				queryResult = PXSelectGroupBy<EPTimecardDetail,
									Where<EPTimecardDetail.timeCardCD, Equal<Required<EPTimeCard.timeCardCD>>,
										And<EPTimecardDetail.reportedInTimeZoneID, IsNotNull,
										And<EPTimecardDetail.reportedInTimeZoneID, NotEqual<Required<EPTimecardDetail.reportedInTimeZoneID>>>>>,
									Aggregate<GroupBy<EPTimecardDetail.reportedInTimeZoneID>>,
									OrderBy<Asc<EPTimecardDetail.reportedInTimeZoneID>>>
									.SelectWindowed(this, 0, topCount,
										timeCard.TimeCardCD,
										currentTimeZone?.Id);
			}
			else
			{
				queryResult = PXSelectJoinGroupBy<EPTimecardDetail,
									InnerJoin<CREmployee,
										On<CREmployee.userID, Equal<EPTimecardDetail.ownerID>>>,
									Where<EPTimecardDetail.reportedInTimeZoneID, IsNotNull,
										And<EPTimecardDetail.reportedInTimeZoneID, NotEqual<Required<EPTimecardDetail.reportedInTimeZoneID>>, 
										And<CREmployee.bAccountID, Equal<Required<EPTimeCard.employeeID>>,
										And<EPTimecardDetail.weekID, Equal<Required<EPTimeCard.weekId>>,
										And<EPTimecardDetail.trackTime, Equal<True>,
										And<EPTimecardDetail.approvalStatus, NotEqual<ActivityStatusListAttribute.canceled>,
										And<Where<EPTimecardDetail.timeCardCD, IsNull, Or<EPTimecardDetail.timeCardCD, Equal<Required<EPTimeCard.timeCardCD>>>>>>>>>>>,
									Aggregate<GroupBy<EPTimecardDetail.reportedInTimeZoneID>>,
									OrderBy<Asc<EPTimecardDetail.reportedInTimeZoneID>>>
									.SelectWindowed(this, 0, topCount,
										currentTimeZone?.Id,
										timeCard.EmployeeID,
										timeCard.WeekID,
										timeCard.TimeCardCD);
			}

			EPTimecardDetail[] details = queryResult.RowCast<EPTimecardDetail>().ToArray();

			if (details.Length > 1)
			{
				Document.Cache.RaiseExceptionHandling<EPTimeCard.timeCardCD>(Document.Current, Document.Current.TimeCardCD,
					new PXSetPropertyException(Messages.CurrentUserTimeZoneDiffersFromAtLeastOneActivitiesWereReportedIn, PXErrorLevel.Warning, currentTimeZone.DisplayName));
			}
			else if (details.Length == 1)
			{
				string timeZoneToDisplay = PXTimeZoneInfo.FindSystemTimeZoneById(details.Single().ReportedInTimeZoneID).DisplayName;

				Document.Cache.RaiseExceptionHandling<EPTimeCard.timeCardCD>(Document.Current, Document.Current.TimeCardCD,
					new PXSetPropertyException(Messages.CurrentUserTimeZoneDiffersFromActivitiesWereReportedIn, PXErrorLevel.Warning, currentTimeZone.DisplayName, timeZoneToDisplay));
			}
		}

		public bool PrepareImportRow(string viewName, IDictionary keys, IDictionary values)
		{
			return true;
		}

		public bool RowImporting(string viewName, object row)
		{
			return row == null;
		}

		public bool RowImported(string viewName, object row, object oldRow)
		{
			return oldRow == null;
		}

		public void PrepareItems(string viewName, IEnumerable items) { }

		#endregion

		#region Local Types

		public class DayActivities
		{
			public List<EPTimecardDetail> Activities;
			public DayOfWeek Day;

			public DayActivities()
			{
				Activities = new List<EPTimecardDetail>();
			}

			public int GetTotalTime(int summaryLineNbr)
			{
				int total = 0;
				foreach (EPTimecardDetail activity in Activities)
				{
					if (activity.SummaryLineNbr == null || activity.SummaryLineNbr == summaryLineNbr)
						total += activity.TimeSpent.GetValueOrDefault();
				}

				return total;
			}

			public EPTimecardDetail GetAdjustingActivity(int summaryLineNbr)
			{
				foreach (EPTimecardDetail activity in Activities)
				{
					if (activity.SummaryLineNbr == summaryLineNbr && activity.Released != true)
						return activity;
				}

				if (Activities.Count == 1 && Activities[0].SummaryLineNbr == null && Activities[0].Released != true)
				{
					return Activities[0];
				}

				return null;
			}

		}

		protected class SummaryRecord
		{
			public SummaryRecord(EPTimeCardSummaryWithInfo summary)
			{
				this.Summary = summary;
				LinkedDetails = new List<EPTimecardDetail>();
				NotLinkedDetails = new List<EPTimecardDetail>();
			}

			public EPTimeCardSummaryWithInfo Summary { get; private set; }
			public string SummaryKey { get; set; }
			public List<EPTimecardDetail> LinkedDetails { get; private set; }
			public List<EPTimecardDetail> NotLinkedDetails { get; private set; }

			public EPTimeCardSummary SummariseDetails()
			{
				EPTimeCardSummary keyItem = new EPTimeCardSummary();

				if (Summary != null)
				{
					keyItem.TimeCardCD = Summary.TimeCardCD;
					keyItem.LineNbr = Summary.LineNbr;
				}

				foreach (EPTimecardDetail detail in LinkedDetails)
				{
					AddActivityTimeToSummary(keyItem, detail, 1);
				}

				foreach (EPTimecardDetail detail in NotLinkedDetails)
				{
					AddActivityTimeToSummary(keyItem, detail, 1);
				}

				return keyItem;
			}

			public SummaryRecordInfo GetInfo()
			{
				SummaryRecordInfo info = new SummaryRecordInfo();

				decimal? rate = null;
				bool containsMixedRates = false;
				foreach (EPTimecardDetail detail in LinkedDetails.Union(NotLinkedDetails))
				{
					if (detail.SummaryLineNbr == null)
					{
						info.HasManualDetails = true;
					}

					if (detail.ApprovalStatus == ActivityStatusListAttribute.Open)
					{
						info.HasOpen = true;
					}

					if (detail.ApprovalStatus == ActivityStatusListAttribute.PendingApproval)
					{
						info.HasCompleted = true;
					}

					if (detail.ApprovalStatus == ActivityStatusListAttribute.Approved)
					{
						info.HasApproved = true;
					}

					if (detail.ApprovalStatus == ActivityStatusListAttribute.Rejected)
					{
						info.HasRejected = true;
					}


					if (detail.ApproverID != null)
					{
						info.ApprovalRequired = true;
					}

					if (rate == null && detail.EmployeeRate != null)
					{
						rate = detail.EmployeeRate;
					}
					else if (rate != null && detail.EmployeeRate != null && rate != detail.EmployeeRate)
					{
						containsMixedRates = true;
					}
				}

				info.Rate = rate;
				info.ContainsMixedRates = containsMixedRates;
												
				return info;
			}

			private void AddActivityTimeToSummary(EPTimeCardSummary summary, PMTimeActivity activity, int mult)
			{
				if (activity.TimeSpent != null)
				{
					switch (activity.Date?.DayOfWeek)
					{
						case DayOfWeek.Monday:
							summary.Mon = summary.Mon.GetValueOrDefault() + (mult * activity.TimeSpent.Value);
							break;
						case DayOfWeek.Tuesday:
							summary.Tue = summary.Tue.GetValueOrDefault() + (mult * activity.TimeSpent.Value);
							break;
						case DayOfWeek.Wednesday:
							summary.Wed = summary.Wed.GetValueOrDefault() + (mult * activity.TimeSpent.Value);
							break;
						case DayOfWeek.Thursday:
							summary.Thu = summary.Thu.GetValueOrDefault() + (mult * activity.TimeSpent.Value);
							break;
						case DayOfWeek.Friday:
							summary.Fri = summary.Fri.GetValueOrDefault() + (mult * activity.TimeSpent.Value);
							break;
						case DayOfWeek.Saturday:
							summary.Sat = summary.Sat.GetValueOrDefault() + (mult * activity.TimeSpent.Value);
							break;
						case DayOfWeek.Sunday:
							summary.Sun = summary.Sun.GetValueOrDefault() + (mult * activity.TimeSpent.Value);
							break;
					}
				}
			}

			public struct SummaryRecordInfo
			{
				public bool HasManualDetails { get; set; }
				public bool HasOpen { get; set; }
				public bool HasApproved { get; set; }
				public bool HasRejected { get; set; }
				public bool HasCompleted { get; set; }
				public bool ApprovalRequired { get; set; }
				public decimal? Rate { get; set; }
				public bool ContainsMixedRates { get; set; }

			}
		}

		private class ReleasedActivity
		{
			public EPTimecardDetail Activity { get; private set; }
			public decimal Cost { get; private set; }
			public bool IsBilled { get; private set; }

			public ReleasedActivity(EPTimecardDetail activity, decimal cost, bool isBilled)
			{
				this.Activity = activity;
				this.Cost = cost;
				this.IsBilled = isBilled;
			}
		}

		/// <summary>
		/// Required for correct join
		/// </summary>
		[PXHidden]
		[Serializable]
		public class EPTimecardDetailOrig : EPTimecardDetail
		{
			public new abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
			public new abstract class refNoteID : PX.Data.BQL.BqlGuid.Field<refNoteID> { }
			public new abstract class origNoteID : PX.Data.BQL.BqlGuid.Field<origNoteID> { }
		}

		/// <summary>
		/// Required for correct join
		/// </summary>
		[PXHidden]
		[Serializable]
		public class EPTimecardDetailEx : EPTimecardDetail
		{
			public new abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
			public new abstract class origNoteID : PX.Data.BQL.BqlGuid.Field<origNoteID> { }
		}
				
		[Serializable]
		[PXBreakInheritance]
		[PXProjection(typeof(Select2<PMTimeActivity,
			LeftJoin<CRActivityLink,
				On<CRActivityLink.noteID, Equal<PMTimeActivity.refNoteID>>,
			LeftJoin<CRCase,
				On<CRCase.noteID, Equal<CRActivityLink.refNoteID>>,
			LeftJoin<ContractEx,
				On<ContractEx.contractID, Equal<CRCase.contractID>>,
			LeftJoin<PMProject,
				On<PMProject.contractID, Equal<PMTimeActivity.projectID>>>>>>>), new Type[] { typeof(PMTimeActivity) })]
		public class EPTimecardDetail : PMTimeActivity
		{
			//PMTimeActivity overrides:

			#region ParentTaskNoteID
			public new abstract class parentTaskNoteID : PX.Data.BQL.BqlGuid.Field<parentTaskNoteID> { }

			[PXDBGuid()]
			[PXDBDefault(null, PersistingCheck = PXPersistingCheck.Nothing)]
			[CRTaskSelector]
			[PXRestrictor(typeof(Where<CRActivity.classID, Equal<CRActivityClass.task>>), null)]
			[PXUIField(DisplayName = "Task")]
			public override Guid? ParentTaskNoteID { get; set; }
			#endregion

			#region TimeCardCD
			public new abstract class timeCardCD : PX.Data.BQL.BqlString.Field<timeCardCD> { }

			[PXDBString(10)]
			[PXUIField(Visible = false)]
			public override string TimeCardCD { get; set; }
			#endregion

			#region ProjectID
			public new abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }
			[EPTimeCardProject()]
			public override int? ProjectID { get; set; }
			#endregion

			#region ProjectTaskID
			public new abstract class projectTaskID : PX.Data.BQL.BqlInt.Field<projectTaskID> { }
			[PXDefault(typeof(Search<PMTask.taskID, Where<PMTask.projectID, Equal<Current<projectID>>, And<PMTask.isDefault, Equal<True>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
			[ProjectTask(typeof(projectID), BatchModule.TA, DisplayName = "Project Task")]
			public override int? ProjectTaskID { get; set; }
			#endregion

			#region CostCodeID
			public new abstract class costCodeID : PX.Data.BQL.BqlInt.Field<costCodeID> { }
			[CostCode(null, typeof(projectTaskID), GL.AccountType.Expense, ReleasedField = typeof(released))]
			public override Int32? CostCodeID
			{
				get;
				set;
			}
			#endregion

			#region UnionID
			public new abstract class unionID : PX.Data.BQL.BqlString.Field<unionID> { }

			[PMUnion(typeof(projectID), typeof(Select<EPEmployee, Where<EPEmployee.bAccountID, Equal<Current<EPTimeCard.employeeID>>>>))]
			public override String UnionID
			{
				get;
				set;
			}
			#endregion

			#region WeekID
			public new abstract class weekID : PX.Data.BQL.BqlInt.Field<weekID> { }

			[PXDBInt()]
			[PXUIField(DisplayName = "Time Card Week")]
			public override int? WeekID { get; set; }
			#endregion

			#region LabourItemID
			public new abstract class labourItemID : PX.Data.BQL.BqlInt.Field<labourItemID> { }

			[PMLaborItem(typeof(projectID), typeof(earningTypeID), typeof(Select<EPEmployee, Where<EPEmployee.bAccountID, Equal<Current<EPTimeCard.employeeID>>>>))]
			public override int? LabourItemID { get; set; }
			#endregion

			#region TimeSpent
			public new abstract class timeSpent : PX.Data.BQL.BqlInt.Field<timeSpent> { }

			[PXTimeList]
			[PXDefault(0)]
			[PXDBInt()]
			[PXUIField(DisplayName = "Time Spent")]
			public override int? TimeSpent
			{
				get;
				set;
			}
			#endregion

			#region IsBillable
			public new abstract class isBillable : PX.Data.BQL.BqlBool.Field<isBillable> { }

			[PXDBBool()]
			[PXUIField(DisplayName = "Billable")]
			public override Boolean? IsBillable { get; set; }
			#endregion

			#region TimeBillable
			public new abstract class timeBillable : PX.Data.BQL.BqlInt.Field<timeBillable> { }

			[PXTimeList]
			[PXDefault(0)]
			[PXDBInt()]
			[PXUIEnabled(typeof(isBillable))]
			[PXUIVerify(typeof(Where<timeSpent, IsNull, Or<timeBillable, IsNull, Or<timeSpent, GreaterEqual<timeBillable>, Or<isBillable, Equal<False>>>>>), PXErrorLevel.Error, CR.Messages.BillableTimeCannotBeGreaterThanTimeSpent)]
			[PXFormula(typeof(
				Switch<Case<Where<isBillable, Equal<True>>, timeSpent,
							 Case<Where<isBillable, Equal<False>>, int0>>,
							 timeBillable>))]
			[PXUIField(DisplayName = "Billable Time", FieldClass = "BILLABLE")]
			public override int? TimeBillable { get; set; }
			#endregion

			#region tstamp
			public new abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }

			[PXDBTimestamp(RecordComesFirst = true)]
			public override byte[] tstamp { get; set; }
            #endregion

            //Case:
            #region CaseCD
            /// <summary>
			/// CaseCD field
			/// </summary>
            public abstract class caseCD : PX.Data.BQL.BqlString.Field<caseCD> { }
			/// <summary>
			/// Gets sets Case Number
			/// </summary>
			[PXDBString(10, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC", BqlField = typeof(CRCase.caseCD))]
			[PXUIField(DisplayName = "Case ID", Visibility = PXUIVisibility.SelectorVisible)]
			[PXSelector(typeof(Search3<CRCase.caseCD,
				LeftJoin<BAccount, On<BAccount.bAccountID, Equal<CRCase.customerID>>>,
				OrderBy<Desc<CRCase.caseCD>>>),
				typeof(CRCase.caseCD),
				typeof(CRCase.subject),
				typeof(CRCase.status),
				typeof(CRCase.priority),
				typeof(CRCase.severity),
				typeof(CRCase.caseClassID),
				typeof(BAccount.acctName),
				Filterable = true)]
			public virtual String CaseCD { get; set; }
            #endregion

            //ContractEx:
            #region ContractCD
            /// <summary>
            /// ContractCD field
            /// </summary>
            public abstract class contractCD : PX.Data.BQL.BqlString.Field<contractCD> { }
			/// <summary>
			/// Gets sets Contract CD
			/// </summary>
			[PXDimensionSelector(ContractAttribute.DimensionName,
				typeof(Search2<contractCD, InnerJoin<ContractBillingSchedule, On<contractID, Equal<ContractBillingSchedule.contractID>>, LeftJoin<AR.Customer, On<AR.Customer.bAccountID, Equal<ContractEx.customerID>>>>
				, Where<ContractEx.baseType, Equal<CTPRType.contract>>>),
				typeof(contractCD),
				typeof(contractCD), typeof(ContractEx.customerID), typeof(AR.Customer.acctName), typeof(ContractEx.locationID), typeof(ContractEx.description), typeof(ContractEx.status), typeof(ContractEx.expireDate), typeof(ContractBillingSchedule.lastDate), typeof(ContractBillingSchedule.nextDate), DescriptionField = typeof(ContractEx.description), Filterable = true)]
			[PXDBString(IsUnicode = true, InputMask = "", BqlField = typeof(ContractEx.contractCD))]
			[PXUIField(DisplayName = "Contract ID", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
			public virtual string ContractCD { get; set; }
			#endregion


			#region Unbound Fields (Calculated in the TimecardMaint graph)

			public abstract class day : PX.Data.BQL.BqlString.Field<day> { }
			[PXString()]
			[PXUIField(DisplayName = "Day")]
			//[PXStringList(new string[] { "1", "2", "3", "4", "5", "6", "0" }, new string[] { "Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun" })]
			public virtual string Day { get; set; }


			public abstract class isOvertimeCalc : PX.Data.BQL.BqlBool.Field<isOvertimeCalc> { }
			[PXBool()]
			[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
			public virtual bool? IsOvertimeCalc { get; set; }


			public abstract class overtimeMultiplierCalc : PX.Data.BQL.BqlDecimal.Field<overtimeMultiplierCalc> { }
			[PXDecimal(2)]
			[PXUIField(DisplayName = "OT Mult", Enabled = false)]
			public virtual decimal? OvertimeMultiplierCalc { get; set; }


			public abstract class billableTimeCalc : PX.Data.BQL.BqlInt.Field<billableTimeCalc> { }
			[PXTimeList]
			[PXInt]
			[PXUIField(DisplayName = "Billable Time")]
			public virtual int? BillableTimeCalc { get; set; }


			public abstract class billableOvertimeCalc : PX.Data.BQL.BqlInt.Field<billableOvertimeCalc> { }
			[PXTimeList]
			[PXInt]
			[PXUIField(DisplayName = "Billable OT")]
			public virtual int? BillableOvertimeCalc { get; set; }


			public abstract class regularTimeCalc : PX.Data.BQL.BqlInt.Field<regularTimeCalc> { }
			[PXTimeList]
			[PXInt]
			[PXUIField(DisplayName = "RH", Enabled = false)]
			public virtual int? RegularTimeCalc { get; set; }


			public abstract class overTimeCalc : PX.Data.BQL.BqlInt.Field<overTimeCalc> { }
			[PXTimeList]
			[PXInt]
			[PXUIField(DisplayName = "OT", Enabled = false)]
			public virtual int? OverTimeCalc { get; set; }
			#endregion

		}

		/// <summary>
		/// Timecard Summary Record projection with additional fields from Project and Project Task.
		/// </summary>
		[PXCacheName(Messages.TimeCardSummary)]
		[Serializable]
		[PXBreakInheritance]
		[PXProjection(typeof(Select2<EPTimeCardSummary,
			InnerJoin<PMProject,
				On<EPTimeCardSummary.projectID, Equal<PMProject.contractID>>,
			LeftJoin<PMTask,
				On<EPTimeCardSummary.projectTaskID, Equal<PMTask.taskID>>>>>), new Type[] { typeof(EPTimeCardSummary)})]
		public class EPTimeCardSummaryWithInfo : EPTimeCardSummary
		{
            #region ProjectID
            /// <summary>
            ///  ProjectID field
            /// </summary>
            public new abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }
			/// <summary>
			/// Gets sets ProjectID
			/// </summary>
			[ProjectDefault(BatchModule.TA, ForceProjectExplicitly = true)]
			[EPTimeCardProject]
			public override int? ProjectID { get; set; }
            #endregion

            #region ProjectTaskID
            /// <summary>
            ///  ProjectTaskID field
            /// </summary>
            public new abstract class projectTaskID : PX.Data.BQL.BqlInt.Field<projectTaskID> { }
			/// <summary>
			/// Gets sets Project Task
			/// </summary>
			[PXDefault(typeof(Search<PMTask.taskID, Where<PMTask.projectID, Equal<Current<projectID>>, And<PMTask.isDefault, Equal<True>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
			[EPTimecardProjectTask(typeof(projectID), BatchModule.TA, DisplayName = "Project Task")]
			public override int? ProjectTaskID { get; set; }
            #endregion

            #region CostCodeID
            /// <summary>
            ///  CostCodeID field
            /// </summary>
            public new abstract class costCodeID : PX.Data.BQL.BqlInt.Field<costCodeID> { }
			/// <summary>
			/// Gets sets CostCode
			/// </summary>
			[CostCode(null, typeof(projectTaskID), GL.AccountType.Expense)]
			public override Int32? CostCodeID
			{
				get;
				set;
			}
            #endregion

            #region CertifiedJob
            /// <summary>
            ///  CertifiedJob field
            /// </summary>
            public new abstract class certifiedJob : PX.Data.BQL.BqlBool.Field<certifiedJob> { }
			/// <summary>
			/// Gets sets Certified Job
			/// </summary>
			[PXDBBool()]
			[PXDefault(typeof(Coalesce<Search<PMProject.certifiedJob, Where<PMProject.contractID, Equal<Current<projectID>>>>,
				Search<PMProject.certifiedJob, Where<PMProject.nonProject, Equal<True>>>>))]
			[PXUIField(DisplayName = "Certified Job", FieldClass = nameof(FeaturesSet.Construction))]
			public override Boolean? CertifiedJob
			{
				get; set;
			}
            #endregion

            #region UnionID
            /// <summary>
            ///  UnionID field
            /// </summary>
            public new abstract class unionID : PX.Data.BQL.BqlString.Field<unionID> { }
			/// <summary>
			/// Gets sets Union
			/// </summary>
			[PMUnion(typeof(projectID), typeof(Select<EPEmployee, Where<EPEmployee.bAccountID, Equal<Current<EPTimeCard.employeeID>>>>))]
			public override String UnionID
			{
				get;
				set;
			}
            #endregion

            #region LabourItemID
            /// <summary>
            ///  LabourItemID field
            /// </summary>
            public new abstract class labourItemID : PX.Data.BQL.BqlInt.Field<labourItemID> { }
			/// <summary>
			/// Gets sets Labor Item
			/// </summary>
			[PMLaborItem(typeof(projectID), typeof(earningType), typeof(Select<EPEmployee, Where<EPEmployee.bAccountID, Equal<Current<EPTimeCard.employeeID>>>>))]
			public override int? LabourItemID { get; set; }
            #endregion
            #region WorkCodeID
            /// <summary>
            ///  WorkCodeID field
            /// </summary>
            public new abstract class workCodeID : PX.Data.BQL.BqlString.Field<workCodeID> { }
			/// <summary>
			/// Gets sets Work code
			/// </summary>
			[PMWorkCode(typeof(costCodeID))]
			public override String WorkCodeID
			{
				get; set;
			}
            #endregion

            //Project:

            #region ProjectDescription
            /// <summary>
            ///  Project's Description field
            /// </summary>
            public abstract class projectDescription : PX.Data.BQL.BqlString.Field<projectDescription> { }
			/// <summary>
			/// Gets sets Project's Description
			/// </summary>
			[PXUIField(DisplayName = "Project Description", IsReadOnly = true)]
			[PXDBString(BqlField = typeof(PMProject.description))]
			public virtual string ProjectDescription { get; set; }
            #endregion

            #region ProjectManager
            /// <summary>
            ///  Project Manager field
            /// </summary>
            public abstract class projectManager : PX.Data.BQL.BqlInt.Field<projectManager> { }
			/// <summary>
			/// Gets sets Project Manager
			/// </summary>
			[PXDBInt(BqlField = typeof(PMProject.approverID))]
			[PXUIField(DisplayName = "Project Manager", Visibility = PXUIVisibility.SelectorVisible)]
			public virtual Int32? ProjectManager
			{
				get;
				set;
			}
            #endregion

            //Task:

            #region ProjectTaskDescription
            /// <summary>
            ///  Project Task's description field
            /// </summary>
            public abstract class projectTaskDescription : PX.Data.BQL.BqlString.Field<projectTaskDescription> { }
			/// <summary>
			/// Gets sets Project Task's description
			/// </summary>
			[PXUIField(DisplayName = "Project Task Description", IsReadOnly = true)]
			[PXDBString(BqlField = typeof(PMTask.description))]
			public virtual string ProjectTaskDescription { get; set; }
            #endregion

            #region 
            /// <summary>
            ///  Task Approver field
            /// </summary>
            public abstract class taskApproverID : PX.Data.BQL.BqlInt.Field<taskApproverID> { }

			/// <summary>
			/// Gets sets Task Approver
			/// </summary>
			[PXDBInt(BqlField = typeof(PMTask.approverID))]
			[PXEPEmployeeSelector]
			public virtual Int32? TaskApproverID
			{
				get;
				set;
			}
            #endregion

            //Unbound Fields:

            #region 
            /// <summary>
            ///   Approval Status field
            /// </summary>
            public abstract class approvalStatus : PX.Data.BQL.BqlString.Field<approvalStatus> { }
			/// <summary>
			/// Gets sets Approval Status
			/// </summary>
			[PXString(2, IsFixed = true)]
			[ApprovalStatusList]
			[PXUIField(DisplayName = "Approval Status", Enabled = false)]
			public virtual string ApprovalStatus { get; set; }
            #endregion

            #region ApproverID
            /// <summary>
            ///  Approver (Task or Project) field
            /// </summary>
            public abstract class approverID : PX.Data.BQL.BqlInt.Field<approverID> { }

			/// <summary>
			/// Gets sets Approver (Task or Project)
			/// </summary>
			[PXInt]
			[PXEPEmployeeSelector]
			[PXFormula(typeof(
				Switch<
					Case<Where<taskApproverID, IsNotNull>, taskApproverID>,
					Case<Where<projectManager, IsNotNull>, projectManager>>
				))]
			[PXUIField(DisplayName = "Approver", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
			public virtual int? ApproverID { get; set; }
			#endregion
		}

		[PXHidden]
		[Serializable]
		[PXBreakInheritance]
		public class EPTimecardTask : CRPMTimeActivity
		{
			#region CRActivity

			#region StartDate
			public new abstract class startDate : PX.Data.BQL.BqlDateTime.Field<startDate> { }

			[PXDefault]
			[EPDBDateAndTime(typeof(ownerID), WithoutDisplayNames = true, BqlField = typeof(CRActivity.startDate))]
			[PXUIField(DisplayName = "Date")]
			public override DateTime? StartDate { get; set; }
			#endregion

			#region simple BQL Fields override

			public new abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
			public new abstract class parentNoteID : PX.Data.BQL.BqlGuid.Field<parentNoteID> { }
			public new abstract class ownerID : PX.Data.BQL.BqlGuid.Field<ownerID> { }
			public new abstract class classID : PX.Data.BQL.BqlInt.Field<classID> { }
			public new abstract class type : PX.Data.BQL.BqlString.Field<type> { }
			public new abstract class uistatus : PX.Data.BQL.BqlString.Field<uistatus> { }
			public new abstract class endDate : PX.Data.BQL.BqlDateTime.Field<endDate> { }

			#endregion

			#endregion

			#region PMTimeActivity

			#region TimeCardCD
			public new abstract class timeCardCD : PX.Data.BQL.BqlString.Field<timeCardCD> { }

			[PXDBString(10, BqlField = typeof(PMTimeActivity.timeCardCD))]
			[PXUIField(Visible = false)]
			public override string TimeCardCD { get; set; }
			#endregion

			#region ProjectID
			public new abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }

			[Project(BqlField = typeof(PMTimeActivity.projectID))]
			public override int? ProjectID { get; set; }
			#endregion

			#region ProjectTaskID
			public new abstract class projectTaskID : PX.Data.BQL.BqlInt.Field<projectTaskID> { }
			[ProjectTask(typeof(projectID), BatchModule.TA, DisplayName = "Project Task", BqlField = typeof(PMTimeActivity.projectTaskID))]
			public override int? ProjectTaskID { get; set; }
			#endregion

			#region CostCodeID
			public new abstract class costCodeID : PX.Data.BQL.BqlInt.Field<costCodeID> { }

			[CostCode(null, typeof(projectTaskID), GL.AccountType.Expense, BqlField = typeof(PMTimeActivity.costCodeID), ReleasedField = typeof(released))]
			public override int? CostCodeID { get; set; }
			#endregion

			#region WeekID
			public new abstract class weekID : PX.Data.BQL.BqlInt.Field<weekID> { }

			[PXDBInt(BqlField = typeof(PMTimeActivity.weekID))]
			[PXUIField(DisplayName = "Time Card Week")]
			public override int? WeekID { get; set; }
			#endregion

			#region TimeSpent
			public new abstract class timeSpent : PX.Data.BQL.BqlInt.Field<timeSpent> { }

			[PXTimeList]
			[PXDBInt(BqlField = typeof(PMTimeActivity.timeSpent))]
			[PXUIField(DisplayName = "Time Spent")]
			public override int? TimeSpent { get; set; }
			#endregion

			#region simple BQL Fields override

			public new abstract class origNoteID : PX.Data.BQL.BqlGuid.Field<origNoteID> { }
			public new abstract class overtimeSpent : PX.Data.BQL.BqlInt.Field<overtimeSpent> { }
			public new abstract class summaryLineNbr : PX.Data.BQL.BqlInt.Field<summaryLineNbr> { }

			#endregion

			#endregion



			#region Unbound Fields (Calculated in the TimecardMaint graph)

			public abstract class isOvertimeCalc : PX.Data.BQL.BqlBool.Field<isOvertimeCalc> { }
			[PXBool()]
			[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
			public virtual bool? IsOvertimeCalc { get; set; }

			public abstract class overtimeMultiplierCalc : PX.Data.BQL.BqlDecimal.Field<overtimeMultiplierCalc> { }
			[PXDecimal(1)]
			[PXUIField(DisplayName = "OT Mult")]
			public virtual decimal? OvertimeMultiplierCalc { get; set; }

			public abstract class billableTimeCalc : PX.Data.BQL.BqlInt.Field<billableTimeCalc> { }
			[PXInt]
			[PXUIField(DisplayName = "Billable Time")]
			public virtual int? BillableTimeCalc { get; set; }

			public abstract class billableOvertimeCalc : PX.Data.BQL.BqlInt.Field<billableOvertimeCalc> { }
			[PXInt]
			[PXUIField(DisplayName = "Billable OT")]
			public virtual int? BillableOvertimeCalc { get; set; }

			public abstract class regularTimeCalc : PX.Data.BQL.BqlInt.Field<regularTimeCalc> { }
			[PXInt]
			[PXUIField(DisplayName = "RH", Enabled = false)]
			public virtual int? RegularTimeCalc { get; set; }


			public abstract class overTimeCalc : PX.Data.BQL.BqlInt.Field<overTimeCalc> { }
			[PXInt]
			[PXUIField(DisplayName = "OT", Enabled = false)]
			public virtual int? OverTimeCalc { get; set; }

			#endregion
		}

		[Serializable]
		[PXHidden]
		public class EPTimeCardItemOrig : EPTimeCardItem
		{
			public new abstract class timeCardCD : PX.Data.BQL.BqlString.Field<timeCardCD> { }
			public new abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }
			public new abstract class origLineNbr : PX.Data.BQL.BqlInt.Field<origLineNbr> { }
		}

		[Serializable]
		[PXHidden]
		public class EPTimeCardItemEx : EPTimeCardItem
		{
			public new abstract class timeCardCD : PX.Data.BQL.BqlString.Field<timeCardCD> { }
			public new abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }
			public new abstract class origLineNbr : PX.Data.BQL.BqlInt.Field<origLineNbr> { }
		}

		[Serializable]
		public class ContractEx : Contract
		{
			public new abstract class contractID : PX.Data.BQL.BqlInt.Field<contractID> { }

			[Obsolete(Common.InternalMessages.PropertyIsObsoleteAndWillBeRemoved2019R2)]
			public new abstract class isTemplate : PX.Data.BQL.BqlBool.Field<isTemplate> { }
			public new abstract class baseType : PX.Data.BQL.BqlString.Field<baseType> { }
			public new abstract class description : PX.Data.BQL.BqlString.Field<description> { }
			public new abstract class customerID : PX.Data.BQL.BqlInt.Field<customerID> { }
			public new abstract class status : PX.Data.BQL.BqlString.Field<status> { }
			public new abstract class expireDate : PX.Data.BQL.BqlDateTime.Field<expireDate> { }

			#region ContractCD
			public new abstract class contractCD : PX.Data.BQL.BqlString.Field<contractCD> { }

			[PXDimensionSelector(ContractAttribute.DimensionName,
				typeof(Search2<contractCD, InnerJoin<ContractBillingSchedule, On<contractID, Equal<ContractBillingSchedule.contractID>>, LeftJoin<AR.Customer, On<AR.Customer.bAccountID, Equal<customerID>>>>
				, Where<baseType, Equal<CTPRType.contract>>>),
				typeof(contractCD),
				typeof(contractCD), typeof(customerID), typeof(AR.Customer.acctName), typeof(Contract.locationID), typeof(description), typeof(status), typeof(expireDate), typeof(ContractBillingSchedule.lastDate), typeof(ContractBillingSchedule.nextDate), DescriptionField = typeof(description), Filterable = true)]
			[PXDBString(IsUnicode = true, IsKey = true, InputMask = "")]
			[PXUIField(DisplayName = "Contract ID", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
			public override string ContractCD { get; set; }
			#endregion
		}

		[Serializable]
		public enum ActivityValidationError
		{
			ActivityIsNotCompleted,
			ActivityIsRejected,
			ActivityIsNotApproved,
			ProjectIsNotActive,
			ProjectIsCompleted,
			ProjectTaskIsCancelled,
			ProjectTaskIsCompleted,
			ProjectTaskIsNotActive,
			LaborClassNotSpecified,
			OvertimeLaborClassNotSpecified
		}

		public class TimeCardSummaryCopiedInfo
		{
			public string Description { get; set; }
			public string Note { get; set; }

			public TimeCardSummaryCopiedInfo(string description, string note)
			{
				Description = description;
				Note = note;
			}
		}

		#endregion
	}
}
