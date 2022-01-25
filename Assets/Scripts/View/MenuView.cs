using UnityEngine;
using UnityEngine.UI;

public class MenuView : MonoBehaviour
{
    private void OnEnable()
    {
        AnswersManager.Instance.TestAnswers = new Answers();
        CameraController.Instance.Image = null;
        StudentsManager.Instance.RemoveStudents();

        GetComponentInChildren<Toggle>().onValueChanged.AddListener(delegate { SaveManager.Instance.SaveEnabled = GetComponentInChildren<Toggle>().isOn; });
    }
}
