using System.Linq;
using PX.Objects.PJ.RequestsForInformation.PJ.DAC;
using PX.Data;
using PX.Data.BQL;

namespace PX.Objects.PJ.RequestsForInformation.PJ.Descriptor.Attributes
{
    public class RequestForInformationRelationTypeAttribute : PXEventSubscriberAttribute,
        IPXFieldSelectingSubscriber, IPXFieldDefaultingSubscriber, IPXFieldUpdatedSubscriber
    {
        public const string Contact = "Contact";
        public const string Project = "Project";
        public const string ProjectTask = "Project Task";
        public const string PurchaseOrder = "Purchase Order";
        public const string Subcontract = "Subcontract";
        public const string ApInvoice = "AP Invoice";
        public const string ArInvoice = "AR Invoice";
        public const string RequestForInformation = "RFI";

        public void FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs args)
        {
            if (args.Row is RequestForInformationRelation requestForInformationRelation
                && requestForInformationRelation.Role != null)
            {
                var types = GetRoleTypes(requestForInformationRelation);
                args.NewValue = types.First();
            }
        }

        public void FieldSelecting(PXCache cache, PXFieldSelectingEventArgs args)
        {
            if (args.Row is RequestForInformationRelation requestForInformationRelation)
            {
                var allowedTypes = GetRoleTypes(requestForInformationRelation);
                var enabled = IsEnabled(requestForInformationRelation);
                args.ReturnState = GetFieldState(args.ReturnState, allowedTypes, enabled);
            }
        }

        public void FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs args)
        {
            if (cache.Graph.Views.ContainsKey(nameof(RequestForInformationRelationDocumentSelectorAttribute)))
            {
                cache.Graph.Views[nameof(RequestForInformationRelationDocumentSelectorAttribute)].RequestRefresh();
            }
        }

        private PXFieldState GetFieldState(object returnState, string[] allowedTypes, bool enabled)
        {
            var fieldState = CreateFieldState(returnState, allowedTypes);
            fieldState.Enabled = enabled;
            return fieldState;
        }

        private PXFieldState CreateFieldState(object originalFieldState, string[] allowedTypes)
        {
            return PXStringState.CreateInstance(originalFieldState,
                null, null, _FieldName, null, -1, null, allowedTypes, allowedTypes, true, null);
        }

        private static bool IsEnabled(RequestForInformationRelation requestForInformationRelation)
        {
            return requestForInformationRelation.Role != null;
        }

        private static string[] GetRoleTypes(RequestForInformationRelation requestForInformationRelation)
        {
            switch (requestForInformationRelation.Role)
            {
                case null:
                    return new string[0];
                case RequestForInformationRoleListAttribute.RelatedEntity:
                    return GetTypesForRelatedEntityRole();
                default:
                    return GetTypesForOtherRoles();
            }
        }

        private static string[] GetTypesForRelatedEntityRole()
        {
            return new[]
            {
                Project,
                ProjectTask,
                PurchaseOrder,
                Subcontract,
                ApInvoice,
                ArInvoice,
                RequestForInformation
            };
        }

        private static string[] GetTypesForOtherRoles()
        {
            return new[]
            {
                Contact
            };
        }

        public sealed class apInvoice : BqlString.Constant<apInvoice>
        {
            public apInvoice()
                : base(ApInvoice)
            {
            }
        }

        public sealed class arInvoice : BqlString.Constant<arInvoice>
        {
            public arInvoice()
                : base(ArInvoice)
            {
            }
        }

        public sealed class requestForInformation : BqlString.Constant<requestForInformation>
        {
            public requestForInformation()
                : base(RequestForInformation)
            {
            }
        }
    }
}