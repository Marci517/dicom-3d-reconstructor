using Coordinate3D;

namespace DicomReader
{
    internal class SaveObj
    {
        private static List<Point3D> previousPoints = null;
        public static int SaveComponentAsObj(string filePath, string componentName, List<Point3D> points, int globalIndex, bool isLast)
        {          
            using (StreamWriter writer = new StreamWriter(filePath, true))
            {
                writer.WriteLine($"# Component {componentName}"); 
               
                foreach (var point in points)
                {
                    writer.WriteLine($"v {point.X} {point.Y} {point.Z}");                  
                }

                if (globalIndex == 1 || isLast)
                {                
                    for (int i = 1; i < points.Count - 1; i++)
                    {
                        writer.WriteLine($"f {globalIndex} {globalIndex + i} {globalIndex + i + 1}");
                    }
                }
                else
                {                 
                    for (int i = 0; i < points.Count - 2; i++)
                    {
                        writer.WriteLine($"f {globalIndex + i} {globalIndex + i + 1} {globalIndex + i + 2}");
                    }
                    writer.WriteLine($"f {globalIndex + points.Count - 2} {globalIndex + points.Count - 1} {globalIndex}");
                }

                if (previousPoints != null)
                {
                    int previousIndex = globalIndex - previousPoints.Count;

                    for (int i = 0; i < Math.Min(previousPoints.Count, points.Count) - 1; i++)
                    {
                        writer.WriteLine($"f {previousIndex + i} {globalIndex + i} {previousIndex + i + 1}");
                        writer.WriteLine($"f {globalIndex + i} {globalIndex + i + 1} {previousIndex + i + 1}");
                    }
                }
              
                previousPoints = new List<Point3D>(points);
                globalIndex += points.Count;              
            }
            return globalIndex;
        }

    }
}
