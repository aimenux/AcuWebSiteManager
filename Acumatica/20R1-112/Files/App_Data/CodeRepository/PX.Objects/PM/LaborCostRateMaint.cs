using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Data;
using System.Collections;
using PX.Objects.IN;
using PX.Objects.EP;
using PX.Objects.CS;
using PX.Objects.CR;
using PX.Data.Api.Export;
using PX.Objects.CT;

namespace PX.Objects.PM
{
	[NonOptimizable(IgnoreOptimizationBehavior = true)]
	public class LaborCostRateMaint : PXGraph<LaborCostRateMaint>, PXImportAttribute.IPXPrepareItems
	{
		public PXFilter<PMLaborCostRateFilter> Filter;
		public PXCancel<PMLaborCostRateFilter> Cancel;
		public PXSave<PMLaborCostRateFilter> Save;
		[PXImport(typeof(PMLaborCostRateFilter))]
		public PXSelect<PMLaborCostRate, 
			Where2<Where<PMLaborCostRate.type, Equal<Current<PMLaborCostRateFilter.type>>, Or<Current<PMLaborCostRateFilter.type>, Equal<PMLaborCostRateType.all>>>,
				And2<Where<PMLaborCostRate.unionID, Equal<Current<PMLaborCostRateFilter.unionID>>, Or<Current<PMLaborCostRateFilter.unionID>, IsNull>>,
				And2<Where<PMLaborCostRate.projectID, Equal<Current<PMLaborCostRateFilter.projectID>>, Or<Current<PMLaborCostRateFilter.projectID>, IsNull>>,
				And2<Where<PMLaborCostRate.taskID, Equal<Current<PMLaborCostRateFilter.taskID>>, Or<Current<PMLaborCostRateFilter.taskID>, IsNull>>,
				And2<Where<PMLaborCostRate.employeeID, Equal<Current<PMLaborCostRateFilter.employeeID>>, Or<Current<PMLaborCostRateFilter.employeeID>, IsNull>>,
				And2<Where<PMLaborCostRate.inventoryID, Equal<Current<PMLaborCostRateFilter.inventoryID>>, Or<Current<PMLaborCostRateFilter.inventoryID>, IsNull>>,
				And<Where<PMLaborCostRate.effectiveDate, GreaterEqual<Current<PMLaborCostRateFilter.effectiveDate>>, Or<Current<PMLaborCostRateFilter.effectiveDate>, IsNull>>>>>>>>>> Items;
		
		
		protected virtual void _(Events.RowSelected<PMLaborCostRate> e)
		{			
			if (e.Row != null)
			{				
				PXUIFieldAttribute.SetEnabled<PMLaborCostRate.unionID>(e.Cache, e.Row, e.Row.Type == PMLaborCostRateType.Union);
				PXUIFieldAttribute.SetEnabled<PMLaborCostRate.projectID>(e.Cache, e.Row, e.Row.Type == PMLaborCostRateType.Certified || e.Row.Type == PMLaborCostRateType.Project);
				PXUIFieldAttribute.SetEnabled<PMLaborCostRate.employeeID>(e.Cache, e.Row, e.Row.Type == PMLaborCostRateType.Employee || e.Row.Type == PMLaborCostRateType.Project);
				PXUIFieldAttribute.SetEnabled<PMLaborCostRate.employmentType>(e.Cache, e.Row, e.Row.Type == PMLaborCostRateType.Employee);
				PXUIFieldAttribute.SetEnabled<PMLaborCostRate.regularHours>(e.Cache, e.Row, e.Row.Type == PMLaborCostRateType.Employee);
				PXUIFieldAttribute.SetEnabled<PMLaborCostRate.annualSalary>(e.Cache, e.Row, e.Row.EmploymentType != EP.RateTypesAttribute.Hourly);
				PXUIFieldAttribute.SetEnabled<PMLaborCostRate.rate>(e.Cache, e.Row, e.Row.EmploymentType == EP.RateTypesAttribute.Hourly);
			}
		}

