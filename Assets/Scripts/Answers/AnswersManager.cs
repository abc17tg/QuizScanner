using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.IO;

public class AnswersManager : SceneSingleton<AnswersManager>
{
    [HideInInspector] public Answers TestAnswers;

    private List<char> m_correctAnswers = new List<char>();

    public List<char> CorrectAnswers => m_correctAnswers;

    private void OnEnable()
    {
        ResetOldAnswers();
    }

    public void ResetOldAnswers() => TestAnswers = new Answers();

    public bool SetCorrectAnswers(string answersString)
    {
        if (SettingsMenu.Instance.TableParams.ClosedAnswersRows > 0 && SettingsMenu.Instance.TableParams.ClosedAnswersRows == answersString.Length)
        {
            m_correctAnswers = answersString.ToList();
            return true;
        }
        else
        {
            m_correctAnswers.Clear();
            return false;
        }
    }

    public void SaveResults()
    {
        FileStream fs = Paths.CreateFileWithDateInName(Paths.Instance.ResultsFilePath, "results", "txt", true);
        StreamWriter sw = new StreamWriter(fs);

        foreach (var student in StudentsManager.Instance.Students)
        {
            if (!(student.Answers.Count > 0))
                continue;

            string answers = "";
            int count = 1;
            foreach (var ans in student.Answers)
                answers += count++.ToString() + "." + ans.ToString() + " ";

            sw.WriteLine(student.Id + "|" + answers + "|" + PercentageResult(student.Answers, m_correctAnswers).ToString("0.00") + " % ");
        }

        sw.Close();
        NativeFilePicker.ExportFile(fs.Name);
        fs.Close();
    }

    private float PercentageResult(List<char> answers, List<char> correctAnswers)
    {
        int count = 0;
        for (int i = 0; i < answers.Count; i++)
            if (answers[i] == correctAnswers[i])
                count++;
        return (float)count / answers.Count * 100f;
    }
}
