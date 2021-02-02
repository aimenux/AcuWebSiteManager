using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Common;
using PX.Data;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.PM;

namespace PX.Objects.EP
{

    public class EmployeeCostEngine
	{
		protected PXGraph graph;
		protected string defaultUOM = EPSetup.Hour;
		
		public EmployeeCostEngine(PXGraph graph)
		{
			if ( graph == null )
				throw new ArgumentNullException();

			this.graph = graph;

			EPSetup setup = PXSelect<EPSetup>.Select(graph);
            if (setup != null && !String.IsNullOrEmpty(setup.EmployeeRateUnit))
			    defaultUOM = setup.EmployeeRateUnit;
		}


		public class Rate
		{
			public int? EmployeeID { get; private set; }
			public decimal HourlyRate { get; private set; }
			public string Type { get; private set; }
			public string UOM { get; private set; }

            /// <summary>
            /// Total Hours in week
            /// </summary>
			public decimal? RegularHours { get; private set; }

            /// <summary>
            /// Employee Rate for a Week
            /// </summary>
			public decimal? RateByType { get; private set; }
			
			public Rate(int? employeeID, string type, decimal? hourlyRate, string uom, decimal? regularHours, decimal? rateByType)
			{
				this.EmployeeID = employeeID;
				this.UOM = uom;
				this.HourlyRate = hourlyRate ?? 0m;
				this.Type = string.IsNullOrEmpty(type) ? RateTypesAttribute.Hourly : type;
				this.RegularHours = regularHours ?? 0;
				this.RateByType = rateByType;
			}
		}
		
		public class LaborCost
		{
			public decimal? Rate { get; private set; }
			public decimal? OvertimeMultiplier { get; private set; }

			public LaborCost(decimal? rate, decimal? mult)
			{
				this.Rate = rate;
				this.OvertimeMultiplier = mult;
			}
		}


