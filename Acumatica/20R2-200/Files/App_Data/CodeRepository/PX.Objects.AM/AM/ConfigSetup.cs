using PX.Data;

namespace PX.Objects.AM
{
    public class ConfigSetup : PXGraph<ConfigSetup>
    {
        public PXSave<AMConfiguratorSetup> Save;
        public PXCancel<AMConfiguratorSetup> Cancel;

        public PXSelect<AMConfiguratorSetup> ConfigSetupRecord;
    }
}