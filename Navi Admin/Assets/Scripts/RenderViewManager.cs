using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RenderViewManager : MonoBehaviour
{
    [Header("Required Stuff")]
    [SerializeField] private MapEditorUIController _editorUILayout;
    [SerializeField] private MapEditorGridManager _gridManager;
    [SerializeField] private GameObject _3DViewLayout;
    [SerializeField] private GameObject _mapDrawLayout;

    private GameObject _wallsRender;
    private GameObject _wallLines;
    private MapEditorCameraManager _cameraManager;

    void Start()
    {
        _cameraManager = Camera.main.GetComponent<MapEditorCameraManager>();
        _wallLines = _mapDrawLayout.transform.GetChild(0).gameObject;
        _wallsRender = this.transform.GetChild(0).gameObject;
    }

    public void ShowRenderView()
    {   // Show the 3D view of the map
        GenerateMapRender();

        _gridManager.gameObject.transform.GetChild(0).gameObject.SetActive(false);
        _editorUILayout.HideEditorInterface();
        _cameraManager.SetPerspectiveView();
        _mapDrawLayout.SetActive(false);

        _3DViewLayout.SetActive(true);
        _wallsRender.SetActive(true);
    }

    public void BackToEditor()
    {   // Return to the editor view
        if (_gridManager.gridActive)
            _gridManager.gameObject.transform.GetChild(0).gameObject.SetActive(true);

        _editorUILayout.gameObject.SetActive(true);
        _cameraManager.SetOrthographicView();
        _mapDrawLayout.SetActive(true);

        _3DViewLayout.SetActive(false);
        _wallsRender.SetActive(false);
    }

    private void GenerateMapRender()
    {
        for (int i = 0; i < _wallLines.transform.childCount; i++)
        {
            _wallLines.transform.GetChild(i).GetComponent<WallLineController>().GenerateWallMesh();
        }
    }
}
