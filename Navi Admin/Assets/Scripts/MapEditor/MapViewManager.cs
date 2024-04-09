using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MapViewManager : MonoBehaviour
{
    [SerializeField] private PolygonsManager _polygonRender;
    [SerializeField] private GameObject _selectTool;
    private GameObject _wallDots;
    private GameObject _entrances;
    private GameObject _polygons;
    public bool editDotsActive;
    private bool _mapViewActive = false;

    void Start()
    {
        _wallDots = this.transform.GetChild(1).gameObject;
        _entrances = this.transform.GetChild(2).gameObject;
        _polygons = this.transform.GetChild(3).gameObject;
        editDotsActive = true;
    }

    void Update()
    {
        if (_mapViewActive) _polygonRender.UpdateLabelsPosition();
    }

    public void ShowMapView()
    {   // Show the map view
        if (!_mapViewActive)
        {
            _polygonRender.Generate2DPolygons();
            _polygonRender.ShowPolygonsLabels();
            _polygons.SetActive(true);
            _mapViewActive = true;
        }
        else
        {   // Hide the map view
            _polygonRender.RemovePolygonsLabels();
            if (!_selectTool.activeSelf) _polygons.SetActive(false);
            _mapViewActive = false;
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
