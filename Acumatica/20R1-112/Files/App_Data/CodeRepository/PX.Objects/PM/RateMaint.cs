using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using PX.Data;
using PX.Objects.IN;
using PX.Objects.CR;

namespace PX.Objects.PM
{
    [Serializable]
	public class RateMaint : PXGraph<RateMaint, PMRateSequence>
	{
		#region Views/Selects
		[PXHidden]
		public PXSelect<EP.EPEmployee> HiddenEmployee;
		
		public PXSelect<PMRateSequence> RateSequence;
	    public PXSelect<PMRateDefinition, Where<PMRateDefinition.rateTableID, Equal<Current<PMRateSequence.rateTableID>>,
			And<PMRateDefinition.rateTypeID, Equal<Current<PMRateSequence.rateTypeID>>,
			And<PMRateDefinition.sequence, Equal<Current<PMRateSequence.sequence>>>>>> RateDefinition;

		public PXSelect<PMRate, Where<PMRate.rateDefinitionID, Equal<Current<PMRateSequence.rateDefinitionID>>, And<PMRate.rateCodeID, Equal<Current<PMRateSequence.rateCodeID>>>>> Rates;
		public PXSelectJoin<PMProjectRate, LeftJoin<PMProject, On<PMProjectRate.projectCD, Equal<PMProject.contractCD>>>, Where<PMProjectRate.rateDefinitionID, Equal<Current<PMRateSequence.rateDefinitionID>>, And<PMProjectRate.rateCodeID, Equal<Current<PMRateSequence.rateCodeID>>>>> Projects;
		public PXSelect<PMTaskRate, Where<PMTaskRate.rateDefinitionID, Equal<Current<PMRateSequence.rateDefinitionID>>, And<PMTaskRate.rateCodeID, Equal<Current<PMRateSequence.rateCodeID>>>>> Tasks;
		public PXSelectJoin<PMAccountGroupRate, InnerJoin<PMAccountGroup, On<PMAccountGroup.groupID, Equal<PMAccountGroupRate.accountGroupID>>>, Where<PMAccountGroupRate.rateDefinitionID, Equal<Current<PMRateSequence.rateDefinitionID>>, And<PMAccountGroupRate.rateCodeID, Equal<Current<PMRateSequence.rateCodeID>>>>> AccountGroups;
		public PXSelectJoin<PMItemRate, InnerJoin<InventoryItem, On<PMItemRate.inventoryID, Equal<InventoryItem.inventoryID>>>, Where<PMItemRate.rateDefinitionID, Equal<Current<PMRateSequence.rateDefinitionID>>, And<PMItemRate.rateCodeID, Equal<Current<PMRateSequence.rateCodeID>>>>> Items;
		public PXSelectJoin<PMEmployeeRate, InnerJoin<BAccount, On<BAccount.bAccountID, Equal<PMEmployeeRate.employeeID>>>, Where<PMEmployeeRate.rateDefinitionID, Equal<Current<PMRateSequence.rateDefinitionID>>, And<PMEmployeeRate.rateCodeID, Equal<Current<PMRateSequence.rateCodeID>>>>> Employees;
		#endregion
		
		
		public override void Persist()
		{
			this.CopyPaste.SetVisible(false);

			OnValidateRate();
			OnValidateEntities();
			base.Persist();
		}

		protected virtual void OnValidateRate()
		{
			List<PMRate> checkedRates = new List<PMRate>();

			foreach (PMRate rate in Rates.Select())
			{
				if (checkedRates.Count > 0)
				{
					if (IsOverlaping(checkedRates, rate))
					{
						Rates.Cache.RaiseExceptionHandling<PMRate.startDate>(rate, rate.StartDate, new PXSetPropertyException(Messages.PeriodsOverlap, PXErrorLevel.RowError));
						throw new PXException(Messages.PeriodsOverlap);
					}
					else
					{
						checkedRates.Add(rate);
					}
				}
				else
				{
					checkedRates.Add(rate);
				}
			
			
			}

		}

