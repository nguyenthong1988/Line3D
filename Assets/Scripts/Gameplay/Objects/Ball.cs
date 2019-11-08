using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : ObjectPool
{
    public enum Color { None = -1, Red, Green, Blue, Cyan, Magenta, Yellow, Brown, Ghost }
    public enum Size { Dot, Ball };
    public enum State { Idle, Selected }

    public Color color { get; protected set; }
    public Size size { get; protected set; }
    public State state { get; protected set; }

    public bool selected { get; protected set; }
    public Action onSelectedChange;

    //
    protected int[] m_ColorTable = { 0xffffff, 0xcd0027, 0x509d28, 0x1d57a9, 0x54b9c1, 0xdd6ba7, 0xffe001, 0x4f3401, 0xe47a0a };
    Animator m_Animator;

    void Awake() => Initialize();

    protected virtual void Initialize()
    {
        m_Animator = GetComponent<Animator>();
        Reset();
    }

    public void Reset()
    {
        SetColor(Color.None);
        SetSize(Size.Dot);
        state = State.Idle;
    }

    public void SetColor(Color color)
    {
        this.color = color;
        GetComponent<MeshRenderer>().material.color = Utilities.ToColor(m_ColorTable[(int)color + 1]);
    }

    public void SetSelection(bool select)
    {
        if (selected == select) return;
        selected = select;
        SetTrigger(selected ? "select" : "idle");
        onSelectedChange?.Invoke();
    }

    public void SetSize(Size size)
    {
        this.size = size;
        float scaleRatio = size == Size.Dot ? 0.25f : 0.5f;
        transform.localScale = new Vector3(scaleRatio, scaleRatio, scaleRatio);
    }

    protected void SetTrigger(string trigger)
    {
        m_Animator.enabled = selected;
        if (m_Animator && !string.IsNullOrEmpty(trigger))
        {
            m_Animator.SetTrigger(trigger);
        }
    }
}