		protected virtual void _(Events.FieldDefaulting<PMLaborCostRate, PMLaborCostRate.type> e)
		{
			if (Filter.Current != null && Filter.Current.Type != PMLaborCostRateType.All)
			{
				e.NewValue = Filter.Current.Type;
			}
			else
			{
				e.NewValue = PMLaborCostRateType.Employee;
			}
		}

		protected virtual void _(Events.FieldUpdated<PMLaborCostRate, PMLaborCostRate.type> e)
		{
			if (e.Row.Type == PMLaborCostRateType.Employee)
			{
				e.Row.UnionID = null;
				e.Row.ProjectID = null;
				e.Row.TaskID = null;
			}
			else if (e.Row.Type == PMLaborCostRateType.Project)
			{
				e.Row.UnionID = null;
				e.Row.RegularHours = null;
				e.Row.AnnualSalary = null;
				e.Row.EmploymentType = EP.RateTypesAttribute.Hourly;
			}
			else if (e.Row.Type == PMLaborCostRateType.Item)
			{
				e.Row.ProjectID = null;
				e.Row.TaskID = null;
				e.Row.UnionID = null;
				e.Row.EmployeeID = null;
				e.Row.RegularHours = null;
				e.Row.AnnualSalary = null;
				e.Row.EmploymentType = EP.RateTypesAttribute.Hourly;
			}
			else if (e.Row.Type == PMLaborCostRateType.Union)
			{
				e.Row.ProjectID = null;
				e.Row.TaskID = null;
				e.Row.EmployeeID = null;
				e.Row.RegularHours = null;
				e.Row.AnnualSalary = null;
				e.Row.EmploymentType = EP.RateTypesAttribute.Hourly;
			}
			else if (e.Row.Type == PMLaborCostRateType.Certified)
			{
				e.Row.UnionID = null;
				e.Row.TaskID = null;
				e.Row.EmployeeID = null;
				e.Row.RegularHours = null;
				e.Row.AnnualSalary = null;
				e.Row.EmploymentType = EP.RateTypesAttribute.Hourly;
			}
		}

		protected virtual void _(Events.FieldDefaulting<PMLaborCostRate, PMLaborCostRate.unionID> e)
		{
			if (Filter.Current != null && Filter.Current.UnionID != null)
			{
				e.NewValue = Filter.Current.UnionID;
			}
		}

		protected virtual void _(Events.FieldDefaulting<PMLaborCostRate, PMLaborCostRate.projectID> e)
		{
			if (Filter.Current != null && Filter.Current.ProjectID != null)
			{
				e.NewValue = Filter.Current.ProjectID;
			}
		}

		protected virtual void _(Events.FieldDefaulting<PMLaborCostRate, PMLaborCostRate.taskID> e)
		{
			if (Filter.Current != null && Filter.Current.TaskID != null)
			{
				e.NewValue = Filter.Current.TaskID;
			}
		}

		protected virtual void _(Events.FieldDefaulting<PMLaborCostRate, PMLaborCostRate.employeeID> e)
		{
			if (Filter.Current != null && Filter.Current.EmployeeID != null)
			{
				e.NewValue = Filter.Current.EmployeeID;
			}
		}

		protected virtual void _(Events.FieldUpdated<PMLaborCostRate, PMLaborCostRate.employeeID> e)
		{
			e.Cache.SetDefaultExt<PMLaborCostRate.inventoryID>(e.Row);

			if (e.Row.EffectiveDate == null)
				e.Cache.SetDefaultExt<PMLaborCostRate.effectiveDate>(e.Row);

			e.Cache.SetDefaultExt<PMLaborCostRate.regularHours>(e.Row);
		}

