using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation;
using TMPro;

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
    private GameObject _navAgent;
    private LineRenderer _pathLine;
    private InputMap _input;
    public bool isPlacingAgent = false;

    private void OnEnable()
    {
        _input.RenderView.Enable();
        _input.RenderView.Click.started += ctx => OnClick();
        PlaceAgentPosition();
    }

    private void OnDisable() => _input.RenderView.Disable();

    private void Awake()
    {
        _input = new InputMap();
        _navMeshSurface = this.GetComponent<NavMeshSurface>();
        _navAgent = this.transform.GetChild(0).gameObject;
        _pathLine = this.transform.GetChild(1).GetComponent<LineRenderer>();
        _placeMarker = this.transform.GetChild(2).gameObject;
        _navAgent.SetActive(false);
    }
    private void Update()
    {
        if (isPlacingAgent) _navAgent.transform.position = GetCursorPositionOnPlane();
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
        else return _navAgent.transform.position;
    }

    public void SetDropdownOptions()
    {   // Set the dropdown options
        List<string> _rooms = new List<string>();
        _rooms.Add("Select a room");
        _polygonsManager.polygons.ForEach(polygon => _rooms.Add(polygon.polygonLabel));

        _roomsDropdown.ClearOptions();
        _roomsDropdown.AddOptions(_rooms);
    }

    public void GenerateNavMesh() => _navMeshSurface.BuildNavMesh();

    public void OnClick()
    {   // On click set the agent position or start move it
        if (isPlacingAgent) SetAgentPosition();
        else
        {   // If the agent is not moving, check if is clicked to move it
            Ray ray = Camera.main.ScreenPointToRay(_input.RenderView.Position.ReadValue<Vector2>());
            if (Physics.Raycast(ray, out RaycastHit hit) && hit.collider.CompareTag("NavAgent"))
                PlaceAgentPosition();
        }
    }

    public void PlaceAgentPosition()
    {   // Place the agent in the clicked position
        isPlacingAgent = true;
        _navAgent.SetActive(true);
        _pathLine.gameObject.SetActive(false);
        _placeMarker.SetActive(false);
    }

    private void SetAgentPosition()
    {   // Set the agent position to the clicked position
        _navAgent.transform.position = GetCursorPositionOnPlane();
        isPlacingAgent = false;
        GeneratePath();
    }

    public void GeneratePath()
    {   // Generate a path from the agent to the room selected
        if (_roomsDropdown.value == 0) return; // If no room is selected, return
        if (!_navAgent.activeSelf) return; // If the agent is not placed, return
        _errorMessageBox.HideMessage();

        // Get the destination point of the selected room
        string _roomName = _roomsDropdown.options[_roomsDropdown.value].text;
        PolygonController _room = _polygonsManager.polygons.Find(polygon => polygon.polygonLabel == _roomName);
        Vector3 _destinationPoint = _room.GetPolygonCentroid(true);
        _destinationPoint.y = 0.4f;

        // Calculate the path to the destination point and show it
        NavMeshPath _path = new NavMeshPath();
        NavMesh.CalculatePath(_navAgent.transform.position, _destinationPoint, NavMesh.AllAreas, _path);

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

    public void ActivateNavigation()
    {   // Activate or deactivate the navigation agent
        if (this.gameObject.activeSelf) this.gameObject.SetActive(false);
        else this.gameObject.SetActive(true);
    }
}
