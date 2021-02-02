namespace PX.Objects.PJ.ProjectManagement.PJ.DAC
{
    public interface IStatus : IDocumentWithConfigurableStatus
    {
        string Name
        {
            get;
            set;
        }

        string Description
        {
            get;
            set;
        }

        bool? IsDefault
        {
            get;
            set;
        }
    }
}