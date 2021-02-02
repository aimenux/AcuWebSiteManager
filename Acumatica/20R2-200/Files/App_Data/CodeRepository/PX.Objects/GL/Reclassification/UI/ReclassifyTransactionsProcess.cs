using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using PX.Api;
using PX.Common;
using PX.Data;
using PX.Objects.CM;
using PX.Objects.Common;
using PX.Objects.CR;
using PX.Objects.GL.Reclassification.Common;
using PX.Objects.GL.Reclassification.Processing;
using PX.Objects.GL.Attributes;
using PX.Objects.GL.FinPeriods;
using PX.Objects.PM;
using PX.Web.UI;
using PX.Objects.Common.Tools;
using PX.Objects.GL.FinPeriods.TableDefinition;

namespace PX.Objects.GL.Reclassification.UI
{
	public class ReclassifyTransactionsProcess : ReclassifyTransactionsBase<ReclassifyTransactionsProcess>
	{
		public PXCancel<GLTranForReclassification> Cancel;
		public PXDelete<GLTranForReclassification> Delete;
        public PXAction<GLTranForReclassification> showLoadTransPopup;
		public PXAction<GLTranForReclassification> reloadTrans;
		public PXAction<GLTranForReclassification> loadTrans;
		public PXAction<GLTranForReclassification> replace;
        public PXAction<GLTranForReclassification> split;
        public PXAction<GLTranForReclassification> validateAndProcess;

        public PXProcessing<GLTranForReclassification,
            Where<True, Equal<True>>,
            OrderBy<Asc<GLTranForReclassification.sortOrder>>> GLTranForReclass;

        public PXSelectJoin<GLTranForReclassification,
						InnerJoin<CurrencyInfo,
							On<GLTran.curyInfoID, Equal<CurrencyInfo.curyInfoID>>>,
						Where<GLTranForReclassification.module, Equal<Required<GLTranForReclassification.module>>,
							And<GLTranForReclassification.batchNbr, Equal<Required<GLTranForReclassification.batchNbr>>,
							And<GLTranForReclassification.lineNbr, Equal<Required<GLTranForReclassification.lineNbr>>,
							And<GLTranForReclassification.reclassBatchNbr, IsNull>>>>> 
						GLTranForReclassWithCuryInfo;

		public PXSelect<GLTranForReclassification,
						Where<GLTranForReclassification.module, Equal<Required<GLTranForReclassification.module>>,
							And<GLTranForReclassification.batchNbr, Equal<Required<GLTranForReclassification.batchNbr>>,
							And<GLTranForReclassification.lineNbr, Equal<Required<GLTranForReclassification.lineNbr>>>>>>
						GLTranForReclass_Module_BatchNbr_LineNbr;

		public PXSelect<GLTranForReclassification,
							Where<GLTranForReclassification.module, Equal<Required<GLTranForReclassification.module>>,
									And<GLTranForReclassification.batchNbr, Equal<Required<GLTranForReclassification.batchNbr>>,
									And<GLTranForReclassification.isReclassReverse, Equal<False>,
									And<GLTranForReclassification.isInterCompany, Equal<False>>>>>>
							GLTransForReclassForReverseView;

		public PXSelect<GLTran,
							Where<GLTran.module, Equal<Required<GLTran.module>>,
									And<GLTran.batchNbr, Equal<Required<GLTran.batchNbr>>,
									And<GLTran.isReclassReverse, Equal<True>>>>>
							ReclassReverseGLTransView;

		public PXFilter<LoadOptions> LoadOptionsView;
		public PXFilter<ReplaceOptions> ReplaceOptionsView;
		
		public PXSelect<BAccountR> BAccountRView;
        public PXSelect<Batch> Batch;
        public PXSelect<CurrencyInfo> currencyInfo;

        private const string ScheduleActionKey = "Schedule";

		private static string[] _emptyStringReplaceWildCards = {"\"\"", "''"};

		[InjectDependency]
		public IFinPeriodRepository FinPeriodRepository { get; set; }

		protected IEnumerable<GLTranForReclassification> GetUpdatedTranForReclass()
		{
            return GLTranForReclass.Cache.Updated.OfType<GLTranForReclassification>();
		}

		protected static Type[] EditableFields = new[]
		{
			typeof (GLTranForReclassification.newBranchID),
			typeof (GLTranForReclassification.newAccountID),
			typeof (GLTranForReclassification.newSubID),
			typeof (GLTranForReclassification.newTranDate),
			typeof (GLTranForReclassification.curyNewAmt)
		};

		public ReclassifyTransactionsProcess()
		{
            Actions[ScheduleActionKey].SetVisible(false);

            GLTranForReclass.SetSelected<GLTranForReclassification.selected>();
            GLTranForReclass.SetProcessCaption(Messages.Process);
            GLTranForReclass.SetProcessVisible(false);
            GLTranForReclass.SetProcessAllVisible(false);

            PXUIFieldAttribute.SetVisible<GLTranForReclassification.refNbr>(GLTranForReclass.Cache, null, false);
			PXUIFieldAttribute.SetVisible<GLTranForReclassification.selected>(GLTranForReclass.Cache, null, true);

			PXUIFieldAttribute.SetVisibility<BAccountR.acctReferenceNbr>(BAccountRView.Cache, null, PXUIVisibility.Visible);
			PXUIFieldAttribute.SetVisibility<BAccountR.parentBAccountID>(BAccountRView.Cache, null, PXUIVisibility.Visible);
			PXUIFieldAttribute.SetVisibility<BAccountR.ownerID>(BAccountRView.Cache, null, PXUIVisibility.Visible);

			var opt = LoadOptionsView.Current;

			showLoadTransPopup.StateSelectingEvents += LoadOptionsButtonFieldSelectingHandler;
			validateAndProcess.StateSelectingEvents += ProcessButtonFieldSelectingHandler;
			loadTrans.StateSelectingEvents += ButtonsFieldSelectingHandlerForDisableAfterProcess;
			replace.StateSelectingEvents += DependingOnRowExistanceButtonsSelectingHandler;
            split.StateSelectingEvents += DependingOnRowExistanceButtonsSelectingHandler;
            Delete.StateSelectingEvents += DependingOnRowExistanceButtonsSelectingHandler;
			reloadTrans.StateSelectingEvents += ReloadTransButtonStateSelectingHandler;
		}

        protected virtual IEnumerable glTranForReclass()
        {
            int order = 0;

            var tranForReclass = GetUpdatedTranForReclass();

            foreach (GLTranForReclassification tran in tranForReclass)
            {
				if(tran.IsSplitting)
				{
					continue;
				}

                tran.SortOrder = order++;

                var tranKey = new GLTranKey(tran);

                if (State.SplittingGroups.Keys.Contains(tranKey))
                {
                    foreach(GLTranKey childKey in State.SplittingGroups[tranKey])
                    {
                        GLTranForReclassification childItem = GLTranForReclass_Module_BatchNbr_LineNbr.Locate(GetGLTranForReclassByKey(childKey));
                        childItem.SortOrder = order++;
                    }
                }
            }

            return tranForReclass;
        }

        protected virtual void GLTranForReclassification_ReclassBatchNbr_FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			TimeSpan timespan;
			Exception ex;
			PXLongRunStatus status = PXLongOperation.GetStatus(this.UID, out timespan, out ex);

            PXUIFieldAttribute.SetVisible<GLTranForReclassification.reclassBatchNbr>(GLTranForReclass.Cache, null, status == PXLongRunStatus.Completed || State.ReclassScreenMode == ReclassScreenMode.Editing);

            var row = (GLTranForReclassification)e.Row;

            if (row?.IsSplitting == false)
            {
                return;
            }

            e.ReturnValue = "";
        }
		#region Event Handlers

		#region GLTranForReclassification

