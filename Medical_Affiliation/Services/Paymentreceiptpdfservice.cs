using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Medical_Affiliation.Services
{
    public class PaymentReceiptCourseLine
    {
        public string CourseName { get; set; } = string.Empty;
        public string CourseLevel { get; set; } = string.Empty;
        public int Intake { get; set; }
    }

    public class PaymentReceiptFeeLine
    {
        public string FeeHead { get; set; } = string.Empty;
        public decimal UnitAmount { get; set; }
        public string Basis { get; set; } = string.Empty;
        public int Multiplier { get; set; }
        public decimal LineAmount { get; set; }
    }

    public class PaymentReceiptData
    {
        public string CollegeCode { get; set; } = string.Empty;
        public string? CollegeName { get; set; }
        public string CourseLevel { get; set; } = string.Empty;
        public string? AffiliationTypeName { get; set; }

        public List<PaymentReceiptCourseLine> MatchedCourses { get; set; } = new();
        public List<PaymentReceiptFeeLine> FeeLines { get; set; } = new();
        public decimal GrandTotal { get; set; }

        public string TransactionId { get; set; } = string.Empty;
        public decimal? PaymentAmount { get; set; }
        public DateTime? PaymentDate { get; set; }
    }

    public interface IPaymentReceiptPdfService
    {
        string GenerateReceipt(PaymentReceiptData data, string outputFolder);
    }

    public class PaymentReceiptPdfService : IPaymentReceiptPdfService
    {
        public string GenerateReceipt(PaymentReceiptData data, string outputFolder)
        {
            if (!Directory.Exists(outputFolder))
                Directory.CreateDirectory(outputFolder);

            string fileName = $"{Guid.NewGuid()}.pdf";
            string fullPath = Path.Combine(outputFolder, fileName);

            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(30);
                    page.Size(PageSizes.A4);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header().Text($"Payment Receipt — {data.CollegeName ?? data.CollegeCode} ({data.CollegeCode})")
                        .FontSize(16).Bold();

                    page.Content().Column(col =>
                    {
                        col.Item().Text($"Course Level: {data.CourseLevel}    Affiliation Type: {data.AffiliationTypeName}");

                        col.Item().PaddingTop(10).Text("Matched Courses").Bold();
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(c =>
                            {
                                c.RelativeColumn(3);
                                c.RelativeColumn(2);
                                c.RelativeColumn(1);
                            });

                            table.Header(h =>
                            {
                                h.Cell().Text("Course Name").Bold();
                                h.Cell().Text("Course Level").Bold();
                                h.Cell().Text("Intake").Bold();
                            });

                            foreach (var c in data.MatchedCourses)
                            {
                                table.Cell().Text(c.CourseName);
                                table.Cell().Text(c.CourseLevel);
                                table.Cell().Text(c.Intake.ToString());
                            }
                        });

                        col.Item().PaddingTop(10).Text("Fee Breakdown").Bold();
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(c =>
                            {
                                c.RelativeColumn(3);
                                c.RelativeColumn(2);
                                c.RelativeColumn(2);
                                c.RelativeColumn(1);
                                c.RelativeColumn(2);
                            });

                            table.Header(h =>
                            {
                                h.Cell().Text("Fee Head").Bold();
                                h.Cell().Text("Unit Amount").Bold();
                                h.Cell().Text("Basis").Bold();
                                h.Cell().Text("x").Bold();
                                h.Cell().Text("Line Amount").Bold();
                            });

                            foreach (var f in data.FeeLines)
                            {
                                table.Cell().Text(f.FeeHead);
                                table.Cell().Text(f.UnitAmount.ToString("N2"));
                                table.Cell().Text(f.Basis);
                                table.Cell().Text(f.Multiplier.ToString());
                                table.Cell().Text(f.LineAmount.ToString("N2"));
                            }
                        });

                        col.Item().PaddingTop(5).AlignRight()
                            .Text($"Grand Total: {data.GrandTotal:N2}").Bold().FontSize(12);

                        col.Item().PaddingTop(15).Text("Payment Details").Bold();
                        col.Item().Text($"Transaction ID: {data.TransactionId}");
                        if (data.PaymentAmount.HasValue)
                            col.Item().Text($"Amount Paid: {data.PaymentAmount:N2}");
                        if (data.PaymentDate.HasValue)
                            col.Item().Text($"Payment Date: {data.PaymentDate:dd-MMM-yyyy}");
                    });

                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("Generated on ");
                        x.Span(DateTime.Now.ToString("dd-MMM-yyyy HH:mm"));
                    });
                });
            })
            .GeneratePdf(fullPath);

            return fullPath;
        }
    }
}