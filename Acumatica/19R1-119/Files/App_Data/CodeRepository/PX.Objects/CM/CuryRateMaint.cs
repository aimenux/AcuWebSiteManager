using System;
using PX.Data;
using System.Collections;
using System.Collections.Generic;

namespace PX.Objects.CM
{
	[PX.Objects.GL.TableAndChartDashboardType]
	public class CuryRateMaint : PXGraph<CuryRateMaint>
	{
		#region Buttons
			public PXSave<CuryRateFilter> Save;	
			public PXCancel<CuryRateFilter> Cancel;
			
			#region Button First
			  public PXAction<CuryRateFilter> first;
			  [PXFirstButton]
			  [PXUIField]
			  protected virtual System.Collections.IEnumerable First(PXAdapter a)
			  {
				  PXLongOperation.ClearStatus(this.UID);
				  CuryRateFilter current		= (CuryRateFilter) Filter.Current;
				  PXSelectBase<CurrencyRate> currentSelectStatement;

				  if (current.ToCurrency != null)
				  {
					  CurrencyRate rate;
					  currentSelectStatement = new PXSelectReadonly<CurrencyRate, Where<CurrencyRate.toCuryID, Equal<Required<CurrencyRate.toCuryID>>>,
																				 OrderBy<Asc<CurrencyRate.curyEffDate>>>(this);
					  rate = (CurrencyRate) currentSelectStatement.View.SelectSingle(current.ToCurrency);
					  if (rate != null)
					  {
						  current = (CuryRateFilter) Filter.Cache.CreateCopy(Filter.Current);
						  current.ToCurrency = rate.ToCuryID;
						  current.EffDate	 = rate.CuryEffDate;
						  current = Filter.Update(current);
						  Filter.Cache.IsDirty = false;
					  }
				  }
				  yield return current;
			  }
			#endregion
			#region Button Prev
			  public PXAction<CuryRateFilter> prev;
			  [PXPreviousButton]
			  [PXUIField]
			  protected virtual System.Collections.IEnumerable Prev(PXAdapter a)
			  {
				  PXLongOperation.ClearStatus(this.UID);
				  CuryRateFilter current		= (CuryRateFilter) Filter.Current;
				  PXSelectBase<CurrencyRate> currentSelectStatement;

				  if (current.ToCurrency != null)
				  {
					  CurrencyRate rate;
					  if (current.EffDate != null)
					  {
						currentSelectStatement = new PXSelectReadonly<CurrencyRate, Where<CurrencyRate.toCuryID, Equal<Required<CurrencyRate.toCuryID>>,
																					  And<CurrencyRate.curyEffDate, Less<Required<CurrencyRate.curyEffDate>>>>,
																				 OrderBy<Desc<CurrencyRate.curyEffDate>>>(this);
						rate	  = (CurrencyRate) currentSelectStatement.View.SelectSingle(current.ToCurrency, current.EffDate);
					  }
					  else
					  {
						  currentSelectStatement = new PXSelectReadonly<CurrencyRate, Where<CurrencyRate.toCuryID, Equal<Required<CurrencyRate.toCuryID>>>,
																					 OrderBy<Desc<CurrencyRate.curyEffDate>>>(this);
						  rate	  = (CurrencyRate) currentSelectStatement.View.SelectSingle(current.ToCurrency);
					  }
					  if (rate != null)
					  {
						  current = (CuryRateFilter) Filter.Cache.CreateCopy(Filter.Current);
						  current.ToCurrency = rate.ToCuryID;
						  current.EffDate	 = rate.CuryEffDate;
						  current = Filter.Update(current);
						  Filter.Cache.IsDirty = false;
					  }
				  }
				  yield return current;
			  }
			#endregion
			#region Button Next
			  public PXAction<CuryRateFilter> next;
			  [PXNextButton]
			  [PXUIField]
			  protected virtual System.Collections.IEnumerable Next(PXAdapter a)
			  {
				  PXLongOperation.ClearStatus(this.UID);
				  CuryRateFilter current		= (CuryRateFilter) Filter.Current;
				  PXSelectBase<CurrencyRate> currentSelectStatement;

				  if (current.ToCurrency != null)
				  {
					  CurrencyRate rate;
					  if (current.EffDate != null)
					  {
						currentSelectStatement = new PXSelectReadonly<CurrencyRate, Where<CurrencyRate.toCuryID, Equal<Required<CurrencyRate.toCuryID>>,
																					  And<CurrencyRate.curyEffDate, Greater<Required<CurrencyRate.curyEffDate>>>>,
																				 OrderBy<Asc<CurrencyRate.curyEffDate>>>(this);
						rate	  = (CurrencyRate) currentSelectStatement.View.SelectSingle(current.ToCurrency, current.EffDate);
					  }
					  else
					  {
						  currentSelectStatement = new PXSelectReadonly<CurrencyRate, Where<CurrencyRate.toCuryID, Equal<Required<CurrencyRate.toCuryID>>>,
																					 OrderBy<Asc<CurrencyRate.curyEffDate>>>(this);
						  rate	  = (CurrencyRate) currentSelectStatement.View.SelectSingle(current.ToCurrency);
					  }
					  if (rate != null)
					  {
						  current = (CuryRateFilter) Filter.Cache.CreateCopy(Filter.Current);
						  current.ToCurrency = rate.ToCuryID;
						  current.EffDate	 = rate.CuryEffDate;
						  current = Filter.Update(current);
						  Filter.Cache.IsDirty = false;
					  }
				  }
				  yield return current;
			  }
			#endregion
			#region Button Last
			  public PXAction<CuryRateFilter> last;
			  [PXLastButton]
			  [PXUIField]
			  protected virtual System.Collections.IEnumerable Last(PXAdapter a)
			  {
				  PXLongOperation.ClearStatus(this.UID);
				  CuryRateFilter current		= (CuryRateFilter) Filter.Current;
				  PXSelectBase<CurrencyRate> currentSelectStatement;
				  
				  if (current.ToCurrency != null)
				  {
					  CurrencyRate rate;
					  currentSelectStatement = new PXSelectReadonly<CurrencyRate, Where<CurrencyRate.toCuryID, Equal<Required<CurrencyRate.toCuryID>>>,
																				 OrderBy<Desc<CurrencyRate.curyEffDate>>>(this);
					  rate = (CurrencyRate) currentSelectStatement.View.SelectSingle(current.ToCurrency);
					  if (rate != null)
					  {
						  current = (CuryRateFilter) Filter.Cache.CreateCopy(Filter.Current);
						  current.ToCurrency = rate.ToCuryID;
						  current.EffDate	 = rate.CuryEffDate;
						  current = Filter.Update(current);
						  Filter.Cache.IsDirty = false;
					  }
				  }
				  yield return current;
			  }
			#endregion
		#endregion

