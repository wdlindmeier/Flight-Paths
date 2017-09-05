using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UnityEngine.Experimental.Input
{
    public class VirtualStick : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
    {
        public enum AxisOption
        {
            // Options for which axes to use
            Both, // Use both
            OnlyHorizontal, // Only horizontal
            OnlyVertical // Only vertical
        }

        public enum StickOption
        {
            LeftStick,
            RightStick
        }

        public int m_MovementRange = 100;
        public AxisOption m_AxesToUse = AxisOption.Both; // The options for the axes that the still will use
        public StickOption m_StickChoice = StickOption.LeftStick;

        Vector3 m_StartPos;
        Vector2 m_PointerDownPos;
        bool m_UseX; // Toggle for using the x axis
        bool m_UseY; // Toggle for using the Y axis
        Camera m_EventCamera;

        void OnEnable()
        {
            CreateVirtualAxes();
        }

        void Start()
        {
            m_StartPos = (transform as RectTransform).anchoredPosition;
            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas)
                m_EventCamera = canvas.worldCamera;
        }

        void UpdateVirtualAxes(Vector3 delta)
        {
            Gamepad gamepad = VirtualDeviceManager.GetDevice<Gamepad>();

            if (m_UseX)
            {
                ButtonControl control;
                control = (m_StickChoice == StickOption.LeftStick ? gamepad.leftStickLeft : gamepad.rightStickLeft);
                VirtualDeviceManager.SendValueToControl(control, Mathf.Max(0, -delta.x));
                control = (m_StickChoice == StickOption.LeftStick ? gamepad.leftStickRight : gamepad.rightStickRight);
                VirtualDeviceManager.SendValueToControl(control, Mathf.Max(0, delta.x));
            }

            if (m_UseY)
            {
                ButtonControl control;
                control = (m_StickChoice == StickOption.LeftStick ? gamepad.leftStickDown : gamepad.rightStickDown);
                VirtualDeviceManager.SendValueToControl(control, Mathf.Max(0, -delta.y));
                control = (m_StickChoice == StickOption.LeftStick ? gamepad.leftStickUp : gamepad.rightStickUp);
                VirtualDeviceManager.SendValueToControl(control, Mathf.Max(0, delta.y));
            }
        }

        void CreateVirtualAxes()
        {
            // Set axes to use
            m_UseX = (m_AxesToUse == AxisOption.Both || m_AxesToUse == AxisOption.OnlyHorizontal);
            m_UseY = (m_AxesToUse == AxisOption.Both || m_AxesToUse == AxisOption.OnlyVertical);
        }

        public void OnPointerDown(PointerEventData data)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(transform.parent as RectTransform, data.position, m_EventCamera, out m_PointerDownPos);
        }

        public void OnDrag(PointerEventData data)
        {
            Vector2 position;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(transform.parent as RectTransform, data.position, m_EventCamera, out position);
            Vector2 delta = position - m_PointerDownPos;

            if (!m_UseX)
                delta.x = 0;

            if (!m_UseY)
                delta.y = 0;

            delta = Vector2.ClampMagnitude(delta, m_MovementRange);

            (transform as RectTransform).anchoredPosition = m_StartPos + (Vector3)delta;
            UpdateVirtualAxes(delta / m_MovementRange);
        }

        public void OnPointerUp(PointerEventData data)
        {
            (transform as RectTransform).anchoredPosition = m_StartPos;
            UpdateVirtualAxes(Vector2.zero);
        }
    }
}
