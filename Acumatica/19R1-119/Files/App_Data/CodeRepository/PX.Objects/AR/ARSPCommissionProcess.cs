using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using PX.Data;
using PX.Objects.CM;
using PX.Objects.CR;
using PX.Objects.EP;
using PX.Objects.GL;
using PX.Objects.GL.FinPeriods;

namespace PX.Objects.AR
{
	[TableAndChartDashboardType]
	[Serializable]
	public class ARSPCommissionProcess:  PXGraph<ARSPCommissionProcess>
	{
		#region Internal Types
		//[PXSubstitute(GraphType = typeof(ARSPCommissionProcess))]
		[Serializable]
		public partial class ARSalesPerTranExt: ARSalesPerTran 
		{
			#region Selected
			public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }
			protected bool? _Selected = false;
			[PXBool]
			[PXDefault(false)]
			[PXUIField(DisplayName = "Selected",Visible = false)]
			public bool? Selected
			{
				get
				{
					return _Selected;
				}
				set
				{
					_Selected = value;
				}
			}
			#endregion
			#region SalespersonID
			public new abstract class salespersonID : PX.Data.BQL.BqlInt.Field<salespersonID> { }
			[SalesPerson(Enabled =false,IsKey =true,DescriptionField =typeof(AR.SalesPerson.descr))]
			public override Int32? SalespersonID
			{
				get
				{
					return this._SalespersonID;
				}
				set
				{
					this._SalespersonID = value;
				}
			}
			#endregion
			#region AveCommnPct
			public abstract class aveCommnPct : PX.Data.BQL.BqlDecimal.Field<aveCommnPct> { }
			[PXDecimal(6)]
			[PXUIField(DisplayName = "Average Commission %",Visibility=PXUIVisibility.Visible)]
			public virtual Decimal? AveCommnPct
			{
				[PXDependsOnFields(typeof(commnblAmt),typeof(curyCommnAmt),typeof(curyCommnblAmt))]
				get
				{
					return ((this._CommnblAmt ?? Decimal.Zero) != Decimal.Zero) ? Math.Round((decimal)((this._CuryCommnAmt.Value/this._CuryCommnblAmt.Value)*100.0m),3): (decimal?)null;
				}
				
			}
			#endregion
			#region CommnPct
			public new abstract class commnPct : PX.Data.BQL.BqlDecimal.Field<commnPct> { }
			[PXDBDecimal(6)]
			[PXUIField(DisplayName = "Maximum Commission %", Visibility = PXUIVisibility.Visible)]
			public override Decimal? CommnPct
			{
				get
				{
					return this._CommnPct;
				}
				set
				{
					this._CommnPct = value;
				}
			}
			#endregion
			#region MinCommnPct
			public abstract class minCommnPct : PX.Data.BQL.BqlDecimal.Field<minCommnPct> { }
			protected Decimal? _MinCommnPct;
			[PXDBDecimal(6,BqlField = typeof(ARSalesPerTran.commnPct))]
			[PXUIField(DisplayName = "Minimum Commission %", Visibility = PXUIVisibility.Visible)]
			public virtual Decimal? MinCommnPct
			{
				get
				{
					return this._MinCommnPct;
				}
				set
				{
					this._MinCommnPct = value;
				}
			}
			#endregion
			#region DocCount
			public abstract class docCount : PX.Data.BQL.BqlInt.Field<docCount> { }
			protected int? _DocCount;
			[PXInt()]
			[PXUIField(DisplayName = "Documents", Visible = true)]
			public virtual int? DocCount
			{

				get
				{
					return this._DocCount;
				}
				set
				{
					this._DocCount = value;
				}
			}
			#endregion
			#region DocType
			public new abstract class docType : PX.Data.BQL.BqlString.Field<docType> { }
			[PXDBString(3,IsFixed = true)]
			public override String DocType
			{
				get
				{
					return this._DocType;
				}
				set
				{
					this._DocType = value;
				}
			}
			#endregion
			#region RefNbr
			public new abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }
			[PXDBString(15, IsUnicode = true)]
			public override String RefNbr
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
			#region AdjNbr
			public new abstract class adjNbr : PX.Data.BQL.BqlInt.Field<adjNbr> { }
			[PXDBInt()]
			public override Int32? AdjNbr
			{
				get
				{
					return this._AdjNbr;
				}
				set
				{
					this._AdjNbr = value;
				}
			}
			#endregion
			#region AdjdDocType
			public new abstract class adjdDocType : PX.Data.BQL.BqlString.Field<adjdDocType> { }
		
			[PXDBString(3, IsFixed = true)]
			public override String AdjdDocType
			{
				get
				{
					return this._AdjdDocType;
				}
				set
				{
					this._AdjdDocType = value;
				}
			}
			#endregion
			#region AdjdRefNbr
			public new abstract class adjdRefNbr : PX.Data.BQL.BqlString.Field<adjdRefNbr> { }
			[PXDBString(15, IsUnicode = true)]
			public override String AdjdRefNbr
			{
				get
				{
					return this._AdjdRefNbr;
				}
				set
				{
					this._AdjdRefNbr = value;
				}
			}
			#endregion
			#region CommnAmt
			public new abstract class commnAmt : PX.Data.BQL.BqlDecimal.Field<commnAmt> { }
			[PXDBBaseCury]
			[PXDefault(TypeCode.Decimal, "0.0")]
			[PXUIField(DisplayName = "Commission Amount", Enabled = false)]
			public override Decimal? CommnAmt
			{
				get
				{
					return this._CommnAmt;
				}
				set
				{
					this._CommnAmt = value;
				}
			}
			#endregion
			#region CuryCommnAmt

			public new abstract class curyCommnAmt : PX.Data.BQL.BqlDecimal.Field<curyCommnAmt> { }
			#endregion
			#region CommnblAmt
			public new abstract class commnblAmt : PX.Data.BQL.BqlDecimal.Field<commnblAmt> { }

			[PXDBBaseCury]
			[PXDefault(TypeCode.Decimal, "0.0")]
			[PXUIField(DisplayName = "Commissionable Amount", Enabled = false)]
			public override Decimal? CommnblAmt
			{
				get
				{
					return this._CommnblAmt;
				}
				set
				{
					this._CommnblAmt = value;
				}
			}
		

