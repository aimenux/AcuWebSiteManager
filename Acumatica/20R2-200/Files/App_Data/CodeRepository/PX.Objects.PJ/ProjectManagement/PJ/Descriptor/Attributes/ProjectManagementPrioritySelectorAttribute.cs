using System;
using PX.Objects.PJ.ProjectManagement.Descriptor;
using PX.Objects.PJ.ProjectManagement.PJ.DAC;
using PX.Data;
using PX.Objects.GL;

namespace PX.Objects.PJ.ProjectManagement.PJ.Descriptor.Attributes
{
    [PXRestrictor(typeof(Where<ProjectManagementClassPriority.isActive, Equal<True>>),
        ProjectManagementMessages.OnlyActivePrioritiesAreAllowed)]
    public sealed class ProjectManagementPrioritySelectorAttribute : AcctSubAttribute
    {
        public ProjectManagementPrioritySelectorAttribute(Type classIdField)
        {
            var searchType = BqlCommand.Compose(
                typeof(Search<,,>),
                typeof(ProjectManagementClassPriority.priorityId),
                typeof(Where<,>),
                typeof(ProjectManagementClassPriority.classId),
                typeof(Equal<>),
                typeof(Current<>),
                classIdField,
                typeof(OrderBy<Asc<ProjectManagementClassPriority.sortOrder>>));
            CreateSelectorAttribute(searchType);
        }

        public ProjectManagementPrioritySelectorAttribute()
        {
            var searchType = typeof(Search3<ProjectManagementClassPriority.priorityId,
                OrderBy<Asc<ProjectManagementClassPriority.sortOrder>>>);
            CreateSelectorAttribute(searchType);
        }

        private void CreateSelectorAttribute(Type searchType)
        {
            var selectorAttribute = new PXSelectorAttribute(searchType)
            {
                DescriptionField = typeof(ProjectManagementClassPriority.priorityName),
                SubstituteKey = typeof(ProjectManagementClassPriority.priorityName)
            };
            _Attributes.Add(selectorAttribute);
        }
    }
}
