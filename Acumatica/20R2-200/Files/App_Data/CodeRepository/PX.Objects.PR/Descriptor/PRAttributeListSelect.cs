using PX.Api.Payroll;
using PX.Common;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Payroll;
using PX.Payroll.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PX.Objects.PR
{
	public class MetaTypeSettingCollection<TDynamicType, TTypeMeta> : Dictionary<string, PRSettingDefinition<TDynamicType, TTypeMeta>>
		where TDynamicType : DynamicEntity<TDynamicType, TTypeMeta>
		where TTypeMeta : SettingDefinitionListAttribute
	{ }
	public class MetaTypeSettingCache<TDynamicType, TTypeMeta> : Dictionary<string, MetaTypeSettingCollection<TDynamicType, TTypeMeta>>
		where TDynamicType : DynamicEntity<TDynamicType, TTypeMeta>
		where TTypeMeta : SettingDefinitionListAttribute
	{ }

	public abstract class PRAttributeSelectBase<TEntity, TSelect, TDynamicType, TTypeMeta> : PXSelectBase<TEntity>
		where TEntity : class, IBqlTable, IPRSetting, new()
		where TSelect : class, IBqlSelect, new()
		where TDynamicType : DynamicEntity<TDynamicType, TTypeMeta>
		where TTypeMeta : SettingDefinitionListAttribute
	{
		protected virtual MetaTypeSettingCollection<TDynamicType, TTypeMeta> SettingDefinitions
		{
			get
			{
				MetaTypeSettingCache<TDynamicType, TTypeMeta> metaCache = PXContext.GetSlot<MetaTypeSettingCache<TDynamicType, TTypeMeta>>() ?? CacheMetaSettings();

				var settingDictionary = new MetaTypeSettingCollection<TDynamicType, TTypeMeta>();
				foreach (IPRReflectiveSettingMapper<TTypeMeta> settingMapper in GetMetaTypes())
				{
					if (!metaCache.ContainsKey(settingMapper.ReflectiveUniqueCode))
					{
						metaCache = CacheMetaSettings();
					}
					settingDictionary.AddRange(metaCache[settingMapper.ReflectiveUniqueCode]);
				}
				return settingDictionary;
			}
		}

		public PRAttributeSelectBase(PXGraph graph)
		{
			_Graph = graph;

			var command = BqlCommand.CreateInstance(typeof(TSelect));
			View = new PXView(graph, false, command, new PXSelectDelegate(viewHandler));
			_Graph.FieldSelecting.AddHandler(typeof(TEntity), "Value", ValueFieldSelecting);
			_Graph.FieldSelecting.AddHandler(typeof(TEntity), "Description", DescriptionFieldSelecting);
			Cache.AllowInsert = _Graph.IsImport;
			Cache.AllowDelete = false;
		}

		protected IEnumerable viewHandler()
		{
			IEnumerable<TEntity> savedValuesList = GetSavedValues();
			IEnumerable<IPRSetting> definitionList = GetAttributeDefinitionList().ToList();

			bool found = false;

			//Delete Deprecated records
			foreach (TEntity savedValue in savedValuesList)
			{
				found = false;

				foreach (IPRSetting definition in definitionList)
				{
					if (savedValue.TypeName == definition.TypeName && savedValue.SettingName == definition.SettingName)
					{
						found = true;
						break;
					}
				}

				if (!found)
				{
					this.Delete(savedValue);
				}
			}

			foreach (IPRSetting definition in definitionList)
			{
				//First look for this attribute in the cache - we can't blindly return all
				//values from Cache.Cached since the PRAttributeListSelect can be used
				//in master-detail-detail scenarios (ex: Employee Payroll Setting -> Tax Codes -> Attributes)
				TEntity cachedValue = (TEntity)Cache.CreateInstance();
				foreach (string key in Cache.Keys)
				{
					//This will take care of setting the values for other keys of this TEntity (ex: BAccountID, TaxID for PREmployeeTaxAttribute)
					Cache.SetDefaultExt(cachedValue, key);
				}
				cachedValue.TypeName = definition.TypeName;
				cachedValue.SettingName = definition.SettingName;
				cachedValue = (TEntity)Cache.Locate(cachedValue);

				if (cachedValue != null)
				{
					//Item was found in cache; make sure to set it to Held status again (this will happen after we persist changes)
					if (Cache.GetStatus(cachedValue) == PXEntryStatus.Notchanged || Cache.GetStatus(cachedValue) == PXEntryStatus.Deleted)
					{
						Cache.SetStatus(cachedValue, PXEntryStatus.Held);
					}

					yield return cachedValue;
					continue;
				}

				found = false;

				//If not found in cache, look in saved values
				foreach (TEntity savedValue in savedValuesList)
				{
					if (savedValue.TypeName == definition.TypeName && savedValue.SettingName == definition.SettingName)
					{
						found = true;

						//Return from DB saved values
						savedValue.Required = savedValue.Required ?? definition.Required;
						savedValue.AllowOverride = savedValue.AllowOverride ?? definition.AllowOverride;
						bool setUpdatedStatus = AssignDefault(savedValue, definition);
						savedValue.SortOrder = savedValue.SortOrder ?? definition.SortOrder;
						if (setUpdatedStatus)
						{
							Cache.SetStatus(savedValue, PXEntryStatus.Updated);
							Cache.IsDirty = true;
						}
						else
						{
							Cache.SetStatus(savedValue, PXEntryStatus.Held);
						}
						yield return savedValue;
						break;
					}
				}

				if (!found)
				{
					// Nothing was found, just insert new row based on current attribute definition
					dynamic dynamicDefinition = definition;
					yield return this.Insert(CreateDacRecord(Cache, dynamicDefinition));
				}
			}
		}

		private IEnumerable<TEntity> GetSavedValues()
		{
			List<TEntity> cachedRecords = PXContext.GetSlot<List<TEntity>>();
			if (cachedRecords == null || !cachedRecords.Any())
			{
				cachedRecords = PXContext.SetSlot(SelectFrom<TEntity>.View.Select(_Graph).FirstTableItems.ToList());
			}

			PXView view = new PXView(_Graph, true, BqlCommand.CreateInstance(typeof(TSelect)));
			object[] queryParameters = view.PrepareParameters(null, null);
			return cachedRecords.Where(x => view.BqlSelect.Meet(_Graph.Caches[typeof(TEntity)], x, queryParameters));
		}

		private TEntity CreateDacRecord(PXCache cache, IPRSetting definition)
		{
			var attr = (TEntity)cache.CreateInstance();
			attr.TypeName = definition.TypeName;
			attr.SettingName = definition.SettingName;
			attr.AllowOverride = definition.AllowOverride;
			attr.Required = definition.Required;
			attr.UseDefault = (definition.AllowOverride == false);
			attr.Value = (attr.UseDefault == false ? definition.Value : null);
			attr.SortOrder = definition.SortOrder;
			attr.AatrixMapping = definition.AatrixMapping;

			if (definition is IStateSpecific)
			{
				cache.SetValue(attr, "State", ((IStateSpecific)definition).State);
			}

			return attr;
		}

		private TEntity CreateDacRecord(PXCache cache, PRSettingDefinition<TDynamicType, TTypeMeta> definition)
		{
			TEntity attr = CreateDacRecord(cache, (IPRSetting)definition);
			definition.AssignSpecificDacFields(cache, attr);
			attr.Value = definition.ControlType == SettingControlType.CheckBox ?
				definition.Value ?? false.ToString() :
				definition.Value;
			return attr;
		}

		protected virtual void ValueFieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			var row = e.Row as TEntity;
			if (e.Row == null)
				return;

			const int answerValueLength = 60;

			PRSettingDefinition<TDynamicType, TTypeMeta> settingDefinition;
			if (SettingDefinitions.TryGetValue(row.SettingName, out settingDefinition))
			{
				bool required = AttributeValueIsRequired(row, settingDefinition.ControlType);
				string errorMsg = null;
				PXErrorLevel errorLevel = PXErrorLevel.Undefined;
				if (required && e.ReturnValue == null)
				{
					errorMsg = InvalidAttributeErrorMessage;
					errorLevel = PXErrorLevel.RowError;
				}
				else if (sender.GetStatus(row) == PXEntryStatus.Inserted)
				{
					errorMsg = Messages.NewTaxSetting;
					errorLevel = PXErrorLevel.RowWarning;
				}

				if (settingDefinition.ControlType == SettingControlType.Combo)
				{
					List<string> allowedValues = new List<string>();
					List<string> allowedLabels = new List<string>();

					foreach (var option in settingDefinition.ComboListItems)
					{
						allowedValues.Add(option.Value);
						allowedLabels.Add(option.DisplayName);
					}

					e.ReturnState = PXStringState.CreateInstance(e.ReturnValue, PRAttributeDetail.ParameterIdLength,
						true, nameof(row.Value), false, required ? 1 : -1, settingDefinition.EntryMask, allowedValues.ToArray(), allowedLabels.ToArray(),
						true, null);
					var stringState = e.ReturnState as PXStringState;
					stringState.Error = errorMsg;
					stringState.ErrorLevel = errorLevel;
				}
				else if (settingDefinition.ControlType == SettingControlType.CheckBox)
				{
					e.ReturnValue = e.ReturnValue ?? false;
					e.ReturnState = PXFieldState.CreateInstance(e.ReturnValue, typeof(bool), false, false, required ? 1 : -1,
						null, null, false, nameof(row.Value), null, null, errorMsg, errorLevel, true, true,
						null, PXUIVisibility.Visible, null, null, null);
				}
				else if (settingDefinition.ControlType == SettingControlType.Datetime)
				{
					e.ReturnState = PXDateState.CreateInstance(e.ReturnValue, nameof(row.Value), false, required ? 1 : -1,
						settingDefinition.EntryMask, settingDefinition.EntryMask, null, null);
					var dateState = e.ReturnState as PXDateState;
					dateState.Error = errorMsg;
					dateState.ErrorLevel = errorLevel;
				}
				else if (settingDefinition.ControlType == SettingControlType.Decimal)
				{
					decimal currentValue;
					if (decimal.TryParse(e.ReturnState as string, out currentValue))
						e.ReturnState = (decimal?)currentValue;

					e.ReturnState = PXFieldState.CreateInstance(e.ReturnValue, typeof(decimal), false, false, required ? 1 : -1,
						settingDefinition.Precision, null, false, nameof(row.Value), null, null, errorMsg, errorLevel, true, true,
						null, PXUIVisibility.Visible, null, null, null);
				}
				else
				{
					//TextBox
					e.ReturnState = PXStringState.CreateInstance(e.ReturnValue, answerValueLength, null,
						nameof(row.Value), false, required ? 1 : -1, settingDefinition.EntryMask, null, null, true, null);
					var stringState = e.ReturnState as PXStringState;
					stringState.Error = errorMsg;
					stringState.ErrorLevel = errorLevel;
				}

				SetErrorLevel(sender, row, errorLevel);
			}
		}

		protected virtual void DescriptionFieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			if (e.Row == null)
				return;

			string settingName = (string)sender.GetValue(e.Row, "SettingName") ?? string.Empty;

			PRSettingDefinition<TDynamicType, TTypeMeta> settingDefinition;
			if (SettingDefinitions.TryGetValue(settingName, out settingDefinition))
			{
				e.ReturnValue = settingDefinition.Description;
			}
		}

		protected virtual void SetErrorLevel(PXCache cache, TEntity row, PXErrorLevel errorLevel)
		{
			if (_Graph.IsImport)
			{
				return;
			}

			TEntity originalRow = (TEntity)Cache.CreateCopy(row);
			cache.SetValue(row, nameof(row.ErrorLevel), (int?)errorLevel);
			// To update parent through PXFormula
			if (row.ErrorLevel != originalRow.ErrorLevel)
			{
				cache.RaiseRowUpdated(row, originalRow); 
			}
		}

		protected abstract IEnumerable<IPRSetting> GetAttributeDefinitionList();

		protected abstract IEnumerable<IPRReflectiveSettingMapper<TTypeMeta>> GetMetaTypes();
		protected abstract IEnumerable<IPRReflectiveSettingMapper<TTypeMeta>> GetAllMetaTypes();

		protected virtual MetaTypeSettingCache<TDynamicType, TTypeMeta> CacheMetaSettings()
		{
			List<IPRReflectiveSettingMapper<TTypeMeta>> allMetaTypes = GetAllMetaTypes().ToList();
			MetaTypeSettingCache<TDynamicType, TTypeMeta> metaCache = new MetaTypeSettingCache<TDynamicType, TTypeMeta>();
			foreach (KeyValuePair<IPRReflectiveSettingMapper<TTypeMeta>, DynamicEntity<TDynamicType, TTypeMeta>.MetaWithSetting> settingMeta in
				GetMetaSettings(allMetaTypes))
			{
				metaCache[settingMeta.Key.ReflectiveUniqueCode] = PRSettingDefinition<TDynamicType, TTypeMeta>.Convert(settingMeta.Value);
			}

			PXContext.SetSlot(metaCache);
			return metaCache;
		}

		protected abstract bool AssignDefault(TEntity record, IPRSetting definition);

		protected virtual bool AttributeValueIsRequired(TEntity attribute)
		{
			if (SettingDefinitions.TryGetValue(attribute.SettingName, out PRSettingDefinition<TDynamicType, TTypeMeta> settingDefinition))
			{
				return AttributeValueIsRequired(attribute, settingDefinition.ControlType);
			}
			return false;
		}

		protected abstract bool AttributeValueIsRequired(TEntity attribute, SettingControlType controlType);

		protected abstract string InvalidAttributeErrorMessage { get; }

		protected virtual Dictionary<IPRReflectiveSettingMapper<TTypeMeta>, DynamicEntity<TDynamicType, TTypeMeta>.MetaWithSetting> GetMetaSettings(IEnumerable<IPRReflectiveSettingMapper<TTypeMeta>> typeSettings)
		{
			using (var payrollAssemblyScope = new PXPayrollAssemblyScope<PayrollMetaProxy>())
			{
				return payrollAssemblyScope.Proxy.GetMetaWithSettings<TDynamicType, TTypeMeta>(typeSettings.ToArray());
			}
		}
	}

	public abstract class PRAttributeValuesSelectBase<TEntity, TSelect, TDefinitionEntity, TDefinitionSelect, TDynamicType, TTypeMeta> : PRAttributeSelectBase<TEntity, TSelect, TDynamicType, TTypeMeta>
		where TEntity : class, IBqlTable, IPRSetting, new()
		where TSelect : class, IBqlSelect, new()
		where TDefinitionEntity : class, IBqlTable, IPRSetting, new()
		where TDefinitionSelect : class, IBqlSelect, new()
		where TDynamicType : DynamicEntity<TDynamicType, TTypeMeta>
		where TTypeMeta : SettingDefinitionListAttribute
	{
		public PRAttributeValuesSelectBase(PXGraph graph) : base(graph)
		{
			_Graph.FieldUpdated.AddHandler(typeof(TEntity), "UseDefault", UseDefaultFieldUpdated);
			_Graph.RowSelected.AddHandler(typeof(TEntity), RowSelected);
		}

		protected override void ValueFieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			base.ValueFieldSelecting(sender, e);
			var fieldState = e.ReturnState as PXFieldState;
			if (fieldState != null)
			{
				bool? useDefault = (bool?)sender.GetValue(e.Row, "UseDefault");
				if (useDefault == true)
				{
					var settingInfo = (TDefinitionEntity)PXSelectorAttribute.Select(sender, e.Row, "SettingName");
					if (settingInfo == null) throw new PXException(Messages.TaxSettingNotFound, sender.GetValue(e.Row, "SettingName"));
					e.ReturnValue = settingInfo.Value;
					if (settingInfo.Value != null)
					{
						fieldState.Error = null;
						fieldState.ErrorLevel = PXErrorLevel.Undefined;
						SetErrorLevel(sender, e.Row as TEntity, PXErrorLevel.Undefined);
					}
				}
			}
		}

		protected virtual void UseDefaultFieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			bool? useDefault = (bool?)sender.GetValue(e.Row, "UseDefault");

			if (useDefault == true)
			{
				//We'll fetch and show default value from ValueFieldSelecting handler
				sender.SetValue(e.Row, "Value", null);
			}
		}

		protected virtual void RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			var row = e.Row as TEntity;
			if (row == null)
			{
				return;
			}

			var parentInfo = PXSelectorAttribute.Select(sender, row, "SettingName") as TDefinitionEntity;
			if (parentInfo == null)
			{
				throw new PXException(Messages.TaxSettingNotFound, row.SettingName);
			}
			PXUIFieldAttribute.SetEnabled(sender, e.Row, nameof(row.UseDefault), parentInfo.Value != null || row.UseDefault == true);
		}

		protected override IEnumerable<IPRSetting> GetAttributeDefinitionList()
		{
			PXView attributeDefinitionView = new PXView(_Graph, true, BqlCommand.CreateInstance(typeof(TDefinitionSelect)));
			foreach (IPRSetting attribute in attributeDefinitionView.SelectMulti().Where(x => ((IPRSetting)x).AllowOverride == true))
			{
				yield return attribute;
			}
		}

		protected override bool AttributeValueIsRequired(TEntity attribute, SettingControlType controlType)
		{
			return attribute.Required == true && controlType != SettingControlType.CheckBox;
		}

		protected override bool AssignDefault(TEntity record, IPRSetting definition)
		{
			if (definition.AllowOverride == false)
			{
				record.UseDefault = true;
			}
			if (record.UseDefault == true)
			{
				record.Value = definition.Value;
			}

			return false;
		}

		protected override string InvalidAttributeErrorMessage => Messages.ValueBlankAndRequired;
	}

	public abstract class PRAttributeDefinitionSelectBase<TEntity, TSelect, TDynamicType, TTypeMeta> : PRAttributeSelectBase<TEntity, TSelect, TDynamicType, TTypeMeta>
		where TEntity : class, IBqlTable, IPRSetting, new()
		where TSelect : class, IBqlSelect, new()
		where TDynamicType : DynamicEntity<TDynamicType, TTypeMeta>
		where TTypeMeta : SettingDefinitionListAttribute
	{

		public PRAttributeDefinitionSelectBase(PXGraph graph) : base(graph) { }

		protected override IEnumerable<IPRSetting> GetAttributeDefinitionList()
		{
			return this.SettingDefinitions.Values;
		}

		protected override bool AttributeValueIsRequired(TEntity attribute, SettingControlType controlType)
		{
			return attribute.Required == true && controlType != SettingControlType.CheckBox;
		}

		protected override bool AssignDefault(TEntity record, IPRSetting definition)
		{
			PRSettingDefinition<TDynamicType, TTypeMeta> settingDefinition = definition as PRSettingDefinition<TDynamicType, TTypeMeta>;
			if (settingDefinition != null && settingDefinition.ControlType == SettingControlType.CheckBox && string.IsNullOrEmpty(record.Value))
			{
				record.Value = false.ToString();
				return true;
			}
			return false;
		}

		protected override string InvalidAttributeErrorMessage => Messages.ValueBlankAndRequired;
	}

	public class PRAttributeValuesSelect<TEntity, TSelect, TDefinitionEntity, TDefinitionSelect, TTypeSettingEntity, TTypeSettingSelect, TDynamicType, TTypeMeta> : PRAttributeValuesSelectBase<TEntity, TSelect, TDefinitionEntity, TDefinitionSelect, TDynamicType, TTypeMeta>
		where TEntity : class, IBqlTable, IPRSetting, new()
		where TSelect : class, IBqlSelect, new()
		where TDefinitionEntity : class, IBqlTable, IPRSetting, new()
		where TDefinitionSelect : class, IBqlSelect, new()
		where TTypeSettingEntity : class, IBqlTable, IPRReflectiveSettingMapper<TTypeMeta>, new()
		where TTypeSettingSelect : class, IBqlSelect, new()
		where TDynamicType : DynamicEntity<TDynamicType, TTypeMeta>
		where TTypeMeta : SettingDefinitionListAttribute
	{
		public PRAttributeValuesSelect(PXGraph graph) : base(graph) { }

		protected override IEnumerable<IPRReflectiveSettingMapper<TTypeMeta>> GetMetaTypes()
		{
			PXView settingView = new PXView(_Graph, true, BqlCommand.CreateInstance(typeof(TTypeSettingSelect)));
			return settingView.SelectMulti().Select(x => x as IPRReflectiveSettingMapper<TTypeMeta>);
		}

		protected override IEnumerable<IPRReflectiveSettingMapper<TTypeMeta>> GetAllMetaTypes()
		{
			return SelectFrom<TTypeSettingEntity>.View.Select(_Graph).FirstTableItems.Select(x => x as IPRReflectiveSettingMapper<TTypeMeta>);
		}
	}

	public class PRAttributeDefinitionSelect<TEntity, TSelect, TTypeSetting, TDynamicType, TTypeMeta> : PRAttributeDefinitionSelectBase<TEntity, TSelect, TDynamicType, TTypeMeta>
		where TEntity : class, IBqlTable, IPRSetting, new()
		where TSelect : class, IBqlSelect, new()
		where TTypeSetting : class, IBqlTable, IPRReflectiveSettingMapper<TTypeMeta>, new()
		where TDynamicType : DynamicEntity<TDynamicType, TTypeMeta>
		where TTypeMeta : SettingDefinitionListAttribute
	{

		public PRAttributeDefinitionSelect(PXGraph graph) : base(graph) { }

		protected override IEnumerable<IPRReflectiveSettingMapper<TTypeMeta>> GetMetaTypes()
		{
			object current = _Graph.Caches[typeof(TTypeSetting)].Current;
			if (current != null)
			{
				yield return current as IPRReflectiveSettingMapper<TTypeMeta>;
			}
			else
			{
				yield break;
			}
		}

		protected override IEnumerable<IPRReflectiveSettingMapper<TTypeMeta>> GetAllMetaTypes()
		{
			return SelectFrom<TTypeSetting>.View.Select(_Graph).FirstTableItems.Select(x => x as IPRReflectiveSettingMapper<TTypeMeta>);
		}
	}

	public class PREmployeeAttributeDefinitionSelect<TEntity, TSelect, TFilterEntity, TFilterField, TSearchEntity, TSearchField> : PRAttributeDefinitionSelectBase<TEntity, TSelect, PX.Payroll.Data.PREmployee, EmployeeLocationSettingsAttribute>
		where TEntity : class, IBqlTable, IPRSetting, new()
		where TSelect : class, IBqlSelect, new()
		where TFilterEntity : class, IBqlTable, new()
		where TFilterField : class, IBqlField
		where TSearchEntity : class, IBqlTable, new()
		where TSearchField : class, IBqlField
	{
		public PREmployeeAttributeDefinitionSelect(PXGraph graph) : base(graph) { }

		protected override IEnumerable<IPRReflectiveSettingMapper<EmployeeLocationSettingsAttribute>> GetMetaTypes()
		{
			HashSet<PRState> displayStates = null;
			if (_Graph.Caches[typeof(TFilterEntity)].GetValue<TFilterField>(_Graph.Caches[typeof(TFilterEntity)].Current).Equals(true))
			{
				displayStates = new HashSet<PRState>();
				foreach (TSearchEntity row in SelectFrom<TSearchEntity>.View.Select(_Graph))
				{
					var value = _Graph.Caches[typeof(TSearchEntity)].GetValue<TSearchField>(row);
					if (value != null)
					{
						var stateAbbr = value as string;
						PRState state = null;

						if (stateAbbr != null)
						{
							state = PRState.FromAbbr(stateAbbr);
						}

						if (state == null)
						{
							throw new PXException(Messages.CantUseFieldAsState, _Graph.Caches[typeof(TSearchEntity)].GetField(typeof(TSearchField)));
						}

						displayStates.Add(state);
					}
				}
				displayStates.Add(PRState.FromAbbr(LocationConstants.FederalStateCode));
			}
			else
			{
				displayStates = new HashSet<PRState>(PRState.All);
			}

			return displayStates.Select(x => new PREmployeeSettingMapper(x));
		}

		protected override IEnumerable<IPRReflectiveSettingMapper<EmployeeLocationSettingsAttribute>> GetAllMetaTypes()
		{
			return PRState.All.Select(x => new PREmployeeSettingMapper(x));
		}

		protected override bool AttributeValueIsRequired(TEntity attribute, SettingControlType controlType)
		{
			return attribute.Required == true && attribute.AllowOverride == false && controlType != SettingControlType.CheckBox;
		}
		protected override string InvalidAttributeErrorMessage => Messages.ValueBlankAndRequiredAndNotOverridable;
	}

	/// <typeparam name="TRequiredInSearch"> BQL Search to retrieve values to fill the Required parameter in <typeparamref name="TDefinitionSelect"/> BQL. </typeparam>
	public class PREmployeeAttributeValueSelect<TEntity, TSelect, TDefinitionEntity, TDefinitionSelect, TRequiredInSearch, TSearchField, TPrimaryViewType> : PRAttributeValuesSelectBase<TEntity, TSelect, TDefinitionEntity, TDefinitionSelect, PX.Payroll.Data.PREmployee, EmployeeLocationSettingsAttribute>
		where TEntity : class, IBqlTable, IPRSetting, new()
		where TSelect : class, IBqlSelect, new()
		where TDefinitionEntity : class, IBqlTable, IPRSetting, new()
		where TDefinitionSelect : class, IBqlSelect, new()
		where TRequiredInSearch : IBqlSelect
		where TSearchField : IBqlField
		where TPrimaryViewType : class, IBqlTable
	{
		public PREmployeeAttributeValueSelect(PXGraph graph) : base(graph) { }

		public void SetSettingNameForDescription(TEntity row)
		{
			if (!string.IsNullOrEmpty(row.Description) && row is IStateSpecific stateSpecificRow && !string.IsNullOrEmpty(stateSpecificRow.State))
			{
				PRSettingDefinition<PX.Payroll.Data.PREmployee, EmployeeLocationSettingsAttribute> definition = 
					SettingDefinitions.Values.First(x => x.State == stateSpecificRow.State && x.Description == row.Description);
				row.SettingName = definition.SettingName;
				row.TypeName = definition.TypeName;
				row.AllowOverride = definition.AllowOverride;
				row.UseDefault = definition.AllowOverride != true;
			}
		}

		protected override IEnumerable<IPRReflectiveSettingMapper<EmployeeLocationSettingsAttribute>> GetMetaTypes()
		{
			List<TDefinitionEntity> fullDefinitionList = SelectFrom<TDefinitionEntity>.View.Select(_Graph).FirstTableItems.ToList();
			if (_Graph.IsImport && fullDefinitionList.FirstOrDefault() is IStateSpecific)
			{
				foreach (string state in PRState.All.Select(x => x.Abbr))
				{
					TDefinitionEntity definition = fullDefinitionList.FirstOrDefault(x => ((IStateSpecific)x).State == state);
					if (definition != null)
					{
						yield return new PREmployeeSettingMapper(definition, (IStateSpecific)definition);
					}
				}

				yield break;
			}

			object[] parameters = GetBqlParameters();
			PXView settingsView = GetAttributDefinitionView(parameters);
			var statesAdded = new HashSet<string>();
			foreach (object row in settingsView.SelectMulti(parameters))
			{
				if (row is IStateSpecific)
				{
					var state = ((IStateSpecific)row).State;
					if (statesAdded.Add(state))
					{
						yield return new PREmployeeSettingMapper((TDefinitionEntity)row, (IStateSpecific)row);
					}
				}
			}

			if (fullDefinitionList.FirstOrDefault() is IStateSpecific)
			{
				PXCache employeeTaxCache = _Graph.Caches[typeof(PREmployeeTax)];
				foreach (PREmployeeTax newEmployeeTax in employeeTaxCache.Inserted)
				{
					var state = (PXSelectorAttribute.Select<PREmployeeTax.taxID>(employeeTaxCache, newEmployeeTax) as PRTaxCode)?.TaxState;
					if (!string.IsNullOrEmpty(state) && statesAdded.Add(state))
					{
						TDefinitionEntity definition = fullDefinitionList.FirstOrDefault(x => ((IStateSpecific)x).State == state);
						if (definition != null)
						{
							yield return new PREmployeeSettingMapper(definition, (IStateSpecific)definition);
						}
					}
				}
			}
		}

		protected override IEnumerable<IPRReflectiveSettingMapper<EmployeeLocationSettingsAttribute>> GetAllMetaTypes()
		{
			return SelectFrom<TDefinitionEntity>.View.Select(_Graph).FirstTableItems
				.Where(x => x is IStateSpecific)
				.Select(x => new PREmployeeSettingMapper(x, (IStateSpecific)x));
		}

		protected override IEnumerable<IPRSetting> GetAttributeDefinitionList()
		{
			if (_Graph.IsImport)
			{
				return new SelectFrom<TDefinitionEntity>.View(_Graph).Select().FirstTableItems;
			}
			
			var list = new List<IPRSetting>();
			PXCache cache = _Graph.Caches[typeof(TPrimaryViewType)];
			if (cache.GetStatus(cache.Current) == PXEntryStatus.Inserted)
			{
				return list;
			}

			object[] parameters = GetBqlParameters();
			PXView attributeDefinitionView = GetAttributDefinitionView(parameters);
			foreach (IPRSetting attribute in attributeDefinitionView.SelectMulti(parameters).Where(x => ((IPRSetting)x).AllowOverride == true))
			{
				list.Add(attribute);
			}

			return list;
		}

		private object[] GetBqlParameters()
		{
			var searchView = new PXView(_Graph, false, BqlCommand.CreateInstance(typeof(TRequiredInSearch)));
			var fieldType = typeof(TSearchField).DeclaringType;
			var fieldName = typeof(TSearchField).Name;
			return searchView.SelectMulti().GroupBy(x => searchView.Cache.GetValue(PXResult.Unwrap(x, fieldType), fieldName)).Select(x => x.Key).Where(x => x != null).ToArray();
		}

		private PXView GetAttributDefinitionView(IEnumerable<object> parameters)
		{
			PXView attributeDefinitionView = new PXView(_Graph, true, BqlCommand.CreateInstance(typeof(TDefinitionSelect)));
			for (int i = 0; i < parameters.Count(); i++)
			{
				attributeDefinitionView.WhereOr(BqlCommand.Compose(typeof(Where<,>), typeof(PRCompanyTaxAttribute.state), typeof(Equal<>), typeof(Required<>), typeof(PRCompanyTaxAttribute.state)));
			}
			return attributeDefinitionView;
		}
	}

	public class PRSettingDefinition<TDynamicType, TTypeMeta> : IPRSetting
		where TDynamicType : DynamicEntity<TDynamicType, TTypeMeta>
		where TTypeMeta : SettingDefinitionListAttribute
	{
		protected PRSettingDefinition(MetaDynamicSetting<TTypeMeta> metaSetting, TTypeMeta typeMeta)
		{
			TypeName = metaSetting.TypeName;
			SettingName = metaSetting.UniqueSettingName;
			AllowOverride = metaSetting.SettingMeta.AllowOverride;
			Required = metaSetting.SettingMeta.Required;
			Value = metaSetting.SettingMeta.DefaultValue as string;
			SortOrder = metaSetting.Order;
			AatrixMapping = metaSetting.AatrixMapping != null ? metaSetting.AatrixMapping.Field : new int?();
			UseDefault = false;
			Description = metaSetting.SettingMeta.Description;

			SetCustomFormat(metaSetting.SettingControlType, metaSetting.SettingMeta);
			SetAttributeSpecificProperties(typeMeta);
		}

		public static MetaTypeSettingCollection<TDynamicType, TTypeMeta> Convert(DynamicEntity<TDynamicType, TTypeMeta>.MetaWithSetting meta)
		{
			MetaTypeSettingCollection<TDynamicType, TTypeMeta> settingCollection = new MetaTypeSettingCollection<TDynamicType, TTypeMeta>();
			if (meta == null || meta.Settings == null)
			{
				return settingCollection;
			}

			meta.Settings.ForEach(kvp => settingCollection[kvp.Key] = new PRSettingDefinition<TDynamicType, TTypeMeta>(kvp.Value, meta.TypeMeta));
			return settingCollection;
		}

		public virtual string TypeName { get; set; }
		public virtual string SettingName { get; set; }
		public virtual bool? AllowOverride { get; set; }
		public virtual bool? Required { get; set; }
		public virtual Boolean? UseDefault { get; set; }
		public virtual string Value { get; set; }
		public virtual int? SortOrder { get; set; }
		public virtual int? AatrixMapping { get; set; }
		public virtual int? ErrorLevel { get; set; }

		public virtual string Description { get; set; }

		public virtual string EntryMask { get; set; }
		public virtual string RegExp { get; set; }

		public virtual decimal? MinValue { get; set; }
		public virtual decimal? MaxValue { get; set; }
		public virtual int? Precision { get; set; }

		public virtual SettingControlType ControlType { get; set; }
		public virtual IEnumerable<PRComboValue> ComboListItems { get; set; }

		private void SetCustomFormat(SettingControlType settingControlType, SettingAttribute setting)
		{
			if (setting is TextSettingAttribute)
			{
				this.ControlType = SettingControlType.Text;

				var textSetting = (TextSettingAttribute)setting;

				EntryMask = textSetting.EntryMask;
				RegExp = textSetting.RegExp;
			}
			else if (setting is DecimalSettingAttribute)
			{
				this.ControlType = SettingControlType.Decimal;

				var decimalSetting = (DecimalSettingAttribute)setting;

				MinValue = decimalSetting.GetMinValue();
				MaxValue = decimalSetting.GetMaxValue();
				Precision = decimalSetting.Precision;
			}
			else if (setting is ComboListSettingAttribute)
			{
				this.ControlType = SettingControlType.Combo;

				var comboListSetting = (ComboListSettingAttribute)setting;

				this.ComboListItems = GetComboListItems(comboListSetting);
			}
			else
			{
				this.ControlType = settingControlType;
			}
		}

		private IEnumerable<PRComboValue> GetComboListItems(ComboListSettingAttribute attr)
		{
			int cntr = 0;
			foreach (var comboListItem in attr.GetValueList())
			{
				yield return new PRComboValue()
				{
					SortOrder = ++cntr,
					Value = comboListItem.Key,
					DisplayName = comboListItem.Value
				};
			}
		}

		#region Specific Properties
		// These properties are not used for all Settings, depends on the interfaces implemented by
		// MetaDynamicSetting.SettingMeta
		public virtual string State { get; set; }

		private void SetAttributeSpecificProperties(TTypeMeta typeMeta)
		{
			if (typeMeta is IStateSpecific)
			{
				State = ((IStateSpecific)typeMeta).State;
			}
		}

		public void AssignSpecificDacFields(PXCache cache, IBqlTable row)
		{
			cache.SetValue(row, "State", State);
		}
		#endregion
	}
	public class PRComboValue
	{
		public virtual int SortOrder { get; set; }
		public virtual string Value { get; set; }
		public virtual string DisplayName { get; set; }
	}
}
