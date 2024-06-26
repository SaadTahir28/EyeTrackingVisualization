// Copyright © 2018 – Property of Tobii AB (publ) - All Rights Reserved

using UnityEngine;

namespace Tobii.XR
{
    public class G2OM_DebugTool : MonoBehaviour
    {
#pragma warning disable 0649 // Field is never assigned to, and will always have its default value null
        [Header("Debug")]
        [Tooltip("Can be null if there is no need for debug visualization")]
        [SerializeField] private  G2OM_DebugVisualization debugVisualization;
        
#if ENABLE_INPUT_SYSTEM
        [SerializeField] private UnityEngine.InputSystem.Key toggleVisibility = UnityEngine.InputSystem.Key.Space;
        [SerializeField] private UnityEngine.InputSystem.Key toggleFreeze = UnityEngine.InputSystem.Key.LeftCtrl;
#else
        [SerializeField] private KeyCode toggleVisibility = KeyCode.Space;
        [SerializeField] private KeyCode toggleFreeze = KeyCode.LeftControl;
#endif
#pragma warning restore 0649

        private void Update()
        {
            if (debugVisualization == null) return;

#if ENABLE_INPUT_SYSTEM
            var visibilityToggled = UnityEngine.InputSystem.Keyboard.current[toggleVisibility].wasPressedThisFrame;
#else
            var visibilityToggled = Input.GetKeyDown(toggleVisibility);
#endif
            if (visibilityToggled)
            {
                debugVisualization.ToggleVisualization();
            }

#if ENABLE_INPUT_SYSTEM
            var freezeToggled = UnityEngine.InputSystem.Keyboard.current[toggleFreeze].wasPressedThisFrame;
#else
            var freezeToggled = Input.GetKeyDown(toggleFreeze);
#endif
            if (freezeToggled)
            {
                debugVisualization.ToggleFreeze();
            }
        }
    }
}