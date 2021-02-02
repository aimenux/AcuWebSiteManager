using System;
using System.Collections.Generic;
using System.Linq;
using PX.Common;
using PX.Data;
using PX.Objects.AR;

namespace PX.Objects.Common
{
	[PXInternalUseOnly]
	public class UpdateIfFieldsChangedScope : IDisposable
	{
		public class Changes
		{
			public HashSet<Type> SourceOfChange { get; set; }
			public int ReferenceCounter { get; set; }
		}

		protected bool _disposed;

		public UpdateIfFieldsChangedScope()
		{
			Changes currentContext = PXContext.GetSlot<Changes>();
			if (currentContext == null)
			{
				currentContext = new Changes();
				PXContext.SetSlot<Changes>(currentContext);
			}
			currentContext.ReferenceCounter++;
		}

		public virtual UpdateIfFieldsChangedScope AppendContext(params Type[] newChanges)
		{
			var data = PXContext.GetSlot<Changes>();
			if (data.SourceOfChange == null) data.SourceOfChange = new HashSet<Type>();

			foreach (var change in newChanges)
				if (!data.SourceOfChange.Contains(change)) data.SourceOfChange.Add(change);

			return this;
		}

		public virtual UpdateIfFieldsChangedScope AppendContext<Field>() where Field : IBqlField
			=> AppendContext(typeof(Field));

		public virtual UpdateIfFieldsChangedScope AppendContext<Field1, Field2>() where Field1 : IBqlField where Field2 : IBqlField
			=> AppendContext(typeof(Field1), typeof(Field2));

		public void Dispose()
		{
			if (_disposed) throw new PXObjectDisposedException();
			_disposed = true;

			Changes currentContext = PXContext.GetSlot<Changes>();
			currentContext.ReferenceCounter--;
			if (currentContext.ReferenceCounter == 0) PXContext.SetSlot<Changes>(null);
		}

		public virtual bool IsUpdateNeeded(params Type[] changes)
		{
			var data = PXContext.GetSlot<Changes>();
			if (data?.SourceOfChange == null) return true;

			foreach (var change in changes)
				if (data.SourceOfChange.Contains(change)) return true;

			return false;
		}

		public virtual bool IsUpdateNeeded<Field>() where Field : IBqlField
			=> IsUpdateNeeded(typeof(Field));

		public virtual bool IsUpdateNeeded<Field1, Field2>() where Field1 : IBqlField where Field2 : IBqlField
			=> IsUpdateNeeded(typeof(Field1), typeof(Field2));

		public virtual bool IsUpdatedOnly(params Type[] fields)
		{
			var data = PXContext.GetSlot<Changes>();
			if (data?.SourceOfChange == null) return true;

			foreach (var sourceOfChange in data.SourceOfChange)
				if (!fields.Contains(sourceOfChange)) return false;

			return true;
		}

		public virtual bool IsUpdatedOnly<Field>() where Field : IBqlField
			=> IsUpdatedOnly(typeof(Field));

		public virtual bool IsUpdatedOnly<Field1, Field2>() where Field1 : IBqlField where Field2 : IBqlField
			=> IsUpdatedOnly(typeof(Field1), typeof(Field2));
	}
}
