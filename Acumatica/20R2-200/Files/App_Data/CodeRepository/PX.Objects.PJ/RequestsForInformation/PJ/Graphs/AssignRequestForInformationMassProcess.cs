using PX.Objects.PJ.ProjectManagement.PJ.DAC;
using PX.Objects.PJ.ProjectManagement.PJ.Graphs;
using PX.Objects.PJ.RequestsForInformation.PJ.DAC;
using PX.Objects.PJ.RequestsForInformation.PJ.Descriptor.Attributes;
using PX.Data;
using PX.SM;
using Messages = PX.Objects.CR.Messages;

namespace PX.Objects.PJ.RequestsForInformation.PJ.Graphs
{
    public class AssignRequestForInformationMassProcess : AssignBaseMassProcess<AssignRequestForInformationMassProcess,
        RequestForInformation, ProjectManagementSetup.requestForInformationAssignmentMapId>
    {
        [PXViewName(Messages.MatchingRecords)]
        [PXFilterable]
        [PXViewDetailsButton(typeof(RequestForInformation))]
        public PXProcessing<RequestForInformation,
                Where<RequestForInformation.status, NotEqual<RequestForInformationStatusAttribute.closedStatus>>>
            RequestsForInformation;
    }
}
