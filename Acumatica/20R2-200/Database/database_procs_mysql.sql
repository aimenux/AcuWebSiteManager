--- Mysql stored procs and functions, please use double dollar sign as batch delimiter (analog for 'go' at mssql)

-- Create Function ident_current
DROP FUNCTION IF EXISTS `ident_current`$$
CREATE FUNCTION `ident_current`(tablename VARCHAR(80) CHARACTER SET utf8 COLLATE utf8_general_ci) RETURNS BIGINT(20) DETERMINISTIC READS SQL DATA
BEGIN
	RETURN COALESCE((SELECT `AUTO_INCREMENT` FROM information_schema.tables WHERE table_schema = DATABASE() AND table_name LIKE tablename LIMIT 1), 1);
END$$

-- Create Function pp_conv2smallInt
DROP FUNCTION IF EXISTS `pp_conv2smallInt`$$
CREATE FUNCTION `pp_conv2smallInt`(n BIGINT) RETURNS SMALLINT(6) DETERMINISTIC CONTAINS SQL RETURN CASE WHEN n > 32767 THEN 32767 WHEN n < -32768 THEN -32768 ELSE n END$$

-- Create Function pp_conv2int
DROP FUNCTION IF EXISTS `pp_conv2int`$$
CREATE FUNCTION `pp_conv2int`(n BIGINT) RETURNS INTEGER(10) DETERMINISTIC CONTAINS SQL RETURN CASE WHEN n > 2147483647 THEN 2147483647 WHEN n < -2147483648 THEN -2147483648 ELSE n END$$

-- Create Function pp_conv2tinyInt1
DROP FUNCTION IF EXISTS `pp_conv2tinyInt1`$$
CREATE FUNCTION `pp_conv2tinyInt1`(n BIGINT) RETURNS TINYINT(1) DETERMINISTIC CONTAINS SQL RETURN CASE WHEN n = 0 THEN 0 ELSE 1 END$$

-- Create View APAROrd
DROP VIEW IF EXISTS `APAROrd`$$
CREATE VIEW `APAROrd` AS SELECT `pp_conv2smallInt`(0) AS `ord` UNION ALL SELECT `pp_conv2smallInt`(1) AS `pp_conv2smallInt(1)`$$

-- Create Function binaryMaskAdd
DROP FUNCTION IF EXISTS `binaryMaskAdd`$$
CREATE FUNCTION `binaryMaskAdd`(source VARBINARY(2000), companyId INT, flags TINYINT) RETURNS VARBINARY(2000) DETERMINISTIC CONTAINS SQL 
BEGIN
DECLARE maskLength INT DEFAULT 32;
DECLARE bytePos INT DEFAULT FLOOR(( companyId + 3 ) / 4);
DECLARE newValue TINYINT UNSIGNED DEFAULT CASE companyId % 4 WHEN 1 THEN flags WHEN 2 THEN flags * 4 WHEN 3 THEN flags * 16 ELSE flags * 64 END;
DECLARE changedByte BINARY(1) DEFAULT CHAR(ASCII(SUBSTRING(source, bytePos, 1)) | newValue);

IF companyId <= 0 THEN RETURN source; END IF;
RETURN CONCAT(CASE bytePos WHEN 1 THEN '' ELSE SUBSTRING(source, 1, bytePos - 1) END, changedByte, CASE WHEN maskLength <= bytePos THEN '' ELSE SUBSTRING(source, bytePos + 1, maskLength - bytePos) END );
END;$$

-- Create Function binaryMaskSet
DROP FUNCTION IF EXISTS `binaryMaskSet`$$
CREATE FUNCTION `binaryMaskSet`(source VARBINARY(2000), companyId INT, flags TINYINT) RETURNS VARBINARY(2000) DETERMINISTIC CONTAINS SQL 
BEGIN
DECLARE maskLength INT DEFAULT 2000;
DECLARE bytePos INT DEFAULT FLOOR(( companyId + 3 ) / 4);
DECLARE newValue TINYINT UNSIGNED DEFAULT CASE companyId % 4 WHEN 1 THEN flags WHEN 2 THEN flags * 4 WHEN 3 THEN flags * 16 ELSE flags * 64 END;
DECLARE mask TINYINT UNSIGNED DEFAULT CASE companyId % 4 WHEN 1 THEN 252 WHEN 2 THEN 243 WHEN 3 THEN 207 ELSE 63 END;
DECLARE maskLen INT DEFAULT LENGTH(source);
DECLARE changedByte BINARY(1);
IF maskLen < bytePos THEN SET source = RPAD(source, bytePos, CHAR(flags * 85)); END IF;

SET changedByte = CHAR((ASCII(SUBSTRING(source, bytePos, 1)) & mask) | newValue);

IF companyId <= 0 THEN RETURN source; END IF;
RETURN CONCAT(CASE bytePos WHEN 1 THEN '' ELSE SUBSTRING(source, 1, bytePos - 1) END, changedByte, CASE WHEN maskLength > bytePos THEN SUBSTRING(source, bytePos + 1, maskLength - bytePos) ELSE '' END );
END;$$

-- Create Function binaryMaskSub
DROP FUNCTION IF EXISTS `binaryMaskSub`$$
CREATE FUNCTION `binaryMaskSub`(source VARBINARY(2000), companyId INT, flags TINYINT) RETURNS VARBINARY(2000) DETERMINISTIC CONTAINS SQL 
BEGIN
DECLARE maskLength INT DEFAULT 2000;
DECLARE res VARBINARY(2000) DEFAULT 0;
DECLARE bytePos INT DEFAULT FLOOR(( companyId + 3 ) / 4);
DECLARE mask TINYINT UNSIGNED DEFAULT 255 - CASE companyId % 4 WHEN 1 THEN flags WHEN 2 THEN flags * 4 WHEN 3 THEN flags * 16 ELSE flags * 64 END;
DECLARE changedByte BINARY(1) DEFAULT CHAR(ASCII(SUBSTRING(source, bytePos, 1)) & mask);

