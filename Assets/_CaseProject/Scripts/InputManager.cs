using System;
using UnityEngine;

/// <summary>
/// A globally accessible Singleton that abstracts raw player input (both Mobile Touch and PC Mouse) 
/// into universal pointer events. Ensures input is only processed during active gameplay states 
/// and broadcasts interaction phases (Down, Drag, Up) to subscribed listeners.
/// </summary>
public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    public event Action<Vector2> OnPointerDownEvent;
    public event Action<Vector2> OnPointerDragEvent;
    public event Action<Vector2> OnPointerUpEvent;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Update()
    {
        if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.Playing)
            return;

        HandlePointerInput();
    }

    private void HandlePointerInput()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                OnPointerDownEvent?.Invoke(touch.position);
            }
            else if (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary)
            {
                OnPointerDragEvent?.Invoke(touch.position);
            }
            else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
            {
                OnPointerUpEvent?.Invoke(touch.position);
            }
        }
        else
        {
            if (Input.GetMouseButtonDown(0))
            {
                OnPointerDownEvent?.Invoke(Input.mousePosition);
            }
            else if (Input.GetMouseButton(0))
            {
                OnPointerDragEvent?.Invoke(Input.mousePosition);
            }
            else if (Input.GetMouseButtonUp(0))
            {
                OnPointerUpEvent?.Invoke(Input.mousePosition);
            }
        }
    }
}
