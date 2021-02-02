<%@ Application Language="C#" Inherits="PX.Web.PXApplication" %>
<script RunAt="server">
    protected override void MergeAdditionalAssemblyResources()
    {
        PX.Web.UI.AssemblyResourceProvider.MergeAssemblyResourcesIntoWebsite<PX.Web.Controls.PXResPanelEditor>();
    }

    protected override void Initialization_ProcessApplication()
    {
        Initialization.ProcessApplication();
    }
</script>

