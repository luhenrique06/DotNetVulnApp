using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotNetVulnApp.Models;
using Microsoft.AspNetCore.Mvc;
using DotNetVulnApp.Utils;

namespace DotNetVulnApp.Controllers
{
    [Route("api/[controller]")]
    public class UpdateDocsController :ControllerBase
    {
        
        [HttpPost]
        public IActionResult PostNewDoc([FromBody] Docs doc){
            
            string defaultPathForDocs = Directory.GetCurrentDirectory() + "/files";

            if(doc.Content == null || doc.FileName == null)
                return BadRequest("Missing mandatory parameters");

            doc.FileName = defaultPathForDocs + "/" + doc.FileName;

            try
            {
                OptimisedIO.saveFileRaw(doc.FileName, doc.Content);
                return Ok(new
                {
                message = "Doc was saved"
                }); 
            }
            catch(Exception ex)
            {
                return BadRequest("Error while save doc");
            }
        }

    }
}