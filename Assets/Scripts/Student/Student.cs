using System.Collections.Generic;

public class Student
{
    private string m_id;
    private List<char> m_answers = new List<char>();

    public string Id => m_id;
    public List<char> Answers => m_answers;

    public Student(string id) => m_id = id;

    public void SetAnswers(List<char> ans) => m_answers = ans;
}
