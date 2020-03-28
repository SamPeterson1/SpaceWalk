using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Optimizer
{
    private static float startTimeSeconds = 0;
    public static void begin()
    {
        startTimeSeconds = Time.realtimeSinceStartup;
    }

    public static float getDeltaTime()
    {
        return 1000 * (Time.realtimeSinceStartup - startTimeSeconds);
    }
}
