// This File is Distributed as Part of Acumatica Shared Source Code 
/* ---------------------------------------------------------------------*
*                               Acumatica Inc.                          *
*              Copyright (c) 1994-2011 All rights reserved.             *
*                                                                       *
*                                                                       *
* This file and its contents are protected by United States and         *
* International copyright laws.  Unauthorized reproduction and/or       *
* distribution of all or any portion of the code contained herein       *
* is strictly prohibited and will result in severe civil and criminal   *
* penalties.  Any violations of this copyright will be prosecuted       *
* to the fullest extent possible under law.                             *
*                                                                       *
* UNDER NO CIRCUMSTANCES MAY THE SOURCE CODE BE USED IN WHOLE OR IN     *
* PART, AS THE BASIS FOR CREATING A PRODUCT THAT PROVIDES THE SAME, OR  *
* SUBSTANTIALLY THE SAME, FUNCTIONALITY AS ANY ProjectX PRODUCT.        *
*                                                                       *
* THIS COPYRIGHT NOTICE MAY NOT BE REMOVED FROM THIS FILE.              *
* ---------------------------------------------------------------------*/

using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.ConstrainedExecution;

namespace PX.Data
{
    /// <exclude/>
	public sealed class KeysCollection : IEnumerable<string>
	{
		private List<string> _Keys = new List<string>();
		public void Add(string field)
		{
			_Keys.Add(field);
		}
		public int Count
		{
			get
			{
				return _Keys.Count;
			}
		}
		public string this[int index]
		{
			get
			{
				return _Keys[index];
			}
		}
		public bool Contains(string field)
		{
			return CompareIgnoreCase.IsInList(_Keys, field);
		}
		IEnumerator<string> IEnumerable<string>.GetEnumerator()
		{
			return _Keys.GetEnumerator();
		}
		IEnumerator IEnumerable.GetEnumerator()
		{
			return _Keys.GetEnumerator();
		}
		public void CopyTo(string[] target)
		{
			_Keys.CopyTo(target);
		}
		public int IndexOf(string field)
		{
			for (int i = 0; i < _Keys.Count; i++)
			{
				if (String.Equals(field, _Keys[i], StringComparison.OrdinalIgnoreCase))
				{
					return i;
				}
			}
			return -1;
		}
		public string[] ToArray()
		{
			return _Keys.ToArray();
		}
		public bool Exists(Predicate<string> match)
		{
			return _Keys.Exists(match);
		}
	}

