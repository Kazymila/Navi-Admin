using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallLineController : MonoBehaviour
{
    public WallDotController startDot;
    public WallDotController endDot;

    public float length;

    public float CalculateLength()
    {
        length = Vector3.Distance(startDot.position, endDot.position);
        return length;
    }
}
