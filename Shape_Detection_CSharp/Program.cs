using OpenCvSharp;

public class Program
{

    public static void Main(string[] args)
    {
        var frameWidth = 640;
        var frameHigh = 480;
        SetupWindow();
        while (true)
        {
            using (var cap = new VideoCapture(0))
            {
                cap.Set(3, frameWidth);
                cap.Set(4, frameHigh);
                Mat img = new Mat();
                Mat imgBlur = new Mat();
                Mat imgGary = new Mat();
                Mat imgCanny = new Mat();
                Mat imgDil = new Mat();
                Mat kernel = new Mat(new Size(5, 5), MatType.CV_16S);
                if (cap.Read(img))
                {
                    var imgContour = img.Clone();
                    var threshold1 = Cv2.GetTrackbarPos("Threshold 1", "Parameters");
                    var threshold2 = Cv2.GetTrackbarPos("Threshold 2", "Parameters");
                    Cv2.GaussianBlur(img, imgBlur, new Size(7, 7), 1);
                    Cv2.CvtColor(imgBlur, imgGary, ColorConversionCodes.BGR2GRAY);
                    Cv2.Canny(imgGary, imgCanny, threshold1, threshold2);
                    Cv2.Dilate(imgCanny, imgDil, kernel, iterations: 1);

                    GetContorous(imgDil, imgContour);

                    var imgStack = StackImages(0.8,new List<List<Mat>>() {  });
                    Cv2.ImShow("Result", imgStack);
                    if ((Cv2.WaitKey(1) & 0xFF) == 'q')
                        break;
                }
            }
        }
    }

    public static void SetupWindow()
    {
        int trackbarThreashold1Value = 150;
        int trackbarThreashold2Value = 255;
        int trackbarAreaValue = 5000;
        Cv2.NamedWindow("Parameters");
        Cv2.ResizeWindow("Parameters", 640, 240);
        Cv2.CreateTrackbar("Threshold 1", "Parameters", ref trackbarThreashold1Value, 255);
        Cv2.CreateTrackbar("Threshold 2", "Parameters", ref trackbarThreashold2Value, 255);
        Cv2.CreateTrackbar("Area", "Parameters", ref trackbarAreaValue, 30000);
    }

    public static Mat? StackImages(double scale, List<List<Mat>> imgArray)
    {
        Mat? result = null;
        if (scale < 0 || scale > 1)
            throw new ArgumentOutOfRangeException(nameof(scale));
        if (imgArray == null)
            throw new ArgumentNullException(nameof(imgArray));
        var rows = imgArray.Count();
        var columns = rows > 0 ? imgArray[0].Count : 1;
        var rowsAvailable = columns > 1;
        var width = imgArray[0][0].Width;
        var height = imgArray[0][0].Height;
        if (rowsAvailable)
        {
            for (int x = 0; x < rows; x++)
            {
                for (int y = 0; y < columns; y++)
                {
                    //if(imgArray[x][y].Data)
                }
            }
        }
        else
        {

        }
        return result;
    }

    public static void GetContorous(Mat img, Mat imgContour)
    {
        if (img == null)
            throw new ArgumentNullException(nameof(img));
        if (imgContour == null)
            throw new ArgumentNullException(nameof(imgContour));
        var hierarchy = new Mat();
        Cv2.FindContours(img, out var contours, hierarchy, RetrievalModes.External, ContourApproximationModes.ApproxNone);
        foreach (var cnt in contours)
        {
            var area = Cv2.ContourArea(cnt);
            var areaMin = Cv2.GetTrackbarPos("Area", "Parameters");
            //Filter na velkost
            if (area > areaMin)
            {
                Cv2.DrawContours(imgContour, new Mat[] { cnt }, -1, new Scalar(255, 0, 255), 7);
                var peri = Cv2.ArcLength(cnt, true);
                var epsilon = 0.02 * peri;
                var approx = new Mat();
                Cv2.ApproxPolyDP(cnt, approx, epsilon, true);
                Console.WriteLine($"{approx.Width}");
                var boundRect = Cv2.BoundingRect(approx);
                Cv2.Rectangle(imgContour, boundRect, new Scalar(0, 255, 0), 5);

                Cv2.PutText(imgContour, $"Points: {approx.Width}", new Point(boundRect.X + boundRect.Width + 20, boundRect.Y + 20), HersheyFonts.HersheyComplex, 0.7, new Scalar(0, 255, 0), 2);
                Cv2.PutText(imgContour, $"Area: {(int)area}", new Point(boundRect.X + boundRect.Width + 20, boundRect.Y + 45), HersheyFonts.HersheyComplex, 0.7, new Scalar(0, 255, 0), 2);

                //Triangle
                if (approx.Width == 3)
                {
                    Cv2.PutText(imgContour, $"Triangle", new Point(boundRect.X, boundRect.Y), HersheyFonts.HersheyComplex, 0.6, new Scalar(255, 255, 255), 2);
                }
                else if (approx.Width == 4)
                {
                    Cv2.PutText(imgContour, $"Rectangle", new Point(boundRect.X, boundRect.Y), HersheyFonts.HersheyComplex, 0.6, new Scalar(255, 255, 255), 2);
                }
                else if (approx.Width == 5)
                {
                    Cv2.PutText(imgContour, $"Pentagon", new Point(boundRect.X, boundRect.Y), HersheyFonts.HersheyComplex, 0.6, new Scalar(255, 255, 255), 2);
                }
                else if (approx.Width == 6)
                {
                    Cv2.PutText(imgContour, $"Hexagon", new Point(boundRect.X, boundRect.Y), HersheyFonts.HersheyComplex, 0.6, new Scalar(255, 255, 255), 2);
                }
                else
                {
                    Cv2.PutText(imgContour, $"Circle", new Point(boundRect.X, boundRect.Y), HersheyFonts.HersheyComplex, 0.6, new Scalar(255, 255, 255), 2);
                }
            }

        }
    }
}