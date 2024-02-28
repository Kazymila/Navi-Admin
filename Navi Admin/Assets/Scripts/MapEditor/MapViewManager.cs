using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapViewManager : MonoBehaviour
{
    private GameObject _wallDots;
    private bool _wallDotsActive;

    // Start is called before the first frame update
    void Start()
    {
        _wallDots = this.transform.GetChild(1).gameObject;
        _wallDotsActive = true;
    }

    public void HideWallDots()
    {   // Hide the dots from the map view
        _wallDotsActive = !_wallDotsActive;
        _wallDots.SetActive(_wallDotsActive);
    }
}
