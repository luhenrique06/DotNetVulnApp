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

        // A05 - Injection (command injection) + path traversal:
        //   'doc' e 'size' entram direto numa linha de shell (enscript | ps2pdf).
        //   Ex.: size=a4 -o /dev/null; id  ->  execução de comando.
        //   doc=../../logs/Access ->  leitura de arquivo fora do diretório (A09: log com senhas).
        [HttpGet]
        [Route("export/{doc}")]
        public IActionResult Export(string doc)
        {
            // Validate and sanitize the 'doc' parameter to prevent path traversal.
            // Only allow alphanumeric characters, hyphens, and underscores.
            if (string.IsNullOrEmpty(doc) || !Regex.IsMatch(doc, @"^[a-zA-Z0-9_\-]+$"))
                return BadRequest("Invalid document name");

            // Validate and sanitize the 'size' parameter to prevent command injection.
            // Only allow known safe paper size values.
            string papersize = Request.Query["size"];
            if (string.IsNullOrEmpty(papersize) || !Regex.IsMatch(papersize, @"^[a-zA-Z0-9]+$"))
                return BadRequest("Invalid paper size");

            // Restrict allowed paper sizes to a known safe whitelist.
            var allowedPaperSizes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "a4", "a3", "a5", "letter", "legal", "ledger", "tabloid"
            };

            if (!allowedPaperSizes.Contains(papersize))
                return BadRequest("Unsupported paper size");

            // Build the source path using only the sanitized doc name, confined to current directory.
            string baseDir = Directory.GetCurrentDirectory();
            string sourcePath = System.IO.Path.Combine(baseDir, doc + ".txt");

            // Ensure the resolved path is within the base directory (prevent path traversal).
            string fullSourcePath = System.IO.Path.GetFullPath(sourcePath);
            if (!fullSourcePath.StartsWith(baseDir + System.IO.Path.DirectorySeparatorChar) &&
                fullSourcePath != baseDir)
                return BadRequest("Invalid document path");

            if (!System.IO.File.Exists(fullSourcePath))
                return BadRequest("Doc não existe");

            string pdfPath = System.IO.Path.Combine(baseDir, "statement.pdf");

            // Pass arguments as separate items to avoid shell injection.
            // Use ProcessStartInfo with explicit arguments rather than shell interpolation.
            var procStartInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "/bin/bash",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardError = true
            };

            // Build the command with sanitized, whitelisted values only.
            procStartInfo.ArgumentList.Add("-c");
            procStartInfo.ArgumentList.Add(
                $"enscript {fullSourcePath} -o - | ps2pdf -dFIXEDMEDIA -sPAPERSIZE={papersize} - {pdfPath}"
            );

            var proc = new System.Diagnostics.Process { StartInfo = procStartInfo };
            proc.Start();
            proc.WaitForExit();

            return PhysicalFile(pdfPath, "application/pdf");
        }
    }
}
