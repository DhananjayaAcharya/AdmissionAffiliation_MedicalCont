using Medical_Affiliation.Models;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

public class PreviewReportDentalPdf : IDocument
{
    private readonly CADentalpreviewViewModel _model;
    private readonly byte[] _logo;
    private readonly byte[] _collegeLogoBytes;

    // ─── Session-derived fields ───
    private readonly string _typeOfAffiliation;
    private readonly string _affiliationTypeId;
    private readonly string _courseLevel;
    private readonly string _facultyCode;

    // ─── Colour palette ───
    private static readonly string PrimaryColor = "#1B3A5C";   // Dark navy
    private static readonly string SecondaryColor = "#2E6DA4";   // Medium blue
    private static readonly string AccentColor = "#D4E6F1";   // Light blue bg
    private static readonly string HeaderBgColor = "#1B3A5C";   // Table header bg
    private static readonly string HeaderFgColor = "#FFFFFF";   // Table header text
    private static readonly string AltRowColor = "#F2F7FC";   // Alternating row
    private static readonly string BorderColor = "#B0C4DE";   // Light steel border
    private static readonly string DividerColor = "#1B3A5C";   // Divider line

    public PreviewReportDentalPdf(
        CADentalpreviewViewModel model,
        byte[] logo,
        byte[] clgLogoBytes,
        string typeOfAffiliation,
        string affiliationTypeId,
        string courseLevel,
        string facultyCode
        )
    {
        _model = model;
        _logo = logo;
        _collegeLogoBytes = clgLogoBytes;
        _typeOfAffiliation = typeOfAffiliation ?? "—";
        _affiliationTypeId = affiliationTypeId ?? "—";
        _courseLevel = courseLevel ?? "—";
        _facultyCode = facultyCode ?? "—";
    }

    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(20);

            page.Background()
                .AlignCenter()
                .AlignMiddle()
                .Width(300)
                .Image(_logo, ImageScaling.FitArea);

            page.Content().Border(1).Padding(5).Column(col =>
            {
                // --- REPORT HEADER ---
                AddReportHeader(col);

                //col.Item().PaddingVertical(10).Text($"Institution Name: {_model.CollegeName}");

                // ═══════════════════════════════════════
                //  INSTITUTION NAME + AFFILIATION INFO
                // ═══════════════════════════════════════
                AddAffiliationInfoBanner(col);

                //--- Institution Basic details - TrustMemberDetails ---
                AddInstitutionBasicDetailsSection(col);

                // --- HEAD OF INSTITUTION DETAILS ---
                AddHeadOfInstitutionSection(col);

                // ---Institution Basic details PRINCIPAL DETAILS ---
                AddPrincipalSection(col);

                // --- Institution Basic details DEAN / DIRECTOR DETAILS ---
                AddDeanOrDirectorSection(col);


                // --- NODAL OFFICER DETAILS ---
                AddNodalOfficerSection(col);

                // --- TRUST / MANAGEMENT DETAILS ---
                AddTrustManagementSection(col);

                //--- Institution Trust Institution details - TrustInstitutionDetails ---
                AddTrustInstitutionDetailsSection(col);

                //--- Institution Trust member details - TrustMemberDetails ---
                AddTrustMembersSection(col);

                // --- ACADEMIC INTAKE ---
                AddAcademicIntakeSection(col);

                // --- TEACHING FACULTY DETAILS ---
                AddTeachingFacultyDetailsSection(col);

                //--- AFFILIATED SANCTIONED INTAKE - Aff_SanctionedIntakeForCourse ---
                AddSanctionedIntakeSection(col);


                // --- HOSPITAL AFFILIATION ---
                AddHospitalAffiliationSection(col);

                AddDentalLandBuildingSection(col);

                AddClassroomAndSkillsLaboratorySection(col);

                // --- DENTAL CHAIR DISTRIBUTION ---
                AddDentalChairDistributionSection(col);

                // --- BED DISTRIBUTION ---
                AddDentalBedDistributionSection(col);

                // --- WORK SHOP DETAILS ---
                AddWorkshopDetails(col);

                // --- ANIMAL HOUSE DETAILS ---
                AddAnimalHouseDetails(col);

                if (_model.FacultyCode == "2")
                {
                    AddDepartmentOfficesAndDeuSection(col);

                    AddEquipmentListSection(col);

                    AddMedicalLibrarySection(col);
                }

                //--- COURSE DETAILS - AFF_CourseDetails ---
                AddAffiliatedCoursesSection(col);

                //--- UG COURSE DETAILS --- Affiliation_CourseDetails
                AddAffiliationCourseSection(col);


                // -- 2. PHYSICAL FACILITIES ----

                //-- SKILLS LAB EQUIPMENT
                AddSkillsLabEquipmentSection(col);

                // DENTAL STAFF DETAILS
                AddDentalStaffDetailsSection(col);

                //if (_model.FacultyCode == "1")
                //{
                //    //-- STUDENT PRACTICAL LABORATORIES ---
                //    AddStudentPracticalLabsSection(col);

                //    //--MUSEUMS ---
                //    AddMuseumsSection(col);

                //    // --- Department MEU ---
                //    AddDepartmentOfficesAndDeuSection(col);


                //    //--- SKILL LAB SECTION ----
                //    //AddSkillsLabSection(col);

                //    // --- LAB EQUIPMENT
                //    AddLaboratoryEquipmentSection(col);
                //    // -- end of chandans code ---
                //}


                //--- 3. RESEARCH AND PUBLICATIONS ---

                //--- LIBRARY RESEARCH PUBLICATIONS ---
                AddResearchPublicationsSection(col);


                //--- OTHER LIBRARY DETAILS - PENDING ---
                //--- LIBRARY OTHER DETAILS ---
                //AddLibraryOtherDetailsSection(col);

                //--- LIBRARAY COMMITTEE ---
                AddLibraryCommitteeSection(col);

                // -- end of the research ------

                // --PART B OF LIBRARY ---

                //--- LIBRARY GENERAL DETAILS ---
                AddLibraryGeneralDetailsSection(col);

                //--- LIBRARY ITEMS SECTION ---
                AddLibraryItemsSection(col);

                //--- LIBRARY BUILDING DETAILS ---
                AddLibraryBuildingSection(col);

                //--- LIBRARY TECHNICAL PROCESS ---
                AddLibraryTechnicalProcessSection(col);


                //--- LIBRARY EQUIPMENT---
                AddLibraryEquipmentSection(col);

                //--- LIBRARY FINANCE ---
                AddLibraryFinanceSection(col);


                //-- Library SERVICES---
                AddLibraryServicesSection(col);

                //--- LIBRARY USAGE REPORT ---
                AddLibraryUsageReportSection(col);

                //--- LIBRARY STAFF ---
                AddLibraryStaffSection(col);

                //--- DEPARTMENT LIBRARY ---
                AddDepartmentalLibrarySection(col);

                // -- END OF LIBRARY ----

                //--- VEHICLE DETAILS ---
                AddVehicleDetailsSection(col);


                // ---ADMIN TEACH BLOCK ---
                AddAdminTeachingBlockSection(col);

                //--- HOSTEL DETAILS ---
                AddHostelDetailsSection(col);

                //--- HOSTEL FACILITIES---
                AddHostelFacilitiesSection(col);

                //--- FACULTY DETIALS ---
                AddFacultyDetailsSection(col);

                //--- SUPER VISION IN FIELD PRACTICE AREA ----

                AddFieldPracticeAreaSection(col);

                //--- COLLEGE DESIGNATION ---
                AddCollegeDesignationSection(col);

                //--- NON TEACHING FACULTY ---
                AddNonTeachingStaffSection(col);

                // -- END OF ADMINISTRATIVE DETAILS ---


                // --- SECTIONS -----------
                AddDepartmentSections(col);

                //--- INDOOR BEDS OCCUPANCY ------
                AddIndoorBedsOccupancySection(col);

                // --- Academic Matters Section ---
                AddAcademicMattersSection(col);


                //--- ACCOUNT AND FEES ---
                AddFinanceAccountsAndFeesSection(col);

                //--- FINANCE STAFF PARTICULARS ---
                AddFinanceStaffParticularsSection(col);

                //--- FINANCE OTHER STAFF DETAILS ---
                AddFinanceOtherStaffDetailsSection(col);


                //--- DEAN / DIRECOTR DETAILS ---
                //AddDeanOrDirectorSection(col);

                ////--- PRINCIPAL DETAILS ---
                //AddPrincipalSection(col);

                AddPaymentSection(col);


                //--SMALL GROUP--- NURSING ONLY
                //AddSmallGroupTeachingSection(col);


            });

