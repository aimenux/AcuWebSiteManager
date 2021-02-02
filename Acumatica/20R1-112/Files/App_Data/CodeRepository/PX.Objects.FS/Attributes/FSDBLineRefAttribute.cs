using System;
using PX.Objects.PM;
using PX.Data;

namespace PX.Objects.FS
{
    public class FSDBLineRefAttribute : PXEventSubscriberAttribute, IPXRowInsertingSubscriber
    {
        private Type _lineNbr;

        public FSDBLineRefAttribute(Type lineNbr)
        {
            _lineNbr = lineNbr;
        }

        public void RowInserting(PXCache sender, PXRowInsertingEventArgs e)
        {
            object lineNbr = sender.GetValue(e.Row, _lineNbr.Name);
            object lineRef = sender.GetValue(e.Row, _FieldName);
            int length = -1;

            if (lineRef == null)
            {
                foreach (PXEventSubscriberAttribute attribute in sender.GetAttributes(_FieldName))
                {
                    if (attribute is PXDBStringAttribute)
                    {
                        length = ((PXDBStringAttribute)attribute).Length;
                        break;
                    }
                }

                if(length > 0 && (int?)lineNbr > 0)
                    sender.SetValue(e.Row, _FieldName, ((int?)lineNbr).Value.ToString().PadLeft(length, '0'));
            } 
        }
    }
}
