using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Threading;
using TMPro;

public class ExpDecomposition : MonoBehaviour
{
    [SerializeField] private Image DotImage;
    [SerializeField] private Image DotImage1;
    [SerializeField] private Image DotImage2;
    [SerializeField] private Image DotImage3;
    public TextMeshProUGUI LifeTime_Title;
    public TextMeshProUGUI Amplitude_Title;
    public Image LifeTime_Bar_1;
    public Image LifeTime_Bar_2;
    public Image LifeTime_Bar_3;
    public TextMeshProUGUI LifeTime_Text_1;
    public TextMeshProUGUI LifeTime_Text_2;
    public TextMeshProUGUI LifeTime_Text_3;
    public TextMeshProUGUI Amplitude_Text_1;
    public TextMeshProUGUI Amplitude_Text_2;
    public TextMeshProUGUI Amplitude_Text_3;
    [SerializeField] private Image DotImage_Sum;
    [SerializeField] private LineRenderer Sum_Line;
    [SerializeField] private GameObject ParentPanel;
    [SerializeField] private GameObject ProgressPanel;
    [SerializeField] private Image ProgressImage_Rough;
    [SerializeField] private Image ProgressImage_Precise;
    [SerializeField] private Toggle addConstant_Toggle;

    private float screen_widthCoeff;
    private float screen_heightCoeff;

    float[] deviation = { 0f,0f,0f,0f };
    float minDeviation = 0f;
    float[] minDeviationArr = { 0f, 0f, 0f, 0f };
    float best_A1 = 0f;
    float best_k1 = 0f;
    float best_A2 = 0f;
    float best_k2 = 0f;
    float best_A3 = 0f;
    float best_k3 = 0f;
    float best_C = 0f;
    float[] best_A1_arr = { 0f, 0f, 0f, 0f };
    float[] best_k1_arr = { 0f, 0f, 0f, 0f };
    float[] best_A2_arr = { 0f, 0f, 0f, 0f };
    float[] best_k2_arr = { 0f, 0f, 0f, 0f };
    float[] best_A3_arr = { 0f, 0f, 0f, 0f };
    float[] best_k3_arr = { 0f, 0f, 0f, 0f };
    float[] best_C_arr = { 0f, 0f, 0f, 0f };
    Thread[] myThread = new Thread[4];
    Thread myThread1;
    float fillStep;
    float fillAmount_Rough;
    float fillAmount_Precise;
    int finished;
    bool roughCalc, preciseCalc;
    int Mode;
    float Amin1, Amax1, Astep1, kmin1, kmax1, kstep1, Amin2, Amax2, Astep2, kmin2, kmax2, kstep2, Amin3, Amax3, Astep3, kmin3, kmax3, kstep3;
    float Cmin, Cmax, Cstep;
    float A1_Delta, A2_Delta, A3_Delta;
    public Vector2 LT_Text_initialPos_1, LT_Text_initialPos_2, LT_Text_initialPos_3;
    private int ThreadsNum = 4;

    public delegate void ParameterizedThreadStart(float A_min, float A_step, float A_max, float k_min, float k_step, float k_max);

    // Start is called before the first frame update
    void Start()
    {
        LT_Text_initialPos_1 = LifeTime_Text_1.rectTransform.localPosition;
        LT_Text_initialPos_2 = LifeTime_Text_2.rectTransform.localPosition;
        LT_Text_initialPos_3 = LifeTime_Text_3.rectTransform.localPosition;
    }

