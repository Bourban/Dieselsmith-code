using System.Collections.Generic;
using System.Drawing;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using Color = UnityEngine.Color;

[System.Serializable]
[ExecuteAlways]
public class OcclusionVolume : MonoBehaviour
{
    [HideInInspector]
    public int Index = -1;

    public BoxCollider Collider = null;

    [SerializeField]
    [Range(0, 1)]
    private float fadeValue = 1.0f;
    public float FadeValue
    {
        get { return fadeValue; }
        set { UpdateFade(value); }
    }

    [Space(5)]
    public bool DrawDebug = false;

    public UnityEvent<GameObject> OnVolumeEnter;
    public UnityEvent<GameObject> OnVolumeExit;
    
    bool ObjectsVisible = true;

    public void OnEnable() 
    {
        Collider = GetComponent<BoxCollider>();
        if(Collider == null)
        {
            Collider = gameObject.AddComponent(typeof(BoxCollider)) as BoxCollider;
        }
    }

    // Updates

    static public void EnableRenderers(Transform transform, bool enable) 
    {
        Renderer[] renderers = transform.GetComponentsInChildren<Renderer>();
        foreach(Renderer renderer in renderers)
        {
            renderer.enabled = enable;
        }
    }

    public void UpdateFade(float newFadeVal)
    {
        // If FadeValue crosses the 1.0 threshold toggle object visibility 
        if (fadeValue != 1.0f && newFadeVal == 1.0f)
        {
            ObjectsVisible = true;
        }
        else if (fadeValue == 1.0f && newFadeVal < 1.0f)
        {
            ObjectsVisible = false;
        }

        fadeValue = newFadeVal;

        // Find transforms inside "Props" and toggle renderers to hide/show
        var PropsTransform = transform.Find("Props");
        if (!PropsTransform) return;

        foreach(Transform child in PropsTransform)
        {
            EnableRenderers(child, ObjectsVisible);
        }
    }

    // Getters
    public List<Vector4> GetVertices()
    {
        var verts = new List<Vector4>();
        if (Collider)
        {
            // Get axis-vertices for this box volume
            verts.Add(Collider.transform.TransformPoint(Collider.center + new Vector3(-Collider.size.x, -Collider.size.y, -Collider.size.z) * 0.5f));
            verts.Add(Collider.transform.TransformPoint(Collider.center + new Vector3(+Collider.size.x, -Collider.size.y, -Collider.size.z) * 0.5f));
            verts.Add(Collider.transform.TransformPoint(Collider.center + new Vector3(-Collider.size.x, +Collider.size.y, -Collider.size.z) * 0.5f));
            verts.Add(Collider.transform.TransformPoint(Collider.center + new Vector3(-Collider.size.x, -Collider.size.y, +Collider.size.z) * 0.5f));
        }
        return verts;
    }

    public Vector3 GetFoward()
    {
        if (!Collider) return Vector3.forward;
        return Collider.transform.forward;
    }

    // Collision Events
    private void OnTriggerEnter(Collider other)
    {
        if (OnVolumeEnter != null) OnVolumeEnter.Invoke(other.gameObject);
    }

    private void OnTriggerExit(Collider other)
    {
        if (OnVolumeExit != null) OnVolumeExit.Invoke(other.gameObject);
    }

    // Debug
    public void DrawDebugArrowGizmo()
    {
        if (Collider)
        {
            Handles.ArrowHandleCap(0, Collider.bounds.center, Quaternion.LookRotation(Collider.transform.forward, Vector3.up), 1.0f, EventType.Repaint);
        }
    }

    public void DrawDebugReferencialsGizmo()
    {
        var verts = GetVertices();
        if (verts.Count == 4)
        {
            var i = Vector4.Normalize(verts[1] - verts[0]);
            var j = Vector4.Normalize(verts[2] - verts[0]);
            var k = Vector4.Normalize(verts[3] - verts[0]);

            Handles.DrawBezier(verts[0], verts[0] + i, verts[0], verts[0] + i, Color.red, null, 5);
            Handles.DrawBezier(verts[0], verts[0] + j, verts[0], verts[0] + j, Color.green, null, 5);
            Handles.DrawBezier(verts[0], verts[0] + k, verts[0], verts[0] + k, Color.blue, null, 5);
        }
    }

    private void OnDrawGizmos()
    {
        if (!DrawDebug) return;
        DrawDebugArrowGizmo();
    }

}