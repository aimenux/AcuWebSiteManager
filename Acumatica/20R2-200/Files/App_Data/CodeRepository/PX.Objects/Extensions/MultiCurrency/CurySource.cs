using System;
using PX.Data;

namespace PX.Objects.Extensions.MultiCurrency
{
    /// <summary>A mapped cache extension that contains information on the currency source.</summary>
    public class CurySource : PXMappedCacheExtension
    {
        #region CuryID
        /// <exclude />
        public abstract class curyID : PX.Data.BQL.BqlString.Field<curyID> { }
        /// <summary>The identifier of the currency in the system.</summary>
        public virtual String CuryID
        {
			get;
			set;
        }
        #endregion
        #region CuryRateTypeID
        /// <exclude />
        public abstract class curyRateTypeID : PX.Data.BQL.BqlString.Field<curyRateTypeID> { }
        /// <summary>The identifier of the currency rate type in the system.</summary>
        public virtual String CuryRateTypeID
        {
			get;
			set;
        }
        #endregion
        #region AllowOverrideCury
        /// <exclude />
        public abstract class allowOverrideCury : PX.Data.BQL.BqlBool.Field<allowOverrideCury> { }
        /// <summary>A property that indicates (if set to <tt>true</tt>) that the currency of the customer documents (<see cref="CuryID" />) can be overridden by a user during document entry.</summary>
        public virtual Boolean? AllowOverrideCury
        {
			get;
			set;
        }
        #endregion
        #region AllowOverrideRate
        /// <exclude />
        public abstract class allowOverrideRate : PX.Data.BQL.BqlBool.Field<allowOverrideRate> { }
        /// <summary>A property that indicates (if set to <tt>true</tt>) that the currency rate for the customer documents (which is calculated by the system based on the currency rate history) can be overridden
        /// by a user during document entry.</summary>
        public virtual Boolean? AllowOverrideRate
        {
			get;
			set;
        }
        #endregion
    }
}
