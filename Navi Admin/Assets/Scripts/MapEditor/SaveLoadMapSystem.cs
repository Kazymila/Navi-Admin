using System;
using UnityEngine;
using MapDataModel;
using SFB;

public class SaveLoadMapSystem : MonoBehaviour
{
    [SerializeField] private Render3DManager _renderViewManager;

    [Header("Map Data")]
    [SerializeField] public MapData mapData;
    [SerializeField] public ARMapData ARMapData;

    ExtensionFilter[] _extensionList = new[] {
        new ExtensionFilter("JSON Files", "json"),
        new ExtensionFilter("All Files", "*" )
        }; // Extension filters for file window

    public void SaveMapData()
    {   // Save the map data to a JSON file
        string _path = StandaloneFileBrowser.SaveFilePanel("Save Map", "", mapData.mapName + "MapData", _extensionList);
        if (_path == "") return; // If the path is empty, return
        JsonDataService.SaveData(_path, mapData);
    }

    public void LoadMapData()
    {   // Load the map data from a JSON file
        string _path = StandaloneFileBrowser.OpenFilePanel("Load Map", "", _extensionList, false)[0];
        if (_path == "") return; // If the path is empty, return
        mapData = JsonDataService.LoadData<MapData>(_path);
    }

    public void SaveRenderMapData()
    {   // Save the render map data to a JSON file
        ARFloorData _renderData = _renderViewManager.GetRenderData();
        ARMapData = new ARMapData
        {
            mapName = mapData.mapName,
            buildingName = mapData.buildingName,
            floors = new ARFloorData[] { _renderData } // Save only the current floor
        };

        string _path = StandaloneFileBrowser.SaveFilePanel("Save AR Map", "", ARMapData.mapName + "ARMapData", _extensionList);
        if (_path == "") return; // If the path is empty, return
        JsonDataService.SaveData(_path, ARMapData);
    }
}
