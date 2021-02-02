using System;
using System.Collections.Generic;

using PX.Data;
using PX.Objects.AR;

using PX.Common.Parser;
using PX.Reports.Parser;

namespace PX.Objects.CT
{	
	public interface IContractInformation
	{
		string GetParametrInventoryPrefix(CTFormulaDescriptionContainer tran);
		string GetParametrActionInvoice(CTFormulaDescriptionContainer tran);
		string GetParametrActionItem(CTFormulaDescriptionContainer tran);
		object Evaluate(CTObjectType objectName, string fieldName, string attribute, CTFormulaDescriptionContainer row);
	}

    public class CTExpressionParser : ExpressionParser
	{

		public IContractInformation Engine { get; protected set; }

		private CTExpressionParser(IContractInformation engine, string text)
			: base(text)
		{
			Engine = engine;
		}

		protected override ExpressionContext CreateContext()
		{
			return new CTExpressionContext(Engine);
		}

		protected override NameNode CreateNameNode(ExpressionNode node, string tokenString)
		{
			return new CTNameNode(node, tokenString, Context);

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

		public static ExpressionNode Parse(IContractInformation engine, string formula)
		{
			if (formula.StartsWith("="))
			{
				formula = formula.Substring(1);
			}

			var expr = new CTExpressionParser(engine, formula);
			return expr.Parse();
		}
	}

	public class CTExpressionContext : ExpressionContext
	{
		protected IContractInformation engine;
		public PX.Data.Reports.SoapNavigator.DATA Data { get; protected set; }

		public CTExpressionContext(IContractInformation engine)
		{
			this.engine = engine;
			Data = new PX.Data.Reports.SoapNavigator.DATA();
		}

		public virtual object Evaluate(CTNameNode node, CTFormulaDescriptionContainer row)
		{
			if (node.IsAttribute)
				return engine.Evaluate(node.ObjectName, null, node.FieldName, row);
			else
				return engine.Evaluate(node.ObjectName, node.FieldName, null, row);
		}
		public virtual string GetParametrInventoryPrefix(CTFormulaDescriptionContainer row)
		{
			return engine.GetParametrInventoryPrefix(row);
		}
		public virtual string GetParametrActionInvoice(CTFormulaDescriptionContainer row)
		{
			return engine.GetParametrActionInvoice(row);
		}
		public virtual string GetParametrActionItem(CTFormulaDescriptionContainer row)
		{
			return engine.GetParametrActionItem(row);
		}
	}

	public class CTNameNode : NameNode
	{
		public CTObjectType ObjectName { get; protected set; }
		public string FieldName { get; protected set; }
		protected bool isAttribute;
		

		public CTNameNode(ExpressionNode node, string tokenString, ExpressionContext context)
			: base(node, tokenString, context)
		{
			string[] parts = Name.Split('.');

			if (parts.Length == 3)
			{
				isAttribute = true;
				ObjectName = (CTObjectType)Enum.Parse(typeof(CTObjectType), parts[0], true);
				FieldName = parts[2].Trim('[', ']').Trim();
			}
			else if (parts.Length == 2)
			{
				ObjectName = (CTObjectType)Enum.Parse(typeof(CTObjectType), parts[0], true);
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
				ObjectName = CTObjectType.Contract;
				FieldName = Name;
			}
		}

		protected bool IsActionInvoice
		{
			get
			{
				return name.StartsWith("@ActionInvoice", StringComparison.InvariantCultureIgnoreCase);
			}
		}

		protected bool IsReport
		{
			get
			{
				return name.StartsWith("Report", StringComparison.InvariantCultureIgnoreCase);
			}
		}

		protected bool IsActionItem
		{
			get
			{
				return name.StartsWith("@ActionItem", StringComparison.InvariantCultureIgnoreCase);
			}
		}

		protected bool IsPrefix
		{
			get
			{
				return name.StartsWith("@Prefix", StringComparison.InvariantCultureIgnoreCase);
			}
		}

		public bool IsAttribute
		{
			get { return isAttribute; }
		}

