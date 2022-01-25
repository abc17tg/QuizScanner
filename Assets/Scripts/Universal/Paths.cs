using UnityEngine;
using System.IO;
using System;

public class Paths : SceneSingleton<Paths>
{
    [HideInInspector] public string FilesPath;
    [HideInInspector] public string PaperFilePath;
    [HideInInspector] public string PhotoFilePath;
    [HideInInspector] public string ResultsFilePath;
    [HideInInspector] public string CellsFilePath;
    [HideInInspector] public string TableFilePath;

    private void OnEnable()
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            FilesPath = ChangeDirSep(Application.persistentDataPath + "/");
            CreateDirectory(FilesPath + "QuizScanner");
            FilesPath += ChangeDirSep("QuizScanner/");
        }
        else
        {
            FilesPath = Application.dataPath + "/";
            CreateDirectory(FilesPath + "Graphics");
            FilesPath += ChangeDirSep("Graphics/"); ;
        }

        PaperFilePath = FilesPath + ChangeDirSep("/Paper/");
        CreateDirectory(FilesPath + "Paper");
        PhotoFilePath = FilesPath + ChangeDirSep("/Photos/");
        CreateDirectory(FilesPath + "Photos");
        CellsFilePath = FilesPath + ChangeDirSep("Cells/");
        CreateDirectory(FilesPath + "Cells");
        TableFilePath = FilesPath + ChangeDirSep("Table/");
        CreateDirectory(FilesPath + "Table");
        ResultsFilePath = FilesPath + ChangeDirSep("Results/");
        CreateDirectory(FilesPath + "Results");
    }

    public static string ChangeDirSep(string path, string separator = "/") =>
        path.Replace(separator, Path.DirectorySeparatorChar.ToString());

    public static void CreateDirectory(string path)
    {
        if (Directory.Exists(path))
            return;
        Directory.CreateDirectory(path);
    }

    public static FileStream CreateFileWithDateInName(string path, string name, string extension, bool attachDate)
    {
        CreateDirectory(path);

        if (attachDate)
            name += "_" + DateTime.Now.Day + "-" + DateTime.Now.Month + "-" + DateTime.Now.Year + "_" + DateTime.Now.Hour + "-" + DateTime.Now.Minute;
        path += name + "." + extension;

        if (File.Exists(path))
            return File.OpenWrite(path);
        return File.Create(path);
    }
}
