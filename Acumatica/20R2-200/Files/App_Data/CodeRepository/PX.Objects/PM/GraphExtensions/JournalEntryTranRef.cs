using PX.Data;
using PX.Objects.AP;
using PX.Objects.AR;
using PX.Objects.GL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.PM.GraphExtensions
{
	public class JournalEntryTranRef : PXGraph<JournalEntryTranRef>
	{
		public virtual string GetDocType(APTran apTran, ARTran arTran, GLTran glTran)
		{
			if (apTran != null)
			{
				switch (apTran.TranType)
				{
					case APDocType.Invoice:
						return PMOrigDocType.APBill;
					case APDocType.CreditAdj:
						return PMOrigDocType.CreditAdjustment;
					case APDocType.DebitAdj:
						return PMOrigDocType.DebitAdjustment;
					default: return null;
				}
			}
			else if (arTran != null)
			{
				switch (arTran.TranType)
				{
					case ARDocType.Invoice:
						return PMOrigDocType.ARInvoice;
					case ARDocType.CreditMemo:
						return PMOrigDocType.CreditMemo;
					case ARDocType.DebitMemo:
						return PMOrigDocType.DebitMemo;
					default: return null;
				}
			}
			else
				return null;
		}

		public virtual Guid? GetNoteID(APTran apTran, ARTran arTran, GLTran glTran)
		{
			if (apTran != null)
			{
				APInvoice invoice = PXSelect<APInvoice,
					Where<APRegister.docType, Equal<Required<APTran.tranType>>,
						And<APRegister.refNbr, Equal<Required<APTran.refNbr>>>>>.Select(this, apTran.TranType, apTran.RefNbr);
				return invoice.NoteID;
			}
			else if (arTran != null)
			{
				ARInvoice invoice = PXSelect<ARInvoice,
					Where<ARRegister.docType, Equal<Required<ARTran.tranType>>,
						And<ARRegister.refNbr, Equal<Required<ARTran.refNbr>>>>>.Select(this, arTran.TranType, arTran.RefNbr);
				return invoice.NoteID;
			}
			else
				return null;
		}

		public virtual void AssignCustomerVendorEmployee(GLTran glTran, PMTran pmTran)
		{
			pmTran.BAccountID = glTran.ReferenceID;
		}
		
		public virtual void AssignAdditionalFields(GLTran glTran, PMTran pmTran) { }
	}
}
