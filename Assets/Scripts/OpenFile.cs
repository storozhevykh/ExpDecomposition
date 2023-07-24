using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;
using TMPro;
using Crosstales.FB;

public class OpenFile : MonoBehaviour
{
    //[SerializeField] private GameObject FileBrowser;
    [SerializeField] private GameObject Open_File_btn;
    [SerializeField] private Image DotImage;
    [SerializeField] private GameObject ParentPanel;
    [SerializeField] private TextMeshProUGUI X_Label_1;
    [SerializeField] private TextMeshProUGUI X_Label_2;
    [SerializeField] private TextMeshProUGUI X_Label_3;
    [SerializeField] private TextMeshProUGUI Y_Label_1;
    [SerializeField] private TextMeshProUGUI Y_Label_2;
    [SerializeField] private TextMeshProUGUI Y_Label_3;
    [SerializeField] private TextMeshProUGUI FileName_Label;
    [SerializeField] private TMP_InputField ExcludeFirstPoints_field;
    [SerializeField] private TMP_InputField ExcludeLastPoints_field;
    [SerializeField] private TMP_Dropdown Channel_dropdown;
    [SerializeField] private TMP_Dropdown Sign_dropdown;
    private bool startParsingPhoto = false;
    private bool startParsingKatodo = false;
    private bool startCollecting = false;
    private bool stopSearchForMaxSignal = false;
    private float time;
    private float signal;
    private float maxSignal = 0f;
    private float minSignal = 99999999f;
    private float maxTime = 0f;
    private float maxSignalTime = 0f;
    private float prevSignal = 0f;
    private float prevTime = 0f;
    private string[] sArr;
    private string timeStr;
    private string signalStr;
    private int averageCounter = 0;
    private int afterMaxSignalCounter = 0;
    private float timeTemp;
    private float signalTemp;
    private float screen_widthCoeff;
    private float screen_heightCoeff;
    private ExpDecomposition expDecomposition;
    private int AveragingPointsNum = 10;
    private bool firstPoint;
    private float startTime;
    private float initialTime;
    private int pointsNum = 0;
    private int AveragingPointsNumCoeff;
    private int channel;
    private bool firstString;
    // Start is called before the first frame update
    void Start()
    {
        expDecomposition = GetComponent<ExpDecomposition>();
        channel = Channel_dropdown.value + 1;
    }

    public void OpenDialog()
    {
        //EditorUtility.DisplayDialog("Choose File", "You must select a texture first!", "OK");
        print("StaticStorage.dotsList.Count = " + StaticStorage.dotsList.Count);

        screen_widthCoeff = Screen.width / 1920f;
        screen_heightCoeff = Screen.height / 1080f;
        print("screen_heightCoeff = " + screen_heightCoeff);

        string path = FileBrowser.Instance.OpenSingleFile("csv");

        if (StaticStorage.dotsList.Count > 0)
        {
            StaticStorage.timeList.Clear();
            StaticStorage.signalList.Clear();
            foreach (Image element in StaticStorage.dotsList)
            {
                Destroy(element.gameObject);
            }
            StaticStorage.dotsList.Clear();

            expDecomposition.DeleteCurve(StaticStorage.sum_List);
            expDecomposition.DeleteCurve(StaticStorage.exp1_List);
            expDecomposition.DeleteCurve(StaticStorage.exp2_List);
            expDecomposition.DeleteCurve(StaticStorage.exp3_List);
            expDecomposition.LifeTime_Bar_1.gameObject.SetActive(false);
            expDecomposition.LifeTime_Bar_2.gameObject.SetActive(false);
            expDecomposition.LifeTime_Bar_3.gameObject.SetActive(false);
            expDecomposition.LifeTime_Text_1.gameObject.SetActive(false);
            expDecomposition.LifeTime_Text_2.gameObject.SetActive(false);
            expDecomposition.LifeTime_Text_3.gameObject.SetActive(false);
            expDecomposition.LifeTime_Title.gameObject.SetActive(false);
            expDecomposition.Amplitude_Text_1.gameObject.SetActive(false);
            expDecomposition.Amplitude_Text_2.gameObject.SetActive(false);
            expDecomposition.Amplitude_Text_3.gameObject.SetActive(false);
            expDecomposition.Amplitude_Title.gameObject.SetActive(false);
            expDecomposition.LifeTime_Text_1.rectTransform.localPosition = expDecomposition.LT_Text_initialPos_1;
            expDecomposition.LifeTime_Text_2.rectTransform.localPosition = expDecomposition.LT_Text_initialPos_2;
            expDecomposition.LifeTime_Text_3.rectTransform.localPosition = expDecomposition.LT_Text_initialPos_3;
            StaticStorage.signalExp_1_1.Clear();
            StaticStorage.signalExp_2_1.Clear();
            StaticStorage.signalExp_2_2.Clear();
            StaticStorage.signalExp_3_1.Clear();
            StaticStorage.signalExp_3_2.Clear();
            StaticStorage.signalExp_3_3.Clear();
            StaticStorage.signalSum_2.Clear();
            StaticStorage.signalSum_3.Clear();
        }

        if (path.Length != 0)
        {
            StaticStorage.allStrings = File.ReadAllLines(path);
            print(StaticStorage.allStrings[0]);
            print(StaticStorage.allStrings[1]);

            StaticStorage.fileName = Path.GetFileNameWithoutExtension(path);
            FileName_Label.text = StaticStorage.fileName;
            //Open_File_btn.SetActive(false);
            firstString = true;
            ParseStrings();
            WriteAxisLabels();
            ExcludeFirstPoints_field.text = "0";
            ExcludeLastPoints_field.text = "0";
        }
    }

