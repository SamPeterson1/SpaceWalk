using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement
{

    private static float toRadians(float deg)
    {
        return deg * Mathf.PI / 180f;
    }

    public static Vector3 strafe(float dist, Vector3 rotation)
    {
        float radians = toRadians(rotation.y + 90f);
        float z = dist * Mathf.Cos(radians);

        radians = toRadians(rotation.x + 90f);
        float y = dist * Mathf.Sin(radians);

        radians = toRadians(rotation.y + 90f);
        float x = dist * Mathf.Sin(radians);

        return new Vector3(x, y, z);
    }

    public static Vector3 move(float dist, Vector3 rotation)
    {
        float radians = toRadians(rotation.y);
        float z = dist * Mathf.Cos(radians);

        radians = toRadians(rotation.x);
        float y = dist * Mathf.Sin(radians);

        radians = toRadians(rotation.y);
        float x = dist * Mathf.Sin(radians);

        return new Vector3(x, y, z);
    }

}
