using UnityEngine;
using UnityEngine.UI;

public class CameraController : SceneSingleton<CameraController>
{
    public WebCamTexture Camera;
    public Texture2D Image;

    [SerializeField] private RawImage m_background;

    public void PausePlayCamera(bool condition)
    {
        if (!condition)
        {
            Camera.Pause();
        }
        else
        { 
            if (Camera != null)
                Camera.Play();
            else
                StartCamera();
        }   
    }

    public void TurnOffOnCamera(bool condition)
    {
        if (!condition)
            Camera.Stop();
        else
            StartCamera();
    }
    
    private void StartCamera()
    {
        WebCamDevice[] devices = WebCamTexture.devices;

        if (devices.Length == 0)
        {
            Debug.Log("No camera detected");
            return;
        }

        for (int i = 0; i < devices.Length; i++)
            if (!devices[i].isFrontFacing)
                Camera = new WebCamTexture();

        if (Camera == null)
        {
            Debug.Log("No back camera detected and using frontfacing camera");
            for (int i = 0; i < devices.Length; i++)
                if (devices[i].isFrontFacing)
                    Camera = new WebCamTexture();
        }

        Camera.Play();
        m_background.texture = Camera;
        if (Camera.height < Camera.width)
        {
            float width = m_background.rectTransform.rect.width;
            float height = m_background.rectTransform.rect.height;
            m_background.transform.localScale = new Vector3(height / width, width / height, 1f);
            m_background.transform.localEulerAngles = new Vector3(0, 0, -90);
        }
    }
}
