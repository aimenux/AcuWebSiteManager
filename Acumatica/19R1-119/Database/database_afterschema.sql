--Tax Period Closing by Branch
---------------------------------------------------------------------------------
--TAX PERIOD 
--[mysql: Skip]
--[OldHash(Hash = 166efe0416e4b898063a8bd23b0f0c4192aa600bec5e518fba284f3ecc3b5df4)]
--[OldHash(Hash = 48c78f20e86953d349172aa190de0c0a2d5117197ed02630024e7fb437c15f01)]
ALTER TABLE TaxPeriod
DROP TaxYear_TaxPeriod_FK1;
GO

--[mssql: Skip]
--[mysql: Native]
--[OldHash(Hash = 166efe0416e4b898063a8bd23b0f0c4192aa600bec5e518fba284f3ecc3b5df4)]
--[OldHash(Hash = 48c78f20e86953d349172aa190de0c0a2d5117197ed02630024e7fb437c15f01)]
SET FOREIGN_KEY_CHECKS=0;
GO

--[OldHash(Hash = 166efe0416e4b898063a8bd23b0f0c4192aa600bec5e518fba284f3ecc3b5df4)]
--[OldHash(Hash = 48c78f20e86953d349172aa190de0c0a2d5117197ed02630024e7fb437c15f01)]
--[OldHash(Hash = 855074731174b24884e97842293c2723cf0f515856b6411da9905f3e824cc840)]
--[OldHash(Hash = c4dcec616277697ec8c435b0b02e373fcee26506dc22bd24f8cfd15bb59e24e5)]    
INSERT INTO TaxPeriod(CompanyID, VendorID, TaxPeriodID, TaxYear, StartDate, EndDate, Filed, Status, BranchID, OrganizationID)
SELECT	TaxPeriod.CompanyID,
		TaxPeriod.VendorID,
		TaxPeriod.TaxPeriodID,
		TaxPeriod.TaxYear,
		TaxPeriod.StartDate,
		TaxPeriod.EndDate,
		TaxPeriod.Filed,
		TaxPeriod.Status,
		Branch.BranchID,
    -Branch.BranchID
FROM Branch Branch
INNER JOIN Ledger Ledger ON (Ledger.CompanyID = Branch.CompanyID)
	AND Ledger.DeletedDatabaseRecord = 0
	AND (Ledger.LedgerID = Branch.LedgerID)
	INNER JOIN TaxPeriod
			ON (Branch.CompanyID = TaxPeriod.CompanyID)
WHERE Branch.DeletedDatabaseRecord = 0
	AND (Ledger.DefBranchID IS NULL
		OR  Ledger.DefBranchID = Branch.BranchID
		);

DELETE FROM TaxPeriod
		WHERE BranchID = 0;
    
DELETE FROM TaxPeriod
		WHERE OrganizationID = 0;    

UPDATE TaxPeriod 
		SET Status = CASE 
						WHEN EXISTS(SELECT * FROM TaxHistory TaxHistory
                          INNER JOIN Branch Branch   
                          ON (TaxHistory.CompanyID = Branch.CompanyID
								                AND TaxHistory.BranchID = Branch.BranchID)
            							WHERE TaxHistory.VendorID = TaxPeriod.VendorID
            									AND Branch.ParentBranchID = TaxPeriod.BranchID
            									AND TaxHistory.TaxPeriodID = TaxPeriod.TaxPeriodID
                      )
							THEN 'P'
						ELSE 'N'
					 END
		WHERE Status = 'P';
    
GO
----------------------------------------
--TAX YEAR
--[OldHash(Hash = 166efe0416e4b898063a8bd23b0f0c4192aa600bec5e518fba284f3ecc3b5df4)]
--[OldHash(Hash = 48c78f20e86953d349172aa190de0c0a2d5117197ed02630024e7fb437c15f01)]
--[OldHash(Hash = bd7a1bf8906ccd4ce031f8eff5a1c76dad5b6d0bbb5b0109daa34ef937cb0d96)]    
--[OldHash(Hash = 215a549f6cc22fa82efdb580d0520d5a20872edd80b22f012b91a8e90ac8d0cc)]   
INSERT INTO TaxYear(CompanyID, VendorID, Year, StartDate, Filed, TaxPeriodType, BranchID, OrganizationID)
SELECT	TaxYear.CompanyID,
		TaxYear.VendorID,
		TaxYear.Year,
		TaxYear.StartDate,
		TaxYear.Filed,
		TaxYear.TaxPeriodType,
		Branch.BranchID,
    -Branch.BranchID   
