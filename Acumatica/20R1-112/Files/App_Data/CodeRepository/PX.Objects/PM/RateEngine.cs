using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security;
using System.Text;
using PX.Data;
using System.CodeDom.Compiler;
using PX.Objects.CS;

namespace PX.Objects.PM
{
	/// <summary>
	/// Rate Engine. Calculates @Rate to be used in the Allocation based on the Rate Tables and the attributes of a given transaction.
	/// </summary>
	/// 
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public class RateEngine
	{
		protected PXGraph graph;
		protected string rateTypeID;
		protected PMTran tran;
		protected StringBuilder trace;
		
		public RateEngine(PXGraph graph, string rateTypeID, PMTran tran)
		{
			if (graph == null)
				throw new ArgumentNullException(nameof(graph));

			if (string.IsNullOrEmpty(rateTypeID))
				throw new ArgumentNullException(nameof(rateTypeID), PXMessages.LocalizeNoPrefix(Messages.ArgumentIsNullOrEmpty));
						
			if (tran == null)
				throw new ArgumentNullException(nameof(tran));

			this.graph = graph;
			this.rateTypeID = rateTypeID;
			this.tran = tran;
		}

		/// <summary>
		/// Searches for Rate based on RateCode, PriceClass and Transaction properties.
		/// Rate is first searched for the given PriceClass; if not found it is then searched in BasePriceClass.
		/// </summary>
		public decimal? GetRate(string rateTableID)
		{
			if (string.IsNullOrEmpty(rateTableID))
                throw new ArgumentNullException(nameof(rateTableID), PXMessages.LocalizeNoPrefix(Messages.ArgumentIsNullOrEmpty));

		    trace = new StringBuilder();

            trace.AppendFormat("Calculating Rate. RateTable:{0}", rateTableID);

            PXSelectBase<PMRateDefinition> select = new PXSelect<PMRateDefinition,
                Where<PMRateDefinition.rateTableID, Equal<Required<PMRateDefinition.rateTableID>>,
                And<PMRateDefinition.rateTypeID, Equal<Required<PMRateDefinition.rateTypeID>>>>,
                OrderBy<Asc<PMRateDefinition.sequence>>>(graph);

            PXResultset<PMRateDefinition> rateDefinitions = select.Select(rateTableID, rateTypeID);

            foreach (PMRateDefinition rd in rateDefinitions)
            {
                trace.AppendFormat("Start Processing Sequence:{0}", rd.Description);
                decimal? rate = GetRate(rd);
                if (rate != null)
                {
                    trace.AppendFormat("End Processing Sequence. Rate Defined:{0}", rate);
                    return rate;
                }
                else
                {
                    trace.AppendFormat("End Processing Sequence. Rate Not Defined");
                }

            }
            return null;
		}

		public string GetTrace()
		{
			//Add transaction properties:
			PMAccountGroup ag = PXSelect<PMAccountGroup, Where<PMAccountGroup.groupID, Equal<Required<PMAccountGroup.groupID>>>>.Select(graph, tran.AccountGroupID);
			if (ag != null)
			{
				trace.AppendFormat(" PMTran.AccountGroup={0} ", ag.GroupCD);
			}

			IN.InventoryItem inventoryItem = PXSelect<IN.InventoryItem, Where<IN.InventoryItem.inventoryID, Equal<Required<IN.InventoryItem.inventoryID>>>>.Select(graph, tran.InventoryID);
			if (inventoryItem != null)
			{
				trace.AppendFormat(" PMTran.InventoryID={0} ", inventoryItem.InventoryCD);
			}
            
			return trace.ToString();
		}
        
		protected virtual decimal? GetRate(PMRateDefinition rd)
		{
			bool isApplicable = true;

			List<string> finalList = null;
			
			if (rd.Project == true)
			{
				List<string> rateCodes;
				if (!IsProjectFit(rd.RateDefinitionID, tran.ProjectID, out rateCodes))
				{
					isApplicable = false;
				}
				else
				{
					finalList = rateCodes;
				}
			}

			if (rd.Task == true)
			{
				List<string> rateCodes;
				if (!IsTaskFit(rd.RateDefinitionID, tran.ProjectID, tran.TaskID, out rateCodes))
				{
					isApplicable = false;
				}
				else
				{
					if (finalList == null)
					{
						finalList = rateCodes;
					}
					else
					{
						finalList = new List<string>(finalList.Intersect(rateCodes));
						if (finalList.Count == 0)
						{
							isApplicable = false;
						}
					}
				}
			}

			if (rd.AccountGroup == true)
			{
				List<string> rateCodes;
				if (!IsAccountGroupFit(rd.RateDefinitionID, tran.AccountGroupID, out rateCodes))
				{
					isApplicable = false;
				}
				else
				{
					if (finalList == null)
					{
						finalList = rateCodes;
					}
					else
					{
						finalList = new List<string>(finalList.Intersect(rateCodes));
						if (finalList.Count == 0)
						{
							isApplicable = false;
						}
					}
				}
			}

			if (rd.RateItem == true)
			{
				List<string> rateCodes;
				if (!IsItemFit(rd.RateDefinitionID, tran.InventoryID, out rateCodes))
				{
					isApplicable = false;
				}
				else
				{
					if (finalList == null)
					{
						finalList = rateCodes;
					}
					else
					{
						finalList = new List<string>(finalList.Intersect(rateCodes));
						if (finalList.Count == 0)
						{
							isApplicable = false;
						}
					}
				}
			}

			if (rd.Employee == true)
			{
				List<string> rateCodes;
				if (!IsEmployeeFit(rd.RateDefinitionID, tran.ResourceID, out rateCodes))
				{
					isApplicable = false;
				}
				else
				{
					if (finalList == null)
					{
						finalList = rateCodes;
					}
					else
					{
						finalList = new List<string>(finalList.Intersect(rateCodes));
						if (finalList.Count == 0)
						{
							isApplicable = false;
						}
					}
				}
			}

			if (isApplicable)
			{
				string ratecodeid = null;
				if (finalList != null && finalList.Count > 0)
				{
					ratecodeid = finalList[0];
					Debug.Assert(finalList.Count == 1, "Intersaction of all ratecodes must result to single element.");
				}
				
				
				PMRate rate;
				if (!string.IsNullOrEmpty(ratecodeid))
				{
				PXSelectBase<PMRate> select = new PXSelect<PMRate,
					Where<PMRate.rateDefinitionID, Equal<Required<PMRate.rateDefinitionID>>,
					And<Where<PMRate.rateCodeID, Equal<Required<PMRate.rateCodeID>>,
					And<Where<PMRate.startDate, LessEqual<Required<PMRate.startDate>>,
					And2<Where<PMRate.endDate, GreaterEqual<Required<PMRate.endDate>>>, Or<PMRate.endDate, IsNull>>>>>>>>(graph);

					rate = select.Select(rd.RateDefinitionID, ratecodeid, tran.Date, tran.Date);
				}
				else
				{
					PXSelectBase<PMRate> select = new PXSelect<PMRate,
						Where<PMRate.rateDefinitionID, Equal<Required<PMRate.rateDefinitionID>>,
						And<Where<PMRate.startDate, LessEqual<Required<PMRate.startDate>>,
						And2<Where<PMRate.endDate, GreaterEqual<Required<PMRate.endDate>>>, Or<PMRate.endDate, IsNull>>>>>>(graph);
					rate = select.Select(rd.RateDefinitionID, tran.Date, tran.Date);
				}
				trace.AppendFormat("	Searching Rate for Date:{0}", tran.Date);
                
				if (rate != null)
				{
					return rate.Rate;
				}
				else

					return null;
			}
			else
			{
				return null;
			}

		}

		protected virtual bool IsProjectFit(int? rateDefinitionID, int? projectID, out List<string> rateCodes)
		{
			rateCodes = new List<string>();
			PMProject project = PXSelect<PMProject, Where<PMProject.contractID, Equal<Required<PMProject.contractID>>>>.Select(graph, projectID);

			if (project == null)
				throw new PXException(Messages.ProjectNotFound, projectID);

			string cd = project.ContractCD;
			
			PXSelectBase<PMProjectRate> select = new PXSelect<PMProjectRate,
				Where<PMProjectRate.rateDefinitionID, Equal<Required<PMProjectRate.rateDefinitionID>>>>(graph);

			bool result = false;
			foreach (PMProjectRate item in select.Select(rateDefinitionID))
			{
				if (IsFit(item.ProjectCD.Trim(), cd.Trim()))
				{
					rateCodes.Add(item.RateCodeID);
					trace.AppendFormat("	Checking Project {0}..Match found.", cd.Trim());
					result = true;
				}
			}

			if (!result)
				trace.AppendFormat("	Checking Project {0}..Match not found.", cd.Trim());

			return result;
		}

		protected virtual bool IsTaskFit(int? rateDefinitionID, int? projectID, int? taskID, out List<string> rateCodes)
		{
			rateCodes = new List<string>();
			PMTask task = PXSelect<PMTask, Where<PMTask.projectID, Equal<Required<PMTask.projectID>>, And<PMTask.taskID, Equal<Required<PMTask.taskID>>>>>.Select(graph, projectID, taskID);
			if (task == null)
				throw new PXException(Messages.TaskNotFound, projectID, taskID);

			string cd = task.TaskCD;

			PXSelectBase<PMTaskRate> select = new PXSelect<PMTaskRate,
				Where<PMTaskRate.rateDefinitionID, Equal<Required<PMTaskRate.rateDefinitionID>>>>(graph);
			
			bool result = false; 
			foreach (PMTaskRate item in select.Select(rateDefinitionID))
			{
				if (IsFit(item.TaskCD.Trim(), cd.Trim()))
				{
					rateCodes.Add(item.RateCodeID);
					trace.AppendFormat("	Checking Task {0}..Match found.", cd.Trim());
					result = true;
				}
			}
			
			if (!result)
				trace.AppendFormat("	Checking Task {0}..Match not found.", cd.Trim());
			
			return result;
		}

		protected virtual bool IsAccountGroupFit(int? rateDefinitionID, int? accountGroupID, out List<string> rateCodes)
		{
			rateCodes = new List<string>();
			PXSelectBase<PMAccountGroupRate> select = new PXSelect<PMAccountGroupRate,
				Where<PMAccountGroupRate.rateDefinitionID, Equal<Required<PMAccountGroupRate.rateDefinitionID>>,
				And<PMAccountGroupRate.accountGroupID, Equal<Required<PMAccountGroupRate.accountGroupID>>>>>(graph);
				
			bool result = false;
			foreach (PMAccountGroupRate item in select.Select(rateDefinitionID, accountGroupID))
			{
				rateCodes.Add(item.RateCodeID);
				trace.AppendFormat("	Checking Account Group {0}..Match found.", accountGroupID);
				result = true;
			}
			
			if (!result)
				trace.AppendFormat("	Checking Account Group {0}..Match not found.", accountGroupID);

			return result;
		}

		protected virtual bool IsItemFit(int? rateDefinitionID, int? inventoryID, out List<string> rateCodes)
		{
			rateCodes = new List<string>();
			PXSelectBase<PMItemRate> select = new PXSelect<PMItemRate,
				Where<PMItemRate.rateDefinitionID, Equal<Required<PMItemRate.rateDefinitionID>>,
				And<PMItemRate.inventoryID, Equal<Required<PMItemRate.inventoryID>>>>>(graph);
			
			bool result = false;
			foreach (PMItemRate item in select.Select(rateDefinitionID, inventoryID))
			{
				rateCodes.Add(item.RateCodeID);
				trace.AppendFormat("	Checking Inventory Item {0}..Match found.", inventoryID);
				result = true;
			}

			if (!result)
				trace.AppendFormat("	Checking Inventory Item {0}..Match not found.", inventoryID);

			return result;
		}

		protected virtual bool IsEmployeeFit(int? rateDefinitionID, int? employeeID, out List<string> rateCodes)
		{
			rateCodes = new List<string>();
			PXSelectBase<PMEmployeeRate> select = new PXSelect<PMEmployeeRate,
				Where<PMEmployeeRate.rateDefinitionID, Equal<Required<PMEmployeeRate.rateDefinitionID>>,
				And<PMEmployeeRate.employeeID, Equal<Required<PMEmployeeRate.employeeID>>>>>(graph);
				
			bool result = false;
			foreach (PMEmployeeRate item in select.Select(rateDefinitionID, employeeID))
			{
				rateCodes.Add(item.RateCodeID);
				trace.AppendFormat("	Checking Employee {0}..Match found.", employeeID);
				result = true;
			}

			if (!result)
				trace.AppendFormat("	Checking Employee {0}..Match not found.", employeeID);

			return result;
		}

		protected virtual bool IsFit(string wildcard, string value)
		{
			if (value.Length < wildcard.Length)
			{
				value += new string(' ', wildcard.Length - value.Length);
			}
			else if (value.Length > wildcard.Length)
			{
				return false;
			}

			for (int i = 0; i < wildcard.Length; i++)
			{
				if (wildcard[i] == '?')
					continue;

				if (wildcard[i] != value[i])
					return false;
			}

			return true;
		}

	}

