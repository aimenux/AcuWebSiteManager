using System;
using PX.Objects.AM.GraphExtensions;
using PX.Data;
using PX.Objects.AM.CacheExtensions;
using PX.Objects.CS;
using PX.Objects.IN;

namespace PX.Objects.AM
{
    /// <summary>
    /// Manufacturing GL Account Helper
    /// </summary>
    public static class GLAccountHelper
    {
        public const string MaskItem = INAcctSubDefault.MaskItem;
        public const string MaskSite = INAcctSubDefault.MaskSite;
        public const string MaskClass = INAcctSubDefault.MaskClass;
        public const string MaskMfg = "X";

        public static int? GetAccountDefaults<Field>(PXGraph graph, InventoryItem inventoryItem, INSite site, INPostClass postclass, 
            AMOrderType amOrderType) where Field : IBqlField
        {
            if (graph == null || inventoryItem == null
                || postclass == null || amOrderType == null)
            {
                // site ok for null when multi warehouse disabled
                return null;
            }

            var postExtension = PXCache<INPostClass>.GetExtension<INPostClassExt>(postclass);

            if (typeof(Field) == typeof(INPostClassExt.aMWIPAcctID))
            {
                string acctDefault = inventoryItem.StkItem.GetValueOrDefault() || postExtension.AMWIPAccountDefault != MaskSite ? postExtension.AMWIPAccountDefault : MaskItem;
                if(acctDefault == MaskSite 
                    && (site == null || !PXAccess.FeatureInstalled<FeaturesSet.warehouse>()))
                {
                    acctDefault = MaskMfg;
                }

                return GetAcctID<Field>(graph, acctDefault, inventoryItem, site, postclass, amOrderType);
            }

            if (typeof(Field) == typeof(INPostClassExt.aMWIPVarianceAcctID))
            {
                string acctDefault = inventoryItem.StkItem.GetValueOrDefault() || postExtension.AMWIPVarianceAccountDefault != MaskSite ? postExtension.AMWIPVarianceAccountDefault : MaskItem;
                if (acctDefault == MaskSite
                    && (site == null || !PXAccess.FeatureInstalled<FeaturesSet.warehouse>()))
                {
                    acctDefault = MaskMfg;
                }

                return GetAcctID<Field>(graph, acctDefault, inventoryItem, site, postclass, amOrderType);
            }

            if (typeof(Field) == typeof(INPostClassExt.aMWIPSubID))
            {
                //This allows no site to still function by defaulting back to order type
                INSite site2 = site ?? new INSite {SiteID = -1, SiteCD = "-1"};
                var siteExtension = PXCache<INSite>.GetExtension<INSiteExt>(site2);
                if (siteExtension != null && siteExtension.AMWIPSubID == null)
                {
                    siteExtension.AMWIPSubID = amOrderType.WIPSubID;
                }

                var inventoryExtension = PXCache<InventoryItem>.GetExtension<InventoryItemExt>(inventoryItem);
                if (inventoryExtension.AMWIPSubID == null)
                {
                    inventoryExtension.AMWIPSubID = amOrderType.WIPSubID;
                }
                if (postExtension.AMWIPSubID == null)
                {
                    postExtension.AMWIPSubID = amOrderType.WIPSubID;
                }

                return GetSubID<Field>(graph, postExtension.AMWIPAccountDefault, postExtension.AMWIPSubMask, inventoryItem, site2, 
                    postclass, amOrderType);
            }

            if (typeof(Field) == typeof(INPostClassExt.aMWIPVarianceSubID))
            {
                //This allows no site to still function by defaulting back to order type
                INSite site2 = site ?? new INSite { SiteID = -1, SiteCD = "-1" };
                var siteExtension = PXCache<INSite>.GetExtension<INSiteExt>(site2);
                if (siteExtension != null && siteExtension.AMWIPVarianceSubID == null)
                {
                    siteExtension.AMWIPVarianceSubID = amOrderType.WIPVarianceSubID;
                }
                var inventoryExtension = PXCache<InventoryItem>.GetExtension<InventoryItemExt>(inventoryItem);
                if (inventoryExtension.AMWIPVarianceSubID == null)
                {
                    inventoryExtension.AMWIPVarianceSubID = amOrderType.WIPVarianceSubID;
                }
                if (postExtension.AMWIPVarianceSubID == null)
                {
                    postExtension.AMWIPVarianceSubID = amOrderType.WIPVarianceSubID;
                }

                return GetSubID<Field>(graph, postExtension.AMWIPVarianceAccountDefault, postExtension.AMWIPVarianceSubMask, 
                    inventoryItem, site2, postclass, amOrderType);
            }

            throw new PXException(Messages.UnableToGetAccountDefault, typeof(Field).Name.ToCapitalized());
        }

