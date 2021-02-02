using System;

namespace PX.Objects.AM.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class AMBatchCacheNameAttribute : PX.Data.PXCacheNameAttribute
    {
        public AMBatchCacheNameAttribute(string name)
            : base(name)
        {
        }

        public override string GetName(object row)
        {
            var batch = row as AMBatch;
            if (batch == null)
            {
                return base.GetName();
            }

            switch (batch.DocType)
            {
                case AMDocType.Material:
                    return Messages.DocTypeMaterial;
                case AMDocType.Move:
                    return Messages.DocTypeMove;
                case AMDocType.Labor:
                    return Messages.DocTypeLabor;
                case AMDocType.ProdCost:
                    return Messages.DocTypeProdCost;
                case AMDocType.WipAdjust:
                    return Messages.DocTypeWipAdjust;
                case AMDocType.Disassembly:
                    return Messages.Disassembly;
            }
            return "AM Batch";
        }
    }
}