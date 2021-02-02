using System;
using System.Collections.Generic;
using System.Text;
using PX.Data;
using System.Collections;
using PX.Objects.CS;
using PX.Objects.Common.Discount;

namespace PX.Objects.AR
{
	[PX.Objects.GL.TableAndChartDashboardType]
	public class ARUpdateDiscounts : PXGraph<ARUpdateDiscounts>
	{
		public PXCancel<ItemFilter> Cancel;
		public PXFilter<ItemFilter> Filter;
		[PXFilterable]
		public PXFilteredProcessing<SelectedItem, ItemFilter> Items;

		public virtual IEnumerable items()
		{
			ItemFilter filter = Filter.Current;
			if (filter == null)
			{
				yield break;
			}
			bool found = false;
			foreach (SelectedItem item in Items.Cache.Inserted)
			{
				found = true;
				yield return item;
			}
			if (found)
				yield break;


			List<string> added = new List<string>();

			foreach (DiscountSequence sequence in PXSelect<
				DiscountSequence,
				Where<
					DiscountSequence.startDate, LessEqual<Current<ItemFilter.pendingDiscountDate>>,
					And<DiscountSequence.isPromotion, Equal<False>,
					And<DiscountSequence.isActive, Equal<True>>>>>
				.Select(this))
			{
				string key = string.Format("{0}.{1}", sequence.DiscountID, sequence.DiscountSequenceID);
				added.Add(key);

				SelectedItem item = new SelectedItem();
				item.DiscountID = sequence.DiscountID;
				item.DiscountSequenceID = sequence.DiscountSequenceID;
				item.Description = sequence.Description;
				item.DiscountedFor = sequence.DiscountedFor;
				item.BreakBy = sequence.BreakBy;
				item.IsPromotion = sequence.IsPromotion;
				item.IsActive = sequence.IsActive;
				item.StartDate = sequence.StartDate;
                item.EndDate = sequence.UpdateDate;

				yield return Items.Insert(item);
			}

			foreach (DiscountDetail detail in PXSelectGroupBy<
				DiscountDetail, 
				Where<
					DiscountDetail.startDate, LessEqual<Current<ItemFilter.pendingDiscountDate>>>,
				Aggregate<
					GroupBy<DiscountDetail.discountID, 
					GroupBy<DiscountDetail.discountSequenceID>>>>
				.Select(this))
			{
				string key = string.Format("{0}.{1}", detail.DiscountID, detail.DiscountSequenceID);

				if (!added.Contains(key))
				{
					DiscountSequence sequence = PXSelect<
						DiscountSequence,
						Where<
							DiscountSequence.discountID, Equal<Required<DiscountSequence.discountID>>,
							And<DiscountSequence.discountSequenceID, Equal<Required<DiscountSequence.discountSequenceID>>,
							And<DiscountSequence.isActive, Equal<True>>>>>
						.Select(this, detail.DiscountID, detail.DiscountSequenceID);

					if (sequence != null && sequence.IsPromotion == false)
					{
						SelectedItem item = new SelectedItem();
						item.DiscountID = sequence.DiscountID;
						item.DiscountSequenceID = sequence.DiscountSequenceID;
						item.Description = sequence.Description;
						item.DiscountedFor = sequence.DiscountedFor;
						item.BreakBy = sequence.BreakBy;
						item.IsPromotion = sequence.IsPromotion;
						item.IsActive = sequence.IsActive;
						item.StartDate = sequence.StartDate;
						item.EndDate = sequence.UpdateDate;

						yield return Items.Insert(item);
					}
				}
			}

			Items.Cache.IsDirty = false;
		}

		public ARUpdateDiscounts()
		{
			Items.SetSelected<SelectedItem.selected>();
			Items.SetProcessCaption(Messages.Process);
			Items.SetProcessAllCaption(Messages.ProcessAll);
		}

		#region EventHandlers
		protected virtual void ItemFilter_RowUpdated(PXCache cache, PXRowUpdatedEventArgs e)
		{
			Items.Cache.Clear();
		}

		protected virtual void ItemFilter_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			ItemFilter filter = Filter.Current;
			DateTime? date = Filter.Current.PendingDiscountDate;
			Items.SetProcessDelegate<UpdateDiscountProcess>(
					delegate(UpdateDiscountProcess graph, ARUpdateDiscounts.SelectedItem item)
					{						
						UpdateDiscount(graph, item, date);
					});
		}
		