        public static int? GetAcctID<Field>(PXGraph graph, string AcctDefault, InventoryItem inventoryItem, INSite site, INPostClass postClass, AMOrderType amOrderType) where Field : IBqlField
        {
            switch (AcctDefault)
            {
                case MaskSite:
                    PXCache cache1 = graph.Caches[typeof(INSiteExt)];
                    try
                    {
                        return new int?((int)cache1.GetValue<Field>((object)site));
                    }
                    catch (NullReferenceException )
                    {
                        return GetSetupAcctID<Field>(graph, inventoryItem, site, postClass, amOrderType);
                    }
                case MaskClass:
                    PXCache cache2 = graph.Caches[typeof(INPostClassExt)];
                    try
                    {
                        return new int?((int)cache2.GetValue<Field>((object)postClass));
                    }
                    catch (NullReferenceException )
                    {
                        return GetSetupAcctID<Field>(graph, inventoryItem, site, postClass, amOrderType);
                    }
                case MaskItem:
                    PXCache cache3 = graph.Caches[typeof(InventoryItemExt)];
                    try
                    {
                        return new int?((int)cache3.GetValue<Field>((object)inventoryItem));
                    }
                    catch (NullReferenceException )
                    {
                        return GetSetupAcctID<Field>(graph, inventoryItem, site, postClass, amOrderType);
                    }
                default:
                    PXCache cache4 = graph.Caches[typeof(AMOrderType)];
                    try
                    {
                        return ConvertFieldToSetup<Field>(graph, amOrderType);
                    }
                    catch (NullReferenceException )
                    {
                        return null;
                    }
            }
        }

        /// <summary>
        /// Convert the given field to its related production setup default account value.
        /// Used to get a default value when the posting class account location is not available or null
        /// </summary>
        /// <typeparam name="Field"></typeparam>
        /// <param name="graph">Calling graph</param>
        /// <param name="amOrderType">current production setup based on Order Type</param>
        /// <returns>Account/subaccount ID</returns>
        public static int? ConvertFieldToSetup<Field>(PXGraph graph, AMOrderType amOrderType) where Field : IBqlField
        {
            PXCache cache = graph.Caches[typeof(AMOrderType)];

            if (typeof(Field) == typeof(INPostClassExt.aMWIPAcctID)
                || typeof(Field) == typeof(INSiteExt.aMWIPAcctID)
                || typeof(Field) == typeof(InventoryItemExt.aMWIPAcctID)
                || typeof(Field) == typeof(AMOrderType.wIPAcctID))
            {
                return new int?((int)cache.GetValue<AMOrderType.wIPAcctID>((object)amOrderType));
            }
            if (typeof(Field) == typeof(INPostClassExt.aMWIPSubID)
                || typeof(Field) == typeof(INSiteExt.aMWIPSubID)
                || typeof(Field) == typeof(InventoryItemExt.aMWIPSubID)
                || typeof(Field) == typeof(AMOrderType.wIPSubID))
            {
                return new int?((int)cache.GetValue<AMOrderType.wIPSubID>((object)amOrderType));
            }

            if (typeof(Field) == typeof(INPostClassExt.aMWIPVarianceAcctID)
                || typeof(Field) == typeof(INSiteExt.aMWIPVarianceAcctID)
                || typeof(Field) == typeof(InventoryItemExt.aMWIPVarianceAcctID)
                || typeof(Field) == typeof(AMOrderType.wIPVarianceAcctID))
            {
                return new int?((int)cache.GetValue<AMOrderType.wIPVarianceAcctID>((object)amOrderType));
            }

            if (typeof(Field) == typeof(INPostClassExt.aMWIPVarianceSubID)
                || typeof(Field) == typeof(INSiteExt.aMWIPVarianceSubID)
                || typeof(Field) == typeof(InventoryItemExt.aMWIPVarianceSubID)
                || typeof(Field) == typeof(AMOrderType.wIPVarianceSubID))
            {
                return new int?((int)cache.GetValue<AMOrderType.wIPVarianceSubID>((object)amOrderType));
            }

            return null;
        }

