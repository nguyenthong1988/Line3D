using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

public class Board : MonoBehaviour
{
    public RuntimeBoard runtimeBoard;

    //
    protected Collider m_Collider;
    protected Vector3 m_BoardSize;
    protected Vector2 m_CellSize;

    void Awake() => Initialize();

    protected virtual void Initialize()
    {
        runtimeBoard = new RuntimeBoard();
        m_Collider = GetComponent<Collider>();
        if (m_Collider)
        {
            Debug.Log(" --- " + m_Collider.bounds.size.x);
            Debug.Log(" --- " + m_Collider.bounds.size.y);
            Debug.Log(" --- " + m_Collider.bounds.size.z);
            m_BoardSize = m_Collider.bounds.size;
            m_CellSize = new Vector2(m_BoardSize.x / RuntimeBoard.BOARD_SIZE, m_BoardSize.z / RuntimeBoard.BOARD_SIZE);
        }
    }

    void Start()
    {
        var mousePressPosStream = Observable.EveryUpdate()
            .Where(_ => Input.GetMouseButtonDown(0))
            .Select(_ => Input.mousePosition)
            .Subscribe(OnTouchDown);

        var mouseReleasePosStream = Observable.EveryUpdate()
            .Where(_ => Input.GetMouseButtonUp(0))
            .Select(_ => Input.mousePosition)
            .Subscribe(OnTouchUp);
    }

    public void OnTouchDown(Vector3 position)
    {
        Ray raycast = Camera.main.ScreenPointToRay(position);
        if (Physics.Raycast(raycast, out var castHit))
        {
            if (castHit.collider.GetComponent<Board>())
            {
                var localHit = transform.InverseTransformPoint(castHit.point);
                Debug.Log("MousePressPos : " + localHit);
                int offsetCol = (int)((localHit.x + m_BoardSize.x / 2) / m_CellSize.x);
                int offsetRow = (int)((localHit.z + m_BoardSize.z / 2) / m_CellSize.y);
                Debug.Log("offset : " + offsetRow + " | " + offsetCol);
                if (runtimeBoard.cells[offsetRow, offsetCol].selectable)
                {
                    runtimeBoard.cells[offsetRow, offsetCol].SetSelection(true);
                }
            }
        }

        var availableCells = runtimeBoard.GetDotCells();
        foreach (var cell in availableCells)
        {
            cell.ball.SetSize(Ball.Size.Ball);
        }

        var emptyCells = runtimeBoard.GetRandomCells(5);
        foreach (var cell in emptyCells)
        {
            var randomColor = (Ball.Color)Random.Range(0, (int)Ball.Color.Ghost + 1);
            var pos = ComputePosition(cell.rowOffset, cell.colOffset) + new Vector3(0, 0.5f, 0);
            var ball = Pooler.instance.Spawn<Ball>("ball", pos, transform);
            ball.SetColor(randomColor);
            cell.ball = ball;
        }

    }

    public void OnTouchUp(Vector3 position)
    {
        //Debug.Log("MouseUpPos : " + Camera.main.ScreenToWorldPoint(position));
    }

    private Vector3 ComputePosition(int row, int col)
    {
        return new Vector3(-m_BoardSize.x * 0.5f + row * m_CellSize.x + m_CellSize.x / 2 + transform.position.x, transform.position.y, -m_BoardSize.z * 0.5f + col * m_CellSize.y + m_CellSize.y / 2 + transform.position.z);
    }
}