    public void Calculate(int mode)
    {
        Mode = mode;
        StaticStorage.Mode = mode;
        screen_widthCoeff = Screen.width / 1920f;
        screen_heightCoeff = Screen.height / 1080f;

        minDeviation = 0f;

        Cmin = -StaticStorage.tailSignal * 1;
        Cmax = StaticStorage.tailSignal * 1;
        Cstep = (Cmax - Cmin) / 10;

        switch (mode)
        {
            case 0:
                Amin1 = 0f;
                Amax1 = StaticStorage.maxSignal;
                Astep1 = (Amax1 - Amin1) / 120;
                kmin1 = -1f / (StaticStorage.maxTime / 200);
                kmax1 = -1f / (StaticStorage.maxTime);
                kstep1 = (kmax1 - kmin1) / 300;
                break;

            case 1:
                Amin1 = 0f;
                Amax1 = StaticStorage.maxSignal;
                Astep1 = (Amax1 - Amin1) / 8;
                kmin1 = -1f / (StaticStorage.maxTime / 20);
                kmax1 = -1f / (StaticStorage.maxTime);
                kstep1 = (kmax1 - kmin1) / 20;
                Amin2 = 0f;
                Amax2 = StaticStorage.maxSignal;
                Astep2 = (Amax2 - Amin2) / 10;
                kmin2 = -1f / (StaticStorage.maxTime / 400);
                kmax2 = -1f / (StaticStorage.maxTime / 10);
                kstep2 = (kmax2 - kmin2) / 40;
                break;

            case 2:
                Amin1 = 0f;
                Amax1 = StaticStorage.maxSignal;
                Astep1 = (Amax1 - Amin1) / 4;
                kmin1 = -1f / (StaticStorage.maxTime / 8);
                kmax1 = -1f / (StaticStorage.maxTime);
                kstep1 = (kmax1 - kmin1) / 10;
                Amin2 = 0f;
                Amax2 = StaticStorage.maxSignal;
                Astep2 = (Amax2 - Amin2) / 5;
                kmin2 = -1f / (StaticStorage.maxTime / 40);
                kmax2 = -1f / (StaticStorage.maxTime / 8);
                kstep2 = (kmax2 - kmin2) / 10;
                Amin3 = 0f;
                Amax3 = StaticStorage.maxSignal;
                Astep3 = (Amax3 - Amin3) / 5;
                kmin3 = -1f / (StaticStorage.maxTime / 400);
                kmax3 = -1f / (StaticStorage.maxTime / 40);
                kstep3 = (kmax3 - kmin3) / 20;
                break;
        }

        StartCoroutine(Progress());
    }
    IEnumerator Progress()
    {
        ProgressImage_Rough.fillAmount = 0;
        ProgressImage_Precise.fillAmount = 0;
        ProgressPanel.SetActive(true);

        DeleteCurve(StaticStorage.sum_List);
        DeleteCurve(StaticStorage.exp1_List);
        DeleteCurve(StaticStorage.exp2_List);
        DeleteCurve(StaticStorage.exp3_List);

        fillAmount_Rough = 0f;
        fillAmount_Precise = 0f;

        LifeTime_Bar_1.gameObject.SetActive(false);
        LifeTime_Bar_2.gameObject.SetActive(false);
        LifeTime_Bar_3.gameObject.SetActive(false);
        LifeTime_Text_1.gameObject.SetActive(false);
        LifeTime_Text_2.gameObject.SetActive(false);
        LifeTime_Text_3.gameObject.SetActive(false);
        LifeTime_Title.gameObject.SetActive(false);
        Amplitude_Text_1.gameObject.SetActive(false);
        Amplitude_Text_2.gameObject.SetActive(false);
        Amplitude_Text_3.gameObject.SetActive(false);
        Amplitude_Title.gameObject.SetActive(false);
        LifeTime_Text_1.rectTransform.localPosition = LT_Text_initialPos_1;
        LifeTime_Text_2.rectTransform.localPosition = LT_Text_initialPos_2;
        LifeTime_Text_3.rectTransform.localPosition = LT_Text_initialPos_3;

        switch (Mode)
        {
            case 0:
                if (StaticStorage.signalExp_1_1.Count > 0)
                {
                    best_k1 = StaticStorage.best_k_1_1;
                    best_A1 = StaticStorage.best_A_1_1;
                    Redraw(StaticStorage.signalExp_1_1, StaticStorage.exp1_List, DotImage1);
                    break;
                }
                preciseCalc = false;
                roughCalc = true;
                A1_Delta = (Amax1 - Amin1) / ThreadsNum;
                Amin1 -= A1_Delta;
                for (int i = 0; i < ThreadsNum; i++)
                {
                    minDeviationArr[i] = 0f;
                    Amin1 += A1_Delta;
                    Amax1 = Amin1 + A1_Delta;
                    ThreadStarting(Amin1, Astep1, Amax1, kmin1, kstep1, kmax1, Cmin, Cstep, Cmax, i);
                }
                while (finished < ThreadsNum)
                {
                    yield return new WaitForSeconds(0.02f);
                    ProgressImage_Rough.fillAmount = fillAmount_Rough;
                }

                minDeviation = 99999999f;
                for (int i = 0; i < ThreadsNum; i++)
                {
                    if (minDeviationArr[i] < minDeviation)
                    {
                        minDeviation = minDeviationArr[i];
                        best_A1 = best_A1_arr[i];
                        best_k1 = best_k1_arr[i];
                        best_C = best_C_arr[i];
                    }
                }

                finished = 0;
                Amin1 = best_A1 - 0.1f * best_A1;
                Amax1 = best_A1 + 0.1f * best_A1;
                Astep1 = (Amax1 - Amin1) / 120;
                kmin1 = best_k1 + 0.1f * best_k1;
                kmax1 = best_k1 - 0.1f * best_k1;
                kstep1 = (kmax1 - kmin1) / 300;
                Cmin = best_C - 0.1f * Mathf.Abs(best_C);
                Cmax = best_C + 0.1f * Mathf.Abs(best_C);
                if (best_C == 0f)
                {
                    Cmin = -StaticStorage.tailSignal * 0.5f;
                    Cmax = StaticStorage.tailSignal * 0.5f;
                }
                Cstep = (Cmax - Cmin) / 10;

                roughCalc = false;
                preciseCalc = true;
                A1_Delta = (Amax1 - Amin1) / ThreadsNum;
                Amin1 -= A1_Delta;
                for (int i = 0; i < ThreadsNum; i++)
                {
                    Amin1 += A1_Delta;
                    Amax1 = Amin1 + A1_Delta;
                    ThreadStarting(Amin1, Astep1, Amax1, kmin1, kstep1, kmax1, Cmin, Cstep, Cmax, i);
                }
                while (finished < ThreadsNum)
                {
                    yield return new WaitForSeconds(0.02f);
                    ProgressImage_Precise.fillAmount = fillAmount_Precise;
                }

                finished = 0;
                minDeviation = 99999999f;
                for (int i = 0; i < ThreadsNum; i++)
                {
                    if (minDeviationArr[i] < minDeviation)
                    {
                        minDeviation = minDeviationArr[i];
                        best_A1 = best_A1_arr[i];
                        best_k1 = best_k1_arr[i];
                        best_C = best_C_arr[i];
                    }
                }
                StaticStorage.best_k_1_1 = best_k1;
                StaticStorage.best_A_1_1 = best_A1;

                for (int i = 0; i < StaticStorage.timeList.Count; i++)
                {
                    StaticStorage.exp1_List.Add(GameObject.Instantiate(DotImage1, new Vector2(StaticStorage.timeList[i] * StaticStorage.X_Scale + 150 * screen_widthCoeff, (best_A1 * Mathf.Exp(best_k1 * StaticStorage.timeList[i]) + best_C) * StaticStorage.Y_Scale + 120 * screen_heightCoeff), Quaternion.identity, ParentPanel.transform));
                    StaticStorage.signalExp_1_1.Add(best_A1 * Mathf.Exp(best_k1 * StaticStorage.timeList[i]) + best_C);
                }
                break;

            case 1:
                if (StaticStorage.signalExp_2_1.Count > 0)
                {
                    best_k1 = StaticStorage.best_k_2_1;
                    best_k2 = StaticStorage.best_k_2_2;
                    best_A1 = StaticStorage.best_A_2_1;
                    best_A2 = StaticStorage.best_A_2_2;
                    Redraw(StaticStorage.signalExp_2_1, StaticStorage.exp1_List, DotImage1);
                    Redraw(StaticStorage.signalExp_2_2, StaticStorage.exp2_List, DotImage2);
                    Redraw(StaticStorage.signalSum_2, StaticStorage.sum_List, DotImage_Sum);
                    break;
                }
                roughCalc = true;
                preciseCalc = false;
                A1_Delta = (Amax1 - Amin1) / ThreadsNum;
                Amin1 -= A1_Delta;
                for (int i = 0; i < ThreadsNum; i++)
                {
                    minDeviationArr[i] = 0f;
                    Amin1 += A1_Delta;
                    Amax1 = Amin1 + A1_Delta;
                    ThreadStarting(Amin1, Astep1, Amax1, kmin1, kstep1, kmax1, Amin2, Astep2, Amax2, kmin2, kstep2, kmax2, Cmin, Cstep, Cmax, i);
                }

                while (finished < ThreadsNum)
                {
                    yield return new WaitForSeconds(0.02f);
                    ProgressImage_Rough.fillAmount = fillAmount_Rough;
                }

                minDeviation = 99999999f;
                for (int i = 0; i < ThreadsNum; i++)
                {
                    if (minDeviationArr[i] < minDeviation)
                    {
                        minDeviation = minDeviationArr[i];
                        best_A1 = best_A1_arr[i];
                        best_k1 = best_k1_arr[i];
                        best_A2 = best_A2_arr[i];
                        best_k2 = best_k2_arr[i];
                        best_C = best_C_arr[i];
                    }
                }

                finished = 0;
                if (best_A1 == 0) best_A1 = StaticStorage.maxSignal * 0.1f;
                if (best_A2 == 0) best_A2 = StaticStorage.maxSignal * 0.1f;
                Amin1 = best_A1 - 0.1f * best_A1;
                Amax1 = best_A1 + 0.1f * best_A1;
                Astep1 = (Amax1 - Amin1) / 12;
                kmin1 = best_k1 + 0.1f * best_k1;
                kmax1 = best_k1 - 0.1f * best_k1;
                kstep1 = Mathf.Abs((kmax1 - kmin1) / 20);
                Amin2 = best_A2 - 0.1f * best_A2;
                Amax2 = best_A2 + 0.1f * best_A2;
                Astep2 = (Amax2 - Amin2) / 10;
                kmin2 = best_k2 + 0.1f * best_k2;
                kmax2 = best_k2 - 0.1f * best_k2;
                kstep2 = Mathf.Abs((kmax2 - kmin2) / 30);
                Cmin = best_C - 0.1f * Mathf.Abs(best_C);
                Cmax = best_C + 0.1f * Mathf.Abs(best_C);
                if (best_C == 0f)
                {
                    Cmin = -StaticStorage.tailSignal * 0.5f;
                    Cmax = StaticStorage.tailSignal * 0.5f;
                }
                Cstep = (Cmax - Cmin) / 10;

                /*print("Amin1 = " + Amin1 + ", Amax1 = " + Amax1 + ", kmin1 = " + kmin1 + ", kmax1 = " + kmax1);
                print("Amin2 = " + Amin2 + ", Amax2 = " + Amax2 + ", kmin2 = " + kmin2 + ", kmax2 = " + kmax2);
                print("kstep1 = " + kstep1);
                print("kstep2 = " + kstep2);*/

                roughCalc = false;
                preciseCalc = true;
                A1_Delta = (Amax1 - Amin1) / ThreadsNum;
                Amin1 -= A1_Delta;
                for (int i = 0; i < ThreadsNum; i++)
                {
                    Amin1 += A1_Delta;
                    Amax1 = Amin1 + A1_Delta;
                    ThreadStarting(Amin1, Astep1, Amax1, kmin1, kstep1, kmax1, Amin2, Astep2, Amax2, kmin2, kstep2, kmax2, Cmin, Cstep, Cmax, i);
                }

                while (finished < ThreadsNum)
                {
                    yield return new WaitForSeconds(0.02f);
                    ProgressImage_Precise.fillAmount = fillAmount_Precise;
                }

                minDeviation = 99999999f;
                for (int i = 0; i < ThreadsNum; i++)
                {
                    if (minDeviationArr[i] < minDeviation)
                    {
                        minDeviation = minDeviationArr[i];
                        best_A1 = best_A1_arr[i];
                        best_k1 = best_k1_arr[i];
                        best_A2 = best_A2_arr[i];
                        best_k2 = best_k2_arr[i];
                        best_C = best_C_arr[i];
                    }
                }

                finished = 0;
                StaticStorage.best_k_2_1 = best_k1;
                StaticStorage.best_k_2_2 = best_k2;
                StaticStorage.best_A_2_1 = best_A1;
                StaticStorage.best_A_2_2 = best_A2;

                for (int i = 0; i < StaticStorage.timeList.Count; i++)
                {
                    StaticStorage.exp1_List.Add(GameObject.Instantiate(DotImage1, new Vector2(StaticStorage.timeList[i] * StaticStorage.X_Scale + 150 * screen_widthCoeff, (best_A1 * Mathf.Exp(best_k1 * StaticStorage.timeList[i]) + best_C) * StaticStorage.Y_Scale + 120 * screen_heightCoeff), Quaternion.identity, ParentPanel.transform));
                    StaticStorage.exp2_List.Add(GameObject.Instantiate(DotImage2, new Vector2(StaticStorage.timeList[i] * StaticStorage.X_Scale + 150 * screen_widthCoeff, (best_A2 * Mathf.Exp(best_k2 * StaticStorage.timeList[i]) + best_C) * StaticStorage.Y_Scale + 120 * screen_heightCoeff), Quaternion.identity, ParentPanel.transform));
                    StaticStorage.sum_List.Add(GameObject.Instantiate(DotImage_Sum, new Vector2(StaticStorage.timeList[i] * StaticStorage.X_Scale + 150 * screen_widthCoeff,
                        (best_A1 * Mathf.Exp(best_k1 * StaticStorage.timeList[i]) + best_A2 * Mathf.Exp(best_k2 * StaticStorage.timeList[i]) + best_C * 2) * StaticStorage.Y_Scale + 120 * screen_heightCoeff), Quaternion.identity, ParentPanel.transform));
                    StaticStorage.signalExp_2_1.Add(best_A1 * Mathf.Exp(best_k1 * StaticStorage.timeList[i]) + best_C);
                    StaticStorage.signalExp_2_2.Add(best_A2 * Mathf.Exp(best_k2 * StaticStorage.timeList[i]) + best_C);
                    StaticStorage.signalSum_2.Add(StaticStorage.signalExp_2_1[i] + StaticStorage.signalExp_2_2[i] + best_C * 2);
                }
                break;

            case 2:
                if (StaticStorage.signalExp_3_1.Count > 0)
                {
                    best_k1 = StaticStorage.best_k_3_1;
                    best_k2 = StaticStorage.best_k_3_2;
                    best_k3 = StaticStorage.best_k_3_3;
                    best_A1 = StaticStorage.best_A_3_1;
                    best_A2 = StaticStorage.best_A_3_2;
                    best_A3 = StaticStorage.best_A_3_3;
                    Redraw(StaticStorage.signalExp_3_1, StaticStorage.exp1_List, DotImage1);
                    Redraw(StaticStorage.signalExp_3_2, StaticStorage.exp2_List, DotImage3);
                    Redraw(StaticStorage.signalExp_3_3, StaticStorage.exp3_List, DotImage2);
                    Redraw(StaticStorage.signalSum_3, StaticStorage.sum_List, DotImage_Sum);
                    break;
                }
                roughCalc = true;
                preciseCalc = false ;
                A1_Delta = (Amax1 - Amin1) / ThreadsNum;
                Amin1 -= A1_Delta;
                for (int i = 0; i < ThreadsNum; i++)
                {
                    minDeviationArr[i] = 0f;
                    Amin1 += A1_Delta;
                    Amax1 = Amin1 + A1_Delta;
                    print("ThreadNum = " + i + ", Amin1 = " + Amin1 + ", Amax1 = " + Amax1 + ", Amin2 = " + Amin2 + ", Amax2 = " + Amax2 + ", Amin3 = " + Amin3 + ", Amax3 = " + Amax3);
                    ThreadStarting(Amin1, Astep1, Amax1, kmin1, kstep1, kmax1, Amin2, Astep2, Amax2, kmin2, kstep2, kmax2, Amin3, Astep3, Amax3, kmin3, kstep3, kmax3, Cmin, Cstep, Cmax, i);
                }

                while (finished < ThreadsNum)
                {
                    yield return new WaitForSeconds(0.02f);
                    ProgressImage_Rough.fillAmount = fillAmount_Rough;
                }

                minDeviation = 99999999f;
                for (int i = 0; i < ThreadsNum; i++)
                {
                    if (minDeviationArr[i] < minDeviation)
                    {
                        minDeviation = minDeviationArr[i];
                        best_A1 = best_A1_arr[i];
                        best_k1 = best_k1_arr[i];
                        best_A2 = best_A2_arr[i];
                        best_k2 = best_k2_arr[i];
                        best_A3 = best_A3_arr[i];
                        best_k3 = best_k3_arr[i];
                        best_C = best_C_arr[i];
                    }
                }

                finished = 0;
                if (best_A1 == 0) best_A1 = StaticStorage.maxSignal * 0.1f;
                if (best_A2 == 0) best_A2 = StaticStorage.maxSignal * 0.1f;
                if (best_A3 == 0) best_A3 = StaticStorage.maxSignal * 0.1f;
                Amin1 = best_A1 - 0.2f * best_A1;
                Amax1 = best_A1 + 0.2f * best_A1;
                Astep1 = (Amax1 - Amin1) / 4;
                kmin1 = best_k1 + 0.1f * best_k1;
                kmax1 = best_k1 - 0.1f * best_k1;
                kstep1 = Mathf.Abs((kmax1 - kmin1) / 10);
                Amin2 = best_A2 - 0.2f * best_A2;
                Amax2 = best_A2 + 0.2f * best_A2;
                Astep2 = (Amax2 - Amin2) / 5;
                kmin2 = best_k2 + 0.1f * best_k2;
                kmax2 = best_k2 - 0.1f * best_k2;
                kstep2 = Mathf.Abs((kmax2 - kmin2) / 10);
                Amin3 = best_A3 - 0.2f * best_A3;
                Amax3 = best_A3 + 0.2f * best_A3;
                Astep3 = (Amax3 - Amin3) / 5;
                kmin3 = best_k3 + 0.1f * best_k3;
                kmax3 = best_k3 - 0.1f * best_k3;
                kstep3 = Mathf.Abs((kmax3 - kmin3) / 10);
                Cmin = best_C - 0.1f * Mathf.Abs(best_C);
                Cmax = best_C + 0.1f * Mathf.Abs(best_C);
                if (best_C == 0f)
                {
                    Cmin = -StaticStorage.tailSignal * 0.5f;
                    Cmax = StaticStorage.tailSignal * 0.5f;
                }
                Cstep = (Cmax - Cmin) / 10;

                roughCalc = false;
                preciseCalc = true;
                A1_Delta = (Amax1 - Amin1) / ThreadsNum;
                Amin1 -= A1_Delta;
                for (int i = 0; i < ThreadsNum; i++)
                {
                    Amin1 += A1_Delta;
                    Amax1 = Amin1 + A1_Delta;
                    ThreadStarting(Amin1, Astep1, Amax1, kmin1, kstep1, kmax1, Amin2, Astep2, Amax2, kmin2, kstep2, kmax2, Amin3, Astep3, Amax3, kmin3, kstep3, kmax3, Cmin, Cstep, Cmax, i);
                }

                while (finished < ThreadsNum)
                {
                    yield return new WaitForSeconds(0.02f);
                    ProgressImage_Precise.fillAmount = fillAmount_Precise;
                }

                minDeviation = 99999999f;
                for (int i = 0; i < ThreadsNum; i++)
                {
                    if (minDeviationArr[i] < minDeviation)
                    {
                        minDeviation = minDeviationArr[i];
                        best_A1 = best_A1_arr[i];
                        best_k1 = best_k1_arr[i];
                        best_A2 = best_A2_arr[i];
                        best_k2 = best_k2_arr[i];
                        best_A3 = best_A3_arr[i];
                        best_k3 = best_k3_arr[i];
                        best_C = best_C_arr[i];
                    }
                }

                finished = 0;
                StaticStorage.best_k_3_1 = best_k1;
                StaticStorage.best_k_3_2 = best_k2;
                StaticStorage.best_k_3_3 = best_k3;
                StaticStorage.best_A_3_1 = best_A1;
                StaticStorage.best_A_3_2 = best_A2;
                StaticStorage.best_A_3_3 = best_A3;

                for (int i = 0; i < StaticStorage.timeList.Count; i++)
                {
                    StaticStorage.exp1_List.Add(GameObject.Instantiate(DotImage1, new Vector2(StaticStorage.timeList[i] * StaticStorage.X_Scale + 150 * screen_widthCoeff, (best_A1 * Mathf.Exp(best_k1 * StaticStorage.timeList[i]) + best_C) * StaticStorage.Y_Scale + 120 * screen_heightCoeff), Quaternion.identity, ParentPanel.transform));
                    StaticStorage.exp2_List.Add(GameObject.Instantiate(DotImage3, new Vector2(StaticStorage.timeList[i] * StaticStorage.X_Scale + 150 * screen_widthCoeff, (best_A2 * Mathf.Exp(best_k2 * StaticStorage.timeList[i]) + best_C) * StaticStorage.Y_Scale + 120 * screen_heightCoeff), Quaternion.identity, ParentPanel.transform));
                    StaticStorage.exp2_List.Add(GameObject.Instantiate(DotImage2, new Vector2(StaticStorage.timeList[i] * StaticStorage.X_Scale + 150 * screen_widthCoeff, (best_A3 * Mathf.Exp(best_k3 * StaticStorage.timeList[i]) + best_C) * StaticStorage.Y_Scale + 120 * screen_heightCoeff), Quaternion.identity, ParentPanel.transform));
                    StaticStorage.sum_List.Add(GameObject.Instantiate(DotImage_Sum, new Vector2(StaticStorage.timeList[i] * StaticStorage.X_Scale + 150 * screen_widthCoeff,
                        (best_A1 * Mathf.Exp(best_k1 * StaticStorage.timeList[i]) + best_A2 * Mathf.Exp(best_k2 * StaticStorage.timeList[i]) + best_A3 * Mathf.Exp(best_k3 * StaticStorage.timeList[i]) + best_C * 3) * StaticStorage.Y_Scale + 120 * screen_heightCoeff), Quaternion.identity, ParentPanel.transform));
                    StaticStorage.signalExp_3_1.Add(best_A1 * Mathf.Exp(best_k1 * StaticStorage.timeList[i]) + best_C);
                    StaticStorage.signalExp_3_2.Add(best_A2 * Mathf.Exp(best_k2 * StaticStorage.timeList[i]) + best_C);
                    StaticStorage.signalExp_3_3.Add(best_A3 * Mathf.Exp(best_k3 * StaticStorage.timeList[i]) + best_C);
                    StaticStorage.signalSum_3.Add(StaticStorage.signalExp_3_1[i] + StaticStorage.signalExp_3_2[i] + StaticStorage.signalExp_3_3[i] + best_C * 3);
                }
                    break;
        }
        ProgressPanel.SetActive(false);

        /*List<Vector2> tempList = new List<Vector2>();
        for (int i = 0; i < StaticStorage.timeList.Count; i++)
        {
            tempList.Add(StaticStorage.dotsList[i].transform.position);
        }
        for (int i = 0; i < StaticStorage.timeList.Count; i++)
        {
            Destroy(StaticStorage.dotsList[i]);
        }
        StaticStorage.dotsList.Clear();
        for (int i = 0; i < StaticStorage.timeList.Count; i++)
        {
            StaticStorage.dotsList.Add(GameObject.Instantiate(DotImage, tempList[i], Quaternion.identity, ParentPanel.transform));
        }*/

        ShowLifeTime();
        ShowAmplitude();
    }

