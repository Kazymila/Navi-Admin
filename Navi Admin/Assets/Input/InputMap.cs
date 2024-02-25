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
                    ""name"": ""Move"",
                    ""type"": ""Button"",
                    ""id"": ""9cc77432-eabd-49d3-851b-74dd54446ad3"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""Hand"",
                    ""type"": ""Button"",
                    ""id"": ""a2bcc8e4-647b-4803-a209-a75d69f82a1a"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""Click"",
                    ""type"": ""Button"",
                    ""id"": ""18b9c35b-bdfe-465a-b066-b5d3a0d1d503"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""Position"",
                    ""type"": ""Value"",
                    ""id"": ""1edc27b6-fbb6-44f0-a7f5-627876b305f4"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": true
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
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""76b61535-af8f-400b-963d-4d7d4015cd08"",
                    ""path"": ""<Mouse>/position"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Position"",
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
                },
                {
                    ""name"": """",
                    ""id"": ""310cc194-16ad-4104-b05a-f04bdb56a898"",
                    ""path"": ""<Mouse>/leftButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Click"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""2de1f4f2-6bf6-4373-ab3e-294bafe91ece"",
                    ""path"": ""<Mouse>/leftButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Hand"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        },
        {
            ""name"": ""RenderView"",
            ""id"": ""7df759c4-8e07-4819-9081-a02919bc8cfc"",
            ""actions"": [
                {
                    ""name"": ""Look"",
                    ""type"": ""PassThrough"",
                    ""id"": ""ecd107d4-4caf-4930-9c2f-e16f13225043"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""Zoom"",
                    ""type"": ""PassThrough"",
                    ""id"": ""4dbe533d-68fa-4e1d-b692-2a5c3c2780f0"",
                    ""expectedControlType"": """",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""Move"",
                    ""type"": ""Button"",
                    ""id"": ""b8274b6f-8194-4e9d-859c-021685c9b62c"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""Hand"",
                    ""type"": ""Button"",
                    ""id"": ""d3d20e5b-cfc5-4d86-9fbf-12066511b6f5"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""Rotate"",
                    ""type"": ""Button"",
                    ""id"": ""6463bbad-69a3-4933-8600-6b04f7ebb87e"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""Position"",
                    ""type"": ""Value"",
                    ""id"": ""36feec16-5b7b-461d-98b1-425a8e91ea4a"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": true
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""66c974bf-5eb2-4fc6-93af-9659a1174a4b"",
                    ""path"": ""<Mouse>/delta"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Look"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""cf43390f-112f-46bc-9e1e-ead451129192"",
                    ""path"": ""<Mouse>/rightButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Rotate"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""167d4118-830c-4297-a46f-a6091a6d703b"",
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
                    ""id"": ""06e6f946-5d60-4cf0-a118-a7a454dcf6fa"",
                    ""path"": ""<Mouse>/position"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Position"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""f63177d4-2a33-4b3f-bb97-48f48f5b16e1"",
                    ""path"": ""<Mouse>/leftButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Hand"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""6695e7f6-8a3d-4cfb-a8b6-5b73c77bc72f"",
                    ""path"": ""<Mouse>/middleButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""a70bdc9a-91f2-434c-8313-8849ecf838c4"",
                    ""path"": ""<Mouse>/leftButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
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
        m_MapEditor_Move = m_MapEditor.FindAction("Move", throwIfNotFound: true);
        m_MapEditor_Hand = m_MapEditor.FindAction("Hand", throwIfNotFound: true);
        m_MapEditor_Click = m_MapEditor.FindAction("Click", throwIfNotFound: true);
        m_MapEditor_Position = m_MapEditor.FindAction("Position", throwIfNotFound: true);
        m_MapEditor_EndDraw = m_MapEditor.FindAction("EndDraw", throwIfNotFound: true);
        // RenderView
        m_RenderView = asset.FindActionMap("RenderView", throwIfNotFound: true);
        m_RenderView_Look = m_RenderView.FindAction("Look", throwIfNotFound: true);
        m_RenderView_Zoom = m_RenderView.FindAction("Zoom", throwIfNotFound: true);
        m_RenderView_Move = m_RenderView.FindAction("Move", throwIfNotFound: true);
        m_RenderView_Hand = m_RenderView.FindAction("Hand", throwIfNotFound: true);
        m_RenderView_Rotate = m_RenderView.FindAction("Rotate", throwIfNotFound: true);
        m_RenderView_Position = m_RenderView.FindAction("Position", throwIfNotFound: true);
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
    private readonly InputAction m_MapEditor_Move;
    private readonly InputAction m_MapEditor_Hand;
    private readonly InputAction m_MapEditor_Click;
    private readonly InputAction m_MapEditor_Position;
    private readonly InputAction m_MapEditor_EndDraw;
    public struct MapEditorActions
    {
        private @InputMap m_Wrapper;
        public MapEditorActions(@InputMap wrapper) { m_Wrapper = wrapper; }
        public InputAction @Zoom => m_Wrapper.m_MapEditor_Zoom;
        public InputAction @Move => m_Wrapper.m_MapEditor_Move;
        public InputAction @Hand => m_Wrapper.m_MapEditor_Hand;
        public InputAction @Click => m_Wrapper.m_MapEditor_Click;
        public InputAction @Position => m_Wrapper.m_MapEditor_Position;
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
            @Move.started += instance.OnMove;
            @Move.performed += instance.OnMove;
            @Move.canceled += instance.OnMove;
            @Hand.started += instance.OnHand;
            @Hand.performed += instance.OnHand;
            @Hand.canceled += instance.OnHand;
            @Click.started += instance.OnClick;
            @Click.performed += instance.OnClick;
            @Click.canceled += instance.OnClick;
            @Position.started += instance.OnPosition;
            @Position.performed += instance.OnPosition;
            @Position.canceled += instance.OnPosition;
            @EndDraw.started += instance.OnEndDraw;
            @EndDraw.performed += instance.OnEndDraw;
            @EndDraw.canceled += instance.OnEndDraw;
        }

        private void UnregisterCallbacks(IMapEditorActions instance)
        {
            @Zoom.started -= instance.OnZoom;
            @Zoom.performed -= instance.OnZoom;
            @Zoom.canceled -= instance.OnZoom;
            @Move.started -= instance.OnMove;
            @Move.performed -= instance.OnMove;
            @Move.canceled -= instance.OnMove;
            @Hand.started -= instance.OnHand;
            @Hand.performed -= instance.OnHand;
            @Hand.canceled -= instance.OnHand;
            @Click.started -= instance.OnClick;
            @Click.performed -= instance.OnClick;
            @Click.canceled -= instance.OnClick;
            @Position.started -= instance.OnPosition;
            @Position.performed -= instance.OnPosition;
            @Position.canceled -= instance.OnPosition;
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

    // RenderView
    private readonly InputActionMap m_RenderView;
    private List<IRenderViewActions> m_RenderViewActionsCallbackInterfaces = new List<IRenderViewActions>();
    private readonly InputAction m_RenderView_Look;
    private readonly InputAction m_RenderView_Zoom;
    private readonly InputAction m_RenderView_Move;
    private readonly InputAction m_RenderView_Hand;
    private readonly InputAction m_RenderView_Rotate;
    private readonly InputAction m_RenderView_Position;
    public struct RenderViewActions
    {
        private @InputMap m_Wrapper;
        public RenderViewActions(@InputMap wrapper) { m_Wrapper = wrapper; }
        public InputAction @Look => m_Wrapper.m_RenderView_Look;
        public InputAction @Zoom => m_Wrapper.m_RenderView_Zoom;
        public InputAction @Move => m_Wrapper.m_RenderView_Move;
        public InputAction @Hand => m_Wrapper.m_RenderView_Hand;
        public InputAction @Rotate => m_Wrapper.m_RenderView_Rotate;
        public InputAction @Position => m_Wrapper.m_RenderView_Position;
        public InputActionMap Get() { return m_Wrapper.m_RenderView; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(RenderViewActions set) { return set.Get(); }
        public void AddCallbacks(IRenderViewActions instance)
        {
            if (instance == null || m_Wrapper.m_RenderViewActionsCallbackInterfaces.Contains(instance)) return;
            m_Wrapper.m_RenderViewActionsCallbackInterfaces.Add(instance);
            @Look.started += instance.OnLook;
            @Look.performed += instance.OnLook;
            @Look.canceled += instance.OnLook;
            @Zoom.started += instance.OnZoom;
            @Zoom.performed += instance.OnZoom;
            @Zoom.canceled += instance.OnZoom;
            @Move.started += instance.OnMove;
            @Move.performed += instance.OnMove;
            @Move.canceled += instance.OnMove;
            @Hand.started += instance.OnHand;
            @Hand.performed += instance.OnHand;
            @Hand.canceled += instance.OnHand;
            @Rotate.started += instance.OnRotate;
            @Rotate.performed += instance.OnRotate;
            @Rotate.canceled += instance.OnRotate;
            @Position.started += instance.OnPosition;
            @Position.performed += instance.OnPosition;
            @Position.canceled += instance.OnPosition;
        }

        private void UnregisterCallbacks(IRenderViewActions instance)
        {
            @Look.started -= instance.OnLook;
            @Look.performed -= instance.OnLook;
            @Look.canceled -= instance.OnLook;
            @Zoom.started -= instance.OnZoom;
            @Zoom.performed -= instance.OnZoom;
            @Zoom.canceled -= instance.OnZoom;
            @Move.started -= instance.OnMove;
            @Move.performed -= instance.OnMove;
            @Move.canceled -= instance.OnMove;
            @Hand.started -= instance.OnHand;
            @Hand.performed -= instance.OnHand;
            @Hand.canceled -= instance.OnHand;
            @Rotate.started -= instance.OnRotate;
            @Rotate.performed -= instance.OnRotate;
            @Rotate.canceled -= instance.OnRotate;
            @Position.started -= instance.OnPosition;
            @Position.performed -= instance.OnPosition;
            @Position.canceled -= instance.OnPosition;
        }

        public void RemoveCallbacks(IRenderViewActions instance)
        {
            if (m_Wrapper.m_RenderViewActionsCallbackInterfaces.Remove(instance))
                UnregisterCallbacks(instance);
        }

        public void SetCallbacks(IRenderViewActions instance)
        {
            foreach (var item in m_Wrapper.m_RenderViewActionsCallbackInterfaces)
                UnregisterCallbacks(item);
            m_Wrapper.m_RenderViewActionsCallbackInterfaces.Clear();
            AddCallbacks(instance);
        }
    }
    public RenderViewActions @RenderView => new RenderViewActions(this);
    public interface IMapEditorActions
    {
        void OnZoom(InputAction.CallbackContext context);
        void OnMove(InputAction.CallbackContext context);
        void OnHand(InputAction.CallbackContext context);
        void OnClick(InputAction.CallbackContext context);
        void OnPosition(InputAction.CallbackContext context);
        void OnEndDraw(InputAction.CallbackContext context);
    }
    public interface IRenderViewActions
    {
        void OnLook(InputAction.CallbackContext context);
        void OnZoom(InputAction.CallbackContext context);
        void OnMove(InputAction.CallbackContext context);
        void OnHand(InputAction.CallbackContext context);
        void OnRotate(InputAction.CallbackContext context);
        void OnPosition(InputAction.CallbackContext context);
    }
}
