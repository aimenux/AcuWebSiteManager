using PX.Data;
using System;

namespace PX.Objects.FS
{
    internal static class GraphHelper
    {
        public static void RaiseRowPersistingException<Field>(PXCache cache, object row)
            where Field : IBqlField
        {
            var fieldName = typeof(Field).Name;

            cache.RaiseExceptionHandling<Field>(
                    row,
                    null,
                    new PXSetPropertyException(PXMessages.LocalizeFormatNoPrefix(ErrorMessages.FieldIsEmpty, fieldName)));

            throw new PXRowPersistingException(fieldName, null, ErrorMessages.FieldIsEmpty, PXUIFieldAttribute.GetDisplayName<Field>(cache));
        }

        public static PXAction GetSaveAction(this PXGraph graph)
        {
            PXAction saveAction = null;
            Type primary = !String.IsNullOrEmpty(graph.PrimaryView) ? graph.Views[graph.PrimaryView].GetItemType() : null;

            foreach (PXAction action in graph.Actions.Values)
            {
                PXButtonState state = action.GetState(null) as PXButtonState;
                if (state != null && state.SpecialType == PXSpecialButtonType.Save)
                {
                    if (primary == null || state.ItemType == null || primary == state.ItemType || primary.IsSubclassOf(state.ItemType))
                    {
                        saveAction = action;
                        break;
                    }
                }
            }

            if (saveAction == null)
            {
                throw new PXException("There is not a Save action in the graph " + graph.GetType());
            }

            return saveAction;
        }

        public static void SetValueExtIfDifferent<Field>(this PXCache cache, object data, object newValue)
            where Field : IBqlField
        {
            object currentValue = cache.GetValue<Field>(data);

            if (
                (currentValue == null && newValue != null)
                || (currentValue != null && newValue == null)
                || (currentValue != null && currentValue.Equals(newValue) == false)
            )
            {
                cache.SetValueExt<Field>(data, newValue);
            }
        }

        public static bool IsValueChanging(PXCache cache, object oldValue, object newValue)
        {
            if (oldValue == null && newValue == null)
            {
                return false;
            }

            if (oldValue != null && newValue != null)
            {
                if (newValue is string && (oldValue is DateTime || oldValue is DateTime?))
                {
                    DateTime? newDateTime = SharedFunctions.TryParseHandlingDateTime(cache, newValue);
                    if (newDateTime == null)
                    {
                        return true;
                    }

                    if (newDateTime == (DateTime)oldValue)
                    {
                        return false;
                    }
                }
                else if (oldValue.Equals(newValue) == true)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
