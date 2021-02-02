if exists(select * from sys.objects where object_id = object_id('dbo.pp_DropConstraint') and objectproperty(object_id,'IsProcedure')=1)
drop proc dbo.pp_DropConstraint
go
create proc dbo.pp_DropConstraint @objectname sysname, @colname sysname as
declare @constname sysname, @stmt nvarchar(max)

select 
  @constname = name 
  from sys.objects where object_id = (
      select object_id from sys.default_constraints where parent_object_id = object_id('dbo.[' + @objectname + ']') and parent_column_id = (
        select column_id from sys.columns where object_id = object_id('dbo.[' + @objectname + ']') and name = @colname))
if @@rowcount = 1
begin
  set @stmt = 'alter table dbo.['+@objectname+'] drop constraint [' + @constname + ']'
  exec sp_executesql @stmt
end
go

if exists(select * from sys.objects where object_id = object_id('dbo.pp_AfterUpgrade') and objectproperty(object_id,'IsProcedure')=1)
drop proc dbo.pp_AfterUpgrade
go

if exists(select * from sys.objects where object_id = object_id('dbo.pp_DisableFullText') and objectproperty(object_id,'IsProcedure')=1)
drop proc dbo.pp_DisableFullText
go
create procedure dbo.pp_DisableFullText as
go

if exists(select * from sys.objects where object_id = object_id('dbo.pp_EnableFullText') and objectproperty(object_id,'IsProcedure')=1)
drop proc dbo.pp_EnableFullText
go
create procedure dbo.pp_EnableFullText as
go

if exists(select * from sys.objects where object_id = object_id('dbo.pp_BeforeUpgrade') and objectproperty(object_id,'IsProcedure')=1)
drop proc dbo.pp_BeforeUpgrade
go
create procedure dbo.pp_BeforeUpgrade @templateCompanyID int as
declare @tableName sysname
declare @stmt nvarchar(max)
declare c cursor for select so.name from sys.columns sc
  inner join sys.objects so on so.object_id = sc.object_id
  where sc.name='CompanyID' and so.type = 'U' and so.schema_id = schema_id('dbo')
open c
fetch c into @tableName
while @@fetch_status >= 0 begin
  set @stmt = N'alter table dbo.[' + @tableName + '] NOCHECK CONSTRAINT ALL'
  exec sp_executesql @stmt
  fetch c into @tableName
end
close c
deallocate c
declare c cursor for select so.name from sys.columns sc
  inner join sys.objects so on so.object_id = sc.object_id
  where sc.name='CompanyID' and so.type = 'U' and so.schema_id = schema_id('dbo')
open c
fetch c into @tableName
while @@fetch_status >= 0 begin
  set @stmt = N'delete dbo.[' + @tableName + '] where CompanyID = ' + convert(varchar(10), @templateCompanyID)
  exec sp_executesql @stmt
  fetch c into @tableName
end
close c
deallocate c

go

if exists(select * from sys.objects where object_id = object_id('dbo.pp_ChangeCompany') and objectproperty(object_id,'IsProcedure')=1)
drop proc dbo.pp_ChangeCompany
go
create procedure dbo.pp_ChangeCompany @oldCompanyID int, @newCompanyID int as
declare @name sysname
declare @stmt nvarchar(max)
declare c cursor for select so.name from sys.columns sc
  inner join sys.objects so on so.object_id = sc.object_id
  where sc.name='CompanyID' and so.type = 'U' and so.schema_id = schema_id('dbo')
open c
fetch c into @name
while @@fetch_status >= 0 begin
  set @stmt = N'alter table dbo.[' + @name + '] NOCHECK CONSTRAINT ALL'
  exec sp_executesql @stmt
  fetch c into @name
end
close c
deallocate c
declare c cursor for select so.name from sys.columns sc
  inner join sys.objects so on so.object_id = sc.object_id
  where sc.name='CompanyID' and so.type = 'U' and so.schema_id = schema_id('dbo')
open c
fetch c into @name
while @@fetch_status >= 0 begin
  set @stmt = N'update dbo.[' + @name + '] set CompanyID = ' + convert(varchar(10), @newCompanyID) + ' where CompanyID = ' + convert(varchar(10), @oldCompanyID)
  exec sp_executesql @stmt
  fetch c into @name
end
close c
deallocate c
declare c cursor for select so.name from sys.columns sc
  inner join sys.objects so on so.object_id = sc.object_id
  where sc.name='CompanyID' and so.type = 'U' and so.schema_id = schema_id('dbo')
open c
fetch c into @name
while @@fetch_status >= 0 begin
  set @stmt = N'alter table dbo.[' + @name + '] CHECK CONSTRAINT ALL'
  exec sp_executesql @stmt
  fetch c into @name
end
close c
deallocate c
go