			#endregion
			#region CuryCommnblAmt
			public new abstract class curyCommnblAmt : PX.Data.BQL.BqlDecimal.Field<curyCommnblAmt> { }
			#endregion
			#region ActuallyUsed
			public new abstract class actuallyUsed : PX.Data.BQL.BqlBool.Field<actuallyUsed> { }
			#endregion
			#region CommnPaymntPeriod
			public new abstract class commnPaymntPeriod : PX.Data.BQL.BqlString.Field<commnPaymntPeriod> { }
			#endregion

		}

		public delegate void DoProcess(List<ARSalesPerTranExt> aToProcess, ARSetup settings, ARSPCommissionPeriod aCommnPeriod, List<ARSPCommissionPeriod> aPeriods, List<ARSPCommissionYear> aYears);
		#endregion

		#region Ctor + Members
		public ARSPCommissionProcess() 
		{
			ARSetup setup = ARSetup.Current;
			PXCache cache = this.ToProcess.Cache;
			PXUIFieldAttribute.SetEnabled(cache, null, false);
			PXUIFieldAttribute.SetEnabled<ARSalesPerTranExt.selected>(cache, null, true);
			cache.AllowInsert = false;
			cache.AllowDelete = false;			
			this.doProcess = ProcessSPCommissions;
			PXUIFieldAttribute.SetEnabled(this.Filter.Cache, null, false);			
		}
		#region Buttons
		public PXCancel<ARSPCommissionPeriod> Cancel;
		
		public PXAction<ARSPCommissionPeriod> ProcessAll;

		public PXSelect<ARSPCommissionPeriod> Filter;

		public PXAction<ARSPCommissionPeriod> reviewSPPeriod;
		[PXUIField(DisplayName =Messages.ReviewSPComissionPeriod, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = true)]
		[PXButton]
		public virtual IEnumerable ReviewSPPeriod(PXAdapter adapter)
		{
			if (this.Filter.Current != null)
			{
				ARSPCommissionPeriod row =this.Filter.Current;
				ARSPCommissionReview graph = PXGraph.CreateInstance<ARSPCommissionReview>();

				ARSPCommissionPeriod result = null;
				if (row.Status == ARSPCommissionPeriodStatus.Open)
				{
					result = PXSelect<ARSPCommissionPeriod,
									Where<ARSPCommissionPeriod.commnPeriodID,
									LessEqual<Required<ARSPCommissionPeriod.commnPeriodID>>,
									And<ARSPCommissionPeriod.status,NotEqual<ARSPCommissionPeriodStatus.open>>>,OrderBy<Desc<ARSPCommissionPeriod.commnPeriodID>>>.Select(graph, row.CommnPeriodID);
				}
				else
				{
					result = PXSelect<ARSPCommissionPeriod,
													Where<ARSPCommissionPeriod.commnPeriodID,
													Equal<Required<ARSPCommissionPeriod.commnPeriodID>>>>.Select(graph, row.CommnPeriodID);
				}
				if (result != null)
				{
					graph.Filter.Current = result;
					throw new PXRedirectRequiredException(graph, "Review");
				}
			}
			return adapter.Get();
		}

		#endregion

		[PXFilterable]
		public PXSelect<ARSalesPerTranExt> ToProcess;
		
		public PXSelect<ARSPCommissionPeriod,Where<ARSPCommissionPeriod.status, Equal<ARSPCommissionPeriodStatus.open>,
										And<ARSPCommissionPeriod.startDate, LessEqual<Required<ARSPCommissionPeriod.startDate>>,
										And<ARSPCommissionPeriod.endDate, Greater<Required<ARSPCommissionPeriod.endDate>>>>>,
										OrderBy<Asc<ARSPCommissionPeriod.endDate>>> ARSPCommnPeriod_Current;
		public PXSelect<ARSPCommissionPeriod, Where<ARSPCommissionPeriod.status, Equal<ARSPCommissionPeriodStatus.open>>,
										OrderBy<Asc<ARSPCommissionPeriod.endDate>>> ARSPCommnPeriod_Open;
		public PXSelect<ARSPCommissionYear, Where<ARSPCommissionYear.year, Equal<Required<ARSPCommissionYear.year>>>> ARSPCommnYear_Current;

		public PXSelectOrderBy<ARSPCommissionPeriod, OrderBy<Desc<ARSPCommissionPeriod.endDate>>> CommnPeriod_Last;

		public PXSetup<ARSetup> ARSetup;

		public DoProcess doProcess;
		#endregion

		#region Internal Variables
		private string[] postmessages;
		private List<ARSalesPerTranExt> _inproc;
		protected List<ARSalesPerTranExt> Inproc 
		{
			get
			{
				if (this._inproc == null)
				{
					
					if (PXLongOperation.Exists(UID))
					{
						this._inproc = getSelected();
						this.postmessages = PXLongOperation.GetCustomInfo(UID) as string[];
					}
					else
					{
						this._inproc = new List<ARSalesPerTranExt>();
					}
				}
				return this._inproc;
			}
		}
		#endregion

		#region Delegates
		protected virtual IEnumerable toprocess() 
		{
			PXCache cache = this.ToProcess.Cache;
			
			if (this.Inproc.Count > 0)
			{
				PXUIFieldAttribute.SetEnabled<ARSalesPerTranExt.selected>(cache, null, false);
				for (int i = 0; i < this.Inproc.Count; i++)
				{
					if (postmessages != null && i < postmessages.Length && !String.IsNullOrEmpty(postmessages[i]))
					{
						cache.RaiseExceptionHandling<ARSalesPerTranExt.salespersonID>(this.Inproc[i], this.Inproc[i].SalespersonID, new PXSetPropertyException(postmessages[i], PXErrorLevel.RowError));
					}
				}
				return this.Inproc;
			}
			else
			{
				using (new PXReadBranchRestrictedScope())
				{
					List<ARSalesPerTranExt> ret = new List<ARSalesPerTranExt>();
					Dictionary<int, ARSalesPerTranExt> excludeList = new Dictionary<int, ARSalesPerTranExt>();
					ARSPCommissionPeriod period = this.Filter.Current;
					if (period == null) return ret;
					PXSelectBase<ARSalesPerTranExt> sel = null;
					bool byPayment = (this.ARSetup.Current.SPCommnCalcType == SPCommnCalcTypes.ByPayment);
					if (byPayment)
					{
						sel = new PXSelectJoinGroupBy<ARSalesPerTranExt,
												InnerJoin<SalesPerson, On<ARSalesPerTranExt.salespersonID, Equal<SalesPerson.salesPersonID>>,
												InnerJoin<ARRegister, On<ARRegister.docType, Equal<ARSalesPerTranExt.docType>,
													And<ARRegister.refNbr, Equal<ARSalesPerTranExt.refNbr>,
													And<ARSalesPerTran.released, Equal<BQLConstants.BitOn>
													>>>>>,
												Where<ARSalesPerTranExt.commnPaymntPeriod, IsNull,
												And<ARRegister.docDate, Less<Required<ARRegister.docDate>>,
												And<ARSalesPerTranExt.adjdDocType, NotEqual<ARDocType.undefined>,
												And<ARSalesPerTranExt.adjdRefNbr, NotEqual<BQLConstants.EmptyString>>>>>,
												Aggregate<
												Sum<ARSalesPerTranExt.curyCommnblAmt,
												Sum<ARSalesPerTranExt.curyCommnAmt,
												Sum<ARSalesPerTranExt.commnblAmt,
												Sum<ARSalesPerTranExt.commnAmt,
												Max<ARSalesPerTranExt.commnPct,
												Min<ARSalesPerTranExt.minCommnPct,
												GroupBy<ARSalesPerTranExt.salespersonID,
												GroupBy<SalesPerson.isActive,
												Count>>>>>>>>>>(this);

						//Calculate total for the documents, that already have been commissioned in previous period - for the case, when commnCalcType was changed
						PXSelectBase<ARSalesPerTran> exclSelect = new PXSelectJoinGroupBy<ARSalesPerTran,
												InnerJoin<ARSalesPerTranExt, On<ARSalesPerTran.salespersonID, Equal<ARSalesPerTranExt.salespersonID>,
																		And<ARSalesPerTran.adjdDocType, Equal<ARSalesPerTranExt.docType>,
																		And<ARSalesPerTran.adjdRefNbr, Equal<ARSalesPerTranExt.refNbr>,
																		And<ARSalesPerTranExt.commnPaymntPeriod, IsNotNull,
																		And<ARSalesPerTranExt.actuallyUsed, Equal<BQLConstants.BitOn>>
																		>>>>,
												InnerJoin<ARRegister, On<ARRegister.docType, Equal<ARSalesPerTran.docType>,
													And<ARRegister.refNbr, Equal<ARSalesPerTran.refNbr>,
													And<ARSalesPerTran.released, Equal<BQLConstants.BitOn>
													>>>>>,
												Where<ARSalesPerTran.commnPaymntPeriod, IsNull,
												And<ARRegister.docDate, Less<Required<ARRegister.docDate>>>>,
												Aggregate<
												Sum<ARSalesPerTran.curyCommnblAmt,
												Sum<ARSalesPerTran.curyCommnAmt,
												Sum<ARSalesPerTran.commnblAmt,
												Sum<ARSalesPerTran.commnAmt,
												Max<ARSalesPerTran.commnPct,
												GroupBy<ARSalesPerTranExt.salespersonID,
												Count>>>>>>>>(this);
						foreach (PXResult<ARSalesPerTran, ARSalesPerTranExt, ARRegister> it in exclSelect.Select(period.EndDate))
						{
							ARSalesPerTran spTl = (ARSalesPerTran)it;
							ARSalesPerTranExt spTotal = new ARSalesPerTranExt();
							PXCache<ARSalesPerTran>.RestoreCopy(spTotal, spTl);
							spTotal.DocCount = it.RowCount;
							excludeList[spTotal.SalespersonID.Value] = spTotal;
						}
					}
					else
					{
						sel = new PXSelectJoinGroupBy<ARSalesPerTranExt,
												InnerJoin<SalesPerson, On<ARSalesPerTranExt.salespersonID, Equal<SalesPerson.salesPersonID>>,
												InnerJoin<ARRegister, On<ARRegister.docType, Equal<ARSalesPerTranExt.docType>,
													And<ARRegister.refNbr, Equal<ARSalesPerTranExt.refNbr>,
													And<ARSalesPerTran.released, Equal<BQLConstants.BitOn>
													>>>>>,
												Where<ARSalesPerTranExt.commnPaymntPeriod, IsNull,
												And<ARRegister.docDate, Less<Required<ARRegister.docDate>>,
												And<Where<ARSalesPerTranExt.adjdDocType, Equal<ARDocType.undefined>,
													And<ARSalesPerTranExt.adjdRefNbr, Equal<BQLConstants.EmptyString>
													>>>>>,
												Aggregate<
												Sum<ARSalesPerTranExt.curyCommnblAmt,
												Sum<ARSalesPerTranExt.curyCommnAmt,
												Sum<ARSalesPerTranExt.commnblAmt,
												Sum<ARSalesPerTranExt.commnAmt,
												Max<ARSalesPerTranExt.commnPct,
												Min<ARSalesPerTranExt.minCommnPct,
												GroupBy<ARSalesPerTranExt.salespersonID,
												GroupBy<SalesPerson.isActive,
												Count>>>>>>>>>>(this);
					}
					foreach (PXResult<ARSalesPerTranExt, SalesPerson> iSPT in sel.Select(period.EndDate))
					{
						ARSalesPerTranExt spTotal = (ARSalesPerTranExt)cache.CreateCopy((ARSalesPerTranExt)iSPT);
						spTotal.DocCount = iSPT.RowCount;
						if (excludeList.ContainsKey((int)spTotal.SalespersonID))
						{
							ARSalesPerTranExt excludeTotal = excludeList[(int)spTotal.SalespersonID];
							Substract(spTotal, excludeTotal);
						}
						ret.Add(spTotal);
					}
					return ret;
				}
			}
		}

		protected virtual IEnumerable filter()
		{
			PXCache cache = this.Filter.Cache;
			ARSPCommissionPeriod processed = PXLongOperation.GetCustomInfo(this.UID) as ARSPCommissionPeriod;
			if(processed != null)
			{
				yield return processed;
				yield break;
			}
			foreach (ARSPCommissionPeriod iPer in this.ARSPCommnPeriod_Open.Select())
			{
				yield return iPer;
				yield break;
			}

			ARSPCommissionPeriod lastper = (ARSPCommissionPeriod)CommnPeriod_Last.Select();
			if (lastper != null)
			{
				lastper = SPCommissionCalendar.Create(this, ARSPCommnYear_Current, ARSPCommnPeriod_Current, ARSetup.Current, lastper.EndDate);
				Caches[typeof(ARSPCommissionPeriod)].IsDirty = false;
				Caches[typeof(ARSPCommissionYear)].IsDirty = false;
				if (lastper != null)
				{
					yield return lastper;
				}
				yield break;
			}
			PXResult<ARSalesPerTran, ARRegister> first_tran = null;
			using (new PXReadBranchRestrictedScope())
			{
				PXSelectBase<ARSalesPerTran> sel = new PXSelectJoinOrderBy<ARSalesPerTran,
								InnerJoin<ARRegister, On<ARSalesPerTran.docType, Equal<ARRegister.docType>,
								And<ARSalesPerTran.refNbr, Equal<ARRegister.refNbr>>>>, OrderBy<Asc<ARRegister.docDate>>>(this);
				 first_tran = (PXResult<ARSalesPerTran, ARRegister>)sel.View.SelectSingle();
			}
			if (first_tran != null)
			{
				ARSPCommissionPeriod current = SPCommissionCalendar.Create(this, ARSPCommnYear_Current, ARSPCommnPeriod_Current, ARSetup.Current, ((ARRegister)first_tran).DocDate);
				Caches[typeof(ARSPCommissionPeriod)].IsDirty = false;
				Caches[typeof(ARSPCommissionYear)].IsDirty = false;
				if (current != null)
				{
					yield return current;
				}
			}            
			yield break;

		}
