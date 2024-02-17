using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallDotController : MonoBehaviour
{
    public List<WallDotController> neighborsDots = new List<WallDotController>();
    public List<GameObject> lines = new List<GameObject>();
    public List<int> linesType = new List<int>();
    public int linesCount = 0;
    public Vector3 position;

    private void Update()
    {
        if (lines.Count == 0) DeleteDot();
        position = this.transform.position;
    }

    public void SetPosition(Vector3 _position)
    {
        for (int i = 0; i < linesCount; i++)
        {
            lines[i].GetComponent<LineRenderer>().SetPosition(linesType[i], _position);
        }
        position = _position;
        this.transform.position = position;
    }

    public void AddLine(GameObject _line, int _type, WallDotController _neighborDot)
    {
        lines.Add(_line);
        linesType.Add(_type);
        neighborsDots.Add(_neighborDot);
        linesCount++;
    }

    public void DeleteLine(int _index)
    {
        lines.RemoveAt(_index);
        linesType.RemoveAt(_index);
        neighborsDots.RemoveAt(_index);
        linesCount--;
    }

    public void DeleteDot(bool _deleteLines = true)
    {
        if (_deleteLines)
        {
            for (int i = 0; i < linesCount; i++)
            {
                neighborsDots[i].DeleteLine(neighborsDots[i].neighborsDots.IndexOf(this));
                Destroy(lines[i]);
            }
        }
        Destroy(gameObject);
    }
}
