using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PX.Common;
using PX.Data;
using PX.Objects.GL.DAC;
using PX.Objects.GL.FinPeriods;
using PX.Objects.WZ;
using PX.Objects.AR;
using PX.Objects.SO;
using PX.CloudServices.DocumentRecognition.InvoiceRecognition;

namespace PX.Objects.CS
{

	public class FeaturesMaint : PXGraph<FeaturesMaint>
	{
		[InjectDependency]
		internal IInvoiceRecognitionClient ImageRecognitionClient { get; set; }

		public PXFilter<AfterActivation> ActivationBehaviour;

		public PXSelect<FeaturesSet> Features;

		protected IEnumerable features()
		{
			FeaturesSet current = (FeaturesSet)PXSelect<FeaturesSet,
								  Where<True, Equal<True>>,
								  OrderBy<Desc<FeaturesSet.status>>>
								  .SelectWindowed(this, 0, 1) ?? Features.Insert();
			current.LicenseID = PXVersionInfo.InstallationID;
			yield return current;
		}

		public FeaturesMaint()
		{
			SaveClose.SetVisible(false);
		}

		public PXSave<FeaturesSet> Save;
		public PXSaveClose<FeaturesSet> SaveClose;
		public PXCancel<FeaturesSet> Cancel;
		public PXAction<FeaturesSet> Insert;

		public PXAction<FeaturesSet> RequestValidation;
		public PXAction<FeaturesSet> CancelRequest;

		public PXSelectJoin<
						MasterFinPeriod,
						InnerJoin<OrganizationFinPeriod,
							On<MasterFinPeriod.finPeriodID, Equal<OrganizationFinPeriod.masterFinPeriodID>,
							And<OrganizationFinPeriod.organizationID, Equal<Required<OrganizationFinPeriod.organizationID>>>>>> MasterFinPeriods;

		public const int MAX_FINPERIOD_DISCREPANCY_MESSAGE_COUNT = 20;

		public override IEnumerable ExecuteSelect(string viewName, object[] parameters, object[] searches, string[] sortcolumns, bool[] descendings, PXFilterRow[] filters, ref int startRow, int maximumRows, ref int totalRows)
		{
			if (viewName == "Features")
				searches = null;

			return base.ExecuteSelect(viewName, parameters, searches, sortcolumns, descendings, filters, ref startRow, maximumRows, ref totalRows);
		}

		[PXButton]
		[PXUIField(DisplayName = "Modify", MapEnableRights = PXCacheRights.Insert, MapViewRights = PXCacheRights.Select)]
		public IEnumerable insert(PXAdapter adapter)
		{
			var activationMode = this.ActivationBehaviour.Current;
			foreach (var item in new PXInsert<FeaturesSet>(this, "Insert").Press(adapter))
			{
				this.ActivationBehaviour.Cache.SetValueExt<AfterActivation.refresh>(this.ActivationBehaviour.Current, activationMode.Refresh);
				yield return item;
			}

		}

