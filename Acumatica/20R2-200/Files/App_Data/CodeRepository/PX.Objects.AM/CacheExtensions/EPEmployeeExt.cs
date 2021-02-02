using System;
using PX.Data;
using PX.Objects.EP;

namespace PX.Objects.AM.CacheExtensions
{
    /// <summary>
    /// Manufacturing Employee Extension
    /// </summary>
    [Serializable]
    public sealed class EPEmployeeExt : PXCacheExtension<EPEmployee>
    {
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<CS.FeaturesSet.manufacturing>();
        }

        #region AMProductionEmployee
        /// <summary>
        /// Indicates an employee is a production employee for use in Manufacturing processes such as 
        /// labor transactions and scheduling as a resource
        /// </summary>
        public abstract class amProductionEmployee : PX.Data.BQL.BqlBool.Field<amProductionEmployee> { }

        /// <summary>
        /// Indicates an employee is a production employee for use in Manufacturing processes such as 
        /// labor transactions and scheduling as a resource
        /// </summary>
        [PXDBBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Production Employee")]
        public Boolean? AMProductionEmployee { get; set; }
        #endregion
    }
}
