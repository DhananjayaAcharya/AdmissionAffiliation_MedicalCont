using Medical_Affiliation.Models;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

public class PreviewReportPdf : IDocument
{
    private readonly CApreviewViewModel _model;
    private readonly byte[] _logo;
    private readonly byte[] _collegeLogoBytes;

    public PreviewReportPdf(CApreviewViewModel model, byte[] logo, byte[] clgLogoBytes)
    {
        _model = model;
        _logo = logo;
        _collegeLogoBytes = clgLogoBytes;
    }

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

                col.Item().PaddingVertical(10).Text($"Institution Name: {_model.CollegeName}");

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
                AddMedicalUGBedDistributionSection(col);

                if(_model.FacultyCode == "2")
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

                if(_model.FacultyCode == "1")
                {
                    //-- STUDENT PRACTICAL LABORATORIES ---
                    AddStudentPracticalLabsSection(col);

                    //--MUSEUMS ---
                    AddMuseumsSection(col);

                    // --- Department MEU ---
                    AddDepartmentOfficesAndMeuSection(col);


                    //--- SKILL LAB SECTION ----
                    AddSkillsLabSection(col);

                    // --- LAB EQUIPMENT
                    AddLaboratoryEquipmentSection(col);
                    // -- end of chandans code ---
                }


                //--- 3. RESEARCH AND PUBLICATIONS ---

                //--- LIBRARY RESEARCH PUBLICATIONS ---
                AddResearchPublicationsSection(col);


                //--- OTHER LIBRARY DETAILS - PENDING ---
                //--- LIBRARY OTHER DETAILS ---
                AddLibraryOtherDetailsSection(col);

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

