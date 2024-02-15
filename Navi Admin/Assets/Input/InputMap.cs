//------------------------------------------------------------------------------
// <auto-generated>
//     This code was auto-generated by com.unity.inputsystem:InputActionCodeGenerator
//     version 1.6.3
//     from Assets/Input/InputMap.inputactions
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public partial class @InputMap: IInputActionCollection2, IDisposable
{
    public InputActionAsset asset { get; }
    public @InputMap()
    {
        asset = InputActionAsset.FromJson(@"{
    ""name"": ""InputMap"",
    ""maps"": [
        {
            ""name"": ""MapEditor"",
            ""id"": ""ff6e5b7b-7c9f-4aa6-9939-6d929fa31a19"",
            ""actions"": [
                {
                    ""name"": ""Zoom"",
                    ""type"": ""PassThrough"",
                    ""id"": ""4391d657-4655-46d2-8698-921f37206760"",
                    ""expectedControlType"": """",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""MoveCamera"",
                    ""type"": ""Button"",
                    ""id"": ""9cc77432-eabd-49d3-851b-74dd54446ad3"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""Draw"",
                    ""type"": ""Button"",
                    ""id"": ""1edc27b6-fbb6-44f0-a7f5-627876b305f4"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""EndDraw"",
                    ""type"": ""Button"",
                    ""id"": ""26a4a3e9-e5e3-4f3c-8b5f-894d15244f76"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""347b2298-4201-4111-9869-e0320efe7ffc"",
                    ""path"": ""<Mouse>/scroll/y"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Zoom"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""a94c076a-a09f-4c0b-a983-9974ad3e6937"",
                    ""path"": ""<Mouse>/middleButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""MoveCamera"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""76b61535-af8f-400b-963d-4d7d4015cd08"",
                    ""path"": ""<Mouse>/leftButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Draw"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""0a954739-fe40-4227-8321-f6b315b92a70"",
                    ""path"": ""<Mouse>/rightButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""EndDraw"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        }
    ],
    ""controlSchemes"": []
}");
        // MapEditor
        m_MapEditor = asset.FindActionMap("MapEditor", throwIfNotFound: true);
        m_MapEditor_Zoom = m_MapEditor.FindAction("Zoom", throwIfNotFound: true);
        m_MapEditor_MoveCamera = m_MapEditor.FindAction("MoveCamera", throwIfNotFound: true);
        m_MapEditor_Draw = m_MapEditor.FindAction("Draw", throwIfNotFound: true);
        m_MapEditor_EndDraw = m_MapEditor.FindAction("EndDraw", throwIfNotFound: true);
    }

    public void Dispose()
    {
        UnityEngine.Object.Destroy(asset);
    }

    public InputBinding? bindingMask
    {
        get => asset.bindingMask;
        set => asset.bindingMask = value;
    }

    public ReadOnlyArray<InputDevice>? devices
    {
        get => asset.devices;
        set => asset.devices = value;
    }

    public ReadOnlyArray<InputControlScheme> controlSchemes => asset.controlSchemes;

    public bool Contains(InputAction action)
    {
        return asset.Contains(action);
    }

    public IEnumerator<InputAction> GetEnumerator()
    {
        return asset.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Enable()
    {
        asset.Enable();
    }

    public void Disable()
    {
        asset.Disable();
    }

    public IEnumerable<InputBinding> bindings => asset.bindings;

    public InputAction FindAction(string actionNameOrId, bool throwIfNotFound = false)
    {
        return asset.FindAction(actionNameOrId, throwIfNotFound);
    }

    public int FindBinding(InputBinding bindingMask, out InputAction action)
    {
        return asset.FindBinding(bindingMask, out action);
    }

    // MapEditor
    private readonly InputActionMap m_MapEditor;
    private List<IMapEditorActions> m_MapEditorActionsCallbackInterfaces = new List<IMapEditorActions>();
    private readonly InputAction m_MapEditor_Zoom;
    private readonly InputAction m_MapEditor_MoveCamera;
    private readonly InputAction m_MapEditor_Draw;
    private readonly InputAction m_MapEditor_EndDraw;
    public struct MapEditorActions
    {
        private @InputMap m_Wrapper;
        public MapEditorActions(@InputMap wrapper) { m_Wrapper = wrapper; }
        public InputAction @Zoom => m_Wrapper.m_MapEditor_Zoom;
        public InputAction @MoveCamera => m_Wrapper.m_MapEditor_MoveCamera;
        public InputAction @Draw => m_Wrapper.m_MapEditor_Draw;
        public InputAction @EndDraw => m_Wrapper.m_MapEditor_EndDraw;
        public InputActionMap Get() { return m_Wrapper.m_MapEditor; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(MapEditorActions set) { return set.Get(); }
        public void AddCallbacks(IMapEditorActions instance)
        {
            if (instance == null || m_Wrapper.m_MapEditorActionsCallbackInterfaces.Contains(instance)) return;
            m_Wrapper.m_MapEditorActionsCallbackInterfaces.Add(instance);
            @Zoom.started += instance.OnZoom;
            @Zoom.performed += instance.OnZoom;
            @Zoom.canceled += instance.OnZoom;
            @MoveCamera.started += instance.OnMoveCamera;
            @MoveCamera.performed += instance.OnMoveCamera;
            @MoveCamera.canceled += instance.OnMoveCamera;
            @Draw.started += instance.OnDraw;
            @Draw.performed += instance.OnDraw;
            @Draw.canceled += instance.OnDraw;
            @EndDraw.started += instance.OnEndDraw;
            @EndDraw.performed += instance.OnEndDraw;
            @EndDraw.canceled += instance.OnEndDraw;
        }

        private void UnregisterCallbacks(IMapEditorActions instance)
        {
            @Zoom.started -= instance.OnZoom;
            @Zoom.performed -= instance.OnZoom;
            @Zoom.canceled -= instance.OnZoom;
            @MoveCamera.started -= instance.OnMoveCamera;
            @MoveCamera.performed -= instance.OnMoveCamera;
            @MoveCamera.canceled -= instance.OnMoveCamera;
            @Draw.started -= instance.OnDraw;
            @Draw.performed -= instance.OnDraw;
            @Draw.canceled -= instance.OnDraw;
            @EndDraw.started -= instance.OnEndDraw;
            @EndDraw.performed -= instance.OnEndDraw;
            @EndDraw.canceled -= instance.OnEndDraw;
        }

        public void RemoveCallbacks(IMapEditorActions instance)
        {
            if (m_Wrapper.m_MapEditorActionsCallbackInterfaces.Remove(instance))
                UnregisterCallbacks(instance);
        }

        public void SetCallbacks(IMapEditorActions instance)
        {
            foreach (var item in m_Wrapper.m_MapEditorActionsCallbackInterfaces)
                UnregisterCallbacks(item);
            m_Wrapper.m_MapEditorActionsCallbackInterfaces.Clear();
            AddCallbacks(instance);
        }
    }
    public MapEditorActions @MapEditor => new MapEditorActions(this);
    public interface IMapEditorActions
    {
        void OnZoom(InputAction.CallbackContext context);
        void OnMoveCamera(InputAction.CallbackContext context);
        void OnDraw(InputAction.CallbackContext context);
        void OnEndDraw(InputAction.CallbackContext context);
    }
}
