using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OcclusionObject : MonoBehaviour
{
    List<Renderer> Renderers;

    public struct OcclusionSubObject
    {
        public Renderer Renderer;
        public List<Collider> Colliders;
    }


    // Take stock of all subobjects so we can make a copy later
    void Start()
    {
        foreach(Transform transf in this.transform)
        {
            OcclusionSubObject subObject = new OcclusionSubObject();

            Renderer renderer = transf.GetComponent<Renderer>();
            if (renderer)
            {
                subObject.Renderer = renderer;
            }

            List<Collider> Colliders = new(transf.GetComponents<Collider>());
            subObject.Colliders = Colliders;
            
        }
    }

    // Enable/Disable Occlusion effect by replacing original renderers
    public void Occlude(bool occluded = true)
    {
        
    }
}