FROM Branch Branch
INNER JOIN Ledger Ledger ON (Ledger.CompanyID = Branch.CompanyID)
	AND Ledger.DeletedDatabaseRecord = 0
	AND (Ledger.LedgerID = Branch.LedgerID)
INNER JOIN TaxYear
			ON (Branch.CompanyID = TaxYear.CompanyID)
WHERE Branch.DeletedDatabaseRecord = 0
	AND (Ledger.DefBranchID IS NULL
		OR  Ledger.DefBranchID = Branch.BranchID
		);

DELETE FROM TaxYear
		WHERE BranchID = 0;

DELETE FROM TaxYear
		WHERE OrganizationID = 0;

GO

--[OldHash(Hash = 166efe0416e4b898063a8bd23b0f0c4192aa600bec5e518fba284f3ecc3b5df4)]
--[OldHash(Hash = 48c78f20e86953d349172aa190de0c0a2d5117197ed02630024e7fb437c15f01)]
UPDATE TaxYear
		SET PlanPeriodsCount = (SELECT COUNT(*) 
												FROM  TaxPeriod
												WHERE TaxPeriod.BranchID = TaxYear.BranchID
														AND TaxPeriod.CompanyID = TaxYear.CompanyID
                            AND TaxPeriod.VendorID = TaxYear.VendorID
														AND TaxPeriod.TaxYear = TaxYear.Year)
			, PeriodsCount = (SELECT COUNT(*) 
												FROM  TaxPeriod
												WHERE TaxPeriod.BranchID = TaxYear.BranchID
														AND TaxPeriod.CompanyID = TaxYear.CompanyID
                            AND TaxPeriod.VendorID = TaxYear.VendorID
														AND TaxPeriod.TaxYear = TaxYear.Year);

GO                            
-------------------------------------
--1099 YEAR
--[OldHash(Hash = b9a34da92cd7d143fc1300688c8aeb38347e2b2f37522eb3bd361aab3a8f213f)]          
--[OldHash(Hash = 93c485a446c688b8e0fbb939a6e371d0397a2cd54f53d8fc7f28779d59502086)]  
--[OldHash(Hash = 3de0d6217a1d2f73610efedf7daa884d286b40d92da9bb01d26eb3610ed97e2d)]   
INSERT INTO AP1099Year(CompanyID, Status, FinYear, BranchID, OrganizationID)
SELECT	AP1099Year.CompanyID,
		AP1099Year.Status,
		AP1099Year.FinYear,
		Branch.BranchID,
    -Branch.BranchID
FROM Branch Branch
INNER JOIN Ledger Ledger ON (Ledger.CompanyID = Branch.CompanyID)
	AND Ledger.DeletedDatabaseRecord = 0
	AND (Ledger.LedgerID = Branch.LedgerID)
	INNER JOIN AP1099Year
			ON (Branch.CompanyID = AP1099Year.CompanyID)
WHERE Branch.DeletedDatabaseRecord = 0
	AND (Ledger.DefBranchID IS NULL
		OR  Ledger.DefBranchID = Branch.BranchID
		);

DELETE FROM AP1099Year
		WHERE BranchID = 0;

DELETE FROM AP1099Year
		WHERE OrganizationID = 0;

GO


