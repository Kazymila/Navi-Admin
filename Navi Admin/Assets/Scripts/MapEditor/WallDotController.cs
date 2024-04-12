using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
public class WallDotController : MonoBehaviour
{
    [Header("Related Stuff")]
    public List<WallDotController> neighborsDots = new List<WallDotController>();
    public List<PolygonController> polygons = new List<PolygonController>();
    public List<GameObject> lines = new List<GameObject>();
    public List<int> linesType = new List<int>();
    public int linesCount = 0;

    [Header("Dot Settings")]
    public int dotIndex;
    public Vector3 position;
    public bool isOnEntranceDot;

    public CircleCollider2D dotCollider;
    private Animator _dotAnimator;

    private PolygonsManager _polygonsManager;

    private void Awake()
    {
        _polygonsManager = FindObjectOfType<PolygonsManager>();
        _dotAnimator = GetComponent<Animator>();
        dotCollider = GetComponent<CircleCollider2D>();
        transform.localRotation = Quaternion.Euler(-90, 0, 0);
        transform.localPosition = new Vector3(
            transform.localPosition.x,
            transform.localPosition.y,
            0.0f);
        dotIndex = transform.GetSiblingIndex();
        name = "Dot_" + dotIndex;
    }

    private void LateUpdate()
    {   // Delete the dot if it has no lines
        if (lines.Count == 0) DeleteDot();
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
            lines[i].GetComponent<LineRenderer>().SetPosition(linesType[i], _position);
            WallLineController _lineController = lines[i].GetComponent<WallLineController>();
            _lineController.CalculateLength();
            _lineController.SetLineCollider();

            // Update the entrances position
            if (_lineController.entrancesList.Count > 0)
                _lineController.entrancesList.ForEach(entrance =>
                    entrance.RepositionEntranceOnWall(
                        (entrance.endDot.transform.position + entrance.startDot.transform.position) / 2,
                        _lineController));
        }
        this.transform.localPosition = _position;
    }

    public void AddLine(GameObject _line, int _type, WallDotController _neighborDot)
    {   // Add a line to the dot
        lines.Add(_line);
        linesType.Add(_type);
        neighborsDots.Add(_neighborDot);
        linesCount++;
    }

    public void DeleteLine(int _index)
    {   // Delete a line from the dot
        if (_index != -1)
        {
            lines.RemoveAt(_index);
            linesType.RemoveAt(_index);
            neighborsDots.RemoveAt(_index);
            linesCount--;
        }
    }

    public void DeleteDot(bool _destroyLines = true)
    {   // Delete the dot and its lines
        for (int i = 0; i < linesCount; i++)
        {
            if (_destroyLines) lines[i].GetComponent<WallLineController>().DestroyLine(false);
            neighborsDots[i].DeleteLine(neighborsDots[i].neighborsDots.IndexOf(this));
            if (neighborsDots[i].linesCount == 0) neighborsDots[i].DeleteDot();
        }
        foreach (PolygonController _polygon in polygons)
        {   // Remove the dot from the polygon and regenerate the mesh
            foreach (WallDotController _node in _polygon.nodes)
            {
                if (_node != this) _node.polygons.Remove(_polygon);
            }
            _polygon.transform.parent.GetComponent<PolygonsManager>().polygons.Remove(_polygon);
            Destroy(_polygon.gameObject);
        }
        Destroy(gameObject);
    }

    public bool FindNeighborDot(WallDotController _neighborDot)
    {   // Find a neighbor dot in the list
        if (neighborsDots.Contains(_neighborDot)) return true;
        else return false;
    }

    public bool CheckForCycle()
    {   // Check if the dots are creating a cycle
        // TODO
        return true;
    }

    #region --- Trigger Events ---
    private void OnTriggerStay2D(Collider2D _collider)
    {
        if (_collider.CompareTag("EntranceDot"))
        {
            isOnEntranceDot = true;
            print("OnEntranceDot");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("EntranceDot"))
        {
            isOnEntranceDot = false;
            print("ExitEntranceDot");
        }
    }
    #endregion
}
