using System;
using PX.Api;
using PX.Data;
using PX.Objects.CN.Common.Descriptor;

namespace PX.Objects.PJ.ProjectManagement.PJ.Descriptor.Attributes
{
    public class ClassChangeConfirmationAttribute : PXEventSubscriberAttribute, IPXFieldVerifyingSubscriber
    {
        public string Message;
        public Type ClassIdField;
        public string ViewName;

        public void FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs args)
        {
            var classIdField = cache.GetField(ClassIdField);
            var classId = cache.GetValue(args.Row, classIdField) as string;
            if (!classId.IsNullOrEmpty() && ShowConfirmationDialog(cache) == WebDialogResult.No)
            {
                args.NewValue = classId;
            }
        }

        private WebDialogResult ShowConfirmationDialog(PXCache cache)
        {
            return cache.Graph.Views[ViewName].Ask(cache.ActiveRow, SharedMessages.Warning, Message,
                MessageButtons.YesNo, MessageIcon.None);
        }
    }
}