#if false
		protected virtual IEnumerable filter()
		{
			PXCache cache = this.Filter.Cache;
			List<ARSPCommissionPeriod> result = new List<ARSPCommissionPeriod>(1);

			ARSPCommissionPeriod open =  this.ARSPCommnPeriod_Open.Select();
			if (open == null)
			{
				ARSPCommissionPeriod lastper = (ARSPCommissionPeriod)CommnPeriod_Last.Select();
				if (lastper != null)
				{
					lastper = SPCommissionCalendar.Create<ARSPCommissionPeriod>(this, this.ARSPCommnYear_Current, this.ARSPCommnPeriod_Current, this.ARSetup.Current, ((ARSPCommissionPeriod)lastper).EndDate);					
				}
				else
				{
					PXSelectBase<ARSalesPerTran> sel = new PXSelectOrderBy<ARSalesPerTran,
								InnerJoin<ARRegister, On<ARSalesPerTran.docType, Equal<ARRegister.docType>,
								And<ARSalesPerTran.refNbr, Equal<ARRegister.refNbr>>>>, OrderBy<Asc<ARRegister.docDate>>>(this);
					PXResult<ARSalesPerTran, ARRegister> first_tran = (PXResult<ARSalesPerTran, ARRegister>)sel.View.SelectSingle();

					if (first_tran != null)
					{
						ARSPCommissionPeriod current = null;
						current = SPCommissionCalendar.Create<ARSPCommissionPeriod>(this, this.ARSPCommnYear_Current, this.ARSPCommnPeriod_Current, this.ARSetup.Current, ((ARRegister)first_tran).DocDate);						
					}
				}
			}
			PXUIFieldAttribute.SetEnabled<ARSPCommissionPeriod.commnPeriodID>(this.Filter.Cache, null, true);
			foreach (ARSPCommissionPeriod iPer in PXSelectOrderBy<ARSPCommissionPeriod, OrderBy<Desc<ARSPCommissionPeriod.commnPeriodID>>>.Select(this))
			{
				yield return iPer;
			}
			
		}
