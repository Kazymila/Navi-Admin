using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
public class WallNodeController : MonoBehaviour
{
    [Header("Related Stuff")]
    public List<WallNodeController> neighborsNodes = new List<WallNodeController>();
    public List<RoomController> rooms = new List<RoomController>();
    public List<GameObject> walls = new List<GameObject>();
    public List<int> linesType = new List<int>();
    public int linesCount = 0;

    [Header("Dot Settings")]
    public int nodeIndex;
    public Vector3 position;
    public bool isOnEntranceDot;

    public CircleCollider2D dotCollider;
    private Animator _dotAnimator;

    private void Awake()
    {
        _dotAnimator = GetComponent<Animator>();
        dotCollider = GetComponent<CircleCollider2D>();
        transform.localRotation = Quaternion.Euler(-90, 0, 0);
        transform.localPosition = new Vector3(
            transform.localPosition.x,
            transform.localPosition.y,
            0.0f);
        nodeIndex = transform.GetSiblingIndex();
        name = "Dot_" + nodeIndex;
    }

    private void LateUpdate()
    {   // Delete the dot if it has no lines
        if (walls.Count == 0) DeleteNode();
        position = this.transform.position;
        position.z = 0.0f;
    }

    public void PlaySelectAnimation() => _dotAnimator.Play("Selected", 0, 0);
    public void PlayDeniedAnimation() => _dotAnimator.Play("Denied", 0, 0);

    public void SetPosition(Vector3 _position)
    {   // Set the position of the dot and update the lines
        for (int i = 0; i < linesCount; i++)
        {
            if (isOnEntranceDot)
            {
                // TODO: Drag the entrance with the dot
            }
            // Update the wall lines position
            walls[i].GetComponent<LineRenderer>().SetPosition(linesType[i], _position);
            WallLineController _lineController = walls[i].GetComponent<WallLineController>();
            _lineController.CalculateLength();
            _lineController.SetLineCollider();

            // Update the entrances position
            if (_lineController.entrances.Count > 0)
                _lineController.entrances.ForEach(entrance =>
                    entrance.RepositionEntranceOnWall(
                        (entrance.endDot.transform.position + entrance.startDot.transform.position) / 2,
                        _lineController));
        }
        this.transform.localPosition = _position;
    }

    public void AddLine(GameObject _line, int _type, WallNodeController _neighborDot)
    {   // Add a line to the dot
        walls.Add(_line);
        linesType.Add(_type);
        neighborsNodes.Add(_neighborDot);
        linesCount++;
    }

    public void DeleteLine(int _index)
    {   // Delete a line from the dot
        if (_index != -1)
        {
            walls.RemoveAt(_index);
            linesType.RemoveAt(_index);
            neighborsNodes.RemoveAt(_index);
            linesCount--;
        }
    }

    public void DeleteNode(bool _destroyLines = true)
    {   // Delete the dot and its lines

        // TODO: Fix some issues when delete the dot after create the polygons

        for (int i = 0; i < linesCount; i++)
        {
            if (_destroyLines) walls[i].GetComponent<WallLineController>().DestroyLine(false);
            neighborsNodes[i].DeleteLine(neighborsNodes[i].neighborsNodes.IndexOf(this));
            if (neighborsNodes[i].linesCount == 0) neighborsNodes[i].DeleteNode();
        }
        foreach (RoomController _room in rooms)
        {   // Remove the dot from the polygon and regenerate the mesh
            foreach (WallNodeController _node in _room.nodes)
                if (_node != this) _node.rooms.Remove(_room);

            //_polygon.transform.parent.GetComponent<PolygonsManager>().UpdatePolygons();
            _room.transform.parent.GetComponent<RoomsManager>().DestroyPolygon(_room);
        }
        Destroy(gameObject);
    }

    public bool FindNeighborNode(WallNodeController _neighborDot)
    {   // Find a neighbor dot in the list
        if (neighborsNodes.Contains(_neighborDot)) return true;
        else return false;
    }

    #region --- Trigger Events ---
    private void OnTriggerStay2D(Collider2D _collider)
    {
        if (_collider.CompareTag("EntranceDot"))
            isOnEntranceDot = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("EntranceDot"))
            isOnEntranceDot = false;
    }
    #endregion
}