        public static int? GetSubID<Field>(PXGraph graph, string AcctDefault, string SubMask, InventoryItem inventoryItem,
            INSite site, INPostClass postclass, AMOrderType amOrderType) where Field : IBqlField
        {
            int? itemSubID = (int?)graph.Caches[typeof(InventoryItemExt)].GetValue<Field>(inventoryItem);
            int? siteSubID = (int?)graph.Caches[typeof(INSiteExt)].GetValue<Field>(site);
            int? classSubID = (int?)graph.Caches[typeof(INPostClassExt)].GetValue<Field>(postclass);
            
            object value = null;

            try
            {
                if (inventoryItem.StkItem.GetValueOrDefault() && typeof(Field) == typeof(INPostClassExt.aMWIPSubID))
                {
                    value = (object)SubAccountMaskAttribute.MakeSub<INPostClassExt.aMWIPSubMask>
                        (graph, SubMask, inventoryItem.StkItem, 
                        new object[3] { itemSubID, siteSubID, classSubID}, 
                        new Type[3] { typeof (InventoryItemExt.aMWIPSubID), typeof (INSiteExt.aMWIPSubID), typeof (INPostClassExt.aMWIPSubID) }
                        );
                }

                if (inventoryItem.StkItem.GetValueOrDefault() && typeof(Field) == typeof(INPostClassExt.aMWIPVarianceSubID))
                {
                    value = (object)SubAccountMaskAttribute.MakeSub<INPostClassExt.aMWIPVarianceSubMask>
                        (graph, SubMask, inventoryItem.StkItem, 
                        new object[3] { itemSubID, siteSubID, classSubID }, 
                        new Type[3] { typeof (InventoryItemExt.aMWIPVarianceSubID), typeof (INSiteExt.aMWIPVarianceSubID), typeof (INPostClassExt.aMWIPVarianceSubID) }
                        );
                }
            }
            catch (PXMaskArgumentException ex)
            {
                object stateExt;
                switch (ex.SourceIdx)
                {   
                    case 1:
                        stateExt = graph.Caches[typeof(INSiteExt)].GetStateExt<INSite.siteCD>((object)site);
                        break;
                    case 2:
                        stateExt = graph.Caches[typeof(INPostClassExt)].GetStateExt<INPostClass.postClassID>((object)postclass);
                        break;
                    case 3:
                        stateExt = graph.Caches[typeof(InventoryItemExt)].GetStateExt<InventoryItem.inventoryCD>((object)inventoryItem);
                        break;
                    default:
                        stateExt = graph.Caches[typeof(AMOrderType)].GetStateExt<AMOrderType.prodNumberingID>((object)amOrderType);
                        break;
                }
                throw new PXMaskArgumentException(ex, stateExt);
            }
            switch (AcctDefault)
            {
            case MaskSite:
                RaiseFieldUpdating<Field>(graph.Caches[typeof(INSiteExt)], site, ref value);
                break;
            case MaskClass:
                RaiseFieldUpdating<Field>(graph.Caches[typeof(INPostClassExt)], postclass, ref value);
                break;
            default:
                RaiseFieldUpdating<Field>(graph.Caches[typeof(InventoryItemExt)], inventoryItem, ref value);
                break;
            }
            return (int?) value;
        }

        public static void RaiseFieldUpdating<Field>(PXCache cache, object item, ref object value) where Field : IBqlField
        {
            try
            {
                cache.RaiseFieldUpdating<Field>(item, ref value);
            }
            catch (PXSetPropertyException ex)
            {
                var name = typeof(Field).Name;
                var itemName = PXUIFieldAttribute.GetItemName(cache);
                var str = PXUIFieldAttribute.GetDisplayName(cache, name);
                var message = ex.Message;

                if (str != null && name != str)
                {
                    int startIndex = message.IndexOf(name, StringComparison.OrdinalIgnoreCase);
                    if (startIndex >= 0)
                    {
                        message = message.Remove(startIndex, name.Length).Insert(startIndex, str);
                    }
                }
                else
                {
                    str = name;
                }

                str = string.Format("{0} {1}", itemName, str);

                throw new PXSetPropertyException(ErrorMessages.ValueDoesntExist, str, value);
            }
        }