            page.Footer()
               .PaddingTop(8)
               .BorderTop(1)
               .BorderColor(DividerColor)
               .PaddingTop(5)
               .Row(row =>
               {
                   row.RelativeItem()
                       .AlignLeft()
                       .Text(text =>
                       {
                           text.Span("Downloaded on : ").FontSize(8).FontColor(SecondaryColor);
                           text.Span(DateTime.Now.ToString("dd-MM-yyyy, HH:mm tt")).FontSize(8).FontColor(PrimaryColor);
                       });

                   row.RelativeItem()
                       .AlignRight()
                       .Text(text =>
                       {
                           text.Span("Page ").FontSize(8).FontColor(SecondaryColor);
                           text.CurrentPageNumber().FontSize(8).FontColor(PrimaryColor);
                           text.Span(" of ").FontSize(8).FontColor(SecondaryColor);
                           text.TotalPages().FontSize(8).FontColor(PrimaryColor);
                       });
               });

        });

    }


    // ═══════════════════════════════════════════════════════
    //  AFFILIATION INFO BANNER  (from session values)
    // ═══════════════════════════════════════════════════════
    private void AddAffiliationInfoBanner(ColumnDescriptor col)
    {
        col.Item()
            .PaddingVertical(6)
            .Background(AccentColor)
            .Padding(10)
            .Border(1)
            .BorderColor(SecondaryColor)
            .Column(banner =>
            {
                banner.Item().AlignCenter()
                    .Text("Affiliation Details")
                    .FontSize(12)
                    .Bold()
                    .FontColor(PrimaryColor);

                banner.Item().PaddingTop(6).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(3);
                        columns.RelativeColumn(4);
                    });

                    // Row 1 – Type of Affiliation
                    AddStyledLabelValueRow(
                        table, "Type of Affiliation", _typeOfAffiliation);

                    // Row 3 – Course Level
                    AddStyledLabelValueRow(
                        table, "Course Level", _courseLevel);

                    // Row 4 – Faculty Code
                    AddStyledLabelValueRow(
                        table, "Faculty Code", _facultyCode);

                    // Row 5 – Institution Name (reiterated for clarity)
                    AddStyledLabelValueRow(
                        table, "Institution Name", _model?.CollegeName ?? "—");
                });
            });
    }

    private static string GetFacultyName(string code) => code switch
    {
        "1" => "Dental",
        "2" => "Medical",
        "3" => "Nursing",
        "4" => "Pharmacy",
        "5" => "AYUSH",
        _ => $"Code {code}"
    };


    /// <summary>
    /// Main section heading – dark background, white text
    /// </summary>
    private void AddMainHeading(ColumnDescriptor col, string title)
    {
        col.Item()
            .PaddingTop(14)
            .PaddingBottom(4)
            .Background(HeaderBgColor)
            .PaddingVertical(6)
            .PaddingHorizontal(10)
            .AlignCenter()
            .Text(title)
            .FontSize(13)
            .Bold()
            .FontColor(HeaderFgColor);
    }


    private static void StyledCell(TableDescriptor table, string text, bool alignCenter = false)
    {
        var cell = table.Cell()
            .Border(1)
            .BorderColor(BorderColor)
            .Padding(4);

        if (alignCenter)
            cell.AlignCenter();

        cell.Text(text ?? "—").FontSize(9);
    }

    /// <summary>
    /// Alternating row background helper
    /// </summary>
    private static string RowBg(int rowIndex) =>
        rowIndex % 2 == 0 ? AltRowColor : "#FFFFFF";

    private void AddReportHeader(ColumnDescriptor col)
    {
        col.Item().PaddingBottom(10).AlignCenter().Row(row =>
        {
            // ================= LOGO =================
            row.ConstantItem(80).AlignMiddle().Width(80)
                .Image(_collegeLogoBytes);

            row.ConstantItem(5); // spacing between logo and text

            // ================= TEXT BLOCK =================
            row.ConstantItem(360).AlignMiddle().Column(c =>
            {
                c.Item().AlignLeft().Text("Rajiv Gandhi University of Health Sciences, Karnataka")
                    .FontSize(14)
                    .Bold();

                c.Item().AlignLeft().PaddingTop(2)
                    .Text("4th 'T' Block, Jayanagar, Bangalore - 560 041")
                    .FontSize(10);
            });
        });

        // ================= DIVIDER LINE =================
        col.Item().PaddingVertical(4).LineHorizontal(1).LineColor(Colors.Black);
    }

    private void AddInstitutionBasicDetailsSection(ColumnDescriptor col)
    {
        var institution = _model?.InstitutionBasicVM?.InstitutionDetails;
        if (institution == null) return;

        AddMainHeading(col, "Institution Basic Details");

        // ── Basic Information ──
        AddSubHeading(col, "Basic Information");

        col.Item().PaddingTop(5).Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(3);
                columns.RelativeColumn(4);
            });

            AddStyledLabelValueRow(table, "Name of Institution", institution.NameOfInstitution);
            AddStyledLabelValueRow(table, "Year of Establishment", institution.YearOfEstablishment);
            AddStyledLabelValueRow(table, "Type of Institution", institution.TypeOfInstitution);
            AddStyledLabelValueRow(table, "Running Course", institution.RunningCourse);
            AddStyledLabelValueRow(table, "Course Level", institution.CourseLevel);
            AddStyledLabelValueRow(table, "Status of College", institution.StatusOfCollege);
            AddStyledLabelValueRow(table, "Financing Authority", institution.FinancingAuthority);
            AddStyledLabelValueRow(table, "Minority Category", institution.MinorityCategory);
            AddStyledLabelValueRow(table, "Minority Institution", institution.MinorityInstitute ? "Yes" : "No");
            AddStyledLabelValueRow(table, "Attached to Medical College", institution.AttachedToMedicalClg ? "Yes" : "No");
            AddStyledLabelValueRow(table, "Rural Institution", institution.RuralInstitute ? "Yes" : "No");
        });

        // ── College Location & Contact ──
        AddSubHeading(col, "College Location & Contact");

        col.Item().PaddingTop(5).Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(3);
                columns.RelativeColumn(4);
            });

            AddStyledLabelValueRow(table, "Address", institution.Address);
            AddStyledLabelValueRow(table, "Village / Town / City", institution.VillageTownCity);
            AddStyledLabelValueRow(table, "District", institution.District);
            AddStyledLabelValueRow(table, "Taluk", institution.Taluk);
            AddStyledLabelValueRow(table, "PIN Code", institution.PinCode);
            AddStyledLabelValueRow(table, "STD Code", institution.StdCode ?? "—");
            AddStyledLabelValueRow(table, "Mobile Number", institution.MobileNumber);
            AddStyledLabelValueRow(table, "Alternate / Landline", institution.AltLandlineMobile ?? "—");
            AddStyledLabelValueRow(table, "Fax", institution.Fax ?? "—");
            AddStyledLabelValueRow(table, "College Email", institution.EmailId);
            AddStyledLabelValueRow(table, "Alternate Email", institution.AltEmailId ?? "—");
            AddStyledLabelValueRow(table, "Website", institution.Website ?? "—");
            AddStyledLabelValueRow(table, "College URL", institution.College_URL ?? "—");
            AddStyledLabelValueRow(table, "Survey No / PID No", institution.SurveyNoPidNo ?? "—");
        });

        // ── Government Autonomous Details ──
        if (!string.IsNullOrWhiteSpace(institution.GovAutonomousCertNumber))
        {
            AddSubHeading(col, "Government Autonomous Details");

            col.Item().PaddingTop(5).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(3);
                    columns.RelativeColumn(4);
                });

                AddStyledLabelValueRow(table, "Gov Autonomous Certificate Number",
                    institution.GovAutonomousCertNumber);
            });
        }
    }

    private void AddTrustManagementSection(ColumnDescriptor col)
    {
        var institution = _model?.InstitutionBasicVM?.InstitutionDetails;
        if (institution == null) return;

        if (string.IsNullOrWhiteSpace(institution.TrustName) &&
            string.IsNullOrWhiteSpace(institution.TrustAddress) &&
            !institution.TrustEstablishmentDate.HasValue &&
            string.IsNullOrWhiteSpace(institution.TrustPresidentName) &&
            string.IsNullOrWhiteSpace(institution.TrustPresidentContactNo))
            return;

        AddMainHeading(col, "Trust Institution Details");
        AddSubHeading(col, "Trust / Management Details");

        col.Item().PaddingTop(5).Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(3);
                columns.RelativeColumn(4);
            });

            AddStyledLabelValueRow(table, "Trust Name", institution.TrustName ?? "—");
            AddStyledLabelValueRow(table, "Establishment Date", institution.TrustEstablishmentDate.HasValue
                ? institution.TrustEstablishmentDate.Value.ToString("dd-MM-yyyy") : "—");
            AddStyledLabelValueRow(table, "President Name", institution.TrustPresidentName ?? "—");
            AddStyledLabelValueRow(table, "President Contact", institution.TrustPresidentContactNo ?? "—");
            AddStyledLabelValueRow(table, "Trust Address", institution.TrustAddress ?? "—");
        });
    }

    private void AddNodalOfficerSection(ColumnDescriptor col)
    {
        var institution = _model?.InstitutionBasicVM?.InstitutionDetails;
        if (institution == null) return;

        if (string.IsNullOrWhiteSpace(institution.NodalOfficer_Name) &&
            string.IsNullOrWhiteSpace(institution.NodalOfficer_Mob_Number) &&
            string.IsNullOrWhiteSpace(institution.NodalOfficer_Email))
            return;

        AddSubHeading(col, "Nodal Officer & Academic Info");

        col.Item().PaddingTop(5).Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(3);
                columns.RelativeColumn(4);
            });

            AddStyledLabelValueRow(table, "Nodal Officer Name", institution.NodalOfficer_Name ?? "—");
            AddStyledLabelValueRow(table, "Nodal Officer Mobile", institution.NodalOfficer_Mob_Number ?? "—");
            AddStyledLabelValueRow(table, "Nodal Officer Email", institution.NodalOfficer_Email ?? "—");
        });
    }

    private void AddTrustMembersSection(ColumnDescriptor col)
    {
        var members = _model?.InstitutionBasicVM?.TrustMemberVM;
        if (members?.Items == null || !members.Items.Any()) return;

        AddSubHeading(col, "Trust Members");

        col.Item().PaddingTop(5).Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(3);
                columns.RelativeColumn(4);
            });

            int i = 1;
            foreach (var member in members.Items)
            {
                // Member heading row
                table.Cell()
                    .ColumnSpan(2)
                    .Border(1)
                    .BorderColor(SecondaryColor)
                    .Background(SecondaryColor)
                    .PaddingVertical(4)
                    .PaddingHorizontal(6)
                    .Text($"Trust Member {i}")
                    .Bold()
                    .FontSize(10)
                    .FontColor(HeaderFgColor);

                AddStyledLabelValueRow(table, "Name",
                    string.IsNullOrWhiteSpace(member.TrustMemberName) ? "—" : member.TrustMemberName);
                AddStyledLabelValueRow(table, "Designation",
                    string.IsNullOrWhiteSpace(member.Designation) ? "—" : member.Designation);
                AddStyledLabelValueRow(table, "Qualification",
                    string.IsNullOrWhiteSpace(member.Qualification) ? "—" : member.Qualification);
                AddStyledLabelValueRow(table, "Mobile",
                    string.IsNullOrWhiteSpace(member.MobileDisplay) ? "—" : member.MobileDisplay);
                AddStyledLabelValueRow(table, "Age", member.Age?.ToString() ?? "—");
                AddStyledLabelValueRow(table, "Joining Date",
                    string.IsNullOrWhiteSpace(member.JoiningDateDisplay) ? "—" : member.JoiningDateDisplay);

                i++;
            }
        });
    }

    // ═══════════════════════════════════════════════════════
    //  Faculty Repository  (styled table)
    // ═══════════════════════════════════════════════════════
    private void AddTeachingFacultyDetailsSection(ColumnDescriptor col)
    {
        var teachingFaculty = _model?.TeachingFacultyDetailsVM;
        if (teachingFaculty?.FacultyDetails == null || !teachingFaculty.FacultyDetails.Any()) return;

        AddMainHeading(col, "Faculty Repository");

        col.Item().PaddingTop(5).Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(2);
                columns.RelativeColumn(2);
                columns.ConstantColumn(80);
                columns.ConstantColumn(80);
            });

            // ── Header ──
            table.Header(header =>
            {
                string[] hdrs = { "Department", "Designation", "Required Faculty", "Available Faculty" };
                foreach (var h in hdrs)
                {
                    header.Cell()
                        .Border(1).BorderColor(BorderColor)
                        .Background(HeaderBgColor)
                        .Padding(5).AlignCenter()
                        .Text(h).Bold().FontSize(9).FontColor(HeaderFgColor);
                }
            });

            int rowIdx = 0;
            foreach (var item in teachingFaculty.FacultyDetails)
            {
                var bg = RowBg(rowIdx++);

                table.Cell().Border(1).BorderColor(BorderColor).Background(bg).Padding(5)
                    .Text(item.DepartmentName ?? "—").FontSize(9);

                table.Cell().Border(1).BorderColor(BorderColor).Background(bg).Padding(5)
                    .Text(item.DesignationName ?? "—").FontSize(9);

                table.Cell().Border(1).BorderColor(BorderColor).Background(bg).Padding(5).AlignCenter()
                    .Text(string.IsNullOrWhiteSpace(item.RequiredFaculty) ? "0" : item.RequiredFaculty).FontSize(9);

                table.Cell().Border(1).BorderColor(BorderColor).Background(bg).Padding(5).AlignCenter()
                    .Text(string.IsNullOrWhiteSpace(item.AvailableFaculty) ? "0" : item.AvailableFaculty).FontSize(9);
            }
        });
    }

    // ═══════════════════════════════════════════════════════
    //  FORMAT HELPER
    // ═══════════════════════════════════════════════════════
    string FormatValue(object? value)
    {
        if (value == null) return "—";
        if (value is decimal d) return d.ToString("0.##");
        if (value is double dbl) return dbl.ToString("0.##");
        if (value is float f) return f.ToString("0.##");
        return value.ToString() ?? "—";
    }

    // ═══════════════════════════════════════════════════════
    //  DENTAL LAND & BUILDING  (styled)
    // ═══════════════════════════════════════════════════════
    private void AddDentalLandBuildingSection(ColumnDescriptor col)
    {
        var lb = _model?.DentalLandBuildingPreview;
        if (lb == null) return;

        AddMainHeading(col, "Land & Building Details");

        // ── A. Land Details ──
        AddSubHeading(col, "A. Land Details");

        col.Item().PaddingTop(5).Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(3);
                columns.RelativeColumn(3);
            });

            AddTableHeader(table, "Particulars", "Details");

            AddStyledLabelValueRow(table, "Seat Intake", lb.SeatIntake.ToString());
            AddStyledLabelValueRow(table, "Seat Slab", lb.SeatSlab.ToString());
            AddStyledLabelValueRow(table, "Land Category", lb.LandCategory ?? "—");
            AddStyledLabelValueRow(table, "Total Land Area (Acres)", FormatValue(lb.TotalLandAreaAcres));
            AddStyledLabelValueRow(table, "Land Ownership Type", lb.LandOwnershipType ?? "—");
            AddStyledLabelValueRow(table, "Future Expansion Space", lb.HasFutureExpansionSpace == true ? "Yes" : "No");
        });

        // ── B. Building Details ──
        AddSubHeading(col, "B. Building Details");

        col.Item().PaddingTop(5).Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(2);
                columns.RelativeColumn(1);
                columns.RelativeColumn(1);
            });

            AddTableHeader(table, "Building Particulars", "Required / Norm", "Available");

            void AddBldRow(string label, string required, string available)
            {
                AddStyledLabelValueRow(table, label, ""); // We need 3 columns, so manual:
            }

            // Manually build 3-column rows
            // Reset table – we'll use a fresh approach below
        });

        // Re-do B. Building Details with 3-column layout
        col.Item().PaddingTop(5).Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(2);
                columns.RelativeColumn(1);
                columns.RelativeColumn(1);
            });

            table.Header(header =>
            {
                string[] hdrs = { "Building Particulars", "Required / Norm", "Available" };
                foreach (var h in hdrs)
                {
                    header.Cell()
                        .Border(1).BorderColor(BorderColor)
                        .Background(HeaderBgColor)
                        .Padding(5).AlignCenter()
                        .Text(h).Bold().FontSize(9).FontColor(HeaderFgColor);
                }
            });

            void AddBRow(string label, string req, string avail, int idx)
            {
                var bg = RowBg(idx);
                table.Cell().Border(1).BorderColor(BorderColor).Background(bg).Padding(5)
                    .Text(label).Bold().FontSize(9).FontColor(PrimaryColor);
                table.Cell().Border(1).BorderColor(BorderColor).Background(bg).Padding(5).AlignCenter()
                    .Text(req).FontSize(9);
                table.Cell().Border(1).BorderColor(BorderColor).Background(bg).Padding(5).AlignCenter()
                    .Text(avail).FontSize(9);
            }

            int r = 0;
            AddBRow("Total Built-up Area", $"{lb.RequiredBuiltupAreaSqm:0.##} Sq.m", $"{lb.TotalBuiltupAreaSqm:0.##} Sq.m", r++);
            AddBRow("Lecture Hall Count", lb.RequiredLectureHallCount.ToString(), lb.LectureHallCount.ToString(), r++);
            AddBRow("Lecture Hall Area", $"{lb.RequiredLectureHallAreaSqm:0.##} Sq.m", $"{lb.LectureHallAreaSqm:0.##} Sq.m", r++);
            AddBRow("Lecture Hall Capacity", lb.RequiredLectureHallCapacity.ToString(), lb.LectureHallSeatingCapacity.ToString(), r++);
            AddBRow("Examination Hall Area", $"{lb.RequiredExamHallAreaSqm:0.##} Sq.m", $"{lb.ExaminationHallAreaSqm:0.##} Sq.m", r++);
            AddBRow("Library Area", $"{lb.RequiredLibraryAreaSqm:0.##} Sq.m", $"{lb.LibraryAreaSqm:0.##} Sq.m", r++);
            AddBRow("Hospital Area", $"{lb.RequiredHospitalAreaSqm:0.##} Sq.m", $"{lb.HospitalAreaSqm:0.##} Sq.m", r++);
            AddBRow("Museum & Demo Rooms", "As per Norms", $"{lb.MuseumDemoRoomsAreaSqm:0.##} Sq.m", r++);
            AddBRow("Department-wise Area", "As per Norms", $"{lb.DepartmentWiseAreaSqm:0.##} Sq.m", r++);
            AddBRow("Preclinical & Skill Lab Area", "As per Norms", $"{lb.PreclinicalSkillLabAreaSqm:0.##} Sq.m", r++);
            AddBRow("Remarks", "—", lb.Remarks ?? "—", r++);
        });

        // ── C. Infrastructure Requirements ──
        if (lb.InfrastructureDetails?.Any() == true)
        {
            AddSubHeading(col, "C. Infrastructure Requirements");

            col.Item().PaddingTop(5).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(35);
                    columns.RelativeColumn(1.4f);
                    columns.RelativeColumn(2.5f);
                    columns.ConstantColumn(75);
                    columns.ConstantColumn(75);
                });

                table.Header(header =>
                {
                    string[] hdrs = { "Sl. No.", "Requirement", "Description", "Required\n(Sq.Ft)", "Available\n(Sq.Ft)" };
                    foreach (var h in hdrs)
                    {
                        header.Cell()
                            .Border(1).BorderColor(BorderColor)
                            .Background(HeaderBgColor)
                            .Padding(5).AlignCenter()
                            .Text(h).Bold().FontSize(8).FontColor(HeaderFgColor);
                    }
                });

                int rowIdx = 0;
                foreach (var item in lb.InfrastructureDetails.OrderBy(x => x.SlNo))
                {
                    var bg = RowBg(rowIdx++);
                    table.Cell().Border(1).BorderColor(BorderColor).Background(bg).Padding(4).AlignCenter().Text(item.SlNo.ToString()).FontSize(8);
                    table.Cell().Border(1).BorderColor(BorderColor).Background(bg).Padding(4).Text(item.RequirementName ?? "—").FontSize(8);
                    table.Cell().Border(1).BorderColor(BorderColor).Background(bg).Padding(4).Text(item.RequirementDescription ?? "—").FontSize(8);
                    table.Cell().Border(1).BorderColor(BorderColor).Background(bg).Padding(4).AlignCenter().Text($"{item.RequiredAreaSqFt:0.00}").FontSize(8);
                    table.Cell().Border(1).BorderColor(BorderColor).Background(bg).Padding(4).AlignCenter().Text($"{item.AvailableAreaSqFt:0.00}").FontSize(8);
                }
            });
        }

        // ── D. Documents ──
        AddSubHeading(col, "D. Land & Building Documents");

        col.Item().PaddingTop(5).Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(3);
                columns.ConstantColumn(90);
            });

            AddTableHeader(table, "Document", "Status");

            void AddDocRow(string name, string? path, int idx)
            {
                var bg = RowBg(idx);
                table.Cell().Border(1).BorderColor(BorderColor).Background(bg).Padding(5)
                    .Text(name).Bold().FontSize(9).FontColor(PrimaryColor);
                table.Cell().Border(1).BorderColor(BorderColor).Background(bg).Padding(5).AlignCenter()
                    .Text(!string.IsNullOrWhiteSpace(path) ? "✓ Uploaded" : "✗ Not Uploaded")
                    .FontSize(9)
                    .FontColor(!string.IsNullOrWhiteSpace(path) ? "#27AE60" : "#E74C3C");
            }

            int d = 0;
            AddDocRow("Sale Deed", lb.SaleDeedDocumentPath, d++);
            AddDocRow("Encumbrance Certificate", lb.EncumbranceCertificateDocumentPath, d++);
            AddDocRow("Land Use Certificate", lb.LandUseCertificateDocumentPath, d++);
            AddDocRow("Approved Layout Plan", lb.ApprovedLayoutPlanDocumentPath, d++);
            AddDocRow("Land Sketch", lb.LandSketchDocumentPath, d++);
            AddDocRow("Distance Certificate", lb.DistanceCertificateDocumentPath, d++);
            AddDocRow("Approved Building Plan", lb.ApprovedBuildingPlanDocumentPath, d++);
            AddDocRow("Completion Certificate", lb.CompletionCertificateDocumentPath, d++);
            AddDocRow("Structural Stability Certificate", lb.StructuralStabilityCertificateDocumentPath, d++);
            AddDocRow("Fire Safety NOC", lb.FireSafetyNocDocumentPath, d++);
            AddDocRow("Lift License", lb.LiftLicenseDocumentPath, d++);
            AddDocRow("Electrical Safety Certificate", lb.ElectricalSafetyCertificateDocumentPath, d++);
            AddDocRow("Water Supply Certificate", lb.WaterSupplyCertificateDocumentPath, d++);
            AddDocRow("Sewage / Sanitation Approval", lb.SewageSanitationApprovalDocumentPath, d++);
        });
    }


    // ═══════════════════════════════════════════════════════
    //  CLASSROOM & SKILLS LABORATORY  (styled)
    // ═══════════════════════════════════════════════════════
    private void AddClassroomAndSkillsLaboratorySection(ColumnDescriptor col)
    {
        var sl = _model?.DentalSkillsLaboratoryVM;
        if (sl == null) return;

        AddMainHeading(col, "Classroom & Skills Laboratory");

        // ── A. Skills Laboratory Details ──
        AddSubHeading(col, "A. Skills Laboratory Details");

        col.Item().PaddingTop(5).Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(3);
                columns.RelativeColumn(2);
            });

            AddStyledLabelValueRow(table, "Annual BDS Intake", sl.AnnualBdsIntake.ToString());
            AddStyledLabelValueRow(table, "Total Area Required (Sq.m)", sl.TotalAreaRequiredSqm.ToString());
            AddStyledLabelValueRow(table, "Total Area Available (Sq.m)", sl.TotalAreaAvailableSqm.ToString());
            AddStyledLabelValueRow(table, "Area Deficiency (Sq.m)", sl.TotalAreaDeficiencySqm.ToString());
            AddStyledLabelValueRow(table, "Number of Examination Rooms", sl.NumberOfExaminationRooms.ToString());
            AddStyledLabelValueRow(table, "Number of Skill Stations", sl.NumberOfSkillStations.ToString());
        });

        // ── B. Infrastructure Compliance ──
        AddSubHeading(col, "B. Infrastructure Compliance");

        col.Item().PaddingTop(5).Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(3);
                columns.ConstantColumn(90);
            });

            AddTableHeader(table, "Requirement", "Status");

            void AddStatusRow(string requirement, bool? status, int idx)
            {
                var bg = RowBg(idx);
                table.Cell().Border(1).BorderColor(BorderColor).Background(bg).Padding(5)
                    .Text(requirement).FontSize(9);
                table.Cell().Border(1).BorderColor(BorderColor).Background(bg).Padding(5).AlignCenter()
                    .Text(status == true ? "✓ Yes" : "✗ No")
                    .FontSize(9)
                    .FontColor(status == true ? "#27AE60" : "#E74C3C");
            }

            int i = 0;
            AddStatusRow("6 Weeks Training Completed", sl.SixWeeksTrainingCompletedBeforeClinical, i++);
            AddStatusRow("Minimum Four Examination Rooms", sl.HasMinFourExamRooms, i++);
            AddStatusRow("Demo Room for Small Groups", sl.HasDemoRoomSmallGroups, i++);
            AddStatusRow("Debrief Area", sl.HasDebriefArea, i++);
            AddStatusRow("Faculty Coordinator Room", sl.HasFacultyCoordinatorRoom, i++);
            AddStatusRow("Support Staff Room", sl.HasSupportStaffRoom, i++);
            AddStatusRow("Storage for Mannequins", sl.HasStorageForMannequins, i++);
            AddStatusRow("Video Recording Facility", sl.HasVideoRecordingFacility, i++);
            AddStatusRow("Group & Individual Stations", sl.HasGroupAndIndividualStations, i++);
            AddStatusRow("Required Trainers & Mannequins", sl.HasRequiredTrainersAndMannequins, i++);
            AddStatusRow("Dedicated Technical Officer", sl.HasDedicatedTechnicalOfficer, i++);
            AddStatusRow("Adequate Support Staff", sl.HasAdequateSupportStaff, i++);
            AddStatusRow("Teaching Areas with AV Facility", sl.TeachingAreasHaveAV, i++);
            AddStatusRow("Teaching Areas with Internet", sl.TeachingAreasHaveInternet, i++);
            AddStatusRow("E-Learning Enabled", sl.SkillsLabEnabledForELearning, i++);
        });

        // ── C. Pre-Clinical & Skills Laboratory Areas ──
        if (sl.PreClinicalAndSkillsLabs?.Any() == true)
        {
            AddSubHeading(col, "C. Pre-Clinical & Skills Laboratory Areas");

            foreach (var labGroup in sl.PreClinicalAndSkillsLabs.GroupBy(x => x.LaboratorySection))
            {
                col.Item()
                    .PaddingTop(8)
                    .Background(AccentColor)
                    .Border(1).BorderColor(SecondaryColor)
                    .PaddingVertical(3).PaddingHorizontal(6)
                    .Text(labGroup.Key ?? "Laboratory Details")
                    .FontSize(10).SemiBold().FontColor(PrimaryColor);

                col.Item().PaddingTop(4).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(2);
                        columns.ConstantColumn(100);
                        columns.ConstantColumn(100);
                    });

                    table.Header(header =>
                    {
                        string[] hdrs = { "Laboratory", "Required Area (Sq.ft)", "Existing Area (Sq.ft)" };
                        foreach (var h in hdrs)
                        {
                            header.Cell()
                                .Border(1).BorderColor(BorderColor)
                                .Background(HeaderBgColor)
                                .Padding(5).AlignCenter()
                                .Text(h).Bold().FontSize(9).FontColor(HeaderFgColor);
                        }
                    });

                    int rowIdx = 0;
                    foreach (var lab in labGroup)
                    {
                        var bg = RowBg(rowIdx++);
                        table.Cell().Border(1).BorderColor(BorderColor).Background(bg).Padding(5)
                            .Text(lab.LabName ?? "—").FontSize(9);
                        table.Cell().Border(1).BorderColor(BorderColor).Background(bg).Padding(5).AlignCenter()
                            .Text(lab.RequiredAreaSqFt.ToString("0.##")).FontSize(9);
                        table.Cell().Border(1).BorderColor(BorderColor).Background(bg).Padding(5).AlignCenter()
                            .Text((lab.ExistingAreaSqFt ?? 0).ToString("0.##")).FontSize(9);
                    }
                });
            }
        }
    }

    // ═══════════════════════════════════════════════════════
    //  DENTAL CHAIR DISTRIBUTION  (styled)
    // ═══════════════════════════════════════════════════════
    private void AddDentalChairDistributionSection(ColumnDescriptor col)
    {
        var chairs = _model?.DentalChairDistribution;
        if (chairs == null || !chairs.Any()) return;

        AddMainHeading(col, "Dental Chair Distribution");
        AddSubHeading(col, "Dental Chair Requirements");

        col.Item().PaddingTop(5).Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.ConstantColumn(35);
                columns.RelativeColumn(1.5f);
                columns.ConstantColumn(60);
                columns.ConstantColumn(60);
                columns.ConstantColumn(60);
                columns.ConstantColumn(70);
                columns.ConstantColumn(70);
            });

            table.Header(header =>
            {
                string[] hdrs = { "Sl.", "Course", "Level", "Intake", "Seat Slab", "Required", "Existing" };
                foreach (var h in hdrs)
                {
                    header.Cell()
                        .Border(1).BorderColor(BorderColor)
                        .Background(HeaderBgColor)
                        .Padding(4).AlignCenter()
                        .Text(h).Bold().FontSize(8).FontColor(HeaderFgColor);
                }
            });

            int slNo = 1;
            foreach (var item in chairs)
            {
                var bg = RowBg(slNo - 1);
                table.Cell().Border(1).BorderColor(BorderColor).Background(bg).Padding(4).AlignCenter().Text(slNo.ToString()).FontSize(8);
                table.Cell().Border(1).BorderColor(BorderColor).Background(bg).Padding(4).Text(item.CourseName ?? "—").FontSize(8);
                table.Cell().Border(1).BorderColor(BorderColor).Background(bg).Padding(4).AlignCenter().Text(item.CourseLevel ?? "—").FontSize(8);
                table.Cell().Border(1).BorderColor(BorderColor).Background(bg).Padding(4).AlignCenter().Text(item.SeatSlab.ToString()).FontSize(8);
                table.Cell().Border(1).BorderColor(BorderColor).Background(bg).Padding(4).AlignCenter().Text(item.SeatSlab.ToString()).FontSize(8);
                table.Cell().Border(1).BorderColor(BorderColor).Background(bg).Padding(4).AlignCenter().Text(item.ChairsRequired.ToString()).FontSize(8);
                table.Cell().Border(1).BorderColor(BorderColor).Background(bg).Padding(4).AlignCenter().Text(item.ChairsExisting.ToString()).FontSize(8);
                slNo++;
            }
        });
    }

    // ═══════════════════════════════════════════════════════
    //  EQUIPMENT LIST  (styled)
    // ═══════════════════════════════════════════════════════
    private void AddEquipmentListSection(ColumnDescriptor col)
    {
        var equipment = _model?.EquipmentPreviewVM;

        if (equipment?.Departments == null ||
            !equipment.Departments.Any())
            return;

        // =========================================================
        // MAIN HEADING
        // =========================================================

        AddMainHeading(col, "Equipment Details");

        // =========================================================
        // DEPARTMENT-WISE EQUIPMENT
        // =========================================================

        foreach (var department in equipment.Departments)
        {
            if (department.Equipments == null ||
                !department.Equipments.Any())
                continue;

            AddSubHeading(
                col,
                string.IsNullOrWhiteSpace(department.DepartmentName)
                    ? "Department"
                    : department.DepartmentName
            );

            col.Item()
                .PaddingTop(8)
                .Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(35);   // Sl No
                        columns.RelativeColumn(2);     // Equipment
                        columns.RelativeColumn(2);     // Specification
                        columns.ConstantColumn(65);    // One Unit Required
                        columns.ConstantColumn(65);    // One Unit Existing
                        columns.ConstantColumn(65);    // Two Unit Required
                        columns.ConstantColumn(65);    // Two Unit Existing
                    });

                    // =================================================
                    // HEADER
                    // =================================================

                    AddTableHeader(
                        table,
                        "Sl. No.",
                        "Equipment",
                        "Specification",
                        "One Unit\nRequired",
                        "One Unit\nExisting",
                        "Two Unit\nRequired",
                        "Two Unit\nExisting"
                    );

                    // =================================================
                    // BODY
                    // =================================================

                    int slNo = 1;

                    foreach (var item in department.Equipments)
                    {
                        var bg = RowBg(slNo - 1);

                        // Sl No
                        table.Cell()
                            .Border(1)
                            .BorderColor(BorderColor)
                            .Background(bg)
                            .Padding(4)
                            .AlignCenter()
                            .Text(slNo.ToString())
                            .FontSize(8);

                        // Equipment
                        table.Cell()
                            .Border(1)
                            .BorderColor(BorderColor)
                            .Background(bg)
                            .Padding(4)
                            .Text(
                                string.IsNullOrWhiteSpace(item.EquipmentName)
                                    ? "—"
                                    : item.EquipmentName
                            )
                            .FontSize(8)
                            .FontColor(PrimaryColor);

                        // Specification
                        table.Cell()
                            .Border(1)
                            .BorderColor(BorderColor)
                            .Background(bg)
                            .Padding(4)
                            .Text(
                                string.IsNullOrWhiteSpace(item.Specification)
                                    ? "—"
                                    : item.Specification
                            )
                            .FontSize(8);

                        // One Unit Required
                        table.Cell()
                            .Border(1)
                            .BorderColor(BorderColor)
                            .Background(bg)
                            .Padding(4)
                            .AlignCenter()
                            .Text(
                                item.OneUnitReq?.ToString() ?? "—"
                            )
                            .FontSize(8);

                        // One Unit Existing
                        table.Cell()
                            .Border(1)
                            .BorderColor(BorderColor)
                            .Background(bg)
                            .Padding(4)
                            .AlignCenter()
                            .Text(
                                item.OneUnitExisting?.ToString() ?? "—"
                            )
                            .FontSize(8);

                        // Two Unit Required
                        table.Cell()
                            .Border(1)
                            .BorderColor(BorderColor)
                            .Background(bg)
                            .Padding(4)
                            .AlignCenter()
                            .Text(
                                item.TwoUnitReq?.ToString() ?? "—"
                            )
                            .FontSize(8);

                        // Two Unit Existing
                        table.Cell()
                            .Border(1)
                            .BorderColor(BorderColor)
                            .Background(bg)
                            .Padding(4)
                            .AlignCenter()
                            .Text(
                                item.TwoUnitExisting?.ToString() ?? "—"
                            )
                            .FontSize(8);

                        slNo++;
                    }
                });
        }
    }


    private void AddSanctionedIntakeSection(ColumnDescriptor col)
    {
        var intakeVM = _model.InstitutionBasicVM?.IntakeForCourseVM;

        if (intakeVM == null || intakeVM.Items == null || !intakeVM.Items.Any())
            return;

        // ===== SECTION HEADING =====
        AddSubHeading(col, "Sanctioned Intake Details");

        // ===== TABLE =====
        col.Item()
            .PaddingTop(8)
            .Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(3); // Course Name
                    columns.RelativeColumn(2); // Sanctioned Intake
                    columns.RelativeColumn(2); // Eligible Seat Slab
                    columns.RelativeColumn(2); // Document
                });

                // ===== HEADER =====
                AddTableHeader(
                    table,
                    "Course Name",
                    "Sanctioned Intake",
                    "Eligible Seat Slab",
                    "Document"
                );

                // ===== BODY =====
                foreach (var item in intakeVM.Items)
                {
                    // Course Name
                    table.Cell()
                        .Border(1)
                        .BorderColor(BorderColor)
                        .Padding(5)
                        .Text(item.CourseName ?? "—")
                        .FontSize(9)
                        .FontColor(PrimaryColor);

                    // Sanctioned Intake
                    table.Cell()
                        .Border(1)
                        .BorderColor(BorderColor)
                        .Padding(5)
                        .AlignCenter()
                        .Text(item.SanctionedIntake?.ToString() ?? "—")
                        .FontSize(9);

                    // Eligible Seat Slab
                    table.Cell()
                        .Border(1)
                        .BorderColor(BorderColor)
                        .Padding(5)
                        .AlignCenter()
                        .Text(item.EligibleSeatSlab ?? "—")
                        .FontSize(9);

                    // Document
                    table.Cell()
                        .Border(1)
                        .BorderColor(BorderColor)
                        .Padding(5)
                        .AlignCenter()
                        .Text(item.HasDocument ? "Available" : "—")
                        .FontSize(9);
                }
            });
    }

    private void AddAffiliatedCoursesSection(ColumnDescriptor col)
    {
        var intakeDetails = _model?.InstitutionBasicVM;
        var courses = intakeDetails?.AffCoursesVM?.Items;

        if (courses == null || !courses.Any())
            return;

        // ===== SECTION HEADING =====
        AddSubHeading(col, "Course Details");

        // ===== TABLE =====
        col.Item()
            .PaddingTop(8)
            .Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(4);   // Course Name
                    columns.ConstantColumn(70);  // Recognized
                    columns.RelativeColumn(3);   // RGUHS Notification No
                    columns.ConstantColumn(70);  // Document
                });

                // ===== HEADER =====
                AddTableHeader(
                    table,
                    "Course Name",
                    "Recognized",
                    "RGUHS Notification No",
                    "Document"
                );

                // ===== BODY =====
                int rowIndex = 0;

                foreach (var item in courses)
                {
                    var bg = RowBg(rowIndex);

                    // Course Name
                    table.Cell()
                        .Border(1)
                        .BorderColor(BorderColor)
                        .Background(bg)
                        .Padding(5)
                        .Text(item.CourseName ?? "—")
                        .FontSize(9)
                        .FontColor(PrimaryColor);

                    // Recognized
                    table.Cell()
                        .Border(1)
                        .BorderColor(BorderColor)
                        .Background(bg)
                        .Padding(5)
                        .AlignCenter()
                        .Text(item.IsRecognized ? "Yes" : "No")
                        .FontSize(9);

                    // RGUHS Notification Number
                    table.Cell()
                        .Border(1)
                        .BorderColor(BorderColor)
                        .Background(bg)
                        .Padding(5)
                        .Text(item.RguhsNotificationNo ?? "—")
                        .FontSize(9);

                    // Document
                    table.Cell()
                        .Border(1)
                        .BorderColor(BorderColor)
                        .Background(bg)
                        .Padding(5)
                        .AlignCenter()
                        .Text(item.HasDocument ? "Available" : "—")
                        .FontSize(9);

                    rowIndex++;
                }
            });
    }

    private void AddAffiliationCourseSection(ColumnDescriptor col)
    {
        var item = _model?.InstitutionBasicVM?.AffiliationCourseDetailVM;

        if (item == null)
            return;

        // ===== SECTION HEADING =====
        AddSubHeading(col, "Affiliated Course Details");

        // ===== DETAILS TABLE =====
        col.Item()
            .PaddingTop(8)
            .Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(3); // Label
                    columns.RelativeColumn(4); // Value
                });

                AddTextRow(table, "Course Name", item.CourseName);
                AddTextRow(table, "Intake 2025-26", item.IntakeDuring202526);
                AddTextRow(table, "Intake Slab", item.IntakeSlab);
                AddTextRow(table, "Permission Type", item.TypeofPermission);
                AddTextRow(table, "Year of LoP", item.YearOfLop);
                AddTextRow(table, "Date of Recognition", item.DateOfRecognition);
                AddTextRow(table, "Year of EC/FC", item.YearOfObtainingEcAndFc);
                AddTextRow(table, "Sanctioned Intake EC/FC", item.SanctionedIntakeEcFc);
                AddTextRow(
                    table,
                    "GoK Order",
                    item.HasGokOrder ? "Available" : "—"
                );
            });
    }

    private void AddDeanOrDirectorSection(ColumnDescriptor col)
    {
        var dean = _model?.InstitutionBasicVM.DeanOrDirectorDetailDisplayVM;
        if (dean == null) return;

        AddSubHeading(col, "Dean / Director Details");

        col.Item().PaddingTop(8).Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(3); // Label
                columns.RelativeColumn(4); // Value
            });

            AddTextRow(table, "Name", dean.DeanName);
            AddTextRow(table, "Dean Mobile Number", dean.DeanMobileNumber);
            AddTextRow(table, "Dean Email Id", dean.DeanEmailId);
        });
    }

    private void AddPrincipalSection(ColumnDescriptor col)
    {
        var principal = _model?.InstitutionBasicVM?.PrincipalDetailDisplayVM;

        if (principal == null)
            return;

        // ===== SECTION HEADING =====
        AddSubHeading(col, "Principal Details");

        // ===== DETAILS TABLE =====
        col.Item()
            .PaddingTop(8)
            .Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(3); // Label
                    columns.RelativeColumn(4); // Value
                });

                AddTextRow(table, "Principal Name", principal.Principal_Name);
                AddTextRow(table, "Principal Email Id", principal.PrincipalEmailId);
                AddTextRow(table, "Principal Mobile Number", principal.PrincipalMobileNumber);
            });
    }

    private void AddHeadOfInstitutionSection(ColumnDescriptor col)
    {
        var institution = _model?.InstitutionBasicVM?.InstitutionDetails;

        if (institution == null)
            return;

        // ===== DON'T SHOW EMPTY SECTION =====
        if (string.IsNullOrWhiteSpace(institution.HeadOfInstitution) &&
            string.IsNullOrWhiteSpace(institution.HeadOfInstitution_Mob_NO) &&
            string.IsNullOrWhiteSpace(institution.HeadOfInstitution_Email) &&
            string.IsNullOrWhiteSpace(institution.HeadAddress))
        {
            return;
        }

        // ===== SECTION HEADING =====
        AddSubHeading(col, "Head of Institution Details");

        // ===== DETAILS TABLE =====
        col.Item()
            .PaddingTop(8)
            .Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(3); // Label
                    columns.RelativeColumn(4); // Value
                });

                AddTextRow(
                    table,
                    "Head of Institution",
                    institution.HeadOfInstitution
                );

                AddTextRow(
                    table,
                    "Head Mobile No",
                    institution.HeadOfInstitution_Mob_NO
                );

                AddTextRow(
                    table,
                    "Head Email",
                    institution.HeadOfInstitution_Email
                );

                AddTextRow(
                    table,
                    "Head Address",
                    institution.HeadAddress
                );
            });
    }

    private void AddTrustInstitutionDetailsSection(ColumnDescriptor col)
    {
        var trust = _model?.TrustDetailsVM;

        if (trust == null)
            return;

        // =========================================================
        // TRUST / SOCIETY DETAILS
        // =========================================================

        AddSubHeading(col, "Trust / Society Details");

        col.Item()
            .PaddingTop(8)
            .Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(3); // Label
                    columns.RelativeColumn(4); // Value
                });

                AddTextRow(table, "Trust Name", trust.TrustName);

                AddTextRow(table, "TRUST PAN Number", trust.PANNumber);

                AddTextRow(table, "Registration Number", trust.RegistrationNumber);

                AddTextRow(
                    table,
                    "Registration Date",
                    trust.RegistrationDate.HasValue
                        ? trust.RegistrationDate.Value.ToString("dd MMMM yyyy")
                        : null
                );

                AddTextRow(table, "President Name", trust.PresidentName);

                AddTextRow(
                    table,
                    "Category of Organisation",
                    trust.CategoryOfOrganisation
                );

                AddTextRow(
                    table,
                    "GOK Obtained Trust Name",
                    trust.GOKObtainedTrustName
                );

                AddTextRow(
                    table,
                    "Amendments",
                    trust.Amendments.HasValue
                        ? (trust.Amendments.Value ? "Yes" : "No")
                        : null
                );
            });


        // =========================================================
        // TRUST CONTACT & COMMUNICATION
        // =========================================================

        AddSubHeading(col, "Trust Contact & Communication");

        col.Item()
            .PaddingTop(8)
            .Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(3); // Label
                    columns.RelativeColumn(4); // Value
                });

                AddTextRow(table, "Address", trust.Address);

                AddTextRow(table, "PIN Code", trust.PinCode);

                AddTextRow(table, "Mobile Number", trust.MobileNumber);

                AddTextRow(table, "STD Code", trust.StdCode);

                AddTextRow(table, "Fax", trust.Fax);

                AddTextRow(
                    table,
                    "Alternate Landline / Mobile",
                    trust.AltLandlineOrMobile
                );

                AddTextRow(table, "Email ID", trust.EmailId);

                AddTextRow(
                    table,
                    "Alternate Email ID",
                    trust.AltEmailId
                );
            });


        // =========================================================
        // TRUST CONTACT PERSON
        // =========================================================

        AddSubHeading(col, "Trust Contact Person");

        col.Item()
            .PaddingTop(8)
            .Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(3); // Label
                    columns.RelativeColumn(4); // Value
                });

                AddTextRow(
                    table,
                    "Full Name",
                    trust.ContactPersonName
                );

                AddTextRow(
                    table,
                    "Designation",
                    trust.ContactPersonRelation
                );

                AddTextRow(
                    table,
                    "Mobile",
                    trust.ContactPersonMobile
                );
            });


        // =========================================================
        // OTHER TRUST INFORMATION
        // =========================================================

        if (!string.IsNullOrWhiteSpace(trust.ExistingTrustName) ||
            trust.ChangesInTrustName.HasValue)
        {
            AddSubHeading(col, "Other Trust Information");

            col.Item()
                .PaddingTop(8)
                .Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(3); // Label
                        columns.RelativeColumn(4); // Value
                    });

                    AddTextRow(
                        table,
                        "Existing Trust Name",
                        trust.ExistingTrustName
                    );

                    AddTextRow(
                        table,
                        "Changes in Trust Name",
                        trust.ChangesInTrustName.HasValue
                            ? (trust.ChangesInTrustName.Value ? "Yes" : "No")
                            : null
                    );
                });
        }


        // =========================================================
        // TRUST DOCUMENTS
        // =========================================================

        AddSubHeading(col, "Trust Documents");

        col.Item()
            .PaddingTop(8)
            .Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(3); // Document
                    columns.RelativeColumn(4); // Status
                });

                AddTextRow(
                    table,
                    "Trust PAN File",
                    trust.HasPANFile ? "Uploaded" : "Not Uploaded"
                );

                AddTextRow(
                    table,
                    "Bank Statement",
                    trust.HasBankStatementFile ? "Uploaded" : "Not Uploaded"
                );

                AddTextRow(
                    table,
                    "Registration Certificate",
                    trust.HasRegistrationCertificateFile
                        ? "Uploaded"
                        : "Not Uploaded"
                );

                AddTextRow(
                    table,
                    "Audit Statement",
                    trust.HasAuditStatementFile
                        ? "Uploaded"
                        : "Not Uploaded"
                );

                AddTextRow(
                    table,
                    "Amended Document",
                    trust.HasAmendedDoc
                        ? "Uploaded"
                        : "Not Uploaded"
                );

                AddTextRow(
                    table,
                    "GOK Order — Existing Courses",
                    trust.HasGokOrderExistingCoursesFile
                        ? "Uploaded"
                        : "Not Uploaded"
                );

                AddTextRow(
                    table,
                    "Registered Trust Member Details",
                    trust.HasRegisteredTrustMemberDetails
                        ? "Uploaded"
                        : "Not Uploaded"
                );

                AddTextRow(
                    table,
                    "Aadhaar File",
                    trust.HasAadhaarFile
                        ? "Uploaded"
                        : "Not Uploaded"
                );

                AddTextRow(
                    table,
                    "Gov Autonomous Certificate",
                    trust.HasGovAutonomousCertFile
                        ? "Uploaded"
                        : "Not Uploaded"
                );

                AddTextRow(
                    table,
                    "Gov Council Membership",
                    trust.HasGovCouncilMembershipFile
                        ? "Uploaded"
                        : "Not Uploaded"
                );

                AddTextRow(
                    table,
                    "First Affiliation Notification",
                    trust.HasFirstAffiliationNotifFile
                        ? "Uploaded"
                        : "Not Uploaded"
                );

                AddTextRow(
                    table,
                    "Continuation Affiliation",
                    trust.HasContinuationAffiliationFile
                        ? "Uploaded"
                        : "Not Uploaded"
                );

                AddTextRow(
                    table,
                    "KNC Certificate",
                    trust.HasKncCertificateFile
                        ? "Uploaded"
                        : "Not Uploaded"
                );

                AddTextRow(
                    table,
                    "DCI Certificate",
                    trust.HasDCIFile
                        ? "Uploaded"
                        : "Not Uploaded"
                );

                AddTextRow(
                    table,
                    "KSDC Certificate",
                    trust.HasKSDCFile
                        ? "Uploaded"
                        : "Not Uploaded"
                );
            });
    }

    private void AddAcademicIntakeCourseLevelSection(
    ColumnDescriptor col,
    string level,
    List<IntakeByLevelViewModel1> courses)
    {
        if (courses == null || !courses.Any())
            return;

        // =========================================================
        // DISPLAY LEVEL NAME
        // =========================================================

        var displayLevel = level?.Trim().ToUpper() switch
        {
            "UG" => "Under Graduate (UG)",
            "PG" => "Post Graduate (PG)",
            "SS" => "Super Specialty (SS)",
            _ => level ?? "Course"
        };

        // =========================================================
        // SECTION HEADING
        // =========================================================

        AddSubHeading(col, $"{displayLevel} Courses");

        // =========================================================
        // COURSE COUNT
        // =========================================================

        col.Item()
            .PaddingTop(4)
            .PaddingBottom(8)
            .Text(
                $"Intake details for {displayLevel} programmes.  |  {courses.Count} Course(s)"
            )
            .FontSize(9)
            .FontColor(PrimaryColor);

        // =========================================================
        // CHECK AY 2026-27
        // =========================================================

        var show2026 = courses.Any(x =>
            (x.AY2026_ExistingIntake ?? 0) > 0 ||
            (x.AY2026_AddRequestedIntake ?? 0) > 0 ||
            (x.AY2026_TotalIntake ?? 0) > 0
        );

        // =========================================================
        // TABLE
        // =========================================================

        col.Item()
            .PaddingTop(5)
            .Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    // Course
                    columns.RelativeColumn(3);

                    // AY 2025-26
                    columns.RelativeColumn(1.3f);
                    columns.RelativeColumn(1.3f);
                    columns.RelativeColumn(1.3f);

                    // AY 2026-27
                    if (show2026)
                    {
                        columns.RelativeColumn(1.3f);
                        columns.RelativeColumn(1.3f);
                        columns.RelativeColumn(1.3f);
                    }
                });

                // =================================================
                // HEADER
                // =================================================

                table.Header(header =>
                {
                    header.Cell()
                        .Border(1)
                        .BorderColor(BorderColor)
                        .Background(HeaderBgColor)
                        .Padding(5)
                        .AlignCenter()
                        .Text("Course")
                        .Bold()
                        .FontSize(8)
                        .FontColor(HeaderFgColor);

                    header.Cell()
                        .Border(1)
                        .BorderColor(BorderColor)
                        .Background(HeaderBgColor)
                        .Padding(5)
                        .AlignCenter()
                        .Text("AY 2025-26\nExisting")
                        .Bold()
                        .FontSize(8)
                        .FontColor(HeaderFgColor);

                    header.Cell()
                        .Border(1)
                        .BorderColor(BorderColor)
                        .Background(HeaderBgColor)
                        .Padding(5)
                        .AlignCenter()
                        .Text("AY 2025-26\nLoP / NMC")
                        .Bold()
                        .FontSize(8)
                        .FontColor(HeaderFgColor);

                    header.Cell()
                        .Border(1)
                        .BorderColor(BorderColor)
                        .Background(HeaderBgColor)
                        .Padding(5)
                        .AlignCenter()
                        .Text("AY 2025-26\nTotal")
                        .Bold()
                        .FontSize(8)
                        .FontColor(HeaderFgColor);

                    if (show2026)
                    {
                        header.Cell()
                            .Border(1)
                            .BorderColor(BorderColor)
                            .Background(HeaderBgColor)
                            .Padding(5)
                            .AlignCenter()
                            .Text("AY 2026-27\nExisting")
                            .Bold()
                            .FontSize(8)
                            .FontColor(HeaderFgColor);

                        header.Cell()
                            .Border(1)
                            .BorderColor(BorderColor)
                            .Background(HeaderBgColor)
                            .Padding(5)
                            .AlignCenter()
                            .Text("AY 2026-27\nRequested")
                            .Bold()
                            .FontSize(8)
                            .FontColor(HeaderFgColor);

                        header.Cell()
                            .Border(1)
                            .BorderColor(BorderColor)
                            .Background(HeaderBgColor)
                            .Padding(5)
                            .AlignCenter()
                            .Text("AY 2026-27\nTotal")
                            .Bold()
                            .FontSize(8)
                            .FontColor(HeaderFgColor);
                    }
                });

                // =================================================
                // BODY
                // =================================================

                int rowIndex = 0;

                foreach (var item in courses)
                {
                    var bg = RowBg(rowIndex);

                    // Course
                    table.Cell()
                        .Border(1)
                        .BorderColor(BorderColor)
                        .Background(bg)
                        .Padding(5)
                        .Text(text =>
                        {
                            text.Span(
                                string.IsNullOrWhiteSpace(item.CourseName)
                                    ? "—"
                                    : item.CourseName
                            )
                            .Bold()
                            .FontSize(9)
                            .FontColor(PrimaryColor);

                            text.EmptyLine();

                            text.Span(
                                string.IsNullOrWhiteSpace(item.CourseCode)
                                    ? "—"
                                    : item.CourseCode
                            )
                            .FontSize(8);
                        });

                    // AY 2025-26 Existing
                    table.Cell()
                        .Border(1)
                        .BorderColor(BorderColor)
                        .Background(bg)
                        .Padding(5)
                        .AlignCenter()
                        .Text((item.AY2025_ExistingIntake ?? 0).ToString())
                        .FontSize(9);

                    // AY 2025-26 LoP / NMC
                    table.Cell()
                        .Border(1)
                        .BorderColor(BorderColor)
                        .Background(bg)
                        .Padding(5)
                        .AlignCenter()
                        .Text((item.AY2025_LopNmcIntake ?? 0).ToString())
                        .FontSize(9);

                    // AY 2025-26 Total
                    table.Cell()
                        .Border(1)
                        .BorderColor(BorderColor)
                        .Background(bg)
                        .Padding(5)
                        .AlignCenter()
                        .Text((item.AY2025_TotalIntake ?? 0).ToString())
                        .Bold()
                        .FontSize(9);

                    // =================================================
                    // AY 2026-27
                    // =================================================

                    if (show2026)
                    {
                        // Existing
                        table.Cell()
                            .Border(1)
                            .BorderColor(BorderColor)
                            .Background(bg)
                            .Padding(5)
                            .AlignCenter()
                            .Text((item.AY2026_ExistingIntake ?? 0).ToString())
                            .FontSize(9);

                        // Requested
                        table.Cell()
                            .Border(1)
                            .BorderColor(BorderColor)
                            .Background(bg)
                            .Padding(5)
                            .AlignCenter()
                            .Text((item.AY2026_AddRequestedIntake ?? 0).ToString())
                            .FontSize(9);

                        // Total
                        table.Cell()
                            .Border(1)
                            .BorderColor(BorderColor)
                            .Background(bg)
                            .Padding(5)
                            .AlignCenter()
                            .Text((item.AY2026_TotalIntake ?? 0).ToString())
                            .Bold()
                            .FontSize(9);
                    }

                    rowIndex++;
                }
            });
    }

    private void AddAcademicIntakeSection(ColumnDescriptor col)
    {
        var intake = _model?.AcademicIntakeVM;

        if (intake == null ||
            intake.SortedCourseLevels == null ||
            !intake.SortedCourseLevels.Any())
        {
            return;
        }

        // =========================================================
        // MAIN SECTION HEADING
        // =========================================================

        AddMainHeading(col, "Academic Intake Details");

        // =========================================================
        // COURSE LEVELS
        // =========================================================

        foreach (var level in intake.SortedCourseLevels)
        {
            if (string.IsNullOrWhiteSpace(level))
                continue;

            var normalizedLevel = level.Trim().ToUpper();

            List<IntakeByLevelViewModel1>? courses = normalizedLevel switch
            {
                "UG" => intake.UgCourses,
                "PG" => intake.PgCourses,
                "SS" => intake.SsCourses,
                _ => null
            };

            if (courses == null || !courses.Any())
                continue;

            AddAcademicIntakeCourseLevelSection(
                col,
                normalizedLevel,
                courses
            );
        }
    }

    private IContainer AcademicIntakeHeaderCell(IContainer container)
    {
        return container
            .Border(1)
            .BorderColor("#D0D0D0")
            .Background("#E9ECEF")
            .Padding(5)
            .AlignMiddle();
    }

    private IContainer AcademicIntakeDataCell(IContainer container)
    {
        return container
            .Border(1)
            .BorderColor("#D0D0D0")
            .Padding(5)
            .AlignMiddle();
    }

    private void AddAcademicMattersSection(ColumnDescriptor col)
    {
        var academic = _model?.CAacademicMattersVM;

        if (academic == null)
            return;

        // =========================================================
        // MAIN HEADING
        // =========================================================

        AddMainHeading(col, "Academic Matters");

        // =========================================================
        // ACADEMIC PERFORMANCE
        // =========================================================

        if (academic.AcademicRows != null &&
            academic.AcademicRows.Any())
        {
            AddSubHeading(col, "Academic Performance");

            col.Item()
                .PaddingTop(8)
                .Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn();      // Year
                        columns.ConstantColumn(60);     // Regular
                        columns.ConstantColumn(70);     // Repeaters
                        columns.ConstantColumn(60);     // Passed
                        columns.ConstantColumn(60);     // Pass %
                        columns.ConstantColumn(70);     // First Class
                        columns.ConstantColumn(75);     // Distinction
                        columns.RelativeColumn();      // Remarks
                    });

                    // ===== HEADER =====

                    AddTableHeader(
                        table,
                        "Year",
                        "Regular",
                        "Repeaters",
                        "Passed",
                        "Pass %",
                        "First Class",
                        "Distinction",
                        "Remarks"
                    );

                    // ===== BODY =====

                    int rowIndex = 0;

                    foreach (var row in academic.AcademicRows)
                    {
                        var bg = RowBg(rowIndex);

                        table.Cell()
                            .Border(1)
                            .BorderColor(BorderColor)
                            .Background(bg)
                            .Padding(4)
                            .AlignCenter()
                            .Text(row.YearName ?? "—")
                            .FontSize(8);

                        table.Cell()
                            .Border(1)
                            .BorderColor(BorderColor)
                            .Background(bg)
                            .Padding(4)
                            .AlignCenter()
                            .Text(row.RegularStudents.ToString())
                            .FontSize(8);

                        table.Cell()
                            .Border(1)
                            .BorderColor(BorderColor)
                            .Background(bg)
                            .Padding(4)
                            .AlignCenter()
                            .Text(row.RepeaterStudents.ToString())
                            .FontSize(8);

                        table.Cell()
                            .Border(1)
                            .BorderColor(BorderColor)
                            .Background(bg)
                            .Padding(4)
                            .AlignCenter()
                            .Text(row.NumberOfStudentsPassed.ToString())
                            .FontSize(8);

                        table.Cell()
                            .Border(1)
                            .BorderColor(BorderColor)
                            .Background(bg)
                            .Padding(4)
                            .AlignCenter()
                            .Text((row.PassPercentage ?? 0).ToString("0.00"))
                            .FontSize(8);

                        table.Cell()
                            .Border(1)
                            .BorderColor(BorderColor)
                            .Background(bg)
                            .Padding(4)
                            .AlignCenter()
                            .Text(row.FirstClassCount.ToString())
                            .FontSize(8);

                        table.Cell()
                            .Border(1)
                            .BorderColor(BorderColor)
                            .Background(bg)
                            .Padding(4)
                            .AlignCenter()
                            .Text(row.DistinctionCount.ToString())
                            .FontSize(8);

                        table.Cell()
                            .Border(1)
                            .BorderColor(BorderColor)
                            .Background(bg)
                            .Padding(4)
                            .AlignCenter()
                            .Text(
                                string.IsNullOrWhiteSpace(row.Remarks)
                                    ? "—"
                                    : row.Remarks
                            )
                            .FontSize(8);

                        rowIndex++;
                    }
                });
        }

        // =========================================================
        // COURSE CURRICULUM
        // =========================================================

        if (academic.CourseCurriculumdvm != null &&
            academic.CourseCurriculumdvm.Any())
        {
            AddSubHeading(col, "Course Curriculum");

            col.Item()
                .PaddingTop(8)
                .Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(4);  // Curriculum
                        columns.RelativeColumn(4);  // Details
                        columns.ConstantColumn(80); // Uploaded
                    });

                    // ===== HEADER =====

                    AddTableHeader(
                        table,
                        "Curriculum",
                        "Details",
                        "Uploaded"
                    );

                    // ===== BODY =====

                    int rowIndex = 0;

                    foreach (var item in academic.CourseCurriculumdvm)
                    {
                        var bg = RowBg(rowIndex);

                        table.Cell()
                            .Border(1)
                            .BorderColor(BorderColor)
                            .Background(bg)
                            .Padding(5)
                            .Text(item.CurriculumName ?? "—")
                            .FontSize(9)
                            .FontColor(PrimaryColor);

                        table.Cell()
                            .Border(1)
                            .BorderColor(BorderColor)
                            .Background(bg)
                            .Padding(5)
                            .Text(item.CurriculumDetails ?? "—")
                            .FontSize(9);

                        table.Cell()
                            .Border(1)
                            .BorderColor(BorderColor)
                            .Background(bg)
                            .Padding(5)
                            .AlignCenter()
                            .Text(item.HasPdf ? "Yes" : "No")
                            .FontSize(9);

                        rowIndex++;
                    }
                });
        }

        // =========================================================
        // EXAMINATION SCHEMES
        // =========================================================

        if (academic.ExaminationSchemes != null &&
            academic.ExaminationSchemes.Any())
        {
            AddSubHeading(col, "Examination Schemes");

            col.Item()
                .PaddingTop(8)
                .Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn();       // Scheme Code
                        columns.ConstantColumn(120);     // Students
                    });

                    // ===== HEADER =====

                    AddTableHeader(
                        table,
                        "Scheme Code",
                        "Number of Students"
                    );

                    // ===== BODY =====

                    int rowIndex = 0;

                    foreach (var scheme in academic.ExaminationSchemes)
                    {
                        var bg = RowBg(rowIndex);

                        table.Cell()
                            .Border(1)
                            .BorderColor(BorderColor)
                            .Background(bg)
                            .Padding(5)
                            .Text(scheme.SchemeCode ?? "—")
                            .FontSize(9)
                            .FontColor(PrimaryColor);

                        table.Cell()
                            .Border(1)
                            .BorderColor(BorderColor)
                            .Background(bg)
                            .Padding(5)
                            .AlignCenter()
                            .Text(scheme.NumberOfStudents.ToString())
                            .FontSize(9);

                        rowIndex++;
                    }
                });
        }

        // =========================================================
        // STUDENT REGISTER RECORDS
        // =========================================================

        if (academic.StudentRegisterRecords != null &&
            academic.StudentRegisterRecords.Any())
        {
            AddSubHeading(col, "Student Register Records");

            col.Item()
                .PaddingTop(8)
                .Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn();       // Register Name
                        columns.ConstantColumn(120);     // Status
                    });

                    // ===== HEADER =====

                    AddTableHeader(
                        table,
                        "Register Name",
                        "Maintenance Status"
                    );

                    // ===== BODY =====

                    int rowIndex = 0;

                    foreach (var record in academic.StudentRegisterRecords)
                    {
                        var bg = RowBg(rowIndex);

                        table.Cell()
                            .Border(1)
                            .BorderColor(BorderColor)
                            .Background(bg)
                            .Padding(5)
                            .Text(record.RegisterName ?? "—")
                            .FontSize(9)
                            .FontColor(PrimaryColor);

                        table.Cell()
                            .Border(1)
                            .BorderColor(BorderColor)
                            .Background(bg)
                            .Padding(5)
                            .AlignCenter()
                            .Text(record.IsExists ? "Yes" : "No")
                            .FontSize(9);

                        rowIndex++;
                    }
                });
        }
    }

    private void AddDentalStaffDetailsSection(ColumnDescriptor col)
    {
        var staffDetails = _model?.DentalStaffDetailsVM;

        if (staffDetails == null)
            return;

        // =========================================================
        // MAIN HEADING
        // =========================================================

        AddMainHeading(col, "Dental Staff Details");

        // =========================================================
        // COURSE / FACULTY INFORMATION
        // =========================================================

        if (!string.IsNullOrWhiteSpace(staffDetails.CourseLevel) ||
            !string.IsNullOrWhiteSpace(staffDetails.CollegeCode) ||
            !string.IsNullOrWhiteSpace(staffDetails.FacultyCode))
        {
            AddSubHeading(col, "Staff Details Information");

            col.Item()
                .PaddingTop(8)
                .Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(3);
                        columns.RelativeColumn(4);
                    });

                    AddTextRow(
                        table,
                        "Course Level",
                        staffDetails.CourseLevel);

                    AddTextRow(
                        table,
                        "College Code",
                        staffDetails.CollegeCode);

                    AddTextRow(
                        table,
                        "Faculty Code",
                        staffDetails.FacultyCode);
                });
        }

        // =========================================================
        // STAFF PAY SCALE
        // =========================================================

        if (staffDetails.StaffPayScaleList != null &&
            staffDetails.StaffPayScaleList.Any())
        {
            AddSubHeading(col, "Staff Pay Scale");

            col.Item()
                .PaddingTop(8)
                .Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(35); // Sl No
                        columns.RelativeColumn(3);  // Staff
                        columns.RelativeColumn(2);  // Designation
                        columns.RelativeColumn(2);  // Pay Scale
                    });

                    AddTableHeader(
                        table,
                        "Sl. No.",
                        "Staff",
                        "Designation",
                        "Pay Scale"
                    );

                    int slNo = 1;

                    foreach (var staff in staffDetails.StaffPayScaleList)
                    {
                        var bg = RowBg(slNo - 1);

                        table.Cell()
                            .Border(1)
                            .BorderColor(BorderColor)
                            .Background(bg)
                            .Padding(4)
                            .AlignCenter()
                            .Text(slNo.ToString())
                            .FontSize(8);

                        // Update these property names according to
                        // Med_CA_StaffParticularsVM if different.
                        table.Cell()
                            .Border(1)
                            .BorderColor(BorderColor)
                            .Background(bg)
                            .Padding(4)
                            .Text(
                                "—"
                            )
                            .FontSize(8);

                        table.Cell()
                            .Border(1)
                            .BorderColor(BorderColor)
                            .Background(bg)
                            .Padding(4)
                            .Text(
                                "—"
                            )
                            .FontSize(8);

                        table.Cell()
                            .Border(1)
                            .BorderColor(BorderColor)
                            .Background(bg)
                            .Padding(4)
                            .Text(
                                "—"
                            )
                            .FontSize(8);

                        slNo++;
                    }
                });
        }

        // =========================================================
        // OTHER STAFF PARTICULARS
        // =========================================================

        if (staffDetails.StaffOther != null)
        {
            AddSubHeading(col, "Other Staff Particulars");

            col.Item()
                .PaddingTop(8)
                .Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(3);
                        columns.RelativeColumn(4);
                    });

                    // AddTextRow(table, "Property Name",
                    //     staffDetails.StaffOther.PropertyName);

                    // Add the actual StaffOther properties here.
                });
        }

        // =========================================================
        // STAFF DOCUMENTS
        // =========================================================

        bool hasDocuments =
            !string.IsNullOrWhiteSpace(staffDetails.ExaminerDetailsPdfName) ||
            !string.IsNullOrWhiteSpace(staffDetails.AEBASLastThreeMonthsPdfName) ||
            !string.IsNullOrWhiteSpace(staffDetails.AEBASInspectionDayPdfName) ||
            !string.IsNullOrWhiteSpace(staffDetails.ProvidentFundPdfName) ||
            !string.IsNullOrWhiteSpace(staffDetails.ESIPdfName);

        if (hasDocuments)
        {
            AddSubHeading(col, "Staff Documents");

            col.Item()
                .PaddingTop(8)
                .Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(4);
                        columns.RelativeColumn(2);
                    });

                    AddTextRow(
                        table,
                        "Examiner Details",
                        !string.IsNullOrWhiteSpace(
                            staffDetails.ExaminerDetailsPdfName)
                            ? "Uploaded"
                            : "—");

                    AddTextRow(
                        table,
                        "AEBAS Last Three Months",
                        !string.IsNullOrWhiteSpace(
                            staffDetails.AEBASLastThreeMonthsPdfName)
                            ? "Uploaded"
                            : "—");

                    AddTextRow(
                        table,
                        "AEBAS Inspection Day",
                        !string.IsNullOrWhiteSpace(
                            staffDetails.AEBASInspectionDayPdfName)
                            ? "Uploaded"
                            : "—");

                    AddTextRow(
                        table,
                        "Provident Fund",
                        !string.IsNullOrWhiteSpace(
                            staffDetails.ProvidentFundPdfName)
                            ? "Uploaded"
                            : "—");

                    AddTextRow(
                        table,
                        "ESI",
                        !string.IsNullOrWhiteSpace(
                            staffDetails.ESIPdfName)
                            ? "Uploaded"
                            : "—");
                });
        }
    }

    private void AddHospitalAffiliationSection(ColumnDescriptor col)
    {
        var hospital = _model?.CAHospitalAFfiliationCompVM;

        if (hospital == null)
            return;

        // =========================================================
        // MAIN SECTION HEADING
        // =========================================================

        AddMainHeading(col, "Hospital Affiliation");


        // =========================================================
        // 1. CLINICAL HOSPITAL DETAILS
        // =========================================================

        if (hospital.ClinicalDentalHospitalDetails != null)
        {
            var h = hospital.ClinicalDentalHospitalDetails;

            AddSubHeading(col, "Clinical Hospital Details");

            col.Item()
                .PaddingTop(8)
                .Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(3); // Label
                        columns.RelativeColumn(4); // Value
                    });

                    AddTextRow(
                        table,
                        "Hospital Name",
                        h.HospitalName
                    );

                    AddTextRow(
                        table,
                        "Hospital Type",
                        h.HospitalType
                    );

                    AddTextRow(
                        table,
                        "Hospital Owned By",
                        h.HospitalOwnedBy
                    );

                    AddTextRow(
                        table,
                        "Owner Name",
                        h.OwnerName
                    );

                    AddTextRow(
                        table,
                        "Location",
                        $"{h.DistrictName ?? "—"}, {h.TalukName ?? "—"}"
                    );

                    AddTextRow(
                        table,
                        "Total Beds",
                        h.TotalBeds.ToString()
                    );

                    AddTextRow(
                        table,
                        "OPD per Day",
                        h.OpdPerDay.ToString()
                    );

                    AddTextRow(
                        table,
                        "IPD Occupancy %",
                        h.IpdOccupancyPercent.ToString()
                    );

                    AddTextRow(
                        table,
                        "Member of Trust",
                        h.IsOwnerAmemberOfTrust ? "Yes" : "No"
                    );

                    AddTextRow(
                        table,
                        "Supporting Documents Uploaded",
                        h.IsSupportingDocExists ? "Yes" : "No"
                    );


                    // --------------------------------------------------
                    // Certificates
                    // --------------------------------------------------

                    AddTextRow(
                        table,
                        "KPME Certificate",
                        h.IsKPMECertificateExists ? "Yes" : "No");

                    AddTextRow(
                        table,
                        "Pollution Control Board Certificate",
                        h.IsPollutionControlBoardCertificateExists ? "Yes" : "No");

                    AddTextRow(
                        table,
                        "Bio-Medical Waste Certificate",
                        h.IsBioMedicalCertificateExists ? "Yes" : "No");

                    AddTextRow(
                        table,
                        "Drug Free Campus Certification",
                        h.IsDrugFreeCampusCertificationExists ? "Yes" : "No");


                    // --------------------------------------------------
                    // Proposed Plans
                    // --------------------------------------------------

                    AddTextRow(
                        table,
                        "Proposed Plans for Future Developments",
                        h.IsProposedPlansForFutureDevelopmentsExists ? "Yes" : "No");


                    // --------------------------------------------------
                    // Anatomy Act
                    // --------------------------------------------------

                    AddTextRow(
                        table,
                        "Registered Under Anatomy Act",
                        h.IsRegisteredUnderAnatomyAct ? "Yes" : "No");

                    AddTextRow(
                        table,
                        "Anatomy Act Registration Details",
                        h.AnatomyActRegistrationDetails ?? "—");


                    // --------------------------------------------------
                    // Tie-Up
                    // --------------------------------------------------

                    AddTextRow(
                        table,
                        "Hospital Tie-Up",
                        h.hasTieUp ? "Yes" : "No");
                });
        }


        // =========================================================
        // 2. AFFILIATED HOSPITAL DOCUMENTS
        // =========================================================

        if (hospital.AffiliatedHospitalDocuments?.Any() == true)
        {
            AddSubHeading(col, "Affiliated Documents");

            col.Item()
                .PaddingTop(8)
                .Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(3);   // Document
                        columns.RelativeColumn(3);   // Hospital
                        columns.ConstantColumn(60);  // Beds
                        columns.ConstantColumn(70);  // Exists
                    });

                    // ===== HEADER =====

                    AddTableHeader(
                        table,
                        "Document Name",
                        "Hospital Name",
                        "Beds",
                        "Exists"
                    );

                    // ===== BODY =====

                    int rowIndex = 0;

                    foreach (var doc in hospital.AffiliatedHospitalDocuments)
                    {
                        var bg = RowBg(rowIndex);

                        table.Cell()
                            .Border(1)
                            .BorderColor(BorderColor)
                            .Background(bg)
                            .Padding(5)
                            .Text(doc.DocumentName ?? "—")
                            .FontSize(9)
                            .FontColor(PrimaryColor);

                        table.Cell()
                            .Border(1)
                            .BorderColor(BorderColor)
                            .Background(bg)
                            .Padding(5)
                            .Text(doc.HospitalName ?? "—")
                            .FontSize(9);

                        table.Cell()
                            .Border(1)
                            .BorderColor(BorderColor)
                            .Background(bg)
                            .Padding(5)
                            .AlignCenter()
                            .Text(doc.TotalBeds.ToString())
                            .FontSize(9);

                        table.Cell()
                            .Border(1)
                            .BorderColor(BorderColor)
                            .Background(bg)
                            .Padding(5)
                            .AlignCenter()
                            .Text(doc.DocumentExists ? "Yes" : "No")
                            .FontSize(9);

                        rowIndex++;
                    }
                });
        }


        // =========================================================
        // 3. DISCIPLINE DETAILS
        // =========================================================

        if (hospital.DisciplineDetails != null &&
            hospital.DisciplineDetails.Disciplines?.Any() == true)
        {
            var discipline = hospital.DisciplineDetails;

            AddSubHeading(col, "Discipline Details");

            col.Item()
                .PaddingTop(8)
                .Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(40); // Sl.
                        columns.RelativeColumn();    // Discipline
                        columns.ConstantColumn(90);  // Available
                    });

                    // ===== HEADER =====

                    AddTableHeader(
                        table,
                        "Sl. No.",
                        "Discipline",
                        "Available"
                    );

                    // ===== BODY =====

                    int slNo = 1;

                    foreach (var item in discipline.Disciplines)
                    {
                        var bg = RowBg(slNo - 1);

                        table.Cell()
                            .Border(1)
                            .BorderColor(BorderColor)
                            .Background(bg)
                            .Padding(5)
                            .AlignCenter()
                            .Text(slNo.ToString())
                            .FontSize(9);

                        table.Cell()
                            .Border(1)
                            .BorderColor(BorderColor)
                            .Background(bg)
                            .Padding(5)
                            .Text(item.DisciplineName ?? "—")
                            .FontSize(9)
                            .FontColor(PrimaryColor);

                        table.Cell()
                            .Border(1)
                            .BorderColor(BorderColor)
                            .Background(bg)
                            .Padding(5)
                            .AlignCenter()
                            .Text(item.IsSelected ? "Yes" : "No")
                            .FontSize(9);

                        slNo++;
                    }
                });
        }


        // =========================================================
        // 4. ENGINEERING / ALLIED SERVICES
        // =========================================================

        if (hospital.EngAlliedServices != null &&
            hospital.EngAlliedServices.Requirements?.Any() == true)
        {
            var allied = hospital.EngAlliedServices;

            AddSubHeading(col, "Services");

            col.Item()
                .PaddingTop(8)
                .Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(40); // Sl.
                        columns.RelativeColumn();    // Requirement
                        columns.ConstantColumn(90);  // Available
                    });

                    // ===== HEADER =====

                    AddTableHeader(
                        table,
                        "Sl. No.",
                        "Requirement",
                        "Available"
                    );

                    // ===== BODY =====

                    int slNo = 1;

                    foreach (var item in allied.Requirements)
                    {
                        var bg = RowBg(slNo - 1);

                        table.Cell()
                            .Border(1)
                            .BorderColor(BorderColor)
                            .Background(bg)
                            .Padding(5)
                            .AlignCenter()
                            .Text(slNo.ToString())
                            .FontSize(9);

                        table.Cell()
                            .Border(1)
                            .BorderColor(BorderColor)
                            .Background(bg)
                            .Padding(5)
                            .Text(item.RequirementName ?? "—")
                            .FontSize(9)
                            .FontColor(PrimaryColor);

                        table.Cell()
                            .Border(1)
                            .BorderColor(BorderColor)
                            .Background(bg)
                            .Padding(5)
                            .AlignCenter()
                            .Text(
                                item.IsAvailable == true
                                    ? "Yes"
                                    : "No"
                            )
                            .FontSize(9);

                        slNo++;
                    }
                });
        }


        // =========================================================
        // 5. DENTAL WARD BED DISTRIBUTION
        // =========================================================

        if (hospital.DentalWardBedDistribution?.Any() == true)
        {
            AddSubHeading(col, "Dental Ward Bed Distribution");

            col.Item()
                .PaddingTop(8)
                .Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(40); // Sl.
                        columns.RelativeColumn();    // Ward
                        columns.ConstantColumn(80);  // Required
                        columns.ConstantColumn(80);  // Present
                    });

                    // ===== HEADER =====

                    AddTableHeader(
                        table,
                        "Sl. No.",
                        "Ward",
                        "Beds Required",
                        "Beds Present"
                    );

                    // ===== BODY =====

                    int slNo = 1;

                    foreach (var ward in hospital.DentalWardBedDistribution)
                    {
                        var bg = RowBg(slNo - 1);

                        table.Cell()
                            .Border(1)
                            .BorderColor(BorderColor)
                            .Background(bg)
                            .Padding(5)
                            .AlignCenter()
                            .Text(slNo.ToString())
                            .FontSize(9);

                        table.Cell()
                            .Border(1)
                            .BorderColor(BorderColor)
                            .Background(bg)
                            .Padding(5)
                            .Text(ward.WardName ?? "—")
                            .FontSize(9)
                            .FontColor(PrimaryColor);

                        table.Cell()
                            .Border(1)
                            .BorderColor(BorderColor)
                            .Background(bg)
                            .Padding(5)
                            .AlignCenter()
                            .Text(ward.BedsRequired.ToString())
                            .FontSize(9);

                        table.Cell()
                            .Border(1)
                            .BorderColor(BorderColor)
                            .Background(bg)
                            .Padding(5)
                            .AlignCenter()
                            .Text(
                                ward.BedsPresent?.ToString() ?? "—"
                            )
                            .FontSize(9);

                        slNo++;
                    }
                });
        }
    }

    private void AddDentalBedDistributionSection(ColumnDescriptor col)
    {
        var bedDistribution = _model?.DentalBedDistributionVM;

        if (bedDistribution == null)
            return;

        // =========================================================
        // MAIN HEADING
        // =========================================================

        AddMainHeading(col, "Dental Bed Distribution");

        // =========================================================
        // ORAL & MAXILLOFACIAL SURGERY
        // =========================================================

        AddSubHeading(col, "Oral & Maxillofacial Surgery");

        col.Item()
            .PaddingTop(8)
            .Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(3);
                    columns.RelativeColumn(1);
                });

                // =================================================
                // HEADER
                // =================================================

                AddTableHeader(
                    table,
                    "Bed Type",
                    "Beds"
                );

                // =================================================
                // BODY
                // =================================================

                var bg = RowBg(0);

                table.Cell()
                    .Border(1)
                    .BorderColor(BorderColor)
                    .Background(bg)
                    .Padding(5)
                    .Text("Oral & Maxillofacial Surgery")
                    .FontSize(9)
                    .FontColor(PrimaryColor);

                table.Cell()
                    .Border(1)
                    .BorderColor(BorderColor)
                    .Background(bg)
                    .Padding(5)
                    .AlignCenter()
                    .Text(
                        bedDistribution.OralMaxillofacialSurgery?.ToString() ?? "0"
                    )
                    .FontSize(9);
            });
    }

    private void AddWorkshopDetails(ColumnDescriptor col)
    {
        var workshops = _model?.WorkshopDetails;

        if (workshops == null || !workshops.Any())
            return;

        // =========================================================
        // MAIN HEADING
        // =========================================================

        AddMainHeading(col, "Workshop Details");

        col.Item()
            .PaddingTop(8)
            .Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(45);  // Sl No
                    columns.RelativeColumn(2);    // Staff
                    columns.RelativeColumn(3);    // Equipment
                    columns.RelativeColumn(4);    // Scope of Work
                });

                // =================================================
                // HEADER
                // =================================================

                AddTableHeader(
                    table,
                    "Sl. No.",
                    "Staff",
                    "Equipment",
                    "Scope of Work"
                );

                // =================================================
                // DATA
                // =================================================

                int slNo = 1;

                foreach (var workshop in workshops)
                {
                    var bg = RowBg(slNo - 1);

                    // Sl No
                    table.Cell()
                        .Border(1)
                        .BorderColor(BorderColor)
                        .Background(bg)
                        .Padding(5)
                        .AlignCenter()
                        .Text(slNo.ToString())
                        .FontSize(9);

                    // Staff
                    table.Cell()
                        .Border(1)
                        .BorderColor(BorderColor)
                        .Background(bg)
                        .Padding(5)
                        .Text(
                            string.IsNullOrWhiteSpace(workshop.Staff)
                                ? "—"
                                : workshop.Staff
                        )
                        .FontSize(9)
                        .FontColor(PrimaryColor);

                    // Equipment
                    table.Cell()
                        .Border(1)
                        .BorderColor(BorderColor)
                        .Background(bg)
                        .Padding(5)
                        .Text(
                            string.IsNullOrWhiteSpace(workshop.Equipment)
                                ? "—"
                                : workshop.Equipment
                        )
                        .FontSize(9);

                    // Scope of Work
                    table.Cell()
                        .Border(1)
                        .BorderColor(BorderColor)
                        .Background(bg)
                        .Padding(5)
                        .Text(
                            string.IsNullOrWhiteSpace(workshop.ScopeOfWork)
                                ? "—"
                                : workshop.ScopeOfWork
                        )
                        .FontSize(9);

                    slNo++;
                }
            });
    }

    private void AddAnimalHouseDetails(ColumnDescriptor col)
    {
        var animalHouses = _model?.AnimalHouseDetails;

        if (animalHouses == null || !animalHouses.Any())
            return;

        // =========================================================
        // MAIN HEADING
        // =========================================================

        AddMainHeading(col, "Animal House Details");

        col.Item()
            .PaddingTop(8)
            .Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(45);  // Sl No
                    columns.RelativeColumn(2);    // Area
                    columns.RelativeColumn(3);    // Staff
                    columns.RelativeColumn(4);    // Type of Animals
                });

                // =================================================
                // HEADER
                // =================================================

                AddTableHeader(
                    table,
                    "Sl. No.",
                    "Area (Sq.m)",
                    "Staff",
                    "Type of Animals"
                );

                // =================================================
                // DATA
                // =================================================

                int slNo = 1;

                foreach (var animalHouse in animalHouses)
                {
                    var bg = RowBg(slNo - 1);

                    // Sl No
                    table.Cell()
                        .Border(1)
                        .BorderColor(BorderColor)
                        .Background(bg)
                        .Padding(5)
                        .AlignCenter()
                        .Text(slNo.ToString())
                        .FontSize(9);

                    // Area
                    table.Cell()
                        .Border(1)
                        .BorderColor(BorderColor)
                        .Background(bg)
                        .Padding(5)
                        .AlignCenter()
                        .Text(
                            animalHouse.Area.HasValue
                                ? animalHouse.Area.Value.ToString("0.##")
                                : "—"
                        )
                        .FontSize(9);

                    // Staff
                    table.Cell()
                        .Border(1)
                        .BorderColor(BorderColor)
                        .Background(bg)
                        .Padding(5)
                        .Text(
                            string.IsNullOrWhiteSpace(animalHouse.Staff)
                                ? "—"
                                : animalHouse.Staff
                        )
                        .FontSize(9)
                        .FontColor(PrimaryColor);

                    // Type of Animals
                    table.Cell()
                        .Border(1)
                        .BorderColor(BorderColor)
                        .Background(bg)
                        .Padding(5)
                        .Text(
                            string.IsNullOrWhiteSpace(animalHouse.TypeOfAnimals)
                                ? "—"
                                : animalHouse.TypeOfAnimals
                        )
                        .FontSize(9);

                    slNo++;
                }
            });
    }

    private void AddDepartmentSections(ColumnDescriptor col)
    {
        var hospital = _model?.CAHospitalAFfiliationCompVM;

        if (hospital?.Sections == null || !hospital.Sections.Any())
            return;

        // =========================================================
        // DEPARTMENT / SECTION DETAILS
        // =========================================================

        foreach (var section in hospital.Sections)
        {
            if (section == null)
                continue;

            // =====================================================
            // SECTION HEADING
            // =====================================================

            if (!string.IsNullOrWhiteSpace(section.SectionName))
            {
                AddSubHeading(
                    col,
                    section.SectionName
                );
            }

            // =====================================================
            // SECTION ITEMS
            // =====================================================

            if (section.Items == null || !section.Items.Any())
                continue;

            col.Item()
                .PaddingTop(8)
                .Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(4);   // Requirement
                        columns.ConstantColumn(80);  // Compliant
                    });

                    // =============================================
                    // HEADER
                    // =============================================

                    AddTableHeader(
                        table,
                        "Requirement",
                        "Compliant"
                    );

                    // =============================================
                    // BODY
                    // =============================================

                    int rowIndex = 0;

                    foreach (var item in section.Items)
                    {
                        var bg = RowBg(rowIndex);

                        // Requirement
                        table.Cell()
                            .Border(1)
                            .BorderColor(BorderColor)
                            .Background(bg)
                            .Padding(5)
                            .Text(
                                string.IsNullOrWhiteSpace(item.RequirementName)
                                    ? "—"
                                    : item.RequirementName
                            )
                            .FontSize(9)
                            .FontColor(PrimaryColor);

                        // Compliant
                        table.Cell()
                            .Border(1)
                            .BorderColor(BorderColor)
                            .Background(bg)
                            .Padding(5)
                            .AlignCenter()
                            .Text(item.IsCompliant ? "Yes" : "No")
                            .FontSize(9);

                        rowIndex++;
                    }
                });
        }
    }


    private void AddIndoorBedsOccupancySection(ColumnDescriptor col)
    {
        var hospitalData = _model?.CAHospitalAFfiliationCompVM;

        if (hospitalData?.IndoorBedsOccupancy == null ||
            !hospitalData.IndoorBedsOccupancy.Any())
        {
            return;
        }

        // =========================================================
        // MAIN HEADING
        // =========================================================

        AddMainHeading(col, "Indoor Beds Occupancy");

        // =========================================================
        // TABLE
        // =========================================================

        col.Item()
            .PaddingTop(8)
            .Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(4);  // Department Name
                    columns.RelativeColumn(2);  // Seat Slab / Intake
                    columns.ConstantColumn(60); // RGUHS Intake
                    columns.ConstantColumn(60); // College Intake
                });

                // =================================================
                // HEADER
                // =================================================

                AddTableHeader(
                    table,
                    "Department Name",
                    "Seat Slab / Intake",
                    "RGUHS Intake",
                    "College Intake"
                );

                // =================================================
                // DATA
                // =================================================

                int rowIndex = 0;

                foreach (var item in hospitalData.IndoorBedsOccupancy)
                {
                    var bg = RowBg(rowIndex);

                    // Department Name
                    table.Cell()
                        .Border(1)
                        .BorderColor(BorderColor)
                        .Background(bg)
                        .Padding(5)
                        .Text(
                            string.IsNullOrWhiteSpace(item.DepartmentName)
                                ? "—"
                                : item.DepartmentName
                        )
                        .FontSize(9)
                        .FontColor(PrimaryColor);

                    // Seat Slab / Intake
                    table.Cell()
                        .Border(1)
                        .BorderColor(BorderColor)
                        .Background(bg)
                        .Padding(5)
                        .AlignCenter()
                        .Text(item.SeatSlabId.ToString())
                        .FontSize(9);

                    // RGUHS Intake
                    table.Cell()
                        .Border(1)
                        .BorderColor(BorderColor)
                        .Background(bg)
                        .Padding(5)
                        .AlignCenter()
                        .Text(item.RGUHSintake.ToString())
                        .FontSize(9);

                    // College Intake
                    table.Cell()
                        .Border(1)
                        .BorderColor(BorderColor)
                        .Background(bg)
                        .Padding(5)
                        .AlignCenter()
                        .Text(item.CollegeIntake.ToString())
                        .FontSize(9);

                    rowIndex++;
                }
            });
    }

    private void AddFieldPracticeAreaSection(ColumnDescriptor col)
    {
        var practiceAreas = _model?.FiedPracticeArea;

        if (practiceAreas == null || !practiceAreas.Any())
            return;

        // =========================================================
        // MAIN HEADING
        // =========================================================

        AddMainHeading(col, "Field Practice Area");

        // =========================================================
        // FIELD PRACTICE AREAS
        // =========================================================

        foreach (var practiceArea in practiceAreas)
        {
            if (practiceArea == null)
                continue;

            AddSubHeading(
                col,
                string.IsNullOrWhiteSpace(practiceArea.Location)
                    ? "Field Practice Area"
                    : practiceArea.Location
            );

            col.Item()
                .PaddingTop(8)
                .Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(3); // Label
                        columns.RelativeColumn(5); // Value
                    });

                    // =================================================
                    // DETAILS
                    // =================================================

                    AddTextRow(
                        table,
                        "Field Type",
                        practiceArea.FieldTypeId?.ToString()
                    );

                    AddTextRow(
                        table,
                        "Location",
                        practiceArea.Location
                    );

                    AddTextRow(
                        table,
                        "Address",
                        practiceArea.Address
                    );

                    AddTextRow(
                        table,
                        "Managed By",
                        practiceArea.ManagedBy
                    );

                    AddTextRow(
                        table,
                        "Staff List",
                        string.IsNullOrWhiteSpace(practiceArea.StaffList)
                            ? "Not Uploaded"
                            : "Uploaded"
                    );

                    AddTextRow(
                        table,
                        "Population Served",
                        practiceArea.PopulationServed?.ToString()
                    );

                    AddTextRow(
                        table,
                        "Activities and Services Provided",
                        practiceArea.ActivitiesAndServices
                    );

                    AddTextRow(
                        table,
                        "Records Maintained",
                        practiceArea.RecordsMaintained
                    );

                    AddTextRow(
                        table,
                        "Equipments Available",
                        practiceArea.EquipmentsAvailable
                    );

                    AddTextRow(
                        table,
                        "Training Activities",
                        practiceArea.TrainingActivities
                    );

                    AddTextRow(
                        table,
                        "How Supervision is Done",
                        practiceArea.SupervisionMethod
                    );

                    AddTextRow(
                        table,
                        "Trainee / Supervisor Accommodation",
                        practiceArea.TraineeSupervisorAccommodation
                    );
                });
        }
    }

    private static string FormatPeriod(DateOnly? from, DateOnly? to)
    {
        if (from == null && to == null)
            return "—";

        if (from != null && to != null)
            return $"{from:yyyy} - {to:yyyy}";

        if (from != null)
            return $"{from:yyyy} - Present";

        return "—";
    }

    //private void AddSkillsLabSection(ColumnDescriptor col)
    //{
    //    var lab = _model.PhysicalFacilities.SkillsLab;
    //    if (lab == null)
    //        return;

    //    // ===== MAIN HEADING =====
    //    col.Item().PaddingTop(30).Column(col2 =>
    //    {
    //        col2.Item()
    //            .AlignCenter()
    //            .Text("Skills Laboratory")
    //            .FontSize(14)
    //            .Bold();

    //    });

    //    // ===== SUBSECTION 1: Intake & Area =====
    //    AddSubHeading(col, "Intake and Area Details", 125);

    //    col.Item().PaddingTop(8).Table(table =>
    //    {
    //        table.ColumnsDefinition(c =>
    //        {
    //            c.RelativeColumn(3);
    //            c.RelativeColumn(2);
    //        });

    //        AddTextRow(table, "Annual MBBS Intake", lab.AnnualMbbsIntake);
    //        AddTextRow(table, "Total Area Required (Sq.m)", lab.TotalAreaRequiredSqm);
    //        AddTextRow(table, "Total Area Available (Sq.m)", lab.TotalAreaAvailableSqm);
    //        AddTextRow(table, "Area Deficiency (Sq.m)", lab.TotalAreaDeficiencySqm);
    //        AddYesNoNullableRow(table, "Six weeks training before clinical posting",
    //            lab.SixWeeksTrainingCompletedBeforeClinical);
    //    });

    //    // ===== SUBSECTION 2: Examination & Infrastructure =====
    //    AddSubHeading(col, "Examination Rooms and Infrastructure", 200);

    //    col.Item().PaddingTop(8).Table(table =>
    //    {
    //        table.ColumnsDefinition(c =>
    //        {
    //            c.RelativeColumn(4);
    //            c.ConstantColumn(90);
    //        });

    //        AddTextRow(table, "Number of examination rooms", lab.NumberOfExaminationRooms);
    //        AddYesNoNullableRow(table, "Minimum four examination rooms available",
    //            lab.HasMinFourExamRooms);
    //        AddYesNoNullableRow(table, "Demonstration room for small groups",
    //            lab.HasDemoRoomSmallGroups);
    //        AddYesNoNullableRow(table, "Debrief / review area available",
    //            lab.HasDebriefArea);
    //        AddYesNoNullableRow(table, "Faculty coordinator room available",
    //            lab.HasFacultyCoordinatorRoom);
    //        AddYesNoNullableRow(table, "Support staff room available",
    //            lab.HasSupportStaffRoom);
    //        AddYesNoNullableRow(table, "Storage for mannequins/equipment available",
    //            lab.HasStorageForMannequins);
    //        AddYesNoNullableRow(table, "Video recording & review facility available",
    //            lab.HasVideoRecordingFacility);
    //    });

    //    // ===== SUBSECTION 3: Skill Stations & Equipment =====
    //    AddSubHeading(col, "Skill Stations and Equipment");

    //    col.Item().PaddingTop(8).Table(table =>
    //    {
    //        table.ColumnsDefinition(c =>
    //        {
    //            c.RelativeColumn(4);
    //            c.ConstantColumn(90);
    //        });

    //        AddTextRow(table, "Number of skill stations", lab.NumberOfSkillStations);
    //        AddYesNoNullableRow(table, "Group and individual stations available",
    //            lab.HasGroupAndIndividualStations);
    //        AddYesNoNullableRow(table, "Required trainers and mannequins as per CBME",
    //            lab.HasRequiredTrainersAndMannequins);
    //    });

    //    // ===== SUBSECTION 4: Staffing & IT Facilities =====
    //    AddSubHeading(col, "Staffing and IT Facilities", 120);

    //    col.Item().PaddingTop(8).Table(table =>
    //    {
    //        table.ColumnsDefinition(c =>
    //        {
    //            c.RelativeColumn(4);
    //            c.ConstantColumn(90);
    //        });

    //        AddYesNoNullableRow(table, "Dedicated technical officer available",
    //            lab.HasDedicatedTechnicalOfficer);
    //        AddYesNoNullableRow(table, "Adequate support staff available",
    //            lab.HasAdequateSupportStaff);
    //        AddYesNoNullableRow(table, "Teaching areas have AV facilities",
    //            lab.TeachingAreasHaveAV);
    //        AddYesNoNullableRow(table, "Teaching areas have Internet",
    //            lab.TeachingAreasHaveInternet);
    //        AddYesNoNullableRow(table, "Skills lab enabled for E-learning",
    //            lab.SkillsLabEnabledForELearning);
    //    });
    //}


    private void AddLaboratoryEquipmentSection(ColumnDescriptor col)
    {
        var vm = _model.PhysicalFacilities.LaboratoryEquipment;
        if (vm == null || vm.Courses == null || !vm.Courses.Any())
            return;

        // ================= MAIN HEADING =================
        col.Item().PaddingTop(30).Column(col2 =>
        {
            col2.Item()
                .AlignCenter()
                .Text("Laboratory Equipment")
                .FontSize(14)
                .Bold();

        });

        // ================= COURSE LOOP =================
        foreach (var course in vm.Courses)
        {
            // ---- Course Heading ----
            //AddSubHeading(col, $"Course : {course.CourseCode}");

            foreach (var subject in course.Subjects)
            {
                // ---- Subject Heading ----
                col.Item().PaddingTop(10)
                    .Text($"Subject : {subject.Subject}")
                    .FontSize(11)
                    .Bold();

                // ---- Equipment Table ----
                col.Item().PaddingTop(6).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(4);  // Equipment Name
                        columns.ConstantColumn(80); // Required
                        columns.ConstantColumn(80); // Available
                        columns.ConstantColumn(80); // Status
                    });

                    // Header
                    table.Header(header =>
                    {
                        header.Cell().Border(1).Padding(5).Text("Equipment Name").Bold();
                        header.Cell().Border(1).Padding(5).AlignCenter().Text("Required").Bold();
                        header.Cell().Border(1).Padding(5).AlignCenter().Text("Available").Bold();
                        header.Cell().Border(1).Padding(5).AlignCenter().Text("Status").Bold();
                    });

                    // Rows
                    foreach (var eq in subject.Equipments)
                    {
                        table.Cell().Border(1).Padding(5)
                            .Text(eq.EquipmentName);

                        table.Cell().Border(1).Padding(5)
                            .AlignCenter()
                            .Text(eq.RequiredAsPerNorm.ToString());

                        table.Cell().Border(1).Padding(5)
                            .AlignCenter()
                            .Text(eq.AvailableQuantity?.ToString() ?? "0");

                        table.Cell().Border(1).Padding(5)
                            .AlignCenter()
                            .Text(eq.IsDeficient ? "Deficient" : "No Deficient");
                    }
                });
            }
        }

        // ================= SUMMARY =================
        //col.Item().PaddingTop(20).Text(
        //    $"Total Equipment Items: {vm.TotalEquipments}")
        //    .Bold();
    }

    private void AddSkillsLabEquipmentSection(ColumnDescriptor col)
    {
        var vm = _model?.PhysicalFacilities?.SkillsLabEquipment;

        if (vm?.Items == null || !vm.Items.Any())
            return;

        // =========================================================
        // MAIN HEADING
        // =========================================================

        AddMainHeading(col, "Skills Lab Equipment");

        // =========================================================
        // EQUIPMENT LIST
        // =========================================================

        AddSubHeading(col, "Equipment List");

        col.Item()
            .PaddingTop(8)
            .Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(4);  // Equipment Name
                    columns.ConstantColumn(80); // Required
                    columns.ConstantColumn(80); // Available
                    columns.ConstantColumn(60); // Quantity
                });

                // =================================================
                // HEADER
                // =================================================

                AddTableHeader(
                    table,
                    "Equipment Name",
                    "Required",
                    "Available",
                    "Qty"
                );

                // =================================================
                // DATA
                // =================================================

                int rowIndex = 0;

                foreach (var item in vm.Items)
                {
                    var bg = RowBg(rowIndex);

                    // Equipment Name
                    table.Cell()
                        .Border(1)
                        .BorderColor(BorderColor)
                        .Background(bg)
                        .Padding(5)
                        .Text(
                            string.IsNullOrWhiteSpace(item.Name)
                                ? "—"
                                : item.Name
                        )
                        .FontSize(9)
                        .FontColor(PrimaryColor);

                    // Required
                    table.Cell()
                        .Border(1)
                        .BorderColor(BorderColor)
                        .Background(bg)
                        .Padding(5)
                        .AlignCenter()
                        .Text(item.IsRequired ? "Yes" : "No")
                        .FontSize(9);

                    // Available
                    table.Cell()
                        .Border(1)
                        .BorderColor(BorderColor)
                        .Background(bg)
                        .Padding(5)
                        .AlignCenter()
                        .Text(item.IsAvailable ? "Yes" : "No")
                        .FontSize(9);

                    // Quantity
                    table.Cell()
                        .Border(1)
                        .BorderColor(BorderColor)
                        .Background(bg)
                        .Padding(5)
                        .AlignCenter()
                        .Text(item.Quantity?.ToString() ?? "—")
                        .FontSize(9);

                    rowIndex++;
                }
            });

        // =========================================================
        // ADDITIONAL FACILITIES
        // =========================================================

        if (vm.HasTrainingModulesForAllModels != null ||
            vm.UsesHybridModelsOrSimulations != null ||
            vm.HasComputerAssistedLearningSpace != null)
        {
            AddSubHeading(col, "Additional Facilities");

            col.Item()
                .PaddingTop(8)
                .Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(4);
                        columns.ConstantColumn(90);
                    });

                    AddYesNoNullableRow(
                        table,
                        "Training modules available for all models",
                        vm.HasTrainingModulesForAllModels
                    );

                    AddYesNoNullableRow(
                        table,
                        "Hybrid models or simulation-based training used",
                        vm.UsesHybridModelsOrSimulations
                    );

                    AddYesNoNullableRow(
                        table,
                        "Computer-assisted learning space available",
                        vm.HasComputerAssistedLearningSpace
                    );
                });
        }
    }

    private void AddDepartmentOfficesAndDeuSection(ColumnDescriptor col)
    {
        var model = _model?.DepartmentOfficesMeuVM;

        if (model == null)
            return;

        // =========================================================
        // MAIN HEADING
        // =========================================================

        AddMainHeading(
            col,
            "Department Offices & Dental Education Unit"
        );

        // =========================================================
        // DEPARTMENT OFFICE REQUIREMENTS
        // =========================================================

        AddSubHeading(
            col,
            "Department Office Requirements"
        );

        col.Item()
            .PaddingTop(8)
            .Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(4);
                    columns.ConstantColumn(90);
                });

                // -------------------------------------------------
                // Status Rows
                // -------------------------------------------------

                AddYesNoNullableRow(
                    table,
                    "HOD room with office and records",
                    model.HasHodRoomWithOfficeAndRecords
                );

                AddYesNoNullableRow(
                    table,
                    "Rooms for faculty and residents",
                    model.HasRoomsForFacultyAndResidents
                );

                AddYesNoNullableRow(
                    table,
                    "Faculty rooms have communication, computer and internet facilities",
                    model.FacultyRoomsHaveCommunicationComputerInternet
                );

                AddYesNoNullableRow(
                    table,
                    "Rooms for non-teaching staff",
                    model.HasRoomsForNonTeachingStaff
                );
            });

        // =========================================================
        // DENTAL EDUCATION UNIT
        // =========================================================

        if (model.Dental == null)
            return;

        AddSubHeading(
            col,
            "Dental Education Unit"
        );

        col.Item()
            .PaddingTop(8)
            .Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(4);
                    columns.RelativeColumn(2);
                });

                // =================================================
                // DEU AVAILABILITY
                // =================================================

                AddTextRow(
                    table,
                    "Dental Education Unit Available",
                    model.Dental.HasDentalEducationUnit.HasValue
                        ? (model.Dental.HasDentalEducationUnit.Value
                            ? "Yes"
                            : "No")
                        : "—"
                );

                // =================================================
                // DEU DETAILS
                // =================================================

                AddTextRow(
                    table,
                    "Dental Education Unit Area (Sq.m)",
                    FormatValue(model.Dental.DentalEducationUnitAreaSqm)
                );

                AddTextRow(
                    table,
                    "Audio Visual Facility",
                    model.Dental.DentalEducationUnitHasAudioVisual.HasValue
                        ? (model.Dental.DentalEducationUnitHasAudioVisual.Value
                            ? "Yes"
                            : "No")
                        : "—"
                );

                AddTextRow(
                    table,
                    "Internet Facility",
                    model.Dental.DentalEducationUnitHasInternet.HasValue
                        ? (model.Dental.DentalEducationUnitHasInternet.Value
                            ? "Yes"
                            : "No")
                        : "—"
                );

                // =================================================
                // COORDINATOR DETAILS
                // =================================================

                AddTextRow(
                    table,
                    "Coordinator Name",
                    model.Dental.DeuCoordinatorName
                );

                AddTextRow(
                    table,
                    "Coordinator Designation / Department",
                    model.Dental.DeuCoordinatorDesignationDepartment
                );

                AddTextRow(
                    table,
                    "Coordinator Phone",
                    model.Dental.DeuCoordinatorPhone
                );

                AddTextRow(
                    table,
                    "Coordinator Email",
                    model.Dental.DeuCoordinatorEmail
                );

                // =================================================
                // ACTIVITIES
                // =================================================

                AddTextRow(
                    table,
                    "Activities During Last Academic Year",
                    model.Dental.DeuActivitiesLastAcademicYear
                );

                // =================================================
                // MEMBERS LIST
                // =================================================

                AddTextRow(
                    table,
                    "Members List Uploaded",
                    model.Dental.HasDeuMembersListFile
                        ? "Yes"
                        : "No"
                );
            });
    }

    private void AddSmallGroupTeachingSection(ColumnDescriptor col)
    {
        var vm = _model.PhysicalFacilities.SmallGroupTeaching;
        if (vm == null)
            return;

        // ================= MAIN HEADING =================
        col.Item().PaddingTop(30).Column(col2 =>
        {
            col2.Item()
                .AlignCenter()
                .Text("Small Group Teaching")
                .FontSize(14)
                .Bold();

        });

        // ================= GENERAL DETAILS =================
        AddSubHeading(col, "General Details", 83);

        col.Item().PaddingTop(8).Table(table =>
        {
            table.ColumnsDefinition(c =>
            {
                c.RelativeColumn(4);
                c.ConstantColumn(100);
            });

            AddTextRow(table, "Annual MBBS Intake", vm.AnnualMbbsIntake.ToString());
            AddTextRow(table, "Small Group Batch Size", vm.SmallGroupBatchSize.ToString());

            AddYesNoNullableRow(table, "Teaching areas shared by all departments", vm.TeachingAreasSharedAllDepts);
            AddYesNoNullableRow(table, "AV available in all teaching areas", vm.AvInAllTeachingAreas);
            AddYesNoNullableRow(table, "Internet available in all teaching areas", vm.InternetInAllTeachingAreas);
            AddYesNoNullableRow(table, "Digitally linked teaching areas", vm.DigitalLinkAllTeachingAreas);
        });

        // ================= TEACHING ROOMS & AREA =================
        AddSubHeading(col, "Teaching Rooms & Area", 125);

        col.Item().PaddingTop(8).Table(table =>
        {
            table.ColumnsDefinition(c =>
            {
                c.RelativeColumn(4);
                c.ConstantColumn(100);
            });

            AddTextRow(table, "Number of students per small group", vm.SmallGroupStudents.ToString());
            AddTextRow(table, "Required area (sqm)", vm.RequiredAreaSqm.ToString("0.##"));
            AddTextRow(table, "Available area (sqm)", vm.AvailableAreaSqm.ToString("0.##"));
            AddTextRow(table, "Area deficiency (sqm)", vm.AreaDeficiencySqm.ToString("0.##"));

            AddYesNoNullableRow(table, "Rooms shared by all departments", vm.RoomsSharedByAllDepts);
            AddYesNoNullableRow(table, "Appropriate area for each specialty", vm.AppropriateAreaEachSpecialty);
            AddYesNoNullableRow(table, "Connected to lecture halls", vm.ConnectedToLectureHalls);
            AddYesNoNullableRow(table, "Internet available in teaching rooms", vm.InternetInTeachingRooms);
        });
    }

    private void AddStudentPracticalLabsSection(ColumnDescriptor col)
    {
        var vm = _model.PhysicalFacilities.SmallGroupStudentLabs;
        if (vm == null)
            return;

        // ================= MAIN HEADING =================
        col.Item().PaddingTop(30).Column(c =>
        {
            c.Item()
                .AlignCenter()
                .Text("Student Practical Laboratories")
                .FontSize(14)
                .Bold();
        });

        // ================= AVAILABILITY & SHARING =================
        AddSubHeading(col, "Laboratory Availability & Sharing", 170);

        col.Item().PaddingTop(8).Table(table =>
        {
            table.ColumnsDefinition(c =>
            {
                c.RelativeColumn(4);   // Lab name
                c.ConstantColumn(90);  // Available
                c.ConstantColumn(90);  // Shared
            });

            // Header
            table.Cell().Border(1).Padding(5).Text("Laboratory").Bold();
            table.Cell().Border(1).Padding(5).AlignCenter().Text("Available").Bold();
            table.Cell().Border(1).Padding(5).AlignCenter().Text("Shared").Bold();

            AddLabRow(table, "Histology", vm.HistologyAvailable, vm.HistologyShared);
            AddLabRow(table, "Clinical Physiology", vm.ClinicalPhysiologyAvailable, vm.ClinicalPhysiologyShared);
            AddLabRow(table, "Biochemistry", vm.BiochemistryAvailable, vm.BiochemistryShared);
            AddLabRow(table, "Histopathology / Cytopathology", vm.HistopathCytopathAvailable, vm.HistopathCytopathShared);
            AddLabRow(table, "Clinical Pathology / Hematology", vm.ClinPathHemeAvailable, vm.ClinPathHemeShared);
            AddLabRow(table, "Microbiology", vm.MicrobiologyAvailable, vm.MicrobiologyShared);
            AddLabRow(table, "Clinical Pharmacology", vm.ClinicalPharmAvailable, vm.ClinicalPharmShared);
            AddLabRow(table, "Community & Allied Pharmacology", vm.CalPharmAvailable, vm.CalPharmShared);
        });

        // ================= COMMON FACILITIES =================
        AddSubHeading(col, "Common Facilities", 100);

        col.Item().PaddingTop(8).Table(table =>
        {
            table.ColumnsDefinition(c =>
            {
                c.RelativeColumn(4);
                c.ConstantColumn(100);
            });

            AddYesNoNullableRow(table, "AV facilities available in all laboratories", vm.AllLabsHaveAV);
            AddYesNoNullableRow(table, "Internet available in all laboratories", vm.AllLabsHaveInternet);
            AddYesNoNullableRow(table, "Technical staff facilities ensured", vm.TechnicalStaffFacilitiesEnsured);
        });
    }


    private void AddMedicalLibrarySection(ColumnDescriptor col)
    {
        var library = _model?.MedicalLibraryPreviewVM;

        if (library == null)
            return;

        AddMainHeading(col, library.facultyCode == 2 ? "Dental Library" : "Medical Library");

        // =========================================================
        // USAGE REPORT
        // =========================================================

        if (library.HasUsageReport)
        {
            AddSubHeading(col, "Usage Report");

            col.Item()
                .PaddingTop(5)
                .Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(2);
                        columns.RelativeColumn(1);
                    });

                    table.Cell()
                        .Border(1)
                        .Padding(5)
                        .Text("Usage Report Uploaded")
                        .Bold();

                    table.Cell()
                        .Border(1)
                        .Padding(5)
                        .AlignCenter()
                        .Text("Yes");
                });
        }

        // =========================================================
        // LIBRARY STAFF
        // =========================================================

        if (library.LibraryStaff?.Any() == true)
        {
            AddLibraryStaffPdfSection(col, library.LibraryStaff);
        }

        // =========================================================
        // DEPARTMENT LIBRARIES
        // =========================================================

        if (library.DepartmentLibraries?.Any() == true)
        {
            AddDepartmentLibrariesPdfSection(col, library.DepartmentLibraries);
        }

        // =========================================================
        // OTHER DETAILS
        // =========================================================

        if (library.OtherDetails != null)
        {
            AddLibraryOtherDetailsPdfSection(col, library.OtherDetails);
        }

        // =========================================================
        // DENTAL LIBRARY RECORDS
        // =========================================================

        if (library.facultyCode == 2 &&
            library.DentalLibraryRecords?.Any() == true)
        {
            AddDentalLibraryRecordsPdfSection(col, library.DentalLibraryRecords);
        }

        // =========================================================
        // RESEARCH PUBLICATIONS
        // =========================================================

        if (library.ResearchPublications != null)
        {
            AddResearchPublicationsPdfSection(col, library.ResearchPublications);
        }

        // =========================================================
        // LIBRARY INFORMATION
        // =========================================================

        if (library.LibraryInformation != null)
        {
            AddLibraryInformationPdfSection(col, library.LibraryInformation);
        }
    }

    private void AddLibraryInformationPdfSection(ColumnDescriptor col, LibraryInformationPreviewVM model)
    {
        if (model == null)
            return;

        // =========================================================
        // GENERAL INFORMATION
        // =========================================================

        AddSubHeading(col, "Library General Information");

        col.Item()
            .PaddingTop(5)
            .Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(2);
                    columns.RelativeColumn(3);
                });

                AddTextRow(
                    table,
                    "Library Email ID",
                    model.General.LibraryEmailId ?? "—");

                AddTextRow(
                    table,
                    "Digital Library",
                    model.General.DigitalLibrary ?? "—");

                AddTextRow(
                    table,
                    "HELINET Services",
                    model.General.HelinetServices ?? "—");

                AddTextRow(
                    table,
                    "Department-wise Library",
                    model.General.DepartmentWiseLibrary ?? "—");
            });

        // =========================================================
        // LIBRARY HOLDINGS
        // =========================================================

        if (model.Items?.Any() == true)
        {
            AddSubHeading(col, "Library Holdings");

            col.Item()
                .PaddingTop(5)
                .Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(3);
                        columns.RelativeColumn(1);
                        columns.RelativeColumn(1);
                        columns.RelativeColumn(1);
                        columns.RelativeColumn(1);
                    });

                    table.Header(header =>
                    {
                        header.Cell().Border(1).Padding(4)
                            .Text("Item").Bold();

                        header.Cell().Border(1).Padding(4)
                            .AlignCenter()
                            .Text("Current Foreign").Bold();

                        header.Cell().Border(1).Padding(4)
                            .AlignCenter()
                            .Text("Current Indian").Bold();

                        header.Cell().Border(1).Padding(4)
                            .AlignCenter()
                            .Text("Previous Foreign").Bold();

                        header.Cell().Border(1).Padding(4)
                            .AlignCenter()
                            .Text("Previous Indian").Bold();
                    });

                    foreach (var item in model.Items)
                    {
                        table.Cell().Border(1).Padding(4)
                            .Text(item.ItemName ?? "—");

                        table.Cell().Border(1).Padding(4)
                            .AlignCenter()
                            .Text(item.CurrentForeign.ToString());

                        table.Cell().Border(1).Padding(4)
                            .AlignCenter()
                            .Text(item.CurrentIndian.ToString());

                        table.Cell().Border(1).Padding(4)
                            .AlignCenter()
                            .Text(item.PreviousForeign.ToString());

                        table.Cell().Border(1).Padding(4)
                            .AlignCenter()
                            .Text(item.PreviousIndian.ToString());
                    }
                });
        }

        // =========================================================
        // LIBRARY BUILDING
        // =========================================================

        AddSubHeading(col, "Library Building");

        col.Item()
            .PaddingTop(5)
            .Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(2);
                    columns.RelativeColumn(2);
                });

                AddTextRow(
                    table,
                    "Independent Library Building",
                    model.Building.IsIndependent ?? "—");

                AddTextRow(
                    table,
                    "Area (Sq. Mtrs.)",
                    model.Building.AreaSqMtrs?.ToString("0.##") ?? "—");
            });

        // =========================================================
        // TECHNICAL PROCESS
        // =========================================================

        if (model.TechnicalProcesses?.Any() == true)
        {
            AddSubHeading(col, "Technical Process");

            col.Item()
                .PaddingTop(5)
                .Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(3);
                        columns.RelativeColumn(2);
                    });

                    table.Header(header =>
                    {
                        header.Cell().Border(1).Padding(4)
                            .Text("Process").Bold();

                        header.Cell().Border(1).Padding(4)
                            .Text("Value").Bold();
                    });

                    foreach (var item in model.TechnicalProcesses)
                    {
                        table.Cell().Border(1).Padding(4)
                            .Text(item.ProcessName ?? "—");

                        table.Cell().Border(1).Padding(4)
                            .Text(item.Value ?? "—");
                    }
                });
        }

        // =========================================================
        // LIBRARY EQUIPMENTS
        // =========================================================

        if (model.Equipments?.Any() == true)
        {
            AddSubHeading(col, "Library Equipments");

            col.Item()
                .PaddingTop(5)
                .Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(3);
                        columns.RelativeColumn(1);
                    });

                    table.Header(header =>
                    {
                        header.Cell().Border(1).Padding(4)
                            .Text("Equipment").Bold();

                        header.Cell().Border(1).Padding(4)
                            .AlignCenter()
                            .Text("Available").Bold();
                    });

                    foreach (var item in model.Equipments)
                    {
                        table.Cell().Border(1).Padding(4)
                            .Text(item.EquipmentName ?? "—");

                        table.Cell().Border(1).Padding(4)
                            .AlignCenter()
                            .Text(item.HasEquipment ?? "—");
                    }
                });
        }

        // =========================================================
        // FINANCE
        // =========================================================

        AddSubHeading(col, "Library Finance");

        col.Item()
            .PaddingTop(5)
            .Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(2);
                    columns.RelativeColumn(2);
                });

                AddTextRow(
                    table,
                    "Total Budget (Lakhs)",
                    model.Finance.TotalBudgetLakhs?
                        .ToString("0.##") ?? "—");

                AddTextRow(
                    table,
                    "Expenditure on Books (Lakhs)",
                    model.Finance.ExpenditureBooksLakhs?
                        .ToString("0.##") ?? "—");
            });
    }

    private void AddResearchPublicationsPdfSection(ColumnDescriptor col, ResearchPublicationsPreviewVM model)
    {
        if (model == null)
            return;

        // =========================================================
        // RESEARCH PUBLICATIONS
        // =========================================================

        AddSubHeading(col, "Research Publications");

        col.Item()
            .PaddingTop(5)
            .Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(2);
                    columns.RelativeColumn(1);
                });

                AddTextRow(
                    table,
                    "Number of Publications",
                    model.PublicationsNo.ToString());

                AddTextRow(
                    table,
                    "Publications PDF",
                    model.HasPublicationsPdf
                        ? "Uploaded"
                        : "No");

                AddTextRow(
                    table,
                    "Clinical Trials PDF",
                    model.HasClinicalTrialsPdf
                        ? "Uploaded"
                        : "No");
            });

        // =========================================================
        // STUDENT RESEARCH PROJECTS
        // =========================================================

        AddSubHeading(col, "Student Research Projects");

        col.Item()
            .PaddingTop(5)
            .Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(2);
                    columns.RelativeColumn(1);
                });

                AddTextRow(
                    table,
                    "RGUHS Funded",
                    model.StudentsRGUHSFunded?.ToString() ?? "—");

                AddTextRow(
                    table,
                    "External Body Funding",
                    model.StudentsExternalBodyFunding?.ToString() ?? "—");

                AddTextRow(
                    table,
                    "Projects PDF",
                    model.HasStudentProjectsPdf
                        ? "Uploaded"
                        : "No");
            });

        // =========================================================
        // FACULTY RESEARCH PROJECTS
        // =========================================================

        AddSubHeading(col, "Faculty Research Projects");

        col.Item()
            .PaddingTop(5)
            .Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(2);
                    columns.RelativeColumn(1);
                });

                AddTextRow(
                    table,
                    "RGUHS Funded",
                    model.FacultyRGUHSFunded?.ToString() ?? "—");

                AddTextRow(
                    table,
                    "External Body Funding",
                    model.FacultyExternalBodyFunding?.ToString() ?? "—");

                AddTextRow(
                    table,
                    "Projects PDF",
                    model.HasFacultyProjectsPdf
                        ? "Uploaded"
                        : "No");
            });

        // =========================================================
        // OTHER ACADEMIC ACTIVITIES
        // =========================================================

        if (model.OtherActivities?.Any() == true)
        {
            AddSubHeading(col, "Other Academic Activities");

            col.Item()
                .PaddingTop(5)
                .Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(2);
                        columns.RelativeColumn(2);
                        columns.RelativeColumn(1.5f);
                        columns.RelativeColumn(1);
                    });

                    table.Header(header =>
                    {
                        header.Cell().Border(1).Padding(4)
                            .Text("Activity").Bold();

                        header.Cell().Border(1).Padding(4)
                            .Text("Department").Bold();

                        header.Cell().Border(1).Padding(4)
                            .Text("Department Wise").Bold();

                        header.Cell().Border(1).Padding(4)
                            .AlignCenter()
                            .Text("Document").Bold();
                    });

                    foreach (var item in model.OtherActivities)
                    {
                        table.Cell().Border(1).Padding(4)
                            .Text(item.ActivityName ?? "—");

                        table.Cell().Border(1).Padding(4)
                            .Text(item.DepartmentName ?? "—");

                        table.Cell().Border(1).Padding(4)
                            .Text(item.DepartmentWise?.ToString() ?? "—");

                        table.Cell().Border(1).Padding(4)
                            .AlignCenter()
                            .Text(item.HasDocument
                                ? "Uploaded"
                                : "No");
                    }
                });
        }

        // =========================================================
        // COMMITTEES
        // =========================================================

        if (model.Committees?.Any() == true)
        {
            AddSubHeading(col, "Committees");

            col.Item()
                .PaddingTop(5)
                .Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(3);
                        columns.RelativeColumn(1);
                        columns.RelativeColumn(1);
                    });

                    table.Header(header =>
                    {
                        header.Cell().Border(1).Padding(4)
                            .Text("Committee").Bold();

                        header.Cell().Border(1).Padding(4)
                            .AlignCenter()
                            .Text("Available").Bold();

                        header.Cell().Border(1).Padding(4)
                            .AlignCenter()
                            .Text("Document").Bold();
                    });

                    foreach (var item in model.Committees)
                    {
                        table.Cell().Border(1).Padding(4)
                            .Text(item.CommitteeName ?? "—");

                        table.Cell().Border(1).Padding(4)
                            .AlignCenter()
                            .Text(item.IsPresent ?? "-");

                        table.Cell().Border(1).Padding(4)
                            .AlignCenter()
                            .Text(item.HasDocument
                                ? "Uploaded"
                                : "No");
                    }
                });
        }

        // =========================================================
        // DEPARTMENT PUBLICATIONS
        // =========================================================

        if (model.DepartmentPublications?.Any() == true)
        {
            AddSubHeading(col, "Department-wise Publications");

            col.Item()
                .PaddingTop(5)
                .Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(3);
                        columns.RelativeColumn(1);
                        columns.RelativeColumn(1);
                    });

                    table.Header(header =>
                    {
                        header.Cell().Border(1).Padding(4)
                            .Text("Department").Bold();

                        header.Cell().Border(1).Padding(4)
                            .AlignCenter()
                            .Text("Publication Count").Bold();

                        header.Cell().Border(1).Padding(4)
                            .AlignCenter()
                            .Text("Document").Bold();
                    });

                    foreach (var item in model.DepartmentPublications)
                    {
                        table.Cell().Border(1).Padding(4)
                            .Text(item.DepartmentName ?? "—");

                        table.Cell().Border(1).Padding(4)
                            .AlignCenter()
                            .Text(item.PublicationsCount.ToString());

                        table.Cell().Border(1).Padding(4)
                            .AlignCenter()
                            .Text(item.HasDocument
                                ? "Uploaded"
                                : "No");
                    }
                });
        }
    }


    private void AddDentalLibraryRecordsPdfSection(ColumnDescriptor col, List<DentalLibraryRecordPreviewVM> records)
    {
        AddSubHeading(col, "Dental Library Records");

        col.Item()
            .PaddingTop(5)
            .Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(3);
                    columns.RelativeColumn(1);
                });

                table.Header(header =>
                {
                    header.Cell()
                        .Border(1)
                        .Padding(5)
                        .Text("Record")
                        .Bold();

                    header.Cell()
                        .Border(1)
                        .Padding(5)
                        .AlignCenter()
                        .Text("Document")
                        .Bold();
                });

                foreach (var item in records)
                {
                    table.Cell()
                        .Border(1)
                        .Padding(5)
                        .Text(item.RecordName ?? "—");

                    table.Cell()
                        .Border(1)
                        .Padding(5)
                        .AlignCenter()
                        .Text(item.HasDocument
                            ? "Uploaded"
                            : "No");
                }
            });
    }


    private void AddLibraryStaffPdfSection(ColumnDescriptor col, List<LibraryStaffPreviewVM> staff)
    {
        AddSubHeading(col, "Library Staff");

        col.Item()
            .PaddingTop(5)
            .Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(2);
                    columns.RelativeColumn(2);
                    columns.RelativeColumn(2);
                    columns.RelativeColumn(1);
                    columns.RelativeColumn(1.5f);
                });

                table.Header(header =>
                {
                    header.Cell().Border(1).Padding(5)
                        .Text("Name").Bold();

                    header.Cell().Border(1).Padding(5)
                        .Text("Designation").Bold();

                    header.Cell().Border(1).Padding(5)
                        .Text("Qualification").Bold();

                    header.Cell().Border(1).Padding(5)
                        .AlignCenter()
                        .Text("Experience").Bold();

                    header.Cell().Border(1).Padding(5)
                        .Text("Category").Bold();
                });

                foreach (var item in staff)
                {
                    table.Cell().Border(1).Padding(5)
                        .Text(item.StaffName ?? "—");

                    table.Cell().Border(1).Padding(5)
                        .Text(item.Designation ?? "—");

                    table.Cell().Border(1).Padding(5)
                        .Text(item.Qualification ?? "—");

                    table.Cell().Border(1).Padding(5)
                        .AlignCenter()
                        .Text(item.Experience?.ToString() ?? "—");

                    table.Cell().Border(1).Padding(5)
                        .Text(item.Category ?? "—");
                }
            });
    }


    private void AddDepartmentLibrariesPdfSection(ColumnDescriptor col, List<DepartmentLibraryPreviewVM> departments)
    {
        AddSubHeading(col, "Department Libraries");

        col.Item()
            .PaddingTop(5)
            .Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(2);
                    columns.RelativeColumn(1);
                    columns.RelativeColumn(1);
                    columns.RelativeColumn(1);
                    columns.RelativeColumn(1);
                    columns.RelativeColumn(1);
                    columns.RelativeColumn(1);
                    columns.RelativeColumn(1);
                });

                table.Header(header =>
                {
                    header.Cell().Border(1).Padding(4)
                        .Text("Department").Bold();

                    header.Cell().Border(1).Padding(4)
                        .AlignCenter()
                        .Text("Total Books").Bold();

                    header.Cell().Border(1).Padding(4)
                        .AlignCenter()
                        .Text("Books Added").Bold();

                    header.Cell().Border(1).Padding(4)
                        .AlignCenter()
                        .Text("Current Journals").Bold();

                    header.Cell().Border(1).Padding(4)
                        .AlignCenter()
                        .Text("Titles").Bold();

                    header.Cell().Border(1).Padding(4)
                        .AlignCenter()
                        .Text("International Journals").Bold();

                    header.Cell().Border(1).Padding(4)
                        .AlignCenter()
                        .Text("Back Volumes").Bold();

                    header.Cell().Border(1).Padding(4)
                        .AlignCenter()
                        .Text("Print Journal %").Bold();
                });

                foreach (var item in departments)
                {
                    table.Cell().Border(1).Padding(4)
                        .Text(item.DepartmentName ?? "—");

                    table.Cell().Border(1).Padding(4)
                        .AlignCenter()
                        .Text(item.TotalBooks?.ToString() ?? "—");

                    table.Cell().Border(1).Padding(4)
                        .AlignCenter()
                        .Text(item.BooksAddedInYear?.ToString() ?? "—");

                    table.Cell().Border(1).Padding(4)
                        .AlignCenter()
                        .Text(item.CurrentJournals?.ToString() ?? "—");

                    table.Cell().Border(1).Padding(4)
                        .AlignCenter()
                        .Text(item.Titles?.ToString() ?? "—");

                    table.Cell().Border(1).Padding(4)
                        .AlignCenter()
                        .Text(item.InternationalJournals?.ToString() ?? "—");

                    table.Cell().Border(1).Padding(4)
                        .AlignCenter()
                        .Text(item.BackVolumes?.ToString() ?? "—");

                    table.Cell().Border(1).Padding(4)
                        .AlignCenter()
                        .Text(item.PrintJournalPercentage?.ToString() ?? "—");
                }
            });
    }
    private void AddLibraryOtherDetailsPdfSection(ColumnDescriptor col, MedicalLibraryOtherPreviewVM other)
    {
        AddSubHeading(col, "Other Details");

        col.Item()
            .PaddingTop(5)
            .Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(2);
                    columns.RelativeColumn(1);
                });

                AddTextRow(
                    table,
                    "Digital Valuation Centre",
                    other.HasDigitalValuationCentre ?? "—");

                AddTextRow(
                    table,
                    "No. of Systems",
                    other.NoOfSystems?.ToString() ?? "—");

                AddTextRow(
                    table,
                    "Stable Internet",
                    other.HasStableInternet ?? "—");

                AddTextRow(
                    table,
                    "CCTV Camera System",
                    other.HasCccameraSystem ?? "—");

                AddTextRow(
                    table,
                    "Special Features",
                    other.HasSpecialFeatures
                        ? "Yes"
                        : "No");
            });
    }


    private void AddLibraryServicesSection(ColumnDescriptor col)
    {
        var library = _model.LibraryDisplay;
        var services = library?.caAffMedicalLibraryvm?.LibraryServices;

        if (services == null || !services.Any())
            return;

        // ================= MAIN HEADING =================
        col.Item().PaddingTop(30).Column(c =>
        {
            c.Item()
                .AlignCenter()
                .Text("Library")
                .FontSize(14)
                .Bold();
        });

        // ================= SUB HEADING =================
        AddSubHeading(col, "Library Services", 85);

        // ================= TABLE =================
        col.Item().PaddingTop(8).Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(4);  // Service Name
                columns.ConstantColumn(100); // Available
                columns.ConstantColumn(140); // Document
            });

            // ---------- Header ----------
            table.Header(header =>
            {
                header.Cell().Border(1).Padding(5).Text("Service").Bold();
                header.Cell().Border(1).Padding(5).AlignCenter().Text("Available").Bold();
                header.Cell().Border(1).Padding(5).AlignCenter().Text("Supporting Document").Bold();
            });

            // ---------- Rows ----------
            foreach (var service in services)
            {
                table.Cell().Border(1).Padding(5)
                    .Text(service.ServiceName);

                table.Cell().Border(1).Padding(5).AlignCenter()
                    .Text(string.IsNullOrWhiteSpace(service.IsAvailable) ? "—" : service.IsAvailable);

                table.Cell().Border(1).Padding(5).AlignCenter()
                    .Text(service.HasPdf ? "Available" : "—");
            }
        });
    }

    private void AddLibraryUsageReportSection(ColumnDescriptor col)
    {
        var library = _model.LibraryDisplay;
        var usageVm = library?.caAffMedicalLibraryvm;

        if (usageVm == null)
            return;

        bool hasUsageReport =
            !string.IsNullOrWhiteSpace(usageVm.ExistingUsageReportFileName);

        // ================= SUB HEADING =================
        AddSubHeading(col, "Library Usage Report", 112);

        // ================= TABLE =================
        col.Item().PaddingTop(8).Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(4);   // Parameter
                columns.ConstantColumn(100); // Status
                columns.ConstantColumn(140); // Document
            });

            // ---------- Header ----------
            table.Header(header =>
            {
                header.Cell().Border(1).Padding(5).Text("Parameter").Bold();
                header.Cell().Border(1).Padding(5).AlignCenter().Text("Available").Bold();
                header.Cell().Border(1).Padding(5).AlignCenter().Text("Supporting Document").Bold();
            });

            // ---------- Row ----------
            table.Cell().Border(1).Padding(5)
                .Text("Library Usage Report (Last Academic Year)");

            table.Cell().Border(1).Padding(5).AlignCenter()
                .Text(hasUsageReport ? "Yes" : "No");

            table.Cell().Border(1).Padding(5).AlignCenter()
                .Text(hasUsageReport ? "Available" : "—");
        });
    }

    private void AddLibraryStaffSection(ColumnDescriptor col)
    {
        var library = _model.LibraryDisplay;
        var staffList = library?.caAffMedicalLibraryvm?.LibraryStaff;

        if (staffList == null || !staffList.Any())
            return;

        // ================= SUB HEADING =================
        AddSubHeading(col, "Library Staff", 65);

        // ================= TABLE =================
        col.Item().PaddingTop(8).Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(3);   // Name
                columns.RelativeColumn(3);   // Designation
                columns.RelativeColumn(3);   // Qualification
                columns.ConstantColumn(80);  // Experience
                columns.RelativeColumn(2);   // Category
            });

            // ---------- Header ----------
            table.Header(header =>
            {
                header.Cell().Border(1).Padding(5).Text("Staff Name").Bold();
                header.Cell().Border(1).Padding(5).Text("Designation").Bold();
                header.Cell().Border(1).Padding(5).Text("Qualification").Bold();
                header.Cell().Border(1).Padding(5).AlignCenter().Text("Experience (Years)").Bold();
                header.Cell().Border(1).Padding(5).Text("Category").Bold();
            });

            // ---------- Rows ----------
            foreach (var staff in staffList.Where(s => !s.IsDeleted))
            {
                table.Cell().Border(1).Padding(5)
                    .Text(staff.StaffName ?? "—");

                table.Cell().Border(1).Padding(5)
                    .Text(staff.Designation ?? "—");

                table.Cell().Border(1).Padding(5)
                    .Text(staff.Qualification ?? "—");

                table.Cell().Border(1).Padding(5).AlignCenter()
                    .Text(staff.Experience?.ToString() ?? "—");

                table.Cell().Border(1).Padding(5)
                    .Text(staff.Category ?? "—");
            }
        });
    }

    private void AddDepartmentalLibrarySection(ColumnDescriptor col)
    {
        var library = _model.LibraryDisplay;
        var departments = library?.caAffMedicalLibraryvm?.DepartmentLibraries;

        if (departments == null || !departments.Any())
            return;

        // ================= SUB HEADING =================
        AddSubHeading(col, "Departmental Library", 115);

        // ================= TABLE =================
        col.Item().PaddingTop(8).Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(3);   // Department
                columns.ConstantColumn(80);  // Total Books
                columns.ConstantColumn(100); // Books Added
                columns.ConstantColumn(110); // Journals
                columns.RelativeColumn(2);   // Staff
            });

            // ---------- Header ----------
            table.Header(header =>
            {
                header.Cell().Border(1).Padding(5).Text("Department").Bold();
                header.Cell().Border(1).Padding(5).AlignCenter().Text("Total Books").Bold();
                header.Cell().Border(1).Padding(5).AlignCenter().Text("Books Added (Year)").Bold();
                header.Cell().Border(1).Padding(5).AlignCenter().Text("Current Journals").Bold();
                header.Cell().Border(1).Padding(5).Text("Library Staff").Bold();
            });

            // ---------- Rows ----------
            foreach (var dept in departments.Where(d => !d.IsDeleted))
            {
                table.Cell().Border(1).Padding(5)
                    .Text(dept.DepartmentName ?? "—");

                table.Cell().Border(1).Padding(5).AlignCenter()
                    .Text(dept.TotalBooks?.ToString() ?? "—");

                table.Cell().Border(1).Padding(5).AlignCenter()
                    .Text(dept.BooksAddedInYear?.ToString() ?? "—");

                table.Cell().Border(1).Padding(5).AlignCenter()
                    .Text(dept.CurrentJournals?.ToString() ?? "—");

                // Combine staff names cleanly
                var staffNames = string.Join(", ",
                    new[] { dept.LibraryStaff1, dept.LibraryStaff2 }
                        .Where(s => !string.IsNullOrWhiteSpace(s)));

                table.Cell().Border(1).Padding(5)
                    .Text(string.IsNullOrWhiteSpace(staffNames) ? "—" : staffNames);
            }
        });
    }


    private void AddLibraryCommitteeSection(ColumnDescriptor col)
    {
        var committees = _model.LibraryDisplay?.librarayCommitteeVM?.Committees;

        if (committees == null || !committees.Any())
            return;

        // ================= SUB HEADING =================
        AddSubHeading(col, "Library Committee Details", 140);

        // ================= TABLE =================
        col.Item().PaddingTop(8).Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(4); // Committee Name
                columns.RelativeColumn(2); // Available
                columns.RelativeColumn(2); // Document
            });

            // ---------- HEADER ----------
            table.Header(header =>
            {
                header.Cell().Border(1).Padding(5).Text("Committee Name").Bold();
                header.Cell().Border(1).Padding(5).AlignCenter().Text("Available").Bold();
                header.Cell().Border(1).Padding(5).AlignCenter().Text("Supporting Document").Bold();
            });

            // ---------- ROWS ----------
            foreach (var committee in committees)
            {
                table.Cell().Border(1).Padding(5)
                    .Text(committee.CommitteeName ?? "—");

                table.Cell().Border(1).Padding(5).AlignCenter()
                    .Text(committee.IsPresent ? "Yes" : "No");

                table.Cell().Border(1).Padding(5).AlignCenter()
                    .Text(committee.HasCommitteePdf ? "Available" : "—");
            }
        });
    }


    private void AddLibraryGeneralDetailsSection(ColumnDescriptor col)
    {
        var general = _model.LibraryDisplay?.LibraryGeneralVM;

        if (general == null)
            return;

        // ================= SUB HEADING =================
        AddSubHeading(col, "Library General Details", 120);

        // ================= TABLE =================
        col.Item().PaddingTop(8).Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(3); // Label
                columns.RelativeColumn(2); // Value
            });

            // Library Email
            AddTextRow(
                table,
                "Library Email ID",
                general.LibraryEmailId ?? "—"
            );

            // Digital Library
            AddYesNoNullableRow(
                table,
                "Digital Library Available",
                general.HasDigitalLibrary
            );

            // Department-wise Library
            AddYesNoNullableRow(
                table,
                "Department-wise Library Available",
                general.HasDepartmentWiseLibrary
            );

            // HELINET Services
            AddYesNoNullableRow(
                table,
                "HELInet Services Available",
                general.HasHelinetServices
            );
        });
    }

    private void AddLibraryItemsSection(ColumnDescriptor col)
    {
        var items = _model.LibraryDisplay?.LibraryItemListVM?.Items;

        if (items == null || !items.Any())
            return;

        // ================= SUB HEADING =================
        AddSubHeading(col, "Library Items Details", 110);

        col.Item().PaddingTop(8).Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(3); // Item Name
                columns.RelativeColumn(2); // Previous Year
                columns.RelativeColumn(2); // Current Year
            });

            // ---------- HEADER ----------
            table.Header(header =>
            {
                header.Cell().Border(1).Padding(5).Text("Item").Bold();
                header.Cell().Border(1).Padding(5).AlignCenter().Text("Previous Year").Bold();
                header.Cell().Border(1).Padding(5).AlignCenter().Text("Current Year").Bold();
            });

            // ---------- ROWS ----------
            foreach (var item in items)
            {
                // If Indian / Foreign split is required
                if (item.HasIndianForeignSplit)
                {
                    table.Cell().Border(1).Padding(5)
                        .Text(item.ItemName);

                    table.Cell().Border(1).Padding(5)
                        .Column(c =>
                        {
                            c.Item().Text($"Indian : {item.PreviousIndian}");
                            c.Item().Text($"Foreign : {item.PreviousForeign}");
                        });

                    table.Cell().Border(1).Padding(5)
                        .Column(c =>
                        {
                            c.Item().Text($"Indian : {item.CurrentIndian}");
                            c.Item().Text($"Foreign : {item.CurrentForeign}");
                        });
                }
                else
                {
                    table.Cell().Border(1).Padding(5)
                        .Text(item.ItemName);

                    table.Cell().Border(1).Padding(5).AlignCenter()
                        .Text((item.PreviousIndian + item.PreviousForeign).ToString());

                    table.Cell().Border(1).Padding(5).AlignCenter()
                        .Text((item.CurrentIndian + item.CurrentForeign).ToString());
                }
            }
        });
    }

    private void AddLibraryBuildingSection(ColumnDescriptor col)
    {
        var building = _model.LibraryDisplay?.LibraryBuildingVM;

        if (building == null)
            return;

        // ================= SUB HEADING =================
        AddSubHeading(col, "Library Building Details", 120);

        // ================= TABLE =================
        col.Item().PaddingTop(8).Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(3); // Label
                columns.RelativeColumn(2); // Value
            });

            // Independent Building
            AddYesNoNullableRow(
                table,
                "Independent Library Building",
                building.IsIndependent
            );

            // Area
            AddTextRow(
                table,
                "Total Area (in Sq. Mtrs)",
                building.AreaSqMtrs?.ToString("0.##") ?? "—"
            );
        });
    }


    private void AddLibraryTechnicalProcessSection(ColumnDescriptor col)
    {
        var processes = _model.LibraryDisplay?.LibraryTechListVM?.Processes;

        if (processes == null || !processes.Any())
            return;

        // ================= SUB HEADING =================
        AddSubHeading(col, "Library Technical Processes", 145);

        // ================= TABLE =================
        col.Item().PaddingTop(8).Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(3); // Process
                columns.RelativeColumn(2); // Value
            });

            // ---------- HEADER ----------
            table.Header(header =>
            {
                header.Cell().Border(1).Padding(5).Text("Process").Bold();
                header.Cell().Border(1).Padding(5).AlignCenter().Text("Details").Bold();
            });

            // ---------- ROWS ----------
            foreach (var process in processes)
            {
                table.Cell().Border(1).Padding(5)
                    .Text(process.ProcessName);

                table.Cell().Border(1).Padding(5).AlignCenter()
                    .Text(process.HasValue ? process.Value! : "—");
            }
        });
    }

    private void AddLibraryFinanceSection(ColumnDescriptor col)
    {
        var finance = _model.LibraryDisplay?.LibraryFinancVM;

        if (finance == null)
            return;

        // ================= SUB HEADING =================
        AddSubHeading(col, "Library Finance Details", 122);

        // ================= TABLE =================
        col.Item().PaddingTop(8).Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(3); // Label
                columns.RelativeColumn(2); // Value
            });

            AddTextRow(
                table,
                "Total Budget (₹ in Lakhs)",
                finance.TotalBudgetLakhs?.ToString("0.##") ?? "—"
            );

            AddTextRow(
                table,
                "Expenditure on Books (₹ in Lakhs)",
                finance.ExpenditureBooksLakhs?.ToString("0.##") ?? "—"
            );
        });
    }

    private void AddLibraryEquipmentSection(ColumnDescriptor col)
    {
        var equipmentList = _model.LibraryDisplay;

        if (equipmentList == null || equipmentList.LibraryEquipmentListVM.Items == null || !equipmentList.LibraryEquipmentListVM.Items.Any())
            return;

        //AddMainHeading(col, "Library Section");

        AddSubHeading(col, "Medical Library Equipment", 135);

        col.Item().PaddingTop(8).Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(3); // Equipment Name
                columns.RelativeColumn(2); // Status
            });


            foreach (var item in equipmentList.LibraryEquipmentListVM.Items)
            {
                AddTextRow(
                    table,
                    item.EquipmentName,
                    item.HasEquipment ? "Available" : "Not Available"
                );
            }
        });
    }

    private void AddResearchPublicationsSection(ColumnDescriptor col)
    {
        var data = _model?.LibraryDisplay?.ResearchPublicationsDisplayViewModel;

        if (data == null)
            return;

        // ---- Section Title ----
        col.Item().PaddingTop(15).Column(col2 =>
        {
            col2.Item().Text("Research Publications")
                .FontSize(12)
                .SemiBold();

            col2.Item().PaddingTop(2).Row(row =>
            {
                row.ConstantItem(120)
                    .LineHorizontal(1)
                    .LineColor(Colors.Black);

                row.RelativeItem();
            });
        });

        col.Item().PaddingTop(8).Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(3);
                columns.RelativeColumn(2);
            });

            AddTextRow(table, "Number of Publications", data.PublicationsNo?.ToString() ?? "0");
            AddTextRow(table, "Principal Investigator", data.Pi ?? "—");
            AddTextRow(table, "RGUHS Funded Projects", data.RguhsFunded?.ToString() ?? "0");
            AddTextRow(table, "External Body Funded Projects", data.ExternalBodyFunding?.ToString() ?? "0");

            AddTextRow(table, "Publications Document", data.HasPublicationsPdf ? "Available" : "—");
            AddTextRow(table, "Projects Document", data.HasProjectsPdf ? "Available" : "—");
            AddTextRow(table, "Clinical Trials Document", data.HasClinicalTrialsPdf ? "Available" : "—");

            AddTextRow(table, "Student Projects (RGUHS)", data.StudentsRguhsFunded?.ToString() ?? "0");
            AddTextRow(table, "Student Projects (External)", data.StudentsExternalFunding?.ToString() ?? "0");
            AddTextRow(table, "Student Projects Document", data.HasStudentsProjectsPdf ? "Available" : "—");

            AddTextRow(table, "Faculty Projects (RGUHS)", data.FacultyRguhsFunded?.ToString() ?? "0");
            AddTextRow(table, "Faculty Projects (External)", data.FacultyExternalFunding?.ToString() ?? "0");
            AddTextRow(table, "Faculty Projects Document", data.HasFacultyProjectsPdf ? "Available" : "—");
        });
    }

    private void AddFinanceAccountsAndFeesSection(ColumnDescriptor col)
    {
        var acc = _model.FinanceVm?.medCaAccountAndFee;

        if (acc == null)
            return;

        AddMainHeading(col, "Finance Section");

        AddSubHeading(col, "Accounts and Fee Details", 135);

        col.Item().PaddingTop(8).Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(3);
                columns.RelativeColumn(2);
            });

            // =========================
            // AUTHORITY DETAILS
            // =========================

            AddTextRow(
                table,
                "Authority Name & Address",
                acc.AuthorityNameAddress);

            AddTextRow(
                table,
                "Authority Contact",
                acc.AuthorityContact);

            // =========================
            // ANNUAL ACCOUNTS
            // =========================

            AddTextRow(
                table,
                "Recurrent Annual (₹)",
                acc.RecurrentAnnual.ToString("0.##"));

            AddTextRow(
                table,
                "Non-Recurrent Annual (₹)",
                acc.NonRecurrentAnnual.ToString("0.##"));

            AddTextRow(
                table,
                "Deposits (₹)",
                acc.Deposits.ToString("0.##"));

            // =========================
            // FEE STRUCTURE
            // =========================

            AddTextRow(
                table,
                "Tuition Fee (₹)",
                acc.TuitionFee.ToString("0.##"));

            AddTextRow(
                table,
                "Sports Fee (₹)",
                acc.SportsFee.ToString("0.##"));

            AddTextRow(
                table,
                "Union Fee (₹)",
                acc.UnionFee.ToString("0.##"));

            AddTextRow(
                table,
                "Library Fee (₹)",
                acc.LibraryFee.ToString("0.##"));

            AddTextRow(
                table,
                "Other Fee (₹)",
                acc.OtherFee.ToString("0.##"));

            AddTextRow(
                table,
                "Total Fee (₹)",
                acc.TotalFee.ToString("0.##"));

            // =========================
            // ACCOUNT DETAILS
            // =========================

            AddTextRow(
                table,
                "Account Books Maintained",
                acc.AccountBooksMaintained);

            AddTextRow(
                table,
                "Audited Statement",
                acc.HasAuditedStatementPdf ? "Available" : "—");

            AddTextRow(
                table,
                "Account Summary",
                acc.HasAccountSummaryPdf ? "Available" : "—");

            AddTextRow(
                table,
                "Governing Council Approval",
                acc.HasGoverningCouncilPdf ? "Available" : "—");
        });
    }

    private void AddFinanceStaffParticularsSection(ColumnDescriptor col)
    {
        var staffList = _model.FinanceVm?
            .staffParticularsVM?
            .StaffParticulars;

        if (staffList == null || !staffList.Any())
            return;

        AddSubHeading(col, "Staff Pay Particulars", 105);

        col.Item().PaddingTop(8).Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(4);
                columns.RelativeColumn(2);
            });

            table.Header(header =>
            {
                header.Cell()
                    .Border(1)
                    .Padding(5)
                    .Text("Designation")
                    .Bold();

                header.Cell()
                    .Border(1)
                    .Padding(5)
                    .AlignCenter()
                    .Text("Pay Scale (₹)")
                    .Bold();
            });

            foreach (var staff in staffList)
            {
                table.Cell()
                    .Border(1)
                    .Padding(5)
                    .Text(staff.DesignationName);

                table.Cell()
                    .Border(1)
                    .Padding(5)
                    .AlignCenter()
                    .Text(staff.PayScale.ToString("0.##"));
            }
        });
    }

    private void AddFinanceOtherStaffDetailsSection(ColumnDescriptor col)
    {
        var other = _model.FinanceVm?.otherStaffParticularsVM;

        if (other == null)
            return;

        AddSubHeading(col, "Other Staff & Compliance Details", 175);

        col.Item().PaddingTop(8).Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(3);
                columns.RelativeColumn(2);
            });

            // =========================
            // EMS
            // =========================

            AddTextRow(
                table,
                "Teachers Updated in EMS",
                other.TeachersUpdatedInEms ? "Yes" : "No");

            // =========================
            // EXAMINER DETAILS
            // =========================

            AddTextRow(
                table,
                "Examiner Details Attached",
                other.ExaminerDetailsAttached ? "Yes" : "No");

            AddTextRow(
                table,
                "Examiner Details Document",
                other.HasExaminerDetailsPdf
                    ? "Available"
                    : "—");

            // =========================
            // AEBAS
            // =========================

            AddTextRow(
                table,
                "AEBAS (Last 3 Months)",
                other.HasAebasLastThreeMonthsPdf
                    ? "Available"
                    : "—");

            AddTextRow(
                table,
                "AEBAS (Inspection Day)",
                other.HasAebasInspectionDayPdf
                    ? "Available"
                    : "—");

            // =========================
            // REGISTERS
            // =========================

            AddTextRow(
                table,
                "Service Register Maintained",
                other.ServiceRegisterMaintained ? "Yes" : "No");

            AddTextRow(
                table,
                "Acquittance Register Maintained",
                other.AcquittanceRegisterMaintained ? "Yes" : "No");

            // =========================
            // PF / ESI
            // =========================

            AddTextRow(
                table,
                "Provident Fund Records",
                other.HasProvidentFundPdf
                    ? "Available"
                    : "—");

            AddTextRow(
                table,
                "ESI Records",
                other.HasEsipdf
                    ? "Available"
                    : "—");
        });
    }

    private void AddVehicleDetailsSection(ColumnDescriptor col)
    {
        var vehicleList = _model.VehicleDetailsVM;

        if (vehicleList == null || vehicleList.Items == null || !vehicleList.Items.Any())
            return;

        AddMainHeading(col, "Transport Section");

        AddSubHeading(col, "Vehicle Details", 80);

        col.Item().PaddingTop(8).Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(3); // Label
                columns.RelativeColumn(2); // Value
            });

            foreach (var vehicle in vehicleList.Items)
            {
                AddTextRow(table, "Vehicle Registration No", vehicle.VehicleRegNo);
                AddTextRow(table, "Vehicle Purpose", vehicle.VehicleForCode);
                AddTextRow(
                    table,
                    "Seating Capacity",
                    vehicle.SeatingCapacity?.ToString() ?? "—"
                );

                AddTextRow(
                    table,
                    "RC Book",
                    vehicle.HasValidRc ? "Available" : "Not Available"
                );

                AddTextRow(
                    table,
                    "Insurance",
                    vehicle.HasValidInsurance ? "Available" : "Not Available"
                );

                AddTextRow(
                    table,
                    "Driving License",
                    vehicle.HasValidLicense ? "Available" : "Not Available"
                );
            }
        });
    }

    private void AddAdminTeachingBlockSection(ColumnDescriptor col)
    {
        var blocks = _model?.AdminTeachAndHostelVM?.AdminTeachingBlockDisplayVM;

        if (blocks == null || !blocks.Any())
            return;

        AddSubHeading(col, "Administrative & Teaching Block Facilities", 220);

        col.Item().PaddingTop(8).Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(3); // Facility
                columns.RelativeColumn(2); // Norms
                columns.RelativeColumn(2); // Available
                columns.RelativeColumn(2); // Rooms
                columns.RelativeColumn(3); // Size / Room
            });

            // ---- Header ----
            table.Header(header =>
            {
                header.Cell().Border(1).Padding(3).Text("Facility").Bold();
                header.Cell().Border(1).Padding(3).AlignCenter().Text("Size as per Norms").Bold();
                header.Cell().Border(1).Padding(3).AlignCenter().Text("Available").Bold();
                header.Cell().Border(1).Padding(3).AlignCenter().Text("No. of Rooms").Bold();
                header.Cell().Border(1).Padding(3).AlignCenter().Text("Size / Room").Bold();
            });

            // ---- Body ----
            foreach (var item in blocks)
            {
                table.Cell().Border(1).Padding(3).Text(item.Facilities);
                table.Cell().Border(1).Padding(3).AlignCenter().Text(item.SizeSqFtAsPerNorms);
                table.Cell().Border(1).Padding(3).AlignCenter().Text(item.IsAvailable);
                table.Cell().Border(1).Padding(3).AlignCenter().Text(item.NoOfRooms);
                table.Cell().Border(1).Padding(3).AlignCenter().Text(item.SizeSqFtAvailablePerRoom);
            }
        });
    }

    private void AddHostelDetailsSection(ColumnDescriptor col)
    {
        var hostel = _model?.HostelPreviewVM;

        if (hostel == null)
            return;

        AddMainHeading(col, "Hostel Details");

        // =========================================================
        // BASIC HOSTEL DETAILS
        // =========================================================

        AddSubHeading(col, "A. Hostel Details");

        col.Item().PaddingTop(5).Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(2);
                columns.RelativeColumn(2);
            });

            void AddRow(string label, string value)
            {
                table.Cell()
                    .Border(1)
                    .Padding(5)
                    .Text(label)
                    .Bold();

                table.Cell()
                    .Border(1)
                    .Padding(5)
                    .Text(value);
            }

            AddRow(
                "Hostel Type",
                hostel.HostelType ?? "—");

            AddRow(
                "Built-up Area (Sq.ft)",
                hostel.BuiltUpAreaSqFt.ToString());

            AddRow(
                "Separate Hostel",
                hostel.HasSeparateHostel == true ? "Yes" : "No");

            AddRow(
                "Separate Provision for Male & Female",
                hostel.SeparateProvisionMaleFemale == true ? "Yes" : "No");

            AddRow(
                "Male Students",
                hostel.TotalMaleStudents.ToString());

            AddRow(
                "Male Rooms",
                hostel.TotalMaleRooms.ToString());

            AddRow(
                "Female Students",
                hostel.TotalFemaleStudents.ToString());

            AddRow(
                "Female Rooms",
                hostel.TotalFemaleRooms.ToString());

            AddRow(
                "Hostel Men Count",
                hostel.HostelMenCount.ToString());

            AddRow(
                "Hostel Women Count",
                hostel.HostelWomenCount.ToString());

            AddRow(
                "Ownership",
                hostel.OwnOrRented ?? "—");

            AddRow(
                "Space Per Student",
                hostel.SpacePerStudent.ToString());

            AddRow(
                "Men Hostel Area (Sq.ft)",
                hostel?.MenHostelAreaSqFt?.ToString() ?? "-");

            AddRow(
                "Women Hostel Area (Sq.ft)",
                hostel?.WomenHostelAreaSqFt?.ToString());

            AddRow(
                "Possession Proof",
                !string.IsNullOrWhiteSpace(hostel.PossessionProofPath)
                    ? "Uploaded"
                    : "Not Uploaded");
        });


        // =========================================================
        // COMMON ROOMS / OTHER FACILITIES
        // =========================================================

        AddSubHeading(col, "B. Common Rooms & Other Details");

        col.Item().PaddingTop(5).Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(2);
                columns.RelativeColumn(2);
            });

            void AddRow(string label, string value)
            {
                table.Cell()
                    .Border(1)
                    .Padding(5)
                    .Text(label)
                    .Bold();

                table.Cell()
                    .Border(1)
                    .Padding(5)
                    .Text(value);
            }

            AddRow(
                "Common Room - Men",
                hostel.CommonRoomMen.HasValue
                    ? (hostel.CommonRoomMen.Value ? "Yes" : "No")
                    : "—");

            AddRow(
                "Common Room - Women",
                hostel.CommonRoomWomen.HasValue
                    ? (hostel.CommonRoomWomen.Value ? "Yes" : "No")
                    : "—");

            AddRow(
                "Any Other Facility",
                hostel.AnyOtherFacility == "true" ? "Yes" : "No");

            AddRow(
                "Facility Details",
                hostel.HostelFacilityDetails ?? "—");
        });


        // =========================================================
        // HOSTEL FACILITIES
        // =========================================================

        AddSubHeading(col, "C. Hostel Facilities");

        col.Item().PaddingTop(5).Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(2);
                columns.RelativeColumn(1);
            });

            table.Header(header =>
            {
                header.Cell()
                    .Border(1)
                    .Padding(5)
                    .Text("Facility")
                    .Bold();

                header.Cell()
                    .Border(1)
                    .Padding(5)
                    .AlignCenter()
                    .Text("Available")
                    .Bold();
            });

            void AddFacilityRow(string name, bool? value)
            {
                table.Cell()
                    .Border(1)
                    .Padding(5)
                    .Text(name);

                table.Cell()
                    .Border(1)
                    .Padding(5)
                    .AlignCenter()
                    .Text(value == true ? "Yes" : "No");
            }

            AddFacilityRow(
                "Sleeping Furniture",
                hostel.SleepingFurniture);

            AddFacilityRow(
                "Sanitary & Bathing Facilities",
                hostel.SanitaryBathing);

            AddFacilityRow(
                "Dining Hall",
                hostel.DiningHall);

            AddFacilityRow(
                "Hostel Common Room",
                hostel.HostelCommonRoom);

            AddFacilityRow(
                "Visitors Room",
                hostel.VisitorsRoom);

            AddFacilityRow(
                "Kitchen / Pantry",
                hostel.KitchenPantry);

            AddFacilityRow(
                "Warden Office",
                hostel.WardenOffice);

            AddFacilityRow(
                "Reception Counter",
                hostel.ReceptionCounter);

            AddFacilityRow(
                "Games / Recreation",
                hostel.GamesRecreation);

            AddFacilityRow(
                "Medical Facilities",
                hostel.MedicalFacilities);
        });
    }

    private void AddHostelFacilitiesSection(ColumnDescriptor col)
    {
        var facilities = _model.AdminTeachAndHostelVM?.AffHostelFacilitiesVM;

        if (facilities == null || !facilities.Any())
            return;

        AddSubHeading(col, "Hostel Facilities", 85);

        col.Item().PaddingTop(8).Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(4);   // Facility
                columns.ConstantColumn(120); // Availability
            });

            // ---- Header ----
            table.Header(header =>
            {
                header.Cell().Border(1).Padding(4).Text("Facility").Bold();
                header.Cell().Border(1).Padding(4).AlignCenter().Text("Availability").Bold();
            });

            // ---- Body ----
            foreach (var item in facilities)
            {
                table.Cell().Border(1).Padding(4).Text(item.FacilityName);
                table.Cell().Border(1).Padding(4).AlignCenter()
                    .Text(item.IsAvailable ? "Available" : "Not Available");
            }
        });
    }

    private void AddFacultyDetailsSection(ColumnDescriptor col)
    {
        var facultyList = _model?
            .FacultyDesigNonTeachDisplayVM?
            .FacultyDetailDisplayVM;

        if (facultyList == null || !facultyList.Any())
            return;

        // =========================================================
        // MAIN HEADING
        // =========================================================

        AddMainHeading(
            col,
            "Faculty, Designation, Non Teaching"
        );

        // =========================================================
        // FACULTY DETAILS
        // =========================================================

        AddSubHeading(
            col,
            "Faculty Details"
        );

        col.Item()
            .PaddingTop(8)
            .Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(3);   // Name
                    columns.RelativeColumn(2);   // Designation
                    columns.RelativeColumn(2);   // Subject
                    columns.ConstantColumn(60);  // PG
                    columns.ConstantColumn(60);  // PhD
                    columns.ConstantColumn(70);  // Litigation
                    columns.ConstantColumn(70);  // Docs
                });

                // =================================================
                // HEADER
                // =================================================

                AddTableHeader(
                    table,
                    "Name",
                    "Designation",
                    "Subject",
                    "PG",
                    "PhD",
                    "Litigation",
                    "Docs"
                );

                // =================================================
                // BODY
                // =================================================

                int rowIndex = 0;

                foreach (var f in facultyList)
                {
                    var bg = RowBg(rowIndex);

                    // Name
                    table.Cell()
                        .Border(1)
                        .BorderColor(BorderColor)
                        .Background(bg)
                        .Padding(4)
                        .Text(
                            string.IsNullOrWhiteSpace(f.NameOfFaculty)
                                ? "—"
                                : f.NameOfFaculty
                        )
                        .FontSize(9)
                        .FontColor(PrimaryColor);

                    // Designation
                    table.Cell()
                        .Border(1)
                        .BorderColor(BorderColor)
                        .Background(bg)
                        .Padding(4)
                        .Text(
                            string.IsNullOrWhiteSpace(f.Designation)
                                ? "—"
                                : f.Designation
                        )
                        .FontSize(9);

                    // Subject
                    table.Cell()
                        .Border(1)
                        .BorderColor(BorderColor)
                        .Background(bg)
                        .Padding(4)
                        .Text(
                            string.IsNullOrWhiteSpace(f.Subject)
                                ? "—"
                                : f.Subject
                        )
                        .FontSize(9);

                    // PG
                    table.Cell()
                        .Border(1)
                        .BorderColor(BorderColor)
                        .Background(bg)
                        .Padding(4)
                        .AlignCenter()
                        .Text(
                            string.IsNullOrWhiteSpace(f.RecognizedPgTeacher)
                                ? "—"
                                : f.RecognizedPgTeacher
                        )
                        .FontSize(9);

                    // PhD
                    table.Cell()
                        .Border(1)
                        .BorderColor(BorderColor)
                        .Background(bg)
                        .Padding(4)
                        .AlignCenter()
                        .Text(
                            string.IsNullOrWhiteSpace(f.RecognizedPhDteacher)
                                ? "—"
                                : f.RecognizedPhDteacher
                        )
                        .FontSize(9);

                    // Litigation
                    table.Cell()
                        .Border(1)
                        .BorderColor(BorderColor)
                        .Background(bg)
                        .Padding(4)
                        .AlignCenter()
                        .Text(
                            string.IsNullOrWhiteSpace(f.LitigationPending)
                                ? "—"
                                : f.LitigationPending
                        )
                        .FontSize(9);

                    // Documents
                    table.Cell()
                        .Border(1)
                        .BorderColor(BorderColor)
                        .Background(bg)
                        .Padding(4)
                        .AlignCenter()
                        .Text(
                            (f.HasGuideRecognitionDoc ||
                             f.HasPhDRecognitionDoc ||
                             f.HasLitigationDoc)
                                ? "Available"
                                : "—"
                        )
                        .FontSize(9);

                    rowIndex++;
                }
            });
    }

    private void AddCollegeDesignationSection(ColumnDescriptor col)
    {
        var groups = _model?
            .FacultyDesigNonTeachDisplayVM?
            .CollegeDesignationDisplayVM;

        if (groups == null || !groups.Any())
            return;

        // =========================================================
        // SECTION HEADING
        // =========================================================

        AddSubHeading(
            col,
            "Designation & Intake Details"
        );

        // =========================================================
        // DESIGNATION TABLE
        // =========================================================

        col.Item()
            .PaddingTop(8)
            .Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(3);   // Department
                    columns.RelativeColumn(3);   // Designation
                    columns.ConstantColumn(80);  // Required
                    columns.ConstantColumn(80);  // Available
                    columns.ConstantColumn(70);  // Seat Slab
                });

                // =================================================
                // HEADER
                // =================================================

                AddTableHeader(
                    table,
                    "Department",
                    "Designation",
                    "Required",
                    "Available",
                    "Seat Slab"
                );

                // =================================================
                // BODY
                // =================================================

                int rowIndex = 0;

                foreach (var group in groups)
                {
                    if (group.Designations == null ||
                        !group.Designations.Any())
                    {
                        continue;
                    }

                    bool isFirstRow = true;

                    foreach (var item in group.Designations)
                    {
                        var bg = RowBg(rowIndex);

                        // Department
                        table.Cell()
                            .Border(1)
                            .BorderColor(BorderColor)
                            .Background(bg)
                            .Padding(4)
                            .Text(
                                isFirstRow
                                    ? (string.IsNullOrWhiteSpace(group.Department)
                                        ? "—"
                                        : group.Department)
                                    : string.Empty
                            )
                            .FontSize(9)
                            .FontColor(PrimaryColor);

                        // Designation
                        table.Cell()
                            .Border(1)
                            .BorderColor(BorderColor)
                            .Background(bg)
                            .Padding(4)
                            .Text(
                                string.IsNullOrWhiteSpace(item.Designation)
                                    ? "—"
                                    : item.Designation
                            )
                            .FontSize(9);

                        // Required
                        table.Cell()
                            .Border(1)
                            .BorderColor(BorderColor)
                            .Background(bg)
                            .Padding(4)
                            .AlignCenter()
                            .Text(item.RequiredIntake.ToString())
                            .FontSize(9);

                        // Available
                        table.Cell()
                            .Border(1)
                            .BorderColor(BorderColor)
                            .Background(bg)
                            .Padding(4)
                            .AlignCenter()
                            .Text(item.AvailableIntake.ToString())
                            .FontSize(9);

                        // Seat Slab
                        table.Cell()
                            .Border(1)
                            .BorderColor(BorderColor)
                            .Background(bg)
                            .Padding(4)
                            .AlignCenter()
                            .Text(item.SeatSlab.ToString())
                            .FontSize(9);

                        isFirstRow = false;
                        rowIndex++;
                    }
                }
            });
    }

    private void AddPaymentSection(ColumnDescriptor col)
    {
        var payment = _model?.DentalPaymentVM;

        if (payment == null ||
            !payment.PaymentId.HasValue ||
            payment.PaymentId <= 0)
        {
            return;
        }

        // =========================================================
        // MAIN HEADING
        // =========================================================

        AddMainHeading(col, "Payment Details");

        // =========================================================
        // PAYMENT DETAILS
        // =========================================================

        col.Item()
            .PaddingTop(8)
            .Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(3); // Label
                    columns.RelativeColumn(4); // Value
                });

                AddTextRow(
                    table,
                    "Type of Affiliation",
                    payment.AffiliationCategory ?? "—"
                );

                AddTextRow(
                    table,
                    "Course Level",
                    payment.CourseLevel ?? "—"
                );

                // Amount Paid
                AddTextRow(
                    table,
                    "Amount Paid",
                    payment.AmountPaid > 0
                        ? $"₹ {payment.AmountPaid:N2}"
                        : "—"
                );

                // Transaction ID
                AddTextRow(
                    table,
                    "Transaction ID",
                    payment.TransactionId
                );

                // Supporting Document
                AddTextRow(
                    table,
                    "Supporting Document",
                    !string.IsNullOrWhiteSpace(payment.TransactionReceiptPath)
                        ? "Available"
                        : "—"
                );
            });
    }

    private void AddNonTeachingStaffSection(ColumnDescriptor col)
    {
        var staffList = _model?
            .FacultyDesigNonTeachDisplayVM?
            .NonTeachingStaffSectionVM?
            .Staffs;

        if (staffList == null || !staffList.Any())
            return;

        // =========================================================
        // SECTION HEADING
        // =========================================================

        AddSubHeading(
            col,
            "Non-Teaching Staff Details"
        );

        // =========================================================
        // STAFF TABLE
        // =========================================================

        col.Item()
            .PaddingTop(8)
            .Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(3);    // Staff Name
                    columns.RelativeColumn(3);    // Designation
                    columns.ConstantColumn(60);   // PF
                    columns.ConstantColumn(60);   // ESI
                    columns.ConstantColumn(100);  // Service Register
                    columns.ConstantColumn(100);  // Salary Register
                });

                // =================================================
                // HEADER
                // =================================================

                AddTableHeader(
                    table,
                    "Staff Name",
                    "Designation",
                    "PF",
                    "ESI",
                    "Service Register",
                    "Salary Register"
                );

                // =================================================
                // BODY
                // =================================================

                int rowIndex = 0;

                foreach (var staff in staffList)
                {
                    var bg = RowBg(rowIndex);

                    // Staff Name
                    table.Cell()
                        .Border(1)
                        .BorderColor(BorderColor)
                        .Background(bg)
                        .Padding(4)
                        .Text(
                            string.IsNullOrWhiteSpace(staff.StaffName)
                                ? "—"
                                : staff.StaffName
                        )
                        .FontSize(9)
                        .FontColor(PrimaryColor);

                    // Designation
                    table.Cell()
                        .Border(1)
                        .BorderColor(BorderColor)
                        .Background(bg)
                        .Padding(4)
                        .Text(
                            string.IsNullOrWhiteSpace(staff.Designation)
                                ? "—"
                                : staff.Designation
                        )
                        .FontSize(9);

                    // PF
                    table.Cell()
                        .Border(1)
                        .BorderColor(BorderColor)
                        .Background(bg)
                        .Padding(4)
                        .AlignCenter()
                        .Text(staff.PfProvided ? "Yes" : "No")
                        .FontSize(9);

                    // ESI
                    table.Cell()
                        .Border(1)
                        .BorderColor(BorderColor)
                        .Background(bg)
                        .Padding(4)
                        .AlignCenter()
                        .Text(staff.EsiProvided ? "Yes" : "No")
                        .FontSize(9);

                    // Service Register
                    table.Cell()
                        .Border(1)
                        .BorderColor(BorderColor)
                        .Background(bg)
                        .Padding(4)
                        .AlignCenter()
                        .Text(staff.ServiceRegisterMaintained ? "Yes" : "No")
                        .FontSize(9);

                    // Salary Register
                    table.Cell()
                        .Border(1)
                        .BorderColor(BorderColor)
                        .Background(bg)
                        .Padding(4)
                        .AlignCenter()
                        .Text(staff.SalaryAcquaintanceRegister ? "Yes" : "No")
                        .FontSize(9);

                    rowIndex++;
                }
            });
    }

    private static void AddLabRow(TableDescriptor table, string label, bool available, bool shared)
    {
        table.Cell().Border(1).Padding(5).Text(label);
        table.Cell().Border(1).Padding(5).AlignCenter().Text(available ? "Yes" : "No");
        table.Cell().Border(1).Padding(5).AlignCenter().Text(shared ? "Yes" : "No");
    }

    //private void AddMainHeading(ColumnDescriptor col, string title)
    //{
    //    col.Item().PaddingTop(20).Column(c =>
    //    {
    //        c.Item().Text(title)
    //            .FontSize(14)
    //            .AlignCenter()
    //            .Bold();

    //    });
    //}

    //private void AddSubHeading(ColumnDescriptor col, string title, int lineLength = 150)
    //{
    //    col.Item().PaddingTop(15).Column(c =>
    //    {
    //        c.Item().Text(title)
    //            .FontSize(12)
    //            .Bold();

    //        c.Item().PaddingTop(2).Row(row =>
    //        {
    //            row.ConstantItem(lineLength)
    //                .LineHorizontal(1)
    //                .LineColor(Colors.Black);

    //            row.RelativeItem();
    //        });

    //    });
    //}

    private void AddSubHeading(ColumnDescriptor col, string title, int widthPercent = 100)
    {
        col.Item()
            .PaddingTop(8)
            .PaddingBottom(2)
            .Background(AccentColor)
            .Border(1)
            .BorderColor(SecondaryColor)
            .PaddingVertical(4)
            .PaddingHorizontal(8)
            .Text(title)
            .FontSize(11)
            .SemiBold()
            .FontColor(PrimaryColor);
    }

    private void AddTableHeader(TableDescriptor table, params string[] headers)
    {
        table.Header(header =>
        {
            foreach (var h in headers)
            {
                header.Cell()
                    .Border(1)
                    .BorderColor(BorderColor)
                    .Background(HeaderBgColor)
                    .Padding(5)
                    .AlignCenter()
                    .Text(h)
                    .Bold()
                    .FontSize(9)
                    .FontColor(HeaderFgColor);
            }
        });
    }

    private void AddStyledLabelValueRow(TableDescriptor table, string label, string value)
    {
        table.Cell()
            .Border(1)
            .BorderColor(BorderColor)
            .Background(AccentColor)
            .Padding(5)
            .Text(label)
            .Bold()
            .FontSize(9)
            .FontColor(PrimaryColor);

        table.Cell()
            .Border(1)
            .BorderColor(BorderColor)
            .Padding(5)
            .Text(value ?? "—")
            .FontSize(9);
    }

    //private static void AddTextRow(TableDescriptor table, string label, object value)
    //{
    //    table.Cell().Border(1).Padding(5).Text(label);
    //    table.Cell().Border(1).Padding(5).AlignCenter()
    //        .Text(value?.ToString() ?? "—");
    //}

    private void AddTextRow(TableDescriptor table, string label, string? value)
    {
        table.Cell()
            .Border(1)
            .BorderColor(BorderColor)
            .Background(AccentColor)
            .Padding(5)
            .Text(label)
            .Bold()
            .FontSize(9)
            .FontColor(PrimaryColor);

        table.Cell()
            .Border(1)
            .BorderColor(BorderColor)
            .Padding(5)
            .Text(value ?? "—")
            .FontSize(9);
    }


    private static void AddYesNoNullableRow(TableDescriptor table, string label, bool? value)
    {
        table.Cell().Border(1).Padding(5).Text(label);

        table.Cell().Border(1).Padding(5).AlignCenter()
            .Text(value == null ? "—" : value.Value ? "Yes" : "No");
    }

    static IContainer CellHeader(IContainer container)
    {
        return container
            .Padding(6)
            .Border(1)
            .BorderColor(Colors.Grey.Lighten2)
            .Background(Colors.Grey.Lighten3)
            .AlignMiddle();
    }

    static IContainer CellBody(IContainer container)
    {
        return container
            .Padding(6)
            .Border(1)
            .BorderColor(Colors.Grey.Lighten3)
            .AlignMiddle();
    }


}