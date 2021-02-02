using PX.Data;
using PX.Objects.EP;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PX.Objects.PR
{
	public class EarningTypeCategory
	{
		public class ListAttribute : PXStringListAttribute,
			IPXRowSelectedSubscriber, IPXFieldUpdatingSubscriber
		{
			private static Dictionary<string, string> _EarningTypeFlags =
				new Dictionary<string, string>
				{
					{ nameof(EPEarningType.IsOvertime), Overtime},
					{ nameof(PREarningType.IsPiecework), Piecework},
					{ nameof(PREarningType.IsAmountBased), AmountBased},
					{ nameof(PREarningType.IsPTO), TimeOff}
				};

			private static Tuple<string, string>[] _ValuesAndLabelsWithPiecework;
			private static Tuple<string, string>[] _ValuesAndLabelsWithoutPiecework;

			public ListAttribute() : base(GetValuesAndLabels())
			{
			}

			public void RowSelected(PXCache sender, PXRowSelectedEventArgs e)
			{
				PRSetup preferences = sender.Graph.Caches[typeof(PRSetup)].Current as PRSetup ??
									new PXSetupSelect<PRSetup>(sender.Graph).SelectSingle();

				if (preferences.EnablePieceworkEarningType != true)
					SetList(sender, e.Row, FieldName, GetValuesAndLabels(false));
			}

			public override void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
			{
				base.FieldSelecting(sender, e);

				if (e.Row == null)
					return;

				e.ReturnValue = GetEarningType(sender, e.Row);
			}

			public void FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
			{
				string earningTypeValue = e.NewValue as string;

				if (string.IsNullOrWhiteSpace(earningTypeValue))
					return;

				foreach (KeyValuePair<string, string> flagNameAndValue in _EarningTypeFlags)
				{
					bool? oldFlagValue = sender.GetValue(e.Row, flagNameAndValue.Key) as bool?;
					bool newFlagValue = flagNameAndValue.Value == earningTypeValue;

					if (oldFlagValue != newFlagValue)
						sender.SetValueExt(e.Row, flagNameAndValue.Key, newFlagValue);
				}
			}

			private string GetEarningType(PXCache sender, object earningTypeRecord)
			{
				foreach (KeyValuePair<string, string> flagNameAndValue in _EarningTypeFlags)
					if (sender.GetValue(earningTypeRecord, flagNameAndValue.Key) as bool? == true)
						return flagNameAndValue.Value;

				return Salary;
			}

			private static Tuple<string, string>[] GetValuesAndLabels(bool addPiecework = true)
			{
				if (addPiecework && _ValuesAndLabelsWithPiecework != null)
					return _ValuesAndLabelsWithPiecework;

				if (!addPiecework && _ValuesAndLabelsWithoutPiecework != null)
					return _ValuesAndLabelsWithoutPiecework;

				List<Tuple<string, string>> valuesAndLabels = new List<Tuple<string, string>>();

				valuesAndLabels.Add(new Tuple<string, string>(Salary, PXLocalizer.Localize(Messages.Salary)));
				valuesAndLabels.Add(new Tuple<string, string>(Overtime, PXLocalizer.Localize(Messages.Overtime)));
				if (addPiecework)
					valuesAndLabels.Add(new Tuple<string, string>(Piecework, PXLocalizer.Localize(Messages.Piecework)));
				valuesAndLabels.Add(new Tuple<string, string>(AmountBased, PXLocalizer.Localize(Messages.AmountBased)));
				valuesAndLabels.Add(new Tuple<string, string>(TimeOff, PXLocalizer.Localize(Messages.TimeOff)));

				Tuple<string, string>[] result = valuesAndLabels.ToArray();

				if (addPiecework)
					_ValuesAndLabelsWithPiecework = result;
				else
					_ValuesAndLabelsWithoutPiecework = result;

				return result;
			}

			public static Type GetWhereClauseForEarningTypeCategory(PXCache cache, string earningTypeCategory)
			{
				Dictionary<string, (Type field, bool value)> queryParameters = _EarningTypeFlags.ToDictionary(kvp => kvp.Value, kvp => (cache.GetBqlField(kvp.Key), false));
				if (queryParameters.ContainsKey(earningTypeCategory))
				{
					queryParameters[earningTypeCategory] = (queryParameters[earningTypeCategory].field, true);
				}

				List<Type> bqlParameters = new List<Type>();
				bool first = true;
				foreach ((Type field, bool value) in queryParameters.Values)
				{
					if (field == null)
					{
						continue;
					}

					if (first)
					{
						bqlParameters.Add(value ? typeof(Where<,,>) : typeof(Where2<,>));
						first = false;
					}
					else
					{
						bqlParameters.Add(value ? typeof(And<,,>) : typeof(And2<,>));
					}

					if (value)
					{
						bqlParameters.Add(field);
						bqlParameters.Add(typeof(Equal<>));
						bqlParameters.Add(typeof(True));
					}
					else
					{
						bqlParameters.Add(typeof(Where<,,>));
						bqlParameters.Add(field);
						bqlParameters.Add(typeof(NotEqual<>));
						bqlParameters.Add(typeof(True));
						bqlParameters.Add(typeof(Or<,>));
						bqlParameters.Add(field);
						bqlParameters.Add(typeof(IsNull));
					}
				}
				bqlParameters.Add(typeof(And<,>));
				bqlParameters.Add(typeof(True));
				bqlParameters.Add(typeof(Equal<>));
				bqlParameters.Add(typeof(True));

				return BqlCommand.Compose(bqlParameters.ToArray());
			}
		}

		public const string Salary = "SLR";
		public const string Overtime = "OVT";
		public const string Piecework = "PCW";
		public const string AmountBased = "AMB";
		public const string TimeOff = "TOF";
	}
}