		[PXButton]
		[PXUIField(DisplayName = "Enable")]
		public IEnumerable requestValidation(PXAdapter adapter)
		{
			foreach (FeaturesSet feature in adapter.Get())
			{
				if (feature.Status == 3)
				{
					bool? customerDiscountsOld = PXAccess.FeatureInstalled<FeaturesSet.customerDiscounts>();
					bool? branchOld = PXAccess.FeatureInstalled<FeaturesSet.branch>();
					PXCache cache = new PXCache<FeaturesSet>(this);
					FeaturesSet update = PXCache<FeaturesSet>.CreateCopy(feature);
					update.Status = 0;
					update = this.Features.Update(update);
					this.Features.Delete(feature);

					if (update.Status != 1)
						this.Features.Delete(new FeaturesSet() { Status = 1 });

					this.Persist();
					PXAccess.Version++;

					var tasks = PXSelect<WZTask>.Select(this);
					WZTaskEntry taskGraph = CreateInstance<WZTaskEntry>();
					foreach (WZTask task in tasks)
					{
						bool disableTask = false;
						bool enableTask = false;
						foreach (
							WZTaskFeature taskFeature in
								PXSelectReadonly<WZTaskFeature, Where<WZTaskFeature.taskID, Equal<Required<WZTask.taskID>>>>.Select(
									this, task.TaskID))
						{
							bool featureInstalled = (bool?)cache.GetValue(update, taskFeature.Feature) == true;

							if (!featureInstalled)
							{
								disableTask = true;
								enableTask = false;
								break;
							}

							enableTask = true;
						}

						if (disableTask)
						{
							task.Status = WizardTaskStatusesAttribute._DISABLED;
							taskGraph.TaskInfo.Update(task);
							taskGraph.Save.Press();
						}

						if (enableTask && task.Status == WizardTaskStatusesAttribute._DISABLED)
						{

							bool needToBeOpen = false;
							WZScenario scenario = PXSelect<WZScenario, Where<WZScenario.scenarioID, Equal<Required<WZTask.scenarioID>>>>.Select(this, task.ScenarioID);
							if (scenario != null && scenario.Status == WizardScenarioStatusesAttribute._ACTIVE)
							{
								WZTask parentTask =
									PXSelect<WZTask, Where<WZTask.taskID, Equal<Required<WZTask.parentTaskID>>>>.Select(
										this, task.ParentTaskID);

								if (parentTask != null && (parentTask.Status == WizardTaskStatusesAttribute._OPEN ||
														   parentTask.Status == WizardTaskStatusesAttribute._ACTIVE))
								{
									needToBeOpen = true;
								}

								foreach (
									PXResult<WZTaskPredecessorRelation, WZTask> predecessorResult in
										PXSelectJoin<WZTaskPredecessorRelation,
											InnerJoin
												<WZTask,
													On<WZTask.taskID, Equal<WZTaskPredecessorRelation.predecessorID>>>,
											Where<WZTaskPredecessorRelation.taskID, Equal<Required<WZTask.taskID>>>>.
											Select(this, task.TaskID))
								{
									WZTask predecessorTask = (WZTask)predecessorResult;
									if (predecessorTask != null)
									{
										if (predecessorTask.Status == WizardTaskStatusesAttribute._COMPLETED)
										{
											needToBeOpen = true;

										}
										else
										{
											needToBeOpen = false;
											break;
										}
									}
								}
							}
							task.Status = needToBeOpen ? WizardTaskStatusesAttribute._OPEN : WizardTaskStatusesAttribute._PENDING;
							taskGraph.TaskInfo.Update(task);
							taskGraph.Save.Press();
						}
					}

					if (customerDiscountsOld == true && update.CustomerDiscounts != true)
					{
						PXUpdate<Set<ARSetup.applyLineDiscountsIfCustomerPriceDefined, True>, ARSetup>.Update(this);
						PXUpdate<Set<ARSetup.applyLineDiscountsIfCustomerClassPriceDefined, True>, ARSetup>.Update(this);
						PXUpdate<Set<SOOrderType.recalculateDiscOnPartialShipment, False, Set<SOOrderType.postLineDiscSeparately, False>>, SOOrderType>.Update(this);
					}

					if (branchOld != update.Branch)
					{
						PXUpdate<Set<ListEntryPoint.isActive, Required<ListEntryPoint.isActive>>, ListEntryPoint, 
							Where<ListEntryPoint.entryScreenID, Equal<Required<ListEntryPoint.entryScreenID>>>>
							.Update(this, update.Branch == true, "CS101500");
					}

					yield return update;
				}
				else
					yield return feature;
			}

			bool needRefresh = !(ActivationBehaviour.Current != null && ActivationBehaviour.Current.Refresh == false);

			PXDatabase.ResetSlots();
			PXPageCacheUtils.InvalidateCachedPages();
			this.Clear();
			if (needRefresh)
				throw new PXRefreshException();
		}

		[PXButton]
		[PXUIField(DisplayName = "Cancel Validation Request", Visible = false)]
		public IEnumerable cancelRequest(PXAdapter adapter)
		{
			foreach (FeaturesSet feature in adapter.Get())
			{
				if (feature.Status == 2)
				{
					FeaturesSet update = PXCache<FeaturesSet>.CreateCopy(feature);
					update.Status = 3;
					this.Features.Delete(feature);
					update = this.Features.Update(update);
					this.Persist();
					yield return update;
				}
				else
					yield return feature;
			}
		}

