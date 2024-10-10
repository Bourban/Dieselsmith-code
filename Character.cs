using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ECharacterState
{
    Default,
    Carrying,
    WorkStation,
    Dialogue
}

public class Character : MonoBehaviour
{
    private Animator Animator;
    private Rigidbody Rigidbody;
    private CharacterController Controller;

    [SerializeField] 
    private float MaxSpeed = 8.5f;
    [SerializeField] 
    private float RotationSpeed = 15.0f;

    private bool HasBeenIntitialised = false;

    private ECharacterState CharacterState;
    
    private static readonly int MoveX = Animator.StringToHash("Horizontal");
    private static readonly int MoveY = Animator.StringToHash("Vertical");
    private static readonly int Moving = Animator.StringToHash("Moving");
    private static readonly int Pickup = Animator.StringToHash("Pickup");
    private static readonly int Drop = Animator.StringToHash("Drop");

    //Maybe private this and make a public setter
    public Interactable FocussedInteractable;
    
    private void Start()
    {
        SetupComponents();
    }

    private void Update()
    {
        HandleMovement(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

        if (FocussedInteractable)
        {
            if (Input.GetKeyDown(KeyCode.F))
            {
                FocussedInteractable.Interact(this);
            }
            
            // Check if we're still overlapping FocussedInteractable.
        }
    }

    private void HandleMovement(float Horizontal, float Vertical)
    {
        Vector3 Input = new Vector3(Horizontal, 0.0f, Vertical).normalized;

        Vector3 Rotation = Input;
        Rotation.y = 0;

        HandleRotation(Rotation);

        Vector3 Movement = Input;
        Movement *= MaxSpeed;

        Animator.SetFloat(MoveX, Input.x);
        Animator.SetFloat(MoveY, Input.y);
        // Animator.SetFloat(MoveY, Mathf.Lerp(0, 1, Controller.velocity.magnitude / MaxSpeed)); 

        bool IsMoving = Movement != Vector3.zero;
        Animator.SetBool(Moving, IsMoving);

        Movement += Physics.gravity;
        Controller.Move(Movement * Time.fixedDeltaTime);
    }

    void HandleRotation(Vector3 direction)
    {
        Vector3 newDirection =
            Vector3.RotateTowards(transform.forward, direction, RotationSpeed * Time.deltaTime, 0.0f);
        transform.rotation = Quaternion.LookRotation(newDirection);
        transform.Rotate(-transform.rotation.eulerAngles.x, 0, 0, Space.World);
    }

    private bool SetupComponents()
    {
        if (HasBeenIntitialised)
        {
            return true;
        }

        Animator = GetComponentInChildren<Animator>();
        Rigidbody = GetComponentInChildren<Rigidbody>();
        Controller = GetComponentInChildren<CharacterController>();

        HasBeenIntitialised = true;

        // Todo: Validate components. 
        return HasBeenIntitialised;
    }

    public void PickupItem(Interactable Item)
    {
        Animator.SetTrigger(Pickup);
        Item.transform.parent = this.transform;
    }

    public void DropItem(Interactable Item)
    {
        Animator.SetTrigger(Drop);
        Item.transform.parent = null;  
    }

    public ECharacterState GetCurrentState()
    {
        return CharacterState;
    }
    
    public void FootR()
    {
        return;
    }
    public void FootL()
    {
        return;
    }
}