    /// <summary>The enumeration that specifies a data record status. A data record status changes as a result of manipulations with the data record: insertion, update, or
    /// removal.</summary>
    /// <remarks>
    ///   <para>The table below shows data record status update depending on different <tt>PXCache</tt> methods.</para>
    ///   <table>
    ///     <tbody>
    ///       <tr>
    ///         <td>
    ///           <para align="center">
    ///             <strong>Original Status</strong>
    ///           </para>
    ///         </td>
    ///         <td>
    ///           <para align="center">
    ///             <strong>Previous status</strong>
    ///           </para>
    ///         </td>
    ///         <td>
    ///           <para align="center">
    ///             <strong>PXCache Method Invoked</strong>
    ///           </para>
    ///         </td>
    ///         <td>
    ///           <para align="center">
    ///             <strong>Status After</strong>
    ///           </para>
    ///         </td>
    ///       </tr>
    ///       <tr>
    ///         <td>N/A</td>
    ///         <td>N/A</td>
    ///         <td>Insert() / Insert(object)</td>
    ///         <td>Inserted</td>
    ///       </tr>
    ///       <tr>
    ///         <td>N/A</td>
    ///         <td>Inserted</td>
    ///         <td>Update(object)</td>
    ///         <td>Inserted</td>
    ///       </tr>
    ///       <tr>
    ///         <td>N/A</td>
    ///         <td>Inserted</td>
    ///         <td>Delete(object)</td>
    ///         <td>InsertedDeleted</td>
    ///       </tr>
    ///       <tr>
    ///         <td>Inserted</td>
    ///         <td>InsertedDeleted</td>
    ///         <td>Insert(object) / Update(object)</td>
    ///         <td>Inserted</td>
    ///       </tr>
    ///       <tr>
    ///         <td>N/A</td>
    ///         <td>Notchanged</td>
    ///         <td>Update(object)</td>
    ///         <td>Updated</td>
    ///       </tr>
    ///       <tr>
    ///         <td>N/A</td>
    ///         <td>Notchanged</td>
    ///         <td>Delete(object)</td>
    ///         <td>Deleted</td>
    ///       </tr>
    ///       <tr>
    ///         <td>Notchanged</td>
    ///         <td>Deleted</td>
    ///         <td>Insert(object) / Update(object)</td>
    ///         <td>Updated</td>
    ///       </tr>
    ///       <tr>
    ///         <td>N/A</td>
    ///         <td>Updated</td>
    ///         <td>Delete(object)</td>
    ///         <td>Deleted</td>
    ///       </tr>
    ///       <tr>
    ///         <td>Updated</td>
    ///         <td>Deleted</td>
    ///         <td>Insert(object) / Update(object)</td>
    ///         <td>Updated</td>
    ///       </tr>
    ///     </tbody>
    ///   </table>
    /// </remarks>
    public enum PXEntryStatus
    {
        /// <summary>The data record has not been modified since it was placed in
        /// the <tt>PXCache</tt> object or since the last time the <tt>Save</tt>
        /// action was invoked (triggering execution of BLC's
        /// <tt>Actions.PressSave()</tt>).</summary>
		Notchanged,
        /// <summary>The data record has been modified, and the <tt>Save</tt>
        /// action has not been invoked. After the changes are saved to the
        /// database, the data record status changes to
        /// <tt>Notchanged</tt>.</summary>
		Updated,
        /// <summary>The data record is new and has been added to the
        /// <tt>PXCache</tt> object, and the <tt>Save</tt> action has not been
        /// invoked. After the changes are saved to the database, the data record
        /// status changes to <tt>Notchanged</tt>.</summary>
		Inserted,
        /// <summary>The data record is not new and has been marked as
        /// <tt>Deleted</tt> within the <tt>PXCache</tt> object. After the changes
        /// are saved, the data record is deleted from the database and removed
        /// from the <tt>PXCache</tt> object.</summary>
		Deleted,
        /// <summary>The data record is new and has been added to the
        /// <tt>PXCache</tt> object and then marked as <tt>Deleted</tt> within the
        /// <tt>PXCache</tt> object. After the changes are saved, the data record
        /// is removed from the <tt>PXCache</tt> object.</summary>
		InsertedDeleted,
        /// <summary>An <tt>Unchanged</tt> data record can be marked as
        /// <tt>Held</tt> within the <tt>PXCache</tt> object to avoid being
        /// collected during memory cleanup. <tt>Updated</tt>, <tt>Inserted</tt>,
        /// <tt>Deleted</tt>, <tt>InsertedDeleted</tt>, or <tt>Held</tt> data
        /// records are never collected during memory cleanup. Any
        /// <tt>Notchanged</tt> data record can be removed from the
        /// <tt>PXCache</tt> object during memory cleanup.</summary>
        Held,
        /// <summary>This flag is passed to the <tt>PXCache&lt;&gt;.SetStatus</tt>
        /// method to indicate that the data record must be saved. The final status assigned
        /// to the data record (whether <tt>Inserted</tt> or <tt>Updated</tt>) depends
		/// on the initial status of the data record.</summary>
        Modified
    }