	public class RateEngineV2
	{
		protected PXGraph graph;
		protected Dictionary<string, List<PMRateDefinition>> definitions;
		protected StringBuilder trace;

		public RateEngineV2(PXGraph graph, IList<string> rateTables, IList<string> rateTypes)
		{
			this.graph = graph;
			definitions = new Dictionary<string, List<PMRateDefinition>>(rateTables.Count * rateTypes.Count);

			foreach (string rateTable in rateTables)
			{
				foreach (PMRateDefinition item in ((IRateTable)graph).GetRateDefinitions(rateTable))
				{
					if (rateTypes.Contains(item.RateTypeID))
					{
						string key = GetDefinitionKey(rateTable, item.RateTypeID);
						if (!definitions.ContainsKey(key))
						{
							definitions.Add(key, new List<PMRateDefinition>());
						}

						definitions[key].Add(item);
					}
					
				}
			}
		}

		protected string GetDefinitionKey(string rateTableID, string rateTypeID)
		{
			return string.Format("{0}.{1}", rateTableID, rateTypeID);
		}

		public decimal? GetRate(string rateTableID, string rateTypeID, PMTran tran)
		{
			if (string.IsNullOrEmpty(rateTableID))
				throw new ArgumentNullException(nameof(rateTableID), PXMessages.LocalizeNoPrefix(Messages.ArgumentIsNullOrEmpty));

			trace = new StringBuilder();

			trace.AppendFormat("Calculating Rate. RateTable:{0}, RateType:{1}", rateTableID, rateTypeID);

			List<PMRateDefinition> list;
			if (definitions.TryGetValue(GetDefinitionKey(rateTableID, rateTypeID), out list))
			{
				foreach (PMRateDefinition rd in list)
				{
					trace.AppendFormat("Start Processing Sequence:{0}", rd.Description);
					decimal? rate = GetRate(rd, tran);
					if (rate != null)
					{
						trace.AppendFormat("End Processing Sequence. Rate Defined:{0}", rate);
						return rate;
					}
					else
					{
						trace.AppendFormat("End Processing Sequence. Rate Not Defined");
					}

				}
			}
			return null;
		}