IF companyId <= 0 THEN RETURN source; END IF;
RETURN CONCAT(CASE bytePos WHEN 1 THEN '' ELSE SUBSTRING(source, 1, bytePos - 1) END, changedByte, CASE WHEN maskLength <= bytePos THEN '' ELSE SUBSTRING(source, bytePos + 1, maskLength - bytePos) END );
END;$$

-- Create Function binaryMaskCopy
DROP FUNCTION IF EXISTS `binaryMaskCopy`$$
CREATE FUNCTION `binaryMaskCopy`(source VARBINARY(2000), companyFrom INT, companyTo INT) RETURNS VARBINARY(2000) DETERMINISTIC CONTAINS SQL 
BEGIN
DECLARE maskLength INT DEFAULT 2000;
DECLARE srcBytePos INT DEFAULT FLOOR(( companyFrom + 3 ) / 4);
DECLARE dstBytePos INT DEFAULT FLOOR(( companyTo + 3 ) / 4);
DECLARE maxBytePos INT DEFAULT CASE WHEN srcBytePos > dstBytePos THEN srcBytePos ELSE dstBytePos END;
DECLARE srcPosMask TINYINT UNSIGNED DEFAULT CASE companyFrom % 4 WHEN 1 THEN 3 WHEN 2 THEN 12 WHEN 3 THEN 48 ELSE 192 END;
DECLARE srcDivisor TINYINT UNSIGNED DEFAULT CASE companyFrom % 4 WHEN 1 THEN 1 WHEN 2 THEN 4 WHEN 3 THEN 16 ELSE 64 END;
DECLARE dstNegMask TINYINT UNSIGNED DEFAULT CASE companyTo % 4 WHEN 1 THEN 252 WHEN 2 THEN 243 WHEN 3 THEN 207 ELSE 63 END;
DECLARE dstMultiplier TINYINT UNSIGNED DEFAULT CASE companyTo % 4 WHEN 1 THEN 1 WHEN 2 THEN 4 WHEN 3 THEN 16 ELSE 64 END;
DECLARE srcValue TINYINT UNSIGNED DEFAULT (ASCII(SUBSTRING(source, srcBytePos, 1)) & srcPosMask) / srcDivisor;
DECLARE changedByte BINARY(1);
IF companyTo <= 0 THEN RETURN source; END IF;

IF companyFrom <= 0 OR LENGTH(source) < srcBytePos THEN SET srcValue = 2; END IF;
IF LENGTH(source) < maxBytePos THEN SET source = RPAD(source, maxBytePos, '\0'); END IF;

SET changedByte = CHAR((ASCII(SUBSTRING(source, dstBytePos, 1)) & dstNegMask) | (srcValue * dstMultiplier));

RETURN CONCAT(CASE dstBytePos WHEN 1 THEN '' ELSE SUBSTRING(source, 1, dstBytePos - 1) END, changedByte, CASE WHEN maskLength <= dstBytePos THEN '' ELSE SUBSTRING(source, dstBytePos + 1, maskLength - dstBytePos) END );
END;$$

-- Create Function binaryMaskTest
DROP FUNCTION IF EXISTS `binaryMaskTest`$$
CREATE FUNCTION `binaryMaskTest`(source VARBINARY(2000), companyId INT, flags TINYINT) RETURNS TINYINT(1) DETERMINISTIC CONTAINS SQL 
BEGIN
DECLARE mask TINYINT UNSIGNED DEFAULT CASE companyId % 4 WHEN 1 THEN flags WHEN 2 THEN flags * 4 WHEN 3 THEN flags * 16 ELSE flags * 64 END;
RETURN CASE ASCII(SUBSTRING(source, FLOOR(( companyId + 3 ) / 4), 1)) & mask WHEN mask then 1 else 0 end;
END;$$

-- Create StoredProcedure pp_GetCompanyRelations
DROP PROCEDURE IF EXISTS `pp_GetCompanyRelations`$$
CREATE PROCEDURE `pp_GetCompanyRelations`()
BEGIN
  DECLARE w_cnt INT UNSIGNED DEFAULT 1;
  
  DROP TEMPORARY TABLE IF EXISTS `companyRelations`;
  CREATE TEMPORARY TABLE `companyRelations`( `Viewer` INT NOT NULL, `Child` INT NOT NULL, `Parent` INT NOT NULL, PRIMARY KEY (Viewer, Child, Parent) ) ENGINE=MEMORY;

  -- mysql cannot reopen the same temporary table (i.e. join it twice or insert into it when its joined in select) and throws error 1137
  -- second temp table solves the problem
  DROP TEMPORARY TABLE IF EXISTS `tmp_cr2`;
  CREATE TEMPORARY TABLE `tmp_cr2`( `Viewer` INT NOT NULL, `Child` INT NOT NULL, `Parent` INT NOT NULL, PRIMARY KEY (Viewer, Child, Parent) ) ENGINE=MEMORY;

  INSERT INTO `companyRelations` SELECT `CompanyID`, `CompanyID`, `ParentCompanyID` FROM `Company` WHERE CompanyID > 0 AND ParentCompanyID > 0;
  SET w_cnt = ROW_COUNT();  
  
  WHILE w_cnt > 0 DO
    INSERT IGNORE INTO tmp_cr2 
      SELECT c1.Viewer, c2.CompanyID, c2.ParentCompanyID FROM `companyRelations` c1 
      INNER JOIN Company c2 ON (c1.Parent = c2.CompanyID) 
      WHERE c2.CompanyID > 0 AND c2.ParentCompanyID > 0;
      
    SET w_cnt = ROW_COUNT();      
    INSERT IGNORE INTO companyRelations SELECT * FROM `tmp_cr2`;
  END WHILE;

  DROP TEMPORARY TABLE IF EXISTS `tmp_cr3`;  
  CREATE TEMPORARY TABLE `tmp_cr3`( `Viewer` INT NOT NULL, `Child` INT NOT NULL, `Parent` INT NOT NULL, PRIMARY KEY (Viewer, Child, Parent) ) ENGINE=MEMORY;
  INSERT IGNORE INTO `tmp_cr3` SELECT * FROM `companyRelations`;
  
  SET w_cnt = 1;
  WHILE w_cnt > 0 DO
    INSERT IGNORE INTO tmp_cr2 
      SELECT c1.Viewer, c1.Child, c2.Parent FROM `companyRelations` c1 
      INNER JOIN `tmp_cr3` c2 ON (c1.Viewer = c2.Viewer AND c2.Child = c1.Parent );
      
    SET w_cnt = ROW_COUNT();      
    INSERT IGNORE INTO companyRelations SELECT * FROM `tmp_cr2`;
  END WHILE;

