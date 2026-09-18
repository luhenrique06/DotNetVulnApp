using DotNetVulnApp.Models;
using DotNetVulnApp.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
            string sourcePath = doc + ".txt";

            if (!System.IO.File.Exists(sourcePath))
                return BadRequest("Doc não existe");

            string papersize = Request.Query["size"];
            string pdfPath = "statement.pdf";

            var procStartInfo = new System.Diagnostics.ProcessStartInfo(
                "/bin/bash",
                $"-c \"enscript {sourcePath} -o - | ps2pdf -dFIXEDMEDIA -sPAPERSIZE={papersize} - {pdfPath}\""
            )
            {
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardError = true
            };

            var proc = new System.Diagnostics.Process { StartInfo = procStartInfo };
            proc.Start();
            proc.WaitForExit();

            string localPdfPath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), pdfPath);
            return PhysicalFile(localPdfPath, "application/pdf");
        }
    }
}