		protected virtual void ReclassGraphState_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
            var state = PXCache<ReclassGraphState>.CreateCopy(State);
            GLTranForReclass.SetProcessDelegate(delegate (List<GLTranForReclassification> transForReclass)
            {
                var graph = CreateInstance<ReclassifyTransactionsProcessor>();

                graph.ProcessTransForReclassification(transForReclass, state);
            });
        }

		protected virtual void GLTranForReclassification_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			var tran = e.Row as GLTranForReclassification;

			if (tran == null)
				return;

			bool canEdit = !PXLongOperation.Exists(UID);
			bool enableForParent = tran.IsSplitted != true || (tran.IsSplitted == true && IsReclassAttrChanged(tran));
			bool isTranDateAndDescEnabled = IsTranDateAndDescEnabled(tran);

			PXUIFieldAttribute.SetEnabled<GLTranForReclassification.selected>(cache, tran, tran.Selected == true && canEdit && enableForParent);
			PXUIFieldAttribute.SetEnabled<GLTranForReclassification.newBranchID>(cache, tran, canEdit);
			PXUIFieldAttribute.SetEnabled<GLTranForReclassification.newAccountID>(cache, tran, canEdit);
			PXUIFieldAttribute.SetEnabled<GLTranForReclassification.newSubID>(cache, tran, canEdit);
			PXUIFieldAttribute.SetEnabled<GLTranForReclassification.newFinPeriodID>(cache, tran, canEdit);
			PXUIFieldAttribute.SetEnabled<GLTranForReclassification.newTranDate>(cache, tran, isTranDateAndDescEnabled);
            PXUIFieldAttribute.SetEnabled<GLTranForReclassification.newTranDesc>(cache, tran, isTranDateAndDescEnabled);
            PXUIFieldAttribute.SetEnabled<GLTranForReclassification.curyNewAmt>(cache, tran, tran.IsSplitting == true);

			if (!tran.VerifyingForFromValuesInvoked)
			{
				VerifyAndRememberErorrForNewFields(tran);

				tran.VerifyingForFromValuesInvoked = true;
			}

			IsDateWithinPeriod(cache, tran);
			SetErrorToNewFieldsIfNeed(tran);

			bool splitFieldsVisibility = State.SplittingGroups.Any();
            PXUIFieldAttribute.SetVisible<GLTranForReclassification.curyNewAmt>(cache, null, splitFieldsVisibility);
            PXUIFieldAttribute.SetVisible<GLTranForReclassification.splittedIcon>(cache, null, splitFieldsVisibility);
		}

		/// <summary>
		/// Setting of the error to "New" fields if they contain values are equal to invalid values from "From" fields.
		/// </summary>
		private void SetErrorToNewFieldsIfNeed(GLTranForReclassification tran)
		{
			foreach (var fieldNameErrorPair in tran.FieldsErrorForInvalidFromValues)
			{
				if (fieldNameErrorPair.Value != null)
				{
					object curValue = null;

					if (fieldNameErrorPair.Key == typeof(GLTranForReclassification.newBranchID).Name)
					{
						curValue = tran.NewBranchID;
					}
					else if (fieldNameErrorPair.Key == typeof(GLTranForReclassification.newAccountID).Name)
					{
						curValue = tran.NewAccountID;
					}
					else if (fieldNameErrorPair.Key == typeof(GLTranForReclassification.newSubID).Name)
					{
						curValue = tran.NewSubID;
					}
					else if (fieldNameErrorPair.Key == typeof(GLTranForReclassification.newTranDate).Name)
					{
						curValue = tran.NewTranDate;
					}
					else if (fieldNameErrorPair.Key == typeof(GLTranForReclassification.newFinPeriodID).Name)
					{
						curValue = tran.NewFinPeriodID;
					}
					else if (fieldNameErrorPair.Key == typeof(GLTranForReclassification.curyNewAmt).Name)
					{
						curValue = tran.CuryNewAmt;
					}

					if (curValue != null && curValue.Equals(fieldNameErrorPair.Value.ErrorValue)
					    || curValue == null && fieldNameErrorPair.Value.ErrorValue == null)
					{
						GLTranForReclass.Cache.RaiseExceptionHandling(fieldNameErrorPair.Key, tran, fieldNameErrorPair.Value.ErrorUIValue,
							fieldNameErrorPair.Value.Error);
					}
				}
			}
		}

        protected virtual void GLTranForReclassification_Selected_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			var tranForReclass = (GLTranForReclassification) e.Row;

			if (e.ExternalCall && tranForReclass.Selected == false)
			{
				InitTranForReclassEditableFields(tranForReclass);
			}

			if (tranForReclass.ReclassRowType == ReclassRowTypes.Editing)
			{
				if (tranForReclass.Selected == true)
				{
					State.GLTranForReclassToDelete.Remove(tranForReclass);
                }
                else
				{
					State.GLTranForReclassToDelete.Add(tranForReclass);
				}
			}
		}

	    protected virtual void GLTranForReclassification_NewBranchID_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
	    {
	        var tranForReclass = (GLTranForReclassification)e.Row;

	        if (tranForReclass.NewBranchID == null)
	            return;

	        GetNewFinPeriod(tranForReclass).RaiseIfHasError();
        }

        protected virtual void GLTranForReclassification_NewBranchID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
	    {
	        var tranForReclass = (GLTranForReclassification)e.Row;

	        tranForReclass.NewFinPeriodID = GetNewFinPeriod(tranForReclass).GetValueOrRaiseError().FinPeriodID;
	    }

	    protected virtual ProcessingResult<FinPeriod> GetNewFinPeriod(GLTranForReclassification tranForReclass)
	    {
	        return FinPeriodRepository.GetFinPeriodByMasterPeriodID(
	            PXAccess.GetParentOrganizationID(tranForReclass.NewBranchID),
	            tranForReclass.TranPeriodID);
	    }

        protected virtual void GLTranForReclassification_NewSubID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			var tran = (GLTranForReclassification)e.Row;

			var toSubIDState = (PXFieldState)GLTranForReclass.Cache.GetStateExt<GLTranForReclassification.newSubID>(tran);
			tran.NewSubCD = (string)toSubIDState.Value;
		}

		protected virtual void GLTranForReclassification_NewBranchID_FieldUpdating(PXCache cache, PXFieldUpdatingEventArgs e)
		{
			var tran = (GLTranForReclassification)e.Row;

			GLTranForReclass.Cache.RaiseExceptionHandling<GLTranForReclassification.newBranchID>(tran, e.NewValue, null);
		}

		protected virtual void GLTranForReclassification_NewAccountID_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
		{
			if (e.Row == null || e.NewValue == null) return;
			var account = (Account)PXSelectorAttribute.Select<GLTranForReclassification.newAccountID>(cache, e.Row, e.NewValue);
			AccountAttribute.VerifyAccountIsNotControl<GLTranForReclassification.newAccountID>(cache, e, account, true);
		}

		protected virtual void GLTranForReclassification_NewAccountID_FieldUpdating(PXCache cache, PXFieldUpdatingEventArgs e)
		{
			var tran = (GLTranForReclassification)e.Row;

			GLTranForReclass.Cache.RaiseExceptionHandling<GLTranForReclassification.newAccountID>(tran, e.NewValue, null);
		}

		protected virtual void GLTranForReclassification_NewSubID_FieldUpdating(PXCache cache, PXFieldUpdatingEventArgs e)
		{
			var tran = (GLTranForReclassification)e.Row;

			GLTranForReclass.Cache.RaiseExceptionHandling<GLTranForReclassification.newSubID>(tran, e.NewValue, null);
		}

		protected virtual void GLTranForReclassification_NewTranDate_FieldUpdating(PXCache cache, PXFieldUpdatingEventArgs e)
		{
			var tran = (GLTranForReclassification)e.Row;

			GLTranForReclass.Cache.RaiseExceptionHandling<GLTranForReclassification.newTranDate>(tran, e.NewValue, null);
		}

		protected virtual void GLTranForReclassification_NewFinPeriodID_ExceptionHandling(PXCache cache, PXExceptionHandlingEventArgs e)
		{
			var tran = (GLTranForReclassification)e.Row;

			var newTranDatehasError = FieldHasError(tran, typeof(GLTranForReclassification.newTranDate).Name);

			if (!newTranDatehasError)
			{
				if(e.Exception is PXSetPropertyException exc)
				{
					exc.ErrorValue = tran.NewTranDate;
				}
				cache.RaiseExceptionHandling<GLTranForReclassification.newTranDate>(tran, tran.NewTranDate, e.Exception);
			}
		}

		protected virtual void GLTranForReclassification_RowUpdated(PXCache cache, PXRowUpdatedEventArgs e)
		{
			var tran = (GLTranForReclassification)e.Row;

			CalcSelectedFieldValue(tran);

			if (tran.NewBranchID == null)
			{
				cache.RaiseExceptionHandling<GLTranForReclassification.newBranchID>(e.Row, null,
					new PXSetPropertyException(ErrorMessages.FieldIsEmpty, PXErrorLevel.Error,
						typeof(GLTranForReclassification.newBranchID).Name));
			}

			if (tran.NewAccountID == null)
			{
				cache.RaiseExceptionHandling<GLTranForReclassification.newAccountID>(e.Row, null,
					new PXSetPropertyException(ErrorMessages.FieldIsEmpty, PXErrorLevel.Error,
						typeof(GLTranForReclassification.newAccountID).Name));
			}

			if (tran.NewSubID == null)
			{
				cache.RaiseExceptionHandling<GLTranForReclassification.newSubID>(e.Row, null,
					new PXSetPropertyException(ErrorMessages.FieldIsEmpty, PXErrorLevel.Error,
						typeof(GLTranForReclassification.newSubID).Name));
			}

			if (tran.NewTranDate == null)
			{
				cache.RaiseExceptionHandling<GLTranForReclassification.newTranDate>(e.Row, null,
					new PXSetPropertyException(ErrorMessages.FieldIsEmpty, PXErrorLevel.Error,
						typeof(GLTranForReclassification.newTranDate).Name));
			}

			if(tran.IsSplitted == true && IsReclassAttrChanged(tran) && tran.ReclassRowType == ReclassRowTypes.EditingVirtualParentTran)
			{
				tran.ReclassRowType = ReclassRowTypes.AddingNew;
			}
		}

		private void IsDateWithinPeriod(PXCache cache, GLTranForReclassification tran)
		{
			if (tran?.NewTranDate != null )
			{
				try
				{
					FinPeriodRepository.CheckIsDateWithinPeriod(tran.NewFinPeriodID,
																PXAccess.GetParentOrganizationID(tran.NewBranchID),
																tran.NewTranDate.Value,
																Messages.FiscalPeriodNotCurrent,
																PXErrorLevel.Warning);

					cache.RaiseExceptionHandling<GLTranForReclassification.newBranchID>(tran, tran.NewBranchID, null);
				}
				catch(PXSetPropertyException e)
				{
					cache.RaiseExceptionHandling<GLTranForReclassification.newTranDate>(tran, tran.NewTranDate, e);
				}
			}
		}

		private void VerifyAndRememberErorrForNewFields(GLTranForReclassification tran)
		{
			VerifyAndRememberErorr<GLTranForReclassification.newBranchID>(tran.NewBranchID, tran);
			VerifyAndRememberErorr<GLTranForReclassification.newAccountID>(tran.NewAccountID, tran);
			VerifyAndRememberErorr<GLTranForReclassification.newSubID>(tran.NewSubID, tran);
			VerifyAndRememberErorr<GLTranForReclassification.newTranDate>(tran.NewTranDate, tran);
			VerifyAndRememberErorr<GLTranForReclassification.newFinPeriodID>(tran.NewFinPeriodID, tran);
			VerifyAndRememberErorr<GLTranForReclassification.curyNewAmt>(tran.CuryNewAmt, tran);
		}

        protected virtual void GLTranForReclassification_CuryNewAmt_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            GLTranForReclassification tran = (GLTranForReclassification)e.Row;

            if (tran.IsSplitting == false)
            {
                return;
            }

			GLTranForReclassification parent = GLTranForReclass_Module_BatchNbr_LineNbr.Locate(GetGLTranForReclassByKey(tran.ParentKey));

            var sign = parent.CuryDebitAmt + parent.CuryCreditAmt > 0m ? 1 : -1;

            if (sign*(parent.CuryNewAmt + ((decimal?)e.OldValue ?? 0.0m) ?? 0.0m) < sign*(tran.CuryNewAmt ?? 0m))
            {
				SetExhaustedAmountError(tran);
			}

            parent.CuryNewAmt += ((decimal?)e.OldValue ?? 0.0m) - (tran.CuryNewAmt ?? 0.0m);
            cache.Update(parent);

            GLTranForReclass.View.RequestRefresh();
        }

        protected virtual void GLTranForReclassification_CuryNewAmt_FieldSelecting(PXCache cache, PXFieldSelectingEventArgs e)
        {
            var row = (GLTranForReclassification)e.Row;

            if (row == null)
            {
                return;
            }

            if(!(row.IsSplitted || row.IsSplitting))
            {
                e.ReturnValue = null;
            }

            if ((row.IsSplitted || row.IsSplitting) && e.ReturnValue == null)
            {
                e.ReturnValue = 0m;
            }
        }

        protected virtual void GLTranForReclassification_SplittedIcon_FieldSelecting(PXCache cache, PXFieldSelectingEventArgs e)
        {
            var row = (GLTranForReclassification)e.Row;

            if(row == null)
            {
                return;
            }

            if(State.SplittingGroups.ContainsKey(new GLTranKey(row)))
            {
                e.ReturnValue = SplitIcon.Parent;
            }

            if (row.IsSplitting)
            {
                e.ReturnValue = SplitIcon.Split;
            }
        }
        #endregion


        #region LoadOptions

        protected virtual void LoadOptions_FromAccountID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			var options = (LoadOptions)e.Row;

			var fromAccountIDState = (PXFieldState)cache.GetStateExt<LoadOptions.fromAccountID>(options);
			var toAccountIDState = (PXFieldState)cache.GetStateExt<LoadOptions.toAccountID>(options);

			var fromAccountCD = (string) fromAccountIDState.Value;
			var toAccountCD = (string) toAccountIDState.Value;

			if (String.CompareOrdinal(fromAccountCD, toAccountCD) > 0)
			{
				cache.SetValueExt<LoadOptions.toAccountID>(e.Row, fromAccountCD);
			}
		}

		protected virtual void LoadOptions_ToAccountID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			var options = (LoadOptions)e.Row;

			var fromAccountIDState = (PXFieldState)cache.GetStateExt<LoadOptions.fromAccountID>(options);
			var toAccountIDState = (PXFieldState)cache.GetStateExt<LoadOptions.toAccountID>(options);

			var fromAccountCD = (string)fromAccountIDState.Value;
			var toAccountCD = (string)toAccountIDState.Value;

			if (toAccountCD != null && String.CompareOrdinal(fromAccountCD, toAccountCD) > 0)
			{
				cache.SetValueExt<LoadOptions.fromAccountID>(e.Row, toAccountCD);
			}
		}

		protected virtual void LoadOptions_FromSubID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			var options = (LoadOptions)e.Row;

			var fromSubIDState = (PXFieldState)cache.GetStateExt<LoadOptions.fromSubID>(options);
			var toSubIDState = (PXFieldState)cache.GetStateExt<LoadOptions.toSubID>(options);

			var fromSubCD = (string)fromSubIDState.Value;
			var toSubCD = (string)toSubIDState.Value;

			if (String.CompareOrdinal(fromSubCD, toSubCD) > 0)
			{
				cache.SetValue<LoadOptions.toSubID>(e.Row, options.FromSubID);
			}
		}

		protected virtual void LoadOptions_ToSubID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			var options = (LoadOptions)e.Row;

			var fromSubIDState = (PXFieldState)cache.GetStateExt<LoadOptions.fromSubID>(options);
			var toSubIDState = (PXFieldState)cache.GetStateExt<LoadOptions.toSubID>(options);

			var fromSubCD = (string)fromSubIDState.Value;
			var toSubCD = (string)toSubIDState.Value;

			if (toSubCD != null && String.CompareOrdinal(fromSubCD, toSubCD) > 0)
			{
				cache.SetValue<LoadOptions.fromSubID>(e.Row, options.ToSubID);
			}
		}

		protected virtual void LoadOptions_FromFinPeriodID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			var options = (LoadOptions)e.Row;

			if (String.CompareOrdinal(options.FromFinPeriodID, options.ToFinPeriodID) > 0)
			{
				cache.SetValue<LoadOptions.toFinPeriodID>(e.Row, options.FromFinPeriodID);
			}

			SetPeriodDates(options);
		}

		protected virtual void LoadOptions_ToFinPeriodID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			var options = (LoadOptions)e.Row;

			if (options.ToFinPeriodID != null && String.CompareOrdinal(options.FromFinPeriodID, options.ToFinPeriodID) > 0)
			{
				cache.SetValue<LoadOptions.fromFinPeriodID>(e.Row, options.ToFinPeriodID);
			}

			SetPeriodDates(options);
		}

		protected virtual void LoadOptions_FromDate_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			var options = (LoadOptions)e.Row;

			if (options.FromDate > options.ToDate || options.ToDate == null)
			{
				cache.SetValue<LoadOptions.toDate>(e.Row, options.FromDate);
			}
		}

		protected virtual void LoadOptions_ToDate_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			var options = (LoadOptions)e.Row;

			if (options.ToDate != null && options.FromDate > options.ToDate)
			{
				cache.SetValue<LoadOptions.fromDate>(e.Row, options.ToDate);
			}
		}

		protected virtual void LoadOptions_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			var loadOptions = e.Row as LoadOptions;

			if (loadOptions == null)
				return;

			if (loadOptions.ToDate.HasValue && ((loadOptions.PeriodEndDate.HasValue && loadOptions.ToDate > loadOptions.PeriodEndDate) || (loadOptions.PeriodStartDate.HasValue && loadOptions.ToDate < loadOptions.PeriodStartDate)))
			{
				cache.RaiseExceptionHandling<LoadOptions.toDate>(e.Row, loadOptions.ToDate, new PXSetPropertyException(Messages.TheDateIsOutsideOfTheSpecifiedPeriod, PXErrorLevel.Warning));
			}
			else
			{
				cache.RaiseExceptionHandling<LoadOptions.toDate>(e.Row, null, null);
			}

			if (loadOptions.FromDate.HasValue && ((loadOptions.PeriodStartDate.HasValue && loadOptions.FromDate < loadOptions.PeriodStartDate) || (loadOptions.PeriodEndDate.HasValue && loadOptions.FromDate >= loadOptions.PeriodEndDate)))
			{
				cache.RaiseExceptionHandling<LoadOptions.fromDate>(e.Row, loadOptions.FromDate, new PXSetPropertyException(Messages.TheDateIsOutsideOfTheSpecifiedPeriod, PXErrorLevel.Warning));
			}
			else
			{
				cache.RaiseExceptionHandling<LoadOptions.fromDate>(e.Row, null, null);
			}
		}

		#endregion

		private void ButtonsFieldSelectingHandlerForDisableAfterProcess(PXCache sender, PXFieldSelectingEventArgs e)
		{
			e.ReturnState = CreateReturnState(e.ReturnState);

			((PXButtonState)e.ReturnState).Enabled = PXLongOperation.GetStatus(this.UID) == PXLongRunStatus.NotExists;
		}

		private void LoadOptionsButtonFieldSelectingHandler(PXCache sender, PXFieldSelectingEventArgs e)
		{
			e.ReturnState = CreateReturnState(e.ReturnState);

			ReclassifyTransactionsProcess processGraph = sender.Graph as ReclassifyTransactionsProcess;
			((PXButtonState)e.ReturnState).Enabled = PXLongOperation.GetStatus(this.UID) == PXLongRunStatus.NotExists 
				&& (processGraph == null || processGraph.State.ReclassScreenMode != ReclassScreenMode.Editing);
		}

		private void ProcessButtonFieldSelectingHandler(PXCache sender, PXFieldSelectingEventArgs e)
		{
			e.ReturnState = CreateReturnState(e.ReturnState);

			((PXButtonState)e.ReturnState).Enabled = PXLongOperation.GetStatus(this.UID) == PXLongRunStatus.NotExists && sender.Cached.Any_();
		}

		private void DependingOnRowExistanceButtonsSelectingHandler(PXCache sender, PXFieldSelectingEventArgs e)
		{
			e.ReturnState = CreateReturnState(e.ReturnState);

			ButtonsFieldSelectingHandlerForDisableAfterProcess(sender, e);

			((PXButtonState)e.ReturnState).Enabled = ((PXButtonState)e.ReturnState).Enabled && sender.Cached.Any_();
		}

		private void ReloadTransButtonStateSelectingHandler(PXCache sender, PXFieldSelectingEventArgs e)
		{
			e.ReturnState = CreateReturnState(e.ReturnState);

			((PXButtonState)e.ReturnState).Enabled = PXLongOperation.GetStatus(this.UID) == PXLongRunStatus.NotExists
													  && State.ReclassScreenMode != ReclassScreenMode.Editing;
		}

		#endregion


		#region ReplaceOptions

		protected virtual void ReplaceOptions_NewFinPeriodID_ExceptionHandling(PXCache cache, PXExceptionHandlingEventArgs e)
		{
			var replaceOptionRow = (ReplaceOptions)e.Row;

			cache.RaiseExceptionHandling<ReplaceOptions.newDate>(replaceOptionRow, replaceOptionRow.NewDate, e.Exception);
		}

		protected virtual void ReplaceOptions_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			var replaceOptionRow = (ReplaceOptions)e.Row;

			if (!replaceOptionRow.Showed)
			{
				replaceOptionRow.Showed = true;

				if (replaceOptionRow.NewFinPeriodID != null)
				{
					object finPeriodID = replaceOptionRow.NewFinPeriodID;

					cache.RaiseFieldVerifying<ReplaceOptions.newFinPeriodID>(replaceOptionRow, ref finPeriodID);
				}
				else if (replaceOptionRow.NewDate != null)
				{
					cache.RaiseFieldUpdated<ReplaceOptions.newDate>(replaceOptionRow, replaceOptionRow.NewDate);
				}
			}

            bool showWarning = State.SplittingGroups.Any();
            var message = Messages.ReplaceActionWillAffectSplitTransacionsThatMatchTheSelectedCriteria;

            replaceOptionRow.Warning = message; 
            PXUIFieldAttribute.SetVisible<ReplaceOptions.warning>(cache, replaceOptionRow, showWarning);
            cache.RaiseExceptionHandling<ReplaceOptions.warning>(replaceOptionRow, message, new PXSetPropertyException(message, PXErrorLevel.Warning));
        }

		protected virtual void ReplaceOptions_WithAccountID_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
		{
			if (e.Row == null || e.NewValue == null) return;
			var account = (Account)PXSelectorAttribute.Select<ReplaceOptions.withAccountID>(cache, e.Row, e.NewValue);
			AccountAttribute.VerifyAccountIsNotControl<ReplaceOptions.withAccountID>(cache, e, account, true);
		}

		protected virtual void ReplaceOptions_NewAccountID_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
		{
			if (e.Row == null || e.NewValue == null) return;
			var account = (Account)PXSelectorAttribute.Select<ReplaceOptions.newAccountID>(cache, e.Row, e.NewValue);
			AccountAttribute.VerifyAccountIsNotControl<ReplaceOptions.newAccountID>(cache, e, account, true);
		}

		#endregion


		#region Actions

		[PXButton(ImageKey = Sprite.Main.RecordDel)]
		[PXUIField(DisplayName = ActionsMessages.Delete, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		protected virtual IEnumerable delete(PXAdapter adapter)
		{
			var tran = GLTranForReclass.Current;

            var message = tran.IsSplitted ? Messages.CurrentRecordAndItsChildRecordsWillBeDeleted : Messages.CurrentRecordWillBeDeleted;

            if(adapter.View.Ask(message, MessageButtons.OKCancel) == WebDialogResult.Cancel)
            {
                adapter.Get();
            }

            GLTranKey tranKey = new GLTranKey(tran);

            

            if(State.SplittingGroups.ContainsKey(tranKey))
            {
                foreach(GLTranKey key in State.SplittingGroups[tranKey])
				{
					GLTranForReclassification splitRow = GLTranForReclass_Module_BatchNbr_LineNbr.Locate(GetGLTranForReclassByKey(key));
					RemoveTran(splitRow);
				}

				State.SplittingGroups.Remove(tranKey);
            }

			RemoveTran(tran);
			
			GLTranForReclass.Cache.Remove(tran);

            if (tran.IsSplitting)
            {
                if (State.SplittingGroups[tran.ParentKey].Any(m => m.Equals(tranKey)))
                {
                    State.SplittingGroups[tran.ParentKey].Remove(tranKey);
                }

                GLTranForReclassification parentRow = GLTranForReclass_Module_BatchNbr_LineNbr.Locate(GetGLTranForReclassByKey(tran.ParentKey));

                if (!State.SplittingGroups[tran.ParentKey].Any())
                {
                    parentRow.IsSplitted = false;
                    parentRow.CuryNewAmt = 0m;
                    State.SplittingGroups.Remove(tran.ParentKey);
                }

                parentRow.CuryNewAmt += tran.CuryNewAmt;
                GLTranForReclass.Cache.Update(parentRow);
            }

            return adapter.Get();
		}

		private void RemoveTran(GLTranForReclassification splitRow)
		{
			if (splitRow.ReclassRowType == ReclassRowTypes.Editing)
			{
				State.GLTranForReclassToDelete.Add(splitRow);
			}
			GLTranForReclass.Cache.Remove(splitRow);
		}

		[PXCancelButton]
		[PXUIField(DisplayName = ActionsMessages.Cancel, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		protected virtual IEnumerable cancel(PXAdapter adapter)
		{
			var addedTranForReclass = GetUpdatedTranForReclass().Where(tran => tran.ReclassRowType == ReclassRowTypes.AddingNew);
			var state = State;
			state.GLTranForReclassToDelete.Clear();

			Clear();

			State = state;
            State.ClearSplittingGroups();
            PutTransForReclassToCacheByKey(addedTranForReclass);
			PutReclassificationBatchTransForEditingToCache(State.EditingBatchModule, State.EditingBatchNbr);

			return adapter.Get();
		}

		public PXAction<GLTranForReclassification> ViewReclassBatch;
		[PXUIField(Visible = false, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable viewReclassBatch(PXAdapter adapter)
		{
			var tran = GLTranForReclass.Current;

			if (tran != null)
			{
				JournalEntry.RedirectToBatch(this, tran.ReclassBatchModule, tran.ReclassBatchNbr);
			}

			return adapter.Get();
		}

		[PXUIField(DisplayName = Messages.Load, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXButton]
		public virtual IEnumerable ShowLoadTransPopup(PXAdapter adapter)
		{
			LoadOptionsView.AskExt();

			return adapter.Get();
		}

		[PXUIField(DisplayName = "Reload", Visible = false, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXButton]
		public virtual IEnumerable ReloadTrans(PXAdapter adapter)
		{
			var res = GLTranForReclass.Ask(InfoMessages.TransactionsListedOnTheFormIfAnyWillBeRemoved, MessageButtons.OKCancel);

			if (res != WebDialogResult.OK)
				return adapter.Get();
			
			GLTranForReclass.Cache.Clear();
            GLTranForReclass.Cache.ClearQueryCacheObsolete();
            State.ClearSplittingGroups();

            LoadTransactionsProc(isReload: true);

			return adapter.Get(); 
		}

		[PXUIField(DisplayName = Messages.Load, Visible = false, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXButton]
		public virtual IEnumerable LoadTrans(PXAdapter adapter)
		{
			LoadTransactionsProc();

			return adapter.Get(); 
		}

		[PXUIField(DisplayName = "Replace", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXButton]
		public virtual IEnumerable Replace(PXAdapter adapter)
		{
			var options = ReplaceOptionsView.Current;

			options.Showed = false;

			if (ReplaceOptionsView.AskExt() == WebDialogResult.OK)
			{
				if (options.NewBranchID == null
				    && options.NewAccountID == null
				    && options.NewSubID == null
				    && options.NewFinPeriodID == null
				    && options.NewDate == null
				    && options.NewTranDesc == null)
				{
					return adapter.Get();
				}

				var trans = GetUpdatedTranForReclass();

				if (options.WithBranchID != null)
				{
					trans = trans.Where(tran => tran.NewBranchID == options.WithBranchID);
				}

				if (options.WithAccountID != null)
				{
					trans = trans.Where(tran => tran.NewAccountID == options.WithAccountID);
				}

				if (options.WithSubID != null)
				{
					trans = trans.Where(tran => tran.NewSubID == options.WithSubID);
				}

				if (options.WithFinPeriodID != null)
				{
				    string masterPeriodID =
				        FinPeriodIDAttribute.CalcMasterPeriodID<ReplaceOptions.withFinPeriodID>(
				            ReplaceOptionsView.Cache, options);

					trans = trans.Where(tran => tran.TranPeriodID == masterPeriodID);
				}

				if (options.WithDate != null)
				{
					trans = trans.Where(tran => tran.NewTranDate == options.WithDate);
				}

				if (options.WithTranDescFilteringValue != null)
				{
					trans = AddTransDescWhereConditionForReplace(trans, options.WithTranDescFilteringValue);
				}

				foreach (var tran in trans)
				{
					var oldTran = PXCache<GLTranForReclassification>.CreateCopy(tran);

					if (options.NewBranchID != null && options.NewBranchID != tran.NewBranchID)
					{
						tran.NewBranchID = options.NewBranchID;
					}

					if (options.NewAccountID != null && options.NewAccountID != tran.NewAccountID)
					{
						tran.NewAccountID = options.NewAccountID;
					}

					if (options.NewSubID != null && options.NewSubID != tran.NewSubID)
					{
						var newSubIDState = (PXFieldState) ReplaceOptionsView.Cache.GetStateExt<ReplaceOptions.newSubID>(options);
						GLTranForReclass.Cache.SetValueExt<GLTranForReclassification.newSubID>(tran, newSubIDState.Value);
					}

					if (options.NewDate != null && options.NewDate != tran.NewTranDate)
					{
						tran.NewTranDate = options.NewDate;

						object newValue = tran.NewTranDate;

						try
						{
							GLTranForReclass.Cache.RaiseFieldVerifying<GLTranForReclassification.newTranDate>(tran, ref newValue);
						}
						catch (PXSetPropertyException ex)
						{
							GLTranForReclass.Cache.RaiseExceptionHandling<GLTranForReclassification.newTranDate>(tran, newValue, ex);
						}
					}

					if (options.NewTranDesc != null)
					{
						tran.NewTranDesc = _emptyStringReplaceWildCards.Contains((options.NewTranDesc))
							? null
							: options.NewTranDesc;
					}

					GLTranForReclass.Cache.RaiseRowUpdated(tran, oldTran);
					GLTranForReclass.Cache.RaiseRowSelected(tran);
				}
			}

			return adapter.Get();
		}

        [PXUIField(DisplayName = "Split", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
        [PXButton]
        public virtual IEnumerable Split(PXAdapter adapter)
        {
            GLTranForReclassification parent = null;

            if(GLTranForReclass.Current.IsSplitting)
            {
                parent = GLTranForReclass.Locate(GetGLTranForReclassByKey(GLTranForReclass.Current.ParentKey));
            }
            else
            {
                parent = GLTranForReclass.Current;
            }

            var splitRow = SplitTransaction(GLTranForReclass.Cache, parent);
            
            var parentKey = new GLTranKey(parent);
            var childKey = new GLTranKey(splitRow);

            if (!State.SplittingGroups.ContainsKey(parentKey))
            {
                State.SplittingGroups[parentKey] = new List<GLTranKey> { new GLTranKey(splitRow) };

                parent.CuryNewAmt = parent.CuryReclassRemainingAmt != 0 ? parent.CuryReclassRemainingAmt : parent.CuryDebitAmt + parent.CuryCreditAmt;
                parent.IsSplitted = true;

                if(IsReclassAttrChanged(parent) == false)
                {
                    parent.NewTranDate = parent.TranDate;
                    parent.NewTranDesc = parent.TranDesc;
                }

                GLTranForReclass.Update(parent);
            }
            else
            {
                State.SplittingGroups[parentKey].Add(childKey);
            }

            return adapter.Get();
        }

        [PXUIField(DisplayName = "Process", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
        [PXButton]
        public virtual IEnumerable ValidateAndProcess(PXAdapter adapter)
        {
            ValidateSplitGroups();

            return Actions["Process"].Press(adapter);
        }

        private GLTranForReclassification SplitTransaction(PXCache cache, GLTranForReclassification originalTran)
        {
			currencyInfo.Current = PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Required<GLTran.curyInfoID>>>>.Select(this, originalTran.CuryInfoID);

			var parentKey = new GLTranKey(originalTran);

            var splittedTran = (GLTranForReclassification)GLTranForReclass.Cache.CreateCopy(originalTran);

            splittedTran.LineNbr = State.CurrentSplitLineNbr;
            splittedTran.ParentKey = parentKey;
            splittedTran.CuryNewAmt = 0.0m;
            splittedTran.SourceCuryDebitAmt = originalTran.CuryDebitAmt;
            splittedTran.SourceCuryCreditAmt = originalTran.CuryCreditAmt;
            splittedTran.CuryDebitAmt = 0m;
            splittedTran.CuryCreditAmt = 0m;
            splittedTran.NewBranchID = originalTran.BranchID;
            splittedTran.NewAccountID = originalTran.AccountID;
            splittedTran.NewSubID = originalTran.SubID;
            splittedTran.NewTranDesc = originalTran.TranDesc;
            splittedTran.NewTranDate = originalTran.TranDate;
            splittedTran.ProjectID = originalTran.ProjectID;
            splittedTran.ReclassRowType = ReclassRowTypes.AddingNew;
            splittedTran.IsSplitted = false;
            splittedTran.Selected = false;

            State.IncSplitLineNbr();

            Batch.Current = (Batch)PXParentAttribute.SelectParent(cache, originalTran);
            splittedTran = GLTranForReclass.Insert(splittedTran);

            if (splittedTran == null)
            {
                throw new PXException(Messages.SplitRecordHasNotBeenAdded);
            }

            GLTranForReclass.Cache.SetStatus(splittedTran, PXEntryStatus.Updated);

            return splittedTran;
        }

        public PXAction<GLTranForReclassification> viewDocument;
		[PXUIField(MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = false)]
		[PXButton]
		public virtual IEnumerable ViewDocument(PXAdapter adapter)
		{
			var tranForReclass = GLTranForReclass.Current;
			
			if (tranForReclass != null)
			{
				Batch batch = JournalEntry.FindBatch(this, tranForReclass);

				PXGraph.CreateInstance<JournalEntry>().RedirectToDocumentByTran(tranForReclass, batch);
			}

			return adapter.Get();
		}

		#endregion


		#region UI

		private void VerifyAndRememberErorr<TField>(object newValue, GLTranForReclassification tran) 
			where TField : IBqlField
		{
			var origNewValue = newValue;

			try
			{
				GLTranForReclass.Cache.RaiseFieldVerifying<TField>(tran, ref newValue);
			}
			catch (PXSetPropertyException ex)
			{
				var fieldName = typeof(TField).Name;

				tran.FieldsErrorForInvalidFromValues[fieldName] = new GLTranForReclassification.ExceptionAndErrorValuesTriple()
				{
					Error = ex,
					ErrorValue = origNewValue,
					ErrorUIValue = newValue
				};

				GLTranForReclass.Cache.RaiseExceptionHandling<TField>(tran, newValue, ex);
			}
		}

		private PXButtonState CreateReturnState(object returnState)
		{
			return PXButtonState.CreateInstance(returnState, null, null, null, null, null, false,
					   PXConfirmationType.Unspecified, null, null, null, null, null, null, null, null, null, null, null,
					   typeof(GLTranForReclassification));

		}

		protected void LoadTransactionsProc(bool isReload = false)
		{
			var dataRows = GetTransForReclassByLoadOptions(LoadOptionsView.Current);

			var nonReclassifiableFound = false;
			var reclassifiableFound = false;

			foreach (var row in dataRows)
			{
				var tranForReclass = (GLTranForReclassification) row;
				var curyInfo = (CurrencyInfo) row;
				var batch = (Batch) row;
				var ledger = (Ledger) row;

                if (JournalEntry.IsTransactionReclassifiable(tranForReclass, batch.BatchType, ledger.BalanceType, ProjectDefaultAttribute.NonProject()))
				{
                    if((IsReclassAttrChangedAndNotNull(tranForReclass) == true || (tranForReclass.CuryNewAmt ?? 0m) != 0m) && isReload == false)
                    {
                        continue;
                    }

					InitTranForReclassAdditionalFields(tranForReclass, curyInfo);
                    InitOriginalAmountIfRepeatedReclassification(tranForReclass);

                    if (isReload && tranForReclass.IsSplitted == true)
                    {
                        tranForReclass.IsSplitted = false;
                    }

                    tranForReclass.Selected = false;

                    GLTranForReclass.Cache.SetStatus(tranForReclass, PXEntryStatus.Updated);

					reclassifiableFound = true;
				}
				else if (tranForReclass.ReclassRowType != ReclassRowTypes.Editing && tranForReclass.ReclassRowType != ReclassRowTypes.EditingVirtualParentTran 
					&& tranForReclass.IsSplitted == false && tranForReclass.IsSplitting == false)
				{
					nonReclassifiableFound = true;

					GLTranForReclass.Cache.Remove(tranForReclass);
				}
			}

			if (!reclassifiableFound)
			{
				throw new PXException(
					InfoMessages.NoReclassifiableTransactionsHaveBeenFoundToMatchTheCriteria);
			}

			if (nonReclassifiableFound)
			{
				GLTranForReclass.Ask(
					InfoMessages
						.SomeTransactionsCannotBeReclassified,
					MessageButtons.OK);
			}
		}

        protected virtual void SetPeriodDates(LoadOptions loadOptions)
		{
			int? calendarOrganizationID = FinPeriodRepository.GetCalendarOrganizationID(loadOptions.BranchID, loadOptions.UseMasterCalendar);
			FinPeriod fromPeriod = FinPeriodRepository.FindByID(calendarOrganizationID, loadOptions.FromFinPeriodID);
			FinPeriod endPeriod = FinPeriodRepository.FindByID(calendarOrganizationID, loadOptions.ToFinPeriodID);

			if (fromPeriod != null && endPeriod != null)
			{
				loadOptions.PeriodStartDate = fromPeriod.StartDate <= endPeriod.StartDate ? fromPeriod.StartDate : endPeriod.StartDate;
				loadOptions.PeriodEndDate = endPeriod.EndDate >= fromPeriod.EndDate ? endPeriod.EndDate : fromPeriod.EndDate;
			}
			else if (fromPeriod != null || endPeriod != null)
			{
				FinPeriod datesPeriod = fromPeriod ?? endPeriod;
				loadOptions.PeriodStartDate = datesPeriod.StartDate;
				loadOptions.PeriodEndDate = datesPeriod.EndDate;
			}
			else
			{
				loadOptions.PeriodStartDate = null;
				loadOptions.PeriodEndDate = null;
			}
		}

        private bool IsAllowProcess(GLTranForReclassification tranForReclass)
        {
            var anyReclassAttrChanged = IsReclassAttrChanged(tranForReclass) || tranForReclass.IsSplitted == true;

            var allNotNull = tranForReclass.NewBranchID != null &&
                             tranForReclass.NewAccountID != null &&
                             tranForReclass.NewSubID != null &&
                             tranForReclass.NewTranDate != null;

            var hasErrorInFields = false;

			IsDateWithinPeriod(GLTranForReclass.Cache, tranForReclass);
			foreach (var editableField in EditableFields)
            {
                hasErrorInFields = FieldHasError(tranForReclass, editableField.Name);

                if (hasErrorInFields)
                    break;
            }

            return anyReclassAttrChanged && allNotNull && !hasErrorInFields;
        }

		public static bool IsReclassAttrChangedAndNotNull(GLTranForReclassification tranForReclass)
		{
			return (tranForReclass.NewBranchID != tranForReclass.BranchID && tranForReclass.NewBranchID != null) ||
					(tranForReclass.NewAccountID != tranForReclass.AccountID && tranForReclass.NewBranchID != null) ||
					(tranForReclass.NewSubID != tranForReclass.SubID && tranForReclass.NewBranchID != null);
		}

		private void CalcSelectedFieldValue(GLTranForReclassification tranForReclass)
		{
			if (tranForReclass.IsSplitted == true || tranForReclass.IsSplitting == true)
			{
				CalcGroupSelectedValues(tranForReclass);
				return;
			}
			else
			{
				bool allowProcess = IsAllowProcess(tranForReclass);
				tranForReclass.Selected = allowProcess;
			}
		}

		private void CalcGroupSelectedValues(GLTranForReclassification tran)
		{
			GLTranKey grKey = tran.IsSplitting ? tran.ParentKey : new GLTranKey(tran);

			var parent = GLTranForReclass.Locate(GetGLTranForReclassByKey(grKey));
			decimal parentBalance = parent.CuryDebitAmt.Value + parent.CuryCreditAmt.Value;
			var sign = parent.CuryDebitAmt + parent.CuryCreditAmt > 0m ? 1 : -1;

			bool isParentAllowProcess = IsAllowProcess(parent);
			bool isParentModified = IsReclassAttrChanged(parent);
			bool allChildsProcessible = true;
			bool allChildsProcessibilityWithoutOverflowCheck = true;

			IReadOnlyCollection<GLTranKey> splitKeysGroup = State.SplittingGroups[grKey];
			
			if(tran.IsSplitting)
			{
				var currentKey = new GLTranKey(tran);
				splitKeysGroup = splitKeysGroup.Except(currentKey).Union(new List<GLTranKey> { currentKey }).ToList();
			}
			else
			{
				splitKeysGroup = State.SplittingGroups[grKey];
			}

			decimal totalSplitAmt = 0m;
			bool isErrorAppeared = false;
			bool isSplitAmountProcessible = true;

			var splitGroup = new List<GLTranForReclassification>();

			foreach (var splitKey in splitKeysGroup)
			{
				var split = GLTranForReclass.Locate(GetGLTranForReclassByKey(splitKey));
				splitGroup.Add(split);
			}

			foreach (var split in splitGroup)
			{
				bool allowProcess = IsAllowProcess(split);
				bool isModified = IsReclassAttrChanged(split);
				bool hasDuplicated = IsDuplicate(split, parent) || splitGroup.Any(m => m.LineNbr != split.LineNbr && IsDuplicate(m, split));

				parentBalance -= split.CuryNewAmt ?? 0m;
				totalSplitAmt += split.CuryNewAmt ?? 0m;

				bool isAmountOverflowed = sign * parentBalance < 0m;

				if (isAmountOverflowed && (isErrorAppeared == false))
				{
					SetExhaustedAmountError(split);
					isErrorAppeared = true;
					isSplitAmountProcessible = false;
				}
				else
				{
					GLTranForReclass.Cache.RaiseExceptionHandling<GLTranForReclassification.curyNewAmt>(split, split.CuryNewAmt, null);
				}

				bool allowProcessWithoutOverflowCheck = allowProcess && (isParentModified || !hasDuplicated) && isModified && (split.CuryNewAmt ?? 0m) != 0m;
				bool selected = allowProcessWithoutOverflowCheck && isSplitAmountProcessible;

				split.Selected = selected;
				allChildsProcessibilityWithoutOverflowCheck &= allowProcessWithoutOverflowCheck;
				allChildsProcessible &= selected;
			}

			if(allChildsProcessibilityWithoutOverflowCheck == true && sign * (parent.CuryDebitAmt.Value + parent.CuryCreditAmt.Value - totalSplitAmt) >= 0m)
			{
				foreach (var split in splitGroup)
				{
					GLTranForReclass.Cache.RaiseExceptionHandling<GLTranForReclassification.curyNewAmt>(split, split.CuryNewAmt, null);
					split.Selected = true;
				}

				allChildsProcessible = true;
			}
			

			bool parentHasDuplicated = splitGroup.Any(m => IsDuplicate(m, parent));
			bool parentSelected = (allChildsProcessible || isParentModified) && !parentHasDuplicated && isParentAllowProcess;
			parent.Selected = parentSelected;

			GLTranForReclass.View.RequestRefresh();
		}

		private bool IsDuplicate(GLTranForReclassification ltran, GLTranForReclassification rtran)
		{
			return ltran.NewBranchID == rtran.NewBranchID && ltran.NewAccountID == rtran.NewAccountID && ltran.NewSubID == rtran.NewSubID;
		}

		private bool FieldHasError(GLTranForReclassification tranForReclass, string fieldName)
		{
			return GLTranForReclass.Cache.GetAttributesReadonly(tranForReclass, fieldName)
															.OfType<IPXInterfaceField>()
															.Any(attr => attr.ErrorText != null && attr.ErrorLevel == PXErrorLevel.Error);
		}

        private bool IsTranDateAndDescEnabled(GLTranForReclassification tran)
        {
            return (tran.IsSplitted == true &&
				   IsReclassAttrChanged(tran)) ||
                   tran.IsSplitted != true;
        }

		private IEnumerable<GLTranForReclassification> AddTransDescWhereConditionForReplace(IEnumerable<GLTranForReclassification> trans, string filteringValue)
		{
			if (_emptyStringReplaceWildCards.Contains(filteringValue))
			{
				return trans.Where(tran => string.IsNullOrEmpty(tran.NewTranDesc));
			}

			var pattern = Regex.Escape(filteringValue);
			pattern = string.Concat("^", pattern.Replace("\\*", ".*").Replace("\\?", "."), "$");

			var regex = new Regex(pattern, RegexOptions.IgnoreCase);

			return trans.Where(tran => regex.IsMatch(tran.TranDesc));
		}

		private IList<PXResult<GLTranForReclassification, CurrencyInfo>> RegetAndKeepInCacheOnlyReclassifiableTransByKey(IEnumerable<GLTran> trans)
		{
			var resultTrans = new List<PXResult<GLTranForReclassification, CurrencyInfo>>();

			foreach (var tran in trans)
			{
				if(tran.ReclassBatchNbr == null || (tran.ReclassBatchNbr != null && tran.Reclassified == true && (tran.CuryReclassRemainingAmt ?? 0m) != 0m))
                {
					var refreshedTran = (PXResult<GLTranForReclassification, CurrencyInfo>)
                            PXSelectJoin<GLTranForReclassification,
                        InnerJoin<CurrencyInfo,
                            On<GLTran.curyInfoID, Equal<CurrencyInfo.curyInfoID>>>,
                        Where<GLTranForReclassification.module, Equal<Required<GLTranForReclassification.module>>,
                            And<GLTranForReclassification.batchNbr, Equal<Required<GLTranForReclassification.batchNbr>>,
                            And<GLTranForReclassification.lineNbr, Equal<Required<GLTranForReclassification.lineNbr>>>>>>.Select(this, tran.Module, tran.BatchNbr, tran.LineNbr);

					GLTranForReclassification refreshedTranForReclass = refreshedTran;

					if (refreshedTran != null && (refreshedTranForReclass.ReclassBatchNbr == null 
						|| (refreshedTranForReclass.ReclassBatchNbr != null && refreshedTranForReclass.Reclassified == true && (refreshedTranForReclass.CuryReclassRemainingAmt ?? 0m) != 0m)) 
						&& JournalEntry.HasUnreleasedReclassTran(refreshedTranForReclass) == false)
					{
						resultTrans.Add(refreshedTran);
					}
					else
					{
						TryRemoveTranForReclassFromCacheByKey(tran);
					}
				}
				else
				{
					TryRemoveTranForReclassFromCacheByKey(tran);
				}
			}

			return resultTrans;
		}

		private void TryRemoveTranForReclassFromCacheByKey(GLTran tran)
		{
			var tranForReclass = new GLTranForReclassification()
			{
				Module = tran.Module,
				BatchNbr = tran.BatchNbr,
				LineNbr = tran.LineNbr
			};

			tranForReclass = GLTranForReclass.Locate(tranForReclass);

			if (tranForReclass != null)
			{
				GLTranForReclass.Cache.Remove(tranForReclass);
			}
		}

		private void PutTransForReclassToCacheByKey(IEnumerable<GLTran> trans)
		{
			var resultRows = RegetAndKeepInCacheOnlyReclassifiableTransByKey(trans);

			foreach (PXResult<GLTranForReclassification, CurrencyInfo> row in resultRows)
			{
				var tranForReclass = (GLTranForReclassification) row;
				var curyInfo = (CurrencyInfo) row;

                InitOriginalAmountIfRepeatedReclassification(tranForReclass);
                InitTranForReclassAdditionalFields(tranForReclass, curyInfo);

				GLTranForReclass.Cache.SetStatus(tranForReclass, PXEntryStatus.Updated);
                CalcSelectedFieldValue(tranForReclass);
				IsDateWithinPeriod(GLTranForReclass.Cache, tranForReclass);
            }
		}

		private void PutReclassificationBatchTransForEditingToCache(string module, string batchNbr)
		{
			var transForEditing = GetTransReclassTypeSorted(this, module, batchNbr);

            var parentsList = new Dictionary<GLTranKey, GLTranForReclassification>();

			foreach(var tranToEdit in transForEditing)
			{
				if(tranToEdit.IsReclassReverse == true)
				{
					continue;
				}

				if (tranToEdit.ReclassType == ReclassType.Split)
				{
					PutSplitAmountTransactions(parentsList, tranToEdit);
				}

				if (tranToEdit.ReclassType == ReclassType.Common)
				{
					PutCommonReclassifiedTransaction(parentsList, tranToEdit);
				}
			}

			foreach (var parent in parentsList)
			{
				CalcSelectedFieldValue(parent.Value);
				GLTranForReclass.Cache.RaiseRowSelected(parent.Value);
			}
		}

		private void PutCommonReclassifiedTransaction(Dictionary<GLTranKey, GLTranForReclassification> parentsList, GLTran tranToEdit)
		{
			var tranForReclass = (GLTranForReclassification)GLTranForReclass_Module_BatchNbr_LineNbr.Select(tranToEdit.OrigModule,
																																tranToEdit.OrigBatchNbr,
																																tranToEdit.OrigLineNbr);

			GLTranForReclass.Cache.SetStatus(tranForReclass, PXEntryStatus.Updated);

			InitTranForReclassAdditionalFieldsForEditing(tranForReclass, tranToEdit);
			tranForReclass.EditingPairReclassifyingLineNbr = tranToEdit.LineNbr;

			var thisTranKey = new GLTranKey(tranForReclass);
			if(parentsList.ContainsKey(thisTranKey))
			{
				tranForReclass.IsSplitted = true;
				InitTranForReclassAdditionalFieldsForEditing(tranForReclass, tranToEdit);
				InitOriginalAmountIfRepeatedReclassification(tranForReclass);
			}
			else
			{
				CalcSelectedFieldValue(tranForReclass);
			}

			tranForReclass.ReclassRowType = ReclassRowTypes.Editing;
			GLTranForReclass.Cache.RaiseRowSelected(tranForReclass);
		}

		private void PutSplitAmountTransactions(Dictionary<GLTranKey, GLTranForReclassification> parentsList, GLTran tranToEdit)
		{
			var parentKey = new GLTranKey(tranToEdit.OrigModule, tranToEdit.OrigBatchNbr, tranToEdit.OrigLineNbr);

			var parent = parentsList.ContainsKey(parentKey) ? parentsList[parentKey] : null;

			if (parent == null)
			{
				parent = RegenerateParentOfGroup(parentsList, parentKey);
			}

			var tranForReclass = (GLTranForReclassification)GLTranForReclass_Module_BatchNbr_LineNbr.Select(tranToEdit.Module, tranToEdit.BatchNbr, tranToEdit.LineNbr);

			GLTranForReclass.Cache.SetStatus(tranForReclass, PXEntryStatus.Updated);
			InitSplitTranForEditing(tranToEdit, parentKey, parent, tranForReclass);

			State.SplittingGroups[parentKey].Add(new GLTranKey(tranForReclass));
			GLTranForReclass.Cache.RaiseFieldUpdated<GLTranForReclassification.curyNewAmt>(tranForReclass, 0m);
			CalcSelectedFieldValue(tranForReclass);
			GLTranForReclass.Cache.RaiseRowSelected(tranForReclass);
		}

		private void InitSplitTranForEditing(GLTran tranToEdit, GLTranKey parentKey, GLTranForReclassification parent, GLTranForReclassification tranForReclass)
		{
			InitSplitTranForReclassAdditionalFieldsForEditing(tranForReclass, parent, tranToEdit);
			tranForReclass.EditingPairReclassifyingLineNbr = tranToEdit.LineNbr;
			tranForReclass.CuryNewAmt = tranToEdit.CuryDebitAmt + tranToEdit.CuryCreditAmt;
			tranForReclass.ParentKey = parentKey;
			tranForReclass.SourceCuryDebitAmt = tranForReclass.CuryDebitAmt;
			tranForReclass.SourceCuryCreditAmt = tranForReclass.CuryCreditAmt;
			tranForReclass.CuryDebitAmt = 0m;
			tranForReclass.CuryCreditAmt = 0m;
			tranForReclass.ReclassRowType = ReclassRowTypes.Editing;
		}

		private GLTranForReclassification RegenerateParentOfGroup(Dictionary<GLTranKey, GLTranForReclassification> parentsList, GLTranKey parentKey)
		{
			GLTranForReclassification parent = (GLTranForReclassification)GLTranForReclass_Module_BatchNbr_LineNbr.Select(parentKey.Module,
																											parentKey.BatchNbr,
																											parentKey.LineNbr);
			State.SplittingGroups[parentKey] = new List<GLTranKey>();
			GLTranForReclass.Cache.SetStatus(parent, PXEntryStatus.Updated);

			InitTranForReclassAdditionalFieldsForEditing(parent, parent);
			parent.EditingPairReclassifyingLineNbr = parent.LineNbr;
			parent.CuryNewAmt = (parent.CuryReclassRemainingAmt ?? 0m) == 0m ? parent.CuryDebitAmt + parent.CuryCreditAmt : parent.CuryReclassRemainingAmt;
			InitOriginalAmountIfRepeatedReclassification(parent);
			parent.IsSplitted = true;
			parent.ReclassRowType = ReclassRowTypes.EditingVirtualParentTran;
			State.SplittingGroups[parentKey] = new List<GLTranKey>();
			parentsList[parentKey] = parent;
			CalcSelectedFieldValue(parent);
			return parent;
		}

		private void PutReclassificationBatchTransForReversingToCache(string module, string batchNbr, string curyID)
		{
			var transForReclass = GLTransForReclassForReverseView.Select(module, batchNbr)
															.RowCast<GLTranForReclassification>();

			var reverseTrans = ReclassReverseGLTransView.Select(module, batchNbr)
														.RowCast<GLTran>()
														.ToDictionary(tran => tran.LineNbr, tran => tran);

			foreach (var tranForReclass in transForReclass)
			{
				var reverseTran = reverseTrans[tranForReclass.LineNbr.Value - 1];

				InitTranForReclassEditableFieldsFromTran(tranForReclass, reverseTran);
				tranForReclass.CuryID = curyID;

				GLTranForReclass.Cache.SetStatus(tranForReclass, PXEntryStatus.Updated);
                CalcSelectedFieldValue(tranForReclass);
            }
		}

		public static void TryOpenForReclassificationOfDocument(PXView askView, string module, string batchNbr, string docType, string refNbr)
		{
			var graph = PXGraph.CreateInstance<ReclassifyTransactionsProcess>();

			var dataRows = PXSelectJoin<GLTranForReclassification,
									InnerJoin<CurrencyInfo,
										On<GLTran.curyInfoID, Equal<CurrencyInfo.curyInfoID>>>,
									Where<GLTranForReclassification.module, Equal<Required<GLTranForReclassification.module>>,
										And<GLTranForReclassification.batchNbr, Equal<Required<GLTranForReclassification.batchNbr>>,
										And<GLTranForReclassification.tranType, Equal<Required<GLTranForReclassification.tranType>>,
										And<GLTranForReclassification.refNbr, Equal<GLTranForReclassification.refNbr> >>>>>
									.Select(graph, module, batchNbr, docType, refNbr)
									.Select<PXResult<GLTranForReclassification, CurrencyInfo>>();

			var nonReclassifiableFound = false;
			var reclassifiableFound = false;

			foreach (var row in dataRows)
			{
				var tran = (GLTranForReclassification) row;
				var curyInfo = (CurrencyInfo) row;


				if (JournalEntry.IsTransactionReclassifiable(tran, null, null, ProjectDefaultAttribute.NonProject()))
				{
					graph.InitTranForReclassAdditionalFields(tran, curyInfo);

					graph.GLTranForReclass.Cache.SetStatus(tran, PXEntryStatus.Updated);

					reclassifiableFound = true;
				}
				else
				{
					nonReclassifiableFound = true;

					graph.GLTranForReclass.Cache.Remove(tran);
				}
			}

			if (!reclassifiableFound)
			{
				throw new PXException(
					InfoMessages.NoReclassifiableTransactionsHaveBeenFoundInTheBatch);
			}

			if (nonReclassifiableFound)
			{
				askView.Ask(
					InfoMessages.SomeTransactionsOfTheBatchCannotBeReclassified,
					MessageButtons.OK);
			}

			throw new PXRedirectRequiredException(graph, true, string.Empty)
			{
				Mode = PXBaseRedirectException.WindowMode.New
			};
		}

		public static void TryOpenForReclassification<TTran>(IEnumerable<TTran> trans, 
																Ledger ledger,
																Func<TTran, string> getBatchTypeDelegate,
																PXView askView,
																string someTransactionsCannotBeReclassifiedMessage,
																string noTransactionsForWhichTheReclassificationCanBePerformed,
																PXBaseRedirectException.WindowMode redirectMode = PXBaseRedirectException.WindowMode.New)
			where TTran : GLTran
		{
			var nonReclassifiableFound = false;

			var validTrans = new List<TTran>();

			foreach (var tran in trans)
			{
				if (JournalEntry.IsTransactionReclassifiable(tran, getBatchTypeDelegate(tran), ledger?.BalanceType, ProjectDefaultAttribute.NonProject()))
				{
					validTrans.Add(tran);
				}
				else
				{
					nonReclassifiableFound = true;
				}
			}

			if (validTrans.Count == 0)
			{
				throw new PXException(noTransactionsForWhichTheReclassificationCanBePerformed);
			}

			if (nonReclassifiableFound)
			{
				askView.Ask(someTransactionsCannotBeReclassifiedMessage, MessageButtons.OK);
			}

			OpenForReclassification(validTrans, redirectMode);
		}

        public static void TryOpenForReclassification<TTran>(IEnumerable<TTran> trans,
                                                                string batchType,
                                                                PXView askView,
                                                                string someTransactionsCannotBeReclassifiedMessage,
                                                                string noTransactionsForWhichTheReclassificationCanBePerformed,
                                                                PXBaseRedirectException.WindowMode redirectMode = PXBaseRedirectException.WindowMode.New)
            where TTran : GLTran
        {
			TryOpenForReclassification(trans, null, tran => batchType, askView, 
				someTransactionsCannotBeReclassifiedMessage, noTransactionsForWhichTheReclassificationCanBePerformed, redirectMode);
        }

        public static void OpenForReclassification(IReadOnlyCollection<GLTran> trans, PXBaseRedirectException.WindowMode redirectMode = PXBaseRedirectException.WindowMode.New)
		{
			if (trans == null)
				throw new ArgumentNullException("trans");

			var graph = PXGraph.CreateInstance<ReclassifyTransactionsProcess>();

			graph.State.ReclassScreenMode = ReclassScreenMode.Reclassification;
			graph.PutTransForReclassToCacheByKey(trans);

			throw new PXRedirectRequiredException(graph, true, string.Empty)
			{
				Mode = redirectMode
			};
		}

		public static void OpenForReclassBatchEditing(Batch batch)
		{
			var graph = PXGraph.CreateInstance<ReclassifyTransactionsProcess>();

			graph.State.ReclassScreenMode = ReclassScreenMode.Editing;
			graph.State.EditingBatchModule = batch.Module;
			graph.State.EditingBatchNbr = batch.BatchNbr;
			graph.State.EditingBatchMasterPeriodID = batch.TranPeriodID;
			graph.State.EditingBatchCuryID = batch.CuryID;

			graph.PutReclassificationBatchTransForEditingToCache(batch.Module, batch.BatchNbr);

			throw new PXRedirectRequiredException(graph, true, string.Empty)
			{
				Mode = PXBaseRedirectException.WindowMode.Same
			};
		}

		public static void OpenForReclassBatchReversing(Batch batch)
		{
			var graph = PXGraph.CreateInstance<ReclassifyTransactionsProcess>();

			graph.State.ReclassScreenMode = ReclassScreenMode.Reversing;
			graph.State.OrigBatchModuleToReverse = batch.Module;
			graph.State.OrigBatchNbrToReverse = batch.BatchNbr;

			graph.PutReclassificationBatchTransForReversingToCache(batch.Module, batch.BatchNbr, batch.CuryID);

			throw new PXRedirectRequiredException(graph, true, string.Empty)
			{
				Mode = PXBaseRedirectException.WindowMode.Same
			};
		}

		private void InitTranForReclassAdditionalFieldsForEditing(GLTranForReclassification tranForReclass, GLTran tran)
		{
			InitTranForReclassEditableFieldsFromTran(tranForReclass, tran);
			tranForReclass.CuryID = State.EditingBatchCuryID;
		}

        private void InitSplitTranForReclassAdditionalFieldsForEditing(GLTranForReclassification splitForReclass, GLTranForReclassification parentForReclass, GLTran tran)
        {
			splitForReclass.BranchID = parentForReclass.BranchID;
            splitForReclass.AccountID = parentForReclass.AccountID;
            splitForReclass.SubID = parentForReclass.SubID;
            splitForReclass.TranDate = parentForReclass.TranDate;
            splitForReclass.FinPeriodID = parentForReclass.FinPeriodID;
            splitForReclass.TranPeriodID = parentForReclass.TranPeriodID;
            splitForReclass.TranDesc = parentForReclass.TranDesc;
            splitForReclass.CuryDebitAmt = parentForReclass.CuryDebitAmt;
            splitForReclass.CuryCreditAmt = parentForReclass.CuryCreditAmt;
			InitTranForReclassEditableFieldsFromTran(splitForReclass, tran);

            splitForReclass.CuryID = State.EditingBatchCuryID;
        }

        private void InitTranForReclassEditableFieldsFromTran(GLTranForReclassification tranForReclass, GLTran tran)
		{
			tranForReclass.NewBranchID = tran.BranchID;
			tranForReclass.NewAccountID = tran.AccountID;
			tranForReclass.NewSubID = tran.SubID;
			tranForReclass.NewSubCD = null;
			tranForReclass.NewTranDate = tran.TranDate;
			tranForReclass.NewFinPeriodID = tran.FinPeriodID;
			tranForReclass.NewTranDesc = tran.TranDesc;
        }

		private void InitTranForReclassEditableFields(GLTranForReclassification tranForReclass)
		{
			tranForReclass.NewFinPeriodID = tranForReclass.FinPeriodID;
			tranForReclass.NewTranDate = tranForReclass.TranDate;
			tranForReclass.NewBranchID = tranForReclass.BranchID;
			tranForReclass.NewAccountID = tranForReclass.AccountID;
			tranForReclass.NewSubID = tranForReclass.SubID;
			tranForReclass.NewSubCD = null;
			tranForReclass.NewTranDesc = tranForReclass.TranDesc;
			if (State.SplittingGroups.ContainsKey(new GLTranKey(tranForReclass)) == false)
			{
				decimal? oldValue = tranForReclass.CuryNewAmt;
				tranForReclass.CuryNewAmt = 0m;
				GLTranForReclass.Cache.RaiseFieldUpdated<GLTranForReclassification.curyNewAmt>(tranForReclass, oldValue);
			}
		}

		private void InitTranForReclassAdditionalFields(GLTranForReclassification tran, CurrencyInfo curyInfo)
		{
			InitTranForReclassEditableFields(tran);
			tran.CuryID = curyInfo.CuryID;
			tran.VerifyingForFromValuesInvoked = false;
		}

        private void InitOriginalAmountIfRepeatedReclassification(GLTranForReclassification tran)
        {
            if((tran.CuryReclassRemainingAmt ?? 0m) == 0m)
            {
                return;
            }

            tran.CuryDebitAmt = tran.CuryDebitAmt != 0m ? tran.CuryReclassRemainingAmt : 0m;
            tran.CuryCreditAmt = tran.CuryCreditAmt != 0m ? tran.CuryReclassRemainingAmt : 0m;
        }

        private IEnumerable<PXResult<GLTranForReclassification, Account, Sub, Batch, CurrencyInfo, Ledger>> GetTransForReclassByLoadOptions(LoadOptions loadOptions)
		{
			PXSelectBase<GLTranForReclassification> query = new PXSelectJoinOrderBy<GLTranForReclassification,
																		InnerJoin<Account,
																			On<GLTranForReclassification.accountID, Equal<Account.accountID>>,
																		InnerJoin<Sub,
																			On<GLTranForReclassification.subID, Equal<Sub.subID>>,
																		InnerJoin<Batch,
																			On<Batch.module, Equal<GLTranForReclassification.module>,
																				And<Batch.batchNbr, Equal<GLTranForReclassification.batchNbr>>>,
																		InnerJoin<CurrencyInfo,
																			On<GLTranForReclassification.curyInfoID, Equal<CurrencyInfo.curyInfoID>>,
																		InnerJoin<Ledger,
																			On<GLTranForReclassification.ledgerID, Equal<Ledger.ledgerID>>>>>>>,
																		OrderBy<Asc<GLTranForReclassification.module, 
																				Asc<GLTranForReclassification.batchNbr, 
																				Asc<GLTranForReclassification.lineNbr>>>>>
																		(this);

			var pars = new List<object>();

			if (loadOptions.BranchID != null)
			{
				query.WhereAnd<Where<GLTranForReclassification.branchID, Equal<Required<GLTranForReclassification.branchID>>>>();
				pars.Add(loadOptions.BranchID);
			}

			if (loadOptions.FromAccountID != null)
			{
				var fromAccount = AccountAttribute.GetAccount(this, loadOptions.FromAccountID);

				query.WhereAnd<Where<Account.accountCD, GreaterEqual<Required<Account.accountCD>>>>();
				pars.Add(fromAccount.AccountCD);
			}
			if (loadOptions.ToAccountID != null)
			{
				var toAccount = AccountAttribute.GetAccount(this, loadOptions.ToAccountID);

				query.WhereAnd<Where<Account.accountCD, LessEqual<Required<Account.accountCD>>>>();
				pars.Add(toAccount.AccountCD);
			}

			if (loadOptions.FromSubID != null)
			{
				var fromSubaccount = SubAccountAttribute.GetSubaccount(this, loadOptions.FromSubID);

				query.WhereAnd<Where<Sub.subCD, GreaterEqual<Required<Sub.subCD>>>>();
				pars.Add(fromSubaccount.SubCD);
			}
			if (loadOptions.ToSubID != null)
			{
				var toSubaccount = SubAccountAttribute.GetSubaccount(this, loadOptions.ToSubID);

				query.WhereAnd<Where<Sub.subCD, LessEqual<Required<Sub.subCD>>>>();
				pars.Add(toSubaccount.SubCD);
			}

			if (loadOptions.FromDate != null)
			{
				query.WhereAnd<Where<GLTranForReclassification.tranDate, GreaterEqual<Current<LoadOptions.fromDate>>>>();
			}
			if (loadOptions.ToDate != null)
			{
				query.WhereAnd<Where<GLTranForReclassification.tranDate, LessEqual<Current<LoadOptions.toDate>>>>();
			}

			if (loadOptions.FromFinPeriodID != null)
			{
				query.WhereAnd<Where<GLTranForReclassification.finPeriodID, GreaterEqual<Current<LoadOptions.fromFinPeriodID>>>>();
			}
			if (loadOptions.ToFinPeriodID != null)
			{
				query.WhereAnd<Where<GLTranForReclassification.finPeriodID, LessEqual<Current<LoadOptions.toFinPeriodID>>>>();
			}

			if (loadOptions.Module != null)
			{
				query.WhereAnd<Where<GLTranForReclassification.module, Equal<Current<LoadOptions.module>>>>();
			}

			if (loadOptions.BatchNbr != null)
			{
				query.WhereAnd<Where<GLTranForReclassification.batchNbr, Equal<Current<LoadOptions.batchNbr>>>>();
			}

			if (loadOptions.RefNbr != null)
			{
				query.WhereAnd<Where<GLTranForReclassification.refNbr, Equal<Current<LoadOptions.refNbr>>>>();
			}

			if (loadOptions.ReferenceID != null)
			{
				query.WhereAnd<Where<GLTranForReclassification.referenceID, Equal<Current<LoadOptions.referenceID>>>>();
			}

			var trans = loadOptions.MaxTrans == null
				? query.Select(pars.ToArray())
				: query.SelectWindowed(0, (int)loadOptions.MaxTrans, pars.ToArray());

			return trans.Select<PXResult<GLTranForReclassification, Account, Sub, Batch, CurrencyInfo, Ledger>>();
		}

        private void ValidateSplitGroups()
        {
            string error = string.Empty;

            foreach (var group in State.SplittingGroups)
            {
				var parent = GLTranForReclass.Locate(GetGLTranForReclassByKey(group.Key));
				decimal parentBalance = parent.CuryDebitAmt.Value + parent.CuryCreditAmt.Value;
				error = ValidateSplitRecord(parent) ?? error;

				var sign = parent.CuryDebitAmt + parent.CuryCreditAmt > 0m ? 1 : -1;

				foreach (var splitKey in group.Value)
                {
					var split = GLTranForReclass.Locate(GetGLTranForReclassByKey(splitKey));
                    error = ValidateSplitRecord(split) ?? error;

					parentBalance -= split.CuryNewAmt ?? 0m;
					
					if (sign * parentBalance < 0m)
					{
						error = Messages.CannotIncreaseAmtOfSplittedGreatestThanRestOfOriginalTran;
						SetExhaustedAmountError(split);
					}
				}

                if(string.IsNullOrEmpty(error) == false)
                {
                    throw new PXException(error);
                }
			}
        }

		private void SetExhaustedAmountError(GLTranForReclassification tran)
		{
			var fieldError = new PXSetPropertyException(Messages.CannotIncreaseAmtOfSplittedGreatestThanRestOfOriginalTran, PXErrorLevel.Error);
			GLTranForReclass.Cache.RaiseExceptionHandling<GLTranForReclassification.curyNewAmt>(tran, tran.CuryNewAmt, fieldError);
		}

		private string ValidateSplitRecord(GLTranForReclassification row)
        {
            if(!row.IsSplitting && !row.IsSplitted)
            {
                return null;
            }

            if (row.Selected == false)
            {
				var excptn = new PXSetPropertyException(Messages.NotAllMembersOfSplittingGroupWereSelected, PXErrorLevel.RowError);
				GLTranForReclass.Cache.RaiseExceptionHandling(typeof(GLTranForReclassification.selected).Name, row, row.Selected, excptn);
                return Messages.NotAllMembersOfSplittingGroupWereSelected;
            }
            else
            {
                GLTranForReclass.Cache.RaiseExceptionHandling(typeof(GLTranForReclassification.selected).Name, row, row.Selected, null);
                return null;
            }
        }
        #endregion


        #region DACs & DTOs

        public partial class LoadOptions : IBqlTable
		{
			#region BranchID

			public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }

			protected Int32? _BranchID;
			[Branch(IsDBField = false, Required = false)]
			public virtual Int32? BranchID
			{
				get { return this._BranchID; }
				set { this._BranchID = value; }
			}

			#endregion
			#region FromAccountID

			public abstract class fromAccountID : PX.Data.BQL.BqlInt.Field<fromAccountID> { }

			protected Int32? _FromAccountID;

			[AccountAny(DisplayName = "From Account")]
			public virtual Int32? FromAccountID
			{
				get { return this._FromAccountID; }
				set { this._FromAccountID = value; }
			}

			#endregion
			#region ToAccountID

			public abstract class toAccountID : PX.Data.BQL.BqlInt.Field<toAccountID> { }

			protected Int32? _ToAccountID;

			[AccountAny(DisplayName = "To Account")]
			public virtual Int32? ToAccountID
			{
				get { return this._ToAccountID; }
				set { this._ToAccountID = value; }
			}

			#endregion	
			#region FromSubID

			public abstract class fromSubID : PX.Data.BQL.BqlInt.Field<fromSubID> { }

			protected Int32? _FromSubID;
			
			[SubAccount(DisplayName = "From Subaccount")]
			public virtual Int32? FromSubID
			{
				get { return this._FromSubID; }
				set { this._FromSubID = value; }
			}

			#endregion
			#region ToSubID

			public abstract class toSubID : PX.Data.BQL.BqlInt.Field<toSubID> { }

			protected Int32? _ToSubID;

			[SubAccount(DisplayName = "To Subaccount")]
			public virtual Int32? ToSubID
			{
				get { return this._ToSubID; }
				set { this._ToSubID = value; }
			}

			#endregion
			#region FromDate
			public abstract class fromDate : PX.Data.BQL.BqlDateTime.Field<fromDate> { }
			protected DateTime? _FromDate;
			[PXDBDate]
			[PXUIField(DisplayName = "From Date")]
			public virtual DateTime? FromDate
			{
				get
				{
					return _FromDate;
				}
				set
				{
					_FromDate = value;
				}
			}
			#endregion
			#region ToDate
			public abstract class toDate : PX.Data.BQL.BqlDateTime.Field<toDate> { }
			protected DateTime? _ToDate;
			[PXDBDate]
			[PXUIField(DisplayName = "To Date")]
			public virtual DateTime? ToDate
			{
				get
				{
					return _ToDate;
				}
				set
				{
					_ToDate = value;
				}
			}
			#endregion
			#region FromPeriodID
			public abstract class fromFinPeriodID : PX.Data.BQL.BqlString.Field<fromFinPeriodID> { }
			protected String _FromFinPeriodID;
			[FinPeriodSelector(null, 
				typeof(AccessInfo.businessDate),
				branchSourceType: typeof(LoadOptions.branchID),
				useMasterCalendarSourceType: typeof(LoadOptions.useMasterCalendar),
				redefaultOrRevalidateOnOrganizationSourceUpdated: false)]
			[PXUIField(DisplayName = "From Period", Required = false)]
			public virtual String FromFinPeriodID
			{
				get
				{
					return _FromFinPeriodID;
				}
				set
				{
					_FromFinPeriodID = value;
				}
			}
			#endregion
			#region ToFinPeriodID
			public abstract class toFinPeriodID : PX.Data.BQL.BqlString.Field<toFinPeriodID> { }
			protected String _ToFinPeriodID;
			[FinPeriodSelector(null,
				typeof(AccessInfo.businessDate),
				branchSourceType: typeof(LoadOptions.branchID),
				useMasterCalendarSourceType: typeof(LoadOptions.useMasterCalendar),
				redefaultOrRevalidateOnOrganizationSourceUpdated: false)]
			[PXUIField(DisplayName = "To Period", Required = false)]
			public virtual String ToFinPeriodID
			{
				get
				{
					return _ToFinPeriodID;
				}
				set
				{
					_ToFinPeriodID = value;
				}
			}
			#endregion
			#region Module
			public abstract class module : PX.Data.BQL.BqlString.Field<module> { }
			protected String _Module;

			[PXDBString(2, IsFixed = true)]
			[PXUIField(DisplayName = "Module", Visibility = PXUIVisibility.SelectorVisible)]
			[ModuleList()]
			public virtual String Module
			{
				get
				{
					return this._Module;
				}
				set
				{
					this._Module = value;
				}
			}
			#endregion
			#region BatchNbr
			public abstract class batchNbr : PX.Data.BQL.BqlString.Field<batchNbr> { }
			protected String _BatchNbr;

			[PXDBString(15, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
			[PXSelector(typeof(Search<Batch.batchNbr, Where<Batch.module, Equal<Current<LoadOptions.module>>, And<Batch.draft, Equal<False>>>, OrderBy<Desc<Batch.batchNbr>>>), Filterable = true)]
			[PXUIField(DisplayName = "Batch Number", Visibility = PXUIVisibility.SelectorVisible)]
			public virtual String BatchNbr
			{
				get
				{
					return this._BatchNbr;
				}
				set
				{
					this._BatchNbr = value;
				}
			}
			#endregion
			#region RefNbr
			public abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }
			protected String _RefNbr;

			[PXDBString(15, IsUnicode = true)]
			[PXUIField(DisplayName = "Ref. Number")]
			public virtual String RefNbr
			{
				get
				{
					return this._RefNbr;
				}
				set
				{
					this._RefNbr = value;
				}
			}
			#endregion
			#region ReferenceID
			public abstract class referenceID : PX.Data.BQL.BqlInt.Field<referenceID> { }
			protected Int32? _ReferenceID;

			[PXDBInt()]
			[PXSelector(typeof(Search<BAccountR.bAccountID>), SubstituteKey = typeof(BAccountR.acctCD))]
			[PXUIField(DisplayName = "Customer/Vendor")]
			[CustomerVendorRestrictor]
			public virtual Int32? ReferenceID
			{
				get
				{
					return this._ReferenceID;
				}
				set
				{
					this._ReferenceID = value;
				}
			}
			#endregion
			#region MaxTrans
			public abstract class maxTrans : PX.Data.BQL.BqlInt.Field<maxTrans> { }
			protected int? _MaxTrans;
			[PXDBInt]
			[PXUIField(DisplayName = "Max. Number of Transactions")]
			[PXDefault(999, PersistingCheck = PXPersistingCheck.Nothing)]
			public virtual int? MaxTrans
			{
				get
				{
					return _MaxTrans;
				}
				set
				{
					_MaxTrans = value;
				}
			}

			#endregion

			#region PeriodStartDate
			public abstract class periodStartDate : PX.Data.BQL.BqlDateTime.Field<periodStartDate> { }
			protected DateTime? _PeriodStartDate;
			[PXDBDate()]
			public virtual DateTime? PeriodStartDate
			{
				get
				{
					return this._PeriodStartDate;
				}
				set
				{
					this._PeriodStartDate = value;
				}
			}
			#endregion
			#region PeriodEndDate
			public abstract class periodEndDate : PX.Data.BQL.BqlDateTime.Field<periodEndDate> { }
			protected DateTime? _PeriodEndDate;
			
			[PXDBDate()]
			public virtual DateTime? PeriodEndDate
			{
				get
				{
					return this._PeriodEndDate;
				}
				set
				{
					this._PeriodEndDate = value;
				}
			}
			#endregion

			#region UseMasterCalendar
			public abstract class useMasterCalendar : PX.Data.BQL.BqlBool.Field<useMasterCalendar> { }

			[PXBool]
			public bool? UseMasterCalendar { get; set; }
			#endregion

			public class ModuleListAttribute : PXStringListAttribute
			{
				public ModuleListAttribute()
					: base(
					new string[] { BatchModule.GL, BatchModule.AP, BatchModule.AR, BatchModule.CA, BatchModule.IN, BatchModule.DR, BatchModule.FA, BatchModule.PM },
					new string[] { Messages.ModuleGL, Messages.ModuleAP, Messages.ModuleAR, Messages.ModuleCA, Messages.ModuleIN, Messages.ModuleDR, Messages.ModeleFA, Messages.ModulePM}) { }
			}
		}

		public partial class ReplaceOptions : IBqlTable
		{
			public virtual bool Showed { get; set; }

            #region Warning

            public abstract class warning : PX.Data.BQL.BqlString.Field<warning> { }

            [PXString]
            [PXUIField(DisplayName = "", IsReadOnly = true, Visible = false)]
            public virtual string Warning { get; set; }

            #endregion
            #region WithBranchID

            public abstract class withBranchID : PX.Data.BQL.BqlInt.Field<withBranchID> { }

			protected Int32? _WithBranchID;

			[Branch(useDefaulting: false, IsDBField = false, Required = false)]
			public virtual Int32? WithBranchID
			{
				get { return this._WithBranchID; }
				set { this._WithBranchID = value; }
			}

			#endregion
			#region WithAccountID

			public abstract class withAccountID : PX.Data.BQL.BqlInt.Field<withAccountID> { }

			protected Int32? _WithAccountID;

			[AccountAny(DisplayName = "Account", AvoidControlAccounts = true)]
			public virtual Int32? WithAccountID
			{
				get { return this._WithAccountID; }
				set { this._WithAccountID = value; }
			}

			#endregion
			#region WithSubID

			public abstract class withSubID : PX.Data.BQL.BqlInt.Field<withSubID> { }

			protected Int32? _WithSubID;

			[SubAccount(DisplayName = "Subaccount")]
			public virtual Int32? WithSubID
			{
				get { return this._WithSubID; }
				set { this._WithSubID = value; }
			}

			#endregion
			#region WithDate
			public abstract class withDate : PX.Data.BQL.BqlDateTime.Field<withDate> { }
			protected DateTime? _WithDate;
			[PXDBDate]
			[PXUIField(DisplayName = "Date")]
			public virtual DateTime? WithDate
			{
				get
				{
					return _WithDate;
				}
				set
				{
					_WithDate = value;
				}
			}
			#endregion
			#region WithFinPeriodID
			public abstract class withFinPeriodID : PX.Data.BQL.BqlString.Field<withFinPeriodID> { }
			protected String _WithFinPeriodID;
			[OpenPeriod(null,
				typeof(ReplaceOptions.withDate),
				typeof(ReplaceOptions.withBranchID),
				redefaultOrRevalidateOnOrganizationSourceUpdated: false,
				useMasterOrganizationIDByDefault: true,
				IsDBField = false)]
			[PXUIField(DisplayName = "Post Period", Visibility = PXUIVisibility.SelectorVisible, Required = false)]
			public virtual String WithFinPeriodID
			{
				get
				{
					return _WithFinPeriodID;
				}
				set
				{
					_WithFinPeriodID = value;
				}
			}
			#endregion
			#region WithTranDesc
			public abstract class withTranDescFilteringValue : PX.Data.BQL.BqlString.Field<withTranDescFilteringValue> { }
			protected String _WithTranDescFilteringValue;

			[PXString(256, IsUnicode = true)]
			[PXUIField(DisplayName = "Transaction Description")]
			public virtual String WithTranDescFilteringValue
			{
				get
				{
					return this._WithTranDescFilteringValue;
				}
				set
				{
					this._WithTranDescFilteringValue = value;
				}
			}
			#endregion

			#region NewBranchID

			public abstract class newBranchID : PX.Data.BQL.BqlInt.Field<newBranchID> { }

			protected Int32? _NewBranchID;

			[Branch(useDefaulting: false, DisplayName = "Branch", IsDBField = false, Required = false)]
			public virtual Int32? NewBranchID
			{
				get { return this._NewBranchID; }
				set { this._NewBranchID = value; }
			}

			#endregion
			#region NewAccountID

			public abstract class newAccountID : PX.Data.BQL.BqlInt.Field<newAccountID> { }

			protected Int32? _NewAccountID;

			[AccountAny(DisplayName = "Account", AvoidControlAccounts = true)]
			public virtual Int32? NewAccountID
			{
				get { return this._NewAccountID; }
				set { this._NewAccountID = value; }
			}

			#endregion
			#region NewSubID

			public abstract class newSubID : PX.Data.BQL.BqlInt.Field<newSubID> { }

			protected Int32? _NewSubID;

			[SubAccount(DisplayName = "Subaccount")]
			public virtual Int32? NewSubID
			{
				get { return this._NewSubID; }
				set { this._NewSubID = value; }
			}

			#endregion
			#region NewDate
			public abstract class newDate : PX.Data.BQL.BqlDateTime.Field<newDate> { }
			protected DateTime? _NewDate;
			[PXDBDate]
			[PXUIField(DisplayName = "Date")]
			public virtual DateTime? NewDate
			{
				get
				{
					return _NewDate;
				}
				set
				{
					_NewDate = value;
				}
			}
			#endregion
			#region NewFinPeriodID
			public abstract class newFinPeriodID : PX.Data.BQL.BqlString.Field<newFinPeriodID> { }
			protected String _NewFinPeriodID;

			//Used only for validation
			[OpenPeriod(null, typeof(ReplaceOptions.newDate), typeof(ReplaceOptions.newBranchID), IsDBField = false)]
			public virtual String NewFinPeriodID
			{
				get
				{
					return _NewFinPeriodID;
				}
				set
				{
					_NewFinPeriodID = value;
				}
			}
			#endregion
			#region NewTranDesc
			public abstract class newTranDesc : PX.Data.BQL.BqlString.Field<newTranDesc> { }
			protected String _NewTranDesc;

			[PXString(256, IsUnicode = true)]
			[PXUIField(DisplayName = "Transaction Description")]
			public virtual String NewTranDesc
			{
				get
				{
					return this._NewTranDesc;
				}
				set
				{
					this._NewTranDesc = value;
				}
			}
			#endregion
		}

		#endregion
	}
}