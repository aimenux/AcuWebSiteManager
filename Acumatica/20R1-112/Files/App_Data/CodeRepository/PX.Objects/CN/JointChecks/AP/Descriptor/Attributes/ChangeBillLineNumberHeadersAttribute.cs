using System;
using PX.Data;
using PX.Objects.CN.Common.Descriptor;
using PX.Objects.CN.JointChecks.Descriptor;

namespace PX.Objects.CN.JointChecks.AP.Descriptor.Attributes
{
    public class ChangeBillLineNumberHeadersAttribute : PXCustomizeBaseAttributeAttribute
    {
	    public ChangeBillLineNumberHeadersAttribute(Type targetAttribute)
            : base(targetAttribute, Constants.AttributeProperties.Headers, new[]
            {
                JointCheckLabels.BillLine.LineNumber,
                BusinessMessages.InventoryID,
                JointCheckLabels.BillLine.TransactionDescription,
                JointCheckLabels.BillLine.Project,
                JointCheckLabels.BillLine.ProjectTask,
                JointCheckLabels.BillLine.CostCode,
                JointCheckLabels.BillLine.Account,
                JointCheckLabels.BillLine.Balance
            })
        {
        }
    }
}