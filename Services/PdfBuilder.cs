using Syncfusion.Drawing;
using System.Globalization;
using Syncfusion.Pdf;
using Syncfusion.Pdf.Graphics;
using Syncfusion.Pdf.Grid;
using Syncfusion.Pdf.Parsing;
using Syncfusion.Pdf.Security;
using static PdfReportAndSignatureValidationSample.Services.StaticReportData;

namespace PdfReportAndSignatureValidationSample.Services;

public class PdfBuilder
{
    private static readonly CultureInfo EnUs = new("en-US");
    private static readonly PdfColor Brand = new(0, 82, 147);
    private static readonly PdfColor LightGray = new(242, 244, 247);

    private readonly DemoCertificateProvider _certificates;

    public PdfBuilder(DemoCertificateProvider certificates) => _certificates = certificates;

    public MemoryStream Build(string policyId, string name, bool sign)
    {
        var title = new PdfStandardFont(PdfFontFamily.Helvetica, 22, PdfFontStyle.Bold);
        var h2 = new PdfStandardFont(PdfFontFamily.Helvetica, 13, PdfFontStyle.Bold);
        var body = new PdfStandardFont(PdfFontFamily.Helvetica, 10);
        var bold = new PdfStandardFont(PdfFontFamily.Helvetica, 10, PdfFontStyle.Bold);
        var small = new PdfStandardFont(PdfFontFamily.Helvetica, 8);

        using var document = new PdfDocument();
        document.PageSettings.Margins.All = 40;
        document.PageSettings.Size = PdfPageSize.A4;
        document.DocumentInformation.Title = $"Report {policyId}";
        document.DocumentInformation.Author = IssuerName;

        AddFooter(document, small);

        var page = document.Pages.Add();
        var g = page.Graphics;
        float width = page.GetClientSize().Width;

        // Header band with a drawn logo (no image asset needed)
        g.DrawRectangle(new PdfSolidBrush(Brand), new RectangleF(0, 0, width, 70));
        g.DrawEllipse(new PdfSolidBrush(new PdfColor(255, 255, 255)), new RectangleF(14, 15, 40, 40));
        g.DrawString("SH", bold, new PdfSolidBrush(Brand), new RectangleF(14, 29, 40, 14),
            new PdfStringFormat(PdfTextAlignment.Center));
        g.DrawString("PROPOSTA DE SEGURO", title, PdfBrushes.White, new PointF(70, 12));
        g.DrawString(IssuerName, body, PdfBrushes.White, new PointF(70, 42));

        float y = 90;
        y = DrawKeyValues(g, h2, bold, body, "Report Details", y, width, new[]
        {
            ("Policy ID", policyId),
            ("Contact name", name),
            ("Issue date", DateTime.Now.ToString("dd MMM yyyy", EnUs)),
            ("Policy period", $"{ValidFrom.ToString("dd MMM yyyy", EnUs)} to {ValidTo.ToString("dd MMM yyyy", EnUs)}"),
            ("Product", ProductName),
            ("Insurer", InsurerName),
        });
        y = DrawKeyValues(g, h2, bold, body, "Corporate Client", y + 8, width, new[]
        {
            ("Company", ClientCompany),
            ("Registration no.", ClientRegistration),
            ("Address", ClientAddress),
        });
        y = DrawKeyValues(g, h2, bold, body, "Issued By", y + 8, width, new[]
        {
            ("Organization", IssuerName),
            ("Registration no.", IssuerRegistration),
            ("Address", IssuerAddress),
            ("Contact", IssuerContact),
        });

        g.DrawString("Coverages and Premiums", h2, new PdfSolidBrush(Brand), new PointF(0, y + 12));
        var grid = BuildCoverageGrid(body, bold);
        var result = grid.Draw(page, new PointF(0, y + 32), new PdfGridLayoutFormat { Layout = PdfLayoutType.Paginate });

        // Totals + terms may land on a following page
        var last = result.Page;
        float ty = result.Bounds.Bottom + 12;
        if (ty > last.GetClientSize().Height - 260)
        {
            last = document.Pages.Add();
            ty = 0;
        }
        var lg = last.Graphics;
        decimal total = Coverages.Sum(c => c.Premium);
        lg.DrawRectangle(new PdfSolidBrush(LightGray), new RectangleF(0, ty, width, 28));
        lg.DrawString("Total annual premium", bold, PdfBrushes.Black, new PointF(8, ty + 8));
        lg.DrawString(total.ToString("C", EnUs), h2, new PdfSolidBrush(Brand),
            new RectangleF(0, ty + 7, width - 8, 16), new PdfStringFormat(PdfTextAlignment.Right));

        ty += 44;
        lg.DrawString("General Conditions", h2, new PdfSolidBrush(Brand), new PointF(0, ty));
        ty += 20;
        foreach (var term in Terms)
        {
            var layout = new PdfStringFormat { LineSpacing = 2 };
            var rect = new RectangleF(10, ty, width - 10, 40);
            lg.DrawString("•", body, PdfBrushes.Black, new PointF(0, ty));
            lg.DrawString(term, body, PdfBrushes.Black, rect, layout);
            ty += body.MeasureString(term, width - 10).Height + 4;
        }

        if (sign)
        {
            ty += 20;
            lg.DrawString("Digital signature", bold, PdfBrushes.Black, new PointF(0, ty));
            var cert = new PdfCertificate(_certificates.Certificate);
            var signature = new PdfSignature(last, cert, "Signature")
            {
                Bounds = new RectangleF(0, ty + 16, 280, 70),
                ContactInfo = "info@example.com",
                LocationInfo = "Sample City",
                Reason = $"Issuance of report {policyId}",
            };
            signature.Settings.CryptographicStandard = CryptographicStandard.CMS;
            signature.Settings.DigestAlgorithm = DigestAlgorithm.SHA256;
        }

        var output = new MemoryStream();
        document.Save(output);
        output.Position = 0;
        return output;
    }

