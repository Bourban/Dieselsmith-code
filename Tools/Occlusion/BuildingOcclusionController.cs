using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.Rendering;
using UnityEngine;

[ExecuteAlways]
public class BuildingOcclusionController : MonoBehaviour
{
    [System.Serializable]
    public class Floor
    {
        public string name = "Floor";

        public List<OcclusionVolume> AreaVolumes = null;
        public List<OcclusionVolume> WallVolumes = null;

        [System.Serializable]
        public class ObjectDictionary
        {
            public Dictionary<GameObject, int> ObjectOccurences = new Dictionary<GameObject, int>();

            public int Add(GameObject obj)
            {
                ObjectOccurences.TryGetValue(obj, out var count);
                ObjectOccurences[obj] = ++count;
                return count;
            }

            public int Remove(GameObject obj)
            {
                if (ObjectOccurences.TryGetValue(obj, out var count) && --count > 0)
                {
                    ObjectOccurences[obj] = count;
                    return count;
                }
                else
                {
                    ObjectOccurences.Remove(obj);
                    return 0;
                }
            }
        }

        [HideInInspector]
        public ObjectDictionary Objects = new ObjectDictionary();

        [HideInInspector]
        public bool IsPlayerInside = false;

        [HideInInspector]
        public bool IsVisible = true;
    }

    // References
    [Header("Materials")]
    public Material BuildingOcclusionMaterial = null;
    public Material BuildingGlassOcclusionMaterial = null;

    [Space(5)]
    public List<Floor> Floors;

    // Shader params - Keep in sync with variable in shader
    private static int MAX_VOLUME_NUMBER = 25;
    private List<Vector4> VolumeVerts       = new List<Vector4>(new Vector4[MAX_VOLUME_NUMBER * 4]);
    private List<float>   VolumeFadeValues  = new List<float>(new float[MAX_VOLUME_NUMBER]);
    private int VolumeNumber = 0;

    [Header("Debug")]
    public bool DrawDebug = false;


    // Getter
    public Floor GetCurrentFloor()
    {
        Floor currentFloor = null;
        foreach(var floor in Floors)
        {
            if (floor.IsPlayerInside)
            {
                currentFloor = floor;
            }
        }
        return currentFloor;
    }

    // Load and Update Volumes
    private void LoadVolumes()
    {
        VolumeNumber = 0;
        Action<List<OcclusionVolume>> VolumesGetVerts = (volumes) =>
        {
            foreach (var volume in volumes)
            {
                volume.Index = VolumeNumber++;
                var verts = volume.GetVertices();
                if(verts.Count == 4)
                {
                    VolumeVerts[volume.Index * 4 + 0] = verts[0];
                    VolumeVerts[volume.Index * 4 + 1] = verts[1];
                    VolumeVerts[volume.Index * 4 + 2] = verts[2];
                    VolumeVerts[volume.Index * 4 + 3] = verts[3];
                }
            }
        };

        // Get verts and load them into the shader
        foreach (var floor in Floors)
        {
            VolumesGetVerts(floor.AreaVolumes);
            VolumesGetVerts(floor.WallVolumes);
        }

        // Set shader params
        if (BuildingOcclusionMaterial)
        {
            BuildingOcclusionMaterial.SetVectorArray("_VolumeVerts", VolumeVerts);
            BuildingOcclusionMaterial.SetInt("_VolumeNumber", VolumeNumber);
        }

        if (BuildingGlassOcclusionMaterial)
        {
            BuildingGlassOcclusionMaterial.SetVectorArray("_VolumeVerts", VolumeVerts);
            BuildingGlassOcclusionMaterial.SetInt("_VolumeNumber", VolumeNumber);
        }
    }

    private void UpdateVolumes()
    {
        Action<Floor, List<OcclusionVolume>> WallVolumesUpdateFade = (floor, volumes) =>
        {
            foreach (var volume in volumes)
            {
                var fade = 1.0f;
                var currentFloor = GetCurrentFloor();
                if (currentFloor != null)
                {
                    if(floor == currentFloor)
                    {
                        fade = (Vector3.Dot(Camera.main.transform.forward, volume.GetFoward()) >= -0.2f) ? 1.0f : 0.0f;
                    }
                    else if (Floors.IndexOf(floor) > Floors.IndexOf(currentFloor))
                    {
                        fade = 0.0f;
                    }
                }
                volume.FadeValue = Mathf.MoveTowards(volume.FadeValue, fade, 0.01f);

                VolumeFadeValues[volume.Index] = volume.FadeValue;
            }
        };

        Action<Floor, List<OcclusionVolume>> AreaVolumesUpdateFade = (floor, volumes) =>
        {
            bool visible = true;
            foreach (var volume in volumes)
            {
                var fade = 1.0f;
                var currentFloor = GetCurrentFloor();
                if (currentFloor != null)
                {
                    if(Floors.IndexOf(floor) > Floors.IndexOf(currentFloor))
                    {
                        fade = 0.0f;
                        visible = false;
                    }
                }
                volume.FadeValue = Mathf.MoveTowards(volume.FadeValue, fade, 0.01f);

                VolumeFadeValues[volume.Index] = volume.FadeValue;
            }
            floor.IsVisible = visible;
        };

        // Only update if currently in this building
        if (OcclusionController.Instance.OccludedBuilding != this) return;

        // Update volume fade values depending on view angle
        foreach (var floor in Floors)
        {
            AreaVolumesUpdateFade(floor, floor.AreaVolumes);
            WallVolumesUpdateFade(floor, floor.WallVolumes);
        }

        UpdateVolumeObjects();

        // Set shader params
        if(BuildingOcclusionMaterial)       BuildingOcclusionMaterial.SetFloatArray("_VolumeFadeValues", VolumeFadeValues);
        if(BuildingGlassOcclusionMaterial)  BuildingGlassOcclusionMaterial.SetFloatArray("_VolumeFadeValues", VolumeFadeValues);
    }