    /// <exclude/>
	public class PXCollection<T> : IEnumerable<T>
		where T : class
	{
        /// <exclude/>
		[DebuggerDisplay("[{status}] {value}")]
		private struct Entry
		{
			public int hashCode;
			public int next;
			public PXEntryStatus status;
			public T value;
		}

        /// <exclude/>
		public struct Enumerator : IEnumerator<T>, IDisposable, IEnumerator
		{
			private PXCollection<T> collection;
			private int version;
			private int index;
			private T current;

			internal Enumerator(PXCollection<T> collection)
			{
				this.collection = collection;
				this.version = collection.version;
				this.index = 0;
				this.current = (T)null;
			}

			public bool MoveNext()
			{
				if (this.version != this.collection.version)
				{
					throw new PXInvalidOperationException(ErrorMessages.CollectionChanged);
				}
				while (this.index < this.collection.count)
				{
					if (this.collection.entries[this.index].hashCode >= 0)
					{
						this.current = this.collection.entries[this.index].value;
						this.index++;
						return true;
					}
					this.index++;
				}
				this.index = this.collection.count + 1;
				this.current = (T)null;
				return false;
			}

			public T Current
			{
				get
				{
					return current;
				}
			}

			public void Dispose()
			{
			}

			object IEnumerator.Current
			{
				get
				{
					if ((this.index == 0) || (this.index == (this.collection.count + 1)))
					{
						throw new PXInvalidOperationException(ErrorMessages.CantStartEnumeration);
					}
					return this.current;
				}
			}

			void IEnumerator.Reset()
			{
				if (this.version != this.collection.version)
				{
					throw new PXInvalidOperationException(ErrorMessages.CollectionChanged);
				}
				this.index = 0;
				this.current = (T)null;
			}
		}

		private PXCache _Cache;
		public PXCollection(PXCache cache)
		{
			_Cache = cache;
		}

		private int[] buckets;
		private int count;
		private Entry[] entries;
		private int freeCount;
		private int freeList;
		private const string HashSizeName = "HashSize";
		private int version;
		private int _version;
		internal int Version
		{
			get
			{
				return this._version;
			}
			set
			{
				this._version = value;
			}
		}
		private const string VersionName = "Version";

		public void Add(T value)
		{
			bool wasUpdated;
			T result = this.PlaceNotChanged(value, out wasUpdated);

			if (!object.ReferenceEquals(value, result))
			{
				throw new PXArgumentException((string)null, ErrorMessages.DuplicateEntryAdded);
			}
		}

		public PXEntryStatus GetStatus(T value)
		{
			if (this.buckets == null)
			{
				this.Initialize(0);
			}
			int hash = _Cache.GetObjectHashCode(value) & 0x7fffffff;
			for (int i = this.buckets[hash % this.buckets.Length]; i >= 0; i = this.entries[i].next)
			{
				if ((this.entries[i].hashCode == hash) && _Cache.ObjectsEqual(this.entries[i].value, value))
				{
					return this.entries[i].status == PXEntryStatus.Modified ? PXEntryStatus.Updated : this.entries[i].status;
				}
			}
			return PXEntryStatus.Notchanged;
		}

		public T Locate(T value)
		{
			if (this.buckets == null)
			{
				this.Initialize(0);
			}
			int hash = _Cache.GetObjectHashCode(value) & 0x7fffffff;
			for (int i = this.buckets[hash % this.buckets.Length]; i >= 0; i = this.entries[i].next)
			{
				if ((this.entries[i].hashCode == hash) && _Cache.ObjectsEqual(this.entries[i].value, value))
				{
					return this.entries[i].value;
				}
			}
			return null;
		}

        public void Remove(T value)
        {
            if (this.buckets != null)
            {
                int hash = _Cache.GetObjectHashCode(value) & 0x7fffffff;
                int prev = -1;
                for (int i = this.buckets[hash % this.buckets.Length]; i >= 0; i = this.entries[i].next)
                {
                    if ((this.entries[i].hashCode == hash) && _Cache.ObjectsEqual(this.entries[i].value, value))
                    {
                        if (prev < 0)
                        {
                            this.buckets[hash % this.buckets.Length] = this.entries[i].next;
                        }
                        else
                        {
                            this.entries[prev].next = this.entries[i].next;
                        }
                        this.entries[i].hashCode = -1;
                        this.entries[i].next = this.freeList;
                        this.entries[i].value = null;
                        this.freeList = i;
                        this.freeCount++;
                        this.version++;
                        return;
                    }
                    prev = i;
                }
            }
        }

        public PXEntryStatus? SetStatus(T value, PXEntryStatus status)
		{
			int free;
			if (this.buckets == null)
			{
				this.Initialize(0);
			}
			int hash = _Cache.GetObjectHashCode(value) & 0x7fffffff;
			for (int i = this.buckets[hash % this.buckets.Length]; i >= 0; i = this.entries[i].next)
			{
				if ((this.entries[i].hashCode == hash) && _Cache.ObjectsEqual(this.entries[i].value, value))
				{
					if ((status == PXEntryStatus.Deleted || status == PXEntryStatus.Inserted || status == PXEntryStatus.Modified || status == PXEntryStatus.Updated)
						&& (this.entries[i].status == PXEntryStatus.Held || this.entries[i].status == PXEntryStatus.Notchanged))
					{
						this._version++;
					}
					PXEntryStatus ret = this.entries[i].status;
					if (status != PXEntryStatus.Modified || this.entries[i].status == PXEntryStatus.Notchanged || this.entries[i].status == PXEntryStatus.Held)
					{
						this.entries[i].status = status != PXEntryStatus.Modified ? status : PXEntryStatus.Updated;
					}
					return ret;
				}
			}
			if (this.freeCount > 0)
			{
				free = this.freeList;
				this.freeList = this.entries[free].next;
				this.freeCount--;
			}
			else
			{
				if (this.count == this.entries.Length)
				{
					this.Resize();
				}
				free = this.count;
				this.count++;
			}
			int k = hash % this.buckets.Length;
			this.entries[free].hashCode = hash;
			this.entries[free].next = this.buckets[k];
			this.entries[free].value = value;
			this.entries[free].status = status;
			this.buckets[k] = free;
			this.version++;
			if (status == PXEntryStatus.Deleted || status == PXEntryStatus.Inserted || status == PXEntryStatus.Modified || status == PXEntryStatus.Updated)
			{
				this._version++;
			}
			return null;
		}

        public T PlaceNotChanged(T value, out bool wasUpdated)
        {
            return PlaceNotChanged(value, false, out wasUpdated);
        }
        internal T PlaceNotChanged(T value, bool selecting, out bool wasUpdated)
		{
			wasUpdated = false;
			int free;
			if (this.buckets == null)
			{
				this.Initialize(0);
			}
			int hash = _Cache.GetObjectHashCode(value) & 0x7fffffff;
			for (int i = this.buckets[hash % this.buckets.Length]; i >= 0; i = this.entries[i].next)
			{
				if ((this.entries[i].hashCode == hash) && _Cache.ObjectsEqual(this.entries[i].value, value))
				{
					if (this.entries[i].status == PXEntryStatus.Deleted)
					{
						return (T)null;
					}
                    if (this.entries[i].status == PXEntryStatus.InsertedDeleted)
                    {
                        if (selecting)
                        {
                            this.entries[i].status = PXEntryStatus.Deleted;
                        }
                        return (T)null;
                    }
                    if (this.entries[i].status == PXEntryStatus.Updated || this.entries[i].status == PXEntryStatus.Inserted)
					{
						wasUpdated = true;
					}
					else if (this.entries[i].status == PXEntryStatus.Modified)
					{
						wasUpdated = true;
						this.entries[i].status = PXEntryStatus.Updated;
					}
					return this.entries[i].value;
				}
			}
			if (this.freeCount > 0)
			{
				free = this.freeList;
				this.freeList = this.entries[free].next;
				this.freeCount--;
			}
			else
			{
				if (this.count == this.entries.Length)
				{
					this.Resize();
				}
				free = this.count;
				this.count++;
			}
			int k = hash % this.buckets.Length;
			this.entries[free].hashCode = hash;
			this.entries[free].next = this.buckets[k];
			this.entries[free].value = value;
			this.entries[free].status = PXEntryStatus.Notchanged;
			this.buckets[k] = free;
			this.version++;

			return value;
		}

		public T PlaceUpdated(T value, bool bypassCheck)
		{
			int free;
			if (this.buckets == null)
			{
				this.Initialize(0);
			}
			int hash = _Cache.GetObjectHashCode(value) & 0x7fffffff;
			for (int i = this.buckets[hash % this.buckets.Length]; i >= 0; i = this.entries[i].next)
			{
				if ((this.entries[i].hashCode == hash) && _Cache.ObjectsEqual(this.entries[i].value, value))
				{
					if (!bypassCheck && (this.entries[i].status == PXEntryStatus.Deleted || this.entries[i].status == PXEntryStatus.InsertedDeleted))
					{
						return (T)null;
					}
					//this.entries[i].value = value;
					if (this.entries[i].status != PXEntryStatus.Inserted)
					{
						this.entries[i].status = PXEntryStatus.Updated;
					}
					this._version++;
					return this.entries[i].value;
				}
			}
			if (!bypassCheck)
			{
				return (T)null;
			}
			if (this.freeCount > 0)
			{
				free = this.freeList;
				this.freeList = this.entries[free].next;
				this.freeCount--;
			}
			else
			{
				if (this.count == this.entries.Length)
				{
					this.Resize();
				}
				free = this.count;
				this.count++;
			}
			int k = hash % this.buckets.Length;
			this.entries[free].hashCode = hash;
			this.entries[free].next = this.buckets[k];
			this.entries[free].value = value;
			this.entries[free].status = PXEntryStatus.Updated;
			this.buckets[k] = free;
			this.version++;
			this._version++;

			return value;
		}
        /// <summary>
        /// places row into collection, returns the same value on success.<br/>
        /// returns null if cache contains row with incompatible status
        /// </summary>
        /// <param name="value"></param>
        /// <param name="wasDeleted"></param>
        /// <returns></returns>
		public T PlaceInserted(T value, out bool wasDeleted)
		{
			wasDeleted = false;
			int free;
			if (this.buckets == null)
			{
				this.Initialize(0);
			}
			int hash = _Cache.GetObjectHashCode(value) & 0x7fffffff;
			for (int i = this.buckets[hash % this.buckets.Length]; i >= 0; i = this.entries[i].next)
			{
				if ((this.entries[i].hashCode == hash) && _Cache.ObjectsEqual(this.entries[i].value, value))
				{
					if (this.entries[i].status != PXEntryStatus.Deleted && this.entries[i].status != PXEntryStatus.InsertedDeleted && this.entries[i].status != PXEntryStatus.Modified)
					{
						return (T)null;
					}
					this.entries[i].value = value;
					this.entries[i].status = this.entries[i].status == PXEntryStatus.InsertedDeleted || this.entries[i].status == PXEntryStatus.Modified ? PXEntryStatus.Inserted : PXEntryStatus.Updated;
					if (this.entries[i].status == PXEntryStatus.Updated)
					{
						wasDeleted = true;
					}
					this.version++;
					this._version++;
					return value;
				}
			}
			if (this.freeCount > 0)
			{
				free = this.freeList;
				this.freeList = this.entries[free].next;
				this.freeCount--;
			}
			else
			{
				if (this.count == this.entries.Length)
				{
					this.Resize();
				}
				free = this.count;
				this.count++;
			}
			int k = hash % this.buckets.Length;
			this.entries[free].hashCode = hash;
			this.entries[free].next = this.buckets[k];
			this.entries[free].value = value;
			this.entries[free].status = PXEntryStatus.Inserted;
			this.buckets[k] = free;
			this.version++;
			this._version++;

			return value;
		}

		public T PlaceDeleted(T value, bool bypassCheck)
		{
			int free;
			if (this.buckets == null)
			{
				this.Initialize(0);
			}
			int hash = _Cache.GetObjectHashCode(value) & 0x7fffffff;
			for (int i = this.buckets[hash % this.buckets.Length]; i >= 0; i = this.entries[i].next)
			{
				if ((this.entries[i].hashCode == hash) && _Cache.ObjectsEqual(this.entries[i].value, value))
				{
					if (!bypassCheck && (this.entries[i].status == PXEntryStatus.Deleted || this.entries[i].status == PXEntryStatus.InsertedDeleted))
					{
						return (T)null;
					}
					this.entries[i].status = this.entries[i].status == PXEntryStatus.Inserted ? PXEntryStatus.InsertedDeleted : PXEntryStatus.Deleted;
					this._version++;
					return this.entries[i].value;
				}
			}
			if (!bypassCheck)
			{
				return (T)null;
			}
			if (this.freeCount > 0)
			{
				free = this.freeList;
				this.freeList = this.entries[free].next;
				this.freeCount--;
			}
			else
			{
				if (this.count == this.entries.Length)
				{
					this.Resize();
				}
				free = this.count;
				this.count++;
			}
			int k = hash % this.buckets.Length;
			this.entries[free].hashCode = hash;
			this.entries[free].next = this.buckets[k];
			this.entries[free].value = value;
			this.entries[free].status = PXEntryStatus.Deleted;
			this.buckets[k] = free;
			this.version++;
			this._version++;

			return value;
		}

        public bool IsDirty
        {
            get
            {
                return Dirty.Any();
            }
        }

		public IEnumerable<T> Inserted
		{
			get
			{
				for (int i = 0; i < this.count; i++)
				{
					if (this.entries[i].hashCode >= 0 && this.entries[i].status == PXEntryStatus.Inserted)
					{
						yield return this.entries[i].value;
					}
				}
			}
		}

		public IEnumerable<T> NotChanged
		{
			get
			{
				for (int i = 0; i < this.count; i++)
				{
					if (this.entries[i].hashCode >= 0 && (this.entries[i].status == PXEntryStatus.Notchanged || this.entries[i].status == PXEntryStatus.Held))
					{
						yield return this.entries[i].value;
					}
				}
			}
		}

		public IEnumerable<T> Cached
		{
			get
			{
				for (int i = 0; i < this.count; i++)
				{
					if (this.entries[i].hashCode >= 0)
					{
						yield return this.entries[i].value;
					}
				}
			}
		}

        public IEnumerable<T> Dirty
        {
            get
            {
                for (int i = 0; i < this.count; i++)
                {
					if (this.entries[i].hashCode >= 0 && (this.entries[i].status == PXEntryStatus.Inserted || this.entries[i].status == PXEntryStatus.Updated || this.entries[i].status == PXEntryStatus.Deleted || this.entries[i].status == PXEntryStatus.Modified))
                    {
                        yield return this.entries[i].value;
                    }
                }
            }
        }

		public IEnumerable<T> Updated
		{
			get
			{
				for (int i = 0; i < this.count; i++)
				{
					if (this.entries[i].hashCode >= 0 && (this.entries[i].status == PXEntryStatus.Updated || this.entries[i].status == PXEntryStatus.Modified))
					{
						yield return this.entries[i].value;
					}
				}
			}
		}

		public IEnumerable<T> Deleted
		{
			get
			{
				for (int i = 0; i < this.count; i++)
				{
					if (this.entries[i].hashCode >= 0 && this.entries[i].status == PXEntryStatus.Deleted)
					{
						yield return this.entries[i].value;
					}
				}
			}
		}

        public IEnumerable<T> Held
        {
            get
            {
                for (int i = 0; i < this.count; i++)
                {
                    if (this.entries[i].hashCode >= 0 && this.entries[i].status == PXEntryStatus.Held)
                    {
                        yield return this.entries[i].value;
                    }
                }
            }
        }
	    public int CachedCount
	    {
            get { return count; }
	    }
        public IEnumerator GetEnumerator()
		{
			return new PXCollection<T>.Enumerator(this);
		}

		IEnumerator<T> IEnumerable<T>.GetEnumerator()
		{
			return new PXCollection<T>.Enumerator(this);
		}

		private void Initialize(int capacity)
		{
			int length = HashHelpers.GetPrime(capacity);
			this.buckets = new int[length];
			for (int i = 0; i < this.buckets.Length; i++)
			{
				this.buckets[i] = -1;
			}
			this.entries = new PXCollection<T>.Entry[length];
			this.freeList = -1;
		}

		private void Resize()
		{
			int length = HashHelpers.GetPrime(this.count * 2);
			int[] intArray = new int[length];
			for (int i = 0; i < intArray.Length; i++)
			{
				intArray[i] = -1;
			}
			PXCollection<T>.Entry[] entryArray = new PXCollection<T>.Entry[length];
			Array.Copy(this.entries, 0, entryArray, 0, this.count);
			for (int i = 0; i < this.count; i++)
			{
				int j = entryArray[i].hashCode % length;
				entryArray[i].next = intArray[j];
				intArray[j] = i;
			}
			this.buckets = intArray;
			this.entries = entryArray;
		}

		public void Normalize(T item)
		{
			this.Normalize(item, PXEntryStatus.Inserted);
		}

		internal void Normalize(T item, PXEntryStatus status)
		{
			if (this.count > 0)
			{
				int itemHashCode = -1;
				for (int i = 0; i < this.buckets.Length; i++)
				{
					this.buckets[i] = -1;
				}
				for (int i = 0; i < this.count; i++)
				{
					if (this.entries[i].hashCode >= 0)
					{
						if (item != null && object.ReferenceEquals(item, this.entries[i].value))
						{
							this.entries[i].hashCode = itemHashCode = _Cache.GetObjectHashCode(this.entries[i].value) & 0x7fffffff;
						}
						else if (this.entries[i].status == status)
						{
							this.entries[i].hashCode = _Cache.GetObjectHashCode(this.entries[i].value) & 0x7fffffff;
						}
					}
					int j = this.entries[i].hashCode % this.buckets.Length;
					if (j == -1)
						continue;
					this.entries[i].next = this.buckets[j];
					this.buckets[j] = i;
				}
				if (item != null && itemHashCode >= 0)
				{
					for (int i = this.buckets[itemHashCode % this.buckets.Length]; i >= 0; i = this.entries[i].next)
					{
						if ((this.entries[i].hashCode == itemHashCode) && !object.ReferenceEquals(this.entries[i].value, item) && _Cache.ObjectsEqual(this.entries[i].value, item))
						{
							throw new PXBadDictinaryException();
						}
					}
				}
			}
		}
	}