		protected virtual void _(Events.FieldDefaulting<PMLaborCostRate, PMLaborCostRate.inventoryID> e)
		{
			if (Filter.Current != null && Filter.Current.InventoryID != null)
			{
				e.NewValue = Filter.Current.InventoryID;
			}
			else if (e.Row.EmployeeID != null)
			{
				EPEmployee employee = PXSelect<EPEmployee, Where<EPEmployee.bAccountID, Equal<Required<EPEmployee.bAccountID>>>>.Select(this, e.Row.EmployeeID);
				if (employee != null)
				{
					PMLaborCostRate existing = PXSelect<PMLaborCostRate, Where<PMLaborCostRate.employeeID, Equal<Required<PMLaborCostRate.employeeID>>,
						And<PMLaborCostRate.inventoryID, Equal<Required<PMLaborCostRate.inventoryID>>,
						And<PMLaborCostRate.type, Equal<Required<PMLaborCostRate.type>>,
						And<PMLaborCostRate.recordID, NotEqual<Required<PMLaborCostRate.recordID>>>>>>>.Select(this, e.Row.EmployeeID, employee.LabourItemID, e.Row.Type, e.Row.RecordID);

					if (existing == null)
					{
						e.NewValue = employee.LabourItemID;
					}
				}
			}

		}

		protected virtual void _(Events.FieldUpdated<PMLaborCostRate, PMLaborCostRate.inventoryID> e)
		{
			if (string.IsNullOrEmpty(e.Row.Description))
				e.Cache.SetDefaultExt<PMLaborCostRate.description>(e.Row);

			if (e.Row.EffectiveDate == null)
				e.Cache.SetDefaultExt<PMLaborCostRate.effectiveDate>(e.Row);
        }

        protected virtual void _(Events.FieldSelecting<PMLaborCostRate, PMLaborCostRate.uOM> e)
        {
            if (e.Row != null)
            {
                var select = new PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>(this);
                InventoryItem item = select.Select(e.Row.InventoryID);
                if (item != null)
                {
                    e.ReturnValue = item.BaseUnit;
                }    
                else
                {
                    e.ReturnValue = EPSetup.Hour;
                }
            }
        }

        protected virtual void _(Events.FieldDefaulting<PMLaborCostRate, PMLaborCostRate.description> e)
		{
			if (e.Row != null)
			{
				if (e.Row.InventoryID != null)
				{
					var select = new PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>(this);
					InventoryItem item = select.Select(e.Row.InventoryID);

					if (item != null)
					{
						e.NewValue = item.Descr;
					}
				}
			}
		}
				
		protected virtual void _(Events.FieldUpdated<PMLaborCostRate, PMLaborCostRate.employmentType> e)
		{
			if (e.Row.EmploymentType == EP.RateTypesAttribute.Hourly)
			{
				e.Row.AnnualSalary = null;
			}

			e.Cache.SetDefaultExt<PMLaborCostRate.regularHours>(e.Row);

		}

		protected virtual void _(Events.FieldDefaulting<PMLaborCostRate, PMLaborCostRate.regularHours> e)
		{
			if (e.Row != null && e.Row.EmployeeID != null && e.Row.Type == PMLaborCostRateType.Employee)
			{
				EmployeeCostEngine engine = new EmployeeCostEngine(this);
				e.NewValue = engine.GetEmployeeHoursFromCalendar(e.Row.EmployeeID);
			}
		}

		protected virtual void _(Events.FieldUpdated<PMLaborCostRate, PMLaborCostRate.regularHours> e)
		{
			if (e.Row.EmploymentType == RateTypesAttribute.Salary || e.Row.EmploymentType == RateTypesAttribute.SalaryWithExemption)
				e.Row.Rate = CalculateHourlyRate(e.Row.RegularHours, e.Row.AnnualSalary);
		}

		protected virtual void _(Events.FieldUpdated<PMLaborCostRate, PMLaborCostRate.annualSalary> e)
		{
			if (e.Row.EmploymentType == RateTypesAttribute.Salary || e.Row.EmploymentType == RateTypesAttribute.SalaryWithExemption)
				e.Row.Rate = CalculateHourlyRate(e.Row.RegularHours, e.Row.AnnualSalary);
		}

