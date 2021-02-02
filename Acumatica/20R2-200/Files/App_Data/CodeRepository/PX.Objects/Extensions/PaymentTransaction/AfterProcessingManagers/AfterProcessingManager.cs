using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;
using PX.Objects;
using PX.Objects.AR.CCPaymentProcessing.Interfaces;
using PX.Objects.AR.CCPaymentProcessing.Common;
using PX.Objects.AR.CCPaymentProcessing;
namespace PX.Objects.Extensions.PaymentTransaction
{
	public abstract class AfterProcessingManager
	{
		public bool IsMassProcess { get; set; }

		public AfterProcessingManager()
		{

		}

		public virtual void RunAuthorizeActions(IBqlTable table, bool success)
		{
		}

		public virtual void RunCaptureActions(IBqlTable table, bool success)
		{
		}

		public virtual void RunPriorAuthorizedCaptureActions(IBqlTable table, bool success)
		{ 
		}

		public virtual void RunVoidActions(IBqlTable table, bool success)
		{
		}

		public virtual void RunCreditActions(IBqlTable table, bool success)
		{
		}

		public virtual void RunCaptureOnlyActions(IBqlTable table, bool success)
		{
		}

		public virtual void RunUnknownActions(IBqlTable table, bool success)
		{
		}

		public abstract PXGraph GetGraph();

		public abstract void PersistData();
	}
}
