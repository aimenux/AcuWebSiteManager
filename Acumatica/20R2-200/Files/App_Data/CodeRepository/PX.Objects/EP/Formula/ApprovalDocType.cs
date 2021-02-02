using System;
using System.Collections.Generic;
using System.Web.Compilation;
using PX.Data;

namespace PX.Objects.EP
{
	/// <summary>
	/// Formula that defines a UI-friendly type of the entity to be approved.
	/// Uses the detailed record-level type (if it is not empty), otherwise,
	/// uses the friendly name of the source item's cache.
	/// </summary>
	/// <typeparam name="EntityTypeName">
	/// Field containing the type name of the source item, e.g.
	/// <see cref="EPApprovalProcess.EPOwned.entityType"/>. From
	/// this type name, the friendly cache name will be deduced if
	/// the record-level entity type field returns <c>null</c>.
	/// </typeparam>
	/// <typeparam name="SourceItemType">
	/// Field containing the detailed, record-level source item type, e.g.
	/// <see cref="EPApproval.sourceItemType"/>. If this field does not
	/// contain <c>null</c>, its value will be returned by this formula.
	/// </typeparam>
	public class ApprovalDocType<EntityTypeName, SourceItemType> : BqlFormulaEvaluator<EntityTypeName, SourceItemType>
		where EntityTypeName : IBqlOperand
		where SourceItemType : IBqlOperand
	{
		public override object Evaluate(PXCache cache, object item, Dictionary<Type, object> pars)
		{
			string sourceItemType = (string)pars[typeof(SourceItemType)];

			if (!string.IsNullOrEmpty(sourceItemType))
			{
				return sourceItemType;
			}

			string entityType = (string)pars[typeof(EntityTypeName)];
			if (!string.IsNullOrEmpty(entityType))
			{
				return EntityHelper.GetFriendlyEntityName(PXBuildManager.GetType(entityType, false, true));
			}
			return null;
		}
	}
}