		protected virtual void FeaturesSet_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			this.Save.SetVisible(false);
			this.Features.Cache.AllowInsert = true;
			FeaturesSet row = (FeaturesSet)e.Row;
			if (row == null) return;

			this.RequestValidation.SetEnabled(row.Status == 3);
			this.CancelRequest.SetEnabled(row.Status == 2);
			this.Features.Cache.AllowInsert = row.Status < 2;
			this.Features.Cache.AllowUpdate = row.Status == 3;
			this.Features.Cache.AllowDelete = false;

			bool screenIsOpenedFromScenario = !(ActivationBehaviour.Current != null && ActivationBehaviour.Current.Refresh == true);
			if (screenIsOpenedFromScenario && this.Actions.Contains("CancelClose"))
				this.Actions["CancelClose"].SetTooltip(WZ.Messages.BackToScenario);
		}

		protected virtual void FeaturesSet_RowInserting(PXCache sender, PXRowInsertingEventArgs e)
		{
			int? status = (int?)sender.GetValue<FeaturesSet.status>(e.Row);
			if (status != 3) return;

			FeaturesSet current = PXSelect<FeaturesSet,
				Where<True, Equal<True>>,
				OrderBy<Desc<FeaturesSet.status>>>
				.SelectWindowed(this, 0, 1);
			if (current != null)
			{
				sender.RestoreCopy(e.Row, current);
				sender.SetValue<FeaturesSet.status>(e.Row, 3);
			}
		}

		protected virtual void CheckMasterOrganizationCalendarDiscrepancy()
		{
			int messageCount = 0;
			bool isError = false;

			foreach (Organization organization in PXSelect<Organization>.Select(this))
			{
				foreach (MasterFinPeriod problemPeriod in PXSelectJoin<
					MasterFinPeriod,
					LeftJoin<OrganizationFinPeriod,
						On<MasterFinPeriod.finPeriodID, Equal<OrganizationFinPeriod.masterFinPeriodID>,
						And<OrganizationFinPeriod.organizationID, Equal<Required<OrganizationFinPeriod.organizationID>>>>>,
					Where<OrganizationFinPeriod.finPeriodID, IsNull>>
					.Select(this, organization.OrganizationID))
				{
					isError = true;
					if (messageCount <= MAX_FINPERIOD_DISCREPANCY_MESSAGE_COUNT)
					{
						PXTrace.WriteError(GL.Messages.DiscrepancyPeriod, organization.OrganizationCD, problemPeriod.FinPeriodID);

						messageCount++;
					}
					else
					{
						break;
					}
				}
			}

			if (isError)
			{
				throw new PXSetPropertyException(GL.Messages.DiscrepancyPeriodError);
			}
		}

		private Organization etalonOrganization = null;
		protected Organization EtalonOrganization => etalonOrganization ?? (etalonOrganization = PXSelect<Organization>.SelectSingleBound(this, new object[] { }));