    /// <exclude/>
	internal static class HashHelpers
	{
		// Methods
		static HashHelpers()
		{
			HashHelpers.primes = new int[] { 
            3, 7, 11, 0x11, 0x17, 0x1d, 0x25, 0x2f, 0x3b, 0x47, 0x59, 0x6b, 0x83, 0xa3, 0xc5, 0xef, 
            0x125, 0x161, 0x1af, 0x209, 0x277, 0x2f9, 0x397, 0x44f, 0x52f, 0x63d, 0x78b, 0x91d, 0xaf1, 0xd2b, 0xfd1, 0x12fd, 
            0x16cf, 0x1b65, 0x20e3, 0x2777, 0x2f6f, 0x38ff, 0x446f, 0x521f, 0x628d, 0x7655, 0x8e01, 0xaa6b, 0xcc89, 0xf583, 0x126a7, 0x1619b, 
            0x1a857, 0x1fd3b, 0x26315, 0x2dd67, 0x3701b, 0x42023, 0x4f361, 0x5f0ed, 0x72125, 0x88e31, 0xa443b, 0xc51eb, 0xec8c1, 0x11bdbf, 0x154a3f, 0x198c4f, 
            0x1ea867, 0x24ca19, 0x2c25c1, 0x34fa1b, 0x3f928f, 0x4c4987, 0x5b8b6f, 0x6dda89
       };
		}

		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		internal static int GetPrime(int min)
		{
			if (min < 0)
			{
				throw new PXArgumentException((string)null, ErrorMessages.CapacityOverflow);
			}
			for (int i = 0; i < HashHelpers.primes.Length; i++)
			{
				int j = HashHelpers.primes[i];
				if (j >= min)
				{
					return j;
				}
			}
			for (int i = min | 1; i < 0x7fffffff; i += 2)
			{
				if (HashHelpers.IsPrime(i))
				{
					return i;
				}
			}
			return min;
		}

		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		internal static bool IsPrime(int candidate)
		{
			if ((candidate & 1) == 0)
			{
				return (candidate == 2);
			}
			int i = (int)Math.Sqrt((double)candidate);
			for (int j = 3; j <= i; j += 2)
			{
				if ((candidate % j) == 0)
				{
					return false;
				}
			}
			return true;
		}

