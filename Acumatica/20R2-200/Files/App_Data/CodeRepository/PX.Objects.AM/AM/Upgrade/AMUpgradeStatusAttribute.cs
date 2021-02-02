using PX.Data;

namespace PX.Objects.AM.Upgrade
{
    [PXDBInt]
    [PXDefault(UpgradeVersions.CurrentVersion)]
    [PXUIField(DisplayName = "Upgrade Status", Enabled = false, Visible = false)]
    public class AMUpgradeStatusAttribute : PX.Objects.GL.AcctSubAttribute
    {

    }
}