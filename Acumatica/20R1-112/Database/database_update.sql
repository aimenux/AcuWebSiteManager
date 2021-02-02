--- Convert xml widget settings
--[Method(Method = PX.BulkInsert.SpecialUpgrade.Dashboards.XmlMigrations.UpgradeWidgetSettings, IgnoreUPScripts = True)]
--[MinVersion(Hash = b4f3a539f46f228342acc35b4e7ae9b4d50bdba116a915d356bd8889b0a281fa)]
--[MinVersion(Hash = b4f3a539f46f228342acc35b4e7ae9b4d50bdba116a915d356bd8889b0a281fa)]
--[MinVersion(Hash = b4f3a539f46f228342acc35b4e7ae9b4d50bdba116a915d356bd8889b0a281fa)]
--[MinVersion(Hash = b4f3a539f46f228342acc35b4e7ae9b4d50bdba116a915d356bd8889b0a281fa)]
--[MinVersion(Hash = b4f3a539f46f228342acc35b4e7ae9b4d50bdba116a915d356bd8889b0a281fa)]
--[MinVersion(Hash = b4f3a539f46f228342acc35b4e7ae9b4d50bdba116a915d356bd8889b0a281fa)]
--[MinVersion(Hash = b4f3a539f46f228342acc35b4e7ae9b4d50bdba116a915d356bd8889b0a281fa)]
--[MinVersion(Hash = b4f3a539f46f228342acc35b4e7ae9b4d50bdba116a915d356bd8889b0a281fa)]
--[MinVersion(Hash = b4f3a539f46f228342acc35b4e7ae9b4d50bdba116a915d356bd8889b0a281fa)]
RAISERROR( N'MM> Dashboard settings migration (XML): this code should not be executed by T-SQL engine. The logics has been moved to a method in DLL as specified by a batch attribute above', 15, -1)
GO

-- Remove "dead" preferences records for non-existing PivotField records
--[IfExists(Column = PivotFieldPreferences.Expression)]
--[SmartExecute]
--[MinVersion(Hash = 4e489c287e0f63dc34394e68487112e95226eea4a2becb5db2b982193c54402a)]
--[MinVersion(Hash = 4e489c287e0f63dc34394e68487112e95226eea4a2becb5db2b982193c54402a)]
--[MinVersion(Hash = 4e489c287e0f63dc34394e68487112e95226eea4a2becb5db2b982193c54402a)]
--[MinVersion(Hash = 4e489c287e0f63dc34394e68487112e95226eea4a2becb5db2b982193c54402a)]
--[MinVersion(Hash = 4e489c287e0f63dc34394e68487112e95226eea4a2becb5db2b982193c54402a)]
delete pref
from PivotFieldPreferences pref 
left join PivotField pf
on
	pf.ScreenID = pref.ScreenID and 
	pf.PivotTableID = pref.PivotTableID and 
	pf.Type = pref.Type and 
	pf.Expression = pref.Expression
where pf.PivotTableID is null
GO

--[IfExists(Column = PivotFieldPreferences.Expression)]
--[SmartExecute]
--[MinVersion(Hash = b915e00cab10dac4f4bd50a731d2e2d231adf91e87b7d8cbc8d2be23a8e29c28)]
--[MinVersion(Hash = b915e00cab10dac4f4bd50a731d2e2d231adf91e87b7d8cbc8d2be23a8e29c28)]
--[MinVersion(Hash = b915e00cab10dac4f4bd50a731d2e2d231adf91e87b7d8cbc8d2be23a8e29c28)]
--[MinVersion(Hash = b915e00cab10dac4f4bd50a731d2e2d231adf91e87b7d8cbc8d2be23a8e29c28)]
--[MinVersion(Hash = b915e00cab10dac4f4bd50a731d2e2d231adf91e87b7d8cbc8d2be23a8e29c28)]
UPDATE pref
SET pref.PivotFieldID = pf.PivotFieldID
FROM PivotFieldPreferences pref INNER JOIN PivotField pf
ON 
	pf.ScreenID = pref.ScreenID and 
	pf.PivotTableID = pref.PivotTableID and 
	pf.Type = pref.Type and 
	pf.Expression = pref.Expression
GO

-- Remove PivotFieldPreferences if they weren't updated by previous script because they have negative company id
--[IfExists(Column = PivotFieldPreferences.Expression)]
--[MinVersion(Hash = 8f6eef29f8e57ebf9dde6f8823977cc2bcbf52dd4755305bdf959685a3d8e900)]
delete from PivotFieldPreferences
where PivotFieldPreferences.PivotFieldID is null 
or PivotFieldPreferences.PivotFieldID = 0
GO

--[IfExists(Column = Version.Type)]
--[MinVersion(Hash = 1ab9825e884265bd6bc245f689219cd4d0ef5f728a81e1d0d25146082cb4351e)]
--[MinVersion(Hash = 1ab9825e884265bd6bc245f689219cd4d0ef5f728a81e1d0d25146082cb4351e)]
--[MinVersion(Hash = 1ab9825e884265bd6bc245f689219cd4d0ef5f728a81e1d0d25146082cb4351e)]
--[MinVersion(Hash = 1ab9825e884265bd6bc245f689219cd4d0ef5f728a81e1d0d25146082cb4351e)]
--[MinVersion(Hash = 1ab9825e884265bd6bc245f689219cd4d0ef5f728a81e1d0d25146082cb4351e)]
--[MinVersion(Hash = 1ab9825e884265bd6bc245f689219cd4d0ef5f728a81e1d0d25146082cb4351e)]
UPDATE Version SET ComponentName = Version.Type, ComponentType='P'
GO

--[IfExists(Column = UPHistory.FromVersion)]
--[MinVersion(Hash = 8cc4e7d3fa7b072b0ff183e3134d79f40f67bba33074224809b63ed2521d96d5)]
--[MinVersion(Hash = 8cc4e7d3fa7b072b0ff183e3134d79f40f67bba33074224809b63ed2521d96d5)]
--[MinVersion(Hash = 8cc4e7d3fa7b072b0ff183e3134d79f40f67bba33074224809b63ed2521d96d5)]
--[MinVersion(Hash = 8cc4e7d3fa7b072b0ff183e3134d79f40f67bba33074224809b63ed2521d96d5)]
--[MinVersion(Hash = 8cc4e7d3fa7b072b0ff183e3134d79f40f67bba33074224809b63ed2521d96d5)]
--[MinVersion(Hash = 8cc4e7d3fa7b072b0ff183e3134d79f40f67bba33074224809b63ed2521d96d5)]
INSERT INTO UPHistoryComponents (UpdateID, ComponentName, ComponentType, FromVersion, ToVersion)
SELECT UpdateID, COALESCE((SELECT TOP 1 ComponentName FROM Version), 'Application'), 'P', FromVersion, ToVersion
FROM UPHistory
GO

--Remove standalone pages from localization resources
--[MinVersion(Hash = d8a8fd7813d27de21c5777dcc1990a613fa46847ab8d801502f1438c7b6e5a5c)]
--[MinVersion(Hash = d8a8fd7813d27de21c5777dcc1990a613fa46847ab8d801502f1438c7b6e5a5c)]
--[MinVersion(Hash = d8a8fd7813d27de21c5777dcc1990a613fa46847ab8d801502f1438c7b6e5a5c)]
--[MinVersion(Hash = d8a8fd7813d27de21c5777dcc1990a613fa46847ab8d801502f1438c7b6e5a5c)]
--[MinVersion(Hash = d8a8fd7813d27de21c5777dcc1990a613fa46847ab8d801502f1438c7b6e5a5c)]
--[MinVersion(Hash = d8a8fd7813d27de21c5777dcc1990a613fa46847ab8d801502f1438c7b6e5a5c)]
DELETE FROM LocalizationResourceByScreen
WHERE ScreenID IN ('dArticle', 'Feedback', 'icleEdit', 'iki/Show', 'mentEdit', 'mparison', 'PageEdit', 'ShowWiki', 'tionEdit', 'wTfsItem')
GO

--[MinVersion(Hash = 33670abf6663fcead6b999eb94ad18c913ba8f1c4453d44a272d58dbd980ea29)]
--[mysql: Skip]
--[MinVersion(Hash = 33670abf6663fcead6b999eb94ad18c913ba8f1c4453d44a272d58dbd980ea29)]
--[MinVersion(Hash = 33670abf6663fcead6b999eb94ad18c913ba8f1c4453d44a272d58dbd980ea29)]
--[MinVersion(Hash = 33670abf6663fcead6b999eb94ad18c913ba8f1c4453d44a272d58dbd980ea29)]
--[MinVersion(Hash = 33670abf6663fcead6b999eb94ad18c913ba8f1c4453d44a272d58dbd980ea29)]
--[MinVersion(Hash = 33670abf6663fcead6b999eb94ad18c913ba8f1c4453d44a272d58dbd980ea29)]
ALTER TABLE UploadFileRevision ALTER COLUMN Comment NVARCHAR(500) NULL
GO

--[MinVersion(Hash = 8d603b5addfe6d7345c36b399ece2488ab73f34045c171802ddc49c917d5ecc1)]
--[MinVersion(Hash = 8d603b5addfe6d7345c36b399ece2488ab73f34045c171802ddc49c917d5ecc1)]
--[MinVersion(Hash = 8d603b5addfe6d7345c36b399ece2488ab73f34045c171802ddc49c917d5ecc1)]
--[MinVersion(Hash = 8d603b5addfe6d7345c36b399ece2488ab73f34045c171802ddc49c917d5ecc1)]
--[MinVersion(Hash = 8d603b5addfe6d7345c36b399ece2488ab73f34045c171802ddc49c917d5ecc1)]
--[MinVersion(Hash = 8d603b5addfe6d7345c36b399ece2488ab73f34045c171802ddc49c917d5ecc1)]
--[MinVersion(Hash = 8d603b5addfe6d7345c36b399ece2488ab73f34045c171802ddc49c917d5ecc1)]
UPDATE RMRow
SET ColumnGroupID = UPPER(ColumnGroupID), UnitGroupID = UPPER(UnitGroupID)
WHERE ColumnGroupID IS NOT NULL OR UnitGroupID IS NOT NULL
GO

--[IfExists(Column = PivotField.DateRounding)]
--[MinVersion(Hash = bc63d0e80a5b126d04b4f7bc4397f4791cfd40c572492e3c5154b738006cadbf)]
--[MinVersion(Hash = bc63d0e80a5b126d04b4f7bc4397f4791cfd40c572492e3c5154b738006cadbf)]
--[MinVersion(Hash = bc63d0e80a5b126d04b4f7bc4397f4791cfd40c572492e3c5154b738006cadbf)]
--[MinVersion(Hash = bc63d0e80a5b126d04b4f7bc4397f4791cfd40c572492e3c5154b738006cadbf)]
UPDATE PivotField SET Transformation = CONCAT('DR', DateRounding) WHERE DateRounding is not null and DateRounding != 0
GO

--[IfExists(Column = PreferencesGeneral.TimeZone)]
--[MinVersion(Hash = aa91569453b2671feae6a747c38fd5e1fda48694fe49b3d8bc5b10f613331a98)]
--[MinVersion(Hash = aa91569453b2671feae6a747c38fd5e1fda48694fe49b3d8bc5b10f613331a98)]
--[MinVersion(Hash = aa91569453b2671feae6a747c38fd5e1fda48694fe49b3d8bc5b10f613331a98)]
--[MinVersion(Hash = aa91569453b2671feae6a747c38fd5e1fda48694fe49b3d8bc5b10f613331a98)]
UPDATE PreferencesGeneral
SET TimeZone = CASE TimeZone	
	WHEN 'GMTM1100A' THEN 'GMTP1300M'
	WHEN 'GMTM0430A' THEN 'GMTM0400Y'
	WHEN 'GMTP0300Y' THEN 'GMTP0400Y'
	WHEN 'GMTP0600A' THEN 'GMTP0700N'
	ELSE TimeZone
	END
WHERE TimeZone IN ('GMTM1100A', 'GMTM0430A', 'GMTP0300Y', 'GMTP0600A')
GO

--[IfExists(Column = UserPreferences.TimeZone)]
--[MinVersion(Hash = a1e3d8a15d251fba9ab7b76bb17141eb67c9fe350e7d83d078855d6a358b597b)]
--[MinVersion(Hash = a1e3d8a15d251fba9ab7b76bb17141eb67c9fe350e7d83d078855d6a358b597b)]
--[MinVersion(Hash = a1e3d8a15d251fba9ab7b76bb17141eb67c9fe350e7d83d078855d6a358b597b)]
--[MinVersion(Hash = a1e3d8a15d251fba9ab7b76bb17141eb67c9fe350e7d83d078855d6a358b597b)]
UPDATE UserPreferences
SET TimeZone = CASE TimeZone	
	WHEN 'GMTM1100A' THEN 'GMTP1300M'
	WHEN 'GMTM0430A' THEN 'GMTM0400Y'
	WHEN 'GMTP0300Y' THEN 'GMTP0400Y'
	WHEN 'GMTP0600A' THEN 'GMTP0700N'
	ELSE TimeZone
	END
WHERE TimeZone IN ('GMTM1100A', 'GMTM0430A', 'GMTP0300Y', 'GMTP0600A')
GO

--[IfExists(Column = AUSchedule.TimeZoneID)]
--[MinVersion(Hash = 9bdee68eb78745205cee74a69e726e4e2ba44e4060225b92eddccc4997c4b0f9)]
--[MinVersion(Hash = 9bdee68eb78745205cee74a69e726e4e2ba44e4060225b92eddccc4997c4b0f9)]
--[MinVersion(Hash = 9bdee68eb78745205cee74a69e726e4e2ba44e4060225b92eddccc4997c4b0f9)]
--[MinVersion(Hash = 9bdee68eb78745205cee74a69e726e4e2ba44e4060225b92eddccc4997c4b0f9)]
UPDATE AUSchedule
SET TimeZoneID = CASE TimeZoneID	
	WHEN 'GMTM1100A' THEN 'GMTP1300M'
	WHEN 'GMTM0430A' THEN 'GMTM0400Y'
	WHEN 'GMTP0300Y' THEN 'GMTP0400Y'
	WHEN 'GMTP0600A' THEN 'GMTP0700N'
	ELSE TimeZoneID
	END
WHERE TimeZoneID IN ('GMTM1100A', 'GMTM0430A', 'GMTP0300Y', 'GMTP0600A')
GO

--[IfExists(Column = CSCalendar.TimeZone)]
--[MinVersion(Hash = b27d95e4f3b5ecceb6c93c4c01bb4882e8973e7cc773d38149db95ab3b8764b2)]
--[MinVersion(Hash = b27d95e4f3b5ecceb6c93c4c01bb4882e8973e7cc773d38149db95ab3b8764b2)]
--[MinVersion(Hash = b27d95e4f3b5ecceb6c93c4c01bb4882e8973e7cc773d38149db95ab3b8764b2)]
--[MinVersion(Hash = b27d95e4f3b5ecceb6c93c4c01bb4882e8973e7cc773d38149db95ab3b8764b2)]
UPDATE CSCalendar
SET TimeZone = CASE TimeZone	
	WHEN 'GMTM1100A' THEN 'GMTP1300M'
	WHEN 'GMTM0430A' THEN 'GMTM0400Y'
	WHEN 'GMTP0300Y' THEN 'GMTP0400Y'
	WHEN 'GMTP0600A' THEN 'GMTP0700N'
	ELSE TimeZone
	END
WHERE TimeZone IN ('GMTM1100A', 'GMTM0430A', 'GMTP0300Y', 'GMTP0600A')
GO

--[IfExists(Column = MUIFavoriteScreen.WorkspaceID)]
--[MinVersion(Hash = 0b11f65555a77410e68ea820add74fff17d08f3ab260acd010d9c3d6a6f46801)]
--[MinVersion(Hash = 0b11f65555a77410e68ea820add74fff17d08f3ab260acd010d9c3d6a6f46801)]
--[MinVersion(Hash = 0b11f65555a77410e68ea820add74fff17d08f3ab260acd010d9c3d6a6f46801)]
--[MinVersion(Hash = 0b11f65555a77410e68ea820add74fff17d08f3ab260acd010d9c3d6a6f46801)]
INSERT INTO MUIPinnedScreen (CompanyID, Username, NodeID, WorkspaceID, IsPinned, CompanyMask) 
SELECT CompanyID, Username, NodeID, WorkspaceID, 1, CompanyMask FROM MUIFavoriteScreen WHERE SimplifiedMode = 0
GO

--[IfExists(Column = MUIFavoriteScreen.WorkspaceID)]
--[mssql: Native]
--[mysql: Skip]
--[MinVersion(Hash = 15f7860fa4ed5b06d5f60aed1dd6b069e847caf4bbe34637a8e608fa94335485)]
--[MinVersion(Hash = 15f7860fa4ed5b06d5f60aed1dd6b069e847caf4bbe34637a8e608fa94335485)]
--[MinVersion(Hash = 15f7860fa4ed5b06d5f60aed1dd6b069e847caf4bbe34637a8e608fa94335485)]
--[MinVersion(Hash = 15f7860fa4ed5b06d5f60aed1dd6b069e847caf4bbe34637a8e608fa94335485)]
ALTER TABLE MUIFavoriteScreen ADD [AutoID] INT IDENTITY(1,1)
GO

--[IfExists(Column = MUIFavoriteScreen.WorkspaceID)]
--[mssql: Skip]
--[mysql: Native]
--[MinVersion(Hash = db9f44e37bd3c6c66a9d6989c8b24e45ed873039a7a584957f1844f867b562ac)]
--[MinVersion(Hash = db9f44e37bd3c6c66a9d6989c8b24e45ed873039a7a584957f1844f867b562ac)]
--[MinVersion(Hash = db9f44e37bd3c6c66a9d6989c8b24e45ed873039a7a584957f1844f867b562ac)]
--[MinVersion(Hash = db9f44e37bd3c6c66a9d6989c8b24e45ed873039a7a584957f1844f867b562ac)]
ALTER TABLE MUIFavoriteScreen ADD AutoID int NOT NULL AUTO_INCREMENT;
GO

-- Remove duplicates
--[IfExists(Column = MUIFavoriteScreen.SimplifiedMode)]
--[MinVersion(Hash = c703a0f4366a62d564497ddc288f22cb2e529cf735fdb86977e608a1bb150d76)]
--[MinVersion(Hash = c703a0f4366a62d564497ddc288f22cb2e529cf735fdb86977e608a1bb150d76)]
--[MinVersion(Hash = c703a0f4366a62d564497ddc288f22cb2e529cf735fdb86977e608a1bb150d76)]
--[MinVersion(Hash = c703a0f4366a62d564497ddc288f22cb2e529cf735fdb86977e608a1bb150d76)]
DELETE FROM MUIFavoriteScreen WHERE SimplifiedMode = 1 AND AutoID NOT IN (SELECT MIN(AutoID) FROM MUIFavoriteScreen GROUP BY CompanyID, NodeID)
GO

--[IfExists(Column = MUIFavoriteScreen.SimplifiedMode)]
--[MinVersion(Hash = 352a27fb35d9992b238a4e08ddeafb0faa3370083af0287b3bcd318cbbe31aca)]
--[MinVersion(Hash = 352a27fb35d9992b238a4e08ddeafb0faa3370083af0287b3bcd318cbbe31aca)]
--[MinVersion(Hash = 352a27fb35d9992b238a4e08ddeafb0faa3370083af0287b3bcd318cbbe31aca)]
--[MinVersion(Hash = 352a27fb35d9992b238a4e08ddeafb0faa3370083af0287b3bcd318cbbe31aca)]
DELETE FROM MUIFavoriteScreen WHERE SimplifiedMode = 0
GO

--[SmartExecute]
--[IfExists(Table = Favorite)]
--[MinVersion(Hash = 0946eeedf73281084c882d65336589bf8cb96e19f50729b97ba6a72fbfdd5da8)]
INSERT INTO MUIFavoriteScreen (CompanyID, Username, NodeID)
SELECT f.CompanyID, u.Username, f.SiteMapID FROM Favorite f
INNER JOIN Users u ON f.UserID = u.PKID
LEFT JOIN MUIFavoriteScreen muif ON f.SiteMapID = muif.NodeID
WHERE f.SiteMapID IS NOT NULL AND muif.NodeID IS NULL
GROUP BY f.CompanyID, u.Username, f.SiteMapID
GO

--[IfExists(Column = MUIFavoriteWorkspace.IsFavorite)]
--[MinVersion(Hash = 12d8ee5a3413de4c54e022afea0ceea9b87c75de70c8c98531110f1fe9672ab5)]
--[MinVersion(Hash = 12d8ee5a3413de4c54e022afea0ceea9b87c75de70c8c98531110f1fe9672ab5)]
--[MinVersion(Hash = 12d8ee5a3413de4c54e022afea0ceea9b87c75de70c8c98531110f1fe9672ab5)]
--[MinVersion(Hash = 12d8ee5a3413de4c54e022afea0ceea9b87c75de70c8c98531110f1fe9672ab5)]
UPDATE MUIFavoriteWorkspace SET IsFavorite = 1 WHERE IsFavorite = 0 AND Username = ''
GO

--Convert ScreenID in critical tables to upper case (it will be to expensive to update all rows in all tables)
--[MinVersion(Hash = bbc1bd6bd7fe57cb021dc5c4a1ead13ac15cf27b34c0546f8312fa71f5b341cc)]
--[MinVersion(Hash = bbc1bd6bd7fe57cb021dc5c4a1ead13ac15cf27b34c0546f8312fa71f5b341cc)]
--[MinVersion(Hash = bbc1bd6bd7fe57cb021dc5c4a1ead13ac15cf27b34c0546f8312fa71f5b341cc)]
--[MinVersion(Hash = bbc1bd6bd7fe57cb021dc5c4a1ead13ac15cf27b34c0546f8312fa71f5b341cc)]
--[MinVersion(Hash = bbc1bd6bd7fe57cb021dc5c4a1ead13ac15cf27b34c0546f8312fa71f5b341cc)]
--[MinVersion(Hash = bbc1bd6bd7fe57cb021dc5c4a1ead13ac15cf27b34c0546f8312fa71f5b341cc)]
UPDATE SiteMap SET ScreenID = UPPER(ScreenID)
UPDATE PortalMap SET ScreenID = UPPER(ScreenID)
UPDATE RolesInGraph SET ScreenID = UPPER(ScreenID)
UPDATE RolesInCache SET ScreenID = UPPER(ScreenID)
UPDATE RolesInMember SET ScreenID = UPPER(ScreenID)
UPDATE FilterHeader SET ScreenID = UPPER(ScreenID)
UPDATE ListEntryPoint SET EntryScreenID = UPPER(EntryScreenID), ListScreenID = UPPER(ListScreenID)
UPDATE GIDesign SET PrimaryScreenIDNew = UPPER(PrimaryScreenIDNew)
GO

--[MinVersion(Hash = ce3e5b16d2dc2b71718e9dee9d2a06a2d6fb6c838ff9a1269e0800685dab9c63)]
--[MinVersion(Hash = ce3e5b16d2dc2b71718e9dee9d2a06a2d6fb6c838ff9a1269e0800685dab9c63)]
--[MinVersion(Hash = ce3e5b16d2dc2b71718e9dee9d2a06a2d6fb6c838ff9a1269e0800685dab9c63)]
--[MinVersion(Hash = ce3e5b16d2dc2b71718e9dee9d2a06a2d6fb6c838ff9a1269e0800685dab9c63)]
--[MinVersion(Hash = ce3e5b16d2dc2b71718e9dee9d2a06a2d6fb6c838ff9a1269e0800685dab9c63)]
--[MinVersion(Hash = ce3e5b16d2dc2b71718e9dee9d2a06a2d6fb6c838ff9a1269e0800685dab9c63)]
UPDATE GIResult SET RowID = NEWID() WHERE NoteID IS NULL
UPDATE GIResult SET RowID = NoteID WHERE RowID IS NULL
GO

-- LEP: Replace list screen records with entry screen records
--[SmartExecute]
--[MinVersion(Hash = b70adcb14aeb0aba3c13b2dc20da19d3001ac493c3af32b45b9b0c5fd4db5208)]
--[MinVersion(Hash = b70adcb14aeb0aba3c13b2dc20da19d3001ac493c3af32b45b9b0c5fd4db5208)]
--[MinVersion(Hash = b70adcb14aeb0aba3c13b2dc20da19d3001ac493c3af32b45b9b0c5fd4db5208)]
--[MinVersion(Hash = b70adcb14aeb0aba3c13b2dc20da19d3001ac493c3af32b45b9b0c5fd4db5208)]
insert into MUIScreen (CompanyID, NodeID, WorkspaceID, [Order], SubcategoryID, CompanyMask)
select mui.CompanyID, sm2.NodeID, mui.WorkspaceID, mui.[Order], mui.SubcategoryID, mui.CompanyMask from ListEntryPoint lep
inner join SiteMap sm on lep.ListScreenID = sm.ScreenID
inner join SiteMap sm2 on lep.EntryScreenID = sm2.ScreenID
inner join MUIScreen mui on sm.NodeID = mui.NodeID
where lep.IsActive = 1
group by mui.CompanyID, sm2.NodeID, mui.WorkspaceID, mui.SubcategoryID, mui.[Order], mui.CompanyMask

delete from MUIScreen where NodeID in
(select sm.NodeID from ListEntryPoint lep
inner join SiteMap sm on lep.ListScreenID = sm.ScreenID
inner join SiteMap sm2 on lep.EntryScreenID = sm2.ScreenID
where lep.IsActive = 1)
GO

--[SmartExecute]
--[MinVersion(Hash = 0b00c7c51346405b25fff5894dec4ae647c894802b653cbab9f7b0983989b107)]
--[MinVersion(Hash = 0b00c7c51346405b25fff5894dec4ae647c894802b653cbab9f7b0983989b107)]
--[MinVersion(Hash = 0b00c7c51346405b25fff5894dec4ae647c894802b653cbab9f7b0983989b107)]
--[MinVersion(Hash = 0b00c7c51346405b25fff5894dec4ae647c894802b653cbab9f7b0983989b107)]
insert into MUIPinnedScreen(CompanyID, NodeID, WorkspaceID, Username, IsPinned, CompanyMask)
select mui.CompanyID, sm2.NodeID, mui.WorkspaceID, mui.Username, mui.IsPinned, mui.CompanyMask from ListEntryPoint lep
inner join SiteMap sm on lep.ListScreenID = sm.ScreenID
inner join SiteMap sm2 on lep.EntryScreenID = sm2.ScreenID
inner join MUIPinnedScreen mui on sm.NodeID = mui.NodeID
left join MUIPinnedScreen muiExisting on muiExisting.NodeID = sm2.NodeID and muiExisting.WorkspaceID = mui.WorkspaceID and muiExisting.Username = mui.Username
where lep.IsActive = 1 and muiExisting.NodeID is null
group by mui.CompanyID, sm2.NodeID, mui.WorkspaceID, mui.Username, mui.IsPinned, mui.CompanyMask

delete mui from MUIPinnedScreen mui
inner join SiteMap sm on sm.NodeID = mui.NodeID
inner join ListEntryPoint lep on lep.ListScreenID = sm.ScreenID
inner join SiteMap sm2 on lep.EntryScreenID = sm2.ScreenID
where lep.IsActive = 1
GO

--[SmartExecute]
--[MinVersion(Hash = d47c25f9f9ad0a1448229fe6736ab2c01fdfadf5424de621fbeace575f95f317)]
--[MinVersion(Hash = d47c25f9f9ad0a1448229fe6736ab2c01fdfadf5424de621fbeace575f95f317)]
--[MinVersion(Hash = d47c25f9f9ad0a1448229fe6736ab2c01fdfadf5424de621fbeace575f95f317)]
--[MinVersion(Hash = d47c25f9f9ad0a1448229fe6736ab2c01fdfadf5424de621fbeace575f95f317)]
insert into MUIFavoriteScreen(CompanyID, NodeID, Username, CompanyMask)
select mui.CompanyID, sm2.NodeID, mui.Username, mui.CompanyMask from ListEntryPoint lep
inner join SiteMap sm on lep.ListScreenID = sm.ScreenID
inner join SiteMap sm2 on lep.EntryScreenID = sm2.ScreenID
inner join MUIPinnedScreen mui on sm.NodeID = mui.NodeID
left join MUIFavoriteScreen muiExisting on muiExisting.NodeID = sm2.NodeID and muiExisting.Username = mui.Username
where lep.IsActive = 1 and muiExisting.NodeID is null
group by mui.CompanyID, sm2.NodeID, mui.Username, mui.CompanyMask

delete mui from MUIFavoriteScreen mui
inner join SiteMap sm on sm.NodeID = mui.NodeID
inner join ListEntryPoint lep on lep.ListScreenID = sm.ScreenID
inner join SiteMap sm2 on lep.EntryScreenID = sm2.ScreenID
where lep.IsActive = 1
GO

--[mssql: Native]
--[mysql: Skip]
--Create tables for Localization update script MSSql
--[OldHash(Hash = 4f40453ee486f97844d4a7f8f2e0397a239e7d44025e78756ae4a2ab1bb370a4)]
--[MinVersion(Hash = 4f40453ee486f97844d4a7f8f2e0397a239e7d44025e78756ae4a2ab1bb370a4)]
--[MinVersion(Hash = 027a00168f50e17208881a79103ae5a91e0a3a5a87a4fd80816f361be0f6a361)]
--[MinVersion(Hash = 027a00168f50e17208881a79103ae5a91e0a3a5a87a4fd80816f361be0f6a361)]
--[MinVersion(Hash = 027a00168f50e17208881a79103ae5a91e0a3a5a87a4fd80816f361be0f6a361)]
--[MinVersion(Hash = 027a00168f50e17208881a79103ae5a91e0a3a5a87a4fd80816f361be0f6a361)]
CREATE TABLE [dbo].[LocalizationValue_tmp](
	[Id_fake] [int] identity NOT NULL,
	[CompanyID] [int] NULL,
	[Id] [char](32) NULL,
	[Id_new] [char](32) NULL,
	[NeutralValue] [nvarchar](max) NULL,
	[IsNotLocalized] [bit] NULL,
	[IsSite] [bit] NULL,
	[IsPortal] [bit] NULL,
	[IsObsolete] [bit] NULL,
	[IsObsoletePortal] [bit] NULL,
	[TranslationCount] [int] NULL,
	[CompanyMask] [varbinary](32) NULL,
	[CreatedByID] [uniqueidentifier] NULL,
	[CreatedByScreenID] [char](8) NULL,
	[CreatedDateTime] [datetime] NULL,
	[LastModifiedByID] [uniqueidentifier] NULL,
	[LastModifiedByScreenID] [char](8) NULL,
	[LastModifiedDateTime] [datetime] NULL,
	[tstamp] [timestamp] NOT NULL,
	CONSTRAINT [LocalizationValue_tmp_PK] PRIMARY KEY CLUSTERED ([Id_fake])
)
CREATE NONCLUSTERED INDEX [IX_LocalizationValue_tmp] ON [dbo].[LocalizationValue_tmp] ([Id], [CompanyID], [Id_new])

CREATE TABLE [dbo].[LocalizationResource_tmp]
(
	[Id_fake] [int] identity NOT NULL,
	[CompanyID] [int] NULL,
	[IdValue] [char](32) NULL,
	[Id] [char](32) NULL,
	[ResKey] [nvarchar](max) NULL,
	[ResType] [int] NULL,
	[IsSite] [bit] NULL,
	[IsPortal] [bit] NULL,
	[IsNotLocalized] [bit] NULL,
	[CompanyMask] [varbinary](32) NULL,
	[CreatedByID] [uniqueidentifier] NULL,
	[CreatedByScreenID] [char](8) NULL,
	[CreatedDateTime] [datetime] NULL,
	[LastModifiedByID] [uniqueidentifier] NULL,
	[LastModifiedByScreenID] [char](8) NULL,
	[LastModifiedDateTime] [datetime] NULL,
	[tstamp] [timestamp] NULL,
	CONSTRAINT [LocalizationResource_tmp_PK] PRIMARY KEY CLUSTERED ([Id_fake])
)
CREATE NONCLUSTERED INDEX [IX_LocalizationResource_tmp] ON [dbo].[LocalizationResource_tmp] ([IdValue], [CompanyID], [Id])

CREATE TABLE [dbo].[LocalizationResourceByScreen_tmp]
(
	[Id_fake] [int] identity NOT NULL,
	[CompanyID] [int] NULL,
	[IdValue] [char](32) NULL,
	[IdRes] [char](32) NULL,
	[ScreenID] [char](8) NULL,
	[CompanyMask] [varbinary](32) NULL,
	[CreatedByID] [uniqueidentifier] NULL,
	[CreatedByScreenID] [char](8) NULL,
	[CreatedDateTime] [datetime] NULL,
	[LastModifiedByID] [uniqueidentifier] NULL,
	[LastModifiedByScreenID] [char](8) NULL,
	[LastModifiedDateTime] [datetime] NULL,
	[tstamp] [timestamp] NULL,
	CONSTRAINT [LocalizationResourceByScreen_tmp_PK] PRIMARY KEY CLUSTERED ([Id_fake])
)
CREATE NONCLUSTERED INDEX [IX_LocalizationResourceByScreen_tmp] ON [dbo].[LocalizationResourceByScreen_tmp] ([IdValue], [CompanyID], [IdRes], [ScreenID])

CREATE TABLE [dbo].[LocalizationTranslation_tmp](
	[Id_fake] [int] identity NOT NULL,
	[CompanyID] [int] NULL,
	[IdValue] [char](32) NULL,
	[IdRes] [char](32) NULL,
	[Locale] [nvarchar](12) NULL,
	[Value] [nvarchar](max) NULL,
	[CompanyMask] [varbinary](32) NULL,
	[CreatedByID] [uniqueidentifier] NULL,
	[CreatedByScreenID] [char](8) NULL,
	[CreatedDateTime] [datetime] NULL,
	[LastModifiedByID] [uniqueidentifier] NULL,
	[LastModifiedByScreenID] [char](8) NULL,
	[LastModifiedDateTime] [datetime] NULL,
	[tstamp] [timestamp] NULL,
	CONSTRAINT [LocalizationTranslation_tmp_PK] PRIMARY KEY CLUSTERED ([Id_fake])
)
CREATE NONCLUSTERED INDEX [IX_LocalizationTranslation_tmp] ON [dbo].[LocalizationTranslation_tmp] ([IdValue], [CompanyID], [IdRes], [Locale])
GO

--[mysql: Native]
--[mssql: Skip]
--Create tables for Localization update script MySql
--[OldHash(Hash = 93ba1121f04bfa43dd0e85030c49ba87086dbc898dfdf178cb3c0691bb343f18)]
--[MinVersion(Hash = 93ba1121f04bfa43dd0e85030c49ba87086dbc898dfdf178cb3c0691bb343f18)]
--[MinVersion(Hash = a8defb395c69c9625ce3c8c0945cf2424e01bcf6a8aab92a981a812526de4f57)]
--[MinVersion(Hash = a8defb395c69c9625ce3c8c0945cf2424e01bcf6a8aab92a981a812526de4f57)]
--[MinVersion(Hash = a8defb395c69c9625ce3c8c0945cf2424e01bcf6a8aab92a981a812526de4f57)]
--[MinVersion(Hash = a8defb395c69c9625ce3c8c0945cf2424e01bcf6a8aab92a981a812526de4f57)]
CREATE TABLE `LocalizationValue_tmp` (
  `Id_Fake` int(11) NOT NULL AUTO_INCREMENT,
  `CompanyID` int(11) DEFAULT '0',
  `Id` char(32) CHARACTER SET latin1 COLLATE latin1_general_ci DEFAULT NULL,
  `Id_new` char(32) CHARACTER SET latin1 COLLATE latin1_general_ci DEFAULT NULL,
  `NeutralValue` longtext COLLATE utf8mb4_unicode_ci,
  `IsNotLocalized` tinyint(1) DEFAULT NULL COMMENT 'sy_boolean;',
  `IsSite` tinyint(1) DEFAULT NULL COMMENT 'sy_boolean;',
  `IsPortal` tinyint(1) DEFAULT NULL COMMENT 'sy_boolean;',
  `IsObsolete` tinyint(1) DEFAULT NULL COMMENT 'sy_boolean;',
  `IsObsoletePortal` tinyint(1) DEFAULT NULL COMMENT 'sy_boolean;',
  `TranslationCount` int(11) DEFAULT NULL,
  `CompanyMask` varbinary(32) DEFAULT 'xAA' COMMENT 'df_MaskAA;',
  `CreatedByID` char(36) CHARACTER SET ascii DEFAULT NULL COMMENT 'sy_guid;',
  `CreatedByScreenID` char(8) CHARACTER SET latin1 COLLATE latin1_general_ci DEFAULT NULL,
  `CreatedDateTime` datetime(6) DEFAULT NULL,
  `LastModifiedByID` char(36) CHARACTER SET ascii DEFAULT NULL COMMENT 'sy_guid;',
  `LastModifiedByScreenID` char(8) CHARACTER SET latin1 COLLATE latin1_general_ci DEFAULT NULL,
  `LastModifiedDateTime` datetime(6) DEFAULT NULL,
  `tstamp` timestamp(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
  KEY `IX_LocalizationValue_tmp` (`Id`, `CompanyID`, `Id_new`),
  PRIMARY KEY (`Id_Fake`)
) DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE `LocalizationResource_tmp` (
  `Id_Fake` int(11) NOT NULL AUTO_INCREMENT,
  `CompanyID` int(11) DEFAULT '0',
  `IdValue` char(32) CHARACTER SET latin1 COLLATE latin1_general_ci DEFAULT NULL,
  `Id` char(32) CHARACTER SET latin1 COLLATE latin1_general_ci DEFAULT NULL,
  `ResKey` longtext COLLATE utf8mb4_unicode_ci,
  `ResType` int(11) DEFAULT NULL,
  `IsSite` tinyint(1) DEFAULT NULL COMMENT 'sy_boolean;',
  `IsPortal` tinyint(1) DEFAULT NULL COMMENT 'sy_boolean;',
  `IsNotLocalized` tinyint(1) DEFAULT NULL COMMENT 'sy_boolean;',
  `CompanyMask` varbinary(32) DEFAULT 'xAA' COMMENT 'df_MaskAA;',
  `CreatedByID` char(36) CHARACTER SET ascii DEFAULT NULL COMMENT 'sy_guid;',
  `CreatedByScreenID` char(8) CHARACTER SET latin1 COLLATE latin1_general_ci DEFAULT NULL,
  `CreatedDateTime` datetime(6) DEFAULT NULL,
  `LastModifiedByID` char(36) CHARACTER SET ascii DEFAULT NULL COMMENT 'sy_guid;',
  `LastModifiedByScreenID` char(8) CHARACTER SET latin1 COLLATE latin1_general_ci DEFAULT NULL,
  `LastModifiedDateTime` datetime(6) DEFAULT NULL,
  `tstamp` timestamp(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
  KEY `IX_LocalizationResource_tmp` (`IdValue`, `CompanyID`, `Id`),
  PRIMARY KEY (`Id_Fake`)
) DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE `LocalizationResourceByScreen_tmp` (
  `Id_Fake` int(11) NOT NULL AUTO_INCREMENT,
  `CompanyID` int(11) DEFAULT '0',
  `IdValue` char(32) CHARACTER SET latin1 COLLATE latin1_general_ci DEFAULT NULL,
  `IdRes` char(32) CHARACTER SET latin1 COLLATE latin1_general_ci DEFAULT NULL,
  `ScreenID` char(8) CHARACTER SET latin1 COLLATE latin1_general_ci DEFAULT NULL,
  `CompanyMask` varbinary(32) DEFAULT 'xAA' COMMENT 'df_MaskAA;',
  `CreatedByID` char(36) CHARACTER SET ascii DEFAULT NULL COMMENT 'sy_guid;',
  `CreatedByScreenID` char(8) CHARACTER SET latin1 COLLATE latin1_general_ci DEFAULT NULL,
  `CreatedDateTime` datetime(6) DEFAULT NULL,
  `LastModifiedByID` char(36) CHARACTER SET ascii DEFAULT NULL COMMENT 'sy_guid;',
  `LastModifiedByScreenID` char(8) CHARACTER SET latin1 COLLATE latin1_general_ci DEFAULT NULL,
  `LastModifiedDateTime` datetime(6) DEFAULT NULL,
  `tstamp` timestamp(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
  KEY `IX_LocalizationResourceByScreen_tmp` (`IdValue`, `CompanyID`, `IdRes`, `ScreenID`),
  PRIMARY KEY (`Id_Fake`)
) DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE `LocalizationTranslation_tmp` (
  `Id_Fake` int(11) NOT NULL AUTO_INCREMENT,
  `CompanyID` int(11) DEFAULT '0',
  `IdValue` char(32) CHARACTER SET latin1 COLLATE latin1_general_ci DEFAULT NULL,
  `IdRes` char(32) CHARACTER SET latin1 COLLATE latin1_general_ci DEFAULT NULL,
  `Locale` varchar(12) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `Value` longtext COLLATE utf8mb4_unicode_ci,
  `CompanyMask` varbinary(32) DEFAULT 'xAA' COMMENT 'df_MaskAA;',
  `CreatedByID` char(36) CHARACTER SET ascii DEFAULT NULL COMMENT 'sy_guid;',
  `CreatedByScreenID` char(8) CHARACTER SET latin1 COLLATE latin1_general_ci DEFAULT NULL,
  `CreatedDateTime` datetime(6) DEFAULT NULL,
  `LastModifiedByID` char(36) CHARACTER SET ascii DEFAULT NULL COMMENT 'sy_guid;',
  `LastModifiedByScreenID` char(8) CHARACTER SET latin1 COLLATE latin1_general_ci DEFAULT NULL,
  `LastModifiedDateTime` datetime(6) DEFAULT NULL,
  `tstamp` timestamp(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
  KEY `IX_LocalizationTranslation_tmp` (`IdValue`, `CompanyID`, `IdRes`, `Locale`),
  PRIMARY KEY (`Id_Fake`)
) DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
GO

--[mysql: Native]
--[mssql: Skip]
--[IfExists(Column = LocalizationValue.IDlv)]
--[OldHash(Hash = 474df156351eb90972b92408975350f2a4042563601b232c0998fb2b52f3eac5)]
--[MinVersion(Hash = 474df156351eb90972b92408975350f2a4042563601b232c0998fb2b52f3eac5)]
--[MinVersion(Hash = d6cffe464896bb8afee212b02c5832f48e6fcd23e8dbcb5cef3563ae547d749c)]
--[MinVersion(Hash = d6cffe464896bb8afee212b02c5832f48e6fcd23e8dbcb5cef3563ae547d749c)]
--[MinVersion(Hash = d6cffe464896bb8afee212b02c5832f48e6fcd23e8dbcb5cef3563ae547d749c)]
--[MinVersion(Hash = d6cffe464896bb8afee212b02c5832f48e6fcd23e8dbcb5cef3563ae547d749c)]
ALTER TABLE `LocalizationValue` ALTER COLUMN `IDlv` SET DEFAULT '00112233-4455-6677-8899-AABBCCDDEEFF';
CREATE TRIGGER `before_insert_LocalizationValue`
  BEFORE INSERT ON `LocalizationValue`
  FOR EACH ROW
	SET NEW.`IDlv` = UUID();

ALTER TABLE `LocalizationResource` ALTER COLUMN `IDlr` SET DEFAULT '00112233-4455-6677-8899-AABBCCDDEEFF';
CREATE TRIGGER `before_insert_LocalizationResource`
  BEFORE INSERT ON `LocalizationResource`
  FOR EACH ROW
	SET NEW.`IDlr` = UUID();

ALTER TABLE `LocalizationTranslation` ALTER COLUMN `IDlt` SET DEFAULT '00112233-4455-6677-8899-AABBCCDDEEFF';
ALTER TABLE `LocalizationTranslation` ALTER COLUMN `ResKey` SET DEFAULT 'qwerty';
CREATE TRIGGER `before_insert_LocalizationTranslation`
  BEFORE INSERT ON `LocalizationTranslation`
  FOR EACH ROW
	SET NEW.`IDlt` = UUID(), NEW.`ResKey` = NEW.`IdValue`;
GO

--[RefreshMetadata]
--[Timeout(Multiplier = 10)]
--[mssql: VmSql]
--Migrate localization strings to case-insensitive state
--Update LocalizationValue
--[OldHash(Hash = 1c921bd9a58adbf65da8134af97c05749e452497e6995c6240e6c75225b3de72)]
--[MinVersion(Hash = 1c921bd9a58adbf65da8134af97c05749e452497e6995c6240e6c75225b3de72)]
--[MinVersion(Hash = cb377fc1c4a0b0e09afa2e66bf891980f887a9c4325ad676b7265b33caa62544)]
--[MinVersion(Hash = cb377fc1c4a0b0e09afa2e66bf891980f887a9c4325ad676b7265b33caa62544)]
--[MinVersion(Hash = cb377fc1c4a0b0e09afa2e66bf891980f887a9c4325ad676b7265b33caa62544)]
--[MinVersion(Hash = cb377fc1c4a0b0e09afa2e66bf891980f887a9c4325ad676b7265b33caa62544)]
INSERT INTO [LocalizationValue_tmp] ([CompanyID], [Id], [Id_new], [NeutralValue], [IsNotLocalized], [IsSite], [IsPortal], [IsObsolete], [IsObsoletePortal], [TranslationCount], [CompanyMask], [CreatedByID], [CreatedByScreenID], [CreatedDateTime], [LastModifiedByID], [LastModifiedByScreenID], [LastModifiedDateTime])
SELECT [CompanyID], [Id], Md5Generic([NeutralValue]), [NeutralValue], [IsNotLocalized], [IsSite], [IsPortal], [IsObsolete], [IsObsoletePortal], [TranslationCount], [CompanyMask], [CreatedByID], [CreatedByScreenID], [CreatedDateTime], [LastModifiedByID], [LastModifiedByScreenID], [LastModifiedDateTime]
FROM [LocalizationValue]

DELETE FROM [LocalizationValue]

INSERT INTO [LocalizationValue] ([CompanyID], [Id], [NeutralValue], [IsNotLocalized], [IsSite], [IsPortal], [IsObsolete], [IsObsoletePortal], [TranslationCount], [CompanyMask], [CreatedByID], [CreatedByScreenID], [CreatedDateTime], [LastModifiedByID], [LastModifiedByScreenID], [LastModifiedDateTime])
SELECT [CompanyID], [Id_new], MAX([NeutralValue]), MIN(CONVERT(int, [IsNotLocalized])), MAX(CONVERT(int, [IsSite])), MAX(CONVERT(int, [IsPortal])), MIN(CONVERT(int, [IsObsolete])), MIN(CONVERT(int, [IsObsoletePortal])), MAX([TranslationCount]), MAX([CompanyMask]), MAX([CreatedByID]), MAX([CreatedByScreenID]), MAX([CreatedDateTime]), MAX([LastModifiedByID]), MAX([LastModifiedByScreenID]), MAX([LastModifiedDateTime])
FROM [LocalizationValue_tmp]
GROUP BY [CompanyID], [Id_new]

--Update LocalizationResource
INSERT INTO [LocalizationResource_tmp] ([CompanyID], [IdValue], [Id], [ResKey], [ResType], [IsSite], [IsPortal], [IsNotLocalized], [CompanyMask], [CreatedByID], [CreatedByScreenID], [CreatedDateTime], [LastModifiedByID], [LastModifiedByScreenID], [LastModifiedDateTime])
SELECT [CompanyID], [IdValue], [Id], [ResKey], [ResType], [IsSite], [IsPortal], [IsNotLocalized], [CompanyMask], [CreatedByID], [CreatedByScreenID], [CreatedDateTime], [LastModifiedByID], [LastModifiedByScreenID], [LastModifiedDateTime]
FROM [LocalizationResource]

DELETE FROM [LocalizationResource]

UPDATE [LocalizationResource_tmp]
SET [IdValue] = lv.[Id_new]
FROM [LocalizationResource_tmp] AS lr
INNER JOIN [LocalizationValue_tmp] AS lv
ON lv.[Id] = lr.[IdValue]

INSERT INTO [LocalizationResource] ([CompanyID], [IdValue], [Id], [ResKey], [ResType], [IsSite], [IsPortal], [IsNotLocalized], [CompanyMask], [CreatedByID], [CreatedByScreenID], [CreatedDateTime], [LastModifiedByID], [LastModifiedByScreenID], [LastModifiedDateTime])
SELECT [CompanyID], [IdValue], [Id], MAX([ResKey]), MAX([ResType]), MAX(CONVERT(int, [IsSite])), MAX(CONVERT(int, [IsPortal])), MIN(CONVERT(int, [IsNotLocalized])), MAX([CompanyMask]), MAX([CreatedByID]), MAX([CreatedByScreenID]), MAX([CreatedDateTime]), MAX([LastModifiedByID]), MAX([LastModifiedByScreenID]), MAX([LastModifiedDateTime])
FROM [LocalizationResource_tmp]
GROUP BY [CompanyID], [IdValue], [Id]

DROP TABLE [LocalizationResource_tmp]

--Update LocalizationResourceByScreen
INSERT INTO [LocalizationResourceByScreen_tmp] ([CompanyID], [IdValue], [IdRes], [ScreenID], [CompanyMask], [CreatedByID], [CreatedByScreenID], [CreatedDateTime], [LastModifiedByID], [LastModifiedByScreenID], [LastModifiedDateTime])
SELECT [CompanyID], [IdValue], [IdRes], [ScreenID], [CompanyMask], [CreatedByID], [CreatedByScreenID], [CreatedDateTime], [LastModifiedByID], [LastModifiedByScreenID], [LastModifiedDateTime]
FROM [LocalizationResourceByScreen]

DELETE FROM [LocalizationResourceByScreen]

UPDATE [LocalizationResourceByScreen_tmp]
SET [IdValue] = lv.[Id_new]
FROM [LocalizationResourceByScreen_tmp] AS lrs
INNER JOIN [LocalizationValue_tmp] AS lv
ON lv.[Id] = lrs.[IdValue]

INSERT INTO [LocalizationResourceByScreen] ([CompanyID], [IdValue], [IdRes], [ScreenID], [CompanyMask], [CreatedByID], [CreatedByScreenID], [CreatedDateTime], [LastModifiedByID], [LastModifiedByScreenID], [LastModifiedDateTime])
SELECT [CompanyID], [IdValue], [IdRes], [ScreenID], MAX([CompanyMask]), MAX([CreatedByID]), MAX([CreatedByScreenID]), MAX([CreatedDateTime]), MAX([LastModifiedByID]), MAX([LastModifiedByScreenID]), MAX([LastModifiedDateTime])
FROM [LocalizationResourceByScreen_tmp]
GROUP BY [CompanyID], [IdValue], [IdRes], [ScreenID]

DROP TABLE [LocalizationResourceByScreen_tmp]

--Update LocalizationTranslation
INSERT INTO [LocalizationTranslation_tmp] ([CompanyID], [IdValue], [IdRes], [Locale], [Value], [CompanyMask], [CreatedByID], [CreatedByScreenID], [CreatedDateTime], [LastModifiedByID], [LastModifiedByScreenID], [LastModifiedDateTime])
SELECT [CompanyID], [IdValue], [IdRes], [Locale], [Value], [CompanyMask], [CreatedByID], [CreatedByScreenID], [CreatedDateTime], [LastModifiedByID], [LastModifiedByScreenID], [LastModifiedDateTime]
FROM [LocalizationTranslation]

DELETE FROM [LocalizationTranslation]

UPDATE [LocalizationTranslation_tmp]
SET [IdValue] = lv.[Id_new]
FROM [LocalizationTranslation_tmp] AS lt
INNER JOIN [LocalizationValue_tmp] AS lv
ON lv.[id] = lt.[IdValue]

INSERT INTO [LocalizationTranslation] ([CompanyID], [IdValue], [IdRes], [Locale], [Value], [CompanyMask], [CreatedByID], [CreatedByScreenID], [CreatedDateTime], [LastModifiedByID], [LastModifiedByScreenID], [LastModifiedDateTime])
SELECT [CompanyID], [IdValue], [IdRes], [Locale], MAX([Value]), MAX([CompanyMask]), MAX([CreatedByID]), MAX([CreatedByScreenID]), MAX([CreatedDateTime]), MAX([LastModifiedByID]), MAX([LastModifiedByScreenID]), MAX([LastModifiedDateTime])
FROM [LocalizationTranslation_tmp]
GROUP BY [CompanyID], [IdValue], [IdRes], [Locale]

DROP TABLE [LocalizationTranslation_tmp]

DROP TABLE [LocalizationValue_tmp]
GO

--[mysql: Native]
--[mssql: Skip]
--[IfExists(Column = LocalizationValue.IDlv)]
--[OldHash(Hash = 5a0e881a4f122807481d2401042fa0b46cd59fbd2b7baf203fad6c8f998c5f8f)]
--[MinVersion(Hash = 5a0e881a4f122807481d2401042fa0b46cd59fbd2b7baf203fad6c8f998c5f8f)]
--[MinVersion(Hash = 03903b1867e8a174baebc7ef82bfe2fe9a733f4ec4252269d26894214ceaa4ad)]
--[MinVersion(Hash = 03903b1867e8a174baebc7ef82bfe2fe9a733f4ec4252269d26894214ceaa4ad)]
--[MinVersion(Hash = 03903b1867e8a174baebc7ef82bfe2fe9a733f4ec4252269d26894214ceaa4ad)]
--[MinVersion(Hash = 03903b1867e8a174baebc7ef82bfe2fe9a733f4ec4252269d26894214ceaa4ad)]
DROP TRIGGER IF EXISTS `before_insert_LocalizationValue`;
DROP TRIGGER IF EXISTS `before_insert_LocalizationResource`;
DROP TRIGGER IF EXISTS `before_insert_LocalizationTranslation`;
GO

--[RefreshMetadata]
--[IfExists(Column = PivotField.DateRounding)]
--[MinVersion(Hash = bc63d0e80a5b126d04b4f7bc4397f4791cfd40c572492e3c5154b738006cadbf)]
--[MinVersion(Hash = bc63d0e80a5b126d04b4f7bc4397f4791cfd40c572492e3c5154b738006cadbf)]
--[MinVersion(Hash = bc63d0e80a5b126d04b4f7bc4397f4791cfd40c572492e3c5154b738006cadbf)]
--[MinVersion(Hash = bc63d0e80a5b126d04b4f7bc4397f4791cfd40c572492e3c5154b738006cadbf)]
--[MinVersion(Hash = bc63d0e80a5b126d04b4f7bc4397f4791cfd40c572492e3c5154b738006cadbf)]
UPDATE PivotField SET Transformation = CONCAT('DR', DateRounding) WHERE DateRounding is not null and DateRounding != 0
GO

--[IfExists(Column = PreferencesGeneral.TimeZone)]
--[MinVersion(Hash = aa91569453b2671feae6a747c38fd5e1fda48694fe49b3d8bc5b10f613331a98)]
--[MinVersion(Hash = aa91569453b2671feae6a747c38fd5e1fda48694fe49b3d8bc5b10f613331a98)]
--[MinVersion(Hash = aa91569453b2671feae6a747c38fd5e1fda48694fe49b3d8bc5b10f613331a98)]
--[MinVersion(Hash = aa91569453b2671feae6a747c38fd5e1fda48694fe49b3d8bc5b10f613331a98)]
--[MinVersion(Hash = aa91569453b2671feae6a747c38fd5e1fda48694fe49b3d8bc5b10f613331a98)]
UPDATE PreferencesGeneral
SET TimeZone = CASE TimeZone	
	WHEN 'GMTM1100A' THEN 'GMTP1300M'
	WHEN 'GMTM0430A' THEN 'GMTM0400Y'
	WHEN 'GMTP0300Y' THEN 'GMTP0400Y'
	WHEN 'GMTP0600A' THEN 'GMTP0700N'
	ELSE TimeZone
	END
WHERE TimeZone IN ('GMTM1100A', 'GMTM0430A', 'GMTP0300Y', 'GMTP0600A')
GO

--[IfExists(Column = PreferencesGeneral.PersonNameFormat)]
--[MinVersion(Hash = 59ba75fc7dfcf07ca8128294bd6b32f01345dbc718fb1377b505f87daed224fd)]
--[MinVersion(Hash = 59ba75fc7dfcf07ca8128294bd6b32f01345dbc718fb1377b505f87daed224fd)]
UPDATE PreferencesGeneral
SET PersonNameFormat = 'LEGACY' WHERE PersonNameFormat IS NULL
GO

--[IfExists(Column = UserPreferences.TimeZone)]
--[MinVersion(Hash = a1e3d8a15d251fba9ab7b76bb17141eb67c9fe350e7d83d078855d6a358b597b)]
--[MinVersion(Hash = a1e3d8a15d251fba9ab7b76bb17141eb67c9fe350e7d83d078855d6a358b597b)]
--[MinVersion(Hash = a1e3d8a15d251fba9ab7b76bb17141eb67c9fe350e7d83d078855d6a358b597b)]
--[MinVersion(Hash = a1e3d8a15d251fba9ab7b76bb17141eb67c9fe350e7d83d078855d6a358b597b)]
--[MinVersion(Hash = a1e3d8a15d251fba9ab7b76bb17141eb67c9fe350e7d83d078855d6a358b597b)]
UPDATE UserPreferences
SET TimeZone = CASE TimeZone	
	WHEN 'GMTM1100A' THEN 'GMTP1300M'
	WHEN 'GMTM0430A' THEN 'GMTM0400Y'
	WHEN 'GMTP0300Y' THEN 'GMTP0400Y'
	WHEN 'GMTP0600A' THEN 'GMTP0700N'
	ELSE TimeZone
	END
WHERE TimeZone IN ('GMTM1100A', 'GMTM0430A', 'GMTP0300Y', 'GMTP0600A')
GO

--[IfExists(Column = AUSchedule.TimeZoneID)]
--[MinVersion(Hash = 9bdee68eb78745205cee74a69e726e4e2ba44e4060225b92eddccc4997c4b0f9)]
--[MinVersion(Hash = 9bdee68eb78745205cee74a69e726e4e2ba44e4060225b92eddccc4997c4b0f9)]
--[MinVersion(Hash = 9bdee68eb78745205cee74a69e726e4e2ba44e4060225b92eddccc4997c4b0f9)]
--[MinVersion(Hash = 9bdee68eb78745205cee74a69e726e4e2ba44e4060225b92eddccc4997c4b0f9)]
--[MinVersion(Hash = 9bdee68eb78745205cee74a69e726e4e2ba44e4060225b92eddccc4997c4b0f9)]
UPDATE AUSchedule
SET TimeZoneID = CASE TimeZoneID	
	WHEN 'GMTM1100A' THEN 'GMTP1300M'
	WHEN 'GMTM0430A' THEN 'GMTM0400Y'
	WHEN 'GMTP0300Y' THEN 'GMTP0400Y'
	WHEN 'GMTP0600A' THEN 'GMTP0700N'
	ELSE TimeZoneID
	END
WHERE TimeZoneID IN ('GMTM1100A', 'GMTM0430A', 'GMTP0300Y', 'GMTP0600A')
GO

--[IfExists(Column = CSCalendar.TimeZone)]
--[MinVersion(Hash = b27d95e4f3b5ecceb6c93c4c01bb4882e8973e7cc773d38149db95ab3b8764b2)]
--[MinVersion(Hash = b27d95e4f3b5ecceb6c93c4c01bb4882e8973e7cc773d38149db95ab3b8764b2)]
--[MinVersion(Hash = b27d95e4f3b5ecceb6c93c4c01bb4882e8973e7cc773d38149db95ab3b8764b2)]
--[MinVersion(Hash = b27d95e4f3b5ecceb6c93c4c01bb4882e8973e7cc773d38149db95ab3b8764b2)]
--[MinVersion(Hash = b27d95e4f3b5ecceb6c93c4c01bb4882e8973e7cc773d38149db95ab3b8764b2)]
UPDATE CSCalendar
SET TimeZone = CASE TimeZone	
	WHEN 'GMTM1100A' THEN 'GMTP1300M'
	WHEN 'GMTM0430A' THEN 'GMTM0400Y'
	WHEN 'GMTP0300Y' THEN 'GMTP0400Y'
	WHEN 'GMTP0600A' THEN 'GMTP0700N'
	ELSE TimeZone
	END
WHERE TimeZone IN ('GMTM1100A', 'GMTM0430A', 'GMTP0300Y', 'GMTP0600A')
GO

--[IfExists(Column = WikiDescriptor.IsActive)]
--[MinVersion(Hash = b57043b554d649efff92e2f1f550865967649eedc342f3bf6ca952aabe041f2c)]
--[MinVersion(Hash = b57043b554d649efff92e2f1f550865967649eedc342f3bf6ca952aabe041f2c)]
--[MinVersion(Hash = b57043b554d649efff92e2f1f550865967649eedc342f3bf6ca952aabe041f2c)]
--[MinVersion(Hash = b57043b554d649efff92e2f1f550865967649eedc342f3bf6ca952aabe041f2c)]
update WikiDescriptor set
	Position = '1',
	IsActive = '1',
	DefaultUrl = '86177E2B-BA51-483E-9AC2-6C3A02B48CC7'
where PageID = '5f840b3d-a58f-472e-9303-dca2591698dc' 

update WikiDescriptor set 
	Position = '2', 
	IsActive = '1', 
	DefaultUrl = '24499660-CC16-4976-BC88-00F4A9A38739' 
where PageID='20f237dd-409f-4338-b5ef-39cff26e1985'

update WikiDescriptor set 
	Position = '3', 
	IsActive = '1', 
	DefaultUrl = 'FCE35D0B-A014-4903-A849-31884E1E8A6C' 
where PageID='66879f04-d7e3-4d12-8e3d-ec9b694c2f85'

update WikiDescriptor set 
	Position = '4', 
	IsActive = '1', 
	DefaultUrl = 'A7107850-2BA2-48C3-913F-EFBED9F306D8' 
where PageID='b5004480-b8ac-4294-a2f4-035e2d0c657e'

update WikiDescriptor set 
	Position = '5', 
	IsActive = '1', 
	DefaultUrl = 'A4BE22B4-AE3B-43E0-A004-150A98B7CDA8' 
where PageID='dfb882d4-f5be-44d0-a729-9698faed3326'

update WikiDescriptor set 
	Position = '6', 
	IsActive = '1', 
	DefaultUrl = '24B77BBE-0EB3-4D37-9350-071AE5743571' 
where PageID='c9650508-8c8f-4770-8e8e-927b3000eccf'

update WikiDescriptor set 
	Position = '7', 
	IsActive = '1', 
	DefaultUrl = '316B14FA-F406-4788-993C-7B043B1C5BD9' 
where PageID='34d5aaf6-a66c-493d-ae9c-333dd0bcb6fc'

update WikiDescriptor set 
	Position = '8', 
	IsActive = '1', 
	DefaultUrl = '41099FEF-66BF-4A9F-B57D-6C066B26ABF2' 
where PageID='73019742-84e3-4454-ada0-904017859150'

update WikiDescriptor set 
	Position = '9', 
	IsActive = '1' 
	where PageID = '69bf9d26-d4d9-4511-8793-1a73dcaf87ce'

update WikiDescriptor set 
	Position = '10', 
	IsActive = '1', 
	DefaultUrl = '414552D7-C156-461B-B064-4119DE76C903' 
where PageID='d562bede-d318-416c-a4eb-d8c447af225e'
GO

-- Insert all existing dashboard nodes into Data Views workspace in Dashboards subcategory
--[SmartExecute]
--[MinVersion(Hash = fc9d633a42c90b5339b3d0d3de99db9d55e387971cb62752a20bf875f6683e28)]
--[MinVersion(Hash = fc9d633a42c90b5339b3d0d3de99db9d55e387971cb62752a20bf875f6683e28)]
--[MinVersion(Hash = fc9d633a42c90b5339b3d0d3de99db9d55e387971cb62752a20bf875f6683e28)]
--[MinVersion(Hash = fc9d633a42c90b5339b3d0d3de99db9d55e387971cb62752a20bf875f6683e28)]
declare @cid int;
declare @cmask varbinary(32)
declare @nodeId uniqueidentifier
declare @prevNodeId uniqueidentifier
declare @wid uniqueidentifier = 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb' -- "Data Views" workspace
declare @sid uniqueidentifier = '29d8e453-ee74-405d-abe1-930728d1f9ba' -- "Dashboards" subcategory
declare @maxOrder real;

set @maxOrder = (select isnull(max([Order]), 0) from MUIScreen where WorkspaceID = @wid and SubcategoryID = @sid);

declare data_cursor cursor for
select sm.NodeID, sm.CompanyID, sm.CompanyMask from Dashboard d
inner join SiteMap sm on d.ScreenID = sm.ScreenID
left join MUIScreen mui on sm.NodeID = mui.NodeID
where mui.NodeID is null and d.IsPortal = 0
group by sm.NodeID, sm.CompanyID, sm.CompanyMask
order by sm.NodeID, sm.CompanyID

open data_cursor
fetch next from data_cursor into @nodeId, @cid, @cmask
while @@FETCH_STATUS = 0
begin
	if (@prevNodeId != @nodeId)
	begin
		set @maxOrder = @maxOrder + 64
	end

	insert into MUIScreen(CompanyID, NodeID, WorkspaceID, [Order], SubcategoryID, CompanyMask)
	values (@cid, @nodeId, @wid, @maxOrder, @sid, @cmask)

	set @prevNodeId = @nodeId
	fetch next from data_cursor into @nodeId, @cid, @cmask
end
close data_cursor
deallocate data_cursor
GO

-- Disable New UI for existing installations
--[SmartExecute]
--[OldHash(Hash = acf28608c9d06699771c18795424bffddd53df88e7687384ccdb2743f5af8149)]
--[MinVersion(Hash = e07eb41e1b56ec526860114eedc916ed8680b2a5f2e4c2ff742d2e1c508016db)]
if exists (select * from Users) -- there is no data at this moment when we creating new DB
begin
	insert into UserPreferences (CompanyID, UserID, SignatureToReplyAndForward, SignatureToNewEmail, CreatedByID, CreatedByScreenID, CreatedDateTime, LastModifiedByID, LastModifiedByScreenID, LastModifiedDateTime, CompanyMask, NoteID)
	select u.CompanyID, u.PKID, 0, 1, 'B5344897-037E-4D58-B5C3-1BDFD0F47BF9', '00000000', GETDATE(), 'B5344897-037E-4D58-B5C3-1BDFD0F47BF9', '00000000', GETDATE(), u.CompanyMask, NEWID() from Users u
	left join UserPreferences pref
	on u.PKID = pref.UserID
	where pref.UserID is null
	group by u.CompanyID, u.PKID, u.CompanyMask
end
GO

-- Insert all existing ARm nodes into Data Views workspace in Financial Statements subcategory
--[SmartExecute]
--[MinVersion(Hash = 3d6f5bf70471e8bf69413f8197958e60c41924977dc29b1cfa894fa8a98c83f2)]
--[MinVersion(Hash = 3d6f5bf70471e8bf69413f8197958e60c41924977dc29b1cfa894fa8a98c83f2)]
--[MinVersion(Hash = 3d6f5bf70471e8bf69413f8197958e60c41924977dc29b1cfa894fa8a98c83f2)]
--[MinVersion(Hash = 3d6f5bf70471e8bf69413f8197958e60c41924977dc29b1cfa894fa8a98c83f2)]
declare @cid int;
declare @cmask varbinary(32)
declare @nodeId uniqueidentifier
declare @prevNodeId uniqueidentifier
declare @wid uniqueidentifier = 'B5EC7B62-D2E5-4234-999D-0C92A0B0B74D' -- "Data Views" workspace
declare @sid uniqueidentifier = 'AB5C09BF-AA38-48AB-B0CE-6D464DD2846A' -- "Financial Statements" subcategory
declare @maxOrder real;

set @maxOrder = (select isnull(max([Order]), 0) from MUIScreen where WorkspaceID = @wid and SubcategoryID = @sid);

declare data_cursor cursor for
select sm.NodeID, sm.CompanyID, sm.CompanyMask from RMReport r
inner join SiteMap sm 
on sm.Url = '~/Frames/RMLauncher.aspx?ID=' + r.ReportCode or
sm.Url = '~/Frames/RMLauncher.aspx?ID=' + r.ReportCode + '.rpx'
left join MUIScreen mui on sm.NodeID = mui.NodeID
where mui.NodeID is null
group by sm.NodeID, sm.CompanyID, sm.CompanyMask
order by sm.NodeID, sm.CompanyID

open data_cursor
fetch next from data_cursor into @nodeId, @cid, @cmask
while @@FETCH_STATUS = 0
begin
	if (@prevNodeId != @nodeId)
	begin
		set @maxOrder = @maxOrder + 64
	end

	insert into MUIScreen(CompanyID, NodeID, WorkspaceID, [Order], SubcategoryID, CompanyMask)
	values (@cid, @nodeId, @wid, @maxOrder, @sid, @cmask)

	set @prevNodeId = @nodeId
	fetch next from data_cursor into @nodeId, @cid, @cmask
end
close data_cursor
deallocate data_cursor
GO

-- Insert all existing Pivot Table nodes into Data Views workspace in Pivot Tables subcategory
--[SmartExecute]
--[MinVersion(Hash = 7dc18dd6a4ab1703029cb3de5f037c3f6388bb866437bdb4d0ecaaf986d81768)]
--[MinVersion(Hash = 7dc18dd6a4ab1703029cb3de5f037c3f6388bb866437bdb4d0ecaaf986d81768)]
--[MinVersion(Hash = 7dc18dd6a4ab1703029cb3de5f037c3f6388bb866437bdb4d0ecaaf986d81768)]
--[MinVersion(Hash = 7dc18dd6a4ab1703029cb3de5f037c3f6388bb866437bdb4d0ecaaf986d81768)]
declare @cid int;
declare @cmask varbinary(32)
declare @nodeId uniqueidentifier
declare @prevNodeId uniqueidentifier
declare @wid uniqueidentifier = 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb' -- "Data Views" workspace
declare @sid uniqueidentifier = '717C7AA8-A4D2-4C84-A4CF-CCDF4C0B68AC' -- "Pivot Tables" subcategory
declare @maxOrder real;

set @maxOrder = (select isnull(max([Order]), 0) from MUIScreen where WorkspaceID = @wid and SubcategoryID = @sid);

declare data_cursor cursor for
select sm.NodeID, sm.CompanyID, sm.CompanyMask from PivotTable p
inner join SiteMap sm 
on sm.Url like concat('~/Pivot/Pivot.aspx?ScreenID=________&PivotTableID=', convert(nvarchar(max), p.PivotTableID), '%') or
sm.Url like concat('~/Pivot/Pivot.aspx?PivotTableID=', convert(nvarchar(max), p.PivotTableID), '%')
left join MUIScreen mui on sm.NodeID = mui.NodeID
where mui.NodeID is null
group by sm.NodeID, sm.CompanyID, sm.CompanyMask
order by sm.NodeID, CompanyID

open data_cursor
fetch next from data_cursor into @nodeId, @cid, @cmask
while @@FETCH_STATUS = 0
begin
	if (@prevNodeId != @nodeId)
	begin
		set @maxOrder = @maxOrder + 64
	end

	insert into MUIScreen(CompanyID, NodeID, WorkspaceID, [Order], SubcategoryID, CompanyMask)
	values (@cid, @nodeId, @wid, @maxOrder, @sid, @cmask)

	set @prevNodeId = @nodeId
	fetch next from data_cursor into @nodeId, @cid, @cmask
end
close data_cursor
deallocate data_cursor
GO

-- Insert all existing GI nodes from Classic UI into corresponding workspaces in Inquiry category in Modern UI
--[SmartExecute]
--[OldHash(Hash = 848b91d2aee72b9c7e563d03c76de2b00e36a7d2f43e73db64987e924bd79721)]
--[MinVersion(Hash = d5a5acef252adf84b0798c464fd07e66b8b7062c1652d27e80b2bb57aa21e195)]
declare @cid int;
declare @p3screenId varchar(8)
declare @p4screenId varchar(8)
declare @screenId varchar(8)
declare @cmask varbinary(32)
declare @nodeId uniqueidentifier
declare @wid uniqueidentifier -- WorkspaceID
declare @prevWid uniqueidentifier
declare @sid uniqueidentifier = '98E86774-69E3-41EA-B94F-EB2C7A8426D4' -- "Inquiries" subcategory
declare @maxOrder real;

declare data_cursor cursor for
select sm.CompanyID, sm.NodeID, sm.CompanyMask, max(p3.ScreenID), max(p4.ScreenID) from SiteMap sm
inner join SiteMap p1 on p1.NodeID = sm.ParentID
inner join SiteMap p2 on p2.NodeID = p1.ParentID
inner join SiteMap p3 on p3.NodeID = p2.ParentID
left join SiteMap p4 on p4.NodeID = p3.ParentID
left join MUIScreen mui on mui.NodeID = sm.NodeID
where (p3.ScreenID is not null and p3.ScreenID != '00000000' or p4.ScreenID is not null and p4.ScreenID != '00000000')
	and p3.ScreenID != 'HD000000' and p4.ScreenID != 'HD000000'
	and sm.Url like '~/GenericInquiry/GenericInquiry.aspx%'
	and mui.NodeID is null
group by sm.CompanyID, sm.NodeID, sm.CompanyMask
order by max(p3.ScreenID)

open data_cursor
fetch next from data_cursor into @cid, @nodeId, @cmask, @p3screenId, @p4screenId
while @@FETCH_STATUS = 0
begin
	if (@p4screenId is not null and @p4screenId != '00000000') -- if there is additional custom folder
		set @screenId = @p4screenId;
	else
		set @screenId = @p3screenId;
	
	set @wid = (select case
		when @screenId = 'AI000000' then '51CE11C7-DFB9-4637-8C8A-917B358E413C' -- Automation -> System Management
		when @screenId = 'AP000000' then 'C6F8A479-2339-4665-A20D-50F5BF38C228' -- Accounts Payable -> Payables
		when @screenId = 'AR000000' then 'D5CBC4C3-5F8B-40DA-80C9-49897F1F74B8' -- Accounts Receivable -> Receivables
		when @screenId = 'CA000000' then '3AA30CCC-5763-453F-8469-35507EB3CC4F' -- Cash Management -> Banking
		when @screenId = 'CM000000' then '6A637FDF-EE71-441C-B9CE-B47B91728EA6' -- Currency Management -> Currency Management
		when @screenId = 'CO000000' then '244FAB62-7ACC-4FFD-8367-569493194460' -- Communication -> Time and Expenses
		when @screenId = 'CR000000' then '5266A681-AD68-4B2D-B94D-0CDBD24128D6' -- Customer Management -> Marketing
		when @screenId = 'CS000000' then '3206E17E-8A34-4E3E-9648-5CEE25DEFDE5' -- Configuration -> Configuration
		when @screenId = 'CU000000' then 'B6902036-C01F-44CB-84FF-340DA661381B' -- Customization -> Customization
		when @screenId = 'DM000000' then '51CE11C7-DFB9-4637-8C8A-917B358E413C' -- Document Management -> System Management
		when @screenId = 'DR000000' then 'AC7E0B27-825D-42E9-BA99-7A5AADDEC2C9' -- Deferred Revenue -> Deferred Revenue
		when @screenId = 'EM000000' then '244FAB62-7ACC-4FFD-8367-569493194460' -- Email -> Time and Expenses
		when @screenId = 'EQ000000' then '3541FC3A-BE4F-4BC1-B9F0-F908D27841C5' -- Equipment Management -> Equipment
		when @screenId = 'FA000000' then '18A67EBA-D15D-4C72-A752-874DBA96AD3F' -- Fixed Assets -> Fixed Assets
		when @screenId = 'GL000000' then 'B5EC7B62-D2E5-4234-999D-0C92A0B0B74D' -- General Ledger -> Finance
		when @screenId = 'IN000000' then '6557C1C6-747E-45BB-9072-54F096598D61' -- Inventory -> Inventory
		when @screenId = 'IS000000' then 'A7303716-FC3E-45F7-AE84-5F5EA75A8204' -- Integration -> Integration
		when @screenId = 'OS000000' then '3206E17E-8A34-4E3E-9648-5CEE25DEFDE5' -- Organization Structure -> Configuration
		when @screenId = 'PM000000' then '6DBFA68E-79E9-420B-9F64-E1036A28998C' -- Projects -> Projects
		when @screenId = 'PO000000' then '5D973D14-A4D0-4193-9A87-7856486C62EE' -- Purchase Orders -> Purchases
		when @screenId = 'RQ000000' then '5D973D14-A4D0-4193-9A87-7856486C62EE' -- Purchase Requisitions -> Purchases
		when @screenId = 'RS000000' then 'E5EA142D-EEB2-41CC-A36D-8521753AFEF6' -- Row-Level Security -> Row Level Security
		when @screenId = 'RT000000' then '9E0967E7-B8B1-4A20-87E2-F96D2273A326' -- Route Management -> Routes
		when @screenId = 'SM000000' then '51CE11C7-DFB9-4637-8C8A-917B358E413C' -- Management -> System Management
		when @screenId = 'SO000000' then 'E2C3849A-6280-41DF-81F3-552B91ADFAE5' -- Sales Orders -> Sales Orders
		when @screenId = 'SV000000' then 'F0CF4498-99C4-48F3-8DD8-BDE9ED67F9D3' -- Service Management -> Services
		when @screenId = 'TM000000' then '244FAB62-7ACC-4FFD-8367-569493194460' -- Time & Expenses -> Time and Expenses
		when @screenId = 'TX000000' then '3CA80FC2-2FA4-485A-8672-C8832DAA2ACC' -- Taxes -> Taxes
		when @screenId = 'US000000' then '9260E24B-7D67-4EC6-8073-F95DF4D24075' -- User Security -> User Security
		else 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb'								-- all other -> Data Views
	end)
	
	if (@prevWid != @wid)
		set @maxOrder = (select isnull(max([Order]), 0) from MUIScreen where WorkspaceID = @wid and SubcategoryID = @sid);
		
	set @maxOrder = @maxOrder + 64;

	insert into MUIScreen(CompanyID, NodeID, WorkspaceID, [Order], SubcategoryID, CompanyMask)
	values (@cid, @nodeId, @wid, @maxOrder, @sid, @cmask)

	set @prevWid = @wid
	fetch next from data_cursor into @cid, @nodeId, @cmask, @p3screenId, @p4screenId
end
close data_cursor
deallocate data_cursor
GO

-- Set FilterType = Advanced for all existed filters
--[MinVersion(Hash = 021b31eea1e7d4e25e1323b1c09183778cabf5dfb813b52a8ad694451757b370)]
--[MinVersion(Hash = 021b31eea1e7d4e25e1323b1c09183778cabf5dfb813b52a8ad694451757b370)]
--[MinVersion(Hash = 021b31eea1e7d4e25e1323b1c09183778cabf5dfb813b52a8ad694451757b370)]
UPDATE FilterRow
SET FilterRow.FilterType = 0
GO

--[MinVersion(Hash = 902e75c9c418726a607fd90d8ef834be1cb3fe6124caad5c79b341fedba202e0)]
--[MinVersion(Hash = 902e75c9c418726a607fd90d8ef834be1cb3fe6124caad5c79b341fedba202e0)]
--[MinVersion(Hash = 902e75c9c418726a607fd90d8ef834be1cb3fe6124caad5c79b341fedba202e0)]
--[MinVersion(Hash = 902e75c9c418726a607fd90d8ef834be1cb3fe6124caad5c79b341fedba202e0)]
update wde set wde.RootPageId = wde.RootPrintPageId
from WikiDescriptorExt wde where CompanyID = 1
GO

--[MinVersion(Hash = 382568496291131b511740f589bd891ce25f3e01901a1d193f5be039036ae298)]
--[MinVersion(Hash = 382568496291131b511740f589bd891ce25f3e01901a1d193f5be039036ae298)]
--[MinVersion(Hash = 382568496291131b511740f589bd891ce25f3e01901a1d193f5be039036ae298)]
--[MinVersion(Hash = 382568496291131b511740f589bd891ce25f3e01901a1d193f5be039036ae298)]
UPDATE
	wd
SET
	wd.rootpageid = WP.good
FROM
	wikidescriptorext wd
INNER JOIN (
	SELECT
		wp2.pageid AS [BAD],
		wp1.pageid AS [GOOD]
	FROM
		wikipage wp2
	INNER JOIN wikipage wp1
		ON wp1.wikiid = wp2.wikiid
		AND wp1.NAME = wp2.NAME
	WHERE
		wp2.NAME = 'ContainerTemplate:HelpRoot'
		AND wp1.companyid = 1
		AND wp2.companyid > 1
) WP
	ON [WP].[bad] = wd.rootpageid
	
UPDATE
	wd
SET
	wd.rootpageid = WP.good
FROM
	wikidescriptorext wd
INNER JOIN (
	SELECT
		wp2.pageid AS [BAD],
		wp1.pageid AS [GOOD]
	FROM
		wikipage wp2
	INNER JOIN wikipage wp1
		ON wp1.wikiid = wp2.wikiid
		AND wp1.NAME = wp2.NAME
	WHERE
		wp2.NAME = 'GenTemplate:HelpHeaderPanel'
		AND wp1.companyid = 1
		AND wp2.companyid > 1
) WP
	ON [WP].[bad] = wd.rootpageid

UPDATE
	wd
SET
	wd.rootpageid = WP.good
FROM
	wikidescriptorext wd
INNER JOIN (
	SELECT
		wp2.pageid AS [BAD],
		wp1.pageid AS [GOOD]
	FROM
		wikipage wp2
	INNER JOIN wikipage wp1
		ON wp1.wikiid = wp2.wikiid
		AND wp1.NAME = wp2.NAME
	WHERE
		wp2.NAME = 'GenTemplate:HelpFooterPanel'
		AND wp1.companyid = 1
		AND wp2.companyid > 1
) WP
	ON [WP].[bad] = wd.rootpageid

UPDATE
	wr
SET
	wr.pageid = WP.good
FROM
	wikirevision wr
INNER JOIN (
	SELECT
		wp2.pageid AS [BAD],
		wp1.pageid AS [GOOD]
	FROM
		wikipage wp2
	INNER JOIN wikipage wp1
		ON wp1.wikiid = wp2.wikiid
		AND wp1.NAME = wp2.NAME
	WHERE
		wp2.NAME IN ('ContainerTemplate:HelpRoot',
		'GenTemplate:HelpHeaderPanel',
		'GenTemplate:HelpFooterPanel')
		AND wp1.companyid = 1
		AND wp2.companyid > 1
) WP
	ON [WP].[bad] = wr.pageid


UPDATE
	wl
SET
	wl.pageid = WP.good
FROM
	wikipagelanguage wl
INNER JOIN (
	SELECT
		wp2.pageid AS [BAD],
		wp1.pageid AS [GOOD]
	FROM
		wikipage wp2
	INNER JOIN wikipage wp1
		ON wp1.wikiid = wp2.wikiid
		AND wp1.NAME = wp2.NAME
	WHERE
		wp2.NAME IN ('ContainerTemplate:HelpRoot',
		'GenTemplate:HelpHeaderPanel',
		'GenTemplate:HelpFooterPanel')
		AND wp1.companyid = 1
		AND wp2.companyid > 1
) WP
	ON [WP].[bad] = wl.pageid

UPDATE
	wp_res
SET
	wp_res.pageid = WP.good,
	wp_res.parentuid = WP.[goodparent],
	wp_res.folder = WP.[goodfolder]
FROM
	wikipage wp_res
INNER JOIN (
	SELECT
		wp2.pageid AS [BAD],
		wp1.pageid AS [GOOD],
		wp1.parentuid AS [GoodParent],
		wp1.folder AS [GoodFolder]
	FROM
		wikipage wp2
	INNER JOIN wikipage wp1
		ON wp1.wikiid = wp2.wikiid
		AND wp1.NAME = wp2.NAME
	WHERE
		wp2.NAME IN ('ContainerTemplate:HelpRoot',
		'GenTemplate:HelpHeaderPanel',
		'GenTemplate:HelpFooterPanel')
		AND wp1.companyid = 1
		AND wp2.companyid > 1
) WP
	ON [WP].[bad] = wp_res.pageid  

update wp set wp.ParentUID = '59DDABDA-D321-A07D-EB30-C7457CE6152A' 
from WikiPage wp
where wp.WikiID = 'B5004480-B8AC-4294-A2F4-035E2D0C657E' 
and wp.Name like '%GenTemplate:%'

update wp set wp.ParentUID = 'B3BD0873-19ED-D072-AF14-6B01BE102F43' 
from WikiPage wp
where wp.WikiID = '34D5AAF6-A66C-493D-AE9C-333DD0BCB6FC' 
and wp.Name like '%GenTemplate:%'

update wp set wp.ParentUID = '1C42E88C-EEB1-BB3C-9CF8-5C9DFDA11BFD' 
from WikiPage wp
where wp.WikiID = '20F237DD-409F-4338-B5EF-39CFF26E1985' 
and wp.Name like '%GenTemplate:%'

update wp set wp.ParentUID = '2B642ECA-BAD7-CE12-9CF1-C4D7A767CEEA' 
from WikiPage wp
where wp.WikiID = '73019742-84E3-4454-ADA0-904017859150' 
and wp.Name like '%GenTemplate:%'

update wp set wp.ParentUID = '97EA5F26-CD48-D7BE-B3BB-6D9828EDB650' 
from WikiPage wp
where wp.WikiID = 'C9650508-8C8F-4770-8E8E-927B3000ECCF' 
and wp.Name like '%GenTemplate:%'

update wp set wp.ParentUID = 'B29FD069-36A3-637C-CCDC-B682D0293571' 
from WikiPage wp
where wp.WikiID = 'DFB882D4-F5BE-44D0-A729-9698FAED3326' 
and wp.Name like '%GenTemplate:%'

update wp set wp.ParentUID = '967B79E7-F3CC-4EA7-45BD-DAA6EA548E93' 
from WikiPage wp
where wp.WikiID = 'A4A68914-72B8-4265-ABB6-B27A271C638C' 
and wp.Name like '%GenTemplate:%'

update wp set wp.ParentUID = '967B79E7-F3CC-4EA7-45BD-DAA6EA548E93' 
from WikiPage wp
where wp.WikiID = 'A4A68914-72B8-4265-ABB6-B27A271C638C' 
and wp.Name like '%GenTemplate:%'

update wp set wp.ParentUID = 'DF6E3934-AD3A-F177-3CE6-B1EFE1A97065' 
from WikiPage wp
where wp.WikiID = 'D562BEDE-D318-416C-A4EB-D8C447AF225E' 
and wp.Name like '%GenTemplate:%'

update wp set wp.ParentUID = '24AE8B71-5A56-F1BE-3F2C-D9B485D9A244' 
from WikiPage wp
where wp.WikiID = '5F840B3D-A58F-472E-9303-DCA2591698DC' 
and wp.Name like '%GenTemplate:%'

update wp set wp.ParentUID = '13E552F5-9E77-8B99-DE29-2EC7E9949EE9' 
from WikiPage wp
where wp.WikiID = '66879F04-D7E3-4D12-8E3D-EC9B694C2F85' 
and wp.Name like '%GenTemplate:%'
GO

--[MinVersion(Hash = 037d049db8bf3e6811fb853694aa8ae81299c5ca6a980c8b722811a6fdc15d46)]
--[MinVersion(Hash = 037d049db8bf3e6811fb853694aa8ae81299c5ca6a980c8b722811a6fdc15d46)]
UPDATE AUDefinitionDetail
SET TableName = (SELECT TOP 1 TableName FROM AUStep S WHERE S.CompanyID = D.CompanyID AND S.ScreenID = D.ScreenID)
FROM AUDefinitionDetail D
WHERE TableName IS NULL
GO

-- Remove all wiki screens from Modern UI menu because there is a special frameset for wiki
--[SmartExecute]
--[MinVersion(Hash = 640d075fa9e40e484b9d8b479cde33541609b27e4b37cf2a3c3ac1ff1dfd58ba)]
--[MinVersion(Hash = 640d075fa9e40e484b9d8b479cde33541609b27e4b37cf2a3c3ac1ff1dfd58ba)]
--[MinVersion(Hash = 640d075fa9e40e484b9d8b479cde33541609b27e4b37cf2a3c3ac1ff1dfd58ba)]
delete s from MUIScreen s inner join WikiDescriptor w on s.NodeID = w.PageID
GO

-- Move per line flag rom subscriber level to event level
--[SmartExecute]
--[MinVersion(Hash = bd258da4175dc3644c3811a57aa865524fec32a60aaacc56c3de59be9ae518cc)]
--[MinVersion(Hash = 23ff201c6621f93a8649d6291d071af03ed359e776a458d9145cb3136ed77935)]
--[MinVersion(Hash = 23ff201c6621f93a8649d6291d071af03ed359e776a458d9145cb3136ed77935)]
update [BPEvent] 
set RowProcessingType = coalesce ((select top 1 isprocesssingleline from BPEventSubscriber where EventID = BPEvent.EventId and IsProcessSingleLine = 1), 0)
from BPEvent
GO

--[MinVersion(Hash = 7885928386f2c04bce3c85310c3eb9df3d29e4a72205080fd2f940fc74fb5097)]
--[MinVersion(Hash = 7885928386f2c04bce3c85310c3eb9df3d29e4a72205080fd2f940fc74fb5097)]
update GIResult set SortOrder = LineNbr where SortOrder is null
GO

--[SmartExecute]
--[MinVersion(Hash = b81492248f5e2c0218368f471c0d152416bfca6f0f42dc8d40ab94d21031e196)]
--[MinVersion(Hash = b81492248f5e2c0218368f471c0d152416bfca6f0f42dc8d40ab94d21031e196)]
update PivotTable set NoteID = NEWID() where NoteID is null
GO

-- Create a filter record for every privately owned pivot table
--[IfExists(Column = PivotTable.OwnerName)]
--[OldHash(Hash = 41339aac4f957457ead6590791ff5e0a41e0ea6e78237468e380eb328957c624)]
--[MinVersion(Hash = 86920939045d8e7a696f77b8d0ed0cd5b485532a74e4f38a331255195b690306)]
declare @companyID int
declare @ownerName varchar(64)
declare @name nvarchar(255)
declare @screenID varchar(8)
declare @companyMask varbinary(max)
declare @noteID uniqueidentifier

declare pivot_cursor cursor for
select CompanyID, OwnerName, ScreenID, Name, CompanyMask, NoteID from PivotTable
where OwnerName is not null

open pivot_cursor
fetch next from pivot_cursor into @companyID, @ownerName, @screenID, @name, @companyMask, @noteID
while @@FETCH_STATUS = 0 begin
	insert into FilterHeader (CompanyID, UserName, ScreenID, ViewName, FilterName, IsDefault, IsShared, IsShortcut, IsSystem, IsHidden, CompanyMask, NoteID, RefNoteID)
	values (@companyID, @ownerName, @screenID, 'Results', left(@name, 50), 0, 0, 0, 0, 0, @companyMask, NEWID(), @noteID)
	fetch next from pivot_cursor into @companyID, @ownerName, @screenID, @name, @companyMask, @noteID
end
close pivot_cursor
deallocate pivot_cursor
GO

-- Default "SortType" column: DateTime and FinPeriod fields must be sorted by value, not by display value
--[MinVersion(Hash = a9be9d0681533b95714bcf37451f79eb72b8aa4212201f7628d260c7ffde50be)]
update PivotField set SortType = 1
where [Expression] like '%Date]' or [Expression] like '%DateTime]' 
or [Expression] like '%Period]' or [Expression] like '%PeriodID]'
GO

--[IfExists(Column = GIResult.FastFilter)]
--[MinVersion(Hash = 1038bca02c46883f79369e2f04651176eb71725d2dcee0d51be1719bc7d8f908)]
update [GIResult] set [FastFilter] = 0 where [Field] like '%Date' or [Field] like '%DateTime'
GO

-- Set default values for AUSchedule history
--[MinVersion(Hash = a86c5428150eb8a7d7217c3a3ede7d8cbd4c91d1156a08e868542cc7f6ca670f)]
UPDATE [dbo].[AUSchedule]
   SET [HistoryRetainCount] = 1
      ,[KeepFullHistory] = 0
 WHERE [HistoryRetainCount] is null
GO

-- Delete all schedule history except last record for each schedule
--[MinVersion(Hash = 99ba722656b769cfc105401cf33fb3d307b1e9aad7df86536ba03796a8028fbf)]
delete h1 from AUScheduleHistory h1 inner join
(select h2.CompanyID, h2.ScreenID, h2.ScheduleID, max(h2.ExecutionDate) as ExecutionDate
from AUScheduleHistory h2
group by h2.CompanyID, h2.ScreenID, h2.ScheduleID) s
on s.CompanyID = h1.CompanyID and s.ScreenID = h1.ScreenID and s.ScheduleID = h1.ScheduleID and s.ExecutionDate > h1.ExecutionDate
GO

-- Fill schedule execution table if it is empty, but shedule history exists
--[MinVersion(Hash = 2840131da5a0501a441369eb04bc473b96efb317c3ee090f91c88380b789d521)]
IF NOT EXISTS (SELECT TOP 1 * FROM [AUScheduleExecution]) AND EXISTS (SELECT TOP 1 * FROM [AUScheduleHistory])
insert into [AUScheduleExecution] (CompanyID, ScreenID, ScheduleID, ExecutionDate, ProcessedCount, WarningsCount, ErrorsCount, TotalCount)
select CompanyID, ScreenID, ScheduleID, ExecutionDate,
	sum(case when ErrorLevel = 0 or ErrorLevel = 1 then 1 else 0 end) as ProcessedCount,
	sum(case when ErrorLevel = 2 or ErrorLevel = 3 then 1 else 0 end) as WarningsCount,
	sum(case when ErrorLevel = 4 or ErrorLevel = 5 then 1 else 0 end) as ErrorsCount,
	sum(1) as TotalCount
from [AUScheduleHistory]
group by CompanyID, ScreenID, ScheduleID, ExecutionDate
GO

--[IfExists(Column = GINavigationParameter.ScreenID)]
--[SmartExecute]
--[OldHash(Hash = 34e42291f6085cbe27a6baf1d310191a0f0e54689a1bf10b39102f324af63480)]
--[MinVersion(Hash = 34e42291f6085cbe27a6baf1d310191a0f0e54689a1bf10b39102f324af63480)]
delete from GINavigationParameter 
where 
	( select ScreenID
	from GINavigationScreen 
    where DesignID = GINavigationParameter.DesignID and ScreenID = GINavigationParameter.ScreenID ) is null
GO

--[IfExists(Column = GINavigationParameter.ScreenID)]
--[MinVersion(Hash = 2dcd68b315462db59ae9a864fe025a5af91d56eeccefdd0edf244f0d84a1108e)]
delete from GINavigationParameter 
where (CompanyID < 1)
GO

--[IfExists(Column = GINavigationParameter.ScreenID)]
--[SmartExecute]
--[MinVersion(Hash = 90baefa7d132c4f82d77fd93b37dfb516ba7c7a4d8d7d3d75713d21021c4ce86)]
delete from GINavigationParameter from GINavigationParameter as nParam left join  
		GINavigationScreen as nScreen on 
			nParam.DesignID = nScreen.DesignID and 
			nParam.ScreenID = nScreen.ScreenID 
		where nScreen.DesignID IS NULL
GO

--[IfExists(Column = GINavigationParameter.ScreenID)]
--[MinVersion(Hash = 9029cfd59b78c9f44b9fdb0a8e823727529b632e059f317ce41711c306bf42ae)]
declare @counter int;
declare @lineNbr int;
declare @lastModified uniqueidentifier;
declare @CompanyID int, @DesignID uniqueidentifier, @ScreenID varchar(8);

set @counter = 1
set @lastModified = '00000000-0000-0000-0000-000000000000';

declare GINavigationScreen_cursor cursor
for 
	select 
		[CompanyID],
		[DesignID],
		[ScreenID]
	from 
		GINavigationScreen
	order by
		CompanyID,
		DesignID asc,
		case when GINavigationScreen.WindowMode = 'I' then 0 else 100 end,
		ScreenID asc,
		CreatedDateTime,
		LastModifiedDateTime;

open GINavigationScreen_cursor;

fetch next from GINavigationScreen_cursor
into @CompanyID, @DesignID, @ScreenID;

while @@FETCH_STATUS = 0
begin 
	if (@lastModified != @DesignID)
	begin
		set @counter = 1;
		set @lastModified = @DesignID;
	end;
	
	update 
		GINavigationScreen set LineNbr = @counter 
	where
		GINavigationScreen.CompanyID = @CompanyID and
		GINavigationScreen.DesignID = @DesignID and 
		GINavigationScreen.ScreenID = @ScreenID  
	
	set @counter = @counter + 1;

	fetch next from GINavigationScreen_cursor
	into  @CompanyID, @DesignID, @ScreenID;
end;

close GINavigationScreen_cursor;
deallocate GINavigationScreen_cursor;
GO

--[IfExists(Column = GINavigationParameter.ScreenID)]
--[SmartExecute]
--[OldHash(Hash = 34ba29b3d266d89e03c33824608c9574c96aeb779a730b64d0a7a395b55bb140)]
--[OldHash(Hash = 1d94593d1cee7d5aba1131734a981169b46706b2366f5d6f5d2e971fbce2095b)]
--[MinVersion(Hash = a7bead0d814877fd645407d73f349392c81ca18d80e6a162ae3581577e3c1874)]
declare @counter int;
declare @lineNbr int;
declare @lastModified uniqueidentifier, @lastNavigationScreenLineNbr int;
declare @DesignID uniqueidentifier, @NavigationScreenLineNbr int, @ParamID int, @ScreenID varchar(8);
declare @CompanyID int, @CompanyMask int;

set @counter = 1
set @lastModified = '00000000-0000-0000-0000-000000000000';
set @lastNavigationScreenLineNbr = -1;

declare GINavigationParameter_cursor cursor
for 
	select 
		nParam.[DesignID],
		nParam.[ScreenID],
		nParam.[ParamID],
		nScreen.[LineNbr] as NavigationScreenLineNbr,
		nParam.[CompanyID],
		nParam.[CompanyMask]
	from 
		GINavigationParameter as nParam inner join  
		GINavigationScreen as nScreen on 
			nParam.DesignID = nScreen.DesignID and 
			nParam.ScreenID = nScreen.ScreenID
	where
		nParam.LineNbr is null
	order by
		nParam.[DesignID] asc,
		case when nScreen.WindowMode = 'I' then 0 else 100 end,
		nParam.[ScreenID] asc,
		nParam.[CreatedDateTime],
		nParam.[LastModifiedDateTime],
		nParam.[ParamID],
		NavigationScreenLineNbr;

open GINavigationParameter_cursor;

fetch next from GINavigationParameter_cursor
into @DesignID, @ScreenID, @ParamID, @NavigationScreenLineNbr, @CompanyID, @CompanyMask;

while @@FETCH_STATUS = 0
begin 
	if (@lastModified != @DesignID ) or (@lastNavigationScreenLineNbr != @NavigationScreenLineNbr)
	begin
		set @counter = 1;
		set @lastModified = @DesignID;
	end;
	
	update 
		GINavigationParameter set LineNbr = @counter, NavigationScreenLineNbr = @NavigationScreenLineNbr
	where
		GINavigationParameter.DesignID = @DesignID and 
		GINavigationParameter.ScreenID = @ScreenID and
		GINavigationParameter.ParamID = @ParamID and
		GINavigationParameter.LineNbr is null and
		GINavigationParameter.CompanyID = @CompanyID and 
		GINavigationParameter.CompanyMask = @CompanyMask
		
		
	set @lastNavigationScreenLineNbr = @NavigationScreenLineNbr;
	set @counter = @counter + 1;

	fetch next from GINavigationParameter_cursor
	into @DesignID, @ScreenID, @ParamID, @NavigationScreenLineNbr, @CompanyID, @CompanyMask;
end;

close GINavigationParameter_cursor;
deallocate GINavigationParameter_cursor;
GO

--[SmartExecute]
--[MinVersion(Hash = ab87c22cfb7a07cbb835356b2248a512a185dc32010956084451c32c22ec1bdd)]
update GIResult 
set GIResult.NavigationNbr = (select top 1 GINavigationScreen.LineNbr 
	from GINavigationScreen 
	where GINavigationScreen.ScreenID = GIResult.NavigateTo and 
		GINavigationScreen.DesignID = GIResult.DesignID
	order by GINavigationScreen.CreatedDateTime)
GO

-- Move link to user to MobileRegTokenToUserLink
--[IfExists(Column = MobilePushNotificationRegToken.CompanyID)]
--[MinVersion(Hash = ce9e48cc5f55e9bedef8773d0f8a23cc4a0570314365ac9d064283afa5afe0b5)]
INSERT INTO [MobileAccountToUserLink]([ApplicationInstanceID], [AccountID], [CompanyID], [UserID], [Enabled])
SELECT [ApplicationInstanceID], [AccountID], [CompanyID], [UserID], [Enabled]
FROM [MobilePushNotificationRegToken]
GO

--remove duplicates with different companyid
--[IfExists(Column = MobilePushNotificationRegToken.CompanyID)]
--[MinVersion(Hash = 7a3c087b9e2620b9f8fa25371d3a53b085026d1b75fcc5c917e0a113735e8ac6)]
delete rt from MobilePushNotificationRegToken rt 
	inner join
		(select [ApplicationInstanceID], [AccountID], MIN(CompanyID) as companyId
		from MobilePushNotificationRegToken rt2
		group by rt2.ApplicationInstanceID, rt2.AccountID ) t
	on rt.AccountID = t.AccountID and rt.ApplicationInstanceID = t.ApplicationInstanceID and rt.CompanyID<>t.companyId
GO

--remove duplicates with different userid
--[IfExists(Column = MobilePushNotificationRegToken.CompanyID)]
--[MinVersion(Hash = 4a808b6e063272a75bebaddc6ddd217b027f01e069fdbdeda98f66f78c31e446)]
delete rt from MobilePushNotificationRegToken rt 
	inner join
		(select [ApplicationInstanceID], [AccountID], MIN(UserID) as userId
		from MobilePushNotificationRegToken rt2
		group by rt2.ApplicationInstanceID, rt2.AccountID ) t
	on rt.AccountID = t.AccountID and rt.ApplicationInstanceID = t.ApplicationInstanceID and rt.UserID<>t.userId
GO

--[MinVersion(Hash = 129c496fd3073982cd24e598aa40e5894106ee533d4e9825618957e4430585be)]
UPDATE PreferencesEmail
SET TwoFactorNewDeviceNotificationId = CASE WHEN TwoFactorNewDeviceNotificationId IS NULL THEN 290 ELSE TwoFactorNewDeviceNotificationId END,	
	TwoFactorCodeByNotificationId = CASE WHEN TwoFactorCodeByNotificationId IS NULL THEN 291 ELSE TwoFactorCodeByNotificationId END
WHERE TwoFactorNewDeviceNotificationId IS NULL OR TwoFactorCodeByNotificationId IS NULL
GO

--[SmartExecute]
--[MinVersion(Hash = 0720dbb6fb9281bafe0d8505c495f6e40c342908f40e57e7ff8cbad055b75d2c)]
Update graph
set graph.Accessrights = cache.Accessrights
from RolesInGraph graph inner join RolesInCache  cache
on 	graph.ScreenID = cache.ScreenID and 
	graph.Rolename = cache.Rolename and 
	graph.ApplicationName = cache.ApplicationName 
Where graph.Accessrights = -1 AND
	graph.ScreenID IN 
	(Select RolesInCache.ScreenID from RolesInCache
	where RolesInCache.Cachetype = 'PX.Dashboards.DAC.Dashboard' and RolesInCache.Accessrights = 0
	and RolesInCache.CompanyID = graph.CompanyID)
GO

--[SmartExecute]
--[MinVersion(Hash = 1cf539be2401fd475bc96db86a1ade9f2869aede18f292390d4be98afcdc85f3)]
Update RolesInGraph 
set RolesInGraph.Accessrights = 4
Where RolesInGraph.Accessrights = -1 AND
	RolesInGraph.ScreenID IN 
	(Select RolesInCache.ScreenID from RolesInCache
	where RolesInCache.Cachetype = 'PX.Dashboards.DAC.Dashboard' and RolesInCache.Accessrights = 0
	and RolesInCache.CompanyID = RolesInGraph.CompanyID)
GO

--[MinVersion(Hash = 8fc1746de07caae5bba10336af103fd4b50a0b86780f731dd7ee7f5fd820e37e)]
Delete from RolesInCache
where RolesInCache.Cachetype = 'PX.Dashboards.DAC.Dashboard'
GO

--[MinVersion(Hash = c2280871f24f9e65dd3794d8acc7d6a0bbf648aed046e2836984f36b3c1a2de6)]
Delete RolesInGraph
where Accessrights < 0
GO

--[OldHash(Hash = d262f72dc6e94ce4f1480dae377b30ac6941bbc1c2b50692d516708670be18e1)]
--[MinVersion(Hash = 0e7b37436464f6a0d8c0bc67a673a337fa06d45bfb69bc9f99157fb905750947)]
Delete from SiteMap
where (Url is null or Url = '')
and (ScreenID != '00000000' or ScreenID is null) and 
NodeID != '00000000-0000-0000-0000-000000000000'
GO

--[OldHash(Hash = 20a8fb4f21f311b2013fbf41c775bd1d80f0ae45a2f8e74bf573a1152f4060dd)]
--[MinVersion(Hash = 663847009b69cc3fe28f797b4bd76840b72cfbcda1c8ecd2bda7056600daa35a)]
Delete from PortalMap
where (Url is null or Url = '')
and ScreenID != '00000000' and NodeID != '00000000-0000-0000-0000-000000000000'
GO

--[OldHash(Hash = aa18bd2c1300f13158a1f896f95b277511b917e6f287832ddb933daf55ace762)]
--[MinVersion(Hash = 053541953af7e83de739f2323a24e93a6fc99de293106ecbde60d69ea45decfc)]
Delete from PortalISVMap
where (Url is null or Url = '')
and ScreenID != '00000000' and NodeID != '00000000-0000-0000-0000-000000000000'
GO

--[SmartExecute]
--[OldHash(Hash = 9f1af52bdee2e0bbc82d1e737b93c3da9a52db57e60303790afbdcb81487d9bd)]
--[MinVersion(Hash = f9fabe624d978f983d73e19c9feb7df59e883b9d1adefbcbb76866732c1d1ffe)]
Delete from SiteMap
Where ScreenID not in (Select ScreenID from Dashboard Where ScreenID is not null and IsPortal = 0) 
and Url like '%default.aspx%'
and ScreenID != '00000000' and NodeID != '00000000-0000-0000-0000-000000000000'
GO

--[SmartExecute]
--[OldHash(Hash = 7817678ae26bd00896517ea8f71aed55eef64689e2d07fa2f733f2ca8044a93d)]
--[MinVersion(Hash = 509a74799a7834b58d8c44eb0a3b708671a5fb5b7d66f8d850d9a1a8a1f52883)]
Delete from PortalMap
Where ScreenID not in (Select ScreenID from Dashboard Where ScreenID is not null and IsPortal = 1) 
and Url like '%default.aspx%'
and ScreenID != '00000000' and NodeID != '00000000-0000-0000-0000-000000000000'
GO

--[SmartExecute]
--[OldHash(Hash = 7d85b3454c1ac210a70344fdecd71a26efd44322e5278337ffcc75f3f0761c86)]
--[MinVersion(Hash = 00c20d4f6748e07867f4057bdf6cd8d91d3961199a2bffb01c6b3ec7f5c925b6)]
Delete from PortalISVMap
Where ScreenID not in (Select ScreenID from Dashboard Where ScreenID is not null and IsPortal = 1) 
and Url like '%default.aspx%'
and ScreenID != '00000000' and NodeID != '00000000-0000-0000-0000-000000000000'
GO

--[SmartExecute]
--Clear parent id for all sitemaps except in customization project
--[MinVersion(Hash = 1db17d6a73ed7befaa7c93bd431d06e34188e417fc94785fc15b938d2d3b3099)]
Update p0 
Set p0.ParentID = '00000000-0000-0000-0000-000000000000'
from
 SiteMap p0
	left join SiteMap as p1
	on p0.ParentID = p1.NodeID 
	left join SiteMap as p2
	on p1.ParentID = p2.NodeID
	left join SiteMap as p3
	on p2.ParentID = p3.NodeID
	where  p3.ParentID != '9B393502-B104-4B46-B848-EBB342AA7DA9' and 
	p2.ParentID != '9B393502-B104-4B46-B848-EBB342AA7DA9' and 
	p1.ParentID != '9B393502-B104-4B46-B848-EBB342AA7DA9' and 
	p0.ParentID != '9B393502-B104-4B46-B848-EBB342AA7DA9'
GO

--[SmartExecute]
--Clear parent id for all sitemaps except in customization project
--[MinVersion(Hash = 615f01a9d1793aacbd2abb5f3771fc17311e3a61cf8d0536cfa8cd45034cd52a)]
Update p0 
Set p0.ParentID = '00000000-0000-0000-0000-000000000000'
from
 PortalMap p0
	left join PortalMap as p1
	on p0.ParentID = p1.NodeID 
	left join PortalMap as p2
	on p1.ParentID = p2.NodeID
	left join PortalMap as p3
	on p2.ParentID = p3.NodeID
	where  p3.ParentID != '9B393502-B104-4B46-B848-EBB342AA7DA9' and 
	p2.ParentID != '9B393502-B104-4B46-B848-EBB342AA7DA9' and 
	p1.ParentID != '9B393502-B104-4B46-B848-EBB342AA7DA9' and 
	p0.ParentID != '9B393502-B104-4B46-B848-EBB342AA7DA9'
GO

--[SmartExecute]
--Clear parent id for all sitemaps except in customization project
--[MinVersion(Hash = 643a019c58059c8b32adc228224d241e6883f9f49935b5042989d26f0666160c)]
Update p0 
Set p0.ParentID = '00000000-0000-0000-0000-000000000000'
from
 PortalISVMap p0
	left join PortalISVMap as p1
	on p0.ParentID = p1.NodeID 
	left join PortalISVMap as p2
	on p1.ParentID = p2.NodeID
	left join PortalISVMap as p3
	on p2.ParentID = p3.NodeID
	where  p3.ParentID != '9B393502-B104-4B46-B848-EBB342AA7DA9' and 
	p2.ParentID != '9B393502-B104-4B46-B848-EBB342AA7DA9' and 
	p1.ParentID != '9B393502-B104-4B46-B848-EBB342AA7DA9' and 
	p0.ParentID != '9B393502-B104-4B46-B848-EBB342AA7DA9'
GO

--[IfExistsSelect(From = PivotField, WhereIsNull = PivotFieldID)]
--[MinVersion(Hash = 4d8ec3d54581341f9acf25a0f4dadaa68ed13ec8111696b3844a0a121d2cdd7a)]
UPDATE PivotField
SET PivotFieldID = CompanyPivotFieldID
WHERE PivotFieldID IS NULL
GO

--[MinVersion(Hash = 3be7d70997b0c59bf25cf49ddd074ce961d725a9fd691f5d6e72c16fe2db41d0)]
UPDATE RMDataSource
SET UseMasterCalendar = 0
WHERE UseMasterCalendar IS NULL
GO

-- Generate new NoteID value for rows with NULL or duplicates 
--[Method(Method = PX.BulkInsert.Installer.SysUpdates.Notes.GenerateValueForNullOrDuplicateNoteId)]
--[MinVersion(Hash = 4fd68e925d09db3aaa4a6c37466c1c516d9af24a72c187575ad1d2ee20be64d6)]
RAISERROR( N'MM> Generate new NoteID value for rows with NULL or duplicates: this code should not be executed by T-SQL engine. The logics has been moved to a method in DLL as specified by a batch attribute above. Second call.', 15, -1)
GO

-- Make NoteId columns unique and NotNull
--[Method(Method = PX.BulkInsert.Installer.SysUpdates.Notes.MakeNoteIdUniqueAndNotNull)]
--[MinVersion(Hash = 5d111d46a5e8d54fb5eb0a34eeece5d51b6ad68853fba4a59d81259181498e88)]
RAISERROR( N'MM> Make NoteId columns unique and NotNull: this code should not be executed by T-SQL engine. The logics has been moved to a method in DLL as specified by a batch attribute above. Second call.', 15, -1)
GO

--changing ScheduleID for ML Events processing
--[MinVersion(Hash = 4a33758e34824138b7b2b40a25718c19f8bb30d61f96924458ae28bbc1a0a4e1)]
UPDATE AUScheduleHistory
SET ScheduleID = 100000
WHERE ScheduleID = 153 AND CompanyID IN (
	SELECT CompanyID FROM AUSchedule WHERE ScheduleID = 153 AND ScreenID = 'ML501000'
)

UPDATE AUSchedule
SET ScheduleID = 100000
WHERE ScheduleID = 153 AND ScreenID = 'ML501000'
GO

--[mssql: Native]
--[mysql: Skip]
--[IfExists(Column = AUScheduleHistory.CompanyMask)]
--[MinVersion(Hash = 3620d5b57fcef21908abd07bf0d2ad03203b46fcba4194a2af8264f26c4a24cd)]
EXEC pp_DropConstraint 'AUScheduleHistory', 'CompanyMask';
ALTER TABLE AUScheduleHistory
DROP COLUMN CompanyMask
GO

--[mssql: Native]
--[mysql: Skip]
--[IfExists(Column = AUScheduleHistory.UsrCompanyMask)]
--[MinVersion(Hash = 67b70dbf5847333e325322f294167012d58e585906efa0eb11050ab6b444c71d)]
EXEC pp_DropConstraint 'AUScheduleHistory', 'UsrCompanyMask';
ALTER TABLE AUScheduleHistory
DROP column UsrCompanyMask
GO

--[mssql: Skip]
--[mysql: Native]
--[IfExists(Column = AUScheduleHistory.CompanyMask)]
--[MinVersion(Hash = 4a3e968db969f13ba9feb9b06dce697c5f50b8830fac744fddee979a60124cb1)]
ALTER TABLE AUScheduleHistory
DROP COLUMN CompanyMask
GO

--[mssql: Skip]
--[mysql: Native]
--[IfExists(Column = AUScheduleHistory.UsrCompanyMask)]
--[MinVersion(Hash = b8d5a88c1e9516b4c3c69832da4bddc0f80a2e572d95e66add05836390d020d1)]
ALTER TABLE AUScheduleHistory
DROP column UsrCompanyMask
GO

-- Remove invalid Audit configuration for Access Rights by Screen and Access Rights by Role
--[MinVersion(Hash = e25ec80d250209ddb15768c3c520cf262897f7dffa575a3ef3b06850301f9fd0)]
DELETE FROM AUAuditSetup WHERE ScreenID IN ('SM201020', 'SM201025')
DELETE FROM AUAuditTable WHERE ScreenID IN ('SM201020', 'SM201025')
DELETE FROM AUAuditField WHERE ScreenID IN ('SM201020', 'SM201025')
GO

-- Switch PadLeft and PadRight functions in formulas
--[MinVersion(Hash = 8f4e927a1fab61d0dbb132225fb95ae539e463d8099945804b32add84ff694b0)]
UPDATE GIResult SET Field = REPLACE(Field, 'PadLeft(', 'PadRightTEMP(') WHERE Field LIKE '=%' AND Field LIKE '%PadLeft(%'
UPDATE GIResult SET Field = REPLACE(Field, 'PadLeft (', 'PadRightTEMP (') WHERE Field LIKE '=%' AND Field LIKE '%PadLeft (%'
UPDATE GIResult SET Field = REPLACE(Field, 'PadRight(', 'PadLeft(') WHERE Field LIKE '=%' AND Field LIKE '%PadRight(%'
UPDATE GIResult SET Field = REPLACE(Field, 'PadRight (', 'PadLeft (') WHERE Field LIKE '=%' AND Field LIKE '%PadRight (%'
UPDATE GIResult SET Field = REPLACE(Field, 'PadRightTEMP(', 'PadRight(') WHERE Field LIKE '=%' AND Field LIKE '%PadRightTEMP(%'
UPDATE GIResult SET Field = REPLACE(Field, 'PadRightTEMP (', 'PadRight (') WHERE Field LIKE '=%' AND Field LIKE '%PadRightTEMP (%'

UPDATE GISort SET DataFieldName = REPLACE(DataFieldName, 'PadLeft(', 'PadRightTEMP(') WHERE DataFieldName LIKE '=%' AND DataFieldName LIKE '%PadLeft(%'
UPDATE GISort SET DataFieldName = REPLACE(DataFieldName, 'PadLeft (', 'PadRightTEMP (') WHERE DataFieldName LIKE '=%' AND DataFieldName LIKE '%PadLeft (%'
UPDATE GISort SET DataFieldName = REPLACE(DataFieldName, 'PadRight(', 'PadLeft(') WHERE DataFieldName LIKE '=%' AND DataFieldName LIKE '%PadRight(%'
UPDATE GISort SET DataFieldName = REPLACE(DataFieldName, 'PadRight (', 'PadLeft (') WHERE DataFieldName LIKE '=%' AND DataFieldName LIKE '%PadRight (%'
UPDATE GISort SET DataFieldName = REPLACE(DataFieldName, 'PadRightTEMP(', 'PadRight(') WHERE DataFieldName LIKE '=%' AND DataFieldName LIKE '%PadRightTEMP(%'
UPDATE GISort SET DataFieldName = REPLACE(DataFieldName, 'PadRightTEMP (', 'PadRight (') WHERE DataFieldName LIKE '=%' AND DataFieldName LIKE '%PadRightTEMP (%'

UPDATE GIGroupBy SET DataFieldName = REPLACE(DataFieldName, 'PadLeft(', 'PadRightTEMP(') WHERE DataFieldName LIKE '=%' AND DataFieldName LIKE '%PadLeft(%'
UPDATE GIGroupBy SET DataFieldName = REPLACE(DataFieldName, 'PadLeft (', 'PadRightTEMP (') WHERE DataFieldName LIKE '=%' AND DataFieldName LIKE '%PadLeft (%'
UPDATE GIGroupBy SET DataFieldName = REPLACE(DataFieldName, 'PadRight(', 'PadLeft(') WHERE DataFieldName LIKE '=%' AND DataFieldName LIKE '%PadRight(%'
UPDATE GIGroupBy SET DataFieldName = REPLACE(DataFieldName, 'PadRight (', 'PadLeft (') WHERE DataFieldName LIKE '=%' AND DataFieldName LIKE '%PadRight (%'
UPDATE GIGroupBy SET DataFieldName = REPLACE(DataFieldName, 'PadRightTEMP(', 'PadRight(') WHERE DataFieldName LIKE '=%' AND DataFieldName LIKE '%PadRightTEMP(%'
UPDATE GIGroupBy SET DataFieldName = REPLACE(DataFieldName, 'PadRightTEMP (', 'PadRight (') WHERE DataFieldName LIKE '=%' AND DataFieldName LIKE '%PadRightTEMP (%'

UPDATE GIWhere SET Value1 = REPLACE(Value1, 'PadLeft(', 'PadRightTEMP(') WHERE Value1 LIKE '=%' AND Value1 LIKE '%PadLeft(%'
UPDATE GIWhere SET Value1 = REPLACE(Value1, 'PadLeft (', 'PadRightTEMP (') WHERE Value1 LIKE '=%' AND Value1 LIKE '%PadLeft (%'
UPDATE GIWhere SET Value1 = REPLACE(Value1, 'PadRight(', 'PadLeft(') WHERE Value1 LIKE '=%' AND Value1 LIKE '%PadRight(%'
UPDATE GIWhere SET Value1 = REPLACE(Value1, 'PadRight (', 'PadLeft (') WHERE Value1 LIKE '=%' AND Value1 LIKE '%PadRight (%'
UPDATE GIWhere SET Value1 = REPLACE(Value1, 'PadRightTEMP(', 'PadRight(') WHERE Value1 LIKE '=%' AND Value1 LIKE '%PadRightTEMP(%'
UPDATE GIWhere SET Value1 = REPLACE(Value1, 'PadRightTEMP (', 'PadRight (') WHERE Value1 LIKE '=%' AND Value1 LIKE '%PadRightTEMP (%'

UPDATE GIWhere SET Value2 = REPLACE(Value2, 'PadLeft(', 'PadRightTEMP(') WHERE Value2 LIKE '=%' AND Value2 LIKE '%PadLeft(%'
UPDATE GIWhere SET Value2 = REPLACE(Value2, 'PadLeft (', 'PadRightTEMP (') WHERE Value2 LIKE '=%' AND Value2 LIKE '%PadLeft (%'
UPDATE GIWhere SET Value2 = REPLACE(Value2, 'PadRight(', 'PadLeft(') WHERE Value2 LIKE '=%' AND Value2 LIKE '%PadRight(%'
UPDATE GIWhere SET Value2 = REPLACE(Value2, 'PadRight (', 'PadLeft (') WHERE Value2 LIKE '=%' AND Value2 LIKE '%PadRight (%'
UPDATE GIWhere SET Value2 = REPLACE(Value2, 'PadRightTEMP(', 'PadRight(') WHERE Value2 LIKE '=%' AND Value2 LIKE '%PadRightTEMP(%'
UPDATE GIWhere SET Value2 = REPLACE(Value2, 'PadRightTEMP (', 'PadRight (') WHERE Value2 LIKE '=%' AND Value2 LIKE '%PadRightTEMP (%'
GO

-- Change graph type for Access Rights by Screen and Access Rights by Role screens
-- AUSchedule and AUStep also store graph name, but they cannot be configured for these screens
--[MinVersion(Hash = 4edb2b23bc54945a1d286798d18cf8ec1c2b30644bf350f74383b00f91c5bf55)]
UPDATE SYMapping SET GraphName = 'PX.SiteMap.Graph.ModernAccess' WHERE ScreenID IN ('SM201020', 'SM201025')
UPDATE AUNotification SET GraphName = 'PX.SiteMap.Graph.ModernAccess' WHERE ScreenID IN ('SM201020', 'SM201025')
GO

--[SmartExecute]
--[MinVersion(Hash = 893d03f88d88b36c9f2d83a401945a6390e443047e347dcebee304c92e4e0cac)]
INSERT INTO PreferencesGlobal(SingleRowKey, EnableTelemetry) VALUES(0, 1)
GO

--[MinVersion(Hash = 805dceba9eeeb0e275fe5407d0231ef97b31a50a077c5e312e0ce7eb8faf2951)]
UPDATE GITable SET Name = 'PX.EP.EPLoginType' WHERE Name = 'PX.Objects.EP.EPLoginType'
GO

--[MinVersion(Hash = 076d4ebca90168bd3a9076ee9e90600d7177f4117dcc429eb87815ef8acbfa4e)]
UPDATE SiteMap
SET ScreenID = '00000000'
WHERE NodeID = '00000000-0000-0000-0000-000000000000'
GO

--[MinVersion(Hash = d2a810f2e2fbfa29fd0924882b2ff91a81bebe71468bfb232f85ac06ff7b082b)]
DELETE FROM MUIScreen
WHERE WorkspaceID = 'FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF'
GO

--[MinVersion(Hash = 6594617fab240c0afa68d519b918e0676763535e418ca437c73f8563a54fd2eb)]
DELETE FROM MUIWorkspace 
WHERE WorkspaceID = 'FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF'
GO

--[MinVersion(Hash = eff9dcc981409b139160618044e6b2210efed44a634afffe17a2b8b2babe3d34)]
DELETE FROM SiteMap
WHERE Title = 'User Favorites'
GO

--[SmartExecute]
-- for GETUTCDATE() because it is different in MySQL
--[MinVersion(Hash = f016729fb8eb207a2bbd3ad472a9e1e5a7cf9e9d748beb169973d102dec8ba7e)]
UPDATE 
	MUIScreen 
SET 
	CreatedByID = 'B5344897-037E-4D58-B5C3-1BDFD0F47BF9',
	CreatedDateTime = GETUTCDATE(),
	CreatedByScreenID = '00000000', 
	LastModifiedByID = 'B5344897-037E-4D58-B5C3-1BDFD0F47BF9',
	LastModifiedDateTime = GETUTCDATE(),
	LastModifiedByScreenID = '00000000'

UPDATE 
	MUIWorkspace
SET 
	CreatedByID = 'B5344897-037E-4D58-B5C3-1BDFD0F47BF9',
	CreatedDateTime = GETUTCDATE(),
	CreatedByScreenID = '00000000', 
	LastModifiedByID = 'B5344897-037E-4D58-B5C3-1BDFD0F47BF9',
	LastModifiedDateTime = GETUTCDATE(),
	LastModifiedByScreenID = '00000000'

UPDATE 
	MUISubcategory
SET 
	CreatedByID = 'B5344897-037E-4D58-B5C3-1BDFD0F47BF9',
	CreatedDateTime = GETUTCDATE(),
	CreatedByScreenID = '00000000', 
	LastModifiedByID = 'B5344897-037E-4D58-B5C3-1BDFD0F47BF9',
	LastModifiedDateTime = GETUTCDATE(),
	LastModifiedByScreenID = '00000000'


UPDATE 
	MUIArea
SET 
	CreatedByID = 'B5344897-037E-4D58-B5C3-1BDFD0F47BF9',
	CreatedDateTime = GETUTCDATE(),
	CreatedByScreenID = '00000000', 
	LastModifiedByID = 'B5344897-037E-4D58-B5C3-1BDFD0F47BF9',
	LastModifiedDateTime = GETUTCDATE(),
	LastModifiedByScreenID = '00000000'
	
UPDATE 
	MUIFavoriteScreen
SET 
	CreatedByID = 'B5344897-037E-4D58-B5C3-1BDFD0F47BF9',
	CreatedDateTime = GETUTCDATE(),
	CreatedByScreenID = '00000000', 
	LastModifiedByID = 'B5344897-037E-4D58-B5C3-1BDFD0F47BF9',
	LastModifiedDateTime = GETUTCDATE(),
	LastModifiedByScreenID = '00000000'
	
UPDATE 
	MUIFavoriteTile
SET 
	CreatedByID = 'B5344897-037E-4D58-B5C3-1BDFD0F47BF9',
	CreatedDateTime = GETUTCDATE(),
	CreatedByScreenID = '00000000', 
	LastModifiedByID = 'B5344897-037E-4D58-B5C3-1BDFD0F47BF9',
	LastModifiedDateTime = GETUTCDATE(),
	LastModifiedByScreenID = '00000000'

UPDATE 
	MUIPinnedScreen
SET 
	CreatedByID = 'B5344897-037E-4D58-B5C3-1BDFD0F47BF9',
	CreatedDateTime = GETUTCDATE(),
	CreatedByScreenID = '00000000', 
	LastModifiedByID = 'B5344897-037E-4D58-B5C3-1BDFD0F47BF9',
	LastModifiedDateTime = GETUTCDATE(),
	LastModifiedByScreenID = '00000000'
	
UPDATE 
	MUIFavoriteWorkspace
SET 
	CreatedByID = 'B5344897-037E-4D58-B5C3-1BDFD0F47BF9',
	CreatedDateTime = GETUTCDATE(),
	CreatedByScreenID = '00000000', 
	LastModifiedByID = 'B5344897-037E-4D58-B5C3-1BDFD0F47BF9',
	LastModifiedDateTime = GETUTCDATE(),
	LastModifiedByScreenID = '00000000'
	
UPDATE 
	MUIUserPreferences
SET 
	CreatedByID = 'B5344897-037E-4D58-B5C3-1BDFD0F47BF9',
	CreatedDateTime = GETUTCDATE(),
	CreatedByScreenID = '00000000', 
	LastModifiedByID = 'B5344897-037E-4D58-B5C3-1BDFD0F47BF9',
	LastModifiedDateTime = GETUTCDATE(),
	LastModifiedByScreenID = '00000000'

UPDATE 
	MUITile
SET 
	CreatedByID = 'B5344897-037E-4D58-B5C3-1BDFD0F47BF9',
	CreatedDateTime = GETUTCDATE(),
	CreatedByScreenID = '00000000', 
	LastModifiedByID = 'B5344897-037E-4D58-B5C3-1BDFD0F47BF9',
	LastModifiedDateTime = GETUTCDATE(),
	LastModifiedByScreenID = '00000000'
GO

--[IfExists(Column = PivotField.ShowPercent)]
--[IfExists(Column = PivotField.CalculationType)]
--[MinVersion(Hash = ed0ebd2138154975d0c45d8e77ba8568cc6db8920e9cd0827d616486b8a8e51b)]
UPDATE PivotField SET CalculationType = 1 WHERE ShowPercent = 1
GO

-- Change sequence value for records with the same sequence value
--[MinVersion(Hash = e38fed4ac8741f4627b72ed076e9c161c44c79c389a0cbd4255871e10633adec)]
declare @maxSequence int
declare @companyID int
declare @prevCompanyID int
declare @sequence int
set @maxSequence = (select ISNULL(max([Sequence]), 0) from Company)

declare company_cursor cursor for
select c1.Sequence, c1.CompanyID from Company c1
inner join Company c2 on c1.Sequence = c2.Sequence
where c1.CompanyID > 0 and c2.CompanyID > 0

open company_cursor
fetch next from company_cursor into @sequence, @companyID
while @@FETCH_STATUS = 0
begin
	if @prevCompanyID != @companyID
	begin
		set @maxSequence = @maxSequence + 1
		set @sequence = @maxSequence
		update Company set Sequence = @sequence where CompanyID = @companyID
	end
	set @prevCompanyID = @companyID
	fetch next from company_cursor into @sequence, @companyID
end
close company_cursor
deallocate company_cursor
GO

-- Update UploadFile table - set PrimaryScreenID for files downloaded from mobile
--[mssql: Native]
--[mysql: Skip]
--[MinVersion(Hash = 0e6233735f9e5570d868ae463d5f9acac7608d97da871012ecc4f1bb2b63d61c)]
DECLARE @table_name NVARCHAR(200);
DECLARE c_reader CURSOR 
	FOR
		SELECT DISTINCT t.name 
		FROM sys.tables t
		INNER JOIN sys.columns c
			ON t.object_id = c.object_id
		INNER JOIN sys.columns c1
			ON t.object_id = c1.object_id
		WHERE c.name LIKE 'CreatedByScreenID'
		AND c1.name LIKE 'NoteID'

OPEN c_reader
FETCH NEXT FROM c_reader INTO @table_name
WHILE @@FETCH_STATUS =0
BEGIN
	BEGIN TRY
		DECLARE @statement NVARCHAR(1000);
		SET @statement = 'UPDATE UploadFile 
		SET PrimaryScreenID=e.CreatedByScreenID 
		FROM UploadFile u 
		INNER JOIN NoteDoc n 
			ON n.FileID = u.FileID
		INNER JOIN ['+@table_name+'] e 
			ON e.NoteId = n.NoteID 
		WHERE u.PrimaryScreenID IS NULL 
		AND u.PrimaryPageID IS NULL'
		EXEC sp_executesql  @statement
	END TRY
	BEGIN CATCH
	END CATCH	
	FETCH NEXT FROM c_reader INTO @table_name
END

CLOSE c_reader
DEALLOCATE c_reader
GO

-- Update UploadFile table - set PrimaryScreenID for files downloaded from mobile
--[mssql: Skip]
--[mysql: Native]
--[MinVersion(Hash = 57f125cca030a3e0d0c9943bf83970fabd48f2d53839f222ac4bf41a7194db81)]
DROP PROCEDURE IF EXISTS updateMobileFileUpload;
GO

--[mssql: Skip]
--[mysql: Native]
--[MinVersion(Hash = 7d37f28d2b169c1ff60e7084e00f5ed941a3d5781f750ec0a99e424ce83b8b48)]
CREATE PROCEDURE updateMobileFileUpload ()
BEGIN
	DECLARE schema_ nvarchar(100) DEFAULT DATABASE();	
	DECLARE tableName nvarchar(100);
	DECLARE finished INTEGER DEFAULT 0;
	DECLARE hasError INTEGER DEFAULT 0;
	DECLARE c_reader CURSOR FOR 
		SELECT DISTINCT t.table_name 
		FROM information_schema.tables t
		INNER JOIN information_schema.columns c 
			ON t.table_name = c.table_name
		INNER JOIN information_schema.columns c1 
			ON t.table_name = c1.table_name
		WHERE t.table_schema = schema_ 
		AND c.table_schema = schema_ 
		AND c1.table_schema = schema_ 
		AND c.column_name like 'CreatedByScreenID' 
		AND c1.column_name like 'NoteID';

	DECLARE CONTINUE HANDLER 
        FOR NOT FOUND SET finished = 1;
	DECLARE CONTINUE HANDLER FOR SQLEXCEPTION 
		SET hasError = 1;
                        	
	OPEN c_reader;

	tableLoop: LOOP
        FETCH c_reader INTO tableName;
        IF finished = 1 THEN 
            LEAVE tableLoop;
        END IF;		
		SET @statement = CONCAT(
		'UPDATE UploadFile u, 
				(SELECT n.FileID, s.CreatedByScreenID 
					FROM NoteDoc n
					INNER JOIN ', tableName,' s 
						ON s.NoteID = n.NoteID) e
		SET PrimaryScreenID = e.CreatedByScreenID 
		WHERE PrimaryScreenID IS NULL 
		AND PrimaryPageID IS NULL
		AND u.FileID = e.FileID');
		PREPARE stmt FROM @statement;	
		EXECUTE stmt;
		DEALLOCATE PREPARE stmt;    	
   	END LOOP;
 	CLOSE c_reader;
END
GO

--[mssql: Skip]
--[mysql: Native]
--[MinVersion(Hash = 9cc9f5143aa5e9e9fce17647a7d00362e3ba42d6dfbe6a8c0489db91222bb5b1)]
CALL updateMobileFileUpload;
GO

--[mssql: Skip]
--[mysql: Native]
--[MinVersion(Hash = 57f125cca030a3e0d0c9943bf83970fabd48f2d53839f222ac4bf41a7194db81)]
DROP PROCEDURE IF EXISTS updateMobileFileUpload;
GO

--[MinVersion(Hash = 78da39de8257af6e07fa13c427e5c5991894c319ab0f80a1f5523e70d23fa2c4)]
delete from SiteMap
Where ScreenID = 'AU201040' AND Title = 'Related Screens'
GO

--[MinVersion(Hash = fed4e4876b0a584fa0fa1d090ddafbbef58b505f9518cf240dae19e87d07794f)]
update RMColumn 
set CellEvalOrder = 1 where ColumnType = 1
GO

--[MinVersion(Hash = 43214ab51147e5636df067920aa00a8db44cc3fa702a92674148363965f9a43e)]
update RMDataSource set StartSub = null where StartSub = ''
update RMDataSource set EndSub = null where EndSub = ''
GO

--[SmartExecute]
--[MinVersion(Hash = 1a3a1e7af3d4374af0058a003728eb27e1611fd733ea05460bbdf8052d861422)]
update d 
set d.PrimaryScreenIDNew = null 
from GIDesign d inner join SiteMap sm on sm.Url like concat('%GenericInquiry.aspx?id=', convert(nvarchar(50), d.DesignID), '%') or sm.Url like concat('%GenericInquiry.aspx?name=', d.Name, '%')
where d.PrimaryScreenIDNew = sm.ScreenID
GO

-- Merge two SiteMap nodes into one (CR302000 Contacts)
--[SmartExecute]
--[IfExists(Table = WZScenario)]
--[MinVersion(Hash = be1526a38eae1dbdf4dedef674db615c2675d855534b781510011e2bc87b9abe)]
update WZScenario set NodeID = '298C4862-6999-4AE0-AA54-1223A143D1E2' where NodeID = '893B8666-2FAA-4347-AF60-C30501D110DC'
GO

--[SmartExecute]
--[MinVersion(Hash = b519704263fe407130dafc25478f15bd1dca5b8db20e39bed999182e070bff2b)]
update MUIScreen set NodeID = newid(), LastModifiedByID = '298C4862-6999-4AE0-AA54-1223A143D1E2' where NodeID = '893B8666-2FAA-4347-AF60-C30501D110DC'
delete s from MUIScreen s inner join MUIScreen s2 on s.IsPortal = s2.IsPortal and s.NodeID = s2.LastModifiedByID and s.WorkspaceID = s2.WorkspaceID where s.NodeID = '298C4862-6999-4AE0-AA54-1223A143D1E2'
update MUIScreen set NodeID = '298C4862-6999-4AE0-AA54-1223A143D1E2', LastModifiedByID = '00000000-0000-0000-0000-000000000000' where LastModifiedByID = '298C4862-6999-4AE0-AA54-1223A143D1E2'

update MUIFavoriteScreen set NodeID = newid(), LastModifiedByID = '298C4862-6999-4AE0-AA54-1223A143D1E2' where NodeID = '893B8666-2FAA-4347-AF60-C30501D110DC'
delete s from MUIFavoriteScreen s inner join MUIFavoriteScreen s2 on s.IsPortal = s2.IsPortal and s.Username = s2.Username and s.NodeID = s2.LastModifiedByID where s.NodeID = '298C4862-6999-4AE0-AA54-1223A143D1E2'
update MUIFavoriteScreen set NodeID = '298C4862-6999-4AE0-AA54-1223A143D1E2', LastModifiedByID = '00000000-0000-0000-0000-000000000000' where LastModifiedByID = '298C4862-6999-4AE0-AA54-1223A143D1E2'

update MUIPinnedScreen set NodeID = newid(), LastModifiedByID = '298C4862-6999-4AE0-AA54-1223A143D1E2' where NodeID = '893B8666-2FAA-4347-AF60-C30501D110DC'
delete s from MUIPinnedScreen s inner join MUIPinnedScreen s2 on s.IsPortal = s2.IsPortal and s.Username = s2.Username and s.WorkspaceID = s2.WorkspaceID and s.NodeID = s2.LastModifiedByID where s.NodeID = '298C4862-6999-4AE0-AA54-1223A143D1E2'
update MUIPinnedScreen set NodeID = '298C4862-6999-4AE0-AA54-1223A143D1E2', LastModifiedByID = '00000000-0000-0000-0000-000000000000' where LastModifiedByID = '298C4862-6999-4AE0-AA54-1223A143D1E2'

delete from MUIUserScreenRelevance where NodeID = '893B8666-2FAA-4347-AF60-C30501D110DC'
delete from SiteMap where NodeID = '893B8666-2FAA-4347-AF60-C30501D110DC'
GO



--[RefreshMetadata()]
GO

--[MinVersion(Hash = c9266ad5b605aa75fcc94c4b698f6a2aa4cca041209e736093cca27846d9ffd1)]
if exists(select * from sys.views where name = 'APAROrd') 
drop view [dbo].[APAROrd]
GO

--[MinVersion(Hash = 49331b02ccbbdc33e9595b6e6375369828c80daace41fa408a2708b589e197f1)]
create view [dbo].[APAROrd] as
select ord = convert(smallint, 0) union all select convert(smallint,1)
GO

--[MinVersion(Hash = b6c7e57fb6626237c76c691f1948ba6a09eb62a7a36e96450baa42320c3fc244)]
IF (EXISTS(SELECT 1 FROM sys.columns WHERE object_id = object_id('FeaturesSet') AND name = 'CaseManagement' AND is_nullable = 1))
BEGIN
EXEC sp_executesql N'UPDATE FeaturesSet SET CaseManagement = CustomerModule'
END
GO

--[IfExists(Table = CROpportunityRevision)]
--[IfExists(Column = CROpportunityProducts.CROpportunityID)]
--[IfNotExistsSelect(From = CROpportunityRevision)]
--[IfExistsSelect(From = CROpportunity)]
--[MinVersion(Hash = 81fea54cc5792c3d132f503a8e0f88a13a7aa6523329e35dbe99b1e848fb2983)]
UPDATE CROpportunity SET ClassID = CROpportunityClassID, Subject = OpportunityName, Details = Description
UPDATE CROpportunityProducts 
SET LineNbr = CROpportunityProductID, Descr = TransactionDescription,
    GroupDiscountRate    = COALESCE(GroupDiscountRate,1),
    DocumentDiscountRate = COALESCE(DocumentDiscountRate,1)

UPDATE CROpportunityTax SET LineNbr = OpportunityProductID
GO

--[mysql: Skip]
--[MinVersion(Hash = 6ba4e3c94215d1c43bc20630b5a1991dc29962912fc984882eeba98cbf736163)]
CREATE TABLE #OppNames
(
	 RecordID int NOT NULL IDENTITY (1, 1),
	 TableName nvarchar(64) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,		
  	 OldField nvarchar(64) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
   	 NewField nvarchar(64) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	 Primary Key (RecordID)
)

INSERT INTO #OppNames
(
	TableName,
	OldField,
	NewField
)
VALUES
('PX.Objects.CR.CROpportunity', 'OpportunityName',		'Subject'),
('PX.Objects.CR.CROpportunity', 'Description',			'Details'),
('PX.Objects.CR.CROpportunity', 'CROpportunityClassID', 'ClassID')

UPDATE
	r
SET
	Field = LOWER(LEFT(n.NewField, 1)) + SUBSTRING(n.NewField, 2, LEN(n.NewField))
FROM
	GIResult r
INNER JOIN GITable t
	ON t.DesignID = r.DesignID
	AND t.CompanyID = r.CompanyID
	AND r.ObjectName = t.Alias
INNER JOIN #OppNames n
	ON LOWER(r.Field) = LOWER(n.OldField)
	AND t.Name = n.TableName
WHERE
	r.CompanyID <> 1

UPDATE r	
SET 
	Field = REPLACE(Field, t.Alias + '.' + n.OldField, t.Alias + '.' + n.NewField)
FROM 
	GIResult r
INNER JOIN GITable t 
	ON r.ObjectName = t.Alias 
	AND r.CompanyID = t.CompanyID 
	AND r.DesignID = t.DesignID
INNER JOIN #OppNames n 
	ON CHARINDEX( LOWER(n.TableName), LOWER(t.[Name]) ) > 0 
	AND CHARINDEX( LOWER(t.Alias) + '.' + LOWER(n.OldField), LOWER(r.Field) ) > 0						  
WHERE 
	r.CompanyID <> 1	

UPDATE fr
SET 
	DataField = REPLACE(fr.DataField, n.OldField, LOWER(LEFT(n.NewField, 1)) + SUBSTRING(n.NewField, 2, LEN(n.NewField)))
FROM 
	FilterRow fr
INNER JOIN #OppNames n 
	ON CHARINDEX(LOWER(fr.[DataField]), LOWER(n.TableName) + '_' + n.OldField) > 0
where
	fr.CompanyID <> 1	

UPDATE
	np
SET
	FieldName = n.NewField,
	ParameterName = t.Alias + '.' + n.NewField
FROM
	GINavigationParameter np
INNER JOIN GITable t
	ON t.DesignID = np.DesignID
	AND t.CompanyID = np.CompanyID
	AND CHARINDEX(LOWER(t.Alias), LOWER(np.ParameterName)) > 0
INNER JOIN #OppNames n
	ON LOWER(np.ParameterName) = LOWER(t.Alias + '.' + n.OldField)
	AND t.Name = n.TableName
WHERE
	np.CompanyID <> 1


UPDATE 
	s
SET 
	DataFieldName = t.Alias + '.' + LOWER(LEFT(n.NewField, 1)) + SUBSTRING(n.NewField, 2, LEN(n.NewField))
FROM 
	GISort s
INNER JOIN GITable t
	ON t.DesignID = s.DesignID
	AND t.CompanyID = s.CompanyID
	AND CHARINDEX(LOWER(t.Alias), LOWER(s.DataFieldName)) > 0
INNER JOIN #OppNames n
	ON LOWER(s.DataFieldName) = LOWER(t.Alias + '.' + n.OldField)
	AND t.Name = n.TableName
WHERE 
	s.CompanyID <> 1

UPDATE
	g
SET
	DataFieldName = t.Alias + '.' + n.NewField
	FROM 
	GIGroupBy g
INNER JOIN GITable t
	ON t.DesignID = g.DesignID
	AND t.CompanyID = g.CompanyID
	AND CHARINDEX(LOWER(t.Alias), LOWER(g.DataFieldName)) > 0
INNER JOIN #OppNames n
	ON LOWER(g.DataFieldName) = LOWER(t.Alias + '.' + n.OldField)
	AND t.Name = n.TableName
	WHERE 
	g.CompanyID <> 1


UPDATE
	f
SET
	FieldName = t.Alias + '.' + LOWER(LEFT(n.NewField, 1)) + SUBSTRING(n.NewField, 2, LEN(n.NewField)),
	[Name] = n.NewField
	FROM 
	GIFilter f
INNER JOIN GITable t
	ON t.DesignID = f.DesignID
	AND t.CompanyID = f.CompanyID
	AND CHARINDEX(LOWER(t.Alias), LOWER(f.FieldName)) > 0
INNER JOIN #OppNames n
	ON LOWER(f.FieldName) = LOWER(t.Alias + '.' + n.OldField)
	AND n.TableName = t.Name
	WHERE 
	f.CompanyID <> 1
		
UPDATE
	o
		SET
	ParentField = LOWER(LEFT(n.NewField, 1)) + SUBSTRING(n.NewField, 2, LEN(n.NewField))
		FROM 
	GIOn o
INNER JOIN GIRelation r
	ON r.DesignID = o.DesignID
	AND r.CompanyID = o.CompanyID
	AND r.LineNbr = o.RelationNbr
INNER JOIN GITable pt
	ON pt.DesignID = r.DesignID
	AND pt.CompanyID = r.CompanyID
	AND pt.Alias = r.ParentTable
INNER JOIN #OppNames n
	ON LOWER(n.OldField) = LOWER(o.ParentField)
	AND n.TableName = pt.Name
WHERE
	o.CompanyID <> 1
	
	UPDATE
	o
	SET
	ChildField = LOWER(LEFT(n.NewField, 1)) + SUBSTRING(n.NewField, 2, LEN(n.NewField))
	FROM
	GIOn o
INNER JOIN GIRelation r
	ON r.DesignID = o.DesignID
	AND r.CompanyID = o.CompanyID
	AND r.LineNbr = o.RelationNbr
INNER JOIN GITable ct
	ON ct.DesignID = r.DesignID
	AND ct.CompanyID = r.CompanyID
	AND ct.Alias = r.ChildTable
INNER JOIN #OppNames n
	ON LOWER(n.OldField) = LOWER(o.ChildField)
	AND n.TableName = ct.Name
	WHERE
	o.CompanyID <> 1
		
	UPDATE 
	w
	SET 
	DataFieldName = t.Alias + '.' + LOWER(LEFT(n.NewField, 1)) + SUBSTRING(n.NewField, 2, LEN(n.NewField))
	FROM 
	GIWhere w
INNER JOIN GITable t
	ON t.DesignID = w.DesignID
	AND t.CompanyID = w.CompanyID
	AND CHARINDEX(LOWER(t.Alias), LOWER(w.DataFieldName)) > 0
INNER JOIN #OppNames n
	ON LOWER(w.DataFieldName) = LOWER(t.Alias + '.' + n.OldField)
	AND n.TableName = t.Name
	WHERE 
	w.CompanyID <> 1

DROP TABLE #OppNames
GO

--[mysql: Skip]
--[MinVersion(Hash = 019d9d3e06e4fb78177446c1f8af743100cebf06aca4c51eaf59e0aaf74e43c8)]
CREATE TABLE #OppNames
(
	 RecordID int NOT NULL IDENTITY (1, 1),
	 TableName nvarchar(64) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,		
  	 OldField nvarchar(64) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
   	 NewField nvarchar(64) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	 Primary Key (RecordID)
	)

INSERT INTO #OppNames
(
	TableName,
	OldField,
	NewField
)
VALUES
('PX.Objects.CR.CROpportunity', 'OpportunityName',		'Subject'),
('PX.Objects.CR.CROpportunity', 'Description',			'Details'),
('PX.Objects.CR.CROpportunity', 'CROpportunityClassID', 'ClassID')

	UPDATE 
	rc
	SET 
	FieldName = REPLACE(FieldName, n.OldField, n.NewField)
	FROM 
	[EPRuleCondition] rc
INNER JOIN #OppNames n
	ON LOWER(rc.FieldName) = LOWER(n.OldField)
	AND rc.Entity = n.TableName
WHERE
	rc.CompanyID <> 1

DROP TABLE #OppNames
GO

--[mysql: Native]
--[mssql: Skip]
--[MinVersion(Hash = 5c102e07878611f04429eaa188410d89507329ced4888a8ce441bdf56b40894d)]
CREATE TEMPORARY TABLE OppNames
(
	 RecordID INT NOT NULL AUTO_INCREMENT, 
	 TableName varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,		
  	 OldField varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
   	 NewField varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
     Primary Key (RecordID)
);
	
INSERT INTO OppNames
(
	TableName,
	OldField,
	NewField
)
VALUES
('PX.Objects.CR.CROpportunity', 'OpportunityName',		'Subject'),
('PX.Objects.CR.CROpportunity', 'Description',			'Details'),
('PX.Objects.CR.CROpportunity', 'CROpportunityClassID', 'ClassID');
	
	UPDATE 
	GIResult r
INNER JOIN GITable t
	ON t.DesignID = r.DesignID
	AND t.CompanyID = r.CompanyID
	AND r.ObjectName = t.Alias
INNER JOIN OppNames n
	ON LOWER(r.Field) = LOWER(n.OldField)
	AND t.Name = n.TableName
	SET 
	Field = CONCAT(LOWER(LEFT(n.NewField, 1)), SUBSTRING(n.NewField, 2))
WHERE
	r.CompanyID <> 1;
	
	UPDATE 
	GIResult r	
INNER JOIN GITable t 
	ON r.ObjectName = t.Alias 
	AND r.CompanyID = t.CompanyID 
	AND r.DesignID = t.DesignID
INNER JOIN OppNames n 
	ON INSTR(LOWER(r.Field), CONCAT(LOWER(t.Alias), '.', LOWER(n.OldField))) > 0 
	AND INSTR(LOWER(t.Name), LOWER(n.TableName)) > 0
	SET 
	r.Field = REPLACE(r.Field, CONCAT(t.Alias, '.', n.OldField), CONCAT(t.Alias, '.', n.NewField))
WHERE 
	r.CompanyID <> 1;
	
	UPDATE 
	FilterRow fr
INNER JOIN OppNames n 
	ON INSTR( CONCAT(LOWER(n.TableName), '_', n.OldField), LOWER(fr.DataField) ) > 0
	SET 
	fr.DataField = REPLACE(fr.DataField, n.OldField, CONCAT(LOWER(LEFT(n.NewField, 1)), SUBSTRING(n.NewField, 2)))
where 
	fr.CompanyID <> 1;
	
	UPDATE
	GINavigationParameter np
INNER JOIN GITable t
	ON t.DesignID = np.DesignID
	AND t.CompanyID = np.CompanyID
	AND INSTR(LOWER(t.Alias), LOWER(np.ParameterName)) > 0
INNER JOIN OppNames n
	ON LOWER(np.ParameterName) = LOWER(CONCAT(t.Alias, '.', n.OldField))
	AND t.Name = n.TableName
	SET 
	FieldName = n.NewField,
	ParameterName = CONCAT(t.Alias, '.', n.NewField)
WHERE
	np.CompanyID <> 1;
	
	UPDATE 
	GISort s
INNER JOIN GITable t
	ON t.DesignID = s.DesignID
	AND t.CompanyID = s.CompanyID
	AND INSTR(LOWER(t.Alias), LOWER(s.DataFieldName)) > 0
INNER JOIN OppNames n
	ON LOWER(s.DataFieldName) = LOWER(CONCAT(t.Alias, '.', n.OldField))
	AND t.Name = n.TableName
	SET 
	DataFieldName = CONCAT(t.Alias, '.', LOWER(LEFT(n.NewField, 1)), SUBSTRING(n.NewField, 2))
WHERE
	s.CompanyID <> 1;
		
	UPDATE 
	GIGroupBy g
INNER JOIN GITable t
	ON t.DesignID = g.DesignID
	AND t.CompanyID = g.CompanyID
	AND INSTR(LOWER(t.Alias), LOWER(g.DataFieldName)) > 0
INNER JOIN OppNames n
	ON LOWER(g.DataFieldName) = LOWER(CONCAT(t.Alias, '.', n.OldField))
	AND t.Name = n.TableName
	SET 
	DataFieldName = CONCAT(t.Alias, '.', n.NewField)
	WHERE
	g.CompanyID <> 1;
	
	
	UPDATE 
	GIFilter f
INNER JOIN GITable t
	ON t.DesignID = f.DesignID
	AND t.CompanyID = f.CompanyID
	AND INSTR(LOWER(t.Alias), LOWER(f.FieldName)) > 0
INNER JOIN OppNames n
	ON LOWER(f.FieldName) = LOWER(CONCAT(t.Alias, '.', n.OldField))
	AND n.TableName = t.Name
	SET
	f.FieldName = CONCAT(t.Alias, '.', LOWER(LEFT(n.NewField, 1)), SUBSTRING(n.NewField, 2)),
	f.Name = n.NewField
	WHERE
	f.CompanyID <> 1;

UPDATE 
	GIOn o
INNER JOIN GIRelation r
	ON r.DesignID = o.DesignID
	AND r.CompanyID = o.CompanyID
	AND r.LineNbr = o.RelationNbr
INNER JOIN GITable pt
	ON pt.DesignID = r.DesignID
	AND pt.CompanyID = r.CompanyID
	AND pt.Alias = r.ParentTable
INNER JOIN OppNames n
	ON LOWER(n.OldField) = LOWER(o.ParentField)
	AND n.TableName = pt.Name
	SET
	ParentField = CONCAT(LOWER(LEFT(n.NewField, 1)), SUBSTRING(n.NewField, 2))
	WHERE
	o.CompanyID <> 1;

UPDATE 
	GIOn o
INNER JOIN GIRelation r
	ON r.DesignID = o.DesignID
	AND r.CompanyID = o.CompanyID
	AND r.LineNbr = o.RelationNbr
INNER JOIN GITable ct
	ON ct.DesignID = r.DesignID
	AND ct.CompanyID = r.CompanyID
	AND ct.Alias = r.ChildTable
INNER JOIN OppNames n
	ON LOWER(n.OldField) = LOWER(o.ChildField)
	AND n.TableName = ct.Name
	SET
	ChildField = CONCAT(LOWER(LEFT(n.NewField, 1)), SUBSTRING(n.NewField, 2))
WHERE
	o.CompanyID <> 1;

UPDATE 
	GIWhere w
INNER JOIN GITable t
	ON t.DesignID = w.DesignID
	AND t.CompanyID = w.CompanyID
	AND INSTR(LOWER(t.Alias), LOWER(w.DataFieldName)) > 0
INNER JOIN OppNames n
	ON LOWER(w.DataFieldName) = LOWER(CONCAT(t.Alias, '.', n.OldField))
	AND n.TableName = t.Name
	SET
	DataFieldName = CONCAT(t.Alias, '.', LOWER(LEFT(n.NewField, 1)), SUBSTRING(n.NewField, 2))
WHERE
	w.CompanyID <> 1;

DROP TABLE OppNames;
GO

--[mysql: Native]
--[mssql: Skip]
--[MinVersion(Hash = a697fd41547e6fe39fb7bfc80bf584d9b8359655feed07ea6c23eadc761bba6c)]
CREATE TEMPORARY TABLE OppNames
(
	 RecordID INT NOT NULL AUTO_INCREMENT, 
	 TableName varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,		
  	 OldField varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
   	 NewField varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
     Primary Key (RecordID)
);

INSERT INTO OppNames
(
	TableName,
	OldField,
	NewField
		  )
VALUES
('PX.Objects.CR.CROpportunity', 'OpportunityName',		'Subject'),
('PX.Objects.CR.CROpportunity', 'Description',			'Details'),
('PX.Objects.CR.CROpportunity', 'CROpportunityClassID', 'ClassID');

UPDATE 
	EPRuleCondition rc
INNER JOIN OppNames n 
	ON LOWER(rc.FieldName) = LOWER(n.OldField) 
	AND rc.Entity = n.TableName
SET 
	rc.FieldName = REPLACE(rc.FieldName, n.OldField, n.NewField)
WHERE 
	rc.CompanyID <> 1;

DROP TABLE OppNames;
GO

--[IfExists(Table = CROpportunityRevision)]
--[IfExists(Column = CROpportunity.AllowOverrideContactAddress)]
--[IfExists(Column = CROpportunityProducts.CROpportunityID)]
--[IfNotExistsSelect(From = CROpportunityRevision)]
--[IfExistsSelect(From = CROpportunity)]
--[OldHash(Hash = 60de117d283b01b2e078f1952497c7e17dc1f812ec78a3ec891fc9b0fecf7569)]
--[MinVersion(Hash = e7c0ab77c72b57307f5ae9eefa491adc87299aecbdd45536f1652e47c8e498ea)]
INSERT INTO CROpportunityRevision(CompanyID, OpportunityID, RevisionID, DocumentDate, BranchID, CuryID, CuryInfoID, BAccountID, LocationID, ContactID, AllowOverrideContactAddress, CampaignSourceID, ParentBAccountID, ProjectID, 
ManualTotalEntry, Amount, CuryAmount, ProductsAmount, CuryProductsAmount, 
WorkgroupID, OwnerID, TaxZoneID, TaxCalcMode, CuryTaxTotal, TaxTotal, CuryLineTotal, LineTotal, DiscTot, CuryDiscTot, LineDocDiscountTotal, CuryLineDocDiscountTotal, IsTaxValid, VatTaxableTotal, CuryVatTaxableTotal, VatExemptTotal, CuryVatExemptTotal, CreatedByID, CreatedDateTime, CreatedByScreenID, LastModifiedByID, LastModifiedByScreenID, LastModifiedDateTime, OpportunityAddressID, OpportunityContactID, NoteID, ProductCntr, 
LineDiscountTotal, CuryLineDiscountTotal,
ExtPriceTotal, CuryExtPriceTotal)
SELECT R.CompanyID, R.OpportunityID, 0, CloseDate, BranchID, CuryID, CuryInfoID, BAccountID, LocationID, ContactID, AllowOverrideContactAddress, CampaignSourceID, ParentBAccountID, ProjectID, 
ManualTotalEntry, 
(CASE WHEN ManualTotalEntry = 1 THEN Amount             ELSE ISNULL(P.ExtPrice - P.DiscAmt, 0)            END), 
(CASE WHEN ManualTotalEntry = 1 THEN CuryAmount         ELSE ISNULL(P.CuryExtPrice - P.CuryDiscAmt, 0)    END),  
(CASE WHEN ManualTotalEntry = 1 THEN ProductsAmount     ELSE ISNULL(P.ExtPrice - P.DiscAmt - DiscTot, 0)  END), 
(CASE WHEN ManualTotalEntry = 1 THEN CuryProductsAmount ELSE ISNULL(P.CuryExtPrice - P.CuryDiscAmt - CuryDiscTot, 0)  END),  
WorkgroupID, OwnerID, TaxZoneID, 'T', CuryTaxTotal, TaxTotal, CuryLineTotal, LineTotal, DiscTot, CuryDiscTot, ISNULL(D.DiscountAmt, 0), ISNULL(D.CuryDiscountAmt, 0), IsTaxValid, VatTaxableTotal, CuryVatTaxableTotal, VatExemptTotal, CuryVatExemptTotal, CreatedByID, CreatedDateTime, CreatedByScreenID, LastModifiedByID, LastModifiedByScreenID, LastModifiedDateTime, OpportunityAddressID, OpportunityContactID, 
NEWID(), ProductCntr, 
COALESCE(P.DiscAmt,0), 
COALESCE(P.CuryDiscAmt,0),
COALESCE(P.ExtPrice,0), 
COALESCE(P.CuryExtPrice,0)
FROM CROpportunity R
LEFT JOIN (SELECT P.CompanyID, P.CROpportunityID,
                 SUM(COALESCE(DiscAmt, 0)) as DiscAmt, 
                 SUM(COALESCE(CuryDiscAmt, 0)) as CuryDiscAmt,
                 SUM(COALESCE(ExtPrice, 0)) as ExtPrice, 
                 SUM(COALESCE(CuryExtPrice, 0)) as CuryExtPrice
           FROM CROpportunityProducts P
           GROUP BY  P.CompanyID, P.CROpportunityID
 ) P
 ON  P.CompanyID = R.CompanyID 
			AND P.CROpportunityID = R.OpportunityID
LEFT JOIN (SELECT D.CompanyID, D.OpportunityID,
                 SUM(COALESCE(DiscountAmt, 0)) as DiscountAmt, 
                 SUM(COALESCE(CuryDiscountAmt, 0)) as CuryDiscountAmt
           FROM CROpportunityDiscountDetail D
		   WHERE [Type] = 'D'
           GROUP BY  D.CompanyID, D.OpportunityID
 ) D
 ON  D.CompanyID = R.CompanyID 
			AND D.OpportunityID = R.OpportunityID

UPDATE CROpportunity SET DefQuoteID = CROpportunityRevision.NoteID
	FROM CROpportunityRevision
	WHERE CROpportunity.CompanyID = CROpportunityRevision.CompanyID AND CROpportunity.OpportunityID = CROpportunityRevision.OpportunityID
GO

--[IfExists(Table = CROpportunityRevision)]
--[IfNotExists(Column = CROpportunity.AllowOverrideContactAddress)]
--[IfExists(Column = CROpportunityProducts.CROpportunityID)]
--[IfNotExistsSelect(From = CROpportunityRevision)]
--[IfExistsSelect(From = CROpportunity)]
--[OldHash(Hash = 503f6d5914aae69935747a0fd39c93c67d2e9abef9e096c1fd8cae40d19c90d1)]
--[MinVersion(Hash = 25257aa91ac4f68b84e605c3740225144360ade30ff4c843e8f117ec687e00a5)]
INSERT INTO CROpportunityRevision(CompanyID, OpportunityID, RevisionID, DocumentDate, BranchID, CuryID, CuryInfoID, BAccountID, LocationID, ContactID, AllowOverrideContactAddress, CampaignSourceID, ParentBAccountID, ProjectID, ManualTotalEntry, Amount, CuryAmount, ProductsAmount, CuryProductsAmount, WorkgroupID, OwnerID, TaxZoneID, TaxCalcMode, CuryTaxTotal, TaxTotal, CuryLineTotal, LineTotal, DiscTot, CuryDiscTot, LineDocDiscountTotal, CuryLineDocDiscountTotal, IsTaxValid, VatTaxableTotal, CuryVatTaxableTotal, VatExemptTotal, CuryVatExemptTotal, CreatedByID, CreatedDateTime, CreatedByScreenID, LastModifiedByID, LastModifiedByScreenID, LastModifiedDateTime, OpportunityAddressID, OpportunityContactID, NoteID, ProductCntr, 
LineDiscountTotal, CuryLineDiscountTotal,
ExtPriceTotal, CuryExtPriceTotal)
SELECT R.CompanyID, R.OpportunityID, 0, CloseDate, BranchID, CuryID, CuryInfoID, BAccountID, LocationID, ContactID, NULL, CampaignSourceID, ParentBAccountID, ProjectID, 
ManualTotalEntry, 
(CASE WHEN ManualTotalEntry = 1 THEN Amount             ELSE ISNULL(P.ExtPrice - P.DiscAmt, 0)            END), 
(CASE WHEN ManualTotalEntry = 1 THEN CuryAmount         ELSE ISNULL(P.CuryExtPrice - P.CuryDiscAmt, 0)    END),  
(CASE WHEN ManualTotalEntry = 1 THEN ProductsAmount     ELSE ISNULL(P.ExtPrice - P.DiscAmt - DiscTot, 0)  END), 
(CASE WHEN ManualTotalEntry = 1 THEN CuryProductsAmount ELSE ISNULL(P.CuryExtPrice - P.CuryDiscAmt - CuryDiscTot, 0)  END),  
WorkgroupID, OwnerID, TaxZoneID, 'T', CuryTaxTotal, TaxTotal, CuryLineTotal, LineTotal, DiscTot, CuryDiscTot, ISNULL(D.DiscountAmt, 0), ISNULL(D.CuryDiscountAmt, 0), IsTaxValid, VatTaxableTotal, CuryVatTaxableTotal, VatExemptTotal, CuryVatExemptTotal, CreatedByID, CreatedDateTime, CreatedByScreenID, LastModifiedByID, LastModifiedByScreenID, LastModifiedDateTime, NULL, NULL, 
NEWID(), ProductCntr, 
COALESCE(P.DiscAmt,0), 
COALESCE(P.CuryDiscAmt,0),
COALESCE(P.ExtPrice,0), 
COALESCE(P.CuryExtPrice,0)
FROM CROpportunity R
LEFT JOIN (SELECT P.CompanyID, P.CROpportunityID,
                 SUM(COALESCE(DiscAmt, 0)) as DiscAmt, 
                 SUM(COALESCE(CuryDiscAmt, 0)) as CuryDiscAmt,
                 SUM(COALESCE(ExtPrice, 0)) as ExtPrice, 
                 SUM(COALESCE(CuryExtPrice, 0)) as CuryExtPrice
           FROM CROpportunityProducts P
           GROUP BY  P.CompanyID, P.CROpportunityID
 ) P
 ON  P.CompanyID = R.CompanyID 
			AND P.CROpportunityID = R.OpportunityID
LEFT JOIN (SELECT D.CompanyID, D.OpportunityID,
                 SUM(COALESCE(DiscountAmt, 0)) as DiscountAmt, 
                 SUM(COALESCE(CuryDiscountAmt, 0)) as CuryDiscountAmt
           FROM CROpportunityDiscountDetail D
		   WHERE [Type] = 'D'
           GROUP BY  D.CompanyID, D.OpportunityID
 ) D
 ON  D.CompanyID = R.CompanyID 
			AND D.OpportunityID = R.OpportunityID

UPDATE CROpportunity SET DefQuoteID = CROpportunityRevision.NoteID
	FROM CROpportunityRevision
	WHERE CROpportunity.CompanyID = CROpportunityRevision.CompanyID AND CROpportunity.OpportunityID = CROpportunityRevision.OpportunityID
GO

--[IfExists(Table = CROpportunityRevision)]
--[MinVersion(Hash = 23bb7113a8efaedead31f0e1abfc376dbc1d428494d59052affb8a7d0dfaf577)]
UPDATE CROpportunityRevision
	SET LineDocDiscountTotal = DiscTot, CuryLineDocDiscountTotal = CuryDiscTot
	WHERE LineDocDiscountTotal IS NULL OR CuryLineDocDiscountTotal IS NULL
GO

--[IfExists(Table = CROpportunityRevision)]
--[IfNotExistsSelect(From = CROpportunityRevision)]
--[IfExistsSelect(From = CROpportunity)]
--[MinVersion(Hash = 1601824c6d37d0152c23ee1dd8ec77d7b7e34833da2505a147f9575bfe0481d6)]
UPDATE CSAnswers
SET RefNoteID = O.NoteID
FROM CSAnswers A
     INNER JOIN CROpportunityRevision R ON R.CompanyID = A.CompanyID AND R.NoteID = A.RefNoteID
	 INNER JOIN CROpportunity         O ON R.CompanyID = O.CompanyID AND R.OpportunityID = O.OpportunityID
GO

--[IfExists(Column = CRSetup.QuoteNumberingID)]
--[MinVersion(Hash = 4dd2a0ee470f3fa49878a5ec0bfdff9c9e150ba57f5b3c1c91b4142e5c998f3e)]
UPDATE CRSetup SET QuoteNumberingID = 'CRQUOTE' WHERE QuoteNumberingID IS NULL
GO

--[IfExists(Column = PMSetup.QuoteNumberingID)]
--[MinVersion(Hash = a4c01e5aeddc4b5887c7bd426eef1c0af35d5862bbb6dc892398be6e29fbdf9d)]
UPDATE PMSetup SET QuoteNumberingID = 'PMQUOTE' WHERE QuoteNumberingID IS NULL
GO

--Check duplicates NoteID in CROpportunity
--[MinVersion(Hash = 51df1b083ff321bd36b327099877a84f4a291fb98d81f3927ec3bee483368d5c)]
update CROpportunity
set NoteID=newid()
from    CROpportunity C
        INNER JOIN (SELECT CompanyID, OpportunityID, 
		                  (select top 1
                                    c2.OpportunityID
                           from     CROpportunity c2
                           where    c2.NoteID = C.NoteID
                                    and c2.CompanyID = C.CompanyID
                           order by c2.OpportunityID desc
                          ) as SourceID
					 FROM CROpportunity C
				    ) D
	            ON C.CompanyID = D.CompanyID AND
				   C.OpportunityID = D.OpportunityID  
WHERE C.OpportunityID != D.SourceID
GO

--[IfNotExists(Column = CROpportunity.OpportunityContactID)]
--[IfExists(Table = CROpportunityRevision)]
--[OldHash(Hash = 88407b3702523164e1fd1a4800490058064ef4d9f709214a300b5b128a37d59f)]
--[MinVersion(Hash = 66d1d08587a3977074c5578a9874007c98715472febf9fee297d39e56a610e4f)]
if exists(select * from CROpportunityRevision c where c.OpportunityContactID is null )
begin
insert  into CRContact
        (CompanyID,
         FirstName,
         LastName,
         Title,
         Salutation,
         MidName,
         FullName,
         Email,
         WebSite,
         Phone1,
         Phone1Type,
         Phone2,
         Phone2Type,
         Phone3,
         Phone3Type,
         Fax,
         FaxType,
         DisplayName,
         CreatedByID,
         CreatedByScreenID,
         CreatedDateTime,
         LastModifiedByID,
         LastModifiedByScreenID,
         LastModifiedDateTime,         
         BAccountID,
         BAccountContactID,
         BAccountLocationID,
         RevisionID,
         IsDefaultContact,
   OpportunityId,
   NoteID)
        select  co.CompanyID,
  c.FirstName,
  c.LastName,
  c.Title,
  c.Salutation,
  c.MidName,
  c.FullName,
  c.EMail,
  c.WebSite,
  c.Phone1,
  c.Phone1Type,
  c.Phone2,
  c.Phone2Type,
  c.Phone3,
  c.Phone3Type,
  c.Fax,
  c.FaxType,
  c.DisplayName,
  co.CreatedByID,
  co.CreatedByScreenID,
  co.CreatedDateTime,
  co.LastModifiedByID,
  co.LastModifiedByScreenID,
  co.LastModifiedDateTime,  
  c.BAccountID,
  null,
  null,
  c.RevisionID,
  null,
  co.OpportunityId,
  NEWID()  
        from    CROpportunityRevision co
                left join Contact c on (c.ContactID = co.ContactID
					and c.CompanyID = co.CompanyID)
        where   (co.OpportunityContactID is null 
					and co.ContactID is not null)


if exists(select * from CROpportunityRevision c where c.OpportunityContactID is null )
insert  into CRContact
        (CompanyID,
         FirstName,
         LastName,
         Title,
         Salutation,
         MidName,
         FullName,
         Email,
         WebSite,
         Phone1,
         Phone1Type,
         Phone2,
         Phone2Type,
         Phone3,
         Phone3Type,
         Fax,
         FaxType,
         DisplayName,
         CreatedByID,
         CreatedByScreenID,
         CreatedDateTime,
         LastModifiedByID,
         LastModifiedByScreenID,
         LastModifiedDateTime,         
         BAccountID,
         BAccountContactID,
         BAccountLocationID,
         RevisionID,
         IsDefaultContact,
   OpportunityId,
   NoteID)
        select  co.CompanyID,
  c.FirstName,
  c.LastName,
  c.Title,
  c.Salutation,
  c.MidName,
  c.FullName,
  c.EMail,
  c.WebSite,
  c.Phone1,
  c.Phone1Type,
  c.Phone2,
  c.Phone2Type,
  c.Phone3,
  c.Phone3Type,
  c.Fax,
  c.FaxType,
  c.DisplayName,
  co.CreatedByID,
  co.CreatedByScreenID,
  co.CreatedDateTime,
  co.LastModifiedByID,
  co.LastModifiedByScreenID,
  co.LastModifiedDateTime,  
  c.BAccountID,
  null,
  null,
  c.RevisionID,
  null,
  co.OpportunityId,
  NEWID()  
        from    CROpportunityRevision co
                left join Location l on l.LocationID = co.LocationID
							and l.CompanyID = co.CompanyID
                left join Contact c on c.ContactID = l.DefContactID
							and c.CompanyID = co.CompanyID
        where   co.OpportunityContactID is null 
							and co.ContactID is null 

update  co
set     co.OpportunityContactID = cr.ContactID
from    CROpportunityRevision co
        left join CRContact cr on cr.CompanyId = co.CompanyID
                                   and cr.OpportunityId = co.OpportunityID
end
GO

--[IfNotExists(Column = CROpportunity.OpportunityAddressID)]
--[IfExists(Table = CROpportunityRevision)]
--[OldHash(Hash = b1935d475f455dc7ab2542160abd84dbcfe9d1342b3cde0f4e140c161d3c7770)]
--[MinVersion(Hash = 752ded6b9827eaa585235d3357237a89cae3d7425a79db81a81df4ece04411ec)]
if exists(select * from CROpportunityRevision c where c.OpportunityAddressID is null )
begin
insert  into CRAddress
        (CompanyID,
        AddressLine1,
        AddressLine2,
		AddressLine3,
        City,
        CountryID,
        State,
        PostalCode,
        CreatedByID,
        CreatedByScreenID,
        CreatedDateTime,
        LastModifiedByID,
        LastModifiedByScreenID,
        LastModifiedDateTime,
        BAccountID,
        BAccountAddressID,
        RevisionID,
        IsDefaultAddress,        
        IsValidated,
   OpportunityId,
   NoteID)
        select  co.CompanyID,        
        a.AddressLine1,
        a.AddressLine2,
        a.AddressLine3,
        a.City,
		a.CountryID,
        a.State,        
        a.PostalCode,
        co.CreatedByID,
        co.CreatedByScreenID,
        co.CreatedDateTime,
        co.LastModifiedByID,
        co.LastModifiedByScreenID,
        co.LastModifiedDateTime,
		a.BAccountID,
		a.BAccountID,
        a.RevisionID,
		null,
        a.IsValidated,
  co.OpportunityId,
  NEWID()  
        from    CROpportunityRevision co
                left join Contact c on c.contactID = co.contactID
					and c.CompanyID = co.CompanyID
                left join Address a on c.DefAddressID=a.AddressID
					and a.CompanyID = co.CompanyID
        where   (co.OpportunityAddressID is null
			and co.ContactID is not null)

insert  into CRAddress
        (CompanyID,
        AddressLine1,
        AddressLine2,
		AddressLine3,
        City,
        CountryID,
        State,
        PostalCode,
        CreatedByID,
        CreatedByScreenID,
        CreatedDateTime,
        LastModifiedByID,
        LastModifiedByScreenID,
        LastModifiedDateTime,
        BAccountID,
        BAccountAddressID,
        RevisionID,
        IsDefaultAddress,        
        IsValidated,
   OpportunityId,
   NoteID)
        select  co.CompanyID,        
        a.AddressLine1,
        a.AddressLine2,
        a.AddressLine3,
        a.City,
		a.CountryID,
        a.State,        
        a.PostalCode,
        co.CreatedByID,
        co.CreatedByScreenID,
        co.CreatedDateTime,
        co.LastModifiedByID,
        co.LastModifiedByScreenID,
        co.LastModifiedDateTime,
		a.BAccountID,
		a.BAccountID,
        a.RevisionID,
		null,
        a.IsValidated,
  co.OpportunityId,
  NEWID()  
        from    CROpportunityRevision co
					left join Location l on l.LocationID = co.LocationID 
						and l.CompanyID = co.CompanyID
					left join Address a on 
					l.DefAddressID=a.AddressID 
						and a.CompanyID = co.CompanyID
        where   co.OpportunityAddressID is null and co.ContactID is null

update  co
set     co.OpportunityAddressID = a.AddressID
from    CROpportunityRevision co
        left join CRAddress a on a.CompanyId = co.CompanyID
                                   and a.OpportunityId = co.OpportunityID
end
GO

--[IfNotExists(Column = CROpportunity.AllowOverrideContactAddress)]
--[IfExists(Table = CROpportunityRevision)]
--[MinVersion(Hash = d88401798e06f18001998b1a46fc2e27978454c305ba01d1365fa743132eda83)]
begin
update  co
set     co.AllowOverrideContactAddress = 1
from    CROpportunityRevision co
        where co.LocationID is null 
		and co.ContactID is null 
end
GO

--[IfExists(Table = CROpportunityRevision)]
--[IfExists(Column = CROpportunityRevision.AllowOverrideShippingContactAddress)]
--[MinVersion(Hash = 95e25ca4da23039e146de022d9f2964412e0445e96181667bf12231486a046cc)]
DECLARE @Creator uniqueidentifier
DECLARE @NoteID uniqueidentifier
DECLARE @CompanyID INT
DECLARE @ShipContactID INT
DECLARE @ShipAddressID INT
DECLARE @BAccountID INT
DECLARE @LocationID INT
DECLARE @ContactID INT

INSERT INTO [CRContact]
  ([CompanyID],[FirstName],[LastName],[Title],[Salutation],[Attention],[MidName],[FullName],[Email]
  ,[WebSite],[Phone1],[Phone1Type],[Phone2],[Phone2Type],[Phone3],[Phone3Type],[Fax],[FaxType],[DisplayName]
  ,[BAccountID],[BAccountContactID],[IsDefaultContact], [NoteID]
  ,[CreatedByID],[CreatedByScreenID],[CreatedDateTime],[LastModifiedByID],[LastModifiedByScreenID],[LastModifiedDateTime])
SELECT C.[CompanyID], C.[FirstName],C.[LastName],C.[Title],C.[Salutation],C.[Attention],C.[MidName],C.[FullName],C.[EMail]
  ,C.[WebSite],C.[Phone1],C.[Phone1Type],C.[Phone2],C.[Phone2Type],C.[Phone3],C.[Phone3Type],C.[Fax],C.[FaxType],C.[DisplayName]
  ,C.[BAccountID],C.[ContactID], 1, O.NoteID,
  U.[PKID], 'CR304000', GETDATE(), U.[PKID],  'CR304000', GETDATE()
FROM [Contact] C
INNER JOIN [Location] L ON C.ContactID = L.DefContactID AND C.CompanyID = L.CompanyID
INNER JOIN [CROpportunityRevision] O ON O.LocationID = L.LocationID AND L.CompanyID = O.CompanyID
INNER JOIN [Users] U ON U.CompanyID = O.CompanyID AND U.[Username] = 'admin'
WHERE O.ShipContactID IS NULL AND
	  O.LocationID IS NOT NULL AND O.BAccountID IS NULL AND O.ContactID IS NULL
	   

INSERT INTO [CRAddress]
   ([CompanyID],[AddressLine1],[AddressLine2],[AddressLine3],[City],[CountryID],[State],[PostalCode],[BAccountID]
   ,[BAccountAddressID],[IsDefaultAddress],[IsValidated],[NoteID]
   ,[CreatedByID],[CreatedByScreenID],[CreatedDateTime],[LastModifiedByID],[LastModifiedByScreenID],[LastModifiedDateTime])
SELECT A.[CompanyID], A.[AddressLine1], A.[AddressLine2], A.[AddressLine3], A.[City], A.[CountryID], A.[State], A.[PostalCode], A.[BAccountID]
  ,A.[AddressID], 1, A.[IsValidated], O.NoteID,
  U.[PKID], 'CR304000', GETDATE(), U.[PKID],  'CR304000', GETDATE()
FROM [Address] A
INNER JOIN [Location] L ON A.AddressID = L.DefAddressID AND L.CompanyID = A.CompanyID
INNER JOIN [CROpportunityRevision] O ON O.LocationID = L.LocationID AND L.CompanyID = O.CompanyID 
INNER JOIN [Users] U ON U.CompanyID = O.CompanyID AND U.[Username] = 'admin'
WHERE O.ShipAddressID IS NULL AND
      O.LocationID IS NOT NULL AND O.BAccountID IS NULL AND O.ContactID IS NULL  

INSERT INTO [CRContact]
  ([CompanyID],[FirstName],[LastName],[Title],[Salutation],[Attention],[MidName],[FullName],[Email]
  ,[WebSite],[Phone1],[Phone1Type],[Phone2],[Phone2Type],[Phone3],[Phone3Type],[Fax],[FaxType],[DisplayName]
  ,[BAccountID],[BAccountContactID],[IsDefaultContact], [NoteID]
  ,[CreatedByID],[CreatedByScreenID],[CreatedDateTime],[LastModifiedByID],[LastModifiedByScreenID],[LastModifiedDateTime])
SELECT C.[CompanyID], C.[FirstName],C.[LastName],C.[Title],C.[Salutation],C.[Attention],C.[MidName],C.[FullName],C.[EMail]
  ,C.[WebSite],C.[Phone1],C.[Phone1Type],C.[Phone2],C.[Phone2Type],C.[Phone3],C.[Phone3Type],C.[Fax],C.[FaxType],C.[DisplayName]
  ,C.[BAccountID],C.[ContactID], 1, O.NoteID,
  U.[PKID], 'CR304000', GETDATE(), U.[PKID],  'CR304000', GETDATE()
FROM [Contact] C
INNER JOIN BAccount BA ON BA.BAccountID = C.BAccountID
                       AND BA.DefContactID = C.ContactID
INNER JOIN [CROpportunityRevision] O ON O.BAccountID = BA.BAccountID AND BA.CompanyID = O.CompanyID
INNER JOIN [Users] U ON U.CompanyID = O.CompanyID AND BA.CompanyID = C.CompanyID AND U.[Username] = 'admin'
WHERE O.ShipContactID IS NULL AND
	  O.BAccountID IS NOT NULL   

INSERT INTO [CRAddress]
   ([CompanyID],[AddressLine1],[AddressLine2],[AddressLine3],[City],[CountryID],[State],[PostalCode],[BAccountID]
   ,[BAccountAddressID],[IsDefaultAddress],[IsValidated],[NoteID]
   ,[CreatedByID],[CreatedByScreenID],[CreatedDateTime],[LastModifiedByID],[LastModifiedByScreenID],[LastModifiedDateTime])
SELECT A.[CompanyID], A.[AddressLine1], A.[AddressLine2], A.[AddressLine3], A.[City], A.[CountryID], A.[State], A.[PostalCode], A.[BAccountID]
  ,A.[AddressID], 1, A.[IsValidated], O.NoteID,
  U.[PKID], 'CR304000', GETDATE(), U.[PKID],  'CR304000', GETDATE()
FROM [Address] A
INNER JOIN BAccount BA ON BA.BAccountID = A.BAccountID
                       AND BA.DefAddressID = A.AddressID
					   AND BA.CompanyID = A.CompanyID
INNER JOIN [CROpportunityRevision] O ON O.BAccountID = BA.BAccountID AND BA.CompanyID = O.CompanyID
INNER JOIN [Users] U ON U.CompanyID = O.CompanyID AND U.[Username] = 'admin'
WHERE O.ShipAddressID IS NULL AND
	  O.BAccountID IS NOT NULL  

INSERT INTO [CRContact]
  ([CompanyID],[FirstName],[LastName],[Title],[Salutation],[Attention],[MidName],[FullName],[Email]
  ,[WebSite],[Phone1],[Phone1Type],[Phone2],[Phone2Type],[Phone3],[Phone3Type],[Fax],[FaxType],[DisplayName]
  ,[BAccountID],[BAccountContactID],[IsDefaultContact], [NoteID]
  ,[CreatedByID],[CreatedByScreenID],[CreatedDateTime],[LastModifiedByID],[LastModifiedByScreenID],[LastModifiedDateTime])
SELECT C.[CompanyID], C.[FirstName],C.[LastName],C.[Title],C.[Salutation],C.[Attention],C.[MidName],C.[FullName],C.[EMail]
  ,C.[WebSite],C.[Phone1],C.[Phone1Type],C.[Phone2],C.[Phone2Type],C.[Phone3],C.[Phone3Type],C.[Fax],C.[FaxType],C.[DisplayName]
  ,C.[BAccountID],C.[ContactID], 1, O.NoteID,
  U.[PKID], 'CR304000', GETDATE(), U.[PKID],  'CR304000', GETDATE()
FROM [Contact] C
INNER JOIN [CROpportunityRevision] O ON O.ContactID = C.ContactID AND C.CompanyID = O.CompanyID 
INNER JOIN [Users] U ON U.CompanyID = O.CompanyID AND U.[Username] = 'admin'
WHERE O.ShipContactID IS NULL AND
	  O.BAccountID IS NULL AND
	  O.ContactID IS NOT NULL 

INSERT INTO [CRAddress]
   ([CompanyID],[AddressLine1],[AddressLine2],[AddressLine3],[City],[CountryID],[State],[PostalCode],[BAccountID]
   ,[BAccountAddressID],[IsDefaultAddress],[IsValidated],[NoteID]
   ,[CreatedByID],[CreatedByScreenID],[CreatedDateTime],[LastModifiedByID],[LastModifiedByScreenID],[LastModifiedDateTime])
SELECT A.[CompanyID], A.[AddressLine1], A.[AddressLine2], A.[AddressLine3], A.[City], A.[CountryID], A.[State], A.[PostalCode], A.[BAccountID]
  ,A.[AddressID], 1, A.[IsValidated], O.NoteID,
  U.[PKID], 'CR304000', GETDATE(), U.[PKID],  'CR304000', GETDATE()
FROM [Address] A
INNER JOIN Contact C ON C.DefAddressID = A.AddressID AND A.CompanyID = C.CompanyID 
INNER JOIN [CROpportunityRevision] O ON O.ContactID = C.ContactID AND C.CompanyID = O.CompanyID
INNER JOIN [Users] U ON U.CompanyID = O.CompanyID AND U.[Username] = 'admin'
WHERE O.ShipAddressID IS NULL AND
	  O.BAccountID IS NULL AND
	  O.ContactID IS NOT NULL	   

UPDATE O
SET O.ShipContactID = CR.ContactID,
	O.[LastModifiedDateTime] = GETDATE()
FROM [CROpportunityRevision] O
INNER JOIN CRContact CR ON CR.NoteID = O.NoteID AND CR.CompanyID = O.CompanyID
WHERE O.ShipContactID IS NULL 

UPDATE O
SET O.ShipAddressID = CR.AddressID,
	O.[LastModifiedDateTime] = GETDATE()
FROM [CROpportunityRevision] O
INNER JOIN CRAddress CR ON CR.NoteID = O.NoteID AND CR.CompanyID = O.CompanyID
WHERE O.ShipAddressID IS NULL 

UPDATE CR
SET CR.NoteID = NEWID(),
	CR.[LastModifiedDateTime] = GETDATE()
FROM [CRContact] CR
INNER JOIN [CROpportunityRevision] O ON CR.NoteID = O.NoteID
			AND O.CompanyID = CR.CompanyID

UPDATE CR
SET CR.NoteID = NEWID(),
	CR.[LastModifiedDateTime] = GETDATE()
FROM [CRAddress] CR
INNER JOIN [CROpportunityRevision] O ON CR.NoteID = O.NoteID
			AND O.CompanyID = CR.CompanyID
GO

--[MinVersion(Hash = ce24d084cb7999d8d156f025f550ab00bacf2469cd8279b724738f3cf92742c8)]
INSERT INTO [NotificationSource]
( 
	[CompanyID],
	[SetupID],
	[ClassID],
	[NotificationID],
	[ReportID],
	[Format],
	[Active],

	[CreatedByID],
	[CreatedDateTime],
	[CreatedByScreenID],
	[LastModifiedByID],
	[LastModifiedDateTime],
	[LastModifiedByScreenID]
)
	SELECT
		[CompanyID],
		'07de59b6-441e-46eb-9fa5-6265d1c8867a',
		[CRCustomerClassID],
		293,
		'CR604500',
		'P',
		0,

		'b5344897-037e-4d58-b5c3-1bdfd0f47bf9',
		'2017-12-10 06:12:19.403',
		'CR208000',
		'b5344897-037e-4d58-b5c3-1bdfd0f47bf9',
		'2017-12-10 06:12:19.403',
		'CR208000'
FROM 
		[CRCustomerClass]
GO

--[OldHash(Hash = 66873d8e88652d15610dd97456df5483d19d0a2d71b44db2ba240dceaa59a713)]
--[MinVersion(Hash = 50ef6c86593a73adecbab107a3465a33760b12361a0642571aa2c49ccccf8ad3)]
update pol set Released = por.Released from POReceiptLine pol join POReceipt por on pol.CompanyID = por.CompanyID and pol.ReceiptType = por.ReceiptType and pol.ReceiptNbr=por.ReceiptNbr
GO

--init 2-step partial transit logic
--[mysql: Skip]
--[MinVersion(Hash = 3f63a9a4e44a47b88404b0fcd520f65bc6bb979475cfb9ede84fc28db470fc87)]
BEGIN
	declare @fkname nvarchar(100)
	declare @Command  nvarchar(1000)
	
	select @fkname = fk.name from sys.foreign_keys fk
	inner join sys.foreign_key_columns fkc on fk.object_id = fkc.constraint_object_id
	inner join sys.columns c1 on fkc.parent_column_id = c1.column_id and c1.object_id = fkc.parent_object_id
	inner join sys.columns c2 on fkc.referenced_column_id = c2.column_id and c2.object_id = fkc.referenced_object_id
	where fk.referenced_object_id = OBJECT_ID('INLocation') and fk.parent_object_id = OBJECT_ID('INLocationStatus')
	and c1.name = 'LocationID' and c2.name = 'LocationID'

	if @fkname is not null 
		begin
		select @Command = 'alter table INLocationStatus drop constraint ' + @fkname
		execute (@Command)
		end
	end
GO

--[OldHash(Hash = 8d3cfdce6043fa3aec5347a35bab50dc5a093c2b1012c3acc55e04606c5dd449)]
--[OldHash(Hash = 66f2aa757a3bb2dc1c25dce5a1144ef1d823c6a642bf9e05791f51fab66ba3bb)]
--[OldHash(Hash = 169724bcdcd071f764839178800c69a4b68c1b4cfcdccd10aa070ff670ec138d)]
--[OldHash(Hash = 32566d943145b77fb759c8260c952d1e7a05c6e9e32b183b015a99fa5c679177)]
--[MinVersion(Hash = 533fc1c1e28daf4833b8849a1d3509f1ed3d1aedf1f365468484b9ed4688ecd3)]
update INPlanType set DeleteOnEvent = 1 where PlanType in ('43', '45')

update INPlanType set DeleteOnEvent = 0, ReplanOnEvent = '43' where PlanType = '42'
update INPlanType set DeleteOnEvent = 0, ReplanOnEvent = '45' where PlanType = '44'

SET IDENTITY_INSERT INCostSite ON
insert INCostSite (CompanyID, CostSiteID) 
select q.CompanyID, SiteID from INSite q
left join INCostSite ics on ics.CompanyID=q.CompanyID and ics.CostSiteID=q.SiteID 
where ics.CompanyID is null

insert INCostSite (CompanyID, CostSiteID) 
select q.CompanyID, LocationID from INLocation q
left join INCostSite ics on ics.CompanyID=q.CompanyID and ics.CostSiteID=q.LocationID 
where ics.CompanyID is null
SET IDENTITY_INSERT INCostSite OFF

INSERT INSite (
	[INSite].[CompanyID]
	,[INSite].[SiteID]
	,[INSite].[SiteCD]
	,[INSite].[Descr]
	,[INSite].[LocationValid]
	,[INSite].[BranchID]
	,[INSite].[AvgDefaultCost]
	,[INSite].[FIFODefaultCost]
	,[INSite].[OverrideInvtAccSub]
	,[INSite].[Active]
	,[INSite].[CreatedByID]
	,[INSite].[CreatedDateTime]
	,[INSite].[CreatedByScreenID]
	,[INSite].[LastModifiedByID]
	,[INSite].[LastModifiedDateTime]
	,[INSite].[LastModifiedByScreenID]
	,[INSite].[GroupMask]
	,[INSite].[UseItemDefaultLocationForPicking]
	,[INSite].[NoteID]
	)
select 
	c.companyid
	,(select coalesce(max(CostSiteID), 1) from INCostSite where CostSiteID > 0 and CompanyID = c.companyid) + 1
	,N'INTR'
	,N'Transit DEFAULT warehouse'
	,'V'
	, branchsq.branchid
	,'A'
	,'A'
	,1
	,1
	,ins.CreatedByID
	,GETDATE()
	,ins.CreatedByScreenID
	,ins.LastModifiedByID
	,GETDATE()
	,ins.LastModifiedByScreenID
	,0x
	,0
	,NEWID()
from Company c
join INSetup ins on c.CompanyID=ins.CompanyID and ins.transitsiteid is null
join (select min(branchid) branchid, companyid from Branch where deleteddatabaserecord=0 group by companyid) as branchsq
on branchsq.companyid = c.companyid
left join INSite iss on iss.CompanyID = ins.CompanyID and SiteCD='INTR'
where c.isreadonly=0 and iss.SiteID is null

update ins set TransitSiteID = inst.SiteID from INSetup ins 
join INSite inst on ins.CompanyID = inst.CompanyID and inst.SiteCD='INTR'
where ins.TransitSiteID is null

INSERT INTO INLocation (
	companyid
	,siteid
	,locationcd
	,locationid
	,iscosted
	,InclQtyAvail
	,PickPriority
	,SalesValid
	,ReceiptsValid
	,TransfersValid
	,PrimaryItemValid
	,AssemblyValid
	,Active
	,CreatedByID
	,CreatedByScreenID
	,CreatedDateTime
	,LastModifiedByID
	,LastModifiedByScreenID
	,LastModifiedDateTime
	,NoteID
	)
SELECT c.companyid
	,s.SiteID
	,'MAIN'
	,(select coalesce(max(CostSiteID), 1) from INCostSite where CostSiteID > 0 and CompanyID = c.companyid) + 2
	,0
	,0
	,0
	,0
	,0
	,1
	,0
	,0
	,1
	,s.CreatedByID
	,s.CreatedByScreenID
	,s.CreatedDateTime
	,s.LastModifiedByID
	,s.LastModifiedByScreenID
	,s.LastModifiedDateTime
	,NEWID()
FROM Company c
join INSetup ins on c.CompanyID=ins.CompanyID
INNER JOIN INSite s on ins.CompanyID = s.CompanyID and ins.TransitSiteID = s.SiteID
LEFT JOIN INLocation l ON l.CompanyID = c.CompanyID
	AND l.SiteID = s.SiteID
WHERE l.LocationID IS NULL


SET IDENTITY_INSERT INCostSite ON
insert INCostSite (CompanyID, CostSiteID) 
select q.CompanyID, SiteID from INSite q
left join INCostSite ics on ics.CompanyID=q.CompanyID and ics.CostSiteID=q.SiteID 
where ics.CompanyID is null

insert INCostSite (CompanyID, CostSiteID) 
select q.CompanyID, LocationID from INLocation q
left join INCostSite ics on ics.CompanyID=q.CompanyID and ics.CostSiteID=q.LocationID 
where ics.CompanyID is null
SET IDENTITY_INSERT INCostSite OFF
GO

--address AC-72606
--[MinVersion(Hash = 7bdb473d12860861464936d4591bf74c61bf5e2298fc1bca8952d75e69a1cae7)]
UPDATE ireg
SET ireg.TransferType=2
FROM
INRegister ireg 
JOIN INTranSplit its ON ireg.CompanyID = its.CompanyID AND ireg.DocType = its.DocType AND ireg.RefNbr=its.RefNbr AND ireg.Released=its.Released AND its.InvtMult=-1 
AND ireg.TransferType = its.TransferType
LEFT JOIN INTran INTran ON
		INTran.CompanyID = its.CompanyID 
		and INTran.OrigTranType = its.TranType
		AND INTran.OrigRefNbr = its.RefNbr
		AND INTran.OrigLineNbr = its.LineNbr
		WHERE ireg.Released = 1 AND INTran.RefNbr IS NULL AND ireg.DocType='T' AND ireg.TransferType=1

update its set 
its.TransferType = ireg.TransferType
from INTranSplit its join INRegister ireg on ireg.CompanyID=its.CompanyID and ireg.DocType = its.DocType 
and ireg.RefNbr=its.RefNbr and ireg.Released = its.Released and ireg.TransferType<>its.TransferType
where its.Released=1 and its.InvtMult=-1 and its.DocType = 'T'
GO

--[OldHash(Hash = 43e2e152cdc2acc79191dd60f3198296bc329a692fcb751482042b19da3201b4)]
--[OldHash(Hash = a3114db130782180613d0898933bb77bf5a78aa087c9e7abf0c64b7aff27557e)]
--[OldHash(Hash = 5f0c7bb831eb9f0e0c00f19794ad9e9dcad83f935e3cc74044b7429f31e76000)]
--[MinVersion(Hash = a8739c1284659f41203a327f589bb2e3a3e8b939a055c0c2dfab56468f76e2b9)]
begin
declare @companyID int
declare @costSiteID int
declare @docType char(1)
declare @tranType char(3)
declare @refNbr nvarchar(15)
declare @lineNbr int
declare @sOOrderType char(2)
declare @sOOrderNbr nvarchar(15)
declare @sOOrderLineNbr int
declare @sOShipmentType char(1)
declare @sOShipmentNbr nvarchar(15)
declare @sOShipmentLineNbr int
declare @origModule char(2)
declare @toSiteID int
declare @siteID int
declare @lotSerTrack char(1)
declare @toLocationID int
declare @noteID uniqueidentifier
declare @docNoteID uniqueidentifier
declare @entityType varchar(255)
declare @graphType varchar(255)
declare @transitSiteID int
declare @baseQty decimal(25,6)

declare transit_cursor cursor for 
	select 
	it.CompanyID, 
	it.DocType,
	max(it.TranType), 
	it.RefNbr, 
	it.LineNbr,
	max(it.SOOrderType),
	max(it.SOOrderNbr),
	max(it.SOOrderLineNbr), 
	max(it.SOShipmentType),
	max(it.SOShipmentNbr), 
	max(it.SOShipmentLineNbr),   
	max(it.ToSiteID),
	max(it.SiteID),
	max(it.ToLocationID),
	max(lsc.LotSerTrack),
	maX(it.BaseQty),
	max(insetup.TransitSiteID),
	CAST(MIN(CAST(ireg.NoteID AS CHAR(36))) as uniqueidentifier)
	from INTran it
	left join INTran it2 on it2.CompanyID=it.CompanyID and it2.OrigTranType = it.TranType and it2.OrigRefNbr = it.RefNbr and it2.OrigLineNbr = it.LineNbr and it2.released=1
	join INRegister ireg on ireg.DocType = it.DocType and ireg.RefNbr=it.RefNbr and ireg.CompanyID = it.CompanyID
	join INTranSplit its on its.DocType = it.DocType and its.RefNbr = it.RefNbr and its.LineNbr = it.LineNbr and it.CompanyID = its.CompanyID
	join InventoryItem ii on its.InventoryID = ii.InventoryID and its.CompanyID = ii.CompanyID
	join INLotSerClass lsc on lsc.CompanyID = ii.CompanyID and lsc.LotSerClassID = ii.LotSerClassID
	join INSetup insetup on insetup.CompanyID = it.CompanyID and transitsiteid is not null
	left join INTransitLine tl on tl.CompanyID=it.CompanyID and tl.TransferNbr=it.RefNbr and tl.TransferLineNbr = it.LineNbr
	where it.Released = 1 and tl.TransferNbr is null and ireg.TransferType = '2'and ireg.DocType='T' and it.InvtMult = -1 and it2.CompanyID is null
group by it.CompanyID, it.DocType, it.RefNbr, it.LineNbr

	INSERT INTO INCostStatus
	(
		CompanyId,
		InventoryID,
		CostSubItemID,
		CostSiteID,
		SiteID,
		AccountID,
		SubID,
		ReceiptNbr,
		ReceiptDate,
		LotSerialNbr,
		OrigQty,
		QtyOnHand,
		UnitCost,
		TotalCost,
		ValMethod,
		LayerType
	)
	select 
		mr.CompanyID,
		mr.InventoryID,
		mr.CostSubItemID,
		mr.CostSiteID,
		mr.SiteID,
		mr.AccountID,
		mr.SubID,
		mr.ReceiptNbr,
		max(mr.ReceiptDate),
		mr.lotSerialNbr,
		sum(mr.origQty),
		sum(mr.QtyOnHand),
		0,
		sum(mr.TotalCost),
		mr.ValMethod,
		'N'
	from
	(select 
		ics.CompanyID,
		ics.InventoryID,
		ics.CostSubItemID,
		insetup.TransitSiteID as CostSiteID,
		insetup.TransitSiteID as SiteID,
		sq.AcctID as AccountID,
		sq.SubID as SubID,
		itc.CostRefNbr as ReceiptNbr,
		itc.TranDate as ReceiptDate,
		case when ics.valmethod = 'S' then ics.LotSerialNbr else '' end as lotSerialNbr,
		itc.qty as origQty,
		itc.qty as QtyOnHand,
		itc.TranCost as TotalCost,
		ics.ValMethod
	from INCostStatus ics 
		join INTranCost itc on ics.CompanyID = itc.CompanyID and ics.CostID = itc.CostID
		join INSetup insetup on insetup.CompanyID = ics.CompanyID and transitsiteid is not null
		join 
		(select it.companyid, it.doctype, it.refnbr, it.linenbr, it.AcctID, it.SubID	from INTran it
			join INTranSplit its on its.DocType = it.DocType and its.RefNbr = it.RefNbr and its.LineNbr = it.LineNbr and it.CompanyID = its.CompanyID
			left join INTransitLine tl on tl.CompanyID = it.CompanyID and tl.TransferNbr = it.RefNbr and tl.TransferLineNbr = it.LineNbr
			left join INTran it2 on it2.CompanyID=it.CompanyID and it2.OrigTranType = it.TranType and it2.OrigRefNbr = it.RefNbr and it2.OrigLineNbr = it.LineNbr and it2.released=1
			where it.Released = 1 and it.DocType = 'T' and it.InvtMult = -1 and its.TransferType='2' and tl.CompanyID is null and it2.CompanyID is null
			group by it.CompanyID, it.DocType, it.RefNbr, it.LineNbr, it.AcctID, it.SubID) sq on 
			itc.CompanyID = sq.companyid and itc.CostDocType = sq.DocType and itc.CostRefNbr = sq.RefNbr and itc.LineNbr = sq.LineNbr) mr
		left join INCostStatus icspos on icspos.CompanyID = mr.CompanyID and icspos.InventoryID = mr.InventoryID and icspos.CostSubItemID = mr.CostSubItemID
		and icspos.AccountID = mr.AccountID and icspos.SubID = mr.SubID and icspos.ReceiptNbr = mr.ReceiptNbr and ISNULL(icspos.LotSerialNbr, '') = ISNULL(mr.lotSerialNbr, '') and icspos.CostSiteID = mr.CostSiteID
		where icspos.CompanyID is null
		group by mr.CompanyID, mr.InventoryID, mr.CostSubItemID, mr.CostSiteID, mr.SiteID, mr.accountid, mr.subid, mr.ReceiptNbr, mr.ValMethod, mr.lotSerialNbr
		
	insert into intrancost(
	CompanyID,
	DocType,
	TranType,
	RefNbr,
	LineNbr,
	CostId,
	CostDocType,
	CostRefNbr,
	IsOversold,
	InvtMult,
	Qty,
	TranCost,
	TranDate,
	FinPeriodID,
	TranPeriodID,
	InventoryID,
	CostSubItemID,
	CostSiteID,
	LotSerialNbr,
	CreatedByID,
	CreatedByScreenID,
	CreatedDateTime,
	LastModifiedByID,
	LastModifiedByScreenID,
	LastModifiedDateTime,
	COGSAcctID,
	COGSSubID,
	InvtAcctID,
	InvtSubID,
	VarCost,
	IsVirtual)
	select 
		itc.CompanyID,
		'T',
		itc.TranType,
		itc.RefNbr,
		itc.LineNbr,
		icsnew.CostID,
		itc.CostDocType,
		itc.CostRefNbr,
		0,
		1,
		sum(itc.Qty),
		sum(itc.TranCost),
		max(itc.TranDate),
		max(itc.FinPeriodID),
		max(itc.TranPeriodID),
		max(itc.InventoryID),
		max(itc.CostSubItemID),
		max(insetup.TransitSiteID),
		max(ics.LotSerialNbr),
		CAST(MAX(CAST(itc.CreatedByID AS CHAR(36))) as uniqueidentifier),
		max(itc.CreatedByScreenID),
		max(itc.CreatedDateTime),
		CAST(MAX(CAST(itc.LastModifiedByID AS CHAR(36))) as uniqueidentifier),
		max(itc.LastModifiedByScreenID),
		max(itc.LastModifiedDateTime),
		max(itc.COGSAcctID),
		max(itc.COGSSubID),
		max(sq.AcctID),
		max(sq.SubID),
		0,
		1
		from INCostStatus ics 
		join INTranCost itc on ics.CompanyID = itc.CompanyID and ics.CostID = itc.CostID
		join INSetup insetup on ics.CompanyID = insetup.CompanyID and transitsiteid is not null
		join 
		(select it.companyid, it.doctype, it.refnbr, it.linenbr, it.AcctID, it.SubID	from INTran it
			join INTranSplit its on its.DocType = it.DocType and its.RefNbr = it.RefNbr and its.LineNbr = it.LineNbr and it.CompanyID = its.CompanyID
			left join INTransitLine tl on tl.CompanyID = it.CompanyID and tl.TransferNbr = it.RefNbr and tl.TransferLineNbr = it.LineNbr
			left join INTran it2 on it2.CompanyID=it.CompanyID and it2.OrigTranType = it.TranType and it2.OrigRefNbr = it.RefNbr and it2.OrigLineNbr = it.LineNbr and it2.released=1
			where it.Released = 1 and it.DocType = 'T' and it.InvtMult = -1 and its.TransferType='2' and tl.CompanyID is null and it2.CompanyID is null
			group by it.CompanyID, it.DocType, it.RefNbr, it.LineNbr, it.AcctID, it.SubID) sq on 
			itc.CompanyID = sq.companyid and itc.CostDocType = sq.DocType and itc.CostRefNbr = sq.RefNbr and itc.LineNbr = sq.LineNbr
		join INCostStatus icsnew on 
			icsnew.CompanyID = ics.CompanyID and icsnew.AccountID = sq.AcctID and icsnew.SubID = sq.SubID and icsnew.CostSiteID = insetup.TransitSiteID
			and icsnew.CostSubItemID = ics.CostSubItemID and icsnew.InventoryID = ics.InventoryID and icsnew.ReceiptNbr = sq.RefNbr and 
			(case when ics.ValMethod='S' then ics.LotSerialNbr else '' end) = icsnew.LotSerialNbr
		left join INTranCost positc on itc.CompanyID = positc.CompanyID and positc.TranType = itc.TranType and positc.RefNbr= itc.RefNbr 
		and positc.LineNbr = itc.LineNbr and positc.CostID = itc.CostID and positc.CostDocType = itc.CostDocType and positc.CostRefNbr = itc.CostRefNbr
		where positc.CompanyID is null
		group by 
			itc.CompanyID, itc.TranType, itc.RefNbr, itc.LineNbr, icsnew.CostID, itc.CostDocType, itc.CostRefNbr
			
  open transit_cursor
  
  fetch transit_cursor into 
	@companyID, @docType, @tranType, @refNbr, @lineNbr, @sOOrderType, @sOOrderNbr, @sOOrderLineNbr, @sOShipmentType, @sOShipmentNbr, @sOShipmentLineNbr, 
	@toSiteID, @siteID, @toLocationID, @lotSerTrack, @baseQty, @transitSiteID, @docNoteID 
  while @@fetch_status >= 0 
	begin

	UPDATE it set MaxTransferBaseQty = it.BaseQty from INTran it where it.OrigLineNbr= @lineNbr and it.OrigRefNbr = @refNbr and it.OrigTranType = @tranType and @companyID=it.CompanyID
	UPDATE its set MaxTransferBaseQty = its.BaseQty from INTranSplit its
		join INTran it on it.CompanyID=its.CompanyID and it.RefNbr = its.RefNbr and it.DocType = its.DocType and it.LineNbr = its.LineNbr
		where it.OrigLineNbr= @lineNbr and it.OrigRefNbr = @refNbr and it.OrigTranType = @tranType and @companyID=it.CompanyID
	
	if @sOOrderType is not null
		begin
		set @origModule = 'SO' 
		end
	else
		BEGIN
		set @origModule = 'IN' 
		END

	if @origModule = 'SO' begin
		UPDATE pol set MaxTransferBaseQty = pol.BaseReceiptQty from POReceiptLine pol where pol.OrigLineNbr= @lineNbr and pol.OrigRefNbr = @refNbr and pol.OrigTranType = @tranType and @companyID=pol.CompanyID
		UPDATE pols set MaxTransferBaseQty = pols.BaseQty from POReceiptLineSplit pols 
			join POReceiptLine pol on pol.CompanyID=pols.CompanyID and pol.ReceiptNbr = pols.ReceiptNbr and pol.ReceiptType = pols.ReceiptType and pol.LineNbr = pols.LineNbr
			where pol.OrigLineNbr= @lineNbr and pol.OrigRefNbr = @refNbr and pol.OrigTranType = @tranType and @companyID=pol.CompanyID
	end

	set @entityType = 'PX.Objects.IN.INTransitLine' 
	set @graphType = 'PX.Objects.IN.INTransferEntry' 
	

	set @noteID = NEWID()
	insert Note (NoteID, CompanyID, NoteText, EntityType, GraphType) values (@noteID, @companyID, '', @entityType, @graphType)
	insert INCostSite (CompanyID) values (@companyID)
	set @costSiteID = @@IDENTITY
	
	insert INTransitLine 
		(CompanyID,  
		CostSiteID,
		TransferNbr,
		"TransferLineNbr",
		"SOOrderType",
		"SOOrderNbr",
		"SOOrderLineNbr",
		"SOShipmentType",
		"SOShipmentNbr",
		"SOShipmentLineNbr",
		"OrigModule",
		"ToSiteID",
		"SiteID",
		"ToLocationID",
		"CreatedByID",
		"CreatedByScreenID",
		CreatedDateTime,
		"LastModifiedByID",
		"LastModifiedByScreenID",
		LastModifiedDateTime,
		NoteID,
		RefNoteID)
	values
		(
			@companyID,
			@costSiteID,
			@refNbr,
			@lineNbr,
			@sOOrderType,
			@sOOrderNbr,
			@sOOrderLineNbr,
			@sOShipmentType,
			@sOShipmentNbr,
			@sOShipmentLineNbr,
			@origModule,
			@toSiteID,
			@siteID,
			@toLocationID,
			'00000000-0000-0000-0000-000000000000',
			'00000000',
			GETDATE(),
			'00000000-0000-0000-0000-000000000000',
			'00000000',
			GETDATE(),
			@noteID,
			coalesce(@docNoteID, '')
		)
			
		INSERT INTO INLocationStatus(
				CompanyID,
				InventoryID,
				SubItemID,
				SiteID,
				LocationID,
				QtyOnHand,
				QtyAvail,
				QtyHardAvail,
				QtySOBackOrdered,QtySOPrepared,QtySOBooked,QtySOShipped,QtySOShipping,QtyINIssues,QtyINReceipts,QtyInTransit,QtyInTransitToSO,QtyPOReceipts,QtyPOPrepared,QtyPOOrders,QtySOFixed,QtyPOFixedOrders,QtyPOFixedPrepared,QtyPOFixedReceipts,QtySODropShip,QtyPODropShipOrders,QtyPODropShipPrepared,QtyPODropShipReceipts,QtyINAssemblySupply,QtyINAssemblyDemand,
				LastModifiedDateTime)
				select 
				sq.CompanyID,
				sq.InventoryID,
				sq.SubItemID,
				@transitSiteID,
				sq.CostSiteID,
				sum(sq.BaseQty),
				sum(sq.BaseQty),
				sum(sq.BaseQty),
				0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
				GETDATE()
				from
				(select tl.CompanyID, its.InventoryID, coalesce(its.SubItemID, it.SubItemID) as subItemID, its.BaseQty, tl.CostSiteID
				from INTransitLine tl
				join INTran it on it.CompanyID = tl.CompanyID and it.DocType='T' and it.RefNbr= tl.TransferNbr and it.LineNbr = tl.TransferLineNbr
				join INTranSplit its on its.CompanyID = tl.CompanyID and its.DocType = it.DocType and its.RefNbr = it.RefNbr and its.LineNbr=it.LineNbr
				where tl.CompanyID = @companyID and tl.costSiteID = @costSiteID) sq
				group by sq.CompanyID, sq.InventoryID, sq.SubItemID, sq.CostSiteID 
		
				INSERT INTO INLotSerialStatus
				(CompanyID,
				InventoryID,
				SubItemID,
				SiteID,
				LocationID,
				LotSerialNbr,
				LotSerTrack,
				QtyOnHand,
				QtyAvail,
				QtyHardAvail,
				ReceiptDate,
				QtySOBackOrdered,QtySOPrepared,QtySOBooked,QtySOShipped,QtySOShipping,QtyINIssues,QtyINReceipts,QtyInTransit,QtyInTransitToSO,QtyPOReceipts,QtyPOPrepared,QtyPOOrders,QtySOFixed,QtyPOFixedOrders,QtyPOFixedPrepared,QtyPOFixedReceipts,QtySODropShip,QtyPODropShipOrders,QtyPODropShipPrepared,QtyPODropShipReceipts,QtyINAssemblySupply,QtyINAssemblyDemand, 
				LastModifiedDateTime)
				select 
				its.companyID,
				its.inventoryID,
				its.subItemID,
				@transitSiteID,
				@costSiteID,
				its.lotSerialNbr,
				lsc.LotSerTrack,
				baseQty,
				baseQty,
				baseQty,
				lss.ReceiptDate,
				0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
				GETDATE()
				 from INTranSplit its 
				 join InventoryItem ii on ii.InventoryID = its.InventoryID and ii.CompanyID = its.CompanyID
				 join INTransitLine tl on tl.CompanyID = @companyID and tl.costSiteID = @costSiteID
				 join INLotSerClass lsc on lsc.LotSerClassID = ii.LotSerClassID and lsc.CompanyID = its.CompanyID and lsc.LotSerTrack<>'N' and lsc.LotSerAssign = 'R' 
				 join INLotSerialStatus lss on 
				 lss.CompanyID = its.CompanyID
				 and lss.inventoryID = its.InventoryID 
				 and lss.SubItemID = its.SubItemID 
				 and lss.LotSerialNbr = its.LotSerialNbr 
				 and lss.SiteID = its.SiteID 
				 and lss.LocationID = its.LocationID
				 where its.companyid = @companyID and its.docType = @docType and its.refnbr = @refNbr and its.linenbr = @lineNbr and its.LotSerialNbr is not null
			
		Update pl set OrigNoteID=@noteID	
		from INItemPlan pl
		join INTranSplit its on 
			its.InventoryID = pl.InventoryID and its.SubItemID = pl.SubItemID and its.CompanyID = pl.CompanyID and 
			its.RefNbr = @refNbr and its.LineNbr = @lineNbr and its.DocType = @docType
		join INTranSplit itsnew on itsnew.CompanyID = @companyID and itsnew.PlanID = pl.PlanID
		join INTran itnew on itnew.CompanyID = itsnew.CompanyID and itnew.DocType = itsnew.DocType and itnew.RefNbr = itsnew.RefNbr and itnew.LineNbr = itsnew.LineNbr
		and itnew.OrigTranType = @tranType and itnew.OrigRefNbr = @refNbr and itnew.OrigLineNbr = @lineNbr
		join INTransitLine loc on loc.CompanyID = @companyID and loc.NoteID = @noteID
		where pl.OrigNoteID = @docNoteID
		and pl.CompanyID = @companyID
		and PlanType in ('43','45')
			
	if @origModule = 'SO' begin
		Update pl set OrigNoteID=@noteID	
		from INItemPlan pl
		join INTranSplit its on 
			its.InventoryID = pl.InventoryID and its.SubItemID = pl.SubItemID and its.CompanyID = pl.CompanyID and 
			its.RefNbr = @refNbr and its.LineNbr = @lineNbr and its.DocType = @docType
		join POReceiptLineSplit posnew on posnew.CompanyID = @companyID and posnew.PlanID = pl.PlanID
		join POReceiptLine ponew on ponew.CompanyID = posnew.CompanyID and ponew.ReceiptType = posnew.ReceiptType and ponew.ReceiptNbr = posnew.ReceiptNbr and ponew.LineNbr = posnew.LineNbr
		and ponew.OrigTranType = @tranType and ponew.OrigRefNbr = @refNbr and ponew.OrigLineNbr = @lineNbr
		join INTransitLine loc on loc.CompanyID = @companyID and loc.NoteID = @noteID
		where pl.OrigNoteID = @docNoteID
		and pl.CompanyID = @companyID
		and PlanType in ('43','45')
	end
			
		Update pl set RefNoteID = @noteID 
		from INItemPlan pl  
		join INTransitLine loc on loc.CompanyID = @companyID and loc.NoteID = @noteID
		join INTranSplit its on 
			its.PlanID = pl.PlanID and its.CompanyID = pl.CompanyID and 
			its.RefNbr = @refNbr and its.LineNbr = @lineNbr and its.DocType = @docType
		where pl.CompanyID = @companyID and PlanType in ('42', '44')

		update its set planid = null 
			from INTranSplit its join INItemPlan pl on 
			its.PlanID = pl.PlanID and its.CompanyID = pl.CompanyID and 
			its.RefNbr = @refNbr and its.LineNbr = @lineNbr and its.DocType = @docType
			join INTransitLine loc on loc.CompanyID = @companyID and loc.NoteID = @noteID
			where its.CompanyID = @companyID and PlanType in ('42', '44')

  fetch transit_cursor into 
	@companyID, @docType, @tranType, @refNbr, @lineNbr, @sOOrderType, @sOOrderNbr, @sOOrderLineNbr, @sOShipmentType, @sOShipmentNbr, @sOShipmentLineNbr, 
	@toSiteID, @siteID, @toLocationID, @lotSerTrack, @baseQty, @transitSiteID, @docNoteID 
  end
	
	insert into INSiteStatus 
	(
			CompanyID,
			InventoryID,
			SubItemID,
			SiteID,
			QtyOnHand,
			QtyAvail,
			QtyHardAvail,
			QtySOBackOrdered,QtySOPrepared,QtySOBooked,QtySOShipped,QtySOShipping,QtyINIssues,QtyINReceipts,QtyInTransit,QtyInTransitToSO,QtyPOReceipts,QtyPOPrepared,QtyPOOrders,QtySOFixed,QtyPOFixedOrders,QtyPOFixedPrepared,QtyPOFixedReceipts,QtySODropShip,QtyPODropShipOrders,QtyPODropShipPrepared,QtyPODropShipReceipts,QtyINAssemblySupply,QtyINAssemblyDemand, QtyNotAvail, QtyINReplaned,
			LastModifiedDateTime
	)
	select 
		ils.CompanyID,
		ils.InventoryID,
		ils.SubItemID,
		max(insetup.TransitSiteID),
		0,
		0,
		0,
		0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
		GETDATE()
	from INLocationStatus ils
	join INSetup insetup on ils.CompanyID = insetup.CompanyID and insetup.transitSiteID is not null 
	left join INSiteStatus iss on iss.CompanyID = ils.CompanyID and iss.SiteID = insetup.TransitSiteID and iss.InventoryID = ils.InventoryID and iss.SubItemID = ils.SubItemID
	where ils.siteid = insetup.TransitSiteID and iss.CompanyID is null
	group by ils.CompanyID, ils.InventoryID, ils.SubItemID

	UPDATE 
	iss
	SET
		iss.QtyOnHand = sq.QtyOnHand,
		iss.QtyAvail = sq.QtyAvail,
		iss.QtyHardAvail = sq.QtyHardAvail
	from
	INSiteStatus iss
	JOIN INSetup ins ON iss.CompanyID = ins.CompanyID AND ins.transitSiteID = iss.SiteID
	JOIN
	(SELECT ils.CompanyID, ils.SiteID, ils.InventoryID, ils.SubItemID, SUM(ils.QtyOnHand) AS QtyOnHand, SUM(ils.QtyAvail) AS QtyAvail, SUM(ils.QtyHardAvail) AS QtyHardAvail FROM INLocationStatus ils
	GROUP BY ils.CompanyID, ils.SiteID, ils.InventoryID, ils.SubItemID) sq
	ON iss.CompanyID = sq.CompanyID AND
	iss.SiteID = sq.SiteID AND 
	iss.InventoryID = sq.InventoryID AND
	iss.SubItemID = sq.SubItemID	
	
close transit_cursor
deallocate transit_cursor	
end
GO

--[MinVersion(Hash = 3ee580cdfc6c9924aaa440f50f0a0303f02aebaab22cad889860bb42bc09bd6b)]
UPDATE 
	SMEmail
SET 
	RefNoteID = NoteID 
WHERE 
	RefNoteID IS NULL
GO

--Delete workbooks from the sitemap
--[MinVersion(Hash = 74ccefb9761202d7fadf1b4d01e878fbb61f5f4cb5b8a6889412034641495c23)]
DELETE SiteMap 
FROM SiteMap 
INNER JOIN GLWorkBook 
	ON GLWorkBook.CompanyID=SiteMap.CompanyID 
	AND GLWorkBook.SitemapID=SiteMap.NodeID
GO

--[MinVersion(Hash = 546875d0a88cd1f54a9a0aa05ff2dea07b29424d3118fa385ce3362b524252d9)]
UPDATE p SET
      PrintCheck = 1
FROM APPayment p
     LEFT JOIN PaymentMethod pm ON p.PaymentMethodID = pm.PaymentMethodID
                                   AND p.CompanyID = pm.CompanyID
WHERE p.PrintCheck IS NULL
      AND (pm.APPrintChecks = 1
           OR pm.APCreateBatchPayment = 1)
GO

--Update [LastCostDate] if it is null.
--[MinVersion(Hash = 4a867da5c7f509b8dc3357ce13216c9ac8e51a6a2453f7bee11477088633e892)]
UPDATE INItemStats 
SET INItemStats.LastCostDate = '1900-01-01 00:00:00' 
FROM INItemStats 
WHERE INItemStats.LastCostDate is null
GO

--[MinVersion(Hash = 5c1cd26ce17b1ccf4a189f059308e8b0f2741288133a1210dd7045747523bb5a)]
UPDATE APPaymentChargeTran
	SET APPaymentChargeTran.Released=1
FROM APPaymentChargeTran
INNER JOIN APRegister
	on APPaymentChargeTran.CompanyID=APRegister.CompanyID
	AND APPaymentChargeTran.DocType=APRegister.DocType
	AND APPaymentChargeTran.RefNbr=APRegister.RefNbr
	AND APRegister.DeletedDatabaseRecord=0
	AND APRegister.Released=1
GO

--[MinVersion(Hash = 2551cc662f87ee9ea4fbe88ba3c9dd4115d0e2b49d6923c4a358d9bf288257cf)]
UPDATE ARPaymentChargeTran
	SET ARPaymentChargeTran.Released=1
FROM ARPaymentChargeTran
INNER JOIN ARRegister
	on ARPaymentChargeTran.CompanyID=ARRegister.CompanyID
	AND ARPaymentChargeTran.DocType=ARRegister.DocType
	AND ARPaymentChargeTran.RefNbr=ARRegister.RefNbr
	AND ARRegister.DeletedDatabaseRecord=0
	AND ARRegister.Released=1
GO

--[MinVersion(Hash = 3624bd768088d64ac74a6a14bc6a9e36bbe854c9e8bdd58ce2b87e5219e4d1fe)]
delete CATran from Batch 
inner join GLTran on 
Batch.CompanyID=GLTran.CompanyID and 
Batch.Module=GLTran.Module and 
Batch.BatchNbr=GLTran.BatchNbr  
inner join CATran on 
Batch.CompanyID=CATran.CompanyID and 
GLTran.CATranID=CATran.TranID
where Batch.DeletedDatabaseRecord=0 and (Batch.Scheduled=1 OR Batch.Voided=1)
and CATran.Released=0 and CATran.Reconciled=0;
GO

--[MinVersion(Hash = a28ff86524b45d7dbf275911068c38da2fb1bc9cd6c6ee7f34b73f3f8c0ebcc2)]
update GLTran set CATranID = Null from GLTran
inner join Batch on 
Batch.CompanyID=GLTran.CompanyID and 
Batch.Module=GLTran.Module and 
Batch.BatchNbr=GLTran.BatchNbr  
left join CATran on 
Batch.CompanyID=CATran.CompanyID and 
GLTran.CATranID=CATran.TranID
where Batch.DeletedDatabaseRecord=0 and (Batch.Scheduled=1 OR Batch.Voided=1) 
and CATran.TranID is null and GLTran.CATranID is not null;
GO

--[MinVersion(Hash = 546875d0a88cd1f54a9a0aa05ff2dea07b29424d3118fa385ce3362b524252d9)]
UPDATE p SET
      PrintCheck = 1
FROM APPayment p
     LEFT JOIN PaymentMethod pm ON p.PaymentMethodID = pm.PaymentMethodID
                                   AND p.CompanyID = pm.CompanyID
WHERE p.PrintCheck IS NULL
      AND (pm.APPrintChecks = 1
           OR pm.APCreateBatchPayment = 1)
GO

--Update AUStepAction - deactivate OnSuccess for Open step (Shipments)
--[MinVersion(Hash = 28e4a87b7bc0738355fcbfc4fe4689798c5e41fdc7d436130e70889fe064451b)]
UPDATE AUStepAction SET IsSuccessActive = 0 WHERE 
ScreenID = 'SO302000' and StepID = 'Open' AND ActionName = 'Action' AND SuccessStepID = 'SO Open' AND SuccessScreenID = 'SO301000' AND SuccessActionName = 'OnShipment@Flow'
GO

--Update [LastCostDate] if it is null.
--[MinVersion(Hash = 4a867da5c7f509b8dc3357ce13216c9ac8e51a6a2453f7bee11477088633e892)]
UPDATE INItemStats 
SET INItemStats.LastCostDate = '1900-01-01 00:00:00' 
FROM INItemStats 
WHERE INItemStats.LastCostDate is null
GO

--[MinVersion(Hash = 21e03dc4d76af957019ac9882e102c5232ff2ad4cf6c212695f295da2837b6d8)]
UPDATE TaxTran
SET
    CuryOrigTaxableAmt = 0.0,
    OrigTaxableAmt = 0.0
WHERE CuryOrigTaxableAmt IS NULL OR OrigTaxableAmt IS NULL
GO

--[MinVersion(Hash = 826def9332b85a8c386d482e935c5f71190542c4c2b8d6c1c04cb6b7e28a4528)]
UPDATE CATax
SET
    CuryOrigTaxableAmt = 0.0,
    OrigTaxableAmt = 0.0
WHERE CuryOrigTaxableAmt IS NULL OR OrigTaxableAmt IS NULL
GO

--[MinVersion(Hash = d37ee9d389012b925197c725268d2fc33296e0ddbc81eba481b6197c6c873927)]
UPDATE ARTax
SET
    CuryOrigTaxableAmt = 0.0,
    OrigTaxableAmt = 0.0
WHERE CuryOrigTaxableAmt IS NULL OR OrigTaxableAmt IS NULL
GO

--[MinVersion(Hash = 7155f6cd61f1d153521aed588d85ed2cfc7e6cd21de4b7fe0b6d523a0b3bebb7)]
UPDATE APTax
SET
    CuryOrigTaxableAmt = 0.0,
    OrigTaxableAmt = 0.0
WHERE CuryOrigTaxableAmt IS NULL OR OrigTaxableAmt IS NULL
GO

-- Update Tasks
--[MinVersion(Hash = 6a74c1a3f79ff3b94febd6a6260df7d3d7f797e68edcc9dd66ac78ef816ac314)]
UPDATE CRActivity SET
	AllDay = 1,
	StartDate = CONVERT(date, StartDate),
	EndDate = CONVERT(date, EndDate)
	WHERE ClassID = 0 AND ISNULL(AllDay, 0) = 0 -- CRActivityClass.Task == 0
GO

--[OldHash(Hash = ab232b9b4cec97e9368a099376d0e02079d68518c7cf845134ade799a7abf407)]
--[MinVersion(Hash = 641251ad1b9ef4a7e2dd3d4c4709e49ab7610e9103ea1ee0b6a98b1ea5382a80)]
update it set it.IsCostUnmanaged=1
from intran it
join inventoryitem ii on it.CompanyID=ii.CompanyID and it.InventoryID = ii.InventoryID
left join poreceiptline prl on prl.CompanyID = it.CompanyID and prl.ReceiptNbr = it.POReceiptNbr and prl.LineNbr = it.POReceiptLineNbr
where it.trantype in ('RCA', 'INV') and ii.StkItem = 0
	or it.trantype='ADJ' and prl.LineType in ('GP', 'NP')
GO

--[MinVersion(Hash = 19da88965772f0444263d3e80fad93b438976812c9d345cbf1b5065d45f8b2fd)]
if exists(select * from sys.columns where object_id = object_id('POSetup') and name = 'VendorPriceUpdate') 
and exists(select * from sys.columns where object_id = object_id('APSetup') and name = 'VendorPriceUpdate')
begin
EXEC sp_executesql N'
update APSetup
set APSetup.VendorPriceUpdate = POSetup.VendorPriceUpdate
from POSetup
where POSetup.CompanyID = APSetup.CompanyID
'
end
GO

--[MinVersion(Hash = 8bebc6439de526d22b8d1f4ce2c0638b9a85d5950b8f140d8d2a614f13a169eb)]
UPDATE Account
SET IsCashAccount=1
FROM Account
INNER JOIN CashAccount
  ON Account.CompanyID = CashAccount.CompanyID
  AND Account.AccountID = CashAccount.AccountID
WHERE IsCashAccount=0
GO

--#26 Missing Note records (POLine)
--[MinVersion(Hash = a6d6286f4e4caf4e193a82ec4d64f148ebcaeeee634b681e87cea05baa81f034)]
insert into Note (CompanyID, NoteText, GraphType, EntityType, NoteID)
select po.CompanyID, '', 'PX.Objects.PO.POOrderEntry', 'PX.Objects.PO.POOrder', po.NoteID 
from POLine p 
inner join POOrder po on po.CompanyID = p.CompanyID and po.OrderType = p.OrderType and po.OrderNbr = p.OrderNbr
inner join INItemPlan i on p.CompanyID = i.CompanyID and p.PlanID = i.PlanID
left join Note n on n.CompanyID = po.CompanyID and n.NoteID = po.NoteID
where n.NoteID is null and po.CompanyID > 0
group by po.CompanyID, po.OrderType, po.OrderNbr, po.NoteID;
GO

--#27 Missing Note records (POReceiptLineSplit)
--[MinVersion(Hash = 129df8ef571bf1a39ba8731b81326938fb11e95a218d0ca40f645dc011aac2d5)]
insert into Note (CompanyID, NoteText, GraphType, EntityType, NoteID)
select po.CompanyID, '', 'PX.Objects.PO.POReceiptEntry', 'PX.Objects.PO.POReceipt', po.NoteID 
from POReceiptLineSplit p 
inner join POReceipt po on po.CompanyID = p.CompanyID and po.ReceiptNbr = p.ReceiptNbr
inner join INItemPlan i on p.CompanyID = i.CompanyID and p.PlanID = i.PlanID
left join Note n on n.CompanyID = p.CompanyID and n.NoteID = po.NoteID
where n.NoteID is null and po.CompanyID > 0
group by po.CompanyID, po.ReceiptNbr, po.NoteID;
GO

--#28 Missing Note records (INTranSplit)
--[MinVersion(Hash = 99a5f15aa582360aae123ca175ab97412855730111a769ef1f2ef7dddba9b83a)]
insert into Note (CompanyID, NoteText, GraphType, EntityType, NoteID)
select po.CompanyID, '', 'PX.Objects.IN.INIssueEntry', 'PX.Objects.IN.INRegister', po.NoteID 
from INTranSplit p 
inner join INRegister po on po.CompanyID = p.CompanyID and po.DocType = p.DocType and po.RefNbr = p.RefNbr
inner join INItemPlan i on p.CompanyID = i.CompanyID and p.PlanID = i.PlanID
left join Note n on n.CompanyID = po.CompanyID and n.NoteID = po.NoteID
where po.DocType = 'I' and n.NoteID is null and po.CompanyID > 0
group by po.CompanyID, po.DocType, po.RefNbr, po.NoteID;
GO

--[MinVersion(Hash = 7f02492289a89cf8498c07c90183f7c04f8819767f75ef7ebc0dcb03dff24ba7)]
insert into Note (CompanyID, NoteText, GraphType, EntityType, NoteID)
select po.CompanyID, '', 'PX.Objects.IN.INReceiptEntry', 'PX.Objects.IN.INRegister', po.NoteID from 
INTranSplit p 
inner join INRegister po on po.CompanyID = p.CompanyID and po.DocType = p.DocType and po.RefNbr = p.RefNbr
inner join INItemPlan i on p.CompanyID = i.CompanyID and p.PlanID = i.PlanID
left join Note n on n.CompanyID = po.CompanyID and n.NoteID = po.NoteID
where po.DocType = 'R' and n.NoteID is null and p.CompanyID > 0
group by po.CompanyID, po.DocType, po.RefNbr, po.NoteID;
GO

--[MinVersion(Hash = d9310324044ccacd866e2ab672efe389efef9f87ce5349509d80a69ff1620a40)]
insert into Note (CompanyID, NoteText, GraphType, EntityType, NoteID)
select po.CompanyID, '', 'PX.Objects.IN.INTransferEntry', 'PX.Objects.IN.INRegister', po.NoteID 
from INTranSplit p 
inner join INRegister po on po.CompanyID = p.CompanyID and po.DocType = p.DocType and po.RefNbr = p.RefNbr
inner join INItemPlan i on p.CompanyID = i.CompanyID and p.PlanID = i.PlanID
left join Note n on n.CompanyID = p.CompanyID and n.NoteID = po.NoteID
where po.DocType = 'T' and n.NoteID is null and po.CompanyID > 0
group by po.CompanyID, po.DocType, po.RefNbr, po.NoteID;
GO

--#29 Missing Note records (SOShipLineSplit)
--[MinVersion(Hash = 7ae7b3c2126725aef9d3b51787193fa5a9b54626ea0619eedd68f725c486715e)]
insert into Note (CompanyID, NoteText, GraphType, EntityType, NoteID)
select po.CompanyID, '', 'PX.Objects.SO.SOShipmentEntry', 'PX.Objects.SO.SOShipment', po.NoteID 
from SOShipLineSplit p 
inner join SOShipment po on po.CompanyID = p.CompanyID and po.ShipmentNbr = p.ShipmentNbr
inner join INItemPlan i on p.CompanyID = i.CompanyID and p.PlanID = i.PlanID
left join Note n on n.CompanyID = p.CompanyID and n.NoteID = po.NoteID
where n.NoteID is null and po.CompanyID > 0 and po.ShipmentType != 'H'
group by po.CompanyID, po.ShipmentNbr, po.NoteID;
GO

--#30 Missing Note records (SOLineSplit)
--[MinVersion(Hash = 0cb9a228d5487320d96c35322a338be8245e705438136376c64c15ea3c7d66ea)]
insert into Note (CompanyID, NoteText, GraphType, EntityType, NoteID)
select po.CompanyID, '', 'PX.Objects.SO.SOOrderEntry', 'PX.Objects.SO.SOOrder', po.NoteID 
from SOLineSplit p 
inner join SOOrder po on po.CompanyID = p.CompanyID and po.OrderType = p.OrderType and po.OrderNbr = p.OrderNbr
inner join INItemPlan i on p.CompanyID = i.CompanyID and p.PlanID = i.PlanID
left join Note n on n.CompanyID = po.CompanyID and n.NoteID = po.NoteID
where n.NoteID is null and po.CompanyID > 0
group by po.CompanyID, po.OrderType, po.OrderNbr, po.NoteID;
GO

--Updating Summary TimeXXX on freezed timecards. TimeXXX fields has been just added.
--[MinVersion(Hash = ce8bb883fb2240042e5e03ee57169917e1561a33073ac7680f5d13bb72758a10)]
UPDATE t
SET t.[TimeSpent] = TimeSpentTotal,
    t.[OvertimeSpent] = OvertimeSpentTotal,
    t.[TimeBillable] = TimeBillableTotal,
    t.[OvertimeBillable] = OvertimeBillableTotal
FROM EPTimeCard t INNER JOIN ( 
    SELECT 
	   a.CompanyID,
	   a.TimeCardCD, 
	   SUM(TimeSpent) AS TimeSpentTotal, 
	   SUM([OvertimeSpent]) AS OvertimeSpentTotal, 
	   SUM([TimeBillable]) AS TimeBillableTotal, 
	   SUM([OvertimeBillable]) AS OvertimeBillableTotal 
    FROM PMTimeActivity a
    WHERE a.TimeCardCD IS NOT NULL
    GROUP BY a.CompanyID, a.TimeCardCD
     ) Totals ON t.CompanyID = Totals.CompanyID AND t.TimeCardCD = Totals.TimeCardCD
WHERE t.IsHold = 0 AND t.[TimeSpent] IS NULL
GO

--[SmartExecute]
--[MinVersion(Hash = 3764a808fcaec4903b65a50fa831c479ece58751eac626c0b8103942536ce5cc)]
UPDATE EPActivityType SET NoteID = NEWID() WHERE NoteID IS NULL

INSERT EPActivityTypeKVExt (CompanyID, RecordID, FieldName, ValueString)
SELECT l.CompanyID, t.NoteID, 'Description' + MAX(UPPER(SUBSTRING(l.LocaleName, 1, 2))), MAX(t.Description)
FROM EPActivityType t
INNER JOIN Locale l ON l.IsDefault = 1
LEFT JOIN EPActivityTypeKVExt e ON e.CompanyID = l.CompanyID AND e.RecordID = t.NoteID AND e.FieldName = 'Description' + UPPER(SUBSTRING(l.LocaleName, 1, 2))
WHERE e.CompanyID IS NULL
GROUP BY l.CompanyID, t.NoteID

INSERT CRCaseClassKVExt (CompanyID, RecordID, FieldName, ValueString)
SELECT l.CompanyID, t.NoteID, 'Description' + MAX(UPPER(SUBSTRING(l.LocaleName, 1, 2))), MAX(t.Description)
FROM CRCaseClass t
INNER JOIN Locale l ON l.IsDefault = 1
LEFT JOIN CRCaseClassKVExt e ON e.CompanyID = l.CompanyID AND e.RecordID = t.NoteID AND e.FieldName = 'Description' + UPPER(SUBSTRING(l.LocaleName, 1, 2))
WHERE e.CompanyID IS NULL
GROUP BY l.CompanyID, t.NoteID

UPDATE PaymentMethodDetail SET NoteID = NEWID() WHERE NoteID IS NULL

INSERT PaymentMethodDetailKVExt (CompanyID, RecordID, FieldName, ValueString)
SELECT l.CompanyID, t.NoteID, 'Descr' + MAX(UPPER(SUBSTRING(l.LocaleName, 1, 2))), MAX(t.Descr)
FROM PaymentMethodDetail t
INNER JOIN Locale l ON l.IsDefault = 1
LEFT JOIN PaymentMethodDetailKVExt e ON e.CompanyID = l.CompanyID AND e.RecordID = t.NoteID AND e.FieldName = 'Descr' + UPPER(SUBSTRING(l.LocaleName, 1, 2))
WHERE e.CompanyID IS NULL
GROUP BY l.CompanyID, t.NoteID
GO

--[MinVersion(Hash = 006706ccc2095a14f381091107ecaa008042c3bbed1996bf107f1e57eaa570da)]
INSERT ContractItemKVExt (CompanyID, RecordID, FieldName, ValueString)
SELECT l.CompanyID, t.NoteID, 'Descr' + MAX(UPPER(SUBSTRING(l.LocaleName, 1, 2))), MAX(t.Descr)
FROM ContractItem t
INNER JOIN Locale l ON l.IsDefault = 1
LEFT JOIN ContractItemKVExt e ON e.CompanyID = l.CompanyID AND e.RecordID = t.NoteID AND e.FieldName = 'Descr' + UPPER(SUBSTRING(l.LocaleName, 1, 2))
WHERE e.CompanyID IS NULL
GROUP BY l.CompanyID, t.NoteID
GO

--[MinVersion(Hash = 77138e5d0dec48ff3f3b1033d47f9e401ea7ac8dddb52710d1cde4d46c408878)]
INSERT ContractKVExt (CompanyID, RecordID, FieldName, ValueString)
SELECT l.CompanyID, t.NoteID, 'Description' + MAX(UPPER(SUBSTRING(l.LocaleName, 1, 2))), MAX(t.Description)
FROM Contract t
INNER JOIN Locale l ON l.IsDefault = 1
LEFT JOIN ContractKVExt e ON e.CompanyID = l.CompanyID AND e.RecordID = t.NoteID AND e.FieldName = 'Description' + UPPER(SUBSTRING(l.LocaleName, 1, 2))
WHERE e.CompanyID IS NULL
GROUP BY l.CompanyID, t.NoteID
GO

--[MinVersion(Hash = ad95713cde1ff85ec3c7d097ef5d54a75761419cc2fd3e575ace7cdad611c34b)]
UPDATE ContractDetail SET NoteID = NEWID() WHERE NoteID IS NULL

INSERT ContractDetailKVExt (CompanyID, RecordID, FieldName, ValueString)
SELECT l.CompanyID, t.NoteID, 'Description' + MAX(UPPER(SUBSTRING(l.LocaleName, 1, 2))), MAX(t.Description)
FROM ContractDetail t
INNER JOIN Locale l ON l.IsDefault = 1
LEFT JOIN ContractDetailKVExt e ON e.CompanyID = l.CompanyID AND e.RecordID = t.NoteID AND e.FieldName = 'Description' + UPPER(SUBSTRING(l.LocaleName, 1, 2))
WHERE e.CompanyID IS NULL
GROUP BY l.CompanyID, t.NoteID
GO

--[OldHash(Hash = bce3ca22308a4f0bf6d8fe8ea6ddac9f5be941faf8a40842f11f696d18b80e6b)]
--[IfExists(Column = SOOrderShipment.NoteID)]
--[MinVersion(Hash = 8c621fe83c179450d64361c558cb73b1ac0b5d824be3cb0b86afc3902f8a5752)]
insert SOOrderShipment(CompanyID, ShipmentNbr, OrderType, OrderNbr, CustomerID, CustomerLocationID, ShipAddressID , ShipContactID, 
LineCntr, ShipDate, ProjectID, Hold, ShipComplete, ShipmentQty, 
CreatedByID, CreatedByScreenID, CreatedDateTime, LastModifiedByID, LastModifiedByScreenID, LastModifiedDateTime, 
SiteID, Confirmed, Operation, ShipmentWeight, ShipmentVolume, LineTotal, ShipmentType, InvoiceReleased, 
CreateINDoc, NoteID, ShipmentNoteID, ShippingRefNoteID)
select rn.CompanyID, rn.ReceiptNbr, sn.OrderType, sn.OrderNbr, max(so.CustomerID), max(so.CustomerLocationID), max(so.ShipAddressID), max(so.ShipContactID),
0, max(rd.ReceiptDate), null, 0, 'C', sum(rn.ReceiptQty),
'00000000-0000-0000-0000-000000000000', max(rd.CreatedByScreenID), max(rd.CreatedDateTime), '00000000-0000-0000-0000-000000000000', max(rd.LastModifiedByScreenID), max(rd.LastModifiedDateTime),
null, 1, 'I', sum(rn.ExtWeight), sum(rn.ExtVolume), 0, 'H', 0,
1, so.NoteID, null, '00000000-0000-0000-0000-000000000000'
from 
POReceiptLine rn
inner join POReceipt rd on rd.CompanyID = rn.CompanyID and rd.ReceiptNbr = rn.ReceiptNbr and rd.ReceiptType = rn.ReceiptType
inner join SOLineSplit sn on sn.CompanyID = rn.CompanyID and sn.POType = rn.POType and sn.PONbr = rn.PONbr and sn.POLineNbr = rn.POLineNbr
inner join SOOrder so on so.CompanyID = sn.CompanyID and so.OrderType = sn.OrderType and so.OrderNbr = sn.OrderNbr
left join SOOrderShipment os on os.CompanyID = rn.CompanyID and os.ShipmentType = 'H' and os.ShipmentNbr = rn.ReceiptNbr and os.OrderType = sn.OrderType and os.OrderNbr = sn.OrderNbr
where (rn.LineType = 'GP' or rn.LineType = 'NP') and rd.Released = 1 and os.ShipmentNbr is null
group by rn.CompanyID, rn.ReceiptNbr, sn.OrderType, sn.OrderNbr, so.NoteID
GO

--[MinVersion(Hash = 76408903ac4594fb6bbe154a7dcaeb255bd496ae080e66c2ba911faef9d14802)]
update asa set ProcessingGraphName='PX.Objects.PO.POCreate', ProcessingScreenID = 'PO505000' from AUStepAction asa
join AUStepFill asf on
asa.Companyid = asf.CompanyID and
asa.ScreenID = asf.ScreenID and asa.StepID = asf.StepID and asf.ActionName='Action' and asf.FieldName = '@actionID' and asf.Value = '5'
and asa.MenuText=asf.MenuText
where  asa.ScreenID='SO301000' and asa.StepID = 'SO Open'
and asa.ProcessingGraphName is null
GO

--[MinVersion(Hash = 68d8718006f7f26a8269d78d05da7d985060c7ce873c153a78c772c13c5b74a3)]
delete from INItemXRef where AlternateId=''
GO

-- Script to fix data inconsistency caused by AC-81993
--[MinVersion(Hash = d28d7256db6ff8e33ee43c940298c35d88966d42619ee3ee1353bcd99bde3b22)]
UPDATE APPaymentChargeTran
	SET APPaymentChargeTran.TranPeriodID=APRegister.TranPeriodID,
	APPaymentChargeTran.FinPeriodID=APRegister.FinPeriodID,
	APPaymentChargeTran.TranDate=APRegister.DocDate
FROM APPaymentChargeTran
INNER JOIN APRegister
	ON APPaymentChargeTran.CompanyID=APRegister.CompanyID
	AND APPaymentChargeTran.DocType=APRegister.DocType
	AND APPaymentChargeTran.RefNbr=APRegister.RefNbr
	AND APRegister.DeletedDatabaseRecord=0
	AND APRegister.Released=1
	AND (APPaymentChargeTran.TranDate <> APRegister.DocDate
	OR APPaymentChargeTran.FinPeriodID <> APRegister.FinPeriodID
	OR APPaymentChargeTran.TranPeriodID <> APRegister.TranPeriodID);
GO

-- Script to fix data inconsistency caused by AC-81993
--[MinVersion(Hash = f059f3e8a3d19f6888c2e464531889a9e6bc82ccd8cf6b851211dbb366959b16)]
UPDATE ARPaymentChargeTran
	SET ARPaymentChargeTran.TranPeriodID=ARRegister.TranPeriodID,
	ARPaymentChargeTran.FinPeriodID=ARRegister.FinPeriodID,
	ARPaymentChargeTran.TranDate=ARRegister.DocDate
FROM ARPaymentChargeTran
INNER JOIN ARRegister
	ON ARPaymentChargeTran.CompanyID=ARRegister.CompanyID
	AND ARPaymentChargeTran.DocType=ARRegister.DocType
	AND ARPaymentChargeTran.RefNbr=ARRegister.RefNbr
	AND ARRegister.DeletedDatabaseRecord=0
	AND ARRegister.Released=1
	AND (ARPaymentChargeTran.TranDate <> ARRegister.DocDate
	OR ARPaymentChargeTran.FinPeriodID <> ARRegister.FinPeriodID
	OR ARPaymentChargeTran.TranPeriodID <> ARRegister.TranPeriodID);
GO

-- Script to fix data inconsistency caused by AC-82000
--[MinVersion(Hash = af5b5939708946e18ff3508b58ce89918620169c9488777336ac8672dc7e587e)]
UPDATE CATran
	SET CATran.Released=ARPaymentChargeTran.Released,
	CATran.FinPeriodID=ARPaymentChargeTran.FinPeriodID,
	CATran.TranPeriodID=ARPaymentChargeTran.TranPeriodID,
	CATran.TranDate=ARPaymentChargeTran.TranDate,
	CATran.CuryTranAmt=GLTran.CuryDebitAmt-GLTran.CuryCreditAmt,
	CATran.TranAmt=GLTran.DebitAmt-GLTran.CreditAmt
FROM CATran 
INNER JOIN ARPaymentChargeTran
	ON ARPaymentChargeTran.CompanyID=CATran.CompanyID
	AND ARPaymentChargeTran.CashTranID=CATran.TranID
	AND ARPaymentChargeTran.CashAccountID IS NOT NULL
	AND CATran.OrigModule='AR'
	AND CATran.OrigTranType=ARPaymentChargeTran.DocType
	AND CATran.OrigRefNbr=ARPaymentChargeTran.RefNbr
	AND ARPaymentChargeTran.Released=1
	AND CATran.Released=0
	AND ARPaymentChargeTran.Consolidate=0
INNER JOIN GLTran
	ON GLTran.CompanyID=CATran.CompanyID
	AND GLTran.CATranID=CATran.TranID;
GO

-- Script to fix data consistency issue caused by AC-84571
--[MinVersion(Hash = 9c84e61097aa64ef5d3fb2e4224351ec0eb9db7cd71447b41faef799bfb96448)]
update CATran
set CATran.Reconciled=0, CATran.ReconNbr=Null, CATran.ReconDate=Null
FROM CATran
left join CARecon 
	on CATran.CompanyID=CARecon.CompanyID 
	and CATran.CashAccountId = CARecon.CashAccountId
	and CATran.ReconNbr=CARecon.ReconNbr 
	and CARecon.Voided =0 
	and (CARecon.DeletedDatabaseRecord=0 or CARecon.DeletedDatabaseRecord is null)
where CATran.ReconNbr is not null and CARecon.ReconNbr is Null;
GO

-- Script to fix data consistency issue caused by AC-84571
--[MinVersion(Hash = 2b96ebbd0a294b72eafc2aa5588e2700655ca6b2662d4846e7d0f98ad8e2bc98)]
update CABatch
set CABatch.Reconciled=0, CABatch.ReconNbr=Null, CABatch.ReconDate=Null
FROM CABatch
	left join CARecon 
	on CABatch.CompanyID=CARecon.CompanyID 
	and CABatch.CashAccountId = CARecon.CashAccountId
	and CABatch.ReconNbr=CARecon.ReconNbr 
	and CARecon.Voided =0 
where CABatch.ReconNbr is not null and CARecon.ReconNbr is Null;
GO

--[OldHash(Hash = bce3ca22308a4f0bf6d8fe8ea6ddac9f5be941faf8a40842f11f696d18b80e6b)]
--[IfExists(Column = SOOrderShipment.NoteID)]
--[MinVersion(Hash = 8c621fe83c179450d64361c558cb73b1ac0b5d824be3cb0b86afc3902f8a5752)]
insert SOOrderShipment(CompanyID, ShipmentNbr, OrderType, OrderNbr, CustomerID, CustomerLocationID, ShipAddressID , ShipContactID, 
LineCntr, ShipDate, ProjectID, Hold, ShipComplete, ShipmentQty, 
CreatedByID, CreatedByScreenID, CreatedDateTime, LastModifiedByID, LastModifiedByScreenID, LastModifiedDateTime, 
SiteID, Confirmed, Operation, ShipmentWeight, ShipmentVolume, LineTotal, ShipmentType, InvoiceReleased, 
CreateINDoc, NoteID, ShipmentNoteID, ShippingRefNoteID)
select rn.CompanyID, rn.ReceiptNbr, sn.OrderType, sn.OrderNbr, max(so.CustomerID), max(so.CustomerLocationID), max(so.ShipAddressID), max(so.ShipContactID),
0, max(rd.ReceiptDate), null, 0, 'C', sum(rn.ReceiptQty),
'00000000-0000-0000-0000-000000000000', max(rd.CreatedByScreenID), max(rd.CreatedDateTime), '00000000-0000-0000-0000-000000000000', max(rd.LastModifiedByScreenID), max(rd.LastModifiedDateTime),
null, 1, 'I', sum(rn.ExtWeight), sum(rn.ExtVolume), 0, 'H', 0,
1, so.NoteID, null, '00000000-0000-0000-0000-000000000000'
from 
POReceiptLine rn
inner join POReceipt rd on rd.CompanyID = rn.CompanyID and rd.ReceiptNbr = rn.ReceiptNbr and rd.ReceiptType = rn.ReceiptType
inner join SOLineSplit sn on sn.CompanyID = rn.CompanyID and sn.POType = rn.POType and sn.PONbr = rn.PONbr and sn.POLineNbr = rn.POLineNbr
inner join SOOrder so on so.CompanyID = sn.CompanyID and so.OrderType = sn.OrderType and so.OrderNbr = sn.OrderNbr
left join SOOrderShipment os on os.CompanyID = rn.CompanyID and os.ShipmentType = 'H' and os.ShipmentNbr = rn.ReceiptNbr and os.OrderType = sn.OrderType and os.OrderNbr = sn.OrderNbr
where (rn.LineType = 'GP' or rn.LineType = 'NP') and rd.Released = 1 and os.ShipmentNbr is null
group by rn.CompanyID, rn.ReceiptNbr, sn.OrderType, sn.OrderNbr, so.NoteID
GO

--[MinVersion(Hash = 1e0698df856df1409e8d22412e88bb710c89b94e08809fe8fa84b5f3d35ed880)]
if (exists(select * from sys.columns where object_id = object_id('ARSetup') and name = 'IgnoreDiscountsIfPriceDefined')
and exists(select * from sys.columns where object_id = object_id('ARSetup') and name = 'ApplyLineDiscountsIfCustomerPriceDefined')) begin
EXEC sp_executesql N'
UPDATE ARSetup set ApplyLineDiscountsIfCustomerPriceDefined = 
CASE IgnoreDiscountsIfPriceDefined WHEN 0 THEN 1 ELSE 0 
END'
end
GO

--[MinVersion(Hash = 76408903ac4594fb6bbe154a7dcaeb255bd496ae080e66c2ba911faef9d14802)]
update asa set ProcessingGraphName='PX.Objects.PO.POCreate', ProcessingScreenID = 'PO505000' from AUStepAction asa
join AUStepFill asf on
asa.Companyid = asf.CompanyID and
asa.ScreenID = asf.ScreenID and asa.StepID = asf.StepID and asf.ActionName='Action' and asf.FieldName = '@actionID' and asf.Value = '5'
and asa.MenuText=asf.MenuText
where  asa.ScreenID='SO301000' and asa.StepID = 'SO Open'
and asa.ProcessingGraphName is null
GO

--[MinVersion(Hash = 68d8718006f7f26a8269d78d05da7d985060c7ce873c153a78c772c13c5b74a3)]
delete from INItemXRef where AlternateId=''
GO

--[MinVersion(Hash = 5b41bbeecc15c07e9bcddc8b5e20b09feaafdbf7b986955f9ab61d5b5e59f1a7)]
update CABankTran
set CuryApplAmtCA = 0
where	Processed = 0 and
		not exists (
			select * from CABankTranDetail detail
			where	CABankTran.TranID = detail.BankTranID and 
					CABankTran.CompanyID = detail.CompanyID)

update CABankTran
set CuryApplAmt = 0
where	Processed = 0 and
		not exists (
			select * from CABankTranAdjustment adj 
			where	CABankTran.TranID = adj.TranID and 
					CABankTran.CompanyID = adj.CompanyID)
GO

--[MinVersion(Hash = 76408903ac4594fb6bbe154a7dcaeb255bd496ae080e66c2ba911faef9d14802)]
update asa set ProcessingGraphName='PX.Objects.PO.POCreate', ProcessingScreenID = 'PO505000' from AUStepAction asa
join AUStepFill asf on
asa.Companyid = asf.CompanyID and
asa.ScreenID = asf.ScreenID and asa.StepID = asf.StepID and asf.ActionName='Action' and asf.FieldName = '@actionID' and asf.Value = '5'
and asa.MenuText=asf.MenuText
where  asa.ScreenID='SO301000' and asa.StepID = 'SO Open'
and asa.ProcessingGraphName is null
GO

--[MinVersion(Hash = 68d8718006f7f26a8269d78d05da7d985060c7ce873c153a78c772c13c5b74a3)]
delete from INItemXRef where AlternateId=''
GO

--[MinVersion(Hash = 147f76b959afba0a5f0f81fababe7dd63cd1705dada3c55c6fdfa8327d201708)]
UPDATE ARTran SET SortOrder = LineNbr WHERE SortOrder IS NULL
GO

--[MinVersion(Hash = 874af771310367a2e5aa18624283a8189733de5bda611f6750721b0393613ff1)]
if exists(select * from AppSchema where ARInvoiceDisableFlagRemoved is null or ARInvoiceDisableFlagRemoved = 0)
begin
--this statement is senseless and remains here only because of autotests
Update Appschema Set ARInvoiceDisableFlagRemoved=1  
end
GO

--[MinVersion(Hash = 7a6d116ffa9345b5dd2c8e9ddbec5b53f53e7622895a139070872284fd49f59f)]
if exists(select * from AppSchema where ARInvoiceDisableFlagTrueRemoved is null or ARInvoiceDisableFlagTrueRemoved = 0)
begin
update AUStepField set IsDisabled=0 where tablename='PX.Objects.AR.ARInvoice' and fieldname='<TABLE>' and stepid='Released' and ScreenID='SO303000'

Update Appschema Set ARInvoiceDisableFlagTrueRemoved=1
end
GO

--[MinVersion(Hash = 0437e6f65920c82ad398ad0d950a420783a57a100f2f496e032fd92cd7cd6df5)]
UPDATE SOLine SET SortOrder = LineNbr WHERE SortOrder IS NULL
GO

--[MinVersion(Hash = fdda03484478e82d2c1b7092a0ee4fba05c9c66a78ee7321b9b7a7f7b984be30)]
UPDATE POLine SET SortOrder = LineNbr WHERE SortOrder IS NULL
GO

--[MinVersion(Hash = ccac5a88cc145b787490231dddf2e173e9a42eaace3d7542676c1c3bf7e04700)]
UPDATE POReceiptLine SET SortOrder = LineNbr WHERE SortOrder IS NULL
GO

--[MinVersion(Hash = 8abe1281b48f0dd0b4ba85875a906d738de42ffad13e74976a5db023c7bb82d7)]
UPDATE APTran SET SortOrder = LineNbr WHERE SortOrder IS NULL
GO

-- Sets defaul value for Branch.DefaultRUTROTType
--[MinVersion(Hash = 98e0bc240c9bf281beda018a13590f4d1e8dd7dd90464c6dd425ada2d801dd88)]
update Branch
set DefaultRUTROTType = 'U'
where DefaultRUTROTType is null
GO

--[MinVersion(Hash = 507789ffcded5368cc5308411a79a3df25ebb78e419425a6117cbebcc9b0fd6a)]
update POReceiptLine
set OrigPlanType =
	case
		when LineType in ('GI', 'GR') then '70'
		when LineType = 'GS' then '76'
		when LineType = 'GP' then '74'
		else null
	end
from POReceiptLine
where OrigPlanType is null and PONbr is not null and Released <> 1

update rls
set OrigPlanType = rl.OrigPlanType
from POReceiptLineSplit rls
inner join POReceiptLine rl on rl.CompanyID = rls.CompanyID and rl.ReceiptType = rls.ReceiptType
	and rl.ReceiptNbr = rls.ReceiptNbr and rl.LineNbr = rls.LineNbr
where rls.OrigPlanType is null and rl.OrigPlanType is not null and rl.Released <> 1

update pl
set pl.OrigPlanLevel = 0, pl.OrigPlanType = rls.OrigPlanType
from INItemPlan pl
inner join POReceiptLineSplit rls on rls.CompanyID = pl.CompanyID and rls.PlanID = pl.PlanID
where pl.OrigPlanType is null and rls.OrigPlanType is not null
GO

--[IfExists(Column = INItemPlan.IgnoreOrigPlan)]
--[MinVersion(Hash = 66e21f186b95651b354cd710cfc4a7c2bc6795df077a92d3570ea4817b661c1f)]
update pl
set pl.IgnoreOrigPlan =
	case
		when sls.IsComponentItem = 1 or isnull(ls.LotSerialNbr, '') <> '' and ls.LotSerialNbr <> sls.LotSerialNbr then 1
		else 0
	end
from INItemPlan pl
inner join SOShipLineSplit sls on sls.CompanyID = pl.CompanyID and sls.PlanID = pl.PlanID
left join SOLineSplit ls on ls.CompanyID = sls.CompanyID and ls.OrderType = sls.OrigOrderType
	and ls.OrderNbr = sls.OrigOrderNbr and ls.LineNbr = sls.OrigLineNbr and ls.SplitLineNbr = sls.OrigSplitLineNbr
GO

--[MinVersion(Hash = f1d1a19662a26db39c11a544f3e0c3483db0c25b66aadf391780e5c7b3abde05)]
UPDATE
  APRegister
SET
  APRegister.OrigModule = 'EP'
WHERE
  APRegister.CreatedByScreenID IN ('EP301030','EP301000','EP501000')
  AND APRegister.OrigModule = 'AP'
GO

--Add WasReleased field to GLBudgetLine to be able to distinguish those, which were never released from those edited after release
--[MinVersion(Hash = 7ef0472d11eac23fa07ab37df9e559fd10230c2ca5575da44ac628424317b2fa)]
update l
  set l.WasReleased = 1
  from GLBudgetLine l
inner join GLBudgetLineDetail d
  on l.CompanyID = d.CompanyID
  and l.BranchID = d.BranchID
  and l.LedgerID = d.LedgerID
  and l.FinYear = d.FinYear
  and l.GroupID = d.GroupID
where d.ReleasedAmount <> 0;
GO

--[MinVersion(Hash = 2c385dfe676a66e59bc1832087422fec5d598d5f2f886d34ebe4b7b8462224bd)]
update ins set TransitBranchID = sit.BranchID 
from insetup ins join insite sit on 
ins.companyid = sit.CompanyID and
ins.transitsiteid = sit.SiteID
where ins.TransitBranchID is null
GO

--[MinVersion(Hash = 835a026d405914ed0810a58c705d08864e82ff20159ea3e1b8ef6eaacac33217)]
update itc set  
itc.OversoldQty = iitc.OversoldQty,
itc.OversoldTranCost = iitc.OversoldTranCost
from INTranCost itc
join INTran it on 
itc.CompanyID = it.CompanyID and
itc.TranType = it.TranType and
itc.RefNbr = it.RefNbr and 
itc.LineNbr = it.LineNbr
join
(select 
companyid,
CostID,
TranType,
RefNbr,
LineNbr,
IsOversold,
sum(trancost) OversoldTranCost,
sum(qty) OversoldQty
from INTranCost 
where IsOversold=1
group by 
companyid,
CostID,
TranType,
RefNbr,
LineNbr,
IsOversold) iitc on
iitc.CompanyID = itc.CompanyID and 
iitc.CostID = itc.CostID and 
iitc.TranType = itc.trantype and 
iitc.RefNbr = itc.refnbr and 
iitc.LineNbr = itc.LineNbr and 
iitc.IsOversold  = itc.IsOversold
where itc.IsOversold = 1 
and (itc.OversoldQty is null or itc.OversoldTranCost is null)
and itc.CostRefNbr = itc.RefNbr 
and itc.CostDocType = it.DocType
GO

--[MinVersion(Hash = 5b41bbeecc15c07e9bcddc8b5e20b09feaafdbf7b986955f9ab61d5b5e59f1a7)]
update CABankTran
set CuryApplAmtCA = 0
where	Processed = 0 and
		not exists (
			select * from CABankTranDetail detail
			where	CABankTran.TranID = detail.BankTranID and 
					CABankTran.CompanyID = detail.CompanyID)

update CABankTran
set CuryApplAmt = 0
where	Processed = 0 and
		not exists (
			select * from CABankTranAdjustment adj 
			where	CABankTran.TranID = adj.TranID and 
					CABankTran.CompanyID = adj.CompanyID)
GO

--[MinVersion(Hash = 7409c7c8cbd324e3c4a9d85505908d276e52e52177710ae3b572523dee7e1c4d)]
update itch set SiteID = ins.siteid from INItemCostHist itch
join insite ins on itch.companyid = ins.companyid and itch.costsiteid = ins.siteid
where itch.siteid is null

update itch set SiteID = inl.siteid from INItemCostHist itch
join INLocation inl on itch.companyid = inl.companyid and itch.costsiteid = inl.locationid
where itch.siteid is null

update itch set SiteID = CostSiteID from INItemCostHist itch
where itch.siteid is null
GO

--[MinVersion(Hash = 8f41edb8e0995ed9ec2c3ffa4fce9a1ce648c7bd93f911ae22589c52f18eacf7)]
UPDATE INReceiptStatus set OrigQty = QtyOnHand where OrigQty=0
GO

--Add WasReleased field to GLBudgetLine to be able to distinguish those, which were never released from those edited after release
--[MinVersion(Hash = 7ef0472d11eac23fa07ab37df9e559fd10230c2ca5575da44ac628424317b2fa)]
update l
  set l.WasReleased = 1
  from GLBudgetLine l
inner join GLBudgetLineDetail d
  on l.CompanyID = d.CompanyID
  and l.BranchID = d.BranchID
  and l.LedgerID = d.LedgerID
  and l.FinYear = d.FinYear
  and l.GroupID = d.GroupID
where d.ReleasedAmount <> 0;
GO

--[MinVersion(Hash = 396fc5aa08b92a52613f5064637e740328b6f8094637f9d86f3f0ed972a2873f)]
update POOrder
set
    PayToVendorID = VendorID
where PayToVendorID is null
GO

--[MinVersion(Hash = b9ed0765fa1ff74ee94645cdae5c8ae9eaad0fac1e2cf44cce1e9487b6467f86)]
update i
set
    SuppliedByVendorID = r.VendorID
from APInvoice i
left join APRegister r
on i.CompanyID = r.CompanyID
    and i.DocType = r.DocType
    and i.RefNbr = r.RefNbr
where SuppliedByVendorID is null
GO

--[MinVersion(Hash = 1808b1bb12853bc1459c121b471c2fc327ab6e2ce92300bc50004c90b381fdad)]
update i
set
    SuppliedByVendorLocationID = r.VendorLocationID
from APInvoice i
left join APRegister r
on i.CompanyID = r.CompanyID
    and i.DocType = r.DocType
    and i.RefNbr = r.RefNbr
where SuppliedByVendorLocationID is null
GO

--[IfExists(Column = Country.AllowStateEdit)]
--[MinVersion(Hash = 7265bd924744422537c86a15d5ed22da3c5d6263389d1702cb2005b546bf8e55)]
UPDATE Country
SET
	CountryValidationMethod = 'I',
	StateValidationMethod = CASE AllowStateEdit
		WHEN 1 THEN 'X' ELSE 'I'
	END
WHERE
	CountryValidationMethod IS NULL

--[IfExists(Column = Notification.NotificationID)]
--[MinVersion(Branch = 6.10, Version = 6.10.0841, Hash = 2bfedc1beffb6f0dd9588a9ac747bb559aec2372fc2252bde6d78671492310ba)]
--[MinVersion(Branch = 6.20, Version = 6.20.0098, Hash = 2bfedc1beffb6f0dd9588a9ac747bb559aec2372fc2252bde6d78671492310ba)]
delete q
FROM Notification q
INNER JOIN(
SELECT *, (
  SELECT TOP (1) CompanyNotificationID
  FROM Notification n2
  WHERE n2.NotificationID = n1.NotificationID
    AND n2.CompanyID = n1.CompanyID
  ORDER BY n2.LastModifiedDateTime DESC
   ,n2.CompanyNotificationID DESC
  ) as OriginalID
FROM Notification n1 ) D
ON
  q.CompanyID = D.CompanyID and
  q.CompanyNotificationID = D.CompanyNotificationID AND 
  q.CompanyNotificationID != D.OriginalID
GO

--[MinVersion(Hash = 417049e1f8168f86ea7a35c085e184c93e650d3e9ca478abba877f6afc658f5b)]
update SOSetup
set SalesProfitabilityForNSKits = 'C'
where SalesProfitabilityForNSKits is null
GO

--[MinVersion(Hash = f2871e1f505cb73851d8cc366485ec300b7445d738ab8438da2733830ae06dca)]
UPDATE DiscountSequenceDetail SET PendingAmount = 0.0 WHERE PendingAmount IS NULL;
GO

--[MinVersion(Hash = a8200b00677323a6b04b305b6503aef93f18f32ba1c2a4ef90ce6f5592d5e5d2)]
UPDATE DiscountSequenceDetail SET PendingQuantity = 0.0 WHERE PendingQuantity IS NULL;
GO

--[MinVersion(Hash = 4af3773eb5332c63ce4ae4c996b432ff57bd4931159ec6d34c94c8742ba601a9)]
UPDATE  DiscountSequenceDetail SET PendingFreeItemQty = 0.0 WHERE PendingFreeItemQty IS NULL;
GO

--[MinVersion(Hash = 95592ddd3d7d2c670e541d2e7df24314cb8b5d2bbaf5ce0777ffb60e460b1685)]
INSERT INTO DRSetup (CompanyID, ScheduleNumberingID)
SELECT DISTINCT DRSchedule.CompanyID, 'DRSCHEDULE'
FROM DRSchedule 
LEFT JOIN DRSetup ON DRSchedule.CompanyID = DRSetup.CompanyID
WHERE DRSetup.CompanyID IS NULL
GO

--[MinVersion(Hash = ad6f22b57a75a5139f8cd4d4b2c8c9a3b11a7e22f21c85770e7b0773a8088b1d)]
UPDATE DRSchedule SET ScheduleNbr = RIGHT('00000000' + CAST(ScheduleID AS NVARCHAR(8)), 8);
GO

--[MinVersion(Hash = a4eb67260c23395a745b4b3b2a604dbb62613641144efec73070a6ac3c85c9fd)]
update APSetup set ValidateDataConsistencyOnRelease = 0;
GO

--[MinVersion(Hash = a906659e6b3186e66fe858861550784eae76accbdcbd8b4cbff5827c81296973)]
update ARSetup set ValidateDataConsistencyOnRelease = 0;
GO

--[MinVersion(Hash = f267c12f419791611e4acac2b905ed2645678678ba03bf668a087d66039951db)]
insert INPIStatusLoc(CompanyID, SiteID, LocationID, PIID, Active, 
CreatedByID, CreatedByScreenID, CreatedDateTime, 
LastModifiedByID, LastModifiedByScreenID,LastModifiedDateTime)
select s.CompanyID, s.SiteID, s.LocationID, s.PIID, max(convert(smallint, s.Active)), 
'00000000-0000-0000-0000-000000000000', max(s.CreatedByScreenID), max(s.CreatedDateTime), 
'00000000-0000-0000-0000-000000000000', max(s.LastModifiedByScreenID),max(s.LastModifiedDateTime)
from INPIStatus s
left join INPIStatusLoc n on n.CompanyID = s.CompanyID and n.PIID = s.PIID and n.LocationID = s.LocationID 
where n.PIID is null
group by s.CompanyID, s.SiteID, s.LocationID, s.PIID
GO

--[MinVersion(Hash = ae15bb40a9f2fd2e88368405b081ec30409e13aff7a10eeedabe5697efe9c4f1)]
insert INPIStatusItem(CompanyID, SiteID, InventoryID, PIID, Active, 
CreatedByID, CreatedByScreenID, CreatedDateTime, 
LastModifiedByID, LastModifiedByScreenID,LastModifiedDateTime)
select s.CompanyID, s.SiteID, s.InventoryID, s.PIID, max(convert(smallint, s.Active)), 
'00000000-0000-0000-0000-000000000000', max(s.CreatedByScreenID), max(s.CreatedDateTime), 
'00000000-0000-0000-0000-000000000000', max(s.LastModifiedByScreenID),max(s.LastModifiedDateTime)
from INPIStatus s
left join INPIStatusItem n on n.CompanyID = s.CompanyID and n.PIID = s.PIID and n.InventoryID is null
where s.InventoryID is null and n.PIID is null 
group by s.CompanyID, s.SiteID, s.InventoryID, s.PIID
GO

--Filling default names for stages
--[IfExists(Column = CROpportunityProbability.Name)]
--[MinVersion(Hash = a92225382f71c0e46049d27c80bb8d97a72cd3fec8afd9a35edbdc4337d6330d)]
UPDATE [CROpportunityProbability]
SET [Name] = ISNULL(NULLIF([Name],''),
    CASE [StageCode]
        WHEN 'L' THEN 'Prospect'
        WHEN 'N' THEN 'Nurture'
        WHEN 'P' THEN 'Qualification'
        WHEN 'Q' THEN 'Development'
        WHEN 'V' THEN 'Solution'
        WHEN 'A' THEN 'Proof'
        WHEN 'R' THEN 'Negotiation'
        WHEN 'W' THEN 'Won'
        ELSE 'Custom Stage'
    END),
	[SortOrder] = ISNULL([SortOrder], [Probability])
GO

--Adding all stages as active for the current classes
--[SmartExecute]
--[IfExists(Table = CROpportunityClassProbability)]
--[IfNotExistsSelect(From = CROpportunityClassProbability)]
--[MinVersion(Hash = 8760ff0de938e20ac8573171c6410c977f070835114357f737ad82f5a436f4a0)]
INSERT INTO CROpportunityClassProbability
	(CompanyID,ClassID,StageID,CreatedByID,CreatedByScreenID,CreatedDateTime,LastModifiedByID,LastModifiedByScreenID,LastModifiedDateTime)
	SELECT DISTINCT
		c.CompanyID,
		c.CROpportunityClassID,
		p.StageCode,
		'00000000-0000-0000-0000-000000000000',
		'00000000',
		GETDATE(),
		'00000000-0000-0000-0000-000000000000',
		'00000000',
		GETDATE()
	FROM CROpportunityProbability p
	JOIN CROpportunityClass c ON 1 = 1
GO

--Tax Period Closing by Branch
--[MinVersion(Hash = 60bf1ca8ca540fa67f48d5337be4ee4e2937709de590d43359f7007f05104c2c)]
UPDATE b
	SET b.ParentBranchID = COALESCE(l.DefBranchID, b.BranchID)
		FROM Branch b
		LEFT JOIN Ledger l
			ON (b.CompanyID = l.CompanyID
					AND b.LedgerID = l.LedgerID);
GO

--[MinVersion(Hash = e51117e5a824f5bd8c70830664dcce480fe4141772d58ad3cc1bf4eea4c8d603)]
UPDATE TaxYear 
		SET TaxPeriodType = (SELECT v.TaxPeriodType 
										FROM Vendor v 
										WHERE v.BAccountID = TaxYear.VendorID
												AND v.CompanyID = TaxYear.CompanyID)
		WHERE TaxPeriodType IS NULL;
GO

--[MinVersion(Hash = efc4a56731b65b7743ea5a0cf3df626b8f004ea10c26e47c81e82a5a8a69c6c1)]
UPDATE Vendor
		SET AutoGenerateTaxBill = 1
		WHERE AutoGenerateTaxBill IS NULL;
GO

--End of Tax Period Closing by Branch
--[MinVersion(Hash = ad22c27d3c0c07732164b36ae7550c808bcd0ec5c1abdb19bbf909b40038f307)]
update Dimension
set LookupMode = case when Validate = 0 then 'SA' else 'K0' end
GO

--[SmartExecute]
--[IfExists(Column = INItemClass.RemovedItemClassID)]
--[IfExists(Column = INItemClass.ItemClassID)]
--[IfExists(Column = INItemClass.ItemClassCD)]
--[MinVersion(Hash = d4ef0936148a1cf1193a33272f2031369e44bcc2be1c006424aaec1fd7fda089)]
UPDATE INItemClass
SET ItemClassCD = RemovedItemClassID

UPDATE csis
SET ItemClassID = c.ItemClassID
FROM INItemClassSubItemSegment csis
INNER JOIN INItemClass c ON c.RemovedItemClassID = csis.RemovedItemClassID

UPDATE u
SET ItemClassID = c.ItemClassID
FROM INUnit u
INNER JOIN INItemClass c ON c.RemovedItemClassID = u.RemovedItemClassID

UPDATE INUnit
SET ItemClassID = 0
WHERE ItemClassID IS NULL

UPDATE cr
SET ItemClassID = c.ItemClassID
FROM INItemClassRep cr
INNER JOIN INItemClass c ON c.RemovedItemClassID = cr.RemovedItemClassID

UPDATE s
SET DfltStkItemClassID = c.ItemClassID
FROM INSetup s
INNER JOIN INItemClass c ON c.RemovedItemClassID = s.DfltItemClassID AND c.StkItem = 1

UPDATE s
SET DfltNonStkItemClassID = c.ItemClassID
FROM INSetup s
INNER JOIN INItemClass c ON c.RemovedItemClassID = s.DfltItemClassID AND c.StkItem = 0

UPDATE l
SET PrimaryItemClassID = c.ItemClassID
FROM INLocation l
INNER JOIN INItemClass c ON c.RemovedItemClassID = l.RemovedPrimaryItemClassID

UPDATE i
SET ItemClassID = c.ItemClassID
FROM InventoryItem i
INNER JOIN INItemClass c ON c.RemovedItemClassID = i.RemovedItemClassID

INSERT INTO INPIClassItemClass (CompanyID, PIClassID, ItemClassID,
		CreatedByID, CreatedByScreenID, CreatedDateTime, LastModifiedByID, LastModifiedByScreenID, LastModifiedDateTime)
SELECT pic.CompanyID, pic.PIClassID, c.ItemClassID,
		pic.CreatedByID, pic.CreatedByScreenID, pic.CreatedDateTime, pic.LastModifiedByID, pic.LastModifiedByScreenID, pic.LastModifiedDateTime
FROM INPIClass pic
INNER JOIN INItemClass c ON c.RemovedItemClassID = pic.ItemClassID
WHERE pic.Method = 'C'
GO

--Update attribute definition to new key
--[mysql: Skip]
--[IfExists(Column = INItemClass.ItemClassCD)]
--[MinVersion(Hash = 3218ff80d0d48f7b88b8c3f690b300ef13ee7ad5b97c859096d60c0ad4466173)]
UPDATE CSAttributeGroup 
SET EntityClassID = C.ItemClassID
FROM CSAttributeGroup G
     INNER JOIN INItemClass C ON C.CompanyID = G.CompanyID AND C.ItemClassCD = G.EntityClassID
WHERE EntityType = 'PX.Objects.IN.InventoryItem'
GO

--Separate script - workaround for MySql bug
--[mysql: Native]
--[mssql: Skip]
--[IfExists(Column = INItemClass.ItemClassCD)]
--[OldHash(Hash = ce44b3f17b8e797470e132d9255680db0d3d238a0942a49f4eadad2b3b416f8b)]
--[OldHash(Hash = 3218ff80d0d48f7b88b8c3f690b300ef13ee7ad5b97c859096d60c0ad4466173)]
--[MinVersion(Hash = 090a4c9da50f8a514b6a19a61ae4a5cec744fe96e54040f6c8ce139d247ddd68)]
UPDATE `CSAttributeGroup` `G`
INNER JOIN `INItemClass` `C` ON `C`.`CompanyID` = `G`.`CompanyID` AND `C`.`ItemClassCD` = `G`.`EntityClassID`
SET `G`.`EntityClassID` = CONCAT('~', `C`.`ItemClassID`)
WHERE `G`.`EntityType` = 'PX.Objects.IN.InventoryItem';

UPDATE `CSAttributeGroup`
SET `EntityClassID` = SUBSTRING(`EntityClassID`, 2)
WHERE `EntityType` = 'PX.Objects.IN.InventoryItem' AND `EntityClassID` LIKE '~%';
GO

-- Move Availability Settings from INItemClass into a separate entity - INAvailabilityScheme
--[IfExists(Column = INItemClass.ItemClassCD)]
--[IfExists(Column = INItemClass.AvailabilitySchemeID)]
--[IfExists(Column = INItemClass.InclQtyINIssues)]
--[IfExists(Table = INAvailabilityScheme)]
--[MinVersion(Hash = 3d068c71a79ecc7e9f90e0cdc219cbc890913fa1f96e003f841864c7975ec72f)]
INSERT INTO INAvailabilityScheme (CompanyID, AvailabilitySchemeID, Description,
		InclQtySOBackOrdered, InclQtySOPrepared, InclQtySOBooked, InclQtySOShipped, InclQtySOShipping, InclQtyInTransit, InclQtyPOReceipts,
		InclQtyPOPrepared, InclQtyPOOrders, InclQtyINIssues, InclQtyINReceipts, InclQtyINAssemblyDemand, InclQtyINAssemblySupply, InclQtySOReverse,
		CreatedByID, CreatedByScreenID, CreatedDateTime, LastModifiedByID, LastModifiedByScreenID, LastModifiedDateTime)
SELECT CompanyID, ItemClassCD, Descr,
		InclQtySOBackOrdered, 0, InclQtySOBooked, InclQtySOShipped, InclQtySOShipping, InclQtyInTransit, InclQtyPOReceipts,
		InclQtyPOPrepared, InclQtyPOOrders, InclQtyINIssues, InclQtyINReceipts, InclQtyINAssemblyDemand, InclQtyINAssemblySupply, InclQtySOReverse,
		CreatedByID, CreatedByScreenID, CreatedDateTime, LastModifiedByID, LastModifiedByScreenID, LastModifiedDateTime
FROM INItemClass
WHERE StkItem = 1

UPDATE INItemClass
SET AvailabilitySchemeID = ItemClassCD
WHERE StkItem = 1
	AND AvailabilitySchemeID IS NULL
GO

-- Separate script for updating INAvailabilityScheme.InclQtySOPrepared because it was added only in 5.3.x
--[IfExists(Column = INItemClass.ItemClassCD)]
--[IfExists(Column = INItemClass.AvailabilitySchemeID)]
--[IfExists(Column = INItemClass.InclQtySOPrepared)]
--[IfExists(Table = INAvailabilityScheme)]
--[MinVersion(Hash = d2b919cba063bf8b0eefdd384860d9853318c18d8ea55dadd703a21dd4e2c9d4)]
UPDATE s 
SET InclQtySOPrepared = c.InclQtySOPrepared
FROM INAvailabilityScheme s
INNER JOIN INItemClass c ON s.CompanyID = c.CompanyID AND s.AvailabilitySchemeID = c.AvailabilitySchemeID
GO

--Update Automation steps so that Release of produced IN Document is invoked on success of SO Invoice Release
--[OldHash(Hash = cdbaa6fa028ea2751b26e087fd38b43fc3cb295408f885a4c21c8af54684ab30)]
--[MinVersion(Hash = e2f89e7df8c938fd80c8a07ec3ba1de608fa70f5f2ffbde319cdbf773845ea79)]
INSERT INTO [AUStepAction] (CompanyID, ScreenID, StepID, RowNbr,
							IsActive, ActionName, IsAutomatic, IsDefault, IsDisabled, BatchMode, AutoSave, SplitByValues,
							IsRetryActive, RetryScreenID, RetryStepID, RetryCntr, IsSuccessActive, SuccessScreenID, SuccessStepID, IsFailActive, FailScreenID, FailStepID,
							CreatedByID, CreatedByScreenID, CreatedDateTime, LastModifiedByID, LastModifiedByScreenID, LastModifiedDateTime, CompanyMask, NoteID)
SELECT	st.CompanyID, st.ScreenID, st.StepID, (SELECT ISNULL(MAX(RowNbr), 0) FROM [AUStepAction] WHERE ScreenID = st.ScreenID AND StepID = st.StepID) + 1,
		1, 'Release', 0, 1, 0, 1, 0, 0,
		0, st.ScreenID, st.StepID, 0, 0, st.ScreenID, st.StepID, 0, st.ScreenID, st.StepID, 
		st.CreatedByID, st.CreatedByScreenID, st.CreatedDateTime, st.LastModifiedByID, st.LastModifiedByScreenID, st.LastModifiedDateTime, st.CompanyMask, NEWID()
FROM [AUStep] st
WHERE st.ScreenID = 'IN302000' AND st.StepID = 'Balanced'
	AND NOT EXISTS (SELECT * FROM [AUStepAction] WHERE CompanyID = st.CompanyID AND ScreenID = st.ScreenID AND StepID = st.StepID AND ActionName = 'Release')

UPDATE [AUStepAction]
SET IsSuccessActive = 1, SuccessScreenID = 'IN302000', SuccessStepID = 'Balanced', SuccessActionName = 'Release'
WHERE ScreenID = 'SO303000' AND StepID IN ('Balanced', 'Pending Print') AND ActionName = 'Action' AND MenuText = 'Release'
GO

--Populate QtyActual column of the IN Status tables
--[IfExists(Column = INSiteStatus.QtyActual)]
--[IfExists(Column = INLocationStatus.QtyActual)]
--[IfExists(Column = INLotSerialStatus.QtyActual)]
--[IfExists(Column = INItemLotSerial.QtyActual)]
--[IfExists(Column = INSiteLotSerial.QtyActual)]
--[MinVersion(Hash = 77ed67f343f6ea83ac53f5b02b395e0663c5aaa48c8fed4cdcd78340b7095e61)]
UPDATE [INSiteStatus]
SET QtyActual = QtyOnHand - QtySOShipped - QtyNotAvail

UPDATE [INLocationStatus]
SET QtyActual = QtyOnHand - QtySOShipped

UPDATE [INLotSerialStatus]
SET QtyActual = QtyOnHand - QtySOShipped

UPDATE ils
SET QtyActual = QtyOnHand - ISNULL(QtySOShippedSum, 0)
FROM [INItemLotSerial] AS ils
LEFT JOIN (
	SELECT CompanyID, InventoryID, LotSerialNbr, SUM(QtySOShipped) AS QtySOShippedSum
	FROM [INLotSerialStatus]
	GROUP BY CompanyID, InventoryID, LotSerialNbr) as lssa
ON ils.CompanyID = lssa.CompanyID AND ils.InventoryID = lssa.InventoryID AND ils.LotSerialNbr = lssa.LotSerialNbr

UPDATE sls
SET QtyActual = QtyOnHand - ISNULL(QtySOShippedSum, 0)
FROM [INSiteLotSerial] AS sls
LEFT JOIN (
	SELECT CompanyID, InventoryID, SiteID, LotSerialNbr, SUM(QtySOShipped) AS QtySOShippedSum
	FROM [INLotSerialStatus]
	GROUP BY CompanyID, InventoryID, SiteID, LotSerialNbr) as lssa
ON sls.CompanyID = lssa.CompanyID AND sls.InventoryID = lssa.InventoryID AND sls.SiteID = lssa.SiteID AND sls.LotSerialNbr = lssa.LotSerialNbr
GO

--Populate Original Note ID of SO Shipped plans with the Note ID of the source SO Order
--[MinVersion(Hash = 3b8469d5582ccf0807d8bd499372a881b984b701692ea8402eccc26f39d988f4)]
UPDATE p
SET p.OrigNoteID = o.NoteID
FROM [INItemPlan] p
INNER JOIN [SOShipLineSplit] s
ON p.CompanyID = s.CompanyID AND p.PlanID = s.PlanID
INNER JOIN [SOOrder] o
ON s.CompanyID = o.CompanyID AND s.OrigOrderType = o.OrderType AND s.OrigOrderNbr = o.OrderNbr
WHERE p.PlanType = '62'
GO

--Populate Inventory Note ID of SO Order Shipments
--[MinVersion(Hash = 293260a428361d39827f24d72714344750e266f33d99f00b03567b138ba68f91)]
UPDATE os
SET os.InvtNoteID = r.NoteID
FROM [SOOrderShipment] os
INNER JOIN [INRegister] r
ON os.CompanyID = r.CompanyID AND os.InvtDocType = r.DocType AND os.InvtRefNbr = r.RefNbr
GO

--Note ID of SO Order Shipment should be equal to the Note ID of the source SO Order
--[IfExists(Column = SOOrderShipment.NoteID)]
--[MinVersion(Hash = a726847cc85189ddde99ce2a95e7b1960859aaedf617f20c6ef73ac66e857fa3)]
UPDATE os
SET os.NoteID = o.NoteID
FROM [SOOrderShipment] os
INNER JOIN [SOOrder] o
ON os.CompanyID = o.CompanyID AND os.OrderType = o.OrderType AND os.OrderNbr = o.OrderNbr
WHERE os.NoteID <> o.NoteID
GO

--[IfExists(Column = CRMarketingListMember.Activated)]
--[MinVersion(Hash = f4ae3ea2dcac72107bd90305ce9839cff9f3c37b8803ab3bb9b3e55c547a3552)]
UPDATE 
	CRMarketingListMember
SET
	IsSubscribed = ISNULL(Activated, 0)
GO

--[MinVersion(Hash = 9607cc3c7138ffa7d21d7ce4496420f66fd1aa681793fc63e54773888fda431a)]
UPDATE 
	CRMarketingList
SET
	IsActive = ISNULL(IsActive, 0),
	IsDynamic = ISNULL(IsDynamic, 0),
	NoFax = ISNULL(NoFax, 0),
	NoMail = ISNULL(NoMail, 0),
	NoMarketing = ISNULL(NoMarketing, 0),
	NoCall = ISNULL(NoCall, 0),
	NoEMail = ISNULL(NoEMail, 0),
	NoMassMail = ISNULL(NoMassMail, 0)
GO

--[OldHash(Hash = a07634f3f2151c027bcc7eca3747f8e9e5b6d1c50278cd1a4999856daf86d0a4)]
--[MinVersion(Hash = 6943c5e72a1354d76e881106578eab12be3051639320558e9f70dd3c479c7402)]
if (exists(select * from sys.columns where object_id = object_id('FeaturesSet') and name = 'Inventory')
and exists(select * from sys.columns where object_id = object_id('FeaturesSet') and name = 'DistributionStandard'))
begin
  exec sp_executesql N'update FeaturesSet set Inventory = DistributionStandard where Inventory is null';
end
GO

--[MinVersion(Hash = ac5e39d0e910ff1e829161fb18924ba714a871122e1b73a6f7b95abefab200f8)]
if (exists(select * from sys.columns where object_id = object_id('CommonSetup') and name = 'WeightUOM')
and exists(select * from sys.columns where object_id = object_id('INSetup') and name = 'WeightUOM'))
begin
  exec sp_executesql N'
UPDATE c SET 
	c.WeightUOM = i.WeightUOM
FROM CommonSetup c
INNER JOIN INSetup i
	on c.CompanyID=i.CompanyID
where c.WeightUOM is null

UPDATE c SET
	c.VolumeUOM = i.VolumeUOM
FROM CommonSetup c
INNER JOIN INSetup i
	on c.CompanyID=i.CompanyID
where c.VolumeUOM is null';
end
GO

--[MinVersion(Hash = e5f2362105099a9af705bc34a6686c02c95ca1f200077710243054031071c07a)]
UPDATE FSSchedule
  SET FSSchedule.CustomerlocationID = FSServiceContract.CustomerlocationID
FROM FSSchedule
INNER JOIN FSServiceContract
  ON FSServiceContract.ServiceContractID = FSSchedule.EntityID
  AND FSSchedule.EntityType = 'C'
  AND FSServiceContract.CompanyID = FSSchedule.CompanyID
WHERE FSSchedule.CustomerlocationID IS NULL;
GO

--[MinVersion(Hash = fca83854c44e20f15a351fc1d654f830a4e258c9ffb98c69bd28684acebe93a7)]
UPDATE FSServiceContract
  SET FSServiceContract.ScheduleGenType = CASE WHEN FSServiceContract.RecordType = 'NRSC' THEN 'SO' ELSE 'AP' END
FROM FSServiceContract
WHERE FSServiceContract.ScheduleGenType IS NULL;
GO

--[MinVersion(Hash = 83410ce9620eaafd8936b5a5ccad15301f53d08f184e8ecba4604abf55731353)]
UPDATE FSSchedule
  SET FSSchedule.ScheduleGenType = CASE WHEN FSServiceContract.RecordType = 'NRSC' THEN 'SO' ELSE 'AP' END
FROM FSSchedule
INNER JOIN FSServiceContract
  ON FSServiceContract.ServiceContractID = FSSchedule.EntityID
  AND FSSchedule.EntityType = 'C'
  AND FSServiceContract.CompanyID = FSSchedule.CompanyID
WHERE FSSchedule.ScheduleGenType IS NULL;
GO

--[MinVersion(Hash = 2e3554e8f73765c46476b65523aad6ce384ca639304ff67391ba2b53b651df4f)]
UPDATE FSxService
  SET BillingRule = CASE WHEN ItemType = 'S' THEN 'FLRA' ELSE 'NONE' END
  FROM FSxService
  INNER JOIN InventoryItem
    ON FSxService.InventoryID = InventoryItem.InventoryID
    AND FSxService.CompanyID = InventoryItem.CompanyID
WHERE BillingRule IS NULL;
GO

--[IfExists(Column = FSSchedule.EnableExpirationDate)]
--[IfExists(Column = FSServiceContract.EnableExpirationDate)]
--[IfExists(Column = FSSchedule.EndDate)]
--[IfExists(Column = FSServiceContract.EndDate)]
--[MinVersion(Hash = 51fd4e7bcea1414be1ce9efb601b41b62a306374f90a0375d59d26f222d060f2)]
UPDATE FSSchedule SET
    FSSchedule.EnableExpirationDate = FSServiceContract.EnableExpirationDate,
    FSSchedule.EndDate = FSServiceContract.EndDate
FROM FSSchedule
    INNER JOIN FSServiceContract ON FSServiceContract.CompanyID = FSSchedule.CompanyID
                                AND FSServiceContract.ServiceContractID = FSSchedule.EntityID
                                AND FSSchedule.EntityType = 'C'
WHERE
    FSSchedule.EnableExpirationDate <> FSServiceContract.EnableExpirationDate
    OR FSSchedule.EnableExpirationDate IS NULL
    OR FSSchedule.EndDate <> FSServiceContract.EndDate;
GO

--[IfExists(Column = INItemClass.ItemClassID)]
--[IfExists(Column = INItemClass.ItemClassCD)]
--[IfExists(Column = FSxServiceClass.RemovedItemClassID)]
--[MinVersion(Hash = eaf367581b900a61fb1a2fb8e74e4a2c5ffc175606ca8a84a3d46eb51dec8987)]
UPDATE FSxServiceClass
SET ItemClassID = INItemClass.ItemClassID
FROM FSxServiceClass
INNER JOIN INItemClass ON INItemClass.CompanyID = FSxServiceClass.CompanyID AND INItemClass.ItemClassCD = FSxServiceClass.RemovedItemClassID;
GO

--[IfExists(Column = INItemClass.ItemClassID)]
--[IfExists(Column = INItemClass.ItemClassCD)]
--[IfExists(Column = FSxEquipmentModelTemplate.RemovedItemClassID)]
--[MinVersion(Hash = bcdc92ef4d66a7fbdf40596a319c2b6fe5cb6df31d201b2d295f06ccea005080)]
UPDATE FSxEquipmentModelTemplate
SET ItemClassID = INItemClass.ItemClassID
FROM FSxEquipmentModelTemplate
INNER JOIN INItemClass ON INItemClass.CompanyID = FSxEquipmentModelTemplate.CompanyID AND INItemClass.ItemClassCD = FSxEquipmentModelTemplate.RemovedItemClassID;
GO

--[IfExists(Column = INItemClass.ItemClassID)]
--[IfExists(Column = INItemClass.ItemClassCD)]
--[IfExists(Column = FSModelTemplateComponent.RemovedModelTemplateID)]
--[MinVersion(Hash = 8516602fbb008318ce64b1298a5bb15bc490e0c3c1e4b5f9b61180b7efc69747)]
UPDATE FSModelTemplateComponent
SET ModelTemplateID = INItemClass.ItemClassID
FROM FSModelTemplateComponent
INNER JOIN INItemClass ON INItemClass.CompanyID = FSModelTemplateComponent.CompanyID AND INItemClass.ItemClassCD = FSModelTemplateComponent.RemovedModelTemplateID;
GO

--Project 2.0 Block Starts (AC-78312)
--Initialization of new (NOT NULL) IsExpense field
--[MinVersion(Hash = f959318f49e218bdcd91f3b204864f2eaf137bbfbd32b3004711f25856699619)]
UPDATE PMAccountGroup SET IsExpense = 1 WHERE [Type]='E'
GO

--[MinVersion(Hash = b969cfe748c30304a319e7b3177acfbe14294ad840a72fa3281d9a2b90b7c1e8)]
UPDATE PMSetup SET ProformaNumbering='PROFORMA' WHERE ProformaNumbering IS NULL
GO

--[MinVersion(Hash = 19f4383ca110fbeca8d9a1b54609af202d35e33dc24cd301265bb6999cb32352)]
UPDATE PMSetup SET ChangeOrderNumbering='CHANGEORD' WHERE ChangeOrderNumbering IS NULL
GO

--Init Default Cost code for existing companies.
--[MinVersion(Hash = 2f0e938bfcf592c2e0278439cd63bcd13e2b07134b9bd1399e5c21c8316e5e56)]
INSERT INTO [PMCostCode] ([CompanyID],[IsDefault],[CostCodeCD],[Description],[NoteID],[CreatedByID],[CreatedByScreenID],[CreatedDateTime],[LastModifiedByID],[LastModifiedByScreenID],[LastModifiedDateTime])
SELECT s.[CompanyID],1,'0000','DEFAULT',NEWID(),s.[CreatedByID],s.[CreatedByScreenID],s.[CreatedDateTime],s.[LastModifiedByID],s.[LastModifiedByScreenID],s.[LastModifiedDateTime] FROM PMSetup s
LEFT JOIN PMCostCode c ON s.CompanyID = c.CompanyID AND c.IsDefault = 1
WHERE c.CostCodeID IS NULL
GO

--Reset the legth of default CostCodeCD to 4 chars. Note: this script can be removed in 2018R1 Release.
--[MinVersion(Hash = 12e281466eefee27cad0cfc942472c3193ed0cccc1f57a8a031c9ada705fce8c)]
UPDATE [PMCostCode] SET CostCodeCD='0000' WHERE IsDefault=1 AND CostCodeCD='000000'
GO

--Turn LimitsEnabled ON for existing projects with Limits ON in budgets
--[MinVersion(Hash = f266d25e7195da3d53a82cdb539dfcdc0729d87e671db8dc6a7fa540eb06b878)]
UPDATE p
SET p.LimitsEnabled = 1
FROM [Contract] p
INNER JOIN [PMBudget] b ON p.CompanyID = b.CompanyID AND p.ContractID = b.ProjectID
WHERE p.LimitsEnabled IS NULL AND b.LimitAmount = 1
GO

--Turning off Automation on Project Task screen.
--[MinVersion(Hash = 573db91c4360dcfffdc59a1e14175ddffa335b5e566b1b8e27e868e7550ebf82)]
UPDATE AUStep SET IsActive = 0 WHERE ScreenID='PM302000'
GO

--Initialize Step in Billing Rule.
--[MinVersion(Hash = 908aab0f4707204e6c4eeb159f38eb215ae66ab07fa307249d091899ed7280f3)]
UPDATE PMBillingRule SET StepID = AccountGroupID WHERE StepID is NULL
UPDATE PMBillingRule SET [Type] = 'T' WHERE [Type] is NULL
UPDATE b 
SET b.[Description] = a.GroupCD
FROM PMBillingRule b
INNER JOIN PMAccountGroup a ON b.CompanyID = a.CompanyID
AND b.AccountGroupID = a.GroupID
WHERE b.[Description] IS NULL
GO

--Initialize BranchSource with None
--[MinVersion(Hash = a6b0f2e64fa02d7a488dbbfb3c1d44dfc63aaf3f0cdad7a021a1ac5758a42414)]
UPDATE PMBillingRule SET BranchSource = 'N' WHERE BranchSource IS NULL
UPDATE PMBillingRule SET IsActive = 1 WHERE IsActive IS NULL
-- Set the right UserID for Employees (get UserID from the corresponded contact)
--[MinVersion(Branch = 6.10, Version = 6.10.1336, Hash = 89d48ec0ce413c9648ef9782bc4b405fc88a9a82b68224c0f368765f41b21455)]
UPDATE EPEmployee SET EPEmployee.UserID = Contact.UserID
FROM EPEmployee
LEFT JOIN BAccount ON BAccount.CompanyID = EPEmployee.CompanyID AND BAccount.BAccountID = EPEmployee.BAccountID
LEFT JOIN Contact ON Contact.CompanyID = BAccount.CompanyID AND Contact.ContactID = BAccount.DefContactID
WHERE ISNULL(EPEmployee.UserID, '00000000-0000-0000-0000-000000000000') <> ISNULL(Contact.UserID, '00000000-0000-0000-0000-000000000000')
GO

--[mysql: Skip]
--Move old InvoiceDescription to InvoiceGroup and InvoiceFormula
--[MinVersion(Hash = 0d1588c0dbfacc8bbea0a626137c334773b3bfaafc98517efefc6d476768ac64)]
IF EXISTS(SELECT * FROM sys.columns WHERE object_id = object_id('PMBillingRule') and name = 'InvoiceDescription')
BEGIN
  exec sp_executesql N'
  CREATE TABLE #group (RecordID int NOT NULL IDENTITY (1, 1), CompanyID int NOT NULL, InvoiceDescription nvarchar(255) COLLATE LATIN1_GENERAL_CI_AS NOT NULL, PRIMARY KEY (RecordID) )
  INSERT INTO #group select DISTINCT CompanyID, InvoiceDescription FROM PMBillingRule WHERE InvoiceDescription IS NOT NULL

  UPDATE r
  SET r.InvoiceGroup = g.RecordID, r.InvoiceFormula = ''=''  + char(39) + g.InvoiceDescription  + char(39) + ''''
  FROM PMBillingRule r
  INNER JOIN #group g ON r.CompanyID = g.CompanyID AND r.InvoiceDescription = g.InvoiceDescription COLLATE SQL_Latin1_General_CP1_CI_AS' 

  exec sp_executesql N'
  INSERT PMBillingRuleKVExt (CompanyID, RecordID, FieldName, ValueString)
  SELECT l.CompanyID, t.NoteID, ''InvoiceDescription'' + MAX(UPPER(SUBSTRING(l.LocaleName, 1, 2))), MAX(InvoiceDescription)
  FROM PMBillingRule t
  INNER JOIN Locale l ON l.IsDefault = 1
  LEFT JOIN PMBillingRuleKVExt e ON e.CompanyID = l.CompanyID AND e.RecordID = t.NoteID AND e.FieldName = ''InvoiceDescription'' + UPPER(SUBSTRING(l.LocaleName, 1, 2)) COLLATE SQL_Latin1_General_CP1_CI_AS
  WHERE e.CompanyID IS NULL
  GROUP BY l.CompanyID, t.NoteID'

  UPDATE PMBillingRuleKVExt SET FieldName = REPLACE(FieldName, 'InvoiceDescription', 'InvoiceFormula')

END
GO

--[mysql: Native]
--[mssql: Skip]
--[IfExists(Column = PMBillingRule.InvoiceDescription)]
--[MinVersion(Hash = 8567ab996be3c4572a97f130c9a9c7631537b19a3d753db97f31ac4ec81819a3)]
CREATE TEMPORARY TABLE `#group` (
  `RecordID` INT NOT NULL AUTO_INCREMENT, 
  `CompanyID` INT NULL, 
  `InvoiceDescription` VARCHAR(255)  CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL, 
  Primary Key (`RecordID`));
  
  INSERT INTO `#group` (`CompanyID`, `InvoiceDescription`)
  SELECT DISTINCT `CompanyID`, `InvoiceDescription`
  FROM `PMBillingRule` WHERE `InvoiceDescription` IS NOT NULL;

  UPDATE `PMBillingRule` `r`
  INNER JOIN `#group` `g` ON `r`.`CompanyID` = `g`.`CompanyID` AND `r`.`InvoiceDescription` = `g`.`InvoiceDescription`
  SET `r`.InvoiceGroup = `g`.`RecordID`, `r`.`InvoiceFormula` = Concat('=''' , Concat(`g`.`InvoiceDescription`, ''''));
  
  INSERT `PMBillingRuleKvExt` (`CompanyID`, `RecordID`, `FieldName`, `ValueString`)
  SELECT `l`.`CompanyID`, `t`.`NoteID`, 'InvoiceDescription' + MAX(UPPER(SUBSTRING(`l`.`LocaleName`, 1, 2))), MAX(`InvoiceDescription`)
  FROM `PMBillingRule` `t`
  INNER JOIN `Locale` `l` ON `l`.`IsDefault` = 1
  LEFT JOIN `PMBillingRuleKvExt` `e` ON `e`.`CompanyID` = `l`.`CompanyID` AND `e`.`RecordID` = `t`.`NoteID` AND `e`.`FieldName` = 'InvoiceDescription' + UPPER(SUBSTRING(`l`.`LocaleName`, 1, 2))
  WHERE `e`.`CompanyID` IS NULL
  GROUP BY `l`.`CompanyID`, `t`.`NoteID`;

  UPDATE `PMBillingRuleKvExt` 
  SET `FieldName` = REPLACE(`FieldName`, 'InvoiceDescription', 'InvoiceFormula')
GO

--Initialize Budget Level on Project to 'Task'
--[MinVersion(Hash = c1c2874b241dbe02f8648cde7933d23b1d4109bfcc4495cea9c127d1d5afafae)]
UPDATE [Contract] SET BudgetLevel='I' WHERE BudgetLevel IS NULL AND BaseType='P'
GO

--Move reference from ARTran.PMTranID to PMTran.ARRefNbr,RefLineNbr
--[MinVersion(Hash = ba26e5165a4f4347b78453e438804b932e768364484603637a8cf846287920b1)]
IF EXISTS(SELECT * FROM sys.columns WHERE object_id = object_id('ARTran') and name = 'PMTranID')
BEGIN
exec sp_executesql N'
UPDATE p
SET p.ARTranType = a.TranType, p.ARRefNbr = a.RefNbr, p.RefLineNbr = a.LineNbr
FROM ARTran a
INNER JOIN PMTran p ON a.CompanyID = p.CompanyID AND a.PMTranID = p.TranID
WHERE p.ARRefNbr IS NULL'
END
GO

--PMTask: Move from  CompletePct (int) to CompletedPercent (decimal)
--[MinVersion(Hash = b9b71a971327a823cf8e8466f8d05991ae35fa3495b6ca668c103099f0a40584)]
IF EXISTS(SELECT * FROM sys.columns WHERE object_id = object_id('PMTask') and name = 'CompletedPct')
BEGIN
exec sp_executesql N'
UPDATE PMTask SET CompletedPercent = CompletedPct'
END
GO

--[mysql: Skip]
--[IfNotExistsSelect(From = PMBillingRecord)]
--[MinVersion(Hash = f079f4ad1d4854d47b4fe8695845785d5ac392c38c5ab0e1b4e89786c2ea62e6)]
CREATE TABLE #billed (RecordID int NOT NULL IDENTITY (1, 1), CompanyID int NOT NULL, DocType char(3) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL, RefNbr nvarchar(15) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL, ProjectID int NOT NULL, PRIMARY KEY (RecordID) )
  INSERT INTO #billed SELECT a.CompanyID, a.DocType, a.RefNbr, a.ProjectID FROM ARInvoice a
  INNER JOIN ARRegister r ON a.CompanyID = r.CompanyID AND a.DocType = r.DocType AND a.RefNbr = r.RefNbr 
  INNER JOIN Contract p ON a.CompanyID = p.CompanyID AND a.ProjectID = p.ContractID AND p.BaseType='P' AND p.NonProject = 0
  ORDER BY a.CompanyID, a.ProjectID, a.RefNbr

  INSERT INTO [PMBillingRecord] ([CompanyID],[ProjectID], [RecordID],[BillingTag],[Date],[ProformaRefNbr],[ARDocType],[ARRefNbr])
  SELECT a.CompanyID, b.ProjectID, b.RecordID + 1 - (SELECT MIN(RecordID) FROM #billed s WHERE s.CompanyID = b.CompanyID AND s.ProjectID = b.ProjectID ) AS RecordID, 'P', r.DocDate, NULL, a.DocType, a.RefNbr FROM ARInvoice a
  INNER JOIN ARRegister r ON a.CompanyID = r.CompanyID AND a.DocType = r.DocType AND a.RefNbr = r.RefNbr
  INNER JOIN Contract p ON a.CompanyID = p.CompanyID AND a.ProjectID = p.ContractID AND p.BaseType='P' AND p.NonProject = 0 
  INNER JOIN #billed b ON a.CompanyID = b.CompanyID AND a.ProjectID = b.ProjectID and a.DocType = b.DocType COLLATE SQL_Latin1_General_CP1_CI_AS and a.RefNbr = b.RefNbr COLLATE SQL_Latin1_General_CP1_CI_AS

  UPDATE p
  SET p.billingLineCntr = b.Cntr
  FROM Contract p
  INNER JOIN (select CompanyID, ProjectID, MAX(RecordID) as Cntr FROM PMBillingRecord GROUP BY CompanyID, ProjectID) b ON b.CompanyID = p.CompanyID and b.ProjectID = p.ContractID
  WHERE b.Cntr > 0
GO

--[mysql: Native]
--[mssql: Skip]
--[IfNotExistsSelect(From = PMBillingRecord)]
--[MinVersion(Hash = 050e0b189de0e33a97e7b9ca0f120556c67a5265fa1c14546aa8f3e396f9ce94)]
CREATE TEMPORARY TABLE `#billedA` (
  `RecordID` INT NOT NULL AUTO_INCREMENT, 
  `CompanyID` INT NULL, 
  `DocType` CHAR(3)  CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL, 
  `RefNbr` VARCHAR(15)  CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL, 
  `ProjectID` INT NULL,
  Primary Key (`RecordID`));

CREATE TEMPORARY TABLE `#billedB` (
  `RecordID` INT NOT NULL AUTO_INCREMENT, 
  `CompanyID` INT NULL, 
  `DocType` CHAR(3)  CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL, 
  `RefNbr` VARCHAR(15)  CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL, 
  `ProjectID` INT NULL,
  Primary Key (`RecordID`));

INSERT INTO `#billedA` (`CompanyID`, `DocType`, `RefNbr`, `ProjectID`) 
(SELECT `a`.`CompanyID`, `a`.`DocType`, `a`.`RefNbr`, `a`.`ProjectID`
FROM `ARInvoice` `a`
INNER JOIN `ARRegister` `r` ON (((`a`.`CompanyID` = `r`.`CompanyID`) AND (`a`.`DocType` = `r`.`DocType`)) AND (`a`.`RefNbr` = `r`.`RefNbr`))
INNER JOIN `Contract` `p` ON ((((`a`.`CompanyID` = `p`.`CompanyID`) AND (`a`.`ProjectID` = `p`.`ContractID`)) AND (`p`.`BaseType` = 'P')) AND (`p`.`NonProject` = 0)));

INSERT INTO `#billedB` (`CompanyID`, `DocType`, `RefNbr`, `ProjectID`) 
(SELECT `a`.`CompanyID`, `a`.`DocType`, `a`.`RefNbr`, `a`.`ProjectID`
FROM `ARInvoice` `a`
INNER JOIN `ARRegister` `r` ON (((`a`.`CompanyID` = `r`.`CompanyID`) AND (`a`.`DocType` = `r`.`DocType`)) AND (`a`.`RefNbr` = `r`.`RefNbr`))
INNER JOIN `Contract` `p` ON ((((`a`.`CompanyID` = `p`.`CompanyID`) AND (`a`.`ProjectID` = `p`.`ContractID`)) AND (`p`.`BaseType` = 'P')) AND (`p`.`NonProject` = 0)));

INSERT INTO `PMBillingRecord` (`CompanyID`, `ProjectID`, `RecordID`, `BillingTag`, `Date`, `ProformaRefNbr`, `ARDocType`, `ARRefNbr`)
(SELECT `a`.`CompanyID`, `#billedB`.`ProjectID`, ((`#billedB`.`RecordID` + 1) - (SELECT MIN(`RecordID`) FROM `#billedA`  WHERE (`#billedA`.`CompanyID` = `#billedB`.`CompanyID`) AND (`#billedA`.`ProjectID` = `#billedB`.`ProjectID`) LIMIT 1)) AS `RecordID`, 'P', `r`.`DocDate`, NULL, `a`.`DocType`, `a`.`RefNbr`
FROM `ARInvoice` `a`
INNER JOIN `ARRegister` `r` ON (((`a`.`CompanyID` = `r`.`CompanyID`) AND (`a`.`DocType` = `r`.`DocType`)) AND (`a`.`RefNbr` = `r`.`RefNbr`))
INNER JOIN `Contract` `p` ON((((`a`.`CompanyID` = `p`.`CompanyID`) AND (`a`.`ProjectID` = `p`.`ContractID`)) AND (`p`.`BaseType` = 'P')) AND (`p`.`NonProject` = 0))
INNER JOIN `#billedB` ON (((`a`.`CompanyID` = `#billedB`.`CompanyID`) AND (`a`.`ProjectID` = `#billedB`.`ProjectID`)) AND (`a`.`DocType` = `#billedB`.`DocType`) AND (`a`.`RefNbr` = `#billedB`.`RefNbr`)));

UPDATE `Contract` `p`
INNER JOIN (SELECT `CompanyID`, `ProjectID`, MAX(`RecordID`) AS `Cntr` FROM `PMBillingRecord`
GROUP BY `CompanyID`, `ProjectID`) `b` ON ((`b`.`CompanyID` = `p`.`CompanyID`) AND (`b`.`ProjectID` = `p`.`ContractID`)) SET `p`.`billingLineCntr` = `b`.`Cntr`
WHERE `b`.`Cntr` > 0;
GO

--AC76024 begin
--[MinVersion(Hash = a555be82bba75a20a0c8a0e1f6e681a91fee93744a5158948abf4cdd5f5d7ae7)]
if (exists(select * from sys.columns where object_id = object_id('Contract') and name = 'VisibleInEP')
and exists(select * from sys.columns where object_id = object_id('Contract') and name = 'VisibleInTA')
and exists(select * from sys.columns where object_id = object_id('Contract') and name = 'VisibleInEA'))
begin
  exec sp_executesql N'UPDATE Contract SET VisibleInTA = VisibleInEP, VisibleInEA = VisibleInEP'
end
GO

--[MinVersion(Hash = 27ffe536b838cd99b7c2f4e78ea8b97d4c4da0745ce296a4df52b0a732d414e9)]
if (exists(select * from sys.columns where object_id = object_id('PMTask') and name = 'VisibleInEP')
and exists(select * from sys.columns where object_id = object_id('PMTask') and name = 'VisibleInTA')
and exists(select * from sys.columns where object_id = object_id('PMTask') and name = 'VisibleInEA'))
begin 
  exec sp_executesql N'UPDATE PMTask SET VisibleInTA = VisibleInEP, VisibleInEA = VisibleInEP'
end
GO

--[MinVersion(Hash = 49b8ea2eec50fa8c40d7d68f6725c4481185e8178b4eb4d964c72be132b7e53c)]
if (exists(select * from sys.columns where object_id = object_id('PMSetup') and name = 'VisibleInEP')
and exists(select * from sys.columns where object_id = object_id('PMSetup') and name = 'VisibleInTA')
and exists(select * from sys.columns where object_id = object_id('PMSetup') and name = 'VisibleInEA'))
begin 
  exec sp_executesql N'UPDATE PMSetup SET VisibleInTA = VisibleInEP, VisibleInEA = VisibleInEP'
end
GO

--AC76024 end
--[MinVersion(Hash = b776d38b56b498fa003754e54fe73316552160205e8c0a6f86517f6d9f6fca5e)]
if (exists(select * from sys.columns where object_id = object_id('PMSetup') and name = 'RestrictProjectSelect'))
begin 
  exec sp_executesql N'UPDATE PMSetup SET RestrictProjectSelect = ''A'' WHERE RestrictProjectSelect is null'
end
GO

--Move WipAccountGroup from Billing Rule to Task 
--[MinVersion(Hash = b5850c70c17bb63956798a7d9350d1bdb9b91d64a55c3f0a60b9c04adccd6078)]
if (exists(select * from sys.columns where object_id = object_id('PMBillingRule') and name = 'WipAccountGroup'))
begin 
  exec sp_executesql N'UPDATE t
    SET WipAccountGroupID = b.WipAccountGroupID
    FROM PMTask t
    INNER JOIN PMBillingRule b ON t.CompanyID = b.CompanyID AND t.BillingID = b.BillingID
    WHERE t.WipAccountGroupID IS NULL AND b.WipAccountGroupID IS NOT NULL'
end
GO

--Init Progressive Billing Rule: AccountSource = AccountGroup
--[MinVersion(Hash = 0b47d94f5ce94fd4a31f2647d1846b54980ca37249632660127fdc0fb6f6580a)]
UPDATE PMBillingRule SET AccountSource = 'A' WHERE AccountSource IS NULL AND Type='B'
GO

--[MinVersion(Hash = ed37ab3e5fe644cceb4e8530d99e572deb4afaeac348117a968fbf62bdd8fd97)]
if (exists(select * from sys.columns where object_id = object_id('Contract') and name = 'VisibleInAP'))
begin 
  exec sp_executesql N'UPDATE Contract SET VisibleInAP = 1
  WHERE VisibleInAP = 0 AND NonProject = 1 '
end
GO

--[MinVersion(Hash = 5b6da80f1b8935add17563bd3344297b95f36aa6f64ec015cb90ccc6d3193b7b)]
if (exists(select * from sys.columns where object_id = object_id('Contract') and name = 'VisibleInAR'))
begin 
  exec sp_executesql N'UPDATE Contract SET VisibleInAR = 1
  WHERE VisibleInAR = 0 AND NonProject = 1 '
end
GO

--[MinVersion(Hash = fcf98f31abcd014017ca9d75fe5cd606003e8f85923031ec39afaa9d0603b0d1)]
if (exists(select * from sys.columns where object_id = object_id('Contract') and name = 'VisibleInCA'))
begin 
  exec sp_executesql N'UPDATE Contract SET VisibleInCA = 1
  WHERE VisibleInCA = 0 AND NonProject = 1 '
end
GO

--[MinVersion(Hash = 497b4ff75abe6348b076169e9ae49cbbf3f933b8c4110fc02dfccf518c46c5dd)]
if (exists(select * from sys.columns where object_id = object_id('Contract') and name = 'VisibleInCR'))
begin 
  exec sp_executesql N'UPDATE Contract SET VisibleInCR = 1
  WHERE VisibleInCR = 0 AND NonProject = 1 '
end
GO

--[MinVersion(Hash = 0a78a8fdffd7620b54ad36bb879a9563e609c39428081e8071e3a23924429139)]
if (exists(select * from sys.columns where object_id = object_id('Contract') and name = 'VisibleInEA'))
begin 
  exec sp_executesql N'UPDATE Contract SET VisibleInEA = 1
  WHERE VisibleInEA = 0 AND NonProject = 1 '
end
GO

--[MinVersion(Hash = 7e177baa828597afe9b7347cb606eaa013161ece0d798fe04311f082463e0616)]
if (exists(select * from sys.columns where object_id = object_id('Contract') and name = 'VisibleInGL'))
begin 
  exec sp_executesql N'UPDATE Contract SET VisibleInGL = 1
  WHERE VisibleInGL = 0 AND NonProject = 1 '
end
GO

--[MinVersion(Hash = 019bea36314411273750c80acba11dbcf96c7d690887b73548b791ea2e99fba7)]
if (exists(select * from sys.columns where object_id = object_id('Contract') and name = 'VisibleInIN'))
begin 
  exec sp_executesql N'UPDATE Contract SET VisibleInIN = 1
  WHERE VisibleInIN = 0 AND NonProject = 1 '
end
GO

--[MinVersion(Hash = c9f30b8085f37a08b5a9249be5a9edc44dfd1319cd6e4f6d4d8324af0ff484a2)]
if (exists(select * from sys.columns where object_id = object_id('Contract') and name = 'VisibleInPO'))
begin 
  exec sp_executesql N'UPDATE Contract SET VisibleInPO = 1
  WHERE VisibleInPO = 0 AND NonProject = 1 '
end
GO

--[MinVersion(Hash = 61898a2e68689839053b9d995c3a08c9907395f2b2f5f5c1438b93cc0ff5e707)]
if (exists(select * from sys.columns where object_id = object_id('Contract') and name = 'VisibleInSO'))
begin 
  exec sp_executesql N'UPDATE Contract SET VisibleInSO = 1
  WHERE VisibleInSO = 0 AND NonProject = 1 '
end
GO

--[MinVersion(Hash = 2a21132ced1db02038aabc1cbcf3fb04ab14cb69f4bc25642f133497f286cb4b)]
if (exists(select * from sys.columns where object_id = object_id('Contract') and name = 'VisibleInTA'))
begin 
  exec sp_executesql N'UPDATE Contract SET VisibleInTA = 1
  WHERE VisibleInTA = 0 AND NonProject = 1 '
end
GO

--[MinVersion(Hash = cec7699f775dc11e019b93d5b39c56899d982ccd812eecccf6a710ca40de3faf)]
if exists(select * from Contract c where c.BillAddressID is null and c.BaseType='P')
begin

	insert into PMAddress (
       [CompanyID]
      ,[CustomerID]
      ,[RevisionID]
      ,[AddressLine1]
      ,[AddressLine2]
      ,[AddressLine3]
      ,[City]
      ,[State]
      ,[CountryID]
      ,[PostalCode]
	  ,[NoteID]
      ,[IsValidated]
      ,[CreatedByID]
      ,[CreatedByScreenID]
      ,[CreatedDateTime]
      ,[LastModifiedByID]
      ,[LastModifiedByScreenID]
      ,[LastModifiedDateTime]
      ,[IsDefaultBillAddress]
		  ,[CustomerAddressID]
		  ,ContractID)
	SELECT co.[CompanyID]
		  ,co.[CustomerID]
		  ,a.[RevisionID]
		  ,a.[AddressLine1]
		  ,a.[AddressLine2]
		  ,a.[AddressLine3]
		  ,a.[City]
		  ,a.[State]
		  ,a.[CountryID]
		  ,a.[PostalCode]
		  ,NewID()
		  ,a.[IsValidated]
		  ,a.[CreatedByID]
		  ,a.[CreatedByScreenID]
		  ,a.[CreatedDateTime]
		  ,a.[LastModifiedByID]
		  ,a.[LastModifiedByScreenID]
		  ,a.[LastModifiedDateTime]
      ,1
      ,cus.DefBillAddressID
		  ,co.ContractID
	 from  Contract co
		 inner join Customer cus on co.CustomerID=cus.BAccountID AND co.CompanyID = cus.CompanyID
		 inner join Address a on cus.BAccountID=a.BAccountID AND a.AddressID = cus.DefBillAddressID	and a.CompanyID = co.CompanyID	
	 where  co.BillAddressID is null

end
GO

--[MinVersion(Hash = ae8fc717cf1e558efbb63549aa491037d71d40bce3fc1de93505e696ad574953)]
UPDATE con set con.BillAddressID = ad.AddressID from Contract con
    join PMAddress ad on ad.CompanyId = con.CompanyID and ad.ContractID = con.ContractID
WHERE con.BillAddressID is Null
GO

--[MinVersion(Hash = 1ca6c88d82940035ceb44cdd323a40d6fc4fbe7b6150d8f94c12ff2b122885b5)]
if exists(select * from Contract c where c.BillContactID is null and c.BaseType='P')
begin
	insert into PMContact (
       [CompanyID]
      ,[CustomerID]
      ,[CustomerContactID]
      ,[RevisionID]
      ,[IsDefaultContact]
      ,[Title]
      ,[Salutation]
      ,[FullName]
      ,[Email]
      ,[Phone1]
      ,[Phone1Type]
      ,[Phone2]
      ,[Phone2Type]
      ,[Phone3]
      ,[Phone3Type]
      ,[Fax]
      ,[FaxType]
	  ,[NoteID]
      ,[CreatedByID]
      ,[CreatedByScreenID]
      ,[CreatedDateTime]
      ,[LastModifiedByID]
      ,[LastModifiedByScreenID]
		  ,[LastModifiedDateTime]
		  ,[ContractID])
	SELECT co.[CompanyID]
		  ,co.[CustomerID]
      ,cus.DefBillContactID
		  ,c.[RevisionID]
      ,1
		  ,c.[Title]
		  ,c.[Salutation]
		  ,c.[FullName]
		  ,c.[Email]
		  ,c.[Phone1]
		  ,c.[Phone1Type]
		  ,c.[Phone2]
		  ,c.[Phone2Type]
		  ,c.[Phone3]
		  ,c.[Phone3Type]
		  ,c.[Fax]
		  ,c.[FaxType]
		  ,NewID()
		  ,c.[CreatedByID]
		  ,c.[CreatedByScreenID]
		  ,c.[CreatedDateTime]
		  ,c.[LastModifiedByID]
		  ,c.[LastModifiedByScreenID]
		  ,c.[LastModifiedDateTime]
		  ,co.ContractID
	from  Contract co
	 inner join Customer cus on co.CustomerID=cus.BAccountID AND co.CompanyID = cus.CompanyID
	 inner join Contact c on cus.BAccountID=c.BAccountID AND c.ContactID = cus.DefBillContactID	and c.CompanyID = co.CompanyID	
	where co.BillContactID is null

end
GO

--[MinVersion(Hash = 737e708d4f99cae31290c322ee4dfc7e1ec7611290f4a8ab4bdd9a8d25364c72)]
UPDATE con set con.BillContactID = c.ContactID from Contract con
    join PMContact c on c.CompanyId = con.CompanyID and c.ContractID = con.ContractID
WHERE con.BillcontactID is Null
GO

--Project 2.0 Block Ends (AC-78312)
----------------------------------------------------------------------------------------------------------------------------
--[MinVersion(Hash = 4034287911b9752353d27a074b9aeab97d092a15d1ee01bc15006b3b68cecbce)]

GO

--[MinVersion(Hash = e8fa9a5e48b5a5f612c2104a414c4356000fe38d21b4a876c12d780bfb8b471d)]
UPDATE dbo.PaymentMethodDetail 
  SET IsCVV = 1
  WHERE DetailID = 'CVV';
GO

--[MinVersion(Hash = e24ddc636493c7aa5189c5bb434f170f9c5a637477e6552c02a31583d9bfe77d)]
update s
set s.OrderCntr = o.OrderCntr
from SOShipment s 
inner join (select h.CompanyID, h.ShipmentNbr, h.ShipmentType, count(h.ShipmentNbr) as OrderCntr from SOOrderShipment h group by h.CompanyID, h.ShipmentType, h.ShipmentNbr) o
on o.CompanyID = s.CompanyID and o.ShipmentType = s.ShipmentType and o.ShipmentNbr = s.ShipmentNbr
GO

--Populate flag SOOrderShipment.CreateINDoc for confirmed shipments which not invoiced yet
--[MinVersion(Hash = 3f69af462f1d699a14ce77bb6271e96011b1b85215eb34d1d03ae4e278724e9b)]
UPDATE os
SET os.CreateINDoc = 1
FROM [SOOrderShipment] os
INNER JOIN [SOShipLine] sl
ON sl.CompanyID = os.CompanyID AND sl.ShipmentType = os.ShipmentType AND sl.OrigOrderType = os.OrderType AND sl.OrigOrderNbr = os.OrderNbr
INNER JOIN [SOOrderType] t
ON t.CompanyID = os.CompanyID AND t.OrderType = os.OrderType
WHERE (os.CreateINDoc IS NULL OR os.CreateINDoc = 0) AND os.InvoiceNbr IS NULL AND os.Confirmed = 1 AND sl.RequireINUpdate = 1 AND t.INDocType <> 'UND'
GO

--Populate flag SOOrderShipment.CreateINDoc for drop shipments because IN Issues are created for all drop shipments (even non-stock)
--[MinVersion(Hash = b67b1956219ea4aeff1209868c92bc94701f910ac5fbc8a32f85e9812ab4e066)]
UPDATE os
SET os.CreateINDoc = 1
FROM [SOOrderShipment] os
WHERE (os.CreateINDoc IS NULL OR os.CreateINDoc = 0) AND os.ShipmentType = 'H'
GO

--Populate newly added flag SOInvoice.CreateINDoc
--[IfExists(Column = SOInvoice.CreateINDoc)]
--[MinVersion(Hash = f6779fe73f5e64f5158a2b274bc69044fc10512838dc8739ccaa153f7c2be16f)]
UPDATE soi
SET soi.CreateINDoc = 1
FROM [SOInvoice] soi
INNER JOIN [SOOrderShipment] os
ON os.CompanyID = soi.CompanyID AND os.InvoiceType = soi.DocType AND os.InvoiceNbr = soi.RefNbr
INNER JOIN [SOOrderType] t
ON t.CompanyID = os.CompanyID AND t.OrderType = os.OrderType
WHERE (soi.CreateINDoc IS NULL OR soi.CreateINDoc = 0) AND os.CreateINDoc = 1 AND os.InvtRefNbr IS NULL AND t.INDocType <> 'UND'
GO

--[MinVersion(Hash = bf5dce3356b9565e3f39244b80669b155623012437300f8cbf708ef75c36fdc3)]
UPDATE INItemLotSerial
SET QtyOrig = -1
WHERE LotSerTrack = 'S' AND LotSerAssign = 'U' AND QtyOnHand = -1 AND QtyOrig IS NULL
GO

--[MinVersion(Hash = 1808b1bb12853bc1459c121b471c2fc327ab6e2ce92300bc50004c90b381fdad)]
update i
set
    SuppliedByVendorLocationID = r.VendorLocationID
from APInvoice i
left join APRegister r
on i.CompanyID = r.CompanyID
    and i.DocType = r.DocType
    and i.RefNbr = r.RefNbr
where SuppliedByVendorLocationID is null
GO

--[SmartExecute]
--[MinVersion(Hash = f9a9201de0d435dbc5b8dd6b262cbaf520fade00da3312c7ac00789e3885e8da)]
update ic
set ItemClassCD = left(ic.ItemClassCD + replicate(' ', COALESCE(d.Length, 10)), COALESCE(d.Length, 10))
from INItemClass ic
left join Dimension d on d.DimensionID = 'INItemClass'
GO

--[MinVersion(Hash = 415abdfccf86a51de32f94e5f89331df7801a29795615cce4d6fbc1bb76bd1c4)]
update p
set p.CompletePOLine = coalesce(i.CompletePOLine, 'A')
from POLine p left join InventoryItem i on i.CompanyID = p.CompanyID and i.InventoryID = p.InventoryID 
where p.CompletePOLine is null
GO

--[MinVersion(Hash = 1a911ccb8358a36390b551d9ef4cbf59bc8a6774b93d46192e5e7ece927b3df4)]
update t set Template = OrderType
from SOOrderType t
where (OrderType ='CS' and Template = 'IN') or (OrderType = 'TR' and Template = 'SO')
and Template <> OrderType

update t set IsSystem = 1
from SOOrderType t
where t.OrderType in ('CM', 'CS', 'IN', 'QT', 'RM', 'SA', 'SO', 'TR') and
t.OrderType = t.Template and t.IsSystem is null

update t set IsSystem = 0
from SOOrderType t 
where t.IsSystem is null
GO

--[IfExists(Column = Notification.NotificationID)]
--[MinVersion(Hash = 2bfedc1beffb6f0dd9588a9ac747bb559aec2372fc2252bde6d78671492310ba)]
delete q
FROM Notification q
INNER JOIN(
SELECT *, (
  SELECT TOP (1) CompanyNotificationID
  FROM Notification n2
  WHERE n2.NotificationID = n1.NotificationID
    AND n2.CompanyID = n1.CompanyID
  ORDER BY n2.LastModifiedDateTime DESC
   ,n2.CompanyNotificationID DESC
  ) as OriginalID
FROM Notification n1 ) D
ON
  q.CompanyID = D.CompanyID and
  q.CompanyNotificationID = D.CompanyNotificationID AND 
  q.CompanyNotificationID != D.OriginalID
GO

--[MinVersion(Hash = 74e9ba55e9578d6f208611709f1908f0e66bd543c47804e743e570d59e106707)]
UPDATE CASetup
SET	ReceiptTranDaysBeforeIncPayments = ReceiptTranDaysBefore,
	ReceiptTranDaysAfterIncPayments = ReceiptTranDaysAfter,
	DisbursementTranDaysBeforeIncPayments = DisbursementTranDaysBefore,
	DisbursementTranDaysAfterIncPayments = DisbursementTranDaysAfter,
	RefNbrCompareWeightIncPayments = RefNbrCompareWeight,
	EmptyRefNbrMatchingIncPayments = EmptyRefNbrMatching,
	DateCompareWeightIncPayments = DateCompareWeight,
	PayeeCompareWeightIncPayments = PayeeCompareWeight,
	DateMeanOffsetIncPayments = DateMeanOffset,
	DateSigmaIncPayments = DateSigma
GO

--[MinVersion(Hash = 87c6ba5b91b331e43fbfc741516b08b855209cc621ca127bea2c3b001abe5697)]
Update APSetupApproval set DocType = 'INV'
GO

--[MinVersion(Hash = 24b3b806d45c2780fea79b4e957976c188763d2391f1e80edbc2f8aaad923b0f)]
Update ARRegister set Approved = 1
GO

--[MinVersion(Hash = ef5aa930145c8086dbd9fcb29c4ad49458b0d84f9b3ef6b712a4a542922d5b5b)]
INSERT INTO APSetupApproval (CompanyID
, AssignmentNotificationID
, AssignmentMapID
, CreatedByID
, CreatedByScreenID
, CreatedDateTime
, LastModifiedByID
, LastModifiedByScreenID
, LastModifiedDateTime
, IsActive
, DocType)
  SELECT 
    CompanyID
   ,AssignmentNotificationID
   ,AssignmentMapID
   ,CreatedByID
   ,CreatedByScreenID
   ,CreatedDateTime
   ,LastModifiedByID
   ,LastModifiedByScreenID
   ,LastModifiedDateTime
   ,IsActive
   ,'ACR'
  FROM dbo.APSetupApproval
  WHERE DocType = 'INV';

INSERT INTO APSetupApproval (CompanyID
, AssignmentNotificationID
, AssignmentMapID
, CreatedByID
, CreatedByScreenID
, CreatedDateTime
, LastModifiedByID
, LastModifiedByScreenID
, LastModifiedDateTime
, IsActive
, DocType)
  SELECT 
    CompanyID
   ,AssignmentNotificationID
   ,AssignmentMapID
   ,CreatedByID
   ,CreatedByScreenID
   ,CreatedDateTime
   ,LastModifiedByID
   ,LastModifiedByScreenID
   ,LastModifiedDateTime
   ,IsActive
   ,'ADR'
  FROM dbo.APSetupApproval
  WHERE DocType = 'INV';
INSERT INTO APSetupApproval (CompanyID
, AssignmentNotificationID
, AssignmentMapID
, CreatedByID
, CreatedByScreenID
, CreatedDateTime
, LastModifiedByID
, LastModifiedByScreenID
, LastModifiedDateTime
, IsActive
, DocType)
  SELECT 
    CompanyID
   ,AssignmentNotificationID
   ,AssignmentMapID
   ,CreatedByID
   ,CreatedByScreenID
   ,CreatedDateTime
   ,LastModifiedByID
   ,LastModifiedByScreenID
   ,LastModifiedDateTime
   ,IsActive
   ,'PPR'
  FROM dbo.APSetupApproval
  WHERE DocType = 'INV';
GO

--[IfExists(Column = FSAppointment.HandleManuallyScheduleTime)]
--[IfExists(Column = FSAppointment.KeepTotalServicesDuration)]
--[MinVersion(Hash = 546377b730f220b51480b4417bb5df1f737a73f2476e243f33ba9f9b60712549)]
UPDATE FSAppointment
  SET HandleManuallyScheduleTime = KeepTotalServicesDuration 
WHERE HandleManuallyScheduleTime IS NULL;
GO

--[MinVersion(Hash = 794fd5eb94e2019100d99e091de4065c752bf25d1966becdde833c3136a50402)]
UPDATE FSAppointment 
	SET EstimatedLineTotal = LineTotal
WHERE EstimatedLineTotal IS NULL;
GO

--[MinVersion(Hash = 26d9a7f976440f30b5614c52e6637971f62613cc8d26afc4c1e39100b20e06bc)]
UPDATE FSAppointmentDet 
	SET EstimatedTranAmt = TranAmt
WHERE EstimatedTranAmt IS NULL;
GO

--[MinVersion(Hash = 995bd7dfd362744d60a4bd3182b5970a778ed42dc425b531bf3d342dbbc7f846)]
UPDATE FSAppointmentDet 
	SET EstimatedQty = Qty
WHERE EstimatedQty IS NULL;
GO

--[IfExists(Column = FSAppointmentDet.ActualDateTimeBegin)]
--[MinVersion(Hash = 3ec88473a57c9edad787770706948aa2e524f362e0dd4ab03c8bc37326de8794)]
UPDATE Det 
SET 
	ActualDateTimeBegin = NULL, 
	ActualDateTimeEnd = NULL
FROM FSAppointmentDet Det
INNER JOIN FSAppointment App
	ON Det.CompanyID = App.CompanyID
	AND Det.AppointmentID = App.AppointmentID
WHERE App.Status = 'S' 
OR App.Status = 'A';
GO

--[MinVersion(Hash = 16ac3e39a347f3abd98ded76dcff5a7d1b69abae70643b787ab7cc91da769822)]
UPDATE Det 
SET Qty = 0, 
	ActualDuration = 0
FROM FSAppointmentDet Det
INNER JOIN FSAppointment App
	ON Det.CompanyID = App.CompanyID
	AND Det.AppointmentID = App.AppointmentID
WHERE App.Status = 'S' 
OR App.Status = 'A';
GO

--[MinVersion(Hash = 1e852603011d215f941d54a3eb4d9196de92d6941260d8364dc8d008d8215c80)]
UPDATE FSAppointment 
	SET ActualDurationTotal = 0
WHERE Status = 'S' 
OR Status = 'A';
GO

--[MinVersion(Hash = e1dd9ec664b93c5e1e4281ea112497f2dc2ce78eb4338160b83b67d2da6ec2ba)]
UPDATE FSServiceOrder
	SET FSServiceOrder.AllowInvoice = 1
FROM FSServiceOrder
WHERE FSServiceOrder.AllowInvoice IS NULL;
GO

--[MinVersion(Hash = 34d774a1a996d335236a1a2ed0826d705aad3ebf8493565674b54cb214e0b085)]
UPDATE FSRouteSetup SET EnableSeasonScheduleContract = 0 WHERE EnableSeasonScheduleContract IS NULL;
GO

--[MinVersion(Hash = 8dd1bd9aedf6e62b2d747e41a6fa690727abd07e6708ce468c9b2bcfb806986e)]
UPDATE FSSetup SET EquipmentCalculateWarrantyFrom = 'SD' WHERE EquipmentCalculateWarrantyFrom IS NULL;
GO

--[IfExists(Column = INItemClass.ItemClassID)]
--[IfExists(Column = INItemClass.ItemClassCD)]
--[IfExists(Column = FSEquipmentComponent.RemovedItemClassID)]
--[MinVersion(Hash = 20f56dd7d0ed7262f715e0b82bd7a2e00eeacf9b61d7d2fb49b7ec1bd2792429)]
UPDATE FSEquipmentComponent
SET ItemClassID = INItemClass.ItemClassID
FROM FSEquipmentComponent
INNER JOIN INItemClass ON INItemClass.CompanyID = FSEquipmentComponent.CompanyID AND INItemClass.ItemClassCD = FSEquipmentComponent.RemovedItemClassID;
GO

--[IfExists(Column = INItemClass.ItemClassID)]
--[IfExists(Column = INItemClass.ItemClassCD)]
--[IfExists(Column = FSModelComponent.RemovedClassID)]
--[MinVersion(Hash = cccf0ab776c56277226f36a140bda764a9d6450d59df71c17c2ea9b98edf9809)]
UPDATE FSModelComponent
SET ClassID = INItemClass.ItemClassID
FROM FSModelComponent
INNER JOIN INItemClass ON INItemClass.CompanyID = FSModelComponent.CompanyID AND INItemClass.ItemClassCD = FSModelComponent.RemovedClassID;
GO

--[IfExists(Column = INItemClass.ItemClassID)]
--[IfExists(Column = INItemClass.ItemClassCD)]
--[IfExists(Column = FSModelTemplateComponent.RemovedClassID)]
--[MinVersion(Hash = be77498716d6dd6b1eaf57206fa7b8ab5f4a67c06b8b4f050a605a5d8abd160f)]
UPDATE FSModelTemplateComponent
SET ClassID = INItemClass.ItemClassID
FROM FSModelTemplateComponent
INNER JOIN INItemClass ON INItemClass.CompanyID = FSModelTemplateComponent.CompanyID AND INItemClass.ItemClassCD = FSModelTemplateComponent.RemovedClassID;
GO

--[MinVersion(Hash = e25313ccdb3e030b90bbdcbfd4e34b18115596e6872a386dfce27d20c5a1dbb7)]
UPDATE FSEquipment SET CpnyWarrantyType = 'M' WHERE CpnyWarrantyType IS NULL;
GO

--[MinVersion(Hash = 93c9b2b74d5fd753121e6568bf694343076b743ad41a59efd147de21ed4b2f8e)]
UPDATE FSEquipment SET VendorWarrantyType = 'M' WHERE VendorWarrantyType IS NULL;
GO

--[IfExists(Column = FSEquipment.Status)]
--[MinVersion(Hash = 0f7f81aefcd529a0ef609b91775c265da37eb34e0feb61fbe64c9e769bd5ab4d)]
UPDATE FSEquipment SET Status = 'S' WHERE Status = 'I';
GO

--[MinVersion(Hash = 4fd37bb7fe61b91e33b23f182c91999af464ebdef65fdebb12a019c52977d237)]
UPDATE FSxEquipmentModelTemplate SET EquipmentItemClass = 'OI' WHERE EQEnabled = 0;
UPDATE FSxEquipmentModelTemplate SET EquipmentItemClass = 'ME' WHERE EQEnabled = 1;
GO

--[MinVersion(Hash = 4855a4485983db66c8eebc874ce6fe84b4678bb7f991f41da93ba710bf3e4550)]
UPDATE FSxEquipmentModel SET EquipmentItemClass = 'OI' WHERE EQEnabled = 0;
UPDATE FSxEquipmentModel SET EquipmentItemClass = 'ME' WHERE EQEnabled = 1;
GO

--[MinVersion(Hash = f45572638329cb2f630087294e8819ddd349f11531fd03f2b6ae00acb3b48efb)]
UPDATE FSxEquipmentModel SET CpnyWarrantyValue = 0 WHERE FSxEquipmentModel.CpnyWarrantyValue IS NULL;
GO

--[MinVersion(Hash = ff46e9dce364141d41c33ca14aba41214fc1966275ea095ccc866fb0315eda02)]
UPDATE FSxEquipmentModel SET VendorWarrantyValue = 0 WHERE FSxEquipmentModel.VendorWarrantyValue IS NULL;
GO

--[MinVersion(Hash = 4124fc06e2b6379ff6802cf8cf279b15ec97aadde29bdeeaf41bddf3562a9861)]
UPDATE FSxEquipmentModel SET CpnyWarrantyType = 'M' WHERE FSxEquipmentModel.CpnyWarrantyType IS NULL;
GO

--[MinVersion(Hash = 5ccfd2fcd7237c132888229ad164035ec3b086588763948b5c3e348ad6bb3d56)]
UPDATE FSxEquipmentModel SET VendorWarrantyType = 'M' WHERE FSxEquipmentModel.VendorWarrantyType IS NULL;
GO

--[MinVersion(Hash = b3e87b76b21880916ae46552180edca33a684af2b3a79296e2c8fa3db1a2f9fd)]
UPDATE FSModelComponent SET VendorWarrantyType = 'M' WHERE FSModelComponent.VendorWarrantyType IS NULL;
GO

--[MinVersion(Hash = 8f0698976e4bea81b7202cfdeb10d1e5b4349b1fa5970b5170f7fc031eb3f0fa)]
UPDATE FSModelComponent SET CpnyWarrantyType = 'M' WHERE FSModelComponent.CpnyWarrantyType IS NULL;
GO

--[IfExists(Column = FSModelComponent.CpnyWarrantyDuration)]
--[IfExists(Column = FSModelComponent.CpnyWarrantyValue)]
--[MinVersion(Hash = e8b4cc13b1bb12d42b7fdfa76778b8abd1aec17bd246986f6cb8d7c3d4dc398c)]
UPDATE FSModelComponent SET CpnyWarrantyValue = CpnyWarrantyDuration WHERE CpnyWarrantyValue IS NULL;
GO

--[IfExists(Column = FSModelComponent.VendorWarrantyDuration)]
--[IfExists(Column = FSModelComponent.VendorWarrantyValue)]
--[MinVersion(Hash = be7ee0fc3bd5081d3c8f1cdf752ff33620a9ce2a782783981a564814b720cbb3)]
UPDATE FSModelComponent SET VendorWarrantyValue = VendorWarrantyDuration WHERE VendorWarrantyValue IS NULL;
GO

--[MinVersion(Hash = 949c0315b25b140037f6084032c6cb8fb7803a40e4b2debd389d33cf3bc4fa0d)]
UPDATE FSxSOLine SET EquipmentAction = 'NO' WHERE FSxSOLine.EquipmentAction IS NULL;
GO

--[MinVersion(Hash = 63ac7ead3e9bfcdf5de38ff75cc0571b552223875d237846d4e6c32455de2145)]
UPDATE FSModelComponent SET IsOptional = 0 WHERE FSModelComponent.IsOptional IS NULL;
GO

--[mysql: Skip]
--mysql: does not work with variable and EXEC sp_executesql
--[IfExists(Table = FSEquipmentWarranty)]
--[MinVersion(Hash = d54d90a1bcafe65c6f4b0a50a399fb545c099d966da166f15418c50fedb401e2)]
declare @myquery nvarchar(max);

set @myquery = 
N'
    INSERT INTO FSModelTemplateComponent
         (CompanyID
         ,ModelTemplateID
         ,Active
         ,TemplateComponentCD
         ,Descr
         ,CreatedByID
         ,CreatedByScreenID
         ,CreatedDateTime
         ,LastModifiedByID
         ,LastModifiedByScreenID
         ,LastModifiedDateTime
         ,ClassID)
    SELECT
          FSEquipmentWarranty.CompanyID
         ,InventoryItem.ItemClassID
         ,1
         ,UPPER(RTRIM(FSEquipmentWarranty.ComponentID))
         ,LEFT(RTRIM(FSEquipmentWarranty.LongDescr), 250)
         ,''B5344897-037E-4D58-B5C3-1BDFD0F47BF9''
         ,''00000000''
         ,GETDATE()
         ,''B5344897-037E-4D58-B5C3-1BDFD0F47BF9''
         ,''00000000''
         ,GETDATE()
         ,NULL
    FROM FSEquipmentWarranty
    INNER JOIN FSEquipment ON (
    FSEquipment.CompanyID = FSEquipmentWarranty.CompanyID
        AND FSEquipment.SMEquipmentID = FSEquipmentWarranty.SMEquipmentID)
    INNER JOIN InventoryItem ON (
    InventoryItem.CompanyID = FSEquipmentWarranty.CompanyID
        AND InventoryItem.InventoryID = FSEquipment.InventoryID)
    LEFT JOIN FSModelTemplateComponent ON (
    FSModelTemplateComponent.CompanyID = FSEquipmentWarranty.CompanyID
        AND FSModelTemplateComponent.ModelTemplateID = InventoryItem.ItemClassID
        AND FSModelTemplateComponent.TemplateComponentCD = UPPER(RTRIM(FSEquipmentWarranty.ComponentID)))
    WHERE
        FSModelTemplateComponent.TemplateComponentCD IS NULL
';
EXEC sp_executesql @myquery;
GO

--[IfExists(Column = FSModelTemplateComponent.TemplateComponentID)]
--[IfExists(Column = FSModelTemplateComponent.ComponentID)]
--[MinVersion(Hash = a8babed57e2dcbad3e7e8f6d1f9d75c716e007404a70dce3c44fd52358ccef12)]
UPDATE FSModelTemplateComponent SET ComponentID = TemplateComponentID WHERE ComponentID IS NULL;
GO

--[IfExists(Column = FSModelTemplateComponent.TemplateComponentCD)]
--[IfExists(Column = FSModelTemplateComponent.ComponentCD)]
--[MinVersion(Hash = c3a5cd6add76b021e5a0c5ecde9413cce65e8581b46c9b0ed44286e03ef066e3)]
UPDATE FSModelTemplateComponent SET ComponentCD = TemplateComponentCD WHERE ComponentCD IS NULL;
GO

--[mysql: Skip]
--mysql: cannot ROW_NUMBER() OVER(PARTITION BY ... 
--[IfExists(Table = FSEquipmentWarranty)]
--[IfNotExistsSelect(From = FSEquipmentComponent)]
--[MinVersion(Hash = 6ed307c97ae69bc680fa252e983da5506f8a0ca3a3f9560f528d09294b95b91f)]
declare @myquery nvarchar(max);

set @myquery = 
N'
     INSERT INTO FSEquipmentComponent
           (CompanyID
           ,SMEquipmentID
           ,LineNbr
           ,Comment
           ,ComponentID
           ,ComponentReplaced
           ,CpnyWarrantyDuration
           ,CpnyWarrantyEndDate
           ,CpnyWarrantyType
           ,InstallationDate
           ,InstAppointmentID
           ,InstServiceOrderID
           ,InventoryID
           ,InvoiceRefNbr
           ,ItemClassID
           ,LastReplacementDate
           ,LineRef
           ,LongDescr
           ,RequireSerial
           ,SalesDate
           ,SalesOrderNbr
           ,SalesOrderType
           ,SerialNumber
           ,Status
           ,VendorID
           ,VendorWarrantyDuration
           ,VendorWarrantyEndDate
           ,VendorWarrantyType
           ,CreatedByID
           ,CreatedByScreenID
           ,CreatedDateTime
           ,LastModifiedByID
           ,LastModifiedByScreenID
           ,LastModifiedDateTime)
     SELECT
            FSEquipmentWarranty.CompanyID
           ,FSEquipmentWarranty.SMEquipmentID
           ,ROW_NUMBER() OVER(PARTITION BY FSEquipmentWarranty.CompanyID, FSEquipmentWarranty.SMEquipmentID ORDER BY FSEquipmentWarranty.CreatedDateTime)
           ,NULL
           ,FSModelTemplateComponent.ComponentID
           ,NULL
           ,CpnyWarrantyDuration
           ,FSEquipmentWarranty.CpnyWarrantyEndDate
           ,''M''
           ,InstallationDate
           ,NULL
           ,NULL
           ,NULL
           ,NULL
           ,NULL
           ,LastReplacementDate
           ,''.....''
           ,LongDescr
           ,RequireSerial
           ,NULL
           ,NULL
           ,NULL
           ,FSEquipmentWarranty.SerialNumber
           ,''A''
           ,FSEquipmentWarranty.VendorID
           ,VendorWarrantyDuration
           ,FSEquipmentWarranty.VendorWarrantyEndDate
           ,''M''
           ,FSEquipmentWarranty.CreatedByID
           ,FSEquipmentWarranty.CreatedByScreenID
           ,FSEquipmentWarranty.CreatedDateTime
           ,FSEquipmentWarranty.LastModifiedByID
           ,FSEquipmentWarranty.LastModifiedByScreenID
           ,FSEquipmentWarranty.LastModifiedDateTime
     FROM FSEquipmentWarranty
    INNER JOIN FSEquipment ON (
      FSEquipment.CompanyID = FSEquipmentWarranty.CompanyID
      AND FSEquipment.SMEquipmentID = FSEquipmentWarranty.SMEquipmentID)
    INNER JOIN InventoryItem ON (
      InventoryItem.CompanyID = FSEquipmentWarranty.CompanyID
      AND InventoryItem.InventoryID = FSEquipment.InventoryID)
    INNER JOIN FSModelTemplateComponent ON (
      FSModelTemplateComponent.CompanyID = FSEquipmentWarranty.CompanyID
      AND FSModelTemplateComponent.ModelTemplateID = InventoryItem.ItemClassID
      AND FSModelTemplateComponent.ComponentCD = UPPER(RTRIM(FSEquipmentWarranty.ComponentID)))
';
EXEC sp_executesql @myquery;

set @myquery = 
N'UPDATE FSEquipmentComponent SET LineRef = RIGHT(''00000'' + CAST(LineNbr AS VARCHAR(5)), 5) WHERE LineRef = ''.....'' ';
EXEC sp_executesql @myquery;
GO

--[IfExists(Table = FSEquipmentWarranty)]
--[mysql: Native]
--[mssql: Skip]
--[MinVersion(Hash = e8eadb1e2ba40ec078f1ceff741de2b1ccfecad9f1e2daa2e375aeab512850fc)]
RENAME TABLE FSEquipmentWarranty TO Obsolete_FSEquipmentWarranty;
GO

--[IfExists(Table = FSEquipmentWarranty)]
--[mysql: Skip]
--[mssql: Native]
--[MinVersion(Hash = 9879af91c02204bb0769f3a4eac2213c5f5ba120da1f422cb81e28da05202be7)]
EXEC sp_rename 'FSEquipmentWarranty', 'Obsolete_FSEquipmentWarranty';
GO

--[MinVersion(Hash = 36790e7ab13e4fac8bf90c2117add681c27c7d6e4577d678de3f11a160871d82)]
UPDATE FSEquipmentComponent SET CpnyWarrantyType = 'M' WHERE CpnyWarrantyType IS NULL;
GO

--[MinVersion(Hash = f549ab2c123c0bb2824b02ae22275afa46b2c20bb83671930c7f189a0b9d25f9)]
UPDATE FSEquipmentComponent SET VendorWarrantyType = 'M' WHERE VendorWarrantyType IS NULL;
GO

--[MinVersion(Hash = aeef46f1b11e6c59ce8fbb567addf0755bc668d497e32bf533bddb319d9d733b)]
UPDATE FSEquipmentComponent SET Status = 'A' WHERE Status IS NULL;
GO

--[MinVersion(Hash = 5860b7557a1a82e426676f3e7d04a0b44934e737436d40b023c30ad932b9eacc)]
UPDATE FSModelTemplateComponent SET Optional = 0 WHERE Optional IS NULL;
GO

--[MinVersion(Hash = 1d4dca44a19a1a991b7a92afdaf06716d473e330ac2c326bb93e710ea657ade0)]
UPDATE FSModelTemplateComponent SET Qty = 1 WHERE Qty IS NULL;
GO

--[MinVersion(Hash = 5e5891a6faec24e756626d667bafdb84ecb7606396b8b6e7f953773e618993fa)]
UPDATE FSModelComponent SET Optional = 0 WHERE Optional IS NULL;
GO

--[MinVersion(Hash = f923ce79e6d55801c3a45095a4000ad95e6526ccbb929f29b88b086ba5775dd3)]
UPDATE FSModelComponent SET Qty = 1 WHERE Qty IS NULL;
GO

--[MinVersion(Hash = b959d0baa98a1c1c8a7d3326c7754eeeff5f17c172ddada17fb3491f227cfa34)]
UPDATE FSAppointmentDet SET EquipmentAction = 'NO' WHERE FSAppointmentDet.EquipmentAction IS NULL;
GO

--[MinVersion(Hash = b3d1ad4a24b04876bf7a64c3bd1dc006538cf4d50b433795d567c6fcc5ec3cab)]
UPDATE FSSODet SET EquipmentAction = 'NO' WHERE FSSODet.EquipmentAction IS NULL;
GO

--[MinVersion(Hash = 59c6e2dffc44f24e3f2311da113fbec5abfe3cce1c9c8b5c30298dd0a92febd2)]
UPDATE FSScheduleDet SET EquipmentAction = 'NO' WHERE FSScheduleDet.EquipmentAction IS NULL;
GO

--[MinVersion(Hash = deb72f341628ad00bfc5259ffb441cde8d872b3e552ead1fd9593db6afac7f6b)]
UPDATE FSAppointmentDet SET Warranty = 0 WHERE FSAppointmentDet.Warranty IS NULL;
GO

--[MinVersion(Hash = 26537ed1195627a418ad7ca7b7bcfabff71c1987c257816ebcc97ac8cd1a66cc)]
UPDATE FSSODet SET Warranty = 0 WHERE FSSODet.Warranty IS NULL;
GO

--[MinVersion(Hash = 879c106d14768e0bc70bca60f21b601abf84eb7a8fbca3e5aab1b49b2704b673)]
UPDATE FSxEquipmentModel SET EQEnabled = 1 WHERE EquipmentItemClass = 'CT';
GO

--[IfExists(Column = FSxSOLine.AppointmentDate)]
--[IfExists(Column = FSxSOLine.ServiceOrderDate)]
--[MinVersion(Hash = 6ec64772315994e0f2cbbeb3af29371d500d1e92949a44f75bfb59954c8e1f56)]
UPDATE FSxSOLine
SET 
  ServiceOrderDate = FSServiceOrder.OrderDate,
  AppointmentDate = FSAppointment.ExecutionDate
FROM FSxSOLine
INNER JOIN FSServiceOrder ON FSServiceOrder.CompanyID = FSxSOLine.CompanyID AND FSServiceOrder.SOID = FSxSOLine.SOID
LEFT JOIN FSAppointment ON FSAppointment.CompanyID = FSxSOLine.CompanyID AND FSAppointment.AppointmentID = FSxSOLine.AppointmentID;
GO

--[IfExists(Column = FSxARTran.AppointmentDate)]
--[IfExists(Column = FSxARTran.ServiceOrderDate)]
--[MinVersion(Hash = d00a29117d7fd02c168c32b686b302b3bf0f8d72d2ca08680ef835d1d25c5c11)]
UPDATE FSxARTran
SET 
  ServiceOrderDate = FSServiceOrder.OrderDate,
  AppointmentDate = FSAppointment.ExecutionDate
FROM FSxARTran
INNER JOIN FSServiceOrder ON FSServiceOrder.CompanyID = FSxARTran.CompanyID AND FSServiceOrder.SOID = FSxARTran.SOID
LEFT JOIN FSAppointment ON FSAppointment.CompanyID = FSxARTran.CompanyID AND FSAppointment.AppointmentID = FSxARTran.AppointmentID;
GO

--[IfExists(Column = FSxAPTran.AppointmentDate)]
--[IfExists(Column = FSxAPTran.ServiceOrderDate)]
--[MinVersion(Hash = 05b820359d8376d000fb17e0ef75a7003587d3c26848215136ca5748f01295b6)]
UPDATE FSxAPTran
SET 
  ServiceOrderDate = FSServiceOrder.OrderDate,
  AppointmentDate = FSAppointment.ExecutionDate
FROM FSxAPTran
INNER JOIN FSServiceOrder ON FSServiceOrder.CompanyID = FSxAPTran.CompanyID AND FSServiceOrder.SOID = FSxAPTran.SOID
LEFT JOIN FSAppointment ON FSAppointment.CompanyID = FSxAPTran.CompanyID AND FSAppointment.AppointmentID = FSxAPTran.AppointmentID;
GO

--[MinVersion(Hash = 417049e1f8168f86ea7a35c085e184c93e650d3e9ca478abba877f6afc658f5b)]
update SOSetup
set SalesProfitabilityForNSKits = 'C'
where SalesProfitabilityForNSKits is null
GO

-- Set the right UserID for Employees (get UserID from the corresponded contact)
--[MinVersion(Hash = 89d48ec0ce413c9648ef9782bc4b405fc88a9a82b68224c0f368765f41b21455)]
UPDATE EPEmployee SET EPEmployee.UserID = Contact.UserID
FROM EPEmployee
LEFT JOIN BAccount ON BAccount.CompanyID = EPEmployee.CompanyID AND BAccount.BAccountID = EPEmployee.BAccountID
LEFT JOIN Contact ON Contact.CompanyID = BAccount.CompanyID AND Contact.ContactID = BAccount.DefContactID
WHERE ISNULL(EPEmployee.UserID, '00000000-0000-0000-0000-000000000000') <> ISNULL(Contact.UserID, '00000000-0000-0000-0000-000000000000')
GO

-- update Contact CreatedDateTime to UTC
--[Timeout(Multiplier = 100)]
--[mysql: Skip]
--[MinVersion(Hash = 8a1d632628c305d8de2db37fb145ce3fc0d4bd40b167dec7e38b584bd6ccf3b5)]
UPDATE Contact SET CreatedDateTime = DATEADD(hh, DATEDIFF(hh, GETDATE(), GETUTCDATE()), CreatedDateTime)
GO

-- update Contact CreatedDateTime to UTC
--[mysql: Native]
--[mssql: Skip]
--[MinVersion(Hash = c12c629d095bdec017ac49a8ff2aa46e34b602519f6d2578aa3c8ff3b61dc912)]
UPDATE Contact SET CreatedDateTime = DATE_ADD(CreatedDateTime, INTERVAL TIMEDIFF(UTC_TIMESTAMP(), NOW()) HOUR_SECOND)
GO

--[MinVersion(Hash = 8b73085a20921f0c38296c2753c389f63f49f406ed2069a3f8c2865c9bc51e66)]
UPDATE APRegister 
SET CuryChargeAmt = IsNull((SELECT
SUM(CuryTranAmt) AS curyamt
FROM  APPaymentChargeTran charge
WHERE APRegister.CompanyID=charge.CompanyID
AND APRegister.DocType=charge.DocType
AND APRegister.RefNbr=charge.RefNbr
GROUP BY CompanyID, DocType, RefNbr),0);
GO

--[MinVersion(Hash = b16ea526a1846ab316eb2bd668a69d0e22951577782f7413941b23d036b0c5f4)]
UPDATE APRegister 
SET ChargeAmt = IsNull((SELECT
SUM(TranAmt) AS curyamt
FROM  APPaymentChargeTran charge
WHERE APRegister.CompanyID=charge.CompanyID
AND APRegister.DocType=charge.DocType
AND APRegister.RefNbr=charge.RefNbr
GROUP BY CompanyID, DocType, RefNbr),0);
GO

--[MinVersion(Hash = b8235311485169f3535cd0328e4e7de9438fb89184ae905f8ffac9d8f350e217)]
UPDATE ARRegister 
SET CuryChargeAmt = IsNull((SELECT
SUM(CuryTranAmt) AS curyamt
FROM  ARPaymentChargeTran charge
WHERE ARRegister.CompanyID=charge.CompanyID
AND ARRegister.DocType=charge.DocType
AND ARRegister.RefNbr=charge.RefNbr
GROUP BY CompanyID, DocType, RefNbr),0);
GO

--[MinVersion(Hash = 259d4e8b04dedec4e1767842069f3178092297abc1a3a45d500e1014fd13ea99)]
UPDATE ARRegister 
SET ChargeAmt = IsNull((SELECT
SUM(TranAmt) AS curyamt
FROM  ARPaymentChargeTran charge
WHERE ARRegister.CompanyID=charge.CompanyID
AND ARRegister.DocType=charge.DocType
AND ARRegister.RefNbr=charge.RefNbr
GROUP BY CompanyID, DocType, RefNbr),0);
GO

--[MinVersion(Hash = bb6edbd0b6212e2d0a40c462b31849d01778cb9f8cfc71db38fb6e43dd10a22c)]
UPDATE FSAppointmentEmployee SET LineNbr = EmployeeID WHERE LineNbr IS NULL
UPDATE FSSOEmployee SET LineNbr = EmployeeID WHERE LineNbr IS NULL
GO

--[MinVersion(Hash = 205d1f35c59ba778f4a17f756a16a745b370fc5e631953a192c9f016f44a0c96)]
ALTER TABLE FSPostDet ALTER COLUMN INDocType CHAR(1) NULL
ALTER TABLE FSPostInfo ALTER COLUMN INDocType CHAR(1) NULL
GO

--[MinVersion(Hash = b0a3afc29864921738c2afd839b28d3e4a3458c605d98a2ffdee9d87ce05c8d4)]
update APRegister
set APRegister.OrigModule='EP', APRegister.OrigRefNbr=EPExpenseClaim.RefNbr
from APRegister
inner join EPExpenseClaim 
	on APRegister.CompanyID=EPExpenseClaim.CompanyID
	and APRegister.RefNbr=EPExpenseClaim.APRefNbr
	and APRegister.DocType=EPExpenseClaim.APDocType
WHERE APRegister.OrigRefNbr is null
GO

--[MinVersion(Hash = 47545f54ffd18f93cc1e2ab820cadb02db0ad26157672a485eb2abd2473bb9c7)]
insert into dbo.EPTaxAggregate (CompanyID, RefNbr, TaxID, TaxRate, CuryInfoID, CuryTaxableAmt, TaxableAmt, CuryTaxAmt, TaxAmt, NonDeductibleTaxRate, ExpenseAmt, CuryExpenseAmt, CreatedByID, CreatedByScreenID, CreatedDateTime, LastModifiedByID, LastModifiedByScreenID, LastModifiedDateTime)
select CompanyID, RefNbr, TaxID, TaxRate, CuryInfoID, CuryTaxableAmt, TaxableAmt, CuryTaxAmt, TaxAmt, NonDeductibleTaxRate, ExpenseAmt, CuryExpenseAmt, CreatedByID, CreatedByScreenID, CreatedDateTime, LastModifiedByID, LastModifiedByScreenID, LastModifiedDateTime from EPTax
where ClaimDetailID=2147483647;
GO

--[MinVersion(Hash = 26df29f1a838258a33d56c2283160c14c94356c24665545ff798eb9c80c326ed)]
delete from EPTax;
GO

--[MinVersion(Hash = 5c2a0356984d7c039ae64e00321ce6e6ac921dbb36ba76f945b98d52aa2bb3db)]
update EPExpenseClaim set CuryTaxTotal=IsNull((select SUM(CuryTaxAmt) + SUM(CuryExpenseAmt) 
from EPTaxAggregate 
where EPTaxAggregate.RefNbr = EPExpenseClaim.RefNbr and EPTaxAggregate.CompanyID = EPExpenseClaim.CompanyID 
group by EPTaxAggregate.CompanyID, EPTaxAggregate.RefNbr),0);
GO

--[MinVersion(Hash = 6437ae71760c2af5bc04becbdde906735eef79003b17d31a07f5bb05c2d37188)]
update EPExpenseClaim set TaxTotal=IsNull((select SUM(TaxAmt) + SUM(ExpenseAmt) 
from EPTaxAggregate 
where EPTaxAggregate.RefNbr = EPExpenseClaim.RefNbr and EPTaxAggregate.CompanyID = EPExpenseClaim.CompanyID 
group by EPTaxAggregate.CompanyID, EPTaxAggregate.RefNbr),0);
GO

--[MinVersion(Hash = 0a71601a34c9ebdffe76f733b5159c20fd906c5477c2d6e90645037a4ff186c4)]
update EPExpenseClaimDetails set SubmitedDate=LastModifiedDateTime where RefNbr is not null;
GO

--[MinVersion(Hash = 8c9c610b8e190310fd07dc5ebf081a7eb666f8058839328ec12daf8803e72784)]
update EPExpenseClaimDetails set CreatedFromClaim=1 where CreatedByScreenID='EP301000';
GO

--[MinVersion(Hash = 5d309bf212fa616813eaae236cf70e0797a55a1d179b4eab5b48cc3d7f8b4a2e)]
UPDATE ARSetup SET NoteID = NEWID() WHERE NoteID IS NULL

update EPExpenseClaimDetails set CuryID=(select CuryID from CurrencyInfo where curyInfoID=EPExpenseClaimDetails.curyInfoID and CurrencyInfo.CompanyID = EPExpenseClaimDetails.CompanyID);
GO

--[MinVersion(Hash = 38719fad18ec2f29b6c953626f56d0598a91e025dedacb015311e45f99ad9258)]
update EPExpenseClaimDetails set TaxZoneID=(select TaxZoneID from EPExpenseClaim where EPExpenseClaim.RefNbr=EPExpenseClaimDetails.RefNbr and EPExpenseClaim.CompanyID = EPExpenseClaimDetails.CompanyID);
GO

--[MinVersion(Hash = 042aa18e63618294b004e4a4939581d70bd0bc03a3883216334dddba74f46c4a)]
update EPExpenseClaimDetails set LegacyReceipt=1 where TaxZoneID is not null;
GO

--[MinVersion(Hash = 01912dc3f86df8a291f0fb7f10b7d3998dd5a0e5529bd29e51995a9ff3cc3ce6)]
update EPExpenseClaimDetails set 
CuryAmountWithTaxes=CuryTranAmt, CuryTranAmtWithTaxes=CuryTranAmt, CuryTaxableAmt=CuryTranAmt, 
TaxableAmt=TranAmt, TranAmtWithTaxes=TranAmt,
ClaimTranAmtWithTaxes=ClaimTranAmt, ClaimCuryTranAmtWithTaxes = ClaimCuryTranAmt;
GO

--[MinVersion(Hash = 3953ad9ad7e14dc253917f16c477c05428b0aaf9809f98b32902c10811bd69ef)]
UPDATE FSxCustomer SET DefaultBillingCustomerSource = 'SO' WHERE FSxCustomer.DefaultBillingCustomerSource IS NULL;
GO

--[MinVersion(Hash = 14db14db5e07f6f0f974056582e1dd6b48b7f9c35cb2e520abec2a95b34df15c)]
UPDATE FSxCustomerClass SET DefaultBillingCustomerSource = 'SO' WHERE FSxCustomerClass.DefaultBillingCustomerSource IS NULL;
GO

--[IfExists(Column = FSServiceOrder.OrderTotal)]
--[MinVersion(Hash = 94acf6eba0fbb7ed962cae19b97d930d27148987b9d63d7214a14a319c260acc)]
UPDATE  FSServiceOrder
	SET EstimatedOrderTotal = OrderTotal
WHERE EstimatedOrderTotal is NULL;
GO

--[IfExistsSelect(From = FSServiceOrder, WhereIsNull = ApptOrderTotal)]
--[MinVersion(Hash = e814a8e15a0889a1d5cafac312db9b41ff9e6a9893ca6c50b2f050ca24e63619)]
UPDATE  FSServiceOrder
	SET ApptOrderTotal = ISNULL((Select SUM(LineTotal) FROM FSAppointment 
							WHERE FSAppointment.CompanyID = FSServiceOrder.CompanyID
								AND FSAppointment.SOID = FSServiceOrder.SOID
								AND FSAppointment.Status <> 'X'), 0)
WHERE FSServiceOrder.ApptOrderTotal is NULL;
GO

--[IfExists(Column = FSServiceOrder.CuryApptOrderTotal)]
--[IfExistsSelect(From = FSServiceOrder, WhereIsNull = CuryApptOrderTotal)]
--[MinVersion(Hash = 9be122af1b6d86e27637d1c3ec856a1fb761a13c0373fee286ab3063188c9f70)]
UPDATE FSServiceOrder
	SET CuryApptOrderTotal = ApptOrderTotal
WHERE FSServiceOrder.CuryApptOrderTotal is NULL;
GO

--[MinVersion(Hash = 90cd3a5eb85388b8cd2042d188932f2dfd1378254c77df431304fbc9aa91f715)]
UPDATE  FSServiceOrder
	SET ApptDurationTotal = (Select SUM(ActualDurationTotal) FROM FSAppointment 
								WHERE FSAppointment.CompanyID = FSServiceOrder.CompanyID
									AND FSAppointment.SOID = FSServiceOrder.SOID
									AND FSAppointment.Status <> 'X')
WHERE FSServiceOrder.ApptDurationTotal is NULL AND
FSServiceOrder.Status NOT IN ('X','Z');
GO

--[IfExists(Column = FSSODet.Qty)]
--[MinVersion(Hash = 056b7b53b28abcf294e5d32c407f76652708f91ed18ce809393901d141d4e1de)]
UPDATE  FSSODet
	SET EstimatedQty = Qty
WHERE EstimatedQty is NULL;
GO

--[IfExists(Column = FSSODet.TranAmt)]
--[MinVersion(Hash = 71da2c633a2e50190ba4acb5e4e3f4157a2b41f8070efb2cf0da068ad1874507)]
UPDATE  FSSODet
	SET EstimatedTranAmt = TranAmt
WHERE EstimatedTranAmt is NULL;
GO

--[IfExists(Column = FSSODet.ApptNumber)]
--[MinVersion(Hash = d33899048526c16c71cb4334c558589591150e99e079c31180ce4a8d735ed4f2)]
UPDATE  FSSODet
SET ApptDuration = CASE WHEN ApptDuration IS NULL THEN ISNULL(A.TotalActualDuration, 0) ELSE ApptDuration END,
ApptQty = CASE WHEN ApptQty IS NULL THEN ISNULL(A.TotalQty, 0) ELSE ApptQty END,
ApptTranAmt = CASE WHEN ApptTranAmt IS NULL THEN ISNULL(A.TotalTranAmt, 0) ELSE ApptTranAmt END,
ApptNumber = CASE WHEN ApptNumber IS NULL THEN ISNULL(A.NumAppointments, 0) ELSE ApptNumber END
FROM FSSODet
LEFT JOIN (
	SELECT CompanyID, SODetID,TotalActualDuration = SUM(ActualDuration), TotalQty = SUM(Qty),
			TotalTranAmt = SUM(TranAmt), NumAppointments = COUNT(DISTINCT(FSAppointmentDet.AppointmentID))
	FROM FSAppointmentDet 
	WHERE FSAppointmentDet.Status <> 'X'
	GROUP BY FSAppointmentDet.CompanyID, FSAppointmentDet.SODetID
) A ON A.CompanyID = FSSODet.CompanyID AND A.SODetID = FSSODet.SODetID
WHERE ApptDuration IS NULL OR ApptQty IS NULL OR ApptTranAmt IS NULL OR  ApptNumber IS NULL
GO

--[OldHash(Hash = cec7cc1f0356e90a8ae5edcc76c526af2a63841a1bc5e90a5b5c21720f89e68b)]
--[OldHash(Hash = 644d3bdd3ca6c579c045fab2a0a958e1e4851ced48f4415ffbb9f3f7dffe64e7)]
--[OldHash(Hash = 84a4f148fd1a8ab13b6e0e61a3ab0833b3c0cb4915b774779326399cdfc9b627)]
--[OldHash(Hash = f062ff01ccc5abcfb8d992e596a1f68dc6b07a19fdcae11b86244e08223dd8ce)]
--[MinVersion(Hash = 17efde224ea9df09b8ffcf2d51362947b4827775d45c3913b2dde4b395c667cb)]
INSERT INTO InventoryItem ([CompanyID]
           ,[InventoryCD]
           ,[CreatedByID]
           ,[CreatedByScreenID]
           ,[CreatedDateTime]
           ,[LastModifiedByID]
           ,[LastModifiedByScreenID]
           ,[LastModifiedDateTime]
           ,[ItemType]
           ,[ItemStatus]
           ,[WeightItem]
           ,[BaseUnit]
           ,[SalesUnit]
           ,[PurchaseUnit]
		   ,[DecimalBaseUnit]
		   ,[DecimalSalesUnit]
		   ,[DecimalPurchaseUnit]
           ,[BasePrice]
           ,[Commisionable]
           ,[GroupMask]
           ,[DefaultTerm]
           ,[DefaultTermUOM]
           ,[LastStdCost]
           ,[PendingStdCost]
           ,[StdCost]
           ,[StkItem]
           ,[DefaultSubItemOnEntry]
           ,[BaseWeight]
           ,[BaseItemWeight]
           ,[BaseVolume]
           ,[BaseItemVolume]
           ,[PackageOption]
           ,[PackSeparately]
           ,[IsSplitted]
           ,[UseParentSubID]
           ,[MinGrossProfitPct]
           ,[KitItem]
           ,[DeletedDatabaseRecord]
           ,[ABCCodeIsFixed]
           ,[MovementClassIsFixed]
           ,[NonStockReceipt]
           ,[NonStockShip]
           ,[IsRUTROTDeductible]
		   ,[TaxCalcMode]
		   ,[NoteID]
		   ,[UndershipThreshold]
		   ,[OvershipThreshold])
SELECT c.[CompanyID]
          ,'<N/A>'
           ,'B5344897-037E-4D58-B5C3-1BDFD0F47BF9'
           ,'SM206036'
           ,GETDATE()
           ,'B5344897-037E-4D58-B5C3-1BDFD0F47BF9'
           ,'SM206036'
           ,GETDATE()
           ,'N'
           ,'XX'
           ,0--[WeightItem]
           ,'HOUR'--[BaseUnit]
           ,'HOUR'--[SalesUnit]
           ,'HOUR'--[PurchaseUnit]
		   ,1 --[DecimalBaseUnit]
           ,1 --[DecimalSalesUnit]
           ,1 --[DecimalPurchaseUnit]
           ,0 --[BasePrice]
           ,0 --[Commisionable]
           ,0x
           ,0 --[DefaultTerm]
           ,'Y' --[DefaultTermUOM]
           ,0 --[LastStdCost]
           ,0 --[PendingStdCost]
           ,0 --[StdCost]
           ,0 --[StkItem]
           ,0 --[DefaultSubItemOnEntry]
           ,0 --[BaseWeight]
           ,0 --[BaseItemWeight]
           ,0 --[BaseVolume]
           ,0 --[BaseItemVolume]
           ,'N' --[PackageOption]
           ,0 --[PackSeparately]
           ,0 --[IsSplitted]
           ,0 --[UseParentSubID]
           ,0 --[MinGrossProfitPct]
           ,0 --[KitItem]
           ,0 --[DeletedDatabaseRecord]
           ,0 --[ABCCodeIsFixed]
           ,0 --[MovementClassIsFixed]
           ,0 --[NonStockReceipt]
           ,0 --[NonStockShip]
           ,0 --[IsRUTROTDeductible]
		   ,'T' --[TaxCalcMode]
		   ,NEWID() --[NoteID]
		   ,100 --[UndershipThreshold]
		   ,100 --[OvershipThreshold]
FROM Company c
LEFT JOIN InventoryItem a ON a.CompanyID = c.CompanyID AND a.ItemStatus = 'XX'
WHERE a.InventoryCD is NULL AND c.IsReadOnly=0

UPDATE b
SET b.InventoryID = i.InventoryID
FROM PMBudget b
INNER JOIN InventoryItem i ON i.CompanyID = b.CompanyID AND i.InventoryCD='<N/A>'
WHERE b.InventoryID = 0

UPDATE h
SET h.InventoryID = i.InventoryID
FROM PMHistory h
INNER JOIN InventoryItem i ON i.CompanyID = h.CompanyID AND i.InventoryCD='<N/A>'
WHERE h.InventoryID = 0

UPDATE c
SET c.InventoryID = i.InventoryID
FROM PMCommitment c
INNER JOIN InventoryItem i ON i.CompanyID = c.CompanyID AND i.InventoryCD='<N/A>'
WHERE c.InventoryID = 0

UPDATE c
SET c.InventoryID = i.InventoryID
FROM PMProformaLine  c
INNER JOIN InventoryItem i ON i.CompanyID = c.CompanyID AND i.InventoryCD='<N/A>'
WHERE c.InventoryID = 0

UPDATE c
SET c.InventoryID = i.InventoryID
FROM PMTaskAllocTotal  c
INNER JOIN InventoryItem i ON i.CompanyID = c.CompanyID AND i.InventoryCD='<N/A>'
WHERE c.InventoryID = 0

UPDATE c
SET c.InventoryID = i.InventoryID
FROM PMTran c
INNER JOIN InventoryItem i ON i.CompanyID = c.CompanyID AND i.InventoryCD='<N/A>'
WHERE c.InventoryID = 0

UPDATE PMSetup SET EmptyItemCode = '<N/A>' WHERE EmptyItemCode IS NULL
GO

--[MinVersion(Hash = e76718c2f1f421d8b4ab8491a7f5dae37600f6eb515b7198b9135e322c5f9ffc)]
UPDATE INItemClass
SET AvailabilitySchemeID = NULL,
	LotSerClassID = NULL
WHERE (StkItem = 0 OR StkItem IS NULL)
	AND (AvailabilitySchemeID IS NOT NULL OR LotSerClassID IS NOT NULL)
GO

--[MinVersion(Hash = b1ebaeaf1c925b70bfe9c771404da60a279e87bb5cfbe72d93cb64851a4f4ac5)]
UPDATE Branch
SET MainLogoName = LogoName
WHERE MainLogoName IS NULL
GO

--[MinVersion(Hash = 9edbdd85f0a4950583cc3f8f07ccb23820e41e6c7a77982074b976e202f1f616)]
UPDATE FeaturesSet
SET Reporting1099 = 1
WHERE (SELECT COUNT(*) FROM Vendor
		WHERE FeaturesSet.CompanyID = Vendor.CompanyID AND Vendor.Vendor1099 = 1) > 0
GO

--[MinVersion(Hash = 6e3fee985539abc163874392b2d66f511d05ef2261214e0a402212d2b7e51175)]
UPDATE FSAppointment SET NoteID = NEWID() WHERE NoteID IS NULL
GO

--[MinVersion(Hash = a42ba334c7e5005cf0a6aca7e8ea38b2c8a00c68f1fa7b4591dcdc90210bf827)]
INSERT INTO [NotificationSource]
           ([CompanyID],[SetupID],[ReportID],[Format],[Active],[CreatedByID],[CreatedDateTime],[LastModifiedByID],[LastModifiedByScreenID] ,[LastModifiedDateTime],[CreatedByScreenID],[RefNoteID])
SELECT     p.CompanyID
           ,'f3d621f9-58ba-4113-8b96-f2cc4c671ec8' AS SetupID
           ,'PM642000' AS ReportID
           ,'P' AS [Format]
           ,1 AS Active
           ,p.[CreatedByID]
           ,p.[CreatedDateTime]
           ,p.[LastModifiedByID]
           ,p.[LastModifiedByScreenID]
           ,p.[LastModifiedDateTime]
           ,p.[CreatedByScreenID]
           ,p.[NoteID]
FROM Contract p
LEFT JOIN [NotificationSource] s ON p.CompanyID = s.CompanyID AND s.RefNoteID = p.NoteID AND s.SetupID='f3d621f9-58ba-4113-8b96-f2cc4c671ec8'
WHERE p.BaseType='P' AND p.NonProject = 0 AND s.SourceID IS NULL
GO

--[MinVersion(Hash = e587b0016e3dd83d993359cc7ac1af6023e68d11e6305fc75647e8d895a975af)]
INSERT INTO [NotificationSource]
           ([CompanyID],[SetupID],[ReportID],[Format],[Active],[CreatedByID],[CreatedDateTime],[LastModifiedByID],[LastModifiedByScreenID] ,[LastModifiedDateTime],[CreatedByScreenID],[RefNoteID])
SELECT     p.CompanyID
            ,'8B6A8B13-7A69-44A6-A5F7-B573D401548D' AS SetupID
           ,'PM641000' AS ReportID
           ,'P' AS [Format]
           ,0 AS Active
           ,p.[CreatedByID]
           ,p.[CreatedDateTime]
           ,p.[LastModifiedByID]
           ,p.[LastModifiedByScreenID]
           ,p.[LastModifiedDateTime]
           ,p.[CreatedByScreenID]
           ,p.[NoteID]
FROM Contract p
LEFT JOIN [NotificationSource] s ON p.CompanyID = s.CompanyID AND s.RefNoteID = p.NoteID AND s.SetupID='8B6A8B13-7A69-44A6-A5F7-B573D401548D'
WHERE p.BaseType='P' AND p.NonProject = 0 AND s.SourceID IS NULL
GO

--[IfExists(Column = PMSetup.CostBudgetUpdateMode)]
--[MinVersion(Hash = 2fb2549c52e7317e90fc42b3e3a829008a63c6ae4a3810293f5d72d240aa4a32)]
UPDATE PMSetup SET [CostBudgetUpdateMode] = 'S' WHERE [CostBudgetUpdateMode] IS NULL
GO

--[MinVersion(Hash = 6263e90664b685b21ebb67399d3fa67e4807b571141ad2d2b1eb7698144e68d7)]
UPDATE Contract SET RestrictToEmployeeList = 0 WHERE NonProject = 1 AND RestrictToEmployeeList = 1
UPDATE Contract SET RestrictToResourceList = 0 WHERE NonProject = 1 AND RestrictToResourceList = 1
UPDATE Contract SET CustomerID = NULL WHERE NonProject = 1 AND CustomerID IS NOT NULL
GO

--[IfExists(Table = FSPostRegister)]
--[IfNotExistsSelect(From = FSPostRegister)]
--[MinVersion(Hash = ea79f0bc8e6b8053212b1a49e39112fb53d15808e9157892be74b54f96bd4afd)]
INSERT INTO [FSPostRegister]
(
  [CompanyID],
  [SrvOrdType],
  [RefNbr],
  [Type],
  [BatchID],
  [EntityType],
  [PostedTO],
  [PostDocType],
  [PostRefNbr],
  [ProcessID]
)
SELECT
  B.CompanyID,
  SO.SrvOrdType,
  CASE WHEN APP.RefNbr IS NOT NULL THEN APP.RefNbr ELSE SO.RefNbr END,
  'INVCP',
  MAX(B.BatchID),
  MAX(D.EntityType),
  MAX(D.PostedTO),
  MAX(D.PostDocType),
  MAX(D.PostRefNbr),
  MAX(D.ProcessID)
FROM FSPostBatch B
INNER JOIN FSPostDoc D ON(D.CompanyID = B.CompanyID AND D.BatchID = B.BatchID)
INNER JOIN FSServiceOrder SO ON(SO.CompanyID = D.CompanyID AND SO.SOID = D.SOID)
LEFT JOIN FSAppointment APP ON(APP.CompanyID = D.CompanyID AND APP.AppointmentID = D.AppointmentID)
GROUP BY B.CompanyID, SO.SrvOrdType, APP.RefNbr, SO.RefNbr;
GO

--[IfExists(Column = FSServiceOrder.RemovedCustWorkOrderRefNbr)]
--[MinVersion(Hash = cdf2d3077589e505500cdf5d9589ad7ba444c13eb1e78eea3e78408705006c09)]
UPDATE FSServiceOrder
SET FSServiceOrder.CustWorkOrderRefNbr = FSServiceOrder.RemovedCustWorkOrderRefNbr;
GO

--[IfExists(Column = FSServiceOrder.RemovedCustPORefNbr)]
--[MinVersion(Hash = f67d770c10f201fed24401bba3a0865f94ea08b9f2af4fd39d002ee08dddfdfa)]
UPDATE FSServiceOrder
SET FSServiceOrder.CustPORefNbr = FSServiceOrder.RemovedCustPORefNbr;
GO

--[IfExistsSelect(From = FSAppointment, WhereIsNull = NoteID)]
--[MinVersion(Hash = ca638c062a3704dda84bb8ed15942295f58280ee7e169d2454ce3e0aafd398aa)]
UPDATE FSAppointment 
SET NoteID = NEWID() 
WHERE NoteID IS NUll;
GO

--[IfExistsSelect(From = FSSchedule, WhereIsNull = ScheduleGenType)]
--[MinVersion(Hash = ee0ff1cf2ff5860d1251a8883be71c9bd7656a724250f8012b32ad25c5039ba6)]
UPDATE FSSchedule
SET FSSchedule.ScheduleGenType = 'AP' 
FROM FSSchedule
INNER JOIN FSServiceContract ON 
  FSSchedule.EntityID = FSServiceContract.ServiceContractID
  AND FSSchedule.CompanyID = FSServiceContract.CompanyID
WHERE
  FSSchedule.ScheduleGenType IS NULL
  AND FSServiceContract.RecordType = 'IRSC';
GO

--[MinVersion(Hash = c2ed480c563074998dd67be24e767369a64e7804f5f162d32fd45192e2aeeea1)]
UPDATE ARSetup SET NoteID = NEWID() WHERE NoteID IS NULL
GO

--[MinVersion(Hash = 0b7f0c6d5ef8c1233c6b930e2a1a118949d1f06b6234d3a0f8e03b2b943c95cf)]
update itch set SiteID = ins.siteid from INItemCostHist itch
join insite ins on itch.companyid = ins.companyid and itch.costsiteid = ins.siteid
where itch.siteid is null

update itch set SiteID = inl.siteid from INItemCostHist itch
join INLocation inl on itch.companyid = inl.companyid and itch.costsiteid = inl.locationid
where itch.siteid is null
GO

--[MinVersion(Hash = 0aa3e86f074cea8224ab14e3ab2ea1a8f071f23893d6ecd950b6a1ef63c97818)]
UPDATE INPlanType
SET IsSupply = 1, IsDemand = 0
WHERE PlanType IN ('44', '45') AND IsSupply = 0 AND IsDemand = 1
GO

--[MinVersion(Hash = 5aca97e4cff59fc8a35d7c1147f2e60488aca922ab259a5e29987e6b2541a102)]
update pol set Released = por.Released from POReceiptLine pol join POReceipt por on pol.ReceiptType = por.ReceiptType and pol.ReceiptNbr = por.ReceiptNbr
GO

--[MinVersion(Hash = f458e0564b2dd6c77d6d2347fe9763f7c66401dad8014e7a4397f702f3558f45)]
UPDATE ARSetup SET NoteID = NEWID() WHERE NoteID IS NULL

INSERT ARSetupKVExt (CompanyID, RecordID, FieldName, ValueString)
SELECT l.CompanyID, t.NoteID, 'PPDCreditMemoDescr' + MAX(UPPER(SUBSTRING(l.LocaleName, 1, 2))), MAX(PPDCreditMemoDescr)
FROM ARSetup t
INNER JOIN Locale l ON l.IsDefault = 1
LEFT JOIN ARSetupKVExt e ON e.CompanyID = l.CompanyID AND e.RecordID = t.NoteID AND e.FieldName = 'PPDCreditMemoDescr' + UPPER(SUBSTRING(l.LocaleName, 1, 2))
WHERE e.CompanyID IS NULL
GROUP BY l.CompanyID, t.NoteID
GO

--Set correct sync sort order for SFEntityType
--[MinVersion(Hash = 329df52f32fd6eb8fefe69480af8088beb93b145858220c05317724ff57e5f5c)]
UPDATE SFEntitySetup
SET SyncSortOrder = CASE
	WHEN EntityType = 1 THEN 1
	WHEN EntityType = 2 THEN 2
	WHEN EntityType = 3 THEN 3
	WHEN EntityType = 4 THEN 7
	WHEN EntityType = 5 THEN 4
	WHEN EntityType = 6 THEN 5
	WHEN EntityType = 7 THEN 8
	WHEN EntityType = 8 THEN 6
	END
GO

--[MinVersion(Hash = 2a49c52d0013f3e8cf16a262ecbf8290bcd2f0424139febfee953d18a8e2470d)]
update arreg 
	set origmodule = 'AR' 
from ARRegister arreg join 
	ARInvoice inv on 
	inv.CompanyID = arreg.CompanyID and
	arreg.DocType = inv.DocType and 
	arreg.RefNbr = inv.RefNbr and 
	inv.InstallmentNbr is not null and 
	arreg.OrigModule='SO'
GO

--[MinVersion(Hash = a1566ad8e7a5b8b20010c6e5edc7a7778e9900e0c5387e9a1f475637e6657266)]
UPDATE FADetails SET[Status]='H' WHERE [Status]='C'
GO

-- Add SOOrder relations to CROpportunity
--[IfExists(Column = CROpportunity.OrderType)]
--[IfExists(Column = CROpportunity.OrderNbr)]
--[IfExists(Column = CROpportunity.BAccountID)]
--[IfExists(Column = CROpportunity.ContactID)]
--[IfExists(Column = CROpportunity.ARRefNbr)]
--[IfExists(Column = CROpportunity.BAccountID)]
--[IfExists(Column = CROpportunity.ContactID)]
--[MinVersion(Hash = 586f56dd62f98e9e07defc618c17a8bdacc52b71f175f48c7b459beec2d2cc6b)]
INSERT INTO CRRelation (CompanyID, [Role], IsPrimary, TargetType, TargetNoteID, DocNoteID, EntityID, ContactID, AddToCC, RefNoteID)
SELECT SOOrder.CompanyID, 'SR', 0, 'PX.Objects.CR.CROpportunity', CROpportunity.NoteID, CROpportunity.NoteID, CROpportunity.BAccountID, CROpportunity.ContactID, NULL, SOOrder.NoteID
FROM SOOrder
JOIN CROpportunity ON CROpportunity.CompanyID = SOOrder.CompanyID AND CROpportunity.OrderType = SOOrder.OrderType AND CROpportunity.OrderNbr = SOOrder.OrderNbr

-- Add SOOrder relations to CRCampaign
INSERT INTO CRRelation (CompanyID, [Role], IsPrimary, TargetType, TargetNoteID, DocNoteID, EntityID, ContactID, AddToCC, RefNoteID)
SELECT SOOrder.CompanyID, 'SR', 0, 'PX.Objects.CR.CRCampaign', CRCampaign.NoteID, CRCampaign.NoteID, NULL, NULL, NULL, SOOrder.NoteID
FROM SOOrder
JOIN CRCampaign ON CRCampaign.CompanyID = SOOrder.CompanyID AND CRCampaign.CampaignID = SOOrder.CampaignID
WHERE SOOrder.CampaignID IS NOT NULL

-- Add ARInvoice relations to CRCampaign
INSERT INTO CRRelation (CompanyID, [Role], IsPrimary, TargetType, TargetNoteID, DocNoteID, EntityID, ContactID, AddToCC, RefNoteID)
SELECT ARRegister.CompanyID, 'SR', 0, 'PX.Objects.CR.CROpportunity', CROpportunity.NoteID, CROpportunity.NoteID, CROpportunity.BAccountID, CROpportunity.ContactID, null, ARRegister.NoteID
FROM ARRegister
JOIN CROpportunity ON CROpportunity.CompanyID = ARRegister.CompanyID AND CROpportunity.ARRefNbr = ARRegister.RefNbr
WHERE ARRegister.DocType='INV'

-- Add ARInvoice relations to CRCampaign
INSERT INTO CRRelation (CompanyID, [Role], IsPrimary, TargetType, TargetNoteID, DocNoteID, EntityID, ContactID, AddToCC, RefNoteID)
SELECT ARRegister.CompanyID, 'SR', 0, 'PX.Objects.CR.CRCampaign', CRCampaign.NoteID, CRCampaign.NoteID, NULL, NULL, NULL, ARRegister.NoteID
FROM ARInvoice
JOIN ARRegister ON ARRegister.CompanyID = ARInvoice.CompanyID AND ARRegister.DocType = ARInvoice.DocType AND ARRegister.RefNbr = ARInvoice.RefNbr
JOIN CRCampaign ON CRCampaign.CompanyID = ARInvoice.CompanyID AND CRCampaign.CampaignID = ARInvoice.CampaignID
WHERE ARInvoice.CampaignID IS NOT NULL
GO

--GL SPLIT
--[MinVersion(Hash = 5aad878912620a09805223acbda0c27a8d9c2948b016ce3a625445c7ce6d79ed)]
UPDATE reclTran
SET ReclassOrigTranDate = origTran.TranDate,
    ReclassType = 'C'      
FROM GLTran reclTran
JOIN GLTran origTran
    ON reclTran.CompanyID = origTran.CompanyID
			AND reclTran.OrigModule = origTran.Module
			AND reclTran.OrigBatchNbr = origTran.BatchNbr
			AND reclTran.OrigLineNbr = origTran.LineNbr
JOIN Batch b
	ON reclTran.CompanyID = b.CompanyID
		AND reclTran.Module = b.Module
		AND reclTran.BatchNbr = b.BatchNbr
WHERE b.BatchType = 'RCL'
GO

--[Timeout(Multiplier = 100)]
--[MinVersion(Hash = 8edd00d8da58c40c1a890dcbe90d23c0280bee073f470fabe85d59fb16bcfa94)]
update origTran
set origTran.ReclassTotalCount = 1,
	origTran.ReclassReleasedCount = CASE WHEN reclassTran.Released = 1 THEN 1 ELSE 0 END
from GLTran origTran
	inner join GLTran reclassTran
		on origTran.Module = reclassTran.OrigModule and
			origTran.BatchNbr = reclassTran.OrigBatchNbr and
			origTran.LineNbr = reclassTran.OrigLineNbr and
      origTran.CompanyID = reclassTran.CompanyID
where origTran.ReclassBatchNbr IS NOT NULL
GO

--GL SPLIT END
--[MinVersion(Hash = 4034287911b9752353d27a074b9aeab97d092a15d1ee01bc15006b3b68cecbce)]

GO

--[MinVersion(Hash = d773ac4a24566dc5680e73fca09db08aee84d004f9690149ab1e2c2cbc21b16a)]
update EPExpenseClaimDetails
set EPExpenseClaimDetails.APDocType = i.DocType,
	EPExpenseClaimDetails.APRefNbr = i.RefNbr
from EPExpenseClaimDetails cd
inner join EPExpenseClaim c on	c.RefNbr = cd.RefNbr and 
							c.CompanyID = cd.CompanyID
inner join APRegister r on	r.OrigModule = 'EP' and 
						r.OrigRefNbr = cd.RefNbr and 
						r.TaxCalcMode = cd.TaxCalcMode and
						r.CompanyID = cd.CompanyID
inner join APInvoice i on i.RefNbr = r.RefNbr and 
						i.DocType = r.DocType and 
						((i.TaxZoneID = cd.TaxZoneID) or (i.TaxZoneID is null and cd.TaxZoneID is null)) and
						i.CompanyID = cd.CompanyID
GO

--[MinVersion(Hash = 41edd48dfe097ee38014c4c62d89f2fd58863d5778dc0ba8cdd80c6f0434bbfd)]
update ContractBillingSchedule 
set  InvoiceFormula = '=@ActionInvoice+'' ''+[Contract.ContractCD]+'': ''+[Contract.Description]+''.''',
	 TranFormula = '=IIf( @Prefix=Null, '''', @Prefix+'': '')+ IIf( @ActionItem=Null,'''',@ActionItem+'': '')+[UsageData.Description]'
GO

-- Update RolesInGraph for CR304500 from CR304000
--[MinVersion(Hash = fc9152e824b9dfe92be733e69ee1cdf03f692f330406872e9c3ca8bc6b38f2e6)]
IF NOT EXISTS(Select * FROM RolesInGraph WHERE CompanyID <> 1 AND ScreenID = 'CR304500')
INSERT INTO RolesInGraph (
	CompanyID,
	ScreenID,
	Rolename,
	ApplicationName,
	Accessrights,
	CompanyMask,
	CreatedByID,
	CreatedByScreenID,
	CreatedDateTime,
	LastModifiedByID,
	LastModifiedByScreenID,
	LastModifiedDateTime,
	RecordSourceID)
	SELECT
		CompanyID,
		'CR304500',
		Rolename,
		ApplicationName,
		Accessrights,
		CompanyMask,
		CreatedByID,
		CreatedByScreenID,
		CreatedDateTime,
		LastModifiedByID,
		LastModifiedByScreenID,
		LastModifiedDateTime,
		RecordSourceID
	FROM RolesInGraph
	WHERE CompanyID <> 1 AND ScreenID = 'CR304000'
GO

--Preset Access Rights for screens: EP205010, EP205015, EP205510, EP205515
--[MinVersion(Hash = 7380409924a107561898753752179c140c0658f64efde575d8bf74969521c175)]
IF Not EXISTS(Select * From RolesInGraph where ScreenID = 'EP205010')
insert Into RolesInGraph (CompanyID, ScreenID, Rolename, ApplicationName, Accessrights, CompanyMask, CreatedByID, CreatedByScreenID, CreatedDateTime, LastModifiedByID, LastModifiedByScreenID, LastModifiedDateTime) 
Select CompanyID, 'EP205010', Rolename, ApplicationName, Accessrights, CompanyMask, CreatedByID, CreatedByScreenID, CreatedDateTime, LastModifiedByID, LastModifiedByScreenID, LastModifiedDateTime 
From RolesInGraph where ScreenID = 'EP205000'
GO

--[MinVersion(Hash = d1ffaf63b416dab84992d1778e58db10c1e33f07bc59448baabc6967718f7b57)]
IF Not EXISTS(Select * From RolesInGraph where ScreenID = 'EP205015')
insert Into RolesInGraph (CompanyID, ScreenID, Rolename, ApplicationName, Accessrights, CompanyMask, CreatedByID, CreatedByScreenID, CreatedDateTime, LastModifiedByID, LastModifiedByScreenID, LastModifiedDateTime) 
Select CompanyID, 'EP205015', Rolename, ApplicationName, Accessrights, CompanyMask, CreatedByID, CreatedByScreenID, CreatedDateTime, LastModifiedByID, LastModifiedByScreenID, LastModifiedDateTime 
From RolesInGraph where ScreenID = 'EP205000'
GO

--[MinVersion(Hash = ee58f294277e1302fce79223a6fb8587fd5e84634db5929c581fc183362e3367)]
IF Not EXISTS(Select * From RolesInGraph where ScreenID = 'EP205510')
insert Into RolesInGraph (CompanyID, ScreenID, Rolename, ApplicationName, Accessrights, CompanyMask, CreatedByID, CreatedByScreenID, CreatedDateTime, LastModifiedByID, LastModifiedByScreenID, LastModifiedDateTime) 
Select CompanyID, 'EP205510', Rolename, ApplicationName, Accessrights, CompanyMask, CreatedByID, CreatedByScreenID, CreatedDateTime, LastModifiedByID, LastModifiedByScreenID, LastModifiedDateTime 
From RolesInGraph where ScreenID = 'EP205000'
GO

--[MinVersion(Hash = 8fcedb61c66abc35ccf217648c3d6a8d2a73048a502b3d7b6e34395a29b31107)]
IF Not EXISTS(Select * From RolesInGraph where ScreenID = 'EP205515')
insert Into RolesInGraph (CompanyID, ScreenID, Rolename, ApplicationName, Accessrights, CompanyMask, CreatedByID, CreatedByScreenID, CreatedDateTime, LastModifiedByID, LastModifiedByScreenID, LastModifiedDateTime) 
Select CompanyID, 'EP205515', Rolename, ApplicationName, Accessrights, CompanyMask, CreatedByID, CreatedByScreenID, CreatedDateTime, LastModifiedByID, LastModifiedByScreenID, LastModifiedDateTime 
From RolesInGraph where ScreenID = 'EP205000'
GO

--[mssql: Skip]
--[IfExists(Column = ARTran.SOOrderSortOrder)]
--[IfExists(Column = ARTran.SubItemID)]
--[IfExists(Column = ARTran.LocationID)]
--[IfExists(Column = ARTran.LotSerialNbr)]
--[IfExists(Column = ARTran.ExpireDate)]
--[MinVersion(Hash = 6a87c41f78a1d0d1aaeb020446aa9fcca383789d4bf88027806b6a63943381be)]
UPDATE t
SET [SubItemID] = sol.[SubItemID],
	[SOOrderSortOrder] = sol.[SortOrder],
    [LocationID] = CASE WHEN sot.[Behavior] IN ('IN', 'CM', 'QT') THEN sol.[LocationID] ELSE shl.[LocationID] END,
    [LotSerialNbr] = CASE WHEN sot.[Behavior] IN ('IN', 'CM', 'QT') THEN sol.[LotSerialNbr] ELSE shl.[LotSerialNbr] END,
    [ExpireDate] = CASE WHEN sot.[Behavior] IN ('IN', 'CM', 'QT') THEN sol.[ExpireDate] ELSE shl.[ExpireDate] END
FROM ARTran t
INNER JOIN SOOrderType sot
ON sot.[CompanyID] = t.[CompanyID] AND sot.[OrderType] = t.[SOOrderType]
INNER JOIN SOLine sol
ON sol.[CompanyID] = t.[CompanyID] AND sol.[OrderType] = t.[SOOrderType] AND sol.[OrderNbr] = t.[SOOrderNbr] AND sol.[LineNbr] = t.[SOOrderLineNbr]
LEFT JOIN SOShipLine shl
ON shl.[CompanyID] = t.[CompanyID] AND shl.[ShipmentType] = t.[SOShipmentType] AND shl.[ShipmentNbr] = t.[SOShipmentNbr] AND shl.[LineNbr] = t.[SOShipmentLineNbr]
WHERE t.[LineType] NOT IN ('FR', 'DS', 'MI') AND t.[SOShipmentNbr] IS NOT NULL
GO

--Duplicated script for MS SQL turns off the parallelism for better performance (option - maxdop 1)
--[mysql: Skip]
--[mssql: Native]
--[IfExists(Column = ARTran.SOOrderSortOrder)]
--[IfExists(Column = ARTran.SubItemID)]
--[IfExists(Column = ARTran.LocationID)]
--[IfExists(Column = ARTran.LotSerialNbr)]
--[IfExists(Column = ARTran.ExpireDate)]
--[MinVersion(Hash = bc6fe35746812a7981daa456fb1975ce38b7803fedb4af2bc45a8e7a11147283)]
UPDATE t
SET [SubItemID] = sol.[SubItemID],
	[SOOrderSortOrder] = sol.[SortOrder],
    [LocationID] = CASE WHEN sot.[Behavior] IN ('IN', 'CM', 'QT') THEN sol.[LocationID] ELSE shl.[LocationID] END,
    [LotSerialNbr] = CASE WHEN sot.[Behavior] IN ('IN', 'CM', 'QT') THEN sol.[LotSerialNbr] ELSE shl.[LotSerialNbr] END,
    [ExpireDate] = CASE WHEN sot.[Behavior] IN ('IN', 'CM', 'QT') THEN sol.[ExpireDate] ELSE shl.[ExpireDate] END
FROM ARTran t
INNER JOIN SOOrderType sot
ON sot.[CompanyID] = t.[CompanyID] AND sot.[OrderType] = t.[SOOrderType]
INNER JOIN SOLine sol
ON sol.[CompanyID] = t.[CompanyID] AND sol.[OrderType] = t.[SOOrderType] AND sol.[OrderNbr] = t.[SOOrderNbr] AND sol.[LineNbr] = t.[SOOrderLineNbr]
LEFT JOIN SOShipLine shl
ON shl.[CompanyID] = t.[CompanyID] AND shl.[ShipmentType] = t.[SOShipmentType] AND shl.[ShipmentNbr] = t.[SOShipmentNbr] AND shl.[LineNbr] = t.[SOShipmentLineNbr]
WHERE t.[LineType] NOT IN ('FR', 'DS', 'MI') AND t.[SOShipmentNbr] IS NOT NULL
option (maxdop 1)
GO

--Populate the incomplete link from Return SO Line to Original Invoice (InvoiceType was missing)
--[IfExists(Column = SOLine.InvoiceType)]
--[MinVersion(Hash = 07114d45786d043aced700acc713ef4670ac1a4bf8307eb903a78f87c4e3d678)]
UPDATE rsol
SET InvoiceType = oart.TranType
FROM SOLine rsol
INNER JOIN SOLine osol
ON osol.CompanyID = rsol.CompanyID AND osol.OrderType = rsol.OrigOrderType AND osol.OrderNbr = rsol.OrigOrderNbr AND osol.LineNbr = rsol.OrigLineNbr
INNER JOIN ARTran oart
ON oart.CompanyID = osol.CompanyID AND oart.SOOrderType = osol.OrderType AND oart.SOOrderNbr = osol.OrderNbr AND oart.SOOrderLineNbr = osol.LineNbr
    AND oart.RefNbr = rsol.InvoiceNbr
WHERE rsol.InvoiceType IS NULL AND rsol.InvoiceNbr IS NOT NULL
    AND rsol.Behavior IN ('RM', 'CM')
GO

--Copy link to Original Invoice from Return SO Line to Return Invoice Line
--[IfExists(Column = SOLine.InvoiceType)]
--[IfExists(Column = ARTran.OrigInvoiceType)]
--[IfExists(Column = ARTran.OrigInvoiceNbr)]
--[MinVersion(Hash = 125d98ae8579ed7c2a2e72ee49e974db0ec49910c56d3b7c12a8cf46ac66586e)]
UPDATE rart
SET OrigInvoiceType = rsol.InvoiceType, OrigInvoiceNbr = rsol.InvoiceNbr, OrigInvoiceDate = rsol.InvoiceDate
FROM ARTran rart
INNER JOIN SOLine rsol
ON rsol.CompanyID = rart.CompanyID AND rsol.OrderType = rart.SOOrderType AND rsol.OrderNbr = rart.SOOrderNbr AND rsol.LineNbr = rart.SOOrderLineNbr
WHERE rart.OrigInvoiceNbr IS NULL AND rart.OrigInvoiceDate IS NULL AND rsol.InvoiceNbr IS NOT NULL
    AND rsol.Behavior IN ('RM', 'CM')
GO

--Populate SOOrderShipment.ShippingRefNoteID to be a part of PK
--[OldHash(Hash = c2c140d24d9275bc203c3ee9e76b2dbdc5efdf80d6fa73d4084a2e0552c34b0e)]
--[MinVersion(Hash = b729acd44ecd27f203ab6e9733770cc6ef53df59c9a90d078158a5c5323d4a7b)]
UPDATE os
SET ShippingRefNoteID = COALESCE(s.NoteID, r.NoteID, i.NoteID)
FROM SOOrderShipment os
LEFT JOIN SOShipment s ON os.CompanyID = s.CompanyID AND os.ShipmentType = s.ShipmentType AND os.ShipmentNbr = s.ShipmentNbr
LEFT JOIN POReceipt r ON os.CompanyID = r.CompanyID AND os.ShipmentType = 'H' AND os.ShipmentNbr = r.ReceiptNbr
LEFT JOIN ARRegister i ON os.CompanyID = i.CompanyID AND os.InvoiceType = i.DocType AND os.InvoiceNbr = i.RefNbr
WHERE COALESCE(s.NoteID, r.NoteID, i.NoteID) IS NOT NULL

UPDATE SOOrderShipment SET ShippingRefNoteID = NEWID() WHERE ShippingRefNoteID IS NULL OR ShippingRefNoteID = '00000000-0000-0000-0000-000000000000'
GO

--[MinVersion(Hash = f0458c5c2226fee3be77dfda4017f61685c8e6f5cb50f4112bfc5b1526d296d9)]
UPDATE deletedBranch 
	SET deletedBranch.BAccountID = -deletedBranch.BranchID -- fake value
FROM Branch deletedBranch
INNER JOIN Branch liveBranch ON liveBranch.CompanyID = deletedBranch.CompanyID
	AND liveBranch.BranchID <> deletedBranch.BranchID
	AND liveBranch.BranchCD = deletedBranch.BranchCD
	AND liveBranch.BAccountID = deletedBranch.BAccountID
WHERE deletedBranch.DeletedDatabaseRecord = 1

INSERT INTO Note
(
	CompanyID,
	oldNoteID,
	ExternalKey,
	NoteText,
	GraphType,
	EntityType,
	NoteID
)
(
	SELECT 
		Branch.CompanyID,
		-1,
		BranchID,
		BranchCD,
		'PX.Objects.GL.BranchMaint',
		'PX.Objects.GL.Branch',
		NEWID()
	FROM BAccount
	INNER JOIN 
	(
		SELECT 
			BAccount.CompanyID, 
			BAccount.BAccountID
		FROM BAccount
		INNER JOIN Branch ON Branch.CompanyID = BAccount.CompanyID
			AND Branch.BAccountID = BAccount.BAccountID
		GROUP BY 
			BAccount.CompanyID, 
			BAccount.BAccountID
		HAVING COUNT(*) > 1
	) duplicates ON duplicates.CompanyID = BAccount.CompanyID
		AND duplicates.BAccountID = BAccount.BAccountID
	INNER JOIN Branch ON Branch.CompanyID = BAccount.CompanyID
		AND Branch.BAccountID = BAccount.BAccountID
	WHERE BAccount.AcctCD <> Branch.BranchCD
)

INSERT INTO BAccount
(
	CompanyID,
	AcctCD,
	AcctName,
	AcctReferenceNbr,
	ParentBAccountID,
	ConsolidateToParent,
	Type,
	Status,
	TaxZoneID,
	TaxRegistrationID,
	DefContactID,
	DefAddressID,
	DefLocationID,
	SearchID,
	CampaignSourceID,
	CreatedByID,
	CreatedByScreenID,
	CreatedDateTime,
	LastModifiedByID,
	LastModifiedByScreenID,
	LastModifiedDateTime,
	WorkGroupID,
	OwnerID,
	ClassID,
	ConsolidatingBAccountID,
	DeletedDatabaseRecord,
	BAccount.NoteID
)
(
	SELECT
		BAccount.CompanyID,
		Branch.BranchCD,
		BAccount.AcctName,
		BAccount.AcctReferenceNbr,
		BAccount.ParentBAccountID,
		BAccount.ConsolidateToParent,
		BAccount.Type,
		BAccount.Status,
		BAccount.TaxZoneID,
		BAccount.TaxRegistrationID,
		BAccount.DefContactID,
		BAccount.DefAddressID,
		BAccount.DefLocationID,
		BAccount.SearchID,
		BAccount.CampaignSourceID,
		Note.NoteID,
		'00000000',
		GETDATE(),
		'00000000-0000-0000-0000-000000000000',
		'00000000',
		GETDATE(),
		BAccount.WorkGroupID,
		BAccount.OwnerID,
		BAccount.ClassID,
		BAccount.ConsolidatingBAccountID,
		Branch.DeletedDatabaseRecord,
		NEWID()
	FROM BAccount
	INNER JOIN Branch ON Branch.CompanyID = BAccount.CompanyID
		AND Branch.BAccountID = BAccount.BAccountID
	INNER JOIN Note ON Note.CompanyID = Branch.CompanyID
		AND Note.oldNoteID = -1
		AND Note.ExternalKey = Branch.BranchID
		AND Note.NoteText = Branch.BranchCD
		AND Note.GraphType = 'PX.Objects.GL.BranchMaint'
		AND Note.EntityType = 'PX.Objects.GL.Branch'
)

UPDATE Branch
	SET Branch.BAccountID = BAccount.BAccountID
FROM Branch 
INNER JOIN Note ON Note.CompanyID = Branch.CompanyID
	AND Note.oldNoteID = -1
	AND Note.ExternalKey = Branch.BranchID
	AND Note.NoteText = Branch.BranchCD
	AND Note.GraphType = 'PX.Objects.GL.BranchMaint'
	AND Note.EntityType = 'PX.Objects.GL.Branch'
INNER JOIN BAccount ON BAccount.CompanyID = Note.CompanyID
	AND BAccount.CreatedByID = Note.NoteID

UPDATE BAccount 
	SET BAccount.CreatedByID = '00000000-0000-0000-0000-000000000000'
FROM BAccount
INNER JOIN Note ON Note.CompanyID = BAccount.CompanyID
	AND Note.NoteID = BAccount.CreatedByID

DELETE Note
FROM Note
INNER JOIN Branch ON Branch.CompanyID = Note.CompanyID
	AND Branch.BranchID = Note.ExternalKey
	AND Branch.BranchCD = Note.NoteText
WHERE Note.oldNoteID = -1
	AND Note.GraphType = 'PX.Objects.GL.BranchMaint'
	AND Note.EntityType = 'PX.Objects.GL.Branch'
GO

--[OldHash(Hash = 2ad493f6092af5a7b68f45461a21d235d9a017195760e58af88bfe4eb501cbc4)]
--[OldHash(Hash = b50f1b5dd94689582636afdd25e9a0a0f41f34b588b55a935c32cd2ce5921eab)]
--[OldHash(Hash = dbb379caf70c7c2772963518a31d5c587744df6952102052f88a7f346922e4ff)]
--[OldHash(Hash = aae5109f06616d033e637aa7e38e734c74cea3fa0fcab0726848715d8edf3c82)]
--[OldHash(Hash = 7b3c41ade6179b39e4c02344c26c0c6658e1f26696911679d1a804d2cf658c6b)]
--[MinVersion(Hash = 87ca3c2f49e4dcc9d0a6eb6ce8adaabe0bb78ea4bd6b6a396be6676dd51193c9)]
/*
AC-42755 - Separate Branch and Company.
UPGRADE SCRIPT STRUCTURE:

		[Branch.LedgerID]
		|				|
		IS NULL			IS NOT NULL
		|				|
		|			[Ledger.DefBranchID]
		|			|					|
		|			IS NULL				IS NOT NULL
		|			|					|		
Convert to Organization of	[Ledger.PostInterCompany]
"Without Branches" type		|						|
							false					true
							|						|
				Convert to Organization of		Convert to Organization of 
				"With Branches Not Requiring	"With Branches Requiring 
				Balancing" type					Balancing" type
*/
BEGIN
        -- Check ledger configuration
	UPDATE Branch 
	SET LedgerID = Ledger.LedgerID
	FROM Branch
	     INNER JOIN Ledger ON  Ledger.CompanyID = Branch.CompanyID
			       AND Ledger.DefBranchID = Branch.BranchID

	-- Set the same logo for reports
	--
	UPDATE Branch SET 
		LogoNameReport = LogoName, 
		LogoName = MainLogoName

	INSERT INTO Organization 
	(
		CompanyID,
		OrganizationCD,
		OrganizationType,
		ActualLedgerID,
		Active,
		RoleName,
		CountryID,
		PhoneMask,
		BAccountID,
		LogoName,
		LogoNameReport,
		AllowsRUTROT,
		RUTROTDeductionPct,
		RUTROTPersonalAllowanceLimit,
		RUTDeductionPct,
		RUTPersonalAllowanceLimit,
		RUTExtraAllowanceLimit,
		ROTDeductionPct,
		ROTPersonalAllowanceLimit,
		ROTExtraAllowanceLimit,
		RUTROTCuryID,
		RUTROTClaimNextRefNbr,
		RUTROTOrgNbrValidRegEx,
		TCC,
		ForeignEntity,
		CFSFiler,
		ContactName,
		CTelNumber,
		CEmail,
		NameControl,
		Reporting1099,
		DefaultRUTROTType,
		CreatedByID,
		CreatedByScreenID,
		CreatedDateTime,
		LastModifiedByID,
		LastModifiedByScreenID,
		LastModifiedDateTime,
		tstamp,
		GroupMask, 
		DeletedDatabaseRecord,
    ConvertedFromBranchID,
	NoteID
	)
	SELECT 
		OrganizationsWithoutBranches.CompanyID,
		OrganizationsWithoutBranches.BranchCD,
		'WithoutBranches', -- OrganizationType
		OrganizationsWithoutBranches.LedgerID, --ActualLedgerID
		OrganizationsWithoutBranches.Active,
		OrganizationsWithoutBranches.RoleName,
		OrganizationsWithoutBranches.CountryID,
		OrganizationsWithoutBranches.PhoneMask,
		OrganizationsWithoutBranches.BAccountID,
		OrganizationsWithoutBranches.LogoName,
		OrganizationsWithoutBranches.LogoNameReport,
		OrganizationsWithoutBranches.AllowsRUTROT,
		OrganizationsWithoutBranches.RUTROTDeductionPct,
		OrganizationsWithoutBranches.RUTROTPersonalAllowanceLimit,
		OrganizationsWithoutBranches.RUTDeductionPct,
		OrganizationsWithoutBranches.RUTPersonalAllowanceLimit,
		OrganizationsWithoutBranches.RUTExtraAllowanceLimit,
		OrganizationsWithoutBranches.ROTDeductionPct,
		OrganizationsWithoutBranches.ROTPersonalAllowanceLimit,
		OrganizationsWithoutBranches.ROTExtraAllowanceLimit,
		OrganizationsWithoutBranches.RUTROTCuryID,
		OrganizationsWithoutBranches.RUTROTClaimNextRefNbr,
		OrganizationsWithoutBranches.RUTROTOrgNbrValidRegEx,
		OrganizationsWithoutBranches.TCC,
		OrganizationsWithoutBranches.ForeignEntity,
		OrganizationsWithoutBranches.CFSFiler,
		OrganizationsWithoutBranches.ContactName,
		OrganizationsWithoutBranches.CTelNumber,
		OrganizationsWithoutBranches.CEmail,
		OrganizationsWithoutBranches.NameControl,
		OrganizationsWithoutBranches.Reporting1099,
		OrganizationsWithoutBranches.DefaultRUTROTType,
		BAccount.CreatedByID,
		BAccount.CreatedByScreenID,
		BAccount.CreatedDateTime,
		BAccount.LastModifiedByID,
		BAccount.LastModifiedByScreenID,
		BAccount.LastModifiedDateTime,
		NULL, -- tstamp
		OrganizationsWithoutBranches.GroupMask, 
		OrganizationsWithoutBranches.DeletedDatabaseRecord,
    OrganizationsWithoutBranches.BranchID,
	NEWID()
	FROM (SELECT Branch.* 
		FROM Branch
		LEFT JOIN Ledger ON Ledger.CompanyID = Branch.CompanyID
			AND Ledger.LedgerID = Branch.LedgerID
		WHERE (Branch.LedgerID IS NULL 
			OR Ledger.DefBranchID IS NULL)) OrganizationsWithoutBranches
	INNER JOIN BAccount ON BAccount.CompanyID = OrganizationsWithoutBranches.CompanyID
		AND BAccount.BAccountID = OrganizationsWithoutBranches.BAccountID
		AND BAccount.Type = 'CP'

	-- We should set OrganizationID for all Branches,
	-- that have been converted to Organization Without Branches.
	--
	UPDATE Branch SET 
		Branch.OrganizationID = Organization.OrganizationID
	FROM Branch
	INNER JOIN Organization ON Organization.CompanyID = Branch.CompanyID
		AND Organization.ConvertedFromBranchID = Branch.BranchID
		AND Organization.BAccountID = Branch.BAccountID

	-- BAccount records related to Organizations Without Branches
	-- should have OrganizationBranchCombinedType type
	--
	UPDATE BAccount SET
		BAccount.Type = 'OB'
	FROM BAccount
	INNER JOIN (SELECT Branch.* 
		FROM Branch
		LEFT JOIN Ledger ON Ledger.CompanyID = Branch.CompanyID
			AND Ledger.LedgerID = Branch.LedgerID
		WHERE (Branch.LedgerID IS NULL 
			OR Ledger.DefBranchID IS NULL)) OrganizationsWithoutBranches ON OrganizationsWithoutBranches.CompanyID = BAccount.CompanyID
		AND OrganizationsWithoutBranches.BAccountID = BAccount.BAccountID
		AND BAccount.Type = 'CP'

	INSERT INTO Organization 
	(
		CompanyID,
		OrganizationCD,
		OrganizationType,
		ActualLedgerID,
		Active,
		RoleName,
		CountryID,
		PhoneMask,
		BAccountID,
		LogoName,
		LogoNameReport,
		AllowsRUTROT,
		RUTROTDeductionPct,
		RUTROTPersonalAllowanceLimit,
		RUTDeductionPct,
		RUTPersonalAllowanceLimit,
		RUTExtraAllowanceLimit,
		ROTDeductionPct,
		ROTPersonalAllowanceLimit,
		ROTExtraAllowanceLimit,
		RUTROTCuryID,
		RUTROTClaimNextRefNbr,
		RUTROTOrgNbrValidRegEx,
		TCC,
		ForeignEntity,
		CFSFiler,
		ContactName,
		CTelNumber,
		CEmail,
		NameControl,
		Reporting1099,
		DefaultRUTROTType,
		CreatedByID,
		CreatedByScreenID,
		CreatedDateTime,
		LastModifiedByID,
		LastModifiedByScreenID,
		LastModifiedDateTime,
		tstamp,
		GroupMask, 
		DeletedDatabaseRecord,
    ConvertedFromBranchID,
	NoteID
	)
	SELECT 
		OrganizationsWithBranches.CompanyID,
		OrganizationsWithBranches.BranchCD,
		CASE WHEN Ledger.PostInterCompany = 1 
			THEN 'Balancing'
			ELSE 'NotBalancing' 
		END, -- OrganizationType
		OrganizationsWithBranches.LedgerID, --ActualLedgerID
		OrganizationsWithBranches.Active,
		OrganizationsWithBranches.RoleName,
		OrganizationsWithBranches.CountryID,
		OrganizationsWithBranches.PhoneMask,
		OrganizationsWithBranches.BAccountID,
		OrganizationsWithBranches.LogoName,
		OrganizationsWithBranches.LogoNameReport,
		OrganizationsWithBranches.AllowsRUTROT,
		OrganizationsWithBranches.RUTROTDeductionPct,
		OrganizationsWithBranches.RUTROTPersonalAllowanceLimit,
		OrganizationsWithBranches.RUTDeductionPct,
		OrganizationsWithBranches.RUTPersonalAllowanceLimit,
		OrganizationsWithBranches.RUTExtraAllowanceLimit,
		OrganizationsWithBranches.ROTDeductionPct,
		OrganizationsWithBranches.ROTPersonalAllowanceLimit,
		OrganizationsWithBranches.ROTExtraAllowanceLimit,
		OrganizationsWithBranches.RUTROTCuryID,
		OrganizationsWithBranches.RUTROTClaimNextRefNbr,
		OrganizationsWithBranches.RUTROTOrgNbrValidRegEx,
		OrganizationsWithBranches.TCC,
		OrganizationsWithBranches.ForeignEntity,
		OrganizationsWithBranches.CFSFiler,
		OrganizationsWithBranches.ContactName,
		OrganizationsWithBranches.CTelNumber,
		OrganizationsWithBranches.CEmail,
		OrganizationsWithBranches.NameControl,
		OrganizationsWithBranches.Reporting1099,
		OrganizationsWithBranches.DefaultRUTROTType,
		BAccount.CreatedByID,
		BAccount.CreatedByScreenID,
		BAccount.CreatedDateTime,
		BAccount.LastModifiedByID,
		BAccount.LastModifiedByScreenID,
		BAccount.LastModifiedDateTime,
		NULL, -- tstamp
		OrganizationsWithBranches.GroupMask, 
		OrganizationsWithBranches.DeletedDatabaseRecord,
    OrganizationsWithBranches.BranchID,
	NEWID()
	FROM 	(SELECT DISTINCT Branch.*
		FROM Branch
		INNER JOIN Ledger ON Ledger.CompanyID = Branch.CompanyID
			AND Ledger.DefBranchID = Branch.BranchID) OrganizationsWithBranches
	INNER JOIN BAccount ON BAccount.CompanyID = OrganizationsWithBranches.CompanyID
		AND BAccount.BAccountID = OrganizationsWithBranches.BAccountID AND BAccount.Type = 'CP'
	INNER JOIN Ledger ON Ledger.CompanyID = OrganizationsWithBranches.CompanyID
		AND Ledger.LedgerID = OrganizationsWithBranches.LedgerID

	-- We should set OrganizationID for all Branches,
	-- that have been converted to Organization With Branches.
	--
	UPDATE Branch SET
		Branch.OrganizationID = Organization.OrganizationID
	FROM Branch
	INNER JOIN Ledger ON Ledger.CompanyID = Branch.CompanyID
		AND Ledger.LedgerID = Branch.LedgerID
	INNER JOIN (SELECT DISTINCT Branch.*
		FROM Branch
		INNER JOIN Ledger ON Ledger.CompanyID = Branch.CompanyID
			AND Ledger.DefBranchID = Branch.BranchID) OrganizationsWithBranches ON OrganizationsWithBranches.CompanyID = Ledger.CompanyID
		AND OrganizationsWithBranches.BranchID = Ledger.DefBranchID
	INNER JOIN Organization ON Organization.CompanyID = OrganizationsWithBranches.CompanyID
		AND Organization.ConvertedFromBranchID = OrganizationsWithBranches.BranchID
		AND Organization.BAccountID = OrganizationsWithBranches.BAccountID

	-- BAccount records related to Organizations With Branches
	-- should have OrganizationType type
	--
	UPDATE BAccount SET
	BAccount.Type = 'OR'
	FROM BAccount
	INNER JOIN (SELECT DISTINCT Branch.*
		FROM Branch
		INNER JOIN Ledger ON Ledger.CompanyID = Branch.CompanyID
			AND Ledger.DefBranchID = Branch.BranchID) OrganizationsWithBranches ON OrganizationsWithBranches.CompanyID = BAccount.CompanyID
		AND OrganizationsWithBranches.BAccountID = BAccount.BAccountID
		AND BAccount.Type = 'CP'

	-- We should add links between Organizations
	-- and their actual Ledgers. Note we don't need
	-- to consider any other cases because only
	-- one Ledger for one Consol. Branch is allowed in the 
	-- previos version.
	--
	INSERT INTO OrganizationLedgerLink
	(
		CompanyID,
		OrganizationID,
		LedgerID
	)
	SELECT 
		CompanyID,
		OrganizationID,
		ActualLedgerID
	FROM Organization
	WHERE ActualLedgerID IS NOT NULL
		AND DeletedDatabaseRecord = 0

END
GO

--[MinVersion(Hash = 77aaccdea7fb0c039387b15bf13d38e8aba7238908c971bc4b5caa97f89728ca)]
INSERT INTO OrganizationLedgerLink (CompanyID, OrganizationID, LedgerID)
SELECT DISTINCT b.CompanyID, b.OrganizationID, gl.LedgerID
FROM GLHistory gl 
INNER JOIN Branch b ON b.CompanyID = gl.CompanyID 
	AND b.BranchID = gl.BranchID 
	AND b.DeletedDatabaseRecord = 0
INNER JOIN Ledger l ON l.CompanyID = gl.CompanyID 
	AND l.LedgerID = gl.LedgerID 
	AND l.BalanceType <> 'A'
	AND l.DeletedDatabaseRecord = 0
LEFT JOIN OrganizationLedgerLink oll ON oll.CompanyID = gl.CompanyID 
	AND oll.OrganizationID = b.OrganizationID 
	AND oll.LedgerID = l.LedgerID
WHERE oll.CompanyID IS NULL
ORDER BY b.OrganizationID, gl.LedgerID
GO

--[MinVersion(Hash = 8a6fc5430a165664df51299d982d0940fadaffd38579a2cc9e90a8768a16eb73)]
UPDATE o
SET o.OrganizationName = ba.AcctName
FROM Organization o
	JOIN BAccount ba
		ON o.CompanyID = ba.CompanyID
			AND o.BAccountID = ba.BAccountID
GO

--[MinVersion(Hash = bd4e5b32f741802be2a83c9f0e1cba9415ee610d4a366c8a70580d3b971ceb5d)]
UPDATE TaxPeriod
SET OrganizationID = -BranchID
WHERE OrganizationID IS NULL;

UPDATE TaxYear
SET OrganizationID = -BranchID
WHERE OrganizationID IS NULL;

UPDATE AP1099Year
SET OrganizationID = -BranchID
WHERE OrganizationID IS NULL;
GO

--[MinVersion(Hash = 14630b23ef9fd362d062a66b8c5fd1d71a7cc4ccd75907758dd9eda9397b4abd)]
UPDATE org
SET org.FileTaxesByBranches = l.PostInterCompany
FROM Organization org
	JOIN Ledger l
			ON org.CompanyID = l.CompanyID
				AND org.ActualLedgerID = l.LedgerID
WHERE org.OrganizationType <> 'WithoutBranches'
GO

--[MinVersion(Hash = 1d3e3657b1c85a2fd0a69aa957bb9b01a1f3b905691db7bc8edbdf5d4e8b64bc)]
UPDATE b
SET b.OrganizationLogoNameReport = o.LogoNameReport
FROM Branch b
	INNER JOIN Organization o
		ON o.OrganizationID = b.OrganizationID
			AND o.CompanyID = b.CompanyID
GO

--[MinVersion(Hash = 2517bd24de92a56ea81bcfc9fc16ae10981b9cdb2606804cbb8297614ae46a4f)]
UPDATE FADetails SET[Status]='H' WHERE [Status]='C';
GO

--[MinVersion(Hash = 6642fc0f47fee2cd64495af3b22914f0071689812be545730be582ea85d299a4)]
update o
set NoteID = b.NoteID
from Organization o
inner join BAccount b
	on o.CompanyID = b.CompanyID
		and o.OrganizationCD = b.AcctCD
where o.NoteID is null;
GO

--[IfExists(Column = FSSODet.ServiceID)]
--[MinVersion(Hash = f1c9036580e60c9a6c4727da97ec5052d65da0fdf66354dd15ba81c861605171)]
UPDATE FSSODet SET
  InventoryID = ServiceID
WHERE
  LineType in ('SERVI', 'NSTKI')
  AND ServiceID IS NOT NULL
GO

--[IfExists(Column = FSAppointmentDet.ServiceID)]
--[MinVersion(Hash = 1fc53c21a56427244d38a3bc3cf1c0d47c46c40364d307cc9ba24322a43a93af)]
UPDATE FSAppointmentDet SET
  InventoryID = ServiceID
WHERE
  LineType in ('SERVI', 'NSTKI')
  AND ServiceID IS NOT NULL
GO

--[IfExists(Column = FSSODet.SrvOrdType)]
--[IfExists(Column = FSSODet.RefNbr)]
--[MinVersion(Hash = a1363ce5230129f64468dac5178ba2b9e178c1a06b8abb31946bcee0510aae60)]
UPDATE FSSODet
SET
  RefNbr = FSServiceOrder.RefNbr,
  SrvOrdType = FSServiceOrder.SrvOrdType
FROM FSSODet
INNER JOIN FSServiceOrder ON FSServiceOrder.CompanyID = FSSODet.CompanyID AND FSServiceOrder.SOID = FSSODet.SOID
GO

--[IfExists(Column = FSSOAttendee.SrvOrdType)]
--[IfExists(Column = FSSOAttendee.RefNbr)]
--[MinVersion(Hash = 6a57528b7e702eb3af0cd932d0fb7d2a6a1745737a65471faa30f784ae633ac9)]
UPDATE FSSOAttendee
SET
  RefNbr = FSServiceOrder.RefNbr,
  SrvOrdType = FSServiceOrder.SrvOrdType
FROM FSSOAttendee
INNER JOIN FSServiceOrder ON FSServiceOrder.CompanyID = FSSOAttendee.CompanyID AND FSServiceOrder.SOID = FSSOAttendee.SOID
GO

--[IfExists(Column = FSSOEmployee.SrvOrdType)]
--[IfExists(Column = FSSOEmployee.RefNbr)]
--[MinVersion(Hash = baec8fe7ec8753495bb605840bb37e679bd774f60a0deebd32df9a986a4e9178)]
UPDATE FSSOEmployee
SET
  RefNbr = FSServiceOrder.RefNbr,
  SrvOrdType = FSServiceOrder.SrvOrdType
FROM FSSOEmployee
INNER JOIN FSServiceOrder ON FSServiceOrder.CompanyID = FSSOEmployee.CompanyID AND FSServiceOrder.SOID = FSSOEmployee.SOID
GO

--[IfExists(Column = FSSOResource.SrvOrdType)]
--[IfExists(Column = FSSOResource.RefNbr)]
--[MinVersion(Hash = 38da45380e70a06b96e5d9e04a2c32a741fecc09e49040ade08a8ea7898a958a)]
UPDATE FSSOResource
SET
  RefNbr = FSServiceOrder.RefNbr,
  SrvOrdType = FSServiceOrder.SrvOrdType
FROM FSSOResource
INNER JOIN FSServiceOrder ON FSServiceOrder.CompanyID = FSSOResource.CompanyID AND FSServiceOrder.SOID = FSSOResource.SOID
GO

--[IfExists(Column = FSAppointmentDet.SrvOrdType)]
--[IfExists(Column = FSAppointmentDet.RefNbr)]
--[MinVersion(Hash = e439d661e597bf3db0460d208554878852035b631aaf13a052624c3607228d2d)]
UPDATE FSAppointmentDet
SET
  RefNbr = FSAppointment.RefNbr,
  SrvOrdType = FSAppointment.SrvOrdType
FROM FSAppointmentDet
INNER JOIN FSAppointment ON FSAppointment.CompanyID = FSAppointmentDet.CompanyID AND FSAppointment.AppointmentID = FSAppointmentDet.AppointmentID
GO

--[IfExists(Column = FSAppointmentEmployee.SrvOrdType)]
--[IfExists(Column = FSAppointmentEmployee.RefNbr)]
--[MinVersion(Hash = 96a9e8ab2f750971db03a47136a63ee853fac79ab0d3a7c0c34de56e37917891)]
UPDATE FSAppointmentEmployee
SET
  RefNbr = FSAppointment.RefNbr,
  SrvOrdType = FSAppointment.SrvOrdType
FROM FSAppointmentEmployee
INNER JOIN FSAppointment ON FSAppointment.CompanyID = FSAppointmentEmployee.CompanyID AND FSAppointment.AppointmentID = FSAppointmentEmployee.AppointmentID
GO

--[IfExists(Column = FSAppointmentAttendee.SrvOrdType)]
--[IfExists(Column = FSAppointmentAttendee.RefNbr)]
--[MinVersion(Hash = 8e4280bfda13f9c792f0d84937bf23a0b4ccd4dfcbe5f508ede56416a4758c40)]
UPDATE FSAppointmentAttendee
SET
  RefNbr = FSAppointment.RefNbr,
  SrvOrdType = FSAppointment.SrvOrdType
FROM FSAppointmentAttendee
INNER JOIN FSAppointment ON FSAppointment.CompanyID = FSAppointmentAttendee.CompanyID AND FSAppointment.AppointmentID = FSAppointmentAttendee.AppointmentID
GO

--[IfExists(Column = FSAppointmentResource.SrvOrdType)]
--[IfExists(Column = FSAppointmentResource.RefNbr)]
--[MinVersion(Hash = 30657e08db2e75883fe12418f6ff9fc8d12b381848e8f95497d0af6c71e7deda)]
UPDATE FSAppointmentResource
SET
  RefNbr = FSAppointment.RefNbr,
  SrvOrdType = FSAppointment.SrvOrdType
FROM FSAppointmentResource
INNER JOIN FSAppointment ON FSAppointment.CompanyID = FSAppointmentResource.CompanyID AND FSAppointment.AppointmentID = FSAppointmentResource.AppointmentID
GO

--[IfExists(Column = FSAppointmentDet.OldAppointmentInventoryID)]
--[IfExists(Table = FSAppointmentInventoryItem)]
--[MinVersion(Hash = dc572043cfed5c915085eae0df7ed6585e474aad773b3827c2287e965b6214b6)]
INSERT INTO [FSAppointmentDet]
  ([CompanyID]
  ,[SrvOrdType]
  ,[RefNbr]
  ,[AcctID]
  ,[ActualDateTimeBegin]
  ,[ActualDateTimeBeginUTC]
  ,[ActualDateTimeEnd]
  ,[ActualDateTimeEndUTC]
  ,[ActualDuration]
  ,[AppointmentID]
  ,[BillableQty]
  ,[BillableTime]
  ,[BranchID]
  ,[ComponentID]
  ,[EquipmentAction]
  ,[EquipmentLineRef]
  ,[EstimatedDuration]
  ,[ExpenseEmployeeID]
  ,[InventoryID]
  ,[IsBillable]
  ,[IsPrepaid]
  ,[KeepActualDateTimes]
  ,[LineRef]
  ,[LineType]
  ,[ManualPrice]
  ,[NewTargetEquipmentLineNbr]
  ,[OldAppointmentInventoryID]
  ,[PostID]
  ,[ProjectID]
  ,[ProjectTaskID]
  ,[Qty]
  ,[ScheduleDetID]
  ,[ScheduleID]
  ,[PickupDeliveryServiceID]
  ,[ServiceType]
  ,[SiteID]
  ,[SiteLocationID]
  ,[SMEquipmentID]
  ,[SODetID]
  ,[SourceSalesOrderRefNbr]
  ,[SourceSalesOrderType]
  ,[StaffActualDuration]
  ,[StaffID]
  ,[Status]
  ,[SubID]
  ,[SubItemID]
  ,[SuspendedTargetEquipmentID]
  ,[TranAmt]
  ,[TranDate]
  ,[TranDesc]
  ,[UnitPrice]
  ,[UOM]
  ,[Warranty]
  ,[CreatedByID]
  ,[CreatedByScreenID]
  ,[CreatedDateTime]
  ,[LastModifiedByID]
  ,[LastModifiedByScreenID]
  ,[LastModifiedDateTime]
  ,[NoteID])
SELECT
  FSIT.CompanyID
  ,FSAPP.SrvOrdType
  ,FSAPP.RefNbr
  ,NULL
  ,NULL
  ,NULL
  ,NULL
  ,NULL
  ,0
  ,FSIT.AppointmentID
  ,FSIT.Qty
  ,0
  ,FSIT.BranchID
  ,NULL
  ,'NO'
  ,NULL
  ,0
  ,NULL
  ,FSIT.InventoryID
  ,1
  ,0
  ,0
  ,FSSODet.LineRef
  ,'PU_DL'
  ,FSIT.ManualPrice
  ,NULL
  ,FSIT.AppointmentInventoryItemID
  ,FSIT.PostID
  ,FSIT.ProjectID
  ,FSIT.ProjectTaskID
  ,FSIT.Qty
  ,NULL
  ,NULL
  ,FSIT.ServiceID
  ,NULL
  ,FSIT.SiteID
  ,NULL
  ,NULL
  ,FSIT.SODetID
  ,NULL
  ,NULL
  ,0
  ,NULL
  ,'O'
  ,NULL
  ,FSIT.SubItemID
  ,NULL
  ,FSIT.TranAmt
  ,FSAPP.ExecutionDate
  ,FSIT.TranDesc
  ,FSIT.UnitPrice
  ,FSIT.UOM
  ,0
  ,FSIT.CreatedByID
  ,FSIT.AppointmentInventoryItemID
  ,FSIT.CreatedDateTime
  ,FSIT.LastModifiedByID
  ,FSIT.LastModifiedByScreenID
  ,FSIT.LastModifiedDateTime
  ,FSIT.NoteID
FROM FSAppointmentInventoryItem FSIT
INNER JOIN FSAppointment FSAPP ON (
      FSAPP.CompanyID = FSIT.CompanyID
      AND FSAPP.AppointmentID = FSIT.AppointmentID)
INNER JOIN FSSODet ON (
      FSSODet.CompanyID = FSIT.CompanyID
      AND FSSODet.SODetID = FSIT.SODetID)
;

UPDATE PickupDeliveryLine SET
  ServiceType = ServiceLine.ServiceType
FROM FSAppointmentDet ServiceLine
  INNER JOIN FSAppointmentDet PickupDeliveryLine ON (
    PickupDeliveryLine.CompanyID = ServiceLine.CompanyID
    AND PickupDeliveryLine.AppointmentID = ServiceLine.AppointmentID
    AND PickupDeliveryLine.LineType = 'PU_DL'
    AND ServiceLine.LineType = 'SERVI'
    AND PickupDeliveryLine.PickupDeliveryServiceID = ServiceLine.InventoryID
    AND PickupDeliveryLine.SODetID = ServiceLine.SODetID
  )
WHERE
  PickupDeliveryLine.ServiceType IS NULL
;
GO

--[IfExists(Column = FSAppointmentDet.OldAppointmentInventoryID)]
--[MinVersion(Hash = 41c389242863a67e7538d4504045a0ba8d65ce8cb52cd0bd1497b31485402918)]
UPDATE FSxSOLine
SET AppDetID = FSAppointmentDet.AppDetID
FROM FSxSOLine
INNER JOIN FSAppointmentDet ON FSAppointmentDet.CompanyID = FSxSOLine.CompanyID AND FSAppointmentDet.OldAppointmentInventoryID = FSxSOLine.AppDetID;

UPDATE FSxARTran
SET AppDetID = FSAppointmentDet.AppDetID
FROM FSxARTran
INNER JOIN FSAppointmentDet ON FSAppointmentDet.CompanyID = FSxARTran.CompanyID AND FSAppointmentDet.OldAppointmentInventoryID = FSxARTran.AppDetID;

UPDATE FSxAPTran
SET AppDetID = FSAppointmentDet.AppDetID
FROM FSxAPTran
INNER JOIN FSAppointmentDet ON FSAppointmentDet.CompanyID = FSxAPTran.CompanyID AND FSAppointmentDet.OldAppointmentInventoryID = FSxAPTran.AppDetID;

UPDATE FSxINTran
SET AppDetID = FSAppointmentDet.AppDetID
FROM FSxINTran
INNER JOIN FSAppointmentDet ON FSAppointmentDet.CompanyID = FSxINTran.CompanyID AND FSAppointmentDet.OldAppointmentInventoryID = FSxINTran.AppDetID;
GO

--[IfExists(Column = FSServiceContract.EnableExpirationDate)]
--[MinVersion(Hash = c7857434ce47cc22a85cedbdccccd6e11b87f02201fece489bb450e5290f7f2d)]
UPDATE FSServiceContract SET ExpirationType = 'E' WHERE EnableExpirationDate = 1

UPDATE FSServiceContract SET ExpirationType = 'U' WHERE EnableExpirationDate = 0
GO

--[MinVersion(Hash = 2899bbe264c6d6c791e4c81a21fc23b21fabb34dde0c183be870a0187fef8431)]
UPDATE FSServiceContract SET StatusEffectiveFromDate = StartDate WHERE Status = 'A'

UPDATE FSServiceContract SET BillingType = 'APFB' WHERE BillingType IS NULL OR BillingType = ''

UPDATE FSServiceContract SET BillTo = 'C' WHERE BillTo IS NULL OR BillTo = ''

UPDATE FSServiceContract SET BillCustomerID = CustomerID WHERE BillCustomerID IS NULL

UPDATE FSServiceContract SET BillLocationID = CustomerLocationID WHERE BillLocationID IS NULL

UPDATE FSServiceContract SET ActivationDate = StartDate WHERE ActivationDate IS NULL

UPDATE FSServiceContract SET BillingPeriod = 'M' WHERE BillingPeriod IS NULL OR BillingPeriod = ''

UPDATE FSServiceContract SET ExpirationType = 'U' WHERE EndDate IS NULL AND ExpirationType = ''
GO

--[MinVersion(Hash = d0a729fd6db2e296b4db16fada3734c51d2ff464bc6d99b7a3c401eb72b857b7)]
UPDATE FSServiceContract 
SET Status = CASE WHEN FSServiceContract.Status = 'H' THEN 'S'
			 	  WHEN FSServiceContract.Status = 'I' THEN 'C'
			 	  ELSE FSServiceContract.Status
			 END;
GO

--[MinVersion(Hash = dca474d9e03805bb86d31ee56bc50d38554d48cb58c6abfd9d02f1086a1c9375)]
UPDATE FSSetup SET ContractPostTo = 'AR' WHERE ContractPostTo IS NULL OR ContractPostTo = '';

UPDATE FSSetup SET ContractSalesAcctSource = 'CL' WHERE ContractSalesAcctSource IS NULL OR ContractSalesAcctSource = '';
 
UPDATE FSSetup SET ContractCombineSubFrom = 'LLLLLL' WHERE ContractCombineSubFrom IS NULL OR ContractCombineSubFrom = '';
GO

--[IfExists(Column = FSSODet.RemovedApptTranAmt)]
--[MinVersion(Hash = 544bf61728629983c0d9035a0fbd950d76b346122620687f80729e71ffd442f6)]
UPDATE FSSODet
SET FSSODet.ApptTranAmt = FSSODet.RemovedApptTranAmt;
GO

--[IfExists(Column = FSSODet.RemovedEstimatedTranAmt)]
--[MinVersion(Hash = 1731bb4d7be292a830d1661823cb6a09dc42bdb4b3edaf345cbafaf4c99aeaad)]
UPDATE FSSODet
SET FSSODet.EstimatedTranAmt = FSSODet.RemovedEstimatedTranAmt;
GO

--[IfExists(Column = FSAppointment.RemovedEstimatedLineTotal)]
--[MinVersion(Hash = b052b40655d8ccaad775e1211a22db22b8e775fa198a231a2efe5f63388eda85)]
UPDATE FSAppointment
SET FSAppointment.EstimatedLineTotal = FSAppointment.RemovedEstimatedLineTotal;
GO

--[IfExists(Column = FSAppointmentDet.RemovedEstimatedTranAmt)]
--[MinVersion(Hash = 9151bd8adb4dc69cc39fb9fdfc83ce7d18409e1e0a5c1ff4f50a33b69af4a63e)]
UPDATE FSAppointmentDet
SET FSAppointmentDet.EstimatedTranAmt = FSAppointmentDet.RemovedEstimatedTranAmt;
GO

--[IfExists(Column = FSAppointmentDet.RemovedTranAmt)]
--[MinVersion(Hash = 1bccc080c4a097a64cef5e4df5853acf941c32419ee666be0e36ae1788a870a5)]
UPDATE FSAppointmentDet
SET FSAppointmentDet.TranAmt = FSAppointmentDet.RemovedTranAmt;
GO

--[IfExists(Column = FSSODet.BillableTranAmt)]
--[IfExistsSelect(From = FSSODet, WhereIsNull = BillableTranAmt)]
--[MinVersion(Hash = 3285aa43930084e01f2e81b311893a359fee8d79e6530311472358525c9941bb)]
UPDATE FSSODet
SET
  FSSODet.BillableTranAmt = CASE WHEN FSSODet.isBillable = 1 THEN FSSODet.EstimatedTranAmt ELSE 0 END
GO

--[IfExists(Column = FSAppointmentDet.BillableTranAmt)]
--[IfExistsSelect(From = FSAppointmentDet, WhereIsNull = BillableTranAmt)]
--[MinVersion(Hash = a8577adfde58880e0c17fc63ce0b69a12afefb4ea1ea4cc7d468200f040f4aae)]
UPDATE FSAppointmentDet
SET
  FSAppointmentDet.BillableTranAmt = CASE WHEN FSAppointmentDet.isBillable = 1 THEN FSAppointmentDet.TranAmt ELSE 0 END
GO

--[IfExists(Column = FSServiceOrder.BillableOrderTotal)]
--[IfExistsSelect(From = FSServiceOrder, WhereIsNull = BillableOrderTotal)]
--[MinVersion(Hash = cf364ddc888830bc3b229109dcc6f4ec6841b32f578c058e9cadda279b894a14)]
UPDATE FSServiceOrder
  SET BillableOrderTotal = (SELECT SUM(FSSODet.BillableTranAmt)
                            FROM  FSSODet
                            WHERE FSSODet.CompanyID = FSServiceOrder.CompanyID
                                AND FSSODet.SrvOrdType = FSServiceOrder.SrvOrdType
                                AND FSSODet.RefNbr = FSServiceOrder.RefNbr
                                AND FSSODet.IsBillable = 1)
FROM FSServiceOrder
WHERE BillableOrderTotal IS NULL;

--[IfExists(Column = FSServiceOrder.BillableOrderTotal)]
--[IfExistsSelect(From = FSServiceOrder, WhereIsNull = BillableOrderTotal)]
UPDATE FSServiceOrder
  SET BillableOrderTotal = 0.0
FROM FSServiceOrder
WHERE BillableOrderTotal IS NULL;

--[IfExists(Column = FSAppointment.BillableLineTotal)]
--[IfExistsSelect(From = FSAppointment, WhereIsNull = BillableLineTotal)]
UPDATE FSAppointment
  SET BillableLineTotal = (SELECT SUM(FSAppointmentDet.BillableTranAmt)
                            FROM  FSAppointmentDet
                            WHERE FSAppointmentDet.CompanyID = FSAppointment.CompanyID
                                AND FSAppointmentDet.SrvOrdType = FSAppointment.SrvOrdType
                                AND FSAppointmentDet.RefNbr = FSAppointment.RefNbr
                                AND FSAppointmentDet.IsBillable = 1)
FROM FSAppointment
WHERE BillableLineTotal IS NULL;

--[IfExists(Column = FSAppointment.BillableLineTotal)]
--[IfExistsSelect(From = FSAppointment, WhereIsNull = BillableLineTotal)]
UPDATE FSAppointment
  SET BillableLineTotal = 0.0
FROM FSAppointment
WHERE BillableLineTotal IS NULL;

--[IfExists(Column = FSServiceOrder.CuryInfoID)]
--[IfExistsSelect(From = FSServiceOrder, WhereIsNull = CuryInfoID)]
--[SmartExecute]
INSERT INTO CurrencyInfo
           (CompanyID
           ,CuryID
           ,CuryRateTypeID
           ,CuryEffDate
           ,CuryMultDiv
           ,CuryRate
           ,BaseCuryID
           ,RecipRate
           ,BaseCalc)
     SELECT
            CompanyID
           ,BaseCuryID
           ,NULL
           ,'2010-01-01'
           ,'M'
           ,1
           ,BaseCuryID
           ,1
           ,1
     FROM Company
	 WHERE
	 CompanyID > 1

UPDATE FSServiceOrder SET
	CuryID = (SELECT Company.BaseCuryID FROM Company WHERE Company.CompanyID = FSServiceOrder.CompanyID),
	CuryInfoID = (SELECT MAX(CuryInfoID) FROM CurrencyInfo WHERE CurrencyInfo.CompanyID = FSServiceOrder.CompanyID),
	CuryEstimatedOrderTotal = EstimatedOrderTotal,
	CuryApptOrderTotal = ApptOrderTotal,
  CuryBillableOrderTotal = BillableOrderTotal
FROM FSServiceOrder
WHERE
	FSServiceOrder.CuryInfoID IS NULL

UPDATE FSSODet SET
	CuryInfoID = (SELECT MAX(CuryInfoID) FROM CurrencyInfo WHERE CurrencyInfo.CompanyID = FSSODet.CompanyID),
	CuryUnitPrice = UnitPrice,
	CuryEstimatedTranAmt = EstimatedTranAmt,
	CuryApptTranAmt = ApptTranAmt,
  CuryBillableTranAmt = BillableTranAmt
FROM FSSODet
WHERE
	FSSODet.CuryInfoID IS NULL

UPDATE FSAppointment SET
	CuryID = (SELECT Company.BaseCuryID FROM Company WHERE Company.CompanyID = FSAppointment.CompanyID),
	CuryInfoID = (SELECT MAX(CuryInfoID) FROM CurrencyInfo WHERE CurrencyInfo.CompanyID = FSAppointment.CompanyID),
	CuryEstimatedLineTotal = EstimatedLineTotal,
	CuryLineTotal = LineTotal,
  CuryBillableLineTotal = BillableLineTotal
FROM FSAppointment
WHERE
	FSAppointment.CuryInfoID IS NULL

UPDATE FSAppointmentDet SET
	CuryInfoID = (SELECT MAX(CuryInfoID) FROM CurrencyInfo WHERE CurrencyInfo.CompanyID = FSAppointmentDet.CompanyID),
	CuryUnitPrice = UnitPrice,
	CuryEstimatedTranAmt = EstimatedTranAmt,
	CuryTranAmt = TranAmt,
  CuryBillableTranAmt = BillableTranAmt
FROM FSAppointmentDet
WHERE
	FSAppointmentDet.CuryInfoID IS NULL
GO

--[IfExists(Column = FSServiceOrder.CuryInfoID)]
--[IfExists(Column = FSServiceOrder.CuryBillableOrderTotal)]
--[IfExistsSelect(From = FSServiceOrder, WhereIsNull = CuryBillableOrderTotal)]
--[MinVersion(Hash = 8222f0fa0c3eb0100a219154436863e51261eb3851833efd1646457ece156f1b)]
UPDATE FSServiceOrder SET
  	CuryBillableOrderTotal = (SELECT SUM(FSSODet.CuryBillableTranAmt)
	                          FROM  FSSODet
	                          WHERE FSSODet.CompanyID = FSServiceOrder.CompanyID
	                          AND FSSODet.RefNbr = FSServiceOrder.RefNbr
	                          AND FSSODet.SrvOrdType = FSServiceOrder.SrvOrdType
	                          AND FSSODet.IsBillable = 1)
FROM FSServiceOrder
WHERE
	FSServiceOrder.CuryInfoID IS NOT NULL
	AND FSServiceOrder.CuryBillableOrderTotal IS NULL;

--[IfExists(Column = FSServiceOrder.CuryInfoID)]
--[IfExists(Column = FSServiceOrder.CuryBillableOrderTotal)]
--[IfExistsSelect(From = FSServiceOrder, WhereIsNull = CuryBillableOrderTotal)]
UPDATE FSServiceOrder SET
  	CuryBillableOrderTotal = 0.0
FROM FSServiceOrder
WHERE
	FSServiceOrder.CuryInfoID IS NOT NULL
	AND FSServiceOrder.CuryBillableOrderTotal IS NULL;

--[IfExists(Column = FSAppointment.CuryInfoID)]
--[IfExists(Column = FSAppointment.CuryBillableLineTotal)]
--[IfExistsSelect(From = FSAppointment, WhereIsNull = CuryBillableLineTotal)]
UPDATE FSAppointment
  SET CuryBillableLineTotal = (SELECT SUM(FSAppointmentDet.CuryBillableTranAmt)
  							   FROM  FSAppointmentDet
  							   WHERE FSAppointmentDet.SrvOrdType = FSAppointment.SrvOrdType
                               AND FSAppointmentDet.RefNbr = FSAppointment.RefNbr
                               AND FSAppointmentDet.CompanyID = FSAppointment.CompanyID
                               AND FSAppointmentDet.IsBillable = 1)
FROM FSAppointment
WHERE 
	FSAppointment.CuryInfoID IS NOT NULL
	AND FSAppointment.CuryBillableLineTotal IS NULL;

--[IfExists(Column = FSAppointment.CuryInfoID)]
--[IfExists(Column = FSAppointment.CuryBillableLineTotal)]
--[IfExistsSelect(From = FSAppointment, WhereIsNull = CuryBillableLineTotal)]
UPDATE FSAppointment
  SET CuryBillableLineTotal = 0.0
FROM FSAppointment
WHERE 
	FSAppointment.CuryInfoID IS NOT NULL
	AND FSAppointment.CuryBillableLineTotal IS NULL;

--[MinVersion(Branch = 18.0, Version = 18.091.0047, Hash = 075ccbe7e9ab9f1f5b14ab0538292d0f192b8bb5400ed6c3b835a9a916de2679)]
--[MinVersion(Branch = 18.1, Version = 18.100.0000, Hash = 075ccbe7e9ab9f1f5b14ab0538292d0f192b8bb5400ed6c3b835a9a916de2679)]
--[MinVersion(Branch = 18.2, Version = 18.200.0001, Hash = 075ccbe7e9ab9f1f5b14ab0538292d0f192b8bb5400ed6c3b835a9a916de2679)]
UPDATE FSSODet SET
	BillingRule = 'FLRA'
WHERE
	(
		(BillingRule IS NULL OR BillingRule <> 'FLRA')
		AND LineType IN ('SLPRO', 'PU_DL', 'NSTKI')
	)
	OR (
		BillingRule IS NULL
		AND LineType = 'SERVI'
	)
GO

--[IfExists(Column = FSServiceOrder.LineCntr)]
--[MinVersion(Hash = 4b0d93dece1b2b827e19717f66f7bab91da053787faef241355df7bcc03a5464)]
UPDATE FSServiceOrder
  SET LineCntr = ISNULL((SELECT MAX(LineNbr)
                    FROM  FSSODet
                    WHERE FSSODet.SrvOrdType = FSServiceOrder.SrvOrdType
                        AND FSSODet.RefNbr = FSServiceOrder.RefNbr
                        AND FSSODet.CompanyID = FSServiceOrder.CompanyID), 0)
FROM FSServiceOrder
WHERE LineCntr IS NULL OR
	  LineCntr <> ISNULL((SELECT MAX(LineNbr)
                    FROM  FSSODet
                    WHERE FSSODet.SrvOrdType = FSServiceOrder.SrvOrdType
                        AND FSSODet.RefNbr = FSServiceOrder.RefNbr
                        AND FSSODet.CompanyID = FSServiceOrder.CompanyID), 0);
GO

--[IfExistsSelect(From = FSxCustomerClass, WhereIsNull = BillShipmentSource)]
--[MinVersion(Hash = 91b889678b71926c34b79ff6be7cab41fad863f37b700d9e71e1aa6a5edadf8f)]
UPDATE FSxCustomerClass
  SET FSxCustomerClass.BillShipmentSource = 'SO';
GO

--[IfExistsSelect(From = FSCustomerClassBillingSetup, WhereIsNull = BillShipmentSource)]
--[MinVersion(Hash = 42b6486803507c121076db335f7d5298a887dd63c1ee312adce7d40a95be72ff)]
UPDATE FSCustomerClassBillingSetup
  SET FSCustomerClassBillingSetup.BillShipmentSource = 'SO';
GO

--[MinVersion(Hash = 5aca97e4cff59fc8a35d7c1147f2e60488aca922ab259a5e29987e6b2541a102)]
update pol set Released = por.Released from POReceiptLine pol join POReceipt por on pol.ReceiptType = por.ReceiptType and pol.ReceiptNbr = por.ReceiptNbr
GO

--[IfExistsSelect(From = FSxCustomer, WhereIsNull = BillShipmentSource)]
--[MinVersion(Hash = f58833735797752bdd11790e86647e3cc9622dfa5277efd4f600e6d42f1da75a)]
UPDATE FSxCustomer
  SET FSxCustomer.BillShipmentSource = 'SO';
GO

--[IfExistsSelect(From = FSCustomerBillingSetup, WhereIsNull = BillShipmentSource)]
--[MinVersion(Hash = d38a52e3fde4da2aeb70d2c7144138e6151267faca31741446320b1ee1422ccf)]
UPDATE FSCustomerBillingSetup
  SET FSCustomerBillingSetup.BillShipmentSource = 'SO';
GO

--[IfExists(Column = FSSODet.CuryExtraUsageUnitPrice)]
--[IfExistsSelect(From = FSSODet, WhereIsNull = CuryExtraUsageUnitPrice)]
--[MinVersion(Hash = ab31a4ca5958f8cb76307296ec0b97e1a4b67068fe59ff731613a31d9ef60d3b)]
UPDATE FSSODet 
	SET CuryExtraUsageUnitPrice = 0
FROM FSSODet
WHERE CuryExtraUsageUnitPrice IS NULL;
GO

--[IfExists(Column = FSAppointmentDet.CuryExtraUsageUnitPrice)]
--[IfExistsSelect(From = FSAppointmentDet, WhereIsNull = CuryExtraUsageUnitPrice)]
--[MinVersion(Hash = fd707a1989f87d774b8725a680eb0157326f81bc59ceebfa79874d97ddf4d866)]
UPDATE FSAppointmentDet 
	SET CuryExtraUsageUnitPrice = 0
FROM FSAppointmentDet
WHERE CuryExtraUsageUnitPrice IS NULL;
GO

--[IfExists(Column = FSSODet.ExtraUsageUnitPrice)]
--[IfExistsSelect(From = FSSODet, WhereIsNull = ExtraUsageUnitPrice)]
--[MinVersion(Hash = 4289f22fc8275a596c90564f7bfc35d81809e9028bb0b4faefbf16b3bbeb6f7f)]
UPDATE FSSODet 
	SET ExtraUsageUnitPrice = 0
FROM FSSODet
WHERE ExtraUsageUnitPrice IS NULL;
GO

--[IfExists(Column = FSAppointmentDet.ExtraUsageUnitPrice)]
--[IfExistsSelect(From = FSAppointmentDet, WhereIsNull = ExtraUsageUnitPrice)]
--[MinVersion(Hash = 6fcfc850fb5dfd15f91d731b73e4249d5fd4c3e909a202d129cedc85c11dfc30)]
UPDATE FSAppointmentDet 
	SET ExtraUsageUnitPrice = 0
FROM FSAppointmentDet
WHERE ExtraUsageUnitPrice IS NULL;
GO

--[mysql: Skip]
--[IfExists(Column = FSAppointmentDet.ActualDateTimeBegin)]
--[MinVersion(Hash = aa0024f7111fccdae1d1348537d68e5a5b6d5fd17d2936b808ad607abc18bcf5)]
UPDATE Emp
	SET Emp.ActualDateTimeBegin = App.ActualDateTimeBegin, 
		Emp.ActualDateTimeEnd = CASE WHEN Emp.ActualDuration = 0 
									 THEN App.ActualDateTimeEnd 
									 ELSE DATEADD(minute, Emp.ActualDuration, App.ActualDateTimeBegin) 
							    END,
		Emp.ActualDuration = CASE WHEN Emp.ActualDuration = 0 
								  THEN DATEDIFF(minute, App.ActualDateTimeBegin, App.ActualDateTimeEnd)
								  ELSE Emp.ActualDuration 
							 END
FROM FSAppointmentEmployee Emp
INNER JOIN FSAppointment App ON (Emp.CompanyID = App.CompanyID AND
									Emp.AppointmentID = App.AppointmentID)
WHERE
(App.Status = 'C' OR App.Status = 'Z') AND
Emp.ActualDateTimeBegin IS NULL
GO

--[mysql: Native]
--[mssql: Skip]
--[IfExists(Column = FSAppointmentDet.ActualDateTimeBegin)]
--[MinVersion(Hash = 598108a80e70023b7b4a626d4c9809ada3215f5b39de4865e9e52d3543742b17)]
UPDATE FSAppointmentEmployee 
		INNER JOIN FSAppointment 
			ON (FSAppointmentEmployee.CompanyID = FSAppointment.CompanyID AND
									FSAppointmentEmployee.AppointmentID = FSAppointment.AppointmentID)

	SET FSAppointmentEmployee.ActualDateTimeBegin = FSAppointment.ActualDateTimeBegin, 
		FSAppointmentEmployee.ActualDateTimeEnd = CASE 
													WHEN FSAppointmentEmployee.ActualDuration = 0 
												 	THEN FSAppointment.ActualDateTimeEnd 
												 	ELSE DATE_ADD(FSAppointment.ActualDateTimeBegin, INTERVAL FSAppointmentEmployee.ActualDuration MINUTE) 
										    	END,
		FSAppointmentEmployee.ActualDuration = CASE 
												   WHEN FSAppointmentEmployee.ActualDuration = 0 
									  			   THEN TIMESTAMPDIFF(minute, FSAppointment.ActualDateTimeBegin, FSAppointment.ActualDateTimeEnd)
									  			   ELSE FSAppointmentEmployee.ActualDuration 
							 				   END
WHERE
(FSAppointment.Status = 'C' OR FSAppointment.Status = 'Z') AND
FSAppointmentEmployee.ActualDateTimeBegin IS NULL
GO

--[MinVersion(Hash = 5e0fa3b1b9d8c6e1ef13cae6927b2eb36e7aa4ce11dd5d2c2f9612a65d6079a1)]
if exists(select * from sys.tables where object_id = object_id('PaymentType') )
exec sp_executesql N'DROP TABLE PaymentType'
GO

--[MinVersion(Hash = df2bce3c5640b791cbed9cb24658921c6b0c4116d964c2ebf5aa054993321256)]
if exists(select * from sys.tables where object_id = object_id('PaymentTypeDetail') )
exec sp_executesql N'DROP TABLE PaymentTypeDetail'
GO

--[SmartExecute]
--[MinVersion(Hash = 7daf569a60427eae219f0ca1c0fbb070ca3d0561b5e3dbd20336d95e98f0228d)]
UPDATE ils
SET QtyOnReceipt = ISNULL(PlanQtySum, 0)
FROM [INItemLotSerial] AS ils
LEFT JOIN (
	SELECT InventoryID, LotSerialNbr, SUM(PlanQty) AS PlanQtySum
	FROM [INItemPlan] ip
	INNER JOIN [INPlanType] pt ON ip.PlanType = pt.PlanType
	WHERE ip.[Reverse] = 0 AND (pt.InclQtyINReceipts + pt.InclQtyPOPrepared + pt.InclQtyPOOrders + pt.InclQtyPOReceipts + pt.InclQtyPOFixedReceipts + pt.InclQtyINAssemblySupply > 0)
		OR ip.[Reverse] = 1 AND (pt.InclQtySOBackOrdered + pt.InclQtySOPrepared + pt.InclQtySOBooked + pt.InclQtySOShipped + pt.InclQtySOShipping > 0)
	GROUP BY InventoryID, LotSerialNbr) AS pla
ON ils.InventoryID = pla.InventoryID AND ils.LotSerialNbr = pla.LotSerialNbr
GO

--[MinVersion(Hash = e80e6f9e3819fd2e749f86d1ac660bed8a3fc6e3be19aeb6873a2288950a1197)]
update n 
set n.SOOrderSortOrder = so.SortOrder
from ARTran n
inner join SOLine so on so.CompanyID = n.CompanyID and so.OrderType = n.SOOrderType and so.OrderNbr = n.SOOrderNbr and so.LineNbr = n.SOOrderLineNbr
where n.SOOrderSortOrder is null
GO

--[SmartExecute]
--[MinVersion(Hash = 7daf569a60427eae219f0ca1c0fbb070ca3d0561b5e3dbd20336d95e98f0228d)]
UPDATE ils
SET QtyOnReceipt = ISNULL(PlanQtySum, 0)
FROM [INItemLotSerial] AS ils
LEFT JOIN (
	SELECT InventoryID, LotSerialNbr, SUM(PlanQty) AS PlanQtySum
	FROM [INItemPlan] ip
	INNER JOIN [INPlanType] pt ON ip.PlanType = pt.PlanType
	WHERE ip.[Reverse] = 0 AND (pt.InclQtyINReceipts + pt.InclQtyPOPrepared + pt.InclQtyPOOrders + pt.InclQtyPOReceipts + pt.InclQtyPOFixedReceipts + pt.InclQtyINAssemblySupply > 0)
		OR ip.[Reverse] = 1 AND (pt.InclQtySOBackOrdered + pt.InclQtySOPrepared + pt.InclQtySOBooked + pt.InclQtySOShipped + pt.InclQtySOShipping > 0)
	GROUP BY InventoryID, LotSerialNbr) AS pla
ON ils.InventoryID = pla.InventoryID AND ils.LotSerialNbr = pla.LotSerialNbr
GO

--[MinVersion(Hash = e80e6f9e3819fd2e749f86d1ac660bed8a3fc6e3be19aeb6873a2288950a1197)]
update n 
set n.SOOrderSortOrder = so.SortOrder
from ARTran n
inner join SOLine so on so.CompanyID = n.CompanyID and so.OrderType = n.SOOrderType and so.OrderNbr = n.SOOrderNbr and so.LineNbr = n.SOOrderLineNbr
where n.SOOrderSortOrder is null
GO

--[SmartExecute]
--[MinVersion(Hash = 7daf569a60427eae219f0ca1c0fbb070ca3d0561b5e3dbd20336d95e98f0228d)]
UPDATE ils
SET QtyOnReceipt = ISNULL(PlanQtySum, 0)
FROM [INItemLotSerial] AS ils
LEFT JOIN (
	SELECT InventoryID, LotSerialNbr, SUM(PlanQty) AS PlanQtySum
	FROM [INItemPlan] ip
	INNER JOIN [INPlanType] pt ON ip.PlanType = pt.PlanType
	WHERE ip.[Reverse] = 0 AND (pt.InclQtyINReceipts + pt.InclQtyPOPrepared + pt.InclQtyPOOrders + pt.InclQtyPOReceipts + pt.InclQtyPOFixedReceipts + pt.InclQtyINAssemblySupply > 0)
		OR ip.[Reverse] = 1 AND (pt.InclQtySOBackOrdered + pt.InclQtySOPrepared + pt.InclQtySOBooked + pt.InclQtySOShipped + pt.InclQtySOShipping > 0)
	GROUP BY InventoryID, LotSerialNbr) AS pla
ON ils.InventoryID = pla.InventoryID AND ils.LotSerialNbr = pla.LotSerialNbr
GO

--[MinVersion(Hash = e80e6f9e3819fd2e749f86d1ac660bed8a3fc6e3be19aeb6873a2288950a1197)]
update n 
set n.SOOrderSortOrder = so.SortOrder
from ARTran n
inner join SOLine so on so.CompanyID = n.CompanyID and so.OrderType = n.SOOrderType and so.OrderNbr = n.SOOrderNbr and so.LineNbr = n.SOOrderLineNbr
where n.SOOrderSortOrder is null
GO

--[MinVersion(Hash = 58c0a132279461f9751e768c3c2f628c1388403fbbc47ab9b519af40b1654cb2)]
update n 
set n.SubItemID = so.SubItemID
from ARTran n
inner join SOLine so on so.CompanyID = n.CompanyID and so.OrderType = n.SOOrderType and so.OrderNbr = n.SOOrderNbr and so.LineNbr = n.SOOrderLineNbr
where n.SubItemID is null
GO

--[MinVersion(Hash = 59ba2c7b460ed393ad543384abd98ab55f1a9693b86a8b5c32b37acad4dd8dcb)]
update i set
TermsID = null,
DiscDate = null
from ARInvoice i
inner join SOInvoice s
on s.CompanyID = i.CompanyID and s.DocType = i.DocType and s.RefNbr = i.RefNbr
where i.DocType = 'CRM'
GO

--[MinVersion(Hash = 6105913be28138250538d9d6260878f5255ffb477b38638b6ee2a0183d80e1fa)]
update a set
a.CuryOrigDiscAmt = 0,
a.OrigDiscAmt = 0,
a.DueDate = null
from ARRegister a
inner join SOInvoice s
on s.CompanyID = a.CompanyID and s.DocType = a.DocType and s.RefNbr = a.RefNbr
where a.DocType = 'CRM'
GO

--[IfExists(Table = LandedCostTran)]
--[MinVersion(Hash = 167e186f8d02d62b70cf519e9d3a9f2b41c9b67feb96d53ceb093acb2f5af433)]
update a
set a.LineType = 'LA'
from
APTran a inner join LandedCostTran lc on lc.CompanyID = a.CompanyID and lc.LCTranID = a.LCTranID
where a.LineType = '' and (lc.Source = 'AP' or lc.PostponeAP = 1)
GO

--[IfExists(Table = LandedCostTran)]
--[MinVersion(Hash = 9473e98690b4054266218fb152bcc50edc0cde44b8b422d92b7193513dc5c577)]
update a
set a.LineType = 'LP'
from
APTran a inner join LandedCostTran lc on lc.CompanyID = a.CompanyID and lc.LCTranID = a.LCTranID
where a.LineType = '' and lc.Source = 'PO' and lc.PostponeAP = 0
GO

--[MinVersion(Hash = 6cbeebcfad7d61b247103844e6455ec1d7595bfe243ddcea8cfe4b7372d9ae1c)]
update FeaturesSet set CustomerDiscounts = VendorDiscounts where CustomerDiscounts is null
GO

--[MinVersion(Hash = e8fa9a5e48b5a5f612c2104a414c4356000fe38d21b4a876c12d780bfb8b471d)]
UPDATE dbo.PaymentMethodDetail 
  SET IsCVV = 1
  WHERE DetailID = 'CVV';
GO

--[MinVersion(Hash = 9dbc5f97023788e1586cb1e46a2f0a6bc7e67792006457c765a0142d1429bbf9)]
INSERT CCProcessingCenterDetail (CompanyID, ProcessingCenterID, DetailID, Descr, Value, ControlType, ComboValues, IsEncryptionRequired, IsEncrypted, CreatedByID, CreatedByScreenID, CreatedDateTime, LastModifiedByID, LastModifiedByScreenID, LastModifiedDateTime)
SELECT ccd.CompanyID
      ,ccd.ProcessingCenterID
      ,'VALIDATION'
      ,'The processing mode for the request'
      ,'LiveMode'
      ,2
      ,'LiveMode|Live Mode;TestMode|Test Mode;'
      ,ccd.IsEncryptionRequired
      ,ccd.IsEncrypted
      ,ccd.CreatedByID
      ,ccd.CreatedByScreenID
      ,ccd.CreatedDateTime
      ,ccd.LastModifiedByID
      ,ccd.LastModifiedByScreenID
      ,ccd.LastModifiedDateTime
  FROM CCProcessingCenter cc 
  INNER JOIN CCProcessingCenterDetail ccd ON cc.CompanyID = ccd.CompanyID AND cc.ProcessingCenterID = ccd.ProcessingCenterID
  WHERE cc.ProcessingTypeName = 'PX.CCProcessing.V2.AuthnetProcessingPlugin' AND ccd.DetailID = 'TESTMODE';
GO

--[Timeout(Multiplier = 100)]
--[MinVersion(Hash = 57f41df098d292af8f0305cb44b30c9248525295624098b433188e2a2b78db69)]
UPDATE
	Contact
SET
	Attention = Salutation
WHERE
	ContactType IN ('AP', 'EP')
GO

--[Timeout(Multiplier = 100)]
--[OldHash(Hash = 5f3c2a9ff1fe4513e5488c966db05ee9142920f0ce6cc5ac6a92d8c5588624fb)]
--[MinVersion(Hash = b16786e4ff97829146d237e60e24d578825fddb2e561fb2129046c80f5fb385e)]
UPDATE soc
SET
	Attention = c.Attention
FROM
	SOContact soc
INNER JOIN Contact c
	ON c.ContactID = soc.CustomerContactID
	AND c.CompanyID = soc.CompanyID
WHERE 
	soc.IsDefaultContact = 1 AND soc.Attention IS NULL

UPDATE pmc
SET
	Attention = c.Attention
FROM 
	PMContact pmc
INNER JOIN Contact c
	ON c.ContactID = pmc.CustomerContactID
	AND c.CompanyID = pmc.CompanyID
WHERE 
	pmc.IsDefaultContact = 1 AND pmc.Attention IS NULL
	
UPDATE arc
SET
	Attention = c.Attention
FROM
	ARContact arc
INNER JOIN Contact c
	ON c.ContactID = arc.CustomerContactID
	AND c.CompanyID = arc.CompanyID
WHERE 
	arc.IsDefaultContact = 1 AND arc.Attention IS NULL	
	
UPDATE apc
SET
	Attention = c.Attention
FROM
	APContact apc
INNER JOIN Contact c
	ON c.ContactID = apc.VendorContactID
	AND c.CompanyID = apc.CompanyID
WHERE 
	apc.IsDefaultContact = 1 AND apc.Attention IS NULL	

UPDATE crc
SET
	Attention = c.Attention
FROM
	CRContact crc
INNER JOIN Contact c
	ON c.ContactID = crc.BAccountContactID
	AND c.CompanyID = crc.CompanyID
WHERE 
	crc.IsDefaultContact = 1 AND crc.Attention IS NULL		

UPDATE poc
SET
	Attention = c.Attention
FROM
	POContact poc
INNER JOIN Contact c
	ON c.ContactID = poc.BAccountContactID
	AND c.CompanyID = poc.CompanyID
WHERE 
	poc.IsDefaultContact = 1 AND poc.Attention IS NULL
GO

--[MinVersion(Hash = 225098424f23fc6fe3e78161d2a854a28ccbe21232c563ecafda64568cd46c9a)]
UPDATE soc
SET
	Attention = Salutation
FROM
	SOContact soc
WHERE    
soc.IsDefaultContact = 0 AND soc.Attention IS NULL

UPDATE pmc
SET
	Attention = Salutation
FROM 
	PMContact pmc
WHERE 
	pmc.IsDefaultContact = 0 AND pmc.Attention IS NULL
	
UPDATE arc
SET
	Attention = Salutation
FROM
	ARContact arc
WHERE 
	arc.IsDefaultContact = 0 AND arc.Attention IS NULL	
	
UPDATE apc
SET
	Attention = Salutation
FROM
	APContact apc
WHERE 
	apc.IsDefaultContact = 0 AND apc.Attention IS NULL	

UPDATE crc
SET
	Attention = Salutation
FROM
	CRContact crc
WHERE 
	crc.IsDefaultContact = 0 AND crc.Attention IS NULL		

UPDATE poc
SET
	Attention = Salutation
FROM
	POContact poc
WHERE 
	poc.IsDefaultContact = 0 AND poc.Attention IS NULL
GO

--[MinVersion(Hash = a9448dd389dede6c49f459ee71cd21e385b50c92ae52991554c8629719f12afe)]
IF
(	EXISTS(SELECT 1 FROM sys.columns WHERE object_id = object_id('APAdjust') AND name = 'CuryAdjgPPDAmt') AND 
	EXISTS(SELECT 1 FROM sys.columns WHERE object_id = object_id('APAdjust') AND name = 'CuryAdjdPPDAmt') AND 
	EXISTS(SELECT 1 FROM sys.columns WHERE object_id = object_id('APAdjust') AND name = 'AdjPPDAmt')
)
BEGIN
EXEC sp_executesql N'
UPDATE APAdjust SET 
	CuryAdjgPPDAmt = CuryAdjgDiscAmt, 
	CuryAdjdPPDAmt = CuryAdjdDiscAmt, 
	AdjPPDAmt = AdjDiscAmt
WHERE CuryAdjgPPDAmt IS NULL AND 
	CuryAdjdPPDAmt IS NULL AND
	AdjPPDAmt IS NULL'
END
GO

--[MinVersion(Hash = cc9b5c82b927ff325e94bade2c1fefed0638be56aa8f594966b015282a4a78eb)]
UPDATE Widget
SET Settings = REPLACE(
			   REPLACE(
			   REPLACE(
			   REPLACE(
			   REPLACE(Settings,'<InquiryScreenID>CR3010PL</InquiryScreenID>', '<InquiryScreenID>CR3010P9</InquiryScreenID>'),
							    '<InquiryScreenID>CR3020PL</InquiryScreenID>', '<InquiryScreenID>CR3020P9</InquiryScreenID>'),
							    '<InquiryScreenID>CR3030PL</InquiryScreenID>', '<InquiryScreenID>CR3030P9</InquiryScreenID>'),
							    '<InquiryScreenID>CR3040PL</InquiryScreenID>', '<InquiryScreenID>CR3040P9</InquiryScreenID>'),
							    '<InquiryScreenID>CR3060PL</InquiryScreenID>', '<InquiryScreenID>CR3060P9</InquiryScreenID>')
WHERE CompanyID != 1 And Settings LIKE '%>CR%'

UPDATE PivotTable
SET ScreenID = REPLACE(
      REPLACE(
      REPLACE(
      REPLACE(
      REPLACE(ScreenID,'CR3010PL', 'CR3010P9'),
           'CR3020PL', 'CR3020P9'),
           'CR3030PL', 'CR3030P9'),
           'CR3040PL', 'CR3040P9'),
           'CR3060PL', 'CR3060P9')
WHERE CompanyID != 1 AND ScreenID LIKE 'CR%'

UPDATE PivotField
SET ScreenID = REPLACE(
      REPLACE(
      REPLACE(
      REPLACE(
      REPLACE(ScreenID,'CR3010PL', 'CR3010P9'),
           'CR3020PL', 'CR3020P9'),
           'CR3030PL', 'CR3030P9'),
           'CR3040PL', 'CR3040P9'),
           'CR3060PL', 'CR3060P9')
WHERE CompanyID != 1 AND ScreenID LIKE 'CR%'

UPDATE PivotFieldPreferences
SET ScreenID = REPLACE(
      REPLACE(
      REPLACE(
      REPLACE(
      REPLACE(ScreenID,'CR3010PL', 'CR3010P9'),
           'CR3020PL', 'CR3020P9'),
           'CR3030PL', 'CR3030P9'),
           'CR3040PL', 'CR3040P9'),
           'CR3060PL', 'CR3060P9')
WHERE CompanyID != 1 AND ScreenID LIKE 'CR%'

UPDATE SiteMap
SET Url = REPLACE(
      REPLACE(
      REPLACE(
      REPLACE(
      REPLACE(Url,'CR3010PL', 'CR3010P9'),
           'CR3020PL', 'CR3020P9'),
           'CR3030PL', 'CR3030P9'),
           'CR3040PL', 'CR3040P9'),
           'CR3060PL', 'CR3060P9')
WHERE CompanyID != 1 AND Url LIKE '%~/Pivot/Pivot.aspx?ScreenID=CR%'
GO

-- Update CROpportunity: DocumentDate=CloseDate, when no Primary Quote is defined
--[IfExists(Column = CRQuote.RevisionID)]
--[MinVersion(Hash = 5a6dbb50000bb4f6db246a7522b36f98fa74a8a74110ea9a7a624ab76c5c788f)]
UPDATE r SET DocumentDate=o.CloseDate
FROM CROpportunityRevision AS r
JOIN CROpportunity AS o ON o.CompanyID=r.CompanyID AND o.OpportunityID=r.OpportunityID AND o.DefRevisionID=r.RevisionID
LEFT JOIN CRQuote AS q ON q.CompanyID=r.CompanyID AND q.OpportunityID=r.OpportunityID AND q.RevisionID=r.RevisionID
WHERE q.CompanyID IS NULL
GO

-- Update CROpportunity: DocumentDate=CloseDate, when no Primary Quote is defined
--[IfExists(Column = CRQuote.QuoteID)]
--[IfNotExists(Column = CROpportunity.DefQuoteID)]
--[IfNotExists(Column = CRQuote.RevisionID)]
--[MinVersion(Hash = 19a11cdb84a0456725dd3c1dd3c036b800d48300255e44fb466ff1b92ca0279b)]
UPDATE r SET DocumentDate=o.CloseDate
FROM CROpportunityRevision AS r
JOIN CROpportunity AS o ON o.CompanyID=r.CompanyID AND o.OpportunityID=r.OpportunityID AND o.DefRevisionID=r.RevisionID
LEFT JOIN CRQuote AS q ON q.CompanyID=r.CompanyID AND q.QuoteID=r.NoteID
WHERE q.CompanyID IS NULL
GO

-- Update CROpportunity: DocumentDate=CloseDate, when no Primary Quote is defined
--[IfExists(Column = CRQuote.QuoteID)]
--[IfExists(Column = CROpportunity.DefQuoteID)]
--[IfNotExists(Column = CRQuote.RevisionID)]
--[MinVersion(Hash = 875f3dd790a9bae3434d9e91d14d5f81ae9955b2fbe30e0ff7793f36dd264cef)]
UPDATE r SET DocumentDate=o.CloseDate
FROM CROpportunityRevision AS r
JOIN CROpportunity AS o ON o.CompanyID=r.CompanyID AND o.DefQuoteID=r.NoteID
LEFT JOIN CRQuote AS q ON q.CompanyID=r.CompanyID AND q.QuoteID=r.NoteID
WHERE q.CompanyID IS NULL
GO

--[mysql: Native]
--[mssql: Skip]
--[MinVersion(Hash = d83baf3a5ebed4abc34ca8b7244cd982cb1dd00031de49a79882f3fc7d3318c6)]
/*Update AR661000. LetterID -> DunningLetterID*/
update UserReport set xml = UpdateXML(xml, '/Report/Filters/FilterExp/Value[text()="@LetterID"]', '<Value>@DunningLetterID</Value>') 
where ReportFileName = "ar661000.rpx";

update UserReport set xml = UpdateXML(xml, '/Report/Filters/FilterExp/DataField[text()="@LetterID"]', '<DataField>@DunningLetterID</DataField>') 
where ReportFileName = "ar661000.rpx";

update UserReport set xml = UpdateXML(xml, '/Report/Parameters/ReportParameter/Name[text()="LetterID"]', '<Name>DunningLetterID</Name>') 
where ReportFileName = "ar661000.rpx";


/*Update AR641500. CustomerID -> StatementCustomerID*/
update UserReport set xml = UpdateXML(xml, '/Report/ExportFileName[text()="=Format( ''AR Statement {0} {1}'',[@CustomerID],[@StatementDate])"]', 
'<ExportFileName>=Format( ''AR Statement {0} {1}'',[@StatementCustomerID],[@StatementDate])</ExportFileName>') 
where ReportFileName = "ar641500.rpx";

update UserReport set xml = UpdateXML(xml, '/Report/Filters/FilterExp/Value[text()="@CustomerId"]', '<Value>@StatementCustomerId</Value>') 
where ReportFileName = "ar641500.rpx";

update UserReport set xml = UpdateXML(xml, '/Report/Filters/FilterExp/DataField[text()="@CustomerId"]', '<DataField>@StatementCustomerId</DataField>') 
where ReportFileName = "ar641500.rpx";

update UserReport set xml = UpdateXML(xml, '/Report/Parameters/ReportParameter/Name[text()="CustomerId"]', '<Name>StatementCustomerId</Name>') 
where ReportFileName = "ar641500.rpx";


/*Update AR642000. CustomerID -> StatementCustomerID*/
update UserReport set xml = UpdateXML(xml, '/Report/ExportFileName[text()="=Format( ''AR Statement MC {0} {1}'', [@CustomerID],[@StatementDate])"]', 
'<ExportFileName>=Format( ''AR Statement MC {0} {1}'', [@StatementCustomerID],[@StatementDate])</ExportFileName>') 
where ReportFileName = "ar642000.rpx";

update UserReport set xml = UpdateXML(xml, '/Report/Filters/FilterExp/Value[text()="@CustomerId"]', '<Value>@StatementCustomerId</Value>') 
where ReportFileName = "ar642000.rpx";

update UserReport set xml = UpdateXML(xml, '/Report/Filters/FilterExp/DataField[text()="@CustomerId"]', '<DataField>@StatementCustomerId</DataField>') 
where ReportFileName = "ar642000.rpx";

update UserReport set xml = UpdateXML(xml, '/Report/Parameters/ReportParameter/Name[text()="CustomerId"]', '<Name>StatementCustomerId</Name>') 
where ReportFileName = "ar642000.rpx";


/*Update SO641010. RefNbr -> OrderNbr*/
update UserReport set xml = UpdateXML(xml, '/Report/Filters/FilterExp/Value[text()="@RefNbr"]', '<Value>@OrderNbr</Value>') 
where ReportFileName = "so641010.rpx";

update UserReport set xml = UpdateXML(xml, '/Report/Parameters/ReportParameter/Name[text()="RefNbr"]', '<Name>OrderNbr</Name>') 
where ReportFileName = "so641010.rpx";
GO

--[mysql: Skip]
--[MinVersion(Hash = 6e373817d1bdfd14b886101fb1b7481cc29555c6317a7fba5985d0ab4ba405d3)]
create table TempUserReport
(
	CompanyID int,
	ReportFileName nvarchar(50),
	Version int,
	ReportXML xml,
	ReportNVarChar nvarchar(max)
)

--Update AR661000. LetterID -> DunningLetterID
insert into TempUserReport
select CompanyID, ReportFileName, Version, null, Xml
from UserReport where ReportFileName = 'AR661000.rpx'

update TempUserReport set ReportXML = cast(REPLACE(TempUserReport.ReportNVarChar,
'<?xml version="1.0" encoding="utf-8"?>',
'<?xml version="1.0" encoding="utf-16"?>') as xml)
update TempUserReport set ReportXML.modify('replace value of (/Report/Filters/FilterExp/Value[text()="@LetterID"]/text())[1] with "@DunningLetterID"')
update TempUserReport set ReportXML.modify('replace value of (/Report/Filters/FilterExp/DataField[text()="@LetterID"]/text())[1] with "@DunningLetterID"')
update TempUserReport set ReportXML.modify('replace value of (/Report/Parameters/ReportParameter/Name[text()="LetterID"]/text())[1] with "DunningLetterID"')
update TempUserReport set ReportNVarChar = '<?xml version="1.0" encoding="utf-8"?>' + cast(ReportXML as nvarchar(max))

update u set u.Xml = t.ReportNVarChar
from UserReport u inner join TempUserReport t 
on t.CompanyID = u.CompanyID and t.ReportFileName = u.ReportFileName and t.Version = u.Version
where u.ReportFileName = 'AR661000.rpx'

delete from TempUserReport

--Update AR641500. CustomerID -> StatementCustomerID
insert into TempUserReport
select CompanyID, ReportFileName, Version, null, Xml
from UserReport where ReportFileName = 'AR641500.rpx'

update TempUserReport set ReportXML = cast(REPLACE(TempUserReport.ReportNVarChar,
'<?xml version="1.0" encoding="utf-8"?>',
'<?xml version="1.0" encoding="utf-16"?>') as xml)
update TempUserReport set ReportXML.modify('replace value of (/Report/ExportFileName[text()="=Format( ''AR Statement {0} {1}'',[@CustomerID],[@StatementDate])"]/text())[1] with "=Format( ''AR Statement {0} {1}'',[@StatementCustomerID],[@StatementDate])"')
update TempUserReport set ReportXML.modify('replace value of (/Report/Filters/FilterExp/Value[text()="@CustomerId"]/text())[1] with "@StatementCustomerId"')
update TempUserReport set ReportXML.modify('replace value of (/Report/Filters/FilterExp/DataField[text()="@CustomerId"]/text())[1] with "@StatementCustomerId"')
update TempUserReport set ReportXML.modify('replace value of (/Report/Parameters/ReportParameter/Name[text()="CustomerId"]/text())[1] with "StatementCustomerId"')
update TempUserReport set ReportNVarChar = '<?xml version="1.0" encoding="utf-8"?>' + cast(ReportXML as nvarchar(max))

update u set u.Xml = t.ReportNVarChar
from UserReport u inner join TempUserReport t on t.CompanyID = u.CompanyID and t.ReportFileName = u.ReportFileName and t.Version = u.Version
where u.ReportFileName = 'AR641500.rpx'

delete from TempUserReport

--Update AR642000. CustomerID -> StatementCustomerID
insert into TempUserReport
select CompanyID, ReportFileName, Version, null, Xml
from UserReport where ReportFileName = 'AR642000.rpx'

update TempUserReport set ReportXML = cast(REPLACE(TempUserReport.ReportNVarChar,
'<?xml version="1.0" encoding="utf-8"?>',
'<?xml version="1.0" encoding="utf-16"?>') as xml)
update TempUserReport set ReportXML.modify('replace value of (/Report/ExportFileName[text()="=Format( ''AR Statement MC {0} {1}'', [@CustomerID],[@StatementDate])"]/text())[1] with "=Format( ''AR Statement MC {0} {1}'', [@StatementCustomerID],[@StatementDate])"')
update TempUserReport set ReportXML.modify('replace value of (/Report/Filters/FilterExp/Value[text()="@CustomerId"]/text())[1] with "@StatementCustomerId"')
update TempUserReport set ReportXML.modify('replace value of (/Report/Filters/FilterExp/DataField[text()="@CustomerId"]/text())[1] with "@StatementCustomerId"')
update TempUserReport set ReportXML.modify('replace value of (/Report/Parameters/ReportParameter/Name[text()="CustomerId"]/text())[1] with "StatementCustomerId"')
update TempUserReport set ReportNVarChar = '<?xml version="1.0" encoding="utf-8"?>' + cast(ReportXML as nvarchar(max))

update u set u.Xml = t.ReportNVarChar
from UserReport u inner join TempUserReport t on t.CompanyID = u.CompanyID and t.ReportFileName = u.ReportFileName and t.Version = u.Version
where u.ReportFileName = 'AR642000.rpx'

delete from TempUserReport

--Update SO641010. RefNbr -> OrderNbr
insert into TempUserReport
select CompanyID, ReportFileName, Version, null, Xml
from UserReport where ReportFileName = 'SO641010.rpx'

update TempUserReport set ReportXML = cast(REPLACE(TempUserReport.ReportNVarChar,
'<?xml version="1.0" encoding="utf-8"?>',
'<?xml version="1.0" encoding="utf-16"?>') as xml)
update TempUserReport set ReportXML.modify('replace value of (/Report/Filters/FilterExp/Value[text()="@RefNbr"]/text())[1] with "@OrderNbr"')
update TempUserReport set ReportXML.modify('replace value of (/Report/Parameters/ReportParameter/Name[text()="RefNbr"]/text())[1] with "OrderNbr"')
update TempUserReport set ReportNVarChar = '<?xml version="1.0" encoding="utf-8"?>' + cast(ReportXML as nvarchar(max))

update u set u.Xml = t.ReportNVarChar
from UserReport u inner join TempUserReport t on t.CompanyID = u.CompanyID and t.ReportFileName = u.ReportFileName and t.Version = u.Version
where u.ReportFileName = 'SO641010.rpx'

Drop Table TempUserReport
GO

-- Update RolesInGraph for SM202065 from SM201060
--[MinVersion(Hash = f7a6018bd41a241f56072eaf5e1a7aaa078042cc4038b404d961ace663891bb2)]
IF NOT EXISTS(Select * FROM RolesInGraph WHERE CompanyID <> 1 AND ScreenID = 'SM202065')
INSERT INTO RolesInGraph (
	CompanyID,
	ScreenID,
	Rolename,
	ApplicationName,
	Accessrights,
	CompanyMask,
	CreatedByID,
	CreatedByScreenID,
	CreatedDateTime,
	LastModifiedByID,
	LastModifiedByScreenID,
	LastModifiedDateTime,
	RecordSourceID)
	SELECT
		CompanyID,
		'SM202065',
		Rolename,
		ApplicationName,
		Accessrights,
		CompanyMask,
		CreatedByID,
		CreatedByScreenID,
		CreatedDateTime,
		LastModifiedByID,
		LastModifiedByScreenID,
		LastModifiedDateTime,
		RecordSourceID
	FROM 
		RolesInGraph
	WHERE 
		CompanyID <> 1 
		AND ScreenID = 'SM201060'
GO

--[IfExists(Column = FSServiceOrder.CuryDocTotal)]
--[IfExistsSelect(From = FSServiceOrder, WhereIsNull = CuryDocTotal)]
--[MinVersion(Hash = 13f48ed1289a72d869c0f48ea0d690c6b7630daceb06af9fdd41d8d6f647b81b)]
UPDATE FSServiceOrder SET CuryDocTotal = COALESCE(CuryBillableOrderTotal, 0) WHERE CuryDocTotal IS NULL;
GO

--[IfExists(Column = FSServiceOrder.DocTotal)]
--[IfExistsSelect(From = FSServiceOrder, WhereIsNull = DocTotal)]
--[MinVersion(Hash = 9451135fea226c42583dc0249566261df90e6c30fef680dc1106be746e87f0b8)]
UPDATE FSServiceOrder SET DocTotal = COALESCE(BillableOrderTotal, 0) WHERE DocTotal IS NULL;
GO

--[IfExists(Column = FSAppointment.ApptLineNbr)]
--[MinVersion(Hash = c56be00f921165501755e362dd5caa259b86cf46eb5c6c6b38e199b25a894d62)]
UPDATE FSAppointmentDet SET ApptLineNbr = AppDetID where ApptLineNbr IS NULL;
GO

--[IfExists(Column = FSAppointment.ApptLineNbr)]
--[IfExists(Column = FSAppointment.LineCntr)]
--[IfExistsSelect(From = FSAppointment, WhereIsNull = LineCntr)]
--[MinVersion(Hash = 24eb96a7b8298d68d3dba9a935f7eebbe671b0e2bdc713fb4daf5d7b3d2a324f)]
UPDATE FSAppointment
  SET LineCntr = (SELECT MAX(ApptLineNbr)
                    FROM  FSAppointmentDet
                    WHERE FSAppointmentDet.SrvOrdType = FSAppointment.SrvOrdType
                        AND FSAppointmentDet.RefNbr = FSAppointment.RefNbr
                        AND FSAppointmentDet.CompanyID = FSAppointment.CompanyID)
FROM FSAppointment
WHERE LineCntr IS NULL;
GO

--[IfExists(Column = PortalSetup.BranchID)]
--[MinVersion(Hash = 6f07b0844d227f9fc757ecc1212962610b18f40023a174e8b4d78c5c8b4aaed3)]
UPDATE PortalSetup SET
	RestrictByBranchID = branch.BranchID,
	RestrictByOrganizationID = CASE WHEN branch.BranchID IS NULL THEN childBranch.OrganizationID ELSE NULL END,
	DisplayFinancialDocuments =
		CASE
			WHEN branch.BranchID IS NOT NULL THEN 'B'
			WHEN childBranch.OrganizationID IS NOT NULL THEN 'C'
			ELSE 'A'
		END
FROM PortalSetup AS setup
LEFT JOIN Branch AS branch ON
	branch.CompanyID = setup.CompanyID AND
	branch.BranchID = setup.BranchID AND
	NOT EXISTS(SELECT '' FROM BAccount WHERE CompanyID = branch.CompanyID AND AcctCD = branch.BranchCD AND [Type] = 'OR')
LEFT JOIN Branch AS childBranch ON
	childBranch.CompanyID = setup.CompanyID AND
	childBranch.ParentBranchID = setup.BranchID AND
	NOT EXISTS(SELECT '' FROM BAccount WHERE CompanyID = childBranch.CompanyID AND AcctCD = childBranch.BranchCD AND [Type] = 'OR')
GO

--[IfExists(Column = INSite.Exclude)]
--[MinVersion(Hash = 24599046dd5ba1fa28759c178be848c62f6a8e929f6ca3ec614b4468264ef7ee)]
INSERT INTO WarehouseReference (CompanyID, SiteID, PortalSetupID)
SELECT insite.CompanyID, insite.SiteID, '' 
FROM INSite AS insite 
WHERE insite.Exclude=0 AND 
      insite.Active=1 AND
	  insite.SiteID NOT IN (SELECT TransitSiteID FROM INSetup WHERE CompanyID = insite.CompanyID)
GO

--[IfExists(Column = FSAppointment.BranchID)]
--[IfExistsSelect(From = FSAppointment, WhereIsNull = BranchID)]
--[MinVersion(Hash = e943fde9fcb9d64271463c7d63537c8afae3fa0004febd2915988ec70c6f1810)]
UPDATE FSAppointment
  SET BranchID = FSServiceOrder.BranchID
FROM FSAppointment
INNER JOIN FSServiceOrder
ON(
  FSServiceOrder.SrvOrdType = FSAppointment.SrvOrdType
  AND FSServiceOrder.RefNbr = FSAppointment.SORefNbr
  AND FSServiceOrder.CompanyID = FSAppointment.CompanyID)
WHERE FSAppointment.BranchID IS NULL;
GO

--[IfExists(Column = FSAppointment.CuryDocTotal)]
--[IfExistsSelect(From = FSAppointment, WhereIsNull = CuryDocTotal)]
--[MinVersion(Hash = 3365a580b8cd085ea9425b65199656f46b697f6a800146f6302a60d8c64776f1)]
UPDATE FSAppointment SET CuryDocTotal = COALESCE(CuryBillableLineTotal, 0) WHERE CuryDocTotal IS NULL;
GO

--[IfExists(Column = FSAppointment.DocTotal)]
--[IfExistsSelect(From = FSAppointment, WhereIsNull = DocTotal)]
--[MinVersion(Hash = 13370854802d28a56504b9a803f4cc53fab79ab22306cc7c25fcf1143017c58d)]
UPDATE FSAppointment SET DocTotal = COALESCE(BillableLineTotal, 0) WHERE DocTotal IS NULL;
GO

--[MinVersion(Hash = 7469fabf7b865960bc4bd6ee700319f23e7e40780bb962d9db9cf414dba881ed)]
update a
set a.isActive = ISNULL(so.OrderRequestApproval, 0)
from SOSetupApproval a
left join SOSetup so on so.CompanyID = a.CompanyID
where a.isActive <> ISNULL(so.OrderRequestApproval, 0)
GO

--[MinVersion(Hash = bfa78ec3e000ecb16d2ee9c0d2b3c1ff2610993eca7eba213bb66a5b5fd44a7a)]
IF
(	EXISTS(SELECT 1 FROM sys.columns WHERE object_id = object_id('Organization') AND name = 'FileTaxesByBranches') AND 
	EXISTS(SELECT 1 FROM sys.columns WHERE object_id = object_id('Branch') AND name = 'FileTaxesByBranches')
)
BEGIN
EXEC sp_executesql N'
UPDATE o 
	SET o.FileTaxesByBranches = b.FileTaxesByBranches
FROM Branch AS b
	INNER JOIN Organization AS o
	on b.CompanyID = o.CompanyID
		AND b.BranchID = o.ConvertedFromBranchID
		AND o.OrganizationType IN (''Balancing'', ''NotBalancing'')'
END
GO

--[MinVersion(Hash = 6682c02567fbb6014d07743b1dffc0c277fcb7abfcc15667e2324935fb06c9d7)]
UPDATE APRegister SET ProjectID = ISNUll(c.ContractID, 0)
FROM APRegister a
LEFT JOIN Contract c ON a.CompanyID = c.CompanyID AND c.BaseType='P' AND c.NonProject = 1
WHERE a.ProjectID IS NULL

UPDATE APTran SET ProjectID = ISNUll(c.ContractID, 0)
FROM APTran a
LEFT JOIN Contract c ON a.CompanyID = c.CompanyID AND c.BaseType='P' AND c.NonProject = 1
WHERE a.ProjectID IS NULL

UPDATE POOrder SET ProjectID = ISNUll(c.ContractID, 0)
FROM POOrder a
LEFT JOIN Contract c ON a.CompanyID = c.CompanyID AND c.BaseType='P' AND c.NonProject = 1
WHERE a.ProjectID IS NULL

UPDATE POLine SET ProjectID = ISNUll(c.ContractID, 0)
FROM POLine a
LEFT JOIN Contract c ON a.CompanyID = c.CompanyID AND c.BaseType='P' AND c.NonProject = 1
WHERE a.ProjectID IS NULL

UPDATE POReceipt SET ProjectID = ISNUll(c.ContractID, 0)
FROM POReceipt a
LEFT JOIN Contract c ON a.CompanyID = c.CompanyID AND c.BaseType='P' AND c.NonProject = 1
WHERE a.ProjectID IS NULL

UPDATE POReceiptLine SET ProjectID = ISNUll(c.ContractID, 0)
FROM POReceiptLine a
LEFT JOIN Contract c ON a.CompanyID = c.CompanyID AND c.BaseType='P' AND c.NonProject = 1
WHERE a.ProjectID IS NULL
GO

--Populate default SO Freight Amount Source
--[IfExists(Column = ShipTerms.FreightAmountSource)]
--[IfExists(Column = SOOrder.FreightAmountSource)]
--[IfExists(Column = SOShipment.FreightAmountSource)]
--[MinVersion(Hash = 6655224c7dd29d31cb98fecad1a98efac215eb93975534f62fc9051c7e4f2baf)]
UPDATE ShipTerms
SET FreightAmountSource = 'S'
WHERE FreightAmountSource IS NULL

UPDATE SOOrder
SET FreightAmountSource = 'S'
WHERE FreightAmountSource IS NULL

UPDATE SOShipment
SET FreightAmountSource = 'S'
WHERE FreightAmountSource IS NULL AND (OrderCntr > 0 OR ShipTermsID IS NOT NULL)
GO

--Populate reference to Sales Order in SOFreightDetail as it became a part of the PK
--[MinVersion(Hash = e6e199afdf4648423f961be03fad9bcf85a74976a3a04d0a499a9d9d3d496533)]
UPDATE fd
SET OrderType = COALESCE(os.OrderType, ''), OrderNbr = COALESCE(os.OrderNbr, '')
FROM SOFreightDetail fd
LEFT JOIN SOOrderShipment os
ON os.CompanyID = fd.CompanyID AND os.ShipmentType = fd.ShipmentType AND os.ShipmentNbr = fd.ShipmentNbr AND os.InvoiceType = fd.DocType AND os.InvoiceNbr = fd.RefNbr
WHERE fd.OrderType IS NULL OR fd.OrderNbr IS NULL

UPDATE tr
SET SOOrderType = fd.OrderType, SOOrderNbr = fd.OrderNbr
FROM ARTran tr
INNER JOIN SOFreightDetail fd
ON fd.CompanyID = tr.CompanyID AND fd.ShipmentType = tr.SOShipmentType AND fd.ShipmentNbr = tr.SOShipmentNbr AND fd.DocType = tr.TranType AND fd.RefNbr = tr.RefNbr
WHERE tr.LineType = 'FR' AND tr.Released = 0 AND (tr.SOOrderType IS NULL OR tr.SOOrderNbr IS NULL)
GO

--Populate the new flag SOOrderShipment.OrderFreightAllocated
--[IfExists(Column = SOOrderShipment.OrderFreightAllocated)]
--[MinVersion(Hash = 28e30f50cd1d47a32dba8aa65cdf25d6b64b5498ea07afd43f74ecd98ddb8d20)]
UPDATE os
SET OrderFreightAllocated = 1
FROM SOOrderShipment os
INNER JOIN SOSetup setup ON setup.CompanyID = os.CompanyID
INNER JOIN SOFreightDetail fd ON fd.CompanyID = os.CompanyID AND fd.ShipmentType = os.ShipmentType AND fd.ShipmentNbr = os.ShipmentNbr AND fd.OrderType = os.OrderType AND fd.OrderNbr = os.OrderNbr
WHERE setup.FreightAllocation = 'A' AND fd.CuryPremiumFreightAmt > 0 AND os.ShipmentNbr != '<NEW>' AND os.OrderFreightAllocated IS NULL

UPDATE SOOrderShipment
SET OrderFreightAllocated = 0
WHERE OrderFreightAllocated IS NULL
GO

--[IfExists(Column = CROpportunityRevision.LineCntr)]
--[MinVersion(Hash = f9813af15bd6558cb093d2bbf10edf39fd207e63f1a01b5e72508eddc4a6e06b)]
update CROpportunityRevision set LineCntr = 0 where LineCntr is null
GO

--[IfExistsSelect(From = ARInvoiceDiscountDetail, WhereIsNull = RecordID)]
--[IfExists(Column = ARInvoiceDiscountDetail.LineNbr)]
--[IfExists(Column = ARInvoiceDiscountDetail.RecordID)]
--[MinVersion(Hash = 1e4115d3c26fe0b525c3d6d03e5561117a5de146a4d196934793ebc7f29c83f3)]
IF
(	EXISTS(SELECT 1 FROM sys.columns WHERE object_id = object_id('ARInvoiceDiscountDetail') AND name = 'LineNbr') AND 
	EXISTS(SELECT 1 FROM sys.columns WHERE object_id = object_id('ARInvoiceDiscountDetail') AND name = 'RecordID' AND is_nullable = 1)
)
BEGIN
EXEC sp_executesql N'
update ARInvoiceDiscountDetail set
RecordID = LineNbr where RecordID is null'
END
GO

--[IfExistsSelect(From = APInvoiceDiscountDetail, WhereIsNull = RecordID)]
--[IfExists(Column = APInvoiceDiscountDetail.LineNbr)]
--[IfExists(Column = APInvoiceDiscountDetail.RecordID)]
--[MinVersion(Hash = afd7718d4d187289dde57a61e4a724101e93672d66129a2b1eafd0ddf38b86c8)]
IF
(	EXISTS(SELECT 1 FROM sys.columns WHERE object_id = object_id('APInvoiceDiscountDetail') AND name = 'LineNbr') AND 
	EXISTS(SELECT 1 FROM sys.columns WHERE object_id = object_id('APInvoiceDiscountDetail') AND name = 'RecordID' AND is_nullable = 1)
)
BEGIN
EXEC sp_executesql N'
update APInvoiceDiscountDetail set
RecordID = LineNbr where RecordID is null'
END
GO

--[IfExistsSelect(From = CROpportunityDiscountDetail, WhereIsNull = RecordID)]
--[IfExists(Column = CROpportunityDiscountDetail.LineNbr)]
--[IfExists(Column = CROpportunityDiscountDetail.RecordID)]
--[MinVersion(Hash = 9d5cbdae6657406ba0feaffda4f0486a2576d0ade37e3ecae3710fad0d2cb492)]
IF
(	EXISTS(SELECT 1 FROM sys.columns WHERE object_id = object_id('CROpportunityDiscountDetail') AND name = 'LineNbr') AND 
	EXISTS(SELECT 1 FROM sys.columns WHERE object_id = object_id('CROpportunityDiscountDetail') AND name = 'RecordID' AND is_nullable = 1)
)
BEGIN
EXEC sp_executesql N'
update CROpportunityDiscountDetail set
RecordID = LineNbr where RecordID is null'
END
GO

--[IfExistsSelect(From = POOrderDiscountDetail, WhereIsNull = RecordID)]
--[IfExists(Column = POOrderDiscountDetail.LineNbr)]
--[IfExists(Column = POOrderDiscountDetail.RecordID)]
--[MinVersion(Hash = 92b58e0a1fe6c3bb3c52126549889713191253414b6979897a3fa573aaaeb08e)]
IF
(	EXISTS(SELECT 1 FROM sys.columns WHERE object_id = object_id('POOrderDiscountDetail') AND name = 'LineNbr') AND 
	EXISTS(SELECT 1 FROM sys.columns WHERE object_id = object_id('POOrderDiscountDetail') AND name = 'RecordID' AND is_nullable = 1)
)
BEGIN
EXEC sp_executesql N'
update POOrderDiscountDetail set
RecordID = LineNbr where RecordID is null'
END
GO

--[IfExistsSelect(From = SOShipmentDiscountDetail, WhereIsNull = RecordID)]
--[IfExists(Column = SOShipmentDiscountDetail.LineNbr)]
--[IfExists(Column = SOShipmentDiscountDetail.RecordID)]
--[MinVersion(Hash = 10db5f34c5244ec219f5b7289f3727186473472e004a20948070a4641ed8af8b)]
IF
(	EXISTS(SELECT 1 FROM sys.columns WHERE object_id = object_id('SOShipmentDiscountDetail') AND name = 'LineNbr') AND 
	EXISTS(SELECT 1 FROM sys.columns WHERE object_id = object_id('SOShipmentDiscountDetail') AND name = 'RecordID' AND is_nullable = 1)
)
BEGIN
EXEC sp_executesql N'
update SOShipmentDiscountDetail set
RecordID = LineNbr where RecordID is null'
END
GO

--[IfExistsSelect(From = SOOrderDiscountDetail, WhereIsNull = RecordID)]
--[IfExists(Column = SOOrderDiscountDetail.LineNbr)]
--[IfExists(Column = SOOrderDiscountDetail.RecordID)]
--[MinVersion(Hash = a750568912caab08b75396ed086e9f6cd9adcb5dc1641e5bae91717621dc5dce)]
IF
(	EXISTS(SELECT 1 FROM sys.columns WHERE object_id = object_id('SOOrderDiscountDetail') AND name = 'LineNbr') AND 
	EXISTS(SELECT 1 FROM sys.columns WHERE object_id = object_id('SOOrderDiscountDetail') AND name = 'RecordID' AND is_nullable = 1)
)
BEGIN
EXEC sp_executesql N'
update SOOrderDiscountDetail set
RecordID = LineNbr where RecordID is null'
END
GO

--[MinVersion(Hash = 8058924cd2c80b87b3e5e494f69a2baa01e6cf012a99153109d2bc66c177ada5)]
update APSetup set VendorPriceUpdate = 'B' where VendorPriceUpdate = 'R' or VendorPriceUpdate = 'S'
GO

-- Generate new PrinterIDs
--[MinVersion(Hash = 6bd8c997ff7ad3bcb060326a2ca006c8ed5fffd6b858c48176684e67fe83371b)]
update SMPrinter set PrinterID = NEWID() where PrinterID is null
GO

-- Generate new ScaleDeviceIDs
--[MinVersion(Hash = e1614e29aa48e320b27be67e958b73b1a864da4b3abf33b3ae967967613e2dba)]
update SMScale set ScaleDeviceID = NEWID() where ScaleDeviceID is null
GO

-- Set default DeviceHubID
--[MinVersion(Hash = 52e141a540b6c038788c88f2de21f9af8857d012ccfd8928a75d6d791806ec29)]
update SMPrinter set DeviceHubID = 'DEFAULT' where DeviceHubID is null
GO

--[MinVersion(Hash = 1ba700f42cdc679b6bd93b936d05f9617c03b293b36609898af8c58fe8393d23)]
update SMPrintJob set DeviceHubID = 'DEFAULT' where DeviceHubID is null
GO

--[MinVersion(Hash = 0c9d461fee941701a94805a3c00ef0d3079cbc413d4d7220def74b5a14c75a9b)]
update SMScale set DeviceHubID = 'DEFAULT' where DeviceHubID is null
GO

-- Update default printers
--[IfExists(Column = Organization.DefaultPrinter)]
--[OldHash(Hash = cd7b1bb3d5ab216131f55ce3afd060b7990800509a5e36d274094663418da8d6)]
--[MinVersion(Hash = d33524aa972df8c2f66a9edad1f28eaa8435b4eabc5fd5dcdcada6e99536a870)]
update o
set o.DefaultPrinterID = p.PrinterID
from Organization o
inner join SMPrinter p on p.CompanyID = o.CompanyID and o.DefaultPrinter = p.PrinterName
where o.DefaultPrinterID is null
GO

--[IfExists(Column = Branch.DefaultPrinter)]
--[OldHash(Hash = ecc3239294bbd590888650ecd8e238abce3c1ce3b6b734de3d04478866e942d1)]
--[MinVersion(Hash = 98d254abc256c890710d48bb74a009d46ce2a03bddf15173f452db2572536502)]
update b
set b.DefaultPrinterID = p.PrinterID
from Branch b
inner join SMPrinter p on p.CompanyID = b.CompanyID and b.DefaultPrinter = p.PrinterName
where b.DefaultPrinterID is null
GO

--[IfExists(Column = NotificationSetup.DefaultPrinter)]
--[OldHash(Hash = 7305b7070bd5893f91c077de8d48267696d8b60f7c702984bf83b6bb2436aa55)]
--[MinVersion(Hash = 6888092b6b6f28a0ddb8aa2f65b652134ec11b79a44099a4ca28cb277181a75a)]
update n
set n.DefaultPrinterID = p.PrinterID
from NotificationSetup n
inner join SMPrinter p on p.CompanyID = n.CompanyID and n.DefaultPrinter = p.PrinterName
where n.DefaultPrinterID is null
GO

--[IfExists(Column = NotificationSetupUserOverride.DefaultPrinter)]
--[OldHash(Hash = ad65e691d7346251d6f2b20511e845effb0599b165073c5dc5282c6fcf38f841)]
--[MinVersion(Hash = 6377402c9dbc3fff3bbbab60d7cffd1ce9440ff62498df32ce2bda851402438c)]
update n
set n.DefaultPrinterID = p.PrinterID
from NotificationSetupUserOverride n
inner join SMPrinter p on p.CompanyID = n.CompanyID and n.DefaultPrinter = p.PrinterName
where n.DefaultPrinterID is null
GO

--[IfExists(Column = UserPreferences.DefaultPrinter)]
--[OldHash(Hash = 490b60da169bc8502d7669e0fdf596702f4c732cc69f7466e1346a3abf8a0324)]
--[MinVersion(Hash = 7049075d7c180dad4610be304e1b4c911bf5a747c0e7dc4bccd8a41218c5f334)]
update u
set u.DefaultPrinterID = p.PrinterID
from UserPreferences u
inner join SMPrinter p on p.CompanyID = u.CompanyID and u.DefaultPrinter = p.PrinterName
where u.DefaultPrinterID is null
GO

--[IfExists(Column = SOQuickProcessParameters.PrinterName)]
--[OldHash(Hash = a9c7be9e94c9fdd9f0762378831416c6fe56eb7036d9c020f41beae5db26abc6)]
--[MinVersion(Hash = ab506a9424017892b101ec425a5121de7c55d60253a1c5ce896bd38a224dc7d8)]
update s
set s.PrinterID = p.PrinterID
from SOQuickProcessParameters s
inner join SMPrinter p on p.CompanyID = s.CompanyID and s.PrinterName = p.PrinterName
where s.PrinterID is null
GO

--Set ScaleDeviceID
--[IfExists(Column = UserPreferences.DefaultScales)]
--[OldHash(Hash = 08571cc00ca24f83a25e27d117f50bb6e4aa353c4ebcf9398bd94f97ffc4fd73)]
--[MinVersion(Hash = 4d2b4290a1f0dce758702e07e028135422714d1723240165927d52e16f94325a)]
update u
set u.DefaultScalesID = p.ScaleID
from UserPreferences u
inner join SMScale p on p.CompanyID = u.CompanyID and u.DefaultScales = p.ScaleID
where u.DefaultScalesID is null
GO

--[IfExists(Column = POReceivePutAwayUserSetup.ScaleID)]
--[OldHash(Hash = 9931d3834feb008bff1b7efb717ba9de3cf869d48994e4977ccc78b5d283f243)]
--[MinVersion(Hash = eddacb2ad5a6dce92da3627324f1a77750beaf02c78c71409a917f133dac9dd6)]
update o
set o.ScaleDeviceID = p.ScaleDeviceID
from POReceivePutAwayUserSetup o
inner join SMScale p on p.CompanyID = o.CompanyID and o.ScaleID = p.ScaleID
where o.ScaleDeviceID is null
GO

--[IfExists(Column = SOPickPackShipUserSetup.ScaleID)]
--[OldHash(Hash = e78baac9488d85275782e74579a422cb684f1487cd659f1d86942c6fe5774415)]
--[MinVersion(Hash = 1b69e1e6a8a8331ec3e76f3780eb0ffeb57c699c4aa12c0c5a5da809bf42859f)]
update o
set o.ScaleDeviceID = p.ScaleDeviceID
from SOPickPackShipUserSetup o
inner join SMScale p on p.CompanyID = o.CompanyID and o.ScaleID = p.ScaleID
where o.ScaleDeviceID is null
GO

--[IfExists(Column = INScanUserSetup.ScaleID)]
--[OldHash(Hash = 565da7dbe8d6a1f6992ee15480f43e36f859a7049152b36ee08e88604cb92bae)]
--[MinVersion(Hash = 1b2680058cae88643d3e5b90f1f11e6f8ed9923dd8291093dad63849dd8195ea)]
update o
set o.ScaleDeviceID = p.ScaleDeviceID
from INScanUserSetup o
inner join SMScale p on p.CompanyID = o.CompanyID and o.ScaleID = p.ScaleID
where o.ScaleDeviceID is null
GO

-- rename warehouse duplicates so that unique index by SiteCD can be added to INSite table
-- for duplicated (CompanyID, SiteCD) SiteCD = SiteCD+"_"+hash(CompanyID, SiteID)
--[OldHash(Hash = b0bf2d40e3657ecd8f126aadb9865573ed53b19499b2bb61fed37afb73e0ee90)]
--[OldHash(Hash = ad0b74f0926060c2342dc4e35e594492bfbaa2f8efe3f489aea4261672449c45)]
--[MinVersion(Hash = 6f52b14ba7903f0cac63379aa297fa1ed0310b8c9378ec8f72527c2d042a308b)]
UPDATE INSite 
SET SiteCD = rtrim(SiteCD)+ '+' + cast((SiteID % 7879) as nvarchar(10))
WHERE 
SiteID NOT IN
(
	SELECT MIN(S1.SiteID) 
	FROM (select CompanyID, SiteID, SiteCD from INSite) as S1
	WHERE S1.CompanyID = CompanyID
	GROUP BY S1.CompanyID, S1.SiteCD 
	HAVING count(*) > 1
) AND
SiteID IN
(
	SELECT G1.SiteID
	FROM (select CompanyID, SiteID, SiteCD from INSite) AS G1
	INNER JOIN (select CompanyID, SiteID, SiteCD from INSite) AS G2
	ON G1.SiteCD = G2.SiteCD AND G1.CompanyID = G2.CompanyID
		AND G1.SiteID <> G2.SiteID
) AND EXISTS
(
	SELECT G1.SiteID
	FROM (select CompanyID as CID, SiteID, SiteCD from INSite) AS G1
	INNER JOIN (select CompanyID as CID, SiteID, SiteCD from INSite) AS G2
	ON G1.SiteCD = G2.SiteCD AND G1.CID = G2.CID
		AND G1.SiteID <> G2.SiteID
	WHERE G1.CID = CompanyID
);
GO

--[MinVersion(Hash = 1597b2ed31c5948a5b2f1eab3ca64a2391a2ad7c036a980b11cd15ca6e9ebccd)]
update ics set SiteID = ins.siteid from INCostStatus ics
join insite ins on ics.companyid = ins.companyid and ics.costsiteid = ins.siteid
where ics.siteid is null
GO

--[MinVersion(Hash = 942b37eff2a43761bee17dd92bf57b5e6bdf4c1da338907117fcfbf116583bae)]
update ics set SiteID = inl.siteid from INCostStatus ics
join INLocation inl on ics.companyid = inl.companyid and ics.costsiteid = inl.locationid
where ics.siteid is null
GO

--Correct RefNoteID for SO Shipped plans reattached to IN Issue
--[OldHash(Hash = 4765ed16520535d545a624dff15102e8d724c83546db3503ae942d6393421f5d)]
--[MinVersion(Hash = ff9491e00511d565d25d5d6cd43844dd0a424d47ea8ae926d68a918bd4d69f2d)]
UPDATE ip
SET RefNoteID = ir.NoteID
FROM INItemPlan ip
INNER JOIN SOOrder o ON o.CompanyID = ip.CompanyID AND o.NoteID = ip.RefNoteID
LEFT JOIN SOLineSplit s ON s.CompanyID = ip.CompanyID AND s.PlanID = ip.PlanID
INNER JOIN INTranSplit its ON its.CompanyID = ip.CompanyID AND its.PlanID = ip.PlanID
INNER JOIN INRegister ir ON ir.CompanyID = its.CompanyID AND ir.DocType = its.DocType AND ir.RefNbr = its.RefNbr
WHERE s.OrderNbr IS NULL AND ip.PlanType = '62'

UPDATE ip
SET RefNoteID = ir.NoteID
FROM INItemPlan ip
INNER JOIN SOShipment o ON o.CompanyID = ip.CompanyID AND o.NoteID = ip.RefNoteID
LEFT JOIN SOShipLineSplit s ON s.CompanyID = ip.CompanyID AND s.PlanID = ip.PlanID
INNER JOIN INTranSplit its ON its.CompanyID = ip.CompanyID AND its.PlanID = ip.PlanID
INNER JOIN INRegister ir ON ir.CompanyID = its.CompanyID AND ir.DocType = its.DocType AND ir.RefNbr = its.RefNbr
WHERE s.ShipmentNbr IS NULL AND ip.PlanType = '62'
GO

--Delete duplicate records in PMAccountTask table:
--[IfExists(Column = PMAccountTask.RecordID)]
--[MinVersion(Hash = 43774a56dfd60be53808dea9a9069b5dd163d7d4a23cbf2ae10728a68d5388ea)]
DELETE dups FROM PMAccountTask dups
LEFT OUTER JOIN ( 
    SELECT CompanyID, MIN(RecordID) as RowID, ProjectID, AccountID FROM PMAccountTask 
    GROUP BY CompanyID, ProjectID, AccountID 
) AS KeepRows ON KeepRows.CompanyID = dups.CompanyID AND KeepRows.RowID = dups.RecordID 
WHERE KeepRows.RowID IS NULL
GO

--[mysql: Skip]
--[MinVersion(Hash = 2290e9342e39c9d3e669de1452a9e617840d27d95ae8fb9d71586e282332c21a)]
IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = OBJECT_ID(N'Branch_BranchID__FSAppointmentInventoryItem_BranchID_2'))
	DROP TRIGGER Branch_BranchID__FSAppointmentInventoryItem_BranchID_2
GO

--[MinVersion(Hash = 50ec4417af659ea661c0aafdf277c0fb83628dd250550b5737b751507ff1ff2b)]
IF (EXISTS(SELECT 1 FROM sys.columns WHERE object_id = object_id('Branch') AND name = 'FileTaxesByBranches'))
BEGIN
EXEC sp_executesql N'
UPDATE Branch
	SET FileTaxesByBranches = 1
FROM Branch 
	INNER JOIN Ledger
		ON Branch.LedgerID = Ledger.LedgerID
	LEFT JOIN TaxHistory
		ON Branch.BranchID = TaxHistory.BranchID
WHERE 
	Branch.BranchID <> Ledger.DefBranchID
	OR (Ledger.PostInterCompany = 1
		AND TaxHistory.BranchID is null)'
END
GO

--[MinVersion(Hash = 3780952b3f0014c847ac7f83e7e28d1bac945f546a3ce37da244eb033f31fd99)]
UPDATE TaxReportLine
SET SortOrder = LineNbr
WHERE SortOrder IS NULL
AND TempLineNbr IS NULL

UPDATE detailTaxReportLines
SET detailTaxReportLines.SortOrder = templateTaxReportLines.SortOrder
FROM TaxReportLine detailTaxReportLines
INNER JOIN TaxReportLine templateTaxReportLines 
ON templateTaxReportLines.CompanyID = detailTaxReportLines.CompanyID 
AND templateTaxReportLines.VendorID = detailTaxReportLines.VendorID
AND templateTaxReportLines.LineNbr = detailTaxReportLines.TempLineNbr
WHERE templateTaxReportLines.SortOrder IS NOT NULL
  AND detailTaxReportLines.SortOrder IS NULL
  AND detailTaxReportLines.TempLineNbr IS NOT NULL
GO

--[MinVersion(Hash = 5f185fc2f95cc3a157a45647f8ce0ed8d921ff419f3f2f94bc0da1c457fd82fc)]
UPDATE FSSODet SET
    GroupDiscountRate    = COALESCE(GroupDiscountRate,1),
    DocumentDiscountRate = COALESCE(DocumentDiscountRate,1)
WHERE
	GroupDiscountRate IS NULL
    OR DocumentDiscountRate IS NULL
GO

--[MinVersion(Hash = 37e7b63a62d51adefe695ae5226633a46d6f93f6e3601468fb3ff079d50a85d1)]
UPDATE FSAppointmentDet SET
    GroupDiscountRate    = COALESCE(GroupDiscountRate,1),
    DocumentDiscountRate = COALESCE(DocumentDiscountRate,1)
WHERE
	GroupDiscountRate IS NULL
    OR DocumentDiscountRate IS NULL
GO

--[MinVersion(Hash = d7242d0977b545d70b50c3e9717a09e042950af436dea7af3f3274cd4c8054a0)]
DELETE CATran 
FROM CATran 
	INNER JOIN APPayment 
		ON CATran.CompanyID = APPayment.CompanyID 
		AND CATran.TranID = APPayment.CATranID
		AND CATran.CashAccountID = APPayment.CashAccountID
	INNER JOIN APRegister 
		ON APRegister.CompanyID = APPayment.CompanyID 
		AND APRegister.DocType = APPayment.DocType
		AND APRegister.RefNbr = APPayment.RefNbr
WHERE 
	APRegister.IsMigratedRecord = 1
GO

--[MinVersion(Hash = 934adde5028213b51b2f6cbfd6132960f2f01cd34dfbf0d86da7a8078b6903ce)]
DELETE CATran 
FROM CATran
	INNER JOIN ARPayment 
		ON CATran.CompanyID = ARPayment.CompanyID
		AND CATran.TranID = ARPayment.CATranID
		AND CATran.CashAccountID = ARPayment.CashAccountID
	INNER JOIN ARRegister 
		ON ARRegister.CompanyID = ARPayment.CompanyID
		AND ARRegister.DocType = ARPayment.DocType
		AND ARRegister.RefNbr = ARPayment.RefNbr
WHERE 
	ARRegister.IsMigratedRecord = 1
GO

--AC-1035555 Move Avalara settings to Tax Plugin settings
--[IfExists(Table = TXAvalaraSetup)]
--[OldHash(Hash = 94f0fbbaaa50e4967ff517e600974d35df1dae1353761604873c34578cf1c61d)]
--[MinVersion(Hash = 1be7eb8505d6fdf02ab7f9df2ef84e9ebfcbfbb3a735437ab3b91d75e1713fcc)]
INSERT INTO [TaxPlugin] ([CompanyID], [TaxPluginID], [Description], [PluginTypeName], [IsActive], [CreatedByID], [CreatedByScreenID], [CreatedDateTime], [LastModifiedByID], [LastModifiedByScreenID], [LastModifiedDateTime], [NoteID])
SELECT [CompanyID], 'AVALARA' AS [TaxPluginID], 'Avalara Tax Provider' AS [Description], 'PX.TaxProvider.AvalaraSoap.AvalaraSoapTaxProvider' AS [PluginTypeName], [IsActive], [CreatedByID], [CreatedByScreenID], [CreatedDateTime], [LastModifiedByID], [LastModifiedByScreenID], [LastModifiedDateTime], NEWID() 
FROM [TXAvalaraSetup]

INSERT INTO [TaxPluginDetail] ([CompanyID], [TaxPluginID], [SettingID], [SortOrder], [Description], [Value], [ControlTypeValue], [ComboValuesStr], [CreatedByID], [CreatedByScreenID], [CreatedDateTime], [LastModifiedByID], [LastModifiedByScreenID], [LastModifiedDateTime])
SELECT [CompanyID], 'AVALARA' AS [TaxPluginID], 'ACCOUNTID' AS [SettingID], 1 AS [SortOrder], 'Account Number' AS [DESCRIPTION], [Account] AS [VALUE], 1 AS [CONTROLTYPEVALUE], '' AS [COMBOVALUESSTR], [CreatedByID], [CreatedByScreenID], [CreatedDateTime], [LastModifiedByID], [LastModifiedByScreenID], [LastModifiedDateTime]
FROM [TXAvalaraSetup]

INSERT INTO [TaxPluginDetail] ([CompanyID], [TaxPluginID], [SettingID], [SortOrder], [Description], [Value], [ControlTypeValue], [ComboValuesStr], [CreatedByID], [CreatedByScreenID], [CreatedDateTime], [LastModifiedByID], [LastModifiedByScreenID], [LastModifiedDateTime])
SELECT [CompanyID], 'AVALARA' AS [TaxPluginID], 'LICENCEKEY' AS [SettingID], 2 AS [SortOrder], 'Licence Key' AS [DESCRIPTION], [Licence] AS [VALUE], 1 AS [CONTROLTYPEVALUE], '' AS [COMBOVALUESSTR], [CreatedByID], [CreatedByScreenID], [CreatedDateTime], [LastModifiedByID], [LastModifiedByScreenID], [LastModifiedDateTime]
FROM [TXAvalaraSetup]

INSERT INTO [TaxPluginDetail] ([CompanyID], [TaxPluginID], [SettingID], [SortOrder], [Description], [Value], [ControlTypeValue], [ComboValuesStr], [CreatedByID], [CreatedByScreenID], [CreatedDateTime], [LastModifiedByID], [LastModifiedByScreenID], [LastModifiedDateTime])
SELECT [CompanyID], 'AVALARA' AS [TaxPluginID], 'ENDPOINT' AS [SettingID], 3 AS [SortOrder], 'URL' AS [DESCRIPTION], [Url] AS [VALUE], 1 AS [CONTROLTYPEVALUE], '' AS [COMBOVALUESSTR], [CreatedByID], [CreatedByScreenID], [CreatedDateTime], [LastModifiedByID], [LastModifiedByScreenID], [LastModifiedDateTime]
FROM [TXAvalaraSetup]

INSERT INTO [TaxPluginDetail] ([CompanyID], [TaxPluginID], [SettingID], [SortOrder], [Description], [Value], [ControlTypeValue], [ComboValuesStr], [CreatedByID], [CreatedByScreenID], [CreatedDateTime], [LastModifiedByID], [LastModifiedByScreenID], [LastModifiedDateTime])
SELECT [CompanyID], 'AVALARA' AS [TaxPluginID], 'TIMEOUT' AS [SettingID], 4 AS [SortOrder], 'Request Timeout (sec)' AS [DESCRIPTION], CAST([Timeout] AS NVARCHAR) AS [VALUE], 1 AS [CONTROLTYPEVALUE], '' AS [COMBOVALUESSTR], [CreatedByID], [CreatedByScreenID], [CreatedDateTime], [LastModifiedByID], [LastModifiedByScreenID], [LastModifiedDateTime]
FROM [TXAvalaraSetup]

INSERT INTO [TaxPluginDetail] ([CompanyID], [TaxPluginID], [SettingID], [SortOrder], [Description], [Value], [ControlTypeValue], [ComboValuesStr], [CreatedByID], [CreatedByScreenID], [CreatedDateTime], [LastModifiedByID], [LastModifiedByScreenID], [LastModifiedDateTime])
SELECT [CompanyID], 'AVALARA' AS [TaxPluginID], 'SENDACC' AS [SettingID], 5 AS [SortOrder], 'Send Sales Account' AS [DESCRIPTION], CASE WHEN [SendRevenueAccount] = 1 THEN 'True' ELSE 'False' END AS [VALUE], 3 AS [CONTROLTYPEVALUE], '' AS [COMBOVALUESSTR], [CreatedByID], [CreatedByScreenID], [CreatedDateTime], [LastModifiedByID], [LastModifiedByScreenID], [LastModifiedDateTime]
FROM [TXAvalaraSetup]

INSERT INTO [TaxPluginDetail] ([CompanyID], [TaxPluginID], [SettingID], [SortOrder], [Description], [Value], [ControlTypeValue], [ComboValuesStr], [CreatedByID], [CreatedByScreenID], [CreatedDateTime], [LastModifiedByID], [LastModifiedByScreenID], [LastModifiedDateTime])
SELECT [CompanyID], 'AVALARA' AS [TaxPluginID], 'ADDRCHECK' AS [SettingID], 6 AS [SortOrder], 'Check Address Before Calculating Tax' AS [DESCRIPTION],CASE WHEN [AlwaysCheckAddress] = 1 THEN 'True' ELSE 'False' END AS [VALUE], 3 AS [CONTROLTYPEVALUE], '' AS [COMBOVALUESSTR], [CreatedByID], [CreatedByScreenID], [CreatedDateTime], [LastModifiedByID], [LastModifiedByScreenID], [LastModifiedDateTime]
FROM [TXAvalaraSetup]

INSERT INTO [TaxPluginDetail] ([CompanyID], [TaxPluginID], [SettingID], [SortOrder], [Description], [Value], [ControlTypeValue], [ComboValuesStr], [CreatedByID], [CreatedByScreenID], [CreatedDateTime], [LastModifiedByID], [LastModifiedByScreenID], [LastModifiedDateTime])
SELECT [CompanyID], 'AVALARA' AS [TaxPluginID], 'TAXINCL' AS [SettingID], 7 AS [SortOrder], 'Inclusive Tax' AS [DESCRIPTION], CASE WHEN [IsInclusiveTax] = 1 THEN 'True' ELSE 'False' END AS [VALUE], 3 AS [CONTROLTYPEVALUE], '' AS [COMBOVALUESSTR], [CreatedByID], [CreatedByScreenID], [CreatedDateTime], [LastModifiedByID], [LastModifiedByScreenID], [LastModifiedDateTime]
FROM [TXAvalaraSetup]

INSERT INTO [TaxPluginDetail] ([CompanyID], [TaxPluginID], [SettingID], [SortOrder], [Description], [Value], [ControlTypeValue], [ComboValuesStr], [CreatedByID], [CreatedByScreenID], [CreatedDateTime], [LastModifiedByID], [LastModifiedByScreenID], [LastModifiedDateTime])
SELECT [CompanyID], 'AVALARA' AS [TaxPluginID], 'ADDRESSINUPPER' AS [SettingID], 1000 AS [SortOrder], 'Address In Uppercase' AS [DESCRIPTION], CASE WHEN [AddressInUppercase] = 1 THEN 'True' ELSE 'False' END AS [VALUE], 3 AS [CONTROLTYPEVALUE], '' AS [COMBOVALUESSTR], [CreatedByID], [CreatedByScreenID], [CreatedDateTime], [LastModifiedByID], [LastModifiedByScreenID], [LastModifiedDateTime]
FROM [TXAvalaraSetup]

INSERT INTO [TaxPluginDetail] ([CompanyID], [TaxPluginID], [SettingID], [SortOrder], [Description], [Value], [ControlTypeValue], [ComboValuesStr], [CreatedByID], [CreatedByScreenID], [CreatedDateTime], [LastModifiedByID], [LastModifiedByScreenID], [LastModifiedDateTime])
SELECT [CompanyID], 'AVALARA' AS [TaxPluginID], 'DISABLETAXCALC' AS [SettingID], 1001 AS [SortOrder], 'Disable Tax Calculation' AS [DESCRIPTION], CASE WHEN [DisableTaxCalculation] = 1 THEN 'True' ELSE 'False' END AS [VALUE], 3 AS [CONTROLTYPEVALUE], '' AS [COMBOVALUESSTR], [CreatedByID], [CreatedByScreenID], [CreatedDateTime], [LastModifiedByID], [LastModifiedByScreenID], [LastModifiedDateTime]
FROM [TXAvalaraSetup]

INSERT INTO [TaxPluginDetail] ([CompanyID], [TaxPluginID], [SettingID], [SortOrder], [Description], [Value], [ControlTypeValue], [ComboValuesStr], [CreatedByID], [CreatedByScreenID], [CreatedDateTime], [LastModifiedByID], [LastModifiedByScreenID], [LastModifiedDateTime])
SELECT [CompanyID], 'AVALARA' AS [TaxPluginID], 'DISABLEADDRVAL' AS [SettingID], 1002 AS [SortOrder], 'Disable Address Validation' AS [DESCRIPTION], CASE WHEN [DisableAddressValidation] = 1 THEN 'True' ELSE 'False' END AS [VALUE], 3 AS [CONTROLTYPEVALUE], '' AS [COMBOVALUESSTR], [CreatedByID], [CreatedByScreenID], [CreatedDateTime], [LastModifiedByID], [LastModifiedByScreenID], [LastModifiedDateTime]
FROM [TXAvalaraSetup]

INSERT INTO [TaxPluginDetail] ([CompanyID], [TaxPluginID], [SettingID], [SortOrder], [Description], [Value], [ControlTypeValue], [ComboValuesStr], [CreatedByID], [CreatedByScreenID], [CreatedDateTime], [LastModifiedByID], [LastModifiedByScreenID], [LastModifiedDateTime])
SELECT [CompanyID], 'AVALARA' AS [TaxPluginID], 'ENABLELOGGING' AS [SettingID], 1003 AS [SortOrder], 'Enable Logging' AS [DESCRIPTION], CASE WHEN [EnableLogging] = 1 THEN 'True' ELSE 'False' END AS [VALUE], 3 AS [CONTROLTYPEVALUE], '' AS [COMBOVALUESSTR], [CreatedByID], [CreatedByScreenID], [CreatedDateTime], [LastModifiedByID], [LastModifiedByScreenID], [LastModifiedDateTime]
FROM [TXAvalaraSetup]

INSERT INTO [TaxPluginDetail] ([CompanyID], [TaxPluginID], [SettingID], [SortOrder], [Description], [Value], [ControlTypeValue], [ComboValuesStr], [CreatedByID], [CreatedByScreenID], [CreatedDateTime], [LastModifiedByID], [LastModifiedByScreenID], [LastModifiedDateTime])
SELECT [CompanyID], 'AVALARA' AS [TaxPluginID], 'SHOWALLWARN' AS [SettingID], 1004 AS [SortOrder], 'Show All Warnings' AS [DESCRIPTION], CASE WHEN [ShowAllWarnings] = 1 THEN 'True' ELSE 'False' END AS [VALUE], 3 AS [CONTROLTYPEVALUE], '' AS [COMBOVALUESSTR], [CreatedByID], [CreatedByScreenID], [CreatedDateTime], [LastModifiedByID], [LastModifiedByScreenID], [LastModifiedDateTime]
FROM [TXAvalaraSetup]

INSERT INTO [TaxPluginDetail] ([CompanyID], [TaxPluginID], [SettingID], [SortOrder], [Description], [Value], [ControlTypeValue], [ComboValuesStr], [CreatedByID], [CreatedByScreenID], [CreatedDateTime], [LastModifiedByID], [LastModifiedByScreenID], [LastModifiedDateTime])
SELECT [CompanyID], 'AVALARA' AS [TaxPluginID], 'SHOWTAXDETAILS' AS [SettingID], 1005 AS [SortOrder], 'Show Tax Details' AS [DESCRIPTION], CASE WHEN [ShowTaxDetails] = 1 THEN 'True' ELSE 'False' END AS [VALUE], 3 AS [CONTROLTYPEVALUE], '' AS [COMBOVALUESSTR], [CreatedByID], [CreatedByScreenID], [CreatedDateTime], [LastModifiedByID], [LastModifiedByScreenID], [LastModifiedDateTime]
FROM [TXAvalaraSetup]

INSERT INTO [TaxPluginMapping] ([CompanyID], [TaxPluginID], [BranchID], [CompanyCode], [CreatedByID], [CreatedByScreenID], [CreatedDateTime], [LastModifiedByID], [LastModifiedByScreenID], [LastModifiedDateTime])
SELECT [CompanyID], 'AVALARA' AS [TaxPluginID], [BranchID], [CompanyCode], [CreatedByID], [CreatedByScreenID], [CreatedDateTime], [LastModifiedByID], [LastModifiedByScreenID], [LastModifiedDateTime]
FROM [TXAvalaraMapping]

UPDATE [TaxZone] SET [TaxPluginID] = 'AVALARA' WHERE [IsExternal]= 1
GO

--AC-105111 Create Address Validation settings
--[IfExists(Column = Country.AddressVerificationTypeName)]
--[IfExists(Table = TXAvalaraSetup)]
--[MinVersion(Hash = 3ffffed7cda4cf609f7df0c4b7bac14c997220372d6426f675bf3627fa82e526)]
UPDATE [Country]
SET [AddressValidatorPluginID] = 'AVALARA'
WHERE [AddressVerificationTypeName] = 'PX.Objects.CS.AvalaraAddressValidator'
GO

--AC-105111 Create Address Validation settings
--[IfExists(Column = Country.AddressVerificationTypeName)]
--[IfExists(Table = TXAvalaraSetup)]
--[mysql: Skip]
--[MinVersion(Hash = cc54a7b92f831a359a0499223007b60082ebd1d542ba9039b4be41af4f417957)]
UPDATE [Country]
SET [AddressValidatorPluginID] = UPPER(SUBSTRING([AddressVerificationTypeName], CASE
  WHEN CHARINDEX('.', [AddressVerificationTypeName]) > 0 THEN LEN([AddressVerificationTypeName]) + 2 - CHARINDEX('.', REVERSE([AddressVerificationTypeName]))
  ELSE 0
END, 15))
WHERE [AddressVerificationTypeName] <> ''
AND [AddressValidatorPluginID] is null
GO

--AC-105111 Create Address Validation settings
--[IfExists(Column = Country.AddressVerificationTypeName)]
--[IfExists(Table = TXAvalaraSetup)]
--[mysql: Native]
--[mssql: Skip]
--[MinVersion(Hash = 92a7a89de0cd64c773bc148ed676fe634c19e983c01c0a9259df4e937c80ba4f)]
UPDATE `Country`
SET `AddressValidatorPluginID` = SUBSTRING_INDEX(`AddressVerificationTypeName`, '.', -1)
WHERE `AddressVerificationTypeName` <> ''
AND `AddressValidatorPluginID` is null
GO

--AC-105111 Create Address Validation settings
--[IfExists(Column = Country.AddressVerificationTypeName)]
--[IfExists(Table = TXAvalaraSetup)]
--[OldHash(Hash = 98c68c2db720b6a9f26a803d5ef55a07427db6a4a7410935e0478190c3b8c0fb)]
--[MinVersion(Hash = 9700971111c4cc06d71fa25f0b7b5a1df7d607aa0719226a294aaeaf56845ed7)]
INSERT INTO [AddressValidatorPlugin] ([CompanyID]
, [AddressValidatorPluginID]
, [Description]
, [PluginTypeName]
, [IsActive]
, [CreatedByID]
, [CreatedByScreenID]
, [CreatedDateTime]
, [LastModifiedByID]
, [LastModifiedByScreenID]
, [LastModifiedDateTime]
, [NoteID])
  SELECT [CompanyID], [AddressValidatorPluginID], CASE WHEN [AddressValidatorPluginID] = 'AVALARA' THEN 'Avalara Address Validator' ELSE MAX([AddressVerificationTypeName]) END AS [Description], CASE WHEN [AddressValidatorPluginID] = 'AVALARA' THEN 'PX.AddressValidator.AvalaraSoap.AvalaraSoapAddressValidator' ELSE MAX([AddressVerificationTypeName]) END AS [PluginTypeName], CASE WHEN [AddressValidatorPluginID] = 'AVALARA' THEN ISNULL((SELECT TOP 1 [IsActive] FROM [TXAvalaraSetup]), 0) ELSE 0 END AS [IsActive], MAX([CreatedByID]) AS [CreatedByID], 'CS103000' AS [CreatedByScreenID], GETDATE() AS [CreatedDateTime], MAX([LastModifiedByID]) AS [LastModifiedByID], 'CS103000' AS [LastModifiedByScreenID], GETDATE() AS [LastModifiedDateTime], NEWID() AS [NoteID]
  FROM [Country]
  WHERE [AddressVerificationTypeName] <> ''
  GROUP BY [CompanyID]
          ,[AddressValidatorPluginID]

INSERT INTO [AddressValidatorPluginDetail] ([CompanyID], [AddressValidatorPluginID], [SettingID], [SortOrder], [Description], [Value], [ControlTypeValue], [ComboValuesStr], [CreatedByID], [CreatedByScreenID], [CreatedDateTime], [LastModifiedByID], [LastModifiedByScreenID], [LastModifiedDateTime])
  SELECT [P].[CompanyID], 'AVALARA' AS [AddressValidatorPluginID], 'ACCOUNTID' AS [SettingID], 1 AS [SortOrder], 'Account Number' AS [DESCRIPTION], [Account] AS [VALUE], 1 AS [CONTROLTYPEVALUE], '' AS [COMBOVALUESSTR], [P].[CreatedByID], [P].[CreatedByScreenID], [P].[CreatedDateTime], [P].[LastModifiedByID], [P].[LastModifiedByScreenID], [P].[LastModifiedDateTime]
  FROM [TXAvalaraSetup]
  INNER JOIN [AddressValidatorPlugin] [P]
    ON [TXAvalaraSetup].[CompanyID] = [P].[CompanyID]
      AND [P].[AddressValidatorPluginID] = 'AVALARA'

INSERT INTO [AddressValidatorPluginDetail] ([CompanyID], [AddressValidatorPluginID], [SettingID], [SortOrder], [Description], [Value], [ControlTypeValue], [ComboValuesStr], [CreatedByID], [CreatedByScreenID], [CreatedDateTime], [LastModifiedByID], [LastModifiedByScreenID], [LastModifiedDateTime])
  SELECT [P].[CompanyID], 'AVALARA' AS [AddressValidatorPluginID], 'LICENCEKEY' AS [SettingID], 2 AS [SortOrder], 'Licence Key' AS [DESCRIPTION], [Licence] AS [VALUE], 1 AS [CONTROLTYPEVALUE], '' AS [COMBOVALUESSTR], [P].[CreatedByID], [P].[CreatedByScreenID], [P].[CreatedDateTime], [P].[LastModifiedByID], [P].[LastModifiedByScreenID], [P].[LastModifiedDateTime]
  FROM [TXAvalaraSetup]
  INNER JOIN [AddressValidatorPlugin] [P]
    ON [TXAvalaraSetup].[CompanyID] = [P].[CompanyID]
      AND [P].[AddressValidatorPluginID] = 'AVALARA'

INSERT INTO [AddressValidatorPluginDetail] ([CompanyID], [AddressValidatorPluginID], [SettingID], [SortOrder], [Description], [Value], [ControlTypeValue], [ComboValuesStr], [CreatedByID], [CreatedByScreenID], [CreatedDateTime], [LastModifiedByID], [LastModifiedByScreenID], [LastModifiedDateTime])
  SELECT [P].[CompanyID], 'AVALARA' AS [AddressValidatorPluginID], 'ENDPOINT' AS [SettingID], 3 AS [SortOrder], 'URL' AS [DESCRIPTION], [Url] AS [VALUE], 1 AS [CONTROLTYPEVALUE], '' AS [COMBOVALUESSTR], [P].[CreatedByID], [P].[CreatedByScreenID], [P].[CreatedDateTime], [P].[LastModifiedByID], [P].[LastModifiedByScreenID], [P].[LastModifiedDateTime]
  FROM [TXAvalaraSetup]
  INNER JOIN [AddressValidatorPlugin] [P]
    ON [TXAvalaraSetup].[CompanyID] = [P].[CompanyID]
      AND [P].[AddressValidatorPluginID] = 'AVALARA'

INSERT INTO [AddressValidatorPluginDetail] ([CompanyID], [AddressValidatorPluginID], [SettingID], [SortOrder], [Description], [Value], [ControlTypeValue], [ComboValuesStr], [CreatedByID], [CreatedByScreenID], [CreatedDateTime], [LastModifiedByID], [LastModifiedByScreenID], [LastModifiedDateTime])
  SELECT [P].[CompanyID], 'AVALARA' AS [AddressValidatorPluginID], 'TIMEOUT' AS [SettingID], 4 AS [SortOrder], 'Request Timeout (sec)' AS [DESCRIPTION], CAST([Timeout] AS NVARCHAR) AS [VALUE], 1 AS [CONTROLTYPEVALUE], '' AS [COMBOVALUESSTR], [P].[CreatedByID], [P].[CreatedByScreenID], [P].[CreatedDateTime], [P].[LastModifiedByID], [P].[LastModifiedByScreenID], [P].[LastModifiedDateTime]
  FROM [TXAvalaraSetup]
  INNER JOIN [AddressValidatorPlugin] [P]
    ON [TXAvalaraSetup].[CompanyID] = [P].[CompanyID]
      AND [P].[AddressValidatorPluginID] = 'AVALARA'

INSERT INTO [AddressValidatorPluginDetail] ([CompanyID], [AddressValidatorPluginID], [SettingID], [SortOrder], [Description], [Value], [ControlTypeValue], [ComboValuesStr], [CreatedByID], [CreatedByScreenID], [CreatedDateTime], [LastModifiedByID], [LastModifiedByScreenID], [LastModifiedDateTime])
  SELECT [P].[CompanyID], 'AVALARA' AS [AddressValidatorPluginID], 'ADDRESSINUPPER' AS [SettingID], 1000 AS [SortOrder], 'Address In Uppercase' AS [DESCRIPTION], CASE WHEN [AddressInUppercase] = 1 THEN 'True' ELSE 'False' END AS [VALUE], 1 AS [CONTROLTYPEVALUE], '' AS [COMBOVALUESSTR], [P].[CreatedByID], [P].[CreatedByScreenID], [P].[CreatedDateTime], [P].[LastModifiedByID], [P].[LastModifiedByScreenID], [P].[LastModifiedDateTime]
  FROM [TXAvalaraSetup]
  INNER JOIN [AddressValidatorPlugin] [P]
    ON [TXAvalaraSetup].[CompanyID] = [P].[CompanyID]
      AND [P].[AddressValidatorPluginID] = 'AVALARA'
GO

--AC-1035555 Rename AU SO Action
--[MinVersion(Hash = e9cc0683976014becec76abde751ab89ea16fc9c2a898669898618f74f8d24c6)]
UPDATE [AUStepAction] SET MenuText = 'RecalcExternalTax' WHERE MenuText = 'RecalcAvalara'
UPDATE [AUAction] SET MenuText = 'RecalcExternalTax' WHERE MenuText = 'RecalcAvalara'
GO

--AC-60568 Move totals from SOInvoice to ARInvoice
--[IfExists(Column = ARInvoice.CuryGoodsTotal)]
--[IfExists(Column = SOInvoice.CuryLineTotal)]
--[MinVersion(Hash = af433a9b1bf6377e06d203c52acb27badc63836c5e42a0ef50a28189096a5902)]
UPDATE [AR]
SET [CuryGoodsTotal] = [SO].[CuryLineTotal],
	[GoodsTotal] = [SO].[LineTotal],
	[CuryPremiumFreightAmt] = [SO].[CuryPremiumFreightAmt],
	[CuryFreightCost] = [SO].[CuryFreightCost],
	[FreightCost] = [SO].[FreightCost],
	[PremiumFreightAmt] = [SO].[PremiumFreightAmt],
	[CuryFreightAmt] = [SO].[CuryFreightAmt],
	[FreightAmt] = [SO].[FreightAmt],
	[CuryFreightTot] = [SO].[CuryFreightTot],
	[FreightTot] = [SO].[FreightTot],
	[CuryMiscTot] = [SO].[CuryMiscTot],
	[MiscTot] = [SO].[MiscTot]
FROM [ARInvoice] [AR]
INNER JOIN [SOInvoice] [SO] ON [AR].[CompanyID] = [SO].[CompanyID] AND [AR].[RefNbr] = [SO].[RefNbr] AND [AR].[DocType] = [SO].[DocType]
GO

--AC-60568 Move totals from ARRegister to ARInvoice
--[IfExists(Column = ARInvoice.CuryDiscTot)]
--[IfExists(Column = ARRegister.CuryDiscTot)]
--[MinVersion(Hash = cdda6312f090baab7fa411b0577fcdd4d4bd6ce378058551b7e79735c1166ecd)]
UPDATE [AR]
SET [CuryDiscTot] = [RG].[CuryDiscTot],
	[DiscTot] = [RG].[DiscTot]
FROM [ARInvoice] [AR]
INNER JOIN [ARRegister] [RG] ON [AR].[CompanyID] = [RG].[CompanyID] AND [AR].[RefNbr] = [RG].[RefNbr] AND [AR].[DocType] = [RG].[DocType]
GO

-- Creates CAExpenses for CATransfers
--[OldHash(Hash = 51a6939ad8acd7d35b7779cb2d2ba657d4687bb473bbfb48db981bad0e5e87eb)]
--[MinVersion(Hash = 242f02308e509e2490e67cf326e506fe8aa6deaa3816d36895d25d8b50de8cf6)]
insert into CAExpense
select 
CATran.CompanyID,
CAAdj.BranchID,
CATran.OrigRefNbr,
lineNbr.val,
CAAdj.AdjRefNbr,
CATran.CashAccountID,
CAAdj.EntryTypeID,
CASplit.AccountID,
CASplit.SubID,
CAAdj.DrCr,
CATran.CuryID,
CATran.CuryInfoID,
CATran.TranID,
CAAdj.CuryTranAmt,
CAAdj.TranAmt,
CATran.Released,
CATran.Cleared,
CATran.ClearDate,
CATran.ExtRefNbr,
CAAdj.TranDate,
CAAdj.TranPeriodID,
CAAdj.FinPeriodID,
CAAdj.TranDesc,
NEWID(),
CATran.CreatedByID,
CATran.CreatedByScreenID,
CATran.CreatedDateTime,
CATran.LastModifiedByID,
CATran.LastModifiedByScreenID,
CATran.LastModifiedDateTime,
null
from CATran
inner join CATransfer on CATran.CompanyID = CATransfer.CompanyID and CATran.OrigRefNbr = CATransfer.TransferNbr and CATran.OrigTranType = 'CTE'
inner join CAAdj on CATran.CompanyID = CAAdj.CompanyID and CAAdj.TranID = CATran.TranID and CATran.OrigRefNbr = CAAdj.TransferNbr and CAAdj.AdjTranType = 'CTE' and CAAdj.CashAccountID = CATran.CashAccountID
inner join CASplit on CATran.CompanyID = CASplit.CompanyID and CAAdj.AdjRefNbr = CASplit.AdjRefNbr and CASplit.LineNbr = 1
inner join (SELECT a.CompanyID, a.TransferNbr, a.TranID, count(*) as val FROM CAAdj a
inner join CAAdj b ON a.CompanyID = b.CompanyID and a.TransferNbr = b.TransferNbr AND a.TranID >= b.TranID
where a.AdjTranType = 'CTE' 
GROUP BY a.CompanyID, a.TransferNbr, a.TranID) lineNbr on CATran.CompanyID = lineNbr.CompanyID and CAAdj.TransferNbr = lineNbr.TransferNbr and CAAdj.TranID = lineNbr.TranID
left join CAExpense on CATran.CompanyID = CAExpense.CompanyID and CATran.OrigRefNbr = CAExpense.RefNbr and CAExpense.LineNbr = lineNbr.val  and CAExpense.CashTranID = CATran.TranID
where CAExpense.CompanyID is null and CAExpense.RefNbr is null and CAExpense.LineNbr is null
GO

-- Update CATransfer.ExpenseCntr
--[MinVersion(Hash = 486c626260b60a5d481c876278c786bb342244d2677327cf536dc9562ec8280a)]
update CATransfer
set CATransfer.ExpenseCntr = GroupedExpenses.lastLineNbr
from CATransfer inner join
(select RefNbr, max(LineNbr) as lastLineNbr from CAExpense group by RefNbr) as GroupedExpenses
on CATransfer.TransferNbr = GroupedExpenses.RefNbr and CATransfer.ExpenseCntr is null
GO

-- Update CAExpense.NoteID from related CATran.NoteID
--[MinVersion(Hash = d494aed15aae5d90abdb6f6fbd13ed8417144308dc4c41559ba3f7bdfca8618a)]
update CAExpense
set CAExpense.NoteID = CATran.NoteID
from CAExpense
inner join CATran on CAExpense.CompanyID = CATran.CompanyID and CAExpense.CashTranID = CATran.TranID
where CATran.NoteID is not null and CAExpense.NoteID is null
GO

--[Timeout(Multiplier = 100)]
--[IfExists(Column = CRQuote.QuoteID)]
--[IfExists(Column = CRQuote.RevisionID)]
--[IfNotExists(Column = CROpportunityProducts.CROpportunityID)]
--[MinVersion(Hash = 23193f69d4c0a72377afc80ec358971585771cf1f2ff506bf9967a11de42e856)]
UPDATE CRQuote SET QuoteID = rev.NoteID
FROM CROpportunityRevision AS rev WHERE
	rev.CompanyID = CRQuote.CompanyID AND
	rev.OpportunityID = CRQuote.OpportunityID AND
	rev.RevisionID = CRQuote.RevisionID

UPDATE CROpportunityProducts SET QuoteID = rev.NoteID
FROM CROpportunityRevision AS rev WHERE
	rev.CompanyID = CROpportunityProducts.CompanyID AND
	rev.OpportunityID = CROpportunityProducts.OpportunityID AND
	rev.RevisionID = CROpportunityProducts.RevisionID

UPDATE CROpportunityTax SET QuoteID = rev.NoteID
FROM CROpportunityRevision AS rev WHERE
	rev.CompanyID = CROpportunityTax.CompanyID AND
	rev.OpportunityID = CROpportunityTax.OpportunityID AND
	rev.RevisionID = CROpportunityTax.RevisionID

UPDATE CROpportunityDiscountDetail SET QuoteID = rev.NoteID
FROM CROpportunityRevision AS rev WHERE
	rev.CompanyID = CROpportunityDiscountDetail.CompanyID AND
	rev.OpportunityID = CROpportunityDiscountDetail.OpportunityID AND
	rev.RevisionID = CROpportunityDiscountDetail.RevisionID

UPDATE CRQuote SET QuoteID=NEWID() WHERE QuoteID IS NULL

UPDATE CROpportunityProducts SET QuoteID=NEWID() WHERE QuoteID IS NULL

UPDATE CROpportunityTax SET QuoteID=NEWID() WHERE QuoteID IS NULL

UPDATE CROpportunityDiscountDetail SET QuoteID=NEWID() WHERE QuoteID IS NULL
GO

-- Set BranchID to the DRScheduleDetail if company has only one branch
--[MinVersion(Hash = 5e25d4373fc1aa86ce11d6de9bd45ee5e8c356012160021c8d947003d67d27a8)]
UPDATE DRScheduleDetail 
SET BranchID = SingleBranch.BranchID 
FROM DRScheduleDetail 
   INNER JOIN
      ( 
		SELECT Branch.CompanyID, Branch.BranchID 
        FROM Branch 
            INNER JOIN
               (
                  SELECT CompanyID, COUNT(*) AS BranchCount 
                  FROM Branch 
                  WHERE DeletedDatabaseRecord = 0 
                  GROUP BY CompanyID
               ) AS CompanyBranches 
               ON Branch.CompanyID = CompanyBranches.CompanyID 
        WHERE
            CompanyBranches.BranchCount = 1
			AND DeletedDatabaseRecord = 0 
      ) AS SingleBranch 
      ON DRScheduleDetail.CompanyID = SingleBranch.CompanyID
WHERE DRScheduleDetail.BranchID IS NULL

-- Set BranchID from DRScheduleTran (get any Branch) for DRScheduleDetail
UPDATE DRScheduleDetail 
SET BranchID = AnyBranchFromTran.BranchID 
FROM DRScheduleDetail 
   INNER JOIN
      (
         SELECT
            DRScheduleDetail.CompanyID,
            DRScheduleDetail.ScheduleID,
            DRScheduleDetail.ComponentID,
            MAX(DRScheduleTran.BranchID) AS BranchID 
         FROM DRScheduleDetail 
            INNER JOIN DRScheduleTran 
               ON DRScheduleDetail.CompanyID = DRScheduleTran.CompanyID 
               AND DRScheduleDetail.ScheduleID = DRScheduleTran.ScheduleID 
               AND DRScheduleDetail.ComponentID = DRScheduleTran.ComponentID 
         WHERE
            DRScheduleDetail.BranchID IS NULL
         GROUP BY
            DRScheduleDetail.CompanyID,
            DRScheduleDetail.ScheduleID,
            DRScheduleDetail.ComponentID
      ) AS AnyBranchFromTran 
      ON DRScheduleDetail.CompanyID = AnyBranchFromTran.CompanyID 
      AND DRScheduleDetail.ScheduleID = AnyBranchFromTran.ScheduleID 
      AND DRScheduleDetail.ComponentID = AnyBranchFromTran.ComponentID
	  
-- Set BranchID to the balance and projection tables if the company has only one branch
UPDATE DRExpenseBalance 
SET BranchID = SingleBranch.BranchID 
FROM DRExpenseBalance 
   INNER JOIN
      ( 
		SELECT Branch.CompanyID, Branch.BranchID 
        FROM Branch 
            INNER JOIN
               (
                  SELECT CompanyID, COUNT(*) AS BranchCount 
                  FROM Branch 
                  WHERE DeletedDatabaseRecord = 0 
                  GROUP BY CompanyID
               ) AS CompanyBranches 
               ON Branch.CompanyID = CompanyBranches.CompanyID 
        WHERE
            CompanyBranches.BranchCount = 1
			AND DeletedDatabaseRecord = 0 
      ) AS SingleBranch 
      ON DRExpenseBalance.CompanyID = SingleBranch.CompanyID
WHERE DRExpenseBalance.BranchID IS NULL
	  
UPDATE DRRevenueBalance
SET BranchID = SingleBranch.BranchID 
FROM DRRevenueBalance 
   INNER JOIN
      ( 
		SELECT Branch.CompanyID, Branch.BranchID 
        FROM Branch 
            INNER JOIN
               (
                  SELECT CompanyID, COUNT(*) AS BranchCount 
                  FROM Branch 
                  WHERE DeletedDatabaseRecord = 0 
                  GROUP BY CompanyID
               ) AS CompanyBranches 
               ON Branch.CompanyID = CompanyBranches.CompanyID 
        WHERE
            CompanyBranches.BranchCount = 1
			AND DeletedDatabaseRecord = 0 
      ) AS SingleBranch 
      ON DRRevenueBalance.CompanyID = SingleBranch.CompanyID
WHERE DRRevenueBalance.BranchID IS NULL

UPDATE DRExpenseProjection
SET BranchID = SingleBranch.BranchID 
FROM DRExpenseProjection 
   INNER JOIN
      ( 
		SELECT Branch.CompanyID, Branch.BranchID 
        FROM Branch 
            INNER JOIN
               (
                  SELECT CompanyID, COUNT(*) AS BranchCount 
                  FROM Branch 
                  WHERE DeletedDatabaseRecord = 0 
                  GROUP BY CompanyID
               ) AS CompanyBranches 
               ON Branch.CompanyID = CompanyBranches.CompanyID 
        WHERE
            CompanyBranches.BranchCount = 1
			AND DeletedDatabaseRecord = 0 
      ) AS SingleBranch 
      ON DRExpenseProjection.CompanyID = SingleBranch.CompanyID
WHERE DRExpenseProjection.BranchID IS NULL

UPDATE DRRevenueProjection
SET BranchID = SingleBranch.BranchID 
FROM DRRevenueProjection 
   INNER JOIN
      ( 
		SELECT Branch.CompanyID, Branch.BranchID 
        FROM Branch 
            INNER JOIN
               (
                  SELECT CompanyID, COUNT(*) AS BranchCount 
                  FROM Branch 
                  WHERE DeletedDatabaseRecord = 0 
                  GROUP BY CompanyID
               ) AS CompanyBranches 
               ON Branch.CompanyID = CompanyBranches.CompanyID 
        WHERE
            CompanyBranches.BranchCount = 1
			AND DeletedDatabaseRecord = 0 
      ) AS SingleBranch 
      ON DRRevenueProjection.CompanyID = SingleBranch.CompanyID
WHERE DRRevenueProjection.BranchID IS NULL
GO

-- AC-109046 - unique NoteID for tables
--[mysql: Skip]
--[mssql: Native]
--[MinVersion(Hash = b3bb6f8eb8f2962b11a8a8bff47509df73d11c9c48006b58d8a4b59679f7e6b0)]
UPDATE Contact
SET 
	NoteID = NEWID()
WHERE 
	NoteID IS NULL 

CREATE TABLE #ContactNoteIDDups
(
	[CompanyID] [int] NOT NULL,
	[NoteID] [uniqueidentifier] NOT NULL,
	[MinID] [int] NOT NULL
)

CREATE INDEX Contact_NoteID_Migrate ON Contact(CompanyID, NoteID, ContactID)
CREATE INDEX ContactNoteIDDups_NoteID_Migrate ON #ContactNoteIDDups(CompanyID, NoteID, MinID)

INSERT INTO #ContactNoteIDDups
SELECT
	[CompanyID], [NoteID], MIN([ContactID])
FROM Contact
GROUP BY [CompanyID], [NoteID]
HAVING Count(1) > 1

UPDATE Contact
SET 
	Contact.NoteID = NEWID()
FROM Contact
INNER JOIN #ContactNoteIDDups C
	ON C.CompanyID = Contact.CompanyID
	AND C.NoteID = Contact.NoteID
	AND C.MinID < Contact.ContactID	

UPDATE S
SET 
	S.LocalID = C.ContactID
FROM SFSyncRecord S
INNER JOIN Contact C
	ON C.CompanyID = S.CompanyID
	AND C.NoteID = S.LocalGuid
	
DROP INDEX Contact_NoteID_Migrate ON Contact;

DROP TABLE #ContactNoteIDDups

-- Address
UPDATE Address
SET 
	NoteID = NEWID()
WHERE 
	NoteID IS NULL 

CREATE TABLE #AddressNoteIDDups
(
	[CompanyID] [int] NOT NULL,
	[NoteID] [uniqueidentifier] NOT NULL,
	[MinID] [int] NOT NULL
)

CREATE INDEX Address_NoteID_Migrate ON Address(CompanyID, NoteID, AddressID)
CREATE INDEX AddressNoteIDDups_NoteID_Migrate ON #AddressNoteIDDups(CompanyID, NoteID, MinID)

INSERT INTO #AddressNoteIDDups
SELECT
	[CompanyID], [NoteID], MIN([AddressID])
FROM Address
GROUP BY [CompanyID], [NoteID]
HAVING Count(1) > 1

UPDATE Address
SET
	Address.NoteID = NEWID()
FROM Address
INNER JOIN #AddressNoteIDDups A
	ON A.CompanyID = Address.CompanyID
	AND A.NoteID = Address.NoteID
	AND A.MinID < Address.AddressID

DROP INDEX Address_NoteID_Migrate ON Address

DROP TABLE #AddressNoteIDDups

-- BAccount 
UPDATE BAccount
SET 
	NoteID = NEWID()
WHERE 
	NoteID IS NULL 

CREATE TABLE #BAccountNoteIDDups
(
	[CompanyID] [int] NOT NULL,
	[NoteID] [uniqueidentifier] NOT NULL,
	[MinID] [int] NOT NULL
)

CREATE INDEX BAccount_NoteID_Migrate ON BAccount(CompanyID, NoteID, BAccountID)
CREATE INDEX BAccountNoteIDDups_NoteID_Migrate ON #BAccountNoteIDDups(CompanyID, NoteID, MinID)

INSERT INTO #BAccountNoteIDDups
SELECT
	[CompanyID], [NoteID], MIN([BAccountID])
FROM BAccount
GROUP BY [CompanyID], [NoteID]
HAVING Count(1) > 1

UPDATE BAccount
SET
	BAccount.NoteID = NEWID()
FROM BAccount
INNER JOIN #BAccountNoteIDDups C
	ON C.CompanyID = BAccount.CompanyID
	AND C.NoteID = BAccount.NoteID
	AND C.MinID < BAccount.BAccountID

UPDATE S
SET S.LocalID = B.BAccountID
FROM SFSyncRecord S
INNER JOIN BAccount B
	ON B.CompanyID = S.CompanyID
	AND B.NoteID = S.LocalGuid

DROP INDEX BAccount_NoteID_Migrate ON BAccount

DROP TABLE #BAccountNoteIDDups
GO

--[mssql: Skip]
--[mysql: Native]
--[MinVersion(Hash = 8cd3ef91deee926369a0cfae38ded54b4d6bee00b01991a7b292ba6500c4accb)]
UPDATE Contact
SET 
	NoteID = UUID()
WHERE 
	NoteID IS NULL;

CREATE TEMPORARY TABLE ContactNoteIDDups
(
	CompanyID int NOT NULL,
	NoteID char(36) NOT NULL,
	MinID int NOT NULL
);

CREATE INDEX Contact_NoteID_Migrate ON Contact(CompanyID, NoteID, ContactID);
CREATE INDEX ContactNoteIDDups_NoteID_Migrate ON ContactNoteIDDups(CompanyID, NoteID, MinID);

INSERT INTO ContactNoteIDDups
SELECT
	CompanyID, NoteID, MIN(ContactID)
FROM Contact
GROUP BY CompanyID, NoteID
HAVING Count(1) > 1;

UPDATE Contact
INNER JOIN ContactNoteIDDups C
	ON C.CompanyID = Contact.CompanyID
	AND C.NoteID = Contact.NoteID
	AND C.MinID < Contact.ContactID	
SET 
	Contact.NoteID = UUID();

UPDATE SFSyncRecord
INNER JOIN Contact
	ON Contact.CompanyID = SFSyncRecord.CompanyID
	AND Contact.NoteID = SFSyncRecord.LocalGuid
SET 
	SFSyncRecord.LocalID = Contact.ContactID;
	
DROP INDEX Contact_NoteID_Migrate ON Contact;

DROP TABLE ContactNoteIDDups;

-- Address
UPDATE Address
SET 
	NoteID = UUID()
WHERE 
	NoteID IS NULL;
	
CREATE TEMPORARY TABLE AddressNoteIDDups
(
	CompanyID int NOT NULL,
	NoteID char(36) NOT NULL,
	MinID int NOT NULL
);

CREATE INDEX Address_NoteID_Migrate ON Address(CompanyID, NoteID, AddressID);
CREATE INDEX AddressNoteIDDups_NoteID_Migrate ON AddressNoteIDDups(CompanyID, NoteID, MinID);

INSERT INTO AddressNoteIDDups
SELECT
	CompanyID, NoteID, MIN(AddressID)
FROM Address
GROUP BY CompanyID, NoteID
HAVING Count(1) > 1;

UPDATE Address
INNER JOIN AddressNoteIDDups A
	ON A.CompanyID = Address.CompanyID
	AND A.NoteID = Address.NoteID
	AND A.MinID < Address.AddressID
SET
	Address.NoteID = UUID();

DROP INDEX Address_NoteID_Migrate ON Address;

DROP TABLE AddressNoteIDDups;

-- BAccount 
UPDATE BAccount
SET 
	NoteID = UUID()
WHERE 
	NoteID IS NULL;
	
CREATE TEMPORARY TABLE BAccountNoteIDDups
(
	CompanyID int NOT NULL,
	NoteID char(36) NOT NULL,
	MinID int NOT NULL
);

CREATE INDEX BAccount_NoteID_Migrate ON BAccount(CompanyID, NoteID, BAccountID);
CREATE INDEX BAccountNoteIDDups_NoteID_Migrate ON BAccountNoteIDDups(CompanyID, NoteID, MinID);

INSERT INTO BAccountNoteIDDups
SELECT
	CompanyID, NoteID, MIN(BAccountID)
FROM BAccount
GROUP BY CompanyID, NoteID
HAVING Count(1) > 1;

UPDATE BAccount
INNER JOIN BAccountNoteIDDups B
	ON B.CompanyID = BAccount.CompanyID
	AND B.NoteID = BAccount.NoteID
	AND B.MinID < BAccount.BAccountID
SET
	BAccount.NoteID = UUID();

UPDATE SFSyncRecord
INNER JOIN BAccount
	ON BAccount.CompanyID = SFSyncRecord.CompanyID
	AND BAccount.NoteID = SFSyncRecord.LocalGuid
SET SFSyncRecord.LocalID = BAccount.BAccountID;

DROP INDEX BAccount_NoteID_Migrate ON BAccount;

DROP TABLE BAccountNoteIDDups;
GO

--[MinVersion(Hash = af4c9e18e2e3a963d7cbcd4258e8f9ce0b62494aec71c012c743f606a15b0555)]
UPDATE APRegister SET AdjCntr = LineCntr
GO

--[MinVersion(Hash = 708b9a863dec06dbfb65dd215cf5404d1e39db6ad399abcc9d14dd265e96337e)]
UPDATE WikiRevision SET Content = REPLACE(CAST(Content AS varchar(MAX)), 'Main.aspx', 'Main')
FROM WikiRevision
GO

--[IfExists(Column = CROpportunity.DefQuoteID)]
--[IfExists(Column = CROpportunity.DefRevisionID)]
--[MinVersion(Hash = f951cf16c2242e64027f428c4d012437840959134ced543058825b642f75f8a1)]
UPDATE CROpportunity SET DefQuoteID = CROpportunityRevision.NoteID
FROM CROpportunityRevision WHERE
	CROpportunityRevision.CompanyID = CROpportunity.CompanyID AND 
	CROpportunityRevision.OpportunityID = CROpportunity.OpportunityID AND
	CROpportunityRevision.RevisionID = CROpportunity.DefRevisionID
GO

--[IfExists(Column = CROpportunityProducts.ExtCost)]
--[IfExists(Column = CROpportunityProducts.CuryExtCost)]
--[MinVersion(Hash = 071519a5fa0d59b033fbaca6a4548c78e59b0ca08d8eaa8bf8935f6830bc8890)]
UPDATE CROpportunityProducts SET ExtCost=0 WHERE ExtCost IS NULL
UPDATE CROpportunityProducts SET CuryExtCost=0 WHERE CuryExtCost IS NULL
GO

--Move Employee Base cost to new PMLaborCostRate table.
--[OldHash(Hash = 325b1a8f2069e4f28685bea48cd285a857d42b072513a4bfaaab5b3ccc9b44d7)]
--[MinVersion(Hash = 2e5666adab8888de86d843fb7392b18625f6d9a797a25bba3b65c14955453f85)]
INSERT INTO [PMLaborCostRate] ([CompanyID],[Type],[EmployeeID],[InventoryID],[EmploymentType],[RegularHours],[AnnualSalary],[CuryID],[EffectiveDate],[Rate],[CreatedByID],[CreatedByScreenID],[CreatedDateTime],[LastModifiedByID],[LastModifiedByScreenID],[LastModifiedDateTime],[NoteID])
SELECT r.[CompanyID],'E' as [Type],r.[EmployeeID], e.[LabourItemID],r.[RateType],r.[RegularHours],r.[AnnualSalary],c.[BaseCuryID],r.[EffectiveDate],r.[HourlyRate],
r.[CreatedByID],r.[CreatedByScreenID],r.[CreatedDateTime],r.[LastModifiedByID],r.[LastModifiedByScreenID],r.[LastModifiedDateTime],NEWID()
  FROM EPEmployeeRate r
INNER JOIN EPEmployee e ON r.CompanyID = e.CompanyID AND r.EmployeeID = e.BAccountID
INNER JOIN Company c ON r.CompanyID = c.CompanyID
WHERE e.LabourItemID IS NOT NULL AND r.HourlyRate IS NOT NULL
GO

--Move Employee specific cost to new PMLaborCostRate table.
--[OldHash(Hash = 0a9c2b36bd11e6e1e10204efc458491861f12c07d5968104a92aeffcf77ccedb)]
--[MinVersion(Hash = a449a156d1e8568eb71d1daa5485957d22aec95abbb89697da9adf6d9a2960cf)]
INSERT INTO [PMLaborCostRate] ([CompanyID],[Type],[ProjectID],[TaskID],[EmployeeID],[InventoryID],[EmploymentType],[CuryID],[EffectiveDate],[Rate],[CreatedByID],[CreatedByScreenID],[CreatedDateTime],[LastModifiedByID],[LastModifiedByScreenID],[LastModifiedDateTime],[NoteID])
SELECT r.[CompanyID],'P' as [Type],p.[ProjectID],p.[TaskID], r.[EmployeeID], e.[LabourItemID],'H',c.[BaseCuryID],r.[EffectiveDate],p.[HourlyRate],
r.[CreatedByID],r.[CreatedByScreenID],r.[CreatedDateTime],r.[LastModifiedByID],r.[LastModifiedByScreenID],r.[LastModifiedDateTime],NEWID()
  FROM EPEmployeeRate r
INNER JOIN EPEmployee e ON r.CompanyID = e.CompanyID AND r.EmployeeID = e.BAccountID
INNER JOIN Company c ON r.CompanyID = c.CompanyID
LEFT JOIN EPEmployeeRateByProject p ON r.CompanyID = p.CompanyID AND r.RateID = p.RateID
WHERE p.ProjectID IS NOT NULL AND e.LabourItemID IS NOT NULL AND p.HourlyRate IS NOT NULL
GO

--Init LabourItem in Time Activity
--[MinVersion(Hash = 53ab67488b7c2b85b8d9375a2485b6bc8f921951417a8d58e757bd093b9c8d5e)]
UPDATE a
SET a.LabourItemID=e.LabourItemID
FROM PMTimeActivity a
INNER JOIN EPEmployee e on a.CompanyID = e.CompanyID AND a.OwnerID = e.UserID
WHERE a.LabourItemID IS NULL AND e.LabourItemID IS NOT NULL
GO

--Init LabourItem in Time Card Summary record.
--[MinVersion(Hash = 2d43d010018a9a51146b6e6b7ee1eec65aa3a073e70ae068619081721dd9e45d)]
UPDATE s
SET s.LabourItemID=e.LabourItemID
FROM EPTimeCardSummary s
INNER JOIN EPTimeCard t ON t.CompanyID = s.CompanyID AND t.TimeCardCD = s.TimeCardCD
INNER JOIN EPEmployee e on s.CompanyID = e.CompanyID AND t.EmployeeID = e.BAccountID
WHERE s.LabourItemID IS NULL AND e.LabourItemID IS NOT NULL
GO

--[IfExists(Column = EPSetup.PostToOffBalance)]
--[MinVersion(Hash = 9418399c13732edd612f76c39d86744ce2ee801b03a71bc7afd428c0d14cfbe7)]
UPDATE EPSetup SET PostingOption='O' WHERE PostToOffBalance = 1 AND OffBalanceAccountGroupID IS NOT NULL
GO

--Delete duplicate records in PMAccountTask table:
--[IfExists(Column = PMAccountTask.RecordID)]
--[MinVersion(Hash = 43774a56dfd60be53808dea9a9069b5dd163d7d4a23cbf2ae10728a68d5388ea)]
DELETE dups FROM PMAccountTask dups
LEFT OUTER JOIN ( 
    SELECT CompanyID, MIN(RecordID) as RowID, ProjectID, AccountID FROM PMAccountTask 
    GROUP BY CompanyID, ProjectID, AccountID 
) AS KeepRows ON KeepRows.CompanyID = dups.CompanyID AND KeepRows.RowID = dups.RecordID 
WHERE KeepRows.RowID IS NULL
GO

-- Remove extra SMCalendarSettings, leave only one per user
--[MinVersion(Hash = eb37a7cf4390a6941aece11b9b36761f38b4079ee6b385ec646c48ad8ac8e2fb)]
DELETE extra
FROM SMCalendarSettings AS orig
JOIN SMCalendarSettings AS extra ON extra.CompanyID = orig.CompanyID AND extra.UserID = orig.UserID AND extra.pkID > orig.pkID
GO

-- Update ExpirationDate if it 's saved as the last day of month
--[mssql: Native]
--[mysql: Skip]
--[MinVersion(Hash = 55394b4a77f4bbee55edac5e4c595f4e006ac52cf53e78d70116ebbd43c1da4a)]
UPDATE CustomerPaymentMethod SET ExpirationDate = DATEADD(DAY, 1, CustomerPaymentMethod.ExpirationDate)
WHERE DAY(CustomerPaymentMethod.ExpirationDate) = DAY(EOMONTH(ExpirationDate))
GO

--[mysql: Native]
--[mssql: Skip]
--[MinVersion(Hash = 49baf2b412777e0053d4f7161816b0eecb217593116f6dc902c1b2797fa1e68d)]
UPDATE CustomerPaymentMethod SET ExpirationDate = DATE_ADD(CustomerPaymentMethod.ExpirationDate, INTERVAL 1 DAY)
WHERE DAY(CustomerPaymentMethod.ExpirationDate) = DAY(LAST_DAY(ExpirationDate))
GO

--[MinVersion(Hash = b908c7eff236a57dfb101942d6b904efeb29ec3274cf2cc7af8fa3232477f90f)]
UPDATE EPApproval
SET EPApproval.WaitTime = (CASE WHEN (EPApproval.WorkgroupID = EPCompanyTreeMemberA.WorkGroupID) AND
									 (EPCompanyTreeMemberA.UserID = EPApproval.OwnerID)
	   						    THEN EPCompanyTreeMemberA.WaitTime
								WHEN EPCompanyTreeH.ParentWGLevel < EPCompanyTreeH.WorkGroupLevel
								THEN EPCompanyTreeH.WaitTime
								ELSE NULL 
						   END)
FROM EPApproval as EPApproval
INNER JOIN EPCompanyTreeH AS EPCompanyTreeH 
								ON (EPCompanyTreeH.WorkGroupID = EPApproval.workGroupID) AND
								   (EPCompanyTreeH.CompanyID = EPApproval.CompanyID)
INNER JOIN EPCompanyTreeMember AS EPCompanyTreeMember
							    ON (EPCompanyTreeH.ParentWGID = EPCompanyTreeMember.WorkGroupID) AND
                                   (EPCompanyTreeMember.Active = 1) AND
                                   (EPCompanyTreeMember.UserID = EPApproval.OwnerID) AND
								   (EPCompanyTreeMember.CompanyID = EPApproval.CompanyID)
LEFT OUTER JOIN EPCompanyTreeMember AS EPCompanyTreeMemberA
                                ON (EPCompanyTreeMemberA.WorkGroupID = EPCompanyTreeMember.WorkGroupID) AND
								   (EPCompanyTreeMemberA.UserID = EPApproval.OwnerID) AND
								   (EPCompanyTreeMemberA.CompanyID = EPApproval.CompanyID)
WHERE (EPApproval.WaitTime IS NULL)
GO

--[MinVersion(Hash = 080d203ed41d63c625b8fbb5fe8c6ebd86807d7fcb1d56ba23a954a0fdc258f0)]
update itl
set itl.IsFixedInTransit = its.IsFixedInTransit,
	itl.IsLotSerial = case when its.LotSerialNbr is not null and its.LotSerialNbr != '' then 1 else 0 end
from INTransitLine itl
join INTranSplit its on
	its.CompanyID = itl.CompanyID and
	its.DocType = 'T' and
	its.RefNbr = itl.TransferNbr and
	its.LineNbr = itl.TransferLineNbr
GO

--[MinVersion(Hash = dc5ad1e7036b13ecad64f9deda26d650706d00ef56f75ebbeef53ae34dd23a6c)]
update prl
set prl.OrigToLocationID = itl.ToLocationID,
	prl.OrigIsLotSerial = itl.IsLotSerial,
	prl.OrigIsFixedInTransit = itl.IsFixedInTransit,
	prl.OrigNoteID = itl.NoteID
from POReceiptLine prl
join INTransitLine itl on
	itl.CompanyID = prl.CompanyID and
	itl.TransferNbr = prl.OrigRefNbr and
	itl.TransferLineNbr = prl.OrigLineNbr 
where prl.OrigTranType = 'TRX'
	and prl.OrigToLocationID is null
	and prl.OrigIsLotSerial is null
	and prl.OrigIsFixedInTransit is null
	and prl.OrigNoteID is null
GO

--[MinVersion(Hash = c220a673f96262fdf6b5911c6e3deb164e6ed56d035633f526f7d729e3f1219a)]
update it
set it.OrigToLocationID = orig.ToLocationID,
	it.OrigIsLotSerial = case when orig.LotSerialNbr is not null and orig.LotSerialNbr != '' then 1 else 0 end,
	it.OrigNoteID = origDoc.NoteID
from INTran it
join INRegister origDoc on
	origDoc.CompanyID = it.CompanyID and
	origDoc.RefNbr = it.OrigRefNbr and 
	origDoc.DocType = 'T'
join INTranSplit orig on
	orig.CompanyID = origDoc.CompanyID and
	orig.DocType = origDoc.DocType and
	orig.RefNbr = origDoc.RefNbr and
	orig.LineNbr = it.OrigLineNbr
where it.OrigTranType = 'TRX'
	and it.OrigToLocationID is null
	and it.OrigIsLotSerial is null
	and it.OrigNoteID is null
GO

--Disable Customized AU Steps:
--[MinVersion(Hash = db386f8020f689d6d59040776e287c2c26fa69ebde943e451d0d0be3a2905685)]
UPDATE AUStep SET IsActive=0 WHERE ScreenID='PO301000' AND StepID IN ('NL Closed', 'NL Open', 'NL Pending Email','NL Pending Printed')
GO

--[IfExists(Table = FSxSOOrderType)]
--[IfNotExistsSelect(From = FSxSOOrderType)]
--[MinVersion(Hash = 598f9afb0a55d70e69cf44ee300048a9c1bde77694c51605ad75c2b27ec7de07)]
BEGIN
	INSERT INTO FSxSOOrderType
	(
		    CompanyID
           ,OrderType
           ,EnableFSIntegration
	)
    SELECT
            CompanyID
           ,OrderType
           ,1
	FROM (
		SELECT DISTINCT SOOrder.CompanyID, SOOrder.OrderType
		FROM SOOrder
			INNER JOIN FSxSOOrder ON (FSxSOOrder.CompanyID = SOOrder.CompanyID
									AND FSxSOOrder.OrderType = SOOrder.OrderType
									AND FSxSOOrder.OrderNbr = SOOrder.OrderNbr)
		WHERE
			FSxSOOrder.SrvOrdType IS NOT NULL
			OR SOOrder.CreatedByScreenID like 'FS%'
	 ) OrderTypesWithServiceOrders;

	INSERT INTO FSxSOOrderType
	(
		    CompanyID
           ,OrderType
           ,EnableFSIntegration
	)
    SELECT
            CompanyID
           ,ContractPostOrderType
           ,1
	FROM (
		SELECT DISTINCT FSSetup.CompanyID, FSSetup.ContractPostOrderType
		FROM FSSetup
			LEFT OUTER JOIN FSxSOOrderType ON (FSxSOOrderType.CompanyID = FSSetup.CompanyID
											AND FSxSOOrderType.OrderType = FSSetup.ContractPostOrderType)
		WHERE
			FSxSOOrderType.OrderType IS NULL
			AND ContractPostOrderType IS NOT NULL
	 ) OrderTypeInSetup;

	INSERT INTO FSxSOOrderType
	(
		    CompanyID
           ,OrderType
           ,EnableFSIntegration
	)
    SELECT
            CompanyID
           ,PostOrderTypeNegativeBalance
           ,1
	FROM (
		SELECT DISTINCT FSSrvOrdType.CompanyID, FSSrvOrdType.PostOrderTypeNegativeBalance
		FROM FSSrvOrdType
			LEFT OUTER JOIN FSxSOOrderType ON (FSxSOOrderType.CompanyID = FSSrvOrdType.CompanyID
											AND FSxSOOrderType.OrderType = FSSrvOrdType.PostOrderTypeNegativeBalance)
		WHERE
			FSxSOOrderType.OrderType IS NULL
			AND PostOrderTypeNegativeBalance IS NOT NULL
	 ) OrderTypeInSrvOrdType;

	INSERT INTO FSxSOOrderType
	(
		    CompanyID
           ,OrderType
           ,EnableFSIntegration
	)
    SELECT
            CompanyID
           ,PostOrderType
           ,1
	FROM (
		SELECT DISTINCT FSSrvOrdType.CompanyID, FSSrvOrdType.PostOrderType
		FROM FSSrvOrdType
			LEFT OUTER JOIN FSxSOOrderType ON (FSxSOOrderType.CompanyID = FSSrvOrdType.CompanyID
											AND FSxSOOrderType.OrderType = FSSrvOrdType.PostOrderType)
		WHERE
			FSxSOOrderType.OrderType IS NULL
			AND PostOrderType IS NOT NULL
	 ) OrderTypeInSrvOrdType;
END
GO

--Fin Period Closing by Companies
--[MinVersion(Hash = 61f01995484ce814bf328ad8bc9820a01972626a3419ef345d81990e519b3324)]
update CATran set CATran.BranchID=CashAccount.BranchID
from CATran 
inner join CashAccount 
	on CashAccount.CompanyID=CATran.CompanyID
	and CashAccount.CashAccountID=CATran.CashAccountID
GO

--[MinVersion(Hash = 89b57e047546d4006f6fa4eaabcd3a7d5917f56df7ffbb36812ba33b8168da1e)]
update CADeposit set CADeposit.BranchID=CashAccount.BranchID
from CADeposit 
inner join CashAccount 
	on CashAccount.CompanyID=CADeposit.CompanyID
	and CashAccount.CashAccountID=CADeposit.CashAccountID
GO

--[MinVersion(Hash = 2d27705f94e8f1929812daa064bb725327bcb95f20148b7f7ab79a1b37960251)]
update CATransfer set CATransfer.OutBranchID=CashAccount.BranchID
from CATransfer 
inner join CashAccount 
	on CashAccount.CompanyID=CATransfer.CompanyID
	and CashAccount.CashAccountID=CATransfer.OutAccountID
GO

--[MinVersion(Hash = 31ccb60d81a16c0ca9be4ee791c7fea23f8149374e18dbbe4b4ce037faa2bda1)]
update CATransfer set CATransfer.InBranchID=CashAccount.BranchID
from CATransfer 
inner join CashAccount 
	on CashAccount.CompanyID=CATransfer.CompanyID
	and CashAccount.CashAccountID=CATransfer.InAccountID
GO

--[MinVersion(Hash = be54379f528abaab0e50e219f1a3c7c769e97833c9adca0d7b2ee03c3ca331a6)]
UPDATE GLSetup
SET RestrictAccessToClosedPeriods = case when PostClosedPeriods = 0 then 1 else 0 end
GO

--[MinVersion(Hash = 8a3374f4f8afd7856484c418e07da35aaab839a24b39bc8ea1a32053d8b02177)]
UPDATE GITable
SET Name = 'PX.Objects.GL.FinPeriods.MasterFinPeriod'
WHERE Name = 'PX.Objects.GL.FinPeriod'

UPDATE DashboardParameter
SET ObjectName = 'PX.Objects.GL.FinPeriods.MasterFinPeriod'
WHERE ObjectName = 'PX.Objects.GL.FinPeriod'
GO

--[MinVersion(Hash = e3c7a8097b2e78fd57a354531c3cd2e6cc65180770de45b3ae5847a6c4c1c84b)]
UPDATE GITable
SET Name = 'PX.Objects.GL.FinPeriods.MasterFinYear'
WHERE Name = 'PX.Objects.GL.FinYear'

UPDATE DashboardParameter
SET ObjectName = 'PX.Objects.GL.FinPeriods.MasterFinYear'
WHERE ObjectName = 'PX.Objects.GL.FinYear'
GO

--End of Fin Period Closing by Companies
--[MinVersion(Hash = 10f06c15e085a3e3f3d5bdc5748da3fd5d95cbe5fef6229da961b0840d2119ea)]
UPDATE EPExpenseClaimDetails
SET ClaimDetailCD = RIGHT('000000'+CAST(ClaimDetailID AS nvarchar(6)), 6)
GO

--[IfExists(Column = CROpportunityProducts.CROpportunityID)]
--[MinVersion(Hash = 8565bf7747f8513be575ed777b876b5df42e00f56a3d52bceb5995c5e65691b6)]
UPDATE CROpportunityProducts SET Descr = CROpportunityID, QuoteID = rev.NoteID
FROM CROpportunityRevision AS rev WHERE
	rev.CompanyID = CROpportunityProducts.CompanyID AND
    rev.OpportunityID = CROpportunityProducts.CROpportunityID AND
    QuoteID IS NULL

UPDATE CROpportunityTax SET QuoteID = rev.NoteID
FROM CROpportunityRevision AS rev WHERE
	rev.CompanyID = CROpportunityTax.CompanyID AND
	rev.OpportunityID = CROpportunityTax.OpportunityID AND
	QuoteID IS NULL

UPDATE CROpportunityDiscountDetail SET QuoteID = rev.NoteID
FROM CROpportunityRevision AS rev WHERE
	rev.CompanyID = CROpportunityDiscountDetail.CompanyID AND
	rev.OpportunityID = CROpportunityDiscountDetail.OpportunityID AND
	QuoteID IS NULL
GO

--[MinVersion(Hash = 218e9022cc93ae52eeb662658c90cf17255f8e40a062dbeda6258ff70031d38c)]
UPDATE CRQuote SET QuoteType = 'D' WHERE QuoteType IS NULL
GO

--[MinVersion(Hash = b594beb24325b3d035e46bedb54afd693dcc605556a6e591c1a753880758c405)]
UPDATE CROpportunityProducts SET LineType = 'D' WHERE LineType IS NULL
GO

--[MinVersion(Hash = 883e7cb930da1ff59cf816044b83676f4d08bb84be76fe1e6322855cba4d1f29)]
UPDATE CROpportunityProducts SET SortOrder = LineNbr WHERE SortOrder IS NULL
GO

--[MinVersion(Hash = 77b28857368607870943129415e6f4458b2d19e44002ed5ef44fb5a3e1f2d934)]
INSERT INTO CRTaxTran(
       CompanyID, QuoteID, LineNbr, TaxID, TaxRate, CuryInfoID, CuryTaxableAmt, TaxableAmt, CuryTaxAmt, TaxAmt, NonDeductibleTaxRate, ExpenseAmt, CuryExpenseAmt, CreatedByID, CreatedByScreenID, CreatedDateTime, LastModifiedByID, LastModifiedByScreenID, LastModifiedDateTime)
SELECT CompanyID, QuoteID, LineNbr, TaxID, TaxRate, CuryInfoID, CuryTaxableAmt, TaxableAmt, CuryTaxAmt, TaxAmt, NonDeductibleTaxRate, ExpenseAmt, CuryExpenseAmt, CreatedByID, CreatedByScreenID, CreatedDateTime, LastModifiedByID, LastModifiedByScreenID, LastModifiedDateTime
FROM CROpportunityTax WHERE LineNbr = 2147483647
GO

--[MinVersion(Hash = 0db7da9b553cf348d93dda92492dedd462fc4538cbc19082235dc424cfa4197b)]
UPDATE FeaturesSet SET ProjectAccounting = ProjectModule WHERE ProjectAccounting <> ProjectModule OR ProjectAccounting is null
GO

--[IfExists(Column = FSxCROpportunityProducts.QuoteID)]
--[IfExists(Column = FSxCROpportunityProducts.RevisionID)]
--[IfExists(Column = FSxCROpportunityProducts.OpportunityID)]
--[MinVersion(Hash = ede9c8674de890db499bdc1bea8b4542f36396abe3a141b1616a96b52d2510b1)]
UPDATE FSxCROpportunityProducts SET QuoteID = rev.NoteID
FROM CROpportunityRevision AS rev WHERE
	rev.CompanyID = FSxCROpportunityProducts.CompanyID AND
	rev.OpportunityID = FSxCROpportunityProducts.OpportunityID AND
	rev.RevisionID = FSxCROpportunityProducts.RevisionID
GO

--Set default value for the new field POAccrualType.
--[OldHash(Hash = dfe5b17718fce36f45a30f38a923cfcb2c034102f3ee691dc3e1a5a221f00551)]
--[MinVersion(Hash = 7e54eeef9b012cac15b105feec8c1e173b69a5be676a05672b05e94fffced90c)]
UPDATE pol
SET POAccrualType = CASE
    WHEN pol.LineType = 'SV'
        AND (pol.OrderType IN ('RO', 'RS') AND COALESCE(setup.AddServicesFromNormalPOtoPR, 0) = 0
        OR pol.OrderType = 'DP' AND COALESCE(setup.AddServicesFromDSPOtoPR, 0) = 0)
    THEN 'O'
    ELSE 'R' END
FROM POLine pol
INNER JOIN POSetup setup ON setup.CompanyID = pol.CompanyID
WHERE pol.POAccrualType IS NULL

UPDATE prl
SET POAccrualType = COALESCE(pol.POAccrualType, 'R')
FROM POReceiptLine prl
LEFT JOIN POLine pol ON pol.CompanyID = prl.CompanyID AND pol.OrderType = prl.POType AND pol.OrderNbr = prl.PONbr AND pol.LineNbr = prl.POLineNbr
WHERE prl.POAccrualType IS NULL

UPDATE apt
SET POAccrualType = COALESCE(prl.POAccrualType, pol.POAccrualType)
FROM APTran apt
LEFT JOIN POReceiptLine prl ON prl.CompanyID = apt.CompanyID AND prl.ReceiptNbr = apt.ReceiptNbr AND prl.LineNbr = apt.ReceiptLineNbr
LEFT JOIN POLine pol ON pol.CompanyID = apt.CompanyID AND pol.OrderType = apt.POOrderType AND pol.OrderNbr = apt.PONbr AND pol.LineNbr = apt.POLineNbr
WHERE apt.POAccrualType IS NULL AND (apt.ReceiptNbr IS NOT NULL OR apt.PONbr IS NOT NULL)
GO

-- Populate new fields POReceiptLine.TranUnitCost, TranCost, TranCostFinal
--[IfExists(Column = POReceiptLine.ExtCost)]
--[IfExists(Table = POReceiptTax)]
--[OldHash(Hash = dd0d89ac19d1580bae301974ef431cc0d7b213d67c85954e7b4e12cb13126141)]
--[MinVersion(Hash = 83c40ef631c8a8d3fd5ae14902b4d7625e38d3649d9eed9ef25ffa58b8fc504d)]
UPDATE rl
SET [TranCost] = COALESCE(it.[TranCost],
            CASE WHEN t.[TaxID] IS NOT NULL THEN rt.[TaxableAmt] + rl.[ExtCost] - ROUND(rl.[ExtCost] * rl.[GroupDiscountRate] * rl.[DocumentDiscountRate], COALESCE(cl.[DecimalPlaces], 2))
                 ELSE rl.[ExtCost] END)
FROM [POReceiptLine] rl
LEFT JOIN [INTran] it ON it.[CompanyID] = rl.[CompanyID] AND it.[DocType] = 'R' AND it.[POReceiptType] = rl.[ReceiptType] AND it.[POReceiptNbr] = rl.[ReceiptNbr] AND it.[POReceiptLineNbr] = rl.[LineNbr]
LEFT JOIN [POReceiptTax] rt ON rt.[CompanyID] = rl.[CompanyID] AND rt.[ReceiptNbr] = rl.[ReceiptNbr] AND rt.[LineNbr] = rl.[LineNbr]
LEFT JOIN [Tax] t ON t.[CompanyID] = rt.[CompanyID] AND t.[TaxID] = rt.[TaxID] AND t.[TaxCalcLevel] = '0' AND t.[TaxType] <> 'W' AND t.[ReverseTax] = 0
INNER JOIN [Company] c ON c.[CompanyID] = rl.[CompanyID]
LEFT JOIN [CurrencyList] cl ON cl.[CompanyID] = c.[CompanyID] AND cl.[CuryID] = c.[BaseCuryID]

UPDATE POReceiptLine
SET [TranUnitCost] = CASE WHEN [ReceiptQty] = 0 THEN [UnitCost] ELSE [TranCost] / [ReceiptQty] END

UPDATE POReceiptLine
SET [TranCostFinal] = CASE WHEN [Released] = 1 THEN [TranCost] ELSE [TranCostFinal] END
WHERE [ReceiptType] <> 'RN'

UPDATE rl
SET [TranCostFinal] = CASE WHEN ii.[StkItem] = 1 THEN
                         (CASE WHEN it.[Released] = 1 THEN it.[TranCost] ELSE rl.[TranCostFinal] END)
                      ELSE
                         (CASE WHEN rl.[Released] = 1 THEN rl.[TranCost] ELSE rl.[TranCostFinal] END)
                      END
FROM [POReceiptLine] rl
LEFT JOIN [InventoryItem] ii ON ii.[CompanyID] = rl.[CompanyID] AND ii.[InventoryID] = rl.[InventoryID]
LEFT JOIN [INTran] it ON it.[CompanyID] = rl.[CompanyID] AND it.[DocType] = 'I' AND it.[POReceiptType] = rl.[ReceiptType] AND it.[POReceiptNbr] = rl.[ReceiptNbr] AND it.[POReceiptLineNbr] = rl.[LineNbr]
WHERE rl.[ReceiptType] = 'RN'
GO

--AC-116545 Fill APTran BaseQty
--[MinVersion(Hash = d696901f16ea90b0e37e8e6f143ffa3f7cc4b10f7235b8e87239a4d5e0c07731)]
UPDATE [T]
	SET [T].[BaseQty] = CASE 
		WHEN [U].[UnitMultDiv] = 'M' THEN [T].[Qty] * [U].[UnitRate] 
		WHEN [U].[UnitMultDiv] = 'D' THEN ROUND([T].[Qty] / [U].[UnitRate], COALESCE([S].DecPlQty, 2))
		WHEN [BU].[UnitMultDiv] = 'M' THEN [T].[Qty] * [BU].[UnitRate] 
		WHEN [BU].[UnitMultDiv] = 'D' THEN ROUND([T].[Qty] / [BU].[UnitRate], COALESCE([S].DecPlQty, 2))
		ELSE [T].[Qty] END
	FROM [APTran] [T]
		INNER JOIN [CommonSetup] [S] ON [T].[CompanyID] = [S].[CompanyID]
		LEFT JOIN [InventoryItem] [I] ON [T].[CompanyID] = [I].[CompanyID] AND [T].[InventoryID] = [I].[InventoryID] 
		LEFT JOIN [INUnit] [U] ON [T].[CompanyID] = [U].[CompanyID] AND [T].[InventoryID] = [U].[InventoryID] 
			AND [T].[UOM] = [U].[FromUnit] AND [I].[BaseUnit] = [U].[ToUnit]
		LEFT JOIN [INUnit] [BU] ON [T].[CompanyID] = [BU].[CompanyID] AND ([BU].[UnitType] = 3)
			AND [T].[UOM] = [BU].[FromUnit] AND [I].[BaseUnit] = [BU].[ToUnit]
	WHERE [T].[BaseQty] IS NULL
GO

-- Correct base qty fields in PO Lines, PO Receipt Lines, AP Bill Lines with empty Inventory (services)
--[MinVersion(Hash = c7ef5f916d9862e94c280bfb27eaae92b9c043108e9be54c013dc73bc18ce772)]
UPDATE POLine
SET BaseOrderQty = OrderQty,
	BaseReceivedQty = ReceivedQty,
	BaseCompletedQty = CompletedQty,
	BaseBilledQty = BilledQty,
	BaseOpenQty = OpenQty,
	BaseUnbilledQty = UnbilledQty
WHERE InventoryID IS NULL

UPDATE POReceiptLine
SET BaseReceiptQty = ReceiptQty,
	BaseMultReceiptQty = InvtMult * ReceiptQty,
	BaseUnbilledQty = UnbilledQty
WHERE InventoryID IS NULL

UPDATE APTran
SET BaseQty = Qty
WHERE InventoryID IS NULL
GO

-- Insert into POAccrualStatus all records corresponding to existing PO Lines with released Receipt or AP Bill (order-based billing)
-- Order-based billing in the scope of upgrade is possible only for Service Lines, so some fields may stay not accurately filled
--[IfExists(Column = POReceiptLine.CuryUnbilledAmt)]
--[IfExists(Column = POReceiptLine.CuryUnbilledDiscountAmt)]
--[IfExists(Column = POReceiptLine.BillPPVAmt)]
--[MinVersion(Hash = 25dbd2bf972ef72e10897cba26f9008f01e83cebdca8d816da4715049d06cc9d)]
INSERT INTO [POAccrualStatus] (
    [CompanyID], [RefNoteID], [LineNbr], [Type],
    [LineType], [OrderType], [OrderNbr], [OrderLineNbr],
    [ReceiptType], [ReceiptNbr],
    [VendorID], [InventoryID], [SubItemID], [SiteID], [AcctID], [SubID],
    [OrigUOM], [OrigQty], [BaseOrigQty],
    [OrigCuryID], [CuryOrigAmt], [OrigAmt],
    [CuryOrigCost], [OrigCost],
    [CuryOrigDiscAmt], [OrigDiscAmt],
    [ReceivedUOM], [ReceivedQty], [BaseReceivedQty],
    [ReceivedCost],
    [BilledUOM], [BilledQty], [BaseBilledQty],
    [BillCuryID], [CuryBilledAmt], [BilledAmt],
    [CuryBilledCost], [BilledCost],
    [CuryBilledDiscAmt], [BilledDiscAmt],
    [PPVAmt],
    [CreatedDateTime], [CreatedByID], [CreatedByScreenID],
    [LastModifiedDateTime], [LastModifiedByID], [LastModifiedByScreenID])
SELECT po.[CompanyID], po.[NoteID], pl.[LineNbr], pl.[POAccrualType],
    pl.[LineType], po.[OrderType], po.[OrderNbr], pl.[LineNbr],
    NULL, NULL,
    po.[VendorID], pl.[InventoryID], pl.[SubItemID], pl.[SiteID], NULL, NULL,
    pl.[UOM], pl.[OrderQty], COALESCE(pl.[BaseOrderQty], 0),
    NULL, COALESCE(pl.[CuryExtCost] + pl.[CuryRetainageAmt], 0), COALESCE(pl.[ExtCost] + pl.[RetainageAmt], 0),
    COALESCE(pl.[CuryExtCost] + pl.[CuryRetainageAmt], 0), COALESCE(pl.[ExtCost] + pl.[RetainageAmt], 0),
    COALESCE(pl.[CuryDiscAmt], 0), COALESCE(pl.[DiscAmt], 0),
    NULL, NULL, COALESCE(prl.[BaseReceivedQty], 0),
    0,
    NULL, NULL, COALESCE(apt.[BaseBilledQty], 0),
    NULL, NULL, COALESCE(apt.[BilledAmt], 0),
    NULL, COALESCE(apt.[BilledCost], 0),
    NULL, COALESCE(apt.[BilledDiscAmt], 0),
    0,
    pl.[CreatedDateTime], pl.[CreatedByID], pl.[CreatedByScreenID],
    pl.[LastModifiedDateTime], pl.[LastModifiedByID], pl.[LastModifiedByScreenID]
FROM [POLine] pl
INNER JOIN [POOrder] po ON po.[CompanyID] = pl.[CompanyID] AND po.[OrderType] = pl.[OrderType] AND po.[OrderNbr] = pl.[OrderNbr]

LEFT JOIN (SELECT
    aptn.[CompanyID], aptn.[POOrderType], aptn.[PONbr], aptn.[POLineNbr],
    COALESCE(SUM(COALESCE(aptn.[BaseQty], aptn.[Qty]) * (CASE WHEN aptn.[DrCr] = 'D' THEN 1 ELSE -1 END)), 0) AS BaseBilledQty,
    COALESCE(SUM(aptn.[DiscAmt] * (CASE WHEN aptn.[DrCr] = 'D' THEN 1 ELSE -1 END)), 0) AS BilledDiscAmt,
    COALESCE(SUM((aptn.[TranAmt] + COALESCE(aptn.[RetainageAmt], 0)) * (CASE WHEN aptn.[DrCr] = 'D' THEN 1 ELSE -1 END)), 0) AS BilledAmt,
    COALESCE(SUM((CASE WHEN inclTax.[CompanyID] IS NOT NULL THEN inclTax.[TaxableAmt] + COALESCE(inclTax.[RetainedTaxableAmt], 0) + aptn.[TranAmt] - ROUND(aptn.[TranAmt] * aptn.[GroupDiscountRate] * aptn.[DocumentDiscountRate], COALESCE(cl.[DecimalPlaces], 2))
        ELSE aptn.[TranAmt] + COALESCE(aptn.[RetainageAmt], 0) END) * (CASE WHEN aptn.[DrCr] = 'D' THEN 1 ELSE -1 END)), 0) AS BilledCost
FROM [APTran] aptn
LEFT JOIN (
    SELECT aptx.[CompanyID], aptx.[TranType], aptx.[RefNbr], aptx.[LineNbr], MIN(aptx.[TaxableAmt]) as TaxableAmt, MIN(aptx.[RetainedTaxableAmt]) as RetainedTaxableAmt
    FROM [APTax] aptx
    INNER JOIN [Tax] t ON t.[CompanyID] = aptx.[CompanyID] AND t.[TaxID] = aptx.[TaxID]
    WHERE t.[TaxCalcLevel] = '0' AND t.[TaxType] <> 'W' AND t.[ReverseTax] = 0
    GROUP BY aptx.CompanyID, aptx.TranType, aptx.RefNbr, aptx.LineNbr) inclTax
    ON inclTax.[CompanyID] = aptn.[CompanyID] AND inclTax.[TranType] = aptn.[TranType] AND inclTax.[RefNbr] = aptn.[RefNbr] AND inclTax.[LineNbr] = aptn.[LineNbr]
INNER JOIN [Company] c ON c.[CompanyID] = aptn.[CompanyID]
LEFT JOIN [CurrencyList] cl ON cl.[CompanyID] = c.[CompanyID] AND cl.[CuryID] = c.[BaseCuryID]
WHERE aptn.[Released] = 1 AND aptn.[POOrderType] IS NOT NULL AND aptn.[PONbr] IS NOT NULL AND aptn.[POLineNbr] IS NOT NULL
GROUP BY aptn.[CompanyID], aptn.[POOrderType], aptn.[PONbr], aptn.[POLineNbr]) apt
ON apt.[CompanyID] = pl.[CompanyID] AND apt.[POOrderType] = pl.[OrderType] AND apt.[PONbr] = pl.[OrderNbr] AND apt.[POLineNbr] = pl.[LineNbr]

LEFT JOIN (SELECT
    porl.[CompanyID], porl.[POType], porl.[PONbr], porl.[POLineNbr],
    SUM(porl.[BaseReceiptQty] * (CASE WHEN porl.[InvtMult] < 0 THEN -1 ELSE 1 END)) AS BaseReceivedQty
FROM [POReceiptLine] porl
WHERE porl.[Released] = 1
GROUP BY porl.[CompanyID], porl.[POType], porl.[PONbr], porl.[POLineNbr]) prl
ON prl.[CompanyID] = pl.[CompanyID] AND prl.[POType] = pl.[OrderType] AND prl.[PONbr] = pl.[OrderNbr] AND prl.[POLineNbr] = pl.[LineNbr]

LEFT JOIN [POAccrualStatus] pas ON pas.[CompanyID] = po.[CompanyID] AND pas.[RefNoteID] = po.[NoteID] AND pas.[LineNbr] = pl.[LineNbr] AND pas.[Type] = pl.[POAccrualType]

WHERE pl.[POAccrualType] = 'O' AND pas.[CompanyID] IS NULL AND (apt.[CompanyID] IS NOT NULL OR prl.[CompanyID] IS NOT NULL)
GO

-- Insert into POAccrualStatus all records corresponding to existing released PO Receipt Lines (receipt-based billing)
--[IfExists(Column = POReceiptLine.CuryUnbilledAmt)]
--[IfExists(Column = POReceiptLine.CuryUnbilledDiscountAmt)]
--[IfExists(Column = POReceiptLine.BillPPVAmt)]
--[MinVersion(Hash = 70a4472a9e8bfa24c08cd44db3dc1a022b5e74db4c3f8cd4c822a70beecd56b1)]
INSERT INTO [POAccrualStatus] (
    [CompanyID], [RefNoteID], [LineNbr], [Type],
    [LineType], [OrderType], [OrderNbr], [OrderLineNbr],
    [ReceiptType], [ReceiptNbr],
    [VendorID], [InventoryID], [SubItemID], [SiteID], [AcctID], [SubID],
    [OrigUOM], [OrigQty], [BaseOrigQty],
    [OrigCuryID], [CuryOrigAmt], [OrigAmt],
    [CuryOrigCost], [OrigCost],
    [CuryOrigDiscAmt], [OrigDiscAmt],
    [ReceivedUOM], [ReceivedQty], [BaseReceivedQty],
    [ReceivedCost],
    [BilledUOM], [BilledQty], [BaseBilledQty],
    [BillCuryID], [CuryBilledAmt], [BilledAmt],
    [CuryBilledCost], [BilledCost],
    [CuryBilledDiscAmt], [BilledDiscAmt],
    [PPVAmt],
    [CreatedDateTime], [CreatedByID], [CreatedByScreenID],
    [LastModifiedDateTime], [LastModifiedByID], [LastModifiedByScreenID])
SELECT rl.[CompanyID], r.[NoteID], rl.[LineNbr], rl.[POAccrualType],
    rl.[LineType], rl.[POType], rl.[PONbr], rl.[POLineNbr],
    rl.[ReceiptType], rl.[ReceiptNbr],
    rl.[VendorID], rl.[InventoryID], rl.[SubItemID], rl.[SiteID], rl.[POAccrualAcctID], rl.[POAccrualSubID],
    ol.[UOM], ol.[OrderQty], COALESCE(ol.[BaseOrderQty], 0),
    NULL, COALESCE(ol.[CuryExtCost] + ol.[CuryRetainageAmt], 0), COALESCE(ol.[ExtCost] + ol.[RetainageAmt], 0),
    COALESCE(ol.[CuryExtCost] + ol.[CuryRetainageAmt], 0), COALESCE(ol.[ExtCost] + ol.[RetainageAmt], 0),
    COALESCE(ol.[CuryDiscAmt], 0), COALESCE(ol.[DiscAmt], 0),
    rl.[UOM], rl.[ReceiptQty] * (CASE WHEN rl.[InvtMult] < 0 THEN -1 ELSE 1 END), COALESCE(rl.[BaseReceiptQty], 0) * (CASE WHEN rl.[InvtMult] < 0 THEN -1 ELSE 1 END),
    COALESCE(rl.[TranCostFinal], 0) * (CASE WHEN rl.[InvtMult] < 0 THEN -1 ELSE 1 END),
    rl.[UOM], 0, 0,
    NULL, NULL, 0,
    NULL, 0,
    NULL, 0,
    rl.[BillPPVAmt],
    rl.[CreatedDateTime], rl.[CreatedByID], rl.[CreatedByScreenID],
    rl.[LastModifiedDateTime], rl.[LastModifiedByID], rl.[LastModifiedByScreenID]
FROM [POReceiptLine] rl
INNER JOIN [POReceipt] r ON r.[CompanyID] = rl.[CompanyID] AND r.[ReceiptNbr] = rl.[ReceiptNbr]
LEFT JOIN [POLine] ol ON ol.[CompanyID] = rl.[CompanyID] AND ol.[OrderType] = rl.[POType] AND ol.[OrderNbr] = rl.[PONbr] AND ol.[LineNbr] = rl.[POLineNbr]
LEFT JOIN [POAccrualStatus] pas ON pas.[CompanyID] = rl.[CompanyID] AND pas.[RefNoteID] = r.[NoteID] AND pas.[LineNbr] = rl.[LineNbr] AND pas.[Type] = rl.[POAccrualType]
WHERE rl.[POAccrualType] = 'R' AND pas.[CompanyID] IS NULL AND rl.[ReceiptType] IN ('RT', 'RN') AND rl.[Released] = 1

UPDATE pas
SET pas.[BilledQty] = apt.[BilledQty],
    pas.[BaseBilledQty] = apt.[BaseBilledQty],
    pas.[BilledDiscAmt] = apt.[BilledDiscAmt],
    pas.[BilledAmt] = apt.[BilledAmt],
    pas.[BilledCost] = apt.[BilledCost]
FROM [POAccrualStatus] pas
INNER JOIN (SELECT
    aptn.[CompanyID], aptn.[ReceiptNbr], aptn.[ReceiptLineNbr],
    SUM(aptn.[Qty] * (CASE WHEN aptn.[DrCr] = 'D' THEN 1 ELSE -1 END)) AS BilledQty,
    COALESCE(SUM(COALESCE(aptn.[BaseQty], aptn.[Qty]) * (CASE WHEN aptn.[DrCr] = 'D' THEN 1 ELSE -1 END)), 0) AS BaseBilledQty,
    COALESCE(SUM(aptn.[DiscAmt] * (CASE WHEN aptn.[DrCr] = 'D' THEN 1 ELSE -1 END)), 0) AS BilledDiscAmt,
    COALESCE(SUM((aptn.[TranAmt] + COALESCE(aptn.[RetainageAmt], 0)) * (CASE WHEN aptn.[DrCr] = 'D' THEN 1 ELSE -1 END)), 0) AS BilledAmt,
    COALESCE(SUM((CASE WHEN inclTax.[CompanyID] IS NOT NULL THEN inclTax.[TaxableAmt] + COALESCE(inclTax.[RetainedTaxableAmt], 0) + aptn.[TranAmt] - ROUND(aptn.[TranAmt] * aptn.[GroupDiscountRate] * aptn.[DocumentDiscountRate], COALESCE(cl.[DecimalPlaces], 2))
        ELSE aptn.[TranAmt] + COALESCE(aptn.[RetainageAmt], 0) END) * (CASE WHEN aptn.[DrCr] = 'D' THEN 1 ELSE -1 END)), 0) AS BilledCost
FROM [APTran] aptn
LEFT JOIN (
    SELECT aptx.[CompanyID], aptx.[TranType], aptx.[RefNbr], aptx.[LineNbr], MIN(aptx.[TaxableAmt]) as TaxableAmt, MIN(aptx.[RetainedTaxableAmt]) as RetainedTaxableAmt
    FROM [APTax] aptx
    INNER JOIN [Tax] t ON t.[CompanyID] = aptx.[CompanyID] AND t.[TaxID] = aptx.[TaxID]
    WHERE t.[TaxCalcLevel] = '0' AND t.[TaxType] <> 'W' AND t.[ReverseTax] = 0
    GROUP BY aptx.CompanyID, aptx.TranType, aptx.RefNbr, aptx.LineNbr) inclTax
    ON inclTax.[CompanyID] = aptn.[CompanyID] AND inclTax.[TranType] = aptn.[TranType] AND inclTax.[RefNbr] = aptn.[RefNbr] AND inclTax.[LineNbr] = aptn.[LineNbr]
INNER JOIN [Company] c ON c.[CompanyID] = aptn.[CompanyID]
LEFT JOIN [CurrencyList] cl ON cl.[CompanyID] = c.[CompanyID] AND cl.[CuryID] = c.[BaseCuryID]
WHERE aptn.[Released] = 1 AND aptn.[ReceiptNbr] IS NOT NULL AND aptn.[ReceiptLineNbr] IS NOT NULL
GROUP BY aptn.[CompanyID], aptn.[ReceiptNbr], aptn.[ReceiptLineNbr]) apt
ON apt.[CompanyID] = pas.[CompanyID] AND apt.[ReceiptNbr] = pas.[ReceiptNbr] AND apt.[ReceiptLineNbr] = pas.[LineNbr]
WHERE pas.[Type] = 'R'
GO

--Populate newly added POLine.POAccrualAcctID and POAccrualSubID
--[MinVersion(Hash = f95c2399255bb829f143afdbc10ae2eb400d7ba6ef90593953da659ac4f13163)]
UPDATE ol
SET POAccrualAcctID = rl.POAccrualAcctID, POAccrualSubID = rl.POAccrualSubID
FROM POLine ol
INNER JOIN POReceiptLine rl ON rl.CompanyID = ol.CompanyID AND rl.POType = ol.OrderType AND rl.PONbr = ol.OrderNbr AND rl.POLineNbr = ol.LineNbr
WHERE (ol.POAccrualAcctID IS NULL OR ol.POAccrualSubID IS NULL) and rl.ReceiptType = 'RT'
GO

-- Copy PO Open Qty and Amounts to newly created Unbilled Qty and Amounts
--[IfExists(Column = POLine.CuryUnbilledAmt)]
--[IfExists(Column = POLine.CuryOpenAmt)]
--[IfExists(Column = POLine.UnbilledQty)]
--[IfExists(Column = POLine.OpenQty)]
--[MinVersion(Hash = 7a7ba5a48ccf012ea0c328b957c3d3ca901ea852d27aaa7043cd19a0e8840860)]
UPDATE POLine
SET Closed = Completed,
	CuryUnbilledAmt = CuryOpenAmt,
	UnbilledAmt = OpenAmt,
	UnbilledQty = OpenQty,
	BaseUnbilledQty = BaseOpenQty,
	CompletedQty = OrderQty - OpenQty,
	BaseCompletedQty = BaseOrderQty - BaseOpenQty,
	BilledQty = OrderQty - OpenQty,
	BaseBilledQty = BaseOrderQty - BaseOpenQty,
	CuryBilledAmt = CuryLineAmt - CuryDiscAmt - CuryOpenAmt,
	BilledAmt = LineAmt - DiscAmt - OpenAmt
WHERE Closed IS NULL
	OR CuryUnbilledAmt IS NULL
	OR UnbilledAmt IS NULL
	OR UnbilledQty IS NULL
	OR BaseUnbilledQty IS NULL
	OR CompletedQty IS NULL
	OR BaseCompletedQty IS NULL
	OR BilledQty IS NULL
	OR BaseBilledQty IS NULL
	OR CuryBilledAmt IS NULL
	OR BilledAmt IS NULL
GO

--[IfExists(Column = POOrder.CuryUnbilledOrderTotal)]
--[IfExists(Column = POOrder.CuryOpenOrderTotal)]
--[IfExists(Column = POOrder.UnbilledOrderQty)]
--[IfExists(Column = POOrder.OpenOrderQty)]
--[IfExists(Column = POOrder.IsUnbilledTaxValid)]
--[IfExists(Column = POOrder.IsOpenTaxValid)]
--[MinVersion(Hash = aa7abe7c5ed7b33b4753550f2cf2b22b4a902f2a6e5b3288b9e69b84a34ab3b2)]
UPDATE POOrder
SET CuryUnbilledOrderTotal = CuryOpenOrderTotal,
	UnbilledOrderTotal = OpenOrderTotal,
	CuryUnbilledLineTotal = CuryOpenLineTotal,
	UnbilledLineTotal = OpenLineTotal,
	CuryUnbilledTaxTotal = CuryOpenTaxTotal,
	UnbilledTaxTotal = OpenTaxTotal,
	UnbilledOrderQty = OpenOrderQty,
	IsUnbilledTaxValid = IsOpenTaxValid
WHERE CuryUnbilledOrderTotal IS NULL
	OR UnbilledOrderTotal IS NULL
	OR CuryUnbilledLineTotal IS NULL
	OR UnbilledLineTotal IS NULL
	OR CuryUnbilledTaxTotal  IS NULL
	OR UnbilledTaxTotal IS NULL
	OR UnbilledOrderQty IS NULL
	OR IsUnbilledTaxValid IS NULL
GO

--[IfExists(Column = POTax.CuryUnbilledTaxableAmt)]
--[IfExists(Column = POTax.CuryOpenTaxableAmt)]
--[IfExists(Column = POTax.CuryUnbilledTaxAmt)]
--[IfExists(Column = POTax.CuryOpenTaxAmt)]
--[MinVersion(Hash = 278493805150fd3f930b6df88755eb1d702931630d788fd4853309e803f3554b)]
UPDATE POTax
SET CuryUnbilledTaxableAmt = CuryOpenTaxableAmt,
	UnbilledTaxableAmt = OpenTaxableAmt,
	CuryUnbilledTaxAmt = CuryOpenTaxAmt,
	UnbilledTaxAmt = OpenTaxAmt
WHERE CuryUnbilledTaxableAmt IS NULL
	OR UnbilledTaxableAmt IS NULL
	OR CuryUnbilledTaxAmt IS NULL
	OR UnbilledTaxAmt IS NULL
GO

--[IfExists(Column = POTaxTran.CuryUnbilledTaxableAmt)]
--[IfExists(Column = POTaxTran.CuryOpenTaxableAmt)]
--[IfExists(Column = POTaxTran.CuryUnbilledTaxAmt)]
--[IfExists(Column = POTaxTran.CuryOpenTaxAmt)]
--[MinVersion(Hash = 2352a2018a31e18174f438c009f4e3bd6a4036ff1933347994ead2b9f606a09a)]
UPDATE POTaxTran
SET CuryUnbilledTaxableAmt = CuryOpenTaxableAmt,
	UnbilledTaxableAmt = OpenTaxableAmt,
	CuryUnbilledTaxAmt = CuryOpenTaxAmt,
	UnbilledTaxAmt = OpenTaxAmt
WHERE CuryUnbilledTaxableAmt IS NULL
	OR UnbilledTaxableAmt IS NULL
	OR CuryUnbilledTaxAmt IS NULL
	OR UnbilledTaxAmt IS NULL
GO

--[IfExists(Column = POLine.CuryReceivedCost)]
--[IfExists(Column = POLine.ReceivedCost)]
--[MinVersion(Hash = 7cf5e61f6975af1c82a16777ccd43082880d2c0741b237fda8be3c36b276ccdc)]
UPDATE POLine
SET CuryBLOrderedCost = CuryReceivedCost, BLOrderedCost = ReceivedCost
WHERE OrderType = 'BL'
GO

-- Insert Landed Costs Numbering and update PO Setup
--[MinVersion(Hash = 47fa2cc1dd77f0c171f2fb0c614c44791363917e7f04ccdbbb8d343ebe994b43)]
INSERT INTO [Numbering] ([CompanyID]
, [NumberingID]
, [Descr]
, [UserNumbering]
, [NewSymbol]
, [CreatedByID]
, [CreatedByScreenID]
, [CreatedDateTime]
, [LastModifiedByID]
, [LastModifiedByScreenID]
, [LastModifiedDateTime]
, [NoteID])
  SELECT [C].[CompanyID]
        ,'POLANDCOST' AS [NumberingID]
        ,'PO Landed Costs' AS [Descr]
        ,0 AS [UserNumbering]
        ,'<NEW>' AS [NewSymbol]
        ,'00000000-0000-0000-0000-000000000000' AS [CreatedByID]
        ,'CS201010' AS [CreatedByScreenID]
        ,GETDATE() AS [CreatedDateTime]
        ,'00000000-0000-0000-0000-000000000000' AS [LastModifiedByID]
        ,'CS201010' AS [LastModifiedByScreenID]
        ,GETDATE() AS [LastModifiedDateTime]
        ,NEWID() AS [NoteID]
  FROM [POSetup] [C] 
  LEFT JOIN [Numbering] [N] ON [C].[CompanyID] = [N].[CompanyID] AND [N].[NumberingID] = 'POLANDCOST'
  WHERE [N].[CompanyID] IS NULL
  

INSERT INTO [NumberingSequence] ([CompanyID]
, [NumberingID]
, [NBranchID]
, [StartNbr]
, [EndNbr]
, [StartDate]
, [LastNbr]
, [WarnNbr]
, [NbrStep]
, [CreatedByID]
, [CreatedByScreenID]
, [CreatedDateTime]
, [LastModifiedByID]
, [LastModifiedByScreenID]
, [LastModifiedDateTime]
, [NumberingSeq])
  SELECT [C].[CompanyID]
        ,'POLANDCOST' AS [NumberingID]
        ,NULL AS [NBranchID]
        ,'LC000000' AS [StartNbr]
        ,'LC999999' AS [EndNbr]
        ,'1900-01-01 00:00:00' AS [StartDate]
        ,'LC000000' AS [LastNbr]
        ,'LC999990' AS [WarnNbr]
        ,1 AS [NbrStep]
        ,'00000000-0000-0000-0000-000000000000' AS [CreatedByID]
        ,'CS201010' AS [CreatedByScreenID]
        ,GETDATE() AS [CreatedDateTime]
        ,'00000000-0000-0000-0000-000000000000' AS [LastModifiedByID]
        ,'CS201010' AS [LastModifiedByScreenID]
        ,GETDATE() AS [LastModifiedDateTime]
        ,'10010' AS [NumberingSeq]
  FROM [POSetup] [C]
  LEFT JOIN [NumberingSequence] [N] ON [C].[CompanyID] = [N].[CompanyID] AND [N].[NumberingID] = 'POLANDCOST'
  WHERE [N].[CompanyID] IS NULL
  
UPDATE [POSetup]
SET [LandedCostDocNumberingID] = 'POLANDCOST'
WHERE [LandedCostDocNumberingID] IS NULL
GO

--[mysql: Skip]
--[mssql: Native]
--[IfExists(Table = LandedCostTran)]
--[IfExists(Table = LandedCostTranSplit)]
--[IfExists(Column = POReceipt.TaxZoneID)]
--[IfNotExistsSelect(From = POLandedCostDoc)]
-- Create Landed Costs Documents
--[OldHash(Hash = 09ce77c1f2ac71287b56200affee43827db62bda3c0a746e789f392e64e4c6b5)]
--[OldHash(Hash = cfeffcf306d019abad4dea6bbc9e490fc5598d873262bba8f8d97dba5bd3a408)]
--[MinVersion(Hash = e9e47c969a6522fd2d707f05bed06f7cd16c254920b7ea8f597c0a076a1005c8)]
DECLARE @companyID INT
       ,@branchId INT
       ,@poReceiptType CHAR(3)
       ,@poReceiptNbr NVARCHAR(15)
       ,@poReleased BIT
       ,@apDocType CHAR(3)
       ,@apRefNbr VARCHAR(15)
       ,@apReleased BIT
       ,@apHold BIT
       ,@apCreated BIT
       ,@inCreated BIT
       ,@curyID NVARCHAR(5)
       ,@curyInfoID BIGINT
       ,@postponeAP BIT
       ,@tranSource CHAR(2)
       ,@ownerID UNIQUEIDENTIFIER
       ,@noteID UNIQUEIDENTIFIER
       ,@termsID NVARCHAR(10)
       ,@invoiceDate DATETIME
       ,@invoiceNbr NVARCHAR(40)
       ,@vendorID INT
       ,@vendorLocationID NVARCHAR(40)
       ,@createdByID UNIQUEIDENTIFIER
       ,@createdByScreenID CHAR(8)
       ,@createdDateTime DATETIME
       ,@lastModifiedByID UNIQUEIDENTIFIER
       ,@lastModifiedByScreenID CHAR(8)
       ,@lastModifiedDateTime DATETIME
       ,@finPeriodID CHAR(6)
       ,@lcDocType CHAR(3)
       ,@lcRefNbr NVARCHAR(15)
       ,@lcRefCntr INT
       ,@curyInfoCuryID NVARCHAR(5)
       ,@curyInfoCuryMultDiv CHAR(1)
       ,@curyInfoCuryRate DECIMAL(18, 9)
       ,@curyInfoBaseCuryID NVARCHAR(5)
       ,@splitTranID INT
       ,@taxZoneID NVARCHAR(10)
       ,@isPOTranSource BIT
       ,@curyLineTotal DECIMAL(19, 4)
       ,@lineTotal DECIMAL(19, 4)
       ,@curyAllocatedTotal DECIMAL(19, 4)
       ,@allocatedTotal DECIMAL(19, 4)
       ,@curyTaxTotal DECIMAL(19, 4)
       ,@taxTotal DECIMAL(19, 4);

UPDATE [LCT]
SET [LCT].[TermsID] = [API].[TermsID]
FROM [LandedCostTran] [LCT]
INNER JOIN [APInvoice] [API] ON [LCT].[CompanyID] = [API].[CompanyID] AND [LCT].[APDocType] = [API].[DocType] AND [LCT].[APRefNbr] = [API].[RefNbr]
WHERE [LCT].[Source] = 'AP'

DECLARE lcDoc_cursor CURSOR FOR SELECT
  [LCT].[CompanyID]
 ,CASE WHEN [LCT].[Source] = 'PO' THEN [POR].[BranchID] ELSE [APR].[BranchID] END AS BranchID 
 ,[LCT].[POReceiptType]
 ,[LCT].[POReceiptNbr]
 ,ISNULL([POR].[Released], 0) AS [POReleased]
 ,[LCT].[APDocType]
 ,[LCT].[APRefNbr]
 ,ISNULL([APR].[Released], 0) AS [APReleased]
 ,[LCT].[CuryID]
 ,[CI].[CuryID]
 ,[CI].[CuryMultDiv]
 ,[CI].[CuryRate]
 ,[CI].[BaseCuryID]
 ,MAX([LCT].[CuryInfoID]) AS [CuryInfoID]
 ,[LCT].[Source]
 ,[LCT].[PostponeAP]
 ,[LCT].[TermsID]
 ,[LCT].[VendorID]
 ,[LCT].[VendorLocationID]
 ,[LCT].[InvoiceDate]
 ,[FP].[FinPeriodID]
 ,[LCT].[InvoiceNbr]
 ,[LCTS].[LCTranID] AS [SplitTranID]
 ,CASE WHEN [LCT].[Source] = 'PO' THEN [POR].[TaxZoneID] ELSE [API].[TaxZoneID] END AS [TaxZoneID]
 ,MAX([LCT].[CreatedByID]) AS [CreatedByID]
 ,MAX([LCT].[CreatedByScreenID]) AS [CreatedByScreenID]
 ,MAX([LCT].[CreatedDateTime]) AS [CreatedDateTime]
 ,MAX([LCT].[LastModifiedByID]) AS [LastModifiedByID]
 ,MAX([LCT].[LastModifiedByScreenID]) AS [LastModifiedByScreenID]
 ,MAX([LCT].[LastModifiedDateTime]) AS [LastModifiedDateTime]
FROM [LandedCostTran] [LCT]
  INNER JOIN [CurrencyInfo] [CI] ON [LCT].[CompanyID] = [CI].[CompanyID] AND [LCT].[CuryInfoID] = [CI].[CuryInfoID]
  INNER JOIN [FinPeriod] [FP] ON [LCT].[CompanyID] = [FP].[CompanyID] AND [FP].[StartDate] <= [LCT].[InvoiceDate] AND [FP].[EndDate] > [LCT].[InvoiceDate]
  LEFT JOIN [APRegister] [APR] ON [LCT].[CompanyID] = [APR].[CompanyID] AND [LCT].[APDocType] = [APR].[DocType] AND [LCT].[APRefNbr] = [APR].[RefNbr]
  LEFT JOIN [APInvoice] [API] ON [LCT].[CompanyID] = [API].[CompanyID] AND [LCT].[APDocType] = [API].[DocType] AND [LCT].[APRefNbr] = [API].[RefNbr]
  LEFT JOIN [POReceipt] [POR] ON [LCT].[CompanyID] = [POR].[CompanyID] AND [LCT].[POReceiptType] = [POR].[ReceiptType] AND [LCT].[POReceiptNbr] = [POR].[ReceiptNbr]
  LEFT JOIN [LandedCostTranSplit] [LCTS] ON [LCT].[CompanyID] = [LCTS].[CompanyID] AND [LCT].[LCTranID] = [LCTS].[LCTranID]
WHERE ([LCT].[Source] = 'PO' AND [POR].[CompanyID] IS NOT NULL) OR ([LCT].[Source] = 'AP' AND [APR].[CompanyID] IS NOT NULL)
GROUP BY [LCT].[CompanyID]
        ,[POR].[BranchID]
        ,[APR].[BranchID]
        ,[LCT].[POReceiptType]
        ,[LCT].[POReceiptNbr]
        ,[POR].[Released]
        ,[LCT].[APDocType]
        ,[LCT].[APRefNbr]
        ,[APR].[Released]
        ,[LCT].[CuryID]
        ,[LCT].[Source]
        ,[LCT].[PostponeAP]
        ,[LCT].[TermsID]
        ,[LCT].[VendorID]
        ,[LCT].[VendorLocationID]
        ,[LCT].[InvoiceDate]
        ,[FP].[FinPeriodID]
        ,[LCT].[InvoiceNbr]
        ,[CI].[CuryID]
        ,[CI].[CuryMultDiv]
        ,[CI].[CuryRate]
        ,[CI].[BaseCuryID]
        ,[LCTS].[LCTranID]
        ,[POR].[TaxZoneID]
        ,[API].[TaxZoneID]
ORDER BY [LCT].[InvoiceDate], [LCT].[POReceiptNbr], [LCT].[APRefNbr]
OPEN lcDoc_cursor

FETCH NEXT FROM lcDoc_cursor
INTO @companyID
, @branchId
, @poReceiptType
, @poReceiptNbr
, @poReleased
, @apDocType
, @apRefNbr
, @apReleased
, @curyID
, @curyInfoCuryID
, @curyInfoCuryMultDiv
, @curyInfoCuryRate
, @curyInfoBaseCuryID
, @curyInfoID
, @tranSource
, @postponeAP
, @termsID
, @vendorID
, @vendorLocationID
, @invoiceDate
, @finPeriodID
, @invoiceNbr
, @splitTranID
, @taxZoneID
, @createdByID
, @createdByScreenID
, @createdDateTime
, @lastModifiedByID
, @lastModifiedByScreenID
, @lastModifiedDateTime

WHILE @@FETCH_STATUS = 0
BEGIN
SET @lcDocType = 'L'
SELECT @lcRefCntr = ISNULL(MAX(CAST(SUBSTRING([RefNbr], 3, 6) AS INT)), 0) + 1
FROM [POLandedCostDoc]
WHERE CompanyID = @companyID

SET @lcRefNbr = CONVERT(NVARCHAR, @lcRefCntr)
SET @lcRefNbr = CASE 
WHEN @lcRefCntr < 10 THEN '00000' + @lcRefNbr
WHEN @lcRefCntr < 100 THEN '0000' + @lcRefNbr
WHEN @lcRefCntr < 1000 THEN '000' + @lcRefNbr
WHEN @lcRefCntr < 10000 THEN '00' + @lcRefNbr
WHEN @lcRefCntr < 100000 THEN '0' + @lcRefNbr
ELSE @lcRefNbr END
SET @lcRefNbr = 'LC' + @lcRefNbr

INSERT INTO [POLandedCostDetail] ([CompanyID]
, [DocType]
, [RefNbr]
, [LineNbr]
, [SortOrder]
, [BranchID]
, [LandedCostCodeID]
, [Descr]
, [AllocationMethod]
, [CuryInfoID]
, [LineAmt]
, [CuryLineAmt]
, [TaxCategoryID]
, [InventoryID]
, [SubItemID]
, [LCAccrualAcct]
, [LCAccrualSub]
, [APDocType]
, [APRefNbr]
, [INDocType]
, [INRefNbr]
, [CreatedByID]
, [CreatedByScreenID]
, [CreatedDateTime]
, [LastModifiedByID]
, [LastModifiedByScreenID]
, [LastModifiedDateTime]
,[NoteID])
  SELECT @companyID AS [CompanyID]
        ,@lcDocType AS [DocType]
        ,@lcRefNbr AS [RefNbr]
        ,[LCT].[LCTranID] AS [LineNbr]
        ,0 AS [SortOrder]
        ,@branchId AS [BranchID]
        ,[LCT].[LandedCostCodeID] AS [LandedCostCodeID]
        ,[LCT].[Descr] AS [Descr]
        ,[LCC].[AllocationMethod] AS [AllocationMethod]
        ,[LCT].[CuryInfoID] AS [CuryInfoID]
        ,[LCT].[LCAmount] AS [LineAmt]
        ,[LCT].[CuryLCAmount] AS [CuryLineAmt]
        ,[LCT].[TaxCategoryID] AS [TaxCategoryID]
        ,[LCT].[InventoryID] AS [InventoryID]
        ,NULL AS [SubItemID]
        ,[LCT].[LCAccrualAcct] AS [LCAccrualAcct]
        ,[LCT].[LCAccrualSub] AS [LCAccrualSub]
        ,[LCT].[APDocType] AS [APDocType]
        ,[LCT].[APRefNbr] AS [APRefNbr]
        ,[LCT].[INDocType] AS [INDocType]
        ,[LCT].[INRefNbr] AS [INRefNbr]
        ,[LCT].[CreatedByID] AS [CreatedByID]
        ,[LCT].[CreatedByScreenID] AS [CreatedByScreenID]
        ,[LCT].[CreatedDateTime] AS [CreatedDateTime]
        ,[LCT].[LastModifiedByID] AS [LastModifiedByID]
        ,[LCT].[LastModifiedByScreenID] AS [LastModifiedByScreenID]
        ,[LCT].[LastModifiedDateTime] AS [LastModifiedDateTime]
        ,NEWID() AS [NoteID]
  FROM [LandedCostTran] [LCT]
  INNER JOIN [LandedCostCode] [LCC] ON [LCT].[CompanyID] = [LCC].[CompanyID] AND [LCT].[LandedCostCodeID] = [LCC].[LandedCostCodeID]
  INNER JOIN [CurrencyInfo] [CI] ON [LCT].[CompanyID] = [CI].[CompanyID] AND [LCT].[CuryInfoID] = [CI].[CuryInfoID]
  WHERE [LCT].[CompanyID] = @companyID
  AND ([LCT].[POReceiptType] = @poReceiptType OR ([LCT].[POReceiptType] IS NULL AND @poReceiptType IS NULL))
  AND ([LCT].[POReceiptNbr] = @poReceiptNbr OR ([LCT].[POReceiptNbr] IS NULL AND @poReceiptNbr IS NULL))
  AND ([LCT].[APDocType] = @apDocType OR ([LCT].[APDocType] IS NULL AND @apDocType IS NULL))
  AND ([LCT].[APRefNbr] = @apRefNbr OR ([LCT].[APRefNbr] IS NULL AND @apRefNbr IS NULL))
  AND [LCT].[CuryID] = @curyID
  AND [LCT].[Source] = @tranSource
  AND [LCT].[PostponeAP] = @postponeAP
  AND ([LCT].[TermsID] = @termsID OR [LCT].[TermsID] IS NULL AND @termsID IS NULL)
  AND [LCT].[VendorID] = @vendorID
  AND [LCT].[VendorLocationID] = @vendorLocationID
  AND [LCT].[InvoiceDate] = @invoiceDate
  AND [CI].[CuryID] = @curyInfoCuryID
  AND [CI].[CuryMultDiv] = @curyInfoCuryMultDiv
  AND [CI].[CuryRate] = @curyInfoCuryRate
  AND [CI].[BaseCuryID] = @curyInfoBaseCuryID
  AND ([LCT].[LCTranID] = @splitTranID OR @splitTranID IS NULL)
  AND (([LCT].[InvoiceNbr] IS NULL AND @invoiceNbr IS NULL) OR [LCT].[InvoiceNbr] = @invoiceNbr)

INSERT INTO [POLandedCostReceiptLine] ([CompanyID]
, [DocType]
, [RefNbr]
, [LineNbr]
, [SortOrder]
, [BranchID]
, [InventoryID]
, [SubItemID]
, [TranDesc]
, [CuryInfoID]
, [POReceiptType]
, [POReceiptNbr]
, [POReceiptLineNbr]
, [SiteID]
, [UOM]
, [ReceiptQty]
, [BaseReceiptQty]
, [UnitWeight]
, [UnitVolume]
, [ExtWeight]
, [ExtVolume]
, [LineAmt]
, [CuryAllocatedLCAmt]
, [AllocatedLCAmt]
, [CreatedByID]
, [CreatedByScreenID]
, [CreatedDateTime]
, [LastModifiedByID]
, [LastModifiedByScreenID]
, [LastModifiedDateTime])
  SELECT [RL].[CompanyID] AS [CompanyID]
        ,@lcDocType AS [DocType]
        ,@lcRefNbr AS [DocType]
        ,COALESCE([LCTS].[SplitLineNbr], 0) * 10000 + [RL].[LineNbr] AS [LineNbr]
        ,[RL].[SortOrder] AS [SortOrder]
        ,@branchId AS [BranchID]
        ,[RL].[InventoryID] AS [InventoryID]
        ,[RL].[SubItemID] AS [SubItemID]
        ,[RL].[TranDesc] AS [TranDesc]
        ,@curyInfoID AS [CuryInfoID]
        ,[RL].[ReceiptType] AS [POReceiptType]
        ,[RL].[ReceiptNbr] AS [POReceiptNbr]
        ,[RL].[LineNbr] AS [POReceiptLineNbr]
        ,[RL].[SiteID] AS [SiteID]
        ,[RL].[UOM] AS [UOM]
        ,[RL].[ReceiptQty] AS [ReceiptQty]
        ,[RL].[BaseReceiptQty] AS [BaseReceiptQty]
        ,[RL].[UnitWeight] AS [UnitWeight]
        ,[RL].[UnitVolume] AS [UnitVolume]
        ,[RL].[ExtWeight] AS [ExtWeight]
        ,[RL].[ExtVolume] AS [ExtVolume]
        ,[RL].[TranCostFinal] AS [LineAmt]
        ,0 AS [CuryAllocatedLCAmt]
        ,0 AS [AllocatedLCAmt]
        ,MAX([LCT].[CreatedByID]) AS [CreatedByID]
        ,MAX([LCT].[CreatedByScreenID]) AS [CreatedByScreenID]
        ,MAX([LCT].[CreatedDateTime]) AS [CreatedDateTime]
        ,MAX([LCT].[LastModifiedByID]) AS [LastModifiedByID]
        ,MAX([LCT].[LastModifiedByScreenID]) AS [LastModifiedByScreenID]
        ,MAX([LCT].[LastModifiedDateTime]) AS [LastModifiedDateTime]
  FROM [LandedCostTran] [LCT]
  LEFT JOIN [LandedCostTranSplit] [LCTS] ON [LCT].[CompanyID] = [LCTS].[CompanyID] AND [LCT].[LCTranID] = [LCTS].[LCTranID]
  INNER JOIN [POReceiptLine] [RL] ON [RL].[CompanyID] = [LCT].[CompanyID] AND (([RL].[ReceiptNbr] = [LCT].[POReceiptNbr] AND [LCTS].[POReceiptNbr] IS NULL) OR ([RL].[ReceiptNbr] = [LCTS].[POReceiptNbr] AND ([RL].[InventoryID] = [LCTS].[InventoryID] OR [LCTS].[InventoryID] IS NULL)))
  INNER JOIN [CurrencyInfo] [CI] ON [LCT].[CompanyID] = [CI].[CompanyID] AND [LCT].[CuryInfoID] = [CI].[CuryInfoID]
  WHERE [LCT].[CompanyID] = @companyID
  AND ([LCT].[POReceiptType] = @poReceiptType OR ([LCT].[POReceiptType] IS NULL AND @poReceiptType IS NULL))
  AND ([LCT].[POReceiptNbr] = @poReceiptNbr OR ([LCT].[POReceiptNbr] IS NULL AND @poReceiptNbr IS NULL))
  AND ([LCT].[APDocType] = @apDocType OR ([LCT].[APDocType] IS NULL AND @apDocType IS NULL))
  AND ([LCT].[APRefNbr] = @apRefNbr OR ([LCT].[APRefNbr] IS NULL AND @apRefNbr IS NULL))
  AND [LCT].[CuryID] = @curyID
  AND [LCT].[Source] = @tranSource
  AND [LCT].[PostponeAP] = @postponeAP
  AND ([LCT].[TermsID] = @termsID OR [LCT].[TermsID] IS NULL AND @termsID IS NULL)
  AND [LCT].[VendorID] = @vendorID
  AND [LCT].[VendorLocationID] = @vendorLocationID
  AND [LCT].[InvoiceDate] = @invoiceDate
  AND [CI].[CuryID] = @curyInfoCuryID
  AND [CI].[CuryMultDiv] = @curyInfoCuryMultDiv
  AND [CI].[CuryRate] = @curyInfoCuryRate
  AND [CI].[BaseCuryID] = @curyInfoBaseCuryID
  AND ([LCT].[LCTranID] = @splitTranID OR @splitTranID IS NULL)
  AND (([LCT].[InvoiceNbr] IS NULL AND @invoiceNbr IS NULL) OR [LCT].[InvoiceNbr] = @invoiceNbr)
  AND [RL].[LineType] NOT IN ('SV', 'FT')
  GROUP BY [RL].[CompanyID]
          ,[RL].[LineNbr]
          ,[RL].[SortOrder]
          ,[RL].[InventoryID]
          ,[RL].[SubItemID]
          ,[RL].[TranDesc]
          ,[RL].[ReceiptType]
          ,[RL].[ReceiptNbr]
          ,[RL].[LineNbr]
          ,[RL].[SiteID]
          ,[RL].[UOM]
          ,[RL].[ReceiptQty]
          ,[RL].[BaseReceiptQty]
          ,[RL].[UnitWeight]
          ,[RL].[UnitVolume]
          ,[RL].[ExtWeight]
          ,[RL].[ExtVolume]
          ,[RL].[TranCostFinal]
          ,[LCTS].[SplitLineNbr]

INSERT INTO [POLandedCostSplit] ([CompanyID]
, [DocType]
, [RefNbr]
, [DetailLineNbr]
, [ReceiptLineNbr]
, [CuryInfoID]
, [CuryLineAmt]
, [LineAmt]
, [NoteID]
, [CreatedByID]
, [CreatedByScreenID]
, [CreatedDateTime]
, [LastModifiedByID]
, [LastModifiedByScreenID]
, [LastModifiedDateTime])
  SELECT [LCD].[CompanyID] AS [CompanyID]
        ,[LCD].[DocType] AS [DocType]
        ,[LCD].[RefNbr] AS [RefNbr]
        ,[LCD].[LineNbr] AS [DetailLineNbr]
        ,[LCRL].[LineNbr] AS [ReceiptLineNbr]
        ,[LCD].[CuryInfoID] AS [CuryInfoID]
        ,SUM(CASE WHEN [LCD].[LineAmt] <> 0 THEN COALESCE([IT].[TranCost] * [LCD].[CuryLineAmt] / [LCD].[LineAmt], 0) ELSE 0 END) AS [CuryLineAmt]
        ,SUM(COALESCE([IT].[TranCost], 0)) AS [LineAmt]
        ,NEWID() AS [NoteID]
        ,[LCD].[CreatedByID] AS [CreatedByID]
        ,[LCD].[CreatedByScreenID] AS [CreatedByScreenID]
        ,[LCD].[CreatedDateTime] AS [CreatedDateTime]
        ,[LCD].[LastModifiedByID] AS [LastModifiedByID]
        ,[LCD].[LastModifiedByScreenID] AS [LastModifiedByScreenID]
        ,[LCD].[LastModifiedDateTime] AS [LastModifiedDateTime]
  FROM [POLandedCostDetail] [LCD]
  INNER JOIN [POLandedCostReceiptLine] [LCRL]
    ON [LCD].[CompanyID] = [LCRL].[CompanyID]
      AND [LCD].[DocType] = [LCRL].[DocType]
      AND [LCD].[RefNbr] = [LCRL].[RefNbr]
  LEFT JOIN [INTran] [IT]
    ON [IT].[CompanyID] = [LCD].[CompanyID]
      AND [IT].[DocType] = [LCD].[INDocType]
      AND [IT].[RefNbr] = [LCD].[INRefNbr]
      AND [IT].[POReceiptNbr] = [LCRL].[POReceiptNbr]
      AND [IT].[POReceiptLineNbr] = [LCRL].[POReceiptLineNbr]
  WHERE [LCD].[CompanyID] = @companyID
  AND [LCD].[DocType] = @lcDocType
  AND [LCD].[RefNbr] = @lcRefNbr
  GROUP BY [LCD].[CompanyID]
          ,[LCD].[DocType]
          ,[LCD].[RefNbr]
          ,[LCD].[LineNbr]
          ,[LCRL].[LineNbr]
          ,[LCRL].[LineAmt]
          ,[LCD].[CuryLineAmt]
          ,[LCD].[LineAmt]
          ,[LCD].[CuryInfoID]
          ,[LCD].[CreatedByID]
          ,[LCD].[CreatedByScreenID]
          ,[LCD].[CreatedDateTime]
          ,[LCD].[LastModifiedByID]
          ,[LCD].[LastModifiedByScreenID]
          ,[LCD].[LastModifiedDateTime]

UPDATE [LCRL]
SET [LCRL].[CuryAllocatedLCAmt] = (SELECT
        SUM([LCS].[CuryLineAmt])
      FROM [POLandedCostSplit] [LCS]
      WHERE [LCRL].[CompanyID] = [LCS].[CompanyID]
      AND [LCRL].[DocType] = [LCS].[DocType]
      AND [LCRL].[RefNbr] = [LCS].[RefNbr]
      AND [LCRL].[LineNbr] = [LCS].[ReceiptLineNbr])
   ,[LCRL].[AllocatedLCAmt] = (SELECT
        SUM([LCS].[LineAmt])
      FROM [POLandedCostSplit] [LCS]
      WHERE [LCRL].[CompanyID] = [LCS].[CompanyID]
      AND [LCRL].[DocType] = [LCS].[DocType]
      AND [LCRL].[RefNbr] = [LCS].[RefNbr]
      AND [LCRL].[LineNbr] = [LCS].[ReceiptLineNbr])
FROM [POLandedCostReceiptLine] [LCRL]
WHERE [LCRL].[CompanyID] = @companyID
AND [LCRL].[DocType] = @lcDocType
AND [LCRL].[RefNbr] = @lcRefNbr

SET @curyLineTotal = (SELECT SUM(COALESCE([LCRL].[CuryLineAmt], 0))
    FROM [POLandedCostDetail] [LCRL]
    WHERE [LCRL].[CompanyID] = @companyID AND [LCRL].[DocType] = @lcDocType AND [LCRL].[RefNbr] = @lcRefNbr)

SET @lineTotal = (SELECT SUM(COALESCE([LCRL].[LineAmt], 0))
    FROM [POLandedCostDetail] [LCRL]
    WHERE [LCRL].[CompanyID] = @companyID AND [LCRL].[DocType] = @lcDocType AND [LCRL].[RefNbr] = @lcRefNbr)

SET @curyAllocatedTotal = (SELECT SUM(COALESCE([LCRL].[CuryAllocatedLCAmt], 0))
    FROM [POLandedCostReceiptLine] [LCRL]
    WHERE [LCRL].[CompanyID] = @companyID AND [LCRL].[DocType] = @lcDocType AND [LCRL].[RefNbr] = @lcRefNbr)

SET @allocatedTotal = (SELECT SUM(COALESCE([LCRL].[AllocatedLCAmt], 0))
    FROM [POLandedCostReceiptLine] [LCRL]
    WHERE [LCRL].[CompanyID] = @companyID AND [LCRL].[DocType] = @lcDocType AND [LCRL].[RefNbr] = @lcRefNbr)

INSERT INTO [POLandedCostDoc] ([CompanyID]
, [DocType]
, [RefNbr]
, [BranchID]
, [OpenDoc]
, [Released]
, [Hold]
, [Status]
, [IsTaxValid]
, [NonTaxable]
, [DocDate]
, [TranPeriodID]
, [FinPeriodID]
, [VendorID]
, [VendorLocationID]
, [VendorRefNbr]
, [LineCntr]
, [CuryID]
, [CuryInfoID]
, [CreateBill]
, [CuryLineTotal]
, [LineTotal]
, [CuryDocTotal]
, [DocTotal]
, [CuryTaxTotal]
, [TaxTotal]
, [CuryAllocatedTotal]
, [AllocatedTotal]
, [VatTaxableTotal]
, [CuryVatTaxableTotal]
, [VatExemptTotal]
, [CuryVatExemptTotal]
, [CuryControlTotal]
, [ControlTotal]
, [TermsID]
, [BillDate]
, [DueDate]
, [DiscDate]
, [CuryDiscAmt]
, [DiscAmt]
, [TaxZoneID]
, [PayToVendorID]
, [WorkgroupID]
, [OwnerID]
, [APDocCreated]
, [INDocCreated]
, [NoteID]
, [CreatedByID]
, [CreatedByScreenID]
, [CreatedDateTime]
, [LastModifiedByID]
, [LastModifiedByScreenID]
, [LastModifiedDateTime])
  VALUES (@companyID, @lcDocType, @lcRefNbr, @branchId, 1 --<OpenDoc, bit,>
  , 0 --<Released, bit,>
  , 0 --<Hold, bit,>
  , 'H' --<Status, char(1),>
  , 0 --<IsTaxValid, bit,>
  , 0 --<NonTaxable, bit,>
  , @invoiceDate --<DocDate, datetime2(0),>
  , @finPeriodID --<TranPeriodID, char(6),>
  , @finPeriodID --<FinPeriodID, char(6),>
  , @vendorID --<VendorID, int,>
  , @vendorLocationID --<VendorLocationID, int,>
  , COALESCE(@invoiceNbr, '') --<VendorRefNbr, nvarchar(40),>
  , 0 --<LineCntr, int,>
  , @curyID, @curyInfoID, 0 --<CreateBill, bit,>
  , COALESCE(@curyLineTotal, 0) --<CuryLineTotal, decimal(19,4),>
  , COALESCE(@lineTotal, 0) --<LineTotal, decimal(19,4),>
  , COALESCE(@curyLineTotal, 0) + COALESCE(@curyTaxTotal, 0) --<CuryDocTotal, decimal(19,4),>
  , COALESCE(@lineTotal, 0) + COALESCE(@taxTotal, 0) --<DocTotal, decimal(19,4),>
  , COALESCE(@curyTaxTotal, 0) --<CuryTaxTotal, decimal(19,4),>
  , COALESCE(@taxTotal, 0) --<TaxTotal, decimal(19,4),>
  , COALESCE(@curyAllocatedTotal, 0) --<CuryAllocatedTotal, decimal(19,4),>
  , COALESCE(@allocatedTotal, 0) --<AllocatedTotal, decimal(19,4),>
  , 0 --<VatTaxableTotal, decimal(19,4),>
  , 0 --<CuryVatTaxableTotal, decimal(19,4),>
  , 0 --<VatExemptTotal, decimal(19,4),>
  , 0 --<CuryVatExemptTotal, decimal(19,4),>
  , COALESCE(@curyLineTotal, 0) + COALESCE(@curyTaxTotal, 0) --<CuryControlTotal, decimal(19,4),>
  , COALESCE(@lineTotal, 0) + COALESCE(@taxTotal, 0) --<ControlTotal, decimal(19,4),>
  , @termsID --<TermsID, nvarchar(10),>
  , @invoiceDate --<BillDate, datetime2(0),>
  , @invoiceDate --<DueDate, datetime2(0),>
  , @invoiceDate --<DiscDate, datetime2(0),>
  , 0 --<CuryDiscAmt, decimal(19,4),>
  , 0 --<DiscAmt, decimal(19,4),>
  , @taxZoneID --<TaxZoneID, nvarchar(10),>
  , @vendorID --<PayToVendorID, int,>
  , NULL --<WorkgroupID, int,>
  , NULL --<OwnerID, uniqueidentifier,>
  , 0 --<APDocCreated, bit,>
  , 0 --<INDocCreated, bit,>
  , NEWID()--<NoteID, uniqueidentifier,>
  , @createdByID, @createdByScreenID, @createdDateTime, @lastModifiedByID, @lastModifiedByScreenID, @lastModifiedDateTime)

SELECT @apCreated = COALESCE(MIN(CASE WHEN [LCD].[APRefNbr] IS NULL THEN 0 ELSE 1 END), 0)
      ,@inCreated = COALESCE(MIN(CASE WHEN [LCD].[INRefNbr] IS NULL AND [LCC].[AllocationMethod] <> 'N' THEN 0 ELSE 1 END), 0)
      ,@apReleased = COALESCE(MIN(CASE WHEN COALESCE([AP].[Released], 0) = 0 THEN 0 ELSE 1 END), 0)
      ,@apHold = COALESCE(MIN(CASE WHEN COALESCE([AP].[Hold], 0) = 0 THEN 0 ELSE 1 END), 0)
    FROM [POLandedCostDetail] [LCD]
        INNER JOIN [LandedCostCode] [LCC] ON [LCD].[CompanyID] = [LCC].[CompanyID] AND [LCD].[LandedCostCodeID] = [LCC].[LandedCostCodeID]
        LEFT JOIN [APRegister] [AP] ON [AP].[CompanyID] = [LCD].[CompanyID] AND [AP].[DocType] = [LCD].[APDocType] AND [AP].[RefNbr] = [LCD].[APRefNbr]
    WHERE [LCD].[CompanyID] = @companyID AND [LCD].[DocType] = @lcDocType AND [LCD].[RefNbr] = @lcRefNbr

SET @isPOTranSource = CASE WHEN @tranSource = 'PO' THEN 1 ELSE 0 END

UPDATE [POLandedCostDoc]
    SET Released = CASE WHEN (@poReleased = 1 AND @apCreated = 1 AND @isPOTranSource = 1 AND @inCreated = 1) 
                          OR (@apCreated = 1 AND @apReleased = 1 AND @isPOTranSource = 0 AND @inCreated = 1) 
                          OR (@poReleased = 1 AND @postponeAP = 1 AND @isPOTranSource = 1 AND @inCreated = 1) THEN 1 ELSE 0 END
       ,CreateBill = CASE WHEN (@poReleased = 1 AND @apCreated = 1 AND @isPOTranSource = 1 AND @postponeAP = 0) 
                            OR (@poReleased = 0 AND @apCreated = 0 AND @isPOTranSource = 1 AND @postponeAP = 0) THEN 1 ELSE 0 END
       ,Hold = CASE WHEN 
           NOT ((@poReleased = 1 AND @apCreated = 1 AND @isPOTranSource = 1) 
             OR (@apCreated = 1 AND @apReleased = 1 AND @isPOTranSource = 0 AND @inCreated = 1) 
             OR (@poReleased = 1 AND @postponeAP = 1 AND @isPOTranSource = 1))
           AND (@isPOTranSource = 1 OR (@isPOTranSource = 0 AND @apHold = 1)) THEN 1 ELSE 0 END
       ,APDocCreated = @apCreated
       ,INDocCreated = @inCreated
    WHERE [CompanyID] = @companyID AND [DocType] = @lcDocType AND [RefNbr] = @lcRefNbr

UPDATE [POLandedCostDoc]
    SET [Status] = CASE WHEN [Released] = 1 THEN 'R' ELSE 'H' END
    WHERE [CompanyID] = @companyID AND [DocType] = @lcDocType AND [RefNbr] = @lcRefNbr

FETCH NEXT FROM lcDoc_cursor
INTO @companyID
, @branchId
, @poReceiptType
, @poReceiptNbr
, @poReleased
, @apDocType
, @apRefNbr
, @apReleased
, @curyID
, @curyInfoCuryID
, @curyInfoCuryMultDiv
, @curyInfoCuryRate
, @curyInfoBaseCuryID
, @curyInfoID
, @tranSource
, @postponeAP
, @termsID
, @vendorID
, @vendorLocationID
, @invoiceDate
, @finPeriodID
, @invoiceNbr
, @splitTranID
, @taxZoneID
, @createdByID
, @createdByScreenID
, @createdDateTime
, @lastModifiedByID
, @lastModifiedByScreenID
, @lastModifiedDateTime
;
END
CLOSE lcDoc_cursor;
DEALLOCATE lcDoc_cursor;

UPDATE [APT] 
SET [APT].[LCDocType] = [LCD].[DocType]
   ,[APT].[LCRefNbr] = [LCD].[RefNbr]
   ,[APT].[LCLineNbr] = [LCD].[LineNbr]
FROM [APTran] [APT]
INNER JOIN [POLandedCostDetail] [LCD] ON [APT].[CompanyID] = [LCD].[CompanyID] AND [APT].[TranType] = [LCD].[APDocType] AND [APT].[RefNbr] = [LCD].[APRefNbr] AND [APT].[LCTranID] = [LCD].[LineNbr]

INSERT INTO [POLandedCostReceipt] ([CompanyID], [LCDocType], [LCRefNbr], [POReceiptType], [POReceiptNbr])
  SELECT [CompanyID],
         [DocType] AS [LCDOCTYPE],
         [RefNbr] AS [LCREFNBR],
         [POReceiptType],
         [POReceiptNbr]
    FROM [dbo].[POLandedCostReceiptLine]
    GROUP BY [CompanyID], [DocType], [RefNbr], [POReceiptType], [POReceiptNbr]

DELETE [LCRL] 
FROM [POLandedCostReceiptLine] [LCRL]
    INNER JOIN [POReceipt] [R] ON [LCRL].[CompanyID] = [R].[CompanyID] AND [LCRL].[POReceiptType] = [R].[ReceiptType] AND [LCRL].[POReceiptNbr] = [R].[ReceiptNbr]
WHERE [R].[Released] = 0

UPDATE [NS]
SET [NS].[LastNbr] = (SELECT
    COALESCE(MAX([LC].[RefNbr]), 'LC000000')
  FROM [POLandedCostDoc] [LC]
  WHERE [NS].[CompanyID] = [LC].[CompanyID])
FROM [NumberingSequence] [NS]
WHERE [NS].[NumberingID] = 'POLANDCOST'
GO

--[mysql: Native]
--[mssql: Skip]
--[IfExists(Table = LandedCostTran)]
--[IfExists(Table = LandedCostTranSplit)]
--[IfExists(Column = POReceipt.TaxZoneID)]
--[IfNotExistsSelect(From = POLandedCostDoc)]
-- Create Landed Costs Documents
--[OldHash(Hash = 09ce77c1f2ac71287b56200affee43827db62bda3c0a746e789f392e64e4c6b5)]
--[OldHash(Hash = cfeffcf306d019abad4dea6bbc9e490fc5598d873262bba8f8d97dba5bd3a408)]
--[OldHash(Hash = 1179bbc681486cc61a5bbe783d84d485f9ec109d79eab60df9a3d00bc9ce8c67)]
--[OldHash(Hash = 05942c356ac8e5051385ce0eceeea5275d111315d22b442fc9f2150729ef450c)]
--[MinVersion(Hash = b67bc77f0cc8ec401a56a55d6328365d9811c7f09e88f6ddad62a98d83178e95)]
DROP PROCEDURE IF EXISTS CreateLandedCostsDocuments;

CREATE PROCEDURE CreateLandedCostsDocuments()
BEGIN
	DECLARE companyID INT;
	DECLARE branchId INT;
	DECLARE poReceiptType CHAR(3) CHARACTER SET 'latin1' COLLATE 'latin1_general_ci';
	DECLARE poReceiptNbr VARCHAR(15) CHARACTER SET 'utf8mb4' COLLATE 'utf8mb4_unicode_ci';
	DECLARE poReleased BIT;
	DECLARE apDocType CHAR(3) CHARACTER SET 'latin1' COLLATE 'latin1_general_ci';
	DECLARE apRefNbr VARCHAR(15) CHARACTER SET 'latin1' COLLATE 'latin1_general_ci';
	DECLARE apReleased BIT;
	DECLARE apHold BIT;
	DECLARE apCreated BIT;
	DECLARE inCreated BIT;
	DECLARE curyID VARCHAR(5) CHARACTER SET 'utf8mb4' COLLATE 'utf8mb4_unicode_ci';
	DECLARE curyInfoID BIGINT;
	DECLARE postponeAP BIT;
	DECLARE tranSource CHAR(2) CHARACTER SET 'latin1' COLLATE 'latin1_general_ci';
	DECLARE ownerID CHAR(36);
	DECLARE noteID CHAR(36);
	DECLARE termsID VARCHAR(10) CHARACTER SET 'utf8mb4' COLLATE 'utf8mb4_unicode_ci';
	DECLARE invoiceDate DATETIME;
	DECLARE invoiceNbr VARCHAR(40) CHARACTER SET 'utf8mb4' COLLATE 'utf8mb4_unicode_ci';
	DECLARE vendorID INT;
	DECLARE vendorLocationID VARCHAR(40) CHARACTER SET 'utf8mb4' COLLATE 'utf8mb4_unicode_ci';
	DECLARE createdByID CHAR(36);
	DECLARE createdByScreenID CHAR(8) CHARACTER SET 'latin1' COLLATE 'latin1_general_ci';
	DECLARE createdDateTime DATETIME;
	DECLARE lastModifiedByID CHAR(36);
	DECLARE lastModifiedByScreenID CHAR(8) CHARACTER SET 'latin1' COLLATE 'latin1_general_ci';
	DECLARE lastModifiedDateTime DATETIME;
	DECLARE finPeriodID CHAR(6) CHARACTER SET 'latin1' COLLATE 'latin1_general_ci';
	DECLARE lcDocType CHAR(3) CHARACTER SET 'utf8mb4' COLLATE 'utf8mb4_unicode_ci';
	DECLARE lcRefNbr VARCHAR(15) CHARACTER SET 'utf8mb4' COLLATE 'utf8mb4_unicode_ci';
	DECLARE lcRefCntr INT;
	DECLARE curyInfoCuryID VARCHAR(5) CHARACTER SET 'utf8mb4' COLLATE 'utf8mb4_unicode_ci';
	DECLARE curyInfoCuryMultDiv CHAR(1) CHARACTER SET 'latin1' COLLATE 'latin1_general_ci';
	DECLARE curyInfoCuryRate DECIMAL(18, 9);
	DECLARE curyInfoBaseCuryID VARCHAR(5) CHARACTER SET 'utf8mb4' COLLATE 'utf8mb4_unicode_ci';
	DECLARE splitTranID INT;
	DECLARE taxZoneID VARCHAR(10) CHARACTER SET 'utf8mb4' COLLATE 'utf8mb4_unicode_ci';
	DECLARE isPOTranSource BIT;
	DECLARE curyLineTotal DECIMAL(19, 4);
	DECLARE lineTotal DECIMAL(19, 4);
	DECLARE curyAllocatedTotal DECIMAL(19, 4);
	DECLARE allocatedTotal DECIMAL(19, 4);
	DECLARE curyTaxTotal DECIMAL(19, 4);
	DECLARE taxTotal DECIMAL(19, 4);
    
    DECLARE done INT DEFAULT FALSE;

	DECLARE lcDoc_cursor CURSOR FOR SELECT
	  `LCT`.`CompanyID`
	 ,CASE WHEN `LCT`.`Source` = 'PO' THEN `POR`.`BranchID` ELSE `APR`.`BranchID` END AS BranchID 
	 ,`LCT`.`POReceiptType`
	 ,`LCT`.`POReceiptNbr`
	 ,COALESCE(`POR`.`Released`, 0) AS `POReleased`
	 ,`LCT`.`APDocType`
	 ,`LCT`.`APRefNbr`
	 ,COALESCE(`APR`.`Released`, 0) AS `APReleased`
	 ,`LCT`.`CuryID`
	 ,`CI`.`CuryID`
	 ,`CI`.`CuryMultDiv`
	 ,`CI`.`CuryRate`
	 ,`CI`.`BaseCuryID`
	 ,MAX(`LCT`.`CuryInfoID`) AS `CuryInfoID`
	 ,`LCT`.`Source`
	 ,`LCT`.`PostponeAP`
	 ,`LCT`.`TermsID`
	 ,`LCT`.`VendorID`
	 ,`LCT`.`VendorLocationID`
	 ,`LCT`.`InvoiceDate`
	 ,`FP`.`FinPeriodID`
	 ,`LCT`.`InvoiceNbr`
	 ,`LCTS`.`LCTranID` AS `SplitTranID`
	 ,CASE WHEN `LCT`.`Source` = 'PO' THEN `POR`.`TaxZoneID` ELSE `API`.`TaxZoneID` END AS `TaxZoneID`
	 ,MAX(`LCT`.`CreatedByID`) AS `CreatedByID`
	 ,MAX(`LCT`.`CreatedByScreenID`) AS `CreatedByScreenID`
	 ,MAX(`LCT`.`CreatedDateTime`) AS `CreatedDateTime`
	 ,MAX(`LCT`.`LastModifiedByID`) AS `LastModifiedByID`
	 ,MAX(`LCT`.`LastModifiedByScreenID`) AS `LastModifiedByScreenID`
	 ,MAX(`LCT`.`LastModifiedDateTime`) AS `LastModifiedDateTime`
	FROM `LandedCostTran` `LCT`
	  INNER JOIN `CurrencyInfo` `CI` ON `LCT`.`CompanyID` = `CI`.`CompanyID` AND `LCT`.`CuryInfoID` = `CI`.`CuryInfoID`
	  INNER JOIN `FinPeriod` `FP` ON `LCT`.`CompanyID` = `FP`.`CompanyID` AND `FP`.`StartDate` <= `LCT`.`InvoiceDate` AND `FP`.`EndDate` > `LCT`.`InvoiceDate`
	  LEFT JOIN `APRegister` `APR` ON `LCT`.`CompanyID` = `APR`.`CompanyID` AND `LCT`.`APDocType` = `APR`.`DocType` AND `LCT`.`APRefNbr` = `APR`.`RefNbr`
	  LEFT JOIN `APInvoice` `API` ON `LCT`.`CompanyID` = `API`.`CompanyID` AND `LCT`.`APDocType` = `API`.`DocType` AND `LCT`.`APRefNbr` = `API`.`RefNbr`
	  LEFT JOIN `POReceipt` `POR` ON `LCT`.`CompanyID` = `POR`.`CompanyID` AND `LCT`.`POReceiptType` = `POR`.`ReceiptType` AND `LCT`.`POReceiptNbr` = `POR`.`ReceiptNbr`
	  LEFT JOIN `LandedCostTranSplit` `LCTS` ON `LCT`.`CompanyID` = `LCTS`.`CompanyID` AND `LCT`.`LCTranID` = `LCTS`.`LCTranID`
	WHERE (`LCT`.`Source` = 'PO' AND `POR`.`CompanyID` IS NOT NULL) OR (`LCT`.`Source` = 'AP' AND `APR`.`CompanyID` IS NOT NULL)
	GROUP BY `LCT`.`CompanyID`
			,`POR`.`BranchID`
			,`APR`.`BranchID`
			,`LCT`.`POReceiptType`
			,`LCT`.`POReceiptNbr`
			,`POR`.`Released`
			,`LCT`.`APDocType`
			,`LCT`.`APRefNbr`
			,`APR`.`Released`
			,`LCT`.`CuryID`
			,`LCT`.`Source`
			,`LCT`.`PostponeAP`
			,`LCT`.`TermsID`
			,`LCT`.`VendorID`
			,`LCT`.`VendorLocationID`
			,`LCT`.`InvoiceDate`
			,`FP`.`FinPeriodID`
			,`LCT`.`InvoiceNbr`
			,`CI`.`CuryID`
			,`CI`.`CuryMultDiv`
			,`CI`.`CuryRate`
			,`CI`.`BaseCuryID`
			,`LCTS`.`LCTranID`
			,`POR`.`TaxZoneID`
			,`API`.`TaxZoneID`
	ORDER BY `LCT`.`InvoiceDate`, `LCT`.`POReceiptNbr`, `LCT`.`APRefNbr`;
    
	DECLARE CONTINUE HANDLER FOR NOT FOUND SET done = TRUE;
     
	UPDATE `LandedCostTran` `LCT`
	INNER JOIN `APInvoice` `API` ON `LCT`.`CompanyID` = `API`.`CompanyID` AND `LCT`.`APDocType` = `API`.`DocType` AND `LCT`.`APRefNbr` = `API`.`RefNbr`
    SET `LCT`.`TermsID` = `API`.`TermsID`
	WHERE `LCT`.`Source` = 'AP';
    
	OPEN lcDoc_cursor;

	FETCH lcDoc_cursor 
    INTO companyID
	, branchId
	, poReceiptType
	, poReceiptNbr
	, poReleased
	, apDocType
	, apRefNbr
	, apReleased
	, curyID
	, curyInfoCuryID
	, curyInfoCuryMultDiv
	, curyInfoCuryRate
	, curyInfoBaseCuryID
	, curyInfoID
	, tranSource
	, postponeAP
	, termsID
	, vendorID
	, vendorLocationID
	, invoiceDate
	, finPeriodID
	, invoiceNbr
	, splitTranID
	, taxZoneID
	, createdByID
	, createdByScreenID
	, createdDateTime
	, lastModifiedByID
	, lastModifiedByScreenID
	, lastModifiedDateTime;

	WHILE done = 0 DO
		SET lcDocType = 'L';
        
		SELECT COALESCE(MAX(CAST(SUBSTRING(`RefNbr`, 3, 6) AS SIGNED INTEGER)), 0) + 1 
        INTO lcRefCntr
		FROM `POLandedCostDoc`
		WHERE CompanyID = companyID;

		SET lcRefNbr = CONVERT(lcRefCntr, NCHAR);
		SET lcRefNbr = CASE 
			WHEN lcRefCntr < 10 THEN CONCAT('00000', lcRefNbr)
			WHEN lcRefCntr < 100 THEN CONCAT('0000', lcRefNbr)
			WHEN lcRefCntr < 1000 THEN CONCAT('000', lcRefNbr)
			WHEN lcRefCntr < 10000 THEN CONCAT('00', lcRefNbr)
			WHEN lcRefCntr < 100000 THEN CONCAT('0', lcRefNbr)
			ELSE lcRefNbr END;
		SET lcRefNbr = CONCAT('LC', lcRefNbr);

		INSERT INTO `POLandedCostDetail` (`CompanyID`
		, `DocType`
		, `RefNbr`
		, `LineNbr`
		, `SortOrder`
		, `BranchID`
		, `LandedCostCodeID`
		, `Descr`
		, `AllocationMethod`
		, `CuryInfoID`
		, `LineAmt`
		, `CuryLineAmt`
		, `TaxCategoryID`
		, `InventoryID`
		, `SubItemID`
		, `LCAccrualAcct`
		, `LCAccrualSub`
		, `APDocType`
		, `APRefNbr`
		, `INDocType`
		, `INRefNbr`
		, `CreatedByID`
		, `CreatedByScreenID`
		, `CreatedDateTime`
		, `LastModifiedByID`
		, `LastModifiedByScreenID`
		, `LastModifiedDateTime`
		,`NoteID`)
		  SELECT companyID AS `CompanyID`
				,lcDocType AS `DocType`
				,lcRefNbr AS `RefNbr`
				,`LCT`.`LCTranID` AS `LineNbr`
				,0 AS `SortOrder`
				,branchId AS `BranchID`
				,`LCT`.`LandedCostCodeID` AS `LandedCostCodeID`
				,`LCT`.`Descr` AS `Descr`
				,`LCC`.`AllocationMethod` AS `AllocationMethod`
				,`LCT`.`CuryInfoID` AS `CuryInfoID`
				,`LCT`.`LCAmount` AS `LineAmt`
				,`LCT`.`CuryLCAmount` AS `CuryLineAmt`
				,`LCT`.`TaxCategoryID` AS `TaxCategoryID`
				,`LCT`.`InventoryID` AS `InventoryID`
				,NULL AS `SubItemID`
				,`LCT`.`LCAccrualAcct` AS `LCAccrualAcct`
				,`LCT`.`LCAccrualSub` AS `LCAccrualSub`
				,`LCT`.`APDocType` AS `APDocType`
				,`LCT`.`APRefNbr` AS `APRefNbr`
				,`LCT`.`INDocType` AS `INDocType`
				,`LCT`.`INRefNbr` AS `INRefNbr`
				,`LCT`.`CreatedByID` AS `CreatedByID`
				,`LCT`.`CreatedByScreenID` AS `CreatedByScreenID`
				,`LCT`.`CreatedDateTime` AS `CreatedDateTime`
				,`LCT`.`LastModifiedByID` AS `LastModifiedByID`
				,`LCT`.`LastModifiedByScreenID` AS `LastModifiedByScreenID`
				,`LCT`.`LastModifiedDateTime` AS `LastModifiedDateTime`
				,UUID() AS `NoteID`
		  FROM `LandedCostTran` `LCT`
		  INNER JOIN `LandedCostCode` `LCC` ON `LCT`.`CompanyID` = `LCC`.`CompanyID` AND `LCT`.`LandedCostCodeID` = `LCC`.`LandedCostCodeID`
		  INNER JOIN `CurrencyInfo` `CI` ON `LCT`.`CompanyID` = `CI`.`CompanyID` AND `LCT`.`CuryInfoID` = `CI`.`CuryInfoID`
		  WHERE `LCT`.`CompanyID` = companyID
		  AND (`LCT`.`POReceiptType` = poReceiptType OR (`LCT`.`POReceiptType` IS NULL AND poReceiptType IS NULL))
		  AND (`LCT`.`POReceiptNbr` = poReceiptNbr OR (`LCT`.`POReceiptNbr` IS NULL AND poReceiptNbr IS NULL))
		  AND (`LCT`.`APDocType` = apDocType OR (`LCT`.`APDocType` IS NULL AND apDocType IS NULL))
		  AND (`LCT`.`APRefNbr` = apRefNbr OR (`LCT`.`APRefNbr` IS NULL AND apRefNbr IS NULL))
		  AND `LCT`.`CuryID` = curyID
		  AND `LCT`.`Source` = tranSource
		  AND `LCT`.`PostponeAP` = postponeAP
		  AND (`LCT`.`TermsID` = termsID OR `LCT`.`TermsID` IS NULL AND termsID IS NULL)
		  AND `LCT`.`VendorID` = vendorID
		  AND `LCT`.`VendorLocationID` = vendorLocationID
		  AND `LCT`.`InvoiceDate` = invoiceDate
		  AND `CI`.`CuryID` = curyInfoCuryID
		  AND `CI`.`CuryMultDiv` = curyInfoCuryMultDiv
		  AND `CI`.`CuryRate` = curyInfoCuryRate
		  AND `CI`.`BaseCuryID` = curyInfoBaseCuryID
		  AND (`LCT`.`LCTranID` = splitTranID OR splitTranID IS NULL)
		  AND ((`LCT`.`InvoiceNbr` IS NULL AND invoiceNbr IS NULL) OR `LCT`.`InvoiceNbr` = invoiceNbr);

		INSERT INTO `POLandedCostReceiptLine` (`CompanyID`
		, `DocType`
		, `RefNbr`
		, `LineNbr`
		, `SortOrder`
		, `BranchID`
		, `InventoryID`
		, `SubItemID`
		, `TranDesc`
		, `CuryInfoID`
		, `POReceiptType`
		, `POReceiptNbr`
		, `POReceiptLineNbr`
		, `SiteID`
		, `UOM`
		, `ReceiptQty`
		, `BaseReceiptQty`
		, `UnitWeight`
		, `UnitVolume`
		, `ExtWeight`
		, `ExtVolume`
		, `LineAmt`
		, `CuryAllocatedLCAmt`
		, `AllocatedLCAmt`
		, `CreatedByID`
		, `CreatedByScreenID`
		, `CreatedDateTime`
		, `LastModifiedByID`
		, `LastModifiedByScreenID`
		, `LastModifiedDateTime`)
		  SELECT `RL`.`CompanyID` AS `CompanyID`
				,lcDocType AS `DocType`
				,lcRefNbr AS `DocType`
				,COALESCE(`LCTS`.`SplitLineNbr`, 0) * 10000 + `RL`.`LineNbr` AS `LineNbr`
				,`RL`.`SortOrder` AS `SortOrder`
				,branchId AS `BranchID`
				,`RL`.`InventoryID` AS `InventoryID`
				,`RL`.`SubItemID` AS `SubItemID`
				,`RL`.`TranDesc` AS `TranDesc`
				,curyInfoID AS `CuryInfoID`
				,`RL`.`ReceiptType` AS `POReceiptType`
				,`RL`.`ReceiptNbr` AS `POReceiptNbr`
				,`RL`.`LineNbr` AS `POReceiptLineNbr`
				,`RL`.`SiteID` AS `SiteID`
				,`RL`.`UOM` AS `UOM`
				,`RL`.`ReceiptQty` AS `ReceiptQty`
				,`RL`.`BaseReceiptQty` AS `BaseReceiptQty`
				,`RL`.`UnitWeight` AS `UnitWeight`
				,`RL`.`UnitVolume` AS `UnitVolume`
				,`RL`.`ExtWeight` AS `ExtWeight`
				,`RL`.`ExtVolume` AS `ExtVolume`
				,`RL`.`TranCostFinal` AS `LineAmt`
				,0 AS `CuryAllocatedLCAmt`
				,0 AS `AllocatedLCAmt`
				,MAX(`LCT`.`CreatedByID`) AS `CreatedByID`
				,MAX(`LCT`.`CreatedByScreenID`) AS `CreatedByScreenID`
				,MAX(`LCT`.`CreatedDateTime`) AS `CreatedDateTime`
				,MAX(`LCT`.`LastModifiedByID`) AS `LastModifiedByID`
				,MAX(`LCT`.`LastModifiedByScreenID`) AS `LastModifiedByScreenID`
				,MAX(`LCT`.`LastModifiedDateTime`) AS `LastModifiedDateTime`
		  FROM `LandedCostTran` `LCT`
		  LEFT JOIN `LandedCostTranSplit` `LCTS` ON `LCT`.`CompanyID` = `LCTS`.`CompanyID` AND `LCT`.`LCTranID` = `LCTS`.`LCTranID`
		  INNER JOIN `POReceiptLine` `RL` ON `RL`.`CompanyID` = `LCT`.`CompanyID` AND ((`RL`.`ReceiptNbr` = `LCT`.`POReceiptNbr` AND `LCTS`.`POReceiptNbr` IS NULL) OR (`RL`.`ReceiptNbr` = `LCTS`.`POReceiptNbr` AND (`RL`.`InventoryID` = `LCTS`.`InventoryID` OR `LCTS`.`InventoryID` IS NULL)))
		  INNER JOIN `CurrencyInfo` `CI` ON `LCT`.`CompanyID` = `CI`.`CompanyID` AND `LCT`.`CuryInfoID` = `CI`.`CuryInfoID`
		  WHERE `LCT`.`CompanyID` = companyID
		  AND (`LCT`.`POReceiptType` = poReceiptType OR (`LCT`.`POReceiptType` IS NULL AND poReceiptType IS NULL))
		  AND (`LCT`.`POReceiptNbr` = poReceiptNbr OR (`LCT`.`POReceiptNbr` IS NULL AND poReceiptNbr IS NULL))
		  AND (`LCT`.`APDocType` = apDocType OR (`LCT`.`APDocType` IS NULL AND apDocType IS NULL))
		  AND (`LCT`.`APRefNbr` = apRefNbr OR (`LCT`.`APRefNbr` IS NULL AND apRefNbr IS NULL))
		  AND `LCT`.`CuryID` = curyID
		  AND `LCT`.`Source` = tranSource
		  AND `LCT`.`PostponeAP` = postponeAP
		  AND (`LCT`.`TermsID` = termsID OR `LCT`.`TermsID` IS NULL AND termsID IS NULL)
		  AND `LCT`.`VendorID` = vendorID
		  AND `LCT`.`VendorLocationID` = vendorLocationID
		  AND `LCT`.`InvoiceDate` = invoiceDate
		  AND `CI`.`CuryID` = curyInfoCuryID
		  AND `CI`.`CuryMultDiv` = curyInfoCuryMultDiv
		  AND `CI`.`CuryRate` = curyInfoCuryRate
		  AND `CI`.`BaseCuryID` = curyInfoBaseCuryID
		  AND (`LCT`.`LCTranID` = splitTranID OR splitTranID IS NULL)
		  AND ((`LCT`.`InvoiceNbr` IS NULL AND invoiceNbr IS NULL) OR `LCT`.`InvoiceNbr` = invoiceNbr)
		  AND `RL`.`LineType` NOT IN ('SV', 'FT')
		  GROUP BY `RL`.`CompanyID`
				  ,`RL`.`LineNbr`
				  ,`RL`.`SortOrder`
				  ,`RL`.`InventoryID`
				  ,`RL`.`SubItemID`
				  ,`RL`.`TranDesc`
				  ,`RL`.`ReceiptType`
				  ,`RL`.`ReceiptNbr`
				  ,`RL`.`LineNbr`
				  ,`RL`.`SiteID`
				  ,`RL`.`UOM`
				  ,`RL`.`ReceiptQty`
				  ,`RL`.`BaseReceiptQty`
				  ,`RL`.`UnitWeight`
				  ,`RL`.`UnitVolume`
				  ,`RL`.`ExtWeight`
				  ,`RL`.`ExtVolume`
				  ,`RL`.`TranCostFinal`
				  ,`LCTS`.`SplitLineNbr`;

		INSERT INTO `POLandedCostSplit` (`CompanyID`
		, `DocType`
		, `RefNbr`
		, `DetailLineNbr`
		, `ReceiptLineNbr`
		, `CuryInfoID`
		, `CuryLineAmt`
		, `LineAmt`
		, `NoteID`
		, `CreatedByID`
		, `CreatedByScreenID`
		, `CreatedDateTime`
		, `LastModifiedByID`
		, `LastModifiedByScreenID`
		, `LastModifiedDateTime`)
		  SELECT `LCD`.`CompanyID` AS `CompanyID`
				,`LCD`.`DocType` AS `DocType`
				,`LCD`.`RefNbr` AS `RefNbr`
				,`LCD`.`LineNbr` AS `DetailLineNbr`
				,`LCRL`.`LineNbr` AS `ReceiptLineNbr`
				,`LCD`.`CuryInfoID` AS `CuryInfoID`
				,SUM(CASE WHEN `LCD`.`LineAmt` <> 0 THEN COALESCE(`IT`.`TranCost` * `LCD`.`CuryLineAmt` / `LCD`.`LineAmt`, 0) ELSE 0 END) AS `CuryLineAmt`
				,SUM(COALESCE(`IT`.`TranCost`, 0)) AS `LineAmt`
				,NULL AS `NoteID`
				,`LCD`.`CreatedByID` AS `CreatedByID`
				,`LCD`.`CreatedByScreenID` AS `CreatedByScreenID`
				,`LCD`.`CreatedDateTime` AS `CreatedDateTime`
				,`LCD`.`LastModifiedByID` AS `LastModifiedByID`
				,`LCD`.`LastModifiedByScreenID` AS `LastModifiedByScreenID`
				,`LCD`.`LastModifiedDateTime` AS `LastModifiedDateTime`
		  FROM `POLandedCostDetail` `LCD`
		  INNER JOIN `POLandedCostReceiptLine` `LCRL`
			ON `LCD`.`CompanyID` = `LCRL`.`CompanyID`
			  AND `LCD`.`DocType` = `LCRL`.`DocType`
			  AND `LCD`.`RefNbr` = `LCRL`.`RefNbr`
		  LEFT JOIN `INTran` `IT`
			ON `IT`.`CompanyID` = `LCD`.`CompanyID`
			  AND `IT`.`DocType` = `LCD`.`INDocType`
			  AND `IT`.`RefNbr` = `LCD`.`INRefNbr`
			  AND `IT`.`POReceiptNbr` = `LCRL`.`POReceiptNbr`
			  AND `IT`.`POReceiptLineNbr` = `LCRL`.`POReceiptLineNbr`
		  WHERE `LCD`.`CompanyID` = companyID
		  AND `LCD`.`DocType` = lcDocType
		  AND `LCD`.`RefNbr` = lcRefNbr
		  GROUP BY `LCD`.`CompanyID`
				  ,`LCD`.`DocType`
				  ,`LCD`.`RefNbr`
				  ,`LCD`.`LineNbr`
				  ,`LCRL`.`LineNbr`
				  ,`LCRL`.`LineAmt`
				  ,`LCD`.`CuryLineAmt`
				  ,`LCD`.`LineAmt`
				  ,`LCD`.`CuryInfoID`
				  ,`LCD`.`CreatedByID`
				  ,`LCD`.`CreatedByScreenID`
				  ,`LCD`.`CreatedDateTime`
				  ,`LCD`.`LastModifiedByID`
				  ,`LCD`.`LastModifiedByScreenID`
				  ,`LCD`.`LastModifiedDateTime`;

		UPDATE `POLandedCostReceiptLine` `LCRL`
		SET `LCRL`.`CuryAllocatedLCAmt` = (SELECT
				SUM(`LCS`.`CuryLineAmt`)
			  FROM `POLandedCostSplit` `LCS`
			  WHERE `LCRL`.`CompanyID` = `LCS`.`CompanyID`
			  AND `LCRL`.`DocType` = `LCS`.`DocType`
			  AND `LCRL`.`RefNbr` = `LCS`.`RefNbr`
			  AND `LCRL`.`LineNbr` = `LCS`.`ReceiptLineNbr`)
		   ,`LCRL`.`AllocatedLCAmt` = (SELECT
				SUM(`LCS`.`LineAmt`)
			  FROM `POLandedCostSplit` `LCS`
			  WHERE `LCRL`.`CompanyID` = `LCS`.`CompanyID`
			  AND `LCRL`.`DocType` = `LCS`.`DocType`
			  AND `LCRL`.`RefNbr` = `LCS`.`RefNbr`
			  AND `LCRL`.`LineNbr` = `LCS`.`ReceiptLineNbr`)
		WHERE `LCRL`.`CompanyID` = companyID
		AND `LCRL`.`DocType` = lcDocType
		AND `LCRL`.`RefNbr` = lcRefNbr;

		SET curyLineTotal = (SELECT SUM(COALESCE(`LCRL`.`CuryLineAmt`, 0))
			FROM `POLandedCostDetail` `LCRL`
			WHERE `LCRL`.`CompanyID` = companyID AND `LCRL`.`DocType` = lcDocType AND `LCRL`.`RefNbr` = lcRefNbr);

		SET lineTotal = (SELECT SUM(COALESCE(`LCRL`.`LineAmt`, 0))
			FROM `POLandedCostDetail` `LCRL`
			WHERE `LCRL`.`CompanyID` = companyID AND `LCRL`.`DocType` = lcDocType AND `LCRL`.`RefNbr` = lcRefNbr);

		SET curyAllocatedTotal = (SELECT SUM(COALESCE(`LCRL`.`CuryAllocatedLCAmt`, 0))
			FROM `POLandedCostReceiptLine` `LCRL`
			WHERE `LCRL`.`CompanyID` = companyID AND `LCRL`.`DocType` = lcDocType AND `LCRL`.`RefNbr` = lcRefNbr);

		SET allocatedTotal = (SELECT SUM(COALESCE(`LCRL`.`AllocatedLCAmt`, 0))
			FROM `POLandedCostReceiptLine` `LCRL`
			WHERE `LCRL`.`CompanyID` = companyID AND `LCRL`.`DocType` = lcDocType AND `LCRL`.`RefNbr` = lcRefNbr);

		INSERT INTO `POLandedCostDoc` (`CompanyID`
		, `DocType`
		, `RefNbr`
		, `BranchID`
		, `OpenDoc`
		, `Released`
		, `Hold`
		, `Status`
		, `IsTaxValid`
		, `NonTaxable`
		, `DocDate`
		, `TranPeriodID`
		, `FinPeriodID`
		, `VendorID`
		, `VendorLocationID`
		, `VendorRefNbr`
		, `LineCntr`
		, `CuryID`
		, `CuryInfoID`
		, `CreateBill`
		, `CuryLineTotal`
		, `LineTotal`
		, `CuryDocTotal`
		, `DocTotal`
		, `CuryTaxTotal`
		, `TaxTotal`
		, `CuryAllocatedTotal`
		, `AllocatedTotal`
		, `VatTaxableTotal`
		, `CuryVatTaxableTotal`
		, `VatExemptTotal`
		, `CuryVatExemptTotal`
		, `CuryControlTotal`
		, `ControlTotal`
		, `TermsID`
		, `BillDate`
		, `DueDate`
		, `DiscDate`
		, `CuryDiscAmt`
		, `DiscAmt`
		, `TaxZoneID`
		, `PayToVendorID`
		, `WorkgroupID`
		, `OwnerID`
		, `APDocCreated`
		, `INDocCreated`
		, `NoteID`
		, `CreatedByID`
		, `CreatedByScreenID`
		, `CreatedDateTime`
		, `LastModifiedByID`
		, `LastModifiedByScreenID`
		, `LastModifiedDateTime`)
		  VALUES (companyID, lcDocType, lcRefNbr, branchId, 1 -- <OpenDoc, bit,>
		  , 0 -- <Released, bit,>
		  , 0 -- <Hold, bit,>
		  , 'H' -- <Status, char(1),>
		  , 0 -- <IsTaxValid, bit,>
		  , 0 -- <NonTaxable, bit,>
		  , invoiceDate -- <DocDate, datetime2(0),>
		  , finPeriodID -- <TranPeriodID, char(6),>
		  , finPeriodID -- <FinPeriodID, char(6),>
		  , vendorID -- <VendorID, int,>
		  , vendorLocationID -- <VendorLocationID, int,>
		  , COALESCE(invoiceNbr, '') -- <VendorRefNbr, nvarchar(40),>
		  , 0 -- <LineCntr, int,>
		  , curyID, curyInfoID, 0 -- <CreateBill, bit,>
		  , COALESCE(curyLineTotal, 0) -- <CuryLineTotal, decimal(19,4),>
		  , COALESCE(lineTotal, 0) -- <LineTotal, decimal(19,4),>
		  , COALESCE(curyLineTotal, 0) + COALESCE(curyTaxTotal, 0) -- <CuryDocTotal, decimal(19,4),>
		  , COALESCE(lineTotal, 0) + COALESCE(taxTotal, 0) -- <DocTotal, decimal(19,4),>
		  , COALESCE(curyTaxTotal, 0) -- <CuryTaxTotal, decimal(19,4),>
		  , COALESCE(taxTotal, 0) -- <TaxTotal, decimal(19,4),>
		  , COALESCE(curyAllocatedTotal, 0) -- <CuryAllocatedTotal, decimal(19,4),>
		  , COALESCE(allocatedTotal, 0) -- <AllocatedTotal, decimal(19,4),>
		  , 0 -- <VatTaxableTotal, decimal(19,4),>
		  , 0 -- <CuryVatTaxableTotal, decimal(19,4),>
		  , 0 -- <VatExemptTotal, decimal(19,4),>
		  , 0 -- <CuryVatExemptTotal, decimal(19,4),>
		  , COALESCE(curyLineTotal, 0) + COALESCE(curyTaxTotal, 0) -- <CuryControlTotal, decimal(19,4),>
		  , COALESCE(lineTotal, 0) + COALESCE(taxTotal, 0) -- <ControlTotal, decimal(19,4),>
		  , termsID -- <TermsID, nvarchar(10),>
		  , invoiceDate -- <BillDate, datetime2(0),>
		  , invoiceDate -- <DueDate, datetime2(0),>
		  , invoiceDate -- <DiscDate, datetime2(0),>
		  , 0 -- <CuryDiscAmt, decimal(19,4),>
		  , 0 -- <DiscAmt, decimal(19,4),>
		  , taxZoneID -- <TaxZoneID, nvarchar(10),>
		  , vendorID -- <PayToVendorID, int,>
		  , NULL -- <WorkgroupID, int,>
		  , NULL -- <OwnerID, uniqueidentifier,>
		  , 0 -- <APDocCreated, bit,>
		  , 0 -- <INDocCreated, bit,>
		  , UUID()-- <NoteID, uniqueidentifier,>
		  , createdByID, createdByScreenID, createdDateTime, lastModifiedByID, lastModifiedByScreenID, lastModifiedDateTime);

		SELECT COALESCE(MIN(CASE WHEN `LCD`.`APRefNbr` IS NULL THEN 0 ELSE 1 END), 0)
			  ,COALESCE(MIN(CASE WHEN `LCD`.`INRefNbr` IS NULL AND `LCC`.`AllocationMethod` <> 'N' THEN 0 ELSE 1 END), 0)
			  ,COALESCE(MIN(CASE WHEN COALESCE(`AP`.`Released`, 0) = 0 THEN 0 ELSE 1 END), 0)
			  ,COALESCE(MIN(CASE WHEN COALESCE(`AP`.`Hold`, 0) = 0 THEN 0 ELSE 1 END), 0)
			INTO apCreated, inCreated, apReleased, apHold
			FROM `POLandedCostDetail` `LCD`
				INNER JOIN `LandedCostCode` `LCC` ON `LCD`.`CompanyID` = `LCC`.`CompanyID` AND `LCD`.`LandedCostCodeID` = `LCC`.`LandedCostCodeID`
				LEFT JOIN `APRegister` `AP` ON `AP`.`CompanyID` = `LCD`.`CompanyID` AND `AP`.`DocType` = `LCD`.`APDocType` AND `AP`.`RefNbr` = `LCD`.`APRefNbr`
			WHERE `LCD`.`CompanyID` = companyID AND `LCD`.`DocType` = lcDocType AND `LCD`.`RefNbr` = lcRefNbr;

		SET isPOTranSource = CASE WHEN tranSource = 'PO' THEN 1 ELSE 0 END;

		UPDATE `POLandedCostDoc`
			SET Released = CASE WHEN (poReleased = 1 AND apCreated = 1 AND isPOTranSource = 1 AND inCreated = 1) 
								  OR (apCreated = 1 AND apReleased = 1 AND isPOTranSource = 0 AND inCreated = 1) 
								  OR (poReleased = 1 AND postponeAP = 1 AND isPOTranSource = 1 AND inCreated = 1) THEN 1 ELSE 0 END
			   ,CreateBill = CASE WHEN (poReleased = 1 AND apCreated = 1 AND isPOTranSource = 1 AND postponeAP = 0) 
									OR (poReleased = 0 AND apCreated = 0 AND isPOTranSource = 1 AND postponeAP = 0) THEN 1 ELSE 0 END
			   ,Hold = CASE WHEN 
				   NOT ((poReleased = 1 AND apCreated = 1 AND isPOTranSource = 1) 
					 OR (apCreated = 1 AND apReleased = 1 AND isPOTranSource = 0 AND inCreated = 1) 
					 OR (poReleased = 1 AND postponeAP = 1 AND isPOTranSource = 1))
				   AND (isPOTranSource = 1 OR (isPOTranSource = 0 AND apHold = 1)) THEN 1 ELSE 0 END
			   ,APDocCreated = apCreated
			   ,INDocCreated = inCreated
			WHERE `CompanyID` = companyID AND `DocType` = lcDocType AND `RefNbr` = lcRefNbr;

		UPDATE `POLandedCostDoc`
			SET `Status` = CASE WHEN `Released` = 1 THEN 'R' ELSE 'H' END
			WHERE `CompanyID` = companyID AND `DocType` = lcDocType AND `RefNbr` = lcRefNbr;

		FETCH lcDoc_cursor
		INTO companyID
		, branchId
		, poReceiptType
		, poReceiptNbr
		, poReleased
		, apDocType
		, apRefNbr
		, apReleased
		, curyID
		, curyInfoCuryID
		, curyInfoCuryMultDiv
		, curyInfoCuryRate
		, curyInfoBaseCuryID
		, curyInfoID
		, tranSource
		, postponeAP
		, termsID
		, vendorID
		, vendorLocationID
		, invoiceDate
		, finPeriodID
		, invoiceNbr
		, splitTranID
		, taxZoneID
		, createdByID
		, createdByScreenID
		, createdDateTime
		, lastModifiedByID
		, lastModifiedByScreenID
		, lastModifiedDateTime
		;
	END WHILE;
    
	CLOSE lcDoc_cursor;

	UPDATE `APTran` `APT`
	INNER JOIN `POLandedCostDetail` `LCD` ON `APT`.`CompanyID` = `LCD`.`CompanyID` AND `APT`.`TranType` = `LCD`.`APDocType` AND `APT`.`RefNbr` = `LCD`.`APRefNbr` AND `APT`.`LCTranID` = `LCD`.`LineNbr`
	SET `APT`.`LCDocType` = `LCD`.`DocType`
	   ,`APT`.`LCRefNbr` = `LCD`.`RefNbr`
	   ,`APT`.`LCLineNbr` = `LCD`.`LineNbr`;

	INSERT INTO `POLandedCostReceipt` (`CompanyID`, `LCDocType`, `LCRefNbr`, `POReceiptType`, `POReceiptNbr`)
	  SELECT `CompanyID`,
			 `DocType` AS `LCDOCTYPE`,
			 `RefNbr` AS `LCREFNBR`,
			 `POReceiptType`,
			 `POReceiptNbr`
		FROM `POLandedCostReceiptLine`
		GROUP BY `CompanyID`, `DocType`, `RefNbr`, `POReceiptType`, `POReceiptNbr`;

	DELETE `LCRL` 
	FROM `POLandedCostReceiptLine` `LCRL`
		INNER JOIN `POReceipt` `R` ON `LCRL`.`CompanyID` = `R`.`CompanyID` AND `LCRL`.`POReceiptType` = `R`.`ReceiptType` AND `LCRL`.`POReceiptNbr` = `R`.`ReceiptNbr`
	WHERE `R`.`Released` = 0;

	UPDATE `NumberingSequence` `NS`
	SET `NS`.`LastNbr` = (SELECT
		COALESCE(MAX(`LC`.`RefNbr`), 'LC000000')
	  FROM `POLandedCostDoc` `LC`
	  WHERE `NS`.`CompanyID` = `LC`.`CompanyID`)
	WHERE `NS`.`NumberingID` = 'POLANDCOST';

END;

call CreateLandedCostsDocuments();
DROP PROCEDURE CreateLandedCostsDocuments;
GO

--[MinVersion(Hash = f98bf70de0fe567d325162852552c19c33451683830506de8cb859b5f09a9248)]
update CashAccount set RestrictVisibilityWithBranch=0 
from CashAccount inner join 
FeaturesSet on FeaturesSet.CompanyID=CashAccount.CompanyID
where FeaturesSet.InterBranch=0 and FeaturesSet.Branch=1 and CashAccount.RestrictVisibilityWithBranch=1
GO

-- Set LastDocDate of ARBalances if value is null
--[mysql: Skip]
--[mssql: Native]
--[MinVersion(Hash = 42e89a6f1b9cee25aff173f667d3132c37575556f1df9307bae07d15a79c0c9a)]
UPDATE b SET b.LastDocDate=maxDates.maxDate
FROM ARBalances AS b 
INNER JOIN 
(SELECT CompanyID, BranchID, CustomerID, CustomerLocationID, Max(dDate) as maxDate
	FROM (	SELECT r.CompanyID, r.BranchID, r.CustomerID, r.CustomerLocationID, DocDate as dDate 
				FROM ARRegister r 
				WHERE Released=1
			UNION
			SELECT g.CompanyID, gr.BranchID, g.CustomerID, gr.CustomerLocationID, g.AdjgDocDate as dDate
				FROM ARAdjust g 
				INNER JOIN ARRegister gr ON gr.CompanyID = g.CompanyID AND (gr.DocType = g.AdjgDocType AND gr.RefNbr = g.AdjgRefNbr) -- Outgoing applications of the document		
				WHERE g.Released=1
			UNION
			SELECT g.CompanyID, gr.BranchID, g.CustomerID, gr.CustomerLocationID, g.AdjgDocDate as dDate
				FROM ARAdjust g 
				INNER JOIN ARRegister gr ON gr.CompanyID = g.CompanyID AND (gr.DocType = g.AdjdDocType AND gr.RefNbr = g.AdjdRefNbr) -- Incoming applications to the document
				WHERE g.Released=1
		) as dDate
	GROUP BY CompanyID, BranchID, CustomerID, CustomerLocationID
) maxDates ON
	b.CompanyID = maxDates.CompanyID AND 
	b.BranchID	= maxDates.BranchID AND 
	b.CustomerID = maxDates.CustomerID AND 
	b.CustomerLocationID = maxDates.CustomerLocationID
WHERE b.LastDocDate IS NULL
GO

-- Set LastDocDate of ARBalances if value is null
--[mysql: Native]
--[mssql: Skip]
--[MinVersion(Hash = 11a386c6bbb989b25976d67c7aa4753ef4896b85c9ca2e98a0a0437585c7cbd3)]
UPDATE ARBalances AS b 
INNER JOIN 
(SELECT CompanyID, BranchID, CustomerID, CustomerLocationID, Max(dDate) as maxDate
	FROM (	SELECT r.CompanyID, r.BranchID, r.CustomerID, r.CustomerLocationID, DocDate as dDate 
				FROM ARRegister r 
				WHERE Released=1
			UNION
			SELECT g.CompanyID, gr.BranchID, g.CustomerID, gr.CustomerLocationID, g.AdjgDocDate as dDate
				FROM ARAdjust g 
				INNER JOIN ARRegister gr ON gr.CompanyID = g.CompanyID AND (gr.DocType = g.AdjgDocType AND gr.RefNbr = g.AdjgRefNbr) -- Outgoing applications of the document		
				WHERE g.Released=1
			UNION
			SELECT g.CompanyID, gr.BranchID, g.CustomerID, gr.CustomerLocationID, g.AdjgDocDate as dDate
				FROM ARAdjust g 
				INNER JOIN ARRegister gr ON gr.CompanyID = g.CompanyID AND (gr.DocType = g.AdjdDocType AND gr.RefNbr = g.AdjdRefNbr) -- Incoming applications to the document
				WHERE g.Released=1
		) as dDate
	GROUP BY CompanyID, BranchID, CustomerID, CustomerLocationID
) maxDates ON
	b.CompanyID = maxDates.CompanyID AND 
	b.BranchID	= maxDates.BranchID AND 
	b.CustomerID = maxDates.CustomerID AND 
	b.CustomerLocationID = maxDates.CustomerLocationID
SET b.LastDocDate=maxDates.maxDate
WHERE b.LastDocDate IS NULL
GO

--[mysql: Skip]
--[MinVersion(Hash = 44ad96707a314b98d0384d5840a9ef97c023143937ea70b50f181c34f6b1175b)]
IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = OBJECT_ID(N'InventoryItem_InventoryID__FSAppointmentInventoryItem_InventoryID_2'))
	DROP TRIGGER InventoryItem_InventoryID__FSAppointmentInventoryItem_InventoryID_2 
IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = OBJECT_ID(N'InventoryItem_ServiceID__FSAppointmentInventoryItem_InventoryID_2'))
	DROP TRIGGER InventoryItem_ServiceID__FSAppointmentInventoryItem_InventoryID_2
GO

--[MinVersion(Hash = 39aaeb6afe6c680b7b456c9d59726d99a43aeaeaec27d4def7904384026b260d)]
UPDATE CSAttributeGroup 
SET CSAttributeGroup.EntityClassID = Contract.ContractID
FROM CSAttributeGroup
INNER JOIN Contract
ON CSAttributeGroup.EntityClassID = Contract.ContractCD AND
	CSAttributeGroup.CompanyID = Contract.CompanyID
WHERE CSAttributeGroup.EntityType = 'PX.Objects.CT.Contract' AND
		Contract.IsTemplate = 1 AND
		Contract.BaseType = 'C'
GO

--[MinVersion(Hash = dcae0e4315659c5803c1d2cca0d0383832140685456f9698a0927e8973512de0)]
UPDATE Contract set BaseType='R' where BaseType='P' and IsTemplate = 1
UPDATE Contract set BaseType='T' where BaseType='C' and isTemplate = 1
GO

--[IfExists(Column = FSAppointment.Finished)]
--[IfExistsSelect(From = FSAppointment, WhereIsNull = Finished)]
--[MinVersion(Hash = b4b24dc802e7f123b125a28dda99578cc3a30863900ecf54ebf27cad7e6e747e)]
UPDATE FSAppointment 
	SET Finished = 1 
FROM FSAppointment
WHERE (FSAppointment.Status = 'C' OR FSAppointment.Status = 'Z') AND Finished IS NULL

UPDATE FSAppointment 
	SET Finished = 0 
FROM FSAppointment
WHERE Finished IS NULL
GO

--[IfExists(Column = FSServiceOrder.AppointmentsNeeded)]
--[IfExistsSelect(From = FSServiceOrder, WhereIsNull = AppointmentsNeeded)]
--[MinVersion(Hash = 36548afcd771169fa850a1e4b2d2bb59252cb56a806b1945a3f688f3ba57602f)]
UPDATE FSServiceOrder
SET AppointmentsNeeded = 1
FROM FSServiceOrder 
   INNER JOIN
      ( 
		SELECT FSServiceOrder.SOID, FSServiceOrder.CompanyID 
		FROM FSServiceOrder
			LEFT JOIN FSAppointment 
				ON FSAppointment.SOID = FSServiceOrder.SOID 
				AND FSAppointment.CompanyID = FSServiceOrder.CompanyID
				AND FSAppointment.Status <> 'X'
		WHERE 
			FSAppointment.AppointmentID IS NULL 
			AND FSServiceOrder.Quote = 0
		GROUP BY FSServiceOrder.SOID, FSServiceOrder.CompanyID
      ) AS SingleFSServiceOrder 
      ON FSServiceOrder.CompanyID = SingleFSServiceOrder.CompanyID AND FSServiceOrder.SOID = SingleFSServiceOrder.SOID
WHERE FSServiceOrder.AppointmentsNeeded IS NULL AND FSServiceOrder.Quote = 0

UPDATE FSServiceOrder
SET AppointmentsNeeded = 1
FROM FSServiceOrder 
   INNER JOIN
      ( 
		SELECT FSServiceOrder.SOID, FSServiceOrder.CompanyID 
		FROM FSServiceOrder
			INNER JOIN FSSODet 
				ON FSSODet.SOID = FSServiceOrder.SOID 
				AND FSSODet.CompanyID = FSServiceOrder.CompanyID
			LEFT JOIN FSAppointmentDet 
				ON FSAppointmentDet.SODetID = FSSODet.SODetID 
				AND FSAppointmentDet.CompanyID = FSSODet.CompanyID
		WHERE 
			(FSSODet.LineType = 'SERVI' OR FSSODet.LineType = 'SLPRO') 
			AND FSSODet.Status <> 'X'
			AND FSAppointmentDet.AppDetID IS NULL
			AND FSServiceOrder.Quote = 0
		GROUP BY FSServiceOrder.SOID, FSServiceOrder.CompanyID
      ) AS SingleFSServiceOrder 
      ON FSServiceOrder.CompanyID = SingleFSServiceOrder.CompanyID AND FSServiceOrder.SOID = SingleFSServiceOrder.SOID
WHERE FSServiceOrder.AppointmentsNeeded IS NULL AND FSServiceOrder.Quote = 0

UPDATE FSServiceOrder
SET AppointmentsNeeded = 1
FROM FSServiceOrder 
   INNER JOIN
      ( 
		SELECT p1.SOID, p1.CompanyID
		FROM FSAppointment p1 
		LEFT JOIN FSAppointment p2
			ON (p1.SOID = p2.SOID 
			AND p1.CompanyID = p2.CompanyID 
			AND p1.ScheduledDateTimeBegin < p2.ScheduledDateTimeBegin
			AND p2.Status <> 'X')
		WHERE p2.SOID IS NULL AND p1.Status <> 'X' AND p1.Finished = 0 AND (p1.Status = 'C' OR p1.Status = 'Z')
		GROUP BY p1.SOID, p1.CompanyID
      ) AS SingleFSServiceOrder 
      ON FSServiceOrder.CompanyID = SingleFSServiceOrder.CompanyID AND FSServiceOrder.SOID = SingleFSServiceOrder.SOID
WHERE FSServiceOrder.AppointmentsNeeded IS NULL AND FSServiceOrder.Quote = 0

UPDATE FSServiceOrder
	SET AppointmentsNeeded = 0
FROM FSServiceOrder
WHERE AppointmentsNeeded IS NULL AND FSServiceOrder.Quote = 0
GO

--[IfExists(Column = FSSODet.POCompleted)]
--[IfExistsSelect(From = FSSODet, WhereIsNull = POCompleted)]
--[MinVersion(Hash = 1d1a232d4c0a935f61a30ab0185fffa5008a3fc430d43e28e2ceca12e4ccce14)]
UPDATE FSSODet 
	SET FSSODet.POCompleted = POLine.Completed 
FROM FSSODet
INNER JOIN POLine 
	ON POLine.CompanyID = FSSODet.CompanyID 
	AND POLine.OrderType = FSSODet.POType
	AND POLine.OrderNbr = FSSODet.PONbr
	AND POLine.LineNbr = FSSODet.POLineNbr 
WHERE FSSODet.POCompleted IS NULL

UPDATE FSSODet 
	SET POCompleted = 0 
FROM FSSODet
WHERE POCompleted IS NULL
GO

--[IfExists(Column = FSServiceOrder.WaitingForParts)]
--[IfExistsSelect(From = FSServiceOrder, WhereIsNull = WaitingForParts)]
--[MinVersion(Hash = f4c4d7d9d05e347bee74e637908bc95ea28f1b0c8df94df7673935d0b898461b)]
UPDATE FSServiceOrder 
	SET WaitingForParts = 1 
FROM FSServiceOrder
INNER JOIN FSSODet ON FSServiceOrder.SOID = FSSODet.SOID AND FSServiceOrder.CompanyID = FSSODet.CompanyID
WHERE FSSODet.EnablePO = 1 AND (FSSODet.POCompleted = 0 OR FSSODet.POCompleted IS NULL) AND WaitingForParts IS NULL


UPDATE FSServiceOrder
	SET WaitingForParts = 0
WHERE WaitingForParts IS NULL
GO

--[IfExists(Column = FSAppointment.WaitingForParts)]
--[IfExistsSelect(From = FSAppointment, WhereIsNull = WaitingForParts)]
--[MinVersion(Hash = 4a30507508b37b1479797514cb550066d06a77dea43836c01e962b74edd92e59)]
UPDATE FSAppointment 
	SET WaitingForParts = 1 
FROM FSAppointment
INNER JOIN FSAppointmentDet ON FSAppointmentDet.AppointmentID = FSAppointment.AppointmentID AND FSAppointmentDet.CompanyID = FSAppointment.CompanyID
INNER JOIN FSSODet ON FSAppointmentDet.SODetID = FSSODet.SODetID AND FSAppointmentDet.CompanyID = FSSODet.CompanyID
WHERE FSSODet.EnablePO = 1 AND (FSSODet.POCompleted = 0 OR FSSODet.POCompleted IS NULL) AND WaitingForParts IS NULL


UPDATE FSAppointment
	SET WaitingForParts = 0
WHERE WaitingForParts IS NULL
GO

--[IfExists(Column = FSAppointmentDet.RemovedPriceType)]
--[MinVersion(Hash = 3f4ca2fb719463ea374ec9ac5ebb1ec604ab5fbf9e4301d009b1c5a05128d3c2)]
UPDATE FSAppointmentDet
SET PriceType = CASE WHEN RTRIM(RemovedPriceType) = 'BASE' THEN 'BASEP' ELSE RemovedPriceType END
GO

--[IfExists(Column = FSCreatedDoc.RemovedCreatedDocType)]
--[MinVersion(Hash = bfbdc9f427af05123f6fba9b3eced0e1cfc0c8dc150c036b0c0e39b938ea479b)]
UPDATE FSCreatedDoc
SET CreatedDocType = RemovedCreatedDocType
GO

--[IfExists(Column = FSCreatedDoc.RemovedCreatedRefNbr)]
--[MinVersion(Hash = dd8340b1ca18c58914f402b1bf28b185b5a9eb98c8ff3a22dad662fd41aae30c)]
UPDATE FSCreatedDoc
SET CreatedRefNbr = RemovedCreatedRefNbr
GO

--[IfExists(Column = FSCustomerBillingSetup.RemovedFrequencyType)]
--[MinVersion(Hash = 7186a64fa8bbd3cded84d1ebe94f832bdd779f28422051ca56487a581db19859)]
UPDATE FSCustomerBillingSetup
SET FrequencyType = RemovedFrequencyType
GO

--[IfExists(Column = FSCustomerClassBillingSetup.RemovedFrequencyType)]
--[MinVersion(Hash = 40971e3ab90fe8f956d3b2bc583dcda38d38fe0b1289b0903dc1a9e74bd8fd47)]
UPDATE FSCustomerClassBillingSetup
SET FrequencyType = RemovedFrequencyType
GO

--[IfExists(Column = FSBillingCycle.RemovedBillingCycleType)]
--[MinVersion(Hash = b228a96d7ef034d799b7faec3dc1f59f2a8ba3ef3f616e1debc3504597f48dea)]
UPDATE FSBillingCycle
SET BillingCycleType = RemovedBillingCycleType
GO

--[IfExists(Column = FSBillingCycle.RemovedTimeCycleType)]
--[MinVersion(Hash = a706ea15a668f9e6ea9e6e823aa9785eab728248316ebad650ba2c54a3d21f60)]
UPDATE FSBillingCycle
SET TimeCycleType = RemovedTimeCycleType
GO

--[IfExists(Column = FSSODet.BillCustomerID)]
--[IfExistsSelect(From = FSSODet, WhereIsNull = BillCustomerID)]
--[MinVersion(Hash = b22d35aa4efbe734402789925948a55304fda7e0852133812597078d82d45641)]
UPDATE FSSODet SET
    FSSODet.BillCustomerID = FSServiceOrder.BillCustomerID
FROM FSSODet
INNER JOIN FSServiceOrder ON FSServiceOrder.CompanyID = FSSODet.CompanyID
                                AND FSServiceOrder.SrvOrdType = FSSODet.SrvOrdType
                                AND FSServiceOrder.RefNbr = FSSODet.RefNbr
WHERE
    FSSODet.BillCustomerID IS NULL 
    AND FSServiceOrder.BillCustomerID IS NOT NULL;
GO

--[IfExists(Column = FSSODet.UnassignedQty)]
--[IfExistsSelect(From = FSSODet, WhereIsNull = UnassignedQty)]
--[MinVersion(Hash = 6f062d517ad3bb1e38b0d56ea481ea268926736d7025f2d2e3c6d9139c5699af)]
UPDATE FSSODet SET
    FSSODet.UnassignedQty = 0
WHERE
    FSSODet.UnassignedQty IS NULL;
GO

--[IfExists(Column = FSSODet.BaseEstimatedQty)]
--[IfExistsSelect(From = FSSODet, WhereIsNull = BaseEstimatedQty)]
--[MinVersion(Hash = 76f6b6f7152c5fc5869b2ecf97487ed0da63a322fbdea91a35afe13989d9b39c)]
UPDATE FSSODet SET
    FSSODet.BaseEstimatedQty = FSSODet.EstimatedQty
WHERE
    FSSODet.InventoryID IS NULL;

UPDATE FSSODet
SET FSSODet.BaseEstimatedQty = CASE WHEN INUnit.UnitMultDiv = 'M' THEN ROUND(FSSODet.EstimatedQty * INUnit.UnitRate, CommonSetup.DecPlQty)
							   WHEN INUnit.UnitMultDiv = 'D' AND INUnit.UnitRate = 0 THEN 0
							   ELSE ROUND(FSSODet.EstimatedQty / INUnit.UnitRate, CommonSetup.DecPlQty) END
FROM FSSODet
INNER JOIN InventoryItem ON (InventoryItem.CompanyID = FSSODet.CompanyID 
								AND InventoryItem.InventoryID = FSSODet.InventoryID)
INNER JOIN INUnit ON (INUnit.CompanyID = InventoryItem.CompanyID
						AND INUnit.InventoryID = InventoryItem.InventoryID
						AND INUnit.FromUnit = FSSODet.UOM AND INUnit.ToUnit = InventoryItem.BaseUnit)
INNER JOIN CommonSetup ON (CommonSetup.CompanyID = InventoryItem.CompanyID)
WHERE FSSODet.BaseEstimatedQty IS NULL;

UPDATE FSSODet
SET FSSODet.BaseEstimatedQty = CASE WHEN INUnit.UnitMultDiv = 'M' THEN ROUND(FSSODet.EstimatedQty * INUnit.UnitRate, CommonSetup.DecPlQty)
							   WHEN INUnit.UnitMultDiv = 'D' AND INUnit.UnitRate = 0 THEN 0
							   ELSE ROUND(FSSODet.EstimatedQty / INUnit.UnitRate, CommonSetup.DecPlQty) END
FROM FSSODet
INNER JOIN InventoryItem ON (InventoryItem.CompanyID = FSSODet.CompanyID 
								AND InventoryItem.InventoryID = FSSODet.InventoryID)
INNER JOIN INItemClass ON (INItemClass.CompanyID = InventoryItem.CompanyID 
								AND INItemClass.ItemClassID = InventoryItem.ItemClassID)
INNER JOIN INUnit ON (INUnit.CompanyID = INItemClass.CompanyID
						AND INUnit.ItemClassID = INItemClass.ItemClassID
						AND INUnit.FromUnit = FSSODet.UOM AND INUnit.ToUnit = InventoryItem.BaseUnit)
INNER JOIN CommonSetup ON (CommonSetup.CompanyID = InventoryItem.CompanyID)
WHERE FSSODet.BaseEstimatedQty IS NULL;


UPDATE FSSODet
SET FSSODet.BaseEstimatedQty = CASE WHEN INUnit.UnitMultDiv = 'M' THEN ROUND(FSSODet.EstimatedQty * INUnit.UnitRate, CommonSetup.DecPlQty)
							   WHEN INUnit.UnitMultDiv = 'D' AND INUnit.UnitRate = 0 THEN 0
							   ELSE ROUND(FSSODet.EstimatedQty / INUnit.UnitRate, CommonSetup.DecPlQty) END
FROM FSSODet
INNER JOIN InventoryItem ON (InventoryItem.CompanyID = FSSODet.CompanyID 
								AND InventoryItem.InventoryID = FSSODet.InventoryID)
INNER JOIN INUnit ON (INUnit.CompanyID = FSSODet.CompanyID
						AND INUnit.ItemClassID = 0
						AND INUnit.InventoryID = 0
						AND INUnit.FromUnit = FSSODet.UOM
						AND INUnit.ToUnit = InventoryItem.BaseUnit)
INNER JOIN CommonSetup ON (CommonSetup.CompanyID = InventoryItem.CompanyID)
WHERE FSSODet.BaseEstimatedQty IS NULL;
GO

--[IfExists(Column = FSSODet.OpenQty)]
--[IfExistsSelect(From = FSSODet, WhereIsNull = OpenQty)]
--[MinVersion(Hash = 13611a162342a293f94bad90d3806b729edea83843561bb3248159f3d25fc604)]
UPDATE FSSODet SET
    FSSODet.OpenQty = FSSODet.EstimatedQty
WHERE
    FSSODet.OpenQty IS NULL;
GO

--[IfExists(Column = FSSODet.BaseOpenQty)]
--[IfExistsSelect(From = FSSODet, WhereIsNull = BaseOpenQty)]
--[MinVersion(Hash = 7d55ce815e5f1b8496e75a80c41ab45305526a29c04fbdaa77d1dd0835d98a7f)]
UPDATE FSSODet SET
    FSSODet.BaseOpenQty = FSSODet.BaseEstimatedQty
WHERE
    FSSODet.BaseOpenQty IS NULL;
GO

--[IfExists(Column = FSSODet.InvtMult)]
--[IfExistsSelect(From = FSSODet, WhereIsNull = InvtMult)]
--[MinVersion(Hash = 719e08f0fca522af61ec6170292ad9f9212589d6a456ffa7d2015bb46a6f2c1a)]
UPDATE FSSODet SET
    FSSODet.InvtMult = CASE WHEN FSSrvOrdType.Behavior = 'QT' THEN 0 ELSE -1 END
FROM FSSODet
INNER JOIN FSSrvOrdType ON (FSSrvOrdType.CompanyID = FSSODet.CompanyID AND FSSrvOrdType.SrvOrdType = FSSODet.SrvOrdType)
WHERE
    FSSODet.InvtMult IS NULL;
GO

--[MinVersion(Hash = f9683e68cd276598d5d89cb9b410e970bc1fae71d0103b81fef587d50b7f2d9f)]
UPDATE INAvailabilityScheme SET
    INAvailabilityScheme.InclQtyFSSrvOrdBooked = INAvailabilityScheme.InclQtySOBooked,
    INAvailabilityScheme.InclQtyFSSrvOrdAllocated = INAvailabilityScheme.InclQtySOShipping,
    INAvailabilityScheme.InclQtyFSSrvOrdPrepared = INAvailabilityScheme.InclQtySOPrepared
FROM INAvailabilityScheme
GO

--[IfExists(Column = FSSODet.ShippedQty)]
--[IfExistsSelect(From = FSSODet, WhereIsNull = ShippedQty)]
--[MinVersion(Hash = b7ea3f36342d87d4e5f779508eabb964cb81ab068a815ceafdb5846ba653cd28)]
UPDATE FSSODet SET
    FSSODet.ShippedQty = 0, FSSODet.BaseShippedQty = 0
WHERE
    FSSODet.ShippedQty IS NULL;
GO

--[IfExists(Column = FSSODet.Completed)]
--[IfExistsSelect(From = FSSODet, WhereIsNull = Completed)]
--[MinVersion(Hash = bb67b54012ad9f26435e269f77704016d580bbf72d20c9a2ab9728bec1a662e5)]
UPDATE FSSODet SET
    FSSODet.Completed = 0
WHERE
    FSSODet.Completed IS NULL;
GO

--[IfExists(Column = FSSODet.ShipComplete)]
--[IfExistsSelect(From = FSSODet, WhereIsNull = ShipComplete)]
--[MinVersion(Hash = 4103ed8de9e1c32c5f0747f8f85aa9f3573fde83596efd1ce9fec9c727d720d7)]
UPDATE FSSODet SET
    FSSODet.ShipComplete = 'L'
WHERE
    FSSODet.ShipComplete IS NULL;
GO

--[IfExists(Column = FSSODet.ShipDate)]
--[IfExistsSelect(From = FSSODet, WhereIsNull = ShipDate)]
--[MinVersion(Hash = e1b5c1d781b3c3433c728cf1558456bc38b884d5b5a0b6c1e71e087ed56cb329)]
UPDATE FSSODet SET
    FSSODet.ShipDate = FSSODet.TranDate
WHERE
    FSSODet.ShipDate IS NULL;
GO

--[IfExists(Column = FSSrvOrdType.AllocationOrderType)]
--[IfExistsSelect(From = FSSrvOrdType, WhereIsNull = AllocationOrderType)]
--[MinVersion(Hash = 7132e30feebc587cc110111d9afded2fb80ad1b5cf545e479762679223c50fc0)]
UPDATE FSSrvOrdType SET
    FSSrvOrdType.AllocationOrderType = 'SO'
WHERE
    FSSrvOrdType.AllocationOrderType IS NULL;
GO

--[IfExists(Column = POOrder.OpenOrderQty)]
--[MinVersion(Hash = d9f6bf20537da23561d8abb4bae98526e067f704eaf29c773cfb3293102e975f)]
UPDATE po
SET [OpenOrderQty] = COALESCE(POLineGroup.OpenQtySum, 0)
FROM [POOrder] po
LEFT JOIN (
    SELECT pol.[CompanyID], pol.[OrderType], pol.[OrderNbr], SUM(pol.[OpenQty]) AS OpenQtySum
    FROM [POLine] pol
    GROUP BY pol.[CompanyID], pol.[OrderType], pol.[OrderNbr]) POLineGroup
ON POLineGroup.[CompanyID] = po.[CompanyID] AND POLineGroup.[OrderType] = po.[OrderType] AND POLineGroup.[OrderNbr] = po.[OrderNbr]
WHERE po.[OpenOrderQty] IS NULL
GO

--[IfExists(Column = FSSODet.BaseBillableQty)]
--[IfExistsSelect(From = FSSODet, WhereIsNull = BaseBillableQty)]
--[MinVersion(Hash = 8174dd28fc43d1e3f335a220819a81b248591c118e0a6f88c725548e108f2d4d)]
UPDATE FSSODet SET
    FSSODet.BaseBillableQty = FSSODet.BillableQty
WHERE
    FSSODet.InventoryID IS NULL;

UPDATE FSSODet
SET FSSODet.BaseBillableQty = CASE WHEN INUnit.UnitMultDiv = 'M' THEN ROUND(FSSODet.BillableQty * INUnit.UnitRate, CommonSetup.DecPlQty)
							   WHEN INUnit.UnitMultDiv = 'D' AND INUnit.UnitRate = 0 THEN 0
							   ELSE ROUND(FSSODet.BillableQty / INUnit.UnitRate, CommonSetup.DecPlQty) END
FROM FSSODet
INNER JOIN InventoryItem ON (InventoryItem.CompanyID = FSSODet.CompanyID 
								AND InventoryItem.InventoryID = FSSODet.InventoryID)
INNER JOIN INUnit ON (INUnit.CompanyID = InventoryItem.CompanyID
						AND INUnit.InventoryID = InventoryItem.InventoryID
						AND INUnit.FromUnit = FSSODet.UOM AND INUnit.ToUnit = InventoryItem.BaseUnit)
INNER JOIN CommonSetup ON (CommonSetup.CompanyID = InventoryItem.CompanyID)
WHERE FSSODet.BaseBillableQty IS NULL;

UPDATE FSSODet
SET FSSODet.BaseBillableQty = CASE WHEN INUnit.UnitMultDiv = 'M' THEN ROUND(FSSODet.BillableQty * INUnit.UnitRate, CommonSetup.DecPlQty)
							   WHEN INUnit.UnitMultDiv = 'D' AND INUnit.UnitRate = 0 THEN 0
							   ELSE ROUND(FSSODet.BillableQty / INUnit.UnitRate, CommonSetup.DecPlQty) END
FROM FSSODet
INNER JOIN InventoryItem ON (InventoryItem.CompanyID = FSSODet.CompanyID 
								AND InventoryItem.InventoryID = FSSODet.InventoryID)
INNER JOIN INItemClass ON (INItemClass.CompanyID = InventoryItem.CompanyID 
								AND INItemClass.ItemClassID = InventoryItem.ItemClassID)
INNER JOIN INUnit ON (INUnit.CompanyID = INItemClass.CompanyID
						AND INUnit.ItemClassID = INItemClass.ItemClassID
						AND INUnit.FromUnit = FSSODet.UOM AND INUnit.ToUnit = InventoryItem.BaseUnit)
INNER JOIN CommonSetup ON (CommonSetup.CompanyID = InventoryItem.CompanyID)
WHERE FSSODet.BaseBillableQty IS NULL;


UPDATE FSSODet
SET FSSODet.BaseBillableQty = CASE WHEN INUnit.UnitMultDiv = 'M' THEN ROUND(FSSODet.BillableQty * INUnit.UnitRate, CommonSetup.DecPlQty)
							   WHEN INUnit.UnitMultDiv = 'D' AND INUnit.UnitRate = 0 THEN 0
							   ELSE ROUND(FSSODet.BillableQty / INUnit.UnitRate, CommonSetup.DecPlQty) END
FROM FSSODet
INNER JOIN InventoryItem ON (InventoryItem.CompanyID = FSSODet.CompanyID 
								AND InventoryItem.InventoryID = FSSODet.InventoryID)
INNER JOIN INUnit ON (INUnit.CompanyID = FSSODet.CompanyID
						AND INUnit.ItemClassID = 0
						AND INUnit.InventoryID = 0
						AND INUnit.FromUnit = FSSODet.UOM
						AND INUnit.ToUnit = InventoryItem.BaseUnit)
INNER JOIN CommonSetup ON (CommonSetup.CompanyID = InventoryItem.CompanyID)
WHERE FSSODet.BaseBillableQty IS NULL;
GO

--[IfExists(Column = FSAppointmentDet.BaseBillableQty)]
--[IfExistsSelect(From = FSAppointmentDet, WhereIsNull = BaseBillableQty)]
--[MinVersion(Hash = 812c6431057aae390a21038fa5b05b742f1bf358b7f16d50f54a053230be2140)]
UPDATE FSAppointmentDet SET
    FSAppointmentDet.BaseBillableQty = FSAppointmentDet.BillableQty
WHERE
    FSAppointmentDet.InventoryID IS NULL;

UPDATE FSAppointmentDet
SET FSAppointmentDet.BaseBillableQty = CASE WHEN INUnit.UnitMultDiv = 'M' THEN ROUND(FSAppointmentDet.BillableQty * INUnit.UnitRate, CommonSetup.DecPlQty)
							   WHEN INUnit.UnitMultDiv = 'D' AND INUnit.UnitRate = 0 THEN 0
							   ELSE ROUND(FSAppointmentDet.BillableQty / INUnit.UnitRate, CommonSetup.DecPlQty) END
FROM FSAppointmentDet
INNER JOIN InventoryItem ON (InventoryItem.CompanyID = FSAppointmentDet.CompanyID 
								AND InventoryItem.InventoryID = FSAppointmentDet.InventoryID)
INNER JOIN INUnit ON (INUnit.CompanyID = InventoryItem.CompanyID
						AND INUnit.InventoryID = InventoryItem.InventoryID
						AND INUnit.FromUnit = FSAppointmentDet.UOM AND INUnit.ToUnit = InventoryItem.BaseUnit)
INNER JOIN CommonSetup ON (CommonSetup.CompanyID = InventoryItem.CompanyID)
WHERE FSAppointmentDet.BaseBillableQty IS NULL;

UPDATE FSAppointmentDet
SET FSAppointmentDet.BaseBillableQty = CASE WHEN INUnit.UnitMultDiv = 'M' THEN ROUND(FSAppointmentDet.BillableQty * INUnit.UnitRate, CommonSetup.DecPlQty)
							   WHEN INUnit.UnitMultDiv = 'D' AND INUnit.UnitRate = 0 THEN 0
							   ELSE ROUND(FSAppointmentDet.BillableQty / INUnit.UnitRate, CommonSetup.DecPlQty) END
FROM FSAppointmentDet
INNER JOIN InventoryItem ON (InventoryItem.CompanyID = FSAppointmentDet.CompanyID 
								AND InventoryItem.InventoryID = FSAppointmentDet.InventoryID)
INNER JOIN INItemClass ON (INItemClass.CompanyID = InventoryItem.CompanyID 
								AND INItemClass.ItemClassID = InventoryItem.ItemClassID)
INNER JOIN INUnit ON (INUnit.CompanyID = INItemClass.CompanyID
						AND INUnit.ItemClassID = INItemClass.ItemClassID
						AND INUnit.FromUnit = FSAppointmentDet.UOM AND INUnit.ToUnit = InventoryItem.BaseUnit)
INNER JOIN CommonSetup ON (CommonSetup.CompanyID = InventoryItem.CompanyID)
WHERE FSAppointmentDet.BaseBillableQty IS NULL;


UPDATE FSAppointmentDet
SET FSAppointmentDet.BaseBillableQty = CASE WHEN INUnit.UnitMultDiv = 'M' THEN ROUND(FSAppointmentDet.BillableQty * INUnit.UnitRate, CommonSetup.DecPlQty)
							   WHEN INUnit.UnitMultDiv = 'D' AND INUnit.UnitRate = 0 THEN 0
							   ELSE ROUND(FSAppointmentDet.BillableQty / INUnit.UnitRate, CommonSetup.DecPlQty) END
FROM FSAppointmentDet
INNER JOIN InventoryItem ON (InventoryItem.CompanyID = FSAppointmentDet.CompanyID 
								AND InventoryItem.InventoryID = FSAppointmentDet.InventoryID)
INNER JOIN INUnit ON (INUnit.CompanyID = FSAppointmentDet.CompanyID
						AND INUnit.ItemClassID = 0
						AND INUnit.InventoryID = 0
						AND INUnit.FromUnit = FSAppointmentDet.UOM
						AND INUnit.ToUnit = InventoryItem.BaseUnit)
INNER JOIN CommonSetup ON (CommonSetup.CompanyID = InventoryItem.CompanyID)
WHERE FSAppointmentDet.BaseBillableQty IS NULL;
GO

--[IfExists(Column = PMBillingRule.IncludeZeroAmount)]
--[MinVersion(Hash = fd4a62383abe54cf42742f7e52a2e581d94e94ecbaa389b700879ef9e0d3a06d)]
UPDATE PMBillingRule SET IncludeZeroAmountAndQty = 
    CASE WHEN [Type] = 'B'
	THEN IncludeZeroAmount
	ELSE 1 END
GO

--Moving Data from PMDetail into PMReccuringItem
--[MinVersion(Hash = 3bde7ba05e8fbfa635f0e7ef53835034c5b916a558d075a8826e80ef40668cfe)]
IF NOT EXISTS(SELECT TOP 1 * FROM PMRecurringItem)
	INSERT INTO [PMRecurringItem] ([CompanyID], [ProjectID], [TaskID], [InventoryID], [Description], [UOM], [Amount], [AccountID], [AccountSource], [SubMask], [SubID], [ResetUsage], [Included], [Used], [UsedTotal], [LastBilledDate], [LastBilledQty], [CreatedByID], [CreatedByScreenID], [CreatedDateTime],[LastModifiedByID],[LastModifiedByScreenID],[LastModifiedDateTime] ,[NoteID])
	SELECT [CompanyID], [ContractID], [TaskID], [InventoryID], [Description], [UOM], [ItemFee], [AccountID], [AccountSource], [SubMask], [SubID], [ResetUsage], [Included], [Used], [UsedTotal], [LastBilledDate], [LastBilledQty], [CreatedByID], [CreatedByScreenID], [CreatedDateTime],[LastModifiedByID],[LastModifiedByScreenID],[LastModifiedDateTime] , NEWID() FROM PMDetail
GO

--[MinVersion(Hash = f98bf70de0fe567d325162852552c19c33451683830506de8cb859b5f09a9248)]
update CashAccount set RestrictVisibilityWithBranch=0 
from CashAccount inner join 
FeaturesSet on FeaturesSet.CompanyID=CashAccount.CompanyID
where FeaturesSet.InterBranch=0 and FeaturesSet.Branch=1 and CashAccount.RestrictVisibilityWithBranch=1
GO

--[MinVersion(Hash = 8f16d1a92d3d9bd6b40b40fe22f68994254473cb84df47ad3965120e8ca226a9)]
UPDATE PMCostCode SET NoteID = NEWID() WHERE NoteID is null
GO

--[MinVersion(Hash = 78bec0b8ec2ca67966cbd27f05838f51b57e6d3332ffbdffac69da86d18a2d72)]
UPDATE InventoryItem 
SET	ValMethod = 'T',
	CompletePOLine = 'Q',
	MarkupPct = 0
WHERE ItemStatus = 'XX'
GO

--[MinVersion(Hash = 4271c3936392ee70792ac4a95341a8ad2bee15995c4e11a0d21aaa8d9a9b87b9)]
update INItemClass set noteid=newid() where noteid is null
GO

-- Populate the new field POLine.OrderNoteID (denormalization)
--[MinVersion(Hash = 3f659d4c56f147663926e269f587399c66d0e8ef8fa742808008d9900561eff0)]
UPDATE pol
SET OrderNoteID = COALESCE(po.NoteID, NEWID())
FROM POLine pol
LEFT JOIN POOrder po ON po.CompanyID = pol.CompanyID and po.OrderType = pol.OrderType and po.OrderNbr = pol.OrderNbr
WHERE pol.OrderNoteID IS NULL
GO

-- Populate the new fields POReceiptLine.POAccrualRefNoteID, POAccrualLineNbr
--[MinVersion(Hash = 8d8624e4ce30732d6b851ba58299083ee1d21351d37f90b74a571817ed6713ec)]
UPDATE prl
SET POAccrualRefNoteID = pol.OrderNoteID, POAccrualLineNbr = pol.LineNbr
FROM POReceiptLine prl
INNER JOIN POLine pol ON pol.CompanyID = prl.CompanyID AND pol.OrderType = prl.POType AND pol.OrderNbr = prl.PONbr AND pol.LineNbr = prl.POLineNbr
WHERE prl.POAccrualType = 'O' AND (prl.POAccrualRefNoteID IS NULL OR prl.POAccrualLineNbr IS NULL)

UPDATE prl
SET POAccrualRefNoteID = pr.NoteID, POAccrualLineNbr = prl.LineNbr
FROM POReceiptLine prl
INNER JOIN POReceipt pr ON pr.CompanyID = prl.CompanyID AND pr.ReceiptNbr = prl.ReceiptNbr
WHERE prl.POAccrualType = 'R' AND (prl.POAccrualRefNoteID IS NULL OR prl.POAccrualLineNbr IS NULL)

UPDATE POReceiptLine
SET POAccrualRefNoteID = NEWID(), POAccrualLineNbr = 0
WHERE POAccrualRefNoteID IS NULL OR POAccrualLineNbr IS NULL
GO

-- Populate the new fields APTran.POAccrualRefNoteID, POAccrualLineNbr
--[MinVersion(Hash = 9ba511ade89b17ea34db5c9e6c9c94d0a3e39795abbad7c22dd448382e56e7bf)]
UPDATE apt
SET POAccrualRefNoteID = pol.OrderNoteID, POAccrualLineNbr = pol.LineNbr
FROM APTran apt
INNER JOIN POLine pol ON pol.CompanyID = apt.CompanyID AND pol.OrderType = apt.POOrderType AND pol.OrderNbr = apt.PONbr AND pol.LineNbr = apt.POLineNbr
WHERE apt.POAccrualType = 'O' AND (apt.POAccrualRefNoteID IS NULL OR apt.POAccrualLineNbr IS NULL)

UPDATE apt
SET POAccrualRefNoteID = prl.POAccrualRefNoteID, POAccrualLineNbr = prl.POAccrualLineNbr
FROM APTran apt
INNER JOIN POReceiptLine prl ON prl.CompanyID = apt.CompanyID AND prl.ReceiptNbr = apt.ReceiptNbr AND prl.LineNbr = apt.ReceiptLineNbr
WHERE apt.POAccrualType = 'R' AND (apt.POAccrualRefNoteID IS NULL OR apt.POAccrualLineNbr IS NULL)
GO

--[MinVersion(Hash = 5c4b84b065d250b484ab13782e3dc6395c553789b83cab7635c9d0a4d7a3f9e2)]
update r
set r.ReceiptType = isnull(o.ReceiptType, 'RT')
from POOrderReceipt r
         left join POReceipt o
                    on r.CompanyID = o.CompanyID and r.ReceiptNbr = o.ReceiptNbr
where r.ReceiptType is null
GO

--AC-117429  populating of POReceipt.InvtDocType, InvtRefNbr for Receipts for Normal PO
--[MinVersion(Hash = 02d78df35961687a139be2de5b548247c81e6367b979789a31206786ba6bad4f)]
UPDATE [DST]
SET [DST].[InvtDocType] = [GR].[INDocType],
	[DST].[InvtRefNbr] = [GR].[INRefNbr]
FROM [POReceipt] [DST]
	INNER JOIN (
		SELECT [IT].[CompanyID], [IT].[POReceiptType], [IT].[POReceiptNbr], MAX([IT].[DocType]) AS [INDocType], MAX([IT].[RefNbr]) AS [INRefNbr] 
			FROM [INTran] [IT]
			WHERE [IT].[DocType] = 'R' AND [IT].[POReceiptType] IS NOT NULL AND [IT].[POReceiptNbr] IS NOT NULL
			GROUP BY [IT].[CompanyID], [IT].[POReceiptType], [IT].[POReceiptNbr]
		) [GR] ON  [DST].[CompanyID] = [GR].[CompanyID] AND [DST].[ReceiptType] = [GR].[POReceiptType] AND [DST].[ReceiptNbr] = [GR].[POReceiptNbr]
WHERE [DST].[ReceiptType] = 'RT' AND [DST].[InvtDocType] IS NULL
GO

--[MinVersion(Hash = 4211a6c02252975462096e8dfdcb08ec401b19a0738611ce42ce8a48c3d139b5)]
UPDATE EPEventShowAs SET NoteID = NEWID() WHERE NoteID IS NULL
GO

--[MinVersion(Hash = ec01749615f934f2099277e026756dec87f085d4fcbdbf89c9a037ca1b5ba76e)]
INSERT EPEventShowAsKVExt (CompanyID, RecordID, FieldName, ValueString)
SELECT l.CompanyID, t.NoteID, 'Description' + MAX(UPPER(SUBSTRING(l.LocaleName, 1, 2))), MAX(t.Description)
FROM EPEventShowAs t
INNER JOIN Locale l ON l.IsDefault = 1
LEFT JOIN EPEventShowAsKVExt e ON e.CompanyID = l.CompanyID AND e.RecordID = t.NoteID AND e.FieldName = 'Description' + UPPER(SUBSTRING(l.LocaleName, 1, 2))
WHERE e.CompanyID IS NULL
GROUP BY l.CompanyID, t.NoteID
GO

--[MinVersion(Hash = 4ab448a631a18aae7cbbfe1b7cf7bc62c4d4e527792e7c8747c87afb7d124c97)]
UPDATE PMTask SET NoteID = NEWID() WHERE NoteID IS NULL

INSERT PMTaskKVExt (CompanyID, RecordID, FieldName, ValueString)
SELECT l.CompanyID, t.NoteID, 'Description' + MAX(UPPER(SUBSTRING(l.LocaleName, 1, 2))), MAX(t.Description)
FROM PMTask t
INNER JOIN Locale l ON l.IsDefault = 1
LEFT JOIN PMTaskKVExt e ON e.CompanyID = l.CompanyID AND e.RecordID = t.NoteID AND e.FieldName = 'Description' + UPPER(SUBSTRING(l.LocaleName, 1, 2))
WHERE e.CompanyID IS NULL
GROUP BY l.CompanyID, t.NoteID
GO

--[MinVersion(Hash = a9d98ab7f01d1cfe75f913b12d046f729e24b44533e796b65610894c7fdc06f3)]
UPDATE PMAccountGroup SET NoteID = NEWID() WHERE NoteID IS NULL

INSERT PMAccountGroupKVExt (CompanyID, RecordID, FieldName, ValueString)
SELECT l.CompanyID, t.NoteID, 'Description' + MAX(UPPER(SUBSTRING(l.LocaleName, 1, 2))), MAX(t.Description)
FROM PMAccountGroup t
INNER JOIN Locale l ON l.IsDefault = 1
LEFT JOIN PMAccountGroupKVExt e ON e.CompanyID = l.CompanyID AND e.RecordID = t.NoteID AND e.FieldName = 'Description' + UPPER(SUBSTRING(l.LocaleName, 1, 2))
WHERE e.CompanyID IS NULL
GROUP BY l.CompanyID, t.NoteID
GO

--[MinVersion(Hash = df443dee6d2ad49f441f3c8872030939aec176e23a6771f9ca892236d9342331)]
INSERT PMCostCodeKVExt (CompanyID, RecordID, FieldName, ValueString)
SELECT l.CompanyID, t.NoteID, 'Description' + MAX(UPPER(SUBSTRING(l.LocaleName, 1, 2))), MAX(t.Description)
FROM PMCostCode t
INNER JOIN Locale l ON l.IsDefault = 1
LEFT JOIN PMCostCodeKVExt e ON e.CompanyID = l.CompanyID AND e.RecordID = t.NoteID AND e.FieldName = 'Description' + UPPER(SUBSTRING(l.LocaleName, 1, 2))
WHERE e.CompanyID IS NULL
GROUP BY l.CompanyID, t.NoteID
GO

-- Insert POAccrualSplit records for legacy data (only receipt-based billing)
--[mssql: Skip]
--[MinVersion(Hash = 66ecd5b6b5f7b93e10e6bd5751997c22759bb5044bc13fc0080ad3b55d21bc58)]
INSERT INTO [POAccrualSplit] (
	[CompanyID], [RefNoteID], [LineNbr], [Type],
	[APDocType], [APRefNbr], [APLineNbr],
	[POReceiptNbr], [POReceiptLineNbr],
	[UOM], [AccruedQty],
	[BaseAccruedQty],
	[AccruedCost],
	[PPVAmt], [IsReversed],
	[CreatedDateTime], [CreatedByID], [CreatedByScreenID],
	[LastModifiedDateTime], [LastModifiedByID], [LastModifiedByScreenID])
SELECT prl.[CompanyID], prl.[POAccrualRefNoteID], prl.[LineNbr], prl.[POAccrualType],
	apt.[TranType], apt.[RefNbr], apt.[LineNbr],
	prl.[ReceiptNbr], prl.[LineNbr],
	apt.[UOM], apt.[Qty] * (CASE WHEN apt.[DrCr] = 'D' THEN 1 ELSE -1 END),
	COALESCE(apt.[BaseQty] * (CASE WHEN apt.[DrCr] = 'D' THEN 1 ELSE -1 END), 0),
	COALESCE((CASE WHEN inclTax.[CompanyID] IS NOT NULL THEN inclTax.[TaxableAmt] + COALESCE(inclTax.[RetainedTaxableAmt], 0) + apt.[TranAmt] - ROUND(apt.[TranAmt] * apt.[GroupDiscountRate] * apt.[DocumentDiscountRate], COALESCE(cl.[DecimalPlaces], 2))
		ELSE apt.[TranAmt] + COALESCE(apt.[RetainageAmt], 0) END) * (CASE WHEN apt.[DrCr] = 'D' THEN 1 ELSE -1 END), 0),
	apt.[POPPVAmt], 0,
	apt.[LastModifiedDateTime], apt.[LastModifiedByID], apt.[LastModifiedByScreenID],
	apt.[LastModifiedDateTime], apt.[LastModifiedByID], apt.[LastModifiedByScreenID]
FROM [POReceiptLine] prl
INNER JOIN [APTran] apt
	ON apt.[CompanyID] = prl.[CompanyID]
	AND apt.[ReceiptNbr] = prl.[ReceiptNbr]
	AND apt.[ReceiptLineNbr] = prl.[LineNbr]
LEFT JOIN (
	SELECT aptx.[CompanyID], aptx.[TranType], aptx.[RefNbr], aptx.[LineNbr], MIN(aptx.[TaxableAmt]) as TaxableAmt, MIN(aptx.[RetainedTaxableAmt]) as RetainedTaxableAmt
	FROM [APTax] aptx
	INNER JOIN [Tax] t ON t.[CompanyID] = aptx.[CompanyID] AND t.[TaxID] = aptx.[TaxID]
	WHERE t.[TaxCalcLevel] = '0' AND t.[TaxType] <> 'W' AND t.[ReverseTax] = 0
	GROUP BY aptx.CompanyID, aptx.TranType, aptx.RefNbr, aptx.LineNbr) inclTax
	ON inclTax.[CompanyID] = apt.[CompanyID] AND inclTax.[TranType] = apt.[TranType] AND inclTax.[RefNbr] = apt.[RefNbr] AND inclTax.[LineNbr] = apt.[LineNbr]
INNER JOIN [Company] c ON c.[CompanyID] = apt.[CompanyID]
LEFT JOIN [CurrencyList] cl ON cl.[CompanyID] = c.[CompanyID] AND cl.[CuryID] = c.[BaseCuryID]
LEFT JOIN [POAccrualSplit] pas
	ON pas.[CompanyID] = prl.[CompanyID]
	AND pas.[RefNoteID] = prl.[POAccrualRefNoteID]
	AND pas.[LineNbr] = prl.[LineNbr]
	AND pas.[Type] = prl.[POAccrualType]
	AND pas.[APDocType] = apt.[TranType]
	AND pas.[APRefNbr] = apt.[RefNbr]
	AND pas.[APLineNbr] = apt.[LineNbr]
	AND pas.[POReceiptNbr] = prl.[ReceiptNbr]
	AND pas.[POReceiptLineNbr] = prl.[LineNbr]
WHERE pas.[CompanyID] IS NULL
	AND prl.[POAccrualType] = 'R'
	AND prl.[Released] = 1
	AND apt.[Released] = 1
GO

--Duplicated script for MS SQL turns off the parallelism for better performance (option - maxdop 1)
--[mysql: Skip]
--[mssql: Native]
--[OldHash(Hash = 66ecd5b6b5f7b93e10e6bd5751997c22759bb5044bc13fc0080ad3b55d21bc58)]
--[MinVersion(Hash = d5bc54d4dfb47a1985857b747ffdaef9fb23003f9211b0c45320dd30ca126318)]
INSERT INTO [POAccrualSplit] (
	[CompanyID], [RefNoteID], [LineNbr], [Type],
	[APDocType], [APRefNbr], [APLineNbr],
	[POReceiptNbr], [POReceiptLineNbr],
	[UOM], [AccruedQty],
	[BaseAccruedQty],
	[AccruedCost],
	[PPVAmt], [IsReversed],
	[CreatedDateTime], [CreatedByID], [CreatedByScreenID],
	[LastModifiedDateTime], [LastModifiedByID], [LastModifiedByScreenID])
SELECT prl.[CompanyID], prl.[POAccrualRefNoteID], prl.[LineNbr], prl.[POAccrualType],
	apt.[TranType], apt.[RefNbr], apt.[LineNbr],
	prl.[ReceiptNbr], prl.[LineNbr],
	apt.[UOM], apt.[Qty] * (CASE WHEN apt.[DrCr] = 'D' THEN 1 ELSE -1 END),
	COALESCE(apt.[BaseQty] * (CASE WHEN apt.[DrCr] = 'D' THEN 1 ELSE -1 END), 0),
	COALESCE((CASE WHEN inclTax.[CompanyID] IS NOT NULL THEN inclTax.[TaxableAmt] + COALESCE(inclTax.[RetainedTaxableAmt], 0) + apt.[TranAmt] - ROUND(apt.[TranAmt] * apt.[GroupDiscountRate] * apt.[DocumentDiscountRate], COALESCE(cl.[DecimalPlaces], 2))
		ELSE apt.[TranAmt] + COALESCE(apt.[RetainageAmt], 0) END) * (CASE WHEN apt.[DrCr] = 'D' THEN 1 ELSE -1 END), 0),
	apt.[POPPVAmt], 0,
	apt.[LastModifiedDateTime], apt.[LastModifiedByID], apt.[LastModifiedByScreenID],
	apt.[LastModifiedDateTime], apt.[LastModifiedByID], apt.[LastModifiedByScreenID]
FROM [POReceiptLine] prl
INNER JOIN [APTran] apt
	ON apt.[CompanyID] = prl.[CompanyID]
	AND apt.[ReceiptNbr] = prl.[ReceiptNbr]
	AND apt.[ReceiptLineNbr] = prl.[LineNbr]
LEFT JOIN (
	SELECT aptx.[CompanyID], aptx.[TranType], aptx.[RefNbr], aptx.[LineNbr], MIN(aptx.[TaxableAmt]) as TaxableAmt, MIN(aptx.[RetainedTaxableAmt]) as RetainedTaxableAmt
	FROM [APTax] aptx
	INNER JOIN [Tax] t ON t.[CompanyID] = aptx.[CompanyID] AND t.[TaxID] = aptx.[TaxID]
	WHERE t.[TaxCalcLevel] = '0' AND t.[TaxType] <> 'W' AND t.[ReverseTax] = 0
	GROUP BY aptx.CompanyID, aptx.TranType, aptx.RefNbr, aptx.LineNbr) inclTax
	ON inclTax.[CompanyID] = apt.[CompanyID] AND inclTax.[TranType] = apt.[TranType] AND inclTax.[RefNbr] = apt.[RefNbr] AND inclTax.[LineNbr] = apt.[LineNbr]
INNER JOIN [Company] c ON c.[CompanyID] = apt.[CompanyID]
LEFT JOIN [CurrencyList] cl ON cl.[CompanyID] = c.[CompanyID] AND cl.[CuryID] = c.[BaseCuryID]
LEFT JOIN [POAccrualSplit] pas
	ON pas.[CompanyID] = prl.[CompanyID]
	AND pas.[RefNoteID] = prl.[POAccrualRefNoteID]
	AND pas.[LineNbr] = prl.[LineNbr]
	AND pas.[Type] = prl.[POAccrualType]
	AND pas.[APDocType] = apt.[TranType]
	AND pas.[APRefNbr] = apt.[RefNbr]
	AND pas.[APLineNbr] = apt.[LineNbr]
	AND pas.[POReceiptNbr] = prl.[ReceiptNbr]
	AND pas.[POReceiptLineNbr] = prl.[LineNbr]
WHERE pas.[CompanyID] IS NULL
	AND prl.[POAccrualType] = 'R'
	AND prl.[Released] = 1
	AND apt.[Released] = 1
option (maxdop 1)
GO

--[MinVersion(Hash = d66523be6bb271463c32e3ddc80517bfd1a7aface17ffeaa6fd3b0345d1101d3)]
update rl
set rl.INReleased = 1
from POReceiptLine rl
inner join INTran t on
		t.CompanyID = rl.CompanyID
	and t.POReceiptNbr = rl.ReceiptNbr
	and t.POReceiptLineNbr = rl.LineNbr
where
		t.Released = 1
	and (rl.INReleased IS NULL or rl.INReleased = 0)
GO

--[IfExists(Column = EPLoginType.AllowedLoginType)]
--[IfExistsSelect(From = EPLoginType, WhereIsNull = AllowedLoginType)]
--[MinVersion(Hash = fdd7572b1c4cf48827685005fa70ddd34c831ebc9adc2aef5f15d66071b92f5c)]
UPDATE EPLoginType
SET AllowedLoginType = 'R'
WHERE AllowedLoginType IS NULL
GO

--[mssql: Native]
--[mysql: Skip]
-- Adjust BAccount.GroupMask size
--[MinVersion(Hash = b8a6a13e7bcaf31d21bbf202d6da3f5d5277e1a3ad0d2d734a46d79057f54673)]
declare @groupMaskLength int;

select top 1 @groupMaskLength = CHARACTER_MAXIMUM_LENGTH
from INFORMATION_SCHEMA.COLUMNS
where TABLE_NAME = 'RelationGroup' and COLUMN_NAME = 'GroupMask'

if @groupMaskLength > 32
begin
	declare @alterStatement nvarchar(max) =
	N'alter table BAccount 
	alter column GroupMask varbinary(' + cast(@groupMaskLength as nvarchar(max)) + N') null';
	exec sp_executesql @alterStatement
end

-- Sync GroupMask for BAccount that is Customer
update BAccount
set GroupMask = Result.GroupMask
from
(
	select BAccount_Inner.CompanyID,
		   BAccount_Inner.BAccountID,
		   Customer.GroupMask
	from BAccount as BAccount_Inner
	left join Customer
	on Customer.CompanyID = BAccount_Inner.CompanyID and
	   Customer.BAccountID = BAccount_Inner.BAccountID
	where BAccount_Inner.Type = 'CU' and
		  Customer.BAccountID is not null
) as Result
where BAccount.CompanyID = Result.CompanyID and
	  BAccount.BAccountID = Result.BAccountID

-- Sync GroupMask for BAccount that is Vendor
update BAccount
set GroupMask = Result.GroupMask
from
(
	select BAccount_Inner.CompanyID,
		   BAccount_Inner.BAccountID,
		   Vendor.GroupMask
	from BAccount as BAccount_Inner
	left join Vendor
	on Vendor.CompanyID = BAccount_Inner.CompanyID and
	   Vendor.BAccountID = BAccount_Inner.BAccountID
	where BAccount_Inner.Type = 'VE' and
		  Vendor.BAccountID is not null
) as Result
where BAccount.CompanyID = Result.CompanyID and
	  BAccount.BAccountID = Result.BAccountID

-- Sync GroupMask for BAccount that is Customer&Vendor
update BAccount
set GroupMask = dbo.binaryMaskOr(Result.CustomerGroupMask, Result.VendorGroupMask)
from
(
	select BAccount_Inner.CompanyID,
		   BAccount_Inner.BAccountID,
		   Customer.GroupMask as CustomerGroupMask,
		   Vendor.GroupMask as VendorGroupMask
	from BAccount as BAccount_Inner
	left join Customer
	on Customer.CompanyID = BAccount_Inner.CompanyID and
	   Customer.BAccountID = BAccount_Inner.BAccountID
	left join Vendor
	on Vendor.CompanyID = BAccount_Inner.CompanyID and
	   Vendor.BAccountID = BAccount_Inner.BAccountID
	where BAccount_Inner.Type = 'VC' and
		  Customer.BAccountID is not null and
		  Vendor.BAccountID is not null
) as Result
where BAccount.CompanyID = Result.CompanyID and
	  BAccount.BAccountID = Result.BAccountID

-- Generate Neighbour for BAccount
declare @companyId int;
declare companyId_cursor cursor
for select CompanyID from Company;

open companyId_cursor;
fetch next from companyId_cursor
into @companyId;

while @@FETCH_STATUS = 0
begin
	declare @coverageMask varbinary(128);
	declare @inverseMask varbinary(128);
	declare @winCoverageMask varbinary(128);
	declare @winInverseMask varbinary(128);
	declare @neighbourExists bit = 0;

	-- Sync Users-BAccount Neighbours
	select @coverageMask = CoverageMask,
		   @inverseMask = InverseMask,
		   @winCoverageMask = WinCoverageMask,
		   @winInverseMask = WinInverseMask,
		   @neighbourExists = 1
	from Neighbour
	where CompanyID = @companyId and
		  LeftEntityType = 'PX.SM.Users' and
		  RightEntityType = 'PX.Objects.AR.Customer'

	select @coverageMask = dbo.binaryMaskOr(@coverageMask, CoverageMask),
		   @inverseMask = dbo.binaryMaskOr(@inverseMask, InverseMask),
		   @winCoverageMask = dbo.binaryMaskOr(@winCoverageMask, WinCoverageMask),
		   @winInverseMask = dbo.binaryMaskOr(@winInverseMask, WinInverseMask),
		   @neighbourExists = 1
	from Neighbour
	where CompanyID = @companyId and
		  LeftEntityType = 'PX.SM.Users' and
		  RightEntityType = 'PX.Objects.AP.Vendor'

	if @neighbourExists = 1 and
	   exists
	(
		select *
		from Neighbour
		where CompanyID = @companyId and
		      LeftEntityType = 'PX.SM.Users' and
			  RightEntityType = 'PX.Objects.CR.BAccount'
	)
	begin
		update Neighbour
		set CoverageMask = @coverageMask,
			InverseMask = @inverseMask,
			WinCoverageMask = @winCoverageMask,
			WinInverseMask = @winInverseMask
		where CompanyID = @companyId and
		      LeftEntityType = 'PX.SM.Users' and
			  RightEntityType = 'PX.Objects.CR.BAccount'
	end
	else if @neighbourExists = 1
	begin
		insert into Neighbour
		(
			CompanyID,
			LeftEntityType,
			RightEntityType,
			CoverageMask,
			InverseMask,
			WinCoverageMask,
			WinInverseMask
		)
		values
		(
			@companyId,
			'PX.SM.Users',
			'PX.Objects.CR.BAccount',
			@coverageMask,
			@inverseMask,
			@winCoverageMask,
			@winInverseMask
		)
	end

	set @neighbourExists = 0;

	-- Sync BAccount-Users Neighbours
	select @coverageMask = CoverageMask,
		   @inverseMask = InverseMask,
		   @winCoverageMask = WinCoverageMask,
		   @winInverseMask = WinInverseMask,
		   @neighbourExists = 1
	from Neighbour
	where CompanyID = @companyId and
		  LeftEntityType = 'PX.Objects.AR.Customer' and
		  RightEntityType = 'PX.SM.Users'

	select @coverageMask = dbo.binaryMaskOr(@coverageMask, CoverageMask),
		   @inverseMask = dbo.binaryMaskOr(@inverseMask, InverseMask),
		   @winCoverageMask = dbo.binaryMaskOr(@winCoverageMask, WinCoverageMask),
		   @winInverseMask = dbo.binaryMaskOr(@winInverseMask, WinInverseMask),
		   @neighbourExists = 1
	from Neighbour
	where CompanyID = @companyId and
		  LeftEntityType = 'PX.Objects.AP.Vendor' and
		  RightEntityType = 'PX.SM.Users'

	if @neighbourExists = 1 and
	   exists
	(
		select *
		from Neighbour
		where CompanyID = @companyId and
		      LeftEntityType = 'PX.Objects.CR.BAccount' and
			  RightEntityType = 'PX.SM.Users'
	)
	begin
		update Neighbour
		set CoverageMask = @coverageMask,
			InverseMask = @inverseMask,
			WinCoverageMask = @winCoverageMask,
			WinInverseMask = @winInverseMask
		where CompanyID = @companyId and
		      LeftEntityType = 'PX.Objects.CR.BAccount' and
			  RightEntityType = 'PX.SM.Users'
	end
	else if @neighbourExists = 1
	begin
		insert into Neighbour
		(
			CompanyID,
			LeftEntityType,
			RightEntityType,
			CoverageMask,
			InverseMask,
			WinCoverageMask,
			WinInverseMask
		)
		values
		(
			@companyId,
			'PX.Objects.CR.BAccount',
			'PX.SM.Users',
			@coverageMask,
			@inverseMask,
			@winCoverageMask,
			@winInverseMask
		)
	end

	fetch next from companyId_cursor
	into @companyId;
end

close companyId_cursor;
deallocate companyId_cursor;
GO

--[mssql: Skip]
--[mysql: Native]
--[MinVersion(Hash = bdd82c91d5989bc6ac3be49074744895526743e5e962a7351a4d73834480021d)]
DELIMITER $$;
DROP PROCEDURE IF EXISTS AdjustGroupMaskLength;
CREATE PROCEDURE AdjustGroupMaskLength()
BEGIN
	SELECT CHARACTER_MAXIMUM_LENGTH
	INTO @groupMaskLength
	FROM information_schema.COLUMNS
	WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'RelationGroup' AND COLUMN_NAME = 'GroupMask' 
	LIMIT 1;

	IF @groupMaskLength > 32 THEN
	BEGIN
		SET @alterStatement = CONCAT('ALTER TABLE BAccount MODIFY COLUMN GroupMask VARBINARY(', @groupMaskLength, ') NOT DEFAULT ''0''');
		PREPARE stmt FROM @alterStatement;
		EXECUTE stmt;
		DEALLOCATE PREPARE stmt;
	END;
	END IF;
END;

CALL AdjustGroupMaskLength;
DROP PROCEDURE AdjustGroupMaskLength;
GO

--[mssql: Skip]
--[mysql: Native]
-- Sync GroupMask for BAccount that is Customer
--[MinVersion(Hash = c784912b5b6500c8f89e439df8c5e0d24f955336fd52a537916890729fffccad)]
UPDATE BAccount
LEFT JOIN Customer
ON Customer.CompanyID = BAccount.CompanyID AND
	Customer.BAccountID = BAccount.BAccountID
SET BAccount.GroupMask = Customer.GroupMask
WHERE BAccount.Type = 'CU' AND
		Customer.BAccountID IS NOT NULL;

-- Sync GroupMask for BAccount that is Vendor
UPDATE BAccount
LEFT JOIN Vendor
ON Vendor.CompanyID = BAccount.CompanyID AND
	Vendor.BAccountID = BAccount.BAccountID
SET BAccount.GroupMask = Vendor.GroupMask
WHERE BAccount.Type = 'VE' AND
		Vendor.BAccountID IS NOT NULL;

-- Sync GroupMask for BAccount that is Customer&Vendor
UPDATE BAccount
LEFT JOIN Customer
ON Customer.CompanyID = BAccount.CompanyID AND
	Customer.BAccountID = BAccount.BAccountID
LEFT JOIN Vendor
ON Vendor.CompanyID = BAccount.CompanyID AND
	Vendor.BAccountID = BAccount.BAccountID
SET BAccount.GroupMask = binaryMaskOr(Customer.GroupMask, Vendor.GroupMask)
WHERE BAccount.Type = 'VC' AND
		Customer.BAccountID IS NOT NULL AND
		Vendor.BAccountID IS NOT NULL;
GO

--[mssql: Skip]
--[mysql: Native]
--[OldHash(Hash = 65ec1e49e01dd006e2d3dedd1faed417aefa033f29385fdab9c608d98dc0a6dd)]
--[MinVersion(Hash = a1a903ffacbfc82ea09abbb3491f3425bcc2ba531287a629bf74666a974cf010)]
DELIMITER $$;
drop procedure if exists SyncBAccountGroupMask;

CREATE PROCEDURE SyncBAccountGroupMask()
BEGIN
	-- Generate Neighbour for BAccount
	declare covMask varbinary(128);
	declare invMask varbinary(128);
	declare winCovMask varbinary(128);
	declare winInvMask varbinary(128);
	declare neighbourExists int default FALSE;

	declare cmId int;
	DECLARE done INT;
	declare companyId_cursor cursor for select CompanyID from Company;
	DECLARE CONTINUE HANDLER FOR NOT FOUND SET done = TRUE;

	open companyId_cursor;

	read_loop: LOOP
		SET cmId = 0;
		fetch next from companyId_cursor into cmId;
		IF cmId = 0 THEN
			LEAVE read_loop;
		END IF;

		set neighbourExists = FALSE;

		-- Sync Users-BAccount Neighbours
		select CoverageMask,
			   InverseMask,
			   WinCoverageMask,
			   WinInverseMask,
			   TRUE
		into covMask,
			 invMask,
			 winCovMask,
			 winInvMask,
			 neighbourExists
		from Neighbour
		where CompanyID = cmId and
			LeftEntityType = 'PX.SM.Users' and
			RightEntityType = 'PX.Objects.AR.Customer'
		limit 1;

		select binaryMaskOr(covMask, CoverageMask),
			   binaryMaskOr(invMask, InverseMask),
			   binaryMaskOr(winCovMask, WinCoverageMask),
			   binaryMaskOr(winInvMask, WinInverseMask),
			   TRUE
		into covMask,
			 invMask,
			 winCovMask,
			 winInvMask,
			 neighbourExists
		from Neighbour
		where CompanyID = cmId and
			LeftEntityType = 'PX.SM.Users' and
			RightEntityType = 'PX.Objects.AP.Vendor'
		limit 1;

		if neighbourExists and
		exists
		(
			select *
			from Neighbour
			where CompanyID = cmId and
				LeftEntityType = 'PX.SM.Users' and
				RightEntityType = 'PX.Objects.CR.BAccount'
		) then
		begin
			update Neighbour
			set CoverageMask = covMask,
				InverseMask = invMask,
				WinCoverageMask = winCovMask,
				WinInverseMask = winInvMask
			where CompanyID = cmId and
				LeftEntityType = 'PX.SM.Users' and
				RightEntityType = 'PX.Objects.CR.BAccount';
		end;
		elseif neighbourExists then
		begin
			insert into Neighbour
			(
				CompanyID,
				LeftEntityType,
				RightEntityType,
				CoverageMask,
				InverseMask,
				WinCoverageMask,
				WinInverseMask
			)
			values
			(
				cmId,
				'PX.SM.Users',
				'PX.Objects.CR.BAccount',
				covMask,
				invMask,
				winCovMask,
				winInvMask
			);
		end;
		end if;

		set neighbourExists = FALSE;

		-- Sync BAccount-Users Neighbours
		select CoverageMask,
			   InverseMask,
			   WinCoverageMask,
			   WinInverseMask,
			   TRUE
		into covMask,
			 invMask,
			 winCovMask,
			 winInvMask,
			 neighbourExists
		from Neighbour
		where CompanyID = cmId and
			LeftEntityType = 'PX.Objects.AR.Customer' and
			RightEntityType = 'PX.SM.Users'
		limit 1;

		select binaryMaskOr(covMask, CoverageMask),
			   binaryMaskOr(invMask, InverseMask),
			   binaryMaskOr(winCovMask, WinCoverageMask),
			   binaryMaskOr(winInvMask, WinInverseMask),
			   TRUE
		into covMask,
			 invMask,
			 winCovMask,
			 winInvMask,
			 neighbourExists
		from Neighbour
		where CompanyID = cmId and
			LeftEntityType = 'PX.Objects.AP.Vendor' and
			RightEntityType = 'PX.SM.Users'
		limit 1;

		if neighbourExists and
		exists
		(
			select *
			from Neighbour
			where CompanyID = cmId and
				LeftEntityType = 'PX.Objects.CR.BAccount' and
				RightEntityType = 'PX.SM.Users'
		) then
		begin
			update Neighbour
			set CoverageMask = covMask,
				InverseMask = invMask,
				WinCoverageMask = winCovMask,
				WinInverseMask = winInvMask
			where CompanyID = cmId and
				LeftEntityType = 'PX.Objects.CR.BAccount' and
				RightEntityType = 'PX.SM.Users';
		end;
		elseif neighbourExists then
		begin
			insert into Neighbour
			(
				CompanyID,
				LeftEntityType,
				RightEntityType,
				CoverageMask,
				InverseMask,
				WinCoverageMask,
				WinInverseMask
			)
			values
			(
				cmId,
				'PX.Objects.CR.BAccount',
				'PX.SM.Users',
				covMask,
				invMask,
				winCovMask,
				winInvMask
			);
		end;
		end if;
	END LOOP read_loop;

	close companyId_cursor;
END;

CALL SyncBAccountGroupMask;
DROP PROCEDURE SyncBAccountGroupMask;
GO

-- Update RolesInGraph for CR3060BI from CR306000 
--[MinVersion(Hash = 5e61508c0d905333bc31b4c17b36b82d6d0b06e9423d1ba6b4383c7ccdb05abb)]
IF NOT EXISTS(Select * FROM RolesInGraph WHERE CompanyID <> 1 AND ScreenID = 'CR3060BI')
INSERT INTO RolesInGraph (
	CompanyID,
	ScreenID,
	Rolename,
	ApplicationName,
	Accessrights,
	CompanyMask,
	CreatedByID,
	CreatedByScreenID,
	CreatedDateTime,
	LastModifiedByID,
	LastModifiedByScreenID,
	LastModifiedDateTime,
	RecordSourceID)
	SELECT
		CompanyID,
		'CR3060BI',
		Rolename,
		ApplicationName,
		Accessrights,
		CompanyMask,
		CreatedByID,
		CreatedByScreenID,
		CreatedDateTime,
		LastModifiedByID,
		LastModifiedByScreenID,
		LastModifiedDateTime,
		RecordSourceID
	FROM RolesInGraph
	WHERE CompanyID <> 1 AND ScreenID = 'CR306000'
GO

-- Update CRActivity Type for ClassID Task = 0, Event = 1, Email = 4;
--[MinVersion(Hash = 787f3fd83024c9abda6c7e53fb94c15725f9dd779f4eedbe8a26df72a23c5387)]
UPDATE CRActivity
	SET 
		[Type] = CASE
					WHEN ClassID = 0
					THEN 'AT'
					WHEN ClassID = 1
					THEN 'AA'
					WHEN ClassID = 4
					THEN 'AE'
				END
	WHERE [Type] IS NULL AND ClassID IN (0, 1, 4)
GO

-- Update EPActivityType ClassID
--[MinVersion(Hash = 9f9efdc1242580265217a9dae90aab157ff692e1231bd62e3e9683db07b31a8c)]
UPDATE EPActivityType
	SET 
		ClassID = CASE
					WHEN [Type] = 'AT'
					THEN 0
					WHEN [Type] = 'AA'
					THEN 1 
					WHEN [Type] = 'AE'
					THEN 4
					ELSE 2
				END
GO

-- Update RolesInGraph for CR3010BI from CR301000
--[MinVersion(Hash = 26e3bc4b4e395e9218de6ecb980f4b54e336639dbc5cdfda936877ce4c0db847)]
IF NOT EXISTS(Select * FROM RolesInGraph WHERE CompanyID <> 1 AND ScreenID = 'CR3010BI')
INSERT INTO RolesInGraph (
	CompanyID,
	ScreenID,
	Rolename,
	ApplicationName,
	Accessrights,
	CompanyMask,
	CreatedByID,
	CreatedByScreenID,
	CreatedDateTime,
	LastModifiedByID,
	LastModifiedByScreenID,
	LastModifiedDateTime,
	RecordSourceID)
	SELECT
		CompanyID,
		'CR3010BI',
		Rolename,
		ApplicationName,
		Accessrights,
		CompanyMask,
		CreatedByID,
		CreatedByScreenID,
		CreatedDateTime,
		LastModifiedByID,
		LastModifiedByScreenID,
		LastModifiedDateTime,
		RecordSourceID
	FROM RolesInGraph
	WHERE CompanyID <> 1 AND ScreenID = 'CR301000'
GO

-- Update RolesInGraph for CR3011BI from CR301000
--[MinVersion(Hash = c3fd166b4521f20cddd329880de8ad8502c9527b7bdc3f3061633e2bdf05b99c)]
IF NOT EXISTS(Select * FROM RolesInGraph WHERE CompanyID <> 1 AND ScreenID = 'CR3011BI')
INSERT INTO RolesInGraph (
	CompanyID,
	ScreenID,
	Rolename,
	ApplicationName,
	Accessrights,
	CompanyMask,
	CreatedByID,
	CreatedByScreenID,
	CreatedDateTime,
	LastModifiedByID,
	LastModifiedByScreenID,
	LastModifiedDateTime,
	RecordSourceID)
	SELECT
		CompanyID,
		'CR3011BI',
		Rolename,
		ApplicationName,
		Accessrights,
		CompanyMask,
		CreatedByID,
		CreatedByScreenID,
		CreatedDateTime,
		LastModifiedByID,
		LastModifiedByScreenID,
		LastModifiedDateTime,
		RecordSourceID
	FROM RolesInGraph
	WHERE CompanyID <> 1 AND ScreenID = 'CR301000'
GO

--[MinVersion(Hash = a3febb95d0e0173997d6814ff5402426b59b2dd90d741d22d58a4d3a14a0165a)]
UPDATE WZScenario 
SET WorkspaceID = MUIScreen.WorkspaceID,
    SubcategoryID = MUIScreen.SubcategoryID
FROM MUIScreen
WHERE WZScenario.ScenarioID = MUIScreen.NodeID
AND WZScenario.WorkspaceID IS NULL
GO



--[RefreshMetadata()]
GO

--CRM scripts
--[MinVersion(Hash = 4034287911b9752353d27a074b9aeab97d092a15d1ee01bc15006b3b68cecbce)]

GO

--[IfExists(Column = EPRule.IsActive)]
--[MinVersion(Hash = 9afe7046c6e6d71ca873e9192bb596e7f88b3617ed730fca63b2cfc1cf22295c)]
UPDATE EPRule
SET EPRule.IsActive = 1
WHERE (EPRule.IsActive IS NULL)
GO

--[IfExists(Column = EPRuleCondition.IsActive)]
--[MinVersion(Hash = ead386324b5d5f1b8250ace29ad24b9578e83b25e0d7176e49dea85f93277512)]
UPDATE EPRuleCondition
SET EPRuleCondition.IsActive = 1
WHERE (EPRuleCondition.IsActive IS NULL)
GO

--[IfExists(Column = EPRule.ReasonForApprove)]
--[MinVersion(Hash = dd699e1fdf608b486740f4461bd72fcd70f81d4361238f2a87b792aa9b8aa8d3)]
UPDATE EPRule SET ReasonForApprove = 'N' WHERE ReasonForApprove IS NULL
GO

--[IfExists(Column = EPRule.ReasonForReject)]
--[MinVersion(Hash = 7ce28aebdfe0c476cb8f648b71c9bb56b5b2e1066140c109f56271f4ec7eeb0c)]
UPDATE EPRule SET ReasonForReject = 'N' WHERE ReasonForReject IS NULL
GO

--[IfExists(Column = EPRule.ExecuteStep)]
--[MinVersion(Hash = 8265e4ab23143fd4d69fd8b190dff25b209544c2d0e209895dd84b1c2c70d5c1)]
UPDATE EPRule SET ExecuteStep = 'A' WHERE ExecuteStep IS NULL
GO

--[MinVersion(Hash = 7e4a3bd02cef4da9d5e45404509af31781280959724404ef577831901c2a5a54)]
UPDATE EmailAccount SET SenderDisplayNameSource = 'E' WHERE SenderDisplayNameSource IS NULL
GO

--[MinVersion(Hash = 2786e0944db1d07cc8a37be644c613318b182f19ea3701527d5ca8560cb700e6)]
UPDATE EMailAccount SET RouteEmployeeEmails=IncomingProcessing WHERE RouteEmployeeEmails IS NULL
GO

-- ARTran update CRCase references
--[IfExists(Table = ARTran)]
--[IfExists(Column = ARTran.CaseID)]
--[MinVersion(Hash = 7250fd2fc522b857ac85cc768c39f9f9b4de9b8f343e416d502c52dc65b5fb45)]
UPDATE ARTran SET CaseID_obsolete = ARTran.CaseID, CaseCD = CRCase.CaseCD
FROM ARTran
	LEFT JOIN CRCase ON CRCase.CompanyID = ARTran.CompanyID AND CRCase.CaseID = ARTran.CaseID
GO

-- PMTran update CRCase references
--[IfExists(Table = PMTran)]
--[IfExists(Column = PMTran.CaseID)]
--[MinVersion(Hash = 8c026751bdf4c1358f0311c3563773944c00f62be12a2368554142da6364c266)]
UPDATE PMTran SET CaseID_obsolete = PMTran.CaseID, CaseCD = CRCase.CaseCD
FROM PMTran
	LEFT JOIN CRCase ON CRCase.CompanyID = PMTran.CompanyID AND CRCase.CaseID = PMTran.CaseID
GO

-- CRCaseArticle update CRCase references
--[IfExists(Table = CRCaseArticle)]
--[IfExists(Column = CRCaseArticle.CaseID)]
--[MinVersion(Hash = f8e7559a49afd2afa9ba988539ab40a5f7396f2c98176f6dd33f4e5001fa137d)]
UPDATE CRCaseArticle SET CaseID_obsolete = CRCaseArticle.CaseID, CaseCD = CRCase.CaseCD
FROM CRCaseArticle
	LEFT JOIN CRCase ON CRCase.CompanyID = CRCaseArticle.CompanyID AND CRCase.CaseID = CRCaseArticle.CaseID
GO

-- CRCaseContacts update CRCase references
--[IfExists(Table = CRCaseContacts)]
--[IfExists(Column = CRCaseContacts.CaseID)]
--[MinVersion(Hash = 5adaf803f3c9ed891ddc4b6e03f8def9697986d1150dd76eacbcee498d09f3e3)]
UPDATE CRCaseContacts SET CaseID_obsolete = CRCaseContacts.CaseID, CaseCD = CRCase.CaseCD
FROM CRCaseContacts
	LEFT JOIN CRCase ON CRCase.CompanyID = CRCaseContacts.CompanyID AND CRCase.CaseID = CRCaseContacts.CaseID
GO

-- CRCaseReference update CRCase references
-- CRCaseContacts update CRCase references
--[IfExists(Table = CRCaseReference)]
--[IfExists(Column = CRCaseReference.ParentCaseID)]
--[IfExists(Column = CRCaseReference.ChildCaseID)]
--[MinVersion(Hash = f354ed0fc76ceb01c64b45cb18537211fb98c0f3683845eb3b380a7c6cfdd888)]
UPDATE CRCaseReference SET ParentCaseID_obsolete = CRCaseReference.ParentCaseID, ParentCaseCD = CRCase.CaseCD
FROM CRCaseReference
	LEFT JOIN CRCase ON CRCase.CompanyID = CRCaseReference.CompanyID AND CRCase.CaseID = CRCaseReference.ParentCaseID

UPDATE CRCaseReference SET ChildCaseID_obsolete = CRCaseReference.ChildCaseID, ChildCaseCD = CRCase.CaseCD
FROM CRCaseReference
	LEFT JOIN CRCase ON CRCase.CompanyID = CRCaseReference.CompanyID AND CRCase.CaseID = CRCaseReference.ChildCaseID
GO

-- FSxCRCase update CRCase references
--[IfExists(Table = FSxCRCase)]
--[IfExists(Column = FSxCRCase.CaseID)]
--[MinVersion(Hash = 94450ebe5300d66003f73fcf4ca30df17c51e650de8c99fd18a9b3b270bfdd10)]
UPDATE FSxCRCase SET CaseID_obsolete = FSxCRCase.CaseID, CaseCD = CRCase.CaseCD
FROM FSxCRCase
	LEFT JOIN CRCase ON CRCase.CompanyID = FSxCRCase.CompanyID AND CRCase.CaseID = FSxCRCase.CaseID
GO

-- CRCase update keys
--[IfExists(Column = CRCase.CaseID)]
--[MinVersion(Hash = eb6a863a54e485af321b3cb8ade33477e7746d37f00d40b19508c6c7f768772d)]
UPDATE CRCase SET CaseID_obsolete = CaseID
GO

--[mysql: Skip]
--[MinVersion(Hash = fe2aebd21271640281d8f36582968c6f82ed99ab5e98bcb6a11dee9494c8060c)]
CREATE TABLE #CaseNames
(
	 RecordID int NOT NULL IDENTITY (1, 1),
	 TableName nvarchar(64) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,		
  	 OldField nvarchar(64) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
   	 NewField nvarchar(64) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	 Primary Key (RecordID)
)
INSERT INTO #CaseNames(TableName, OldField, NewField)
VALUES
('PX.Objects.AR.CRCase', 'CaseID', 'CaseCD'),
('PX.Objects.AR.CRCaseReference', 'ParentCaseID', 'ParentCaseCD'),
('PX.Objects.AR.CRCaseReference', 'ChildCaseID', 'ChildCaseCD'),
('PX.Objects.CR.CRCaseArticle', 'CaseID', 'CaseCD'),
('PX.Objects.AR.CRCaseContacts', 'CaseID', 'CaseCD'),
('PX.Objects.AR.ARTran', 'CaseID', 'CaseCD'),
('PX.Objects.AR.PMTran', 'CaseID', 'CaseCD'),
('PX.Objects.AR.FSxCRCase', 'CaseID', 'CaseCD')

UPDATE r SET Field = LOWER(LEFT(n.NewField, 1)) + SUBSTRING(n.NewField, 2, LEN(n.NewField))
FROM GIResult r
INNER JOIN GITable t
	ON t.DesignID = r.DesignID
	AND t.CompanyID = r.CompanyID
	AND r.ObjectName = t.Alias
INNER JOIN #CaseNames n
	ON LOWER(r.Field) = LOWER(n.OldField)
	AND t.Name = n.TableName
WHERE r.CompanyID <> 1

UPDATE r SET Field = REPLACE(Field, t.Alias + '.' + n.OldField, t.Alias + '.' + n.NewField)
FROM GIResult r
INNER JOIN GITable t
	ON r.ObjectName = t.Alias
	AND r.CompanyID = t.CompanyID
	AND r.DesignID = t.DesignID
INNER JOIN #CaseNames n
	ON CHARINDEX( LOWER(n.TableName), LOWER(t.[Name]) ) > 0
	AND CHARINDEX( LOWER(t.Alias) + '.' + LOWER(n.OldField), LOWER(r.Field) ) > 0
WHERE r.CompanyID <> 1	

UPDATE fr SET DataField = REPLACE(fr.DataField, n.OldField, LOWER(LEFT(n.NewField, 1)) + SUBSTRING(n.NewField, 2, LEN(n.NewField)))
FROM FilterRow fr
INNER JOIN #CaseNames n
	ON CHARINDEX(LOWER(fr.[DataField]), LOWER(n.TableName) + '_' + n.OldField) > 0
WHERE fr.CompanyID <> 1

UPDATE np SET FieldName = n.NewField, ParameterName = t.Alias + '.' + n.NewField
FROM GINavigationParameter np
INNER JOIN GITable t
	ON t.DesignID = np.DesignID
	AND t.CompanyID = np.CompanyID
	AND CHARINDEX(LOWER(t.Alias), LOWER(np.ParameterName)) > 0
INNER JOIN #CaseNames n
	ON LOWER(np.ParameterName) = LOWER(t.Alias + '.' + n.OldField)
	AND t.Name = n.TableName
WHERE np.CompanyID <> 1

UPDATE s SET DataFieldName = t.Alias + '.' + LOWER(LEFT(n.NewField, 1)) + SUBSTRING(n.NewField, 2, LEN(n.NewField))
FROM GISort s
INNER JOIN GITable t
	ON t.DesignID = s.DesignID
	AND t.CompanyID = s.CompanyID
	AND CHARINDEX(LOWER(t.Alias), LOWER(s.DataFieldName)) > 0
INNER JOIN #CaseNames n
	ON LOWER(s.DataFieldName) = LOWER(t.Alias + '.' + n.OldField)
	AND t.Name = n.TableName
WHERE s.CompanyID <> 1

UPDATE g SET DataFieldName = t.Alias + '.' + n.NewField
FROM GIGroupBy g
INNER JOIN GITable t
	ON t.DesignID = g.DesignID
	AND t.CompanyID = g.CompanyID
	AND CHARINDEX(LOWER(t.Alias), LOWER(g.DataFieldName)) > 0
INNER JOIN #CaseNames n
	ON LOWER(g.DataFieldName) = LOWER(t.Alias + '.' + n.OldField) AND t.Name = n.TableName
WHERE g.CompanyID <> 1

UPDATE f
SET FieldName = t.Alias + '.' + LOWER(LEFT(n.NewField, 1)) + SUBSTRING(n.NewField, 2, LEN(n.NewField)),
	[Name] = n.NewField
FROM GIFilter f
INNER JOIN GITable t
	ON t.DesignID = f.DesignID
	AND t.CompanyID = f.CompanyID
	AND CHARINDEX(LOWER(t.Alias), LOWER(f.FieldName)) > 0
INNER JOIN #CaseNames n
	ON LOWER(f.FieldName) = LOWER(t.Alias + '.' + n.OldField)
	AND n.TableName = t.Name
WHERE f.CompanyID <> 1
		
UPDATE o SET ParentField = LOWER(LEFT(n.NewField, 1)) + SUBSTRING(n.NewField, 2, LEN(n.NewField))
FROM GIOn o
INNER JOIN GIRelation r
	ON r.DesignID = o.DesignID
	AND r.CompanyID = o.CompanyID
	AND r.LineNbr = o.RelationNbr
INNER JOIN GITable pt
	ON pt.DesignID = r.DesignID
	AND pt.CompanyID = r.CompanyID
	AND pt.Alias = r.ParentTable
INNER JOIN #CaseNames n
	ON LOWER(n.OldField) = LOWER(o.ParentField)
	AND n.TableName = pt.Name
WHERE o.CompanyID <> 1

UPDATE o SET ChildField = LOWER(LEFT(n.NewField, 1)) + SUBSTRING(n.NewField, 2, LEN(n.NewField))
FROM GIOn o
INNER JOIN GIRelation r
	ON r.DesignID = o.DesignID
	AND r.CompanyID = o.CompanyID
	AND r.LineNbr = o.RelationNbr
INNER JOIN GITable ct
	ON ct.DesignID = r.DesignID
	AND ct.CompanyID = r.CompanyID
	AND ct.Alias = r.ChildTable
INNER JOIN #CaseNames n
	ON LOWER(n.OldField) = LOWER(o.ChildField)
	AND n.TableName = ct.Name
WHERE o.CompanyID <> 1

UPDATE w SET DataFieldName = t.Alias + '.' + LOWER(LEFT(n.NewField, 1)) + SUBSTRING(n.NewField, 2, LEN(n.NewField))
FROM GIWhere w
INNER JOIN GITable t
	ON t.DesignID = w.DesignID
	AND t.CompanyID = w.CompanyID
	AND CHARINDEX(LOWER(t.Alias), LOWER(w.DataFieldName)) > 0
INNER JOIN #CaseNames n
	ON LOWER(w.DataFieldName) = LOWER(t.Alias + '.' + n.OldField)
	AND n.TableName = t.Name
WHERE w.CompanyID <> 1

UPDATE rc SET FieldName = REPLACE(FieldName, n.OldField, n.NewField)
FROM  [EPRuleCondition] rc
INNER JOIN #CaseNames n
	ON LOWER(rc.FieldName) = LOWER(n.OldField)
	AND rc.Entity = n.TableName
WHERE rc.CompanyID <> 1

DROP TABLE #CaseNames
GO

--[mysql: Native]
--[mssql: Skip]
--[MinVersion(Hash = c8ab2d6493f226f1ed81d89f9dc3a64eb4d1a99799e066fea8fdfe12762484f6)]
CREATE TEMPORARY TABLE CaseNames
(
	 RecordID INT NOT NULL AUTO_INCREMENT, 
	 TableName varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,		
  	 OldField varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
   	 NewField varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
     Primary Key (RecordID)
);
INSERT INTO CaseNames(TableName, OldField, NewField)
VALUES
('PX.Objects.AR.CRCase', 'CaseID', 'CaseCD'),
('PX.Objects.AR.CRCaseReference', 'ParentCaseID', 'ParentCaseCD'),
('PX.Objects.AR.CRCaseReference', 'ChildCaseID', 'ChildCaseCD'),
('PX.Objects.CR.CRCaseArticle', 'CaseID', 'CaseCD'),
('PX.Objects.AR.CRCaseContacts', 'CaseID', 'CaseCD'),
('PX.Objects.AR.ARTran', 'CaseID', 'CaseCD'),
('PX.Objects.AR.PMTran', 'CaseID', 'CaseCD'),
('PX.Objects.AR.FSxCRCase', 'CaseID', 'CaseCD');

UPDATE GIResult r
INNER JOIN GITable t
	ON t.DesignID = r.DesignID
	AND t.CompanyID = r.CompanyID
	AND r.ObjectName = t.Alias
INNER JOIN CaseNames n
	ON LOWER(r.Field) = LOWER(n.OldField)
	AND t.Name = n.TableName
SET Field = CONCAT(LOWER(LEFT(n.NewField, 1)), SUBSTRING(n.NewField, 2))
WHERE r.CompanyID <> 1;

UPDATE GIResult r
INNER JOIN GITable t
	ON r.ObjectName = t.Alias
	AND r.CompanyID = t.CompanyID
	AND r.DesignID = t.DesignID
INNER JOIN CaseNames n
	ON INSTR(LOWER(r.Field), CONCAT(LOWER(t.Alias), '.', LOWER(n.OldField))) > 0
	AND INSTR(LOWER(t.Name), LOWER(n.TableName)) > 0
SET Field = REPLACE(r.Field, CONCAT(t.Alias, '.', n.OldField), CONCAT(t.Alias, '.', n.NewField))
WHERE r.CompanyID <> 1	;

UPDATE FilterRow fr
INNER JOIN CaseNames n
	ON INSTR(CONCAT(LOWER(n.TableName), '_', n.OldField), LOWER(fr.DataField)) > 0
SET DataField = REPLACE(fr.DataField, n.OldField, CONCAT(LOWER(LEFT(n.NewField, 1)), SUBSTRING(n.NewField, 2)))
WHERE fr.CompanyID <> 1;

UPDATE GINavigationParameter np
INNER JOIN GITable t
	ON t.DesignID = np.DesignID
	AND t.CompanyID = np.CompanyID
	AND INSTR(LOWER(t.Alias), LOWER(np.ParameterName)) > 0
INNER JOIN CaseNames n
	ON LOWER(np.ParameterName) = LOWER(CONCAT(t.Alias, '.', n.OldField))
	AND t.Name = n.TableName
SET FieldName = CONCAT(t.Alias, '.', n.NewField)
WHERE np.CompanyID <> 1;

UPDATE GISort s
INNER JOIN GITable t
	ON t.DesignID = s.DesignID
	AND t.CompanyID = s.CompanyID
	AND INSTR(LOWER(t.Alias), LOWER(s.DataFieldName)) > 0
INNER JOIN CaseNames n
	ON LOWER(s.DataFieldName) = LOWER(CONCAT(t.Alias, '.', n.OldField))
	AND t.Name = n.TableName
SET DataFieldName = CONCAT(t.Alias, '.', LOWER(LEFT(n.NewField, 1)), SUBSTRING(n.NewField, 2))
WHERE s.CompanyID <> 1;

UPDATE GIGroupBy g
INNER JOIN GITable t
	ON t.DesignID = g.DesignID
	AND t.CompanyID = g.CompanyID
	AND INSTR(LOWER(t.Alias), LOWER(g.DataFieldName)) > 0
INNER JOIN CaseNames n
	ON LOWER(g.DataFieldName) = LOWER(CONCAT(t.Alias, '.', n.OldField))
SET DataFieldName = CONCAT(t.Alias, '.', n.NewField)
WHERE g.CompanyID <> 1;

UPDATE GIFilter f
INNER JOIN GITable t
	ON t.DesignID = f.DesignID
	AND t.CompanyID = f.CompanyID
	AND INSTR(LOWER(t.Alias), LOWER(f.FieldName)) > 0
INNER JOIN CaseNames n
	ON LOWER(f.FieldName) = LOWER(CONCAT(t.Alias, '.', n.OldField))
	AND n.TableName = t.Name
SET FieldName = CONCAT(t.Alias, '.', LOWER(LEFT(n.NewField, 1)), SUBSTRING(n.NewField, 2)),	f.`Name` = n.NewField
WHERE f.CompanyID <> 1;
		
UPDATE GIOn o
INNER JOIN GIRelation r
	ON r.DesignID = o.DesignID
	AND r.CompanyID = o.CompanyID
	AND r.LineNbr = o.RelationNbr
INNER JOIN GITable pt
	ON pt.DesignID = r.DesignID
	AND pt.CompanyID = r.CompanyID
	AND pt.Alias = r.ParentTable
INNER JOIN CaseNames n
	ON LOWER(n.OldField) = LOWER(o.ParentField)
	AND n.TableName = pt.Name
SET ParentField = CONCAT(LOWER(LEFT(n.NewField, 1)), SUBSTRING(n.NewField, 2))
WHERE o.CompanyID <> 1;

UPDATE GIOn o
INNER JOIN GIRelation r
	ON r.DesignID = o.DesignID
	AND r.CompanyID = o.CompanyID
	AND r.LineNbr = o.RelationNbr
INNER JOIN GITable ct
	ON ct.DesignID = r.DesignID
	AND ct.CompanyID = r.CompanyID
	AND ct.Alias = r.ChildTable
INNER JOIN CaseNames n
	ON LOWER(n.OldField) = LOWER(o.ChildField)
	AND n.TableName = ct.Name
SET ChildField = CONCAT(LOWER(LEFT(n.NewField, 1)), SUBSTRING(n.NewField, 2))
WHERE o.CompanyID <> 1;

UPDATE GIWhere w
INNER JOIN GITable t
	ON t.DesignID = w.DesignID
	AND t.CompanyID = w.CompanyID
	AND INSTR(LOWER(t.Alias), LOWER(w.DataFieldName)) > 0
INNER JOIN CaseNames n
	ON LOWER(w.DataFieldName) = LOWER(CONCAT(t.Alias, '.', n.OldField))
	AND n.TableName = t.Name
SET DataFieldName = CONCAT(t.Alias, '.', LOWER(LEFT(n.NewField, 1)), SUBSTRING(n.NewField, 2))
WHERE w.CompanyID <> 1;

UPDATE EPRuleCondition rc
INNER JOIN CaseNames n
	ON LOWER(rc.FieldName) = LOWER(n.OldField)
	AND rc.Entity = n.TableName
SET FieldName = REPLACE(FieldName, n.OldField, n.NewField)
WHERE rc.CompanyID <> 1;

DROP TABLE CaseNames;
GO

--[mysql: Skip]
--[mssql: Native]
--[MinVersion(Hash = 74afc25d7b1174f8009d7f489c61108a439ddd2b8d15a687212a1c97b1e8d882)]
UPDATE c 
SET 
	TimeSpent = ISNULL(ta.TimeSpent, 0),
	OvertimeSpent = ISNULL(ta.OvertimeSpent, 0)
FROM CRCase c
INNER JOIN 
(
	SELECT a.CompanyID, a.RefNoteID, SUM(TimeSpent) as TimeSpent, SUM(OvertimeSpent) as OvertimeSpent
	FROM PMTimeActivity ta
	INNER JOIN CRActivity a
		ON a.CompanyID = ta.CompanyID
		AND a.NoteID = ta.RefNoteID
	GROUP BY a.CompanyID, a.RefNoteID
) ta
	ON ta.CompanyID = c.CompanyID
	AND ta.RefNoteID = c.NoteID
GO

--[mysql: Native]
--[mssql: Skip]
--[MinVersion(Hash = 723480fea97e1f2ac4df45bc5c9797a47e47cb9dde23648ac20531b07d7a72b8)]
UPDATE CRCase c
INNER JOIN 
(
	SELECT a.CompanyID, a.RefNoteID, SUM(ta.TimeSpent) TimeSpentSum, SUM(ta.OvertimeSpent) OvertimeSpentSum
	FROM PMTimeActivity ta
	INNER JOIN CRActivity a
		ON a.CompanyID = ta.CompanyID
		AND a.NoteID = ta.RefNoteID
	GROUP BY a.CompanyID, a.RefNoteID
) tact
	ON tact.CompanyID = c.CompanyID
	AND tact.RefNoteID = c.NoteID
SET 
	TimeSpent = IFNULL(tact.TimeSpentSum, 0),
	OvertimeSpent = IFNULL(tact.OvertimeSpentSum, 0)
GO

--[MinVersion(Hash = 97defc59897a9f45a98c380e0662b629e165b09a169b66b23fd2a9376e51c0de)]
UPDATE EMailSyncAccount SET NoteID = NEWID() WHERE NoteID IS NULL
GO

--[IfExists(Column = CRActivityRelation.NoteID)]
--[MinVersion(Hash = 89a8f993c3aa730ac8603bf70d9349d795768121fa5503e8b142f956c32980d6)]
UPDATE
	CRActivityRelation
SET 
	ParentNoteID = NoteID
WHERE
	ParentNoteID IS NULL
GO

--[IfExists(Column = SMPersonalData.NoteID)]
--[MinVersion(Hash = a388787beab47f9d3399cb0bb9b6b456d763afa112b65e6db4000ee187f24425)]
UPDATE
	SMPersonalData
SET 
	EntityID = NoteID
WHERE
	EntityID IS NULL
GO

--[IfExists(Column = SMEmail.IsArchived)]
--[MinVersion(Hash = 2853272730fc6184cc4b09cb7edf31f6cecc030ab18bd4de53daf16816a4e433)]
UPDATE SMEmail
SET isArchived = CASE
					WHEN MPStatus = 'AR' THEN 1
					ELSE 0
				END,
	MPStatus = CASE
					WHEN MPStatus = 'AR' THEN 'PD'
					ELSE MPStatus
				END
WHERE MPStatus = 'AR'
GO

--[mysql: Skip]
--[mssql: Native]
--[IfExists(Column = SMEmail.IsArchived)]
--[MinVersion(Hash = 27972ae650acd4f120d7a01d7af3423a63c8af779dd57f1944efbab9504f1481)]
CREATE TABLE #OppNames
(
	 RecordID int NOT NULL IDENTITY (1, 1),
	 TableName nvarchar(64) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,		
  	 OldField nvarchar(64) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
   	 NewField nvarchar(64) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	 Primary Key (RecordID)
)

INSERT INTO #OppNames
(
	TableName,
	OldField,
	NewField
)
VALUES
('PX.Objects.CR.SMEmail', 'MPStatus', 'IsArchived'),
('PX.Objects.CR.CRSMEmail', 'MPStatus', 'IsArchived')

UPDATE fr
SET 
	DataField = REPLACE(fr.DataField, n.OldField, LOWER(LEFT(n.NewField, 1)) + SUBSTRING(n.NewField, 2, LEN(n.NewField))),
	ValueSt = '1'
FROM 
	FilterRow fr
INNER JOIN #OppNames n 
	ON CHARINDEX(LOWER(fr.DataField), LOWER(n.TableName) + '_' + n.OldField) > 0
where
	fr.CompanyID <> 1 AND fr.ValueSt = 'AR'	

UPDATE 
	w
	SET 
	DataFieldName = t.Alias + '.' + LOWER(LEFT(n.NewField, 1)) + SUBSTRING(n.NewField, 2, LEN(n.NewField)),
	Value1 = 'True'
	FROM 
	GIWhere w
INNER JOIN GITable t
	ON t.DesignID = w.DesignID
	AND t.CompanyID = w.CompanyID
	AND CHARINDEX(LOWER(t.Alias), LOWER(w.DataFieldName)) > 0
INNER JOIN #OppNames n
	ON LOWER(w.DataFieldName) = LOWER(t.Alias + '.' + n.OldField)
	AND n.TableName = t.Name
	WHERE 
	w.CompanyID <> 1 AND w.Value1 = 'AR';
GO

--[mysql: Native]
--[mssql: Skip]
--[IfExists(Column = SMEmail.IsArchived)]
--[MinVersion(Hash = 4fbe3b6b25d35358b5741a948fd5d2ba799b2dedd67d5dc05051cf7e203ac1c9)]
CREATE TEMPORARY TABLE OppNames
(
	 RecordID INT NOT NULL AUTO_INCREMENT, 
	 TableName varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,		
  	 OldField varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
   	 NewField varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
     Primary Key (RecordID)
);
	
INSERT INTO OppNames
(
	TableName,
	OldField,
	NewField
)
VALUES
('PX.Objects.CR.SMEmail', 'MPStatus', 'IsArchived'),
('PX.Objects.CR.CRSMEmail', 'MPStatus', 'IsArchived');

UPDATE FilterRow fr
INNER JOIN OppNames n 
	ON INSTR(LOWER(fr.DataField), LOWER(n.TableName) + '_' + n.OldField) > 0
SET 
	fr.DataField = REPLACE(fr.DataField, n.OldField, CONCAT(LOWER(LEFT(n.NewField, 1)), SUBSTRING(n.NewField, 2))),
	ValueSt = '1'
where
	fr.CompanyID <> 1 AND fr.ValueSt = 'AR';

UPDATE GIWhere w
INNER JOIN GITable t
	ON t.DesignID = w.DesignID
	AND t.CompanyID = w.CompanyID
	AND INSTR(LOWER(t.Alias), LOWER(w.DataFieldName)) > 0
INNER JOIN OppNames n
	ON LOWER(w.DataFieldName) = LOWER(CONCAT(t.Alias, '.', n.OldField))
	AND n.TableName = t.Name
SET 
	DataFieldName = CONCAT(t.Alias, '.', LOWER(LEFT(n.NewField, 1)), SUBSTRING(n.NewField, 2)),
	Value1 = 'True'
WHERE 
	w.CompanyID <> 1 AND w.Value1 = 'AR';
GO

--[mysql: Native]
--[mssql: Native]
--[MinVersion(Hash = 54eacdb0fd454d132f125544f2bee719392b628060c7f0ab10d67c5aba8a45a4)]
UPDATE SYProviderField 
SET DataType='System.Int64' 
WHERE ProviderID='E5D9B3B5-E9D2-41B9-B331-8FA38D0FD016'
	AND Name like '%companyid'
	AND DataType='System.Int32'
	AND CompanyID <> 1
GO

--[MinVersion(Hash = 1087ca7d44ea9af37d551e422fe718bedd110d171ab871900fed1c3e2e736606)]
UPDATE SMEmail SET IsArchived = 0 WHERE IsArchived IS NULL
GO

--[MinVersion(Hash = bdcd46b000f32375ae281e1345241e6151cf86b485827732161ea8f601c23733)]
UPDATE
	Contact
SET
	WorkgroupID = NULL
WHERE
	WorkgroupID NOT IN (SELECT WorkgroupID FROM EPCompanyTree)
GO

--[mysql: Native]
--[mssql: Skip]
--[MinVersion(Hash = 243ecd8a919ca52ccd00c13ff624765280cc42c6daea7d8fdde124259a841925)]
ALTER TABLE Address
ADD
	OldAddressID INTEGER null
;	
CREATE INDEX Address_OldAddressID ON Address(CompanyID, OldAddressID)
GO

--[mysql: Skip]
--[mssql: Native]
--[MinVersion(Hash = 0921fe9cf3ab20507f95e72eb568f5fc8d1004879ba7a68aa40e13c3324f86a2)]
ALTER TABLE [Address]
ADD
	[OldAddressID] int null
	
CREATE NONCLUSTERED INDEX Address_OldAddressID ON Address(CompanyID, OldAddressID)
GO

--[MinVersion(Hash = b21a2fec4c33ed1079315084bf92404a78e2c82cabc6b570aa6ddaef1a399479)]
--[IfExists(Column = Address.OldAddressID)]
INSERT INTO Address
(
	[CompanyID], [OldAddressID], [BAccountID], [AddressType], [AddressLine1], [AddressLine2], [AddressLine3], [City], [State], [CountryID], [PostalCode], [IsValidated], [TaxLocationCode], [TaxMunicipalCode], [TaxSchoolCode],
	[CreatedByID], [CreatedByScreenID], [CreatedDateTime], [LastModifiedByID], [LastModifiedByScreenID], [LastModifiedDateTime], [RevisionID], [PseudonymizationStatus], [NoteID]
) 
SELECT
	[A].[CompanyID], [AddressID], [A].[BAccountID], [AddressType], [AddressLine1], [AddressLine2], [AddressLine3], [City], [State], [CountryID], [PostalCode], [IsValidated], [TaxLocationCode], [TaxMunicipalCode], [TaxSchoolCode],
	[A].[CreatedByID], [A].[CreatedByScreenID], [A].[CreatedDateTime], [A].[LastModifiedByID], [A].[LastModifiedByScreenID], [A].[LastModifiedDateTime], [A].[RevisionID], [A].[PseudonymizationStatus], NEWID()
FROM
	[Address] as [A]
INNER JOIN [Contact] as [C]
	ON [C].[CompanyID] = [A].[CompanyID]
	AND [C].[DefAddressID] = [A].[AddressID]
WHERE
	[C].[CompanyID] > 1
	AND [C].[ContactType] = 'PN'
	AND [C].[Status] = 'C'
GO

--[mysql: Native]
--[mssql: Skip]
--[MinVersion(Hash = 3a76eb5a7b801e9a9e9f486e47f6c30c2ff155d0378e079b3a0fc4881df9dd1f)]
ALTER TABLE Contact
ADD COLUMN RefContactID INTEGER null,
ADD COLUMN OldNoteID char(38) null
GO

--[mysql: Skip]
--[mssql: Native]
--[MinVersion(Hash = 2c5a5c708c654608bacf8f1840eec25ec8c20bf3e5d75214c0abac9027df26b1)]
ALTER TABLE [Contact]
ADD
	[RefContactID] int null,
	[OldNoteID] uniqueidentifier null
GO

--[IfExists(Column = Contact.RefContactID)]
--[MinVersion(Hash = 04ee8acd1bf11e8496a74bb7b2c2e8205c05b7cad8c9558c042ef2df90be65ff)]
INSERT INTO [Contact]
(
	[CompanyID], [BAccountID], [ContactType], [RefContactID], [OldNoteID], [FullName], [DisplayName], [Title], [Salutation], [Attention], [FirstName], [MidName], [LastName], [EMail], [WebSite],
	[Fax], [FaxType], [Phone1], [Phone1Type], [Phone2], [Phone2Type], [Phone3], [Phone3Type], [DateOfBirth], 
	[DefAddressID], 
	[IsActive], [WorkgroupID], [OwnerID], [UserID], [Method], [LanguageID],
	[CreatedByID], [CreatedByScreenID], [CreatedDateTime], [LastModifiedByID], [LastModifiedByScreenID], [LastModifiedDateTime], [RevisionID], [DeletedDatabaseRecord], [NoFax], [NoMail], [NoMarketing], [NoCall], [NoEMail], [NoMassMail],
	[Gender], [MaritalStatus], [Anniversary], [Spouse], [Img], [Source], [MajorStatus], [Status], [Resolution], [DuplicateStatus], [CampaignID], [AssignDate], [ClassID], [ParentBAccountID],[IsConvertable],
	[GrammValidationDateTime], [ExtRefNbr], [Synchronize], [PseudonymizationStatus], [ConsentAgreement], [ConsentDate], [ConsentExpirationDate], [NoteID]
)
	SELECT
		[Contact].[CompanyID], [Contact].[BAccountID], 'LD', [ContactID], [Contact].[NoteID], [FullName], [DisplayName], [Title], [Salutation], [Attention], [FirstName], [MidName], [LastName], [EMail], [WebSite],
		[Fax], [FaxType], [Phone1], [Phone1Type], [Phone2], [Phone2Type], [Phone3], [Phone3Type], [DateOfBirth],
		[Address].[AddressID],
		[IsActive], [WorkgroupID], [OwnerID], [UserID], [Method], [LanguageID],
		[Contact].[CreatedByID], [Contact].[CreatedByScreenID], [Contact].[CreatedDateTime], [Contact].[LastModifiedByID], [Contact].[LastModifiedByScreenID], [Contact].[LastModifiedDateTime], [Contact].[RevisionID], [DeletedDatabaseRecord], [NoFax], [NoMail], [NoMarketing], [NoCall], [NoEMail], [NoMassMail],
		[Gender], [MaritalStatus], [Anniversary], [Spouse], [Img], [Source], [MajorStatus], [Status], [Resolution], [DuplicateStatus], [CampaignID], [AssignDate], [ClassID], [ParentBAccountID],[IsConvertable],
		[GrammValidationDateTime], [ExtRefNbr], [Synchronize], [Contact].[PseudonymizationStatus], [ConsentAgreement], [ConsentDate], [ConsentExpirationDate], NEWID()
	FROM
		[Contact]
	INNER JOIN [Address]
		ON [Address].[CompanyID] = [Contact].[CompanyID]
		AND [Address].[OldAddressID] = [Contact].[DefAddressID]
	WHERE
		[Contact].[CompanyID] > 1
		AND [ContactType] = 'PN'
		AND [Status] = 'C'
GO

--[mysql: Skip]
--[mssql: Native]
--[IfExists(Column = Contact.QualificationDate)]
--[MinVersion(Hash = 87e2394eb4f55fc73488ce3759837861415fa577fe0aafdbf010759ae7157dc1)]
INSERT INTO [CRLead]
(
	[CompanyID], [ContactID], [Status], [Resolution], [ClassID], [RefContactID], [OverrideRefContact], [Description], [QualificationDate], [ConvertedBy], [NoteID], [DeletedDatabaseRecord],
	[CreatedByID], [CreatedByScreenID], [CreatedDateTime], [LastModifiedByID], [LastModifiedByScreenID], [LastModifiedDateTime]
)
	SELECT
		[CompanyID], [ContactID], [Status], [Resolution], [ClassID], [RefContactID], IIF([RefContactID] IS NOT NULL OR [BAccountID] IS NOT NULL, 1, 0), NULL, [QualificationDate], [ConvertedBy], [NoteID], [DeletedDatabaseRecord],
		[CreatedByID], [CreatedByScreenID], [CreatedDateTime], [LastModifiedByID], [LastModifiedByScreenID], [LastModifiedDateTime]
	FROM 
		[Contact]
	WHERE 
		[CompanyID] > 1
		AND [ContactType] = 'LD'
GO

--[mysql: Native]
--[mssql: Skip]
--[IfExists(Column = Contact.QualificationDate)]
--[MinVersion(Hash = a1583feab72db041729a4c40a63aed1bae734e4e505a0a78b52bcf3824d9bfe1)]
INSERT INTO CRLead
(
	CompanyID, ContactID, Status, Resolution, ClassID, RefContactID, OverrideRefContact, Description, QualificationDate, ConvertedBy, NoteID, DeletedDatabaseRecord,
	CreatedByID, CreatedByScreenID, CreatedDateTime, LastModifiedByID, LastModifiedByScreenID, LastModifiedDateTime
)
	SELECT
		CompanyID, ContactID, Status, Resolution, ClassID, RefContactID, IF(RefContactID IS NOT NULL OR BAccountID IS NOT NULL, 1, 0), NULL, QualificationDate, ConvertedBy, NoteID, DeletedDatabaseRecord,
		CreatedByID, CreatedByScreenID, CreatedDateTime, LastModifiedByID, LastModifiedByScreenID, LastModifiedDateTime
	FROM 
		Contact
	WHERE 
		CompanyID > 1
		AND ContactType = 'LD'
GO

--[MinVersion(Hash = 55bde8727149d760abd011a3158a0e1253156143dbcf859b7038f7fd87376462)]
UPDATE
	[Contact]
SET
	[Title] = NULL
WHERE
	[CompanyID] > 0
	AND [ContactType] IN ('PN', 'LD')
	AND [Title] IS NOT NULL
GO

--[mysql: Skip]
--[mssql: Native]
--[MinVersion(Hash = 4bac4b5a8a3f3c1e9abe725b04a6f7ae856cbdabcb1e590e22c611a89caac874)]
UPDATE C
SET C.[DisplayName] = 
	CASE
		WHEN C.Title IS     NULL AND C.FirstName IS     NULL and C.MidName IS     NULL THEN C.LastName
		WHEN C.Title IS     NULL AND C.FirstName IS     NULL and C.MidName IS NOT NULL THEN CONCAT(C.LastName, ', ', C.MidName)
		WHEN C.Title IS     NULL AND C.FirstName IS NOT NULL and C.MidName IS     NULL THEN CONCAT(C.LastName, ', ', C.FirstName)
		WHEN C.Title IS     NULL AND C.FirstName IS NOT NULL and C.MidName IS NOT NULL THEN CONCAT(C.LastName, ', ', C.FirstName, ' ',  C.MidName)
		WHEN C.Title IS NOT NULL AND C.FirstName IS     NULL and C.MidName IS     NULL THEN CONCAT(C.LastName, ', ', C.Title)
		WHEN C.Title IS NOT NULL AND C.FirstName IS     NULL and C.MidName IS NOT NULL THEN CONCAT(C.LastName, ' ',  C.MidName,   ', ', C.Title)
		WHEN C.Title IS NOT NULL AND C.FirstName IS NOT NULL and C.MidName IS     NULL THEN CONCAT(C.LastName, ' ',  C.FirstName, ', ', C.Title)
		WHEN C.Title IS NOT NULL AND C.FirstName IS NOT NULL and C.MidName IS NOT NULL THEN CONCAT(C.LastName, ' ',  C.FirstName, ' ',  C.MidName, ', ', C.Title)
		ELSE C.DisplayName
	END
FROM [Contact] C
  INNER JOIN [PreferencesGeneral] P ON P.CompanyID = C.CompanyID
WHERE P.PersonNameFormat = 'LEGACY' AND C.LastName IS NOT NULL AND C.ContactType IN ('PN', 'LD')

UPDATE C
SET C.[DisplayName] = CONCAT(C.LastName, IIF(C.FirstName IS NULL, '', CONCAT(', ', C.FirstName)))
FROM [Contact] C
  INNER JOIN [PreferencesGeneral] P ON P.CompanyID = C.CompanyID
WHERE P.PersonNameFormat = 'EASTERN' AND C.LastName IS NOT NULL AND C.ContactType IN ('PN', 'LD')

UPDATE C
SET C.[DisplayName] = CONCAT(IIF(C.FirstName IS NULL, '', CONCAT(C.FirstName, ' ')), C.LastName)
FROM [Contact] C
  INNER JOIN [PreferencesGeneral] P ON P.CompanyID = C.CompanyID
WHERE P.PersonNameFormat = 'WESTERN' AND C.LastName IS NOT NULL AND C.ContactType IN ('PN', 'LD')

UPDATE C
SET C.[DisplayName] = 
	CASE
		WHEN C.Title IS     NULL AND C.FirstName IS     NULL and C.MidName IS     NULL THEN C.LastName
		WHEN C.Title IS     NULL AND C.FirstName IS     NULL and C.MidName IS NOT NULL THEN CONCAT(C.LastName, ', ', C.MidName)
		WHEN C.Title IS     NULL AND C.FirstName IS NOT NULL and C.MidName IS     NULL THEN CONCAT(C.LastName, ', ', C.FirstName)
		WHEN C.Title IS     NULL AND C.FirstName IS NOT NULL and C.MidName IS NOT NULL THEN CONCAT(C.LastName, ', ', C.FirstName, ' ', C.MidName)
		WHEN C.Title IS NOT NULL AND C.FirstName IS     NULL and C.MidName IS     NULL THEN CONCAT(C.Title, ' ', C.LastName)
		WHEN C.Title IS NOT NULL AND C.FirstName IS     NULL and C.MidName IS NOT NULL THEN CONCAT(C.Title, ' ', C.LastName, ', ', C.MidName)
		WHEN C.Title IS NOT NULL AND C.FirstName IS NOT NULL and C.MidName IS     NULL THEN CONCAT(C.Title, ' ', C.LastName, ', ', C.FirstName)
		WHEN C.Title IS NOT NULL AND C.FirstName IS NOT NULL and C.MidName IS NOT NULL THEN CONCAT(C.Title, ' ', C.LastName, ' ',  C.FirstName, ' ', C.MidName)
		ELSE C.DisplayName
	END
FROM [Contact] C
  INNER JOIN [PreferencesGeneral] P ON P.CompanyID = C.CompanyID
WHERE P.PersonNameFormat = 'EASTERN_WITH_TITLE' AND C.LastName IS NOT NULL AND C.ContactType IN ('PN', 'LD')
GO

--[mysql: Native]
--[mssql: Skip]
--[MinVersion(Hash = f8ded1000e6dede2dc49e6681a6f2395f079a7b8cdfe290bc958f5a4aff93283)]
UPDATE Contact C
  INNER JOIN PreferencesGeneral P ON P.CompanyID = C.CompanyID
SET C.DisplayName = 
	CASE
		WHEN C.Title IS     NULL AND C.FirstName IS     NULL and C.MidName IS     NULL THEN C.LastName
		WHEN C.Title IS     NULL AND C.FirstName IS     NULL and C.MidName IS NOT NULL THEN CONCAT(C.LastName, ', ', C.MidName)
		WHEN C.Title IS     NULL AND C.FirstName IS NOT NULL and C.MidName IS     NULL THEN CONCAT(C.LastName, ', ', C.FirstName)
		WHEN C.Title IS     NULL AND C.FirstName IS NOT NULL and C.MidName IS NOT NULL THEN CONCAT(C.LastName, ', ', C.FirstName, ' ',  C.MidName)
		WHEN C.Title IS NOT NULL AND C.FirstName IS     NULL and C.MidName IS     NULL THEN CONCAT(C.LastName, ', ', C.Title)
		WHEN C.Title IS NOT NULL AND C.FirstName IS     NULL and C.MidName IS NOT NULL THEN CONCAT(C.LastName, ' ',  C.MidName,   ', ', C.Title)
		WHEN C.Title IS NOT NULL AND C.FirstName IS NOT NULL and C.MidName IS     NULL THEN CONCAT(C.LastName, ' ',  C.FirstName, ', ', C.Title)
		WHEN C.Title IS NOT NULL AND C.FirstName IS NOT NULL and C.MidName IS NOT NULL THEN CONCAT(C.LastName, ' ',  C.FirstName, ' ',  C.MidName, ', ', C.Title)
		ELSE C.DisplayName
	END
WHERE P.PersonNameFormat = 'LEGACY' AND C.LastName IS NOT NULL AND C.ContactType IN ('PN', 'LD')
;
UPDATE Contact C
  INNER JOIN PreferencesGeneral P ON P.CompanyID = C.CompanyID
SET C.DisplayName = CONCAT(C.LastName, IF(C.FirstName IS NULL, '', CONCAT(', ', C.FirstName)))
WHERE P.PersonNameFormat = 'EASTERN' AND C.LastName IS NOT NULL AND C.ContactType IN ('PN', 'LD')
;
UPDATE Contact C
  INNER JOIN PreferencesGeneral P ON P.CompanyID = C.CompanyID
SET C.DisplayName = CONCAT(IF(C.FirstName IS NULL, '', CONCAT(C.FirstName, ' ')), C.LastName)
WHERE P.PersonNameFormat = 'WESTERN' AND C.LastName IS NOT NULL AND C.ContactType IN ('PN', 'LD')
;
UPDATE Contact C
  INNER JOIN PreferencesGeneral P ON P.CompanyID = C.CompanyID
SET C.DisplayName = 
	CASE
		WHEN C.Title IS     NULL AND C.FirstName IS     NULL and C.MidName IS     NULL THEN C.LastName
		WHEN C.Title IS     NULL AND C.FirstName IS     NULL and C.MidName IS NOT NULL THEN CONCAT(C.LastName, ', ', C.MidName)
		WHEN C.Title IS     NULL AND C.FirstName IS NOT NULL and C.MidName IS     NULL THEN CONCAT(C.LastName, ', ', C.FirstName)
		WHEN C.Title IS     NULL AND C.FirstName IS NOT NULL and C.MidName IS NOT NULL THEN CONCAT(C.LastName, ', ', C.FirstName, ' ', C.MidName)
		WHEN C.Title IS NOT NULL AND C.FirstName IS     NULL and C.MidName IS     NULL THEN CONCAT(C.Title, ' ', C.LastName)
		WHEN C.Title IS NOT NULL AND C.FirstName IS     NULL and C.MidName IS NOT NULL THEN CONCAT(C.Title, ' ', C.LastName, ', ', C.MidName)
		WHEN C.Title IS NOT NULL AND C.FirstName IS NOT NULL and C.MidName IS     NULL THEN CONCAT(C.Title, ' ', C.LastName, ', ', C.FirstName)
		WHEN C.Title IS NOT NULL AND C.FirstName IS NOT NULL and C.MidName IS NOT NULL THEN CONCAT(C.Title, ' ', C.LastName, ' ',  C.FirstName, ' ', C.MidName)
		ELSE C.DisplayName
	END
WHERE P.PersonNameFormat = 'EASTERN_WITH_TITLE' AND C.LastName IS NOT NULL AND C.ContactType IN ('PN', 'LD')
GO

--[MinVersion(Hash = 06523a9b2a401234aaec85b1e763f0c9b26192ad03948160b91bfed90d4ec271)]
UPDATE
	[Contact]
SET
	[Status] = NULL
WHERE
	[CompanyID] > 1
	AND [ContactType] = 'PN'
	AND [Status] = 'C'
	
UPDATE
	[Contact]
SET
	[ClassID] = NULL,
	[Status] = NULL,
	[Resolution] = NULL
WHERE
	[CompanyID] > 1
	AND [ContactType] = 'LD'
GO

--[IfExists(Column = Contact.RefContactID)]
--[MinVersion(Hash = 8f656dc1461518d27d9cc6be88f22a91ba4029c603aead4e78eb5b2967dcd39a)]
INSERT INTO [CRRelation]
(
	[CompanyID], [role], [IsPrimary], [TargetType], [TargetNoteID], [DocNoteID], [EntityID], [ContactID], [AddToCC], [RefNoteID]
)
SELECT
	[CRRelation].[CompanyID], [role], [IsPrimary], [TargetType], [TargetNoteID], [DocNoteID], [EntityID], [CRRelation].[ContactID], [AddToCC], [Contact].[NoteID]
FROM
	[CRRelation]
INNER JOIN [Contact]
	ON [OldNoteID] = [RefNoteID]
GO

--[IfExists(Column = Contact.RefContactID)]
--[MinVersion(Hash = 18ab7caf5e64c1475c8f9fbc4e26d6e629904fc9d71f0c2b12469d9651f4bfe3)]
INSERT INTO [CRRelation]
(
	[CompanyID], [Role], [IsPrimary], [TargetType], [TargetNoteID], [DocNoteID], [EntityID], [ContactID], [AddToCC],
	[RefNoteID]
)
	SELECT
		[CompanyID], 'SR', 0, 'PX.Objects.CR.CRLead', [NoteID], [NoteID], NULL, [ContactID], 0,
		(
			SELECT
				[NoteID]
			FROM
				[Contact]
			WHERE
				[CompanyID] = [C].[CompanyID]
				AND [ContactID] = [C].[RefContactID]
		)
	FROM
		[Contact] [C]
	WHERE
		[CompanyID] > 1
		AND [ContactType] = 'LD'
		AND [RefContactID] IS NOT NULL
GO

--[IfExists(Column = CROpportunity.ConvertedLeadID)]
--[MinVersion(Hash = daeedf16045c4e14a9c0c577a2b684c4856b32286b9ed638dc1ba983292e9e9b)]
UPDATE 
	[CROpportunity]
SET 
	[LeadID] = (
		SELECT
			[NoteID]
		FROM
			[Contact]
		WHERE
			[Contact].[CompanyID] = [O].[CompanyID]
			AND [Contact].[ContactID] = [O].[ConvertedLeadID]
		)
FROM 
	[CROpportunity] [O]
WHERE 
	[CompanyID] > 1
	AND [LeadID] IS NULL
	AND [ConvertedLeadID] IS NOT NULL
GO

--[MinVersion(Hash = ba4863fdc252bbfa14c0c634aeda2ab2351cfcd2de511c75c6691c869e205130)]
DROP INDEX Address_OldAddressID ON Address
GO

--[mysql: Skip]
--[mssql: Native]
--[MinVersion(Hash = 623779620c881f13faf15e6cca36b9be5f9e3f43ad2f626b3aa0adffb6f4388f)]
ALTER TABLE [Contact]
DROP COLUMN
	[RefContactID],
	[OldNoteID]

ALTER TABLE [Address]
DROP COLUMN [OldAddressID]
GO

--[mysql: Native]
--[mssql: Skip]
--[MinVersion(Hash = edd872059de669168568e388c3201159a83aabeaeea27f2973ff1b92387ec8ad)]
ALTER TABLE Contact
DROP COLUMN RefContactID,
DROP COLUMN OldNoteID
;
ALTER TABLE Address
DROP COLUMN OldAddressID
GO

--[mssql: Skip]
--[mysql: Native]
--[IfExists(Column = CRContactClass.OwnerIsCreatedUser)]
--[MinVersion(Hash = 7221fa2228794e34e6bd54524c44ebe842bfbb544bd793f8d61c7be1e0c22729)]
INSERT INTO CRLeadClass
(
	CompanyID, ClassID, IsInternal, Description, DefaultSource, DefaultOwner, TargetBAccountClassID, TargetOpportunityClassID, DefaultEMailAccountID,
	NoteID, CreatedByID, CreatedByScreenID, CreatedDateTime, LastModifiedByID, LastModifiedByScreenID, LastModifiedDateTime
)
	SELECT
		CompanyID, ClassID, IsInternal, Description, DefaultSource, IF(OwnerIsCreatedUser = 1, 'C', NULL), TargetBAccountClassID, TargetOpportunityClassID, DefaultEMailAccountID,
		UUID(), CreatedByID, CreatedByScreenID, CreatedDateTime, LastModifiedByID, LastModifiedByScreenID, LastModifiedDateTime
	FROM
		CRContactClass
	WHERE
		CompanyID > 1
GO

--[mssql: Native]
--[mysql: Skip]
--[IfExists(Column = CRContactClass.OwnerIsCreatedUser)]
--[MinVersion(Hash = bb35d73b3c881e1b3e8e16e25c0a717763ad0d39a0ab7cb29e6e1fd33d6a57e4)]
INSERT INTO [CRLeadClass]
(
	[CompanyID], [ClassID], [IsInternal], [Description], [DefaultSource], [DefaultOwner], [TargetBAccountClassID], [TargetOpportunityClassID], [DefaultEMailAccountID],
	[NoteID], [CreatedByID], [CreatedByScreenID], [CreatedDateTime], [LastModifiedByID], [LastModifiedByScreenID], [LastModifiedDateTime]
)
	SELECT
		[CompanyID], [ClassID], [IsInternal], [Description], [DefaultSource], IIF([OwnerIsCreatedUser] = 1, 'C', NULL), [TargetBAccountClassID], [TargetOpportunityClassID], [DefaultEMailAccountID],
		NEWID(), [CreatedByID], [CreatedByScreenID], [CreatedDateTime], [LastModifiedByID], [LastModifiedByScreenID], [LastModifiedDateTime]
	FROM
		CRContactClass
	WHERE
		[CompanyID] > 1
GO

--[mssql: Native]
--[mysql: Skip]
--[MinVersion(Hash = 0977606e20d3dfceab142315820baccc681b8699a39a5dd3cf51463c463d1747)]
UPDATE
	[A]
SET
	[A].[DocumentNoteID] = [C].[NoteID],
	[A].[RefNoteID] = [Cn].[NoteID]
FROM
	[CRActivity] [A]
INNER JOIN [CRCampaign] [C]
	ON [C].[CompanyID] = [A].[CompanyID]
	AND [C].[NoteID] = [A].[RefNoteID]
INNER JOIN [Contact] [Cn]
	ON [Cn].[CompanyID] = [A].[CompanyID]
	AND [Cn].[ContactID] = [A].[ContactID]
	AND [Cn].[ContactType] = 'LD'
WHERE
	[A].[CompanyID] > 1

UPDATE
	[A]
SET
	[A].[RefNoteID] = [Cn].[NoteID]
FROM
	[CRActivity] [A]
INNER JOIN [Contact] [Cn]
	ON [Cn].[CompanyID] = [A].[CompanyID]
	AND [Cn].[ContactID] = [A].[ContactID]
	AND [Cn].[ContactType] = 'LD'
WHERE
	[A].[CompanyID] > 1
GO

--[mysql: Native]
--[mssql: Skip]
--[MinVersion(Hash = a0829e6bebc64aff15c2c4250748c025fc37d4d96ab7278137fc777743924c3c)]
UPDATE
	CRActivity
INNER JOIN CRCampaign
	ON CRCampaign.CompanyID = CRActivity.CompanyID
	AND CRCampaign.NoteID = CRActivity.RefNoteID
INNER JOIN Contact
	ON Contact.CompanyID = CRActivity.CompanyID
	AND Contact.ContactID = CRActivity.ContactID
	AND Contact.ContactType = 'LD'
SET
	CRActivity.DocumentNoteID = CRCampaign.NoteID,
	CRActivity.RefNoteID = Contact.NoteID
WHERE
	CRActivity.CompanyID > 1
;
UPDATE
	CRActivity
INNER JOIN Contact
	ON Contact.CompanyID = CRActivity.CompanyID
	AND Contact.ContactID = CRActivity.ContactID
	AND Contact.ContactType = 'LD'
SET
	CRActivity.RefNoteID = Contact.NoteID
WHERE
	CRActivity.CompanyID > 1
GO

--[MinVersion(Hash = b9245ee6bb90e960742a16420e05cb11f70cc9d7f36e4c2346bd53134c9a4607)]
UPDATE
	EPAssignmentMap
SET
	EntityType = 'PX.Objects.CR.CRLead'
WHERE
	GraphType = 'PX.Objects.CR.LeadMaint'
GO

--[OldHash(Hash = dc7ad8fb8bf9b4ae7fefb7f8e3e74a01715711bd6000ad1a4f4d1b234d295502)]
--[MinVersion(Hash = 232a65c8e7c83364b9ffb0afe860f31fa0d43447a76d96d795426b51e2a510a8)]
INSERT INTO [CSAttributeGroup]
(
	[CompanyID], [AttributeID], [EntityClassID], [EntityType], [SortOrder], [CreatedByID], [CreatedByScreenID], [CreatedDateTime], [LastModifiedByID], [LastModifiedByScreenID], [LastModifiedDateTime], [DefaultValue], [Required], [IsActive], [AttributeCategory]
)
	SELECT
		[CompanyID], [AttributeID], [EntityClassID], 'PX.Objects.CR.CRLead', [SortOrder], [CreatedByID], [CreatedByScreenID], [CreatedDateTime], [LastModifiedByID], [LastModifiedByScreenID], [LastModifiedDateTime], [DefaultValue], [Required], [IsActive], [AttributeCategory]
	FROM
		CSAttributeGroup
	WHERE
		[CompanyID] > 1
		AND [EntityType] = 'PX.Objects.CR.Contact'
GO

--[mysql: Skip]
--[mssql: Native]
--[IfExists(Column = CRCaseClass.Status)]
--[MinVersion(Hash = 9076865af18a6bf73b8af1920396762706f1294979272ad249d9e3444b88192a)]
ALTER TABLE CRCaseClass
	ADD DEFAULT 'N' FOR Status
GO

--[mysql: Native]
--[mssql: Skip]
--[IfExists(Column = CRCaseClass.Status)]
--[MinVersion(Hash = a99aeb0b4a9f84d6dd305e9025aa3ae0efa0b2fe4bf151f4ad7032ad52d48bd9)]
ALTER TABLE CRCaseClass 
	ALTER COLUMN Status SET DEFAULT 'N';
GO

--[MinVersion(Hash = 7591aa4b112fc36b5d6f9d201925fb450727cd673b7b2640235329c09bded8f0)]
UPDATE AUStep
SET IsActive = 0
WHERE ScreenID IN ('CR304000', 'CR306000', 'CR301000')

DELETE
FROM AUCombo
WHERE TableName IN ('PX.Objects.CR.CRCase', 'PX.Objects.CR.CROpportunity', 'PX.Objects.CR.CRLead')
GO

--[mssql: Native]
--[mysql: Skip]
--[IfExists(Column = CRActivityStatistics.InitialOutgoingActivityCompletedAtDate)]
--[MinVersion(Hash = 4638f9e61ebf357d0c4c0629031a9d2f650c237b1f21e8a99389c3366c293245)]
UPDATE [S]
  SET 
	  [S].[InitialOutgoingActivityCompletedAtNoteID] = [A_MinDateAllCols].NoteID, 
	  [S].[InitialOutgoingActivityCompletedAtDate] = [A_MinDateAllCols].MINCompletedDate
FROM CRActivityStatistics [S]
	 INNER JOIN
(
	SELECT [A_MinDateAllCols].CompanyID CompanyID, 
		   [A_MinDateAllCols].RefNoteID RefNoteID, 
		   [A_MinDateAllCols].CompletedDate MINCompletedDate, 
		   [A_MinDateAllCols].NoteID NoteID
	FROM CRActivity [A_MinDateAllCols]
		 INNER JOIN
	(
		SELECT [A].CompanyID, 
			   [A].RefNoteID, 
			   MIN([A].CompletedDate) MINCompletedDate
		FROM CRActivity [A]
		WHERE [A].Outgoing = 1
			  AND [A].DeletedDatabaseRecord = 0
			  AND [A].UIStatus = 'CD'
			  AND [A].CompletedDate IS NOT NULL
		GROUP BY [A].CompanyID, 
				 [A].RefNoteID
	) [A_MinDate] ON [A_MinDateAllCols].CompanyID = [A_MinDate].CompanyID
					 AND [A_MinDateAllCols].RefNoteID = [A_MinDate].RefNoteID
					 AND [A_MinDateAllCols].CompletedDate = [A_MinDate].MINCompletedDate
					 AND [A_MinDateAllCols].Outgoing = 1
					 AND [A_MinDateAllCols].DeletedDatabaseRecord = 0
					 AND [A_MinDateAllCols].UIStatus = 'CD'
) [A_MinDateAllCols] ON [S].CompanyID = [A_MinDateAllCols].CompanyID
						AND [S].NoteID = [A_MinDateAllCols].RefNoteID
						AND [S].[InitialOutgoingActivityCompletedAtNoteID] IS NULL
						AND [S].[InitialOutgoingActivityCompletedAtDate] IS NULL;
GO

--[mysql: Native]
--[mssql: Skip]
--[MinVersion(Hash = d9f4b95e461491c9cd447a9adfa2553e80186eebe988f8034f2017313e8ae1ea)]
UPDATE CRActivityStatistics S
	 INNER JOIN
(
	SELECT A_MinDateAllCols.CompanyID CompanyID, 
		A_MinDateAllCols.RefNoteID RefNoteID, 
		A_MinDateAllCols.CompletedDate MINCompletedDate, 
		A_MinDateAllCols.NoteID NoteID
	FROM CRActivity A_MinDateAllCols
		 INNER JOIN
	(
		SELECT CompanyID, 
		       RefNoteID, 
		       MIN(CompletedDate) MINCompletedDate
		FROM CRActivity
		WHERE CRActivity.Outgoing = 1
			  AND CRActivity.DeletedDatabaseRecord = 0
			  AND CRActivity.UIStatus = 'CD'
			  AND CRActivity.CompletedDate IS NOT NULL
		GROUP BY CRActivity.CompanyID, 
			CRActivity.RefNoteID
	) A_MinDate ON A_MinDateAllCols.CompanyID = A_MinDate.CompanyID
					 AND A_MinDateAllCols.RefNoteID = A_MinDate.RefNoteID
					 AND A_MinDateAllCols.CompletedDate = A_MinDate.MINCompletedDate
					 AND A_MinDateAllCols.Outgoing = 1
					 AND A_MinDateAllCols.DeletedDatabaseRecord = 0
					 AND A_MinDateAllCols.UIStatus = 'CD'
) A_MinDateAllCols ON S.CompanyID = A_MinDateAllCols.CompanyID
						AND S.NoteID = A_MinDateAllCols.RefNoteID
						AND S.InitialOutgoingActivityCompletedAtNoteID IS NULL
						AND S.InitialOutgoingActivityCompletedAtDate IS NULL
  SET 
	  S.InitialOutgoingActivityCompletedAtNoteID = A_MinDateAllCols.NoteID, 
	  S.InitialOutgoingActivityCompletedAtDate = A_MinDateAllCols.MINCompletedDate
GO

--[IfExists(Column = CROpportunity.MajorStatus)]
--[MinVersion(Hash = bf2beab9d339674016cb769ebea86bac7c5703be0c728a22878ad88c31c77f5b)]
UPDATE CROpportunity
SET IsActive =
	CASE
		WHEN MajorStatus = 4 THEN 0
		ELSE 1
	END
GO

--[IfExists(Column = CRCase.MajorStatus)]
--[MinVersion(Hash = edc6b2650d2102fb061e0b0fb15486823db1b86e2897d22d0b3fca4e950f9f38)]
UPDATE CRCase
SET IsActive =
	CASE
		WHEN MajorStatus in (4, 5, 6) THEN 0
		ELSE 1
	END
GO

--[mysql: Native]
--[mssql: Skip]
--[OldHash(Hash = 787d8f787ecda081c9bcb4a988fcd8abc8ed98457ea20561c6e3de2a5e601e59)]
--[OldHash(Hash = 2100444ca9e79f32e7858859b39b8164cd1cf36f8003d749221045d2fc5508f2)]
--[MinVersion(Hash = fd1fc79f6fe10119e70404b4af80064b281335d1e9a1afc3aac959c472117c11)]
UPDATE Address addr
  INNER JOIN (
      SELECT MAX(BAccountID) MaxBAccountID, COUNT(BAccountID), DefAddressID, CompanyID 
      FROM BAccount 
      WHERE DefAddressID IS NOT NULL 
      Group BY DefAddressID, CompanyID 
      HAVING COUNT(BAccountID) > 1
    ) A ON A.CompanyID = addr.CompanyID AND A.DefAddressID = addr.AddressID
  SET addr.BAccountID = A.MaxBAccountID;

CREATE TEMPORARY TABLE OldAddressTmp
(
  CompanyID INT NOT NULL,
  AddressID INT NOT NULL AUTO_INCREMENT, 
  OldAddressID INT,
  BAccountID INT,
  PRIMARY KEY (AddressID)
);
INSERT INTO OldAddressTmp SELECT 1, MAX(AddressID)+1, NULL, NULL FROM Address;

INSERT INTO OldAddressTmp (CompanyID, OldAddressID, BAccountID)
SELECT A.CompanyID, A.AddressID, B.BAccountID
FROM BAccount B
  INNER JOIN (
    SELECT MAX(BAccountID) MaxAcc, DefAddressID, CompanyID FROM BAccount WHERE DefAddressID IS NOT NULL Group BY DefAddressID, CompanyID HAVING COUNT(BAccountID) > 1
  ) M ON B.DefAddressID = M.DefAddressID AND B.CompanyID = M.CompanyID
  LEFT JOIN Address A ON A.CompanyID = B.CompanyID AND A.AddressID = B.DefAddressID
WHERE A.BAccountID <> B.BAccountID;

INSERT INTO Address (
  AddressID,
  CompanyID, 
  BAccountID, 
  AddressType, 
  AddressLine1, 
  AddressLine2, 
  AddressLine3, 
  City, 
  State, 
  CountryID, 
  PostalCode, 
  IsValidated, 
  TaxLocationCode, 
  TaxMunicipalCode, 
  TaxSchoolCode, 
  tstamp,
  CreatedByID, 
  CreatedByScreenID, 
  CreatedDateTime, 
  LastModifiedByID, 
  LastModifiedByScreenID, 
  LastModifiedDateTime, 
  RevisionID, 
  NoteID
)
SELECT 
  tmp.AddressID,
  A.CompanyID, 
  tmp.BAccountID, 
  A.AddressType, 
  A.AddressLine1, 
  A.AddressLine2, 
  A.AddressLine3, 
  A.City, 
  A.State, 
  A.CountryID, 
  A.PostalCode, 
  A.IsValidated, 
  A.TaxLocationCode, 
  A.TaxMunicipalCode, 
  A.TaxSchoolCode, 
  A.tstamp,
  A.CreatedByID, 
  A.CreatedByScreenID, 
  A.CreatedDateTime, 
  A.LastModifiedByID, 
  A.LastModifiedByScreenID, 
  A.LastModifiedDateTime, 
  A.RevisionID, 
  UUID()
FROM OldAddressTmp tmp
  INNER JOIN Address A ON A.CompanyID = tmp.CompanyID AND A.AddressID = tmp.OldAddressID
WHERE tmp.OldAddressID IS NOT NULL;

UPDATE BAccount B
  INNER JOIN OldAddressTmp tmp ON B.CompanyID = tmp.CompanyID AND B.BAccountID = tmp.BAccountID
  SET B.DefAddressID = tmp.AddressID
WHERE tmp.OldAddressID IS NOT NULL;

DROP TABLE OldAddressTmp;
GO

--[mysql: Skip]
--[mssql: Native]
--[MinVersion(Hash = d34f24106b4b1dc3e6d11edc5f8435fc240a3f8a6800814c43de05700cc82890)]
IF (COL_LENGTH('Address', 'OldAddressID') IS NULL) BEGIN
  ALTER TABLE Address ADD OldAddressID INT NULL
END
GO

--[mysql: Skip]
--[mssql: Native]
--[OldHash(Hash = 5ac40e1646e4afa00bad491d60ccebbcb84b89480f2c5c0d1ff4c8e932f8133c)]
--[MinVersion(Hash = 2dafbca0737072818eda88402a085e2753f2efff73b44999617896d2daa00871)]
UPDATE addr
SET addr.BAccountID = A.MaxBAccountID
FROM Address addr
INNER JOIN (
    SELECT MAX(BAccountID) MaxBAccountID, DefAddressID, CompanyID 
    FROM BAccount 
    WHERE DefAddressID IS NOT NULL 
    Group BY DefAddressID, CompanyID 
    HAVING COUNT(BAccountID) > 1
) A ON A.CompanyID = addr.CompanyID AND A.DefAddressID = addr.AddressID

INSERT INTO Address (
  CompanyID, 
  BAccountID, 
  AddressType, 
  AddressLine1, 
  AddressLine2, 
  AddressLine3, 
  City, 
  State, 
  CountryID, 
  PostalCode, 
  IsValidated, 
  TaxLocationCode, 
  TaxMunicipalCode, 
  TaxSchoolCode, 
  CreatedByID, 
  CreatedByScreenID, 
  CreatedDateTime, 
  LastModifiedByID, 
  LastModifiedByScreenID, 
  LastModifiedDateTime, 
  RevisionID, 
  NoteID,
  OldAddressID
)
SELECT 
  A.CompanyID, 
  B.BAccountID, 
  A.AddressType, 
  A.AddressLine1, 
  A.AddressLine2, 
  A.AddressLine3, 
  A.City, 
  A.State, 
  A.CountryID, 
  A.PostalCode, 
  A.IsValidated, 
  A.TaxLocationCode, 
  A.TaxMunicipalCode, 
  A.TaxSchoolCode, 
  A.CreatedByID, 
  A.CreatedByScreenID, 
  A.CreatedDateTime, 
  A.LastModifiedByID, 
  A.LastModifiedByScreenID, 
  A.LastModifiedDateTime, 
  A.RevisionID, 
  NEWID(),
  A.AddressID
FROM BAccount B
  INNER JOIN (
    SELECT MAX(BAccountID) MaxAcc, DefAddressID, CompanyID FROM BAccount WHERE DefAddressID IS NOT NULL Group BY DefAddressID, CompanyID HAVING COUNT(BAccountID) > 1
  ) M ON B.DefAddressID = M.DefAddressID AND B.CompanyID = M.CompanyID
  LEFT JOIN Address A ON A.CompanyID = B.CompanyID AND A.AddressID = B.DefAddressID
WHERE A.BAccountID <> B.BAccountID

UPDATE B
SET B.DefAddressID = tmp.AddressID
FROM BAccount B
  INNER JOIN Address tmp ON B.CompanyID = tmp.CompanyID AND B.BAccountID = tmp.BAccountID
WHERE tmp.OldAddressID IS NOT NULL
GO

--[mysql: Skip]
--[mssql: Native]
--[MinVersion(Hash = 669935e3f66474b624b11375b6c0221147ef240c2bff120634c1652fc0e173e7)]
IF (COL_LENGTH('Address', 'OldAddressID') IS NOT NULL) BEGIN
  ALTER TABLE Address DROP COLUMN OldAddressID
END
GO

-- Fixing ProjectID
--[mssql: Native]
--[mysql: Skip]
--[MinVersion(Hash = 98663b3e4f28f7d1bdee24564fb937c28888fb8959917166955fd370f248fb11)]
UPDATE CROpportunityProducts
SET ProjectID = CROpportunityRevision.ProjectID
FROM CROpportunityProducts
INNER JOIN CROpportunityRevision
	ON CROpportunityRevision.NoteID = CROpportunityProducts.QuoteID
		AND CROpportunityRevision.CompanyID = CROpportunityProducts.CompanyID
		AND CROpportunityProducts.ProjectID IS NULL
WHERE CROpportunityProducts.CompanyID > 1
GO

-- Fixing ProjectID
--[mssql: Skip]
--[mysql: Native]
--[MinVersion(Hash = 2f5083132e67e1b9a443e87fd1fb48f1937821b21a81922e63dee19d3abf3c83)]
UPDATE CROpportunityProducts
INNER JOIN CROpportunityRevision
	ON CROpportunityRevision.NoteID = CROpportunityProducts.QuoteID
		AND CROpportunityRevision.CompanyID = CROpportunityProducts.CompanyID
		AND CROpportunityProducts.ProjectID IS NULL
SET CROpportunityProducts.ProjectID = CROpportunityRevision.ProjectID
WHERE CROpportunityProducts.CompanyID > 1
GO

--[mssql: Native]
--[mysql: Skip]
--[IfExists(Table = CRLead)]
--[MinVersion(Hash = a102c30ef5bcffb80398d5119fd4dae98462343c8966e08fb3152ea5b043e868)]
EXEC pp_UpdateConfigurationOfCustomGIs @oldTableName = 'PX.Objects.CR.Lead', @oldFieldName = NULL, @newTableName = 'PX.Objects.CR.CRLead', @newFieldName = NULL
EXEC pp_UpdateConfigurationOfCustomGIs @oldTableName = NULL, @oldFieldName = 'contactID', @newTableName = 'PX.Objects.CR.CRLead', @newFieldName = 'noteID'
EXEC pp_UpdateConfigurationOfCustomGIs @oldTableName = NULL, @oldFieldName = 'convertedLeadID', @newTableName = 'PX.Objects.CR.CROpportunity', @newFieldName = 'leadID'
GO

--[mssql: Skip]
--[mysql: Native]
--[IfExists(Table = CRLead)]
--[MinVersion(Hash = ac98ff2391077c3c0539d65cb3045537e365abc70312cd7868d7bfd80278cd7c)]
CALL pp_UpdateConfigurationOfCustomGIs('PX.Objects.CR.Lead', NULL, 'PX.Objects.CR.CRLead', NULL);
CALL pp_UpdateConfigurationOfCustomGIs(NULL, 'contactID', 'PX.Objects.CR.CRLead', 'noteID');
CALL pp_UpdateConfigurationOfCustomGIs(NULL, 'convertedLeadID', 'PX.Objects.CR.CROpportunity', 'leadID');
GO

-- Add roles for CR205000/CR2050PL screens according to roles for CR207000/CR2070PL
--[mssql: Native]
--[mysql: Skip]
--[MinVersion(Hash = ad373a1f591dc2aa1d0281606f01970930b49467114a6e3511545dd85fc5ee7a)]
EXEC pp_CopyCustomerRolesInGraphForScreenIfNotSetYet @sourceScreenID = 'CR207000', @newScreenID = 'CR205000'
EXEC pp_CopyCustomerRolesInGraphForScreenIfNotSetYet @sourceScreenID = 'CR2070PL', @newScreenID = 'CR2050PL'
GO

-- Add roles for CR205000/CR2050PL screens according to roles for CR207000/CR2070PL
--[mssql: Skip]
--[mysql: Native]
--[MinVersion(Hash = eddea3d07b58418f65987ffd05f24c48ceed2e947e95a028b4eb412a7ce2d3b5)]
CALL pp_CopyCustomerRolesInGraphForScreenIfNotSetYet('CR207000', 'CR205000');
CALL pp_CopyCustomerRolesInGraphForScreenIfNotSetYet('CR2070PL', 'CR2050PL');
GO



--[RefreshMetadata()]
GO

--FIN scripts
--[MinVersion(Hash = 4034287911b9752353d27a074b9aeab97d092a15d1ee01bc15006b3b68cecbce)]

GO

--[mssql: Native]
--[mysql: Skip]
--[MinVersion(Hash = 451916492024629a4ca9734a97c9dd290027673845b54ac60c2f0999768a63a8)]
CREATE INDEX ARAddress_CreatedByID ON ARAddress (CompanyID, CreatedByID);
CREATE INDEX ARContact_CreatedByID ON ARContact (CompanyID, CreatedByID);
GO

--[mssql: Skip]
--[mysql: Native]
--[MinVersion(Hash = 451916492024629a4ca9734a97c9dd290027673845b54ac60c2f0999768a63a8)]
CREATE INDEX ARAddress_CreatedByID ON ARAddress (CompanyID, CreatedByID);
CREATE INDEX ARContact_CreatedByID ON ARContact (CompanyID, CreatedByID);
GO

-- Restore empty SOAddress.CustomerAddressID from BAccount.DefAddressID
--
--[MinVersion(Hash = d3e4967591b75d40f89d94b210b4c59d0086c84a4629e23dd04b04b37be33a1f)]
UPDATE SOAddress SET
	CustomerAddressID = BAccount.DefAddressID
FROM SOAddress
JOIN BAccount
	ON BAccount.CompanyID = SOAddress.CompanyID
		AND BAccount.BAccountID = SOAddress.CustomerID
WHERE SOAddress.CustomerAddressID IS NULL
GO

--[MinVersion(Hash = f7ffc4934b565b428f770c544660403522d3483318fbce7f7777a8d8ed9cfbeb)]
/* 
////////////////////////////////////////////////////////
///                SHIPPING ADDRESSES                ///
////////////////////////////////////////////////////////
*/

-- Update script for AR ARInvoice.ShipAddressID.
-- We should take default address from the location,
-- related to the document. Note we don't need to
-- create a new ARAddress record for each document, this
-- is the reason for GROUP BY expression below.
-- 
INSERT INTO ARAddress 
(
	CompanyID, 
	CustomerID,
	RevisionID,
	AddressLine1,
	AddressLine2,
	AddressLine3,
	City,
	State,
	CountryID,
	PostalCode,
	IsValidated,
	NoteID,
	CreatedByID,
	CreatedByScreenID,
	CreatedDateTime,
	LastModifiedByID,
	LastModifiedByScreenID,
	LastModifiedDateTime,
	IsDefaultBillAddress,
	CustomerAddressID
)
(
	SELECT
		Address.CompanyID, 
		MAX(Address.BAccountID),
		MAX(Address.RevisionID),
		MAX(Address.AddressLine1),
		MAX(Address.AddressLine2),
		MAX(Address.AddressLine3),
		MAX(Address.City),
		MAX(Address.State),
		MAX(Address.CountryID),
		MAX(Address.PostalCode),
		MAX(CONVERT(int, Address.IsValidated)),
		NEWID(),
		'00000000-0000-0000-0000-000000000000',
		'00000000',
		GETDATE(),
		'00000000-0000-0000-0000-000000000000',
		'00000000',
		GETDATE(),
		1,
		Address.AddressID
	FROM ARInvoice 
	LEFT JOIN SOInvoice ON SOInvoice.CompanyID = ARInvoice.CompanyID
		AND SOInvoice.DocType = ARInvoice.DocType
		AND SOInvoice.RefNbr = ARInvoice.RefNbr
	INNER JOIN ARRegister ON ARRegister.CompanyID = ARInvoice.CompanyID
		AND ARRegister.DocType = ARInvoice.DocType
		AND ARRegister.RefNbr = ARInvoice.RefNbr
	INNER JOIN Location ON Location.CompanyID = ARRegister.CompanyID
		AND Location.BAccountID = ARRegister.CustomerID 
		AND Location.LocationID = ARRegister.CustomerLocationID
	INNER JOIN Address ON Address.CompanyID = Location.CompanyID 
		AND Address.BAccountID = Location.BAccountID
		AND Address.AddressID = Location.DefAddressID
	LEFT JOIN ARAddress ON ARAddress.CompanyID = Address.CompanyID
		AND ARAddress.CustomerID = Address.BAccountID
		AND ARAddress.CustomerAddressID = Address.AddressID
		AND ARAddress.RevisionID = Address.RevisionID
		AND ARAddress.IsDefaultBillAddress = 1
	WHERE 
		SOInvoice.ShipAddressID IS NULL
		AND ARInvoice.ShipAddressID IS NULL
		AND ARAddress.CompanyID IS NULL
	GROUP BY 
		Address.CompanyID, 
		Address.AddressID
)

UPDATE ARInvoice SET 
	ARInvoice.ShipAddressID = data.AddressID
FROM
	ARInvoice
	INNER JOIN 
		(SELECT
			ARInvoice.CompanyID,
			ARInvoice.DocType,
			ARInvoice.RefNbr,
			(SELECT TOP 1
				ARAddress.AddressID
			FROM
				ARAddress
			WHERE
				ARAddress.CompanyID = Address.CompanyID
				AND ARAddress.CustomerID = Address.BAccountID
				AND ARAddress.CustomerAddressID = Address.AddressID
				AND ARAddress.RevisionID = Address.RevisionID
				AND ARAddress.IsDefaultBillAddress = 1) AddressID
		FROM ARInvoice
		LEFT JOIN SOInvoice ON SOInvoice.CompanyID = ARInvoice.CompanyID
	AND SOInvoice.DocType = ARInvoice.DocType
	AND SOInvoice.RefNbr = ARInvoice.RefNbr
		INNER JOIN ARRegister ON ARRegister.CompanyID = ARInvoice.CompanyID
	AND ARRegister.DocType = ARInvoice.DocType
	AND ARRegister.RefNbr = ARInvoice.RefNbr
		INNER JOIN Location ON Location.CompanyID = ARRegister.CompanyID
	AND Location.BAccountID = ARRegister.CustomerID 
	AND Location.LocationID = ARRegister.CustomerLocationID
		INNER JOIN Address ON Address.CompanyID = Location.CompanyID 
	AND Address.BAccountID = Location.BAccountID
	AND Address.AddressID = Location.DefAddressID
		WHERE 
	SOInvoice.ShipAddressID IS NULL
			AND ARInvoice.ShipAddressID IS NULL) data
	ON ARInvoice.CompanyID = data.CompanyID
		AND ARInvoice.DocType = data.DocType
		AND ARInvoice.RefNbr = data.RefNbr

-- Update script for SO ARInvoice.ShipAddressID.
-- We should copy existing SO address from the 
-- SOInvoice.ShipAddressID field. 
-- Assumed that the SOAddress.NoteID is not null
-- 
INSERT INTO ARAddress 
(
	CompanyID, 
	CustomerID,
	RevisionID,
	AddressLine1,
	AddressLine2,
	AddressLine3,
	City,
	State,
	CountryID,
	PostalCode,
	IsValidated,
	NoteID,
	CreatedByID,
	CreatedByScreenID,
	CreatedDateTime,
	LastModifiedByID,
	LastModifiedByScreenID,
	LastModifiedDateTime,
	IsDefaultBillAddress,
	CustomerAddressID
)
(
	SELECT
		SOAddress.CompanyID, 
		MAX(SOAddress.CustomerID),
		MAX(SOAddress.RevisionID),
		MAX(SOAddress.AddressLine1),
		MAX(SOAddress.AddressLine2),
		MAX(SOAddress.AddressLine3),
		MAX(SOAddress.City),
		MAX(SOAddress.State),
		MAX(SOAddress.CountryID),
		MAX(SOAddress.PostalCode),
		MAX(CONVERT(int, SOAddress.IsValidated)),
		NEWID(),
		MAX(SOAddress.NoteID),
		'00000000',
		GETDATE(),
		'00000000-0000-0000-0000-000000000000',
		'00000000',
		GETDATE(),
		MAX(CONVERT(int, SOAddress.IsDefaultAddress)),
		MAX(SOAddress.CustomerAddressID)
	FROM ARInvoice 
	INNER JOIN SOInvoice ON SOInvoice.CompanyID = ARInvoice.CompanyID
		AND SOInvoice.DocType = ARInvoice.DocType
		AND SOInvoice.RefNbr = ARInvoice.RefNbr
	INNER JOIN SOAddress ON SOAddress.CompanyID = SOInvoice.CompanyID
		AND SOAddress.CustomerID = SOInvoice.CustomerID
		AND SOAddress.AddressID = SOInvoice.ShipAddressID
	WHERE 
		SOInvoice.ShipAddressID IS NOT NULL
		AND ARInvoice.ShipAddressID IS NULL
	GROUP BY 
		SOAddress.CompanyID, 
		SOAddress.AddressID
)

UPDATE ARInvoice SET 
	ARInvoice.ShipAddressID = ARAddress.AddressID
FROM ARInvoice 
INNER JOIN SOInvoice ON SOInvoice.CompanyID = ARInvoice.CompanyID
	AND SOInvoice.DocType = ARInvoice.DocType
	AND SOInvoice.RefNbr = ARInvoice.RefNbr
INNER JOIN SOAddress ON SOAddress.CompanyID = SOInvoice.CompanyID
	AND SOAddress.CustomerID = SOInvoice.CustomerID
	AND SOAddress.AddressID = SOInvoice.ShipAddressID
INNER JOIN ARAddress ON ARAddress.CompanyID = SOAddress.CompanyID
	AND ARAddress.CreatedByID = SOAddress.NoteID
WHERE 
	SOInvoice.ShipAddressID IS NOT NULL
	AND ARInvoice.ShipAddressID IS NULL


/* 
////////////////////////////////////////////////////////
///                SHIPPING CONTACTS                 ///
////////////////////////////////////////////////////////
*/

-- Update script for AR ARInvoice.ShipContactID.
-- We should take default contact from the location,
-- related to the document. Note we don't need to
-- create a new ARContact record for each document, this
-- is the reason for GROUP BY expression below.
-- 
INSERT INTO ARContact 
(
	CompanyID,
	CustomerID,
	CustomerContactID,
	RevisionID,
	IsDefaultContact,
	Title,
	Salutation,
	Attention,
	FullName,
	Email,
	Phone1,
	Phone1Type,
	Phone2,
	Phone2Type,
	Phone3,
	Phone3Type,
	Fax,
	FaxType,
	NoteID,
	CreatedByID,
	CreatedByScreenID,
	CreatedDateTime,
	LastModifiedByID,
	LastModifiedByScreenID,
	LastModifiedDateTime
)
(
	SELECT
		Contact.CompanyID,
		MAX(Contact.BAccountID),
		Contact.ContactID,
		MAX(Contact.RevisionID),
		1,
		MAX(Contact.Title),
		MAX(Contact.Salutation),
		MAX(Contact.Attention),
		MAX(Contact.FullName),
		MAX(Contact.Email),
		MAX(Contact.Phone1),
		MAX(Contact.Phone1Type),
		MAX(Contact.Phone2),
		MAX(Contact.Phone2Type),
		MAX(Contact.Phone3),
		MAX(Contact.Phone3Type),
		MAX(Contact.Fax),
		MAX(Contact.FaxType),
		NEWID(),
		'00000000-0000-0000-0000-000000000000',
		'00000000',
		GETDATE(),
		'00000000-0000-0000-0000-000000000000',
		'00000000',
		GETDATE()
	FROM ARInvoice 
	LEFT JOIN SOInvoice ON SOInvoice.CompanyID = ARInvoice.CompanyID
		AND SOInvoice.DocType = ARInvoice.DocType
		AND SOInvoice.RefNbr = ARInvoice.RefNbr
	INNER JOIN ARRegister ON ARRegister.CompanyID = ARInvoice.CompanyID
		AND ARRegister.DocType = ARInvoice.DocType
		AND ARRegister.RefNbr = ARInvoice.RefNbr
	INNER JOIN Location ON Location.CompanyID = ARRegister.CompanyID
		AND Location.BAccountID = ARRegister.CustomerID 
		AND Location.LocationID = ARRegister.CustomerLocationID
	INNER JOIN Contact ON Contact.CompanyID = Location.CompanyID 
		AND Contact.BAccountID = Location.BAccountID
		AND Contact.ContactID = Location.DefContactID
	LEFT JOIN ARContact ON ARContact.CompanyID = Contact.CompanyID
		AND ARContact.CustomerID = Contact.BAccountID
		AND ARContact.CustomerContactID = Contact.ContactID
		AND ARContact.RevisionID = Contact.RevisionID
		AND ARContact.IsDefaultContact = 1
	WHERE 
		SOInvoice.ShipContactID IS NULL
		AND ARInvoice.ShipContactID IS NULL
		AND ARContact.CompanyID IS NULL
	GROUP BY 
		Contact.CompanyID, 
		Contact.ContactID
)

UPDATE ARInvoice SET
	ARInvoice.ShipContactID = data.ContactID
FROM
	ARInvoice
	INNER JOIN
		(SELECT
			ARInvoice.CompanyID,
			ARInvoice.DocType,
			ARInvoice.RefNbr,
			(SELECT TOP 1 
				ARContact.ContactID
			FROM
				ARContact
			WHERE
				ARContact.CompanyID = Contact.CompanyID
				AND ARContact.CustomerID = Contact.BAccountID
				AND ARContact.CustomerContactID = Contact.ContactID
				AND ARContact.RevisionID = Contact.RevisionID
				AND ARContact.IsDefaultContact = 1) ContactID
		FROM ARInvoice 
		LEFT JOIN SOInvoice ON SOInvoice.CompanyID = ARInvoice.CompanyID
	AND SOInvoice.DocType = ARInvoice.DocType
	AND SOInvoice.RefNbr = ARInvoice.RefNbr
		INNER JOIN ARRegister ON ARRegister.CompanyID = ARInvoice.CompanyID
	AND ARRegister.DocType = ARInvoice.DocType
	AND ARRegister.RefNbr = ARInvoice.RefNbr
		INNER JOIN Location ON Location.CompanyID = ARRegister.CompanyID
	AND Location.BAccountID = ARRegister.CustomerID 
	AND Location.LocationID = ARRegister.CustomerLocationID
		INNER JOIN Contact ON Contact.CompanyID = Location.CompanyID 
	AND Contact.BAccountID = Location.BAccountID
	AND Contact.ContactID = Location.DefContactID
		WHERE 
	SOInvoice.ShipContactID IS NULL
			AND	ARInvoice.ShipContactID IS NULL) data
	ON ARInvoice.CompanyID = data.CompanyID
	AND ARInvoice.DocType = data.DocType
	AND ARInvoice.RefNbr = data.RefNbr

-- Update script for SO ARInvoice.ShipContactID.
-- We should copy existing SO contact from the 
-- SOInvoice.ShipContactID field. 
-- Assumed that the SOContact.NoteID is not null
-- 
INSERT INTO ARContact 
(
	CompanyID,
	CustomerID,
	CustomerContactID,
	RevisionID,
	IsDefaultContact,
	Title,
	Salutation,
	Attention,
	FullName,
	Email,
	Phone1,
	Phone1Type,
	Phone2,
	Phone2Type,
	Phone3,
	Phone3Type,
	Fax,
	FaxType,
	NoteID,
	CreatedByID,
	CreatedByScreenID,
	CreatedDateTime,
	LastModifiedByID,
	LastModifiedByScreenID,
	LastModifiedDateTime
)
(
	SELECT
		SOContact.CompanyID,
		MAX(SOContact.CustomerID),
		SOContact.ContactID,
		MAX(SOContact.RevisionID),
		MAX(CONVERT(int, SOContact.IsDefaultContact)),
		MAX(SOContact.Title),
		MAX(SOContact.Salutation),
		MAX(SOContact.Attention),
		MAX(SOContact.FullName),
		MAX(SOContact.Email),
		MAX(SOContact.Phone1),
		MAX(SOContact.Phone1Type),
		MAX(SOContact.Phone2),
		MAX(SOContact.Phone2Type),
		MAX(SOContact.Phone3),
		MAX(SOContact.Phone3Type),
		MAX(SOContact.Fax),
		MAX(SOContact.FaxType),
		NEWID(),
		MAX(SOContact.NoteID),
		'00000000',
		GETDATE(),
		'00000000-0000-0000-0000-000000000000',
		'00000000',
		GETDATE()
	FROM ARInvoice 
	INNER JOIN SOInvoice ON SOInvoice.CompanyID = ARInvoice.CompanyID
		AND SOInvoice.DocType = ARInvoice.DocType
		AND SOInvoice.RefNbr = ARInvoice.RefNbr
	INNER JOIN SOContact ON SOContact.CompanyID = SOInvoice.CompanyID
		AND SOContact.CustomerID = SOInvoice.CustomerID
		AND SOContact.ContactID = SOInvoice.ShipContactID
	WHERE 
		SOInvoice.ShipContactID IS NOT NULL
		AND ARInvoice.ShipContactID IS NULL
	GROUP BY 
		SOContact.CompanyID, 
		SOContact.ContactID
)

UPDATE ARInvoice SET 
	ARInvoice.ShipContactID = ARContact.ContactID
FROM ARInvoice 
INNER JOIN SOInvoice ON SOInvoice.CompanyID = ARInvoice.CompanyID
	AND SOInvoice.DocType = ARInvoice.DocType
	AND SOInvoice.RefNbr = ARInvoice.RefNbr
INNER JOIN SOContact ON SOContact.CompanyID = SOInvoice.CompanyID
	AND SOContact.CustomerID = SOInvoice.CustomerID
	AND SOContact.ContactID = SOInvoice.ShipContactID
INNER JOIN ARContact ON ARContact.CompanyID = SOContact.CompanyID
	AND ARContact.CreatedByID = SOContact.NoteID
WHERE 
	SOInvoice.ShipContactID IS NOT NULL
	AND ARInvoice.ShipContactID IS NULL
GO

--[MinVersion(Hash = 2799d9bf42c8ebf9b96d1f59abe4d61e4b1dd8d156b94790478b55f981a6646c)]
UPDATE FinPeriod SET NoteID= NEWID() WHERE NoteID IS NULL
GO

-- Delete duplicate removed Organizations by BAccountID
--[MinVersion(Hash = 0ddcba2880f286361c9c5372b34536ca47ba2a0a9535823532088f53960a0b42)]
DELETE Organization
FROM Organization
INNER JOIN 
	(SELECT CompanyID, BAccountID
		FROM Organization
		GROUP BY CompanyID, BAccountID
		HAVING COUNT (*)> 1) AS Duplicate
	ON Organization.CompanyID = Duplicate.CompanyID
	AND Organization.BAccountID = Duplicate.BAccountID
	AND Organization.DeletedDatabaseRecord = 1
GO

-- Delete duplicate removed Branches by BAccountID
--[MinVersion(Hash = 775f96cddb21326b935cbb3caf0083506c9b228b645fad4dd590189e9611d0dc)]
DELETE Branch
FROM Branch
INNER JOIN 
	(SELECT CompanyID, BAccountID
		FROM Branch
		GROUP BY CompanyID, BAccountID
		HAVING COUNT (*)> 1) AS Duplicate
	ON Branch.CompanyID = Duplicate.CompanyID
	AND Branch.BAccountID = Duplicate.BAccountID
	AND Branch.DeletedDatabaseRecord = 1
GO

--Drop already deleted branch without organization
--[MinVersion(Hash = 1642a4fb41560d6f48841fdaeb50a3944bcb73e69431c1035e2260a7061547a2)]
DELETE Branch
FROM Branch
LEFT JOIN Organization 
	ON Organization.CompanyID = Branch.CompanyID
	AND Organization.OrganizationID = Branch.OrganizationID
WHERE Branch.DeletedDatabaseRecord = 1
	AND Organization.OrganizationID IS NULL;
GO

--[mssql: Native]
--[mysql: Skip]
--[OldHash(Hash = c93275ec8dd76733d3a6e6d0488223a16774d76684e4ed42d15922d7de6d987d)]
--[MinVersion(Hash = f5aff9a03e744e58e277e20e33d83171a36ce0980f144761b852ec7a67c9561b)]
CREATE INDEX GLTran_RefNbr ON GLTran(CompanyID, Module, RefNbr)


UPDATE GLTran 
SET RefNbr = ISNULL(DRSchedule.ScheduleNbr, RIGHT('00000000' + CAST(ScheduleID AS NVARCHAR(8)), 8))
FROM GLTran
INNER JOIN DRSchedule 
	ON GLTran.CompanyID = DRSchedule.CompanyID
	AND GLTran.RefNbr = CAST(DRSchedule.ScheduleID as NVARCHAR)
WHERE GLTran.Module = 'DR'

DROP INDEX GLTran_RefNbr ON GLTran
GO

--[mysql: Native]
--[mssql: Skip]
--[OldHash(Hash = 47ffd1a3e6ef0ccf6e38a1efd794601013e4ddf3351d47252bda4e67379739fe)]
--[MinVersion(Hash = 4e887ed451f29906b70dcc3371b378073d8efa7e799352bb8de01809580ac65d)]
CREATE INDEX GLTran_RefNbr ON GLTran(CompanyID, Module, RefNbr);


UPDATE GLTran 
INNER JOIN DRSchedule 
	ON (GLTran.CompanyID = DRSchedule.CompanyID) 
	AND (GLTran.RefNbr = CAST(DRSchedule.ScheduleID AS NCHAR))
SET GLTran.RefNbr = IFNULL(DRSchedule.ScheduleNbr, RIGHT(CONCAT('00000000', CAST(ScheduleID AS NCHAR)), 8)) 
WHERE GLTran.Module = 'DR';

DROP INDEX GLTran_RefNbr ON GLTran;
GO

--[SmartExecute]
--[MinVersion(Hash = cd5cf7ccd784a3bcb2a373298f3a60d16fe03f60f86ee870a905577de2438a38)]
UPDATE fa SET
	fa.IsAcquired = 1
FROM FixedAsset fa
INNER JOIN FATran t ON fa.AssetID = t.AssetID
WHERE
    fa.IsAcquired IS NULL
	AND t.TranType = 'P+'
	AND t.Released = 1
GO

--Shifted Calendars
--[MinVersion(Hash = 697c85ffa01fc454649d98016de17cc945e8e599ecd7f318b7e45734ef7c7467)]
UPDATE GLTran
SET TranPeriodID = FinPeriodID

UPDATE Batch
SET TranPeriodID = FinPeriodID

UPDATE APTran
SET TranPeriodID = FinPeriodID

UPDATE ARTran
SET TranPeriodID = FinPeriodID

UPDATE ARRegister
SET TranPeriodID = FinPeriodID

UPDATE APRegister
SET TranPeriodID = FinPeriodID

UPDATE APPayment
SET AdjTranPeriodID = AdjFinPeriodID

UPDATE ARPayment
SET AdjTranPeriodID = AdjFinPeriodID

UPDATE APAdjust
SET AdjdTranPeriodID = AdjdFinPeriodID
, AdjgTranPeriodID = AdjgFinPeriodID

UPDATE ARAdjust
SET AdjdTranPeriodID = AdjdFinPeriodID
, AdjgTranPeriodID = AdjgFinPeriodID

UPDATE GLHistory
SET CuryTranBegBalance = CuryFinBegBalance
, CuryTranPtdCredit = CuryFinPtdCredit
, CuryTranPtdDebit = CuryFinPtdDebit
, CuryTranYtdBalance = CuryFinYtdBalance
, TranBegBalance = FinBegBalance
, TranPtdCredit = FinPtdCredit
, TranPtdDebit = FinPtdDebit
, TranYtdBalance = FinYtdBalance

UPDATE SVATConversionHist
SET AdjdTranPeriodID = AdjdFinPeriodID,
AdjgTranPeriodID = AdjgFinPeriodID

UPDATE APHistory
SET TranBegBalance = FinBegBalance,
TranPtdCrAdjustments = FinPtdCrAdjustments,
TranPtdDeposits = FinPtdDeposits,
TranPtdDiscTaken = FinPtdDiscTaken,
TranPtdDrAdjustments = FinPtdDrAdjustments,
TranPtdPayments = FinPtdPayments,
TranPtdPurchases = FinPtdPurchases,
TranPtdRetainageReleased = FinPtdRetainageReleased,
TranPtdRetainageWithheld = FinPtdRetainageWithheld,
TranPtdRGOL = FinPtdRGOL,
TranPtdWhTax = FinPtdWhTax,
TranYtdBalance = FinYtdBalance,
TranYtdDeposits = FinYtdDeposits,
TranYtdRetainageReleased = FinYtdRetainageReleased,
TranYtdRetainageWithheld = FinYtdRetainageWithheld

UPDATE ARHistory
SET TranBegBalance = FinBegBalance,
TranPtdSales = FinPtdSales,
TranPtdPayments = FinPtdPayments,
TranPtdDrAdjustments = FinPtdDrAdjustments,
TranPtdCrAdjustments = FinPtdCrAdjustments,
TranPtdDiscounts = FinPtdDiscounts,
TranPtdCOGS = FinPtdCOGS,
TranPtdRGOL = FinPtdRGOL,
TranPtdFinCharges = FinPtdFinCharges,
TranYtdBalance = FinYtdBalance,
TranPtdDeposits = FinPtdDeposits,
TranYtdDeposits = FinYtdDeposits,
TranPtdItemDiscounts = FinPtdItemDiscounts,     
TranPtdRetainageWithheld = FinPtdRetainageWithheld,
TranYtdRetainageWithheld = FinYtdRetainageWithheld,
TranPtdRetainageReleased = FinPtdRetainageReleased,
TranYtdRetainageReleased = FinYtdRetainageReleased

UPDATE CuryAPHistory
SET CuryTranBegBalance = CuryFinBegBalance,
CuryTranPtdCrAdjustments = CuryFinPtdCrAdjustments,
CuryTranPtdDeposits = CuryFinPtdDeposits,
CuryTranPtdDiscTaken = CuryFinPtdDiscTaken,
CuryTranPtdDrAdjustments = CuryFinPtdDrAdjustments,
CuryTranPtdPayments = CuryFinPtdPayments,
CuryTranPtdPurchases = CuryFinPtdPurchases,
CuryTranPtdRetainageReleased = CuryFinPtdRetainageReleased,
CuryTranPtdRetainageWithheld = CuryFinPtdRetainageWithheld,
CuryTranPtdWhTax = CuryFinPtdWhTax,
CuryTranYtdBalance = CuryFinYtdBalance,
CuryTranYtdDeposits = CuryFinYtdDeposits,
CuryTranYtdRetainageReleased = CuryFinYtdRetainageReleased,
CuryTranYtdRetainageWithheld = CuryFinYtdRetainageWithheld,
TranBegBalance = FinBegBalance,
TranPtdCrAdjustments = FinPtdCrAdjustments,
TranPtdDeposits = FinPtdDeposits,
TranPtdDiscTaken = FinPtdDiscTaken,
TranPtdDrAdjustments = FinPtdDrAdjustments,
TranPtdPayments = FinPtdPayments,
TranPtdPurchases = FinPtdPurchases,
TranPtdRetainageReleased = FinPtdRetainageReleased,
TranPtdRetainageWithheld = FinPtdRetainageWithheld,
TranPtdRGOL = FinPtdRGOL,
TranPtdWhTax = FinPtdWhTax,
TranYtdBalance = FinYtdBalance,
TranYtdDeposits = FinYtdDeposits,
TranYtdRetainageReleased = FinYtdRetainageReleased,
TranYtdRetainageWithheld = FinYtdRetainageWithheld

UPDATE CuryARHistory
SET TranBegBalance = FinBegBalance,
TranPtdSales = FinPtdSales,
TranPtdPayments = FinPtdPayments,
TranPtdDrAdjustments = FinPtdDrAdjustments,
TranPtdCrAdjustments = FinPtdCrAdjustments,
TranPtdDiscounts = FinPtdDiscounts,
TranPtdRGOL = FinPtdRGOL,
TranPtdCOGS = FinPtdCOGS,
TranPtdFinCharges = FinPtdFinCharges,
TranYtdBalance = FinYtdBalance,
TranPtdDeposits = FinPtdDeposits,
TranYtdDeposits = FinYtdDeposits,
CuryTranBegBalance = CuryFinBegBalance,
CuryTranPtdSales = CuryFinPtdSales,
CuryTranPtdPayments = CuryFinPtdPayments,
CuryTranPtdDrAdjustments = CuryFinPtdDrAdjustments,
CuryTranPtdCrAdjustments = CuryFinPtdCrAdjustments,
CuryTranPtdDiscounts = CuryFinPtdDiscounts,
CuryTranPtdFinCharges = CuryFinPtdFinCharges,
CuryTranYtdBalance = CuryFinYtdBalance,
CuryTranPtdDeposits = CuryFinPtdDeposits,
CuryTranYtdDeposits = CuryFinYtdDeposits,
CuryTranPtdRetainageWithheld = CuryFinPtdRetainageWithheld,
TranPtdRetainageWithheld = FinPtdRetainageWithheld,
CuryTranYtdRetainageWithheld = CuryFinYtdRetainageWithheld,
TranYtdRetainageWithheld = FinYtdRetainageWithheld,
CuryTranPtdRetainageReleased = CuryFinPtdRetainageReleased,
TranPtdRetainageReleased = FinPtdRetainageReleased,
CuryTranYtdRetainageReleased = CuryFinYtdRetainageReleased,
TranYtdRetainageReleased = FinYtdRetainageReleased

UPDATE CuryGLHistory
SET TranPtdCredit = FinPtdCredit,
TranPtdDebit = FinPtdDebit,
TranYtdBalance = FinYtdBalance,
TranBegBalance = FinBegBalance,
CuryTranPtdCredit = CuryFinPtdCredit,
CuryTranPtdDebit = CuryFinPtdDebit,
CuryTranYtdBalance = CuryFinYtdBalance,
CuryTranBegBalance = CuryFinBegBalance
GO

--[MinVersion(Hash = 0d6580ad63adea3cd59b4e3c2d74d592b79abcdd57798bc1553df5822bde690d)]
UPDATE TaxAdjustment
SET TranPeriodID = FinPeriodID

UPDATE DRScheduleDetail
SET TranPeriodID = FinPeriodID

UPDATE DRScheduleTran
SET TranPeriodID = FinPeriodID
GO

--[MinVersion(Hash = 639c145ca9a175a2a074c4cc4f7b95d58a444c82defd7f7c50dd47d7bf44c990)]
UPDATE DRExpenseProjection
SET TranPTDProjected = PTDProjected,
	TranPTDRecognized = PTDRecognized,
	TranPTDRecognizedSamePeriod = PTDRecognizedSamePeriod
GO

--[MinVersion(Hash = 0ea4fefdfc681cc66464ed18fcb3a81a4f26b83e513f9369e372d981e2ca7d4c)]
UPDATE DRRevenueProjection
SET TranPTDProjected = PTDProjected,
	TranPTDRecognized = PTDRecognized,
	TranPTDRecognizedSamePeriod = PTDRecognizedSamePeriod
GO

--[MinVersion(Hash = 56e7d555ff0385f750d9acd2d6e2f5b149da4b759911c83cc0f6573b949e72c5)]
UPDATE DRExpenseBalance
SET TranBegBalance = BegBalance,
	TranBegProjected = BegProjected,
	TranPTDDeferred = PTDDeferred,
	TranPTDRecognized = PTDRecognized,
	TranPTDRecognizedSamePeriod = PTDRecognizedSamePeriod,
	TranPTDProjected = PTDProjected,
	TranEndBalance = EndBalance,
	TranEndProjected = EndProjected
GO

--[MinVersion(Hash = a563ecede192c3d9440688fda1ff80a90f1dd981d873a78d27cf5a118fb77e79)]
UPDATE DRRevenueBalance
SET TranBegBalance = BegBalance,
	TranBegProjected = BegProjected,
	TranPTDDeferred = PTDDeferred,
	TranPTDRecognized = PTDRecognized,
	TranPTDRecognizedSamePeriod = PTDRecognizedSamePeriod,
	TranPTDProjected = PTDProjected,
	TranEndBalance = EndBalance,
	TranEndProjected = EndProjected
GO

--[MinVersion(Hash = be4b5c628f6599b3cc161dca863bbb60cb33bcc02995408d8c53eba8f1d0c218)]
UPDATE DRScheduleDetail
SET CuryTotalAmt = TotalAmt,
CuryDefAmt = DefAmt
GO

--[MinVersion(Hash = 2ee2704e1ccc364609e8ac1185dba147bb2b8bfe9d865b1ecb7e26e5b6f134ba)]
update DRSchedule
set DRSchedule.CuryInfoID = ARTran.CuryInfoID
from DRSchedule 
inner join ARTran on 
DRSchedule.CompanyID = ARTran.CompanyID and
DRSchedule.DocType = ARTran.TranType and 
DRSchedule.RefNbr = ARTran.RefNbr and 
DRSchedule.LineNbr = ARTran.LineNbr
inner join CurrencyInfo on 
ARTran.CompanyID = CurrencyInfo.CompanyID and
ARTran.CuryInfoID = CurrencyInfo.CuryInfoID
where DRSchedule.CuryInfoID = 0 or 
DRSchedule.CuryInfoID is null
GO

--[MinVersion(Hash = a8837626a3173c8297717f7cb3982b198fb60cd509c6b62ba07b56f36cfe8b1a)]
update DRSchedule
set DRSchedule.CuryInfoID = APTran.CuryInfoID
from DRSchedule 
join APTran on 
DRSchedule.CompanyID = APTran.CompanyID and
DRSchedule.DocType = APTran.TranType and 
DRSchedule.RefNbr = APTran.RefNbr and 
DRSchedule.LineNbr = APTran.LineNbr
inner join CurrencyInfo on 
APTran.CompanyID = CurrencyInfo.CompanyID and
APTran.CuryInfoID = CurrencyInfo.CuryInfoID
where DRSchedule.CuryInfoID = 0 or 
DRSchedule.CuryInfoID is null
GO

--[MinVersion(Hash = 2d8b143901297bee46af8467cd07423d0f317f6e6ea80f1e8215b2fce308eafa)]
update ARRegister
set ARRegister.DRSchedCntr = ARRegister.LineCntr
from ARRegister 
inner join DRSchedule 
on ARRegister.CompanyID = DRSchedule.CompanyID and 
	ARRegister.DocType = DRSchedule.DocType and 
	ARRegister.RefNbr = DRSchedule.RefNbr
GO

--[MinVersion(Hash = d596abfec0a647e8a1ad09ab523143df7e576f597c5eaadc47bdd53c0b6a540c)]
update ba
  set 
      LegalName = coalesce(nullif(c.FullName, ''), nullif(c.DisplayName, ''), ba.AcctName)
from BAccount ba
     inner join Contact c
         on ba.CompanyID = c.CompanyID
            and ba.BAccountID = c.BAccountID
            and ba.DefContactID = c.ContactID
where ba.Type <> 'EP' and ba.LegalName is null;

update ba
  set 
      LegalName = coalesce(nullif(c.DisplayName, ''), ba.AcctName)
from BAccount ba
     inner join Contact c
         on ba.CompanyID = c.CompanyID
            and ba.ParentBAccountID = c.BAccountID
            and ba.DefContactID = c.ContactID
where ba.Type = 'EP' and ba.LegalName is null;
GO

--Insert missed Contacts for BAccount
--[mysql: Skip]
--[mssql: Native]
--[OldHash(Hash = 085f5ce06cdd6335fdf3d4c0d76c96006f680b9900d1528d73f8ac087e2a2464)]
--[OldHash(Hash = c5df889f4934d04ad2108b8d55d333f161098bedb967d4bc6dd9a767f308b0a0)]
--[OldHash(Hash = 06211c851235ce51910069b167b9747604ca7627fc10caa2d83fca10d1ede876)]
--[MinVersion(Hash = 50aaa2cae935fa17fa6214a9771043bd13d83087cfc6f059e8203831ad1f8313)]
UPDATE Contact SET DeletedDatabaseRecord = 0
	FROM Contact
		JOIN BAccount ON BAccount.CompanyID = Contact.CompanyID AND BAccount.DefContactID = Contact.ContactID
	WHERE BAccount.DeletedDatabaseRecord = 0 AND Contact.DeletedDatabaseRecord = 1

SET IDENTITY_INSERT Contact ON;
INSERT INTO Contact (
	CompanyID, ContactID, BAccountID, ContactType, FullName, DisplayName, IsActive, RevisionID, NoteID,
	FaxType, Phone1Type, Phone2Type, Phone3Type, Method, MajorStatus, DuplicateStatus, IsConvertable, Synchronize,
	GrammValidationDateTime, CreatedByID, CreatedByScreenID, CreatedDateTime, LastModifiedByID, LastModifiedByScreenID, LastModifiedDateTime)
SELECT
	CompanyID, DefContactID, BAccountID, 'AP', AcctName, '', 1, 1, NEWID(),
	'BF', 'B1', 'C', 'H1', 'A', -2, 'NV', 0, 1,
	'19000101', CreatedByID, CreatedByScreenID, CreatedDateTime, LastModifiedByID, LastModifiedByScreenID, LastModifiedDateTime
FROM BAccount WHERE DeletedDatabaseRecord = 0 AND DefContactID IS NOT NULL AND
	NOT EXISTS(SELECT * FROM Contact WHERE Contact.CompanyID = BAccount.CompanyID AND Contact.ContactID = BAccount.DefContactID)
SET IDENTITY_INSERT Contact OFF;


SET IDENTITY_INSERT Address ON;
INSERT INTO Address (
	CompanyID, AddressID, BAccountID, AddressType,
	CountryID,
	IsValidated, RevisionID, NoteID,
	CreatedByID, CreatedByScreenID, CreatedDateTime, LastModifiedByID, LastModifiedByScreenID, LastModifiedDateTime)
SELECT 
	BAccount.CompanyID, BAccount.DefAddressID, BAccount.BAccountID, 'BS',
	(SELECT TOP 1 CountryID -- get most common country
		FROM  (SELECT CompanyID, CountryID FROM Address GROUP BY Companyid, CountryID) T
		WHERE T.CompanyID = CompanyID
		GROUP BY CountryID
		ORDER BY Count(*) DESC),
	0, 1, NEWID(),
	CreatedByID, CreatedByScreenID, CreatedDateTime, NoteID, LastModifiedByScreenID, LastModifiedDateTime
FROM BAccount 
INNER JOIN (
	SELECT CompanyID, DefAddressID, Min(BAccountID) as BAccountID
	FROM BAccount
	WHERE DeletedDatabaseRecord = 0 AND DefContactID IS NOT NULL AND
	NOT EXISTS(SELECT * FROM Address WHERE Address.CompanyID = BAccount.CompanyID AND Address.AddressID = BAccount.DefAddressID)
	GROUP BY CompanyID, DefAddressID
) ba ON  BAccount.CompanyID=ba.CompanyID AND BAccount.BAccountID=ba.BAccountID
SET IDENTITY_INSERT Address OFF;


SET IDENTITY_INSERT Location ON;
INSERT INTO Location (
	CompanyID, BAccountID, LocationID, LocationCD, Descr, DefAddressID, DefContactID, IsActive, LocType, NoteID,
	CTaxCalcMode, VTaxCalcMode, VPaymentByType, VRcptQtyMax, VRcptQtyAction, VRcptQtyThreshold, VPrintOrder, VEmailOrder, CShipComplete, VPaymentLeadTime, VSeparateCheck, VPrepaymentPct,
	VPaymentInfoLocationID, VAPAccountLocationID, CARAccountLocationID, CAvalaraCustomerUsageType,
	CreatedByID, CreatedByScreenID, CreatedDateTime, LastModifiedByID, LastModifiedByScreenID, LastModifiedDateTime)
SELECT
	CompanyID, BAccountID, DefLocationID, 'MAIN', 'Primary Location', DefAddressID, DefContactID, 1, 'VE', NEWID(),
	'T', 'T', 0, 100, 'W', 100, 0, 0, 'L', 0, 0, 100,
	DefLocationID, DefLocationID, DefLocationID, '0',
	CreatedByID, CreatedByScreenID, CreatedDateTime, LastModifiedByID, LastModifiedByScreenID, LastModifiedDateTime
FROM BAccount WHERE DeletedDatabaseRecord = 0 AND DefContactID IS NOT NULL AND DefLocationID IS NOT NULL AND
	NOT EXISTS(SELECT * FROM Location WHERE Location.CompanyID = BAccount.CompanyID AND Location.LocationID = BAccount.DefLocationID) AND
	NOT EXISTS(SELECT * FROM Location WHERE Location.CompanyID = BAccount.CompanyID AND Location.BAccountID = BAccount.BAccountID AND Location.LocationCD = 'MAIN')
SET IDENTITY_INSERT Location OFF;

UPDATE l SET l.DefContactID = ba.DefContactID
	FROM Location AS l
	LEFT JOIN BAccount AS ba ON ba.CompanyID = l.CompanyID AND ba.BAccountID = l.BAccountID
	WHERE l.DefContactID IS NOT NULL AND NOT EXISTS(SELECT * FROM Contact AS c WHERE c.CompanyID = l.CompanyID AND c.ContactID = l.DefContactID);

UPDATE l SET l.DefAddressID = ba.DefAddressID
	FROM Location AS l
	LEFT JOIN BAccount AS ba ON ba.CompanyID = l.CompanyID AND ba.BAccountID = l.BAccountID
	WHERE l.DefAddressID IS NOT NULL AND NOT EXISTS(SELECT * FROM Address AS a WHERE a.CompanyID = l.CompanyID AND a.AddressID = l.DefAddressID);

UPDATE l SET l.VRemitContactID = ba.DefContactID
	FROM Location AS l
	LEFT JOIN BAccount AS ba ON ba.CompanyID = l.CompanyID AND ba.BAccountID = l.BAccountID
	WHERE l.VRemitContactID IS NOT NULL AND NOT EXISTS(SELECT * FROM Contact AS c WHERE c.CompanyID = l.CompanyID AND c.ContactID = l.VRemitContactID);

UPDATE l SET l.VRemitAddressID = ba.DefAddressID
	FROM Location AS l
	LEFT JOIN BAccount AS ba ON ba.CompanyID = l.CompanyID AND ba.BAccountID = l.BAccountID
	WHERE l.VRemitAddressID IS NOT NULL AND NOT EXISTS(SELECT * FROM Address AS a WHERE a.CompanyID = l.CompanyID AND a.AddressID = l.VRemitAddressID);
	
UPDATE cust SET cust.DefBillContactID = ba.DefContactID
	FROM Customer AS cust
	JOIN BAccount AS ba ON ba.CompanyID = cust.CompanyID AND ba.BAccountID = cust.BAccountID
	WHERE cust.DeletedDatabaseRecord = 0 AND cust.DefBillContactID IS NOT NULL AND NOT EXISTS(SELECT * FROM Contact AS cnt WHERE cnt.CompanyID = cust.CompanyID AND cnt.ContactID = cust.DefBillContactID);

UPDATE cust SET cust.DefBillAddressID = ba.DefAddressID
	FROM Customer AS cust
	JOIN BAccount AS ba ON ba.CompanyID = cust.CompanyID AND ba.BAccountID = cust.BAccountID
	WHERE cust.DeletedDatabaseRecord = 0 AND cust.DeletedDatabaseRecord = 0 AND cust.DefBillAddressID IS NOT NULL AND NOT EXISTS(SELECT * FROM Address AS addr WHERE addr.CompanyID = cust.CompanyID AND addr.AddressID = cust.DefBillAddressID);

UPDATE vend SET vend.DefRemitContactID = ba.DefContactID
	FROM Vendor AS vend
	JOIN BAccount AS ba ON ba.CompanyID = vend.CompanyID AND ba.BAccountID = vend.BAccountID
	WHERE vend.DeletedDatabaseRecord = 0 AND vend.DefRemitContactID IS NOT NULL AND NOT EXISTS(SELECT * FROM Contact AS cnt WHERE cnt.CompanyID = vend.CompanyID AND cnt.ContactID = vend.DefRemitContactID);

UPDATE vend SET vend.DefRemitAddressID = ba.DefAddressID
	FROM Vendor AS vend
	JOIN BAccount AS ba ON ba.CompanyID = vend.CompanyID AND ba.BAccountID = vend.BAccountID
	WHERE vend.DeletedDatabaseRecord = 0 AND vend.DefRemitAddressID IS NOT NULL AND NOT EXISTS(SELECT * FROM Address AS addr WHERE addr.CompanyID = vend.CompanyID AND addr.AddressID = vend.DefRemitAddressID);

UPDATE vend SET vend.DefPOAddressID = ba.DefAddressID
	FROM Vendor AS vend
	JOIN BAccount AS ba ON ba.CompanyID = vend.CompanyID AND ba.BAccountID = vend.BAccountID
	WHERE vend.DeletedDatabaseRecord = 0 AND vend.DefPOAddressID IS NOT NULL AND NOT EXISTS(SELECT * FROM Address AS addr WHERE addr.CompanyID = vend.CompanyID AND addr.AddressID = vend.DefPOAddressID);
GO

--Insert missed Addresses for BAccount
--[mysql: Native]
--[mssql: Skip]
--[OldHash(Hash = 2f14324d3fe676eb82fc30bc33a85d565953d942094e44716a321d199324db8f)]
--[OldHash(Hash = d0b98cb1ff7d8e6da9db660342429c3e49bcf340d820904779cca7be8b7ae063)]
--[OldHash(Hash = 54842d0212941e77188b47195aac36b742f27aceb6c65dc6b8cf734219bdf1fb)]
--[OldHash(Hash = dd00a5bb04d52890b1ac656b23b0fe45d524441406d7f5a0442d35a8839a8b5a)]
--[MinVersion(Hash = ce30010096e16a0d80cadeb68b339974f7f1ab3194503e0de5fd4c2e5ca7f3ae)]
UPDATE Contact 
	JOIN BAccount ON BAccount.CompanyID = Contact.CompanyID AND BAccount.DefContactID = Contact.ContactID
    SET Contact.DeletedDatabaseRecord = 0
	WHERE BAccount.DeletedDatabaseRecord = 0 AND Contact.DeletedDatabaseRecord = 1;

INSERT INTO Contact (
	CompanyID, ContactID, BAccountID, ContactType, FullName, DisplayName, IsActive, RevisionID, NoteID,
	FaxType, Phone1Type, Phone2Type, Phone3Type, Method, MajorStatus, DuplicateStatus, IsConvertable, Synchronize,
	GrammValidationDateTime, CreatedByID, CreatedByScreenID, CreatedDateTime, LastModifiedByID, LastModifiedByScreenID, LastModifiedDateTime)
SELECT
	CompanyID, DefContactID, BAccountID, 'AP', AcctName, '', 1, 1, UUID(),
	'BF', 'B1', 'C', 'H1', 'A', -2, 'NV', 0, 1,
	'19000101', CreatedByID, CreatedByScreenID, CreatedDateTime, LastModifiedByID, LastModifiedByScreenID, LastModifiedDateTime
FROM BAccount WHERE DeletedDatabaseRecord = 0 AND DefContactID IS NOT NULL AND
	NOT EXISTS(SELECT * FROM Contact WHERE Contact.CompanyID = BAccount.CompanyID AND Contact.ContactID = BAccount.DefContactID);

INSERT INTO Address (
	CompanyID, AddressID, BAccountID, AddressType,
	CountryID,
	IsValidated, RevisionID, NoteID,
	CreatedByID, CreatedByScreenID, CreatedDateTime, LastModifiedByID, LastModifiedByScreenID, LastModifiedDateTime)
SELECT 
	BAccount.CompanyID, BAccount.DefAddressID, BAccount.BAccountID, 'BS',
	(SELECT CountryID -- get most common country
		FROM  (SELECT CompanyID, CountryID FROM Address GROUP BY Companyid, CountryID) T
		WHERE T.CompanyID = CompanyID
		GROUP BY CountryID
		ORDER BY Count(*) DESC
        LIMIT 1),
	0, 1, UUID(),
	CreatedByID, CreatedByScreenID, CreatedDateTime, NoteID, LastModifiedByScreenID, LastModifiedDateTime
FROM BAccount 
INNER JOIN (
	SELECT CompanyID, DefAddressID, Min(BAccountID) as BAccountID
	FROM BAccount
	WHERE DeletedDatabaseRecord = 0 AND DefContactID IS NOT NULL AND
	NOT EXISTS(SELECT * FROM Address WHERE Address.CompanyID = BAccount.CompanyID AND Address.AddressID = BAccount.DefAddressID)
	GROUP BY CompanyID, DefAddressID
) ba ON  BAccount.CompanyID=ba.CompanyID AND BAccount.BAccountID=ba.BAccountID;

INSERT INTO Location (
	CompanyID, BAccountID, LocationID, LocationCD, Descr, DefAddressID, DefContactID, IsActive, LocType, NoteID,
	CTaxCalcMode, VTaxCalcMode, VPaymentByType, VRcptQtyMax, VRcptQtyAction, VRcptQtyThreshold, VPrintOrder, VEmailOrder, CShipComplete, VPaymentLeadTime, VSeparateCheck, VPrepaymentPct,
	VPaymentInfoLocationID, VAPAccountLocationID, CARAccountLocationID, CAvalaraCustomerUsageType,
	CreatedByID, CreatedByScreenID, CreatedDateTime, LastModifiedByID, LastModifiedByScreenID, LastModifiedDateTime)
SELECT
	CompanyID, BAccountID, DefLocationID, 'MAIN', 'Primary Location', DefAddressID, DefContactID, 1, 'VE', UUID(),
	'T', 'T', 0, 100, 'W', 100, 0, 0, 'L', 0, 0, 100,
	DefLocationID, DefLocationID, DefLocationID, '0',
	CreatedByID, CreatedByScreenID, CreatedDateTime, LastModifiedByID, LastModifiedByScreenID, LastModifiedDateTime
FROM BAccount WHERE DeletedDatabaseRecord = 0 AND DefContactID IS NOT NULL AND DefLocationID IS NOT NULL AND
	NOT EXISTS(SELECT * FROM Location WHERE Location.CompanyID = BAccount.CompanyID AND Location.LocationID = BAccount.DefLocationID) AND
	NOT EXISTS(SELECT * FROM Location WHERE Location.CompanyID = BAccount.CompanyID AND Location.BAccountID = BAccount.BAccountID AND Location.LocationCD = 'MAIN');

UPDATE Location AS l
	LEFT JOIN BAccount AS ba ON ba.CompanyID = l.CompanyID AND ba.BAccountID = l.BAccountID
SET l.DefContactID = ba.DefContactID
	WHERE l.DefContactID IS NOT NULL AND NOT EXISTS(SELECT * FROM Contact AS c WHERE c.CompanyID = l.CompanyID AND c.ContactID = l.DefContactID);

UPDATE Location AS l
	LEFT JOIN BAccount AS ba ON ba.CompanyID = l.CompanyID AND ba.BAccountID = l.BAccountID
SET l.DefAddressID = ba.DefAddressID
	WHERE l.DefAddressID IS NOT NULL AND NOT EXISTS(SELECT * FROM Address AS a WHERE a.CompanyID = l.CompanyID AND a.AddressID = l.DefAddressID);

UPDATE Location AS l
	LEFT JOIN BAccount AS ba ON ba.CompanyID = l.CompanyID AND ba.BAccountID = l.BAccountID
SET l.VRemitContactID = ba.DefContactID
	WHERE l.VRemitContactID IS NOT NULL AND NOT EXISTS(SELECT * FROM Contact AS c WHERE c.CompanyID = l.CompanyID AND c.ContactID = l.VRemitContactID);

UPDATE Location AS l
	LEFT JOIN BAccount AS ba ON ba.CompanyID = l.CompanyID AND ba.BAccountID = l.BAccountID
SET l.VRemitAddressID = ba.DefAddressID
	WHERE l.VRemitAddressID IS NOT NULL AND NOT EXISTS(SELECT * FROM Address AS a WHERE a.CompanyID = l.CompanyID AND a.AddressID = l.VRemitAddressID);

UPDATE Customer AS cust
	JOIN BAccount AS ba ON ba.CompanyID = cust.CompanyID AND ba.BAccountID = cust.BAccountID
SET cust.DefBillContactID = ba.DefContactID
	WHERE cust.DeletedDatabaseRecord = 0 AND cust.DefBillContactID IS NOT NULL AND NOT EXISTS(SELECT * FROM Contact AS cnt WHERE cnt.CompanyID = cust.CompanyID AND cnt.ContactID = cust.DefBillContactID);

UPDATE Customer AS cust
	JOIN BAccount AS ba ON ba.CompanyID = cust.CompanyID AND ba.BAccountID = cust.BAccountID
SET cust.DefBillAddressID = ba.DefAddressID
	WHERE cust.DeletedDatabaseRecord = 0 AND cust.DeletedDatabaseRecord = 0 AND cust.DefBillAddressID IS NOT NULL AND NOT EXISTS(SELECT * FROM Address AS addr WHERE addr.CompanyID = cust.CompanyID AND addr.AddressID = cust.DefBillAddressID);

UPDATE Vendor AS vend
	JOIN BAccount AS ba ON ba.CompanyID = vend.CompanyID AND ba.BAccountID = vend.BAccountID
SET vend.DefRemitContactID = ba.DefContactID
	WHERE vend.DeletedDatabaseRecord = 0 AND vend.DefRemitContactID IS NOT NULL AND NOT EXISTS(SELECT * FROM Contact AS cnt WHERE cnt.CompanyID = vend.CompanyID AND cnt.ContactID = vend.DefRemitContactID);

UPDATE Vendor AS vend
	JOIN BAccount AS ba ON ba.CompanyID = vend.CompanyID AND ba.BAccountID = vend.BAccountID
SET vend.DefRemitAddressID = ba.DefAddressID
	WHERE vend.DeletedDatabaseRecord = 0 AND vend.DefRemitAddressID IS NOT NULL AND NOT EXISTS(SELECT * FROM Address AS addr WHERE addr.CompanyID = vend.CompanyID AND addr.AddressID = vend.DefRemitAddressID);

UPDATE Vendor AS vend
	JOIN BAccount AS ba ON ba.CompanyID = vend.CompanyID AND ba.BAccountID = vend.BAccountID
SET vend.DefPOAddressID = ba.DefAddressID
	WHERE vend.DeletedDatabaseRecord = 0 AND vend.DefPOAddressID IS NOT NULL AND NOT EXISTS(SELECT * FROM Address AS addr WHERE addr.CompanyID = vend.CompanyID AND addr.AddressID = vend.DefPOAddressID);
GO

--[MinVersion(Hash = 6e3cc060fdc59831605794558f71292e29f89396b9f9259e2687a543262482e6)]
UPDATE APSetup SET DataInconsistencyHandlingMode = 'L'  WHERE DataInconsistencyHandlingMode='P'
UPDATE ARSetup SET DataInconsistencyHandlingMode = 'L'  WHERE DataInconsistencyHandlingMode='P'
GO

--[MinVersion(Hash = 31deba23cc183049901755885608581ce812a01f99ee5d7384797ade382dbeaa)]
UPDATE Branch
SET BranchCD = 
	CASE 
		WHEN BranchID = ID THEN B.BranchCD 
		ELSE CONCAT(RTRIM(B.BranchCD), '@', BranchID) 
	END
FROM
	Branch B
	INNER JOIN 
			(SELECT 
				CompanyID, 
				BranchCD, 
				COUNT(1) AS C, 
				MIN(CASE 
						WHEN DeletedDatabaseRecord = 0 THEN BranchID 
						ELSE NULL 
					END) AS ID
			FROM Branch
			GROUP BY CompanyID, BranchCD
			HAVING COUNT(1) > 1) doubles
		ON doubles.CompanyID = B.CompanyID
		AND doubles.BranchCD = B.BranchCD
GO

--[MinVersion(Hash = 4c29808693961738f50e7e262908113ce7339e08ce82c8926e3f4c0343adb2fe)]
UPDATE Organization
SET OrganizationCD = 
	CASE 
		WHEN OrganizationID = ID THEN B.OrganizationCD 
		ELSE CONCAT(RTRIM(B.OrganizationCD), '@', OrganizationID) 
	END
FROM
	Organization B
	INNER JOIN 
			(SELECT 
				CompanyID, 
				OrganizationCD, 
				COUNT(1) AS C, 
				MIN(CASE 
						WHEN DeletedDatabaseRecord = 0 THEN OrganizationID 
						ELSE NULL 
					END) AS ID
			FROM Organization
			GROUP BY CompanyID, OrganizationCD
			HAVING COUNT(1) > 1) doubles
		ON doubles.CompanyID = B.CompanyID
		AND doubles.OrganizationCD = B.OrganizationCD
GO

--[MinVersion(Hash = a0838631564a7945c02df12e4a2cb7fa5efc0103fe72b8537ff79831a133dac3)]
INSERT INTO CurrencyInUse (CompanyID, CuryID)
SELECT 
	CompanyID,
	CuryID
FROM Batch
WHERE Released = 1
GROUP BY CompanyID, CuryID
GO

--[MinVersion(Hash = e1fe02a48cc0dd159345ca3df7c176bc3d663d20b9fc3058e0948acafb76143a)]
UPDATE APRegister SET
	CuryRetainageReleased = CuryRetainageTotal - CuryRetainageUnreleasedAmt,
	RetainageReleased = RetainageTotal - RetainageUnreleasedAmt
GO

--[mysql: Skip]
--[mssql: Native]
--[MinVersion(Hash = 9e6855a81c9f2fefaf48bbca55467d4ce7ccc5c26803be294e0a1562d690168d)]
--[OldHash(Hash = 9e6855a81c9f2fefaf48bbca55467d4ce7ccc5c26803be294e0a1562d690168d)]
UPDATE ARRegister
SET ClosedDate = DATEADD(day,-1,FinPeriod.EndDate)
FROM ARRegister
INNER JOIN Branch
	ON Branch.CompanyID = ARRegister.CompanyID
	AND Branch.BranchID = ARRegister.BranchID
INNER JOIN FinPeriod
	ON FinPeriod.CompanyID = ARRegister.CompanyID
	AND FinPeriod.FinPeriodID = ARRegister.ClosedFinPeriodID
	AND FinPeriod.OrganizationID = Branch.OrganizationID
WHERE ARRegister.ClosedDate IS NULL
GO

--[mysql: Skip]
--[mssql: Native]
--[OldHash(Hash = 9e6855a81c9f2fefaf48bbca55467d4ce7ccc5c26803be294e0a1562d690168d)]
--[MinVersion(Hash = a8a8a33f87accadab81f52efe6f8a44d44ffbf26a9630b247a22709f728efc24)]
UPDATE ARRegister
SET ClosedDate = DATEADD(day,-1,FinPeriod.EndDate)
FROM ARRegister
INNER JOIN Branch
	ON Branch.CompanyID = ARRegister.CompanyID
	AND Branch.BranchID = ARRegister.BranchID
INNER JOIN FinPeriod
	ON FinPeriod.CompanyID = ARRegister.CompanyID
	AND FinPeriod.FinPeriodID = ARRegister.ClosedFinPeriodID
WHERE ARRegister.ClosedDate IS NULL
GO

--[mysql: Native]
--[mssql: Skip]
--[MinVersion(Hash = 244c7cbf35545ad0fbd124a033328121a924ea605d22c29ab6bfcd8db07c0d8d)]
--[OldHash(Hash = 244c7cbf35545ad0fbd124a033328121a924ea605d22c29ab6bfcd8db07c0d8d)]
UPDATE ARRegister
INNER JOIN Branch
	ON Branch.CompanyID = ARRegister.CompanyID
	AND Branch.BranchID = ARRegister.BranchID
INNER JOIN FinPeriod
	ON FinPeriod.CompanyID = ARRegister.CompanyID
	AND FinPeriod.FinPeriodID = ARRegister.ClosedFinPeriodID
	AND FinPeriod.OrganizationID = Branch.OrganizationID
SET ClosedDate = DATE_ADD(FinPeriod.EndDate, INTERVAL -1 DAY )
WHERE ARRegister.ClosedDate IS NULL
GO

--[mysql: Native]
--[mssql: Skip]
--[OldHash(Hash = 244c7cbf35545ad0fbd124a033328121a924ea605d22c29ab6bfcd8db07c0d8d)]
--[MinVersion(Hash = 0047feac43fc683a50e8e89ceeae9a13bce4837e3058b5b8de018e2c738be7c3)]
UPDATE ARRegister
INNER JOIN Branch
	ON Branch.CompanyID = ARRegister.CompanyID
	AND Branch.BranchID = ARRegister.BranchID
INNER JOIN FinPeriod
	ON FinPeriod.CompanyID = ARRegister.CompanyID
	AND FinPeriod.FinPeriodID = ARRegister.ClosedFinPeriodID
SET ClosedDate = DATE_ADD(FinPeriod.EndDate, INTERVAL -1 DAY )
WHERE ARRegister.ClosedDate IS NULL
GO

--[MinVersion(Hash = ddd53ae3236fb263141f25f5a240751f51d5238d08915c603666e5a20bae0563)]
UPDATE Vendor
	SET Vendor.PaymentsByLinesAllowed = 1
FROM Vendor
INNER JOIN FeaturesSet ON FeaturesSet.CompanyID = Vendor.CompanyID
	AND FeaturesSet.PaymentsByLines = 1
WHERE Vendor.TaxAgency <> 1 
	AND Vendor.Vendor1099 <> 1
GO

--[MinVersion(Hash = 89b78bc4e270f0a02154ff4cc3681b74b8f0b8e260ac481a5a9bc6afe48c2a5d)]
UPDATE ARRegister SET
	CuryRetainageReleased = CuryRetainageTotal - CuryRetainageUnreleasedAmt,
	RetainageReleased = RetainageTotal - RetainageUnreleasedAmt
GO

--[mysql: Skip]
--[mssql: Native]
--[MinVersion(Hash = 793c49ac9fad3c41277d1c950f195fe08c801133374fa44d66aa8d81208dc3d6)]
UPDATE FALocationHistory
	SET ClassID = fa.ClassID
FROM FALocationHistory AS l
	JOIN FADetails AS det ON det.CompanyID = l.CompanyID AND det.AssetID = l.AssetID AND det.LocationRevID = l.RevisionID
	JOIN FixedAsset AS fa ON fa.CompanyID = det.CompanyID AND fa.AssetID = det.AssetID
GO

--[mysql: Native]
--[mssql: Skip]
--[MinVersion(Hash = 8810ecb83e1018fb002eea786291f91a29076bc22fdd246c5cc48c7ba99b371c)]
UPDATE FALocationHistory AS l
	JOIN FADetails AS det ON det.CompanyID = l.CompanyID AND det.AssetID = l.AssetID AND det.LocationRevID = l.RevisionID
	JOIN FixedAsset AS fa ON fa.CompanyID = det.CompanyID AND fa.AssetID = det.AssetID
SET l.ClassID = fa.ClassID
GO

-- Reset FALocationHistory.RefNbr for non-existing FARegister
--[MinVersion(Hash = e72b5761fca9d059dbfb8ceafe98e17aa79f761933fb840f7c7c5febfd82fc1c)]
UPDATE FALocationHistory SET RefNbr = NULL WHERE LEFT(RefNbr, 4) = '*##@'
GO

--[MinVersion(Hash = e7347db8eb5c4db85f07a08192f0f50e6785ecbd64b432661e8d875d5cdd1f6d)]
INSERT INTO Dimension ([CompanyID],[DimensionID],[Descr],[Length],[Segments],[Internal],[NumberingID],[Validate],[LookupMode],[SpecificModule],[ParentDimensionID],[TableName],[FieldName],[DividerField],[DividerValue],[CreatedByID],[CreatedByScreenID],[CreatedDateTime],[LastModifiedByID],[LastModifiedByScreenID],[LastModifiedDateTime],[CompanyMask])
SELECT a.[CompanyID], [DimensionID]='BRANCH' ,[Descr]='Branch ID'
,a.[Length],a.[Segments],a.[Internal],a.[NumberingID]
,a.[Validate],a.[LookupMode],a.[SpecificModule]
,[ParentDimensionID]='BIZACCT'
,a.[TableName],a.[FieldName],a.[DividerField],a.[DividerValue]
,a.[CreatedByID],a.[CreatedByScreenID],a.[CreatedDateTime],a.[LastModifiedByID],a.[LastModifiedByScreenID],a.[LastModifiedDateTime],a.[CompanyMask]
FROM Dimension a 
	LEFT JOIN Dimension b ON a.CompanyID = b.CompanyID AND b.DimensionID ='BRANCH'
WHERE a.CompanyID>1 AND a.DimensionID = 'BIZACCT' AND b.DimensionID IS NULL
GO

-- Copy currency rounding accounts from GL Preferences to Tax Preferences
--[MinVersion(Hash = 09c7848e1e031cbb8469359ef7b938e5f1d1fbb5102f0b0a7e4253cf2752718b)]
UPDATE tx SET
	TaxRoundingGainAcctID = ISNULL(TaxRoundingGainAcctID, curr.RoundingGainAcctID),
	TaxRoundingGainSubID  = ISNULL(TaxRoundingGainSubID, curr.RoundingGainSubID),
	TaxRoundingLossAcctID = ISNULL(TaxRoundingLossAcctID, curr.RoundingLossAcctID),
	TaxRoundingLossSubID  = ISNULL(TaxRoundingLossSubID, curr.RoundingLossAcctID)
	FROM TXSetup AS tx
	JOIN Company AS comp ON comp.CompanyID = tx.CompanyID
	JOIN Currency AS curr ON curr.CompanyID = comp.CompanyID AND curr.CuryID = comp.BaseCuryID
GO

--[MinVersion(Hash = f034ca9094df704555cd011494924a08b2ad53bab484c2d4dd88cee916148b69)]
INSERT INTO TXSetup (CompanyID, TaxRoundingGainAcctID, TaxRoundingGainSubID, TaxRoundingLossAcctID, TaxRoundingLossSubID, CreatedByID, CreatedByScreenID, CreatedDateTime, LastModifiedByID, LastModifiedByScreenID, LastModifiedDateTime)
	SELECT curr.CompanyID, curr.RoundingGainAcctID, curr.RoundingGainSubID, curr.RoundingLossAcctID, curr.RoundingLossSubID, curr.CreatedByID, curr.CreatedByScreenID, curr.CreatedDateTime, curr.LastModifiedByID, curr.LastModifiedByScreenID, curr.LastModifiedDateTime
	FROM Company AS comp
	JOIN Currency AS curr ON curr.CompanyID = comp.CompanyID AND curr.CuryID = comp.BaseCuryID
	LEFT JOIN TXSetup AS tx ON tx.CompanyID = comp.CompanyID
	WHERE tx.CompanyID IS NULL
GO

--[MinVersion(Hash = 9f05fbcb8d14e42ca40e12b55fe1f906fbdd509b540b0aa68470325b6b0dc7af)]
UPDATE TaxTran SET
	CuryTaxAmtSumm = CuryTaxAmt,
	TaxAmtSumm = TaxAmt,
	CuryRetainedTaxAmtSumm = CuryRetainedTaxAmt,
	RetainedTaxAmtSumm = RetainedTaxAmt
GO

--[MinVersion(Hash = be59d0b84079768ed661e04669399f481d6283f1356e046a9e0383fc0c1b1129)]
INSERT INTO Dimension ([CompanyID],[DimensionID],[Descr],[Length],[Segments],[Internal],[NumberingID],[Validate],[LookupMode],[SpecificModule],[ParentDimensionID],[TableName],[FieldName],[DividerField],[DividerValue],[CreatedByID],[CreatedByScreenID],[CreatedDateTime],[LastModifiedByID],[LastModifiedByScreenID],[LastModifiedDateTime],[CompanyMask])
SELECT a.[CompanyID], [DimensionID]='COMPANY',[Descr]='Company ID'
,a.[Length],a.[Segments],a.[Internal],a.[NumberingID]
,a.[Validate],a.[LookupMode],a.[SpecificModule]
,[ParentDimensionID]='BIZACCT'
,a.[TableName],a.[FieldName],a.[DividerField],a.[DividerValue]
,a.[CreatedByID],a.[CreatedByScreenID],a.[CreatedDateTime],a.[LastModifiedByID],a.[LastModifiedByScreenID],a.[LastModifiedDateTime],a.[CompanyMask]
FROM Dimension a 
	LEFT JOIN Dimension b ON a.CompanyID = b.CompanyID AND b.DimensionID ='COMPANY'
WHERE a.CompanyID>1 AND a.DimensionID = 'BIZACCT' AND b.DimensionID IS NULL
GO

-- AC-142289:  Correct IsFinancial flag for base currencies of all tenants in the database
--[MinVersion(Hash = 791c3814bcd8b291a2d19d378875e869f2ddf019c6471bfe209dde0c58b7659c)]
UPDATE CurrencyList
SET CurrencyList.IsFinancial = 1
WHERE CurrencyList.IsFinancial = 0 
AND EXISTS 
(
	SELECT CompanyID FROM  Company WHERE Company.BaseCuryID = CurrencyList.CuryID AND Company.CompanyID = CurrencyList.CompanyID
)
GO

-- set CuryInfoID into DRScheduleDetail from related ARTran
--[MinVersion(Hash = e98500000e2f28e145cd0cb60be5bf807f4342c5b47c2cb45d3459dfd7f80e48)]
UPDATE DRScheduleDetail
SET DRScheduleDetail.CuryInfoID = ARTran.CuryInfoID
FROM DRScheduleDetail
INNER JOIN DRSchedule
	ON DRSchedule.CompanyID = DRScheduleDetail.CompanyID
	AND DRSchedule.ScheduleID = DRScheduleDetail.ScheduleID
INNER JOIN  ARTran
	ON DRSchedule.CompanyID = ARTran.CompanyID
	AND DRSchedule.DocType = ARTran.TranType
	AND DRSchedule.RefNbr = ARTran.RefNbr
	AND DRSchedule.LineNbr = ARTran.LineNbr
	AND DRSchedule.Module = 'AR'
INNER JOIN  CurrencyInfo
	ON ARTran.CompanyID = CurrencyInfo.CompanyID
	AND ARTran.CuryInfoID = CurrencyInfo.CuryInfoID
WHERE (DRScheduleDetail.CuryInfoID = 0
		OR DRScheduleDetail.CuryInfoID IS NULL)
	AND (
		EXISTS	-- has denominated AccountID account
			(
				SELECT 1 FROM Account 
				WHERE DRScheduleDetail.CompanyID = Account.CompanyID
					AND DRScheduleDetail.AccountID = Account.AccountID
					AND Account.CuryID IS NOT NULL
			)
		OR EXISTS	-- has denominated DefAcctID account
			(
				SELECT 1 FROM Account 
				WHERE DRScheduleDetail.CompanyID = Account.CompanyID
					AND DRScheduleDetail.DefAcctID = Account.AccountID
					AND Account.CuryID IS NOT NULL
			)
		)
GO

-- set CuryInfoID into DRScheduleDetail from related APTran
--[MinVersion(Hash = 79d0d930706d3580fe1bba77bb476a4f8cb277a6c5066ad0908ffc518721206f)]
UPDATE DRScheduleDetail
SET DRScheduleDetail.CuryInfoID = APTran.CuryInfoID
FROM DRScheduleDetail
INNER JOIN DRSchedule
	ON DRSchedule.CompanyID = DRScheduleDetail.CompanyID
	AND DRSchedule.ScheduleID = DRScheduleDetail.ScheduleID
INNER JOIN  APTran
	ON DRSchedule.CompanyID = APTran.CompanyID
	AND DRSchedule.DocType = APTran.TranType
	AND DRSchedule.RefNbr = APTran.RefNbr
	AND DRSchedule.LineNbr = APTran.LineNbr
	AND DRSchedule.Module = 'AP'
INNER JOIN  CurrencyInfo
	ON APTran.CompanyID = CurrencyInfo.CompanyID
	AND APTran.CuryInfoID = CurrencyInfo.CuryInfoID
WHERE (DRScheduleDetail.CuryInfoID = 0
		OR DRScheduleDetail.CuryInfoID IS NULL)
	AND (
		EXISTS	-- has denominated AccountID account
			(
				SELECT 1 FROM Account 
				WHERE DRScheduleDetail.CompanyID = Account.CompanyID
					AND DRScheduleDetail.AccountID = Account.AccountID
					AND Account.CuryID IS NOT NULL
			)
		OR EXISTS	-- has denominated DefAcctID account
			(
				SELECT 1 FROM Account 
				WHERE DRScheduleDetail.CompanyID = Account.CompanyID
					AND DRScheduleDetail.DefAcctID = Account.AccountID
					AND Account.CuryID IS NOT NULL
			)
		)
GO

-- create a new CuryInfoID into DRScheduleDetail when a schedule without link to documents
--[MinVersion(Hash = a4ba3977cc4aab1231db17ad703b267e9d98aaf3014bc453696175074c90d53f)]
INSERT INTO CurrencyInfo (
	CompanyID
	,CuryID
	,CuryRateTypeID
	,CuryEffDate
	,CuryMultDiv
	,CuryRate
	,BaseCuryID
	,RecipRate
	,BaseCalc
	)
SELECT
	Company.CompanyID
	,Company.BaseCuryID
	,NULL
	,'1900-01-01'
	,'M'
	,1
	,Company.BaseCuryID
	,1
	,1
FROM Company
WHERE Company.BaseCuryID IS NOT NULL;

-- update AR Schedule details without link to documents (RefNbr is NULL)
UPDATE DRScheduleDetail
SET DRScheduleDetail.CuryInfoID = (SELECT MAX(CurrencyInfo.CuryInfoID) FROM CurrencyInfo WHERE CurrencyInfo.CompanyID = Company.CompanyID)
FROM DRScheduleDetail
INNER JOIN Company 
	ON Company.CompanyID = DRScheduleDetail.CompanyID
INNER JOIN DRSchedule
	ON DRSchedule.CompanyID = DRScheduleDetail.CompanyID
	AND DRSchedule.ScheduleID = DRScheduleDetail.ScheduleID
LEFT JOIN  ARTran
	ON DRSchedule.CompanyID = ARTran.CompanyID
	AND DRSchedule.DocType = ARTran.TranType
	AND DRSchedule.RefNbr = ARTran.RefNbr
	AND DRSchedule.LineNbr = ARTran.LineNbr
WHERE Company.BaseCuryID IS NOT NULL
	AND (DRScheduleDetail.CuryInfoID = 0
		OR DRScheduleDetail.CuryInfoID IS NULL)
	AND DRSchedule.Module = 'AR'
	AND ARTran.CompanyID is null
	AND (
		EXISTS	-- has denominated AccountID account
			(
				SELECT 1 FROM Account 
				WHERE DRScheduleDetail.CompanyID = Account.CompanyID
					AND DRScheduleDetail.AccountID = Account.AccountID
					AND Account.CuryID IS NOT NULL
			)
		OR EXISTS	-- has denominated DefAcctID account
			(
				SELECT 1 FROM Account 
				WHERE DRScheduleDetail.CompanyID = Account.CompanyID
					AND DRScheduleDetail.DefAcctID = Account.AccountID
					AND Account.CuryID IS NOT NULL
			)
		);

-- update AP Schedule details without link to documents (RefNbr is NULL)
UPDATE DRScheduleDetail
SET DRScheduleDetail.CuryInfoID = (SELECT MAX(CurrencyInfo.CuryInfoID) FROM CurrencyInfo WHERE CurrencyInfo.CompanyID = Company.CompanyID)
FROM DRScheduleDetail
INNER JOIN Company 
	ON Company.CompanyID = DRScheduleDetail.CompanyID
INNER JOIN DRSchedule
	ON DRSchedule.CompanyID = DRScheduleDetail.CompanyID
	AND DRSchedule.ScheduleID = DRScheduleDetail.ScheduleID
LEFT JOIN  APTran
	ON DRSchedule.CompanyID = APTran.CompanyID
	AND DRSchedule.DocType = APTran.TranType
	AND DRSchedule.RefNbr = APTran.RefNbr
	AND DRSchedule.LineNbr = APTran.LineNbr
WHERE Company.BaseCuryID IS NOT NULL
	AND (DRScheduleDetail.CuryInfoID = 0
		OR DRScheduleDetail.CuryInfoID IS NULL)
	AND DRSchedule.Module = 'AP'
	AND APTran.CompanyID is null
	AND (
		EXISTS	-- has denominated AccountID account
			(
				SELECT 1 FROM Account 
				WHERE DRScheduleDetail.CompanyID = Account.CompanyID
					AND DRScheduleDetail.AccountID = Account.AccountID
					AND Account.CuryID IS NOT NULL
			)
		OR EXISTS	-- has denominated DefAcctID account
			(
				SELECT 1 FROM Account 
				WHERE DRScheduleDetail.CompanyID = Account.CompanyID
					AND DRScheduleDetail.DefAcctID = Account.AccountID
					AND Account.CuryID IS NOT NULL
			)
		)
GO

--[mysql: Skip]
--[mssql: Native]
--[MinVersion(Hash = c8c4c00e6d71cd74ad98586c7114124757573c220f04ea33a5b3a2737b10c3c3)]
--[OldHash(Hash = c8c4c00e6d71cd74ad98586c7114124757573c220f04ea33a5b3a2737b10c3c3)]
UPDATE APRegister
SET ClosedDate = DATEADD(day, -1, FinPeriod.EndDate)
FROM APRegister
INNER JOIN Branch
	ON Branch.CompanyID = APRegister.CompanyID
	AND Branch.BranchID = APRegister.BranchID
INNER JOIN FinPeriod
	ON FinPeriod.CompanyID = APRegister.CompanyID
	AND FinPeriod.FinPeriodID = APRegister.ClosedFinPeriodID
	AND FinPeriod.OrganizationID = Branch.OrganizationID
WHERE APRegister.ClosedDate IS NULL
GO

--[mysql: Skip]
--[mssql: Native]
--[OldHash(Hash = c8c4c00e6d71cd74ad98586c7114124757573c220f04ea33a5b3a2737b10c3c3)]
--[MinVersion(Hash = b39310991520b9a96bac23d0ebf421f3eaf9a4e60162c6b2bae6c55916b3d116)]
UPDATE APRegister
SET ClosedDate = DATEADD(day, -1, FinPeriod.EndDate)
FROM APRegister
INNER JOIN Branch
	ON Branch.CompanyID = APRegister.CompanyID
	AND Branch.BranchID = APRegister.BranchID
INNER JOIN FinPeriod
	ON FinPeriod.CompanyID = APRegister.CompanyID
	AND FinPeriod.FinPeriodID = APRegister.ClosedFinPeriodID
WHERE APRegister.ClosedDate IS NULL
GO

--[mysql: Native]
--[mssql: Skip]
--[MinVersion(Hash = cad6385062fa82cc63a4394b477caaf63a3ff817c97b8dda3e4c637ce2ececdb)]
--[OldHash(Hash = c8c4c00e6d71cd74ad98586c7114124757573c220f04ea33a5b3a2737b10c3c3)]
UPDATE APRegister
INNER JOIN Branch
	ON Branch.CompanyID = APRegister.CompanyID
	AND Branch.BranchID = APRegister.BranchID
INNER JOIN FinPeriod
	ON FinPeriod.CompanyID = APRegister.CompanyID
	AND FinPeriod.FinPeriodID = APRegister.ClosedFinPeriodID
	AND FinPeriod.OrganizationID = Branch.OrganizationID
SET ClosedDate = DATE_ADD(FinPeriod.EndDate, INTERVAL -1 DAY )
WHERE APRegister.ClosedDate IS NULL
GO

--[mysql: Native]
--[mssql: Skip]
--[OldHash(Hash = cad6385062fa82cc63a4394b477caaf63a3ff817c97b8dda3e4c637ce2ececdb)]
--[MinVersion(Hash = b84f18659a652c54c3405b0026bf5f887649fdb42dc74153f7d930a24237355b)]
UPDATE APRegister
INNER JOIN Branch
	ON Branch.CompanyID = APRegister.CompanyID
	AND Branch.BranchID = APRegister.BranchID
INNER JOIN FinPeriod
	ON FinPeriod.CompanyID = APRegister.CompanyID
	AND FinPeriod.FinPeriodID = APRegister.ClosedFinPeriodID
SET ClosedDate = DATE_ADD(FinPeriod.EndDate, INTERVAL -1 DAY )
WHERE APRegister.ClosedDate IS NULL
GO

--[MinVersion(Hash = a8f1efe2d1e00df1dca4152d395eb56bf52bc397d26b6f4d8b0b2565c3db5228)]
update FADepreciationMethod 
set PercentPerYear = PercentPerYear * 100
where PercentPerYear is not null
	and PercentPerYear <= 1;
GO

--[MinVersion(Hash = c9ee8a1bc30679e3cf03b5bacad480e02bc0070aa8e84cc0ef1c3ca1f70ebfd2)]
UPDATE AP1099Box
SET Descr = 'Nonemployee Compensation (1099-NEC Box 1 starting 2020)'
WHERE BoxNbr = 7

UPDATE AP1099Box
SET Descr = 'Direct Sales (1099-MISC Box 7 starting 2020)'
WHERE BoxNbr = 9

UPDATE AP1099Box
SET Descr = 'Crop Insurance Proceeds (1099-MISC Box 9 starting 2020)'
WHERE BoxNbr = 10

UPDATE AP1099Box
SET Descr = 'Gross Proceeds to Attorney (1099-MISC Box 10 starting 2020)'
WHERE BoxNbr = 14

UPDATE AP1099Box
SET Descr = 'State Tax Withheld (1099-MISC Box 15 starting 2020)'
WHERE BoxNbr = 16

UPDATE AP1099Box
SET Descr = 'State Income (1099-MISC Box 17 starting 2020)'
WHERE BoxNbr = 18

UPDATE AP1099Box
SET Descr = 'Section 409A Deferral (1099-MISC Box 12 starting 2020)'
WHERE BoxNbr = 151

UPDATE AP1099Box
SET Descr = 'Section 409A Income (1099-MISC Box 14 starting 2020)'
WHERE BoxNbr = 152
GO



--[RefreshMetadata()]
GO

--OEM scripts
--[MinVersion(Hash = 4034287911b9752353d27a074b9aeab97d092a15d1ee01bc15006b3b68cecbce)]

GO

--[MinVersion(Hash = 876e8009ee61beeba2924a97009c2c1b6454b36b73bef82ca321c8f55e5dfe0e)]
update CATransfer
set InTranPeriodID = InPeriodID,
  OutTranPeriodID = OutPeriodID
GO

--[MinVersion(Hash = 609ef1695a3e8e3123cccdc5c31ea5c66fb73e63e87e8e1b397bc5fc692c8151)]
update CAAdj
set TranPeriodID = FinPeriodID
GO

--[MinVersion(Hash = c6800128fe2b49a105a58fbd4a204de5c356ab499fcf55e9a83e96b8743afc88)]
update CAExpense
set TranPeriodID = FinPeriodID
GO

--[MinVersion(Hash = 9e067502e4c7b0e0c36daf80fad24ae2a59e399bcaf97d654e67f5bc75a4fbaf)]
update CATran
set TranPeriodID = FinPeriodID
GO

--[MinVersion(Hash = 55ab9ed232f95e7a05ebbeccc1b67419b0d309c5a9fab40b3d680d2dd733c729)]
update CASplit
set CASplit.FinPeriodID = CAAdj.FinPeriodID,
CASplit.TranPeriodID = CAAdj.TranPeriodID
from CASplit
inner join CAAdj 
on CASplit.CompanyID = CAAdj.CompanyID  and CASplit.AdjTranType = CAAdj.AdjTranType and CASplit.AdjRefNbr = CAAdj.AdjRefNbr
GO

--[MinVersion(Hash = df4e714d43a43599497675fc4f6b0b5496e21e3fbdd79c01717c27025149b403)]
update CATransfer
set CATransfer.InPeriodID = FinPeriod.FinPeriodID,
CATransfer.InTranPeriodID = COALESCE(FinPeriod.MasterFinPeriodID, FinPeriod.FinPeriodID)
from CATransfer
	join FinPeriod on CATransfer.CompanyID = FinPeriod.CompanyID 
			and CATransfer.InDate >= FinPeriod.StartDate 
			and CATransfer.InDate < FinPeriod.EndDate
where CATransfer.InPeriodID is null
GO

--[MinVersion(Hash = 8b91d72a05a232fbb33dc3916992f45dd16a7dc52901f9b6a8f3025f9a79ecd7)]
update CATransfer
set CATransfer.OutPeriodID = FinPeriod.FinPeriodID,
CATransfer.OutTranPeriodID = COALESCE(FinPeriod.MasterFinPeriodID, FinPeriod.FinPeriodID)
from CATransfer
	join FinPeriod on CATransfer.CompanyID = FinPeriod.CompanyID 
			and CATransfer.OutDate >= FinPeriod.StartDate 
			and CATransfer.OutDate < FinPeriod.EndDate 
where CATransfer.OutPeriodID is null
GO

--[MinVersion(Hash = 0c069431f749bb6dcc02312ce08b621cf3a9b5fd1cfe2bce4ec88e033bfad60e)]
INSERT INTO CCProcessingCenterDetail(CompanyID, ProcessingCenterID, DetailID, Descr, Value,
	ControlType, ComboValues, IsEncryptionRequired, IsEncrypted, CreatedByID, CreatedByScreenID,CreatedDateTime,
	LastModifiedByID, LastModifiedByScreenID, LastModifiedDateTime) 
SELECT CompanyID,ProcessingCenterID, 'SIGNKEY', 'Your Signature Key', null, 1, null, 1, 1, '00000000-0000-0000-0000-000000000000', 
	CreatedByScreenID, CreatedDateTime, '00000000-0000-0000-0000-000000000000', LastModifiedByScreenID, LastModifiedDateTime
FROM CCProcessingCenter T1
WHERE [ProcessingTypeName] = 'PX.CCProcessing.V2.AuthnetProcessingPlugin'
AND NOT EXISTS (SELECT 1 FROM CCProcessingCenterDetail WHERE DetailID = 'SIGNKEY' AND ProcessingCenterID = T1.ProcessingCenterID)
GO

--[MinVersion(Hash = a43ca4db9942d8b80ae732a6dae909d55486fa40bc9ae326ccc9c5afdcb2ab5a)]
UPDATE Terms SET DayDue01 = DayFrom00, DayFrom00 = 0 WHERE DueType = 'F' and DayFrom00 <> 0
GO

--[mysql: Skip]
--[mssql: Native]
--[MinVersion(Hash = d2e195df82f0a64b71e934e45e6c35a23979ddcf6d36c13dfe12244e9482a34b)]
UPDATE T1 SET T1.PCTranNumber = T2.PCTranNumber 
FROM CCProcTran T1 
INNER JOIN CCProcTran T2 ON T1.CompanyID = T2.CompanyID AND T1.PMInstanceID = T2.PMInstanceID AND T1.RefTranNbr = T2.TranNbr
INNER JOIN CCProcessingCenter T3 ON T1.CompanyID = T3.CompanyID AND T1.ProcessingCenterID = T3.ProcessingCenterID
WHERE T3.ProcessingTypeName IN (N'PX.CCProcessing.V2.AuthnetProcessingPlugin', N'PX.CCProcessing.AuthorizeNetProcessing', N'PX.CCProcessing.AuthorizeNetTokenizedProcessing') 
AND T1.TranStatus = 'APR' AND T1.ProcStatus = 'FIN' AND T1.PCTranNumber = '0' AND COALESCE(T2.PCTranNumber,'0') <> '0'
AND COALESCE(T1.DocType,'') = COALESCE(T2.DocType,'') AND COALESCE(T1.RefNbr,'') = COALESCE(T2.RefNbr,'') 
AND COALESCE(T1.OrigDocType,'') = COALESCE(T2.OrigDocType,'') AND COALESCE(T1.OrigRefNbr,'') = COALESCE(T2.OrigRefNbr,'') 
AND T1.TranType IN ('VDG','PAC')
GO

--[mysql: Skip]
--[mssql: Native]
--[MinVersion(Hash = 5e3d284de6b2244ac8879927bcfc529a77eed0ddea105fb17aeca296d80e7c56)]
IF EXISTS (SELECT * FROM sys.objects WHERE name = 'TempExternalTransaction' AND type = 'U')
DROP TABLE TempExternalTransaction
CREATE TABLE TempExternalTransaction
(
	TransactionId int not null,
	CompanyID int not null,
	TempTranNbr int,
	TempCdtParentNbr int,
	PMInstanceID int not null,
	PCTranNumber nvarchar(50),
	AuthNumber nvarchar(50),
	Amount decimal(19,4),
	ProcessingStatus char(3),
	LastActivityDate datetime,
	Direction char(1),
	Active bit,
	Completed bit,
	ParentTransaction int,
	ExpirationDate datetime,
	OrigDocType char(3),
	OrigRefNbr nvarchar(15),
	DocType char(3),
	RefNbr nvarchar(15),
	CVVVerification char(3)
)
CREATE INDEX "TempExternalTransaction_Index" on "TempExternalTransaction" (
"CompanyID", "TransactionID", "TempTranNbr")  

TRUNCATE TABLE ExternalTransaction
INSERT INTO TempExternalTransaction(TransactionID, TempTranNbr, CompanyID, TempCdtParentNbr, PMInstanceID,
	PCTranNumber,AuthNumber,Amount,ProcessingStatus,LastActivityDate,Direction,Active,
	Completed, ExpirationDate, OrigDocType, OrigRefNbr, DocType, RefNbr, CVVVerification) 
SELECT 
ROW_NUMBER() OVER (Partition by T1.CompanyID ORDER BY T1.TranNbr ASC),
T1.TranNbr, 
T1.CompanyID,
CASE WHEN  COALESCE(T1.CdtRefTranNbr, - 1) <> -1 
THEN 
	(SELECT MAX(TranNbr) FROM CCProcTran WHERE PCTranNumber = 
	(SELECT PCTranNumber FROM CCProcTran WHERE CompanyID = T1.CompanyID AND PMInstanceID = PMInstanceID AND TranNbr = T1.CdtRefTranNbr) 
		AND CompanyID = T1.CompanyID AND PMInstanceID = T1.PMInstanceID)
ELSE 
	NULL 
END AS 'CdtParent',
T1.PMInstanceID AS 'PMInstanceID',
T1.PCTranNumber AS 'PCTranNumber',
T1.AuthNumber AS 'AuthNumber',
T2.Amount AS 'Amount',
COALESCE (
	CASE WHEN COALESCE(T5.TranStatus,'') = 'ERR' 
	THEN 
		CASE T5.TranType 
		WHEN 'AUT' THEN 'AUF' WHEN 'AAC' THEN 'CAF' 
		WHEN 'PAC' THEN 'CAF' WHEN 'CAP' THEN 'CAF' 
		WHEN 'VDG' THEN 'VDF' WHEN 'CDT' THEN 'CDF' 
		END 
	WHEN COALESCE(T2.TranStatus,'') = 'ERR' 
	THEN 
		CASE T2.TranType 
		WHEN 'AUT' THEN 'AUF'  WHEN 'AAC' THEN 'CAF' 
		WHEN 'PAC' THEN 'CAF' WHEN 'CAP' THEN 'CAF' 
		WHEN 'VDG' THEN 'VDF' WHEN 'CDT' THEN 'CDF' 
		END
	WHEN COALESCE(T2.TranStatus,'') = 'HFR' 
	THEN 
		CASE T2.TranType 
		WHEN 'AUT' THEN 'AUH' WHEN 'AAC' THEN 'CAH'
		WHEN 'PAC' THEN 'CAH' WHEN 'CAP' THEN 'CAH' 
		WHEN 'VDG' THEN 'VDH' WHEN 'CDT' THEN 'CDH' 
		END
	WHEN COALESCE(T2.TranStatus,'') = 'APR' 
	THEN 
		CASE T2.TranType 
		WHEN 'AUT' THEN 'AUS' WHEN 'AAC' THEN 'CAS' 
		WHEN 'PAC' THEN 'CAS' WHEN 'CAP' THEN 'CAS' 
		WHEN 'VDG' THEN 'VDS' WHEN 'CDT' THEN 'CDS' 
		END
	WHEN COALESCE(T2.TranStatus,'') = 'DEC' 
	THEN 
		CASE T2.TranType 
		WHEN 'AUT' THEN 'AUD' WHEN 'AAC' THEN 'CAD' 
		WHEN 'PAC' THEN 'CAD' WHEN 'CAP' THEN 'CAD' 
		WHEN 'VDG' THEN 'VDD' WHEN 'CDT' THEN 'CDD' 
		END
END, 'UKN') AS 'ProcessingStatus',
CASE WHEN COALESCE(T5.EndTime,'19000101') > COALESCE(T2.EndTime,'19000101') THEN T5.EndTime ELSE T2.EndTime END AS 'LastActivityDate',
CASE WHEN T1.DirectionInt > 0 THEN 'C' ELSE 'D' END AS 'Direction',
CASE WHEN T2.TranStatus = 'ERR' OR T2.ProcStatus = 'OPN' OR T2.TranType = 'VDG' OR T2.TranStatus  = 'DEC' THEN 0 ELSE 1 END AS 'Active',
CASE WHEN T2.TranStatus = 'APR' AND (T2.TranType = 'CDT' OR T2.TranType = 'AAC' OR T2.TranType = 'PAC' OR T2.TranType = 'CAP') THEN 1 ELSE 0 END AS 'Completed',
T1.ExpirationDate AS 'ExpirationDate',
T1.OrigDocType AS 'OrigDocType',
T1.OrigRefNbr AS 'OrigRefNbr',
T1.DocType AS 'DocType',
T1.RefNbr AS 'RefNbr',
T2.CVVVerificationStatus as 'CVVVerification'
FROM 
(
	(
		SELECT CompanyID,PMInstanceID,DocType,RefNbr,OrigDocType,OrigRefNbr,PCTranNumber,MAX(TranNbr) AS TranNbr, 
		MAX(AuthNumber) AS AuthNumber, MAX(ExpirationDate) AS ExpirationDate,SUM(DirectionInt) AS DirectionInt, MAX(CdtRefTranNbr) AS CdtRefTranNbr
		FROM 
		( 
			SELECT CompanyID,DocType, RefNbr, OrigDocType, OrigRefNbr, PCTranNumber, Amount, TranNbr, AuthNumber, ExpirationDate,
				CASE WHEN PMInstanceID = -1 
				THEN (SELECT TOP 1 T3.PMInstanceID FROM CCProcTran T3 WHERE T3.CompanyID = T2.CompanyID AND T3.DocType = T2.DocType 
					AND T3.RefNbr = T2.RefNbr AND T3.PCTranNumber = T2.PCTranNumber ORDER BY T3.TranNbr desc) 
				ELSE PMInstanceID 
				END AS PMInstanceID, 
				CASE WHEN TranType = 'CDT' THEN 1 ELSE 0 END AS DirectionInt, 
				CASE WHEN TranType = 'CDT' THEN RefTranNbr ELSE NULL END AS CdtRefTranNbr 
			FROM CCProcTran T2
			WHERE COALESCE(PCTranNumber,'') <> '' AND TranStatus <> 'ERR'
		) T3
		GROUP BY T3.CompanyID, T3.PMInstanceID, T3.DocType, T3.RefNbr, T3.OrigDocType, T3.OrigRefNbr, T3.PCTranNumber
		UNION
		SELECT CompanyID, PMInstanceID, DocType, RefNbr,
		OrigDocType, OrigRefNbr, PCTranNumber, TranNbr, AuthNumber AS AuthNumber, ExpirationDate as ExpirationDate,
		CASE WHEN TranType = 'CDT' THEN 1 ELSE 0 END as DirectionInt, CASE WHEN TranType = 'CDT' 
		THEN RefTranNbr ELSE Null END AS CdtRefTranNbr 
		FROM CCProcTran
		WHERE (COALESCE(PCTranNumber,'0') = '0' OR PCTranNumber = '' OR TranStatus = 'ERR') AND NOT ( COALESCE(RefTranNbr, -1) <> -1 AND TranType <> 'CDT' ) 
	) T1 
	INNER JOIN 
	(
		SELECT * FROM CCProcTran 
	) T2 ON T1.CompanyID = T2.CompanyID AND T1.TranNbr = T2.TranNbr 
	LEFT JOIN
	(
		SELECT T3.CompanyID, T3.PMInstanceID, T4.TranNbr, T3.MaxTranNbr, RefTranNbr, T4.TranType, T4.ProcStatus, T4.TranStatus, T4.EndTime 
		FROM 
		(
			SELECT MAX(TranNbr) AS MaxTranNbr, RefTranNbr, CompanyID, PMInstanceID 
			FROM CCProcTran
			WHERE COALESCE(RefTranNbr, -1) <> -1 AND TranType <> 'CDT' AND TranStatus = 'ERR'
			GROUP BY CompanyID, PMInstanceID, RefTranNbr
		) T3 
		INNER JOIN
		(
			SELECT TranNbr, CompanyID, PMInstanceID, EndTime, TranType, ProcStatus, TranStatus 
			FROM CCProcTran
		) T4 ON T3.CompanyID = T4.CompanyID  AND T3.PMInstanceID = T4.PMInstanceID AND T3.MaxTranNbr = T4.TranNbr
	) T5 ON T5.CompanyID = T1.CompanyID AND T5.PMInstanceID = T1.PMInstanceID AND T5.RefTranNbr = T1.TranNbr
) 
ORDER BY T1.CompanyID, T1.TranNbr  
  
UPDATE T1 SET ParentTransaction = T2.TransactionId 
FROM TempExternalTransaction T1 
LEFT JOIN TempExternalTransaction T2 ON T1.CompanyID = T2.CompanyID AND T1.PMInstanceID = T2.PMInstanceID AND T1.TempCdtParentNbr = T2.TempTranNbr 
WHERE COALESCE(T1.TempCdtParentNbr, -1) <> -1

UPDATE T2 SET Active = 0 FROM TempExternalTransaction T1 INNER JOIN TempExternalTransaction T2 ON T1.TempCdtParentNbr = T2.TempTranNbr AND T1.CompanyID = T2.CompanyID
WHERE T1.Active = 1 AND COALESCE(T1.RefNbr,'') = COALESCE(T2.RefNbr,'') AND COALESCE(T1.DocType,'') = COALESCE(T2.DocType,'') 
AND COALESCE(T1.OrigRefNbr,'') = COALESCE(T2.OrigRefNbr,'') AND COALESCE(T1.OrigDocType,'') = COALESCE(T2.OrigDocType,'') 

UPDATE CCProcTran SET TransactionID = 0

UPDATE CCProcTran SET CCProcTran.TransactionID = TempExternalTransaction.TransactionId 
FROM CCProcTran INNER JOIN TempExternalTransaction 
ON CCProcTran.CompanyID = TempExternalTransaction.CompanyID AND CCProcTran.PMInstanceID = TempExternalTransaction.PMInstanceID AND CCProcTran.TranNbr = TempExternalTransaction.TempTranNbr

UPDATE T3 SET T3.TransactionID = T4.TransactionID  
FROM 
(
	SELECT T1.RefTranNbr,T1.TranNbr, T1.TransactionId,  COALESCE(T2.PCTranNumber, T1.PCTranNumber) as PCTranNumber, 
	T1.CompanyID, T1.Doctype, T1.RefNbr, T1.OrigDocType, T1.OrigRefNbr, 
	CASE WHEN T1.PMInstanceID = -1 THEN (SELECT TOP 1 T3.PMInstanceID FROM CCProcTran T3 WHERE T3.CompanyID = T1.CompanyID AND T3.DocType = T1.DocType 
	AND T3.RefNbr = T1.RefNbr AND T1.PCTranNumber = T3.PCTranNumber ORDER BY T3.TranNbr desc) 
	ELSE T1.PMInstanceID END AS PMInstanceID
	FROM CCProcTran T1  
	LEFT JOIN CCProcTran T2 ON T1.CompanyID = T2.CompanyID AND T1.RefTranNbr = T2.TranNbr
	WHERE T1.TranType <> 'CDT' 
) T3
INNER JOIN TempExternalTransaction T4 ON T4.CompanyID = T3.CompanyID AND T4.PMInstanceID = T3.PMInstanceID AND T3.PCTranNumber = T4.PCTranNumber 
AND COALESCE(T3.DocType,'') =  COALESCE(T4.DocType,'') AND COALESCE(T3.RefNbr,'') = COALESCE(T4.RefNbr,'')
AND COALESCE(T3.OrigDocType,'') = COALESCE(T4.OrigDocType,'') AND COALESCE(T3.OrigRefNbr,'') = COALESCE(T4.OrigRefNbr,'')
WHERE T3.TransactionID = 0

UPDATE T1 SET T1.TransactionID = T2.TransactionID 
FROM CCProcTran T1 
INNER JOIN CCProcTran T2 ON T1.CompanyID = T2.CompanyID AND T1.PMInstanceID = T2.PMInstanceID
AND COALESCE(T1.DocType,'') =  COALESCE(T2.DocType,'') AND COALESCE(T1.RefNbr,'') = COALESCE(T2.RefNbr,'')
AND COALESCE(T1.OrigDocType,'') = COALESCE(T2.OrigDocType,'') AND COALESCE(T1.OrigRefNbr,'') = COALESCE(T2.OrigRefNbr,'')
AND T1.RefTranNbr = T2.RefTranNbr
WHERE T1.TransactionID = 0 AND T2.TransactionID <> 0

UPDATE T1 SET T1.TransactionID = T2.TransactionID 
FROM CCProcTran T1
INNER JOIN CCProcTran T2 ON T1.CompanyID = T2.CompanyID AND T1.RefTranNbr = T2.TranNbr 
WHERE T1.TransactionID = 0 AND T2.TransactionID <> 0 AND T1.TranType IN ('PAC','VDG') AND T1.TranStatus = 'ERR'

SET IDENTITY_INSERT ExternalTransaction ON

INSERT INTO ExternalTransaction(TransactionId, CompanyId, PMInstanceID, DocType, RefNbr, 
OrigDocType, OrigRefNbr, TranNumber, AuthNumber, Amount, ProcessingStatus, 
LastActivityDate, Direction, Active, Completed, ParentTranID, ExpirationDate, CVVVerification)
  
SELECT TransactionId, CompanyID, PMInstanceID, DocType, RefNbr, OrigDocType, OrigRefNbr, PCTranNumber, AuthNumber, 
Amount, ProcessingStatus, LastActivityDate, Direction, Active, Completed,
ParentTransaction, ExpirationDate, CVVVerification
FROM TempExternalTransaction 

SET IDENTITY_INSERT ExternalTransaction OFF

DROP TABLE TempExternalTransaction
GO

--[mysql: Native]
--[mssql: Skip]
--[MinVersion(Hash = 179c92c630ec9409da061c0dd6f86418532a18ddfc48b15dc7936056919cda13)]
UPDATE CCProcTran T5, (SELECT T1.CompanyID,T1.TranNbr,T1.PMInstanceId, T2.PCTranNumber 
FROM CCProcTran T1 
INNER JOIN CCProcTran T2 ON T1.CompanyID = T2.CompanyID AND T1.PMInstanceID = T2.PMInstanceID AND T1.RefTranNbr = T2.TranNbr
INNER JOIN CCProcessingCenter T3 ON T1.CompanyID = T3.CompanyID AND T1.ProcessingCenterID = T3.ProcessingCenterID
WHERE T3.ProcessingTypeName IN ('PX.CCProcessing.V2.AuthnetProcessingPlugin', 'PX.CCProcessing.AuthorizeNetProcessing', 'PX.CCProcessing.AuthorizeNetTokenizedProcessing') 
AND T1.TranStatus = 'APR' AND T1.ProcStatus = 'FIN' AND T1.PCTranNumber = '0' AND COALESCE(T2.PCTranNumber,'0') <> '0'
AND T1.DocType <=> T2.DocType AND T1.RefNbr <=> T2.RefNbr
AND T1.OrigDocType <=> T2.OrigDocType AND T1.OrigRefNbr <=> T2.OrigRefNbr 
AND T1.TranType IN ('VDG','PAC')) T4
SET T5.PCTranNumber = T4.PCTranNumber 
WHERE T4.CompanyID = T5.CompanyID AND T4.PMInstanceID = T5.PMInstanceID AND T4.TranNbr = T5.TranNbr;
GO

--[mysql: Native]
--[mssql: Skip]
--[OldHash(Hash = 8eefba1ca778876fc84b9b29127c843df1d235b0f1368192d3862dc738b5e644)]
--[OldHash(Hash = e309b4c0fe595dc8eb6da8b91edbd98741e50fd7cabfb2422dd29293294ae80b)]
--[MinVersion(Hash = 4ca1b24bf7981b1b7d028624e83c1450b939904960a4321af499331726f34fdc)]
DROP TABLE IF EXISTS TempExternalTransaction;
CREATE TABLE TempExternalTransaction
(
	TransactionId int not null,
	CompanyID int not null,
	TempTranNbr int,
	TempCdtParentNbr int,
	PMInstanceID int not null,
	PCTranNumber nvarchar(50),
	AuthNumber nvarchar(50),
	Amount decimal(19,4),
	ProcessingStatus char(3),
	LastActivityDate datetime,
	Direction char(1),
	Active bit,
	Completed bit,
	ParentTransaction int,
	ExpirationDate datetime,
	OrigDocType char(3),
	OrigRefNbr nvarchar(15),
	DocType char(3),
	RefNbr nvarchar(15),
	CVVVerification char(3),
    PrevValue int
); 
TRUNCATE TABLE ExternalTransaction;
CREATE INDEX TempExternalTransaction_Index ON TempExternalTransaction (CompanyID, TransactionID, TempTranNbr);  

INSERT INTO TempExternalTransaction(TransactionID, TempTranNbr, CompanyID, TempCdtParentNbr, PMInstanceID,
	PCTranNumber,AuthNumber,Amount,ProcessingStatus,LastActivityDate,Direction,Active,
	Completed, ExpirationDate, OrigDocType, OrigRefNbr, DocType, RefNbr, CVVVerification, PrevValue)
SELECT @row_num :=IF(@prev_value=T1.CompanyID,@row_num+1,1),
T1.TranNbr, T1.CompanyID, T1.CdtParent, T1.PMInstanceID, T1.PCTranNumber, 
T1.AuthNumber, T1.Amount, T1.ProcessingStatus, T1.LastActivityDate, 
T1.Direction, T1.Active, T1.Completed, T1.ExpirationDate,T1.OrigDocType,
T1.OrigRefNbr,T1.DocType, T1.RefNbr, T1.CVVVerification,
@prev_value := T1.CompanyID
FROM (SELECT 
T1.TranNbr, 
T1.CompanyID,
CASE WHEN  COALESCE(T1.CdtRefTranNbr, - 1) <> -1 
THEN 
	(SELECT MAX(TranNbr) FROM CCProcTran WHERE PCTranNumber = 
	(SELECT PCTranNumber FROM CCProcTran WHERE CompanyID = T1.CompanyID AND PMInstanceID = PMInstanceID AND TranNbr = T1.CdtRefTranNbr) 
		AND CompanyID = T1.CompanyID AND PMInstanceID = T1.PMInstanceID)
ELSE 
	NULL 
END AS 'CdtParent',
T1.PMInstanceID AS 'PMInstanceID',
T1.PCTranNumber AS 'PCTranNumber',
T1.AuthNumber AS 'AuthNumber',
T2.Amount AS 'Amount',
COALESCE (
	CASE WHEN COALESCE(T5.TranStatus,'') = 'ERR' 
	THEN 
		CASE T5.TranType 
		WHEN 'AUT' THEN 'AUF' WHEN 'AAC' THEN 'CAF' 
		WHEN 'PAC' THEN 'CAF' WHEN 'CAP' THEN 'CAF' 
		WHEN 'VDG' THEN 'VDF' WHEN 'CDT' THEN 'CDF' 
		END 
	WHEN COALESCE(T2.TranStatus,'') = 'ERR' 
	THEN 
		CASE T2.TranType 
		WHEN 'AUT' THEN 'AUF'  WHEN 'AAC' THEN 'CAF' 
		WHEN 'PAC' THEN 'CAF' WHEN 'CAP' THEN 'CAF' 
		WHEN 'VDG' THEN 'VDF' WHEN 'CDT' THEN 'CDF' 
		END
	WHEN COALESCE(T2.TranStatus,'') = 'HFR' 
	THEN 
		CASE T2.TranType 
		WHEN 'AUT' THEN 'AUH' WHEN 'AAC' THEN 'CAH'
		WHEN 'PAC' THEN 'CAH' WHEN 'CAP' THEN 'CAH' 
		WHEN 'VDG' THEN 'VDH' WHEN 'CDT' THEN 'CDH' 
		END
	WHEN COALESCE(T2.TranStatus,'') = 'APR' 
	THEN 
		CASE T2.TranType 
		WHEN 'AUT' THEN 'AUS' WHEN 'AAC' THEN 'CAS' 
		WHEN 'PAC' THEN 'CAS' WHEN 'CAP' THEN 'CAS' 
		WHEN 'VDG' THEN 'VDS' WHEN 'CDT' THEN 'CDS' 
		END
	WHEN COALESCE(T2.TranStatus,'') = 'DEC' 
	THEN 
		CASE T2.TranType 
		WHEN 'AUT' THEN 'AUD' WHEN 'AAC' THEN 'CAD' 
		WHEN 'PAC' THEN 'CAD' WHEN 'CAP' THEN 'CAD' 
		WHEN 'VDG' THEN 'VDD' WHEN 'CDT' THEN 'CDD' 
		END
END, 'UKN') AS 'ProcessingStatus',
CASE WHEN COALESCE(T5.EndTime,'19000101') > COALESCE(T2.EndTime,'19000101') THEN T5.EndTime ELSE T2.EndTime END AS 'LastActivityDate',
CASE WHEN T1.DirectionInt > 0 THEN 'C' ELSE 'D' END AS 'Direction',
CASE WHEN T2.TranStatus = 'ERR' OR T2.ProcStatus = 'OPN' OR T2.TranType = 'VDG' OR T2.TranStatus  = 'DEC' THEN 0 ELSE 1 END AS 'Active',
CASE WHEN T2.TranStatus = 'APR' AND (T2.TranType = 'CDT' OR T2.TranType = 'AAC' OR T2.TranType = 'PAC' OR T2.TranType = 'CAP') THEN 1 ELSE 0 END AS 'Completed',
T1.ExpirationDate AS 'ExpirationDate',
T1.OrigDocType AS 'OrigDocType',
T1.OrigRefNbr AS 'OrigRefNbr',
T1.DocType AS 'DocType',
T1.RefNbr AS 'RefNbr',
T2.CVVVerificationStatus as 'CVVVerification'
FROM 
(
	SELECT CompanyID,PMInstanceID,DocType,RefNbr,OrigDocType,OrigRefNbr,PCTranNumber,MAX(TranNbr) AS TranNbr, 
	MAX(AuthNumber) AS AuthNumber, MAX(ExpirationDate) AS ExpirationDate,SUM(DirectionInt) AS DirectionInt, MAX(CdtRefTranNbr) AS CdtRefTranNbr
	FROM 
	( 
		SELECT CompanyID,DocType, RefNbr, OrigDocType, OrigRefNbr, PCTranNumber, Amount, TranNbr, AuthNumber, ExpirationDate,
			CASE WHEN PMInstanceID = -1 
			THEN (SELECT T3.PMInstanceID FROM CCProcTran T3 WHERE T3.CompanyID = T2.CompanyID AND T3.DocType = T2.DocType 
				AND T3.RefNbr = T2.RefNbr AND T3.PCTranNumber = T2.PCTranNumber ORDER BY T3.TranNbr DESC LIMIT 1) 
			ELSE PMInstanceID 
			END AS PMInstanceID, 
			CASE WHEN TranType = 'CDT' THEN 1 ELSE 0 END AS DirectionInt, 
			CASE WHEN TranType = 'CDT' THEN RefTranNbr ELSE NULL END AS CdtRefTranNbr 
		FROM CCProcTran T2
		WHERE COALESCE(PCTranNumber,'') <> '' AND TranStatus <> 'ERR'
	) T3
	GROUP BY T3.CompanyID, T3.PMInstanceID, T3.DocType, T3.RefNbr, T3.OrigDocType, T3.OrigRefNbr, T3.PCTranNumber
    UNION
	SELECT CompanyID, PMInstanceID, DocType, RefNbr,
	OrigDocType, OrigRefNbr, PCTranNumber, TranNbr, AuthNumber AS AuthNumber, ExpirationDate as ExpirationDate,
	CASE WHEN TranType = 'CDT' THEN 1 ELSE 0 END as DirectionInt, CASE WHEN TranType = 'CDT' 
	THEN RefTranNbr ELSE Null END AS CdtRefTranNbr 
	FROM CCProcTran
	WHERE (COALESCE(PCTranNumber,'0') = '0' OR PCTranNumber = '' OR TranStatus = 'ERR') AND NOT ( COALESCE(RefTranNbr, -1) <> -1 AND TranType <> 'CDT' ) 
) T1 
INNER JOIN 
(
	SELECT * FROM CCProcTran 
) T2 ON T1.CompanyID = T2.CompanyID AND T1.TranNbr = T2.TranNbr 
LEFT JOIN
(
	SELECT T3.CompanyID, T3.PMInstanceID, T4.TranNbr,T3.MaxTranNbr, RefTranNbr, T4.TranType, T4.ProcStatus, T4.TranStatus, T4.EndTime 
	FROM 
	(
		SELECT MAX(TranNbr) AS MaxTranNbr, RefTranNbr, CompanyID, PMInstanceID
		FROM CCProcTran
		WHERE COALESCE(RefTranNbr, -1) <> -1 AND TranType <> 'CDT' AND TranStatus = 'ERR'
		GROUP BY CompanyID, PMInstanceID, RefTranNbr
	) T3 
	INNER JOIN
	(
		SELECT TranNbr, CompanyID, PMInstanceID, DocType, RefNbr, 
		OrigDocType, OrigRefNbr, EndTime, TranType, ProcStatus, TranStatus 
		FROM CCProcTran
	) T4 ON T3.CompanyID = T4.CompanyID  AND T3.PMInstanceID = T4.PMInstanceID AND T3.MaxTranNbr = T4.TranNbr
) T5 ON T5.CompanyID = T1.CompanyID AND T5.PMInstanceID = T1.PMInstanceID AND T5.RefTranNbr = T1.TranNbr 
) T1 
CROSS JOIN (SELECT @row_num :=1,  @prev_value :=0) vars
ORDER BY T1.CompanyID, T1.TranNbr;
  

UPDATE TempExternalTransaction T1, (SELECT CompanyID, PMInstanceID, TransactionId, TempTranNbr FROM TempExternalTransaction ) T2
SET T1.ParentTransaction = T2.TransactionId
WHERE COALESCE(T1.TempCdtParentNbr, -1) <> -1 AND T1.CompanyID = T2.CompanyID AND T1.PMInstanceID = T2.PMInstanceID AND T1.TempCdtParentNbr = T2.TempTranNbr; 

UPDATE TempExternalTransaction T1, (SELECT T3.CompanyId, T3.TempTranNbr FROM TempExternalTransaction T2 INNER JOIN TempExternalTransaction T3
ON T2.TempCdtParentNbr = T3.TempTranNbr AND T2.CompanyID = T3.CompanyID 
WHERE T2.Active = 1 AND T2.RefNbr <=> T3.RefNbr AND T2.DocType <=> T3.DocType 
AND T2.OrigRefNbr <=> T3.OrigRefNbr AND T2.OrigDocType <=> T3.OrigDocType ) T4
SET T1.Active = 0 WHERE T1.CompanyId = T4.CompanyId AND T1.TempTranNbr=T4.TempTranNbr;

UPDATE CCProcTran SET TransactionID = 0;

UPDATE CCProcTran T1, (SELECT CompanyID, TransactionID, PMInstanceID, TempTranNbr FROM TempExternalTransaction) T2
SET T1.TransactionID = T2.TransactionID
WHERE T1.CompanyID = T2.CompanyID AND T1.PMInstanceID = T2.PMInstanceID
AND T1.TranNbr = T2.TempTranNbr;
  
UPDATE CCProcTran T4 SET T4.TransactionID = COALESCE((SELECT T3.TransactionID 
	FROM TempExternalTransaction T3 
	WHERE T4.CompanyID = T3.CompanyID AND (T4.PMInstanceID = T3.PMInstanceID OR T4.PmInstanceID = -1)
	AND T4.DocType <=> T3.DocType AND T4.RefNbr <=> T3.RefNbr
    AND T4.OrigDocType  <=> T3.OrigDocType AND T4.OrigRefNbr <=> T3.OrigRefNbr
    AND T3.PCTranNumber = 
		(SELECT COALESCE((SELECT T2.PCTranNumber FROM (SELECT * FROM CCProcTran) T2 
			WHERE T2.CompanyID = T1.companyID AND T2.PMInstanceID = T1.PMInstanceID
			AND T1.RefTranNbr = T2.TranNbr AND T1.TranType <> 'CDT'
			LIMIT 1), T1.PCTranNumber) AS PCTranNumber
    FROM (SELECT * FROM CCProcTran) T1 
	WHERE T4.CompanyID = T1.CompanyID AND (T4.PmInstanceID = T1.PMInstanceID OR T4.PmInstanceID = -1)
    AND T4.DocType <=> T1.DocType 
    AND T4.RefNbr <=> T1.RefNbr
    AND T4.OrigDocType  <=> T1.OrigDocType
    AND T4.OrigRefNbr <=> T1.OrigRefNbr
	AND T4.TranType <> 'CDT'
	AND (T4.RefTranNbr = T1.RefTranNbr OR T4.TranNbr = T1.RefTranNbr)
    ORDER BY PCTranNumber DESC
	LIMIT 1) 
LIMIT 1), 0)
WHERE T4.TransactionID = 0;
    
UPDATE CCProcTran T1 
INNER JOIN CCProcTran T2 ON T1.CompanyID = T2.CompanyID AND T1.PMInstanceID = T2.PMInstanceID
AND T1.DocType <=> T2.DocType AND T1.RefNbr <=> T2.RefNbr
AND T1.OrigDocType <=> T2.OrigDocType AND T1.OrigRefNbr <=> T2.OrigRefNbr
AND T1.RefTranNbr = T2.RefTranNbr
SET T1.TransactionID = T2.TransactionID
WHERE T1.TransactionID = 0 AND T2.TransactionID <> 0; 	
	
UPDATE CCProcTran T1 
INNER JOIN CCProcTran T2 ON T1.CompanyID = T2.CompanyID AND T1.RefTRanNbr = T2.TranNbr
SET T1.TransactionID = T2.TransactionID
WHERE T1.TransactionID = 0 AND T2.TransactionID <> 0 AND T1.TranType IN ('PAC','VDG') AND T1.TranStatus = 'ERR';	
	
INSERT INTO ExternalTransaction(TransactionId, CompanyId, PMInstanceID, DocType, RefNbr, 
OrigDocType, OrigRefNbr, TranNumber, AuthNumber, Amount, ProcessingStatus, 
LastActivityDate, Direction, Active, Completed, ParentTranID, ExpirationDate, CVVVerification, NoteID)
SELECT TransactionId, CompanyID, PMInstanceID, DocType, RefNbr, OrigDocType, OrigRefNbr, PCTranNumber, AuthNumber, 
Amount, ProcessingStatus, LastActivityDate, Direction, Active, Completed,
ParentTransaction, ExpirationDate, CVVVerification, uuid()
FROM TempExternalTransaction 
ORDER BY CompanyID, TransactionId;

DROP TABLE TempExternalTransaction;
GO

--[MinVersion(Hash = b30772c600e5bb29b9f11899c8b8c4c7fcaa0f6c6e8d1c4e01fb69ee7421984c)]
UPDATE CASetup
SET AmountWeight = 10
GO

--[MinVersion(Hash = e3cb800908d8eaf8766430d7786ad300b1309b57598225aa3d749447a9b66045)]
UPDATE ca
SET 
  ca.ReceiptTranDaysBefore = s.ReceiptTranDaysBefore
  ,ca.ReceiptTranDaysAfter=s.ReceiptTranDaysAfter
  ,ca.DisbursementTranDaysBefore=s.DisbursementTranDaysBefore
  ,ca.DisbursementTranDaysAfter=s.DisbursementTranDaysAfter			
  ,ca.RefNbrCompareWeight=s.RefNbrCompareWeight
  ,ca.EmptyRefNbrMatching=s.EmptyRefNbrMatching
  ,ca.DateCompareWeight=s.DateCompareWeight	
  ,ca.PayeeCompareWeight=s.PayeeCompareWeight
  ,ca.DateMeanOffset=s.DateMeanOffset
  ,ca.DateSigma=s.DateSigma
  ,ca.CuryDiffThreshold=s.CuryDiffThreshold
  ,ca.AmountWeight=s.AmountWeight
  ,ca.SkipVoided=s.SkipVoided
  ,ca.AllowMatchingCreditMemo = s.AllowMatchingCreditMemo
  ,ca.MatchSettingsPerAccount = 0
  FROM CashAccount ca
  INNER JOIN CASetup s
  ON ca.CompanyID = s.CompanyID
GO

--[MinVersion(Hash = 17566a465c2f8cb1fbae8b8306bb8abead7e605e296ffc0484942c4136be5662)]
UPDATE CABankTran
SET LineCntrMatch = 
	( CASE  
         WHEN mTable.LineCount IS NOT NULL 
			THEN mTable.LineCount
         ELSE 0 
       END  
    )  
FROM CABankTran cbt
LEFT JOIN (SELECT m.CompanyID, m.TranID, COUNT(*) LineCount
			FROM CABankTranMatch m
			GROUP BY m.CompanyID, m.TranID) mTable
		ON mTable.CompanyID=cbt.CompanyID AND mTable.TranID=cbt.TranID;
GO

--[OldHash(Hash = 6aa5df4e13bd1e05aec5f05d4c32964824504780ad0dc85f7ad0e3b9478c16d7)]
--[OldHash(Hash = c9c5acdb7c5999f8c100afdd95efd5813d4d3c63d11e54ca2f784116e2238358)]
--[MinVersion(Hash = 3ce6fff4988c10793877a10bbcbaeca519a5ea2e7fef4a622889dda075fb8897)]
UPDATE ARRegister SET PendingProcessing = 0;
UPDATE T1 SET T1.PendingProcessing = 1 FROM ARRegister T1 
INNER JOIN ARPayment T2 ON T1.CompanyID = T2.CompanyID AND T1.DocType = T2.DocType AND T1.RefNbr = T2.RefNbr
INNER JOIN PaymentMethod T3 ON T2.CompanyID = T3.CompanyID AND T2.PaymentMethodID = T3.PaymentMethodID
LEFT JOIN (
	SELECT T8.DocType, T8.RefNbr, T8.CompanyID, T9.ProcessingStatus, T9.Active FROM 
    (SELECT T7.DocType, T7.RefNbr, T7.CompanyID, T5.DocType AS ETDocType, T5.RefNbr AS ETRefNbr, MAX(TransactionID) as TransactionID FROM ExternalTransaction T5 
    INNER JOIN ARPayment T6 ON T6.CompanyID = T5.CompanyID AND T6.PMInstanceID = T5.PMInstanceID
    AND T6.DocType = T5.DocType AND T6.RefNbr = T5.RefNbr
    INNER JOIN ARRegister T7 ON T7.CompanyID = T5.CompanyID AND T7.OrigRefNbr = T5.RefNbr AND T7.OrigDocType = T5.DocType 
    WHERE (T7.DocType = 'RPM' OR T7.DocType = 'RCS')
	GROUP BY T7.DocType, T7.RefNbr, T7.CompanyID, T5.DocType, T5.RefNbr) T8
	INNER JOIN ExternalTransaction T9 ON T8.CompanyID = T9.CompanyID AND T8.ETDocType = T9.DocType AND T8.ETRefNbr = T9.RefNbr
) T10 ON T10.CompanyID = T1.CompanyID AND T10.DocType = T1.DocType AND T10.RefNbr = T1.RefNbr
LEFT JOIN (
	SELECT T12.DocType, T12.refNbr, T12.CompanyID, MAX(T11.ProcessingStatus) AS ProcessingStatus FROM ExternalTransaction T11
    INNER JOIN ARPayment T12 ON T12.CompanyID = T11.CompanyID AND T12.PMInstanceID = T11.PMInstanceId
    AND T12.DocType = T11.DocType AND T12.RefNbr = T11.RefNbr 
    WHERE (T11.ProcessingStatus = 'CDS' OR T11.ProcessingStatus = 'CAS')
    GROUP BY T12.DocType, T12.refNbr, T12.CompanyID
) T13 ON T13.CompanyID = T1.CompanyID AND T13.DocType = T1.DocType AND T13.RefNbr = T1.RefNbr
WHERE T1.Released = 0 AND T1.Voided = 0 AND T3.ARIsOnePerCustomer = 0 AND T3.PaymentType = 'CCD'
AND T1.DocType IN ('PMT','RPM','PPM','REF','CSL','RCS') 
AND COALESCE(T10.Active, 1) = 1 AND NOT(T10.Active IS NULL AND T1.DocType = 'RPM')
AND COALESCE(T13.ProcessingStatus,'') = ''
GO

--[MinVersion(Hash = 892248ff958170ab1c5ca9827d10deded952c8997afa924aa21601727301291a)]
UPDATE ARPayment SET IsCCAuthorized = 0, IsCCCaptured = 0, IsCCRefunded = 0
UPDATE T1 
SET IsCCAuthorized = CASE WHEN COALESCE(T6.TranType,'') = 'AUT' AND COALESCE(T6.TranStatus,'') NOT IN ('DEC','HFR') AND COALESCE(T10.TranType,'') = '' THEN 1 
	WHEN COALESCE(T10.TranType,'') = 'AUT' AND COALESCE(T10.TranStatus,'') NOT IN ('DEC','HFR') THEN 1 ELSE 0 END,
IsCCCaptured = CASE WHEN  COALESCE(T6.TranType,'') IN ('AAC','PAC','CAP') AND COALESCE(T6.TranStatus,'') NOT IN ('DEC','HFR') AND COALESCE(T10.TranType,'') = '' THEN 1 
	WHEN COALESCE(T10.TranType,'') IN ('AAC','PAC','CAP') AND COALESCE(T10.TranStatus,'') NOT IN ('DEC','HFR') THEN 1 ELSE 0 END,
IsCCRefunded = CASE WHEN COALESCE(T6.TranType,'') = 'CDT' AND COALESCE(T6.TranStatus,'') NOT IN ('DEC','HFR') AND COALESCE(T10.TranType,'') = '' THEN 1 
	WHEN COALESCE(T10.TranType,'') = 'CDT' AND COALESCE(T10.TranStatus,'') NOT IN ('DEC','HFR') THEN 1 ELSE 0 END
FROM ARPayment T1
INNER JOIN ARRegister T2 ON T1.CompanyID = T2.CompanyID AND T1.DocType = T2.DocType AND T1.RefNbr = T2.RefNbr
INNER JOIN PaymentMethod T3 ON T1.CompanyID = T3.CompanyID AND T1.PaymentMethodID = T3.PaymentMethodID 
LEFT JOIN 
(
	SELECT T5.TranNbr,T5.CompanyID,T5.PMInstanceID,T5.DocType,T5.RefNbr,T6.TranType,T6.TranStatus
	FROM 
	(	SELECT MAX(TranNbr) AS TranNbr, T4.CompanyID, T4.PMInstanceID, T4.DocType,T4.RefNbr
		FROM CCProcTran T4 WHERE T4.TranStatus IN ('APR','HFR','DEC') AND T4.ProcStatus = 'FIN' 
		GROUP BY T4.CompanyID, T4.PMInstanceID, T4.DocType, T4.RefNbr 
	) T5
	INNER JOIN CCProcTran T6 ON T5.TranNbr = T6.TranNbr AND T5.CompanyID = T6.CompanyID AND T5.PMInstanceID = T6.PMInstanceID
) T6 ON T6.CompanyID = T2.CompanyID AND T6.PMInstanceID = T1.PMInstanceID AND COALESCE(T6.DocType,'') = COALESCE(T2.OrigDocType,T2.DocType,'') 
AND COALESCE(T6.RefNbr,'') = COALESCE(T2.OrigRefNbr,T2.RefNbr,'')
LEFT JOIN 
(
	SELECT T8.TranNbr,T8.CompanyID,T8.PMInstanceID,T8.DocType,T8.RefNbr,T9.TranType,T9.TranStatus
	FROM 
	(	SELECT MAX(TranNbr) AS TranNbr, T7.CompanyID, T7.PMInstanceID, T7.DocType,T7.RefNbr
		FROM CCProcTran T7 WHERE T7.TranStatus IN ('APR','HFR','DEC') AND T7.ProcStatus = 'FIN' 
		GROUP BY T7.CompanyID, T7.PMInstanceID, T7.DocType, T7.RefNbr 
	) T8
	INNER JOIN CCProcTran T9 ON T8.TranNbr = T9.TranNbr AND T8.CompanyID = T9.CompanyID AND T8.PMInstanceID = T9.PMInstanceID
) T10 ON T10.CompanyID = T2.CompanyID AND T10.PMInstanceID = T1.PMInstanceID AND COALESCE(T10.DocType,'') = COALESCE(T2.DocType,'') 
AND COALESCE(T10.RefNbr,'') = COALESCE(T2.RefNbr,'')
WHERE T3.ARIsOnePerCustomer = 0 AND T3.PaymentType = 'CCD' AND T2.DocType IN ('PMT','RPM','PPM','REF','CSL','RCS')
GO

--[MinVersion(Hash = adc3aa42e56966ee313418d652ceac4fa229c7a467b999517ade1ba1b4674992)]
UPDATE ExternalTransaction SET NoteID = NEWID()
GO

--[MinVersion(Hash = e39ef3f18950b237398301e124e086c9e524bb294d6da5cbea5607f8fd435a9e)]
update APRegister set RefNoteID = APRegister.NoteID
from APRegister 
inner join CABankTran on APRegister.CompanyID = CABankTran.CompanyID and APRegister.NoteID = CABankTran.NoteID
GO

--[MinVersion(Hash = 42d3157b77ac8fbc41cecaa6a1cc285f7830e32721f743a7874b148673e5130b)]
update ARRegister set RefNoteID = ARRegister.NoteID
from ARRegister 
inner join CABankTran on ARRegister.CompanyID = CABankTran.CompanyID and ARRegister.NoteID = CABankTran.NoteID
GO



--[RefreshMetadata()]
GO

--DIST scripts
--[MinVersion(Hash = 4034287911b9752353d27a074b9aeab97d092a15d1ee01bc15006b3b68cecbce)]

GO

--[MinVersion(Hash = 0ab5c23810430ff4be5aff088da39130e64bfd5ac3ec3d8a4f468dc6ddc4be1e)]
UPDATE POReceipt
SET TranPeriodID = FinPeriodID
GO

--[MinVersion(Hash = b0851e9c89866627c85c99a79288ca388a0c2bd0fffbcb93e7b2bc4646a1e236)]
UPDATE INRegister
SET TranPeriodID = FinPeriodID
GO

--[MinVersion(Hash = 26ee4e3e91e5d903032a597d56541aaefbbc6081ad27cee490bea56e8d88fb0d)]
UPDATE INTran
SET TranPeriodID = FinPeriodID
GO

--[MinVersion(Hash = de6fee57e8c654e5ee2fc3ea7ed0612aea80d20a366750758b51db4451190765)]
UPDATE INTranCost
SET TranPeriodID = FinPeriodID
GO

--[MinVersion(Hash = 99b07f82f46bfb94587b9ca393c36acb1592f1def3f44d48d9a4d7a092915b53)]
UPDATE INPIHeader
SET TranPeriodID = FinPeriodID
GO

--[MinVersion(Hash = dc214e5816633d9894d57f1739f2fd7319a86d17dd946fda8b145bf9b54baf64)]
UPDATE INItemSiteHist
SET
	TranPtdQtyReceived = FinPtdQtyReceived,
	TranPtdQtyIssued = FinPtdQtyIssued,
	TranPtdQtySales = FinPtdQtySales,
	TranPtdQtyCreditMemos = FinPtdQtyCreditMemos,
	TranPtdQtyDropShipSales = FinPtdQtyDropShipSales,
	TranPtdQtyTransferIn = FinPtdQtyTransferIn,
	TranPtdQtyTransferOut = FinPtdQtyTransferOut,
	TranPtdQtyAssemblyIn = FinPtdQtyAssemblyIn,
	TranPtdQtyAssemblyOut = FinPtdQtyAssemblyOut,
	TranPtdQtyAdjusted = FinPtdQtyAdjusted,
	TranBegQty = FinBegQty,
	TranYtdQty = FinYtdQty
GO

--[MinVersion(Hash = 50387069075fcfac511b3847e6ee529872732313873071e56684784d7815146d)]
UPDATE INItemCostHist
SET
	TranPtdCostReceived = FinPtdCostReceived,
	TranPtdCostIssued = FinPtdCostIssued,
	TranPtdQtyReceived = FinPtdQtyReceived,
	TranPtdQtyIssued = FinPtdQtyIssued,
	TranPtdCOGS = FinPtdCOGS,
	TranPtdCOGSCredits = FinPtdCOGSCredits,
	TranPtdCOGSDropShips = FinPtdCOGSDropShips,
	TranPtdCostTransferIn = FinPtdCostTransferIn,
	TranPtdCostTransferOut = FinPtdCostTransferOut,
	TranPtdCostAssemblyIn = FinPtdCostAssemblyIn,
	TranPtdCostAssemblyOut = FinPtdCostAssemblyOut,
	TranPtdCostAdjusted = FinPtdCostAdjusted,
	TranPtdQtySales = FinPtdQtySales,
	TranPtdQtyCreditMemos = FinPtdQtyCreditMemos,
	TranPtdQtyDropShipSales = FinPtdQtyDropShipSales,
	TranPtdQtyTransferIn = FinPtdQtyTransferIn,
	TranPtdQtyTransferOut = FinPtdQtyTransferOut,
	TranPtdQtyAssemblyIn = FinPtdQtyAssemblyIn,
	TranPtdQtyAssemblyOut = FinPtdQtyAssemblyOut,
	TranPtdQtyAdjusted = FinPtdQtyAdjusted,
	TranPtdSales = FinPtdSales,
	TranPtdCreditMemos = FinPtdCreditMemos,
	TranPtdDropShipSales = FinPtdDropShipSales,
	TranBegCost = FinBegCost,
	TranYtdCost = FinYtdCost,
	TranBegQty = FinBegQty,
	TranYtdQty = FinYtdQty
GO

--[MinVersion(Hash = 0b2d81b9be84aea395fcf69e53a08ad15db8a7a9ada913b23339f761405f36a7)]
UPDATE INItemSalesHist
SET
	TranPtdCOGS = FinPtdCOGS,
	TranPtdCOGSCredits = FinPtdCOGSCredits,
	TranPtdCOGSDropShips = FinPtdCOGSDropShips,
	TranPtdQtySales = FinPtdQtySales,
	TranPtdQtyCreditMemos = FinPtdQtyCreditMemos,
	TranPtdQtyDropShipSales = FinPtdQtyDropShipSales,
	TranPtdSales = FinPtdSales,
	TranPtdCreditMemos = FinPtdCreditMemos,
	TranPtdDropShipSales = FinPtdDropShipSales,
	TranYtdCOGS = FinYtdCOGS,
	TranYtdCOGSCredits = FinYtdCOGSCredits,
	TranYtdCOGSDropShips = FinYtdCOGSDropShips,
	TranYtdQtySales = FinYtdQtySales,
	TranYtdQtyCreditMemos = FinYtdQtyCreditMemos,
	TranYtdQtyDropShipSales = FinYtdQtyDropShipSales,
	TranYtdSales = FinYtdSales,
	TranYtdCreditMemos = FinYtdCreditMemos,
	TranYtdDropShipSales = FinYtdDropShipSales
GO

--[MinVersion(Hash = 6758c6faff66026161f1c0375cdbf64830d10b260d6853259790005d3eaca977)]
UPDATE INItemCustSalesHist
SET
	TranPtdCOGS = FinPtdCOGS,
	TranPtdCOGSCredits = FinPtdCOGSCredits,
	TranPtdCOGSDropShips = FinPtdCOGSDropShips,
	TranPtdQtySales = FinPtdQtySales,
	TranPtdQtyCreditMemos = FinPtdQtyCreditMemos,
	TranPtdQtyDropShipSales = FinPtdQtyDropShipSales,
	TranPtdSales = FinPtdSales,
	TranPtdCreditMemos = FinPtdCreditMemos,
	TranPtdDropShipSales = FinPtdDropShipSales,
	TranYtdCOGS = FinYtdCOGS,
	TranYtdCOGSCredits = FinYtdCOGSCredits,
	TranYtdCOGSDropShips = FinYtdCOGSDropShips,
	TranYtdQtySales = FinYtdQtySales,
	TranYtdQtyCreditMemos = FinYtdQtyCreditMemos,
	TranYtdQtyDropShipSales = FinYtdQtyDropShipSales,
	TranYtdSales = FinYtdSales,
	TranYtdCreditMemos = FinYtdCreditMemos,
	TranYtdDropShipSales = FinYtdDropShipSales
GO

--Remove tables which are not presented anymore in PO Receipt Entry from the audit
--[MinVersion(Hash = 843565481ef427f9c61fe29833d482649dfd461a7b4ccb3103ec3d1104c1a337)]
DELETE FROM AUAuditTable
WHERE ScreenID = 'PO302000'
	AND TableName IN ('CurrencyInfo', 'LandedCostTran', 'POReceiptDiscountDetail', 'POReceiptTaxTran')

DELETE FROM AUAuditField
WHERE ScreenID = 'PO302000'
	AND TableName IN ('CurrencyInfo', 'LandedCostTran', 'POReceiptDiscountDetail', 'POReceiptTaxTran')
GO

--set IsComponentItem for INTran records created from SO
--[MinVersion(Hash = 85eee3708dc4cfba93ea222d32ab4d3873d2b17c0d680dfb21f87d66a4a992af)]
update  i set
i.IsComponentItem = s.IsComponentItem
from INtran i inner join SOShipLineSplit s on s.CompanyID = i.CompanyID and i.SOShipmentType != 'H' 
and s.ShipmentNbr = i.SOShipmentNbr and s.LineNbr = i.SOShipmentLineNbr where i.IsComponentItem is null
GO

--set SOOrderLineOperation for ARTran records created from SO
--[MinVersion(Hash = bf0bf732a68cc8aa2cd7e0ead591e1f60e4d6b505a7ff5a874e6406666197de9)]
update a set
a.SOOrderLineOperation = s.Operation
from ARTran a inner join SOLine s on s.CompanyID = a.CompanyID and 
s.OrderType = a.SOOrderType and s.OrderNbr = a.SOOrderNbr and s.LineNbr = a.SOOrderLineNbr
where s.Operation is not null and a.SOOrderLineOperation is null
GO

-- Populate newly added fields in INItemSiteHistDay (the script is intended to work only on upgrade from 19.092 to 19.093)
--[MinVersion(Hash = ef35d617e13e2a6affcf9e436ed7dafebb9f8bb657bd622a3be8e44cb2251d89)]
UPDATE h
SET QtyDebit = COALESCE(sg.QTYDEBIT, 0), QtyCredit = COALESCE(sg.QTYCREDIT, 0)
FROM INItemSiteHistDay h
LEFT JOIN (
	SELECT CompanyID, InventoryID, SubItemID, SiteID, LocationID, TranDate,
	SUM(CASE WHEN InvtMult > 0 THEN Qty ELSE 0 END) AS QTYDEBIT, SUM(CASE WHEN InvtMult < 0 THEN Qty ELSE 0 END) AS QTYCREDIT
	FROM INTranSplit
	WHERE Released = 1
	GROUP BY CompanyID, InventoryID, SubItemID, SiteID, LocationID, TranDate) sg
ON h.CompanyID = sg.CompanyID AND h.InventoryID = sg.InventoryID AND h.SubItemID = sg.SubItemID AND h.SiteID = sg.SiteID AND h.LocationID = sg.LocationID AND h.SDate = sg.TranDate
GO

-- Fill INItemSiteHistDay (Stage 1 - populate all fields besides running totals)
--[OldHash(Hash = 173e30e9e6251951ec26d1258d30c2dd88d0f179de69269f0a7dfd8069f1a3d3)]
--[MinVersion(Hash = ef429ad1423e447c44d13919d08f3d187f6bc8d8f8a51a2629bfc756ec3240f9)]
INSERT INTO INItemSiteHistDay (CompanyID, InventoryID, SubItemID, SiteID, LocationID, SDate,
	BegQty, EndQty,
	QtyReceived, QtyIssued,
	QtySales, QtyCreditMemos,
	QtyDropShipSales,
	QtyTransferIn, QtyTransferOut,
	QtyAssemblyIn, QtyAssemblyOut,
	QtyAdjusted,
	QtyDebit, QtyCredit)
SELECT CompanyID, InventoryID, SubItemID, SiteID, LocationID, TranDate AS SDATE,
	0, 0,
	SUM(CASE WHEN TranType = 'RCP' THEN Qty ELSE 0 END) AS QTYRECEIVED, SUM(CASE WHEN TranType = 'III' THEN Qty WHEN TranType = 'RET' THEN -Qty ELSE 0 END) AS QTYISSUED, 
	SUM(CASE WHEN (TranType = 'DRM' OR TranType = 'INV') AND InvtMult <> 0 THEN Qty WHEN TranType = 'ADJ' AND InvtMult = 0 THEN Qty ELSE 0 END) AS QTYSALES, SUM(CASE WHEN TranType = 'CRM' AND InvtMult <> 0 THEN Qty ELSE 0 END) AS QTYCREDITMEMOS, 
	SUM(CASE WHEN (TranType = 'DRM' OR TranType = 'INV') AND InvtMult = 0 THEN Qty WHEN TranType = 'CRM' AND InvtMult = 0 THEN -Qty ELSE 0 END) AS QTYDROPSHIPSALES,
	SUM(CASE WHEN TranType = 'TRX' AND InvtMult = 1 THEN Qty ELSE 0 END) AS QTYTRANSFERIN, SUM(CASE WHEN TranType = 'TRX' AND InvtMult <> 1 THEN Qty ELSE 0 END) AS QTYTRANSFEROUT,
	SUM(CASE WHEN (TranType = 'ASY' OR TranType = 'DSY') AND InvtMult = 1 THEN Qty ELSE 0 END) AS QTYASSEMBLYIN, SUM(CASE WHEN (TranType = 'ASY' OR TranType = 'DSY') AND InvtMult <> 1 THEN Qty ELSE 0 END) AS QTYASSEMBLYOUT, 
	SUM(CASE WHEN TranType = 'ADJ' AND InvtMult = 0 THEN Qty WHEN TranType = 'ADJ' AND InvtMult <> 0 THEN InvtMult * Qty WHEN TranType = 'ASC' OR TranType = 'NSC' THEN Qty ELSE 0 END) AS QTYADJUSTED,
	SUM(CASE WHEN InvtMult > 0 THEN Qty ELSE 0 END) AS QTYDEBIT, SUM(CASE WHEN InvtMult < 0 THEN Qty ELSE 0 END) AS QTYCREDIT
FROM INTranSplit
WHERE Released = 1 AND LocationID IS NOT NULL
GROUP BY CompanyID, InventoryID, SubItemID, SiteID, LocationID, TranDate
GO

-- Fill INItemSiteHistDay (Stage 2 - populate running totals) - MS SQL specific version
--[mysql: Skip]
--[mssql: Native]
--[OldHash(Hash = 173e30e9e6251951ec26d1258d30c2dd88d0f179de69269f0a7dfd8069f1a3d3)]
--[MinVersion(Hash = f210d9a00ff41ae5e49d779e5604837602ccff82dfa51ee07d3616a5d0dda94b)]
WITH RunningTotal AS
(
SELECT CompanyID, InventoryID, SubItemID, SiteID, LocationID, SDate, SUM(QtyDebit - QtyCredit) OVER (PARTITION BY CompanyID, InventoryID, SubItemID, SiteID, LocationID ORDER BY SDate) AS CalcEndQty
FROM INItemSiteHistDay
)
UPDATE h
SET BegQty = rt.CalcEndQty - (QtyDebit - QtyCredit), EndQty = rt.CalcEndQty
FROM INItemSiteHistDay h
INNER JOIN RunningTotal rt
ON h.CompanyID = rt.CompanyID AND h.InventoryID = rt.InventoryID AND h.SubItemID = rt.SubItemID AND h.SiteID = rt.SiteID AND h.LocationID = rt.LocationID AND h.SDate = rt.SDate
GO

-- Fill INItemSiteHistDay (Stage 2 - populate running totals) - MySQL specific version uses "quirky" updates
--[mssql: Skip]
--[mysql: Native]
--[OldHash(Hash = 173e30e9e6251951ec26d1258d30c2dd88d0f179de69269f0a7dfd8069f1a3d3)]
--[OldHash(Hash = 69884213653a153ff1db7a69284bf253869b3e467b73a12deda9b79ada1cca76)]
--[MinVersion(Hash = afe16cae9a6f3c07d3c509075becccc1a15511d8a3a81c2c6f35a746af7fc6ef)]
SET @varBegQty = CAST(0 AS DECIMAL(25,6));
SET @varPrevCompanyID = NULL;
SET @varPrevSiteID = NULL;
SET @varPrevInventoryID = NULL;
SET @varPrevLocationID = NULL;
SET @varPrevSubItemID = NULL;

UPDATE INItemSiteHistDay
SET
	BegQty = (@varBegQty :=
		(CASE WHEN @varPrevCompanyID IS NULL OR @varPrevCompanyID <> CompanyID
			OR @varPrevSiteID <> SiteID
			OR @varPrevInventoryID <> InventoryID
			OR @varPrevLocationID <> LocationID
			OR @varPrevSubItemID <> SubItemID
		THEN CAST(0 AS DECIMAL(25,6)) ELSE @varBegQty END)),
	EndQty = (@varBegQty :=
		(CASE WHEN (@varPrevCompanyID := CompanyID) = CompanyID
			AND (@varPrevSiteID := SiteID) = SiteID
			AND (@varPrevInventoryID := InventoryID) = InventoryID
			AND (@varPrevLocationID := LocationID) = LocationID
			AND (@varPrevSubItemID := SubItemID) = SubItemID
		THEN @varBegQty + QtyDebit - QtyCredit ELSE CAST(0 AS DECIMAL(25,6)) END))
ORDER BY CompanyID, SiteID, InventoryID, LocationID, SubItemID, SDate;
GO

--AC-121633 Fill InventoryItemLotSerNumVal
--[IfExists(Column = InventoryItem.LotSerNumVal)]
--[MinVersion(Hash = dc4462bda616b39d356f0192f357d493f32bffab0b37f79338eeba5b04695ead)]
insert into [InventoryItemLotSerNumVal]
(
	[CompanyID],
	[InventoryID],
	[LotSerNumVal],
	[CreatedByID],
	[CreatedByScreenID],
	[CreatedDateTime],
	[LastModifiedByID],
	[LastModifiedByScreenID],
	[LastModifiedDateTime]
)
select
	i.[CompanyID],
	i.[InventoryID],
	i.[LotSerNumVal],
	i.[CreatedByID],
	i.[CreatedByScreenID],
	i.[CreatedDateTime],
	i.[LastModifiedByID],
	i.[LastModifiedByScreenID],
	i.[LastModifiedDateTime]
	from [InventoryItem] i
	where i.[StkItem] = 1 and i.[LotSerNumVal] is not null
GO

--AC-121633 Fill INLotSerClassLotSerNumVal
--[IfExists(Column = INLotSerClass.LotSerNumVal)]
--[MinVersion(Hash = 60ad7c6d1eeb9fbbb304b20d3e156e8acebe53af79791fb6f062040246f8652d)]
insert into [INLotSerClassLotSerNumVal]
(
	[CompanyID],
	[LotSerClassID],
	[LotSerNumVal],
	[CreatedByID],
	[CreatedByScreenID],
	[CreatedDateTime],
	[LastModifiedByID],
	[LastModifiedByScreenID],
	[LastModifiedDateTime]
)
select
	lsc.[CompanyID],
	lsc.[LotSerClassID],
	lsc.[LotSerNumVal],
	lsc.[CreatedByID],
	lsc.[CreatedByScreenID],
	lsc.[CreatedDateTime],
	lsc.[LastModifiedByID],
	lsc.[LastModifiedByScreenID],
	lsc.[LastModifiedDateTime]
	from [INLotSerClass] lsc
	where lsc.[LotSerTrack] <> 'N' and lsc.[LotSerNumVal] is not null
GO

--[MinVersion(Hash = 8796be29bf3d5e2fabc2829dde7211058bc04467e7657d54302405574699ff52)]
update ARAdjust set 
CuryAdjdOrigAmt = 0
where CuryAdjdOrigAmt is null
GO

--AC-123138 Migrate configuration option "Freeze Inventory When PI Count Is In Data Entry State"
--[IfExists(Column = INSite.LockSitePICountEntry)]
--[MinVersion(Hash = 770bf049fa867c6bb15729778136bc417b47aeae2011553f04da0d73523011c6)]
UPDATE CL
SET CL.UnlockSiteOnCountingFinish = 1 - S.LockSitePICountEntry
FROM INPIClass CL
INNER JOIN INSite S
	ON CL.SiteID = S.SiteID
	AND CL.CompanyID = S.CompanyID
GO

--[MinVersion(Hash = 038fc4b0a0f0d5f6b1547c9c6f2d5f9b8cfb868a08de526bf83a3c71630a2f68)]
UPDATE INSetup
SET AutoReleasePIAdjustment = 1
GO

--[MinVersion(Hash = 5eeac840e0eac581b9663c74920cd364a4b303696bd91065b9a39095777ead4b)]
update InventoryItem set CostBasis = 'S' where CostBasis is null
GO

-- Correct base qty fields in PO Lines with empty Inventory (services)
--[IfExists(Column = POLine.BaseVoucheredQty)]
--[OldHash(Hash = c7ef5f916d9862e94c280bfb27eaae92b9c043108e9be54c013dc73bc18ce772)]
--[MinVersion(Hash = 13c7cde7184af7b94eb27ec3f8bcbcd6f768298b053fc43dbcacf7510b377dad)]
UPDATE POLine
SET BaseOrderQty = OrderQty,
	BaseReceivedQty = ReceivedQty,
	BaseVoucheredQty = VoucheredQty,
	BaseOpenQty = OpenQty
WHERE InventoryID IS NULL
GO

-- Correct base qty fields in PO Receipt Lines with empty Inventory (services)
--[IfExists(Column = POReceiptLine.BaseVoucheredQty)]
--[OldHash(Hash = c7ef5f916d9862e94c280bfb27eaae92b9c043108e9be54c013dc73bc18ce772)]
--[MinVersion(Hash = 66c390076de871c1954cd4d7f4353059c7e765bfa59b81945a9ea31b1af4b6f0)]
UPDATE POReceiptLine
SET BaseReceiptQty = ReceiptQty,
	BaseMultReceiptQty = InvtMult * ReceiptQty,
	BaseVoucheredQty = VoucheredQty,
	BaseUnbilledQty = UnbilledQty
WHERE InventoryID IS NULL
GO

-- Correct base qty fields in AP Bill Lines with empty Inventory (services)
--[OldHash(Hash = c7ef5f916d9862e94c280bfb27eaae92b9c043108e9be54c013dc73bc18ce772)]
--[MinVersion(Hash = 0732f3e9e088b5795d5bf42af596d70a58f6e38eecc29ecd6c93e138cd3c6c7a)]
UPDATE APTran
SET BaseQty = Qty
WHERE InventoryID IS NULL
GO

-- Behavior of Return IN Transaction Type is changed for When Used Serial Numbers.
-- Need to update not finalized documents accordingly.
--[SmartExecute]
--[MinVersion(Hash = 618364f1c1424e3f921e1c560225bdbd66109f769773d51026f0360d5e77b45b)]
UPDATE t
SET UnassignedQty = t.BaseQty
FROM INTran t
INNER JOIN InventoryItem i ON i.InventoryID = t.InventoryID
INNER JOIN INLotSerClass c ON c.LotSerClassID = i.LotSerClassID
WHERE t.DocType = 'I' AND t.TranType = 'RET' AND t.Released = 0
	AND c.LotSerTrack IN ('S', 'L') AND c.LotSerAssign = 'U'

UPDATE l
SET UnassignedQty = l.BaseShippedQty
FROM SOShipLine l
INNER JOIN SOOrderTypeOperation o ON o.OrderType = l.OrigOrderType AND o.Operation = l.Operation
INNER JOIN InventoryItem i ON i.InventoryID = l.InventoryID
INNER JOIN INLotSerClass c ON c.LotSerClassID = i.LotSerClassID
WHERE l.Operation = 'R' AND o.INDocType = 'RET' AND l.Confirmed = 0
	AND c.LotSerTrack IN ('S', 'L') AND c.LotSerAssign = 'U'
GO

-- Set Completed flag in SOLine and SOLineSplit for Orders not requiring Shipment and for which SO Invoice is released
--[SmartExecute]
--[MinVersion(Hash = 8ed610e60cb601d3dd6272fb41e2eee609e00cbfa5ff4eeb5a5b40d67c5b3a34)]
UPDATE ol
SET Completed = 1
FROM SOLine ol
INNER JOIN SOOrderType tp ON tp.OrderType = ol.OrderType
INNER JOIN ARTran tr ON tr.SOOrderType = ol.OrderType AND tr.SOOrderNbr = ol.OrderNbr AND tr.SOOrderLineNbr = ol.LineNbr
WHERE ol.Completed = 0 AND tp.RequireShipping = 0 AND tr.Released = 1

UPDATE os
SET Completed = 1
FROM SOLineSplit os
INNER JOIN SOOrderType tp ON tp.OrderType = os.OrderType
INNER JOIN ARTran tr ON tr.SOOrderType = os.OrderType AND tr.SOOrderNbr = os.OrderNbr AND tr.SOOrderLineNbr = os.LineNbr
WHERE os.Completed = 0 AND tp.RequireShipping = 0 AND tr.Released = 1
GO

-- Set Completed flag in SOLine of Misc Charge type for which SO Invoice is released
--[MinVersion(Hash = 9c13b874066ba006a001b63fbfc941c278684b75a4104f1625eda18ead9d41a2)]
UPDATE ol
SET Completed = 1
FROM SOLine ol
INNER JOIN ARTran tr ON tr.CompanyID = ol.CompanyID AND tr.SOOrderType = ol.OrderType AND tr.SOOrderNbr = ol.OrderNbr AND tr.SOOrderLineNbr = ol.LineNbr
WHERE ol.Completed = 0 AND ol.LineType = 'MI' AND tr.Released = 1
GO

--[SmartExecute]
--[MinVersion(Hash = ccc33207856bf686cf5c145a81551476c4fc7ab509cbdf76c1453f7eaae759d6)]
UPDATE ic
SET [ItemClassCD] = LEFT(ic.[ItemClassCD] + REPLICATE(' ', d.[Length]), d.[Length])
FROM INItemClass ic
INNER JOIN Dimension d on d.[DimensionID] = 'INITEMCLASS'
WHERE [ItemClassCD] < LEFT(ic.[ItemClassCD] + REPLICATE('_', d.[Length]), d.[Length])
GO

-- Generate new name for obsolete USPS carrier plugin details
--[OldHash(Hash = 19922079e31d559df202739959612d75f3979ddbfc4d54b9d45ff7cdd1a22062)]
--[MinVersion(Hash = 203ca9c8db756f9c3230dc6c1a313e787eceda1ba4b86d2a2f87b91a7b448a61)]
declare @uid char(38)
declare @pluginID nvarchar(15)

declare carrier_cursor cursor for
select CarrierPluginID from CarrierPlugin where PluginTypeName = 'PX.UspsCarrier.UspsCarrier' and CompanyID > 0 group by CarrierPluginID

open carrier_cursor
fetch next from carrier_cursor into @pluginID

while @@FETCH_STATUS = 0
begin

set @uid = 'USPS' + LEFT(newid(), 8)

update d
set d.CarrierPluginID = RTRIM(@uid)
from CarrierPluginDetail d where d.CarrierPluginID = @pluginID and d.CompanyID > 0

-- Create new header for obsolete USPS carrier plugin details
insert into CarrierPlugin 
(
CompanyID, 
CarrierPluginID, 
Description, 
PluginTypeName, 
UnitType, 
UOM, 
DetailLineCntr, 
ReturnLabelNotification, 
SiteID, 
CreatedByID, 
CreatedByScreenID, 
CreatedDateTime, 
LastModifiedByID, 
LastModifiedByScreenID, 
LastModifiedDateTime,
NoteID
)
select 
d.CompanyID, 
RTRIM(@uid),
LEFT('Obsolete ' + d.CarrierPluginID + ' | ' + d.Description, 60), 
null, 
d.UnitType, 
d.UOM, 
d.DetailLineCntr, 
d.ReturnLabelNotification, 
d.SiteID, 
d.CreatedByID, 
d.CreatedByScreenID, 
d.CreatedDateTime, 
d.LastModifiedByID, 
d.LastModifiedByScreenID, 
d.LastModifiedDateTime,
NEWID()
from CarrierPlugin d where d.CarrierPluginID = @pluginID and d.CompanyID > 0

fetch next from carrier_cursor into @pluginID
end

close carrier_cursor
deallocate carrier_cursor
GO

-- Change PluginType for all USPS carriers to Stamps. All USPS ship-via codes will become invalid 
--[MinVersion(Hash = a924114287cdc7e53c50dde8803e18f83dfa1f2fdf28f46e4f98449cd8159dfe)]
update CarrierPlugin set PluginTypeName = 'PX.StampsCarrier.StampsCarrier' where PluginTypeName = 'PX.UspsCarrier.UspsCarrier'
GO

-- INRegister.TotalAmount should be zero for all inventory document types except Issues
--[MinVersion(Hash = 23b6ff4404908495bb2dfd633587b9fe3deac9b18b3e120de4cdf91e70303b6c)]
UPDATE INRegister
SET TotalAmount = 0
WHERE DocType <> 'I' AND TotalAmount <> 0
GO

-- BaseUnbilledQty were incorrect in some cases because of insufficient decimal precision
--[MinVersion(Hash = 18fad32a91bd0e232ee6cbaa2fd330a738ae460f539291f31e0ecdec5cf63fa6)]
update POReceiptLine
set BaseUnbilledQty = BaseReceiptQty
where BaseUnbilledQty <> 0 and abs(BaseUnbilledQty - BaseReceiptQty) < 0.0001 and BaseUnbilledQty <> BaseReceiptQty
GO

-- Set POCreated flag in SOLine
--[MinVersion(Hash = a3b130349dec80c23180ea5b5decdc9fd1f25d77fe9acb46660446d0c5b1d6b9)]
UPDATE line
SET line.POCreated = 1
FROM SOLine line
WHERE EXISTS(
	SELECT *
	FROM SOLineSplit split
	WHERE
		split.CompanyID = line.CompanyID
		AND split.OrderType = line.OrderType
		AND split.OrderNbr = line.OrderNbr
		AND split.LineNbr = line.LineNbr
		AND split.POCreate = 1
		AND split.PONbr is not null)
GO

-- Calculate POLine.BilledQty by APTran
--[MinVersion(Hash = 29707dfdaa104de1185d7b4013b56ece699d07a6374251f5ceccc25adb1b85c6)]
update pol
set	BilledQty = coalesce(aptg.QtySum, 0),
	BaseBilledQty = coalesce(aptg.BaseQtySum, 0)
from POLine pol
	left join (
		select apt.CompanyID, apt.POOrderType, apt.PONbr, apt.POLineNbr, max(apt.UOM) as MaxUom, min(apt.UOM) as MinUom,
			sum(apt.Qty*(case when apt.DrCr = 'D' then 1 else -1 end)) as QtySum,
			sum(coalesce(apt.BaseQty, apt.Qty)*(case when apt.DrCr = 'D' then 1 else -1 end)) as BaseQtySum
		from APTran apt
		where apt.Released = 1 and apt.POOrderType is not null and apt.PONbr is not null and apt.POLineNbr is not null
		group by apt.CompanyID, apt.POOrderType, apt.PONbr, apt.POLineNbr
	) aptg on aptg.CompanyID = pol.CompanyID and aptg.POOrderType = pol.OrderType and aptg.PONbr = pol.OrderNbr and aptg.POLineNbr = pol.LineNbr
where (pol.BilledQty is null or pol.BilledQty <> coalesce(aptg.QtySum, 0)
			or pol.BaseBilledQty is null or pol.BaseBilledQty <> coalesce(aptg.BaseQtySum, 0))
	and ((aptg.MinUom is null and aptg.MaxUom is null)
		or (aptg.MinUom = pol.UOM and aptg.MaxUom = pol.UOM))
GO

-- Calculate POLine.CompletedQty by POReceiptLine
--[MinVersion(Hash = 0611b405a2d06ccce6784710be0f0178488f87c4b9cc1c61ec1bd9a5e7152861)]
update pol
set CompletedQty = coalesce(porlg.QtySum, 0),
	BaseCompletedQty = coalesce(porlg.BaseQtySum, 0)
from POLine pol
	left join (
		select porl.CompanyID, porl.POType, porl.PONbr, porl.POLineNbr, max(porl.UOM) as MaxUom, min(porl.UOM) as MinUom,
			sum(porl.ReceiptQty*(case when porl.InvtMult < 0 then -1 else 1 end)) as QtySum,
			sum(coalesce(porl.BaseReceiptQty, porl.ReceiptQty)*(case when porl.InvtMult < 0 then -1 else 1 end)) as BaseQtySum
		from POReceiptLine porl
		where porl.Released = 1 and porl.POType is not null and porl.PONbr is not null and porl.POLineNbr is not null
		group by porl.CompanyID, porl.POType, porl.PONbr, porl.POLineNbr
	) porlg on porlg.CompanyID = pol.CompanyID and porlg.POType= pol.OrderType and porlg.PONbr = pol.OrderNbr and porlg.POLineNbr = pol.LineNbr
where (pol.CompletedQty is null or pol.CompletedQty <> coalesce(porlg.QtySum, 0)
		or pol.BaseCompletedQty is null or pol.BaseCompletedQty <> coalesce(porlg.BaseQtySum, 0))
	and ((porlg.MinUom is null and porlg.MaxUom is null)
		or (porlg.MinUom = pol.UOM and porlg.MaxUom = pol.UOM))
GO

-- Calculate POLine.BilledAmt by APTran
--[MinVersion(Hash = a02b1a95e38b763b45a6d18d09da62590dba9be2d31f247b1dd275e4eaadf2cc)]
update pol
set BilledAmt = coalesce(aptg.TranAmtSum, 0),
	CuryBilledAmt = coalesce(aptg.CuryTranAmtSum, 0)
from POLine pol
	inner join POOrder po on pol.CompanyID = po.CompanyID and pol.OrderType = po.OrderType and pol.OrderNbr = po.OrderNbr
	left join (
		select apt.CompanyID, apt.POOrderType, apt.PONbr, apt.POLineNbr,
			sum( (apt.TranAmt + coalesce(apt.RetainageAmt, 0))*(case when apt.DrCr = 'D' then 1 else -1 end) ) as TranAmtSum,
			sum( (apt.CuryTranAmt + coalesce(apt.CuryRetainageAmt, 0))*(case when apt.DrCr = 'D' then 1 else -1 end) ) as CuryTranAmtSum,
			min(apdoc.CuryID) as MinCuryID, max(apdoc.CuryID) as MaxCuryID
		from APTran apt
			inner join APRegister apdoc on apt.CompanyID = apdoc.CompanyID and apt.TranType = apdoc.DocType and apt.RefNbr = apdoc.RefNbr
		where apt.Released = 1 and apt.POOrderType is not null and apt.PONbr is not null and apt.POLineNbr is not null
		group by apt.CompanyID, apt.POOrderType, apt.PONbr, apt.POLineNbr
	) aptg on aptg.CompanyID = pol.CompanyID and aptg.POOrderType = pol.OrderType and aptg.PONbr = pol.OrderNbr and aptg.POLineNbr = pol.LineNbr
where (pol.BilledAmt is null or pol.BilledAmt <> coalesce(aptg.TranAmtSum, 0)
		or pol.CuryBilledAmt is null or pol.CuryBilledAmt <> coalesce(aptg.CuryTranAmtSum, 0))
	and ((aptg.MinCuryID is null and aptg.MaxCuryID is null)
		or (aptg.MinCuryID = po.CuryID and aptg.MaxCuryID = po.CuryID))
GO

-- Set Product Manager Override (INItemSite) ProductManagerID, ProductWorkGroupID.
--[MinVersion(Hash = 68ecb350d02aa6f3a713a783b553ce8195332bae1b9ed5bc4350ea531410d5ea)]
update INItemSite
set ProductManagerOverride = 1
where (ProductManagerID is not null or ProductWorkGroupID is not null)
	and ProductManagerOverride is null

update itemSite
set ProductManagerID = item.ProductManagerID,
	ProductWorkGroupID = item.ProductWorkGroupID
from INItemSite itemSite
	inner join InventoryItem item on itemSite.CompanyID = item.CompanyID and itemSite.InventoryID = item.InventoryID
where 
	(ProductManagerOverride = 0 or ProductManagerOverride is null)
	and (item.ProductManagerID is not null or item.ProductWorkGroupID is not null)
GO

--[MinVersion(Hash = 25e3516c1bb74da181b24370b48ffdf10684a1d893bc94efd086511762901fd4)]
delete units
from INUnit units
	inner join InventoryItem items on units.UnitType = 1 --re-run after AC-156437 fix
		and items.CompanyID = units.CompanyID
		and items.InventoryID = units.InventoryID
		and items.BaseUnit <> units.ToUnit
GO

--[MinVersion(Hash = f6fcfa6fce270bdc5ce1f4f03451152c8db285f5efe8b5fdfaf2ff69da6f0e59)]
delete units
from INUnit units
	inner join INItemClass ic on units.UnitType = 2 --re-run after AC-156437 fix
		and ic.CompanyID = units.CompanyID
		and ic.ItemClassID = units.ItemClassID
		and ic.BaseUnit <> units.ToUnit
GO

-- AC-118647. Add OrigDocType column to INTran
--[MinVersion(Hash = 04e8324cfc3baa75da74843e72c11319aa5f7c3d6850c63343fd1220928fa7d8)]
update t
set t.OrigDocType = ot.DocType
from INTran t
	inner join INTran ot 
		on t.CompanyID = ot.CompanyID
		and t.OrigTranType = ot.TranType
		and t.OrigRefNbr = ot.RefNbr 
		and t.OrigLineNbr = ot.LineNbr
where t.OrigDocType is null
GO

-- AC-118647. Add OrigDocType column to POReceiptLine
--[MinVersion(Hash = a2391e3ead11eac2f0c837f460c07cf1fb7f19a32852657e1af2d8e5b35c0b2a)]
update rl
set rl.OrigDocType = t.DocType
from POReceiptLine rl
	inner join INTran t 
		on rl.CompanyID = t.CompanyID
		and rl.OrigTranType = t.TranType
		and rl.OrigRefNbr = t.RefNbr 
		and rl.OrigLineNbr = t.LineNbr
where rl.OrigDocType is null
GO

-- AC-118647. Add DocType column to INTranCost
--[MinVersion(Hash = 4eed4bfa11c984fb729791554e15b577658dd6c715bd2624d080b8bd6b50bfb9)]
update tc
set tc.DocType = t.DocType
from INTranCost tc
	inner join INTran t 
		on t.CompanyID = tc.CompanyID
		and t.TranType = tc.TranType
		and t.RefNbr = tc.RefNbr
		and t.LineNbr = tc.LineNbr
where tc.DocType is null
GO

-- AC-118647. Calculate DocType by TranType to INTranCost
--[MinVersion(Hash = 3f2cae5b48a20448dcef93a16dd9c92542667b852fcca783a0721eaf2e8154ca)]
update tc
set tc.DocType = case tc.TranType 
	when 'ADJ' then 'A'
	when 'ASC' then 'A'
	when 'NSC' then 'A'
	when 'RCA' then 'A'
	when 'ASY' then 'P'
	when 'DSY' then 'D'
	when 'RCP' then 'R'
	when 'TRX' then 'T'
	when 'III' then 'I'
	when 'RET' then 'I'
	when 'INV' then 'I'
	when 'DRM' then 'I'
	when 'CRM' then 'I'
	else '0' end
from INTranCost tc
where tc.DocType is null
GO

-- AC-126987 Support PI upgrade scenario: filling backward reference from Adjustment to PI.
--[MinVersion(Hash = 71fa8703461e0a41a60c5c04878f27e94141f9bcd8aa6b7b0cff75cce6f06ed3)]
UPDATE R
SET R.PIID = H.PIID
FROM INRegister R
INNER JOIN INPIHeader H
	ON R.CompanyID = H.CompanyID
	AND R.DocType = 'A'
	AND R.RefNbr = H.PIAdjRefNbr
GO

-- AC-126987 Support PI upgrade scenario: filling backward reference from Adjustment to PI.
--[MinVersion(Hash = 4109b2919b4fdddc19955018e652c8d901be2701aa9c89fd78a6f9062d1337b9)]
UPDATE H
SET H.Status = 'R'
FROM INPIHeader H
INNER JOIN INRegister R
	ON R.CompanyID = H.CompanyID
	AND R.DocType = 'A'
	AND R.RefNbr = H.PIAdjRefNbr
WHERE H.Status = 'C' AND R.Status IN ('B', 'H')
GO

-- AC-127882 Ability to hide Book Qty in "Physical Count Sheets" Report and om "PI Count" Form.
--[MinVersion(Hash = 0f5fbaf97adfbe91d448f47398e32786471edec72ca678097ed7822d04730867)]
UPDATE INPIClass
SET HideBookQty = 0
GO

-- AC-130691: synchronize PackedQty of SOShipLines with PackedQty of SOShipLineSplits
--[MinVersion(Hash = 9140f82efe1d5d5fcd8b4bc75378140066bbef90aa7feb388c8738342703b6e3)]
update sl
set
	BasePackedQty = sls.BasePackedQty,
	PackedQty = ROUND(case when u.UnitMultDiv = 'D' then sls.BasePackedQty * u.UnitRate else sls.BasePackedQty / u.UnitRate end, cs.DecPlQty)
from SOShipLine sl
join CommonSetup cs on cs.CompanyID = sl.CompanyID
join InventoryItem i on i.CompanyID = sl.CompanyID and i.InventoryID = sl.InventoryID
join INUnit u on u.CompanyID = sl.CompanyID and u.InventoryID = i.InventoryID and u.UnitType = 1 and u.FromUnit = sl.UOM and u.ToUnit = i.BaseUnit
join (	select CompanyID, ShipmentNbr, LineNbr, sum(BasePackedQty) as BasePackedQty
		from SOShipLineSplit
		where IsUnassigned = 0 and BasePackedQty <> 0
		group by CompanyID, ShipmentNbr, LineNbr) as sls
	on sl.CompanyID = sls.CompanyID and sl.ShipmentNbr = sls.ShipmentNbr and sl.LineNbr = sls.LineNbr
GO

-- AC-106291: Check for duplicates on the 'Customer Order' field on Sales Order entry screen.
--[MinVersion(Hash = fc942f5c68679df11e11cc167b4453a481b6937faede7bc41d436fe291f03476)]
UPDATE SOOrderType
SET CustomerOrderValidation = 'N'
GO

-- Insert POOrderPrepayment records for Prepayment Requests created before 2019R2
--[IfExists(Column = POOrder.PrepaymentDocType)]
--[IfExists(Column = POOrder.PrepaymentRefNbr)]
--[MinVersion(Hash = acfa572b47a56ff3ab72cf9b694626e807481eba883739aec80400c4d91b754e)]
INSERT INTO POOrderPrepayment (
	CompanyID, OrderType, OrderNbr,
	APDocType, APRefNbr, CuryInfoID,
	IsRequest, CuryLineTotal, LineTotal,
	CuryAppliedAmt,
	AppliedAmt,
	CreatedByID, CreatedByScreenID, CreatedDateTime,
	LastModifiedByID, LastModifiedByScreenID, LastModifiedDateTime)
SELECT po.CompanyID, po.OrderType, po.OrderNbr,
	apr.DocType, apr.RefNbr, po.CuryInfoID,
	1, api.CuryLineTotal, api.LineTotal,
	CASE WHEN apr.Released = 1 AND apr.Voided = 0 THEN apr.CuryOrigDocAmt ELSE 0 END,
	CASE WHEN apr.Released = 1 AND apr.Voided = 0 THEN apr.OrigDocAmt ELSE 0 END,
	apr.CreatedByID, apr.CreatedByScreenID, apr.CreatedDateTime,
	apr.LastModifiedByID, apr.LastModifiedByScreenID, apr.LastModifiedDateTime
FROM APRegister apr
INNER JOIN APInvoice api ON api.CompanyID = apr.CompanyID AND api.DocType = apr.DocType AND api.RefNbr = apr.RefNbr
INNER JOIN POOrder po ON po.CompanyID = apr.CompanyID AND po.PrepaymentDocType = apr.DocType AND po.PrepaymentRefNbr = apr.RefNbr
LEFT JOIN POOrderPrepayment op
ON op.CompanyID = apr.CompanyID AND op.APDocType = apr.DocType AND op.APRefNbr = apr.RefNbr
	AND op.OrderType = po.OrderType AND op.OrderNbr = po.OrderNbr
WHERE op.CompanyID IS NULL AND apr.DocType = 'PPM'
GO

-- Update POOrder Prepayment Total Amount for released Prepayment Requests created before 2019R2
--[IfExists(Column = POOrder.PrepaymentDocType)]
--[IfExists(Column = POOrder.PrepaymentRefNbr)]
--[MinVersion(Hash = 1692d08cca771a044b13ea8a12614aa18f0e0591fca6f406a552da16cf9336cc)]
UPDATE po
SET CuryPrepaidTotal = apr.CuryDocBal,
	PrepaidTotal = apr.DocBal
FROM POOrder po
INNER JOIN APRegister apr ON apr.CompanyID = po.CompanyID AND apr.DocType = po.PrepaymentDocType AND apr.RefNbr = po.PrepaymentRefNbr
WHERE apr.Released = 1 AND apr.Voided = 0
	AND (po.CuryPrepaidTotal IS NULL OR po.PrepaidTotal IS NULL)
GO

-- Update POOrder Unpaid Amount for released Prepayment Requests created before 2019R2
--[MinVersion(Hash = fc2ff001afe8c5e4b545e7994e7238bb8df10eb3700497d63b28ed44044b791b)]
UPDATE POOrder
SET CuryUnprepaidTotal = ISNULL(CuryUnbilledOrderTotal, 0) - ISNULL(CuryPrepaidTotal, 0),
	UnprepaidTotal = ISNULL(UnbilledOrderTotal, 0) - ISNULL(PrepaidTotal, 0)
GO

-- Update Original Module to PO for Prepayment Requests related to PO and created before 2019R2
--[MinVersion(Hash = 601a08d879dd959426e1d55767ef002aa37e3b257fc04682289e67ec519c9e7d)]
UPDATE apr
SET OrigModule = 'PO'
FROM APRegister apr
INNER JOIN POOrderPrepayment op
ON op.CompanyID = apr.CompanyID AND op.APDocType = apr.DocType AND op.APRefNbr = apr.RefNbr
WHERE apr.DocType = 'PPM' AND (apr.OrigModule IS NULL OR apr.OrigModule <> 'PO')
GO

-- AC-135631: Fix Note.entityType when user added attachment to SOOrder on SOShipment screen. (through SOOrderShipment and grid).
--[MinVersion(Hash = df3336310e5377aca96e5f9d1ffdac88220c5034f1298ab88b8a0ffd64f65c5f)]
update n
set EntityType = 'PX.Objects.SO.SOOrder'
from dbo.Note n
    inner join dbo.SOOrder o on o.CompanyID = n.CompanyID and o.NoteID = n.NoteID
where n.EntityType = 'PX.Objects.SO.SOOrderShipment'
GO

-- AC-140925: Fix Note.entityType when user added attachment to SOShipment on SOOrder screen. (through SOOrderShipment and grid).
--[MinVersion(Hash = 7a48c6b235756819f3b79b91c98554cb4ffafe7c62df8f5783eeb4e518ef2bdc)]
update n
set EntityType = 'PX.Objects.SO.SOShipment'
from dbo.Note n
	inner join dbo.SOShipment s on s.CompanyID = n.CompanyID and s.NoteID = n.NoteID
where n.EntityType = 'PX.Objects.SO.SOOrderShipment'
GO

-- AC-140925: Fix Note.entityType when user added attachment to ARInvoice on SOOrder screen. (through SOOrderShipment and grid).
--[MinVersion(Hash = 36d545adcb51c7b4accd1e9fc3a96174769a28ad2772b6c7fcefb80537f9010c)]
update n
set EntityType = 'PX.Objects.SO.ARInvoice'
from dbo.Note n
	inner join dbo.ARRegister r on r.CompanyID = n.CompanyID and r.NoteID = n.NoteID
where n.EntityType = 'PX.Objects.SO.SOOrderShipment'
GO

-- AC-140925: Fix Note.entityType when user added attachment to POOrder on POReceipt screen. (through POOrderReceipt and grid).
--[MinVersion(Hash = 0027444b290a45f97feb3dcd58c0e8036862a1311b8169227b1e1ebb8b30f5dc)]
update n
set EntityType = 'PX.Objects.SO.POOrder'
from dbo.Note n
	inner join dbo.POOrder o on o.CompanyID = n.CompanyID and o.NoteID = n.NoteID
where n.EntityType = 'PX.Objects.PO.POOrderReceipt'
GO

-- AC-134353. Can't change UOM for the item, if it is referenced in a cancelled requisition.
--[MinVersion(Hash = 5a60e18af80e9c4f46355e168075abbe7b0fefa7acf44697d49b15bce7823e20)]
UPDATE RQRequisition
SET Cancelled = 1
WHERE [Status] = 'L'
GO

-- AC-133006: Can not delete non stock item due to link from Kit Specification Detail
--[MinVersion(Hash = ed748ce5f52f621bb431f393c11a72e59642bb46b3cb470abc1f27d7c2b29246)]
DELETE D
FROM INKitSpecStkDet D
LEFT JOIN INKitSpecHdr H
	ON H.CompanyID = D.CompanyID
	AND H.KitInventoryID = D.KitInventoryID
	AND H.RevisionID = D.RevisionID
WHERE H.KitInventoryID IS NULL

DELETE D
FROM INKitSpecNonStkDet D
LEFT JOIN INKitSpecHdr H
	ON H.CompanyID = D.CompanyID
	AND H.KitInventoryID = D.KitInventoryID
	AND H.RevisionID = D.RevisionID
WHERE H.KitInventoryID IS NULL
GO

-- Update Payment Ref. in PO Prepayments
--[MinVersion(Hash = d630364ab6aee5b6eceeac3f099eea767e02f21262eef5d7b84c096c87965af0)]
UPDATE pop
SET PayDocType = apd.AdjgDocType, PayRefNbr = apd.AdjgRefNbr
FROM POOrderPrepayment pop
INNER JOIN APAdjust apd ON apd.CompanyID = pop.CompanyID AND apd.AdjdDocType = pop.APDocType AND apd.AdjdRefNbr = pop.APRefNbr
WHERE pop.PayDocType IS NULL AND pop.PayRefNbr IS NULL AND pop.IsRequest = 1
	AND apd.Released = 1 AND apd.CuryAdjdAmt > 0

UPDATE pop
SET PayDocType = pod.AdjgDocType, PayRefNbr = pod.AdjgRefNbr
FROM POOrderPrepayment pop
INNER JOIN POAdjust pod ON pod.CompanyID = pop.CompanyID AND pod.AdjgDocType = pop.APDocType AND pod.AdjgRefNbr = pop.APRefNbr
	AND pod.AdjdOrderType = pop.OrderType AND pod.AdjdOrderNbr = pop.OrderNbr
WHERE pop.PayDocType IS NULL AND pop.PayRefNbr IS NULL AND pop.IsRequest = 0
	AND pod.Released = 1 AND pod.CuryAdjdAmt > 0 AND pod.IsRequest = 0
GO

-- AC-131556: Introducing new SOShipment.CustomerOrderNbr field. It will be filled if shipment has only one SOOrder. Refer to AC-61417.
--[MinVersion(Hash = 8da5863de655db2e105f82fb30d3496bfb2f7ec1968b9616280817f11ce5d28e)]
UPDATE S
SET S.CustomerOrderNbr = O.CustomerOrderNbr
FROM SOShipment S
INNER JOIN SOOrderShipment OS
	ON OS.CompanyID = S.CompanyID
	AND OS.ShipmentType = S.ShipmentType
	AND OS.ShipmentNbr = S.ShipmentNbr
INNER JOIN SOOrder O
	ON O.CompanyID = OS.CompanyID
	AND O.OrderType = OS.OrderType
	AND O.OrderNbr = OS.OrderNbr
WHERE S.OrderCntr = 1
GO

-- AC-131556: Introducing new SOShipment.IsManualPackage flag. Now shipments should be divided by SOOrder.IsManualPackage value. Refer to AC-27233.
--[MinVersion(Hash = 6a43e40f7ae6eeb764e271bbdf7b076adc8d10fe323e2834e7057edc6c14701c)]
UPDATE S
SET S.IsManualPackage = SO.IsManualPackage
FROM SOShipment S
INNER JOIN
(
	SELECT OS.CompanyID, OS.ShipmentNbr, OS.ShipmentType, MAX(CAST(O.IsManualPackage AS INT)) AS IsManualPackage
	FROM SOOrderShipment OS
	INNER JOIN SOOrder O
		ON O.CompanyID = OS.CompanyID
		AND O.OrderType = OS.OrderType
		AND O.OrderNbr = OS.OrderNbr
	GROUP BY OS.CompanyID, OS.ShipmentNbr, OS.ShipmentType
	HAVING MAX(CAST(O.IsManualPackage AS INT)) = MIN(CAST(O.IsManualPackage AS INT))
) SO
	ON SO.CompanyID = S.CompanyID
	AND SO.ShipmentNbr = S.ShipmentNbr
	AND SO.ShipmentType = S.ShipmentType
GO

-- AC-131556: Introducing new SOOrderShipment.CreateARDoc flag.
--[SmartExecute]
--[MinVersion(Hash = 4596cdcf27d4ea3e75b83b0394136b58eb08bec1797b27dabfe3840515d6a159)]
UPDATE OS
SET OS.CreateARDoc = CASE WHEN OT.ARDocType <> 'UND' THEN 1 ELSE 0 END
FROM SOOrderShipment OS
INNER JOIN SOOrderType OT
	ON OS.OrderType = OT.OrderType
GO

-- Rename SOOrderShipment column: NoteID -> OrderNoteID
--[IfExists(Column = SOOrderShipment.NoteID)]
--[MinVersion(Hash = 0d8a86236bea7d1da456b3664a85f7e2a49889f46796e51a903bc5b97be76aa4)]
UPDATE SOOrderShipment
SET OrderNoteID = NoteID
GO

-- AC-126239: Fill in LocationCode in the GetTax calls to AvaTax with WarehouseID.
--[MinVersion(Hash = 3e883770c805c1f88a31391bcfc5c47413858b13a9b2cf5c34b224b69cf1e352)]
UPDATE APT
SET APT.SiteID = ISNULL(PRL.SiteID, PL.SiteID)
FROM APTran APT
LEFT JOIN POLine PL
	ON PL.CompanyID = APT.CompanyID
	AND PL.OrderType = APT.POOrderType
	AND PL.OrderNbr = APT.PONbr
	AND PL.LineNbr = APT.POLineNbr
LEFT JOIN POReceiptLine PRL
	ON PRL.CompanyID = APT.CompanyID
	AND PRL.ReceiptNbr = APT.ReceiptNbr
	AND PRL.LineNbr = APT.ReceiptLineNbr
WHERE (APT.POLineNbr IS NOT NULL
	OR APT.ReceiptLineNbr IS NOT NULL)
	AND APT.SiteID IS NULL
GO

--AC-137096: Enable a freight calculation for internal carriers
--[MinVersion(Hash = 8c5ec471455cc6e58907b5769614dfbe2a997f19422ae7ea9762508660de9045)]
update Carrier
set CalcFreightOnReturn = 1
where coalesce(IsExternal, 0) = 0
GO

--[MinVersion(Hash = f82383afcc6fdc1d1d90b68242e8a995397e6917f859ea99b0ef793a194c294c)]
UPDATE APTran
SET PrepaymentPct = 100,
	CuryPrepaymentAmt = CuryLineAmt - CuryDiscAmt,
	PrepaymentAmt = LineAmt - DiscAmt
WHERE TranType = 'PPM'
GO

-- AC-139156: Set zero to POAccrualStatus.CuryBilledTaxAdjCost
--[MinVersion(Hash = d9e83a80ddb1ab1cae49e0a89d1310c66126fdbdcba6fcf04f9be8bdb56f234b)]
update POAccrualStatus
set CuryBilledTaxAdjCost = 0
where CuryBilledTaxAdjCost is null
GO

--AC-137442 Invert value for UseItemDefaultLocationForPicking/PickByLocationsPickPriority
--[IfExists(Column = INSite.PickByLocationsPickPriority)]
--[MinVersion(Hash = e0b53e94826c413a444c217f65287c0cda19087d1f6c44cdbbcfaa4d85bf6200)]
update INSite
set UseItemDefaultLocationForPicking = 1 - PickByLocationsPickPriority
GO

--AC-140204 - Remove obsolete inventory plans from PO
--[MinVersion(Hash = 576f75753eb2e30af0c6d6288ca606ba4130355827a9e1e9e4a26bbeaafcfe1e)]
delete p
from INItemPlan p
inner join POLine l
	on l.CompanyID = p.CompanyID and l.PlanID = p.PlanID
left join INItemPlan d
	on d.CompanyID = p.CompanyID and d.SupplyPlanID = p.PlanID
where p.PlanQty = 0 and d.PlanID is null and (l.Completed = 1 or l.Cancelled = 1)
GO

--[MinVersion(Hash = 00f63bfbcbc1589501d23a79a3c309ba9c0bdda1218671dca6b2745d133ec09d)]
delete p
from INItemPlan p
inner join POOrder o
	on o.CompanyID = p.CompanyID and o.NoteID = p.RefNoteID
left join INItemPlan d
	on d.CompanyID = p.CompanyID and d.SupplyPlanID = p.PlanID
where p.PlanQty = 0 and d.PlanID is null and o.Status in ('C', 'L', 'M')
GO

-- AC-143602: Set non-zero POOrder.UnbilledOrderQty and POOrder.CuryUnbilledOrderTotal in order to avoid the issue with AP Bills created without taxes.
-- This script is supposed to execute only on upgrade from 2018R1 to 2018R2 or 2019R1, POOrder.CuryOpenOrderTotal and POReceiptTax are added for that purpose.
--[IfExists(Column = POOrder.CuryUnbilledOrderTotal)]
--[IfExists(Column = POOrder.UnbilledOrderTotal)]
--[IfExists(Column = POOrder.CuryOpenOrderTotal)]
--[IfExists(Column = POOrder.UnbilledOrderQty)]
--[IfExists(Table = POReceiptTax)]
--[MinVersion(Hash = 8ada3f12356bc87794fa1697e3132486f15782ed480b972c01cd5988c98edbe4)]
update po
set UnbilledOrderQty = POReceiptLineGroup.UnbilledQtySum
from POOrder po
inner join
(	select pl.CompanyID, pl.OrderType, pl.OrderNbr, sum(prl.UnbilledQty) as UnbilledQtySum
	from POLine pl
	inner join POReceiptLine prl on prl.CompanyID = pl.CompanyID and prl.POType = pl.OrderType and prl.PONbr = pl.OrderNbr and prl.POLineNbr = pl.LineNbr
	where prl.UnbilledQty > 0
	group by pl.CompanyID, pl.OrderType, pl.OrderNbr
) POReceiptLineGroup
on po.CompanyID = POReceiptLineGroup.CompanyID and po.OrderType = POReceiptLineGroup.OrderType and po.OrderNbr = POReceiptLineGroup.OrderNbr
where po.OrderQty > 0 and po.UnbilledOrderQty = 0

update po
set CuryUnbilledOrderTotal = po.CuryOrderTotal, UnbilledOrderTotal = po.OrderTotal
from POOrder po
left join APTran ap on po.CompanyID = ap.CompanyID and po.OrderType = ap.POOrderType and po.OrderNbr = ap.PONbr
where ap.RefNbr is null and po.OrderType in ('RO','DS')
	and po.CuryOrderTotal > 0 and po.CuryUnbilledOrderTotal = 0
GO

--[MinVersion(Hash = d0e08e2a8058a0e46e784fb0f779e41a1b2b5e871c2bd0190cfa9f839380a652)]
UPDATE SOShipmentSplitToCartSplitLink
SET
  CreatedByID = '00000000-0000-0000-0000-000000000000',
  CreatedByScreenID = '00000000',
  CreatedDateTime = GETDATE(),
  LastModifiedByID = '00000000-0000-0000-0000-000000000000',
  LastModifiedByScreenID = '00000000',
  LastModifiedDateTime = GETDATE()
WHERE CreatedByID IS NULL
GO

-- AC-145657: Filling APPrintCheckDetail for historical data.
--[MinVersion(Hash = 03549629a5f89510e428d73331983ada01c939e6780fe36e963a5ee64ce77f39)]
insert APPrintCheckDetail (
	[CompanyID],
	[AdjgDocType],
	[AdjgRefNbr],
	[Source],
	[AdjdDocType],
	[AdjdRefNbr],
	[CashAccountID],
	[PaymentMethodID],
	[StubNbr],
	[AdjgCuryInfoID],
	[AdjdCuryInfoID],
	[CuryOutstandingBalance],
	[OutstandingBalance],
	[OutstandingBalanceDate],
	[CuryAdjgAmt],
	[AdjgAmt],
	[CuryAdjgDiscAmt],
	[AdjgDiscAmt],
	[CuryExtraDocBal],
	[ExtraDocBal],
	[CreatedByID],
	[CreatedByScreenID],
	[CreatedDateTime],
	[LastModifiedByID],
	[LastModifiedByScreenID],
	[LastModifiedDateTime])
select
	[APPayment].CompanyID as [CompanyID],
	[APPayment].[DocType] as [AdjgDocType],
	[APPayment].[RefNbr] as [AdjgRefNbr],
	'A' as [Source],
	[APAdjust].[AdjdDocType] as [AdjdDocType],
	[APAdjust].[AdjdRefNbr] as [AdjdRefNbr],
	[APPayment].[CashAccountID] as [CashAccountID],
	[APPayment].[PaymentMethodID] as [PaymentMethodID],
	[APPayment].[ExtRefNbr] as [StubNbr],
	max([APAdjust].AdjgCuryInfoID) as [AdjgCuryInfoID],
	max([APAdjust].AdjdCuryInfoID) as [AdjdCuryInfoID],
	null as [CuryOutstandingBalance],
	null as [OutstandingBalance],
	null as [OutstandingBalanceDate],
	sum([APAdjust].[CuryAdjgAmt]) as [CuryAdjgAmt],
	sum([APAdjust].[AdjAmt]) as [AdjgAmt],
	sum([APAdjust].[CuryAdjgDiscAmt]) as [CuryAdjgDiscAmt],
	sum([APAdjust].[AdjDiscAmt]) as [AdjgDiscAmt],
	0 as [CuryExtraDocBal],
	0 as [ExtraDocBal],
	[APPayment_APRegister].[CreatedByID] as [CreatedByID],
	[APPayment_APRegister].[CreatedByScreenID] as [CreatedByScreenID],
	[APPayment_APRegister].[CreatedDateTime] as [CreatedDateTime],
	[APPayment_APRegister].[LastModifiedByID] as [LastModifiedByID],
	[APPayment_APRegister].[LastModifiedByScreenID] as [LastModifiedByScreenID],
	[APPayment_APRegister].[LastModifiedDateTime] as [LastModifiedDateTime]
from [APPayment]
	inner join [APRegister] [APPayment_APRegister] on
		[APPayment].[CompanyID] = [APPayment_APRegister].[CompanyID]
		and [APPayment].[DocType] = [APPayment_APRegister].[DocType]
		and [APPayment].[RefNbr] = [APPayment_APRegister].[RefNbr]
	inner join [APAdjust] on 
		[APPayment].[CompanyID] = [APAdjust].[CompanyID]
		and [APPayment].[DocType] = [APAdjust].[AdjgDocType]
		and [APPayment].[RefNbr] = [APAdjust].[AdjgRefNbr]
	left join [APPrintCheckDetail] on
		[APPayment].[CompanyID] = [APPrintCheckDetail].[CompanyID]
		and [APPayment].[DocType] = [APPrintCheckDetail].[AdjgDocType]
		and [APPayment].[RefNbr] = [APPrintCheckDetail].[AdjgRefNbr]
		and [APPrintCheckDetail].[Source] = 'A'
		and [APAdjust].[AdjdDocType] = [APPrintCheckDetail].[AdjdDocType]
		and [APAdjust].[AdjdRefNbr] = [APPrintCheckDetail].[AdjdRefNbr]
where [APPayment_APRegister].[Hold] = 0
	and [APPayment_APRegister].[Voided] = 0
	and [APPayment].[ExtRefNbr] is not null
	and [APPrintCheckDetail].[CompanyID] is null
group by
	[APPayment].[CompanyID],
	[APPayment].[DocType],
	[APPayment].[RefNbr],
	[APAdjust].[AdjdDocType],
	[APAdjust].[AdjdRefNbr],
	[APPayment].[CashAccountID],
	[APPayment].[PaymentMethodID],
	[APPayment].[ExtRefNbr],
	[APPayment_APRegister].[CreatedByID],
	[APPayment_APRegister].[CreatedByScreenID],
	[APPayment_APRegister].[CreatedDateTime],
	[APPayment_APRegister].[LastModifiedByID],
	[APPayment_APRegister].[LastModifiedByScreenID],
	[APPayment_APRegister].[LastModifiedDateTime]
having sum([APAdjust].[CuryAdjgAmt]) != 0
GO

-- AC-151867: Adding audit fields
--[MinVersion(Hash = 95d4ea010b66b5526754ebf65271515003f2c7fa9d38e828bc5e3c9a00eb6ad3)]
update APVendorRefNbr 
set 
	CreatedByID = '00000000-0000-0000-0000-000000000000',
	CreatedByScreenID = '00000000',
	CreatedDateTime = GETDATE()
where CreatedByID is null
GO

--[SmartExecute]
--[MinVersion(Hash = d848e3e4b8ef2798de2ba75725a25ffbb5e4791b7c1a05cba6640069fd70ddf8)]
UPDATE setup
SET PickingWorksheetNumberingID = ns.NumberingID
FROM SOSetup setup
INNER JOIN Numbering ns ON ns.NumberingID = 'PICKWORKSH'
WHERE setup.PickingWorksheetNumberingID IS NULL
GO

-- AC-150598 Add NoteID field to ARSalesPrice: set NoteID value
--[MinVersion(Hash = 927628a78ec3ea2ea82a2e6538c7a37911513ead0d98e7aa90357236d92ed883)]
UPDATE ARSalesPrice SET NoteID = NEWID()
GO

-- AC-150598 Add NoteID field to ARSalesPrice: insert Note records
--[MinVersion(Hash = a450947a5e35e32e9869268bde21321e908df80fc90fe5e38129811ae1365580)]
insert into Note (CompanyID, NoteText, GraphType, EntityType, NoteID)
select sp.CompanyID, '', 'PX.Objects.AR.ARSalesPriceMaint', 'PX.Objects.AR.ARSalesPrice', sp.NoteID 
from ARSalesPrice sp
	left join Note n on n.CompanyID = sp.CompanyID and n.NoteID = sp.NoteID
where n.NoteID is null and sp.CompanyID > 0
GO

--[IfExists(Table = MQMAItemGroupHdr)]
--[IfExists(Table = MQMAItemGroupDet)]
--[IfExists(Table = MQMAInventoryExt)]
--[IfExists(Table = MQMAInventoryAttr)]
--[IfExistsSelect(From = MQMAItemGroupHdr)]
--[IfExistsSelect(From = MQMAItemGroupDet)]
--[IfExistsSelect(From = MQMAInventoryExt)]
--[IfExistsSelect(From = MQMAInventoryAttr)]
--[MinVersion(Hash = bc6477fcdaf060bf31f6a539a7c2535cafe7a9c3ae9bacdf0eaaa987815d5fbd)]
RAISERROR( N'MM> Please contact Acumatica Support for assisting in the upgrade of MaxQ Matrix Inventory solution', 15, -1)
GO

-- AC-114046: Avalara Entity Usage Type on the SO line level
--[MinVersion(Hash = f77d2425c820aa3e3a049efe46b657dfa2aaa2a4feec3001e6f4a0dfad268f5e)]
UPDATE L
SET L.AvalaraCustomerUsageType = O.AvalaraCustomerUsageType
FROM SOLine L
INNER JOIN SOOrder O
	ON O.CompanyID = L.CompanyID
	AND O.OrderType = L.OrderType
	AND O.OrderNbr = L.OrderNbr
GO

--[MinVersion(Hash = f012430fcd0fefe08f7f08d6d124a3a4a42095e160790a9e55ac4599fc181fa9)]
UPDATE T
SET T.AvalaraCustomerUsageType = I.AvalaraCustomerUsageType
FROM ARTran T
INNER JOIN ARInvoice I
	ON I.CompanyID = T.CompanyID
	AND I.DocType = T.TranType
	AND I.RefNbr = T.RefNbr
GO

-- AC-141729: Adding audit fields
--[MinVersion(Hash = eaaafe5d401d51eb44826b908ba66881ba9c6102c3dd752617cffb19d9eb9ec2)]
update INItemPlan set 
"CreatedByID" = '00000000-0000-0000-0000-000000000000',
"CreatedByScreenID" = '00000000',
"CreatedDateTime" = GETDATE(),
"LastModifiedByID" = '00000000-0000-0000-0000-000000000000',
"LastModifiedByScreenID" = '00000000',
"LastModifiedDateTime" = GETDATE()
where "CreatedByID" is null
GO

--AC-151147 - Fill INItemPlan.RefEntityType from Note.EntityType
--[MinVersion(Hash = ada1a034d41c233c5273fd17804a04e8a944732024b7e9c06b92ad769ea7b419)]
update p
set RefEntityType = isnull(n.EntityType, '')
from INItemPlan p
	left join Note n on p.RefNoteID = n.NoteID and p.CompanyID = n.CompanyID
GO

--AC-151147 - Fill INItemPlan.RefEntityType from SOOrder
--[MinVersion(Hash = 96b411f561885c5583f9cd10e9bbb8e1ba893aa63e2c42aca89c3ddd83612677)]
update p
set RefEntityType = 'PX.Objects.SO.SOOrder'
from INItemPlan p
	inner join SOOrder o on p.RefNoteID = o.NoteID and p.CompanyID = o.CompanyID
where p.RefEntityType = ''
GO

--AC-151147 - Fill INItemPlan.RefEntityType from SOShipment
--[MinVersion(Hash = d8871d6a14bd7fcf70e6513b2c179df3f209d8b5d7d1d9fa80d400d60dc5b24e)]
update p
set RefEntityType = 'PX.Objects.SO.SOShipment'
from INItemPlan p
	inner join SOShipment s on p.RefNoteID = s.NoteID and p.CompanyID = s.CompanyID
where p.RefEntityType = ''
GO

--AC-151147 - Fill INItemPlan.RefEntityType from POOrder
--[MinVersion(Hash = fd80751782d445a6ab888b744fc815716ff075d9e0d8ec7a19c8a14a70647ec8)]
update p
set RefEntityType = 'PX.Objects.PO.POOrder'
from INItemPlan p
	inner join POOrder o on p.RefNoteID = o.NoteID and p.CompanyID = o.CompanyID
where p.RefEntityType = ''
GO

--AC-151147 - Fill INItemPlan.RefEntityType from POReceipt
--[MinVersion(Hash = 169c8a096f6bffd01d525aeba8bb95e78fe2361044b9f8d3293915dac9bc1878)]
update p
set RefEntityType = 'PX.Objects.PO.POReceipt'
from INItemPlan p
	inner join POReceipt r on p.RefNoteID = r.NoteID and p.CompanyID = r.CompanyID
where p.RefEntityType = ''
GO

--AC-151147 - Fill INItemPlan.RefEntityType from INTransitLine
--[MinVersion(Hash = 81a1f30a7a8a29eff043eed7dbdf646d9d7f37244a30bdc50de6f052a753e919)]
update p
set RefEntityType = 'PX.Objects.IN.INTransitLine'
from INItemPlan p
	inner join INTransitLine l on p.RefNoteID = l.NoteID and p.CompanyID = l.CompanyID
where p.RefEntityType = ''
GO

--AC-151147 - Fill INItemPlan.RefEntityType from INReplenishmentOrder
--[MinVersion(Hash = 951c50f8411306cd196139f7d774d42826c7530ddf3380e5d4c7f466c7f5aa48)]
update p
set RefEntityType = 'PX.Objects.IN.INReplenishmentOrder'
from INItemPlan p
	inner join INReplenishmentOrder o on p.RefNoteID = o.NoteID and p.CompanyID = o.CompanyID
where p.RefEntityType = ''
GO

--AC-151147 - Fill INItemPlan.RefEntityType from INRegister
--[MinVersion(Hash = a161a246e9840b322a2a1c11dbf8400bb70ac38abd92ab136ebece41417c1676)]
update p
set RefEntityType = case when r.DocType in ('P', 'D') then 'PX.Objects.IN.INKitRegister' else 'PX.Objects.IN.INRegister' end
from INItemPlan p
	inner join INRegister r on p.RefNoteID = r.NoteID and p.CompanyID = r.CompanyID
where p.RefEntityType = ''
GO

--[MinVersion(Hash = d0e08e2a8058a0e46e784fb0f779e41a1b2b5e871c2bd0190cfa9f839380a652)]
UPDATE SOShipmentSplitToCartSplitLink
SET
  CreatedByID = '00000000-0000-0000-0000-000000000000',
  CreatedByScreenID = '00000000',
  CreatedDateTime = GETDATE(),
  LastModifiedByID = '00000000-0000-0000-0000-000000000000',
  LastModifiedByScreenID = '00000000',
  LastModifiedDateTime = GETDATE()
WHERE CreatedByID IS NULL
GO

-- AC-139235: UPS freight price fix.
--[MinVersion(Hash = 20e00fbfe9af074c16d5af46ab5917e561514d9e6cd9889cffa24c35a573ed47)]
UPDATE SOShipLineSplitPackage
SET UnitPriceFactor = 1
GO

-- AC-143602: Set non-zero POOrder.UnbilledOrderQty and POOrder.CuryUnbilledOrderTotal in order to avoid the issue with AP Bills created without taxes.
-- This script is supposed to execute only on upgrade from 2018R1 to 2018R2 or 2019R1, POOrder.CuryOpenOrderTotal and POReceiptTax are added for that purpose.
--[IfExists(Column = POOrder.CuryUnbilledOrderTotal)]
--[IfExists(Column = POOrder.UnbilledOrderTotal)]
--[IfExists(Column = POOrder.CuryOpenOrderTotal)]
--[IfExists(Column = POOrder.UnbilledOrderQty)]
--[IfExists(Table = POReceiptTax)]
--[MinVersion(Hash = 8ada3f12356bc87794fa1697e3132486f15782ed480b972c01cd5988c98edbe4)]
update po
set UnbilledOrderQty = POReceiptLineGroup.UnbilledQtySum
from POOrder po
inner join
(	select pl.CompanyID, pl.OrderType, pl.OrderNbr, sum(prl.UnbilledQty) as UnbilledQtySum
	from POLine pl
	inner join POReceiptLine prl on prl.CompanyID = pl.CompanyID and prl.POType = pl.OrderType and prl.PONbr = pl.OrderNbr and prl.POLineNbr = pl.LineNbr
	where prl.UnbilledQty > 0
	group by pl.CompanyID, pl.OrderType, pl.OrderNbr
) POReceiptLineGroup
on po.CompanyID = POReceiptLineGroup.CompanyID and po.OrderType = POReceiptLineGroup.OrderType and po.OrderNbr = POReceiptLineGroup.OrderNbr
where po.OrderQty > 0 and po.UnbilledOrderQty = 0

update po
set CuryUnbilledOrderTotal = po.CuryOrderTotal, UnbilledOrderTotal = po.OrderTotal
from POOrder po
left join APTran ap on po.CompanyID = ap.CompanyID and po.OrderType = ap.POOrderType and po.OrderNbr = ap.PONbr
where ap.RefNbr is null and po.OrderType in ('RO','DS')
	and po.CuryOrderTotal > 0 and po.CuryUnbilledOrderTotal = 0
GO

-- AC-147493: Discrepancy between INCostStatus and INTranCost after return by original cost.
--[MinVersion(Hash = 4025f6798b05eb39b74160973fd15fc0ecda5b17bd7e30ce051f640d479ee84c)]
IF EXISTS(SELECT TOP 1 * FROM INTran WHERE ExactCost = 1)
	UPDATE CS
	SET CS.TotalCost = Fix.ValidTotalCost
	FROM INCostStatus CS
	INNER JOIN
	(
		SELECT LayersToFix.CompanyID, LayersToFix.CostID, SUM(TC.InvtMult * TC.TranCost) AS ValidTotalCost
		FROM
		(
			SELECT iCS.CompanyID, iCS.CostID
			FROM INTran iT
			INNER JOIN INTranCost iTC
				ON iTC.CompanyID = iT.CompanyID
				AND iTC.DocType = iT.DocType
				AND iTC.RefNbr = iT.RefNbr
			INNER JOIN INCostStatus iCS
				ON iCS.CompanyID = iTC.CompanyID
				AND iCS.CostID = iTC.CostID
				AND iCS.LayerType = 'N'
			INNER JOIN InventoryItem I
				ON I.CompanyID = iTC.CompanyID
				AND I.InventoryID = iTC.InventoryID
			WHERE iT.ExactCost = 1 AND I.ValMethod <> 'T'
			GROUP BY iCS.CompanyID, iCS.CostID
		) LayersToFix
		INNER JOIN INTranCost TC
			ON TC.CompanyID = LayersToFix.CompanyID
			AND TC.CostID = LayersToFix.CostID
		GROUP BY LayersToFix.CompanyID, LayersToFix.CostID
	) Fix
		ON Fix.CompanyID = CS.CompanyID
		AND Fix.CostID = CS.CostID
	WHERE CS.TotalCost <> Fix.ValidTotalCost
GO

-- AC-154820: restore correct UnitType
--[MinVersion(Hash = 4c2cc94928b80de45b8be28e0553a983209159f468e0f887ce6025335d6e04a2)]
update INUnit set 
ItemClassID = 0,
UnitType = 1
where UnitType = 0 and ItemClassID = 1
GO

--[SmartExecute]
--[MinVersion(Hash = de2d86b6d1000291bb0091afc385371e2beca3a5ee6abe32b4a1adf7f6e098a9)]
INSERT INTO [INUnit]
	([CompanyID]
	,[UnitType]
	,[ItemClassID]
	,[InventoryID]
	,[FromUnit]
	,[ToUnit]
	,[UnitMultDiv]
	,[UnitRate]
	,[PriceAdjustmentMultiplier]
	,[CreatedByID]
	,[CreatedByScreenID]
	,[CreatedDateTime]
	,[LastModifiedByID]
	,[LastModifiedByScreenID]
	,[LastModifiedDateTime])
SELECT
	i.CompanyID,
	1 AS UnitType,
	0 AS ItemClassID,
	i.InventoryID,
	i.BaseUnit AS [FromUnit],
	i.BaseUnit AS [ToUnit],
	'M' AS [UnitMultDiv],
	1.0 AS UnitRate,
	1.0 AS PriceAdjustmentMultiplier, 
	i.CreatedByID,
	i.CreatedByScreenID,
	GETDATE() AS [CreatedDateTime],
	i.LastModifiedByID,
	i.LastModifiedByScreenID,
	GETDATE() AS [LastModifiedDateTime]
FROM InventoryItem i
WHERE i.StkItem = 1
	AND i.DeletedDatabaseRecord = 0
	AND i.BaseUnit <> ''
	AND NOT EXISTS(
		SELECT 1
		FROM INUnit u
		WHERE u.InventoryID = i.InventoryID	AND u.FromUnit = i.BaseUnit	AND u.ToUnit = i.BaseUnit --re-run after AC-156437 fix
	)
GO

-- AC-151113 Drop unused SMUpdatePrinterList table
--[IfExists(Table = SMUpdatePrinterList)]
--[MinVersion(Hash = 97696bfe89296583cca5f012c96e66cdf6af347f2a017e43f1ecb49b428744a0)]
drop table SMUpdatePrinterList
GO

-- AC-151113 Drop unused SMUpdateScannerList table
--[IfExists(Table = SMUpdateScannerList)]
--[MinVersion(Hash = 967466f424935d687ea7225bde1d8badd32292f2036d2a65c0a62373ecd0f549)]
drop table SMUpdateScannerList
GO

-- AC-153415: Incorrect SOOrder total amounts.
--[OldHash(Hash = f12ac79c0fb2dd3d761d979ec9c7782fd366d4f85a3add4f8f810868c800809d)]
--[MinVersion(Hash = 1d661cc55a3c6ebef6933967e29afe60905a2687a4fcb967a3701dab9ce9bbb1)]
UPDATE O
	SET O.CuryUnbilledDiscTotal = ROUND(LG.CuryUnbilledDiscTotal, COALESCE(CL.DecimalPlaces, 2)),
		O.UnbilledDiscTotal = ROUND(LG.UnbilledDiscTotal, COALESCE(CL.DecimalPlaces, 2)),
		O.CuryOpenDiscTotal = ROUND(LG.CuryOpenDiscTotal, COALESCE(CL.DecimalPlaces, 2)),
		O.OpenDiscTotal = ROUND(LG.OpenDiscTotal, COALESCE(CL.DecimalPlaces, 2))
FROM SOOrder O
INNER JOIN 
(
	SELECT CompanyID, OrderType, OrderNbr, 
		SUM(COALESCE(CuryUnbilledAmt * (1 - GroupDiscountRate * DocumentDiscountRate), 0)) AS CuryUnbilledDiscTotal,
		SUM(COALESCE(UnbilledAmt * (1 - GroupDiscountRate * DocumentDiscountRate), 0)) AS UnbilledDiscTotal,
		SUM(COALESCE(CuryOpenAmt * (1 - GroupDiscountRate * DocumentDiscountRate), 0)) AS CuryOpenDiscTotal,
		SUM(COALESCE(OpenAmt * (1 - GroupDiscountRate * DocumentDiscountRate), 0)) AS OpenDiscTotal
	FROM SOLine
	GROUP BY CompanyID, OrderType, OrderNbr
) LG
	ON LG.CompanyID = O.CompanyID
	AND LG.OrderType = O.OrderType
	AND LG.OrderNbr = O.OrderNbr
LEFT JOIN CurrencyList CL
	ON CL.CompanyID = O.CompanyID
	AND CL.CuryID = O.CuryID
GO

-- AC-156841: Fix for Shipment.Released flag.
--[MinVersion(Hash = 6ad0845f890c277a56c651f10c060ee8122932f9e7facd0fa8c891900538bbd2)]
UPDATE S
SET S.Released = CAST(OS.Released AS BIT)
FROM SOShipment S
INNER JOIN (
	SELECT CompanyID, ShipmentType, ShipmentNbr,
		MIN(CASE WHEN InvtRefNbr IS NOT NULL THEN 1 ELSE 0 END) Released
	FROM SOOrderShipment
	GROUP BY CompanyID, ShipmentType, ShipmentNbr
) OS
ON S.CompanyID = OS.CompanyID
	AND S.ShipmentType = OS.ShipmentType
	AND S.ShipmentNbr = OS.ShipmentNbr
GO

-- AC-146509: Fill system fields for SOShipLineSplitPackage table
--[MinVersion(Hash = 2b2a37f039c36040aa66c2736f3a4d0365cb3611092597119446ac995f245eb3)]
UPDATE SOShipLineSplitPackage
SET
  CreatedByID = '00000000-0000-0000-0000-000000000000',
  CreatedByScreenID = '00000000',
  CreatedDateTime = GETDATE(),
  LastModifiedByID = '00000000-0000-0000-0000-000000000000',
  LastModifiedByScreenID = '00000000',
  LastModifiedDateTime = GETDATE()
WHERE CreatedByID IS NULL
GO

-- AC-141269: Fill IsStockItem column for ARTran table
--[MinVersion(Hash = 9af9550c1407640004692d39af2ec20ba78600529053f9f54532602b559a81f1)]
update t
set IsStockItem = i.StkItem
from ARTran t
	inner join InventoryItem i
		on i.CompanyID = t.CompanyID
		and i.InventoryID =  t.InventoryID
GO

-- AC-163245: Replace Receipt type of INTran related with CashReturn invoice to CreditMemo
--[MinVersion(Hash = f527ff4bf1fc2644255a05fb3d4606066c7779a508ccf139286f4a4da04d97f7)]
update line
set 
	TranType = 'CRM'
from INTran line
	inner join SOInvoice invoice
		on line.ARDocType = 'RCS'
		and line.DocType = 'I'
		and line.TranType = 'RCP'
		and line.CompanyID = invoice.CompanyID
		and line.ARDocType = invoice.DocType
		and line.ARRefNbr = invoice.RefNbr
GO

--[MinVersion(Hash = 692b06341c18524eb5d016969aebd76f815f28453ab4b794387f36f143faf840)]
update n set 
  n.SubItemID = p.SubItemID
from APTran n
inner join POReceiptLine p on p.CompanyID = n.CompanyID and p.ReceiptNbr = n.ReceiptNbr and p.LineNbr = n.ReceiptLineNbr
where p.ReceiptNbr is not null and n.SubItemID is null
GO

-- AC-166884: Corrected Unbilled Amounts for Purchase Orders and Lines with negative Line Amounts
--[MinVersion(Hash = 1cf6a2b17ac7365e6eaf56fa538c5771fb2b7a36e8af5b2873ab23ee82143ff1)]
UPDATE POLine
SET CuryUnbilledAmt = CASE WHEN CuryLineAmt - CuryDiscAmt - CuryBilledAmt < 0
		THEN CuryLineAmt - CuryDiscAmt - CuryBilledAmt
		ELSE 0 END,
	UnbilledAmt = CASE WHEN LineAmt - DiscAmt - BilledAmt < 0
		THEN LineAmt - DiscAmt - BilledAmt
		ELSE 0 END
WHERE CuryLineAmt < 0

UPDATE po
SET CuryUnbilledLineTotal = lg.CuryUnbilledLineTotal,
	UnbilledLineTotal = lg.UnbilledLineTotal,
	CuryUnbilledOrderTotal = po.CuryUnbilledOrderTotal - po.CuryUnbilledLineTotal + lg.CuryUnbilledLineTotal,
	UnbilledOrderTotal = po.UnbilledOrderTotal - po.UnbilledLineTotal + lg.UnbilledLineTotal,
	CuryUnprepaidTotal =
		CASE WHEN po.CuryUnbilledOrderTotal - po.CuryUnbilledLineTotal + lg.CuryUnbilledLineTotal - po.CuryPrepaidTotal > 0
			THEN po.CuryUnbilledOrderTotal - po.CuryUnbilledLineTotal + lg.CuryUnbilledLineTotal - po.CuryPrepaidTotal
		ELSE 0 END,
	UnprepaidTotal =
		CASE WHEN po.UnbilledOrderTotal - po.UnbilledLineTotal + lg.UnbilledLineTotal - po.PrepaidTotal > 0
			THEN po.UnbilledOrderTotal - po.UnbilledLineTotal + lg.UnbilledLineTotal - po.PrepaidTotal
		ELSE 0 END
FROM POOrder po
INNER JOIN
(
	SELECT CompanyID, OrderType, OrderNbr, 
		SUM(CuryUnbilledAmt) AS CuryUnbilledLineTotal,
		SUM(UnbilledAmt) AS UnbilledLineTotal
	FROM POLine
	GROUP BY CompanyID, OrderType, OrderNbr
	HAVING MIN(CuryLineAmt) < 0
) lg ON lg.CompanyID = po.CompanyID AND lg.OrderType = po.OrderType AND lg.OrderNbr = po.OrderNbr
GO

-- AC-177322: Clear links to SO when reversal is performed in the context of ARInvoiceEntry
--[MinVersion(Hash = dc07ff0ba8c9824ed850631dbb75f8b6a70b7607b7c342cfd1bcd77a9d13960c)]
update t
set SOOrderType = null,
	SOOrderNbr = null,
	SOOrderLineNbr = null,
	SOOrderLineOperation = null,
	SOOrderSortOrder = null,
	SOShipmentType = null,
	SOShipmentNbr = null,
	SOShipmentLineNbr = null
from ARTran t
	inner join ARRegister r on t.CompanyID = r.CompanyID and t.TranType = r.DocType and t.RefNbr = r.RefNbr
where r.OrigModule <> 'SO' and
	(t.SOOrderType is not null
		or t.SOOrderNbr is not null
		or t.SOOrderLineNbr is not null
		or t.SOOrderLineOperation is not null
		or t.SOOrderSortOrder is not null
		or t.SOShipmentType is not null
		or t.SOShipmentNbr is not null
		or t.SOShipmentLineNbr is not null)
GO



--[RefreshMetadata()]
GO

--PROJ scripts
--[MinVersion(Hash = 67ba330e812a192ac8d78ec5317511bbdd8b856c29c316717b7ea59ebab2f45e)]
UPDATE b
SET b.InventoryID = i.InventoryID
FROM PMforecastDetail b
INNER JOIN InventoryItem i ON i.CompanyID = b.CompanyID AND i.InventoryCD='<N/A>'
WHERE b.InventoryID = 0
GO

--[MinVersion(Hash = c8435463d82c6ce9ceffed0834638e8e153532cd711ba482c0afd8ba7313b949)]
/* 
////////////////////////////////////////////////////////
///                SHIPPING ADDRESSES                ///
////////////////////////////////////////////////////////
*/

-- Update script for PMProforma.ShipAddressID
-- We should take default address from the location,
-- related to the document. Note we don't need to
-- create a new PMAddress record for each document, this
-- is the reason for GROUP BY expression below.
-- 
INSERT INTO PMAddress 
(
	CompanyID, 
	CustomerID,
	RevisionID,
	AddressLine1,
	AddressLine2,
	AddressLine3,
	City,
	State,
	CountryID,
	PostalCode,
	IsValidated,
	NoteID,
	CreatedByID,
	CreatedByScreenID,
	CreatedDateTime,
	LastModifiedByID,
	LastModifiedByScreenID,
	LastModifiedDateTime,
	IsDefaultBillAddress,
	CustomerAddressID
)
(
	SELECT
		Address.CompanyID, 
		MAX(Address.BAccountID),
		MAX(Address.RevisionID),
		MAX(Address.AddressLine1),
		MAX(Address.AddressLine2),
		MAX(Address.AddressLine3),
		MAX(Address.City),
		MAX(Address.State),
		MAX(Address.CountryID),
		MAX(Address.PostalCode),
		MAX(CONVERT(int, Address.IsValidated)),
		NEWID(),
		'00000000-0000-0000-0000-000000000000',
		'00000000',
		GETDATE(),
		'00000000-0000-0000-0000-000000000000',
		'00000000',
		GETDATE(),
		1,
		Address.AddressID
	FROM PMProforma 
	INNER JOIN Location ON Location.CompanyID = PMProforma.CompanyID
		AND Location.BAccountID = PMProforma.CustomerID 
		AND Location.LocationID = PMProforma.LocationID
	INNER JOIN Address ON Address.CompanyID = Location.CompanyID 
		AND Address.BAccountID = Location.BAccountID
		AND Address.AddressID = Location.DefAddressID
	LEFT JOIN PMAddress ON PMAddress.CompanyID = Address.CompanyID
		AND PMAddress.CustomerID = Address.BAccountID
		AND PMAddress.CustomerAddressID = Address.AddressID
		AND PMAddress.RevisionID = Address.RevisionID
		AND PMAddress.IsDefaultBillAddress = 1
	WHERE 
		PMProforma.ShipAddressID IS NULL
		AND PMAddress.CompanyID IS NULL
	GROUP BY 
		Address.CompanyID, 
		Address.AddressID
)

UPDATE PMProforma SET 
	PMProforma.ShipAddressID = data.AddressID
FROM PMProforma
	INNER JOIN
		(SELECT
			PMProforma.CompanyID,
			PMProforma.RefNbr,
			(SELECT TOP 1
				PMAddress.AddressID
			FROM
				PMAddress
			WHERE
				PMAddress.CompanyID = Address.CompanyID
				AND PMAddress.CustomerID = Address.BAccountID
				AND PMAddress.CustomerAddressID = Address.AddressID
				AND PMAddress.RevisionID = Address.RevisionID
				AND PMAddress.IsDefaultBillAddress = 1) AddressID
		FROM PMProforma
		INNER JOIN Location ON Location.CompanyID = PMProforma.CompanyID
			AND Location.BAccountID = PMProforma.CustomerID 
			AND Location.LocationID = PMProforma.LocationID
		INNER JOIN Address ON Address.CompanyID = Location.CompanyID 
			AND Address.BAccountID = Location.BAccountID
			AND Address.AddressID = Location.DefAddressID
		WHERE PMProforma.ShipAddressID IS NULL) data
	ON PMProforma.CompanyID = data.CompanyID
	AND PMProforma.RefNbr = data.RefNbr

/* 
////////////////////////////////////////////////////////
///                SHIPPING CONTACTS                 ///
////////////////////////////////////////////////////////
*/

-- Update script for PMProforma.ShipContactID
-- We should take default contact from the location,
-- related to the document. Note we don't need to
-- create a new PMContact record for each document, this
-- is the reason for GROUP BY expression below.
-- 
INSERT INTO PMContact 
(
	CompanyID,
	CustomerID,
	CustomerContactID,
	RevisionID,
	IsDefaultContact,
	Title,
	Salutation,
	Attention,
	FullName,
	Email,
	Phone1,
	Phone1Type,
	Phone2,
	Phone2Type,
	Phone3,
	Phone3Type,
	Fax,
	FaxType,
	NoteID,
	CreatedByID,
	CreatedByScreenID,
	CreatedDateTime,
	LastModifiedByID,
	LastModifiedByScreenID,
	LastModifiedDateTime
)
(
	SELECT
		Contact.CompanyID,
		MAX(Contact.BAccountID),
		Contact.ContactID,
		MAX(Contact.RevisionID),
		1,
		MAX(Contact.Title),
		MAX(Contact.Salutation),
		MAX(Contact.Attention),
		MAX(Contact.FullName),
		MAX(Contact.Email),
		MAX(Contact.Phone1),
		MAX(Contact.Phone1Type),
		MAX(Contact.Phone2),
		MAX(Contact.Phone2Type),
		MAX(Contact.Phone3),
		MAX(Contact.Phone3Type),
		MAX(Contact.Fax),
		MAX(Contact.FaxType),
		NEWID(),
		'00000000-0000-0000-0000-000000000000',
		'00000000',
		GETDATE(),
		'00000000-0000-0000-0000-000000000000',
		'00000000',
		GETDATE()
	FROM PMProforma 
	INNER JOIN Location ON Location.CompanyID = PMProforma.CompanyID
		AND Location.BAccountID = PMProforma.CustomerID 
		AND Location.LocationID = PMProforma.LocationID
	INNER JOIN Contact ON Contact.CompanyID = Location.CompanyID 
		AND Contact.BAccountID = Location.BAccountID
		AND Contact.ContactID = Location.DefContactID
	LEFT JOIN PMContact ON PMContact.CompanyID = Contact.CompanyID
		AND PMContact.CustomerID = Contact.BAccountID
		AND PMContact.CustomerContactID = Contact.ContactID
		AND PMContact.RevisionID = Contact.RevisionID
		AND PMContact.IsDefaultContact = 1
	WHERE 
		PMProforma.ShipContactID IS NULL
		AND PMContact.CompanyID IS NULL
	GROUP BY 
		Contact.CompanyID, 
		Contact.ContactID
)

UPDATE PMProforma SET
	PMProforma.ShipContactID = data.ContactID
FROM PMProforma
	INNER JOIN
		(SELECT
			PMProforma.CompanyID,
			PMProforma.RefNbr,
			(SELECT TOP 1
				PMContact.ContactID
			FROM
				PMContact
			WHERE
				PMContact.CompanyID = Contact.CompanyID
				AND PMContact.CustomerID = Contact.BAccountID
				AND PMContact.CustomerContactID = Contact.ContactID
				AND PMContact.RevisionID = Contact.RevisionID
				AND PMContact.IsDefaultContact = 1) ContactID
		FROM PMProforma 
		INNER JOIN Location ON Location.CompanyID = PMProforma.CompanyID
			AND Location.BAccountID = PMProforma.CustomerID 
			AND Location.LocationID = PMProforma.LocationID
		INNER JOIN Contact ON Contact.CompanyID = Location.CompanyID 
			AND Contact.BAccountID = Location.BAccountID
			AND Contact.ContactID = Location.DefContactID
		WHERE PMProforma.ShipContactID IS NULL) data
	ON PMProforma.CompanyID = data.CompanyID
	AND PMProforma.RefNbr = data.RefNbr
GO

-- Shifted calendars were introduced. TranPeriod column
-- now contains a reference to Master Calendar
-- Existing rows do not belong to shifted calendars
-- so TranPeriod is equal to FinPeriod (it was taken from Date before)
--[MinVersion(Hash = 44a6d5ab94a0431751ea7fb41582a27524311c1c7dd7c2b3839a688381e88b5c)]
UPDATE PMTran SET TranPeriodId = FinPeriodId
GO

-- Shifted calendars were introduced. PMHistory table
-- now contain BranchID column. All existing data
-- to be removed and Validate Project Balances operation
-- to be run manually after upgrade (out of upgrade script scope)
--[MinVersion(Hash = 8d6f10165f0dae89eebc22a79442945ec9c38d8c2b5d83da22b46359d60e9fa1)]
DELETE PMHistory
GO

--[MinVersion(Hash = 67ba330e812a192ac8d78ec5317511bbdd8b856c29c316717b7ea59ebab2f45e)]
UPDATE b
SET b.InventoryID = i.InventoryID
FROM PMforecastDetail b
INNER JOIN InventoryItem i ON i.CompanyID = b.CompanyID AND i.InventoryCD='<N/A>'
WHERE b.InventoryID = 0
GO

--BEGIN Project-Multi-Currency Update script block
--[IfNotExistsSelect(From = CMSetup, WhereIsNotNull = PMRateTypeDflt)]
--[MinVersion(Hash = a0b5e483c2b964a627b21bab539ddb8e098e7e0cafebff2bdf20dc87d4232ba0)]
UPDATE CMSetup SET PMRateTypeDflt = GLRateTypeDflt
GO

--[IfNotExistsSelect(From = PMTran, WhereIsNotNull = TranCuryId)]
--[MinVersion(Hash = e7787908b4a9e8f536ceaf4c04757b1de491cf61553a0b3eec8f08818aa804ca)]
UPDATE t
SET t.TranCuryId = c.BaseCuryID,
	t.TranCuryAmount = t.Amount,
	t.ProjectCuryAmount = t.Amount,
	t.TranCuryUnitRate = t.UnitRate, 
	t.ProjectCuryInvoicedAmount = t.InvoicedAmount
FROM PMTran t
INNER JOIN Company c ON t.CompanyID = c.CompanyID
GO

--[IfNotExistsSelect(From = Contract, WhereIsNotNull = BillingCuryID)]
--[MinVersion(Hash = 1e2daf7e85fc7e46ef1b5a767ff9f3f29b3ee0e625729eba42b6480454cd67b4)]
UPDATE p
SET p.CuryID = comp.BaseCuryID,
	p.RateTypeID = CASE WHEN p.BaseType = 'P' THEN ISNULL(cust.CuryRateTypeID, cm.APRateTypeDflt) ELSE NULL END,
	p.BillingCuryID = CASE WHEN p.BaseType = 'P' THEN ISNULL(cust.CuryID, comp.BaseCuryID) ELSE NULL END
FROM [Contract] p
INNER JOIN Company comp ON p.CompanyID = comp.CompanyID
LEFT JOIN CMSetup cm ON p.CompanyID = cm.CompanyID
LEFT JOIN Customer cust ON p.CustomerID = cust.bAccountID AND p.CompanyID = cust.CompanyID
WHERE p.BaseType IN ('P','R') AND p.NonProject = 0
GO

--[IfNotExistsSelect(From = PMBudget, WhereIsNotNull = CuryAmount)]
--[MinVersion(Hash = 36d4f069b9daa558c736a1d720df98757410d7b79f9db10895b78961492f99e1)]
EXEC sp_executesql N'UPDATE PMBudget SET 
	CuryAmount = Amount,
	CuryRevisedAmount = RevisedAmount,
	CuryActualAmount = ActualAmount,
	CuryInvoicedAmount = InvoicedAmount,
	CuryAmountToInvoice = AmountToInvoice,
	CuryChangeOrderAmount = ChangeOrderAmount,
	CuryCommittedAmount = CommittedAmount,
	CuryCommittedOrigAmount = CommittedOrigAmount,
	CuryCommittedOpenAmount = CommittedOpenAmount,
	CuryCommittedInvoicedAmount = CommittedInvoicedAmount,
	CuryPrepaymentAmount = PrepaymentAmount,
	CuryPrepaymentAvailable = PrepaymentAvailable,
	CuryPrepaymentInvoiced = PrepaymentInvoiced,
	CuryLastCostToComplete = LastCostToComplete,
	CuryCostToComplete = CostToComplete,
	CuryLastCostAtCompletion = LastCostAtCompletion,
	CuryCostAtCompletion = CostAtCompletion,
	CuryMaxAmount = MaxAmount,
	CuryUnitRate = Rate,
	CuryUnitPrice = UnitPrice'
GO

--[IfNotExistsSelect(From = PMBudgetProduction, WhereIsNotNull = CuryCostToComplete)]
--[MinVersion(Hash = ae895a1e9903ae8e554ff91d02e77adde72fc5f66492885f05c1c4a98c4d5392)]
UPDATE PMBudgetProduction SET 
	CuryCostToComplete = CostToComplete,
	CuryCostAtCompletion = CostAtCompletion
GO

--[IfNotExistsSelect(From = PMProformaLine, WhereIsNotNull = CuryPreviouslyInvoiced)]
--[MinVersion(Hash = cc9a14b11a5f72142e3190cfb4f1479b59254ac60dbd92f13ed2f75b2275fbad)]
UPDATE PMProformaLine SET 
	CuryPreviouslyInvoiced = PreviouslyInvoiced
GO

--[IfNotExistsSelect(From = PMHistory, WhereIsNotNull = FinPTDCuryAmount)]
--[MinVersion(Hash = 249e4a89a0edeab1dc089edcc2d7086101b6a0ccf8370863dd154a69eea2cb18)]
UPDATE PMHistory SET 
	FinPTDCuryAmount = FinPTDAmount,
	TranPTDCuryAmount = TranPTDAmount,
	FinYTDCuryAmount = FinYTDAmount,
	TranYTDCuryAmount = TranYTDAmount
GO

--[IfNotExistsSelect(From = PMTaskTotal, WhereIsNotNull = CuryAsset)]
--[MinVersion(Hash = c94dc4293480bbe6fd1c514316020f371e29a88e91b44983920f459bd807e7ad)]
UPDATE PMTaskTotal SET 
	CuryAsset = Asset,
	CuryLiability = Liability,
	CuryIncome = Income,
	CuryExpense = Expense
GO

--[IfExists(Column = PMChangeOrderLine.AmountInBaseCury)]
--[IfNotExistsSelect(From = PMChangeOrderLine, WhereIsNotNull = AmountInProjectCury)]
--[MinVersion(Hash = 0d398671c09dc6c459a2f6bbc91e54acd9085fec55f36c63f7a3ba53953f6c5c)]
UPDATE PMChangeOrderLine SET AmountInProjectCury = AmountInBaseCury
GO

--[IfNotExistsSelect(From = PMBudget, WhereIsNotNull = CuryInfoID)]
--[MinVersion(Hash = 920f3077f09c2c105cc44843eb65edd8e222584e6d8ff23e740d5ed8bd36f64a)]
BEGIN
	DECLARE proj_cursor CURSOR FOR 
		SELECT p.CompanyID, p.CuryID, p.ContractID, cm.PMRateTypeDflt 
		FROM [Contract] p
		LEFT JOIN CMSetup cm ON p.CompanyID = cm.CompanyID
		WHERE BaseType = 'P' AND NonProject = 0
	DECLARE @companyID INT
	DECLARE @curyID NVARCHAR(5)
	DECLARE @contractID INT
	DECLARE @rateTypeId NVARCHAR(6)
	OPEN proj_cursor
	FETCH NEXT FROM proj_cursor INTO @companyID, @curyID, @contractID, @rateTypeId
	WHILE @@FETCH_STATUS = 0  
	BEGIN  
		INSERT INTO CurrencyInfo (CompanyID, CuryID, CuryRateTypeID, CuryEffDate, CuryMultDiv, CuryRate, BaseCuryID, RecipRate, BaseCalc)
		VALUES (@companyId, @curyID, @rateTypeId, CAST(GETDATE() AS DATE), 'M', 1, @curyID, 1, 1)
		UPDATE [Contract] SET CuryInfoID = @@IDENTITY
		WHERE ContractID = @contractID AND CompanyID = @companyID
		UPDATE PMBudget SET CuryInfoID = @@IDENTITY
		WHERE ProjectID = @contractID AND CompanyID = @companyID
		FETCH NEXT FROM proj_cursor INTO @companyID, @curyID, @contractID, @rateTypeId
	END
	CLOSE proj_cursor
	DEALLOCATE proj_cursor
END
GO

--[IfNotExistsSelect(From = PMTran, WhereIsNotNull = ProjectCuryInfoId)]
--[MinVersion(Hash = 76c3243c359767d8a3a0af8446a54571da64c75f4a3bc021ca699d6e27e4fe13)]
BEGIN
	DECLARE comp_cursor CURSOR FOR 
		SELECT c.CompanyID, c.BaseCuryID, cm.PMRateTypeDflt
		FROM Company c 
		LEFT JOIN CMSetup cm ON c.CompanyID = cm.CompanyID
	DECLARE @companyID INT
	DECLARE @baseCuryID NVARCHAR(5)
	DECLARE @rateTypeId NVARCHAR(6)
	OPEN comp_cursor
	FETCH NEXT FROM comp_cursor INTO @companyID, @baseCuryID, @rateTypeId
	WHILE @@FETCH_STATUS = 0  
	BEGIN
		IF EXISTS(SELECT * FROM PMTran WHERE CompanyID = @companyID AND Released = 1)
		BEGIN
			INSERT INTO CurrencyInfo (CompanyID, CuryID, CuryRateTypeID, CuryEffDate, CuryMultDiv, CuryRate, BaseCuryID, RecipRate, BaseCalc)
			VALUES (@companyId, @baseCuryID, @rateTypeId, CAST(GETDATE() AS DATE), 'M', 1, @baseCuryID, 1, 1)
			UPDATE PMTran SET ProjectCuryInfoId = @@IDENTITY, BaseCuryInfoId = @@IDENTITY
			WHERE CompanyID = @companyID AND Released = 1
		END
		FETCH NEXT FROM comp_cursor INTO @companyID, @baseCuryID, @rateTypeId
	END
	CLOSE comp_cursor
	DEALLOCATE comp_cursor

	DECLARE tran_cursor CURSOR FOR
		SELECT t.CompanyID, t.TranCuryID, t.TranID, cm.PMRateTypeDflt 
		FROM PMTran t
		LEFT JOIN CMSetup cm ON t.CompanyID = cm.CompanyID
		WHERE Released = 0
	DECLARE @tranCuryID NVARCHAR(5)
	DECLARE @tranID BIGINT
	OPEN tran_cursor
	FETCH NEXT FROM tran_cursor INTO @companyID, @tranCuryID, @tranID, @rateTypeId
	WHILE @@FETCH_STATUS = 0  
	BEGIN  
		INSERT INTO CurrencyInfo (CompanyID, CuryID, CuryRateTypeID, CuryEffDate, CuryMultDiv, CuryRate, BaseCuryID, RecipRate, BaseCalc)
		VALUES (@companyId, @tranCuryID, @rateTypeId, CAST(GETDATE() AS DATE), 'M', 1, @tranCuryID, 1, 1)
		UPDATE PMTran SET ProjectCuryInfoId = @@IDENTITY
		WHERE TranID = @tranID AND CompanyID = @companyID
		INSERT INTO CurrencyInfo (CompanyID, CuryID, CuryRateTypeID, CuryEffDate, CuryMultDiv, CuryRate, BaseCuryID, RecipRate, BaseCalc)
		VALUES (@companyId, @tranCuryID, @rateTypeId, CAST(GETDATE() AS DATE), 'M', 1, @tranCuryID, 1, 1)
		UPDATE PMTran SET BaseCuryInfoId = @@IDENTITY
		WHERE TranID = @tranID AND CompanyID = @companyID
		FETCH NEXT FROM tran_cursor INTO @companyID, @tranCuryID, @tranID, @rateTypeId
	END
	CLOSE tran_cursor
	DEALLOCATE tran_cursor
END
GO

--END Project-MultyCurrency Update script block
--[MinVersion(Hash = a9d3074f221a56035bd3347eee86e95ce8bca91d15e384bfec39ad001fd77591)]
UPDATE PMSetup SET [RevenueBudgetUpdateMode] = 'S' WHERE [RevenueBudgetUpdateMode] IS NULL
GO

--[MinVersion(Hash = 33b1739d6bb9465876089ba49b2084ed33c1128f1f6fcf4c08f824fd752754ab)]
UPDATE [Contract] SET [CostBudgetLevel] = 'T' WHERE [NonProject] = 1
GO

--[MinVersion(Hash = 7b57cdbddebe2d52e484d1d389835c174c0000591cf06c81ad96d0915dac2abd)]
UPDATE FeaturesSet
SET ExpenseManagement = 1
GO

--[mysql: Skip]
--[IfExists(Column = PMSetup.UsrBudgetControl)]
--[MinVersion(Hash = b6348f3d84c5fcc33331ecd78c9f2500b67a1dc025b3ffd685d7472e05a2705c)]
BEGIN
	DECLARE @dcname NVARCHAR(100)
	DECLARE @command  NVARCHAR(1000)

	SELECT @dcname = d.name
	FROM sys.default_constraints d  
	INNER JOIN sys.columns c ON d.parent_object_id = c.object_id AND d.parent_column_id = c.column_id  
	WHERE d.parent_object_id = OBJECT_ID(N'PMSetup', N'U') AND c.name = 'UsrBudgetControl'

	IF @dcname is not null
	BEGIN
		SELECT @command = 'ALTER TABLE PMSetup DROP CONSTRAINT ' + @dcname
		EXECUTE (@command)
	END
END

--[IfExists(Column = PMSetup.UsrBudgetControl)]
UPDATE PMSetup SET BudgetControl = UsrBudgetControl
ALTER TABLE PMSetup DROP COLUMN UsrBudgetControl
GO

--Initilize Cost Budget Level in Projects
--[MinVersion(Hash = b66f84654b2e95fb8388ba23385d7fe16bf2cc45d13d7c1fd53be6b6ce7c2dce)]
UPDATE p
SET p.CostBudgetLevel = (CASE 
	WHEN f.CostCodes = 1 AND s.CostBudgetUpdateMode = 'S' THEN 'T'
	WHEN f.CostCodes = 1 AND s.CostBudgetUpdateMode = 'D' THEN 'C'
	WHEN f.CostCodes = 0 AND s.CostBudgetUpdateMode = 'S' THEN 'T'
	WHEN f.CostCodes = 0 AND s.CostBudgetUpdateMode = 'D' THEN 'I'
END) FROM Contract p
INNER JOIN FeaturesSet f ON p.CompanyID = f.CompanyID
INNER JOIN PMSetup s ON p.CompanyID = s.CompanyID
WHERE p.BudgetLevel IS NOT NULL
GO

--[MinVersion(Hash = 8af0141d6dbfc128ed96d4ddfe85bdf94ea7c52cc5283c69751fd03029008623)]
UPDATE EPExpenseClaimDetails
SET PaidWith = 'PersAcc'
GO

--[MinVersion(Hash = cdefff31ee41f7b21f446434c12f2172b807654f201c7da1ce9cc1cb9133ce35)]
UPDATE CASetup
SET CuryDiffThreshold = 5
GO

--[MinVersion(Hash = 27980fe5f64e539e3bf97772a97df9c728a11f665db83b35ff0ee5727d8d68d0)]
UPDATE CASetup
SET CorpCardNumberingID = 'CORPCC'
GO

--Initilize Cost Budget Level in Projects
--[MinVersion(Hash = b66f84654b2e95fb8388ba23385d7fe16bf2cc45d13d7c1fd53be6b6ce7c2dce)]
UPDATE p
SET p.CostBudgetLevel = (CASE 
	WHEN f.CostCodes = 1 AND s.CostBudgetUpdateMode = 'S' THEN 'T'
	WHEN f.CostCodes = 1 AND s.CostBudgetUpdateMode = 'D' THEN 'C'
	WHEN f.CostCodes = 0 AND s.CostBudgetUpdateMode = 'S' THEN 'T'
	WHEN f.CostCodes = 0 AND s.CostBudgetUpdateMode = 'D' THEN 'I'
END) FROM Contract p
INNER JOIN FeaturesSet f ON p.CompanyID = f.CompanyID
INNER JOIN PMSetup s ON p.CompanyID = s.CompanyID
WHERE p.BudgetLevel IS NOT NULL
GO

--[IfExists(Column = PMSetup.UsrChangeRequestNumbering)]
--[MinVersion(Hash = bc6d6c73b89fb8fc6900f9d56a263940143126fdc52b32a5161304f15f591afe)]
UPDATE FeaturesSet SET ChangeRequest = 1
UPDATE PMSetup SET ChangeRequestNumbering = UsrChangeRequestNumbering
ALTER TABLE PMSetup DROP COLUMN UsrChangeRequestNumbering
GO

--[IfExists(Column = PMSetup.ChangeRequestNumbering)]
--[MinVersion(Hash = 97f8accd6aa5b135fe598aabeff5111e85d8166da2be17040797bd2d62c97cc0)]
UPDATE PMSetup SET ChangeRequestNumbering = 'CHANGERST' WHERE ChangeRequestNumbering IS NULL
GO

--[IfExists(Column = PMSetup.UsrChangeRequestApprovalMapID)]
--[MinVersion(Hash = c3905cde44ced67733850003b9b4c34b3f18fa4ecd16317e7e95d3743d2d8137)]
UPDATE PMSetup SET ChangeRequestApprovalMapID = UsrChangeRequestApprovalMapID
ALTER TABLE PMSetup DROP COLUMN UsrChangeRequestApprovalMapID
GO

--[IfExists(Column = PMSetup.UsrChangeRequestApprovalNotificationID)]
--[MinVersion(Hash = 7615cc639e2152947da0a8f87984579d67a6bbe0fe0527a1f047deb60c899cc5)]
UPDATE PMSetup SET ChangeRequestApprovalNotificationID = UsrChangeRequestApprovalNotificationID
ALTER TABLE PMSetup DROP COLUMN UsrChangeRequestApprovalNotificationID
GO

--[mysql: Skip]
--[IfExists(Column = PMSetup.UsrDefaultPriceMarkupPct)]
--[MinVersion(Hash = 7ddf97732d2ad7e207392f5f4f81a8cb687b7644977b1bc2416fb1f4adbf7f00)]
BEGIN
	DECLARE @dcname NVARCHAR(100)
	DECLARE @command  NVARCHAR(1000)

	SELECT @dcname = d.name
	FROM sys.default_constraints d  
	INNER JOIN sys.columns c ON d.parent_object_id = c.object_id AND d.parent_column_id = c.column_id  
	WHERE d.parent_object_id = OBJECT_ID(N'PMSetup', N'U') AND c.name = 'UsrDefaultPriceMarkupPct'

	IF @dcname is not null
	BEGIN
		SELECT @command = 'ALTER TABLE PMSetup DROP CONSTRAINT ' + @dcname
		EXECUTE (@command)
	END
END
GO

--[IfExists(Column = PMSetup.UsrDefaultPriceMarkupPct)]
--[MinVersion(Hash = 2b7e96b78a98f12eda9e906ae689971382148a07610b4b58ed5c1e5819248d7d)]
UPDATE PMSetup SET DefaultPriceMarkupPct = UsrDefaultPriceMarkupPct
ALTER TABLE PMSetup DROP COLUMN UsrDefaultPriceMarkupPct
GO

--[mysql: Skip]
--[IfExists(Column = PMAccountGroup.UsrCreatesCommitment)]
--[MinVersion(Hash = 3edbdc19c50dafb288b25722661dcc0d418922af17a9009da1a6362140fc74f1)]
BEGIN
	DECLARE @dcname NVARCHAR(100)
	DECLARE @command  NVARCHAR(1000)

	SELECT @dcname = d.name
	FROM sys.default_constraints d  
	INNER JOIN sys.columns c ON d.parent_object_id = c.object_id AND d.parent_column_id = c.column_id  
	WHERE d.parent_object_id = OBJECT_ID(N'PMAccountGroup', N'U') AND c.name = 'UsrCreatesCommitment'

	IF @dcname is not null
	BEGIN
		SELECT @command = 'ALTER TABLE PMAccountGroup DROP CONSTRAINT ' + @dcname
		EXECUTE (@command)
	END
END
GO

--[IfExists(Column = PMAccountGroup.UsrCreatesCommitment)]
--[MinVersion(Hash = 4cf67fa3a476ffcef63194468d3516ef5b04b3f5f1458645b9d47bce2d7dc7ff)]
UPDATE PMAccountGroup SET CreatesCommitment = UsrCreatesCommitment
ALTER TABLE PMAccountGroup DROP COLUMN UsrCreatesCommitment
GO

--[mysql: Skip]
--[IfExists(Column = PMAccountGroup.UsrDefaultLineMarkupPct)]
--[MinVersion(Hash = c6abaa21df36b1c2fe6bd100785e760dd7c0e3a361b6260fa408a238262766ad)]
BEGIN
	DECLARE @dcname NVARCHAR(100)
	DECLARE @command  NVARCHAR(1000)

	SELECT @dcname = d.name
	FROM sys.default_constraints d  
	INNER JOIN sys.columns c ON d.parent_object_id = c.object_id AND d.parent_column_id = c.column_id  
	WHERE d.parent_object_id = OBJECT_ID(N'PMAccountGroup', N'U') AND c.name = 'UsrDefaultLineMarkupPct'

	IF @dcname is not null
	BEGIN
		SELECT @command = 'ALTER TABLE PMAccountGroup DROP CONSTRAINT ' + @dcname
		EXECUTE (@command)
	END
END
GO

--[IfExists(Column = PMAccountGroup.UsrDefaultLineMarkupPct)]
--[MinVersion(Hash = 7544bc2fd82b6c23663c614c5dd3cf00a0d754eb1f652ff6f471f3b1ca30e54c)]
UPDATE PMAccountGroup SET DefaultLineMarkupPct = UsrDefaultLineMarkupPct
ALTER TABLE PMAccountGroup DROP COLUMN UsrDefaultLineMarkupPct
GO

--[mysql: Skip]
--[IfExists(Column = PMChangeOrder.UsrChangeRequestCostTotal)]
--[MinVersion(Hash = 62d0232f836a8efa76de9390a4e5ab9e982b32a693c2f2149516d0c62dd4a5f3)]
BEGIN
	DECLARE @dcname NVARCHAR(100)
	DECLARE @command  NVARCHAR(1000)

	SELECT @dcname = d.name
	FROM sys.default_constraints d  
	INNER JOIN sys.columns c ON d.parent_object_id = c.object_id AND d.parent_column_id = c.column_id  
	WHERE d.parent_object_id = OBJECT_ID(N'PMChangeOrder', N'U') AND c.name = 'UsrChangeRequestCostTotal'

	IF @dcname is not null
	BEGIN
		SELECT @command = 'ALTER TABLE PMChangeOrder DROP CONSTRAINT ' + @dcname
		EXECUTE (@command)
	END
END
GO

--[IfExists(Column = PMChangeOrder.UsrChangeRequestCostTotal)]
--[MinVersion(Hash = 9019551fe2ebb8052b69e9a49293871c44f499f8d8b71ce773a14435b89e299a)]
UPDATE PMChangeOrder SET ChangeRequestCostTotal = UsrChangeRequestCostTotal
ALTER TABLE PMChangeOrder DROP COLUMN UsrChangeRequestCostTotal
GO

--[mysql: Skip]
--[IfExists(Column = PMChangeOrder.UsrChangeRequestLineTotal)]
--[MinVersion(Hash = ec212b4788a69d6d25ac41ba94372c62321306698ce42d0d29ce01f2f28cc466)]
BEGIN
	DECLARE @dcname NVARCHAR(100)
	DECLARE @command  NVARCHAR(1000)

	SELECT @dcname = d.name
	FROM sys.default_constraints d  
	INNER JOIN sys.columns c ON d.parent_object_id = c.object_id AND d.parent_column_id = c.column_id  
	WHERE d.parent_object_id = OBJECT_ID(N'PMChangeOrder', N'U') AND c.name = 'UsrChangeRequestLineTotal'

	IF @dcname is not null
	BEGIN
		SELECT @command = 'ALTER TABLE PMChangeOrder DROP CONSTRAINT ' + @dcname
		EXECUTE (@command)
	END
END
GO

--[IfExists(Column = PMChangeOrder.UsrChangeRequestLineTotal)]
--[MinVersion(Hash = 0e4f948fb2f8cab37ac10eb4cfedab1a5ee25775178f35b9b503621443b7d899)]
UPDATE PMChangeOrder SET ChangeRequestLineTotal = UsrChangeRequestLineTotal
ALTER TABLE PMChangeOrder DROP COLUMN UsrChangeRequestLineTotal
GO

--[mysql: Skip]
--[IfExists(Column = PMChangeOrder.UsrChangeRequestMarkupTotal)]
--[MinVersion(Hash = 30301fb383d3f8b711721754363fde7c92c8a1f49549ea431e521b64a28d84f1)]
BEGIN
	DECLARE @dcname NVARCHAR(100)
	DECLARE @command  NVARCHAR(1000)

	SELECT @dcname = d.name
	FROM sys.default_constraints d  
	INNER JOIN sys.columns c ON d.parent_object_id = c.object_id AND d.parent_column_id = c.column_id  
	WHERE d.parent_object_id = OBJECT_ID(N'PMChangeOrder', N'U') AND c.name = 'UsrChangeRequestMarkupTotal'

	IF @dcname is not null
	BEGIN
		SELECT @command = 'ALTER TABLE PMChangeOrder DROP CONSTRAINT ' + @dcname
		EXECUTE (@command)
	END
END
GO

--[IfExists(Column = PMChangeOrder.UsrChangeRequestMarkupTotal)]
--[MinVersion(Hash = 6326fe9d7955dc21f28a041ed1746583f6de6c95c487c0af2588f939d4a64970)]
UPDATE PMChangeOrder SET ChangeRequestMarkupTotal = UsrChangeRequestMarkupTotal
ALTER TABLE PMChangeOrder DROP COLUMN UsrChangeRequestMarkupTotal
GO

--[mysql: Skip]
--[IfExists(Column = PMChangeOrder.UsrChangeRequestPriceTotal)]
--[MinVersion(Hash = bc3a0ef640575add8447065c7908874e32c87afda1a293599db60dd102bbc881)]
BEGIN
	DECLARE @dcname NVARCHAR(100)
	DECLARE @command  NVARCHAR(1000)

	SELECT @dcname = d.name
	FROM sys.default_constraints d  
	INNER JOIN sys.columns c ON d.parent_object_id = c.object_id AND d.parent_column_id = c.column_id  
	WHERE d.parent_object_id = OBJECT_ID(N'PMChangeOrder', N'U') AND c.name = 'UsrChangeRequestPriceTotal'

	IF @dcname is not null
	BEGIN
		SELECT @command = 'ALTER TABLE PMChangeOrder DROP CONSTRAINT ' + @dcname
		EXECUTE (@command)
	END
END
GO

--[IfExists(Column = PMChangeOrder.UsrChangeRequestPriceTotal)]
--[MinVersion(Hash = ba46bed20703f47c57635b9a17c2146476825dbf32a4d3f6588b37dbf1faa9e1)]
UPDATE PMChangeOrder SET ChangeRequestPriceTotal = UsrChangeRequestPriceTotal
ALTER TABLE PMChangeOrder DROP COLUMN UsrChangeRequestPriceTotal
GO

--[mysql: Skip]
--[IfExists(Column = PMChangeOrderBudget.UsrChangeRequestQty)]
--[MinVersion(Hash = b95bc494ee48232fef7eccd8f0074e988a4cfec43a63fe9af3bcb4b15ec8df74)]
BEGIN
	DECLARE @dcname NVARCHAR(100)
	DECLARE @command  NVARCHAR(1000)

	SELECT @dcname = d.name
	FROM sys.default_constraints d  
	INNER JOIN sys.columns c ON d.parent_object_id = c.object_id AND d.parent_column_id = c.column_id  
	WHERE d.parent_object_id = OBJECT_ID(N'PMChangeOrderBudget', N'U') AND c.name = 'UsrChangeRequestQty'

	IF @dcname is not null
	BEGIN
		SELECT @command = 'ALTER TABLE PMChangeOrderBudget DROP CONSTRAINT ' + @dcname
		EXECUTE (@command)
	END
END
GO

--[IfExists(Column = PMChangeOrderBudget.UsrChangeRequestQty)]
--[MinVersion(Hash = 4683eec3d8bd78739ee4aed52515a763528523525cbf875a44b534ab4e68bfca)]
UPDATE PMChangeOrderBudget SET ChangeRequestQty = UsrChangeRequestQty
ALTER TABLE PMChangeOrderBudget DROP COLUMN UsrChangeRequestQty
GO

--[mysql: Skip]
--[IfExists(Column = PMChangeOrderBudget.UsrChangeRequestAmount)]
--[MinVersion(Hash = 4971174b7e892656e788cae5792a5d17a90a9334268efca667aff457b6d363e2)]
BEGIN
	DECLARE @dcname NVARCHAR(100)
	DECLARE @command  NVARCHAR(1000)

	SELECT @dcname = d.name
	FROM sys.default_constraints d  
	INNER JOIN sys.columns c ON d.parent_object_id = c.object_id AND d.parent_column_id = c.column_id  
	WHERE d.parent_object_id = OBJECT_ID(N'PMChangeOrderBudget', N'U') AND c.name = 'UsrChangeRequestAmount'

	IF @dcname is not null
	BEGIN
		SELECT @command = 'ALTER TABLE PMChangeOrderBudget DROP CONSTRAINT ' + @dcname
		EXECUTE (@command)
	END
END
GO

--[IfExists(Column = PMChangeOrderBudget.UsrChangeRequestAmount)]
--[MinVersion(Hash = 7177a941fff0580036bb71b8213b8f96125f4033ed4a2bd5d99bc569455f5388)]
UPDATE PMChangeOrderBudget SET ChangeRequestAmount = UsrChangeRequestAmount
ALTER TABLE PMChangeOrderBudget DROP COLUMN UsrChangeRequestAmount
GO

--[mysql: Skip]
--[IfExists(Column = PMChangeOrderBudget.UsrIsDisabled)]
--[MinVersion(Hash = 3d1d57856bc47f136020ba66ed4c4c690ca017095fc1a8d2226b89f503c34d16)]
BEGIN
	DECLARE @dcname NVARCHAR(100)
	DECLARE @command  NVARCHAR(1000)

	SELECT @dcname = d.name
	FROM sys.default_constraints d  
	INNER JOIN sys.columns c ON d.parent_object_id = c.object_id AND d.parent_column_id = c.column_id  
	WHERE d.parent_object_id = OBJECT_ID(N'PMChangeOrderBudget', N'U') AND c.name = 'UsrIsDisabled'

	IF @dcname is not null
	BEGIN
		SELECT @command = 'ALTER TABLE PMChangeOrderBudget DROP CONSTRAINT ' + @dcname
		EXECUTE (@command)
	END
END
GO

--[IfExists(Column = PMChangeOrderBudget.UsrIsDisabled)]
--[MinVersion(Hash = 8359c48a7c0454d24007a13af9f353926da5d84b8995712c1ceb37cab7c03228)]
UPDATE PMChangeOrderBudget SET IsDisabled = UsrIsDisabled
ALTER TABLE PMChangeOrderBudget DROP COLUMN UsrIsDisabled
GO

--[mysql: Skip]
--[IfExists(Column = PMChangeOrderClass.UsrIsAdvance)]
--[MinVersion(Hash = 87d91e96f45d76b55bdaf83a3c71dc76240d07a85ab6722e86d7e0b4cefdfb37)]
BEGIN
	DECLARE @dcname NVARCHAR(100)
	DECLARE @command  NVARCHAR(1000)

	SELECT @dcname = d.name
	FROM sys.default_constraints d  
	INNER JOIN sys.columns c ON d.parent_object_id = c.object_id AND d.parent_column_id = c.column_id  
	WHERE d.parent_object_id = OBJECT_ID(N'PMChangeOrderClass', N'U') AND c.name = 'UsrIsAdvance'

	IF @dcname is not null
	BEGIN
		SELECT @command = 'ALTER TABLE PMChangeOrderClass DROP CONSTRAINT ' + @dcname
		EXECUTE (@command)
	END
END
GO

--[IfExists(Column = PMChangeOrderClass.UsrIsAdvance)]
--[MinVersion(Hash = 124b3a556af5465aa6053e8016e471c1798d2aadef4a2734caaf87c73560129d)]
UPDATE PMChangeOrderClass SET IsAdvance = UsrIsAdvance
ALTER TABLE PMChangeOrderClass DROP COLUMN UsrIsAdvance
GO

--[IfExists(Column = PMChangeOrderLine.UsrChangeRequestRefNbr)]
--[MinVersion(Hash = d6b6068275ddb34784a5ea117ede339fd0c4c3a1d298f318e49e5d57908a8306)]
UPDATE PMChangeOrderLine SET ChangeRequestRefNbr = UsrChangeRequestRefNbr
ALTER TABLE PMChangeOrderLine DROP COLUMN UsrChangeRequestRefNbr
GO

--[IfExists(Column = PMChangeOrderLine.UsrChangeRequestLineNbr)]
--[MinVersion(Hash = 3b24bcafc228752261f964951e078403752881972dd3b8cc3f73d1eb50d46b6e)]
UPDATE PMChangeOrderLine SET ChangeRequestLineNbr = UsrChangeRequestLineNbr
ALTER TABLE PMChangeOrderLine DROP COLUMN UsrChangeRequestLineNbr
GO

--Removing PM308500 & PM3085PL (Change Requests) from User Company (will be installed as part of system data)
--[MinVersion(Hash = ebc97785a1928e14aca1c79f6733c39a55471d112d9123c60714682ee23350ca)]
DELETE a FROM MUIScreen a
INNER JOIN SiteMap m ON a.CompanyID = m.CompanyID AND a.NodeID = m.NodeID
INNER JOIN Company c ON m.CompanyID = c.CompanyID AND c.IsReadOnly = 0
WHERE m.ScreenID IN ('PM308500', 'PM3085PL')

DELETE a FROM MUIPinnedScreen a
INNER JOIN SiteMap m ON a.CompanyID = m.CompanyID AND a.NodeID = m.NodeID
INNER JOIN Company c ON m.CompanyID = c.CompanyID AND c.IsReadOnly = 0
WHERE m.ScreenID IN ('PM308500', 'PM3085PL')

DELETE a FROM MUIFavoriteScreen a
INNER JOIN SiteMap m ON a.CompanyID = m.CompanyID AND a.NodeID = m.NodeID
INNER JOIN Company c ON m.CompanyID = c.CompanyID AND c.IsReadOnly = 0
WHERE m.ScreenID IN ('PM308500', 'PM3085PL')

DELETE a FROM GINavigationScreen a
INNER JOIN GIDesign g ON a.CompanyID = g.CompanyID AND a.DesignID = g.DesignID
INNER JOIN Company c ON a.CompanyID = c.CompanyID AND c.IsReadOnly = 0
WHERE PrimaryScreenIDNew = 'PM308500'

DELETE a FROM GINavigationParameter a
INNER JOIN GIDesign g ON a.CompanyID = g.CompanyID AND a.DesignID = g.DesignID
INNER JOIN Company c ON a.CompanyID = c.CompanyID AND c.IsReadOnly = 0
WHERE PrimaryScreenIDNew = 'PM308500'

DELETE a FROM GISort a
INNER JOIN GIDesign g ON a.CompanyID = g.CompanyID AND a.DesignID = g.DesignID
INNER JOIN Company c ON a.CompanyID = c.CompanyID AND c.IsReadOnly = 0
WHERE PrimaryScreenIDNew = 'PM308500'

DELETE a FROM GITable a
INNER JOIN GIDesign g ON a.CompanyID = g.CompanyID AND a.DesignID = g.DesignID
INNER JOIN Company c ON a.CompanyID = c.CompanyID AND c.IsReadOnly = 0
WHERE PrimaryScreenIDNew = 'PM308500'

DELETE a FROM GIResult a
INNER JOIN GIDesign g ON a.CompanyID = g.CompanyID AND a.DesignID = g.DesignID
INNER JOIN Company c ON a.CompanyID = c.CompanyID AND c.IsReadOnly = 0
WHERE PrimaryScreenIDNew = 'PM308500'

DELETE a FROM GIRelation a
INNER JOIN GIDesign g ON a.CompanyID = g.CompanyID AND a.DesignID = g.DesignID
INNER JOIN Company c ON a.CompanyID = c.CompanyID AND c.IsReadOnly = 0
WHERE PrimaryScreenIDNew = 'PM308500'

DELETE a FROM GIOn a
INNER JOIN GIDesign g ON a.CompanyID = g.CompanyID AND a.DesignID = g.DesignID
INNER JOIN Company c ON a.CompanyID = c.CompanyID AND c.IsReadOnly = 0
WHERE PrimaryScreenIDNew = 'PM308500'

DELETE a FROM GIDesign a
INNER JOIN Company c ON a.CompanyID = c.CompanyID AND c.IsReadOnly = 0
WHERE PrimaryScreenIDNew = 'PM308500'

DELETE a FROM FilterRow a
INNER JOIN Company c ON a.CompanyID = c.CompanyID AND c.IsReadOnly = 0
INNER JOIN FilterHeader h ON a.CompanyID = h.CompanyID AND a.FilterID = h.FilterID
WHERE ScreenID = 'PM3085PL'

DELETE a FROM FilterHeader a
INNER JOIN Company c ON a.CompanyID = c.CompanyID AND c.IsReadOnly = 0
WHERE ScreenID = 'PM3085PL'

DELETE m FROM MobileSiteMap m
INNER JOIN Company c ON m.CompanyID = c.CompanyID AND c.IsReadOnly = 0
WHERE m.ScreenID IN ('PM308500','PM3085PL')

DELETE m FROM SiteMap m
INNER JOIN Company c ON m.CompanyID = c.CompanyID AND c.IsReadOnly = 0
WHERE m.ScreenID IN ('PM308500','PM3085PL')
GO

--[MinVersion(Hash = 917fc0d8da986bc8913a9815bcbeace65ede176cc4bdabd1b5f2ea146c67d9ba)]
UPDATE GITable SET Name = 'PX.Objects.PM.PMChangeRequest' WHERE DesignID = '8CD7EF82-0F80-42CB-9915-867AD3F03A68' AND Name='ChangeRequest.PMChangeRequest'
UPDATE EPAssignmentMap SET EntityType = 'PX.Objects.PM.PMChangeRequest' WHERE EntityType='ChangeRequest.PMChangeRequest'
UPDATE EPAssignmentMap SET GraphType = 'PX.Objects.PM.ChangeRequest.ChangeRequestEntry' WHERE GraphType='ChangeRequest.ChangeRequestEntry'
GO

--[MinVersion(Hash = 3c70605be0e86de362cdc515ce5c56f9ce0265cd101933334de4acb25ead675e)]
UPDATE com
SET com.BranchID = pol.BranchID
FROM [PMCommitment] com
INNER JOIN POLine pol ON pol.CompanyID = com.CompanyID AND pol.CommitmentID = com.CommitmentID
WHERE com.Type = 'I'
GO

--[MinVersion(Hash = 9d7ffaf254729a9139eef83630816f80378225340d30fd9a24906d4d8c8fbc21)]
UPDATE com
SET com.BranchID = ISNULL(up.DefBranchID, br.BranchID)
FROM [PMCommitment] com
LEFT JOIN UserPreferences up ON up.CompanyID = com.CompanyID AND up.UserId = com.CreatedByID
LEFT JOIN
(
	SELECT MIN(BranchID) AS BranchID, CompanyID
	FROM Branch 
	WHERE DeletedDatabaseRecord = 0 AND Active = 1 
	GROUP BY CompanyID
) AS br ON br.CompanyID = com.CompanyID
WHERE com.Type = 'E'
GO

--[MinVersion(Hash = 713d7cfccf1f68a7502677ee6eff9f14f7a2ed3dbda54bd33a55373af712aba4)]
INSERT INTO [PMAllocationDetail]
           ([CompanyID]
           ,[AllocationID]
           ,[StepID]
           ,[Description]
           ,[SelectOption]
           ,[Post]
           ,[QtyFormula]
           ,[BillableQtyFormula]
           ,[AmountFormula]
           ,[DescriptionFormula]
           ,[RangeStart]
           ,[RangeEnd]
           ,[RateTypeID]
           ,[SourceBranchID]
           ,[TargetBranchID]
           ,[AccountGroupFrom]
           ,[AccountGroupTo]
           ,[Method]
           ,[UpdateGL]
           ,[ProjectOrigin]
           ,[ProjectID]
           ,[TaskOrigin]
           ,[TaskID]
           ,[BranchOrigin]
           ,[AccountGroupOrigin]
           ,[AccountGroupID]
           ,[AccountOrigin]
           ,[AccountID]
           ,[SubMask]
           ,[SubID]
           ,[OffsetBranchOrigin]
           ,[OffsetProjectOrigin]
           ,[OffsetProjectID]
           ,[OffsetTaskOrigin]
           ,[OffsetTaskID]
           ,[OffsetAccountGroupOrigin]
           ,[OffsetAccountGroupID]
           ,[OffsetAccountOrigin]
           ,[OffsetAccountID]
           ,[OffsetSubMask]
           ,[OffsetSubID]
           ,[Reverse]
           ,[NoRateOption]
           ,[DateSource]
           ,[GroupByItem]
           ,[GroupByEmployee]
           ,[GroupByDate]
           ,[GroupByVendor]
           ,[AllocateZeroAmount]
           ,[AllocateZeroQty]
           ,[AllocateNonBillable]
           ,[MarkAsNotAllocated]
           ,[CopyNotes]
           ,[CreatedByID]
           ,[CreatedByScreenID]
           ,[CreatedDateTime]
           ,[LastModifiedByID]
           ,[LastModifiedByScreenID]
           ,[LastModifiedDateTime]
           ,[NoteID])

SELECT [CompanyID]
      ,[AllocationID]
      ,[StepID]
      ,[Description]
      ,[SelectOption]
      ,[Post]
      ,[QtyFormula]
      ,[BillableQtyFormula]
      ,[AmountFormula]
      ,[DescriptionFormula]
      ,[RangeStart]
      ,[RangeEnd]
      ,[RateTypeID]
      ,[SourceBranchID]
      ,[TargetBranchID]
      ,[AccountGroupFrom]
      ,[AccountGroupTo]
      ,[Method]
      ,[UpdateGL]
      ,[ProjectOrigin]
      ,[ProjectID]
      ,[TaskOrigin]
      ,[TaskID]
      ,[BranchOrigin]
      ,[AccountGroupOrigin]
      ,[AccountGroupID]
      ,[AccountOrigin]
      ,[AccountID]
      ,[SubMask]
      ,[SubID]
      ,[OffsetBranchOrigin]
      ,[OffsetProjectOrigin]
      ,[OffsetProjectID]
      ,[OffsetTaskOrigin]
      ,[OffsetTaskID]
      ,[OffsetAccountGroupOrigin]
      ,[OffsetAccountGroupID]
      ,[OffsetAccountOrigin]
      ,[OffsetAccountID]
      ,[OffsetSubMask]
      ,[OffsetSubID]
      ,[Reverse]
      ,[NoRateOption]
      ,[DateSource]
      ,[GroupByItem]
      ,[GroupByEmployee]
      ,[GroupByDate]
      ,[GroupByVendor]
      ,[AllocateZeroAmount]
      ,[AllocateZeroQty]
      ,[AllocateNonBillable]
      ,[MarkAsNotAllocated]
      ,[CopyNotes]
      ,[CreatedByID]
      ,[CreatedByScreenID]
      ,[CreatedDateTime]
      ,[LastModifiedByID]
      ,[LastModifiedByScreenID]
      ,[LastModifiedDateTime]
      ,[NoteID]
  FROM [PMAllocationStep]
GO

--[IfExists(Column = PMAllocationStep.UsrTaskCD)]
--[MinVersion(Hash = 8fcaf359b833ec87c2911e07c160e5cc4d0afe06e106a68c210c21d55f9964bf)]
UPDATE PMAllocationDetail 
SET PMAllocationDetail.TaskCD = 
	(SELECT UsrTaskCD FROM PMAllocationStep 
	WHERE 
		PMAllocationDetail.CompanyID = PMAllocationStep.CompanyID
		AND PMAllocationDetail.AllocationID = PMAllocationStep.AllocationID 
		AND PMAllocationDetail.StepID = PMAllocationStep.StepID)
GO

--[IfExists(Column = PMAllocationStep.UsrOffsetTaskCD)]
--[MinVersion(Hash = 7eaccb27aabc5c90cd1983e62090bfbd53ab9cd9edce28296f0cb1d05dcefc99)]
UPDATE PMAllocationDetail 
SET PMAllocationDetail.OffsetTaskCD = 
	(SELECT UsrOffsetTaskCD FROM PMAllocationStep 
	WHERE 
		PMAllocationDetail.CompanyID = PMAllocationStep.CompanyID 
		AND PMAllocationDetail.AllocationID = PMAllocationStep.AllocationID 
		AND PMAllocationDetail.StepID = PMAllocationStep.StepID)
GO

--[MinVersion(Hash = cb0beba84016ce2f4e7a57884e5068e4a2c1ae5839b72f308bef13ea62d46b53)]
INSERT INTO PMAllocationDetailKvExt SELECT * FROM PMAllocationStepKvExt
GO

--[MinVersion(Hash = cec0946ab88f0043d2870678846b1ff680e89ed693e4d44b77e534d56a792d14)]
UPDATE b 
SET b.RevenueInventoryID = i.InventoryID
FROM PMBudget b
INNER JOIN InventoryItem i ON b.CompanyID = i.CompanyID and i.ItemStatus='XX'
WHERE b.RevenueInventoryID IS NULL AND b.Type='I'
GO

--[MinVersion(Hash = 43d0a436fb03692fb766d1cbb7a62642bf92e4ee5305e316e14ba09a5fe19605)]
UPDATE EPSetup
SET RequireRefNbrInExpenseReceipts = 0
GO

--[MinVersion(Hash = caf14ac4439ec3165eb51dbc0f81dacaf78fecf2e26373b67090b1f71eff449f)]
UPDATE EPSetup
SET PostSummarizedCorpCardExpenseReceipts = 0
GO

--Remove Construction edition guide added by customization
--[MinVersion(Hash = e32bcf7d29dc1f35ccc9375a30b8f3238e19f48a98cab6858cbd0d523ecbb98c)]
if EXISTS(select * from WikiPage where PageID = '1b1e7aaa-c108-4b2e-a987-9df61cb5788c' )
begin
	delete WikiRevision
	from WikiRevision wr 
	inner join WikiPageLanguage wpl on wr.CompanyID = wpl.CompanyID and wr.PageID = wpl.PageID and wr.[Language] = wpl.[Language]
	inner join WikiPage wp on wr.CompanyID = wp.CompanyID and wr.PageID = wp.PageID
	where wp.WikiID = '1b1e7aaa-c108-4b2e-a987-9df61cb5788c' or wp.PageID = '1b1e7aaa-c108-4b2e-a987-9df61cb5788c' 

	delete WikiPageLanguage
	from WikiPageLanguage wpl 
	inner join WikiPage wp on wpl.CompanyID = wp.CompanyID and wpl.PageID = wp.PageID
	where wp.WikiID = '1b1e7aaa-c108-4b2e-a987-9df61cb5788c' or wp.PageID = '1b1e7aaa-c108-4b2e-a987-9df61cb5788c' 

	delete from WikiAccessRights where PageID = '1b1e7aaa-c108-4b2e-a987-9df61cb5788c'

	delete from SiteMap where NodeID = '1b1e7aaa-c108-4b2e-a987-9df61cb5788c'

	delete from WikiPage where PageID = '1b1e7aaa-c108-4b2e-a987-9df61cb5788c' or WikiID = '1b1e7aaa-c108-4b2e-a987-9df61cb5788c'

	delete WikiCss
	from WikiCss css 
	inner join WikiDescriptor wd on css.CompanyID = wd.CompanyID and css.CssID = wd.CssID
	where wd.PageID = '1b1e7aaa-c108-4b2e-a987-9df61cb5788c'

	delete from WikiDescriptor where PageID = '1b1e7aaa-c108-4b2e-a987-9df61cb5788c'

	delete from WikiDescriptorExt where PageID = '1b1e7aaa-c108-4b2e-a987-9df61cb5788c'
end
GO

--Initialize CostCodeID with default.
--[MinVersion(Hash = 45ab8f85f673c888ac6b95888e4ebda3006d56f7305d6bd7094c3e15026b10c6)]
update t
set t.CostCodeID = c.CostCodeID
from PMTaskAllocTotal t
inner join PMCostCode c on t.CompanyID=c.CompanyID and c.IsDefault = 1
where t.CostCodeID is null
GO

--Initialize CostCodeID with default.
--[MinVersion(Hash = 45ab8f85f673c888ac6b95888e4ebda3006d56f7305d6bd7094c3e15026b10c6)]
update t
set t.CostCodeID = c.CostCodeID
from PMTaskAllocTotal t
inner join PMCostCode c on t.CompanyID=c.CompanyID and c.IsDefault = 1
where t.CostCodeID is null
GO

--[MinVersion(Hash = a3ffbaead568a524013b566dea1792b6e83f1c2505d995ff23aa6bcb72c431c4)]
UPDATE EPEmployee
SET TimeCardRequired = 0
WHERE TimeCardRequired IS NULL
GO

--Construction subcontract migration
--[MinVersion(Hash = c8f3d859fc146da81247e5730db5f3eefb04639e310e3ab6e7ec725cae099985)]
UPDATE POSetup
        SET RequireSubcontractControlTotal = 0
			, SubcontractRequestApproval = 0;
GO

--[MinVersion(Hash = b07cb9914550805bf60eda84acc2212454a635604cf61304de0caba199806aac)]
IF (EXISTS(SELECT 1 FROM sys.columns WHERE object_id = object_id('POSetup') AND name = 'UsrSubcontractNumberingID'))
BEGIN
EXEC sp_executesql N'
UPDATE POSetup SET SubcontractNumberingID = UsrSubcontractNumberingID
ALTER TABLE POSetup DROP COLUMN UsrSubcontractNumberingID'
END
GO

--[mysql: Skip]
--[MinVersion(Hash = d51635ff39cd405b58cdf83175b4e390edb69eaa0ab4015a374a3b7b43fb5c93)]
IF COL_LENGTH('POSetup','UsrIsSubcontractSetupSaved') IS NOT NULL
BEGIN
    IF COL_LENGTH('POSetup','UsrIsSubcontractSetupSaved') IS NULL
        EXEC sp_rename
        @objname = 'POSetup.UsrIsSubcontractSetupSaved',
        @newname = 'UsrIsSubcontractSetupSaved',
        @objtype = 'COLUMN'
    ELSE
    BEGIN
        DECLARE @ConstraintName nvarchar(200)

        SELECT @ConstraintName = [Name]
        FROM SYS.DEFAULT_CONSTRAINTS
        WHERE PARENT_OBJECT_ID = OBJECT_ID('POSetup')
            AND PARENT_COLUMN_ID = (
                SELECT column_id
                FROM sys.columns
                WHERE NAME = N'UsrIsSubcontractSetupSaved'
                    AND object_id = OBJECT_ID(N'POSetup'))

        IF @ConstraintName IS NOT NULL
            EXEC('ALTER TABLE POSetup DROP CONSTRAINT ' + @ConstraintName)
    END
END
GO

--[MinVersion(Hash = 4101a0b3be942823bc5456efb73ae3399dbe7399117eab1d25b0952003d9a077)]
IF (EXISTS(SELECT 1 FROM sys.columns WHERE object_id = object_id('POSetup') AND name = 'UsrIsSubcontractSetupSaved'))
BEGIN
EXEC sp_executesql N'
UPDATE POSetup SET IsSubcontractSetupSaved = UsrIsSubcontractSetupSaved
ALTER TABLE POSetup DROP COLUMN UsrIsSubcontractSetupSaved'
END
GO

--[MinVersion(Hash = 9f3c27050a08834222a5bb84d54d41cb11ea690b1c4d3838c652dffb1be68eb4)]
IF (EXISTS(SELECT 1 FROM sys.columns WHERE object_id = object_id('POSetup') AND name = 'UsrRequireSubcontractControlTotal'))
BEGIN
EXEC sp_executesql N'
UPDATE POSetup SET RequireSubcontractControlTotal = UsrRequireSubcontractControlTotal
ALTER TABLE POSetup DROP COLUMN UsrRequireSubcontractControlTotal'
END
GO

--[MinVersion(Hash = d6e3384c316184cc8cb684978ed0c8def7ee2a6b0141900e523d592efdbf6759)]
IF (EXISTS(SELECT 1 FROM sys.columns WHERE object_id = object_id('POSetup') AND name = 'UsrSubcontractRequestApproval'))
BEGIN
EXEC sp_executesql N'
UPDATE POSetup SET SubcontractRequestApproval = UsrSubcontractRequestApproval
ALTER TABLE POSetup DROP COLUMN UsrSubcontractRequestApproval'
END
GO

--[MinVersion(Hash = 0f765d488352b2d025dddf3f6a9b78dc72b431075bb3d4c52373dadf0c08e118)]
UPDATE PMTran
SET ExcludedFromBalance = 1
WHERE RemainderOfTranID IS NOT NULL
GO

--[MinVersion(Hash = d7972bc029ae42811d29b4a7188ce819094004147b6b0e244bc1f4ade4f9f0d1)]
UPDATE PMTran
SET ARTranType = NULL,
    ARRefNbr = NULL
WHERE TaskID IS NOT NULL AND RefLineNbr IS NULL
GO

--[MinVersion(Hash = b0478e7bb893fe8090bfee026bc033892d43fe4ada9f699dec309dc78b97e8ee)]
UPDATE PMTran
SET ExcludedFromBilling = 1  
WHERE TaskID IS NOT NULL AND Reversed = 1
GO

--[MinVersion(Hash = 108c620f62732ebbfc1065f692c1df08f20be23cd3c4efd9cc849dabf4e331c6)]
UPDATE PMTran
SET OrigTranID = DuplicateOfTranID
WHERE TaskID IS NOT NULL AND OrigTranID IS NULL AND DuplicateOfTranID IS NOT NULL
GO

--[mysql: Native]
--[mssql: Skip]
--[MinVersion(Hash = 558f73979ac2ccb7949669e3801c2aa3a0a7ea7d661586b7ca5de44b0e88fed4)]
CREATE INDEX PMRegister_Temp ON PMRegister(CompanyID, OrigDocType, OrigDocNbr)
GO

--[mysql: Skip]
--[mssql: Native]
--[MinVersion(Hash = 9a04e76adf4b35e12a8da97bc9d76522030c38a55fd90f8568013fdb013dddb2)]
CREATE NONCLUSTERED INDEX PMRegister_Temp ON PMRegister(CompanyID, OrigDocType, OrigDocNbr)
GO

--[MinVersion(Hash = 286224b03227acb2e00a90e6407678de69c70eb3ee5969ecc462f584c0a324cf)]
UPDATE r
SET r.OrigNoteID = tc.NoteID
FROM PMRegister r
INNER JOIN EPTimeCard tc ON tc.TimeCardCD = r.OrigDocNbr AND tc.CompanyID = r.CompanyID
WHERE r.OrigDocType = 'TC'
GO

--[MinVersion(Hash = c356b64e24e23f414290c8c127e8aba9fa6edeb7d4951ff56b24c617c6f2ebda)]
UPDATE r
SET r.OrigNoteID = tc.NoteID
FROM PMRegister r
INNER JOIN EPEquipmentTimeCard tc ON tc.TimeCardCD = r.OrigDocNbr AND tc.CompanyID = r.CompanyID
WHERE r.OrigDocType = 'ET'
GO

--[MinVersion(Hash = a07d76bcb74f0f3dddbd1f7a2078ef5b4c1d90f77f3840bc2103b81b6019e439)]
UPDATE r
SET r.OrigNoteID = ec.NoteID
FROM PMRegister r
INNER JOIN EPExpenseClaim ec ON ec.RefNbr = r.OrigDocNbr AND ec.CompanyID = r.CompanyID
WHERE r.OrigDocType = 'EC'
GO

--[MinVersion(Hash = 36c183b2b9ac72fa258b0dbda533aae62296ba8ecb2e50753989ac443c2459c2)]
UPDATE r
SET r.OrigNoteID = orr.NoteID
FROM PMRegister r
INNER JOIN PMRegister orr ON orr.RefNbr = r.OrigDocNbr AND orr.CompanyID = r.CompanyID
WHERE r.OrigDocType = 'RV'
GO

--[MinVersion(Hash = 86c605939f9f77df33a0a387e18fe574abcf0fd50b2d8e317af9146502c03220)]
UPDATE r
SET r.OrigNoteID = orr.NoteID
FROM PMRegister r
INNER JOIN PMRegister orr ON orr.RefNbr = r.OrigDocNbr AND orr.CompanyID = r.CompanyID
WHERE r.OrigDocType = 'AR' AND r.CreatedByScreenID = 'PM505000'
GO

--[MinVersion(Hash = 17bd5e80960a6be4435a4fab833e38be9419fd00895c7b6747bb39936c14d37c)]
UPDATE r
SET r.OrigNoteID = pf.NoteID
FROM PMRegister r
INNER JOIN PMProforma pf ON pf.RefNbr = r.OrigDocNbr AND pf.CompanyID = r.CompanyID
WHERE r.OrigDocType = 'AR' AND r.CreatedByScreenID = 'PM307000'
GO

--[MinVersion(Hash = 8892de7b0a9afd79b76bd9f6b60d73b70c1ef12cb352d471df15b61f75a3e70d)]
DROP INDEX PMRegister_Temp ON PMRegister
GO

--[MinVersion(Hash = 133912a6851b7678f54bc98000f926cd537748ffef22375f726628def60aa55d)]
INSERT INTO Note (CompanyID, NoteText, GraphType, EntityType, NoteID)
SELECT r.CompanyId, '', 'PX.Objects.PM.RegisterEntry', 'PX.Objects.PM.PMRegister', r.NoteID
FROM PMRegister r
WHERE r.NoteID NOT IN (SELECT NoteID FROM Note)
GO

--[MinVersion(Hash = af7da91788e9cb0e886e2fa350762dce3f70b6a90fd1f409f5c62bc9719be036)]
UPDATE POSetup
        SET SubcontractNumberingID = 'SUBCONTR'
		WHERE SubcontractNumberingID IS NULL
GO

--[MinVersion(Hash = 624a117c1d9fa1d6e222f2ff16e25420c59f1f470e2ce544606b2886ad1a84cc)]
UPDATE POSetup
SET IsSubcontractSetupSaved = 0
WHERE IsSubcontractSetupSaved IS NULL
GO

--[MinVersion(Hash = f080c4108d0aca31c0bdd751142f9326033f11c97a71b30cba31f3a3f4f97234)]
IF (EXISTS(SELECT 1 FROM sys.columns WHERE object_id = object_id('POOrder') AND name = 'UsrBaseType'))
BEGIN
EXEC sp_executesql N'
UPDATE POOrder
        SET OrderType = ''RS''
        WHERE UsrBaseType = ''Subcontract'''
END
GO

--[MinVersion(Hash = ec97e4bc6efb3213603c41a05cb54efa668b87b5dea832bddb1184ce338b135b)]
IF (EXISTS(SELECT 1 FROM sys.columns WHERE object_id = object_id('POOrder') AND name = 'UsrBaseType'))
BEGIN
EXEC sp_executesql N'
UPDATE pol
        SET OrderType = ''RS''
        FROM dbo.POLine pol
        INNER JOIN dbo.POOrder po
            ON po.OrderNbr = pol.OrderNbr
            AND po.OrderType = pol.OrderType
            AND po.CompanyID = pol.CompanyID
        WHERE po.UsrBaseType = ''Subcontract''
ALTER TABLE POOrder DROP COLUMN UsrBaseType'
END
GO

--[MinVersion(Hash = 431d8ae6be7202dff102aa7e1490c340a179d515b6238088b0d8466942de83c6)]
IF (EXISTS(SELECT 1 FROM sys.columns WHERE object_id = object_id('POSetupApproval') AND name = 'UsrBaseType'))
BEGIN
EXEC sp_executesql N'
UPDATE POSetupApproval
        SET OrderType = ''RS''
        WHERE UsrBaseType = ''Subcontract''
ALTER TABLE POSetupApproval DROP COLUMN UsrBaseType'
END
GO

--[MinVersion(Hash = fc2be143954d37d0d388125fe08d45645c5988a92b337d6ad8839205d02f7170)]
DELETE FROM FilterHeader
    WHERE FilterID = 5135
        AND ScreenID = 'SELECTOR'
        AND FilterName = 'Items';
DELETE FROM FilterHeader
    WHERE FilterID = 5134
        AND ScreenID = 'SELECTOR'
        AND FilterName = 'Account Groups'
GO

--[MinVersion(Hash = fa10c28c431e975a276edf69241e7edb8fd024d6067e285327b76c781f368880)]
DELETE FROM FilterHeader
    WHERE FilterID = 53217
        AND ScreenID = 'SELECTOR';
GO

--[MinVersion(Hash = f2d613a1a52bac6c5e5880d3cb812af42894e4dd4b065b073862e3b6b8bb7e45)]
IF (EXISTS(SELECT 1 FROM sys.tables WHERE object_id = object_id('PMChangeOrderLineCLExt')))
BEGIN
EXEC sp_executesql N'
UPDATE o
        SET o.CommitmentType = ext.CommitmentType
	FROM PMChangeOrderLine o
		INNER JOIN PMChangeOrderLineCLExt ext
			ON (o.CompanyID = ext.CompanyID
					AND o.RefNbr = ext.RefNbr
					AND o.LineNbr = ext.LineNbr)
					
DROP TABLE PMChangeOrderLineCLExt'
END
GO

--[MinVersion(Hash = 58c830f50acac9ed261845876345e6e365fe13c46b6e18846ee3547813862ce8)]
UPDATE PMChangeOrderLine
SET CommitmentType = 'Purchase Order'
WHERE CommitmentType IS NULL
GO

--[MinVersion(Hash = 0e924cff0801c779261ec3acd28185fd5827679f96eef95956ade3b87a6c731a)]
UPDATE EPAssignmentMap
SET GraphType = 'PX.Objects.CN.Subcontracts.SC.Graphs.SubcontractEntry'
WHERE GraphType IN ('CcsLego.SC.Graphs.SubcontractEntry', 'CcsLego.Subcontracts.SC.Graphs.SubcontractEntry')

UPDATE Note
SET GraphType = 'PX.Objects.CN.Subcontracts.SC.Graphs.SubcontractEntry'
WHERE GraphType IN ('CcsLego.SC.Graphs.SubcontractEntry', 'CcsLego.Subcontracts.SC.Graphs.SubcontractEntry')
GO

--[MinVersion(Hash = d3f55a159c3b4e0e745ff2a5da329888cb8637da31b8c3385fd13a9f7d122610)]
UPDATE i
SET i.EntityType = 'PX.Objects.CN.Subcontracts.SC.DAC.Subcontract'
FROM            
	SearchIndex i
    INNER JOIN POOrder o
		ON i.NoteID = o.NoteID
			AND i.CompanyID = o.CompanyID
            AND o.OrderType = 'RS'
WHERE i.EntityType IN ('PX.Objects.PO.POOrder', 'CcsLego.Subcontracts.SC.DAC.Subcontract')
GO

--[MinVersion(Hash = 9e4c7399a6461a7553d65ed829915b26b42d23393d04a5a5c095d248b56ffc69)]
UPDATE GITable
SET Name = 'PX.Objects.CN.Subcontracts.SC.DAC.Subcontract'
WHERE Name = 'CcsLego.Subcontracts.SC.DAC.Subcontract'

UPDATE DashboardParameter
SET ObjectName = 'PX.Objects.CN.Subcontracts.SC.DAC.Subcontract'
WHERE ObjectName = 'CcsLego.Subcontracts.SC.DAC.Subcontract'
GO

--[OldHash(Hash = 78fcc68d68658a3e953b5cb7c28b6c2e8381c7b4127e789ac79a703b299fdf9b)]
--[OldHash(Hash = ede9cd3194b895594bfaa6eaa3bd7095cae38b04f4a6e71ef72cb5716b08db3c)]
--[MinVersion(Hash = 2b7c889ba0f215ec34a5d835150ca1200d09de3300ec685f831b22292f653032)]
DELETE FROM AUStep
WHERE ScreenID = 'SC301000'
		AND CompanyID > 1

DELETE FROM AUStepField
WHERE ScreenID = 'SC301000'
		AND CompanyID > 1

DELETE FROM AUStepFilter
WHERE ScreenID = 'SC301000'
		AND CompanyID > 1

DELETE FROM AUStepFill
WHERE ScreenID = 'SC301000'
		AND CompanyID > 1

DELETE FROM AUAction
WHERE ScreenID = 'SC301000'
		AND CompanyID > 1

DELETE FROM AUAction
WHERE ScreenID = 'SC301000'
		AND CompanyID > 1

DELETE FROM AUStepCombo
WHERE ScreenID = 'SC301000'
		AND CompanyID > 1

DELETE FROM AUStepAction
WHERE ScreenID = 'SC301000'
		AND CompanyID > 1
GO

--end of Construction subcontract migration
--start Joint check & Compliance migration
--[MinVersion(Hash = 924b6ff7c4aeecd67592df36ba443c47cc794d3fc111b4f00938d14b425377e8)]
IF (EXISTS(SELECT 1 FROM sys.columns WHERE object_id = object_id('VendorClass') AND name = 'UsrShouldGenerateLienWaivers'))
BEGIN
EXEC sp_executesql N'
UPDATE VendorClass
        SET ShouldGenerateLienWaivers = UsrShouldGenerateLienWaivers';

EXEC sp_executesql N'ALTER TABLE VendorClass DROP COLUMN UsrShouldGenerateLienWaivers';
END
GO

--[MinVersion(Hash = dba693bdd3a36d82a3172b98e7ed410364ed606c686526deb876a178ca007487)]
IF (EXISTS(SELECT 1 FROM sys.columns WHERE object_id = object_id('Vendor') AND name = 'UsrShouldGenerateLienWaivers'))
BEGIN
EXEC sp_executesql N'
UPDATE Vendor
        SET ShouldGenerateLienWaivers = UsrShouldGenerateLienWaivers';

EXEC sp_executesql N'ALTER TABLE Vendor DROP COLUMN UsrShouldGenerateLienWaivers';
END
GO

--[MinVersion(Hash = 11f6362afe6fc2af8be9278a87d9488ec707a44a2df63b2c89933029d0c02654)]
IF (EXISTS(SELECT 1 FROM sys.columns WHERE object_id = object_id('Vendor') AND name = 'UsrVendorDefaultCostCodeId'))
BEGIN
EXEC sp_executesql N'
UPDATE Vendor
        SET VendorDefaultCostCodeId = UsrVendorDefaultCostCodeId';

EXEC sp_executesql N'ALTER TABLE Vendor DROP COLUMN UsrVendorDefaultCostCodeId';
END
GO

--[MinVersion(Hash = 7cc5586ea889050b6ce8bfac21f7bb94f760d96eb2eda668e592cc9af1cd0d70)]
IF (EXISTS(SELECT 1 FROM sys.columns WHERE object_id = object_id('Vendor') AND name = 'UsrVendorDefaultInventoryId'))
BEGIN
EXEC sp_executesql N'
UPDATE Vendor
        SET VendorDefaultInventoryId = UsrVendorDefaultInventoryId';

EXEC sp_executesql N'ALTER TABLE Vendor DROP COLUMN UsrVendorDefaultInventoryId';
END
GO

--[MinVersion(Hash = 53f47b5eba9b3c3f76fe4868a99d76fc955d39a00f107de2faaba1b7935a161a)]
IF (EXISTS(SELECT 1 FROM sys.columns WHERE object_id = object_id('Contract') AND name = 'UsrThroughDateSourceConditional'))
BEGIN
EXEC sp_executesql N'
UPDATE Contract
        SET ThroughDateSourceConditional = UsrThroughDateSourceConditional';

EXEC sp_executesql N'ALTER TABLE Contract DROP COLUMN UsrThroughDateSourceConditional';
END
GO

--[MinVersion(Hash = d4705c0f3f65fffd3c93aabda95e2165f40c8a149914392a3a33fb55e7a27908)]
IF (EXISTS(SELECT 1 FROM sys.columns WHERE object_id = object_id('Contract') AND name = 'UsrThroughDateSourceUnconditional'))
BEGIN
EXEC sp_executesql N'
UPDATE Contract
        SET ThroughDateSourceUnconditional = UsrThroughDateSourceUnconditional';

EXEC sp_executesql N'ALTER TABLE Contract DROP COLUMN UsrThroughDateSourceUnconditional';
END
GO

--[MinVersion(Hash = f632dd9ddf4a43b09f83de25769d63f174eaf39020f4c1e958066d24014f3470)]
INSERT INTO POOrderPrepayment (
	      CompanyID, OrderType, OrderNbr,
	      APDocType, APRefNbr, CuryInfoID,
	      IsRequest, CuryLineTotal, LineTotal,
	      CuryAppliedAmt,
	      AppliedAmt,
	      CreatedByID, CreatedByScreenID, CreatedDateTime,
	      LastModifiedByID, LastModifiedByScreenID, LastModifiedDateTime)
    SELECT po.CompanyID, po.OrderType, po.OrderNbr,
	apr.DocType, apr.RefNbr, po.CuryInfoID,
	1, api.CuryLineTotal, api.LineTotal,
	CASE WHEN apr.Released = 1 AND apr.Voided = 0 THEN apr.CuryOrigDocAmt ELSE 0 END,
	CASE WHEN apr.Released = 1 AND apr.Voided = 0 THEN apr.OrigDocAmt ELSE 0 END,
	apr.CreatedByID, apr.CreatedByScreenID, apr.CreatedDateTime,
	apr.LastModifiedByID, apr.LastModifiedByScreenID, apr.LastModifiedDateTime
FROM 
	APTran apt
INNER JOIN APRegister apr
	ON apt.CompanyID = apr.CompanyID
		AND apt.TranType = apr.DocType
		AND apt.RefNbr = apr.RefNbr
INNER JOIN APInvoice api 
	ON api.CompanyID = apr.CompanyID 
		AND api.DocType = apr.DocType 
		AND api.RefNbr = apr.RefNbr
INNER JOIN POOrder po 
	ON po.CompanyID = apt.CompanyID 
		AND po.OrderType = apt.POOrderType
		AND po.OrderNbr = apt.PONbr
		AND apt.POOrderType = 'RS'
LEFT JOIN POOrderPrepayment op
	ON op.CompanyID = apr.CompanyID 
		AND op.APDocType = apr.DocType 
		AND op.APRefNbr = apr.RefNbr
		AND op.OrderType = po.OrderType 
		AND op.OrderNbr = po.OrderNbr
WHERE op.CompanyID IS NULL AND apr.DocType = 'PPM'
GROUP BY po.CompanyID, po.OrderType, po.OrderNbr,
	apr.DocType, apr.RefNbr, po.CuryInfoID,
	api.CuryLineTotal, api.LineTotal,
	CASE WHEN apr.Released = 1 AND apr.Voided = 0 THEN apr.CuryOrigDocAmt ELSE 0 END,
	CASE WHEN apr.Released = 1 AND apr.Voided = 0 THEN apr.OrigDocAmt ELSE 0 END,
	apr.CreatedByID, apr.CreatedByScreenID, apr.CreatedDateTime,
	apr.LastModifiedByID, apr.LastModifiedByScreenID, apr.LastModifiedDateTime

UPDATE apr
SET OrigModule = 'PO'
FROM APRegister apr
INNER JOIN POOrderPrepayment op
ON op.CompanyID = apr.CompanyID AND op.APDocType = apr.DocType AND op.APRefNbr = apr.RefNbr
WHERE apr.DocType = 'PPM' AND (apr.OrigModule IS NULL OR apr.OrigModule <> 'PO')

UPDATE pop
SET PayDocType = apd.AdjgDocType, PayRefNbr = apd.AdjgRefNbr
FROM POOrderPrepayment pop
INNER JOIN APAdjust apd ON apd.CompanyID = pop.CompanyID AND apd.AdjdDocType = pop.APDocType AND apd.AdjdRefNbr = pop.APRefNbr
WHERE pop.PayDocType IS NULL AND pop.PayRefNbr IS NULL AND pop.IsRequest = 1
	AND apd.Released = 1 AND apd.CuryAdjdAmt > 0

UPDATE pop
SET PayDocType = pod.AdjgDocType, PayRefNbr = pod.AdjgRefNbr
FROM POOrderPrepayment pop
INNER JOIN POAdjust pod ON pod.CompanyID = pop.CompanyID AND pod.AdjgDocType = pop.APDocType AND pod.AdjgRefNbr = pop.APRefNbr
	AND pod.AdjdOrderType = pop.OrderType AND pod.AdjdOrderNbr = pop.OrderNbr
WHERE pop.PayDocType IS NULL AND pop.PayRefNbr IS NULL AND pop.IsRequest = 0
	AND pod.Released = 1 AND pod.CuryAdjdAmt > 0 AND pod.IsRequest = 0
GO

--[IfExists(Column = PMBudget.UsrCuryRetainedAmount)]
--[MinVersion(Hash = 8ad3c109398827562d46aab3d08a054a4279da84657c9940dd6b5e6c26793c5f)]
UPDATE PMBudget SET CuryRetainedAmount = UsrCuryRetainedAmount WHERE UsrCuryRetainedAmount IS NOT NULL
ALTER TABLE PMBudget DROP COLUMN UsrCuryRetainedAmount
GO

--[IfExists(Column = PMBudget.UsrRetainedAmount)]
--[MinVersion(Hash = 715d48538ca92c2774d918448b499064411130534880a53ba84cf69d25089e26)]
UPDATE PMBudget SET RetainedAmount = UsrRetainedAmount WHERE UsrRetainedAmount IS NOT NULL
ALTER TABLE PMBudget DROP COLUMN UsrRetainedAmount
GO

--[IfExists(Column = PMBudget.UsrCuryDraftRetainedAmount)]
--[MinVersion(Hash = 9816c0449aa2bca838a3095bbdd1af90ef78f94303e9acb17bd1e30cfb28e7b1)]
UPDATE PMBudget SET CuryDraftRetainedAmount = UsrCuryDraftRetainedAmount WHERE UsrCuryDraftRetainedAmount IS NOT NULL
ALTER TABLE PMBudget DROP COLUMN UsrCuryDraftRetainedAmount
GO

--[IfExists(Column = PMBudget.UsrDraftRetainedAmount)]
--[MinVersion(Hash = d6845d53e76f9f6817afcc704db16c42dc59516032ae87a78e816528cd81a87c)]
UPDATE PMBudget SET DraftRetainedAmount = UsrDraftRetainedAmount WHERE UsrDraftRetainedAmount IS NOT NULL
ALTER TABLE PMBudget DROP COLUMN UsrDraftRetainedAmount
GO

--[IfExists(Column = PMBudget.UsrRetainageMaxPct)]
--[MinVersion(Hash = 9990535c9f38b9f80ef700c83258a6e07eae1c938013ccf1a3d436bf4658b196)]
UPDATE PMBudget SET RetainageMaxPct = UsrRetainageMaxPct WHERE UsrRetainageMaxPct IS NOT NULL
ALTER TABLE PMBudget DROP COLUMN UsrRetainageMaxPct
GO

--[IfExists(Column = Contract.UsrRetainageMode)]
--[MinVersion(Hash = f9efdde37d1804c3a81357a1ae11e42ebe2cb91a9d0b6811a20dbd4caa73e166)]
UPDATE [Contract] SET RetainageMode = UsrRetainageMode WHERE UsrRetainageMode IS NOT NULL
ALTER TABLE [Contract] DROP COLUMN UsrRetainageMode
GO

--[IfExists(Column = Contract.UsrIncludeCO)]
--[MinVersion(Hash = f930ddebb8a8943eebf8221a0900c42cea3d42eab1c6bafe110da971aaa20470)]
UPDATE [Contract] SET IncludeCO = UsrIncludeCO WHERE UsrIncludeCO IS NOT NULL
ALTER TABLE [Contract] DROP COLUMN UsrIncludeCO
GO

--[IfExists(Column = Contract.UsrSteppedRetainage)]
--[MinVersion(Hash = 123eca1eb36610a86e9ef0b0f232e37cfdeebf31820dca1ddf2bf9c9a3b1db29)]
UPDATE [Contract] SET SteppedRetainage = UsrSteppedRetainage WHERE UsrSteppedRetainage IS NOT NULL
ALTER TABLE [Contract] DROP COLUMN UsrSteppedRetainage
GO

--[IfExists(Column = Contract.UsrRetainageMaxPct)]
--[MinVersion(Hash = f1474b5964fb8fd5bee3ecf94988d9c8f6b79adea07f11b601bdf302f7e7c82b)]
UPDATE [Contract] SET RetainageMaxPct = UsrRetainageMaxPct WHERE UsrRetainageMaxPct IS NOT NULL
ALTER TABLE [Contract] DROP COLUMN UsrRetainageMaxPct
GO

--[IfExists(Column = PMProformaLine.UsrCuryAllocatedRetainedAmount)]
--[MinVersion(Hash = d195ddaad8bd0e4cf306ca61cba303d5d5c3516b928f30178a61ff8393bf822c)]
UPDATE PMProformaLine SET CuryAllocatedRetainedAmount = UsrCuryAllocatedRetainedAmount WHERE UsrCuryAllocatedRetainedAmount IS NOT NULL
ALTER TABLE PMProformaLine DROP COLUMN UsrCuryAllocatedRetainedAmount
GO

--[IfExists(Column = PMProformaLine.UsrAllocatedRetainedAmount)]
--[MinVersion(Hash = 9c9962e775e1ed85fbf2791062120dd0b89fcd0218fad174d4c36baa3a1fdb2b)]
UPDATE PMProformaLine SET AllocatedRetainedAmount = UsrAllocatedRetainedAmount WHERE UsrAllocatedRetainedAmount IS NOT NULL
ALTER TABLE PMProformaLine DROP COLUMN UsrAllocatedRetainedAmount
GO

--[IfExists(Column = PMProforma.UsrRetainagePct)]
--[MinVersion(Hash = f14546dd710346ccfd8488c2042302512078bfb8844941056d5da0efd70ca774)]
UPDATE PMProforma SET RetainagePct = UsrRetainagePct WHERE UsrRetainagePct IS NOT NULL
ALTER TABLE PMProforma DROP COLUMN UsrRetainagePct
GO

--[IfExists(Column = PMProforma.UsrCuryAllocatedRetainedTotal)]
--[MinVersion(Hash = 0bd563528f24969b6337f11302f5a32b044a9484002d6b5639253d207fc18264)]
UPDATE PMProforma SET CuryAllocatedRetainedTotal = UsrCuryAllocatedRetainedTotal WHERE UsrCuryAllocatedRetainedTotal IS NOT NULL
ALTER TABLE PMProforma DROP COLUMN UsrCuryAllocatedRetainedTotal
GO

--[IfExists(Column = PMProforma.UsrAllocatedRetainedTotal)]
--[MinVersion(Hash = 4c7ca9920c4e907b1949427f1bfa27c1dd573ce4a9cf36618ea9b03fa17c4311)]
UPDATE PMProforma SET AllocatedRetainedTotal = UsrAllocatedRetainedTotal WHERE UsrAllocatedRetainedTotal IS NOT NULL
ALTER TABLE PMProforma DROP COLUMN UsrAllocatedRetainedTotal
GO

--[MinVersion(Hash = a559165b991ed5966739f7c4609937e847a17ab1480047090f2bdd8040cad886)]
IF (EXISTS(SELECT 1 FROM sys.columns WHERE object_id = object_id('Address') AND name = 'UsrCounty'))
BEGIN
EXEC sp_executesql N'
UPDATE Address
        SET County = UsrCounty';

EXEC sp_executesql N'ALTER TABLE Address DROP COLUMN UsrCounty';
END
GO

--[MinVersion(Hash = b0d622d519cae30e29770c5d0d83b93e26fc62c05c38908b72195b3e3f1935b5)]
IF (EXISTS(SELECT 1 FROM sys.columns WHERE object_id = object_id('PMQuoteTask') AND name = 'UsrType'))
BEGIN
EXEC sp_executesql N'
UPDATE PMQuoteTask
SET Type = 
	CASE UsrType
		WHEN ''Sub Job'' THEN ''Cost''   
		WHEN ''Schedule of Values'' THEN ''Rev'' 
		ELSE ''CostRev''
	END';

EXEC sp_executesql N'ALTER TABLE PMQuoteTask DROP COLUMN UsrType';
END
GO

--[MinVersion(Hash = 6081324804d25b464a3fc33d099725ff05aac0882c7817592be93a63fa60369e)]
IF (EXISTS(SELECT 1 FROM sys.columns WHERE object_id = object_id('PMTask') AND name = 'UsrType'))
BEGIN
EXEC sp_executesql N'
UPDATE PMTask
SET Type = 
	CASE UsrType
		WHEN ''Sub Job'' THEN ''Cost''   
		WHEN ''Schedule of Values'' THEN ''Rev'' 
		ELSE ''CostRev''
	END';

EXEC sp_executesql N'ALTER TABLE PMTask DROP COLUMN UsrType';
END
GO

--[MinVersion(Hash = cddc595d509048fbc5a3a8127b08e927224c756fc204e044accd314e2e44929c)]
UPDATE PMTask
SET Type = 'CostRev'
WHERE Type IS NULL
GO

--[mysql: Skip]
--[MinVersion(Hash = 2395a0bc15efe463c29dee0773742c19856fa5a69f16ab346850c404ad67b970)]
IF (EXISTS(SELECT 1 FROM sys.tables WHERE object_id = object_id('APInvoiceJCExt')))
BEGIN
EXEC sp_executesql N'
UPDATE i
SET 
	i.IsJointPayees = ie.IsJointPayees,
	i.TotalJointAmount = ie.TotalJointAmount,
	i.IsPaymentCycleWorkflow = ie.IsPaymentCycleWorkflow
FROM APInvoice i
INNER JOIN APInvoiceJCExt ie
ON
	i.CompanyID = ie.CompanyID
	AND i.DocType = ie.DocType
	AND i.RefNbr = ie.RefNbr';

EXEC sp_executesql N'DROP TABLE APInvoiceJCExt';
END
GO

--[mysql: Skip]
--[MinVersion(Hash = e49e22c4146ceaca1da7b4f93b94917d62079c9055340fd8de05c4057aa1c904)]
IF (EXISTS(SELECT 1 FROM sys.columns WHERE object_id = object_id('ComplianceDocument') AND name = 'JointVendorID'))
BEGIN
EXEC sp_executesql N'
UPDATE ComplianceDocument
        SET JointVendorInternalId = JointVendorID';

EXEC sp_executesql N'ALTER TABLE ComplianceDocument DROP COLUMN JointVendorID';
END
GO

--[mysql: Skip]
--[MinVersion(Hash = 6f338773c994aa244e7758fbd29063047d30305ddbd108372edd672393a8b0c9)]
IF (EXISTS(SELECT 1 FROM sys.tables WHERE object_id = object_id('ComplianceDocumentAttribute')))
BEGIN
EXEC sp_executesql N'
INSERT INTO [CSAnswers] ([CompanyID], [RefNoteID], [AttributeID], [Value], [NoteID])
SELECT cda.[CompanyID], cda.[ComplianceDocumentNoteID], cda.[AttributeID], cda.[Value], NEWID()
FROM dbo.ComplianceDocumentAttribute cda
WHERE NOT EXISTS
(
    SELECT *
    FROM [CSAnswers] AS ans
    WHERE ans.[CompanyID] = cda.[CompanyID]
        AND ans.[RefNoteID] = cda.[ComplianceDocumentNoteID]
        AND ans.[AttributeID] = cda.[AttributeID]
)';

EXEC sp_executesql N'DROP TABLE ComplianceDocumentAttribute';
END
GO

--[mysql: Skip]
--[OldHash(Hash = 5ba2e8fd2ac104707a14aabd62cae1ea14f71df7c6f754b5733952aa19eb3fb9)]
--[MinVersion(Hash = 9e8209f6b70396ed96862e49c2d8de5639c1045ee043b23e53a1552229a38daf)]
IF (EXISTS(SELECT 1 FROM sys.tables WHERE object_id = object_id('ComplianceAttributeGroup')))
BEGIN
EXEC sp_executesql N'
INSERT INTO [CSAttributeGroup] (
    [CompanyID], [AttributeID], [EntityClassID], [EntityType], [SortOrder], [CreatedByID],
    [CreatedByScreenID], [CreatedDateTime], [LastModifiedByID], [LastModifiedByScreenID],
    [LastModifiedDateTime], [DefaultValue], [Required], [IsActive], [AttributeCategory])
SELECT cag.[CompanyID], cag.[AttributeID], ''COMPLIANCE'', ''CcsLego.Compliance.CL.DAC.ComplianceDocument'',
    cag.[SortOrder], cag.[CreatedByID], cag.[CreatedByScreenID], cag.[CreatedDateTime], cag.[LastModifiedByID],
    cag.[LastModifiedByScreenID], cag.[LastModifiedDateTime], cag.[DefaultValue], cag.[Required], 1,''A''
FROM ComplianceAttributeGroup cag
WHERE NOT EXISTS
(
    SELECT *
    FROM [CSAttributeGroup] AS csag
    WHERE csag.[CompanyID] = cag.[CompanyID]
        AND csag.[EntityType] = ''CcsLego.Compliance.CL.DAC.ComplianceDocument''
        AND csag.[AttributeID] = cag.[AttributeID]
)';

EXEC sp_executesql N'DROP TABLE ComplianceAttributeGroup';
END
GO

--[mysql: Skip]
--[MinVersion(Hash = b1209d56239931c64743b5e2b17d8238089048c0784a3f1f53dc5d16ad2ecb1f)]
IF (EXISTS(SELECT 1 FROM sys.columns WHERE object_id = object_id('JointPayeePayment') AND name = 'InvoiceRefNbr') AND
	EXISTS(SELECT 1 FROM sys.columns WHERE object_id = object_id('JointPayeePayment') AND name = 'InvoiceDocType'))
BEGIN
EXEC sp_executesql N'
UPDATE JointPayeePayment
        SET InvoiceDocType = reg.DocType, InvoiceRefNbr = reg.RefNbr
        FROM JointPayee jp
            INNER JOIN APRegister reg
                ON jp.BillID = reg.NoteID AND jp.CompanyID = reg.CompanyID
        WHERE jp.JointPayeeID = JointPayeePayment.JointPayeeID
            AND InvoiceDocType IS NULL
            AND InvoiceRefNbr IS NULL';

END
GO

--[MinVersion(Hash = 53e0df6c98feaccadc6d95c0949954ac8b489fdeca32385cb27b8a8934b35416)]
UPDATE ComplianceDocument
SET SourceType = 'AP Bill'
WHERE SourceType = 'AP Inv';
GO

--[MinVersion(Hash = f606165b80ab8a0010825e27670beca6e428b992967767c4a5255b114ec1dc21)]
UPDATE ComplianceDocument
SET LinkToPayment = 0
WHERE LinkToPayment IS NULL;
GO

--[MinVersion(Hash = bd2263efa397c14bfb7cdfe208b36d15bf454e72f5bd3319ddd8ad338f85d8ea)]
UPDATE Contract
SET ThroughDateSourceConditional = 'Posting Period End Date',
    ThroughDateSourceUnconditional = 'AP Check Date'
WHERE BaseType IN ('P', 'R')
    AND NonProject = 0
    AND ThroughDateSourceConditional IS NULL
    AND ThroughDateSourceUnconditional IS NULL;

UPDATE JointPayee
SET BillLineNumber = 0
WHERE BillLineNumber IS NULL;

UPDATE JointPayeePayment SET AdjustmentNumber = 0
WHERE AdjustmentNumber IS NULL;

UPDATE ComplianceAttribute
SET "Value" =
    CASE
        WHEN "Value" = 'Conditional-Partial' THEN 'Conditional Partial'
        WHEN "Value" = 'Conditional-Final' THEN 'Conditional Final'
        WHEN "Value" = 'Unconditional-Partial' THEN 'Unconditional Partial'
        WHEN "Value" = 'Unconditional-Final' THEN 'Unconditional Final'
    END
WHERE "Value" IN ('Conditional-Partial', 'Conditional-Final', 'Unconditional-Partial', 'Unconditional-Final');

DELETE FROM CSAttributeGroup
WHERE CSAttributeGroup.EntityClassID = 'COMPLIANCE'
    AND AttributeID IN
        (SELECT * FROM (SELECT CSAttributeGroup.AttributeID FROM CSAttributeGroup
            LEFT JOIN CSAttribute ON CSAttributeGroup.AttributeID = CSAttribute.AttributeID
                AND CSAttribute.CompanyID = CSAttributeGroup.CompanyID
            WHERE CSAttributeGroup.EntityClassID = 'COMPLIANCE'
                AND CSAttribute.AttributeID IS NULL) AS attributeList);

DELETE FROM FilterHeader
    WHERE FilterID = 5135
        AND ScreenID = 'SELECTOR'
        AND FilterName = 'Items';

DELETE FROM FilterHeader
    WHERE FilterID = 5134
        AND ScreenID = 'SELECTOR'
        AND FilterName = 'Account Groups';

DELETE FROM JointPayeePayment
WHERE PaymentDocType = 'INV';

--[mysql: Skip]
IF NOT EXISTS(SELECT 1 FROM dbo.ComplianceDocumentReference)
BEGIN
    --ARPayment
    INSERT INTO dbo.ComplianceDocumentReference (
        [ComplianceDocumentReferenceId], [Type], [ReferenceNumber], [RefNoteId],
        [CompanyId], [CreatedByID], [CreatedByScreenID], [CreatedDateTime],
        [LastModifiedByID], [LastModifiedByScreenID], [LastModifiedDateTime])
    SELECT NEWID(), a.DocType, a.RefNbr, a.NoteID,
        a.CompanyID, cd.CreatedByID, cd.CreatedByScreenID, cd.CreatedDateTime,
        cd.LastModifiedByID, cd.LastModifiedByScreenID, cd.LastModifiedDateTime
    FROM dbo.ARRegister AS a
        INNER JOIN dbo.ComplianceDocument AS cd ON cd.ArPaymentID = a.NoteID;
    UPDATE cd
    SET cd.ArPaymentID = cdr.ComplianceDocumentReferenceId
    FROM dbo.ComplianceDocumentReference cdr
        INNER JOIN dbo.ComplianceDocument cd ON cd.ArPaymentID = cdr.RefNoteId;

    --ARInvoice
    INSERT INTO dbo.ComplianceDocumentReference (
        [ComplianceDocumentReferenceId], [Type], [ReferenceNumber], [RefNoteId],
        [CompanyId], [CreatedByID], [CreatedByScreenID], [CreatedDateTime],
        [LastModifiedByID], [LastModifiedByScreenID], [LastModifiedDateTime])
    SELECT NEWID(), a.DocType, a.RefNbr, a.NoteID,
        a.CompanyID, cd.CreatedByID, cd.CreatedByScreenID, cd.CreatedDateTime,
        cd.LastModifiedByID, cd.LastModifiedByScreenID, cd.LastModifiedDateTime
    FROM dbo.ARRegister AS a
        INNER JOIN dbo.ComplianceDocument AS cd ON cd.InvoiceID = a.NoteID;
    UPDATE cd
    SET cd.InvoiceID = cdr.ComplianceDocumentReferenceId
    FROM dbo.ComplianceDocumentReference cdr
        INNER JOIN dbo.ComplianceDocument cd ON cd.InvoiceID = cdr.RefNoteId;

    --APInvoice
    INSERT INTO dbo.ComplianceDocumentReference (
        [ComplianceDocumentReferenceId], [Type], [ReferenceNumber], [RefNoteId],
        [CompanyId], [CreatedByID], [CreatedByScreenID], [CreatedDateTime],
        [LastModifiedByID], [LastModifiedByScreenID], [LastModifiedDateTime])
    SELECT NEWID(), a.DocType, a.RefNbr, a.NoteID,
        a.CompanyID, cd.CreatedByID, cd.CreatedByScreenID, cd.CreatedDateTime,
        cd.LastModifiedByID, cd.LastModifiedByScreenID, cd.LastModifiedDateTime
    FROM dbo.APRegister AS a
        INNER JOIN dbo.ComplianceDocument AS cd ON cd.BillID = a.NoteID;
    UPDATE cd
    SET cd.BillID = cdr.ComplianceDocumentReferenceId
    FROM dbo.ComplianceDocumentReference cdr
        INNER JOIN dbo.ComplianceDocument cd ON cd.BillID = cdr.RefNoteId;

    --APPayment
    INSERT INTO dbo.ComplianceDocumentReference (
        [ComplianceDocumentReferenceId], [Type], [ReferenceNumber], [RefNoteId],
        [CompanyId], [CreatedByID], [CreatedByScreenID], [CreatedDateTime],
        [LastModifiedByID], [LastModifiedByScreenID], [LastModifiedDateTime])
    SELECT NEWID(), a.DocType, a.RefNbr, a.NoteID,
        a.CompanyID, cd.CreatedByID, cd.CreatedByScreenID, cd.CreatedDateTime,
        cd.LastModifiedByID, cd.LastModifiedByScreenID, cd.LastModifiedDateTime
    FROM dbo.APRegister AS a
        INNER JOIN dbo.ComplianceDocument AS cd ON cd.ApCheckID = a.NoteID;
    UPDATE cd
    SET cd.ApCheckID = cdr.ComplianceDocumentReferenceId
    FROM dbo.ComplianceDocumentReference cdr
        INNER JOIN dbo.ComplianceDocument cd ON cd.ApCheckID = cdr.RefNoteId;

    --Purchase Order
    INSERT INTO dbo.ComplianceDocumentReference (
        [ComplianceDocumentReferenceId], [Type], [ReferenceNumber], [RefNoteId],
        [CompanyId], [CreatedByID], [CreatedByScreenID], [CreatedDateTime],
        [LastModifiedByID], [LastModifiedByScreenID], [LastModifiedDateTime])
    SELECT NEWID(), p.OrderType, p.OrderNbr, p.NoteID,
        p.CompanyID, cd.CreatedByID, cd.CreatedByScreenID, cd.CreatedDateTime,
        cd.LastModifiedByID, cd.LastModifiedByScreenID, cd.LastModifiedDateTime
    FROM dbo.POOrder AS p
        INNER JOIN dbo.ComplianceDocument AS cd ON cd.PurchaseOrder = p.NoteID;
    UPDATE cd
    SET cd.PurchaseOrder = cdr.ComplianceDocumentReferenceId
    FROM dbo.ComplianceDocumentReference cdr
        INNER JOIN dbo.ComplianceDocument cd ON cd.PurchaseOrder = cdr.RefNoteId;

    --ProjectTransaction
    INSERT INTO dbo.ComplianceDocumentReference (
        [ComplianceDocumentReferenceId], [Type], [ReferenceNumber], [RefNoteId],
        [CompanyId], [CreatedByID], [CreatedByScreenID], [CreatedDateTime],
        [LastModifiedByID], [LastModifiedByScreenID], [LastModifiedDateTime])
    SELECT NEWID(), p.Module, p.RefNbr, p.NoteID,
        p.CompanyID, cd.CreatedByID, cd.CreatedByScreenID, cd.CreatedDateTime,
        cd.LastModifiedByID, cd.LastModifiedByScreenID, cd.LastModifiedDateTime
    FROM dbo.PMRegister AS p
        INNER JOIN dbo.ComplianceDocument AS cd ON cd.ProjectTransactionID = p.NoteID;
    UPDATE cd
    SET cd.ProjectTransactionID = cdr.ComplianceDocumentReferenceId
    FROM dbo.ComplianceDocumentReference cdr
        INNER JOIN dbo.ComplianceDocument cd ON cd.ProjectTransactionID = cdr.RefNoteId;
END;
GO

--[IfExists(Column = ComplianceDocument.ScheduleOfValues)]
--[MinVersion(Hash = 9867ee71480b1d2091f159eef1234d61dcd5698fc61edb3bf44b716124807ef0)]
UPDATE ComplianceDocument
SET RevenueTaskID = ScheduleOfValues
	, CostTaskID = SubJob
GO

--end Joint check & Compliance migration
--[MinVersion(Hash = 4034287911b9752353d27a074b9aeab97d092a15d1ee01bc15006b3b68cecbce)]

GO

--[MinVersion(Hash = c65455146b060b527494ec57ddc783e30eb9cc26e94cdf3b1b05b8f1458e8836)]
UPDATE "PMTask"
SET "Type" = 'CostRev'
WHERE "Type" IS NULL
GO

--[MinVersion(Hash = 743b76f308585658ccdd704f43d2835097ac8cf5df1c5489adf12f0acdde22d9)]
UPDATE "PMQuoteTask"
SET "Type" = 'CostRev'
WHERE "Type" IS NULL
GO

--Fixing CompanyMask when Change Requests moved from Construction customization into product:
--[mysql: Skip]
--[MinVersion(Hash = f113b07f002b87a2e30f234e905c87ffa204b028a3d51bbf532a4a6028a9c568)]
declare @newCompanyMask varbinary(10)
declare @maxID int
declare @charnbr int

select @maxID = max(CompanyID) from dbo.Company
set @charnbr = ((@maxID - 1) / 4) * 2 + 2
set @newCompanyMask = CONVERT(varbinary(max), '0x' + replicate('A', @charnbr), 1);

UPDATE a SET CompanyMask = @newCompanyMask  
FROM MUIScreen a
INNER JOIN SiteMap m ON a.CompanyID = m.CompanyID AND a.NodeID = m.NodeID
WHERE m.ScreenID IN ('PM308500', 'PM3085PL')

UPDATE a SET CompanyMask = @newCompanyMask  
FROM MUIPinnedScreen a
INNER JOIN SiteMap m ON a.CompanyID = m.CompanyID AND a.NodeID = m.NodeID
WHERE m.ScreenID IN ('PM308500', 'PM3085PL')

UPDATE a SET CompanyMask = @newCompanyMask  
FROM MUIFavoriteScreen a
INNER JOIN SiteMap m ON a.CompanyID = m.CompanyID AND a.NodeID = m.NodeID
WHERE m.ScreenID IN ('PM308500', 'PM3085PL')

UPDATE a SET CompanyMask = @newCompanyMask  
FROM GINavigationScreen a
INNER JOIN GIDesign g ON a.CompanyID = g.CompanyID AND a.DesignID = g.DesignID
WHERE a.CompanyID = 1 AND PrimaryScreenIDNew = 'PM308500'

UPDATE a SET CompanyMask = @newCompanyMask  
FROM GINavigationParameter a
INNER JOIN GIDesign g ON a.CompanyID = g.CompanyID AND a.DesignID = g.DesignID
WHERE a.CompanyID = 1 AND PrimaryScreenIDNew = 'PM308500'

UPDATE a SET CompanyMask = @newCompanyMask  
FROM GISort a
INNER JOIN GIDesign g ON a.CompanyID = g.CompanyID AND a.DesignID = g.DesignID
WHERE a.CompanyID = 1 AND PrimaryScreenIDNew = 'PM308500'

UPDATE a SET CompanyMask = @newCompanyMask  
FROM GITable a
INNER JOIN GIDesign g ON a.CompanyID = g.CompanyID AND a.DesignID = g.DesignID
WHERE a.CompanyID = 1 AND PrimaryScreenIDNew = 'PM308500'

UPDATE a SET CompanyMask = @newCompanyMask  
FROM GIResult a
INNER JOIN GIDesign g ON a.CompanyID = g.CompanyID AND a.DesignID = g.DesignID
WHERE a.CompanyID = 1 AND PrimaryScreenIDNew = 'PM308500'

UPDATE a SET CompanyMask = @newCompanyMask  
FROM GIRelation a
INNER JOIN GIDesign g ON a.CompanyID = g.CompanyID AND a.DesignID = g.DesignID
WHERE a.CompanyID = 1 AND PrimaryScreenIDNew = 'PM308500'

UPDATE a SET CompanyMask = @newCompanyMask  
FROM GIOn a
INNER JOIN GIDesign g ON a.CompanyID = g.CompanyID AND a.DesignID = g.DesignID
WHERE a.CompanyID = 1 AND PrimaryScreenIDNew = 'PM308500'

UPDATE a SET CompanyMask = @newCompanyMask  
FROM GIDesign a
WHERE a.CompanyID = 1 AND PrimaryScreenIDNew = 'PM308500'

UPDATE a SET CompanyMask = @newCompanyMask  
FROM FilterRow a
INNER JOIN FilterHeader h ON a.CompanyID = h.CompanyID AND a.FilterID = h.FilterID
WHERE a.CompanyID = 1 AND ScreenID = 'PM308500'

UPDATE a SET CompanyMask = @newCompanyMask 
FROM FilterRow a
INNER JOIN Company c ON a.CompanyID = c.CompanyID AND c.IsReadOnly = 0
INNER JOIN FilterHeader h ON a.CompanyID = h.CompanyID AND a.FilterID = h.FilterID
WHERE ScreenID = 'PM3085PL'

UPDATE a SET CompanyMask = @newCompanyMask 
FROM FilterHeader a
WHERE a.CompanyID = 1 AND ScreenID = 'PM308500'

UPDATE m SET CompanyMask = @newCompanyMask 
FROM MobileSiteMap m
WHERE m.CompanyID = 1 AND m.ScreenID IN ('PM308500','PM3085PL')

UPDATE m SET CompanyMask = @newCompanyMask 
FROM sitemap m
WHERE m.CompanyID = 1 AND m.ScreenID IN ('PM308500','PM3085PL')
GO

--UpdateNotificationSourceByNewWaiverTypes
--[MinVersion(Hash = 66618091aa146ac056cc17361c9ae51d669e9ade875cdec8843aa6b2f6f4ee5e)]
UPDATE source
SET source.SetupID = setup_new.SetupID
FROM NotificationSource source
inner join NotificationSetup setup_old 
	ON source.CompanyID = setup_old.CompanyID
		AND source.SetupID = setup_old.SetupID
		AND setup_old.NotificationCD = 'Unconditional-Partial'
INNER JOIN NotificationSetup setup_new
	ON source.CompanyID = setup_new.CompanyID
		AND setup_new.NotificationCD = 'Unconditional Partial'

UPDATE source
SET source.SetupID = setup_new.SetupID
FROM NotificationSource source
inner join NotificationSetup setup_old 
	ON source.CompanyID = setup_old.CompanyID
		AND source.SetupID = setup_old.SetupID
		AND setup_old.NotificationCD = 'Unconditional-Final'
INNER JOIN NotificationSetup setup_new
	ON source.CompanyID = setup_new.CompanyID
		AND setup_new.NotificationCD = 'Unconditional Final'

UPDATE source
SET source.SetupID = setup_new.SetupID
FROM NotificationSource source
inner join NotificationSetup setup_old 
	ON source.CompanyID = setup_old.CompanyID
		AND source.SetupID = setup_old.SetupID
		AND setup_old.NotificationCD = 'Conditional-Partial'
INNER JOIN NotificationSetup setup_new
	ON source.CompanyID = setup_new.CompanyID
		AND setup_new.NotificationCD = 'Conditional Partial'

UPDATE source
SET source.SetupID = setup_new.SetupID
FROM NotificationSource source
inner join NotificationSetup setup_old 
	ON source.CompanyID = setup_old.CompanyID
		AND source.SetupID = setup_old.SetupID
		AND setup_old.NotificationCD = 'Conditional-Final'
INNER JOIN NotificationSetup setup_new
	ON source.CompanyID = setup_new.CompanyID
		AND setup_new.NotificationCD = 'Conditional Final'
GO

--[MinVersion(Hash = 0ab7e6436848540c84f54259f311550121225d186a8cd71133a9c665d7c7e924)]
IF (EXISTS(SELECT 1 FROM sys.columns WHERE object_id = object_id('Contract') AND name = 'UsrAllowNonProjectAccountGroups'))
BEGIN
EXEC sp_executesql N'UPDATE Contract SET AllowNonProjectAccountGroups = UsrAllowNonProjectAccountGroups';

EXEC sp_executesql N'ALTER TABLE Contract DROP COLUMN UsrAllowNonProjectAccountGroups';
END
GO

--[mysql: Skip]
--[MinVersion(Hash = fa4875847ea6cb9013b9a44e38f84a0852c263ea08e36c0d136a2af632f14350)]
IF(EXISTS(SELECT TOP 1 * FROM ComplianceAttribute t
						WHERE t.CompanyAttributeID <> t.AttributeID OR t.CompanyAttributeID IS NULL))
BEGIN
EXEC sp_executesql N'UPDATE ComplianceAttribute
SET CompanyAttributeID = AttributeID
WHERE CompanyAttributeID <> AttributeID OR CompanyAttributeID IS NULL';
END
GO

--[mysql: Skip]
--[MinVersion(Hash = f4c0937b0ad3001f348fa2e9aae35e781bfdffc170514b51585e627c5c2ce7de)]
IF (EXISTS(SELECT TOP 1 * FROM ComplianceAttribute attr 
			WHERE attr.CompanyID <> 1 
				AND attr.Value in ('Conditional Partial', 'Conditional Final', 'Unconditional Partial', 'Unconditional Final'))
		OR EXISTS(SELECT TOP 1 * FROM ComplianceAttributeType attrType
						WHERE attrType.CompanyID <> 1
							AND attrType.Type in ('Certificate', 'Status', 'Other', 'Notice', 'Lien Waiver', 'Insurance')))
BEGIN

UPDATE attr
SET attr.Type =
    CASE
        WHEN attrType.Type = 'Certificate' THEN 1
        WHEN attrType.Type = 'Status' THEN 2
        WHEN attrType.Type = 'Other' THEN 3
        WHEN attrType.Type = 'Notice' THEN 4
        WHEN attrType.Type = 'Lien Waiver' THEN 5
        WHEN attrType.Type = 'Insurance' THEN 6
	END
FROM ComplianceAttribute attr
	INNER JOIN ComplianceAttributeType attrType
		ON attr.Type = attrType.ComplianceAttributeTypeID
			AND attr.CompanyID = attrType.CompanyID
WHERE attr.CompanyID <> 1 

UPDATE doc
SET doc.DocumentType =
    CASE
        WHEN attrType.Type = 'Certificate' THEN 1
        WHEN attrType.Type = 'Status' THEN 2
        WHEN attrType.Type = 'Other' THEN 3
        WHEN attrType.Type = 'Notice' THEN 4
        WHEN attrType.Type = 'Lien Waiver' THEN 5
        WHEN attrType.Type = 'Insurance' THEN 6
    END
FROM ComplianceDocument doc
	INNER JOIN ComplianceAttributeType attrType
		ON doc.DocumentType = attrType.ComplianceAttributeTypeID
			AND doc.CompanyID = attrType.CompanyID
			AND attrType.Type in ('Certificate', 'Status', 'Other', 'Notice', 'Lien Waiver', 'Insurance')
WHERE doc.CompanyID <> 1 

UPDATE doc
SET doc.DocumentTypeValue = 
    CASE
        WHEN attr.Value = 'Conditional Partial' THEN 1
        WHEN attr.Value = 'Conditional Final' THEN 2
        WHEN attr.Value = 'Unconditional Partial' THEN 3
        WHEN attr.Value = 'Unconditional Final' THEN 4
    END
FROM ComplianceDocument doc
	INNER JOIN ComplianceAttribute attr
		ON doc.DocumentTypeValue = attr.AttributeID
			AND doc.CompanyID = attr.CompanyID
			AND attr.Value in ('Conditional Partial', 'Conditional Final', 'Unconditional Partial', 'Unconditional Final')

DELETE attr
FROM ComplianceAttribute attr
WHERE attr.CompanyID <> 1 
	AND attr.Value in ('Conditional Partial', 'Conditional Final', 'Unconditional Partial', 'Unconditional Final')

DELETE attrType
FROM ComplianceAttributeType attrType
WHERE attrType.CompanyID <> 1 
	AND attrType.Type in ('Certificate', 'Status', 'Other', 'Notice', 'Lien Waiver', 'Insurance')

END
GO

--[mysql: Skip]
--[MinVersion(Hash = 12206bda306406666f93f141890dc5e593eadb6fc37ff28d77722fa51536346b)]
BEGIN
	DECLARE @TableCount TABLE( TempCount INT );
	DECLARE @TempCount INT;

	IF (EXISTS(SELECT 1 FROM sys.columns WHERE 
		object_id = object_id('CROpportunity') AND 
		"name" = 'MultipleAccounts'))
	BEGIN
		INSERT INTO @TableCount
		EXEC sp_executesql N'SELECT COUNT(*) FROM CROpportunity WHERE MultipleAccounts IS NOT NULL';
	END

	IF (EXISTS(SELECT 1 FROM sys.columns WHERE 
		object_id = object_id('CROpportunity') AND 
		"name" = 'Cost'))
	BEGIN
		INSERT INTO @TableCount
		EXEC sp_executesql N'SELECT COUNT(*) FROM CROpportunity WHERE Cost IS NOT NULL';
	END

	IF (EXISTS(SELECT 1 FROM sys.columns WHERE 
		object_id = object_id('CROpportunity') AND 
		"name" = 'TotalAmount'))
	BEGIN
		INSERT INTO @TableCount
		EXEC sp_executesql N'SELECT COUNT(*) FROM CROpportunity WHERE TotalAmount IS NOT NULL';
	END

	IF (EXISTS(SELECT 1 FROM sys.columns WHERE 
		object_id = object_id('CROpportunity') AND 
		"name" = 'QuotedAmount'))
	BEGIN
		INSERT INTO @TableCount
		EXEC sp_executesql N'SELECT COUNT(*) FROM CROpportunity WHERE QuotedAmount IS NOT NULL';
	END

	IF (EXISTS(SELECT 1 FROM sys.tables WHERE 
		object_id = object_id('RequestForInformation')))
	BEGIN
		INSERT INTO @TableCount VALUES ( 1 );
	END

	SELECT @TempCount = COALESCE(SUM(TempCount), 0) FROM @TableCount;

	--------------------------------------------------------------------------------

	IF (@TempCount > 0 AND NOT EXISTS(SELECT 1 FROM sys.columns WHERE 
		object_id = object_id('CROpportunity') AND 
		"name" = 'UsrMultipleAccounts'))
	BEGIN
	EXEC sp_executesql N'ALTER TABLE CROpportunity ADD UsrMultipleAccounts BIT NULL';
	END

	IF (@TempCount > 0 AND EXISTS(SELECT 1 FROM sys.columns WHERE 
		object_id = object_id('CROpportunity') AND 
		"name" = 'MultipleAccounts'))
	BEGIN
	EXEC sp_executesql N'
	UPDATE CROpportunity
	        SET UsrMultipleAccounts = MultipleAccounts
		WHERE 
			UsrMultipleAccounts IS NULL AND
			MultipleAccounts IS NOT NULL'; 
	END

	--------------------------------------------------------------------------------

	IF (@TempCount > 0 AND NOT EXISTS(SELECT 1 FROM sys.columns WHERE 
		object_id = object_id('CROpportunity') AND 
		"name" = 'UsrCost'))
	BEGIN
	EXEC sp_executesql N'ALTER TABLE CROpportunity ADD UsrCost DECIMAL(25,2) NULL';
	END

	IF (@TempCount > 0 AND EXISTS(SELECT 1 FROM sys.columns WHERE 
		object_id = object_id('CROpportunity') AND 
		"name" = 'Cost'))
	BEGIN
	EXEC sp_executesql N'
	UPDATE CROpportunity
	        SET UsrCost = Cost
		WHERE 
			UsrCost IS NULL AND
			Cost IS NOT NULL';
	END

	--------------------------------------------------------------------------------

	IF (@TempCount > 0 AND NOT EXISTS(SELECT 1 FROM sys.columns WHERE 
		object_id = object_id('CROpportunity') AND 
		"name" = 'UsrQuotedAmount'))
	BEGIN
	EXEC sp_executesql N'ALTER TABLE CROpportunity ADD UsrQuotedAmount DECIMAL(25,2) NULL';
	END

	IF (@TempCount > 0 AND EXISTS(SELECT 1 FROM sys.columns WHERE 
		object_id = object_id('CROpportunity') AND 
		"name" = 'QuotedAmount'))
	BEGIN
	EXEC sp_executesql N'
	UPDATE CROpportunity
	        SET UsrQuotedAmount = QuotedAmount
		WHERE 
			UsrQuotedAmount IS NULL AND
			QuotedAmount IS NOT NULL';
	END

	--------------------------------------------------------------------------------

	IF (@TempCount > 0 AND NOT EXISTS(SELECT 1 FROM sys.columns WHERE 
		object_id = object_id('CROpportunity') AND 
		"name" = 'UsrTotalAmount'))
	BEGIN
	EXEC sp_executesql N'ALTER TABLE CROpportunity ADD UsrTotalAmount DECIMAL(25,2) NULL';
	END

	IF (@TempCount > 0 AND EXISTS(SELECT 1 FROM sys.columns WHERE 
		object_id = object_id('CROpportunity') AND 
		"name" = 'TotalAmount'))
	BEGIN
	EXEC sp_executesql N'
	UPDATE CROpportunity
	        SET UsrTotalAmount = TotalAmount
		WHERE 
			UsrTotalAmount IS NULL AND
			TotalAmount IS NOT NULL';
	END
END
GO

--[mysql: Skip]
--[MinVersion(Hash = 9fc956caed8d51ec640233866331fbea96e2f7ba43d5d131d5339362ad652cbc)]
DECLARE @createdBySysAdmin uniqueidentifier;

SET @createdBySysAdmin = 'B5344897-037E-4D58-B5C3-1BDFD0F47BF9';

IF(NOT EXISTS(SELECT TOP 1 * FROM NotificationSetup WHERE SetupID = 'af1d29f2-53cd-4400-b22a-41151f8c399d'))
BEGIN
INSERT INTO NotificationSetup
           (CompanyID
           ,SetupID
           ,Module
           ,SourceCD
           ,NotificationCD
           ,ReportID
           ,Format
           ,Active
           ,CreatedByID
           ,CreatedDateTime
           ,LastModifiedByID
           ,LastModifiedByScreenID
           ,LastModifiedDateTime
           ,CreatedByScreenID		   )
     VALUES
           (1
           ,'af1d29f2-53cd-4400-b22a-41151f8c399d'
           ,'CL'
           ,'Vendor'
           ,'Conditional Partial'
           ,'CL642001'
           ,'P'
           ,1
           ,@createdBySysAdmin
           ,SYSDATETIME()
           ,@createdBySysAdmin
           ,'00000000'
           ,SYSDATETIME()
           ,'00000000')
END

IF(NOT EXISTS(SELECT TOP 1 * FROM NotificationSetup WHERE SetupID = 'baba9b32-3e14-4e31-b78a-6efdd8245a71'))
BEGIN
INSERT INTO NotificationSetup
           (CompanyID
           ,SetupID
           ,Module
           ,SourceCD
           ,NotificationCD
           ,ReportID
           ,Format
           ,Active
           ,CreatedByID
           ,CreatedDateTime
           ,LastModifiedByID
           ,LastModifiedByScreenID
           ,LastModifiedDateTime
           ,CreatedByScreenID		   )
     VALUES
           (1
           ,'baba9b32-3e14-4e31-b78a-6efdd8245a71'
           ,'CL'
           ,'Vendor'
           ,'Unconditional Final'
           ,'CL642004'
           ,'P'
           ,1
           ,@createdBySysAdmin
           ,SYSDATETIME()
           ,@createdBySysAdmin
           ,'00000000'
           ,SYSDATETIME()
           ,'00000000')
END

IF(NOT EXISTS(SELECT TOP 1 * FROM NotificationSetup WHERE SetupID = '3a99fb5f-e965-4f03-9769-731641f79d31'))
BEGIN
INSERT INTO NotificationSetup
           (CompanyID
           ,SetupID
           ,Module
           ,SourceCD
           ,NotificationCD
           ,ReportID
           ,Format
           ,Active
           ,CreatedByID
           ,CreatedDateTime
           ,LastModifiedByID
           ,LastModifiedByScreenID
           ,LastModifiedDateTime
           ,CreatedByScreenID		   )
     VALUES
           (1
           ,'3a99fb5f-e965-4f03-9769-731641f79d31'
           ,'CL'
           ,'Vendor'
           ,'Conditional Final'
           ,'CL642003'
           ,'P'
           ,1
           ,@createdBySysAdmin
           ,SYSDATETIME()
           ,@createdBySysAdmin
           ,'00000000'
           ,SYSDATETIME()
           ,'00000000')
END

IF(NOT EXISTS(SELECT TOP 1 * FROM NotificationSetup WHERE SetupID = 'F7A67E58-5A98-41DC-B3AE-BDA89976D803'))
BEGIN
	 INSERT INTO NotificationSetup
           (CompanyID
           ,SetupID
           ,Module
           ,SourceCD
           ,NotificationCD
           ,ReportID
           ,Format
           ,Active
           ,CreatedByID
           ,CreatedDateTime
           ,LastModifiedByID
           ,LastModifiedByScreenID
           ,LastModifiedDateTime
           ,CreatedByScreenID		   )
     VALUES
           (1
           ,'F7A67E58-5A98-41DC-B3AE-BDA89976D803'
           ,'CL'
           ,'Vendor'
           ,'Unconditional Partial'
           ,'CL642002'
           ,'P'
           ,1
           ,@createdBySysAdmin
           ,SYSDATETIME()
           ,@createdBySysAdmin
           ,'00000000'
           ,SYSDATETIME()
           ,'00000000')
END

IF(NOT EXISTS(SELECT TOP 1 * FROM NotificationSetup WHERE SetupID = '86c3538c-f196-4ec0-a60b-38e7a4bf2fe0'))
BEGIN
	 INSERT INTO NotificationSetup
           (CompanyID
           ,SetupID
           ,Module
           ,SourceCD
           ,NotificationCD
           ,ReportID
           ,Format
           ,Active
           ,CreatedByID
           ,CreatedDateTime
           ,LastModifiedByID
           ,LastModifiedByScreenID
           ,LastModifiedDateTime
           ,CreatedByScreenID		   )
     VALUES
           (1
           ,'86c3538c-f196-4ec0-a60b-38e7a4bf2fe0'
           ,'SC'
           ,'Vendor'
           ,'SUBCONTRACT'
           ,'SC641000'
           ,'P'
           ,1
           ,@createdBySysAdmin
           ,SYSDATETIME()
           ,@createdBySysAdmin
           ,'00000000'
           ,SYSDATETIME()
           ,'00000000')
END

UPDATE nsrec
SET nsrec.SetupID = nsSys.SetupID
FROM NotificationSetupRecipient nsrec
INNER JOIN NotificationSetup nsUser
	ON nsrec.CompanyID = nsUser.CompanyID
		AND nsrec.SetupID = nsUser.SetupID
INNER JOIN NotificationSetup nsSys
	ON (nsUser.NBranchID = nsSys.NBranchID OR (nsUser.NBranchID IS NULL AND nsSys.NBranchID IS NULL))
		AND nsUser.SourceCD = nsSys.SourceCD
		AND nsUser.NotificationCD = nsSys.NotificationCD
		AND nsUser.Module = nsSys.Module
		AND nsUser.SetupID <> nsSys.SetupID
		AND nsSys.CompanyID = 1
WHERE nsrec.CompanyID <> 1
AND nsSys.SetupID IN 
('af1d29f2-53cd-4400-b22a-41151f8c399d'
,'baba9b32-3e14-4e31-b78a-6efdd8245a71'
,'3a99fb5f-e965-4f03-9769-731641f79d31'
,'F7A67E58-5A98-41DC-B3AE-BDA89976D803'
,'86c3538c-f196-4ec0-a60b-38e7a4bf2fe0')

UPDATE nsOver
SET nsOver.SetupID = nsSys.SetupID
FROM NotificationSetupUserOverride nsOver
INNER JOIN NotificationSetup nsUser
	ON nsOver.CompanyID = nsUser.CompanyID
		AND nsOver.SetupID = nsUser.SetupID
INNER JOIN NotificationSetup nsSys
	ON (nsUser.NBranchID = nsSys.NBranchID OR (nsUser.NBranchID IS NULL AND nsSys.NBranchID IS NULL))
		AND nsUser.SourceCD = nsSys.SourceCD
		AND nsUser.NotificationCD = nsSys.NotificationCD
		AND nsUser.Module = nsSys.Module
		AND nsUser.SetupID <> nsSys.SetupID
		AND nsSys.CompanyID = 1
WHERE nsOver.CompanyID <> 1
AND nsSys.SetupID IN 
('af1d29f2-53cd-4400-b22a-41151f8c399d'
,'baba9b32-3e14-4e31-b78a-6efdd8245a71'
,'3a99fb5f-e965-4f03-9769-731641f79d31'
,'F7A67E58-5A98-41DC-B3AE-BDA89976D803'
,'86c3538c-f196-4ec0-a60b-38e7a4bf2fe0')

UPDATE nSource
SET nSource.SetupID = nsSys.SetupID
FROM NotificationSource nSource
INNER JOIN NotificationSetup nsUser
	ON nSource.CompanyID = nsUser.CompanyID
		AND nSource.SetupID = nsUser.SetupID
INNER JOIN NotificationSetup nsSys
	ON (nsUser.NBranchID = nsSys.NBranchID OR (nsUser.NBranchID IS NULL AND nsSys.NBranchID IS NULL))
		AND nsUser.SourceCD = nsSys.SourceCD
		AND nsUser.NotificationCD = nsSys.NotificationCD
		AND nsUser.Module = nsSys.Module
		AND nsUser.SetupID <> nsSys.SetupID
		AND nsSys.CompanyID = 1
WHERE nSource.CompanyID <> 1
AND nsSys.SetupID IN 
('af1d29f2-53cd-4400-b22a-41151f8c399d'
,'baba9b32-3e14-4e31-b78a-6efdd8245a71'
,'3a99fb5f-e965-4f03-9769-731641f79d31'
,'F7A67E58-5A98-41DC-B3AE-BDA89976D803'
,'86c3538c-f196-4ec0-a60b-38e7a4bf2fe0')

UPDATE nsrec
SET nsrec.SetupID = nsSys.SetupID
FROM NotificationRecipient nsrec
INNER JOIN NotificationSetup nsUser
	ON nsrec.CompanyID = nsUser.CompanyID
		AND nsrec.SetupID = nsUser.SetupID
INNER JOIN NotificationSetup nsSys
	ON (nsUser.NBranchID = nsSys.NBranchID OR (nsUser.NBranchID IS NULL AND nsSys.NBranchID IS NULL))
		AND nsUser.SourceCD = nsSys.SourceCD
		AND nsUser.NotificationCD = nsSys.NotificationCD
		AND nsUser.Module = nsSys.Module
		AND nsUser.SetupID <> nsSys.SetupID
		AND nsSys.CompanyID = 1
WHERE nsrec.CompanyID <> 1
AND nsSys.SetupID IN 
('af1d29f2-53cd-4400-b22a-41151f8c399d'
,'baba9b32-3e14-4e31-b78a-6efdd8245a71'
,'3a99fb5f-e965-4f03-9769-731641f79d31'
,'F7A67E58-5A98-41DC-B3AE-BDA89976D803'
,'86c3538c-f196-4ec0-a60b-38e7a4bf2fe0')

UPDATE nsUser
SET nsUser.SetupID = nsSys.SetupID
FROM NotificationSetup nsUser
INNER JOIN NotificationSetup nsSys
	ON (nsUser.NBranchID = nsSys.NBranchID OR (nsUser.NBranchID IS NULL AND nsSys.NBranchID IS NULL))
		AND nsUser.SourceCD = nsSys.SourceCD
		AND nsUser.NotificationCD = nsSys.NotificationCD
		AND nsUser.Module = nsSys.Module
		AND nsUser.SetupID <> nsSys.SetupID
		AND nsSys.CompanyID = 1
WHERE nsUser.CompanyID <> 1
AND nsSys.SetupID IN 
('af1d29f2-53cd-4400-b22a-41151f8c399d'
,'baba9b32-3e14-4e31-b78a-6efdd8245a71'
,'3a99fb5f-e965-4f03-9769-731641f79d31'
,'F7A67E58-5A98-41DC-B3AE-BDA89976D803'
,'86c3538c-f196-4ec0-a60b-38e7a4bf2fe0')
        
		
DELETE FROM NotificationSetup
WHERE SetupID IN 
('af1d29f2-53cd-4400-b22a-41151f8c399d'
,'baba9b32-3e14-4e31-b78a-6efdd8245a71'
,'3a99fb5f-e965-4f03-9769-731641f79d31'
,'F7A67E58-5A98-41DC-B3AE-BDA89976D803'
,'86c3538c-f196-4ec0-a60b-38e7a4bf2fe0')
AND CompanyID = 1
GO

--[mysql: Skip]
--[MinVersion(Hash = b1d7f4f49c1c4753dfa6276567cec77fb824fd9f0bdd47f1524d05516b82b699)]
DECLARE @maxSeq INT;

SELECT @maxSeq = MAX(NumberingSeq) + 1 FROM NumberingSequence;

UPDATE [NumberingSequence]
SET NumberingSeq = @maxSeq
FROM [NumberingSequence]
WHERE CompanyID <> 1
		AND NumberingID = 'SUBCONTR'
		AND NumberingSeq = 10057
		AND (NBranchID IS NOT NULL OR StartDate <> CONVERT (datetime2, '1900-01-01 00:00:00', 20))

UPDATE ns
SET ns.NumberingSeq = 10057
FROM NumberingSequence ns
	LEFT JOIN NumberingSequence ns2
		ON ns2.CompanyID = ns.CompanyID
				AND ns2.NumberingID = 'SUBCONTR'
				AND ns2.NumberingSeq = 10057
				AND ns2.NBranchID IS NULL 
				AND ns2.StartDate = CONVERT (datetime2, '1900-01-01 00:00:00', 20)
		WHERE ns.CompanyID <> 1
				AND ns.NumberingID = 'SUBCONTR'
				AND ns.NumberingSeq <> 10057
				AND ns.NBranchID IS NULL 
				AND ns.StartDate = CONVERT (datetime2, '1900-01-01 00:00:00', 20)
				AND ns2.CompanyID is null
GO



--[RefreshMetadata()]
GO

--PR scripts
--[MinVersion(Hash = 4034287911b9752353d27a074b9aeab97d092a15d1ee01bc15006b3b68cecbce)]

GO

--[IfExists(Column = PaymentMethodAccount.UseForPR)]
--[MinVersion(Hash = 66669faf6825f8190f1390d55c4759844f043d65bc9061399039b929941c2b2c)]
UPDATE PaymentMethodAccount SET UseForPR = 0 WHERE UseForPR IS NULL
GO

--[IfExists(Column = FinPeriod.PRClosed)]
--[MinVersion(Hash = cc9fca68d7bf9faa244ed11a5030d6edd1e93facb2612c176c3845d204b975a1)]
UPDATE FinPeriod SET PRClosed = 0 WHERE PRClosed IS NULL
GO

--[IfExists(Column = PaymentMethod.UseForPR)]
--[MinVersion(Hash = 9178b78a99aad1488e3faa6f6b2d7a78c21b5c2e7c216211c372527b3ddd3041)]
UPDATE PaymentMethod SET UseForPR = 0 WHERE UseForPR IS NULL
GO

--[IfExists(Column = PaymentMethod.PRStubLines)]
--[MinVersion(Hash = 9ecbb36393a5e272dad2c905d8d41f8b35603c2d6997071022f42231d2088597)]
UPDATE PaymentMethod SET PRStubLines = 0 WHERE PRStubLines IS NULL
GO

--[IfExists(Column = FeaturesSet.PayrollModule)]
--[MinVersion(Hash = 685ce0b32c13b1ff59818c3d95bc2b6d744ba95be1c73b7e48bf65400897f813)]
UPDATE FeaturesSet SET PayrollModule = 0 WHERE PayrollModule IS NULL
GO

--[IfExists(Column = CABatchDetail.OrigLineNbr)]
--[MinVersion(Hash = 11b3f92f11c315a76888f268f5f286863e2d9bde78b79dcf7a2e3378e2c8b3be)]
UPDATE CABatchDetail SET OrigLineNbr = 0 WHERE OrigLineNbr IS NULL
GO

--[IfExists(Column = CABatch.OrigModule)]
--[MinVersion(Hash = 026c90320a77ef36c3b103b51befc86e59094de1e565c476177f6e16dcbd548b)]
UPDATE CABatch SET OrigModule = 'AP' WHERE OrigModule IS NULL
GO

--[IfExists(Table = PREmployeeDD)]
--[MinVersion(Hash = f1148abc9fbe1f3f413d7dc384e69cc98be9ed5da7c4e9ce2b745ac58242279c)]
DROP TABLE PREmployeeDD
GO

--[IfExists(Column = PREmployeePTOBank.IsActive)]
--[MinVersion(Hash = d779280790cfcb83809a5306b8ed2071bd65196741d2afafef41caf603f6586f)]
UPDATE PREmployeePTOBank SET IsActive = 1 WHERE IsActive IS NULL
GO

--[IfExists(Column = PRAcaCompanyYearlyInformation.OrgBAccountID)]
--[MinVersion(Hash = 587b0ad00e10d54c8ea1727dcfec4405ac17c3523654b272e3e9f8e1dd293f2d)]
UPDATE PRAcaCompanyYearlyInformation SET OrgBAccountID = CASE WHEN BranchID IS NOT NULL THEN BranchID ELSE 0 END
WHERE OrgBAccountID IS NULL
GO

--[IfExists(Column = PRAcaCompanyMonthlyInformation.OrgBAccountID)]
--[MinVersion(Hash = a354776b0062be0430407ebf2b345ceffdc12a10a2933d7844f7daa04c1a5000)]
UPDATE PRAcaCompanyMonthlyInformation SET OrgBAccountID = CASE WHEN BranchID IS NOT NULL THEN BranchID ELSE 0 END
WHERE OrgBAccountID IS NULL
GO

--[IfExists(Column = PRAcaEmployeeMonthlyInformation.OrgBAccountID)]
--[MinVersion(Hash = 61431f1215a882dcd612d387f1786b0579578916b976aec2a8610c700b2012f9)]
UPDATE PRAcaEmployeeMonthlyInformation SET OrgBAccountID = CASE WHEN BranchID IS NOT NULL THEN BranchID ELSE 0 END
WHERE OrgBAccountID IS NULL
GO

--[IfExists(Column = PRAcaAggregateGroupMember.OrgBAccountID)]
--[MinVersion(Hash = f492dfb731b6d74bfb11b13d09d4acba190eda699dd598c6f59526a9113e93d8)]
UPDATE PRAcaAggregateGroupMember SET OrgBAccountID = CASE WHEN BranchID IS NOT NULL THEN BranchID ELSE 0 END
WHERE OrgBAccountID IS NULL
GO

--[IfExists(Column = PRPayment.BenefitAmount)]
--[MinVersion(Hash = f78afaf153a7df05fba609821de82eddfaffd6557e385a335754c4cd705be96f)]
UPDATE PRPayment 
SET BenefitAmount = 
	(SELECT ISNULL(SUM(PRBenefitDetail.Amount), 0) AS TotalBenefitAmount 
	 FROM PRBenefitDetail 
	 WHERE PRBenefitDetail.CompanyID = PRPayment.CompanyID AND 
		   PRBenefitDetail.PaymentDocType = PRPayment.DocType AND 
		   PRBenefitDetail.PaymentRefNbr = PRPayment.RefNbr) 
WHERE BenefitAmount IS NULL
GO

--[IfExists(Column = PRPayment.EmployerTaxAmount)]
--[MinVersion(Hash = 24281f0eb4d68e3fbbaee988e91f10bd838d2f5bb392dce5f6867f2ca42136fe)]
UPDATE PRPayment 
SET EmployerTaxAmount = 
	(SELECT ISNULL(SUM(PRPaymentTax.TaxAmount), 0) AS TotalTaxAmount 
	 FROM PRPaymentTax 
	 WHERE PRPaymentTax.CompanyID = PRPayment.CompanyID AND 
		   PRPaymentTax.TaxCategory = 'CNT' AND 
		   PRPaymentTax.DocType = PRPayment.DocType AND 
		   PRPaymentTax.RefNbr = PRPayment.RefNbr) 
WHERE EmployerTaxAmount IS NULL
GO

--[IfExists(Column = PREarningType.WageTypeCD)]
--[MinVersion(Hash = 5e99a8cb5255a93c6b2f6bd5dc535fb7255fd0720bc4b86fd4507c598c797e1d)]
UPDATE PREarningType SET WageTypeCD = 1 WHERE WageTypeCD IS NULL
GO

--[IfExists(Column = PREmployeeDeduct.DedMaxFreqType)]
--[MinVersion(Hash = 38953f719263247b7f638dc73315a1f437deb56d0843be6eae3fb8291b7ac357)]
UPDATE PREmployeeDeduct
SET DedMaxFreqType = PRDeductCode.DedMaxFreqType,
	DedMaxAmount = CASE WHEN PREmployeeDeduct.DedMaxAmount IS NULL 
	THEN PRDeductCode.DedMaxAmount 
	ELSE PREmployeeDeduct.DedMaxAmount END
FROM PRDeductCode
WHERE PREmployeeDeduct.DedMaxFreqType IS NULL 
	AND PREmployeeDeduct.DedUseDflt = 0
	AND PREmployeeDeduct.CodeID = PRDeductCode.CodeID
	AND PREmployeeDeduct.CompanyID = PRDeductCode.CompanyID
GO

--[IfExists(Column = PREmployeeDeduct.CntMaxFreqType)]
--[MinVersion(Hash = 18ce94c5535be6ee21e448553bdef52fd88e7add7b3a2719669a129254ed69cd)]
UPDATE PREmployeeDeduct
SET CntMaxFreqType = PRDeductCode.CntMaxFreqType,
	CntMaxAmount = CASE WHEN PREmployeeDeduct.CntMaxAmount IS NULL 
	THEN PRDeductCode.CntMaxAmount 
	ELSE PREmployeeDeduct.CntMaxAmount END
FROM PRDeductCode
WHERE PREmployeeDeduct.CntMaxFreqType IS NULL 
	AND PREmployeeDeduct.CntUseDflt = 0
	AND PREmployeeDeduct.CodeID = PRDeductCode.CodeID
	AND PREmployeeDeduct.CompanyID = PRDeductCode.CompanyID
GO

--[IfExists(Column = PREmployee.UnionID)]
--[IfExists(Column = EPEmployee.UnionID)]
--[MinVersion(Hash = 0e12d545ab88f83d9aeaa2ca4961538f7b63c042e01292ed5a092cad5e7ee61c)]
UPDATE EPEmployee
SET UnionID = PREmployee.UnionID
FROM PREmployee
WHERE EPEmployee.UnionID IS NULL 
	AND EPEmployee.BAccountID = PREmployee.BAccountID
	AND EPEmployee.CompanyID = PREmployee.CompanyID
GO

--[IfExists(Column = PREmployee.TaxExpenseAcctID)]
--[IfExists(Column = PREmployee.PayrollTaxExpenseAcctID)]
--[MinVersion(Hash = 26925f9cb853d938b64ce992990b4d97d1945d15e398104b6e9a82449fa2457e)]
UPDATE PREmployee
SET PayrollTaxExpenseAcctID = TaxExpenseAcctID
GO

--[IfExists(Column = PREmployee.TaxExpenseSubID)]
--[IfExists(Column = PREmployee.PayrollTaxExpenseSubID)]
--[MinVersion(Hash = 8559890dce22a6bd6c7e7f24c6b899f8230a8a36156aca7bbc2a0bde343d4eac)]
UPDATE PREmployee
SET PayrollTaxExpenseSubID = TaxExpenseSubID
GO

--[IfExists(Column = PREmployee.TaxLiabilityAcctID)]
--[IfExists(Column = PREmployee.PayrollTaxLiabilityAcctID)]
--[MinVersion(Hash = cb0bcf9ba6371a6136d57bb1c19c33d5196d9fae89f815dc44948482d8b070fa)]
UPDATE PREmployee
SET PayrollTaxLiabilityAcctID = TaxLiabilityAcctID
GO

--[IfExists(Column = PREmployee.TaxLiabilitySubID)]
--[IfExists(Column = PREmployee.PayrollTaxLiabilitySubID)]
--[MinVersion(Hash = ae5ccd327b1edb4e819255e00ac0f27982638b9e0323185646b5392e4d152f7d)]
UPDATE PREmployee
SET PayrollTaxLiabilitySubID = TaxLiabilitySubID
GO

--[IfExists(Column = PREmployee.ActiveInPayroll)]
--[MinVersion(Hash = dabe8aa386737dd5705290923829cdae4f97eb3f141ddfad5fc4fb214c3272c4)]
UPDATE PREmployee SET ActiveInPayroll = 1 WHERE ActiveInPayroll IS NULL
GO

--[IfExists(Column = PREarningType.IsPTO)]
--[MinVersion(Hash = 3be5a997b86b15d411df86dc812a2902849f02bbcf39977edb431037768c5c36)]
UPDATE PREarningType
SET IsPTO = 0
WHERE IsPTO IS NULL
GO

--[IfExists(Column = PREarningType.AccruePTO)]
--[MinVersion(Hash = 18e9cf0e4279c85af06148cf05238bdc163be9121d38ec0b399c3685385de57e)]
UPDATE PREarningType
SET AccruePTO = 1
WHERE AccruePTO IS NULL
GO

--[IfExists(Column = PRSetup.ProjectCostAssignment)]
--[MinVersion(Hash = 3ec36e26f4e9abc623b024e4f557fbaad7493390f19511304523cf85258c063e)]
UPDATE PRSetup SET ProjectCostAssignment = 'WCA' WHERE ProjectCostAssignment IS NULL
GO

--[IfExists(Column = PRSetup.TaxExpenseAcctDefault)]
--[MinVersion(Hash = 72ae6f9b16d6adf66da412c882eaccff1ef89ef1d4530526b5b0be6ecb93c07f)]
UPDATE PRSetup SET TaxExpenseAcctDefault = CASE WHEN TaxExpenseAcctDefault = 'T' THEN 'X' ELSE TaxExpenseAcctDefault END
GO

--[IfExists(Column = PRSetup.TaxLiabilityAcctDefault)]
--[MinVersion(Hash = f62669075efcd5de120b0be5ce09b42850b63dec534a65829a3ebad8b3e02e6b)]
UPDATE PRSetup SET TaxLiabilityAcctDefault = CASE WHEN TaxLiabilityAcctDefault = 'T' THEN 'X' ELSE TaxLiabilityAcctDefault END
GO

--[IfExists(Column = PRSetup.TaxExpenseSubMask)]
--[MinVersion(Hash = 13de4bc0b9d4cc889f87e788780377610021f4cf8735352eca7065a2a167cc61)]
UPDATE PRSetup SET TaxExpenseSubMask = REPLACE(TaxExpenseSubMask, 'T', 'X')
GO

--[IfExists(Column = PRSetup.TaxLiabilitySubMask)]
--[MinVersion(Hash = e922422b9b400e3882018c3fbecb9cca349b5e535c64e33466df46ab4b2b39f9)]
UPDATE PRSetup SET TaxLiabilitySubMask = REPLACE(TaxLiabilitySubMask, 'T', 'X')
GO

--[IfExists(Column = PREarningType.IsPTO)]
--[MinVersion(Hash = 4d21ee9a388ad5a02473ab37ef6305404b201349ce02f5e473bf7187c901809d)]
UPDATE PREarningType
SET IsPTO = 1
FROM PREarningType
INNER JOIN PRPTOBank ON PRPTOBank.EarningTypeCD = PREarningType.TypeCD AND PRPTOBank.CompanyID = PREarningType.CompanyID
GO

--[MinVersion(Hash = d543499a316d7bd0bf6a835175843dfcc24b8e5de7e7579489537d345f7d3f21)]
UPDATE EPEarningType
SET IsActive = 1
FROM EPEarningType
INNER JOIN PROvertimeRule ON PROvertimeRule.DisbursingTypeCD = EPEarningType.TypeCD AND PROvertimeRule.CompanyID = EPEarningType.CompanyID
WHERE PROvertimeRule.IsActive = 1
GO

--[MinVersion(Hash = fd552081df44ebe324b3af20555fb68563c445c2059d4be4640e0097eed881d2)]
UPDATE EPEarningType
SET IsActive = 1
FROM EPEarningType
INNER JOIN PRPTOBank ON PRPTOBank.EarningTypeCD = EPEarningType.TypeCD AND PRPTOBank.CompanyID = EPEarningType.CompanyID
WHERE PRPTOBank.ISActive = 1
GO

--[MinVersion(Hash = 688613208ed4e11d3536c0001f625807b5cb2dafe398829f911969678c085b41)]
UPDATE EPEarningType
SET IsActive = 1
FROM EPEarningType
INNER JOIN PREarningDetail ON PREarningDetail.TypeCD = EPEarningType.TypeCD AND PREarningDetail.CompanyID = EPEarningType.CompanyID
LEFT JOIN PRPayment ON PRPayment.DocType = PREarningDetail.PaymentDocType AND PRPayment.RefNbr = PREarningDetail.PaymentRefNbr AND PRPayment.CompanyID = PREarningDetail.CompanyID
LEFT JOIN PRBatch ON PRBatch.BatchNbr = PREarningDetail.BatchNbr AND PRBatch.CompanyID = PREarningDetail.CompanyID
WHERE PRPayment.Released = 0 OR
	(PRPayment.RefNbr IS NULL AND
		PRBatch.BatchNbr IS NOT NULL)
GO

--[IfExists(Column = PRDeductionAndBenefitProjectPackage.DeductionAmount)]
-- Remove CATran duplicates that were (incorrectly) created on PRBatch Release Action.
--[IfExists(Column = PRDirectDepositSplit.CATranID)]
--[MinVersion(Hash = 1124429937daf5ef8686f4501fec4928685a8f114bf15315330d39db99dc9da9)]
DELETE CATanRecord FROM CATran CATanRecord
LEFT JOIN PRPayment on 
	CATanRecord.CompanyID = PRPayment.CompanyID AND 
	CATanRecord.TranID = PRPayment.CATranID 
LEFT JOIN PRDirectDepositSplit on 
	CATanRecord.CompanyID = PRDirectDepositSplit.CompanyID AND 
	CATanRecord.TranID = PRDirectDepositSplit.CATranID
WHERE OrigModule = 'PR' AND PRPayment.RefNbr IS NULL AND PRDirectDepositSplit.CATranID IS NULL
GO

--[MinVersion(Hash = 32dba9fa018c652b074512dce79da2b4860059faa1d849514ccd18b1ab372539)]
UPDATE PREarningType SET RegularTypeCD =
	(SELECT TOP 1 RegularHoursType FROM PRSetup
	 WHERE PRSetup.CompanyID = PREarningType.CompanyID)
WHERE PREarningType.IsPTO = 1
	AND PREarningType.RegularTypeCD IS NULL
GO

--[mysql: Skip]
--[mssql: Native]
--[IfExists(Column = PRDeductionAndBenefitProjectPackage.DeductionAmount)]
--[IfExists(Column = PRDeductionAndBenefitProjectPackage.BenefitAmount)]
--[MinVersion(Hash = 5f071bfcb739503d17ce3470610255e3863269d6b4af043cbd0ac8dc23eee19b)]
ALTER TABLE PRDeductionAndBenefitProjectPackage ALTER COLUMN [DeductionRate] [decimal] (19,6) NULL
ALTER TABLE PRDeductionAndBenefitProjectPackage ALTER COLUMN [BenefitRate] [decimal] (19,6) NULL
GO

--[mysql: Native]
--[mssql: Skip]
--[IfExists(Column = PRDeductionAndBenefitProjectPackage.DeductionAmount)]
--[IfExists(Column = PRDeductionAndBenefitProjectPackage.BenefitAmount)]
--[MinVersion(Hash = 5b72883d045c95d20f768212c54cb2347aae42af3d40988a38c0e1310a439a93)]
ALTER TABLE PRDeductionAndBenefitProjectPackage MODIFY COLUMN DeductionRate decimal (19,6) NULL;
ALTER TABLE PRDeductionAndBenefitProjectPackage MODIFY COLUMN BenefitRate decimal (19,6) NULL;
GO

--[IfExists(Column = PRDeductionAndBenefitProjectPackage.DeductionAmount)]
--[IfExists(Column = PRDeductionAndBenefitProjectPackage.BenefitAmount)]
--[MinVersion(Hash = a0fffb900e7d9b3580e64f529061e2ab59cc8dfd7c77bd98a9fbb32c606a3675)]
UPDATE PRDeductionAndBenefitProjectPackage
SET PRDeductionAndBenefitProjectPackage.DeductionAmount = CASE WHEN PRDeductCode.ContribType = 'CNT' OR (PRDeductCode.DedCalcType <> 'FIX' AND PRDeductCode.DedCalcType <> 'APH') THEN NULL ELSE PRDeductCode.DedAmount END,
    PRDeductionAndBenefitProjectPackage.DeductionRate = CASE WHEN PRDeductCode.ContribType = 'CNT' OR PRDeductCode.DedCalcType <> 'GRS' THEN NULL ELSE CASE WHEN PRDeductionAndBenefitProjectPackage.DeductionRate IS NULL THEN PRDeductCode.DedPercent ELSE PRDeductionAndBenefitProjectPackage.DeductionRate END END,
	PRDeductionAndBenefitProjectPackage.BenefitAmount = CASE WHEN PRDeductCode.ContribType = 'DED' OR (PRDeductCode.CntCalcType <> 'FIX' AND PRDeductCode.CntCalcType <> 'APH') THEN NULL ELSE PRDeductCode.CntAmount END,
    PRDeductionAndBenefitProjectPackage.BenefitRate = CASE WHEN PRDeductCode.ContribType = 'DED' OR PRDeductCode.CntCalcType <> 'GRS' THEN NULL ELSE CASE WHEN PRDeductionAndBenefitProjectPackage.BenefitRate IS NULL THEN PRDeductCode.CntPercent ELSE PRDeductionAndBenefitProjectPackage.BenefitRate END END
FROM PRDeductionAndBenefitProjectPackage
INNER JOIN PRDeductCode ON PRDeductCode.CodeID = PRDeductionAndBenefitProjectPackage.DeductionAndBenefitCodeID
GO

--[mysql: Skip]
--[mssql: Native]
--[IfExists(Column = PRDeductionAndBenefitUnionPackage.DeductionAmount)]
--[IfExists(Column = PRDeductionAndBenefitUnionPackage.BenefitAmount)]
--[MinVersion(Hash = dc39d7194a1d967f66a1fce6738422f3c2cee33715716304c3eaf1a254a112b6)]
ALTER TABLE PRDeductionAndBenefitUnionPackage ALTER COLUMN [DeductionRate] [decimal] (19,6) NULL
ALTER TABLE PRDeductionAndBenefitUnionPackage ALTER COLUMN [BenefitRate] [decimal] (19,6) NULL
GO

--[mysql: Native]
--[mssql: Skip]
--[IfExists(Column = PRDeductionAndBenefitUnionPackage.DeductionAmount)]
--[IfExists(Column = PRDeductionAndBenefitUnionPackage.BenefitAmount)]
--[MinVersion(Hash = 4e3cf45273c4ad93bbc0de4ce78fe336e4fb7f7200ad8e1692ba84fe6abd79eb)]
ALTER TABLE PRDeductionAndBenefitUnionPackage MODIFY COLUMN DeductionRate decimal (19,6) NULL;
ALTER TABLE PRDeductionAndBenefitUnionPackage MODIFY COLUMN BenefitRate decimal (19,6) NULL;
GO

--[IfExists(Column = PRDeductionAndBenefitUnionPackage.DeductionAmount)]
--[IfExists(Column = PRDeductionAndBenefitUnionPackage.BenefitAmount)]
--[MinVersion(Hash = d7897cde3a3587b49479626d9f151df6f47ebbabeeb7890269928872a6c6da83)]
UPDATE PRDeductionAndBenefitUnionPackage
SET PRDeductionAndBenefitUnionPackage.DeductionAmount = CASE WHEN PRDeductCode.ContribType = 'CNT' OR (PRDeductCode.DedCalcType <> 'FIX' AND PRDeductCode.DedCalcType <> 'APH') THEN NULL ELSE PRDeductCode.DedAmount END,
    PRDeductionAndBenefitUnionPackage.DeductionRate = CASE WHEN PRDeductCode.ContribType = 'CNT' OR PRDeductCode.DedCalcType <> 'GRS' THEN NULL ELSE CASE WHEN PRDeductionAndBenefitUnionPackage.DeductionRate IS NULL THEN PRDeductCode.DedPercent ELSE PRDeductionAndBenefitUnionPackage.DeductionRate END END,
	PRDeductionAndBenefitUnionPackage.BenefitAmount = CASE WHEN PRDeductCode.ContribType = 'DED' OR (PRDeductCode.CntCalcType <> 'FIX' AND PRDeductCode.CntCalcType <> 'APH') THEN NULL ELSE PRDeductCode.CntAmount END,
    PRDeductionAndBenefitUnionPackage.BenefitRate = CASE WHEN PRDeductCode.ContribType = 'DED' OR PRDeductCode.CntCalcType <> 'GRS' THEN NULL ELSE CASE WHEN PRDeductionAndBenefitUnionPackage.BenefitRate IS NULL THEN PRDeductCode.CntPercent ELSE PRDeductionAndBenefitUnionPackage.BenefitRate END END
FROM PRDeductionAndBenefitUnionPackage
INNER JOIN PRDeductCode ON PRDeductCode.CodeID = PRDeductionAndBenefitUnionPackage.DeductionAndBenefitCodeID
GO

--[IfExists(Column = PRProjectFringeBenefitRateReducingDeduct.CertifiedReportType)]
--[IfExists(Column = PRDeductCode.CertifiedReportType)]
--[MinVersion(Hash = 0d52a59b120fc002eeed645aceec6749ba0ba1e9af6429891dbea16326b6966d)]
UPDATE PRDeductCode
SET CertifiedReportType = (
	SELECT TOP 1 CertifiedReportType
	FROM PRProjectFringeBenefitRateReducingDeduct
	WHERE DeductCodeID = CodeID
		AND PRProjectFringeBenefitRateReducingDeduct.CompanyID = PRDeductCode.CompanyID
)
GO

--[IfExists(Column = Contract.WageAbovePrevailingAnnualizationException)]
--[MinVersion(Hash = fd41fd5d9428c361ae253f333b3b9e0866e0e56fc3e67dec4d28e52e810900c0)]
UPDATE Contract
SET WageAbovePrevailingAnnualizationException = 1 WHERE WageAbovePrevailingAnnualizationException IS NULL
GO

--[IfExists(Column = Contract.FileEmptyCertifiedReport)]
--[MinVersion(Hash = 9e1da3e96e1a944b1daa271f7b8931c58f10c19536981b965754f9a6274efdc3)]
UPDATE Contract
SET FileEmptyCertifiedReport = 0 WHERE FileEmptyCertifiedReport IS NULL
GO



--[RefreshMetadata()]
GO

--FS scripts
--[MinVersion(Hash = 4034287911b9752353d27a074b9aeab97d092a15d1ee01bc15006b3b68cecbce)]

GO

--[mysql: Native]
--[mssql: Skip]
--[IfExists(Column = FSScheduleDet.LineNbr)]
--[MinVersion(Hash = 9cdcfab130ec826cce6e122459e430fcbda1dc266f717cc8f372bb8b5dbb77cb)]
UPDATE FSScheduleDet AS Det
  INNER JOIN (SELECT count(*) AS RowCountNbr, A.ScheduleID, A.ScheduleDetID, A.CompanyID FROM FSScheduleDet A 
          INNER JOIN FSScheduleDet B ON A.CompanyID = B.CompanyID
                        AND A.ScheduleID = B.ScheduleID
                        AND A.ScheduleDetID >= B.ScheduleDetID
          GROUP BY A.CompanyID, A.ScheduleID, A.ScheduleDetID) AS SelectCount ON
            SelectCount.ScheduleID = Det.ScheduleID 
            AND SelectCount.ScheduleDetID = Det.ScheduleDetID
            AND SelectCount.CompanyID = Det.CompanyID
  SET Det.LineNbr = SelectCount.RowCountNbr
WHERE Det.LineNbr IS NULL;
GO

--[mysql: Skip]
--[mssql: Native]
--[IfExists(Column = FSScheduleDet.LineNbr)]
--[MinVersion(Hash = 841c0142163f04a8b43003ca9ee6b343c47e45a4d8f549851271a71c6f30930d)]
UPDATE Det
  SET LineNbr = (SELECT count(*) FROM FSScheduleDet A 
          INNER JOIN FSScheduleDet B ON A.CompanyID = B.CompanyID
                        AND A.ScheduleID = B.ScheduleID
                        AND A.ScheduleDetID >= B.ScheduleDetID
          WHERE A.ScheduleID = Det.ScheduleID 
            AND A.ScheduleDetID = Det.ScheduleDetID
            AND A.CompanyID = Det.CompanyID
          GROUP BY A.CompanyID, A.ScheduleID, A.ScheduleDetID)
FROM FSScheduleDet Det
WHERE Det.LineNbr IS NULL;
GO

--[IfExists(Column = FSSchedule.LineCntr)]
--[MinVersion(Hash = 322aa5137ebcf976d001bc9e859df838f175f0930b9dd041c6ac17d8e22d63ec)]
UPDATE FSSchedule
  SET LineCntr = ISNULL((SELECT MAX(LineNbr)
                    FROM  FSScheduleDet
                    WHERE FSScheduleDet.ScheduleID = FSSchedule.ScheduleID
                        AND FSScheduleDet.CompanyID = FSSchedule.CompanyID), 0)
FROM FSSchedule
WHERE LineCntr IS NULL;
GO

--[IfExists(Column = FSSetup.ScheduleNumberingID)]
--[IfExists(Column = FSSetup.ServiceContractNumberingID)]
--[IfExistsSelect(From = FSSetup, WhereIsNull = ScheduleNumberingID)]
--[MinVersion(Hash = cd68b2f4bf4f8c429c8692b19bcdf415eded1fd4cc4c6880cb66de47ac482f8b)]
UPDATE FSSetup SET ScheduleNumberingID = 'FSSCHEDULE', ServiceContractNumberingID = 'FSCONTRACT';
GO

--[mysql: Skip]
--[mssql: Native]
--[IfExists(Column = FSServiceContract.CustomerContractNbr)]
--[IfExistsSelect(From = FSServiceContract, WhereIsNull = CustomerContractNbr)]
--[MinVersion(Hash = 4516f2a45a90b037a30dd144c367460654a149053b1bc315b147c9e786efc6a3)]
UPDATE FSServiceContract SET CustomerContractNbr = RefNbr WHERE CustomerContractNbr IS NULL;
UPDATE FSSchedule SET OldContractScheduleRefNbr = RefNbr WHERE EntityType = 'C';
UPDATE FSServiceContract SET RefNbr = RTRIM(CONCAT(SUBSTRING('FCT00000000', 1, LEN('FCT00000000') - LEN(CAST(ServiceContractID AS CHAR))) , CAST(ServiceContractID AS CHAR)));
UPDATE FSSchedule SET RefNbr = RTRIM(CONCAT(SUBSTRING('FSC00000000', 1, LEN('FSC00000000') - LEN(CAST(ScheduleID AS CHAR))) , CAST(ScheduleID AS CHAR)))
WHERE FSSchedule.EntityType = 'C';

INSERT INTO 
NumberingSequence(
  CompanyID
  ,NumberingID
  ,NBranchID
  ,StartNbr
  ,EndNbr
  ,StartDate
  ,LastNbr
  ,WarnNbr
  ,NbrStep
  ,CreatedByID
  ,CreatedByScreenID
  ,CreatedDateTime
  ,LastModifiedByID
  ,LastModifiedByScreenID
  ,LastModifiedDateTime
  ,NumberingSeq)
SELECT
  FSSetup.CompanyID
  ,'FSCONTRACT'
  ,NULL
  ,'FCT00000000'
  ,'FCT99999999'
  ,'1900-01-01 00:00:00.000'
  ,(SELECT MAX(RefNbr) FROM FSServiceContract WHERE FSServiceContract.CompanyID = FSSetup.CompanyID)
  ,'FCT99999899'
  ,1
  ,'00000000-0000-0000-0000-000000000000'
  ,'CS201010'
  ,GETDATE()
  ,'00000000-0000-0000-0000-000000000000'
  ,'CS201010'
  ,GETDATE()
  ,'75'
FROM FSSetup
LEFT JOIN NumberingSequence ON(NumberingSequence.CompanyID = FSSetup.CompanyID AND NumberingSequence.NumberingID = 'FSCONTRACT')
WHERE NumberingSequence.CompanyID IS NULL;

INSERT INTO 
NumberingSequence(
  CompanyID
  ,NumberingID
  ,NBranchID
  ,StartNbr
  ,EndNbr
  ,StartDate
  ,LastNbr
  ,WarnNbr
  ,NbrStep
  ,CreatedByID
  ,CreatedByScreenID
  ,CreatedDateTime
  ,LastModifiedByID
  ,LastModifiedByScreenID
  ,LastModifiedDateTime
  ,NumberingSeq)
SELECT
  FSSetup.CompanyID
  ,'FSSCHEDULE'
  ,NULL
  ,'FSC00000000'
  ,'FSC99999999'
  ,'1900-01-01 00:00:00.000'
  ,(SELECT MAX(RefNbr) FROM FSSchedule WHERE FSSchedule.CompanyID = FSSetup.CompanyID)
  ,'FSC99999899'
  ,1
  ,'00000000-0000-0000-0000-000000000000'
  ,'CS201010'
  ,GETDATE()
  ,'00000000-0000-0000-0000-000000000000'
  ,'CS201010'
  ,GETDATE()
  ,'76'
FROM FSSetup
LEFT JOIN NumberingSequence ON(NumberingSequence.CompanyID = FSSetup.CompanyID AND NumberingSequence.NumberingID = 'FSSCHEDULE')
WHERE NumberingSequence.CompanyID IS NULL;
GO

--[mysql: Native]
--[mssql: Skip]
--[IfExists(Column = FSServiceContract.CustomerContractNbr)]
--[IfExistsSelect(From = FSServiceContract, WhereIsNull = CustomerContractNbr)]
--[MinVersion(Hash = a3222af702296b250d34af5563f19cc475f05534d18c5d098445c44b26356504)]
UPDATE FSServiceContract SET CustomerContractNbr = RefNbr WHERE CustomerContractNbr IS NULL;
UPDATE FSSchedule SET OldContractScheduleRefNbr = RefNbr WHERE EntityType = 'C';
UPDATE FSServiceContract SET RefNbr = RTRIM(CONCAT(SUBSTRING('FCT00000000', 1, LENGTH('FCT00000000') - LENGTH(CAST(ServiceContractID AS CHAR))) , CAST(ServiceContractID AS CHAR)));
UPDATE FSSchedule SET RefNbr = RTRIM(CONCAT(SUBSTRING('FSC00000000', 1, LENGTH('FSC00000000') - LENGTH(CAST(ScheduleID AS CHAR))) , CAST(ScheduleID AS CHAR)))
WHERE FSSchedule.EntityType = 'C';

INSERT INTO 
NumberingSequence(
  CompanyID
  ,NumberingID
  ,NBranchID
  ,StartNbr
  ,EndNbr
  ,StartDate
  ,LastNbr
  ,WarnNbr
  ,NbrStep
  ,CreatedByID
  ,CreatedByScreenID
  ,CreatedDateTime
  ,LastModifiedByID
  ,LastModifiedByScreenID
  ,LastModifiedDateTime
  ,NumberingSeq)
SELECT
  FSSetup.CompanyID
  ,'FSCONTRACT'
  ,NULL
  ,'FCT00000000'
  ,'FCT99999999'
  ,'1900-01-01 00:00:00.000'
  ,(SELECT MAX(RefNbr) FROM FSServiceContract WHERE FSServiceContract.CompanyID = FSSetup.CompanyID)
  ,'FCT99999899'
  ,1
  ,'00000000-0000-0000-0000-000000000000'
  ,'CS201010'
  ,NOW()
  ,'00000000-0000-0000-0000-000000000000'
  ,'CS201010'
  ,NOW()
  ,'75'
FROM FSSetup
LEFT JOIN NumberingSequence ON(NumberingSequence.CompanyID = FSSetup.CompanyID AND NumberingSequence.NumberingID = 'FSCONTRACT')
WHERE NumberingSequence.CompanyID IS NULL;

INSERT INTO 
NumberingSequence(
  CompanyID
  ,NumberingID
  ,NBranchID
  ,StartNbr
  ,EndNbr
  ,StartDate
  ,LastNbr
  ,WarnNbr
  ,NbrStep
  ,CreatedByID
  ,CreatedByScreenID
  ,CreatedDateTime
  ,LastModifiedByID
  ,LastModifiedByScreenID
  ,LastModifiedDateTime
  ,NumberingSeq)
SELECT
  FSSetup.CompanyID
  ,'FSSCHEDULE'
  ,NULL
  ,'FSC00000000'
  ,'FSC99999999'
  ,'1900-01-01 00:00:00.000'
  ,(SELECT MAX(RefNbr) FROM FSSchedule WHERE FSSchedule.CompanyID = FSSetup.CompanyID)
  ,'FSC99999899'
  ,1
  ,'00000000-0000-0000-0000-000000000000'
  ,'CS201010'
  ,NOW()
  ,'00000000-0000-0000-0000-000000000000'
  ,'CS201010'
  ,NOW()
  ,'76'
FROM FSSetup
LEFT JOIN NumberingSequence ON(NumberingSequence.CompanyID = FSSetup.CompanyID AND NumberingSequence.NumberingID = 'FSSCHEDULE')
WHERE NumberingSequence.CompanyID IS NULL;
GO

--[IfExists(Column = FSManufacturer.AllowOverrideContactAddress)]
--[MinVersion(Hash = ece93adff0a98c32a075a01c7306e1463ec5a0d117726abb1d052b313c2a6670)]
UPDATE FSManufacturer SET
  AllowOverrideContactAddress = CASE WHEN ContactID IS NULL THEN 1 ELSE 0 END
WHERE
  AllowOverrideContactAddress IS NULL
GO

--[IfExists(Column = FSManufacturer.AddressLine1)]
--[IfExists(Table = FSAddress)]
--[IfExistsSelect(From = FSManufacturer, WhereIsNull = ManufacturerAddressID)]
--[MinVersion(Hash = 5b6eb7fe829c28023cd9205682b5c10f45c026f8f1346ed411bbe37f023a52ff)]
INSERT INTO [FSAddress]
           ([CompanyID]
           ,[AddressLine1]
           ,[AddressLine2]
           ,[AddressLine3]
           ,[City]
           ,[CountryID]
           ,[State]
           ,[PostalCode]
           ,[BAccountID]
           ,[BAccountAddressID]
           ,[RevisionID]
           ,[IsDefaultAddress]
           ,[IsValidated]
           ,[PseudonymizationStatus]
           ,[NoteID]
           ,[CreatedByID]
           ,[CreatedByScreenID]
           ,[CreatedDateTime]
           ,[LastModifiedByID]
           ,[LastModifiedByScreenID]
           ,[LastModifiedDateTime]
           ,[EntityID]
           ,[EntityType])
     SELECT
            CompanyID
           ,AddressLine1
           ,AddressLine2
           ,AddressLine3
           ,City
           ,CountryID
           ,[State]
           ,PostalCode
           ,NULL AS BAccountID
           ,NULL AS BAccountAddressID
           ,NULL AS RevisionID
           ,0 AS IsDefaultAddress
           ,IsValidated
           ,0 AS PseudonymizationStatus
           ,NewID() AS NoteID
           ,CreatedByID
           ,CreatedByScreenID
           ,CreatedDateTime
           ,LastModifiedByID
           ,LastModifiedByScreenID
           ,LastModifiedDateTime
           ,ManufacturerID AS EntityID
           ,'MNFC' AS EntityType
     FROM FSManufacturer

  UPDATE FSManufacturer SET
    ManufacturerAddressID = FSAddress.AddressID
  FROM FSManufacturer
    INNER JOIN FSAddress ON (FSAddress.CompanyID = FSManufacturer.CompanyID
                  AND FSAddress.EntityID = FSManufacturer.ManufacturerID
                  AND FSAddress.EntityType = 'MNFC')
  WHERE
    ManufacturerAddressID IS NULL
GO

--[IfExists(Column = FSManufacturer.Salutation)]
--[IfExists(Table = FSContact)]
--[IfExistsSelect(From = FSManufacturer, WhereIsNull = ManufacturerContactID)]
--[MinVersion(Hash = 036ee71581afae8722f4b09a5192dff294efc29c01c287c7e090503b81b94497)]
INSERT INTO [FSContact]
           ([CompanyID]
           ,[FirstName]
           ,[LastName]
           ,[Title]
           ,[Salutation]
           ,[Attention]
           ,[MidName]
           ,[FullName]
           ,[Email]
           ,[WebSite]
           ,[Phone1]
           ,[Phone1Type]
           ,[Phone2]
           ,[Phone2Type]
           ,[Phone3]
           ,[Phone3Type]
           ,[Fax]
           ,[FaxType]
           ,[DisplayName]
           ,[BAccountID]
           ,[BAccountContactID]
           ,[BAccountLocationID]
           ,[RevisionID]
           ,[IsDefaultContact]
           ,[PseudonymizationStatus]
           ,[ConsentAgreement]
           ,[ConsentDate]
           ,[ConsentExpirationDate]
           ,[NoteID]
           ,[CreatedByID]
           ,[CreatedByScreenID]
           ,[CreatedDateTime]
           ,[LastModifiedByID]
           ,[LastModifiedByScreenID]
           ,[LastModifiedDateTime]
           ,[EntityID]
           ,[EntityType])
     SELECT
            CS.CompanyID
           ,CASE WHEN CO.ContactID != null THEN CO.FirstName ELSE NULL END
           ,CASE WHEN CO.ContactID != null THEN CO.LastName ELSE NULL END
           ,CASE WHEN CO.ContactID != null THEN CO.Title ELSE NULL END
           ,CASE WHEN CO.ContactID != null THEN CO.Salutation ELSE NULL END
           ,CS.Salutation AS Attention
           ,CASE WHEN CO.ContactID != null THEN CO.MidName ELSE NULL END
           ,CASE WHEN CO.ContactID != null THEN CO.FullName ELSE NULL END
           ,CS.Email
           ,CS.WebSite
           ,CS.Phone1
           ,'B1' AS Phone1Type
           ,CS.Phone2
           ,'B2' AS Phone2Type
           ,CS.Phone3
           ,'H1' AS Phone3Type
           ,CS.Fax
           ,'BF' AS FaxType
           ,'' AS DisplayName
           ,NULL AS BAccountID
           ,NULL AS BAccountContactID
           ,NULL AS BAccountLocationID
           ,NULL AS RevisionID
           ,0 AS IsDefaultContact
           ,0 AS PseudonymizationStatus
           ,0 AS ConsentAgreement
           ,NULL AS ConsentDate
           ,NULL AS ConsentExpirationDate
           ,NewID() AS NoteID
           ,CS.CreatedByID
           ,CS.CreatedByScreenID
           ,CS.CreatedDateTime
           ,CS.LastModifiedByID
           ,CS.LastModifiedByScreenID
           ,CS.LastModifiedDateTime
           ,ManufacturerID AS EntityID
           ,'MNFC' AS EntityType
     FROM FSManufacturer AS CS
   LEFT JOIN Contact AS CO ON(CS.CompanyID = CO.CompanyID AND CS.ContactID = CO.ContactID)

  UPDATE FSManufacturer SET
    ManufacturerContactID = FSContact.ContactID
  FROM FSManufacturer
    INNER JOIN FSContact ON (FSContact.CompanyID = FSManufacturer.CompanyID
                  AND FSContact.EntityID = FSManufacturer.ManufacturerID
                  AND FSContact.EntityType = 'MNFC')
  WHERE
    ManufacturerContactID IS NULL
GO

--[IfExists(Column = FSBranchLocation.AllowOverrideContactAddress)]
--[MinVersion(Hash = 664bfd98ecfd204cc43fe4f482e8b298a5cd80f5092d78d2b7873d441c15fce6)]
UPDATE FSBranchLocation SET
  AllowOverrideContactAddress = 0
WHERE
  AllowOverrideContactAddress IS NULL
GO

--[IfExists(Column = FSBranchLocation.AddressLine1)]
--[IfExists(Table = FSBranchLocation)]
--[IfExistsSelect(From = FSBranchLocation, WhereIsNull = BranchLocationAddressID)]
--[MinVersion(Hash = 3dd6f87fc5f20f10ca1c794c8451d63e84c44a35930a100fdf75f0e9879a4033)]
INSERT INTO [FSAddress]
           ([CompanyID]
           ,[AddressLine1]
           ,[AddressLine2]
           ,[AddressLine3]
           ,[City]
           ,[CountryID]
           ,[State]
           ,[PostalCode]
           ,[BAccountID]
           ,[BAccountAddressID]
           ,[RevisionID]
           ,[IsDefaultAddress]
           ,[IsValidated]
           ,[PseudonymizationStatus]
           ,[NoteID]
           ,[CreatedByID]
           ,[CreatedByScreenID]
           ,[CreatedDateTime]
           ,[LastModifiedByID]
           ,[LastModifiedByScreenID]
           ,[LastModifiedDateTime]
           ,[EntityID]
           ,[EntityType])
     SELECT
            CompanyID
           ,AddressLine1
           ,AddressLine2
           ,AddressLine3
           ,City
           ,CountryID
           ,[State]
           ,PostalCode
           ,NULL AS BAccountID
           ,NULL AS BAccountAddressID
           ,NULL AS RevisionID
           ,0 AS IsDefaultAddress
           ,IsValidated
           ,0 AS PseudonymizationStatus
           ,NewID() AS NoteID
           ,CreatedByID
           ,CreatedByScreenID
           ,CreatedDateTime
           ,LastModifiedByID
           ,LastModifiedByScreenID
           ,LastModifiedDateTime
           ,BranchLocationID AS EntityID
           ,'BLOC' AS EntityType
     FROM FSBranchLocation

  UPDATE FSBranchLocation SET
    BranchLocationAddressID = FSAddress.AddressID
  FROM FSBranchLocation
    INNER JOIN FSAddress ON (FSAddress.CompanyID = FSBranchLocation.CompanyID
                  AND FSAddress.EntityID = FSBranchLocation.BranchLocationID
                  AND FSAddress.EntityType = 'BLOC')
  WHERE
    BranchLocationAddressID IS NULL
GO

--[IfExists(Column = FSBranchLocation.Salutation)]
--[IfExists(Table = FSContact)]
--[IfExistsSelect(From = FSBranchLocation, WhereIsNull = BranchLocationContactID)]
--[MinVersion(Hash = ff7db9d7354d427733427f6bd1a553b5b6a1f86cf5bfbef3c43481715e0fbe49)]
INSERT INTO [FSContact]
           ([CompanyID]
           ,[FirstName]
           ,[LastName]
           ,[Title]
           ,[Salutation]
           ,[Attention]
           ,[MidName]
           ,[FullName]
           ,[Email]
           ,[WebSite]
           ,[Phone1]
           ,[Phone1Type]
           ,[Phone2]
           ,[Phone2Type]
           ,[Phone3]
           ,[Phone3Type]
           ,[Fax]
           ,[FaxType]
           ,[DisplayName]
           ,[BAccountID]
           ,[BAccountContactID]
           ,[BAccountLocationID]
           ,[RevisionID]
           ,[IsDefaultContact]
           ,[PseudonymizationStatus]
           ,[ConsentAgreement]
           ,[ConsentDate]
           ,[ConsentExpirationDate]
           ,[NoteID]
           ,[CreatedByID]
           ,[CreatedByScreenID]
           ,[CreatedDateTime]
           ,[LastModifiedByID]
           ,[LastModifiedByScreenID]
           ,[LastModifiedDateTime]
           ,[EntityID]
           ,[EntityType])
     SELECT
            CompanyID
           ,NULL AS FirstName
           ,NULL AS LastName
           ,NULL AS Title
           ,CS.Salutation AS Salutation
           ,NULL AS Attention
           ,NULL AS MidName
           ,NULL AS FullName
           ,Email
           ,WebSite
           ,Phone1
           ,'B1' AS Phone1Type
           ,Phone2
           ,'B2' AS Phone2Type
           ,Phone3
           ,'H1' AS Phone3Type
           ,Fax
           ,'BF' AS FaxType
           ,'' AS DisplayName
           ,NULL AS BAccountID
           ,NULL AS BAccountContactID
           ,NULL AS BAccountLocationID
           ,NULL AS RevisionID
           ,0 AS IsDefaultContact
           ,0 AS PseudonymizationStatus
           ,0 AS ConsentAgreement
           ,NULL AS ConsentDate
           ,NULL AS ConsentExpirationDate
           ,NewID() AS NoteID
           ,CreatedByID
           ,CreatedByScreenID
           ,CreatedDateTime
           ,LastModifiedByID
           ,LastModifiedByScreenID
           ,LastModifiedDateTime
           ,BranchLocationID AS EntityID
           ,'BLOC' AS EntityType
     FROM FSBranchLocation AS CS

  UPDATE FSBranchLocation SET
    BranchLocationContactID = FSContact.ContactID
  FROM FSBranchLocation
    INNER JOIN FSContact ON (FSContact.CompanyID = FSBranchLocation.CompanyID
                  AND FSContact.EntityID = FSBranchLocation.BranchLocationID
                  AND FSContact.EntityType = 'BLOC')
  WHERE
    BranchLocationContactID IS NULL
GO

--[IfExists(Column = FSServiceOrder.AllowOverrideContactAddress)]
--[MinVersion(Hash = f9cd7ba27a588c10e6b955efe0b449008f3f82ad6009dc718fdc3ec0f3039d52)]
UPDATE FSServiceOrder SET
  AllowOverrideContactAddress = CASE WHEN ContactID IS NULL AND LocationID IS NULL THEN 0 ELSE 1 END
WHERE
  AllowOverrideContactAddress IS NULL
GO

--[IfExists(Column = FSServiceOrder.AddressLine1)]
--[IfExists(Table = FSAddress)]
--[IfExistsSelect(From = FSServiceOrder, WhereIsNull = ServiceOrderAddressID)]
--[MinVersion(Hash = 4c874c5786841cee331d0d132c20f6dc4ec9df6c69f331c30a149ff44d4a4c66)]
INSERT INTO [FSAddress]
           ([CompanyID]
           ,[AddressLine1]
           ,[AddressLine2]
           ,[AddressLine3]
           ,[City]
           ,[CountryID]
           ,[State]
           ,[PostalCode]
           ,[BAccountID]
           ,[BAccountAddressID]
           ,[RevisionID]
           ,[IsDefaultAddress]
           ,[IsValidated]
           ,[PseudonymizationStatus]
           ,[NoteID]
           ,[CreatedByID]
           ,[CreatedByScreenID]
           ,[CreatedDateTime]
           ,[LastModifiedByID]
           ,[LastModifiedByScreenID]
           ,[LastModifiedDateTime]
           ,[EntityID]
           ,[EntityType])
     SELECT
            CompanyID
           ,AddressLine1
           ,AddressLine2
           ,AddressLine3
           ,City
           ,CountryID
           ,[State]
           ,PostalCode
           ,NULL AS BAccountID
           ,NULL AS BAccountAddressID
           ,NULL AS RevisionID
           ,0 AS IsDefaultAddress
           ,AddressValidated AS IsValidated
           ,0 AS PseudonymizationStatus
           ,NewID() AS NoteID
           ,CreatedByID
           ,CreatedByScreenID
           ,CreatedDateTime
           ,LastModifiedByID
           ,LastModifiedByScreenID
           ,LastModifiedDateTime
           ,SOID AS EntityID
           ,'SROR' AS EntityType
     FROM FSServiceOrder

  UPDATE FSServiceOrder SET
    ServiceOrderAddressID = FSAddress.AddressID
  FROM FSServiceOrder
    INNER JOIN FSAddress ON (FSAddress.CompanyID = FSServiceOrder.CompanyID
                  AND FSAddress.EntityID = FSServiceOrder.SOID
                  AND FSAddress.EntityType = 'SROR')
  WHERE
    ServiceOrderAddressID IS NULL
GO

--[IfExists(Column = FSServiceOrder.Attention)]
--[IfExists(Table = FSContact)]
--[IfExistsSelect(From = FSServiceOrder, WhereIsNull = ServiceOrderContactID)]
--[MinVersion(Hash = bf06446bd2a8485f927578d4eef9de40c586dea8acd6e9fe3eb4a96a00422164)]
INSERT INTO [FSContact]
           ([CompanyID]
           ,[FirstName]
           ,[LastName]
           ,[Title]
           ,[Salutation]
           ,[Attention]
           ,[MidName]
           ,[FullName]
           ,[Email]
           ,[WebSite]
           ,[Phone1]
           ,[Phone1Type]
           ,[Phone2]
           ,[Phone2Type]
           ,[Phone3]
           ,[Phone3Type]
           ,[Fax]
           ,[FaxType]
           ,[DisplayName]
           ,[BAccountID]
           ,[BAccountContactID]
           ,[BAccountLocationID]
           ,[RevisionID]
           ,[IsDefaultContact]
           ,[PseudonymizationStatus]
           ,[ConsentAgreement]
           ,[ConsentDate]
           ,[ConsentExpirationDate]
           ,[NoteID]
           ,[CreatedByID]
           ,[CreatedByScreenID]
           ,[CreatedDateTime]
           ,[LastModifiedByID]
           ,[LastModifiedByScreenID]
           ,[LastModifiedDateTime]
           ,[EntityID]
           ,[EntityType])
     SELECT
            CS.CompanyID
           ,CASE WHEN CO.ContactID != null THEN CO.FirstName ELSE LCO.FirstName END
           ,CASE WHEN CO.ContactID != null THEN CO.LastName ELSE LCO.LastName END
       ,CASE WHEN CO.ContactID != null THEN CO.Title ELSE LCO.Title END
           ,CASE WHEN CO.ContactID != null THEN CO.Salutation ELSE LCO.Salutation END
       ,CS.Attention
       ,CASE WHEN CO.ContactID != null THEN CO.MidName ELSE LCO.MidName END
       ,CASE WHEN CO.ContactID != null THEN CO.FullName ELSE LCO.FullName END
           ,CS.Email
           ,NULL AS WebSite
           ,CS.Phone1
           ,'B1' AS Phone1Type
           ,CS.Phone2
           ,'B2' AS Phone2Type
           ,CS.Phone3
           ,'H1' AS Phone3Type
           ,CS.Fax
           ,'BF' AS FaxType
           ,'' AS DisplayName
           ,NULL AS BAccountID
           ,NULL AS BAccountContactID
           ,NULL AS BAccountLocationID
           ,NULL AS RevisionID
           ,0 AS IsDefaultContact
           ,0 AS PseudonymizationStatus
           ,0 AS ConsentAgreement
           ,NULL AS ConsentDate
           ,NULL AS ConsentExpirationDate
           ,NewID() AS NoteID
           ,CS.CreatedByID
           ,CS.CreatedByScreenID
           ,CS.CreatedDateTime
           ,CS.LastModifiedByID
           ,CS.LastModifiedByScreenID
           ,CS.LastModifiedDateTime
           ,SOID AS EntityID
           ,'SROR' AS EntityType
     FROM FSServiceOrder AS CS
   LEFT JOIN Contact AS CO ON(CS.CompanyID = CO.CompanyID AND CS.ContactID = CO.ContactID)
   LEFT JOIN Location AS LO ON(CS.CompanyID = LO.CompanyID AND CS.LocationID = LO.LocationID)
   LEFT JOIN Contact AS LCO ON(CS.CompanyID = LCO.CompanyID AND LO.DefContactID = LCO.ContactID)

  UPDATE FSServiceOrder SET
    ServiceOrderContactID = FSContact.ContactID
  FROM FSServiceOrder
    INNER JOIN FSContact ON (FSContact.CompanyID = FSServiceOrder.CompanyID
                  AND FSContact.EntityID = FSServiceOrder.SOID
                  AND FSContact.EntityType = 'SROR')
  WHERE
    ServiceOrderContactID IS NULL
GO

--[IfExists(Column = FSxARTran.EquipmentAction)]
--[MinVersion(Hash = 1a2bedf5f5f7fcc5953bfe0b22ffa21964f4959a70dbc63c8cdf0614405de9d8)]
UPDATE FSxARTran
SET 
    FSxARTran.SMEquipmentID = FSxSOLine.SMEquipmentID,
    FSxARTran.ComponentID = FSxSOLine.ComponentID,
    FSxARTran.EquipmentLineRef = FSxSOLine.EquipmentLineRef,
    FSxARTran.EquipmentAction = FSxSOLine.EquipmentAction,
    FSxARTran.Comment = FSxSOLine.Comment
FROM FSxARTran
INNER JOIN ARTran ON(ARTran.CompanyID = FSxARTran.CompanyID AND ARTran.TranType = FSxARTran.TranType AND ARTran.RefNbr = FSxARTran.RefNbr AND ARTran.LineNbr = FSxARTran.LineNbr)
INNER JOIN FSxSOLine ON(FSxSOLine.CompanyID = ARTran.CompanyID AND FSxSOLine.OrderType = ARTran.SOOrderType AND FSxSOLine.OrderNbr = ARTran.SOOrderNbr AND FSxSOLine.LineNbr = ARTran.SOOrderLineNbr)
WHERE (FSxSOLine.EquipmentAction IS NOT NULL AND FSxARTran.EquipmentAction IS NULL) OR FSxSOLine.EquipmentAction != FSxARTran.EquipmentAction;

UPDATE FSxARTran
SET 
    FSxARTran.NewTargetEquipmentLineNbr = ARTran2.LineNbr
FROM FSxARTran
INNER JOIN ARTran ON(ARTran.CompanyID = FSxARTran.CompanyID AND ARTran.TranType = FSxARTran.TranType AND ARTran.RefNbr = FSxARTran.RefNbr AND ARTran.LineNbr = FSxARTran.LineNbr)
INNER JOIN FSxSOLine ON(FSxSOLine.CompanyID = ARTran.CompanyID AND FSxSOLine.OrderType = ARTran.SOOrderType AND FSxSOLine.OrderNbr = ARTran.SOOrderNbr AND FSxSOLine.LineNbr = ARTran.SOOrderLineNbr)
INNER JOIN SOLine ON(SOLine.CompanyID = FSxSOLine.CompanyID AND SOLine.OrderType = FSxSOLine.OrderType AND SOLine.OrderNbr = FSxSOLine.OrderNbr AND SOLine.LineNbr = FSxSOLine.NewTargetEquipmentLineNbr)
INNER JOIN ARTran ARTran2 ON(ARTran2.CompanyID = SOLine.CompanyID AND ARTran2.SOOrderType = SOLine.OrderType AND ARTran2.SOOrderNbr = SOLine.OrderNbr AND ARTran2.SOOrderLineNbr = SOLine.LineNbr)
WHERE FSxSOLine.NewTargetEquipmentLineNbr != null AND FSxARTran.NewTargetEquipmentLineNbr IS NULL;
GO

--[IfExists(Column = FSPostInfo.SOInvPosted)]
--[IfExistsSelect(From = FSPostInfo, WhereIsNull = SOInvPosted)]
--[MinVersion(Hash = 2ae63be6651bc454fdb95e2f38728f52180d7729063ba9ab147667b92aafabd0)]
UPDATE FSPostInfo
SET FSPostInfo.SOInvPosted = 0
WHERE FSPostInfo.SOInvPosted IS NULL;
GO

--[IfExistsSelect(From = FSSetup)]
--[MinVersion(Hash = 948218d5c2663bd5accf91cbe6ce802b8b52a008c25d261c9d1dcaa9370723ea)]
UPDATE FSSetup SET DfltCalendarViewMode = 'VE' WHERE DfltCalendarViewMode IS NULL;
GO

--[MinVersion(Hash = 9b7a2dddd3a03e55ef56569452938e383076697148da4a4f8437130b3c0b1fa0)]
UPDATE FSSalesPrice
SET FSSalesPrice.LineType = 
  CASE
    WHEN InventoryItem.StkItem = 0 AND InventoryItem.ItemType = 'S' THEN 'SERVI'
    WHEN InventoryItem.StkItem = 0 AND InventoryItem.ItemType <> 'S' THEN 'NSTKI'
    ELSE 'SLPRO'
  END
FROM FSSalesPrice
INNER JOIN InventoryItem on InventoryItem.InventoryID = FSSalesPrice.InventoryID 
              AND InventoryItem.CompanyID = FSSalesPrice.CompanyID
WHERE FSSalesPrice.LineType = 'TEMPL'
GO

--[MinVersion(Hash = cce120769f0c57d56fca6fb64b0cbd43cca7e86ad7d9e69391dd68a2a436d30a)]
UPDATE FSxService
  SET FSxService.BillingRule = 'FLRA'
FROM FSxService
INNER JOIN InventoryItem ON FSxService.CompanyID = InventoryItem.CompanyID
                        AND FSxService.InventoryID = InventoryItem.InventoryID
WHERE InventoryItem.StkItem = 1
AND FSxService.BillingRule <> 'FLRA'
GO

--[IfExists(Table = FSCustomFieldAppointment)]
--[MinVersion(Hash = 8774a6b0b21fa709eedb57c9f4921de47c507ffb68ac26a544b8e5f0c886cb86)]
DELETE FROM FSCustomFieldAppointment WHERE FieldName = 'AttendeeCount';
GO

--[IfExists(Column = FSManufacturer.AllowOverrideContactAddress)]
--[MinVersion(Hash = efa88ab44f08b09fe1c02554aa32406cc46ccffc9861d2df5168eda01e61096b)]
UPDATE FSManufacturer SET
  AllowOverrideContactAddress = 1
WHERE
  ContactID IS NULL
  AND (
	AllowOverrideContactAddress IS NULL OR AllowOverrideContactAddress = 0
  )
GO

--[IfExists(Table = FSCause)]
--[IfExistsSelect(From = FSCause)]
--[IfExists(Column = FSServiceOrder.CauseID)]
--[MinVersion(Hash = 122fb1aabab47eb4c333ab990b54d21dbe10d59ad762e105af196232e2f5ce6c)]
IF NOT EXISTS(SELECT * FROM CSAttribute WHERE AttributeID = 'FSCAUSE')
BEGIN
  INSERT INTO [CSAttribute]
         ([CompanyID],[AttributeID],[Description],[ControlType],[EntryMask],[RegExp],[List],[IsInternal],[ContainsPersonalData]
         ,[CreatedByID],[CreatedByScreenID],[CreatedDateTime],[LastModifiedByID],[LastModifiedByScreenID],[LastModifiedDateTime],[NoteID])
  SELECT
        FSSrvOrdType.CompanyID,'FSCAUSE','Cause Code',2,NULL,NULL,NULL,0,0,MAX(FSSrvOrdType.CreatedByID),'CS205000',GETDATE()
        ,MAX(FSSrvOrdType.CreatedByID),'CS205000',GETDATE(),NEWID()
  FROM FSSrvOrdType 
  GROUP BY FSSrvOrdType.CompanyID
END
GO

--[IfExists(Table = FSCause)]
--[IfExistsSelect(From = FSCause)]
--[IfExists(Column = FSServiceOrder.CauseID)]
--[MinVersion(Hash = 0af507a97637000d9412a75dab9a4257f67cb954a0cb86c9de0156a3d6272391)]
IF EXISTS(SELECT * FROM CSAttribute WHERE AttributeID = 'FSCAUSE')
AND NOT EXISTS(SELECT * FROM CSAttributeDetail WHERE AttributeID = 'FSCAUSE')
BEGIN
  INSERT INTO [CSAttributeDetail]
         ([CompanyID],[AttributeID],[ValueID],[Description],[SortOrder],[Disabled],[CreatedByID],[CreatedByScreenID]
         ,[CreatedDateTime],[LastModifiedByID],[LastModifiedByScreenID],[LastModifiedDateTime],[NoteID])
  SELECT
        FC.CompanyID,'FSCAUSE',LEFT(LTRIM(FC.CauseCD), 10),FC.Descr,NULL,0,FC.CreatedByID,'CS205000',FC.CreatedDateTime,FC.LastModifiedByID
        ,'CS205000',FC.LastModifiedDateTime,NewID()
    FROM FSCause AS FC
    INNER JOIN CSAttribute ON FC.CompanyID  = CSAttribute.CompanyID 
    WHERE CSAttribute.AttributeID = 'FSCAUSE'
END
GO

--[IfExists(Table = FSCause)]
--[IfExists(Column = FSServiceOrder.CauseID)]
--[OldHash(Hash = 9a29fdb9b9eb0bcba479c96e2d264ee896f167dbd0afff95138d2b41bb4cba2b)]
--[MinVersion(Hash = cd5256cbb2b4f6dbf453c12ae22a78f6a1c972ce813f99f132ce74ef5ca29450)]
IF EXISTS(SELECT * FROM CSAttribute WHERE AttributeID = 'FSCAUSE')
AND NOT EXISTS(SELECT * FROM CSAttributeGroup WHERE AttributeID = 'FSCAUSE')
BEGIN
  INSERT INTO [CSAttributeGroup]
        ([CompanyID],[AttributeID],[EntityClassID],[EntityType],[CreatedByID],[CreatedByScreenID],[CreatedDateTime]
        ,[LastModifiedByID],[LastModifiedByScreenID],[LastModifiedDateTime],[Required],[IsActive],[AttributeCategory])
  SELECT 
        FSSrvOrdType.CompanyID,'FSCAUSE',FSSrvOrdType.SrvOrdType,'PX.Objects.FS.FSSrvOrdTypeAttributeList'
        ,MAX(FSSrvOrdType.CreatedByID),'FS202300',GETDATE(),MAX(FSSrvOrdType.CreatedByID),'FS202300',GETDATE(),0,1,'A'
  FROM FSSrvOrdType
  INNER JOIN FSServiceOrder ON FSServiceOrder.CompanyID = FSSrvOrdType.CompanyID 
                            AND FSServiceOrder.SrvOrdType = FSSrvOrdType.SrvOrdType
  WHERE FSServiceOrder.CauseID IS NOT NULL
  GROUP BY FSSrvOrdType.CompanyID, FSSrvOrdType.SrvOrdType
END
GO

--[IfExists(Table = FSCause)]
--[IfExists(Column = FSServiceOrder.CauseID)]
--[MinVersion(Hash = 45840c6c4d1c7999357b2cc1cc8e20f08069e95739a6f504844e79a0c725adbc)]
IF EXISTS(SELECT * FROM CSAttribute WHERE AttributeID = 'FSCAUSE')
AND NOT EXISTS(SELECT * FROM CSAnswers WHERE AttributeID = 'FSCAUSE')
BEGIN
  INSERT INTO [CSAnswers]
        ([CompanyID],[RefNoteID],[AttributeID],[Value],[NoteID])
  SELECT 
        FSServiceOrder.CompanyID,FSServiceOrder.NoteID,'FSCAUSE',FSCause.CauseCD,NEWID()
  FROM FSServiceOrder
  INNER JOIN FSCause ON FSCause.CompanyID = FSServiceOrder.CompanyID 
                    AND FSCause.CauseID = FSServiceOrder.CauseID
  WHERE FSServiceOrder.CauseID IS NOT NULL
  GROUP BY FSServiceOrder.CompanyID, FSServiceOrder.NoteID, FSCause.CauseCD
END
GO

--[IfExists(Column = FSAppointment.AdditionalCommentsCustomer)]
--[MinVersion(Hash = 918b1094956a491f449318fdb7ffcffdd3cc341aece139dc947c9dcd71f3b568)]
IF NOT EXISTS(select *  from CSAttribute where AttributeID = 'FSSIGCCOM')
BEGIN
  INSERT INTO [CSAttribute]
        ([CompanyID],[AttributeID],[Description],[ControlType],[EntryMask],[RegExp],[List],[IsInternal],[ContainsPersonalData],[CreatedByID]
        ,[CreatedByScreenID],[CreatedDateTime],[LastModifiedByID],[LastModifiedByScreenID],[LastModifiedDateTime],[NoteID])
  SELECT
        FSSrvOrdType.CompanyID,'FSSIGCCOM','Signature Customer Comments',1,NULL,NULL,NULL,0,0,MAX(FSSrvOrdType.CreatedByID)
        ,'CS205000',GETDATE(),MAX(FSSrvOrdType.CreatedByID),'CS205000',GETDATE(),NEWID()
  FROM FSSrvOrdType 
  INNER JOIN FSAppointment ON FSSrvOrdType.CompanyID  = FSAppointment.CompanyID 
                          AND FSSrvOrdType.SrvOrdType = FSAppointment.SrvOrdType
  WHERE  FSAppointment.AdditionalCommentsCustomer IS NOT NULL
  GROUP BY FSSrvOrdType.CompanyID
END
GO

--[IfExists(Column = FSAppointment.AdditionalCommentsCustomer)]
--[OldHash(Hash = 1672400d145056bccecf3507fa7ceeae6b8f06c3349e784863aaf52565fd1ac2)]
--[MinVersion(Hash = ce6897237708d04a2ad717b0340824561119609e3a60666556d0f441d80f02b0)]
IF EXISTS(SELECT *  FROM CSAttribute WHERE AttributeID = 'FSSIGCCOM')
AND NOT EXISTS(SELECT * FROM CSAttributeGroup WHERE AttributeID = 'FSSIGCCOM')
BEGIN
  INSERT INTO [CSAttributeGroup]
      ([CompanyID],[AttributeID],[EntityClassID],[EntityType],[SortOrder],[CreatedByID],[CreatedByScreenID],[CreatedDateTime],
    [LastModifiedByID],[LastModifiedByScreenID],[LastModifiedDateTime],[Required],[IsActive],[AttributeCategory])
  SELECT FSSrvOrdType.CompanyID,'FSSIGCCOM',FSSrvOrdType.SrvOrdType,'PX.Objects.FS.FSSrvOrdTypeAttributeList',0,MAX(FSSrvOrdType.CreatedByID),'FS202300',GETDATE(),
    MAX(FSSrvOrdType.CreatedByID),'FS202300',GETDATE(),0, 1,'A'
  FROM FSSrvOrdType
  INNER JOIN FSAppointment ON FSSrvOrdType.CompanyID  = FSAppointment.CompanyID 
                            AND FSSrvOrdType.SrvOrdType = FSAppointment.SrvOrdType
  WHERE  FSAppointment.AdditionalCommentsCustomer IS NOT NULL
  GROUP BY FSSrvOrdType.CompanyID, FSSrvOrdType.SrvOrdType
END
GO

--[IfExists(Column = FSAppointment.AdditionalCommentsCustomer)]
--[MinVersion(Hash = 864c35671f88e214fad749fc0a56a35545ec078244fedd9e44b01d2eed42b3ce)]
IF EXISTS(SELECT *  FROM CSAttribute WHERE AttributeID = 'FSSIGCCOM')
AND NOT EXISTS(SELECT * FROM CSAnswers WHERE AttributeID = 'FSSIGCCOM')
BEGIN
  INSERT INTO [CSAnswers]
          ([CompanyID],[RefNoteID],[AttributeID],[Value],[PseudonymizationStatus],[NoteID])
  SELECT CompanyID ,NoteID,'FSSIGCCOM',AdditionalCommentsCustomer,0, NEWID() 
  FROM FSAppointment
  WHERE AdditionalCommentsCustomer IS NOT NULL
END
GO

--[IfExists(Column = FSAppointment.AdditionalCommentsStaff)]
--[MinVersion(Hash = e4ddb4d9e16e9d3e88f7738de38f941fc5ec4b9cd828d15755be6010716bb1cb)]
IF NOT EXISTS(SELECT *  FROM CSAttribute WHERE AttributeID = 'FSSIGSCOM')
BEGIN
  INSERT INTO [CSAttribute]
        ([CompanyID],[AttributeID],[Description],[ControlType],[EntryMask],[RegExp],[List],[IsInternal],[ContainsPersonalData],[CreatedByID]
        ,[CreatedByScreenID],[CreatedDateTime],[LastModifiedByID],[LastModifiedByScreenID],[LastModifiedDateTime],[NoteID])
  SELECT
        FSSrvOrdType.CompanyID,'FSSIGSCOM','Signature Staff Comments',1,NULL,NULL,NULL,0,0,MAX(FSSrvOrdType.CreatedByID)
        ,'CS205000',GETDATE(),MAX(FSSrvOrdType.CreatedByID),'CS205000',GETDATE(),NEWID()
  FROM FSSrvOrdType 
  INNER JOIN FSAppointment ON FSSrvOrdType.CompanyID  = FSAppointment.CompanyID 
                          AND FSSrvOrdType.SrvOrdType = FSAppointment.SrvOrdType
  WHERE  FSAppointment.AdditionalCommentsStaff IS NOT NULL
  GROUP BY FSSrvOrdType.CompanyID
END
GO

--[IfExists(Column = FSAppointment.AdditionalCommentsStaff)]
--[OldHash(Hash = 9adcd5474f31c505169fe9ed78d0cec42303d3d74e93dfca8fca82f5f5c25556)]
--[MinVersion(Hash = c1bf69aa63d7259bb43daf9136d105256d598784e33acd0e59b2ce712f24dd70)]
IF EXISTS(SELECT *  FROM CSAttribute WHERE AttributeID = 'FSSIGSCOM')
AND NOT EXISTS(SELECT * FROM CSAttributeGroup WHERE AttributeID = 'FSSIGSCOM')
BEGIN
  INSERT INTO [CSAttributeGroup]
      ([CompanyID],[AttributeID],[EntityClassID],[EntityType],[SortOrder],[CreatedByID],[CreatedByScreenID],[CreatedDateTime],
    [LastModifiedByID],[LastModifiedByScreenID],[LastModifiedDateTime],[Required],[IsActive],[AttributeCategory])
  SELECT FSSrvOrdType.CompanyID,'FSSIGSCOM',FSSrvOrdType.SrvOrdType,'PX.Objects.FS.FSSrvOrdTypeAttributeList',0,MAX(FSSrvOrdType.CreatedByID),'FS202300',GETDATE(),
    MAX(FSSrvOrdType.CreatedByID),'FS202300',GETDATE(),0, 1, 'A'
  FROM FSSrvOrdType
  INNER JOIN FSAppointment ON FSSrvOrdType.CompanyID  = FSAppointment.CompanyID 
                            AND FSSrvOrdType.SrvOrdType = FSAppointment.SrvOrdType
  WHERE  FSAppointment.AdditionalCommentsStaff IS NOT NULL
  GROUP BY FSSrvOrdType.CompanyID, FSSrvOrdType.SrvOrdType
END
GO

--[IfExists(Column = FSAppointment.AdditionalCommentsStaff)]
--[MinVersion(Hash = 6e61f73f0475ea4d0876d5b1eba2fc7aef8c1c4515a1186e63fb69cc67fbd646)]
IF EXISTS(SELECT *  FROM CSAttribute WHERE AttributeID = 'FSSIGSCOM')
AND NOT EXISTS(SELECT * FROM CSAnswers WHERE AttributeID = 'FSSIGSCOM')
BEGIN
  INSERT INTO [CSAnswers]
          ([CompanyID],[RefNoteID],[AttributeID],[Value],[PseudonymizationStatus],[NoteID])
  SELECT CompanyID ,NoteID,'FSSIGSCOM',AdditionalCommentsStaff,0, NEWID() 
  FROM FSAppointment
  WHERE AdditionalCommentsStaff IS NOT NULL
END
GO

--[IfExists(Column = FSAppointment.AgreementSignature)]
--[MinVersion(Hash = 91d52a741b00e46f19d731cb611b6a0af3ba501268e7f1c0ae8c2505d196f1a0)]
IF NOT EXISTS(SELECT *  FROM CSAttribute WHERE AttributeID = 'FSSIGAGREE')
BEGIN
  INSERT INTO [CSAttribute]
        ([CompanyID],[AttributeID],[Description],[ControlType],[EntryMask],[RegExp],[List],[IsInternal],[ContainsPersonalData],[CreatedByID]
        ,[CreatedByScreenID],[CreatedDateTime],[LastModifiedByID],[LastModifiedByScreenID],[LastModifiedDateTime],[NoteID])
  SELECT
        FSSrvOrdType.CompanyID,'FSSIGAGREE','Signature Customer Completion Agreement',4,NULL,NULL,NULL,0,0,MAX(FSSrvOrdType.CreatedByID)
        ,'CS205000',GETDATE(),MAX(FSSrvOrdType.CreatedByID),'CS205000',GETDATE(),NEWID()
  FROM FSSrvOrdType 
  INNER JOIN FSAppointment ON FSSrvOrdType.CompanyID  = FSAppointment.CompanyID 
                          AND FSSrvOrdType.SrvOrdType = FSAppointment.SrvOrdType
  WHERE  FSAppointment.AgreementSignature = 1
  GROUP BY FSSrvOrdType.CompanyID
END
GO

--[IfExists(Column = FSAppointment.AgreementSignature)]
--[OldHash(Hash = cd3cb165b1106e47cc23c8cac059deb95c675f30a88a7368f11cc6ea76093515)]
--[MinVersion(Hash = 63541b6cfbd595be138f672fc66ed54eec001bf8b6e119c4c8448b200ea0ebab)]
IF EXISTS(SELECT *  FROM CSAttribute WHERE AttributeID = 'FSSIGAGREE')
AND NOT EXISTS(SELECT * FROM CSAttributeGroup WHERE AttributeID = 'FSSIGAGREE')
BEGIN
  INSERT INTO [CSAttributeGroup]
    ([CompanyID],[AttributeID],[EntityClassID],[EntityType],[SortOrder],[CreatedByID],[CreatedByScreenID],[CreatedDateTime],
  [LastModifiedByID],[LastModifiedByScreenID],[LastModifiedDateTime],[Required],[IsActive],[AttributeCategory])
  SELECT FSSrvOrdType.CompanyID,'FSSIGAGREE',FSSrvOrdType.SrvOrdType,'PX.Objects.FS.FSSrvOrdTypeAttributeList',0,MAX(FSSrvOrdType.CreatedByID),'FS202300',GETDATE(),
  MAX(FSSrvOrdType.CreatedByID),'FS202300',GETDATE(),0, 1, 'A'
  FROM FSSrvOrdType
  INNER JOIN FSAppointment ON FSSrvOrdType.CompanyID  = FSAppointment.CompanyID 
                          AND FSSrvOrdType.SrvOrdType = FSAppointment.SrvOrdType
  WHERE  FSAppointment.AgreementSignature = 1
  GROUP BY FSSrvOrdType.CompanyID, FSSrvOrdType.SrvOrdType
END
GO

--[IfExists(Column = FSAppointment.AgreementSignature)]
--[MinVersion(Hash = 222a1d62afd2ddcf35a707c599a428fc29eb3987814502183c0a7a2754300335)]
IF EXISTS(SELECT *  FROM CSAttribute WHERE AttributeID = 'FSSIGAGREE')
AND NOT EXISTS(SELECT * FROM CSAnswers WHERE AttributeID = 'FSSIGAGREE')
BEGIN
  INSERT INTO [CSAnswers]
          ([CompanyID],[RefNoteID],[AttributeID],[Value],[PseudonymizationStatus],[NoteID])
  SELECT CompanyID ,NoteID,'FSSIGAGREE',AgreementSignature,0, NEWID() 
  FROM FSAppointment
  WHERE AgreementSignature = 1
END
GO

--[IfExists(Table = FSResolution)]
--[IfExists(Column = FSServiceOrder.ResolutionID)]
--[MinVersion(Hash = 25f0aa733c519f57f4e5c215bde18bb70ca98fe7975a5c30fb14d11d3e63bf4d)]
IF NOT EXISTS(SELECT *  FROM CSAttribute WHERE AttributeID = 'FSRESOL')
BEGIN
  INSERT INTO [CSAttribute]
        ([CompanyID],[AttributeID],[Description],[ControlType],[EntryMask],[RegExp],[List],[IsInternal],[ContainsPersonalData],[CreatedByID]
        ,[CreatedByScreenID],[CreatedDateTime],[LastModifiedByID],[LastModifiedByScreenID],[LastModifiedDateTime],[NoteID])
  SELECT
        FSSrvOrdType.CompanyID,'FSRESOL','Resolution Code',2,NULL,NULL,NULL,0,0,MAX(FSSrvOrdType.CreatedByID)
        ,'CS205000',GETDATE(),MAX(FSSrvOrdType.CreatedByID),'CS205000',GETDATE(),NEWID()
  FROM FSSrvOrdType 
  GROUP BY FSSrvOrdType.CompanyID
END
GO

--[IfExists(Table = FSResolution)]
--[IfExists(Column = FSServiceOrder.ResolutionID)]
--[MinVersion(Hash = 97db39c0857bd805ed118fb84da3d8385c5544978baa826dad39081666bf5e9c)]
IF EXISTS(SELECT *  FROM CSAttribute WHERE AttributeID = 'FSRESOL')
AND NOT EXISTS(SELECT * FROM CSAttributeDetail WHERE AttributeID = 'FSRESOL')
BEGIN
  INSERT INTO [CSAttributeDetail]
       ([CompanyID],[AttributeID],[ValueID],[Description],[SortOrder],[Disabled],[CreatedByID]
       ,[CreatedByScreenID],[CreatedDateTime],[LastModifiedByID],[LastModifiedByScreenID],[LastModifiedDateTime],[NoteID])
  SELECT
      RS.CompanyID,'FSRESOL',LEFT(LTRIM(RS.ResolutionCD), 10),RS.Descr,NULL,0,RS.CreatedByID
      ,'CS205000',RS.CreatedDateTime,RS.LastModifiedByID,'CS205000',RS.LastModifiedDateTime, NewID()
  FROM FSResolution AS RS
  INNER JOIN CSAttribute ON RS.CompanyID  = CSAttribute.CompanyID 
  WHERE CSAttribute.AttributeID = 'FSRESOL'
END
GO

--[IfExists(Table = FSResolution)]
--[IfExists(Column = FSServiceOrder.ResolutionID)]
--[OldHash(Hash = 690a8a44a80d54c3eba9ba63f7ecf2c8832d19ccde99da948e3d68e3ee822ca7)]
--[MinVersion(Hash = 3d973a466a0d2a0196d0957b525dfb81b656fcdc35238cda25d7c05faa74a30c)]
IF EXISTS(SELECT *  FROM CSAttribute WHERE AttributeID = 'FSRESOL')
AND NOT EXISTS(SELECT * FROM CSAttributeGroup WHERE AttributeID = 'FSRESOL')
BEGIN
  INSERT INTO [CSAttributeGroup]
      ([CompanyID],[AttributeID],[EntityClassID],[EntityType],[SortOrder],[CreatedByID],[CreatedByScreenID],[CreatedDateTime]
      ,[LastModifiedByID],[LastModifiedByScreenID],[LastModifiedDateTime],[Required],[IsActive],[AttributeCategory])
  SELECT FSSrvOrdType.CompanyID,'FSRESOL',FSSrvOrdType.SrvOrdType,'PX.Objects.FS.FSSrvOrdTypeAttributeList',0,MAX(FSSrvOrdType.CreatedByID),'FS202300',GETDATE(),
    MAX(FSSrvOrdType.CreatedByID),'FS202300',GETDATE(),0, 1, 'A'
  FROM FSSrvOrdType
  INNER JOIN FSServiceOrder  ON FSServiceOrder.CompanyID  = FSSrvOrdType.CompanyID 
                            AND FSServiceOrder.SrvOrdType = FSSrvOrdType.SrvOrdType
  WHERE FSServiceOrder.ResolutionID IS NOT NULL
  GROUP BY FSSrvOrdType.CompanyID, FSSrvOrdType.SrvOrdType
END
GO

--[IfExists(Table = FSResolution)]
--[IfExists(Column = FSServiceOrder.ResolutionID)]
--[MinVersion(Hash = edbb5db829327d608144d187b5e4564feab983324743772fc53421725effb53f)]
IF EXISTS(SELECT *  FROM CSAttribute WHERE AttributeID = 'FSRESOL')
AND NOT EXISTS(SELECT * FROM CSAnswers WHERE AttributeID = 'FSRESOL')
BEGIN
  INSERT INTO [CSAnswers]
        ([CompanyID],[RefNoteID],[AttributeID],[Value],[PseudonymizationStatus],[NoteID])
  SELECT 
       FSServiceOrder.CompanyID,FSServiceOrder.NoteID,'FSRESOL',FSResolution.ResolutionCD,0,NEWID()
  FROM FSServiceOrder
  INNER JOIN FSResolution ON FSResolution.CompanyID = FSServiceOrder.CompanyID AND FSResolution.ResolutionID = FSServiceOrder.ResolutionID
  WHERE FSServiceOrder.ResolutionID IS NOT NULL
  GROUP BY FSServiceOrder.CompanyID, FSServiceOrder.NoteID, FSResolution.ResolutionCD
END
GO

--[IfExists(Column = FSServiceOrder.PromisedDate)]
--[MinVersion(Hash = 831f027bb4e76c222fe3dc76206907e3c94ef27f5dc0e0f94355a4161eae244e)]
IF NOT EXISTS(SELECT *  FROM CSAttribute WHERE AttributeID = 'FSPROMDATE')
BEGIN
  INSERT INTO [CSAttribute]
        ([CompanyID],[AttributeID],[Description],[ControlType],[EntryMask],[RegExp],[List],[IsInternal],[ContainsPersonalData],[CreatedByID]
        ,[CreatedByScreenID],[CreatedDateTime],[LastModifiedByID],[LastModifiedByScreenID],[LastModifiedDateTime],[NoteID])
  SELECT
        FSSrvOrdType.CompanyID,'FSPROMDATE','Promised Date',5,NULL,NULL,NULL,0,0,MAX(FSSrvOrdType.CreatedByID)
        ,'CS205000',GETDATE(),MAX(FSSrvOrdType.CreatedByID),'CS205000',GETDATE(),NEWID()
  FROM FSSrvOrdType 
  INNER JOIN FSServiceOrder ON FSSrvOrdType.CompanyID  = FSServiceOrder.CompanyID 
                          AND FSSrvOrdType.SrvOrdType = FSServiceOrder.SrvOrdType
  WHERE  FSServiceOrder.PromisedDate IS NOT NULL
  GROUP BY FSSrvOrdType.CompanyID
END
GO

--[IfExists(Column = FSServiceOrder.PromisedDate)]
--[OldHash(Hash = 0785eacaf8c469049b3521420d13d71fafd49b58a77679e8ae143fde934f209b)]
--[MinVersion(Hash = 43d4124dd956c45159ff706421d85518e35feb2303a7cd7f21c6fa7d85526438)]
IF EXISTS(SELECT *  FROM CSAttribute WHERE AttributeID = 'FSPROMDATE')
AND NOT EXISTS(SELECT * FROM CSAttributeGroup WHERE AttributeID = 'FSPROMDATE')
BEGIN
  INSERT INTO [CSAttributeGroup]
      ([CompanyID],[AttributeID],[EntityClassID],[EntityType],[SortOrder],[CreatedByID],[CreatedByScreenID],[CreatedDateTime],
    [LastModifiedByID],[LastModifiedByScreenID],[LastModifiedDateTime],[Required],[IsActive],[AttributeCategory])
  SELECT FSSrvOrdType.CompanyID,'FSPROMDATE',FSSrvOrdType.SrvOrdType,'PX.Objects.FS.FSSrvOrdTypeAttributeList',0,MAX(FSSrvOrdType.CreatedByID),'FS202300',GETDATE(),
    MAX(FSSrvOrdType.CreatedByID),'FS202300',GETDATE(),0, 1, 'A'
  FROM FSSrvOrdType
  INNER JOIN FSServiceOrder ON FSSrvOrdType.CompanyID  = FSServiceOrder.CompanyID 
                            AND FSSrvOrdType.SrvOrdType = FSServiceOrder.SrvOrdType
  WHERE  FSServiceOrder.PromisedDate IS NOT NULL
  GROUP BY FSSrvOrdType.CompanyID, FSSrvOrdType.SrvOrdType
END
GO

--[IfExists(Column = FSServiceOrder.PromisedDate)]
--[MinVersion(Hash = 15c0fc25e457b16d2c91ce7f64c208600196787ee9efdb0c2b95c9c3171feb85)]
IF EXISTS(SELECT *  FROM CSAttribute WHERE AttributeID = 'FSPROMDATE')
AND NOT EXISTS(SELECT * FROM CSAnswers WHERE AttributeID = 'FSPROMDATE')
BEGIN
  INSERT INTO [CSAnswers]
          ([CompanyID],[RefNoteID],[AttributeID],[Value],[PseudonymizationStatus],[NoteID])
  SELECT CompanyID ,NoteID,'FSPROMDATE',PromisedDate,0, NEWID() 
  FROM FSServiceOrder
  WHERE PromisedDate IS NOT NULL
END
GO

--[IfExists(Column = FSServiceOrder.ResolutionDate)]
--[MinVersion(Hash = b5c644008f6d885b1b6e8490969fa00bf9c76844e47a15b6d613cb28cbb882e4)]
IF NOT EXISTS(SELECT * FROM CSAttribute WHERE AttributeID = 'FSRESDATE')
BEGIN
  INSERT INTO [CSAttribute]
        ([CompanyID],[AttributeID],[Description],[ControlType],[EntryMask],[RegExp],[List],[IsInternal],[ContainsPersonalData],[CreatedByID]
        ,[CreatedByScreenID],[CreatedDateTime],[LastModifiedByID],[LastModifiedByScreenID],[LastModifiedDateTime],[NoteID])
  SELECT
        FSSrvOrdType.CompanyID,'FSRESDATE','Resolution Date',5,NULL,NULL,NULL,0,0,MAX(FSSrvOrdType.CreatedByID)
        ,'CS205000',GETDATE(),MAX(FSSrvOrdType.CreatedByID),'CS205000',GETDATE(),NEWID()
  FROM FSSrvOrdType 
  INNER JOIN FSServiceOrder ON FSSrvOrdType.CompanyID  = FSServiceOrder.CompanyID 
                            AND FSSrvOrdType.SrvOrdType = FSServiceOrder.SrvOrdType
  WHERE  FSServiceOrder.ResolutionDate IS NOT NULL
  GROUP BY FSSrvOrdType.CompanyID
END
GO

--[IfExists(Column = FSServiceOrder.ResolutionDate)]
--[OldHash(Hash = 26280fb8be472d5a67a0e9237becab9c8973d401cadad0e44dbb0ce56ebda42b)]
--[MinVersion(Hash = c6f0733685e291394807d3249e3f6561b37ab89a6c932c8a9d29366be471b34b)]
IF EXISTS(SELECT * FROM CSAttribute WHERE AttributeID = 'FSRESDATE')
AND NOT EXISTS(SELECT * FROM CSAttributeGroup WHERE AttributeID = 'FSRESDATE')
BEGIN
  INSERT INTO [CSAttributeGroup]
        ([CompanyID],[AttributeID],[EntityClassID],[EntityType],[SortOrder],[CreatedByID],[CreatedByScreenID],[CreatedDateTime]
        ,[LastModifiedByID],[LastModifiedByScreenID],[LastModifiedDateTime],[Required],[IsActive],[AttributeCategory])
  SELECT 
        FSSrvOrdType.CompanyID,'FSRESDATE',FSSrvOrdType.SrvOrdType,'PX.Objects.FS.FSSrvOrdTypeAttributeList',0
        ,MAX(FSSrvOrdType.CreatedByID),'FS202300',GETDATE(),MAX(FSSrvOrdType.CreatedByID),'FS202300',GETDATE(),0,1, 'A'
  FROM FSSrvOrdType
  INNER JOIN FSServiceOrder ON FSSrvOrdType.CompanyID  = FSServiceOrder.CompanyID 
                            AND FSSrvOrdType.SrvOrdType = FSServiceOrder.SrvOrdType
  WHERE FSServiceOrder.ResolutionDate IS NOT NULL
  GROUP BY FSSrvOrdType.CompanyID, FSSrvOrdType.SrvOrdType
END
GO

--[IfExists(Column = FSServiceOrder.ResolutionDate)]
--[MinVersion(Hash = e51f21a3570d3bd4f2d42dd496e32c49dfcbc27e1df039af99baa834e6287ce8)]
IF EXISTS(SELECT * FROM CSAttribute WHERE AttributeID = 'FSRESDATE')
AND NOT EXISTS(SELECT * FROM CSAnswers WHERE AttributeID = 'FSRESDATE')
BEGIN
  INSERT INTO [CSAnswers]
        ([CompanyID],[RefNoteID],[AttributeID],[Value],[PseudonymizationStatus],[NoteID])
  SELECT 
        CompanyID ,NoteID,'FSRESDATE',ResolutionDate,0,NEWID()
  FROM FSServiceOrder
  WHERE ResolutionDate IS NOT NULL
END
GO

--[IfExistsSelect(From = FSAppointmentEmployee)]
--[IfExists(Column = FSSetup.EnableDfltStaffOnServiceOrder)]
--[IfExistsSelect(From = FSSetup, WhereIsNull = EnableDfltStaffOnServiceOrder)]
--[MinVersion(Hash = 7cc7bf89a0f2ae9b09a09959972bfb9336cddb320dcb1bb384f7529b38ce6955)]
UPDATE FSSetup SET EnableDfltStaffOnServiceOrder = 1
GO

--[IfExistsSelect(From = FSAppointmentResource)]
--[IfExists(Column = FSSetup.EnableDfltResEquipOnServiceOrder)]
--[IfExistsSelect(From = FSSetup, WhereIsNull = EnableDfltResEquipOnServiceOrder)]
--[MinVersion(Hash = 3492dbcc293c344840dd44f5db2eec4c7f86f95aad163ae4602d68b431a98076)]
UPDATE FSSetup SET EnableDfltResEquipOnServiceOrder = 1
GO

--[IfExists(Column = FSScheduleDet.SortOrder)]
--[MinVersion(Hash = 90eec2c7675554f4d17fcb1c6f2d5357d46cabdc1a18409a6ea06ca5e59be194)]
UPDATE FSScheduleDet SET SortOrder = LineNbr WHERE SortOrder IS NULL
GO

--[IfExists(Column = FSAppointmentDet.ApptLineNbr)]
--[IfExists(Column = FSAppointmentDet.LineNbr)]
--[MinVersion(Hash = 47254d105dcd121a5186fb6c7bbed590c34ba3b4943282e5e87a4c809372aa02)]
UPDATE FSAppointmentDet SET LineNbr = ApptLineNbr WHERE LineNbr IS NULL;

--[IfExists(Column = FSAppointmentDet.SortOrder)]
--[IfExists(Column = FSAppointmentDet.LineNbr)]
UPDATE FSAppointmentDet SET SortOrder = LineNbr WHERE SortOrder IS NULL
GO

--[IfExists(Column = FSSODet.SortOrder)]
--[IfExists(Column = FSSODet.LineNbr)]
--[MinVersion(Hash = 08792d6645ac0570acdc46cfee518b69d2db3d2af1619c90fc7dca4411ca9f15)]
UPDATE FSSODet SET SortOrder = LineNbr WHERE SortOrder IS NULL
GO

--[IfExists(Column = FSSetup.DayResolution)]
--[MinVersion(Hash = e0899d984c1e1012dede97fc1a01d69c463ac928a625fa0bc8d15b37aabc3394)]
UPDATE FSSetup SET
  DayResolution = 14
WHERE
  DayResolution < 14
GO

--[IfExists(Column = FSSetup.WeekResolution)]
--[MinVersion(Hash = 3425520a551f07c2099ad61c20823391c33c845657e9cc5db41ab68d1c34576d)]
UPDATE FSSetup SET
  WeekResolution = 12
WHERE
  WeekResolution < 12
GO

--[IfExists(Column = FSSetup.MonthResolution)]
--[MinVersion(Hash = 58fb7644e1a5b0493aa3131c3fe4f1a97df9980049cbac7f1e8a495e569ab87d)]
UPDATE FSSetup SET
  MonthResolution = 10
WHERE
  MonthResolution < 10
GO

--[IfExists(Column = FSSODet.CuryBillableExtPrice)]
--[IfExistsSelect(From = FSSODet, WhereIsNull = CuryBillableExtPrice)]
--[MinVersion(Hash = c5c25f96150bae50cd0100a5d9f2d796ee3b86ffb251f320b5aaad934d7a6786)]
UPDATE FSSODet
SET 
  FSSODet.CuryBillableExtPrice = FSSODet.CuryBillableTranAmt,
  FSSODet.BillableExtPrice = FSSODet.BillableTranAmt,
  FSSODet.CuryDiscAmt = 0,
  FSSODet.DiscAmt = 0,
  FSSODet.DiscPct = 0,
  FSSODet.ManualDisc = 0
WHERE FSSODet.CuryBillableExtPrice IS NULL;
GO

--[IfExists(Column = FSAppointmentDet.CuryBillableExtPrice)]
--[IfExistsSelect(From = FSAppointmentDet, WhereIsNull = CuryBillableExtPrice)]
--[MinVersion(Hash = d143a7bdb5148fd88f7e899159556df1c1acc20a423ce1476d37c8fe88465712)]
UPDATE FSAppointmentDet
SET 
  FSAppointmentDet.CuryBillableExtPrice = FSAppointmentDet.CuryBillableTranAmt,
  FSAppointmentDet.BillableExtPrice = FSAppointmentDet.BillableTranAmt,
  FSAppointmentDet.CuryDiscAmt = 0,
  FSAppointmentDet.DiscAmt = 0,
  FSAppointmentDet.DiscPct = 0,
  FSAppointmentDet.ManualDisc = 0
WHERE FSAppointmentDet.CuryBillableExtPrice IS NULL;
GO

--[MinVersion(Hash = 1b15a17864a382d017bbb790923955c65ac664afab1bb4dd08ebf02c3811d405)]
UPDATE FSServiceOrder SET
  SourceType = 'SD'
WHERE
  SourceType = 'QU'
GO

--[IfExists(Column = FSServiceContract.ProjectID)]
--[MinVersion(Hash = 82376d5baa5b6317b635d7a915c10c0253fe1f6798c9979dc016968b188b9b9d)]
UPDATE FSServiceContract SET ProjectID = ISNUll(Contract.ContractID, 0)
FROM FSServiceContract
LEFT JOIN Contract ON FSServiceContract.CompanyID = Contract.CompanyID AND Contract.BaseType='P' AND Contract.NonProject = 1
WHERE FSServiceContract.ProjectID IS NULL

--[IfExists(Column = FSContractPeriodDet.ProjectID)]
UPDATE FSContractPeriodDet SET ProjectID = ISNUll(Contract.ContractID, 0)
FROM FSContractPeriodDet
LEFT JOIN Contract ON FSContractPeriodDet.CompanyID = Contract.CompanyID AND Contract.BaseType='P' AND Contract.NonProject = 1
WHERE FSContractPeriodDet.ProjectID IS NULL

--[IfExists(Column = FSSchedule.ProjectID)]
UPDATE FSSchedule SET ProjectID = ISNUll(Contract.ContractID, 0)
FROM FSSchedule
LEFT JOIN Contract ON FSSchedule.CompanyID = Contract.CompanyID AND Contract.BaseType='P' AND Contract.NonProject = 1
WHERE FSSchedule.ProjectID IS NULL

--[IfExists(Column = FSScheduleDet.ProjectID)]
UPDATE FSScheduleDet SET ProjectID = ISNUll(Contract.ContractID, 0)
FROM FSScheduleDet
LEFT JOIN Contract ON FSScheduleDet.CompanyID = Contract.CompanyID AND Contract.BaseType='P' AND Contract.NonProject = 1
WHERE FSScheduleDet.ProjectID IS NULL
GO

--[MinVersion(Hash = 94e4f15f6a48dca38aa83a3341dde195c6ee4dc8fe5eb3fe04f904ba6b8601f7)]
UPDATE FSPostDoc SET PostDocType = RTRIM(PostDocType)
GO

--[MinVersion(Hash = d55322565a633e052a9ab658872dc71480f3fca86cbc3bd930351690265a2fa0)]
UPDATE FSServiceOrder
SET ApptDurationTotal = ISNULL((SELECT SUM(FSSODet.ApptDuration) 
               FROM FSSODet
             WHERE FSSODet.CompanyID = FSServiceOrder.CompanyID  AND 
              FSSODet.SrvOrdType = FSServiceOrder.SrvOrdType AND 
              FSSODet.RefNbr = FSServiceOrder.RefNbr AND
              FSSODet.LineType = 'SERVI'
             GROUP BY FSSODet.CompanyID, FSSODet.SrvOrdType, FSSODet.RefNbr), 0)
GO

--[MinVersion(Hash = f4065fb8929442ca942c4c6698105a175a17ec55a8715b53c517ec21fc3907b9)]
UPDATE FSServiceOrder SET
  ServiceOrderAddressID = FSAddress.AddressID
FROM FSServiceOrder
  INNER JOIN FSAddress ON (FSAddress.CompanyID = FSServiceOrder.CompanyID
                AND FSAddress.EntityID = FSServiceOrder.SOID
                AND FSAddress.EntityType = 'SROR')
WHERE
  ServiceOrderAddressID <> FSAddress.AddressID

UPDATE FSServiceOrder SET
  ServiceOrderContactID = FSContact.ContactID
FROM FSServiceOrder
  INNER JOIN FSContact ON (FSContact.CompanyID = FSServiceOrder.CompanyID
                AND FSContact.EntityID = FSServiceOrder.SOID
                AND FSContact.EntityType = 'SROR')
WHERE
  ServiceOrderContactID <> FSContact.ContactID

UPDATE FSBranchLocation SET
  BranchLocationContactID = FSContact.ContactID
FROM FSBranchLocation
  INNER JOIN FSContact ON (FSContact.CompanyID = FSBranchLocation.CompanyID
                AND FSContact.EntityID = FSBranchLocation.BranchLocationID
                AND FSContact.EntityType = 'BLOC')
WHERE
  BranchLocationContactID <> FSContact.ContactID

UPDATE FSBranchLocation SET
  BranchLocationAddressID = FSAddress.AddressID
FROM FSBranchLocation
  INNER JOIN FSAddress ON (FSAddress.CompanyID = FSBranchLocation.CompanyID
                AND FSAddress.EntityID = FSBranchLocation.BranchLocationID
                AND FSAddress.EntityType = 'BLOC')
WHERE
  BranchLocationAddressID <> FSAddress.AddressID

UPDATE FSManufacturer SET
  ManufacturerContactID = FSContact.ContactID
FROM FSManufacturer
  INNER JOIN FSContact ON (FSContact.CompanyID = FSManufacturer.CompanyID
                AND FSContact.EntityID = FSManufacturer.ManufacturerID
                AND FSContact.EntityType = 'MNFC')
WHERE
  ManufacturerContactID <> FSContact.ContactID

UPDATE FSManufacturer SET
  ManufacturerAddressID = FSAddress.AddressID
FROM FSManufacturer
  INNER JOIN FSAddress ON (FSAddress.CompanyID = FSManufacturer.CompanyID
                AND FSAddress.EntityID = FSManufacturer.ManufacturerID
                AND FSAddress.EntityType = 'MNFC')
WHERE
  ManufacturerAddressID <> FSAddress.AddressID
GO

--[MinVersion(Hash = 70210d56844ac1965c777a1e25653e1479e113133b86d5565d9b5a58a2c5b27c)]
UPDATE NumberingSequence SET 
  WarnNbr = 'FSC99999899'
WHERE 
  WarnNbr = 'FCT99999899' AND NumberingID = 'FSSCHEDULE';
GO

--[OldHash(Hash = aa333c28fb9417e22e9588e86138b2ae5a7b90ece06a13d4a2440d433de6fb59)]
--[MinVersion(Hash = 707e6aefedd1f35d75b77691505c36e9451a742be47eec6d57503218704ed786)]
INSERT INTO [CSAttributeGroup] 
     ([CompanyID],[AttributeID],[EntityClassID],[EntityType],[SortOrder],[CreatedByID],[CreatedByScreenID],
      [CreatedDateTime],[LastModifiedByID],[LastModifiedByScreenID],[LastModifiedDateTime],[DefaultValue],[Required],[IsActive],[AttributeCategory])
SELECT  [CompanyID],[AttributeID],[EntityClassID],'PX.Objects.FS.FSAppointment',[SortOrder],[CreatedByID],[CreatedByScreenID],
          [CreatedDateTime],[LastModifiedByID],[LastModifiedByScreenID],[LastModifiedDateTime],[DefaultValue],[Required],[IsActive],[AttributeCategory]
FROM CSAttributeGroup
WHERE EntityType = 'PX.Objects.FS.FSSrvOrdTypeAttributeList'
GO

--[OldHash(Hash = 54bf1c74919c692c926035543031b51fcbb95247253eb972c408e9635e9cad98)]
--[MinVersion(Hash = 4c5074e904edc1fcbcd8792634d0acbccff48651564ec59da687eda89f76d9a3)]
INSERT INTO [CSAttributeGroup] 
     ([CompanyID],[AttributeID],[EntityClassID],[EntityType],[SortOrder],[CreatedByID],[CreatedByScreenID],
      [CreatedDateTime],[LastModifiedByID],[LastModifiedByScreenID],[LastModifiedDateTime],[DefaultValue],[Required],[IsActive],[AttributeCategory])
SELECT  [CompanyID],[AttributeID],[EntityClassID],'PX.Objects.FS.FSServiceOrder',[SortOrder],[CreatedByID],[CreatedByScreenID],
          [CreatedDateTime],[LastModifiedByID],[LastModifiedByScreenID],[LastModifiedDateTime],[DefaultValue],[Required],[IsActive],[AttributeCategory]
FROM CSAttributeGroup
WHERE EntityType = 'PX.Objects.FS.FSSrvOrdTypeAttributeList'
GO

--[OldHash(Hash = 703b864145bb811c4806e8f5ca84dd1386426885b01014ddc45bdd869af75421)]
--[MinVersion(Hash = e7cb0ee414ab69558d8c86b5a608a8610d8a5090604b30eec9cdd7e0be9a507f)]
INSERT INTO [CSAttributeGroup] 
     ([CompanyID],[AttributeID],[EntityClassID],[EntityType],[SortOrder],[CreatedByID],[CreatedByScreenID],
      [CreatedDateTime],[LastModifiedByID],[LastModifiedByScreenID],[LastModifiedDateTime],[DefaultValue],[Required],[IsActive],[AttributeCategory])
SELECT  [CompanyID],[AttributeID],[EntityClassID],'PX.Objects.FS.FSSchedule',[SortOrder],[CreatedByID],[CreatedByScreenID],
          [CreatedDateTime],[LastModifiedByID],[LastModifiedByScreenID],[LastModifiedDateTime],[DefaultValue],[Required],[IsActive],[AttributeCategory]
FROM CSAttributeGroup
WHERE EntityType = 'PX.Objects.FS.FSSrvOrdTypeAttributeList'
GO

--[MinVersion(Hash = d603e2d45f3c8ecae1e9bbc9239fec69686111ee1ed1d4b439df76acf9e51065)]
DELETE FROM CSAttributeGroup
WHERE EntityType = 'PX.Objects.FS.FSSrvOrdTypeAttributeList'
GO

--[mysql: Skip]
--[mssql: Native]
--[IfExistsSelect(From = Locale)]
--[MinVersion(Hash = 3e5ec96e05ae8e59f097db97a74c83f06fe3cbc77c88c54ea0f136976c391a24)]
IF EXISTS(
  SELECT * 
  FROM CSAttribute
  WHERE 
    NOT NoteID IN (SELECT RecordID FROM CSAttributeKvExt) 
  AND 
    AttributeID IN ('FSRESDATE','FSPROMDATE', 'FSRESOL', 'FSSIGAGREE', 'FSSIGCCOM', 'FSSIGSCOM', 'FSCAUSE')
)
BEGIN
  INSERT INTO [CSAttributeKvExt]
           ([CompanyID]
           ,[RecordID]
           ,[FieldName]
           ,[ValueNumeric]
           ,[ValueDate]
           ,[ValueString]
           ,[ValueText])
  SELECT
    CSAttribute.CompanyID, 
    CSAttribute.NoteID, 
    CONCAT('Description', (SELECT TOP(1) UPPER(LEFT(LocaleName, CHARINDEX('-', REVERSE(LocaleName)) - 1)) FROM Locale WHERE IsDefault = 1)), 
    NULL, 
    NULL, 
    CSAttribute.Description, 
    NULL
  FROM 
    CSAttribute 
  WHERE 
    NOT NoteID IN (SELECT RecordID FROM CSAttributeKvExt) 
  AND 
    AttributeID IN ('FSRESDATE','FSPROMDATE', 'FSRESOL', 'FSSIGAGREE', 'FSSIGCCOM', 'FSSIGSCOM', 'FSCAUSE')  
END
GO

--[mysql: Skip]
--[mssql: Native]
--[IfExistsSelect(From = Locale)]
--[MinVersion(Hash = 7085c828f14baa7b6f8a06c8820bb7a401ea8d6dd0202092bd358b30f172f501)]
IF EXISTS(
  SELECT * 
  FROM CSAttributeDetail
  WHERE 
    NOT NoteID IN (SELECT RecordID FROM CSAttributeDetailKvExt) 
  AND 
    AttributeID IN ('FSRESDATE','FSPROMDATE', 'FSRESOL', 'FSSIGAGREE', 'FSSIGCCOM', 'FSSIGSCOM', 'FSCAUSE')
)
BEGIN
  INSERT INTO [CSAttributeDetailKvExt]
           ([CompanyID]
           ,[RecordID]
           ,[FieldName]
           ,[ValueNumeric]
           ,[ValueDate]
           ,[ValueString]
           ,[ValueText])
  SELECT
    CSAttributeDetail.CompanyID, 
    CSAttributeDetail.NoteID, 
    CONCAT('Description', (SELECT TOP(1) UPPER(LEFT(LocaleName, CHARINDEX('-', REVERSE(LocaleName)) - 1)) FROM Locale WHERE IsDefault = 1)), 
    NULL, 
    NULL, 
    CSAttributeDetail.Description, 
    NULL
  FROM 
    CSAttributeDetail 
  WHERE 
    NOT NoteID IN (SELECT RecordID FROM CSAttributeDetailKvExt) 
  AND 
    AttributeID IN ('FSRESDATE','FSPROMDATE', 'FSRESOL', 'FSSIGAGREE', 'FSSIGCCOM', 'FSSIGSCOM', 'FSCAUSE')  
END
GO

--[mssql: Skip]
--[mysql: Native]
--[IfExistsSelect(From = Locale)]
--[MinVersion(Hash = 033d123058bd0aa1e8994ff8b772ae3a2cb6726900b74f23d707c3802825a30c)]
INSERT INTO CSAttributeKvExt
         (CompanyID
         ,RecordID
         ,FieldName
         ,ValueNumeric
         ,ValueDate
         ,ValueString
         ,ValueText)
SELECT
  CSAttribute.CompanyID, 
  CSAttribute.NoteID, 
  CONCAT('Description', (SELECT UPPER(LEFT(LocaleName,LOCATE('-',LocaleName) - 1)) FROM Locale WHERE IsDefault = 1 LIMIT 1)), 
  NULL, 
  NULL, 
  CSAttribute.Description, 
  NULL
FROM 
  CSAttribute 
WHERE 
  NOT NoteID IN (SELECT RecordID FROM CSAttributeKvExt) 
AND 
  AttributeID IN ('FSRESDATE','FSPROMDATE', 'FSRESOL', 'FSSIGAGREE', 'FSSIGCCOM', 'FSSIGSCOM', 'FSCAUSE')
AND
  EXISTS(SELECT LocaleName FROM Locale WHERE IsDefault = 1 LIMIT 1)
GO

--[mssql: Skip]
--[mysql: Native]
--[IfExistsSelect(From = Locale)]
--[MinVersion(Hash = 95e68803fa08596d83e04bbf848599551ad3272ddccc7b28cf6cd22fde929d1e)]
INSERT INTO CSAttributeDetailKvExt
         (CompanyID
         ,RecordID
         ,FieldName
         ,ValueNumeric
         ,ValueDate
         ,ValueString
         ,ValueText)
SELECT
  CSAttributeDetail.CompanyID, 
  CSAttributeDetail.NoteID, 
  CONCAT('Description', (SELECT UPPER(LEFT(LocaleName,LOCATE('-',LocaleName) - 1)) FROM Locale WHERE IsDefault = 1 LIMIT 1)), 
  NULL, 
  NULL, 
  CSAttributeDetail.Description, 
  NULL
FROM 
  CSAttributeDetail 
WHERE 
  NOT NoteID IN (SELECT RecordID FROM CSAttributeDetailKvExt) 
AND 
  AttributeID IN ('FSRESDATE','FSPROMDATE', 'FSRESOL', 'FSSIGAGREE', 'FSSIGCCOM', 'FSSIGSCOM', 'FSCAUSE')
AND
  EXISTS(SELECT LocaleName FROM Locale WHERE IsDefault = 1 LIMIT 1)
GO

--[IfExistsSelect(From = FSServiceOrder)]
--[MinVersion(Hash = c1db552988e9c51b1268ceba0f57d5cb40ec4b3a7a0333f9910635202c69b605)]
update FSServiceOrder set
	FSServiceOrder.AllowInvoice = 0
from FSServiceOrder
	left outer join FSCustomerBillingSetup on (FSCustomerBillingSetup.CompanyID = FSServiceOrder.CompanyID
					and FSCustomerBillingSetup.CBID = FSServiceOrder.CBID)
	left outer join FSBillingCycle on (FSBillingCycle.CompanyID = FSServiceOrder.CompanyID
					and FSBillingCycle.BillingCycleID = FSCustomerBillingSetup.BillingCycleID)
where
	FSServiceOrder.AllowInvoice = 1
	and (FSBillingCycle.BillingBy IS NULL OR FSBillingCycle.BillingBy = 'AP')
GO

--[MinVersion(Hash = 49d9ea7ec90e5757c0d20cd917d7a3e24d0d63425038e91f7f376e2b59dfb8b1)]
UPDATE FSSODet
SET UOM = (SELECT ISNULL(WeightUOM,'EACH') FROM CommonSetup WHERE CompanyID = FSSODet.CompanyID)
WHERE 
UOM IS NULL AND
(
  LineType = 'CM_PT' OR
  LineType = 'IT_PT' OR
  LineType = 'CM_SV' OR
  LineType = 'IT_SV'
);
GO

--[MinVersion(Hash = 25962c7c2ba6a70a842b3f32769f1863366806feeb82118d9d41096860207afb)]
UPDATE FSAppointmentDet
SET UOM = (SELECT ISNULL(WeightUOM,'EACH') FROM CommonSetup WHERE CompanyID = FSAppointmentDet.CompanyID)
WHERE 
UOM IS NULL AND
(
  LineType = 'CM_PT' OR
  LineType = 'IT_PT' OR
  LineType = 'CM_SV' OR
  LineType = 'IT_SV'
);
GO

--[IfExistsSelect(From = FSServiceOrder)]
--[MinVersion(Hash = c1db552988e9c51b1268ceba0f57d5cb40ec4b3a7a0333f9910635202c69b605)]
update FSServiceOrder set
	FSServiceOrder.AllowInvoice = 0
from FSServiceOrder
	left outer join FSCustomerBillingSetup on (FSCustomerBillingSetup.CompanyID = FSServiceOrder.CompanyID
					and FSCustomerBillingSetup.CBID = FSServiceOrder.CBID)
	left outer join FSBillingCycle on (FSBillingCycle.CompanyID = FSServiceOrder.CompanyID
					and FSBillingCycle.BillingCycleID = FSCustomerBillingSetup.BillingCycleID)
where
	FSServiceOrder.AllowInvoice = 1
	and (FSBillingCycle.BillingBy IS NULL OR FSBillingCycle.BillingBy = 'AP')
GO

--[MinVersion(Hash = a77bdf8357909341afb0d5d933ed61258821913785e1262d89e17ef6845904eb)]
UPDATE FSAppointmentDet
SET LineType = CASE WHEN FSAppointmentDet.LineType = 'CM_SV' OR FSAppointmentDet.LineType = 'CM_PT' THEN 'CM_LN'
          WHEN FSAppointmentDet.LineType = 'IT_SV' OR FSAppointmentDet.LineType = 'IT_PT' THEN 'IT_LN'
          ELSE FSAppointmentDet.LineType
        END;

UPDATE FSSODet
SET LineType = CASE WHEN FSSODet.LineType = 'CM_SV' OR FSSODet.LineType = 'CM_PT' THEN 'CM_LN'
          WHEN FSSODet.LineType = 'IT_SV' OR FSSODet.LineType = 'IT_PT' THEN 'IT_LN'
          ELSE FSSODet.LineType
        END;

UPDATE FSScheduleDet
SET LineType = CASE WHEN FSScheduleDet.LineType = 'CM_SV' OR FSScheduleDet.LineType = 'CM_PT' THEN 'CM_LN'
          WHEN FSScheduleDet.LineType = 'IT_SV' OR FSScheduleDet.LineType = 'IT_PT' THEN 'IT_LN'
          ELSE FSScheduleDet.LineType
        END
UPDATE FSServiceTemplateDet
SET LineType = CASE WHEN FSServiceTemplateDet.LineType = 'CM_SV' OR FSServiceTemplateDet.LineType = 'CM_PT' THEN 'CM_LN'
          WHEN FSServiceTemplateDet.LineType = 'IT_SV' OR FSServiceTemplateDet.LineType = 'IT_PT' THEN 'IT_LN'
          ELSE FSServiceTemplateDet.LineType
        END;
GO

--[mysql: Native]
--[mssql: Skip]
--[IfExists(Column = FSAppointmentDet.LineNbr)]
--[MinVersion(Hash = cf1bd581623c01c5f661fcf706ebe75c185619b83b7d37765558299a88e90270)]
UPDATE FSAppointmentDet AS Det
  INNER JOIN (SELECT count(*) AS RowCountNbr, A.AppointmentID, A.AppDetID, A.CompanyID FROM FSAppointmentDet A 
          INNER JOIN FSAppointmentDet B ON A.CompanyID = B.CompanyID
                        AND A.AppointmentID = B.AppointmentID
                        AND A.AppDetID >= B.AppDetID
          GROUP BY A.CompanyID, A.AppointmentID, A.AppDetID) AS SelectCount ON
            SelectCount.AppointmentID = Det.AppointmentID 
            AND SelectCount.AppDetID = Det.AppDetID
            AND SelectCount.CompanyID = Det.CompanyID
  SET Det.LineNbr = SelectCount.RowCountNbr
GO

--[mysql: Skip]
--[mssql: Native]
--[IfExists(Column = FSAppointmentDet.LineNbr)]
--[MinVersion(Hash = 6ceb66bb29388f7730609606f9acb97a352603926da49877cc3365b797fa9f97)]
UPDATE Det
  SET LineNbr = (SELECT count(*) FROM FSAppointmentDet A 
          INNER JOIN FSAppointmentDet B ON A.CompanyID = B.CompanyID
                        AND A.AppointmentID = B.AppointmentID
                        AND A.AppDetID >= B.AppDetID
          WHERE A.AppointmentID = Det.AppointmentID 
            AND A.AppDetID = Det.AppDetID
            AND A.CompanyID = Det.CompanyID
          GROUP BY A.CompanyID, A.AppointmentID, A.AppDetID)
FROM FSAppointmentDet Det
GO

--[IfExists(Column = FSAppointmentDet.LineNbr)]
--[IfExists(Column = FSAppointment.LineCntr)]
--[MinVersion(Hash = e30b87773616746784667a2d28dfb9f3115f662c1f0b0128cc439403ec62c541)]
UPDATE FSAppointment
  SET LineCntr = ISNULL((SELECT MAX(LineNbr)
                    FROM  FSAppointmentDet
                    WHERE FSAppointmentDet.SrvOrdType = FSAppointment.SrvOrdType
                        AND FSAppointmentDet.RefNbr = FSAppointment.RefNbr
                        AND FSAppointmentDet.CompanyID = FSAppointment.CompanyID), 0)
FROM FSAppointment
GO

--[IfExists(Column = FSAppointmentDet.OldLineRef)]
--[MinVersion(Hash = 9c7c3f05682d45d60719ec1a5c968e0ff7ee6dd7f5932e91bb38a640ed454998)]
UPDATE FSAppointmentDet 
SET OldLineRef = LineRef WHERE OldLineRef IS NULL;

UPDATE FSAppointmentDet 
SET LineRef = RIGHT('0000' + CAST(LineNbr AS VARCHAR(4)), 4);

UPDATE FSAppointmentEmployee SET ServiceLineRef = FSAppointmentDet.LineRef
FROM FSAppointmentEmployee 
INNER JOIN FSAppointmentDet 
      ON FSAppointmentDet.OldLineRef = FSAppointmentEmployee.ServiceLineRef 
      AND FSAppointmentDet.CompanyID = FSAppointmentEmployee.CompanyID 
      AND FSAppointmentEmployee.AppointmentID = FSAppointmentDet.AppointmentID
WHERE
FSAppointmentEmployee.ServiceLineRef IS NOT NULL;

UPDATE t1  SET t1.NewTargetEquipmentLineNbr = t2.LineRef
FROM FSAppointmentDet t1
INNER JOIN FSAppointmentDet t2 
    ON t2.NewTargetEquipmentLineNbr = t1.OldLineRef 
    AND t2.CompanyID = t1.CompanyID 
    AND t2.AppointmentID = t1.AppointmentID
WHERE 
t1.NewTargetEquipmentLineNbr IS NOT NULL
GO

--[mysql: Skip]
--[mssql: Native]
--[IfExists(Column = FSAppointmentDet.SODetID)]
--[MinVersion(Hash = 0dc1825ce95fbf57f377c4b993cf69a3a625047662af616308a6d860c2a59f32)]
ALTER TABLE FSAppointmentDet ALTER COLUMN SODetID INT NULL;
GO

--[mysql: Native]
--[mssql: Skip]
--[IfExists(Column = FSAppointmentDet.SODetID)]
--[MinVersion(Hash = fc80c7dfad3a26d7457bd87d184bb0b1b3c022ae327b4d4f65d09d35cd79612c)]
ALTER TABLE FSAppointmentDet MODIFY COLUMN SODetID INT NULL;
GO

--[IfExists(Column = FSAppointmentDet.OldPickupSODetID)]
--[MinVersion(Hash = 576f619bd6b80abe801916f22ec485bd9431ee132642f04aaf8811009c7dc91f)]
UPDATE FSAppointmentDet SET OldPickupSODetID = SODetID WHERE OldPickupSODetID IS NULL AND LineType = 'PU_DL'

UPDATE FSAppointmentDet SET SODetID = Null WHERE LineType = 'PU_DL'
GO

--[IfExists(Column = FSAppointmentDet.PickupDeliveryAppLineRef)]
--[IfExists(Column = FSAppointmentDet.OldPickupSODetID)]
--[MinVersion(Hash = 353a83cfe8ed414788239f48781d8e239175310eb409ca8a4f01a2bdd8200a69)]
UPDATE t1 
SET t1.PickupDeliveryAppLineRef = t2.LineRef
FROM FSAppointmentDet AS t1
INNER JOIN FSAppointmentDet AS t2 
ON t2.SODetID = t1.OldPickupSODetID 
  AND t2.CompanyID = t1.CompanyID 
  AND t2.AppointmentID = t1.AppointmentID
  AND t2.LineType = 'SERVI'
WHERE t1.LineType = 'PU_DL' AND t1.PickupDeliveryAppLineRef IS NULL
GO

--[MinVersion(Hash = a5388b6f37bf934582fc8d3b079525286020090ceb14d7255f7bfe351eda84e9)]
UPDATE FSScheduleDet 
SET BillingRule = 'FLRA' 
WHERE LineType = 'SLPRO' AND BillingRule = 'NONE';
GO

--[IfExists(Column = FSxPMTimeActivity.OldAppEmpID)]
--[IfExists(Column = FSxPMTimeActivity.AppEmpID)]
--[MinVersion(Hash = df6ebd64d22bb0682347380946868e212b261aa8f982777c0e6e1460821c1c65)]
UPDATE FSxPMTimeActivity 
  SET OldAppEmpID = AppEmpID
WHERE OldAppEmpID IS NULL;
GO

--[IfExists(Table = FSLog)]
--[IfExists(Table = Note)]
--[IfExists(Column = FSAppointmentEmployee.ActualDateTimeBegin)]
--[IfExists(Column = FSAppointmentEmployee.CuryUnitCost)]
--[MinVersion(Hash = 9880749b28f1981a73d845dffc87123fb835caa65945793657cbad4696eeea04)]
INSERT INTO FSLog
  (CompanyID
  ,DateTimeBegin
  ,DateTimeEnd
  ,TimeDuration
  ,ApprovedTime
  ,CostCodeID
  ,CuryExtCost
  ,CuryInfoID
  ,CuryUnitCost
  ,DetLineRef
  ,DocID
  ,DocRefNbr
  ,DocType
  ,EarningType
  ,BAccountID
  ,BAccountType
  ,ExtCost
  ,KeepDateTimes
  ,LaborItemID
  ,LineRef
  ,LineNbr
  ,OldFSAppointmentEmployeeLineNbr
  ,ProjectID
  ,ProjectTaskID
  ,Status
  ,TimeCardCD
  ,TrackOnService
  ,TrackTime
  ,UnitCost
  ,Type
  ,CreatedByID
  ,CreatedByScreenID
  ,CreatedDateTime
  ,LastModifiedByID
  ,LastModifiedByScreenID
  ,LastModifiedDateTime
  ,NoteID)
SELECT
  FSAppointmentEmployee.CompanyID
  ,CASE
    WHEN FSAppointmentEmployee.ActualDuration IS NOT NULL 
      AND FSAppointmentEmployee.ActualDateTimeBegin IS NULL
      AND FSAppointmentEmployee.ActualDateTimeEnd IS NULL
    THEN FSAppointment.ActualDateTimeBegin
    ELSE FSAppointmentEmployee.ActualDateTimeBegin
  END
  ,CASE
    WHEN FSAppointmentEmployee.ActualDuration IS NOT NULL 
      AND FSAppointmentEmployee.ActualDateTimeBegin IS NULL
      AND FSAppointmentEmployee.ActualDateTimeEnd IS NULL
    THEN DATEADD(minute, FSAppointmentEmployee.ActualDuration, FSAppointment.ActualDateTimeBegin)
    ELSE FSAppointmentEmployee.ActualDateTimeEnd
  END
  ,FSAppointmentEmployee.ActualDuration
  ,ApprovedTime
  ,FSAppointmentEmployee.CostCodeID
  ,FSAppointmentEmployee.CuryExtCost
  ,FSAppointmentEmployee.CuryInfoID
  ,FSAppointmentEmployee.CuryUnitCost
  ,ServiceLineRef
  ,FSAppointmentEmployee.AppointmentID
  ,FSAppointmentEmployee.RefNbr
  ,FSAppointmentEmployee.SrvOrdType
  ,EarningType
  ,EmployeeID
  ,[Type]
  ,FSAppointmentEmployee.ExtCost
  ,FSAppointmentEmployee.KeepActualDateTimes
  ,LaborItemID
  ,''
  ,0
  ,FSAppointmentEmployee.LineNbr
  ,CASE
    WHEN FSAppointmentDet.LineNbr IS NOT NULL THEN FSAppointmentDet.ProjectID 
    ELSE FSServiceOrder.ProjectID
  END
  ,CASE
    WHEN FSAppointmentDet.LineNbr IS NOT NULL THEN FSAppointmentDet.ProjectTaskID 
    ELSE FSAppointment.DfltProjectTaskID
  END
  ,'C'
  ,TimeCardCD
  ,0
  ,TrackTime
  ,FSAppointmentEmployee.UnitCost
  ,'SE'
  ,FSAppointmentEmployee.CreatedByID
  ,FSAppointmentEmployee.CreatedByScreenID
  ,FSAppointmentEmployee.CreatedDateTime
  ,FSAppointmentEmployee.LastModifiedByID
  ,FSAppointmentEmployee.LastModifiedByScreenID
  ,FSAppointmentEmployee.LastModifiedDateTime
  , NEWID()
FROM FSAppointmentEmployee
LEFT JOIN FSAppointmentDet
  ON(FSAppointmentDet.CompanyID = FSAppointmentEmployee.CompanyID
    AND FSAppointmentDet.AppointmentID = FSAppointmentEmployee.AppointmentID
    AND FSAppointmentDet.LineRef = FSAppointmentEmployee.ServiceLineRef)
INNER JOIN FSAppointment 
  ON(FSAppointment.CompanyID = FSAppointmentEmployee.CompanyID
    AND FSAppointment.SrvOrdType = FSAppointmentEmployee.SrvOrdType
    AND FSAppointment.RefNbr = FSAppointmentEmployee.RefNbr)
INNER JOIN FSServiceOrder 
  ON(FSServiceOrder.CompanyID = FSAppointment.CompanyID
    AND FSServiceOrder.SrvOrdType = FSAppointment.SrvOrdType
    AND FSServiceOrder.RefNbr = FSAppointment.SORefNbr)
WHERE
FSAppointment.Status NOT IN ('S','A')
;

INSERT INTO Note
  (CompanyID
  ,oldNoteID
  ,ExternalKey
  ,NoteText
  ,NotePopupText
  ,GraphType
  ,EntityType
  ,NoteID)
SELECT
    FSAppointmentEmployee.CompanyID
    ,NULL
    ,NULL
    ,FSAppointmentEmployee.Comment
    ,NULL
    ,'PX.Objects.FS.AppointmentEntry'
    ,'PX.Objects.FS.FSLog'
    ,FSLog.NoteID
FROM FSAppointmentEmployee
INNER JOIN FSLog
  ON(FSLog.OldFSAppointmentEmployeeLineNbr = FSAppointmentEmployee.LineNbr
    AND FSLog.CompanyID = FSAppointmentEmployee.CompanyID
    AND FSLog.DocID = FSAppointmentEmployee.AppointmentID)
INNER JOIN FSSetup
  ON(FSLog.CompanyID = FSSetup.CompanyID)
WHERE FSAppointmentEmployee.Comment IS NOT NULL;

INSERT INTO FSLog
  (CompanyID
  ,DateTimeBegin
  ,DateTimeEnd
  ,TimeDuration
  ,ApprovedTime
  ,CostCodeID
  ,CuryExtCost
  ,CuryInfoID
  ,CuryUnitCost
  ,DetLineRef
  ,DocID
  ,DocRefNbr
  ,DocType
  ,EarningType
  ,BAccountID
  ,BAccountType
  ,ExtCost
  ,KeepDateTimes
  ,LaborItemID
  ,LineRef
  ,LineNbr
  ,OldFSAppointmentEmployeeLineNbr
  ,ProjectID
  ,ProjectTaskID
  ,Status
  ,TimeCardCD
  ,TrackOnService
  ,TrackTime
  ,UnitCost
  ,Type
  ,CreatedByID
  ,CreatedByScreenID
  ,CreatedDateTime
  ,LastModifiedByID
  ,LastModifiedByScreenID
  ,LastModifiedDateTime
  ,NoteID)
SELECT
  FSAppointmentDet.CompanyID
  ,FSAppointmentDet.ActualDateTimeBegin
  ,FSAppointmentDet.ActualDateTimeEnd
  ,FSAppointmentDet.ActualDuration
  ,0
  ,FSAppointmentDet.CostCodeID
  ,FSAppointmentDet.CuryExtCost
  ,FSAppointmentDet.CuryInfoID
  ,FSAppointmentDet.CuryUnitCost
  ,FSAppointmentDet.LineRef
  ,FSAppointmentDet.AppointmentID
  ,FSAppointmentDet.RefNbr
  ,FSAppointmentDet.SrvOrdType
  ,NULL
  ,NULL
  ,NULL
  ,FSAppointmentDet.ExtCost
  ,FSAppointmentDet.KeepActualDateTimes
  ,NULL
  ,''
  ,0
  ,NULL
  ,FSAppointmentDet.ProjectID
  ,FSAppointmentDet.ProjectTaskID
  ,'C'
  ,NULL
  ,1
  ,0
  ,FSAppointmentDet.UnitCost
  ,'SE'
  ,FSAppointmentDet.CreatedByID
  ,FSAppointmentDet.CreatedByScreenID
  ,FSAppointmentDet.CreatedDateTime
  ,FSAppointmentDet.LastModifiedByID
  ,FSAppointmentDet.LastModifiedByScreenID
  ,FSAppointmentDet.LastModifiedDateTime
  , NEWID()
FROM FSAppointmentDet
WHERE
  FSAppointmentDet.StaffID IS NULL 
  AND FSAppointmentDet.ActualDateTimeBegin IS NOT NULL
  AND FSAppointmentDet.ActualDateTimeEnd IS NOT NULL
ORDER BY FSAppointmentDet.LineNbr ASC
GO

--[mysql: Skip]
--[mssql: Native]
--[IfExists(Table = FSLog)]
--[IfExists(Column = FSLog.LineNbr)]
--[MinVersion(Hash = 37b44efe9be701dc1a24c47a4b4661f77d61d7c82ee357a2f628512ec0cc4830)]
UPDATE LogDet
  SET LineNbr = (SELECT count(*) FROM FSLog A 
    INNER JOIN FSLog B ON A.CompanyID = B.CompanyID
                AND A.DocID = B.DocID
                AND A.LogID >= B.LogID
        WHERE A.DocID = LogDet.DocID 
        AND A.LogID = LogDet.LogID
        AND A.CompanyID = LogDet.CompanyID
        GROUP BY A.CompanyID, A.DocID, A.LogID)
FROM FSLog LogDet
WHERE LogDet.LineNbr = 0;
GO

--[mysql: Native]
--[mssql: Skip]
--[IfExists(Table = FSLog)]
--[IfExists(Column = FSLog.LineNbr)]
--[MinVersion(Hash = dd01735199ca50f560b45bc191c6bd59fe7c2e786dd536eef4249301ce2ea83d)]
UPDATE FSLog AS Det
  INNER JOIN (SELECT count(*) AS RowCountNbr, A.DocID, A.LogID, A.CompanyID FROM FSLog A 
          INNER JOIN FSLog B ON A.CompanyID = B.CompanyID
                AND A.DocID = B.DocID
                AND A.LogID >= B.LogID
          GROUP BY A.CompanyID, A.DocID, A.LogID) AS SelectCount ON
        SelectCount.DocID = Det.DocID 
        AND SelectCount.LogID = Det.LogID
        AND SelectCount.CompanyID = Det.CompanyID
  SET Det.LineNbr = SelectCount.RowCountNbr
WHERE Det.LineNbr = 0;
GO

--[IfExists(Table = FSLog)]
--[IfExists(Column = FSAppointment.LogLineCntr)]
--[MinVersion(Hash = c73aea51f363f0596d064f69f41c220bf657b2ae81bc967039d29b6e74fd5f2b)]
UPDATE FSLog SET LineRef = RIGHT('000' + CAST(LineNbr AS VARCHAR(3)), 3);

UPDATE FSAppointment
  SET LogLineCntr = ISNULL((SELECT MAX(LineNbr)
                    FROM  FSLog
                    WHERE FSLog.DocType = FSAppointment.SrvOrdType
                          AND FSLog.DocRefNbr = FSAppointment.RefNbr
                          AND FSLog.CompanyID = FSAppointment.CompanyID), 0)
FROM FSAppointment
WHERE LogLineCntr IS NULL;
GO

--[IfExists(Table = FSLog)]
--[IfExists(Column = FSxPMTimeActivity.OldAppEmpID)]
--[IfExists(Column = FSLog.OldFSAppointmentEmployeeLineNbr)]
--[MinVersion(Hash = c8da14e745d7f5e96178f279df3ebeec5bf9f03afc559b7bbbc5fbcba4eec65f)]
UPDATE FSxPMTimeActivity
  SET LogLineNbr = FSLog.LineNbr
FROM FSxPMTimeActivity
INNER JOIN FSAppointmentEmployee
  ON(FSAppointmentEmployee.AppointmentID = FSxPMTimeActivity.AppointmentID
    AND FSAppointmentEmployee.LineNbr = FSxPMTimeActivity.OldAppEmpID
    AND FSAppointmentEmployee.CompanyID = FSxPMTimeActivity.CompanyID)
INNER JOIN FSLog
  ON(FSLog.DocID = FSAppointmentEmployee.AppointmentID
    AND FSLog.OldFSAppointmentEmployeeLineNbr = FSAppointmentEmployee.LineNbr
    AND FSLog.CompanyID = FSAppointmentEmployee.CompanyID);
GO

--[mysql: Skip]
--[mssql: Native]
--[IfExists(Table = FSLog)]
--[IfExists(Column = FSAppointmentEmployee.GroupNbr)]
--[MinVersion(Hash = 827c8df5baf1ee15a1de172e98155c01e5d39d98e6ab53ae6346635f09a9f29d)]
UPDATE EmpDet
  SET GroupNbr = (SELECT count(*) FROM FSAppointmentEmployee A 
          INNER JOIN FSAppointmentEmployee B ON A.CompanyID = B.CompanyID
                        AND A.AppointmentID = B.AppointmentID
            AND A.EmployeeID = B.EmployeeID
            AND ISNULL(A.ServiceLineRef, -1) = ISNULL(B.ServiceLineRef, -1)
                        AND A.LineNbr >= B.LineNbr
          WHERE A.AppointmentID = EmpDet.AppointmentID 
            AND A.LineNbr = EmpDet.LineNbr
      AND A.EmployeeID = EmpDet.EmployeeID
      AND ISNULL(A.ServiceLineRef, -1) = ISNULL(EmpDet.ServiceLineRef, -1)
            AND A.CompanyID = EmpDet.CompanyID
          GROUP BY A.CompanyID, A.AppointmentID, A.EmployeeID, A.ServiceLineRef)
FROM FSAppointmentEmployee EmpDet
WHERE EmpDet.GroupNbr IS NULL;
GO

--[mysql: Native]
--[mssql: Skip]
--[IfExists(Table = FSLog)]
--[IfExists(Column = FSAppointmentEmployee.GroupNbr)]
--[MinVersion(Hash = c0fdb2abd2e68eadacbf3f5d6ca1dac16e592ecb4f124a224a2dda82baa2da6a)]
UPDATE FSAppointmentEmployee AS EmpDet
  INNER JOIN (SELECT count(*) AS RowCountNbr, A.AppointmentID, A.EmployeeID, A.ServiceLineRef , A.CompanyID FROM FSAppointmentEmployee A 
          INNER JOIN FSAppointmentEmployee B ON A.CompanyID = B.CompanyID
                        AND A.AppointmentID = B.AppointmentID
            AND A.EmployeeID = B.EmployeeID
            AND ISNULL(A.ServiceLineRef) = ISNULL(B.ServiceLineRef)
                        AND A.LineNbr >= B.LineNbr
          GROUP BY A.CompanyID, A.AppointmentID, A.EmployeeID, A.ServiceLineRef) AS SelectCount ON
      SelectCount.AppointmentID = EmpDet.AppointmentID 
      AND SelectCount.EmployeeID = EmpDet.EmployeeID
      AND ISNULL(SelectCount.ServiceLineRef) = ISNULL(EmpDet.ServiceLineRef)
            AND SelectCount.CompanyID = EmpDet.CompanyID
  SET EmpDet.GroupNbr = SelectCount.RowCountNbr
WHERE EmpDet.GroupNbr IS NULL;
GO

--[IfExists(Column = FSAppointmentEmployee.GroupNbr)]
--[MinVersion(Hash = 4d8967758301e57ba85309459e82e352c22ac4f13aa5fe3dd47d7c95d53a1802)]
DELETE FROM FSAppointmentEmployee WHERE GroupNbr > 1;
GO

--[mysql: Skip]
--[mssql: Native]
--[IfExists(Table = FSLog)]
--[IfExists(Column = FSAppointmentEmployee.PrimaryDriver)]
--[MinVersion(Hash = 3f1e73c14183f7ced71cde2b047408a1ee25dcb6065d664fcf58659e33f65dee)]
UPDATE FSEmp
SET PrimaryDriver = 
        CASE 
        WHEN FSEmp.LineNbr = 
            (SELECT MIN(LineNbr) FROM FSAppointmentEmployee
            WHERE CompanyID = FSEmp.CompanyID AND
                SrvOrdType = FSEmp.SrvOrdType AND
                RefNbr = FSEmp.RefNbr)
        THEN 1 ELSE 0 END
FROM FSAppointmentEmployee AS FSEmp
WHERE FSEmp.PrimaryDriver IS NULL

UPDATE FSEmp
SET PrimaryDriver = 1
FROM FSAppointmentEmployee AS FSEmp
WHERE EmployeeID IN ( SELECT EmployeeID FROM FSAppointmentEmployee
            WHERE PrimaryDriver = 1 AND
            CompanyID = FSEmp.CompanyID AND
            SrvOrdType = FSEmp.SrvOrdType AND
            RefNbr = FSEmp.RefNbr)
GO

--[mysql: Native]
--[mssql: Skip]
--[IfExists(Table = FSLog)]
--[IfExists(Column = FSAppointmentEmployee.PrimaryDriver)]
--[MinVersion(Hash = a29ab24ebc40f7825a7db3aa126fd0094c4e197e397ab57eb88c26ef04553ee5)]
UPDATE FSAppointmentEmployee AS FSEmp
LEFT JOIN (SELECT MIN(LineNbr) AS LineNbr, CompanyID, SrvOrdType, RefNbr FROM FSAppointmentEmployee
       GROUP BY CompanyID, SrvOrdType, RefNbr) AS MinLineNbr ON
           FSEmp.CompanyID = MinLineNbr.CompanyID AND
           FSEmp.SrvOrdType = MinLineNbr.SrvOrdType AND
           FSEmp.RefNbr = MinLineNbr.RefNbr
SET PrimaryDriver = CASE WHEN FSEmp.LineNbr = MinLineNbr.LineNbr THEN 1 ELSE 0 END
WHERE PrimaryDriver IS NULL;

UPDATE FSAppointmentEmployee AS FSEmp
INNER JOIN (SELECT EmployeeID, CompanyID, SrvOrdType, RefNbr FROM FSAppointmentEmployee
            WHERE PrimaryDriver = 1
            GROUP BY CompanyID, SrvOrdType, RefNbr, EmployeeID) AS EmpPrimaryDriver ON
            FSEmp.CompanyID = EmpPrimaryDriver.CompanyID AND
           FSEmp.SrvOrdType = EmpPrimaryDriver.SrvOrdType AND
           FSEmp.RefNbr = EmpPrimaryDriver.RefNbr
SET PrimaryDriver = 1
WHERE FSEmp.EmployeeID = EmpPrimaryDriver.EmployeeID
GO

--[IfExists(Column = FSAppointmentEmployee.NoteID)]
--[MinVersion(Hash = 72f3cbfac377f5162114d5780e529e6daba28b2b0977ecd53e446499000dc111)]
UPDATE FSAppointmentEmployee SET
  NoteID = NEWID()
WHERE NoteID IS NULL;
GO

--[IfExists(Column = FSAppointmentEmployee.DfltProjectID)]
--[IfExists(Column = FSAppointmentEmployee.DfltProjectTaskID)]
--[MinVersion(Hash = d5ec24ea34879422492a88d5e6a9780bb14fca69894fffd837105c8859d84700)]
UPDATE FSAppointmentEmployee SET
  DfltProjectID = CASE
    WHEN FSAppointmentDet.LineNbr IS NOT NULL THEN FSAppointmentDet.ProjectID 
    ELSE FSServiceOrder.ProjectID
  END
  ,DfltProjectTaskID = CASE
    WHEN FSAppointmentDet.LineNbr IS NOT NULL THEN FSAppointmentDet.ProjectTaskID 
    ELSE FSAppointment.DfltProjectTaskID
  END
FROM FSAppointmentEmployee
LEFT JOIN FSAppointmentDet
  ON(FSAppointmentDet.CompanyID = FSAppointmentEmployee.CompanyID
    AND FSAppointmentDet.AppointmentID = FSAppointmentEmployee.AppointmentID
    AND FSAppointmentDet.LineRef = FSAppointmentEmployee.ServiceLineRef)
INNER JOIN FSAppointment 
  ON(FSAppointment.CompanyID = FSAppointmentEmployee.CompanyID
    AND FSAppointment.SrvOrdType = FSAppointmentEmployee.SrvOrdType
    AND FSAppointment.RefNbr = FSAppointmentEmployee.RefNbr)
INNER JOIN FSServiceOrder 
  ON(FSServiceOrder.CompanyID = FSAppointment.CompanyID
    AND FSServiceOrder.SrvOrdType = FSAppointment.SrvOrdType
    AND FSServiceOrder.RefNbr = FSAppointment.SORefNbr)
WHERE FSAppointmentEmployee.DfltProjectTaskID IS NULL;
GO

--[IfExists(Column = FSSrvOrdType.OnStartApptSetStartTimeInHeader)]
--[IfExists(Column = FSSrvOrdType.StartAppointmentActionBehavior)]
--[MinVersion(Hash = 8571abe6332c234151d3ebf3e5d6e455038b0579e201020dfd728626f2105eb4)]
UPDATE FSSrvOrdType
SET
    OnStartApptSetStartTimeInHeader = CASE WHEN StartAppointmentActionBehavior = 'HO'
                                                  OR StartAppointmentActionBehavior = 'HS' THEN 1 ELSE 0 END,
    OnStartApptStartUnassignedStaff = CASE WHEN StartAppointmentActionBehavior = 'HS' THEN 1 ELSE 0 END,
    OnStartApptStartServiceAndStaff = CASE WHEN StartAppointmentActionBehavior = 'HS' THEN 1 ELSE 0 END,
    OnCompleteApptSetEndTimeInHeader = CASE WHEN CompleteAppointmentActionBehavior = 'HO' 
                                                  OR CompleteAppointmentActionBehavior = 'HS' THEN 1 ELSE 0 END,
    OnCompleteApptSetInProcessItemsAs = 'CP',
    OnStartTimeChangeUpdateLogStartTime = UpdateServiceActualDateTimeBegin,
    OnEndTimeChangeUpdateLogEndTime = UpdateServiceActualDateTimeEnd,
    AllowManualLogTimeEdition = KeepActualDateTimes,
    SetTimeInHeaderBasedOnLog = CASE WHEN UpdateServiceActualDateTimeBegin = 1 OR UpdateServiceActualDateTimeBegin = 1 THEN 0 
                                     ELSE UpdateHeaderActualDateTimes END,
    OnCompleteApptRequireLog = RequireServiceActualDateTimes;
GO

--[IfExists(Table = FSLog)]
--[IfExists(Column = FSLog.TrackOnService)]
--[MinVersion(Hash = f9bfeae6009644f2cf626cdabfbc43ab58641297ec1e474bc9198ba37100efc4)]
UPDATE FSLog
  SET FSLog.TrackOnService = 1
WHERE
FSLog.DetLineRef IS NOT NULL;
GO

--[IfExists(Column = FSAppointmentDet.LogActualDuration)]
--[IfExists(Column = FSAppointmentDet.StaffActualDuration)]
--[MinVersion(Hash = a42b8d739ce962ce44d0f577a5879fda94c1249a4abf488a7e912e828c1ca470)]
UPDATE FSAppointmentDet 
  SET LogActualDuration = StaffActualDuration
WHERE
LogActualDuration IS NULL;
GO

--[IfExists(Column = FSAppointment.MaxLogTimeEnd)]
--[IfExists(Column = FSAppointmentDet.MinLogTimeBegin)]
--[MinVersion(Hash = 464e3ac6a7663543cdded1a6fb4a2e17b007735134517fcc7bdd52298ef6dedd)]
UPDATE FSAppointment 
  SET
    MinLogTimeBegin = Upt.MinLogTimeBegin,
    MaxLogTimeEnd = Upt.MaxLogTimeEnd
FROM FSAppointment
INNER JOIN (
  SELECT  CompanyID = FSAppointment.CompanyID,
      SrvOrdType = FSAppointment.SrvOrdType,
      RefNbr = FSAppointment.RefNbr,
      MinLogTimeBegin = MIN(FSLog.DateTimeBegin),
      MaxLogTimeEnd = MAX(FSLog.DateTimeEnd)
  FROM FSAppointment
  INNER JOIN FSLog 
  ON (FSLog.CompanyID = FSAppointment.CompanyID
      AND FSLog.DocType = FSAppointment.SrvOrdType
      AND FSLog.DocRefNbr = FSAppointment.RefNbr)
  GROUP BY FSAppointment.CompanyID, FSAppointment.SrvOrdType, FSAppointment.RefNbr) Upt
ON (Upt.CompanyID = FSAppointment.CompanyID
  AND Upt.SrvOrdType = FSAppointment.SrvOrdType
  AND Upt.RefNbr = FSAppointment.RefNbr)
WHERE
  FSAppointment.MaxLogTimeEnd IS NULL;
GO

--[IfExists(Column = FSAppointmentDet.IsTravelItem)]
--[MinVersion(Hash = 48f7e40cbbccd2fb84854aacb3ae439fec0a553d2fa965a037dbbb06ae23f8f4)]
UPDATE FSAppointmentDet
  SET IsTravelItem = 0
WHERE
FSAppointmentDet.IsTravelItem IS NULL;
GO

--[IfExists(Table = FSAppointmentTax)]
--[IfExists(Table = FSServiceOrderTax)]
--[IfExists(Table = FSTaxDetail)]
--[IfNotExists(Column = FSTaxDetail.TaxUOM)]
--[MinVersion(Hash = c65ac32ec2effd738eab9c193dfa61f2733aa79e5eab02fefc925bdda1601383)]
INSERT INTO [FSServiceOrderTax]
	(CompanyID
	,SrvOrdType
	,RefNbr
	,LineNbr
	,TaxID
	,CuryExpenseAmt
	,CuryInfoID
	,CuryTaxableAmt
	,CuryTaxAmt
	,ExpenseAmt
	,NonDeductibleTaxRate
	,TaxableAmt
	,TaxAmt
	,TaxRate
	,CreatedByID
	,CreatedByScreenID
	,CreatedDateTime
	,LastModifiedByID
	,LastModifiedByScreenID
	,LastModifiedDateTime)
SELECT
	FSTaxDetail.CompanyID
	,SrvOrdType
	,RefNbr
	,LineNbr
	,TaxID
	,CuryExpenseAmt
	,FSTaxDetail.CuryInfoID
	,CuryTaxableAmt
	,CuryTaxAmt
	,ExpenseAmt
	,NonDeductibleTaxRate
	,TaxableAmt
	,TaxAmt
	,TaxRate
	,FSTaxDetail.CreatedByID
	,FSTaxDetail.CreatedByScreenID
	,FSTaxDetail.CreatedDateTime
	,FSTaxDetail.LastModifiedByID
	,FSTaxDetail.LastModifiedByScreenID
	,FSTaxDetail.LastModifiedDateTime
FROM FSTaxDetail
INNER JOIN FSServiceOrder 
ON(
    FSServiceOrder.CompanyID = FSTaxDetail.CompanyID 
	  AND FSServiceOrder.SOID = FSTaxDetail.EntityID
	  AND FSTaxDetail.EntityType = 'SO'
	  AND FSTaxDetail.LineNbr != 2147483647);

INSERT INTO [FSServiceOrderTaxTran]
  (CompanyID
  ,SrvOrdType
  ,RefNbr
  ,TaxID
  ,JurisType
  ,JurisName
  ,CuryExpenseAmt
  ,CuryInfoID
  ,CuryTaxableAmt
  ,CuryTaxAmt
  ,ExpenseAmt
  ,NonDeductibleTaxRate
  ,TaxableAmt
  ,TaxAmt
  ,TaxRate
  ,CreatedByID
  ,CreatedByScreenID
  ,CreatedDateTime
  ,LastModifiedByID
  ,LastModifiedByScreenID
  ,LastModifiedDateTime)
SELECT
  FSTaxDetail.CompanyID
  ,SrvOrdType
  ,RefNbr
  ,TaxID
  ,NULL
  ,NULL
  ,CuryExpenseAmt
  ,FSTaxDetail.CuryInfoID
  ,CuryTaxableAmt
  ,CuryTaxAmt
  ,ExpenseAmt
  ,NonDeductibleTaxRate
  ,TaxableAmt
  ,TaxAmt
  ,TaxRate
  ,FSTaxDetail.CreatedByID
  ,FSTaxDetail.CreatedByScreenID
  ,FSTaxDetail.CreatedDateTime
  ,FSTaxDetail.LastModifiedByID
  ,FSTaxDetail.LastModifiedByScreenID
  ,FSTaxDetail.LastModifiedDateTime
FROM FSTaxDetail
INNER JOIN FSServiceOrder 
ON(
    FSServiceOrder.CompanyID = FSTaxDetail.CompanyID 
    AND FSServiceOrder.SOID = FSTaxDetail.EntityID
    AND FSTaxDetail.EntityType = 'SO'
    AND FSTaxDetail.LineNbr = 2147483647);

INSERT INTO [FSAppointmentTax]
	(CompanyID
	,SrvOrdType
	,RefNbr
	,LineNbr
	,TaxID
	,CuryExpenseAmt
	,CuryInfoID
	,CuryTaxableAmt
	,CuryTaxAmt
	,ExpenseAmt
	,NonDeductibleTaxRate
	,TaxableAmt
	,TaxAmt
	,TaxRate
	,CreatedByID
	,CreatedByScreenID
	,CreatedDateTime
	,LastModifiedByID
	,LastModifiedByScreenID
	,LastModifiedDateTime)
SELECT
	FSTaxDetail.CompanyID
	,SrvOrdType
	,RefNbr
	,LineNbr
	,TaxID
	,CuryExpenseAmt
	,FSTaxDetail.CuryInfoID
	,CuryTaxableAmt
	,CuryTaxAmt
	,ExpenseAmt
	,NonDeductibleTaxRate
	,TaxableAmt
	,TaxAmt
	,TaxRate
	,FSTaxDetail.CreatedByID
	,FSTaxDetail.CreatedByScreenID
	,FSTaxDetail.CreatedDateTime
	,FSTaxDetail.LastModifiedByID
	,FSTaxDetail.LastModifiedByScreenID
	,FSTaxDetail.LastModifiedDateTime
FROM FSTaxDetail
INNER JOIN FSAppointment 
ON(
    FSAppointment.CompanyID = FSTaxDetail.CompanyID 
    AND FSAppointment.AppointmentID = FSTaxDetail.EntityID
    AND FSTaxDetail.EntityType = 'AP'
    AND FSTaxDetail.LineNbr != 2147483647);

INSERT INTO [FSAppointmentTaxTran]
    ([CompanyID]
    ,[SrvOrdType]
    ,[RefNbr]
    ,[TaxID]
    ,[JurisType]
    ,[JurisName]
    ,[CuryExpenseAmt]
    ,[CuryInfoID]
    ,[CuryTaxableAmt]
    ,[CuryTaxAmt]
    ,[ExpenseAmt]
    ,[NonDeductibleTaxRate]
    ,[TaxableAmt]
    ,[TaxAmt]
    ,[TaxRate]

    ,[CreatedByID]
    ,[CreatedByScreenID]
    ,[CreatedDateTime]
    ,[LastModifiedByID]
    ,[LastModifiedByScreenID]
    ,[LastModifiedDateTime])
SELECT
	FSTaxDetail.CompanyID
	,SrvOrdType
	,RefNbr
	,TaxID
	,NULL
	,NULL
	,CuryExpenseAmt
	,FSTaxDetail.CuryInfoID
	,CuryTaxableAmt
	,CuryTaxAmt
	,ExpenseAmt
	,NonDeductibleTaxRate
	,TaxableAmt
	,TaxAmt
	,TaxRate
	,FSTaxDetail.CreatedByID
	,FSTaxDetail.CreatedByScreenID
	,FSTaxDetail.CreatedDateTime
	,FSTaxDetail.LastModifiedByID
	,FSTaxDetail.LastModifiedByScreenID
	,FSTaxDetail.LastModifiedDateTime
FROM FSTaxDetail
INNER JOIN FSAppointment 
ON(
    FSAppointment.CompanyID = FSTaxDetail.CompanyID 
    AND FSAppointment.AppointmentID = FSTaxDetail.EntityID
    AND FSTaxDetail.EntityType = 'AP'
    AND FSTaxDetail.LineNbr = 2147483647);
GO

--[IfExists(Table = FSAppointmentTax)]
--[IfExists(Table = FSServiceOrderTax)]
--[IfExists(Table = FSTaxDetail)]
--[IfExists(Column = FSTaxDetail.TaxUOM)]
--[MinVersion(Hash = 7cf8df9da7c98f2c6cd81803cfc754cc6eea51b6b4aa2bf64e99f6cdd91c1bd2)]
INSERT INTO [FSServiceOrderTax]
	(CompanyID
	,SrvOrdType
	,RefNbr
	,LineNbr
	,TaxID
	,CuryExpenseAmt
	,CuryInfoID
	,CuryTaxableAmt
	,CuryTaxAmt
	,ExpenseAmt
	,NonDeductibleTaxRate
	,TaxableAmt
	,TaxAmt
	,TaxRate
	,TaxUOM
	,TaxableQty
	,CreatedByID
	,CreatedByScreenID
	,CreatedDateTime
	,LastModifiedByID
	,LastModifiedByScreenID
	,LastModifiedDateTime)
SELECT
	FSTaxDetail.CompanyID
	,SrvOrdType
	,RefNbr
	,LineNbr
	,TaxID
	,CuryExpenseAmt
	,FSTaxDetail.CuryInfoID
	,CuryTaxableAmt
	,CuryTaxAmt
	,ExpenseAmt
	,NonDeductibleTaxRate
	,TaxableAmt
	,TaxAmt
	,TaxRate
	,TaxUOM
	,TaxableQty
	,FSTaxDetail.CreatedByID
	,FSTaxDetail.CreatedByScreenID
	,FSTaxDetail.CreatedDateTime
	,FSTaxDetail.LastModifiedByID
	,FSTaxDetail.LastModifiedByScreenID
	,FSTaxDetail.LastModifiedDateTime
FROM FSTaxDetail
INNER JOIN FSServiceOrder 
ON(
    FSServiceOrder.CompanyID = FSTaxDetail.CompanyID 
	  AND FSServiceOrder.SOID = FSTaxDetail.EntityID
	  AND FSTaxDetail.EntityType = 'SO'
	  AND FSTaxDetail.LineNbr != 2147483647);

INSERT INTO [FSServiceOrderTaxTran]
  (CompanyID
  ,SrvOrdType
  ,RefNbr
  ,TaxID
  ,JurisType
  ,JurisName
  ,CuryExpenseAmt
  ,CuryInfoID
  ,CuryTaxableAmt
  ,CuryTaxAmt
  ,ExpenseAmt
  ,NonDeductibleTaxRate
  ,TaxableAmt
  ,TaxAmt
  ,TaxRate
  ,TaxUOM
  ,TaxableQty
  ,CreatedByID
  ,CreatedByScreenID
  ,CreatedDateTime
  ,LastModifiedByID
  ,LastModifiedByScreenID
  ,LastModifiedDateTime)
SELECT
  FSTaxDetail.CompanyID
  ,SrvOrdType
  ,RefNbr
  ,TaxID
  ,NULL
  ,NULL
  ,CuryExpenseAmt
  ,FSTaxDetail.CuryInfoID
  ,CuryTaxableAmt
  ,CuryTaxAmt
  ,ExpenseAmt
  ,NonDeductibleTaxRate
  ,TaxableAmt
  ,TaxAmt
  ,TaxRate
  ,TaxUOM
  ,TaxableQty
  ,FSTaxDetail.CreatedByID
  ,FSTaxDetail.CreatedByScreenID
  ,FSTaxDetail.CreatedDateTime
  ,FSTaxDetail.LastModifiedByID
  ,FSTaxDetail.LastModifiedByScreenID
  ,FSTaxDetail.LastModifiedDateTime
FROM FSTaxDetail
INNER JOIN FSServiceOrder 
ON(
    FSServiceOrder.CompanyID = FSTaxDetail.CompanyID 
    AND FSServiceOrder.SOID = FSTaxDetail.EntityID
    AND FSTaxDetail.EntityType = 'SO'
    AND FSTaxDetail.LineNbr = 2147483647);

INSERT INTO [FSAppointmentTax]
	(CompanyID
	,SrvOrdType
	,RefNbr
	,LineNbr
	,TaxID
	,CuryExpenseAmt
	,CuryInfoID
	,CuryTaxableAmt
	,CuryTaxAmt
	,ExpenseAmt
	,NonDeductibleTaxRate
	,TaxableAmt
	,TaxAmt
	,TaxRate
	,TaxUOM
	,TaxableQty
	,CreatedByID
	,CreatedByScreenID
	,CreatedDateTime
	,LastModifiedByID
	,LastModifiedByScreenID
	,LastModifiedDateTime)
SELECT
	FSTaxDetail.CompanyID
	,SrvOrdType
	,RefNbr
	,LineNbr
	,TaxID
	,CuryExpenseAmt
	,FSTaxDetail.CuryInfoID
	,CuryTaxableAmt
	,CuryTaxAmt
	,ExpenseAmt
	,NonDeductibleTaxRate
	,TaxableAmt
	,TaxAmt
	,TaxRate
	,TaxUOM
	,TaxableQty
	,FSTaxDetail.CreatedByID
	,FSTaxDetail.CreatedByScreenID
	,FSTaxDetail.CreatedDateTime
	,FSTaxDetail.LastModifiedByID
	,FSTaxDetail.LastModifiedByScreenID
	,FSTaxDetail.LastModifiedDateTime
FROM FSTaxDetail
INNER JOIN FSAppointment 
ON(
    FSAppointment.CompanyID = FSTaxDetail.CompanyID 
    AND FSAppointment.AppointmentID = FSTaxDetail.EntityID
    AND FSTaxDetail.EntityType = 'AP'
    AND FSTaxDetail.LineNbr != 2147483647);

INSERT INTO [FSAppointmentTaxTran]
    ([CompanyID]
    ,[SrvOrdType]
    ,[RefNbr]
    ,[TaxID]
    ,[JurisType]
    ,[JurisName]
    ,[CuryExpenseAmt]
    ,[CuryInfoID]
    ,[CuryTaxableAmt]
    ,[CuryTaxAmt]
    ,[ExpenseAmt]
    ,[NonDeductibleTaxRate]
    ,[TaxableAmt]
    ,[TaxAmt]
    ,[TaxRate]
    ,[TaxUOM]
    ,[TaxableQty]
    ,[CreatedByID]
    ,[CreatedByScreenID]
    ,[CreatedDateTime]
    ,[LastModifiedByID]
    ,[LastModifiedByScreenID]
    ,[LastModifiedDateTime])
SELECT
	FSTaxDetail.CompanyID
	,SrvOrdType
	,RefNbr
	,TaxID
	,NULL
	,NULL
	,CuryExpenseAmt
	,FSTaxDetail.CuryInfoID
	,CuryTaxableAmt
	,CuryTaxAmt
	,ExpenseAmt
	,NonDeductibleTaxRate
	,TaxableAmt
	,TaxAmt
	,TaxRate
	,TaxUOM
	,TaxableQty
	,FSTaxDetail.CreatedByID
	,FSTaxDetail.CreatedByScreenID
	,FSTaxDetail.CreatedDateTime
	,FSTaxDetail.LastModifiedByID
	,FSTaxDetail.LastModifiedByScreenID
	,FSTaxDetail.LastModifiedDateTime
FROM FSTaxDetail
INNER JOIN FSAppointment 
ON(
    FSAppointment.CompanyID = FSTaxDetail.CompanyID 
    AND FSAppointment.AppointmentID = FSTaxDetail.EntityID
    AND FSTaxDetail.EntityType = 'AP'
    AND FSTaxDetail.LineNbr = 2147483647);
GO

--[IfExists(Table = FSTaxDetail)]
--[MinVersion(Hash = f81f7bf8a27bed42899639250f8f5619b7bacab2db200606eb31a50ca3c89b42)]
exec sp_executesql N'DROP TABLE FSTaxDetail';
GO

--[mysql: Skip]
--[mssql: Native]
--[IfExists(Column = FSEquipmentComponent.NoteID)]
--[MinVersion(Hash = df484417c8cfc8118fe2ad0d518692dafa22c11a9059434669855580880ba009)]
UPDATE FSEquipmentComponent SET NoteID = NEWID() WHERE NoteID IS NULL;
UPDATE FSGeoZone SET NoteID = NEWID() WHERE NoteID IS NULL;
UPDATE FSManufacturer SET NoteID = NEWID() WHERE NoteID IS NULL;
UPDATE FSManufacturerModel SET NoteID = NEWID() WHERE NoteID IS NULL;
UPDATE FSMasterContract SET NoteID = NEWID() WHERE NoteID IS NULL;
UPDATE FSModelComponent SET NoteID = NEWID() WHERE NoteID IS NULL;
UPDATE FSModelTemplateComponent SET NoteID = NEWID() WHERE NoteID IS NULL;
UPDATE FSSkill SET NoteID = NEWID() WHERE NoteID IS NULL;
UPDATE FSWFStage SET NoteID = NEWID() WHERE NoteID IS NULL;
GO

--[mssql: Skip]
--[mysql: Native]
--[IfExists(Column = FSEquipmentComponent.NoteID)]
--[MinVersion(Hash = 703e377ad51321d6680f03061f140af9c1cc577b0d9b9773afaf0c467aefc9d9)]
UPDATE FSEquipmentComponent SET NoteID = UUID() WHERE NoteID IS NULL;
UPDATE FSGeoZone SET NoteID = UUID() WHERE NoteID IS NULL;
UPDATE FSManufacturer SET NoteID = UUID() WHERE NoteID IS NULL;
UPDATE FSManufacturerModel SET NoteID = UUID() WHERE NoteID IS NULL;
UPDATE FSMasterContract SET NoteID = UUID() WHERE NoteID IS NULL;
UPDATE FSModelComponent SET NoteID = UUID() WHERE NoteID IS NULL;
UPDATE FSModelTemplateComponent SET NoteID = UUID() WHERE NoteID IS NULL;
UPDATE FSSkill SET NoteID = UUID() WHERE NoteID IS NULL;
UPDATE FSWFStage SET NoteID = UUID() WHERE NoteID IS NULL;
GO

--[IfExists(Column = FSPostInfo.PMPosted)]
--[MinVersion(Hash = 3d191b8a7eea5366298d0fbf8ed638f2fac174683ded5b41c67a01a507aeb74f)]
UPDATE FSPostInfo
SET FSPostInfo.PMPosted = 0
WHERE FSPostInfo.PMPosted IS NULL;
GO

--[IfExists(Column = FSPostDet.PMPosted)]
--[MinVersion(Hash = 0096035e710c849befc299ed982ba1e0d793ccd16c9ad11c236f221293521aa2)]
UPDATE FSPostDet
SET FSPostDet.PMPosted = 0
WHERE FSPostDet.PMPosted IS NULL;
GO

--[IfExists(Column = FSSrvOrdType.BillingType)]
--[MinVersion(Hash = 3e00d5fa4cdca52fbef2ed5bf44da84fa96f37847a4cc4629f3a74b65c136dd4)]
UPDATE FSSrvOrdType SET BillingType = 'CC' WHERE BillingType IS NULL
GO

--[IfExists(Column = FSSrvOrdType.BillingType)]
--[IfExists(Column = FSSrvOrdType.ReleaseIssueOnInvoice)]
--[MinVersion(Hash = c1d4c03f92a2eab68324c4e85ed9c74a388c39ebfc02a6a00b1d3b277a17856c)]
UPDATE FSSrvOrdType SET ReleaseIssueOnInvoice = 1 WHERE BillingType = 'CC'
GO

--[mysql: Skip]
--[mssql: Native]
--[MinVersion(Hash = 2921d57849d69e92c570fc5017d06649aa29340c9d3870453fd3da1750c674ee)]
ALTER TABLE FSAppointmentDet ALTER COLUMN Status Char(2) NULL;
ALTER TABLE FSSODet ALTER COLUMN Status Char(2) NULL;
GO

--[mysql: Native]
--[mssql: Skip]
--[MinVersion(Hash = fa8b843ac31dd8bc7d6cce991a782763733631d39e40c1301ae52a6ec9cbf929)]
ALTER TABLE FSAppointmentDet MODIFY COLUMN `status` CHAR(2) NULL;
ALTER TABLE FSSODet MODIFY COLUMN `status` CHAR(2) NULL;
GO

--[MinVersion(Hash = 3494f49915c560661c335aa5b99e660b4d86b1901ad585049cfaf170a4695943)]
UPDATE FSAppointmentDet
SET Status = Case 
              When Status = 'O' THEN 'NS'
              When Status = 'X' THEN 'CC'
              When Status = 'C' THEN 'CP'
              When Status = 'P' THEN 'IP'
              ELSE 'NS'
             End
WHERE Status NOT IN ('NS', 'IP', 'NF', 'NP', 'CC', 'CP');

UPDATE FSSODet
SET Status = Case 
              When Status = 'O' THEN 'SN'
              When Status = 'X' THEN 'CC'
              When Status = 'C' THEN 'CP'
              When Status = 'P' THEN 'SN'
              ELSE 'SN'
            End
WHERE Status NOT IN ('SN', 'CC', 'CP', 'SN');

UPDATE FSAppointmentDet 
  SET Status = 'CC'
FROM FSAppointmentDet
INNER JOIN FSAppointment 
  ON(FSAppointment.CompanyID = FSAppointmentDet.CompanyID
    AND FSAppointment.SrvOrdType = FSAppointmentDet.SrvOrdType
    AND FSAppointment.RefNbr = FSAppointmentDet.RefNbr)
WHERE
  FSAppointment.Status = 'X';
GO

--[IfExists(Column = FSSrvOrdType.OnCompleteApptSetNotStartedItemsAs)]
--[MinVersion(Hash = 6402ee623b44f1c58d4b4c54d73cfb5bdb4be577900a066828ae018abfbcacdc)]
UPDATE FSSrvOrdType SET
	OnCompleteApptSetNotStartedItemsAs = 'CP'
WHERE
	OnCompleteApptSetNotStartedItemsAs IS NULL
GO

--[IfExists(Column = FSSrvOrdType.OnCompleteApptSetInProcessItemsAs)]
--[MinVersion(Hash = bab93376d476e2b6d5eefb356c3512d08c4bb2d7411dd41f7cf54107e5d9dd1e)]
UPDATE FSSrvOrdType SET
	OnCompleteApptSetInProcessItemsAs = 'CP'
WHERE
	OnCompleteApptSetInProcessItemsAs IS NULL
GO

--[IfExists(Column = FSSODet.ApptCntr)]
--[IfExists(Column = FSSODet.ApptNumber)]
--[MinVersion(Hash = 84a70e0755147b8f9b0e8b24b07e2307f29e9a1c102fa0927cc438c3ad561053)]
UPDATE FSSODet
SET ApptCntr = ApptNumber;
GO

--[IfExists(Column = FSSODet.ApptEstimatedDuration)]
--[IfExists(Column = FSSODet.ApptCntr)]
--[MinVersion(Hash = 9babc576a31eefe6b8b60ed3765665a5269ec050030835118e787ca3d9956098)]
UPDATE FSSODet
SET 
  ApptEstimatedDuration = ISNULL((SELECT SUM(FSAppointmentDet.EstimatedDuration)
                        FROM  FSAppointmentDet
                        WHERE 
                          FSAppointmentDet.CompanyID = FSSODet.CompanyID
                          AND FSAppointmentDet.SODetID = FSSODet.SODetID
                          AND FSAppointmentDet.Status <> 'CC'
                          AND FSAppointmentDet.Status <> 'NP'), 0),
	ApptDuration = ISNULL((SELECT SUM(FSAppointmentDet.ActualDuration)
                        FROM  FSAppointmentDet
                        WHERE 
                          FSAppointmentDet.CompanyID = FSSODet.CompanyID
                          AND FSAppointmentDet.SODetID = FSSODet.SODetID
                          AND FSAppointmentDet.Status <> 'CC'
                          AND FSAppointmentDet.Status <> 'NP'), 0),
	ApptQty = ISNULL((SELECT SUM(FSAppointmentDet.BillableQty)
                        FROM  FSAppointmentDet
                        WHERE 
                          FSAppointmentDet.CompanyID = FSSODet.CompanyID
                          AND FSAppointmentDet.SODetID = FSSODet.SODetID
                          AND FSAppointmentDet.Status <> 'CC'
                          AND FSAppointmentDet.Status <> 'NP'), 0),
  ApptCntr = ISNULL((SELECT COUNT(FSAppointmentDet.AppDetID)
                        FROM  FSAppointmentDet
                        WHERE 
                          FSAppointmentDet.CompanyID = FSSODet.CompanyID
                          AND FSAppointmentDet.SODetID = FSSODet.SODetID
                          AND FSAppointmentDet.Status <> 'CC'
                          AND FSAppointmentDet.Status <> 'NP'), 0),
  ApptTranAmt = ISNULL((SELECT SUM(FSAppointmentDet.BillableTranAmt)
                        FROM  FSAppointmentDet
                        WHERE 
                          FSAppointmentDet.CompanyID = FSSODet.CompanyID
                          AND FSAppointmentDet.SODetID = FSSODet.SODetID
                          AND FSAppointmentDet.Status <> 'CC'
                          AND FSAppointmentDet.Status <> 'NP'), 0),
  CuryApptTranAmt = ISNULL((SELECT SUM(FSAppointmentDet.CuryBillableTranAmt)
                        FROM  FSAppointmentDet
                        WHERE 
                          FSAppointmentDet.CompanyID = FSSODet.CompanyID
                          AND FSAppointmentDet.SODetID = FSSODet.SODetID
                          AND FSAppointmentDet.Status <> 'CC'
                          AND FSAppointmentDet.Status <> 'NP'), 0)
FROM FSSODet;

UPDATE FSSODet
SET 
  Status = 'CP'
FROM FSSODet
WHERE (
    SELECT COUNT(FSAppointmentDet.AppDetID)
    FROM FSAppointmentDet
    WHERE FSAppointmentDet.CompanyID = FSSODet.CompanyID
    AND FSAppointmentDet.SODetID = FSSODet.SODetID
    AND FSAppointmentDet.Status = 'CP'
) = FSSODet.ApptCntr
AND ApptCntr <> 0;

UPDATE FSSODet
SET 
  Status = 'CC'
FROM FSSODet
WHERE (
    SELECT COUNT(FSAppointmentDet.AppDetID)
    FROM FSAppointmentDet
    WHERE FSAppointmentDet.CompanyID = FSSODet.CompanyID
    AND FSAppointmentDet.SODetID = FSSODet.SODetID
    AND FSAppointmentDet.Status = 'CC'
) <> 0
AND
(
    SELECT COUNT(FSAppointmentDet.AppDetID)
    FROM FSAppointmentDet
    WHERE FSAppointmentDet.CompanyID = FSSODet.CompanyID
    AND FSAppointmentDet.SODetID = FSSODet.SODetID
    AND FSAppointmentDet.Status <> 'CC'
) = 0;

UPDATE FSSODet
SET FSSODet.Status = 'SN'
FROM FSSODet
WHERE
(
  (FSSODet.EstimatedDuration > FSSODet.ApptEstimatedDuration AND FSSODet.BillingRule = 'TIME')
  OR (FSSODet.EstimatedQty > FSSODet.ApptQty AND FSSODet.BillingRule <> 'TIME')
);

UPDATE FSSODet
SET FSSODet.Status = 'SC'
FROM FSSODet
WHERE
(
  (FSSODet.ApptEstimatedDuration >= FSSODet.EstimatedDuration AND FSSODet.BillingRule = 'TIME')
  OR (FSSODet.ApptQty >= FSSODet.EstimatedQty AND FSSODet.BillingRule <> 'TIME')
);

UPDATE FSSODet
SET 
  Status = 'CP'
FROM FSSODet
INNER JOIN FSServiceOrder
  ON (FSServiceOrder.CompanyID = FSSODet.CompanyID
      AND FSServiceOrder.SOID = FSSODet.SOID)
WHERE FSServiceOrder.Status = 'C';

UPDATE FSSODet
SET 
  Status = 'CC'
FROM FSSODet
INNER JOIN FSServiceOrder
  ON (FSServiceOrder.CompanyID = FSSODet.CompanyID
      AND FSServiceOrder.SOID = FSSODet.SOID)
WHERE FSServiceOrder.Status = 'X';
GO

--[IfExists(Column = FSAppointmentDet.Status)]
--[IfExists(Column = FSAppointment.Status)]
--[MinVersion(Hash = 100b2de872cf17d8aa9e1df6bff024eb335652dc03ff8273cfa68638cd5098bd)]
UPDATE FSAppointmentDet 
SET Status='CP'
FROM FSAppointmentDet
INNER JOIN FSAppointment 
ON(FSAppointment.CompanyID = FSAppointmentDet.CompanyID
AND FSAppointment.SrvOrdType = FSAppointmentDet.SrvOrdType
AND FSAppointment.RefNbr = FSAppointmentDet.RefNbr)
WHERE
  (FSAppointment.Status = 'C' OR FSAppointment.Status = 'Z')
  AND (FSAppointmentDet.Status = 'NS' OR FSAppointmentDet.Status = 'IP')
GO

--[IfExists(Column = FSSrvOrdType.OnStartApptStartServiceAndStaff)]
--[IfExists(Column = FSSrvOrdType.OnStartApptSetNotStartItemInProcess)]
--[MinVersion(Hash = b60332fef4d32284774e0daad82af230dea09ad74dbbf76abf9f00c19049b7b9)]
UPDATE FSSrvOrdType 
SET OnStartApptSetNotStartItemInProcess = OnStartApptStartServiceAndStaff
WHERE OnStartApptSetNotStartItemInProcess IS NULL;
GO

--[IfExists(Column = FSAppointment.EmployeeLineCntr)]
--[MinVersion(Hash = 6b921467c81da23e509d5e681de0ee5f3905421675ac9db61fd63f5c8f5f896f)]
UPDATE FSAppointment
  SET EmployeeLineCntr = ISNULL((SELECT MAX(LineNbr)
                    FROM  FSAppointmentEmployee
                    WHERE FSAppointmentEmployee.AppointmentID = FSAppointment.AppointmentID
                        AND FSAppointmentEmployee.CompanyID = FSAppointment.CompanyID), 0)
FROM FSAppointment
WHERE EmployeeLineCntr IS NULL;

--[IfExists(Column = FSAppointment.PrimaryDriver)]
--[MinVersion(Hash = 1e7f6c3f42310abcee647c04b968dd6f0ecf9279f4dfe3e5e6207ac97daf558f)]
UPDATE FSAppointment
SET PrimaryDriver = (SELECT TOP 1 FSAppointmentEmployee.EmployeeID FROM FSAppointmentEmployee
                    WHERE FSAppointmentEmployee.CompanyID = FSAppointment.CompanyID
                    AND FSAppointmentEmployee.SrvOrdType = FSAppointment.SrvOrdType
                    AND FSAppointmentEmployee.RefNbr = FSAppointment.RefNbr
                    AND FSAppointmentEmployee.PrimaryDriver = 1)
WHERE PrimaryDriver IS NULL;
GO

--[IfExists(Column = FSAppointment.ROLunchBreakDuration)]
--[MinVersion(Hash = 7f43ce8286c40b260f2ffce3caef59e8e5d75ed205e210ed79d219a4d3521d1f)]
UPDATE FSSetup
SET 
  ROLunchBreakDuration = 60,
  ROLunchBreakStartTimeFrame = '1900-01-01 12:00:00.000',
  ROLunchBreakEndTimeFrame = '1900-01-01 14:00:00.000'
WHERE ROLunchBreakDuration IS NULL;
GO

--[MinVersion(Hash = a2f0ac9334e0223736d3c2204a57e838e29c84009e00a0bbb969946b7cfc66e6)]
UPDATE FSServiceOrder
SET
  CuryApptOrderTotal = SUMSODet.CuryApptTranAmt,
  ApptOrderTotal = SUMSODet.ApptTranAmt
FROM FSServiceOrder
INNER JOIN 
(
  SELECT
  CompanyID, SrvOrdType, RefNbr, CuryApptTranAmt = SUM(CuryApptTranAmt), ApptTranAmt = SUM(CuryApptTranAmt)
  FROM FSSODet
  GROUP BY FSSODet.CompanyID, FSSODet.SrvOrdType, FSSODet.RefNbr
) SUMSODet
ON
SUMSODet.CompanyID = FSServiceOrder.CompanyID
AND SUMSODet.SrvOrdType = FSServiceOrder.SrvOrdType
AND SUMSODet.RefNbr = FSServiceOrder.RefNbr;
GO

--[IfExists(Column = FSLog.CuryInfoID)]
--[MinVersion(Hash = ca55aa8edc6cf3d406446c8e10e16cf9fe1354d245aafcc5e38b270f976455cb)]
UPDATE FSLog
SET FSLog.CuryInfoID = FSAppointment.CuryInfoID
FROM FSLog
INNER JOIN FSAppointment ON FSLog.CompanyID = FSAppointment.CompanyID AND
                            FSLog.DocType = FSAppointment.SrvOrdType AND
                            FSLog.DocRefNbr = FSAppointment.RefNbr
WHERE FSLog.CuryInfoID IS NULL;
GO

--[IfExists(Column = FSSOEmployee.CuryInfoID)]
--[MinVersion(Hash = 2d736fc5f265e7a73c7bdbb55bebc40031a26a696114e19e9fb24802c5ef06ee)]
UPDATE FSSOEmployee
SET FSSOEmployee.CuryInfoID = FSServiceOrder.CuryInfoID
FROM FSSOEmployee
INNER JOIN FSServiceOrder ON FSSOEmployee.CompanyID = FSServiceOrder.CompanyID AND
                             FSSOEmployee.SrvOrdType = FSServiceOrder.SrvOrdType AND
                             FSSOEmployee.RefNbr = FSServiceOrder.RefNbr
WHERE FSSOEmployee.CuryInfoID IS NULL;
GO


