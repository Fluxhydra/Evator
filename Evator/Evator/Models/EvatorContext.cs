using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Evator.Models
{
    public partial class EvatorContext : DbContext
    {
        public EvatorContext()
        {
        }

        public EvatorContext(DbContextOptions<EvatorContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Accounts> Accounts { get; set; }
        public virtual DbSet<AtToken> AtToken { get; set; }
        public virtual DbSet<Attend> Attend { get; set; }
        public virtual DbSet<Events> Events { get; set; }
        public virtual DbSet<Invite> Invite { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
//#warning To protect potentially sensitive information in your connection string, you should move it out of source code. See http://go.microsoft.com/fwlink/?LinkId=723263 for guidance on storing connection strings.
                optionsBuilder.UseSqlServer("Server=localhost;Database=Evator;User Id=sa;Password=Rahman55");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("ProductVersion", "2.2.6-servicing-10079");

            modelBuilder.Entity<Accounts>(entity =>
            {
                entity.HasKey(e => e.AtId)
                    .HasName("PK__accounts__61F859883499E25F");

                entity.ToTable("accounts");

                entity.Property(e => e.AtId).HasColumnName("at_id");

                entity.Property(e => e.AtName)
                    .HasColumnName("at_name")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.AtPassword)
                    .HasColumnName("at_password")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.AtProfile)
                    .HasColumnName("at_profile")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Department)
                    .HasColumnName("department")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Email)
                    .HasColumnName("email")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.EmployeeId).HasColumnName("employee_id");

                entity.Property(e => e.RoleType).HasColumnName("role_type");
            });

            modelBuilder.Entity<AtToken>(entity =>
            {
                entity.HasKey(e => e.Token)
                    .HasName("PK__at_token__CA90DA7B48F05805");

                entity.ToTable("at_token");

                entity.Property(e => e.Token)
                    .HasColumnName("token")
                    .HasMaxLength(64)
                    .IsUnicode(false)
                    .ValueGeneratedNever();

                entity.Property(e => e.AtId).HasColumnName("at_id");

                entity.Property(e => e.TnStatus).HasColumnName("tn_status");

                entity.HasOne(d => d.At)
                    .WithMany(p => p.AtToken)
                    .HasForeignKey(d => d.AtId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_tn_at");
            });

            modelBuilder.Entity<Attend>(entity =>
            {
                entity.HasKey(e => e.AdId)
                    .HasName("PK__attend__CAA4A627246CAD01");

                entity.ToTable("attend");

                entity.Property(e => e.AdId).HasColumnName("ad_id");

                entity.Property(e => e.AdRecord)
                    .HasColumnName("ad_record")
                    .HasColumnType("datetime");

                entity.Property(e => e.AtId).HasColumnName("at_id");

                entity.Property(e => e.EtId).HasColumnName("et_id");

                entity.HasOne(d => d.At)
                    .WithMany(p => p.Attend)
                    .HasForeignKey(d => d.AtId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_ad_at");

                entity.HasOne(d => d.Et)
                    .WithMany(p => p.Attend)
                    .HasForeignKey(d => d.EtId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_ad_et");
            });

            modelBuilder.Entity<Events>(entity =>
            {
                entity.HasKey(e => e.EtId)
                    .HasName("PK__events__A78DA3D022A9F2BC");

                entity.ToTable("events");

                entity.Property(e => e.EtId).HasColumnName("et_id");

                entity.Property(e => e.Banner)
                    .HasColumnName("banner")
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.DateEnd)
                    .HasColumnName("date_end")
                    .HasColumnType("date");

                entity.Property(e => e.DateStart)
                    .HasColumnName("date_start")
                    .HasColumnType("date");

                entity.Property(e => e.EtDescription)
                    .HasColumnName("et_description")
                    .HasMaxLength(250)
                    .IsUnicode(false);

                entity.Property(e => e.EtLocation)
                    .HasColumnName("et_location")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.EtName)
                    .HasColumnName("et_name")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.EtStatus).HasColumnName("et_status");

                entity.Property(e => e.OwnerId).HasColumnName("owner_id");

                entity.Property(e => e.QrAttend)
                    .HasColumnName("qr_attend")
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.QrInvite)
                    .HasColumnName("qr_invite")
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.Speaker)
                    .HasColumnName("speaker")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.TimeEnd).HasColumnName("time_end");

                entity.Property(e => e.TimeStart).HasColumnName("time_start");

                entity.HasOne(d => d.Owner)
                    .WithMany(p => p.Events)
                    .HasForeignKey(d => d.OwnerId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_et_at");
            });

            modelBuilder.Entity<Invite>(entity =>
            {
                entity.HasKey(e => e.IeId)
                    .HasName("PK__invite__1871891ACC69C4B9");

                entity.ToTable("invite");

                entity.Property(e => e.IeId).HasColumnName("ie_id");

                entity.Property(e => e.AtId).HasColumnName("at_id");

                entity.Property(e => e.EtId).HasColumnName("et_id");

                entity.HasOne(d => d.At)
                    .WithMany(p => p.Invite)
                    .HasForeignKey(d => d.AtId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_ie_at");

                entity.HasOne(d => d.Et)
                    .WithMany(p => p.Invite)
                    .HasForeignKey(d => d.EtId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_ie_et");
            });
        }
    }
}
