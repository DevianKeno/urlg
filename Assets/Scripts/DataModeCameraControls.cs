using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;
using UnityEngine.InputSystem;
using Cinemachine;
using System;
using UnityEngine.UI;

namespace RL
{
    public class DataModeCameraControls : MonoBehaviour
    {
        public int ZoomValue = 6;
        public int PreciseZoomValue = 6;
        public float PanSensitivity = 1f;
        public float PanSmoothing = 0.1f;
        public float PanArrowSpeed = 1f;

        [Header("Camera Settings")]
        public int MinPPU = 1;
        public int MaxPPU = 300;
        
        bool _isHoldingShift;
        bool _isHoldingPan;
        bool _isHoldingPanArrow;

        Vector3 _currentPanVelocity;
        Vector3 _targetPanPosition;

        [SerializeField] CinemachineVirtualCamera virtualCamera;
        [SerializeField] Slider zoomSlider;
        
        Camera mainCamera;
        PixelPerfectCamera ppc;
        InputAction zoomInput;
        InputAction shiftInput;
        InputAction panInput;
        InputAction panArrowInput;
        InputAction lookInput;

        void Awake()
        {
            mainCamera = Camera.main;
            ppc = mainCamera.GetComponent<PixelPerfectCamera>();
            zoomSlider.onValueChanged.AddListener(UpdateZoomLevel);
        }

        void Start()
        {
            InitializeInputs();
        }

        void InitializeInputs()
        {
            var map = Game.Main.PlayerInput.actions.FindActionMap("Test Mode Camera");

            zoomInput = map.FindAction("Zoom");
            panInput = map.FindAction("Pan");
            panArrowInput = map.FindAction("Pan Arrow");
            shiftInput = map.FindAction("Shift");
            lookInput = map.FindAction("Look");

            zoomInput.performed += OnInputZoom;
            shiftInput.started += OnInputShift;
            shiftInput.canceled += OnInputShift;
            panInput.started += OnInputPan;
            panInput.canceled += OnInputPan;
            panArrowInput.started += OnInputPanArrow;
            panArrowInput.canceled += OnInputPanArrow;

            zoomInput.Enable();
            shiftInput.Enable();
            panInput.Enable();
            panArrowInput.Enable();
            lookInput.Enable();
        }

        void OnInputShift(InputAction.CallbackContext context)
        {
            _isHoldingShift = context.ReadValueAsButton();
        }

        void OnInputZoom(InputAction.CallbackContext context)
        {
            var value = context.ReadValue<float>();

            if (value > 0)
            {
                ppc.assetsPPU += _isHoldingShift ? PreciseZoomValue : ZoomValue;
            }
            else if (value < 0)
            {
                ppc.assetsPPU -= _isHoldingShift ? PreciseZoomValue : ZoomValue;
            }
            UpdateSlider();
        }

        void OnInputPan(InputAction.CallbackContext context)
        {
            _isHoldingPan = context.started;
        }

        void OnInputPanArrow(InputAction.CallbackContext context)
        {
            _isHoldingPanArrow = context.started;
        }

        void Update()
        {
            HandlePanning();
            HandlePanArrow();
        }

        void HandlePanning()
        {
            if (_isHoldingPan)
            {
                _targetPanPosition = virtualCamera.transform.position;
                var look = lookInput.ReadValue<Vector2>();
                float panX = look.x * Time.deltaTime;
                float panY = look.y * Time.deltaTime;
                
                var target = _targetPanPosition -= new Vector3(panX, panY, 0f).normalized;
                target.z = -10f;
                _targetPanPosition = target * (PanSensitivity * (_isHoldingShift ? 0.1f : 1f));
                
                virtualCamera.transform.position = Vector3.SmoothDamp(
                    virtualCamera.transform.position,
                    _targetPanPosition,
                    ref _currentPanVelocity,
                    PanSmoothing
                );
            }
        }

        void HandlePanArrow()
        {
            if (_isHoldingPanArrow)
            {
                var input = panArrowInput.ReadValue<Vector2>();
                input *= Time.deltaTime;
                var inputVec3 = new Vector3(input.x, input.y).normalized * PanArrowSpeed;
                inputVec3 *= _isHoldingShift ? 0.2f : 1f;
                virtualCamera.transform.position += inputVec3;
            }
        }

        public void UpdateZoomLevel(float value)
        {
            ppc.assetsPPU = (int) Mathf.Lerp(MinPPU, MaxPPU, value);
        }

        public void UpdateSlider()
        {
            zoomSlider.value = (float) ppc.assetsPPU / MaxPPU;
        }
    }
}