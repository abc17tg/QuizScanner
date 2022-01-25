using OpenCvSharp;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Answers
{
    private List<Mat> m_cellsImages = new List<Mat>();
    private List<int> m_cellsKPixCount = new List<int>();

    public List<char> FinalAnswers = new List<char>();

    public void SetImage(Mat mat) => m_cellsImages.Add(mat);

    public List<char> CheckAnswers()
    {
        foreach (var img in m_cellsImages)
        {         
            Cv2.Threshold(img, img, 0, 255, ThresholdTypes.Binary);
            m_cellsKPixCount.Add((m_cellsImages.First().Height * m_cellsImages.First().Width) - Cv2.CountNonZero(img));
        }

        // Make list of chunks of list equal to number of columns
        List<List<int>> allAnswers = Utils.ChunkBy(m_cellsKPixCount, SettingsMenu.Instance.TableParams.AnswersColumns);

        foreach (var answers in allAnswers)
            FinalAnswers.Add(IntToCharAns(CompareCells(answers)));

#if (UNITY_EDITOR)
        string tempString = "";
        foreach (var ans in FinalAnswers)
            tempString += ans.ToString() + " ";
        Debug.Log("Answers: " + tempString);
#endif

        StudentsManager.Instance.CurrentStudent.SetAnswers(FinalAnswers);
        return FinalAnswers;
    }

    private int CompareCells(List<int> cellsKPixCount)
    {
        List<int> cellsKPixCountOrd = cellsKPixCount.OrderByDescending(p => p).ToList();
        if (!(((float)cellsKPixCountOrd.First() / (m_cellsImages.First().Height * m_cellsImages.First().Width)) > 0.05f))
            return -20; // ASCII '-' symbol number
        if ((float)cellsKPixCountOrd[1] / cellsKPixCountOrd.First() > 0.80)
            return cellsKPixCount.IndexOf(cellsKPixCountOrd.First()) + 32; // ASCII to small letters offset +32
        return cellsKPixCount.IndexOf(cellsKPixCountOrd.First());
    }

    private char IntToCharAns(int ans) => (char)(ans + 65); // ASCII to letters offset +65


}
