using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public static class Helper
{
    public static void Shuffle<T>(ref List<T> list) where T : UnityEngine.Object
    {
        T temp = null;

        for (int i = 0; i < list.Count / 2; i++)
        {
            int j = Random.Range(0, list.Count - 1);

            temp = list[i];
            list[i] = list[j];
            list[j] = temp;

        }
    }

    public static Color GetColorWithAlpha(Color color, float alpha)
    {
        return new Color(color.r, color.g, color.b, alpha);
    }
}
