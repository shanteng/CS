using System;
using System.Collections;
using UnityEditor;

namespace UnityEngine.PlayerIdentity.Utils
{
    internal interface ICoroutineExecutor : IDisposable
    {
        Coroutine StartCoroutine(IEnumerator enumerator);
    }
    
    /// <summary>
    /// A helper class for running coroutines from non <see cref="T:UnityEngine.MonoBehaviour" /> classes.
    /// </summary>
    internal class CoroutineExecutor : ICoroutineExecutor
    {
        private static ICoroutineExecutor s_instance = new CoroutineExecutor();

        public static ICoroutineExecutor Instance
        {
            get { return s_instance; }
            internal set { s_instance = value; }
        }
        
        internal class CoroutineExecutorMonoBehaviour : MonoBehaviour
        {
            public int referenceCount;
        }
        
        private GameObject m_GameObject;
        private CoroutineExecutorMonoBehaviour m_Component;
        private bool m_Disposed;
        
        public CoroutineExecutor()
        {
            const string hiddenObjectNames = "UnityEngine_PlayerIdentity_CoroutineExecutorHiddenGameObject";
            var existingCoroutineExecutorGameObject = GameObject.Find(hiddenObjectNames);
            if (existingCoroutineExecutorGameObject != null)
            {
                m_GameObject = existingCoroutineExecutorGameObject;
                m_Component = m_GameObject.GetComponent<CoroutineExecutorMonoBehaviour>();
                m_Component.referenceCount++;
                return;
            }
            m_GameObject = new GameObject(hiddenObjectNames) {
                hideFlags = HideFlags.HideInInspector | HideFlags.HideInHierarchy
            };
            m_Component = m_GameObject.AddComponent<CoroutineExecutorMonoBehaviour>();
            m_Component.referenceCount++;
            GameObject.DontDestroyOnLoad(m_GameObject);
        }
        
        public Coroutine StartCoroutine(IEnumerator enumerator)
        {
            return m_Component.StartCoroutine(enumerator);
        }
        
        public void Dispose()
        {
            if (!m_Disposed)
            {
                m_Disposed = true;
                m_Component.referenceCount--;
                if (m_Component.referenceCount == 0)
                {
                    Object.DestroyImmediate(m_GameObject);
                }
                m_GameObject = null;
                m_Component = null;
            }
        }
    }
}