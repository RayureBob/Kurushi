using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CoroutineUtility
{
    private static int GetSecondsInFixedStep(Func<float, int> roundDelegate)
    {
        return roundDelegate(1f / Time.fixedDeltaTime);
    }

    public static int GetDurationInFixedStep(float duration, bool ceil)
    {
        Func<float, int> roundDelegate = ceil ? Mathf.CeilToInt : Mathf.FloorToInt;
        return roundDelegate(duration * GetSecondsInFixedStep(roundDelegate));
    }
}
