using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UniRx;
using DG.Tweening;

public class Board : MonoBehaviour
{
    public RuntimeBoard runtimeBoard;

    //
    protected Collider m_Collider;
    protected Vector3 m_BoardSize;
    protected Vector2 m_CellSize;

    protected Vector2Int m_PrevSelected = Vector2Int.left;
    protected List<Vector2Int> m_PointsToCheck;
    protected List<Vector2Int> m_MovePath;

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

        StartCoroutine(StartGame());
    }

    public IEnumerator StartGame()
    {
        yield return new WaitForSeconds(1.0f);

        var emptyCells = runtimeBoard.GetRandomCells(3);
        foreach (var cell in emptyCells)
        {
            var randomColor = (Ball.Color)Random.Range(0, (int)Ball.Color.Yellow);
            var pos = ComputePosition(cell.rowOffset, cell.colOffset) + new Vector3(0, 0.5f, 0);
            var ball = Pooler.instance.Spawn<Ball>("ball", pos, transform);
            ball.SetColor(randomColor);
            cell.ball = ball;
            cell.ball.SetSize(Ball.Size.Ball);
        }
        SpawnDot();
    }

    public void OnTouchDown(Vector3 position)
    {
        Ray raycast = Camera.main.ScreenPointToRay(position);
        if (Physics.Raycast(raycast, out var castHit))
        {
            if (castHit.collider.GetComponent<Ball>())
            {
                var ball = castHit.collider.GetComponent<Ball>();
                Vector2Int offset = GetBoardOffset(ball.transform.position);
                Debug.Log("offset : " + offset.x + " | " + offset.y);
                if (runtimeBoard.cells[offset.x, offset.y].selectable)
                {
                    if (m_PrevSelected != Vector2Int.left)
                        runtimeBoard.cells[m_PrevSelected.x, m_PrevSelected.y]?.SetSelection(false);
                    runtimeBoard.cells[offset.x, offset.y].SetSelection(true);
                    m_PrevSelected = offset;
                }
            }
            else if (castHit.collider.GetComponent<Board>())
            {
                Vector2Int offset = GetBoardOffset(castHit.point);
                Debug.Log("offset : " + offset.x + " | " + offset.y);
                if (runtimeBoard.cells[offset.x, offset.y].available)
                {
                    if (m_PrevSelected != Vector2Int.left)
                        MoveBall(m_PrevSelected, new Vector2Int(offset.x, offset.y));
                }
            }
        }

        SpawnDot();
    }

    private void SpawnDot()
    {
        var emptyCells = runtimeBoard.GetRandomCells(3);
        foreach (var cell in emptyCells)
        {
            var randomColor = (Ball.Color)Random.Range(0, (int)Ball.Color.Ghost + 1);
            var pos = ComputePosition(cell.rowOffset, cell.colOffset) + new Vector3(0, 0.5f, 0);
            var ball = Pooler.instance.Spawn<Ball>("ball", pos, transform);
            ball.SetColor(randomColor);
            cell.ball = ball;
        }
    }

    private Vector2Int GetBoardOffset(Vector3 point)
    {
        var boardPoint = transform.InverseTransformPoint(point);
        return new Vector2Int((int)((boardPoint.x + m_BoardSize.x / 2) / m_CellSize.x), (int)((boardPoint.z + m_BoardSize.z / 2) / m_CellSize.y));
    }

    public void OnTouchUp(Vector3 position)
    {
        //Debug.Log("MouseUpPos : " + Camera.main.ScreenToWorldPoint(position));
    }

    private Vector3 ComputePosition(int row, int col)
    {
        return new Vector3(-m_BoardSize.x * 0.5f + row * m_CellSize.x + m_CellSize.x / 2 + transform.position.x, transform.position.y, -m_BoardSize.z * 0.5f + col * m_CellSize.y + m_CellSize.y / 2 + transform.position.z);
    }

    protected void MoveBall(Vector2Int from, Vector2Int to)
    {
        var movePath = runtimeBoard.BuildPath(from, to);
        if (!movePath.IsNullOrEmpty())
        {
            var worldMovePath = new List<Vector3>();
            foreach (var point in movePath)
            {
                worldMovePath.Add(ComputePosition(point.x, point.y));
            }
            runtimeBoard.cells[m_PrevSelected.x, m_PrevSelected.y].ball.transform.DOPath(worldMovePath.ToArray(), 0.5f);
        }
    }

    public void OnMovePathDone()
    {
        Debug.Log("mOnMoveDone");

        CheckBoard();

        if (m_PointsToCheck.Count > 0)
        {
            foreach (var point in m_PointsToCheck)
            {
                List<Vector2Int> points = runtimeBoard.CheckLines(point);

                if (points != null && points.Count > 0)
                {
                    foreach (var p in points) ExplodeBall(p);
                }
            }

            m_PointsToCheck.Clear();
        }
    }

    public void CheckBoard()
    {
        var availableCells = runtimeBoard.GetDotCells();
        foreach (var cell in availableCells)
        {
            cell.ball.SetSize(Ball.Size.Ball);
            m_PointsToCheck.Add(new Vector2Int(cell.rowOffset, cell.colOffset));
        }
    }

    protected void ExplodeBall(Vector2Int cellIndex)
    {

    }
}