		protected virtual void _(Events.FieldDefaulting<PMLaborCostRate, PMLaborCostRate.effectiveDate> e)
		{
			if (e.Row != null)
			{
				PMLaborCostRate latestDuplicate = GetLatestDuplicate(e.Row);

				if (latestDuplicate != null && latestDuplicate.EffectiveDate != null)
				{
					if (latestDuplicate.EffectiveDate.Value.Date < Accessinfo.BusinessDate)
						e.NewValue = Accessinfo.BusinessDate;
				}
				else if(AllParametersEntered(e.Row))
				{
					DateTime? minDate = GetMinEffectiveDate(e.Row);
					if (minDate != null)
					{
						e.NewValue = minDate;
					}
					else
					{
						DateTime? startDate = GetProjectOrEmployeeStartDate(e.Row);
						if (startDate != null)
						{
							e.NewValue = startDate;
						}
						else
						{
							e.NewValue = Accessinfo.BusinessDate;
						}
					}
				}
				else
				{					
					e.NewValue = Accessinfo.BusinessDate;
				}
			}
		}

		protected virtual void _(Events.FieldVerifying<PMLaborCostRate, PMLaborCostRate.effectiveDate> e)
		{
			PMLaborCostRate latestDuplicate = GetLatestDuplicate(e.Row);
			DateTime? newValue = (DateTime?)e.NewValue;

			if (latestDuplicate != null && latestDuplicate.EffectiveDate != null)
			{
				if (newValue != null && newValue.Value.Date <= latestDuplicate.EffectiveDate.Value.Date )
				{
					throw new PXSetPropertyException(Messages.EffectiveDateShouldBeGreater, latestDuplicate.EffectiveDate);
				}
			}
			else
			{
				DateTime? minEffectiveDate = GetMinEffectiveDate(e.Row);
				if (newValue != null && minEffectiveDate != null && newValue.Value.Date > minEffectiveDate.Value.Date)
				{
					throw new PXSetPropertyException(Messages.EffectiveDateShouldBeLess, minEffectiveDate.Value.AddDays(1));
				}
			}
		}

		protected virtual void _(Events.FieldSelecting<PMLaborCostRate, PMLaborCostRate.taskID> e)
		{
			if (e.Row != null && e.Row.Type == PMLaborCostRateType.Certified)
			{
				e.ReturnState = PXStringState.CreateInstance(null, null, null, nameof(PMLaborCostRate.TaskID), false, null, null, null, null, null, null);
				PXFieldState ss = e.ReturnState as PXFieldState;
				ss.Enabled = false;
				ss.Visible = PXAccess.FeatureInstalled<FeaturesSet.projectModule>();
				ss.Visibility = PXAccess.FeatureInstalled<FeaturesSet.projectModule>() ? PXUIVisibility.Visible : PXUIVisibility.Invisible;
				e.Cancel = true;
			}
			else if (e.Row == null)
			{
				//For Excel Import - make field as editable in order to map the field.
				e.ReturnState = PXStringState.CreateInstance(null, null, null, nameof(PMLaborCostRate.TaskID), false, null, null, null, null, null, null);
				PXFieldState ss = e.ReturnState as PXFieldState;
				ss.Enabled = true;
				ss.Visible = PXAccess.FeatureInstalled<FeaturesSet.projectModule>();
				ss.Visibility = PXAccess.FeatureInstalled<FeaturesSet.projectModule>() ? PXUIVisibility.Visible : PXUIVisibility.Invisible;
				ss.DisplayName = PXUIFieldAttribute.GetDisplayName<PMLaborCostRate.taskID>(e.Cache);
				e.Cancel = true;
			}
		}

