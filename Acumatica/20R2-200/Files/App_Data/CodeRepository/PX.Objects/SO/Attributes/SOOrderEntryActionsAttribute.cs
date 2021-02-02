using PX.Common;
using PX.Data;
using System;

namespace PX.Objects.SO
{
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.Method, AllowMultiple = false)]
	public class SOOrderEntryActionsAttribute : PXIntListAttribute
	{
		public static class Values
		{
			public const int CreateShipment = 1;
			public const int ApplyAssignmentRules = 2;
			public const int CreateInvoice = 3;
			public const int PostInvoiceToIN = 4;
			public const int CreatePurchaseOrder = 5;
			public const int CreateTransferOrder = 6;
			public const int ReopenOrder = 7;
		}

		[PXLocalizable]
		public static class DisplayNames
		{
			public const string CreateShipment = "Create Shipment";
			public const string ApplyAssignmentRules = "Apply Assignment Rules";
			public const string PrepareInvoice = "Prepare Invoice";
			public const string PostInvoiceToIN = "Post Invoice to IN";
			public const string CreatePurchaseOrder = "Create Purchase Order";
			public const string CreateTransferOrder = "Create Transfer Order";
			public const string ReopenOrder = "Re-Open Order";
		}

		private static Tuple<int, string>[] ValuesToLabels => new Tuple<int, string>[]
		{
			Pair(Values.CreateShipment, DisplayNames.CreateShipment),
			Pair(Values.ApplyAssignmentRules, DisplayNames.ApplyAssignmentRules),
			Pair(Values.CreateShipment, DisplayNames.CreateShipment),
			Pair(Values.PostInvoiceToIN, DisplayNames.PostInvoiceToIN),
			Pair(Values.CreatePurchaseOrder, DisplayNames.CreatePurchaseOrder),
			Pair(Values.CreateTransferOrder, DisplayNames.CreateTransferOrder),
			Pair(Values.ReopenOrder, DisplayNames.ReopenOrder)
		};

		public SOOrderEntryActionsAttribute():base(ValuesToLabels)
		{ }
	}
}
