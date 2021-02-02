using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using PX.Data;

using PX.Objects.GL.Overrides.ScheduleProcess;
using PX.Objects.CS;
using PX.Objects.CM;
using PX.Objects.Common;
using PX.Objects.GL.FinPeriods.TableDefinition;
using PX.Objects.GL.FinPeriods;

namespace PX.Objects.GL
{
	public class ScheduleDet
	{
		public DateTime? ScheduledDate;
		public string ScheduledPeriod;
	}

	[PX.Objects.GL.TableAndChartDashboardType]
	public class ScheduleRun : ScheduleRunBase<ScheduleRun, ScheduleMaint, ScheduleProcess>
	{
		[Serializable]
		public partial class Parameters : IBqlTable
		{
			#region StartDate
			[Obsolete("This field is not used anymore and will be removed in Acumatica 8.0")]
			public abstract class startDate : PX.Data.BQL.BqlDateTime.Field<startDate> { }
			[Obsolete("This field is not used anymore and will be removed in Acumatica 8.0")]
			public virtual DateTime? StartDate { get; set; }
			#endregion
			#region LimitTypeSel
			public abstract class limitTypeSel : PX.Data.BQL.BqlString.Field<limitTypeSel> { }
			[PXDBString(1, IsFixed = true)]
			[PXUIField(DisplayName = Messages.Stop, Visibility = PXUIVisibility.Visible, Required = true)]
			[PXDefault(ScheduleRunLimitType.StopAfterNumberOfExecutions)]
			[LabelList(typeof(ScheduleRunLimitType))]
			public virtual string LimitTypeSel { get; set; }
			#endregion
			#region EndDate
			[Obsolete("This field is not used anymore and will be removed in Acumatica 8.0")]
			public abstract class endDate : PX.Data.BQL.BqlDateTime.Field<endDate> { }
			[Obsolete("This field is not used anymore and will be removed in Acumatica 8.0")]
			public virtual DateTime? EndDate { get; set; }
			#endregion
			#region ExecutionDate
			public abstract class executionDate : PX.Data.BQL.BqlDateTime.Field<executionDate> { }
			[PXDBDate]
			[PXDefault(typeof(AccessInfo.businessDate))]
			[PXUIField(DisplayName = Messages.ExecutionDate, Visibility = PXUIVisibility.Visible)]
			public virtual DateTime? ExecutionDate { get; set; }
			#endregion
			#region RunLimit
			public abstract class runLimit : PX.Data.BQL.BqlShort.Field<runLimit> { }
			[PXDBShort(MinValue = 1)]
			[PXUIField(DisplayName = Messages.StopAfterNumberOfExecutions, Visibility = PXUIVisibility.Visible)]
			[PXDefault((short)1, PersistingCheck = PXPersistingCheck.Nothing)]
			public virtual short? RunLimit { get; set; }
			#endregion
		}
	
		public PXSetup<GLSetup> GLSetup;

		protected override bool checkAnyScheduleDetails => false;

		public ScheduleRun()
		{
			GLSetup setup = GLSetup.Current;

			Schedule_List.WhereAnd<Where<
				Schedule.module, Equal<BatchModule.moduleGL>>>();

			Schedule_List.WhereAnd<Where<Exists<
				Select<Batch,
				Where<Batch.scheduleID, Equal<Schedule.scheduleID>,
					And<Batch.scheduled, Equal<True>>>>>>>();
		}

