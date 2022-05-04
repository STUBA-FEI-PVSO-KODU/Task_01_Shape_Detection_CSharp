using OpenCvSharp;
using OpenCvSharp.Extensions;
using Shape_Detection_CSharp;
using System.Drawing;

public class Program
{
    public static void Main(string[] args)
    {
        //Program starts there.
        Task02();
    }
    public static void SetupWindow(string parameterName1 = "threshold1", string parameterName2 = "threshold2", string parameterName3 = "threshold3", int threshold1 = 150, int threshold2 = 255, int threshold3 = 5000)
    {
        Cv2.NamedWindow("Parameters");
        Cv2.ResizeWindow("Parameters", 640, 240);
        Cv2.CreateTrackbar(parameterName1, "Parameters", ref threshold1, 255);
        Cv2.CreateTrackbar(parameterName2, "Parameters", ref threshold2, 255);
        Cv2.CreateTrackbar(parameterName3, "Parameters", ref threshold3, 255);
    }
    public static void Task02()
    {
        SetupWindow(parameterName1: "Canny threshold 1", parameterName2: "Canny threshold 2", parameterName3: "Hough threshold", threshold1: 100, threshold2: 150, threshold3: 105);
        string imgPathSource = @$"{Environment.CurrentDirectory}\..\..\..\img\org_img.png";
        //string imgPathSource = @$"{Environment.CurrentDirectory}\..\..\..\img\sudoku.png";
        //string imgPathSource = @$"{Environment.CurrentDirectory}\..\..\..\img\flats.jpg";
        Mat imgEdge = new Mat();
        Mat imgBlur = new Mat();
        Mat imgRes = new Mat();
        var houghTrans = new HoughTransformation();
        var imgOrig = Cv2.ImRead(imgPathSource, ImreadModes.Color);
        while (true)
        {
            var threshold1 = Cv2.GetTrackbarPos("Canny threshold 1", "Parameters");
            var threshold2 = Cv2.GetTrackbarPos("Canny threshold 2", "Parameters");
            var threshold3 = Cv2.GetTrackbarPos("Hough threshold", "Parameters");
            imgRes = imgOrig.Clone();
            Cv2.Blur(imgOrig, imgBlur, new OpenCvSharp.Size(5, 5));
            Cv2.Canny(imgBlur, imgEdge, threshold1, threshold2, 3);
            int w = imgEdge.Cols;
            int h = imgEdge.Rows;
            var data = new List<byte>();
            for (int y = 0; y < imgEdge.Rows; y++)
            {
                for (int x = 0; x < imgEdge.Cols; x++)
                {
                    data.Add(imgEdge.At<byte>(y, x));
                }
            }
            //Hough Transform
            houghTrans.Transform(data, w, h);
            //Get lines
            if (threshold3 == 0)
                threshold3 = w > h ? w / 4 : h / 4;
            var lines = houghTrans.GetLines(threshold3);
            //Draw lines 
            foreach (var line in lines)
            {
                Cv2.Line(imgRes, (int)line.Start.X, (int)line.Start.Y, (int)line.End.X, (int)line.End.Y, new Scalar(255, 0, 0), 2);
            }
            //Visualize Accumulator
            int aw, ah;
            var accu = houghTrans.GetAccu(out aw, out ah);
            var imgAccuBitmap = new Bitmap(aw, ah);
            int imgX = 0;
            int imgY = 0;
            for (int p = 0; p < (ah * aw); p++)
            {
                byte c = (byte)accu[p];
                imgAccuBitmap.SetPixel(imgX, imgY, Color.FromArgb(255-c, 255-c, 255));
                if (imgX < aw - 1)
                {
                    imgX++;
                }
                else
                {
                    imgY++;
                    imgX = 0;
                }
            }
            //Show results in images
            var imgAccu = imgAccuBitmap.ToMat();
            var result = new Mat();
            Cv2.HConcat(new Mat[] { imgOrig, imgRes }, result);
            Cv2.ImShow("Result", result);
            Cv2.ImShow("Result-Edges", imgEdge);
            Cv2.ImShow("Result-Accu", imgAccu);
            //Cv2.ImWrite(imgPathDestAccum, imgAccu);
            if ((Cv2.WaitKey(10) & 0xFF) == 'q')
                break;
        }
        Cv2.DestroyAllWindows();
    }
}