INSERT INTO NumberingSequence (CompanyID, NumberingID, StartNbr, EndNbr, StartDate, LastNbr,
WarnNbr, NbrStep, CreatedByID, CreatedByScreenID, CreatedDateTime, LastModifiedByID, LastModifiedByScreenID, LastModifiedDateTime, NumberingSeq)
SELECT d.CompanyID, 'DRSCHEDULE', '00000000', '99999999', '1900-01-01 00:00:00', MAX(RIGHT('00000000' + CAST(ScheduleID AS NVARCHAR(8)), 8)),
'99999899', 1, 'b5344897-037e-4d58-b5c3-1bdfd0f47bf9', 'CS201010', GETDATE(), 'b5344897-037e-4d58-b5c3-1bdfd0f47bf9', 'CS201010', GETDATE(), '2480'
FROM DRSchedule AS d LEFT JOIN NumberingSequence AS n ON d.CompanyID = n.CompanyID AND n.NumberingID = 'DRSCHEDULE'
WHERE n.CompanyID IS NULL
GROUP BY d.CompanyID

GO

--TAX PERIOD 
--[mysql: Skip]
if exists(select * from INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS c
			where c.CONSTRAINT_NAME = 'TaxYear_TaxPeriod_FK1') 
begin
       exec sp_executesql N'ALTER TABLE TaxPeriod DROP [TaxYear_TaxPeriod_FK1]';
end
GO

--[mssql: Skip]
--[mysql: Native]
SET FOREIGN_KEY_CHECKS = 0;
GO

--[OldHash(Hash = 45d253eb0360f27fbab2cef528d350511d08bc739f62e4ab81e2210f3911f34e)] 
-- TODO: Remove attributes after AC-101276
--[mysql: Skip]
UPDATE tyear
SET tyear.OrganizationID = o.OrganizationID
FROM TaxYear tyear
	JOIN Branch b
		ON (tyear.BranchID = b.BranchID
			and tyear.CompanyID = b.CompanyID)
	JOIN Organization o
		ON (b.BranchID = o.ConvertedFromBranchID
			and b.CompanyID = o.CompanyID);
GO

--[OldHash(Hash = ed04d4b5629d84a5407060e8e58f706300ba528a1a507b4384c96e2756fc3612)] 
-- TODO: Remove statement after AC-101276   
--[mssql: Skip]
--[mysql: Native]
UPDATE TaxYear tyear
	JOIN Branch b
		ON (tyear.BranchID = b.BranchID
			and tyear.CompanyID = b.CompanyID)
	JOIN Organization o
		ON (b.BranchID = o.ConvertedFromBranchID
			and b.CompanyID = o.CompanyID)
SET tyear.OrganizationID = o.OrganizationID;
GO

--[OldHash(Hash = 49336b35fb1dfa317864f23e1657b2c6c9c4a9376668d5a45f25d4c1d8bf6ea2)]
-- TODO: Remove attributes after AC-101276  
--[mysql: Skip]
UPDATE period
SET period.OrganizationID = o.OrganizationID
FROM TaxPeriod period
	JOIN Branch b
		ON (period.BranchID = b.BranchID
			and period.CompanyID = b.CompanyID)
	JOIN Organization o
		ON (b.BranchID = o.ConvertedFromBranchID
			and b.CompanyID = o.CompanyID);
GO

--[OldHash(Hash = ec161eb6f1ce802125225657f011f2062b31044dae329f3c4375e4ac8096360d)]
-- TODO: Remove statement after AC-101276   
--[mssql: Skip]
--[mysql: Native]
UPDATE TaxPeriod period
	JOIN Branch b
		ON (period.BranchID = b.BranchID
			and period.CompanyID = b.CompanyID)
	JOIN Organization o
		ON (b.BranchID = o.ConvertedFromBranchID
			and b.CompanyID = o.CompanyID)
SET period.OrganizationID = o.OrganizationID;
GO

--[mysql: Skip]
ALTER TABLE [TaxPeriod]  WITH NOCHECK ADD  CONSTRAINT TaxYear_TaxPeriod_FK1 FOREIGN KEY(CompanyID, OrganizationID, VendorID, TaxYear)
REFERENCES TaxYear (CompanyID, OrganizationID, VendorID, Year);

ALTER TABLE [TaxPeriod] CHECK CONSTRAINT TaxYear_TaxPeriod_FK1;
GO

