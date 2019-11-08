using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : Component
{
    protected static T m_Instance;

    public static T instance
    {
        get
        {
            if (m_Instance == null)
            {
                m_Instance = FindObjectOfType<T>();
                if (m_Instance == null)
                {
                    GameObject obj = new GameObject();
                    m_Instance = obj.AddComponent<T>();
                }
            }
            return m_Instance;
        }
    }

    protected virtual void Awake()
    {
        m_Instance = this as T;
    }
}

public class SingletonSimple<T> where T : class, new()
{
    protected static T m_Instance;

    public static T instance
    {
        get
        {
            if (m_Instance == null)
            {
                m_Instance = new T();

            }
            return m_Instance;
        }
    }
}

public class ClassSingleton<T> where T : class, new()
{
    protected static T m_Instance;

    public static T instance
    {
        get
        {
            if (m_Instance == null)
            {
                m_Instance = new T();

            }
            return m_Instance;
        }
    }
}