#endif

		#region Button Delegates
		[PXUIField(DisplayName = Messages.ProcessAll, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXProcessButton]
		public virtual IEnumerable processAll(PXAdapter adapter)
		{
			PXCache cache = Caches[typeof(ARSalesPerTranExt)];
			if (PXLongOperation.Exists(UID))
			{
				throw new ApplicationException(GL.Messages.PrevOperationNotCompleteYet);
			}
			ARSPCommissionPeriod processPeriod = (ARSPCommissionPeriod)this.Filter.Current;
			ARSPCommissionPeriod nonClosed = PXSelect<ARSPCommissionPeriod,
														Where<ARSPCommissionPeriod.commnPeriodID,Less<Required<ARSPCommissionPeriod.commnPeriodID>>,
															And<ARSPCommissionPeriod.status,NotEqual<ARSPCommissionPeriodStatus.closed>>>>.Select(this,processPeriod.CommnPeriodID);
			if (nonClosed != null)
			{
				throw new ApplicationException(Messages.SPCommissionPeriodMayNotBeProcessedThereArePeriodsOpenBeforeIt);
			}
			if (this.Accessinfo.BusinessDate < processPeriod.StartDate) 
			{
				throw new ApplicationException(Messages.SPFuturePeriodIsInvalidToProcess);
			}
			if (this.Accessinfo.BusinessDate >= processPeriod.StartDate && this.Accessinfo.BusinessDate < processPeriod.EndDate) 
			{
				WebDialogResult result = this.Filter.Ask(Messages.Attention, Messages.SPOpenPeriodProcessingConfirmation, MessageButtons.OKCancel);
				if (result != WebDialogResult.OK) 
				{
					return adapter.Get();
				}
			}
			List<ARSalesPerTranExt> list = new List<ARSalesPerTranExt>();
			foreach (ARSalesPerTranExt it in this.getAll())
			{
				it.Selected = true;
				cache.Update(it);
				list.Add(it);
			}
			//if (list.Count > 0)
			{
			
				PXLongOperation.StartOperation(this, delegate() { doProcess(list, this.ARSetup.Current, processPeriod, this.getPeriodsToInsert(), this.getYearsToInsert()); });
				PXUIFieldAttribute.SetEnabled<ARSalesPerTranExt.selected>(cache, null, false);
			}
			//return list;
			return adapter.Get();
		}

		#endregion
		#endregion

		#region Processing
		static void ProcessSPCommissions(List<ARSalesPerTranExt> aProcessList, ARSetup settings, ARSPCommissionPeriod aProcessPeriod, List<ARSPCommissionPeriod> aPeriods, List<ARSPCommissionYear> aYears) 
		{
			
			string[] perrowmessage = new string[aProcessList.Count>0?aProcessList.Count:1];
			bool isex = false;
			PXLongOperation.SetCustomInfo(perrowmessage);
			ARSPCommissionUpdate updateBO = PXGraph.CreateInstance<ARSPCommissionUpdate>();
			PXDBDefaultAttribute.SetDefaultForUpdate<ARSalesPerTran.curyInfoID>(updateBO.SPTrans.Cache, null, false);
			bool ByPayment = (settings.SPCommnCalcType == SPCommnCalcTypes.ByPayment);
			using (PXTransactionScope ts0 = new PXTransactionScope())
			{
				updateBO.InsertCommissionPeriods(aYears, aPeriods, aProcessPeriod);
				for (int i = 0; i < aProcessList.Count; i++)
				{
					try
					{
						updateBO.ProcessSPCommission(aProcessList[i].SalespersonID, aProcessPeriod, ByPayment);
						//updateBO.Clear();
						//ProcessSPCommission(updateBO, aProcessList[i], aCommnPeriod, ByPayment);
					}
					catch (Exception ex)
					{
						isex = true;
						perrowmessage[i] = ex.Message;
					}
				}
				if (isex)
				{
					throw new ApplicationException(Messages.SPCommissionCalcFailure);
				}
				else
				{
					updateBO.MarkPeriodAsPrepared(aProcessPeriod, true);
					updateBO.Actions.PressSave();
				}
				perrowmessage[0] = aProcessPeriod.CommnPeriodID;
				ts0.Complete();
			}
			PXLongOperation.SetCustomInfo(aProcessPeriod);
		}
		#endregion

		#region Internal utility functions
		private List<ARSalesPerTranExt> getSelected()
		{
			List<ARSalesPerTranExt> ret = new List<ARSalesPerTranExt>();
			foreach (ARSalesPerTranExt it in this.ToProcess.Cache.Updated)
			{
				if (it.Selected == true)
				{
					ret.Add(it);
				}
			}
			return ret;
		}
		private List<ARSalesPerTranExt> getAll()
		{
			List<ARSalesPerTranExt> ret = new List<ARSalesPerTranExt>();
			foreach (ARSalesPerTranExt it in this.ToProcess.Select())
			{
				ret.Add(it);
			}
			return ret;
		}
		private List<ARSPCommissionPeriod> getPeriodsToInsert() 
		{
			List<ARSPCommissionPeriod> result = new List<ARSPCommissionPeriod>();
			foreach(ARSPCommissionPeriod it in this.ARSPCommnPeriod_Current.Cache.Inserted)
			{
				result.Add(it);
			}
			return result;
		}
		private List<ARSPCommissionYear> getYearsToInsert() 
		{
			List<ARSPCommissionYear> result = new List<ARSPCommissionYear>();
			foreach(ARSPCommissionYear it in this.ARSPCommnYear_Current.Cache.Inserted)
			{
				result.Add(it);
			}
			return result;
		}

		static void Substract(ARSalesPerTranExt aRes, ARSalesPerTranExt aOp2) 
		{
			aRes.CommnblAmt -= aOp2.CommnblAmt;
			aRes.CommnAmt-= aOp2.CommnAmt;
			aRes.CuryCommnblAmt -= aOp2.CuryCommnblAmt;
			aRes.CuryCommnAmt -= aOp2.CuryCommnAmt;
			aRes.DocCount -= aOp2.DocCount;
		}
		#endregion
		
	}

	[PXHidden()]
	public class ARSPCommissionUpdate : PXGraph<ARSPCommissionUpdate>
	{
		#region InternalType
		public class SPHistoryKey : AP.Triplet<int, int, int>
		{
			public SPHistoryKey(int aA1, int aA2, int aA3 ) : base(aA1, aA2, aA3) { }
			public virtual int GetHashCode(SPHistoryKey obj) 
			{
				return (obj.first + 13 * (obj.second + 67 * obj.third));
			}
		}

		public class KeyComparer : IEqualityComparer<SPHistoryKey>
		{

			#region IEqualityComparer<SPHistoryKey> Members

			public virtual bool Equals(SPHistoryKey x, SPHistoryKey y)
			{
				return (x.CompareTo(y) == 0);
			}

			public virtual int GetHashCode(SPHistoryKey obj)
			{
				return (obj.first +13*(obj.second + 67*obj.third));
			}

			#endregion
		}
		#endregion

		public PXSelect<ARSPCommissionYear, Where<ARSPCommissionYear.year, Equal<Required<ARSPCommissionYear.year>>>> CommnYear; 
		public PXSelect<ARSPCommissionPeriod, Where<ARSPCommissionPeriod.commnPeriodID, Equal<Required<ARSPCommissionPeriod.commnPeriodID>>>> CommnPeriod;
		
		
		public PXSelectJoin<ARSalesPerTran, InnerJoin<ARRegister, On<ARSalesPerTran.docType, Equal<ARRegister.docType>,
										And<ARSalesPerTran.refNbr, Equal<ARRegister.refNbr>,
										And<ARSalesPerTran.released, Equal<BQLConstants.BitOn>,
										And<ARRegister.docDate, Less<Required<ARRegister.docDate>>>>>>>,
									Where<ARSalesPerTran.salespersonID, Equal<Required<ARSalesPerTran.salespersonID>>,
										And<ARSalesPerTran.commnPaymntPeriod, IsNull>>> SPTrans;
		public PXSelect<ARSPCommnHistory, Where<ARSPCommnHistory.salesPersonID, Equal<Required<ARSPCommnHistory.salesPersonID>>,
						And<ARSPCommnHistory.branchID, Equal<Required<ARSPCommnHistory.branchID>>,
						And<ARSPCommnHistory.customerID, Equal<Required<ARSPCommnHistory.customerID>>,
						And<ARSPCommnHistory.customerLocationID, Equal<Required<ARSPCommnHistory.customerLocationID>>,
						And<ARSPCommnHistory.commnPeriod,Equal<Required<ARSPCommnHistory.commnPeriod>>>>>>>> SPHistory;

		public virtual void ProcessSPCommission(int? aSalesPersonID, ARSPCommissionPeriod aPeriod, bool aByPayment) 
		{
			//KeyComparer comparer = new KeyComparer();
			Dictionary<SPHistoryKey, ARSPCommnHistory> history = new Dictionary<SPHistoryKey, ARSPCommnHistory>();
			foreach (PXResult<ARSalesPerTran, ARRegister> iSPT in this.SPTrans.Select(aPeriod.EndDate, aSalesPersonID))
			{
				ARSalesPerTran iTran = (ARSalesPerTran)iSPT;
				ARRegister iReg = (ARRegister)iSPT;                
				bool isActuallyUsed = false;
				if (aByPayment)
				{
					isActuallyUsed = (iTran.AdjdDocType != ARDocType.Undefined) && !string.IsNullOrEmpty(iTran.AdjdRefNbr);
					if (isActuallyUsed)
					{
						ARSalesPerTran origTran = PXSelect<ARSalesPerTran, Where<ARSalesPerTran.docType, Equal<Required<ARSalesPerTran.docType>>,
								And<ARSalesPerTran.refNbr, Equal<Required<ARSalesPerTran.refNbr>>,
								And<ARSalesPerTran.salespersonID, Equal<Required<ARSalesPerTran.salespersonID>>,
								And<ARSalesPerTran.adjdDocType, Equal<ARDocType.undefined>,
								And<ARSalesPerTran.commnPaymntPeriod, IsNotNull,
								And<ARSalesPerTran.actuallyUsed, Equal<BQLConstants.BitOn>>>>>>>>.Select(this, iTran.AdjdDocType, iTran.AdjdRefNbr, iTran.SalespersonID);
						if (origTran != null)
							isActuallyUsed = false;        
					}
				}
				else
				{
					isActuallyUsed = (iTran.AdjdDocType == ARDocType.Undefined) && string.IsNullOrEmpty(iTran.AdjdRefNbr);                    
				}
				iTran.ActuallyUsed = isActuallyUsed;
				iTran.CommnPaymntPeriod = aPeriod.CommnPeriodID;
				iTran.CommnPaymntDate = this.Accessinfo.BusinessDate;
				if (isActuallyUsed)
				{
					SPHistoryKey key = new SPHistoryKey(iTran.BranchID.Value, iReg.CustomerID.Value, iReg.CustomerLocationID.Value);
					if (history.ContainsKey(key))
					{
						Aggregate(history[key], iTran);
					}
					else
					{
						ARSPCommnHistory hst = new ARSPCommnHistory();
						Copy(hst, iTran);
						Copy(hst, iReg);
						history[key] = hst;
					}
				}
				iTran = this.SPTrans.Update(iTran);
			}
			foreach (ARSPCommnHistory iHst in history.Values)
			{
				ARSPCommnHistory curr = this.SPHistory.Select(iHst.SalesPersonID,iHst.BranchID, iHst.CustomerID, iHst.CustomerLocationID,iHst.CommnPeriod);
				if (curr != null)
				{
					curr.CommnAmt += iHst.CommnAmt;
					curr.CommnblAmt += iHst.CommnblAmt;
					curr = this.SPHistory.Update(curr);
				}
				else
				{
					this.SPHistory.Insert(iHst);
				}
			}
		}
		public virtual void InsertCommissionPeriods(List<ARSPCommissionYear> aYears, List<ARSPCommissionPeriod> aPeriods,ARSPCommissionPeriod aCurrent) 
		{
			foreach (ARSPCommissionYear iYr in aYears) 
			{
				ARSPCommissionYear tst = (ARSPCommissionYear) this.CommnYear.Select(iYr.Year);
				if(tst == null)
					this.CommnYear.Insert(iYr);
			}
			foreach (ARSPCommissionPeriod iPeriod in aPeriods)
			{
				ARSPCommissionPeriod tst = (ARSPCommissionPeriod)this.CommnPeriod.Select(iPeriod.CommnPeriodID);
				if (tst == null)
				{
					if (iPeriod.EndDate < aCurrent.EndDate)
						iPeriod.Status = ARSPCommissionPeriodStatus.Prepared;
					this.CommnPeriod.Insert(iPeriod);
				}
				else 
				{
					if (iPeriod.EndDate < aCurrent.EndDate)
					{
						this.MarkPeriodAsPrepared(iPeriod,false);
					}
				}
			}
		}
		public virtual void MarkPeriodAsPrepared(ARSPCommissionPeriod aCurrent, bool doSelect) 
		{
			if (doSelect)
			{
				//ARSPCommissionPeriod tmp = (ARSPCommissionPeriod)this.CommnPeriod.Select(aCurrent.CommnPeriodID);
				//if (tmp != null) 
				//{
				//	aCurrent = tmp;
				//}
			}
			aCurrent.Status = ARSPCommissionPeriodStatus.Prepared;
			aCurrent = this.CommnPeriod.Update(aCurrent);
		}
		public virtual void VoidReportProc(ARSPCommissionPeriod aCommnPeriod)
		{
			using (PXConnectionScope cs = new PXConnectionScope())
			{
				using (PXTransactionScope ts = new PXTransactionScope())
				{
					PXDatabase.Update<ARSalesPerTran>(
						new PXDataFieldAssign("CommnPaymntPeriod", null),
						new PXDataFieldAssign("CommnPaymntDate", null),
						new PXDataFieldAssign("ActuallyUsed", false),
						new PXDataFieldRestrict("CommnPaymntPeriod", PXDbType.Char, 6, aCommnPeriod.CommnPeriodID, PXComp.EQ)
						);

					PXDatabase.Delete<ARSPCommnHistory>(

						new PXDataFieldRestrict("CommnPeriod", PXDbType.Char, 6, aCommnPeriod.CommnPeriodID, PXComp.EQ)
						);

					foreach (ARSPCommissionPeriod per in this.CommnPeriod.Select(aCommnPeriod.CommnPeriodID))
					{
						if (per.Status != ARSPCommissionPeriodStatus.Prepared)
						{
							throw new PXException();
						}

						per.Status = ARSPCommissionPeriodStatus.Open;
						this.CommnPeriod.Cache.Update(per);
					}

					this.CommnPeriod.Cache.Persist(PXDBOperation.Update);
					this.CommnPeriod.Cache.Persisted(false);

					ts.Complete();
				}
			}
		}
		public virtual void ClosePeriodProc(ARSPCommissionPeriod aCurrent) 
		{
			using (PXConnectionScope cs = new PXConnectionScope())
			{
				using (PXTransactionScope ts = new PXTransactionScope())
				{
					foreach (ARSPCommissionPeriod res in this.CommnPeriod.Select(aCurrent.CommnPeriodID))
					{
						if (res.Status != ARSPCommissionPeriodStatus.Prepared)
						{
							throw new PXException();
						}
						res.Status = ARSPCommissionPeriodStatus.Closed;
						this.CommnPeriod.Cache.Update(res);
					}


					this.CommnPeriod.Cache.Persist(PXDBOperation.Update);
					this.CommnPeriod.Cache.Persisted(false);
					ts.Complete();
				}
			}
		}

		public virtual void ReopenPeriodProc(ARSPCommissionPeriod aCurrent)
		{
			using (PXConnectionScope cs = new PXConnectionScope())
			{
				using (PXTransactionScope ts = new PXTransactionScope())
				{
					foreach (ARSPCommissionPeriod res in this.CommnPeriod.Select(aCurrent.CommnPeriodID))
					{
						if (res.Status != ARSPCommissionPeriodStatus.Closed)
						{
							throw new PXException(Messages.CommissionPeriodNotClosed);
						}
						res.Status = ARSPCommissionPeriodStatus.Prepared;
						ARSPCommnHistory history = PXSelectReadonly<ARSPCommnHistory, Where<ARSPCommnHistory.commnPeriod, Equal<Required<ARSPCommnHistory.commnPeriod>>>>.Select(this, res.CommnPeriodID);
						if (history == null) 
						{
							ARSalesPerTran tran = PXSelectReadonly<ARSalesPerTran, Where<ARSalesPerTran.commnPaymntPeriod, Equal<Required<ARSalesPerTran.commnPaymntPeriod>>>>.Select(this, res.CommnPeriodID);
							if (tran == null) 
							{
								res.Status = ARSPCommissionPeriodStatus.Open; 
							}
						}
						this.CommnPeriod.Cache.Update(res);
					}


					this.CommnPeriod.Cache.Persist(PXDBOperation.Update);
					this.CommnPeriod.Cache.Persisted(false);
					ts.Complete();
				}
			}
		}

		#region Utility Functions
		public static void Copy(ARSPCommnHistory aDest, ARSalesPerTran aSource)
		{
			aDest.BranchID = aSource.BranchID;
			aDest.SalesPersonID = aSource.SalespersonID;
			aDest.CommnAmt = aSource.CommnAmt ?? Decimal.Zero;
			aDest.CommnblAmt = aSource.CommnblAmt ?? Decimal.Zero;
			aDest.CommnPeriod = aSource.CommnPaymntPeriod;
		}

		public static void Copy(ARSPCommnHistory aDest, ARRegister aSource)
		{
			aDest.CustomerID = aSource.CustomerID;
			aDest.CustomerLocationID = aSource.CustomerLocationID;
		}

		public static void Aggregate(ARSPCommnHistory aDest, ARSalesPerTran aSource)
		{
			aDest.CommnAmt += aSource.CommnAmt ?? Decimal.Zero;
			aDest.CommnblAmt += aSource.CommnblAmt ?? Decimal.Zero;
		}
		#endregion
	}

	public static class SPCommissionCalendar
	{
		public static TPeriod Create<TPeriod>(PXGraph graph, PXSelectBase<ARSPCommissionYear> Year_Select, PXSelectBase<TPeriod> Period_Select, ARSetup aSetup, DateTime? date)
			where TPeriod : ARSPCommissionPeriod, new()
		{
			short? CalendarPeriods = null;

			switch (aSetup.SPCommnPeriodType)
			{
				case SPCommnPeriodTypes.Monthly:
					CalendarPeriods = 12;
					break;
				case SPCommnPeriodTypes.Quarterly:
					CalendarPeriods = 4;
					break;
				case SPCommnPeriodTypes.Yearly:
					CalendarPeriods = 1;
					break;
			}

			if (CalendarPeriods != null)
			{
				int year = ((DateTime)date).Year;
				int month = 1;
				string yearAsString = year.ToString();
				ARSPCommissionYear new_year = (ARSPCommissionYear)Year_Select.Select(yearAsString);
				if (new_year == null)
				{
					new_year = new ARSPCommissionYear();
					new_year.Year = yearAsString;
					new_year =(ARSPCommissionYear) Year_Select.Cache.Insert(new_year);
				}

				for (int i = 1; i < CalendarPeriods + 1; i++)
				{
					TPeriod new_per = new TPeriod();
					new_per.Year = new_year.Year;
					new_per.CommnPeriodID = new_year.Year + (i < 10 ? "0" : "") + i.ToString();
					new_per.StartDate = new DateTime(year, month, 1);
					month += 12 / (int)CalendarPeriods;
					if (month > 12)
					{
						month = 1;
						year++;
					}
					new_per.EndDate = new DateTime(year, month, 1);
					if (new_per.EndDate <= date.Value)
						new_per.Status = ARSPCommissionPeriodStatus.Closed;
					new_per = (TPeriod)Period_Select.Cache.Insert(new_per);
				}
			}
			else
			{
				ARSPCommissionYear new_year = null ;
				foreach (PXResult<MasterFinYear, MasterFinPeriod> res in PXSelectJoin<
																			MasterFinYear, 
																			InnerJoin<MasterFinPeriod, 
																				On<MasterFinPeriod.finYear, Equal<MasterFinYear.year>>>, 
																			Where<MasterFinYear.startDate, LessEqual<Required<MasterFinYear.startDate>>>, 
																			OrderBy<
																				Desc<MasterFinYear.year>>>
					.Select(graph, (object)date))
				{
					if (new_year == null)
					{
						new_year = (ARSPCommissionYear)Year_Select.Select(((MasterFinYear)res).Year);
						if (new_year == null)
						{
							new_year = CreateFrom((MasterFinYear)res);
							new_year = (ARSPCommissionYear)Year_Select.Cache.Insert(new_year);
						}
					}
					else if (object.Equals(((MasterFinYear)res).Year, new_year.Year) == false)
					{
						break;
					}
					ARSPCommissionPeriod new_per = CreateFrom((MasterFinPeriod)res);
					if (new_per.EndDate <= date.Value)
					{
						new_per.Status = ARSPCommissionPeriodStatus.Closed;
					}
					Period_Select.Cache.Insert(new_per);
				}
				
			}
			

			return (TPeriod)Period_Select.Select(date, date);
		}

		public static ARSPCommissionPeriod CreateFrom(MasterFinPeriod aFiscalPeriod)
		{
			ARSPCommissionPeriod res = new ARSPCommissionPeriod();
			res.CommnPeriodID = aFiscalPeriod.FinPeriodID;
			res.StartDate = aFiscalPeriod.StartDate;
			res.EndDate = aFiscalPeriod.EndDate;
			res.Year = aFiscalPeriod.FinYear;
			return res;
		}

		public static ARSPCommissionYear CreateFrom(MasterFinYear aFiscalYear)
		{
			ARSPCommissionYear res = new ARSPCommissionYear();
			res.Year = aFiscalYear.Year;
			return res;
		}
	}

	[TableAndChartDashboardType]
	public class ARSPCommissionReview : PXGraph<ARSPCommissionProcess>
	{
		#region Internal Types

		public delegate void DoProcess(ARSPCommissionPeriod aCommnPeriod);
		#endregion

		#region Ctor + Members
		public ARSPCommissionReview()
		{
			ARSetup setup = ARSetup.Current;
			PXCache cache = this.ToProcess.Cache;
			PXUIFieldAttribute.SetEnabled(cache, null, false);
			cache.AllowInsert = false;
			cache.AllowDelete = false;
			cache.AllowUpdate = false;
			PXUIFieldAttribute.SetEnabled(this.Filter.Cache, null, false);
			PXUIFieldAttribute.SetEnabled<ARSPCommissionPeriod.commnPeriodID>(this.Filter.Cache, null, true);
			this.Filter.Cache.AllowInsert = false;
			this.Filter.Cache.AllowDelete = false;

			actionsFolder.MenuAutoOpen = true;
			actionsFolder.AddMenuAction(VoidCommissions);
			actionsFolder.AddMenuAction(ClosePeriod);
			actionsFolder.AddMenuAction(ReopenPeriod);
		}
		#region Buttons
		
		public PXCancel<ARSPCommissionPeriod> Cancel;
		[Obsolete(Common.Messages.WillBeRemovedInAcumatica2019R1)]
		public PXAction<ARSPCommissionPeriod> EditDetail;
		public PXFirst<ARSPCommissionPeriod> First;
		public PXPrevious<ARSPCommissionPeriod> Previous;
		public PXNext<ARSPCommissionPeriod> Next;
		public PXLast<ARSPCommissionPeriod> Last;
		public PXAction<ARSPCommissionPeriod> actionsFolder;
		public PXAction<ARSPCommissionPeriod> VoidCommissions;
		public PXAction<ARSPCommissionPeriod> ClosePeriod;
		public PXAction<ARSPCommissionPeriod> ReopenPeriod;

		#region Sub-screen Navigation Buttons
		[PXUIField(DisplayName = "", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = false)]
		[PXEditDetailButton]
		public virtual IEnumerable editDetail(PXAdapter adapter)
		{
			if ((this.ToProcess.Current != null) && (this.Filter.Current != null))
			{
				ARSPCommissionPeriod filter = this.Filter.Current;
				ARSPCommnHistory current = this.ToProcess.Current;
				ARSPCommissionDocEnq graph = PXGraph.CreateInstance<ARSPCommissionDocEnq>();
				SPDocFilter dstFilter = graph.Filter.Current;
				dstFilter.SalesPersonID = current.SalesPersonID;
				dstFilter.CommnPeriod = filter.CommnPeriodID;
				graph.Filter.Update(dstFilter);
				throw new PXRedirectRequiredException(graph, "Document"){Mode = PXBaseRedirectException.WindowMode.NewWindow};
			}
			return adapter.Get();
		}
		#endregion
		#endregion

		public PXSelect<ARSPCommissionPeriod> Filter;
		[PXFilterable]
		public PXSelect<ARSPCommnHistory> ToProcess;

		protected virtual IEnumerable toprocess()
		{
			PXSelectBase<ARSPCommnHistory> select = new PXSelectGroupBy<ARSPCommnHistory, 
				Where<ARSPCommnHistory.commnPeriod, Equal<Current<ARSPCommissionPeriod.commnPeriodID>>>,
				Aggregate<GroupBy<ARSPCommnHistory.salesPersonID, Sum<ARSPCommnHistory.commnblAmt, Sum<ARSPCommnHistory.commnAmt, Max<ARSPCommnHistory.pRProcessedDate>>>>>>(this);

			foreach (ARSPCommnHistory item in select.Select())
			{
				EPEmployee employee = PXSelect<EPEmployee, Where<EPEmployee.salesPersonID, Equal<Required<ARSPCommnHistory.salesPersonID>>>>.Select(this, item.SalesPersonID);
				item.Type = employee != null ? BAccountType.EmployeeType : BAccountType.VendorType;

				yield return item;
			}
		} 

		#endregion

		#region Internal Variables
		private string[] postmessages;
		private List<ARSPCommnHistory> _inproc;
		protected List<ARSPCommnHistory> Inproc
			{
				get
				{
					if (this._inproc == null)
					{

						if (PXLongOperation.Exists(UID))
						{
							this._inproc = getSelected();
							this.postmessages = PXLongOperation.GetCustomInfo(UID) as string[];
						}
						else
						{
							this._inproc = new List<ARSPCommnHistory>();
						}
					}
					return this._inproc;
				}
			}
		#endregion

		#region Delegates
		
		#region Button Delegates
		[PXUIField(DisplayName = "Actions", MapEnableRights = PXCacheRights.Select)]
		[PXButton(SpecialType = PXSpecialButtonType.ActionsFolder)]
		protected virtual IEnumerable ActionsFolder(PXAdapter adapter)
		{
			return adapter.Get();
		}

		[PXUIField(DisplayName = Messages.VoidCommissions, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXProcessButton]
		public virtual IEnumerable voidCommissions(PXAdapter adapter)
		{
			PXCache cache = Caches[typeof(ARSPCommnHistory)];
			if (PXLongOperation.Exists(UID))
			{
				throw new ApplicationException(GL.Messages.PrevOperationNotCompleteYet);
			}
			ARSPCommissionPeriod processPeriod = (ARSPCommissionPeriod)this.Filter.Current;
			if(processPeriod!=null)
			{

				PXLongOperation.StartOperation(this, delegate() { VoidCommissionsProc(processPeriod); });
			}
			return adapter.Get();
		}

		[PXUIField(DisplayName = Messages.ClosePeriod, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXProcessButton]
		public virtual IEnumerable closePeriod(PXAdapter adapter)
		{
			PXCache cache = Caches[typeof(ARSPCommnHistory)];
			if (PXLongOperation.Exists(UID))
			{
				throw new ApplicationException(GL.Messages.PrevOperationNotCompleteYet);
			}
			ARSPCommissionPeriod processPeriod = (ARSPCommissionPeriod)this.Filter.Current;
			if (processPeriod != null)
			{
				ARSPCommissionPeriod nonClosed = PXSelect<ARSPCommissionPeriod,
														   Where<ARSPCommissionPeriod.commnPeriodID, Less<Required<ARSPCommissionPeriod.commnPeriodID>>,
															   And<ARSPCommissionPeriod.status, NotEqual<ARSPCommissionPeriodStatus.closed>>>>.Select(this, processPeriod.CommnPeriodID);
				if (nonClosed != null)
				{
					throw new ApplicationException(Messages.SPCommissionPeriodMayNotBeClosedThereArePeriodsOpenBeforeIt);
				}
				{

					PXLongOperation.StartOperation(this, delegate() { CloseCommnPeriod(processPeriod); });
				}
			}
			return adapter.Get();
		}

		[PXUIField(DisplayName = Messages.ReopenPeriod, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXProcessButton]
		public virtual IEnumerable reopenPeriod(PXAdapter adapter)
		{
			PXCache cache = Caches[typeof(ARSPCommnHistory)];
			if (PXLongOperation.Exists(UID))
			{
				throw new ApplicationException(GL.Messages.PrevOperationNotCompleteYet);
			}
			ARSPCommissionPeriod processPeriod = (ARSPCommissionPeriod)this.Filter.Current;
			if (processPeriod != null)
			{
				ARSPCommissionPeriod closedOrPrepared = PXSelect<ARSPCommissionPeriod,
														   Where<ARSPCommissionPeriod.commnPeriodID, Greater<Required<ARSPCommissionPeriod.commnPeriodID>>,
															   And<ARSPCommissionPeriod.status, NotEqual<ARSPCommissionPeriodStatus.open>>>>.Select(this, processPeriod.CommnPeriodID);
				if (closedOrPrepared != null)
				{
					throw new ApplicationException(Messages.SPCommissionPeriodMayNotBeReopendThereAreClosedPeriodsAfterIt);
				}
				{

					PXLongOperation.StartOperation(this, delegate() { ReopenCommnPeriod(processPeriod); });
				}
			}
			return adapter.Get();
		}

		
		#endregion
		#endregion

		#region Processing
		static void VoidCommissionsProc(ARSPCommissionPeriod aProcessPeriod)
		{
			int size = 1;
			string[] perrowmessage =  new string[size];
			PXLongOperation.SetCustomInfo(perrowmessage);
			try
			{
				ARSPCommissionUpdate updateBO = PXGraph.CreateInstance<ARSPCommissionUpdate>();
				updateBO.VoidReportProc(aProcessPeriod);
			}
			catch (Exception ex)
			{
				perrowmessage[0] = ex.Message;
				throw new ApplicationException(Messages.VoidingCommissionsFailed);
			}
			
		}
		static void CloseCommnPeriod(ARSPCommissionPeriod aProcessPeriod)
		{
			int size = 1;
			string[] perrowmessage = new string[size];
			PXLongOperation.SetCustomInfo(perrowmessage);
			try
			{
				ARSPCommissionUpdate proceesBO = PXGraph.CreateInstance<ARSPCommissionUpdate>();
				proceesBO.ClosePeriodProc(aProcessPeriod);
			}
			catch (Exception ex)
			{
				perrowmessage[0] = ex.Message;
				throw new ApplicationException("Closing commission period has failed");
			}
		}
		static void ReopenCommnPeriod(ARSPCommissionPeriod aProcessPeriod)
		{
			int size = 1;
			string[] perrowmessage = new string[size];
			PXLongOperation.SetCustomInfo(perrowmessage);
			try
			{
				ARSPCommissionUpdate proceesBO = PXGraph.CreateInstance<ARSPCommissionUpdate>();
				proceesBO.ReopenPeriodProc(aProcessPeriod);
			}
			catch (Exception ex)
			{
				perrowmessage[0] = ex.Message;
				throw new ApplicationException("Reopening commission period has failed");
			}
		}
		#endregion

		#region Events Handlers

		protected virtual void ARSPCommissionPeriod_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			if (e.Row == null) return;

			ARSPCommissionPeriod row = (ARSPCommissionPeriod) e.Row;
			bool isPreparedPeriod = ((ARSPCommissionPeriod) e.Row).Status == ARSPCommissionPeriodStatus.Prepared;
			bool isClosed = ((ARSPCommissionPeriod) e.Row).Status == ARSPCommissionPeriodStatus.Closed;

			ARSPCommissionPeriod closedOrPrepared = PXSelect
				<ARSPCommissionPeriod,
					Where<ARSPCommissionPeriod.commnPeriodID, Greater<Required<ARSPCommissionPeriod.commnPeriodID>>,
						And<ARSPCommissionPeriod.status, NotEqual<ARSPCommissionPeriodStatus.open>>>>.SelectWindowed(this, 0, 1,
							row.CommnPeriodID);
			bool isLastClosed = (closedOrPrepared == null);
			this.VoidCommissions.SetEnabled(isPreparedPeriod);
			this.ClosePeriod.SetEnabled(isPreparedPeriod);
			this.ReopenPeriod.SetEnabled(isClosed && isLastClosed);
		}
		
		#endregion

		#region Internal utility functions
		private List<ARSPCommnHistory> getSelected()
		{
			return getAll();
		}
		private List<ARSPCommnHistory> getAll()
		{
			List<ARSPCommnHistory> ret = new List<ARSPCommnHistory>();
			foreach (ARSPCommnHistory it in this.ToProcess.Select())
			{
				ret.Add(it);
			}
			return ret;
		}
		#endregion
	
		public PXSetup<ARSetup> ARSetup;
	}
}
