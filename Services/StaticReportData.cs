namespace PdfReportAndSignatureValidationSample.Services;

/// <summary>
/// Fixed content for the sample report. In the real CRM this would come from the database;
/// the POC only takes Policy ID and Name from the UI.
/// </summary>
public static class StaticReportData
{
    public const string IssuerName = "Sample Organization Ltd.";
    public const string IssuerRegistration = "REG-000000";
    public const string IssuerAddress = "123 Sample Street, Sample City";
    public const string IssuerContact = "info@example.com  |  +00 000 000 0000";

    public const string ClientCompany = "Sample Client Corp.";
    public const string ClientRegistration = "TAX-000000";
    public const string ClientAddress = "456 Example Avenue, Sample City";
    public const string ProductName = "Commercial Property & Liability Insurance";
    public const string InsurerName = "Sample Insurer Co.";

    public static readonly DateTime ValidFrom = new(2026, 11, 1);
    public static readonly DateTime ValidTo = new(2027, 10, 31);

    public record Coverage(string Name, string Description, decimal InsuredAmount, decimal Premium);

    public static readonly IReadOnlyList<Coverage> Coverages = new List<Coverage>
    {
        new("Fire, Lightning & Explosion", "Damage to buildings and contents caused by fire, lightning or explosion.", 5_000_000m, 18_450.00m),
        new("Electrical Damage", "Short circuit, power surge and voltage variation damage to equipment.", 800_000m, 4_120.50m),
        new("Theft & Burglary", "Loss of property from the premises following forced entry or violence.", 600_000m, 5_980.75m),
        new("Public Liability", "Bodily injury and property damage to third parties arising from business operations.", 1_500_000m, 7_300.00m),
        new("Business Interruption", "Loss of gross revenue during downtime following a covered loss.", 2_000_000m, 9_870.40m),
        new("24/7 Emergency Assistance", "Locksmith, security guard, emergency repairs and debris removal.", 50_000m, 1_250.00m),
    };

    public static readonly IReadOnlyList<string> Terms = new List<string>
    {
        "This report is valid for 30 days from the date of issue.",
        "A deductible of 10% of each insured loss applies, with a minimum of $5,000.00 per claim.",
        "Payment terms: 6 interest-free monthly installments by corporate card or invoice.",
        "Acceptance is subject to underwriting review and a prior risk survey by the insurer.",
        "This document was generated automatically for demonstration purposes (POC).",
    };
}
