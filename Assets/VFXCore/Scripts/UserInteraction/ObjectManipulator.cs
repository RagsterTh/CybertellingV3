/*===============================================================================
Copyright (c) 2025 PTC Inc. and/or Its Subsidiary Companies. All Rights Reserved.

Confidential and Proprietary - Protected under copyright and other laws.
Vuforia is a trademark of PTC Inc., registered in the United States and other
countries.
===============================================================================*/

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

namespace VFX
{
    /// <summary>
    /// A component allowing user to manipulate an object
    /// i.e., translating/rotating the object,
    /// using screen-touch gestures.
    /// </summary>
    public class ObjectManipulator : MonoBehaviour
    {
        [Header("Motion Constraints")]
        public bool ConstrainX;
        public bool ConstrainY;
        public bool ConstrainZ;

        [Header("Game Object Layer")]
        public bool UsePickableLayer;
        public string PickableLayer;

        Vector2 mLastMousePos;
        
        void OnEnable()
        {
            // Enable EnhancedTouchSupport on touchscreen devices to access touch data at runtime
            if (Touchscreen.current != null)
                EnhancedTouchSupport.Enable();
            else
                mLastMousePos = Mouse.current?.position.value ?? Vector2.zero;
        }

        void OnDisable()
        {
            // Disable EnhancedTouchSupport to avoid consuming resources when not needed anymore
            if (Touchscreen.current != null)
                EnhancedTouchSupport.Disable();
        }

        void Update()
        {
            if (IsPointerOverUIObject())
                return;

            if (EnhancedTouchSupport.enabled)
                HandleTouchGestures();
            else
                HandleMouseEvents();
        }
        
        void HandleMouseEvents()
        {
            GameObject pickedObject;
            if (Mouse.current.leftButton.isPressed || Mouse.current.rightButton.isPressed)
                TryPickObject(Mouse.current.position.value, out pickedObject);
            else
                return;

            if (pickedObject)
            {
                // Left mouse button down
                if (Mouse.current.leftButton.isPressed)
                {
                    var dx = Mathf.Clamp((Mouse.current.position.value.x - mLastMousePos.x) / Screen.width, -0.5f, 0.5f);
                    var angle = -dx * 180;
                    RotateObject(pickedObject, angle);
                }

                // Right mouse button down
                if (Mouse.current.rightButton.isPressed)
                {
                    var delta = Mouse.current.position.value - mLastMousePos;
                    var worldSpaceMotion = ScreenToWorldMotion(pickedObject, delta);
                    DragObject(pickedObject, worldSpaceMotion);
                }
            }

            // Remember last mouse position
            mLastMousePos = new Vector2(Mouse.current.position.value.x, Mouse.current.position.value.y);
        }
        
        void HandleTouchGestures()
        {
            if (Touch.activeTouches.Count == 0)
                return;

            if (!TryPickObject(Touch.activeTouches[0].screenPosition, out var pickedObject))
                return;

            if (Touch.activeTouches.Count == 1)
            {
                var dx = Touch.activeTouches[0].delta.x / Screen.width;
                var angle = -dx * 180;
                RotateObject(pickedObject, angle);
            }
            else if (Touch.activeTouches.Count == 2)
            {
                var pan = GetTouchPanScreenDelta();
                var worldSpaceMotion = ScreenToWorldMotion(pickedObject, pan);
                DragObject(pickedObject, worldSpaceMotion);
            }
        }

        bool TryPickObject(Vector2 screenPosition, out GameObject pickedObject)
        {
            pickedObject = null;
            var cam = VuforiaCameraUtil.GetCamera();
            if (!cam)
                return false;

            var ray = cam.ScreenPointToRay(screenPosition);
            if (UsePickableLayer && Physics.Raycast(ray, out RaycastHit hit, cam.farClipPlane, layerMask: 
                LayerMask.NameToLayer(PickableLayer)))
            {
                pickedObject = hit.collider.gameObject;
                return true;
            }
            if (Physics.Raycast(ray, out hit, cam.farClipPlane))
            {
                pickedObject = hit.collider.gameObject;
                return true;
            }
            return false;
        }

        Vector2 GetTouchPanScreenDelta()
        {
            var touch0 = Touch.activeTouches[0];
            var touch1 = Touch.activeTouches[1];
            var touchMotion = 0.5f * (touch0.delta + touch1.delta);
            return touchMotion;
        }

        Vector3 ScreenToWorldMotion(GameObject pickedObject, Vector2 screenMotion)
        {
            var cam = VuforiaCameraUtil.GetCamera();
            if (!cam)
                return Vector3.zero;

            var dx = 2.0f * screenMotion.x / Screen.width;
            var dy = 2.0f * screenMotion.y / Screen.height;

            var viewPoint = cam.transform.position;
            var viewDir = cam.transform.forward;
            var objPos = pickedObject ? pickedObject.transform.position : viewPoint;
            var camToObj = objPos - viewPoint;
            var depth = Vector3.Dot(camToObj, viewDir);
            var vertFovRad = CameraUtil.GetVerticalFovRadians(cam);
            var horizFovRad = CameraUtil.GetHorizontalFovRadians(cam);
            var motionScaleX = depth * Mathf.Tan(0.5f * horizFovRad);
            var motionScaleY = depth * Mathf.Tan(0.5f * vertFovRad);

            var cameraSpaceMotion = new Vector3(motionScaleX * dx, motionScaleY * dy, 0);
            var worldSpaceMotion = cam.transform.TransformVector(cameraSpaceMotion);
            return worldSpaceMotion;
        }

        void DragObject(GameObject pickedObject, Vector3 worldSpaceMotion)
        {
            if (pickedObject == null)
                return;

            var worldDx = ConstrainX ? 0 : worldSpaceMotion.x;
            var worldDy = ConstrainY ? 0 : worldSpaceMotion.y;
            var worldDz = ConstrainZ ? 0 : worldSpaceMotion.z;
            pickedObject.transform.position += new Vector3(worldDx, worldDy, worldDz);
        }

        void RotateObject(GameObject pickedObject, float angle)
        {
            if (!pickedObject)
                return;

            pickedObject.transform.Rotate(Vector3.up, angle, Space.World);
        }

        static bool IsPointerOverUIObject()
        {
            if (Mouse.current?.leftButton.wasPressedThisFrame != true || Mouse.current?.rightButton.wasPressedThisFrame != true)
                return false;
            if (!EnhancedTouchSupport.enabled || Touch.activeTouches.Count == 0 || Touch.activeTouches[0].phase != TouchPhase.Began)
                return false;
            
            if (EventSystem.current == null)
                return false;

            var inputPosition = EnhancedTouchSupport.enabled
                               ? Touch.activeTouches[0].screenPosition
                               : Mouse.current.position.value;

            var eventDataCurrentPosition = new PointerEventData(EventSystem.current);
            eventDataCurrentPosition.position = inputPosition;
            var results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
            return results.Count > 0;
        }
    }
}
