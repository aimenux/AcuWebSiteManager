using System;
using PX.Objects.PM;
using PX.Data;

namespace PX.Objects.FS
{
    public class FSDBDateAndTimeAttribute : PXDBDateAndTimeAttribute, IPXFieldUpdatedSubscriber
    {
        private Type _SourceType;
        protected string _SourceField;
        protected bool _IgnoreSeconds;
        private bool _FromAttribute;

        public bool IgnoreSeconds
        {
            get
            {
                return _IgnoreSeconds;
            }
            set
            {
                _IgnoreSeconds = value;
            }
        }

        public FSDBDateAndTimeAttribute(Type sourceType) : base()
        {
            if (sourceType.IsNested && typeof(IBqlField).IsAssignableFrom(sourceType))
            {
                _SourceType = BqlCommand.GetItemType(sourceType);
                _SourceField = sourceType.Name;
            }
        }

        public override void CacheAttached(PXCache sender)
        {
            base.CacheAttached(sender);
            sender.Graph.FieldUpdating.AddHandler(sender.GetItemType(), _FieldName + PXDBDateAndTimeAttribute.TIME_FIELD_POSTFIX, Time_FieldUpdating);
        }

        public new void Time_FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
        {
            if (_SourceType != null)
            {
                PXCache sourceCache = sender.Graph.Caches[_SourceType];
                if (sourceCache.Current != null)
                {
                    object dateTimeObject = sourceCache.GetValue(sourceCache.Current, _SourceField);

                    DateTime? dateTimeField = SharedFunctions.TryParseHandlingDateTime(sourceCache, dateTimeObject);
                    DateTime? timeField = SharedFunctions.TryParseHandlingDateTime(sender, e.NewValue);

                    if (dateTimeField != null && timeField != null)
                    {
                        e.NewValue = SharedFunctions.GetTimeWithSpecificDate(timeField, dateTimeField, _IgnoreSeconds);
                        sender.SetValuePending(e.Row, _FieldName, e.NewValue);
                        _FromAttribute = true;
                    }
                }
            }

            base.Time_FieldUpdating(sender, e);
        }

        public void FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            var valuePending = sender.GetValuePending(e.Row, _FieldName);

            if (_FromAttribute == false || PXCache.NotSetValue == valuePending)
            {
                return;
            }

            _FromAttribute = false;

            DateTime? dateTimeValue = (DateTime?)valuePending;
            if (dateTimeValue != null)
            {
                sender.SetValue(e.Row, _FieldName, dateTimeValue);
            }
        }
    }
}
