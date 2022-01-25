using UnityEngine;
using UnityEngine.UI;

public class SetupScreen2Controller : MonoBehaviour
{
    [SerializeField] private InputField m_studentIdField;
    [SerializeField] private Button m_startCameraButton;
    [SerializeField] private Button m_endButton;

    private void OnEnable()
    {
        m_studentIdField.text = "";
        m_startCameraButton.interactable = false;
        CheckInputsFields();
        if (StudentsManager.Instance.Students.Count > 0)
            m_endButton.interactable = true;
        m_startCameraButton.onClick.AddListener(delegate { StudentsManager.Instance.AddStudent(m_studentIdField.text); });
    }

    private void Start()
    {
        SetInputsFields();
        m_endButton.onClick.AddListener(AnswersManager.Instance.SaveResults);
    }

    public void CheckInputsFields()
    {
        if (m_studentIdField.text.Length > 0)
            m_startCameraButton.interactable = true;
        else
            m_startCameraButton.interactable = false;
    }

    private void SetInputsFields()
    {
        m_studentIdField.keyboardType = TouchScreenKeyboardType.Default;
        m_studentIdField.characterLimit = 35;
    }
}
