using PdfReportAndSignatureValidationSample.Models;
using PdfReportAndSignatureValidationSample.Services;
using Syncfusion.Licensing;

var builder = WebApplication.CreateBuilder(args);

// Register the Syncfusion license: SYNCFUSION_LICENSE_KEY environment variable or Syncfusion:LicenseKey in appsettings.json.
var licenseKey = Environment.GetEnvironmentVariable("SYNCFUSION_LICENSE_KEY")
                 ?? builder.Configuration["Syncfusion:LicenseKey"];
if (!string.IsNullOrWhiteSpace(licenseKey))
    SyncfusionLicenseProvider.RegisterLicense(licenseKey);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<DemoCertificateProvider>();
builder.Services.AddSingleton<PdfBuilder>();
builder.Services.AddSingleton<SignatureValidator>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
// The signing certificate lives in wwwroot; never serve it to browsers.
app.Use((context, next) =>
{
    if (context.Request.Path.Value?.EndsWith(".pfx", StringComparison.OrdinalIgnoreCase) == true)
    {
        context.Response.StatusCode = StatusCodes.Status404NotFound;
        return Task.CompletedTask;
    }
    return next();
});
app.UseDefaultFiles();
app.UseStaticFiles();

var pdf = app.MapGroup("/api/pdf").WithTags("PDF");

pdf.MapPost("/createreport", (ReportRequest request, PdfBuilder builder) =>
    {
        if (string.IsNullOrWhiteSpace(request.PolicyId) || string.IsNullOrWhiteSpace(request.Name))
            return Results.BadRequest("Policy ID and Name are required.");

        var stream = builder.Build(request.PolicyId.Trim(), request.Name.Trim(), request.Sign);
        var safeId = string.Concat(request.PolicyId.Trim().Where(c => char.IsLetterOrDigit(c) || c is '-' or '_'));
        return Results.File(stream, "application/pdf", $"report-{safeId}.pdf");
    })
    .Produces<FileResult>(200, "application/pdf")
    .WithSummary("Generate the PDF report for a Policy ID and Name.");

pdf.MapPost("/validatesignature", async (IFormFile file, SignatureValidator validator, DemoCertificateProvider certificates) =>
    {
        if (file.Length == 0) return Results.BadRequest("Empty file.");
        using var ms = new MemoryStream();
        await file.CopyToAsync(ms);
        ms.Position = 0;
        try
        {
            return Results.Ok(validator.Validate(ms, file.FileName, certificates.RootCertificate));
        }
        catch (Exception ex)
        {
            return Results.BadRequest($"Could not read the PDF: {ex.Message}");
        }
    })
    .DisableAntiforgery()
    .WithSummary("Validate the digital signatures in an uploaded PDF.");

app.Run();

// Swagger helper type for the file response
internal sealed class FileResult { }
