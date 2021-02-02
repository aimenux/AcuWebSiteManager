using System;
using PX.Data;
using PX.Objects.CA.BankStatementProtoHelpers;
using PX.Objects.Common;

namespace PX.Objects.CA
{
	[Serializable]
    [PXCacheName(Messages.BankTranMatch)]
	public partial class CABankTranMatch : IBqlTable
	{
		#region TranID
		public abstract class tranID : PX.Data.BQL.BqlInt.Field<tranID> { }

		[PXDBInt(IsKey = true)]
		public virtual int? TranID
		{
			get;
			set;
		}
		#endregion
		#region LineNbr
		public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }
		[PXDBInt(IsKey = true)]
		[PXUIField(DisplayName = "Line Nbr.", Visibility = PXUIVisibility.Visible, Visible = false)]
		[PXLineNbr(typeof(CABankTran.lineCntrMatch))]
		[PXParent(typeof(Select<CABankTran, Where<CABankTran.tranID, Equal<Current<CABankTranMatch.tranID>>>>))]
		public virtual int? LineNbr
		{
			get;
			set;
		}
		#endregion
		#region TranType
		public abstract class tranType : PX.Data.BQL.BqlString.Field<tranType> { }

		[PXDBString(1, IsFixed = true)]
		[PXDefault]
		[CABankTranType.List]
		public virtual string TranType
		{
			get;
			set;
		}
		#endregion
		#region CATranID
		public abstract class cATranID : PX.Data.BQL.BqlLong.Field<cATranID> { }

		[PXDBLong]
		public virtual long? CATranID
		{
			get;
			set;
		}
		#endregion
		#region DocModule
		public abstract class docModule : PX.Data.BQL.BqlString.Field<docModule> { }

		[PXDBString(2, IsFixed = true)]
		[PXStringList(new string[] { GL.BatchModule.AP, GL.BatchModule.AR }, new string[] { GL.Messages.ModuleAP, GL.Messages.ModuleAR })]
		public virtual string DocModule
		{
			get;
			set;
		}
		#endregion
		#region DocType
		public abstract class docType : PX.Data.BQL.BqlString.Field<docType> { }

		[PXDBString(3, IsFixed = true, InputMask = "")]
		public virtual string DocType
		{
			get;
			set;
		}
		#endregion
		#region DocRefNbr
		public abstract class docRefNbr : PX.Data.BQL.BqlString.Field<docRefNbr> { }

		[PXDBString(15, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
		public virtual string DocRefNbr
		{
			get;
			set;
		}
		#endregion
		#region ReferenceID
		public abstract class referenceID : PX.Data.BQL.BqlInt.Field<referenceID> { }

		[PXDBInt]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual int? ReferenceID
		{
			get;
			set;
		}
		#endregion
		#region CuryAmt
		public abstract class curyAmt : PX.Data.BQL.BqlDecimal.Field<curyAmt> { }

		[PXDBDecimal]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual decimal? CuryAmt
		{
			get;
			set;
		}
		#endregion
		#region CuryApplAmt
		public abstract class curyApplAmt : PX.Data.BQL.BqlDecimal.Field<curyApplAmt> { }

		[PXDBDecimal]
		[PXFormula(typeof(CABankTranMatch.curyApplAmt), typeof(SumCalc<CABankTran.curyApplAmtMatch>))]
		public virtual decimal? CuryApplAmt
		{
			get;
			set;
		}
		#endregion
		#region tstamp
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }

		[PXDBTimestamp]
		public virtual byte[] tstamp
		{
			get;
			set;
		}
		#endregion
		public static void Redirect(PXGraph graph, CABankTranMatch match)
		{
			if (match.DocModule == GL.BatchModule.AP && match.DocType == CATranType.CABatch && match.DocRefNbr != null)
			{
				CABatchEntry docGraph = PXGraph.CreateInstance<CABatchEntry>();
				docGraph.Clear();
				docGraph.Document.Current = PXSelect<CABatch, Where<CABatch.batchNbr, Equal<Required<CATran.origRefNbr>>>>.Select(docGraph, match.DocRefNbr);
				throw new PXRedirectRequiredException(docGraph, true, "Document") { Mode = PXBaseRedirectException.WindowMode.NewWindow };
			}
			else if (match.CATranID != null)
			{
				CATran catran = PXSelect<CATran, Where<CATran.tranID, Equal<Required<CABankTranMatch.cATranID>>>>.Select(graph, match.CATranID);
				CATran.Redirect(null, catran);
			}
			else if (match.DocModule != null && match.DocType != null && match.DocRefNbr != null)
			{
				RedirectionToOrigDoc.TryRedirect(match.DocType, match.DocRefNbr, match.DocModule);
			}
		}

		public void Copy(CABankTranDocRef docRef)
		{
			CATranID = docRef.CATranID;
			DocModule = docRef.DocModule;
			DocType = docRef.DocType;
			DocRefNbr = docRef.DocRefNbr;
			ReferenceID = docRef.ReferenceID;

			bool cashDiscIsApplicable = docRef.CuryDiscAmt != null
												&& docRef.TranDate != null
												&& docRef.DiscDate != null
												&& (DateTime)docRef.TranDate <= (DateTime)docRef.DiscDate;
			CuryApplAmt = docRef.CuryTranAmt - (cashDiscIsApplicable ? docRef.CuryDiscAmt : 0);
		}
	}

	public partial class CABankTranMatch2 : CABankTranMatch
	{
		#region TranID
		public new abstract class tranID : PX.Data.BQL.BqlInt.Field<tranID> { }
		#endregion
		#region TranType
		public new abstract class tranType : PX.Data.BQL.BqlString.Field<tranType> { }
		#endregion
		#region CATranID
		public new abstract class cATranID : PX.Data.BQL.BqlLong.Field<cATranID> { }
		#endregion
		#region DocModule
		public new abstract class docModule : PX.Data.BQL.BqlString.Field<docModule> { }
		#endregion
		#region DocType
		public new abstract class docType : PX.Data.BQL.BqlString.Field<docType> { }
		#endregion
		#region DocRefNbr
		public new abstract class docRefNbr : PX.Data.BQL.BqlString.Field<docRefNbr> { }
		#endregion
	}
}
