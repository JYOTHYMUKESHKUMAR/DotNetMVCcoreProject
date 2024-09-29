using Microsoft.EntityFrameworkCore.Migrations;

public partial class AddCashFlowSummaryView : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
            CREATE VIEW CashFlowSummary AS
            WITH CashInSummary AS (
                SELECT
                    ISNULL(CI.DelayedDate, CI.Date) AS TransactionDate,
                    SUM(CI.AmountDue) AS CashIn,
                    0 AS CashOut
                FROM
                    CashIn CI
                WHERE
                    CI.Status IN ('Received', 'Scheduled')
                GROUP BY
                    ISNULL(CI.DelayedDate, CI.Date)
                
                UNION ALL
                
                SELECT
                    ISNULL(CII.DueDate, CI.Date) AS TransactionDate,
                    SUM(CII.Amount) AS CashIn,
                    0 AS CashOut
                FROM
                    CashInInstallment CII
                JOIN
                    CashIn CI ON CII.CashInId = CI.Id
                WHERE
                    CI.Status IN ('Received', 'Scheduled')
                GROUP BY
                    ISNULL(CII.DueDate, CI.Date)
            ),
            CashOutSummary AS (
                SELECT
                    ISNULL(CO.DelayedDate, CO.Date) AS TransactionDate,
                    0 AS CashIn,
                    SUM(CO.AmountDue) AS CashOut
                FROM
                    CashOut CO
                WHERE
                    CO.Status IN ('Received', 'Scheduled')
                GROUP BY
                    ISNULL(CO.DelayedDate, CO.Date)
                
                UNION ALL
                
                SELECT
                    ISNULL(COI.DueDate, CO.Date) AS TransactionDate,
                    0 AS CashIn,
                    SUM(COI.Amount) AS CashOut
                FROM
                    CashOutInstallment COI
                JOIN
                    CashOut CO ON COI.CashOutId = CO.Id
                WHERE
                    CO.Status IN ('Received', 'Scheduled')
                GROUP BY
                    ISNULL(COI.DueDate, CO.Date)
            ),
            CombinedSummary AS (
                SELECT
                    TransactionDate,
                    SUM(CashIn) AS TotalCashIn,
                    SUM(CashOut) AS TotalCashOut
                FROM (
                    SELECT * FROM CashInSummary
                    UNION ALL
                    SELECT * FROM CashOutSummary
                ) AS Combined
                GROUP BY
                    TransactionDate
            )
            SELECT
                TransactionDate AS [Date],
                TotalCashIn AS [CashIn],
                TotalCashOut AS [CashOut],
                SUM(TotalCashIn - TotalCashOut) OVER (ORDER BY TransactionDate) AS [Balance]
            FROM
                CombinedSummary
        ");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("DROP VIEW CashFlowSummary");
    }
}