END;$$

-- Create StoredProcedure pp_GetMatrix
DROP PROCEDURE IF EXISTS `pp_GetMatrix`;$$
CREATE PROCEDURE `pp_GetMatrix`(IN companyID_in INT, IN companyMask_in VARBINARY(120)) 
BEGIN
  DECLARE w_cnt INT UNSIGNED DEFAULT 1;

  DROP TEMPORARY TABLE IF EXISTS `matrix`;
  CREATE TEMPORARY TABLE `matrix`( `CompanyID` INT NOT NULL, PRIMARY KEY (CompanyID) ) ENGINE=MEMORY;

  -- mysql cannot reopen the same temporary table (i.e. join it twice or insert into it when its joined in select) and throws error 1137
  -- second temp table solves the problem
  DROP TEMPORARY TABLE IF EXISTS `matrix2`;
  CREATE TEMPORARY TABLE `matrix2`( `CompanyID` INT NOT NULL, PRIMARY KEY (CompanyID) ) ENGINE=MEMORY;

  INSERT `matrix` VALUES (companyID_in);

  WHILE w_cnt > 0 DO
    INSERT IGNORE INTO `matrix2`
    SELECT c.CompanyID
    FROM Company c
    INNER JOIN `matrix` ON `matrix`.CompanyID = c.ParentCompanyID;
    SET w_cnt = ROW_COUNT();
    INSERT IGNORE INTO `matrix` SELECT * FROM `matrix2`;
   END WHILE;
   -- exclude hidden records
   DELETE FROM `matrix` WHERE binaryMaskTest(companyMask_in, CompanyID, 2) = 0 AND CompanyID <> companyID_in;
END;$$

DROP PROCEDURE IF EXISTS `pp_ExecuteUntilZeroRows`;$$

CREATE PROCEDURE `pp_ExecuteUntilZeroRows`(IN queryText VARCHAR(4000))
BEGIN
  DECLARE w_cnt INT UNSIGNED DEFAULT 1;
  SET @qtext = queryText; 
  PREPARE stmt FROM @qtext;
  WHILE w_cnt > 0 DO
    EXECUTE stmt;
    SET w_cnt = ROW_COUNT();
  END WHILE;
  DEALLOCATE PREPARE stmt;
END;$$

DROP FUNCTION IF EXISTS `GetDateByWeek`;$$

CREATE FUNCTION `GetDateByWeek`(pYear int, pMonth int, pWeek int, pWeekday int, pTime time)
RETURNS DATETIME
BEGIN
	declare res datetime;
    if (pWeek < 5) 
    then
		begin
			declare prLastday datetime;
			select last_day( DATE_ADD(makedate(pYear, 1), interval pMonth - 2 month) ) into prLastday;   
			SELECT DATE_ADD( makedate(1900, pWeekday), INTERVAL DATEDIFF(prLastday, makedate(1900, pWeekday)) div 7 *7 + 7 * pWeek DAY) into res;  
        end ;
	else
		begin 
			declare lastday datetime;
			select last_day( DATE_ADD(makedate(pYear, 1), interval pMonth - 1 month) ) into lastday;   
			SELECT DATE_ADD( makedate(1900, pWeekday), INTERVAL DATEDIFF(lastday, makedate(1900, pWeekday)) div 7 *7 DAY) into res;    
		end ;
	end if;
    RETURN ADDTIME(res, pTime);
    
END;$$

-- https://forums.mysql.com/read.php?10,377625,378101#msg-378101
drop procedure if exists sp_calcRowIndexSize $$
create procedure sp_calcRowIndexSize (dbName VARCHAR(255),
                                       tableName  VARCHAR(255),
                                       columnName  VARCHAR(255),
                                       out index_size bigint)
