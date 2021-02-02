using PX.Data;
using PX.Data.SQLTree;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PX.Objects.PR
{
	public enum RequiredMode
	{
		Default = 0,
		Never = 1
	}

	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
	public class UseDefaultValueAttribute : PXEventSubscriberAttribute, IPXFieldSelectingSubscriber, IPXRowPersistingSubscriber, IPXRowSelectedSubscriber, IPXCommandPreparingSubscriber
	{
		public const string _SubSelect = "(SELECT ";
		protected bool _HasUIEnabledAttribute;
		protected bool _HasUIRequiredAttribute;
		protected Type _SelectorField;
		protected Type _DefaultFieldCacheType;
		protected Type _DefaultField;
		protected Type _UseDefaultField;
		protected RequiredMode _Required = RequiredMode.Default;

		public UseDefaultValueAttribute(Type selectorField, Type defaultField, Type useDefaultField)
		{
			_SelectorField = selectorField;
			_DefaultFieldCacheType = BqlCommand.GetItemType(defaultField);
			_DefaultField = defaultField;
			_UseDefaultField = useDefaultField;
		}

		public RequiredMode Required
		{
			get
			{
				return _Required;
			}
			set
			{
				_Required = value;
			}
		}

		public bool ManageUIEnabled { get; set; } = true;
		public bool ManageErrorLevel { get; set; } = true;

		public override void CacheAttached(PXCache sender)
		{
			_HasUIEnabledAttribute = sender.GetAttributesReadonly(_FieldName).OfType<PXUIEnabledAttribute>().FirstOrDefault() != null;
			_HasUIRequiredAttribute = sender.GetAttributesReadonly(_FieldName).OfType<PXUIRequiredAttribute>().FirstOrDefault() != null;

			PXDefaultAttribute defAttr = sender.GetAttributesReadonly(_FieldName).OfType<PXDefaultAttribute>().FirstOrDefault();
			if (defAttr != null)
			{
				defAttr.PersistingCheck = PXPersistingCheck.Nothing;
			}
			else
			{
				throw new PXException(ErrorMessages.UsageOfAttributeWithoutPrerequisites, GetType().Name, typeof(PXDefaultAttribute).Name, sender.GetItemType().FullName, FieldName);
			}

			base.CacheAttached(sender);
		}

		public void RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			if (e.Row == null || !ManageUIEnabled)
				return;

			bool? useDefault = (bool?)sender.GetValue(e.Row, _UseDefaultField.Name);
			if (useDefault == true)
			{
				PXUIFieldAttribute.SetEnabled(sender, e.Row, _FieldName, false);
			}
			else
			{
				// We forcibly enable the field ONLY if it's not used in conjunction with PXUIEnabledAttribute
				if (!_HasUIEnabledAttribute)
				{
					PXUIFieldAttribute.SetEnabled(sender, e.Row, _FieldName, true);
				}
			}

		}

		public virtual void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			if (e.Row == null)
				return;

			object value = e.ReturnState;

			bool? useDefault = (bool?)sender.GetValue(e.Row, _UseDefaultField.Name);
			if (useDefault == true)
			{
				PXErrorLevel initialErrorLevel = PXErrorLevel.Undefined;
				string initialErrorMsg = string.Empty;
				if (value is PXFieldState initialFieldState && !ManageErrorLevel)
				{
					initialErrorLevel = initialFieldState.ErrorLevel;
					initialErrorMsg = initialFieldState.Error;
				}

				object defaultRow = GetDefaultRow(sender, e.Row);
				if (defaultRow != null)
				{
					PXCache defaultRowCache = sender.Graph.Caches[_DefaultFieldCacheType];
					value = defaultRowCache.GetValue(defaultRow, _DefaultField.Name);
					sender.SetValue(e.Row, this.FieldName, value);
					defaultRowCache.RaiseFieldSelecting(_DefaultField.Name, defaultRow, ref value, true);
					((PXFieldState)value).Enabled = false;
				}

				if (!ManageErrorLevel && value is PXFieldState finalFieldState && initialErrorLevel >= finalFieldState.ErrorLevel)
				{
					finalFieldState.ErrorLevel = initialErrorLevel;
					finalFieldState.Error = initialErrorMsg;
				}
			}

			e.ReturnState = value;
		}

		protected object GetDefaultRow(PXCache sender, object row)
		{
			return PXSelectorAttribute.Select(sender, row, _SelectorField.Name);
		}

		protected PXPersistingCheck GetPersistingCheck(PXCache cache, object row)
		{
			bool? useDefault = (bool?)cache.GetValue(row, _UseDefaultField.Name);
			if (useDefault == false && _Required == RequiredMode.Default)
			{
				return PXPersistingCheck.NullOrBlank;
			}
			else
			{
				return PXPersistingCheck.Nothing;
			}
		}

		public virtual void RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			if (_HasUIRequiredAttribute)
				return;

			PXDefaultAttribute defaultAttribute = sender.GetAttributesReadonly(_FieldName).OfType<PXDefaultAttribute>().FirstOrDefault();
			if (defaultAttribute != null)
			{
				defaultAttribute.PersistingCheck = GetPersistingCheck(sender, e.Row);
				defaultAttribute.RowPersisting(sender, e);
				defaultAttribute.PersistingCheck = PXPersistingCheck.Nothing;
			}
		}

		public virtual void CommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
		{
			// Scalars are not supported inside of aggregates, so this logic will only be executed for normal, selects (not inside GROUP BY)
			if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Select && (e.Operation & PXDBOperation.GroupBy) == 0)
			{
				/*
                This code is adapted from PXSelectorAttribute::DescriptionFieldSelecting.
                 
                Ideally, we should run this logic *only* when the field is part of a where code. 
                From UI the FieldSelecting code will achieve the same result, and in a much simpler way.
                AndrewB says something new is coming in relation with KvExt tables that would allow that.
                */

				PXDBFieldAttribute attr = sender.GetAttributesReadonly(_FieldName).OfType<PXDBFieldAttribute>().FirstOrDefault();
				if (attr == null)
					return;

				PXSelectorAttribute selectorAttribute = sender.GetAttributesReadonly(_SelectorField.Name).OfType<PXSelectorAttribute>().FirstOrDefault();
				Type selectorTable = selectorAttribute.PrimarySelect.GetFirstTable();
				string extTable = selectorTable.Name + "Ext";
				string defaultFieldName = _DefaultField.Name;
				string tableName = BqlCommand.GetTableName(selectorTable);

				if (!String.IsNullOrEmpty(tableName) && !tableName.Trim().StartsWith(_SubSelect)) // do not expand description field to subselect in case when description field is being selected from another expanded subselect
				{
					PXCommandPreparingEventArgs.FieldDescription defaultFieldDescription;
					Type defaultTable = BqlCommand.GetItemType(_DefaultField);
					PXCache defaultCache = sender.Graph.Caches[defaultTable];
					PXDBOperation operation = PXDBOperation.Select;
					if (PXDBLocalizableStringAttribute.IsEnabled && defaultCache.GetAttributes(_DefaultField.Name).Any(_ => _ is PXDBLocalizableStringAttribute))
					{
						operation |= PXDBOperation.External;
					}
					defaultCache.RaiseCommandPreparing(_DefaultField.Name, null, null, operation, defaultTable, out defaultFieldDescription);
					if (defaultFieldDescription != null && defaultFieldDescription.Expr != null)
					{
						defaultFieldName = defaultFieldDescription.Expr.AliasOrName();
					}
				}

				Type bqlTable = sender.GetItemType();
				var innerQuery = new Query().Field(new Column(defaultFieldName, extTable))
					.From(new SimpleTable(tableName, extTable))
					.Where(new Column(((IBqlSearch)selectorAttribute.PrimarySelect).GetField().Name, extTable).EQ(new Column(_SelectorField.Name, e.Table == null ? bqlTable.Name : e.Table.Name)));
				var thenExpr = new SubQuery(innerQuery);

				string whenTable = e.Table == null ? bqlTable.Name : e.Table.Name;
				SQLExpression whenExpr = new Column(_UseDefaultField.Name, whenTable).EQ(1);
				SQLExpression elseExpr = BuildElseExpression(bqlTable);

				var switchExpr = new SQLSwitch().Case(whenExpr, thenExpr).Default(elseExpr);
				Query q = new Query().Field(switchExpr);
				e.Expr = new SubQuery(q);
				e.Cancel = true;
				e.BqlTable = bqlTable;
			}
		}

		protected virtual SQLExpression BuildElseExpression(Type bqlTable)
		{
			return new Column(_FieldName, bqlTable.Name);
		}
	}

	/// <summary>
	/// Fetches value from a "default" source or from a "else" source, depending on flag value. This attribute can fetch values from a different DAC/Table than the one where this attribute is applied.
	/// </summary>
	public class UseDefaultValueFromSourceAttribute : UseDefaultValueAttribute
	{
		protected Type[] _DacKeys;
		protected Type[] _ElseSourceKeys;

		/// <param name="selectorField">Selector field to use as the source of the "default" value if the <paramref name="useDefaultField"/> flag value is true.</param>
		/// <param name="defaultField">Value to use from the <paramref name="selectorField"/> source</param>
		/// <param name="useDefaultField">DAC field which value act as a flag indicating if the value is fetched from the <paramref name="selectorField"/> source or the "else" source. </param>
		/// <param name="dacKeys">Array of fields found on the DAC in which this attribute is applied. These fields will be used in the "Where" clause of the "Else" statement</param>
		/// <param name="elseSourceKeys">Array of fields found on the foreign table that will match those defined in <paramref name="dacKeys"/>. Matching keys need to be on the same array index.</param>
		/// <example>
		/// <code>
		/// [UseDefaultValueFromSource(typeof(PREmployee.employeeClassID), typeof(PREmployeeClass.unionID), typeof(PREmployee.unionUseDflt),
		/// 	new[] { typeof(PREmployee.bAccountID) }, 
		/// 	new[] { typeof(EPEmployee.bAccountID) }, Required = RequiredMode.Never)]
		/// </code>
		/// will lead to SQL similar to this :
		/// SELECT CASE WHEN [PREmployee].[unionUseDflt] = 1 
		/// THEN (SELECT [PREmployeeClassExt].[UnionID] FROM [PREmployeeClass] [PREmployeeClassExt] WHERE [PREmployeeClassExt].[employeeClassID] = [PREmployee].[employeeClassID]) 
		/// ELSE (SELECT [EPEmployee].[UnionID] FROM [EPEmployee] [EPEmployee] WHERE [EPEmployee].[bAccountID] = [PREmployee].[bAccountID]) END
		/// </example>
		public UseDefaultValueFromSourceAttribute(Type selectorField, Type defaultField, Type useDefaultField, Type[] dacKeys, Type[] elseSourceKeys) : base(selectorField, defaultField, useDefaultField)
		{
			_DacKeys = dacKeys;
			_ElseSourceKeys = elseSourceKeys;
		}

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);
			sender.Graph.OnBeforePersist += PrePersist;
		}

		/// <summary>
		/// If the "Use Default" flag is true, prevent overwrite of value already in database.
		/// </summary>
		private void PrePersist(PXGraph currentGraph)
		{
			PXCache currentCache = currentGraph.Caches[this.BqlTable];
			object currentRow = currentCache.Current;
			bool? useDefault = (bool?)currentCache.GetValue(currentRow, _UseDefaultField.Name);
			if (useDefault == true)
			{
				BqlCommand bql = BqlCommand.CreateInstance(BqlCommand.Compose(typeof(Select<>), _ElseSourceKeys[0].DeclaringType));
				var parameters = new List<object>();
				for (int i = 0; i < _ElseSourceKeys.Length; i++)
				{
					Type elseKey = _ElseSourceKeys[i];
					bql = bql.WhereAnd(BqlCommand.Compose(typeof(Where<,>), elseKey, typeof(Equal<>), typeof(Required<>), elseKey));
					parameters.Add(currentCache.GetValue(currentRow, _DacKeys[i].Name));
				}

				var readOnlyGraph = PXGraph.CreateInstance<PXGraph>();
				PXCache originalRowCache = readOnlyGraph.Caches[_ElseSourceKeys[0].DeclaringType];
				var view = new PXView(readOnlyGraph, true, bql);
				object originalRow = view.SelectSingle(parameters.ToArray());
				if (originalRow != null)
				{
					object value = originalRowCache.GetValue(originalRow, this.FieldName);
					currentCache.SetValue(currentRow, this.FieldName, value);
				}
			}
		}

		protected override SQLExpression BuildElseExpression(Type bqlTable)
		{
			if (_DacKeys.Length == 0 || _DacKeys.Length != _ElseSourceKeys.Length)
			{
				throw new PXException(Messages.AttributeKeysInvalid, this.GetType().Name, nameof(_DacKeys), nameof(_ElseSourceKeys));
			}

			var elseSourceTable = _ElseSourceKeys[0].DeclaringType;
			SQLExpression whereQuery = null;
			int i = 0;
			do
			{
				var dacKey = _DacKeys[i];
				var elseSourceKey = _ElseSourceKeys[i];
				if (i == 0)
				{
					whereQuery = new Column(elseSourceKey.Name, elseSourceTable).EQ(new Column(dacKey.Name, bqlTable));
				}
				else
				{
					whereQuery = whereQuery.And(new Column(elseSourceKey.Name, elseSourceTable).EQ(new Column(dacKey.Name, bqlTable)));
				}

				i++;
			}
			while (i < _DacKeys.Length);

			var elseQuery = new SubQuery(new Query().Field(new Column(_FieldName, elseSourceTable.Name))
				.From(new SimpleTable(elseSourceTable.Name))
				.Where(whereQuery));

			return elseQuery;
		}
	}
}