    private void EditorUpdateVolumes()
    {
        Action<Floor, List<OcclusionVolume>> WallVolumesUpdateFade = (floor, volumes) =>
        {
            foreach (var volume in volumes)
            {
                VolumeFadeValues[volume.Index] = volume.FadeValue;
            }
        };

        Action<Floor, List<OcclusionVolume>> AreaVolumesUpdateFade = (floor, volumes) =>
        {
            foreach (var volume in volumes)
            {
                VolumeFadeValues[volume.Index] = volume.FadeValue;
            }
        };

        // Update volume fade values depending on view angle
        foreach (var floor in Floors)
        {
            AreaVolumesUpdateFade(floor, floor.AreaVolumes);
            WallVolumesUpdateFade(floor, floor.WallVolumes);
        }

        // Set shader params
        if (BuildingOcclusionMaterial)      BuildingOcclusionMaterial.SetFloatArray("_VolumeFadeValues", VolumeFadeValues);
        if (BuildingGlassOcclusionMaterial) BuildingGlassOcclusionMaterial.SetFloatArray("_VolumeFadeValues", VolumeFadeValues);
    }

    private void EditorAddVolumes()
    {
        Action<Floor, List<OcclusionVolume>, bool> AddVolumes = (floor, volumes, isWallVolume) =>
        {
            for (var i = 0; i < volumes.Count; i++)
            {
                if (volumes[i] == null)
                {
                    var parentTransf = (isWallVolume) ? floor.AreaVolumes[0].transform : transform;

                    var prefab = Resources.Load("Occlusion/OcclusionVolume");
                    var obj = PrefabUtility.InstantiatePrefab(prefab, parentTransf);
                    obj.name = "OcclusionVolume";
                    volumes[i] = obj.GetComponent<OcclusionVolume>();
                }
            }
        };
        
        // If null volume (one was just added to the list) add a volume prefab to the transform
        foreach (var floor in Floors)
        {
            AddVolumes(floor, floor.AreaVolumes, false);
            AddVolumes(floor, floor.WallVolumes, true);
        }
    }

    // Object Detection in Volumes
    private void InitVolumeObjectDetection()
    {

        Action<Floor, GameObject> OnVolumeEnter = (floor, gameObject) =>
        {
            // Update objects
            var count = floor.Objects.Add(gameObject);

            // Check for player
            if (gameObject == OcclusionController.Instance.PlayerTransform.gameObject)
            {
                floor.IsPlayerInside = true;
                OcclusionController.Instance.OccludedBuilding = this;

                LoadVolumes();
            }
        };

        Action<Floor, GameObject> OnVolumeExit = (floor, gameObject) =>
        {
            // Update objects
            var count = floor.Objects.Remove(gameObject);

            // Check for player
            if (gameObject == OcclusionController.Instance.PlayerTransform.gameObject)
            {
                if(count == 0)
                {
                    floor.IsPlayerInside = false;
                }
            }
        };

        foreach (var floor in Floors)
        {
            foreach (var areaVolume in floor.AreaVolumes)
            {
                if (areaVolume.tag == "OcclusionIgnorePlayer") continue;
                areaVolume.OnVolumeEnter.AddListener((gameObject) => OnVolumeEnter(floor, gameObject));
                areaVolume.OnVolumeExit.AddListener((gameObject)  => OnVolumeExit(floor, gameObject));
            }
        }
    }

    private void DeInitVolumeObjectDetection()
    {
        foreach (var floor in Floors)
        {
            foreach (var areaVolume in floor.AreaVolumes)
            {
                areaVolume.OnVolumeEnter.RemoveAllListeners();
                areaVolume.OnVolumeExit.RemoveAllListeners();
            }
        }
    }

    private void UpdateVolumeObjects()
    {
        Action<Floor> UpdateFloorObjects = (floor) =>
        {
            foreach (GameObject obj in floor.Objects.ObjectOccurences.Keys)
            {
                if (obj == OcclusionController.Instance.PlayerTransform.gameObject) return;
                OcclusionVolume.EnableRenderers(obj.transform, floor.IsVisible);
            }
        };

        foreach (var floor in Floors)
        {
            foreach (var areaVolume in floor.AreaVolumes)
            {
                UpdateFloorObjects(floor);
            }
        }
    }

    // Methods
    private void OnEnable()
    {
        LoadVolumes();

        if (Application.isPlaying)
        {
            InitVolumeObjectDetection();
        }
    }

    private void OnValidate()
    {
        if (Application.isEditor)
        {
            EditorAddVolumes();
            LoadVolumes();
        }
    }

    private void LateUpdate()
    {
        if (Application.isPlaying)
        {
            UpdateVolumes();
        }
        else if (Application.isEditor)
        {
            LoadVolumes();
            EditorUpdateVolumes();
        }
    }

    private void OnDisable()
    {
        if (Application.isPlaying)
        {
            DeInitVolumeObjectDetection();
        }
    }

    // Debug
    private void OnDrawGizmos()
    {
        if (!DrawDebug) return;

        Action<List<OcclusionVolume>> VolumesDrawDebug = (volumes) =>
        {
            foreach (var volume in volumes)
            {
                volume.DrawDebugArrowGizmo();
            }
        };

        foreach (var floor in Floors)
        {
            //VolumesDrawDebug(floor.AreaVolumes);
            VolumesDrawDebug(floor.WallVolumes);
        }
    }
}