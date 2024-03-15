using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MapViewManager : MonoBehaviour
{
    private GameObject _wallDots;
    private GameObject _entrances;
    public bool editDotsActive;

    void Start()
    {
        _wallDots = this.transform.GetChild(1).gameObject;
        _entrances = this.transform.GetChild(2).gameObject;
        editDotsActive = true;
    }

    public void ViewEditDots()
    {   // Show or hide the dots from the map view
        editDotsActive = !editDotsActive;
        _wallDots.SetActive(editDotsActive);

        foreach (Transform _entrance in _entrances.transform)
        {
            _entrance.GetComponent<EntrancesController>().ActivateDots(editDotsActive);
        }
    }
}
