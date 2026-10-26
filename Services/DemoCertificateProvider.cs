using System.Security.Cryptography.X509Certificates;

namespace PdfReportAndSignatureValidationSample.Services;

/// <summary>
/// Loads the demo signing certificate from wwwroot/PDF.pfx and extracts its root certificate,
/// which is later passed to the signature validation.
/// In production the signing certificate would be loaded from Azure Key Vault instead.
/// </summary>
public sealed class DemoCertificateProvider
{
    public X509Certificate2 Certificate { get; }

    /// <summary>Public part of the root certificate of the signing certificate's chain.</summary>
    public X509Certificate2 RootCertificate { get; }

    public DemoCertificateProvider(IWebHostEnvironment environment, IConfiguration configuration)
    {
        var path = Path.Combine(environment.WebRootPath, "PDF.pfx");
        Certificate = new X509Certificate2(path, configuration["Signing:PfxPassword"],
            X509KeyStorageFlags.Exportable | X509KeyStorageFlags.EphemeralKeySet);
        RootCertificate = ExtractRoot(Certificate);
    }

    /// <summary>Walks the chain to its last element. For a self-signed certificate that is the certificate itself.</summary>
    private static X509Certificate2 ExtractRoot(X509Certificate2 certificate)
    {
        using var chain = new X509Chain();
        chain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
        chain.ChainPolicy.VerificationFlags = X509VerificationFlags.AllowUnknownCertificateAuthority;
        chain.Build(certificate);

        return new X509Certificate2(chain.ChainElements[^1].Certificate.RawData); // public key only
    }
}
