using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.AP;
using PX.Objects.AR;
using PX.Objects.CS;
using PX.Objects.EP;
using PX.Objects.GL;
using PX.Objects.PM;
using PX.Objects.PM.GraphExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.PR
{
	public class PRxJournalEntryTranRef : PXGraphExtension<JournalEntryTranRef>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.payrollModule>();
		}

		public delegate string GetDocTypeDelegate(APTran apTran, ARTran arTran, GLTran glTran);
		[PXOverride]
		public virtual string GetDocType(APTran apTran, ARTran arTran, GLTran glTran, GetDocTypeDelegate baseMethod)
		{
			if (glTran.Module == BatchModule.PR)
			{
				switch (glTran.TranType)
				{
					case PayrollType.Regular:
						return PMOrigDocType.RegularPaycheck;
					case PayrollType.Special:
						return PMOrigDocType.SpecialPaycheck;
					case PayrollType.Adjustment:
						return PMOrigDocType.AdjustmentPaycheck;
					case PayrollType.VoidCheck:
						return PMOrigDocType.VoidPaycheck;
					default: return null;
				}
			}

			return baseMethod(apTran, arTran, glTran);
		}

		public delegate Guid? GetNoteIDDelegate(APTran apTran, ARTran arTran, GLTran glTran);
		[PXOverride]
		public virtual Guid? GetNoteID(APTran apTran, ARTran arTran, GLTran glTran, GetNoteIDDelegate baseMethod)
		{
			if (glTran.Module == BatchModule.PR)
			{
				return new SelectFrom<PRPayment>
					.Where<PRPayment.docType.IsEqual<P.AsString>
					.And<PRPayment.refNbr.IsEqual<P.AsString>>>.View(Base).SelectSingle(glTran.TranType, glTran.RefNbr).NoteID;
			}

			return baseMethod(apTran, arTran, glTran);
		}

		public delegate void AssignCustomerVendorEmployeeDelegate(GLTran glTran, PMTran pmTran);
		[PXOverride]
		public virtual void AssignCustomerVendorEmployee(GLTran glTran, PMTran pmTran, AssignCustomerVendorEmployeeDelegate baseMethod)
		{
			if (glTran.Module == BatchModule.PR)
			{
				pmTran.ResourceID = glTran.ReferenceID;
			}
			else
			{
				baseMethod(glTran, pmTran);
			}
		}

		public delegate void AssignAdditionalFieldsDelegate(GLTran glTran, PMTran pmTran);
		[PXOverride]
		public virtual void AssignAdditionalFields(GLTran glTran, PMTran pmTran, AssignAdditionalFieldsDelegate baseMethod)
		{
			baseMethod(glTran, pmTran);

			if (glTran.Module == BatchModule.PR)
			{
				PRxGLTran glTranExt = PXCache<GLTran>.GetExtension<PRxGLTran>(glTran);
				pmTran.LocationID = glTranExt.PayrollWorkLocationID;
				pmTran.Qty = glTran.Qty;
				pmTran.TranCuryUnitRate = glTran.Qty != null && glTran.Qty != 0 ? (glTran.DebitAmt - glTran.CreditAmt) / glTran.Qty : null;
				if (!string.IsNullOrEmpty(glTranExt.EarningTypeCD))
				{
					pmTran.EarningType = glTranExt.EarningTypeCD;
					EPEarningType earningType = new SelectFrom<EPEarningType>.Where<EPEarningType.typeCD.IsEqual<P.AsString>>.View(Base).SelectSingle(glTranExt.EarningTypeCD);
					pmTran.OvertimeMultiplier = earningType?.OvertimeMultiplier;
				}
			}
		}
	}
}