BEGIN	
	if (dbName = '' ) or (tableName = '') or (columnName = '') then
		select   'Please enter all the parameters.' AS MSG_ERROR;
	else
		-- select columnName;
    
		set @v_column_type  = '';
		set @max_length = 0;
        set @is_nullable = 0;

		select c.data_type INTO @v_column_type
		FROM information_schema.columns c
		where c.table_name = tableName
		and c.column_name = columnName
		and c.table_schema = dbName
		limit 1;
		
        -- select @v_column_type;
        
		-- STRING TYPE
		
        if (@v_column_type = 'varchar'
			or @v_column_type = 'char'
			or @v_column_type = 'tinyblob'
            or @v_column_type = 'tinytext'
			or @v_column_type = 'blob'
            or @v_column_type = 'text'
			or @v_column_type = 'mediumblob'
            or @v_column_type = 'mediumtext'
			or @v_column_type = 'longblob'
            or @v_column_type = 'longtext') then
		begin
			select c.character_maximum_length into @max_length
            FROM information_schema.columns c
			where c.table_name = tableName
			and c.column_name = columnName
			and c.table_schema = dbName
			limit 1;
            
            if (@max_length >= 0 and @max_length <= 30) then
				set @max_length = round(@max_length / 2);
			elseif (@max_length = -1 or @max_length > 512) then
            begin
				/*set @query = concat('select avg(length(',columnName,')) into @max_length
					from ', tableName);
                prepare stm from @query;
                execute stm;*/
                set @max_length = 0;
            end;
            else
				set @max_length = round(@max_length / 10);
            end if;
            
            -- select @max_length;
            
            select c.is_nullable into @is_nullable
            FROM information_schema.columns c
			where c.table_name = tableName
			and c.column_name = columnName
			and c.table_schema = dbName
			limit 1;
            
            if (@is_nullable = 'YES') then
				set @max_length = @max_length + 1;
			end if;

			if (@v_column_type = 'varchar') then		 
				select (@max_length + 1 + 5) * 2.8 into index_size;		 			
			elseif (@v_column_type = 'char') then
				select (@max_length+5)*2.8 into index_size;		 			
			elseif (@v_column_type = 'tinyblob') or (@v_column_type = 'tinytext') then
				select (@max_length+1+5)*2.8 into index_size;
			elseif (@v_column_type = 'blob') or (@v_column_type = 'text') then
				select (@max_length+2+5)*2.8 into index_size;		 			
			elseif (@v_column_type = 'mediumblob') or (@v_column_type = 'mediumtext') then
				select (@max_length+3+5)*2.8 into index_size;		 			
			elseif (@v_column_type = 'longblob') or (@v_column_type = 'longtext') then
				select (@max_length+4+5)*2.8 into index_size;
            end if;

        end;
        
        -- DATE and TIME TYPE
        
        elseif (@v_column_type = 'date'
			or @v_column_type = 'datetime'
			or @v_column_type = 'time'
			or @v_column_type = 'year') then
		begin
			select c.datetime_precision into @max_length
            FROM information_schema.columns c
			where c.table_name = tableName
			and c.column_name = columnName
			and c.table_schema = dbName
			limit 1;
            
			select c.is_nullable into @is_nullable
            FROM information_schema.columns c
			where c.table_name = tableName
			and c.column_name = columnName
			and c.table_schema = dbName
			limit 1;
            
            if (@is_nullable = 'YES') then
				set @max_length = @max_length + 1;
			end if;
            
			if (@v_column_type = 'date') or (@v_column_type = 'time') then
				select @max_length+3+5 into index_size;
			elseif (@v_column_type = 'datetime') then
				select @max_length+8+5 into index_size;
			elseif (@v_column_type = 'year') then
				select @max_length+5 into index_size;
			end if;
        end;
        
		-- NUMBER TYPE
        
        elseif (@v_column_type = 'tinyint'
			or @v_column_type = 'smallint'
			or @v_column_type = 'mediumint'
			or @v_column_type = 'int'
            or @v_column_type = 'integer'
            or @v_column_type = 'bigint'
            or @v_column_type = 'double'
            or @v_column_type = 'float'
            or @v_column_type = 'bit'
            or @v_column_type = 'decimal'
            or @v_column_type = 'numeric') then
		begin
        
			select c.numeric_precision into @max_length
            FROM information_schema.columns c
			where c.table_name = tableName
			and c.column_name = columnName
			and c.table_schema = dbName
			limit 1;
            
            select c.is_nullable into @is_nullable
            FROM information_schema.columns c
			where c.table_name = tableName
			and c.column_name = columnName
			and c.table_schema = dbName
			limit 1;
            
            if (@is_nullable = 'YES') then
				set @max_length = @max_length + 1;
			end if;
        
			if (@v_column_type = 'tinyint') then
				select (@max_length+5)*2.8 into index_size;
			elseif (@v_column_type = 'smallint') then
				select (@max_length+2+5)*2.8 into index_size;
			elseif (@v_column_type = 'mediumint') then
				select (@max_length+3+5)*2.8 into index_size;
			elseif (@v_column_type = 'int') or (@v_column_type = 'integer') then
				select (@max_length+4+5)*2.8 into index_size;
			elseif (@v_column_type = 'bigint') then
				select (@max_length+8+5)*2.8 into index_size;
			elseif (@v_column_type = 'float') then
				select (@max_length+4+5)*2.8 into index_size;
			elseif (@v_column_type = 'double') then
				select (@max_length+8+5)*2.8 into index_size;
			elseif (@v_column_type = 'bit') then
				select (@max_length+1+5)*2.8 into index_size;
			elseif (@v_column_type = 'decimal') or (@v_column_type = 'numeric') then
				select (@max_length+5)*2.8 into index_size;
			end if;
		end;
		end if;
	end if;	
    
    set index_size = if (index_size is null, 0, index_size);
    -- select index_size;

END $$

DROP PROCEDURE IF EXISTS getTables $$
create procedure getTables(schema_name text)
begin
	declare tablename nvarchar(255);
	declare stmt longtext;
    declare v_finished int default 0;
	declare csr cursor for
		select distinct c.table_name
        from information_schema.columns c
        where c.column_name = 'CompanyID'
        and c.table_schema = schema_name;
    DECLARE CONTINUE HandLER 
	FOR NOT FOUND set v_finished = 1;
    
	drop temporary table if exists TableNames;
	CREATE temporary TABLE TableNames (
		TableName nvarchar(255) NOT NULL,
	 CONSTRAINT TableNames_pk PRIMARY KEY CLUSTERED 
	(
		TableName ASC
	));
	
	open csr;
	get_table: loop
		fetch csr into tablename;
		if v_finished = 1 then
			leave get_table;
		end if;
		insert into TableNames
        select tablename;
	end loop get_table;
	close csr;
end $$

