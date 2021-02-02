using System;
using PX.Common.Parser;
using PX.Reports.Parser;

namespace PX.Objects.PM
{	
    public class PMExpressionParser : ExpressionParser
	{
		public IRateTable Engine { get; protected set; }
		
		private PMExpressionParser(IRateTable engine, string text)
			: base(text)
		{
			Engine = engine;
		}

		protected override ExpressionContext CreateContext()
		{
			return new PMExpressionContext(Engine);
		}

		protected override NameNode CreateNameNode(ExpressionNode node, string tokenString)
		{
			return new PMNameNode(node, tokenString, Context);

		}

		protected override void ValidateName(NameNode node, string tokenString)
		{
			
		}
		
		protected override bool IsAggregate(string nodeName)
		{
			return ReportAggregateNode.IsAggregate(nodeName);
		}

		protected override AggregateNode CreateAggregateNode(string name, string dataField)
		{
			return new ReportAggregateNode(name, dataField);
		}

		public static ExpressionNode Parse(IRateTable engine, string formula)
		{
			if (formula.StartsWith("="))
			{
				formula = formula.Substring(1);
			}

			var expr = new PMExpressionParser(engine, formula);
			return expr.Parse();
		}

		protected override FunctionNode CreateFunctionNode(ExpressionNode node, string name)
		{
			return new PMFunctionNode(null, name, (PMExpressionContext)this.Context);
		}
	}

	public class PMExpressionContext : ExpressionContext
	{
		protected IRateTable engine;
		
		public PMExpressionContext(IRateTable engine)
		{
			this.engine = engine;
		}
				
		public virtual object Evaluate(PMNameNode node, PMTran row)
		{
			if ( node.IsAttribute )
				return engine.Evaluate(node.ObjectName, null, node.FieldName, row);
			else
				return engine.Evaluate(node.ObjectName, node.FieldName, null, row);
		}

		public virtual decimal? GetPrice(PMTran row)
		{
			return engine.GetPrice(row);
		}

		public virtual decimal? ConvertAmountToCurrency(FunctionContext context, string fromCuryID, string toCuryID, string rateType, DateTime? effectiveDate, decimal? value)
		{
			return engine.ConvertAmountToCurrency(fromCuryID, toCuryID, rateType, effectiveDate, value);
		}
}

	public class PMNameNode : NameNode
	{
		public PMObjectType ObjectName { get; protected set; }
		public string FieldName { get; protected set; }
		protected bool isAttribute;

		public PMNameNode(ExpressionNode node, string tokenString, ExpressionContext context)
			: base(node, tokenString, context)
		{
			string[] parts = Name.Split('.');

			if (parts.Length == 3)
			{
				isAttribute = true;
				ObjectName = (PMObjectType)Enum.Parse(typeof(PMObjectType), parts[0], true);
				FieldName = parts[2].Trim('[', ']').Trim();
			}
			else if (parts.Length == 2)
			{
				ObjectName = (PMObjectType)Enum.Parse(typeof(PMObjectType), parts[0], true);
				if (parts[1].Trim().EndsWith("_Attributes"))
				{
					isAttribute = true;
					FieldName = parts[1].Substring(0, parts[1].Length - 11);
				}
				else
					FieldName = parts[1];
			}
			else
			{
				ObjectName = PMObjectType.PMTran;
				FieldName = Name;
			}
		}

		protected bool IsRate
		{
			get
			{
				return name.StartsWith("@Rate", StringComparison.InvariantCultureIgnoreCase);
			}
		}

		protected bool IsPrice
		{
			get
			{
				return name.StartsWith("@Price", StringComparison.InvariantCultureIgnoreCase);
			}
		}
	
		public bool IsAttribute
		{
			get { return isAttribute; }
		}

		public override object Eval(object row)
		{
			if (IsRate)
			{
				return ((PMTran)row).Rate;
			}

			if (IsPrice)
			{
				return ((PMExpressionContext)context).GetPrice((PMTran)row);
			}

			return ((PMExpressionContext)context).Evaluate(this, (PMTran)row);
		}

	}

	public class PMFunctionNode : FunctionNode
	{
		private PMExpressionContext context;
		public PMFunctionNode(ExpressionNode node, string name, PMExpressionContext context)
			: base(node, name, context)
		{
			this.context = context;
		}
	}

	public enum PMObjectType
	{
		PMTran,
		PMProject,
		PMTask,
		PMAccountGroup,
		EPEmployee,
		Customer,
		Vendor,
		InventoryItem,
		PMBudget
	}
}
