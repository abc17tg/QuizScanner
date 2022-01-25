using System.IO;
using UnityEngine;
using UnityEngine.Android;

public class SaveManager : SceneSingleton<SaveManager>
{
    public bool SaveEnabled = true;

    void OnEnable()
    {
        Permission.RequestUserPermission(Permission.ExternalStorageRead);
        Permission.RequestUserPermission(Permission.ExternalStorageWrite);
        if (!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageRead) ||
            !Permission.HasUserAuthorizedPermission(Permission.ExternalStorageWrite))
            SaveEnabled = false;
#if (UNITY_EDITOR)
        Debug.Log($"Save enabled: {SaveEnabled}");
#endif
    }

    public static void ImageSaverFromTexture2D(Texture2D texture, string imageFullPath, string imageName, string extension = "jpg")
    {
        if (!Instance.SaveEnabled)
            return;

        string path = imageFullPath + imageName + "." + extension;
        if (extension == "jpg")
            File.WriteAllBytes(path, texture.EncodeToJPG());
        else if (extension == "png")
            File.WriteAllBytes(path, texture.EncodeToPNG());
        else
            return;

#if (UNITY_EDITOR)
        Debug.Log($"Image saved: { path }");
#endif
    }
}
