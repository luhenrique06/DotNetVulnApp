using DotNetVulnApp.Models;
using DotNetVulnApp.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;

namespace DotNetVulnApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StatementsController : ControllerBase
    {
        // A08/A01 - grava arquivo com nome controlado pelo cliente (path traversal +
        // conteúdo bruto, sem validação de integridade).
        [HttpPost]
        [Route("upload")]
        [Authorize]
        public IActionResult Upload([FromBody] Docs doc)
        {
            string basePath = Directory.GetCurrentDirectory();

            if (doc.Content == null || doc.FileName == null)
                return BadRequest("Missing mandatory parameters");

            // Sem sanitização: "../../etc/algo" escapa o diretório.
            doc.FileName = basePath + "/" + doc.FileName;

            try
            {
                OptimisedIO.saveFileRaw(doc.FileName, doc.Content);
                return Ok(new { message = "Doc was saved" });
            }
            catch (Exception)
            {
                return BadRequest("Error while save doc");
            }
        }

        // A05 - Fixed: Injection (command injection) + path traversal mitigated by:
        //   1. Validating 'doc' against an allowlist (alphanumeric + hyphen/underscore only)
        //      and ensuring the resolved path stays within the base directory.
        //   2. Validating 'size' against a strict allowlist of known paper sizes.
        //   3. Passing arguments directly to each process without shell interpolation
        //      instead of using /bin/bash -c with a constructed command string.
        [HttpGet]
        [Route("export/{doc}")]
        public IActionResult Export(string doc)
        {
            // Validate 'doc': only allow alphanumeric characters, hyphens, and underscores.
            if (string.IsNullOrWhiteSpace(doc) || !Regex.IsMatch(doc, @"^[a-zA-Z0-9_\-]+$"))
                return BadRequest("Invalid document name.");

            string basePath = Directory.GetCurrentDirectory();
            string sourcePath = System.IO.Path.GetFullPath(System.IO.Path.Combine(basePath, doc + ".txt"));

            // Ensure the resolved path is within the base directory (prevent path traversal).
            if (!sourcePath.StartsWith(basePath + System.IO.Path.DirectorySeparatorChar) &&
                !sourcePath.Equals(basePath))
                return BadRequest("Invalid document name.");

            if (!System.IO.File.Exists(sourcePath))
                return BadRequest("Doc não existe");

            // Validate 'size' against a strict allowlist of known paper sizes.
            string requestedSize = Request.Query["size"];
            var allowedPaperSizes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "a4", "a3", "a5", "letter", "legal", "tabloid", "ledger",
                "b4", "b5", "executive", "folio", "quarto"
            };

            if (string.IsNullOrWhiteSpace(requestedSize) || !allowedPaperSizes.Contains(requestedSize))
                return BadRequest("Invalid or unsupported paper size.");

            string papersize = requestedSize.ToLowerInvariant();
            string pdfPath = System.IO.Path.Combine(basePath, "statement.pdf");

            // Step 1: Run enscript to convert the text file to PostScript, writing to a temp file.
            string tempPsPath = System.IO.Path.Combine(basePath, System.IO.Path.GetRandomFileName() + ".ps");
            try
            {
                var enscriptInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "enscript",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardError = true
                };
                // Pass arguments individually to avoid shell injection.
                enscriptInfo.ArgumentList.Add(sourcePath);
                enscriptInfo.ArgumentList.Add("-o");
                enscriptInfo.ArgumentList.Add(tempPsPath);

                using (var enscriptProc = new System.Diagnostics.Process { StartInfo = enscriptInfo })
                {
                    enscriptProc.Start();
                    enscriptProc.WaitForExit();
                    if (enscriptProc.ExitCode != 0)
                        return StatusCode(500, "Error during document conversion (enscript).");
                }

                if (!System.IO.File.Exists(tempPsPath))
                    return StatusCode(500, "Error during document conversion (enscript output missing).");

                // Step 2: Run ps2pdf to convert PostScript to PDF.
                var ps2pdfInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "ps2pdf",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardError = true
                };
                ps2pdfInfo.ArgumentList.Add("-dFIXEDMEDIA");
                ps2pdfInfo.ArgumentList.Add($"-sPAPERSIZE={papersize}");
                ps2pdfInfo.ArgumentList.Add(tempPsPath);
                ps2pdfInfo.ArgumentList.Add(pdfPath);

                using (var ps2pdfProc = new System.Diagnostics.Process { StartInfo = ps2pdfInfo })
                {
                    ps2pdfProc.Start();
                    ps2pdfProc.WaitForExit();
                    if (ps2pdfProc.ExitCode != 0)
                        return StatusCode(500, "Error during document conversion (ps2pdf).");
                }

                return PhysicalFile(pdfPath, "application/pdf");
            }
            finally
            {
                // Clean up the temporary PostScript file.
                if (System.IO.File.Exists(tempPsPath))
                    System.IO.File.Delete(tempPsPath);
            }
        }
    }
}