		#region Select Statements	
		public PXFilter<CuryRateFilter> Filter;

		[PXImport(typeof(CuryRateFilter))]
		public PXSelectOrderBy<CurrencyRate, OrderBy<Asc<CurrencyRate.fromCuryID, Asc<CurrencyRate.curyRateType>>>> CuryRateRecordsEntry;
		protected virtual IEnumerable curyRateRecordsEntry()
		{
			CuryRateFilter f = Filter.Current as CuryRateFilter;
			List<CurrencyRate> ret = new List<CurrencyRate>();

			foreach (CurrencyRate r in PXSelect<CurrencyRate,
				Where<CurrencyRate.toCuryID, Equal<Required<CurrencyRate.toCuryID>>,
				And<CurrencyRate.curyEffDate, Equal<Required<CurrencyRate.curyEffDate>>>>>.Select(this, f.ToCurrency, f.EffDate))
			{
				ret.Add(r);
			}
			foreach (CurrencyRate r in CuryRateRecordsEntry.Cache.Inserted)
			{
				if (!ret.Contains(r))
					ret.Add(r);
			}
			foreach (CurrencyRate r in CuryRateRecordsEntry.Cache.Updated)
			{
				if (!ret.Contains(r))
					ret.Add(r);
			}

			return ret;
		}
		public PXSelectOrderBy<CurrencyRate2, OrderBy<Asc<CurrencyRate2.fromCuryID, Asc<CurrencyRate2.curyRateType>>>> CuryRateRecordsEffDate;
		protected virtual IEnumerable curyRateRecordsEffDate()
		{
			PXSelectBase<CurrencyRate2> sel = new PXSelect<CurrencyRate2,
				Where<CurrencyRate2.toCuryID, Equal<Required<CurrencyRate2.toCuryID>>,
				And<CurrencyRate2.fromCuryID, Equal<Required<CurrencyRate2.fromCuryID>>,
				And<CurrencyRate2.curyRateType, Equal<Required<CurrencyRate2.curyRateType>>,
				And<CurrencyRate2.curyEffDate, Equal<Required<CurrencyRate2.curyEffDate>>>>>>>(this);

			CuryRateFilter f = Filter.Current as CuryRateFilter;
			List<CurrencyRate2> ret = new List<CurrencyRate2>();

			foreach (CurrencyRate2 r in PXSelectGroupBy<CurrencyRate2,
				Where<CurrencyRate2.toCuryID, Equal<Current<CuryRateFilter.toCurrency>>,
				And<CurrencyRate2.curyEffDate, LessEqual<Current<CuryRateFilter.effDate>>>>,
				Aggregate<Max<CurrencyRate2.curyEffDate,
				GroupBy<CurrencyRate2.curyRateType,
				GroupBy<CurrencyRate2.fromCuryID>>>>>.Select(this))
			{
				ret.Add((CurrencyRate2)sel.Select(f.ToCurrency, r.FromCuryID, r.CuryRateType, r.CuryEffDate));
			}
			return ret;
		}

