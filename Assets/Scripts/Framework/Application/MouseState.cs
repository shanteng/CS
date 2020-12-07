using UnityEngine;
using System.Collections;

public class MouseState
{
    private static MouseState m_instance = null;

    public static MouseState instance
    {
        get
        {
            if (m_instance == null) m_instance = new MouseState();
            return m_instance;
        }
    }

    private bool m_mouseDownOrNot = false;
    private bool m_rightNow = false;
    private bool m_mouseRayDirty = true;
    private Vector3 m_mousePositionLast = Vector3.zero;
    private Vector3 m_mousePosition = Vector3.zero;
    private Vector3 m_mouseDelta = Vector3.zero;
    private Ray m_mouseRay;
    private RaycastHit m_mouseRayHit;
    private float m_mouseDistance = 0f;

    public void update()
    {
#if UNITY_EDITOR
        updateMouseState(Input.GetMouseButton(0), Input.mousePosition);
        return;
#endif
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase != TouchPhase.Canceled && touch.phase != TouchPhase.Ended)
            {
                updateMouseState(true, touch.position);
            }
            else
            {
                updateMouseState(false, touch.position);
            }
        }
        else
        {
            updateMouseState(false, m_mousePosition);
        }
    }

    public void updateMouseState(bool downOrNot, Vector3 mousePosition)
    {
        if (m_mouseDownOrNot != downOrNot)
        {
            m_mouseDownOrNot = downOrNot;
            m_rightNow = true;
            m_mouseRayDirty = true;
            m_mousePositionLast = m_mouseDownOrNot ? mousePosition : m_mousePosition;
            m_mousePosition = mousePosition;
            m_mouseDelta.Set(0f, 0f, 0f);
        }
        else
        {
            if (m_mouseDownOrNot)
            {
                m_mousePositionLast = m_mousePosition;
                m_mousePosition = mousePosition;
                m_mouseDelta = m_mousePosition - m_mousePositionLast;
            }
            m_rightNow = false;
            m_mouseRayDirty = true;
        }
    }

    public Vector3 getMousePosition()
    {
        return m_mousePosition;
    }

    public void getMousePosition(ref Vector3 pos)
    {
        pos.Set(m_mousePosition.x, m_mousePosition.y, m_mousePosition.z);
    }

    public Vector3 getMousePositionLast()
    {
        return m_mousePositionLast;
    }

    public Vector3 getMouseDelta()
    {
        return m_mouseDelta;
    }

    public Ray getMouseRay()
    {
        refreshMouseRay();
        return m_mouseRay;
    }

    protected void refreshMouseRay()
    {
        if (m_mouseRayDirty)
        {
            if (Camera.main != null)
            {
                m_mouseRayDirty = false;
                m_mouseRay = Camera.main.ScreenPointToRay(m_mousePosition);
            }
        }
    }

    public bool isMouseDownRightNow()
    {
        return m_mouseDownOrNot && m_rightNow;
    }

    public bool isMouseDown()
    {
        return m_mouseDownOrNot;
    }

    public bool isMouseUpRightNow()
    {
        return !m_mouseDownOrNot && m_rightNow;
    }

    public bool isMouseHit(Collider collider, out RaycastHit hitInfo, float distance)
    {
        if (collider == null)
        {
            hitInfo = m_mouseRayHit;
            return false;
        }
        if (collider.Raycast(getMouseRay(), out m_mouseRayHit, distance))
        {
            hitInfo = m_mouseRayHit;
            return true;
        }
        hitInfo = m_mouseRayHit;
        return false;
    }

    public bool isMouseHit(Collider collider, out RaycastHit hitInfo)
    {
        if (collider == null)
        {
            hitInfo = m_mouseRayHit;
            return false;
        }
        m_mouseDistance = Vector3.Distance(collider.transform.position, Camera.main.transform.position) * 2f;
        if (collider.Raycast(getMouseRay(), out m_mouseRayHit, m_mouseDistance))
        {
            hitInfo = m_mouseRayHit;
            return true;
        }
        hitInfo = m_mouseRayHit;
        return false;
    }

    public bool isMouseHit(Collider collider, float distance)
    {
        if (collider == null)
        {
            return false;
        }
        return collider.Raycast(getMouseRay(), out m_mouseRayHit, distance);
    }

    public bool isMouseHit(Collider collider)
    {
        if (collider == null)
        {
            return false;
        }
        m_mouseDistance = Vector3.Distance(collider.transform.position, Camera.main.transform.position) * 2f;
        return collider.Raycast(getMouseRay(), out m_mouseRayHit, m_mouseDistance);
    }

    public Vector3 getLastHitPoint()
    {
        return m_mouseRayHit.point;
    }
}
