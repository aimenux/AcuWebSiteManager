using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Data;
using PX.Objects.GL;
using PX.Reports.Parser;
using System.Diagnostics;
using PX.Objects.AR;
using PX.Objects.CR;
using PX.Objects.EP;
using PX.Objects.AP;
using PX.Objects.IN;
using PX.Objects.CS;

namespace PX.Objects.PM
{
	public class ChangeOrderClassMaint : PXGraph<ChangeOrderClassMaint, PMChangeOrderClass>
	{	
		public PXSelect<PMChangeOrderClass> Item;
		public PXSelect<PMChangeOrderClass, Where<PMChangeOrderClass.classID, Equal<Current<PMChangeOrderClass.classID>>>> ItemSettings;

		[PXViewName(CR.Messages.Attributes)]
		public CSAttributeGroupList<PMChangeOrderClass, PMChangeOrder> Mapping;

		protected virtual void _(Events.FieldVerifying<PMChangeOrderClass, PMChangeOrderClass.isCostBudgetEnabled> e)
		{
			var select = new PXSelectJoin<PMChangeOrderBudget, 
				InnerJoin<PMChangeOrder, On<PMChangeOrderBudget.refNbr, Equal<PMChangeOrder.refNbr>>>,
				Where<PMChangeOrderBudget.type, Equal<GL.AccountType.expense>,
				And<PMChangeOrder.classID, Equal<Current<PMChangeOrderClass.classID>>>>>(this);

			PMChangeOrderBudget res = select.SelectWindowed(0, 1);
			if (res != null)
			{
				throw new PXSetPropertyException<PMChangeOrderClass.isCostBudgetEnabled>(Messages.ClassContainsCostBudget);
			}
		}

		protected virtual void _(Events.FieldVerifying<PMChangeOrderClass, PMChangeOrderClass.isRevenueBudgetEnabled> e)
		{
			var select = new PXSelectJoin<PMChangeOrderBudget,
				InnerJoin<PMChangeOrder, On<PMChangeOrderBudget.refNbr, Equal<PMChangeOrder.refNbr>>>,
				Where<PMChangeOrderBudget.type, Equal<GL.AccountType.income>,
				And<PMChangeOrder.classID, Equal<Current<PMChangeOrderClass.classID>>>>>(this);

			PMChangeOrderBudget res = select.SelectWindowed(0, 1);
			if (res != null)
			{
				throw new PXSetPropertyException<PMChangeOrderClass.isRevenueBudgetEnabled>(Messages.ClassContainsRevenueBudget);
			}
		}

		protected virtual void _(Events.FieldVerifying<PMChangeOrderClass, PMChangeOrderClass.isPurchaseOrderEnabled> e)
		{
			var select = new PXSelectJoin<PMChangeOrderLine,
				InnerJoin<PMChangeOrder, On<PMChangeOrderLine.refNbr, Equal<PMChangeOrder.refNbr>>>,
				Where<PMChangeOrder.classID, Equal<Current<PMChangeOrderClass.classID>>>>(this);

			PMChangeOrderLine res = select.SelectWindowed(0, 1);
			if (res != null)
			{
				throw new PXSetPropertyException<PMChangeOrderClass.isPurchaseOrderEnabled>(Messages.ClassContainsDetails);
			}
		}
	}

}