		public virtual DateTime? GetMinEffectiveDate(PMLaborCostRate row)
		{
			if (row == null)
				return null;

			if (!AllParametersEntered(row))
				return null;

			DateTime? result = null;

			EPEmployee employee = PXSelect<EPEmployee, Where<EPEmployee.bAccountID, Equal<Required<EPEmployee.bAccountID>>>>.Select(this, row.EmployeeID);
			if (employee != null)
			{
				PMTimeActivity firstUnreleased = PXSelect<PMTimeActivity, Where<PMTimeActivity.ownerID, Equal<Required<PMTimeActivity.ownerID>>,
					And<PMTimeActivity.released, NotEqual<True>>>,
					OrderBy<Asc<PMTimeActivity.date>>>.SelectWindowed(this, 0, 1, employee.OwnerID);

				if (firstUnreleased != null)
				{
					if (firstUnreleased.Date < result)
						result = firstUnreleased.Date;
				}
			}

			return result;
		}

		public virtual DateTime? GetProjectOrEmployeeStartDate(PMLaborCostRate row)
		{
			if (row == null)
				return null;

			if (!AllParametersEntered(row))
				return null;

			DateTime? result = null;

			DateTime? employeeDate = null;
			DateTime? projectDate = null;

			if (row.EmployeeID != null)
			{
				EPEmployeePosition activePosition = PXSelect<EPEmployeePosition, Where<EPEmployeePosition.employeeID, Equal<Required<EPEmployeePosition.employeeID>>,
					And<EPEmployeePosition.isActive, Equal<True>>>>.Select(this, row.EmployeeID);

				if (activePosition != null)
				{
					employeeDate = activePosition.StartDate;
				}
			}

			if (row.ProjectID != null)
			{
				PMProject project = PXSelect<PMProject, Where<PMProject.contractID, Equal<Required<PMProject.contractID>>>>.Select(this, row.ProjectID);
				if (project != null)
				{
					projectDate = project.StartDate;
				}
			}

			if (employeeDate != null && projectDate != null)
			{
				if (employeeDate > projectDate)
					result = employeeDate;
				else
					result = projectDate;
			}
			else
			{
				result = employeeDate ?? projectDate;
			}
						
			return result;
		}
			
		public virtual bool AllParametersEntered(PMLaborCostRate row)
		{
			if (row.Type == PMLaborCostRateType.Employee)
			{
				return row.EmployeeID != null;
			}
			else if (row.Type == PMLaborCostRateType.Union)
			{
				return row.UnionID != null && row.InventoryID != null;
			}
			else if (row.Type == PMLaborCostRateType.Certified)
			{
				return row.ProjectID != null && row.InventoryID != null;
			}
			else if (row.Type == PMLaborCostRateType.Project)
			{
				return row.ProjectID != null;
			}
			else if (row.Type == PMLaborCostRateType.Item)
			{
				return row.InventoryID != null;
			}

			return false;
		}

