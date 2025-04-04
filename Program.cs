using Contourdetection;
using FellowOakDicom;
using FellowOakDicom.Imaging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Coordinate3D;
using DicomReader;

namespace DicomReader
{
    public class Program
    {
        static void Main(string[] args)
        {

            if (args.Length != 2)
            {
                Console.WriteLine("Please provide paths for the DICOM directory and the output directory!");
                return;
            }

            string dicomDirectory = args[0];
            string outputDirectory = args[1];

            try
            {
                if (!Directory.Exists(outputDirectory))
                {
                    Directory.CreateDirectory(outputDirectory);
                }
            }
            catch
            {
                Console.WriteLine("Please provide an existing path!");
                return;
            }


            int globalIndex = 1;
            var dicomFiles = Directory.GetFiles(dicomDirectory).ToList();
            foreach (var (dicomSourcePath, jpgPath, imageCoord3d, imageWidth2d, imageHeight2d, pixelSpacing) in ExtractImagesFromDICOM(dicomDirectory, outputDirectory))
            {
                Console.WriteLine($"Converted {Path.GetFileName(dicomSourcePath)} to JPEG as {Path.GetFileName(jpgPath)}.");              

                string projectDirectory = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.Parent.FullName;
                string dicomJpegFileName = Path.ChangeExtension(Path.GetFileNameWithoutExtension(dicomSourcePath), ".jpeg");
                var result = ContourDetector.FindContoursOnImage(dicomJpegFileName, Path.Combine(projectDirectory, "result"));

                double imageHeight3d = imageHeight2d * pixelSpacing[0];
                double imageWidth3d = imageWidth2d * pixelSpacing[1];
                bool isLast = dicomSourcePath == dicomFiles.Last();

                foreach (var contour in result)
                {
                    List<Point3D> coordinate3D = new List<Point3D>();
                    foreach (var point in contour.Points)
                    {                     
                        double x3d = imageCoord3d[0] + (point.X - imageWidth2d / 2) * (imageWidth3d / imageWidth2d);
                        double y3d = imageCoord3d[1] + (point.Y - imageHeight2d / 2) * (imageHeight3d / imageHeight2d);
                        double z3d = imageCoord3d[2];
                        coordinate3D.Add(new Point3D(x3d, y3d, z3d));
                        Console.WriteLine("point 2d + edges coordinates:");              
                        string neighbors = string.Join(", ", point.EdgeCoordinates);
                        Console.WriteLine($"Point: ({point.X}, {point.Y}) - : [{neighbors}]");
                        Console.WriteLine("point 3d:");
                        Console.WriteLine($"{ x3d}, {y3d}, {z3d}");
                        Console.WriteLine();
                    }

                    String componentName = $"Component {contour.Component}";
                    String resultFile = Path.Combine(outputDirectory, "result.obj");
                    globalIndex = SaveObj.SaveComponentAsObj(resultFile, componentName, coordinate3D, globalIndex, isLast);
               }
                
            }

        }

        // dicomSorceFilePath: the source of the dicom file which we process
        // jpgPath: the path where we can find the output JPG

        public static IEnumerable<(string dicomSourceFilepath, string jpgPath, float[] imageCoord3d, int imageWidth2d, int imageHeight2d, float[] pixelSpacing)>
            ExtractImagesFromDICOM(string dicomDirectory, string outputDirectory)
        {
            foreach (var dicomSourceFilepath in Directory.GetFiles(dicomDirectory))
            {
                DicomFile dicomFile = DicomFile.Open(dicomSourceFilepath);

                DicomDataset dataset = dicomFile.Dataset;
                DicomImage dicomImage = new DicomImage(dicomFile.Dataset);             

                // byte -> pixel
                var renderedImage = dicomImage.RenderImage();

                var imageCoord3d = dataset.GetValues<float>(DicomTag.ImagePositionPatient);             
                var pixelSpacing = dataset.GetValues<float>(DicomTag.PixelSpacing);
              

                if (imageCoord3d != null && imageCoord3d.Length == 3)
                {
                    Console.WriteLine("Image Position: " + string.Join(", ", imageCoord3d));
                }
                else
                {
                    Console.WriteLine("Image Position data is not available.");
                }                

                if (pixelSpacing != null && pixelSpacing.Length == 2)
                {
                    Console.WriteLine($"Pixel Sapcings: {pixelSpacing[0]}, {pixelSpacing[1]}");
                }
                else
                {
                    Console.WriteLine("Unable to calculate resolution: Missing data.");
                }

                var resolutionX = renderedImage.Width;
                var resolutionY = renderedImage.Height;

                Console.WriteLine($"Resolution: {resolutionX} x {resolutionY}");

                // pixels to image
                var image = new Image<Rgba32>(renderedImage.Width, renderedImage.Height);

                for (int y = 0; y < renderedImage.Height; y++)
                {
                    for (int x = 0; x < renderedImage.Width; x++)
                    {
                        // get pixel color
                        var pixel = renderedImage.GetPixel(x, y);

                        // add pixel to image
                        image[x, y] = new Rgba32(pixel.R, pixel.G, pixel.B);
                    }
                }

                string jpgPath = Path.Combine(outputDirectory,
                        Path.GetFileNameWithoutExtension(dicomSourceFilepath) + ".jpeg");

                //image.SaveAsJpeg(jpgPath);

                yield return (dicomSourceFilepath, jpgPath, imageCoord3d, resolutionX, resolutionY, pixelSpacing);
            }
        }
    }
}