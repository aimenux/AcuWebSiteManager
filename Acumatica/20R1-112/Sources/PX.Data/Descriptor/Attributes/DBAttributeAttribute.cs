// This File is Distributed as Part of Acumatica Shared Source Code 
/* ---------------------------------------------------------------------*
*                               Acumatica Inc.                          *
*              Copyright (c) 1994-2011 All rights reserved.             *
*                                                                       *
*                                                                       *
* This file and its contents are protected by United States and         *
* International copyright laws.  Unauthorized reproduction and/or       *
* distribution of all or any portion of the code contained herein       *
* is strictly prohibited and will result in severe civil and criminal   *
* penalties.  Any violations of this copyright will be prosecuted       *
* to the fullest extent possible under law.                             *
*                                                                       *
* UNDER NO CIRCUMSTANCES MAY THE SOURCE CODE BE USED IN WHOLE OR IN     *
* PART, AS THE BASIS FOR CREATING A PRODUCT THAT PROVIDES THE SAME, OR  *
* SUBSTANTIALLY THE SAME, FUNCTIONALITY AS ANY ProjectX PRODUCT.        *
*                                                                       *
* THIS COPYRIGHT NOTICE MAY NOT BE REMOVED FROM THIS FILE.              *
* ---------------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using PX.Common;
using System.Collections.Concurrent;
using PX.Data.SQLTree;
using PX.Data.PushNotifications;

namespace PX.Data
{
    /// <exclude/>
	public class PXDBAttributeAttribute : PXDBFieldAttribute
	{
		protected readonly BqlCommand _SingleSelect;
		protected readonly BqlCommand _SubSelect;
		protected readonly IBqlWhere _PureWhere;
		protected Type _Field;
		protected PXView _View;
		protected BqlCommand _Selector;		
		protected PXFieldState[] _Fields;
		protected Dictionary<string, int> _AttributeIndices;
		protected bool _IsActive;

		protected readonly bool aggregateAttributes = true; // enables/disables the feature

		protected string _DescriptionField = "Description";
		protected string _ControlTypeField = "ControlType";
		protected string _EntryMaskField = "EntryMask";
		protected string _ListField = "List";

        /// <summary>Get, set.</summary>
		public string DescriptionField 
		{
			get { return _DescriptionField; }
			set { _DescriptionField = value; }
		}
        /// <summary>Get, set.</summary>
		public string ControlTypeField
		{
			get { return _ControlTypeField; }
			set { _ControlTypeField = value; }
		}
        /// <summary>Get, set.</summary>
		public string EntryMaskField
		{
			get { return _EntryMaskField; } 
			set { _EntryMaskField = value; }
		}
        /// <summary>Get, set.</summary>
		public string ListField
		{
			get { return _ListField; }
			set { _ListField = value; }
		}

		internal Type Field => _Field;

		internal bool IsActive => _IsActive;


		public static void Activate(PXCache cache)
		{
			foreach (PXDBAttributeAttribute attribute in cache.GetAttributes(null).OfType<PXDBAttributeAttribute>())
			{
				attribute._IsActive = true;
			}
		}

        /// 
		public PXDBAttributeAttribute(Type valueSearch, Type attributeID)
		{
			if (valueSearch == null)
			{
				throw new PXArgumentException("type", ErrorMessages.ArgumentNullException);
			}
			if (attributeID == null)
			{
				throw new PXArgumentException("field", ErrorMessages.ArgumentNullException);
			}
			_Field = attributeID;
			if (typeof(IBqlSearch).IsAssignableFrom(valueSearch))
			{
				_SingleSelect = BqlCommand.CreateInstance(valueSearch);
			}
			else if (valueSearch.IsNested && typeof(IBqlField).IsAssignableFrom(valueSearch))
			{
				_SingleSelect = BqlCommand.CreateInstance(typeof(Search<>), valueSearch);
			}
			else
			{
				throw new PXArgumentException(nameof(valueSearch), ErrorMessages.CantCreateForeignKeyReference, valueSearch);
			}
			_PureWhere = (_SingleSelect as IHasBqlWhere).GetWhere();
			_SubSelect = _SingleSelect.WhereAnd(typeof(Where<,>).MakeGenericType(_Field, typeof(Equal<AttributeIDPlaceholder>)));
			_SingleSelect = _SingleSelect.WhereAnd(BqlCommand.Compose(typeof(Where<,>), _Field, typeof(Equal<>), typeof(Required<>), _Field));
		}

        /// 
		public PXDBAttributeAttribute(Type valueSearch, Type attributeID, Type attributeSearch)
			: this(valueSearch, attributeID)
		{
			if (attributeSearch == null)
			{
				throw new PXArgumentException("attributeSearch", ErrorMessages.ArgumentNullException);
			}
			if (typeof(IBqlSearch).IsAssignableFrom(attributeSearch))
			{
				_Selector = BqlCommand.CreateInstance(attributeSearch);
			}
			else if (attributeSearch.IsNested && typeof(IBqlField).IsAssignableFrom(attributeSearch))
			{
				_Selector = BqlCommand.CreateInstance(typeof(Search<>), attributeSearch);
			}
			else
			{
				throw new PXArgumentException(nameof(attributeSearch), ErrorMessages.CantCreateForeignKeyReference, attributeSearch);
			}
		}

        /// <exclude/>
		protected sealed class DefinitionParams
		{
			public readonly PXCache Main;
			private Type _ForeignType;
			private PXCache _Foreign;
			public PXCache Foreign
			{
				get
				{
					if (_Foreign == null)
					{
						_Foreign = Main.Graph.Caches[_ForeignType];
					}
					return _Foreign;
				}
			}
			public readonly Type Field;
			public readonly BqlCommand Selector;
			public readonly string DescriptionField;
			public readonly string ControlTypeField;
			public readonly string EntryMaskField;
			public readonly string ListField;
			public readonly string FieldName;
			public DefinitionParams(PXCache main, Type foreignType, Type field, BqlCommand selector,
				string descriptionField, string controlTypeField, string entryMaskField, string listField,
				string fieldName)
			{
				Main = main;
				_ForeignType = foreignType;
				Field = field;
				Selector = selector;
				DescriptionField = descriptionField;
				ControlTypeField = controlTypeField;
				EntryMaskField = entryMaskField;
				ListField = listField;
				FieldName = fieldName;
			}
		}

        /// <exclude/>
		public class AttributeIDPlaceholder : PX.Data.BQL.BqlString.Constant<AttributeIDPlaceholder>
		{
			public AttributeIDPlaceholder()
				: base("{04d5718e-a4d3-4e57-b476-782b3105c3d3}")
			{
			}
		}

        /// <exclude/>
		protected class Definition : IPrefetchable<DefinitionParams>
		{
			public PXFieldState[] Fields;
			void IPrefetchable<DefinitionParams>.Prefetch(DefinitionParams parameters)
			{
				try
				{
					PXContext.SetSlot<bool>(PXSelectorAttribute.selectorBypassInit, true);
					List<object> attributes = new List<object>();
					List<string> distinct = new List<string>();
					PXView view = null;
					IBqlSearch srch = null;
					if (parameters.Selector == null)
					{
						PXFieldState state = parameters.Foreign.GetStateExt(null, parameters.Field.Name) as PXFieldState;
						if (state != null && !String.IsNullOrEmpty(state.ViewName))
						{
							view = parameters.Main.Graph.Views[state.ViewName];
							srch = view.BqlSelect as IBqlSearch;
						}
					}
					else
					{
						view = new PXView(parameters.Main.Graph, true, parameters.Selector);
						srch = view.BqlSelect as IBqlSearch;
					}
					if (srch != null)
					{
						if (!view.AllowSelect)
						{
							throw new PXException(ErrorMessages.NotEnoughRightsToAccessObject, view.GetItemType().Name);
						}
						object current = parameters.Main.CreateInstance();
						foreach (object res in view.SelectMultiBound(new object[] {current}))
						{
							object attr = (res is PXResult ? ((PXResult) res)[0] : res);
							if (attr != null)
							{
								string name = view.Cache.GetValue(attr, srch.GetField().Name) as string;
								if (!String.IsNullOrEmpty(name) && !distinct.Contains(name))
								{
									distinct.Add(name);
									attributes.Add(attr);
								}
							}
						}

						// Creating empty graph to get original field state without overridden attributes on CacheAttached in parameters.Main.Graph
						PXGraph independentGraph = PXGraph.CreateInstance<AttrGraph>();
						PXCache independentCache = independentGraph.Caches[parameters.Main.GetItemType()];
						foreach (IBqlParameter par in view.BqlSelect.GetParameters())
						{
							if (par.HasDefault && par.GetReferencedType().IsNested && BqlCommand.GetItemType(par.GetReferencedType()).IsAssignableFrom(parameters.Main.GetItemType()))
							{
								PXFieldState state = independentCache.GetStateExt(null, par.GetReferencedType().Name) as PXFieldState; // graph-independent state
								if (state != null && !String.IsNullOrEmpty(state.ViewName))
								{
									PXView parview = independentGraph.Views[state.ViewName];
									List<object> pars = new List<object>();
									IBqlSearch parsrch = parview.BqlSelect as IBqlSearch;
									if (parsrch != null)
									{
										foreach (object res in parview.SelectMultiBound(new object[] {current}))
										{
											object item = res;
											if (item is PXResult)
												item = PXResult.Unwrap(item, parview.CacheGetItemType());
											if (item is PXFieldState)
												item = PXFieldState.UnwrapValue(item);
											object cls = parview.Cache.GetValue(item, parsrch.GetField().Name);
											if (cls != null)
											{
												parameters.Main.SetValue(current, par.GetReferencedType().Name, cls);
												foreach (object parres in view.SelectMultiBound(new object[] {current}))
												{
													object attr = (parres is PXResult ? ((PXResult) parres)[0] : parres);
													if (attr != null)
													{
														string name = view.Cache.GetValue(attr, srch.GetField().Name) as string;
														if (!String.IsNullOrEmpty(name) && !distinct.Contains(name))
														{
															distinct.Add(name);
															attributes.Add(attr);
														}
													}
												}
											}
										}
									}
								}
								break;
							}
						}
					}

					List<PXFieldState> fields = new List<PXFieldState>();
					foreach (object attr in attributes)
					{
						var fs = CreateFieldState(((string)view.Cache.GetValue(attr, srch.GetField().Name)) + '_' + parameters.FieldName,
							(string)view.Cache.GetValue(attr, parameters.DescriptionField),
							(int) view.Cache.GetValue(attr, parameters.ControlTypeField),
							(string)view.Cache.GetValue(attr, parameters.EntryMaskField),
							(string)view.Cache.GetValue(attr, parameters.ListField));	

						if (fs != null)
						{
							fields.Add(fs);
						}
					}
					Fields = fields.ToArray();
				}
				finally
				{
					PXContext.SetSlot<bool>(PXSelectorAttribute.selectorBypassInit, false);
				}
			}

			public static PXFieldState CreateFieldState(string fieldName, string description, int ctype, string entryMask, string list)
			{				
				string msgprefix;
				string displayName = string.Format("${0}$-{1}", PXMessages.Localize(ActionsMessages.Attributes, out msgprefix),
															  description);
				PXFieldState fs = null;				
				switch (ctype)
				{
					case 1:
						{
							fs = PXStringState.CreateInstance(null, null, null, fieldName, null, null, entryMask, null, null, null, null);
							fs.Visibility = PXUIVisibility.Dynamic;
							fs.DisplayName = displayName;
							fs.Enabled = false;
							fs.Visible = false;
							break;
						}
					case 2:
					case 6:
						{
							List<string> vals = new List<string>();
							List<string> lbls = new List<string>();							
							if (!String.IsNullOrEmpty(list))
							{
								foreach (string elem in list.Split(new char[] { '\t' }, StringSplitOptions.RemoveEmptyEntries))
								{
									string[] pair = elem.Split(new char[] { '\0' }, StringSplitOptions.RemoveEmptyEntries);
									if (pair.Length > 0)
									{
										vals.Add(pair[0]);
										if (pair.Length > 1)
										{
											lbls.Add(pair[1]);
										}
										else
										{
											lbls.Add(pair[0]);
										}
									}
								}
							}
							fs = PXStringState.CreateInstance(null, null, null, fieldName, null, null, null, vals.ToArray(), lbls.ToArray(), true, null);
							fs.Visibility = PXUIVisibility.Dynamic;
							fs.DisplayName = displayName;
							fs.Enabled = false;
							fs.Visible = false;
							if (ctype == 6)
							{
								((PXStringState)fs).MultiSelect = true;
							}
							break;
						}
					case 4:
						{
							fs = PXFieldState.CreateInstance(null, typeof(Boolean), null, null, -1, null, null, null, fieldName, null, displayName, null, PXErrorLevel.Undefined, false, false, null, PXUIVisibility.Dynamic, null, null, null);
							break;
						}
					case 5:
						{
							fs = PXDateState.CreateInstance(null, fieldName, null, null, "d", "d", null, null);
							fs.Visibility = PXUIVisibility.Dynamic;
							fs.DisplayName = displayName;
							fs.Enabled = false;
							fs.Visible = false;
							break;
						}
				}
				return fs;
			}
		}

        /// <summary>Get, set.</summary>
		public bool DefaultVisible { get; set; }

		protected virtual void AttributeFieldSelecting(PXCache sender, PXFieldSelectingEventArgs e, PXFieldState state, string attributeName, int idx)
		{
			if (!_IsActive) return;

			if (_AttributeLevel == PXAttributeLevel.Item || e.IsAltered)
			{
				object value = e.ReturnValue;
				e.ReturnState = ((ICloneable)state).Clone();
				if (value != null)
					e.ReturnValue = value;
			}
			if (e.Row != null)
			{
				string[] fetched = sender.GetValue(e.Row, _FieldOrdinal) as string[];
				if (fetched == null)
				{
					int startRow = 0;
					int totalRows = 0;
					List<object> res = _View.Select(
						new object[] { e.Row },
						new object[] { attributeName },
						null,
						null,
						new bool[] { false },
						null,
						ref startRow,
						1,
						ref totalRows);
					if (res.Count > 0)
					{
						object ret = res[0];
						if (ret is PXResult)
						{
							ret = ((PXResult)ret)[0];
						}
						e.ReturnValue = _View.Cache.GetValue(ret, ((IBqlSearch)_SingleSelect).GetField().Name);
					}
				}
				else
				{
					e.ReturnValue = fetched[idx];
				}
			}
			if (e.ReturnValue is string)
			{
				if (state.DataType == typeof (bool))
				{
					int value;
					if (int.TryParse((string) e.ReturnValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out value))
					{
						e.ReturnValue = Convert.ToBoolean(value);
					}
				}
				if (state.DataType == typeof (DateTime))
				{
					DateTime d;
					if (DateTime.TryParse((string) e.ReturnValue, CultureInfo.InvariantCulture, DateTimeStyles.None, out d))
					{
						e.ReturnValue = d;
					}
				}
			}
		}

		protected virtual void AttributeFieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e, PXFieldState state, string attributeName, int iField)
		{
			if (!_IsActive) return;

			string newValue = e.NewValue as string;
			if (newValue != null)
			{
				if (state.DataType == typeof (bool))
				{
					bool value;
					if (bool.TryParse(newValue, out value))
					{
						e.NewValue = value;
					}
				}
				if (state.DataType == typeof (DateTime))
				{
					DateTime dt;
					if (DateTime.TryParse(newValue, CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
					{
						e.NewValue = dt;
					}
				}
			}
		}

		protected virtual void AttributeCommandPreparing(PXCache sender, PXCommandPreparingEventArgs e, PXFieldState state, string attributeName, int iField)
		{
			if (!_IsActive || (e.Operation & PXDBOperation.Command) != PXDBOperation.Select) return;
			
			if (!_BqlTable.IsAssignableFrom(sender.BqlTable))
			{
				if (sender.Graph.Caches[_BqlTable].BqlSelect != null &&
				    ((e.Operation & PXDBOperation.Option) == PXDBOperation.External ||
				     (e.Operation & PXDBOperation.Option) == PXDBOperation.Normal && e.Value == null))
				{
					e.Cancel = true;
					e.DataType = PXDbType.NVarChar;
					e.DataValue = e.Value;
					e.BqlTable = _BqlTable;
					e.Expr = new Column(state.Name, (e.Operation & PXDBOperation.Option) == PXDBOperation.External ? sender.GetItemType() : _BqlTable);
				}
				else
				{
					PXCommandPreparingEventArgs.FieldDescription description;
					e.Cancel = !sender.Graph.Caches[_BqlTable].RaiseCommandPreparing(state.Name, e.Row, e.Value, e.Operation, e.Table, out description);
					if (description != null)
					{
						e.DataType = description.DataType;
						e.DataValue = description.DataValue;
						e.BqlTable = _BqlTable;
						e.Expr = description.Expr;
					}
				}
			}
			else if (((e.Operation & PXDBOperation.Option) == PXDBOperation.External ||
			          (e.Operation & PXDBOperation.Option) == PXDBOperation.Normal && e.Value == null))
			{
				e.Cancel = true;
				e.DataValue = e.Value;
				string sValue = e.Value as string;
				if (state.DataType == typeof (bool))
				{
					e.DataType = PXDbType.Bit;

					bool value;
					if (sValue != null && bool.TryParse(sValue, out value))
					{
						e.DataValue = Convert.ToInt32(value).ToString(CultureInfo.InvariantCulture);
					}
				}
				else if (state.DataType == typeof (DateTime))
				{
					e.DataType = PXDbType.DateTime;
				    string dateFormat = "yyyy-MM-dd HH:mm:ss.fff";
					if (sValue != null && DateTime.TryParse(sValue, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
						e.DataValue = dt.ToString(dateFormat, CultureInfo.InvariantCulture);
				    if (e.Value is DateTime date)
				        e.DataValue = date.ToString(dateFormat, CultureInfo.InvariantCulture);
				}
				else
				{
					e.DataType = PXDbType.NVarChar;
				}

				List<Type> init = new List<Type>();
				if ((e.Operation & PXDBOperation.Option) == PXDBOperation.External) {
					init.Add(sender.GetItemType());
				}
				else if (e.Table != null) {
					init.Add(e.Table);
				}
				else {
					init.Add(_BqlTable);
				}


				if (aggregateAttributes && (e.Operation & PXDBOperation.Option) != PXDBOperation.External)
				{
					e.Expr = null;
				}
				else
				{
					SQLExpression fexp = BqlCommand.GetSingleExpression(((IBqlSearch)_SingleSelect).GetField(), sender.Graph, new List<Type>(_SingleSelect.GetTables()), null, BqlCommand.FieldPlace.Select);
					Query q = _SubSelect.GetQueryInternal(sender.Graph, new BqlCommandInfo(false) {Tables = init}, null);
					q.ClearSelection();
					q.Field(fexp);
					q.GetWhere().substituteConstant((string)(new AttributeIDPlaceholder().Value), attributeName);
					var where = q.GetWhere();
					var whereWithoutRestriction = where;
					TableChangingScope.AppendRestrictionsOnIsNew(ref where, sender.Graph, init, new BqlCommand.Selection());
					var isNewRestrictionsAdded = where != whereWithoutRestriction;
					q.Where(where);

					// Attribute subquery must use push notifications temp table only if outer table is also temp table.
					// To check that, we look if expression <attr_table>.IsNew... == <outer_table>.IsNew... was addded to where.
					var sqlTable = q.GetFrom()?.FirstOrDefault().Table() as SimpleTable;
					if (isNewRestrictionsAdded && sqlTable != null)
					{
						Func<Table> sqlTableGetter = () => sqlTable;
						TableChangingScope.AddUnchangedRealName(sqlTable.Name);
						q.GetFrom()[0].setTable(TableChangingScope.GetSQLTable(sqlTableGetter, sqlTable.Name));
					}

					var stringState = state as PXStringState;
					if (state?.DataType == typeof(bool)) {
						e.Expr = new SubQuery(q).Coalesce(new SQLConst(0));
					}
					else {
						e.Expr = new SubQuery(q);
					}

                }
				e.BqlTable = _BqlTable;
			}
			else
			{
				e.Expr = null;
			}
		}

		public override void RowSelecting(PXCache sender, PXRowSelectingEventArgs e)
		{
			if (!_IsActive || _Fields == null) return;
			string firstColumn = e.Record.GetString(e.Position);
			var sqlDialect = sender.Graph.SqlDialect;

			string[] fetched;
			if(aggregateAttributes && sqlDialect.tryExtractAttributes(firstColumn, _AttributeIndices, out fetched))
			{
				sender.SetValue(e.Row, _FieldOrdinal, fetched);
				e.Position++;
				return;
			}

			int? nbr = e.Record.GetInt32(e.Position);
			e.Position++;
			if (nbr != null)
			{
				if (nbr > 0)
				{
					fetched = new string[_Fields.Length];
					sender.SetValue(e.Row, _FieldOrdinal, fetched);
					for (int i = 0; i < fetched.Length; i++)
					{
						fetched[i] = e.Record.GetString(e.Position);
						e.Position++;
					}
				}
			}
			else
			{
				e.Position += _Fields.Length;
			}
		}

		public override void CommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
		{
			if (!_IsActive || _Fields == null || !e.IsSelect()) return;

			PXDBOperation eOp = e.Operation & PXDBOperation.Option;
			bool operationAllowsSelectForXml = 
				eOp == PXDBOperation.External 
				|| eOp == PXDBOperation.Internal 
				|| eOp == PXDBOperation.Normal && e.Value == null
				|| eOp == PXDBOperation.GroupBy && sender.BqlSelect != null; // it's ok because the subselect will be used inside FROM (...) part without any aggregation
			Type tableToUse = eOp == PXDBOperation.External ? sender.GetItemType() : e.Table ?? _BqlTable;
			
			if (!_BqlTable.IsAssignableFrom(sender.BqlTable))
			{
				if (sender.Graph.Caches[_BqlTable].BqlSelect != null && operationAllowsSelectForXml)
				{
					e.BqlTable = _BqlTable;
					e.Expr = new Column(_DatabaseFieldName, (e.Operation & PXDBOperation.Option) == PXDBOperation.External ? sender.GetItemType() : _BqlTable);
				}
				else
				{
					PXCommandPreparingEventArgs.FieldDescription description;
					sender.Graph.Caches[_BqlTable].RaiseCommandPreparing(_DatabaseFieldName, e.Row, e.Value, e.Operation, e.Table, out description);
					if (description != null)
					{
						e.DataType = description.DataType;
						e.DataValue = description.DataValue;
						e.BqlTable = _BqlTable;
						e.Expr = description.Expr;
					}
				}
			}
			else
			{
				var dialect = e.SqlDialect;
				if (operationAllowsSelectForXml) {
					if (aggregateAttributes && (e.Operation & PXDBOperation.Option) != PXDBOperation.External) {
						List<Type> init = new List<Type>() { tableToUse };

						e.BqlTable = tableToUse;
						e.DataType = PXDbType.NVarChar;

						e.Expr = new SQLTree.SubQuery(GetAttributesJoinedQuery(sender.Graph, init, ((IBqlSearch)_SingleSelect).GetField(), _PureWhere));
						return;
					}
					else {
						e.BqlTable = _BqlTable;

						Query q = new Query().Field(new SQLConst(_Fields.Length));
						e.Expr = new SubQuery(q);
					}
				}
				else {
					e.Expr = SQLExpression.Null();
				}
			}
			e.DataType = PXDbType.Int;
			e.DataLength = 4;
		}

		protected SQLTree.Query GetAttributesJoinedQuery(PXGraph graph, List<Type> types, Type fieldWithValue, IBqlWhere _PureWhere) {
			Type table0 = BqlCommand.GetItemType(fieldWithValue);
			Type table = BqlCommand.FindRealTableForType(types, table0);
			SQLTree.SQLExpression exp = null;
			_PureWhere.AppendExpression(ref exp, graph, new BqlCommandInfo(false) {Tables = types}, null);

			var tables = types.ToList();
			var useRealTables = types.All(t => graph.Caches[t].BqlSelect == null);
			tables.Add(useRealTables ? table : table0);
			var where = exp;
			TableChangingScope.AppendRestrictionsOnIsNew(ref where, graph, tables, new BqlCommand.Selection(), useRealTables);
			var isNewRestrictionsAdded = where != exp;

			Table sqlTable;
			Func<Table> sqlTableGetter = () => new SimpleTable(table.Name);
			// Attribute subquery must use push notifications temp table only if outer table is also temp table.
			// To check that we look if expression <attr_table>.IsNew... == <outer_table>.IsNew... was addded to where.
			if (isNewRestrictionsAdded)
			{
				TableChangingScope.AddUnchangedRealName(table.Name);
				sqlTable = TableChangingScope.GetSQLTable(sqlTableGetter, table.Name);
			}
			else
			{
				sqlTable = sqlTableGetter();
			}			

			return new SQLTree.JoinedAttrQuery(sqlTable, fieldWithValue.Name, "AttributeID", "RefNoteID", where);
		}

		protected Type GetBqlTable(Type table)
		{
			while (typeof(IBqlTable).IsAssignableFrom(table.BaseType)
			       && !table.IsDefined(typeof(PXTableAttribute), false)
			       && (!table.IsDefined(typeof(PXTableNameAttribute), false) || !((PXTableNameAttribute)table.GetCustomAttributes(typeof(PXTableNameAttribute), false)[0]).IsActive))
			{
				table = table.BaseType;
			}
			return table;
		}
		public override void CacheAttached(PXCache sender)
		{
			if (sender.Graph.GetType() == typeof(AttrGraph)) return;

			if (sender.Graph.GetType() == typeof(PXGraph) || sender.Graph.GetType() == typeof(PX.Data.Maintenance.GI.GenericInquiryDesigner))
			{
				_IsActive = true;
			}
			base.CacheAttached(sender);

			InitializeFields(sender);
			
			Type selectType = _SingleSelect.GetType();
			Type itemType = sender.GetItemType();
			while (itemType != typeof(object) && selectType == _SingleSelect.GetType())
			{
				selectType = BqlCommand.Parametrize(itemType, selectType);
				itemType = itemType.BaseType;
			}
			_View = new PXView(sender.Graph, true, BqlCommand.CreateInstance(selectType));
		}

		private List<Type> GetTables(PXCache sender)
		{
			List<Type> tables = new List<Type>();
			Type cacheType = _SingleSelect.GetTables()[0];
			if (_Selector == null)
			{
				PXFieldState state = sender.Graph.Caches[cacheType].GetStateExt(null, _Field.Name) as PXFieldState;
				if (state != null && !String.IsNullOrEmpty(state.ViewName))
				{
					foreach (Type t in sender.Graph.Views[state.ViewName].BqlSelect.GetTables())
					{
						Type bt = GetBqlTable(t);
						if (!tables.Contains(bt))
						{
							tables.Add(bt);
						}
					}
				}
			}
			else
			{
				foreach (Type t in _Selector.GetTables())
				{
					Type bt = GetBqlTable(t);
					if (!tables.Contains(bt))
					{
						tables.Add(bt);
					}
				}
			}
			return tables;
		}

		protected virtual void InitializeFields(PXCache sender)
		{
			Type cacheType = _SingleSelect.GetTables()[0];
			var tables = GetTables(sender);

			_Fields = GetSlot(_FieldName + '_' + sender.GetItemType().FullName + '_' + System.Threading.Thread.CurrentThread.CurrentUICulture.Name,
				new DefinitionParams(sender, cacheType, _Field, _Selector, _DescriptionField, _ControlTypeField, _EntryMaskField, _ListField, _FieldName),
				tables.ToArray());

			if (_Fields != null)
			{
				var ai = new Dictionary<string, int>();
				for (int i = 0; i < _Fields.Length; i++)
				{
					string key = _Fields[i].Name;
					int idxUnderscore = key.IndexOf('_');
					ai.Add(idxUnderscore > 0 ? key.Substring(0, idxUnderscore) : key, i);
				}
				_AttributeIndices = ai;

				int attributesIdx = sender.Fields.IndexOf(_FieldName);
				for (int i = 0; i < _Fields.Length; i++)
				{
					int idx = i;
					PXFieldState field = (PXFieldState)((ICloneable)_Fields[idx]).Clone();
					field.Visible = DefaultVisible;
					//field.Visibility = PXUIVisibility.Invisible;
					sender.Fields.Insert(++attributesIdx, field.Name);
					string name = field.Name.Substring(0, field.Name.Length - _FieldName.Length - 1);
					sender.Graph.FieldSelecting.AddHandler(sender.GetItemType(), field.Name, (c, a) => AttributeFieldSelecting(c, a, field, name, idx));
					sender.Graph.FieldUpdating.AddHandler(sender.GetItemType(), field.Name, (c, a) => AttributeFieldUpdating(c, a, field, name, idx));
					sender.Graph.CommandPreparing.AddHandler(sender.GetItemType(), field.Name, (c, a) => AttributeCommandPreparing(c, a, field, name, idx));
				}
			}
		}

		protected internal override void SetBqlTable(Type bqlTable)
		{
			base.SetBqlTable(bqlTable);
			_BqlTablesUsed.AddOrUpdate(_BqlTable, _BqlTable, (t1, t2) => t1);
		}
		internal static ConcurrentDictionary<Type, Type> _BqlTablesUsed = new ConcurrentDictionary<Type, Type>();

		protected virtual PXFieldState[] GetSlot(string name, DefinitionParams definitionParams, Type[] tables)
		{
			Definition def = PXDatabase.GetSlot<Definition, DefinitionParams>(
				name, definitionParams, tables);
			return def == null ? null : def.Fields;
		}

        /// <exclude/>
		private class AttrGraph : PXGraph // marker type for cache instantiation - PXDBAttributeAttribute.CacheAttached is skipped for this type
		{
		}
	}
}