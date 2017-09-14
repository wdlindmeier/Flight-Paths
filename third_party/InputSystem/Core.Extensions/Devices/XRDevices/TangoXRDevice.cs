using System;
using System.Collections.Generic;
using Assets.Utilities;
using UnityEngine;
using UnityEngine.Experimental.Input;

namespace UnityEngine.Experimental.Input
{
    public class TangoXRDevice : TrackedInputDevice
    {
        internal enum Node
        {
            Device = 4,
            IMU = 5,
            Display = 6,
            CameraColor = 7,
            CameraDepth = 8,
            CameraFisheye = 9,
        }

        public override void AddStandardControls(ControlSetup setup)
        {
            base.AddStandardControls(setup);

            setup.GetControl(CommonControls.Pose).name = "IMU Pose";

            devicePosition = (Vector3Control)setup.AddControl(SupportedControl.Get<Vector3Control>("Device Position"));
            deviceRotation = (QuaternionControl)setup.AddControl(SupportedControl.Get<QuaternionControl>("Device Rotation"));
            devicePose = (PoseControl)setup.AddControl(SupportedControl.Get<PoseControl>("Device Pose"));

            displayPosition = (Vector3Control)setup.AddControl(SupportedControl.Get<Vector3Control>("Display Position"));
            displayRotation = (QuaternionControl)setup.AddControl(SupportedControl.Get<QuaternionControl>("Display Rotation"));
            displayPose = (PoseControl)setup.AddControl(SupportedControl.Get<PoseControl>("Display Pose"));

            colorCameraPosition = (Vector3Control)setup.AddControl(SupportedControl.Get<Vector3Control>("Color Camera Position"));
            colorCameraRotation = (QuaternionControl)setup.AddControl(SupportedControl.Get<QuaternionControl>("Color Camera Rotation"));
            colorCameraPose = (PoseControl)setup.AddControl(SupportedControl.Get<PoseControl>("Color Camera Pose"));

            depthCameraPosition = (Vector3Control)setup.AddControl(SupportedControl.Get<Vector3Control>("Depth Camera Position"));
            depthCameraRotation = (QuaternionControl)setup.AddControl(SupportedControl.Get<QuaternionControl>("Depth Camera Rotation"));
            depthCameraPose = (PoseControl)setup.AddControl(SupportedControl.Get<PoseControl>("Depth Camera Pose"));

            fisheyeCameraPosition = (Vector3Control)setup.AddControl(SupportedControl.Get<Vector3Control>("Fisheye Camera Position"));
            fisheyeCameraRotation = (QuaternionControl)setup.AddControl(SupportedControl.Get<QuaternionControl>("Fisheye Camera Rotation"));
            fisheyeCameraPose = (PoseControl)setup.AddControl(SupportedControl.Get<PoseControl>("Fisheye Camera Pose"));
        }

        public override bool ProcessEventIntoState(InputEvent inputEvent, InputState intoState)
        {
            var consumed = false;

            var trackingEvent = inputEvent as TrackingEvent;
            if (trackingEvent != null)
            {
                switch (trackingEvent.nodeId)
                {
                    //case (int)Node.IMU:
                    // IMU is handled by the base class

                    case (int)Node.Device:
                        consumed |= SetPoseData(
                                intoState,
                                devicePosition.index,
                                deviceRotation.index,
                                devicePose.index,
                                trackingEvent.availableFields,
                                trackingEvent.localPosition,
                                trackingEvent.localRotation,
                                devicePosition.value,
                                deviceRotation.value);
                        break;
                    case (int)Node.Display:
                        consumed |= SetPoseData(
                                intoState,
                                displayPosition.index,
                                displayRotation.index,
                                displayPose.index,
                                trackingEvent.availableFields,
                                trackingEvent.localPosition,
                                trackingEvent.localRotation,
                                displayPosition.value,
                                displayRotation.value);
                        break;
                    case (int)Node.CameraColor:
                        consumed |= SetPoseData(
                                intoState,
                                colorCameraPosition.index,
                                colorCameraRotation.index,
                                colorCameraPose.index,
                                trackingEvent.availableFields,
                                trackingEvent.localPosition,
                                trackingEvent.localRotation,
                                colorCameraPosition.value,
                                colorCameraRotation.value);
                        break;
                    case (int)Node.CameraDepth:
                        consumed |= SetPoseData(
                                intoState,
                                depthCameraPosition.index,
                                depthCameraRotation.index,
                                depthCameraPose.index,
                                trackingEvent.availableFields,
                                trackingEvent.localPosition,
                                trackingEvent.localRotation,
                                depthCameraPosition.value,
                                depthCameraRotation.value);
                        break;
                    case (int)Node.CameraFisheye:
                        consumed |= SetPoseData(
                                intoState,
                                fisheyeCameraPosition.index,
                                fisheyeCameraRotation.index,
                                fisheyeCameraPose.index,
                                trackingEvent.availableFields,
                                trackingEvent.localPosition,
                                trackingEvent.localRotation,
                                fisheyeCameraPosition.value,
                                fisheyeCameraRotation.value);
                        break;
                }
            }

            if (!consumed)
            {
                consumed = base.ProcessEventIntoState(inputEvent, intoState);
            }

            return consumed;
        }

        // IMU is the base class in this case.

        public Vector3Control fisheyeCameraPosition { get; private set; }
        public QuaternionControl fisheyeCameraRotation { get; private set; }
        public PoseControl fisheyeCameraPose { get; private set; }

        public Vector3Control depthCameraPosition { get; private set; }
        public QuaternionControl depthCameraRotation { get; private set; }
        public PoseControl depthCameraPose { get; private set; }

        public Vector3Control colorCameraPosition { get; private set; }
        public QuaternionControl colorCameraRotation { get; private set; }
        public PoseControl colorCameraPose { get; private set; }

        public Vector3Control displayPosition { get; private set; }
        public QuaternionControl displayRotation { get; private set; }
        public PoseControl displayPose { get; private set; }

        public Vector3Control devicePosition { get; private set; }
        public QuaternionControl deviceRotation { get; private set; }
        public PoseControl devicePose { get; private set; }
    }
}