--[mssql: Skip]
--[mysql: Native]
SET FOREIGN_KEY_CHECKS = 1;
GO

--[OldHash(Hash = 8dcc456ab47c726ab23f6cb5b6dcf3b96aacf2df048cd1afee59614032033894)]
-- TODO: Remove attributes after AC-101276       
--[mysql: Skip]
UPDATE apyear
SET apyear.OrganizationID = o.OrganizationID
FROM AP1099Year apyear
	JOIN Branch b
		ON (apyear.BranchID = b.BranchID
			and apyear.CompanyID = b.CompanyID)
	JOIN Organization o
		ON (b.BranchID = o.ConvertedFromBranchID
			and b.CompanyID = o.CompanyID);
GO

--[OldHash(Hash = 8c649d446b6fd80d70ac8028d40627d41d117ff1daafae951819c964d6817cdf)]      
-- TODO: Remove statement after AC-101276    
--[mssql: Skip]
--[mysql: Native]
UPDATE AP1099Year apyear
	JOIN Branch b
		ON (apyear.BranchID = b.BranchID
			and apyear.CompanyID = b.CompanyID)
	JOIN Organization o
		ON (b.BranchID = o.ConvertedFromBranchID
			and b.CompanyID = o.CompanyID)
SET apyear.OrganizationID = o.OrganizationID;
GO
      
-- We should remove Branch part for all Consol. Branches, 
-- that have been converted to Organization entity.
--
DELETE b
FROM Branch b
INNER JOIN BAccount ba
    ON (b.BranchCD = ba.AcctCD
        AND b.CompanyID = ba.CompanyID
        AND ba.Type = 'OR');
GO

UPDATE AP1099Year
SET BranchID = null;

UPDATE TaxPeriod
SET BranchID = null;

UPDATE TaxYear
SET BranchID = null;
GO

-- DR scripts had moved here because new columns PendingExpenseValidate and
-- PendingRevenueValidate in the DRSetup can't be updated in
-- the AppUpdates.sql

-- Set PendingExpenseValidate if there are records in the DRExpenseBalance
-- and the company has more than one branch
UPDATE DRSetup
SET PendingExpenseValidate = 1
FROM DRSetup
	INNER JOIN 
		( 
			SELECT CompanyID, COUNT(*) AS BranchCount 
			FROM Branch 
			WHERE DeletedDatabaseRecord = 0 
			GROUP BY CompanyID
		) AS CompanyBranches 
		ON DRSetup.CompanyID = CompanyBranches.CompanyID
	INNER JOIN 
		(
			SELECT DISTINCT DRExpenseBalance.CompanyID 
			FROM DRExpenseBalance
		) AS Companies
		ON DRSetup.CompanyID = Companies.CompanyID
WHERE PendingExpenseValidate = 0
	AND CompanyBranches.BranchCount > 1

-- Set PendingRevenueValidate if there are records in the DRRevenueBalance
-- and the company has more than one branch
UPDATE DRSetup
SET PendingRevenueValidate = 1
FROM DRSetup
	INNER JOIN 
		( 
			SELECT CompanyID, COUNT(*) AS BranchCount 
			FROM Branch 
			WHERE DeletedDatabaseRecord = 0 
			GROUP BY CompanyID
		) AS CompanyBranches 
		ON DRSetup.CompanyID = CompanyBranches.CompanyID
	INNER JOIN 
		(
			SELECT DISTINCT DRRevenueBalance.CompanyID 
			FROM DRRevenueBalance
		) AS Companies
		ON DRSetup.CompanyID = Companies.CompanyID
WHERE PendingRevenueValidate = 0
	AND CompanyBranches.BranchCount > 1

-- Set PendingExpenseValidate if there are records in the DRExpenseProjection
-- and the company has more than one branch
UPDATE DRSetup
SET PendingExpenseValidate = 1
FROM DRSetup
	INNER JOIN 
		( 
			SELECT CompanyID, COUNT(*) AS BranchCount 
			FROM Branch 
			WHERE DeletedDatabaseRecord = 0 
			GROUP BY CompanyID
		) AS CompanyBranches 
		ON DRSetup.CompanyID = CompanyBranches.CompanyID
	INNER JOIN 
		(
			SELECT DISTINCT DRExpenseProjection.CompanyID 
			FROM DRExpenseProjection
		) AS Companies
		ON DRSetup.CompanyID = Companies.CompanyID
