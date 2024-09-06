using UnityEngine;
using UnityEngine.InputSystem;

public class HighProfiler : MonoBehaviour
{
    private InputControls _controls;
    public GameObject firearm;
    
    private void Awake()
    {
        _controls = new InputControls();
        firearm.SetActive(false);
    }
    
    private void OnEnable()
    {
        _controls.Profiler.Enable();
        _controls.Profiler.LowProfile.performed += LowProfile;
        _controls.Profiler.HighProfile.performed += HighProfile;
        _controls.Profiler.Firearm.performed += FireRifle;
    }
    
    private void OnDisable()
    {
        _controls.Profiler.Disable();
        _controls.Profiler.LowProfile.performed -= LowProfile;
        _controls.Profiler.HighProfile.performed -= HighProfile;
        _controls.Profiler.Firearm.performed -= FireRifle;
    }
    
    private void LowProfile(InputAction.CallbackContext obj)
    {
        firearm.SetActive(false);
    }

    private void HighProfile(InputAction.CallbackContext obj)
    {
        firearm.SetActive(true);
    }
    
    private void FireRifle(InputAction.CallbackContext obj)
    {
        print("fired");
    }
}
