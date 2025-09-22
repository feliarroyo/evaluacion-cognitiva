using System;
using System.Collections.Generic;
using System.Linq;

public static class MetricsCalculator
{
    public static int TruePositive(List<string> memObjects, List<string> foundObjects)
    {
        return memObjects.Count(obj => foundObjects.Contains(obj));
    }

    public static int TrueNegative(List<string> decoyObjects, List<string> foundObjects)
    {
        return decoyObjects.Count(obj => !foundObjects.Contains(obj));
    }

    public static int FalsePositive(List<string> decoyObjects, List<string> foundObjects)
    {
        return decoyObjects.Count(obj => foundObjects.Contains(obj));
    }

    public static int FalseNegative(List<string> memObjects, List<string> foundObjects)
    {
        return memObjects.Count(obj => !foundObjects.Contains(obj));
    }

    public static float Accuracy(int tp, int tn, int fp, int fn)
    {
        int total = tp + tn + fp + fn;
        return total > 0 ? (float)(tp + tn) / total : 0f;
    }

    public static float Precision(int tp, int fp)
    {
        return (tp + fp) > 0 ? (float)tp / (tp + fp) : 0f;
    }

    public static float Recall(int tp, int fn)
    {
        return (tp + fn) > 0 ? (float)tp / (tp + fn) : 0f;
    }

    public static float OmissionIndex(int tp, int fn)
    {
        return (tp + fn) > 0 ? (float)fn / (tp + fn) : 0f;
    }

    public static float ErrorRate(int tp, int fp)
    {
        return (tp + fp) > 0 ? (float)fp / (tp + fp) : 0f;
    }

    public static float TemporalEfficiency(float searchTime, float availableTime)
    {
        return availableTime > 0 ? searchTime / availableTime : 0f;
    }

    public static List<string> MemNotChosen(List<string> memObjects, List<string> foundObjects)
    {
        return memObjects.Where(obj => !foundObjects.Contains(obj)).ToList();
    }
}