		public string GetTrace(PMTran tran)
		{
			//Add transaction properties:
			PMAccountGroup ag = PXSelect<PMAccountGroup, Where<PMAccountGroup.groupID, Equal<Required<PMAccountGroup.groupID>>>>.Select(graph, tran.AccountGroupID);
			if (ag != null)
			{
				trace.AppendFormat(" PMTran.AccountGroup={0} ", ag.GroupCD);
			}

			IN.InventoryItem inventoryItem = PXSelect<IN.InventoryItem, Where<IN.InventoryItem.inventoryID, Equal<Required<IN.InventoryItem.inventoryID>>>>.Select(graph, tran.InventoryID);
			if (inventoryItem != null)
			{
				trace.AppendFormat(" PMTran.InventoryID={0} ", inventoryItem.InventoryCD);
			}

			return trace.ToString();
		}

		protected virtual decimal? GetRate(PMRateDefinition rd, PMTran tran)
		{
			bool isApplicable = true;

			List<string> finalList = null;

			if (rd.Project == true)
			{
				List<string> rateCodes;
				if (!IsProjectFit(rd.RateDefinitionID, tran.ProjectID, out rateCodes))
				{
					isApplicable = false;
				}
				else
				{
					finalList = rateCodes;
				}
			}

			if (rd.Task == true)
			{
				List<string> rateCodes;
				if (!IsTaskFit(rd.RateDefinitionID, tran.ProjectID, tran.TaskID, out rateCodes))
				{
					isApplicable = false;
				}
				else
				{
					if (finalList == null)
					{
						finalList = rateCodes;
					}
					else
					{
						finalList = new List<string>(finalList.Intersect(rateCodes));
						if ( finalList.Count == 0 )
						{
							isApplicable = false;
						}
					}
				}
			}

			if (rd.AccountGroup == true)
			{
				List<string> rateCodes;
				if (!IsAccountGroupFit(rd.RateDefinitionID, tran.AccountGroupID, out rateCodes))
				{
					isApplicable = false;
				}
				else
				{
					if (finalList == null)
					{
						finalList = rateCodes;
					}
					else
					{
						finalList = new List<string>(finalList.Intersect(rateCodes));
						if (finalList.Count == 0)
						{
							isApplicable = false;
						}
					}
				}
			}

			if (rd.RateItem == true)
			{
				List<string> rateCodes;
				if (!IsItemFit(rd.RateDefinitionID, tran.InventoryID, out rateCodes))
				{
					isApplicable = false;
				}
				else
				{
					if (finalList == null)
					{
						finalList = rateCodes;
					}
					else
					{
						finalList = new List<string>(finalList.Intersect(rateCodes));
						if (finalList.Count == 0)
						{
							isApplicable = false;
						}
					}
				}
			}

			if (rd.Employee == true)
			{
				List<string> rateCodes;
				if (!IsEmployeeFit(rd.RateDefinitionID, tran.ResourceID, out rateCodes))
				{
					isApplicable = false;
				}
				else
				{
					if (finalList == null)
					{
						finalList = rateCodes;
					}
					else
					{
						finalList = new List<string>(finalList.Intersect(rateCodes));
						if (finalList.Count == 0)
						{
							isApplicable = false;
						}
					}
				}
			}

			if (isApplicable)
			{
				string ratecodeid = null;
				if (finalList != null && finalList.Count > 0)
				{
					ratecodeid = finalList[0];
					Debug.Assert(finalList.Count == 1, "Intersaction of all ratecodes must result to single element.");
				}


				PMRate rate;
				if (!string.IsNullOrEmpty(ratecodeid))
				{
					PXSelectBase<PMRate> select = new PXSelect<PMRate,
						Where<PMRate.rateDefinitionID, Equal<Required<PMRate.rateDefinitionID>>,
						And<Where<PMRate.rateCodeID, Equal<Required<PMRate.rateCodeID>>,
						And<Where<PMRate.startDate, LessEqual<Required<PMRate.startDate>>,
						And2<Where<PMRate.endDate, GreaterEqual<Required<PMRate.endDate>>>, Or<PMRate.endDate, IsNull>>>>>>>>(graph);

					rate = select.Select(rd.RateDefinitionID, ratecodeid, tran.Date, tran.Date);
				}
				else
				{
					PXSelectBase<PMRate> select = new PXSelect<PMRate,
						Where<PMRate.rateDefinitionID, Equal<Required<PMRate.rateDefinitionID>>,
						And<Where<PMRate.startDate, LessEqual<Required<PMRate.startDate>>,
						And2<Where<PMRate.endDate, GreaterEqual<Required<PMRate.endDate>>>, Or<PMRate.endDate, IsNull>>>>>>(graph);
					rate = select.Select(rd.RateDefinitionID, tran.Date, tran.Date);
				}
				trace.AppendFormat("	Searching Rate for Date:{0}", tran.Date);

				if (rate != null)
				{
					return rate.Rate;
				}
				else

					return null;
			}
			else
			{
				return null;
			}

		}