    private void ParseStrings()
    {
        int length = StaticStorage.allStrings.Length;
        startParsingPhoto = false;
        startParsingKatodo = false;
        startCollecting = false;
        stopSearchForMaxSignal = false;
        averageCounter = 0;
        afterMaxSignalCounter = 0;
        maxSignal = 0f;
        minSignal = 99999999f;
        maxTime = 0f;
        prevSignal = 0f;
        prevTime = 0f;
        time = 0f;
        signal = 0f;
        initialTime = 0f;
        pointsNum = 0;

        foreach (string s in StaticStorage.allStrings)
        {
            if (s.Contains("TIME,CH1") || s.Contains("TIME,CH2"))
            {
                startParsingPhoto = true;
                continue;
            }

            if (s.Contains("Firmware Version"))
            {
                startParsingKatodo = true;
                continue;
            }

            if (s == "")
            {
                startParsingPhoto = false;
                startParsingKatodo = false;
            }

            if (!startParsingPhoto && !startParsingKatodo) continue;

            sArr = s.Split(";");

            if (startParsingKatodo)
            {
                if (sArr.Length == 1)
                {
                    sArr = sArr[0].Replace(",,,", "").Replace(" ", "").Split(",");
                }
            }

            if (startParsingPhoto)
            {
                if (sArr.Length == 1)
                {
                    sArr = sArr[0].Split(",");
                }
            }

            //print("sArr[0] = " + sArr[0] + ", sArr[1] = " + sArr[1]);

            pointsNum++;
            timeStr = sArr[0].Replace(".", ",").Replace("e", "E");
            signalStr = sArr[channel].Replace(".", ",").Replace("inf", (maxSignal/1000).ToString());
            if (Sign_dropdown.value == 0) signalTemp = -float.Parse(signalStr);
            else signalTemp = float.Parse(signalStr);
            //if (channel == 2) signalTemp = -signalTemp;
            signal += signalTemp;
            time = float.Parse(timeStr) - initialTime;
            if (firstString && startParsingKatodo)
            {
                initialTime = time;
                firstString = false;
            }
            averageCounter++;
            afterMaxSignalCounter++;
            if (signalTemp * 1000 > maxSignal && !stopSearchForMaxSignal)
            {
                maxSignal = signalTemp * 1000;
                maxSignalTime = time;
                afterMaxSignalCounter = 0;
            }
            if (afterMaxSignalCounter >= 3000) stopSearchForMaxSignal = true;
            if (averageCounter == 50)
            {
                if (signal / averageCounter < minSignal && time > 0.0000003f)
                {
                    minSignal = signal / averageCounter;
                }
                averageCounter = 0;
                signal = 0f;
            }
        }

        maxTime = time - maxSignalTime;
        maxSignal -= minSignal * 1000;

        StaticStorage.X_Scale = (1600 / maxTime) * screen_widthCoeff;
        StaticStorage.Y_Scale = (828000 / maxSignal) * screen_heightCoeff;

        StaticStorage.maxSignal = (maxSignal / 1000);
        StaticStorage.maxTime = maxTime;

        print("maxSignal time = " + maxSignalTime);
        print("maxSignal = " + StaticStorage.maxSignal);
        print("minSignal = " + minSignal);
        print("Y_Scale = " + StaticStorage.Y_Scale);
        print("maxTime = " + maxTime);

        //X_Scale = 3000000;

        startParsingPhoto = false;
        startParsingKatodo = false;
        time = 0f;
        signal = 0f;
        averageCounter = 0;
        firstPoint = true;
        startTime = 0f;
        AveragingPointsNumCoeff = Mathf.RoundToInt(pointsNum / 10000);
        if (AveragingPointsNumCoeff < 1) AveragingPointsNumCoeff = 1;

        foreach (string s in StaticStorage.allStrings)
        {
            if (s.Contains("TIME,CH1") || s.Contains("TIME,CH2"))
            {
                startParsingPhoto = true;
                continue;
            }

            if (s.Contains("Firmware Version"))
            {
                startParsingKatodo = true;
                continue;
            }

            if (s == "")
            {
                startParsingPhoto = false;
                startParsingKatodo = false;
            }

            if (!startParsingPhoto && !startParsingKatodo) continue;

            sArr = s.Split(";");

            if (startParsingKatodo)
            {
                if (sArr.Length == 1)
                {
                    sArr = sArr[0].Replace(",,,", "").Replace(" ", "").Split(",");
                }
            }

            if (startParsingPhoto)
            {
                if (sArr.Length == 1)
                {
                    sArr = sArr[0].Split(",");
                }
            }

            timeStr = sArr[0].Replace(".", ",").Replace("e", "E");
            signalStr = sArr[channel].Replace(".", ",").Replace("inf", (maxSignal / 1000).ToString());
            timeTemp = float.Parse(timeStr) - initialTime;
            if (startParsingPhoto) timeTemp -= maxSignalTime;
            //print("timeTemp = " + timeTemp);
            if (Sign_dropdown.value == 0) signalTemp = -float.Parse(signalStr) - minSignal;
            else signalTemp = float.Parse(signalStr) - minSignal;
            //if (channel == 2) signalTemp = -signalTemp;
            if (timeTemp < 0) continue;

            //print("signalTemp = " + (signalTemp * 1000) + ", maxSignal = " + maxSignal);
            //print("sArr[channel] = " + sArr[channel]);

            if (signalTemp * 1000 >= maxSignal - 0.1f || sArr[channel].Contains("inf"))
            {
                startCollecting = true;
                print("start collecting time = " + timeTemp);
            }

            if (!startCollecting) continue;

            if (timeTemp < 0.0000003f) AveragingPointsNum = 1 * AveragingPointsNumCoeff;
            else if (timeTemp < 0.000001f) AveragingPointsNum = 5 * AveragingPointsNumCoeff;
            else AveragingPointsNum = 10 * AveragingPointsNumCoeff;

            if (startParsingKatodo) AveragingPointsNum = 1;



            time = time + timeTemp;
            signal = signal + signalTemp;
            averageCounter++;

            //print("averageCounter = " + averageCounter + ", AveragingPointsNum = " + AveragingPointsNum);

            if (averageCounter == AveragingPointsNum)
            {
                time /= AveragingPointsNum;
                signal /= AveragingPointsNum;
                averageCounter = 0;
                time -= startTime;
                //print("time = " + time);
                if ((signal < prevSignal || prevSignal == 0f) && time > prevTime)
                {
                    if (firstPoint)
                    {
                        startTime = time;
                        firstPoint = false;
                        time = 0f;
                        maxTime -= startTime;
                        StaticStorage.X_Scale = (1600 / maxTime) * screen_widthCoeff;
                    }
                    StaticStorage.timeList.Add(time);
                    StaticStorage.signalList.Add(signal);
                    //print(time.ToString());
                    //StaticStorage.dotsList.Add(GameObject.Instantiate(DotImage, new Vector2(time * StaticStorage.X_Scale + 150 * screen_widthCoeff, signal * StaticStorage.Y_Scale + 120 * screen_heightCoeff), Quaternion.identity, ParentPanel.transform));
                    if (prevSignal == 0f)
                    {
                        print("signal = " + signal);
                        print("signal * Y_Scale = " + signal * StaticStorage.Y_Scale);
                    }
                }
                prevSignal = signal;
                prevTime = time;
                time = 0;
                signal = 0;
            }
        }
        ExcludePoints();
    }