    private void ThreadStarting(float A_min, float A_step, float A_max, float k_min, float k_step, float k_max, float C_min, float C_step, float C_max, int threadNum)
    {
        myThread[threadNum] = new Thread(() => DecompositionCycle(A_min, A_step, A_max, k_min, k_step, k_max, C_min, C_step, C_max, threadNum));
        myThread[threadNum].Start();
    }

    private void ThreadStarting(float A1_min, float A1_step, float A1_max, float k1_min, float k1_step, float k1_max,
        float A2_min, float A2_step, float A2_max, float k2_min, float k2_step, float k2_max, float C_min, float C_step, float C_max, int threadNum)
    {
        myThread[threadNum] = new Thread(() => DecompositionCycle(A1_min, A1_step, A1_max, k1_min, k1_step, k1_max,
            A2_min, A2_step, A2_max, k2_min, k2_step, k2_max, C_min, C_step, C_max, threadNum));
        myThread[threadNum].Start();
    }

    private void ThreadStarting(float A1_min, float A1_step, float A1_max, float k1_min, float k1_step, float k1_max,
        float A2_min, float A2_step, float A2_max, float k2_min, float k2_step, float k2_max,
        float A3_min, float A3_step, float A3_max, float k3_min, float k3_step, float k3_max, float C_min, float C_step, float C_max, int threadNum)
    {
        myThread[threadNum] = new Thread(() => DecompositionCycle(A1_min, A1_step, A1_max, k1_min, k1_step, k1_max,
            A2_min, A2_step, A2_max, k2_min, k2_step, k2_max, A3_min, A3_step, A3_max, k3_min, k3_step, k3_max, C_min, C_step, C_max, threadNum));
        myThread[threadNum].Start();
    }

