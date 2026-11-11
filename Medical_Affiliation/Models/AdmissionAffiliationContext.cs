using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Medical_Affiliation.Models;

public partial class AdmissionAffiliationContext : DbContext
{
    public AdmissionAffiliationContext()
    {
    }

    public AdmissionAffiliationContext(DbContextOptions<AdmissionAffiliationContext> options)
        : base(options)
    {
    }

    public virtual DbSet<TblAyurvedaStaffAvailability> TblAyurvedaStaffAvailabilities { get; set; }

//    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
//#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
//        => optionsBuilder.UseSqlServer("Server=localhost\\MSSQLSERVER01;Database=Admission_Affiliation;TrustServerCertificate=True;Trusted_Connection=true;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TblAyurvedaStaffAvailability>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Tbl_Ayur__3214EC0752D731C0");

            entity.ToTable("Tbl_AyurvedaStaffAvailability");

            entity.HasIndex(e => new { e.FacultyId, e.CollegeCode, e.CourseLevel, e.AcademicYear, e.StaffId, e.StaffDetailId }, "IX_Tbl_AyurvedaStaffAvailability_Lookup");

            entity.Property(e => e.AcademicYear).HasMaxLength(20);
            entity.Property(e => e.CollegeCode).HasMaxLength(50);
            entity.Property(e => e.CourseLevel).HasMaxLength(20);
            entity.Property(e => e.CreatedOn).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.StaffCadre).HasMaxLength(50);
            entity.Property(e => e.StaffDepartments).HasMaxLength(200);
            entity.Property(e => e.StaffDetailId).HasColumnName("StaffDetailID");
            entity.Property(e => e.StaffId).HasColumnName("StaffID");
            entity.Property(e => e.StaffSubDepartments).HasMaxLength(500);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
