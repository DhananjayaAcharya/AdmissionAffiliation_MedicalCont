//using DocumentFormat.OpenXml.Spreadsheet;
//using DocumentFormat.OpenXml.Spreadsheet; // (if you decided to keep this)
using Medical_Affiliation.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SkiaSharp;
public class PreviewReportPdf : IDocument
{
    private readonly CApreviewViewModel _model;
    private readonly byte[] _logo;
    private readonly byte[] _collegeLogoBytes;
    private readonly PdfTheme _theme;
    private int _sectionNumber;
    private byte[]? _watermarkLogoBytes;

    public PreviewReportPdf(CApreviewViewModel model, byte[] logo, byte[] clgLogoBytes)
    {
        _model = model;
        _logo = logo;
        _collegeLogoBytes = clgLogoBytes;
        _theme = PdfTheme.ForApplicationType(model.ApplicationType);
    }

    private byte[] GetWatermarkLogo()
    {
        if (_watermarkLogoBytes != null)
            return _watermarkLogoBytes;

        using var original = SKBitmap.Decode(_collegeLogoBytes);
        using var surface = SKSurface.Create(new SKImageInfo(original.Width, original.Height, SKColorType.Rgba8888, SKAlphaType.Premul));
        using var paint = new SKPaint { Color = SKColors.White.WithAlpha(32) };

        surface.Canvas.Clear(SKColors.Transparent);
        surface.Canvas.DrawBitmap(original, 0, 0, paint);

        using var image = surface.Snapshot();
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        _watermarkLogoBytes = data.ToArray();
        return _watermarkLogoBytes;
    }


