using System;
using System.Collections.Generic;
using System.Text;
using PX.Data;
using System.Collections;
using System.Linq;
using PX.Objects.AP;
using PX.Objects.AR;
using PX.Objects.CA;
using PX.Objects.CM;
using PX.Objects.GL;
using PX.TaxProvider;

namespace PX.Objects.TX
{
    [Serializable]
	public class ExternalTaxPost : PXGraph<ExternalTaxPost>
	{
		[PXFilterable]
		public PXProcessing<Document> Items;
		
		public IEnumerable items()
		{
			bool found = false;
			foreach (Document item in Items.Cache.Inserted)
			{
				found = true;
				yield return item;
			}
			if (found)
				yield break;

			PXSelectBase<ARInvoice> selectAR = new PXSelectJoin<ARInvoice,
				InnerJoin<TaxZone, On<TaxZone.taxZoneID, Equal<ARInvoice.taxZoneID>>>,
				Where<TaxZone.isExternal, Equal<True>,
				And<ARInvoice.isTaxValid, Equal<True>,
				And<ARInvoice.released, Equal<True>,
				And<ARInvoice.isTaxPosted, Equal<False>,
				And<ARInvoice.nonTaxable, Equal<False>>>>>>>(this);

			PXSelectBase<APInvoice> selectAP = new PXSelectJoin<APInvoice, 
				InnerJoin<TaxZone, On<TaxZone.taxZoneID, Equal<APInvoice.taxZoneID>>>,
				Where<TaxZone.isExternal, Equal<True>,
				And<APInvoice.isTaxValid, Equal<True>,
				And<APInvoice.released, Equal<True>,
				And<APInvoice.isTaxPosted, Equal<False>,
				And<APInvoice.nonTaxable, Equal<False>>>>>>>(this);

			PXSelectBase<CAAdj> selectCA = new PXSelectJoin<CAAdj,
				InnerJoin<TaxZone, On<TaxZone.taxZoneID, Equal<CAAdj.taxZoneID>>>,
				Where<TaxZone.isExternal, Equal<True>,
				And<CAAdj.isTaxValid, Equal<True>,
				And<CAAdj.released, Equal<True>,
				And<CAAdj.isTaxPosted, Equal<False>,
				And<CAAdj.nonTaxable, Equal<False>>>>>>>(this);


			foreach ( ARInvoice item in selectAR.Select())
			{
				Document doc = new Document();
				doc.Module = GL.BatchModule.AR;
			    doc.TaxZoneID = item.TaxZoneID;
                doc.DocType = item.DocType;
				doc.RefNbr = item.RefNbr;
				doc.BranchID = item.BranchID;
				doc.DocDate = item.DocDate;
				doc.FinPeriodID = item.FinPeriodID;
				doc.Amount = item.CuryDocBal;
				doc.CuryID = item.CuryID;
				doc.DocDesc = item.DocDesc;
				doc.DrCr = item.DrCr;

				yield return Items.Insert(doc);
			}

			foreach (APInvoice item in selectAP.Select())
			{
				Document doc = new Document();
				doc.Module = GL.BatchModule.AP;
				doc.DocType = item.DocType;
			    doc.TaxZoneID = item.TaxZoneID;
                doc.RefNbr = item.RefNbr;
				doc.BranchID = item.BranchID;
				doc.DocDate = item.DocDate;
				doc.FinPeriodID = item.FinPeriodID;
				doc.Amount = item.CuryDocBal;
				doc.CuryID = item.CuryID;
				doc.DocDesc = item.DocDesc;
				doc.DrCr = item.DrCr;

				yield return Items.Insert(doc);
			}

			foreach (CAAdj item in selectCA.Select())
			{
				Document doc = new Document();
				doc.Module = GL.BatchModule.CA;
				doc.DocType = item.DocType;
			    doc.TaxZoneID = item.TaxZoneID;
                doc.RefNbr = item.RefNbr;
				doc.BranchID = item.BranchID;
				doc.DocDate = item.TranDate;
				doc.FinPeriodID = item.FinPeriodID;
				doc.Amount = item.CuryTranAmt;
				doc.CuryID = item.CuryID;
				doc.DocDesc = item.TranDesc;
				doc.DrCr = item.DrCr;

				yield return Items.Insert(doc);
			}

			Items.Cache.IsDirty = false;
		}

		public ExternalTaxPost()
		{
			Items.SetSelected<Document.selected>();
			Items.SetProcessDelegate(Release);
			Items.SetProcessCaption(Messages.ProcessCaptionPost);
			Items.SetProcessAllCaption(Messages.ProcessCaptionPostAll);
		}

