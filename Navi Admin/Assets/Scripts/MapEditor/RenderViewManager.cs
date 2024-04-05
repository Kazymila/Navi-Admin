using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RenderViewManager : MonoBehaviour
{
    [Header("UI Stuff")]
    [SerializeField] private EditorLayoutController _editorUILayout;
    [SerializeField] private GameObject _3DViewLayout;
    [SerializeField] private GameObject _mapDrawLayout;

    [Header("Managers")]
    [SerializeField] private MapEditorGridManager _gridManager;
    [SerializeField] private PolygonsManager _polygonsManager;
    [SerializeField] private NavMeshManager _navMeshManager;

    private MapEditorCameraManager _cameraManager;
    private GameObject _wallLines;

    void Start()
    {
        _cameraManager = Camera.main.GetComponent<MapEditorCameraManager>();
        _wallLines = _mapDrawLayout.transform.GetChild(0).gameObject;
    }

    private void ShowRenderElements(bool show)
    {   // Show or hide the render elements
        foreach (Transform _child in this.transform)
            _child.gameObject.SetActive(show);
    }

    public void ShowRenderView()
    {   // Show the 3D view of the map
        GenerateMapRender();

        _gridManager.gameObject.transform.GetChild(0).gameObject.SetActive(false);
        _editorUILayout.HideEditorInterface();
        _cameraManager.SetPerspectiveView();
        _mapDrawLayout.SetActive(false);

        _3DViewLayout.SetActive(true);
        ShowRenderElements(true);

        _navMeshManager.gameObject.SetActive(true);
        _navMeshManager.SetDropdownOptions();
        _navMeshManager.GenerateNavMesh();
    }

    public void BackToEditor()
    {   // Return to the editor view
        if (_gridManager.gridActive) // If the grid was active, reactivate it
            _gridManager.gameObject.transform.GetChild(0).gameObject.SetActive(true);

        _editorUILayout.gameObject.SetActive(true);
        _cameraManager.SetOrthographicView();
        _mapDrawLayout.SetActive(true);

        _3DViewLayout.SetActive(false);
        ShowRenderElements(false);

        _navMeshManager.HideNavigation();
        _navMeshManager.gameObject.SetActive(false);
    }

    private void GenerateMapRender()
    {   // Generate the map render elements (walls, polygons)
        for (int i = 0; i < _wallLines.transform.childCount; i++)
        {
            _wallLines.transform.GetChild(i).GetComponent<WallLineController>().GenerateWallMesh();
        }
        _polygonsManager.GeneratePolygons();
        _polygonsManager.RemovePolygonsLabels();
        _polygonsManager.Generate3DPolygons();
    }
}