                AddSupervisionInFieldPracticeAreaSection(col);

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
            page.Footer() .PaddingTop(10)
                .Row(row =>
                {
                    // Left: Date
                    row.RelativeItem()
                        .AlignLeft()
                        .Text(text =>
                        {
                            text.Span("Downloaded on : ");
                            text.Span(DateTime.Now.ToString("dd-MM-yyyy, HH:mm tt"));
                        });

                    // Right: Page X of Y
                    row.RelativeItem()
                        .AlignRight()
                        .Text(text =>
                        {
                            text.Span("Page ");
                            text.CurrentPageNumber();
                            text.Span(" of ");
                            text.TotalPages();
                        });
                });

        });

    }

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

        if (institution == null)
            return;

        AddMainHeading(col, "Institution Basic Details");

        // --------------------------------------------------
        // BASIC INFORMATION
        // --------------------------------------------------

        AddSubHeading(col, "Basic Information", 95);

        col.Item()
            .PaddingTop(8)
            .Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(3); // Label
                    columns.RelativeColumn(2); // Value
                });

                AddTextRow( table, "Name of Institution", institution.NameOfInstitution);
                AddTextRow( table, "Year of Establishment", institution.YearOfEstablishment);
                AddTextRow( table, "Type of Institution", institution.TypeOfInstitution);
                AddTextRow( table, "Running Course", institution.RunningCourse);
                AddTextRow( table, "Course Level", institution.CourseLevel);
                AddTextRow( table,"Status of College", institution.StatusOfCollege);
                AddTextRow( table, "Financing Authority", institution.FinancingAuthority);
                AddTextRow( table,"Minority Category", institution.MinorityCategory);
                AddTextRow( table, "Minority Institution", institution.MinorityInstitute ? "Yes" : "No");
                AddTextRow( table, "Attached to Medical College", institution.AttachedToMedicalClg ? "Yes" : "No");
                AddTextRow( table, "Rural Institution", institution.RuralInstitute ? "Yes" : "No");
            });


        // --------------------------------------------------
        // COLLEGE LOCATION & CONTACT
        // --------------------------------------------------

        AddSubHeading(col, "College Location & Contact", 145);

        col.Item()
            .PaddingTop(8)
            .Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(3); // Label
                    columns.RelativeColumn(2); // Value
                });

                AddTextRow( table, "Address", institution.Address);
                AddTextRow( table, "Village / Town / City", institution.VillageTownCity);
                AddTextRow( table, "District", institution.District);
                AddTextRow( table, "Taluk", institution.Taluk);
                AddTextRow( table, "PIN Code", institution.PinCode);
                AddTextRow( table, "STD Code", institution.StdCode ?? "—");
                AddTextRow( table, "Mobile Number", institution.MobileNumber);
                AddTextRow( table, "Alternate / Landline", institution.AltLandlineMobile ?? "—");
                AddTextRow( table, "Fax", institution.Fax ?? "—");
                AddTextRow( table, "College Email", institution.EmailId);
                AddTextRow( table, "Alternate Email", institution.AltEmailId ?? "—");
                AddTextRow( table, "Website", institution.Website ?? "—");
                AddTextRow( table,  "College URL", institution.College_URL ?? "—");
                AddTextRow( table, "Survey No / PID No",institution.SurveyNoPidNo ?? "—");
            });


        // --------------------------------------------------
        // GOVERNMENT AUTONOMOUS DETAILS
        // --------------------------------------------------

        if (!string.IsNullOrWhiteSpace(institution.GovAutonomousCertNumber))
        {
            AddSubHeading(col, "Government Autonomous Details", 78);

            col.Item()
                .PaddingTop(8)
                .Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(3);
                        columns.RelativeColumn(2);
                    });

                    AddTextRow( table,"Gov Autonomous Certificate Number",  institution.GovAutonomousCertNumber);
                });
        }
    }


    private void AddTrustManagementSection(ColumnDescriptor col)
    {
        var institution = _model?.InstitutionBasicVM?.InstitutionDetails;

        if (institution == null)
            return;

        // Don't show the section if there is no trust/management data
        if (string.IsNullOrWhiteSpace(institution.TrustName) &&
            string.IsNullOrWhiteSpace(institution.TrustAddress) &&
            !institution.TrustEstablishmentDate.HasValue &&
            string.IsNullOrWhiteSpace(institution.TrustPresidentName) &&
            string.IsNullOrWhiteSpace(institution.TrustPresidentContactNo))
        {
            return;
        }

        AddMainHeading(col, "Trust Institution Details");

        AddSubHeading(col, "Trust / Management Details");

        col.Item()
            .PaddingTop(8)
            .Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(3);
                    columns.RelativeColumn(2);
                });

                AddTextRow( table, "Trust Name", institution.TrustName ?? "—");
                AddTextRow( table, "Establishment Date", institution.TrustEstablishmentDate.HasValue ? institution.TrustEstablishmentDate.Value.ToString("dd-MM-yyyy") : "—");
                AddTextRow( table, "President Name", institution.TrustPresidentName ?? "—");
                AddTextRow( table, "President Contact", institution.TrustPresidentContactNo ?? "—");
                AddTextRow( table, "Trust Address", institution.TrustAddress ?? "—");
            });
    }

    private void AddNodalOfficerSection(ColumnDescriptor col)
    {
        var institution = _model?.InstitutionBasicVM?.InstitutionDetails;

        if (institution == null)
            return;

        // Don't show empty section
        if (string.IsNullOrWhiteSpace(institution.NodalOfficer_Name) &&
            string.IsNullOrWhiteSpace(institution.NodalOfficer_Mob_Number) &&
            string.IsNullOrWhiteSpace(institution.NodalOfficer_Email))
        {
            return;
        }

        AddSubHeading(col, "Nodal Officer & Academic Info", 165);

        col.Item()
            .PaddingTop(8)
            .Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(3);
                    columns.RelativeColumn(2);
                });

                AddTextRow( table, "Nodal Officer Name", institution.NodalOfficer_Name ?? "—");
                AddTextRow( table, "Nodal Officer Mobile", institution.NodalOfficer_Mob_Number ?? "—");
                AddTextRow( table, "Nodal Officer Email", institution.NodalOfficer_Email ?? "—");
            });
    }

    private void AddTrustMembersSection(ColumnDescriptor col)
    {
        var members = _model?.InstitutionBasicVM?.TrustMemberVM;

        if (members?.Items == null || !members.Items.Any())
            return;

        AddSubHeading(col, "Trust Members", 85);

        col.Item()
            .PaddingTop(8)
            .Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(3); // Label
                    columns.RelativeColumn(4); // Value
                });

                int memberNumber = 1;

                foreach (var member in members.Items)
                {
                    // =========================
                    // Member Heading
                    // =========================

                    table.Cell()
                        .ColumnSpan(2)
                        .PaddingTop(memberNumber == 1 ? 0 : 10)
                        .PaddingBottom(5)
                        .Text($"Trust Member {memberNumber}")
                        .Bold()
                        .FontSize(10);

                    // =========================
                    // Member Details
                    // =========================

                    AddTextRow(
                        table,
                        "Name",
                        string.IsNullOrWhiteSpace(member.TrustMemberName)
                            ? "—"
                            : member.TrustMemberName);

                    AddTextRow(
                        table,
                        "Designation",
                        string.IsNullOrWhiteSpace(member.Designation)
                            ? "—"
                            : member.Designation);

                    AddTextRow(
                        table,
                        "Qualification",
                        string.IsNullOrWhiteSpace(member.Qualification)
                            ? "—"
                            : member.Qualification);

                    AddTextRow(
                        table,
                        "Mobile",
                        string.IsNullOrWhiteSpace(member.MobileDisplay)
                            ? "—"
                            : member.MobileDisplay);

                    AddTextRow(
                        table,
                        "Age",
                        member.Age?.ToString() ?? "—");

                    AddTextRow(
                        table,
                        "Joining Date",
                        string.IsNullOrWhiteSpace(member.JoiningDateDisplay)
                            ? "—"
                            : member.JoiningDateDisplay);

                    memberNumber++;
                }
            });
    }

    private void AddTeachingFacultyDetailsSection(ColumnDescriptor col)
    {
        var teachingFaculty = _model?.TeachingFacultyDetailsVM;

        if (teachingFaculty == null ||
            teachingFaculty.FacultyDetails == null ||
            !teachingFaculty.FacultyDetails.Any())
            return;

        AddMainHeading(col, "Teaching Faculty Details");

        col.Item()
            .PaddingTop(8)
            .Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(2); // Department
                    columns.RelativeColumn(2); // Designation
                    columns.RelativeColumn(1); // Required
                    columns.RelativeColumn(1); // Available
                });

                table.Header(header =>
                {
                    header.Cell()
                        .Border(1)
                        .Padding(5)
                        .AlignCenter()
                        .Text("Department")
                        .Bold();

                    header.Cell()
                        .Border(1)
                        .Padding(5)
                        .AlignCenter()
                        .Text("Designation")
                        .Bold();

                    header.Cell()
                        .Border(1)
                        .Padding(5)
                        .AlignCenter()
                        .Text("Required Faculty")
                        .Bold();

                    header.Cell()
                        .Border(1)
                        .Padding(5)
                        .AlignCenter()
                        .Text("Available Faculty")
                        .Bold();
                });

                foreach (var item in teachingFaculty.FacultyDetails)
                {
                    table.Cell()
                        .Border(1)
                        .Padding(5)
                        .Text(string.IsNullOrWhiteSpace(item.DepartmentName)
                            ? "—"
                            : item.DepartmentName);

                    table.Cell()
                        .Border(1)
                        .Padding(5)
                        .Text(string.IsNullOrWhiteSpace(item.DesignationName)
                            ? "—"
                            : item.DesignationName);

                    table.Cell()
                        .Border(1)
                        .Padding(5)
                        .AlignCenter()
                        .Text(string.IsNullOrWhiteSpace(item.RequiredFaculty)
                            ? "0"
                            : item.RequiredFaculty);

                    table.Cell()
                        .Border(1)
                        .Padding(5)
                        .AlignCenter()
                        .Text(string.IsNullOrWhiteSpace(item.AvailableFaculty)
                            ? "0"
                            : item.AvailableFaculty);
                }
            });
    }


    string FormatValue(object? value)
    {
        if (value == null)
            return "—";

        if (value is decimal decimalValue)
            return decimalValue.ToString("0.##");

        if (value is double doubleValue)
            return doubleValue.ToString("0.##");

        if (value is float floatValue)
            return floatValue.ToString("0.##");

        return value.ToString() ?? "—";
    }

    private void AddDentalLandBuildingSection(ColumnDescriptor col)
    {
        var landBuilding = _model?.DentalLandBuildingPreview;

        if (landBuilding == null)
            return;

        AddMainHeading(col, "Land & Building Details");

        // =========================================================
        // A. LAND DETAILS
        // =========================================================

        AddSubHeading(col, "Land Details", 70);

        
        col.Item().PaddingTop(5).Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn();
                columns.RelativeColumn();
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

            table.Header(header =>
            {
                header.Cell()
                    .Border(1)
                    .Padding(5)
                    .Text("Particulars")
                    .Bold();

                header.Cell()
                    .Border(1)
                    .Padding(5)
                    .Text("Details")
                    .Bold();
            });

            AddRow(
                "Seat Intake",
                landBuilding.SeatIntake.ToString());

            AddRow(
                "Seat Slab",
                landBuilding.SeatSlab.ToString());

            AddRow(
                "Land Category",
                landBuilding.LandCategory ?? "—");

            AddRow(
                "Total Land Area (Acres)",
                FormatValue(landBuilding.TotalLandAreaAcres));

            AddRow(
                "Land Ownership Type",
                landBuilding.LandOwnershipType ?? "—");

            AddRow(
                "Future Expansion Space",
                landBuilding.HasFutureExpansionSpace == true
                    ? "Yes"
                    : "No");
        });

        // =========================================================
        // B. BUILDING DETAILS
        // =========================================================

        AddSubHeading(col, "Building Details", 85);

        col.Item().PaddingTop(5).Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(2);   // Particular
                columns.RelativeColumn(1);   // Required
                columns.RelativeColumn(1);   // Available
            });

            void AddRow(string label, string required, string available)
            {
                table.Cell()
                    .Border(1)
                    .Padding(5)
                    .Text(label)
                    .Bold();

                table.Cell()
                    .Border(1)
                    .Padding(5)
                    .Text(required);

                table.Cell()
                    .Border(1)
                    .Padding(5)
                    .Text(available);
            }

            table.Header(header =>
            {
                header.Cell()
                    .Border(1)
                    .Padding(5)
                    .Text("Building Particulars")
                    .Bold();

                header.Cell()
                    .Border(1)
                    .Padding(5)
                    .AlignCenter()
                    .Text("Required / Norm")
                    .Bold();

                header.Cell()
                    .Border(1)
                    .Padding(5)
                    .AlignCenter()
                    .Text("Available")
                    .Bold();
            });

            AddRow(
                "Total Built-up Area",
                $"{landBuilding.RequiredBuiltupAreaSqm:0.##} Sq.m",
                $"{landBuilding.TotalBuiltupAreaSqm:0.##} Sq.m");

            AddRow(
                "Lecture Hall Count",
                landBuilding.RequiredLectureHallCount.ToString(),
                landBuilding.LectureHallCount.ToString());

            AddRow(
                "Lecture Hall Area",
                $"{landBuilding.RequiredLectureHallAreaSqm:0.##} Sq.m",
                $"{landBuilding.LectureHallAreaSqm:0.##} Sq.m");

            AddRow(
                "Lecture Hall Capacity",
                landBuilding.RequiredLectureHallCapacity.ToString(),
                landBuilding.LectureHallSeatingCapacity.ToString());

            AddRow(
                "Examination Hall Area",
                $"{landBuilding.RequiredExamHallAreaSqm:0.##} Sq.m",
                $"{landBuilding.ExaminationHallAreaSqm:0.##} Sq.m");

            AddRow(
                "Library Area",
                $"{landBuilding.RequiredLibraryAreaSqm:0.##} Sq.m",
                $"{landBuilding.LibraryAreaSqm:0.##} Sq.m");

            AddRow(
                "Hospital Area",
                $"{landBuilding.RequiredHospitalAreaSqm:0.##} Sq.m",
                $"{landBuilding.HospitalAreaSqm:0.##} Sq.m");

            AddRow(
                "Museum & Demo Rooms",
                "As per Norms",
                $"{landBuilding.MuseumDemoRoomsAreaSqm:0.##} Sq.m");

            AddRow(
                "Department-wise Area",
                "As per Norms",
                $"{landBuilding.DepartmentWiseAreaSqm:0.##} Sq.m");

            AddRow(
                "Preclinical & Skill Lab Area",
                "As per Norms",
                $"{landBuilding.PreclinicalSkillLabAreaSqm:0.##} Sq.m");

            AddRow(
                "Remarks",
                "—",
                landBuilding.Remarks ?? "—");
        });


        // =========================================================
        // C. INFRASTRUCTURE REQUIREMENTS
        // =========================================================

        if (landBuilding.InfrastructureDetails?.Any() == true)
        {
            AddSubHeading(col, "Infrastructure Requirements");

            col.Item().PaddingTop(5).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(35);  // Sl No
                    columns.RelativeColumn(1.4f); // Requirement
                    columns.RelativeColumn(2.5f); // Description
                    columns.ConstantColumn(75);  // Required
                    columns.ConstantColumn(75);  // Available
                });

                table.Header(header =>
                {
                    header.Cell()
                        .Border(1)
                        .Padding(5)
                        .AlignCenter()
                        .Text("Sl. No.")
                        .Bold();

                    header.Cell()
                        .Border(1)
                        .Padding(5)
                        .Text("Requirement")
                        .Bold();

                    header.Cell()
                        .Border(1)
                        .Padding(5)
                        .Text("Description")
                        .Bold();

                    header.Cell()
                        .Border(1)
                        .Padding(5)
                        .AlignCenter()
                        .Text("Required\n(Sq.Ft)")
                        .Bold();

                    header.Cell()
                        .Border(1)
                        .Padding(5)
                        .AlignCenter()
                        .Text("Available\n(Sq.Ft)")
                        .Bold();
                });

                foreach (var item in landBuilding.InfrastructureDetails
                             .OrderBy(x => x.SlNo))
                {
                    table.Cell()
                        .Border(1)
                        .Padding(5)
                        .AlignCenter()
                        .Text(item.SlNo.ToString());

                    table.Cell()
                        .Border(1)
                        .Padding(5)
                        .Text(item.RequirementName ?? "—");

                    table.Cell()
                        .Border(1)
                        .Padding(5)
                        .Text(item.RequirementDescription ?? "—");

                    table.Cell()
                        .Border(1)
                        .Padding(5)
                        .AlignCenter()
                        .Text($"{item.RequiredAreaSqFt:0.00} Sq.ft");

                    table.Cell()
                        .Border(1)
                        .Padding(5)
                        .AlignCenter()
                        .Text($"{item.AvailableAreaSqFt:0.00} Sq.ft");
                }
            });
        }


        // =========================================================
        // D. DOCUMENTS
        // =========================================================

        AddSubHeading(col, "Land & Building Documents");

        col.Item().PaddingTop(5).Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn();
                columns.ConstantColumn(90);
            });

            void AddDocumentRow(string name, string? path)
            {
                table.Cell()
                    .Border(1)
                    .Padding(5)
                    .Text(name)
                    .Bold();

                table.Cell()
                    .Border(1)
                    .Padding(5)
                    .AlignCenter()
                    .Text(!string.IsNullOrWhiteSpace(path)
                        ? "Uploaded"
                        : "Not Uploaded");
            }

            AddDocumentRow(
                "Sale Deed",
                landBuilding.SaleDeedDocumentPath);

            AddDocumentRow(
                "Encumbrance Certificate",
                landBuilding.EncumbranceCertificateDocumentPath);

            AddDocumentRow(
                "Land Use Certificate",
                landBuilding.LandUseCertificateDocumentPath);

            AddDocumentRow(
                "Approved Layout Plan",
                landBuilding.ApprovedLayoutPlanDocumentPath);

            AddDocumentRow(
                "Land Sketch",
                landBuilding.LandSketchDocumentPath);

            AddDocumentRow(
                "Distance Certificate",
                landBuilding.DistanceCertificateDocumentPath);

            AddDocumentRow(
                "Approved Building Plan",
                landBuilding.ApprovedBuildingPlanDocumentPath);

            AddDocumentRow(
                "Completion Certificate",
                landBuilding.CompletionCertificateDocumentPath);

            AddDocumentRow(
                "Structural Stability Certificate",
                landBuilding.StructuralStabilityCertificateDocumentPath);

            AddDocumentRow(
                "Fire Safety NOC",
                landBuilding.FireSafetyNocDocumentPath);

            AddDocumentRow(
                "Lift License",
                landBuilding.LiftLicenseDocumentPath);

            AddDocumentRow(
                "Electrical Safety Certificate",
                landBuilding.ElectricalSafetyCertificateDocumentPath);

            AddDocumentRow(
                "Water Supply Certificate",
                landBuilding.WaterSupplyCertificateDocumentPath);

            AddDocumentRow(
                "Sewage / Sanitation Approval",
                landBuilding.SewageSanitationApprovalDocumentPath);
        });
    }

    private void AddClassroomAndSkillsLaboratorySection(ColumnDescriptor col)
    {
        var skillsLab = _model?.DentalSkillsLaboratoryVM;

        if (skillsLab == null)
            return;

        AddMainHeading(col, "Classroom & Skills Laboratory");

        // =========================================================
        // A. SKILLS LABORATORY DETAILS
        // =========================================================

        AddSubHeading(col, "Skills Laboratory Details", 130);

        col.Item()
            .PaddingTop(5)
            .Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(2);
                    columns.RelativeColumn(1);
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
                        .AlignCenter()
                        .Text(value);
                }

                AddRow(
                    "Annual BDS Intake",
                    skillsLab.AnnualBdsIntake.ToString());

                AddRow(
                    "Total Area Required (Sq.m)",
                    skillsLab.TotalAreaRequiredSqm.ToString());

                AddRow(
                    "Total Area Available (Sq.m)",
                    skillsLab.TotalAreaAvailableSqm.ToString());

                AddRow(
                    "Area Deficiency (Sq.m)",
                    skillsLab.TotalAreaDeficiencySqm.ToString());

                AddRow(
                    "Number of Examination Rooms",
                    skillsLab.NumberOfExaminationRooms.ToString());

                AddRow(
                    "Number of Skill Stations",
                    skillsLab.NumberOfSkillStations.ToString());
            });


        // =========================================================
        // B. INFRASTRUCTURE COMPLIANCE
        // =========================================================

        AddSubHeading(col, "Infrastructure Compliance", 140);

        col.Item()
            .PaddingTop(5)
            .Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(2);
                    columns.ConstantColumn(90);
                });

                table.Header(header =>
                {
                    header.Cell()
                        .Border(1)
                        .Padding(5)
                        .Text("Requirement")
                        .Bold();

                    header.Cell()
                        .Border(1)
                        .Padding(5)
                        .AlignCenter()
                        .Text("Status")
                        .Bold();
                });

                void AddStatusRow(string requirement, bool? status)
                {
                    table.Cell()
                        .Border(1)
                        .Padding(5)
                        .Text(requirement);

                    table.Cell()
                        .Border(1)
                        .Padding(5)
                        .AlignCenter()
                        .Text(status == true ? "Yes" : "No");
                }

                AddStatusRow(
                    "6 Weeks Training Completed",
                    skillsLab.SixWeeksTrainingCompletedBeforeClinical);

                AddStatusRow(
                    "Minimum Four Examination Rooms",
                    skillsLab.HasMinFourExamRooms);

                AddStatusRow(
                    "Demo Room for Small Groups",
                    skillsLab.HasDemoRoomSmallGroups);

                AddStatusRow(
                    "Debrief Area",
                    skillsLab.HasDebriefArea);

                AddStatusRow(
                    "Faculty Coordinator Room",
                    skillsLab.HasFacultyCoordinatorRoom);

                AddStatusRow(
                    "Support Staff Room",
                    skillsLab.HasSupportStaffRoom);

                AddStatusRow(
                    "Storage for Mannequins",
                    skillsLab.HasStorageForMannequins);

                AddStatusRow(
                    "Video Recording Facility",
                    skillsLab.HasVideoRecordingFacility);

                AddStatusRow(
                    "Group & Individual Stations",
                    skillsLab.HasGroupAndIndividualStations);

                AddStatusRow(
                    "Required Trainers & Mannequins",
                    skillsLab.HasRequiredTrainersAndMannequins);

                AddStatusRow(
                    "Dedicated Technical Officer",
                    skillsLab.HasDedicatedTechnicalOfficer);

                AddStatusRow(
                    "Adequate Support Staff",
                    skillsLab.HasAdequateSupportStaff);

                AddStatusRow(
                    "Teaching Areas with AV Facility",
                    skillsLab.TeachingAreasHaveAV);

                AddStatusRow(
                    "Teaching Areas with Internet",
                    skillsLab.TeachingAreasHaveInternet);

                AddStatusRow(
                    "E-Learning Enabled",
                    skillsLab.SkillsLabEnabledForELearning);
            });


        // =========================================================
        // C. PRE-CLINICAL & SKILLS LABORATORY AREAS
        // =========================================================

        if (skillsLab.PreClinicalAndSkillsLabs?.Any() == true)
        {
            AddSubHeading(
                col,"Pre-Clinical & Skills Laboratory Areas");

            foreach (var labGroup in skillsLab.PreClinicalAndSkillsLabs
                .GroupBy(x => x.LaboratorySection))
            {
                // Group heading
                col.Item()
                    .PaddingTop(8)
                    .Text(labGroup.Key ?? "Laboratory Details")
                    .FontSize(11)
                    .SemiBold();

                col.Item()
                    .PaddingTop(4)
                    .Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(2);
                            columns.ConstantColumn(100);
                            columns.ConstantColumn(100);
                        });

                        table.Header(header =>
                        {
                            header.Cell()
                                .Border(1)
                                .Padding(5)
                                .Text("Laboratory")
                                .Bold();

                            header.Cell()
                                .Border(1)
                                .Padding(5)
                                .AlignCenter()
                                .Text("Required Area (Sq.ft)")
                                .Bold();

                            header.Cell()
                                .Border(1)
                                .Padding(5)
                                .AlignCenter()
                                .Text("Existing Area (Sq.ft)")
                                .Bold();
                        });

                        foreach (var lab in labGroup)
                        {
                            table.Cell()
                                .Border(1)
                                .Padding(5)
                                .Text(lab.LabName ?? "—");

                            table.Cell()
                                .Border(1)
                                .Padding(5)
                                .AlignCenter()
                                .Text(lab.RequiredAreaSqFt.ToString("0.##"));

                            table.Cell()
                                .Border(1)
                                .Padding(5)
                                .AlignCenter()
                                .Text(
                                    (lab.ExistingAreaSqFt ?? 0)
                                    .ToString("0.##"));
                        }
                    });
            }
        }
    }

    private void AddDentalChairDistributionSection(ColumnDescriptor col)
    {
        var chairs = _model?.DentalChairDistribution;

        if (chairs == null || !chairs.Any())
            return;

        AddMainHeading(col, "Dental Chair Distribution");

        AddSubHeading(col, "Dental Chair Requirements", 140);

        col.Item()
            .PaddingTop(5)
            .Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(40);   // Sl. No.
                    columns.RelativeColumn(1.5f); // Course
                    columns.ConstantColumn(60);   // Level
                    columns.ConstantColumn(70);   // Intake
                    columns.ConstantColumn(70);   // Slab
                    columns.ConstantColumn(80);   // Required
                    columns.ConstantColumn(80);   // Existing
                });

                table.Header(header =>
                {
                    header.Cell()
                        .Border(1)
                        .Padding(5)
                        .AlignCenter()
                        .Text("Sl. No.")
                        .Bold();

                    header.Cell()
                        .Border(1)
                        .Padding(5)
                        .Text("Course")
                        .Bold();

                    header.Cell()
                        .Border(1)
                        .Padding(5)
                        .AlignCenter()
                        .Text("Level")
                        .Bold();

                    header.Cell()
                        .Border(1)
                        .Padding(5)
                        .AlignCenter()
                        .Text("Intake")
                        .Bold();

                    header.Cell()
                        .Border(1)
                        .Padding(5)
                        .AlignCenter()
                        .Text("Seat Slab")
                        .Bold();

                    header.Cell()
                        .Border(1)
                        .Padding(5)
                        .AlignCenter()
                        .Text("Chairs Required")
                        .Bold();

                    header.Cell()
                        .Border(1)
                        .Padding(5)
                        .AlignCenter()
                        .Text("Chairs Existing")
                        .Bold();
                });

                int slNo = 1;

                foreach (var item in chairs)
                {
                    table.Cell()
                        .Border(1)
                        .Padding(5)
                        .AlignCenter()
                        .Text(slNo++.ToString());

                    table.Cell()
                        .Border(1)
                        .Padding(5)
                        .Text(item.CourseName ?? "—");

                    table.Cell()
                        .Border(1)
                        .Padding(5)
                        .AlignCenter()
                        .Text(item.CourseLevel ?? "—");

                    table.Cell()
                        .Border(1)
                        .Padding(5)
                        .AlignCenter()
                        .Text(item.SeatSlab.ToString());

                    table.Cell()
                        .Border(1)
                        .Padding(5)
                        .AlignCenter()
                        .Text(item.SeatSlab.ToString());

                    table.Cell()
                        .Border(1)
                        .Padding(5)
                        .AlignCenter()
                        .Text(item.ChairsRequired.ToString());

                    table.Cell()
                        .Border(1)
                        .Padding(5)
                        .AlignCenter()
                        .Text(item.ChairsExisting.ToString());
                }
            });
    }

    private void AddEquipmentListSection(ColumnDescriptor col)
    {
        var equipment = _model?.EquipmentPreviewVM;

        if (equipment?.Departments == null || !equipment.Departments.Any())
            return;

        AddMainHeading(col, "Equipment Details");

        foreach (var department in equipment.Departments)
        {
            if (department.Equipments == null || !department.Equipments.Any())
                continue;

            AddSubHeading(
                col,
                department.DepartmentName ?? "Department");

            col.Item()
                .PaddingTop(5)
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

                    table.Header(header =>
                    {
                        header.Cell()
                            .Border(1)
                            .Padding(5)
                            .AlignCenter()
                            .Text("Sl. No.")
                            .Bold();

                        header.Cell()
                            .Border(1)
                            .Padding(5)
                            .Text("Equipment")
                            .Bold();

                        header.Cell()
                            .Border(1)
                            .Padding(5)
                            .Text("Specification")
                            .Bold();

                        header.Cell()
                            .Border(1)
                            .Padding(5)
                            .AlignCenter()
                            .Text("One Unit\nRequired")
                            .Bold();

                        header.Cell()
                            .Border(1)
                            .Padding(5)
                            .AlignCenter()
                            .Text("One Unit\nExisting")
                            .Bold();

                        header.Cell()
                            .Border(1)
                            .Padding(5)
                            .AlignCenter()
                            .Text("Two Unit\nRequired")
                            .Bold();

                        header.Cell()
                            .Border(1)
                            .Padding(5)
                            .AlignCenter()
                            .Text("Two Unit\nExisting")
                            .Bold();
                    });

                    int slNo = 1;

                    foreach (var item in department.Equipments)
                    {
                        table.Cell()
                            .Border(1)
                            .Padding(5)
                            .AlignCenter()
                            .Text(slNo.ToString());

                        table.Cell()
                            .Border(1)
                            .Padding(5)
                            .Text(item.EquipmentName ?? "—");

                        table.Cell()
                            .Border(1)
                            .Padding(5)
                            .Text(item.Specification ?? "—");

                        table.Cell()
                            .Border(1)
                            .Padding(5)
                            .AlignCenter()
                            .Text(item.OneUnitReq?.ToString() ?? "—");

                        table.Cell()
                            .Border(1)
                            .Padding(5)
                            .AlignCenter()
                            .Text(item.OneUnitExisting?.ToString() ?? "—");

                        table.Cell()
                            .Border(1)
                            .Padding(5)
                            .AlignCenter()
                            .Text(item.TwoUnitReq?.ToString() ?? "—");

                        table.Cell()
                            .Border(1)
                            .Padding(5)
                            .AlignCenter()
                            .Text(item.TwoUnitExisting?.ToString() ?? "—");

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

        AddSubHeading(col, "Sanctioned Intake Details", 135);

        col.Item().PaddingTop(8).Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(3);  // Course Name
                columns.RelativeColumn(2);  // Sanctioned Intake
                columns.RelativeColumn(2);  // Eligible Seat Slab
                columns.RelativeColumn(2);  // Document
            });

            // ---- Header ----
            table.Header(header =>
            {
                header.Cell().Border(1).Padding(3).Text("Course Name").Bold();
                header.Cell().Border(1).Padding(3).AlignCenter().Text("Sanctioned Intake").Bold();
                header.Cell().Border(1).Padding(3).AlignCenter().Text("Eligible Seat Slab").Bold();
                header.Cell().Border(1).Padding(3).AlignCenter().Text("Document").Bold();
            });

            // ---- Body ----
            foreach (var item in intakeVM.Items)
            {
                table.Cell().Border(1).Padding(3).Text(item.CourseName);
                table.Cell().Border(1).Padding(3).AlignCenter().Text(item.SanctionedIntake);
                table.Cell().Border(1).Padding(3).AlignCenter().Text(item.EligibleSeatSlab ?? "—");
                table.Cell().Border(1).Padding(3).AlignCenter().Text(item.HasDocument ? "Available" : "—");
            }
        });
    }
    private void AddAffiliatedCoursesSection(ColumnDescriptor col)
    {
        var intakeDetails = _model?.InstitutionBasicVM;
        var courses = intakeDetails?.AffCoursesVM?.Items;

        if (courses == null || !courses.Any())
            return;

        // ---- Section Title ----
        AddSubHeading(col, "Course Details", 80);

        col.Item().PaddingTop(8).Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(4); // Course Name
                columns.ConstantColumn(70); // Recognized
                columns.RelativeColumn(3); // RGUHS Notification No
                columns.ConstantColumn(70); // Document
            });

            // ---- Header ----
            table.Header(header =>
            {
                header.Cell().Border(1).Padding(3).Text("Course Name").Bold();
                header.Cell().Border(1).Padding(3).AlignCenter().Text("Recognized").Bold();
                header.Cell().Border(1).Padding(3).Text("RGUHS Notification No").Bold();
                header.Cell().Border(1).Padding(3).AlignCenter().Text("Document").Bold();
            });

            // ---- Body ----
            foreach (var item in courses)
            {
                table.Cell().Border(1).Padding(3).Text(item.CourseName);
                table.Cell().Border(1).Padding(3).AlignCenter().Text(item.IsRecognized ? "Yes" : "No");
                table.Cell().Border(1).Padding(3).Text(item.RguhsNotificationNo ?? "—");
                table.Cell().Border(1).Padding(3).AlignCenter().Text(item.HasDocument ? "Available" : "—");
            }
        });
    }

    private void AddAffiliationCourseSection(ColumnDescriptor col)
    {
        var item = _model?.InstitutionBasicVM?.AffiliationCourseDetailVM;

        if (item == null) return;

        AddSubHeading(col, "Affiliated Course Details", 130);

        col.Item().PaddingTop(8).Table(table =>
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
            AddTextRow(table, "GoK Order", item.HasGokOrder ? "Available" : "—");
        });
    }

    private void AddDeanOrDirectorSection(ColumnDescriptor col)
    {
        var dean = _model?.InstitutionBasicVM.DeanOrDirectorDetailDisplayVM;
        if (dean == null) return;

        AddSubHeading(col, "Dean / Director Details", 125);

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
        var principal = _model?.InstitutionBasicVM.PrincipalDetailDisplayVM;
        if (principal == null) return;

        AddSubHeading(col, "Principal Details", 85);

        col.Item().PaddingTop(8).Table(table =>
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

        // Don't show empty section
        if (string.IsNullOrWhiteSpace(institution.HeadOfInstitution) &&
            string.IsNullOrWhiteSpace(institution.HeadOfInstitution_Mob_NO) &&
            string.IsNullOrWhiteSpace(institution.HeadOfInstitution_Email) &&
            string.IsNullOrWhiteSpace(institution.HeadAddress))
        {
            return;
        }

        AddSubHeading(col, "Head of Institution Details", 140);

        col.Item()
            .PaddingTop(8)
            .Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(3);
                    columns.RelativeColumn(2);
                });

                AddTextRow( table, "Head of Institution", institution.HeadOfInstitution ?? "—");
                AddTextRow( table, "Head Mobile No", institution.HeadOfInstitution_Mob_NO ?? "—");
                AddTextRow( table, "Head Email", institution.HeadOfInstitution_Email ?? "—");
                AddTextRow( table, "Head Address", institution.HeadAddress ?? "—");
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

        AddSubHeading(col, "Trust / Society Details", 120);

        col.Item()
            .PaddingTop(8)
            .Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(3);
                    columns.RelativeColumn(2);
                });

                AddTextRow(
                    table,
                    "Trust Name",
                    string.IsNullOrWhiteSpace(trust.TrustName)
                        ? "—"
                        : trust.TrustName);

                AddTextRow(
                    table,
                    "TRUST PAN Number",
                    string.IsNullOrWhiteSpace(trust.PANNumber)
                        ? "—"
                        : trust.PANNumber);

                AddTextRow(
                    table,
                    "Registration Number",
                    string.IsNullOrWhiteSpace(trust.RegistrationNumber)
                        ? "—"
                        : trust.RegistrationNumber);

                AddTextRow(
                    table,
                    "Registration Date",
                    trust.RegistrationDate.HasValue
                        ? trust.RegistrationDate.Value.ToString("dd MMMM yyyy")
                        : "—");

                AddTextRow(
                    table,
                    "President Name",
                    string.IsNullOrWhiteSpace(trust.PresidentName)
                        ? "—"
                        : trust.PresidentName);

                AddTextRow(
                    table,
                    "Category of Organisation",
                    string.IsNullOrWhiteSpace(trust.CategoryOfOrganisation)
                        ? "—"
                        : trust.CategoryOfOrganisation);

                AddTextRow(
                    table,
                    "GOK Obtained Trust Name",
                    string.IsNullOrWhiteSpace(trust.GOKObtainedTrustName)
                        ? "—"
                        : trust.GOKObtainedTrustName);

                AddTextRow(
                    table,
                    "Amendments",
                    trust.Amendments.HasValue
                        ? (trust.Amendments.Value ? "Yes" : "No")
                        : "—");
            });


        // =========================================================
        // TRUST CONTACT & COMMUNICATION
        // =========================================================

        AddSubHeading(col, "Trust Contact & Communication", 170);

        col.Item()
            .PaddingTop(8)
            .Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(3);
                    columns.RelativeColumn(2);
                });

                AddTextRow(
                    table,
                    "Address",
                    string.IsNullOrWhiteSpace(trust.Address)
                        ? "—"
                        : trust.Address);

                AddTextRow(
                    table,
                    "PIN Code",
                    string.IsNullOrWhiteSpace(trust.PinCode)
                        ? "—"
                        : trust.PinCode);

                AddTextRow(
                    table,
                    "Mobile Number",
                    string.IsNullOrWhiteSpace(trust.MobileNumber)
                        ? "—"
                        : trust.MobileNumber);

                AddTextRow(
                    table,
                    "STD Code",
                    string.IsNullOrWhiteSpace(trust.StdCode)
                        ? "—"
                        : trust.StdCode);

                AddTextRow(
                    table,
                    "Fax",
                    string.IsNullOrWhiteSpace(trust.Fax)
                        ? "—"
                        : trust.Fax);

                AddTextRow(
                    table,
                    "Alternate Landline / Mobile",
                    string.IsNullOrWhiteSpace(trust.AltLandlineOrMobile)
                        ? "—"
                        : trust.AltLandlineOrMobile);

                AddTextRow(
                    table,
                    "Email ID",
                    string.IsNullOrWhiteSpace(trust.EmailId)
                        ? "—"
                        : trust.EmailId);

                AddTextRow(
                    table,
                    "Alternate Email ID",
                    string.IsNullOrWhiteSpace(trust.AltEmailId)
                        ? "—"
                        : trust.AltEmailId);
            });


        // =========================================================
        // TRUST CONTACT PERSON
        // =========================================================

        AddSubHeading(col, "Trust Contact Person", 110);

        col.Item()
            .PaddingTop(8)
            .Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(3);
                    columns.RelativeColumn(2);
                });

                AddTextRow(
                    table,
                    "Full Name",
                    string.IsNullOrWhiteSpace(trust.ContactPersonName)
                        ? "—"
                        : trust.ContactPersonName);

                AddTextRow(
                    table,
                    "Designation",
                    string.IsNullOrWhiteSpace(trust.ContactPersonRelation)
                        ? "—"
                        : trust.ContactPersonRelation);

                AddTextRow(
                    table,
                    "Mobile",
                    string.IsNullOrWhiteSpace(trust.ContactPersonMobile)
                        ? "—"
                        : trust.ContactPersonMobile);
            });


        // =========================================================
        // OTHER TRUST INFORMATION
        // =========================================================

        if (!string.IsNullOrWhiteSpace(trust.ExistingTrustName) ||
            trust.ChangesInTrustName.HasValue)
        {
            AddSubHeading(col, "Other Trust Information", 128);

            col.Item()
                .PaddingTop(8)
                .Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(3);
                        columns.RelativeColumn(2);
                    });

                    AddTextRow(
                        table,
                        "Existing Trust Name",
                        string.IsNullOrWhiteSpace(trust.ExistingTrustName)
                            ? "—"
                            : trust.ExistingTrustName);

                    AddTextRow(
                        table,
                        "Changes in Trust Name",
                        trust.ChangesInTrustName.HasValue
                            ? (trust.ChangesInTrustName.Value ? "Yes" : "No")
                            : "—");
                });
        }


        // =========================================================
        // TRUST DOCUMENTS
        // =========================================================

        AddSubHeading(col, "Trust Documents", 92);

        col.Item()
            .PaddingTop(8)
            .Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(3);
                    columns.RelativeColumn(2);
                });

                AddTextRow(
                    table,
                    "Trust PAN File",
                    trust.HasPANFile ? "Uploaded" : "Not Uploaded");

                AddTextRow(
                    table,
                    "Bank Statement",
                    trust.HasBankStatementFile ? "Uploaded" : "Not Uploaded");

                AddTextRow(
                    table,
                    "Registration Certificate",
                    trust.HasRegistrationCertificateFile
                        ? "Uploaded"
                        : "Not Uploaded");

                AddTextRow(
                    table,
                    "Audit Statement",
                    trust.HasAuditStatementFile
                        ? "Uploaded"
                        : "Not Uploaded");

                AddTextRow(
                    table,
                    "Amended Document",
                    trust.HasAmendedDoc
                        ? "Uploaded"
                        : "Not Uploaded");

                AddTextRow(
                    table,
                    "GOK Order — Existing Courses",
                    trust.HasGokOrderExistingCoursesFile
                        ? "Uploaded"
                        : "Not Uploaded");

                AddTextRow(
                    table,
                    "Registered Trust Member Details",
                    trust.HasRegisteredTrustMemberDetails
                        ? "Uploaded"
                        : "Not Uploaded");

                AddTextRow(
                    table,
                    "Aadhaar File",
                    trust.HasAadhaarFile
                        ? "Uploaded"
                        : "Not Uploaded");

                AddTextRow(
                    table,
                    "Gov Autonomous Certificate",
                    trust.HasGovAutonomousCertFile
                        ? "Uploaded"
                        : "Not Uploaded");

                AddTextRow(
                    table,
                    "Gov Council Membership",
                    trust.HasGovCouncilMembershipFile
                        ? "Uploaded"
                        : "Not Uploaded");

                AddTextRow(
                    table,
                    "First Affiliation Notification",
                    trust.HasFirstAffiliationNotifFile
                        ? "Uploaded"
                        : "Not Uploaded");

                AddTextRow(
                    table,
                    "Continuation Affiliation",
                    trust.HasContinuationAffiliationFile
                        ? "Uploaded"
                        : "Not Uploaded");

                AddTextRow(
                    table,
                    "KNC Certificate",
                    trust.HasKncCertificateFile
                        ? "Uploaded"
                        : "Not Uploaded");

                AddTextRow(
                    table,
                    "DCI Certificate",
                    trust.HasDCIFile
                        ? "Uploaded"
                        : "Not Uploaded");

                AddTextRow(
                    table,
                    "KSDC Certificate",
                    trust.HasKSDCFile
                        ? "Uploaded"
                        : "Not Uploaded");
            });
    }

    private void AddAcademicIntakeCourseLevelSection(ColumnDescriptor col, string level, List<IntakeByLevelViewModel1> courses)
    {
        if (courses == null || !courses.Any())
            return;

        // =========================
        // Display Level Name
        // =========================

        var displayLevel = level?.Trim().ToUpper() switch
        {
            "UG" => "Under Graduate (UG)",
            "PG" => "Post Graduate (PG)",
            "SS" => "Super Specialty (SS)",
            _ => level
        };

        // =========================
        // Section Heading
        // =========================

        AddSubHeading(col, $"{displayLevel} Courses");

        // =========================
        // Course Count
        // =========================

        col.Item()
            .PaddingTop(4)
            .PaddingBottom(8)
            .Text(
                $"Intake details for {displayLevel} programmes.  |  {courses.Count} Course(s)"
            )
            .FontSize(9);

        // =========================
        // Check AY 2026-27
        // =========================

        var show2026 = courses.Any(x =>
            (x.AY2026_ExistingIntake ?? 0) > 0 ||
            (x.AY2026_AddRequestedIntake ?? 0) > 0 ||
            (x.AY2026_TotalIntake ?? 0) > 0);

        // =========================
        // Table
        // =========================

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

                // =========================
                // Header
                // =========================

                table.Header(header =>
                {
                    header.Cell()
                        .Element(AcademicIntakeHeaderCell)
                        .Text("Course");

                    header.Cell()
                        .Element(AcademicIntakeHeaderCell)
                        .Text("AY 2025-26\nExisting");

                    header.Cell()
                        .Element(AcademicIntakeHeaderCell)
                        .Text("AY 2025-26\nLoP / NMC");

                    header.Cell()
                        .Element(AcademicIntakeHeaderCell)
                        .Text("AY 2025-26\nTotal");

                    if (show2026)
                    {
                        header.Cell()
                            .Element(AcademicIntakeHeaderCell)
                            .Text("AY 2026-27\nExisting");

                        header.Cell()
                            .Element(AcademicIntakeHeaderCell)
                            .Text("AY 2026-27\nRequested");

                        header.Cell()
                            .Element(AcademicIntakeHeaderCell)
                            .Text("AY 2026-27\nTotal");
                    }
                });

                // =========================
                // Rows
                // =========================

                foreach (var item in courses)
                {
                    // Course
                    table.Cell()
                        .Element(AcademicIntakeDataCell)
                        .Text(text =>
                        {
                            text.Span(
                                string.IsNullOrWhiteSpace(item.CourseName)
                                    ? "—"
                                    : item.CourseName)
                                .Bold();

                            text.EmptyLine();

                            text.Span(
                                string.IsNullOrWhiteSpace(item.CourseCode)
                                    ? "—"
                                    : item.CourseCode)
                                .FontSize(8);
                        });

                    // AY 2025-26 Existing
                    table.Cell()
                        .Element(AcademicIntakeDataCell)
                        .AlignCenter()
                        .Text(
                            (item.AY2025_ExistingIntake ?? 0)
                                .ToString());

                    // AY 2025-26 LoP / NMC
                    table.Cell()
                        .Element(AcademicIntakeDataCell)
                        .AlignCenter()
                        .Text(
                            (item.AY2025_LopNmcIntake ?? 0)
                                .ToString());

                    // AY 2025-26 Total
                    table.Cell()
                        .Element(AcademicIntakeDataCell)
                        .AlignCenter()
                        .Text(
                            (item.AY2025_TotalIntake ?? 0)
                                .ToString());

                    // =========================
                    // AY 2026-27
                    // =========================

                    if (show2026)
                    {
                        // Existing
                        table.Cell()
                            .Element(AcademicIntakeDataCell)
                            .AlignCenter()
                            .Text(
                                (item.AY2026_ExistingIntake ?? 0)
                                    .ToString());

                        // Requested
                        table.Cell()
                            .Element(AcademicIntakeDataCell)
                            .AlignCenter()
                            .Text(
                                (item.AY2026_AddRequestedIntake ?? 0)
                                    .ToString());

                        // Total
                        table.Cell()
                            .Element(AcademicIntakeDataCell)
                            .AlignCenter()
                            .Text(
                                (item.AY2026_TotalIntake ?? 0)
                                    .ToString());
                    }
                }
            });
    }


    private void AddAcademicIntakeSection(ColumnDescriptor col)
    {
        var intake = _model?.AcademicIntakeVM;

        if (intake == null)
            return;

        if (intake.SortedCourseLevels == null ||
            !intake.SortedCourseLevels.Any())
            return;

        AddMainHeading(col, "Academic Intake Details");

        foreach (var level in intake.SortedCourseLevels)
        {
            List<IntakeByLevelViewModel1>? courses = level switch
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
                level,
                courses);
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
        var academic = _model.CAacademicMattersVM;
        if (academic == null) return;

        // --- Section Title ---
        col.Item().PaddingTop(25)
            .AlignCenter()
            .Text("Academic Matters")
            .FontSize(14)
            .Bold();

        // --- Academic Performance Subtitle ---
        col.Item().PaddingTop(5).Column(col2 =>
        {
            col2.Item().Text("Academic Performance")
                .FontSize(12)
                .SemiBold();

            col2.Item().PaddingTop(2).Row(row =>
            {
                row.ConstantItem(125)   // adjust length to fit text nicely
                    .LineHorizontal(1)
                    .LineColor(Colors.Black);

                row.RelativeItem();
            });
        });

        //col.Item().Text("Year-wise results, pass percentage and classifications");

        var rows = academic.AcademicRows;

        // --- Academic Performance Table ---
        col.Item().PaddingTop(10).Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn();      // Year
                columns.ConstantColumn(60);   // Regular
                columns.ConstantColumn(70);   // Repeaters
                columns.ConstantColumn(60);   // Passed
                columns.ConstantColumn(60);   // Pass %
                columns.ConstantColumn(70);   // First Class
                columns.ConstantColumn(80);   // Distinction
                columns.RelativeColumn();     // Remarks
            });

            table.Header(header =>
            {
                header.Cell().Border(1).Padding(5).Text("Year").Bold().AlignCenter();
                header.Cell().Border(1).Padding(5).Text("Regular").Bold().AlignCenter();
                header.Cell().Border(1).Padding(5).Text("Repeaters").Bold().AlignCenter();
                header.Cell().Border(1).Padding(5).Text("Passed").Bold().AlignCenter();
                header.Cell().Border(1).Padding(5).Text("Pass %").Bold().AlignCenter();
                header.Cell().Border(1).Padding(5).Text("First Class").Bold().AlignCenter();
                header.Cell().Border(1).Padding(5).Text("Distinction").Bold().AlignCenter();
                header.Cell().Border(1).Padding(5).Text("Remarks").Bold();
            });

            foreach (var row in rows)
            {
                table.Cell().Border(1).Padding(2).AlignCenter().Text(row.YearName);
                table.Cell().Border(1).Padding(2).AlignCenter().Text(row.RegularStudents.ToString());
                table.Cell().Border(1).Padding(2).AlignCenter().Text(row.RepeaterStudents.ToString());
                table.Cell().Border(1).Padding(2).AlignCenter().Text(row.NumberOfStudentsPassed.ToString());
                table.Cell().Border(1).Padding(2).AlignCenter().Text((row.PassPercentage ?? 0).ToString("0.00"));
                table.Cell().Border(1).Padding(2).AlignCenter().Text(row.FirstClassCount.ToString());
                table.Cell().Border(1).Padding(2).AlignCenter().Text(row.DistinctionCount.ToString());
                table.Cell().Border(1).Padding(2).AlignCenter().Text(string.IsNullOrEmpty(row.Remarks) ? "—" : row.Remarks);
            }
        });

        // --- Course Curriculum Section ---
        if (academic.CourseCurriculumdvm != null && academic.CourseCurriculumdvm.Any())
        {
            // ---- Course Curriculum ----
            col.Item().PaddingTop(15).Column(col2 =>
            {
                col2.Item().Text("Course Curriculum")
                    .FontSize(12)
                    .Bold();

                col2.Item().PaddingTop(2).Row(row =>
                {
                    row.ConstantItem(100)   // adjust length to fit text nicely
                        .LineHorizontal(1)
                        .LineColor(Colors.Black);

                    row.RelativeItem();
                });
            });




            col.Item().PaddingTop(8).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(4); // Curriculum Name
                    columns.RelativeColumn(4); // Details
                    columns.ConstantColumn(80); // Uploaded
                });

                table.Header(header =>
                {
                    header.Cell().Border(1).Padding(5).Text("Curriculum").Bold();
                    header.Cell().Border(1).Padding(5).Text("Details").Bold();
                    header.Cell().Border(1).Padding(5).AlignCenter().Text("Uploaded").Bold();
                });

                foreach (var item in academic.CourseCurriculumdvm)
                {
                    table.Cell().Border(1).Padding(5).Text(item.CurriculumName);
                    table.Cell().Border(1).Padding(5).Text(item.CurriculumDetails ?? "—");
                    table.Cell().Border(1).Padding(5).AlignCenter().Text(item.HasPdf ? "Yes" : "No");
                }
            });
        }

        // --- Examination Schemes Section ---
        if (academic.ExaminationSchemes != null && academic.ExaminationSchemes.Any())
        {
            // ---- Academic Performance ----
            col.Item().PaddingTop(15).Column(col2 =>
            {
                col2.Item().Text("Examination Schemes")
                    .FontSize(12)
                    .Bold();

                col2.Item().PaddingTop(2).Row(row =>
                {
                    row.ConstantItem(120)   // adjust length to fit text nicely
                        .LineHorizontal(1)
                        .LineColor(Colors.Black);

                    row.RelativeItem();
                });
            });

            col.Item().PaddingTop(8).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn();   // Scheme Code
                    columns.ConstantColumn(120); // Number of Students
                });

                table.Header(header =>
                {
                    header.Cell().Border(1).Padding(5).Text("Scheme Code").Bold();
                    header.Cell().Border(1).Padding(5).AlignCenter().Text("Number of Students").Bold();
                });

                foreach (var scheme in academic.ExaminationSchemes)
                {
                    table.Cell().Border(1).Padding(5).Text(scheme.SchemeCode);
                    table.Cell().Border(1).Padding(5).AlignCenter().Text(scheme.NumberOfStudents.ToString());
                }
            });
        }

        // --- Student Register Records Section ---
        if (academic.StudentRegisterRecords != null && academic.StudentRegisterRecords.Any())
        {
            // ---- Academic Performance ----
            col.Item().PaddingTop(15).Column(col2 =>
            {
                col2.Item().Text("Student Register Records")
                    .FontSize(12)
                    .SemiBold();

                col2.Item().PaddingTop(2).Row(row =>
                {
                    row.ConstantItem(130)   // adjust length to fit text nicely
                        .LineHorizontal(1)
                        .LineColor(Colors.Black);

                    row.RelativeItem();
                });
            });
            col.Item().PaddingTop(8).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn();      // Register Name
                    columns.ConstantColumn(120);   // Maintained
                });

                table.Header(header =>
                {
                    header.Cell().Border(1).Padding(5).Text("Register Name").Bold();
                    header.Cell().Border(1).Padding(5).AlignCenter().Text("Maintenance Status").Bold();
                });

                foreach (var record in academic.StudentRegisterRecords)
                {
                    table.Cell().Border(1).Padding(5).Text(record.RegisterName);
                    table.Cell().Border(1).Padding(5).AlignCenter().Text(record.IsExists ? "Yes" : "No");
                }
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

        col.Item()
           .PaddingTop(30)
           .AlignCenter()
           .Text("Hospital Affiliation")
           .FontSize(14)
           .Bold();


        // =========================================================
        // 1. CLINICAL HOSPITAL DETAILS
        // =========================================================

        if (hospital.ClinicalHospitalDetails != null)
        {
            var h = hospital.ClinicalHospitalDetails;

            AddSubHeading(col, "Clinical Hospital Details", 130);

            col.Item().PaddingTop(5).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                });

                void AddRow(string property, string value)
                {
                    table.Cell()
                        .Border(1)
                        .Padding(5)
                        .Text(property)
                        .Bold();

                    table.Cell()
                        .Border(1)
                        .Padding(5)
                        .Text(value);
                }

                AddRow("Hospital Name", h.HospitalName ?? "—");
                AddRow("Hospital Type", h.HospitalType ?? "—");
                AddRow("Hospital Owned By", h.HospitalOwnedBy ?? "—");
                AddRow("Owner Name", h.OwnerName ?? "—");
                AddRow("Location", $"{h.DistrictName ?? "—"}, {h.TalukName ?? "—"}");
                AddRow("Total Beds", h.TotalBeds.ToString());
                AddRow("OPD per Day", h.OpdPerDay.ToString());
                AddRow("IPD Occupancy %", h.IpdOccupancyPercent.ToString());
                AddRow(
                    "Member of Trust",
                    h.IsOwnerAmemberOfTrust ? "Yes" : "No");

                AddRow(
                    "Supporting Documents Uploaded",
                    h.IsSupportingDocExists ? "Yes" : "No");
            });
        }


        // =========================================================
        // 2. AFFILIATED HOSPITAL DOCUMENTS
        // =========================================================

        if (hospital.AffiliatedHospitalDocuments?.Any() == true)
        {
            AddSubHeading(col, "Affiliated Documents");

            col.Item().PaddingTop(5).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                    columns.ConstantColumn(60);
                    columns.ConstantColumn(70);
                });

                table.Header(header =>
                {
                    header.Cell()
                        .Border(1)
                        .Padding(5)
                        .Text("Document Name")
                        .Bold();

                    header.Cell()
                        .Border(1)
                        .Padding(5)
                        .Text("Hospital Name")
                        .Bold();

                    header.Cell()
                        .Border(1)
                        .Padding(5)
                        .Text("Beds")
                        .Bold();

                    header.Cell()
                        .Border(1)
                        .Padding(5)
                        .Text("Exists")
                        .Bold();
                });

                foreach (var doc in hospital.AffiliatedHospitalDocuments)
                {
                    table.Cell()
                        .Border(1)
                        .Padding(5)
                        .Text(doc.DocumentName ?? "—");

                    table.Cell()
                        .Border(1)
                        .Padding(5)
                        .Text(doc.HospitalName ?? "—");

                    table.Cell()
                        .Border(1)
                        .Padding(5)
                        .AlignCenter()
                        .Text(doc.TotalBeds.ToString());

                    table.Cell()
                        .Border(1)
                        .Padding(5)
                        .AlignCenter()
                        .Text(doc.DocumentExists ? "Yes" : "No");
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

            col.Item().PaddingTop(5).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(40);
                    columns.RelativeColumn();
                    columns.ConstantColumn(90);
                });

                table.Header(header =>
                {
                    header.Cell()
                        .Border(1)
                        .Padding(5)
                        .AlignCenter()
                        .Text("Sl. No.")
                        .Bold();

                    header.Cell()
                        .Border(1)
                        .Padding(5)
                        .Text("Discipline")
                        .Bold();

                    header.Cell()
                        .Border(1)
                        .Padding(5)
                        .AlignCenter()
                        .Text("Available")
                        .Bold();
                });

                int slNo = 1;

                foreach (var item in discipline.Disciplines)
                {
                    table.Cell()
                        .Border(1)
                        .Padding(5)
                        .AlignCenter()
                        .Text(slNo.ToString());

                    table.Cell()
                        .Border(1)
                        .Padding(5)
                        .Text(item.DisciplineName ?? "—");

                    table.Cell()
                        .Border(1)
                        .Padding(5)
                        .AlignCenter()
                        .Text(item.IsSelected ? "Yes" : "No");

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

            AddSubHeading(col, "Services", 45);

            col.Item().PaddingTop(5).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(40);
                    columns.RelativeColumn();
                    columns.ConstantColumn(90);
                });

                table.Header(header =>
                {
                    header.Cell()
                        .Border(1)
                        .Padding(5)
                        .AlignCenter()
                        .Text("Sl. No.")
                        .Bold();

                    header.Cell()
                        .Border(1)
                        .Padding(5)
                        .Text("Requirement")
                        .Bold();

                    header.Cell()
                        .Border(1)
                        .Padding(5)
                        .AlignCenter()
                        .Text("Available")
                        .Bold();
                });

                int slNo = 1;

                foreach (var item in allied.Requirements)
                {
                    table.Cell()
                        .Border(1)
                        .Padding(5)
                        .AlignCenter()
                        .Text(slNo.ToString());

                    table.Cell()
                        .Border(1)
                        .Padding(5)
                        .Text(item.RequirementName ?? "—");

                    table.Cell()
                        .Border(1)
                        .Padding(5)
                        .AlignCenter()
                        .Text(
                            item.IsAvailable == true
                                ? "Yes"
                                : "No");

                    slNo++;
                }
            });
        }


        // =========================================================
        // 5. DENTAL WARD BED DISTRIBUTION
        // =========================================================

        if (hospital.DentalWardBedDistribution?.Any() == true)
        {
            AddSubHeading(col, "Dental Ward Bed Distribution", 160);

            col.Item().PaddingTop(5).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(40);
                    columns.RelativeColumn();
                    columns.ConstantColumn(80);
                    columns.ConstantColumn(80);
                });

                table.Header(header =>
                {
                    header.Cell()
                        .Border(1)
                        .Padding(5)
                        .AlignCenter()
                        .Text("Sl. No.")
                        .Bold();

                    header.Cell()
                        .Border(1)
                        .Padding(5)
                        .Text("Ward")
                        .Bold();

                    header.Cell()
                        .Border(1)
                        .Padding(5)
                        .AlignCenter()
                        .Text("Beds Required")
                        .Bold();

                    header.Cell()
                        .Border(1)
                        .Padding(5)
                        .AlignCenter()
                        .Text("Beds Present")
                        .Bold();
                });

                int slNo = 1;

                foreach (var ward in hospital.DentalWardBedDistribution)
                {
                    table.Cell()
                        .Border(1)
                        .Padding(5)
                        .AlignCenter()
                        .Text(slNo.ToString());

                    table.Cell()
                        .Border(1)
                        .Padding(5)
                        .Text(ward.WardName ?? "—");

                    table.Cell()
                        .Border(1)
                        .Padding(5)
                        .AlignCenter()
                        .Text(ward.BedsRequired.ToString());

                    table.Cell()
                        .Border(1)
                        .Padding(5)
                        .AlignCenter()
                        .Text(
                            ward.BedsPresent?.ToString() ?? "—");

                    slNo++;
                }
            });
        }
    }

    private void AddMedicalUGBedDistributionSection(ColumnDescriptor col)
    {
        var bedDistribution = _model?.MedicalUGBedDistributionVM;

        if (bedDistribution == null)
            return;

        // =========================================================
        // MEDICAL
        // =========================================================

        if (bedDistribution.Medical != null)
        {
            AddMainHeading(col, "Medical UG Bed Distribution");

            // Existing medical PDF rendering here...
        }

        // =========================================================
        // DENTAL
        // =========================================================

        if (bedDistribution.Dental != null)
        {
            AddMainHeading(col, "Bed Distribution");

            AddSubHeading(col, "Oral & Maxillofacial Surgery");

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
                        .Text("Oral & Maxillofacial Surgery")
                        .Bold();

                    table.Cell()
                        .Border(1)
                        .Padding(5)
                        .AlignCenter()
                        .Text(
                            bedDistribution.Dental
                                .OralMaxillofacialSurgery?
                                .ToString() ?? "0");
                });


            // =====================================================
            // DENTAL WARDS
            // =====================================================

            //if (bedDistribution.Dental.DentalWards?.Any() == true)
            //{
            //    AddSubHeading(col, "Dental Ward Bed Distribution");

            //    col.Item()
            //        .PaddingTop(5)
            //        .Table(table =>
            //        {
            //            table.ColumnsDefinition(columns =>
            //            {
            //                columns.ConstantColumn(45);  // Sl No
            //                columns.RelativeColumn(2);   // Ward
            //                columns.ConstantColumn(80);  // Seat Slab
            //                columns.ConstantColumn(90);  // Required
            //                columns.ConstantColumn(90);  // Present
            //            });

            //            table.Header(header =>
            //            {
            //                header.Cell()
            //                    .Border(1)
            //                    .Padding(5)
            //                    .AlignCenter()
            //                    .Text("Sl. No.")
            //                    .Bold();

            //                header.Cell()
            //                    .Border(1)
            //                    .Padding(5)
            //                    .Text("Ward")
            //                    .Bold();

            //                header.Cell()
            //                    .Border(1)
            //                    .Padding(5)
            //                    .AlignCenter()
            //                    .Text("Seat Slab")
            //                    .Bold();

            //                header.Cell()
            //                    .Border(1)
            //                    .Padding(5)
            //                    .AlignCenter()
            //                    .Text("Beds Required")
            //                    .Bold();

            //                header.Cell()
            //                    .Border(1)
            //                    .Padding(5)
            //                    .AlignCenter()
            //                    .Text("Beds Present")
            //                    .Bold();
            //            });

            //            int slNo = 1;

            //            foreach (var ward in bedDistribution.Dental.DentalWards)
            //            {
            //                table.Cell()
            //                    .Border(1)
            //                    .Padding(5)
            //                    .AlignCenter()
            //                    .Text(slNo++.ToString());

            //                table.Cell()
            //                    .Border(1)
            //                    .Padding(5)
            //                    .Text(ward.WardName ?? "—");

            //                table.Cell()
            //                    .Border(1)
            //                    .Padding(5)
            //                    .AlignCenter()
            //                    .Text(ward.SeatSlab.ToString());

            //                table.Cell()
            //                    .Border(1)
            //                    .Padding(5)
            //                    .AlignCenter()
            //                    .Text(ward.BedsRequired.ToString());

            //                table.Cell()
            //                    .Border(1)
            //                    .Padding(5)
            //                    .AlignCenter()
            //                    .Text(
            //                        ward.BedsPresent?.ToString() ?? "0");
            //            }
            //        });
            //}
        }
    }

    private void AddDepartmentSections(ColumnDescriptor col)
    {
        var hospital = _model.CAHospitalAFfiliationCompVM;
        if (hospital?.Sections == null || !hospital.Sections.Any())
            return;

        // Loop through each department/section
        foreach (var section in hospital.Sections)
        {
            // Section heading
            col.Item().PaddingTop(15).Row(row =>
            {
                // Text
                row.AutoItem().Column(colText =>
                {
                    colText.Item().Text(section.SectionName)
                        .FontSize(12)
                        .SemiBold();

                    // Underline matching text width
                    colText.Item().PaddingTop(2)
                        .LineHorizontal(1)
                        .LineColor(Colors.Black);
                });

                // Fill remaining space
                row.RelativeItem();
            });


            // Table for section items
            if (section.Items != null && section.Items.Any())
            {
                col.Item().PaddingTop(8).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(4); // Requirement Name
                        columns.ConstantColumn(80); // Compliance
                        //columns.RelativeColumn(3); // Remarks
                    });

                    // Header row
                    table.Header(header =>
                    {
                        header.Cell().Border(1).Padding(5).Text("Requirement").Bold();
                        header.Cell().Border(1).Padding(5).AlignCenter().Text("Compliant").Bold();
                        //header.Cell().Border(1).Padding(5).Text("Remarks").Bold();
                    });

                    // Data rows
                    foreach (var item in section.Items)
                    {
                        table.Cell().Border(1).Padding(5).Text(item.RequirementName);
                        table.Cell().Border(1).Padding(5).AlignCenter().Text(item.IsCompliant ? "Yes" : "No");
                        //table.Cell().Border(1).Padding(5).Text(string.IsNullOrEmpty(item.Remarks) ? "—" : item.Remarks);
                    }
                });
            }
        }
    }


    private void AddIndoorBedsOccupancySection(ColumnDescriptor col)
    {
        var hospitalData = _model.CAHospitalAFfiliationCompVM;
        if (hospitalData.IndoorBedsOccupancy == null || !hospitalData.IndoorBedsOccupancy.Any())
            return;

        // --- Section Heading ---
        // Main heading text
        col.Item()
            .PaddingTop(20)
            .AlignCenter()
            .Text("Indoor Beds Occupancy")
            .FontSize(14)
            .Bold();

        // --- Table ---
        col.Item().PaddingTop(10).Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(4);   // Department Name
                columns.RelativeColumn(2);   // Seat Slab / Intake
                columns.ConstantColumn(60);  // RGUHS Intake
                columns.ConstantColumn(60);  // College Intake
            });

            // Header row
            table.Header(header =>
            {
                header.Cell().Border(1).Padding(5).Text("Department Name").Bold();
                header.Cell().Border(1).Padding(5).Text("Seat Slab / Intake").Bold();
                header.Cell().Border(1).Padding(5).AlignCenter().Text("RGUHS Intake").Bold();
                header.Cell().Border(1).Padding(5).AlignCenter().Text("College Intake").Bold();
            });

            // Data rows
            foreach (var item in hospitalData.IndoorBedsOccupancy)
            {
                table.Cell().Border(1).Padding(5).Text(item.DepartmentName);
                table.Cell().Border(1).Padding(5).Text(item.SeatSlabId.ToString());
                table.Cell().Border(1).Padding(5).AlignCenter().Text(item.RGUHSintake.ToString());
                table.Cell().Border(1).Padding(5).AlignCenter().Text(item.CollegeIntake.ToString());
            }
        });
    }

    private void AddSupervisionInFieldPracticeAreaSection(ColumnDescriptor col)
    {
        var supervisionList = _model.CAHospitalAFfiliationCompVM?.SuperVisionInFPa;
        if (supervisionList == null || !supervisionList.Any())
            return;

        var supervision = supervisionList.First(); // one per college
        if (supervision.Items == null || !supervision.Items.Any())
            return;

        col.Item().PaddingTop(30)
                .AlignCenter()
                .Text("Supervision in Field Practice Area")
                .FontSize(14)
                .Bold();
       

        // ---- TABLE ----
        col.Item().PaddingTop(15).Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.ConstantColumn(60);   // Post
                columns.RelativeColumn(2);    // Name
                columns.RelativeColumn(2);    // Qualification
                columns.ConstantColumn(70);   // Year
                columns.RelativeColumn(2);    // University
                columns.ConstantColumn(90);   // UG Period
                columns.ConstantColumn(90);   // PG Period
                columns.RelativeColumn(3);    // Responsibilities
            });

            // ---- HEADER ----
            table.Header(header =>
            {
                header.Cell().Border(1).Padding(2).Text("Post").Bold();
                header.Cell().Border(1).Padding(2).Text("Name").Bold();
                header.Cell().Border(1).Padding(2).Text("Qualification").Bold();
                header.Cell().Border(1).Padding(2).Text("Year").Bold();
                header.Cell().Border(1).Padding(2).Text("University").Bold();
                header.Cell().Border(1).Padding(2).Text("UG Period").Bold();
                header.Cell().Border(1).Padding(2).Text("PG Period").Bold();
                header.Cell().Border(1).Padding(2).Text("Responsibilities").Bold();
            });

            // ---- ROWS ----
            foreach (var item in supervision.Items)
            {
                table.Cell().Border(1).Padding(5).Text(item.Post);
                table.Cell().Border(1).Padding(5).Text(item.Name);
                table.Cell().Border(1).Padding(5).Text(item.Qualification);
                table.Cell().Border(1).Padding(5).AlignCenter()
                    .Text(item.YearOfQualification.ToString());

                table.Cell().Border(1).Padding(5).Text(item.University);

                table.Cell().Border(1).Padding(5).AlignCenter()
                    .Text(FormatPeriod(item.UgFromDate, item.UgToDate));

                table.Cell().Border(1).Padding(5).AlignCenter()
                    .Text(FormatPeriod(item.PgFromDate, item.PgToDate));

                table.Cell().Border(1).Padding(5)
                    .Text(string.IsNullOrWhiteSpace(item.Responsibilities) ? "—" : item.Responsibilities);
            }
        });

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

    private void AddSkillsLabSection(ColumnDescriptor col)
    {
        var lab = _model.PhysicalFacilities.SkillsLab;
        if (lab == null)
            return;

        // ===== MAIN HEADING =====
        col.Item().PaddingTop(30).Column(col2 =>
        {
            col2.Item()
                .AlignCenter()
                .Text("Skills Laboratory")
                .FontSize(14)
                .Bold();

        });

        // ===== SUBSECTION 1: Intake & Area =====
        AddSubHeading(col, "Intake and Area Details", 125);

        col.Item().PaddingTop(8).Table(table =>
        {
            table.ColumnsDefinition(c =>
            {
                c.RelativeColumn(3);
                c.RelativeColumn(2);
            });

            AddTextRow(table, "Annual MBBS Intake", lab.AnnualMbbsIntake);
            AddTextRow(table, "Total Area Required (Sq.m)", lab.TotalAreaRequiredSqm);
            AddTextRow(table, "Total Area Available (Sq.m)", lab.TotalAreaAvailableSqm);
            AddTextRow(table, "Area Deficiency (Sq.m)", lab.TotalAreaDeficiencySqm);
            AddYesNoNullableRow(table, "Six weeks training before clinical posting",
                lab.SixWeeksTrainingCompletedBeforeClinical);
        });

        // ===== SUBSECTION 2: Examination & Infrastructure =====
        AddSubHeading(col, "Examination Rooms and Infrastructure", 200);

        col.Item().PaddingTop(8).Table(table =>
        {
            table.ColumnsDefinition(c =>
            {
                c.RelativeColumn(4);
                c.ConstantColumn(90);
            });

            AddTextRow(table, "Number of examination rooms", lab.NumberOfExaminationRooms);
            AddYesNoNullableRow(table, "Minimum four examination rooms available",
                lab.HasMinFourExamRooms);
            AddYesNoNullableRow(table, "Demonstration room for small groups",
                lab.HasDemoRoomSmallGroups);
            AddYesNoNullableRow(table, "Debrief / review area available",
                lab.HasDebriefArea);
            AddYesNoNullableRow(table, "Faculty coordinator room available",
                lab.HasFacultyCoordinatorRoom);
            AddYesNoNullableRow(table, "Support staff room available",
                lab.HasSupportStaffRoom);
            AddYesNoNullableRow(table, "Storage for mannequins/equipment available",
                lab.HasStorageForMannequins);
            AddYesNoNullableRow(table, "Video recording & review facility available",
                lab.HasVideoRecordingFacility);
        });

        // ===== SUBSECTION 3: Skill Stations & Equipment =====
        AddSubHeading(col, "Skill Stations and Equipment");

        col.Item().PaddingTop(8).Table(table =>
        {
            table.ColumnsDefinition(c =>
            {
                c.RelativeColumn(4);
                c.ConstantColumn(90);
            });

            AddTextRow(table, "Number of skill stations", lab.NumberOfSkillStations);
            AddYesNoNullableRow(table, "Group and individual stations available",
                lab.HasGroupAndIndividualStations);
            AddYesNoNullableRow(table, "Required trainers and mannequins as per CBME",
                lab.HasRequiredTrainersAndMannequins);
        });

        // ===== SUBSECTION 4: Staffing & IT Facilities =====
        AddSubHeading(col, "Staffing and IT Facilities", 120);

        col.Item().PaddingTop(8).Table(table =>
        {
            table.ColumnsDefinition(c =>
            {
                c.RelativeColumn(4);
                c.ConstantColumn(90);
            });

            AddYesNoNullableRow(table, "Dedicated technical officer available",
                lab.HasDedicatedTechnicalOfficer);
            AddYesNoNullableRow(table, "Adequate support staff available",
                lab.HasAdequateSupportStaff);
            AddYesNoNullableRow(table, "Teaching areas have AV facilities",
                lab.TeachingAreasHaveAV);
            AddYesNoNullableRow(table, "Teaching areas have Internet",
                lab.TeachingAreasHaveInternet);
            AddYesNoNullableRow(table, "Skills lab enabled for E-learning",
                lab.SkillsLabEnabledForELearning);
        });
    }


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
        var vm = _model.PhysicalFacilities.SkillsLabEquipment;
        if (vm == null || vm.Items == null || !vm.Items.Any())
            return;

        // ================= MAIN HEADING =================
        col.Item().PaddingTop(30).Column(col2 =>
        {
            col2.Item()
                .AlignCenter()
                .Text("Skills Lab Equipment")
                .FontSize(14)
                .Bold();

        });

        // ================= EQUIPMENT TABLE =================
        AddSubHeading(col, "Equipment List", 80);

        col.Item().PaddingTop(8).Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(4);   // Equipment Name
                columns.ConstantColumn(80);  // Required
                columns.ConstantColumn(80);  // Available
                columns.ConstantColumn(60);  // Quantity
            });

            // Header
            table.Header(header =>
            {
                header.Cell().Border(1).Padding(5).Text("Equipment Name").Bold();
                header.Cell().Border(1).Padding(5).AlignCenter().Text("Required").Bold();
                header.Cell().Border(1).Padding(5).AlignCenter().Text("Available").Bold();
                header.Cell().Border(1).Padding(5).AlignCenter().Text("Qty").Bold();
            });

            // Rows
            foreach (var item in vm.Items)
            {
                table.Cell().Border(1).Padding(5)
                    .Text(item.Name);

                table.Cell().Border(1).Padding(5)
                    .AlignCenter()
                    .Text(item.IsRequired ? "Yes" : "No");

                table.Cell().Border(1).Padding(5)
                    .AlignCenter()
                    .Text(item.IsAvailable ? "Yes" : "No");

                table.Cell().Border(1).Padding(5)
                    .AlignCenter()
                    .Text(item.Quantity?.ToString() ?? "—");
            }
        });

        // ================= ADDITIONAL FACILITIES =================
        if (vm.HasTrainingModulesForAllModels != null ||
            vm.UsesHybridModelsOrSimulations != null ||
            vm.HasComputerAssistedLearningSpace != null)
        {
            AddSubHeading(col, "Additional Facilities");

            col.Item().PaddingTop(8).Table(table =>
            {
                table.ColumnsDefinition(c =>
                {
                    c.RelativeColumn(4);
                    c.ConstantColumn(90);
                });

                AddYesNoNullableRow(table,
                    "Training modules available for all models",
                    vm.HasTrainingModulesForAllModels);

                AddYesNoNullableRow(table,
                    "Hybrid models or simulation-based training used",
                    vm.UsesHybridModelsOrSimulations);

                AddYesNoNullableRow(table,
                    "Computer-assisted learning space available",
                    vm.HasComputerAssistedLearningSpace);
            });
        }
    }

    private void AddDepartmentOfficesAndDeuSection(ColumnDescriptor col)
    {
        var model = _model?.DepartmentOfficesMeuVM;

        if (model == null)
            return;

        AddMainHeading(col, "Department Offices & Education Unit");

        // =========================================================
        // DEPARTMENT OFFICE REQUIREMENTS
        // =========================================================

        AddSubHeading(col, "Department Office Requirements", 168);

        col.Item().PaddingTop(5).Table(table =>
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
                    .Text("Requirement")
                    .Bold();

                header.Cell()
                    .Border(1)
                    .Padding(5)
                    .AlignCenter()
                    .Text("Status")
                    .Bold();
            });

            void AddStatusRow(string requirement, bool? status)
            {
                table.Cell()
                    .Border(1)
                    .Padding(5)
                    .Text(requirement);

                table.Cell()
                    .Border(1)
                    .Padding(5)
                    .AlignCenter()
                    .Text(status == true ? "Yes" : "No");
            }

            AddStatusRow(
                "HOD room with office and records",
                model.HasHodRoomWithOfficeAndRecords);

            AddStatusRow(
                "Rooms for faculty and residents",
                model.HasRoomsForFacultyAndResidents);

            AddStatusRow(
                "Faculty rooms have communication, computer and internet facilities",
                model.FacultyRoomsHaveCommunicationComputerInternet);

            AddStatusRow(
                "Rooms for non-teaching staff",
                model.HasRoomsForNonTeachingStaff);
        });


        // =========================================================
        // DENTAL EDUCATION UNIT
        // =========================================================

        if (model.Dental != null)
        {
            AddSubHeading(col, "Dental Education Unit", 120);

            col.Item().PaddingTop(5).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(3);
                    columns.RelativeColumn(1);
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
                    "Dental Education Unit Available",
                    model.Dental.HasDentalEducationUnit == true ? "Yes" : "No");

                AddRow(
                    "DEU Area (Sq.m)",
                    FormatValue(model.Dental.DentalEducationUnitAreaSqm));

                AddRow(
                    "Audio Visual Facility",
                    model.Dental.DentalEducationUnitHasAudioVisual == true
                        ? "Yes"
                        : "No");

                AddRow(
                    "Internet Facility",
                    model.Dental.DentalEducationUnitHasInternet == true
                        ? "Yes"
                        : "No");

                AddRow(
                    "Coordinator Name",
                    model.Dental.DeuCoordinatorName ?? "—");

                AddRow(
                    "Coordinator Designation / Department",
                    model.Dental.DeuCoordinatorDesignationDepartment ?? "—");

                AddRow(
                    "Coordinator Phone",
                    model.Dental.DeuCoordinatorPhone ?? "—");

                AddRow(
                    "Coordinator Email",
                    model.Dental.DeuCoordinatorEmail ?? "—");

                AddRow(
                    "Activities During Last Academic Year",
                    model.Dental.DeuActivitiesLastAcademicYear ?? "—");

                AddRow(
                    "Members List Uploaded",
                    model.Dental.HasDeuMembersListFile == true
                        ? "Yes"
                        : "No");
            });
        }


        // =========================================================
        // MEDICAL EDUCATION UNIT
        // =========================================================

        if (model.Medical != null)
        {
            AddSubHeading(col, "Medical Education Unit");

            col.Item().PaddingTop(5).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(3);
                    columns.RelativeColumn(1);
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
                    "Medical Education Unit Available",
                    model.Medical.HasMedicalEducationUnit == true ? "Yes" : "No");

                AddRow(
                    "MEU Area (Sq.m)",
                    FormatValue(model.Medical.MedicalEducationUnitAreaSqm));

                AddRow(
                    "Audio Visual Facility",
                    model.Medical.MedicalEducationUnitHasAudioVisual == true
                        ? "Yes"
                        : "No");

                AddRow(
                    "Internet Facility",
                    model.Medical.MedicalEducationUnitHasInternet == true
                        ? "Yes"
                        : "No");

                AddRow(
                    "Coordinator Name",
                    model.Medical.MeuCoordinatorName ?? "—");

                AddRow(
                    "Coordinator Designation / Department",
                    model.Medical.MeuCoordinatorDesignationDepartment ?? "—");

                AddRow(
                    "Coordinator Phone",
                    model.Medical.MeuCoordinatorPhone ?? "—");

                AddRow(
                    "Coordinator Email",
                    model.Medical.MeuCoordinatorEmail ?? "—");

                AddRow(
                    "Activities During Last Academic Year",
                    model.Medical.MeuActivitiesLastAcademicYear ?? "—");

                AddRow(
                    "Members List Uploaded",
                    model.Medical.HasMeuMembersListFile == true
                        ? "Yes"
                        : "No");
            });
        }
    }

    private void AddDepartmentOfficesAndMeuSection(ColumnDescriptor col)
    {
        var vm = _model.PhysicalFacilities.DeptOfficeMeu;
        if (vm == null)
            return;

        // ===== MAIN HEADING =====
        col.Item().PaddingTop(30).Column(col2 =>
        {
            col2.Item()
                .AlignCenter()
                .Text("Department Offices & Medical Education Unit (MEU)")
                .FontSize(14)
                .Bold();
        });

        // ============================================================
        // 1.8 Department Offices, Rooms for Staff
        // ============================================================
        AddSubHeading(col, "Department Offices and Staff Rooms", 185);

        col.Item().PaddingTop(8).Table(table =>
        {
            table.ColumnsDefinition(c =>
            {
                c.RelativeColumn(4);
                c.ConstantColumn(90);
            });

            AddYesNoNullableRow(table,
                "HOD room with office space and record maintenance available",
                vm.HasHodRoomWithOfficeAndRecords);

            AddYesNoNullableRow(table,
                "Rooms available for faculty and residents",
                vm.HasRoomsForFacultyAndResidents);

            AddYesNoNullableRow(table,
                "Faculty rooms have communication, computer and internet facilities",
                vm.FacultyRoomsHaveCommunicationComputerInternet);

            AddYesNoNullableRow(table,
                "Rooms available for non-teaching staff",
                vm.HasRoomsForNonTeachingStaff);
        });

        // ============================================================
        // 1.9 Medical Education Unit (MEU)
        // ============================================================
        AddSubHeading(col, "Medical Education Unit (MEU)", 158);

        col.Item().PaddingTop(8).Table(table =>
        {
            table.ColumnsDefinition(c =>
            {
                c.RelativeColumn(4);
                c.ConstantColumn(90);
            });

            AddYesNoNullableRow(table,
                "Medical Education Unit available",
                vm.HasMedicalEducationUnit);

            AddTextRow(table,
                "Medical Education Unit area (Sq.m)",
                vm.MedicalEducationUnitAreaSqm);

            AddYesNoNullableRow(table,
                "MEU equipped with Audio-Visual facilities",
                vm.MedicalEducationUnitHasAudioVisual);

            AddYesNoNullableRow(table,
                "MEU has Internet connectivity",
                vm.MedicalEducationUnitHasInternet);
        });

        // ============================================================
        // 3. MEU – Coordinator Details
        // ============================================================
        AddSubHeading(col, "MEU Coordinator Details", 135);

        col.Item().PaddingTop(8).Table(table =>
        {
            table.ColumnsDefinition(c =>
            {
                c.RelativeColumn(3);
                c.RelativeColumn(4);
            });

            AddTextRow(table, "Coordinator Name", vm.MeuCoordinatorName);
            AddTextRow(table, "Designation / Department",
                vm.MeuCoordinatorDesignationDepartment);
            AddTextRow(table, "Phone Number", vm.MeuCoordinatorPhone);
            AddTextRow(table, "Email Address", vm.MeuCoordinatorEmail);
        });

        // ============================================================
        // MEU Members & Activities
        // ============================================================
        AddSubHeading(col, "MEU Members and Activities", 155);

        col.Item().PaddingTop(8).Table(table =>
        {
            table.ColumnsDefinition(c =>
            {
                c.RelativeColumn(3);
                c.RelativeColumn(4);
            });

            AddTextRow(table,
                "MEU Members List (Description)",
                vm.MeuMembersListDescription);

            AddTextRow(table,
                "MEU Activities during last academic year",
                vm.MeuActivitiesLastAcademicYear);

            AddTextRow(table,
                "Members List Document Uploaded",
                vm.HasMeuMembersListFile ? "Yes" : "No");
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

    private void AddMuseumsSection(ColumnDescriptor col)
    {
        var vm = _model.PhysicalFacilities.SmallGroupMuseums;
        if (vm == null)
            return;

        // ================= MAIN HEADING =================
        col.Item().PaddingTop(30).Column(c =>
        {
            c.Item()
                .AlignCenter()
                .Text("Museums")
                .FontSize(14)
                .Bold();
        });

        // ================= MUSEUM AVAILABILITY =================
        AddSubHeading(col, "Museum Availability", 105);

        col.Item().PaddingTop(8).Table(table =>
        {
            table.ColumnsDefinition(c =>
            {
                c.RelativeColumn(4);
                c.ConstantColumn(120);
            });

            AddYesNoNullableRow(table, "Separate Anatomy Museum available", vm.SeparateAnatomyMuseumAvailable);
            AddYesNoNullableRow(table, "Pathology & Forensic Medicine shared museum", vm.PathologyForensicSharedMuseum);
            AddYesNoNullableRow(table, "Pharmacology, Microbiology & Community Medicine shared museum", vm.PharmMicroCommSharedMuseum);
            AddYesNoNullableRow(table, "Teaching time-sharing programmed", vm.TeachingTimeSharingProgrammed);
        });

        // ================= SEATING & AREA =================
        AddSubHeading(col, "Seating Capacity & Area", 130);

        col.Item().PaddingTop(8).Table(table =>
        {
            table.ColumnsDefinition(c =>
            {
                c.RelativeColumn(4);
                c.ConstantColumn(120);
            });

            AddTextRow(table, "Seating capacity per museum", vm.SeatingCapacityPerMuseum);
            AddTextRow(table, "Seating area available (Sq. m)", vm.SeatingAreaAvailableSqm);
            AddTextRow(table, "Seating area required (Sq. m)", vm.SeatingAreaRequiredSqm);
            AddTextRow(table, "Seating area deficiency (Sq. m)", vm.SeatingAreaDeficiencySqm);
        });

        // ================= FACILITIES =================
        AddSubHeading(col, "Museum Facilities", 95);

        col.Item().PaddingTop(8).Table(table =>
        {
            table.ColumnsDefinition(c =>
            {
                c.RelativeColumn(4);
                c.ConstantColumn(120);
            });

            AddYesNoNullableRow(table, "Audio-visual facilities available", vm.MuseumsHaveAV);
            AddYesNoNullableRow(table, "Internet facility available", vm.MuseumsHaveInternet);
            AddYesNoNullableRow(table, "Digitally linked museums", vm.MuseumsDigitallyLinked);
            AddYesNoNullableRow(table, "Adequate racks and shelves available", vm.MuseumsHaveRacksShelves);
            AddYesNoNullableRow(table, "Radiology display facilities available", vm.MuseumsHaveRadiologyDisplay);
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
            AddLibraryStaffPdfSection( col,library.LibraryStaff);
        }

        // =========================================================
        // DEPARTMENT LIBRARIES
        // =========================================================

        if (library.DepartmentLibraries?.Any() == true)
        {
            AddDepartmentLibrariesPdfSection( col, library.DepartmentLibraries);
        }

        // =========================================================
        // OTHER DETAILS
        // =========================================================

        if (library.OtherDetails != null)
        {
            AddLibraryOtherDetailsPdfSection( col, library.OtherDetails);
        }

        // =========================================================
        // DENTAL LIBRARY RECORDS
        // =========================================================

        if (library.facultyCode == 2 &&
            library.DentalLibraryRecords?.Any() == true)
        {
            AddDentalLibraryRecordsPdfSection( col, library.DentalLibraryRecords);
        }

        // =========================================================
        // RESEARCH PUBLICATIONS
        // =========================================================

        if (library.ResearchPublications != null)
        {
            AddResearchPublicationsPdfSection( col, library.ResearchPublications);
        }

        // =========================================================
        // LIBRARY INFORMATION
        // =========================================================

        if (library.LibraryInformation != null)
        {
            AddLibraryInformationPdfSection( col, library.LibraryInformation);
        }
    }

    private void AddLibraryInformationPdfSection(  ColumnDescriptor col, LibraryInformationPreviewVM model)
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
            AddSubHeading(col, "Library Holdings", 90);

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

        AddSubHeading(col, "Library Building", 90);

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
            AddSubHeading(col, "Technical Process" ,90);

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
            AddSubHeading(col, "Library Equipments", 90);

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

        AddSubHeading(col, "Library Finance", 90);

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

    private void AddResearchPublicationsPdfSection( ColumnDescriptor col, ResearchPublicationsPreviewVM model)
    {
        if (model == null)
            return;

        // =========================================================
        // RESEARCH PUBLICATIONS
        // =========================================================

        AddSubHeading(col, "Research Publications", 100);

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

        AddSubHeading(col, "Faculty Research Projects", 120);

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
            AddSubHeading(col, "Committees", 80);

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
            AddSubHeading(col, "Department-wise Publications", 160);

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


    private void AddDentalLibraryRecordsPdfSection( ColumnDescriptor col, List<DentalLibraryRecordPreviewVM> records)
    {
        AddSubHeading(col, "Dental Library Records", 120);

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


    private void AddLibraryStaffPdfSection( ColumnDescriptor col,  List<LibraryStaffPreviewVM> staff)
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


    private void AddDepartmentLibrariesPdfSection( ColumnDescriptor col,  List<DepartmentLibraryPreviewVM> departments)
    {
        AddSubHeading(col, "Department Libraries", 120);

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

    private void AddLibraryOtherDetailsSection(ColumnDescriptor col)
    {
        var library = _model.LibraryDisplay;
        var details = library?.caAffMedicalLibraryvm?.OtherDetails;

        if (details == null)
            return;

        // ================= SUB HEADING =================
        AddSubHeading(col, "Other Library Details", 120);

        // ================= TABLE =================
        col.Item().PaddingTop(8).Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(3);    // Label
                columns.RelativeColumn(2);    // Value
            });

            // Digital Valuation Centre
            AddTextRow(table,
                "Digital Valuation Centre Available",
                details.HasDigitalValuationCentre ?? "—");

            AddTextRow(table,
                "Number of Systems",
                details.NoOfSystems);

            AddTextRow(table,
                "Stable Internet Facility",
                details.HasStableInternet ?? "—");

            AddTextRow(table,
                "CCTV / Surveillance System",
                details.HasCccameraSystem ?? "—");

            // ---------- Special Features ----------
            AddTextRow(table,
                "Special Features / Achievements",
                details.SpecialFeaturesQuestion ?? "—");

            AddTextRow(table,
                "Supporting Document",
                details.HasSpecialFeaturesPdf == true ? "Available" : "—");
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

            // Authority
            AddTextRow(table, "Authority Name & Address", acc.AuthorityNameAddress);
            AddTextRow(table, "Authority Contact", acc.AuthorityContact);

            // Annual Accounts
            AddTextRow(table, "Recurrent Annual (₹)", acc.RecurrentAnnual.ToString("0.##"));
            AddTextRow(table, "Non-Recurrent Annual (₹)", acc.NonRecurrentAnnual.ToString("0.##"));
            AddTextRow(table, "Deposits (₹)", acc.Deposits.ToString("0.##"));

            // Fees
            AddTextRow(table, "Tuition Fee (₹)", acc.TuitionFee.ToString("0.##"));
            AddTextRow(table, "Sports Fee (₹)", acc.SportsFee.ToString("0.##"));
            AddTextRow(table, "Union Fee (₹)", acc.UnionFee.ToString("0.##"));
            AddTextRow(table, "Library Fee (₹)", acc.LibraryFee.ToString("0.##"));
            AddTextRow(table, "Other Fee (₹)", acc.OtherFee.ToString("0.##"));
            AddTextRow(table, "Total Fee (₹)", acc.TotalFee.ToString("0.##"));

            // Accounts
            AddTextRow(table, "Account Books Maintained", acc.AccountBooksMaintained);
            AddTextRow(table, "Audited Statement", acc.HasAuditedStatementPdf ? "Available" : "—");
            AddTextRow(table, "Account Summary", acc.HasAccountSummaryPdf ? "Available" : "—");
            AddTextRow(table, "Governing Council Approval", acc.HasGoverningCouncilPdf ? "Available" : "—");
        });
    }

    private void AddFinanceStaffParticularsSection(ColumnDescriptor col)
    {
        var staffList = _model.FinanceVm?.staffParticularsVM?.StaffParticulars;

        if (staffList == null || !staffList.Any())
            return;

        AddSubHeading(col, "Staff Pay Particulars", 105);

        col.Item().PaddingTop(8).Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(4); // Designation
                columns.RelativeColumn(2); // Pay Scale
            });

            table.Header(header =>
            {
                header.Cell().Border(1).Padding(5).Text("Designation").Bold();
                header.Cell().Border(1).Padding(5).AlignCenter().Text("Pay Scale (₹)").Bold();
            });

            foreach (var staff in staffList)
            {
                table.Cell().Border(1).Padding(5)
                    .Text(staff.DesignationName);

                table.Cell().Border(1).Padding(5).AlignCenter()
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

            AddYesNoNullableRow(table, "Teachers Updated in EMS", other.TeachersUpdatedInEms);
            AddYesNoNullableRow(table, "Examiner Details Attached", other.ExaminerDetailsAttached);

            AddTextRow(table, "Examiner Details Document",
                other.HasExaminerDetailsPdf ? "Available" : "—");

            AddTextRow(table, "AEBAS (Last 3 Months)",
                other.HasAebasLastThreeMonthsPdf ? "Available" : "—");

            AddTextRow(table, "AEBAS (Inspection Day)",
                other.HasAebasInspectionDayPdf ? "Available" : "—");

            AddYesNoNullableRow(table, "Service Register Maintained",
                other.ServiceRegisterMaintained);

            AddYesNoNullableRow(table, "Acquittance Register Maintained",
                other.AcquittanceRegisterMaintained);

            AddTextRow(table, "Provident Fund Records",
                other.HasProvidentFundPdf ? "Available" : "—");

            AddTextRow(table, "ESI Records",
                other.HasEsipdf ? "Available" : "—");
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

        AddSubHeading(col, "Hostel Details", 80);

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
                hostel.MenHostelAreaSqFt.ToString());

            AddRow(
                "Women Hostel Area (Sq.ft)",
                hostel.WomenHostelAreaSqFt.ToString());

            AddRow(
                "Possession Proof",
                !string.IsNullOrWhiteSpace(hostel.PossessionProofPath)
                    ? "Uploaded"
                    : "Not Uploaded");
        });


        // =========================================================
        // COMMON ROOMS / OTHER FACILITIES
        // =========================================================

        AddSubHeading(col, "Common Rooms & Other Details");

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

        AddSubHeading(col, "Hostel Facilities" , 80);

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
        var facultyList = _model?.FacultyDesigNonTeachDisplayVM?.FacultyDetailDisplayVM;

        if (facultyList == null || !facultyList.Any())
            return;
        AddMainHeading(col, "Faculty, Designation, Non Teaching");
        AddSubHeading(col, "Faculty Details", 80);

        col.Item().PaddingTop(8).Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(3); // Name
                columns.RelativeColumn(2); // Designation
                columns.RelativeColumn(2); // Subject
                columns.ConstantColumn(60); // PG
                columns.ConstantColumn(60); // PhD
                columns.ConstantColumn(70); // Litigation
                columns.ConstantColumn(70); // Docs
            });

            // Header
            table.Header(header =>
            {
                header.Cell().Border(1).Padding(3).Text("Name").Bold();
                header.Cell().Border(1).Padding(3).Text("Designation").Bold();
                header.Cell().Border(1).Padding(3).Text("Subject").Bold();
                header.Cell().Border(1).Padding(3).AlignCenter().Text("PG").Bold();
                header.Cell().Border(1).Padding(3).AlignCenter().Text("PhD").Bold();
                header.Cell().Border(1).Padding(3).AlignCenter().Text("Litigation").Bold();
                header.Cell().Border(1).Padding(3).AlignCenter().Text("Docs").Bold();
            });

            // Body
            foreach (var f in facultyList)
            {
                table.Cell().Border(1).Padding(3).Text(f.NameOfFaculty);
                table.Cell().Border(1).Padding(3).Text(f.Designation);
                table.Cell().Border(1).Padding(3).Text(f.Subject ?? "—");
                table.Cell().Border(1).Padding(3).AlignCenter().Text(f.RecognizedPgTeacher ?? "—");
                table.Cell().Border(1).Padding(3).AlignCenter().Text(f.RecognizedPhDteacher ?? "—");
                table.Cell().Border(1).Padding(3).AlignCenter().Text(f.LitigationPending ?? "—");
                table.Cell().Border(1).Padding(3).AlignCenter()
                    .Text(
                        (f.HasGuideRecognitionDoc || f.HasPhDRecognitionDoc || f.HasLitigationDoc)
                        ? "Available"
                        : "—"
                    );
            }
        });
    }

    private void AddCollegeDesignationSection(ColumnDescriptor col)
    {
        var groups = _model.FacultyDesigNonTeachDisplayVM?.CollegeDesignationDisplayVM;

        if (groups == null || !groups.Any())
            return;

        AddSubHeading(col, "Designation & Intake Details", 120);

        col.Item().PaddingTop(8).Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(3);   // Department
                columns.RelativeColumn(3);   // Designation
                columns.ConstantColumn(80);  // Required
                columns.ConstantColumn(80);  // Available
                columns.ConstantColumn(70);  // Seat Slab
            });

            // ---- Header ----
            table.Header(header =>
            {
                header.Cell().Border(1).Padding(4).Text("Department").Bold();
                header.Cell().Border(1).Padding(4).Text("Designation").Bold();
                header.Cell().Border(1).Padding(4).AlignCenter().Text("Required").Bold();
                header.Cell().Border(1).Padding(4).AlignCenter().Text("Available").Bold();
                header.Cell().Border(1).Padding(4).AlignCenter().Text("Seat Slab").Bold();
            });

            // ---- Body ----
            foreach (var group in groups)
            {
                bool isFirstRow = true;

                foreach (var item in group.Designations)
                {
                    // Department (print only once)
                    table.Cell().Border(1).Padding(4)
                        .Text(isFirstRow ? group.Department ?? "—" : string.Empty);

                    table.Cell().Border(1).Padding(4)
                        .Text(item.Designation);

                    table.Cell().Border(1).Padding(4)
                        .AlignCenter()
                        .Text(item.RequiredIntake);

                    table.Cell().Border(1).Padding(4)
                        .AlignCenter()
                        .Text(item.AvailableIntake);

                    table.Cell().Border(1).Padding(4)
                        .AlignCenter()
                        .Text(item.SeatSlab.ToString() ?? "—");

                    isFirstRow = false;
                }
            }
        });
    }

    private void AddPaymentSection(ColumnDescriptor col)
    {
        var payment = _model?.PaymentVM;

        if (payment == null || payment.Id <= 0)
            return;

        // ===== MAIN HEADING =====
        col.Item().PaddingTop(25)
            .AlignCenter()
            .Text("Payment Details")
            .FontSize(14)
            .Bold();

        // ===== TABLE =====
        col.Item().PaddingTop(10).Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(3); // Label
                columns.RelativeColumn(4); // Value
            });

            AddTextRow(table, "Amount Paid", payment.Amount);
            AddTextRow(table,
                "Payment Date",
                payment.PaymentDate != default
                    ? payment.PaymentDate.ToString("dd MMM yyyy")
                    : "—");
            AddTextRow(table, "Transaction Reference", payment.TransactionReferenceNo ?? "—");

            AddTextRow(table,
                "Supporting Document",
                payment.HasDocument ? "Available" : "—");
        });
    }

    private void AddNonTeachingStaffSection(ColumnDescriptor col)
    {
        var staffList = _model.FacultyDesigNonTeachDisplayVM?
                            .NonTeachingStaffSectionVM?
                            .Staffs;

        if (staffList == null || !staffList.Any())
            return;

        AddSubHeading(col, "Non-Teaching Staff Details", 120);

        col.Item().PaddingTop(8).Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(3);   // Staff Name
                columns.RelativeColumn(3);   // Designation
                columns.ConstantColumn(80);  // PF
                columns.ConstantColumn(80);  // ESI
                columns.ConstantColumn(100); // Service Register
                columns.ConstantColumn(120); // Salary Register
            });

            // -------- Header --------
            table.Header(header =>
            {
                header.Cell().Border(1).Padding(4).Text("Staff Name").Bold();
                header.Cell().Border(1).Padding(4).Text("Designation").Bold();
                header.Cell().Border(1).Padding(4).AlignCenter().Text("PF").Bold();
                header.Cell().Border(1).Padding(4).AlignCenter().Text("ESI").Bold();
                header.Cell().Border(1).Padding(4).AlignCenter().Text("Service Register").Bold();
                header.Cell().Border(1).Padding(4).AlignCenter().Text("Salary Register").Bold();
            });

            // -------- Body --------
            foreach (var staff in staffList)
            {
                table.Cell().Border(1).Padding(4)
                    .Text(staff.StaffName);

                table.Cell().Border(1).Padding(4)
                    .Text(staff.Designation);

                table.Cell().Border(1).Padding(4).AlignCenter()
                    .Text(staff.PfProvided ? "Yes" : "No");

                table.Cell().Border(1).Padding(4).AlignCenter()
                    .Text(staff.EsiProvided ? "Yes" : "No");

                table.Cell().Border(1).Padding(4).AlignCenter()
                    .Text(staff.ServiceRegisterMaintained ? "Yes" : "No");

                table.Cell().Border(1).Padding(4).AlignCenter()
                    .Text(staff.SalaryAcquaintanceRegister ? "Yes" : "No");
            }
        });
    }

    private static void AddLabRow(TableDescriptor table, string label, bool available, bool shared)
    {
        table.Cell().Border(1).Padding(5).Text(label);
        table.Cell().Border(1).Padding(5).AlignCenter().Text(available ? "Yes" : "No");
        table.Cell().Border(1).Padding(5).AlignCenter().Text(shared ? "Yes" : "No");
    }

    private void AddMainHeading(ColumnDescriptor col, string title)
    {
        col.Item().PaddingTop(20).Column(c =>
        {
            c.Item().Text(title)
                .FontSize(14)
                .AlignCenter()
                .Bold();

        });
    }

    private void AddSubHeading(ColumnDescriptor col, string title, int lineLength = 150)
    {
        col.Item().PaddingTop(15).Column(c =>
        {
            c.Item().Text(title)
                .FontSize(12)
                .Bold();

            c.Item().PaddingTop(2).Row(row =>
            {
                row.ConstantItem(lineLength)
                    .LineHorizontal(1)
                    .LineColor(Colors.Black);

                row.RelativeItem();
            });

        });
    }
    private static void AddTextRow(TableDescriptor table, string label, object value)
    {
        table.Cell().Border(1).Padding(5).Text(label);
        table.Cell().Border(1).Padding(5).AlignCenter()
            .Text(value?.ToString() ?? "—");
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