    private void WriteAxisLabels()
    {
        X_Label_1.text = "0,00";
        X_Label_2.text = (maxTime * 1000000 / 2).ToString("#0.00");
        X_Label_3.text = (maxTime * 1000000).ToString("#0.00");
        Y_Label_1.text = "0,000";
        Y_Label_2.text = (maxSignal / 2000).ToString("#0.000");
        Y_Label_3.text = (maxSignal / 1000).ToString("#0.000");
    }

    public void ExcludePoints()
    {
        print("StaticStorage.timeList.Count: " + StaticStorage.timeList.Count);
        int numFirst = int.Parse(ExcludeFirstPoints_field.text);
        int numLast = int.Parse(ExcludeLastPoints_field.text);
        //if (numFirst == 0 && numLast == 0) return;
        List<float> timeList_Temp = new List<float>();
        List<float> signalList_Temp = new List<float>();
        minSignal = 99999999f;
        averageCounter = 0;
        signalTemp = 0f;

        for (int i = numFirst; i < StaticStorage.timeList.Count - numLast; i++)
        {
            averageCounter++;
            timeList_Temp.Add(StaticStorage.timeList[i] - StaticStorage.timeList[numFirst]);
            signalList_Temp.Add(StaticStorage.signalList[i]);
            signalTemp += StaticStorage.signalList[i];
            if (averageCounter == 5)
            {
                signalTemp /= 5;
                if (signalTemp < minSignal)
                {
                    minSignal = signalTemp;
                }
                signalTemp = 0;
                averageCounter = 0;
            }
        }

        for (int i = 0; i < signalList_Temp.Count; i++)
        {
            signalList_Temp[i] -= minSignal;
        }

        maxTime = timeList_Temp[timeList_Temp.Count - 1];
        StaticStorage.maxTime = maxTime;
        StaticStorage.X_Scale = (1600 / maxTime) * screen_widthCoeff;
        maxSignal = signalList_Temp[0];
        StaticStorage.maxSignal = maxSignal;
        StaticStorage.Y_Scale = (820 / maxSignal) * screen_heightCoeff;
        StaticStorage.timeList = timeList_Temp;
        StaticStorage.signalList = signalList_Temp;

        expDecomposition.DeleteCurve(StaticStorage.dotsList);
        expDecomposition.DeleteCurve(StaticStorage.sum_List);
        expDecomposition.DeleteCurve(StaticStorage.exp1_List);
        expDecomposition.DeleteCurve(StaticStorage.exp2_List);
        expDecomposition.DeleteCurve(StaticStorage.exp3_List);
        StaticStorage.signalExp_1_1.Clear();
        StaticStorage.signalExp_2_1.Clear();
        StaticStorage.signalExp_2_2.Clear();
        StaticStorage.signalExp_3_1.Clear();
        StaticStorage.signalExp_3_2.Clear();
        StaticStorage.signalExp_3_3.Clear();
        StaticStorage.signalSum_2.Clear();
        StaticStorage.signalSum_3.Clear();

        for (int i = 0; i < StaticStorage.timeList.Count; i++)
        {
            time = StaticStorage.timeList[i];
            signal = StaticStorage.signalList[i];
            StaticStorage.dotsList.Add(GameObject.Instantiate(DotImage, new Vector2(time * StaticStorage.X_Scale + 150 * screen_widthCoeff, signal * StaticStorage.Y_Scale + 120 * screen_heightCoeff), Quaternion.identity, ParentPanel.transform));
        }

        CalcTail();
        maxSignal *= 1000;
        WriteAxisLabels();
    }

    public void ChangeChannel()
    {
        channel = Channel_dropdown.value + 1;
    }

    private void CalcTail()
    {
        float tailTime = maxTime * 0.8f;
        float tailSignal = 0f;
        for (int i = 0; i < StaticStorage.timeList.Count; i++)
        {
            time = StaticStorage.timeList[i];
            if (time >= tailTime)
            {
                for (int j = i - 5; j <= i + 5; j++)
                {
                    tailSignal += StaticStorage.signalList[j];
                }
                tailSignal /= 10;
                break;
            }
        }
        StaticStorage.tailSignal = tailSignal;
    }
}
