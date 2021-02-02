--Tax Period Closing by Branch
---------------------------------------------------------------------------------
--TAX PERIOD 
--[mysql: Skip]
--[OldHash(Hash = 166efe0416e4b898063a8bd23b0f0c4192aa600bec5e518fba284f3ecc3b5df4)]
--[OldHash(Hash = 48c78f20e86953d349172aa190de0c0a2d5117197ed02630024e7fb437c15f01)]
--[MinVersion(Hash = b6cc905c4e1c2145f11ee255344ebde1d5f800aecf1e4318c3f424b3f3fc7c42)]
ALTER TABLE TaxPeriod
DROP TaxYear_TaxPeriod_FK1;
GO

--[mssql: Skip]
--[mysql: Native]
--[OldHash(Hash = 166efe0416e4b898063a8bd23b0f0c4192aa600bec5e518fba284f3ecc3b5df4)]
--[OldHash(Hash = 48c78f20e86953d349172aa190de0c0a2d5117197ed02630024e7fb437c15f01)]
--[MinVersion(Hash = bcb9e197f3c733db4e73be5f7deae2e90095180c92953c3a2a861553130ae957)]
SET FOREIGN_KEY_CHECKS=0;
GO

--[OldHash(Hash = 166efe0416e4b898063a8bd23b0f0c4192aa600bec5e518fba284f3ecc3b5df4)]
--[OldHash(Hash = 48c78f20e86953d349172aa190de0c0a2d5117197ed02630024e7fb437c15f01)]
--[OldHash(Hash = 855074731174b24884e97842293c2723cf0f515856b6411da9905f3e824cc840)]
--[OldHash(Hash = c4dcec616277697ec8c435b0b02e373fcee26506dc22bd24f8cfd15bb59e24e5)]
--[MinVersion(Hash = 0b400725a8d6ae7536f231b227a26d8ff22dff850e71cc4ad63d68b696ea10f5)]
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
--[MinVersion(Hash = fbd1b87f086be8354a7d450d9f6f4c8125c31de9637e2d9d0443b062826d29c5)]
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
--[MinVersion(Hash = 1108856f389ecc486f58d5bbb37ba8d3516a67bd6c16b4cc23827c5f4794c36e)]
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
--[MinVersion(Hash = b276d7eb9189efaa3e0f1834033467e88532c07edb6055e16ed7105c2493d1a1)]
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

--[MinVersion(Hash = 936b39cbab6631f5802a21516747ed5e845c2378fc9d39d6e6ae0ee103092c71)]
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
--[MinVersion(Hash = 5347ae3c10252d1171142e903b3464636d04eda7bbc1846ca2f55aeaf5a9d0a1)]
if exists(select * from INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS c
			where c.CONSTRAINT_NAME = 'TaxYear_TaxPeriod_FK1') 
begin
       exec sp_executesql N'ALTER TABLE TaxPeriod DROP [TaxYear_TaxPeriod_FK1]';
end
GO

--[mssql: Skip]
--[mysql: Native]
--[MinVersion(Hash = b6db0d6ccbcbe30557b9f88e12664f144f1aa6209bd05d40da62edbb8053afb0)]
SET FOREIGN_KEY_CHECKS = 0;
GO

--[OldHash(Hash = 45d253eb0360f27fbab2cef528d350511d08bc739f62e4ab81e2210f3911f34e)]
-- TODO: Remove attributes after AC-101276
--[mysql: Skip]
--[MinVersion(Hash = ea87b988bd646d2727300324444b900a7e8d7912e57dfc48bd930d9e7d3ba4b2)]
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
--[MinVersion(Hash = d5c9d41b1a0ab6b8beb7a8bebfd1275267171538ed77bbe956a57f7f49db6086)]
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
--[MinVersion(Hash = f93833001ede903ea975a9a2461f11dd4f163e4a186b212b8b99a41b4457ffaa)]
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
--[MinVersion(Hash = 6e908e1f4a6570590c8d787924f5767ce9098a4aedaa1e4e6f106403b760ae70)]
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
--[MinVersion(Hash = dca26f30e596561d591e2dbfd3a37ca989c25d02027dd6bf161bc24f52cb5296)]
ALTER TABLE [TaxPeriod]  WITH NOCHECK ADD  CONSTRAINT TaxYear_TaxPeriod_FK1 FOREIGN KEY(CompanyID, OrganizationID, VendorID, TaxYear)
REFERENCES TaxYear (CompanyID, OrganizationID, VendorID, Year);

