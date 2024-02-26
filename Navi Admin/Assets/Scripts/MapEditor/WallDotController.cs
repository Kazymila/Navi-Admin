using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
public class WallDotController : MonoBehaviour
{
    public List<WallDotController> neighborsDots = new List<WallDotController>();
    public List<GameObject> lines = new List<GameObject>();
    public List<int> linesType = new List<int>();
    public int linesCount = 0;
    public Vector3 position;

    public CircleCollider2D DotCollider;
    private Animator _dotAnimator;

    private void Start()
    {
        _dotAnimator = GetComponent<Animator>();
        DotCollider = GetComponent<CircleCollider2D>();
    }

    private void Update()
    {   // Delete the dot if it has no lines
        if (lines.Count == 0) DeleteDot();
        position = this.transform.position;
    }

    public void PlayHoverAnimation() => _dotAnimator.Play("Hovered", 0, 0);
    public void PlayDeniedAnimation() => _dotAnimator.Play("Denied", 0, 0);

    public void SetPosition(Vector3 _position)
    {   // Set the position of the dot and update the lines
        for (int i = 0; i < linesCount; i++)
        {
            lines[i].GetComponent<LineRenderer>().SetPosition(linesType[i], _position);
            lines[i].GetComponent<WallLineController>().SetLineCollider();
        }
        position = _position + new Vector3(0, 0, -0.5f);
        this.transform.position = position;
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
        Destroy(gameObject);
    }

    public bool FindNeighbor(WallDotController _neighborDot)
    {   // Find a neighbor dot in the list
        if (neighborsDots.Contains(_neighborDot)) return true;
        else return false;
    }
}
