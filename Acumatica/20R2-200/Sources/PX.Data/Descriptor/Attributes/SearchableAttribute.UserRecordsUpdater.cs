// This File is Distributed as Part of Acumatica Shared Source Code 
/* ---------------------------------------------------------------------*
*                               Acumatica Inc.                          *
*              Copyright (c) 1994-2016 All rights reserved.             *
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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using PX.Api.Soap.Screen;
using PX.BulkInsert.Provider;
using PX.Common;
using PX.Data.Update;
using PX.Data.UserRecords;
using PX.Data.UserRecords.FavoriteRecords;
using PX.Data.UserRecords.RecentlyVisitedRecords;
using PX.DbServices.Model.DataSet;
using PX.DbServices.Points.DbmsBase;
using PX.DbServices.Points.PXDataSet;

namespace PX.Data
{
	public partial class PXSearchableAttribute : PXEventSubscriberAttribute
	{
		/// <summary>
		/// A user records updater class. Updates row's user records cached content on row persisted event. 
		/// This functionality is integrated with <see cref="PXSearchableAttribute"/> because there will be too much overhead to introduce a new attribute. 
		/// The functionality of user records is related to the search since user records uses fields declared in <see cref="PXSearchableAttribute"/> constructor in DAC 
		/// to build information displayed to the user.
		/// </summary>
		[PXInternalUseOnly]
		protected class UserRecordsUpdater
		{
			/// <summary>
			/// Updates row's user records cached content on row persisted event. This functionality is integrated with <see cref="PXSearchableAttribute"/> because it will be too much overhead to avoid
			/// introduction of new attribute. The functionality is a bit related since user records uses <see cref="PXSearchableAttribute"/> to build information displayed to the user.
			/// </summary>
			/// <param name="e">The row persisted event information.</param>
			/// <param name="noteID">Note ID.</param>
			/// <param name="contentUpdater">The content updater.</param>
			public void UpdateUserRecordsCachedContentOnRowPersisted(PXRowPersistedEventArgs e, Guid noteID, IRecordCachedContentUpdater contentUpdater)
			{
				if (!(e.Row is IBqlTable entity) || e.Operation == PXDBOperation.Insert)
					return;

				const string userRecordsSlotName = "UserRecordsSlot";
				Type entityType = entity.GetType();
				var entityKey = (noteID, entityType);
				var cachedRecordContents = PXContext.GetSlot<Dictionary<(Guid NoteID, Type Type), string>>(userRecordsSlotName);

				if (cachedRecordContents == null)
				{
					cachedRecordContents = new Dictionary<(Guid NoteID, Type Type), string>();
					PXContext.SetSlot(userRecordsSlotName, cachedRecordContents);
				}

				if (!cachedRecordContents.TryGetValue(entityKey, out string content))
				{
					content = contentUpdater?.BuildCachedContent(entity);
					cachedRecordContents[entityKey] = content;
				}

				if (e.TranStatus == PXTranStatus.Completed)
				{
					// Acuminator disable once PX1043 SavingChangesInEventHandlers Diagnostic is not appliable in NetTools
					// Acuminator disable once PX1073 ExceptionsInRowPersisted [Diagnostic is not appliable in NetTools]
					PersistRecordChangesOnTransactionCompletion(e.Operation, noteID, entityType.FullName, content);
				}
			}

			private void PersistRecordChangesOnTransactionCompletion(PXDBOperation operation, Guid noteID, string entityType, string content)
			{
				bool isPortal = PXSiteMap.IsPortal;

				switch (operation.Command())
				{
					case PXDBOperation.Update:
						try
						{
							UpdateVisitedRecordsCachedContentForAllUsers(noteID, entityType, isPortal, content);
							UpdateFavoriteRecordsCachedContentForAllUsers(noteID, entityType, isPortal, content);
						}
						catch (PXDatabaseException dbException)
						{
							PXTrace.WriteError(dbException, "Failed to update user records for entity of type {EntityType} with NoteID {NoteID} and IsPortal equal {IsPortal}" +
											   "The error message is {ErrorMessage}", entityType, noteID, isPortal, dbException.Message);
						}

						return;

					case PXDBOperation.Delete:
						try
						{
							DeleteVisitedRecordsForAllUsers(noteID, entityType, isPortal);
							DeleteFavoriteRecordsForAllUsers(noteID, entityType, isPortal);
						}
						catch (PXDatabaseException dbException)
						{
							PXTrace.WriteError(dbException, "Failed to delete user records for entity of type {EntityType} with NoteID {NoteID} and IsPortal equal {IsPortal}" +
											   "The error message is {ErrorMessage}", entityType, noteID, isPortal, dbException.Message);
						}

						return;
				}
			}

			private void UpdateVisitedRecordsCachedContentForAllUsers(Guid noteID, string entityType, bool isPortal, string content) =>
				PXDatabase.Update<VisitedRecord>(new PXDataFieldRestrict(nameof(VisitedRecord.refNoteID), PXDbType.UniqueIdentifier, noteID),
												 new PXDataFieldRestrict(nameof(VisitedRecord.entityType), PXDbType.NVarChar, entityType),
												 new PXDataFieldRestrict(nameof(VisitedRecord.isPortal), PXDbType.Bit, isPortal ? 1 : 0),
												 new PXDataFieldAssign(nameof(VisitedRecord.recordContent), PXDbType.NText, content));

			private void UpdateFavoriteRecordsCachedContentForAllUsers(Guid noteID, string entityType, bool isPortal, string content) =>
				PXDatabase.Update<FavoriteRecord>(new PXDataFieldRestrict(nameof(FavoriteRecord.refNoteID), PXDbType.UniqueIdentifier, noteID),
								                  new PXDataFieldRestrict(nameof(FavoriteRecord.entityType), PXDbType.NVarChar, entityType),
								                  new PXDataFieldRestrict(nameof(FavoriteRecord.isPortal), PXDbType.Bit, isPortal ? 1 : 0),
								                  new PXDataFieldAssign(nameof(FavoriteRecord.recordContent), PXDbType.NText, content));

			private void DeleteVisitedRecordsForAllUsers(Guid noteID, string entityType, bool isPortal) =>
				PXDatabase.Delete<VisitedRecord>(
								  new PXDataFieldRestrict(nameof(VisitedRecord.refNoteID), PXDbType.UniqueIdentifier, noteID),
								  new PXDataFieldRestrict(nameof(VisitedRecord.entityType), PXDbType.NVarChar, entityType),
								  new PXDataFieldRestrict(nameof(VisitedRecord.isPortal), PXDbType.Bit, isPortal ? 1 : 0));

			private void DeleteFavoriteRecordsForAllUsers(Guid noteID, string entityType, bool isPortal) =>
				PXDatabase.Delete(typeof(FavoriteRecord),
								  new PXDataFieldRestrict(nameof(FavoriteRecord.refNoteID), PXDbType.UniqueIdentifier, noteID),
								  new PXDataFieldRestrict(nameof(FavoriteRecord.entityType), PXDbType.NVarChar, entityType),
								  new PXDataFieldRestrict(nameof(FavoriteRecord.isPortal), PXDbType.Bit, isPortal ? 1 : 0));
		}
	}
}
