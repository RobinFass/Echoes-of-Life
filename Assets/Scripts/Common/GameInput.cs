using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameInput : MonoBehaviour
{
    public static GameInput Instance {get; private set;}
    
    private InputActions inputActions;
    public event EventHandler OnDashEvent; 
    public event EventHandler OnMEnuEvent; 
    public event EventHandler OnAttackEvent; 
        
    private void Awake()
    {
        Instance = this;
        inputActions = new InputActions();
        inputActions.Enable();
        
        inputActions.Player.Dash.performed += OnDash;
        inputActions.Player.Menu.performed += OnMenu;
        inputActions.Player.Attack.performed += OnAttack;
    }

    private void OnDestroy()
    {
        inputActions.Disable();
    }
    
    public Vector2 OnMove()
    {
        return inputActions.Player.Move.ReadValue<Vector2>();
    }
    
    public bool OnSprint()
    {
        return inputActions.Player.Sprint.IsPressed();
    }

    private void OnDash(InputAction.CallbackContext callbackContext)
    {
        OnDashEvent?.Invoke(this, EventArgs.Empty);
    }
    
    private void OnAttack(InputAction.CallbackContext callbackContext)
    {
        OnAttackEvent?.Invoke(this, EventArgs.Empty);
    }
    
    private void OnMenu(InputAction.CallbackContext callbackContext)
    {
        OnMEnuEvent?.Invoke(this, EventArgs.Empty);
    }
}