if exists(select * from sys.objects where object_id = object_id('dbo.pp_AdjustForeigns') and objectproperty(object_id,'IsProcedure')=1)
drop proc dbo.pp_AdjustForeigns
go
create procedure dbo.pp_AdjustForeigns @table sysname as
  declare @stmt nvarchar(max)
  declare @index sysname
  declare @rtable sysname
  declare indexcsr insensitive cursor for select f.name, object_name(f.referenced_object_id)
    from sys.foreign_keys f
    inner join sys.objects o on o.object_id = f.parent_object_id and o.type = 'U' and o.schema_id = schema_id('dbo')
    where f.parent_object_id = object_id('dbo.[' + @table + ']')
  open indexcsr
  fetch indexcsr into @index, @rtable
  while @@fetch_status >= 0 begin
      declare @deleted sysname
      set @deleted = null
      select @deleted = name from sys.columns where name in('DeletedDatabaseRecord', 'UsrDeletedDatabaseRecord') and object_id = object_id('dbo.[' + @rtable + ']')
      declare @masked sysname
      set @masked = null
      select @masked = name from sys.columns where name = 'CompanyMask' and object_id = object_id('dbo.[' + @rtable + ']')
      declare @dropanyway sysname
      set @dropanyway = null
      select @dropanyway = name from sys.columns where name = 'CompanyMask' and object_id = object_id('dbo.[' + @table + ']')
      declare @primary varchar(max)
      set @primary = null
      declare @indid smallint
      set @indid = null
      declare @pkname sysname
      select @indid = index_id, @pkname = name from sys.indexes where is_primary_key = 1 and object_id = object_id('dbo.[' + @rtable + ']')
      if @indid is not null begin
        declare @k int
        set @k = 1
        declare @nextcol sysname
        while @k > 0 begin
          select @nextcol = index_col('dbo.[' + @rtable + ']', @indid, @k)
          if @nextcol is not null begin
            if @primary is null
              set @primary = 'i.[' + @nextcol + '] = d.[' + @nextcol + ']'
            else
              set @primary = @primary + ' and i.[' + @nextcol + '] = d.[' + @nextcol + ']'
            set @k = @k + 1
          end else
            set @k = -1
        end
      end
      declare @fcol sysname
      declare @rcol sysname
      declare @link varchar(max)
      set @link = null
      declare @companypresent bit
      set @companypresent = 0
      declare colcsr cursor for select col_name(object_id(@table), c.parent_column_id), col_name(object_id(@rtable), c.referenced_column_id)
      from sys.foreign_key_columns c
      where c.constraint_object_id = object_id(@index)
      order by c.constraint_column_id
      open colcsr
      fetch colcsr into @fcol, @rcol
      while @@fetch_status >= 0 begin
        if @fcol <> 'CompanyID' begin
          if @link is null
            set @link = 't.[' + @fcol + '] = d.[' + @rcol + ']'
          else
            set @link = @link + ' and t.[' + @fcol + '] = d.[' + @rcol + ']'
        end else
          set @companypresent = 1
        fetch colcsr into @fcol, @rcol
      end
      close colcsr
      deallocate colcsr
      if @deleted is not null or @masked is not null or @dropanyway is not null begin
        set @stmt = 'alter table dbo.[' + @table + '] drop constraint [' + @index + ']'
        exec sp_executesql @stmt
      end
      if @primary is not null and @link is not null begin
        declare @rdeleted sysname
        set @rdeleted = null
        select @rdeleted = name from sys.columns where name in ('DeletedDatabaseRecord', 'UsrDeletedDatabaseRecord') and object_id = object_id('dbo.[' + @table + ']')
        if @rdeleted is not null
          set @link = @link + ' and t.[' + @rdeleted + '] = 0'
        if @deleted is not null begin
          set @stmt = 'create trigger dbo.[' + @index + case when @masked is not null then '_3' else '_2' end + '] on dbo.[' + @rtable + '] for update as' + char(10)
          set @stmt = @stmt + '  set nocount on' + char(10)
          set @stmt = @stmt + '  if (select count(*) from deleted) <> 1 return' + char(10)
          if @masked is not null and @companypresent = 1 begin
            set @stmt = @stmt + '  declare @companyID int' + char(10)
            set @stmt = @stmt + '  declare @companyMask varbinary(max)' + char(10)
            set @stmt = @stmt + '  select @companyID = CompanyID, @companyMask = ' + @masked + ' from deleted where ' + @deleted + ' = 0' + char(10)
            set @stmt = @stmt + '  if @companyID is null return' + char(10)
            set @stmt = @stmt + '  declare @matrix table (CompanyID int not null, primary key clustered (CompanyID))' + char(10)
            set @stmt = @stmt + '  insert @matrix select CompanyID from dbo.pp_GetMatrix(@companyID, @companyMask)' + char(10)
            set @stmt = @stmt + '  if exists(select * from deleted d' + char(10)
            set @stmt = @stmt + '    inner join inserted i on ' + @primary + ' and d.[' + @deleted + '] = 0 and i.[' + @deleted + '] = 1' + char(10)
            set @stmt = @stmt + '    cross join @matrix m' + char(10)
            set @stmt = @stmt + '    inner join dbo.[' + @table + '] t on t.[CompanyID] = m.[CompanyID] and ' + @link + ')' + char(10)
          end else begin
            set @stmt = @stmt + '  if exists(select * from deleted d' + char(10)
            set @stmt = @stmt + '    inner join inserted i on ' + @primary + ' and d.[' + @deleted + '] = 0 and i.[' + @deleted + '] = 1' + char(10)
            set @stmt = @stmt + '    inner join dbo.[' + @table + case when @companypresent = 1 then '] t on t.[CompanyID] = d.[CompanyID] and ' else '] t on ' end + @link + ')' + char(10)
          end
          set @stmt = @stmt + '  raiserror(''''''' + @table + ''''' exists for this ''''' + @rtable + '''''.'', 16, 1)' + char(10)
          exec sp_executesql @stmt
        end else if @masked is not null begin
          set @stmt = 'create trigger dbo.[' + @index + '_1] on dbo.[' + @rtable + '] for delete as' + char(10)
          set @stmt = @stmt + '  set nocount on' + char(10)
          set @stmt = @stmt + '  if (select count(*) from deleted) <> 1 return' + char(10)
          if @companypresent = 1 begin
            set @stmt = @stmt + '  declare @companyID int' + char(10)
            set @stmt = @stmt + '  declare @companyMask varbinary(max)' + char(10)
            set @stmt = @stmt + '  select @companyID = CompanyID, @companyMask = ' + @masked + ' from deleted' + char(10)
            set @stmt = @stmt + '  if @companyID is null return' + char(10)
            set @stmt = @stmt + '  declare @matrix table (CompanyID int not null, primary key clustered (CompanyID))' + char(10)
            set @stmt = @stmt + '  insert @matrix select CompanyID from dbo.pp_GetMatrix(@companyID, @companyMask)' + char(10)
            set @stmt = @stmt + '  if exists(select * from deleted d' + char(10)
            set @stmt = @stmt + '    cross join @matrix m' + char(10)
            set @stmt = @stmt + '    inner join dbo.[' + @table + '] t on t.[CompanyID] = m.[CompanyID] and ' + @link + ')' + char(10)
          end else begin
            set @stmt = @stmt + '  if exists(select * from deleted d' + char(10)
            set @stmt = @stmt + '    inner join dbo.[' + @table + '] t on ' + @link + ')' + char(10)
          end
          set @stmt = @stmt + '  raiserror(''''''' + @table + ''''' exists for this ''''' + @rtable + '''''.'', 16, 1)' + char(10)
          exec sp_executesql @stmt
        end else if @dropanyway is not null begin
          set @stmt = 'create trigger dbo.[' + @index + '] on dbo.[' + @rtable + '] for delete as' + char(10)
          set @stmt = @stmt + '  set nocount on' + char(10)
          set @stmt = @stmt + '  if (select count(*) from deleted) <> 1 return' + char(10)
          set @stmt = @stmt + '  if exists(select * from deleted d' + char(10)
          set @stmt = @stmt + '    inner join dbo.[' + @table + '] t on t.[CompanyID] = d.[CompanyID] and ' + @link + ')' + char(10)
          set @stmt = @stmt + '  raiserror(''''''' + @table + ''''' exists for this ''''' + @rtable + '''''.'', 16, 1)' + char(10)
          exec sp_executesql @stmt
        end
      end
    fetch indexcsr into @index, @rtable
  end
  close indexcsr
  deallocate indexcsr
go

if exists(select * from sys.objects where object_id = object_id('dbo.pp_SplitTable') and objectproperty(object_id,'IsProcedure')=1)
drop procedure dbo.pp_SplitTable
go
create procedure dbo.pp_SplitTable @tableName sysname, @option int as
  declare @stmt nvarchar(max)
  declare @maxID int
  select @maxID = max(CompanyID) from company
  if(@maxID is null) set @maxID = 1  
  declare @def sysname
  declare @con sysname
  select @def = sdc.definition, @con = so.name
    from sys.default_constraints sdc
    inner join sys.objects so on so.object_id = sdc.object_id
    inner join sys.columns sc on sdc.parent_column_id=sc.column_id and so.parent_object_id=sc.object_id 
    where sc.name='CompanyMask' and so.type='D' and sc.object_id=object_id('dbo.[' + @tableName + ']')
  if @option = 0 begin
    if @def is not null and substring(@def, 1, 4) <> '(0x0' begin
      set @stmt = 'alter table dbo.[' + @tableName + '] drop constraint [' + @con + ']'
      exec sp_executesql @stmt
      set @stmt = 'alter table dbo.[' + @tableName + '] add constraint [' + @con + '] default 0x' + replicate('0', ((@maxID - 1) / 4 + 1) * 2) + ' for CompanyMask'
      exec sp_executesql @stmt
    end
  end else begin
    if @def is null begin
      declare @ident sysname
      declare @type int
      select @ident=name, @type=system_type_id from sys.identity_columns where object_id = object_id('dbo.[' + @tableName + ']')
      if @ident is not null and substring(@ident, 1, 7) <> 'Company' begin
        declare @primary sysname
        select @primary = name from sys.objects where parent_object_id = object_id('dbo.[' + @tableName + ']') and type='PK'
        if @primary is not null begin
          declare @indid smallint
          select @indid=index_id from sys.indexes where name=@primary
          declare @cols varchar(200)
          set @cols = ''
          declare @k int
          set @k = 1
          declare @nextcol sysname
          while @k > 0 begin
            select @nextcol = index_col('dbo.[' + @tableName + ']', @indid, @k)
            if @nextcol is not null begin
              if @cols = ''
                set @cols = '[' + @nextcol + ']'
              else
                set @cols = @cols + ', [' + @nextcol + ']'
              set @k = @k + 1
            end else
              set @k = -1
          end
          set @stmt = 'exec sp_rename ''dbo.[' + @tableName + '].[' + @ident + ']'', ''Company' + @ident + ''''
          exec sp_executesql @stmt
          set @stmt = 'exec sp_rename ''dbo.[' + @primary + ']'', ''Usr' + @primary + ''''
          exec sp_executesql @stmt
          set @stmt = 'alter table dbo.[' + @tableName + '] add [' + @ident + '] '
          if @type = 127
            set @stmt = @stmt + 'bigint'
          else
            set @stmt = @stmt + 'int'
          set @stmt = @stmt + ' default ident_current(''' + @tableName + ''')'
          exec sp_executesql @stmt
          set @stmt = 'update dbo.[' + @tableName + '] set [' + @ident + '] = [Company' + @ident + ']'
          exec sp_executesql @stmt
          set @stmt = 'alter table dbo.[' + @tableName + '] alter column [' + @ident + '] '
          if @type = 127
            set @stmt = @stmt + 'bigint not null'
          else
            set @stmt = @stmt + 'int not null'
          exec sp_executesql @stmt
          set @stmt = 'create unique index [' + @primary + '] on dbo.[' + @tableName + '] (' + @cols + ')'
          exec sp_executesql @stmt
        end
      end
      set @stmt = N'alter table dbo.[' + @tableName + ']'
                  + ' add CompanyMask varbinary('+ cast(32 * ( 1 + @maxID / 128 ) as varchar(10)) + ') not null default 0x' 
                  + replicate(case @option when 1 then 'A' else 'F' end, ((@maxID - 1) / 4 + 1) * 2)
      exec sp_executesql @stmt
      set @stmt = N'alter table dbo.[' + @tableName + ']'
                  + ' add UsrCompanyMask varbinary(1) null' 
      exec sp_executesql @stmt
      declare @triggerdrop nvarchar(max)
      declare @triggercreate nvarchar(max)
      declare triggercsr insensitive cursor for select 'drop trigger dbo.[' + t.name + ']',
      replace(replace(replace(replace(definition, 'inner join dbo.[', 'cross join @matrix m
    inner join dbo.['),
      '] t on t.[CompanyID] = d.[CompanyID]', '] t on t.[CompanyID] = m.[CompanyID]'),
      'if exists(select * from deleted d',
      'declare @companyID int
  declare @companyMask varbinary(max)
  select @companyID = CompanyID, @companyMask = CompanyMask from deleted where DeletedDatabaseRecord = 0
  if @companyID is null return
  declare @matrix table (CompanyID int not null, primary key clustered (CompanyID))
  insert @matrix select CompanyID from dbo.pp_GetMatrix(@companyID, @companyMask)
  if exists(select * from deleted d'),
        '_2] on dbo.[',
        '_3] on dbo.[')
      from sys.triggers t
      inner join sys.sql_modules m on m.object_id = t.object_id
      where t.parent_id = object_id('dbo.[' + @tableName + ']') and t.name like '%[_]2' and t.name not like 'Usr%'
      open triggercsr
      fetch triggercsr into @triggerdrop, @triggercreate
      while @@fetch_status >= 0 begin
        exec sp_executesql @triggerdrop
        exec sp_executesql @triggercreate
        fetch triggercsr into @triggerdrop, @triggercreate
      end
      close triggercsr
      deallocate triggercsr
      declare @foreignTable sysname
      declare crs insensitive cursor for select distinct so.name from sys.foreign_keys sf
        inner join sys.objects so on so.object_id = sf.parent_object_id
        where sf.referenced_object_id = object_id('dbo.[' + @tableName + ']') and so.type = 'U' and so.schema_id = schema_id('dbo')
      open crs
      fetch crs into @foreignTable
      while @@fetch_status >= 0 begin
        set @stmt = 'exec pp_AdjustForeigns ''' + @foreignTable + ''''
        exec sp_executesql @stmt
        fetch crs into @foreignTable
      end
      close crs
      deallocate crs
      set @stmt = 'exec pp_AdjustForeigns ''' + @tableName + ''''
      exec sp_executesql @stmt
    end else begin
      if @option = 1 and substring(@def, 1, 4) <> '(0xA' begin
        set @stmt = 'alter table dbo.[' + @tableName + '] drop constraint [' + @con + ']'
        exec sp_executesql @stmt
        set @stmt = 'alter table dbo.[' + @tableName + '] add constraint [' + @con + '] default 0x' + replicate('A', ((@maxID - 1) / 4 + 1) * 2) + ' for CompanyMask'
        exec sp_executesql @stmt
      end else if @option = 2 and substring(@def, 1, 4) <> '(0xF' begin
        set @stmt = 'alter table dbo.[' + @tableName + '] drop constraint [' + @con + ']'
        exec sp_executesql @stmt
        set @stmt = 'alter table dbo.[' + @tableName + '] add constraint [' + @con + '] default 0x' + replicate('F', ((@maxID - 1) / 4 + 1) * 2) + ' for CompanyMask'
        exec sp_executesql @stmt
      end
    end
  end
go


-- discard old family of functions
if exists(select * from sys.objects where object_id = object_id('dbo.pp_MaskBinarySet') and objectproperty(object_id,'IsScalarFunction')=1)
drop FUNCTION dbo.pp_MaskBinarySet
go
if exists(select * from sys.objects where object_id = object_id('dbo.pp_MaskBinaryClear') and objectproperty(object_id,'IsScalarFunction')=1)
drop FUNCTION dbo.pp_MaskBinaryClear
go
if exists(select * from sys.objects where object_id = object_id('dbo.pp_MaskBinaryTest') and objectproperty(object_id,'IsScalarFunction')=1)
drop FUNCTION dbo.pp_MaskBinaryTest
go

-- use new ones
if exists(select * from sys.objects where object_id = object_id('dbo.binaryMaskAdd') and objectproperty(object_id,'IsScalarFunction')=1)
drop FUNCTION dbo.binaryMaskAdd
go

CREATE FUNCTION dbo.binaryMaskAdd(@source varbinary(2048), @companyId int, @flags tinyint)
RETURNS varbinary(2048)
AS
BEGIN
	if @companyId <= 0 or @companyId > 2048*4 
		return @source

	declare @MASK_SIZE int
	set @MASK_SIZE = 2048
	declare @bytePos int 
	set @bytePos = ( @companyId + 3 ) / 4
	declare @addedValue tinyint 
	set @addedValue = case @companyId % 4 when 1 then @flags when 2 then @flags * 4 when 3 then @flags * 16 else @flags * 64 end
	if len(@source) < @bytePos set @source = @source + convert(varbinary(2048), REPLICATE( 0x00, @bytePos - len(@source)))

	return case when @bytePos > 1 then substring(@source, 1, @bytePos - 1) else 0x end 
		 + convert(binary(1), @addedValue | convert(binary(1), substring(@source, @bytePos, 1)) )
		 + case when @MASK_SIZE > @bytePos then substring(@source, @bytePos + 1, @MASK_SIZE - @bytePos) else 0x end
END
GO

if exists(select * from sys.objects where object_id = object_id('dbo.binaryMaskSub') and objectproperty(object_id,'IsScalarFunction')=1)
drop FUNCTION dbo.binaryMaskSub
go

CREATE FUNCTION dbo.binaryMaskSub(@source varbinary(2048), @companyId int, @flags tinyint)
RETURNS varbinary(2048)
AS
BEGIN
	if @companyId <= 0 or @companyId > 2048*4
		return @source

	declare @MASK_SIZE int
	set @MASK_SIZE = 2048
	declare @bytePos int 
	set @bytePos = ( @companyId + 3 ) / 4
	declare @mask tinyint
	set @mask = 255 - case @companyId % 4 when 1 then @flags when 2 then @flags * 4 when 3 then @flags * 16 else @flags * 64 end

	RETURN case when @bytePos > 1 then substring(@source, 1, @bytePos - 1) else 0x end 
		 + convert(binary(1), @mask & convert(binary(1), substring(@source, @bytePos, 1)))
		 + case when @MASK_SIZE > @bytePos then substring(@source, @bytePos + 1, @MASK_SIZE - @bytePos) else 0x end
END
GO

if exists(select * from sys.objects where object_id = object_id('dbo.binaryMaskSet') and objectproperty(object_id,'IsScalarFunction')=1)
drop FUNCTION dbo.binaryMaskSet
go

CREATE FUNCTION dbo.binaryMaskSet(@source varbinary(2048), @companyId int, @flags tinyint)
RETURNS varbinary(2048)
AS
BEGIN
	if @companyId <= 0 or @companyId > 2048*4
		return @source

	declare @MASK_SIZE int
	set @MASK_SIZE = 2048
	declare @bytePos int 
	set @bytePos = ( @companyId + 3 ) / 4
	declare @mask tinyint 
	set @mask = case @companyId % 4 when 1 then 252 when 2 then 243 when 3 then 207 else 63 end
	if len(@source) < @bytePos set @source = @source + convert(varbinary(2048), REPLICATE( 0x00, @bytePos - len(@source)))

	declare @newValue tinyint
	set @newValue = case @companyId % 4 when 1 then @flags when 2 then @flags * 4 when 3 then @flags * 16 else @flags * 64 end

	RETURN case when @bytePos > 1 then substring(@source, 1, @bytePos - 1) else 0x end 
		 + convert(binary(1), @newValue | (convert(binary(1), substring(@source, @bytePos, 1)) & @mask))
		 + case when @MASK_SIZE > @bytePos then substring(@source, @bytePos + 1, @MASK_SIZE - @bytePos) else 0x end
END
GO

if exists(select * from sys.objects where object_id = object_id('dbo.binaryMaskCopy') and objectproperty(object_id,'IsScalarFunction')=1)
	drop FUNCTION dbo.binaryMaskCopy
go

CREATE FUNCTION dbo.binaryMaskCopy(@source varbinary(2048), @companyFrom int, @companyTo int)
RETURNS varbinary(2048)
AS
BEGIN
	if @companyFrom <= 0 or @companyFrom > 2048*4 or @companyTo <= 0 or @companyTo > 2048*4 
		return @source

	declare @MASK_SIZE int
	set @MASK_SIZE = 2048
	
	-- read mask from source company
	declare @bytePosFrom int 
	set @bytePosFrom = ( @companyFrom + 3 ) / 4
	declare @divisor tinyint
	set @divisor = CASE @companyFrom % 4 WHEN 1 THEN 1 WHEN 2 THEN 4 WHEN 3 THEN 16 ELSE 64 END
	declare @srcPosMask tinyint
	set @srcPosMask = CASE @companyFrom % 4 WHEN 1 THEN 3 WHEN 2 THEN 12 WHEN 3 THEN 48 ELSE 192 END

	declare @mask tinyint
	set @mask = ( convert(binary(1), substring(@source, @bytePosFrom, 1)) & @srcPosMask ) / @divisor

	-- set mask into desired value
	declare @bytePosTo int 
	set @bytePosTo = ( @companyTo + 3 ) / 4
	DECLARE @dstNegMask int
	set @dstNegMask = CASE @companyTo % 4 WHEN 1 THEN 252 WHEN 2 THEN 243 WHEN 3 THEN 207 ELSE 63 END
	DECLARE @dstMultiplier tinyint
	set @dstMultiplier = CASE @companyTo % 4 WHEN 1 THEN 1 WHEN 2 THEN 4 WHEN 3 THEN 16 ELSE 64 END

	declare @changedByte tinyint
	set @changedByte = (convert(binary(1), substring(@source, @bytePosTo, 1)) & @dstNegMask) | ( @mask * @dstMultiplier )

	if len(@source) < @bytePosTo 
		set @source = @source + convert(varbinary(2048), REPLICATE( 0x00, @bytePosTo - len(@source)))

	return case when @bytePosTo > 1 then substring(@source, 1, @bytePosTo - 1) else 0x end 
		 + convert(binary(1), @changedByte)
		 + case when @MASK_SIZE > @bytePosTo then substring(@source, @bytePosTo + 1, @MASK_SIZE - @bytePosTo) else 0x end
END
GO



/* unit-test: 
select dbo.binaryMaskSub(0xAAAAAA, 8, 1) as [sub], dbo.binaryMaskSet(0xAAAAAA, 8, 1) as [set], dbo.binaryMaskAdd(0xAAAAAA, 8, 1) as [add]
union all
select dbo.binaryMaskSub(0xAAAAAA, 8, 2), dbo.binaryMaskSet(0xAAAAAA, 8, 2), dbo.binaryMaskAdd(0xAAAAAA, 8, 2)
union all
select dbo.binaryMaskSub(0xAAAAAA, 8, 3), dbo.binaryMaskSet(0xAAAAAA, 8, 3), dbo.binaryMaskAdd(0xAAAAAA, 8, 3)
union all
select dbo.binaryMaskSub(0xAAAA, 8, 1) as [sub], dbo.binaryMaskSet(0xAAAA, 8, 1) as [set], dbo.binaryMaskAdd(0xAAAA, 8, 1) as [add]
union all
select dbo.binaryMaskSub(0xAAAA, 8, 2), dbo.binaryMaskSet(0xAAAA, 8, 2), dbo.binaryMaskAdd(0xAAAA, 8, 2)
union all
select dbo.binaryMaskSub(0xAAAA, 8, 3), dbo.binaryMaskSet(0xAAAA, 8, 3), dbo.binaryMaskAdd(0xAAAA, 8, 3)
union all
select dbo.binaryMaskSub(0xAAAA, 4, 1) as [sub], dbo.binaryMaskSet(0xAAAA, 4, 1) as [set], dbo.binaryMaskAdd(0xAAAA, 4, 1) as [add]
union all
select dbo.binaryMaskSub(0xAAAA, 4, 2), dbo.binaryMaskSet(0xAAAA, 4, 2), dbo.binaryMaskAdd(0xAAAA, 4, 2)
union all
select dbo.binaryMaskSub(0xAAAA, 4, 3), dbo.binaryMaskSet(0xAAAA, 4, 3), dbo.binaryMaskAdd(0xAAAA, 4, 3)
union all
select dbo.binaryMaskSub(0xAA, 4, 1) as [sub], dbo.binaryMaskSet(0xAA, 4, 1) as [set], dbo.binaryMaskAdd(0xAA, 4, 1) as [add]
union all
select dbo.binaryMaskSub(0xAA, 4, 2), dbo.binaryMaskSet(0xAA, 4, 2), dbo.binaryMaskAdd(0xAA, 4, 2)
union all
select dbo.binaryMaskSub(0xAA, 4, 3), dbo.binaryMaskSet(0xAA, 4, 3), dbo.binaryMaskAdd(0xAA, 4, 3)

should return int last 3 rows - {row 1: {0xAA, 0x6A, 0xEA}, row 2: {0x2A, 0xAA, 0xAA}, row 3: {0x2A, 0xEA, 0xEA}}
*/


if exists(select * from sys.objects where object_id = object_id('dbo.binaryMaskTest') and objectproperty(object_id,'IsScalarFunction')=1)
drop FUNCTION dbo.binaryMaskTest
go
CREATE FUNCTION [dbo].[binaryMaskTest](@source varbinary(2048), @companyId int, @flag tinyint)
RETURNS bit
AS
BEGIN
	if @companyId <= 0 
	  return 0

	declare @mask tinyint
	set @mask = case @companyId % 4 when 1 then @flag when 2 then @flag * 4 when 3 then @flag * 16 else @flag * 64 end

	RETURN CASE (substring(@source, ( @companyId + 3 ) / 4, 1) & @mask) WHEN @mask then 1 else 0 end
END
go

if exists(select * from sys.objects where object_id = object_id('dbo.pp_EnableTriggers') and objectproperty(object_id,'IsProcedure')=1)
drop proc dbo.pp_EnableTriggers
go
create procedure dbo.pp_EnableTriggers @enable bit as
	declare @trigname sysname
	declare @trigtable sysname
	declare @trigstmt nvarchar(max)
	declare @enableString nvarchar(10)
	
	SET @enableString = CASE WHEN @enable = 1 THEN 'ENABLE' ELSE 'DISABLE' END
  
	declare ctrig cursor for select tr.name, tb.name from sys.triggers tr
		join sys.tables tb on tr.parent_id = tb.object_id
		where tr.is_Disabled = @enable
	open ctrig
	fetch ctrig into @trigname, @trigtable
	while @@fetch_status >= 0
	BEGIN
		set @trigstmt = @enableString + ' TRIGGER [' + @trigname + '] ON dbo.[' + @trigtable + ']'
		EXEC sp_executesql @trigstmt
		fetch ctrig into @trigname, @trigtable
	END
	close ctrig
	deallocate ctrig
go

if exists(select * from sys.objects where object_id = object_id('dbo.pp_DisableConstraints') and objectproperty(object_id,'IsProcedure')=1)
drop proc dbo.pp_DisableConstraints
go
create procedure dbo.pp_DisableConstraints as
BEGIN
  declare @name sysname
  declare @stmt nvarchar(max)

  --disabling constraints
  declare c cursor for select so.name from sys.columns sc
    inner join sys.objects so on so.object_id = sc.object_id
    where sc.name='CompanyID' and so.type = 'U' and so.schema_id = schema_id('dbo')
  open c
  fetch c into @name
  while @@fetch_status >= 0 begin
    set @stmt = N'alter table dbo.[' + @name + '] NOCHECK CONSTRAINT ALL'
    exec sp_executesql @stmt
    fetch c into @name
  end
  close c
  deallocate c
  
  --disabling triggers
  exec pp_EnableTriggers 0
END
GO

if exists(select * from sys.objects where object_id = object_id('dbo.pp_EnableConstraints') and objectproperty(object_id,'IsProcedure')=1)
drop proc dbo.pp_EnableConstraints
go
create procedure dbo.pp_EnableConstraints as
BEGIN
  declare @name sysname
  declare @stmt nvarchar(max)

  --enabling constraints
  declare c cursor for select so.name from sys.columns sc
    inner join sys.objects so on so.object_id = sc.object_id
    where sc.name='CompanyID' and so.type = 'U' and so.schema_id = schema_id('dbo')
  open c
  fetch c into @name
  while @@fetch_status >= 0 begin
    set @stmt = N'alter table dbo.[' + @name + '] CHECK CONSTRAINT ALL'
    exec sp_executesql @stmt
    fetch c into @name
  end
  close c
  deallocate c

  --enabling triggers
  exec pp_EnableTriggers 1
END
GO


if exists(select * from sys.objects where object_id = object_id('dbo.pp_BeforeDataReplacement') and objectproperty(object_id,'IsProcedure')=1)
drop proc dbo.pp_BeforeDataReplacement
go
create procedure dbo.pp_BeforeDataReplacement @companyID int, @specificTables varchar(MAX) = NULL as
  declare @name sysname
  declare @stmt nvarchar(max)
  declare @reverceID int

  --removing reverce company
  set @reverceID = -@companyID
  exec pp_DeleteCompany @reverceID

  --if we using some specific tables
  CREATE TABLE #specificTablesList (tableName sysname COLLATE DATABASE_DEFAULT)
  if(@specificTables is not null) BEGIN
    SET @specificTables = REPLACE(@specificTables, ' ', '')
	SET @stmt = 'SELECT ''' + REPLACE(@specificTables, ',', ''' UNION SELECT ''') + ''''
	INSERT #specificTablesList EXEC(@stmt)
  END

  exec pp_DisableConstraints

  declare c cursor for select so.name from sys.columns sc
    inner join sys.objects so on so.object_id = sc.object_id
    where sc.name='CompanyID' and so.type = 'U' and so.schema_id = schema_id('dbo')
		and (@specificTables is NULL or so.name in (select tableName from #specificTablesList))
  open c
  fetch c into @name
  while @@fetch_status >= 0 begin
    set @stmt = N'update dbo.[' + @name + '] set CompanyID = ' + convert(varchar(10), @reverceID) + ' where CompanyID = ' + convert(varchar(10), @companyID)
    exec sp_executesql @stmt
    fetch c into @name
  end
  close c
  deallocate c

  DROP TABLE #specificTablesList
go

if exists(select * from sys.objects where object_id = object_id('dbo.pp_AfterDataReplacement') and objectproperty(object_id,'IsProcedure')=1)
drop proc dbo.pp_AfterDataReplacement
go
create procedure dbo.pp_AfterDataReplacement @companyID int, @specificTables varchar(MAX) = NULL as
  declare @tableName sysname
  declare @stmt nvarchar(max)

   exec pp_DisableConstraints -- triggers may have been enabled after importing data

  --if we using some specific tables
  CREATE TABLE #specificTablesList (tableName sysname COLLATE DATABASE_DEFAULT)
  if(@specificTables is not null) BEGIN
    SET @specificTables = REPLACE(@specificTables, ' ', '')
	SET @stmt = 'SELECT ''' + REPLACE(@specificTables, ',', ''' UNION SELECT ''') + ''''
	INSERT #specificTablesList EXEC(@stmt)
  END 

  declare c cursor for select so.name from sys.columns sc
    inner join sys.objects so on so.object_id = sc.object_id
	left join sys.columns usc on usc.object_id = so.object_id and usc.name = 'UsrCompanyID'
    where sc.name='CompanyMask' and so.type = 'U' and so.schema_id = schema_id('dbo') and usc.object_id is null
		and (@specificTables is NULL or so.name in (select tableName from #specificTablesList))
  open c
  fetch c into @tableName
  while @@fetch_status >= 0 begin
    declare @primary sysname
    select @primary = name from sys.objects where parent_object_id = object_id('dbo.[' + @tableName + ']') and type='PK'
    if @primary is not null begin
      declare @indid smallint
      select @indid=index_id from sys.indexes where name=@primary
      declare @cols varchar(1000)
      set @cols = ''
      declare @m int
      set @m = 1
      declare @nextcol sysname
      while @m > 0 begin
        select @nextcol = index_col('dbo.[' + @tableName + ']', @indid, @m)
        if @nextcol is not null begin
          if @nextcol <> 'CompanyID' begin
            if @cols = ''
              set @cols = 'u.[' + @nextcol + '] = f.[' + @nextcol + ']'
            else
              set @cols = @cols + ' and u.[' + @nextcol + '] = f.[' + @nextcol + ']'
          end
          set @m = @m + 1
        end
        else
          set @m = -1
      end
    end
    set @stmt=N'update u set u.companymask = f.companymask'
    set @stmt=@stmt+' from dbo.[' + @tableName + '] u inner join dbo.[' + @tableName + '] f on ' + @cols
    if @cols <> ''
       set @stmt=@stmt + ' and f.companyid = ' + convert(varchar(10), -@companyID) + ' where u.CompanyID = ' + convert(varchar(10), @companyID)
    else
       set @stmt=@stmt + 'f.companyid = ' + convert(varchar(10), -@companyID) + ' where u.CompanyID = ' + convert(varchar(10), @companyID)
    exec sp_ExecuteSql @stmt
    fetch c into @tableName
  end
  close c
  deallocate c
  declare c cursor for select so.name from sys.columns sc
    inner join sys.objects so on so.object_id = sc.object_id
    where sc.name='CompanyID' and so.type = 'U' and so.schema_id = schema_id('dbo')
		and (@specificTables is NULL or so.name in (select tableName from #specificTablesList))
  open c
  fetch c into @tableName
  while @@fetch_status >= 0 begin
    set @stmt=N'delete dbo.[' + @tableName + '] where CompanyID = ' + convert(varchar(10), -@companyID)
    exec sp_ExecuteSql @stmt
    fetch c into @tableName
  end
  close c
  deallocate c
  
  exec pp_EnableConstraints
go

if exists(select * from sys.objects where object_id = object_id('dbo.pp_ReserveCompanyID') and objectproperty(object_id,'IsProcedure')=1)
drop proc dbo.pp_ReserveCompanyID
go
create procedure dbo.pp_ReserveCompanyID @companyID int AS
	declare @maxID int
	select @maxID = max(CompanyID) from company
	declare @bytesToAppendToMask int
	select @bytesToAppendToMask = (@companyID + 3) / 4 - (@maxID + 3) / 4 -- mind the automatic int rounding!

	declare @dropStmt nvarchar(max)
	declare @addStmt nvarchar(max)
	declare @updateStmt nvarchar(max)
	declare @extendStmt nvarchar(max)

	declare c INSENSITIVE cursor  for select
	N'alter table dbo.[' + sp.name + '] drop constraint ' + so.name,
	N'alter table dbo.[' + sp.name + '] add constraint ' + so.name + ' default 0x' + substring(
		replace(sdc.definition,')','') + replicate(case when sdc.definition like '(0xF%' then 'F' when sdc.definition like '(0x0%' then '0' else 'A' end, (2 + (@companyID + 3) / 4) * 2)
		, 4, ((@companyID + 3) / 4) * 2) + ' for CompanyMask',

	-- note: the default mask value will be taken from default value set for 1st and 2nd company, even if a different value is present in mask at a place assigned for @companyID
	N'update u set CompanyMask = dbo.binaryMaskSet(CompanyMask, ' + cast(@companyID as varchar(10)) + ', case CompanyId when 1 then 2 else ' + ( case when sdc.definition like '(0xF%' then '3' when sdc.definition like '(0x0%' then '0' else '2' end ) + ' end)'
	+ ' from ' + object_name(so.parent_object_id) + ' u where CompanyID > 0',
	case when sc.max_length * 4 < @companyID THEN 'ALTER TABLE dbo.[' + object_name(sc.object_id) + '] ALTER COLUMN [' + sc.name + '] VARBINARY(' + cast( 32 * ( 1 + @companyID / 128) as varchar(10) ) + ') NOT NULL' else null end

	from sys.default_constraints sdc
	inner join sys.objects so on so.object_id=sdc.object_id
	inner join sys.columns sc on sdc.parent_column_id=sc.column_id and so.parent_object_id=sc.object_id 
	inner join sys.objects sp on sp.object_id = so.parent_object_id and sp.schema_id = schema_id('dbo') and sp.type = 'U'
	left join sys.columns usc on usc.object_id = so.parent_object_id and usc.name = 'UsrCompanyID'
	where sc.name='CompanyMask' and so.type='D' and usc.object_id is null
	open c
	
	while 1 > 0 begin
		fetch c into @dropStmt, @addStmt, @updateStmt, @extendStmt
		if( @@fetch_status < 0 ) break

		if @extendStmt is not null begin -- make the column wider?
			print @extendStmt
			exec sp_executesql @extendStmt
		end

		if @bytesToAppendToMask > 0 begin -- alter defaults?
			print @dropStmt
			exec sp_executesql @dropStmt
			print @addStmt
			exec sp_executesql @addStmt
		end

		print @updateStmt
		exec sp_executesql @updateStmt
	end
	close c
	deallocate c
  
go

if exists(select * from sys.objects where object_id = object_id('dbo.pp_AfterDataInsertion') and objectproperty(object_id,'IsProcedure')=1)
drop proc dbo.pp_AfterDataInsertion
go
create procedure dbo.pp_AfterDataInsertion @companyID int as
  declare @maxID int
  select @maxID = max(CompanyID) from company
  declare @changeMask nvarchar(max)
  set @changeMask = N'update u set u.CompanyMask='
  if @companyID > 4
    set @changeMask = @changeMask + 'substring(u.CompanyMask, 1, ' + convert(varchar(10), (@companyID - 1) / 4) + ') + '
  if @companyID % 4 = 1
    set @changeMask = @changeMask + 'convert(binary(1), substring(u.CompanyMask, ' + convert(varchar(10), (@companyID - 1) / 4 + 1) + ', 1) & 253)'
  if @companyID % 4 = 2
    set @changeMask = @changeMask + 'convert(binary(1), substring(u.CompanyMask, ' + convert(varchar(10), (@companyID - 1) / 4 + 1) + ', 1) & 247)'
  if @companyID % 4 = 3
    set @changeMask = @changeMask + 'convert(binary(1), substring(u.CompanyMask, ' + convert(varchar(10), (@companyID - 1) / 4 + 1) + ', 1) & 223)'
  if @companyID % 4 = 0
    set @changeMask = @changeMask + 'convert(binary(1), substring(u.CompanyMask, ' + convert(varchar(10), (@companyID - 1) / 4 + 1) + ', 1) & 127)'
  if ((@maxID - 1) / 4) > ((@companyID - 1) / 4 + 1)
    set @changeMask = @changeMask + ' + substring(u.CompanyMask, ' + convert(varchar(10), (@companyID - 1) / 4 + 2) + ', ' + convert(varchar(10), (@maxID - 1) / 4 - (@companyID - 1) / 4 - 1) + ')'
  set @changeMask = @changeMask + ' from '
  declare @stmt nvarchar(max)
  declare @tableName sysname
  declare c INSENSITIVE cursor  for select sp.name
  from sys.default_constraints sdc
  inner join sys.objects so on so.object_id=sdc.object_id
  inner join sys.columns sc on sdc.parent_column_id=sc.column_id and so.parent_object_id=sc.object_id 
  inner join sys.objects sp on sp.object_id = so.parent_object_id and sp.schema_id = schema_id('dbo') and sp.type = 'U'
  left join sys.columns usc on usc.object_id = sp.object_id and usc.name = 'UsrCompanyID'
  where sc.name='CompanyMask' and so.type='D' and usc.object_id is null
  open c
  fetch c into @tableName
  while @@fetch_status >= 0 begin
    declare @primary sysname
    select @primary = name from sys.objects where parent_object_id = object_id('dbo.[' + @tableName + ']') and type='PK'
    if @primary is not null begin
      declare @indid smallint
      select @indid=index_id from sys.indexes where name=@primary
      declare @cols varchar(max)
      set @cols = ''
      declare @m int
      set @m = 1
      declare @nextcol sysname
      while @m > 0 begin
        select @nextcol = index_col('dbo.[' + @tableName + ']', @indid, @m)
        if @nextcol is not null begin
          if @nextcol <> 'CompanyID' begin
            if @cols = ''
              set @cols = 'u.[' + @nextcol + '] = f.[' + @nextcol + ']'
            else
              set @cols = @cols + ' and u.[' + @nextcol + '] = f.[' + @nextcol + ']'
          end
          set @m = @m + 1
        end
        else
          set @m = -1
      end
    end
    set @stmt = @changeMask + @tableName + ' u inner join dbo.[' + @tableName + '] f on ' + @cols
    if @cols <> ''
       set @stmt=@stmt + ' and f.companyid = ' + convert(varchar(10), @companyID) + ' where u.CompanyID <> ' + convert(varchar(10), @companyID)
    else
       set @stmt=@stmt + 'f.companyid = ' + convert(varchar(10), @companyID) + ' where u.CompanyID <> ' + convert(varchar(10), @companyID)
    exec sp_executesql @stmt
    fetch c into @tableName
  end
  close c
  deallocate c
go

if exists(select * from sys.objects where object_id = object_id('dbo.pp_InsertCompanyRecord') and objectproperty(object_id,'IsProcedure')=1)
drop proc dbo.pp_InsertCompanyRecord
go
create procedure dbo.pp_InsertCompanyRecord @companyID int, @companyCD nvarchar(128), @parentCompanyID int, @companyType nvarchar(128), @isReadonly int, @companyKey nvarchar(256) AS
BEGIN	
	IF (@parentCompanyID < 0) SET @parentCompanyID = NULL
	
	DECLARE @stmt nvarchar(MAX)
	DECLARE @cpi NVARCHAR(15)
	DECLARE @cpn NVARCHAR(128)
	DECLARE @pcpn NVARCHAR(15)
	DECLARE @isro NVARCHAR(10)
	DECLARE @cpt NVARCHAR(256)
	DECLARE @cpk NVARCHAR(256)
	SET @cpi = convert(varchar(15), @companyID)
	SET @pcpn = convert(varchar(15), @parentCompanyID)
	SET @isro = convert(varchar(10), @isReadonly)
	IF (@companyType is null) SELECT @cpt = '''Custom'''
	ELSE SET @cpt = '''' + @companyType + ''''
	IF (@companyKey is null) SELECT @cpk = 'NULL'
	ELSE SELECT @cpk = '''' + REPLACE(@companyKey,'''','''''') + ''''

	--Preparing Company CD
	IF (@companyCD is not null) SET @cpn = @companyCD
	ELSE BEGIN
		IF exists (select * from dbo.Company where CompanyID = @companyID)
			SELECT @cpn = CompanyCD from dbo.Company where CompanyID = @companyID
		IF (@cpn is null) SELECT @cpn =  @cpi
	END
	SET @cpn = '''' + @cpn + ''''
	
	IF EXISTS(SELECT * FROM sys.columns WHERE NAME='BAccountID' AND object_id = OBJECT_ID('dbo.Company'))
	BEGIN
		if @parentCompanyID is NULL
		begin
			if not exists (select * from dbo.Company where CompanyID = @companyID)
			BEGIN
				SET @stmt = 'insert dbo.Company (CompanyID, CompanyCD, PhoneMask, CompanyType, IsReadOnly, CompanyKey) 
					values (' + @cpi + ', ' + @cpi + ', '''', ' + @cpt + ', ' + @isro + ', ' + @cpk + ')'
			END
			ELSE
			BEGIN
				SET @stmt = 'update dbo.Company set CompanyCD = ' + @cpn + ', ParentCompanyID = null, CompanyType = ' + @cpt + ', IsReadOnly = ' + @isro + ', CompanyKey = ' + @cpk + '
					where CompanyID = ' + @cpi
			END
		end 
		ELSE
		begin
			if not exists (select * from dbo.Company where CompanyID = @companyID)
			BEGIN
				if exists (select * from dbo.Company where CompanyID = @parentCompanyID)
				BEGIN
					SET @stmt = 'insert dbo.Company (CompanyID, CompanyCD, CountryID, PhoneMask, ParentCompanyID, CompanyType, IsReadOnly, CompanyKey)
						select ' + @cpi + ', ' + @cpn + ', CountryID, PhoneMask, ' + @pcpn + ', ' + @cpt + ', ' + @isro + ', ' + @cpk + '
						from dbo.Company where CompanyID = ' + @pcpn
				END
				ELSE
				BEGIN
					SET @stmt = 'insert dbo.Company (CompanyID, CompanyCD, PhoneMask, ParentCompanyID, CompanyType, IsReadOnly, CompanyKey)
						values (' + @cpi + ', ' + @cpn + ', '''', ' + @pcpn + ', ' + @cpt + ', ' + @isro + ', ' + @cpk + ')'
				END
			END
			ELSE
			BEGIN
				SET @stmt = 'update dbo.Company set CompanyCD = ' + @cpn + ', ParentCompanyID = ' + @pcpn + ', CompanyType = ' + @cpt + ', IsReadOnly = ' + @isro + ', CompanyKey = ' + @cpk + '
					where CompanyID = ' + @cpi
			END
		END
	END
	ELSE
  		BEGIN
		if @parentCompanyID is NULL
		begin
			if not exists (select * from dbo.Company where CompanyID = @companyID)
			BEGIN
				SET @stmt = 'insert dbo.Company (CompanyID, CompanyCD, CompanyType, IsReadOnly, CompanyKey) 
					values (' + @cpi + ', ''' + @cpn + ''', ' + @cpt + ', ' + @isro + ', ' + @cpk + ')'
			END
			ELSE
			BEGIN
				SET @stmt = 'update dbo.Company set CompanyCD = ' + @cpn + ', ParentCompanyID = null, CompanyType = ' + @cpt + ', IsReadOnly = ' + @isro + ', CompanyKey = ' + @cpk + '
					where CompanyID = ' + @cpi
			END
		end 
		ELSE
		begin
			if not exists (select * from Company where CompanyID = @companyID)
			BEGIN
				SET @stmt = 'insert dbo.Company (CompanyID, CompanyCD, ParentCompanyID, CompanyType, IsReadOnly, CompanyKey)
				values (' + @cpi + ', ''' + @cpn + ''', ' + @pcpn + ', ' + @cpt + ', ' + @isro + ', ' + @cpk + ')'
			END
			ELSE
			BEGIN
				SET @stmt = 'update dbo.Company set CompanyCD = ' + @cpn + ', ParentCompanyID = ' + @pcpn + ', CompanyType = ' + @cpt + ', IsReadOnly = ' + @isro + ', CompanyKey = ' + @cpk + '
					where CompanyID = ' + @cpi
			END
		END
	END 
	--print @stmt
	exec sp_executesql @stmt
END
go

if exists(select * from sys.objects where object_id = object_id('dbo.pp_ReinitialiseCompanies') and objectproperty(object_id,'IsProcedure')=1)
drop proc dbo.pp_ReinitialiseCompanies
go
create procedure dbo.pp_ReinitialiseCompanies as
BEGIN	
	DECLARE @stmt nvarchar(MAX)
	DECLARE @companyID int
	
	declare c cursor for select c.CompanyID from Company c where CompanyID > 0
	open c
	fetch c into @companyID
	while @@fetch_status >= 0 begin
		SET @stmt = 'INSERT INTO WatchDog (CompanyID, TableName) values(' + convert(varchar(10), @companyID) + ',''Company'')'
		exec sp_executesql @stmt
		
		fetch c into @companyID
	end
	close c
	deallocate c	
END
go

if exists(select * from sys.objects where object_id = object_id('dbo.pp_AfterUpdate') and objectproperty(object_id,'IsProcedure')=1)
drop proc dbo.pp_AfterUpdate
go
create procedure dbo.pp_AfterUpdate as
BEGIN
	DECLARE @stmt nvarchar(MAX)
	DECLARE @name sysname

	declare c cursor for select p.Name from sys.procedures p
		join sys.schemas s on p.schema_id = s.schema_id
		left join information_schema.parameters pp on p.name = pp.SPECIFIC_NAME	
		where s.name='dbo' and pp.SPECIFIC_NAME is null and p.type = 'P' and p.name like 'up_%'
	open c
	fetch c into @name
	while @@fetch_status >= 0 begin
		SET @stmt = 'exec ' + @name
		print 'automatic executing procedure ''' + @name + ''' after update.'
		exec sp_executesql @stmt
		
		fetch c into @name
	end
	close c
	deallocate c
END
GO

if exists(select * from sys.objects where object_id = object_id('dbo.pp_ReinitialiseVersion') and objectproperty(object_id,'IsProcedure')=1)
drop proc dbo.pp_ReinitialiseVersion
go
create procedure dbo.pp_ReinitialiseVersion as
BEGIN	
	if not exists (SELECT * FROM sys.objects WHERE object_id = object_id('[dbo].[Company]')) return
	if not exists (SELECT * FROM sys.objects WHERE object_id = object_id('[dbo].[Version]')) return
	if not exists (SELECT * FROM sys.objects WHERE object_id = object_id('[dbo].[WatchDog]')) return

	DECLARE @stmt nvarchar(MAX)
	DECLARE @companyID int
	
	IF EXISTS(SELECT TOP 1 * FROM [dbo].Version) BEGIN
		SET @stmt = 'Update Version Set Altered = GETDATE()'
		exec sp_executesql @stmt
	END

	declare c cursor for select c.CompanyID from Company c  where CompanyID > 0
	open c
	fetch c into @companyID
	while @@fetch_status >= 0 begin
		SET @stmt = 'INSERT INTO WatchDog (CompanyID, TableName) values(' + convert(varchar(10), @companyID) + ',''Company'')'
		exec sp_executesql @stmt
		SET @stmt = 'INSERT INTO WatchDog (CompanyID, TableName) values(' + convert(varchar(10), @companyID) + ',''Version'')'
		exec sp_executesql @stmt
		
		fetch c into @companyID
	end
	close c
	deallocate c	
END
go

if exists(select * from sys.objects where object_id = object_id('dbo.pp_DeleteCompany') and objectproperty(object_id,'IsProcedure')=1)
drop proc dbo.pp_DeleteCompany
go
create procedure dbo.pp_DeleteCompany @companyID int, @preserveData int = 0 as
  declare @tableName sysname
  declare @stmt nvarchar(max)
  declare @trig nvarchar(max)
  declare @restriction nvarchar(max)
  declare @parent int

  --removing snapshots if not preserved (preserveData <= 0)
  if(@preserveData = 0 and @companyID > 0 and exists(select * from sys.objects where name='UPSnapshot' and type = 'U' and schema_id = schema_id('dbo')))
  begin
	  declare cc cursor for select LinkedCompany from UPSnapshot s
		inner join Company lc on lc.CompanyID = s.CompanyID
		where s.CompanyID = @companyID and LinkedCompany is not null
	  open cc
	  fetch cc into @parent
	  while @@fetch_status >= 0 begin
		exec pp_DeleteCompany @parent
		
		fetch cc into @parent
	  end
	  close cc
	  deallocate cc
  end

  set @parent = @companyID
  --correcting masks
  while @parent is not null begin
    if @restriction is null
      set @restriction = N'('
    else if len(@restriction) > 1
      set @restriction = @restriction + ', ' + convert(varchar(10), @parent)
    else
      set @restriction = @restriction + convert(varchar(10), @parent)
    
    if exists(select * from dbo.Company where CompanyID = @parent)
      select @parent = ParentCompanyID from dbo.Company where CompanyID = @parent
    else select @parent = null
  end
  if len(@restriction) > 1 begin
    set @restriction = @restriction + ')'
    declare c cursor for
      select N'update dbo.[' + sp.name + '] set CompanyMask = '
        + case when @companyID > 4 then 'substring(CompanyMask, 1, ' + convert(varchar(10), (@companyID - 1) / 4) + ') + ' else '' end
        + 'convert(binary(1), convert(int, substring(CompanyMask, ' + convert(varchar(10), (@companyID - 1) / 4 + 1) + ', 1)) | '
        + convert(varchar(10),
        ((case when substring(substring(sdc.definition, 4, len(sdc.definition) - 4), (@companyID - 1) / 2 + 1, 1) = '' then 0
        when substring(substring(sdc.definition, 4, len(sdc.definition) - 4), (@companyID - 1) / 2 + 1, 1) between '0' and '9'
        then ascii(substring(substring(sdc.definition, 4, len(sdc.definition) - 4), (@companyID - 1) / 2 + 1, 1)) - 48
        else ascii(substring(substring(sdc.definition, 4, len(sdc.definition) - 4), (@companyID - 1) / 2 + 1, 1)) - 55
        end) & case when @companyID % 2 = 1 then 3 else 12 end) * case when @companyID % 4 in (1, 2) then 1 else 16 end)
        + ' ) + substring(CompanyMask, ' + convert(varchar(10), (@companyID - 1) / 4 + 2) + ', 2048) where CompanyID in ' + @restriction
        from sys.default_constraints sdc
          inner join sys.columns sc on sc.object_id = sdc.parent_object_id and sc.column_id = sdc.parent_column_id
          inner join sys.objects sp on sp.object_id = sdc.parent_object_id and sp.schema_id = schema_id('dbo') and sp.type = 'U'
        where sc.name = 'CompanyMask' and ((case when substring(substring(sdc.definition, 4, len(sdc.definition) - 4), (@companyID - 1) / 2 + 1, 1) = '' then 0
        when substring(substring(sdc.definition, 4, len(sdc.definition) - 4), (@companyID - 1) / 2 + 1, 1) between '0' and '9'
        then ascii(substring(substring(sdc.definition, 4, len(sdc.definition) - 4), (@companyID - 1) / 2 + 1, 1)) - 48
        else ascii(substring(substring(sdc.definition, 4, len(sdc.definition) - 4), (@companyID - 1) / 2 + 1, 1)) - 55
        end) & case when @companyID % 2 = 1 then 3 else 12 end) <> 0
    open c
    fetch c into @stmt
    while @@fetch_status >= 0 begin
      exec sp_ExecuteSql @stmt
      fetch c into @stmt
    end
    close c
    deallocate c
  end

  --Removing Data
  declare c cursor for select so.name from sys.columns sc
    inner join sys.objects so on so.object_id = sc.object_id
    where sc.name='CompanyID' and so.type = 'U' and so.schema_id = schema_id('dbo')
  open c
  fetch c into @tableName
  while @@fetch_status >= 0 begin
    set @stmt = N'alter table dbo.[' + @tableName + '] NOCHECK CONSTRAINT ALL'
    exec sp_executesql @stmt
    fetch c into @tableName
  end
  close c
  deallocate c
  declare c cursor for select so.name from sys.columns sc
    inner join sys.objects so on so.object_id = sc.object_id
    where sc.name='CompanyID' and so.type = 'U' and so.schema_id = schema_id('dbo')
		and not(@preserveData > 0 and so.name like 'UPSnapshot%')
		and not(@preserveData > 1 and so.name like 'Company')
	order by so.name
  open c
  fetch c into @tableName
  while @@fetch_status >= 0 begin
  	--disable trigger
	declare ctrig cursor for select c.name from sys.objects c join sys.objects p on c.parent_object_id = p.object_id where c.type='TR' and p.name = @tableName
	open ctrig
	fetch ctrig into @trig
	while @@fetch_status >= 0
	BEGIN
	  set @stmt = 'DISABLE TRIGGER [' + @trig + '] ON dbo.[' + @tableName + ']'
	  EXEC sp_executesql @stmt
	fetch ctrig into @trig
	END
	close ctrig
	deallocate ctrig
  
    set @stmt = N'delete dbo.[' + @tableName + '] where companyid = ' + convert(varchar(10), @companyID)
	--print @stmt
    exec sp_executesql @stmt
    
  	--enable trigger
	declare ctrig cursor for select c.name from sys.objects c join sys.objects p on c.parent_object_id = p.object_id where c.type='TR' and p.name = @tableName
	open ctrig
	fetch ctrig into @trig
	while @@fetch_status >= 0
	BEGIN
	  set @stmt = 'ENABLE TRIGGER [' + @trig + '] ON dbo.[' + @tableName + ']'
	  EXEC sp_executesql @stmt
  	  fetch ctrig into @trig
	END
	close ctrig
	deallocate ctrig
	    
    fetch c into @tableName
  end
  close c
  deallocate c
  declare c cursor for select so.name from sys.columns sc
    inner join sys.objects so on so.object_id = sc.object_id
    where sc.name='CompanyID' and so.type = 'U' and so.schema_id = schema_id('dbo')
  open c
  fetch c into @tableName
  while @@fetch_status >= 0 begin
    set @stmt = N'alter table dbo.[' + @tableName + '] CHECK CONSTRAINT ALL'
    exec sp_executesql @stmt
    fetch c into @tableName
  end
  close c
  deallocate c
go

if exists(select * from sys.objects where object_id = object_id('dbo.pp_CorrectCompanyMask') and objectproperty(object_id,'IsProcedure')=1)
drop proc dbo.pp_CorrectCompanyMask
go
create procedure dbo.pp_CorrectCompanyMask @companyID int = NULL AS
  -- Find links from child companies to parent ones
  create table #pc (Viewer int, Child int, Parent INT, PRIMARY KEY(Viewer, Child, Parent))
  -- each company has link to immediate parent
  insert into #pc select CompanyID, CompanyID, ParentCompanyID from Company where CompanyID > 0 and ParentCompanyID >0;

  -- each company has path to root node
  while @@rowcount <> 0 begin
    insert into #pc select c1.Viewer, c2.CompanyID, c2.ParentCompanyID from #pc c1 
      inner join Company c2 on (c1.Parent = c2.CompanyID )
      left join #pc c3 on (c3.Viewer = c1.Viewer and c3.Child = c2.CompanyID and c3.Parent = c2.ParentCompanyID )
      where c2.CompanyID > 0 and c2.ParentCompanyID > 0 and c3.Viewer is null
  end 

  -- each company has all possible links to their parents
  while 1 > 0 begin
    insert into #pc select distinct c1.Viewer, c1.Child, c2.Parent from #pc c1 
      inner join #pc c2 on (c1.Viewer = c2.Viewer and c2.Child = c1.Parent )
      left join #pc c3 on (c3.Viewer = c1.Viewer and c3.Child = c1.Child and c3.Parent = c2.Parent )
      where c3.Viewer is null
	if @@rowcount = 0 break 
  end 
    
    --disabling triggers
  declare @trigname sysname
  declare @trigtable sysname
  declare @trigstmt nvarchar(max)
  declare ctrig cursor for select tr.name, tb.name from sys.triggers tr
	join sys.tables tb on tr.parent_id = tb.object_id
  open ctrig
  fetch ctrig into @trigname, @trigtable
  while @@fetch_status >= 0
  BEGIN
	set @trigstmt = 'DISABLE TRIGGER [' + @trigname + '] ON dbo.[' + @trigtable + ']'
	EXEC sp_executesql @trigstmt
  	fetch ctrig into @trigname, @trigtable
  END
  close ctrig
  deallocate ctrig

  -- prepare for iteration over companies
  declare @maxID int
  select @maxID = max(CompanyID) from dbo.Company
  declare @bytesUsedByMask varchar(10)
  set @bytesUsedByMask = convert(varchar(10), (@maxID + 3) / 4)
 
  declare @rc int

  declare @tableName sysname
  declare @tableID int
  declare @indid smallint

	-- In every table hide parents' records from children companies that have matching records (same key fields) with their own CompanyId value
    declare t cursor for select so.name, so.object_id, si.index_id from sys.columns sc
      inner join sys.objects so on so.object_id = sc.object_id
      inner join sys.indexes si on si.object_id = so.object_id and si.is_unique = 1 and si.Name not like '%[_]NoteID'
	  left join sys.columns usc on usc.object_id = so.object_id and usc.name = 'UsrCompanyID'
      where sc.name='CompanyMask' and so.type = 'U' and so.schema_id = schema_id('dbo') and usc.object_id is null
    open t
    fetch t into @tableName, @tableID, @indid
    while @@fetch_status >= 0 begin
      declare @cols varchar(1000)
      set @cols = ''
      declare @m int
      set @m = 1
      declare @nextcol sysname
      while @m > 0 begin
        select @nextcol = index_col('dbo.[' + @tableName + ']', @indid, @m)
        if @nextcol is not null begin
          if @nextcol <> 'CompanyID' begin
		    if @cols <> '' begin
			  set @cols = @cols + ' and '
			end

            if (select columnproperty(@tableID, @nextcol, 'AllowsNull')) = 1 begin
                set @cols = @cols + '(p.[' + @nextcol + '] = c.[' + @nextcol + '] or p.[' + @nextcol + '] is null and c.[' + @nextcol + '] is null)'
            end else begin
                set @cols = @cols + 'p.[' + @nextcol + '] = c.[' + @nextcol + ']'
            end
          end
          set @m = @m + 1
        end
        else
          set @m = -1
      end
      
	  -- update from select updates first row only
	  declare @stmt nvarchar(max)
      set @stmt = N'UPDATE p SET p.CompanyMask = dbo.binaryMaskSub(p.CompanyMask, pc.Viewer, 2)'
      set @stmt = @stmt + ' FROM dbo.[' + @tableName + '] p' 
      set @stmt = @stmt + ' INNER JOIN #pc pc on (pc.Parent = p.CompanyID)'
	  if(@companyID is not null)  set @stmt = @stmt + ' AND pc.Viewer = ' + convert(varchar(10), @companyID)
	  set @stmt = @stmt + ' INNER JOIN dbo.[' + @tableName + '] c ON (c.CompanyID = pc.Child'
	  if( @cols <> '')
		set @stmt = @stmt + ' AND ' + @cols
	  set @stmt = @stmt + ' AND (c.CompanyID = pc.Viewer OR dbo.binaryMaskTest(c.CompanyMask, pc.Viewer, 2) = 1))'
	  set @stmt = @stmt + ' WHERE dbo.binaryMaskTest(p.CompanyMask, pc.Viewer, 2) = 1'

		--print @stmt
		set @rc = 1
		-- This loop is needed to update records on all levels of tree. Query will run at least once, at most N = depth of company tree 
		while @rc > 0 begin
			exec sp_ExecuteSql @stmt
			set @rc = @@ROWCOUNT
		end

    fetch t into @tableName, @tableID, @indid
    end
    close t
    deallocate t


	-- set read and write permissions for own records in all companies
	declare d insensitive cursor for select 
		N'update u set u.CompanyMask = dbo.binaryMaskAdd(u.CompanyMask, u.CompanyID, 3) from dbo.[' + so.name + '] u where u.CompanyID > 1 AND dbo.binaryMaskTest(u.CompanyMask, u.CompanyID, 3) = 0'
		from sys.objects so
		inner join sys.columns sc on sc.object_id = so.object_id and sc.name = 'CompanyMask'
		where so.schema_id = schema_id('dbo') and so.type = 'U'
		order by so.name
	open d
	fetch d into @stmt
	while @@fetch_status >= 0 begin	
		--print @stmt
		exec sp_ExecuteSql @stmt
	fetch d into @stmt
	end
	close d
	deallocate d
  
  --enabling triggers
  declare ctrig cursor for select tr.name, tb.name from sys.triggers tr
	join sys.tables tb on tr.parent_id = tb.object_id
  open ctrig
  fetch ctrig into @trigname, @trigtable
  while @@fetch_status >= 0
  BEGIN
	set @trigstmt = 'ENABLE TRIGGER [' + @trigname + '] ON dbo.[' + @trigtable + ']'
	EXEC sp_executesql @trigstmt

  	fetch ctrig into @trigname, @trigtable
  END
  close ctrig
  deallocate ctrig

  --Correction default value for CompanyMask
  declare @charnbr int
  set @charnbr = ((@maxID - 1) / 4) * 2 + 2
  declare d insensitive cursor for select N'alter table dbo.[' + sp.name + '] drop constraint [' + object_name(sdc.object_id)
    + ']; alter table dbo.[' + sp.name + '] add constraint [' + object_name(sdc.object_id)
    + '] default 0x'
    + case when len(sdc.definition) > 3 then substring(substring(sdc.definition, 4, len(sdc.definition) - 4) + replicate(substring(sdc.definition, 4, 1), @charnbr), 1, @charnbr)
      else replicate('A', @charnbr) end
    + ' for CompanyMask'
    from sys.default_constraints sdc
      inner join sys.columns sc on sc.object_id = sdc.parent_object_id and sc.column_id = sdc.parent_column_id
      inner join sys.objects sp on sp.object_id = sdc.parent_object_id and sp.schema_id = schema_id('dbo') and sp.type = 'U'
    and sc.name = 'CompanyMask' and len(sdc.definition) < @charnbr + 4
  open d
  fetch d into @stmt
  while @@fetch_status >= 0 begin
	--print @stmt
	exec sp_ExecuteSql @stmt
	fetch d into @stmt
  end
  close d
  deallocate d
go

if exists(select * from sys.objects where object_id = object_id('dbo.pp_AfterSnapshotRestoration') and objectproperty(object_id,'IsProcedure')=1)
drop proc dbo.pp_AfterSnapshotRestoration
go
create procedure dbo.pp_AfterSnapshotRestoration @companyID int, @snapshotID int as
begin
  declare @tableName sysname
  declare @stmt nvarchar(max)
  declare @restriction nvarchar(max) 
  declare @changeMask nvarchar(1000)
  declare @parent int
  declare @maxID int     
  
  --finding parents
  SET @parent = @companyID
  while @parent is not null begin
	if (@parent is not null)
	BEGIN
		if @restriction is null
		  set @restriction = N'('
		else if len(@restriction) > 1
		  set @restriction = @restriction + ', ' + convert(varchar(10), @parent)
		else
		  set @restriction = @restriction + convert(varchar(10), @parent)
    END

	if exists(select * from dbo.Company where CompanyID = @parent)
      select @parent = ParentCompanyID from dbo.Company where CompanyID = @parent
    else select @parent = null
  end
  set @restriction = @restriction + ')'
  if (LEN(@restriction) <= 2) return

  --iteration on tables where CompanyMask is
  declare c cursor for select so.name from sys.columns sc
    inner join sys.objects so on so.object_id = sc.object_id
	left join sys.columns usc on usc.object_id = so.object_id and usc.name = 'UsrCompanyID'
    where sc.name='CompanyMask' and so.type = 'U' and so.schema_id = schema_id('dbo') and usc.object_id is null
	order by so.name
  open c
  fetch c into @tableName
  while @@fetch_status >= 0 begin
    declare @primary sysname
    select @primary = name from sys.objects where parent_object_id = object_id('dbo.[' + @tableName + ']') and type='PK'
    if @primary is not null begin
      declare @indid smallint
	  declare @cols nvarchar(1000)
	  declare @nextcol sysname
	  declare @m int

	  --finding primary key
      select @indid=index_id from sys.indexes where name=@primary      
      set @cols = ''      
      set @m = 1      
      while @m > 0 begin
        select @nextcol = index_col('dbo.[' + @tableName + ']', @indid, @m)
        if @nextcol is not null begin
          if @nextcol <> 'CompanyID' begin
            if @cols = ''
              set @cols = '[target].[' + @nextcol + '] = [snapshot].[' + @nextcol + ']'
            else
              set @cols = @cols + ' and [target].[' + @nextcol + '] = [snapshot].[' + @nextcol + ']'
          end
          set @m = @m + 1
        end
        else
          set @m = -1
      end
    end

	--correcting masks
	set @changeMask = N''
	select @maxID = max(CompanyID) from dbo.Company
    if @companyID > 4
      set @changeMask = @changeMask + 'substring([target].[CompanyMask], 1, ' + convert(varchar(10), (@companyID - 1) / 4) + ') + '
    if @companyID % 4 = 1
      set @changeMask = @changeMask + 'convert(binary(1), substring([target].[CompanyMask], ' + convert(varchar(10), (@companyID - 1) / 4 + 1) + ', 1) & 253)'
    if @companyID % 4 = 2
      set @changeMask = @changeMask + 'convert(binary(1), substring([target].[CompanyMask], ' + convert(varchar(10), (@companyID - 1) / 4 + 1) + ', 1) & 247)'
    if @companyID % 4 = 3
      set @changeMask = @changeMask + 'convert(binary(1), substring([target].[CompanyMask], ' + convert(varchar(10), (@companyID - 1) / 4 + 1) + ', 1) & 223)'
    if @companyID % 4 = 0
      set @changeMask = @changeMask + 'convert(binary(1), substring([target].[CompanyMask], ' + convert(varchar(10), (@companyID - 1) / 4 + 1) + ', 1) & 127)'
    if ((@maxID - 1) / 4) > ((@companyID - 1) / 4)
      set @changeMask = @changeMask + ' + substring([target].[CompanyMask], ' + convert(varchar(10), (@companyID - 1) / 4 + 2) + ', ' + convert(varchar(10), ((@maxID - 1) / 4 - (@companyID - 1) / 4)) + ')'


	--updating table
	set @stmt = N'update [target] set [target].[CompanyMask] = '
	set @stmt = @stmt + @changeMask
	set @stmt = @stmt + char(10) + char(9) + 'from dbo.[' + @tableName + '] [target] '
	set @stmt = @stmt + char(10) + 'inner join dbo.[' + @tableName + '] snapshot on ' + @cols
	if (@cols <> '') set @stmt = @stmt + char(10) + char(9) + ' and '
	set @stmt = @stmt + '[snapshot].[CompanyMask] = 0x and [snapshot].[CompanyID] = ' + convert(varchar(10), @snapshotID) 
	set @stmt = @stmt + char(10) + 'where [target].[CompanyID] in ' + @restriction
    
	print @stmt
	exec sp_ExecuteSql @stmt

	if(@companyID = @snapshotID)
	BEGIN
		set @stmt = N'Delete from dbo.[' + @tableName + N'] where CompanyMask = 0x and [CompanyID] = ' + convert(varchar(10), @snapshotID) 
		print @stmt
		exec sp_ExecuteSql @stmt	
	END

    fetch c into @tableName
  end
  close c
  deallocate c
end
go

if exists(select * from sys.objects where object_id = object_id('dbo.pp_GetWebSiteWikies') and objectproperty(object_id,'IsProcedure')=1)
drop proc dbo.pp_GetWebSiteWikies
go
create procedure dbo.pp_GetWebSiteWikies @companyID int as
begin
  with dictionary(CompanyID, ParentCompanyID, Level) as 
(
  Select CompanyID, ParentCompanyID, 0
  From dbo.Company Where CompanyID=@companyID
  Union ALL
  Select t.CompanyID, t.ParentCompanyID, Level+1
  From dbo.Company t Join Dictionary d on d.ParentCompanyID = t.CompanyID 
)
Select p.Name from dbo.WikiPage p
Join dbo.WikiDescriptor des on des.PageID=p.PageID and des.CompanyID=p.CompanyID
Join dictionary d on p.CompanyID=d.CompanyID
where  des.WikiArticleType = 13 and not exists (
  select 1 from dbo.WikiPage u 
  Join dictionary ind on u.CompanyID=ind.CompanyID
  where u.Name = p.Name and ind.level<d.level
) and 
0 = case 
  when p.CompanyID % 4 = 1 then convert(binary(1), substring(p.CompanyMask, (p.CompanyID - 1) / 4 + 1, 1) & 2) - 2
  when p.CompanyID % 4 = 2 then convert(binary(1), substring(p.CompanyMask, (p.CompanyID - 1) / 4 + 1, 1) & 8) -8
  when p.CompanyID % 4 = 3 then convert(binary(1), substring(p.CompanyMask, (p.CompanyID - 1) / 4 + 1, 1) & 32) -32
  when p.CompanyID % 4 = 0 then convert(binary(1), substring(p.CompanyMask, (p.CompanyID - 1) / 4 + 1, 1) & 128)-128
end
Order by  p.Name
end
Go


if exists(select * from sys.objects where object_id = object_id('dbo.pp_GetUsers') and objectproperty(object_id,'IsProcedure')=1)
drop proc dbo.pp_GetUsers
go
create procedure dbo.pp_GetUsers @companyID int as
begin
with dictionary(CompanyID, ParentCompanyID, Level) as 
(
  Select CompanyID, ParentCompanyID, 0
  From dbo.Company Where CompanyID=@companyID
  Union ALL
  Select t.CompanyID, t.ParentCompanyID, Level+1
  From dbo.Company t Join Dictionary d on d.ParentCompanyID = t.CompanyID 
)
Select p.username from dbo.Users p
Join dictionary d on p.CompanyID=d.CompanyID
where not exists (
  select 1 from dbo.Users u 
  Join dictionary ind on u.CompanyID=ind.CompanyID
  where u.Username = p.Username and ind.level<d.level
) and 
0 = case 
  when p.CompanyID % 4 = 1 then convert(binary(1), substring(p.CompanyMask, (p.CompanyID - 1) / 4 + 1, 1) & 2) - 2
  when p.CompanyID % 4 = 2 then convert(binary(1), substring(p.CompanyMask, (p.CompanyID - 1) / 4 + 1, 1) & 8) -8
  when p.CompanyID % 4 = 3 then convert(binary(1), substring(p.CompanyMask, (p.CompanyID - 1) / 4 + 1, 1) & 32) -32
  when p.CompanyID % 4 = 0 then convert(binary(1), substring(p.CompanyMask, (p.CompanyID - 1) / 4 + 1, 1) & 128)-128
end
Order by  p.username
end
Go

if exists(select * from sys.objects where object_id = object_id('dbo.pp_GetTables') and objectproperty(object_id,'IsProcedure')=1)
drop proc dbo.pp_GetTables
go
create procedure dbo.pp_GetTables as
  select sot.name, 
         case when sdc.definition is null or substring(sdc.definition, 1, 4) = '(0x0' then 0
         when substring(sdc.definition, 1, 4) = '(0xA' then 1
         when substring(sdc.definition, 1, 4) = '(0xF' then 2 end
    from sys.objects sot
      left join sys.columns sc on sc.object_id = sot.object_id and sc.name='CompanyMask'
      left join sys.default_constraints sdc on sdc.parent_column_id=sc.column_id and sdc.parent_object_id=sot.object_id
      left join sys.objects so on so.object_id=sdc.object_id and so.parent_object_id=sc.object_id and so.type='D'
    where sot.type = 'U' and sot.schema_id = schema_id('dbo')
    order by sot.name
go

if exists(select * from sys.objects where object_id = object_id('dbo.pp_RemoveConstraints') and objectproperty(object_id,'IsProcedure')=1)
drop proc dbo.pp_RemoveConstraints
go
create procedure dbo.pp_RemoveConstraints @table sysname as
  declare @stmt nvarchar(max)
  declare @foreign sysname
  declare @foreignTable sysname
  declare foreigncrs cursor for select sf.name, sp.name
    from sys.foreign_keys sf
    inner join sys.objects sp on sp.object_id = sf.parent_object_id and sp.type = 'U' and sp.schema_id = schema_id('dbo')
    where sf.referenced_object_id = object_id('dbo.[' + @table + ']')
  open foreigncrs
  fetch foreigncrs into @foreign, @foreignTable
  while @@fetch_status >= 0 begin
    set @stmt = 'alter table dbo.[' + @foreignTable + '] drop constraint [' + @foreign + ']'
    exec sp_executesql @stmt
    fetch foreigncrs into @foreign, @foreignTable
  end
  close foreigncrs
  deallocate foreigncrs
  declare foreigncrs cursor for select name from sys.foreign_keys where parent_object_id = object_id('dbo.[' + @table + ']')
  open foreigncrs
  fetch foreigncrs into @foreign
  while @@fetch_status >= 0 begin
    set @stmt = 'alter table dbo.[' + @table + '] drop constraint [' + @foreign + ']'
    exec sp_executesql @stmt
    fetch foreigncrs into @foreign
  end
  close foreigncrs
  deallocate foreigncrs
  declare @primary sysname
  declare @type sysname
  declare @skip sysname
  declare constrcsr cursor for select name, type from sys.objects so where so.parent_object_id = object_id('dbo.[' + @table + ']') and so.type in ('C ', 'F ', 'PK', 'UQ', 'D ')
     and not exists(select * from sys.columns sc inner join sys.default_constraints sdc on sdc.parent_object_id=sc.object_id and sdc.parent_column_id=sc.column_id where sc.object_id = so.parent_object_id and sdc.object_id = so.object_id and sc.name = 'CompanyMask')
  open constrcsr
  fetch constrcsr into @primary, @type
  while @@fetch_status >= 0 begin
    if @type <> 'PK'
      set @stmt = 'alter table dbo.[' + @table + '] drop constraint [' + @primary + ']'
    else begin
      set @stmt = 'exec sp_rename ''' + @primary + ''', ''Removed' + @primary + ''''
      set @skip = 'Removed' + @primary
    end
    exec sp_executesql @stmt
    fetch constrcsr into @primary, @type
  end
  close constrcsr
  deallocate constrcsr
  declare @indid int
  declare indexcsr cursor for select name, index_id from sys.indexes where object_id = object_id('dbo.[' + @table + ']') and type <> 1 and index_id > 0 and index_id < 255
  open indexcsr
  fetch indexcsr into @primary, @indid
  while @@fetch_status >= 0 begin
    if (select serverproperty('isfulltextinstalled')) = 1 begin
      declare @cnt int
      set @stmt = N'select @counter=count(*) from sys.fulltext_indexes where object_id = object_id(''dbo.[' + @table + ']'') and unique_index_id = ' + convert(varchar(10), @indid)
      exec sp_executesql @stmt, N'@counter int output', @counter=@cnt output
      if @cnt <> 0 begin
        set @stmt = 'drop fulltext index on dbo.[' + @table + ']'
        exec sp_executesql @stmt
      end
    end
    if @skip is null or @skip <> @primary begin
      set @stmt = 'drop index [' + @primary + '] on dbo.[' + @table + ']'
      exec sp_executesql @stmt
    end
    fetch indexcsr into @primary, @indid
  end
  close indexcsr
  deallocate indexcsr
  declare @trigger sysname
  declare triggercsr insensitive cursor for select name from sys.triggers where (name like '%[^_][_][_]' + @table + '[_][^_]%' or name like @table + '[_][_][^_]%') and name not like 'Usr%'
  open triggercsr
  fetch triggercsr into @trigger
  while @@fetch_status >= 0 begin
    set @stmt = 'drop trigger dbo.[' + @trigger + ']'
    exec sp_executesql @stmt
    fetch triggercsr into @trigger
  end
  close triggercsr
  deallocate triggercsr
go

if exists(select * from sys.objects where object_id = object_id('dbo.pp_DropIndexes') and objectproperty(object_id,'IsProcedure')=1)
drop proc dbo.pp_DropIndexes
go
create procedure dbo.pp_DropIndexes @table sysname as
  declare @stmt nvarchar(max)
  declare @index sysname
  declare @indid int
  declare @uniqueconstraint bit
  declare indexcsr insensitive cursor for select name, index_id, is_unique_constraint from sys.indexes where object_id = object_id('dbo.[' + @table + ']')
          and index_id > 0 and index_id < 255 and is_primary_key = 0 and substring(name, 1, 3) <> 'Usr' and name <> @table + '_PK'
  open indexcsr
  fetch indexcsr into @index, @indid, @uniqueconstraint
  while @@fetch_status >= 0 begin
    if (select serverproperty('isfulltextinstalled')) = 1 begin
      declare @cnt int
      set @stmt = N'select @counter=count(*) from sys.fulltext_indexes where object_id = object_id(''dbo.[' + @table + ']'') and unique_index_id = ' + convert(varchar(10), @indid)
      exec sp_executesql @stmt, N'@counter int output', @counter=@cnt output
      if @cnt <> 0 begin
        set @stmt = 'drop fulltext index on dbo.[' + @table + ']'
        exec sp_executesql @stmt
      end
    end
    if @uniqueconstraint = 0
      set @stmt = 'drop index [' + @index + '] on dbo.[' + @table + ']'
    else
      set @stmt = 'alter table dbo.[' + @table + '] drop constraint [' + @index + ']'
    exec sp_executesql @stmt
    fetch indexcsr into @index, @indid, @uniqueconstraint
  end
  close indexcsr
  deallocate indexcsr
go

if exists(select * from sys.objects where object_id = object_id('dbo.pp_DropForeigns') and objectproperty(object_id,'IsProcedure')=1)
drop proc dbo.pp_DropForeigns
go
create procedure dbo.pp_DropForeigns @table sysname as
  declare @stmt nvarchar(max)
  declare @index sysname
  declare indexcsr insensitive cursor for select name from sys.foreign_keys where parent_object_id = object_id('dbo.[' + @table + ']')
  open indexcsr
  fetch indexcsr into @index
  while @@fetch_status >= 0 begin
    set @stmt = 'alter table dbo.[' + @table + '] drop constraint [' + @index + ']'
    exec sp_executesql @stmt
    fetch indexcsr into @index
  end
  close indexcsr
  deallocate indexcsr
  declare @trigger sysname
  declare triggercsr insensitive cursor for select name from sys.triggers where name like '%[^_][_][_]' + @table + '[_][^_]%' and name not like 'Usr%'
  open triggercsr
  fetch triggercsr into @trigger
  while @@fetch_status >= 0 begin
    set @stmt = 'drop trigger dbo.[' + @trigger + ']'
    exec sp_executesql @stmt
    fetch triggercsr into @trigger
  end
  close triggercsr
  deallocate triggercsr
go

if exists(select * from sys.objects where object_id = object_id('dbo.pp_CreateTable') and objectproperty(object_id,'IsProcedure')=1)
drop proc dbo.pp_CreateTable
GO
CREATE PROCEDURE dbo.pp_CreateTable @tableName sysname, @templateTable sysname, @key sysname, @ident sysname, @type sysname AS
BEGIN	
	DECLARE @stmt NVARCHAR(max)
	DECLARE @tableID int
	DECLARE @col sysname
	DECLARE @cols varchar(1000)
	DECLARE @nextcol sysname
    DECLARE @lastcol sysname
	DECLARE @indid smallint
	DECLARE @identSetup varchar(1000)
    DECLARE @primary varchar(1000)
    
    set @primary = null
    set @cols = ''
	set @identSetup = ''
    Select @tableID = object_id from sys.objects where name = @templateTable and objectproperty(object_id, 'IsUserTable') = 1 
	if (CHARINDEX('|', @ident, 0) > 0)
	BEGIN
		SET @identSetup = ' (' + SUBSTRING(@ident, CHARINDEX('|', @ident, 0) + 1, LEN(@ident) - CHARINDEX('|', @ident, 0))  + ', 1)'
		SET @ident = SUBSTRING(@ident, 1, CHARINDEX('|', @ident, 0) - 1)
	END

	CREATE TABLE #specificTypesList (columnName sysname COLLATE DATABASE_DEFAULT, columnType sysname COLLATE DATABASE_DEFAULT)
	if(@type is not null)
	begin
	    SET @type = REPLACE(@type, ' ', '')
		SET @stmt = REPLACE(@type, ',', ''' UNION SELECT ''')
		SET @stmt = REPLACE(@stmt, '=', ''', ''')
		SET @stmt = 'SELECT ''' + @stmt + ''''
		--print @stmt
		INSERT #specificTypesList EXEC(@stmt)
	end

	set @stmt = 'create table dbo.[' + @tableName + ']'
    set @stmt = @stmt + char(10) + '('    

    declare columncsr cursor for select N'[' + c.name + '] '
			+ case 
				when c.name in (select columnName from #specificTypesList) then (select columnType from #specificTypesList where columnName = c.name)
				when t.name = 'text' then 'varchar(max)' 
				when t.name = 'ntext' then 'nvarchar(max)' 
				when t.name = 'image' then 'varbinary(max)' 
				else t.name 
			end 
			+ case 
				when c.name in (select columnName from #specificTypesList) then ''
				when t.name in('char', 'nchar', 'varchar', 'nvarchar', 'binary', 'varbinary') then '(' 
					+ case convert(smallint, 
						case 
							when c.system_type_id in (34, 35, 99) then null 
							when c.system_type_id = 36 then c.precision 
							when c.max_length = -1 then -1 
							else odbcprec(c.system_type_id, c.max_length, c.precision) 
						end) 
						when -1 then 'max' 
						else convert(varchar(10), convert(smallint, 
							case 
								when c.system_type_id in (34, 35, 99) then null 
								when c.system_type_id = 36 then c.precision 
								when c.max_length = -1 then -1 
								else odbcprec(c.system_type_id, c.max_length, c.precision) 
							end)) 
					end + ')'
				when t.name in('decimal', 'numeric') then '(' + convert(varchar(10), convert(smallint, 
					case 
						when c.system_type_id in (34, 35, 99) then null 
						when c.system_type_id = 36 then c.precision 
						when c.max_length = -1 then -1 
						else odbcprec(c.system_type_id, c.max_length, c.precision) 
					end)) + ', ' + convert(varchar(10), odbcscale(c.system_type_id, c.scale)) + ')'
				else '' 
			end
			+ case 
				when c.name = @ident then ' identity' + @identSetup
				else case /*c.is_nullable */
					when (	(LEN(@key) <= 0 AND (ic.index_id IS NOT NULL)) 
							OR (CHARINDEX(c.name, @key) > 0)
						) then ' not null' 
					else ' null'
				end
			end
			+ case 
				when c.name = 'CompanyID' or c.name = 'DeletedDatabaseRecord' then ' default 0' 
				when c.name = 'CompanyMask' then ' default 0xAA' 
				when c.name = 'GroupMask' then ' default 0x'
				when @ident is not null and substring(@ident, 1, 7) = 'Company' and 'Company' + c.name = @ident then ' default ident_current(''' + @tableName +''')'
				when d.definition = '(getdate())' then ' default getdate()'
				else '' 
			end
		from sys.columns c
		inner join sys.types t on t.user_type_id = c.user_type_id
		left join sys.default_constraints d on d.object_id = c.default_object_id
		left join sys.index_columns ic on ic.index_id = 1 AND c.object_id = ic.object_id AND c.column_id = ic.column_id 
		where c.object_id = @tableID
		order by c.column_id
		
    open columncsr
    fetch columncsr into @col
    while @@fetch_status >= 0 begin
		set @stmt = @stmt + char(10) + char(9) + @col + ','
		
		fetch columncsr into @col
    end
    close columncsr
    deallocate columncsr
    
    select @indid = index_id from sys.indexes 
    where is_primary_key = 1 and object_id = @tableID
    SET @primary = 'constraint [' + @tableName + '_PK] primary key clustered'    
    
    if (@key is not null AND LEN(@key) > 0) SET @cols = @key
    ELSE if @indid > 0 BEGIN
		declare @k int
		set @k = 1
		
		while @k > 0
		begin
			set @nextcol = index_col(N'dbo.[' + @templateTable + ']', @indid, @k)
			if @nextcol is not null begin
				set @lastcol = @nextcol
				set @nextcol = '[' + @nextcol + ']'
				if (@cols = '') set @cols = @nextcol
				else set @cols = @cols + ', ' + @nextcol
				set @k = @k + 1
			end else
			set @k = -1
		end
    END
    set @primary = @primary + '(' + @cols + ')'
	set @stmt = @stmt + char(10) + char(9) + @primary
    set @stmt = @stmt + char(10) + ')'
    
    --print @stmt
    exec sp_executesql @stmt
END
GO

IF exists(SELECT * FROM sys.objects where object_id = object_id('dbo.pp_BinaryToHexString') and type in ('FN', 'IF', 'TF', 'FS', 'FT'))
DROP FUNCTION dbo.pp_BinaryToHexString
go
CREATE FUNCTION dbo.pp_BinaryToHexString (@pbinin VARBINARY(8000), @addPrefix bit = 0)
RETURNS NVARCHAR(4000)
AS
BEGIN
	DECLARE @pstrout NVARCHAR(4000)
	DECLARE @i INT
	DECLARE @firstnibble INT
	DECLARE @secondnibble INT
	DECLARE @tempint INT
	DECLARE @hexstring CHAR(16)
	DECLARE @cbytesin INT 

	--initialize and validate
	IF (@pbinin IS NOT NULL)
	BEGIN
		SET @i = 0
		SET @cbytesin = DATALENGTH(@pbinin)
		SET @pstrout = N''
		if (@addPrefix <> 0) SET @pstrout = N'0x'
		SET @hexstring = '0123456789ABCDEF'

		IF (((@cbytesin * 2) + 2 > 4000) or ((@cbytesin * 2) + 2 < 1)) RETURN NULL

		-- do for each byte
		WHILE (@i < @cbytesin)
		BEGIN
			-- Each byte has two nibbles which we convert to character
			SET @tempint = CAST(SUBSTRING(@pbinin, @i + 1, 1) AS INT)
			SET @firstnibble = @tempint / 16
			SET @secondnibble = @tempint % 16

			-- we need to do an explicit cast with substring for proper string conversion.
			SET @pstrout = @pstrout 
				+ CAST(SUBSTRING(@hexstring, (@firstnibble+1), 1) AS NVARCHAR) 
				+ CAST(SUBSTRING(@hexstring, (@secondnibble+1), 1) AS NVARCHAR)

			SET @i = @i + 1
		END
	END

	RETURN @pstrout
END
GO

if exists(select * from sys.objects where object_id = object_id('dbo.AdjustCompanyMask') and type in ('FN', 'IF', 'TF', 'FS', 'FT'))
DROP FUNCTION dbo.AdjustCompanyMask
go
CREATE FUNCTION dbo.AdjustCompanyMask(@mask VARBINARY(2048), @srcCompanyID int, @dstCompanyID int)
RETURNS VARBINARY(2048) AS
BEGIN	
	DECLARE @srcMask tinyint
	DECLARE @denominator tinyint

	if (@dstCompanyID < 0 or @srcCompanyID < 0) return @mask

	set @denominator = case @srcCompanyID % 4 when 1 then 1 when 2 then 4 when 3 then 16 else 64 end
	set @srcMask = (substring(@mask, (@srcCompanyID + 3) / 4, 1) & ( 3 * @denominator)) / @denominator

	return dbo.binaryMaskSet(@mask, @dstCompanyID, @srcMask)
END
go

if exists(select * from sys.objects where object_id = object_id('dbo.pp_GetVisibility') and type in ('FN', 'IF', 'TF', 'FS', 'FT'))
drop function dbo.pp_GetVisibility
go
create function dbo.pp_GetVisibility (
 @companyID int,
 @companyMask varbinary(max)
 )
 returns varchar(max)
begin
  declare @matrix table (
   CompanyID int not null,
   Start int not null,
   Mask int not null
  )
  declare @cnt int
  insert @matrix values(@companyID, 1, 0)
  set @cnt = 1
  while @cnt > 0 begin
    insert @matrix
    select c.CompanyID, (c.CompanyID - 1) / 4 + 1, power(2, 2 * ((c.CompanyID - 1) - (c.CompanyID - 1) / 4 * 4) + 1)
    from Company c
    inner join @matrix m on m.CompanyID = c.ParentCompanyID
    left join @matrix mm on mm.CompanyID = c.CompanyID
    where mm.CompanyID is null
    set @cnt = @@rowcount
  end
  delete @matrix where convert(int, substring(@companyMask, Start, 1)) & Mask = 0
  declare @ret varchar(max)
  set @ret = convert(varchar(10), @companyID)
  update @matrix set @ret = @ret + ', ' + convert(varchar(10), CompanyID)
  return @ret
end
go

if exists(select * from sys.objects where object_id = object_id('dbo.pp_GetMatrix') and type in ('FN', 'IF', 'TF', 'FS', 'FT'))
drop function dbo.pp_GetMatrix
go
create function dbo.pp_GetMatrix (
 @companyID int,
 @companyMask varbinary(max)
 )
 returns @matrix table (
  CompanyID int not null,
  Start int not null,
  Mask int not null,
  primary key clustered (CompanyID)
 )
begin
  insert @matrix values (@companyID, 1, 0)
  declare @cnt int
  set @cnt = 1
  while @cnt > 0 begin
    insert @matrix
    select c.CompanyID, (c.CompanyID - 1) / 4 + 1, power(2, 2 * ((c.CompanyID - 1) - (c.CompanyID - 1) / 4 * 4) + 1)
    from Company c
    inner join @matrix m on m.CompanyID = c.ParentCompanyID
    left join @matrix mm on mm.CompanyID = c.CompanyID
    where mm.CompanyID is null
    set @cnt = @@ROWCOUNT
  end
  delete @matrix where convert(int, substring(@companyMask, Start, 1)) & Mask = 0 and CompanyID <> @companyID
  return
end
go

if exists(select * from sys.objects where object_id = object_id('dbo.pp_MoveData') and objectproperty(object_id,'IsProcedure')=1)
drop proc dbo.pp_MoveData
go
create procedure dbo.pp_MoveData @table sysname, @fields varchar(max), @selection varchar(max), @ident sysname, @selident sysname as
  if not exists(select * from sys.objects where name='Removed' + @table and type = 'U' and schema_id = schema_id('dbo'))
    return
  declare @stmt nvarchar(max)
  declare @maskdef sysname
  declare @constr sysname
  if not exists(select * from sys.columns where name='CompanyMask' and object_id = object_id('dbo.[' + @table + ']')) begin
    if exists(select * from sys.columns where name='UsrCompanyMask' and object_id = object_id('dbo.[Removed' + @table + ']')) begin
      select @maskdef = t.definition, @constr = object_name(c.default_object_id) from sys.default_constraints t
        inner join sys.columns c on c.default_object_id = t.object_id where c.name = 'CompanyMask' and c.object_id = object_id('dbo.[Removed' + @table +  ']')
      declare @option int
      set @option = case when @maskdef like '(0xF%' then 2 when @maskdef like '(0xA%' then 1 else 0 end
      set @stmt = 'alter table dbo.[Removed' + @table + '] drop constraint [' + @constr + ']'
      exec sp_executesql @stmt
      if @option > 0
        exec pp_SplitTable @table, @option
      else begin
        exec pp_SplitTable @table, 1
        exec pp_SplitTable @table, 0
      end
      if not exists(select * from sys.columns where name='CompanyMask' and object_id = object_id('dbo.[' + @table + ']')) begin
        set @stmt = 'alter table dbo.[Removed' + @table + '] add constraint [' + @constr + '] default ' + @maskdef + ' for CompanyMask'
        exec sp_executesql @stmt
        return
      end
      else begin
        exec dbo.pp_DropConstraint @table, 'CompanyMask'
        set @stmt = 'alter table dbo.[' + @table + '] add constraint [' + @constr + '] default ' + @maskdef + ' for CompanyMask'
        exec sp_executesql @stmt
      end
      if @ident is not null begin
        set @fields = @fields + ', [Company' + @ident + ']'
        if exists(select * from sys.columns where name = 'Company' + @ident and object_id = object_id('dbo.[Removed' + @table + ']'))
          set @selection = @selection + ', [Company' + @ident + ']'
        else
          set @selection = @selection + ', [' + @ident + ']'
      end
      set @fields = @fields + ', CompanyMask'
      set @selection = @selection + ', CompanyMask'
    end
  end else if exists(select * from sys.columns where name='CompanyMask' and object_id = object_id('dbo.[Removed' + @table + ']')) begin
    select @maskdef = t.definition, @constr = object_name(c.default_object_id) from sys.default_constraints t
      inner join sys.columns c on c.default_object_id = t.object_id where c.name = 'CompanyMask' and c.object_id = object_id('dbo.[Removed' + @table + ']')
    if @constr is not null begin
      set @stmt = 'alter table dbo.[Removed' + @table + '] drop constraint [' + @constr + ']'
      exec sp_executesql @stmt
      exec dbo.pp_DropConstraint @table, 'CompanyMask'
      set @stmt = 'alter table dbo.[' + @table + '] add constraint [' + @constr + '] default ' + @maskdef + ' for CompanyMask'
      exec sp_executesql @stmt
    end else begin
      declare @maxID int
      select @maxID = max(CompanyID) from Company
      if @maxID is not null and @maxID > 4
        set @selection = replace(@selection, 'coalesce([CompanyMask], (0xAA))', 'coalesce([CompanyMask], (0x' + replicate('A', ((@maxID - 1) / 4) * 2 + 2) + '))')
    end
  end
  declare @usrcol sysname
  declare @usrtype sysname
  declare @usrident bit
  declare usrcol insensitive cursor for select c.name,
    case t.name when 'text' then 'varchar(max)' when 'ntext' then 'nvarchar(max)' when 'image' then 'varbinary(max)' else t.name end + case when t.name in('char', 'nchar', 'varchar', 'nvarchar', 'binary', 'varbinary') then '(' + case convert(smallint, case when c.system_type_id in (34, 35, 99) then null when c.system_type_id = 36 then c.precision when c.max_length = -1 then -1 else odbcprec(c.system_type_id, c.max_length, c.precision) end) when -1 then 'max' else convert(varchar(10), convert(smallint, case when c.system_type_id in (34, 35, 99) then null when c.system_type_id = 36 then c.precision when c.max_length = -1 then -1 else odbcprec(c.system_type_id, c.max_length, c.precision) end)) end + ')'
                  when t.name in('decimal', 'numeric') then '(' + convert(varchar(10), convert(smallint, case when c.system_type_id in (34, 35, 99) then null when c.system_type_id = 36 then c.precision when c.max_length = -1 then -1 else odbcprec(c.system_type_id, c.max_length, c.precision) end)) + ', ' + convert(varchar(10), odbcscale(c.system_type_id, c.scale)) + ')'
                  else '' end 
				  + case when c.is_identity = 1 and @ident is null /* not exists(select * from sys.identity_columns where object_id=object_id(@table))*/ then ' identity(' + convert(varchar(10), ic.seed_value) + ', 1)' else '' end
				  + case c.is_nullable when 1 then ' NULL' else ' NOT NULL' end
                  + case c.name when 'UsrDeletedDatabaseRecord' then ' default 0' else '' end, c.is_identity
    from sys.columns c
    inner join sys.types t on t.user_type_id = c.user_type_id
	left join sys.identity_columns ic on ic.object_id = c.object_id and ic.column_id = c.column_id
	where c.object_id = object_id('Removed' + @table) and c.name like 'Usr%' and c.name <> 'UsrCompanyMask'
		--as we add Usr columns generation, so if it has been generated we should skip it
		--and c.name not in (select name from sys.columns nc where nc.object_id = object_id(@table))
		and CHARINDEX('['+ c.name + ']', @fields) <= 0 
    order by c.column_id
  open usrcol
  fetch usrcol into @usrcol, @usrtype, @usrident
  while @@fetch_status >= 0 begin
	if (@usrident = 1 and @ident is null)
	begin
		set @ident = @usrcol
		set @selident = @usrcol
	end
	else
	begin
	    set @fields = @fields + ', [' + @usrcol + ']'
		set @selection = @selection + ', [' + @usrcol + ']'
	end
    if not exists(select * from sys.columns where object_id = object_id('dbo.[' + @table + ']') and name = @usrcol) begin
      set @stmt = 'alter table dbo.[' + @table + '] add [' + @usrcol + '] ' + @usrtype
      exec sp_executesql @stmt
    end
    fetch usrcol into @usrcol, @usrtype, @usrident
  end
  close usrcol
  deallocate usrcol
  declare @primary varchar(max)
  set @primary = null
  declare usrcol insensitive cursor for select c.name,
    t.name + '(' + case when rc.max_length = -1 then 'max' else convert(varchar(10), odbcprec(rc.system_type_id, rc.max_length, rc.precision)) end + ')'
    + case c.is_nullable when 1 then ' null' else ' not null' end
    from sys.columns c
    inner join sys.types t on t.user_type_id = c.user_type_id and t.name in('varchar', 'nvarchar', 'varbinary')
    inner join sys.columns rc on rc.object_id = object_id('dbo.[Removed' + @table + ']') and rc.name = c.name
    inner join sys.types rt on rt.user_type_id = rc.user_type_id and rt.name in('varchar', 'nvarchar', 'varbinary')
    where c.object_id = object_id('dbo.[' + @table + ']') and c.name not like 'Usr%'
    and c.max_length <> -1 and (rc.max_length = -1 or odbcprec(rc.system_type_id, rc.max_length, rc.precision) > odbcprec(c.system_type_id, c.max_length, c.precision))
    order by c.column_id
  open usrcol
  fetch usrcol into @usrcol, @usrtype
  while @@fetch_status >= 0 begin
    if @primary is null begin
      declare @indid smallint
      declare @pkname sysname
      select @indid = index_id, @pkname = name,
        @primary = 'constraint [' + name + '] primary key '
                 + case when index_id = 1 then 'clustered ' else '' end
      from sys.indexes where is_primary_key = 1 and object_id = object_id('dbo.[' + @table + ']')
      if @primary is not null begin
        declare @cols varchar(max)
        set @cols = ''
        declare @k int
        set @k = 1
        declare @nextcol sysname
        while @k > 0 begin
          select @nextcol = index_col('dbo.[' + @table + ']', @indid, @k)
          if @nextcol is not null begin
            if @cols = ''
              set @cols = '[' + @nextcol + ']'
            else
              set @cols = @cols + ', [' + @nextcol + ']'
            set @k = @k + 1
          end else
            set @k = -1
        end
        set @primary = @primary + '(' + @cols + ')'
        set @stmt = 'alter table dbo.[' + @table + '] drop constraint [' + @pkname + ']'
        exec sp_executesql @stmt
      end
    end
    set @stmt = 'alter table dbo.[' + @table + '] alter column [' + @usrcol + '] ' + @usrtype
    exec sp_executesql @stmt
    fetch usrcol into @usrcol, @usrtype
  end
  close usrcol
  deallocate usrcol
  if @primary is not null begin
    set @stmt = 'alter table dbo.[' + @table + '] add ' + @primary
    exec sp_executesql @stmt
  end
  
  --moving USR indexes  
  declare @userind sysname
  declare usrindex insensitive cursor for select i.name from sys.indexes i 
		where i.type = 2 and i.name like 'Usr%'	and i.object_id = object_id('dbo.[Removed' + @table + ']')
  open usrindex
  fetch usrindex into @userind
  while @@fetch_status >= 0 begin
	if not exists(select i.name from sys.indexes i where i.name = @userind and i.object_id = object_id('dbo.[' + @table + ']'))
	BEGIN
		set @stmt = 'create index [' + @userind + '] on dbo.[' + @table + '] ('

		declare usrindexcol insensitive cursor for select c.name from sys.indexes i 
			join sys.index_columns ic on i.object_id = ic.object_id and i.index_id = ic.index_id
			join sys.columns c on i.object_id = c.object_id and ic.column_id = c.column_id
			where i.type = 2 and i.name like 'Usr%'	and i.object_id = object_id('dbo.[Removed' + @table + ']')
			Order by ic.index_column_id
		open usrindexcol
		fetch usrindexcol into @usrcol
		while @@fetch_status >= 0 begin
			set @stmt = @stmt + '[' + @usrcol + '], '

			fetch usrindexcol into @usrcol
		end
		close usrindexcol
		deallocate usrindexcol

		set @stmt = SUBSTRING(@stmt, 1, LEN(@stmt) - 1)
		set @stmt = @stmt + ')'

		--print @stmt
		exec sp_executesql @stmt
	END

    fetch usrindex into @userind
  end
  close usrindex
  deallocate usrindex
  
  if @ident is not null
    if substring(@ident, 1, 7) = 'Company'
      set @stmt = 'if not exists(select * from Removed' + @table + ' where ' + @ident + ' is null and ' + substring(@ident, 8, len(@ident) - 7) + ' is null) begin set identity_insert ' + @table + ' on; '
        + 'insert ' + @table + '(' + @ident + ', ' + @fields + ') select ' + @selident + ', ' + @selection + ' from Removed' + @table
        + '; set identity_insert ' + @table + ' off end '
        + 'else insert ' + @table + '(' + @fields + ') select ' + @selection + ' from Removed' + @table
    else
      set @stmt = 'if not exists(select * from Removed' + @table + ' where ' + @ident + ' is null) begin set identity_insert ' + @table + ' on; '
        + 'insert ' + @table + '(' + @ident + ', ' + @fields + ') select ' + @selident + ', ' + @selection + ' from Removed' + @table
        + '; set identity_insert ' + @table + ' off end '
        + 'else insert ' + @table + '(' + @fields + ') select ' + @selection + ' from Removed' + @table
  else
    set @stmt = 'insert dbo.[' + @table + '] (' + @fields + ') select ' + @selection + ' from dbo.[Removed' + @table + ']'
  --print @stmt
  exec sp_executesql @stmt
go

if exists(select * from sys.objects where object_id = object_id('dbo.pp_PrintString') and objectproperty(object_id,'IsProcedure')=1)
drop proc dbo.pp_PrintString
go
create procedure dbo.pp_PrintString @stmt varchar(MAX) as
begin
	DECLARE @index BIGINT
	DECLARE @char CHAR(1)
	DECLARE @tempstmt varchar(max)
	while (LEN(@stmt) > 0)
	Begin
		if(LEN(@stmt) > 8000) 
		BEGIN
			SET @index = 8000;
			While (@index > 0)
			BEGIN 
				SET @char = SUBSTRING(@stmt, @index, 1);
				if (@char = ' ') BREAK				
				SET @index = @index - 1
			END
			set @tempstmt = SUBSTRING(@stmt, 1, @index)
		END
		else
			set @tempstmt = @stmt
		print @tempstmt 
		set @stmt = SUBSTRING(@stmt, LEN(@tempstmt) + 2, LEN(@stmt) - LEN(@tempstmt))
	end
end
GO

if exists(select * from sys.objects where object_id = object_id('dbo.pp_GenerateLast') and objectproperty(object_id,'IsProcedure')=1)
drop proc dbo.pp_GenerateLast
go
create procedure dbo.pp_GenerateLast @specificTables varchar(MAX) = NULL as
  declare @stmt varchar(max)
  declare @temp varchar(max)
  declare @cond int
  declare @fields varchar(max)
  declare @selection varchar(max)
  declare @tableName sysname
  declare @tableID int
  declare @userfields varchar(max)
  set nocount on
  --if we using some specific tables
  CREATE TABLE #specificTablesList (tableName sysname COLLATE DATABASE_DEFAULT)
  if(@specificTables is not null) BEGIN
    SET @specificTables = REPLACE(@specificTables, ' ', '')
	SET @stmt = 'SELECT ''' + REPLACE(@specificTables, ',', ''' UNION SELECT ''') + ''''
	INSERT #specificTablesList EXEC(@stmt)
  END
  
  --enabling triggers
  print ''
  print 'declare @trigname sysname'
  print 'declare @trigtable sysname'
  print 'declare @trigstmt nvarchar(max)'
  print 'declare ctrig cursor for select tr.name, tb.name from sys.triggers tr'
  print '	join sys.tables tb on tr.parent_id = tb.object_id'
  if(@specificTables is NOT NULL) print ' and tb.name in (''' + REPLACE(@specificTables, ',', ''', ''') + ''')'
  print 'open ctrig'
  print 'fetch ctrig into @trigname, @trigtable'
  print 'while @@fetch_status >= 0'
  print 'BEGIN'
  print '	set @trigstmt = ''ENABLE TRIGGER ['' + @trigname + ''] ON dbo.['' + @trigtable + '']'''
  print '	EXEC sp_executesql @trigstmt'
  print '  	fetch ctrig into @trigname, @trigtable'
  print 'END'
  print 'close ctrig'
  print 'deallocate ctrig'
  print 'go'
  
  declare tablecsr cursor for select name, object_id from sys.objects where type = 'U' 
	and (@specificTables is NULL or name in (select tableName from #specificTablesList)) order by name
  open tablecsr
  fetch tablecsr into @tableName, @tableID
  while @@fetch_status >= 0 begin
    print '---' + @tableName
    set @cond = 0
    set @fields = null
    set @selection = null
    set @stmt = 'create table dbo.[' + @tableName + ']'
    set @stmt = @stmt + char(10) + '('
    declare @ident sysname
    set @ident = null
    select @ident = name from sys.identity_columns where object_id = @tableID
    declare @selident sysname
    set @selident = null
    declare @col sysname
    declare @cnt int
    set @cnt = 0
    declare @masked int
    set @masked = 0
    declare @companyExists bit
    set @companyExists = 0
    declare @noteExists bit
    set @noteExists = 0
	set @userfields = N''
    declare @cname sysname, @ccolorder smallint, @tname sysname, @cprec smallint, @cscale int, @cisnullable int, @dtext varchar(max)
    declare columncsr cursor for select N'[' + c.name + '] ' 
             + case t.name when 'text' then 'varchar(max)' when 'ntext' then 'nvarchar(max)' when 'image' then 'varbinary(max)' else t.name end 
			 + case when t.name in('char', 'nchar', 'varchar', 'nvarchar', 'binary', 'varbinary') then '(' + case convert(smallint, case when c.system_type_id in (34, 35, 99) then null when c.system_type_id = 36 then c.precision when c.max_length = -1 then -1 else odbcprec(c.system_type_id, c.max_length, c.precision) end) when -1 then 'max' else convert(varchar(10), convert(smallint, case when c.system_type_id in (34, 35, 99) then null when c.system_type_id = 36 then c.precision when c.max_length = -1 then -1 else odbcprec(c.system_type_id, c.max_length, c.precision) end)) end + ')'
                    when t.name in('decimal', 'numeric') then '(' + convert(varchar(10), convert(smallint, odbcprec(c.system_type_id, c.max_length, c.precision))) + ', ' + convert(varchar(10), odbcscale(c.system_type_id, c.scale)) + ')'
                    else '' end
             + case when c.name = @ident then ' identity' else case c.is_nullable when 1 then ' null' else ' not null' end end
             + case when c.name = 'CompanyID' or c.name = 'DeletedDatabaseRecord' then ' default 0' when c.name = 'CompanyMask' then ' default 0xAA' when c.name = 'GroupMask' then ' default 0x'
                when @ident is not null and substring(@ident, 1, 7) = 'Company' and 'Company' + c.name = @ident then ' default ident_current(''' + @tableName +''')'
                when d.definition = '(getdate())' then ' default getdate()'
				when d.definition = '(newsequentialid())' then ' default newsequentialid()' 
				else '' end,
      c.name, c.column_id, case t.name when 'text' then 'varchar' when 'ntext' then 'nvarchar' when 'image' then 'varbinary' else t.name end, convert(smallint, case when t.name in ('ntext', 'text', 'image') then -1 when c.system_type_id in (34, 35, 99) then null when c.system_type_id = 36 then c.precision when c.max_length = -1 then -1 else odbcprec(c.system_type_id, c.max_length, c.precision) end), odbcscale(c.system_type_id, c.scale), c.is_nullable, d.definition
      from sys.columns c
      inner join sys.types t on t.user_type_id = c.user_type_id
      left join sys.default_constraints d on d.object_id = c.default_object_id
      where c.object_id = @tableID
      order by c.column_id
    open columncsr
    fetch columncsr into @col, @cname, @ccolorder, @tname, @cprec, @cscale, @cisnullable, @dtext
    while @@fetch_status >= 0 begin
      if @cname = 'CompanyMask'
        set @masked = 1
	  else if @cname like 'Usr%' and @cname <> 'UsrCompanyMask'
	    set @userfields = @userfields + N' or name = ''' + @cname + ''''
      set @cnt = @cnt + 1
      set @stmt = @stmt + char(10) + char(9) + @col + ','
      if @cond = 0 begin
        print 'if (select count(*) from sys.columns c inner join sys.types t on t.user_type_id = c.user_type_id'
          + ' where c.object_id = object_id(''dbo.[' + @tableName + ']'')'
        print char(9) + 'and '
        + case when @ident is null or @ident <> @cname or substring(@ident, 1, 7) = 'Company' then '(c.name = ''' + @cname + '''' else  '((c.name = ''' + @cname + ''' or c.name = ''Company' + @cname + ''')' end
        + ' and c.column_id = ' + convert(varchar(10), @ccolorder)
        + ' and t.name = ''' + @tname + ''''
        + case when @cprec is not null then ' and convert(smallint, case when c.system_type_id in (34, 35, 99) then null when c.system_type_id = 36 then c.precision when c.max_length = -1 then -1 else odbcprec(c.system_type_id, c.max_length, c.precision) end)' + case when @cprec = -1 then ' = -1' else ' >= ' + convert(varchar(10), @cprec) end
          else ' and convert(smallint, case when c.system_type_id in (34, 35, 99) then null when c.system_type_id = 36 then c.precision when c.max_length = -1 then -1 else odbcprec(c.system_type_id, c.max_length, c.precision) end) is null' end
        + case when @cscale is not null then ' and odbcscale(c.system_type_id, c.scale) >= ' + convert(varchar(10), @cscale) else ' and odbcscale(c.system_type_id, c.scale) is null' end
        + ' and c.is_nullable = ' + convert(varchar(10), @cisnullable)
        set @cond = 1
      end
      else
        print char(9) + 'or '
        + case when @ident is null or @ident <> @cname or substring(@ident, 1, 7) = 'Company' then 'c.name = ''' + @cname + '''' else  '(c.name = ''' + @cname + ''' or c.name = ''Company' + @cname + ''')' end
        + ' and c.column_id = ' + convert(varchar(10), @ccolorder)
        + ' and t.name = ''' + @tname + ''''
        + case when @cprec is not null then ' and convert(smallint, case when c.system_type_id in (34, 35, 99) then null when c.system_type_id = 36 then c.precision when c.max_length = -1 then -1 else odbcprec(c.system_type_id, c.max_length, c.precision) end)' + case when @cprec = -1 then ' = -1' else ' >= ' + convert(varchar(10), @cprec) end
          else ' and convert(smallint, case when c.system_type_id in (34, 35, 99) then null when c.system_type_id = 36 then c.precision when c.max_length = -1 then -1 else odbcprec(c.system_type_id, c.max_length, c.precision) end) is null' end
        + case when @cscale is not null then ' and odbcscale(c.system_type_id, c.scale) >= ' + convert(varchar(10), @cscale) else ' and odbcscale(c.system_type_id, c.scale) is null' end
        + ' and c.is_nullable = ' + convert(varchar(10), @cisnullable)
      if @tname <> 'timestamp' begin
        if @cisnullable = 0 and @dtext is null
          if @cname = 'CreatedByID' or @cname = 'LastModifiedByID'
            set @dtext = '''B5344897-037E-4D58-B5C3-1BDFD0F47BF9'''
          else if @cname = 'CreatedByScreenID' or @cname = 'LastModifiedByScreenID'
            set @dtext = '''00000000'''
          else if @cname = 'CreatedDateTime' or @cname = 'LastModifiedDateTime'
            set @dtext = 'getdate()'
          else if @tname = 'binary' or @tname = 'varbinary'
            set @dtext = '0x'
          else if @tname = 'uniqueidentifier'
            set @dtext = 'newid()'
          else if @tname = 'decimal' or @tname = 'float'
            or @tname = 'real' or @tname = 'numeric'
            or @tname = 'int' or @tname = 'smallint'
            or @tname = 'tinyint' or @tname = 'bigint'
            or @tname = 'bit' or @tname = 'money'
            or @tname = 'smallmoney'
            set @dtext = '0'
        if @ident is not null and @cname = @ident begin
          if substring(@ident, 1, 7) = 'Company'
            set @selident = 'coalesce([' + @cname + '], [' + substring(@ident, 8, len(@ident) - 7) + '])'
          else
            set @selident = '[' + @cname + ']'
        end
        else if @fields is null begin
          set @fields = '[' + @cname + ']'
          if @cisnullable = 1 or @dtext like '%newsequentialid%' -- that can only be default
            set @selection = '[' + @cname + ']'
          else
            set @selection = 'coalesce([' + @cname + '], ' + case when @dtext is not null then replace(@dtext, '''', '''''') else '''''''''' end + ')'
        end
        else begin
          set @fields = @fields + ', [' + @cname + ']'
          if @cisnullable = 1 or @dtext like '%newsequentialid%' -- that can only be default
            set @selection = @selection + ', [' + @cname + ']'
          else
            set @selection = @selection + ', coalesce([' + @cname + '], ' + case when @dtext is not null then replace(@dtext, '''', '''''') else '''''''''' end + ')'
        end
      end
      if @cname = 'CompanyID'
        set @companyExists = 1
      if @cname = 'NoteID'
        set @noteExists = 1
      fetch columncsr into @col, @cname, @ccolorder, @tname, @cprec, @cscale, @cisnullable, @dtext
    end
    close columncsr
    deallocate columncsr
    print char(9) + ')) <> ' + convert(varchar(10), @cnt)
 	SET @temp = char(9) + 'or (select count(*) from sys.columns where object_id = object_id(''dbo.[' + @tableName + ']'') and (substring(name, 1, 3) <> ''Usr'''
	  + @userfields
      + case @masked when 1 then ' or name = ''UsrCompanyMask'')' else ')' end
      + case @masked when 0 then ' and name <> ''CompanyMask''' else '' end
      + case when @masked = 0 and @ident is not null and @ident <> 'ID' then ' and name <> ''Company' + @ident + '''' else '' end
      + ') <> ' + convert(varchar(10), @cnt)
	exec pp_PrintString @temp

    declare @indid smallint
    declare @primary varchar(1000)
    set @primary = null
    select @indid = index_id,
      @primary = 'constraint [' + case when name like 'Usr%' then 'Usr' else '' end + @tableName + '_PK] primary key '
               + case when index_id = 1 then 'clustered ' else '' end
    from sys.indexes where is_primary_key = 1 and object_id = @tableID
    if @primary is not null begin
      declare @pkcheck varchar(max)
      set @pkcheck = 'or not exists(select * from sys.indexes where name = ''' + @tableName + '_PK'''
      declare @cols varchar(1000)
      set @cols = ''
      declare @k int
      set @k = 1
      declare @nextcol sysname
      declare @lastcol sysname
      while @k > 0 begin
        select @nextcol = index_col('dbo.[' + @tableName + ']', @indid, @k)
        if @nextcol is not null begin
         set @pkcheck = @pkcheck + ' and index_col(''dbo.[' + @tableName + ']'', index_id, ' + convert(varchar(10), @k) + ') = '''
		  + case when @nextcol <> 'CompanyID' and @nextcol like 'Company%' then substring(@nextcol, 8, len(@nextcol) - 7) else @nextcol end + ''''
          if @cols = ''
            set @cols = '[' + @nextcol + ']'
          else
            set @cols = @cols + ', [' + @nextcol + ']'
          set @lastcol = @nextcol
          set @k = @k + 1
        end else begin
          set @pkcheck = @pkcheck + ' and index_col(''dbo.[' + @tableName + ']'', index_id, ' + convert(varchar(10), @k) + ') is null)'
          set @k = -1
        end
      end
      set @primary = @primary + '(' + @cols + ')'
      set @stmt = @stmt + char(10) + char(9) + @primary
      print char(9) + @pkcheck
    end
    if @ident is not null
      print char(9) + 'or not exists(select * from sys.identity_columns where object_id = object_id(''dbo.[' + @tableName + ']'') and (name = ''' + @ident + ''' or name = ''Company' + @ident + '''))'
    else
      print char(9) + 'or exists(select * from sys.identity_columns where object_id = object_id(''dbo.[' + @tableName + ']''))'
    set @stmt = @stmt + char(10) + ')'
    declare @indchecktable table (id int identity(1, 1), stmt varchar(max) not null, primary key clustered (id))
    declare @indcheck varchar(max)
    set @indcheck = null
    delete @indchecktable
    declare @indcreatetable table (id int identity(1, 1), stmt varchar(max) not null, primary key clustered (id))
    declare @indcreate varchar(max)
    set @indcreate = null
    delete @indcreatetable
    declare @index varchar(1000)
    declare @indname sysname
    declare @indexcnt int
    set @indexcnt = 0
    declare indexcsr cursor for select index_id, 'create '
      + case is_unique when 1 then 'unique ' else '' end
      + case index_id when 1 then 'clustered ' else '' end
      + 'index [' + name + ']',
      name
      from sys.indexes where object_id = @tableID and index_id > 0 and index_id < 255 and is_primary_key = 0
    open indexcsr
    fetch indexcsr into @indid, @index, @indname
    while @@fetch_status >= 0 begin
      set @indexcnt = @indexcnt + 1
      if @indexcnt = 1
        set @indcheck = 'if (select count(*) from sys.indexes si where object_id = object_id(''dbo.[' + @tableName + ']'')' + char(10) + char(9) + 'and (name = ''' + @indname + ''''
      else
        set @indcheck = char(9) + 'or name = ''' + @indname + ''''
      set @cols = ''
      set @k = 1
      while @k > 0 begin
        set @nextcol = index_col('dbo.[' + @tableName + ']', @indid, @k)
        if @nextcol is not null begin
          set @lastcol = @nextcol
          set @indcheck = @indcheck + ' and index_col(''dbo.[' + @tableName + ']'', index_id, ' + convert(varchar(10), @k) + ') = ''' + @nextcol + ''''
		  if (select is_descending_key from sys.index_columns where object_id = @tableID AND index_id = @indid AND index_column_id = @k) = 1 begin
            set @nextcol = '[' + @nextcol + '] desc'
            set @indcheck = @indcheck + ' and (select is_descending_key from sys.index_columns where object_id=object_id(''dbo.[' + @tableName + ']'') and index_id=si.index_id and index_column_id=' + convert(varchar(10), @k) + ') = 1'
          end else begin
            set @nextcol = '[' + @nextcol + ']'
            set @indcheck = @indcheck + ' and (select is_descending_key from sys.index_columns where object_id=object_id(''dbo.[' + @tableName + ']'') and index_id=si.index_id and index_column_id=' + convert(varchar(10), @k) + ') = 0'
          end
          if @cols = ''
            set @cols = @nextcol
          else
            set @cols = @cols + ', ' + @nextcol
          set @k = @k + 1
        end else begin
          set @indcheck = @indcheck + ' and index_col(''dbo.[' + @tableName + ']'', index_id, ' + convert(varchar(10), @k) + ') is null'
          set @k = -1
        end
      end
      insert @indchecktable (stmt) values (@indcheck)
      set @index = @index + ' on dbo.[' + @tableName + '] (' + @cols + ')'
      set @stmt = @stmt + char(10) + @index
      set @indcreate = char(9) + char(9) + @index
	  if (select serverproperty('isfulltextinstalled')) = 1 begin
        if exists(select * from sys.fulltext_indexes where object_id = @tableID and unique_index_id = @indid) begin
          select @nextcol = c.name from sys.fulltext_index_columns f inner join sys.columns c on c.object_id = f.object_id and c.column_id = f.column_id where c.object_id = @tableID
          set @indcreate = @indcreate + char(10) + char(9) + char(9) + 'exec dbo.pp_DropConstraint ''' + @tableName + ''', ''' + @lastcol + ''''
          set @indcreate = @indcreate + char(10) + char(9) + char(9) + 'alter table dbo.[' + @tableName + '] add constraint [' + @tableName + '_FT] default newid() for [' + @lastcol + ']'
          set @indcreate = @indcreate + char(10) + char(9) + char(9) + 'if (select serverproperty(''isfulltextinstalled'')) = 1'
          set @indcreate = @indcreate + char(10) + char(9) + char(9) + char(9) + 'exec sp_executesql N''create fulltext index on dbo.[' + @tableName + '] ([' + @nextcol + ']) key index ' + @indname + ''''
          set @stmt = @stmt + char(10) + 'exec dbo.pp_DropConstraint ''' + @tableName + ''', ''' + @lastcol + ''''
          set @stmt = @stmt + char(10) + 'alter table ' + @tableName + ' add constraint [' + @tableName + '_FT] default newid() for [' + @lastcol + ']'
          set @stmt = @stmt + char(10) + 'if (select serverproperty(''isfulltextinstalled'')) = 1'
          set @stmt = @stmt + char(10) + char(9) + 'exec sp_executesql N''create fulltext index on dbo.[' + @tableName + '] ([' + @nextcol + ']) key index ' + @indname + ''''
        end
	  end
      insert @indcreatetable (stmt) values (@indcreate)
      if @indname = @tableName + '_NoteID'
        set @noteExists = 0
      fetch indexcsr into @indid, @index, @indname
    end
    close indexcsr
    deallocate indexcsr
    if @companyExists = 1 and @noteExists = 1 begin
      if @indexcnt = 0
        set @indcheck = 'if (select count(*) from sys.indexes si where object_id = object_id(''dbo.[' + @tableName + ']'')' + char(10) + char(9) + 'and (name = ''' + @tableName + '_NoteID'''
      else
        set @indcheck = char(9) + 'or name = ''' + @tableName + '_NoteID'''
      set @indcheck = @indcheck + ' and index_col(''dbo.[' + @tableName + ']'', index_id, 1) = ''CompanyID'''
      set @indcheck = @indcheck + ' and index_col(''dbo.[' + @tableName + ']'', index_id, 2) = ''NoteID'''
      set @indcheck = @indcheck + ' and index_col(''dbo.[' + @tableName + ']'', index_id, 3) is null'
	  set @indcheck = @indcheck + ' and (select is_descending_key from sys.index_columns where object_id=object_id(''dbo.[' + @tableName + ']'') and index_id=si.index_id and index_column_id=1) = 0'
	  set @indcheck = @indcheck + ' and (select is_descending_key from sys.index_columns where object_id=object_id(''dbo.[' + @tableName + ']'') and index_id=si.index_id and index_column_id=2) = 0'
      insert @indchecktable (stmt) values (@indcheck)
      if @indexcnt = 0
        set @indcreate = char(9) + char(9) + 'create index [' + @tableName + '_NoteID] on dbo.[' + @tableName + '] (CompanyID, NoteID)'
      else
        set @indcreate = char(9) + char(9) + 'create index [' + @tableName + '_NoteID] on dbo.[' + @tableName + '] (CompanyID, NoteID)'
      set @indexcnt = @indexcnt + 1
      insert @indcreatetable (stmt) values (@indcreate)
    end
    print 'begin'
    print 'if exists(select * from sys.objects where object_id = object_id(''dbo.[' + @tableName + ']'')) begin'
	print char(9) + 'if not exists(select * from sys.objects where object_id = object_id(''dbo.[Removed' + @tableName + ']'')) begin'
    print char(9) + char(9) + 'execute pp_RemoveConstraints ''' + @tableName + ''''
    print char(9) + char(9) + 'execute sp_rename ''dbo.[' + @tableName + ']'', ''Removed' + @tableName + ''''
    print char(9) + 'end'	
	print char(9) + 'else begin'
	print char(9) + char(9) + 'raiserror(''Last update of table ' + @tableName + ' was complete unsucsessfully. Please check data in Removed'  + @tableName + ' table or drop it.'' , 16, 1)'
	print char(9) + 'end'	
	print 'end'	
    print 'if not exists(select * from sys.objects where object_id = object_id(''dbo.[' + @tableName + ']'')) begin'
	exec pp_PrintString @stmt
	if @companyExists = 1 and @noteExists = 1 begin
      set @stmt = 'create index [' + @tableName + '_NoteID] on dbo.[' + @tableName + '] (CompanyID, NoteID)'
      print @stmt
    end
    print 'end'
    print 'if exists(select * from sys.objects where object_id = object_id(''dbo.[Removed' + @tableName + ']'')) begin'
    SET @temp = char(9) + 'execute pp_MoveData ''' + @tableName + ''', ''' + @fields + ''','
	exec pp_printString @temp
    SET @temp = char(9) + char(9) + '''' + @selection + ''', ' + case when @ident is null then 'null, null' else '''' + @ident + ''', ''' +@selident + '''' end
	exec pp_printString @temp
    print char(9) + 'if exists(select * from dbo.[' + @tableName + ']) or not exists(select * from dbo.[Removed' + @tableName + '])'
    print char(9) + char(9) + 'drop table dbo.[Removed' + @tableName + ']'
    print 'end'
    print 'end'
    if @indexcnt > 0 begin
      print 'else begin'
      declare indcheck_csr cursor for select stmt from @indchecktable order by id
      open indcheck_csr
      fetch indcheck_csr into @indcheck
      while @@fetch_status >= 0 begin
        print @indcheck
        fetch indcheck_csr into @indcheck
      end
      close indcheck_csr
      deallocate indcheck_csr
      print char(9) + ')) <> ' + convert(varchar(10), @indexcnt)
      print char(9) + 'or (select count(*) from sys.indexes where object_id = object_id(''dbo.[' + @tableName
                    + ']'') and index_id > 0 and index_id < 255 and name <> ''' + @tableName + '_PK'') <> '
                    + convert(varchar(10), @indexcnt)
      print char(9) + 'begin'
      print char(9) + char(9) + 'exec pp_DropIndexes ''' + @tableName + ''''
      declare indcreate_csr cursor for select stmt from @indcreatetable order by id
      open indcreate_csr
      fetch indcreate_csr into @indcreate
      while @@fetch_status >= 0 begin
        print @indcreate
        fetch indcreate_csr into @indcreate
      end
      close indcreate_csr
      deallocate indcreate_csr
      print char(9) + 'end'
      print 'end'
    end
    print 'go'
    fetch tablecsr into @tableName, @tableID
  end
  close tablecsr
  deallocate tablecsr

  declare tablecsr cursor for select name, object_id from sys.objects where objectproperty(object_id, 'IsUserTable') = 1
	and (@specificTables is NULL or name in (select tableName from #specificTablesList))
  open tablecsr
  fetch tablecsr into @tableName, @tableID
  while @@fetch_status >= 0 begin
    set @stmt = null
    declare @delforeignstable table (id int identity(1, 1), stmt varchar(max) not null, primary key clustered (id))
    declare @deltriggerstable table (id int identity(1, 1), stmt varchar(max) not null, primary key clustered (id))
    declare @secondstmttable table (id int identity(1, 1), stmt varchar(max) not null, primary key clustered (id))
    delete @secondstmttable
    declare @foreigns int
    set @foreigns = 0
    declare @lastfname sysname, @lastrtable sysname, @lastfcols varchar(1000), @lastrcols varchar(1000), @lastmodifier varchar(1000)
    set @lastfname = null
    declare @fname sysname, @rtable sysname, @fcol sysname, @rcol sysname, @modifier varchar(1000)
    declare @headfname sysname, @tailfname sysname
    declare @started bit
    set @started = 0
    declare foreigncsr cursor for select f.name, object_name(f.referenced_object_id), col_name(f.parent_object_id, c.parent_column_id), col_name(f.referenced_object_id, c.referenced_column_id),
      case when delete_referential_action = 1 then ' on delete cascade' else '' end
      from sys.foreign_keys f inner join sys.foreign_key_columns c on c.constraint_object_id = f.object_id
      where f.parent_object_id = @tableID
      order by f.name, c.constraint_column_id
    open foreigncsr
    fetch foreigncsr into @fname, @rtable, @fcol, @rcol, @modifier
    while @@fetch_status >= 0 begin
      if @started = 0 and @fcol <> 'CompanyID' and @rcol <> 'CompanyID' begin
        print '--' + @tableName
        print 'if (case when not exists(select * from sys.columns where object_id = object_id(''' + @tableName + ''') and name = ''CompanyMask'') then'
        delete @delforeignstable
        delete @deltriggerstable
        set @started = 1
      end
      if @lastfname is null or @lastfname <> @fname begin
        if @lastfname is not null and @headfname <> @lastrtable begin
          set @foreigns = @foreigns + 1
          print char(9) + case when @foreigns = 1 then '' else '+ ' end + 'case (select coalesce(sum(case name when ''CompanyMask'' then 1 else 2 end), 0) from sys.columns where object_id = object_id(''dbo.[' + @lastrtable +']'') and name in (''CompanyMask'', ''DeletedDatabaseRecord'', ''UsrDeletedDatabaseRecord''))'
          print char(9) + char(9) + 'when 0 then (select count(*) from sys.foreign_keys where name = ''' + @headfname + '__' + @tailfname + ''' and parent_object_id = object_id(''dbo.[' + @tableName + ']''))'
          print char(9) + char(9) + 'when 1 then (select count(*) from sys.triggers where name = ''' + @headfname + '__' + @tailfname + '_1'')'
          print char(9) + char(9) + 'when 2 then (select count(*) from sys.triggers where name = ''' + @headfname + '__' + @tailfname + '_2'')'
          print char(9) + char(9) + 'else (select count(*) from sys.triggers where name = ''' + @headfname + '__' + @tailfname + '_3'') end'
          insert @secondstmttable(stmt) values(case when @foreigns = 1 then char(9) + 'else (' else char(9) + '+ ' end + 'case (select coalesce(sum(case name when ''CompanyMask'' then 1 else 2 end), 0) from sys.columns where object_id = object_id(''dbo.[' + @lastrtable +']'') and name in (''CompanyMask'', ''DeletedDatabaseRecord'', ''UsrDeletedDatabaseRecord''))')
          insert @secondstmttable(stmt) values(char(9) + char(9) + 'when 0 then (select count(*) from sys.triggers where name = ''' + @headfname + '__' + @tailfname + ''')')
          insert @secondstmttable(stmt) values(char(9) + char(9) + 'when 1 then (select count(*) from sys.triggers where name = ''' + @headfname + '__' + @tailfname + '_1'')')
          insert @secondstmttable(stmt) values(char(9) + char(9) + 'when 2 then (select count(*) from sys.triggers where name = ''' + @headfname + '__' + @tailfname + '_2'')')
          insert @secondstmttable(stmt) values(char(9) + char(9) + 'else (select count(*) from sys.triggers where name = ''' + @headfname + '__' + @tailfname + '_3'') end')
          insert @delforeignstable(stmt) values(char(9) + char(9) + '''' + @headfname + '__' + @tailfname + ''',')
          insert @deltriggerstable(stmt) values(char(9) + char(9) + '''' + @headfname + '__' + @tailfname + ''',')
          insert @deltriggerstable(stmt) values(char(9) + char(9) + '''' + @headfname + '__' + @tailfname + '_1'',')
          insert @deltriggerstable(stmt) values(char(9) + char(9) + '''' + @headfname + '__' + @tailfname + '_2'',')
          insert @deltriggerstable(stmt) values(char(9) + char(9) + '''' + @headfname + '__' + @tailfname + '_3'',')
          if @stmt is null
            set @stmt = char(9) + 'alter table dbo.[' + @tableName + '] with nocheck add constraint [' + @headfname + '__' + @tailfname + '] foreign key (' + @lastfcols + ') references dbo.[' + @lastrtable + '] (' + @lastrcols + ')' + @lastmodifier
          else
            set @stmt = @stmt + char(10) + char(9) + 'alter table dbo.[' + @tableName + '] with nocheck add constraint [' + @headfname + '__' + @tailfname + '] foreign key (' + @lastfcols + ') references dbo.[' + @lastrtable + '] (' + @lastrcols + ')' + @lastmodifier
        end
        set @lastfname = @fname
        set @lastrtable = @rtable
        set @lastfcols = '[' + @fcol + ']'
        set @lastrcols = '[' + @rcol + ']'
        set @lastmodifier = @modifier
        if @fcol <> 'CompanyID' and @rcol <> 'CompanyID' begin
          set @headfname = @rtable + '_' + @fcol
          set @tailfname = @tableName + '_' + @rcol
        end else begin
          set @headfname = @rtable
          set @tailfname = @tableName
        end
      end
      else begin
        set @lastfcols = @lastfcols + ', [' + @fcol + ']'
        set @lastrcols = @lastrcols + ', [' + @rcol + ']'
        if @fcol <> 'CompanyID' and @rcol <> 'CompanyID' begin
          set @headfname = @headfname + '_' + @fcol
          set @tailfname = @tailfname + '_' + @rcol
        end
      end
      fetch foreigncsr into @fname, @rtable, @fcol, @rcol, @modifier
    end
    if @lastfname is not null and @headfname <> @lastrtable
       set @foreigns = @foreigns + 1
    if @foreigns > 0 begin
      if @headfname <> @lastrtable begin
        print char(9) + case when @foreigns = 1 then '' else '+ ' end + 'case (select coalesce(sum(case name when ''CompanyMask'' then 1 else 2 end), 0) from sys.columns where object_id = object_id(''dbo.[' + @lastrtable +']'') and name in (''CompanyMask'', ''DeletedDatabaseRecord'', ''UsrDeletedDatabaseRecord''))'
        print char(9) + char(9) + 'when 0 then (select count(*) from sys.foreign_keys where name = ''' + @headfname + '__' + @tailfname + ''' and parent_object_id = object_id(''dbo.[' + @tableName + ']''))'
        print char(9) + char(9) + 'when 1 then (select count(*) from sys.triggers where name = ''' + @headfname + '__' + @tailfname + '_1'')'
        print char(9) + char(9) + 'when 2 then (select count(*) from sys.triggers where name = ''' + @headfname + '__' + @tailfname + '_2'')'
        print char(9) + char(9) + 'else (select count(*) from sys.triggers where name = ''' + @headfname + '__' + @tailfname + '_3'') end'
        insert @secondstmttable(stmt) values(case when @foreigns = 1 then char(9) + 'else (' else char(9) + '+ ' end + 'case (select coalesce(sum(case name when ''CompanyMask'' then 1 else 2 end), 0) from sys.columns where object_id = object_id(''dbo.[' + @lastrtable +']'') and name in (''CompanyMask'', ''DeletedDatabaseRecord'', ''UsrDeletedDatabaseRecord''))')
        insert @secondstmttable(stmt) values(char(9) + char(9) + 'when 0 then (select count(*) from sys.triggers where name = ''' + @headfname + '__' + @tailfname + ''')')
        insert @secondstmttable(stmt) values(char(9) + char(9) + 'when 1 then (select count(*) from sys.triggers where name = ''' + @headfname + '__' + @tailfname + '_1'')')
        insert @secondstmttable(stmt) values(char(9) + char(9) + 'when 2 then (select count(*) from sys.triggers where name = ''' + @headfname + '__' + @tailfname + '_2'')')
        insert @secondstmttable(stmt) values(char(9) + char(9) + 'else (select count(*) from sys.triggers where name = ''' + @headfname + '__' + @tailfname + '_3'') end')
        insert @delforeignstable(stmt) values(char(9) + char(9) + '''' + @headfname + '__' + @tailfname + ''')')
        insert @deltriggerstable(stmt) values(char(9) + char(9) + '''' + @headfname + '__' + @tailfname + ''',')
        insert @deltriggerstable(stmt) values(char(9) + char(9) + '''' + @headfname + '__' + @tailfname + '_1'',')
        insert @deltriggerstable(stmt) values(char(9) + char(9) + '''' + @headfname + '__' + @tailfname + '_2'',')
        insert @deltriggerstable(stmt) values(char(9) + char(9) + '''' + @headfname + '__' + @tailfname + '_3'')')
      end else begin
        update @delforeignstable set stmt = substring(stmt, 1, len(stmt) - 1) + ')' where id = (select MAX(id) from @delforeignstable)
        update @deltriggerstable set stmt = substring(stmt, 1, len(stmt) - 1) + ')' where id = (select MAX(id) from @deltriggerstable)
      end
      update @secondstmttable set stmt = stmt + ') end) <> ' + convert(varchar(10), @foreigns) where id = (select MAX(id) from @secondstmttable)
      update @delforeignstable set stmt = stmt + ')' where id = (select MAX(id) from @delforeignstable)
      update @deltriggerstable set stmt = stmt + ' and name like ''%[^_][_][_]' + @tableName +'[_][^_]%'')' where id = (select MAX(id) from @deltriggerstable)
      declare @secondstmt varchar(max)
      declare secondstmt_csr cursor for select stmt from @secondstmttable order by id
      open secondstmt_csr
      fetch secondstmt_csr into @secondstmt
      while @@fetch_status >= 0 begin
        print @secondstmt
        fetch secondstmt_csr into @secondstmt
      end
      close secondstmt_csr
      deallocate secondstmt_csr
      print char(9) + 'or exists(select * from sys.foreign_keys where parent_object_id = object_id(''dbo.[' + @tableName +']'') and name not in ('
      declare @delforeigns varchar(max)
      declare delforeigns_csr cursor for select stmt from @delforeignstable order by id
      open delforeigns_csr
      fetch delforeigns_csr into @delforeigns
      while @@fetch_status >= 0 begin
        print @delforeigns
        fetch delforeigns_csr into @delforeigns
      end
      close delforeigns_csr
      deallocate delforeigns_csr
      print char(9) + 'or exists(select * from sys.triggers where name not in ('
      declare @deltriggers varchar(max)
      declare deltriggers_csr cursor for select stmt from @deltriggerstable order by id
      open deltriggers_csr
      fetch deltriggers_csr into @deltriggers
      while @@fetch_status >= 0 begin
        print @deltriggers
        fetch deltriggers_csr into @deltriggers
      end
      close deltriggers_csr
      deallocate deltriggers_csr
      print 'begin'
      print char(9) + 'exec pp_DropForeigns ''' + @tableName + ''''
      if @headfname <> @lastrtable begin
        if @stmt is null
          set @stmt = char(9) + 'alter table dbo.[' + @tableName + '] with nocheck add constraint [' + @headfname + '__' + @tailfname + '] foreign key (' + @lastfcols + ') references dbo.[' + @lastrtable + '] (' + @lastrcols + ')' + @lastmodifier
        else
          set @stmt = @stmt + char(10) + char(9) + 'alter table dbo.[' + @tableName + '] with nocheck add constraint [' + @headfname + '__' + @tailfname + '] foreign key (' + @lastfcols + ') references dbo.[' + @lastrtable + '] (' + @lastrcols + ')' + @lastmodifier
      end
      print @stmt
      print char(9) + 'exec pp_AdjustForeigns ''' + @tableName + ''''
      print 'end'
      print 'go'
    end
    close foreigncsr
    deallocate foreigncsr
    fetch tablecsr into @tableName, @tableID
  end
  close tablecsr
  deallocate tablecsr
  
  DROP TABLE #specificTablesList
  set nocount off
go

if exists(select * from sys.objects where object_id = object_id('dbo.pp_GenerateFirst') and objectproperty(object_id,'IsProcedure')=1)
drop proc dbo.pp_GenerateFirst
go
create procedure dbo.pp_GenerateFirst @specificTables varchar(MAX) = NULL as
  declare @stmt varchar(max)

  --if we using some specific tables
  CREATE TABLE #specificTablesList (tableName sysname COLLATE DATABASE_DEFAULT)
  if(@specificTables is not null) BEGIN
    SET @specificTables = REPLACE( @specificTables, ' ', '')
	SET @stmt = 'SELECT ''' + REPLACE(@specificTables, ',', ''' UNION SELECT ''') + ''''
	INSERT #specificTablesList EXEC(@stmt)
  END

  declare @ftdisable varchar(max)
  set @ftdisable = ''
  declare @ftenable varchar(max)
  set @ftenable = ''
  print '--FullText'
  print 'if (select serverproperty(''isfulltextinstalled'')) = 1 begin'
  print char(9) + 'declare @cnt int'
  print char(9) + 'exec sp_executesql N''select @counter=count(*) from sys.fulltext_catalogs'', N''@counter int output'', @counter=@cnt output'
  print char(9) + 'if @cnt=0 begin'
  print char(9) + char(9) + 'exec dbo.sp_fulltext_database ''enable'''
  print char(9) + char(9) + 'exec sp_executesql N''create fulltext catalog ft as default'''
  print char(9) + 'end'
  print 'end'
  print 'go'
  declare @tableName sysname
  declare @tableID int
  declare tablecsr cursor for select name, object_id from sys.objects where objectproperty(object_id, 'IsUserTable') = 1
  	and (@specificTables is NULL or name in (select tableName from #specificTablesList)) order by name
  open tablecsr
  fetch tablecsr into @tableName, @tableID
  while @@fetch_status >= 0 begin
    print '---' + @tableName
    print 'if exists(select * from sys.objects where object_id = object_id(''dbo.[' + @tableName + ']'')) begin'
    declare @ident sysname
    set @ident = null
    select @ident = name from sys.identity_columns where object_id = @tableID
    declare @companyExists bit
    set @companyExists = 0
    declare @noteExists bit
    set @noteExists = 0
    declare @col sysname
    declare @cname sysname
    declare columncsr cursor for select N'[' + c.name + '] ' +
      case t.name when 'text' then 'varchar(max)' when 'ntext' then 'nvarchar(max)' when 'image' then 'varbinary(max)' else t.name end + case when t.name in('char', 'nchar', 'varchar', 'nvarchar', 'binary', 'varbinary') then '(' + case convert(smallint, case when c.system_type_id in (34, 35, 99) then null when c.system_type_id = 36 then c.precision when c.max_length = -1 then -1 else odbcprec(c.system_type_id, c.max_length, c.precision) end) when -1 then 'max' else convert(varchar(10), convert(smallint, case when c.system_type_id in (34, 35, 99) then null when c.system_type_id = 36 then c.precision when c.max_length = -1 then -1 else odbcprec(c.system_type_id, c.max_length, c.precision) end)) end + ')'
                    when t.name in('decimal', 'numeric') then '(' + convert(varchar(10), convert(smallint, case when c.system_type_id in (34, 35, 99) then null when c.system_type_id = 36 then c.precision when c.max_length = -1 then -1 else odbcprec(c.system_type_id, c.max_length, c.precision) end)) + ', ' + convert(varchar(10), odbcscale(c.system_type_id, c.scale)) + ')'
                    else '' end,
      c.name
      from sys.columns c
      inner join sys.types t on t.user_type_id = c.user_type_id
      where c.object_id = @tableID
      order by c.column_id
    open columncsr
    fetch columncsr into @col, @cname
    while @@fetch_status >= 0 begin
      print char(9) + 'if not exists(select * from sys.columns where object_id = object_id(''dbo.[' + @tableName + ']'') and name = ''' + @cname + ''')'
      if @ident is not null and @ident = @cname begin
        print char(9) + char(9) + 'if not exists(select * from sys.identity_columns where object_id = object_id(''dbo.[' + @tableName + ']''))'
        print char(9) + char(9) + char(9) + 'alter table dbo.[' + @tableName + '] add ' + @col + ' identity'
        print char(9) + char(9) + 'else'
        print char(9) + char(9) + char(9) + 'alter table dbo.[' + @tableName + '] add ' + @col + ' null'
      end
      else
        print char(9) + char(9) + 'alter table dbo.[' + @tableName + '] add ' + @col + ' null'
      if @cname = 'CompanyID'
        set @companyExists = 1
      if @cname = 'NoteID'
        set @noteExists = 1
      fetch columncsr into @col, @cname
    end
    close columncsr
    deallocate columncsr
    print 'end'
    print 'else begin'
    set @stmt = 'create table dbo.[' + @tableName + ']'
    set @stmt = @stmt + char(10) + '('
	print @stmt
	
    declare @allnulls varchar(max)
    set @allnulls = null
    declare columncsr cursor for select N'[' + c.name + '] ' 
			 + case t.name when 'text' then 'varchar(max)' when 'ntext' then 'nvarchar(max)' when 'image' then 'varbinary(max)' else t.name end 
			 + case when t.name in('char', 'nchar', 'varchar', 'nvarchar', 'binary', 'varbinary') then '(' + case convert(smallint, case when c.system_type_id in (34, 35, 99) then null when c.system_type_id = 36 then c.precision when c.max_length = -1 then -1 else odbcprec(c.system_type_id, c.max_length, c.precision) end) when -1 then 'max' else convert(varchar(10), convert(smallint, case when c.system_type_id in (34, 35, 99) then null when c.system_type_id = 36 then c.precision when c.max_length = -1 then -1 else odbcprec(c.system_type_id, c.max_length, c.precision) end)) end + ')'
                    when t.name in('decimal', 'numeric') then '(' + convert(varchar(10), convert(smallint, odbcprec(c.system_type_id, c.max_length, c.precision))) + ', ' + convert(varchar(10), odbcscale(c.system_type_id, c.scale)) + ')'
                    else '' end
             + case when c.name = @ident then ' identity' else case when c.name <> 'ID' and (@tableName = 'AppSchema' or @tableName = 'SysSchema') then ' null' else
               case c.is_nullable when 1 then ' null' else ' not null' end end end
             + case when c.name = 'CompanyID' or c.name = 'DeletedDatabaseRecord' then ' default 0' when c.name = 'CompanyMask' then ' default 0xAA' when c.name = 'GroupMask' then ' default 0x'
                    when @ident is not null and substring(@ident, 1, 7) = 'Company' and 'Company' + c.name = @ident then ' default ident_current(''' + @tableName +''')'
                    when d.definition = '(getdate())' then ' default getdate()' else '' end
      from sys.columns c
      inner join sys.types t on t.user_type_id = c.user_type_id
      left join sys.default_constraints d on d.object_id = c.default_object_id
      where c.object_id = @tableID
      order by c.column_id
    open columncsr
    fetch columncsr into @col
    while @@fetch_status >= 0 begin
      set @stmt = char(9) + @col + ','
	  print @stmt
	  
      if @allnulls is null
        set @allnulls = 'values(1'
      else
        set @allnulls = @allnulls + ', null'
      fetch columncsr into @col
    end
    close columncsr
    deallocate columncsr
	set @stmt = ''
	
    declare @indid smallint
    declare @primary varchar(1000)
    set @primary = null
    select @indid = index_id,
      @primary = 'constraint [' + case when name like 'Usr%' then 'Usr' else '' end + @tableName + '_PK] primary key '
               + case when index_id = 1 then 'clustered ' else '' end
    from sys.indexes where is_primary_key = 1 and object_id = @tableID
    declare @nextcol sysname
    declare @lastcol sysname
    if @primary is not null begin
      declare @cols varchar(1000)
      set @cols = ''
      declare @k int
      set @k = 1
      while @k > 0 begin
        set @nextcol = index_col(N'dbo.[' + @tableName + ']', @indid, @k)
        if @nextcol is not null begin
         set @lastcol = @nextcol
          set @nextcol = '[' + @nextcol + ']'
          if @cols = ''
            set @cols = @nextcol
          else
            set @cols = @cols + ', ' + @nextcol
          set @k = @k + 1
        end else
          set @k = -1
      end
      set @primary = @primary + '(' + @cols + ')'
      set @stmt = @stmt + char(10) + char(9) + @primary
    end
    set @stmt = @stmt + char(10) + ')'
    declare @index varchar(1000)
    declare @indname sysname
    declare indexcsr cursor for select index_id, name, 'create '
      + case is_unique when 1 then 'unique ' else '' end
      + case index_id when 1 then 'clustered ' else '' end
      + 'index [' + name + ']'
      from sys.indexes where object_id = @tableID and index_id > 0 and index_id < 255 and is_primary_key = 0
    open indexcsr
    fetch indexcsr into @indid, @indname, @index
    while @@fetch_status >= 0 begin
      set @cols = ''
      set @k = 1
      while @k > 0 begin
        set @nextcol = index_col(N'dbo.[' + @tableName + ']', @indid, @k)
        if @nextcol is not null begin
          set @lastcol = @nextcol
          set @nextcol = '[' + @nextcol + ']'
		  if  (select is_descending_key from sys.index_columns where object_id = @tableID AND index_id = @indid AND index_column_id = @k) = 1 
            set @nextcol = @nextcol + ' desc'
          if @cols = ''
            set @cols = @nextcol
          else
            set @cols = @cols + ', ' + @nextcol
          set @k = @k + 1
        end else
          set @k = -1
      end
      set @index = @index + ' on dbo.[' + @tableName + '] (' + @cols + ')'
      set @stmt = @stmt + char(10) + @index
	  if (select serverproperty('isfulltextinstalled')) = 1 begin
        if exists(select * from sys.fulltext_indexes where object_id = @tableID and unique_index_id = @indid) begin
          select @nextcol = c.name from sys.fulltext_index_columns f inner join sys.columns c on c.object_id = f.object_id and c.column_id = f.column_id where c.object_id = @tableID
          set @stmt = @stmt + char(10) + 'exec dbo.pp_DropConstraint ''' + @tableName + ''', ''' + @lastcol + ''''
          set @stmt = @stmt + char(10) + 'alter table dbo.[' + @tableName + '] add constraint [' + @tableName + '_FT] default newid() for [' + @lastcol + ']'
          set @stmt = @stmt + char(10) + 'if (select serverproperty(''isfulltextinstalled'')) = 1'
          set @stmt = @stmt + char(10) + char(9) + 'exec sp_executesql N''create fulltext index on dbo.[' + @tableName + '] ([' + @nextcol + ']) key index [' + @indname + ']'''
          set @ftdisable = @ftdisable + char(10) + char(9) + 'if (select serverproperty(''''''''EngineEdition'''''''')) <> 5'
          set @ftdisable = @ftdisable + char(10) + char(9) + 'if (exists(SELECT * FROM sys.fulltext_indexes fi JOIN sys.indexes i ON i.[object_id] = fi.[object_id] AND i.[name] = ''''''''' + @indname + '''''''''))'
          set @ftdisable = @ftdisable + char(10) + char(9) + char(9) + 'exec sp_executesql N''''''''drop fulltext index on dbo.[' + @tableName + ']'''''''''
          set @ftdisable = @ftdisable + char(10) + char(9) + 'if (exists (select * from sys.indexes where name = ''''''''' + @indname + '''''''''))'
	  	  set @ftdisable = @ftdisable + char(10) + char(9) + 'drop index [' + @indname + '] on dbo.[' + @tableName + ']'
		  set @ftenable = @ftenable + char(10) + char(9) + 'if (not exists (select * from sys.indexes where name = ''''''''' + @indname + '''''''''))'
  		  set @ftenable = @ftenable + char(10) + char(9) + 'BEGIN'
          set @ftenable = @ftenable + char(10) + char(9) + char(9) + 'update dbo.[' + @tableName + '] set [' + @lastcol + '] = newid()'
          set @ftenable = @ftenable + char(10) + char(9) + char(9) + 'create unique nonclustered index [' + @indname + '] on dbo.[' + @tableName + '] (' + @cols + ')'
          set @ftenable = @ftenable + char(10) + char(9) + char(9) + 'if (select serverproperty(''''''''isfulltextinstalled'''''''')) = 1'
          set @ftenable = @ftenable + char(10) + char(9) + char(9) + char(9) + 'exec sp_executesql N''''''''create fulltext index on dbo.[' + @tableName + '] ([' + @nextcol + ']) key index [' + @indname + ']'''''''''
		  set @ftenable = @ftenable + char(10) + char(9) + 'END'
        end
	  end
      if @indname = @tableName + '_NoteID'
        set @noteExists = 0
      fetch indexcsr into @indid, @indname, @index
    end
    close indexcsr
    deallocate indexcsr
    print @stmt
    if @companyExists = 1 and @noteExists = 1 begin
      set @stmt = 'create index [' + @tableName + '_NoteID] on dbo.[' + @tableName + '] (CompanyID, NoteID)'
      print @stmt
    end
    if @tableName = 'AppSchema' or @tableName = 'SysSchema' begin
      print 'exec sp_executesql N''insert dbo.[' + @tableName + ']'
      print @allnulls + ')'''
    end
    print 'end'	
    print 'go'
    fetch tablecsr into @tableName, @tableID
  end
  close tablecsr
  deallocate tablecsr
  if @ftdisable <> '' begin
    print '--FullText'
    print 'declare @sel nvarchar(max)'
    print 'declare @stmt nvarchar(max)'
    print 'set @sel = ''select @text = replace(definition, ''''create procedure'''', ''''alter procedure'''') + '''''
      + @ftdisable + ''''' from sys.sql_modules where object_id=object_id(''''dbo.pp_DisableFullText'''')'''
    print 'exec sp_executesql @sel, N''@text nvarchar(max) output'', @text = @stmt output'
    print 'exec sp_executesql @stmt'
    print 'go'
    print 'declare @sel nvarchar(max)'
    print 'declare @stmt nvarchar(max)'
    print 'set @sel = ''select @text = replace(definition, ''''create procedure'''', ''''alter procedure'''') + '''''
      + @ftenable + ''''' from sys.sql_modules where object_id=object_id(''''dbo.pp_EnableFullText'''')'''
    print 'exec sp_executesql @sel, N''@text nvarchar(max) output'', @text = @stmt output'
    print 'exec sp_executesql @stmt'
    print 'go'
  end
  
  --disabling triggers
  print ''
  print 'declare @trigname sysname'
  print 'declare @trigtable sysname'
  print 'declare @trigstmt nvarchar(max)'
  print 'declare ctrig cursor for select tr.name, tb.name from sys.triggers tr'
  print '	join sys.tables tb on tr.parent_id = tb.object_id'
  if(@specificTables is NOT NULL) print ' and tb.name in (''' + REPLACE(@specificTables, ',', ''', ''') + ''')'
  print 'open ctrig'
  print 'fetch ctrig into @trigname, @trigtable'
  print 'while @@fetch_status >= 0'
  print 'BEGIN'
  print '	set @trigstmt = ''DISABLE TRIGGER ['' + @trigname + ''] ON dbo.['' + @trigtable + '']'''
  print '	EXEC sp_executesql @trigstmt'
  print '  	fetch ctrig into @trigname, @trigtable'
  print 'END'
  print 'close ctrig'
  print 'deallocate ctrig'
  print 'go'
  
  DROP TABLE #specificTablesList
go

if exists(select * from sys.objects where object_id = object_id('dbo.pp_GetColumns') and objectproperty(object_id,'IsScalarFunction')=1)
drop FUNCTION dbo.pp_GetColumns
go
CREATE FUNCTION dbo.pp_GetColumns (@tablename sysname, @alias sysname = NULL, @excludedColumns nvarchar(MAX) = null/*, @onlyKeys int = 0*/)
RETURNS NVARCHAR(MAX)
AS
BEGIN
  DECLARE @stmt NVARCHAR(MAX)
  DECLARE @column NVARCHAR(50)
  
  if(@excludedColumns IS NULL) SET @excludedColumns = '';
  SET @excludedColumns = REPLACE(@excludedColumns, ' ', '')
  SET @excludedColumns = ',' + @excludedColumns + ','  
  
  declare crs cursor for(SELECT c.[name] FROM sys.[columns] c 
	left join sys.indexes i on c.object_id = i.object_id and i.is_primary_key = 1
	left join sys.index_columns ic on c.object_id = ic.object_id and i.index_id = ic.index_id and c.column_id = ic.column_id
    WHERE c.object_id  = Object_ID(@tablename) /*and (@onlyKeys = 0 OR ic.column_id is not null)*/	) 
  open crs
  fetch crs into @column
  while @@fetch_status >= 0
  BEGIN
  	IF
  	(
  			@column <> 'CompanyID'
  		AND @column <>'CompanyMask' 
		AND @column <>'tstamp' 
		AND CHARINDEX(',' + @column + ',', @excludedColumns, 0) <= 0
		--AND @column NOT IN (SELECT ic.[name] FROM sys.identity_columns ic
		--	JOIN sys.objects o ON o.[object_id] = ic.[object_id] WHERE o.[name] = @tablename)
  	)
  	BEGIN
  		if( @stmt IS NULL) SET @stmt = ''
  		else SET @stmt = @stmt + ', '
		if(@alias IS NOT NULL AND LEN(@alias) > 0) SET @stmt = @stmt + '[' + @alias + '].'
		SET @stmt = @stmt + '[' + @column + ']'
  	END
  	
  	fetch crs into @column
  END
  close crs
  deallocate crs
  
 
  RETURN(@stmt)
END
GO

if exists(select * from sys.objects where object_id = object_id('dbo.pp_CopyCompanyTable') and objectproperty(object_id,'IsProcedure')=1)
drop proc dbo.pp_CopyCompanyTable
go
create procedure dbo.pp_CopyCompanyTable @table sysname, @oldCompanyID int, @newCompanyID int AS
BEGIN
	 exec dbo.pp_CopyCompanyTableExt @table, DEFAULT, @oldCompanyID, @newCompanyID, DEFAULT, DEFAULT, DEFAULT
END
GO

if exists(select * from sys.objects where object_id = object_id('dbo.pp_CopyCompanyTableExt') and objectproperty(object_id,'IsProcedure')=1)
drop proc dbo.pp_CopyCompanyTableExt
go
create procedure dbo.pp_CopyCompanyTableExt @table sysname, @schema sysname = '', @oldCompanyID int, @newCompanyID int, @clearData bit = 0, @alias sysname = '', @condition NVARCHAR(MAX) = '' AS
BEGIN
	if(@table = 'LoginTrace') RETURN

	DECLARE @fullname sysname
	if(@schema is null or @schema = '') SET @schema = 'dbo'
	set @fullname = '[' + @schema + '].[' + @table + ']'
	
 	if Exists(SELECT * FROM sys.COLUMNS c
 				JOIN sys.objects o ON o.[object_id] = c.[object_id]
				WHERE o.[name] = @table AND c.[name] = 'CompanyID')
 	BEGIN
 		PRINT 'Copy table ' + @table
 		
		DECLARE @parent int
 		DECLARE @tbl sysname
 		DECLARE @trig sysname
 		DECLARE @stmt NVARCHAR(max)
	  	DECLARE @inserter NVARCHAR(max)	  	
 		DECLARE @columns NVARCHAR(max)
 		DECLARE @maskExists BIT
		DECLARE @restriction NVARCHAR(max)
 		
 		if exists (Select * from sys.objects o inner join sys.columns c on o.object_id = c.object_id where o.name = @table and c.name = 'CompanyMask')
 			SET @maskExists = 1
 		if(@alias IS NULL OR LEN(@alias) <= 0) SET @alias = @table
 		--SET @condition = REPLACE(@condition, '''', '''''');
 		
		--disable trigger
		declare ctrig cursor for select c.name from sys.objects c join sys.objects p on c.parent_object_id = p.object_id where c.type='TR' and p.name = @table
		open ctrig
		fetch ctrig into @trig
		while @@fetch_status >= 0
		BEGIN
			set @stmt = 'DISABLE TRIGGER [' + @trig + '] ON ' + @fullname
			EXEC sp_executesql @stmt

  			fetch ctrig into @trig
		END
		close ctrig
		deallocate ctrig
		
		--disable foreign keys
		if(@clearData = 1)
		BEGIN
			declare cforeg cursor for select p.Name from sys.foreign_keys f
				inner join sys.tables p on p.object_id = f.parent_object_id
				inner join sys.tables c on c.object_id = f.referenced_object_id
				where c.name = @table
			open cforeg
			fetch cforeg into @tbl
			while @@fetch_status >= 0
			BEGIN
				set @stmt = 'alter table ' + @tbl + ' NOCHECK CONSTRAINT ALL'
				EXEC sp_executesql @stmt
			  
				fetch cforeg into @tbl
			END
			close cforeg
			deallocate cforeg
		END
	 	
 		--identyty and constraints
		SET @inserter = ''
		SET @inserter = @inserter + char(10) 
			+ N'IF EXISTS(SELECT * FROM sys.identity_columns ic WHERE ic.[object_id] = object_id(''' + @fullname + N''')) SET IDENTITY_INSERT ' + @fullname + N' ON'
  		SET @inserter = @inserter + char(10)
  			+ N'ALTER TABLE ' + @fullname + N' NOCHECK CONSTRAINT ALL'
 		
 		--delete
 		if(@clearData = 1)
 			SET @inserter = @inserter + char(10) + char(10)	+ N'DELETE FROM ' + @table + N' WHERE CompanyID = ' + CONVERT(NVARCHAR(10), @newCompanyID)
 		--insert
		SET @columns = dbo.pp_GetColumns(@table, DEFAULT, DEFAULT)
	  	SET @inserter = @inserter + char(10) + char(10)
 			+ N'INSERT INTO ' + @fullname + N' (CompanyID, ' + @columns 
 		IF(@maskExists = 1) SET @inserter = @inserter + N', CompanyMask) '
 		ELSE SET @inserter = @inserter + ') '
 		
 		SET @inserter = @inserter + char(10)
			+ N' SELECT ' + CONVERT(NVARCHAR(50), @newCompanyID) + ' as CompanyID, ' + @columns 
		if(@maskExists = 1) SET @inserter = @inserter + N', CompanyMask'
		SET @inserter = @inserter + char(10) + char(9) + N' FROM ('
 		SET @inserter = @inserter + char(10) + char(9) + char(9) 
 			+ N' SELECT [' + @alias + '].[CompanyID], ' + @columns
		if(@maskExists = 1) SET @inserter = @inserter + N', dbo.AdjustCompanyMask([' + @table + N'].CompanyMask, ' + CONVERT(NVARCHAR(10), @oldCompanyID) + N', ' + CONVERT(NVARCHAR(10), @newCompanyID) + N') AS CompanyMask'
		SET @inserter = @inserter + char(10) + char(9) + char(9)
			+ N' FROM ' + @fullname + ' ' + @alias 
		if(@alias IS NOT NULL AND LEN(@condition) > 0) SET @inserter = @inserter + N' ' + @condition
 		SET @inserter = @inserter + char(10) + char(9) 
 			+ N') vh' + SUBSTRING(CONVERT(NVARCHAR(50), RAND(100000)), 3, 6) + '  WHERE CompanyID = ' + CONVERT(NVARCHAR(50), @oldCompanyID)

		--evaluation parent companyRestriction
		if(@maskExists = 1 AND @oldCompanyID > 0)
		BEGIN
			SET @inserter = @inserter + char(10) + char(10)
			SET @parent = @oldCompanyID
			WHILE @parent is not null 
			BEGIN
				if @restriction is null
				  set @restriction = N'('
				else if len(@restriction) > 1
				  set @restriction = @restriction + ', ' + convert(varchar(10), @parent)
				else
				  set @restriction = @restriction + convert(varchar(10), @parent)
    
				if exists(select * from dbo.Company where CompanyID = @parent)
				  select @parent = ParentCompanyID from dbo.Company where CompanyID = @parent
				else select @parent = null
			END
			set @restriction = @restriction + ')'

			--updating parent records masks
			if len(@restriction) > 2
			BEGIN			
				set @inserter = @inserter + N'update ' + @fullname + ' set [CompanyMask] = '
				set @inserter = @inserter + 'dbo.binaryMaskSet(CompanyMask, ' + cast(@newCompanyID as varchar(10)) + ', 0)'
				set @inserter = @inserter + char(10) + char(9) + 'from ' + @fullname + '  '
				set @inserter = @inserter + char(10) + 'where [CompanyID] in ' + @restriction + ' and dbo.binaryMaskTest(CompanyMask, ' + cast(@oldCompanyID as varchar(10)) + ', 2) = 0'
			END
	 	END

	 	--identity and constraints	  		
		SET @inserter = @inserter + char(10) + char(10) 
			+ N'IF EXISTS(SELECT * FROM sys.identity_columns ic WHERE ic.[object_id] = object_id(''' + @fullname + N''')) SET IDENTITY_INSERT ' + @fullname + N' OFF'
		SET @inserter = @inserter + char(10) 
			+ N'ALTER TABLE ' + @fullname + N' CHECK CONSTRAINT ALL'
 			
		--PRINT @inserter
		EXEC sp_executesql @inserter
		
		--enable foreign keys
		if(@clearData = 1)
		BEGIN
			declare cforeg cursor for select p.Name from sys.foreign_keys f
				inner join sys.tables p on p.object_id = f.parent_object_id
				inner join sys.tables c on c.object_id = f.referenced_object_id
			where c.name = @table
			open cforeg
			fetch cforeg into @tbl
			while @@fetch_status >= 0
			BEGIN
				set @stmt = 'alter table ' + @tbl + ' CHECK CONSTRAINT ALL'
				EXEC sp_executesql @stmt
				
				fetch cforeg into @tbl
			END
			close cforeg
			deallocate cforeg
		END
		
		--enable trigger
		declare ctrig cursor for select c.name from sys.objects c join sys.objects p on c.parent_object_id = p.object_id where c.type='TR' and p.name = @table
		open ctrig
		fetch ctrig into @trig
		while @@fetch_status >= 0
		BEGIN
			set @stmt = 'ENABLE TRIGGER [' + @trig + '] ON ' + @fullname + ''
			EXEC sp_executesql @stmt

  			fetch ctrig into @trig
		END
		close ctrig
		deallocate ctrig
		
		--PRINT '---------------------------------------------------------'  
	END
END
GO

if exists(select * from sys.objects where object_id = object_id('dbo.pp_CopyCompany') and objectproperty(object_id,'IsProcedure')=1)
drop proc dbo.pp_CopyCompany
go
create procedure dbo.pp_CopyCompany @oldCompanyID int, @newCompanyID int, @includeCustomization bit = 0, @preserveSnapshots bit = 0 AS
BEGIN
	exec dbo.pp_DisableFullText

	declare @statement nvarchar(MAX)
	declare @table sysname
	declare @schema sysname
	declare @customizationTables NVARCHAR(1024)
	declare @counter int
	declare @companyKey nvarchar(256)
	declare @isReadonly int
	declare @parentCompanyID int

	set @customizationTables = ',CustomizationDesign,CustomizationPublished,CustProject,CustObject,CustProjectRevision,CustAzureStorage,'
	SET @statement = N'
		select @companyKey = CompanyKey, @isReadonly = IsReadOnly, @parentCompanyID = ParentCompanyID from Company 
			where CompanyID = ' + CONVERT(NVARCHAR(10),@newCompanyID) + '		
		if(@companyKey is null) 
		BEGIN
			Select @companyKey = CompanyKey  from Company where CompanyID = ' + CONVERT(NVARCHAR(10), @oldCompanyID) + '
			set @counter = 1
			WHILE (exists (select * from Company where CompanyKey = @companyKey + Convert(nvarchar(5), @counter)))
			BEGIN
				SET @counter = @counter + 1
			END
			SET @companyKey = @companyKey + Convert(nvarchar(5), @counter);
		END'	
	exec sp_executesql @statement, N'@counter int output, @companyKey nvarchar(256) output, @isReadonly int output, @parentCompanyID int output',
		@counter Output, @companyKey Output, @isReadonly Output, @parentCompanyID Output

	if(@isReadonly IS NULL) 
		exec dbo.pp_ReserveCompanyID @newCompanyID
	IF (@companyKey is not null) set @companyKey = REPLACE(@companyKey,'''','''''')

	declare crs cursor for SELECT DISTINCT t.[name], s.[name] FROM Sys.tables t
		left join sys.columns c on t.object_id = c.object_id
		left join sys.schemas s on t.schema_id = s.schema_id
		where c.[name]='CompanyID'
	open crs
	fetch crs into @table, @schema
	while @@fetch_status >= 0
	BEGIN
		if((@includeCustomization = 0 OR CHARINDEX(',' + @table + ',', @customizationTables, 0) > 0)
			AND (@preserveSnapshots = 0 OR @table not like 'UPSnapshot%')
			AND @table not like 'WatchDog')
		BEGIN
			EXEC dbo.pp_CopyCompanyTableExt @table, @schema, @oldCompanyID, @newCompanyID, 1, DEFAULT, DEFAULT
		END

  		fetch crs into @table, @schema
	END
	close crs
	deallocate crs
  
	SET @statement = 'update Company set CompanyKey = ''' + @companyKey + ''' where CompanyID = ' + CONVERT(NVARCHAR(10), @newCompanyID)
	exec sp_executesql @statement
	if(@isReadonly IS NOT NULL) 
	BEGIN
		SET @statement = 'update Company set IsReadOnly = '  + CONVERT(NVARCHAR(10), case when @isReadonly is null then 0 else  @isReadonly end) + ',	ParentCompanyID = ' + case when @parentCompanyID is null then 'NULL' else CONVERT(NVARCHAR(10), @parentCompanyID) end + '
			where CompanyID = ' + CONVERT(NVARCHAR(10), @newCompanyID)
		exec sp_executesql @statement
	END
	
	--saving audit for copying of companies
	IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[UPSnapshot]') AND type in (N'U')) and EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[UPSnapshotHistory]') AND type in (N'U'))
	BEGIN
		DECLARE @snapid uniqueidentifier
		SET @snapid = NEWID()

		SET @statement ='INSERT Into UPSnapshot (CompanyID, SnapshotID, Date, Version, Host, Name, Description, ExportMode, Customization, MasterCompany, SourceCompany, LinkedCompany, NoteID, CreatedByID, CreatedByScreenID, CreatedDateTime, LastModifiedByID, LastModifiedByScreenID, LastModifiedDateTime, DeletedDatabaseRecord)
			Select ' + CONVERT(NVARCHAR(10), @oldCompanyID) + ', ''' + CONVERT(NVARCHAR(40), @snapid) + ''', GETUTCDATE(), (Select Version from Version), ''SQL'', ''Copy Company'', null, ''FULL'', ''*'', NULL, ' + CONVERT(NVARCHAR(10), @oldCompanyID) + ', null, null, ''B5344897-037E-4D58-B5C3-1BDFD0F47BF9'', ''00000000'', GETUTCDATE(), ''B5344897-037E-4D58-B5C3-1BDFD0F47BF9'', ''00000000'', GETUTCDATE(), 1'
		exec sp_executesql @statement
		SET @statement ='INSERT Into UPSnapshotHistory (CompanyID, SnapshotID, TargetCompany, CreatedByID, CreatedByScreenID, CreatedDateTime)
			Select ' + CONVERT(NVARCHAR(10), @oldCompanyID) + ', ''' + CONVERT(NVARCHAR(40), @snapid) + ''', '+ CONVERT(NVARCHAR(10), @newCompanyID) + ', ''B5344897-037E-4D58-B5C3-1BDFD0F47BF9'', ''00000000'', GETUTCDATE()'
		exec sp_executesql @statement
	END

	exec dbo.pp_correctCompanyMask
	exec dbo.pp_enableFullText
END
GO

if exists(select * from sys.objects where object_id = object_id('dbo.pp_SeparateTable') and objectproperty(object_id,'IsProcedure')=1)
drop procedure dbo.pp_SeparateTable
go
create procedure dbo.pp_SeparateTable @tableName sysname as
BEGIN
	DECLARE @companyID int
	DECLARE @newCompanyID int
	DECLARE @stmt nvarchar(max)
	DECLARE @keys nvarchar(max)
	DECLARE @columns nvarchar(max)	
	DECLARE @cond nvarchar(max)
	DECLARE @defaultname sysname
	DECLARE @indexname sysname
	DECLARE @column sysname
	DECLARE @keyColumn sysname
	DECLARE @identityColumn sysname	

	if not exists(Select * from Sys.tables where name = @tableName) OR not exists(select * from Sys.columns where name = 'CompanyMask' and object_id = OBJECT_ID(@tableName)) return	
	Select @identityColumn = name from sys.identity_columns where object_id = OBJECT_ID(@tableName)
	if LEN(@identityColumn) > 7 AND SUBSTRING(@identityColumn, 1, 7) = 'Company'
		and Exists(Select * from Sys.columns where object_id = OBJECT_ID('dbo.[' + @tableName + ']') and name = SUBSTRING(@identityColumn, 8, LEN(@identityColumn) - 7))
		SET @keyColumn = SUBSTRING(@identityColumn, 8, LEN(@identityColumn) - 7);
	ELSE SET @keyColumn = 'Company' + @identityColumn;
	print @keyColumn

	Select @keys = dbo.pp_GetColumns(@tableName, 't', 'CompanyID')
	Select @columns = dbo.pp_GetColumns(@tableName, 't', 'CompanyID')

	DECLARE c INSENSITIVE CURSOR FOR
		with dictionary(CompanyID, ParentCompanyID, Level) as 
		(
			Select CompanyID, ParentCompanyID, 0 From dbo.Company where ParentCompanyID is null
			Union ALL
			Select t.CompanyID, t.ParentCompanyID, Level+1 From dbo.Company t 
			Join Dictionary d on (d.CompanyID = t.ParentCompanyID)
		)
		select CompanyID from dictionary
		where CompanyID > 1
		order by Level, CompanyID
	OPEN c
	FETCH NEXT FROM c INTO @companyID
	WHILE (@@fetch_status >= 0)
	BEGIN
		DECLARE @hasChanged bit
		Set @hasChanged = 0;

		DECLARE cc INSENSITIVE CURSOR FOR 
			Select CompanyID from dbo.Company where CompanyID > 1 and ParentCompanyID = @companyID
		OPEN cc
		FETCH NEXT FROM cc INTO @newCompanyID
		WHILE (@@fetch_status >= 0)
		BEGIN
			SET @stmt = ''
			SET @hasChanged = 1			

			if(@identityColumn is not null) SET @stmt = @stmt + char(10) + 'SET IDENTITY_INSERT dbo.[' + @tableName + '] ON'
			SET @stmt = @stmt + char(10) + 'insert into dbo.[' + @tableName + '] (CompanyID, CompanyMask, ' + @columns + ')'
			SET @stmt = @stmt + char(10) + 'select ' + convert(nvarchar(32), @newCompanyID) + ', t.CompanyMask, ' /*', dbo.binaryMaskAdd(CompanyMask, '+ convert(nvarchar(32),  @newCompanyID) + ', 3), '*/  + @columns + ' '
			SET @stmt = @stmt + char(10) + 'from dbo.[' + @tableName + '] t '
			
			SET @cond = ''
			declare ind cursor for(SELECT i.name from sys.indexes i
				WHERE i.object_id = Object_ID(@tableName) and i.is_unique = 1) 
			open ind
			fetch ind into @indexname
			while @@fetch_status >= 0
			BEGIN
				SET @stmt = @stmt + char(10) + 'left join dbo.[' + @tableName + '] pt' + @indexname + ' on t.CompanyID = ' + convert(nvarchar(32), @companyID) + ' and pt' + @indexname + '.CompanyID = ' + convert(nvarchar(32), @newCompanyID) + ' '
				SET @cond = @cond + 'and pt' + @indexname + '.CompanyID is null '
				declare crs cursor for(SELECT c.[name] FROM sys.[columns] c 
				join sys.indexes i on c.object_id = i.object_id
				join sys.index_columns ic on c.object_id = ic.object_id and i.index_id = ic.index_id and c.column_id = ic.column_id
				WHERE c.object_id = Object_ID(@tableName) and i.Name = @indexname and c.[name] <> 'CompanyID') 
				open crs
				fetch crs into @column
				while @@fetch_status >= 0
				BEGIN
 					SET @stmt = @stmt + 'and t.' + @column + ' = pt' + @indexname + '.' + @column + ' '

					fetch crs into @column
				END
				close crs
				deallocate crs

				fetch ind into @indexname
			END
			close ind
			deallocate ind

			SET @stmt = @stmt + char(10) +'where t.CompanyID = ' + convert(nvarchar(32), @companyID) + ' and  dbo.binaryMaskTest(t.CompanyMask, ' + convert(nvarchar(32), @newCompanyID) + ', 2) = 1 ' + @cond 
			if(@identityColumn is not null) SET @stmt = @stmt + char(10) + 'SET IDENTITY_INSERT dbo.[' + @tableName + '] OFF'

			print @stmt
			exec sp_executesql @stmt

			FETCH NEXT FROM cc INTO @newCompanyID
		END
		close cc
		deallocate cc

		if(@hasChanged = 1)
		BEGIN
			SET @stmt = 'delete from dbo.[' + @tableName + '] where CompanyID = ' + convert(nvarchar(32), @companyID) + ''
			print @stmt
			exec sp_executesql @stmt
		END

		FETCH NEXT FROM c INTO @companyID
	END
	close c
	deallocate c

	if(@keyColumn is not null and @keyColumn <> @identityColumn and exists(Select * from sys.columns c where c.object_id = OBJECT_ID(@tableName) and name = @keyColumn))
	BEGIN
		/*SET @defaultname = null
		select @defaultname = d.name from sys.default_constraints d
			join sys.columns c on d.parent_object_id = c.object_id and d.parent_column_id = c.column_id and c.name = @keyColumn
			where parent_object_id = Object_ID(@tableName)
		if(@defaultname is not null)
		BEGIN
			set @stmt = 'ALTER TABLE [dbo].[' + @tableName + '] DROP CONSTRAINT [' + @defaultname + ']'
			print @stmt
			execute sp_executesql @stmt
		END
		SET @defaultname = null
		select @defaultname = i.name from sys.indexes i
			join sys.index_columns ic on i.index_id = ic.index_id and i.object_id = ic.object_id
			join sys.columns c on i.object_id = c.object_id and ic.column_id = c.column_id and c.name = 'CashAccountID'
			where i.object_id = Object_ID('cashaccount') and i.is_unique = 1
		if(@defaultname is not null)
		BEGIN
			set @stmt = 'DROP INDEX [' + @defaultname + '] ON [dbo].[' + @tableName + '] '
			print @stmt
			execute sp_executesql @stmt
		END

		set @stmt = 'alter table dbo.[' + @tableName + '] drop column [' + @keyColumn + ']'			
		print @stmt
		execute sp_executesql @stmt*/

		set @stmt = 'exec sp_rename ''dbo.[' + @tableName + '].[' + @identityColumn + ']'', ''Obsolete' + @identityColumn + ''''
        exec sp_executesql @stmt 
	END
	if exists(Select * from sys.columns c where c.object_id = OBJECT_ID(@tableName) and name = 'CompanyMask')
	BEGIN
		SET @defaultname = null
		select @defaultname = d.name from sys.default_constraints d
			join sys.columns c on d.parent_object_id = c.object_id and d.parent_column_id = c.column_id and c.name = 'CompanyMask'
			where parent_object_id = Object_ID(@tableName)
		if(@defaultname is not null)
		BEGIN
			set @stmt = 'ALTER TABLE [dbo].[' + @tableName + '] DROP CONSTRAINT [' + @defaultname + ']'
			print @stmt
			execute sp_executesql @stmt
		END
		set @stmt = 'alter table dbo.[' + @tableName + '] drop column [CompanyMask]'			
		print @stmt
		execute sp_executesql @stmt
	END
	if exists(Select * from sys.columns c where c.object_id = OBJECT_ID(@tableName) and name = 'UsrCompanyMask')
	BEGIN
		set @stmt = 'alter table dbo.[' + @tableName + '] drop column [UsrCompanyMask]'			
		print @stmt
		execute sp_executesql @stmt
	END
END
GO

if exists(select * from sys.objects where object_id = object_id('dbo.pp_CreateTemplateCompany') and objectproperty(object_id,'IsProcedure')=1)
drop proc dbo.pp_CreateTemplateCompany
go
create procedure dbo.pp_CreateTemplateCompany @templateCompanyID int, @companyID int = 0 OUTPUT , @name nvarchar(256)  AS
BEGIN
	if(@companyID is null or @companyID <= 0)
		Select @companyID = MAX(CompanyID) + 1 from Company
	print 'New Company ID = ' + CONVERT(NVARCHAR(10), @companyID)
	
	--exec dbo.pp_DisableFullText
	exec dbo.pp_DeleteCompany @companyID
	--exec dbo.pp_ReserveCompanyID @companyID
	exec dbo.pp_CopyCompany @templateCompanyID, @companyID, 0, 0

	declare @statement nvarchar(MAX)
	declare @columns nvarchar(MAX)
	SET @statement = 'update Company set CompanyKey = ''' + @name + ''' where CompanyID = ' + CONVERT(NVARCHAR(10), @companyID)
	--print @statement
	exec sp_executesql @statement

	Select @columns = dbo.pp_GetColumns('Users', null, 'PKID,Username,Password,PasswordChangeOnNextLogin')
	SET @statement = 'insert into Users (CompanyID, PKID, Username, Password, PasswordChangeOnNextLogin, ' + @columns + ')'
	SET @statement = @statement + 'select ' + convert(nvarchar(32), @companyID) + ', NEWID(), ''' + @name + ''', ''setup'', 1, '  + @columns + ' from Users where CompanyID = 1 and Username = ''admin'''
	--print @statement
	exec sp_executesql @statement

	Select @columns = dbo.pp_GetColumns('UsersInRoles', null, 'Username')
	SET @statement = 'insert into UsersInRoles (CompanyID, Username, ' + @columns + ')'
	SET @statement = @statement + 'select ' + convert(nvarchar(32), @companyID) + ', ''' + @name + ''', '  + @columns + ' from UsersInRoles where CompanyID = 1 and Username = ''admin'''
	--print @statement
	exec sp_executesql @statement

	exec dbo.pp_correctCompanyMask
	--exec dbo.pp_enableFullText
	exec dbo.pp_ReinitialiseCompanies
END
GO

if exists(select * from sys.objects where object_id = object_id('pp_ResetCompanyMask') and objectproperty(object_id,'IsProcedure')=1)
drop proc pp_ResetCompanyMask
go
create proc dbo.pp_ResetCompanyMask @company INT, @mask NVARCHAR(MAX) AS
BEGIN
	DECLARE @stmt NVARCHAR(MAX)
	DECLARE @table NVARCHAR(255)
	
	DECLARE tables CURSOR FOR SELECT DISTINCT t.[name] 
		FROM Sys.tables t left join sys.columns c on t.object_id = c.object_id 
		where c.[name]='CompanyMask'
	OPEN tables
	FETCH NEXT FROM tables INTO @table
	WHILE @@FETCH_STATUS = 0
	BEGIN     
		SET @stmt = 'UPDATE [' + @table + '] SET CompanyMask=' + @mask
		if (@company <> 0)
			SET @stmt = @stmt +  ' where CompanyID=' + Cast(@company as nvarchar)

		--print @stmt
		EXEC sp_executeSql @stmt

		FETCH NEXT FROM tables INTO @table
	END
	CLOSE tables
	DEALLOCATE tables
END
GO

if exists(select * from sys.objects where object_id = object_id('dbo.pp_ExecuteSql') and objectproperty(object_id,'IsProcedure')=1)
drop procedure dbo.pp_ExecuteSql
go
create procedure dbo.pp_ExecuteSql
 @stmt nvarchar(max)
as
begin
declare @statement nvarchar(max)
declare @idxstart bigint
declare @idxstop bigint
declare @idxblank bigint
declare @table sysname
declare @alias sysname
declare @companyID int
declare @tempCompanyID int

declare c insensitive cursor for select CompanyID from Company
open c
fetch c into @companyID
while @@fetch_status >= 0 or @companyID is null begin
  set @companyID = coalesce(@companyID, 1)
  declare @inclause nvarchar(max)
  set @inclause=convert(nvarchar(10), @companyID)
  declare @last int
  set @last=@companyID
  while exists(select * from Company where CompanyID=@last and ParentCompanyID is not null)
    select @inclause = @inclause + ',' + convert(nvarchar(10), ParentCompanyID), @last = ParentCompanyID from Company where CompanyID = @last
  
  if(@companyID > -100000000) SET @tempCompanyID = @companyID
  else set @tempCompanyID = ABS(@companyID /10000 + 10000)
  declare @start int
  set @start = (@tempCompanyID - 1) / 4 + 1
  declare @mask int
  set @mask = power(2, 2 * ((@tempCompanyID - 1) - (@tempCompanyID - 1) / 4 * 4) + 1)
  set @statement = @stmt
  set @idxstart = charindex('{', @statement)
  while @idxstart > 0 and @idxstart < len(@statement) begin
    set @table = null
    set @idxstop = charindex('}', @statement, @idxstart + 1)
    if @idxstop > @idxstart + 2 begin
      set @idxblank = charindex(' ', @statement, @idxstart + 2)
      if @idxblank = 0 begin
        set @table = substring(@statement, @idxstart + 1, @idxstop - @idxstart - 1)
        set @alias = @table
      end else if @idxblank > @idxstart + 2 and @idxblank < @idxstop - 1 begin
        set @table = substring(@statement, @idxstart + 1, @idxblank - @idxstart - 1)
        set @alias = substring(@statement, @idxblank + 1, @idxstop - @idxblank - 1)
      end
    end
    if @table is not null begin
      if not exists(select * from sys.columns where object_id = object_id(@table) and name = 'CompanyMask')
        set @statement = substring(@statement, 1, @idxstart - 1) + @alias + '.' + 'CompanyID = ' + convert(nvarchar(10), @companyID) + substring(@statement, @idxstop + 1, len(@statement) - @idxstop)
      else
        set @statement = substring(@statement, 1, @idxstart - 1) + '(' + @alias + '.' + 'CompanyID in(' + @inclause + ') and convert(int, substring(' + @alias + '.CompanyMask, ' + convert(nvarchar(10), @start) + ', 1)) & ' + convert(nvarchar(10), @mask) + ' <> 0 or ' + @alias + '.CompanyID = ' + convert(nvarchar(10), @companyID) + ')' + substring(@statement, @idxstop + 1, len(@statement) - @idxstop)
    end
    if @idxstart > 0
      set @idxstart = charindex('{', @statement, @idxstart + 1)
  end
  exec sp_executesql @statement
  --print @statement
  if @@fetch_status >= 0
    fetch c into @companyID
end
close c
deallocate c
end
go

if exists(select * from sys.objects where object_id = object_id('dbo.pp_CopyData') and objectproperty(object_id,'IsProcedure')=1)
drop PROCEDURE dbo.pp_CopyData
go
CREATE PROCEDURE dbo.pp_CopyData @sourceTable NVARCHAR(50), @destTable NVARCHAR(50)
AS
BEGIN
  DECLARE @stmt NVARCHAR(MAX)
  DECLARE @columnsSet NVARCHAR(MAX)
  DECLARE @valuesSet NVARCHAR(MAX)
  DECLARE @temp NVARCHAR(255)
  DECLARE @column NVARCHAR(50)
  DECLARE @nulable bit
  DECLARE @type int
  DECLARE @default NVARCHAR(255)
  
  
  declare crs cursor for(SELECT c.[name], c.is_nullable, c.system_type_id, d.name FROM sys.[columns] c 
	JOIN sys.objects o ON o.[object_id] = c.[object_id]
	LEFT JOIN sys.default_constraints d on d.parent_object_id = o.object_id AND d.parent_column_id = c.column_id
    WHERE o.[name] = @destTable) 
  open crs
  fetch crs into @column, @nulable, @type, @default
  while @@fetch_status >= 0
  BEGIN
  	IF
  	(
  			@column <> 'tstamp'
			AND @column NOT IN (SELECT ic.[name] FROM sys.identity_columns ic
			JOIN sys.objects o ON o.[object_id] = ic.[object_id] WHERE o.[name] = @destTable)
  	)
  	BEGIN
  	  	if (exists (Select * FROM sys.[columns] c JOIN sys.objects o ON o.[object_id] = c.[object_id] 
  			WHERE o.[name] = @sourceTable and c.[name] = @column))
  		BEGIN
  			if(@columnsSet IS NULL) SET @columnsSet = '[' + @column + ']'
  			ELSE SET @columnsSet = @columnsSet + ', [' + @column + ']'
  		
  			if(@valuesSet IS NULL) SET @valuesSet = '[' + @column + ']'
  			ELSE SET @valuesSet = @valuesSet + ', [' + @column + ']'
  		END  		
  		ELSE
  		BEGIN 
  			if(@nulable = 0 AND @default is null) 
  			BEGIN
  				if(@columnsSet IS NULL) SET @columnsSet = '[' + @column + ']'
  				ELSE SET @columnsSet = @columnsSet + ', [' + @column + ']'
						
				SET @temp = CASE
					WHEN (@type = 56) THEN '0'
					WHEN (@type = 167) THEN ''''''
					WHEN (@type = 231) THEN ''''''
					WHEN (@type = 52) THEN '0'
					WHEN (@type = 175 ) THEN ''''''
					WHEN (@type = 104) THEN '0'
					WHEN (@type = 36) THEN '''B5344897-037E-4D58-B5C3-1BDFD0F47BF9'''
					WHEN (@type = 58) THEN '''2011-08-17 18:43:00'''
					ELSE ''
				END
				if(@valuesSet IS NULL) SET @valuesSet = @temp
  				ELSE SET @valuesSet = @valuesSet + ', ' + @temp				
			END
  		END
  	END  	
  	fetch crs into @column, @nulable, @type, @default
  END
  close crs
  deallocate crs
  
  SET @stmt = 'INSERT INTO dbo.[' + @destTable + '] (' + @columnsSet + ') SELECT ' + @valuesSet + ' FROM dbo.[' + @sourceTable + ']'
  --print @stmt
  exec sp_executeSql @stmt
END
GO

if exists(select * from sys.objects where object_id = object_id('dbo.pp_SplitWords') and type in ('FN', 'IF', 'TF', 'FS', 'FT'))
drop function dbo.pp_SplitWords
go
create function dbo.pp_SplitWords (
	@stmt nvarchar(Max)
)
returns @phases table (
		ID BIGINT not null identity(0,10),
		Position BIGINT null default ident_current('@phases'),
		Type CHAR(1) not null,
		String NVARCHAR(MAX) COLLATE SQL_Latin1_General_CP1_CI_AS not null 	
	)
begin
	if(@stmt IS NULL OR LEN(@stmt) = 0) return;

	DECLARE @letters NVARCHAR(MAX)
	DECLARE @symbols NVARCHAR(MAX)
	DECLARE @indentation NVARCHAR(2)
	DECLARE @index BIGINT	
	DECLARE @position BIGINT	
	DECLARE @brackets BIGINT
	DECLARE @comments BIGINT	
	
	SET @index = 1
	SET @brackets = 0	
	SET	@letters = ''
	SET @symbols = ''
	SET @comments = 0
	
	--parcisng text
	WHILE @index <= LEN(@stmt)
	BEGIN
		DECLARE @code int
		DECLARE @char char(1)
		SET @char = SUBSTRING(@stmt, @index, 1);		
		SET @code = ASCII(@char);
		
		--analising brackets
		if(@char = '('  AND @brackets = 0) SET @position = @index;
		if(@char = '(') SET @brackets = @brackets + 1;
		IF(@char = ')') SET @brackets = @brackets - 1;
		if(@char = ')' AND @brackets = 0)
		BEGIN
			DECLARE @phase NVARCHAR(MAX)
			SET @phase = SUBSTRING(@stmt, @position + 1, @index - @position - 1);
			
			IF(LEN(@phase) > 0) SET @indentation = char(10) + char(9)
			ELSE SET @indentation = ''
			
			INSERT INTO @phases(Type, String) VALUES('S', @indentation + '(')
			INSERT INTO @phases(Type, String) VALUES('P', @phase)
			INSERT INTO @phases(Type, String) VALUES('S', ')' + @indentation)
		END
		
		--Parcing words
		if(@char = '(' OR @char = ')')
		BEGIN
			IF (DATALENGTH(@letters) > 0) insert into @phases(Type, String) Values('L', @letters);
			IF (DATALENGTH(@symbols) > 0) insert into @phases(Type, String) Values('S', @symbols);
			SET @letters = ''
			SET @symbols = ''
		END
		ELSE if(@brackets = 0)
		BEGIN
			IF (@code = 39)
				SET @comments = @comments + 1
				
			IF (@comments % 2 = 1 OR @code < 46 OR @code > 122 OR (@code > 57 AND @code < 65) OR (@code > 93 AND @code < 95))
			BEGIN
				SET @symbols = @symbols + SUBSTRING(@stmt, @index, 1);
				IF (DATALENGTH(@letters) > 0)
				BEGIN
					insert into @phases(Type,  String) Values('L', @letters);			
				END
				SET @letters = ''	
			END
			ELSE
			BEGIN
				SET @letters = @letters + SUBSTRING(@stmt, @index, 1);
				IF (DATALENGTH(@symbols) > 0)
				BEGIN
					insert into @phases(Type, String) Values('S', @symbols);			
				END
				SET @symbols = ''	
			END
		END
	
		SET @index = @index + 1
	END
	IF (@brackets = 0)
	BEGIN
		IF (LEN(@letters) > 0) insert into @phases(Type, String) Values('L', @letters);
		IF (LEN(@symbols) > 0) insert into @phases(Type, String) Values('S', @symbols);
	END	
	UPDATE @phases SET Position = ID
	
	return
end
go


if exists(select * from sys.objects where object_id = object_id('dbo.pp_InsertSql') and objectproperty(object_id,'IsProcedure')=1)
drop PROCEDURE dbo.pp_InsertSql
go
CREATE PROCEDURE dbo.pp_InsertSql (@stmt nvarchar(max), @companyID int)
as
BEGIN
	if(@stmt IS NULL OR LEN(@stmt) = 0) return;

	DECLARE @table SYSNAME	
	DECLARE @tempTable SYSNAME	
	DECLARE @tempTableFull SYSNAME	
	DECLARE @index INT
	DECLARE @position INT
	DECLARE @statement NVARCHAR(MAX)
	DECLARE @columns NVARCHAR(MAX)
	DECLARE @priparyKey NVARCHAR(MAX)
	DECLARE @identityColumn SYSNAME
	DECLARE @identityMax INT
	DECLARE @keyColumn SYSNAME
	DECLARE @textMask NVARCHAR(66)
	DECLARE @visibleMask INT 
	DECLARE @mask VARBINARY(66)
		
	--finding insert statement and tablename
	SET @index = 1;
	SET @position = 1;
	SELECT top 1 @table = t2.String, @position = t2.ID FROM dbo.pp_SplitWords(@stmt) t1
	join dbo.pp_SplitWords(@stmt) t2 on t1.Position + 20 = t2.Position
	WHERE LOWER(t2.String) != 'into' and (LOWER(t1.String) = 'insert' OR LOWER(t1.String) = 'into')
	IF(@table IS NOT NULL AND LEN(@table) > 0)
	BEGIN
		SELECT @index = @index + (DATALENGTH (t1.String)/2)
		FROM dbo.pp_SplitWords(@stmt) t1 WHERE t1.ID < @position
		if(CHARINDEX('.', @table) > 0) SET @table = SUBSTRING(@table, CHARINDEX('.', @table), LEN(@table) - CHARINDEX('.', @table))
		SET @table = REPLACE(@table, '[', ''); SET @table = REPLACE(@table, ']', '')
	END
		
	--if table not found or 
	if not exists(Select * from Sys.columns c where name = 'CompanyMask' and object_id = OBJECT_ID('dbo.[' + @table + ']'))
		OR	not exists(Select * from Sys.identity_columns c where object_id = OBJECT_ID('dbo.[' + @table + ']'))
	BEGIN
		exec sp_executeSql @stmt
		return
	END	
	
	SET @tempTable = @table + 'Temp'	
	SET @tempTableFull = '[dbo].[' + @tempTable + ']'
	SET @priparyKey = ''
	
	--getting primary key
	declare @pkindexid smallint
	select @pkindexid = index_id from sys.indexes where is_primary_key = 1 and object_id = object_id(N'dbo.[' + @table + ']')
	declare @nextcol sysname
	declare @lastcol sysname
	if @pkindexid is not null
	begin
		declare @k int
		set @k = 1
		set @priparyKey = ''        
		while @k > 0
		begin
			set @nextcol = index_col(N'dbo.[' + @table + ']', @pkindexid, @k)
			if @nextcol is not null
			begin
				set @lastcol = @nextcol
				set @nextcol = '[' + @nextcol + ']'
				if @priparyKey = '' set @priparyKey = @nextcol
				else set @priparyKey = @priparyKey + ', ' + @nextcol
				set @k = @k + 1
			end else
				set @k = -1
		end
	end
	
	--getting identity column
	Select @identityColumn = c.name, @identityMax = CONVERT(int, i.last_value) + 1 from sys.columns c
	join sys.identity_columns i on  i.object_id = c.object_id and i.column_id = c.column_id
	Where c.object_id = OBJECT_ID('dbo.[' + @table + ']')
	if(@identityMax is null) SET @identityMax = 1
	
	--getting key column
	if LEN(@identityColumn) > 7 AND SUBSTRING(@identityColumn, 1, 7) = 'Company'
		and Exists(Select * from Sys.columns where object_id = OBJECT_ID('dbo.[' + @table + ']') and name = SUBSTRING(@identityColumn, 8, LEN(@identityColumn) - 7))
		SET @keyColumn = SUBSTRING(@identityColumn, 8, LEN(@identityColumn) - 7);
	ELSE SET @keyColumn = 'Company' + @identityColumn;
	
	--getting defauilt companyMask
	select @textMask = sdc.definition
    from sys.default_constraints sdc
    inner join sys.objects so on so.object_id = sdc.object_id
    inner join sys.columns sc on sdc.parent_column_id=sc.column_id and so.parent_object_id=sc.object_id 
    where sc.name='CompanyMask' and so.type='D' and sc.object_id=object_id('dbo.[' + @table + ']')
    SET @textMask = SUBSTRING(@textMask, 2, LEN(@textMask) - 2)
    
    SET @statement = N'Select @result = ' + @textMask
	exec sp_executesql @statement, N'@result VARBINARY(2048) output', @mask Output
    
    --getting new mask
	if (@companyID < 0) SET @visibleMask = 170
	if (@companyID % 4 = 1) SET @visibleMask = 2
    if (@companyID % 4 = 2) SET @visibleMask = 8
    if (@companyID % 4 = 3) SET @visibleMask = 32
    if (@companyID % 4 = 0) SET @visibleMask = 128  
	
	SET @statement = N'Select @result = ' + CONVERT(nvarchar(5), @visibleMask) + ' | 0x' + SUBSTRING(@textMask, ((@companyID-1)/4) * 2 + 3, 2)
	exec sp_executesql @statement, N'@result VARBINARY(1) output', @mask Output

	if(@companyID > 0)
	BEGIN
		SET @position = ((@companyID-1)/4) * 2 + 3;
		if(@position <= LEN(@textMask))
			SET @textMask = SUBSTRING(@textMask, 1, @position - 1) + dbo.pp_BinaryToHexString(@mask, 0) + SUBSTRING(@textMask, @position + 2, LEN(@textMask) - (@position + 1))
	END

	--Creating temp table
	SET @statement = 'IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N''' + @tempTableFull + ''') AND type in (N''U'')) DROP TABLE ' + @tempTableFull;
	exec sp_ExecuteSql @statement

	SET @statement = 'if not exists(SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N''' + @table + ''') AND name = ''TempTableID'' ) ALTER TABLE ' + @table + ' ADD TempTableID bigint null'
	exec sp_ExecuteSql @statement

	SET @statement = 'TempTableID|' + CONVERT(VARCHAR(50), @identityMax)
	exec dbo.pp_CreateTable @tempTable, @table, 'CompanyID, TempTableID', @statement, null

	SET @statement = 'if exists(SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N''' + @tempTable + ''') AND name = ''' + @identityColumn + ''' ) ALTER TABLE ' + @tempTableFull + ' drop column ' + @identityColumn + ''
	exec sp_ExecuteSql @statement

	--executing statemtent
	SET @statement = SUBSTRING(@stmt,1, @index - 1) + @tempTableFull + SUBSTRING(@stmt, @index + LEN(@table), LEN(@stmt) - (@index + LEN(@table)) + 1);
	--print @statement
	exec  sp_ExecuteSql @statement
	
	--copying data
	SET @columns = dbo.pp_GetColumns(@table, DEFAULT, @identityColumn + ',' + @keyColumn + ',TempTableID');
	SET @statement = 'INSERT INTO [dbo].[' + @table + '] (' + @keyColumn + ', CompanyID, CompanyMask, ' + @columns + ')';
	SET @statement = @statement + char(10) + char(9) 
		+ ' SELECT TempTableID, ' + CONVERT(NVARCHAR(50), @companyID) + ', ' + @textMask + ', ' + @columns 
		+ ' FROM ' + @tempTableFull
	--print @statement
	exec sp_ExecuteSql @statement
	
	--removing temp table
	SET @statement = 'DROP TABLE ' + @tempTableFull;
	exec sp_ExecuteSql @statement
END
GO

if exists(select * from sys.objects where object_id = object_id('dbo.pp_AnalyzeSql') and objectproperty(object_id,'IsProcedure')=1)
drop PROCEDURE dbo.pp_AnalyzeSql
go
if exists(select * from sys.objects where object_id = object_id('dbo.pp_AnalyzeSql') and objectproperty(object_id,'IsScalarFunction')=1)
drop FUNCTION dbo.pp_AnalyzeSql
go
--CREATE PROCEDURE dbo.pp_AnalyzeSql (@stmt nvarchar(max))
CREATE FUNCTION dbo.pp_AnalyzeSql (@stmt nvarchar(max))
RETURNS NVARCHAR(MAX)
AS
BEGIN
	if(@stmt IS NULL OR LEN(@stmt) = 0) return @stmt;

	DECLARE @statement NVARCHAR(MAX)
	DECLARE @result NVARCHAR(MAX)
	DECLARE @table SYSNAME
	DECLARE @alias SYSNAME
	DECLARE @index BIGINT	
	DECLARE @position BIGINT	

	DECLARE @phases table (
		ID BIGINT not null identity(0,10),
		Position BIGINT null default ident_current('@phases'),
		Type CHAR(1) not null,
		String NVARCHAR(MAX) COLLATE SQL_Latin1_General_CP1_CI_AS not null 	
	)
	INSERT INTO @phases(Position, Type, String) 
		SELECT Position, Type, String FROM dbo.pp_splitWords(@stmt);
	
	--analising subquerries
	DECLARE sub CURSOR FOR SELECT ID, String FROM @phases Where Type='P' Order by ID
	OPEN sub
	FETCH NEXT FROM sub INTO @index, @statement
	WHILE @@FETCH_STATUS = 0
	BEGIN     
		UPDATE @phases SET String = dbo.pp_AnalyzeSql(@statement)
		WHERE ID = @index

		FETCH NEXT FROM sub INTO @index, @statement
	END
	CLOSE sub
	DEALLOCATE sub
	

	--Main Analissing
	SET @index = 0;	
	while exists(SELECT * FROM @phases WHERE Type = 'L' AND Position > @index AND String = 'from')
	BEGIN	
		SELECT top 1 @position = Position from @phases WHERE Type = 'L' AND String = 'from' AND Position > @index order by Position
		SELECT top 1 @table = String FROM @phases WHERE Type = 'L' AND Position > @position order by Position
		SELECT top 2 @alias = String FROM @phases WHERE Type = 'L' AND Position > @position order by Position
		if(CHARINDEX('.', @table) > 0) SET @table = SUBSTRING(@table, CHARINDEX('.', @table) + 1, LEN(@table) - CHARINDEX('.', @table))
		SET @table = REPLACE(@table, '[', ''); SET @table = REPLACE(@table, ']', '')
		if(@alias IS NULL OR @alias = 'where' OR @alias = 'order' 
			OR @alias = 'having' OR	@alias = 'group' OR @alias = 'with')
			SET @alias = @table

		if (exists (Select * from sys.columns where object_id = OBJECT_ID(@table) and name = 'CompanyMask') and exists (Select * from sys.columns where object_id = OBJECT_ID(@table) and name = 'UsrCompanyMask')
			and exists(SELECT * FROM @phases t1 join @phases t2 ON t1.Position + 20 = t2.Position
			WHERE LOWER(t2.String) != 'into' and (LOWER(t1.String) = 'insert' OR LOWER(t1.String) = 'into')))
			return cast('This update script cant be executed because of primary table has been shared.' as int);
		
		DECLARE @whereStatement BIGINT	
		DECLARE @nextStatement BIGINT
		
		SET @whereStatement = NULL;
		SET @nextStatement = NULL;
		
		SELECT @nextStatement = MAX(Position) + 10 from @phases 
		SELECT top 1 @whereStatement = Position from @phases where String = 'where' AND Position > @position ORDER BY Position
		SELECT top 1 @nextStatement = Position - 10 from @phases where Position > @position
			AND (String = 'order' OR String = 'having' OR String = 'group' OR String = 'insert' OR String = 'update' OR String = 'delete')
			 ORDER BY Position
			 
		IF @whereStatement > @nextStatement SET @whereStatement = NULL			 
		
		SET @statement = ''
		IF @alias IS NOT NULL AND exists(Select * from sys.columns c  where c.object_id = OBJECT_ID('[dbo].[' + @table + ']') AND c.name = 'CompanyID' )
		BEGIN
			if not exists(select * from sys.columns where object_id = object_id('[dbo].[' + @table + ']') and name = 'CompanyMask')
				SET @statement = '(' + @alias + '.' + 'CompanyID = {CompanyID}' + ')'
			ELSE
				SET @statement = '(' + @alias + '.' + 'CompanyID IN ({CompanyList}) AND CONVERT(int, SUBSTRING(' + @alias + '.CompanyMask, {Position}, 1)) & {Mask} <> 0)'
				
			IF (@whereStatement IS NULL) SET @statement = char(10) + char(9) + ' WHERE ' + @statement;
			ELSE SET @statement = @statement + char(10) + char(9) + ' AND ( ';
			SELECT @position = MAX(Position) from @phases
			IF(@whereStatement IS NOT NULL) SET @position = @whereStatement + 1;
			ELSE IF(@nextStatement IS NOT NULL) SET @position = @nextStatement - 1;
			
			insert into @phases(Type, Position, String) Values('W', @position, @statement);
			IF(@whereStatement IS NOT NULL) insert into @phases(Type, Position, String) Values('W', @nextStatement - 1, ' ) ');
		END
			
		SET @index = coalesce(@whereStatement, @nextStatement);
	END	
	
	--Joins Analissing
	SET @index = 0;
	While (exists(Select * from @phases WHERE Type = 'L' AND Position > @index AND String = 'join'))
	BEGIN 
		SELECT top 1 @position = Position from @phases WHERE  Type = 'L' AND String = 'join' AND Position > @index order by Position
		SELECT top 1 @table = String FROM @phases WHERE Type = 'L' AND Position > @position order by Position
		SELECT top 2 @alias = String FROM @phases WHERE Type = 'L' AND Position > @position order by Position
		if(CHARINDEX('.', @table) > 0) SET @table = SUBSTRING(@table, CHARINDEX('.', @table) + 1, LEN(@table) - CHARINDEX('.', @table))
		SET @table = REPLACE(@table, '[', ''); SET @table = REPLACE(@table, ']', '')
		if(@alias IS NULL OR @alias = 'on') SET @alias = @table

		DECLARE @onStatement BIGINT	
		DECLARE @endStatement BIGINT
		SELECT @endStatement = MAX(Position) + 10 from @phases
		SELECT top 1 @onStatement = Position from @phases where Position > @index AND String = 'on' ORDER BY Position
		SELECT top 1 @endStatement = Position - 10 from @phases where Position > @onStatement AND (Type = 'W'
				OR String = 'join' OR String = 'where' OR String = 'order' OR String = 'having' OR String = 'group'
				OR String = 'inner' OR String = 'outer' OR String = 'left' OR String = 'right' OR String = 'cross')
			ORDER BY Position
		
			
		SET @statement = ''
		IF @alias IS NOT NULL AND exists(Select * from sys.columns c  where c.object_id = OBJECT_ID('[dbo].[' + @table + ']') AND c.name = 'CompanyID' )
		BEGIN
			if not exists(select * from sys.columns where object_id = object_id('[dbo].[' + @table + ']') and name = 'CompanyMask')
				SET @statement = ' (' + @alias + '.' + 'CompanyID = {CompanyID}' + ')'
			ELSE
				SET @statement = ' (' + @alias + '.' + 'CompanyID IN ({CompanyList}) AND CONVERT(int, SUBSTRING(' + @alias + '.CompanyMask, {Position}, 1)) & {Mask} <> 0' + ')'
			
			SET @statement = @statement + char(10) + char(9) + ' AND (';
			INSERT INTO @phases(Type, Position, String) Values('J', @onStatement + 1, @statement);
			INSERT INTO @phases(Type, Position, String) Values('J', @endStatement + 1, ' ) '); 
		END
		
		SET @index = @onStatement;
	END		
	
	--Generating SQL
	SET @result = ''
	DECLARE querry CURSOR FOR SELECT String FROM @phases Order by Position
	OPEN querry
	FETCH NEXT FROM querry INTO @statement
	WHILE @@FETCH_STATUS = 0
	BEGIN     
		SET @result = @result + @statement

		FETCH NEXT FROM querry INTO @statement
	END
	CLOSE querry
	DEALLOCATE querry
	
	--print @result
	return @result
END
GO

if exists(select * from sys.objects where object_id = object_id('dbo.pp_SmartExecuteSql') and objectproperty(object_id,'IsProcedure')=1)
drop PROCEDURE dbo.pp_SmartExecuteSql
go
CREATE PROCEDURE dbo.pp_SmartExecuteSql (@stmt nvarchar(max))
as
BEGIN
	if(@stmt IS NULL OR LEN(@stmt) = 0) return;

	DECLARE @statement NVARCHAR(MAX)
	DECLARE @companyID int
	DECLARE @tempCompanyID int
	DECLARE @inclause nvarchar(max)
	DECLARE @mask nvarchar(max)
	DECLARE @position nvarchar(max)
	DECLARE @singleRun bit
	DECLARE @insertRun bit
	
	SET @stmt = dbo.pp_AnalyzeSql(@stmt)
	--if table don't have a companyID
	SET @singleRun = 0;
	if((CHARINDEX('{CompanyID}', @stmt COLLATE Latin1_General_CI_AS) <= 0) AND
		(CHARINDEX('{CompanyList}', @stmt COLLATE Latin1_General_CI_AS) <= 0))
			SET @singleRun = 1;
	
	--finding insert statement	
	SET @insertRun = 0;
	if exists(SELECT * FROM dbo.pp_SplitWords(@stmt) t1
	join dbo.pp_SplitWords(@stmt) t2 on t1.Position + 20 = t2.Position
	WHERE LOWER(t2.String) != 'into' and (LOWER(t1.String) = 'insert' OR LOWER(t1.String) = 'into'))
		SET @insertRun = 1;	

	DECLARE c INSENSITIVE CURSOR FOR
		with dictionary(CompanyID, ParentCompanyID, Level) as 
		(
			Select CompanyID, ParentCompanyID, 0 From dbo.Company where ParentCompanyID is null
			Union ALL
			Select t.CompanyID, t.ParentCompanyID, Level+1 From dbo.Company t 
			Join Dictionary d on (d.CompanyID = t.ParentCompanyID)
		)
		select CompanyID from dictionary
		order by Level
	OPEN c
	FETCH NEXT FROM c INTO @companyID
	WHILE (@@fetch_status >= 0 or @companyID is null)
	BEGIN
		SET @statement = @stmt
		SET @companyID = coalesce(@companyID, 1)		
		SET @inclause=convert(nvarchar(10), @companyID)			
		
		--finding visible companies
		DECLARE @last int
		SET @last=@companyID
		WHILE exists(SELECT * FROM Company WHERE CompanyID=@last and ParentCompanyID is not null)
			SELECT @inclause = @inclause + ',' + convert(NVARCHAR(10), ParentCompanyID), @last = ParentCompanyID FROM Company WHERE CompanyID = @last
		
		--Analising CompanyMask
		if(@companyID > -100000000) SET @tempCompanyID = @companyID
		else set @tempCompanyID = ABS(@companyID /10000 + 10000)
		SET @mask = convert(NVARCHAR(10), power(2, 2 * ((@tempCompanyID - 1) - (@tempCompanyID - 1) / 4 * 4) + 1))
		SET @position = convert(NVARCHAR(10), (@tempCompanyID - 1) / 4 + 1)
		
		--Replacing		
		SET @statement = REPLACE(@statement, '{CompanyID}', @companyID);
		SET @statement = REPLACE(@statement, '{CompanyList}', @inclause);
		SET @statement = REPLACE(@statement, '{Position}', @position);
		SET @statement = REPLACE(@statement, '{Mask}', @mask);
				
		--print @statement	
		IF(@insertRun = 0) EXEC sp_executeSql @statement		
		ELSE EXEC dbo.pp_InsertSql @statement, @companyID
		
		IF(@singleRun = 1) BREAK;
		FETCH NEXT FROM c INTO @companyID
	END
	CLOSE c
	DEALLOCATE c
END
GO

if exists(select * from sys.objects where object_id = object_id('pp_RegisterPortal') and objectproperty(object_id,'IsProcedure')=1)
drop proc pp_RegisterPortal
go
create proc dbo.pp_RegisterPortal @company INT as
BEGIN
	if exists(Select * from Sys.tables where name = 'Sitemap')
	BEGIN
		Declare @stmt nvarchar(max)
		set @stmt = 'if not exists(Select * from dbo.Sitemap where [CompanyID] = ' + Cast(@company as nvarchar) + ' and [NodeID] = ''D0B3F1D6-F9F8-46BB-9A84-69EA4DB907BA'')
			insert into dbo.Sitemap ([CompanyID], [Position], [Title], [Description], [Url], [Expanded], [IsFolder], [ScreenID], [NodeID], [ParentID], [CreatedByID], [CreatedByScreenID], [CreatedDateTime], [LastModifiedByID], [LastModifiedByScreenID], [LastModifiedDateTime])
			Select ' + Cast(@company as nvarchar) + ', 941, ''Portal Map'', NULL, ''~/Pages/SM/SM200521.aspx'', 0, 0, ''SM200521'', ''D0B3F1D6-F9F8-46BB-9A84-69EA4DB907BA'', ''DEEB974A-9865-4DFE-A3A8-594CF97A24D1'', ''B5344897-037E-4D58-B5C3-1BDFD0F47BF9'', ''SM200520'', ''2013-02-05 16:42:22.173'', ''B5344897-037E-4D58-B5C3-1BDFD0F47BF9'', ''SM200520'', ''2013-02-05 16:42:22.173'''
		execute sp_executesql @stmt
	END
END
GO

if exists(select * from sys.objects where object_id = object_id('pp_RegisterAdminUser') and objectproperty(object_id,'IsProcedure')=1)
drop proc pp_RegisterAdminUser
go
create proc dbo.pp_RegisterAdminUser @companyID INT, @username NVARCHAR(512), @password NVARCHAR(512), @mustChangePass bit  as
BEGIN
	if exists(Select * from Sys.tables where name = 'Users') and exists(Select * from Sys.tables where name = 'Company')
	BEGIN
		Declare @stmt nvarchar(max)
		Declare @columns nvarchar(max)
		declare @parent int
		
		--correcting masks
		set @parent = @companyID
		while @parent is not null begin
			SET @stmt = 'update Users set CompanyMask = dbo.binaryMaskSub(CompanyMask, ' + convert(nvarchar(32),  @companyID) + ', 3) where username = ''admin'' and CompanyID = ' + convert(nvarchar(32),  @parent)
			exec sp_executesql @stmt
			SET @stmt = 'update UsersInRoles set CompanyMask = dbo.binaryMaskSub(CompanyMask, ' + convert(nvarchar(32),  @companyID) + ', 3) where username = ''admin'' and CompanyID = ' + convert(nvarchar(32),  @parent)
			exec sp_executesql @stmt

			if exists(select * from dbo.Company where CompanyID = @parent)
				select @parent = ParentCompanyID from dbo.Company where CompanyID = @parent
			else break
		  end
		
		SET @stmt = 'delete from Users where CompanyID = ' + convert(nvarchar(32), @companyID) + ' and username = ''admin'''
		exec sp_executesql @stmt
		SET @stmt = 'delete from UsersInRoles where CompanyID = ' + convert(nvarchar(32), @companyID) + ' and username = ''admin'''
		exec sp_executesql @stmt

		Select @columns = dbo.pp_GetColumns('Users', null, 'PKID,Username,Password,PasswordChangeOnNextLogin')
		SET @stmt = 'insert into Users (CompanyID, PKID, Username, Password, PasswordChangeOnNextLogin, ' + @columns + ')'
		SET @stmt = @stmt + 'select ' + convert(nvarchar(32), @companyID) + ', NEWID(), ''' + @username + ''', ''' + @password + ''', '+ convert(nvarchar(32), @mustChangePass) +', '  + @columns + ' from Users where CompanyID = 1 and Username = ''admin'''
		--print @@stmt
		exec sp_executesql @stmt
		SET @stmt = 'update Users set CompanyMask = dbo.binaryMaskAdd(CompanyMask, '+ convert(nvarchar(32),  @companyID) + ', 3) where username = ''' + @username + ''' and CompanyID = ' + convert(nvarchar(32),  @companyID)
		exec sp_executesql @stmt

		Select @columns = dbo.pp_GetColumns('UsersInRoles', null, 'Username')
		SET @stmt = 'insert into UsersInRoles (CompanyID, Username, ' + @columns + ')'
		SET @stmt = @stmt + 'select ' + convert(nvarchar(32), @companyID) + ', ''' + @username + ''', '  + @columns + ' from UsersInRoles where CompanyID = 1 and Username = ''admin'''
		--print @@stmt
		exec sp_executesql @stmt
		SET @stmt = 'update UsersInRoles set CompanyMask = dbo.binaryMaskAdd(CompanyMask, '+ convert(nvarchar(32),  @companyID) + ', 3) where username = ''' + @username + ''' and CompanyID = ' + convert(nvarchar(32),  @companyID)
		exec sp_executesql @stmt

		execute sp_executesql @stmt
	END
END
GO

if exists(select * from sys.objects where object_id = object_id('dbo.GetDateByWeek'))
   DROP FUNCTION dbo.GetDateByWeek
go 

CREATE FUNCTION dbo.GetDateByWeek(@year int, @month int, @week int, @weekday int, @time time)  
--@weekday ignore DATEFIRST, 1 - monday, 7 - sunday
RETURNS DATETIME   
AS    
BEGIN  
    
	DECLARE @ret AS DATETIME;
	IF (@week < 5)
		BEGIN
			DECLARE @prFirstDay AS DATETIME =  DATEADD(MONTH, @month - 2, DATEADD(YEAR, @year-1900, 0));	
			declare @prLast as datetime = DATEADD(MONTH, DATEDIFF(MONTH,0, @prFirstDay),30);
			SET @ret = DATEADD(DAY, DATEDIFF(DAY, @weekday - 1, @prLast)/7*7 + 7 * @week , @weekday - 1 );
		END
	ELSE
		BEGIN
			DECLARE @firstDay AS DATETIME =  DATEADD(MONTH, @month - 1, DATEADD(YEAR, @year-1900, 0));	
			declare @last as datetime = DATEADD(MONTH, DATEDIFF(MONTH,0, @firstDay),30);
			SET @ret = DATEADD(DAY, DATEDIFF(DAY, @weekday - 1, @last)/7*7 , @weekday - 1 );
		END
		
    RETURN @ret + cast(@time as datetime); 
END;  
GO 

if exists(select * from sys.objects where object_id = object_id('sp_CalculateTablesSize') and objectproperty(object_id,'IsProcedure')=1)
drop proc sp_CalculateTablesSize
go
create proc dbo.sp_CalculateTablesSize as
begin
	insert TableSize
	select s.*, c.CompanyId from
	(
	select top 100 max(t.name) as name, 0 as Size, 0 as IndexSize, SUM(p1.Rows) as calcedcount, SUM(a1.total_pages) * 8192 AS PkSpaceKB
	FROM        sys.tables t
	LEFT JOIN   sys.schemas s ON t.schema_id = s.schema_id
	INNER JOIN  sys.indexes i ON t.OBJECT_ID = i.object_id
	LEFT JOIN   sys.partitions p1 ON i.object_id = p1.OBJECT_ID AND i.index_id = p1.index_id and i.type <= 1 left JOIN   sys.allocation_units a1 ON p1.partition_id = a1.container_id
	LEFT JOIN   sys.partitions p2 ON i.object_id = p2.OBJECT_ID AND i.index_id = p2.index_id and i.type > 1 left JOIN   sys.allocation_units a2 ON p2.partition_id = a2.container_id
	JOIN        sys.columns col  ON col.object_id = t.object_id
	WHERE       t.is_ms_shipped = 0 AND i.OBJECT_ID > 255 and col.name = 'CompanyId'
	GROUP BY t.object_id
	order by SUM(a1.total_pages) desc) s
	inner JOIN Company c
	on c.CompanyId <> 1


	declare @oid int
	declare @stmt nvarchar(max)
	declare csr insensitive cursor for select distinct object_id(TableName) from TableSize
	open csr
	fetch csr into @oid
	while @@FETCH_STATUS = 0 begin

		-- nullbitmap
		declare @nullbitmap int
		select @nullbitmap = 2 + ((count(*) + 7) /8)
		from sys.columns
		where object_id=@oid

		-- column size
		declare @size bigint
		select @size = sum(
			case
				when system_type_id = 231 or system_type_id = 167 or system_type_id = 165
					then (
						case
							when max_length >= 0 and max_length <= 30
								then max_length / 2
							when max_length = -1
								then 0
							when max_length > 512
								then 0
							else max_length / 10
						end)
				else max_length
			end + is_nullable) + @nullbitmap + 4
		from sys.columns
		where object_id=@oid and system_type_id<>104

		-- add clustered index size
		declare @NumKeyCols int
		SELECT @NumKeyCols = count(*)
		FROM sys.indexes AS i
		INNER JOIN sys.index_columns AS ic
		ON i.OBJECT_ID = ic.OBJECT_ID
		AND i.index_id = ic.index_id
		right join sys.columns as c
		on ic.column_id = c.column_id
		and ic.OBJECT_ID = c.object_id
		WHERE i.type = 1 and ic.OBJECT_ID = @oid
		print 'NumKeyCols '
		print @NumKeyCols
		-- variable key size
		declare @VariableKeySize bigint
		select @VariableKeySize = coalesce(2 + count(*) * 2 + sum(
			case
				when max_length >= 0 and max_length <= 30
					then max_length / 2
				when max_length = -1
					then 0
				when max_length > 512
					then 0
				else max_length / 10
			end), 0)
		from sys.columns c
		left join sys.index_columns ic
		on ic.column_id = c.column_id
		and ic.object_id = c.object_id
		left join sys.indexes i
		ON i.object_id = c.object_id
		AND i.index_id = ic.index_id
		where c.object_id = @oid
		and c.system_type_id<>104
		and (c.system_type_id = 231 or c.system_type_id = 167 or c.system_type_id = 165)
		and i.type = 1

		-- fixed key size
		declare @FixedKeySize bigint
		select @FixedKeySize = sum(c.max_length)
		from sys.columns c
		left join sys.index_columns ic
		on ic.column_id = c.column_id
		and ic.object_id = c.object_id
		left join sys.indexes i
		ON i.object_id = c.object_id
		AND i.index_id = ic.index_id
		where c.object_id = @oid
		and i.type = 1
		and (system_type_id <> 231 and system_type_id <> 167 and system_type_id <> 165)
		--index null bitmap
		declare @CountNullableKeys int
		select @CountNullableKeys = coalesce(count(*), 0)
		FROM sys.indexes AS i
		INNER JOIN sys.index_columns AS ic
		ON i.object_id = ic.object_id
		AND i.index_id = ic.index_id
		right join sys.columns as c
		on ic.column_id = c.column_id
		and ic.object_id = c.object_id
		WHERE i.type = 1 and ic.object_id = @oid
		and c.is_nullable = 1
		declare @IndexNullBitmap int
		set @IndexNullBitmap = iif (@CountNullableKeys = 0, 0, 2 + ((@NumKeyCols + 7) / 8))

		declare @IndexRowSize bigint
		set @IndexRowSize = @FixedKeySize + @VariableKeySize + @IndexNullBitmap + 7

		--set @size = @size + @IndexRowSize

		-- add nonclustered index size
		declare @NonNumKeyCols int
		SELECT @NonNumKeyCols = count(*)
		FROM sys.indexes AS i
		INNER JOIN sys.index_columns AS ic
		ON i.OBJECT_ID = ic.OBJECT_ID
		AND i.index_id = ic.index_id
		right join sys.columns as c
		on ic.column_id = c.column_id
		and ic.OBJECT_ID = c.object_id
		WHERE i.type = 2 and ic.OBJECT_ID = @oid
		print 'NonNumKeyCols '
		print @NonNumKeyCols
		-- variable key size
		declare @NonVariableKeySize bigint
		select @NonVariableKeySize = coalesce(2 + count(*) * 2 + sum(
			case
				when max_length >= 0 and max_length <= 30
					then max_length / 2
				when max_length = -1
					then 0
				when max_length > 512
					then 0
				else max_length / 10
			end), 0)
		from sys.columns c
		left join sys.index_columns ic
		on ic.column_id = c.column_id
		and ic.object_id = c.object_id
		left join sys.indexes i
		ON i.object_id = c.object_id
		AND i.index_id = ic.index_id
		where c.object_id = @oid
		and c.system_type_id<>104
		and (c.system_type_id = 231 or c.system_type_id = 167 or c.system_type_id = 165)
		and i.type = 2

		-- fixed key size
		declare @NonFixedKeySize bigint
		select @NonFixedKeySize = coalesce(sum(c.max_length), 0)
		from sys.columns c
		left join sys.index_columns ic
		on ic.column_id = c.column_id
		and ic.object_id = c.object_id
		left join sys.indexes i
		ON i.object_id = c.object_id
		AND i.index_id = ic.index_id
		where c.object_id = @oid
		and i.type = 2
		and (system_type_id <> 231 and system_type_id <> 167 and system_type_id <> 165)
		--index null bitmap
		declare @NonCountNullableKeys int
		select @NonCountNullableKeys = coalesce(count(*), 0)
		FROM sys.indexes AS i
		INNER JOIN sys.index_columns AS ic
		ON i.object_id = ic.object_id
		AND i.index_id = ic.index_id
		right join sys.columns as c
		on ic.column_id = c.column_id
		and ic.object_id = c.object_id
		WHERE i.type = 2 and ic.object_id = @oid
		and c.is_nullable = 1
		declare @NonIndexNullBitmap int
		set @NonIndexNullBitmap = iif (@NonCountNullableKeys = 0, 0, 2 + ((@NonNumKeyCols + 7) / 8))

		declare @NonIndexRowSize bigint
		set @NonIndexRowSize = @NonFixedKeySize + @NonVariableKeySize + @NonIndexNullBitmap + 7

		--set @size = @size + @NonIndexRowSize

		if exists (
			select *
			from sys.columns
			where object_id = @oid
				and (system_type_id = 231 or system_type_id = 167 or system_type_id = 165)
				and (max_length = -1 or max_length > 512))
				
			begin
			--set @stmt = N'update TableSize set SizeByCompany = SizeByCompany + coalesce((select sum(convert(bigint, datalength(' + (select top 1 name from sys.columns where object_id = @oid and max_length = -1) + '))) from ' + object_name(@oid) + '), 0) where  TableName = ''' + object_name(@oid) + ''''
			declare @cols varchar(max)
			set @cols=''
			select @cols = @cols + ']),0) as bigint)+cast(coalesce(datalength(t.[' + name
				from sys.columns
				where object_id = @oid
				and (system_type_id = 231 or system_type_id = 167 or system_type_id = 165)
				and (max_length = -1 or max_length > 512)
			set @cols = 'sum(' + substring(@cols, 18, len(@cols) - 17) + '] ),0) as bigint))'

			declare @clusteredIndexCols varchar(max)
			set @clusteredIndexCols=''
			select @clusteredIndexCols = @clusteredIndexCols + ']),0) as bigint)+cast(coalesce(datalength(t.[' + c.name
				from sys.columns c
				left join sys.index_columns ic
				on ic.column_id = c.column_id
				and ic.object_id = c.object_id
				left join sys.indexes i
				ON i.object_id = c.object_id
				AND i.index_id = ic.index_id
				where c.object_id = @oid
				and c.system_type_id<>104
				and i.type = 1
				and (c.system_type_id = 231 or c.system_type_id = 167 or c.system_type_id = 165)
				and (c.max_length = -1 or c.max_length > 512)
			set @clusteredIndexCols = iif(@clusteredIndexCols = '', 'cast(0 as bigint)', 'sum(' + substring(@clusteredIndexCols, 18, len(@clusteredIndexCols) - 17) + '] ),0) as bigint))')

			declare @nonClusteredIndexCols varchar(max)
			set @nonClusteredIndexCols=''
			select @nonClusteredIndexCols = @nonClusteredIndexCols + ']),0) as bigint)+cast(coalesce(datalength(t.[' + c.name
				from sys.columns c
				left join sys.index_columns ic
				on ic.column_id = c.column_id
				and ic.object_id = c.object_id
				left join sys.indexes i
				ON i.object_id = c.object_id
				AND i.index_id = ic.index_id
				where c.object_id = @oid
				and c.system_type_id<>104
				and i.type = 2
				and (c.system_type_id = 231 or c.system_type_id = 167 or c.system_type_id = 165)
				and (c.max_length = -1 or c.max_length > 512)
			set @nonClusteredIndexCols = iif(@nonClusteredIndexCols = '', 'cast(0 as bigint)', 'sum(' + substring(@nonClusteredIndexCols, 18, len(@nonClusteredIndexCols) - 17) + '] ),0) as bigint))')

			set @stmt =
				N'update TableSize
				set CountOfCompanyRecords = s.CountOfCompanyRecords, SizeByCompany = iif (s.CountOfCompanyRecords = 0, 0, s.CountOfCompanyRecords * ' + convert(varchar(10), @size) + ' + s.delta), IndexSizeByCompany = iif (s.CountOfCompanyRecords = 0, 0, s.CountOfCompanyRecords * ' + convert(varchar(10), @NonIndexRowSize + @IndexRowSize) + ' + s.clusteredIndexDelta + s.nonClusteredIndexDelta)
				from TableSize
				inner join
				(select ' + @cols + ' as delta,
					' + @clusteredIndexCols + ' as clusteredIndexDelta,
					' + @nonClusteredIndexCols + ' as nonClusteredIndexDelta,
					c.CompanyId,
					iif (cast(count(*) as bigint) = 1 and (select sum(1) from ' + object_name(@oid) + ' where CompanyId = c.CompanyId) is null, 0, cast(count(*) as bigint)) as CountOfCompanyRecords
					from ' + object_name(@oid) + ' t
					right join Company c on t.CompanyId = c.CompanyId
					where c.CompanyId <> 1
					group by c.CompanyId) s
				on TableSize.Company=s.CompanyId and TableSize.TableName = ''' + object_name(@oid) + ''''
				
		end
		else begin
			set @stmt =
				N'update TableSize
				set CountOfCompanyRecords = s.CountOfCompanyRecords, SizeByCompany = s.CountOfCompanyRecords * ' + convert(varchar(10), @size) + ', IndexSizeByCompany = iif (s.CountOfCompanyRecords = 0, 0, s.CountOfCompanyRecords * ' + convert(varchar(10), @NonIndexRowSize + @IndexRowSize) + ')
				from TableSize
				inner join
				(select c.CompanyId,
					iif (cast(count(*) as bigint) = 1 and (select sum(1) from ' + object_name(@oid) + ' where CompanyId = c.CompanyId) is null, 0, cast(count(*) as bigint)) as CountOfCompanyRecords
					from ' + object_name(@oid) + ' t
					right join Company c on t.CompanyId = c.CompanyId
					where c.CompanyId <> 1
					group by c.CompanyId) s
				on TableSize.Company=s.CompanyId and TableSize.TableName = ''' + object_name(@oid) + ''''
		end
		print @stmt
		exec sp_executesql @stmt

		fetch csr into @oid
	end
	close csr
	deallocate csr
end
go

IF EXISTS(SELECT * FROM sys.objects WHERE object_id = object_id('dbo.binaryMaskOr') AND objectproperty(object_id,'IsScalarFunction') = 1)
  DROP FUNCTION dbo.binaryMaskOr
GO

CREATE FUNCTION dbo.binaryMaskOr(@maskA VARBINARY(128), @maskB VARBINARY(128))
RETURNS VARBINARY(128)
AS
BEGIN
  DECLARE @lengthOfA AS TINYINT = DATALENGTH(@maskA);
  DECLARE @lengthOfB AS TINYINT = DATALENGTH(@maskB);
  DECLARE @stopIndex AS TINYINT;
  DECLARE @result AS VARBINARY(128);
  DECLARE @other AS VARBINARY(128);
  DECLARE @resultByte AS BINARY(1);
  DECLARE @otherByte AS BINARY(1);

  IF @lengthOfA > @lengthOfB
  BEGIN
    SET @stopIndex = @lengthOfB;
    SET @result = @maskA;
    SET @other = @maskB;
  END
  ELSE
  BEGIN
    SET @stopIndex = @lengthOfA;
    SET @result = @maskB;
    SET @other = @maskA;
  END;

  DECLARE @byteIndex AS TINYINT = 1;

  WHILE @byteIndex <= @stopIndex
  BEGIN
    SET @resultByte = SUBSTRING(@result, @byteIndex, 1);
    SET @otherByte = SUBSTRING(@other, @byteIndex, 1);
    SET @resultByte |= CAST(@otherByte AS TINYINT);
    SET @result = CAST(STUFF(@result, @byteIndex, 1, @resultByte) AS VARBINARY(128));
    SET @byteIndex += 1;
  END

  RETURN @result;
END
GO

if exists(select * from sys.objects where object_id = object_id('dbo.pp_UpdateConfigurationOfCustomGIs') and objectproperty(object_id,'IsProcedure')=1)
drop proc dbo.pp_UpdateConfigurationOfCustomGIs
go
CREATE PROC dbo.pp_UpdateConfigurationOfCustomGIs @oldTableName VARCHAR(255), @oldFieldName VARCHAR(255), @newTableName VARCHAR(255), @newFieldName VARCHAR(255)
AS
BEGIN
	
	IF (NOT @oldTableName IS NULL) AND (NOT @newTableName IS NULL) AND (@oldTableName <> @newTableName)
	BEGIN
		UPDATE GITable
		SET Name = @newTableName
		WHERE Name = @oldTableName
			AND CompanyID > 1
	END

	IF (NOT @oldFieldName IS NULL) AND (NOT @newFieldName IS NULL) AND (@oldFieldName <> @newFieldName) AND (NOT @newTableName IS NULL)
	BEGIN
		UPDATE r SET Field = LOWER(LEFT(@newFieldName, 1)) + SUBSTRING(@newFieldName, 2, LEN(@newFieldName))
		FROM GIResult r
		INNER JOIN GITable t
			ON t.DesignID = r.DesignID
			AND t.CompanyID = r.CompanyID
			AND r.ObjectName = t.Alias
		WHERE r.CompanyID > 1
			AND r.Field = @oldFieldName
			AND t.Name = @newTableName

		UPDATE r SET Field = REPLACE(Field, t.Alias + '.' + @oldFieldName, t.Alias + '.' + @newFieldName)
		FROM GIResult r
		INNER JOIN GITable t
			ON r.ObjectName = t.Alias
			AND r.CompanyID = t.CompanyID
			AND r.DesignID = t.DesignID
		WHERE r.CompanyID > 1
			AND CHARINDEX( LOWER(@newTableName), LOWER(t.[Name]) ) > 0
			AND CHARINDEX( LOWER(t.Alias) + '.' + LOWER(@newTableName), LOWER(r.Field) ) > 0

		UPDATE fr SET DataField = REPLACE(fr.DataField, @oldFieldName, LOWER(LEFT(@newFieldName, 1)) + SUBSTRING(@newFieldName, 2, LEN(@newFieldName)))
		FROM FilterRow fr
		WHERE fr.CompanyID > 1
			AND CHARINDEX(LOWER(fr.[DataField]), LOWER(@newTableName) + '_' + @oldFieldName) > 0

		UPDATE np SET FieldName = @newFieldName, ParameterName = t.Alias + '.' + @newFieldName
		FROM GINavigationParameter np
		INNER JOIN GITable t
			ON t.DesignID = np.DesignID
			AND t.CompanyID = np.CompanyID
			AND CHARINDEX(LOWER(t.Alias), LOWER(np.ParameterName)) > 0
		WHERE np.CompanyID > 1
			AND LOWER(np.ParameterName) = LOWER(t.Alias + '.' + @oldFieldName)
			AND t.Name = @newTableName

		UPDATE s SET DataFieldName = t.Alias + '.' + LOWER(LEFT(@newFieldName, 1)) + SUBSTRING(@newFieldName, 2, LEN(@newFieldName))
		FROM GISort s
		INNER JOIN GITable t
			ON t.DesignID = s.DesignID
			AND t.CompanyID = s.CompanyID
			AND CHARINDEX(LOWER(t.Alias), LOWER(s.DataFieldName)) > 0
		WHERE s.CompanyID > 1
			AND LOWER(s.DataFieldName) = LOWER(t.Alias + '.' + @oldFieldName)
			AND t.Name = @newTableName

		UPDATE g SET DataFieldName = t.Alias + '.' + @newFieldName
		FROM GIGroupBy g
		INNER JOIN GITable t
			ON t.DesignID = g.DesignID
			AND t.CompanyID = g.CompanyID
			AND CHARINDEX(LOWER(t.Alias), LOWER(g.DataFieldName)) > 0
		WHERE g.CompanyID > 1
			AND LOWER(g.DataFieldName) = LOWER(t.Alias + '.' + @oldFieldName) AND t.Name = @newTableName

		UPDATE f
		SET FieldName = t.Alias + '.' + LOWER(LEFT(@newFieldName, 1)) + SUBSTRING(@newFieldName, 2, LEN(@newFieldName)),
			[Name] = @newFieldName
		FROM GIFilter f
		INNER JOIN GITable t
			ON t.DesignID = f.DesignID
			AND t.CompanyID = f.CompanyID
			AND CHARINDEX(LOWER(t.Alias), LOWER(f.FieldName)) > 0
		WHERE f.CompanyID > 1
			AND LOWER(f.FieldName) = LOWER(t.Alias + '.' + @oldFieldName)
			AND @newTableName = t.Name
		
		UPDATE o SET ParentField = LOWER(LEFT(@newFieldName, 1)) + SUBSTRING(@newFieldName, 2, LEN(@newFieldName))
		FROM GIOn o
		INNER JOIN GIRelation r
			ON r.DesignID = o.DesignID
			AND r.CompanyID = o.CompanyID
			AND r.LineNbr = o.RelationNbr
		INNER JOIN GITable pt
			ON pt.DesignID = r.DesignID
			AND pt.CompanyID = r.CompanyID
			AND pt.Alias = r.ParentTable
		WHERE o.CompanyID > 1
			AND LOWER(@oldFieldName) = LOWER(o.ParentField)
			AND @newTableName = pt.Name

		UPDATE o SET ChildField = LOWER(LEFT(@newFieldName, 1)) + SUBSTRING(@newFieldName, 2, LEN(@newFieldName))
		FROM GIOn o
		INNER JOIN GIRelation r
			ON r.DesignID = o.DesignID
			AND r.CompanyID = o.CompanyID
			AND r.LineNbr = o.RelationNbr
		INNER JOIN GITable ct
			ON ct.DesignID = r.DesignID
			AND ct.CompanyID = r.CompanyID
			AND ct.Alias = r.ChildTable
		WHERE o.CompanyID > 1
			AND LOWER(@oldFieldName) = LOWER(o.ChildField)
			AND @newTableName = ct.Name

		UPDATE w SET DataFieldName = t.Alias + '.' + LOWER(LEFT(@newFieldName, 1)) + SUBSTRING(@newFieldName, 2, LEN(@newFieldName))
		FROM GIWhere w
		INNER JOIN GITable t
			ON t.DesignID = w.DesignID
			AND t.CompanyID = w.CompanyID
			AND CHARINDEX(LOWER(t.Alias), LOWER(w.DataFieldName)) > 0
		WHERE w.CompanyID > 1
			AND LOWER(w.DataFieldName) = LOWER(t.Alias + '.' + @oldFieldName)
			AND @newTableName = t.Name

	END
	
END
GO

IF EXISTS(SELECT * FROM sys.objects WHERE object_id = object_id('dbo.pp_CopyCustomerRolesInGraphForScreenIfNotSetYet') AND objectproperty(object_id,'IsProcedure') = 1)
  DROP PROCEDURE dbo.pp_CopyCustomerRolesInGraphForScreenIfNotSetYet
GO

CREATE PROCEDURE dbo.pp_CopyCustomerRolesInGraphForScreenIfNotSetYet @sourceScreenID varchar(8), @newScreenID varchar(8)
AS
BEGIN
	IF NOT EXISTS(Select * FROM RolesInGraph WHERE CompanyID > 1 AND ScreenID = @newScreenID)
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
			@newScreenID,
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
		WHERE CompanyID > 1 AND ScreenID = @sourceScreenID
END
GO

IF EXISTS(SELECT * FROM sys.objects WHERE object_id = object_id('dbo.pp_UpdateRemovedColumn') AND objectproperty(object_id,'IsProcedure') = 1)
  DROP PROCEDURE dbo.pp_UpdateRemovedColumn
GO

CREATE PROCEDURE dbo.pp_UpdateRemovedColumn @tableName varchar(128), @removedColumn varchar(128)
AS
BEGIN
	IF EXISTS(
		SELECT
			*
		FROM
			sys.columns 
		WHERE
			object_id = object_id(@tableName)
			AND NAME = 'Removed' + @removedColumn)
	BEGIN
		DECLARE @SqlQuery nvarchar(MAX) = CONCAT(
			'UPDATE Target
			SET
				Target.', @removedColumn, ' = inn.ContactID
			FROM ', @tableName, ' AS Target
			INNER JOIN Contact inn
				ON inn.UserID = Target.Removed', @removedColumn, '
				AND inn.CompanyID = Target.CompanyID
			WHERE
				Target.', @removedColumn, ' IS NULL;' )

		EXEC sp_executesql @SqlQuery
	END
END
GO

if exists(select * from sys.objects where object_id = object_id('dbo.pp_updstats') and objectproperty(object_id,'IsProcedure')=1)
drop proc dbo.pp_updstats
go
create procedure dbo.pp_updstats
with execute as 'dbo'
as
exec sp_updatestats
go

IF EXISTS(select * from sys.objects where object_id = object_id('dbo.pp_OwnerUpdateGIConditions') and objectproperty(object_id,'IsScalarFunction')=1)
  DROP FUNCTION dbo.pp_OwnerUpdateGIConditions
GO

CREATE FUNCTION pp_OwnerUpdateGIConditions (
  @CompanyID int,
  @DesignID nvarchar(36),
  @ParentAlias nvarchar(128),
  @ParentField nvarchar(128),
  @ChildAlias nvarchar(128),
  @ChildField nvarchar(128),
  @DisplayChildField nvarchar(128)
)
RETURNS bit
AS
BEGIN
  DECLARE @RET bit;
  SELECT @RET = CAST(
    CASE WHEN NOT EXISTS (
      SELECT FieldName FROM GIFilter WHERE CompanyID=@CompanyID AND DesignID=@DesignID AND FieldName like @ChildAlias + '.%'
      UNION
      SELECT DataFieldName FROM GIWhere WHERE CompanyID=@CompanyID AND DesignID=@DesignID AND DataFieldName like @ChildAlias + '.%'
      UNION
      SELECT DataFieldName FROM GIGroupBy WHERE CompanyID=@CompanyID AND DesignID=@DesignID AND DataFieldName like @ChildAlias + '.%'
      UNION
      SELECT Field FROM GIResult WHERE CompanyID=@CompanyID AND DesignID=@DesignID AND ObjectName=@ChildAlias AND Field NOT IN (@ChildField, @DisplayChildField)
      UNION
      SELECT 'yes' FROM (
        SELECT COUNT(*) as Cnt
        FROM GIOn o
          LEFT JOIN GIRelation r ON r.CompanyID=o.CompanyID AND o.DesignID=r.DesignID AND o.RelationNbr=r.LineNbr
        WHERE o.CompanyID >= @CompanyID
          AND o.DesignID=@DesignID
          AND r.ParentTable=@ParentAlias AND r.ChildTable=@ChildAlias
          AND o.ParentField=@ParentField AND o.ChildField=@ChildField
        HAVING COUNT(*) > 1
      ) RelCount
      UNION
      SELECT ParameterName FROM GINavigationParameter where CompanyID=@CompanyID AND DesignID=@DesignID AND ParameterName like @ChildAlias + '.%'
    ) THEN 1
    ELSE 0
    END
  AS BIT);
  RETURN @RET;
END;
GO

IF EXISTS(SELECT * FROM sys.objects WHERE object_id = object_id('dbo.pp_DeleteOwnerRelationForCustomGI') AND objectproperty(object_id,'IsProcedure') = 1)
  DROP PROCEDURE dbo.pp_DeleteOwnerRelationForCustomGI
GO

CREATE PROCEDURE dbo.pp_DeleteOwnerRelationForCustomGI 
  @MinCompanyID int,
  @DesignID nvarchar(36),
  @ParentAlias nvarchar(128),
  @ParentField nvarchar(128),
  @ChildAlias nvarchar(128),
  @ChildField nvarchar(128),
  @DisplayChildField nvarchar(128)
AS
BEGIN
  UPDATE GIResult 
  SET ObjectName=@ParentAlias, Field=@ParentField
  WHERE CompanyID>=@MinCompanyID AND DesignID=@DesignID AND ObjectName=@ChildAlias AND Field=@DisplayChildField
    AND dbo.pp_OwnerUpdateGIConditions(GIResult.CompanyID, @DesignID, @ParentAlias, @ParentField, @ChildAlias, @ChildField, @DisplayChildField) = 1;
  
  DELETE FROM GIResult 
  WHERE CompanyID>=@MinCompanyID AND DesignID=@DesignID AND ObjectName=@ChildAlias AND Field<>@DisplayChildField
    AND dbo.pp_OwnerUpdateGIConditions(GIResult.CompanyID, @DesignID, @ParentAlias, @ParentField, @ChildAlias, @ChildField, @DisplayChildField) = 1;
  
  DELETE o
  FROM GIOn o
    LEFT JOIN GIRelation r ON r.CompanyID=o.CompanyID AND o.DesignID=r.DesignID AND o.RelationNbr=r.LineNbr
  WHERE o.CompanyID >= @MinCompanyID
    AND o.DesignID=@DesignID
    AND r.ParentTable=@ParentAlias AND r.ChildTable=@ChildAlias
    AND o.ParentField=@ParentField AND o.ChildField=@ChildField
    AND dbo.pp_OwnerUpdateGIConditions(o.CompanyID, @DesignID, @ParentAlias, @ParentField, @ChildAlias, @ChildField, @DisplayChildField) = 1;
  
  DELETE r
  FROM GIRelation r
  WHERE r.CompanyID >= @MinCompanyID
    AND r.DesignID=@DesignID
    AND r.ParentTable=@ParentAlias AND r.ChildTable=@ChildAlias
    AND dbo.pp_OwnerUpdateGIConditions(r.CompanyID, @DesignID, @ParentAlias, @ParentField, @ChildAlias, @ChildField, @DisplayChildField) = 1;
  
  DELETE FROM GISort 
  WHERE CompanyID>=@MinCompanyID AND DesignID=@DesignID AND DataFieldName like @ChildAlias + '.%'
    AND dbo.pp_OwnerUpdateGIConditions(GISort.CompanyID, @DesignID, @ParentAlias, @ParentField, @ChildAlias, @ChildField, @DisplayChildField) = 1;
  
  DELETE FROM GITable 
  WHERE CompanyID>=@MinCompanyID AND DesignID=@DesignID AND Alias=@ChildAlias
    AND dbo.pp_OwnerUpdateGIConditions(GITable.CompanyID, @DesignID, @ParentAlias, @ParentField, @ChildAlias, @ChildField, @DisplayChildField) = 1;
END
GO