WHERE PendingExpenseValidate = 0
	AND CompanyBranches.BranchCount > 1

-- Set PendingRevenueValidate if there are records in the DRRevenueProjection
-- and the company has more than one branch
UPDATE DRSetup
SET PendingRevenueValidate = 1
FROM DRSetup
	INNER JOIN 
		( 
			SELECT CompanyID, COUNT(*) AS BranchCount 
			FROM Branch 
			WHERE DeletedDatabaseRecord = 0 
			GROUP BY CompanyID
		) AS CompanyBranches 
		ON DRSetup.CompanyID = CompanyBranches.CompanyID
	INNER JOIN 
		(
			SELECT DISTINCT DRRevenueProjection.CompanyID 
			FROM DRRevenueProjection
		) AS Companies
		ON DRSetup.CompanyID = Companies.CompanyID
WHERE PendingRevenueValidate = 0
	AND CompanyBranches.BranchCount > 1
	
-- clean the balance and projection tables for the companies with more than one branch
DELETE DRExpenseBalance 
WHERE
   (
      SELECT COUNT(*) 
      FROM Branch 
      WHERE Branch.CompanyID = DRExpenseBalance.CompanyID
         AND DeletedDatabaseRecord = 0 
   ) > 1 
   AND BranchID = 0

DELETE DRRevenueBalance 
WHERE
   (
      SELECT COUNT(*) 
      FROM Branch 
      WHERE Branch.CompanyID = DRRevenueBalance.CompanyID
         AND DeletedDatabaseRecord = 0 
   ) > 1 
   AND BranchID = 0

DELETE DRExpenseProjection 
WHERE
   (
      SELECT COUNT(*) 
      FROM Branch 
      WHERE Branch.CompanyID = DRExpenseProjection.CompanyID
         AND DeletedDatabaseRecord = 0 
   ) > 1 
   AND BranchID = 0

DELETE DRRevenueProjection 
WHERE
   (
      SELECT COUNT(*) 
      FROM Branch 
      WHERE Branch.CompanyID = DRRevenueProjection.CompanyID 
         AND DeletedDatabaseRecord = 0 
   ) > 1 
   AND BranchID = 0
GO
   
-- AC-36710: Clone calendars (years and periods) for all organizations

-- Create the full organization years for the all organizations

insert FinYear (
	CompanyID,
	OrganizationID,
	Year,
	StartMasterFinPeriodID,
	FinPeriods,
	StartDate,
	EndDate,
	CreatedByID, 
	CreatedByScreenID, 
	CreatedDateTime, 
	LastModifiedByID, 
	LastModifiedByScreenID, 
	LastModifiedDateTime
)
select
	y.CompanyID,
	o.OrganizationID,
	y.Year,
	p.StartPeriodID,
	y.FinPeriods,
	y.StartDate,
	y.EndDate,
	y.CreatedByID, 
	y.CreatedByScreenID, 
	y.CreatedDateTime, 
	y.LastModifiedByID, 
	y.LastModifiedByScreenID, 
	y.LastModifiedDateTime
from FinYear y
inner join Organization o
	on y.CompanyID = o.CompanyID
left join 
	(select
		CompanyID,
		FinYear,
		min(FinPeriodID) StartPeriodID
			from FinPeriod 
		where OrganizationID = 0
		group by CompanyID, FinYear) p
	on y.CompanyID = p.CompanyID
		and y.Year = p.FinYear
where o.DeletedDatabaseRecord = 0;
go

-- Copy organization period from base periods

