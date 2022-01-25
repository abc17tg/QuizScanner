using OpenCvSharp;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ImageExtraction : MonoBehaviour
{
    [Header("HoughLines"), SerializeField] private int m_maxLineGap = 20, m_minThreshold = 15, m_minLineLength = 1200;
    [Header("Cell border offset in pix"), SerializeField, Range(0, 30)] private int m_offset = 15;
    [Header("MedianBlur ksize"), SerializeField, Range(0, 100)] private int m_medianBlurKsize = 69;

    public void GetTable(Mat paper)
    {
        int numberForSaves = 1;

        if (paper.Height < paper.Width)
            paper = paper.Rotate(RotateFlags.Rotate90Clockwise);

        //convert color to gray
        if (paper.Type().Channels == 3)
            Cv2.CvtColor(paper, paper, ColorConversionCodes.BGR2GRAY);
        SaveManager.ImageSaverFromTexture2D(OpenCvSharp.Unity.MatToTexture(paper), Paths.Instance.PhotoFilePath, "Raw_photo_gray");
        //resize to constant resolution
        Cv2.Resize(paper, paper, new Size(1536, 2048));
        //denoise
        Cv2.FastNlMeansDenoising(paper, paper);
        SaveManager.ImageSaverFromTexture2D(OpenCvSharp.Unity.MatToTexture(paper), Paths.Instance.PhotoFilePath, "Denoise");

        Mat paperTemp = EnhanceImage(paper);

        //extract paper from photo
        List<Point2f> srcPoints = GetBiggestPolygonPoints(paperTemp, ref numberForSaves);

        if (srcPoints == null)
        {
#if (UNITY_EDITOR)
            Debug.Log("No paper detected");
#endif
            return;
        }

        //making destination mat 1240 x 1754
        List<Point2f> dstPoints = new List<Point2f>
        {
            new Point2f(0, 0),
            new Point2f(1240, 0),
            new Point2f(1240, 1754),
            new Point2f(0, 1754)
        };

        //warp and crop image
        Mat M = Cv2.GetPerspectiveTransform(srcPoints.ToArray(), dstPoints.ToArray());
        Mat warpedMat = new Mat();
        Cv2.WarpPerspective(paper, warpedMat, M, new Size(1240, 1754));
        SaveManager.ImageSaverFromTexture2D(OpenCvSharp.Unity.MatToTexture(warpedMat), Paths.Instance.PhotoFilePath, "Cropped_Paper");
        warpedMat = EnhanceImage(warpedMat);
        SaveManager.ImageSaverFromTexture2D(OpenCvSharp.Unity.MatToTexture(warpedMat), Paths.Instance.PhotoFilePath, "Cropped_Paper_EN");
        //Erode the image
        Cv2.Erode(warpedMat, warpedMat, new Mat(), null, 1);
        SaveManager.ImageSaverFromTexture2D(OpenCvSharp.Unity.MatToTexture(warpedMat), Paths.Instance.PhotoFilePath, "Erode_EN");
        //blur the image
        Cv2.GaussianBlur(warpedMat, warpedMat, new Size(3, 3), 0);
        SaveManager.ImageSaverFromTexture2D(OpenCvSharp.Unity.MatToTexture(warpedMat), Paths.Instance.PhotoFilePath, "Gaussian_Blur_EN");
        //making the image black and white
        Cv2.Threshold(warpedMat, warpedMat, 0, 255, ThresholdTypes.Otsu);
        SaveManager.ImageSaverFromTexture2D(OpenCvSharp.Unity.MatToTexture(warpedMat), Paths.Instance.PhotoFilePath, "Threshold_Otsu_EN");


        numberForSaves++;
        srcPoints = GetBiggestPolygonPoints(warpedMat, ref numberForSaves);
        if (srcPoints != null)
        {
            M = Cv2.GetPerspectiveTransform(srcPoints.ToArray(), dstPoints.ToArray());
            Cv2.WarpPerspective(warpedMat, warpedMat, M, new Size(1240, 1754));
            gameObject.GetComponentInChildren<RawImage>().texture = OpenCvSharp.Unity.MatToTexture(warpedMat);
            SaveManager.ImageSaverFromTexture2D(OpenCvSharp.Unity.MatToTexture(warpedMat), Paths.Instance.PhotoFilePath, "Cropped_Table");
        }

        if (!SliceTable(warpedMat))
        {
            GetComponent<ImageView>().SetAnswersText("No table detected!");
#if (UNITY_EDITOR)
            Debug.Log("No table detected");
#endif
            return;
        }
    }

    private Mat EnhanceImage(Mat img)
    {
        Mat imgTemp = RemoveShadows(img);
        Cv2.Erode(imgTemp, imgTemp, new Mat(), null, 3);
        SaveManager.ImageSaverFromTexture2D(OpenCvSharp.Unity.MatToTexture(imgTemp), Paths.Instance.PhotoFilePath, "EN_Erode");
        Cv2.Dilate(imgTemp, imgTemp, new Mat(), null, 2);
        SaveManager.ImageSaverFromTexture2D(OpenCvSharp.Unity.MatToTexture(imgTemp), Paths.Instance.PhotoFilePath, "EN_Dilate");
        Cv2.Threshold(imgTemp, imgTemp, 50, 255, ThresholdTypes.Tozero);
        Cv2.BitwiseNot(imgTemp, imgTemp);
        SaveManager.ImageSaverFromTexture2D(OpenCvSharp.Unity.MatToTexture(imgTemp), Paths.Instance.PhotoFilePath, "EN_Threshold_1");
        Cv2.Threshold(imgTemp, imgTemp, 50, 255, ThresholdTypes.Tozero);
        Cv2.BitwiseNot(imgTemp, imgTemp);
        SaveManager.ImageSaverFromTexture2D(OpenCvSharp.Unity.MatToTexture(imgTemp), Paths.Instance.PhotoFilePath, "EN_Threshold_2");
        Cv2.AdaptiveThreshold(imgTemp, imgTemp, 255, AdaptiveThresholdTypes.GaussianC, ThresholdTypes.Binary, 121, 1);
        SaveManager.ImageSaverFromTexture2D(OpenCvSharp.Unity.MatToTexture(imgTemp), Paths.Instance.PhotoFilePath, "EN_Image_Removed_shadow_errode_dilate_AdaptiveThreshold");
        return imgTemp;
    }

    private Mat RemoveShadows(Mat img)
    {
        if (img.Type().Channels == 3)
            Cv2.CvtColor(img, img, ColorConversionCodes.BGR2GRAY);
        Mat mat = img.Dilate(Mat.Ones(15, 15, MatType.CV_8UC1), null, 1);
        SaveManager.ImageSaverFromTexture2D(OpenCvSharp.Unity.MatToTexture(mat), Paths.Instance.PhotoFilePath, "Remove_shadow_Dilate");
        mat = mat.MedianBlur(m_medianBlurKsize);
        SaveManager.ImageSaverFromTexture2D(OpenCvSharp.Unity.MatToTexture(mat), Paths.Instance.PhotoFilePath, "Remove_shadow_MedianBlur");
        Cv2.Absdiff(img, mat, mat);
        SaveManager.ImageSaverFromTexture2D(OpenCvSharp.Unity.MatToTexture(mat), Paths.Instance.PhotoFilePath, "Remove_shadow_Absdiff");
        Cv2.BitwiseNot(mat, mat);
        SaveManager.ImageSaverFromTexture2D(OpenCvSharp.Unity.MatToTexture(mat), Paths.Instance.PhotoFilePath, "Remove_shadow_Final_nonNormalized");
        mat = mat.Normalize(0, 255, NormTypes.MinMax);
        SaveManager.ImageSaverFromTexture2D(OpenCvSharp.Unity.MatToTexture(mat), Paths.Instance.PhotoFilePath, "Remove_shadow_Final");
        return mat;
    }

    private List<Point2f> GetBiggestPolygonPoints(Mat mainMat, ref int numberForSaves)
    {
        Mat workingMat = new Mat();

        //extract the edge of the image
        Cv2.Canny(mainMat, workingMat, 50, 100);
        SaveManager.ImageSaverFromTexture2D(OpenCvSharp.Unity.MatToTexture(workingMat), Paths.Instance.PhotoFilePath, "Canny" + numberForSaves.ToString());

        //finding contours of paper
        List<MatOfPoint> contours = Cv2.FindContoursAsMat(workingMat, RetrievalModes.External, ContourApproximationModes.ApproxSimple).ToList();
        List<MatOfPoint2f> polygons4p = new List<MatOfPoint2f>();
        MatOfPoint2f cornersPoints = new MatOfPoint2f();

        foreach (var contour in contours)
        {
            MatOfPoint2f polygon4p = new MatOfPoint2f(contour.ApproxPolyDP(Cv2.ArcLength((InputArray)contour, true) * 0.03, true));
            polygon4p.ConvertTo(polygon4p, MatType.CV_32FC2);
            if (polygon4p.ToArray().Length == 4)
                polygons4p.Add(polygon4p);
        }
        //cornersPoints = polygons4p.OrderByDescending(p4p => p4p.ContourArea()).ToList();
        cornersPoints = polygons4p.Where(c => c.ContourArea() == polygons4p.Max(cc => cc.ContourArea())).FirstOrDefault();
        if (cornersPoints == null)
            return null;

        List<Point2f> srcPoints = Utils.Sort4Points2fClockwiseFromTop(cornersPoints.ToList());

        //draw circles on detected corners on image
        if (SaveManager.Instance.SaveEnabled)
        {
            Mat mainMatTempCopy = mainMat.Clone();
            mainMatTempCopy = mainMatTempCopy.CvtColor(ColorConversionCodes.GRAY2BGR);
            foreach (var sp in srcPoints)
                Cv2.Circle(mainMatTempCopy, sp, 7, Scalar.Red, 2);
            SaveManager.ImageSaverFromTexture2D(OpenCvSharp.Unity.MatToTexture(mainMatTempCopy), Paths.Instance.PhotoFilePath, "Corners_Paper" + numberForSaves.ToString());
        }

        float pictureToContourRatio = (float)cornersPoints.ContourArea() / (mainMat.Height * mainMat.Width);

        //when paper detected too small
        if (pictureToContourRatio < 0.20)
            return null;

        if (pictureToContourRatio > 0.95)
        {
            List<Point2f> srcPointsTemp = new List<Point2f>
            {
                new Point2f(0 + m_offset, 0 + m_offset),
                new Point2f(mainMat.Width - m_offset, 0 + m_offset),
                new Point2f(mainMat.Width - m_offset, mainMat.Height - m_offset),
                new Point2f(0 + m_offset, mainMat.Height - m_offset)
            };

            List<Point2f> dstPoints = new List<Point2f>
            {
                new Point2f(0, 0),
                new Point2f(1240, 0),
                new Point2f(1240, 1754),
                new Point2f(0, 1754)
            };

            mainMat = mainMat.WarpPerspective(Cv2.GetPerspectiveTransform(srcPointsTemp.ToArray(), dstPoints.ToArray()), new Size(mainMat.Width, mainMat.Height));
            numberForSaves++;
            return GetBiggestPolygonPoints(mainMat, ref numberForSaves);
        }

        return srcPoints;
    }

    private void ResetHoughLinesParameters(HoughLinesParameter parameter)
    {
        switch (parameter)
        {
            case HoughLinesParameter.MaxLineGap:
                m_maxLineGap = 20;
                break;
            case HoughLinesParameter.MinThreshold:
                m_minThreshold = 15;
                break;
            case HoughLinesParameter.MinLineLength:
                m_minLineLength = 1200;
                break;
            case HoughLinesParameter.AllParameters:
                m_minLineLength = 1200;
                m_minThreshold = 15;
                m_maxLineGap = 20;
                break;
        }
    }

    private enum HoughLinesParameter : int
    {
        MaxLineGap = 1,
        MinThreshold = 2,
        MinLineLength = 3,
        AllParameters = 4
    }

    private bool SliceTable(Mat mainMat)
    {
        Mat workingMat = mainMat.Clone();
        //remove gray from picture and invert picture
        Cv2.Threshold(workingMat, workingMat, 100, 255, ThresholdTypes.BinaryInv);
        SaveManager.ImageSaverFromTexture2D(OpenCvSharp.Unity.MatToTexture(workingMat), Paths.Instance.TableFilePath, "Threshold");

        //get table lines and intersections
        Table table = SettingsMenu.Instance.TableParams;
        Lines lines = new Lines();
        Dictionary<HoughLinesParameter, int> bestParameters = new Dictionary<HoughLinesParameter, int>();
        int expectedNumberOfInterscections = (table.Columns + 1) * (table.Rows + 1);
        int previousBiggestNumberOfInterscections = 0;
        int mode = 1;

        for (int i = 0; i < 100; i++)
        {
            lines = new Lines();
            lines.SetLines(Cv2.HoughLinesP(workingMat, 1, Cv2.PI / 180, m_minThreshold, m_minLineLength, m_maxLineGap), workingMat.Size());
            lines.FindIntesections(workingMat.Size(), table, workingMat.Clone());
#if (UNITY_EDITOR)
            Debug.Log($"iteration HoughLines: {i}, mode: {mode}, MaxLineGap = {m_maxLineGap}, MinThreshold = {m_minThreshold}, MinLineLength = {m_minLineLength}, Intersections = {lines.Intersections.Count}");
#endif

            if (lines.Intersections.Count != expectedNumberOfInterscections)
            {
                if (i == 0)
                {
                    bestParameters.Add(HoughLinesParameter.MaxLineGap, m_maxLineGap);
                    bestParameters.Add(HoughLinesParameter.MinLineLength, m_minLineLength);
                    bestParameters.Add(HoughLinesParameter.MinThreshold, m_minThreshold);
                    previousBiggestNumberOfInterscections = lines.Intersections.Count;
                }

                switch (mode)
                {
                    case 1:
                        if (m_minThreshold < 150)
                        {
                            if (previousBiggestNumberOfInterscections < lines.Intersections.Count)
                            {
                                bestParameters[HoughLinesParameter.MinThreshold] = m_minThreshold;
                                previousBiggestNumberOfInterscections = lines.Intersections.Count;
                            }
                            m_minThreshold += Mathf.RoundToInt(0.2f * m_minThreshold);
                            continue;
                        }
                        if (previousBiggestNumberOfInterscections < lines.Intersections.Count)
                            bestParameters[HoughLinesParameter.MinThreshold] = m_minThreshold;
                        previousBiggestNumberOfInterscections = 0;
                        ResetHoughLinesParameters(HoughLinesParameter.MinThreshold);
                        mode++;
                        break;
                    case 2:
                        if (m_maxLineGap < 120)
                        {
                            if (previousBiggestNumberOfInterscections < lines.Intersections.Count)
                            {
                                bestParameters[HoughLinesParameter.MaxLineGap] = m_maxLineGap;
                                previousBiggestNumberOfInterscections = lines.Intersections.Count;
                            }
                            m_maxLineGap += Mathf.RoundToInt(0.15f * m_maxLineGap);
                            continue;
                        }
                        if (previousBiggestNumberOfInterscections < lines.Intersections.Count)
                            bestParameters[HoughLinesParameter.MaxLineGap] = m_maxLineGap;
                        previousBiggestNumberOfInterscections = 0;
                        ResetHoughLinesParameters(HoughLinesParameter.MaxLineGap);
                        mode++;
                        break;
                    case 3:
                        m_minThreshold = bestParameters[HoughLinesParameter.MinThreshold];
                        m_maxLineGap = bestParameters[HoughLinesParameter.MaxLineGap];
                        previousBiggestNumberOfInterscections = lines.Intersections.Count;
                        mode++;
                        break;
                    case 4:
                        if (previousBiggestNumberOfInterscections > lines.Intersections.Count)
                            ResetHoughLinesParameters(HoughLinesParameter.AllParameters);
                        previousBiggestNumberOfInterscections = 0;
                        mode++;
                        break;
                    case 5:
                        if (m_minLineLength > 300)
                        {
                            if (previousBiggestNumberOfInterscections < lines.Intersections.Count)
                            {
                                bestParameters[HoughLinesParameter.MinLineLength] = m_minLineLength;
                                previousBiggestNumberOfInterscections = lines.Intersections.Count;
                            }
                            m_minLineLength -= Mathf.RoundToInt(0.15f * m_minLineLength);
                            continue;
                        }
                        mode++;
                        break;
                    case 6:
                        i = 100;
                        break;
                }
            }
            else
                break;  
        }

        //draw lines on detected lines on image
        if (SaveManager.Instance.SaveEnabled)
        {
            //invert pixels
            Cv2.BitwiseNot(workingMat, workingMat);
            //make image color
            if (workingMat.Type().Channels == 1)
                Cv2.CvtColor(workingMat, workingMat, ColorConversionCodes.GRAY2BGR);
            //draw lines and intersections on picture
            foreach (var line in lines.AllLines)
                Cv2.Line(workingMat, line.P1, line.P2, Scalar.BlueViolet, 2, LineTypes.AntiAlias);
            foreach (var intersection in lines.Intersections)
                Cv2.Circle(workingMat, intersection, 5, Scalar.Red, 2);
            //save image
            SaveManager.ImageSaverFromTexture2D(OpenCvSharp.Unity.MatToTexture(workingMat), Paths.Instance.TableFilePath, $"Lines_table_t{m_minThreshold}_l{m_minLineLength}_g{m_maxLineGap}");
        }

        ResetHoughLinesParameters(HoughLinesParameter.AllParameters);

        if (lines.Intersections.Count != expectedNumberOfInterscections)
            return false;          

        Dictionary<int, List<Point2f>> srcPoints = new Dictionary<int, List<Point2f>>();
        //get 4 points of every cell
        for (int i = 0, j = 0; i < table.Columns * table.Rows; i++, j++)
        {
            if (i % table.Columns == 0 && i != 0)
                j++;

            srcPoints.Add(i, new List<Point2f>
            {
                new Point2f(lines.Intersections[j].X + m_offset, lines.Intersections[j].Y + m_offset),
                new Point2f(lines.Intersections[j + 1].X - m_offset, lines.Intersections[j + 1].Y + m_offset),
                new Point2f(lines.Intersections[j + table.Columns + 2].X - m_offset, lines.Intersections[j + table.Columns + 2].Y - m_offset),
                new Point2f(lines.Intersections[j + table.Columns + 1].X + m_offset, lines.Intersections[j + table.Columns + 1].Y - m_offset)
            });
        }

        // Remove open answers rows
        for (int i = 0; i < table.Columns * table.OpenAnswersRows; i++)
            srcPoints.Remove(srcPoints.Count - 1);
        // Remove numbers column
        for (int i = 0, c = srcPoints.Count; i < c; i++)
            if (i % table.Columns == 0)
                srcPoints.Remove(i);
        // Remove answers row
        for (int i = 0; i <= table.AnswersColumns; i++)
            srcPoints.Remove(i);

        List<Point2f> dstPoints = new List<Point2f>
        {
            new Point2f(0, 0),
            new Point2f(100, 0),
            new Point2f(100, 100),
            new Point2f(0, 100)
        };

        AnswersManager.Instance.ResetOldAnswers();
        int count = 0;
        //save each cell individually
        foreach (var cell in srcPoints)
        {
            Mat M = Cv2.GetPerspectiveTransform(cell.Value.ToArray(), dstPoints.ToArray());
            Mat warpedMat = new Mat();
            Cv2.WarpPerspective(mainMat, warpedMat, M, new Size(100, 100));
            SaveManager.ImageSaverFromTexture2D(OpenCvSharp.Unity.MatToTexture(warpedMat), Paths.Instance.CellsFilePath, "Cell_" + count++.ToString());
            AnswersManager.Instance.TestAnswers.SetImage(warpedMat);
        }
        string answers = new string(AnswersManager.Instance.TestAnswers.CheckAnswers().ToArray());
        GetComponent<ImageView>().SetAnswersText(answers);
        return true;
    }
}
