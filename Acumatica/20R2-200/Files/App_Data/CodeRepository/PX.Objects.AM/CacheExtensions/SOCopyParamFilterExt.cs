using System;
using PX.Data;
using PX.Objects.SO;

namespace PX.Objects.AM.CacheExtensions
{
    /// <summary>
    /// Manufacturing extension on Sales Order - Copy To smart panel
    /// Filter = <see cref="CopyParamFilter"/>
    /// </summary>
    [Serializable]
    public sealed class SOCopyParamFilterExt : PXCacheExtension<CopyParamFilter>
    {
        #region AMIncludeEstimate
        public abstract class aMIncludeEstimate : PX.Data.BQL.BqlString.Field<aMIncludeEstimate> { }

        /// <summary>
        /// Indicates if the estimates should be included in the copy.
        /// This option also checks the settings for the SOOrderType.AMAllowEstimates and performs a copy or convert depending on the setup and from/to order types.
        /// </summary>
        [PXBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Copy/Convert Estimates")]
        public bool? AMIncludeEstimate { get; set; }
        #endregion

        #region CopyConfigurations
        public abstract class copyConfigurations : PX.Data.BQL.BqlBool.Field<copyConfigurations> { }

        [PXBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Copy Configurations")]
        public bool? CopyConfigurations { get; set; }
        #endregion CopyConfigurations

        public static int DetermineAction(CopyParamFilter filter, SOOrderType fromOrderType, SOOrderType toOrderType)
        {
            return DetermineAction(
                PXCache<CopyParamFilter>.GetExtension<SOCopyParamFilterExt>(filter),
                PXCache<SOOrderType>.GetExtension<SOOrderTypeExt>(fromOrderType),
                PXCache<SOOrderType>.GetExtension<SOOrderTypeExt>(toOrderType)
            );
        }

        public static int DetermineAction(SOCopyParamFilterExt filter, SOOrderTypeExt fromOrderType, SOOrderTypeExt toOrderType)
        {
            if (filter == null
                || !filter.AMIncludeEstimate.GetValueOrDefault()
                || fromOrderType == null
                || !fromOrderType.AMEstimateEntry.GetValueOrDefault()
                || toOrderType == null)
            {
                return EstimateAction.NoAction;
            }

            return toOrderType.AMEstimateEntry.GetValueOrDefault()
                ? EstimateAction.CopyAction
                : EstimateAction.ConvertAction;
        }

        /// <summary>
        /// Estimate copy order actions
        /// </summary>
        public class EstimateAction
        {
            /// <summary>
            /// Indicates there is no action taken on the estimates of the order being copied. 
            /// The new order will not have any reference to any estimate.
            /// </summary>
            public const int NoAction = 1;
            /// <summary>
            /// Copy the current estimate to a new estimate on the new order.
            /// </summary>
            public const int CopyAction = 2;
            /// <summary>
            /// Convert the current estimates to document details on the new order.
            /// </summary>
            public const int ConvertAction = 3;
        }
    }
}