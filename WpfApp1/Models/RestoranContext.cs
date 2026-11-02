using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace WpfApp1.Models;

public partial class RestoranContext : DbContext
{
    public RestoranContext()
    {
    }

    public RestoranContext(DbContextOptions<RestoranContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Blydum> Blyda { get; set; }

    public virtual DbSet<Broni> Bronis { get; set; }

    public virtual DbSet<KategoriiMenu> KategoriiMenus { get; set; }

    public virtual DbSet<Oplaty> Oplaties { get; set; }

    public virtual DbSet<PozitsiaZakaza> PozitsiaZakazas { get; set; }

    public virtual DbSet<Table> Tables { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<Zakazy> Zakazies { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=Restoran;Trusted_Connection=True;TrustServerCertificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Blydum>(entity =>
        {
            entity.Property(e => e.Foto).HasMaxLength(255);
            entity.Property(e => e.IdkategoriiMenu).HasColumnName("IDKategoriiMenu");
            entity.Property(e => e.Name).HasMaxLength(50);
            entity.Property(e => e.Prise)
                .HasColumnType("money")
                .HasColumnName("prise");
            entity.Property(e => e.Ves)
                .HasMaxLength(50)
                .UseCollation("Cyrillic_General_CI_AS");

            entity.HasOne(d => d.IdkategoriiMenuNavigation).WithMany(p => p.Blyda)
                .HasForeignKey(d => d.IdkategoriiMenu)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Blyda_KategoriiMenu");
        });

        modelBuilder.Entity<Broni>(entity =>
        {
            entity.ToTable("Broni");

            entity.Property(e => e.Fio)
                .HasMaxLength(150)
                .HasColumnName("FIO");
            entity.Property(e => e.Idtables).HasColumnName("IDTables");
            entity.Property(e => e.Idusers).HasColumnName("IDUsers");
            entity.Property(e => e.Komentari).HasMaxLength(500);
            entity.Property(e => e.Phone)
                .HasMaxLength(30)
                .HasColumnName("phone");
            entity.Property(e => e.Status).HasMaxLength(30);

            entity.HasOne(d => d.IdtablesNavigation).WithMany(p => p.Bronis)
                .HasForeignKey(d => d.Idtables)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Broni_Tables");

            entity.HasOne(d => d.IdusersNavigation).WithMany(p => p.Bronis)
                .HasForeignKey(d => d.Idusers)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Broni_Users");
        });

        modelBuilder.Entity<KategoriiMenu>(entity =>
        {
            entity.ToTable("KategoriiMenu");
        });

        modelBuilder.Entity<Oplaty>(entity =>
        {
            entity.ToTable("Oplaty");

            entity.Property(e => e.Chaevye).HasColumnType("money");
            entity.Property(e => e.DataOplat).HasColumnType("datetime");
            entity.Property(e => e.SposobOplate)
                .HasMaxLength(50)
                .HasColumnName("Sposob_oplate");
            entity.Property(e => e.Summa).HasColumnType("money");

            entity.HasOne(d => d.IdUsersNavigation).WithMany(p => p.Oplaties)
                .HasForeignKey(d => d.IdUsers)
                .HasConstraintName("FK_Oplaty_Users");

            entity.HasOne(d => d.IdZakazyNavigation).WithMany(p => p.Oplaties)
                .HasForeignKey(d => d.IdZakazy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Oplaty_Zakazy");
        });

        modelBuilder.Entity<PozitsiaZakaza>(entity =>
        {
            entity.ToTable("PozitsiaZakaza");

            entity.Property(e => e.Prise).HasColumnType("money");
            entity.Property(e => e.Status)
                .HasMaxLength(15)
                .IsFixedLength();

            entity.HasOne(d => d.IdBlydaNavigation).WithMany(p => p.PozitsiaZakazas)
                .HasForeignKey(d => d.IdBlyda)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PozitsiaZakaza_Blyda");

            entity.HasOne(d => d.IdZakazyNavigation).WithMany(p => p.PozitsiaZakazas)
                .HasForeignKey(d => d.IdZakazy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PozitsiaZakaza_Zakazy");
        });

        modelBuilder.Entity<Table>(entity =>
        {
            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Status)
                .HasMaxLength(10)
                .IsFixedLength();
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.Property(e => e.Fio)
                .HasMaxLength(50)
                .HasColumnName("FIO");
            entity.Property(e => e.Login)
                .HasMaxLength(20)
                .IsFixedLength()
                .HasColumnName("login");
            entity.Property(e => e.Parol)
                .HasMaxLength(10)
                .IsFixedLength()
                .HasColumnName("parol");
            entity.Property(e => e.Phone)
                .HasMaxLength(50)
                .HasColumnName("phone");
            entity.Property(e => e.Rol)
                .HasMaxLength(25)
                .IsFixedLength();
        });

        modelBuilder.Entity<Zakazy>(entity =>
        {
            entity.ToTable("Zakazy", tb =>
            {
                tb.HasTrigger("TR_Zakaz_Insert_UpdateTable");
                tb.HasTrigger("TR_Zakaz_Update_Status");
            });

            entity.Property(e => e.Status).HasMaxLength(20);
            entity.Property(e => e.SummaItogo)
                .HasColumnType("money")
                .HasColumnName("Summa_Itogo");

            entity.HasOne(d => d.IdTablesNavigation).WithMany(p => p.Zakazies)
                .HasForeignKey(d => d.IdTables)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Zakazy_Tables");

            entity.HasOne(d => d.IdUsersNavigation).WithMany(p => p.Zakazies)
                .HasForeignKey(d => d.IdUsers)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Zakazy_Users");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
