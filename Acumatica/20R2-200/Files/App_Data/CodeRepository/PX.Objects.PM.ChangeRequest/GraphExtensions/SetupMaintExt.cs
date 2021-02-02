using PX.Data;


namespace PX.Objects.PM.ChangeRequest.GraphExtensions
{
	public class SetupMaintExt : PXGraphExtension<PX.Objects.PM.SetupMaint>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<CS.FeaturesSet.changeRequest>();
		}

		//Drag & drop not available to configure through customization
		//public PXOrderedSelect<PMSetup, PMMarkup,
		//	InnerJoin<PMProject, On<PMProject.nonProject, Equal<True>>>,
		//	Where<PMMarkup.projectID, Equal<PMProject.contractID>>,
		//	OrderBy<Asc<PMMarkup.sortOrder>>> Markups;

		public PXSelectJoin<PMMarkup,
			InnerJoin<PMProject, On<PMProject.nonProject, Equal<True>>>,
			Where<PMMarkup.projectID, Equal<PMProject.contractID>>,
			OrderBy<Asc<PMMarkup.lineNbr>>> Markups;

		protected virtual void _(Events.FieldDefaulting<PMMarkup, PMMarkup.projectID> e)
		{
			e.NewValue = ProjectDefaultAttribute.NonProject();
		}
	}
}
