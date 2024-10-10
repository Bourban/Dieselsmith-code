using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class PlateSmasher : WorkStation
{
    [SerializeField] 
    private Transform PlateObject;
    [SerializeField] 
    private Transform HammerObject;
    [SerializeField]
    private Transform OutputTransform;

    [SerializeField]
    private GameObject OutputPrefab;

    [SerializeField] 
    private Transform[] HammerTweenPositions;
    
    private SkinnedMeshRenderer Plate;

    private bool bIsReady = true;
    
    protected override void StartWorkUnit()
    {
        base.StartWorkUnit();
    }

    private void Start()
    {
        DOTween.Init(true, false);
        bAutoCompleteOnMaxProgress = false;
    }

    private void Update()
    {
        if (bIsReady && Input.GetKeyDown(KeyCode.Space))
        {
            Hammer();
        }
    }

    private void Hammer()
    {
        bIsReady = false;

        //Move the hammer to the down position, then on complete start moving back to the up position.
        HammerObject.DOLocalRotate(HammerTweenPositions[0].localRotation.eulerAngles, 0.1f, RotateMode.Fast);
        HammerObject.DOLocalMove(HammerTweenPositions[0].localPosition, 0.1f).SetEase(Ease.InSine).OnComplete(() =>
        {
            HammerObject.DOLocalRotate(HammerTweenPositions[1].localRotation.eulerAngles, 0.2f, RotateMode.Fast);
            HammerObject.DOLocalMove(HammerTweenPositions[1].localPosition, 0.2f).OnComplete(() =>  bIsReady = true);
                
            ProgressWork(10.0f);
        });
    }
    
    protected override void ProgressWork(float progress)
    {
        base.ProgressWork(progress);
        
        Plate.SetBlendShapeWeight(0, Math.Clamp(WorkProgress, 0.0f, 100.0f));
    }

    protected override void FinishWorkUnit()
    {
        Instantiate(OutputPrefab, OutputTransform);
        base.FinishWorkUnit();
        Debug.Log("Finished Plate Smashing!");
    }

    public override void SetStationComponentsActive(bool bStationEnabled)
    {
        base.SetStationComponentsActive(bStationEnabled);

        //TODO validate
        PlateObject.gameObject.SetActive(bStationEnabled);
        HammerObject.gameObject.SetActive(bStationEnabled);
        if (bStationEnabled)
        {
            Plate = PlateObject.GetComponentInChildren<SkinnedMeshRenderer>();
        }
    }
}