		public virtual PMLaborCostRate GetLatestDuplicate(PMLaborCostRate row)
		{
			if (row == null)
				return null;

			PMLaborCostRate existing = null;

			if (row.Type == PMLaborCostRateType.Employee)
			{
				if (row.InventoryID != null)
				{
					existing = PXSelect<PMLaborCostRate, Where<PMLaborCostRate.employeeID, Equal<Required<PMLaborCostRate.employeeID>>,
								And<PMLaborCostRate.inventoryID, Equal<Required<PMLaborCostRate.inventoryID>>,
								And<PMLaborCostRate.type, Equal<Required<PMLaborCostRate.type>>,
								And<PMLaborCostRate.recordID, NotEqual<Required<PMLaborCostRate.recordID>>>>>>,
								OrderBy<Desc<PMLaborCostRate.effectiveDate>>>.SelectWindowed(this, 0, 1, row.EmployeeID, row.InventoryID, row.Type, row.RecordID);
				}
				else
				{
					existing = PXSelect<PMLaborCostRate, Where<PMLaborCostRate.employeeID, Equal<Required<PMLaborCostRate.employeeID>>,
							And<PMLaborCostRate.inventoryID, IsNull,
							And<PMLaborCostRate.type, Equal<Required<PMLaborCostRate.type>>,
							And<PMLaborCostRate.recordID, NotEqual<Required<PMLaborCostRate.recordID>>>>>>,
							OrderBy<Desc<PMLaborCostRate.effectiveDate>>>.SelectWindowed(this, 0, 1, row.EmployeeID, row.Type, row.RecordID);
				}
			}
			else if (row.Type == PMLaborCostRateType.Union)
			{
				existing = PXSelect<PMLaborCostRate, Where<PMLaborCostRate.unionID, Equal<Required<PMLaborCostRate.unionID>>,
							And<PMLaborCostRate.inventoryID, Equal<Required<PMLaborCostRate.inventoryID>>,
							And<PMLaborCostRate.type, Equal<Required<PMLaborCostRate.type>>,
							And<PMLaborCostRate.recordID, NotEqual<Required<PMLaborCostRate.recordID>>>>>>,
							OrderBy<Desc<PMLaborCostRate.effectiveDate>>>.SelectWindowed(this, 0, 1, row.UnionID, row.InventoryID, row.Type, row.RecordID);
			}
			else if (row.Type == PMLaborCostRateType.Certified)
			{
					existing = PXSelect<PMLaborCostRate, Where<PMLaborCostRate.projectID, Equal<Required<PMLaborCostRate.projectID>>,
						And<PMLaborCostRate.taskID, IsNull,
						And<PMLaborCostRate.inventoryID, Equal<Required<PMLaborCostRate.inventoryID>>,
						And<PMLaborCostRate.type, Equal<Required<PMLaborCostRate.type>>,
						And<PMLaborCostRate.recordID, NotEqual<Required<PMLaborCostRate.recordID>>>>>>>,
						OrderBy<Desc<PMLaborCostRate.effectiveDate>>>.SelectWindowed(this, 0, 1, row.ProjectID,  row.InventoryID, row.Type,row.RecordID);
			}
			else if (row.Type == PMLaborCostRateType.Project)
			{
				List<object> args = null;
				PXSelectBase<PMLaborCostRate> select = null;

				if (row.InventoryID != null)
				{
					select = new PXSelect<PMLaborCostRate, Where<PMLaborCostRate.projectID, Equal<Required<PMLaborCostRate.projectID>>,
					And<PMLaborCostRate.inventoryID, Equal<Required<PMLaborCostRate.inventoryID>>,
					And<PMLaborCostRate.type, Equal<Required<PMLaborCostRate.type>>,
					And<PMLaborCostRate.recordID, NotEqual<Required<PMLaborCostRate.recordID>>>>>>,
					OrderBy<Desc<PMLaborCostRate.effectiveDate>>>(this);

					args = new List<object>(new object[] { row.ProjectID, row.InventoryID, row.Type, row.RecordID });
				}
				else
				{
					select = new PXSelect<PMLaborCostRate, Where<PMLaborCostRate.projectID, Equal<Required<PMLaborCostRate.projectID>>,
					And<PMLaborCostRate.inventoryID, IsNull,
					And<PMLaborCostRate.type, Equal<Required<PMLaborCostRate.type>>,
					And<PMLaborCostRate.recordID, NotEqual<Required<PMLaborCostRate.recordID>>>>>>,
					OrderBy<Desc<PMLaborCostRate.effectiveDate>>>(this);

					args = new List<object>(new object[] { row.ProjectID, row.Type, row.RecordID });
				}
				
				if (row.TaskID != null)
				{
					select.WhereAnd<Where<PMLaborCostRate.taskID, Equal<Required<PMLaborCostRate.taskID>>>>();
					args.Add(row.TaskID);
				}
				else
				{
					select.WhereAnd<Where<PMLaborCostRate.taskID, IsNull>>();
				}

				if (row.EmployeeID != null)
				{
					select.WhereAnd<Where<PMLaborCostRate.employeeID, Equal<Required<PMLaborCostRate.employeeID>>>>();
					args.Add(row.EmployeeID);
				}
				else
				{
					select.WhereAnd<Where<PMLaborCostRate.employeeID, IsNull>>();
				}

				existing = select.SelectWindowed(0, 1, args.ToArray());
			}
			else if (row.Type == PMLaborCostRateType.Item)
			{
				existing = PXSelect<PMLaborCostRate, Where<PMLaborCostRate.inventoryID, Equal<Required<PMLaborCostRate.inventoryID>>,
						And<PMLaborCostRate.type, Equal<Required<PMLaborCostRate.type>>,
						And<PMLaborCostRate.recordID, NotEqual<Required<PMLaborCostRate.recordID>>>>>,
						OrderBy<Desc<PMLaborCostRate.effectiveDate>>>.SelectWindowed(this, 0, 1, row.InventoryID, row.Type, row.RecordID);

			}

			return existing;
		}