		public virtual Rate GetEmployeeRate(int? laborItemID, int? projectID, int? projectTaskID, bool? certifiedJob, string unionID, int? employeeId, DateTime? date)
		{
			if (date == null)
				return null;

			var select = new PXSelect<PMLaborCostRate, 
				Where2<Where<PMLaborCostRate.inventoryID, Equal<Required<PMLaborCostRate.inventoryID>>, Or<PMLaborCostRate.inventoryID, IsNull>>,
				And2<Where<PMLaborCostRate.employeeID, Equal<Required<PMLaborCostRate.employeeID>>, Or<PMLaborCostRate.employeeID, IsNull>>,
				And2<Where<PMLaborCostRate.projectID, Equal<Required<PMLaborCostRate.projectID>>, Or<PMLaborCostRate.projectID, IsNull>>,
				And<Where<PMLaborCostRate.taskID, Equal<Required<PMLaborCostRate.taskID>>, Or<PMLaborCostRate.taskID, IsNull>>>>>>,
				OrderBy<Asc<PMLaborCostRate.effectiveDate>>>(graph);

			PXResultset<PMLaborCostRate> resultset = select.Select(laborItemID, employeeId, projectID, projectTaskID);

			List<PMLaborCostRate> specificRates = new List<PMLaborCostRate>();
			
			PMLaborCostRate certifiedRate = null;
			PMLaborCostRate unionRate = null;
			
			
			PMLaborCostRate specific1 = null;//Labor rate by project, project task, employee, inventory ID
			PMLaborCostRate specific2 = null;//Labor rate by project, project task, employee, empty inventory ID
			PMLaborCostRate specific3 = null;//Labor rate by project, project task, inventory ID
			PMLaborCostRate specific4 = null;//Labor rate by project, project task, empty inventory ID
			PMLaborCostRate specific5 = null;//Labor rate by project, employee, inventory ID
			PMLaborCostRate specific6 = null;//Labor rate by project, employee, empty inventory ID
			PMLaborCostRate specific7 = null;//Labor rate by project, inventory ID
			PMLaborCostRate specific8 = null;//Labor rate by project, empty inventory ID
			PMLaborCostRate employeeRate1 = null; // With LaborItem
			PMLaborCostRate employeeRate2 = null; // Without LaborItem
			PMLaborCostRate itemRate = null;

			foreach (PMLaborCostRate record in resultset)
			{
				if (record.EffectiveDate > date)
					continue;

				if (record.Type == PMLaborCostRateType.Certified)
				{
					if (certifiedRate == null)
					{
						certifiedRate = record;
					}
					else
					{
						if (record.EffectiveDate > certifiedRate.EffectiveDate)
						{
							certifiedRate = record;
						}
					}
				}
				else if (record.Type == PMLaborCostRateType.Union && !string.IsNullOrEmpty(unionID) && record.UnionID == unionID)
				{
					if (unionRate == null)
					{
						unionRate = record;
					}
					else
					{
						if (record.EffectiveDate > unionRate.EffectiveDate)
						{
							unionRate = record;
						}
					}
				}
				else if (record.Type == PMLaborCostRateType.Item)
				{
					if (itemRate == null)
					{
						itemRate = record;
					}
					else
					{
						if (record.EffectiveDate > itemRate.EffectiveDate)
						{
							itemRate = record;
						}
					}
				}
				else if (record.Type == PMLaborCostRateType.Employee)
				{
					if (record.InventoryID != null)
					{
						if (employeeRate1 == null)
						{
							employeeRate1 = record;
						}
						else
						{
							if (record.EffectiveDate > employeeRate1.EffectiveDate)
							{
								employeeRate1 = record;
							}
						}
					}
					else
					{
						if (employeeRate2 == null)
						{
							employeeRate2 = record;
						}
						else
						{
							if (record.EffectiveDate > employeeRate2.EffectiveDate)
							{
								employeeRate2 = record;
							}
						}
					}
				}
				else
				{
					if (record.ProjectID == projectID && record.TaskID == projectTaskID && record.EmployeeID == employeeId && record.InventoryID != null)
					{
						if (specific1 == null)
						{
							specific1 = record;
						}
						else
						{
							if (record.EffectiveDate > specific1.EffectiveDate)
							{
								specific1 = record;
							}
						}
					}
					else if (record.ProjectID == projectID && record.TaskID == projectTaskID && record.EmployeeID == employeeId && record.InventoryID == null)
					{
						if (specific2 == null)
						{
							specific2= record;
						}
						else
						{
							if (record.EffectiveDate > specific2.EffectiveDate)
							{
								specific2 = record;
							}
						}
					}
					else if (record.ProjectID == projectID && record.TaskID == projectTaskID && record.EmployeeID == null && record.InventoryID != null)
					{
						if (specific3 == null)
						{
							specific3 = record;
						}
						else
						{
							if (record.EffectiveDate > specific3.EffectiveDate)
							{
								specific3 = record;
							}
						}
					}
					else if (record.ProjectID == projectID && record.TaskID == projectTaskID && record.EmployeeID == null && record.InventoryID == null)
					{
						if (specific4 == null)
						{
							specific4 = record;
						}
						else
						{
							if (record.EffectiveDate > specific4.EffectiveDate)
							{
								specific4 = record;
							}
						}
					}
					else if (record.ProjectID == projectID && record.EmployeeID == employeeId && record.InventoryID != null)
					{
						if (specific5 == null)
						{
							specific5 = record;
						}
						else
						{
							if (record.EffectiveDate > specific5.EffectiveDate)
							{
								specific5 = record;
							}
						}
					}
					else if (record.ProjectID == projectID && record.EmployeeID == employeeId && record.InventoryID == null)
					{
						if (specific6 == null)
						{
							specific6 = record;
						}
						else
						{
							if (record.EffectiveDate > specific6.EffectiveDate)
							{
								specific6 = record;
							}
						}
					}
					else if (record.ProjectID == projectID && record.EmployeeID == null && record.InventoryID != null)
					{
						if (specific7 == null)
						{
							specific7 = record;
						}
						else
						{
							if (record.EffectiveDate > specific7.EffectiveDate)
							{
								specific7 = record;
							}
						}
					}
					else if (record.ProjectID == projectID && record.EmployeeID == null && record.InventoryID == null)
					{
						if (specific8 == null)
						{
							specific8 = record;
						}
						else
						{
							if (record.EffectiveDate > specific8.EffectiveDate)
							{
								specific8 = record;
							}
						}
					}
				}
			}

			PMLaborCostRate specific = specific1;
			if (specific == null) specific = specific2;
			if (specific == null) specific = specific3;
			if (specific == null) specific = specific4;
			if (specific == null) specific = specific5;
			if (specific == null) specific = specific6;
			if (specific == null) specific = specific7;
			if (specific == null) specific = specific8;
			if (specific == null) specific = employeeRate1;
			if (specific == null) specific = employeeRate2;
			if (specific == null) specific = itemRate;

			PMLaborCostRate maxRate = specific;
			if (maxRate == null)
			{
				maxRate = unionRate;
			}
			else if (unionRate != null && maxRate.Rate < unionRate.Rate)
			{
				maxRate = unionRate;
			}

			if (certifiedJob == true)
			{
				if (maxRate == null)
				{
					maxRate = certifiedRate;
				}
				else if (certifiedRate != null && maxRate.Rate < certifiedRate.Rate)
				{
					maxRate = certifiedRate;
				}
			}

			if (maxRate == null)
			{
				return null;
			}

			decimal? regularHours = maxRate.RegularHours;
			if (employeeRate1 != null)
				regularHours = employeeRate1.RegularHours;

			return new Rate(employeeId, maxRate.EmploymentType, maxRate.Rate, defaultUOM, regularHours, 
				maxRate.EmploymentType == RateTypesAttribute.Hourly ? maxRate.Rate : maxRate.Rate * regularHours );
		}
		
