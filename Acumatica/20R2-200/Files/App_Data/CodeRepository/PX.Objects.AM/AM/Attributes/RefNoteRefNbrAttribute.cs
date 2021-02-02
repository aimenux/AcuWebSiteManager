using System;
using PX.Data;
using PX.Objects.Common;

namespace PX.Objects.AM.Attributes
{
    /// <summary>
    /// variation of <see cref="PXRefNoteAttribute"/> which relies on a string ref nbr field to supply a string value when such a value has no related noteid or primary screen
    /// </summary>
    public class RefNoteRefNbrAttribute : PXRefNoteBaseAttribute
    {
        protected Type RefNbrField;

        /// <summary>
        /// When true any value in RefNbr is displayed first, otherwise only show refnbr if RefNoteID field is null
        /// </summary>
        public bool RefNbrIsDefault = false;

        public RefNoteRefNbrAttribute(Type refNbrField)
        {
            RefNbrField = refNbrField;
        }

        public static string FormatFieldNbr(params string[] vals)
        {
            return vals == null ? null : string.Join(", ", vals);
        }

        public override void CacheAttached(PXCache sender)
        {
            base.CacheAttached(sender);

            PXButtonDelegate del = delegate (PXAdapter adapter)
            {
                PXCache cache = adapter.View.Graph.Caches[sender.GetItemType()];
                if (cache.Current != null)
                {
                    object val = cache.GetValueExt(cache.Current, _FieldName);

                    PXLinkState state = val as PXLinkState;
                    if (state != null)
                    {
                        helper.NavigateToRow(state.target.FullName, state.keys, PXRedirectHelper.WindowMode.NewWindow);
                    }
                    else
                    {
                        helper.NavigateToRow((Guid?)cache.GetValue(cache.Current, _FieldName), PXRedirectHelper.WindowMode.NewWindow);
                    }
                }

                return adapter.Get();
            };

            string ActionName = sender.GetItemType().Name + "$" + _FieldName + "$Link";
            sender.Graph.Actions[ActionName] = (PXAction)Activator.CreateInstance(typeof(PXNamedAction<>).MakeGenericType(sender.GetItemType()), new object[] { sender.Graph, ActionName, del, new PXEventSubscriberAttribute[] { new PXUIFieldAttribute { MapEnableRights = PXCacheRights.Select } } });
        }

        public override void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
        {
            // If RefNoteID is empty we want to try to use RefNbr as display (if one exists
            var refNbrFieldValue = (string)sender.GetValue(e.Row, RefNbrField.Name);
            if (!string.IsNullOrWhiteSpace(refNbrFieldValue))
            {
                var refNoteId = sender.GetValue(e.Row, _FieldName);
                if (RefNbrIsDefault || refNoteId == null)
                {
                    // Ignore noteid and reference lookup
                    e.ReturnValue = refNbrFieldValue;

                    return;
                }
            }

            base.FieldSelecting(sender, e);
        }
    }
}