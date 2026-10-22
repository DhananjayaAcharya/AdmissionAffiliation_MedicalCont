//using System;
//using System.Collections.Generic;
//using Microsoft.EntityFrameworkCore;

//namespace Medical_Affiliation.Models;

//public partial class ApplicationDbContext : DbContext
//{
//    public ApplicationDbContext()
//    {
//    }

//    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
//        : base(options)
//    {
//    }

//    public virtual DbSet<MedicalSkillsLaboratory> MedicalSkillsLaboratories { get; set; }

////    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
////#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
////        => optionsBuilder.UseSqlServer("Server=DESKTOP-AOV1ROD;Database=Admission_Affiliation;Trusted_Connection=True;TrustServerCertificate=True");

//    protected override void OnModelCreating(ModelBuilder modelBuilder)
//    {
//        modelBuilder.Entity<MedicalSkillsLaboratory>(entity =>
//        {
//            entity.HasKey(e => e.Id)
//                .HasName("PK__Medical___3214EC07723BD6E2")
//                .HasFillFactor(80);

//            entity.ToTable("Medical_SkillsLaboratory");

//            entity.Property(e => e.CollegeCode)
//                .HasMaxLength(10)
//                .IsUnicode(false);
//            entity.Property(e => e.CourseLevel).HasMaxLength(10);
//            entity.Property(e => e.FacultyCode)
//                .HasMaxLength(10)
//                .IsUnicode(false);
//            entity.Property(e => e.SkillsLabEnabledForElearning).HasColumnName("SkillsLabEnabledForELearning");
//            entity.Property(e => e.TeachingAreasHaveAv).HasColumnName("TeachingAreasHaveAV");
//            entity.Property(e => e.TotalAreaAvailableSqm).HasColumnType("decimal(18, 2)");
//            entity.Property(e => e.TotalAreaDeficiencySqm).HasColumnType("decimal(18, 2)");
//            entity.Property(e => e.TotalAreaRequiredSqm).HasColumnType("decimal(18, 2)");
//        });

//        OnModelCreatingPartial(modelBuilder);
//    }

//    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
//}
