using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class SetupScreen1Controller : MonoBehaviour
{
    [SerializeField] private List<InputField> m_inputFields;
    [SerializeField] private InputField m_correctAnsField;
    [SerializeField] private Button m_nextButton;

    private string m_acceptedAnswersOptions = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

    private void OnEnable()
    {
        foreach (var input in m_inputFields)
            input.text = "";
        m_correctAnsField.text = "";
        m_nextButton.interactable = false;
        CheckInputsFields();
    }

    private void Start()
    {
        SetInputsFields();
    }

    public void CheckInputsFields()
    {
        if (m_inputFields.Any(inp => inp.text.Length > 0))
        {
            bool b = AnswersManager.Instance.SetCorrectAnswers(m_correctAnsField.text);
            int c = m_correctAnsField.text.ToList().Distinct().Count();
            SettingsMenu.Instance.TableParams = new Table(ParseStringToIntInputDecimal(m_inputFields[0].text),
                    ParseStringToIntInputDecimal(m_inputFields[2].text), ParseStringToIntInputDecimal(m_inputFields[1].text));

            if (m_inputFields.All(inp => inp.text.Length > 0) && b && c == int.Parse(m_inputFields[1].text))
            {
                m_nextButton.interactable = true;
                SettingsMenu.Instance.TableParams = new Table(ParseStringToIntInputDecimal(m_inputFields[0].text),
                    ParseStringToIntInputDecimal(m_inputFields[2].text), ParseStringToIntInputDecimal(m_inputFields[1].text));
            }
            else
                m_nextButton.interactable = false;
        }
        else if (AnswersManager.Instance.SetCorrectAnswers(m_correctAnsField.text))
        {
            m_nextButton.interactable = true;
            SettingsMenu.Instance.TableParams = new Table();
        }
        else
            m_nextButton.interactable = false;
    }

    private int ParseStringToIntInputDecimal(string t)
    {
        if (t == "")
            return 0;
        return int.Parse(t);
    }

    private void CheckAnsField()
    {
        if (m_correctAnsField.text.Length == 0)
            return;

            m_correctAnsField.text = m_correctAnsField.text.ToUpper();
        if (!m_acceptedAnswersOptions.Contains(m_correctAnsField.text.Last()))
            m_correctAnsField.text = m_correctAnsField.text.Remove(m_correctAnsField.text.Length - 1);
    }

    private void SetInputsFields()
    {
        foreach (var input in m_inputFields)
        {
            input.keyboardType = TouchScreenKeyboardType.NumberPad;
            input.characterLimit = 2;
            input.contentType = InputField.ContentType.IntegerNumber;
        }

        m_correctAnsField.keyboardType = TouchScreenKeyboardType.ASCIICapable;
        m_correctAnsField.onValueChanged.AddListener(delegate { CheckAnsField(); });
    }
}
