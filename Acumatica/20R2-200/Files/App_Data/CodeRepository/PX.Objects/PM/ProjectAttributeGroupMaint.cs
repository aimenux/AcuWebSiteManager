using System;
using PX.Data;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.EP;

namespace PX.Objects.PM
{
    [Serializable]
	public class ProjectAttributeGroupMaint : PXGraph<ProjectAttributeGroupMaint>
	{
		#region Actions/Buttons
		public PXSave<GroupTypeFilter> Save;
		public PXCancel<GroupTypeFilter> Cancel;
		#endregion

		#region Views/Selects

		public PXFilter<GroupTypeFilter> Filter;
		public PXSelect<CSAttributeGroup,
			Where<CSAttributeGroup.entityClassID, Equal<Current<GroupTypeFilter.classID>>,
			And<CSAttributeGroup.entityType, Equal<Current<GroupTypeFilter.entityType>>>>> Mapping;

		#endregion

		public ProjectAttributeGroupMaint()
		{
			if (!Views.Caches.Contains(typeof(CSAnswers)))
				Views.Caches.Add(typeof(CSAnswers));
        }

        #region methods

        /// <summary>
        ///  This function just call to getEntityNameStatic because in another way, a infinty cicle was performed in the
        ///  called point of this method
        /// </summary>
        /// <param name="classid"></param>
        /// <returns></returns>
        public virtual string getEntityName(string classid)
        {
            return getEntityNameStatic(classid);
        }

        /// <summary>
        /// This function get the originals entity names of the DACs listed in Attributes screen
        /// Without Service Management module enabled
        /// </summary>
        /// <param name="classid"></param>
        /// <returns></returns>
        public static string getEntityNameStatic(string classid)
        {
            switch (classid)
            {
                case GroupTypes.AccountGroup:
                    return typeof(PMAccountGroup).FullName;
                //case GroupTypes.Transaction:
                //  return CSAnswerType.ProjectTran;
                case GroupTypes.Task:
                    return typeof(PMTask).FullName;
                case GroupTypes.Project:
                    return typeof(PMProject).FullName;
                case GroupTypes.Equipment:
                    return typeof(EPEquipment).FullName;

                default:
                    return null;
            }
        }
        
        #endregion

        #region Event Handlers

        protected virtual void GroupTypeFilter_EntityType_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            GroupTypeFilter GroupTypeFilterRow = (GroupTypeFilter)e.Row;
            if (GroupTypeFilterRow != null)
            {
                e.NewValue = getEntityName(GroupTypeFilterRow.ClassID);
            }
        }

		protected virtual void CSAttributeGroup_RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
		{
			var attributeGroup = (CSAttributeGroup)e.Row;
			if (attributeGroup == null)
				return;
			if (attributeGroup.IsActive == true)
				throw new PXSetPropertyException(CR.Messages.AttributeCannotDeleteActive);

			if (Mapping.Ask("Warning", CR.Messages.AttributeDeleteWarning, MessageButtons.OKCancel) != WebDialogResult.OK)
			{
				e.Cancel = true;
				return;
			}
			CSAttributeGroupList<CSAttributeGroup, CSAttributeGroup>.DeleteAttributesForGroup(this, attributeGroup);
		}

		protected virtual void CSAttributeGroup_RowInserting(PXCache sender, PXRowInsertingEventArgs e)
		{
			CSAttributeGroup row = e.Row as CSAttributeGroup;

			if (row != null && Filter.Current != null)
			{
				row.EntityClassID = Filter.Current.ClassID;
				row.EntityType = Filter.Current.EntityType;
			}
		}

        #endregion

        #region Local Types
		[PXHidden]
        [Serializable]
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        public partial class GroupTypeFilter : IBqlTable
        {
            #region ClassID
            public abstract class classID : PX.Data.BQL.BqlString.Field<classID> { }
            protected string _ClassID;
            [PXUIField(DisplayName = "Name", Visibility = PXUIVisibility.SelectorVisible)]
            [PXDBString(20, IsUnicode = true)]
            [GroupTypes.List()]
            [PXDefault(GroupTypes.Project)]
            public virtual String ClassID
            {
                get
                {
                    return this._ClassID;
                }
                set
                {
                    this._ClassID = value;
                }
            }
            #endregion
            #region EntityType
            public abstract class entityType : PX.Data.BQL.BqlString.Field<entityType> { }
            [PXString(200, IsUnicode = true)]
            [PXFormula(typeof(Default<classID>))]
			public virtual String EntityType { get; set; }
			#endregion
		}

		#endregion
	}
}