		protected virtual bool IsProjectFit(int? rateDefinitionID, int? projectID, out List<string> rateCodes)
		{
			rateCodes = new List<string>();
			PMProject project = PXSelect<PMProject, Where<PMProject.contractID, Equal<Required<PMProject.contractID>>>>.Select(graph, projectID);

			if (project == null)
				throw new PXException(Messages.ProjectNotFound, projectID);

			string cd = project.ContractCD;

			PXSelectBase<PMProjectRate> select = new PXSelect<PMProjectRate,
				Where<PMProjectRate.rateDefinitionID, Equal<Required<PMProjectRate.rateDefinitionID>>>>(graph);

			bool result = false;
			foreach (PMProjectRate item in select.Select(rateDefinitionID))
			{
				if (IsFit(item.ProjectCD.Trim(), cd.Trim()))
				{
					rateCodes.Add(item.RateCodeID);
					trace.AppendFormat("	Checking Project {0}..Match found.", cd.Trim());
					result = true;
				}
			}

			if (!result)
				trace.AppendFormat("	Checking Project {0}..Match not found.", cd.Trim());

			return result;
		}

		protected virtual bool IsTaskFit(int? rateDefinitionID, int? projectID, int? taskID, out List<string> rateCodes)
		{
			rateCodes = new List<string>();
			PMTask task = PXSelect<PMTask, Where<PMTask.projectID, Equal<Required<PMTask.projectID>>, And<PMTask.taskID, Equal<Required<PMTask.taskID>>>>>.Select(graph, projectID, taskID);
			if (task == null)
				throw new PXException(Messages.TaskNotFound, projectID, taskID);

			string cd = task.TaskCD;

			PXSelectBase<PMTaskRate> select = new PXSelect<PMTaskRate,
				Where<PMTaskRate.rateDefinitionID, Equal<Required<PMTaskRate.rateDefinitionID>>>>(graph);

			bool result = false;
			foreach (PMTaskRate item in select.Select(rateDefinitionID))
			{
				if (IsFit(item.TaskCD.Trim(), cd.Trim()))
				{
					rateCodes.Add(item.RateCodeID);
					trace.AppendFormat("	Checking Task {0}..Match found.", cd.Trim());
					result = true;
				}
			}

			if (!result)
				trace.AppendFormat("	Checking Task {0}..Match not found.", cd.Trim());

			return result;
		}

