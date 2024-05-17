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
    [SerializeField] private RoomsManager _polygonsManager;
    [SerializeField] private TMP_Dropdown _roomsDropdown;

    [Header("Navigation Settings")]
    [SerializeField] private LayerMask _navLayerMask;
    [SerializeField] private bool _navToDoor = true;
    private NavMeshSurface _navMeshSurface;
    private GameObject _placeMarker;
    private GameObject _navAgent;
    private LineRenderer _pathLine;
    private InputMap _input;

    public bool isPlacingAgent = false;
    public string inAgentRoom;

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
        List<string> _rooms = new List<string> { "Select a room" };
        _polygonsManager.rooms.ForEach(polygon => _rooms.Add(polygon.roomName));

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
        inAgentRoom = GetAgentRoom();
        isPlacingAgent = false;
        GeneratePath();
    }

    private string GetAgentRoom()
    {   // Get the room where the agent is placed
        Ray ray = new Ray(_navAgent.transform.position + new Vector3(0, 0.5f, 0), Vector3.down);

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, _navLayerMask))
        {   // If the ray hits the navigation layer, return the room name
            return hit.collider.name;
        }
        else return "";
    }

    public void GeneratePath()
    {   // Generate a path from the agent to the room selected
        if (_roomsDropdown.value == 0) return; // If no room is selected, return
        if (!_navAgent.activeSelf) return; // If the agent is not placed, return
        _errorMessageBox.HideMessage();

        // Get the destination point of the selected room
        string _roomName = _roomsDropdown.options[_roomsDropdown.value].text;
        RoomController _room = _polygonsManager.rooms.Find(polygon => polygon.roomName == _roomName);
        Vector3 _destinationPoint = Vector3.zero;

        if (inAgentRoom == _roomName) _navToDoor = false;
        else _navToDoor = false; // If the agent is not in the selected room, navigate to the door

        // TOOD: Fix the destination point when are more than one door in the path

        if (_navToDoor) _destinationPoint = _room.GetClosestDoor(_navAgent.transform.position);
        if (!_navToDoor || _destinationPoint == Vector3.zero) _destinationPoint = _room.GetPolygonCenter(true);
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