		public PXSetup<CMSetup> CMSetup;
		
		#endregion

		#region Constructor
		public CuryRateMaint()
		{
			PXCache entry   = CuryRateRecordsEntry.Cache;
			PXCache effDate = CuryRateRecordsEffDate.Cache;
			PXUIFieldAttribute.SetVisible<CurrencyRate.curyRateID>		(entry, null, false);
			PXUIFieldAttribute.SetEnabled<CurrencyRate.rateReciprocal>	(entry, null, false);
			
			PXUIFieldAttribute.SetVisible<CurrencyRate2.curyRateID>		(effDate, null, false);
			PXUIFieldAttribute.SetEnabled<CurrencyRate2.rateReciprocal> (effDate, null, false);

			entry.AllowInsert = true;
			entry.AllowUpdate = true;
			entry.AllowDelete = true;

			effDate.AllowDelete = false;
			effDate.AllowInsert = false;
			effDate.AllowUpdate = false;
			
		}
		#endregion

		#region CurrencyRate Events 
		protected virtual void CurrencyRate_RowUpdating(PXCache cache, PXRowUpdatingEventArgs e)
		{
			CurrencyRate cr = e.NewRow as CurrencyRate;
			if (cr.CuryEffDate == null)
				cr.CuryEffDate = DateTime.Now;

			if (cr.FromCuryID != null && cr.ToCuryID != null && String.Compare(cr.FromCuryID, cr.ToCuryID, true) == 0)
			{
				throw new PXException(Messages.DestShouldDifferFromOrig);
			}
			if (cr.CuryRate != null)
			{
				if (Math.Round((decimal)(cr.CuryRate), 8) == 0)
				{
					throw new PXException(Messages.CuryRateCantBeZero);
				}
				cr.RateReciprocal = Math.Round((decimal)(1 / cr.CuryRate), 8);
				if (((CurrencyInfo)cr).CheckRateVariance(cache))
				{
					cache.RaiseExceptionHandling<CurrencyRate.curyRate>(cr, cr.CuryRate, new PXSetPropertyException(Messages.RateVarianceExceeded, PXErrorLevel.Warning));
				}
			}	
			if (cr.FromCuryID   != null && 
				cr.CuryRateType != null && 
				cr.CuryRateType != null && 
				cr.CuryEffDate  != null &&
				cr.CuryRate		!= null)
			{
				CurrencyRate existRate =  PXSelectReadonly<CurrencyRate,
								Where<CurrencyRate.toCuryID,   Equal<Required<CurrencyRate.toCuryID>>,
								And<CurrencyRate.fromCuryID,   Equal<Required<CurrencyRate.fromCuryID>>,
								And<CurrencyRate.curyRateType, Equal<Required<CurrencyRate.curyRateType>>,
								And<CurrencyRate.curyEffDate,  Equal<Required<CurrencyRate.curyEffDate>>>>>>,
								OrderBy<Desc<CurrencyRate.curyEffDate>>>.Select(cache.Graph, cr.ToCuryID, cr.FromCuryID, cr.CuryRateType, cr.CuryEffDate);
			

				if (existRate != null)
				{
					if (CuryRateRecordsEntry.Locate(existRate) == null)
					{
						CurrencyRate newExistRate		= (CurrencyRate) CuryRateRecordsEntry.Cache.CreateCopy(existRate);
						newExistRate.CuryRate			= cr.CuryRate;
						newExistRate.CuryMultDiv		= cr.CuryMultDiv;
						newExistRate.RateReciprocal		= cr.RateReciprocal;
						CuryRateRecordsEntry.Delete(cr);
						CuryRateRecordsEntry.Update(newExistRate);
					}
					cache.RaiseExceptionHandling<CurrencyRate.curyRate>(cr, cr.CuryRate, new PXSetPropertyException(Messages.SuchRateTypeAlreadyExist, PXErrorLevel.Warning));
				}
			}
		}

