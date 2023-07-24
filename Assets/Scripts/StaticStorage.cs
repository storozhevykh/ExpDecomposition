using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;


public static class StaticStorage
{
    public static int Mode = 0;
    public static string fileName;
    public static string[] allStrings;
    public static List<float> timeList = new List<float>();
    public static List<float> signalList = new List<float>();
    public static List<Image> dotsList = new List<Image>();
    public static List<Image> exp1_List = new List<Image>();
    public static List<Image> exp2_List = new List<Image>();
    public static List<Image> exp3_List = new List<Image>();
    public static List<Image> sum_List = new List<Image>();
    public static bool addConstant = false;
    public static float tailSignal;
    public static float X_Scale;
    public static float Y_Scale;
    public static float maxSignal;
    public static float maxTime;
    public static float LifeTime1;
    public static float LifeTime2;
    public static float LifeTime3;
    public static float Amplitude1;
    public static float Amplitude2;
    public static float Amplitude3;
    public static int TimeUnits_Scale;
    public static List<float> signalExp_1_1 = new List<float>();
    public static List<float> signalExp_2_1 = new List<float>();
    public static List<float> signalExp_2_2 = new List<float>();
    public static List<float> signalSum_2 = new List<float>();
    public static List<float> signalExp_3_1 = new List<float>();
    public static List<float> signalExp_3_2 = new List<float>();
    public static List<float> signalExp_3_3 = new List<float>();
    public static List<float> signalSum_3 = new List<float>();

    public static float best_k_1_1;
    public static float best_k_2_1;
    public static float best_k_2_2;
    public static float best_k_3_1;
    public static float best_k_3_2;
    public static float best_k_3_3;
    public static float best_A_1_1;
    public static float best_A_2_1;
    public static float best_A_2_2;
    public static float best_A_3_1;
    public static float best_A_3_2;
    public static float best_A_3_3;
}