    private void DecompositionCycle(float A_min, float A_step, float A_max, float k_min, float k_step, float k_max, float C_min, float C_step, float C_max, int threadNum)
    {
        int A_steps = Mathf.RoundToInt((A_max - A_min) / A_step);
        int k_steps = Mathf.RoundToInt((k_max - k_min) / k_step);
        int C_steps = Mathf.RoundToInt((C_max - C_min) / C_step);
        if (!StaticStorage.addConstant) fillStep = 1f / (A_steps * k_steps * ThreadsNum);
        else fillStep = 1f / (A_steps * k_steps * C_steps * ThreadsNum);
        print("Steps = " + A_steps * k_steps * C_steps);
        print("In cycle: A_min = " + A_min + ", A_max = " + A_max + ", A_step = " + A_step);

        float exp, signal, delta;

        for (float A = A_min; A < A_max - A_step / 2; A = A + A_step)
        {
            for (float k = k_min; k < k_max - k_step / 2; k = k + k_step)
            {
                if (!StaticStorage.addConstant || C_max == C_min)
                {
                    deviation[threadNum] = 0f;
                    for (int i = 0; i < StaticStorage.timeList.Count; i++)
                    {
                        exp = A * Mathf.Exp(k * StaticStorage.timeList[i]);
                        signal = StaticStorage.signalList[i];
                        delta = exp - signal;
                        deviation[threadNum] = deviation[threadNum] + delta * delta;
                    }

                    //print("deviation = " + deviation);

                    if (deviation[threadNum] < minDeviationArr[threadNum] || minDeviationArr[threadNum] == 0f)
                    {
                        minDeviationArr[threadNum] = deviation[threadNum];
                        best_A1_arr[threadNum] = A;
                        best_k1_arr[threadNum] = k;
                    }
                    if (roughCalc) fillAmount_Rough += fillStep;
                    else if (preciseCalc) fillAmount_Precise += fillStep;
                }

                else
                {
                    for (float C = C_min; C < C_max - C_step / 2; C = C + C_step)
                    {
                        deviation[threadNum] = 0f;
                        for (int i = 0; i < StaticStorage.timeList.Count; i++)
                        {
                            exp = A * Mathf.Exp(k * StaticStorage.timeList[i]) + C;
                            signal = StaticStorage.signalList[i];
                            delta = exp - signal;
                            deviation[threadNum] = deviation[threadNum] + delta * delta;
                        }

                        //print("deviation = " + deviation);

                        if (deviation[threadNum] < minDeviationArr[threadNum] || minDeviationArr[threadNum] == 0f)
                        {
                            minDeviationArr[threadNum] = deviation[threadNum];
                            best_A1_arr[threadNum] = A;
                            best_k1_arr[threadNum] = k;
                            best_C_arr[threadNum] = C;
                        }
                        if (roughCalc) fillAmount_Rough += fillStep;
                        else if (preciseCalc) fillAmount_Precise += fillStep;
                    }
                }
                //print("ProgressImage.fillAmount = " + ProgressImage.fillAmount);
            }
        }
        finished++;
        print("best_A = " + best_A1_arr[threadNum]);
        print("best_k = " + best_k1_arr[threadNum]);
        print("threadNum = " + threadNum);
    }

