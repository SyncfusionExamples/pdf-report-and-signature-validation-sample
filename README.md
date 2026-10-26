# PDF Report Creation and Digital Signature Validation Using Syncfusion .NET PDF Library

An ASP.NET Core sample that uses the [Syncfusion .NET PDF Library](https://www.syncfusion.com/document-sdk/net-pdf-library/) to generate a PDF report, digitally sign it, and validate digital signatures on PDF files. It includes a web UI and a REST API.

## Pages

- **Home (`/`)** – Two tabs: *Generate report* and *Validate signature*.
- **Generate report** – Enter a Policy ID and Name, optionally sign the document with the demo certificate, preview the generated PDF in the page and download it.
- **Validate signature** – Upload a signed PDF (or validate the one you just generated) and view the signer certificate and signature details.
- **Swagger (`/swagger`)** – Interactive API documentation.

## API endpoints

| Route | Description |
|---|---|
| `POST /api/pdf/createreport` | Body `{ "policyId", "name", "sign" }`. Returns the PDF (`application/pdf`). Only Policy ID and Name are input; the rest of the content is static sample data. |
| `POST /api/pdf/validatesignature` | `multipart/form-data` with a `file` field. Returns JSON with one entry per signature. |

## Features

- Multi-page PDF report with header, grid table, totals and a "Page X of Y" footer
- Digital signature (CMS, SHA-256) using a certificate loaded from a PFX file
- Signature validation: cryptographic validity, document modification check, digest algorithm, timestamp and LTV status
- Certificate chain check against the root certificate extracted from the signing certificate
- In-browser PDF preview and download

## Project structure

| Path | Purpose |
|---|---|
| `Program.cs` | Service registration, license registration and the two API endpoints |
| `Services/PdfBuilder.cs` | Builds and signs the PDF report |
| `Services/SignatureValidator.cs` | Validates signatures and maps the result to JSON |
| `Services/DemoCertificateProvider.cs` | Loads the PFX and extracts its root certificate |
| `Services/StaticReportData.cs` | Static sample content for the report |
| `Models/Dtos.cs` | Request and response models |
| `wwwroot/` | Web UI (`index.html`, `app.js`, `styles.css`) and `PDF.pfx` |

## Running the sample

1. Open a terminal in the project folder.
2. Restore and run:

   ```bash
   dotnet restore
   dotnet run --urls http://localhost:5100
   ```

3. Open <http://localhost:5100> for the UI or <http://localhost:5100/swagger> for the API.

## Syncfusion license

A license key is required, otherwise the generated PDFs carry a trial banner. Provide the key in one of these ways:

- **Environment variable:** set `SYNCFUSION_LICENSE_KEY` to your key.
- **appsettings.json:** set `Syncfusion:LicenseKey`.

  ```json
  "Syncfusion": { "LicenseKey": "<your key>" }
  ```

- **Code:** call `SyncfusionLicenseProvider.RegisterLicense("<your key>");` in `Program.cs`, before any PDF is created.

`Program.cs` reads the environment variable first, then the `appsettings.json` value, and registers whichever it finds. A free community license is available at [https://www.syncfusion.com/products/communitylicense](https://www.syncfusion.com/products/communitylicense).

## Sample files

The signing certificate is `wwwroot/PDF.pfx` (password `password123`, configured in `appsettings.json` under `Signing:PfxPassword`). It is a self-signed root certificate, so the root extracted for validation is the certificate itself. Requests for `.pfx` files are blocked so the private key is never served to browsers.