		public PXAction<Parameters> EditDetail;
		[PXUIField(DisplayName = "", Visible = false, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
        [PXEditDetailButton]
		public virtual IEnumerable editDetail(PXAdapter adapter) => ViewScheduleAction(adapter);

		[Obsolete("This method has been moved to " + nameof(ScheduleRunBase))]
		public static void SetProcessDelegate<ProcessGraph>(PXGraph graph, ScheduleRun.Parameters filter, PXProcessing<Schedule> view)
			where ProcessGraph : PXGraph<ProcessGraph>, IScheduleProcessing, new()
			=> ScheduleRunBase.SetProcessDelegate<ProcessGraph>(graph, filter, view);
	}

	public class ScheduleProcess : PXGraph<ScheduleProcess>, IScheduleProcessing
	{
		public PXSelect<Schedule> Running_Schedule;
		public PXSelect<BatchNew> Batch_Created;
		public PXSelect<GLTranNew> Tran_Created;
		public PXSelect<CurrencyInfo> CuryInfo_Created;

		[InjectDependency]
		public IFinPeriodRepository FinPeriodRepository { get; set; }

		public GLSetup GLSetup
		{
			get
			{
				return PXSelect<GLSetup>.Select(this);
			}
		}

		protected virtual void BatchNew_BatchNbr_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			// TODO: Need to clarify for what purpose Cancel is set to true.
			// -
			e.Cancel = true;
		}

		[Obsolete("Please use " + nameof(Scheduler.MakeSchedule), false)]
        private List<ScheduleDet> MakeSchedule(Schedule schedule, short times, DateTime runDate)
			=> MakeSchedule(this, schedule, times, runDate);

		[Obsolete("Please use " + nameof(Scheduler.MakeSchedule), false)]
		public static List<ScheduleDet> MakeSchedule(PXGraph graph, Schedule schedule, short times)
			=> MakeSchedule(graph, schedule, times, graph.Accessinfo.BusinessDate.Value);

		[Obsolete("Please use " + nameof(Scheduler.MakeSchedule), false)]
		public static List<ScheduleDet> MakeSchedule(PXGraph graph, Schedule schedule, short times, DateTime runDate)
			=> new Scheduler(graph).MakeSchedule(schedule, times, runDate).ToList();

		[Obsolete("Please use " + nameof(Scheduler.GetNextRunDate), false)]
		public static DateTime? GetNextRunDate(PXGraph graph, Schedule schedule)
			=> new Scheduler(graph).GetNextRunDate(schedule);

		public virtual void GenerateProc(Schedule schedule)
		{
            GenerateProc(schedule, 1, Accessinfo.BusinessDate.Value);
        }

		public virtual void GenerateProc(Schedule schedule, short times, DateTime runDate)
		{
			string lastBatchNbr = "0000000000";
			long lastInfoID = -1;

			IEnumerable<ScheduleDet> occurrences = new Scheduler(this).MakeSchedule(schedule, times, runDate);
			
			using (PXTransactionScope ts = new PXTransactionScope())
			{
				foreach (ScheduleDet occurrence in occurrences)
				{
					foreach (BatchNew scheduledBatch in PXSelect<
						BatchNew, 
						Where<
							BatchNew.scheduleID, Equal<Optional<Schedule.scheduleID>>, 
							And<BatchNew.scheduled,Equal<boolTrue>>>>
						.Select(this, schedule.ScheduleID))
					{
						BatchNew copy = PXCache<BatchNew>.CreateCopy(scheduledBatch);

						copy.OrigBatchNbr = copy.BatchNbr;
						copy.OrigModule = copy.Module;
						copy.CuryInfoID = null;
						copy.NumberCode = "GLREC";
						copy.NoteID = null;

						CurrencyInfo info = (CurrencyInfo)PXSelect<
							CurrencyInfo, 
							Where<CurrencyInfo.curyInfoID, Equal<Required<CurrencyInfo.curyInfoID>>>>
							.Select(this, scheduledBatch.CuryInfoID);

						if (info != null)
						{
							CurrencyInfo infocopy = PXCache<CurrencyInfo>.CreateCopy(info);

							infocopy.CuryInfoID = lastInfoID;
							copy.CuryInfoID = lastInfoID;

							CuryInfo_Created.Cache.Insert(infocopy);
						}

						copy.Posted = false;
						copy.Released = false;
						copy.Status = BatchStatus.Balanced;
						copy.Scheduled = false;
						copy.AutoReverseCopy = false;

						copy.DateEntered = occurrence.ScheduledDate;

						FinPeriod finPeriod =
							FinPeriodRepository.GetFinPeriodByMasterPeriodID(PXAccess.GetParentOrganizationID(copy.BranchID), occurrence.ScheduledPeriod)
							.GetValueOrRaiseError();
						copy.FinPeriodID = finPeriod.FinPeriodID;
						copy.TranPeriodID = null;

						copy.BatchNbr = lastBatchNbr;
						copy.RefBatchNbr = lastBatchNbr;

						lastBatchNbr = AutoNumberAttribute.NextNumber(lastBatchNbr);
						lastInfoID--;

						copy = (BatchNew)Batch_Created.Cache.Insert(copy);

						CurrencyInfoAttribute.SetEffectiveDate<Batch.dateEntered>(Batch_Created.Cache, new PXFieldUpdatedEventArgs(copy, null, false));
						PXNoteAttribute.CopyNoteAndFiles(Caches[typeof(BatchNew)], scheduledBatch, Batch_Created.Cache, copy);

						foreach (GLTranNew scheduledBatchTransaction in PXSelect<
							GLTranNew, 
							Where<
								GLTranNew.module, Equal<Required<GLTranNew.module>>, 
								And<GLTranNew.batchNbr, Equal<Required<GLTranNew.batchNbr>>>>>
							.Select(this, scheduledBatch.Module, scheduledBatch.BatchNbr))
						{
							GLTranNew transactionCopy = PXCache<GLTranNew>.CreateCopy(scheduledBatchTransaction);

							transactionCopy.OrigBatchNbr = transactionCopy.BatchNbr;
							transactionCopy.OrigModule = transactionCopy.Module;
							transactionCopy.BatchNbr = copy.BatchNbr;
							transactionCopy.RefBatchNbr = copy.RefBatchNbr;
							transactionCopy.CuryInfoID = copy.CuryInfoID;
							transactionCopy.CATranID = null;
							transactionCopy.NoteID = null;

							transactionCopy.TranDate = occurrence.ScheduledDate;
						    FinPeriodIDAttribute.SetPeriodsByMaster<GLTranNew.finPeriodID>(Tran_Created.Cache, transactionCopy, occurrence.ScheduledPeriod);

							transactionCopy = Tran_Created.Cache.Insert(transactionCopy) as GLTranNew;
							PXNoteAttribute.CopyNoteAndFiles(Tran_Created.Cache, scheduledBatchTransaction, Tran_Created.Cache, transactionCopy);
						}
					}

					schedule.LastRunDate = occurrence.ScheduledDate;
					Running_Schedule.Cache.Update(schedule);
				}

				Running_Schedule.Cache.Persist(PXDBOperation.Update);

				Batch_Created.Cache.Persist(PXDBOperation.Insert);
				Batch_Created.Cache.Persist(PXDBOperation.Update);

				foreach (GLTranNew createdTransaction in Tran_Created.Cache.Inserted)
				{
					foreach (BatchNew createdBatch in Batch_Created.Cache.Cached)
					{ 
						if (object.Equals(createdBatch.RefBatchNbr, createdTransaction.RefBatchNbr))
						{
							createdTransaction.BatchNbr = createdBatch.BatchNbr;
							createdTransaction.CuryInfoID = createdBatch.CuryInfoID;

							if (!string.IsNullOrEmpty(createdBatch.RefNbr))
							{
								createdTransaction.RefNbr = createdBatch.RefNbr;
							}

							break;
						}
					} 
				}

				Tran_Created.Cache.Normalize();

				Tran_Created.Cache.Persist(PXDBOperation.Insert);
				Tran_Created.Cache.Persist(PXDBOperation.Update);
				Caches[typeof(CA.CADailySummary)].Persist(PXDBOperation.Insert);

				ts.Complete(this);
			}

			Running_Schedule.Cache.Persisted(false);
			Batch_Created.Cache.Persisted(false);
			Tran_Created.Cache.Persisted(false);
			Caches[typeof(CA.CADailySummary)].Persisted(false);
		}
	}
}

namespace PX.Objects.GL.Overrides.ScheduleProcess
{