DROP PROCEDURE IF EXISTS sp_CalculateTablesSize $$
create procedure sp_CalculateTablesSize(schema_name text)
begin
	declare table_name nvarchar(255);
    declare v_finished int default 0;
	declare csr cursor for
	select distinct TableName from TableSize; -- where TableName = 'audefinition';
    DECLARE CONTINUE HandLER 
	FOR NOT FOUND set v_finished = 1;
    
    set SQL_SAFE_UPDATES = 0;
    truncate table TableSize;
    
    call getTables(schema_name);
    
    insert into TableSize -- (TableName, SizeByCompany, IndexSizeByCompany, CountOfCompanyRecords, RealSize, Company)
	select s.*, c.CompanyId from
	(
		select max(t.table_name) as name, 0 as Size, 0 as IndexSize, SUM(t.table_rows) as calcedcount, sum(t.data_length + t.index_length) AS PkSpaceKB
		from information_schema.tables as t 
		inner join TableNames as n on t.table_name = n.TableName
		where t.table_schema = schema_name
		group by t.table_name
		order by sum(t.data_length + t.index_length) desc
		limit 100
	) s
	inner JOIN Company c
	on c.CompanyId <> 1;
    
	open csr;
	get_table: loop
		fetch csr into table_name;
		if v_finished = 1 then
			leave get_table;
		end if;
		
		select sum(
			case
				when c.data_type = 'varchar'
				or c.data_type = 'char'
				or c.data_type = 'tinyblob'
				or c.data_type = 'blob'
                or c.data_type = 'text'
				or c.data_type = 'mediumblob'
                or c.data_type = 'mediumtext'
				or c.data_type = 'longblob'
                or c.data_type = 'longtext'
				then (
					case
						when c.character_maximum_length >= 0 and c.character_maximum_length <= 30
							then round(c.character_maximum_length / 2)
						when c.character_maximum_length = -1
							then 0
						when c.character_maximum_length > 512
							then 0
						else round(c.character_maximum_length / 10)
					end) + if(c.is_nullable = 'YES', 1, 0)
				when c.data_type = 'date'
				or c.data_type = 'time'
				or c.data_type = 'datetime'
				or c.data_type = 'year'
				then 
					c.datetime_precision
				else
					c.numeric_precision
				end + c.is_nullable)
		into @size
		from information_schema.columns c
		where c.table_schema = schema_name
        and c.table_name = table_name;
        
        set @rowIndexSize = 0;
        
        select table_name;
        
        columnCursor: begin
        declare columnName nvarchar(255);
        declare c_finished int default 0;
        declare c_csr cursor for
			select c.column_name
			from information_schema.columns c
			where c.table_schema = schema_name
			and c.table_name = table_name
			and c.column_key <> '';

		DECLARE CONTINUE HandLER 
		FOR NOT FOUND set c_finished = 1;
		open c_csr;
		get_column: loop
			fetch c_csr into columnName;
			if c_finished = 1 then
				leave get_column;
			end if;
            
            call sp_calcRowIndexSize(schema_name, table_name, columnName, @index_size);
            
            set @rowIndexSize = @rowIndexSize + @index_size;
            -- select columnName, @index_size, @rowIndexSize;

        end loop get_column;
		close c_csr;
        end columnCursor;

        -- select @rowIndexSize;
        
        if ((select sum(1 = 1)
			from information_schema.columns c
            where c.table_name = table_name
            and c.table_schema = schema_name
            and (c.data_type = 'varchar'
				or c.data_type = 'char'
				or c.data_type = 'tinyblob'
				or c.data_type = 'blob'
                or c.data_type = 'text'
				or c.data_type = 'mediumblob'
                or c.data_type = 'mediumtext'
				or c.data_type = 'longblob'
                or c.data_type = 'longtext')
            and (c.character_maximum_length = -1 or c.character_maximum_length > 512)) is not null) then
        begin
			-- declare v_cols longtext;
            set @v_cols = '';
            select @v_cols := Concat(@v_cols, '`),0)+coalesce(length(t.`' , c.column_name)
            -- select column_name, data_type, character_maximum_length
			-- 	into @v_cols
				from information_schema.columns c
				where c.table_name = table_name -- 'Address'
                and c.table_schema = schema_name
				and (c.data_type = 'varchar'
					or c.data_type = 'char'
					or c.data_type = 'tinyblob'
					or c.data_type = 'blob'
					or c.data_type = 'text'
					or c.data_type = 'mediumblob'
					or c.data_type = 'mediumtext'
					or c.data_type = 'longblob'
					or c.data_type = 'longtext')
				and (c.character_maximum_length = -1 or c.character_maximum_length > 512);
			-- declare v_cols longtext;
            set @v_cols := Concat('sum(' , substring(@v_cols, 7, char_length(rtrim(@v_cols)) - 6) , '`),0))');
            -- select @v_cols;
            
            set @index_cols = '';
            select @index_cols := Concat(@index_cols, '`),0)+coalesce(length(t.`' , c.column_name)
            -- select column_name, data_type, character_maximum_length
			-- 	into @v_cols
				from information_schema.columns c
				where c.table_name = table_name
                and c.table_schema = schema_name
                and c.column_key <> ''
				and (c.data_type = 'varchar'
					or c.data_type = 'char'
					or c.data_type = 'tinyblob'
					or c.data_type = 'blob'
					or c.data_type = 'text'
					or c.data_type = 'mediumblob'
					or c.data_type = 'mediumtext'
					or c.data_type = 'longblob'
					or c.data_type = 'longtext')
				and (c.character_maximum_length = -1 or c.character_maximum_length > 512);
			-- declare v_cols longtext;
            set @index_cols := if (@index_cols = '', 0,  Concat('sum(' , substring(@index_cols, 7, char_length(rtrim(@index_cols)) - 6) , '`),0))'));
            select @index_cols;
            
            set @queryString :=
				Concat_ws('','update TableSize old,
				( select ', @v_cols ,' as delta, ', @index_cols, ' as indexDelta, c.CompanyId, if (count(*) = 1 and (select sum(1 = 1) from ', table_name,' where CompanyId = c.CompanyId) is null, 0, count(*)) as CountOfCompanyRecords
					from Company c 
					left join ', table_name ,' t on t.CompanyId = c.CompanyId
					where c.CompanyId <> 1
					group by c.CompanyId
					) s
				set old.CountOfCompanyRecords = s.CountOfCompanyRecords,
					old.SizeByCompany = if (s.CountOfCompanyRecords = 0, 0, s.CountOfCompanyRecords * ', @size, ' + s.delta),
					old.IndexSizeByCompany = if (s.CountOfCompanyRecords = 0, 0, s.CountOfCompanyRecords * ', @rowIndexSize, ' + s.indexDelta)
				where old.Company = s.CompanyId and old.TableName = \'', table_name , '\'');
			select @queryString;
        end;
        else
        begin
			set @queryString :=
				Concat_ws('','update TableSize old,
				( select c.CompanyId, if (count(*) = 1 and (select sum(1 = 1) from ', table_name,' where CompanyId = c.CompanyId) is null, 0, count(*)) as CountOfCompanyRecords
					from Company c 
					left join ', table_name ,' t on t.CompanyId = c.CompanyId
					where c.CompanyId <> 1
					group by c.CompanyId
					) s
				set old.CountOfCompanyRecords = s.CountOfCompanyRecords,
					old.SizeByCompany = if (s.CountOfCompanyRecords = 0, 0, s.CountOfCompanyRecords * ', @size, '),
					old.IndexSizeByCompany = if (s.CountOfCompanyRecords = 0, 0, s.CountOfCompanyRecords * ', @rowIndexSize, ' )
				where old.Company = s.CompanyId and old.TableName = \'', table_name, '\'');
			-- select @queryString;
        end;
        end if;
        
        prepare stm from @queryString;
		execute stm;
        
	end loop get_table;
	close csr;