		protected virtual void CurrencyRate_RowInserting(PXCache cache, PXRowInsertingEventArgs e)
		{
			CurrencyRate cr = e.Row as CurrencyRate;
			CuryRateFilter crf = Filter.Current as CuryRateFilter;

			if (cr.FromCuryID != null && crf.ToCurrency != null && String.Compare(cr.FromCuryID, crf.ToCurrency, true) == 0)
			{
				throw new PXException(Messages.DestShouldDifferFromOrig);
			}

			bool importing = e.Row != null && cache.GetValuePending(e.Row, PXImportAttribute.ImportFlag) != null;
			if (e.ExternalCall && !importing)
			{
				if (cr.CuryEffDate == null)
				{
					if (crf.EffDate == null)
						cr.CuryEffDate = DateTime.Now;
					else
						cr.CuryEffDate = crf.EffDate;
				}

				if (cr.CuryRate != null)
				{
					cr.RateReciprocal = Math.Round((decimal)(1 / cr.CuryRate), 8);
					if (((CurrencyInfo)cr).CheckRateVariance(cache))
					{
						cache.RaiseExceptionHandling<CurrencyRate.curyRate>(cr, cr.CuryRate, new PXSetPropertyException(Messages.RateVarianceExceeded, PXErrorLevel.Warning));
					}
				}

				if (cr.CuryMultDiv  == " "  &&
				    cr.FromCuryID   != null && 
				    cr.ToCuryID	    != null &&
				    cr.CuryRateType != null &&
				    cr.CuryEffDate  != null)
				{
					CurrencyRate existRateMultDiv =  PXSelectReadonly<CurrencyRate, 
						Where<CurrencyRate.fromCuryID,	Equal<Required<CurrencyRate.fromCuryID>>,
							And<CurrencyRate.toCuryID,		Equal<Required<CurrencyRate.toCuryID>>,
								And<CurrencyRate.curyRateType,	Equal<Required<CurrencyRate.curyRateType>>,
									And<CurrencyRate.curyEffDate,	LessEqual<Required<CurrencyRate.curyEffDate>>>>>>,
						OrderBy<Desc<CurrencyRate.curyEffDate>>>.Select(cache.Graph, cr.FromCuryID, cr.ToCuryID, cr.CuryRateType, cr.CuryEffDate);

					if (existRateMultDiv != null)
						cr.CuryMultDiv = existRateMultDiv.CuryMultDiv;
				}
			}
			if (cr.FromCuryID   != null && 
				cr.CuryRateType != null && 
				cr.CuryRateType != null && 
				cr.CuryEffDate  != null &&
				cr.CuryRate		!= null)
			{
				CurrencyRate existRate =  PXSelectReadonly<CurrencyRate,
								Where<CurrencyRate.toCuryID,   Equal<Required<CurrencyRate.toCuryID>>,
								And<CurrencyRate.fromCuryID,   Equal<Required<CurrencyRate.fromCuryID>>,
								And<CurrencyRate.curyRateType, Equal<Required<CurrencyRate.curyRateType>>,
								And<CurrencyRate.curyEffDate,  Equal<Required<CurrencyRate.curyEffDate>>>>>>>.Select(cache.Graph, cr.ToCuryID, cr.FromCuryID, cr.CuryRateType, cr.CuryEffDate);

				if (existRate != null)
				{
					e.Cancel = true;
					CurrencyRate newExistRate	= (CurrencyRate) CuryRateRecordsEntry.Cache.CreateCopy(existRate);
					newExistRate.CuryRate		= cr.CuryRate;
					newExistRate.CuryMultDiv	= cr.CuryMultDiv;
					newExistRate.RateReciprocal	= cr.RateReciprocal;
					CuryRateRecordsEntry.Update(newExistRate);
					cache.RaiseExceptionHandling<CurrencyRate.curyRate>(cr, cr.CuryRate, new PXSetPropertyException(Messages.SuchRateTypeAlreadyExist, PXErrorLevel.Warning));
				}
			}	
		}

