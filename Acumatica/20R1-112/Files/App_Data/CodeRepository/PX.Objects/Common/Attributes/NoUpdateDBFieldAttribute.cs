using System;
using System.Collections.Generic;
using System.Linq;
using PX.Data;

namespace PX.Objects.Common.Attributes
{
	/// <summary>
	/// The attribute indicates that the changes of the field marked with it won't be persisted to the database.
	/// </summary>
	public class NoUpdateDBFieldAttribute : PXEventSubscriberAttribute, IPXCommandPreparingSubscriber
	{
		public virtual bool NoInsert
		{
			get;
			set;
		}

		public virtual void CommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
		{
			PXDBOperation command = e.Operation.Command();
			if (command == PXDBOperation.Update || command == PXDBOperation.Insert && this.NoInsert)
			{
				e.ExcludeFromInsertUpdate();
			}
		}
	}
}