end $$

DROP PROCEDURE IF EXISTS sp_updatestats $$
CREATE PROCEDURE sp_updatestats(schema_name text)
begin
  declare tablename nvarchar(255);
  declare stmt longtext;
  declare v_finished int default 0;
  declare csr cursor for
          select distinct c.table_name
          from information_schema.columns c
          where c.column_name = 'CompanyID'
          and c.table_schema = schema_name;
  DECLARE CONTINUE HandLER
  FOR NOT FOUND set v_finished = 1;
  open csr;
  get_table: loop
             fetch csr into tablename;
             if v_finished = 1 then
                leave get_table;
             end if;
             set @queryString := Concat_ws('','analyze table ' , tablename, ';');
             prepare stm from @queryString;
             execute stm;
   end loop get_table;
   close csr;
end$$

-- Create Function binaryMaskOr
DROP FUNCTION IF EXISTS `binaryMaskOr`$$
CREATE FUNCTION `binaryMaskOr`(maskA VARBINARY(128), maskB VARBINARY(128)) RETURNS VARBINARY(128) DETERMINISTIC CONTAINS SQL 
BEGIN
  DECLARE lengthOfA TINYINT UNSIGNED DEFAULT LENGTH(maskA);
  DECLARE lengthOfB TINYINT UNSIGNED DEFAULT LENGTH(maskB);
  DECLARE stopIndex TINYINT UNSIGNED;
  DECLARE result VARBINARY(128);
  DECLARE other VARBINARY(128);
  DECLARE resultByte BINARY(1);
  DECLARE otherByte BINARY(1);
  DECLARE byteIndex TINYINT UNSIGNED DEFAULT 1;

  IF lengthOfA > lengthOfB THEN
  BEGIN
    SET stopIndex = lengthOfB;
    SET result = maskA;
    SET other = maskB;
  END;
  ELSE
  BEGIN
    SET stopIndex = lengthOfA;
    SET result = maskB;
    SET other = maskA;
  END;
  END IF;

  WHILE byteIndex <= stopIndex DO
    SET resultByte = CHAR(ASCII(SUBSTRING(result, byteIndex, 1)) | ASCII(SUBSTRING(other, byteIndex, 1)));
    SET result = INSERT(result, byteIndex, 1, resultByte);
    SET byteIndex = byteIndex + 1;
  END WHILE;

RETURN result;
END;$$

