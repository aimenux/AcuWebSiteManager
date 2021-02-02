using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PX.Data;
using PX.Objects.CN.Common.Extensions;
using PX.Objects.CN.Subcontracts.EP.Descriptor;
using PX.Objects.CN.Subcontracts.SC.Graphs;
using PX.Objects.CN.Subcontracts.SM.Extension;
using PX.Objects.CS;
using PX.Objects.EP;
using PX.SM;

namespace PX.Objects.CN.Subcontracts.EP.GraphExtensions
{
    public class EpApprovalMapMaintExt : PXGraphExtension<EPApprovalMapMaint>
    {
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.construction>();
        }

        protected IEnumerable entityItems(string parent)
        {
            if (Base.AssigmentMap.Current != null)
            {
                var entityType = GraphHelper.GetType(Base.AssigmentMap.Current.EntityType);
                Type primaryGraphType;
                if (Base.AssigmentMap.Current.GraphType == null)
                {
                    if (entityType == null && parent != null)
                    {
                        yield break;
                    }
                    primaryGraphType = EntityHelper.GetPrimaryGraphType(Base, entityType);
                }
                else
                {
                    primaryGraphType = GraphHelper.GetType(Base.AssigmentMap.Current.GraphType);
                }
                var cacheEntityItems = EMailSourceHelper.TemplateEntity(Base, parent, entityType?.FullName,
                    primaryGraphType?.FullName);
                foreach (CacheEntityItem cacheEntityItem in cacheEntityItems)
                {
                    if (primaryGraphType == typeof(SubcontractEntry))
                    {
                        cacheEntityItem.Name = cacheEntityItem.GetSubcontractViewName();
                    }
                    yield return cacheEntityItem;
                }
            }
        }

        protected virtual void EPAssignmentMap_GraphType_FieldSelecting(
            PXCache cache, PXFieldSelectingEventArgs args, PXFieldSelecting baseHandler)
        {
            baseHandler(cache, args);
            PXStringListAttribute.AppendList<EPAssignmentMap.graphType>(cache, args.Row,
                typeof(SubcontractEntry).FullName.CreateArray(),
                Constants.SubcontractTypeName.CreateArray());
        }

        protected virtual void EPRuleCondition_Entity_FieldSelecting(
            PXCache cache, PXFieldSelectingEventArgs args, PXFieldSelecting baseHandler)
        {
            if (Base.AssigmentMap.Current != null &&
                Base.AssigmentMap.Current?.GraphType == typeof(SubcontractEntry).FullName)
            {
                args.ReturnState = CreateFieldStateForEntity(args.ReturnValue,
                    Base.AssigmentMap.Current.EntityType, Base.AssigmentMap.Current.GraphType);
            }
            else
            {
                baseHandler(cache, args);
            }
        }

        private PXFieldState CreateFieldStateForEntity(object returnState, string entityTypeName, string graphTypeName)
        {
            var values = new List<string>();
            var labels = new List<string>();
            var entityType = GraphHelper.GetType(entityTypeName);
            if (entityType != null)
            {
                var graphType = EntityHelper.GetPrimaryGraphType(Base, entityType);
                if (!string.IsNullOrEmpty(graphTypeName))
                {
                    graphType = GraphHelper.GetType(graphTypeName);
                }
                if (graphType == null)
                {
                    var customAttributes =
                        (PXCacheNameAttribute[]) entityType.GetCustomAttributes(typeof(PXCacheNameAttribute), true);
                    if (entityType.IsSubclassOf(typeof(CSAnswers)))
                    {
                        values.Add(entityType.FullName);
                        var name = customAttributes.Length != 0
                            ? customAttributes[0].Name
                            : entityType.Name;
                        labels.Add(name);
                    }
                }
                else
                {
                    var cacheEntityItems = EMailSourceHelper.TemplateEntity(
                            Base, null, entityType.FullName, graphType.FullName)
                        .Cast<CacheEntityItem>()
                        .Where(cacheEntityItem => cacheEntityItem.SubKey != typeof(CSAnswers).FullName);
                    foreach (var cacheEntityItem in cacheEntityItems)
                    {
                        values.Add(cacheEntityItem.SubKey);
                        labels.Add(cacheEntityItem.GetSubcontractViewName());
                    }
                }
            }
            return PXStringState.CreateInstance(returnState, 60, default(bool), "Entity", false, 1, null,
                values.ToArray(), labels.ToArray(), true, null);
        }
    }
}