insert FinPeriod (
	CompanyID, 
	FinPeriodID, 
	MasterFinPeriodID, 
	Descr, 
	Status,
	Closed, 
	APClosed, 
	ARClosed, 
	INClosed,
	CAClosed, 
	FAClosed, 
	DateLocked,
	Active,
	FinYear,
	PeriodNbr,
	Custom,
	StartDate,
	EndDate,
	CreatedByID, 
	CreatedByScreenID, 
	CreatedDateTime, 
	LastModifiedByID, 
	LastModifiedByScreenID, 
	LastModifiedDateTime,
	OrganizationID
)
select 
	p.CompanyID, 
	p.FinPeriodID, 
	p.FinPeriodID, 
	p.Descr, 
	'',
	p.Closed, 
	p.APClosed, 
	p.ARClosed, 
	p.INClosed, 
	p.CAClosed, 
	p.FAClosed, 
	p.DateLocked,
	p.Active,
	p.FinYear,
	p.PeriodNbr,
	p.Custom,
	p.StartDate,
	p.EndDate,
	p.CreatedByID, 
	p.CreatedByScreenID, 
	p.CreatedDateTime, 
	p.LastModifiedByID, 
	p.LastModifiedByScreenID, 
	p.LastModifiedDateTime,
	o.OrganizationID
from FinPeriod p
inner join Organization o
	on p.CompanyID = o.CompanyID
where p.OrganizationID = 0
	and o.DeletedDatabaseRecord = 0;
go

update p
set p.Status = 'Locked'
from FinPeriod p
inner join (select 
		CompanyID,
		OrganizationID, 
		max(FinPeriodID) LastLockedFinPeriodID
	from FinPeriod
	where Status = ''
		and Active = 0
		and Closed = 1
	group by 
		CompanyID,
		OrganizationID) lp
on p.CompanyID = lp.CompanyID
	and p.OrganizationID = lp.OrganizationID
	and p.FinperiodID <= lp.LastLockedFinPeriodID
where p.Status = '';

update FinPeriod
set Status = 'Closed'
where Status = ''
	and Active = 1
	and Closed = 1;

update p
set p.Status = 'Open'
from FinPeriod p
inner join (select 
	CompanyID,
	OrganizationID, 
	max(FinPeriodID) LastOpenFinPeriodID
from FinPeriod
where Status = ''
	and Active = 1
	and Closed = 0
group by 
	CompanyID,
	OrganizationID) lp
on p.CompanyID = lp.CompanyID
	and p.OrganizationID = lp.OrganizationID
	and p.FinperiodID <= lp.LastOpenFinPeriodID
where p.Status = '';

update FinPeriod
set Status = 'Inactive'
where Status = ''
	and Active = 0
	and Closed = 0;
go

INSERT INTO NumberingSequence (CompanyID, NumberingID, StartNbr, EndNbr, StartDate, LastNbr,
WarnNbr, NbrStep, CreatedByID, CreatedByScreenID, CreatedDateTime, LastModifiedByID, LastModifiedByScreenID, LastModifiedDateTime, NumberingSeq)
SELECT r.CompanyID, 'EPRECEIPT', '000000', '999999', '1900-01-01 00:00:00', MAX(RIGHT('000000' + CAST(ClaimDetailID AS NVARCHAR(6)), 6)),
'999990', 1, 'b5344897-037e-4d58-b5c3-1bdfd0f47bf9', 'CS201010', GETDATE(), 'b5344897-037e-4d58-b5c3-1bdfd0f47bf9', 'CS201010', GETDATE(), '10012'
FROM EPExpenseClaimDetails AS r LEFT JOIN NumberingSequence AS n ON r.CompanyID = n.CompanyID AND n.NumberingID = 'EPRECEIPT'
WHERE n.CompanyID IS NULL
GROUP BY r.CompanyID
GO

UPDATE Numbering SET NoteID = NEWID() WHERE NumberingID ='EPRECEIPT' and NoteID IS NULL
GO

