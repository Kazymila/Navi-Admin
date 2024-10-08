using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapViewManager : MonoBehaviour
{
    [SerializeField] private RoomsManager _polygonManager;
    [SerializeField] private GameObject _selectTool;
    private GameObject _wallDots;
    private GameObject _entrances;
    private GameObject _polygons;
    public bool editDotsActive;
    public bool isMapViewActive = false;

    void Start()
    {
        _wallDots = this.transform.GetChild(1).gameObject;
        _entrances = this.transform.GetChild(2).gameObject;
        _polygons = this.transform.GetChild(3).gameObject;
        editDotsActive = true;
    }

    void Update()
    {
        if (isMapViewActive) _polygonManager.UpdateLabelsPosition();
    }

    public void ShowMapView(bool _showPolygons = true)
    {   // Show the map view
        if (!isMapViewActive)
        {
            _polygonManager.GenerateRooms();
            _polygonManager.ShowRoomsLabels();
            _polygons.SetActive(_showPolygons);
            isMapViewActive = true;
        }
        else
        {   // Hide the map view
            _polygonManager.RemoveRoomsLabels();
            if (!_selectTool.activeSelf) _polygons.SetActive(false);
            isMapViewActive = false;
        }

        ViewEditDots();
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