		public PXAction<Document> viewDocument;
		[PXUIField(Visible = false, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXLookupButton()]
		public virtual IEnumerable ViewDocument(PXAdapter adapter)
		{
			Document doc = Items.Current;
			if (doc != null && !String.IsNullOrEmpty(doc.Module) && !String.IsNullOrEmpty(doc.DocType) && !String.IsNullOrEmpty(doc.RefNbr))
			{
				switch (doc.Module)
				{
					case GL.BatchModule.AR:
						ARInvoiceEntry argraph = PXGraph.CreateInstance<ARInvoiceEntry>();
						argraph.Document.Current = argraph.Document.Search<ARInvoice.refNbr>(doc.RefNbr, doc.DocType);
						if (argraph.Document.Current != null)
						{
							throw new PXRedirectRequiredException(argraph, true, "View Document") { Mode = PXBaseRedirectException.WindowMode.NewWindow };
						}						
					break;

					case GL.BatchModule.AP:	
						APInvoiceEntry apgraph = PXGraph.CreateInstance<APInvoiceEntry>();
						apgraph.Document.Current = apgraph.Document.Search<APInvoice.refNbr>(doc.RefNbr, doc.DocType);
						if (apgraph.Document.Current != null)
						{
							throw new PXRedirectRequiredException(apgraph, true, "View Document") { Mode = PXBaseRedirectException.WindowMode.NewWindow };
						}		
					break;

					case GL.BatchModule.CA:
						CATranEntry cagraph = PXGraph.CreateInstance<CATranEntry>();
						cagraph.CAAdjRecords.Current = cagraph.CAAdjRecords.Search<CAAdj.adjRefNbr>(doc.RefNbr, doc.DocType);
						if (cagraph.CAAdjRecords.Current != null)
						{
							throw new PXRedirectRequiredException(cagraph, true, "View Document") { Mode = PXBaseRedirectException.WindowMode.NewWindow };
						}
					break;
				}
			}

			return adapter.Get();
		}

		public static void Release(List<Document> list)
		{
			ExternalTaxPostProcess rg = PXGraph.CreateInstance<ExternalTaxPostProcess>();
			for (int i = 0; i < list.Count; i++)
			{
				Document doc = list[i];

				try
				{
					rg.Clear();
					rg.Post(doc);
					PXProcessing<Document>.SetInfo(i, ActionsMessages.RecordProcessed);
				}
				catch (Exception e)
				{
					PXProcessing<Document>.SetError(i, e is PXOuterException ? e.Message + "\r\n" + String.Join("\r\n", ((PXOuterException)e).InnerMessages) : e.Message);
				}

			}
		}

		protected virtual void Document_RowPersisting(PXCache sedner, PXRowPersistingEventArgs e)
		{
			e.Cancel = true;
		}

        [Serializable]
		public class Document : IBqlTable
		{
			#region Module
			public abstract class module : PX.Data.BQL.BqlString.Field<module> { }
			protected String _Module;
			[PXString(2, IsFixed = true, IsKey = true)]
			[PXUIField(DisplayName = "Module")]
			[GL.BatchModule.FullList()]
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
			#region DocType
			public abstract class docType : PX.Data.BQL.BqlString.Field<docType> { }
			protected String _DocType;
			[PXString(3, IsKey = true, IsFixed = true)]
			[PXUIField(DisplayName = "Doc. Type")]
			public virtual String DocType
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
		    #region TaxZoneID
		    public abstract class taxZoneID : PX.Data.BQL.BqlString.Field<taxZoneID> { }
		    protected String _TaxZoneID;

		    [PXString(10, IsUnicode = true)]
		    [PXUIField(DisplayName = "Customer Tax Zone", Visibility = PXUIVisibility.Visible)]
		    public virtual String TaxZoneID
		    {
		        get
		        {
		            return this._TaxZoneID;
		        }
		        set
		        {
		            this._TaxZoneID = value;
		        }
		    }
            #endregion
            #region RefNbr
            public abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }
			protected String _RefNbr;
			[PXString(15, IsUnicode = true, IsKey = true, InputMask = "")]
			[PXUIField(DisplayName = "Reference Nbr.")]
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

			#region Selected
			public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }
			protected bool? _Selected = false;
			[PXBool]
			[PXDefault(false)]
			[PXUIField(DisplayName = "Selected")]
			public virtual bool? Selected
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
			#region BranchID
			public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
			protected Int32? _BranchID;
			[Branch()]
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
			#region DocDate
			public abstract class docDate : PX.Data.BQL.BqlDateTime.Field<docDate> { }
			protected DateTime? _DocDate;
			[PXDate()]
			[PXUIField(DisplayName = "Date")]
			public virtual DateTime? DocDate
			{
				get
				{
					return this._DocDate;
				}
				set
				{
					this._DocDate = value;
				}
			}
			#endregion
			#region FinPeriodID
			public abstract class finPeriodID : PX.Data.BQL.BqlString.Field<finPeriodID> { }
			protected String _FinPeriodID;
			[PeriodID]
			[PXUIField(DisplayName = "Fin. Period")]
			public virtual String FinPeriodID
			{
				get
				{
					return this._FinPeriodID;
				}
				set
				{
					this._FinPeriodID = value;
				}
			}
			#endregion
			#region Amount
			public abstract class amount : PX.Data.BQL.BqlDecimal.Field<amount> { }
			protected Decimal? _Amount;
			[PXBaseCury]
			[PXUIField(DisplayName = "Amount", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
			public virtual Decimal? Amount
			{
				get
				{
					return this._Amount;
				}
				set
				{
					this._Amount = value;
				}
			}
			#endregion
			#region CuryID
			public abstract class curyID : PX.Data.BQL.BqlString.Field<curyID> { }
			protected String _CuryID;
			[PXString(5, IsUnicode = true, InputMask = ">LLLLL")]
			[PXUIField(DisplayName = "Currency")]
			public virtual String CuryID
			{
				get
				{
					return this._CuryID;
				}
				set
				{
					this._CuryID = value;
				}
			}
			#endregion
			#region DocDesc
			public abstract class docDesc : PX.Data.BQL.BqlString.Field<docDesc> { }
			protected String _DocDesc;
			[PXString(Common.Constants.TranDescLength, IsUnicode = true)]
			[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
			public virtual String DocDesc
			{
				get
				{
					return this._DocDesc;
				}
				set
				{
					this._DocDesc = value;
				}
			}
			#endregion
			#region DrCr
			public abstract class drCr : PX.Data.BQL.BqlString.Field<drCr> { }
			protected String _DrCr;
			[PXDBString(1, IsFixed = true)]
			public virtual String DrCr
			{
				get
				{
					return this._DrCr;
				}
				set
				{
					this._DrCr = value;
				}
			}
			#endregion
		}

		public class ExternalTaxPostProcess : PXGraph<ExternalTaxPostProcess>
		{
			protected readonly Func<PXGraph, string, ITaxProvider> TaxProviderFactory;


            public ExternalTaxPostProcess()
            {
                TaxProviderFactory = ExternalTax.TaxProviderFactory;
			}

			public void Post(Document doc)
			{
				if (!TaxPluginMaint.IsActive(this, doc.TaxZoneID))
					throw new PXException(Messages.ExternalTaxProviderNotConfigured);

				var service = TaxProviderFactory(this, doc.TaxZoneID);

				var request = new CommitTaxRequest();
				request.CompanyCode = ExternalTax.CompanyCodeFromBranch(this, doc.TaxZoneID, doc.BranchID);
				request.DocCode = string.Format("{0}.{1}.{2}", doc.Module, doc.DocType, doc.RefNbr);

				request.DocType = GetTaxDocumentType(doc);
				
				CommitTaxResult result = service.CommitTax(request);
				bool setPosted = false;

				if (result.IsSuccess)
				{
					setPosted = true;
				}
				else
				{
					//Avalara retuned an error - The given document is already marked as posted on the avalara side.
					//Just fix the discrepency by setting the IsTaxPosted=1 in the acumatica document. Do not return this as an error to the user.
					if (!result.IsSuccess && result.Messages.Any(t=>t.Contains("Expected Posted")))
						setPosted = true;
				}

				if (setPosted)
				{

					if (doc.Module == BatchModule.AP)
					{
						PXDatabase.Update<AP.APRegister>(
						new PXDataFieldAssign("IsTaxPosted", true),
						new PXDataFieldRestrict("DocType", PXDbType.Char, 3, doc.DocType, PXComp.EQ),
						new PXDataFieldRestrict("RefNbr", PXDbType.NVarChar, 15, doc.RefNbr, PXComp.EQ)
						);
					}
					else if (doc.Module == BatchModule.AR)
					{
						PXDatabase.Update<AR.ARRegister>(
						new PXDataFieldAssign("IsTaxPosted", true),
						new PXDataFieldRestrict("DocType", PXDbType.Char, 3, doc.DocType, PXComp.EQ),
						new PXDataFieldRestrict("RefNbr", PXDbType.NVarChar, 15, doc.RefNbr, PXComp.EQ)
						);
					}
					else if (doc.Module == BatchModule.CA)
					{
						PXDatabase.Update<CA.CAAdj>(
						new PXDataFieldAssign("IsTaxPosted", true),
						new PXDataFieldRestrict("AdjRefNbr", PXDbType.NVarChar, 15, doc.RefNbr, PXComp.EQ)
						);
					}
				}
				else
				{
					StringBuilder sb = new StringBuilder();
					foreach (var msg in result.Messages)
					{
						sb.AppendLine(msg);
					}

					throw new PXException(sb.ToString());
				}

			}

			public virtual TaxDocumentType GetTaxDocumentType(Document doc)
			{
				switch (doc.Module)
				{
					case BatchModule.AP:
						return doc.DrCr == DrCr.Credit
							? TaxDocumentType.ReturnInvoice
							: TaxDocumentType.PurchaseInvoice;

					case BatchModule.AR:
						return doc.DrCr == DrCr.Debit
							? TaxDocumentType.ReturnInvoice
							: TaxDocumentType.SalesInvoice;

					case BatchModule.CA:
						return (doc.DrCr == CA.CADrCr.CADebit)
							? TaxDocumentType.SalesInvoice
							: TaxDocumentType.PurchaseInvoice;

					default:
						throw new PXException(PXMessages.LocalizeFormatNoPrefixNLA(Messages.InvalidModule, doc.Module));
				}
			}
		}
	}
}