		protected virtual void CheckOrganizationCalendarFieldsDiscrepancy()
		{
			int messageCount = 0;
			bool isError = false;

			if (EtalonOrganization != null)
			{
				foreach (Organization organization in PXSelect<
					Organization, 
					Where<Organization.organizationID, NotEqual<Required<Organization.organizationID>>>>
					.Select(this, EtalonOrganization.OrganizationID))
				{
					foreach (OrganizationFinPeriod problemPeriod in PXSelectJoin<
						OrganizationFinPeriod,
						LeftJoin<OrganizationFinPeriodStatus,
							On<OrganizationFinPeriodStatus.organizationID, Equal<Required<OrganizationFinPeriodStatus.organizationID>>,
							And<OrganizationFinPeriod.finPeriodID, Equal<OrganizationFinPeriodStatus.finPeriodID>,
							And<OrganizationFinPeriod.dateLocked, Equal<OrganizationFinPeriodStatus.dateLocked>,
							And<OrganizationFinPeriod.status, Equal<OrganizationFinPeriodStatus.status>,
							And<OrganizationFinPeriod.aPClosed, Equal<OrganizationFinPeriodStatus.aPClosed>,
							And<OrganizationFinPeriod.aRClosed, Equal<OrganizationFinPeriodStatus.aRClosed>,
							And<OrganizationFinPeriod.iNClosed, Equal<OrganizationFinPeriodStatus.iNClosed>,
							And<OrganizationFinPeriod.cAClosed, Equal<OrganizationFinPeriodStatus.cAClosed>,
							And<OrganizationFinPeriod.fAClosed, Equal<OrganizationFinPeriodStatus.fAClosed>>>>>>>>>>>,
						Where<OrganizationFinPeriodStatus.finPeriodID, IsNull,
							And<OrganizationFinPeriod.organizationID, Equal<Required<OrganizationFinPeriod.organizationID>>>>>
					.Select(this, organization.OrganizationID, EtalonOrganization.OrganizationID))
					{
						isError = true;
						if (messageCount <= MAX_FINPERIOD_DISCREPANCY_MESSAGE_COUNT)
						{
							string problemFields = GetProblemFields(organization, problemPeriod);

							PXTrace.WriteError(GL.Messages.DiscrepancyField,
								EtalonOrganization.OrganizationCD,
								organization.OrganizationCD,
								problemFields,
								problemPeriod.FinPeriodID);

							messageCount++;
						}
						else
						{
							break;
						}
					}
				}

				if (isError)
				{
					throw new PXSetPropertyException(GL.Messages.DiscrepancyFieldError);
				}
			}
		}

		private void CheckFullLengthOrganizationCalendars()
		{
			List<string> organizations = PXSelectJoinGroupBy<
				MasterFinPeriod,
				CrossJoin<Organization,
				LeftJoin<OrganizationFinPeriod, 
					On<MasterFinPeriod.finPeriodID, Equal<OrganizationFinPeriod.masterFinPeriodID>,
					And<Organization.organizationID, Equal<OrganizationFinPeriod.organizationID>>>>>,
				Where<OrganizationFinPeriod.finPeriodID, IsNull>,
				Aggregate<
					GroupBy<Organization.organizationCD>>>
				.Select(this)
				.RowCast<Organization>()
				.Select(org => org.OrganizationCD.Trim())
				.ToList();

			if (organizations.Any())
			{
				throw new PXSetPropertyException(GL.Messages.ShortOrganizationCalendarsDetected, string.Join(", ", organizations));
			}
		}

		private void CheckUnshiftedOrganizationCalendars()
		{
			List<string> organizations = PXSelectJoinGroupBy<
				OrganizationFinPeriod, 
				InnerJoin<Organization, 
					On<OrganizationFinPeriod.organizationID, Equal<Organization.organizationID>>>, 
				Where<OrganizationFinPeriod.finPeriodID, NotEqual<OrganizationFinPeriod.masterFinPeriodID>>, 
				Aggregate<
					GroupBy<Organization.organizationCD>>>
				.Select(this)
				.RowCast<Organization>()
				.Select(org => org.OrganizationCD.Trim())
				.ToList();

			if (organizations.Any())
			{
				throw new PXSetPropertyException(GL.Messages.ShiftedOrganizationCalendarsDetected, string.Join(", ", organizations));
			}
		}

		protected virtual void _(Events.FieldUpdating<FeaturesSet, FeaturesSet.multipleCalendarsSupport> e)
		{
			if (e.Row == null) return;

			if (e.Row.MultipleCalendarsSupport == true &&  (bool?)e.NewValue != true) // try to unset
			{
				CheckUnshiftedOrganizationCalendars();
				CheckFullLengthOrganizationCalendars();
			}
		}

		protected virtual void _(Events.FieldUpdating<FeaturesSet, FeaturesSet.centralizedPeriodsManagement> e)
		{
			e.NewValue = PXBoolAttribute.ConvertValue(e.NewValue);
			if (e.Row == null) return;

			if (e.Row.CentralizedPeriodsManagement != null && e.Row.CentralizedPeriodsManagement != (bool)e.NewValue && (bool)e.NewValue == true) // try to set
			{
				CheckMasterOrganizationCalendarDiscrepancy();
				CheckOrganizationCalendarFieldsDiscrepancy();
			}
		}