		protected virtual void _(Events.RowPersisting<PMLaborCostRate> e)
		{
			if (e.Operation != PXDBOperation.Delete)
			{
				if (e.Row.Type == PMLaborCostRateType.Certified || e.Row.Type == PMLaborCostRateType.Project)
				{
					if (e.Row.ProjectID == null)
					{
						e.Cache.RaiseExceptionHandling<PMLaborCostRate.projectID>(e.Row, null, new PXSetPropertyException<PMLaborCostRate.projectID>(Data.ErrorMessages.FieldIsEmpty, nameof(PMLaborCostRate.ProjectID)));
						throw new PXRowPersistingException(nameof(PMLaborCostRate.ProjectID), null, ErrorMessages.FieldIsEmpty, nameof(PMLaborCostRate.ProjectID));
					}
				}
				else if (e.Row.Type == PMLaborCostRateType.Employee)
				{
					if (e.Row.EmployeeID == null)
					{
						e.Cache.RaiseExceptionHandling<PMLaborCostRate.employeeID>(e.Row, null, new PXSetPropertyException<PMLaborCostRate.employeeID>(Data.ErrorMessages.FieldIsEmpty, nameof(PMLaborCostRate.EmployeeID)));
						throw new PXRowPersistingException(nameof(PMLaborCostRate.EmployeeID), null, ErrorMessages.FieldIsEmpty, nameof(PMLaborCostRate.EmployeeID));
					}

					if (e.Row.RegularHours == null)
					{
						e.Cache.RaiseExceptionHandling<PMLaborCostRate.regularHours>(e.Row, null, new PXSetPropertyException<PMLaborCostRate.regularHours>(Data.ErrorMessages.FieldIsEmpty, nameof(PMLaborCostRate.RegularHours)));
						throw new PXRowPersistingException(nameof(PMLaborCostRate.RegularHours), null, ErrorMessages.FieldIsEmpty, nameof(PMLaborCostRate.EmployeeID));
					}
				}
				else if (e.Row.Type == PMLaborCostRateType.Union)
				{
					if (e.Row.UnionID == null)
					{
						e.Cache.RaiseExceptionHandling<PMLaborCostRate.unionID>(e.Row, null, new PXSetPropertyException<PMLaborCostRate.unionID>(Data.ErrorMessages.FieldIsEmpty, nameof(PMLaborCostRate.UnionID)));
						throw new PXRowPersistingException(nameof(PMLaborCostRate.UnionID), null, ErrorMessages.FieldIsEmpty, nameof(PMLaborCostRate.UnionID));
					}
				}

				if (e.Row.InventoryID == null && e.Row.Type != PMLaborCostRateType.Employee && e.Row.Type != PMLaborCostRateType.Project)
				{
					e.Cache.RaiseExceptionHandling<PMLaborCostRate.inventoryID>(e.Row, null, new PXSetPropertyException<PMLaborCostRate.inventoryID>(Data.ErrorMessages.FieldIsEmpty, nameof(PMLaborCostRate.ProjectID)));
					throw new PXRowPersistingException(nameof(PMLaborCostRate.InventoryID), null, ErrorMessages.FieldIsEmpty, nameof(PMLaborCostRate.InventoryID));
				}
			}
		}
		
