using PX.Data;
using System;
using System.Linq;

namespace PX.Objects.PR
{
	public class DedCntCalculationMethod
	{
		public class ListAttribute : PXStringListAttribute, IPXRowSelectedSubscriber
		{
			private Type _IsWorkersComensationField = null;
			private Type _AffectsTaxesField = null;
			private Type _IsPayableBenefitField = null;

			public ListAttribute() : base(
				new string[] { FixedAmount, PercentOfGross, PercentOfCustom, PercentOfNet, AmountPerHour },
				new string[] { Messages.FixedAmount, Messages.PercentOfGross, Messages.PercentOfCustom, Messages.PercentOfNet, Messages.AmountPerHour })
			{ }
			
			public ListAttribute(Type isWorkersComensationField, Type affectsTaxesField, Type isPayableBenefitField) : this()
			{
				_IsWorkersComensationField = isWorkersComensationField;
				_AffectsTaxesField = affectsTaxesField;
				_IsPayableBenefitField = isPayableBenefitField;
			}

			public override void CacheAttached(PXCache sender)
			{
				base.CacheAttached(sender);
				if (_IsWorkersComensationField == null || _AffectsTaxesField == null || _IsPayableBenefitField == null)
				{
					return;
				}

				sender.Graph.FieldUpdated.AddHandler(sender.GetItemType(), _IsWorkersComensationField.Name, DependentFieldUpdated);
				sender.Graph.FieldUpdated.AddHandler(sender.GetItemType(), _AffectsTaxesField.Name, DependentFieldUpdated);
				sender.Graph.FieldUpdated.AddHandler(sender.GetItemType(), _IsPayableBenefitField.Name, DependentFieldUpdated);
			}

			public void RowSelected(PXCache sender, PXRowSelectedEventArgs e)
			{
				if (e.Row == null || _IsWorkersComensationField == null || _AffectsTaxesField == null || _IsPayableBenefitField == null)
				{
					return;
				}

				GetAvailableValuesAndLabels(sender, e.Row, out string[] values, out string[] labels);
				SetList(sender, e.Row, _FieldName, values, labels);
			}

			private void DependentFieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
			{
				GetAvailableValuesAndLabels(sender, e.Row, out string[] values, out string[] _);
				string calculationMethod = (string)sender.GetValue(e.Row, _FieldName);
				if (!values.Contains(calculationMethod))
				{
					sender.SetValue(e.Row, _FieldName, null);
				}
			}

			private void GetAvailableValuesAndLabels(PXCache sender, object row, out string[] values, out string[] labels)
			{
				bool? isWorkserCompensation = sender.GetValue(row, _IsWorkersComensationField.Name) as bool?;
				bool? affectsTaxes = sender.GetValue(row, _AffectsTaxesField.Name) as bool?;
				bool? isPayableBenefit = sender.GetValue(row, _IsPayableBenefitField.Name) as bool?;

				if (affectsTaxes == true && isPayableBenefit == true)
				{
					values = new string[] { AmountPerHour, FixedAmount };
					labels = new string[] { Messages.AmountPerHour, Messages.FixedAmount };
				}
				else if (affectsTaxes == true)
				{
					values = new string[] { PercentOfGross, AmountPerHour, FixedAmount };
					labels = new string[] { Messages.PercentOfGross, Messages.AmountPerHour, Messages.FixedAmount };
				}
				else if (isPayableBenefit == true)
				{
					values = new string[] { PercentOfCustom, AmountPerHour, FixedAmount };
					labels = new string[] { Messages.PercentOfCustom, Messages.AmountPerHour, Messages.FixedAmount };
				}
				else if (isWorkserCompensation == true)
				{
					values = new string[] { PercentOfGross, PercentOfCustom, AmountPerHour };
					labels = new string[] { Messages.PercentOfGross, Messages.PercentOfCustom, Messages.AmountPerHour };
				}
				else
				{
					values = new string[] { FixedAmount, PercentOfGross, PercentOfCustom, PercentOfNet, AmountPerHour };
					labels = new string[] { Messages.FixedAmount, Messages.PercentOfGross, Messages.PercentOfCustom, Messages.PercentOfNet, Messages.AmountPerHour };
				}
			}
		}

		public class fixedAmount : PX.Data.BQL.BqlString.Constant<fixedAmount>
		{
			public fixedAmount()
				: base(FixedAmount)
			{
			}
		}

		public class percentOfGross : PX.Data.BQL.BqlString.Constant<percentOfGross>
		{
			public percentOfGross()
				: base(PercentOfGross)
			{
			}
		}

		public class percentOfNet : PX.Data.BQL.BqlString.Constant<percentOfNet>
		{
			public percentOfNet()
				: base(PercentOfNet)
			{
			}
		}

		public class amountPerHour : PX.Data.BQL.BqlString.Constant<amountPerHour>
		{
			public amountPerHour()
				: base(AmountPerHour)
			{
			}
		}

		public class percentOfCustom : PX.Data.BQL.BqlString.Constant<percentOfCustom>
		{
			public percentOfCustom()
				: base(PercentOfCustom)
			{
			}
		}

		public const string FixedAmount = "FIX";
		public const string PercentOfGross = "GRS";
		public const string PercentOfNet = "NET";
		public const string AmountPerHour = "APH";
		public const string PercentOfCustom = "CUS";
	}
}
