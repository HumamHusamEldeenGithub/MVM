﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StandardColors : MonoBehaviour
{
    public static string GetStandardColors(string color)
    {
        return color switch
        {
            "Black_Hair" => "#000000",
            "Brown_Hair" => "#4F2903",
            "Blond_Hair" => "C89F73",
            "Gray_Hair" => "#505050",
            _ => "#000000",
        };
    }

    public static string RGBToHex(int r, int g, int b)
    {
        return $"#{r:X2}{g:X2}{b:X2}";
    }
}