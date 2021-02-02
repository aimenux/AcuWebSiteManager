using PX.Data;
using System;

namespace PX.Objects.PR
{
	public class UnitType
	{
		public class ListAttribute : PXStringListAttribute, IPXRowSelectedSubscriber
		{
			private readonly Type _pieceworkEarningTypeField;
			private readonly Type _unitTypeField;

			public void RowSelected(PXCache sender, PXRowSelectedEventArgs e)
			{
				if (_pieceworkEarningTypeField == null || _unitTypeField == null || e.Row == null)
					return;

				bool showTimeUnits = true;

				PRSetup preferences = sender.Graph.Caches[typeof(PRSetup)].Current as PRSetup ??
					new PXSetupSelect<PRSetup>(sender.Graph).SelectSingle();

				if (preferences.EnablePieceworkEarningType == true)
				{
					bool? isPieceworkEarningType = (bool?)sender.GetValue(e.Row, _pieceworkEarningTypeField.Name);
					if (isPieceworkEarningType == true)
						showTimeUnits = false;
				}

				Tuple<string, string>[] valuesToLabels = showTimeUnits ? GetTimeUnits() : GetMiscUnit();
				SetList(sender, e.Row, _unitTypeField.Name, valuesToLabels);
			}

			public ListAttribute() :
				base(new string[] { Hour, Misc }, new string[] { Messages.Hour, Messages.Misc })
			{
			}

			public ListAttribute(Type pieceworkEarningTypeField, Type unitTypeField) : base()
			{
				_pieceworkEarningTypeField = pieceworkEarningTypeField;
				_unitTypeField = unitTypeField;
			}

			private static Tuple<string, string>[] GetTimeUnits()
			{
				return new Tuple<string, string>[]
				{
					Pair(Hour, Messages.Hour),
					Pair(Year, Messages.Year)
				};
			}

			private static Tuple<string, string>[] GetMiscUnit()
			{
				return new Tuple<string, string>[]
				{
					Pair(Misc, Messages.Misc)
				};
			}
		}

		public class hour : PX.Data.BQL.BqlString.Constant<hour>
		{
			public hour() : base(Hour)
			{
			}
		}

		public class year : PX.Data.BQL.BqlString.Constant<year>
		{
			public year() : base(Year)
			{
			}
		}

		public class misc : PX.Data.BQL.BqlString.Constant<misc>
		{
			public misc() : base(Misc)
			{
			}
		}

		public const string Hour = "HOR";
		public const string Year = "SAL";
		public const string Misc = "MSC";
	}
}