		protected virtual bool IsAccountGroupFit(int? rateDefinitionID, int? accountGroupID, out List<string> rateCodes)
		{
			rateCodes = new List<string>();
			PXSelectBase<PMAccountGroupRate> select = new PXSelect<PMAccountGroupRate,
				Where<PMAccountGroupRate.rateDefinitionID, Equal<Required<PMAccountGroupRate.rateDefinitionID>>,
				And<PMAccountGroupRate.accountGroupID, Equal<Required<PMAccountGroupRate.accountGroupID>>>>>(graph);

			bool result = false;
			foreach (PMAccountGroupRate item in select.Select(rateDefinitionID, accountGroupID))
			{
				rateCodes.Add(item.RateCodeID);
				trace.AppendFormat("	Checking Account Group {0}..Match found.", accountGroupID);
				result = true;
			}

			if (!result)
				trace.AppendFormat("	Checking Account Group {0}..Match not found.", accountGroupID);

			return result;
		}

		protected virtual bool IsItemFit(int? rateDefinitionID, int? inventoryID, out List<string> rateCodes)
		{
			rateCodes = new List<string>();
			PXSelectBase<PMItemRate> select = new PXSelect<PMItemRate,
				Where<PMItemRate.rateDefinitionID, Equal<Required<PMItemRate.rateDefinitionID>>,
				And<PMItemRate.inventoryID, Equal<Required<PMItemRate.inventoryID>>>>>(graph);

			bool result = false;
			foreach (PMItemRate item in select.Select(rateDefinitionID, inventoryID))
			{
				rateCodes.Add(item.RateCodeID);
				trace.AppendFormat("	Checking Inventory Item {0}..Match found.", inventoryID);
				result = true;
			}

			if (!result)
				trace.AppendFormat("	Checking Inventory Item {0}..Match not found.", inventoryID);

			return result;
		}

