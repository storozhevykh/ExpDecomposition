using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Crosstales.FB;
using System;
using System.IO;

public class SaveData : MonoBehaviour
{
    [SerializeField] private GameObject[] ExcludedObjects;
    private string path;
    // Start is called before the first frame update
    void Start()
    {
        
    }
    public void MakeScreenshot()
    {
        path = FileBrowser.Instance.SaveFile(StaticStorage.fileName + "_decomposed_by" + "_" + (StaticStorage.Mode + 1) + "_exp", "png");
        StartCoroutine(MakingScreenshot());
    }

    IEnumerator MakingScreenshot()
    {
        foreach (GameObject obj in ExcludedObjects) obj.SetActive(false);
        yield return new WaitForSeconds(0.1f);
        ScreenCapture.CaptureScreenshot(path);
        yield return new WaitForSeconds(0.1f);
        foreach (GameObject obj in ExcludedObjects) obj.SetActive(true);
    }
    public void SaveCSV()
    {
        path = FileBrowser.Instance.SaveFile(StaticStorage.fileName + "_decomposed_by" + "_" + (StaticStorage.Mode + 1) + "_exp", "csv");
        if (path == "") return;
        if (File.Exists(path)) File.Delete(path);
        StreamWriter writer = new StreamWriter(path, true);
        for (int i = 0; i < StaticStorage.timeList.Count; i++)
        {
            switch (StaticStorage.Mode)
            {
                case 0:
                    writer.WriteLine(StaticStorage.timeList[i].ToString() + ";" + StaticStorage.signalList[i].ToString() + ";" + StaticStorage.signalExp_1_1[i].ToString());
                    break;

                case 1:
                    writer.WriteLine(StaticStorage.timeList[i].ToString() + ";" + StaticStorage.signalList[i].ToString() + ";" + 
                        StaticStorage.signalExp_2_1[i].ToString() + ";" + StaticStorage.signalExp_2_2[i].ToString() + ";" + StaticStorage.signalSum_2[i].ToString());
                    break;

                case 2:
                    writer.WriteLine(StaticStorage.timeList[i].ToString() + ";" + StaticStorage.signalList[i].ToString() + ";" +
                        StaticStorage.signalExp_3_1[i].ToString() + ";" + StaticStorage.signalExp_3_2[i].ToString() + ";" + StaticStorage.signalExp_3_3[i].ToString() + ";" + StaticStorage.signalSum_3[i].ToString());
                    break;
            }
        }
        
        writer.Close();
    }
}
