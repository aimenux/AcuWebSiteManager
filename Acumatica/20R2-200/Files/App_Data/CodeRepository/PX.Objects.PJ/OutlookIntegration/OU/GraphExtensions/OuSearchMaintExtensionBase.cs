using System;
using System.Linq;
using PX.Objects.PJ.OutlookIntegration.OU.DAC;
using PX.Data;
using PX.Objects.CR;

namespace PX.Objects.PJ.OutlookIntegration.OU.GraphExtensions
{
    public class OuSearchMaintExtensionBase : PXGraphExtension<OUSearchMaint>
    {
        public PXFilter<RequestForInformationOutlook> RequestForInformationOutlook;
        public PXFilter<ProjectIssueOutlook> ProjectIssueOutlook;

        public void CreateEntity(Action createEntity)
        {
            try
            {
                Base.Filter.Current.ErrorMessage = null;
                using (var scope = new PXTransactionScope())
                {
                    createEntity();
                    scope.Complete();
                }
                Base.Filter.Current.Operation = null;
            }
            catch (PXOuterException exception)
            {
                Base.Filter.Current.ErrorMessage = exception.InnerMessages.First();
            }
            catch (Exception exception)
            {
                Base.Filter.Current.ErrorMessage = exception.InnerException?.Message ?? exception.Message;
            }
        }
    }
}