		private bool IsOverlaping(IList<PMRate> rates, PMRate x)
		{
			foreach (PMRate y in rates)
			{
				if (IsOverlaping(x, y))
					return true;
			}

			return false;
		}

		private bool IsOverlaping(PMRate x, PMRate y)
		{
			if (x.StartDate == null && x.EndDate == null)
			{
				return true; 
			}
			else if (x.StartDate == null && x.EndDate != null)
			{
				if (y.StartDate == null)
					return true;
				else 
					return y.StartDate.Value.Date < x.EndDate.Value.Date;
			}
			else if (x.StartDate != null && x.EndDate == null)
			{
				if (y.EndDate == null)
					return true;
				else
					return y.EndDate.Value.Date > x.StartDate.Value.Date;
			}
			else
			{
				if (y.StartDate == null || y.EndDate == null)
					return true;
				else
				{
					if (x.StartDate.Value.Date <= y.StartDate.Value.Date)
					{
						return y.StartDate.Value.Date < x.EndDate.Value.Date;
					}
					else
					{
						return x.StartDate.Value.Date < y.EndDate.Value.Date;
					}
				}
			}
		}

		protected virtual void OnValidateEntities()
		{
			if (RateDefinition.Current != null && RateSequence.Current != null)
			{
				string errors = RunEntryValidationAndReturnErrors(RateDefinition.Current, RateSequence.Current.RateCodeID);
				if (!string.IsNullOrEmpty(errors))
				{
					var message = PXMessages.LocalizeNoPrefix(Messages.RateTableIsInvalid) + Environment.NewLine + errors;
					throw new PXException(message);
				}
			}
		}

		protected class Entity<K,V,T> where V:IBqlTable where T:IBqlTable
		{
			public K Key { get; private set; }
			public V Value { get; private set; }
			public T Object { get; private set; }

			public Entity()
			{
			}

			public Entity(K key, V value, T entity)
			{
				this.Key = key;
				this.Value = value;
				this.Object = entity;
			}
		}
		
