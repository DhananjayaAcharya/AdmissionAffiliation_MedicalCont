using Medical_Affiliation.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SkiaSharp;

/// <summary>
/// RGUHS affiliation preview report - modern design (QuestPDF 2023.12+).
/// Themed per application type (Continuation / Renewal / Intake Enhancement / New / default).
/// </summary>
public class PreviewReportPdf : IDocument
{
    private readonly CApreviewViewModel _model;
    private readonly byte[] _logo;
    private readonly byte[] _collegeLogoBytes;
    private readonly PdfTheme _theme;
    private int _sectionNumber;
    private byte[]? _watermarkLogoBytes;

    // PG-only sections 08A - 08F (loaded in CAPreviewController via PgSectionLoader).
    private List<PgSectionGroup> _pgSections = new();

    // PG Section 08 - PG Course Affiliation tables.
    private List<PgSectionTable> _pgCourseTables = new();

    // PG intake figures, summed over all PG courses in Mst_MedicalCollegeCourseIntake:
    //   present    = SUM(Intake_26_27)
    //   additional = SUM(Increased_Intake)
    private int? _pgPresentIntake;
    private int? _pgAdditionalSeats;

    public PreviewReportPdf(CApreviewViewModel model, byte[] logo, byte[] clgLogoBytes)
    {
        _model = model;
        _logo = logo;
        _collegeLogoBytes = clgLogoBytes;
        _theme = PdfTheme.ForApplicationType(model.ApplicationType);
    }

    public PreviewReportPdf WithPgSections(List<PgSectionGroup>? groups)
    {
        _pgSections = groups ?? new List<PgSectionGroup>();
        return this;
    }

    public PreviewReportPdf WithPgCourseAffiliation(List<PgSectionTable>? tables)
    {
        _pgCourseTables = tables ?? new List<PgSectionTable>();
        return this;
    }