	[System.SerializableAttribute()]
	[PXCacheName(Messages.BatchNew)]
	public partial class BatchNew : Batch
	{
		#region Module
		public new abstract class module : PX.Data.BQL.BqlString.Field<module> { }
		[PXDBString(2, IsKey = true, IsFixed = true)]
		[PXUIField(DisplayName = "Module")]
		public override String Module
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
		public new abstract class batchNbr : PX.Data.BQL.BqlString.Field<batchNbr> { }
		[PXDBString(15, IsUnicode = true, IsKey = true, InputMask = ">CCCCCCCCCCCCCCC")]
		[PXDefault()]
		[AutoNumber(typeof(GLSetup.batchNumberingID), typeof(BatchNew.dateEntered))]
		[PXUIField(DisplayName = "Batch Number")]
		public override String BatchNbr
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
		#region RefBatchNbr
		public abstract class refBatchNbr : PX.Data.BQL.BqlString.Field<refBatchNbr> { }
		protected string _RefBatchNbr;
		[PXString(15, IsUnicode = true)]
		public virtual string RefBatchNbr
		{
			get
			{
				return this._RefBatchNbr;
			}
			set
			{
				this._RefBatchNbr = value;
			}
		}
		#endregion
		#region DateEntered
		public new abstract class dateEntered : PX.Data.BQL.BqlDateTime.Field<dateEntered> { }
		[PXDBDate()]
		public override DateTime? DateEntered
		{
			get
			{
				return this._DateEntered;
			}
			set
			{
				this._DateEntered = value;
			}
		}
		#endregion
		#region OrigBatchNbr
		public new abstract class origBatchNbr : PX.Data.BQL.BqlString.Field<origBatchNbr> { }
		#endregion
		#region OrigModule
		public new abstract class origModule : PX.Data.BQL.BqlString.Field<origModule> { }
		#endregion
		#region TranPeriodID
		public new abstract class tranPeriodID : PX.Data.IBqlField { }

