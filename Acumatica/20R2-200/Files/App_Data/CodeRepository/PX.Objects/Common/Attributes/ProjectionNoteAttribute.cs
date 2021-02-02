using PX.Data;
using System;

namespace PX.Objects.Common.Attributes
{
	public class ProjectionNoteAttribute : PXNoteAttribute
	{
		protected Type EntityType
		{
			get;
			set;
		}

		public ProjectionNoteAttribute(Type entityType)
		{
			EntityType = entityType;
		}

		protected override bool IsVirtualTable(Type table) => false;

		protected override string GetEntityType(PXCache cache, Guid? noteId) => EntityType.FullName;
	}
}
