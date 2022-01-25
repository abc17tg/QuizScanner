using UnityEngine;
using UnityEngine.UI;

public class ImageView : MonoBehaviour
{
    [SerializeField] private GameObject m_androidCameraObject;
    [SerializeField] private RawImage m_image;
    [SerializeField] private Text m_answers;

    private void OnEnable()
    {
        if (CameraController.Instance.Image == null)
        {
            m_androidCameraObject.SetActive(true);
            CameraController.Instance.TurnOffOnCamera(true);
            gameObject.SetActive(false);
            return;
        }

        m_image.texture = CameraController.Instance.Image;
        GetComponent<ImageExtraction>().GetTable(OpenCvSharp.Unity.TextureToMat((Texture2D)m_image.texture));
    }

    public void SetAnswersText(string text) => m_answers.text = text;
}
