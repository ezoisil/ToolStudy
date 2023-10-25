using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Timeline;

public static class FloatExtensions 
{
    /// <summary>
    /// Rounds a float value to the nearest multiple of the specified size.
    /// </summary>
    /// <param name="value">The value to be rounded.</param>
    /// <param name="size">The size of the rounding grid.</param>
    /// <returns>The rounded value based on the specified size.</returns>
    public static float Round(this float value, float size)
    {
        return Mathf.Round(value / size) * size;
    }
}