    private void DecompositionCycle(float A1_min, float A1_step, float A1_max, float k1_min, float k1_step, float k1_max,
        float A2_min, float A2_step, float A2_max, float k2_min, float k2_step, float k2_max, float C_min, float C_step, float C_max, int threadNum)
    {
        int A1_steps = Mathf.RoundToInt((A1_max - A1_min) / A1_step);
        int k1_steps = Mathf.RoundToInt((k1_max - k1_min) / k1_step);
        int A2_steps = Mathf.RoundToInt((A2_max - A2_min) / A2_step);
        int k2_steps = Mathf.RoundToInt((k2_max - k2_min) / k2_step);
        int C_steps = Mathf.RoundToInt((C_max - C_min) / C_step);
        if (!StaticStorage.addConstant) fillStep = 1f / (A1_steps * k1_steps * A2_steps * k2_steps * ThreadsNum);
        else fillStep = 1f / (A1_steps * k1_steps * A2_steps * k2_steps * C_steps * ThreadsNum);
        print("Steps = " + A1_steps * k1_steps * A2_steps * k2_steps * C_steps);

        float exp1, exp2, signal, delta;

        for (float A1 = A1_min; A1 < A1_max - A1_step / 2; A1 = A1 + A1_step)
        {
            for (float k1 = k1_min; k1 < k1_max - k1_step / 2; k1 = k1 + k1_step)
            {
                for (float A2 = A2_min; A2 < A2_max - A2_step / 2; A2 = A2 + A2_step)
                {
                    for (float k2 = k2_min; k2 < k2_max - k2_step / 2; k2 = k2 + k2_step)
                    {
                        if (!StaticStorage.addConstant || C_max == C_min)
                        {
                            deviation[threadNum] = 0f;
                            for (int i = 0; i < StaticStorage.timeList.Count; i++)
                            {
                                exp1 = A1 * Mathf.Exp(k1 * StaticStorage.timeList[i]);
                                exp2 = A2 * Mathf.Exp(k2 * StaticStorage.timeList[i]);
                                signal = StaticStorage.signalList[i];
                                delta = exp1 + exp2 - signal;
                                deviation[threadNum] = deviation[threadNum] + delta * delta;
                            }

                            if (deviation[threadNum] < minDeviationArr[threadNum] || minDeviationArr[threadNum] == 0f)
                            {
                                minDeviationArr[threadNum] = deviation[threadNum];
                                best_A1_arr[threadNum] = A1;
                                best_k1_arr[threadNum] = k1;
                                best_A2_arr[threadNum] = A2;
                                best_k2_arr[threadNum] = k2;
                            }
                            if (roughCalc) fillAmount_Rough += fillStep;
                            else if (preciseCalc) fillAmount_Precise += fillStep;
                        }
                        else
                        {
                            for (float C = C_min; C < C_max - C_step / 2; C = C + C_step)
                            {
                                deviation[threadNum] = 0f;
                                for (int i = 0; i < StaticStorage.timeList.Count; i++)
                                {
                                    exp1 = A1 * Mathf.Exp(k1 * StaticStorage.timeList[i]) + C;
                                    exp2 = A2 * Mathf.Exp(k2 * StaticStorage.timeList[i]) + C;
                                    signal = StaticStorage.signalList[i];
                                    delta = exp1 + exp2 - signal;
                                    deviation[threadNum] = deviation[threadNum] + delta * delta;
                                }

                                if (deviation[threadNum] < minDeviationArr[threadNum] || minDeviationArr[threadNum] == 0f)
                                {
                                    minDeviationArr[threadNum] = deviation[threadNum];
                                    best_A1_arr[threadNum] = A1;
                                    best_k1_arr[threadNum] = k1;
                                    best_A2_arr[threadNum] = A2;
                                    best_k2_arr[threadNum] = k2;
                                    best_C_arr[threadNum] = C;
                                }
                                if (roughCalc) fillAmount_Rough += fillStep;
                                else if (preciseCalc) fillAmount_Precise += fillStep;
                            }
                        }
                    }
                }     
            }
        }
        finished++;
        print("best_A1 = " + best_A1);
        print("best_k1 = " + best_k1);
        print("best_A2 = " + best_A2);
        print("best_k2 = " + best_k2);
    }

