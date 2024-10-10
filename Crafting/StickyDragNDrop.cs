using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

/// <summary>
/// Allows the item to be dragged and dropped with the mouse, but on release will return to the origin 
/// of the last valid volume it touched.
/// </summary>
public class StickyDragNDrop : MonoBehaviour
{
    private Vector3 MousPos;

    [SerializeField] 
    private bool LockYMovement = true;
   
  
    [SerializeField] 
    private List<WorkStationColliderInfo> StickyColliders = new List<WorkStationColliderInfo>();

    private Collider LastStickyCollider;

    private void Awake()
    {
        //TODO: Validate StickyColliders.Count better.
        LastStickyCollider = StickyColliders[0].collider;
    }

    private Vector3 GetMousePos()
    {
        return Camera.main.WorldToScreenPoint(transform.position);
    }
   
    private void OnMouseDown()
    {
        MousPos = Input.mousePosition - GetMousePos();
    }

    private void OnMouseUp()
    {
        if (StickyColliders.Count == 0)
        {
            Debug.LogWarningFormat("Can't return this object, {0}, to any volumes because StickyAreas list is empty.", this);
            return;
        }
        
        transform.DOMove(LastStickyCollider.transform.position, 0.2f);
        //transform.position = LastStickyCollider.transform.position; // + LastStickyCollider; TODO
    }

    private void OnMouseDrag()
    {
        if (LockYMovement)
        {
            // Instead of this project mouse coord onto XZ plane
            Vector3 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition - MousPos);
            pos.y = transform.position.y;
            
            transform.position = pos;
        }
        else
        {
            transform.position = Camera.main.ScreenToWorldPoint(Input.mousePosition - MousPos);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        foreach (var colliderInfo in StickyColliders)
        {
            if (colliderInfo.collider == other)
            {
                LastStickyCollider = other;

                if (colliderInfo.MatchColliderRotation)
                {
                    // Its doing something wibbly
                    transform.DOLocalRotate(other.transform.localRotation.eulerAngles + colliderInfo.RotationOffset, 0.3f,
                        RotateMode.Fast);

                   // TODO finish this i guess.
                }
            }
        }
    }
    
}

[Serializable]
public struct WorkStationColliderInfo
{
    public Collider collider;
    
    public bool MatchColliderRotation;

    public Vector3 PositionOffset;

    public Vector3 RotationOffset;

}