		public override object Eval(object row)
		{
			if(IsActionInvoice)
			{
				return ((CTExpressionContext)context).GetParametrActionInvoice((CTFormulaDescriptionContainer)row);
			}
			if (IsActionItem)
			{
				return ((CTExpressionContext)context).GetParametrActionItem((CTFormulaDescriptionContainer)row);
			}
			if (IsPrefix)
			{
				return ((CTExpressionContext)context).GetParametrInventoryPrefix((CTFormulaDescriptionContainer)row);
			}
			if (IsReport)
			{
				return ((CTExpressionContext)context).Data;
			}
			return ((CTExpressionContext)context).Evaluate(this, (CTFormulaDescriptionContainer)row);
		}

	}

	public enum CTObjectType
	{
		Contract,
		ContractTemplate,
		Customer,
		Location,
		ContractItem,
		ContractDetail,
		InventoryItem,
		PMTran,
		ContractBillingSchedule,
		UsageData,
		AccessInfo
	}

	public class CTFormulaDescriptionContainer:IBqlTable
	{
		public abstract class actionInvoice : PX.Data.BQL.BqlString.Field<actionInvoice> { }
		public virtual string ActionInvoice { get; set; }

		public abstract class actionItem : PX.Data.BQL.BqlString.Field<actionItem> { }
		public virtual string ActionItem { get; set; }

		public abstract class inventoryPrefix : PX.Data.BQL.BqlString.Field<inventoryPrefix> { }
		public virtual string InventoryPrefix { get; set; }

		public abstract class contractID : PX.Data.BQL.BqlInt.Field<contractID> { }
		[PXInt]
		public virtual Int32? ContractID { get; set; }

		public abstract class customerID : PX.Data.BQL.BqlInt.Field<customerID> { }
		[PXInt]
		public virtual Int32? CustomerID { get; set; }

		public abstract class customerLocationID : PX.Data.BQL.BqlInt.Field<customerLocationID> { }
		[PXInt]
		public virtual Int32? CustomerLocationID { get; set; }

		public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
		[PXInt]
		public virtual Int32? InventoryID { get; set; }

		public abstract class contractItemID : PX.Data.BQL.BqlInt.Field<contractItemID> { }
		[PXInt]
		public virtual Int32? ContractItemID { get; set; }

		public abstract class contractDetailID : PX.Data.BQL.BqlInt.Field<contractDetailID> { }
		[PXInt]
		public virtual Int32? ContractDetailID { get; set; }

		public UsageData usageData;

		public List<long?> pmTranIDs;
	}

	public class CTDataNavigator : PX.Reports.Data.IDataNavigator
	{
		protected List<CTFormulaDescriptionContainer> list;
		protected IContractInformation engine;

		#region IDataNavigator Members

		public CTDataNavigator(IContractInformation engine, List<CTFormulaDescriptionContainer> list)
		{
			this.engine = engine;
			this.list = list;
		}

		public void Clear()
		{
		}

		public void Refresh()
		{
		}

		public object Current
		{
			get { throw new NotImplementedException(); }
		}

		public PX.Reports.Data.IDataNavigator GetChildNavigator(object record)
		{
			return null;
		}

		public object GetItem(object dataItem, string dataField)
		{
			throw new NotImplementedException();
		}

		public System.Collections.IList GetList()
		{
			return list;
		}

		public object GetValue(object dataItem, string dataField, ref string format)
		{
			CTNameNode nn = new CTNameNode(null, dataField, null);
			if (nn.IsAttribute)
				return engine.Evaluate(nn.ObjectName, null, nn.FieldName, (CTFormulaDescriptionContainer)dataItem);
			else
			{
				return engine.Evaluate(nn.ObjectName, nn.FieldName, null, (CTFormulaDescriptionContainer)dataItem);

			}
		}

		public bool MoveNext()
		{
			throw new NotImplementedException();
		}

		public void Reset()
		{
			throw new NotImplementedException();
		}

		public PX.Reports.Data.ReportSelectArguments SelectArguments
		{
			get { throw new NotImplementedException(); }
		}

		public object this[string dataField]
		{
			get { throw new NotImplementedException(); }
		}

		public string CurrentlyProcessingParam
		{
			get
			{
				throw new NotImplementedException();
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		public int[] GetFieldSegments(string field)
		{
			throw new NotImplementedException();
		}
		public object Clone()
		{
			return new CTDataNavigator(engine, list == null ? null : new List<CTFormulaDescriptionContainer>(list));
		}
		#endregion
	}
}