		[PeriodID]
		public override String TranPeriodID { get; set; }
		#endregion
	}

	[Serializable]
	public partial class GLTranNew : GLTran
	{
		#region Module
		public new abstract class module : PX.Data.BQL.BqlString.Field<module> { }
		[PXDBString(2, IsKey = true, IsFixed = true)]
		public override string Module
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
		public new abstract class batchNbr : PX.Data.BQL.BqlString.Field<batchNbr> { }
		[PXDBString(15, IsUnicode = true, IsKey = true)]
		[PXParent(typeof(
			Select<BatchNew, 
			Where<BatchNew.module, Equal<Current<GLTranNew.module>>, 
				And<BatchNew.batchNbr, Equal<Current<GLTranNew.batchNbr>>>>>))]
		public override string BatchNbr
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
		#region RefBatchNbr
		public abstract class refBatchNbr : PX.Data.BQL.BqlString.Field<refBatchNbr> { }
		protected string _RefBatchNbr;
		[PXString(15, IsUnicode = true)]
		public virtual string RefBatchNbr
		{
			get
			{
				return this._RefBatchNbr;
			}
			set
			{
				this._RefBatchNbr = value;
			}
		}
		#endregion
		#region LedgerID
		[PXDBInt]
		public override int? LedgerID
		{
			get
			{
				return this._LedgerID;
			}
			set
			{
				this._LedgerID = value;
			}
		}
		#endregion
		#region AccountID
		[PXDBInt]
		public override int? AccountID
		{
			get
			{
				return this._AccountID;
			}
			set
			{
				this._AccountID = value;
			}
		}
		#endregion
		#region SubID
		[PXDBInt]
		public override int? SubID
		{
			get
			{
				return this._SubID;
			}
			set
			{
				this._SubID = value;
			}
		}
		#endregion
		#region CuryInfoID
		public new abstract class curyInfoID : PX.Data.BQL.BqlLong.Field<curyInfoID> { }
		[PXDBLong]
		public override long? CuryInfoID
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
		#region ReferenceNbr
		[PXDBString(15, IsUnicode = true)]
		public override string RefNbr
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
		#region TranDesc
		[PXDBString(Common.Constants.TranDescLength, IsUnicode = true)]
		public override string TranDesc
		{
			get
			{
				return this._TranDesc;
			}
			set
			{
				this._TranDesc = value;
			}
		}
		#endregion
		#region TranDate
		public new abstract class tranDate : PX.Data.BQL.BqlDateTime.Field<tranDate> { }
		[PXDBDate]
		public override DateTime? TranDate
		{
			get
			{
				return this._TranDate;
			}
			set
			{
				this._TranDate = value;
			}
		}
		#endregion
		public new abstract class taxID : PX.Data.BQL.BqlString.Field<taxID> { }
		#region FinPeriodID
		public new abstract class finPeriodID : PX.Data.IBqlField { }

		[PXDefault]
		[FinPeriodID(
			branchSourceType: typeof(GLTranNew.branchID),
			masterFinPeriodIDType: typeof(GLTranNew.tranPeriodID),
			headerMasterFinPeriodIDType: typeof(BatchNew.tranPeriodID))]
		[PXUIField(DisplayName = "Period ID", Enabled = false, Visible = false)]
		public override String FinPeriodID { get; set; }
		#endregion
		#region TaxID

		[PXDBString(TX.Tax.taxID.Length, IsUnicode = true)]
		public override string TaxID
		{
			get
			{
				return base.TaxID;
			}

			set
			{
				base.TaxID = value;
			}
		}
		#endregion
	}
}