    public void Compose(IDocumentContainer container)
    {
        _sectionNumber = 0;

        container.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(22);
            page.DefaultTextStyle(text => text.FontFamily("Arial").FontSize(9).FontColor("#243B53"));

            // Watermark lives in the page.Background() slot, which QuestPDF always
            // renders at full page size behind the content on EVERY page,
            // regardless of how much content is on that page. This avoids the
            // clipping / "half showing" bug that happened when the watermark
            // was inside page.Content().Layers(), whose layer box height was
            // tied to the remaining content height on each page.
            page.Background()
                .Background(Colors.White)
                .Extend()
                .Element(background => background
                    .AlignCenter()
                    .AlignMiddle()
                    .Width(360)
                    .Height(360)
                    .Image(GetWatermarkLogo())
                    .FitArea());

            page.Content().Border(1.2f).BorderColor(_theme.Primary).Padding(10).Column(col =>
            {
                // --- REPORT HEADER ---
                AddReportHeader(col);

                AddPreviewMetadataHeader(col);

                var courseLevel = (_model.ApplyingCourseLevel ?? string.Empty).Trim().ToUpperInvariant();
                var isUg = courseLevel == "UG";
                var isPg = courseLevel == "PG";
                var isSs = courseLevel == "SS";

                // Basic Details
                AddInstitutionDetailsSection(col);
                AddAffiliatedInstituteDetailsSection(col);
                AddDeanOrDirectorSection(col);
                AddPrincipalSection(col);

                // Intake Details
                if (isUg)
                {
                    AddAffiliationCourseSection(col);
                }
                AddCourseIntakeListSection(col);
                if (isPg)
                {
                    AddAffiliatedCoursesSection(col);
                }

                // Physical Infrastructure
                AddHospitalAffiliationSection(col);
                AddDepartmentSections(col);
                if (isUg)
                {
                    AddBedDistributionSection(col);
                }
                AddSmallGroupTeachingSection(col);
                AddStudentPracticalLabsSection(col);
                AddMuseumsSection(col);
                AddSkillsLabSection(col);
                AddDepartmentOfficesAndMeuSection(col);
                AddHostelDetailsSection(col);
                AddHostelFacilitiesSection(col);
                AddVehicleDetailsSection(col);

                // Academic & Admin
                if (isUg)
                {
                    AddAcademicMattersSection(col);
                }
                AddFinanceStaffParticularsSection(col);
                AddFinanceOtherStaffDetailsSection(col);
                AddAdminTeachingBlockSection(col);

                // Human Resource
                AddFacultyDetailsSection(col);
                AddCollegeDesignationSection(col);
                AddNonTeachingStaffSection(col);

                // Payment
                AddPaymentSection(col);
            });

            page.Footer().PaddingTop(8).BorderTop(1).BorderColor(_theme.Border).Row(row =>
            {
                row.RelativeItem().AlignLeft().Text(text =>
                {
                    text.Span("RGUHS | Confidential affiliation preview | ");
                    text.Span(DateTime.Now.ToString("dd MMM yyyy, HH:mm"));
                });

                row.RelativeItem().AlignRight().Text(text =>
                {
                    text.Span("Page ").Bold();
                    text.CurrentPageNumber();
                    text.Span(" of ");
                    text.TotalPages();
                });
            });
        });

        container.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(42);
            page.DefaultTextStyle(text => text.FontFamily("Arial").FontSize(11).FontColor("#243B53"));

            page.Content()
                .Border(1.2f)
                .BorderColor(_theme.Primary)
                .Padding(28)
                .Column(AddFinalDeclarationPage);

            page.Footer().PaddingTop(8).BorderTop(1).BorderColor(_theme.Border).Row(row =>
            {
                row.RelativeItem().AlignLeft().Text("RGUHS | Final declaration");
                row.RelativeItem().AlignRight().Text(text =>
                {
                    text.Span("Page ").Bold();
                    text.CurrentPageNumber();
                    text.Span(" of ");
                    text.TotalPages();
                });
            });
        });
    }

    private void AddFinalDeclarationPage(ColumnDescriptor col)
    {
        var principalName = _model.DeclarationVM?.PrincipalName;
        if (string.IsNullOrWhiteSpace(principalName))
            principalName = _model.InstitutionDetails?.Principal_Name;

        var collegeName = _model.CollegeName;
        if (string.IsNullOrWhiteSpace(collegeName))
            collegeName = _model.InstitutionDetails?.NameOfInstitution;

        col.Item().AlignCenter().Text("FINAL DECLARATION")
            .FontSize(17).Bold().FontColor(_theme.Primary);

        col.Item().PaddingTop(8).AlignCenter().Text("To be signed and sealed by the Principal")
            .FontSize(10).Italic().FontColor("#64748B");

        col.Item().PaddingTop(34).Text(text =>
        {
            text.Span("I, ").Bold();
            text.Span(string.IsNullOrWhiteSpace(principalName) ? "{Principal name}" : principalName).Bold();
            text.Span(", Principal of ");
            text.Span(string.IsNullOrWhiteSpace(collegeName) ? "{collegename}" : collegeName).Bold();
            text.Span(", hereby solemnly declare and undertake that all documents, particulars, statements, declarations and information furnished or uploaded by the College in this application are true, complete, accurate and correct to the best of my knowledge and belief.");
        });

        col.Item().PaddingTop(18).Text("I further undertake that no material information has been concealed, suppressed or misrepresented in this application. I understand that any false, misleading, fabricated, incomplete or incorrect information or documentation furnished by the College may result in rejection or cancellation of the application, withdrawal of affiliation/approval, or such other action as may be taken by Rajiv Gandhi University of Health Sciences, Karnataka (RGUHS) in accordance with the applicable rules, regulations, ordinances and notifications.");

        col.Item().PaddingTop(58).Row(row =>
        {
            row.RelativeItem().Column(signature =>
            {
                signature.Item().LineHorizontal(1).LineColor(_theme.Primary);
                signature.Item().PaddingTop(6).AlignCenter().Text("Principal Signature").Bold();
                signature.Item().PaddingTop(4).AlignCenter().Text(string.IsNullOrWhiteSpace(principalName) ? "" : principalName).FontSize(10);
            });

            row.ConstantItem(42);

            row.ConstantItem(145).Height(92).Border(1).BorderColor(_theme.Border).AlignCenter().AlignMiddle().Text("OFFICIAL SEAL").Bold().FontColor("#64748B");
        });

        col.Item().PaddingTop(34).Text(text =>
        {
            text.Span("Date: ").Bold();
            text.Span("____________________________");
        });
    }

    private void AddReportHeader(ColumnDescriptor col)
    {
        col.Item().Background(_theme.Primary).Padding(14).Row(row =>
        {
            row.ConstantItem(72).AlignMiddle().Width(72).Height(72)
                .Image(_collegeLogoBytes);

            row.ConstantItem(18);

            row.RelativeItem().AlignMiddle().Column(c =>
            {
                c.Item().AlignCenter().Text("RAJIV GANDHI UNIVERSITY OF HEALTH SCIENCES")
                    .FontSize(16).Bold().FontColor(Colors.White);

                c.Item().PaddingTop(5).AlignCenter().Text("Jayanagar, Bengaluru, Karnataka 560041")
                    .FontSize(9).Bold().FontColor(Colors.White);
            });
        });

        col.Item().PaddingTop(4).LineHorizontal(3).LineColor(_theme.Accent);
    }

    private void AddPreviewMetadataHeader(ColumnDescriptor col)
    {
        var totalSeats = _model.PaymentCalculation?.MatchedCourses
            .FirstOrDefault()?.TotalSeats;

        col.Item().PaddingTop(4).PaddingBottom(9).Column(section =>
        {
            section.Item().PaddingTop(10).AlignCenter().Text(_model.CollegeName ?? "College Affiliation Application")
                .FontSize(14).Bold().FontColor("#123A63");

            var affiliationType = string.IsNullOrWhiteSpace(_model.ApplicationType)
                ? "Affiliation"
                : _model.ApplicationType.Trim();

            section.Item().PaddingTop(4).AlignCenter().Text(text =>
            {
                text.Span("TYPE OF AFFILIATION: ")
                    .FontSize(8).Bold().FontColor("#60758A");
                text.Span(affiliationType)
                    .FontSize(10).Bold().FontColor(_theme.Secondary);
            });

            section.Item().PaddingTop(9).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                });

                AddMetadataCell(table, "Application Type", _model.ApplicationType);
                AddMetadataCell(table, "Applying Course Level", _model.ApplyingCourseLevel);
                AddMetadataCell(table, "Total Seats", totalSeats?.ToString() ?? "—");
            });
        });
    }

    private void AddMetadataCell(TableDescriptor table, string label, string value)
    {
        table.Cell().Border(1).BorderColor(_theme.Border).Background(_theme.Light).Padding(7).Column(cell =>
        {
            cell.Item().Text(label).FontSize(7).Bold().FontColor("#60758A");
            cell.Item().PaddingTop(2).Text(value ?? "—").FontSize(9).Bold().FontColor(_theme.Primary);
        });
    }

    private void AddInstitutionDetailsSection(ColumnDescriptor col)
    {
        var institution = _model?.InstitutionDetails;
        if (institution == null) return;

        AddMainHeading(col, "01 · Institution Details");
        col.Item().PaddingTop(8).Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(3);
                columns.RelativeColumn(4);
            });

            AddTextRow(table, "Institution Type", institution.TypeOfInstitution);
            AddTextRow(table, "Institution Name", institution.NameOfInstitution);
            AddTextRow(table, "Address", institution.Address);
            AddTextRow(table, "Village / Town / City", institution.VillageTownCity);
            AddTextRow(table, "Taluk", institution.Taluk);
            AddTextRow(table, "District", institution.District);
            AddTextRow(table, "PIN Code", institution.PinCode);
            AddTextRow(table, "Mobile Number", institution.MobileNumber);
            AddTextRow(table, "Website", institution.Website);
            AddTextRow(table, "Minority Institution", institution.MinorityInstitute ? "Yes" : "No");
            AddTextRow(table, "Year of Establishment", institution.YearOfEstablishment);
            AddTextRow(table, "Email", institution.EmailId);
            AddTextRow(table, "Alternate Contact", institution.AltLandlineMobile);
            AddTextRow(table, "Alternate Email", institution.AltEmailId);
            AddTextRow(table, "College Status", institution.StatusOfCollege);
            AddTextRow(table, "Nodal Officer", institution.NodalOfficer_Name);
            AddTextRow(table, "Nodal Officer Mobile", institution.NodalOfficer_Mob_Number);
            AddTextRow(table, "Nodal Officer Email", institution.NodalOfficer_Email);
            AddTextRow(table, "Principal", institution.Principal_Name);
            AddTextRow(table, "Trust Name", institution.TrustName);
            AddTextRow(table, "Trust Address", institution.TrustAddress);
            AddTextRow(table, "Trust Establishment Date", institution.TrustEstablishmentDate);
            AddTextRow(table, "Trust President", institution.TrustPresidentName);
            AddTextRow(table, "Trust President Contact", institution.TrustPresidentContactNo);
            AddTextRow(table, "Dean", institution.DeanName);
            AddTextRow(table, "Running Course", institution.RunningCourse);
            AddTextRow(table, "Minority Category", institution.MinorityCategory);
            AddTextRow(table, "Autonomous Certificate Number", institution.GovAutonomousCertNumber);
        });
    }

    private void AddAffiliatedInstituteDetailsSection(ColumnDescriptor col)
    {
        var institute = _model?.AffInstituteDetails;
        if (institute == null) return;

        AddMainHeading(col, "02 · Trust Institution Details");

        AddInstituteTabSection(col, "Trust / Society Details", table =>
        {
            AddTextRow(table, "Trust Name", institute.TrustName);
            AddTextRow(table, "TRUST PAN Number", institute.PANNumber);
            AddTextRow(table, "Registration Number", institute.RegistrationNumber);
            AddTextRow(table, "Registration Date", institute.RegistrationDate);
            AddTextRow(table, "President Name", institute.PresidentName);
            AddTextRow(table, "Category of Organisation", institute.CategoryOfOrganisation);
            AddTextRow(table, "Address", institute.Address);
            AddTextRow(table, "Mobile Number", institute.MobileNumber);
            AddTextRow(table, "Email ID", institute.EmailId);
        });

        AddInstituteTabSection(col, "Trust Contact Person", table =>
        {
            AddTextRow(table, "Full Name", institute.ContactPersonName);
            AddTextRow(table, "Designation", institute.ContactPersonRelation);
            AddTextRow(table, "Mobile", institute.ContactPersonMobile);
        });

        AddInstituteTabSection(col, "Upload Documents", table =>
        {
            AddTextRow(table, "Trust PAN File", institute.hasPANFile ? "Uploaded" : "Not Uploaded");
            AddTextRow(table, "Bank Statement", institute.hasBankStatementFile ? "Uploaded" : "Not Uploaded");
            AddTextRow(table, "Registration Certificate", institute.hasRegistrationCertificateFile ? "Uploaded" : "Not Uploaded");
            AddTextRow(table, "Audit Statement", institute.hasAuditStatementFile ? "Uploaded" : "Not Uploaded");
        });
    }

    private void AddInstituteTabSection(ColumnDescriptor col, string title, Action<TableDescriptor> rows)
    {
        AddSubHeading(col, title);
        col.Item().PaddingTop(6).Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(3);
                columns.RelativeColumn(4);
            });

            rows(table);
        });
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

    private void AddCourseIntakeListSection(ColumnDescriptor col)
    {
        var rows = _model.CourseIntakeList;
        if (rows == null || rows.Count == 0) return;

        AddMainHeading(col, "07 · Course Intake List");
        AddSubHeading(col, "Course Intake List", 135);
        col.Item().PaddingTop(8).Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(4);
                columns.RelativeColumn(1.5f);
                columns.RelativeColumn(1.5f);
                columns.RelativeColumn(1.5f);
                columns.RelativeColumn(3);
            });

            table.Header(header =>
            {
                header.Cell().Border(1).Padding(3).Text("Course").Bold();
                header.Cell().Border(1).Padding(3).Text("Level").Bold();
                header.Cell().Border(1).Padding(3).AlignCenter().Text("Intake 26-27").Bold();
                header.Cell().Border(1).Padding(3).Text("Course Code").Bold();
                header.Cell().Border(1).Padding(3).Text("Match Note").Bold();
            });

            foreach (var item in rows)
            {
                table.Cell().Border(1).Padding(3).Text(item.CourseName);
                table.Cell().Border(1).Padding(3).Text(item.CourseLevel);
                table.Cell().Border(1).Padding(3).AlignCenter().Text(item.Intake?.ToString() ?? "—");
                table.Cell().Border(1).Padding(3).Text(item.CourseCode?.ToString() ?? "—");
                table.Cell().Border(1).Padding(3).Text(string.IsNullOrWhiteSpace(item.MatchNote) ? "—" : item.MatchNote);
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

        AddMainHeading(col, "06 · Details of MBBS Course - UG");
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

    private void AddBedDistributionSection(ColumnDescriptor col)
    {
        var beds = _model.BedDistribution;
        if (beds == null) return;

        AddMainHeading(col, "14 · Bed Distribution - MBBS - UG");
        AddSubHeading(col, "Bed Distribution - MBBS - UG", 135);
        col.Item().PaddingTop(8).Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(3);
                columns.RelativeColumn(1);
                columns.RelativeColumn(3);
                columns.RelativeColumn(1);
            });

            AddTextRow(table, "General Medicine", beds.GenMedicine?.ToString() ?? "—");
            AddTextRow(table, "Paediatrics", beds.Paediatrics?.ToString() ?? "—");
            AddTextRow(table, "Skin & V.D.", beds.SkinVD?.ToString() ?? "—");
            AddTextRow(table, "Psychiatry", beds.Psychiatry?.ToString() ?? "—");
            AddTextRow(table, "General Surgery", beds.GenSurgery?.ToString() ?? "—");
            AddTextRow(table, "Orthopaedics", beds.Orthopaedics?.ToString() ?? "—");
            AddTextRow(table, "Ophthalmology", beds.Ophthalmology?.ToString() ?? "—");
            AddTextRow(table, "ENT", beds.ENT?.ToString() ?? "—");
            AddTextRow(table, "Obstetrics & ANC", beds.ObstetricsANC?.ToString() ?? "—");
            AddTextRow(table, "Gynaecology", beds.Gynaecology?.ToString() ?? "—");
            AddTextRow(table, "Postpartum", beds.Postpartum?.ToString() ?? "—");
            AddTextRow(table, "Major OT", beds.MajorOT?.ToString() ?? "—");
            AddTextRow(table, "Minor OT", beds.MinorOT?.ToString() ?? "—");
            AddTextRow(table, "ICCU", beds.ICCU?.ToString() ?? "—");
            AddTextRow(table, "ICU", beds.ICU?.ToString() ?? "—");
            AddTextRow(table, "PICU / NICU", beds.PICU_NICU?.ToString() ?? "—");
            AddTextRow(table, "SICU", beds.SICU?.ToString() ?? "—");
            AddTextRow(table, "Total ICU Beds", beds.TotalICUBeds?.ToString() ?? "—");
            AddTextRow(table, "Casualty Beds", beds.CasualtyBeds?.ToString() ?? "—");
        });
    }

    private void AddDeanOrDirectorSection(ColumnDescriptor col)
    {
        var dean = _model?.InstitutionBasicVM?.DeanOrDirectorDetailDisplayVM;
        if (dean == null) return;

        AddMainHeading(col, "04 · Staff Particulars - Dean / Director");
        AddSubHeading(col, "Dean / Director Details", 125);

        col.Item().PaddingTop(8).Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(3); // Label
                columns.RelativeColumn(4); // Value
            });

            AddTextRow(table, "Name", dean.DeanOrDirectorName);
            AddTextRow(table, "Qualification", dean.DeanQualification);
            AddTextRow(table, "Qualification Date", dean.DeanQualificationDate);
            AddTextRow(table, "University", dean.DeanUniversity);
            AddTextRow(table, "State Council Number", dean.DeanStateCouncilNumber);
            AddTextRow(table, "Recognized by MCI", dean.RecognizedByMci);
        });
    }

    private void AddPrincipalSection(ColumnDescriptor col)
    {
        var principal = _model?.InstitutionBasicVM?.PrincipalDetailDisplayVM;
        if (principal == null) return;

        AddMainHeading(col, "05 · Staff Particulars - Principal");
        AddSubHeading(col, "Principal Details", 85);

        col.Item().PaddingTop(8).Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(3); // Label
                columns.RelativeColumn(4); // Value
            });

            AddTextRow(table, "Name", principal.PrincipalName);
            AddTextRow(table, "Qualification", principal.PrincipalQualification);
            AddTextRow(table, "Qualification Date", principal.PrincipalQualificationDate);
            AddTextRow(table, "University", principal.PrincipalUniversity);
            AddTextRow(table, "State Council Number", principal.PrincipalStateCouncilNumber);
            AddTextRow(table, "Recognized by MCI", principal.RecognizedByMci);
        });
    }

    private void AddAcademicMattersSection(ColumnDescriptor col)
    {
        var academic = _model.CAacademicMattersVM;
        if (academic == null) return;

        AddMainHeading(col, "18 · Academic Matters - UG");

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

            foreach (var row in rows.Take(4))
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
        var hospital = _model.CAHospitalAFfiliationCompVM;
        if (hospital?.ClinicalHospitalDetails == null) return;

        var h = hospital.ClinicalHospitalDetails;

        AddMainHeading(col, "13 · Clinical Facilities");

        // Section title
        col.Item().PaddingTop(30)
           .AlignCenter()
           .Text("Hospital Affiliation")
           .FontSize(14)
           .Bold();

        // --- CLINICAL HOSPITAL DETAILS ---
        col.Item().PaddingTop(10).Column(col2 =>
        {
            col2.Item().Text("Clinical Hospital Details")
                .FontSize(12)
                .SemiBold();

            col2.Item().PaddingTop(2).Row(row =>
            {
                row.ConstantItem(122)   // adjust length to fit text nicely
                    .LineHorizontal(1)
                    .LineColor(Colors.Black);

                row.RelativeItem();     // fills remaining space
            });
        });



        col.Item().PaddingTop(5).Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(); // Property
                columns.RelativeColumn(); // Value
            });

            // Add rows for each property
            void AddRow(string prop, string value)
            {
                table.Cell().Border(1).Padding(5).Text(prop).Bold();
                table.Cell().Border(1).Padding(5).Text(value);
            }

            AddRow("Hospital Name", h.HospitalName);
            AddRow("Hospital Type", h.HospitalType ?? "—");
            AddRow("Hospital Owned By", h.HospitalOwnedBy ?? "—");
            AddRow("Owner Name", h.OwnerName ?? "—");
            AddRow("Location", $"{h.DistrictName}, {h.TalukName}");
            AddRow("Total Beds", h.TotalBeds.ToString());
            AddRow("OPD per Day", h.OpdPerDay.ToString());
            AddRow("IPD Occupancy %", h.IpdOccupancyPercent.ToString());
            AddRow("Member of Trust", h.IsOwnerAmemberOfTrust ? "Yes" : "No");
            AddRow("Supporting Documents Uploaded", h.IsSupportingDocExists ? "Yes" : "No");
        });

        // --- AFFILIATED DOCUMENTS ---
        if (hospital.AffiliatedHospitalDocuments?.Any() == true)
        {
            col.Item().PaddingTop(20).Column(col2 =>
            {
                // Heading text
                col2.Item().Text("Affiliated Documents")
                    .FontSize(12)
                    .SemiBold();

                // Underline
                col2.Item().PaddingTop(2).Row(row =>
                {
                    row.ConstantItem(115)   // line length in points
                        .LineHorizontal(1)
                        .LineColor(Colors.Black);

                    row.RelativeItem();     // fills remaining space
                });
            });

            col.Item().PaddingTop(5).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn();     // Document Name
                    columns.RelativeColumn();     // Hospital Name
                    columns.ConstantColumn(60);   // Beds
                    columns.ConstantColumn(80);   // Exists
                });

                // Header
                table.Header(header =>
                {
                    header.Cell().Border(1).Padding(5).Text("Document Name").Bold();
                    header.Cell().Border(1).Padding(5).Text("Hospital Name").Bold();
                    header.Cell().Border(1).Padding(5).Text("Beds").Bold();
                    header.Cell().Border(1).Padding(5).Text("Exists").Bold();
                });

                foreach (var doc in hospital.AffiliatedHospitalDocuments)
                {
                    table.Cell().Border(1).Padding(5).Text(doc.DocumentName ?? "—");
                    table.Cell().Border(1).Padding(5).Text(doc.HospitalName ?? "—");
                    table.Cell().Border(1).Padding(5).AlignCenter().Text(doc.TotalBeds.ToString());
                    table.Cell().Border(1).Padding(5).AlignCenter().Text(doc.DocumentExists ? "Yes" : "No");
                }
            });
        }
    }


    private void AddDepartmentSections(ColumnDescriptor col)
    {
        var hospital = _model.CAHospitalAFfiliationCompVM;
        if (hospital?.Sections == null || !hospital.Sections.Any())
            return;

        var excludedSections = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "radiodiagnosis",
            "radiationoncology",
            "pharmacy",
            "indoorbedsunits",
            "outpatientarea",
            "artcentre"
        };

        // Loop through each department/section
        foreach (var section in hospital.Sections)
        {
            var normalizedSectionName = new string((section.SectionName ?? string.Empty)
                .Where(char.IsLetterOrDigit)
                .ToArray());

            if (excludedSections.Contains(normalizedSectionName))
                continue;

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
                .Text("Classroom Details")
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

    private void AddDepartmentOfficesAndMeuSection(ColumnDescriptor col)
    {
        var vm = _model.PhysicalFacilities?.DeptOfficeMeu;
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
                .Text("Building Details")
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

        AddMainHeading(col, "19 · Account and Fee Details");

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

        AddMainHeading(col, "20 · Staff Details");
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

        AddMainHeading(col, "17 · Vehicle Registration & Documents");
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
        var hostelItems = _model?.AdminTeachAndHostelVM?.HostelDetailsVM;

        if (hostelItems == null || !hostelItems.Any())
            return;

        AddMainHeading(col, "16 · Affiliation Hostel Details");
        AddSubHeading(col, "Hostel Details", 75);

        col.Item().PaddingTop(8).Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(2); // Hostel Type
                columns.RelativeColumn(2); // Built-up Area
                columns.ConstantColumn(70); // Separate Hostel
                columns.ConstantColumn(90); // Separate M/F
                columns.RelativeColumn(2); // Female Students
                columns.RelativeColumn(2); // Female Rooms
                columns.RelativeColumn(2); // Male Students
                columns.RelativeColumn(2); // Male Rooms
                columns.ConstantColumn(90); // Document
            });

            // -------- HEADER --------
            table.Header(header =>
            {
                header.Cell().Border(1).Padding(3).Text("Hostel Type").Bold();
                header.Cell().Border(1).Padding(3).Text("Built-up Area (Sq.Ft)").Bold();
                header.Cell().Border(1).Padding(3).AlignCenter().Text("Separate Hostel").Bold();
                header.Cell().Border(1).Padding(3).AlignCenter().Text("Separate M/F").Bold();
                header.Cell().Border(1).Padding(3).AlignCenter().Text("Female Students").Bold();
                header.Cell().Border(1).Padding(3).AlignCenter().Text("Female Rooms").Bold();
                header.Cell().Border(1).Padding(3).AlignCenter().Text("Male Students").Bold();
                header.Cell().Border(1).Padding(3).AlignCenter().Text("Male Rooms").Bold();
                header.Cell().Border(1).Padding(3).AlignCenter().Text("Possession Proof").Bold();
            });

            // -------- BODY --------
            foreach (var item in hostelItems)
            {
                table.Cell().Border(1).Padding(3).Text(item.HostelType);
                table.Cell().Border(1).Padding(3).Text(item.BuiltUpAreaSqFt);
                table.Cell().Border(1).Padding(3).AlignCenter().Text(item.HasSeparateHostel ? "Yes" : "No");
                table.Cell().Border(1).Padding(3).AlignCenter().Text(item.SeparateProvisionMaleFemale ? "Yes" : "No");
                table.Cell().Border(1).Padding(3).AlignCenter().Text(item.TotalFemaleStudents);
                table.Cell().Border(1).Padding(3).AlignCenter().Text(item.TotalFemaleRooms);
                table.Cell().Border(1).Padding(3).AlignCenter().Text(item.TotalMaleStudents);
                table.Cell().Border(1).Padding(3).AlignCenter().Text(item.TotalMaleRooms);
                table.Cell().Border(1).Padding(3).AlignCenter().Text(item.HasPossessionProof ? "Available" : "—");
            }
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
        AddMainHeading(col, "Faculty Details");
        AddSubHeading(col, "UG Faculty Details", 120);

        col.Item().PaddingTop(8).Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(2.2f); // Name
                columns.RelativeColumn(1.3f); // Designation
                columns.RelativeColumn(1.6f); // Contact
            });

            table.Header(header =>
            {
                header.Cell().Border(1).Background("#EAF0F5").Padding(3).Text("Name / Dates").Bold();
                header.Cell().Border(1).Background("#EAF0F5").Padding(3).Text("Designation").Bold();
                header.Cell().Border(1).Background("#EAF0F5").Padding(3).Text("Contact").Bold();
            });

            foreach (var departmentGroup in facultyList
                .GroupBy(f => string.IsNullOrWhiteSpace(f.DepartmentName)
                    ? (f.DepartmentCode ?? "Unassigned Department")
                    : f.DepartmentName)
                .OrderBy(group => group.Key))
            {
                table.Cell().ColumnSpan(3).Border(1).Background("#EAF0F5").Padding(4)
                    .Text($"Department: {departmentGroup.Key} ({departmentGroup.Count()} faculty)").Bold();

                foreach (var f in departmentGroup)
                {
                    table.Cell().Border(1).Padding(3).Column(cell =>
                    {
                        cell.Item().Text(f.NameOfFaculty ?? "—").Bold();
                        cell.Item().PaddingTop(2).Text($"DOB: {f.Dob ?? "—"}");
                        cell.Item().Text($"Appointment: {f.DateOfAppointment ?? "—"}");
                    });
                    table.Cell().Border(1).Padding(3).Text(f.DesignationName ?? f.DesignationCode ?? "—");
                    table.Cell().Border(1).Padding(3).Column(cell =>
                    {
                        cell.Item().Text(f.Mobile ?? "—");
                        cell.Item().Text(f.Email ?? "—");
                    });
                }
            }
        });
    }

    private static string? ResolveFacultyPhotoPath(string? storedPath)
    {
        if (string.IsNullOrWhiteSpace(storedPath))
        {
            return null;
        }

        var normalizedPath = storedPath.Trim().Replace("/", Path.DirectorySeparatorChar.ToString()).Replace("\\", Path.DirectorySeparatorChar.ToString());
        var fullPath = Path.IsPathRooted(normalizedPath)
            ? normalizedPath
            : Path.Combine(@"D:\MedicalUGFacultyList\Photos", Path.GetFileName(normalizedPath));

        return System.IO.File.Exists(fullPath) ? fullPath : null;
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
        var calculation = _model?.PaymentCalculation;

        if (calculation == null || !calculation.HasResult)
            return;

        AddMainHeading(col, "16 · Payment Calculation");

        col.Item().PaddingTop(25)
            .AlignCenter()
            .Text("Payment Calculation")
            .FontSize(14)
            .Bold();

        AddSubHeading(col, "Matched Courses", 105);
        col.Item().PaddingTop(8).Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(3);
                columns.RelativeColumn(3);
                columns.RelativeColumn(1.5f);
                columns.RelativeColumn(1.5f);
                columns.RelativeColumn(1.5f);
                columns.RelativeColumn(1.5f);
                columns.RelativeColumn(1.5f);
            });

            table.Header(header =>
            {
                header.Cell().Border(1).Padding(4).Text("Course Name").Bold();
                header.Cell().Border(1).Padding(4).Text("Course (Intake Sheet)").Bold();
                header.Cell().Border(1).Padding(4).Text("Course Level").Bold();
                header.Cell().Border(1).Padding(4).AlignCenter().Text("Intake").Bold();
                header.Cell().Border(1).Padding(4).AlignCenter().Text("Total Seats").Bold();
                header.Cell().Border(1).Padding(4).AlignCenter().Text("Increased Intake").Bold();
                header.Cell().Border(1).Padding(4).AlignCenter().Text("Academic Year").Bold();
            });

            foreach (var course in calculation.MatchedCourses)
            {
                table.Cell().Border(1).Padding(4).Text(course.CourseName ?? course.course ?? "—");
                table.Cell().Border(1).Padding(4).Text(course.course ?? "—");
                table.Cell().Border(1).Padding(4).Text(course.RawCourseLevel ?? course.ug_pg ?? "—");
                table.Cell().Border(1).Padding(4).AlignCenter().Text(course.Intake_26_27?.ToString() ?? "—");
                table.Cell().Border(1).Padding(4).AlignCenter().Text(course.TotalSeats?.ToString() ?? "0");
                table.Cell().Border(1).Padding(4).AlignCenter().Text(course.IncreasedIntake?.ToString() ?? "—");
                table.Cell().Border(1).Padding(4).AlignCenter().Text(course.AcademicYear ?? "—");
            }
        });

        AddSubHeading(col, "Fee Breakdown", 90);
        col.Item().PaddingTop(8).Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(3);
                columns.RelativeColumn(1.5f);
                columns.RelativeColumn(1.5f);
                columns.RelativeColumn(1.2f);
                columns.RelativeColumn(3);
                columns.RelativeColumn(1.5f);
            });

            table.Header(header =>
            {
                header.Cell().Border(1).Padding(4).Text("Fee Head").Bold();
                header.Cell().Border(1).Padding(4).AlignCenter().Text("Unit Amount").Bold();
                header.Cell().Border(1).Padding(4).AlignCenter().Text("Basis").Bold();
                header.Cell().Border(1).Padding(4).AlignCenter().Text("Multiplier").Bold();
                header.Cell().Border(1).Padding(4).Text("Calculation").Bold();
                header.Cell().Border(1).Padding(4).AlignCenter().Text("Line Amount").Bold();
            });

            foreach (var fee in calculation.FeeLines)
            {
                var basis = fee.IsPerCourse ? "Per course" : fee.IsPerSeat ? "Per seat" : "Fixed";
                var multiplier = fee.Multiplier > 0 ? fee.Multiplier.ToString() : "1";
                var formula = fee.IsPerCourse
                    ? $"{fee.UnitAmount:N2} x {multiplier} course(s)"
                    : fee.IsPerSeat
                        ? $"{fee.UnitAmount:N2} x {multiplier} seat(s)"
                        : $"{fee.UnitAmount:N2} x 1";
                table.Cell().Border(1).Padding(4).Text(fee.FeeHeadName ?? "—");
                table.Cell().Border(1).Padding(4).AlignRight().Text(fee.UnitAmount.ToString("N2"));
                table.Cell().Border(1).Padding(4).AlignCenter().Text(basis);
                table.Cell().Border(1).Padding(4).AlignCenter().Text(multiplier);
                table.Cell().Border(1).Padding(4).Text(formula);
                table.Cell().Border(1).Padding(4).AlignRight().Text(fee.LineAmount.ToString("N2"));
            }

            table.Cell().ColumnSpan(5).Border(1).Padding(5).AlignRight().Text("Grand Total").Bold();
            table.Cell().Border(1).Padding(5).AlignRight()
                .Text((calculation.Summary?.GrandTotal ?? calculation.FeeLines.Sum(x => x.LineAmount)).ToString("N2"))
                .Bold();
        });
    }

    private void AddNonTeachingStaffSection(ColumnDescriptor col)
    {
        var staffList = _model.FacultyDesigNonTeachDisplayVM?
                            .NonTeachingStaffSectionVM?
                            .Staffs;

        if (staffList == null || !staffList.Any())
            return;

        AddMainHeading(col, "23 · Non-Teaching Staff Details");
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
        var titleSeparator = title.IndexOf('·');
        var cleanTitle = titleSeparator >= 0
            ? title[(titleSeparator + 1)..].Trim()
            : title.Trim();
        _sectionNumber++;

        col.Item().PaddingTop(16).PaddingBottom(5).Background(_theme.Primary).BorderLeft(4).BorderColor(_theme.Accent).Padding(8).Row(row =>
        {
            row.ConstantItem(8);
            row.RelativeItem().Text($"{_sectionNumber}. {cleanTitle}").FontSize(11).Bold().FontColor(Colors.White);
        });
    }

    private void AddSubHeading(ColumnDescriptor col, string title, int lineLength = 150)
    {
        col.Item().PaddingTop(12).PaddingBottom(3).Row(row =>
        {
            row.ConstantItem(3).Background(_theme.Accent).Height(13);
            row.ConstantItem(7);
            row.RelativeItem().Text(title).FontSize(10).Bold().FontColor(_theme.Primary);
        });
    }
    private static void AddTextRow(TableDescriptor table, string label, object value)
    {
        table.Cell().BorderBottom(1).BorderColor("#E1E8EF").Background("#F6F8FA").Padding(5)
            .Text(label).FontSize(8).Bold().FontColor("#526A80");
        table.Cell().BorderBottom(1).BorderColor("#E1E8EF").Padding(5)
            .Text(value?.ToString() ?? "—").FontSize(8.5f).FontColor("#243B53");
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

    private sealed class PdfTheme
    {
        public string Primary { get; }
        public string Secondary { get; }
        public string Accent { get; }
        public string Light { get; }
        public string Border { get; }

        private PdfTheme(string primary, string secondary, string accent, string light, string border)
        {
            Primary = primary;
            Secondary = secondary;
            Accent = accent;
            Light = light;
            Border = border;
        }

        public static PdfTheme ForApplicationType(string? applicationType)
        {
            var value = (applicationType ?? string.Empty).Trim().ToLowerInvariant();

            if (value.Contains("continu"))
                return new PdfTheme("#4B5563", "#374151", "#C9A24B", "#F7F4EA", "#D8D1BE");

            if (value.Contains("renew"))
                return new PdfTheme("#7C2D12", "#C2410C", "#F59E0B", "#FFF7ED", "#FED7AA");

            if (value.Contains("increase") || value.Contains("intake"))
                return new PdfTheme("#0A3D7C", "#0D4A94", "#DCAE3F", "#EEF4FB", "#C9D8EA");

            if (value.Contains("first") || value.Contains("new") || value.Contains("initial"))
                return new PdfTheme("#1E3A8A", "#2563EB", "#06B6D4", "#EFF6FF", "#BFDBFE");

            return new PdfTheme("#123A63", "#1F6F6B", "#C9A24B", "#F4F7FA", "#D8E2EC");
        }
    }


}