        private static int? GetSetupAcctID<Field>(PXGraph graph, InventoryItem inventoryItem, INSite site, INPostClass postClass, 
            AMOrderType amOrderType) where Field : IBqlField
        {
            if (typeof (Field) == typeof (INPostClassExt.aMWIPAcctID) 
                || typeof (Field) == typeof (InventoryItemExt.aMWIPAcctID)
                || typeof (Field) == typeof (INSiteExt.aMWIPAcctID))
            {
                return GetAcctID<AMOrderType.wIPAcctID>(graph, MaskMfg, inventoryItem, site, postClass, amOrderType);
            }

            if (typeof(Field) == typeof(INPostClassExt.aMWIPVarianceAcctID)
                || typeof(Field) == typeof(InventoryItemExt.aMWIPVarianceAcctID)
                || typeof(Field) == typeof(INSiteExt.aMWIPVarianceAcctID))
            {
                return GetAcctID<AMOrderType.wIPVarianceAcctID>(graph, MaskMfg, inventoryItem, site, postClass, amOrderType);
            }

            return null;
        }

        /// <summary>
        /// Gets the formated sub account based on the reason code configuration using the account wildcards
        /// </summary>
        /// <param name="cache">cache related to AMMTran</param>
        /// <param name="row">row object related to cache</param>
        /// <returns></returns>
        public static string GetReasonCodeSubIDString(PXCache cache, AMMTran row)
        {
            if (string.IsNullOrWhiteSpace(row?.ReasonCodeID))
            {
                return null;
            }

            var reasonCode = (ReasonCode)cache.Graph.Caches<ReasonCode>().Locate(new ReasonCode {ReasonCodeID = row.ReasonCodeID}) ??
                (ReasonCode)PXSelect<ReasonCode, Where<ReasonCode.reasonCodeID, Equal<Required<ReasonCode.reasonCodeID>>>>.Select(
                    cache.Graph, row.ReasonCodeID);

            if (string.IsNullOrWhiteSpace(reasonCode?.ReasonCodeID))
            {
                return null;
            }

            var inSite = (INSite)cache.Graph.Caches<INSite>().Locate(new INSite { SiteID = row.SiteID }) ??
                             (INSite)PXSelect<INSite, Where<INSite.siteID, Equal<Required<INSite.siteID>>>>.Select(
                                 cache.Graph, row.SiteID);
            if (inSite?.SiteID == null)
            {
                return null;
            }

            var inventoryItem = (InventoryItem)cache.Graph.Caches<InventoryItem>().Locate(new InventoryItem { InventoryID = row.InventoryID }) ??
                                (InventoryItem)PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>.Select(
                                      cache.Graph, row.InventoryID);
            if (inventoryItem?.InventoryID == null)
            {
                return null;
            }

            var inPostClass = (INPostClass)cache.Graph.Caches<INPostClass>().Locate(new INPostClass { PostClassID = inventoryItem.PostClassID }) ??
                              (INPostClass)PXSelect<INPostClass, Where<INPostClass.postClassID, Equal<Required<INPostClass.postClassID>>>>.Select(
                                                    cache.Graph, inventoryItem.PostClassID);
            if (string.IsNullOrWhiteSpace(inPostClass?.PostClassID))
            {
                return null;
            }

            return GetReasonCodeSubIDString(cache, row, reasonCode, inventoryItem, inSite, inPostClass);
        }

        /// <summary>
        /// Gets the formated sub account based on the reason code configuration using the account wildcards
        /// </summary>
        /// <param name="cache">cache related to AMMTran</param>
        /// <param name="row">row object related to cache</param>
        /// <returns></returns>
        public static string GetReasonCodeSubIDString(PXCache cache, object row, ReasonCode reasoncode, InventoryItem item, INSite site, INPostClass postclass)
        {
            if (reasoncode.AccountID != null)
            {
                var reasoncode_SubID = (int?)cache.Graph.Caches[typeof(ReasonCode)].GetValue<ReasonCode.subID>(reasoncode);
                var item_SubID = (int?)cache.Graph.Caches[typeof(InventoryItem)].GetValue<InventoryItem.reasonCodeSubID>(item);
                var site_SubID = (int?)cache.Graph.Caches[typeof(INSite)].GetValue<INSite.reasonCodeSubID>(site);
                var class_SubID = (int?)cache.Graph.Caches[typeof(INPostClass)].GetValue<INPostClass.reasonCodeSubID>(postclass);

                return PX.Objects.IN.ReasonCodeSubAccountMaskAttribute.MakeSub<ReasonCode.subMask>(cache.Graph, reasoncode.SubMask,
                    new object[] { reasoncode_SubID, item_SubID, site_SubID, class_SubID },
                    new Type[] { typeof(ReasonCode.subID), typeof(InventoryItem.reasonCodeSubID), typeof(INSite.reasonCodeSubID), typeof(INPostClass.reasonCodeSubID) });
            }
            return null;
        }
    }
}