       	public virtual LaborCost CalculateEmployeeCost(string timeCardCD, string earningTypeID, int? laborItemID, int? projectID, int? projectTaskID, bool? certifiedJob, string unionID, int? employeeID, DateTime date)
		{
			decimal? cost;
			decimal overtimeMult = 1;
			Rate employeeRate = GetEmployeeRate(laborItemID, projectID, projectTaskID, certifiedJob, unionID, employeeID, date);

			if (employeeRate == null)
				return null;
			
			if (employeeRate.Type == RateTypesAttribute.SalaryWithExemption && timeCardCD != null)
			{
				//Overtime is not applicable. Rate is prorated based on actual hours worked on the given week

				EPTimeCard timecard = PXSelect<EPTimeCard, 
					Where<EPTimeCard.timeCardCD, Equal<Required<PMTimeActivity.timeCardCD>>>>.Select(graph, timeCardCD);

				if (timecard.TotalSpentCalc == null)
				{
					var select = new PXSelectGroupBy<PMTimeActivity, Where<PMTimeActivity.timeCardCD, Equal<Required<PMTimeActivity.timeCardCD>>>, Aggregate<Sum<PMTimeActivity.timeSpent>>>(graph);
					PMTimeActivity total = select.Select(timeCardCD);

					timecard.TotalSpentCalc = total.TimeSpent;
				}

				if (timecard.TotalSpentCalc <= employeeRate.RegularHours * 60m)
                {
					cost = employeeRate.RateByType / employeeRate.RegularHours;
				}
                else
                {
					cost = employeeRate.RateByType / (timecard.TotalSpentCalc / 60m);
                }
			}
			else
			{
				overtimeMult = GetOvertimeMultiplier(earningTypeID, employeeRate);
				cost = employeeRate.HourlyRate * overtimeMult;
			}

			return new LaborCost(cost, overtimeMult);
		}

		public virtual decimal GetOvertimeMultiplier(string earningTypeID, Rate employeeRate)
		{
			if (employeeRate != null && employeeRate.Type == RateTypesAttribute.SalaryWithExemption)
				return 1;

			EPEarningType earningType = PXSelect<EPEarningType>.Search<EPEarningType.typeCD>(graph, earningTypeID);
			return earningType != null && earningType.IsOvertime == true ? (decimal)earningType.OvertimeMultiplier : 1;
		}

		public virtual decimal GetOvertimeMultiplier(string earningTypeID, int? employeeID, int? laborItemID, DateTime date)
		{
			Rate employeeBaseRate = GetEmployeeRate(laborItemID, null, null, false, null, employeeID, date);

			return GetOvertimeMultiplier(earningTypeID, employeeBaseRate);
		}

