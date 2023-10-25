using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Timeline;

public static class FloatExtensions 
{
    public static float Round(this float value, float size)
    {
        return Mathf.Round(value / size) * size;
    }
}
