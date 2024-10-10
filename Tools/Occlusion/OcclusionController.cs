using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class OcclusionController : MonoBehaviour
{
    // Singleton patter property
    public static OcclusionController Instance { get; private set; }

    public void Awake()
    {
        if(Instance != null && Instance != this) Destroy(this);
        else Instance = this;
    }

    // References
    [Header("Player Occlusion")]
    public Transform PlayerTransform;
    public Material  PlayerOcclusionMaterial;

    //[HideInInspector]
    public BuildingOcclusionController OccludedBuilding;

    // Params
    [Header("Mask")]

    // Outer radius of the occlusion mask
    [Range(0,10)]
    public float OuterMaskSize = 2.0f;

    // Inner radius of the occlusion mask
    [Range(0, 10)]
    public float InnerMaskSize = 1.0f;

    [Header("DepthFade")]

    // Minimum distance from the camera that an object will start being faded out
    [Range(0, 10)]
    public float DepthFadeMinDist = 3.0f;

    // Power of the depth fade-out effect when an object is close to the camera
    [Range(0, 10)]
    public float DepthFadePower = 5.0f;

    [Header("ZSorting")]

    // Z Offset applied when checking if object is in front or behind target
    [Range(-5, 0)]
    public float ZOffset = -1.0f;


    // Slow Update
    protected Coroutine SlowUpdateCoroutine = null;
    protected float SlowUpdateRate = 0.5f;

    // Occluded Objects
    private List<OcclusionObject> OccludedObjects = new List<OcclusionObject>();
    //private List<OcclusionObject> old_OccludedObjects = new List<OcclusionObject>();


    private void Start()
    {
        if (Application.isPlaying)
        {
            SlowUpdateCoroutine = StartCoroutine(SlowUpdate());
        }
    }

    private void LateUpdate()
    {
        if (PlayerOcclusionMaterial)
        {
            if (PlayerTransform)
            {
                var pos = PlayerTransform.position;
                PlayerOcclusionMaterial.SetVector("_Position", pos);
            }

            // TODO: Don't really need to set these parameters every update tick. Can move later on

            // Mask parameters
            PlayerOcclusionMaterial.SetFloat("_OuterMaskSize", OuterMaskSize);
            PlayerOcclusionMaterial.SetFloat("_InnerMaskSize", InnerMaskSize);
            
            // Depth Fade parametersw
            PlayerOcclusionMaterial.SetFloat("_DepthFadeMinDist", DepthFadeMinDist);
            PlayerOcclusionMaterial.SetFloat("_DepthFadePower",   DepthFadePower);

            // ZSorting
            PlayerOcclusionMaterial.SetFloat("_ZOffset", ZOffset);
        }
    }

    // TODO: WIP will we still need this?
    private IEnumerator SlowUpdate()
    {
        while (true)
        {
            if (PlayerTransform)
            {
                var pos = PlayerTransform.position;

                RaycastHit[] hits = { };
                Ray ray = Camera.main.ScreenPointToRay(pos);
                float radius = 20.0f;
                //float max_length = 100.0f;

                if (Physics.SphereCastNonAlloc(ray.origin, radius, ray.direction, hits) > 0)
                {
                    List<OcclusionObject> newOccludedObjects = new List<OcclusionObject>();
                    foreach(var hit in hits)
                    {
                        OcclusionObject obj = hit.transform.GetComponent<OcclusionObject>();
                        if (obj && !OccludedObjects.Contains(obj))
                        {
                            OccludedObjects.Add(obj);
                            obj.Occlude(true);
                        }
                    }

                    
                }
            }
            yield return new WaitForSeconds(SlowUpdateRate);
        }
    }
}
