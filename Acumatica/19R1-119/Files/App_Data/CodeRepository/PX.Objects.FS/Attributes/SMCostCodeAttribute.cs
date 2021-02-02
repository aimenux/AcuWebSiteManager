using System;
using PX.Objects.PM;
using PX.Data;

namespace PX.Objects.FS
{
    public class SMCostCodeAttribute : CostCodeAttribute, IPXRowPersistingSubscriber
    {
        private Type _skipRowPersistingValidation;

        public SMCostCodeAttribute(Type account, Type task) : base(account, task)
        {

        }

        public SMCostCodeAttribute(Type skipRowPersistingValidation, Type account, Type task) : base(account, task)
        {
            _skipRowPersistingValidation = skipRowPersistingValidation;
        }

        public new void RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
        {
            bool? skipRowPersistingValidation = _skipRowPersistingValidation != null ? (bool?)sender.GetValue(e.Row, _skipRowPersistingValidation.Name) : null;

            if (skipRowPersistingValidation == false)
            {
                base.RowPersisting(sender, e);
            }
        }
    }
}
