using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RenderViewManager : MonoBehaviour
{
    [Header("UI Stuff")]
    [SerializeField] private EditorLayoutController _editorUILayout;
    [SerializeField] private RenderLayoutController _3DViewLayout;
    [SerializeField] private GameObject _mapDrawLayout;

    [Header("Managers")]
    [SerializeField] private MapEditorGridManager _gridManager;
    [SerializeField] private PolygonsManager _polygonsManager;
    [SerializeField] private NavMeshManager _navMeshManager;

    [Header("Render Elements")]
    [SerializeField] private Transform _wallParent;
    [SerializeField] private Transform _shapesParent;
    [SerializeField] private Transform _shapeRenderParent;
    [SerializeField] private Material _shapeRenderMaterial;

    private MapEditorCameraManager _cameraManager;

    void Start()
    {
        _cameraManager = Camera.main.GetComponent<MapEditorCameraManager>();
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

        _3DViewLayout.gameObject.SetActive(true);
        ShowRenderElements(true);

        _navMeshManager.gameObject.SetActive(true);
        _navMeshManager.SetDropdownOptions();
        _navMeshManager.GenerateNavMesh();
        _navMeshManager.gameObject.SetActive(false);
    }

    public void BackToEditor()
    {   // Return to the editor view
        if (_gridManager.gridActive) // If the grid was active, reactivate it
            _gridManager.gameObject.transform.GetChild(0).gameObject.SetActive(true);

        _editorUILayout.gameObject.SetActive(true);
        _cameraManager.SetOrthographicView();
        _mapDrawLayout.SetActive(true);

        _3DViewLayout.HideRenderInterface();
        ShowRenderElements(false);

        _navMeshManager.gameObject.SetActive(false);
    }

    private void GenerateMapRender()
    {   // Generate the map render elements (walls, polygons, shapes)
        for (int i = 0; i < _wallParent.childCount; i++)
            _wallParent.GetChild(i).GetComponent<WallLineController>().GenerateWallMesh();
        for (int i = 0; i < _shapesParent.childCount; i++)
            _shapesParent.GetChild(i).GetComponent<ShapeController>().GenerateShapeMesh(
                _shapeRenderParent, _shapeRenderMaterial);

        _polygonsManager.Generate2DPolygons();
        _polygonsManager.RemovePolygonsLabels();
        _polygonsManager.Generate3DPolygons();
    }
}
