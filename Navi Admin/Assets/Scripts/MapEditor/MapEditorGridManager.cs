using UnityEngine;
using UnityEngine.UI;

public class MapEditorGridManager : MonoBehaviour
{

    [Header("Button Icons")]
    [SerializeField] private Image _gridViewButtonIcon;
    [SerializeField] private Sprite _gridViewSprite;
    [SerializeField] private Sprite _gridHideSprite;
    [SerializeField] private Image _snapButtonIcon;
    [SerializeField] private Sprite _gridSnapSprite;
    [SerializeField] private Sprite _gridNoSnapSprite;

    [Header("Grid Settings")]
    public float gridSize = 1f;
    public bool gridActive = true;
    public bool snapToGrid = false;

    private GameObject _grid;

    void Start()
    {
        _grid = this.gameObject.transform.GetChild(0).gameObject;
    }

    public void ViewGrid()
    {
        gridActive = !gridActive;
        _grid.SetActive(gridActive);
        _gridViewButtonIcon.sprite = gridActive ? _gridViewSprite : _gridHideSprite;
    }

    public void SnapToGrid()
    {
        snapToGrid = !snapToGrid;
        _snapButtonIcon.sprite = snapToGrid ? _gridSnapSprite : _gridNoSnapSprite;
    }
}
