using PX.Data;
using PX.Objects.CR;
using PX.Objects.CS;
using CommonMessages = PX.Objects.CN.Common.Descriptor.SharedMessages;
using CrMessages = PX.Objects.CR.Messages;

namespace PX.Objects.CN.Common.Services
{
	/// <summary>
	/// Provides generic functionality for integration with common attibutes.
	/// Also is used to repeat create, update and delete operations on attribute groups related to TCache entity.
	/// When a user adds a new attribute to a class, it is created only for TPrimaryCache entity.
	/// The same attribute group is created for related TCache entity from code.
	/// </summary>
	public class CommonAttributesService
	{
		private readonly PXSelectBase<CSAttributeGroup> attributes;
		private readonly AttributeGroupDataProvider dataProvider;
		private readonly PXGraph graph;

		public CommonAttributesService(PXGraph graph, PXSelectBase<CSAttributeGroup> attributes)
		{
			this.attributes = attributes;
			this.graph = graph;
			dataProvider = new AttributeGroupDataProvider(graph);
		}

		/// <summary>
		/// Fills required fields for attribute group. Should be used on RowInserting event.
		/// </summary>
		public void InitializeInsertedAttribute<TPrimaryCache>(CSAttributeGroup attributeGroup, string classId)
			where TPrimaryCache : IBqlTable
		{
			attributeGroup.EntityClassID = classId;
			attributeGroup.EntityType = typeof(TPrimaryCache).FullName;
		}

		/// <summary>
		/// Deletes answers related to the primary attribute group. Should be used on RowDeleting event.
		/// </summary>
		public void DeleteAnswersIfRequired<TPrimaryCache>(Events.RowDeleting<CSAttributeGroup> args)
			where TPrimaryCache : IBqlTable
		{
			var attributeGroup = args.Row;
			if (attributeGroup.IsActive == true)
			{
				throw new PXSetPropertyException(CrMessages.AttributeCannotDeleteActive);
			}
			if (IsDeleteConfirmed())
			{
				CSAttributeGroupList<IBqlTable, TPrimaryCache>.DeleteAttributesForGroup(graph, attributeGroup);
			}
			else
			{
				args.Cancel = true;
			}
		}

		/// <summary>
		/// Configures the type of control for default value field on adding an attribute. Should be used on
		/// FieldSelecting event for <see cref="CSAttributeGroup.defaultValue"/>.
		/// </summary>
		public PXFieldState GetNewReturnState(object returnState, CSAttributeGroup attributeGroup)
		{
			return dataProvider.GetNewReturnState(returnState, attributeGroup);
		}

		public void DeleteRelatedEntityAnswer<TCache>(CSAttributeGroup attributeGroup)
			where TCache : IBqlTable
		{
			var entityAttribute = GetRelatedEntityAttribute<TCache>(
				attributeGroup.AttributeID, attributeGroup.EntityClassID);
			if (entityAttribute != null)
			{
				CSAttributeGroupList<IBqlTable, TCache>.DeleteAttributesForGroup(graph, entityAttribute);
			}
		}

		public void DeleteRelatedEntityAttribute<TCache>(CSAttributeGroup attribute)
			where TCache : IBqlTable
		{
			var entityAttribute = GetRelatedEntityAttribute<TCache>(attribute.AttributeID, attribute.EntityClassID);
			if (entityAttribute != null)
			{
				var attributeGroupCache = (PXCache)graph.Caches<CSAttributeGroup>();
				attributeGroupCache.Delete(entityAttribute);
			}
		}

		public void UpdateRelatedEntityAttribute<TCache>(CSAttributeGroup attribute)
			where TCache : IBqlTable
		{
			var entityAttribute = GetRelatedEntityAttribute<TCache>(attribute.AttributeID, attribute.EntityClassID);
			if (entityAttribute != null)
			{
				var attributeGroupCache = (PXCache)graph.Caches<CSAttributeGroup>();
				attributeGroupCache.RestoreCopy(entityAttribute, attribute);
				entityAttribute.EntityType = typeof(TCache).FullName;
				attributeGroupCache.Update(entityAttribute);
			}
		}

		public void CreateRelatedEntityAttribute<TCache>(CSAttributeGroup attribute)
			where TCache : IBqlTable
		{
			var attributeGroupCache = (PXCache)graph.Caches<CSAttributeGroup>();
			var entityAttribute = (CSAttributeGroup)attributeGroupCache.CreateCopy(attribute);
			entityAttribute.EntityType = typeof(TCache).FullName;
			attributeGroupCache.Insert(entityAttribute);
		}

		private bool IsDeleteConfirmed()
		{
			var dialogResult = attributes.Ask(
				CommonMessages.Warning, CrMessages.AttributeDeleteWarning, MessageButtons.OKCancel);
			return dialogResult == WebDialogResult.OK;
		}

		private CSAttributeGroup GetRelatedEntityAttribute<TCache>(string attributeId, string classId)
			where TCache : IBqlTable
		{
			var query = new PXSelect<CSAttributeGroup,
				Where<CSAttributeGroup.attributeID, Equal<Required<CSAttributeGroup.attributeID>>,
					And<CSAttributeGroup.entityClassID, Equal<Required<CSAttributeGroup.entityClassID>>,
					And<CSAttributeGroup.entityType, Equal<Required<CSAttributeGroup.entityType>>>>>>(graph);
			return query.SelectSingle(attributeId, classId, typeof(TCache).FullName);
		}
	}
}