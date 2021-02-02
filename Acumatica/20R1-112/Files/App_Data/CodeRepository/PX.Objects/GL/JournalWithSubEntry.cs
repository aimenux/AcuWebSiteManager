using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

using PX.Data;
using PX.Objects.AP;
using PX.Objects.AR;
using PX.Objects.CA;
using PX.Objects.CM;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.PM;
using PX.Objects.TX;
using PX.Objects.AP.MigrationMode;
using PX.Objects.AR.MigrationMode;

using EPEmployee = PX.TM.PXOwnerSelectorAttribute.EPEmployee;
using APQuickCheck = PX.Objects.AP.Standalone.APQuickCheck;
using ARCashSale = PX.Objects.AR.Standalone.ARCashSale;
using PX.Objects.Common.Bql;
using PX.Objects.GL.FinPeriods;
using PX.Objects.Common.GraphExtensions.Abstract;
using PX.Objects.Common.GraphExtensions.Abstract.DAC;
using PX.Objects.Common.GraphExtensions.Abstract.Mapping;
using PX.Objects.Common;

namespace PX.Objects.GL
{
	[Serializable]
	public class JournalWithSubEntry : PXGraph<JournalWithSubEntry, GLDocBatch>, PXImportAttribute.IPXPrepareItems
	{
		[InjectDependency]
		public IFinPeriodRepository FinPeriodRepository { get; set; }

		#region Extensions
		public class JournalWithSubEntryDocumentExtension : DocumentWithLinesGraphExtension<JournalWithSubEntry>
		{
			public override void Initialize()
			{
				base.Initialize();

				Documents = new PXSelectExtension<Document>(Base.BatchModule);
				Lines = new PXSelectExtension<DocumentLine>(Base.GLTranModuleBatNbr);
			}		

			protected override DocumentMapping GetDocumentMapping()
			{
				return new DocumentMapping(typeof(GLDocBatch))
				{
					HeaderTranPeriodID = typeof(GLDocBatch.tranPeriodID),
                    HeaderDocDate = typeof(GLDocBatch.dateEntered)
                };
			}

			protected override DocumentLineMapping GetDocumentLineMapping()
			{
				return new DocumentLineMapping(typeof(GLTranDoc));
			}
		}
		#endregion

		#region Internal Type Definition
		[Serializable]
		public class RefDocKey : IBqlTable
		{
			#region BranchID
			public abstract class branchID : IBqlField { }

			[Branch(IsKey = true)]
			public virtual int? BranchID { get; set; }
			#endregion
			#region TranModule
			public abstract class tranModule : PX.Data.BQL.BqlString.Field<tranModule> { }
			protected String _TranModule;
			[PXDBString(2, IsFixed = true, IsKey = true)]
			public virtual String TranModule
			{
				get
				{
					return this._TranModule;
				}
				set
				{
					this._TranModule = value;
				}
			}
			#endregion
			#region TranType
			public abstract class tranType : PX.Data.BQL.BqlString.Field<tranType> { }
			protected String _TranType;
			[PXDBString(3, IsFixed = true, IsKey = true)]
			public virtual String TranType
			{
				get
				{
					return this._TranType;
				}
				set
				{
					this._TranType = value;
				}
			}
			#endregion
			#region RefNbr
			public abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }
			protected String _RefNbr;
			[PXDBString(15, IsUnicode = true, IsKey = true)]
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

			public virtual void Copy(GLTranDoc src)
			{
				this.BranchID = src.BranchID;
				this.TranModule = src.TranModule;
				this.TranType = src.TranType;
				this.RefNbr = src.RefNbr;
			}
		}

		[Serializable]
		[PXHidden]
		public class GLTranDocAP : GLTranDoc
		{
			#region Module
			public new abstract class module : PX.Data.BQL.BqlString.Field<module> { }

			#endregion
			#region BatchNbr
			public new abstract class batchNbr : PX.Data.BQL.BqlString.Field<batchNbr> { }
			[PXDBString(15, IsUnicode = true, IsKey = true)]
			[PXUIField(DisplayName = "Batch Number", Visibility = PXUIVisibility.Visible, Visible = false)]
			public override string BatchNbr
				{
				get;
				set;
			}
			#endregion
			#region LineNbr
			public new abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }
			#endregion
			#region TranModule
			public new abstract class tranModule : PX.Data.BQL.BqlString.Field<tranModule> { }
			#endregion
			#region TranType
			public new abstract class tranType : PX.Data.BQL.BqlString.Field<tranType> { }
			#endregion
			#region RefNbr
			public new abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }

			#endregion
			#region BAccountID
			public new abstract class bAccountID : PX.Data.BQL.BqlInt.Field<bAccountID> { }
			[PXDBInt()]
			[PXVendorCustomerSelector(typeof(GLTranDoc.tranModule), typeof(GLTranDoc.curyID), CacheGlobal = true)]
			[PXUIField(DisplayName = "Vendor", Enabled = true, Visible = true)]
			public override int? BAccountID
			{
				get;
				set;
			}
			#endregion
			#region BranchID
			public new abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
			#endregion
			#region CuryInfoID
			public new abstract class curyInfoID : PX.Data.BQL.BqlLong.Field<curyInfoID> { }
			#endregion
			#region TranDate
			public new abstract class tranDate : PX.Data.BQL.BqlDateTime.Field<tranDate> { }
			#endregion
			#region FinPeriodID
			public new abstract class finPeriodID : PX.Data.BQL.BqlString.Field<finPeriodID> { }

			#endregion
			#region TranPeriodID
			public new abstract class tranPeriodID : PX.Data.BQL.BqlString.Field<tranPeriodID> { }
			#endregion
			#region CuryUnappliedBal
			public new abstract class curyUnappliedBal : PX.Data.BQL.BqlDecimal.Field<curyUnappliedBal> { }
			#endregion
			#region UnappliedBal
			public new abstract class unappliedBal : PX.Data.BQL.BqlDecimal.Field<unappliedBal> { }
			#endregion
			#region CuryApplAmt
			public new abstract class curyApplAmt : PX.Data.BQL.BqlDecimal.Field<curyApplAmt> { }
			#endregion
			#region ApplAmt
			public new abstract class applAmt : PX.Data.BQL.BqlDecimal.Field<applAmt> { }
			#endregion
			#region TaxCategoryID
			public new abstract class taxCategoryID : PX.Data.BQL.BqlString.Field<taxCategoryID> { }

			[PXDBString(10, IsUnicode = true)]
			[PXUIField(DisplayName = "Tax Category", Visibility = PXUIVisibility.Visible)]
			public override string TaxCategoryID
			{
				get;
				set;
			}
			#endregion
		}

		[Serializable]
		[PXHidden]
		public class GLTranDocAR : GLTranDoc
		{
			#region Module
			public new abstract class module : PX.Data.BQL.BqlString.Field<module> { }

			#endregion
			#region BatchNbr
			public new abstract class batchNbr : PX.Data.BQL.BqlString.Field<batchNbr> { }
			[PXDBString(15, IsUnicode = true, IsKey = true)]
			[PXUIField(DisplayName = "Batch Number", Visibility = PXUIVisibility.Visible, Visible = false)]
			public override string BatchNbr
			{
				get;
				set;
			}
			#endregion
			#region LineNbr
			public new abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }
			#endregion
			#region TranModule
			public new abstract class tranModule : PX.Data.BQL.BqlString.Field<tranModule> { }
			#endregion
			#region TranType
			public new abstract class tranType : PX.Data.BQL.BqlString.Field<tranType> { }
			#endregion
			#region RefNbr
			public new abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }

			#endregion
			#region BAccountID
			public new abstract class bAccountID : PX.Data.BQL.BqlInt.Field<bAccountID> { }
			[PXDBInt]
			[PXVendorCustomerSelector(typeof(GLTranDoc.tranModule), typeof(GLTranDoc.curyID), CacheGlobal = true)]
			[PXUIField(DisplayName = "Customer", Enabled = true, Visible = true)]
			public override int? BAccountID
			{
				get;
				set;
			}
			#endregion
			#region BranchID
			public new abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
			#endregion
			#region CuryInfoID
			public new abstract class curyInfoID : PX.Data.BQL.BqlLong.Field<curyInfoID> { }
			#endregion
			#region TranDate
			public new abstract class tranDate : PX.Data.BQL.BqlDateTime.Field<tranDate> { }
			#endregion
			#region FinPeriodID
			public new abstract class finPeriodID : PX.Data.BQL.BqlString.Field<finPeriodID> { }

			#endregion
			#region TranPeriodID
			public new abstract class tranPeriodID : PX.Data.BQL.BqlString.Field<tranPeriodID> { }
			#endregion
			#region CuryUnappliedBal
			public new abstract class curyUnappliedBal : PX.Data.BQL.BqlDecimal.Field<curyUnappliedBal> { }
			#endregion
			#region UnappliedBal
			public new abstract class unappliedBal : PX.Data.BQL.BqlDecimal.Field<unappliedBal> { }
			#endregion
			#region CuryApplAmt
			public new abstract class curyApplAmt : PX.Data.BQL.BqlDecimal.Field<curyApplAmt> { }
			#endregion
			#region ApplAmt
			public new abstract class applAmt : PX.Data.BQL.BqlDecimal.Field<applAmt> { }
			#endregion
			#region TaxCategoryID
			public new abstract class taxCategoryID : PX.Data.BQL.BqlString.Field<taxCategoryID> { }

			[PXDBString(10, IsUnicode = true)]
			[PXUIField(DisplayName = "Tax Category", Visibility = PXUIVisibility.Visible)]
			public override string TaxCategoryID
			{
				get;
				set;
			}
			#endregion
		}

		[Serializable]
		[PXProjection(typeof(Select<APAdjust>))]
		[PXHidden]
		public partial class APAdjust3 : IBqlTable
		{
			#region AdjgDocType
			public abstract class adjgDocType : PX.Data.BQL.BqlString.Field<adjgDocType> { }
			protected String _AdjgDocType;
			[PXDBString(3, IsKey = true, IsFixed = true, InputMask = "", BqlField = typeof(APAdjust.adjgDocType))]
			[PXUIField(DisplayName = "AdjgDocType", Visibility = PXUIVisibility.Visible, Visible = false)]
			public virtual String AdjgDocType
			{
				get
				{
					return this._AdjgDocType;
				}
				set
				{
					this._AdjgDocType = value;
				}
			}
			#endregion
			#region AdjgRefNbr
			public abstract class adjgRefNbr : PX.Data.BQL.BqlString.Field<adjgRefNbr> { }
			protected String _AdjgRefNbr;
			[PXDBString(15, IsUnicode = true, IsKey = true, BqlField = typeof(APAdjust.adjgRefNbr))]
			[PXUIField(DisplayName = "AdjgRefNbr", Visibility = PXUIVisibility.Visible, Visible = false)]
			public virtual String AdjgRefNbr
			{
				get
				{
					return this._AdjgRefNbr;
				}
				set
				{
					this._AdjgRefNbr = value;
				}
			}
			#endregion
			#region AdjdDocType
			public abstract class adjdDocType : PX.Data.BQL.BqlString.Field<adjdDocType> { }
			protected String _AdjdDocType;
			[PXDBString(3, IsKey = true, IsFixed = true, InputMask = "", BqlField = typeof(APAdjust.adjdDocType))]
			[PXDefault(APDocType.Invoice)]
			[PXUIField(DisplayName = "Document Type", Visibility = PXUIVisibility.Visible)]
			public virtual String AdjdDocType
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

			public abstract class adjdRefNbr : PX.Data.BQL.BqlString.Field<adjdRefNbr> { }
			protected String _AdjdRefNbr;
			[PXDBString(15, IsKey = true, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC", BqlField = typeof(APAdjust.adjdRefNbr))]
			[PXDefault()]
			[PXUIField(DisplayName = "Reference Nbr.", Visibility = PXUIVisibility.Visible)]
			public virtual String AdjdRefNbr
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
			#region Released
			public abstract class released : PX.Data.BQL.BqlBool.Field<released> { }
			protected Boolean? _Released;
			[PXDBBool(BqlField = typeof(APAdjust.released))]
			[PXDefault(false)]
			public virtual Boolean? Released
			{
				get
				{
					return this._Released;
				}
				set
				{
					this._Released = value;
				}
			}
			#endregion
		}

		[Serializable]
		[PXProjection(typeof(Select<ARAdjust>))]
		[PXHidden]
		public partial class ARAdjust3 : IBqlTable
		{
			#region AdjgDocType
			public abstract class adjgDocType : PX.Data.BQL.BqlString.Field<adjgDocType> { }
			protected String _AdjgDocType;
			[PXDBString(3, IsKey = true, IsFixed = true, InputMask = "", BqlField = typeof(ARAdjust.adjgDocType))]
			[PXUIField(DisplayName = "AdjgDocType", Visibility = PXUIVisibility.Visible, Visible = false)]
			public virtual String AdjgDocType
			{
				get
				{
					return this._AdjgDocType;
				}
				set
				{
					this._AdjgDocType = value;
				}
			}
			#endregion
			#region AdjgRefNbr
			public abstract class adjgRefNbr : PX.Data.BQL.BqlString.Field<adjgRefNbr> { }
			protected String _AdjgRefNbr;
			[PXDBString(15, IsUnicode = true, IsKey = true, BqlField = typeof(ARAdjust.adjgRefNbr))]
			[PXUIField(DisplayName = "AdjgRefNbr", Visibility = PXUIVisibility.Visible, Visible = false)]
			public virtual String AdjgRefNbr
			{
				get
				{
					return this._AdjgRefNbr;
				}
				set
				{
					this._AdjgRefNbr = value;
				}
			}
			#endregion
			#region AdjdDocType
			public abstract class adjdDocType : PX.Data.BQL.BqlString.Field<adjdDocType> { }
			protected String _AdjdDocType;
			[PXDBString(3, IsKey = true, IsFixed = true, InputMask = "", BqlField = typeof(ARAdjust.adjdDocType))]
			[PXDefault(ARDocType.Invoice)]
			[PXUIField(DisplayName = "Document Type", Visibility = PXUIVisibility.Visible)]
			public virtual String AdjdDocType
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

			public abstract class adjdRefNbr : PX.Data.BQL.BqlString.Field<adjdRefNbr> { }
			protected String _AdjdRefNbr;
			[PXDBString(15, IsKey = true, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC", BqlField = typeof(ARAdjust.adjdRefNbr))]
			[PXDefault()]
			[PXUIField(DisplayName = "Reference Nbr.", Visibility = PXUIVisibility.Visible)]
			public virtual String AdjdRefNbr
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
			#region Released
			public abstract class released : PX.Data.BQL.BqlBool.Field<released> { }
			protected Boolean? _Released;
			[PXDBBool(BqlField = typeof(ARAdjust.released))]
			[PXDefault(false)]
			public virtual Boolean? Released
			{
				get
				{
					return this._Released;
				}
				set
				{
					this._Released = value;
				}
			}
			#endregion
		}

		public class APAdjdRefNbr2Attribute : PXCustomSelectorAttribute
		{
			Type _searchType2;
			public APAdjdRefNbr2Attribute(Type SearchType1, Type SearchType2)
				: base(SearchType1,
				typeof(APRegister.refNbr),
				typeof(APRegister.docDate),
				typeof(APRegister.finPeriodID),
				typeof(APRegister.vendorLocationID),
				typeof(APRegister.curyID),
				typeof(APRegister.curyOrigDocAmt),
				typeof(APRegister.curyDocBal),
				typeof(APRegister.status),
				typeof(APAdjust.APInvoice.dueDate),
				typeof(APAdjust.APInvoice.invoiceNbr),
				typeof(APRegister.docDesc))
			{
				this._searchType2 = SearchType2;
			}

			protected virtual IEnumerable GetRecords(string aAdjdDocType)
			{
				PXView view = new PXView(this._Graph, !this._DirtyRead, this._Select);
				PXCache adjustments = this._Graph.Caches[typeof(APAdjust)];
				object current = null;
				foreach (object item in PXView.Currents)
				{
					if (item != null && (item.GetType() == this._CacheType || item.GetType().IsSubclassOf(this._CacheType)))
					{
						current = item;
						break;
					}
				}
				if (current == null)
				{
					current = adjustments.Current;
				}
				APAdjust row = current as APAdjust;
				string adjdDocType = aAdjdDocType;
				if (String.IsNullOrEmpty(adjdDocType))
				{
					if (row == null) yield break;
					adjdDocType = row.AdjdDocType;
				}
				foreach (object it in view.SelectMulti(adjdDocType))
				{
					APAdjust.APInvoice iDoc = ((PXResult)it).GetItem<APAdjust.APInvoice>();
					if (row != null) //In the case on null we can not detect if the found adjustment is identical to current - need an optimistic assumption
					{
						APAdjust application = null;
						foreach (APAdjust adj in adjustments.Inserted)
						{
							if (adj.AdjdDocType == iDoc.DocType && adj.AdjdRefNbr == iDoc.RefNbr
								&& !(Object.ReferenceEquals(adj, row) || (adj.AdjgDocType == row.AdjgDocType && adj.AdjgRefNbr == row.AdjgRefNbr)))
							{
								application = adj;
								break;
							}
						}
						if (application != null) continue;
					}
					yield return iDoc;
				}
				Search<GLTranDoc.refNbr> type = new Search<GLTranDoc.refNbr>();
				PXView view2 = new PXView(this._Graph, false, BqlCommand.CreateInstance(this._searchType2));
				foreach (object it1 in view2.SelectMulti(adjdDocType))
				{
					GLTranDoc iDoc = ((PXResult)it1).GetItem<GLTranDoc>();
					if (row != null) //In the case on null we can not detect if the found adjustment is identical to current - need an optimistic assumption
					{
						APAdjust application = null;
						foreach (APAdjust adj in adjustments.Inserted)
						{
							if (adj.AdjdDocType == iDoc.TranType && adj.AdjdRefNbr == iDoc.RefNbr
								&& !(Object.ReferenceEquals(adj, row) || (adj.AdjgDocType == row.AdjgDocType && adj.AdjgRefNbr == row.AdjgRefNbr)))
							{
								application = adj;
								break;
							}
						}
						if (application != null) continue;
					}
					APAdjust.APInvoice iDoc1 = new APAdjust.APInvoice();
					Copy(iDoc1, iDoc);
					yield return iDoc1;
				}
				yield break;
			}

			protected virtual void Copy(APAdjust.APInvoice aDest, GLTranDoc aSrc)
			{
				aDest.RefNbr = aSrc.RefNbr;
				aDest.CuryDocBal = aSrc.CuryApplAmt;
				aDest.DocBal = aSrc.ApplAmt;
				aDest.DocDesc = aSrc.TranDesc;
				aDest.DocDate = aSrc.TranDate;
				aDest.CuryID = aSrc.CuryID;
				aDest.CuryOrigDocAmt = aSrc.CuryDocTotal;
				aDest.InvoiceNbr = aSrc.ExtRefNbr;
				//aDest.CuryInfoID = aSrc.CuryInfoID; //Validate 
				aDest.FinPeriodID = aSrc.FinPeriodID;
				aDest.TranPeriodID = aSrc.TranPeriodID;
				aDest.VendorID = aSrc.BAccountID;
				aDest.VendorLocationID = aSrc.LocationID;
				aDest.Status = "D";
				//aDest.TaxZoneID = aSrc.TaxZoneID;
				//aDest.TermsID = aSrc.TermsID;
				aDest.DueDate = aSrc.DueDate;
				aDest.CuryOrigDiscAmt = aSrc.CuryDiscAmt;
				aDest.CuryDocBal = aSrc.CuryUnappliedBal;
				aDest.DocBal = aSrc.UnappliedBal;
				//aDest.DiscDate = aSrc.DiscDate;
				bool isDirect = AP.APInvoiceType.DrCr(aSrc.TranType) == DrCr.Debit;
				aDest.APAccountID = isDirect ? aSrc.CreditAccountID : aSrc.DebitAccountID;
				aDest.APSubID = isDirect ? aSrc.CreditSubID : aSrc.DebitSubID;
				aDest.BranchID = aSrc.BranchID;
			}

			//public override void FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
			//{
			//    if (sender.Current == null)
			//    {
			//        e.Cancel = true;
			//    }
			//    else
			//    {
			//        base.FieldVerifying(sender, e);
			//    }
			//}
		}

		public class ARAdjdRefNbr2Attribute : PXCustomSelectorAttribute
		{
			Type _searchType2;
			public ARAdjdRefNbr2Attribute(Type SearchType1, Type SearchType2)
				: base(SearchType1,
				typeof(ARRegister.refNbr),
				typeof(ARRegister.docDate),
				typeof(ARRegister.finPeriodID),
				typeof(ARRegister.customerLocationID),
				typeof(ARRegister.curyID),
				typeof(ARRegister.curyOrigDocAmt),
				typeof(ARRegister.curyDocBal),
				typeof(ARRegister.status),
				typeof(ARAdjust.ARInvoice.dueDate),
				typeof(ARAdjust.ARInvoice.invoiceNbr),
				typeof(ARRegister.docDesc))
			{
				this._searchType2 = SearchType2;
			}

			protected virtual IEnumerable GetRecords(string aAdjdDocType)
			{
				PXView view = new PXView(this._Graph, !this._DirtyRead, this._Select);
				PXCache adjustments = this._Graph.Caches[this._CacheType];
				//Take current from the view - it contains a valid value more often.                
				object current = null;
				foreach (object item in PXView.Currents)
				{
					if (item != null && (item.GetType() == this._CacheType || item.GetType().IsSubclassOf(this._CacheType)))
					{
						current = item;
						break;
					}
				}
				if (current == null)
				{
					current = adjustments.Current;
				}
				ARAdjust row = current as ARAdjust;
				string adjdDocType = aAdjdDocType;
				if (String.IsNullOrEmpty(adjdDocType))
				{
					if (row == null) yield break;
					adjdDocType = row.AdjdDocType;
				}
				foreach (object it in view.SelectMulti(adjdDocType))
				{
					ARAdjust.ARInvoice iDoc = ((PXResult)it).GetItem<ARAdjust.ARInvoice>();
					ARAdjust application = null;
					if (row != null) //In the case on null we can not detect if the found adjustment is identical to current - need an optimistic assumption
					{
						foreach (ARAdjust adj in adjustments.Inserted)
						{
							if (adj.AdjdDocType == iDoc.DocType && adj.AdjdRefNbr == iDoc.RefNbr
								&& !(Object.ReferenceEquals(adj, row) || (adj.AdjgDocType == row.AdjgDocType && adj.AdjgRefNbr == row.AdjgRefNbr)))
							{
								application = adj;
								break;
							}
						}
					}
					if (application != null) continue;
					yield return iDoc;
				}
				Search<GLTranDoc.refNbr> type = new Search<GLTranDoc.refNbr>();
				PXView view2 = new PXView(this._Graph, false, BqlCommand.CreateInstance(this._searchType2));
				foreach (object it1 in view2.SelectMulti(adjdDocType))
				{
					GLTranDoc iDoc = ((PXResult)it1).GetItem<GLTranDoc>();
					if (row != null) //In the case on null we can not detect if the found adjustment is identical to current - need an optimistic assumption
					{
						ARAdjust application = null;
						foreach (ARAdjust adj in adjustments.Inserted)
						{
							if (adj.AdjdDocType == iDoc.TranType && adj.AdjdRefNbr == iDoc.RefNbr
								&& !(Object.ReferenceEquals(adj, row) || (adj.AdjgDocType == row.AdjgDocType && adj.AdjgRefNbr == row.AdjgRefNbr)))
							{
								application = adj;
								break;
							}
						}
						if (application != null) continue;
					}
					ARAdjust.ARInvoice iDoc1 = new ARAdjust.ARInvoice();
					Copy(iDoc1, iDoc);
					yield return iDoc1;
				}
				yield break;
			}

			protected virtual void Copy(ARAdjust.ARInvoice aDest, GLTranDoc aSrc)
			{
				aDest.RefNbr = aSrc.RefNbr;
				aDest.CuryDocBal = aSrc.CuryApplAmt;
				aDest.DocBal = aSrc.ApplAmt;
				aDest.DocDesc = aSrc.TranDesc;
				aDest.DocDate = aSrc.TranDate;
				aDest.CuryID = aSrc.CuryID;
				aDest.CuryOrigDocAmt = aSrc.CuryDocTotal;
				aDest.InvoiceNbr = aSrc.ExtRefNbr;
				//aDest.CuryInfoID = aSrc.CuryInfoID; //Validate 
				aDest.FinPeriodID = aSrc.FinPeriodID;
				aDest.TranPeriodID = aSrc.TranPeriodID;
				aDest.CustomerID = aSrc.BAccountID;
				aDest.CustomerLocationID = aSrc.LocationID;
				aDest.Status = "D";
				//aDest.TaxZoneID = aSrc.TaxZoneID;
				//aDest.TermsID = aSrc.TermsID;
				aDest.DueDate = aSrc.DueDate;
				aDest.CuryOrigDiscAmt = aSrc.CuryDiscAmt;
				aDest.CuryDocBal = aSrc.CuryUnappliedBal;
				aDest.DocBal = aSrc.UnappliedBal;
				//aDest.DiscDate = aSrc.DiscDate;
				bool isDirect = AR.ARInvoiceType.DrCr(aSrc.TranType) == DrCr.Credit;
				aDest.ARAccountID = isDirect ? aSrc.DebitAccountID : aSrc.CreditAccountID;
				aDest.ARSubID = isDirect ? aSrc.DebitSubID : aSrc.CreditSubID;
				aDest.BranchID = aSrc.BranchID;
			}
		}

		private class PXManualNumberingException: PXException
		{
			internal PXManualNumberingException()
				:base(Messages.ManualNumberingError)
			{
			}
			internal PXManualNumberingException(string message)
				:base(message)
			{
			}
			public PXManualNumberingException(SerializationInfo info, StreamingContext context)
			: base(info, context)
			{
			}
		}
		#endregion

		#region Cache Attached Events
		#region BatchNbr
		[PXDBString(2, IsKey = true, IsFixed = true)]
		[PXUIField(DisplayName = "Module", Visible = true)]
		[BatchModule.List()]
		protected virtual void GLTran_Module_CacheAttached(PXCache sender)
		{
		}

		[PXDBString(15, IsUnicode = true, IsKey = true, InputMask = ">CCCCCCCCCCCCCCC")]
		[PXSelector(typeof(Search<GLDocBatch.batchNbr, Where<GLDocBatch.module, Equal<Current<GLTran.module>>>, OrderBy<Desc<GLDocBatch.batchNbr>>>), Filterable = true)]

		[PXUIField(DisplayName = "Batch Number", Visible = true)]
		protected virtual void GLTran_BatchNbr_CacheAttached(PXCache sender)
		{
		}
		#endregion

		#region APRegister Override
		#region DocType

		[PXDBString(3, IsKey = true, IsFixed = true)]
		[PXDefault()]
		[APDocType.List()]
		[PXUIField(DisplayName = "Type", Visibility = PXUIVisibility.SelectorVisible, Enabled = true, TabOrder = 0)]
		protected virtual void APRegister_DocType_CacheAttached(PXCache sender)
		{
		}
		#endregion
		#region RefNbr

		[PXDBString(15, IsUnicode = true, IsKey = true, InputMask = "")]
		[PXDefault()]
		[PXUIField(DisplayName = "Ref. Nbr.")]
		[APDocNumbering()]
		[PXParent(typeof(Select<GLTranDoc,
							Where<GLTranDoc.tranType, Equal<Current<APRegister.docType>>,
							And<GLTranDoc.refNbr, Equal<Current<APRegister.refNbr>>,
							And<GLTranDoc.tranModule, Equal<GL.BatchModule.moduleAP>,
							And<GLTranDoc.parentLineNbr, IsNull>>>>>))]
		protected virtual void APRegister_RefNbr_CacheAttached(PXCache sender)
		{
		}

		#endregion
		#region OrigModule
		[PXDBString(2, IsFixed = true)]
		[PXDefault(GL.BatchModule.GL)]
		[PXUIField(DisplayName = "Source", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		[GL.BatchModule.FullList()]
		protected virtual void APRegister_OrigModule_CacheAttached(PXCache sender)
		{
		}
		#endregion
		#region CuryID

		[PXDBString(5, IsUnicode = true, InputMask = ">LLLLL")]
		[PXUIField(DisplayName = "Currency", Visibility = PXUIVisibility.SelectorVisible)]
		[PXDefault(typeof(Search<Company.baseCuryID>))]
		[PXSelector(typeof(Currency.curyID))]
		protected virtual void APRegister_CuryID_CacheAttached(PXCache sender)
		{
		}
		#endregion
		#region APAccountID
		[PXDefault(0)]
		[PXDBInt()]
		protected virtual void APRegister_APAccountID_CacheAttached(PXCache sender)
		{
		}
		#endregion
		#region APSubID

		[PXDefault(0)]
		[PXDBInt()]
		protected virtual void APRegister_APSubID_CacheAttached(PXCache sender)
		{
		}
		#endregion
		#region Hold

		[PXDefault(true)]
		[PXDBBool()]
		protected virtual void APRegister_Hold_CacheAttached(PXCache sender)
		{
		}
		#endregion
		#region Tstamp
		//This override is needed to correctly delete not saved rows
		[PXDBTimestamp(RecordComesFirst = true)]
		protected virtual void APRegister_tstamp_CacheAttached(PXCache sender) { }
		#endregion

		public class APDocNumberingAttribute : AutoNumberAttribute
		{
			public APDocNumberingAttribute()
				: base(typeof(AP.APRegister.docType), typeof(AP.APRegister.docDate),
					new string[] { APDocType.Invoice, APDocType.CreditAdj, APDocType.DebitAdj, AP.APDocType.QuickCheck, AP.APDocType.VoidQuickCheck, 
										APDocType.Check, APDocType.Prepayment, APDocType.Refund, APDocType.VoidCheck },
					new Type[] { typeof(APSetup.invoiceNumberingID), typeof(APSetup.creditAdjNumberingID), typeof(APSetup.debitAdjNumberingID), typeof(APSetup.checkNumberingID), null, 
										typeof(APSetup.checkNumberingID), typeof(APSetup.checkNumberingID), typeof(APSetup.checkNumberingID), null })
			{ }
		}
		#endregion

		#region ARRegister Override
		#region DocType

		[PXDBString(3, IsKey = true, IsFixed = true)]
		[PXDefault()]
		[ARDocType.List()]
		[PXUIField(DisplayName = "Type", Visibility = PXUIVisibility.SelectorVisible, Enabled = true, TabOrder = 0)]
		protected virtual void ARRegister_DocType_CacheAttached(PXCache sender)
		{
		}
		#endregion
		#region RefNbr

		[PXDBString(15, IsUnicode = true, IsKey = true, InputMask = "")]
		[PXDefault()]
		[PXUIField(DisplayName = "Ref. Nbr.")]
		[ARDocNumbering()]
		[PXParent(typeof(Select<GLTranDoc,
							Where<GLTranDoc.tranType, Equal<Current<ARRegister.docType>>,
							And<GLTranDoc.refNbr, Equal<Current<ARRegister.refNbr>>,
							And<GLTranDoc.tranModule, Equal<GL.BatchModule.moduleAR>,
							And<GLTranDoc.parentLineNbr, IsNull>>>>>))]
		protected virtual void ARRegister_RefNbr_CacheAttached(PXCache sender)
		{
		}

		#endregion
		#region OrigModule
		[PXDBString(2, IsFixed = true)]
		[PXDefault(GL.BatchModule.GL)]
		[PXUIField(DisplayName = "Source", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		[GL.BatchModule.FullList()]
		protected virtual void ARRegister_OrigModule_CacheAttached(PXCache sender)
		{
		}
		#endregion

		#region CuryID

		[PXDBString(5, IsUnicode = true, InputMask = ">LLLLL")]
		[PXUIField(DisplayName = "Currency", Visibility = PXUIVisibility.SelectorVisible)]
		[PXDefault(typeof(Search<Company.baseCuryID>))]
		[PXSelector(typeof(Currency.curyID))]
		protected virtual void ARRegister_CuryID_CacheAttached(PXCache sender)
		{
		}
		#endregion
		#region ARAccountID
		[PXDefault(0)]
		[PXDBInt()]
		protected virtual void ARRegister_ARAccountID_CacheAttached(PXCache sender)
		{
		}
		#endregion
		#region ARSubID

		[PXDefault(0)]
		[PXDBInt()]
		protected virtual void ARRegister_ARSubID_CacheAttached(PXCache sender)
		{
		}
		#endregion
		#region Hold

		[PXDefault(true)]
		[PXDBBool()]
		protected virtual void ARRegister_Hold_CacheAttached(PXCache sender)
		{
		}
		#endregion
		#region Tstamp
		//This override is needed to correctly delete not saved rows
		[PXDBTimestamp(RecordComesFirst = true)]
		protected virtual void ARRegister_tstamp_CacheAttached(PXCache sender) { }
		#endregion

		public class ARDocNumberingAttribute : AutoNumberAttribute
		{
			public ARDocNumberingAttribute()
				: base(typeof(ARRegister.docType), typeof(ARRegister.docDate),
					 new string[] { ARDocType.Invoice, ARDocType.DebitMemo, ARDocType.CreditMemo, ARDocType.FinCharge, ARDocType.SmallCreditWO, ARDocType.CashSale, ARDocType.CashReturn, 
										ARDocType.Payment,  ARDocType.Prepayment, ARDocType.Refund, ARDocType.VoidPayment, ARDocType.SmallBalanceWO },
					new Type[] { typeof(ARSetup.invoiceNumberingID), typeof(ARSetup.debitAdjNumberingID), typeof(ARSetup.creditAdjNumberingID), typeof(ARSetup.finChargeNumberingID), null, typeof(ARSetup.paymentNumberingID), typeof(ARSetup.paymentNumberingID),
							typeof(ARSetup.paymentNumberingID), typeof(ARSetup.paymentNumberingID), typeof(ARSetup.paymentNumberingID), null, null })
			{ }
		}
		#endregion

		#region CAAdj
		#region AdjRefNbr

		[PXDBString(15, IsUnicode = true, IsKey = true, InputMask = ">CCCCCCCCCCCCCCC")]
		[PXDefault()]
		[PXUIField(DisplayName = "Reference Nbr.", Visibility = PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(Search<CAAdj.adjRefNbr>))]
		[AutoNumber(typeof(CASetup.registerNumberingID), typeof(CAAdj.tranDate))]
		[PXParent(typeof(Select<GLTranDoc, Where<GLTranDoc.refNbr, Equal<Current<CAAdj.adjRefNbr>>,
									And<GLTranDoc.tranType, Equal<Current<CAAdj.adjTranType>>,
									And<GLTranDoc.tranModule, Equal<GL.BatchModule.moduleCA>,
									And<GLTranDoc.parentLineNbr, IsNull>>>>>))]
		protected virtual void CAAdj_AdjRefNbr_CacheAttached(PXCache sender)
		{
		}

		#endregion
		#region TranId
		[PXDBLong()]
		protected virtual void CAAdj_TranID_CacheAttached(PXCache sender)
		{
		}
		#endregion
		#region Draft
		[PXDefault(true)]
		[PXDBBool()]
		protected virtual void CAAdj_Draft_CacheAttached(PXCache sender)
		{
		}
		#endregion
		#region Hold
		[PXDefault(true)]
		[PXDBBool()]
		protected virtual void CAAdj_Hold_CacheAttached(PXCache sender)
		{
		}
		#endregion
		#region TStamp
		//This override is needed to correctly delete not saved and deleted rows
		[PXDBTimestamp(RecordComesFirst = true)]
		protected virtual void CAAdj_tstamp_CacheAttached(PXCache sender) { }
		#endregion
		#endregion

		#region Batch
		#region BatchNbr

		[PXDBString(15, IsUnicode = true, IsKey = true, InputMask = ">CCCCCCCCCCCCCCC")]
		[PXDefault()]
		[PXUIField(DisplayName = "Batch Number", Visibility = PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(Search<Batch.batchNbr, Where<Batch.module, Equal<Current<Batch.module>>>, OrderBy<Desc<Batch.batchNbr>>>), Filterable = true)]
		[BatchModule.Numbering()]
		[PXParent(typeof(Select<GLTranDoc, Where<GLTranDoc.refNbr, Equal<Current<Batch.batchNbr>>,
									And<GLTranDoc.tranModule, Equal<GL.BatchModule.moduleGL>,
									And<GLTranDoc.parentLineNbr, IsNull>>>>))]
		protected virtual void Batch_BatchNbr_CacheAttached(PXCache sender)
		{
		}

		#endregion
		#region Draft
		[PXDefault(true)]
		[PXDBBool()]
		protected virtual void Batch_Draft_CacheAttached(PXCache sender)
		{
		}
		#endregion
		#region Hold
		[PXDefault(true)]
		[PXDBBool()]
		protected virtual void Batch_Hold_CacheAttached(PXCache sender)
		{
		}
		#endregion
		#region TStamp
		//This override is needed to correctly delete not saved and deleted rows
		[PXDBTimestamp(RecordComesFirst = true)]
		protected virtual void Batch_tstamp_CacheAttached(PXCache sender) { }
		#endregion
		#endregion

		#region APAdjust
		#region VendorID

		[Vendor(Visibility = PXUIVisibility.Visible, Visible = false)]
		[PXDefault(typeof(GLTranDocAP.bAccountID))]
		protected virtual void APAdjust_VendorID_CacheAttached(PXCache sender)
		{

		}
		#endregion

		#region AdjgDocType
		[PXDBString(3, IsKey = true, IsFixed = true, InputMask = "")]
		[PXDefault(typeof(GLTranDocAP.tranType))]
		[PXUIField(DisplayName = "AdjgDocType", Visibility = PXUIVisibility.Visible, Visible = false)]
		protected virtual void APAdjust_AdjgDocType_CacheAttached(PXCache sender)
		{
		}
		#endregion

		#region AdjgRefNbr
		[PXDBString(15, IsUnicode = true, IsKey = true)]
		[PXDefault(typeof(GLTranDocAP.refNbr))]
		[PXUIField(DisplayName = "AdjgRefNbr", Visibility = PXUIVisibility.Visible, Visible = false)]
		[PXParent(typeof(Select<GLTranDocAP, Where<GLTranDocAP.tranType, Equal<Current<APAdjust.adjgDocType>>, And<GLTranDocAP.refNbr, Equal<Current<APAdjust.adjgRefNbr>>, And<GLTranDocAP.tranModule, Equal<GL.BatchModule.moduleAP>>>>>))]
		protected virtual void APAdjust_AdjgRefNbr_CacheAttached(PXCache sender)
		{
		}
		#endregion

		#region AdjNbr
		[PXDBInt(IsKey = true)]
		[PXUIField(DisplayName = "Adjustment Nbr.", Visibility = PXUIVisibility.Visible, Visible = false)]
		[PXDefault(0)]
		protected virtual void APAdjust_AdjNbr_CacheAttached(PXCache sender)
		{

		}
		#endregion

		#region AdjdRefNbr

		[PXDBString(15, IsKey = true, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
		[PXDefault()]
		[PXUIField(DisplayName = "Reference Nbr.", Visibility = PXUIVisibility.Visible)]
		[APAdjdRefNbr2Attribute(typeof(Search2<APAdjust.APInvoice.refNbr,
			LeftJoin<APAdjust, On<APAdjust.adjdDocType, Equal<APAdjust.APInvoice.docType>, And<APAdjust.adjdRefNbr, Equal<APAdjust.APInvoice.refNbr>,
				And<APAdjust.released, Equal<False>,
				And<Where<APAdjust.adjgDocType, NotEqual<Current<GLTranDocAP.tranType>>, Or<APAdjust.adjgRefNbr, NotEqual<Current<GLTranDocAP.refNbr>>>>>>>>,
			LeftJoin<APPayment, On<APPayment.docType, Equal<APAdjust.APInvoice.docType>,
				And<APPayment.refNbr, Equal<APAdjust.APInvoice.refNbr>,
				And<Where<APPayment.docType, Equal<APDocType.prepayment>, Or<APPayment.docType, Equal<APDocType.debitAdj>>>>>>>>,
			Where<APAdjust.APInvoice.vendorID, Equal<Current<GLTranDocAP.bAccountID>>,
				And<APAdjust.APInvoice.docType, Equal<Optional<APAdjust.adjdDocType>>,
					And<APAdjust.APInvoice.released, Equal<True>,
					And<APAdjust.APInvoice.openDoc, Equal<True>,
					And<APAdjust.adjgRefNbr, IsNull,
				And2<Where<APPayment.refNbr, IsNull, And<Current<GLTranDocAP.tranType>, NotEqual<APDocType.refund>, Or<APPayment.refNbr, IsNotNull,
					And<Current<GLTranDocAP.tranType>, Equal<APDocType.refund>, Or<APPayment.docType, Equal<APDocType.debitAdj>,
					And<Current<GLTranDocAP.tranType>, Equal<APDocType.check>, Or<APPayment.docType, Equal<APDocType.debitAdj>,
					And<Current<GLTranDocAP.tranType>, Equal<APDocType.voidCheck>>>>>>>>>,
				And<Where<APAdjust.APInvoice.docDate, LessEqual<Current<GLTranDocAP.tranDate>>,
					And<APAdjust.APInvoice.finPeriodID, LessEqual<Current<GLTranDocAP.finPeriodID>>,
					Or<Current<APSetup.earlyChecks>, Equal<True>,
					And<Where<Current<GLTranDocAP.tranType>, Equal<APDocType.check>,
						Or<Current<GLTranDocAP.tranType>, Equal<APDocType.voidCheck>,
						Or<Current<GLTranDocAP.tranType>, Equal<APDocType.prepayment>>>>>>>>>>>>>>>>),
		typeof(Search2<GLTranDoc.refNbr,
					LeftJoin<APAdjust3, On<APAdjust3.adjdDocType, Equal<GLTranDoc.tranType>, And<APAdjust3.adjdRefNbr, Equal<GLTranDoc.refNbr>,
						And<APAdjust3.released, Equal<False>,
						And<Where<APAdjust3.adjgDocType, NotEqual<Current2<GLTranDocAP.tranType>>, Or<APAdjust3.adjgRefNbr, NotEqual<Current2<GLTranDocAP.refNbr>>>>>>>>>,
				Where<GLTranDoc.batchNbr, Equal<Current2<GLTranDocAP.batchNbr>>,
						And<GLTranDoc.parentLineNbr, IsNull,
						And<GLTranDoc.tranModule, Equal<GL.BatchModule.moduleAP>,
						And<GLTranDoc.bAccountID, Equal<Current2<GLTranDocAP.bAccountID>>,
						And<GLTranDoc.docCreated, Equal<False>,
						And<GLTranDoc.tranType, Equal<Optional<APAdjust.adjdDocType>>,
						And<APAdjust3.adjgRefNbr, IsNull>>>>>>>>))]
		[PXParent(typeof(Select<GLTranDoc, Where<GLTranDoc.tranModule, Equal<GL.BatchModule.moduleAP>, And<GLTranDoc.tranType, Equal<Current<APAdjust.adjdDocType>>,
									And<GLTranDoc.refNbr, Equal<Current<APAdjust.adjdRefNbr>>,
									And<GLTranDoc.parentLineNbr, IsNull>>>>>))]
		protected virtual void APAdjust_AdjdRefNbr_CacheAttached(PXCache sender)
		{

		}
		#endregion

		#region AdjgBranchID

		[Branch(typeof(GLTranDocAP.branchID))]
		protected virtual void APAdjust_AdjgBranchID_CacheAttached(PXCache sender) { }

		#endregion

		#region AdjgCuryInfoID
		[PXDBLong()]
		[CurrencyInfo(typeof(GLTranDocAP.curyInfoID), CuryIDField = "AdjgCuryID")]
		protected virtual void APAdjust_AdjgCuryInfoID_CacheAttached(PXCache sender) { }

		#endregion

		#region AdjgDocDate
		[PXDBDate()]
		[PXDefault(typeof(GLTranDocAP.tranDate))]
		protected virtual void APAdjust_AdjgDocDate_CacheAttached(PXCache sender) { }

		#endregion

		#region AdjgFinPeriodID

		[GL.FinPeriodID()]
		[PXDefault(typeof(GLTranDocAP.finPeriodID))]
		[PXUIField(DisplayName = "Application Period", Enabled = false)]
		protected virtual void APAdjust_AdjgFinPeriodID_CacheAttached(PXCache sender) { }

		#endregion
		#region AdjgTranPeriodID
		[GL.FinPeriodID()]
		[PXDefault(typeof(GLTranDocAP.tranPeriodID))]
		protected virtual void APAdjust_AdjgTranPeriodID_CacheAttached(PXCache sender) { }

		#endregion

		#region CuryAdjgAmt
		[PXDBCurrency(typeof(APAdjust.adjgCuryInfoID), typeof(APAdjust.adjAmt), BaseCalc = false)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Amount Paid", Visibility = PXUIVisibility.Visible)]
		[PXUnboundFormula(typeof(Mult<APAdjust.adjgBalSign, APAdjust.curyAdjgAmt>), typeof(SumCalc<GLTranDocAP.curyApplAmt>))]
		protected virtual void APAdjust_CuryAdjgAmt_CacheAttached(PXCache sender) { }

		#endregion

		#region CuryAdjdDiscAmt

		//[PXDBDecimal(4)]
		[PXDBCurrency(typeof(APAdjust.adjdCuryInfoID), typeof(APAdjust.adjDiscAmt))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUnboundFormula(typeof(Mult<APAdjust.adjdBalSign, APAdjust.curyAdjdDiscAmt>), typeof(SumCalc<GLTranDoc.curyDiscTaken>))]
		protected virtual void APAdjust_CuryAdjdDiscAmt_CacheAttached(PXCache sender) { }

		#endregion
		#region CuryAdjdWhTaxAmt

		//[PXDBDecimal(4)]
		[PXDBCurrency(typeof(APAdjust.adjdCuryInfoID), typeof(APAdjust.adjWhTaxAmt))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUnboundFormula(typeof(Mult<APAdjust.adjdBalSign, APAdjust.curyAdjdWhTaxAmt>), typeof(SumCalc<GLTranDoc.curyTaxWheld>))]
		protected virtual void APAdjust_CuryAdjdWhTaxAmt_CacheAttached(PXCache sender) { }

		#endregion
		#region CuryAdjdAmt

		//[PXDBCurrency(typeof(APAdjust.adjdCuryInfoID), typeof(APAdjust.adjdAmt))]
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUnboundFormula(typeof(Mult<APAdjust.adjdBalSign, APAdjust.curyAdjdAmt>), typeof(SumCalc<GLTranDoc.curyApplAmt>))]
		[PXFormula(null, typeof(CountCalc<GLTranDoc.applCount>))]
		protected virtual void APAdjust_CuryAdjdAmt_CacheAttached(PXCache sender) { }

		#endregion

		#endregion

		#region ARAdjust
		#region CustomerID

		[Customer(Visibility = PXUIVisibility.Visible, Visible = false)]
		[PXDefault(typeof(GLTranDocAR.bAccountID))]
		protected virtual void ARAdjust_CustomerID_CacheAttached(PXCache sender)
		{

		}
		#endregion

		#region AdjgDocType
		[PXDBString(3, IsKey = true, IsFixed = true, InputMask = "")]
		[PXDefault(typeof(GLTranDocAR.tranType))]
		[PXUIField(DisplayName = "AdjgDocType", Visibility = PXUIVisibility.Visible, Visible = false)]
		protected virtual void ARAdjust_AdjgDocType_CacheAttached(PXCache sender)
		{
		}
		#endregion

		#region AdjgRefNbr
		[PXDBString(15, IsUnicode = true, IsKey = true)]
		[PXDefault(typeof(GLTranDocAR.refNbr))]
		[PXUIField(DisplayName = "AdjgRefNbr", Visibility = PXUIVisibility.Visible, Visible = false)]
		[PXParent(typeof(Select<GLTranDocAR, Where<GLTranDocAR.tranType, Equal<Current<ARAdjust.adjgDocType>>, And<GLTranDocAR.refNbr, Equal<Current<ARAdjust.adjgRefNbr>>, And<GLTranDocAR.tranModule, Equal<GL.BatchModule.moduleAR>>>>>))]
		protected virtual void ARAdjust_AdjgRefNbr_CacheAttached(PXCache sender)
		{
		}
		#endregion

		#region AdjNbr
		[PXDBInt(IsKey = true)]
		[PXUIField(DisplayName = "Adjustment Nbr.", Visibility = PXUIVisibility.Visible, Visible = false)]
		[PXDefault(0)]
		protected virtual void ARAdjust_AdjNbr_CacheAttached(PXCache sender)
		{

		}
		#endregion

		#region AdjdRefNbr

		[PXDBString(15, IsKey = true, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
		[PXDefault()]
		[PXUIField(DisplayName = "Reference Nbr.", Visibility = PXUIVisibility.Visible)]
		[ARAdjdRefNbr2Attribute(typeof(Search2<ARAdjust.ARInvoice.refNbr,
			LeftJoin<ARAdjust, On<ARAdjust.adjdDocType, Equal<ARAdjust.ARInvoice.docType>, And<ARAdjust.adjdRefNbr, Equal<ARAdjust.ARInvoice.refNbr>,
				And<ARAdjust.released, Equal<False>,
				And<ARAdjust.voided, Equal<False>,
				And<Where<ARAdjust.adjgDocType,
					NotEqual<Current<GLTranDocAR.tranType>>, Or<ARAdjust.adjgRefNbr, NotEqual<Current<GLTranDocAR.refNbr>>>>>>>>>>,
			Where<ARAdjust.ARInvoice.customerID, Equal<Current<GLTranDocAR.bAccountID>>,
				And<ARAdjust.ARInvoice.docType, Equal<Optional<ARAdjust.adjdDocType>>,
					And<ARAdjust.ARInvoice.released, Equal<True>,
					And<ARAdjust.ARInvoice.openDoc, Equal<True>,
					And<ARAdjust.ARInvoice.docDate, LessEqual<Current<GLTranDocAR.tranDate>>,
					And<ARAdjust.ARInvoice.finPeriodID, LessEqual<Current<GLTranDocAR.finPeriodID>>,
					And<ARAdjust.adjgRefNbr, IsNull>>>>>>>>),
			typeof(Search2<GLTranDoc.refNbr,
					LeftJoin<ARAdjust3, On<ARAdjust3.adjdDocType, Equal<GLTranDoc.tranType>, And<ARAdjust3.adjdRefNbr, Equal<GLTranDoc.refNbr>,
						And<ARAdjust3.released, Equal<False>,
						And<Where<ARAdjust3.adjgDocType, NotEqual<Current2<GLTranDocAR.tranType>>, Or<ARAdjust3.adjgRefNbr, NotEqual<Current2<GLTranDocAR.refNbr>>>>>>>>>,
					Where<GLTranDoc.batchNbr, Equal<Current2<GLTranDocAR.batchNbr>>,
						And<GLTranDoc.parentLineNbr, IsNull,
						And<GLTranDoc.tranModule, Equal<GL.BatchModule.moduleAR>,
						And<GLTranDoc.bAccountID, Equal<Current2<GLTranDocAR.bAccountID>>,
						And<GLTranDoc.docCreated, Equal<False>,
						And<GLTranDoc.tranType, Equal<Optional<ARAdjust.adjdDocType>>,
						And<ARAdjust3.adjgRefNbr, IsNull>>>>>>>>), Filterable = true)]
		[PXParent(typeof(Select<GLTranDoc, Where<GLTranDoc.tranModule, Equal<GL.BatchModule.moduleAR>, And<GLTranDoc.tranType, Equal<Current<ARAdjust.adjdDocType>>,
									And<GLTranDoc.refNbr, Equal<Current<ARAdjust.adjdRefNbr>>,
									And<GLTranDoc.parentLineNbr, IsNull>>>>>))]
		protected virtual void ARAdjust_AdjdRefNbr_CacheAttached(PXCache sender)
		{

		}
		#endregion

		#region AdjgBranchID

		[Branch(typeof(GLTranDocAR.branchID))]
		protected virtual void ARAdjust_AdjgBranchID_CacheAttached(PXCache sender) { }

		#endregion

		#region AdjgCuryInfoID
		[PXDBLong()]
		[CurrencyInfo(typeof(GLTranDocAR.curyInfoID), CuryIDField = "AdjgCuryID")]
		protected virtual void ARAdjust_AdjgCuryInfoID_CacheAttached(PXCache sender) { }

		#endregion

		#region AdjgDocDate
		[PXDBDate()]
		[PXDefault(typeof(GLTranDocAR.tranDate))]
		protected virtual void ARAdjust_AdjgDocDate_CacheAttached(PXCache sender) { }

		#endregion

		#region AdjgFinPeriodID

		[GL.FinPeriodID()]
		[PXDefault(typeof(GLTranDocAR.finPeriodID))]
		[PXUIField(DisplayName = "Application Period", Enabled = false)]
		protected virtual void ARAdjust_AdjgFinPeriodID_CacheAttached(PXCache sender) { }

		#endregion
		#region AdjgTranPeriodID
		[GL.FinPeriodID()]
		[PXDefault(typeof(GLTranDocAR.tranPeriodID))]
		protected virtual void ARAdjust_AdjgTranPeriodID_CacheAttached(PXCache sender) { }

		#endregion

		#region CuryAdjgAmt

		[PXDBCurrency(typeof(ARAdjust.adjgCuryInfoID), typeof(ARAdjust.adjAmt), BaseCalc = false)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Amount Paid", Visibility = PXUIVisibility.Visible)]
		[PXUnboundFormula(typeof(Mult<ARAdjust.adjgBalSign, ARAdjust.curyAdjgAmt>), typeof(SumCalc<GLTranDocAR.curyApplAmt>))]
		protected virtual void ARAdjust_CuryAdjgAmt_CacheAttached(PXCache sender) { }

		#endregion

		#region CuryAdjdDiscAmt


		[PXDBCurrency(typeof(ARAdjust.adjdCuryInfoID), typeof(ARAdjust.adjDiscAmt))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUnboundFormula(typeof(Mult<ARAdjust.adjdBalSign, ARAdjust.curyAdjdDiscAmt>), typeof(SumCalc<GLTranDoc.curyDiscTaken>))]
		protected virtual void ARAdjust_CuryAdjdDiscAmt_CacheAttached(PXCache sender) { }

		#endregion
		#region CuryAdjdWhTaxAmt

		//[PXDBDecimal(4)]
		[PXDBCurrency(typeof(ARAdjust.adjdCuryInfoID), typeof(ARAdjust.adjWOAmt))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUnboundFormula(typeof(Mult<ARAdjust.adjdBalSign, ARAdjust.curyAdjdWOAmt>), typeof(SumCalc<GLTranDoc.curyTaxWheld>))]
		protected virtual void ARAdjust_CuryAdjdWOAmt_CacheAttached(PXCache sender) { }

		#endregion
		#region CuryAdjdAmt

		//[PXDBCurrency(typeof(ARAdjust.adjdCuryInfoID), typeof(ARAdjust.adjdAmt))]
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUnboundFormula(typeof(Mult<ARAdjust.adjdBalSign, ARAdjust.curyAdjdAmt>), typeof(SumCalc<GLTranDoc.curyApplAmt>))]
		[PXFormula(null, typeof(CountCalc<GLTranDoc.applCount>))]
		protected virtual void ARAdjust_CuryAdjdAmt_CacheAttached(PXCache sender) { }

		#endregion
		#endregion
		#endregion

		#region Ctor + Selects
		public JournalWithSubEntry()
		{
			var caadjCache = Caches[typeof(PX.Objects.CA.CAAdj)];

			GLSetup glSetup = this.glsetup.Current;
			APSetup apSetup = this.apsetup.Current;
			ARSetup arsetup = this.arsetup.Current;
			CASetup caSetup = this.casetup.Current;
			PXUIFieldAttribute.SetVisible<GLTranDoc.projectID>(GLTranModuleBatNbr.Cache, null, PM.ProjectAttribute.IsPMVisible( GL.BatchModule.GL));
			PXUIFieldAttribute.SetVisible<GLTranDoc.taskID>(GLTranModuleBatNbr.Cache, null, PM.ProjectAttribute.IsPMVisible( GL.BatchModule.GL));
			this.GLTransactions.Cache.AllowDelete = false;
			this.GLTransactions.Cache.AllowUpdate = false;
			this.GLTransactions.Cache.AllowInsert = false;
			FieldDefaulting.AddHandler<BAccountR.type>((sender, e) =>
			{
				GLTranDoc row = this.GLTranModuleBatNbr.Current;
				if (row != null)
				{
					if (row.TranModule == GL.BatchModule.AR)
					{
						e.NewValue = BAccountType.CustomerType;
					}
					else if (row.TranModule == GL.BatchModule.AP)
					{
						e.NewValue = BAccountType.VendorType;
					}
				}
			});
		}
		public ToggleCurrency<GLDocBatch> CurrencyView;

		public PXSelect<GLDocBatch, Where<GLDocBatch.module, Equal<Optional<GLDocBatch.module>>>> BatchModule;
		public PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Current<GLDocBatch.curyInfoID>>>> currencyinfo;
		[PXImport(typeof(GLDocBatch))]
		[PXCopyPasteHiddenFields(typeof(GLTranDoc.parentLineNbr), typeof(GLTranDoc.extRefNbr), FieldsToShowInSimpleImport =
			new Type[] { typeof(GLTranDoc.extRefNbr) })]
		public PXSelect<GLTranDoc, Where<GLTranDoc.module, Equal<Current<GLDocBatch.module>>, And<GLTranDoc.batchNbr, Equal<Current<GLDocBatch.batchNbr>>>>, OrderBy<Asc<GLTranDoc.groupTranID, Asc<GLTranDoc.lineNbr>>>> GLTranModuleBatNbr;

		public PXSelectReadonly<OrganizationFinPeriod,
			Where<OrganizationFinPeriod.finPeriodID, Equal<Current<GLDocBatch.finPeriodID>>,
			And<EqualToOrganizationOfBranch<OrganizationFinPeriod.organizationID, Current<GLDocBatch.branchID>>>>> finperiod;

		public PXSelect<Location, Where<Location.bAccountID, Equal<Required<Location.bAccountID>>,
									And<Location.locationID, Equal<Optional<Location.locationID>>>>> Location;

		public PXSelect<AR.Customer, Where<AR.Customer.bAccountID, Equal<Required<AR.Customer.bAccountID>>>> Customer;
		public PXSelect<AP.Vendor, Where<AP.Vendor.bAccountID, Equal<Required<AP.Vendor.bAccountID>>>> Vendor;

		[PXCopyPasteHiddenView]
		public PXSelectJoin<GLTran, InnerJoin<Batch, On<Batch.module, Equal<GLTran.module>,
									And<Batch.batchNbr, Equal<GLTran.batchNbr>>>>,
								Where<GLTran.module, Equal<Optional<GLDocBatch.module>>,
										And<GLTran.batchNbr, Equal<Optional<GLDocBatch.batchNbr>>>>> GLTransactions;

		public virtual IEnumerable gltransactions()
		{
			Dictionary<int, GLTran> result = new Dictionary<int, GLTran>();
			PXSelectBase selectAP = new PXSelectJoin<GLTran,
										InnerJoin<Batch, On<Batch.module, Equal<GLTran.module>,
											And<Batch.batchNbr, Equal<GLTran.batchNbr>>>,
										InnerJoin<AP.APRegister, On<AP.APRegister.batchNbr, Equal<GLTran.batchNbr>,
											And<GLTran.module, Equal<GL.BatchModule.moduleAP>>>,
										InnerJoin<GLTranDoc, On<AP.APRegister.docType, Equal<GLTranDoc.tranType>,
											And<GLTranDoc.refNbr, Equal<AP.APRegister.refNbr>,
											And<GLTranDoc.tranModule, Equal<GL.BatchModule.moduleAP>,
											And<GLTranDoc.parentLineNbr, IsNull>>>>>>>,
										Where<GLTranDoc.module, Equal<Optional<GLDocBatch.module>>,
											And<GLTranDoc.batchNbr, Equal<Optional<GLDocBatch.batchNbr>>>>,
											OrderBy<Asc<GLTranDoc.lineNbr, Asc<GLTran.lineNbr>>>>(this);
			PXSelectBase selectAR = new PXSelectJoin<GLTran,
										InnerJoin<Batch, On<Batch.module, Equal<GLTran.module>,
											And<Batch.batchNbr, Equal<GLTran.batchNbr>>>,
										InnerJoin<ARRegister, On<ARRegister.batchNbr, Equal<GLTran.batchNbr>,
											And<GLTran.module, Equal<GL.BatchModule.moduleAR>>>,
										InnerJoin<GLTranDoc, On<ARRegister.docType, Equal<GLTranDoc.tranType>,
											And<GLTranDoc.refNbr, Equal<ARRegister.refNbr>,
											And<GLTranDoc.tranModule, Equal<GL.BatchModule.moduleAR>,
											And<GLTranDoc.parentLineNbr, IsNull>>>>>>>,
										Where<GLTranDoc.module, Equal<Optional<GLDocBatch.module>>,
											And<GLTranDoc.batchNbr, Equal<Optional<GLDocBatch.batchNbr>>>>,
										OrderBy<Asc<GLTranDoc.lineNbr, Asc<GLTran.lineNbr>>>>(this);

			PXSelectBase selectCA = new PXSelectJoin<GLTran,
										InnerJoin<Batch, On<Batch.module, Equal<GLTran.module>,
											And<Batch.batchNbr, Equal<GLTran.batchNbr>>>,
										InnerJoin<CATran, On<Batch.module, Equal<GL.BatchModule.moduleCA>,
												And<Batch.batchNbr, Equal<CATran.batchNbr>>>,
										InnerJoin<CAAdj, On<CAAdj.tranID, Equal<CATran.tranID>>,
										InnerJoin<GLTranDoc, On<GLTranDoc.refNbr, Equal<CAAdj.adjRefNbr>,
											And<GLTranDoc.tranType, Equal<CAAdj.adjTranType>,
											And<GLTranDoc.tranModule, Equal<GL.BatchModule.moduleCA>>>>>>>>,
										Where<GLTranDoc.module, Equal<Optional<GLDocBatch.module>>,
											And<GLTranDoc.batchNbr, Equal<Optional<GLDocBatch.batchNbr>>,
											And<GLTranDoc.tranType, Equal<CA.CATranType.cAAdjustment>,
											And<GLTranDoc.parentLineNbr, IsNull>>>>,
											OrderBy<Asc<GLTranDoc.lineNbr, Asc<GLTran.lineNbr>>>>(this);

			PXSelectBase selectGL = new PXSelectJoin<GLTran,
										InnerJoin<Batch, On<Batch.module, Equal<GLTran.module>,
											And<Batch.batchNbr, Equal<GLTran.batchNbr>>>,
										InnerJoin<GLTranDoc, On<GLTranDoc.refNbr, Equal<Batch.batchNbr>,
											And<GLTranDoc.tranModule, Equal<Batch.module>>>>>,
										Where<GLTranDoc.module, Equal<Optional<GLDocBatch.module>>,
											And<GLTranDoc.batchNbr, Equal<Optional<GLDocBatch.batchNbr>>,
											And<GLTranDoc.tranModule, Equal<GL.BatchModule.moduleGL>,
											And<GLTranDoc.parentLineNbr, IsNull>>>>,
											OrderBy<Asc<GLTranDoc.lineNbr, Asc<GLTran.lineNbr>>>>(this);


			foreach (PXResult<GLTran, Batch, AP.APRegister, GLTranDoc> iRes in selectAP.View.SelectMulti())
			{
				GLTran iTran = iRes;
				Batch iBatch = iRes;
				AP.APRegister iRegister = (AP.APRegister)iRes;
				GLTranDoc iTranDoc = (GLTranDoc)iRes;
				if (result.ContainsKey(iTran.TranID.Value)) continue;
				result[iTran.TranID.Value] = iTran;
				PXResult<GLTran, Batch> iDoc = new PXResult<GLTran, Batch>(iTran, iBatch);
				yield return iDoc;
			}
			foreach (PXResult<GLTran, Batch, ARRegister, GLTranDoc> iRes in selectAR.View.SelectMulti())
			{
				GLTran iTran = iRes;
				Batch iBatch = iRes;
				ARRegister iRegister = (ARRegister)iRes;
				GLTranDoc iTranDoc = (GLTranDoc)iRes;
				if (result.ContainsKey(iTran.TranID.Value)) continue;
				result[iTran.TranID.Value] = iTran;
				PXResult<GLTran, Batch> iDoc = new PXResult<GLTran, Batch>(iTran, iBatch);
				yield return iDoc;
			}

			foreach (PXResult<GLTran, Batch, CATran, CAAdj, GLTranDoc> iRes in selectCA.View.SelectMulti())
			{
				GLTran iTran = iRes;
				Batch iBatch = iRes;
				CAAdj iRegister = (CAAdj)iRes;
				GLTranDoc iTranDoc = (GLTranDoc)iRes;
				if (result.ContainsKey(iTran.TranID.Value)) continue;
				result[iTran.TranID.Value] = iTran;
				PXResult<GLTran, Batch> iDoc = new PXResult<GLTran, Batch>(iTran, iBatch);
				yield return iDoc;
			}

			foreach (PXResult<GLTran, Batch, GLTranDoc> iRes in selectGL.View.SelectMulti())
			{
				GLTran iTran = iRes;
				Batch iBatch = iRes;
				GLTranDoc iTranDoc = (GLTranDoc)iRes;
				if (result.ContainsKey(iTran.TranID.Value)) continue;
				result[iTran.TranID.Value] = iTran;
				PXResult<GLTran, Batch> iDoc = new PXResult<GLTran, Batch>(iTran, iBatch);
				yield return iDoc;
			}
		}

		public PXSelectJoin<GLTax, InnerJoin<GLTranDoc, On<GLTranDoc.module, Equal<GLTax.module>,
								And<GLTranDoc.batchNbr, Equal<GLTax.batchNbr>,
								And<GLTranDoc.lineNbr, Equal<GLTax.lineNbr>>>>>,
							Where<GLTranDoc.module, Equal<Current<GLDocBatch.module>>,
								And<GLTranDoc.batchNbr, Equal<Current<GLDocBatch.batchNbr>>,
								And<GLTax.detailType, Equal<GLTaxDetailType.lineTax>>>>,
							OrderBy<Asc<GLTax.module, Asc<GLTax.batchNbr, Asc<GLTax.taxID>>>>> Tax_Rows;

		public PXSelectJoin<GLTaxTran, InnerJoin<Tax, On<Tax.taxID, Equal<GLTaxTran.taxID>>>,
					Where<GLTaxTran.module, Equal<Current<GLDocBatch.module>>,
						And<GLTaxTran.batchNbr, Equal<Current<GLDocBatch.batchNbr>>,
						And<GLTax.detailType, Equal<GLTaxDetailType.docTax>>>>> Taxes;

		public PXSelectJoin<GLTaxTran, InnerJoin<Tax, On<Tax.taxID, Equal<GLTaxTran.taxID>>>,
					Where<GLTaxTran.module, Equal<Current<GLTranDoc.module>>,
						And<GLTaxTran.batchNbr, Equal<Current<GLTranDoc.batchNbr>>,
						And<GLTaxTran.lineNbr, Equal<Current<GLTranDoc.lineNbr>>,
						And<GLTax.detailType, Equal<GLTaxDetailType.docTax>>>>>> CurrentDocTaxes;

		public PXSelect<GLTranDocAP, Where<GLTranDocAP.module, Equal<Current<GLDocBatch.module>>,
										And<GLTranDocAP.batchNbr, Equal<Current<GLDocBatch.batchNbr>>,
										And<GLTranDocAP.tranModule, Equal<GL.BatchModule.moduleAP>,
										And<Where<GLTranDocAP.tranType, Equal<AP.APDocType.check>,
											Or<GLTranDocAP.tranType, Equal<AP.APDocType.prepayment>,
											Or<GLTranDocAP.tranType, Equal<AP.APDocType.refund>>>>>>>>, OrderBy<Asc<GLTranDocAP.lineNbr>>> APPayments;

		public PXSelect<GLTranDocAR, Where<GLTranDocAR.module, Equal<Current<GLDocBatch.module>>,
										And<GLTranDocAR.batchNbr, Equal<Current<GLDocBatch.batchNbr>>,
										And<GLTranDocAR.tranModule, Equal<GL.BatchModule.moduleAR>,
										And<Where<GLTranDocAR.tranType, Equal<AR.ARDocType.payment>,
											Or<GLTranDocAR.tranType, Equal<AR.ARDocType.prepayment>,
											Or<GLTranDocAR.tranType, Equal<AR.ARDocType.refund>>>>>>>>, OrderBy<Asc<GLTranDocAR.lineNbr>>> ARPayments;

		[PXViewName(AP.Messages.APAdjust)]
		[PXCopyPasteHiddenView]
		public PXSelectJoin<APAdjust, LeftJoin<APInvoice, On<APInvoice.docType, Equal<APAdjust.adjdDocType>, And<APInvoice.refNbr, Equal<APAdjust.adjdRefNbr>>>>,
					Where<APAdjust.adjgDocType, Equal<Current<GLTranDocAP.tranType>>, And<APAdjust.adjgRefNbr, Equal<Current<GLTranDocAP.refNbr>>,
							And<APAdjust.adjNbr, Equal<int0>>>>> APAdjustments;

		[PXViewName(AR.Messages.ARAdjust)]
		[PXCopyPasteHiddenView]
		public PXSelectJoin<ARAdjust, LeftJoin<ARInvoice, On<ARInvoice.docType, Equal<ARAdjust.adjdDocType>, And<ARInvoice.refNbr, Equal<ARAdjust.adjdRefNbr>>>>,
					Where<ARAdjust.adjgDocType, Equal<Current<GLTranDocAR.tranType>>, And<ARAdjust.adjgRefNbr, Equal<Current<GLTranDocAR.refNbr>>,
							And<ARAdjust.adjNbr, Equal<int0>>>>> ARAdjustments;

		public PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Required<CurrencyInfo.curyInfoID>>>> CurrencyInfo_CuryInfoID;

		public virtual IEnumerable currentdoctaxes()
		{
			GLTranDoc row = this.GLTranModuleBatNbr.Current;
			if (row != null)
			{
				GLTranDoc parent = row;
				if (row.IsChildTran)
				{
					parent = PXSelect<GLTranDoc, Where<GLTranDoc.module, Equal<Required<GLTranDoc.module>>,
													And<GLTranDoc.batchNbr, Equal<Required<GLTranDoc.batchNbr>>,
														And<GLTranDoc.lineNbr, Equal<Required<GLTranDoc.lineNbr>>>>>>.Select(this, row.Module, row.BatchNbr, row.ParentLineNbr);
				}
				return PXSelectJoin<GLTaxTran, InnerJoin<Tax, On<Tax.taxID, Equal<GLTaxTran.taxID>>>,
					Where<GLTaxTran.module, Equal<Required<GLTranDoc.module>>,
						And<GLTaxTran.batchNbr, Equal<Required<GLTranDoc.batchNbr>>,
						And<GLTaxTran.lineNbr, Equal<Required<GLTranDoc.lineNbr>>,
						And<GLTax.detailType, Equal<GLTaxDetailType.docTax>>>>>>.Select(this, parent.Module, parent.BatchNbr, parent.LineNbr);
			}
			return null;
		}


		public PXSelect<APRegister, Where<APRegister.docType, Equal<Required<APRegister.docType>>,
										And<APRegister.refNbr, Equal<Required<APRegister.refNbr>>>>> apRegister;
		public PXSelect<ARRegister, Where<ARRegister.docType, Equal<Required<ARRegister.docType>>,
										And<ARRegister.refNbr, Equal<Required<ARRegister.refNbr>>>>> arRegister;

		public PXSelect<CAAdj, Where<CAAdj.adjTranType, Equal<Required<CAAdj.adjTranType>>,
										And<CAAdj.adjRefNbr, Equal<Required<CAAdj.adjRefNbr>>>>> caAdj;
		public PXSelect<Batch, Where<Batch.module, Equal<Required<Batch.module>>,
										And<Batch.batchNbr, Equal<Required<Batch.batchNbr>>>>> glBatch;

		public PXSelect<CashAccount, Where<CashAccount.cashAccountID, Equal<Required<CashAccount.cashAccountID>>>> cashAccount;
		public PXSelectJoin<CashAccount, LeftJoin<CashAccountETDetail, On<CashAccountETDetail.accountID, Equal<CashAccount.cashAccountID>,
											And<CashAccountETDetail.entryTypeID, Equal<Required<CAEntryType.entryTypeId>>>>>,
											Where<CashAccount.accountID, Equal<Required<CashAccount.accountID>>, And<CashAccount.branchID, Equal<Required<CashAccount.branchID>>,
											And<Where<Required<CAEntryType.entryTypeId>, IsNull,
											Or<CashAccountETDetail.accountID, IsNotNull>>>>>> cashAccountByAccountID;

		public PXSelectReadonly<APInvoice, Where<APInvoice.vendorID, Equal<Required<APInvoice.vendorID>>, And<APInvoice.docType, Equal<Required<APInvoice.docType>>, And<APInvoice.refNbr, Equal<Required<APInvoice.refNbr>>>>>> APInvoice_VendorID_DocType_RefNbr;
		public PXSelect<APPayment, Where<APPayment.vendorID, Equal<Required<APPayment.vendorID>>, And<APPayment.docType, Equal<Required<APPayment.docType>>, And<APPayment.refNbr, Equal<Required<APPayment.refNbr>>>>>> APPayment_VendorID_DocType_RefNbr;

		public PXSelectReadonly<ARInvoice, Where<ARInvoice.customerID, Equal<Required<ARInvoice.customerID>>, And<ARInvoice.docType, Equal<Required<ARInvoice.docType>>, And<ARInvoice.refNbr, Equal<Required<ARInvoice.refNbr>>>>>> ARInvoice_CustomerID_DocType_RefNbr;
		public PXSelect<ARPayment, Where<ARPayment.customerID, Equal<Required<ARPayment.customerID>>, And<ARPayment.docType, Equal<Required<ARPayment.docType>>, And<ARPayment.refNbr, Equal<Required<ARPayment.refNbr>>>>>> ARPayment_CustomerID_DocType_RefNbr;


		public PXFilter<RefDocKey> deletedKeys;
		#endregion

		#region Properties
		protected Ledger _Ledger;

		public PXSetup<GLSetup> glsetup;
		public APSetupNoMigrationMode apsetup;
		public ARSetupNoMigrationMode arsetup;
		public PXSetup<CASetup> casetup;

		public CMSetupSelect CMSetup;

		public CurrencyInfo currencyInfo
		{
			get
			{
				return currencyinfo.Select();
			}
		}
		public OrganizationFinPeriod FINPERIOD
		{
			get
			{
				return finperiod.Select();
			}
		}
		#endregion

		#region Buttons

		public PXAction<GLDocBatch> release;
		[PXUIField(DisplayName = Messages.Release, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXButton]
		public virtual IEnumerable Release(PXAdapter adapter)
		{
			PXCache cache = Caches[typeof(GLDocBatch)];
			List<GLDocBatch> list = new List<GLDocBatch>();
			foreach (GLDocBatch batch in adapter.Get())
			{
				if (batch.Status == GLDocBatchStatus.Balanced)
				{
					cache.Update(batch);
					list.Add(batch);
				}
			}
			if (list.Count == 0)
			{
				throw new PXException(Messages.BatchStatusInvalid);
			}
			Save.Press();
			if (list.Count > 0)
			{
				PXLongOperation.StartOperation(this, delegate() { ReleaseBatch(list); });
			}
			return list;
		}

		public PXAction<GLDocBatch> viewDocument;
		[PXUIField(DisplayName = Messages.ViewResultDocument, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXLookupButton()]
		public virtual IEnumerable ViewDocument(PXAdapter adapter)
		{

			if (this.GLTranModuleBatNbr.Current != null)
			{
				GLTranDoc tran = (GLTranDoc)this.GLTranModuleBatNbr.Current;

				if (tran.DocCreated == false)
				{
					throw new PXException(Messages.ERR_DocumentForThisRowIsNotCreatedYet);
				}

				IDocGraphCreator creator = null;
				switch (tran.TranModule)
				{
					case PX.Objects.GL.BatchModule.AP:
						creator = new APDocGraphCreator(); break;
					case PX.Objects.GL.BatchModule.AR:
						creator = new ARDocGraphCreator(); break;
					case PX.Objects.GL.BatchModule.CA:
						creator = new CADocGraphCreator(); break;
					case PX.Objects.GL.BatchModule.DR:
						creator = new DRDocGraphCreator(); break;
					case PX.Objects.GL.BatchModule.IN:
						creator = new INDocGraphCreator(); break;
					case PX.Objects.GL.BatchModule.PM:
						creator = new PMDocGraphCreator(); break;
					case PX.Objects.GL.BatchModule.GL:
						creator = new GLDocGraphCreator(); break;
				}
				if (creator != null)
				{
					PXGraph graph = creator.Create(tran.TranType, tran.RefNbr, tran.BAccountID);
					if (graph != null)
					{
						throw new PXRedirectRequiredException(graph, true, "ViewDocument") { Mode = PXBaseRedirectException.WindowMode.NewWindow };
					}
				}
				throw new PXException(Messages.InvalidReferenceNumberClicked);
			}

			return adapter.Get();
		}

		public PXAction<GLDocBatch> showTaxes;
		[PXUIField(DisplayName = Messages.ShowDocumentTaxes, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXLookupButton()]
		public virtual IEnumerable ShowTaxes(PXAdapter adapter)
		{
			this.CurrentDocTaxes.AskExt(true);
			return adapter.Get();
		}
		#endregion

		#region Functions

		public static void ReleaseBatch(List<GLDocBatch> list)
		{
			GLBatchDocRelease pg = PXGraph.CreateInstance<GLBatchDocRelease>();

			for (int i = 0; i < list.Count; i++)
			{
				pg.Clear(PXClearOption.PreserveData);

				GLDocBatch batch = list[i];
				pg.ReleaseBatchProc(batch, true);
			}
		}

		private void SetTransactionsChanged()
		{
			foreach (GLTranDoc tran in GLTranModuleBatNbr.Select())
			{
				Caches[typeof(GLTranDoc)].MarkUpdated(tran);
			}
		}

		private void SetTransactionsChanged<Field>()
				where Field : class, IBqlField
		{
			foreach (GLTranDoc tran in GLTranModuleBatNbr.Select())
			{
				GLTranModuleBatNbr.Cache.SetDefaultExt<Field>(tran);
				Caches[typeof(GLTranDoc)].MarkUpdated(tran);
			}
		}

		protected bool _ExceptionHandling = false;

		private string GetNumberingID(GLTranDoc doc)
		{
			PXCache setupCache = null;
			Type setupField = null;

			switch (doc.TranModule)
			{
				case GL.BatchModule.AP:
					setupField = APInvoiceType.NumberingAttribute.GetNumberingIDField(doc.TranType);
					setupCache = apsetup.Cache;
					break;
				case GL.BatchModule.AR:
					setupField = ARInvoiceType.NumberingAttribute.GetNumberingIDField(doc.TranType);
					setupCache = arsetup.Cache;
					break;
				case GL.BatchModule.CA:
					setupField = typeof(CASetup.registerNumberingID);
					setupCache = casetup.Cache;
					break;
				case GL.BatchModule.GL:
					setupField = PX.Objects.GL.BatchModule.NumberingAttribute.GetNumberingIDField(GL.BatchModule.GL);
					setupCache = glsetup.Cache;
					break;
			}

			if (setupField != null)
			{
				return (string)setupCache.GetValue(setupCache.Current, setupField.Name);
			}
			else
			{
				return null;
			}


		}

		protected virtual NumberingSequence GetNumberingSequence(GLTranDoc doc)
		{
			var numberingID = GetNumberingID(doc);
			if (String.IsNullOrEmpty(numberingID))
			{
				return null;
			}
			Numbering numbering = PXSelect<Numbering, Where<Numbering.numberingID, Equal<Required<Numbering.numberingID>>>>.Select(this, numberingID);
			if (numbering?.UserNumbering == true)
			{
				throw new PXManualNumberingException();
			}
			return AutoNumberAttribute.GetNumberingSequence(numberingID, doc.BranchID, doc.TranDate);
		}

		protected bool ShouldReuseRefNbrs
		{
			get { return glsetup.Current.ReuseRefNbrsInVouchers == true; }
		}

		protected virtual void AttemptReuseRecentlyDeletedKey(GLTranDoc source, NumberingSequence sequence)
		{
			if (ShouldReuseRefNbrs != true)
				return;

			RefDocKey chosenKey = null;
			if (sequence == null)
			{
				if (source.BranchID == null)
				{
					chosenKey = this.deletedKeys.Search<RefDocKey.branchID, RefDocKey.tranModule, RefDocKey.tranType>(source.BranchID, source.TranModule, source.TranType);
				}
				else
				{
				chosenKey = this.deletedKeys.Search<RefDocKey.tranModule, RefDocKey.tranType>(source.TranModule, source.TranType);
			}
			}
			else
			{
				foreach (RefDocKey key in deletedKeys.Select())
				{
					if (key.TranModule == source.TranModule && key.TranType == source.TranType && String.IsNullOrEmpty(key.RefNbr) == false
					&& (sequence.NBranchID == null || key.BranchID == source.BranchID)
					 && (String.IsNullOrEmpty(sequence.StartNbr) || String.Compare(key.RefNbr, sequence.StartNbr) >= 0)
					 && (String.IsNullOrEmpty(sequence.EndNbr) || String.Compare(key.RefNbr, sequence.EndNbr) <= 0))
					{
						chosenKey = key;
						break;
					}
				}
			}

			if (chosenKey != null && String.IsNullOrEmpty(chosenKey.RefNbr) == false)
			{
				source.RefNbr = chosenKey.RefNbr;
				this.deletedKeys.Delete(chosenKey);
				RemoveDeletedRecordFromCache(chosenKey);
			}
		}

		private void RemoveDeletedRecordFromCache(RefDocKey chosenKey)
		{
			switch (chosenKey.TranModule)
			{
				case (GL.BatchModule.AP):
					APRegister apPegister = new APRegister { DocType = chosenKey.TranType, RefNbr = chosenKey.RefNbr };
					if (this.Caches[typeof(APRegister)].GetStatus(apPegister) == PXEntryStatus.Deleted)
					{
						this.Caches[typeof(APRegister)].SetStatus(apPegister, PXEntryStatus.Notchanged);
					}
					break;
				case (GL.BatchModule.AR):
					ARRegister aRRegister = new ARRegister { DocType = chosenKey.TranType, RefNbr = chosenKey.RefNbr };
					if (this.Caches[typeof(ARRegister)].GetStatus(aRRegister) == PXEntryStatus.Deleted)
					{
						this.Caches[typeof(ARRegister)].SetStatus(aRRegister, PXEntryStatus.Notchanged);
					}
					break;
				case (GL.BatchModule.GL):
					Batch batch = new Batch { Module = chosenKey.TranModule, BatchNbr = chosenKey.RefNbr };
					if (this.Caches[typeof(Batch)].GetStatus(batch) == PXEntryStatus.Deleted)
					{
						this.Caches[typeof(Batch)].SetStatus(batch, PXEntryStatus.Notchanged);
					}
					break;
				case (GL.BatchModule.CA):
					CAAdj cAAdj = new CAAdj { DocType = chosenKey.TranType, RefNbr = chosenKey.RefNbr };
					if (this.Caches[typeof(CAAdj)].GetStatus(cAAdj) == PXEntryStatus.Deleted)
					{
						this.Caches[typeof(CAAdj)].SetStatus(cAAdj, PXEntryStatus.Notchanged);
					}
					break;
			}
		}

		protected virtual void CreateRefNbr(GLTranDoc source)
		{
			if (String.IsNullOrEmpty(source.TranType) == false && source.TranDate.HasValue && source.IsChildTran == false && String.IsNullOrEmpty(source.RefNbr))
			{
				if (source.TranModule == GL.BatchModule.AP
					&& source.BAccountID.HasValue && source.LocationID.HasValue)
				{
					NumberingSequence sequence = GetNumberingSequence(source);
					AttemptReuseRecentlyDeletedKey(source, sequence);

					if (String.IsNullOrEmpty(source.RefNbr))
					{
						PXCache cache = this.Caches[typeof(APRegister)];
						string reusedKey = ShouldReuseRefNbrs ?
							this.FindDeletedRefNbr(typeof(APRegister), typeof(APRegister.refNbr).Name, 
													typeof(APRegister.docType).Name, source.TranType,
													typeof(APRegister.branchID).Name, sequence) : null;
						APRegister refDoc = new APRegister();
						refDoc.DocType = source.TranType;
						refDoc.DocDate = source.TranDate;
						refDoc.VendorID = source.BAccountID;
						refDoc.VendorLocationID = source.LocationID;
						refDoc.BranchID = source.BranchID;
						refDoc.FinPeriodID = source.FinPeriodID;
						refDoc.CuryID = source.CuryID;
						refDoc = (AP.APRegister)cache.Insert(refDoc);
						bool reUseExisting = (String.IsNullOrEmpty(reusedKey) == false);
						if (reUseExisting)
						{
							refDoc.RefNbr = reusedKey;
							cache.Normalize();
							cache.SetStatus(refDoc, PXEntryStatus.Updated);
						}
						using (PXTransactionScope ts = new PXTransactionScope())
						{
							try
							{
								this._skipExtensionTables = true;
								if (reUseExisting == false)
								{
									cache.Persist(PXDBOperation.Insert);
									PXDatabase.Delete<APRegister>(new PXDataFieldRestrict<APRegister.docType>(refDoc.DocType), new PXDataFieldRestrict<APRegister.refNbr>(refDoc.RefNbr));
								}
								else
								{
									cache.Persist(PXDBOperation.Update);
									PXDatabase.Delete<APRegister>(new PXDataFieldRestrict<APRegister.docType>(refDoc.DocType), new PXDataFieldRestrict<APRegister.refNbr>(refDoc.RefNbr));
								}
							}
							finally { this._skipExtensionTables = false; }

							foreach (Type extension in cache.GetExtensionTables() ?? new List<Type> { })
							{
								PXDatabase.ForceDelete(extension, new PXDataFieldRestrict("DocType", refDoc.DocType), new PXDataFieldRestrict("RefNbr", refDoc.RefNbr));
							}

							ts.Complete();
							refDoc.tstamp = PXDatabase.SelectTimeStamp();
						}
						cache.Persisted(false);
						source.RefNbr = refDoc.RefNbr;
					}
				}

				if (source.TranModule == GL.BatchModule.AR
					&& source.BAccountID.HasValue && source.LocationID.HasValue)
				{
					NumberingSequence sequence = GetNumberingSequence(source);
					AttemptReuseRecentlyDeletedKey(source, sequence);

					if (String.IsNullOrEmpty(source.RefNbr))
					{
						PXCache cache = this.Caches[typeof(ARRegister)];
						string reusedKey = ShouldReuseRefNbrs ?
							this.FindDeletedRefNbr(typeof(ARRegister), typeof(ARRegister.refNbr).Name, 
													typeof(ARRegister.docType).Name, source.TranType,
													typeof(ARRegister.branchID).Name, sequence) : null;
						ARRegister refDoc = new ARRegister();
						refDoc.DocType = source.TranType;
						refDoc.DocDate = source.TranDate;

						refDoc.CustomerID = source.BAccountID;
						refDoc.CustomerLocationID = source.LocationID;
						refDoc.BranchID = source.BranchID;
						refDoc.FinPeriodID = source.FinPeriodID;
						refDoc.CuryID = source.CuryID;
						refDoc = (ARRegister)cache.Insert(refDoc);
						bool reUseExisting = (String.IsNullOrEmpty(reusedKey) == false);
						if (reUseExisting)
						{
							refDoc.RefNbr = reusedKey;
							cache.Normalize();
							cache.SetStatus(refDoc, PXEntryStatus.Updated);
						}						
						using (PXTransactionScope ts = new PXTransactionScope())
						{
							try
							{
								this._skipExtensionTables = true;
								if (reUseExisting == false)
								{
									cache.Persist(PXDBOperation.Insert);
									PXDatabase.Delete<ARRegister>(new PXDataFieldRestrict<ARRegister.docType>(refDoc.DocType), new PXDataFieldRestrict<ARRegister.refNbr>(refDoc.RefNbr));
								}
								else
								{
									cache.Persist(PXDBOperation.Update);
									PXDatabase.Delete<ARRegister>(new PXDataFieldRestrict<ARRegister.docType>(refDoc.DocType), new PXDataFieldRestrict<ARRegister.refNbr>(refDoc.RefNbr));
								}

							}
							finally { this._skipExtensionTables = false; }

							foreach (Type extension in cache.GetExtensionTables() ?? new List<Type> { })
							{
								PXDatabase.ForceDelete(extension, new PXDataFieldRestrict("DocType", refDoc.DocType), new PXDataFieldRestrict("RefNbr", refDoc.RefNbr));
							}

							ts.Complete();
							refDoc.tstamp = PXDatabase.SelectTimeStamp();
						}
						cache.Persisted(false);
						source.RefNbr = refDoc.RefNbr;
					}
				}

				if (source.TranModule == GL.BatchModule.CA
						&& String.IsNullOrEmpty(source.EntryTypeID) == false
						&& source.CashAccountID.HasValue)
				{
					NumberingSequence sequence = GetNumberingSequence(source);
					AttemptReuseRecentlyDeletedKey(source, sequence);

					if (String.IsNullOrEmpty(source.RefNbr))
					{
						PXCache cache = this.Caches[typeof(CAAdj)];
						string reusedKey = ShouldReuseRefNbrs ?
							this.FindDeletedRefNbr(typeof(CAAdj), typeof(CAAdj.adjRefNbr).Name, 
													typeof(CAAdj.adjTranType).Name, source.TranType,
													typeof(CAAdj.branchID).Name, sequence) : null;
						CAAdj refDoc = new CAAdj();
						refDoc.AdjTranType = source.TranType;
						refDoc.TranDate = source.TranDate;
						refDoc.FinPeriodID = source.FinPeriodID;
						refDoc.CashAccountID = source.CashAccountID;
						refDoc.EntryTypeID = source.EntryTypeID;
						refDoc.ExtRefNbr = "";
						refDoc.BranchID = source.BranchID;
						refDoc.CuryID = source.CuryID;
						refDoc = (CAAdj)cache.Insert(refDoc);
						bool reUseExisting = (String.IsNullOrEmpty(reusedKey) == false);
						if (reUseExisting)
						{
							refDoc.AdjRefNbr = reusedKey;
							cache.Normalize();
							cache.SetStatus(refDoc, PXEntryStatus.Updated);
						}
						using (PXTransactionScope ts = new PXTransactionScope())
						{
							try
							{
								this._skipExtensionTables = true;
								if (reUseExisting == false)
								{
									cache.Persist(PXDBOperation.Insert);
									PXDatabase.Delete<CAAdj>(new PXDataFieldRestrict<CAAdj.adjTranType>(refDoc.AdjTranType), new PXDataFieldRestrict<CAAdj.adjRefNbr>(refDoc.AdjRefNbr));
								}
								else
								{
									cache.Persist(PXDBOperation.Update);
									PXDatabase.Delete<CAAdj>(new PXDataFieldRestrict<CAAdj.adjTranType>(refDoc.AdjTranType), new PXDataFieldRestrict<CAAdj.adjRefNbr>(refDoc.AdjRefNbr));
								}
							}
							finally { this._skipExtensionTables = false; }

							foreach (Type extension in cache.GetExtensionTables() ?? new List<Type> { })
							{
								PXDatabase.ForceDelete(extension, new PXDataFieldRestrict("AdjTranType", refDoc.AdjTranType), new PXDataFieldRestrict("AdjRefNbr", refDoc.AdjRefNbr));
							}

							ts.Complete();
							refDoc.tstamp = PXDatabase.SelectTimeStamp();
						}
						cache.Persisted(false);
						source.RefNbr = refDoc.RefNbr;
					}

				}

				if (source.TranModule == GL.BatchModule.GL
					&& (source.DebitAccountID.HasValue || source.CreditAccountID.HasValue))
				{
					NumberingSequence sequence = GetNumberingSequence(source);
					AttemptReuseRecentlyDeletedKey(source, sequence);

					if (String.IsNullOrEmpty(source.RefNbr))
					{
						PXCache cache = this.Caches[typeof(Batch)];
						string reusedKey = ShouldReuseRefNbrs ?
							this.FindDeletedRefNbr(typeof(Batch), typeof(Batch.batchNbr).Name, 
													typeof(Batch.module).Name, source.TranModule,
													typeof(Batch.branchID).Name, sequence) : null;
						Batch refDoc = new Batch();
						refDoc.Module = source.TranModule;
						refDoc.DateEntered = source.TranDate;
						refDoc.BranchID = source.BranchID;
						refDoc.FinPeriodID = source.FinPeriodID;
						refDoc.CuryID = source.CuryID;
						refDoc = (Batch)cache.Insert(refDoc);
						bool reUseExisting = (String.IsNullOrEmpty(reusedKey) == false);
						if (reUseExisting)
						{
							refDoc.BatchNbr = reusedKey;
							cache.Normalize();
							cache.SetStatus(refDoc, PXEntryStatus.Updated);
						}
						using (PXTransactionScope ts = new PXTransactionScope())
						{
							try
							{
								this._skipExtensionTables = true;
								if (reUseExisting == false)
								{
									cache.Persist(PXDBOperation.Insert);
									PXDatabase.Delete<Batch>(new PXDataFieldRestrict<Batch.module>(refDoc.Module), new PXDataFieldRestrict<Batch.batchNbr>(refDoc.BatchNbr));
								}
								else
								{
									cache.Persist(PXDBOperation.Update);
									PXDatabase.Delete<Batch>(new PXDataFieldRestrict<Batch.module>(refDoc.Module), new PXDataFieldRestrict<Batch.batchNbr>(refDoc.BatchNbr));
								}
							}
							finally
							{
								this._skipExtensionTables = false;
							}

							foreach (Type extension in cache.GetExtensionTables() ?? new List<Type> { })
							{
								PXDatabase.ForceDelete(extension, new PXDataFieldRestrict("Module", refDoc.Module), new PXDataFieldRestrict("BatchNbr", refDoc.BatchNbr));
							}
							ts.Complete();
							refDoc.tstamp = PXDatabase.SelectTimeStamp();
						}
						cache.Persisted(false);
						source.RefNbr = refDoc.BatchNbr;
					}

				}
			}
		}

		private const int UnlockRefNbrThresholdHours = 72;
		private Type prevTable;
		private List<PXDataField> prevParams;

		[Obsolete("Will be removed in Acumatica 2019R1")]
		protected virtual string FindDeletedRefNbr(Type aTable, string aRefNbrField, string aTranTypeField, string aTranType, NumberingSequence sequence)
			=> FindDeletedRefNbr(aTable, aRefNbrField, aTranTypeField, aTranType, null, sequence);

		protected virtual string FindDeletedRefNbr(Type aTable, string aRefNbrField, string aTranTypeField, string aTranType, string aBranchIDField, NumberingSequence sequence)
		{
			string reuseKey = null;
			List<PXDataField> selectParams = new List<PXDataField>(8);
			selectParams.Add(new PXDataField(aRefNbrField));
			selectParams.Add(new PXDataField("CreatedByID"));
			selectParams.Add(new PXDataField("LastModifiedByID"));
			selectParams.Add(new PXDataField("CreatedDateTime"));
			selectParams.Add(new PXDataField("LastModifiedDateTime"));
			if (String.IsNullOrEmpty(aTranTypeField) == false)
			{
				selectParams.Add(new PXDataFieldValue(aTranTypeField, aTranType));
			}
			selectParams.Add(new PXDataFieldValue("DeletedDatabaseRecord", PXDbType.Bit, 1, 1));
			selectParams.Add(new PXDataFieldOrder("LastModifiedDateTime"));
			if (sequence != null && String.IsNullOrEmpty(sequence.StartNbr) == false)
			{
				selectParams.Add(new PXDataFieldValue(aRefNbrField, PXDbType.NVarChar, sequence.StartNbr.Length, sequence.StartNbr, PXComp.GE));
			}
			if (sequence != null && String.IsNullOrEmpty(sequence.EndNbr) == false)
			{
				selectParams.Add(new PXDataFieldValue(aRefNbrField, PXDbType.NVarChar, sequence.EndNbr.Length, sequence.EndNbr, PXComp.LE));
			}
			if (sequence?.NBranchID != null)
			{
				selectParams.Add(new PXDataFieldValue(aBranchIDField, PXDbType.Int, 4, sequence.NBranchID, PXComp.EQ));
			}

			using (PXReadDeletedScope rds = new PXReadDeletedScope())
			{
				DateTime now, nowLocal;
				PXDatabase.SelectDate(out nowLocal, out now);
				if (_importing && prevParams != null && prevTable != null &&
				prevTable == aTable && prevParams.Count == selectParams.Count)
				{
					bool sameParams = true;
					for (int i = 0; i < selectParams.Count; i++)
					{
						if (!prevParams[i].Expression.Equals(selectParams[i].Expression))
						{
							sameParams = false;
							break;
						}
					}
					if (sameParams)
					{
						return null;
					}
				}


				foreach (PXDataRecord record in PXDatabase.SelectMulti(aTable, selectParams.ToArray()))
				{
					var refNbr = record.GetString(0);
					var CreatedBy = record.GetGuid(1);
					var LastModifiedBy = record.GetGuid(2);
					var CreatedDateTime = record.GetDateTime(3);
					var LastModifiedDateTime = record.GetDateTime(4);
					if (String.IsNullOrEmpty(refNbr) == false)
					{
						var keyUser = LastModifiedDateTime > CreatedDateTime ? LastModifiedBy : CreatedBy;
						var keyTime = LastModifiedDateTime > CreatedDateTime ? LastModifiedDateTime : CreatedDateTime;
						TimeSpan sinceCreationModification = now - (keyTime ?? new DateTime());

						if (sinceCreationModification.TotalHours > UnlockRefNbrThresholdHours)
						{
							reuseKey = refNbr;
							break;
						}

						GLTranDoc createdAfter = PXSelectReadonly<GLTranDoc, Where<GLTranDoc.createdByID, Equal<Required<GLTranDoc.createdByID>>,
														And<GLTranDoc.createdDateTime, GreaterEqual<Required<GLTranDoc.createdDateTime>>>>>.Select(this, keyUser, keyTime);
						if (createdAfter != null && String.IsNullOrEmpty(createdAfter.RefNbr) == false)
						{
							reuseKey = refNbr;
							break;
						}
					}
				}
			}
			if (reuseKey == null && _importing)
			{
				prevTable = aTable;
				prevParams = selectParams;
			}
			return reuseKey;
		}

		protected virtual void RestoreRefNbr(GLTranDoc row)
		{
			if (String.IsNullOrEmpty(row.TranType) == false && row.IsChildTran == false && String.IsNullOrEmpty(row.RefNbr) == false)
			{
				if (row.TranModule == GL.BatchModule.GL)
				{
					PXCache cache = this.Caches[typeof(Batch)];
					Batch doc = doc = PXSelectReadonly<Batch, 
										Where<Batch.module, Equal<Required<Batch.module>>,
											And<Batch.batchNbr, Equal<Required<Batch.batchNbr>>>>>.Select(this, row.TranModule, row.RefNbr);
					if (doc == null)
					{
						using (PXReadDeletedScope sc = new PXReadDeletedScope())
						{
							doc = PXSelectReadonly<Batch, 
										Where<Batch.module, Equal<Required<Batch.module>>,
															And<Batch.batchNbr, Equal<Required<Batch.batchNbr>>>>>.Select(this, row.TranModule, row.RefNbr);
						}
						if (doc != null)
						{
							PXDatabase.Update<Batch>(new PXDataFieldAssign("DeletedDatabaseRecord", PXDbType.Bit, false),
													new PXDataFieldRestrict<Batch.module>(doc.Module),
													new PXDataFieldRestrict<Batch.batchNbr>(doc.BatchNbr),
													new PXDataFieldRestrict("DeletedDatabaseRecord", PXDbType.Bit, true));
							foreach (Batch iDoc in cache.Cached)
							{
								if (iDoc.Module == doc.Module && iDoc.BatchNbr == doc.BatchNbr)
								{
									iDoc.tstamp = PXDatabase.SelectTimeStamp();
									break;
								}
							}
						}
					}
				}

				if (row.TranModule == GL.BatchModule.AP)
				{
					PXCache cache = this.Caches[typeof(APRegister)];
					APRegister doc = null;
					doc = PXSelectReadonly<APRegister, Where<APRegister.docType, Equal<Required<APRegister.docType>>,
														And<APRegister.refNbr, Equal<Required<APRegister.refNbr>>>>>.Select(this, row.TranType, row.RefNbr);
					if (doc == null)
					{
						using (PXReadDeletedScope sc = new PXReadDeletedScope())
						{
							doc = PXSelectReadonly<APRegister, Where<APRegister.docType, Equal<Required<APRegister.docType>>,
															And<APRegister.refNbr, Equal<Required<APRegister.refNbr>>>>>.Select(this, row.TranType, row.RefNbr);
						}
						if (doc != null)
						{
							PXDatabase.Update<APRegister>(new PXDataFieldAssign("DeletedDatabaseRecord", PXDbType.Bit, false),
													new PXDataFieldRestrict<APRegister.docType>(doc.DocType),
													new PXDataFieldRestrict<APRegister.refNbr>(doc.RefNbr),
													new PXDataFieldRestrict("DeletedDatabaseRecord", PXDbType.Bit, true));
							foreach (APRegister iDoc in cache.Cached)
							{
								if (iDoc.DocType == doc.DocType && iDoc.RefNbr == doc.RefNbr)
								{
									iDoc.tstamp = PXDatabase.SelectTimeStamp();
									break;
								}
							}
						}
					}
				}

				if (row.TranModule == GL.BatchModule.AR)
				{
					PXCache cache = this.Caches[typeof(ARRegister)];
					ARRegister doc = null;
					doc = PXSelectReadonly<ARRegister, Where<ARRegister.docType, Equal<Required<ARRegister.docType>>,
														And<ARRegister.refNbr, Equal<Required<ARRegister.refNbr>>>>>.Select(this, row.TranType, row.RefNbr);
					if (doc == null)
					{
						using (PXReadDeletedScope sc = new PXReadDeletedScope())
						{
							doc = PXSelectReadonly<ARRegister, Where<ARRegister.docType, Equal<Required<ARRegister.docType>>,
															And<ARRegister.refNbr, Equal<Required<ARRegister.refNbr>>>>>.Select(this, row.TranType, row.RefNbr);
						}
						if (doc != null)
						{
							PXDatabase.Update<ARRegister>(new PXDataFieldAssign("DeletedDatabaseRecord", PXDbType.Bit, false),
													new PXDataFieldRestrict<ARRegister.docType>(doc.DocType),
													new PXDataFieldRestrict<ARRegister.refNbr>(doc.RefNbr),
													new PXDataFieldRestrict("DeletedDatabaseRecord", PXDbType.Bit, true));
							foreach (ARRegister iDoc in cache.Cached)
							{
								if (iDoc.DocType == doc.DocType && iDoc.RefNbr == doc.RefNbr)
								{
									iDoc.tstamp = PXDatabase.SelectTimeStamp();
									break;
								}
							}
						}
					}
				}
				if (row.TranModule == GL.BatchModule.CA)
				{
					PXCache cache = this.Caches[typeof(CAAdj)];
					CAAdj doc = PXSelectReadonly<CAAdj, Where<CAAdj.adjTranType, Equal<Required<CAAdj.adjTranType>>,
								And<CAAdj.adjRefNbr, Equal<Required<CAAdj.adjRefNbr>>>>>.Select(this, row.TranType, row.RefNbr);
					if (doc == null)
					{
						using (PXReadDeletedScope sc = new PXReadDeletedScope())
						{
							doc = PXSelectReadonly<CAAdj, Where<CAAdj.adjTranType, Equal<Required<CAAdj.adjTranType>>,
															And<CAAdj.adjRefNbr, Equal<Required<CAAdj.adjRefNbr>>>>>.Select(this, row.TranType, row.RefNbr);
						}
						if (doc != null)
						{
							PXDatabase.Update<CAAdj>(new PXDataFieldAssign("DeletedDatabaseRecord", PXDbType.Bit, false),
													new PXDataFieldRestrict<CAAdj.adjTranType>(doc.AdjTranType),
													new PXDataFieldRestrict<CAAdj.adjRefNbr>(doc.AdjRefNbr),
													new PXDataFieldRestrict("DeletedDatabaseRecord", PXDbType.Bit, true));
							foreach (CAAdj iDoc in cache.Cached)
							{
								if (iDoc.AdjTranType == doc.AdjTranType && iDoc.AdjRefNbr == doc.AdjRefNbr)
								{
									iDoc.tstamp = PXDatabase.SelectTimeStamp();
									break;
								}
							}
						}
					}
				}
			}
		}

		protected bool _skipExtensionTables = false;

		public override bool ProviderInsert(Type table, params PXDataFieldAssign[] pars)
		{

			if (this._skipExtensionTables)
			{
				if (typeof(PXCacheExtension).IsAssignableFrom(table) && table.BaseType.IsGenericType)
				{
					Type baseType = table.BaseType.GetGenericArguments()[table.BaseType.GetGenericArguments().Length - 1];
					bool isExtension = (typeof(APRegister).IsAssignableFrom(baseType) ||
						typeof(ARRegister).IsAssignableFrom(baseType) ||
						typeof(Batch).IsAssignableFrom(baseType) ||
						typeof(CAAdj).IsAssignableFrom(baseType));
					if (isExtension)
					{
						return true;
				}
			}
			}
			return base.ProviderInsert(table, pars);
		}

		public override bool ProviderUpdate(Type table, params PXDataFieldParam[] pars)
		{
			if (this._skipExtensionTables)
			{
				if (typeof(PXCacheExtension).IsAssignableFrom(table) && table.BaseType.IsGenericType)
				{
					Type baseType = table.BaseType.GetGenericArguments()[table.BaseType.GetGenericArguments().Length - 1];
					bool isExtension = (typeof(APRegister).IsAssignableFrom(baseType) ||
						typeof(ARRegister).IsAssignableFrom(baseType) ||
						typeof(Batch).IsAssignableFrom(baseType) ||
						typeof(CAAdj).IsAssignableFrom(baseType));
					if (isExtension)
					{
						return true;
				}
			}
			}
			return base.ProviderUpdate(table, pars);
		}

		#endregion
		#region Events

		#region CurrencyInfo Events
		protected virtual void CurrencyInfo_CuryID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			GLDocBatch doc = BatchModule.Current;
			if (doc != null && string.IsNullOrEmpty(doc.CuryID) == false)
			{
				e.NewValue = doc.CuryID;
				e.Cancel = true;
			}
			else
			{
				if (_Ledger != null)
				{
					e.NewValue = _Ledger.BaseCuryID;
					e.Cancel = true;
				}
			}
		}

		protected virtual void CurrencyInfo_BaseCuryID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (_Ledger != null)
			{
				e.NewValue = _Ledger.BaseCuryID;
				e.Cancel = true;
			}
		}

		protected virtual void CurrencyInfo_CuryRateTypeID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (PXAccess.FeatureInstalled<FeaturesSet.multicurrency>())
			{
				e.NewValue = CMSetup.Current.GLRateTypeDflt;
			}
		}

		protected virtual void CurrencyInfo_CuryEffDate_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			CurrencyInfo currencyInfo = (CurrencyInfo)e.Row;
			if (currencyInfo == null || BatchModule.Current == null || BatchModule.Current.DateEntered == null) return;
			e.NewValue = BatchModule.Current.DateEntered;
			e.Cancel = true;
		}

		protected virtual void CurrencyInfo_RowInserting(PXCache sender, PXRowInsertingEventArgs e)
		{
			CurrencyInfo info = (CurrencyInfo)e.Row;
			object CuryID = info.CuryID;
			object CuryRateTypeID = info.CuryRateTypeID;
			object CuryMultDiv = info.CuryMultDiv;
			object CuryRate = info.CuryRate;

			if (BatchModule.Current == null || BatchModule.Current.Module != GL.BatchModule.GL)
			{
				BqlCommand sel = new Select<CurrencyInfo, Where<CurrencyInfo.curyID, Equal<Required<CurrencyInfo.curyID>>, And<CurrencyInfo.curyRateTypeID, Equal<Required<CurrencyInfo.curyRateTypeID>>, And<CurrencyInfo.curyMultDiv, Equal<Required<CurrencyInfo.curyMultDiv>>, And<CurrencyInfo.curyRate, Equal<Required<CurrencyInfo.curyRate>>>>>>>();
				foreach (CurrencyInfo summ_info in sender.Cached)
				{
					if (sender.GetStatus(summ_info) != PXEntryStatus.Deleted &&
						sender.GetStatus(summ_info) != PXEntryStatus.InsertedDeleted)
					{
						if (sel.Meet(sender, summ_info, CuryID, CuryRateTypeID, CuryMultDiv, CuryRate))
						{
							sender.SetValue(e.Row, "CuryInfoID", summ_info.CuryInfoID);
							sender.Delete(summ_info);
							return;
						}
					}
				}
			}
		}
		#endregion

		#region GLDocBatch Events

		protected virtual void GLDocBatch_LedgerID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			GLDocBatch batch = (GLDocBatch)e.Row;
			_Ledger = (Ledger)PXSelectorAttribute.Select<GLDocBatch.ledgerID>(BatchModule.Cache, batch);

			currencyinfo.Cache.SetDefaultExt<CurrencyInfo.baseCuryID>(currencyinfo.Current);
			currencyinfo.Cache.SetDefaultExt<CurrencyInfo.curyID>(currencyinfo.Current);
			sender.SetDefaultExt<GLDocBatch.curyID>(batch);

			_Ledger = null;

			foreach (GLTranDoc tran in GLTranModuleBatNbr.Select())
			{
				GLTranDoc newtran = PXCache<GLTranDoc>.CreateCopy(tran);
				newtran.LedgerID = batch.LedgerID;

				GLTranModuleBatNbr.Cache.Update(newtran);
			}
		}

		protected virtual void GLDocBatch_CuryID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (_Ledger != null)
			{
				e.NewValue = _Ledger.BaseCuryID;
				e.Cancel = true;
			}
		}

		protected virtual void GLDocBatch_RowUpdated(PXCache cache, PXRowUpdatedEventArgs e)
		{
			GLDocBatch batch = (GLDocBatch)e.Row;
			if ((bool)glsetup.Current.RequireControlTotal == false || batch.Status == GLDocBatchStatus.Released)
			{
				if (batch.CuryCreditTotal != null && batch.CuryCreditTotal != 0)
				{
					cache.SetValue<GLDocBatch.curyControlTotal>(batch, batch.CuryCreditTotal);
				}
				else if (batch.CuryDebitTotal != null && batch.CuryDebitTotal != 0)
				{
					cache.SetValue<GLDocBatch.curyControlTotal>(batch, batch.CuryDebitTotal);
				}
				else
				{
					cache.SetValue<GLDocBatch.curyControlTotal>(batch, 0m);
				}

				//set control total explicitly
				if (batch.CreditTotal != null && batch.CreditTotal != 0)
				{
					cache.SetValue<GLDocBatch.controlTotal>(batch, batch.CreditTotal);
				}
				else if (batch.DebitTotal != null && batch.DebitTotal != 0)
				{
					cache.SetValue<GLDocBatch.controlTotal>(batch, batch.DebitTotal);
				}
				else
				{
					cache.SetValue<GLDocBatch.controlTotal>(batch, 0m);
			}
			}

			bool isOutOfBalance = false;

			if (batch.Status == GLDocBatchStatus.Balanced)
			{
				if (batch.CuryDebitTotal != batch.CuryCreditTotal)
				{
					isOutOfBalance = true;
				}

				if ((bool)glsetup.Current.RequireControlTotal)
				{
					if (batch.CuryCreditTotal != batch.CuryControlTotal)
					{
						cache.RaiseExceptionHandling<GLDocBatch.curyControlTotal>(batch, batch.CuryControlTotal, new PXSetPropertyException(Messages.BatchOutOfBalance));
					}
					else
					{
						cache.RaiseExceptionHandling<GLDocBatch.curyControlTotal>(batch, batch.CuryControlTotal, null);
					}
				}
			}

			if (isOutOfBalance)
			{
				cache.RaiseExceptionHandling<GLDocBatch.curyDebitTotal>(batch, batch.CuryDebitTotal, new PXSetPropertyException(Messages.BatchOutOfBalance));
			}
			else
			{
				cache.RaiseExceptionHandling<GLDocBatch.curyDebitTotal>(batch, batch.CuryDebitTotal, null);
			}
		}

		protected virtual void GLDocBatch_RowDeleting(PXCache cache, PXRowDeletingEventArgs e)
		{
			bool hasCreatedDocuments = false;
			foreach (GLTranDoc iDetail in this.GLTranModuleBatNbr.Select())
			{
				if (iDetail.DocCreated == true)
				{
					hasCreatedDocuments = true;
					break;
				}
			}
			if (hasCreatedDocuments)
			{
				throw new PXException(Messages.ERR_BatchContainRowsReferencingExistingDocumentAndCanNotBeDeleted);
		}
		}

		protected virtual void GLDocBatch_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			GLDocBatch batch = e.Row as GLDocBatch;

			if (batch == null)
				return;

			if (currencyinfo.Current != null && object.Equals(currencyinfo.Current.CuryInfoID, batch.CuryInfoID) == false)
			{
				currencyinfo.Current = null;
			}

			if (finperiod.Current != null && object.Equals(finperiod.Current.FinPeriodID, batch.FinPeriodID) == false)
			{
				finperiod.Current = null;
			}



			bool batchNotReleased = (batch.Released != true);
			bool batchPosted = (batch.Posted == true);
			bool batchVoided = (batch.Voided == true);
			bool batchModuleGL = (batch.Module == GL.BatchModule.GL);
			bool batchModuleCM = (batch.Module == GL.BatchModule.CM);
			bool batchStatusInserted = (cache.GetStatus(e.Row) == PXEntryStatus.Inserted);


			bool isViewSourceSupported = true;
			/*if (batch.Module == GL.BatchModule.GL
				|| batch.Module == GL.BatchModule.CM
				|| batch.Module == GL.BatchModule.FA)
				isViewSourceSupported = false; */


			viewDocument.SetEnabled(isViewSourceSupported);


			PXUIFieldAttribute.SetVisible<GLDocBatch.curyID>(cache, batch, PXAccess.FeatureInstalled<FeaturesSet.multicurrency>());
			PXDBCurrencyAttribute.SetBaseCalc<GLDocBatch.curyCreditTotal>(cache, batch, batchNotReleased);
			PXDBCurrencyAttribute.SetBaseCalc<GLDocBatch.curyDebitTotal>(cache, batch, batchNotReleased);
			PXDBCurrencyAttribute.SetBaseCalc<GLDocBatch.curyControlTotal>(cache, batch, batchNotReleased);

			release.SetEnabled(false);
			if (!batchModuleGL && (!batchModuleCM) && batchStatusInserted)
			{
				PXUIFieldAttribute.SetEnabled(cache, batch, false);
				cache.AllowUpdate = false;
				cache.AllowDelete = false;
				GLTranModuleBatNbr.Cache.AllowDelete = false;
				GLTranModuleBatNbr.Cache.AllowUpdate = false;
				GLTranModuleBatNbr.Cache.AllowInsert = false;
			}
			else if (batchVoided || !batchNotReleased)
			{
				PXUIFieldAttribute.SetEnabled(cache, batch, false);
				cache.AllowDelete = false;
				cache.AllowUpdate = false;
				GLTranModuleBatNbr.Cache.AllowDelete = false;
				GLTranModuleBatNbr.Cache.AllowUpdate = false;
				GLTranModuleBatNbr.Cache.AllowInsert = false;
			}
			else
			{
				PXUIFieldAttribute.SetEnabled(cache, batch, true);
				PXUIFieldAttribute.SetEnabled<GLDocBatch.status>(cache, batch, false);
				PXUIFieldAttribute.SetEnabled<GLDocBatch.curyCreditTotal>(cache, batch, false);
				PXUIFieldAttribute.SetEnabled<GLDocBatch.curyDebitTotal>(cache, batch, false);
				PXUIFieldAttribute.SetEnabled<GLDocBatch.origBatchNbr>(cache, batch, false);
				PXUIFieldAttribute.SetEnabled<GLDocBatch.branchID>(cache, batch, !GLTranModuleBatNbr.Any());
				cache.AllowDelete = true;
				cache.AllowUpdate = true;
				GLTranModuleBatNbr.Cache.AllowDelete = true;
				GLTranModuleBatNbr.Cache.AllowUpdate = true;
				GLTranModuleBatNbr.Cache.AllowInsert = true;
				bool hasData = false;
				bool isPartiallyReleased = false;
				foreach (GLTranDoc iDet in this.GLTranModuleBatNbr.Select())
				{
					if (!hasData && (iDet.BAccountID.HasValue || iDet.DebitAccountID.HasValue || iDet.CreditAccountID.HasValue))
					{
						hasData = true;
					}
					if (iDet.DocCreated == true)
					{
						isPartiallyReleased = true;
					}
					if (hasData && isPartiallyReleased) break;
				}
				PXUIFieldAttribute.SetEnabled<GLDocBatch.curyID>(cache, batch, !hasData);
				this.currencyinfo.Cache.AllowUpdate = !isPartiallyReleased;
			}
			release.SetEnabled(batchNotReleased && batch.Status != GLDocBatchStatus.Hold && !batchVoided);

			PXUIFieldAttribute.SetEnabled<GLDocBatch.module>(cache, batch);
			PXUIFieldAttribute.SetEnabled<GLDocBatch.batchNbr>(cache, batch);
			PXUIFieldAttribute.SetVisible<GLDocBatch.curyControlTotal>(cache, batch, (bool)glsetup.Current.RequireControlTotal);
			PXUIFieldAttribute.SetEnabled<GLDocBatch.ledgerID>(cache, batch, false);
			//PXUIFieldAttribute.SetVisible<GLTranDoc.tranDate>(GLTranModuleBatNbr.Cache, null, !batchModuleGL);         
		}

		protected virtual void GLDocBatch_FinPeriodID_ExceptionHandling(PXCache sender, PXExceptionHandlingEventArgs e)
		{
			if (_ExceptionHandling)
			{
				e.Cancel = true;
			}
		}

		protected virtual void GLDocBatch_DateEntered_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			GLDocBatch batch = (GLDocBatch)e.Row;

			SetTransactionsChanged<GLTranDoc.tranDate>();

			CurrencyInfoAttribute.SetEffectiveDate<GLDocBatch.dateEntered>(cache, e);
		}

		protected virtual void GLDocBatch_FinPeriodID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			SetTransactionsChanged();
		}

		protected virtual void GLDocBatch_Module_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = GL.BatchModule.GL;
		}

		protected virtual void GLDocBatch_BranchID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			sender.SetDefaultExt<GLDocBatch.ledgerID>(e.Row);
		}
		#endregion

		#region GLTranDoc Events

		private bool _importing;
		private bool _skipDefaulting = false;

		protected virtual void GLTranDoc_TranCode_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (this._skipDefaulting) return;
			GLTranDoc row = (GLTranDoc)e.Row;
			if (row != null)
			{
				if (row.IsChildTran)
				{
					GLTranDoc parent = FindParent(row);
					e.NewValue = sender.GetValue(parent, "TranCode");
					e.Cancel = true;
				}
				else
				{
					GLTranDoc prev = this.FindPrevMasterTran(row);
					if (prev != null)
					{
						e.NewValue = prev.TranCode;
						e.Cancel = true;
					}
				}
			}
		}

		protected virtual void GLTranDoc_BranchID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (this._skipDefaulting) return;
			GLTranDoc row = (GLTranDoc)e.Row;
			if (row != null && row.IsChildTran)
			{
				GLTranDoc parent = FindParent(row);
				e.NewValue = parent.BranchID;
				e.Cancel = true;
			}
		}

		protected virtual void GLTranDoc_TranModule_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (this._skipDefaulting) return;
			GLTranDoc row = (GLTranDoc)e.Row;
			if (row != null && row.IsChildTran)
			{
				GLTranDoc parent = FindParent(row);
				e.NewValue = sender.GetValue(parent, "TranModule");
				e.Cancel = true;
			}
		}

		protected virtual void GLTranDoc_TranType_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (this._skipDefaulting) return;
			GLTranDoc row = (GLTranDoc)e.Row;
			if (row != null && row.IsChildTran)
			{
				GLTranDoc parent = FindParent(row);
				e.NewValue = sender.GetValue(parent, "TranType");
				e.Cancel = true;
			}
		}

		protected virtual void GLTranDoc_TranDate_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (this._skipDefaulting) return;
			GLTranDoc row = (GLTranDoc)e.Row;
			if (row != null && row.IsChildTran)
			{
				GLTranDoc parent = FindParent(row);
				e.NewValue = sender.GetValue(parent, "TranDate");
				e.Cancel = true;
			}
		}

		protected virtual void GLTranDoc_CuryID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (this._skipDefaulting) return;
			GLTranDoc row = (GLTranDoc)e.Row;
			if (row != null && row.IsChildTran)
			{
				GLTranDoc parent = FindParent(row);
				e.NewValue = sender.GetValue(parent, "CuryID");
				e.Cancel = true;
			}
		}

		protected virtual void GLTranDoc_CuryInfoID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (this._skipDefaulting) return;
			GLTranDoc row = (GLTranDoc)e.Row;
			if (row != null && row.IsChildTran)
			{
				GLTranDoc parent = FindParent(row);
				e.NewValue = sender.GetValue(parent, "CuryInfoID");
				e.Cancel = true;
			}
		}

		protected virtual void GLTranDoc_BAccountID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (this._skipDefaulting) return;
			GLTranDoc row = (GLTranDoc)e.Row;
			Type type = e.GetType();
			if (row != null && row.IsChildTran)
			{
				GLTranDoc parent = FindParent(row);
				e.NewValue = parent.BAccountID;
				e.Cancel = true;
			}
		}

		protected virtual void GLTranDoc_LocationID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (this._skipDefaulting) return;
			GLTranDoc row = (GLTranDoc)e.Row;
			if (row != null && row.IsChildTran)
			{
				GLTranDoc parent = FindParent(row);
				e.NewValue = parent.LocationID;
				e.Cancel = true;
			}
		}

		protected virtual void GLTranDoc_RefNbr_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (this._skipDefaulting) return;
			GLTranDoc row = (GLTranDoc)e.Row;
			Type type = e.GetType();
			if (row != null && row.IsChildTran)
			{
				GLTranDoc parent = FindParent(row);
				e.NewValue = parent.RefNbr;
				e.Cancel = true;
			}
		}

		protected virtual void GLTranDoc_EntryTypeID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (this._skipDefaulting) return;
			GLTranDoc row = (GLTranDoc)e.Row;
			if (row != null && row.IsChildTran)
			{
				GLTranDoc parent = FindParent(row);
				e.NewValue = parent.EntryTypeID;
				e.Cancel = true;
			}
		}

		protected virtual void GLTranDoc_ProjectID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (this._skipDefaulting) return;
			GLTranDoc row = (GLTranDoc)e.Row;
			if (row != null && row.IsChildTran)
			{
				GLTranDoc parent = FindParent(row);
				e.NewValue = parent.ProjectID;
				e.Cancel = true;
			}
		}

		protected virtual void GLTranDoc_DebitAccountID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (this._skipDefaulting) return;
			GLTranDoc row = (GLTranDoc)e.Row;
			if (row != null && !string.IsNullOrEmpty(row.TranType))
			{
				if (IsDrCrAcctRequired(row, false))
				{
					e.NewValue = FindDefaultAccount(row, false);
					row.DebitCashAccountID = (this._cashAccountDebit != null) ? this._cashAccountDebit.CashAccountID : null;
				}
				else
				{
					e.NewValue = null;
					row.DebitCashAccountID = null;
				}
				e.Cancel = true;
			}
		}

		protected virtual void GLTranDoc_DebitSubID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (this._skipDefaulting) return;
			GLTranDoc row = (GLTranDoc)e.Row;
			if (row != null && !string.IsNullOrEmpty(row.TranType))
			{
				//if (IsDrCrAcctRequired(row, false))
				if (row.DebitAccountID.HasValue)
				{
					e.NewValue = FindDefaultSubAccount(row, false);
				}
				else
				{
					e.NewValue = null;
				}
				e.Cancel = true;
			}
		}

		protected virtual void GLTranDoc_CreditAccountID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (this._skipDefaulting) return;
			GLTranDoc row = (GLTranDoc)e.Row;
			if (row != null && !string.IsNullOrEmpty(row.TranType))
			{
				if (IsDrCrAcctRequired(row, true))
				{
					e.NewValue = FindDefaultAccount(row, true);
					row.CreditCashAccountID = (this._cashAccountCredit != null) ? this._cashAccountCredit.CashAccountID : null;
				}
				else
				{
					e.NewValue = null;
					row.CreditCashAccountID = null;
				}
				e.Cancel = true;
			}
		}

		protected virtual void GLTranDoc_CreditSubID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (this._skipDefaulting) return;
			GLTranDoc row = (GLTranDoc)e.Row;
			e.NewValue = null;
			if (row != null && !string.IsNullOrEmpty(row.TranType))
			{
				//if (IsDrCrAcctRequired(row, true))
				if (row.CreditAccountID.HasValue)
				{
					e.NewValue = FindDefaultSubAccount(row, true);
				}
				else
				{
					e.NewValue = null;
				}
				e.Cancel = true;
			}
		}

		protected virtual void GLTranDoc_TermsID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			GLTranDoc row = (GLTranDoc)e.Row;
			e.NewValue = null;
			if (row != null && !string.IsNullOrEmpty(row.TranType))
			{
				if (!row.IsChildTran)
				{
					if (IsAPInvoice(row) && row.BAccountID.HasValue)
					{
						AP.Vendor source = this.Vendor.Select(row.BAccountID);
						e.NewValue = source.TermsID;
						e.Cancel = true;
					}
					if (IsARInvoice(row) && row.BAccountID.HasValue)
					{
						AR.Customer source = this.Customer.Select(row.BAccountID);
						e.NewValue = source.TermsID;
						e.Cancel = true;
					}
				}
				else
				{
					e.Cancel = true;
					/*if (row != null && row.IsChildTran)
					{
						GLTranDoc parent = FindParent(row);                        
						e.NewValue = parent.TermsID;
						e.Cancel = true;
					}*/
				}
			}
		}

		private static bool _UseControlTotalEntry = true;

		protected virtual void GLTranDoc_ParentLineNbr_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (this._skipDefaulting) return;
			GLTranDoc row = (GLTranDoc)e.Row;
			bool isPasteOp = sender.Graph.IsImport;
			if ((!this._importing || isPasteOp) && row != null)
			{
				e.NewValue = null;
				Dictionary<int, List<GLTranDoc>> splits = new Dictionary<int, List<GLTranDoc>>();
				GLTranDoc lastParent = null;
				try
				{
					this._skipDefaulting = true;
					foreach (GLTranDoc iTran in this.GLTranModuleBatNbr.Select())
					{
						if (Object.ReferenceEquals(iTran, row)) continue;
						if (iTran.IsChildTran == false && (lastParent == null || lastParent.LineNbr < iTran.LineNbr))
						{
							lastParent = iTran;
						}
						if (iTran.IsBalanced || iTran.Split == false) continue;

						int key = iTran.IsChildTran ? iTran.ParentLineNbr.Value : iTran.LineNbr.Value;
						if (!splits.ContainsKey(key))
						{
							splits[key] = new List<GLTranDoc>();
						}
						splits[key].Add(iTran);
					}
				}
				finally
				{
					this._skipDefaulting = false;
				}
				List<KeyValuePair<GLTranDoc, decimal>> open = new List<KeyValuePair<GLTranDoc, decimal>>();
				GLTranDoc lastVersatile = null;
				foreach (List<GLTranDoc> iSet in splits.Values)
				{
					decimal balance = Decimal.Zero;
					GLTranDoc parent = null;
					foreach (GLTranDoc it in iSet)
					{
						Decimal tranAmount = _UseControlTotalEntry == false ? it.CuryTranAmt.Value : (it.IsChildTran ? it.CuryTranAmt.Value : (it.CuryTranTotal.Value - (it.CuryTaxTotal ?? Decimal.Zero)));
						if (it.DebitAccountID.HasValue)
						{
							balance += tranAmount;
						}
						if (it.CreditAccountID.HasValue)
						{
							balance -= tranAmount;
						}
						if (it.IsChildTran == false && parent == null)
						{
							parent = it;
						}
					}

					if (parent != null && balance != decimal.Zero)
					{
						open.Add(new KeyValuePair<GLTranDoc, decimal>(parent, balance));
					}
					else
					{
						if (parent != null && (parent.TranModule == GL.BatchModule.GL && (parent.TranModule == row.TranModule || String.IsNullOrEmpty(row.TranModule))))
						{
							if (lastVersatile == null || lastVersatile.LineNbr < parent.LineNbr)
							{
								lastVersatile = parent;
							}
						}
					}
				}

				if (lastVersatile != null && lastVersatile.LineNbr < lastParent.LineNbr)
				{
					lastVersatile = null;
				}
				GLTranDoc parentToAssign = open.Count > 0 ? open[0].Key : lastVersatile;
				decimal openBalance = open.Count > 0 ? -open[0].Value : Decimal.Zero; //Complementary value
				if (parentToAssign != null)
				{
					bool? parentIsDebit = IsDebitTran(parentToAssign);
					row.CuryBalanceAmt = !parentIsDebit.HasValue ? Decimal.Zero : (parentIsDebit == false ? openBalance : -openBalance);
					e.NewValue = parentToAssign.LineNbr;
				}
			}
			e.Cancel = true;
		}

		protected virtual void GLTranDoc_CuryTranAmt_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (_UseControlTotalEntry == false)
			{
				if (this._skipDefaulting) return;
				GLTranDoc row = (GLTranDoc)e.Row;
				if (row != null && row.IsChildTran)
				{
					e.NewValue = row.CuryBalanceAmt.HasValue ? row.CuryBalanceAmt.Value : Decimal.Zero;
					e.Cancel = true;
				}
				else
				{
					e.NewValue = Decimal.Zero;
				}
			}
			else
			{
				e.NewValue = Decimal.Zero;
			}
		}

		protected virtual void GLTranDoc_CuryTranTotal_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			//if (_UseControlTotalEntry)
			//{
			//    if (this._skipDefaulting) return;
			//    GLTranDoc row = (GLTranDoc)e.Row;
			//    if (row != null && row.IsChildTran)
			//    {
			//        e.NewValue = row.CuryBalanceAmt.HasValue ? row.CuryBalanceAmt.Value : Decimal.Zero;
			//        e.Cancel = true;
			//    }
			//    else
			//    {
			//        e.NewValue = Decimal.Zero;
			//    }
			//}
		}

		#region Defaulting events for correct caclulation and display of taxes

		protected virtual void GLTranDoc_CuryTaxableAmt_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			GLTranDoc row = (GLTranDoc)e.Row;
			if (row != null && row.IsChildTran == false)
			{
				e.NewValue = Decimal.Zero;
				e.Cancel = true;
			}
		}

		protected virtual void GLTranDoc_TaxableAmt_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			GLTranDoc row = (GLTranDoc)e.Row;
			if (row != null && row.IsChildTran == false)
			{
				e.NewValue = Decimal.Zero;
				e.Cancel = true;
			}
		}

		protected virtual void GLTranDoc_CuryTaxAmt_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			GLTranDoc row = (GLTranDoc)e.Row;
			if (row != null && row.IsChildTran == false)
			{
				e.NewValue = Decimal.Zero;
				e.Cancel = true;
			}
		}

		protected virtual void GLTranDoc_TaxAmt_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			GLTranDoc row = (GLTranDoc)e.Row;
			if (row != null && row.IsChildTran == false)
			{
				e.NewValue = Decimal.Zero;
				e.Cancel = true;
			}
		}

		protected virtual void GLTranDoc_CuryInclTaxAmt_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			GLTranDoc row = (GLTranDoc)e.Row;
			if (row != null && row.IsChildTran == false)
			{
				e.NewValue = Decimal.Zero;
				e.Cancel = true;
			}
		}

		protected virtual void GLTranDoc_InclTaxAmt_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			GLTranDoc row = (GLTranDoc)e.Row;
			if (row != null && row.IsChildTran == false)
			{
				e.NewValue = Decimal.Zero;
				e.Cancel = true;
			}
		}

		protected virtual void GLTranDoc_CuryDiscAmt_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			GLTranDoc row = (GLTranDoc)e.Row;
			if (row != null && row.IsChildTran == false)
			{
				e.NewValue = Decimal.Zero;
				e.Cancel = true;
			}
		}

		protected virtual void GLTranDoc_DiscAmt_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			GLTranDoc row = (GLTranDoc)e.Row;
			if (row != null && row.IsChildTran == false)
			{
				e.NewValue = Decimal.Zero;
				e.Cancel = true;
			}
		}

		#endregion
		protected virtual void GLTranDoc_ExtRefNbr_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			GLTranDoc row = (GLTranDoc)e.Row;
			if (row != null && row.IsChildTran)
			{
				GLTranDoc parent = this.FindParent(row);
				e.NewValue = parent.ExtRefNbr;
				e.Cancel = true;
			}
		}

		protected virtual void GLTranDoc_Split_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			GLTranDoc row = (GLTranDoc)e.Row;
			e.NewValue = false;
			if (row != null && row.IsChildTran)
			{
				e.NewValue = true;
			}
			e.Cancel = true;
		}

		private bool _isPMDefaulting = false;
		protected virtual void GLTranDoc_PaymentMethodID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			GLTranDoc row = (GLTranDoc)e.Row;
			this._isPMDefaulting = false;
			e.NewValue = null;
			if (row != null && !row.IsChildTran)
			{
				if (IsAPPayment(row.TranModule, row.TranType) || IsAPInvoice(row.TranModule, row.TranType))
				{
					if (row.BAccountID.HasValue && row.LocationID.HasValue)
					{
						Location loc = PXSelect<Location, Where<Location.bAccountID, Equal<Required<Location.bAccountID>>,
									And<Location.locationID, Equal<Required<Location.locationID>>>>>.Select(this, row.BAccountID, row.LocationID);
						e.NewValue = (loc != null ? loc.VPaymentMethodID : null);
					}
				}
				else if (IsARPayment(row.TranModule, row.TranType) || IsARInvoice(row.TranModule, row.TranType))
				{
					Customer cpm = PXSelect<Customer, Where<Customer.bAccountID, Equal<Required<GLTranDoc.bAccountID>>>>.Select(this, row.BAccountID);
					e.NewValue = (cpm != null ? cpm.DefPaymentMethodID : null);
				}
			}
			if (e.NewValue != null)
			{
				this._isPMDefaulting = true;
			}
			e.Cancel = true;
		}

		protected virtual void GLTranDoc_PMInstanceID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			GLTranDoc row = (GLTranDoc)e.Row;
			e.NewValue = null;
			if (row != null && !row.IsChildTran)
			{
				if ((IsARPayment(row) || IsARInvoice(row)) && row.BAccountID.HasValue && String.IsNullOrEmpty(row.PaymentMethodID) == false)
				{

					CustomerPaymentMethod type = null;
					PaymentMethod pm = PXSelect<PaymentMethod, Where<PaymentMethod.paymentMethodID, Equal<Required<PaymentMethod.paymentMethodID>>>>.Select(this, row.PaymentMethodID);
					if (pm != null && pm.IsActive == true)
					{
						if (pm.ARIsProcessingRequired == true && IsARPayment(row))
						{
							//For the payments PM requiring processing are forbidden for this interface
							//This checking is needed here because the PaymentMethodID FieldVerifying fails to prevent PaymentMethodID to be set.
						}
						else
						{
							type = PXSelectJoin<CustomerPaymentMethod,
										  InnerJoin<Customer, On<Customer.bAccountID, Equal<CustomerPaymentMethod.bAccountID>>>,
										  Where<CustomerPaymentMethod.bAccountID, Equal<Required<CustomerPaymentMethod.bAccountID>>,
											And<CustomerPaymentMethod.paymentMethodID, Equal<Required<CustomerPaymentMethod.paymentMethodID>>,
											And<CustomerPaymentMethod.isActive, Equal<True>,
											And<Customer.defPMInstanceID, Equal<CustomerPaymentMethod.pMInstanceID>>>>>>.Select(this, row.BAccountID, row.PaymentMethodID);
							if (type == null)
							{
								type = PXSelect<CustomerPaymentMethod, Where<CustomerPaymentMethod.bAccountID,
													Equal<Required<CustomerPaymentMethod.bAccountID>>,
													And<CustomerPaymentMethod.paymentMethodID, Equal<Required<CustomerPaymentMethod.paymentMethodID>>,
													And<CustomerPaymentMethod.isActive, Equal<True>>>>,
												OrderBy<Desc<CustomerPaymentMethod.expirationDate,
													Desc<CustomerPaymentMethod.pMInstanceID>>>>.Select(this, row.BAccountID, row.PaymentMethodID);
							}
						}
					}
					e.NewValue = (type != null ? type.PMInstanceID : null);
				}
			}
			e.Cancel = true;
		}

		protected virtual void GLTranDoc_TaxZoneID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			GLTranDoc row = (GLTranDoc)e.Row;
			e.NewValue = null;

			if (row != null && !row.IsChildTran && row.BAccountID.HasValue && row.LocationID.HasValue)
			{
				Location location = this.Location.Select(row.BAccountID, row.LocationID);
				if (IsARInvoice(row))
				{
					e.NewValue = location.CTaxZoneID;
					e.Cancel = true;
				}

				if (IsAPInvoice(row))
				{
					e.NewValue = location.VTaxZoneID;
					e.Cancel = true;
				}
			}
			if (row != null && (!row.IsChildTran) && row.TranModule == GL.BatchModule.CA
				&& String.IsNullOrEmpty(row.EntryTypeID) == false && row.CashAccountID.HasValue)
			{
				CashAccountETDetail etDetail = GetCashAccountETDetail(row.EntryTypeID, row.CashAccountID);
				if (etDetail != null)
				{
					e.NewValue = etDetail.TaxZoneID;
				}

			}
			e.Cancel = true;
		}

		private CashAccountETDetail GetCashAccountETDetail(string entryTypeID, int? cashAccountID)
		{
			return PXSelect<CashAccountETDetail,
						Where<CashAccountETDetail.entryTypeID, Equal<Required<CashAccountETDetail.entryTypeID>>,
						And<CashAccountETDetail.accountID, Equal<Required<CashAccountETDetail.accountID>>>>>.Select(this, entryTypeID, cashAccountID);
		}

		protected virtual void GLTranDoc_TaxCategoryID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			GLTranDoc row = (GLTranDoc)e.Row;
			_importing = sender.GetValuePending(e.Row, PXImportAttribute.ImportFlag) != null;

			if (row.Split == true && row.IsChildTran == false && _importing && row.TaxCategoryID != null && row.TaxCategoryID != (string)e.OldValue)
			{
				sender.SetValueExt<GLTranDoc.taxCategoryID>(row, null);
			}
		}
		protected virtual void GLTranDoc_TaxCategoryID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			GLTranDoc row = (GLTranDoc)e.Row;
			e.NewValue = null;
			if (row != null && (row.Split == false || row.IsChildTran) && (row.BAccountID.HasValue && row.LocationID.HasValue))
			{
				if (row.IsChildTran)
				{
					GLTranDoc prevLine = FindPrevSibling(row);
					if (prevLine != null)
					{
						e.NewValue = prevLine.TaxCategoryID;
						e.Cancel = true;
					}
				}

				if (e.Cancel != true)
				{
					Location location = this.Location.Select(row.BAccountID, row.LocationID);
					if (IsARInvoice(row) || IsAPInvoice(row))
					{
						//if (TX.TaxAttribute.GetTaxCalc<GLTranDoc.taxCategoryID>(sender, e.Row) == TX.TaxCalc.Calc )
						{
							//if(row.InventoryID == null)
							{
								GLTranDoc source = row.Split == false ? row : FindParent(row);
								string taxZoneID = source.TaxZoneID;
								TX.TaxZone taxZone = PXSelect<TX.TaxZone, Where<TX.TaxZone.taxZoneID, Equal<Required<TX.TaxZone.taxZoneID>>>>.Select(this, taxZoneID);
								if (taxZone != null)
								{
									e.NewValue = taxZone.DfltTaxCategoryID;
								}
							}
						}
					}
				}
			}
			e.Cancel = true;
		}

		protected virtual void GLTranDoc_DebitAccountID_FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			if (e.Row != null)
			{
				_importing = sender.GetValuePending(e.Row, PXImportAttribute.ImportFlag) != null;
			}
		}

		protected virtual void GLTranDoc_CreditAccountID_FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			if (e.Row != null)
			{
				_importing = sender.GetValuePending(e.Row, PXImportAttribute.ImportFlag) != null;
			}
		}

		protected virtual void GLTranDoc_RefNbr_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			GLTranDoc row = (GLTranDoc)e.Row;
			if (e.ExternalCall == true && row != null)
			{
				e.NewValue = row.RefNbr;
				e.Cancel = true;
			}
		}

		protected virtual void GLTranDoc_RefNbr_FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			GLTranDoc row = (GLTranDoc)e.Row;
			if (row != null)
			{
				Dictionary<long, CAMessage> listMessages = PXLongOperation.GetCustomInfoPersistent(this.UID) as Dictionary<long, CAMessage>;
				if (listMessages != null)
				{
					CAMessage message = null;
					if (listMessages.ContainsKey(row.LineNbr.Value))
					{
						message = listMessages[row.LineNbr.Value];
					}
					if (message != null)
					{
						string fieldName = typeof(GLTranDoc.refNbr).Name;

						e.ReturnState = PXFieldState.CreateInstance(e.ReturnState, typeof(String), false, null, null, null, null, null, fieldName,
									null, null, message.Message, message.ErrorLevel, null, null, null, PXUIVisibility.Undefined, null, null, null);
						e.IsAltered = true;
					}
				}
			}
		}

		protected virtual void GLTranDoc_TranCode_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			GLTranDoc row = (GLTranDoc)e.Row;
			GLDocBatch doc = this.BatchModule.Current;
			if (row != null && (String.IsNullOrEmpty(row.RefNbr) == false && !((row.TranModule == GL.BatchModule.GL && doc != null && row.LineNbr == doc.LineCntr))))
			{
				e.NewValue = row.TranCode;
				throw new PXSetPropertyException(Messages.TransactionCodeMayNotBeChangedForTheLineWithAssinedReferencedNumber);
			}
		}

		protected virtual void GLTranDoc_DebitAccountID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			VerifyAccount<GLTranDoc.debitAccountID>(sender, e, isDebitAccount: true);
		}

		protected virtual void GLTranDoc_DebitSubID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			GLTranDoc row = e.Row as GLTranDoc;
			if (row != null && e.NewValue != null && row.DebitAccountID != null)
			{
				bool hasCashAcct = false;
				bool hasSub = false;
				Int32? subID = (Int32?)e.NewValue;
				if (IsMatching(this._cashAccountDebit, row.BranchID, row.DebitAccountID, subID))
				{
					hasCashAcct = true;
					hasSub = true;
				}
				else
				{
					string entryTypeID = (string.IsNullOrEmpty(row.EntryTypeID) == false && row.NeedsDebitCashAccount == true) ? row.EntryTypeID : null;
					this._cashAccountDebit = this.GetCashAccount(row.BranchID.Value, row.DebitAccountID.Value, subID.Value, entryTypeID, out hasCashAcct);
					hasSub = (this._cashAccountDebit != null);
				}
				if (hasCashAcct && hasSub == false)
				{
					Branch branch = (Branch)PXSelectorAttribute.Select<GLTranDoc.branchID>(sender, row);
					Account account = (Account)PXSelectorAttribute.Select<GLTranDoc.debitAccountID>(sender, row);
					Sub sub = PXSelect<Sub, Where<Sub.subID, Equal<Required<Sub.subID>>>>.Select(this, subID);
					e.NewValue = sub.SubCD;
					throw new PXSetPropertyException(Messages.CashAccountDoesNotExist, branch.BranchCD, account.AccountCD, sub.SubCD);
				}
			}
		}


		protected virtual void GLTranDoc_CreditAccountID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			VerifyAccount<GLTranDoc.creditAccountID>(sender, e, isDebitAccount: false);
		}

		private void VerifyAccountIDToBeNoControl<T>(PXCache cache, EventArgs e, GLTranDoc tran, bool debit) where T : IBqlField		{			var code = (GLTranCode)PXSelect<GLTranCode>.Search<GLTranCode.tranCode>(this, tran.TranCode);
			var module = code?.Module;
			var tranType = code?.TranType;
			var credit = !debit;

			if (!(				module == GL.BatchModule.AR && tranType == ARDocType.CashSale ||				module == GL.BatchModule.AP && tranType == APDocType.QuickCheck ||				module == GL.BatchModule.CA && tranType == CATranType.CAAdjustment ||				module == GL.BatchModule.GL && tranType == GLTranType.GLEntry ||				debit &&					(					module == GL.BatchModule.AR && tranType == ARDocType.CreditMemo ||					module == GL.BatchModule.AP && (tranType == APDocType.Invoice || tranType == APDocType.CreditAdj)					) ||				credit &&					(					module == GL.BatchModule.AR && (tranType == ARDocType.Invoice|| tranType == ARDocType.DebitMemo) ||					module == GL.BatchModule.AP && (tranType == APDocType.DebitAdj)					)				)) return;			AccountAttribute.VerifyAccountIsNotControl<T>(cache, e);		}

		private void VerifyDebitAccountToBeControl(PXCache cache, EventArgs e, GLTranDoc tran)		{			var code = (GLTranCode)PXSelect<GLTranCode>.Search<GLTranCode.tranCode>(this, tran.TranCode);
			if (code?.Module == null || code?.TranType == null) return;
			var module = code.Module;
			var type = code.TranType;

			if (!(				module == GL.BatchModule.AR && (type == ARDocType.Invoice || type == ARDocType.DebitMemo) ||				module == GL.BatchModule.AP && (type == APDocType.DebitAdj || type == APDocType.Check || type == APDocType.Prepayment)				)) return;			var account = (Account)PXSelect<Account>.Search<Account.accountID>(this, tran.DebitAccountID);			AccountAttribute.VerifyControlAccount<GLTranDoc.debitAccountID>(cache, e, module);		}

		private void VerifyCreditAccountToBeControl(PXCache cache, EventArgs e, GLTranDoc tran)		{			var code = (GLTranCode)PXSelect<GLTranCode>.Search<GLTranCode.tranCode>(this, tran.TranCode);
			if (code?.Module == null || code?.TranType == null) return;
			var module = code.Module;
			var type = code.TranType;

			if (!(				module == GL.BatchModule.AR && (type == ARDocType.CreditMemo || type == ARDocType.Payment || type == ARDocType.Prepayment) ||				module == GL.BatchModule.AP && (type == APDocType.CreditAdj || type == APDocType.Invoice)				)) return;			var account = (Account)PXSelect<Account>.Search<Account.accountID>(this, tran.CreditAccountID);			AccountAttribute.VerifyControlAccount<GLTranDoc.creditAccountID>(cache, e, module);		}

		protected virtual void GLTranDoc_CreditSubID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			GLTranDoc row = e.Row as GLTranDoc;
			if (row != null && e.NewValue != null && row.CreditAccountID != null)
			{
				bool hasCashAcct = false;
				bool hasSub = false;
				Int32? subID = (Int32?)e.NewValue;
				if (IsMatching(this._cashAccountCredit, row.BranchID, row.CreditAccountID, subID))
				{
					hasCashAcct = true;
					hasSub = true;
				}
				else
				{
					string entryTypeID = (string.IsNullOrEmpty(row.EntryTypeID) == false && row.NeedsCreditCashAccount == true) ? row.EntryTypeID : null;
					this._cashAccountCredit = this.GetCashAccount(row.BranchID.Value, row.CreditAccountID.Value, subID.Value, entryTypeID, out hasCashAcct);
					hasSub = (this._cashAccountCredit != null);
				}
				if (hasCashAcct && hasSub == false)
				{
					Branch branch = (Branch)PXSelectorAttribute.Select<GLTranDoc.branchID>(sender, row);
					Account account = (Account)PXSelectorAttribute.Select<GLTranDoc.creditAccountID>(sender, row);
					Sub sub = PXSelect<Sub, Where<Sub.subID, Equal<Required<Sub.subID>>>>.Select(this, subID);
					e.NewValue = sub.SubCD;
					throw new PXSetPropertyException(Messages.CashAccountDoesNotExist, branch.BranchCD, account.AccountCD, sub.SubCD);
				}
			}
		}

		protected virtual void GLTranDoc_PaymentMethodID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			GLTranDoc row = e.Row as GLTranDoc;
			bool isPaste = this.IsImport;
			try
			{
				if (row != null && String.IsNullOrEmpty(row.TranModule) == false)
				{
					string pmID = (string)e.NewValue;
					if (string.IsNullOrEmpty(pmID) == false && (IsARPayment(row) || IsAPPayment(row)))
					{
						PaymentMethod paymentMethod = PXSelect<PaymentMethod, Where<PaymentMethod.paymentMethodID, Equal<Required<PaymentMethod.paymentMethodID>>>>.Select(this, pmID);
						if (paymentMethod != null)
						{
							if (!(this._isPMDefaulting && isPaste))
							{
								if (row.TranModule == GL.BatchModule.AP
									&& (paymentMethod.APPrintChecks == true || paymentMethod.APCreateBatchPayment == true))
								{
									throw new PXSetPropertyException(Messages.PaymentMethodWithAPPrintCheckOrAPCreateBatchMayNotBeEntered);
								}
								if (row.TranModule == GL.BatchModule.AR
									&& (paymentMethod.ARIsProcessingRequired == true))
								{
									throw new PXSetPropertyException(Messages.PaymentMethodWithARIntegratedProcessingMayNotBeEntered);
								}
							}
							else
							{
								e.Cancel = true;
							}
						}
					}
				}
			}
			finally
			{
				this._isPMDefaulting = false;
			}
		}

		protected virtual void GLTranDoc_Split_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			GLTranDoc row = (GLTranDoc)e.Row;
			if (row != null && row.IsChildTran == false && HasDocumentRow(row))
			{
				if ((bool)e.NewValue == true && row.CuryTranTotal == Decimal.Zero)
				{
					sender.RaiseExceptionHandling<GLTranDoc.split>(e.Row, false, new PXSetPropertyException(Messages.DocumentMustHaveBalanceToMayBeSplitted));
				}
			}
		}

		protected virtual void GLTranDoc_CuryTranTotal_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			GLTranDoc row = (GLTranDoc)e.Row;
			if (row != null && HasDocumentRow(row) && row.IsChildTran == false && row.Split == true)
			{
				Decimal? newValue = (Decimal?)e.NewValue;
				if (!newValue.HasValue || newValue <= Decimal.Zero)
				{
					throw new PXSetPropertyException(Messages.DocumentMustHaveBalanceToMayBeSplitted);
				}
			}
		}

		protected virtual void GLTranDoc_DebitAccountID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			GLTranDoc row = (GLTranDoc)e.Row;
			if (row != null)
			{
				try
				{
					VerifyDebitAccountToBeControl(sender, e, row);
					VerifyAccountIDToBeNoControl<GLTranDoc.debitAccountID>(sender, e, row, true);
				}
				catch (PXSetPropertyException)
				{
				}

				if (row.DebitAccountID != null)
				{
					if (this._cashAccountDebit == null || this._cashAccountDebit.AccountID != row.DebitAccountID)
					{
						bool single;
						string entryTypeID = (row.TranModule == GL.BatchModule.CA && row.NeedsDebitCashAccount == true) ? row.EntryTypeID : null;
						this._cashAccountDebit = GetCashAccount(row.BranchID.Value, row.DebitAccountID.Value, entryTypeID, out single);
						row.DebitCashAccountID = this._cashAccountDebit != null ? this._cashAccountDebit.CashAccountID : null;
					}
				}
				else
				{
					row.DebitCashAccountID = null;
				}
				sender.SetDefaultExt<GLTranDoc.debitSubID>(e.Row);

				if (row.TranModule == GL.BatchModule.CA && row.CADrCr == CADrCr.CADebit)
				{
					sender.SetDefaultExt<GLTranDoc.creditAccountID>(e.Row);
					sender.SetDefaultExt<GLTranDoc.taxZoneID>(e.Row);
				}
			}
		}

		protected virtual void GLTranDoc_CreditAccountID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			GLTranDoc row = (GLTranDoc)e.Row;
			if (row != null)
			{
				VerifyCreditAccountToBeControl(sender, e, row);
				VerifyAccountIDToBeNoControl<GLTranDoc.creditAccountID>(sender, e, row, false);

				if (row.CreditAccountID != null)
				{
					if (this._cashAccountCredit == null || this._cashAccountCredit.AccountID != row.CreditAccountID)
					{
						bool single;
						string entryTypeID = (row.TranModule == GL.BatchModule.CA && row.NeedsCreditCashAccount == true) ? row.EntryTypeID : null;
						this._cashAccountCredit = GetCashAccount(row.BranchID.Value, row.CreditAccountID.Value, entryTypeID, out single);
					}
					row.CreditCashAccountID = this._cashAccountCredit != null ? this._cashAccountCredit.CashAccountID : null;
				}
				else
				{
					row.CreditCashAccountID = null;
				}
				sender.SetDefaultExt<GLTranDoc.creditSubID>(e.Row);
				if (row.TranModule == GL.BatchModule.CA && row.CADrCr == CADrCr.CACredit)
				{
					sender.SetDefaultExt<GLTranDoc.debitAccountID>(e.Row);
					sender.SetDefaultExt<GLTranDoc.taxZoneID>(e.Row);
				}
			}
		}

		protected virtual void GLTranDoc_BAccountID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			if (e.Row != null)
			{
				_importing = sender.GetValuePending(e.Row, PXImportAttribute.ImportFlag) != null;
			}
			sender.SetDefaultExt<GLTranDoc.locationID>(e.Row);
			sender.SetDefaultExt<GLTranDoc.termsID>(e.Row);
			sender.SetDefaultExt<GLTranDoc.projectID>(e.Row);
		}

		protected virtual void GLTranDoc_LocationID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			if (e.Row != null)
			{
				_importing = sender.GetValuePending(e.Row, PXImportAttribute.ImportFlag) != null;
			}
			sender.SetDefaultExt<GLTranDoc.paymentMethodID>(e.Row);
			sender.SetDefaultExt<GLTranDoc.debitAccountID>(e.Row);
			sender.SetDefaultExt<GLTranDoc.debitSubID>(e.Row);
			sender.SetDefaultExt<GLTranDoc.creditAccountID>(e.Row);
			sender.SetDefaultExt<GLTranDoc.creditSubID>(e.Row);
			sender.SetDefaultExt<GLTranDoc.taxZoneID>(e.Row);
			sender.SetDefaultExt<GLTranDoc.taxCategoryID>(e.Row);
		}

		protected virtual void GLTranDoc_EntryTypeID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			if (e.Row != null)
			{
				sender.SetDefaultExt<GLTranDoc.cADrCr>(e.Row);
				sender.SetDefaultExt<GLTranDoc.debitAccountID>(e.Row);
				sender.SetDefaultExt<GLTranDoc.creditAccountID>(e.Row);
			}
		}

		protected virtual void GLTranDoc_Split_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			GLTranDoc row = (GLTranDoc)e.Row;
			if (e.Row != null && row.IsChildTran == false)
			{
				bool isMixedType = IsMixedType(row);
				bool isDebit = false;
				if (isMixedType)
				{
					isDebit = (IsARInvoice(row) ? AR.ARInvoiceType.DrCr(row.TranType) : AP.APInvoiceType.DrCr(row.TranType)) == DrCr.Debit;
				}
				else
				{
					bool? isDebitType = IsDebitType(row, true);
					if (isDebitType.HasValue)
					{
						isDebit = isDebitType.Value;
					}
					else
					{
						if (row.CreditAccountID.HasValue && row.DebitAccountID.HasValue)
						{
							isDebit = false;
						}
						else
						{
							isDebit = row.CreditAccountID.HasValue;
						}

					}
				}

				if (row.Split == true)
				{
					if (isDebit)
					{
						sender.SetValueExt<GLTranDoc.debitAccountID>(row, null);
					}
					else
					{
						sender.SetValueExt<GLTranDoc.creditAccountID>(row, null);
					}

					if (HasDocumentRow(row))
					{
						sender.SetValueExt<GLTranDoc.curyTranAmt>(row, decimal.Zero);
					}
					sender.SetDefaultExt<GLTranDoc.taskID>(row);
				}
				else
				{                                     
					if (isDebit)
					{
						sender.SetDefaultExt<GLTranDoc.debitAccountID>(row);
					}
					else
					{
						sender.SetDefaultExt<GLTranDoc.creditAccountID>(row);
					}
				}
				sender.SetDefaultExt<GLTranDoc.taxCategoryID>(row);
			}
		}

		protected virtual void GLTranDoc_TranCode_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			GLTranDoc row = (GLTranDoc)e.Row;
			if (e.ExternalCall == true && row.IsChildTran && (((string)(e.OldValue)) != row.TranCode))
			{
				row.ParentLineNbr = null;
				row.Split = false;
				row.RefNbr = null;
			}
			if (row != null)
			{
				sender.SetDefaultExt<GLTranDoc.tranModule>(e.Row);
				sender.SetDefaultExt<GLTranDoc.tranType>(e.Row);
			}
		}

		protected virtual void GLTranDoc_PaymentMethodID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			GLTranDoc row = (GLTranDoc)e.Row;
			if (row != null && (IsARPayment(row) || IsARInvoice(row)))
			{
				if (IsARPayment(row))
				{
					sender.SetDefaultExt<GLTranDoc.creditAccountID>(e.Row);
					sender.SetDefaultExt<GLTranDoc.debitAccountID>(e.Row);
				}
				sender.SetDefaultExt<GLTranDoc.pMInstanceID>(e.Row);
			}
			if (row != null && IsAPPayment(row))
			{
				sender.SetDefaultExt<GLTranDoc.creditAccountID>(e.Row);
				sender.SetDefaultExt<GLTranDoc.debitAccountID>(e.Row);
			}
		}

		protected virtual void GLTranDoc_PMInstanceID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			GLTranDoc row = (GLTranDoc)e.Row;
			if (row != null && IsARPayment(row))
			{
				sender.SetDefaultExt<GLTranDoc.creditAccountID>(e.Row);
				sender.SetDefaultExt<GLTranDoc.debitAccountID>(e.Row);
			}
		}

		protected virtual void GLTranDoc_ProjectID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			GLTranDoc row = (GLTranDoc)e.Row;
			if (row != null)
			{
				sender.SetDefaultExt<GLTranDoc.taskID>(e.Row);
			}
		}



		protected virtual void GLTranDoc_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			GLTranDoc row = (GLTranDoc)e.Row;
			if (row != null)
			{
				bool hasDocument = (String.IsNullOrEmpty(row.RefNbr) == false && row.DocCreated == true);
				bool projectInLines = (row.TranModule == GL.BatchModule.AP || row.TranModule == GL.BatchModule.GL);
				if (hasDocument)
				{
					PXUIFieldAttribute.SetEnabled(sender, row, false);
				}
				else
				{
					bool allowDetailAddToBalancedDoc = (row.TranModule == GL.BatchModule.GL);
					bool mayHaveApplications = (row.TranModule == GL.BatchModule.AP || row.TranModule == GL.BatchModule.AR);
					bool hasApplications = false;
					if (row.IsChildTran)
					{
						if (mayHaveApplications)
						{
							GLTranDoc parent = this.FindParent(row);
							hasApplications = (parent.ApplCount ?? 0) > 0;
						}
						if (hasApplications)
						{
							PXUIFieldAttribute.SetEnabled(sender, row, false);
						}
						else
						{
							bool allowBothDRandCR = (row.TranModule == GL.BatchModule.GL); //For details only
							PXUIFieldAttribute.SetEnabled(sender, row, false);
							PXUIFieldAttribute.SetEnabled<GLTranDoc.curyTranAmt>(sender, row, false);
							PXUIFieldAttribute.SetEnabled<GLTranDoc.curyTranTotal>(sender, row, true);
							PXUIFieldAttribute.SetEnabled<GLTranDoc.tranDesc>(sender, row, true);

							bool isInserted = (sender.GetStatus(row) == PXEntryStatus.Inserted) && (row.LineNbr == (this.BatchModule.Current.LineCntr));
							PXUIFieldAttribute.SetEnabled<GLTranDoc.tranCode>(sender, row, allowDetailAddToBalancedDoc && isInserted && ((row.CuryBalanceAmt ?? Decimal.Zero) == Decimal.Zero));

							bool hasDebit = row.DebitAccountID.HasValue;
							bool hasCredit = row.CreditAccountID.HasValue;
							bool isDebitAcctIsCash = row.DebitAccountID.HasValue && row.NeedsDebitCashAccount == true;
							bool isCreditAcctIsCash = row.CreditAccountID.HasValue && row.NeedsCreditCashAccount == true;
							bool singleSubForDebitCA = false;
							bool singleSubForCreditCA = false;
							if (row.TranModule == GL.BatchModule.GL || row.TranModule == GL.BatchModule.CA)
							{
								//GLTran may contain cash account on either side - specific checking is needed
								if (row.DebitAccountID.HasValue)
								{
									string entryTypeID = (row.EntryTypeID != null && row.NeedsDebitCashAccount == true) ? row.EntryTypeID : null;
									isDebitAcctIsCash = HasCashAccount(row.BranchID.Value, row.DebitAccountID.Value, entryTypeID, out singleSubForDebitCA);
								}
								if (row.CreditAccountID.HasValue)
								{
									string entryTypeID = (row.EntryTypeID != null && row.NeedsCreditCashAccount == true) ? row.EntryTypeID : null;
									isCreditAcctIsCash = HasCashAccount(row.BranchID.Value, row.CreditAccountID.Value, entryTypeID, out singleSubForCreditCA);
								}
							}
							else
							{
								if (isDebitAcctIsCash)
								{
									HasCashAccount(row.BranchID.Value, row.DebitAccountID.Value, null, out singleSubForDebitCA);
								}
								if (isCreditAcctIsCash)
								{
									HasCashAccount(row.BranchID.Value, row.CreditAccountID.Value, null, out singleSubForCreditCA);
							}
							}

							bool isCAEntry = (row.TranModule == GL.BatchModule.CA);
							bool needsTaxCategory = IsARInvoice(row) || IsAPInvoice(row) || isCAEntry;
							if (hasCredit == false && hasDebit == false)
							{
								GLTranDoc parent = this.FindParent(row);
								hasCredit = parent != null && parent.DebitAccountID.HasValue;
								hasDebit = parent != null && parent.CreditAccountID.HasValue;
							}

							bool enableDebitAcct = !hasCredit || allowBothDRandCR;
							bool enableCreditAcct = !hasDebit || allowBothDRandCR;

							PXUIFieldAttribute.SetEnabled<GLTranDoc.debitAccountID>(sender, row, enableDebitAcct);
							PXUIFieldAttribute.SetEnabled<GLTranDoc.debitSubID>(sender, row, hasDebit && enableDebitAcct && !(isDebitAcctIsCash && singleSubForDebitCA));
							PXUIFieldAttribute.SetEnabled<GLTranDoc.creditAccountID>(sender, row, enableCreditAcct);
							PXUIFieldAttribute.SetEnabled<GLTranDoc.creditSubID>(sender, row, hasCredit && enableCreditAcct && !(isCreditAcctIsCash && singleSubForCreditCA));
							PXUIFieldAttribute.SetEnabled<GLTranDoc.taxZoneID>(sender, row, false);
							PXUIFieldAttribute.SetEnabled<GLTranDoc.taxCategoryID>(sender, row, needsTaxCategory);
							PXUIFieldAttribute.SetEnabled<GLTranDoc.entryTypeID>(sender, row, false);
							PXUIFieldAttribute.SetEnabled<GLTranDoc.projectID>(sender, row, projectInLines);
							PXUIFieldAttribute.SetEnabled<GLTranDoc.taskID>(sender, row, true);
						}
					}
					else
					{
						hasApplications = (row.ApplCount ?? 0) > 0;
						if (hasApplications)
						{
							PXUIFieldAttribute.SetEnabled(sender, row, false);
						}
						else
						{
							if (string.IsNullOrEmpty(row.TranType))
							{
								PXUIFieldAttribute.SetEnabled(sender, row, false);
								PXUIFieldAttribute.SetEnabled<GLTranDoc.tranCode>(sender, row, true);
								PXUIFieldAttribute.SetEnabled<GLTranDoc.tranType>(sender, row, false);
								PXUIFieldAttribute.SetEnabled<GLTranDoc.tranDate>(sender, row, true);
							}
							else
							{
								bool hasChildren = row.Split.Value;
								bool isAPPayment = IsAPPayment(row);
								bool isARPayment = IsARPayment(row);
								bool isAPPrePayment = IsAPPrePayment(row);
								bool isARInvoice = IsARInvoice(row);
								bool isAPInvoice = IsAPInvoice(row);
								bool isARCreditMemo = IsARCreditMemo(row);
								bool isGLEntry = (row.TranModule == GL.BatchModule.GL);
								bool isCAEntry = (row.TranModule == GL.BatchModule.CA);

								PXUIFieldAttribute.SetEnabled(sender, row, true);
								PXUIFieldAttribute.SetEnabled<GLTranDoc.paymentMethodID>(sender, row, isARPayment || isAPPayment || isAPInvoice || isARInvoice);
								bool isPMInstanceEnabled = false;
								bool isPMInstanceRequired = false;
								if ((isARPayment || isARInvoice) && String.IsNullOrEmpty(row.PaymentMethodID) == false)
								{
									PaymentMethod pm = PXSelect<PaymentMethod, Where<PaymentMethod.paymentMethodID, Equal<Required<PaymentMethod.paymentMethodID>>>>.Select(this, row.PaymentMethodID);
									isPMInstanceEnabled = (pm.IsAccountNumberRequired == true);
								}
								isPMInstanceRequired = (isPMInstanceEnabled && isARPayment);
								PXUIFieldAttribute.SetEnabled<GLTranDoc.pMInstanceID>(sender, row, (isARPayment || isARInvoice) && isPMInstanceEnabled);

								bool isCredit = row.CreditAccountID.HasValue;
								bool isDebit = row.DebitAccountID.HasValue;
								bool isDebitAcctIsCash = row.DebitAccountID.HasValue && row.NeedsDebitCashAccount == true;
								bool isCreditAcctIsCash = row.CreditAccountID.HasValue && row.NeedsCreditCashAccount == true;
								bool singleSubForDebitCA = true;
								bool singleSubForCreditCA = true;
								bool hasEntryType = String.IsNullOrEmpty(row.EntryTypeID) == false;
								bool hasDocumentRef = String.IsNullOrEmpty(row.RefNbr) == false;
								if (row.TranModule == GL.BatchModule.GL || row.TranModule == GL.BatchModule.CA)
								{
									//GLTran may contain cash account on either side - specific checking is needed                                    
									if (row.DebitAccountID.HasValue)
									{
										string entryTypeID = (row.EntryTypeID != null && row.NeedsDebitCashAccount == true) ? row.EntryTypeID : null;
										isDebitAcctIsCash = HasCashAccount(row.BranchID.Value, row.DebitAccountID.Value, entryTypeID, out singleSubForDebitCA);
									}
									if (row.CreditAccountID.HasValue)
									{
										string entryTypeID = (row.EntryTypeID != null && row.NeedsCreditCashAccount == true) ? row.EntryTypeID : null;
										isCreditAcctIsCash = HasCashAccount(row.BranchID.Value, row.CreditAccountID.Value, entryTypeID, out singleSubForCreditCA);
									}
								}
								else
								{
									if (isDebitAcctIsCash)
									{
										HasCashAccount(row.BranchID.Value, row.DebitAccountID.Value, null, out singleSubForDebitCA);
									}
									if (isCreditAcctIsCash)
									{
										HasCashAccount(row.BranchID.Value, row.CreditAccountID.Value, null, out singleSubForCreditCA);
								}
								}

								PXUIFieldAttribute.SetEnabled<GLTranDoc.creditAccountID>(sender, row, (!hasChildren || isCredit) && (!isCAEntry || hasEntryType));
								PXUIFieldAttribute.SetEnabled<GLTranDoc.creditSubID>(sender, row, isCredit && (!hasChildren || isCredit) && !(isCreditAcctIsCash && singleSubForCreditCA) && (!isCAEntry || hasEntryType));
								PXUIFieldAttribute.SetEnabled<GLTranDoc.debitAccountID>(sender, row, (!hasChildren || isDebit) && (!isCAEntry || hasEntryType));
								PXUIFieldAttribute.SetEnabled<GLTranDoc.debitSubID>(sender, row, isDebit && (!hasChildren || isDebit) && !(isDebitAcctIsCash && singleSubForDebitCA) && (!isCAEntry || hasEntryType));

								PXUIFieldAttribute.SetEnabled<GLTranDoc.tranCode>(sender, row, !hasChildren && !hasDocumentRef);
								PXUIFieldAttribute.SetEnabled<GLTranDoc.tranType>(sender, row, false);
								PXUIFieldAttribute.SetEnabled<GLTranDoc.tranDate>(sender, row, true);

								PXUIFieldAttribute.SetEnabled<GLTranDoc.termsID>(sender, row, (isARInvoice || isAPInvoice) && !isARCreditMemo);
								PXUIFieldAttribute.SetEnabled<GLTranDoc.dueDate>(sender, row, (isARInvoice || isAPInvoice) && !isARCreditMemo);
								PXUIFieldAttribute.SetEnabled<GLTranDoc.discDate>(sender, row, (isARInvoice || isAPInvoice) && !isARCreditMemo);
								PXUIFieldAttribute.SetEnabled<GLTranDoc.curyDiscAmt>(sender, row, (isARInvoice || isAPInvoice) && !isARCreditMemo);

								PXUIFieldAttribute.SetEnabled<GLTranDoc.entryTypeID>(sender, row, isCAEntry);
								if (isCAEntry)
								{
									AdjustCAAccountsFields(sender, row);
								}

								PXUIFieldAttribute.SetEnabled<GLTranDoc.split>(sender, row, (isARInvoice || isAPInvoice || isCAEntry || isGLEntry)); //Allow splitting for invoices only - verify later

								bool needsBAccount = (row.TranModule == GL.BatchModule.AP || row.TranModule == GL.BatchModule.AR);
								bool needsTaxInfo = IsARInvoice(row) || IsAPInvoice(row) || isCAEntry;
								PXUIFieldAttribute.SetEnabled<GLTranDoc.taxZoneID>(sender, row, needsTaxInfo);
								PXUIFieldAttribute.SetEnabled<GLTranDoc.taxCategoryID>(sender, row, needsTaxInfo && !hasChildren);
								PXUIFieldAttribute.SetEnabled<GLTranDoc.bAccountID>(sender, row, needsBAccount);
								PXUIFieldAttribute.SetEnabled<GLTranDoc.locationID>(sender, row, needsBAccount);

								PXUIFieldAttribute.SetEnabled<GLTranDoc.refNbr>(sender, row, false);


								PXUIFieldAttribute.SetEnabled<GLTranDoc.projectID>(sender, row, hasChildren == false || projectInLines == false);
								PXUIFieldAttribute.SetEnabled<GLTranDoc.taskID>(sender, row, true); //Actual handling happens  in the attribute

								if (row.Released == false)
								{
									bool isReadyForRelease = (this.BatchModule.Current != null && this.BatchModule.Current.Hold == false);
									bool forceZeroBalance = (isAPPayment && !isAPPrePayment && !isAPInvoice);
									if (forceZeroBalance && isReadyForRelease && row.CuryUnappliedBal != Decimal.Zero && row.TranType != APDocType.Prepayment)
									{
										sender.RaiseExceptionHandling<GLTranDoc.curyUnappliedBal>(row, row.CuryUnappliedBal, new PXSetPropertyException(Messages.DocumentMustByAppliedInFullBeforeItMayBeReleased, PXErrorLevel.Error));
									}
									else
									{
										sender.RaiseExceptionHandling<GLTranDoc.curyUnappliedBal>(row, row.CuryUnappliedBal, null);
									}
								}
							}
						}
					}
				}

				PXUIFieldAttribute.SetEnabled<GLTranDoc.released>(sender, row, false);
				PXUIFieldAttribute.SetEnabled<GLTranDoc.docCreated>(sender, row, false);
				PXUIFieldAttribute.SetEnabled<GLTranDoc.curyTaxableAmt>(sender, row, false);
				PXUIFieldAttribute.SetEnabled<GLTranDoc.curyTaxAmt>(sender, row, false);
				PXUIFieldAttribute.SetEnabled<GLTranDoc.curyTaxTotal>(sender, row, false);
				PXUIFieldAttribute.SetEnabled<GLTranDoc.curyDocTotal>(sender, row, false);
				PXUIFieldAttribute.SetEnabled<GLTranDoc.curyInclTaxAmt>(sender, row, false);

				if (_UseControlTotalEntry)
				{
					PXUIFieldAttribute.SetEnabled<GLTranDoc.curyTranAmt>(sender, row, false);
				}

				int? organizationID = PXAccess.GetParentOrganizationID(row.BranchID);
				if (row.Released != true && !FinPeriodRepository.IsDateWithinPeriod(BatchModule.Current.FinPeriodID, row.TranDate.Value, organizationID))

				if(row.Released != true && BatchModule.Current.FinPeriodID == null)
				{
					sender.RaiseExceptionHandling<GLTranDoc.tranDate>(row, row.TranDate, new PXSetPropertyException(PXLocalizer.LocalizeFormat(Messages.NoFinancialPeriodForDate, row.TranDate?.ToShortDateString()), PXErrorLevel.Error));
				}

				if (row.Released != true && BatchModule.Current.FinPeriodID != null && !FinPeriodRepository.IsDateWithinPeriod(BatchModule.Current.FinPeriodID, row.TranDate.Value, organizationID))
				{
					sender.RaiseExceptionHandling<GLTranDoc.tranDate>(row, row.TranDate, new PXSetPropertyException(Messages.FiscalPeriodNotCurrent, PXErrorLevel.Warning));
				}
			}

			PXUIFieldAttribute.SetVisible<GLTranDoc.curyDocTotal>(sender, null, false);
			PXUIFieldAttribute.SetVisible<GLTranDoc.curyInclTaxAmt>(sender, null, false);
		}


		private void AdjustCAAccountsFields(PXCache cache, GLTranDoc transaction)
		{
			if (transaction.EntryTypeID != null)
			{
				bool disableCredit = (transaction.CADrCr == CADrCr.CADebit) && transaction.DebitAccountID.HasValue == false;
				bool disableDebit = (transaction.CADrCr == CADrCr.CACredit) && transaction.CreditAccountID.HasValue == false;

				if (disableDebit)
				{
					PXUIFieldAttribute.SetEnabled<GLTranDoc.debitAccountID>(cache, transaction, false);
					PXUIFieldAttribute.SetEnabled<GLTranDoc.debitSubID>(cache, transaction, false);
				}
				if (disableCredit)
				{
					PXUIFieldAttribute.SetEnabled<GLTranDoc.creditAccountID>(cache, transaction, false);
					PXUIFieldAttribute.SetEnabled<GLTranDoc.creditSubID>(cache, transaction, false);
				}
			}
		}

		private bool _isMassDelete = false;
		private bool _isCacheSync = false;
		protected virtual void GLTranDoc_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			GLTranDoc row = (GLTranDoc)e.Row;
			GLTranDoc oldRow = (GLTranDoc)e.OldRow;
			bool isPasteOp = sender.Graph.IsImport;

			if (e.ExternalCall && (e.Row == null || !_importing) && sender.GetStatus(row) == PXEntryStatus.Inserted && ((GLTranDoc)e.OldRow).DebitAccountID == null && row.DebitAccountID != null)
			{
				GLTranDoc oldrow = PXCache<GLTranDoc>.CreateCopy(row);
				sender.RaiseRowUpdated(row, oldrow);
			}
			bool needsRefresh = false;

			if (row.IsChildTran == false && row.Split == false && oldRow.Split == true && this._isMassDelete == false)
			{
				DeleteChildren(row);
			}

			if (row.IsChildTran == false && row.Split == true)
			{
				List<int> altered = new List<int>();
				string[] fieldsToSync = { "ExtRefNbr", "TranDate", "BAccountID", "LocationID", "ProjectID" };
				bool skipProject = (row.TranModule == GL.BatchModule.AP || row.TranModule == GL.BatchModule.GL);
				foreach (string iField in fieldsToSync)
				{
					int ordinal = sender.GetFieldOrdinal(iField);
					if (iField == "ProjectID" && skipProject) continue;
					if (Object.Equals(sender.GetValue(row, ordinal), sender.GetValue(oldRow, ordinal)) == false)
					{
						altered.Add(ordinal);
				}
				}

				if (altered.Count > 0)
				{
					foreach (GLTranDoc it in this.GLTranModuleBatNbr.SearchAll<Asc<GLTranDoc.parentLineNbr>>(new object[] { row.LineNbr }))
					{
						bool updated = false;
						if (Object.ReferenceEquals(it, row) == false && row.LineNbr == it.ParentLineNbr)
						{
							GLTranDoc copy = (GLTranDoc)sender.CreateCopy(it);
							foreach (int iFld in altered)
							{
								object value = sender.GetValue(row, iFld);
								sender.SetValue(copy, iFld, value);
								updated = true;
							}
							if (updated)
							{
								Object savePoint = sender.Current;
								sender.Update(copy);
								sender.Current = savePoint;
							}
						}
					}
					needsRefresh = true;
				}
			}

			if ((!this._importing || isPasteOp) && row.IsChildTran && HasDocumentRow(row) && row.CuryTranAmt != oldRow.CuryTranAmt)
			{
				GLTranDoc parent = this.FindParent(row);
				if (parent != null)
				{
					Decimal? newValue = (parent.CuryTranAmt + (row.CuryTranAmt - oldRow.CuryTranAmt));
					GLTranDoc copy = (GLTranDoc)sender.CreateCopy(parent);
					copy.CuryTranAmt = newValue;
					Object savePoint = sender.Current;
					sender.Update(copy);
					sender.Current = savePoint;
				}
			}

			if ((!this._importing || isPasteOp) && HasDocumentRow(row) == false && row.CuryTranTotal != oldRow.CuryTranTotal)
			{
				row.CuryTranAmt = row.CuryTranTotal;
				row.TranAmt = row.TranTotal;
			}


			if (!needsRefresh && row.Split == true && row.IsChildTran)
			{
				string[] fieldsToSync = { "CuryTranAmt", "TaxCategoryID" };
				foreach (string iField in fieldsToSync)
				{
					int ordinal = sender.GetFieldOrdinal(iField);
					if (Object.Equals(sender.GetValue(row, ordinal), sender.GetValue(oldRow, ordinal)) == false)
					{
						needsRefresh = true; break;
					}
				}
			}

			if (row.IsChildTran == false && String.IsNullOrEmpty(row.RefNbr))
			{
				try
				{
					CreateRefNbr(row);
				}
				catch(PXManualNumberingException ex)
				{
					sender.RaiseExceptionHandling<GLTranDoc.refNbr>(row,row.RefNbr,new PXSetPropertyException(ex.MessageNoPrefix,PXErrorLevel.RowError));
				}
				needsRefresh = true;
			}

			if (needsRefresh)
			{
				this.GLTranModuleBatNbr.View.RequestRefresh();
			}

			if (row.IsChildTran == false && row.CuryTranTotal != row.CuryDocTotal)
			{
				decimal diff = row.CuryTranTotal.Value - row.CuryDocTotal.Value;
				sender.RaiseExceptionHandling<GLTranDoc.curyTranTotal>(row, row.CuryTranTotal, new PXSetPropertyException(Messages.DocumentIsOutOfBalance, PXErrorLevel.Warning, diff));
			}
			else
			{
				sender.RaiseExceptionHandling<GLTranDoc.curyTranTotal>(row, row.CuryTranTotal, null);
			}

			if (this._isCacheSync == false && row.IsChildTran == false)  //Add conditions for payments here
			{
				if (row.TranModule == GL.BatchModule.AP)
				{
					GLTranDocAP apRow = (GLTranDocAP)this.APPayments.Search<GLTranDocAP.module, GLTranDocAP.batchNbr, GLTranDocAP.lineNbr>(row.Module, row.BatchNbr, row.LineNbr);
					if (apRow != null)
					{
						sender.RestoreCopy(apRow, row);
						this.APPayments.Update(apRow);
					}
				}
				if (row.TranModule == GL.BatchModule.AR)
				{
					GLTranDocAR arRow = (GLTranDocAR)this.ARPayments.Search<GLTranDocAR.module, GLTranDocAR.batchNbr, GLTranDocAR.lineNbr>(row.Module, row.BatchNbr, row.LineNbr);
					if (arRow != null)
					{
						sender.RestoreCopy(arRow, row);
						this.ARPayments.Update(arRow);
					}
				}
			}
			if (this._cashAccountDebit != null)
			{
				if (IsMatching(this._cashAccountDebit, row.BranchID, row.DebitAccountID, row.DebitSubID))
				{
					row.DebitCashAccountID = this._cashAccountDebit.CashAccountID;
				}
			}
			if (this._cashAccountCredit != null)
			{
				if (IsMatching(this._cashAccountCredit, row.BranchID, row.CreditAccountID, row.CreditSubID))
				{
					row.CreditCashAccountID = this._cashAccountCredit.CashAccountID;
				}
			}
		}

		protected virtual void GLTranDoc_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			GLTranDoc row = (GLTranDoc)e.Row;
			bool needsRefresh = false;
			if (row.IsChildTran && HasDocumentRow(row) && !this.IsCopyPasteContext)
			{
				sender.RaiseFieldUpdated<GLTranDoc.curyTranAmt>(row, Decimal.Zero);
				GLTranDoc parent = this.FindParent(row);
				if (parent != null)
				{
					Decimal? newValue = (parent.CuryTranAmt + row.CuryTranAmt);
					GLTranDoc copy = (GLTranDoc)sender.CreateCopy(parent);
					copy.CuryTranAmt = newValue;
					object savedCurrent = sender.Current;
					sender.Update(copy);
					sender.Current = savedCurrent;
				}
				needsRefresh = true;
			}
			if (row.IsChildTran == false && String.IsNullOrEmpty(row.RefNbr))
			{
				try
				{
					CreateRefNbr(row);
				}
				catch (PXManualNumberingException ex)
				{
					sender.RaiseExceptionHandling<GLTranDoc.refNbr>(row, row.RefNbr, new PXSetPropertyException(ex.MessageNoPrefix, PXErrorLevel.RowError));
				}
				needsRefresh = true;
			}
			if (needsRefresh)
			{
				this.GLTranModuleBatNbr.View.RequestRefresh();
			}

			if (this._cashAccountDebit != null)
			{
				if (IsMatching(this._cashAccountDebit, row.BranchID, row.DebitAccountID, row.DebitSubID))
				{
					row.DebitCashAccountID = this._cashAccountDebit.CashAccountID;
				}
			}
			if (this._cashAccountCredit != null)
			{
				if (IsMatching(this._cashAccountCredit, row.BranchID, row.CreditAccountID, row.CreditSubID))
				{
					row.CreditCashAccountID = this._cashAccountCredit.CashAccountID;
				}
			}
		}

		protected virtual void GLTranDoc_RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
			GLTranDoc row = (GLTranDoc)e.Row;
			if (row != null)
			{
				if (row.IsChildTran && HasDocumentRow(row))
				{
					if (this._isMassDelete == false)
					{
						GLTranDoc parent = this.FindParent(row);
						if (parent != null)
						{
							Decimal? newValue = (parent.CuryTranAmt - row.CuryTranAmt);
							GLTranDoc copy = (GLTranDoc)sender.CreateCopy(parent);
							copy.CuryTranAmt = newValue;
							sender.Update(copy);
						}
					}
					this.GLTranModuleBatNbr.View.RequestRefresh();
				}
				else
				{
					if (row.IsChildTran == false)
					{
						DeleteChildren(row);
						this.DeleteDocRef(row);
						if (this._isCacheSync == false)
						{
							if (row.TranModule == GL.BatchModule.AP)
							{
								GLTranDocAP apRow = (GLTranDocAP)this.APPayments.Search<GLTranDocAP.module, GLTranDocAP.batchNbr, GLTranDocAP.lineNbr>(row.Module, row.BatchNbr, row.LineNbr);
								if (apRow != null)
								{
									this.APPayments.Delete(apRow);
								}
							}
							if (row.TranModule == GL.BatchModule.AR)
							{
								GLTranDocAR arRow = (GLTranDocAR)this.ARPayments.Search<GLTranDocAR.module, GLTranDocAR.batchNbr, GLTranDocAR.lineNbr>(row.Module, row.BatchNbr, row.LineNbr);
								if (arRow != null)
								{
									this.ARPayments.Delete(arRow);
								}
							}
						}
					}
				}
			}
		}

		protected virtual void GLTranDoc_RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
		{
			GLTranDoc row = (GLTranDoc)e.Row;
			if (row != null && String.IsNullOrEmpty(row.RefNbr) == false && row.DocCreated == true)
			{
				throw new PXException(Messages.RowMayNotBeDeletedItReferesExistsingDocument);
			}
		}

		protected virtual void GLTranDoc_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			GLTranDoc row = (GLTranDoc)e.Row;
			GLDocBatch batch = this.BatchModule.Current;
			if (e.Operation == PXDBOperation.Insert || e.Operation == PXDBOperation.Update)
			{
				VerifyDebitAccountToBeControl(sender, e, row);
				VerifyAccountIDToBeNoControl<GLTranDoc.debitAccountID>(sender, e, row, true);
				VerifyCreditAccountToBeControl(sender, e, row);
				VerifyAccountIDToBeNoControl<GLTranDoc.creditAccountID>(sender, e, row, false);

				if (row.IsChildTran == false)
				{
					foreach (GLTranDoc duplicateDoc in PXSelect<GLTranDoc,
						Where<GLTranDoc.tranModule, Equal<Required<GLTranDoc.tranModule>>,
						And<GLTranDoc.tranType, Equal<Required<GLTranDoc.tranType>>,
						And<GLTranDoc.refNbr, Equal<Required<GLTranDoc.refNbr>>,
						And<GLTranDoc.parentLineNbr, IsNull>>>>>.Select(this, row.TranModule, row.TranType, row.RefNbr))
					{
						if (duplicateDoc.Module != row.Module || duplicateDoc.BatchNbr != row.BatchNbr || duplicateDoc.LineNbr != row.LineNbr)
						{
							PXException exc = new PXException(Messages.ReenterRequired);
							sender.RaiseExceptionHandling<GLTranDoc.refNbr>(row, null, exc);
							throw exc;
						}
					}
				}
				if (String.IsNullOrEmpty(row.TranType) == false)
				{
					bool isDebitRequired = true;
					bool isCreditRequired = true;
					bool? isDebit = IsDebitType(row, true);
					if (row.IsChildTran)
					{
						if (isDebit.HasValue)
						{
							isDebitRequired = isDebit.Value;
							isCreditRequired = !isDebit.Value;
						}
						else
						{
							if (row.DebitAccountID.HasValue == false && row.CreditAccountID.HasValue == false)
							{
								GLTranDoc parent = FindParent(row);
								isDebitRequired = parent.CreditAccountID.HasValue;
								isCreditRequired = parent.DebitAccountID.HasValue;
							}
							else
							{
								isDebitRequired = row.DebitAccountID.HasValue;
								isCreditRequired = row.CreditAccountID.HasValue;
							}
						}
					}
					else
					{
						bool hasChildren = row.Split.Value;
						if (isDebit.HasValue)
						{
							isDebitRequired = (isDebit == false) || (isDebit.Value && !hasChildren);
							isCreditRequired = isDebit.Value || (isDebit.Value == false && !hasChildren);
						}
						else
						{
							if (!hasChildren)
							{
								isDebitRequired = isCreditRequired = true;
							}
							else
							{
								isDebitRequired = !row.CreditAccountID.HasValue;
								isCreditRequired = !row.DebitAccountID.HasValue;
							}
						}
					}
					PXDefaultAttribute.SetPersistingCheck<GLTranDoc.debitAccountID>(sender, row, isDebitRequired ? PXPersistingCheck.Null : PXPersistingCheck.Nothing);
					PXDefaultAttribute.SetPersistingCheck<GLTranDoc.debitSubID>(sender, row, isDebitRequired ? PXPersistingCheck.Null : PXPersistingCheck.Nothing);
					PXDefaultAttribute.SetPersistingCheck<GLTranDoc.creditAccountID>(sender, row, isCreditRequired ? PXPersistingCheck.Null : PXPersistingCheck.Nothing);
					PXDefaultAttribute.SetPersistingCheck<GLTranDoc.creditSubID>(sender, row, isCreditRequired ? PXPersistingCheck.Null : PXPersistingCheck.Nothing);
				}

				bool needPaymentMethod = row.IsChildTran == false && (IsARPayment(row) || IsAPPayment(row));
				bool needBAccount = row.IsChildTran == false && (row.TranModule == GL.BatchModule.AP || row.TranModule == GL.BatchModule.AR);
				PXDefaultAttribute.SetPersistingCheck<GLTranDoc.paymentMethodID>(sender, e.Row, needPaymentMethod ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);
				bool isPMInstanceRequired = false;
				if (row.IsChildTran == false && IsARPayment(row))
				{
					if (String.IsNullOrEmpty(row.PaymentMethodID) == false)
					{
						PaymentMethod pm = PXSelect<PaymentMethod, Where<PaymentMethod.paymentMethodID, Equal<Required<PaymentMethod.paymentMethodID>>>>.Select(this, row.PaymentMethodID);
						isPMInstanceRequired = (pm.IsAccountNumberRequired == true);
					}
				}
				PXDefaultAttribute.SetPersistingCheck<GLTranDoc.pMInstanceID>(sender, e.Row, isPMInstanceRequired ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);
				PXDefaultAttribute.SetPersistingCheck<GLTranDoc.bAccountID>(sender, e.Row, needBAccount ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);
				PXDefaultAttribute.SetPersistingCheck<GLTranDoc.locationID>(sender, e.Row, needBAccount ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);
				PXDefaultAttribute.SetPersistingCheck<GLTranDoc.entryTypeID>(sender, e.Row, row.TranModule == GL.BatchModule.CA ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);
				bool validateExtRefNbr = false;
				bool validateTerms = false;
				if (row.IsChildTran == false)
				{
					switch (row.TranModule)
					{
						case GL.BatchModule.AP: validateExtRefNbr = (this.apsetup.Current.RequireVendorRef == true); break;
						case GL.BatchModule.AR: validateExtRefNbr = IsARPayment(row); break;
						case GL.BatchModule.CA: validateExtRefNbr = true; break;
					}

					if (row.TranModule != GL.BatchModule.GL)
					{
						if (row.CuryTranTotal != row.CuryDocTotal)
						{
							decimal diff = row.CuryTranTotal.Value - row.CuryDocTotal.Value;
							sender.RaiseExceptionHandling<GLTranDoc.curyTranTotal>(row, row.CuryTranTotal, new PXSetPropertyException(Messages.DocumentIsOutOfBalance, PXErrorLevel.Error, diff));
						}
					}
					else
					{
						if (row.Split == true)
						{
							decimal balance = GetSignedAmount(row);
							List<GLTranDoc> list = new List<GLTranDoc>();
							foreach (GLTranDoc iChild in PXSelect<GLTranDoc, Where<GLTranDoc.module, Equal<Required<GLTranDoc.module>>,
																				And<GLTranDoc.batchNbr, Equal<Required<GLTranDoc.batchNbr>>,
																					And<GLTranDoc.parentLineNbr, Equal<Required<GLTranDoc.parentLineNbr>>>>>>.Select(this, row.Module, row.BatchNbr, row.LineNbr))
							{
								if (iChild.ParentLineNbr == row.LineNbr)
								{
									balance += GetSignedAmount(iChild);
								}
							}
							if (balance != Decimal.Zero)
							{
								sender.RaiseExceptionHandling<GLTranDoc.curyTranTotal>(row, row.CuryTranTotal, new PXSetPropertyException(Messages.DocumentIsOutOfBalance, PXErrorLevel.Error, balance));
							}
						}
					}
					validateTerms = (IsAPInvoice(row) || IsARInvoice(row)) && !IsARCreditMemo(row);
				}



				PXDefaultAttribute.SetPersistingCheck<GLTranDoc.extRefNbr>(sender, e.Row, validateExtRefNbr ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);
				PXDefaultAttribute.SetPersistingCheck<GLTranDoc.termsID>(sender, e.Row, validateTerms ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);
				PXDefaultAttribute.SetPersistingCheck<GLTranDoc.dueDate>(sender, e.Row, validateTerms ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);
				PXDefaultAttribute.SetPersistingCheck<GLTranDoc.discDate>(sender, e.Row, validateTerms ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);
				PXDefaultAttribute.SetPersistingCheck<GLTranDoc.curyDiscAmt>(sender, e.Row, validateTerms ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);

				//Project logic validation
				if (batch != null && row != null && row.ProjectID != null && !ProjectDefaultAttribute.IsNonProject( row.ProjectID) && row.TaskID != null)//taskID for Contract is null.
				{
					bool? isGLBatch = row.TranModule == GL.BatchModule.GL;
					bool isCAEntry = row.TranModule == GL.BatchModule.CA;
					PXErrorLevel errLevel = PXErrorLevel.Error;
					Account account;
					if (row.CreditAccountID.HasValue && IsAccountValidForProject(row, row.CreditAccountID, false, out account) == false)
					{
						sender.RaiseExceptionHandling<GLTranDoc.creditAccountID>(e.Row, account.AccountCD, new PXSetPropertyException(PM.Messages.NoAccountGroup, errLevel, account.AccountCD));
					}

					if (row.DebitAccountID.HasValue && IsAccountValidForProject(row, row.DebitAccountID, true, out account) == false)
					{
						sender.RaiseExceptionHandling<GLTranDoc.debitAccountID>(e.Row, account.AccountCD, new PXSetPropertyException(PM.Messages.NoAccountGroup, errLevel, account.AccountCD));
					}

				}
			}
		}

		protected virtual void GLTranDoc_RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
		{
			GLTranDoc row = (GLTranDoc)e.Row;
			GLDocBatch batch = this.BatchModule.Current;
			if (((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert
					|| (e.Operation & PXDBOperation.Command) == PXDBOperation.Update) && (e.TranStatus == PXTranStatus.Open))
			{
				if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert && row.IsChildTran == false)
				{
					RestoreRefNbr(row);
				}
			}
		}

		protected virtual Account GetAccount(int? accountID)
		{
			if (accountID.HasValue == false)
			{
				return null;
			}

			return PXSelect<Account, Where<Account.accountID, Equal<Required<Account.accountID>>>>.Select(this, accountID);
		}

		protected bool IsAccountValidForProject(GLTranDoc row, int? accountID, bool isDebitAccount, out Account account)
		{
			bool isGLBatch = row.TranModule == GL.BatchModule.GL;
			bool isCAEntry = row.TranModule == GL.BatchModule.CA;
			account = null;
			if (accountID.HasValue == false)
			{
				return true;
			}
			if (((IsAPInvoice(row) || IsARInvoice(row) || isCAEntry) && IsDebitType(row) == isDebitAccount)
				|| ((IsAPPayment(row) || (IsARPayment(row)) && IsDebitType(row) == !isDebitAccount)
				|| isGLBatch == true))
			{
				account = PXSelect<Account, Where<Account.accountID, Equal<Required<Account.accountID>>>>.Select(this, accountID);
				if (account != null && account.AccountGroupID == null)
				{
					return false;
				}
			}
			return true;
		}

		protected virtual bool IsAccountValidForProject(GLTranDoc row, Account account, bool isDebitAccount)
		{
			bool isGLBatch = row.TranModule == GL.BatchModule.GL;
			bool isCAEntry = row.TranModule == GL.BatchModule.CA;

			if (account == null)
			{
				return true;
			}

			if (((IsAPInvoice(row) || IsARInvoice(row) || isCAEntry) && IsDebitType(row) == isDebitAccount)
				|| ((IsAPPayment(row) || (IsARPayment(row)) && IsDebitType(row) == !isDebitAccount)
				|| isGLBatch == true))
			{
				if (account.AccountGroupID == null)
				{
					return false;
				}
			}

			return true;
		}

		protected virtual bool IsActiveAccount(Account account)
		{
			return account?.Active ?? false;
		}

		private bool IsPendingValue<TField>(PXCache sender, PXFieldVerifyingEventArgs e)
			where TField : IBqlField
		{
			GLTranDoc row = e.Row as GLTranDoc;
			var valuePending = sender.GetValuePending<TField>(row);

			return string.IsNullOrEmpty(valuePending as string) || valuePending == e.NewValue;
		}

		protected virtual void VerifyAccount<TField>(PXCache sender, PXFieldVerifyingEventArgs e, bool isDebitAccount)
			where TField : IBqlField
		{
			GLTranDoc row = e.Row as GLTranDoc;
			GLDocBatch batch = BatchModule.Current;

			if (batch != null && row != null)
			{
				Account account = GetAccount((int?)e.NewValue);

				if (row.ProjectID != null && !ProjectDefaultAttribute.IsNonProject(row.ProjectID) && row.TaskID != null)//taskID for Contract is null.
				{
					if (!IsAccountValidForProject(row, account, isDebitAccount))
					{
						if (IsImport == true && IsPendingValue<TField>(sender, e))
						{
							throw new PXSetPropertyException(PM.Messages.NoAccountGroup, account.AccountCD);
						}
						else
						{
							PXErrorLevel errLevel = PXErrorLevel.Error;
							sender.RaiseExceptionHandling<TField>(e.Row, account.AccountCD, new PXSetPropertyException(PM.Messages.NoAccountGroup, errLevel, account.AccountCD));
						}
					}
				}

				if (account != null && !IsActiveAccount(account))
				{
					if (IsImport == true && IsPendingValue<TField>(sender, e))
					{
						throw new PXSetPropertyException(Messages.AccountInactive);
					}
					else
					{
						PXErrorLevel errLevel = PXErrorLevel.Error;
						sender.RaiseExceptionHandling<TField>(e.Row, account.AccountCD, new PXSetPropertyException(Messages.AccountInactive, errLevel));
					}
				}
			}

			e.Cancel = e.Cancel || (IsImport && !string.IsNullOrEmpty(sender.GetValuePending<TField>(row) as string));
		}

		protected void DeleteChildren(GLTranDoc aRow)
		{
			if (aRow != null && aRow.IsChildTran == false)
			{
				try
				{
					this._isMassDelete = true;
					foreach (GLTranDoc it in this.GLTranModuleBatNbr.SearchAll<Asc<GLTranDoc.parentLineNbr>>(new object[] { aRow.LineNbr }))
					{
						if (Object.ReferenceEquals(it, aRow) == false && aRow.LineNbr == it.ParentLineNbr)
						{
							this.GLTranModuleBatNbr.Delete(it);
						}
					}
				}
				finally
				{
					this._isMassDelete = false;
				}
			}
		}

		protected void DeleteDocRef(GLTranDoc aRow)
		{
			if (aRow != null && String.IsNullOrEmpty(aRow.RefNbr) == false)
			{
				RefDocKey key = new RefDocKey();
				key.Copy(aRow);
				this.deletedKeys.Insert(key);
			}
		}
		private GLTranDoc _parent = null;
		private GLTranDoc _previousTran = null;
		private CashAccount _cashAccountDebit = null;
		private CashAccount _cashAccountCredit = null;

		public GLTranDoc FindParent(GLTranDoc aTran)
		{
			if (aTran.ParentLineNbr == null)
			{
				return null;
			}
			if (this._parent == null || this._parent.LineNbr != aTran.ParentLineNbr)
			{
				this._parent = this.GLTranModuleBatNbr.Search<GLTranDoc.lineNbr>(aTran.ParentLineNbr);
			}
			return this._parent;
		}
		protected GLTranDoc FindPrevMasterTran(GLTranDoc aTran)
		{
			if (aTran.LineNbr == null || aTran.ParentLineNbr != null)
			{
				return null;
			}
			if (this._previousTran == null || this._previousTran.LineNbr.Value >= aTran.LineNbr.Value)
			{
				this._previousTran = PXSelect<GLTranDoc,
							Where<GLTranDoc.module, Equal<Required<GLTranDoc.module>>,
										And<GLTranDoc.batchNbr, Equal<Required<GLTranDoc.batchNbr>>,
										And<GLTranDoc.parentLineNbr, IsNull,
										And<GLTranDoc.lineNbr, Less<Required<GLTranDoc.lineNbr>>>>>>,
										OrderBy<Desc<GLTranDoc.lineNbr>>>.Select(this, aTran.Module, aTran.BatchNbr, aTran.LineNbr);
			}
			return this._previousTran;
		}
		protected GLTranDoc FindPrevSibling(GLTranDoc aTran)
		{
			GLTranDoc prev = null;
			if (aTran.LineNbr == null || aTran.ParentLineNbr == null)
			{
				return null;
			}
			prev = PXSelect<GLTranDoc,
							Where<GLTranDoc.module, Equal<Required<GLTranDoc.module>>,
													And<GLTranDoc.batchNbr, Equal<Required<GLTranDoc.batchNbr>>,
														And<GLTranDoc.parentLineNbr, Equal<Required<GLTranDoc.parentLineNbr>>,
								And<GLTranDoc.lineNbr, Less<Required<GLTranDoc.lineNbr>>>>>>,
							OrderBy<Desc<GLTranDoc.lineNbr>>>.Select(this, aTran.Module, aTran.BatchNbr, aTran.ParentLineNbr, aTran.LineNbr);
			return prev;
		}
		protected virtual int? FindDefaultAccount(GLTranDoc aRow, bool isCredit)
		{
			int? result = null;
			this.SetCashAccount(null, isCredit);

			if (string.IsNullOrEmpty(aRow.TranModule) || string.IsNullOrEmpty(aRow.TranType))
			{
				return null;
			}
			if (aRow.TranModule == GL.BatchModule.AP)
			{
				if (aRow.BAccountID == null || aRow.LocationID == null)
				{
					return null;
				}
				Location location = this.Location.Select(aRow.BAccountID, aRow.LocationID);
				return FindDefaultAPAccount(aRow, isCredit, location);
			}

			if (aRow.TranModule == GL.BatchModule.AR)
			{
				if (aRow.BAccountID == null || aRow.LocationID == null)
				{
					return null;
				}
				Location location = this.Location.Select(aRow.BAccountID, aRow.LocationID);
				return FindDefaultARAccount(aRow, isCredit, location);
			}

			if (aRow.TranModule == GL.BatchModule.CA)
			{
				return FindDefaultCAAccount(aRow, isCredit);
			}
			return result;
		}

		#region Cash Account Helpers
		private void SetCashAccount(CashAccount cashAccount, bool isCredit)
		{
			if (isCredit)
			{
				this._cashAccountCredit = cashAccount;
			}
			else
			{
				this._cashAccountDebit = cashAccount;
			}
		}

		protected bool HasCashAccount(int branch, int account, string entryTypeID, out bool single)
		{
			CashAccount cashAcct = GetCashAccount(branch, account, entryTypeID, out single);
			return cashAcct != null;
		}

		protected CashAccount GetCashAccount(int branch, int account, string entryTypeID, out bool single)
		{
			int subCount = 0;
			single = true;
			CashAccount result = null;

			foreach (CashAccount iCash in this.cashAccountByAccountID.Select(entryTypeID, account, branch, entryTypeID))
			{
				if (iCash != null && iCash.CashAccountID.HasValue)
				{
					if (result == null)
					{
						result = iCash;
					}
					if (subCount > 0)
					{
						single = false;
						break;
					}
					subCount++;
				}
			}
			return result;
		}

		protected virtual CashAccount GetCashAccount(int aBranchID, int aAccountID, int aSubID, string entryTypeID, out bool cashExistForAccount)
		{
			cashExistForAccount = false;
			CashAccount result = null;
			foreach (CashAccount it in PXSelect<CashAccount,
							Where<CashAccount.branchID, Equal<Required<CashAccount.branchID>>,
							And<CashAccount.accountID, Equal<Required<CashAccount.accountID>>>>>.Select(this, aBranchID, aAccountID))
			{
				cashExistForAccount = true;
				if (it.SubID == aSubID)
				{
					result = it;
					break;
				}
			}
			return result;
		}

		protected virtual bool IsMatching(CashAccount aCashAccount, int? branchID, int? accountID, int? subID)
		{
			if (aCashAccount != null && branchID.HasValue && accountID.HasValue && subID.HasValue)
			{
				return (aCashAccount.BranchID == branchID && aCashAccount.AccountID == accountID && aCashAccount.SubID == subID);
			}
			return false;
		}
		#endregion

		#region AP Account Defaulting Helpers
		protected virtual int? FindDefaultAPAccount(GLTranDoc aRow, bool isCredit, Location location)
		{
			switch (aRow.TranType)
			{
				case AP.APInvoiceType.Invoice:
				case AP.APInvoiceType.CreditAdj:
					return (isCredit ? GetDefaultAPPayableAccount(aRow, location) : GetDefaultAPExpenceAccount(aRow, location));
				case AP.APInvoiceType.DebitAdj:
					return (isCredit == false ? GetDefaultAPPayableAccount(aRow, location) : GetDefaultAPExpenceAccount(aRow, location));
				case AP.APInvoiceType.QuickCheck:
					if (isCredit)
					{
						CashAccount cashAccount = FindDefaultAPCashAccount(aRow.PaymentMethodID, location, aRow.BranchID);
						if (cashAccount != null)
						{
							this.SetCashAccount(cashAccount, isCredit);
							return cashAccount.AccountID;
						}
						else
						{
							return null;
					}
					}
					else
					{
						return GetDefaultAPExpenceAccount(aRow, location);
					}
				case AP.APInvoiceType.VoidQuickCheck:
					if (isCredit == false)
					{
						CashAccount cashAccount = FindDefaultAPCashAccount(aRow.PaymentMethodID, location, aRow.BranchID);
						if (cashAccount != null)
						{
							this.SetCashAccount(cashAccount, isCredit);
							return cashAccount.AccountID;
						}
						else
						{
							return null;
					}
					}
					else
					{
						return GetDefaultAPExpenceAccount(aRow, location);
					}
				case AP.APPaymentType.Prepayment:
				case AP.APPaymentType.Check:
					if (isCredit)
					{
						CashAccount cashAccount = FindDefaultAPCashAccount(aRow.PaymentMethodID, location, aRow.BranchID);
						if (cashAccount != null)
						{
							this.SetCashAccount(cashAccount, isCredit);
							return cashAccount.AccountID;
						}
						else
						{
							return null;
					}
					}
					else
					{
						return GetDefaultAPPayableAccount(aRow, location);
					}
				case AP.APPaymentType.Refund:
					if (isCredit == false)
					{
						CashAccount cashAccount = FindDefaultAPCashAccount(aRow.PaymentMethodID, location, aRow.BranchID);
						if (cashAccount != null)
						{
							this.SetCashAccount(cashAccount, isCredit);
							return cashAccount.AccountID;
						}
						else
						{
							return null;
					}
					}
					else
					{
						return GetDefaultAPPayableAccount(aRow, location);
					}
			}
			return null;
		}
		protected virtual int? GetDefaultAPPayableAccount(GLTranDoc aRow, Location location)
		{
			if (aRow.TranType == AP.APDocType.Prepayment)
			{
				Vendor vendor = Vendor.Select(aRow.BAccountID);
				if (vendor != null && vendor.PrepaymentAcctID != null)
				{
					return vendor.PrepaymentAcctID;
			}
			}

		    bool IsAPAccountSameAsMain = !object.Equals(location.LocationID, location.VAPAccountLocationID);

            if (IsAPAccountSameAsMain)
            {
                Location mainLocation = PXSelectJoin<Location, InnerJoin<Vendor, On<Vendor.defLocationID, Equal<Location.locationID>>>,
                                        Where<Vendor.bAccountID, Equal<Required<Location.bAccountID>>>>.Select(this, location.BAccountID);

                return mainLocation.VAPAccountID;
            }
            else
			{
				return location.VAPAccountID;
			}
		}

		protected virtual int? GetDefaultAPExpenceAccount(GLTranDoc aRow, Location location)
		{
			if (location != null)
			{
				return location.VExpenseAcctID;
			}
			return null;
		}
		protected virtual CashAccount FindDefaultAPCashAccount(string aPaymentMethodID, Location aLocation, int? aBranchID)
		{
			if (aLocation != null && aLocation.VPaymentMethodID == aPaymentMethodID && aLocation.VCashAccountID.HasValue)
			{
				return PXSelect<CashAccount, Where<CashAccount.cashAccountID, Equal<Required<CashAccount.cashAccountID>>>>.Select(this, aLocation.VCashAccountID);
			}
			else if (String.IsNullOrEmpty(aPaymentMethodID) == false)
			{
				CashAccount acct = PXSelectJoin<CashAccount, InnerJoin<PaymentMethodAccount,
														On<PaymentMethodAccount.cashAccountID, Equal<CashAccount.cashAccountID>,
															And<PaymentMethodAccount.useForAP, Equal<True>>>>,
														 Where<PaymentMethodAccount.paymentMethodID, Equal<Required<PaymentMethodAccount.paymentMethodID>>,
															And<CashAccount.branchID, Equal<Required<CashAccount.branchID>>,
															And<PaymentMethodAccount.aPIsDefault, Equal<True>,
																And<Match<Current<AccessInfo.userName>>>>>>>.Select(this, aPaymentMethodID, aBranchID);
				return acct;
			}
			return null;
		}
		#endregion

		#region AR Account Defaulting Helpers
		protected virtual int? FindDefaultARAccount(GLTranDoc aRow, bool isCredit, Location location)
		{
			switch (aRow.TranType)
			{
				case AR.ARInvoiceType.Invoice:
				case AR.ARInvoiceType.DebitMemo:
					return (isCredit == false ? GetDefaultARReceivableAccount(aRow, location) : GetDefaultARSalesAccount(aRow, location));
				case AR.ARInvoiceType.CreditMemo:
					return (isCredit ? GetDefaultARReceivableAccount(aRow, location) : GetDefaultARSalesAccount(aRow, location));
				case AR.ARInvoiceType.CashSale:
					if (isCredit == false)
					{
						CashAccount cashAccount = FindDefaultARCashAccount(aRow.PaymentMethodID, aRow.PMInstanceID);
						if (cashAccount != null)
						{
							this.SetCashAccount(cashAccount, isCredit);
							return cashAccount.AccountID;
						}
						else
						{
							return null;
					}
					}
					else
					{
						return GetDefaultARSalesAccount(aRow, location);
					}
				case AR.ARInvoiceType.CashReturn:
					if (isCredit)
					{
						CashAccount cashAccount = FindDefaultARCashAccount(aRow.PaymentMethodID, aRow.PMInstanceID);
						if (cashAccount != null)
						{
							this.SetCashAccount(cashAccount, isCredit);
							return cashAccount.AccountID;
						}
						else
						{
							return null;
					}
					}
					else
					{
						return GetDefaultARSalesAccount(aRow, location);
					}
				case AR.ARPaymentType.Prepayment:
				case AR.ARPaymentType.Payment:
					if (isCredit == false)
					{
						CashAccount cashAccount = FindDefaultARCashAccount(aRow.PaymentMethodID, aRow.PMInstanceID);
						if (cashAccount != null)
						{
							this.SetCashAccount(cashAccount, isCredit);
							return cashAccount.AccountID;
						}
						else
						{
							return null;
					}
					}
					else
					{
						return GetDefaultARReceivableAccount(aRow, location);
					}
				case AR.ARPaymentType.Refund:
					if (isCredit)
					{
						CashAccount cashAccount = FindDefaultARCashAccount(aRow.PaymentMethodID, aRow.PMInstanceID);
						if (cashAccount != null)
						{
							this.SetCashAccount(cashAccount, isCredit);
							return cashAccount.AccountID;
						}
						else
						{
							return null;
					}
					}
					else
					{
						return GetDefaultARReceivableAccount(aRow, location);
					}
			}
			return null;
		}
		protected virtual int? GetDefaultARReceivableAccount(GLTranDoc aRow, Location location)
		{
			if (aRow.TranType == AR.ARDocType.Prepayment)
			{
				Customer customer = Customer.Select(aRow.BAccountID);
				if (customer != null && customer.PrepaymentAcctID != null)
				{
					return customer.PrepaymentAcctID;
			}
			}

            bool IsARAccountSameAsMain = !object.Equals(location.LocationID, location.CARAccountLocationID);

            if (IsARAccountSameAsMain)
            {
                Location mainLocation = PXSelectJoin<Location, InnerJoin<Customer, On<Customer.defLocationID, Equal<Location.locationID>>>, 
                                        Where<Customer.bAccountID, Equal<Required<Location.bAccountID>>>>.Select(this, location.BAccountID);

                return mainLocation.CARAccountID;
		    }
            else
            {
				return location.CARAccountID;
		    }
		}
		protected virtual int? GetDefaultARSalesAccount(GLTranDoc aRow, Location location)
		{
			if (location != null)
			{
				return location.CSalesAcctID;
			}
			return null;
		}
		protected virtual CashAccount FindDefaultARCashAccount(string aPaymentMethodID, int? aPMInstanceID)
		{
			if (string.IsNullOrEmpty(aPaymentMethodID))
			{
				return null;
			}
			CashAccount cashAcct = null;
			if (aPMInstanceID.HasValue)
			{
				cashAcct = PXSelectJoin<CashAccount, InnerJoin<CustomerPaymentMethod, On<CashAccount.cashAccountID, Equal<CustomerPaymentMethod.cashAccountID>>>,
										Where<CustomerPaymentMethod.pMInstanceID, Equal<Required<CustomerPaymentMethod.pMInstanceID>>,
											   And<CustomerPaymentMethod.isActive, Equal<True>>>>.Select(this, aPMInstanceID.Value);

			}
			if (cashAcct == null)
			{
				cashAcct = PXSelectJoin<CashAccount,
											InnerJoin<PaymentMethodAccount, On<CashAccount.cashAccountID, Equal<PaymentMethodAccount.cashAccountID>>,
											InnerJoin<PaymentMethod, On<PaymentMethod.paymentMethodID, Equal<PaymentMethodAccount.paymentMethodID>>>>,
											Where<PaymentMethodAccount.paymentMethodID, Equal<Required<CustomerPaymentMethod.paymentMethodID>>,
											And<PaymentMethodAccount.useForAR, Equal<True>,
												And<PaymentMethod.useForAR, Equal<True>>>>, OrderBy<Desc<PaymentMethodAccount.aRIsDefault>>>.Select(this, aPaymentMethodID);
			}
			return cashAcct;
		}
		#endregion

		#region CA Account Defaulting Helpers
		protected virtual int? FindDefaultCAAccount(GLTranDoc aRow, bool isCredit)
		{
			if (string.IsNullOrEmpty(aRow.EntryTypeID) == false)
			{
				CAEntryType entryType = PXSelect<CAEntryType, Where<CAEntryType.entryTypeId, Equal<Required<CAEntryType.entryTypeId>>>>.Select(this, aRow.EntryTypeID);
				bool isDebit = (entryType.DrCr == CADrCr.CADebit);
				//int? cashAccountID = aRow.IsChildTran? this.FindParent(aRow).CashAccountID : aRow.CashAccountID ;
				int? cashAccountID = null;
				if (aRow.IsChildTran == false)
				{
					cashAccountID = isDebit ? aRow.DebitCashAccountID : aRow.CreditCashAccountID;
				}
				else
				{
					GLTranDoc parent = this.FindParent(aRow);
					cashAccountID = isDebit ? parent.DebitCashAccountID : parent.CreditCashAccountID;
				}

				if (isDebit == isCredit)
				{
					if (cashAccountID.HasValue)
					{
						CashAccountETDetail etDetail = GetCashAccountETDetail(entryType.EntryTypeId, cashAccountID);
						if (etDetail != null && etDetail.OffsetAccountID.HasValue)
						{
							return etDetail.OffsetAccountID;
					}
					}
					return entryType.AccountID;
				}
			}
			return null;
		}
		#endregion

		protected virtual int? FindDefaultSubAccount(GLTranDoc aRow, bool isCredit)
		{
			int? result = null;
			if (string.IsNullOrEmpty(aRow.TranModule) || string.IsNullOrEmpty(aRow.TranType))
			{
				return null;
			}

			int? keyAccount = isCredit ? aRow.CreditAccountID : aRow.DebitAccountID;
			CashAccount cashAccount = isCredit ? this._cashAccountCredit : this._cashAccountDebit;

			if (keyAccount == null)
			{
				return null;
			}
			if (cashAccount != null && keyAccount != cashAccount.AccountID)
			{
				return null;
			}

			bool needsCashSub = (keyAccount.HasValue && cashAccount != null && keyAccount == cashAccount.AccountID);
			if (needsCashSub)
			{
				return (cashAccount != null) ? cashAccount.SubID : null;
			}

			if (aRow.TranModule == GL.BatchModule.AP)
			{
				if (aRow.BAccountID == null || aRow.LocationID == null)
				{
					return null;
				}
				Location location = this.Location.Select(aRow.BAccountID, aRow.LocationID);
				return FindDefaultAPSubID(aRow, isCredit, location);
			}

			if (aRow.TranModule == GL.BatchModule.AR)
			{
				if (aRow.BAccountID == null || aRow.LocationID == null)
				{
					return null;
				}
				Location location = this.Location.Select(aRow.BAccountID, aRow.LocationID);
				return FindDefaultARSubID(aRow, isCredit, location);
			}

			if (aRow.TranModule == GL.BatchModule.CA)
			{
				return FindDefaultCASubID(aRow, isCredit);

			}
			if (aRow.TranModule == GL.BatchModule.GL)
			{
				return FindDefaultGLSubID(aRow, isCredit, keyAccount);
			}
			return result;
		}

		#region AR Sub Defaulting Helpers
		protected virtual int? FindDefaultAPSubID(GLTranDoc aRow, bool isCredit, Location location)
		{
			switch (aRow.TranType)
			{
				case AP.APInvoiceType.Invoice:
				case AP.APInvoiceType.CreditAdj:
					return (isCredit ? GetDefaultAPPayableSubID(aRow, location) : GetDefaultAPExpenceSubID(aRow, location));
				case AP.APInvoiceType.DebitAdj:
					return (isCredit == false ? GetDefaultAPPayableSubID(aRow, location) : GetDefaultAPExpenceSubID(aRow, location));
				case AP.APInvoiceType.QuickCheck:
					return (isCredit ? null : GetDefaultAPExpenceSubID(aRow, location));
				case AP.APInvoiceType.VoidQuickCheck:
					return (isCredit == false ? null : GetDefaultAPExpenceSubID(aRow, location));
				case AP.APPaymentType.Prepayment:
				case AP.APPaymentType.Check:
					return (isCredit ? null : GetDefaultAPPayableSubID(aRow, location));
				case AP.APPaymentType.Refund:
					return (isCredit == false ? null : GetDefaultAPPayableSubID(aRow, location));
			}
			return null;
		}
		protected virtual int? GetDefaultAPPayableSubID(GLTranDoc aRow, Location location)
		{
			if (aRow.TranType == AP.APDocType.Prepayment)
			{
				Vendor vendor = Vendor.Select(aRow.BAccountID);
				if (vendor != null && vendor.PrepaymentSubID != null)
				{
					return vendor.PrepaymentSubID;
			}
			}

            bool IsAPAccountSameAsMain = !object.Equals(location.LocationID, location.VAPAccountLocationID);

            if (IsAPAccountSameAsMain)
            {
                Location mainLocation = PXSelectJoin<Location, InnerJoin<Vendor, On<Vendor.defLocationID, Equal<Location.locationID>>>,
                                        Where<Vendor.bAccountID, Equal<Required<Location.bAccountID>>>>.Select(this, location.BAccountID);

                return mainLocation.VAPSubID;
            }
            else
			{
				return location.VAPSubID;
			}
		}

		protected virtual int? GetDefaultAPExpenceSubID(GLTranDoc aRow, Location location)
		{
			if (location != null)
			{
				return location.VExpenseSubID;
			}
			return null;
		}

		#endregion

		#region AR Sub Defaulting Helpers
		protected virtual int? FindDefaultARSubID(GLTranDoc aRow, bool isCredit, Location location)
		{
			switch (aRow.TranType)
			{
				case AR.ARInvoiceType.Invoice:
				case AR.ARInvoiceType.DebitMemo:
					return (isCredit == false ? GetDefaultARReceivableSubID(aRow, location) : GetDefaultARSalesSubID(aRow, location));
				case AR.ARInvoiceType.CreditMemo:
					return (isCredit ? GetDefaultARReceivableSubID(aRow, location) : GetDefaultARSalesSubID(aRow, location));
				case AR.ARInvoiceType.CashSale:
					return (isCredit == false ? null : GetDefaultARSalesSubID(aRow, location));
				case AR.ARInvoiceType.CashReturn:
					return (isCredit ? null : GetDefaultARSalesSubID(aRow, location));
				case AR.ARPaymentType.Prepayment:
				case AR.ARPaymentType.Payment:
					return (isCredit == false ? null : GetDefaultARReceivableSubID(aRow, location));
				case AR.ARPaymentType.Refund:
					return (isCredit ? null : GetDefaultARReceivableSubID(aRow, location));
			}
			return null;
		}
		protected virtual int? GetDefaultARReceivableSubID(GLTranDoc aRow, Location location)
		{
			if (aRow.TranType == AR.ARDocType.Prepayment)
			{
				Customer customer = Customer.Select(aRow.BAccountID);
				if (customer != null && customer.PrepaymentSubID != null)
				{
					return customer.PrepaymentSubID;
			}
			}

            bool IsARAccountSameAsMain = !object.Equals(location.LocationID, location.CARAccountLocationID);

            if (IsARAccountSameAsMain)
            {
                Location mainLocation = PXSelectJoin<Location, InnerJoin<Customer, On<Customer.defLocationID, Equal<Location.locationID>>>,
                                        Where<Customer.bAccountID, Equal<Required<Location.bAccountID>>>>.Select(this, location.BAccountID);

                return mainLocation.CARSubID;
            }
            else
			{
				return location.CARSubID;
			}
		}
		protected virtual int? GetDefaultARSalesSubID(GLTranDoc aRow, Location location)
		{
			if (location != null)
			{
				return location.CSalesSubID;
			}
			return null;
		}
		#endregion

		#region Other Sub Defaulting Helpers
		protected virtual int? FindDefaultCASubID(GLTranDoc aRow, bool isCredit)
		{
			if (string.IsNullOrEmpty(aRow.EntryTypeID) == false)
			{
				CAEntryType entryType = PXSelect<CAEntryType, Where<CAEntryType.entryTypeId, Equal<Required<CAEntryType.entryTypeId>>>>.Select(this, aRow.EntryTypeID);
				bool isDebit = (entryType.DrCr == CADrCr.CADebit);
				//int? cashAccountID = aRow.IsChildTran ? this.FindParent(aRow).CashAccountID : aRow.CashAccountID;
				int? cashAccountID = null;
				if (aRow.IsChildTran == false)
				{
					cashAccountID = isDebit ? aRow.DebitAccountID : aRow.CreditAccountID;
				}
				else
				{
					GLTranDoc parent = this.FindParent(aRow);
					cashAccountID = isDebit ? parent.DebitAccountID : parent.CreditAccountID;
				}
				if (isDebit == isCredit)
				{
					if (cashAccountID.HasValue)
					{
						CashAccountETDetail etDetail = GetCashAccountETDetail(entryType.EntryTypeId, cashAccountID);
						if (etDetail != null && etDetail.OffsetSubID.HasValue)
						{
							return etDetail.OffsetSubID;
					}
					}
					return entryType.SubID;
				}

			}
			return null;
		}
		protected virtual int? FindDefaultGLSubID(GLTranDoc aRow, bool isCredit, int? keyAccount)
		{
			int? result = null;
			CashAccount acct = this.cashAccount.Select(keyAccount);
			if (acct != null)
			{
				return acct.SubID;
			}
			int? complementarySub = null;
			GLTranDoc parent = aRow;
			if (aRow.IsChildTran)
			{
				parent = this.FindParent(aRow);
			}

			if (parent != null)
			{
				complementarySub = isCredit ? parent.DebitSubID : parent.CreditSubID;
			}
			result = complementarySub;
			Account account = (Account)PXSelect<Account, Where<Account.accountID, Equal<Required<Account.accountID>>>>.Select(this, keyAccount);

			if (account != null && (bool)account.NoSubDetail && glsetup.Current.DefaultSubID != null)
			{
				return glsetup.Current.DefaultSubID;
			}
			return result;
		}
		#endregion
		#endregion

		#region GLTranDocAP

		protected virtual void GLTranDocAP_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			GLTranDoc row = (GLTranDoc)e.Row;
			GLTranDoc oldRow = (GLTranDoc)e.OldRow;
			if (row.CuryApplAmt != oldRow.CuryApplAmt)
			{
				GLTranDoc src = (GLTranDoc)this.GLTranModuleBatNbr.Search<GLTranDoc.module, GLTranDoc.batchNbr, GLTranDoc.lineNbr>(row.Module, row.BatchNbr, row.LineNbr);
				if (src != null)
				{
					GLTranDoc copy = (GLTranDoc)this.GLTranModuleBatNbr.Cache.CreateCopy(src);
					copy.CuryApplAmt = row.CuryApplAmt;
					copy.ApplAmt = row.ApplAmt;
					try
					{
						this._isCacheSync = true;
						this.GLTranModuleBatNbr.Update(copy);
					}
					finally
					{
						this._isCacheSync = false;
					}
				}
			}
		}
		protected virtual void GLTranDocAP_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			e.Cancel = true;
		}
		protected virtual void GLTranDocAP_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			GLTranDocAP row = (GLTranDocAP)e.Row;
			if (row != null && row.Released == false)
			{
				bool isReadyForRelease = (this.BatchModule.Current != null && this.BatchModule.Current.Hold == false);

				if (isReadyForRelease && row.CuryUnappliedBal != Decimal.Zero && row.TranType != APDocType.Prepayment)
				{
					sender.RaiseExceptionHandling<GLTranDocAP.curyUnappliedBal>(row, row.CuryUnappliedBal, new PXSetPropertyException(Messages.DocumentMustByAppliedInFullBeforeItMayBeReleased, PXErrorLevel.Error));
				}
				else
				{
					sender.RaiseExceptionHandling<GLTranDocAP.curyUnappliedBal>(row, row.CuryUnappliedBal, null);
				}
			}
			bool allowModifications = (row != null) && (row.Released == false);
			this.APAdjustments.Cache.AllowInsert = allowModifications;
			this.APAdjustments.Cache.AllowUpdate = allowModifications;
			this.APAdjustments.Cache.AllowDelete = allowModifications;
			if (row != null)
			{
				APPaymentEntry.SetDocTypeList(this.APAdjustments.Cache, row.TranType);
			}
		}
		#endregion

		#region GLTranDocAR

		protected virtual void GLTranDocAR_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			GLTranDoc row = (GLTranDoc)e.Row;
			GLTranDoc oldRow = (GLTranDoc)e.OldRow;
			if (row.CuryApplAmt != oldRow.CuryApplAmt)
			{
				GLTranDoc src = (GLTranDoc)this.GLTranModuleBatNbr.Search<GLTranDoc.module, GLTranDoc.batchNbr, GLTranDoc.lineNbr>(row.Module, row.BatchNbr, row.LineNbr);
				if (src != null)
				{
					GLTranDoc copy = (GLTranDoc)this.GLTranModuleBatNbr.Cache.CreateCopy(src);
					copy.CuryApplAmt = row.CuryApplAmt;
					copy.ApplAmt = row.ApplAmt;
					try
					{
						this._isCacheSync = true;
						this.GLTranModuleBatNbr.Update(copy);
					}
					finally
					{
						this._isCacheSync = false;
					}
				}
			}
		}
		protected virtual void GLTranDocAR_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			e.Cancel = true;
		}
		protected virtual void GLTranDocAR_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			GLTranDocAR row = (GLTranDocAR)e.Row;
			if (row != null && row.Released == false)
			{
				bool isReadyForRelease = (this.BatchModule.Current != null && this.BatchModule.Current.Hold == false);
				if (isReadyForRelease && row.CuryUnappliedBal != Decimal.Zero)
				{
					sender.RaiseExceptionHandling<GLTranDocAR.curyUnappliedBal>(row, row.CuryUnappliedBal, new PXSetPropertyException(Messages.DocumentIsNotAppliedInFull, PXErrorLevel.Warning));
				}
				else
				{
					sender.RaiseExceptionHandling<GLTranDocAR.curyUnappliedBal>(row, row.CuryUnappliedBal, null);
				}
			}
			bool allowModifications = (row != null) && (row.Released == false);
			this.ARAdjustments.Cache.AllowInsert = allowModifications;
			this.ARAdjustments.Cache.AllowUpdate = allowModifications;
			this.ARAdjustments.Cache.AllowDelete = allowModifications;
			if (row != null)
			{
				ARPaymentEntry.SetDocTypeList(this.ARAdjustments.Cache, row.TranType);
			}
		}
		#endregion

		#region APAdjust

		protected virtual void APAdjust_AdjdDocType_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			if (e.Row != null)
			{
				sender.SetDefaultExt<APAdjust.adjdRefNbr>(e.Row);
			}
		}
		protected virtual void APAdjust_AdjdRefNbr_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			APAdjust row = (APAdjust)e.Row;
			GLTranDocAP payment = this.APPayments.Current;
			e.Cancel = (payment != null && (payment.TranType == APDocType.QuickCheck));
		}

		protected virtual void APAdjust_AdjdRefNbr_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			try
			{
				APAdjust adj = e.Row as APAdjust;
				if (adj.AdjdCuryInfoID == null)
				{
					foreach (PXResult<APInvoice, CurrencyInfo> res in PXSelectJoin<APInvoice, InnerJoin<CurrencyInfo, On<CurrencyInfo.curyInfoID, Equal<APInvoice.curyInfoID>>>,
							Where<APInvoice.vendorID, Equal<Current<GLTranDocAP.bAccountID>>,
							And<APInvoice.docType, Equal<Required<APInvoice.docType>>,
							And<APInvoice.refNbr, Equal<Required<APInvoice.refNbr>>>>>>.Select(this, adj.AdjdDocType, adj.AdjdRefNbr))
					{
						APInvoice doc = res;
						CurrencyInfo curyInfo = res;
						RegisterAdapter adapter = new RegisterAdapter(doc);
						APAdjust_AdjdRefNbr_FieldUpdated<RegisterAdapter>(adapter, curyInfo, adj);
						return;
					}

					foreach (PXResult<GLTranDoc, CurrencyInfo> res in PXSelectJoin<GLTranDoc, InnerJoin<CurrencyInfo, On<CurrencyInfo.curyInfoID, Equal<GLTranDoc.curyInfoID>>>,
											Where<GLTranDoc.batchNbr, Equal<Current<GLDocBatch.batchNbr>>,
											And<GLTranDoc.parentLineNbr, IsNull,
											And<GLTranDoc.tranModule, Equal<GL.BatchModule.moduleAP>,
											And<GLTranDoc.bAccountID, Equal<Required<GLTranDoc.bAccountID>>,
											And<GLTranDoc.tranType, Equal<Required<APAdjust.adjdDocType>>,
											And<GLTranDoc.refNbr, Equal<Required<GLTranDoc.refNbr>>>>>>>>>.Select(this, adj.VendorID, adj.AdjdDocType, adj.AdjdRefNbr))
					{
						GLTranDoc doc = res;
						CurrencyInfo curyInfo = res;
						RegisterAdapter adapter = new RegisterAdapter(doc);
						APAdjust_AdjdRefNbr_FieldUpdated<RegisterAdapter>(adapter, curyInfo, adj);
						return;
					}

					foreach (PXResult<APPayment, CurrencyInfo> res in PXSelectJoin<APPayment, InnerJoin<CurrencyInfo, On<CurrencyInfo.curyInfoID, Equal<APPayment.curyInfoID>>>,
							Where<APPayment.vendorID, Equal<Current<GLTranDocAP.bAccountID>>, And<APPayment.docType, Equal<Required<APPayment.docType>>, And<APPayment.refNbr, Equal<Required<APPayment.refNbr>>>>>>.Select(this, adj.AdjdDocType, adj.AdjdRefNbr))
					{
						APPayment doc = res;
						CurrencyInfo curyInfo = res;
						RegisterAdapter adapter = new RegisterAdapter(doc);
						APAdjust_AdjdRefNbr_FieldUpdated<RegisterAdapter>(adapter, curyInfo, adj);
					}
				}
			}
			catch (PXSetPropertyException ex)
			{
				throw new PXException(ex.Message);
			}
		}

		public class RegisterAdapter : IRegister, IInvoice
		{
			public APRegister _apRegister = null;
			public ARRegister _arRegister = null;
			public GLTranDoc _glRegister = null;
			public RegisterAdapter(APRegister aRegister)
			{
				this._apRegister = aRegister;
				this._glRegister = null;
			}

			public RegisterAdapter(ARRegister aRegister)
			{
				this._arRegister = aRegister;
				this._glRegister = null;
			}
			public RegisterAdapter(GLTranDoc aRegister)
			{
				this._apRegister = null;
				this._glRegister = aRegister;
			}

			public IInvoice Invoice
			{
				get
				{
					if (this._apRegister != null)
					{
						return (IInvoice)this._apRegister;
					}
					else if (this._arRegister != null)
					{
						return (IInvoice)this._arRegister;
					}
					else
					{
						return this._glRegister;
					}

				}
			}

			#region IRegister Members

			public int? BAccountID
			{
				get
				{
					return this._apRegister != null ? this._apRegister.VendorID : this._arRegister != null ? this._arRegister.CustomerID : this._glRegister.BAccountID;
				}
				set
				{
					if (this._apRegister != null)
					{
						this._apRegister.VendorID = value;
					}
					else if (this._arRegister != null)
					{
						this._arRegister.CustomerID = value;
					}
					else if (this._glRegister != null)
					{
						this._glRegister.BAccountID = value;
					}
				}
			}

			public int? LocationID
			{
				get
				{
					return this._apRegister != null ? this._apRegister.VendorLocationID : this._arRegister != null ? this._arRegister.CustomerLocationID : this._glRegister.LocationID;
				}
				set
				{
					if (this._apRegister != null)
					{
						this._apRegister.VendorLocationID = value;
					}
					else if (this._arRegister != null)
					{
						this._arRegister.CustomerLocationID = value;
					}
					else if (this._glRegister != null)
					{
						this._glRegister.LocationID = value;
					}
				}
			}

			public int? BranchID
			{
				get
				{
					return this._apRegister != null ? this._apRegister.BranchID : this._arRegister != null ? this._arRegister.BranchID : this._glRegister.BranchID;
				}
				set
				{
					if (this._apRegister != null)
					{
						this._apRegister.BranchID = value;
					}
					else if (this._arRegister != null)
					{
						this._arRegister.BranchID = value;
					}
					else if (this._glRegister != null)
					{
						this._glRegister.BranchID = value;
					}
				}
			}

			public DateTime? DocDate
			{
				get
				{
					return this._apRegister != null ? this._apRegister.DocDate : this._arRegister != null ? this._arRegister.DocDate : this._glRegister.TranDate;
				}
				set
				{
					if (this._apRegister != null)
					{
						this._apRegister.DocDate = value;
					}
					else if (this._arRegister != null)
					{
						this._arRegister.DocDate = value;
					}
					else if (this._glRegister != null)
					{
						this._glRegister.TranDate = value;
					}
				}
			}

			public int? AccountID
			{
				get
				{
					return this._apRegister != null ? this._apRegister.APAccountID :
							this._arRegister != null ? this._arRegister.ARAccountID :
							this._glRegister.TranModule == GL.BatchModule.AP ? (AP.APInvoiceType.DrCr(this._glRegister.TranType) == DrCr.Debit ? this._glRegister.CreditAccountID : this._glRegister.DebitAccountID) :
							this._glRegister.TranModule == GL.BatchModule.AR ? (AR.ARInvoiceType.DrCr(this._glRegister.TranType) == DrCr.Debit ? this._glRegister.CreditAccountID : this._glRegister.DebitAccountID) : null;
				}
				set
				{
					if (this._apRegister != null)
					{
						this._apRegister.APAccountID = value;
					}
					else if (this._arRegister != null)
					{
						this._arRegister.ARAccountID = value;
					}
					else if (this._glRegister != null)
					{
						bool? isDirect = null;
						if (this._glRegister.TranModule == GL.BatchModule.AP)
						{
							isDirect = AP.APInvoiceType.DrCr(this._glRegister.TranType) == DrCr.Debit;
						}
						else if (this._glRegister.TranModule == GL.BatchModule.AR)
						{
							isDirect = AR.ARInvoiceType.DrCr(this._glRegister.TranType) == DrCr.Debit;
						}

						if (isDirect.HasValue)
						{
							if (isDirect.Value)
							{
								this._glRegister.CreditAccountID = value;
							}
							else
							{
								this._glRegister.DebitAccountID = value;
						}
					}
				}
			}
			}

			public int? SubID
			{
				get
				{
					return this._apRegister != null ? this._apRegister.APSubID :
							this._arRegister != null ? this._arRegister.ARSubID :
							this._glRegister.TranModule == GL.BatchModule.AP ? (AP.APInvoiceType.DrCr(this._glRegister.TranType) == DrCr.Debit ? this._glRegister.CreditSubID : this._glRegister.DebitSubID) :
							this._glRegister.TranModule == GL.BatchModule.AR ? (AR.ARInvoiceType.DrCr(this._glRegister.TranType) == DrCr.Debit ? this._glRegister.CreditSubID : this._glRegister.DebitSubID) : null;
				}
				set
				{
					if (this._apRegister != null)
					{
						this._apRegister.APSubID = value;
					}
					else if (this._arRegister != null)
					{
						this._arRegister.ARSubID = value;
					}
					else if (this._glRegister != null)
					{
						bool? isDirect = null;
						if (this._glRegister.TranModule == GL.BatchModule.AP)
						{
							isDirect = AP.APInvoiceType.DrCr(this._glRegister.TranType) == DrCr.Debit;
						}
						else if (this._glRegister.TranModule == GL.BatchModule.AR)
						{
							isDirect = AR.ARInvoiceType.DrCr(this._glRegister.TranType) == DrCr.Debit;
						}
						if (isDirect.HasValue)
						{
							if (isDirect.Value)
							{
								this._glRegister.CreditSubID = value;
							}
							else
							{
								this._glRegister.DebitSubID = value;
						}
					}
				}
			}
			}

			public string CuryID
			{
				get
				{
					return this._apRegister != null ? this._apRegister.CuryID : this._arRegister != null ? this._arRegister.CuryID : this._glRegister.CuryID;
				}
				set
				{
					if (this._apRegister != null)
					{
						this._apRegister.CuryID = value;
					}
					if (this._arRegister != null)
					{
						this._arRegister.CuryID = value;
					}
					else if (this._glRegister != null)
					{
						this._glRegister.CuryID = value;
					}
				}
			}

			public string FinPeriodID
			{
				get
				{
					return this._apRegister != null ? this._apRegister.FinPeriodID : this._arRegister != null ? this._arRegister.FinPeriodID : this._glRegister.FinPeriodID;
				}
				set
				{
					if (this._apRegister != null)
					{
						this._apRegister.FinPeriodID = value;
					}
					if (this._arRegister != null)
					{
						this._arRegister.FinPeriodID = value;
					}
					else if (this._glRegister != null)
					{
						this._glRegister.FinPeriodID = value;
					}
				}
			}

			public long? CuryInfoID
			{
				get
				{
					return this._apRegister != null ? this._apRegister.CuryInfoID : this._arRegister != null ? this._arRegister.CuryInfoID : this._glRegister.CuryInfoID;
				}
				set
				{
					if (this._apRegister != null)
					{
						this._apRegister.CuryInfoID = value;
					}
					if (this._arRegister != null)
					{
						this._arRegister.CuryInfoID = value;
					}
					else if (this._glRegister != null)
					{
						this._glRegister.CuryInfoID = value;
					}
				}
			}

			#endregion

			#region IInvoice Members

			public decimal? CuryDocBal
			{
				get
				{
					return this.Invoice.CuryDocBal;
				}
				set
				{
					this.Invoice.CuryDocBal = value;
				}
			}

			public decimal? DocBal
			{
				get
				{
					return this.Invoice.DocBal;
				}
				set
				{
					this.Invoice.DocBal = value;
				}
			}

			public decimal? CuryDiscBal
			{
				get
				{
					return this.Invoice.CuryDiscBal;
				}
				set
				{
					this.Invoice.CuryDiscBal = value;
				}
			}

			public decimal? DiscBal
			{
				get
				{
					return this.Invoice.DiscBal;
				}
				set
				{
					this.Invoice.DiscBal = value;
				}
			}

			public decimal? CuryWhTaxBal
			{
				get
				{
					return this.Invoice.CuryWhTaxBal;
				}
				set
				{
					this.Invoice.CuryWhTaxBal = value;
				}
			}

			public decimal? WhTaxBal
			{
				get
				{
					return this.Invoice.WhTaxBal;
				}
				set
				{
					this.Invoice.WhTaxBal = value;
				}
			}

			public DateTime? DiscDate
			{
				get
				{
					return this.Invoice.DiscDate;
				}
				set
				{
					this.Invoice.DiscDate = value;
				}
			}

			public string DocType
			{
				get
				{
					return this.Invoice.DocType;
				}
				set
				{
					this.Invoice.DocType = value;
				}
			}

			public string RefNbr
			{
				get
				{
					return this.Invoice.RefNbr;
				}
				set
				{
					this.Invoice.RefNbr = value;
				}
			}

			public string OrigModule
			{
				get
				{
					return this.Invoice.OrigModule;
				}
				set
				{
					this.Invoice.OrigModule = value;
				}
			}

			public decimal? CuryOrigDocAmt
			{
				get { return Invoice.CuryOrigDocAmt; }
				set { Invoice.CuryOrigDocAmt = value; }
			}

			public decimal? OrigDocAmt
			{
				get { return Invoice.OrigDocAmt; }
				set { Invoice.OrigDocAmt = value; }
			}

			public string DocDesc
			{
				get
				{
					return this.Invoice.DocDesc;
				}
				set
				{
					this.Invoice.DocDesc = value;
				}
			}
			#endregion
		}
		private void APAdjust_AdjdRefNbr_FieldUpdated<T>(T aInvoice, CurrencyInfo aInvoiceInfo, APAdjust adj)
			where T : class, IRegister, IInvoice
		{
			CurrencyInfo info = aInvoiceInfo;
			CurrencyInfo info_copy = null;
			T invoice = aInvoice;
			GLTranDocAP payment = this.APPayments.Current;
			DateTime? applDate = payment.TranDate;
			DateTime? docDate = payment.TranDate;

				info_copy = PXCache<CurrencyInfo>.CreateCopy(info);
				info_copy.CuryInfoID = null;
				info_copy = (CurrencyInfo)currencyinfo.Cache.Insert(info_copy);

				info_copy.SetCuryEffDate(currencyinfo.Cache, docDate);

			adj.VendorID = invoice.BAccountID;
			adj.AdjgDocDate = applDate;
			adj.AdjgCuryInfoID = payment.CuryInfoID;
			adj.AdjdCuryInfoID = info_copy.CuryInfoID;
			adj.AdjdOrigCuryInfoID = info.CuryInfoID;
			adj.AdjdBranchID = invoice.BranchID;
			adj.AdjdAPAcct = invoice.AccountID;
			adj.AdjdAPSub = invoice.SubID;
			adj.AdjdDocDate = invoice.DocDate;
			adj.AdjdFinPeriodID = invoice.FinPeriodID;
			adj.Released = false;

			CalcBalances<T>(adj, invoice, false, true);

			if (adj.CuryWhTaxBal >= 0m &&
										adj.CuryDiscBal >= 0m &&
										adj.CuryDocBal - adj.CuryWhTaxBal - adj.CuryDiscBal <= 0m)
			{
				//no amount suggestion is possible
				return;
			}

			decimal? CuryApplDiscAmt = (adj.AdjgDocType == APDocType.DebitAdj) ? 0m : adj.CuryDiscBal;
			decimal? CuryApplAmt = adj.CuryDocBal - adj.CuryWhTaxBal - CuryApplDiscAmt;
			decimal? CuryUnappliedBal = payment.CuryUnappliedBal;

			if (payment != null && adj.AdjgBalSign < 0m)
			{
				if (CuryUnappliedBal < 0m)
				{
					CuryApplAmt = Math.Min((decimal)CuryApplAmt, Math.Abs((decimal)CuryUnappliedBal));
				}
			}
			else if (payment != null && CuryUnappliedBal > 0m && adj.AdjgBalSign > 0m && CuryUnappliedBal < CuryApplDiscAmt)
			{
				CuryApplAmt = CuryUnappliedBal;
				CuryApplDiscAmt = 0m;
			}
			else if (payment != null && CuryUnappliedBal > 0m && adj.AdjgBalSign > 0m)
			{
				CuryApplAmt = Math.Min((decimal)CuryApplAmt, (decimal)CuryUnappliedBal);
			}
			else if (payment != null && CuryUnappliedBal <= 0m && payment.CuryTranAmt > 0)
			{
				CuryApplAmt = 0m;
			}

			adj.CuryAdjgAmt = CuryApplAmt;
			adj.CuryAdjgDiscAmt = CuryApplDiscAmt;
			adj.CuryAdjgWhTaxAmt = adj.CuryWhTaxBal;

			CalcBalances<T>(adj, invoice, true, true);
		}


		protected virtual void APAdjust_CuryDocBal_FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			APAdjust row = (APAdjust)e.Row;
			if (!internalCall)
			{
				if (e.Row != null && row.AdjdCuryInfoID != null && row.CuryDocBal == null && sender.GetStatus(e.Row) != PXEntryStatus.Deleted)
				{
					CalcBalances(row, false);
				}
				if (e.Row != null)
				{
					e.NewValue = row.CuryDocBal;
				}
			}
			e.Cancel = true;
		}

		protected virtual void APAdjust_CuryDiscBal_FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			APAdjust row = (APAdjust)e.Row;
			if (!internalCall)
			{
				if (e.Row != null && row.AdjdCuryInfoID != null && row.CuryDiscBal == null && sender.GetStatus(e.Row) != PXEntryStatus.Deleted)
				{
					CalcBalances(row, false);
				}
				if (e.Row != null)
				{
					e.NewValue = row.CuryDiscBal;
				}
			}
			e.Cancel = true;
		}

		protected virtual void APAdjust_CuryWhTaxBal_FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			APAdjust row = (APAdjust)e.Row;
			if (!internalCall)
			{
				if (e.Row != null && row.AdjdCuryInfoID != null && row.CuryWhTaxBal == null && sender.GetStatus(e.Row) != PXEntryStatus.Deleted)
				{
					CalcBalances(row, false);
				}
				if (e.Row != null)
				{
					e.NewValue = row.CuryWhTaxBal;
				}
			}
			e.Cancel = true;
		}

		protected virtual void APAdjust_AdjdCuryRate_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if ((decimal)e.NewValue <= 0m)
			{
				throw new PXSetPropertyException(CS.Messages.Entry_GT, ((int)0).ToString());
			}
		}

		protected virtual void APAdjust_CuryAdjgAmt_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			APAdjust row = (APAdjust)e.Row;

			if (row.CuryDocBal == null || row.CuryDiscBal == null || row.CuryWhTaxBal == null)
			{
				CalcBalances(row, false);
			}

			if (row.CuryDocBal == null)
			{
				throw new PXSetPropertyException<APAdjust.adjdRefNbr>(ErrorMessages.FieldIsEmpty, PXUIFieldAttribute.GetDisplayName<APAdjust.adjdRefNbr>(sender));
			}

			if (row.VoidAdjNbr == null && (decimal)e.NewValue < 0m)
			{
				throw new PXSetPropertyException(CS.Messages.Entry_GE, ((int)0).ToString());
			}

			if (row.VoidAdjNbr != null && (decimal)e.NewValue > 0m)
			{
				throw new PXSetPropertyException(CS.Messages.Entry_LE, ((int)0).ToString());
			}

			if ((decimal)row.CuryDocBal + (decimal)row.CuryAdjgAmt - (decimal)e.NewValue < 0)
			{
				throw new PXSetPropertyException(AP.Messages.Entry_LE, ((decimal)row.CuryDocBal + (decimal)row.CuryAdjgAmt).ToString());
			}
		}

		protected virtual void APAdjust_CuryAdjgAmt_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			APAdjust row = (APAdjust)e.Row;
			if (e.OldValue != null && row.CuryDocBal == 0m && row.CuryAdjgAmt < (decimal)e.OldValue)
			{
				row.CuryAdjgDiscAmt = 0m;
			}
			CalcBalances(row, true);
		}

		protected virtual void APAdjust_CuryAdjgDiscAmt_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			APAdjust row = (APAdjust)e.Row;

			if (row.CuryDocBal == null || row.CuryDiscBal == null || row.CuryWhTaxBal == null)
			{
				CalcBalances(row, false);
			}

			if (row.CuryDocBal == null || row.CuryDiscBal == null)
			{
				throw new PXSetPropertyException<APAdjust.adjdRefNbr>(ErrorMessages.FieldIsEmpty, PXUIFieldAttribute.GetDisplayName<APAdjust.adjdRefNbr>(sender));
			}

			if (row.VoidAdjNbr == null && (decimal)e.NewValue < 0m)
			{
				throw new PXSetPropertyException(CS.Messages.Entry_GE, ((int)0).ToString());
			}

			if (row.VoidAdjNbr != null && (decimal)e.NewValue > 0m)
			{
				throw new PXSetPropertyException(CS.Messages.Entry_LE, ((int)0).ToString());
			}

			if ((decimal)row.CuryDiscBal + (decimal)row.CuryAdjgDiscAmt - (decimal)e.NewValue < 0)
			{
				throw new PXSetPropertyException(AP.Messages.Entry_LE, ((decimal)row.CuryDiscBal + (decimal)row.CuryAdjgDiscAmt).ToString());
			}

			if (row.CuryAdjgAmt != null && (sender.GetValuePending<APAdjust.curyAdjgAmt>(e.Row) == PXCache.NotSetValue || (Decimal?)sender.GetValuePending<APAdjust.curyAdjgAmt>(e.Row) == row.CuryAdjgAmt))
			{
				if ((decimal)row.CuryDocBal + (decimal)row.CuryAdjgDiscAmt - (decimal)e.NewValue < 0)
				{
					throw new PXSetPropertyException(AP.Messages.Entry_LE, ((decimal)row.CuryDocBal + (decimal)row.CuryAdjgDiscAmt).ToString());
				}
			}
		}

		protected virtual void APAdjust_CuryAdjgDiscAmt_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			APAdjust row = (APAdjust)e.Row;
			CalcBalances(row, true);
		}

		protected virtual void APAdjust_CuryAdjgWhTaxAmt_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			APAdjust row = (APAdjust)e.Row;

			if (row.CuryDocBal == null || row.CuryDiscBal == null || row.CuryWhTaxBal == null)
			{
				CalcBalances(row, false);
			}

			if (row.CuryDocBal == null || row.CuryWhTaxBal == null)
			{
				throw new PXSetPropertyException<APAdjust.adjdRefNbr>(ErrorMessages.FieldIsEmpty, PXUIFieldAttribute.GetDisplayName<APAdjust.adjdRefNbr>(sender));
			}

			if (row.VoidAdjNbr == null && (decimal)e.NewValue < 0m)
			{
				throw new PXSetPropertyException(CS.Messages.Entry_GE, ((int)0).ToString());
			}

			if (row.VoidAdjNbr != null && (decimal)e.NewValue > 0m)
			{
				throw new PXSetPropertyException(CS.Messages.Entry_LE, ((int)0).ToString());
			}

			if ((decimal)row.CuryWhTaxBal + (decimal)row.CuryAdjgWhTaxAmt - (decimal)e.NewValue < 0)
			{
				throw new PXSetPropertyException(AP.Messages.Entry_LE, ((decimal)row.CuryWhTaxBal + (decimal)row.CuryAdjgWhTaxAmt).ToString());
			}

			if (row.CuryAdjgAmt != null && (sender.GetValuePending<APAdjust.curyAdjgAmt>(e.Row) == PXCache.NotSetValue || (Decimal?)sender.GetValuePending<APAdjust.curyAdjgAmt>(e.Row) == row.CuryAdjgAmt))
			{
				if ((decimal)row.CuryDocBal + (decimal)row.CuryAdjgWhTaxAmt - (decimal)e.NewValue < 0)
				{
					throw new PXSetPropertyException(AP.Messages.Entry_LE, ((decimal)row.CuryDocBal + (decimal)row.CuryAdjgWhTaxAmt).ToString());
				}
			}
		}

		protected virtual void APAdjust_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			APAdjust adj = (APAdjust)e.Row;

			if (adj == null || internalCall)
			{
				return;
			}

			bool adjNotReleased = (bool)(adj.Released == false);
			PXUIFieldAttribute.SetEnabled<APAdjust.adjdDocType>(cache, adj, adjNotReleased && (adj.Voided != true) && adj.AdjdRefNbr == null);
			PXUIFieldAttribute.SetEnabled<APAdjust.adjdRefNbr>(cache, adj, adjNotReleased && (adj.Voided != true) && adj.AdjdRefNbr == null);
			PXUIFieldAttribute.SetEnabled<APAdjust.curyAdjgAmt>(cache, adj, adjNotReleased && (adj.Voided != true));
			PXUIFieldAttribute.SetEnabled<APAdjust.curyAdjgDiscAmt>(cache, adj, adjNotReleased && (adj.Voided != true));
			PXUIFieldAttribute.SetEnabled<APAdjust.curyAdjgWhTaxAmt>(cache, adj, adjNotReleased && (adj.Voided != true));
			PXUIFieldAttribute.SetEnabled<APAdjust.adjBatchNbr>(cache, adj, false);
			PXUIFieldAttribute.SetVisible<APAdjust.adjBatchNbr>(cache, adj, !adjNotReleased);
		}

		protected virtual void APAdjust_RowInserting(PXCache sender, PXRowInsertingEventArgs e)
		{
			string errmsg = PXUIFieldAttribute.GetError<APAdjust.adjdRefNbr>(sender, e.Row);
			e.Cancel = (((APAdjust)e.Row).AdjdRefNbr == null || string.IsNullOrEmpty(errmsg) == false);
		}

		protected virtual void APAdjust_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			APAdjust doc = (APAdjust)e.Row;
			GLTranDocAP parentDoc = (GLTranDocAP)this.APPayments.Search<GLTranDocAP.tranType, GLTranDocAP.refNbr>(doc.AdjgDocType, doc.AdjgRefNbr);
			PXCache parentCache = this.APPayments.Cache;
			if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Delete)
			{
				return;
			}
			if (parentDoc == null)
			{
				e.Cancel = true;
				return;
			}

			if (((DateTime)doc.AdjdDocDate).CompareTo((DateTime)parentDoc.TranDate) > 0 && (doc.AdjgDocType != APDocType.Check && doc.AdjgDocType != APDocType.VoidCheck && doc.AdjgDocType != APDocType.Prepayment || apsetup.Current.EarlyChecks == false))
			{
				if (sender.RaiseExceptionHandling<APAdjust.adjdRefNbr>(e.Row, doc.AdjdRefNbr, new PXSetPropertyException(AP.Messages.ApplDate_Less_DocDate, PXErrorLevel.RowError, PXUIFieldAttribute.GetDisplayName<GLTranDocAP.tranDate>(parentCache))))
				{
					throw new PXRowPersistingException(PXDataUtils.FieldName<APAdjust.adjdDocDate>(), doc.AdjdDocDate, AP.Messages.ApplDate_Less_DocDate, PXUIFieldAttribute.GetDisplayName<GLTranDocAP.tranDate>(parentCache));
				}
			}

			if (((string)doc.AdjdFinPeriodID).CompareTo((string)parentDoc.FinPeriodID) > 0 && (doc.AdjgDocType != APDocType.Check && doc.AdjgDocType != APDocType.VoidCheck && doc.AdjgDocType != APDocType.Prepayment || apsetup.Current.EarlyChecks == false))
			{
				if (sender.RaiseExceptionHandling<APAdjust.adjdRefNbr>(e.Row, doc.AdjdRefNbr, new PXSetPropertyException(AP.Messages.ApplPeriod_Less_DocPeriod, PXErrorLevel.RowError, PXUIFieldAttribute.GetDisplayName<GLTranDocAP.finPeriodID>(parentCache))))
				{
					throw new PXRowPersistingException(PXDataUtils.FieldName<APAdjust.adjdFinPeriodID>(), doc.AdjdFinPeriodID, AP.Messages.ApplPeriod_Less_DocPeriod, PXUIFieldAttribute.GetDisplayName<GLTranDocAP.finPeriodID>(parentCache));
				}
			}

			if (doc.AdjdDocType == APDocType.Prepayment)
			{
				doc.AdjdCuryInfoID = parentDoc.CuryInfoID;
				doc.AdjdOrigCuryInfoID = parentDoc.CuryInfoID;
			}

			if (doc.CuryDocBal < 0m)
			{
				sender.RaiseExceptionHandling<APAdjust.curyAdjgAmt>(e.Row, doc.CuryAdjgAmt, new PXSetPropertyException(AP.Messages.DocumentBalanceNegative));
			}

			if (doc.AdjgDocType != APDocType.QuickCheck && doc.CuryDiscBal < 0m)
			{
				sender.RaiseExceptionHandling<APAdjust.curyAdjgDiscAmt>(e.Row, doc.CuryAdjgDiscAmt, new PXSetPropertyException(AP.Messages.DocumentBalanceNegative));
			}

			if (doc.AdjgDocType != APDocType.QuickCheck && doc.CuryWhTaxBal < 0m)
			{
				sender.RaiseExceptionHandling<APAdjust.curyAdjgWhTaxAmt>(e.Row, doc.CuryAdjgWhTaxAmt, new PXSetPropertyException(AP.Messages.DocumentBalanceNegative));
			}
		}

		protected virtual void APAdjust_CuryAdjgWhTaxAmt_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			APAdjust row = (APAdjust)e.Row;
			CalcBalances(row, true);
		}

		#region Payment Application internal valriables
		public bool TakeDiscAlways = false;
		private bool internalCall = false;
		#endregion

		#region Payment Application Utility Functions
		private static bool IsMatchingAdjd(APAdjust adj, GLTranDoc invoice)
		{
			if (adj != null && invoice != null)
			{
				return (invoice.TranModule == GL.BatchModule.AP
				&& invoice.TranType == adj.AdjdDocType
				&& invoice.RefNbr == adj.AdjdRefNbr);
			}
			return false;
		}

		protected virtual GLTranDoc FindMatchingAdjd(APAdjust row)
		{
			GLTranDoc invoice = PXSelect<GLTranDoc, Where<GLTranDoc.batchNbr, Equal<Current<GLDocBatch.batchNbr>>,
											And<GLTranDoc.parentLineNbr, IsNull,
											And<GLTranDoc.tranModule, Equal<GL.BatchModule.moduleAP>,
											And<GLTranDoc.bAccountID, Equal<Required<GLTranDoc.bAccountID>>,
											And<GLTranDoc.tranType, Equal<Required<APAdjust.adjdDocType>>,
											And<GLTranDoc.refNbr, Equal<Required<GLTranDoc.refNbr>>>>>>>>>.Select(this, row.VendorID, row.AdjdDocType, row.AdjdRefNbr);
			return invoice;
		}

		private void CalcBalances(APAdjust row, bool isCalcRGOL)
		{
			CalcBalances(row, isCalcRGOL, !TakeDiscAlways);
		}

		private void CalcBalances(APAdjust row, bool isCalcRGOL, bool DiscOnDiscDate)
		{
			APAdjust adj = row;
			foreach (APInvoice voucher in APInvoice_VendorID_DocType_RefNbr.Select(adj.VendorID, adj.AdjdDocType, adj.AdjdRefNbr))
			{
				CalcBalances<APInvoice>(adj, voucher, isCalcRGOL, DiscOnDiscDate);
				return;
			}

			GLTranDoc protoInvoice0 = FindMatchingAdjd(adj);
			if (protoInvoice0 != null)
			{
				GLTranDoc protoInvoice1 = (GLTranDoc)this.Caches[typeof(GLTranDoc)].CreateCopy(protoInvoice0);
				protoInvoice1.CuryApplAmt -= row.CuryAdjdAmt ?? Decimal.Zero;
				protoInvoice1.CuryDiscTaken -= row.CuryAdjdDiscAmt ?? Decimal.Zero;
				protoInvoice1.CuryTaxWheld -= row.CuryAdjdWhTaxAmt ?? Decimal.Zero;
				CalcBalances<GLTranDoc>(adj, protoInvoice1, isCalcRGOL, DiscOnDiscDate);
				return;
			}

			foreach (APPayment payment in APPayment_VendorID_DocType_RefNbr.Select(adj.VendorID, adj.AdjdDocType, adj.AdjdRefNbr))
			{
				CalcBalances<APPayment>(adj, payment, isCalcRGOL, DiscOnDiscDate);
			}
		}

		private void CalcBalances<T>(APAdjust adj, T voucher, bool isCalcRGOL)
			where T : IInvoice
		{
			AP.APPaymentEntry.CalcBalances<T>(CurrencyInfo_CuryInfoID, adj, voucher, isCalcRGOL, !TakeDiscAlways);
		}

		private void CalcBalances<T>(APAdjust adj, T voucher, bool isCalcRGOL, bool DiscOnDiscDate)
			where T : IInvoice
		{
			AP.APPaymentEntry.CalcBalances<T>(CurrencyInfo_CuryInfoID, adj, voucher, isCalcRGOL, DiscOnDiscDate);
		}
		#endregion
		#endregion

		#region ARAdjust

		protected virtual void ARAdjust_AdjdRefNbr_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			GLTranDocAR payment = this.ARPayments.Current;
			e.Cancel = (payment != null && (payment.TranType == ARDocType.CashSale));
		}

		protected virtual void ARAdjust_AdjdDocType_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			if (e.Row != null)
			{
				sender.SetDefaultExt<ARAdjust.adjdRefNbr>(e.Row);
			}
		}

		protected virtual void ARAdjust_AdjdRefNbr_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			try
			{
				ARAdjust adj = e.Row as ARAdjust;
				if (adj.AdjdCuryInfoID == null)
				{
					foreach (PXResult<ARInvoice, CurrencyInfo> res in PXSelectJoin<ARInvoice, InnerJoin<CurrencyInfo, On<CurrencyInfo.curyInfoID, Equal<ARInvoice.curyInfoID>>>,
							Where<ARInvoice.customerID, Equal<Current<GLTranDocAR.bAccountID>>,
							And<ARInvoice.docType, Equal<Required<ARInvoice.docType>>,
							And<ARInvoice.refNbr, Equal<Required<ARInvoice.refNbr>>>>>>.Select(this, adj.AdjdDocType, adj.AdjdRefNbr))
					{
						ARInvoice doc = res;
						CurrencyInfo curyInfo = res;
						RegisterAdapter adapter = new RegisterAdapter(doc);
						ARAdjust_AdjdRefNbr_FieldUpdated<RegisterAdapter>(adapter, curyInfo, adj);
						return;
					}

					foreach (PXResult<GLTranDoc, CurrencyInfo> res in PXSelectJoin<GLTranDoc, InnerJoin<CurrencyInfo, On<CurrencyInfo.curyInfoID, Equal<GLTranDoc.curyInfoID>>>,
											Where<GLTranDoc.batchNbr, Equal<Current<GLDocBatch.batchNbr>>,
											And<GLTranDoc.parentLineNbr, IsNull,
											And<GLTranDoc.tranModule, Equal<GL.BatchModule.moduleAR>,
											And<GLTranDoc.bAccountID, Equal<Required<GLTranDoc.bAccountID>>,
											And<GLTranDoc.tranType, Equal<Required<APAdjust.adjdDocType>>,
											And<GLTranDoc.refNbr, Equal<Required<GLTranDoc.refNbr>>>>>>>>>.Select(this, adj.CustomerID, adj.AdjdDocType, adj.AdjdRefNbr))
					{
						GLTranDoc doc = res;
						CurrencyInfo curyInfo = res;
						RegisterAdapter adapter = new RegisterAdapter(doc);
						ARAdjust_AdjdRefNbr_FieldUpdated<RegisterAdapter>(adapter, curyInfo, adj);
						return;
					}

					foreach (PXResult<ARPayment, CurrencyInfo> res in PXSelectJoin<ARPayment, InnerJoin<CurrencyInfo, On<CurrencyInfo.curyInfoID, Equal<ARPayment.curyInfoID>>>,
							Where<ARPayment.customerID, Equal<Current<GLTranDocAR.bAccountID>>, And<ARPayment.docType, Equal<Required<ARPayment.docType>>, And<ARPayment.refNbr, Equal<Required<ARPayment.refNbr>>>>>>.Select(this, adj.AdjdDocType, adj.AdjdRefNbr))
					{
						ARPayment doc = res;
						CurrencyInfo curyInfo = res;
						RegisterAdapter adapter = new RegisterAdapter(doc);
						ARAdjust_AdjdRefNbr_FieldUpdated<RegisterAdapter>(adapter, curyInfo, adj);
					}
				}
			}
			catch (PXSetPropertyException ex)
			{
				throw new PXException(ex.Message);
			}
		}

		private void ARAdjust_AdjdRefNbr_FieldUpdated<T>(T aInvoice, CurrencyInfo aInvoiceInfo, ARAdjust adj)
			where T : class, IRegister, IInvoice
		{
			CurrencyInfo info = aInvoiceInfo;
			CurrencyInfo info_copy = null;
			T invoice = aInvoice;
			GLTranDocAR payment = this.ARPayments.Current;
			Customer customer = this.Customer.Select(invoice.BAccountID);
			DateTime? applDate = payment.TranDate;
			DateTime? docDate = payment.TranDate;

			info_copy = PXCache<CurrencyInfo>.CreateCopy(info);
			info_copy.CuryInfoID = null;
			info_copy = (CurrencyInfo)currencyinfo.Cache.Insert(info_copy);
			info_copy.SetCuryEffDate(currencyinfo.Cache, docDate);

			adj.CustomerID = invoice.BAccountID;
			adj.AdjgDocDate = applDate;
			adj.AdjgCuryInfoID = payment.CuryInfoID;
			adj.AdjdCustomerID = invoice.BAccountID;
			adj.AdjdCuryInfoID = info_copy.CuryInfoID;
			adj.AdjdOrigCuryInfoID = info.CuryInfoID;
			adj.AdjdBranchID = invoice.BranchID;
			adj.AdjdARAcct = invoice.AccountID;
			adj.AdjdARSub = invoice.SubID;
			adj.AdjdDocDate = invoice.DocDate;
			adj.AdjdFinPeriodID = invoice.FinPeriodID;
			adj.Released = false;

			CalcBalances<T>(adj, customer, invoice, false, true);

			decimal? CuryApplAmt = adj.CuryDocBal - adj.CuryDiscBal;
			decimal? CuryApplDiscAmt = adj.CuryDiscBal;
			decimal? CuryUnappliedBal = payment.CuryUnappliedBal;

			if (adj.CuryDiscBal >= 0m && adj.CuryDocBal - adj.CuryDiscBal <= 0m)
			{
				//no amount suggestion is possible
				return;
			}

			if (payment != null && string.IsNullOrEmpty(payment.TranDesc))
			{
				//payment.TranDesc = invoice.DocDesc;
			}

			if (payment != null && adj.AdjgBalSign < 0m)
			{
				if (CuryUnappliedBal < 0m)
				{
					CuryApplAmt = Math.Min((decimal)CuryApplAmt, Math.Abs((decimal)CuryUnappliedBal));
				}
			}
			else if (payment != null && CuryUnappliedBal > 0m && adj.AdjgBalSign > 0m)
			{
				CuryApplAmt = Math.Min((decimal)CuryApplAmt, (decimal)CuryUnappliedBal);

				if (CuryApplAmt + CuryApplDiscAmt < adj.CuryDocBal)
				{
					CuryApplDiscAmt = 0m;
				}
			}
			else if (payment != null && CuryUnappliedBal <= 0m && payment.CuryTranAmt > 0)
			{
				CuryApplAmt = 0m;
				CuryApplDiscAmt = 0m;
			}

			adj.CuryAdjgAmt = CuryApplAmt;
			adj.CuryAdjgDiscAmt = CuryApplDiscAmt;
			adj.CuryAdjgWOAmt = 0m;
			CalcBalances<T>(adj, customer, invoice, true, true);
		}

		private bool internalCallAR = false;

		protected virtual void ARAdjust_CuryDocBal_FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			ARAdjust row = (ARAdjust)e.Row;
			if (!internalCallAR)
			{
				if (e.Row != null && ((ARAdjust)e.Row).AdjdCuryInfoID != null && ((ARAdjust)e.Row).CuryDocBal == null && sender.GetStatus(e.Row) != PXEntryStatus.Deleted)
				{
					CalcBalances(row, false);
				}
				if (e.Row != null)
				{
					e.NewValue = ((ARAdjust)e.Row).CuryDocBal;
				}
			}
			e.Cancel = true;
		}

		protected virtual void ARAdjust_CuryDiscBal_FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			ARAdjust row = (ARAdjust)e.Row;
			if (!internalCallAR)
			{
				if (e.Row != null && ((ARAdjust)e.Row).AdjdCuryInfoID != null && ((ARAdjust)e.Row).CuryDiscBal == null && sender.GetStatus(e.Row) != PXEntryStatus.Deleted)
				{
					CalcBalances(row, false);
				}
				if (e.Row != null)
				{
					e.NewValue = ((ARAdjust)e.Row).CuryDiscBal;
				}
			}
			e.Cancel = true;
		}

#if false
		protected virtual void ARAdjust_AdjdCuryRate_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if ((decimal)e.NewValue <= 0m)
			{
				throw new PXSetPropertyException(CS.Messages.Entry_GT, ((int)0).ToString());
			}
		}


		protected virtual void ARAdjust_AdjdCuryRate_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			ARAdjust adj = (ARAdjust)e.Row;

			CurrencyInfo pay_info = PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Current<ARAdjust.adjgCuryInfoID>>>>.SelectSingleBound(this, new object[] { e.Row });
			CurrencyInfo vouch_info = PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Current<ARAdjust.adjdCuryInfoID>>>>.SelectSingleBound(this, new object[] { e.Row });
			GLTranDocAR payment = this.ARPayments.Current;
			DateTime? appDate = payment.TranDate;
			DateTime? docDate = payment.TranDate;
			if (string.Equals(pay_info.CuryID, vouch_info.CuryID) && adj.AdjdCuryRate != 1m)
			{
				adj.AdjdCuryRate = 1m;
				vouch_info.SetCuryEffDate(currencyinfo.Cache, docDate);
			}
			else if (string.Equals(vouch_info.CuryID, vouch_info.BaseCuryID))
			{
				adj.AdjdCuryRate = pay_info.CuryMultDiv == "M" ? 1 / pay_info.CuryRate : pay_info.CuryRate;
			}
			else
			{
				vouch_info.CuryRate = Math.Round((decimal)adj.AdjdCuryRate * (pay_info.CuryMultDiv == "M" ? (decimal)pay_info.CuryRate : 1m / (decimal)pay_info.CuryRate), 8, MidpointRounding.AwayFromZero);
				vouch_info.RecipRate = Math.Round((pay_info.CuryMultDiv == "M" ? 1m / (decimal)pay_info.CuryRate : (decimal)pay_info.CuryRate) / (decimal)adj.AdjdCuryRate, 8, MidpointRounding.AwayFromZero);
				vouch_info.CuryMultDiv = "M";
			}

			if (Caches[typeof(CurrencyInfo)].GetStatus(vouch_info) == PXEntryStatus.Notchanged)
			{
				Caches[typeof(CurrencyInfo)].SetStatus(vouch_info, PXEntryStatus.Updated);
			}
			CalcBalances(e.Row, true);
		} 
#endif

		protected virtual void ARAdjust_CuryAdjgAmt_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			ARAdjust row = (ARAdjust)e.Row;

			if (row.CuryDocBal == null || row.CuryDiscBal == null || row.CuryWOBal == null)
			{
				CalcBalances(row, false, false);
			}

			if (row.CuryDocBal == null)
			{
				throw new PXSetPropertyException<ARAdjust.adjdRefNbr>(ErrorMessages.FieldIsEmpty, PXUIFieldAttribute.GetDisplayName<ARAdjust.adjdRefNbr>(sender));
			}

			if (row.VoidAdjNbr == null && (decimal)e.NewValue < 0m)
			{
				throw new PXSetPropertyException(CS.Messages.Entry_GE, ((int)0).ToString());
			}

			if (row.VoidAdjNbr != null && (decimal)e.NewValue > 0m)
			{
				throw new PXSetPropertyException(CS.Messages.Entry_LE, ((int)0).ToString());
			}

			if ((decimal)row.CuryDocBal + (decimal)row.CuryAdjgAmt - (decimal)e.NewValue < 0)
			{
				throw new PXSetPropertyException(AR.Messages.Entry_LE, ((decimal)row.CuryDocBal + (decimal)row.CuryAdjgAmt).ToString());
			}
		}


		protected virtual void ARAdjust_CuryAdjgAmt_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			ARAdjust row = (ARAdjust)e.Row;
			if (e.OldValue != null && row.CuryDocBal == 0m && row.CuryAdjgAmt < (decimal)e.OldValue)
			{
				row.CuryAdjgDiscAmt = 0m;
			}
			CalcBalances(row, true);
		}


		protected virtual void ARAdjust_CuryAdjgDiscAmt_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			ARAdjust row = (ARAdjust)e.Row;
			if (row.CuryDocBal == null || row.CuryDiscBal == null || row.CuryWOBal == null)
			{
				CalcBalances(row, false, false);
			}

			if (row.CuryDocBal == null || row.CuryDiscBal == null)
			{
				throw new PXSetPropertyException<ARAdjust.adjdRefNbr>(ErrorMessages.FieldIsEmpty, PXUIFieldAttribute.GetDisplayName<ARAdjust.adjdRefNbr>(sender));
			}

			if (row.VoidAdjNbr == null && (decimal)e.NewValue < 0m)
			{
				throw new PXSetPropertyException(CS.Messages.Entry_GE, ((int)0).ToString());
			}

			if (row.VoidAdjNbr != null && (decimal)e.NewValue > 0m)
			{
				throw new PXSetPropertyException(CS.Messages.Entry_LE, ((int)0).ToString());
			}

			if ((decimal)row.CuryDiscBal + (decimal)row.CuryAdjgDiscAmt - (decimal)e.NewValue < 0)
			{
				throw new PXSetPropertyException(AR.Messages.Entry_LE, ((decimal)row.CuryDiscBal + (decimal)row.CuryAdjgDiscAmt).ToString());
			}
			if (row.CuryAdjgAmt != null && (sender.GetValuePending<ARAdjust.curyAdjgAmt>(e.Row) == PXCache.NotSetValue
												|| (Decimal?)sender.GetValuePending<ARAdjust.curyAdjgAmt>(e.Row) == row.CuryAdjgAmt))
			{
				if ((decimal)row.CuryDocBal + (decimal)row.CuryAdjgDiscAmt - (decimal)e.NewValue < 0)
				{
					throw new PXSetPropertyException(AR.Messages.Entry_LE, ((decimal)row.CuryDocBal + (decimal)row.CuryAdjgDiscAmt).ToString());
				}
			}
		}

		protected virtual void ARAdjust_CuryAdjgDiscAmt_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			ARAdjust row = (ARAdjust)e.Row;
			CalcBalances(row, true);
		}

		protected virtual void ARAdjust_CuryAdjgWOAmt_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			ARAdjust row = (ARAdjust)e.Row;

			if (row.CuryDocBal == null || row.CuryDiscBal == null || row.CuryWOBal == null)
			{
				CalcBalances(row, false, false);
			}

			if (row.CuryDocBal == null || row.CuryWOBal == null)
			{
				throw new PXSetPropertyException<ARAdjust.adjdRefNbr>(ErrorMessages.FieldIsEmpty, PXUIFieldAttribute.GetDisplayName<ARAdjust.adjdRefNbr>(sender));
			}

			if (row.VoidAdjNbr == null && (decimal)e.NewValue < 0m)
			{
				throw new PXSetPropertyException(CS.Messages.Entry_GE, ((int)0).ToString());
			}

			if (row.VoidAdjNbr != null && (decimal)e.NewValue > 0m)
			{
				throw new PXSetPropertyException(CS.Messages.Entry_LE, ((int)0).ToString());
			}

			if ((decimal)row.CuryWOBal + (decimal)row.CuryAdjgWOAmt - (decimal)e.NewValue < 0)
			{
				throw new PXSetPropertyException(AR.Messages.Entry_LE, ((decimal)row.CuryWOBal + (decimal)row.CuryAdjgWOAmt).ToString());
			}

			if (row.CuryAdjgAmt != null && (sender.GetValuePending<ARAdjust.curyAdjgAmt>(e.Row) == PXCache.NotSetValue || (Decimal?)sender.GetValuePending<ARAdjust.curyAdjgAmt>(e.Row) == row.CuryAdjgAmt))
			{
				if ((decimal)row.CuryDocBal + (decimal)row.CuryAdjgWOAmt - (decimal)e.NewValue < 0)
				{
					throw new PXSetPropertyException(AR.Messages.Entry_LE, ((decimal)row.CuryDocBal + (decimal)row.CuryAdjgWOAmt).ToString());
				}
			}
		}

		protected virtual void ARAdjust_CuryAdjgWOAmt_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			CalcBalances((ARAdjust)e.Row, true, false);
		}

		protected virtual void ARAdjust_RowInserting(PXCache sender, PXRowInsertingEventArgs e)
		{
			string errmsg = PXUIFieldAttribute.GetError<ARAdjust.adjdRefNbr>(sender, e.Row);
			e.Cancel = (((ARAdjust)e.Row).AdjdRefNbr == null || string.IsNullOrEmpty(errmsg) == false);
		}

		protected virtual void ARAdjust_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			ARAdjust adj = (ARAdjust)e.Row;
			if (adj == null)
			{
				return;
			}
			bool adjNotReleased = (adj.Released != true);
			PXUIFieldAttribute.SetEnabled<ARAdjust.adjdDocType>(cache, adj, adjNotReleased && (adj.Voided == false) && adj.AdjdRefNbr == null);
			PXUIFieldAttribute.SetEnabled<ARAdjust.adjdRefNbr>(cache, adj, adjNotReleased && (adj.Voided == false) && adj.AdjdRefNbr == null);
			PXUIFieldAttribute.SetEnabled<ARAdjust.curyAdjgAmt>(cache, adj, adjNotReleased && (adj.Voided == false));
			PXUIFieldAttribute.SetEnabled<ARAdjust.curyAdjgDiscAmt>(cache, adj, adjNotReleased && (adj.Voided == false));
			PXUIFieldAttribute.SetEnabled<ARAdjust.curyAdjgWOAmt>(cache, adj, adjNotReleased && (adj.Voided == false));
			PXUIFieldAttribute.SetVisible<ARAdjust.adjBatchNbr>(cache, adj, !adjNotReleased);
			PXUIFieldAttribute.SetEnabled<ARAdjust.adjBatchNbr>(cache, adj, false);
		}

		protected virtual void ARAdjust_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			ARAdjust doc = (ARAdjust)e.Row;
			GLTranDocAR parentDoc = (GLTranDocAR)this.ARPayments.Search<GLTranDocAR.tranType, GLTranDocAR.refNbr>(doc.AdjgDocType, doc.AdjgRefNbr);
			PXCache parentCache = this.ARPayments.Cache;
			if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Delete)
			{
				return;
			}
			if (parentDoc == null)
			{
				e.Cancel = true;
				return;
			}

			if (((DateTime)doc.AdjdDocDate).CompareTo((DateTime)parentDoc.TranDate) > 0)
			{
				if (sender.RaiseExceptionHandling<ARAdjust.adjdRefNbr>(e.Row, doc.AdjdRefNbr, new PXSetPropertyException(AR.Messages.ApplDate_Less_DocDate, PXErrorLevel.RowError, PXUIFieldAttribute.GetDisplayName<GLTranDocAR.tranDate>(parentCache))))
				{
					throw new PXRowPersistingException(PXDataUtils.FieldName<ARAdjust.adjdDocDate>(), doc.AdjdDocDate, AR.Messages.ApplDate_Less_DocDate, PXUIFieldAttribute.GetDisplayName<GLTranDocAR.tranDate>(parentCache));
				}
			}

			if (((string)doc.AdjdFinPeriodID).CompareTo((string)parentDoc.FinPeriodID) > 0)
			{
				if (sender.RaiseExceptionHandling<ARAdjust.adjdRefNbr>(e.Row, doc.AdjdRefNbr, new PXSetPropertyException(AR.Messages.ApplPeriod_Less_DocPeriod, PXErrorLevel.RowError, PXUIFieldAttribute.GetDisplayName<GLTranDocAR.finPeriodID>(parentCache))))
				{
					throw new PXRowPersistingException(PXDataUtils.FieldName<ARAdjust.adjdFinPeriodID>(), doc.AdjdFinPeriodID, AR.Messages.ApplPeriod_Less_DocPeriod, PXUIFieldAttribute.GetDisplayName<GLTranDocAR.finPeriodID>(parentCache));
				}
			}

			if (doc.CuryDocBal < 0m)
			{
				sender.RaiseExceptionHandling<ARAdjust.curyAdjgAmt>(e.Row, doc.CuryAdjgAmt, new PXSetPropertyException(AR.Messages.DocumentBalanceNegative));
			}

			if (doc.CuryDiscBal < 0m)
			{
				sender.RaiseExceptionHandling<ARAdjust.curyAdjgDiscAmt>(e.Row, doc.CuryAdjgDiscAmt, new PXSetPropertyException(AR.Messages.DocumentBalanceNegative));
			}

			if (doc.CuryWOBal < 0m)
			{
				sender.RaiseExceptionHandling<ARAdjust.curyAdjgWOAmt>(e.Row, doc.CuryAdjgWOAmt, new PXSetPropertyException(AR.Messages.DocumentBalanceNegative));
			}
			if (doc.CuryAdjgWOAmt > 0 && string.IsNullOrEmpty(doc.WriteOffReasonCode))
			{
				if (sender.RaiseExceptionHandling<ARAdjust.writeOffReasonCode>(e.Row, null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, PXUIFieldAttribute.GetDisplayName<ARAdjust.writeOffReasonCode>(sender))))
				{
					throw new PXRowPersistingException(PXDataUtils.FieldName<ARAdjust.writeOffReasonCode>(), null, ErrorMessages.FieldIsEmpty, PXUIFieldAttribute.GetDisplayName<ARAdjust.writeOffReasonCode>(sender));
				}
			}
		}

		protected virtual void ARAdjust_CuryAdjgWhTaxAmt_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			ARAdjust row = (ARAdjust)e.Row;
			CalcBalances(row, true);
		}

		public bool TakeDiscAlwaysAR = false;

		private static bool IsMatchingAdjd(ARAdjust adj, GLTranDoc invoice)
		{
			if (adj != null && invoice != null)
			{
				return (invoice.TranModule == GL.BatchModule.AR
				&& invoice.TranType == adj.AdjdDocType
				&& invoice.RefNbr == adj.AdjdRefNbr);
			}
			return false;
		}

		protected virtual GLTranDoc FindMatchingAdjd(ARAdjust row)
		{
			GLTranDoc invoice = PXSelect<GLTranDoc, Where<GLTranDoc.batchNbr, Equal<Current<GLDocBatch.batchNbr>>,
											And<GLTranDoc.parentLineNbr, IsNull,
											And<GLTranDoc.tranModule, Equal<GL.BatchModule.moduleAR>,
											And<GLTranDoc.bAccountID, Equal<Required<GLTranDoc.bAccountID>>,
											And<GLTranDoc.tranType, Equal<Required<APAdjust.adjdDocType>>,
											And<GLTranDoc.refNbr, Equal<Required<GLTranDoc.refNbr>>>>>>>>>.Select(this, row.CustomerID, row.AdjdDocType, row.AdjdRefNbr);
			return invoice;
		}

		private void CalcBalances(ARAdjust row, bool isCalcRGOL)
		{
			CalcBalances(row, isCalcRGOL, true);
		}

		private void CalcBalances(ARAdjust row, bool isCalcRGOL, bool DiscOnDiscDate)
		{
			ARAdjust adj = (ARAdjust)row;
			if (adj != null && adj.CustomerID != null)
			{
				Customer customer = PXSelectReadonly<Customer, Where<Customer.bAccountID, Equal<Required<Customer.bAccountID>>>>.Select(this, adj.CustomerID);
				foreach (ARInvoice voucher in ARInvoice_CustomerID_DocType_RefNbr.Select(adj.CustomerID, adj.AdjdDocType, adj.AdjdRefNbr))
				{
					CalcBalances<ARInvoice>(adj, customer, voucher, isCalcRGOL, DiscOnDiscDate);
					return;
				}

				GLTranDoc protoInvoice0 = FindMatchingAdjd(adj);
				if (protoInvoice0 != null)
				{
					GLTranDoc protoInvoice1 = (GLTranDoc)this.Caches[typeof(GLTranDoc)].CreateCopy(protoInvoice0);
					protoInvoice1.CuryApplAmt -= row.CuryAdjdAmt ?? Decimal.Zero;
					protoInvoice1.CuryDiscTaken -= row.CuryAdjdDiscAmt ?? Decimal.Zero;
					protoInvoice1.CuryTaxWheld -= row.CuryAdjdWhTaxAmt ?? Decimal.Zero;
					CalcBalances<GLTranDoc>(adj, customer, protoInvoice1, isCalcRGOL, DiscOnDiscDate);
					return;
				}
				foreach (ARPayment payment in ARPayment_CustomerID_DocType_RefNbr.Select(adj.CustomerID, adj.AdjdDocType, adj.AdjdRefNbr))
				{
					CalcBalances<ARPayment>(adj, customer, payment, isCalcRGOL, DiscOnDiscDate);
				}
			}
		}

		private bool _AutoPaymentApp = false;
		private void CalcBalances<T>(ARAdjust adj, Customer customer, T invoice, bool isCalcRGOL, bool DiscOnDiscDate)
			where T : class, IInvoice
		{
			if (this._AutoPaymentApp)
			{
				internalCallAR = true;
				ARAdjust unreleased = PXSelectGroupBy<ARAdjust, Where<ARAdjust.adjdDocType, Equal<Required<ARAdjust.adjdDocType>>,
								And<ARAdjust.adjdRefNbr, Equal<Required<ARAdjust.adjdRefNbr>>,
								And<ARAdjust.released, Equal<False>,
								And<ARAdjust.voided, Equal<False>,
								And<Where<ARAdjust.adjgDocType, NotEqual<Required<ARAdjust.adjgDocType>>,
								Or<ARAdjust.adjgRefNbr, NotEqual<Required<ARAdjust.adjgRefNbr>>>>>>>>>,
								Aggregate<GroupBy<ARAdjust.adjdDocType, GroupBy<ARAdjust.adjdRefNbr, Sum<ARAdjust.curyAdjdAmt, Sum<ARAdjust.adjAmt, Sum<ARAdjust.curyAdjdDiscAmt, Sum<ARAdjust.adjDiscAmt>>>>>>>>.Select(this, adj.AdjdDocType, adj.AdjdRefNbr, adj.AdjgDocType, adj.AdjgRefNbr);
				internalCallAR = false;
				if (unreleased != null && unreleased.AdjdRefNbr != null)
				{
					invoice.CuryDocBal -= (unreleased.CuryAdjdAmt + unreleased.CuryAdjdDiscAmt);
					invoice.DocBal -= (unreleased.AdjAmt + unreleased.AdjDiscAmt + unreleased.RGOLAmt);
					invoice.CuryDiscBal -= unreleased.CuryAdjdDiscAmt;
					invoice.DiscBal -= unreleased.AdjDiscAmt;
				}
				this._AutoPaymentApp = false;
			}

			PaymentEntry.CalcBalances<T, ARAdjust>(CurrencyInfo_CuryInfoID, adj.AdjgCuryInfoID, adj.AdjdCuryInfoID, invoice, adj);
			if (DiscOnDiscDate)
			{
				PaymentEntry.CalcDiscount<T, ARAdjust>(adj.AdjgDocDate, invoice, adj);
			}
			PaymentEntry.WarnDiscount<T, ARAdjust>(this, adj.AdjgDocDate, invoice, adj);
			if (customer != null && customer.SmallBalanceAllow == true && adj.AdjgDocType != ARDocType.Refund && adj.AdjdDocType != ARDocType.CreditMemo)
			{
				decimal payment_smallbalancelimit;
				CurrencyInfo payment_info = CurrencyInfo_CuryInfoID.Select(adj.AdjgCuryInfoID);
				PXDBCurrencyAttribute.CuryConvCury(CurrencyInfo_CuryInfoID.Cache, payment_info, customer.SmallBalanceLimit ?? 0m, out payment_smallbalancelimit);
				adj.CuryWOBal = payment_smallbalancelimit;
				adj.WOBal = customer.SmallBalanceLimit;
			}
			else
			{
				adj.CuryWOBal = 0m;
				adj.WOBal = 0m;
			}

			PaymentEntry.AdjustBalance<ARAdjust>(CurrencyInfo_CuryInfoID, adj);
			if (isCalcRGOL && (adj.Voided != true))
			{
				PaymentEntry.CalcRGOL<T, ARAdjust>(CurrencyInfo_CuryInfoID, invoice, adj);
				adj.RGOLAmt = (bool)adj.ReverseGainLoss ? -1m * adj.RGOLAmt : adj.RGOLAmt;
			}
		}

		#endregion
		#endregion

		#region TranType related Methods
		public static bool IsAPInvoice(string aModule, string aTranType)
		{
			if (aModule == GL.BatchModule.AP)
			{
				switch (aTranType)
				{
					case AP.APDocType.DebitAdj:
					case AP.APDocType.CreditAdj:
					case AP.APDocType.Invoice:
					case AP.APDocType.QuickCheck:
					case AP.APDocType.VoidQuickCheck:
						return true;
				}
			}
			return false;
		}

		public static bool IsAPPayment(string aModule, string aTranType)
		{
			if (aModule == GL.BatchModule.AP)
			{
				switch (aTranType)
				{
					case AP.APDocType.Check:
					case AP.APDocType.QuickCheck:
					case AP.APDocType.VoidQuickCheck:
					case AP.APDocType.Prepayment:
					case AP.APDocType.Refund:
                    case AP.APDocType.VoidRefund:
                    case AP.APDocType.VoidCheck:
						return true;
				}
			}
			return false;
		}

		public static bool IsARInvoice(string aModule, string aTranType)
		{
			if (aModule == GL.BatchModule.AR)
			{

				switch (aTranType)
				{
					case AR.ARDocType.DebitMemo:
					case AR.ARDocType.CreditMemo:
					case AR.ARDocType.Invoice:
					case AR.ARDocType.CashSale:
					case AR.ARDocType.CashReturn:


						return true;
				}
			}
			return false;
		}

		public static bool IsARPayment(string aModule, string aTranType)
		{
			if (aModule == GL.BatchModule.AR)
			{
				switch (aTranType)
				{
					case AR.ARDocType.Payment:
					//case AR.ARDocType.CreditMemo:  //???
					case AR.ARDocType.CashSale:
					case AR.ARDocType.CashReturn:
					case AR.ARDocType.Prepayment:
					case AR.ARDocType.Refund:
                    case AR.ARDocType.VoidRefund:
                    case AR.ARDocType.VoidPayment:
						return true;
				}
			}
			return false;
		}

		public static bool IsAPPrePayment(string aModule, string aTranType)
		{
			if (aModule == GL.BatchModule.AP && aTranType == AP.APDocType.Prepayment)
			{
				return true;
			}
			return false;
		}

		public static bool IsARInvoice(GLTranDoc aRow)
		{
			return IsARInvoice(aRow.TranModule, aRow.TranType);
		}

		public static bool IsARCreditMemo(GLTranDoc aRow)
		{
			if (aRow.TranModule == GL.BatchModule.AR && aRow.TranType == AR.ARDocType.CreditMemo)
			{
				return true;
			}
			return false;
		}

		public static bool IsARPayment(GLTranDoc aRow)
		{
			return IsARPayment(aRow.TranModule, aRow.TranType);
		}

		public static bool IsAPInvoice(GLTranDoc aRow)
		{
			return IsAPInvoice(aRow.TranModule, aRow.TranType);
		}

		public static bool IsAPPayment(GLTranDoc aRow)
		{
			return IsAPPayment(aRow.TranModule, aRow.TranType);
		}

		public static bool IsAPPrePayment(GLTranDoc aRow)
		{
			return IsAPPayment(aRow.TranModule, aRow.TranType);
		}

		public static bool IsMixedType(GLTranDoc row)
		{
			bool result = false;
			if (row.TranModule == GL.BatchModule.AP)
			{
				result = (row.TranType == AP.APDocType.QuickCheck || row.TranType == AP.APDocType.VoidQuickCheck);
			}
			if (row.TranModule == GL.BatchModule.AR)
			{
				result = (row.TranType == AR.ARDocType.CashSale || row.TranType == AR.ARDocType.CashReturn);
			}
			return result;
		}

		protected static bool? IsDebitType(GLTranDoc tran)
		{
			return IsDebitType(tran, false);
		}

		protected static bool IsDrCrAcctRequired(GLTranDoc row, bool aCredit)
		{
			bool isDebitRequired = true;
			bool isCreditRequired = true;
			bool? isDebit = IsDebitType(row, true);
			if (isDebit.HasValue)
			{
				if (row.IsChildTran)
				{
					if (isDebit.HasValue)
					{
						isDebitRequired = isDebit.Value;
						isCreditRequired = !isDebit.Value;
					}
				}
				else
				{
					bool hasChildren = row.Split.Value;
					isDebitRequired = (isDebit == false) || (isDebit.Value && !hasChildren);
					isCreditRequired = isDebit.Value || (isDebit.Value == false && !hasChildren);
				}
			}
			else
			{
				isDebitRequired = isCreditRequired = false;
			}
			return aCredit ? isCreditRequired : isDebitRequired;
		}

		protected static bool? IsDebitType(GLTranDoc aRow, bool aAsInvoice)
		{
			bool isMixedType = IsMixedType(aRow);
			if (aRow.TranModule == GL.BatchModule.AP)
			{
				if (isMixedType)
				{
					return ((aAsInvoice ? AP.APInvoiceType.DrCr(aRow.TranType) : AP.APPaymentType.DrCr(aRow.TranType)) == DrCr.Debit);
				}
				else
				{
					if (IsAPInvoice(aRow))
					{
						return (AP.APInvoiceType.DrCr(aRow.TranType) == DrCr.Debit);
					}
					if (IsAPPayment(aRow))
					{
						return (AP.APPaymentType.DrCr(aRow.TranType) == DrCr.Debit);
				}
			}
			}
			if (aRow.TranModule == GL.BatchModule.AR)
			{
				if (isMixedType)
				{
					return ((aAsInvoice ? AR.ARInvoiceType.DrCr(aRow.TranType) : AR.ARPaymentType.DrCr(aRow.TranType)) == DrCr.Debit);
				}
				else
				{
					if (IsARInvoice(aRow))
					{
						return (AR.ARInvoiceType.DrCr(aRow.TranType) == DrCr.Debit);
					}
					if (IsARPayment(aRow))
					{
						return (AR.ARPaymentType.DrCr(aRow.TranType) == DrCr.Debit);
				}
			}
			}
			if (aRow.TranModule == GL.BatchModule.CA)
			{
				return (aRow.CADrCr == CADrCr.CACredit); //Inverted compare to others
			}
			if (aRow.TranModule == GL.BatchModule.GL)
			{
				return null;
			}

			return false;
		}

		protected static bool? IsDebitTran(GLTranDoc aRow)
		{
			if (aRow.DebitAccountID.HasValue == aRow.CreditAccountID.HasValue)
			{
				return null;
			}
			return aRow.DebitAccountID.HasValue;
		}

		protected static bool HasDocumentRow(GLTranDoc aRow)
		{
			//For all types except GL Batch parent represents a Document itself. GL Batch has only details.
			return (aRow.TranModule != GL.BatchModule.GL);
		}

		protected static decimal GetSignedAmount(GLTranDoc aRow)
		{
			decimal result = Decimal.Zero;
			if (aRow.IsBalanced)
			{
				return result;
			}
			bool? dr = IsDebitTran(aRow);
			if (dr.HasValue)
			{
				result = dr.Value ? (aRow.CuryTranTotal ?? Decimal.Zero) : (-aRow.CuryTranTotal ?? Decimal.Zero);
			}
			return result;
		}

		#endregion

		#region Implementation of IPXPrepareItems

		protected Dictionary<Pair<string, string>, int> _ImportedDocs;
		public virtual bool PrepareImportRow(string viewName, IDictionary keys, IDictionary values)
		{

			if (viewName == "GLTranModuleBatNbr")
			{
				if (this._ImportedDocs == null)
				{
					this._ImportedDocs = new Dictionary<Pair<string, string>, int>();
				}
				var creditAmt = CorrectImportValue(values, "CreditAmt", "0");
				CorrectImportValue(values, "CuryCreditAmt", creditAmt);
				var debitAmt = CorrectImportValue(values, "DebitAmt", "0");
				CorrectImportValue(values, "CuryDebitAmt", debitAmt);

				if (values.Contains("RefNbr"))
				{
					if (values.Contains("Split"))
					{
						string split = values["Split"].ToString();
						bool isSplit = Boolean.Parse(split);
						if (isSplit)
						{
							if (values.Contains("TranCode"))
							{
								Pair<string, string> key = new Pair<string, string>((string)values["TranCode"], (string)values["RefNbr"]);
								if (this._ImportedDocs.ContainsKey(key))
								{
									values["ParentLineNbr"] = this._ImportedDocs[key].ToString();
								}
								else
								{
									if (keys["LineNbr"] != null)
									{
										string lineN = (keys["LineNbr"].ToString());
										int lineNbr = int.Parse(lineN);
										this._ImportedDocs[key] = lineNbr;
									}
									else
									{
										string TranCode = ((string)values["TranCode"]).Trim();
										string RefNbr = ((string)values["RefNbr"]).Trim();
										foreach (GLTranDoc parentDoc in this.Caches[typeof(GLTranDoc)].Inserted)
										{
											if (parentDoc.TranCode == TranCode && RefNbr == parentDoc.ImportRefNbr)
											{
												if (parentDoc.LineNbr.HasValue)
												{
													this._ImportedDocs[key] = parentDoc.LineNbr.Value;
													values["ParentLineNbr"] = this._ImportedDocs[key].ToString();
												}
												break;
											}
										}
									}
								}
							}
						}
					}
					values["ImportRefNbr"] = values["RefNbr"];
					values["RefNbr"] = null;
				}
				CorrectImportEmptyStrings(values, "EntryTypeID", null);
				CorrectImportEmptyStrings(values, "DebitAccountID", null);
				CorrectImportEmptyStrings(values, "CreditAccountID", null);
				CorrectImportEmptyStrings(values, "TaxCategoryID", null);
				CorrectImportEmptyStrings(values, "TaxZoneID", null);
				CorrectImportEmptyStrings(values, "TermsID", null);
				CorrectImportEmptyStrings(values, "DebitSubID", null);
				CorrectImportEmptyStrings(values, "CreditSubID", null);
				CorrectImportEmptyStrings(values, "PaymentMethodID", null);
				CorrectImportEmptyStrings(values, "BAccountID", null);
				CorrectImportEmptyStrings(values, "LocationID", null);
			}
			return true;
		}

		public virtual void PrepareItems(string viewName, IEnumerable items)
		{
		}

		private static string CorrectImportEmptyStrings(IDictionary dic, string fieldName, string defValue)
		{
			string result = defValue;
			if (!dic.Contains(fieldName))
			{
				dic.Add(fieldName, defValue);
			}
			else
			{
				string val = dic[fieldName] == null ? null : dic[fieldName].ToString();

				if (String.IsNullOrEmpty(val) || String.IsNullOrEmpty(val.Trim()))
				{
					dic[fieldName] = defValue;
				}
				else
				{
					result = val;
				}
			}
			return result;
		}

		private static string CorrectImportValue(IDictionary dic, string fieldName, string defValue)
		{
			var result = defValue;
			if (!dic.Contains(fieldName))
			{
				dic.Add(fieldName, defValue);
			}
			else
			{
				var val = dic[fieldName];
				decimal mVal;
				string sVal;
				if (val == null ||
					string.IsNullOrEmpty(sVal = val.ToString()) ||
					!decimal.TryParse(sVal, out mVal))
				{
					dic[fieldName] = defValue;
				}
				else
				{
					result = sVal;
				}
			}
			return result;
		}

		#endregion

		#region IPXPrepareItems Members

		public bool RowImported(string viewName, object row, object oldRow)
		{
			//  throw new NotImplementedException();
			return true;
		}

		public bool RowImporting(string viewName, object row)
		{
			//throw new NotImplementedException();
			return true;
		}

		#endregion
	}

	[PXHidden]
	[Serializable]
	public class GLBatchDocRelease : PXGraph<GLBatchDocRelease>
	{

		#region Internal Type Definition
		protected bool _suppressARCreditVerification = true;
		protected bool _suppressARPrintVerification = true;

		[PXProjection(typeof(Select<GLTranDoc>), Persistent = true)]
		[Serializable]
		public partial class GLTranDocU : IBqlTable
		{
			#region BranchID
			public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
			protected Int32? _BranchID;
			[PXDBInt(BqlField = typeof(GLTranDoc.branchID), IsKey = true)]
			public virtual Int32? BranchID
			{
				get
				{
					return this._BranchID;
				}
				set
				{
					this._BranchID = value;
				}
			}
			#endregion
			#region Module
			public abstract class module : PX.Data.BQL.BqlString.Field<module> { }
			protected String _Module;
			[PXDBString(2, IsKey = true, IsFixed = true, BqlField = typeof(GLTranDoc.module))]
			[PXUIField(DisplayName = "Module", Visibility = PXUIVisibility.Visible, Visible = false)]
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
			[PXDBString(15, IsUnicode = true, IsKey = true, BqlField = typeof(GLTranDoc.batchNbr))]
			[PXUIField(DisplayName = "Batch Number", Visibility = PXUIVisibility.Visible, Visible = false)]
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
			#region LineNbr
			public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }
			protected Int32? _LineNbr;
			[PXDBInt(IsKey = true, BqlField = typeof(GLTranDoc.lineNbr))]
			[PXDefault()]
			[PXUIField(DisplayName = "Line Nbr.", Visibility = PXUIVisibility.Visible, Visible = false, Enabled = false)]
			public virtual Int32? LineNbr
			{
				get
				{
					return this._LineNbr;
				}
				set
				{
					this._LineNbr = value;
				}
			}
			#endregion
			#region RefNbr
			public abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }
			protected String _RefNbr;
			[PXDBString(15, IsUnicode = true, BqlField = typeof(GLTranDoc.refNbr))]
			[PXUIField(DisplayName = "Ref. Number", Visibility = PXUIVisibility.Visible, Visible = false)]
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
			#region DocCreated
			public abstract class docCreated : PX.Data.BQL.BqlBool.Field<docCreated> { }

			protected Boolean? _DocCreated;
			[PXDBBool(BqlField = typeof(GLTranDoc.docCreated))]
			[PXDefault(false)]
			public virtual Boolean? DocCreated
			{
				get
				{
					return this._DocCreated;
				}
				set
				{
					this._DocCreated = value;
				}
			}
			#endregion

			#region Released
			public abstract class released : PX.Data.BQL.BqlBool.Field<released> { }

			protected Boolean? _Released;
			[PXDBBool(BqlField = typeof(GLTranDoc.released))]
			[PXDefault(false)]
			public virtual Boolean? Released
			{
				get
				{
					return this._Released;
				}
				set
				{
					this._Released = value;
				}
			}
			#endregion
		}
		#endregion
		#region Selects
		public PXSelect<GLDocBatch, Where<GLDocBatch.module, Equal<Required<GLDocBatch.module>>,
								And<GLDocBatch.batchNbr, Equal<Required<GLDocBatch.batchNbr>>>>> batch;
		public PXSelect<GLTranDoc,
					Where<GLTranDoc.module, Equal<Current<GLDocBatch.module>>,
						And<GLTranDoc.batchNbr, Equal<Current<GLDocBatch.batchNbr>>>>> details;
		public PXSelect<GLTaxTran,
					Where<GLTaxTran.module, Equal<Required<GLTaxTran.module>>,
						And<GLTaxTran.batchNbr, Equal<Required<GLTaxTran.batchNbr>>,
						And<GLTaxTran.lineNbr, Equal<Required<GLTaxTran.lineNbr>>>>>> taxTotals;

		public PXSelect<GLTranDocU, Where<GLTranDocU.module, Equal<Current<GLDocBatch.module>>,
						And<GLTranDocU.batchNbr, Equal<Current<GLDocBatch.batchNbr>>>>> detailUpdate;
		#endregion
		#region Public Methods
		public void ReleaseBatchProc(GLDocBatch aBatch, bool useLongOperationErrorHandling)
		{
			this.Clear();
			GLDocBatch row = this.batch.Select(aBatch.Module, aBatch.BatchNbr);
			this.batch.Current = row;
			this.CreateBatchDetailsProc(aBatch, useLongOperationErrorHandling);
			//Release
			//this.ReleaseBatchDetailsProc(aBatch);			
			if (HasUnreleasedDetails(aBatch) == false)
			{
				GLDocBatch copy = (GLDocBatch)this.batch.Cache.CreateCopy(row);
				copy.Released = true;
				this.batch.Update(copy);
				this.Actions.PressSave();
			}
		}

		#endregion
		#region Private Methods
		private void CreateDocProc(KeyValuePair<int, List<GLTranDoc>> aDocInfo, List<Batch> toPost)
		{
			GLTranDoc parent = null;
			foreach (GLTranDoc iDoc in aDocInfo.Value)
			{
				if (iDoc.LineNbr.Value == aDocInfo.Key)
				{
					parent = iDoc;
				}
			}
			PXGraph result = null;
			if (String.IsNullOrEmpty(parent.RefNbr) || parent.DocCreated == false)
			{
				if (parent.TranModule == GL.BatchModule.AP)
				{
					result = CreateAP(parent, aDocInfo.Value, toPost);
					if (result == null)
					{
						throw new PXException(Messages.DocLineHasUnvalidOrUnsupportedType, parent.LineNbr, parent.Module, parent.TranType);
					}
				}

				if (parent.TranModule == GL.BatchModule.AR)
				{
					result = CreateAR(parent, aDocInfo.Value, toPost);
					if (result == null)
					{
						throw new PXException(Messages.DocLineHasUnvalidOrUnsupportedType, parent.LineNbr, parent.Module, parent.TranType);
					}
				}

				if (parent.TranModule == GL.BatchModule.CA)
				{
					result = CreateCA(parent, aDocInfo.Value, toPost);
					if (result == null)
					{
						throw new PXException(Messages.DocLineHasUnvalidOrUnsupportedType, parent.LineNbr, parent.Module, parent.TranType);
					}
				}

				if (parent.TranModule == GL.BatchModule.GL)
				{
					result = CreateGL(parent, aDocInfo.Value, toPost);
					if (result == null)
					{
						throw new PXException(Messages.DocLineHasUnvalidOrUnsupportedType, parent.LineNbr, parent.Module, parent.TranType);
					}
				}

				foreach (GLTranDoc iDoc in aDocInfo.Value)
				{
					GLTranDocU copy = new GLTranDocU();
					Copy(copy, iDoc);
					copy.RefNbr = parent.RefNbr;
					copy.DocCreated = parent.DocCreated;
					copy.Released = parent.Released;
					copy = this.detailUpdate.Update(copy);
				}
			}
		}
		private void CreateBatchDetailsProc(GLDocBatch aBatch, bool useLongOperationErrorHandling)
		{
			Dictionary<int, List<GLTranDoc>> tranGroups0 = new Dictionary<int, List<GLTranDoc>>();
			Dictionary<int, List<GLTranDoc>> tranGroups1 = new Dictionary<int, List<GLTranDoc>>();
			//Result messages collection
			Dictionary<long, CAMessage> listMessages = new Dictionary<long, CAMessage>();

			foreach (GLTranDoc iDetail in details.Select())
			{
				int key = iDetail.IsChildTran ? iDetail.ParentLineNbr.Value : iDetail.LineNbr.Value;
				bool firstPriority = JournalWithSubEntry.IsMixedType(iDetail) == false && (JournalWithSubEntry.IsAPInvoice(iDetail) || JournalWithSubEntry.IsARInvoice(iDetail));
				Dictionary<int, List<GLTranDoc>> tranGroups = firstPriority ? tranGroups0 : tranGroups1;
				if (tranGroups.ContainsKey(key) == false)
				{
					tranGroups.Add(key, new List<GLTranDoc>(1));
				}
				GLTranDoc copy = (GLTranDoc)this.details.Cache.CreateCopy(iDetail);
				tranGroups[key].Add(copy);
			}
			List<int> failed = new List<int>();
			List<Batch> toPost = new List<Batch>(tranGroups0.Count + tranGroups1.Count);
			Dictionary<int, List<GLTranDoc>>[] processQueue = { tranGroups0, tranGroups1 };
			foreach (Dictionary<int, List<GLTranDoc>> iTranGroups in processQueue)
			{
				foreach (KeyValuePair<int, List<GLTranDoc>> it in iTranGroups)
				{
					try
					{
						using (new PXTimeStampScope(this.TimeStamp))
						{
							using (PXTransactionScope ts = new PXTransactionScope())
							{
								CreateDocProc(it, toPost);
								this.Actions.PressSave(); //PerLine save
								ts.Complete();
							}
						}
						listMessages.Add(it.Key, new CAMessage(it.Key, PXErrorLevel.RowInfo, ActionsMessages.RecordProcessed));
					}
					catch (Exception ex)
					{
						failed.Add(it.Key); //prevent process from stoping on first error					
						string message = ex is PXOuterException ? (ex.Message + "\r\n" + String.Join("\r\n", ((PXOuterException)ex).InnerMessages)) : ex.Message;
						listMessages.Add(it.Key, new CAMessage(it.Key, PXErrorLevel.RowError, message));
					}
				}
			}

			if (useLongOperationErrorHandling && listMessages.Count > 0)
			{
				PXLongOperation.SetCustomInfoPersistent(listMessages);
			}

			List<Batch> postFailedList = new List<Batch>();
			if (toPost.Count > 0)
			{
				PostGraph pg = PXGraph.CreateInstance<PostGraph>();

				foreach (Batch iBatch in toPost)
				{
					try
					{
						//if (rg.AutoPost)
						{
							pg.Clear();
							pg.PostBatchProc(iBatch);
						}
					}
					catch (Exception)
					{
						postFailedList.Add(iBatch);
					}
				}
			}
			if (failed.Count > 0)
			{
				throw new PXException(Messages.CreationOfSomeOfTheIncludedDocumentsFailed, failed.Count, processQueue.Sum(i => i.Count));
			}
			if (postFailedList.Count > 0)
			{
				throw new PXException(Messages.PostingOfSomeOfTheIncludedDocumentsFailed, postFailedList.Count, toPost.Count);
			}
		}
		private bool HasUnreleasedDetails(GLDocBatch aBatch)
		{
			foreach (PXResult<GLTranDoc, AP.APRegister, ARRegister, CAAdj, Batch> it in PXSelectReadonly2<GLTranDoc,
								LeftJoin<AP.APRegister, On<AP.APRegister.docType, Equal<GLTranDoc.tranType>,
														And<AP.APRegister.refNbr, Equal<GLTranDoc.refNbr>,
															And<GLTranDoc.tranModule, Equal<GL.BatchModule.moduleAP>>>>,
								LeftJoin<ARRegister, On<ARRegister.docType, Equal<GLTranDoc.tranType>,
														And<ARRegister.refNbr, Equal<GLTranDoc.refNbr>,
															And<GLTranDoc.tranModule, Equal<GL.BatchModule.moduleAR>>>>,
								LeftJoin<CAAdj, On<CAAdj.adjTranType, Equal<GLTranDoc.tranType>,
												And<CAAdj.adjRefNbr, Equal<GLTranDoc.refNbr>,
												And<GLTranDoc.tranModule, Equal<GL.BatchModule.moduleCA>>>>,
								LeftJoin<Batch, On<Batch.batchNbr, Equal<GLTranDoc.refNbr>,
													And<GLTranDoc.tranModule, Equal<GL.BatchModule.moduleGL>>>>>>>,
								Where<GLTranDoc.module, Equal<Required<GLTranDoc.module>>,
								And<GLTranDoc.batchNbr, Equal<Required<GLTranDoc.batchNbr>>,
									And<GLTranDoc.parentLineNbr, IsNull>>>>.Select(this, aBatch.Module, aBatch.BatchNbr))
			{
				AP.APRegister apDoc = it;
				ARRegister arDoc = it;
				CAAdj caDoc = it;
				GLTranDoc glDoc = it;
				Batch glBatch = it;
				if (glDoc.TranModule == GL.BatchModule.AR && (arDoc == null || String.IsNullOrEmpty(arDoc.RefNbr) == true || arDoc.Released == false)) { return true; }
				if (glDoc.TranModule == GL.BatchModule.AP && (apDoc == null || String.IsNullOrEmpty(apDoc.RefNbr) == true || apDoc.Released == false)) { return true; }
				if (glDoc.TranModule == GL.BatchModule.CA && (apDoc == null || String.IsNullOrEmpty(caDoc.RefNbr) == true || caDoc.Released == false)) { return true; }
				if (glDoc.TranModule == GL.BatchModule.GL && (glBatch == null || String.IsNullOrEmpty(glBatch.BatchNbr) == true || glBatch.Released == false)) { return true; }
				if (glDoc == null || String.IsNullOrEmpty(apDoc.RefNbr) == true && String.IsNullOrEmpty(arDoc.RefNbr) && String.IsNullOrEmpty(caDoc.RefNbr) && String.IsNullOrEmpty(glBatch.BatchNbr))
				{
					if (String.IsNullOrEmpty(glDoc.RefNbr) == false)
					{
						throw new PXException(Messages.UnsupportedTypeOfTheDocumentIsDetected, glDoc.LineNbr, glDoc.TranModule, glDoc.TranType, glDoc.RefNbr);
					}
					else
					{
						throw new PXException(Messages.DocumentWasNotCreatedForTheLine, glDoc.TranLineNbr, glDoc.TranCode);
					}
				}
			}
			return false;
		}

		protected virtual PXGraph CreateAP(GLTranDoc doc, List<GLTranDoc> aList, List<Batch> toPost)
		{
			PXGraph result = null;
			bool multiLine = aList.Count > 1;
			APRegister apRegister = null;
			AP.Standalone.APInvoice apInvoice = null;
			AP.Standalone.APPayment apPayment = null;
			Batch batch = null;
			List<APRegister> releaseList = new List<APRegister>(1);
			if (string.IsNullOrEmpty(doc.RefNbr) == false)
			{
				PXResultset<APRegister> res = PXSelectJoin<APRegister,
					LeftJoin<AP.Standalone.APInvoice, On<AP.Standalone.APInvoice.refNbr, Equal<APRegister.refNbr>, And<AP.Standalone.APInvoice.docType, Equal<APRegister.docType>>>,
					LeftJoin<AP.Standalone.APPayment, On<AP.Standalone.APPayment.refNbr, Equal<APRegister.refNbr>, And<AP.Standalone.APPayment.docType, Equal<APRegister.docType>>>,
					LeftJoin<Batch, On<Batch.origModule, Equal<BatchModule.moduleAP>, And<Batch.batchNbr, Equal<APRegister.batchNbr>>>>>>,
					Where<APRegister.docType, Equal<Required<APRegister.docType>>,
						And<APRegister.refNbr, Equal<Required<APRegister.refNbr>>>>>.SelectWindowed(this, 0, 1, doc.TranType, doc.RefNbr);
				if (res.Count > 0)
				{
					apRegister = (APRegister)res[0];
					apInvoice = (AP.Standalone.APInvoice)(PXResult<APRegister, AP.Standalone.APInvoice, AP.Standalone.APPayment, Batch>)res[0];
					apPayment = (AP.Standalone.APPayment)(PXResult<APRegister, AP.Standalone.APInvoice, AP.Standalone.APPayment, Batch>)res[0];
					batch = (Batch)(PXResult<APRegister, AP.Standalone.APInvoice, AP.Standalone.APPayment, Batch>)res[0];
				}
			}
			if (JournalWithSubEntry.IsAPInvoice(doc) && JournalWithSubEntry.IsAPPayment(doc))
			{
				AP.APQuickCheckEntry qcGraph = PXGraph.CreateInstance<AP.APQuickCheckEntry>();
				qcGraph.apsetup.Current.HoldEntry = false;
				APQuickCheck copy = null, row = null;
				if (apRegister != null)
				{
					if ((apInvoice != null && !String.IsNullOrEmpty(apInvoice.RefNbr)) || (apPayment != null && !String.IsNullOrEmpty(apPayment.RefNbr)) || (batch != null && !String.IsNullOrEmpty(batch.BatchNbr)))
					{
						throw new PXException(Messages.DocumentAlreadyExists);
					}
					Copy(apRegister, doc);
					//apRegister.CuryInfoID = null;
					ClearEmployeeIDField(apRegister, qcGraph);
					copy = (APQuickCheck)qcGraph.Document.Cache.Extend<APRegister>(apRegister);
				}
				else
				{
					row = new APQuickCheck();
					row.DocType = doc.TranType;
					qcGraph.Document.Current = qcGraph.Document.Insert(row);
					copy = (APQuickCheck)qcGraph.Document.Cache.CreateCopy(qcGraph.Document.Current);
				}
				PXUIFieldAttribute.SetError(qcGraph.Document.Cache, row, null, null);

				Copy(copy, doc);
				copy.Hold = true;
				row = qcGraph.Document.Update(copy);
				row.TaxCalcMode = TaxCalculationMode.TaxSetting;
				row = qcGraph.Document.Update(copy);

				CloneCuryInfo(qcGraph.currencyinfo, doc.CuryInfoID);
				foreach (GLTranDoc iRow in aList)
				{
					if (multiLine && Object.ReferenceEquals(doc, iRow)) continue;
					APTran tran = new AP.APTran();
					tran = qcGraph.Transactions.Insert(tran);
					APTran tranCopy = (APTran)qcGraph.Transactions.Cache.CreateCopy(tran);
					Copy(tranCopy, iRow);
					tran = qcGraph.Transactions.Update(tranCopy);
                    Copy(tran, iRow);
                    tran = qcGraph.Transactions.Update(tran);
					SetAccountAndSub(iRow, tran);
					tran = qcGraph.Transactions.Update(tran);
				}

				copy.CuryOrigDocAmt = doc.CuryDocTotal;
				copy.CuryOrigDiscAmt = doc.CuryDiscAmt;
				copy.Hold = false;
				row = qcGraph.Document.Update(copy);

				TaxCalc taxCalc = APTaxAttribute.GetTaxCalc<APTran.taxCategoryID>(qcGraph.Transactions.Cache, null);
				APTaxAttribute.SetTaxCalc<APTran.taxCategoryID>(qcGraph.Transactions.Cache, null, TaxCalc.ManualCalc);
				MergeAPTaxes(qcGraph.Taxes, doc);
				APTaxAttribute.SetTaxCalc<APTran.taxCategoryID>(qcGraph.Transactions.Cache, null, taxCalc);
				qcGraph.Save.Press();
				doc.RefNbr = row.RefNbr;
				doc.DocCreated = true;
				result = qcGraph;
				releaseList.Add(row);
			}

			if (result == null && JournalWithSubEntry.IsAPInvoice(doc))
			{
				APInvoiceEntry qcGraph = PXGraph.CreateInstance<AP.APInvoiceEntry>();
				qcGraph.APSetup.Current.HoldEntry = false;
				APInvoice copy = null, row = null;
				if (apRegister != null)
				{
					if ((apInvoice != null && !String.IsNullOrEmpty(apInvoice.RefNbr)) || (apPayment != null && !String.IsNullOrEmpty(apPayment.RefNbr)) || (batch != null && !String.IsNullOrEmpty(batch.BatchNbr)))
					{
						throw new PXException(Messages.DocumentAlreadyExists);
					}
					Copy(apRegister, doc);
					ClearEmployeeIDField(apRegister, qcGraph);
					//apRegister.CuryInfoID = null;
					copy = (APInvoice)qcGraph.Document.Cache.Extend<APRegister>(apRegister);
				}
				else
				{
					row = new AP.APInvoice();
					row.DocType = doc.TranType;
					qcGraph.Document.Current = qcGraph.Document.Insert(row);
					copy = (AP.APInvoice)qcGraph.Document.Cache.CreateCopy(qcGraph.Document.Current);
				}

				Copy(copy, doc);
				copy.Hold = true;
				row = qcGraph.Document.Update(copy);
				row.TaxCalcMode = TaxCalculationMode.TaxSetting;
				row = qcGraph.Document.Update(copy);

				CloneCuryInfo(qcGraph.currencyinfo, doc.CuryInfoID);
				foreach (GLTranDoc iRow in aList)
				{
					if (multiLine && Object.ReferenceEquals(doc, iRow)) continue;
					APTran tran = new AP.APTran();
					tran = qcGraph.Transactions.Insert(tran);
					APTran tranCopy = (APTran)qcGraph.Transactions.Cache.CreateCopy(tran);
					Copy(tranCopy, iRow);
					tran = qcGraph.Transactions.Update(tranCopy);
                    Copy(tran, iRow);
                    tran = qcGraph.Transactions.Update(tran);
					SetAccountAndSub(iRow, tran);
					tran = qcGraph.Transactions.Update(tran);
				}

				copy.CuryOrigDocAmt = doc.CuryDocTotal;
				copy.CuryOrigDiscAmt = doc.CuryDiscAmt;

				if (doc.DocType == APDocType.QuickCheck)
				{
					copy.CuryOrigDocAmt -= copy.CuryOrigDiscAmt;
				}
				copy.Hold = false;
				row = qcGraph.Document.Update(copy);

				TaxCalc taxCalc = APTaxAttribute.GetTaxCalc<APTran.taxCategoryID>(qcGraph.Transactions.Cache, null);
				APTaxAttribute.SetTaxCalc<APTran.taxCategoryID>(qcGraph.Transactions.Cache, null, TaxCalc.ManualCalc);
				MergeAPTaxes(qcGraph.Taxes, doc);
				APTaxAttribute.SetTaxCalc<APTran.taxCategoryID>(qcGraph.Transactions.Cache, null, taxCalc);
				qcGraph.Save.Press();
				doc.RefNbr = row.RefNbr;
				doc.DocCreated = true;
				result = qcGraph;
				releaseList.Add(row);
			}

			if (result == null && JournalWithSubEntry.IsAPPayment(doc))
			{
				if (doc.CuryUnappliedBal != Decimal.Zero && doc.TranType != APDocType.Prepayment)
				{
					throw new PXException(Messages.DocumentMustByAppliedInFullBeforeItMayBeReleased);
				}
				AP.APPaymentEntry qcGraph = PXGraph.CreateInstance<AP.APPaymentEntry>();
				APPayment copy = null, row = null;
				if (apRegister != null)
				{
					if ((apInvoice != null && !String.IsNullOrEmpty(apInvoice.RefNbr)) || (apPayment != null && !String.IsNullOrEmpty(apPayment.RefNbr)) || (batch != null && !String.IsNullOrEmpty(batch.BatchNbr)))
					{
						throw new PXException(Messages.DocumentAlreadyExists);
					}
					Copy(apRegister, doc);
					ClearEmployeeIDField(apRegister, qcGraph);
					//apRegister.CuryInfoID = null;
					copy = (APPayment)qcGraph.Document.Cache.CreateCopy(qcGraph.Document.Cache.Extend<APRegister>(apRegister));
				}
				else
				{
					row = new AP.APPayment();
					row.DocType = doc.TranType;
					qcGraph.Document.Current = qcGraph.Document.Insert(row);
					copy = (AP.APPayment)qcGraph.Document.Cache.CreateCopy(qcGraph.Document.Current);
				}
				PXUIFieldAttribute.SetError(qcGraph.Document.Cache, row, null, null);
				Copy(copy, qcGraph.Document.Current, doc);

				row = qcGraph.Document.Update(copy);
				//PXUIFieldAttribute.SetError(qcGraph.Document.Cache, row, null, null);
				//Need to be after update - cash acctount assigment calls defaulting of the CurrencyInfo 
				CloneCuryInfo(qcGraph.currencyinfo, doc.CuryInfoID);
				qcGraph.RecalcApplAmounts(qcGraph.Document.Cache, row, false);
				row = qcGraph.Document.Update(row);
				row.IsReleaseCheckProcess = true;
				qcGraph.Save.Press();
				doc.RefNbr = row.RefNbr;
				doc.DocCreated = true;
				result = qcGraph;
				releaseList.Add(row);
			}
			if (releaseList.Count > 0)
			{
				APDocumentRelease.ReleaseDoc(releaseList, false, false, toPost);
				doc.Released = true;
			}
			return result;
		}

		/// <summary>
		/// Workaround for AC-162943
		/// </summary>
		/// <param name="apRegister"></param>
		/// <param name="qcGraph"></param>
		private static void ClearEmployeeIDField(APRegister apRegister, PXGraph qcGraph)
		{
			if (apRegister.EmployeeID != null)
			{
				var ePEmployeeEPCompanyTreeMemberResult = PXSelectJoin<EPEmployee,
					LeftJoin<TM.EPCompanyTreeMember,
						On<TM.EPCompanyTreeMember.userID, Equal<EPEmployee.pKID>>>,
					Where<TM.EPCompanyTreeMember.userID, Equal<Required<APInvoice.employeeID>>>>
				.Select(qcGraph, apRegister.EmployeeID);

				BAccount bAccount = PXSelect<BAccount, Where<BAccount.bAccountID, Equal<Required<APRegister.vendorID>>>>.Select(qcGraph, apRegister.VendorID);

				bool shouldBeCleared = true;
				foreach (PXResult<EPEmployee, TM.EPCompanyTreeMember> item in ePEmployeeEPCompanyTreeMemberResult)
				{
					TM.EPCompanyTreeMember member = item;
					if (member?.WorkGroupID == bAccount?.WorkgroupID)
					{
						shouldBeCleared = false;
					}
				}

				if (shouldBeCleared)
				{
					apRegister.EmployeeID = null;
				}
			}
		}

		private static void SetAccountAndSub(GLTranDoc iRow, APTran tran)
		{
			bool isDirect = AP.APInvoiceType.DrCr(iRow.TranType) == DrCr.Debit;
			tran.AccountID = isDirect ? iRow.DebitAccountID : iRow.CreditAccountID;
			tran.SubID = isDirect ? iRow.DebitSubID : iRow.CreditSubID;
		}

		protected virtual PXGraph CreateAR(GLTranDoc doc, List<GLTranDoc> aList, List<Batch> toPost)
		{
			PXGraph result = null;
			bool multiLine = aList.Count > 1;
			ARRegister arRegister = null;
			AR.Standalone.ARInvoice arInvoice = null;
			AR.Standalone.ARPayment arPayment = null;
			Batch batch = null;
			List<ARRegister> releaseList = new List<ARRegister>(1);
			if (string.IsNullOrEmpty(doc.RefNbr) == false)
			{
				PXResultset<ARRegister> res = PXSelectJoin<ARRegister,
					LeftJoin<AR.Standalone.ARInvoice, On<AR.Standalone.ARInvoice.refNbr, Equal<ARRegister.refNbr>, And<AR.Standalone.ARInvoice.docType, Equal<ARRegister.docType>>>,
					LeftJoin<AR.Standalone.ARPayment, On<AR.Standalone.ARPayment.refNbr, Equal<ARRegister.refNbr>, And<AR.Standalone.ARPayment.docType, Equal<ARRegister.docType>>>,
					LeftJoin<Batch, On<Batch.origModule, Equal<BatchModule.moduleAR>, And<Batch.batchNbr, Equal<ARRegister.batchNbr>>>>>>,
					Where<ARRegister.docType, Equal<Required<ARRegister.docType>>,
						And<ARRegister.refNbr, Equal<Required<ARRegister.refNbr>>>>>.SelectWindowed(this, 0, 1, doc.TranType, doc.RefNbr);
				if (res.Count > 0)
				{
					arRegister = (ARRegister)res[0];
					arInvoice = (AR.Standalone.ARInvoice)(PXResult<ARRegister, AR.Standalone.ARInvoice, AR.Standalone.ARPayment, Batch>)res[0];
					arPayment = (AR.Standalone.ARPayment)(PXResult<ARRegister, AR.Standalone.ARInvoice, AR.Standalone.ARPayment, Batch>)res[0];
					batch = (Batch)(PXResult<ARRegister, AR.Standalone.ARInvoice, AR.Standalone.ARPayment, Batch>)res[0];
				}
			}
			bool forceDontPrint = this._suppressARPrintVerification;
			bool forceDontEmail = this._suppressARPrintVerification;
			if (JournalWithSubEntry.IsARInvoice(doc) && JournalWithSubEntry.IsARPayment(doc))
			{
				ARCashSaleEntry qcGraph = PXGraph.CreateInstance<ARCashSaleEntry>();
				qcGraph.arsetup.Current.HoldEntry = false;
				qcGraph.arsetup.Current.CreditCheckError = false;
				AR.PaymentRefAttribute.SetAllowAskUpdateLastRefNbr<ARCashSale.extRefNbr>(qcGraph.Document.Cache, false);
				forceDontPrint = forceDontPrint && (qcGraph.arsetup.Current.PrintBeforeRelease == true);
				forceDontEmail = forceDontEmail && (qcGraph.arsetup.Current.EmailBeforeRelease == true);
				ARCashSale copy = null, row = null;
				if (arRegister != null)
				{
					if ((arInvoice != null && !String.IsNullOrEmpty(arInvoice.RefNbr)) || (arPayment != null && !String.IsNullOrEmpty(arPayment.RefNbr)) || (batch != null && !String.IsNullOrEmpty(batch.BatchNbr)))
					{
						throw new PXException(Messages.DocumentAlreadyExists);
					}
					Copy(arRegister, doc);
					copy = (ARCashSale)qcGraph.Document.Cache.Extend<ARRegister>(arRegister);
				}
				else
				{
					row = new ARCashSale();
					row.DocType = doc.TranType;
					qcGraph.Document.Current = qcGraph.Document.Insert(row);
					copy = (ARCashSale)qcGraph.Document.Cache.CreateCopy(qcGraph.Document.Current);
				}
				PXUIFieldAttribute.SetError(qcGraph.Document.Cache, row, null, null);

				Copy(copy, doc);
				copy.Hold = true;
				if (forceDontPrint)
				{
					copy.DontPrint = true;
				}
				if (forceDontEmail)
				{
					copy.DontEmail = true;
				}
				row = qcGraph.Document.Update(copy);

				CloneCuryInfo(qcGraph.currencyinfo, doc.CuryInfoID);
				foreach (GLTranDoc iRow in aList)
				{
					if (multiLine && Object.ReferenceEquals(doc, iRow)) continue;
					ARTran tran = new ARTran();
					tran = qcGraph.Transactions.Insert(tran);
					ARTran tranCopy = (ARTran)qcGraph.Transactions.Cache.CreateCopy(tran);
					Copy(tranCopy, iRow);
					tran = qcGraph.Transactions.Update(tranCopy);
				}

				copy.CuryOrigDocAmt = doc.CuryDocTotal;
				copy.CuryOrigDiscAmt = doc.CuryDiscAmt;

				if (doc.DocType == ARDocType.CashSale)
				{
					copy.CuryOrigDocAmt -= copy.CuryOrigDiscAmt;
				}

				copy.Hold = false;
				row = qcGraph.Document.Update(copy);

				TaxCalc taxCalc = ARTaxAttribute.GetTaxCalc<ARTran.taxCategoryID>(qcGraph.Transactions.Cache, null);
				ARTaxAttribute.SetTaxCalc<ARTran.taxCategoryID>(qcGraph.Transactions.Cache, null, TaxCalc.ManualCalc);
				MergeARTaxes(qcGraph.Taxes, doc);
				ARTaxAttribute.SetTaxCalc<ARTran.taxCategoryID>(qcGraph.Transactions.Cache, null, taxCalc);
				qcGraph.Save.Press();
				doc.RefNbr = row.RefNbr;
				doc.DocCreated = true;
				releaseList.Add(row);
				result = qcGraph;
			}

			if (result == null && JournalWithSubEntry.IsARInvoice(doc))
			{
				AR.ARInvoiceEntry qcGraph = PXGraph.CreateInstance<AR.ARInvoiceEntry>();
				qcGraph.ARSetup.Current.HoldEntry = false;
				qcGraph.ARSetup.Current.CreditCheckError = false;
				forceDontPrint = forceDontPrint && (qcGraph.ARSetup.Current.PrintBeforeRelease == true);
				forceDontEmail = forceDontEmail && (qcGraph.ARSetup.Current.EmailBeforeRelease == true);
				AR.ARInvoice copy = null, row = null;
				if (arRegister != null)
				{
					if ((arInvoice != null && !String.IsNullOrEmpty(arInvoice.RefNbr)) || (arPayment != null && !String.IsNullOrEmpty(arPayment.RefNbr)) || (batch != null && !String.IsNullOrEmpty(batch.BatchNbr)))
					{
						throw new PXException(Messages.DocumentAlreadyExists);
					}
					Copy(arRegister, doc);
					//arRegister.CuryInfoID = null;
					copy = (AR.ARInvoice)qcGraph.Document.Cache.Extend<ARRegister>(arRegister);
				}
				else
				{
					row = new AR.ARInvoice();
					row.DocType = doc.TranType;
					qcGraph.Document.Current = qcGraph.Document.Insert(row);
					copy = (AR.ARInvoice)qcGraph.Document.Cache.CreateCopy(qcGraph.Document.Current);
				}

				Copy(copy, doc);
				if (forceDontPrint)
				{
					copy.DontPrint = true;
				}
				if (forceDontEmail)
				{
					copy.DontEmail = true;
				}
				row = qcGraph.Document.Update(copy);
				CloneCuryInfo(qcGraph.currencyinfo, doc.CuryInfoID);
				foreach (GLTranDoc iRow in aList)
				{
					if (multiLine && Object.ReferenceEquals(doc, iRow)) continue;
					ARTran tran = new ARTran();
					tran = qcGraph.Transactions.Insert(tran);
					ARTran tranCopy = (ARTran)qcGraph.Transactions.Cache.CreateCopy(tran);
					Copy(tranCopy, iRow);
					tran = qcGraph.Transactions.Update(tranCopy);
				}

				copy.CuryOrigDocAmt = doc.CuryDocTotal;

				if (JournalWithSubEntry.IsARCreditMemo(doc) == false)
				{
				copy.CuryOrigDiscAmt = doc.CuryDiscAmt;
				}

				copy.Hold = false;
				row = qcGraph.Document.Update(copy);

				TaxCalc taxCalc = ARTaxAttribute.GetTaxCalc<ARTran.taxCategoryID>(qcGraph.Transactions.Cache, null);
				ARTaxAttribute.SetTaxCalc<ARTran.taxCategoryID>(qcGraph.Transactions.Cache, null, TaxCalc.ManualCalc);
				MergeARTaxes(qcGraph.Taxes, doc);
				ARTaxAttribute.SetTaxCalc<ARTran.taxCategoryID>(qcGraph.Transactions.Cache, null, taxCalc);
#if ReinsertOnPersist
				try
				{
					PXCache regCache = qcGraph.Caches[typeof(ARRegister)];
					ARRegister reg = (ARRegister) regCache.Current;
					regCache.SetStatus(reg, PXEntryStatus.Deleted);
					regCache.PersistDeleted(row);
					regCache.ResetPersisted(row);
					qcGraph.Document.Cache.SetStatus(row, PXEntryStatus.Inserted);
				}
				catch (Exception e)
				{
					throw e;
				}
#endif
				qcGraph.Save.Press();
				doc.RefNbr = row.RefNbr;
				doc.DocCreated = true;
				releaseList.Add(row);
				result = qcGraph;
			}
			if (result == null && JournalWithSubEntry.IsARPayment(doc))
			{
				AR.ARPaymentEntry qcGraph = PXGraph.CreateInstance<AR.ARPaymentEntry>();
				qcGraph.arsetup.Current.HoldEntry = false;
				ARPayment copy = null, row = null;
				if (arRegister != null)
				{
					if ((arInvoice != null && !String.IsNullOrEmpty(arInvoice.RefNbr)) || (arPayment != null && !String.IsNullOrEmpty(arPayment.RefNbr)) || (batch != null && !String.IsNullOrEmpty(batch.BatchNbr)))
					{
						throw new PXException(Messages.DocumentAlreadyExists);
					}
					Copy(arRegister, doc);
					//arRegister.CuryInfoID = null;

					copy = (ARPayment)qcGraph.Document.Cache.CreateCopy(qcGraph.Document.Cache.Extend<ARRegister>(arRegister));
				}
				else
				{
					row = new AR.ARPayment();
					row.DocType = doc.TranType;
					qcGraph.Document.Current = qcGraph.Document.Insert(row);
					copy = (AR.ARPayment)qcGraph.Document.Cache.CreateCopy(qcGraph.Document.Current);
				}
				PXUIFieldAttribute.SetError(qcGraph.Document.Cache, row, null, null);
				Copy(copy, qcGraph.Document.Current, doc);
				row = qcGraph.Document.Update(copy);
				//Need to be after update - cash acctount assigment calls defaulting of the CurrencyInfo 
				CloneCuryInfo(qcGraph.currencyinfo, doc.CuryInfoID);
				qcGraph.RecalcApplAmounts(qcGraph.Document.Cache, row);
				row = qcGraph.Document.Update(row);
				qcGraph.Save.Press();
				doc.RefNbr = row.RefNbr;
				doc.DocCreated = true;
				releaseList.Add(row);
				result = qcGraph;
			}
			if (releaseList.Count > 0)
			{
				ARDocumentRelease.ReleaseDoc(releaseList, false, toPost, null);
				doc.Released = true;
			}

			return result;
		}
		protected virtual PXGraph CreateCA(GLTranDoc doc, List<GLTranDoc> aList, List<Batch> toPost)
		{
			PXGraph result = null;
			bool multiLine = aList.Count > 1;
			List<CAAdj> releaseList = new List<CAAdj>(1);
			if (doc.TranModule == GL.BatchModule.CA && doc.TranType == CA.CATranType.CAAdjustment)
			{
				CATranEntry caGraph = PXGraph.CreateInstance<CATranEntry>();
				caGraph.casetup.Current.HoldEntry = true;

				CAAdj row, copy;
				if (string.IsNullOrEmpty(doc.RefNbr) == false)
				{
					//row = (CAAdj)caGraph.CAAdjRecords.Search<CAAdj.adjTranType, CAAdj.adjRefNbr>(doc.TranType, doc.RefNbr);
					PXResultset<CAAdj> res = PXSelectJoin<CAAdj,
						LeftJoin<CASplit, On<CASplit.adjRefNbr, Equal<CAAdj.adjRefNbr>, And<CASplit.adjTranType, Equal<CAAdj.adjTranType>>>>,
						Where<CAAdj.adjTranType, Equal<Required<CAAdj.adjTranType>>,
							And<CAAdj.adjRefNbr, Equal<Required<CAAdj.adjRefNbr>>>>>.SelectWindowed(caGraph, 0, 1, doc.TranType, doc.RefNbr);
					if (res.Count > 0)
					{
						row = (CAAdj)res[0];
						CASplit split = (CASplit)(PXResult<CAAdj, CASplit>)res[0];
						if (split != null && !String.IsNullOrEmpty(split.AdjRefNbr))
						{
							throw new PXException(Messages.DocumentAlreadyExists);
						}
						caGraph.CAAdjRecords.Current = row;
					}
					//CurrencyInfo new_info = new CurrencyInfo();
					//new_info = caGraph.currencyinfo.Insert(new_info);
					//row.CuryInfoID = new_info.CuryInfoID;
				}
				else
				{
					row = new CAAdj();
					row.AdjTranType = CATranType.CAAdjustment;
					caGraph.CAAdjRecords.Current = caGraph.CAAdjRecords.Insert(row);
				}
				copy = (CAAdj)caGraph.CAAdjRecords.Cache.CreateCopy(caGraph.CAAdjRecords.Current);
				Copy(copy, caGraph.CAAdjRecords.Current, doc);
				copy.Draft = false;

				row = caGraph.CAAdjRecords.Update(copy);
				row.TaxCalcMode = TaxCalculationMode.TaxSetting;
				row = caGraph.CAAdjRecords.Update(row);
				CloneCuryInfo(caGraph.currencyinfo, doc.CuryInfoID);
				foreach (GLTranDoc iRow in aList)
				{
					if (multiLine && Object.ReferenceEquals(doc, iRow)) continue;
					CASplit tran = new CASplit();
					tran.AdjTranType = row.AdjTranType;
					tran = caGraph.CASplitRecords.Insert(tran);
					CASplit tranCopy = (CASplit)caGraph.CASplitRecords.Cache.CreateCopy(tran);
					Copy(tranCopy, iRow);
					tran = caGraph.CASplitRecords.Update(tranCopy);
				}
				TaxCalc taxCalc = TaxAttribute.GetTaxCalc<CASplit.taxCategoryID>(caGraph.CASplitRecords.Cache, null);
				TaxAttribute.SetTaxCalc<CASplit.taxCategoryID>(caGraph.CASplitRecords.Cache, null, TaxCalc.ManualCalc);
				MergeCATaxes(caGraph.Taxes, doc);
				TaxAttribute.SetTaxCalc<CASplit.taxCategoryID>(caGraph.CASplitRecords.Cache, null, taxCalc);
				row.CuryTaxAmt = row.CuryTaxTotal;
				row.Hold = false;

				foreach (CAAdj it in caGraph.hold.Press(new PXAdapter(caGraph.CurrentDocument, row))) { }
				caGraph.Save.Press();
				doc.RefNbr = row.RefNbr;
				doc.DocCreated = true;
				result = caGraph;
				releaseList.Add(row);
			}
			if (releaseList.Count > 0)
			{
				int count = 0;
				foreach (CAAdj iRes in releaseList)
				{
					CATrxRelease.ReleaseDoc<CAAdj>(iRes, count, toPost);
					count++;
				}
				doc.Released = true;
			}
			return result;
		}
		protected virtual PXGraph CreateGL(GLTranDoc doc, List<GLTranDoc> aList, List<Batch> toPost)
		{
			PXGraph result = null;
			bool multiLine = aList.Count > 1;
			List<Batch> releaseList = new List<Batch>(1);
			if (doc.TranModule == GL.BatchModule.GL)
			{
				JournalEntry graph = PXGraph.CreateInstance<JournalEntry>();
				graph.glsetup.Current.HoldEntry = false;
				graph.glsetup.Current.RequireControlTotal = false;
				Batch row, copy;
				if (string.IsNullOrEmpty(doc.RefNbr) == false)
				{
					PXResultset<Batch> res = PXSelectJoin<Batch,
					   LeftJoin<GLTran, On<GLTran.batchNbr, Equal<Batch.batchNbr>, And<GLTran.module, Equal<Batch.module>>>>,
					   Where<Batch.module, Equal<Required<Batch.module>>,
						   And<Batch.batchNbr, Equal<Required<Batch.batchNbr>>>>>.SelectWindowed(graph, 0, 1, doc.TranModule, doc.RefNbr);
					if (res.Count > 0)
					{
						row = res;
						GLTran tran = (PXResult<Batch, GLTran>)res;
						if (tran != null && !String.IsNullOrEmpty(tran.BatchNbr))
						{
							throw new PXException(Messages.DocumentAlreadyExists);
						}
						graph.BatchModule.Current = row;
					}
				}
				else
				{
					row = new Batch();
					row.Module = doc.TranModule;
					graph.BatchModule.Current = graph.BatchModule.Insert(row);
				}
				copy = (Batch)graph.BatchModule.Cache.CreateCopy(graph.BatchModule.Current);
				Copy(copy, doc);
				copy.Draft = false;
				copy.Hold = false;
				row = graph.BatchModule.Update(copy);
				CloneCuryInfo(graph.currencyinfo, doc.CuryInfoID);
				bool[] debits = { true, false };
				foreach (GLTranDoc iRow in aList)
				{
					int max = iRow.IsBalanced ? 2 : 1;
					for (int i = 0; i < max; i++)
					{
						GLTran tran = new GLTran();
						tran.Module = row.Module;
						Copy(tran, iRow, debits[i]);
						if (i > 0)
							tran.NoteID = null;
						tran = graph.GLTranModuleBatNbr.Insert(tran);
					}
				}
				row.Hold = false;
				graph.Save.Press();
				doc.RefNbr = row.BatchNbr;
				doc.DocCreated = true;
				result = graph;
				releaseList.Add(row);
			}
			if (releaseList.Count > 0)
			{
				JournalEntry.ReleaseBatch(releaseList, toPost);
				doc.Released = true;
			}
			return result;
		}
		#endregion
		#region Utility functions
		#region AP
		protected virtual void Copy(APQuickCheck aDest, GLTranDoc aSrc)
		{
			aDest.NoteID = aSrc.NoteID;
			aDest.DocDesc = aSrc.TranDesc;
			aDest.DocDate = aSrc.TranDate;
			aDest.CuryID = aSrc.CuryID;
			aDest.CuryTaxAmt = aSrc.CuryTaxAmt;
			aDest.InvoiceNbr = aSrc.ExtRefNbr;
			aDest.InvoiceDate = aSrc.TranDate;
			aDest.FinPeriodID = aSrc.FinPeriodID;
			aDest.TranPeriodID = aSrc.TranPeriodID;
			aDest.AdjDate = aSrc.TranDate;
			aDest.AdjFinPeriodID = aSrc.FinPeriodID;
			aDest.AdjTranPeriodID = aSrc.TranPeriodID;
			bool isDirect = AP.APPaymentType.DrCr(aSrc.TranType) == DrCr.Credit;
			aDest.CashAccountID = isDirect ? aSrc.CreditCashAccountID : aSrc.DebitCashAccountID;
			aDest.VendorID = aSrc.BAccountID;
			aDest.ExtRefNbr = aSrc.ExtRefNbr;
			aDest.VendorLocationID = aSrc.LocationID;
			aDest.PaymentMethodID = aSrc.PaymentMethodID;
			aDest.TaxZoneID = aSrc.TaxZoneID;
			aDest.TermsID = aSrc.TermsID;
			aDest.APAccountID = aDest.APAccountID ?? (isDirect ? aSrc.CreditAccountID : aSrc.DebitAccountID);
			aDest.APSubID = aDest.APSubID ?? (isDirect ? aSrc.CreditSubID : aSrc.DebitSubID);
			aDest.BranchID = aSrc.BranchID;
		}
		protected virtual void Copy(AP.APInvoice aDest, GLTranDoc aSrc)
		{
			DocCopyHelper.Copy(aDest, aSrc);
		}

		public static class DocCopyHelper
		{
			public static void Copy(AP.APInvoice aDest, GLTranDoc aSrc)
			{
				aDest.NoteID = aSrc.NoteID;
				aDest.DocDesc = aSrc.TranDesc;
				aDest.DocDate = aSrc.TranDate;
				aDest.InvoiceDate = aSrc.TranDate;
				aDest.CuryID = aSrc.CuryID;
				aDest.CuryTaxAmt = aSrc.CuryTaxAmt;
				aDest.InvoiceNbr = aSrc.ExtRefNbr;
				aDest.FinPeriodID = aSrc.FinPeriodID;
				aDest.TranPeriodID = aSrc.TranPeriodID;
				aDest.VendorID = aSrc.BAccountID;
				aDest.VendorLocationID = aSrc.LocationID;
				aDest.TaxZoneID = aSrc.TaxZoneID;
				aDest.TermsID = aSrc.TermsID;
				aDest.DueDate = aSrc.DueDate;
				aDest.DiscDate = aSrc.DiscDate;

				bool isDirect = AP.APInvoiceType.DrCr(aSrc.TranType) == DrCr.Debit;
				aDest.APAccountID = isDirect ? aSrc.CreditAccountID : aSrc.DebitAccountID;
				aDest.APSubID = isDirect ? aSrc.CreditSubID : aSrc.DebitSubID;
				aDest.BranchID = aSrc.BranchID;
				aDest.PayTypeID = aSrc.PaymentMethodID;
			}
		}
		protected virtual void Copy(AP.APPayment aDest, AP.APPayment aOrig, GLTranDoc aSrc)
		{
			aDest.NoteID = aSrc.NoteID;
			aDest.DocDesc = aSrc.TranDesc;
			aDest.DocDate = aSrc.TranDate;
			aDest.CuryID = aSrc.CuryID;
			aDest.ExtRefNbr = aSrc.ExtRefNbr;
			aDest.FinPeriodID = aSrc.FinPeriodID;
			aDest.TranPeriodID = aSrc.TranPeriodID;
			aDest.AdjDate = aSrc.TranDate;
			aDest.AdjFinPeriodID = aSrc.FinPeriodID;
			aDest.AdjTranPeriodID = aSrc.TranPeriodID;
			aDest.CuryOrigDocAmt = aSrc.CuryTranAmt;
			//aDest.CashAccountID = aSrc.CashAccountID;
			aDest.VendorID = aSrc.BAccountID;
			aDest.VendorLocationID = aSrc.LocationID;
			aDest.PaymentMethodID = aSrc.PaymentMethodID;
			bool isDirect = AP.APPaymentType.DrCr(aSrc.TranType) == DrCr.Credit;
			aDest.CashAccountID = isDirect ? aSrc.CreditCashAccountID : aSrc.DebitCashAccountID;
			aDest.APAccountID = isDirect ? aSrc.DebitAccountID : aSrc.CreditAccountID;
			aDest.APSubID = isDirect ? aSrc.DebitSubID : aSrc.CreditSubID;
			aDest.BranchID = aSrc.BranchID;
			aDest.Hold = false;

			aOrig.NoteID = null;
			aOrig.DocDesc = null;
			aOrig.DocDate = null;
			aOrig.CuryID = null;
			aOrig.ExtRefNbr = null;
			aOrig.FinPeriodID = null;
			aOrig.TranPeriodID = null;
			aOrig.AdjDate = null;
			aOrig.AdjFinPeriodID = null;
			aOrig.AdjTranPeriodID = null;
			aOrig.CuryOrigDocAmt = null;
			aOrig.CashAccountID = null;
			aOrig.VendorID = null;
			aOrig.VendorLocationID = null;
			aOrig.PaymentMethodID = null;
			aOrig.APAccountID = null;
			aOrig.APSubID = null;
			aOrig.BranchID = null;
			aOrig.Hold = null;
		}
		protected virtual void Copy(APRegister aDest, GLTranDoc aSrc)
		{
			aDest.NoteID = aSrc.NoteID;
			aDest.DocDesc = aSrc.TranDesc;
			aDest.DocDate = aSrc.TranDate;
			aDest.CuryID = aSrc.CuryID;
			aDest.FinPeriodID = aSrc.FinPeriodID;
			aDest.TranPeriodID = aSrc.TranPeriodID;
			aDest.CuryOrigDocAmt = aSrc.CuryTranAmt;
			aDest.VendorID = aSrc.BAccountID;
			aDest.VendorLocationID = aSrc.LocationID;
			aDest.APAccountID = null;
			aDest.APSubID = null;
			aDest.BranchID = aSrc.BranchID;
		}
		protected virtual void Copy(AP.APTran aDest, GLTranDoc aSrc)
		{
			aDest.NoteID = aSrc.NoteID;
			bool isDirect = AP.APInvoiceType.DrCr(aSrc.TranType) == DrCr.Debit;
			aDest.AccountID = isDirect ? aSrc.DebitAccountID : aSrc.CreditAccountID;
			aDest.SubID = isDirect ? aSrc.DebitSubID : aSrc.CreditSubID;
			aDest.CuryTranAmt = aSrc.CuryTranAmt;
			aDest.Qty = 1;
			aDest.CuryUnitCost = aSrc.CuryTranAmt;
			aDest.TranDesc = aSrc.TranDesc;
			aDest.TaxCategoryID = aSrc.TaxCategoryID;
			aDest.BranchID = aSrc.BranchID;
			aDest.ProjectID = aSrc.ProjectID;
			aDest.TaskID = aSrc.TaskID;
			aDest.ManualPrice = true;
		}

		#endregion
		#region AR
		protected virtual void Copy(ARCashSale aDest, GLTranDoc aSrc)
		{
			aDest.NoteID = aSrc.NoteID;
			aDest.DocDesc = aSrc.TranDesc;
			aDest.DocDate = aSrc.TranDate;
			aDest.CuryID = aSrc.CuryID;
			aDest.InvoiceNbr = aSrc.ExtRefNbr;
			aDest.InvoiceDate = aSrc.TranDate;
			aDest.FinPeriodID = aSrc.FinPeriodID;
			aDest.TranPeriodID = aSrc.TranPeriodID;
			aDest.AdjDate = aSrc.TranDate;
			aDest.AdjFinPeriodID = aSrc.FinPeriodID;
			aDest.AdjTranPeriodID = aSrc.TranPeriodID;
			bool isDirect = AR.ARPaymentType.DrCr(aSrc.TranType) == DrCr.Debit;
			aDest.CashAccountID = isDirect ? aSrc.DebitCashAccountID : aSrc.CreditCashAccountID;
			aDest.CustomerID = aSrc.BAccountID;
			aDest.ExtRefNbr = aSrc.ExtRefNbr;
			aDest.CustomerLocationID = aSrc.LocationID;
			aDest.PaymentMethodID = aSrc.PaymentMethodID;
			aDest.PMInstanceID = aSrc.PMInstanceID;
			aDest.TaxZoneID = aSrc.TaxZoneID;
			aDest.TermsID = aSrc.TermsID;
			aDest.BranchID = aSrc.BranchID;
			aDest.ProjectID = aSrc.ProjectID;
		}
		protected virtual void Copy(AR.ARInvoice aDest, GLTranDoc aSrc)
		{
			aDest.NoteID = aSrc.NoteID;
			aDest.DocDesc = aSrc.TranDesc;
			aDest.DocDate = aSrc.TranDate;
			aDest.InvoiceDate = aSrc.TranDate;
			aDest.CuryID = aSrc.CuryID;
			aDest.InvoiceNbr = aSrc.ExtRefNbr;
			aDest.FinPeriodID = aSrc.FinPeriodID;
			aDest.TranPeriodID = aSrc.TranPeriodID;
			aDest.CustomerID = aSrc.BAccountID;
			aDest.CustomerLocationID = aSrc.LocationID;
			aDest.TaxZoneID = aSrc.TaxZoneID;
			
			if (!JournalWithSubEntry.IsARCreditMemo(aSrc))
			{
			aDest.TermsID = aSrc.TermsID;
			aDest.DueDate = aSrc.DueDate;
			aDest.DiscDate = aSrc.DiscDate;
			}
			bool isDirect = AR.ARInvoiceType.DrCr(aSrc.TranType) == DrCr.Credit;
			aDest.ARAccountID = isDirect ? aSrc.DebitAccountID : aSrc.CreditAccountID;
			aDest.ARSubID = isDirect ? aSrc.DebitSubID : aSrc.CreditSubID;
			aDest.BranchID = aSrc.BranchID;
			aDest.ProjectID = aSrc.ProjectID;
			aDest.PaymentMethodID = aSrc.PaymentMethodID;
			aDest.PMInstanceID = aSrc.PMInstanceID;
			aDest.Hold = true;
		}
		protected virtual void Copy(AR.ARPayment aDest, AR.ARPayment aOrig, GLTranDoc aSrc)
		{
			aDest.NoteID = aSrc.NoteID;
			aDest.DocDesc = aSrc.TranDesc;
			aDest.DocDate = aSrc.TranDate;
			aDest.CuryID = aSrc.CuryID;
			aDest.ExtRefNbr = aSrc.ExtRefNbr;
			aDest.FinPeriodID = aSrc.FinPeriodID;
			aDest.TranPeriodID = aSrc.TranPeriodID;
			aDest.AdjDate = aSrc.TranDate;
			aDest.AdjFinPeriodID = aSrc.TranPeriodID;
			aDest.AdjTranPeriodID = aSrc.TranPeriodID;
			aDest.CuryOrigDocAmt = aSrc.CuryTranAmt;
			aDest.CustomerID = aSrc.BAccountID;
			aDest.CustomerLocationID = aSrc.LocationID;
			aDest.PaymentMethodID = aSrc.PaymentMethodID;
			aDest.PMInstanceID = aSrc.PMInstanceID;
			bool isDirect = AR.ARPaymentType.DrCr(aSrc.TranType) == DrCr.Debit;
			aDest.CashAccountID = isDirect ? aSrc.DebitCashAccountID : aSrc.CreditCashAccountID;
			aDest.ARAccountID = isDirect ? aSrc.CreditAccountID : aSrc.DebitAccountID;
			aDest.ARSubID = isDirect ? aSrc.CreditSubID : aSrc.DebitSubID;
			aDest.BranchID = aSrc.BranchID;
			aDest.ProjectID = aSrc.ProjectID;
			aDest.TaskID = aSrc.TaskID;
			aDest.Hold = false;

			aOrig.NoteID = null;
			aOrig.DocDesc = null;
			aOrig.DocDate = null;
			aOrig.CuryID = null;
			aOrig.ExtRefNbr = null;
			aOrig.FinPeriodID = null;
			aOrig.TranPeriodID = null;
			aOrig.AdjDate = null;
			aOrig.AdjFinPeriodID = null;
			aOrig.AdjTranPeriodID = null;
			aOrig.CuryOrigDocAmt = null;
			aOrig.CashAccountID = null;
			aOrig.CustomerID = null;
			aOrig.CustomerLocationID = null;
			aOrig.PaymentMethodID = null;
			aOrig.PMInstanceID = null;
			aOrig.ARAccountID = null;
			aOrig.ARSubID = null;
			aOrig.BranchID = null;
			aOrig.Hold = null;
		}
		protected virtual void Copy(ARTran aDest, GLTranDoc aSrc)
		{
			aDest.NoteID = aSrc.NoteID;
			bool isDirect = aSrc.TranType == AR.ARDocType.Invoice || aSrc.TranType == AR.ARDocType.CashSale || aSrc.TranType == AR.ARDocType.DebitMemo;
			aDest.AccountID = isDirect ? aSrc.CreditAccountID : aSrc.DebitAccountID;
			aDest.SubID = isDirect ? aSrc.CreditSubID : aSrc.DebitSubID;
			aDest.CuryExtPrice = aSrc.CuryTranAmt;
			aDest.Qty = 1;
			aDest.CuryUnitPrice = aSrc.CuryTranAmt;
			aDest.TranDesc = aSrc.TranDesc;
			aDest.TaxCategoryID = aSrc.TaxCategoryID;
			aDest.BranchID = aSrc.BranchID;
			aDest.TaskID = aSrc.TaskID;
			aDest.ManualPrice = true;
		}
		protected virtual void Copy(ARRegister aDest, GLTranDoc aSrc)
		{
			aDest.NoteID = aSrc.NoteID;
			aDest.DocDesc = aSrc.TranDesc;
			aDest.DocDate = aSrc.TranDate;
			aDest.CuryID = aSrc.CuryID;
			aDest.FinPeriodID = aSrc.FinPeriodID;
			aDest.TranPeriodID = aSrc.TranPeriodID;
			aDest.CuryOrigDocAmt = aSrc.CuryTranAmt;
			aDest.CustomerID = aSrc.BAccountID;
			aDest.CustomerLocationID = aSrc.LocationID;
			aDest.ARAccountID = null;
			aDest.ARSubID = null;
			aDest.BranchID = aSrc.BranchID;
		}
		#endregion
		#region CA
		protected virtual void Copy(CAAdj aDest, CAAdj aOrig, GLTranDoc aSrc)
		{
			aDest.NoteID = aSrc.NoteID;
			aDest.CashAccountID = aSrc.CashAccountID;
			aDest.CuryID = aSrc.CuryID;
			aDest.DrCr = aSrc.CADrCr;
			aDest.ExtRefNbr = aSrc.ExtRefNbr;
			aDest.Released = false;
			aDest.Cleared = false;
			aDest.TranDate = aSrc.TranDate;
			aDest.TranDesc = aSrc.TranDesc;
			aDest.FinPeriodID = aSrc.FinPeriodID;
			aDest.TranPeriodID = aSrc.TranPeriodID;
			aDest.EntryTypeID = aSrc.EntryTypeID;
			aDest.CuryControlAmt = aSrc.CuryDocTotal;
			aDest.TaxZoneID = aSrc.TaxZoneID;
			aDest.BranchID = aSrc.BranchID;

			aOrig.NoteID = null;
			aOrig.CashAccountID = null;
			aOrig.CuryID = null;
			aOrig.DrCr = null;
			aOrig.ExtRefNbr = null;
			aOrig.TranDate = null;
			aOrig.TranDesc = null;
			aOrig.FinPeriodID = null;
			aOrig.TranPeriodID = null;
			aOrig.EntryTypeID = null;
			aOrig.CuryControlAmt = null;
			aOrig.TaxZoneID = null;
			aOrig.BranchID = null;
		}

		protected virtual void Copy(CASplit aDest, GLTranDoc aSrc)
		{
			aDest.NoteID = aSrc.NoteID;
			bool isDisb = (aSrc.CADrCr == CA.CADrCr.CACredit);
			aDest.AccountID = isDisb ? aSrc.DebitAccountID : aSrc.CreditAccountID;
			aDest.SubID = isDisb ? aSrc.DebitSubID : aSrc.CreditSubID;
			aDest.CuryTranAmt = aSrc.CuryTranAmt;
			aDest.Qty = Decimal.One;
			aDest.CuryUnitPrice = aSrc.CuryTranAmt;
			aDest.TranDesc = aSrc.TranDesc;
			aDest.TaxCategoryID = aSrc.TaxCategoryID;
			aDest.BranchID = aSrc.BranchID;
			aDest.ProjectID = aSrc.ProjectID;
			aDest.TaskID = aSrc.TaskID;
			aDest.CostCodeID = aSrc.CostCodeID;
		}
		#endregion
		#region GL
		protected virtual void Copy(Batch aDest, GLTranDoc aSrc)
		{
			aDest.NoteID = aSrc.NoteID;
			aDest.CuryID = aSrc.CuryID;
			aDest.Released = false;
			aDest.DateEntered = aSrc.TranDate;
			aDest.Description = aSrc.TranDesc;
			aDest.FinPeriodID = aSrc.FinPeriodID;
			aDest.TranPeriodID = aSrc.TranPeriodID;
			aDest.BranchID = aSrc.BranchID;
		}

		protected virtual void Copy(GLTran aDest, GLTranDoc aSrc, bool debit)
		{
			aDest.NoteID = aSrc.NoteID;
			bool isMixed = aSrc.CreditAccountID.HasValue && aSrc.DebitAccountID.HasValue;
			bool isDebit = (isMixed == false) ? aSrc.DebitAccountID.HasValue : debit;
			aDest.AccountID = isDebit ? aSrc.DebitAccountID : aSrc.CreditAccountID;
			aDest.SubID = isDebit ? aSrc.DebitSubID : aSrc.CreditSubID;
			aDest.CuryDebitAmt = isDebit ? aSrc.CuryTranAmt : Decimal.Zero;
			aDest.DebitAmt = isDebit ? aSrc.TranAmt : Decimal.Zero;
			aDest.CuryCreditAmt = !isDebit ? aSrc.CuryTranAmt : Decimal.Zero;
			aDest.CreditAmt = !isDebit ? aSrc.TranAmt : Decimal.Zero;
			aDest.FinPeriodID = aSrc.FinPeriodID;
			aDest.TranPeriodID = aSrc.TranPeriodID;
			aDest.TranDate = aSrc.TranDate;
			aDest.BranchID = aSrc.BranchID;
			aDest.TranDesc = aSrc.TranDesc;
			aDest.ProjectID = aSrc.ProjectID;
			aDest.TaskID = aSrc.TaskID;
			aDest.CostCodeID = aSrc.CostCodeID;
			aDest.RefNbr = aSrc.ExtRefNbr;
		}
		#endregion
		protected virtual void Copy(PXCache cache, TaxDetail aDest, GLTaxTran aSrc)
		{
			cache.SetValue(aDest, typeof(TaxAttribute.curyTaxableAmt).Name, aSrc.CuryTaxableAmt);
			cache.SetValue(aDest, typeof(TaxAttribute.curyTaxAmt).Name, aSrc.CuryTaxAmt);
			aDest.TaxRate = aSrc.TaxRate;
		}
		protected virtual void Copy(GLTranDocU aDest, GLTranDoc aSrc)
		{
			aDest.BranchID = aSrc.BranchID;
			aDest.Module = aSrc.Module;
			aDest.BatchNbr = aSrc.BatchNbr;
			aDest.LineNbr = aSrc.LineNbr;
			aDest.RefNbr = aSrc.RefNbr;
		}
		#endregion
		#region Functions
		protected void CloneCuryInfo(PXSelectBase<CurrencyInfo> aCuryInfoView, long? aCuryInfoID)
		{
			if (aCuryInfoID != null)
			{
				CurrencyInfo tranCuryInfo = PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Required<CurrencyInfo.curyInfoID>>>>.Select(this, aCuryInfoID);
				foreach (CurrencyInfo info in aCuryInfoView.Select())
				{
					CurrencyInfo new_info = PXCache<CurrencyInfo>.CreateCopy(info);
					aCuryInfoView.Cache.RestoreCopy(new_info, tranCuryInfo);
					new_info.CuryInfoID = info.CuryInfoID;
					new_info.tstamp = info.tstamp;
					aCuryInfoView.Cache.Update(new_info);
				}
			}
		}

		protected void MergeAPTaxes(PXSelectBase<AP.APTaxTran> aDestTaxView, GLTranDoc doc)
		{
			MergeTaxes<AP.APTaxTran, AP.APTaxTran.taxID>(aDestTaxView, doc);
		}

		protected void MergeARTaxes(PXSelectBase<AR.ARTaxTran> aDestTaxView, GLTranDoc doc)
		{
			MergeTaxes<AR.ARTaxTran, AR.ARTaxTran.taxID>(aDestTaxView, doc);
		}

		protected void MergeCATaxes(PXSelectBase<CA.CATaxTran> aDestTaxView, GLTranDoc doc)
		{
			MergeTaxes<CA.CATaxTran, CA.CATaxTran.taxID>(aDestTaxView, doc);
		}

		protected void MergeTaxes<T, TTaxIDField>(PXSelectBase<T> aDestTaxView, GLTranDoc doc)
			where T : TX.TaxDetail, IBqlTable, new()
			where TTaxIDField : IBqlField
		{
			foreach (GLTaxTran iSrcTax in this.taxTotals.Select(doc.Module, doc.BatchNbr, doc.LineNbr))
			{
				T iDestTax = (T)aDestTaxView.Search<TTaxIDField>(iSrcTax.TaxID);
				if (iDestTax != null && iDestTax.TaxID == iSrcTax.TaxID)
				{
					T taxCopy = (T)aDestTaxView.Cache.CreateCopy(iDestTax);
					Copy(aDestTaxView.Cache, (T)taxCopy, iSrcTax);
					iDestTax = (T)aDestTaxView.Cache.Update(taxCopy);
				}
			}
			foreach (T iTax in aDestTaxView.Select())
			{
				GLTaxTran srcTax = this.taxTotals.Search<GLTaxTran.taxID>(iTax.TaxID, doc.Module, doc.BatchNbr, doc.LineNbr);
				if (srcTax == null || String.IsNullOrEmpty(srcTax.TaxID))
				{
					aDestTaxView.Cache.Delete(iTax);
				}
			}
		}
		#endregion
	}
}