ALTER TABLE [TaxPeriod] CHECK CONSTRAINT TaxYear_TaxPeriod_FK1;
GO

--[mssql: Skip]
--[mysql: Native]
--[MinVersion(Hash = 24800e14a8d5dddaa3bf1f299ff1c6110e746e114e04fd8983579be5fd68cc9b)]
SET FOREIGN_KEY_CHECKS = 1;
GO

--[OldHash(Hash = 8dcc456ab47c726ab23f6cb5b6dcf3b96aacf2df048cd1afee59614032033894)]
-- TODO: Remove attributes after AC-101276       
--[mysql: Skip]
--[MinVersion(Hash = a10a8873ddb029ea9556d910fd4757e38de8e25c0e4cc62096381895f6940387)]
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
--[MinVersion(Hash = 2ebd509c340cded1b301a114140438e3f325d5065e700ac2113997aff0fd589a)]
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
--[MinVersion(Hash = 67cdbdd7fdaa9145632d77eac837797e698ed79aa3b030d20fab3b2122ce2c38)]
DELETE b
FROM Branch b
INNER JOIN BAccount ba
    ON (b.BranchCD = ba.AcctCD
        AND b.CompanyID = ba.CompanyID
        AND ba.Type = 'OR');
GO

--[MinVersion(Hash = 31edc517a034b9008f2fe482d9a3d036ceea6ebab342227bceec488ab5754dc2)]
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
--[MinVersion(Hash = e6bd7ba5fdfdaf07edaf974591696bfd54e56b846c9512c1f158c478f3da1a12)]
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

--[OldHash(Hash = e8ac9ed1b1631c69f21fbb3009419886d6768b626821c962f59b6fcf3538b202)]
-- AC-36710: Clone calendars (years and periods) for all organizations
-- Create the full organization years for the all organizations
--[MinVersion(Hash = 5b7d8c4d5da536615e62091555943706eb0e0bd94969c86aa73155ef86a13bcf)]
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
	LastModifiedDateTime,
	NoteID
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
	y.LastModifiedDateTime,
	NEWID()
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
GO

--[OldHash(Hash = 881020fcbdcfdb4a0170ec7420c45eb415a7b43702549e2a7386ea15e087afbc)]
-- Copy organization period from base periods
--[MinVersion(Hash = 386c6a26ff698b7dceaeb7b6dedab91a0f5b3f0c3a669cbd6449190da5cdbbb8)]
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
	OrganizationID,
	NoteID
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
	o.OrganizationID,
	NEWID()
from FinPeriod p
inner join Organization o
	on p.CompanyID = o.CompanyID
where p.OrganizationID = 0
	and o.DeletedDatabaseRecord = 0;
GO

--[MinVersion(Hash = 78d07a60507f4f499ee266cf071f784098c748cadcecb84e4101d7e5f9e2d79b)]
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
GO

--[MinVersion(Hash = b6afea2cf93a0986b1d173a1115277bd9b046f1e3998f88e3a3a275cab7ea129)]
INSERT INTO NumberingSequence (CompanyID, NumberingID, StartNbr, EndNbr, StartDate, LastNbr,
WarnNbr, NbrStep, CreatedByID, CreatedByScreenID, CreatedDateTime, LastModifiedByID, LastModifiedByScreenID, LastModifiedDateTime, NumberingSeq)
SELECT r.CompanyID, 'EPRECEIPT', '000000', '999999', '1900-01-01 00:00:00', MAX(RIGHT('000000' + CAST(ClaimDetailID AS NVARCHAR(6)), 6)),
'999990', 1, 'b5344897-037e-4d58-b5c3-1bdfd0f47bf9', 'CS201010', GETDATE(), 'b5344897-037e-4d58-b5c3-1bdfd0f47bf9', 'CS201010', GETDATE(), '10012'
FROM EPExpenseClaimDetails AS r LEFT JOIN NumberingSequence AS n ON r.CompanyID = n.CompanyID AND n.NumberingID = 'EPRECEIPT'
WHERE n.CompanyID IS NULL
GROUP BY r.CompanyID
GO

--[MinVersion(Hash = 7fdee48159606f75d6baa2e6910f641597c66781a985f4106d110ad8459f1de2)]
UPDATE Numbering SET NoteID = NEWID() WHERE NumberingID ='EPRECEIPT' and NoteID IS NULL
GO

