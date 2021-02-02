using AutoMapper;
using PX.Objects.PJ.Common.Descriptor;
using PX.Objects.PJ.DrawingLogs.PJ.DAC;
using PX.Objects.PJ.ProjectsIssue.PJ.DAC;
using PX.Objects.PJ.RequestsForInformation.PJ.DAC;

namespace PX.Objects.PJ.Common.Mappers
{
    public class DrawingLogRelationMapperProfile : Profile
    {
        public DrawingLogRelationMapperProfile()
        {
            CreateMap<RequestForInformation, UnlinkedDrawingLogRelation>()
                .ForMember(relation => relation.DueDate, member => member
                    .MapFrom(requestForInformation => requestForInformation.DueResponseDate))
                .ForMember(relation => relation.DocumentCd, member => member
                    .MapFrom(requestForInformation => requestForInformation.RequestForInformationCd))
                .ForMember(relation => relation.DocumentId, member => member
                    .MapFrom(requestForInformation => requestForInformation.NoteID))
                .ForMember(relation => relation.DocumentType, member => member
                    .UseValue(CacheNames.RequestForInformation));
            CreateMap<ProjectIssue, UnlinkedDrawingLogRelation>()
                .ForMember(relation => relation.DocumentId, member => member
                    .MapFrom(projectIssue => projectIssue.NoteID))
                .ForMember(relation => relation.DocumentCd, member => member
                    .MapFrom(projectIssue => projectIssue.ProjectIssueCd))
                .ForMember(relation => relation.DocumentType, member => member
                    .UseValue(CacheNames.ProjectIssue));
            CreateMap<RequestForInformation, LinkedDrawingLogRelation>()
                .ForMember(relation => relation.DueDate, member => member
                    .MapFrom(requestForInformation => requestForInformation.DueResponseDate))
                .ForMember(relation => relation.DocumentCd, member => member
                    .MapFrom(requestForInformation => requestForInformation.RequestForInformationCd))
                .ForMember(relation => relation.DocumentId, member => member
                    .MapFrom(requestForInformation => requestForInformation.NoteID))
                .ForMember(relation => relation.DocumentType, member => member
                    .UseValue(CacheNames.RequestForInformation));
            CreateMap<ProjectIssue, LinkedDrawingLogRelation>()
                .ForMember(relation => relation.DocumentId, member => member
                    .MapFrom(projectIssue => projectIssue.NoteID))
                .ForMember(relation => relation.DocumentCd, member => member
                    .MapFrom(projectIssue => projectIssue.ProjectIssueCd))
                .ForMember(relation => relation.DocumentType, member => member
                    .UseValue(CacheNames.ProjectIssue));
            CreateMap<UnlinkedDrawingLogRelation, LinkedDrawingLogRelation>();
            CreateMap<LinkedDrawingLogRelation, UnlinkedDrawingLogRelation>();
            CreateMap<LinkedDrawingLogRelation, LinkedDrawingLogRelation>()
                .ForMember(relation => relation.Selected, member => member
                    .Ignore());
        }
    }
}
