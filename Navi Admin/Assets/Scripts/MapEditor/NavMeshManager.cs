using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation;
using TMPro;
using System;

public class NavMeshManager : MonoBehaviour
{
    [Header("Required Stuff")]
    [SerializeField] private ErrorMessageController _errorMessageBox;
    [SerializeField] private PolygonsManager _polygonsManager;
    [SerializeField] private TMP_Dropdown _roomsDropdown;

    [Header("Navigation Settings")]
    [SerializeField] private LayerMask _navLayerMask;

    private NavMeshSurface _navMeshSurface;
    private GameObject _placeMarker;
    private GameObject _navigationAgent;
    private LineRenderer _pathLine;
    private InputMap _input;
    private bool _placingAgent = false;

    private void Start()
    {
        _input = new InputMap();
        _navMeshSurface = this.GetComponent<NavMeshSurface>();
        _navigationAgent = this.transform.GetChild(0).gameObject;
        _pathLine = this.transform.GetChild(1).GetComponent<LineRenderer>();
        _placeMarker = this.transform.GetChild(2).gameObject;
        _navigationAgent.SetActive(false);
    }
    private void Update()
    {
        if (_placingAgent) _navigationAgent.transform.position = GetCursorPositionOnPlane();
        if (_pathLine.gameObject.activeSelf) // Move the path line texture
            _pathLine.material.mainTextureOffset = new Vector2(-Time.time, 0);
    }

    private Vector3 GetCursorPositionOnPlane()
    {   // Get the cursor position in the world
        Ray ray = Camera.main.ScreenPointToRay(_input.RenderView.Position.ReadValue<Vector2>());
        Debug.DrawRay(ray.origin, ray.direction * 100, Color.red); // Draw the ray

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, _navLayerMask))
        {   // If the ray hits the navigation layer, return the hit point
            Vector3 _position = hit.point;
            _position.y = 0.4f;
            return _position;
        }
        else return _navigationAgent.transform.position;
    }
    public void SetDropdownOptions()
    {   // Set the dropdown options
        List<string> _rooms = new List<string>();
        _rooms.Add("Select a room");
        _polygonsManager.polygons.ForEach(polygon => _rooms.Add(polygon.polygonLabel));

        _roomsDropdown.ClearOptions();
        _roomsDropdown.AddOptions(_rooms);
    }

    public void GenerateNavMesh()
    {   // Generate the navigation mesh
        _navMeshSurface.BuildNavMesh();
        //NavMesh.AddNavMeshData(_navMeshSurface.navMeshData);
    }

    public void PlaceAgentPosition()
    {   // Place the agent in the clicked position
        _input.RenderView.Enable();
        _input.RenderView.Click.started += ctx => SetAgentPosition();

        _navigationAgent.SetActive(true);
        _placingAgent = true;
        _pathLine.gameObject.SetActive(false);
        _placeMarker.SetActive(false);
    }

    private void SetAgentPosition()
    {   // Set the agent position to the clicked position
        _navigationAgent.transform.position = GetCursorPositionOnPlane();
        _input.RenderView.Disable();
        _placingAgent = false;
        GeneratePath();
    }

    public void GeneratePath()
    {   // Generate a path from the agent to the room selected
        if (_roomsDropdown.value == 0) return; // If no room is selected, return
        if (!_navigationAgent.activeSelf) return; // If the agent is not placed, return
        _errorMessageBox.HideMessage();

        // Get the destination point of the selected room
        string _roomName = _roomsDropdown.options[_roomsDropdown.value].text;
        PolygonController _room = _polygonsManager.polygons.Find(polygon => polygon.polygonLabel == _roomName);
        Vector3 _destinationPoint = _room.GetPolygonCenter(true);
        _destinationPoint.y = 0.4f;

        // Calculate the path to the destination point and show it
        NavMeshPath _path = new NavMeshPath();
        NavMesh.CalculatePath(_navigationAgent.transform.position, _destinationPoint, NavMesh.AllAreas, _path);

        if (_path.status != NavMeshPathStatus.PathComplete)
        {   // If the destination point is not reachable, show an error message
            _errorMessageBox.ShowMessage("DestinationNotReachable");
            _pathLine.gameObject.SetActive(false);
        }
        else
        {   // Show the path line
            _pathLine.positionCount = _path.corners.Length;
            _pathLine.SetPositions(_path.corners);
            _pathLine.gameObject.SetActive(true);
        }

        // Place the marker in the destination point
        _placeMarker.transform.position = _destinationPoint + new Vector3(0, 0.1f, 0);
        _placeMarker.SetActive(true);
    }

    public void HideNavigation()
    {   // Hide the agent position and the path line
        _navigationAgent.SetActive(false);
        _pathLine.gameObject.SetActive(false);
        _placeMarker.SetActive(false);
    }
}
