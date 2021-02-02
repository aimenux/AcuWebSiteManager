namespace PX.Objects.PJ.ProjectManagement.PJ.DAC
{
    public interface IProjectManagementDocumentBase
    {
        int? ProjectId
        {
            get;
            set;
        }

        int? ProjectTaskId
        {
            get;
            set;
        }
    }
}