		#endregion

		public static void UpdateDiscount(UpdateDiscountProcess graph, SelectedItem item, DateTime? filterDate)
		{
			graph.UpdateDiscount(item, filterDate);
		}

		public static void UpdateDiscount(string discountID, string discountSequenceID, DateTime? filterDate)
		{
			UpdateDiscountProcess graph = PXGraph.CreateInstance<UpdateDiscountProcess>();

			graph.UpdateDiscount(discountID, discountSequenceID, filterDate);
		}



		#region Local Types

		[Serializable]
		public partial class ItemFilter : IBqlTable
		{
			#region PendingDiscountDate
			public abstract class pendingDiscountDate : PX.Data.BQL.BqlDateTime.Field<pendingDiscountDate> { }
			protected DateTime? _PendingDiscountDate;
			[PXDBDate()]
			[PXDefault(typeof(AccessInfo.businessDate))]
			[PXUIField(DisplayName = "Max. Pending Discount Date")]
			public virtual DateTime? PendingDiscountDate
			{
				get
				{
					return this._PendingDiscountDate;
				}
				set
				{
					this._PendingDiscountDate = value;
				}
			}
			#endregion
		}

		[Serializable]
		public partial class SelectedItem : IBqlTable
		{
			#region Selected
			public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }
			protected Boolean? _Selected = false;
			[PXBool()]
			[PXUIField(DisplayName = "Selected")]
			public virtual Boolean? Selected
			{
				get
				{
					return this._Selected;
				}
				set
				{
					this._Selected = value;
				}
			}
			#endregion


			#region DiscountID
			public abstract class discountID : PX.Data.BQL.BqlString.Field<discountID> { }
			protected String _DiscountID;
			[PXDBString(10, IsUnicode = true, IsKey=true)]
			[PXUIField(DisplayName = "Discount Code", Visibility = PXUIVisibility.Visible)]
			public virtual String DiscountID
			{
				get
				{
					return this._DiscountID;
				}
				set
				{
					this._DiscountID = value;
				}
			}
			#endregion
			#region DiscountSequenceID
			public abstract class discountSequenceID : PX.Data.BQL.BqlString.Field<discountSequenceID> { }
			protected String _DiscountSequenceID;
			[PXDBString(10, IsUnicode = true, IsKey=true)]
			[PXUIField(DisplayName = "Sequence", Visibility = PXUIVisibility.Visible)]
			public virtual String DiscountSequenceID
			{
				get
				{
					return this._DiscountSequenceID;
				}
				set
				{
					this._DiscountSequenceID = value;
				}
			}
			#endregion
			#region Description
			public abstract class description : PX.Data.BQL.BqlString.Field<description> { }
			protected String _Description;
			[PXDBString(250, IsUnicode = true)]
			[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
			public virtual String Description
			{
				get
				{
					return this._Description;
				}
				set
				{
					this._Description = value;
				}
			}
			#endregion
			#region DiscountedFor
			public abstract class discountedFor : PX.Data.BQL.BqlString.Field<discountedFor> { }
			protected String _DiscountedFor;
			[PXDBString(1, IsFixed = true)]
			[PXDefault(DiscountOption.Percent)]
			[DiscountOption.List]
			[PXUIField(DisplayName = "Discount by", Visibility = PXUIVisibility.Visible)]
			public virtual String DiscountedFor
			{
				get
				{
					return this._DiscountedFor;
				}
				set
				{
					this._DiscountedFor = value;
				}
			}
			#endregion
			#region BreakBy
			public abstract class breakBy : PX.Data.BQL.BqlString.Field<breakBy> { }
			protected String _BreakBy;
			[PXDBString(1, IsFixed = true)]
			[PXDefault(BreakdownType.Amount)]
			[BreakdownType.List]
			[PXUIField(DisplayName = "Break by", Visibility = PXUIVisibility.Visible)]
			public virtual String BreakBy
			{
				get
				{
					return this._BreakBy;
				}
				set
				{
					this._BreakBy = value;
				}
			}
			#endregion
			#region IsPromotion
			public abstract class isPromotion : PX.Data.BQL.BqlBool.Field<isPromotion> { }
			protected Boolean? _IsPromotion;
			[PXDBBool()]
			[PXDefault(false)]
			[PXUIField(DisplayName = "Promotional", Visibility = PXUIVisibility.Visible)]
			public virtual Boolean? IsPromotion
			{
				get
				{
					return this._IsPromotion;
				}
				set
				{
					this._IsPromotion = value;
				}
			}
			#endregion
			#region IsActive
			public abstract class isActive : PX.Data.BQL.BqlBool.Field<isActive> { }
			protected Boolean? _IsActive;
			[PXDBBool()]
			[PXDefault(true)]
			[PXUIField(DisplayName = "Active", Visibility = PXUIVisibility.Visible)]
			public virtual Boolean? IsActive
			{
				get
				{
					return this._IsActive;
				}
				set
				{
					this._IsActive = value;
				}
			}
			#endregion
			#region StartDate
			public abstract class startDate : PX.Data.BQL.BqlDateTime.Field<startDate> { }
			protected DateTime? _StartDate;
			[PXDBDate()]
			[PXDefault()]
			[PXUIField(DisplayName = "Effective Date", Visibility = PXUIVisibility.Visible)]
			public virtual DateTime? StartDate
			{
				get
				{
					return this._StartDate;
				}
				set
				{
					this._StartDate = value;
				}
			}
			#endregion
			#region EndDate
			public abstract class endDate : PX.Data.BQL.BqlDateTime.Field<endDate> { }
			protected DateTime? _EndDate;
			[PXDBDate()]
			[PXUIField(DisplayName = "Last Update Date", Visibility = PXUIVisibility.Visible, Enabled = false)]
			public virtual DateTime? EndDate
			{
				get
				{
					return this._EndDate;
				}
				set
				{
					this._EndDate = value;
				}
			}
			#endregion
			
		} 