    private static float DrawKeyValues(PdfGraphics g, PdfFont h2, PdfFont bold, PdfFont body,
        string heading, float y, float width, (string Key, string Value)[] rows)
    {
        g.DrawString(heading, h2, new PdfSolidBrush(Brand), new PointF(0, y));
        y += 18;
        g.DrawLine(new PdfPen(Brand, 0.8f), 0, y, width, y);
        y += 5;
        foreach (var (key, value) in rows)
        {
            g.DrawString(key, bold, PdfBrushes.Black, new PointF(0, y));
            var rect = new RectangleF(150, y, width - 150, 30);
            g.DrawString(value, body, PdfBrushes.Black, rect);
            y += Math.Max(14, body.MeasureString(value, width - 150).Height + 3);
        }
        return y;
    }

    private static PdfGrid BuildCoverageGrid(PdfFont body, PdfFont bold)
    {
        var grid = new PdfGrid();
        grid.Columns.Add(4);
        grid.Columns[0].Width = 130;
        grid.Columns[1].Width = 200;
        grid.Columns[2].Width = 95;
        grid.Columns[3].Width = 90;
        grid.Style.CellPadding = new PdfPaddings(4, 4, 4, 4);
        grid.Style.Font = body;

        var header = grid.Headers.Add(1)[0];
        string[] titles = { "Coverage", "Description", "Sum Insured", "Premium" };
        for (int i = 0; i < titles.Length; i++)
        {
            header.Cells[i].Value = titles[i];
            header.Cells[i].Style.BackgroundBrush = new PdfSolidBrush(Brand);
            header.Cells[i].Style.TextBrush = PdfBrushes.White;
            header.Cells[i].Style.Font = bold;
            if (i >= 2) header.Cells[i].StringFormat = new PdfStringFormat(PdfTextAlignment.Right);
        }

        int n = 0;
        foreach (var c in Coverages)
        {
            var row = grid.Rows.Add();
            row.Cells[0].Value = c.Name;
            row.Cells[0].Style.Font = bold;
            row.Cells[1].Value = c.Description;
            row.Cells[2].Value = c.InsuredAmount.ToString("C", EnUs);
            row.Cells[3].Value = c.Premium.ToString("C", EnUs);
            row.Cells[2].StringFormat = new PdfStringFormat(PdfTextAlignment.Right);
            row.Cells[3].StringFormat = new PdfStringFormat(PdfTextAlignment.Right);
            if (n++ % 2 == 1)
                foreach (PdfGridCell cell in row.Cells) cell.Style.BackgroundBrush = new PdfSolidBrush(LightGray);
        }
        return grid;
    }

    private static void AddFooter(PdfDocument document, PdfFont font)
    {
        var footer = new PdfPageTemplateElement(document.PageSettings.Width, 30);
        var brush = new PdfSolidBrush(new PdfColor(90, 90, 90));
        float w = document.PageSettings.Width - 80;
        footer.Graphics.DrawLine(new PdfPen(Brand, 0.5f), 40, 4, 40 + w, 4);
        footer.Graphics.DrawString(IssuerName + "  |  Reg. " + IssuerRegistration, font, brush, new PointF(40, 9));

        var pageNumber = new PdfPageNumberField(font, brush);
        var pageCount = new PdfPageCountField(font, brush);
        var composite = new PdfCompositeField(font, brush, "Page {0} of {1}", pageNumber, pageCount)
        {
            StringFormat = new PdfStringFormat(PdfTextAlignment.Right),
            Bounds = new RectangleF(40, 9, w, 12),
        };
        composite.Draw(footer.Graphics);
        document.Template.Bottom = footer;
    }
}
