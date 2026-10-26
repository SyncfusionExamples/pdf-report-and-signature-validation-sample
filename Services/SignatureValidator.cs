using System.Security.Cryptography.X509Certificates;
using Syncfusion.Pdf.Parsing;
using Syncfusion.Pdf.Security;
using PdfReportAndSignatureValidationSample.Models;

namespace PdfReportAndSignatureValidationSample.Services;

public class SignatureValidator
{
    public SignatureValidationResponse Validate(Stream pdf, string fileName, X509Certificate2 rootCertificate)
    {
        var response = new SignatureValidationResponse { FileName = fileName };

        using var document = new PdfLoadedDocument(pdf);
        var signatureFields = new List<PdfLoadedSignatureField>();
        if (document.Form != null)
        {
            foreach (PdfLoadedField field in document.Form.Fields)
                if (field is PdfLoadedSignatureField sf && sf.IsSigned)
                    signatureFields.Add(sf);
        }

        response.SignatureCount = signatureFields.Count;
        if (signatureFields.Count == 0)
        {
            response.Message = "No digital signatures were found in this PDF.";
            return response;
        }

        foreach (var field in signatureFields)
            response.Signatures.Add(Describe(field, rootCertificate));

        return response;
    }

    private static SignatureInfo Describe(PdfLoadedSignatureField field, X509Certificate2 rootCertificate)
    {
        var info = new SignatureInfo { FieldName = field.Name ?? "" };
        try
        {
            // Revocation (OCSP/CRL) needs outbound network access; the POC keeps it off for self-signed demo files.
            var options = new PdfSignatureValidationOptions { ValidateRevocationStatus = false };
            PdfSignatureValidationResult result = field.ValidateSignature(new X509CertificateCollection { rootCertificate }, options);

            info.SignerName = result.SignatureName ?? "";
            info.Status = result.SignatureStatus.ToString();
            info.IsSignatureValid = result.IsSignatureValid;
            info.IsDocumentModified = result.IsDocumentModified;
            info.DigestAlgorithm = result.DigestAlgorithm.ToString();
            info.CryptographicStandard = result.CryptographicStandard.ToString();
            info.HasTimeStamp = result.TimeStampInformation != null;
            info.IsLtvEnabled = result.LtvVerificationInfo?.IsLtvEnabled ?? false;
            
            var cert = result.SignerCertificates?.FirstOrDefault()?.Certificate;
            if (cert != null)
            {
                info.CertificateSubject = cert.Subject;
                info.CertificateIssuer = cert.Issuer;
            }
            info.SignedAt = field.Signature?.SignedDate.ToString("yyyy-MM-dd HH:mm:ss");
        }
        catch (Exception ex)
        {
            info.Status = "Error";
            info.Errors.Add(ex.Message);
        }
        return info;
    }
}