		protected virtual void _(Events.FieldUpdated<FeaturesSet, FeaturesSet.centralizedPeriodsManagement> e)
		{
			if (e.Row == null) return;

			if ((bool?)e.OldValue != true && 
				e.Row.CentralizedPeriodsManagement == true &&
				EtalonOrganization != null)
			{
				foreach (PXResult<MasterFinPeriod, OrganizationFinPeriod> res in MasterFinPeriods.Select(EtalonOrganization.OrganizationID))
				{
					MasterFinPeriod masterFinPeriod = res;
					OrganizationFinPeriod organizationFinPeriod = res;

					masterFinPeriod.DateLocked = organizationFinPeriod.DateLocked;
					masterFinPeriod.Status = organizationFinPeriod.Status;
					masterFinPeriod.APClosed = organizationFinPeriod.APClosed;
					masterFinPeriod.ARClosed = organizationFinPeriod.ARClosed;
					masterFinPeriod.INClosed = organizationFinPeriod.INClosed;
					masterFinPeriod.CAClosed = organizationFinPeriod.CAClosed;
					masterFinPeriod.FAClosed = organizationFinPeriod.FAClosed;

					MasterFinPeriods.Cache.Update(masterFinPeriod);
				}
			}
		}

		private string GetProblemFields(Organization organization, OrganizationFinPeriod problemPeriod)
		{
			OrganizationFinPeriod currentFinPeriod = PXSelect<
				OrganizationFinPeriod,
				Where<OrganizationFinPeriod.organizationID, Equal<Required<OrganizationFinPeriod.organizationID>>,
					And<OrganizationFinPeriod.finPeriodID, Equal<Required<OrganizationFinPeriod.finPeriodID>>>>>
				.Select(this, organization.OrganizationID, problemPeriod.FinPeriodID);

			List<string> fieldList = new List<string>();
			if (problemPeriod.DateLocked != currentFinPeriod.DateLocked)
				fieldList.Add(nameof(problemPeriod.DateLocked));

			if (problemPeriod.Status != currentFinPeriod.Status)
				fieldList.Add(nameof(problemPeriod.Status));

			if (problemPeriod.APClosed != currentFinPeriod.APClosed)
				fieldList.Add(nameof(problemPeriod.APClosed));

			if (problemPeriod.ARClosed != currentFinPeriod.ARClosed)
				fieldList.Add(nameof(problemPeriod.ARClosed));

			if (problemPeriod.INClosed != currentFinPeriod.INClosed)
				fieldList.Add(nameof(problemPeriod.INClosed));

			if (problemPeriod.CAClosed != currentFinPeriod.CAClosed)
				fieldList.Add(nameof(problemPeriod.CAClosed));

			if (problemPeriod.FAClosed != currentFinPeriod.FAClosed)
				fieldList.Add(nameof(problemPeriod.FAClosed));

			return String.Join(", ", fieldList.ToArray());
		}

		protected virtual void _(Events.FieldUpdating<FeaturesSet.aSC606> e)
		{
			e.NewValue = PXBoolAttribute.ConvertValue(e.NewValue);

			FeaturesSet row = (FeaturesSet)e.Row;
			if (row == null) return;

			bool? oldValue = row.ASC606;

			if (row.ASC606 != null && oldValue != (bool)e.NewValue)
			{
				int? result = PXSelectGroupBy<
					ARTranAlias, 
					Aggregate<Count>>
					.SelectSingleBound(this, null)
					.RowCount;

				if (result > 0)
				{
					string question = PXMessages.LocalizeFormatNoPrefixNLA(AR.Messages.UnreleasedDocsWithDRCodes, result);
					WebDialogResult wdr = Features.Ask(question, MessageButtons.YesNo);
					if (wdr != WebDialogResult.Yes)
					{
						e.NewValue = oldValue;
						e.Cancel = true;
						return;
					}
				}

				//The system calculates the number of Stock and Non-Stock Inventories 
				//in Active status which have MDA deferral code and empty field Allocation Method in Revenue Components.
				if ((bool)e.NewValue == false)
				{
					//use AR.Messages.MDAInventoriesWithoutAllocationMethod
				}
			}
		}

