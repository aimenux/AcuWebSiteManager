using PX.Data;
using PX.Objects.PM;
using System;

namespace PX.Objects.FS
{
    public class SMCostCodeAttribute : CostCodeAttribute, IPXRowPersistingSubscriber
    {
        private Type _SkipRowPersistingValidation;

        public SMCostCodeAttribute(Type account, Type task) : base(account, task)
        {

        }

        public SMCostCodeAttribute(Type skipRowPersistingValidation, Type account, Type task) : base(account, task)
        {
            _SkipRowPersistingValidation = skipRowPersistingValidation;
        }

        public new void RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
        {
            bool? skipRowPersistingValidation = _SkipRowPersistingValidation != null ? (bool?)sender.GetValue(e.Row, _SkipRowPersistingValidation.Name) : null;

            if (skipRowPersistingValidation == false)
            {
                base.RowPersisting(sender, e);
            }
        }
    }
}
