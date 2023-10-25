using UnityEngine;

public static class Vector3Extensions
{
    /// <summary>
    /// Rounds the x, y, and z components of a Vector3 to their nearest integer values.
    /// </summary>
    public static Vector3 Round(this Vector3 v)
    {
        v.x = Mathf.Round(v.x);
        v.y = Mathf.Round(v.y);
        v.z = Mathf.Round(v.z);
        return v;
    }

    /// <summary>
    /// Rounds the x, y, and z components of a Vector3 to their nearest multiple of the specified size.
    /// </summary>
    /// <param name="size">The size of the rounding grid.</param>
    /// <returns>A new Vector3 with rounded components based on the specified size.</returns>
    public static Vector3 Round(this Vector3 v, float size)
    {
        return (v / size).Round() * size;
    }
}



