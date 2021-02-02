using PX.Objects.PJ.Common.Services;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.PJ.Submittals.PJ.DAC;
using PX.Objects.PJ.Submittals.PJ.Graphs;
using PX.Objects.CR;
using System.Collections.Generic;
using System.Linq;

namespace PX.Objects.PJ.Submittals.PJ.Services
{
    public class SubmittalEmailService : EmailActivityService<PJSubmittal>
    {
        private readonly SubmittalEntry SubmittalGraph;

        public SubmittalEmailService(SubmittalEntry graph)
            : base(graph, graph.Submittals.Current.OwnerID)
        {
            SubmittalGraph = graph;
            Entity = graph.Submittals.Current;
        }

        public string EmailSubject => $"SU #[{Entity.SubmittalID}-{Entity.RevisionID} {GetProjectNumber()}] {Entity.Summary}";

        public PXGraph GetEmailActivityGraph()
        {
            return GetEmailActivityGraph<PJSubmittal.noteID>();
        }

        public override string GetRecipientEmails()
        {
            var cache = SubmittalGraph.Caches<PJSubmittalWorkflowItem>();

            var selectedContacts = cache
                .Updated
                .Cast<PJSubmittalWorkflowItem>()
                .Where(it => it.Selected == true && it.ContactID != null)
                .Select(it => it.ContactID)
                .ToArray();

            var emailList = SelectFrom<Contact>
                .Where<Contact.contactID.IsIn<P.AsInt>>
                .View
                .Select(SubmittalGraph, selectedContacts)
                .FirstTableItems
                .Select(contact => contact.EMail)
                .Where(email => !string.IsNullOrWhiteSpace(email))
                .ToList();

            cache.Clear();

            return string.Join(";", emailList);
        }

        protected override string GetSubject() => EmailSubject;
    }
}
