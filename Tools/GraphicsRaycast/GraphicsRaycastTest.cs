using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class GraphicsRaycastTest : MonoBehaviour
{
    public float maxDistance = 100f;
    public bool hitSomething;

    private RaycastHit hit;

    void Update()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        hitSomething = GraphicsRaycast.Raycast(ray.origin, ray.direction, out hit, maxDistance);
    }

    private void OnDrawGizmos()
    {
#if UNITY_EDITOR
        GraphicsRaycast.DrawGizmo(hitSomething, this.transform.position, this.transform.forward, hit, maxDistance, 1f);
#endif
    }
}
