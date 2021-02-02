using System.Collections.Generic;

namespace PX.Objects.Common.GraphExtensions.Abstract
{
    public interface IDocumentWithFinDetailsGraphExtension
    {
        List<int?> GetOrganizationIDsInDetails();
    }
}
