using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;
using PX.Objects.CN.JointChecks.AP.DAC;

namespace PX.Objects.CN.JointChecks.AP.GraphExtensions
{
	public abstract class JointPayeeGraphExtBase<TGraph>: PXGraphExtension<TGraph> 
		where TGraph : PXGraph
	{
		public abstract PXSelectBase<JointPayee> JointPayeeViewAccessor { get; }

		protected void _(Events.RowInserted<JointPayeePayment> e)
		{
			JointPayee payee = JointPayee.PK.Find(Base, e.Row.JointPayeeId);

			payee.LinkedToPayment = true;
			
			JointPayeeViewAccessor.Update(payee);
		}

		protected void _(Events.RowDeleted<JointPayeePayment> e)
		{
			JointPayeePayment payeePayment = e.Row;

			JointPayeePayment otherPayeePaymentForPayee =
				PXSelect<JointPayeePayment,
					Where<JointPayeePayment.jointPayeeId, Equal<Required<JointPayeePayment.jointPayeeId>>>>
					.SelectSingleBound(Base, null, payeePayment.JointPayeeId);

			if (otherPayeePaymentForPayee == null)
			{
				JointPayee payee = JointPayee.PK.Find(Base, payeePayment.JointPayeeId);

				payee.LinkedToPayment = false;

				JointPayeeViewAccessor.Update(payee);
			}
		}
	}
}
