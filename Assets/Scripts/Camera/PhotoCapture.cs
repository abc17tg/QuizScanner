using OpenCvSharp;
using UnityEngine;

public class PhotoCapture : MonoBehaviour
{
    [SerializeField] private Texture2D m_samplePhoto;

    public void TakePicture()
    {
        Texture2D snap = new Texture2D(CameraController.Instance.Camera.width, CameraController.Instance.Camera.height);
        snap.SetPixels(CameraController.Instance.Camera.GetPixels());
        snap.Apply();
        if (snap.height < snap.width)
            CameraController.Instance.Image = OpenCvSharp.Unity.MatToTexture(OpenCvSharp.Unity.TextureToMat(snap).Rotate(RotateFlags.Rotate90Clockwise));
        else
            CameraController.Instance.Image = snap;
        SaveManager.ImageSaverFromTexture2D(CameraController.Instance.Image, Paths.Instance.PhotoFilePath, "RawPhoto");
    }

    public void PickPhoto()
    {
        if (NativeFilePicker.IsFilePickerBusy())
            return;

        NativeFilePicker.PickFile(path =>
        {
            if (path == null)
                return;
            
            Texture2D image = new Texture2D(0, 0);
            ImageConversion.LoadImage(image, System.IO.File.ReadAllBytes(Paths.ChangeDirSep(path)));
            CameraController.Instance.Image = image;
            if (CameraController.Instance.Image != null)
                SaveManager.ImageSaverFromTexture2D(CameraController.Instance.Image, Paths.Instance.PhotoFilePath, "RawPhoto");
        }, new string[] { NativeFilePicker.ConvertExtensionToFileType("jpg"), NativeFilePicker.ConvertExtensionToFileType("png") });
    }
}