		public virtual decimal CalculateHourlyRate(decimal? hours, decimal? salary)
		{
			if (hours.GetValueOrDefault() == 0)
				return 0;

			const decimal weeksInYear = 52;
			return Math.Round( salary.GetValueOrDefault() / weeksInYear / hours.Value, 2, MidpointRounding.ToEven);
		}

		#region PMImport Implementation
		public bool PrepareImportRow(string viewName, IDictionary keys, IDictionary values)
		{
			return true;
		}

		public bool RowImporting(string viewName, object row)
		{
			return row == null;
		}

		public bool RowImported(string viewName, object row, object oldRow)
		{
			return oldRow == null;
		}

		public void PrepareItems(string viewName, IEnumerable items) { }
		#endregion

		#region Local Types
		[PXHidden]
		[Serializable]
		[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
		public partial class PMLaborCostRateFilter : IBqlTable
		{
			#region Type
			public abstract class type : PX.Data.BQL.BqlString.Field<type> { }
			[PXDBString(1)]
			[PXDefault(PMLaborCostRateType.All)]
			[PMLaborCostRateType.FilterList]
			[PXUIField(DisplayName = "Labor Rate Type")]
			public virtual string Type
			{
				get; set;
			}
			#endregion
			#region UnionID
			public abstract class unionID : PX.Data.BQL.BqlString.Field<unionID> { }
			[PXRestrictor(typeof(Where<PMUnion.isActive, Equal<True>>), Messages.InactiveUnion, typeof(PMUnion.unionID))]
			[PXSelector(typeof(Search<PMUnion.unionID>))]
			[PXDBString(PMUnion.unionID.Length, IsUnicode = true)]
			[PXUIField(DisplayName = "Union Local", FieldClass = nameof(FeaturesSet.Construction))]
			public virtual String UnionID
			{
				get;
				set;
			}
			#endregion
			#region ProjectID
			public abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }
			[Project(typeof(Where<PMProject.baseType, Equal<CTPRType.project>, And<PMProject.nonProject, NotEqual<True>>>), WarnIfCompleted = false)]
			public virtual Int32? ProjectID
			{
				get;
				set;
			}
			#endregion
			#region TaskID
			public abstract class taskID : PX.Data.BQL.BqlInt.Field<taskID> { }

			[ProjectTask(typeof(projectID))]
			public virtual Int32? TaskID
			{
				get;
				set;
			}
			#endregion
			#region EmployeeID
			public abstract class employeeID : PX.Data.BQL.BqlInt.Field<employeeID> { }
			[EP.PXEPEmployeeSelector]
			[PXDBInt()]
			[PXUIField(DisplayName = "Employee")]
			public virtual Int32? EmployeeID
			{
				get;
				set;
			}
			#endregion
			#region InventoryID
			public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
			protected Int32? _InventoryID;
			[PXDBInt()]
			[PXUIField(DisplayName = "Labor Item")]
			[PXDimensionSelector(InventoryAttribute.DimensionName, typeof(Search<InventoryItem.inventoryID, Where<InventoryItem.itemType, Equal<INItemTypes.laborItem>, And<Match<Current<AccessInfo.userName>>>>>), typeof(InventoryItem.inventoryCD))]
			public virtual Int32? InventoryID
			{
				get
				{
					return this._InventoryID;
				}
				set
				{
					this._InventoryID = value;
				}
			}
			#endregion
			#region EffectiveDate
			public abstract class effectiveDate : PX.Data.BQL.BqlDateTime.Field<effectiveDate> { }
			[PXDBDate()]
			[PXUIField(DisplayName = "Effective Date")]
			public virtual DateTime? EffectiveDate
			{
				get;
				set;
			}
			#endregion
		}
		#endregion
	}
}