		// Fields
		internal static readonly int[] primes;
	}

    /// <exclude/>
	public class PXHashtable
	{
        /// <exclude/>
		private struct Entry
		{
			public int hashCode;
			public int next;
			public object[] keys;
			public object[] values;
		}
		private int[] buckets;
		private int count;
		private Entry[] entries;
		private int freeCount;
		private int freeList;
		private bool objectEquals(object[] itemKeys, object[] keys)
		{
			if (itemKeys.Length != keys.Length)
			{
				return false;
			}
			for (int i = 0; i < keys.Length; i++)
			{
				if (!object.Equals(itemKeys[i], keys[i]))
				{
					return false;
				}
			}
			return true;
		}
		private object[] getKeys(PXCache sender, object item)
		{
			object[] ret = new object[sender.Keys.Count];
			for (int i = 0; i < ret.Length; i++)
			{
				ret[i] = sender.GetValue(item, sender.Keys[i]);
			}
			return ret;
		}
		public void Put(PXCache sender, object item, params object[] values)
		{
			int free;
			if (this.buckets == null)
			{
				this.Initialize(0);
			}
			int hash = sender.GetObjectHashCode(item) & 0x7fffffff;
			object[] keys = getKeys(sender, item);
			for (int i = this.buckets[hash % this.buckets.Length]; i >= 0; i = this.entries[i].next)
			{
				if ((this.entries[i].hashCode == hash) && this.objectEquals(keys, this.entries[i].keys))
				{
					this.entries[i].values = values;
					return;
				}
			}
			if (this.freeCount > 0)
			{
				free = this.freeList;
				this.freeList = this.entries[free].next;
				this.freeCount--;
			}
			else
			{
				if (this.count == this.entries.Length)
				{
					this.Resize();
				}
				free = this.count;
				this.count++;
			}
			int k = hash % this.buckets.Length;
			this.entries[free].hashCode = hash;
			this.entries[free].next = this.buckets[k];
			this.entries[free].keys = keys;
			this.entries[free].values = values;
			this.buckets[k] = free;
		}
		public object[] Get(PXCache sender, object item)
		{
			if (this.buckets == null)
			{
				this.Initialize(0);
			}
			int hash = sender.GetObjectHashCode(item) & 0x7fffffff;
			object[] keys = getKeys(sender, item);
			for (int i = this.buckets[hash % this.buckets.Length]; i >= 0; i = this.entries[i].next)
			{
				if ((this.entries[i].hashCode == hash) && this.objectEquals(keys, this.entries[i].keys))
				{
					return this.entries[i].values;
				}
			}
			return null;
		}
		private void Initialize(int capacity)
		{
			int length = HashHelpers.GetPrime(capacity);
			this.buckets = new int[length];
			for (int i = 0; i < this.buckets.Length; i++)
			{
				this.buckets[i] = -1;
			}
			this.entries = new PXHashtable.Entry[length];
			this.freeList = -1;
		}
		private void Resize()
		{
			int length = HashHelpers.GetPrime(this.count * 2);
			int[] intArray = new int[length];
			for (int i = 0; i < intArray.Length; i++)
			{
				intArray[i] = -1;
			}
			PXHashtable.Entry[] entryArray = new PXHashtable.Entry[length];
			Array.Copy(this.entries, 0, entryArray, 0, this.count);
			for (int i = 0; i < this.count; i++)
			{
				int j = entryArray[i].hashCode % length;
				entryArray[i].next = intArray[j];
				intArray[j] = i;
			}
			this.buckets = intArray;
			this.entries = entryArray;
		}
	}
}
