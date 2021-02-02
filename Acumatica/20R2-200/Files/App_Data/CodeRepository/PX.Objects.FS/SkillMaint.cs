using PX.Data;

namespace PX.Objects.FS 
{
    public class SkillMaint : PXGraph<SkillMaint, FSSkill>
    {
        [PXImport(typeof(FSSkill))]
        public PXSelect<FSSkill> SkillRecords;
    }
}