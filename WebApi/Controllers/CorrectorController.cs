using System.Drawing;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using Microsoft.AspNetCore.Mvc;
using WebApi.Common;

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

            var filePath = Path.Combine("uploads", file.FileName);

            if (!Directory.Exists("uploads"))
            {
                Directory.CreateDirectory("uploads");
            }

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                file.CopyTo(stream);
            }

            var template = new List<string> { "A", "B", "C" }; // Dummy data

            var buffer = file.OpenReadStream().ReadAllBytes();
            var mat = new Mat();
            CvInvoke.Imdecode(buffer, ImreadModes.Color, mat);
            var image = mat.ToImage<Bgr, byte>(); 

            var grayImage = image.Convert<Gray, byte>();

            var outputGrayFilePath = Path.Combine("uploads", "gray.png");
            grayImage.Save(outputGrayFilePath);

            double thresholdValue = 50;  // 0 = pure black, 255 = pure white
            var binaryImage = new Mat();
            CvInvoke.Threshold(grayImage, binaryImage, thresholdValue, 255, ThresholdType.BinaryInv);

            var kernel = CvInvoke.GetStructuringElement(ElementShape.Ellipse, new Size(3, 3), new Point(-1, -1));
            CvInvoke.MorphologyEx(binaryImage, binaryImage, MorphOp.Open, kernel, new Point(-1, -1), 1, BorderType.Default, new MCvScalar());

            using var contours = new VectorOfVectorOfPoint();
            CvInvoke.FindContours(binaryImage, contours, null, RetrType.External, ChainApproxMethod.ChainApproxSimple);

            for (int i = 0; i < contours.Size; i++)
            {
                var contour = contours[i];
                double area = CvInvoke.ContourArea(contour);
                if (area < 5) // Filter out small contours
                {
                    continue;
                }

                var moments = CvInvoke.Moments(contour);
                int cx = (int)(moments.M10 / moments.M00);
                int cy = (int)(moments.M01 / moments.M00);

                CvInvoke.Circle(image, new Point(cx, cy), 5, new MCvScalar(0, 0, 255), 2); // Draw circle at centroid
            }

            var outputFilePath = Path.Combine("uploads", "output.png");
            image.Save(outputFilePath);

            return Ok(new { FilePath = outputFilePath });
        }
    }
}