		protected virtual void _(Events.FieldVerifying<FeaturesSet, FeaturesSet.multipleCalendarsSupport> e)
		{
			if (((FeaturesSet)e.Cache.Current)?.CentralizedPeriodsManagement == true && (bool?)e.NewValue == true && (bool?)e.OldValue == false)
			{
				e.NewValue = e.OldValue;
				throw new PXSetPropertyException(Messages.FeaturesAvoidanceDisablingRequiredToEnable, PXUIFieldAttribute.GetDisplayName<FeaturesSet.centralizedPeriodsManagement>(e.Cache));
			}
		}

		protected virtual void _(Events.FieldVerifying<FeaturesSet, FeaturesSet.centralizedPeriodsManagement> e)
		{
			if (((FeaturesSet)e.Cache.Current)?.MultipleCalendarsSupport == true && (bool?)e.NewValue == true && (bool?)e.OldValue == false)
			{
				e.NewValue = e.OldValue;
				throw new PXSetPropertyException(Messages.FeaturesAvoidanceDisablingRequiredToEnable, PXUIFieldAttribute.GetDisplayName<FeaturesSet.multipleCalendarsSupport>(e.Cache));
			}

			if (((FeaturesSet)e.Cache.Current)?.Branch == false && (bool?)e.NewValue == false && (bool?)e.OldValue == true)
			{
				e.NewValue = e.OldValue;
				throw new PXSetPropertyException(Messages.FeaturesAvoidanceEnablingRequiredToDisable, PXUIFieldAttribute.GetDisplayName<FeaturesSet.branch>(e.Cache));
			}

		}
		protected virtual void _(Events.FieldVerifying<FeaturesSet, FeaturesSet.branch> e)
		{
			if (((FeaturesSet)e.Cache.Current)?.CentralizedPeriodsManagement == false && (bool?)e.NewValue == false && (bool?)e.OldValue == true)
			{
				e.NewValue = e.OldValue;
				throw new PXSetPropertyException(Messages.FeaturesAvoidanceEnablingRequiredToDisable, PXUIFieldAttribute.GetDisplayName<FeaturesSet.centralizedPeriodsManagement>(e.Cache));
			}
		}

		protected virtual void _(Events.FieldVerifying<FeaturesSet.imageRecognition> e)
		{
			var tryToActivate = (bool?)e.NewValue == true && (bool?)e.OldValue == false;
			if (!tryToActivate)
			{
				return;
			}

			if (!ImageRecognitionClient.IsConfigured())
			{
				e.NewValue = false;
				PopupNoteManager.Message = PXMessages.LocalizeNoPrefix(Messages.FeatureImageRecognitionIsNotConfigured);

				throw new PXSetPropertyException(Messages.FeatureImageRecognitionIsNotConfigured);
			}
		}
	}

	[Serializable]
	public partial class AfterActivation : IBqlTable
	{
		#region Refresh
		public abstract class refresh : PX.Data.BQL.BqlBool.Field<refresh> { }

		protected Boolean? _Refresh;
		[PXDBBool]
		public virtual Boolean? Refresh
		{
			get
			{
				return this._Refresh;
			}
			set
			{
				this._Refresh = value;
			}
		}
		#endregion
	}

	[Serializable]
	[PXProjection(typeof(Select4<
					ARTran,
					Where<ARTran.released, NotEqual<True>,
						And<ARTran.deferredCode, IsNotNull>>,
					Aggregate<
						GroupBy<ARTran.refNbr,
						GroupBy<ARTran.tranType>>>>))]
	public partial class ARTranAlias : IBqlTable
	{
		#region TranType
		public abstract class tranType : IBqlField { }
		[PXDBString(3, IsKey = true, IsFixed = true, BqlField = typeof(ARTran.tranType))]
		public virtual string TranType { get; set; }
		#endregion
		#region RefNbr
		public abstract class refNbr : IBqlField { }
		[PXDBString(15, IsUnicode = true, IsKey = true, BqlField = typeof(ARTran.refNbr))]
		public virtual string RefNbr { get; set; }
		#endregion
	}
}
