using System;
using System.Collections.Generic;
using System.Linq;
using PX.Data;
using PX.Objects.CN.Common.Extensions;
using PX.Objects.CN.Compliance.PM.CacheExtensions;
using PX.Objects.CN.Compliance.PM.DAC;
using PX.Objects.CS;
using PX.Objects.PM;

namespace PX.Objects.CN.Compliance.PM.GraphExtensions.LienWaiver
{
    public class ProjectEntryLienWaiverExtension : LienWaiverBaseExtension<ProjectEntry>
    {
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.construction>();
        }

        [PXOverride]
        public void DefaultFromTemplate(PMProject project, int? templateId,
            ProjectEntry.DefaultFromTemplateSettings settings,
            Action<PMProject, int?, ProjectEntry.DefaultFromTemplateSettings> baseHandler)
        {
            baseHandler(project, templateId, settings);
            UpdateThroughDateSourceFieldsFromTemplate(project, templateId);
            InsertRecipientsFromTemplate(project, templateId);
        }

        private void UpdateThroughDateSourceFieldsFromTemplate(PMProject project, int? templateId)
        {
			var template = PXSelect<PMProject, Where<PMProject.contractID, Equal<Required<PMProject.contractID>>>>.Select(Base, templateId);
			var projectExtension = PXCache<PMProject>.GetExtension<PmProjectExtension>(project);
            var templateExtension = PXCache<PMProject>.GetExtension<PmProjectExtension>(template);
            projectExtension.ThroughDateSourceConditional = templateExtension.ThroughDateSourceConditional;
            projectExtension.ThroughDateSourceUnconditional = templateExtension.ThroughDateSourceUnconditional;
        }

        private void InsertRecipientsFromTemplate(PMProject project, int? templateId)
        {
            var projectRecipients = CreateProjectRecipients(project, templateId);
            LienWaiverRecipients.Cache.InsertAll(projectRecipients);
        }

        private IEnumerable<LienWaiverRecipient> CreateProjectRecipients(PMProject project, int? templateId)
        {
            var templateRecipients =
                Base.Select<LienWaiverRecipient>().Where(lwr => lwr.ProjectId == templateId);
            return templateRecipients.Select(tr => new LienWaiverRecipient
            {
                VendorClassId = tr.VendorClassId,
                ProjectId = project.ContractID,
                MinimumCommitmentAmount = tr.MinimumCommitmentAmount
            });
        }
    }
}
