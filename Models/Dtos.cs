namespace PdfReportAndSignatureValidationSample.Models;

/// <summary>Only these two values come from the UI; everything else is static sample data.</summary>
public record ReportRequest(string PolicyId, string Name, bool Sign = false);

public class SignatureValidationResponse
{
    public string FileName { get; set; } = "";
    public int SignatureCount { get; set; }
    public List<SignatureInfo> Signatures { get; set; } = new();
    public string? Message { get; set; }
}

public class SignatureInfo
{
    public string FieldName { get; set; } = "";
    public string SignerName { get; set; } = "";
    public string? CertificateSubject { get; set; }
    public string? CertificateIssuer { get; set; }
    public string? SignedAt { get; set; }
    public string Status { get; set; } = "";
    public bool IsSignatureValid { get; set; }
    public bool IsDocumentModified { get; set; }
    public bool IsChainTrusted { get; set; }
    public string DigestAlgorithm { get; set; } = "";
    public string CryptographicStandard { get; set; } = "";
    public bool HasTimeStamp { get; set; }
    public bool IsLtvEnabled { get; set; }
    public string RevocationStatus { get; set; } = "Not checked";
    public List<string> Errors { get; set; } = new();
}