		public virtual decimal GetEmployeeRegularHours(int? employeeID, DateTime? effectiveDate)
		{
			var select = new PXSelect<PMLaborCostRate,
				Where<PMLaborCostRate.employeeID, Equal<Required<PMLaborCostRate.employeeID>>,
				And<PMLaborCostRate.type, Equal<PMLaborCostRateType.employee>,
				And<PMLaborCostRate.effectiveDate, LessEqual<Required<PMLaborCostRate.effectiveDate>>>>>,
			    OrderBy<Desc<PMLaborCostRate.effectiveDate>>>(graph);

			PMLaborCostRate res = select.Select(employeeID, effectiveDate);
			if (res != null && res.RegularHours != null)
			{
				return res.RegularHours.Value;
			}
			else
			{
				return GetEmployeeHoursFromCalendar(employeeID);
			}

		}

		public virtual decimal GetEmployeeHoursFromCalendar(int? employeeID)
		{
			decimal hours = 40;
			var select = new PXSelect<EPEmployee, Where<EPEmployee.bAccountID, Equal<Required<EPEmployee.bAccountID>>>>(graph);
			EPEmployee employee = select.Select(employeeID);

			if (employee != null && employee.CalendarID != null)
			{
				CSCalendar calendar = PXSelect<CSCalendar, Where<CSCalendar.calendarID, Equal<Required<CSCalendar.calendarID>>>>.Select(graph, employee.CalendarID);
				if (calendar != null)
				{
					hours = 0;
					hours += CalendarHelper.GetHoursWorkedOnDay(calendar, DayOfWeek.Monday);
					hours += CalendarHelper.GetHoursWorkedOnDay(calendar, DayOfWeek.Tuesday);
					hours += CalendarHelper.GetHoursWorkedOnDay(calendar, DayOfWeek.Wednesday);
					hours += CalendarHelper.GetHoursWorkedOnDay(calendar, DayOfWeek.Thursday);
					hours += CalendarHelper.GetHoursWorkedOnDay(calendar, DayOfWeek.Friday);
					hours += CalendarHelper.GetHoursWorkedOnDay(calendar, DayOfWeek.Saturday);
					hours += CalendarHelper.GetHoursWorkedOnDay(calendar, DayOfWeek.Sunday);
				}
			}

			return hours;
		}

		public virtual int? GetLaborClass(PMTimeActivity activity)
		{
			CRCase refCase = PXSelectJoin<CRCase, 
				InnerJoin<CRActivityLink, 
					On<CRActivityLink.refNoteID, Equal<CRCase.noteID>>>,
				Where<CRActivityLink.noteID, Equal<Required<PMTimeActivity.refNoteID>>>>.Select(graph, activity.RefNoteID);

			EPEmployee employee = PXSelect<EPEmployee>.Search<EPEmployee.userID>(graph, activity.OwnerID);

			return GetLaborClass(activity, employee, refCase);
		}

		public virtual int? GetLaborClass(PMTimeActivity activity, EPEmployee employee, CRCase refCase)
		{
			if (employee == null)
				throw new ArgumentNullException("employee", Messages.EmptyEmployeeID);

			int? laborClassID = null;

			if (refCase != null)
			{
				CRCaseClass caseClass = (CRCaseClass)PXSelectorAttribute.Select<CRCase.caseClassID>(graph.Caches[typeof(CRCase)], refCase);
				if (caseClass.PerItemBilling == BillingTypeListAttribute.PerActivity)
				laborClassID = CRCaseClassLaborMatrix.GetLaborClassID(graph, caseClass.CaseClassID, activity.EarningTypeID);
			}

			if (laborClassID == null && activity.LabourItemID != null)
				laborClassID = activity.LabourItemID;
			if (laborClassID == null && activity.ProjectID != null && employee.BAccountID != null)
				laborClassID = EPContractRate.GetProjectLaborClassID(graph, (int)activity.ProjectID, (int)employee.BAccountID, activity.EarningTypeID);

			if (laborClassID == null)
				laborClassID = EPEmployeeClassLaborMatrix.GetLaborClassID(graph, employee.BAccountID, activity.EarningTypeID);

			if (laborClassID == null)
				laborClassID = employee.LabourItemID;

			return laborClassID;
		}



	}
}
