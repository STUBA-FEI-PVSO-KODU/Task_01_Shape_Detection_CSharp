using OpenCvSharp;

public class Program
{

    public static void Main(string[] args)
    {
        var frameWidth = 640;
        var frameHigh = 480;
        SetupWindow();

        using (var cap = new VideoCapture(0))
        {
            cap.Set(3, frameWidth);
            cap.Set(4, frameHigh);
            while (true)
            {
                Mat img = new Mat();
                Mat imgBlur = new Mat();
                Mat imgGary = new Mat();
                Mat imgCanny = new Mat();
                Mat imgDil = new Mat();
                //Mat sobel = new Mat();
                Mat kernel = Mat.Ones(new Size(5, 5), MatType.CV_16U);
                if (cap.Read(img))
                {
                    var imgContour = img.Clone();
                    var threshold1 = Cv2.GetTrackbarPos("Threshold 1", "Parameters");
                    var threshold2 = Cv2.GetTrackbarPos("Threshold 2", "Parameters");
                    var threshold3 = Cv2.GetTrackbarPos("Area", "Parameters");
                    Cv2.GaussianBlur(img, imgBlur, new Size(7, 7), 1);
                    Cv2.CvtColor(imgBlur, imgGary, ColorConversionCodes.BGR2GRAY);
                    Cv2.Canny(imgGary, imgCanny, threshold1, threshold2);
                    //Cv2.Dilate(imgCanny, imgDil, kernel, iterations: 1);
                    //GetContorous(imgDil, imgContour);
                    /*var cicles = Cv2.HoughCircles(imgGary, HoughModes.Gradient, 1, 20, param1: threshold1, param2: threshold2, minRadius: 10, maxRadius: 50);
                    for (int i = 0; i < cicles.Length; i++)
                    {
                        var cicle = cicles[i];
                        Cv2.Circle(img, (int)cicle.Center.X, (int)cicle.Center.Y, (int)cicle.Radius, new Scalar(0, 255, 0), 2);
                    }
                    Cv2.ImShow("Result",img);*/
                    //Cv2.Sobel(img, sobel, MatType.CV_8UC1, 2, 2);
                    var lines = Cv2.HoughLinesP(imgCanny, 1, Math.PI / 180, threshold3);
                    for (int i = 0; i < lines.Length; i++)
                    {
                        var line = lines[i];
                        Cv2.Line(imgGary, line.P1, line.P2, new Scalar(0, 255, 0), 2);
                    }
                    var result = new Mat();
                    Cv2.HConcat(new Mat[] { imgCanny, imgGary },result);
                    Cv2.ImShow("Result", result);
                    //var imgStack = imgDil;
                    //var imgStack = StackImages(0.8, new List<List<Mat>>() { });
                    //Cv2.ImShow("Result", imgStack);
                    //Cv2.ImShow("Result", imgContour);
                    //Cv2.ImShow("Result", imgContour);
                    if ((Cv2.WaitKey(1) & 0xFF) == 'q')
                        break;

                }
                else
                    break;
            }
        }
        Cv2.DestroyAllWindows();
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
        //Cv2.CreateTrackbar("Area", "Parameters", ref trackbarAreaValue, 30000);
        Cv2.CreateTrackbar("Area", "Parameters", ref trackbarAreaValue, 255);
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
                    if (imgArray[x][y] == imgArray[0][0])
                    {
                        Cv2.Resize(imgArray[x][y], imgArray[x][y], new Size(0, 0), scale, scale, InterpolationFlags.Linear);
                    }
                    else
                    {
                        Cv2.Resize(imgArray[x][y], imgArray[x][y], new Size(height, width), scale, scale, InterpolationFlags.Linear);
                    }
                    if (imgArray[x][y].Cols == 2)
                    {
                        Cv2.CvtColor(imgArray[x][y], imgArray[x][y], ColorConversionCodes.GRAY2BGR);
                    }
                }
            }
        }
        else
        {

        }
        result = new Mat();
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
                var approx = cnt.ApproxPolyDP(epsilon, true);
                //Console.WriteLine($"{approx.Width}");
                var boundRect = Cv2.BoundingRect(approx);
                Cv2.Rectangle(imgContour, boundRect, new Scalar(0, 255, 0), 5);

                Cv2.PutText(imgContour, $"Points: {approx.Size(0)}", new Point(boundRect.X + boundRect.Width + 20, boundRect.Y + 20), HersheyFonts.HersheyComplex, 0.7, new Scalar(0, 255, 0), 2);
                Cv2.PutText(imgContour, $"Area: {(int)area}", new Point(boundRect.X + boundRect.Width + 20, boundRect.Y + 45), HersheyFonts.HersheyComplex, 0.7, new Scalar(0, 255, 0), 2);

                //Triangle
                if (approx.Size(0) == 3)
                {
                    Cv2.PutText(imgContour, $"Triangle", new Point(boundRect.X, boundRect.Y), HersheyFonts.HersheyComplex, 0.6, new Scalar(255, 255, 255), 2);
                }
                else if (approx.Size(0) == 4)
                {
                    Cv2.PutText(imgContour, $"Rectangle", new Point(boundRect.X, boundRect.Y), HersheyFonts.HersheyComplex, 0.6, new Scalar(255, 255, 255), 2);
                }
                else if (approx.Size(0) == 5)
                {
                    Cv2.PutText(imgContour, $"Pentagon", new Point(boundRect.X, boundRect.Y), HersheyFonts.HersheyComplex, 0.6, new Scalar(255, 255, 255), 2);
                }
                else if (approx.Size(0) == 6)
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