		protected virtual void CurrencyRate_FromCuryID_FieldUpdating(PXCache cache, PXFieldUpdatingEventArgs e)
		{
			CuryRateFilter crf = Filter.Current as CuryRateFilter;
			if (e.NewValue != null && crf.ToCurrency != null && String.Compare((string)e.NewValue, crf.ToCurrency, true) == 0)
			{
                cache.RaiseExceptionHandling("FromCuryID", e.Row, e.NewValue, new PXSetPropertyException(Messages.DestShouldDifferFromOrig));
				cache.SetStatus(e.Row, PXEntryStatus.Notchanged);
			}
		}
		
		protected virtual void CurrencyRate_FromCuryID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			CuryRateFilter crf = Filter.Current as CuryRateFilter;
			CurrencyRate  rate = (CurrencyRate)e.Row;
			if (rate == null) return;
			if (rate.FromCuryID != null && rate.CuryRateType != null && rate.CuryEffDate != null)
			{
				CurrencyRate existRate =  PXSelectReadonly<CurrencyRate,
							Where<CurrencyRate.toCuryID,   Equal<Required<CurrencyRate.toCuryID>>,
							And<CurrencyRate.fromCuryID,   Equal<Required<CurrencyRate.fromCuryID>>,
							And<CurrencyRate.curyRateType, Equal<Required<CurrencyRate.curyRateType>>,
							And<CurrencyRate.curyEffDate,  LessEqual<Required<CurrencyRate.curyEffDate>>>>>>,
							OrderBy<Desc<CurrencyRate.curyEffDate>>>.Select(cache.Graph, rate.ToCuryID, rate.FromCuryID, rate.CuryRateType, rate.CuryEffDate);

				if (existRate != null)
				{
					rate.CuryMultDiv = existRate.CuryMultDiv;					
				}	
			}
		}
		protected virtual void CurrencyRate_CuryRateType_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			CuryRateFilter crf = Filter.Current as CuryRateFilter;
			CurrencyRate  rate = (CurrencyRate)e.Row;
			if (rate == null) return;
			if (rate.FromCuryID != null && rate.CuryRateType != null && rate.CuryEffDate != null)
			{
				CurrencyRate existRate =  PXSelectReadonly<CurrencyRate,
							Where<CurrencyRate.toCuryID,   Equal<Required<CurrencyRate.toCuryID>>,
							And<CurrencyRate.fromCuryID,   Equal<Required<CurrencyRate.fromCuryID>>,
							And<CurrencyRate.curyRateType, Equal<Required<CurrencyRate.curyRateType>>,
							And<CurrencyRate.curyEffDate,  LessEqual<Required<CurrencyRate.curyEffDate>>>>>>,
							OrderBy<Desc<CurrencyRate.curyEffDate>>>.Select(cache.Graph, rate.ToCuryID, rate.FromCuryID, rate.CuryRateType, rate.CuryEffDate);

				if (existRate != null)
				{
					rate.CuryMultDiv = existRate.CuryMultDiv;					
				}	
			}
		}
		
		protected virtual void CurrencyRate_CuryEffDate_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			CuryRateFilter crf = Filter.Current as CuryRateFilter;
			CurrencyRate  rate = (CurrencyRate)e.Row;
			if (rate == null) return;
			if (rate.FromCuryID != null && rate.CuryRateType != null && rate.CuryEffDate != null)
			{
				CurrencyRate existRate =  PXSelectReadonly<CurrencyRate,
							Where<CurrencyRate.toCuryID,   Equal<Required<CurrencyRate.toCuryID>>,
							And<CurrencyRate.fromCuryID,   Equal<Required<CurrencyRate.fromCuryID>>,
							And<CurrencyRate.curyRateType, Equal<Required<CurrencyRate.curyRateType>>,
							And<CurrencyRate.curyEffDate,  LessEqual<Required<CurrencyRate.curyEffDate>>>>>>,
							OrderBy<Desc<CurrencyRate.curyEffDate>>>.Select(cache.Graph, rate.ToCuryID, rate.FromCuryID, rate.CuryRateType, rate.CuryEffDate);

				if (existRate != null)
				{
					rate.CuryMultDiv = existRate.CuryMultDiv;					
				}	
			}
		}

		protected virtual void CurrencyRate_CuryRateID_CommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
		{
			if ((e.Operation & PXDBOperation.Option) == PXDBOperation.Second)
			{
				e.Cancel = true;
				e.IsRestriction = true;
				e.Expr = new Data.SQLTree.Column<CurrencyRate.curyRateID>();
				e.DataValue = PXDatabase.SelectIdentity();
			}
		}

		protected virtual void CurrencyRate_CuryRateType_CommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
		{
			if ((e.Operation & PXDBOperation.Option) == PXDBOperation.Second)
			{
				e.IsRestriction = true;
			}
		}

		protected virtual void CurrencyRate_FromCuryID_CommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
		{
			if ((e.Operation & PXDBOperation.Option) == PXDBOperation.Second)
			{
				e.IsRestriction = true;
			}
		}

		protected virtual void CurrencyRate_ToCuryID_CommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
		{
			if ((e.Operation & PXDBOperation.Option) == PXDBOperation.Second)
			{
				e.IsRestriction = true;
			}
		}

		protected virtual void CurrencyRate_CuryEffDate_CommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
		{
			if ((e.Operation & PXDBOperation.Option) == PXDBOperation.Second)
			{
				e.IsRestriction = true;
			}
		}
		
		#endregion
		
		#region CuryRateFilter Events
		protected virtual void CuryRateFilter_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			CuryRateFilter filter = (CuryRateFilter)e.Row;
            if (filter == null) return;

			bool nextEnabled  = false;
			bool prevEnabled  = false;
			bool lastEnabled  = false;
			bool firstEnabled = false;
			
			PXSelectBase currentSelectStatement = null;
			CurrencyRate cachesRateNext			= null;
			CurrencyRate cachesRatePrev			= null;

			if (filter.ToCurrency != null && filter.EffDate != null)
			{
				currentSelectStatement = new PXSelectReadonly<CurrencyRate, Where<CurrencyRate.toCuryID, Equal<Required<CurrencyRate.toCuryID>>,
																		  And<CurrencyRate.curyEffDate, Greater<Required<CurrencyRate.curyEffDate>>>>,
																		 OrderBy<Asc<CurrencyRate.curyEffDate>>>(this);
				cachesRateNext = (CurrencyRate)currentSelectStatement.View.SelectSingle(filter.ToCurrency, filter.EffDate);

				currentSelectStatement = new PXSelectReadonly<CurrencyRate, Where<CurrencyRate.toCuryID, Equal<Required<CurrencyRate.toCuryID>>,
																		  And<CurrencyRate.curyEffDate, Less<Required<CurrencyRate.curyEffDate>>>>,
																		 OrderBy<Desc<CurrencyRate.curyEffDate>>>(this);
				cachesRatePrev = (CurrencyRate)currentSelectStatement.View.SelectSingle(filter.ToCurrency, filter.EffDate);
				nextEnabled  = cachesRateNext != null;
				prevEnabled  = cachesRatePrev != null;

				if (!IsImport)
				{
					bool isModified = this.CuryRateRecordsEntry.Cache.IsDirty;
					PXUIFieldAttribute.SetEnabled<CuryRateFilter.toCurrency>(cache, filter, !isModified);
					PXUIFieldAttribute.SetEnabled<CuryRateFilter.effDate>(cache, filter, !isModified);
				}
			}

			firstEnabled = prevEnabled;
			lastEnabled  = nextEnabled;
		    
			next.SetEnabled (nextEnabled);
		    prev.SetEnabled (prevEnabled);
			first.SetEnabled(firstEnabled);
			last.SetEnabled (lastEnabled);
		}
		#endregion
	}
}
