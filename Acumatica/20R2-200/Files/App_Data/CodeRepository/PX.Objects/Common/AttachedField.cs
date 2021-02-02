using System;
using System.Linq;
using System.Reflection;
using PX.Common;
using PX.Common.Extensions;
using PX.Data;

namespace PX.Objects.Common
{
	[PXInternalUseOnly]
	public static class PXFieldAttachedTo<TTable>
		where TTable : class, IBqlTable, new()
	{
		[PXInternalUseOnly]
		public static class By<TGraph>
			where TGraph : PXGraph
		{
			[PXInternalUseOnly]
			public static class ByExt1<TExt1>
				where TExt1: PXGraphExtension<TGraph>
			{
				public static class ByExt2<TExt2>
					where TExt2 : PXGraphExtension<TExt1, TGraph>
				{
					[PXInternalUseOnly]
					public abstract class As<TValue> : PXGraphExtension<TExt2, TExt1, TGraph>
					{
						protected PXUIFieldAttribute FieldAttribute => GetType().GetCustomAttribute<PXUIFieldAttribute>();

						public override void Initialize()
						{
							FieldName = this.GetType().Name.Split('+').Last();
							Base.Caches<TTable>().Fields.Add(FieldName);
							Base.FieldSelecting.AddHandler(typeof(TTable), FieldName, FieldSelecting);
						}

						private void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
						{
							var state = DefaultState(sender, e);
							if (FieldAttribute != null)
								state = AdjustByAttribute(state, FieldAttribute);
							state = AdjustStateBySelf(state);
							state = AdjustStateByRow(state, (TTable)e.Row);

							e.ReturnState = state;
							if (SuppressValueSetting == false)
								e.ReturnValue = e.Row == null ? ValueForEmptyRow : GetValue((TTable)e.Row);
						}

						protected abstract PXFieldState DefaultState(PXCache sender, PXFieldSelectingEventArgs e);

						protected virtual PXFieldState AdjustByAttribute(PXFieldState state, PXUIFieldAttribute uiAttribute)
						{
							state.Visible = uiAttribute.Visible;
							state.Visibility = uiAttribute.Visibility;
							if (uiAttribute.DisplayName != null)
								state.DisplayName = PXMessages.LocalizeFormatNoPrefix(uiAttribute.DisplayName);
							return state;
						}

						protected virtual PXFieldState AdjustStateBySelf(PXFieldState state)
						{
							state.Enabled = false;
							Visible.With(it => state.Visible = it);
							Visibility.With(it => state.Visibility = it);
							DisplayName.With(it => state.DisplayName = PXMessages.LocalizeFormatNoPrefix(it));
							return state;
						}

						protected virtual PXFieldState AdjustStateByRow(PXFieldState state, TTable row) => state;

						protected virtual bool SuppressValueSetting => false;

						public abstract TValue GetValue(TTable Row);
						protected virtual TValue ValueForEmptyRow => default(TValue);

						protected virtual bool? Visible => null;
						protected virtual PXUIVisibility Visibility => Visible == true ? PXUIVisibility.Visible : PXUIVisibility.Invisible;
						protected virtual string DisplayName => null;

						public Type BqlTable => typeof(TTable);
						public virtual string FieldName { get; private set; }

						public static TValue GetValue<TSelf>(TGraph graph, TTable row)
							where TSelf : As<TValue>
							=> graph.GetExtension<TSelf>().GetValue(row);

						public static object GetValueExt<TSelf>(TGraph graph, TTable row)
							where TSelf : As<TValue>
							=> ((PXFieldState)graph.Caches<TTable>().GetStateExt(row, typeof(TSelf).Name.Split('+').Last()))?.Value;
					}

					[PXInternalUseOnly]
					public abstract class AsDecimal : As<decimal?>
					{
						[PXInternalUseOnly]
						public abstract class Named<TSelf> : AsDecimal
							where TSelf : AsDecimal
						{
							public static decimal? GetValue(TGraph graph, TTable row) => GetValue<TSelf>(graph, row);
						}

						protected override PXFieldState DefaultState(PXCache sender, PXFieldSelectingEventArgs e) => PXDecimalState.CreateInstance(e.ReturnState, Precision, FieldName, null, null, null, null);

						protected virtual int Precision => 2;
					}
				}
			}

			[PXInternalUseOnly]
			public abstract class As<TValue> : PXGraphExtension<TGraph>
			{
				protected PXUIFieldAttribute FieldAttribute => GetType().GetCustomAttribute<PXUIFieldAttribute>();

				public override void Initialize()
				{
					FieldName = this.GetType().Name.LastSegment('+');
					Base.Caches<TTable>().Fields.Add(FieldName);
					Base.FieldSelecting.AddHandler(typeof(TTable), FieldName, FieldSelecting);
				}

				private void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
				{
					var state = DefaultState(sender, e);
					if (FieldAttribute != null)
						state = AdjustByAttribute(state, FieldAttribute);
					state = AdjustStateBySelf(state);
					state = AdjustStateByRow(state, (TTable)e.Row);

					e.ReturnState = state;
					if (SuppressValueSetting == false)
						e.ReturnValue = e.Row == null ? ValueForEmptyRow : GetValue((TTable)e.Row);
				}

				protected abstract PXFieldState DefaultState(PXCache sender, PXFieldSelectingEventArgs e);

				protected virtual PXFieldState AdjustByAttribute(PXFieldState state, PXUIFieldAttribute uiAttribute)
				{
					state.Visible = uiAttribute.Visible;
					state.Visibility = uiAttribute.Visibility;
					if (uiAttribute.DisplayName != null)
						state.DisplayName = PXMessages.LocalizeFormatNoPrefix(uiAttribute.DisplayName);
					return state;
				}

				protected virtual PXFieldState AdjustStateBySelf(PXFieldState state)
				{
					state.Enabled = false;
					Visible.With(it => state.Visible = it);
					DisplayName.With(it => state.DisplayName = PXMessages.LocalizeFormatNoPrefix(it));
					return state;
				}

				protected virtual PXFieldState AdjustStateByRow(PXFieldState state, TTable row) => state;

				protected virtual bool SuppressValueSetting => false;

				public abstract TValue GetValue(TTable Row);
				protected virtual TValue ValueForEmptyRow => default(TValue);

				protected virtual bool? Visible => null;
				protected virtual string DisplayName => null;

				public Type BqlTable => typeof(TTable);
				public virtual string FieldName { get; private set; }

				public static TValue GetValue<TSelf>(TGraph graph, TTable row)
					where TSelf : PXFieldAttachedTo<TTable>.By<TGraph>.As<TValue>
					=> graph.GetExtension<TSelf>().GetValue(row);

				public static object GetValueExt<TSelf>(TGraph graph, TTable row)
					where TSelf : PXFieldAttachedTo<TTable>.By<TGraph>.As<TValue>
					=> ((PXFieldState)graph.Caches<TTable>().GetStateExt(row, typeof(TSelf).Name.LastSegment('+')))?.Value;
			}

			[PXInternalUseOnly]
			public abstract class AsBool : PXFieldAttachedTo<TTable>.By<TGraph>.As<bool?>
			{
				[PXInternalUseOnly]
				public abstract class Named<TSelf> : AsBool
					where TSelf : PXFieldAttachedTo<TTable>.By<TGraph>.AsBool
				{
					public static bool? GetValue(TGraph graph, TTable row) => GetValue<TSelf>(graph, row);
				}

				protected override PXFieldState DefaultState(PXCache sender, PXFieldSelectingEventArgs e) => PXFieldState.CreateInstance(e.ReturnState, typeof(bool), fieldName: FieldName);
			}

			[PXInternalUseOnly]
			public abstract class AsInteger : PXFieldAttachedTo<TTable>.By<TGraph>.As<int?>
			{
				[PXInternalUseOnly]
				public abstract class Named<TSelf> : AsInteger
					where TSelf : PXFieldAttachedTo<TTable>.By<TGraph>.AsInteger
				{
					public static int? GetValue(TGraph graph, TTable row) => GetValue<TSelf>(graph, row);
				}

				protected override PXFieldState DefaultState(PXCache sender, PXFieldSelectingEventArgs e) => PXIntState.CreateInstance(e.ReturnState, FieldName, null, null, null, null, null, null, typeof(int), DefaultValue, null);

				protected virtual int? DefaultValue => null;
			}

			[PXInternalUseOnly]
			public abstract class AsDecimal : PXFieldAttachedTo<TTable>.By<TGraph>.As<decimal?>
			{
				[PXInternalUseOnly]
				public abstract class Named<TSelf> : AsDecimal
					where TSelf : PXFieldAttachedTo<TTable>.By<TGraph>.AsDecimal
				{
					public static decimal? GetValue(TGraph graph, TTable row) => GetValue<TSelf>(graph, row);
				}

				protected override PXFieldState DefaultState(PXCache sender, PXFieldSelectingEventArgs e) => PXDecimalState.CreateInstance(e.ReturnState, Precision, FieldName, null, null, null, null);

				protected virtual int Precision => 2;
			}

			[PXInternalUseOnly]
			public abstract class AsDateTime : PXFieldAttachedTo<TTable>.By<TGraph>.As<DateTime?>
			{
				[PXInternalUseOnly]
				public abstract class Named<TSelf> : AsDateTime
					where TSelf : PXFieldAttachedTo<TTable>.By<TGraph>.AsDateTime
				{
					public static DateTime? GetValue(TGraph graph, TTable row) => GetValue<TSelf>(graph, row);
				}

				protected override PXFieldState DefaultState(PXCache sender, PXFieldSelectingEventArgs e) => PXDateState.CreateInstance(e.ReturnState, FieldName, null, null, null, null, null, null);
			}

			[PXInternalUseOnly]
			public abstract class AsString : PXFieldAttachedTo<TTable>.By<TGraph>.As<String>
			{
				[PXInternalUseOnly]
				public abstract class Named<TSelf> : AsString
					where TSelf : PXFieldAttachedTo<TTable>.By<TGraph>.AsString
				{
					public static String GetValue(TGraph graph, TTable row) => GetValue<TSelf>(graph, row);
				}

				protected override PXFieldState DefaultState(PXCache sender, PXFieldSelectingEventArgs e) => PXStringState.CreateInstance(e.ReturnState, Length, IsUnicode, FieldName, null, null, null, null, null, null, DefaultValue);

				protected virtual int? Length => null;
				protected virtual bool? IsUnicode => null;
				protected virtual string DefaultValue => null;
			}
		}
	}
}