		protected virtual string RunEntryValidationAndReturnErrors(PMRateDefinition definition, string rateCodeID)
		{
			Dictionary<string, RateCodeData> otherRateCodes = new Dictionary<string, RateCodeData>();
			
			#region Fill Hshtables with records from other rate codes
			if (definition.Project == true)
			{
				foreach (PMProjectRate item in PXSelect<PMProjectRate, Where<PMProjectRate.rateDefinitionID, Equal<Required<PMProjectRate.rateDefinitionID>>, And<PMProjectRate.rateCodeID, NotEqual<Required<PMProjectRate.rateCodeID>>>>>.Select(this, definition.RateDefinitionID, rateCodeID))
				{
					if (!otherRateCodes.ContainsKey(item.RateCodeID))
						otherRateCodes.Add(item.RateCodeID, new RateCodeData(item.RateCodeID));
					otherRateCodes[item.RateCodeID].Projects.Add(item.ProjectCD.ToUpper().Trim(), item);
				}
			}

			if (definition.Task == true)
			{
				foreach (PMTaskRate item in PXSelect<PMTaskRate, Where<PMTaskRate.rateDefinitionID, Equal<Required<PMTaskRate.rateDefinitionID>>, And<PMTaskRate.rateCodeID, NotEqual<Required<PMTaskRate.rateCodeID>>>>>.Select(this, definition.RateDefinitionID, rateCodeID))
				{
					if (!otherRateCodes.ContainsKey(item.RateCodeID))
						otherRateCodes.Add(item.RateCodeID, new RateCodeData(item.RateCodeID));
					otherRateCodes[item.RateCodeID].Tasks.Add(item.TaskCD.ToUpper().Trim(), item);
				}
			}

			if (definition.AccountGroup == true)
			{
				foreach (PMAccountGroupRate item in PXSelect<PMAccountGroupRate, Where<PMAccountGroupRate.rateDefinitionID, Equal<Required<PMAccountGroupRate.rateDefinitionID>>, And<PMAccountGroupRate.rateCodeID, NotEqual<Required<PMAccountGroupRate.rateCodeID>>>>>.Select(this, definition.RateDefinitionID, rateCodeID))
				{
					if (!otherRateCodes.ContainsKey(item.RateCodeID))
						otherRateCodes.Add(item.RateCodeID, new RateCodeData(item.RateCodeID));
					otherRateCodes[item.RateCodeID].AccountGroups.Add(item.AccountGroupID.Value, item);
				}

			}

			if (definition.RateItem == true)
			{
				foreach (PMItemRate item in PXSelect<PMItemRate, Where<PMItemRate.rateDefinitionID, Equal<Required<PMItemRate.rateDefinitionID>>, And<PMItemRate.rateCodeID, NotEqual<Required<PMItemRate.rateCodeID>>>>>.Select(this, definition.RateDefinitionID, rateCodeID))
				{
					if (!otherRateCodes.ContainsKey(item.RateCodeID))
						otherRateCodes.Add(item.RateCodeID, new RateCodeData(item.RateCodeID));
					otherRateCodes[item.RateCodeID].Inventory.Add(item.InventoryID.Value, item);
				}
			}

			if (definition.Employee == true)
			{
				foreach (PMEmployeeRate item in PXSelect<PMEmployeeRate, Where<PMEmployeeRate.rateDefinitionID, Equal<Required<PMEmployeeRate.rateDefinitionID>>, And<PMEmployeeRate.rateCodeID, NotEqual<Required<PMEmployeeRate.rateCodeID>>>>>.Select(this, definition.RateDefinitionID, rateCodeID))
				{
					if (!otherRateCodes.ContainsKey(item.RateCodeID))
						otherRateCodes.Add(item.RateCodeID, new RateCodeData(item.RateCodeID));
					otherRateCodes[item.RateCodeID].Employees.Add(item.EmployeeID.Value, item);
				}
			} 
			#endregion

			string errors = null;

			List<Entity<string, PMProjectRate, PMProject>> data1 = new List<Entity<string, PMProjectRate, PMProject>>();//project
			List<Entity<string, PMTaskRate, PMTask>> data2 = new List<Entity<string, PMTaskRate, PMTask>>();//task
			List<Entity<int?, PMAccountGroupRate, PMAccountGroup>> data3 = new List<Entity<int?, PMAccountGroupRate, PMAccountGroup>>();//accountgroup
			List<Entity<int?, PMItemRate, InventoryItem>> data4 = new List<Entity<int?, PMItemRate, InventoryItem>>();//inventory
			List<Entity<int?, PMEmployeeRate, BAccount>> data5 = new List<Entity<int?, PMEmployeeRate, BAccount>>();//employee
			
			#region Fill Combinations
			if (definition.Project == true)
			{
				foreach (PXResult<PMProjectRate, PMProject> project in Projects.Select())
				{
					if (Projects.Cache.GetStatus((PMProjectRate)project) != PXEntryStatus.Deleted)
					{
						data1.Add(new Entity<string, PMProjectRate, PMProject>(((PMProjectRate)project).ProjectCD, (PMProjectRate)project, (PMProject)project));
					}
				}
			}

			if (definition.Task == true)
			{
				foreach (PMTaskRate task in Tasks.Select())
				{
					if (Tasks.Cache.GetStatus(task) != PXEntryStatus.Deleted)
					{
						data2.Add(new Entity<string, PMTaskRate, PMTask>(task.TaskCD, task, null));
					}
				}
			}

			if (definition.AccountGroup == true)
			{
				foreach (PXResult<PMAccountGroupRate, PMAccountGroup> accountGroup in AccountGroups.Select())
				{
					if (AccountGroups.Cache.GetStatus((PMAccountGroupRate)accountGroup) != PXEntryStatus.Deleted)
					{
						data3.Add(new Entity<int?, PMAccountGroupRate, PMAccountGroup>(((PMAccountGroupRate)accountGroup).AccountGroupID, (PMAccountGroupRate)accountGroup, (PMAccountGroup)accountGroup));
					}
				}

			}

			if (definition.RateItem == true)
			{
				foreach (PXResult<PMItemRate, InventoryItem> item in Items.Select())
				{
					if (Items.Cache.GetStatus((PMItemRate)item) != PXEntryStatus.Deleted)
					{
						data4.Add(new Entity<int?, PMItemRate, InventoryItem>(((PMItemRate)item).InventoryID, (PMItemRate)item, (InventoryItem)item));
					}
				}
			}

			if (definition.Employee == true)
			{
				foreach (PXResult<PMEmployeeRate, BAccount> employee in Employees.Select())
				{
					if (Employees.Cache.GetStatus((PMEmployeeRate)employee) != PXEntryStatus.Deleted)
					{
						data5.Add(new Entity<int?, PMEmployeeRate, BAccount>(((PMEmployeeRate)employee).EmployeeID, (PMEmployeeRate)employee, (BAccount)employee));
					}
				}
			}

			if (data1.Count == 0)
				data1.Add(new Entity<string, PMProjectRate, PMProject>());
			if (data2.Count == 0)
				data2.Add(new Entity<string, PMTaskRate, PMTask>());
			if (data3.Count == 0)
				data3.Add(new Entity<int?, PMAccountGroupRate, PMAccountGroup>());
			if (data4.Count == 0)
				data4.Add(new Entity<int?, PMItemRate, InventoryItem>());
			if (data5.Count == 0)
				data5.Add(new Entity<int?, PMEmployeeRate, BAccount>());

			#endregion

			StringBuilder sb = new StringBuilder();

			foreach (RateCodeData otherRateCode in otherRateCodes.Values)
			{
				string error = otherRateCode.Validate(data1, data2, data3, data4, data5);
				if (error != null)
					sb.AppendLine(error);
			}
			if (sb.Length > 0)
			{
				errors = sb.ToString();
			}
			return errors;
		}
	    protected class RateCodeData
	    {
		    public string RateCode { get; private set; }
		    public Hashtable Projects { get; private set; }
			public Hashtable Tasks { get; private set; }
			public Hashtable AccountGroups { get; private set; }
			public Hashtable Inventory { get; private set; }
			public Hashtable Employees { get; private set; }
		    public RateCodeData(string rateCode)
		    {
			    this.RateCode = rateCode;
				Projects = new Hashtable();
				Tasks = new Hashtable();
				AccountGroups = new Hashtable();
				Inventory = new Hashtable();
				Employees = new Hashtable();
		    }
			public string Validate(List<Entity<string, PMProjectRate, PMProject>> data1,
				List<Entity<string, PMTaskRate, PMTask>> data2,
				List<Entity<int?, PMAccountGroupRate, PMAccountGroup>> data3,
				List<Entity<int?, PMItemRate, InventoryItem>> data4,
				List<Entity<int?, PMEmployeeRate, BAccount>> data5)
		    {
				string errors = null;
				StringBuilder sb = new StringBuilder();
			foreach (Entity<string, PMProjectRate, PMProject> i1 in data1)
			{
				foreach (Entity<string, PMTaskRate, PMTask> i2 in data2)
				{
					foreach (Entity<int?, PMAccountGroupRate, PMAccountGroup> i3 in data3)
					{
						foreach (Entity<int?, PMItemRate, InventoryItem> i4 in data4)
						{
							foreach (Entity<int?, PMEmployeeRate, BAccount> i5 in data5)
							{
								bool isDuplicate = true;

									if (i1.Key != null && !Projects.ContainsKey(i1.Key.ToUpper().Trim()))
								{
									isDuplicate = false;
									continue;
								}

									if (i2.Key != null && !Tasks.ContainsKey(i2.Key.ToUpper().Trim()))
								{
									isDuplicate = false;
									continue;
								}

									if (i3.Key != null && !AccountGroups.ContainsKey(i3.Key))
								{
									isDuplicate = false;
									continue;
								}

									if (i4.Key != null && !Inventory.ContainsKey(i4.Key))
								{
									isDuplicate = false;
									continue;
								}

									if (i5.Key != null && !Employees.ContainsKey(i5.Key))
								{
									isDuplicate = false;
									continue;
								}
								
								if (isDuplicate)
								{
									bool hasError = false;
									if (i1.Key != null)
									{
										sb.AppendFormat("{0}:{1}, ", PXMessages.LocalizeNoPrefix(Messages.Project), i1.Key);
										hasError = true;
									}

									if (i2.Key != null)
									{
										sb.AppendFormat("{0}:{1}, ", PXMessages.LocalizeNoPrefix(Messages.ProjectTask), i2.Key);
											
										hasError = true;
									}

									if (i3.Key != null)
									{
											sb.AppendFormat("{0}:{1}, ", PXMessages.LocalizeNoPrefix(Messages.AccountGroup), i3.Object.GroupCD);
										hasError = true;
									}

									if (i4.Key != null)
									{
										sb.AppendFormat("{0}:{1}, ", PXMessages.LocalizeNoPrefix(IN.Messages.InventoryItem), i4.Object.InventoryCD);
										hasError = true;
									}

									if (i5.Key != null)
									{
										sb.AppendFormat("{0}:{1}, ", PXMessages.LocalizeNoPrefix(EP.Messages.Employee), i5.Object.AcctCD);
										hasError = true;
									}

									if ( hasError )
										sb.AppendLine("");

								}

							}
						}
					}
				}
			}

			if (sb.Length > 0)
			{
					errors = string.Format("{0} = {1} ", PXMessages.LocalizeNoPrefix(Messages.RateCode), RateCode) + sb.ToString();
			}
			
			return errors;
		}
		
	    }
		#region Event Handlers

