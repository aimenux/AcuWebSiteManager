using PX.Data;
using System;
using System.Linq;

namespace PX.Objects.AM.Attributes
{
    /// <summary>
    /// Combines value from different fields from the same dac into a "/" delimited string.
    /// </summary>
    public class CombineInfoAttribute : PXStringAttribute,
                                        IPXFieldSelectingSubscriber
    {
        private Type[] _Fields;

        public CombineInfoAttribute(params Type[] fields) : base()
        {
            _Fields = fields;
        }

        public override void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
        {
            if (e.Row == null) return;

            e.ReturnValue = String.Join(" / ", _Fields.Select(field => sender.GetValue(e.Row, field.Name) ?? PXMessages.LocalizeNoPrefix(Messages.None)));
        }
    }
}
