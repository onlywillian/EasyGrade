using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CorrectorController : Controller
    {
        [HttpPost]
        [Consumes("multipart/form-data")]
        public IActionResult GetNotesFromImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            var suportedTypes = new[] { "image/jpeg", "image/png", "image/gif" };

            if (!suportedTypes.Contains(file.ContentType))
            {
                return BadRequest("Unsupported file type.");
            }

            // Process the file here and save it
            // For example, you can save it to a specific directory
            var filePath = Path.Combine("uploads", file.FileName);

            if (!Directory.Exists("uploads"))
            {
                Directory.CreateDirectory("uploads");
            }

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                file.CopyTo(stream);
            }

            // Return a success response
            return Ok(new { FilePath = filePath });
        }
    }
}