		protected virtual void _(Events.FieldSelecting<PMRateSequence, PMRateSequence.sequence> e)
		{
			PXSelectBase<PMRateDefinition> select = new PXSelect<PMRateDefinition, Where<PMRateDefinition.rateTableID, Equal<Required<PMRateSequence.rateTableID>>,
				And<PMRateDefinition.rateTypeID, Equal<Required<PMRateSequence.rateTypeID>>>>, OrderBy<Asc<PMRateDefinition.sequence>>>(this);

			List<string> labels = new List<string>();
			List<int> values = new List<int>();

			int? retValue = (int?)e.ReturnValue;
			bool valueValid = false;

			PMRateSequence currentSequence = RateSequence.Current ?? e.Row;

			if (currentSequence != null)
			{
				foreach (PMRateDefinition rd in select.Select(currentSequence.RateTableID, currentSequence.RateTypeID))
				{
					string description = string.Format("{0} - [ ", rd.Sequence);
					if (rd.Project == true)
						description = description + PXMessages.LocalizeNoPrefix(Messages.Project) + "  ";
					if (rd.Task == true)
						description = description + PXMessages.LocalizeNoPrefix(Messages.Task) + "  ";
					if (rd.AccountGroup == true)
						description = description + PXMessages.LocalizeNoPrefix(Messages.AccountGroup) + "  ";
					if (rd.RateItem == true)
						description = description + PXMessages.LocalizeNoPrefix(Messages.Item) + "  ";
					if (rd.Employee == true)
						description = description + PXMessages.LocalizeNoPrefix(EP.Messages.Employee) + "  ";


					description = description + "] " + rd.Description;

					if (retValue != null && retValue.Value == rd.Sequence.Value)
						valueValid = true;

					labels.Add(description);
					values.Add(Convert.ToInt32(rd.Sequence.Value));
				}
			}


			if (!valueValid && values.Count > 0)
				e.ReturnValue = values[0];

			e.ReturnState = PXIntState.CreateInstance(e.ReturnState, "Sequence", false, 1, null, null,
														values.ToArray(), labels.ToArray(), null, null);

		}
		
		protected virtual void PMRate_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			PMRate row = (PMRate)e.Row;
			if (null != row.StartDate && null != row.EndDate && row.StartDate > row.EndDate && e.Operation != PXDBOperation.Delete)
			{
				sender.RaiseExceptionHandling<PMRate.endDate>(row, row.EndDate, new PXSetPropertyException(CR.Messages.EndDateLessThanStartDate));
			}
		}

		#endregion

		
	}
}