DROP PROCEDURE IF EXISTS `pp_UpdateConfigurationOfCustomGIs`$$
CREATE PROCEDURE `pp_UpdateConfigurationOfCustomGIs`(oldTableName VARCHAR(255), oldFieldName VARCHAR(255), newTableName VARCHAR(255), newFieldName VARCHAR(255)) 
BEGIN
	
    SET SQL_SAFE_UPDATES = 0;
	
	IF (NOT oldTableName IS NULL) AND (NOT newTableName IS NULL) AND (oldTableName <> newTableName) THEN
		UPDATE GITable
		SET Name = newTableName
		WHERE Name = oldTableName
			AND CompanyID > 1;
	END IF;
	
	IF (NOT oldFieldName IS NULL) AND (NOT newFieldName IS NULL) AND (oldFieldName <> newFieldName) AND (NOT newTableName IS NULL) THEN
	
		UPDATE GIResult r
		INNER JOIN GITable t
			ON t.DesignID = r.DesignID
			AND t.CompanyID = r.CompanyID
			AND r.ObjectName = t.Alias
		SET Field = CONCAT(LOWER(LEFT(newFieldName, 1)), SUBSTRING(newFieldName, 2))
		WHERE r.CompanyID > 1
			AND r.Field = oldFieldName
			AND t.Name = newTableName;

		UPDATE GIResult r
		INNER JOIN GITable t
			ON r.ObjectName = t.Alias
			AND r.CompanyID = t.CompanyID
			AND r.DesignID = t.DesignID
		SET Field = REPLACE(Field, CONCAT(t.Alias, '.', oldFieldName), CONCAT(t.Alias, '.', newFieldName))
		WHERE r.CompanyID > 1
			AND INSTR( LOWER(t.Name), LOWER(newTableName) ) > 0
			AND INSTR( LOWER(r.Field), CONCAT(LOWER(t.Alias), '.', LOWER(newTableName)) ) > 0;
	
		UPDATE FilterRow fr
		SET DataField = REPLACE(fr.DataField, oldFieldName, CONCAT(LOWER(LEFT(newFieldName, 1)), SUBSTRING(newFieldName, 2)))
		WHERE fr.CompanyID > 1
			AND INSTR(CONCAT(LOWER(newTableName), '_', oldFieldName), LOWER(fr.DataField)) > 0;

		UPDATE GINavigationParameter np
		INNER JOIN GITable t
			ON t.DesignID = np.DesignID
			AND t.CompanyID = np.CompanyID
			AND INSTR(LOWER(np.ParameterName), LOWER(t.Alias)) > 0
		SET FieldName = newFieldName, ParameterName = CONCAT(t.Alias, '.', newFieldName)
		WHERE np.CompanyID > 1
			AND LOWER(np.ParameterName) = LOWER(CONCAT(t.Alias, '.', oldFieldName))
			AND t.Name = newTableName;

		UPDATE GISort s
		INNER JOIN GITable t
			ON t.DesignID = s.DesignID
			AND t.CompanyID = s.CompanyID
			AND INSTR(LOWER(s.DataFieldName), LOWER(t.Alias)) > 0
		SET DataFieldName = CONCAT(t.Alias, '.', LOWER(LEFT(newFieldName, 1)), SUBSTRING(newFieldName, 2))
		WHERE s.CompanyID > 1
			AND LOWER(s.DataFieldName) = LOWER(CONCAT(t.Alias, '.', oldFieldName))
			AND t.Name = newTableName;

		UPDATE GIGroupBy g
		INNER JOIN GITable t
			ON t.DesignID = g.DesignID
			AND t.CompanyID = g.CompanyID
			AND INSTR(LOWER(g.DataFieldName), LOWER(t.Alias)) > 0
		SET DataFieldName = CONCAT(t.Alias, '.', newFieldName)
		WHERE g.CompanyID > 1
			AND LOWER(g.DataFieldName) = LOWER(CONCAT(t.Alias, '.', oldFieldName)) AND t.Name = newTableName;

		UPDATE GIFilter f
		INNER JOIN GITable t
			ON t.DesignID = f.DesignID
			AND t.CompanyID = f.CompanyID
			AND INSTR(LOWER(f.FieldName), LOWER(t.Alias)) > 0
		SET FieldName = CONCAT(t.Alias, '.', LOWER(LEFT(newFieldName, 1)), SUBSTRING(newFieldName, 2)),
			f.`Name` = newFieldName
		WHERE f.CompanyID > 1
			AND LOWER(f.FieldName) = LOWER(CONCAT(t.Alias, '.', oldFieldName))
			AND newTableName = t.Name;

		UPDATE GIOn o
		INNER JOIN GIRelation r
			ON r.DesignID = o.DesignID
			AND r.CompanyID = o.CompanyID
			AND r.LineNbr = o.RelationNbr
		INNER JOIN GITable pt
			ON pt.DesignID = r.DesignID
			AND pt.CompanyID = r.CompanyID
			AND pt.Alias = r.ParentTable
		SET ParentField = CONCAT(LOWER(LEFT(newFieldName, 1)), SUBSTRING(newFieldName, 2))
		WHERE o.CompanyID > 1
			AND LOWER(oldFieldName) = LOWER(o.ParentField)
			AND newTableName = pt.Name;

		UPDATE GIOn o
		INNER JOIN GIRelation r
			ON r.DesignID = o.DesignID
			AND r.CompanyID = o.CompanyID
			AND r.LineNbr = o.RelationNbr
		INNER JOIN GITable ct
			ON ct.DesignID = r.DesignID
			AND ct.CompanyID = r.CompanyID
			AND ct.Alias = r.ChildTable
		SET ChildField = CONCAT(LOWER(LEFT(newFieldName, 1)), SUBSTRING(newFieldName, 2))
		WHERE o.CompanyID > 1
			AND LOWER(oldFieldName) = LOWER(o.ChildField)
			AND newTableName = ct.Name;

		UPDATE GIWhere w
		INNER JOIN GITable t
			ON t.DesignID = w.DesignID
			AND t.CompanyID = w.CompanyID
			AND INSTR(LOWER(w.DataFieldName), LOWER(t.Alias)) > 0
		SET DataFieldName = CONCAT(t.Alias, '.', LOWER(LEFT(newFieldName, 1)), SUBSTRING(newFieldName, 2))
		WHERE w.CompanyID > 1
			AND LOWER(w.DataFieldName) = LOWER(CONCAT(t.Alias, '.', oldFieldName))
			AND newTableName = t.Name;

	END IF;
	
END;$$

-- Create Procedure pp_CopyCustomerRolesInGraphForScreenIfNotSetYet
DROP PROCEDURE IF EXISTS `pp_CopyCustomerRolesInGraphForScreenIfNotSetYet`$$
CREATE PROCEDURE `pp_CopyCustomerRolesInGraphForScreenIfNotSetYet`(sourceScreenID VARCHAR(8), newScreenID VARCHAR(8))
BEGIN
	
	IF NOT EXISTS(Select * FROM RolesInGraph AS RIG WHERE RIG.CompanyID > 1 AND RIG.ScreenID = newScreenID) THEN
		
		SET SQL_SAFE_UPDATES = 0;
		
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
				oldRIG.CompanyID,
				newScreenID,
				oldRIG.Rolename,
				oldRIG.ApplicationName,
				oldRIG.Accessrights,
				oldRIG.CompanyMask,
				oldRIG.CreatedByID,
				oldRIG.CreatedByScreenID,
				oldRIG.CreatedDateTime,
				oldRIG.LastModifiedByID,
				oldRIG.LastModifiedByScreenID,
				oldRIG.LastModifiedDateTime,
				oldRIG.RecordSourceID
			FROM RolesInGraph AS oldRIG
			WHERE oldRIG.CompanyID > 1 AND oldRIG.ScreenID = sourceScreenID;
		
	END IF;
	
END;$$