    public PreviewReportPdf WithPgIntake(int? presentIntake, int? additionalSeats)
    {
        _pgPresentIntake = presentIntake;
        _pgAdditionalSeats = additionalSeats;
        return this;
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

    // =====================================================================
    //  DOCUMENT COMPOSITION
    // =====================================================================
    public void Compose(IDocumentContainer container)
    {
        _sectionNumber = 0;

        container.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(24);
            page.DefaultTextStyle(text => text.FontFamily("Arial").FontSize(9).FontColor(_theme.Ink));

            // Watermark: page background renders at full page size on every page.
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

            page.Content().Border(1).BorderColor(_theme.Primary).Padding(10).Column(col =>
            {
                AddReportHeader(col);
                AddPreviewMetadataHeader(col);

                var courseLevel = (_model.ApplyingCourseLevel ?? string.Empty).Trim().ToUpperInvariant();
                var isUg = courseLevel == "UG";
                var isPg = courseLevel == "PG";

                // Basic Details
                AddInstitutionDetailsSection(col);
                AddAffiliatedInstituteDetailsSection(col);
                AddDeanOrDirectorSection(col);
                AddPrincipalSection(col);

                // Intake Details
                if (isUg)
                    AddAffiliationCourseSection(col);

                AddCourseIntakeListSection(col);

                if (isPg)
                {
                    AddPgCourseAffiliationSection(col);
                    AddPgSections(col);
                }

                // Physical Infrastructure
                AddHospitalAffiliationSection(col);
                AddDepartmentSections(col);
                if (isUg)
                    AddBedDistributionSection(col);

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
                    AddAcademicMattersSection(col);

                AddLibraryCommitteeSection(col);
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

            page.Footer().Element(c => ComposeFooter(c, "Confidential affiliation preview"));
        });

        container.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(42);
            page.DefaultTextStyle(text => text.FontFamily("Arial").FontSize(11).FontColor(_theme.Ink));

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

            page.Content()
                .Border(1)
                .BorderColor(_theme.Primary)
                .Padding(28)
                .Column(AddFinalDeclarationPage);

            page.Footer().Element(c => ComposeFooter(c, "Final declaration"));
        });
    }

    private void ComposeFooter(IContainer container, string caption)
    {
        container.PaddingTop(6).Column(c =>
        {
            c.Item().LineHorizontal(0.8f).LineColor(_theme.Accent);
            c.Item().PaddingTop(4).Row(row =>
            {
                row.RelativeItem().Text(t =>
                {
                    t.DefaultTextStyle(x => x.FontSize(7.5f).FontColor(_theme.Muted));
                    t.Span("RGUHS").Bold().FontColor(_theme.Primary);
                    t.Span($"  |  {caption}  |  {DateTime.Now:dd MMM yyyy, HH:mm}");
                });

                row.AutoItem().Background(_theme.Light).CornerRadius(8)
                    .PaddingVertical(1.5f).PaddingHorizontal(8)
                    .Text(_theme.TypeLabel).FontSize(7).Bold().FontColor(_theme.Primary);

                row.RelativeItem().AlignRight().Text(t =>
                {
                    t.DefaultTextStyle(x => x.FontSize(7.5f).FontColor(_theme.Muted));
                    t.Span("Page ");
                    t.CurrentPageNumber().Bold().FontColor(_theme.Primary);
                    t.Span(" / ");
                    t.TotalPages();
                });
            });
        });
    }

    // =====================================================================
    //  FINAL DECLARATION PAGE
    // =====================================================================
    private void AddFinalDeclarationPage(ColumnDescriptor col)
    {
        var principalName = _model.DeclarationVM?.PrincipalName;
        if (string.IsNullOrWhiteSpace(principalName))
            principalName = _model.InstitutionDetails?.Principal_Name;

        var collegeName = _model.CollegeName;
        if (string.IsNullOrWhiteSpace(collegeName))
            collegeName = _model.InstitutionDetails?.NameOfInstitution;

        col.Item().Background(_theme.Primary).Padding(16).Column(h =>
        {
            h.Item().AlignCenter().Text("FINAL DECLARATION")
                .FontSize(18).Bold().FontColor(Colors.White).LetterSpacing(0.08f);
            h.Item().PaddingTop(4).AlignCenter().Text("To be signed with the official seal of the Principal")
                .FontSize(9).Italic().FontColor("#CBD5E1");
        });
        col.Item().LineHorizontal(3).LineColor(_theme.Accent);

        col.Item().PaddingTop(30).BorderLeft(3).BorderColor(_theme.Accent).Background(_theme.Light).Padding(14).Text(text =>
        {
            text.Justify();
            text.DefaultTextStyle(x => x.FontSize(10.5f).LineHeight(1.5f));
            text.Span("I, ");
            text.Span(string.IsNullOrWhiteSpace(principalName) ? "{Principal name}" : principalName).Bold().FontColor(_theme.Primary);
            text.Span(", Principal of ");
            text.Span(string.IsNullOrWhiteSpace(collegeName) ? "{collegename}" : collegeName).Bold().FontColor(_theme.Primary);
            text.Span(", hereby solemnly declare and undertake that all documents, particulars, statements, declarations and information furnished or uploaded by the College in this application are true, complete, accurate and correct to the best of my knowledge and belief.");
        });

        col.Item().PaddingTop(16).Text(text =>
        {
            text.Justify();
            text.DefaultTextStyle(x => x.FontSize(10.5f).LineHeight(1.5f));
            text.Span("I further undertake that no material information has been concealed, suppressed or misrepresented in this application. I understand that any false, misleading, fabricated, incomplete or incorrect information or documentation furnished by the College may result in rejection or cancellation of the application, withdrawal of affiliation/approval, or such other action as may be taken by Rajiv Gandhi University of Health Sciences, Karnataka (RGUHS) in accordance with the applicable rules, regulations, ordinances and notifications.");
        });

        col.Item().PaddingTop(40).Row(row =>
        {
            row.RelativeItem().Column(d =>
            {
                d.Item().Text("DATE").FontSize(7.5f).Bold().FontColor(_theme.Muted);
                d.Item().PaddingTop(24).LineHorizontal(1).LineColor(_theme.Primary);
            });

            row.ConstantItem(40);

            row.ConstantItem(200).Column(sig =>
            {
                sig.Item().Height(76).Border(1).BorderColor(_theme.Border).Background(_theme.Light)
                    .AlignCenter().AlignMiddle()
                    .Text("OFFICIAL SEAL").Bold().FontSize(9).FontColor(_theme.Muted);
                sig.Item().PaddingTop(30).LineHorizontal(1).LineColor(_theme.Primary);
                sig.Item().PaddingTop(5).AlignCenter().Text("Principal Signature").Bold().FontColor(_theme.Primary);
                sig.Item().AlignCenter().Text(principalName ?? "").FontSize(9).FontColor(_theme.Muted);
            });
        });
    }

    // =====================================================================
    //  REPORT HEADER + KPI CARDS
    // =====================================================================
    private void AddReportHeader(ColumnDescriptor col)
    {
        col.Item().Background(_theme.Primary).Padding(14).Row(row =>
        {
            row.ConstantItem(66).Height(66).Background(Colors.White).CornerRadius(33).Padding(7)
                .AlignMiddle().AlignCenter().Image(_collegeLogoBytes).FitArea();

            row.ConstantItem(16);

            row.RelativeItem().AlignMiddle().Column(c =>
            {
                c.Item().Text("RAJIV GANDHI UNIVERSITY OF HEALTH SCIENCES")
                    .FontSize(15).Bold().FontColor(Colors.White);

                c.Item().PaddingTop(3).Text("Jayanagar, Bengaluru, Karnataka 560041")
                    .FontSize(8.5f).FontColor("#CBD5E1");

                //c.Item().PaddingTop(7).Row(r =>
                //{
                //    r.AutoItem().Background(_theme.Accent).CornerRadius(8)
                //        .PaddingVertical(2).PaddingHorizontal(10)
                //        .Text("AFFILIATION APPLICATION PREVIEW").FontSize(7.5f).Bold().FontColor(_theme.Primary);
                //});
            });
        });

        col.Item().LineHorizontal(3).LineColor(_theme.Accent);
    }

    private void AddPreviewMetadataHeader(ColumnDescriptor col)
    {
        var affiliationType = string.IsNullOrWhiteSpace(_model.ApplicationType)
            ? "Affiliation"
            : _model.ApplicationType.Trim();

        var isContinuation = string.Equals(affiliationType, "Continuation of Affiliation", StringComparison.OrdinalIgnoreCase);
        var isIncreaseInIntake = affiliationType.Contains("Enhancement", StringComparison.OrdinalIgnoreCase)
                                 || affiliationType.Contains("Increase", StringComparison.OrdinalIgnoreCase);

        var matchedCourses = _model.PaymentCalculation?.MatchedCourses;
        var firstMatchedCourse =
            matchedCourses?.FirstOrDefault(c => string.Equals(c.course?.Trim(), "MBBS", StringComparison.OrdinalIgnoreCase))
            ?? matchedCourses?.FirstOrDefault();

        var slabRows = _model.SeatSlabs ?? new List<SeatSlabRow>();
        var slabRow = slabRows.FirstOrDefault(r => string.Equals(r.Course?.Trim(), "MBBS", StringComparison.OrdinalIgnoreCase))
                      ?? slabRows.FirstOrDefault();

        var presentIntake = firstMatchedCourse?.Intake_26_27 ?? slabRow?.Intake_26_27;
        var additionalSeatsRequested = firstMatchedCourse?.IncreasedIntake;

        var additionalSeatCount = 0;
        if (!string.IsNullOrWhiteSpace(additionalSeatsRequested)
            && int.TryParse(additionalSeatsRequested, out var parsed))
            additionalSeatCount = parsed;

        var isPgLevel = string.Equals((_model.ApplyingCourseLevel ?? string.Empty).Trim(),
            "PG", StringComparison.OrdinalIgnoreCase);

        // PG: present intake = SUM(Intake_26_27) over all PG courses, additional = SUM(Increased_Intake)
        if (isPgLevel)
        {
            if (_pgPresentIntake.HasValue)
            {
                // value supplied by the controller (WithPgIntake)
                presentIntake = _pgPresentIntake.Value;
                additionalSeatCount = _pgAdditionalSeats ?? 0;
                additionalSeatsRequested = additionalSeatCount.ToString();
            }
            else if (matchedCourses != null && matchedCourses.Any())
            {
                // fallback: sum every matched PG course (same rows the Payment section lists)
                var pgCourses = matchedCourses
                    .Where(c => string.Equals(c.ug_pg?.Trim(), "PG", StringComparison.OrdinalIgnoreCase))
                    .ToList();
                if (pgCourses.Count == 0)
                    pgCourses = matchedCourses.ToList();

                presentIntake = pgCourses.Sum(c => c.Intake_26_27 ?? 0);
                additionalSeatCount = pgCourses.Sum(c =>
                    int.TryParse(c.IncreasedIntake, out var inc) ? inc : 0);
                additionalSeatsRequested = additionalSeatCount.ToString();
            }
        }

        // Continuation -> Intake_26_27 only; Increase / others -> Intake_26_27 + Increased_Intake
        var totalSeats = isContinuation
            ? (presentIntake ?? 0)
            : (presentIntake ?? 0) + additionalSeatCount;

        col.Item().PaddingTop(10).PaddingBottom(6).Column(section =>
        {
            section.Item().AlignCenter().Text(_model.CollegeName ?? "College Affiliation Application")
                .FontSize(15).Bold().FontColor(_theme.Primary);

            section.Item().PaddingTop(5).AlignCenter().Background(_theme.Light).CornerRadius(10)
                .PaddingVertical(3).PaddingHorizontal(12).Text(t =>
                {
                    t.Span("TYPE OF AFFILIATION  ").FontSize(7.5f).Bold().FontColor(_theme.Muted);
                    t.Span(affiliationType).FontSize(9.5f).Bold().FontColor(_theme.Secondary);
                });

            section.Item().PaddingTop(10).Row(row =>
            {
                row.Spacing(8);
                AddMetadataCell(row, "Application Type", _model.ApplicationType, false);
                AddMetadataCell(row, "Course Level", _model.ApplyingCourseLevel, false);
                if (isIncreaseInIntake)
                {
                    AddMetadataCell(row, "Present Intake", presentIntake?.ToString() ?? "—", true);
                    AddMetadataCell(row, "Additional Seats", additionalSeatsRequested ?? "—", true);
                }
                AddMetadataCell(row, "Total Seats", totalSeats.ToString(), true, highlight: true);
            });
        });
    }
    private void AddMetadataCell(RowDescriptor row, string label, string? value, bool big, bool highlight = false)
    {
        var card = row.RelativeItem()
            .Background(highlight ? _theme.Primary : _theme.Light)
            .BorderLeft(3).BorderColor(_theme.Accent)
            .PaddingVertical(7).PaddingHorizontal(9);

        card.Column(c =>
        {
            c.Item().Text(label.ToUpperInvariant()).FontSize(6.5f).Bold()
                .FontColor(highlight ? "#CBD5E1" : _theme.Muted);
            c.Item().PaddingTop(2).Text(string.IsNullOrWhiteSpace(value) ? "—" : value)
                .FontSize(big ? 15 : 9).Bold()
                .FontColor(highlight ? Colors.White : _theme.Primary);
        });
    }

    // =====================================================================
    //  01-05  BASIC DETAILS
    // =====================================================================
    private void AddInstitutionDetailsSection(ColumnDescriptor col)
    {
        var institution = _model?.InstitutionDetails;
        if (institution == null) return;

        AddMainHeading(col, "Institution Details");
        col.Item().PaddingTop(4).Table(table =>
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
       
        AddMainHeading(col, "Trust Institution Details");

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
            AddYesNoRow(table, "Trust PAN File", institute.hasPANFile, "Uploaded", "Not Uploaded");
            AddYesNoRow(table, "Bank Statement", institute.hasBankStatementFile, "Uploaded", "Not Uploaded");
            AddYesNoRow(table, "Registration Certificate", institute.hasRegistrationCertificateFile, "Uploaded", "Not Uploaded");
            AddYesNoRow(table, "Audit Statement", institute.hasAuditStatementFile, "Uploaded", "Not Uploaded");
        });
    }

    private void AddInstituteTabSection(ColumnDescriptor col, string title, Action<TableDescriptor> rows)
    {
        AddSubHeading(col, title);
        col.Item().PaddingTop(2).Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(3);
                columns.RelativeColumn(4);
            });

            rows(table);
        });
    }

    private void AddDeanOrDirectorSection(ColumnDescriptor col)
    {
        var dean = _model?.InstitutionBasicVM?.DeanOrDirectorDetailDisplayVM;
        if (dean == null) return;

        AddMainHeading(col, "Staff Particulars - Dean / Director");
        AddSubHeading(col, "Dean / Director Details");

        col.Item().PaddingTop(2).Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(3);
                columns.RelativeColumn(4);
            });

            AddTextRow(table, "Name", dean.DeanOrDirectorName);
            AddTextRow(table, "Qualification", dean.DeanQualification);
            AddTextRow(table, "Contact Number", _model.InstitutionDetails?.DeanMobileNumber);
            AddTextRow(table, "State Council Number", dean.DeanStateCouncilNumber);
            AddTextRow(table, "Recognized by MCI", dean.RecognizedByMci);
        });
    }

    private void AddPrincipalSection(ColumnDescriptor col)
    {
        var principal = _model?.InstitutionBasicVM?.PrincipalDetailDisplayVM;
        if (principal == null) return;

        AddMainHeading(col, "Staff Particulars - Principal");
        AddSubHeading(col, "Principal Details");

        col.Item().PaddingTop(2).Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(3);
                columns.RelativeColumn(4);
            });

            AddTextRow(table, "Name", principal.PrincipalName);
            AddTextRow(table, "Qualification", principal.PrincipalQualification);
            AddTextRow(table, "Contact Number", _model.InstitutionDetails?.PrincipalMobileNumber);
            AddTextRow(table, "State Council Number", principal.PrincipalStateCouncilNumber);
            AddTextRow(table, "Recognized by MCI", principal.RecognizedByMci);
        });
    }

    // =====================================================================
    //  COURSE / INTAKE
    // =====================================================================
    private void AddSanctionedIntakeSection(ColumnDescriptor col)
    {
        var intakeVM = _model.InstitutionBasicVM?.IntakeForCourseVM;

        if (intakeVM == null || intakeVM.Items == null || !intakeVM.Items.Any())
            return;

        AddSubHeading(col, "Sanctioned Intake Details");

        col.Item().PaddingTop(2).Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(3);
                columns.RelativeColumn(2);
                columns.RelativeColumn(2);
                columns.RelativeColumn(2);
            });

            table.Header(header =>
            {
                header.Cell().Element(Th).Text("Course Name");
                header.Cell().Element(Th).AlignCenter().Text("Sanctioned Intake");
                header.Cell().Element(Th).AlignCenter().Text("Eligible Seat Slab");
                header.Cell().Element(Th).AlignCenter().Text("Document");
            });

            foreach (var item in intakeVM.Items)
            {
                table.Cell().Element(Td).Text(item.CourseName);
                table.Cell().Element(Td).AlignCenter().Text(item.SanctionedIntake);
                table.Cell().Element(Td).AlignCenter().Text(item.EligibleSeatSlab ?? "—");
                table.Cell().Element(Td).AlignCenter().Text(item.HasDocument ? "Available" : "—");
            }
        });
    }

    private void AddCourseIntakeListSection(ColumnDescriptor col)
    {
        var rows = _model.CourseIntakeList;
        if (rows == null || rows.Count == 0) return;

        var affiliationType = string.IsNullOrWhiteSpace(_model.ApplicationType)
            ? "Affiliation"
            : _model.ApplicationType.Trim();
        var isIncreaseInIntake = affiliationType.Contains("Enhancement", StringComparison.OrdinalIgnoreCase)
            || affiliationType.Contains("Increase", StringComparison.OrdinalIgnoreCase);

        AddMainHeading(col, "Course Intake List");
        AddSubHeading(col, "Course Intake List");
        col.Item().PaddingTop(2).Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(3);
                columns.RelativeColumn(2);
                columns.RelativeColumn(1.8f);
                columns.RelativeColumn(1.5f);
                if (isIncreaseInIntake)
                {
                    columns.RelativeColumn(2);
                    columns.RelativeColumn(1.8f);
                }
                else
                {
                    columns.RelativeColumn(1.5f);
                    columns.RelativeColumn(2.5f);
                }
            });

            table.Header(header =>
            {
                header.Cell().Element(Th).Text("Course");
                header.Cell().Element(Th).Text("Application Type");
                header.Cell().Element(Th).Text("Applying Course Level");
                header.Cell().Element(Th).AlignCenter().Text("Present Intake");
                if (isIncreaseInIntake)
                {
                    header.Cell().Element(Th).AlignCenter().Text("Additional Seats Requested");
                    header.Cell().Element(Th).AlignCenter().Text("Total Seats");
                }
                else
                {
                    header.Cell().Element(Th).Text("Course Code");
                    header.Cell().Element(Th).Text("Match Note");
                }
            });

            foreach (var item in rows)
            {
                table.Cell().Element(Td).Text(item.CourseName);
                table.Cell().Element(Td).Text(affiliationType);
                table.Cell().Element(Td).Text(string.IsNullOrWhiteSpace(item.ApplyingCourseLevel) ? item.CourseLevel : item.ApplyingCourseLevel);
                table.Cell().Element(Td).AlignCenter().Text(item.Intake?.ToString() ?? "—");

                if (isIncreaseInIntake)
                {
                    table.Cell().Element(Td).AlignCenter().Text(item.AdditionalSeatsRequested?.ToString() ?? "—");
                    table.Cell().Element(Td).AlignCenter().Text(item.TotalSeats?.ToString() ?? "—");
                }
                else
                {
                    table.Cell().Element(Td).Text(item.CourseCode?.ToString() ?? "—");
                    table.Cell().Element(Td).Text(string.IsNullOrWhiteSpace(item.MatchNote) ? "—" : item.MatchNote);
                }
            }
        });
    }

    private void AddAffiliatedCoursesSection(ColumnDescriptor col)
    {
        var intakeDetails = _model?.InstitutionBasicVM;
        var courses = intakeDetails?.AffCoursesVM?.Items;

        if (courses == null || !courses.Any())
            return;

        AddSubHeading(col, "Course Details");

        col.Item().PaddingTop(2).Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(4);
                columns.ConstantColumn(70);
                columns.RelativeColumn(3);
                columns.ConstantColumn(70);
            });

            table.Header(header =>
            {
                header.Cell().Element(Th).Text("Course Name");
                header.Cell().Element(Th).AlignCenter().Text("Recognized");
                header.Cell().Element(Th).Text("RGUHS Notification No");
                header.Cell().Element(Th).AlignCenter().Text("Document");
            });

            foreach (var item in courses)
            {
                table.Cell().Element(Td).Text(item.CourseName);
                table.Cell().Element(Td).Element(c => Pill(c, item.IsRecognized));
                table.Cell().Element(Td).Text(item.RguhsNotificationNo ?? "—");
                table.Cell().Element(Td).AlignCenter().Text(item.HasDocument ? "Available" : "—");
            }
        });
    }

    // Section 08 - PG Course Affiliation (PG only). Falls back to the old course list
    // when none of the four PG course tables returned rows.
    private void AddPgCourseAffiliationSection(ColumnDescriptor col)
    {
        var tables = _pgCourseTables.Where(t => t.Rows.Count > 0).ToList();

        if (tables.Count == 0)
        {
            AddAffiliatedCoursesSection(col);
            return;
        }

        AddMainHeading(col, "PG Course Affiliation - PG");

        foreach (var table in tables)
        {
            AddSubHeading(col, table.Title);
            AddIndexedGrid(col, table.Headers, table.Rows);
        }
    }

    // PG-only sections 08A - 08F.
    //   - one record, or more than 7 columns  -> label / value layout
    //   - several records                     -> grid with a # column
    private void AddPgSections(ColumnDescriptor col)
    {
        foreach (var group in _pgSections.Where(g => g.HasData))
        {
            AddMainHeading(col, $"{group.Number} · {group.Title}");

            foreach (var table in group.Tables.Where(t => t.Rows.Count > 0))
            {
                AddSubHeading(col, table.Title);

                if (table.Rows.Count == 1 || table.Headers.Count > 7)
                {
                    foreach (var row in table.Rows)
                    {
                        col.Item().PaddingTop(4).Table(kv =>
                        {
                            kv.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(3);
                                columns.RelativeColumn(4);
                            });

                            for (var i = 0; i < table.Headers.Count; i++)
                                AddTextRow(kv, table.Headers[i], row[i]);
                        });
                    }
                }
                else
                {
                    AddIndexedGrid(col, table.Headers, table.Rows);
                }
            }
        }
    }

    // Shared "# | col | col ..." grid used by the PG sections.
    private void AddIndexedGrid(ColumnDescriptor col, IList<string> headers, IList<List<string>> rows)
    {
        col.Item().PaddingTop(2).Table(grid =>
        {
            grid.ColumnsDefinition(columns =>
            {
                columns.ConstantColumn(24);
                foreach (var _ in headers)
                    columns.RelativeColumn();
            });
            
            grid.Header(header =>
            {
                header.Cell().Element(Th).AlignCenter().Text("#").FontSize(8);
                foreach (var h in headers)
                    header.Cell().Element(Th).Text(h).FontSize(8);
            });

            for (var r = 0; r < rows.Count; r++)
            {
                grid.Cell().Element(Td).AlignCenter().Text((r + 1).ToString()).FontSize(8);
                foreach (var cell in rows[r])
                    grid.Cell().Element(Td).Text(cell).FontSize(8);
            }
        });
    }

    private void AddAffiliationCourseSection(ColumnDescriptor col)
    {
        var item = _model?.InstitutionBasicVM?.AffiliationCourseDetailVM;
        if (item == null) return;

        AddMainHeading(col, "Details of MBBS Course - UG");
        AddSubHeading(col, "Affiliated Course Details");

        col.Item().PaddingTop(2).Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(3);
                columns.RelativeColumn(4);
            });

            AddTextRow(table, "Course Name", item.CourseName);
            AddTextRow(table, "Intake 2026-27", item.IntakeDuring202526);
            AddTextRow(table, "Intake Slab", item.IntakeSlab);
            AddTextRow(table, "Permission Type", item.TypeofPermission);
            AddTextRow(table, "First Year of LOP", item.YearOfLop);
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

        var totalBeds = _model.CAHospitalAFfiliationCompVM?.ClinicalHospitalDetails?.TotalBeds;
        var totalBedsText = totalBeds > 0 ? totalBeds.ToString() : "—";
        var distributedBedsTotal = new int?[]
        {
            beds.GenMedicine, beds.Paediatrics, beds.SkinVD, beds.Psychiatry, beds.GenSurgery,
            beds.Orthopaedics, beds.Ophthalmology, beds.ENT, beds.ObstetricsANC, beds.Gynaecology,
            beds.Postpartum, beds.MajorOT, beds.MinorOT, beds.ICCU, beds.ICU, beds.PICU_NICU,
            beds.SICU, beds.CasualtyBeds
        }.Sum(value => value ?? 0);

        AddMainHeading(col, $"Bed Distribution - MBBS - UG | Total Beds: {totalBedsText}");
        AddSubHeading(col, "Bed Distribution - MBBS - UG");
        col.Item().PaddingTop(2).Table(table =>
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

        col.Item().PaddingTop(8).AlignRight().Background(_theme.Light).BorderLeft(3).BorderColor(_theme.Accent)
            .PaddingVertical(4).PaddingHorizontal(10).Text(text =>
            {
                text.Span("Total Bed Distribution:  ").Bold().FontColor(_theme.Muted);
                text.Span(distributedBedsTotal.ToString()).Bold().FontSize(12).FontColor(_theme.Primary);
            });
    }

    private void AddAcademicMattersSection(ColumnDescriptor col)
    {
        var academic = _model.CAacademicMattersVM;
        if (academic == null) return;

        AddMainHeading(col, "Academic Matters - UG");
        AddSubHeading(col, "Academic Performance");

        var rows = academic.AcademicRows;

        col.Item().PaddingTop(2).Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn();
                columns.ConstantColumn(60);
                columns.ConstantColumn(70);
                columns.ConstantColumn(60);
                columns.ConstantColumn(60);
                columns.ConstantColumn(70);
                columns.ConstantColumn(80);
                columns.RelativeColumn();
            });

            table.Header(header =>
            {
                header.Cell().Element(Th).AlignCenter().Text("Year");
                header.Cell().Element(Th).AlignCenter().Text("Regular");
                header.Cell().Element(Th).AlignCenter().Text("Repeaters");
                header.Cell().Element(Th).AlignCenter().Text("Passed");
                header.Cell().Element(Th).AlignCenter().Text("Pass %");
                header.Cell().Element(Th).AlignCenter().Text("First Class");
                header.Cell().Element(Th).AlignCenter().Text("Distinction");
                header.Cell().Element(Th).Text("Remarks");
            });

            foreach (var row in rows.Take(4))
            {
                table.Cell().Element(Td).AlignCenter().Text(row.YearName);
                table.Cell().Element(Td).AlignCenter().Text(row.RegularStudents.ToString());
                table.Cell().Element(Td).AlignCenter().Text(row.RepeaterStudents.ToString());
                table.Cell().Element(Td).AlignCenter().Text(row.NumberOfStudentsPassed.ToString());
                table.Cell().Element(Td).AlignCenter().Text((row.PassPercentage ?? 0).ToString("0.00"));
                table.Cell().Element(Td).AlignCenter().Text(row.FirstClassCount.ToString());
                table.Cell().Element(Td).AlignCenter().Text(row.DistinctionCount.ToString());
                table.Cell().Element(Td).Text(string.IsNullOrEmpty(row.Remarks) ? "—" : row.Remarks);
            }

        });

        if (academic.CourseCurriculumdvm != null && academic.CourseCurriculumdvm.Any())
        {
            AddSubHeading(col, "Course Curriculum");
            
            col.Item().PaddingTop(2).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(4);
                    columns.RelativeColumn(4);
                    columns.ConstantColumn(80);
                });

                table.Header(header =>
                {
                    header.Cell().Element(Th).Text("Curriculum");
                    header.Cell().Element(Th).Text("Details");
                    header.Cell().Element(Th).AlignCenter().Text("Uploaded");
                });

                foreach (var item in academic.CourseCurriculumdvm)
                {
                    table.Cell().Element(Td).Text(item.CurriculumName);
                    table.Cell().Element(Td).Text(item.CurriculumDetails ?? "—");
                    table.Cell().Element(Td).Element(c => Pill(c, item.HasPdf));
                }
            });
        }

        if (academic.StudentRegisterRecords != null && academic.StudentRegisterRecords.Any())
        {
            AddSubHeading(col, "Student Register Records");

            col.Item().PaddingTop(2).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn();
                    columns.ConstantColumn(120);
                });

                table.Header(header =>
                {
                    header.Cell().Element(Th).Text("Register Name");
                    header.Cell().Element(Th).AlignCenter().Text("Maintenance Status");
                });

                foreach (var record in academic.StudentRegisterRecords)
                {
                    table.Cell().Element(Td).Text(record.RegisterName);
                    table.Cell().Element(Td).Element(c => Pill(c, record.IsExists));
                }
            });
        }
    }

    // =====================================================================
    //  HOSPITAL / CLINICAL
    // =====================================================================
    private void AddHospitalAffiliationSection(ColumnDescriptor col)
    {
        var hospital = _model.CAHospitalAFfiliationCompVM;
        if (hospital?.ClinicalHospitalDetails == null) return;

        var h = hospital.ClinicalHospitalDetails;

        AddMainHeading(col, "Clinical Facilities");
        AddSubHeading(col, "Hospital Affiliation - Clinical Hospital Details");

        col.Item().PaddingTop(2).Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn();
                columns.RelativeColumn();
            });

            AddTextRow(table, "Hospital Name", h.HospitalName);
            AddTextRow(table, "Hospital Type", h.HospitalType ?? "—");
            AddTextRow(table, "Hospital Owned By", h.HospitalOwnedBy ?? "—");
            AddTextRow(table, "Owner Name", h.OwnerName ?? "—");
            AddTextRow(table, "Location", $"{h.DistrictName}, {h.TalukName}");

            // Highlighted key figure
            table.Cell().Background("#FFF3CD").BorderBottom(0.6f).BorderColor("#D39E00")
                .BorderLeft(2).PaddingVertical(5).PaddingHorizontal(7)
                .Text("Total Beds").FontSize(8.5f).Bold().FontColor("#7A5200");
            table.Cell().Background("#FFF3CD").BorderBottom(0.6f).BorderColor("#D39E00")
                .PaddingVertical(5).PaddingHorizontal(7)
                .Text(h.TotalBeds.ToString()).FontSize(10).Bold().FontColor("#7A5200");

            AddTextRow(table, "OPD per Day", h.OpdPerDay.ToString());
            AddTextRow(table, "IPD Occupancy %", h.IpdOccupancyPercent.ToString());
            AddYesNoNullableRow(table, "Member of Trust", h.IsOwnerAmemberOfTrust);
            AddYesNoNullableRow(table, "Supporting Documents Uploaded", h.IsSupportingDocExists);
        });

        if (hospital.AffiliatedHospitalDocuments?.Any() == true)
        {
            AddSubHeading(col, "Affiliated Documents");

            col.Item().PaddingTop(2).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                    columns.ConstantColumn(60);
                    columns.ConstantColumn(80);
                });

                table.Header(header =>
                {
                    header.Cell().Element(Th).Text("Document Name");
                    header.Cell().Element(Th).Text("Hospital Name");
                    header.Cell().Element(Th).AlignCenter().Text("Beds");
                    header.Cell().Element(Th).AlignCenter().Text("Exists");
                });

                foreach (var doc in hospital.AffiliatedHospitalDocuments)
                {
                    table.Cell().Element(Td).Text(doc.DocumentName ?? "—");
                    table.Cell().Element(Td).Text(doc.HospitalName ?? "—");
                    table.Cell().Element(Td).AlignCenter().Text(doc.TotalBeds.ToString());
                    table.Cell().Element(Td).Element(c => Pill(c, doc.DocumentExists));
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
            "radiodiagnosis", "radiationoncology", "pharmacy",
            "indoorbedsunits", "outpatientarea", "artcentre"
        };

        foreach (var section in hospital.Sections)
        {
            var normalizedSectionName = new string((section.SectionName ?? string.Empty)
                .Where(char.IsLetterOrDigit)
                .ToArray());

            if (excludedSections.Contains(normalizedSectionName))
                continue;

            AddSubHeading(col, section.SectionName ?? "Department");

            if (section.Items != null && section.Items.Any())
            {
                col.Item().PaddingTop(2).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(4);
                        columns.ConstantColumn(80);
                    });

                    table.Header(header =>
                    {
                        header.Cell().Element(Th).Text("Requirement");
                        header.Cell().Element(Th).AlignCenter().Text("Compliant");
                    });

                    foreach (var item in section.Items)
                    {
                        table.Cell().Element(Td).Text(item.RequirementName);
                        table.Cell().Element(Td).Element(c => Pill(c, item.IsCompliant));
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

        AddSubHeading(col, "Indoor Beds Occupancy");

        col.Item().PaddingTop(2).Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(4);
                columns.RelativeColumn(2);
                columns.ConstantColumn(60);
                columns.ConstantColumn(60);
            });

            table.Header(header =>
            {
                header.Cell().Element(Th).Text("Department Name");
                header.Cell().Element(Th).Text("Seat Slab / Intake");
                header.Cell().Element(Th).AlignCenter().Text("RGUHS Intake");
                header.Cell().Element(Th).AlignCenter().Text("College Intake");
            });

            foreach (var item in hospitalData.IndoorBedsOccupancy)
            {
                table.Cell().Element(Td).Text(item.DepartmentName);
                table.Cell().Element(Td).Text(item.SeatSlabId.ToString());
                table.Cell().Element(Td).AlignCenter().Text(item.RGUHSintake.ToString());
                table.Cell().Element(Td).AlignCenter().Text(item.CollegeIntake.ToString());
            }
        });
    }

    private void AddSupervisionInFieldPracticeAreaSection(ColumnDescriptor col)
    {
        var supervisionList = _model.CAHospitalAFfiliationCompVM?.SuperVisionInFPa;
        if (supervisionList == null || !supervisionList.Any())
            return;

        var supervision = supervisionList.First();
        if (supervision.Items == null || !supervision.Items.Any())
            return;

        AddSubHeading(col, "Supervision in Field Practice Area");

        col.Item().PaddingTop(2).Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.ConstantColumn(60);
                columns.RelativeColumn(2);
                columns.RelativeColumn(2);
                columns.ConstantColumn(70);
                columns.RelativeColumn(2);
                columns.ConstantColumn(90);
                columns.ConstantColumn(90);
                columns.RelativeColumn(3);
            });

            table.Header(header =>
            {
                header.Cell().Element(Th).Text("Post");
                header.Cell().Element(Th).Text("Name");
                header.Cell().Element(Th).Text("Qualification");
                header.Cell().Element(Th).Text("Year");
                header.Cell().Element(Th).Text("University");
                header.Cell().Element(Th).Text("UG Period");
                header.Cell().Element(Th).Text("PG Period");
                header.Cell().Element(Th).Text("Responsibilities");
            });

            foreach (var item in supervision.Items)
            {
                table.Cell().Element(Td).Text(item.Post);
                table.Cell().Element(Td).Text(item.Name);
                table.Cell().Element(Td).Text(item.Qualification);
                table.Cell().Element(Td).AlignCenter().Text(item.YearOfQualification.ToString());
                table.Cell().Element(Td).Text(item.University);
                table.Cell().Element(Td).AlignCenter().Text(FormatPeriod(item.UgFromDate, item.UgToDate));
                table.Cell().Element(Td).AlignCenter().Text(FormatPeriod(item.PgFromDate, item.PgToDate));
                table.Cell().Element(Td).Text(string.IsNullOrWhiteSpace(item.Responsibilities) ? "—" : item.Responsibilities);
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

    // =====================================================================
    //  PHYSICAL FACILITIES
    // =====================================================================
    private void AddSkillsLabSection(ColumnDescriptor col)
    {
        var lab = _model.PhysicalFacilities.SkillsLab;
        if (lab == null)
            return;

        AddMainHeading(col, "Classroom Details");

        AddSubHeading(col, "Intake and Area Details");
        col.Item().PaddingTop(2).Table(table =>
        {
            table.ColumnsDefinition(c => { c.RelativeColumn(3); c.RelativeColumn(2); });

            AddTextRow(table, "Annual MBBS Intake", lab.AnnualMbbsIntake);
            AddTextRow(table, "Total Area Required (Sq.m)", lab.TotalAreaRequiredSqm);
            AddTextRow(table, "Total Area Available (Sq.m)", lab.TotalAreaAvailableSqm);
            AddTextRow(table, "Area Deficiency (Sq.m)", lab.TotalAreaDeficiencySqm);
            AddYesNoNullableRow(table, "Six weeks training before clinical posting",
                lab.SixWeeksTrainingCompletedBeforeClinical);
        });

        AddSubHeading(col, "Examination Rooms and Infrastructure");
        col.Item().PaddingTop(2).Table(table =>
        {
            table.ColumnsDefinition(c => { c.RelativeColumn(4); c.ConstantColumn(90); });

            AddTextRow(table, "Number of examination rooms", lab.NumberOfExaminationRooms);
            AddYesNoNullableRow(table, "Minimum four examination rooms available", lab.HasMinFourExamRooms);
            AddYesNoNullableRow(table, "Demonstration room for small groups", lab.HasDemoRoomSmallGroups);
            AddYesNoNullableRow(table, "Debrief / review area available", lab.HasDebriefArea);
            AddYesNoNullableRow(table, "Faculty coordinator room available", lab.HasFacultyCoordinatorRoom);
            AddYesNoNullableRow(table, "Support staff room available", lab.HasSupportStaffRoom);
            AddYesNoNullableRow(table, "Storage for mannequins/equipment available", lab.HasStorageForMannequins);
            AddYesNoNullableRow(table, "Video recording & review facility available", lab.HasVideoRecordingFacility);
        });

        AddSubHeading(col, "Skill Stations and Equipment");
        col.Item().PaddingTop(2).Table(table =>
        {
            table.ColumnsDefinition(c => { c.RelativeColumn(4); c.ConstantColumn(90); });

            AddTextRow(table, "Number of skill stations", lab.NumberOfSkillStations);
            AddYesNoNullableRow(table, "Group and individual stations available", lab.HasGroupAndIndividualStations);
            AddYesNoNullableRow(table, "Required trainers and mannequins as per CBME", lab.HasRequiredTrainersAndMannequins);
        });

        AddSubHeading(col, "Staffing and IT Facilities");
        col.Item().PaddingTop(2).Table(table =>
        {
            table.ColumnsDefinition(c => { c.RelativeColumn(4); c.ConstantColumn(90); });

            AddYesNoNullableRow(table, "Dedicated technical officer available", lab.HasDedicatedTechnicalOfficer);
            AddYesNoNullableRow(table, "Adequate support staff available", lab.HasAdequateSupportStaff);
            AddYesNoNullableRow(table, "Teaching areas have AV facilities", lab.TeachingAreasHaveAV);
            AddYesNoNullableRow(table, "Teaching areas have Internet", lab.TeachingAreasHaveInternet);
            AddYesNoNullableRow(table, "Skills lab enabled for E-learning", lab.SkillsLabEnabledForELearning);
        });
    }

    private void AddLaboratoryEquipmentSection(ColumnDescriptor col)
    {
        var vm = _model.PhysicalFacilities.LaboratoryEquipment;
        if (vm == null || vm.Courses == null || !vm.Courses.Any())
            return;

        AddSubHeading(col, "Laboratory Equipment");

        foreach (var course in vm.Courses)
        {
            foreach (var subject in course.Subjects)
            {
                col.Item().PaddingTop(8).Text($"Subject : {subject.Subject}")
                    .FontSize(10).Bold().FontColor(_theme.Secondary);

                col.Item().PaddingTop(3).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(4);
                        columns.ConstantColumn(80);
                        columns.ConstantColumn(80);
                        columns.ConstantColumn(80);
                    });

                    table.Header(header =>
                    {
                        header.Cell().Element(Th).Text("Equipment Name");
                        header.Cell().Element(Th).AlignCenter().Text("Required");
                        header.Cell().Element(Th).AlignCenter().Text("Available");
                        header.Cell().Element(Th).AlignCenter().Text("Status");
                    });

                    foreach (var eq in subject.Equipments)
                    {
                        table.Cell().Element(Td).Text(eq.EquipmentName);
                        table.Cell().Element(Td).AlignCenter().Text(eq.RequiredAsPerNorm.ToString());
                        table.Cell().Element(Td).AlignCenter().Text(eq.AvailableQuantity?.ToString() ?? "0");
                        table.Cell().Element(Td).Element(c => Pill(c, !eq.IsDeficient, "OK", "Deficient"));
                    }
                });
            }
        }
    }

    private void AddSkillsLabEquipmentSection(ColumnDescriptor col)
    {
        var vm = _model.PhysicalFacilities.SkillsLabEquipment;
        if (vm == null || vm.Items == null || !vm.Items.Any())
            return;

        AddSubHeading(col, "Skills Lab Equipment");

        col.Item().PaddingTop(2).Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(4);
                columns.ConstantColumn(80);
                columns.ConstantColumn(80);
                columns.ConstantColumn(60);
            });

            table.Header(header =>
            {
                header.Cell().Element(Th).Text("Equipment Name");
                header.Cell().Element(Th).AlignCenter().Text("Required");
                header.Cell().Element(Th).AlignCenter().Text("Available");
                header.Cell().Element(Th).AlignCenter().Text("Qty");
            });

            foreach (var item in vm.Items)
            {
                table.Cell().Element(Td).Text(item.Name);
                table.Cell().Element(Td).Element(c => Pill(c, item.IsRequired));
                table.Cell().Element(Td).Element(c => Pill(c, item.IsAvailable));
                table.Cell().Element(Td).AlignCenter().Text(item.Quantity?.ToString() ?? "—");
            }
        });

        if (vm.HasTrainingModulesForAllModels != null ||
            vm.UsesHybridModelsOrSimulations != null ||
            vm.HasComputerAssistedLearningSpace != null)
        {
            AddSubHeading(col, "Additional Facilities");

            col.Item().PaddingTop(2).Table(table =>
            {
                table.ColumnsDefinition(c => { c.RelativeColumn(4); c.ConstantColumn(90); });

                AddYesNoNullableRow(table, "Training modules available for all models", vm.HasTrainingModulesForAllModels);
                AddYesNoNullableRow(table, "Hybrid models or simulation-based training used", vm.UsesHybridModelsOrSimulations);
                AddYesNoNullableRow(table, "Computer-assisted learning space available", vm.HasComputerAssistedLearningSpace);
            });
        }
    }

    private void AddDepartmentOfficesAndMeuSection(ColumnDescriptor col)
    {
        var vm = _model.PhysicalFacilities?.DeptOfficeMeu;
        if (vm == null)
            return;

        AddMainHeading(col, "Department Offices & Medical Education Unit (MEU)");

        AddSubHeading(col, "Department Offices and Staff Rooms");
        col.Item().PaddingTop(2).Table(table =>
        {
            table.ColumnsDefinition(c => { c.RelativeColumn(4); c.ConstantColumn(90); });

            AddYesNoNullableRow(table, "HOD room with office space and record maintenance available", vm.HasHodRoomWithOfficeAndRecords);
            AddYesNoNullableRow(table, "Rooms available for faculty and residents", vm.HasRoomsForFacultyAndResidents);
            AddYesNoNullableRow(table, "Faculty rooms have communication, computer and internet facilities", vm.FacultyRoomsHaveCommunicationComputerInternet);
            AddYesNoNullableRow(table, "Rooms available for non-teaching staff", vm.HasRoomsForNonTeachingStaff);
        });

        AddSubHeading(col, "Medical Education Unit (MEU)");
        col.Item().PaddingTop(2).Table(table =>
        {
            table.ColumnsDefinition(c => { c.RelativeColumn(4); c.ConstantColumn(90); });

            AddYesNoNullableRow(table, "Medical Education Unit available", vm.HasMedicalEducationUnit);
            AddTextRow(table, "Medical Education Unit area (Sq.m)", vm.MedicalEducationUnitAreaSqm);
            AddYesNoNullableRow(table, "MEU equipped with Audio-Visual facilities", vm.MedicalEducationUnitHasAudioVisual);
            AddYesNoNullableRow(table, "MEU has Internet connectivity", vm.MedicalEducationUnitHasInternet);
        });

        AddSubHeading(col, "MEU Coordinator Details");
        col.Item().PaddingTop(2).Table(table =>
        {
            table.ColumnsDefinition(c => { c.RelativeColumn(3); c.RelativeColumn(4); });

            AddTextRow(table, "Coordinator Name", vm.MeuCoordinatorName);
            AddTextRow(table, "Designation / Department", vm.MeuCoordinatorDesignationDepartment);
            AddTextRow(table, "Phone Number", vm.MeuCoordinatorPhone);
            AddTextRow(table, "Email Address", vm.MeuCoordinatorEmail);
        });
    }

    private void AddSmallGroupTeachingSection(ColumnDescriptor col)
    {
        var vm = _model.PhysicalFacilities.SmallGroupTeaching;
        if (vm == null)
            return;

        AddMainHeading(col, "Building Details");

        AddSubHeading(col, "General Details");
        col.Item().PaddingTop(2).Table(table =>
        {
            table.ColumnsDefinition(c => { c.RelativeColumn(4); c.ConstantColumn(100); });

            AddTextRow(table, "Annual MBBS Intake", vm.AnnualMbbsIntake.ToString());
            AddTextRow(table, "Small Group Batch Size", vm.SmallGroupBatchSize.ToString());

            AddYesNoNullableRow(table, "Teaching areas shared by all departments", vm.TeachingAreasSharedAllDepts);
            AddYesNoNullableRow(table, "AV available in all teaching areas", vm.AvInAllTeachingAreas);
            AddYesNoNullableRow(table, "Internet available in all teaching areas", vm.InternetInAllTeachingAreas);
            AddYesNoNullableRow(table, "Digitally linked teaching areas", vm.DigitalLinkAllTeachingAreas);
        });

        AddSubHeading(col, "Teaching Rooms & Area");
        col.Item().PaddingTop(2).Table(table =>
        {
            table.ColumnsDefinition(c => { c.RelativeColumn(4); c.ConstantColumn(100); });

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

        AddMainHeading(col, "Student Practical Laboratories");

        AddSubHeading(col, "Laboratory Availability & Sharing");
        col.Item().PaddingTop(2).Table(table =>
        {
            table.ColumnsDefinition(c =>
            {
                c.RelativeColumn(4);
                c.ConstantColumn(90);
                c.ConstantColumn(90);
            });

            table.Header(header =>
            {
                header.Cell().Element(Th).Text("Laboratory");
                header.Cell().Element(Th).AlignCenter().Text("Available");
                header.Cell().Element(Th).AlignCenter().Text("Shared");
            });

            AddLabRow(table, "Histology", vm.HistologyAvailable, vm.HistologyShared);
            AddLabRow(table, "Clinical Physiology", vm.ClinicalPhysiologyAvailable, vm.ClinicalPhysiologyShared);
            AddLabRow(table, "Biochemistry", vm.BiochemistryAvailable, vm.BiochemistryShared);
            AddLabRow(table, "Histopathology / Cytopathology", vm.HistopathCytopathAvailable, vm.HistopathCytopathShared);
            AddLabRow(table, "Clinical Pathology / Hematology", vm.ClinPathHemeAvailable, vm.ClinPathHemeShared);
            AddLabRow(table, "Microbiology", vm.MicrobiologyAvailable, vm.MicrobiologyShared);
            AddLabRow(table, "Clinical Pharmacology", vm.ClinicalPharmAvailable, vm.ClinicalPharmShared);
            AddLabRow(table, "Community & Allied Pharmacology", vm.CalPharmAvailable, vm.CalPharmShared);
        });

        AddSubHeading(col, "Common Facilities");
        col.Item().PaddingTop(2).Table(table =>
        {
            table.ColumnsDefinition(c => { c.RelativeColumn(4); c.ConstantColumn(100); });

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

        AddMainHeading(col, "Museums");

        AddSubHeading(col, "Museum Availability");
        col.Item().PaddingTop(2).Table(table =>
        {
            table.ColumnsDefinition(c => { c.RelativeColumn(4); c.ConstantColumn(120); });

            AddYesNoNullableRow(table, "Separate Anatomy Museum available", vm.SeparateAnatomyMuseumAvailable);
            AddYesNoNullableRow(table, "Pathology & Forensic Medicine shared museum", vm.PathologyForensicSharedMuseum);
            AddYesNoNullableRow(table, "Pharmacology, Microbiology & Community Medicine shared museum", vm.PharmMicroCommSharedMuseum);
            AddYesNoNullableRow(table, "Teaching time-sharing programmed", vm.TeachingTimeSharingProgrammed);
        });

        AddSubHeading(col, "Seating Capacity & Area");
        col.Item().PaddingTop(2).Table(table =>
        {
            table.ColumnsDefinition(c => { c.RelativeColumn(4); c.ConstantColumn(120); });

            AddTextRow(table, "Seating capacity per museum", vm.SeatingCapacityPerMuseum);
            AddTextRow(table, "Seating area available (Sq. m)", vm.SeatingAreaAvailableSqm);
            AddTextRow(table, "Seating area required (Sq. m)", vm.SeatingAreaRequiredSqm);
            AddTextRow(table, "Seating area deficiency (Sq. m)", vm.SeatingAreaDeficiencySqm);
        });

        AddSubHeading(col, "Museum Facilities");
        col.Item().PaddingTop(2).Table(table =>
        {
            table.ColumnsDefinition(c => { c.RelativeColumn(4); c.ConstantColumn(120); });

            AddYesNoNullableRow(table, "Audio-visual facilities available", vm.MuseumsHaveAV);
            AddYesNoNullableRow(table, "Internet facility available", vm.MuseumsHaveInternet);
            AddYesNoNullableRow(table, "Digitally linked museums", vm.MuseumsDigitallyLinked);
            AddYesNoNullableRow(table, "Adequate racks and shelves available", vm.MuseumsHaveRacksShelves);
            AddYesNoNullableRow(table, "Radiology display facilities available", vm.MuseumsHaveRadiologyDisplay);
        });
    }

    // =====================================================================
    //  LIBRARY
    // =====================================================================
    private void AddLibraryServicesSection(ColumnDescriptor col)
    {
        var services = _model.LibraryDisplay?.caAffMedicalLibraryvm?.LibraryServices;
        if (services == null || !services.Any())
            return;

        AddSubHeading(col, "Library Services");

        col.Item().PaddingTop(2).Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(4);
                columns.ConstantColumn(100);
                columns.ConstantColumn(140);
            });

            table.Header(header =>
            {
                header.Cell().Element(Th).Text("Service");
                header.Cell().Element(Th).AlignCenter().Text("Available");
                header.Cell().Element(Th).AlignCenter().Text("Supporting Document");
            });

            foreach (var service in services)
            {
                table.Cell().Element(Td).Text(service.ServiceName);
                table.Cell().Element(Td).AlignCenter().Text(string.IsNullOrWhiteSpace(service.IsAvailable) ? "—" : service.IsAvailable);
                table.Cell().Element(Td).AlignCenter().Text(service.HasPdf ? "Available" : "—");
            }
        });
    }

    private void AddLibraryUsageReportSection(ColumnDescriptor col)
    {
        var usageVm = _model.LibraryDisplay?.caAffMedicalLibraryvm;
        if (usageVm == null)
            return;

        var hasUsageReport = !string.IsNullOrWhiteSpace(usageVm.ExistingUsageReportFileName);

        AddSubHeading(col, "Library Usage Report");

        col.Item().PaddingTop(2).Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(4);
                columns.ConstantColumn(100);
                columns.ConstantColumn(140);
            });

            table.Header(header =>
            {
                header.Cell().Element(Th).Text("Parameter");
                header.Cell().Element(Th).AlignCenter().Text("Available");
                header.Cell().Element(Th).AlignCenter().Text("Supporting Document");
            });

            table.Cell().Element(Td).Text("Library Usage Report (Last Academic Year)");
            table.Cell().Element(Td).Element(c => Pill(c, hasUsageReport));
            table.Cell().Element(Td).AlignCenter().Text(hasUsageReport ? "Available" : "—");
        });
    }

    private void AddLibraryStaffSection(ColumnDescriptor col)
    {
        var staffList = _model.LibraryDisplay?.caAffMedicalLibraryvm?.LibraryStaff;
        if (staffList == null || !staffList.Any())
            return;

        AddSubHeading(col, "Library Staff");

        col.Item().PaddingTop(2).Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(3);
                columns.RelativeColumn(3);
                columns.RelativeColumn(3);
                columns.ConstantColumn(80);
                columns.RelativeColumn(2);
            });

            table.Header(header =>
            {
                header.Cell().Element(Th).Text("Staff Name");
                header.Cell().Element(Th).Text("Designation");
                header.Cell().Element(Th).Text("Qualification");
                header.Cell().Element(Th).AlignCenter().Text("Experience (Years)");
                header.Cell().Element(Th).Text("Category");
            });

            foreach (var staff in staffList.Where(s => !s.IsDeleted))
            {
                table.Cell().Element(Td).Text(staff.StaffName ?? "—");
                table.Cell().Element(Td).Text(staff.Designation ?? "—");
                table.Cell().Element(Td).Text(staff.Qualification ?? "—");
                table.Cell().Element(Td).AlignCenter().Text(staff.Experience?.ToString() ?? "—");
                table.Cell().Element(Td).Text(staff.Category ?? "—");
            }
        });
    }

    private void AddDepartmentalLibrarySection(ColumnDescriptor col)
    {
        var departments = _model.LibraryDisplay?.caAffMedicalLibraryvm?.DepartmentLibraries;
        if (departments == null || !departments.Any())
            return;

        AddSubHeading(col, "Departmental Library");

        col.Item().PaddingTop(2).Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(3);
                columns.ConstantColumn(80);
                columns.ConstantColumn(100);
                columns.ConstantColumn(110);
                columns.RelativeColumn(2);
            });

            table.Header(header =>
            {
                header.Cell().Element(Th).Text("Department");
                header.Cell().Element(Th).AlignCenter().Text("Total Books");
                header.Cell().Element(Th).AlignCenter().Text("Books Added (Year)");
                header.Cell().Element(Th).AlignCenter().Text("Current Journals");
                header.Cell().Element(Th).Text("Library Staff");
            });

            foreach (var dept in departments.Where(d => !d.IsDeleted))
            {
                table.Cell().Element(Td).Text(dept.DepartmentName ?? "—");
                table.Cell().Element(Td).AlignCenter().Text(dept.TotalBooks?.ToString() ?? "—");
                table.Cell().Element(Td).AlignCenter().Text(dept.BooksAddedInYear?.ToString() ?? "—");
                table.Cell().Element(Td).AlignCenter().Text(dept.CurrentJournals?.ToString() ?? "—");

                var staffNames = string.Join(", ",
                    new[] { dept.LibraryStaff1, dept.LibraryStaff2 }
                        .Where(s => !string.IsNullOrWhiteSpace(s)));

                table.Cell().Element(Td).Text(string.IsNullOrWhiteSpace(staffNames) ? "—" : staffNames);
            }
        });
    }

    private void AddLibraryOtherDetailsSection(ColumnDescriptor col)
    {
        var details = _model.LibraryDisplay?.caAffMedicalLibraryvm?.OtherDetails;
        if (details == null)
            return;

        AddSubHeading(col, "Other Library Details");

        col.Item().PaddingTop(2).Table(table =>
        {
            table.ColumnsDefinition(columns => { columns.RelativeColumn(3); columns.RelativeColumn(2); });

            AddTextRow(table, "Digital Valuation Centre Available", details.HasDigitalValuationCentre ?? "—");
            AddTextRow(table, "Number of Systems", details.NoOfSystems);
            AddTextRow(table, "Stable Internet Facility", details.HasStableInternet ?? "—");
            AddTextRow(table, "CCTV / Surveillance System", details.HasCccameraSystem ?? "—");
            AddTextRow(table, "Special Features / Achievements", details.SpecialFeaturesQuestion ?? "—");
            AddTextRow(table, "Supporting Document", details.HasSpecialFeaturesPdf == true ? "Available" : "—");
        });
    }

    private void AddLibraryCommitteeSection(ColumnDescriptor col)
    {
        var committees = _model.LibraryDisplay?.librarayCommitteeVM?.Committees;
        if (committees == null || !committees.Any())
            return;

        AddSubHeading(col, "Committees");

        col.Item().PaddingTop(2).Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(4);
                columns.RelativeColumn(2);
                columns.RelativeColumn(2);
            });

            table.Header(header =>
            {
                header.Cell().Element(Th).Text("Committee Name");
                header.Cell().Element(Th).AlignCenter().Text("Available");
                header.Cell().Element(Th).AlignCenter().Text("Supporting Document");
            });

            foreach (var committee in committees)
            {
                table.Cell().Element(Td).Text(committee.CommitteeName ?? "—");
                table.Cell().Element(Td).Element(c => Pill(c, committee.IsPresent));
                table.Cell().Element(Td).AlignCenter().Text(committee.HasCommitteePdf ? "Available" : "—");
            }
        });
    }

    private void AddLibraryGeneralDetailsSection(ColumnDescriptor col)
    {
        var general = _model.LibraryDisplay?.LibraryGeneralVM;
        if (general == null)
            return;

        AddSubHeading(col, "Library General Details");

        col.Item().PaddingTop(2).Table(table =>
        {
            table.ColumnsDefinition(columns => { columns.RelativeColumn(3); columns.RelativeColumn(2); });

            AddTextRow(table, "Library Email ID", general.LibraryEmailId ?? "—");
            AddYesNoNullableRow(table, "Digital Library Available", general.HasDigitalLibrary);
            AddYesNoNullableRow(table, "Department-wise Library Available", general.HasDepartmentWiseLibrary);
            AddYesNoNullableRow(table, "HELInet Services Available", general.HasHelinetServices);
        });
    }

    private void AddLibraryItemsSection(ColumnDescriptor col)
    {
        var items = _model.LibraryDisplay?.LibraryItemListVM?.Items;
        if (items == null || !items.Any())
            return;

        AddSubHeading(col, "Library Items Details");

        col.Item().PaddingTop(2).Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(3);
                columns.RelativeColumn(2);
                columns.RelativeColumn(2);
            });

            table.Header(header =>
            {
                header.Cell().Element(Th).Text("Item");
                header.Cell().Element(Th).AlignCenter().Text("Previous Year");
                header.Cell().Element(Th).AlignCenter().Text("Current Year");
            });

            foreach (var item in items)
            {
                if (item.HasIndianForeignSplit)
                {
                    table.Cell().Element(Td).Text(item.ItemName);

                    table.Cell().Element(Td).Column(c =>
                    {
                        c.Item().Text($"Indian : {item.PreviousIndian}");
                        c.Item().Text($"Foreign : {item.PreviousForeign}");
                    });

                    table.Cell().Element(Td).Column(c =>
                    {
                        c.Item().Text($"Indian : {item.CurrentIndian}");
                        c.Item().Text($"Foreign : {item.CurrentForeign}");
                    });
                }
                else
                {
                    table.Cell().Element(Td).Text(item.ItemName);
                    table.Cell().Element(Td).AlignCenter().Text((item.PreviousIndian + item.PreviousForeign).ToString());
                    table.Cell().Element(Td).AlignCenter().Text((item.CurrentIndian + item.CurrentForeign).ToString());
                }
            }
        });
    }

    private void AddLibraryBuildingSection(ColumnDescriptor col)
    {
        var building = _model.LibraryDisplay?.LibraryBuildingVM;
        if (building == null)
            return;

        AddSubHeading(col, "Library Building Details");

        col.Item().PaddingTop(2).Table(table =>
        {
            table.ColumnsDefinition(columns => { columns.RelativeColumn(3); columns.RelativeColumn(2); });

            AddYesNoNullableRow(table, "Independent Library Building", building.IsIndependent);
            AddTextRow(table, "Total Area (in Sq. Mtrs)", building.AreaSqMtrs?.ToString("0.##") ?? "—");
        });
    }

    private void AddLibraryTechnicalProcessSection(ColumnDescriptor col)
    {
        var processes = _model.LibraryDisplay?.LibraryTechListVM?.Processes;
        if (processes == null || !processes.Any())
            return;

        AddSubHeading(col, "Library Technical Processes");

        col.Item().PaddingTop(2).Table(table =>
        {
            table.ColumnsDefinition(columns => { columns.RelativeColumn(3); columns.RelativeColumn(2); });

            table.Header(header =>
            {
                header.Cell().Element(Th).Text("Process");
                header.Cell().Element(Th).AlignCenter().Text("Details");
            });

            foreach (var process in processes)
            {
                table.Cell().Element(Td).Text(process.ProcessName);
                table.Cell().Element(Td).AlignCenter().Text(process.HasValue ? process.Value! : "—");
            }
        });
    }

    private void AddLibraryFinanceSection(ColumnDescriptor col)
    {
        var finance = _model.LibraryDisplay?.LibraryFinancVM;
        if (finance == null)
            return;

        AddSubHeading(col, "Library Finance Details");

        col.Item().PaddingTop(2).Table(table =>
        {
            table.ColumnsDefinition(columns => { columns.RelativeColumn(3); columns.RelativeColumn(2); });

            AddTextRow(table, "Total Budget (₹ in Lakhs)", finance.TotalBudgetLakhs?.ToString("0.##") ?? "—");
            AddTextRow(table, "Expenditure on Books (₹ in Lakhs)", finance.ExpenditureBooksLakhs?.ToString("0.##") ?? "—");
        });
    }

    private void AddLibraryEquipmentSection(ColumnDescriptor col)
    {
        var equipmentList = _model.LibraryDisplay;

        if (equipmentList == null || equipmentList.LibraryEquipmentListVM.Items == null || !equipmentList.LibraryEquipmentListVM.Items.Any())
            return;

        AddSubHeading(col, "Medical Library Equipment");

        col.Item().PaddingTop(2).Table(table =>
        {
            table.ColumnsDefinition(columns => { columns.RelativeColumn(3); columns.RelativeColumn(2); });

            foreach (var item in equipmentList.LibraryEquipmentListVM.Items)
                AddYesNoRow(table, item.EquipmentName, item.HasEquipment, "Available", "Not Available");
        });
    }

    private void AddResearchPublicationsSection(ColumnDescriptor col)
    {
        var data = _model?.LibraryDisplay?.ResearchPublicationsDisplayViewModel;
        if (data == null)
            return;

        AddSubHeading(col, "Research Publications");

        col.Item().PaddingTop(2).Table(table =>
        {
            table.ColumnsDefinition(columns => { columns.RelativeColumn(3); columns.RelativeColumn(2); });

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

    // =====================================================================
    //  FINANCE
    // =====================================================================
    private void AddFinanceAccountsAndFeesSection(ColumnDescriptor col)
    {
        var acc = _model.FinanceVm?.medCaAccountAndFee;
        if (acc == null)
            return;

        AddMainHeading(col, "Account and Fee Details");
        AddSubHeading(col, "Accounts and Fee Details");

        col.Item().PaddingTop(2).Table(table =>
        {
            table.ColumnsDefinition(columns => { columns.RelativeColumn(3); columns.RelativeColumn(2); });

            AddTextRow(table, "Authority Name & Address", acc.AuthorityNameAddress);
            AddTextRow(table, "Authority Contact", acc.AuthorityContact);

            AddTextRow(table, "Recurrent Annual (₹)", acc.RecurrentAnnual.ToString("0.##"));
            AddTextRow(table, "Non-Recurrent Annual (₹)", acc.NonRecurrentAnnual.ToString("0.##"));
            AddTextRow(table, "Deposits (₹)", acc.Deposits.ToString("0.##"));

            AddTextRow(table, "Tuition Fee (₹)", acc.TuitionFee.ToString("0.##"));
            AddTextRow(table, "Sports Fee (₹)", acc.SportsFee.ToString("0.##"));
            AddTextRow(table, "Union Fee (₹)", acc.UnionFee.ToString("0.##"));
            AddTextRow(table, "Library Fee (₹)", acc.LibraryFee.ToString("0.##"));
            AddTextRow(table, "Other Fee (₹)", acc.OtherFee.ToString("0.##"));
            AddTextRow(table, "Total Fee (₹)", acc.TotalFee.ToString("0.##"));

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

        AddMainHeading(col, "Staff Pay Scale Details");
        AddSubHeading(col, "Staff Pay Scale Details");

        col.Item().PaddingTop(2).Table(table =>
        {
            table.ColumnsDefinition(columns => { columns.RelativeColumn(4); columns.RelativeColumn(2); });

            table.Header(header =>
            {
                header.Cell().Element(Th).Text("Designation");
                header.Cell().Element(Th).AlignRight().Text("Pay Scale (₹)");
            });

            foreach (var staff in staffList)
            {
                table.Cell().Element(Td).Text(staff.DesignationName);
                table.Cell().Element(Td).AlignRight().Text(staff.PayScale.ToString("0.##"));
            }
        });
    }

    private void AddFinanceOtherStaffDetailsSection(ColumnDescriptor col)
    {
        var other = _model.FinanceVm?.otherStaffParticularsVM;
        if (other == null)
            return;

        AddSubHeading(col, "Other Staff & Compliance Details");

        col.Item().PaddingTop(2).Table(table =>
        {
            table.ColumnsDefinition(columns => { columns.RelativeColumn(3); columns.RelativeColumn(2); });

            AddYesNoNullableRow(table, "Teachers Updated in EMS", other.TeachersUpdatedInEms);
            AddYesNoNullableRow(table, "Examiner Details Attached", other.ExaminerDetailsAttached);

            AddTextRow(table, "Examiner Details Document", other.HasExaminerDetailsPdf ? "Available" : "—");
            AddTextRow(table, "AEBAS (Last 3 Months)", other.HasAebasLastThreeMonthsPdf ? "Available" : "—");
            AddTextRow(table, "AEBAS (Inspection Day)", other.HasAebasInspectionDayPdf ? "Available" : "—");

            AddYesNoNullableRow(table, "Service Register Maintained", other.ServiceRegisterMaintained);
            AddYesNoNullableRow(table, "Acquittance Register Maintained", other.AcquittanceRegisterMaintained);

            AddTextRow(table, "Provident Fund Records", other.HasProvidentFundPdf ? "Available" : "—");
            AddTextRow(table, "ESI Records", other.HasEsipdf ? "Available" : "—");
        });
    }

    // =====================================================================
    //  VEHICLES / ADMIN / HOSTEL
    // =====================================================================
    private void AddVehicleDetailsSection(ColumnDescriptor col)
    {
        var vehicleList = _model.VehicleDetailsVM;
        if (vehicleList == null || vehicleList.Items == null || !vehicleList.Items.Any())
            return;

        AddMainHeading(col, "Vehicle Registration & Documents");
        AddSubHeading(col, "Vehicle Details");

        col.Item().PaddingTop(2).Table(table =>
        {
            table.ColumnsDefinition(columns => { columns.RelativeColumn(3); columns.RelativeColumn(2); });

            foreach (var vehicle in vehicleList.Items)
                AddTextRow(table, "Vehicle Registration No", vehicle.VehicleRegNo);
        });
    }

    private void AddAdminTeachingBlockSection(ColumnDescriptor col)
    {
        var blocks = _model?.AdminTeachAndHostelVM?.AdminTeachingBlockDisplayVM;
        if (blocks == null || !blocks.Any())
            return;

        AddSubHeading(col, "Administrative & Teaching Block Facilities");

        col.Item().PaddingTop(2).Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(3);
                columns.RelativeColumn(2);
                columns.RelativeColumn(2);
                columns.RelativeColumn(2);
                columns.RelativeColumn(3);
            });

            table.Header(header =>
            {
                header.Cell().Element(Th).Text("Facility");
                header.Cell().Element(Th).AlignCenter().Text("Size as per Norms");
                header.Cell().Element(Th).AlignCenter().Text("Available");
                header.Cell().Element(Th).AlignCenter().Text("No. of Rooms");
                header.Cell().Element(Th).AlignCenter().Text("Size / Room");
            });

            foreach (var item in blocks)
            {
                table.Cell().Element(Td).Text(item.Facilities);
                table.Cell().Element(Td).AlignCenter().Text(item.SizeSqFtAsPerNorms);
                table.Cell().Element(Td).AlignCenter().Text(item.IsAvailable);
                table.Cell().Element(Td).AlignCenter().Text(item.NoOfRooms);
                table.Cell().Element(Td).AlignCenter().Text(item.SizeSqFtAvailablePerRoom);
            }
        });
    }

    private void AddHostelDetailsSection(ColumnDescriptor col)
    {
        var hostelItems = _model?.AdminTeachAndHostelVM?.HostelDetailsVM;
        if (hostelItems == null || !hostelItems.Any())
            return;

        AddMainHeading(col, "Affiliation Hostel Details");
        AddSubHeading(col, "Hostel Details");

        col.Item().PaddingTop(2).Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(2);
                columns.RelativeColumn(2);
                columns.ConstantColumn(70);
                columns.ConstantColumn(90);
                columns.RelativeColumn(2);
                columns.RelativeColumn(2);
                columns.RelativeColumn(2);
                columns.RelativeColumn(2);
                columns.ConstantColumn(90);
            });

            table.Header(header =>
            {
                header.Cell().Element(Th).Text("Hostel Type");
                header.Cell().Element(Th).Text("Built-up Area (Sq.Ft)");
                header.Cell().Element(Th).AlignCenter().Text("Separate Hostel");
                header.Cell().Element(Th).AlignCenter().Text("Separate M/F");
                header.Cell().Element(Th).AlignCenter().Text("Female Students");
                header.Cell().Element(Th).AlignCenter().Text("Female Rooms");
                header.Cell().Element(Th).AlignCenter().Text("Male Students");
                header.Cell().Element(Th).AlignCenter().Text("Male Rooms");
                header.Cell().Element(Th).AlignCenter().Text("Possession Proof");
            });

            foreach (var item in hostelItems)
            {
                table.Cell().Element(Td).Text(item.HostelType);
                table.Cell().Element(Td).Text(item.BuiltUpAreaSqFt);
                table.Cell().Element(Td).Element(c => Pill(c, item.HasSeparateHostel));
                table.Cell().Element(Td).Element(c => Pill(c, item.SeparateProvisionMaleFemale));
                table.Cell().Element(Td).AlignCenter().Text(item.TotalFemaleStudents);
                table.Cell().Element(Td).AlignCenter().Text(item.TotalFemaleRooms);
                table.Cell().Element(Td).AlignCenter().Text(item.TotalMaleStudents);
                table.Cell().Element(Td).AlignCenter().Text(item.TotalMaleRooms);
                table.Cell().Element(Td).AlignCenter().Text(item.HasPossessionProof ? "Available" : "—");
            }
        });
    }

    private void AddHostelFacilitiesSection(ColumnDescriptor col)
    {
        var facilities = _model.AdminTeachAndHostelVM?.AffHostelFacilitiesVM;
        if (facilities == null || !facilities.Any())
            return;

        AddSubHeading(col, "Hostel Facilities");

        col.Item().PaddingTop(2).Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(4);
                columns.ConstantColumn(120);
            });

            table.Header(header =>
            {
                header.Cell().Element(Th).Text("Facility");
                header.Cell().Element(Th).AlignCenter().Text("Availability");
            });

            foreach (var item in facilities)
            {
                table.Cell().Element(Td).Text(item.FacilityName);
                table.Cell().Element(Td).Element(c => Pill(c, item.IsAvailable, "Available", "Not Available"));
            }
        });
    }

    // =====================================================================
    //  HUMAN RESOURCE
    // =====================================================================
    private void AddFacultyDetailsSection(ColumnDescriptor col)
    {
        var facultyList = _model?.FacultyDesigNonTeachDisplayVM?.FacultyDetailDisplayVM;
        if (facultyList == null || !facultyList.Any())
            return;

        AddMainHeading(col, "Faculty Details");
        AddSubHeading(col, "UG Faculty Details");

        col.Item().PaddingTop(2).Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(2.2f);
                columns.RelativeColumn(1.3f);
                columns.RelativeColumn(1.6f);
            });

            table.Header(header =>
            {
                header.Cell().Element(Th).Text("Name / Dates");
                header.Cell().Element(Th).Text("Designation");
                header.Cell().Element(Th).Text("Contact");
            });

            foreach (var departmentGroup in facultyList
                .GroupBy(f => string.IsNullOrWhiteSpace(f.DepartmentName)
                    ? (f.DepartmentCode ?? "Unassigned Department")
                    : f.DepartmentName)
                .OrderBy(group => group.Key))
            {
                table.Cell().ColumnSpan(3).Element(GroupRow)
                    .Text($"Department: {departmentGroup.Key}  ({departmentGroup.Count()} faculty)");

                foreach (var f in departmentGroup)
                {
                    table.Cell().Element(Td).Column(cell =>
                    {
                        cell.Item().Text(f.NameOfFaculty ?? "—").Bold();
                        cell.Item().PaddingTop(2).Text($"DOB: {f.Dob ?? "—"}").FontColor(_theme.Muted).FontSize(8);
                        cell.Item().Text($"Appointment: {f.DateOfAppointment ?? "—"}").FontColor(_theme.Muted).FontSize(8);
                    });
                    table.Cell().Element(Td).Text(f.DesignationName ?? f.DesignationCode ?? "—");
                    table.Cell().Element(Td).Column(cell =>
                    {
                        cell.Item().Text(f.Mobile ?? "—");
                        cell.Item().Text(f.Email ?? "—").FontSize(8);
                    });
                }
            }
        });
    }

    private static string? ResolveFacultyPhotoPath(string? storedPath)
    {
        if (string.IsNullOrWhiteSpace(storedPath))
            return null;

        var normalizedPath = storedPath.Trim().Replace("/", Path.DirectorySeparatorChar.ToString()).Replace("\\", Path.DirectorySeparatorChar.ToString());
        var fullPath = Path.IsPathRooted(normalizedPath)
            ? normalizedPath
            : Path.Combine(@"D:\MedicalUGFacultyList\Photos", Path.GetFileName(normalizedPath));

        return System.IO.File.Exists(fullPath) ? fullPath : null;
    }

    // Available -> _model.AvailableFaculty (UG_Faculty_Details, counted per department + designation)
    // Seat Slab -> _model.SeatSlabs        (Mst_MedicalCollegeCourseIntake, by college code + course level)
    private void AddCollegeDesignationSection(ColumnDescriptor col)
    {
        var groups = _model.FacultyDesigNonTeachDisplayVM?.CollegeDesignationDisplayVM;
        if (groups == null || !groups.Any())
            return;

        var slabRows = _model.SeatSlabs ?? new List<SeatSlabRow>();
        var slabRow = slabRows.FirstOrDefault(r => string.Equals(r.Course?.Trim(), "MBBS", StringComparison.OrdinalIgnoreCase))
                      ?? slabRows.FirstOrDefault();
        var seatSlabText = slabRow?.Intake_26_27?.ToString() ?? "—";

        var availableRows = _model.AvailableFaculty ?? new List<FacultyAvailableRow>();

        AddSubHeading(col, "Designation & Intake Details");

        col.Item().PaddingTop(2).Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(3);
                columns.RelativeColumn(3);
                columns.ConstantColumn(80);
                columns.ConstantColumn(70);
            });

            table.Header(header =>
            {
                header.Cell().Element(Th).Text("Department");
                header.Cell().Element(Th).Text("Designation");
                header.Cell().Element(Th).AlignCenter().Text("Available");
                header.Cell().Element(Th).AlignCenter().Text("Seat Slab");
            });

            foreach (var group in groups)
            {
                var isFirstRow = true;

                foreach (var item in group.Designations)
                {
                    table.Cell().Element(Td).Text(isFirstRow ? group.Department ?? "—" : string.Empty).Bold();
                    table.Cell().Element(Td).Text(item.Designation);
                    table.Cell().Element(Td).AlignCenter().Text(ResolveAvailable(availableRows, group, item));
                    table.Cell().Element(Td).AlignCenter().Text(seatSlabText);

                    isFirstRow = false;
                }
            }
        });
    }

    private static string ResolveAvailable(List<FacultyAvailableRow> rows, object group, object item)
    {
        var designationCode = ReadProp(item, "DesignationCode");
        var departmentCode = ReadProp(group, "DepartmentCode") ?? ReadProp(item, "DepartmentCode");

        if (string.IsNullOrWhiteSpace(designationCode))
            return ReadProp(item, "AvailableIntake") ?? "0";

        var total = rows
            .Where(r => string.Equals(r.DesignationCode, designationCode, StringComparison.OrdinalIgnoreCase)
                        && (string.IsNullOrWhiteSpace(departmentCode)
                            || string.Equals(r.DepartmentCode, departmentCode, StringComparison.OrdinalIgnoreCase)))
            .Sum(r => r.Available);

        return total.ToString();
    }

    private static string? ReadProp(object? source, string name)
    {
        var value = source?.GetType().GetProperty(name)?.GetValue(source)?.ToString()?.Trim();
        return string.IsNullOrWhiteSpace(value) ? null : value;
    }

    private void AddNonTeachingStaffSection(ColumnDescriptor col)
    {
        var staffList = _model.FacultyDesigNonTeachDisplayVM?.NonTeachingStaffSectionVM?.Staffs;
        if (staffList == null || !staffList.Any())
            return;

        AddMainHeading(col, "Non-Teaching Staff Details");
        AddSubHeading(col, "Non-Teaching Staff Details");

        col.Item().PaddingTop(2).Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.ConstantColumn(45);
                columns.RelativeColumn(3);
                columns.RelativeColumn(2.5f);
                columns.RelativeColumn(2);
                columns.RelativeColumn(1.5f);
            });

            table.Header(header =>
            {
                header.Cell().Element(Th).AlignCenter().Text("Id");
                header.Cell().Element(Th).Text("Staff Name");
                header.Cell().Element(Th).Text("Designation");
                header.Cell().Element(Th).AlignCenter().Text("Mobile Number");
                header.Cell().Element(Th).AlignRight().Text("Salary Paid");
            });

            foreach (var staff in staffList)
            {
                table.Cell().Element(Td).AlignCenter().Text(staff.StaffId.ToString());
                table.Cell().Element(Td).Text(staff.StaffName);
                table.Cell().Element(Td).Text(staff.Designation);
                table.Cell().Element(Td).AlignCenter().Text(staff.MobileNumber ?? "—");
                table.Cell().Element(Td).AlignRight().Text(staff.SalaryPaid.ToString("N2"));
            }
        });
    }

    // =====================================================================
    //  PAYMENT
    // =====================================================================
    private void AddPaymentSection(ColumnDescriptor col)
    {
        var calculation = _model?.PaymentCalculation;
        if (calculation == null || !calculation.HasResult)
            return;

        var isContinuationOfAffiliation = string.Equals(
            _model.ApplicationType?.Trim(),
            "Continuation of Affiliation",
            StringComparison.OrdinalIgnoreCase);

        AddMainHeading(col, "Payment Calculation");
        AddSubHeading(col, "Matched Courses");

        col.Item().PaddingTop(2).Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(3);
                columns.RelativeColumn(3);
                columns.RelativeColumn(1.5f);
                columns.RelativeColumn(1.5f);
                columns.RelativeColumn(1.5f);
                if (!isContinuationOfAffiliation)
                {
                    columns.RelativeColumn(1.5f);
                    columns.RelativeColumn(1.5f);
                }
            });

            table.Header(header =>
            {
                header.Cell().Element(Th).Text("Course Name");
                header.Cell().Element(Th).Text("Course (Intake Sheet)");
                header.Cell().Element(Th).Text("Course Level");
                header.Cell().Element(Th).AlignCenter().Text("Intake");
                header.Cell().Element(Th).AlignCenter().Text("Total Seats");
                if (!isContinuationOfAffiliation)
                {
                    header.Cell().Element(Th).AlignCenter().Text("Increased Intake");
                    header.Cell().Element(Th).AlignCenter().Text("Academic Year");
                }
            });

            foreach (var course in calculation.MatchedCourses)
            {
                table.Cell().Element(Td).Text(course.CourseName ?? course.course ?? "—");
                table.Cell().Element(Td).Text(course.course ?? "—");
                table.Cell().Element(Td).Text(course.RawCourseLevel ?? course.ug_pg ?? "—");
                table.Cell().Element(Td).AlignCenter().Text(course.Intake_26_27?.ToString() ?? "—");
                table.Cell().Element(Td).AlignCenter().Text(course.TotalSeats?.ToString() ?? "0");
                if (!isContinuationOfAffiliation)
                {
                    table.Cell().Element(Td).AlignCenter().Text(course.IncreasedIntake?.ToString() ?? "—");
                    table.Cell().Element(Td).AlignCenter().Text(course.AcademicYear ?? "—");
                }
            }
        });

        AddSubHeading(col, "Fee Breakdown");

        col.Item().PaddingTop(2).Table(table =>
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
                header.Cell().Element(Th).Text("Fee Head");
                header.Cell().Element(Th).AlignRight().Text("Unit Amount");
                header.Cell().Element(Th).AlignCenter().Text("Basis");
                header.Cell().Element(Th).AlignCenter().Text("Multiplier");
                header.Cell().Element(Th).Text("Calculation");
                header.Cell().Element(Th).AlignRight().Text("Line Amount");
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

                table.Cell().Element(Td).Text(fee.FeeHeadName ?? "—");
                table.Cell().Element(Td).AlignRight().Text(fee.UnitAmount.ToString("N2"));
                table.Cell().Element(Td).AlignCenter().Text(basis);
                table.Cell().Element(Td).AlignCenter().Text(multiplier);
                table.Cell().Element(Td).Text(formula).FontColor(_theme.Muted);
                table.Cell().Element(Td).AlignRight().Text(fee.LineAmount.ToString("N2")).Bold();
            }

            // Grand total band
            table.Cell().ColumnSpan(5).Background(_theme.Primary).PaddingVertical(7).PaddingHorizontal(8)
                .AlignRight().Text("GRAND TOTAL (₹)").Bold().FontColor(Colors.White);
            table.Cell().Background(_theme.Primary).PaddingVertical(7).PaddingHorizontal(6).AlignRight()
                .Text((calculation.Summary?.GrandTotal ?? calculation.FeeLines.Sum(x => x.LineAmount)).ToString("N2"))
                .Bold().FontSize(10).FontColor(_theme.Accent);
        });
    }

    // =====================================================================
    //  DESIGN SYSTEM - headings, rows, cell styles
    // =====================================================================
    private void AddMainHeading(ColumnDescriptor col, string title)
    {
        var sep = title.IndexOf('·');
        var cleanTitle = sep >= 0 ? title[(sep + 1)..].Trim() : title.Trim();
        _sectionNumber++;

        col.Item().PaddingTop(16).PaddingBottom(6).Column(c =>
        {
            c.Item().Background(_theme.Primary).PaddingVertical(6).PaddingHorizontal(8).Row(row =>
            {
                row.ConstantItem(22).Height(22).Background(_theme.Accent).CornerRadius(11)
                    .AlignMiddle().AlignCenter()
                    .Text(_sectionNumber.ToString("00")).FontSize(8.5f).Bold().FontColor(_theme.Primary);

                row.ConstantItem(10);

                row.RelativeItem().AlignMiddle().Text(cleanTitle.ToUpperInvariant())
                    .FontSize(10.5f).Bold().FontColor(Colors.White).LetterSpacing(0.04f);
            });
            c.Item().LineHorizontal(2).LineColor(_theme.Accent);
        });
    }

    // lineLength kept for backward compatibility (ignored)
    private void AddSubHeading(ColumnDescriptor col, string title, int lineLength = 150)
    {
        col.Item().PaddingTop(10).PaddingBottom(4).Row(row =>
        {
            row.ConstantItem(3).Height(14).Background(_theme.Accent);
            row.ConstantItem(7);
            row.AutoItem().Text(title).FontSize(10).Bold().FontColor(_theme.Primary);
            row.ConstantItem(8);
            row.RelativeItem().AlignMiddle().LineHorizontal(0.6f).LineColor(_theme.Border);
        });
    }

    private void AddTextRow(TableDescriptor table, string label, object? value)
    {
        table.Cell().BorderBottom(0.6f).BorderColor(_theme.Border)
            .BorderLeft(2).BorderColor(_theme.Accent)
            .Background(_theme.Label).PaddingVertical(4).PaddingHorizontal(7)
            .Text(label).FontSize(8).Bold().FontColor(_theme.Muted);

        var text = string.IsNullOrWhiteSpace(value?.ToString()) ? "—" : value!.ToString()!;
        table.Cell().BorderBottom(0.6f).BorderColor(_theme.Border)
            .PaddingVertical(4).PaddingHorizontal(7)
            .Text(text).FontSize(8.5f).FontColor(_theme.Ink);
    }

    private void AddYesNoNullableRow(TableDescriptor table, string label, bool? value)
    {
        table.Cell().BorderBottom(0.6f).BorderColor(_theme.Border)
            .BorderLeft(2).BorderColor(_theme.Accent)
            .Background(_theme.Label).PaddingVertical(4).PaddingHorizontal(7)
            .Text(label).FontSize(8).Bold().FontColor(_theme.Muted);

        table.Cell().BorderBottom(0.6f).BorderColor(_theme.Border)
            .PaddingVertical(3).PaddingHorizontal(7)
            .Element(c => Pill(c, value));
    }

    // Row with a custom pill text (e.g. Uploaded / Not Uploaded)
    private void AddYesNoRow(TableDescriptor table, string label, bool value, string yes, string no)
    {
        table.Cell().BorderBottom(0.6f).BorderColor(_theme.Border)
            .BorderLeft(2).BorderColor(_theme.Accent)
            .Background(_theme.Label).PaddingVertical(4).PaddingHorizontal(7)
            .Text(label).FontSize(8).Bold().FontColor(_theme.Muted);

        table.Cell().BorderBottom(0.6f).BorderColor(_theme.Border)
            .PaddingVertical(3).PaddingHorizontal(7)
            .Element(c => Pill(c, value, yes, no));
    }

    private void AddLabRow(TableDescriptor table, string label, bool available, bool shared)
    {
        table.Cell().Element(Td).Text(label);
        table.Cell().Element(Td).Element(c => Pill(c, available));
        table.Cell().Element(Td).Element(c => Pill(c, shared));
    }

    // Header cell: solid brand colour, white bold text
    private IContainer Th(IContainer c) => c
        .Background(_theme.Primary)
        .BorderBottom(1.5f).BorderColor(_theme.Accent)
        .PaddingVertical(5).PaddingHorizontal(6)
        .DefaultTextStyle(t => t.FontSize(8).Bold().FontColor(Colors.White))
        .AlignMiddle();

    // Body cell: hairline bottom rule only
    private IContainer Td(IContainer c) => c
        .BorderBottom(0.6f).BorderColor(_theme.Border)
        .PaddingVertical(4).PaddingHorizontal(6)
        .DefaultTextStyle(t => t.FontSize(8.5f).FontColor(_theme.Ink))
        .AlignMiddle();

    // Group divider row inside a table (e.g. faculty departments)
    private IContainer GroupRow(IContainer c) => c
        .Background(_theme.Light)
        .BorderLeft(3).BorderColor(_theme.Accent)
        .PaddingVertical(4).PaddingHorizontal(8)
        .DefaultTextStyle(t => t.FontSize(8.5f).Bold().FontColor(_theme.Primary));

    // Green / red status pill
    private void Pill(IContainer c, bool? value, string yes = "Yes", string no = "No")
    {
        if (value == null)
        {
            c.AlignCenter().Text("—").FontColor(_theme.Muted);
            return;
        }

        var ok = value.Value;
        c.AlignCenter().AlignMiddle()
            .Background(ok ? _theme.GoodBg : _theme.BadBg)
            .CornerRadius(8).PaddingVertical(1.5f).PaddingHorizontal(8)
            .Text(ok ? yes : no).FontSize(7.5f).Bold().FontColor(ok ? _theme.Good : _theme.Bad);
    }

    // =====================================================================
    //  THEME (one palette per application type)
    // =====================================================================
    private sealed class PdfTheme
    {
        public string Primary { get; }
        public string Secondary { get; }
        public string Accent { get; }
        public string Light { get; }
        public string Border { get; }
        public string Label { get; }
        public string TypeLabel { get; }

        public string Muted { get; } = "#64748B";
        public string Ink { get; } = "#1E293B";
        public string Good { get; } = "#047857";
        public string GoodBg { get; } = "#D1FAE5";
        public string Bad { get; } = "#B91C1C";
        public string BadBg { get; } = "#FEE2E2";

        private PdfTheme(string primary, string secondary, string accent, string light, string border, string label, string typeLabel)
        {
            Primary = primary;
            Secondary = secondary;
            Accent = accent;
            Light = light;
            Border = border;
            Label = label;
            TypeLabel = typeLabel;
        }

        public static PdfTheme ForApplicationType(string? applicationType)
        {
            var v = (applicationType ?? string.Empty).Trim().ToLowerInvariant();

            if (v.Contains("continu"))   // deep navy + gold
                return new PdfTheme("#12304F", "#1D4E89", "#C9A24B", "#F6F4EE", "#DDD6C4", "#F7F5EF", "CONTINUATION");

            if (v.Contains("renew"))     // burnt orange
                return new PdfTheme("#7C2D12", "#C2410C", "#F59E0B", "#FFF7ED", "#FED7AA", "#FFF9F2", "RENEWAL");

            if (v.Contains("increase") || v.Contains("intake") || v.Contains("enhance"))   // royal blue + amber
                return new PdfTheme("#0B3A75", "#1D63C4", "#E0A82E", "#EEF4FC", "#CBDAEE", "#F4F8FD", "INTAKE ENHANCEMENT");

            if (v.Contains("first") || v.Contains("new") || v.Contains("initial"))         // indigo + cyan
                return new PdfTheme("#1E2F7A", "#3B5BDB", "#06B6D4", "#EEF2FF", "#C7D2FE", "#F5F7FF", "NEW AFFILIATION");

            return new PdfTheme("#123A63", "#1F6F6B", "#C9A24B", "#F4F7FA", "#D8E2EC", "#F7F9FB", "AFFILIATION");
        }
    }
}