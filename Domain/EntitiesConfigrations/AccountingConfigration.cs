using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.EntitiesConfigrations;

public class AccountingSettingConfigration : IEntityTypeConfiguration<AccountingSetting>
{
    public void Configure(EntityTypeBuilder<AccountingSetting> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.FiscalYearName).IsRequired().HasMaxLength(120);
    }
}

public class FinanceBankAccountConfigration : IEntityTypeConfiguration<FinanceBankAccount>
{
    public void Configure(EntityTypeBuilder<FinanceBankAccount> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.BankName).IsRequired().HasMaxLength(160);
        entity.Property(x => x.AccountName).IsRequired().HasMaxLength(160);
        entity.Property(x => x.Iban).IsRequired().HasMaxLength(40);
        entity.Property(x => x.OpeningBalance).HasColumnType("decimal(18,2)");
        entity.HasIndex(x => x.Iban).IsUnique();
    }
}

public class FinanceAccountConfigration : IEntityTypeConfiguration<FinanceAccount>
{
    public void Configure(EntityTypeBuilder<FinanceAccount> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Code).IsRequired().HasMaxLength(40);
        entity.Property(x => x.NameAr).IsRequired().HasMaxLength(200);
        entity.HasIndex(x => x.Code).IsUnique();
        entity.HasOne(x => x.ParentAccount).WithMany().HasForeignKey(x => x.ParentAccountId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class FinanceCostCenterConfigration : IEntityTypeConfiguration<FinanceCostCenter>
{
    public void Configure(EntityTypeBuilder<FinanceCostCenter> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Code).IsRequired().HasMaxLength(40);
        entity.Property(x => x.NameAr).IsRequired().HasMaxLength(200);
        entity.HasIndex(x => x.Code).IsUnique();
    }
}

public class LedgerEntryConfigration : IEntityTypeConfiguration<LedgerEntry>
{
    public void Configure(EntityTypeBuilder<LedgerEntry> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.EntryNumber).IsRequired().HasMaxLength(60);
        entity.Property(x => x.Description).IsRequired().HasMaxLength(1000);
        entity.HasIndex(x => x.EntryNumber).IsUnique();
        entity.HasIndex(x => x.Status);
    }
}

public class LedgerLineConfigration : IEntityTypeConfiguration<LedgerLine>
{
    public void Configure(EntityTypeBuilder<LedgerLine> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Debit).HasColumnType("decimal(18,2)");
        entity.Property(x => x.Credit).HasColumnType("decimal(18,2)");
        entity.Property(x => x.Notes).HasMaxLength(1000);
        entity.HasOne(x => x.LedgerEntry).WithMany(x => x.Lines).HasForeignKey(x => x.LedgerEntryId).OnDelete(DeleteBehavior.Cascade);
        entity.HasOne(x => x.FinanceAccount).WithMany().HasForeignKey(x => x.FinanceAccountId).OnDelete(DeleteBehavior.Restrict);
        entity.HasOne(x => x.FinanceCostCenter).WithMany().HasForeignKey(x => x.FinanceCostCenterId).OnDelete(DeleteBehavior.SetNull);
    }
}

public class ReceiptVoucherConfigration : IEntityTypeConfiguration<ReceiptVoucher>
{
    public void Configure(EntityTypeBuilder<ReceiptVoucher> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.ReceiptNumber).IsRequired().HasMaxLength(60);
        entity.Property(x => x.Amount).HasColumnType("decimal(18,2)");
        entity.Property(x => x.PayerName).IsRequired().HasMaxLength(200);
        entity.Property(x => x.ReferenceNumber).HasMaxLength(100);
        entity.Property(x => x.Notes).HasMaxLength(1000);
        entity.HasIndex(x => x.ReceiptNumber).IsUnique();
        entity.HasIndex(x => new { x.Kind, x.Status });
    }
}

public class DeferredReceivableConfigration : IEntityTypeConfiguration<DeferredReceivable>
{
    public void Configure(EntityTypeBuilder<DeferredReceivable> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.ReceivableNumber).IsRequired().HasMaxLength(60);
        entity.Property(x => x.DebtorName).IsRequired().HasMaxLength(200);
        entity.Property(x => x.Amount).HasColumnType("decimal(18,2)");
        entity.Property(x => x.ReceivedAmount).HasColumnType("decimal(18,2)");
        entity.Property(x => x.Notes).HasMaxLength(1000);
        entity.HasIndex(x => x.ReceivableNumber).IsUnique();
        entity.HasIndex(x => new { x.Kind, x.Status, x.DueDate });
    }
}

public class ExpenseVoucherConfigration : IEntityTypeConfiguration<ExpenseVoucher>
{
    public void Configure(EntityTypeBuilder<ExpenseVoucher> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.ExpenseNumber).IsRequired().HasMaxLength(60);
        entity.Property(x => x.Amount).HasColumnType("decimal(18,2)");
        entity.Property(x => x.PayeeName).IsRequired().HasMaxLength(200);
        entity.Property(x => x.Notes).HasMaxLength(1000);
        entity.HasIndex(x => x.ExpenseNumber).IsUnique();
        entity.HasIndex(x => new { x.Kind, x.Status });
    }
}

public class SalaryDisbursementConfigration : IEntityTypeConfiguration<SalaryDisbursement>
{
    public void Configure(EntityTypeBuilder<SalaryDisbursement> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.TotalAmount).HasColumnType("decimal(18,2)");
        entity.Property(x => x.ExportFormat).IsRequired().HasMaxLength(80);
        entity.Property(x => x.Notes).HasMaxLength(1000);
        entity.HasIndex(x => new { x.PayrollMonth, x.Status });
    }
}

public class FinancialReviewItemConfigration : IEntityTypeConfiguration<FinancialReviewItem>
{
    public void Configure(EntityTypeBuilder<FinancialReviewItem> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.ReferenceNumber).IsRequired().HasMaxLength(100);
        entity.Property(x => x.Amount).HasColumnType("decimal(18,2)");
        entity.Property(x => x.DecisionNotes).HasMaxLength(1000);
        entity.HasIndex(x => new { x.Kind, x.Status });
    }
}

public class FinanceBudgetConfigration : IEntityTypeConfiguration<FinanceBudget>
{
    public void Configure(EntityTypeBuilder<FinanceBudget> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Name).IsRequired().HasMaxLength(200);
        entity.Property(x => x.PlannedIncome).HasColumnType("decimal(18,2)");
        entity.Property(x => x.PlannedExpenses).HasColumnType("decimal(18,2)");
        entity.HasIndex(x => new { x.FiscalYear, x.Status });
    }
}

public class BankReconciliationConfigration : IEntityTypeConfiguration<BankReconciliation>
{
    public void Configure(EntityTypeBuilder<BankReconciliation> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.StatementBalance).HasColumnType("decimal(18,2)");
        entity.Property(x => x.BookBalance).HasColumnType("decimal(18,2)");
        entity.Property(x => x.Notes).HasMaxLength(1000);
        entity.HasIndex(x => new { x.FinanceBankAccountId, x.ReconciliationDate }).IsUnique();
        entity.HasOne(x => x.FinanceBankAccount).WithMany().HasForeignKey(x => x.FinanceBankAccountId).OnDelete(DeleteBehavior.Restrict);
    }
}
