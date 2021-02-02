using PX.Data;

namespace PX.Objects.PR
{
	public class PaymentStatus
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(
				new string[] { Hold, NeedCalculation,
					PendingPrintOrPayment, CheckPrintedOrPaid,
					Released, LiabilityPartiallyPaid,
					Closed, Voided,
					Open },
				new string[] { Messages.Hold, Messages.NeedCalculation,
					Messages.PendingPrintOrPayment, Messages.CheckPrintedOrPaid,
					Messages.Released, Messages.LiabilityPartiallyPaid,
					Messages.Closed, Messages.Voided,
					Messages.Open})
			{ }
		}

		public class hold : PX.Data.BQL.BqlString.Constant<hold>
		{
			public hold() : base(Hold)
			{
			}
		}

		public class needCalculation : PX.Data.BQL.BqlString.Constant<needCalculation>
		{
			public needCalculation() : base(NeedCalculation)
			{
			}
		}

		public class pendingPrintOrPayment : PX.Data.BQL.BqlString.Constant<pendingPrintOrPayment>
		{
			public pendingPrintOrPayment() : base(PendingPrintOrPayment)
			{
			}
		}

		public class checkPrintedOrPaid : PX.Data.BQL.BqlString.Constant<checkPrintedOrPaid>
		{
			public checkPrintedOrPaid() : base(CheckPrintedOrPaid)
			{
			}
		}

		public class released : PX.Data.BQL.BqlString.Constant<released>
		{
			public released() : base(Released)
			{
			}
		}

		public class liabilityPartiallyPaid : PX.Data.BQL.BqlString.Constant<liabilityPartiallyPaid>
		{
			public liabilityPartiallyPaid() : base(LiabilityPartiallyPaid)
			{
			}
		}

		public class closed : PX.Data.BQL.BqlString.Constant<closed>
		{
			public closed() : base(Closed)
			{
			}
		}

		public class voided : PX.Data.BQL.BqlString.Constant<voided>
		{
			public voided() : base(Voided)
			{
			}
		}

		public class open : PX.Data.BQL.BqlString.Constant<open>
		{
			public open() : base(Open)
			{
			}
		}

		public const string Hold = "HLD";
		public const string NeedCalculation = "CAL";
		public const string PendingPrintOrPayment = "PPR";
		public const string CheckPrintedOrPaid = "PRT";
		public const string Released = "REL";
		public const string LiabilityPartiallyPaid = "LPP";
		public const string Closed = "CLS";
		public const string Voided = "VDD";
		public const string Open = "OPN";
	}
}