    private void DecompositionCycle(float A1_min, float A1_step, float A1_max, float k1_min, float k1_step, float k1_max,
        float A2_min, float A2_step, float A2_max, float k2_min, float k2_step, float k2_max,
        float A3_min, float A3_step, float A3_max, float k3_min, float k3_step, float k3_max, float C_min, float C_step, float C_max, int threadNum)
    {
        int A1_steps = Mathf.RoundToInt((A1_max - A1_min) / A1_step);
        int k1_steps = Mathf.RoundToInt((k1_max - k1_min) / k1_step);
        int A2_steps = Mathf.RoundToInt((A2_max - A2_min) / A2_step);
        int k2_steps = Mathf.RoundToInt((k2_max - k2_min) / k2_step);
        int A3_steps = Mathf.RoundToInt((A3_max - A3_min) / A3_step);
        int k3_steps = Mathf.RoundToInt((k3_max - k3_min) / k3_step);
        int C_steps = Mathf.RoundToInt((C_max - C_min) / C_step);
        if (!StaticStorage.addConstant) fillStep = 1f / (A1_steps * k1_steps * A2_steps * k2_steps * A3_steps * k3_steps * ThreadsNum);
        else fillStep = 1f / (A1_steps * k1_steps * A2_steps * k2_steps * A3_steps * k3_steps * C_steps * ThreadsNum);
        print("Steps = " + A1_steps * k1_steps * A2_steps * k2_steps * A3_steps * k3_steps * C_steps);
        int iteration = 0;
        float exp1, exp2, exp3, signal, delta;

        for (float A1 = A1_min; A1 < A1_max - A1_step / 2; A1 = A1 + A1_step)
        {
            for (float k1 = k1_min; k1 < k1_max - k1_step / 2; k1 = k1 + k1_step)
            {
                for (float A2 = A2_min; A2 < A2_max - A2_step / 2; A2 = A2 + A2_step)
                {
                    for (float k2 = k2_min; k2 < k2_max - k2_step / 2; k2 = k2 + k2_step)
                    {
                        for (float A3 = A3_min; A3 < A3_max - A3_step / 2; A3 = A3 + A3_step)
                        {
                            for (float k3 = k3_min; k3 < k3_max - k3_step / 2; k3 = k3 + k3_step)
                            {
                                if (!StaticStorage.addConstant || C_max == C_min)
                                {
                                    deviation[threadNum] = 0f;
                                    for (int i = 0; i < StaticStorage.timeList.Count; i++)
                                    {
                                        exp1 = A1 * Mathf.Exp(k1 * StaticStorage.timeList[i]);
                                        exp2 = A2 * Mathf.Exp(k2 * StaticStorage.timeList[i]);
                                        exp3 = A3 * Mathf.Exp(k3 * StaticStorage.timeList[i]);
                                        signal = StaticStorage.signalList[i];
                                        delta = exp1 + exp2 + exp3 - signal;
                                        deviation[threadNum] = deviation[threadNum] + delta * delta;
                                    }

                                    if (deviation[threadNum] < minDeviationArr[threadNum] || minDeviationArr[threadNum] == 0f)
                                    {
                                        minDeviationArr[threadNum] = deviation[threadNum];
                                        best_A1_arr[threadNum] = A1;
                                        best_k1_arr[threadNum] = k1;
                                        best_A2_arr[threadNum] = A2;
                                        best_k2_arr[threadNum] = k2;
                                        best_A3_arr[threadNum] = A3;
                                        best_k3_arr[threadNum] = k3;
                                    }
                                    if (roughCalc) fillAmount_Rough += fillStep;
                                    else if (preciseCalc) fillAmount_Precise += fillStep;
                                    iteration++;
                                }
                                else
                                {
                                    for (float C = C_min; C < C_max - C_step / 2; C = C + C_step)
                                    {
                                        deviation[threadNum] = 0f;
                                        for (int i = 0; i < StaticStorage.timeList.Count; i++)
                                        {
                                            exp1 = A1 * Mathf.Exp(k1 * StaticStorage.timeList[i]) + C;
                                            exp2 = A2 * Mathf.Exp(k2 * StaticStorage.timeList[i]) + C;
                                            exp3 = A3 * Mathf.Exp(k3 * StaticStorage.timeList[i]) + C;
                                            signal = StaticStorage.signalList[i];
                                            delta = exp1 + exp2 + exp3 - signal;
                                            deviation[threadNum] = deviation[threadNum] + delta * delta;
                                        }

                                        if (deviation[threadNum] < minDeviationArr[threadNum] || minDeviationArr[threadNum] == 0f)
                                        {
                                            minDeviationArr[threadNum] = deviation[threadNum];
                                            best_A1_arr[threadNum] = A1;
                                            best_k1_arr[threadNum] = k1;
                                            best_A2_arr[threadNum] = A2;
                                            best_k2_arr[threadNum] = k2;
                                            best_A3_arr[threadNum] = A3;
                                            best_k3_arr[threadNum] = k3;
                                            best_C_arr[threadNum] = C;
                                        }
                                        if (roughCalc) fillAmount_Rough += fillStep;
                                        else if (preciseCalc) fillAmount_Precise += fillStep;
                                        iteration++;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        finished++;
        print("iterations = " + iteration);
        print("best_A1 = " + best_A1);
        print("best_k1 = " + best_k1);
        print("best_A2 = " + best_A2);
        print("best_k2 = " + best_k2);
        print("best_A3 = " + best_A3);
        print("best_k3 = " + best_k3);
    }

    private void Redraw(List<float> signalList, List<Image> dotList, Image dotImage)
    {
        for (int i = 0; i < signalList.Count; i++)
        {
            dotList.Add(GameObject.Instantiate(dotImage, new Vector2(StaticStorage.timeList[i] * StaticStorage.X_Scale + 150 * screen_widthCoeff, signalList[i] * StaticStorage.Y_Scale + 120 * screen_heightCoeff), Quaternion.identity, ParentPanel.transform));
        }

    }

    public void DeleteCurve(List<Image> dotList)
    {
        for (int i = 0; i < dotList.Count; i++)
        {
            Destroy(dotList[i]);
        }
        dotList.Clear();
    }

    private void ShowLifeTime()
    {
        if (best_k1 != 0) StaticStorage.LifeTime1 = -1 / best_k1;
        if (best_k2 != 0) StaticStorage.LifeTime2 = -1 / best_k2;
        if (best_k3 != 0) StaticStorage.LifeTime3 = -1 / best_k3;
        LifeTime_Title.gameObject.SetActive(true);

        switch (Mode)
        {
            case 0:
                ModifyLF(LifeTime_Bar_1, LifeTime_Text_1, StaticStorage.LifeTime1, DotImage1.color);
                break;
            case 1:
                ModifyLF(LifeTime_Bar_1, LifeTime_Text_1, StaticStorage.LifeTime1, DotImage1.color);
                ModifyLF(LifeTime_Bar_2, LifeTime_Text_2, StaticStorage.LifeTime2, DotImage2.color);
                break;
            case 2:
                ModifyLF(LifeTime_Bar_1, LifeTime_Text_1, StaticStorage.LifeTime1, DotImage1.color);
                ModifyLF(LifeTime_Bar_2, LifeTime_Text_2, StaticStorage.LifeTime2, DotImage3.color);
                ModifyLF(LifeTime_Bar_3, LifeTime_Text_3, StaticStorage.LifeTime3, DotImage2.color);
                break;
        }
    }

    private void ModifyLF(Image LifeTime_Bar, TextMeshProUGUI LifeTime_Text, float lifetime, Color color)
    {
        LifeTime_Bar.gameObject.SetActive(true);
        LifeTime_Text.gameObject.SetActive(true);
        LifeTime_Bar.color = color;
        LifeTime_Bar.rectTransform.sizeDelta = new Vector2(lifetime * StaticStorage.X_Scale, LifeTime_Bar.rectTransform.sizeDelta.y);
        Vector2 textPosition = LifeTime_Text.rectTransform.localPosition;
        textPosition = new Vector2(LifeTime_Text.rectTransform.localPosition.x + lifetime * StaticStorage.X_Scale, LifeTime_Text.rectTransform.localPosition.y);
        LifeTime_Text.rectTransform.localPosition = textPosition;
        if (lifetime >= 0.000001f) LifeTime_Text.text = (lifetime * 1000000).ToString("#.0") + " ìêñ";
        else if (lifetime >= 0.0000001f) LifeTime_Text.text = (lifetime * 1000000).ToString("#0.00") + " ìêñ";
        else LifeTime_Text.text = (lifetime * 1000000000).ToString("#.0") + " íñ";
    }

    private void ShowAmplitude()
    {
        if (best_A1 != 0) StaticStorage.Amplitude1 = best_A1;
        if (best_A2 != 0) StaticStorage.Amplitude2 = best_A2;
        if (best_A3 != 0) StaticStorage.Amplitude3 = best_A3;
        Amplitude_Title.gameObject.SetActive(true);

        switch (Mode)
        {
            case 0:
                ModifyAmp(Amplitude_Text_1, StaticStorage.Amplitude1);
                break;
            case 1:
                ModifyAmp(Amplitude_Text_1, StaticStorage.Amplitude1);
                ModifyAmp(Amplitude_Text_2, StaticStorage.Amplitude2);
                break;
            case 2:
                ModifyAmp(Amplitude_Text_1, StaticStorage.Amplitude1);
                ModifyAmp(Amplitude_Text_2, StaticStorage.Amplitude2);
                ModifyAmp(Amplitude_Text_3, StaticStorage.Amplitude3);
                break;
        }
    }

    private void ModifyAmp(TextMeshProUGUI Amplitude_Text, float Amplitude)
    {
        Amplitude_Text.gameObject.SetActive(true);
        Amplitude_Text.text = Amplitude.ToString("#0.000");
    }

    public void addConstant_Change(bool isChecked)
    {
        StaticStorage.addConstant = addConstant_Toggle.isOn;

        best_C = 0f;
        best_C_arr[0] = 0f;
        best_C_arr[1] = 0f;
        best_C_arr[2] = 0f;
        best_C_arr[3] = 0f;

        var colors = addConstant_Toggle.colors;
        if (!StaticStorage.addConstant)
        {
            colors.normalColor = new Color(0.9f, 0.9f, 0.9f);
            colors.pressedColor = new Color(0.9f, 0.9f, 0.9f);
            colors.selectedColor = new Color(0.9f, 0.9f, 0.9f);
        }
        else
        {
            colors.normalColor = new Color(0.6f, 1f, 0.4f);
            colors.pressedColor = new Color(0.6f, 1f, 0.4f);
            colors.selectedColor = new Color(0.6f, 1f, 0.4f);
        }
        addConstant_Toggle.colors = colors;

        StaticStorage.signalExp_1_1.Clear();
        StaticStorage.signalExp_2_1.Clear();
        StaticStorage.signalExp_2_2.Clear();
        StaticStorage.signalExp_3_1.Clear();
        StaticStorage.signalExp_3_2.Clear();
        StaticStorage.signalExp_3_3.Clear();
        StaticStorage.signalSum_2.Clear();
        StaticStorage.signalSum_3.Clear();
    }
}
