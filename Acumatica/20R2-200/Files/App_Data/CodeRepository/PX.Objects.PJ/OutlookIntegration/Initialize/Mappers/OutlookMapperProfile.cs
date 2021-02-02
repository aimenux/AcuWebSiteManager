using AutoMapper;
using PX.Objects.PJ.OutlookIntegration.OU.DAC;
using PX.Objects.PJ.ProjectsIssue.PJ.DAC;
using PX.Objects.PJ.RequestsForInformation.PJ.DAC;

namespace PX.Objects.PJ.OutlookIntegration.Initialize.Mappers
{
    public class OutlookMapperProfile : Profile
    {
        public OutlookMapperProfile()
        {
            CreateMap<ProjectIssueOutlook, ProjectIssue>();
            CreateMap<RequestForInformationOutlook, RequestForInformation>();
        }
    }
}
