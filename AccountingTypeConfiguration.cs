using Dominio.Core;
using System.Data.Entity.ModelConfiguration;

namespace Datos.Persistencia.Core
{
    public class AccountingTypeConfiguration : EntityTypeConfiguration<Accounting>
    {
        public AccountingTypeConfiguration()
        {
            this.Property(p => p.HoldingAccount).HasColumnType("varchar").HasMaxLength(20);
            this.Property(p => p.DescriptionText).HasColumnType("varchar").HasMaxLength(100);
            this.Property(p => p.DebitText).HasColumnType("varchar").HasMaxLength(100);
            this.Property(p => p.CreditText).HasColumnType("varchar").HasMaxLength(100);
            this.Property(p => p.CreditTransactionId).HasColumnType("varchar").HasMaxLength(10);
            this.Property(p => p.DebitTransactionId).HasColumnType("varchar").HasMaxLength(10);

            this.ToTable("ACH_Accounting");
        }
    }
}