--[mysql: Native]
--[mssql: Skip]
--[MinVersion(Hash = 2537c26b13217257b54deb3acf0e1885e99a17626676b347a435cab724254dec)]
INSERT NumberingKvExt (CompanyID, RecordID, FieldName, ValueString)
SELECT l.CompanyID, t.NoteID, 'NewSymbol' + MAX(UPPER(SUBSTRING(l.LocaleName, 1, 2))), MAX(NewSymbol)
FROM Numbering as t join Locale as l 
LEFT JOIN NumberingKvExt e ON e.CompanyID = l.CompanyID AND e.RecordID = t.NoteID AND e.FieldName = 'NewSymbol' + UPPER(SUBSTRING(l.LocaleName, 1, 2))
WHERE e.CompanyID IS NULL and t.NumberingID ='EPRECEIPT'
GROUP BY l.CompanyID, t.NoteID
GO

--[mysql: Skip]
--[MinVersion(Hash = f04b7a2787617ce3589b0296f781ac187b82321ab4d987a72f1d8ad67c8deef2)]
INSERT NumberingKvExt (CompanyID, RecordID, FieldName, ValueString)
SELECT l.CompanyID, t.NoteID, 'NewSymbol' + MAX(UPPER(SUBSTRING(l.LocaleName, 1, 2))), MAX(NewSymbol)
FROM Numbering as t cross join Locale as l 
LEFT JOIN NumberingKvExt e ON e.CompanyID = l.CompanyID AND e.RecordID = t.NoteID AND e.FieldName = 'NewSymbol' + UPPER(SUBSTRING(l.LocaleName, 1, 2))
WHERE e.CompanyID IS NULL and t.NumberingID ='EPRECEIPT'
GROUP BY l.CompanyID, t.NoteID
GO

--[IfExists(Column = CROpportunityDiscountDetail.RecordID)]
--[MinVersion(Hash = f782c36c792f2413d5a15cc208ddcab9ed8a96ca5ea09a4f4c5c8e75d8cfba33)]
UPDATE CROpportunityDiscountDetail SET LineNbr = 0 WHERE LineNbr IS NULL
GO

-- AC-96903 Shifted company calendars
-- Copy FA master years into organizations
--[SmartExecute]
--[MinVersion(Hash = 7c896e238cdd0866109cbffe8c34fbfba687d4ce950cb94439bf2d3abe0d2b09)]
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
GO

-- end AC-96903
-- AC-87365: Copy values from INTranCost.CreatedDateTime to INTran.ReleasedDateTime
--[OldHash(Hash = 8a5faa34f095716d168db8bdeae0e60f438c719460fc33836de522f9b87ec74b)]
--[MinVersion(Hash = 8a5faa34f095716d168db8bdeae0e60f438c719460fc33836de522f9b87ec74b)]
update tr
set ReleasedDateTime = isnull(cost.CreatedDateTime, tr.LastModifiedDateTime)
from INTran tr
	left join INTranCost cost on tr.CompanyID = cost.CompanyID and tr.DocType = cost.DocType and tr.RefNbr = cost.RefNbr and tr.LineNbr = cost.LineNbr
		and cost.DocType = cost.CostDocType and cost.RefNbr = cost.CostRefNbr
where 
	tr.Released = 1
	and tr.ReleasedDateTime is null
GO

-- AC-87365: Copy values from INTranCost.CreatedDateTime to INTranSplit.ReleasedDateTime
--[OldHash(Hash = 557e31f61b21bad5f80046141cb9914c57ee1d5e783a3bcf9dc6a55768dab36b)]
--[MinVersion(Hash = 557e31f61b21bad5f80046141cb9914c57ee1d5e783a3bcf9dc6a55768dab36b)]
update tr
set ReleasedDateTime = isnull(cost.CreatedDateTime, tr.LastModifiedDateTime)
from INTranSplit tr
	left join INTranCost cost on tr.CompanyID = cost.CompanyID and tr.DocType = cost.DocType and tr.RefNbr = cost.RefNbr and tr.LineNbr = cost.LineNbr
		and cost.DocType = cost.CostDocType and cost.RefNbr = cost.CostRefNbr
where 
	tr.Released = 1
	and tr.ReleasedDateTime is null
GO

