// =========================================
// AUTHOR: Radek Mikulka
// DATE:   09.07.2025
// =========================================

using System;
using System.Threading;
using System.Threading.Tasks;
using AldaEngine;
using AldaEngine.AldaFramework;
using Cysharp.Threading.Tasks;
using KBCore.Refs;
using ServerData;
using UnityEngine;
using Pharaoh;
using Zenject;

namespace Pharaoh
{
    [DefaultExecutionOrder(CScriptExecutionOrder.CameraMover)]
    public class CCameraMover : ValidatedMonoBehaviour, IInitializable, IConstructable, IDestroyable
    {
        [SerializeField, Child] private CCameraZoom _cameraZoom;
        
        private ICameraPlaneProvider _cameraPlaneProvider;
        private readonly CCameraMoverInput _input = new();
        private IMainCameraProvider _mainCamera;
        private CCameraBorders _cameraBorders;
        private CEventSystem _eventSystem;
        private Transform _followTarget;
        private IEventBus _eventBus;

        private Vector3 _outBoundDelta;
        private Vector3 _dragStartPos;
        private Vector3 _dragDelta;
        private Vector3 _targetPos;
        private bool _keyPressed;
        
        [Inject]
        private void Inject(
            ICameraPlaneProvider cameraPlaneProvider, 
            IMainCameraProvider mainCamera, 
            CCameraBorders cameraBorders, 
            CEventSystem eventSystem,
            IEventBus eventBus
            )
        {
            _cameraPlaneProvider = cameraPlaneProvider;
            _cameraBorders = cameraBorders;
            _eventSystem = eventSystem;
            _mainCamera = mainCamera;
            _eventBus = eventBus;
        }

        public void Construct()
        {
            _targetPos = transform.position;
            enabled = false;
            
            _input.DragStarted += OnDragStart;
            _input.DragUpdated += OnDragUpdate;
            _input.DragEnded += OnDragEnd;
        }

        public void OnContextDestroy(bool appExits)
        {
            _input.DragStarted -= OnDragStart;
            _input.DragUpdated -= OnDragUpdate;
            _input.DragEnded -= OnDragEnd;
        }

        public void Initialize()
        {
            _eventBus.Subscribe<CCoreGameLoadedSignal>(OnCoreGameLoaded);
            _eventBus.Subscribe<CPinchEndSignal>(OnPinchEnd);
        }

        private void OnCoreGameLoaded(CCoreGameLoadedSignal signal)
        {
            enabled = true;
        }

        public void Warp(Vector3 position)
        {
            Vector3 correctPos = GetPointOnCameraPlane(position);
            
            transform.position = correctPos;
            _targetPos = correctPos;
        }

        private void MoveTo(Vector3 position)
        {
            Vector3 correctPos = GetPointOnCameraPlane(position);
            _targetPos = correctPos;
        }

        private Vector3 GetPointOnCameraPlane(Vector3 position)
        {
            Plane plane = _cameraPlaneProvider.GetCameraPlane();
            Vector3 camDir = _mainCamera.Camera.Camera.transform.forward;
            Ray ray = new(position, camDir);
            if (plane.Raycast(ray, out float enter))
            {
                Vector3 hitPoint = ray.GetPoint(enter);
                return hitPoint;
            }
            Vector3 result = plane.ClosestPointOnPlane(position);
            return result;
        }

        private void Update()
        {
            bool inputEnabled = !_eventSystem.IsInputLocked();
            if(!inputEnabled)
                return;
            
            _input.Update();
        }

        private void LateUpdate()
        {
            UpdateTargetPos();
            SmoothMove();
        }

        private Vector3 GetTargetPos()
        {
            if (_followTarget)
                return _followTarget.position;
            return _targetPos;
        }

        private void UpdateTargetPos()
        {
            Bounds bounds = _cameraBorders.Bounds;

            Vector3 targetPos = GetTargetPos();
            bool isInBounds = bounds.Contains(targetPos);
            if (!isInBounds)
            {
                _outBoundDelta += _dragDelta;
                
                Vector3 closestPoint = _cameraBorders.Bounds.ClosestPoint(targetPos);
                closestPoint.y = targetPos.y;
                Vector3 direction = closestPoint - targetPos;

                float rubberDelta = RubberDelta(_outBoundDelta.magnitude, 30);
                _targetPos = closestPoint - direction.normalized * rubberDelta;
            }
            else
            {
                _outBoundDelta = Vector3.zero;
            }
            
            _dragDelta = Vector3.zero;
        }

        private void SmoothMove()
        {
            Vector3 targetPos = GetTargetPos();
            float speed = _keyPressed ? 8f : 3f;
            transform.position = Vector3.Lerp(
                transform.position, 
                targetPos, 
                Time.deltaTime * speed
                );
        }

        private void OnDragStart(Vector2 screenCoords)
        {
            Vector3 intersectionPoint = GetCameraPlaneIntersectionPoint(screenCoords);
            _dragStartPos = intersectionPoint;
            
            bool inputLocked = IsInputLocked();
            if(inputLocked)
                return;
            
            _keyPressed = true;
            _targetPos = transform.position;
        }
        
        private void OnPinchEnd(CPinchEndSignal signal)
        {
            bool inputLocked = IsInputLocked();
            if(inputLocked)
                return;
            
            Vector3 intersectionPoint = GetCameraPlaneIntersectionPoint(signal.RemainingTouchScreenPosition);
            
            _targetPos = transform.position;
            _dragStartPos = intersectionPoint;
        }
        
        private void OnDragUpdate(Vector2 screenCoords)
        {
            bool inputLocked = IsInputLocked();
            if(inputLocked)
                return;

            Vector3 intersectionPoint = GetCameraPlaneIntersectionPoint(screenCoords);
            
            _dragDelta = _dragStartPos - intersectionPoint;

            float targetDeltaTime = 1f / 60f;
            float minDelta = 1f / 25f;
            float deltaMod = CMath.InverseLerp(minDelta, targetDeltaTime, Time.deltaTime);
            deltaMod /= 1f + Time.timeScale;
            
            Vector3 rootPos = transform.position;
            _targetPos = Vector3.Lerp(rootPos + _dragDelta, rootPos + _dragDelta * 10f, deltaMod);
        }
        
        private Vector3 GetCameraPlaneIntersectionPoint(Vector2 screenCoords)
        {
            Ray ray = _mainCamera.Camera.ScreenPointToRay(screenCoords);
            Plane plane = _cameraPlaneProvider.GetCameraPlane();
            plane.Raycast(ray, out float distance);
            return ray.GetPoint(distance);
        }

        private void OnDragEnd()
        {
            _keyPressed = false;
            _outBoundDelta = Vector3.zero;
        }
        
        private bool IsInputLocked()
        {
            bool anyScreenOpen = _eventBus.ProcessTask<CIsAnyScreenActiveRequest, CIsAnyScreenActiveResponse>().IsActive;
            return _eventSystem.IsInputLocked() || anyScreenOpen;
        }

        private static float RubberDelta(float overStretching, float viewSize)
        {
            return (1 - 1 / (CMath.Abs(overStretching) * 0.55f / viewSize + 1)) * viewSize * CMath.Sign(overStretching);
        }
    }
}