		#endregion
	}

	public class UpdateDiscountProcess : PXGraph<UpdateDiscountProcess>
	{

		public virtual void UpdateDiscount(ARUpdateDiscounts.SelectedItem item, DateTime? filterDate)
		{
			UpdateDiscount(item.DiscountID, item.DiscountSequenceID, filterDate);
		}

		public virtual void UpdateDiscount(string discountID, string discountSequenceID, DateTime? filterDate)
		{
			using (PXConnectionScope cs = new PXConnectionScope())
			{
                using (PXTransactionScope ts = new PXTransactionScope())
                {
                    foreach (DiscountSequenceDetail detail in PXSelect<DiscountSequenceDetail, Where<DiscountSequenceDetail.discountID, Equal<Required<DiscountSequenceDetail.discountID>>,
                        And<DiscountSequenceDetail.discountSequenceID, Equal<Required<DiscountSequenceDetail.discountSequenceID>>, And<DiscountSequenceDetail.isLast, Equal<False>>>>>.Select(this, discountID, discountSequenceID))
                    {
                        if (detail.PendingDate != null && detail.PendingDate.Value <= filterDate.Value)
                        {
                            if (!PXDatabase.Update<DiscountSequenceDetail>(
	                            				new PXDataFieldAssign("IsActive", PXDbType.Bit, detail.IsActive),                        
                                                new PXDataFieldAssign("Amount", PXDbType.Decimal, detail.Amount),
                                                new PXDataFieldAssign("AmountTo", PXDbType.Decimal, detail.AmountTo),
                                                new PXDataFieldAssign("Quantity", PXDbType.Decimal, detail.Quantity),
                                                new PXDataFieldAssign("QuantityTo", PXDbType.Decimal, detail.QuantityTo),
                                                new PXDataFieldAssign("Discount", PXDbType.Decimal, detail.Discount),
                                                new PXDataFieldAssign("FreeItemQty", PXDbType.Decimal, detail.FreeItemQty),
                                                new PXDataFieldAssign("LastDate", PXDbType.DateTime, detail.PendingDate),
                                                new PXDataFieldAssign("PendingAmount", PXDbType.Decimal, null),
                                                new PXDataFieldAssign("PendingQuantity", PXDbType.Decimal, 0m),
                                                new PXDataFieldAssign("PendingDiscount", PXDbType.Decimal, null),
                                                new PXDataFieldAssign("PendingFreeItemQty", PXDbType.Decimal, 0m),
                                                new PXDataFieldAssign("PendingDate", PXDbType.DateTime, null),
                                                new PXDataFieldRestrict("DiscountID", PXDbType.NVarChar, detail.DiscountID),
                                                new PXDataFieldRestrict("DiscountSequenceID", PXDbType.NVarChar, detail.DiscountSequenceID),
                                                new PXDataFieldRestrict("LineNbr", PXDbType.Int, detail.LineNbr),
                                                new PXDataFieldRestrict("IsLast", PXDbType.Bit, 1)
                                                ))
                            {
                                PXDatabase.Insert<DiscountSequenceDetail>(
                                                new PXDataFieldAssign("DiscountID", PXDbType.NVarChar, detail.DiscountID),
                                                new PXDataFieldAssign("DiscountSequenceID", PXDbType.NVarChar, detail.DiscountSequenceID),
                                                new PXDataFieldAssign("LineNbr", PXDbType.Int, detail.LineNbr),
                                                new PXDataFieldAssign("IsActive", PXDbType.Bit, detail.IsActive),        
                                                new PXDataFieldAssign("IsLast", PXDbType.Bit, 1),            
                                                new PXDataFieldAssign("Amount", PXDbType.Decimal, detail.Amount),
                                                new PXDataFieldAssign("AmountTo", PXDbType.Decimal, detail.AmountTo),
                                                new PXDataFieldAssign("Quantity", PXDbType.Decimal, detail.Quantity),
                                                new PXDataFieldAssign("QuantityTo", PXDbType.Decimal, detail.QuantityTo),
                                                new PXDataFieldAssign("Discount", PXDbType.Decimal, detail.Discount),
                                                new PXDataFieldAssign("FreeItemQty", PXDbType.Decimal, detail.FreeItemQty),
                                                new PXDataFieldAssign("LastDate", PXDbType.DateTime, detail.PendingDate),
                                                new PXDataFieldAssign("PendingAmount", PXDbType.Decimal, null),
                                                new PXDataFieldAssign("PendingQuantity", PXDbType.Decimal, 0m),
                                                new PXDataFieldAssign("PendingDiscount", PXDbType.Decimal, null),
                                                new PXDataFieldAssign("PendingFreeItemQty", PXDbType.Decimal, 0m),
                                                new PXDataFieldAssign("PendingDate", PXDbType.DateTime, null),
                                                new PXDataFieldAssign("CreatedByID", PXDbType.UniqueIdentifier, 16, detail.CreatedByID),
					                            new PXDataFieldAssign("CreatedByScreenID", PXDbType.Char, 8, detail.CreatedByScreenID),
					                            new PXDataFieldAssign("CreatedDateTime", PXDbType.DateTime, 8, detail.CreatedDateTime),
					                            new PXDataFieldAssign("LastModifiedByID", PXDbType.UniqueIdentifier, 16, detail.LastModifiedByID),
					                            new PXDataFieldAssign("LastModifiedByScreenID", PXDbType.Char, 8, detail.LastModifiedByScreenID),
					                            new PXDataFieldAssign("LastModifiedDateTime", PXDbType.DateTime, 8, detail.LastModifiedDateTime)
                                                );
                            }


                            
                            PXDatabase.Update<DiscountSequenceDetail>(
                                                new PXDataFieldAssign("Amount", PXDbType.DirectExpression, "PendingAmount"),
                                                new PXDataFieldAssign("Quantity", PXDbType.DirectExpression, "PendingQuantity"),
                                                new PXDataFieldAssign("Discount", PXDbType.DirectExpression, "PendingDiscount"),
                                                new PXDataFieldAssign("FreeItemQty", PXDbType.DirectExpression, "PendingFreeItemQty"),
                                                new PXDataFieldAssign("LastDate", PXDbType.DirectExpression, "PendingDate"),
                                                new PXDataFieldAssign("PendingAmount", PXDbType.Decimal, null),
                                                new PXDataFieldAssign("PendingQuantity", PXDbType.Decimal, 0m),
                                                new PXDataFieldAssign("PendingDiscount", PXDbType.Decimal, null),
                                                new PXDataFieldAssign("PendingFreeItemQty", PXDbType.Decimal, 0m),
                                                new PXDataFieldAssign("PendingDate", PXDbType.DateTime, null),
                                                new PXDataFieldRestrict("DiscountID", PXDbType.NVarChar, detail.DiscountID),
                                                new PXDataFieldRestrict("DiscountSequenceID", PXDbType.NVarChar, detail.DiscountSequenceID),
                                                new PXDataFieldRestrict("LineNbr", PXDbType.Int, detail.LineNbr),
                                                new PXDataFieldRestrict("IsLast", PXDbType.Bit, 0)
                                                );


                        }
                    }

                    foreach (DiscountSequenceDetail detail in PXSelectReadonly<DiscountSequenceDetail, Where<DiscountSequenceDetail.discountID, Equal<Required<DiscountSequenceDetail.discountID>>,
                        And<DiscountSequenceDetail.discountSequenceID, Equal<Required<DiscountSequenceDetail.discountSequenceID>>>>>.Select(this, discountID, discountSequenceID))
                    {
                        DiscountSequenceDetail amonextval = PXSelectReadonly<DiscountSequenceDetail, Where<DiscountSequenceDetail.discountID, Equal<Required<DiscountSequenceDetail.discountID>>,
                                               And<DiscountSequenceDetail.discountSequenceID, Equal<Required<DiscountSequenceDetail.discountSequenceID>>, And<DiscountSequenceDetail.amount, Greater<Required<DiscountSequenceDetail.amount>>, And<DiscountSequenceDetail.isLast, Equal<Required<DiscountSequenceDetail.isLast>>, And<DiscountSequenceDetail.isActive, Equal<True>>>>>>,
                                               OrderBy<Asc<DiscountSequenceDetail.amount>>>.SelectWindowed(this, 0, 1, discountID, discountSequenceID, detail.Amount, detail.IsLast);
                        DiscountSequenceDetail qtynextval = PXSelectReadonly<DiscountSequenceDetail, Where<DiscountSequenceDetail.discountID, Equal<Required<DiscountSequenceDetail.discountID>>,
                                                And<DiscountSequenceDetail.discountSequenceID, Equal<Required<DiscountSequenceDetail.discountSequenceID>>, And<DiscountSequenceDetail.quantity, Greater<Required<DiscountSequenceDetail.quantity>>, And<DiscountSequenceDetail.isLast, Equal<Required<DiscountSequenceDetail.isLast>>, And<DiscountSequenceDetail.isActive, Equal<True>>>>>>,
                                                OrderBy<Asc<DiscountSequenceDetail.quantity>>>.SelectWindowed(this, 0, 1, discountID, discountSequenceID, detail.Quantity, detail.IsLast);
                        PXDatabase.Update<DiscountSequenceDetail>(
                                            new PXDataFieldAssign("AmountTo", PXDbType.Decimal, (amonextval == null ? null : amonextval.Amount)),
                                            new PXDataFieldAssign("QuantityTo", PXDbType.Decimal, (qtynextval == null ? null : qtynextval.Quantity)),
                                            new PXDataFieldRestrict("DiscountDetailsID", PXDbType.Int, detail.DiscountDetailsID)
                                            );
                    }
                    

                    DiscountSequence sequence = PXSelect<DiscountSequence,
                        Where<DiscountSequence.discountID, Equal<Required<DiscountSequence.discountID>>,
                        And<DiscountSequence.discountSequenceID, Equal<Required<DiscountSequence.discountSequenceID>>>>>.Select(this, discountID, discountSequenceID);

                    if (sequence != null && sequence.StartDate != null && sequence.PendingFreeItemID != null)
                    {
                        PXDatabase.Update<DiscountSequence>(
                                                new PXDataFieldAssign("LastFreeItemID", PXDbType.DirectExpression, "FreeItemID"),
                                                new PXDataFieldAssign("FreeItemID", PXDbType.DirectExpression, "PendingFreeItemID"),
                                                new PXDataFieldAssign("PendingFreeItemID", PXDbType.Int, null),
                                                new PXDataFieldAssign("UpdateDate", PXDbType.DateTime, filterDate.Value),
                            //new PXDataFieldAssign("StartDate", PXDbType.DateTime, null),
                                                new PXDataFieldRestrict("DiscountID", PXDbType.VarChar, discountID),
                                                new PXDataFieldRestrict("DiscountSequenceID", PXDbType.VarChar, discountSequenceID)
                                                );
                    }
                    else if (sequence != null && sequence.StartDate != null)
                    {
                        PXDatabase.Update<DiscountSequence>(
                                                new PXDataFieldAssign("UpdateDate", PXDbType.DateTime, filterDate.Value),
                                                new PXDataFieldRestrict("DiscountID", PXDbType.VarChar, discountID),
                                                new PXDataFieldRestrict("DiscountSequenceID", PXDbType.VarChar, discountSequenceID)
                                                );
                    }
                    ts.Complete();
                }
			}

		}
		
	}
}
