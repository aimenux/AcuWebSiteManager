using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using PX.Data;
using PX.Objects.CN.Common.Extensions;
using PX.Objects.CN.Subcontracts.EP.Attributes;
using PX.Objects.CN.Subcontracts.PO.Extensions;
using PX.Objects.CN.Subcontracts.SC.Graphs;
using PX.Objects.CS;
using PX.Objects.EP;
using PX.Objects.PO;
using EpMessages = PX.Objects.EP.Messages;

namespace PX.Objects.CN.Subcontracts.EP.GraphExtensions
{
    public class EpApprovalProcessExt : PXGraphExtension<EPApprovalProcess>
    {
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.construction>();
        }

        public override void Initialize()
        {
            Base.Records.SetProcessDelegate(Approve);
        }

        [PXMergeAttributes(Method = MergeMethod.Merge)]
        [PXRemoveBaseAttribute(typeof(PXFormulaAttribute))]
        [PXFormula(typeof(ApprovalDocTypeExt))]
        public virtual void _(Events.CacheAttached<EPApprovalProcess.EPOwned.docType> args)
        {
        }

        [PXUIField(MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXEditDetailButton]
        public virtual IEnumerable editDetail(PXAdapter adapter)
        {
            var epOwned = Base.Records.Current;
            var subcontract = epOwned.GetSubcontractEntity(Base);
            if (subcontract == null)
            {
                return Base.editDetail(adapter);
            }
            var graph = PXGraph.CreateInstance<SubcontractEntry>();
            graph.Document.Current = subcontract;
            throw new PXRedirectRequiredException(graph, string.Empty)
            {
                Mode = PXBaseRedirectException.WindowMode.NewWindow
            };
        }

        private static void Approve(List<EPApprovalProcess.EPOwned> approvals)
        {
            var subcontractApprovalDictionary = ExtractSubcontracts(approvals);

            bool entitiesApproveErrorOccured = false;
            if (approvals.Any())
            {
                entitiesApproveErrorOccured = ApproveNonSubcontractEntities(approvals);
            }

            bool subcontractsApproveErrorOccured = false;
            if (subcontractApprovalDictionary.Any())
            {
                var graph = PXGraph.CreateInstance<SubcontractEntry>();
                subcontractsApproveErrorOccured  = ApproveSubcontracts(graph, subcontractApprovalDictionary);
            }

            ThrowErrorMessageIfRequired(entitiesApproveErrorOccured || subcontractsApproveErrorOccured);
        }

        private static bool ApproveNonSubcontractEntities(IReadOnlyCollection<EPApprovalProcess.EPOwned> approvals)
        {
            try
            {
                ApproveEntities(approvals);
                return false;
            }
            catch
            {
                return true;
            }
        }

        private static void ApproveEntities(IReadOnlyCollection<EPApprovalProcess.EPOwned> approvals)
        {
            const BindingFlags bindingFlags = BindingFlags.Static | BindingFlags.NonPublic;
            var baseApproveMethod = typeof(EPApprovalProcess).GetMethod("Approve", bindingFlags);
            baseApproveMethod?.Invoke(null, new object[]
            {
                approvals,
                true
            });
        }

        private static bool ApproveSubcontracts(SubcontractEntry graph,
            Dictionary<EPApprovalProcess.EPOwned, POOrder> subcontractApprovalDictionary)
        {
            return subcontractApprovalDictionary.Select(x => ApproveSubcontract(graph, x)).ToList().Any(x => x);
        }

        private static bool ApproveSubcontract(SubcontractEntry graph,
            KeyValuePair<EPApprovalProcess.EPOwned, POOrder> subcontractApprovalPair)
        {
            try
            {
                PXProcessing<EPApproval>.SetCurrentItem(subcontractApprovalPair.Key);
                ApproveSingleSubcontract(graph, subcontractApprovalPair.Value);
                PXProcessing<EPApproval>.SetProcessed();
                return false;
            }
            catch (Exception exception)
            {
                PXProcessing<EPApproval>.SetError(exception);
                return true;
            }
        }

        private static void ThrowErrorMessageIfRequired(bool errorOccured)
        {
            if (errorOccured)
            {
                throw new PXOperationCompletedWithErrorException(ErrorMessages.SeveralItemsFailed);
            }
        }

        private static Dictionary<EPApprovalProcess.EPOwned, POOrder> ExtractSubcontracts(ICollection<EPApprovalProcess.EPOwned> approvals)
        {
            var graph = PXGraph.CreateInstance<PXGraph>();
            var subcontractApprovalDictionary = new Dictionary<EPApprovalProcess.EPOwned, POOrder>();
            foreach (var approval in GetCommitmentApprovals(approvals))
            {
                var commitment = GetCommitment(graph, approval.RefNoteID);
                if (commitment.OrderType == POOrderType.RegularSubcontract)
                {
                    subcontractApprovalDictionary.Add(approval, commitment);
                    approvals.Remove(approval);
                }
            }
            return subcontractApprovalDictionary;
        }

        private static void ApproveSingleSubcontract(SubcontractEntry graph, POOrder subcontract)
        {
            SetupGraphForApproval(graph, subcontract);
            CheckActionExisting(graph);
            PressApprove(graph, subcontract);
            graph.Persist();
        }

        private static void SetupGraphForApproval(SubcontractEntry graph, POOrder subcontract)
        {
            graph.Clear();
            graph.Document.Current = subcontract;
            SetupAutomationStep(graph, subcontract);
        }

        private static void PressApprove(SubcontractEntry graph, POOrder subcontract)
        {
            var action = graph.Actions[ActionsMessages.Action];
            var adapter = CreateAdapter(graph, subcontract);
            // method Press is lazy, foreach needed for full completion
            foreach (var dummy in action.Press(adapter))
            {
            }
        }

        private static PXAdapter CreateAdapter(SubcontractEntry graph, POOrder subcontract)
        {
            var dummyView = new DummyView(graph, subcontract);
            return new PXAdapter(dummyView)
            {
                Menu = nameof(Approve)
            };
        }

        private static void CheckActionExisting(SubcontractEntry graph)
        {
            if (!graph.Actions.Contains(ActionsMessages.Action))
            {
                throw new PXException(PXMessages.LocalizeFormatNoPrefixNLA(EpMessages.AutomationNotConfigured, graph));
            }
        }

        private static void SetupAutomationStep(SubcontractEntry graph, POOrder subcontract)
        {
            PXAutomation.GetView(graph);
            PXAutomation.GetStep(graph, subcontract.CreateArray(),
                BqlCommand.CreateInstance(typeof(Select<>), typeof(POOrder)));
        }

        private static IEnumerable<EPApprovalProcess.EPOwned> GetCommitmentApprovals(
            IEnumerable<EPApprovalProcess.EPOwned> approvals)
        {
            return approvals.Where(x => x.EntityType == typeof(POOrder).FullName).ToList();
        }

        private static POOrder GetCommitment(PXGraph graph, Guid? noteId)
        {
            var query = new PXSelect<POOrder,
                Where<POOrder.noteID, Equal<Required<POOrder.noteID>>>>(graph);
            return query.SelectSingle(noteId);
        }

        private class DummyView : PXView
        {
            private readonly List<object> records;

            public DummyView(PXGraph graph, object record)
                : base(graph, true, graph.Views[graph.AutomationView].BqlSelect)
            {
                records = new List<object>
                {
                    record
                };
            }

            public override List<object> Select(object[] currents, object[] parameters,
                object[] searches, string[] sortColumns, bool[] descendings,
                PXFilterRow[] filters, ref int startRow, int maximumRows, ref int totalRows)
            {
                return records;
            }
        }
    }
}