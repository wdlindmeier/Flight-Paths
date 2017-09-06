using UnityEngine.Events;

namespace UnityEngine.Experimental.Input
{
    abstract public class TrackedInputDevice : InputDevice
    {
        public override void AddStandardControls(ControlSetup setup)
        {
            position = (Vector3Control)setup.AddControl(CommonControls.Position3d);
            rotation = (QuaternionControl)setup.AddControl(CommonControls.Rotation3d);
            pose = (PoseControl)setup.AddControl(CommonControls.Pose);
        }

        public override bool ProcessEventIntoState(InputEvent inputEvent, InputState intoState)
        {
            var consumed = false;

            var trackingEvent = inputEvent as TrackingEvent;
            if (trackingEvent != null)
            {
                consumed |= SetPoseData(intoState,
                        position.index,
                        rotation.index,
                        pose.index,
                        trackingEvent.availableFields,
                        trackingEvent.localPosition,
                        trackingEvent.localRotation,
                        position.value,
                        rotation.value);
            }

            if (!consumed && inputEvent != null)
                consumed = base.ProcessEventIntoState(inputEvent, intoState);

            return consumed;
        }

        protected bool SetPoseData(
            InputState intoState,
            int positionIndex,
            int rotationIndex,
            int poseIndex,
            TrackingEvent.Flags availableFlags,
            Vector3 localPos,
            Quaternion localRot,
            Vector3 currPos,
            Quaternion currRot)
        {
            bool consumed = false;
            bool positionTracked = (availableFlags & TrackingEvent.Flags.PositionAvailable) != 0;
            bool rotationTracked = (availableFlags & TrackingEvent.Flags.OrientationAvailable) != 0;

            // determine the pose information
            Quaternion rot = localRot;
            Vector3 pos = localPos;

            if (!rotationTracked)
                rot = rotation.value; // reset back to the base value.

            if (!positionTracked)
                pos = position.value;

            Pose pose = new Pose()
            {
                rotation = rot,
                translation = pos
            };

            if (positionTracked)
                consumed |= intoState.SetValueFromEvent(positionIndex, localPos);

            if (rotationTracked)
                consumed |= intoState.SetValueFromEvent(rotationIndex, localRot);

            consumed |= intoState.SetValueFromEvent(poseIndex, pose);

            return consumed;
        }

        public Vector3Control position { get; private set; }
        public QuaternionControl rotation { get; private set; }

        public PoseControl pose { get; private set; }
    }
}