		protected virtual bool IsEmployeeFit(int? rateDefinitionID, int? employeeID, out List<string> rateCodes)
		{
			rateCodes = new List<string>();
			PXSelectBase<PMEmployeeRate> select = new PXSelect<PMEmployeeRate,
				Where<PMEmployeeRate.rateDefinitionID, Equal<Required<PMEmployeeRate.rateDefinitionID>>,
				And<PMEmployeeRate.employeeID, Equal<Required<PMEmployeeRate.employeeID>>>>>(graph);

			bool result = false;
			foreach (PMEmployeeRate item in select.Select(rateDefinitionID, employeeID))
			{
				rateCodes.Add(item.RateCodeID);
				trace.AppendFormat("	Checking Employee {0}..Match found.", employeeID);
				result = true;
			}

			if (!result)
				trace.AppendFormat("	Checking Employee {0}..Match not found.", employeeID);

			return result;
		}

		protected virtual bool IsFit(string wildcard, string value)
		{
			if (value.Length < wildcard.Length)
			{
				value += new string(' ', wildcard.Length - value.Length);
			}
			else if (value.Length > wildcard.Length)
			{
				return false;
			}

			for (int i = 0; i < wildcard.Length; i++)
			{
				if (wildcard[i] == '?')
					continue;

				if (wildcard[i] != value[i])
					return false;
			}

			return true;
		}
	}

	public interface IRateTable
	{
		IList<PMRateDefinition> GetRateDefinitions(string rateTable);
		object Evaluate(PMObjectType objectName, string fieldName, string attribute, PMTran row);
		decimal? GetPrice(PMTran row);
		decimal? ConvertAmountToCurrency(string fromCuryID, string toCuryID, string rateType, DateTime? effectiveDate, decimal? value);
	}
}
