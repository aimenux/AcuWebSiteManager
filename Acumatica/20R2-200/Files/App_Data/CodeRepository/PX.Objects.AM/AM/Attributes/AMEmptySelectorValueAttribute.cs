using PX.Data;

namespace PX.Objects.AM.Attributes
{
    public class AMEmptySelectorValueAttribute : PXEventSubscriberAttribute, IPXFieldSelectingSubscriber
    {
        private bool _OverrideDefautValue;
        private string _DisplayValue;

        public AMEmptySelectorValueAttribute(string displayValue)
        {
            _OverrideDefautValue = true;
            _DisplayValue = displayValue;
        }
        public void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
        {
            if (_OverrideDefautValue && sender.GetValue(e.Row, this.FieldOrdinal) == null)
            {
                e.ReturnState = PXStringState.CreateInstance(e.ReturnState, null, null, this.FieldName, null, null, string.Empty, null, null, null, null);
                e.ReturnValue = _DisplayValue;
            }
        }

        public void OverrideDefaultValue(bool overrideDefaultValue)
        {
            _OverrideDefautValue = overrideDefaultValue;
        }

        public static void OverrideDefaultValue<TField>(PXCache cache, object row, bool overrideDefaultValue = true)
                where TField : IBqlField
        {
            foreach (var attribute in cache.GetAttributes<TField>(row))
            {
                if (attribute is AMEmptySelectorValueAttribute)
                {
                    var emptySelectorValueAttribute = (AMEmptySelectorValueAttribute)attribute;
                    emptySelectorValueAttribute.OverrideDefaultValue(overrideDefaultValue);
                }
            }
        }
    }
}
