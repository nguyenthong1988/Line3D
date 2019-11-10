using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UniRx;
using DG.Tweening;

public class Board : MonoBehaviour
{
    public static readonly int MAX_UNDO_STEPS = 10;
    public RuntimeBoard runtimeBoard;

    //
    protected Collider m_Collider;
    protected Vector3 m_BoardSize;
    protected Vector2 m_CellSize;

    protected Vector2Int m_SelectedIndex = Vector2Int.left;
    protected List<Vector2Int> m_PointsToCheck;
    protected List<Vector2Int> m_MovePath;
    protected Stack<int[]> m_UndoStack;

    void Awake() => Initialize();

    private CompositeDisposable m_Disposables = new CompositeDisposable();

    protected virtual void Initialize()
    {
        runtimeBoard = new RuntimeBoard();
        m_PointsToCheck = new List<Vector2Int>();
        m_MovePath = new List<Vector2Int>();
        m_UndoStack = new Stack<int[]>();
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
        Observable.EveryUpdate()
            .Where(_ => Input.GetMouseButtonDown(0))
            .Select(_ => Input.mousePosition)
            .Subscribe(OnTouchDown).AddTo(m_Disposables);

        Observable.EveryUpdate()
            .Where(_ => Input.GetMouseButtonUp(0))
            .Select(_ => Input.mousePosition)
            .Subscribe(OnTouchUp).AddTo(m_Disposables);
    }

    public void StartGame()
    {
        SpawnDot();
        CheckBoard();
        m_UndoStack.Clear();
        SpawnDot();
    }

    public void EndGame()
    {
        m_Disposables.Clear();
    }

    public IEnumerator StartDebug()
    {
        yield return new WaitForSeconds(1.0f);
        StartGame();
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
                    if (m_SelectedIndex != Vector2Int.left)
                        runtimeBoard.cells[m_SelectedIndex.x, m_SelectedIndex.y]?.SetSelection(false);
                    runtimeBoard.cells[offset.x, offset.y].SetSelection(true);
                    m_SelectedIndex = offset;
                }
            }
            else if (castHit.collider.GetComponent<Board>())
            {
                Vector2Int offset = GetBoardOffset(castHit.point);
                Debug.Log("offset : " + offset.x + " | " + offset.y);
                if (runtimeBoard.cells[offset.x, offset.y].available)
                {
                    if (m_SelectedIndex != Vector2Int.left)
                    {
                        runtimeBoard.cells[m_SelectedIndex.x, m_SelectedIndex.y].SetSelection(false);
                        runtimeBoard.cells[offset.x, offset.y].AttachBall(runtimeBoard.cells[m_SelectedIndex.x, m_SelectedIndex.y].DettachBall());
                        m_PointsToCheck.Clear();
                        m_PointsToCheck.Add(offset);
                        //
                        MoveBall(m_SelectedIndex, offset);
                        m_SelectedIndex = Vector2Int.left;
                    }
                }
            }
        }
    }

    private void SpawnDot()
    {
        var colors = new int[3];
        var emptyCells = runtimeBoard.GetRandomCells(3);
        if (emptyCells.Count < 3)
        {
            GameManager.instance.ChangeGamestate(GameManager.GameState.EndGame);
            return;
        }
        int maxRange = (int)(GameManager.instance.hardMode == GameManager.HardMode.Normal ? Ball.Color.Magenta :
            GameManager.instance.hardMode == GameManager.HardMode.Hard ? Ball.Color.Brown :
            Ball.Color.Ghost) + 1;
        for (int i = 0; i < emptyCells.Count; i++)
        {
            var cell = emptyCells[i];
            var randomColor = (Ball.Color)Random.Range(0, maxRange);
            var pos = ComputePosition(cell.rowOffset, cell.colOffset);
            var ball = Pooler.instance.Spawn<Ball>("ball", pos, transform);
            ball.SetColor(randomColor);
            cell.ball = ball;

            colors[i] = ball.GetColor32Code();
        }
        EventDispatcher.Dispatch(GameEvent.GE_ADD_DOT_COLOR, colors[0], colors[1], colors[2]);
        m_UndoStack.Push(runtimeBoard.BoardData());
        if (m_UndoStack.Count > MAX_UNDO_STEPS) m_UndoStack.Peek();
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
        return new Vector3(-m_BoardSize.x * 0.5f + row * m_CellSize.x + m_CellSize.x / 2 + transform.position.x, transform.position.y + 0.5f, -m_BoardSize.z * 0.5f + col * m_CellSize.y + m_CellSize.y / 2 + transform.position.z);
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
            runtimeBoard.cells[to.x, to.y].ball.transform.DOPath(worldMovePath.ToArray(), 0.3f).OnComplete(() => OnMovePathDone());
        }
    }

    public void OnMovePathDone()
    {
        Debug.Log("mOnMoveDone");

        CheckBoard();
        SpawnDot();

        if (m_PointsToCheck.Count > 0)
        {
            foreach (var point in m_PointsToCheck)
            {
                List<Vector2Int> points = runtimeBoard.CheckLines(point);

                if (points != null && points.Count > 0)
                {
                    Debug.Log("add score");
                    EventDispatcher.Dispatch(GameEvent.GE_ADD_SCORE, points.Count);
                    foreach (var p in points) ExplodeBall(p);
                }
            }

            m_PointsToCheck.Clear();
        }

        //m_PrevSelected = Vector2Int.left;
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
        runtimeBoard.cells[cellIndex.x, cellIndex.y].ball.Explosive();
        runtimeBoard.cells[cellIndex.x, cellIndex.y].ball.Destroy();
        runtimeBoard.cells[cellIndex.x, cellIndex.y].ball = null;
    }
}