--[mysql: Native]
--[mssql: Skip]
INSERT NumberingKvExt (CompanyID, RecordID, FieldName, ValueString)
SELECT l.CompanyID, t.NoteID, 'NewSymbol' + MAX(UPPER(SUBSTRING(l.LocaleName, 1, 2))), MAX(NewSymbol)
FROM Numbering as t join Locale as l 
LEFT JOIN NumberingKvExt e ON e.CompanyID = l.CompanyID AND e.RecordID = t.NoteID AND e.FieldName = 'NewSymbol' + UPPER(SUBSTRING(l.LocaleName, 1, 2))
WHERE e.CompanyID IS NULL and t.NumberingID ='EPRECEIPT'
GROUP BY l.CompanyID, t.NoteID
GO 

--[mysql: Skip]
INSERT NumberingKvExt (CompanyID, RecordID, FieldName, ValueString)
SELECT l.CompanyID, t.NoteID, 'NewSymbol' + MAX(UPPER(SUBSTRING(l.LocaleName, 1, 2))), MAX(NewSymbol)
FROM Numbering as t cross join Locale as l 
LEFT JOIN NumberingKvExt e ON e.CompanyID = l.CompanyID AND e.RecordID = t.NoteID AND e.FieldName = 'NewSymbol' + UPPER(SUBSTRING(l.LocaleName, 1, 2))
WHERE e.CompanyID IS NULL and t.NumberingID ='EPRECEIPT'
GROUP BY l.CompanyID, t.NoteID
GO 

--[IfExists(Column = CROpportunityDiscountDetail.RecordID)]
UPDATE CROpportunityDiscountDetail SET LineNbr = 0 WHERE LineNbr IS NULL
GO

-- AC-96903 Shifted company calendars

-- Copy FA master years into organizations
--[Smart]
insert into FABookYear
(
	CompanyID,
	BookID,
	OrganizationID,
	Year,
	StartDate,
	EndDate,
	FinPeriods,
	CreatedByID, 
	CreatedByScreenID, 
	CreatedDateTime, 
	LastModifiedByID, 
	LastModifiedByScreenID, 
	LastModifiedDateTime
)
select 
	y.CompanyID,
	y.BookID,
	o.OrganizationID,
	y.Year,
	y.StartDate,
	y.EndDate,
	y.FinPeriods,
	'00000000-0000-0000-0000-000000000000',
	'FA000000',
	'1900-01-01',
	'00000000-0000-0000-0000-000000000000',
	'FA000000',
	'1900-01-01'
from FABookYear y 
	inner join FABook b 
		on y.BookID = b.BookID 
			and b.UpdateGL = 1
	inner join Organization o 
		on o.DeletedDatabaseRecord = 0;

-- Copy FA master periods into organizations
--[Smart]
insert into FABookPeriod
(
	CompanyID, 
	BookID,
	OrganizationID,
	FinPeriodID, 
	Descr, 
	Closed, 
	DateLocked,
	Active,
	FinYear,
	PeriodNbr,
	StartDate,
	EndDate,
	CreatedByID, 
	CreatedByScreenID, 
	CreatedDateTime, 
	LastModifiedByID, 
	LastModifiedByScreenID, 
	LastModifiedDateTime
)
select 
	p.CompanyID, 
	p.BookID,
	o.OrganizationID,
	p.FinPeriodID, 
	p.Descr, 
	p.Closed, 
	p.DateLocked,
	p.Active,
	p.FinYear,
	p.PeriodNbr,
	p.StartDate,
	p.EndDate,
	'00000000-0000-0000-0000-000000000000',
	'FA000000',
	'1900-01-01',
	'00000000-0000-0000-0000-000000000000',
	'FA000000',
	'1900-01-01'
from FABookPeriod p 	
	inner join FABook b 
		on p.BookID = b.BookID 
			and b.UpdateGL = 1
	inner join Organization o 
		on o.DeletedDatabaseRecord = 0;

-- Fill master calendar references

update FABookYear
set StartMasterFinPeriodID = Year + '01'
where StartMasterFinPeriodID is null and
	(OrganizationID <> 0 or OrganizationID is not null);

update FABookPeriod
set MasterFinPeriodID = FinPeriodID
where MasterFinPeriodID is null and 
	(OrganizationID <> 0 or OrganizationID is not null);
go
-- end AC-96903