-- Create Procedure pp_UpdateRemovedColumn
DROP PROCEDURE IF EXISTS `pp_UpdateRemovedColumn`$$
CREATE PROCEDURE `pp_UpdateRemovedColumn`(tableName VARCHAR(128), removedColumn VARCHAR(128))
BEGIN
	SET @colName = '';
	SELECT
		IFNULL(column_name, '') INTO @colName
	FROM
		information_schema.columns 
	WHERE
		table_schema = DATABASE()
		AND table_name = tableName
		AND column_name = CONCAT('Removed', removedColumn);

	IF @colName != '' THEN 

		SET @SqlQuery := CONCAT('UPDATE ', tableName, ' AS Target
			INNER JOIN Contact inn
				ON inn.UserID = Target.Removed', removedColumn, '
				AND inn.CompanyID = Target.CompanyID
			SET
				Target.', removedColumn, ' = inn.ContactID
			WHERE Target.', removedColumn, ' IS NULL;' );
		SELECT @SqlQuery;
		PREPARE stmt FROM @SqlQuery;
		EXECUTE stmt;

	END IF;

END;$$

DROP FUNCTION IF EXISTS pp_OwnerUpdateGIConditions $$
CREATE FUNCTION pp_OwnerUpdateGIConditions (
  _CompanyID int,
  _DesignID nvarchar(36),
  _ParentAlias nvarchar(128),
  _ParentField nvarchar(128),
  _ChildAlias nvarchar(128),
  _ChildField nvarchar(128),
  _DisplayChildField nvarchar(128)
)
RETURNS BOOL
BEGIN
  RETURN NOT EXISTS (
    SELECT FieldName FROM GIFilter WHERE CompanyID=_CompanyID AND DesignID=_DesignID AND FieldName like CONCAT(_ChildAlias, '.%')
    UNION
    SELECT DataFieldName FROM GIWhere WHERE CompanyID=_CompanyID AND DesignID=_DesignID AND DataFieldName like CONCAT(_ChildAlias, '.%')
    UNION
    SELECT DataFieldName FROM GIGroupBy WHERE CompanyID=_CompanyID AND DesignID=_DesignID AND DataFieldName like CONCAT(_ChildAlias, '.%')
    UNION
    SELECT Field FROM GIResult WHERE CompanyID=_CompanyID AND DesignID=_DesignID AND ObjectName=_ChildAlias AND Field NOT IN (_ChildField, _DisplayChildField)
    UNION
    SELECT 'yes' FROM (
      SELECT COUNT(*) as Cnt
      FROM GIOn o
          LEFT JOIN GIRelation r ON r.CompanyID=o.CompanyID AND o.DesignID=r.DesignID AND o.RelationNbr=r.LineNbr
      WHERE o.CompanyID = _CompanyID
        AND o.DesignID=_DesignID
        AND r.ParentTable=_ParentAlias AND r.ChildTable=_ChildAlias
        AND o.ParentField=_ParentField AND o.ChildField=_ChildField
      HAVING COUNT(*) > 1
    ) RelCount
    UNION
    SELECT ParameterName FROM GINavigationParameter where CompanyID=_CompanyID AND DesignID=_DesignID AND ParameterName like CONCAT(_ChildAlias, '.%')
  );
END; $$


DROP PROCEDURE IF EXISTS pp_DeleteOwnerRelationForCustomGI $$
CREATE PROCEDURE pp_DeleteOwnerRelationForCustomGI (
  _MinCompanyID int,
  _DesignID nvarchar(36),
  _ParentAlias nvarchar(128),
  _ParentField nvarchar(128),
  _ChildAlias nvarchar(128),
  _ChildField nvarchar(128),
  _DisplayChildField nvarchar(128)
)
BEGIN
  UPDATE GIResult 
  SET ObjectName=_ParentAlias, Field=_ParentField
  WHERE CompanyID>=_MinCompanyID AND DesignID=_DesignID AND ObjectName=_ChildAlias AND Field=_DisplayChildField
    AND pp_OwnerUpdateGIConditions(GIResult.CompanyID, _DesignID, _ParentAlias, _ParentField, _ChildAlias, _ChildField, _DisplayChildField);
  
  DELETE FROM GIResult 
  WHERE CompanyID>=_MinCompanyID AND DesignID=_DesignID AND ObjectName=_ChildAlias AND Field<>_DisplayChildField
    AND pp_OwnerUpdateGIConditions(GIResult.CompanyID, _DesignID, _ParentAlias, _ParentField, _ChildAlias, _ChildField, _DisplayChildField);
  
  DELETE o
  FROM GIOn o
    LEFT JOIN GIRelation r ON r.CompanyID=o.CompanyID AND o.DesignID=r.DesignID AND o.RelationNbr=r.LineNbr
  WHERE o.CompanyID >= _MinCompanyID
    AND o.DesignID=_DesignID
    AND r.ParentTable=_ParentAlias AND r.ChildTable=_ChildAlias
    AND o.ParentField=_ParentField AND o.ChildField=_ChildField
    AND pp_OwnerUpdateGIConditions(o.CompanyID, _DesignID, _ParentAlias, _ParentField, _ChildAlias, _ChildField, _DisplayChildField);
  
  DELETE r
  FROM GIRelation r
  WHERE r.CompanyID >= _MinCompanyID
    AND r.DesignID=_DesignID
    AND r.ParentTable=_ParentAlias AND r.ChildTable=_ChildAlias
    AND pp_OwnerUpdateGIConditions(r.CompanyID, _DesignID, _ParentAlias, _ParentField, _ChildAlias, _ChildField, _DisplayChildField);
  
  DELETE FROM GISort 
  WHERE CompanyID>=_MinCompanyID AND DesignID=_DesignID AND DataFieldName like CONCAT(_ChildAlias, '.%')
    AND pp_OwnerUpdateGIConditions(GISort.CompanyID, _DesignID, _ParentAlias, _ParentField, _ChildAlias, _ChildField, _DisplayChildField);
  
  DELETE FROM GITable 
  WHERE CompanyID>=_MinCompanyID AND DesignID=_DesignID AND Alias=_ChildAlias
    AND pp_OwnerUpdateGIConditions(GITable.CompanyID, _DesignID, _ParentAlias, _ParentField, _ChildAlias, _ChildField, _DisplayChildField);
END; $$