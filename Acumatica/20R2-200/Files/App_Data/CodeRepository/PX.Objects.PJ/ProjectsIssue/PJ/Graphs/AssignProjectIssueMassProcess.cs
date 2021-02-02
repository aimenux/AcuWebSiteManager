using PX.Objects.PJ.ProjectManagement.PJ.DAC;
using PX.Objects.PJ.ProjectManagement.PJ.Graphs;
using PX.Objects.PJ.ProjectsIssue.PJ.DAC;
using PX.Objects.PJ.ProjectsIssue.PJ.Descriptor.Attributes;
using PX.Data;
using PX.SM;
using Messages = PX.Objects.CR.Messages;

namespace PX.Objects.PJ.ProjectsIssue.PJ.Graphs
{
    public class AssignProjectIssueMassProcess : AssignBaseMassProcess<AssignProjectIssueMassProcess,
        ProjectIssue, ProjectManagementSetup.projectIssueAssignmentMapId>
    {
        [PXViewName(Messages.MatchingRecords)]
        [PXFilterable]
        [PXViewDetailsButton(typeof(ProjectIssue))]
        public PXProcessing<ProjectIssue,
                Where<ProjectIssue.status, NotEqual<ProjectIssueStatusAttribute.closed>>> ProjectIssues;
    }
}
