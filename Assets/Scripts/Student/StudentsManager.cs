using System.Collections.Generic;
using System.Linq;

public class StudentsManager : SceneSingleton<StudentsManager>
{
    private List<Student> m_students;

    public List<Student> Students => m_students;
    public Student CurrentStudent => m_students.Last();

    public void AddStudent(string id)
    {
        if (id.Length > 0)
            m_students.Add(new Student(id));
    }
    
    public void RemoveCurrentStudent() => m_students.Remove(CurrentStudent);

    public void RemoveStudents() => m_students.Clear();

    override protected void OnAwake()
    {
        m_students = new List<Student>();
        base.OnAwake();
    }
}
