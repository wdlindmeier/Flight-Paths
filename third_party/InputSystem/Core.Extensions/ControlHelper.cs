using Assets.Utilities;

namespace UnityEngine.Experimental.Input
{
    public static class ControlHelper
    {
        public static void BuildXYCircularInputBasedOnAxisInput(
            ButtonControl controlLeft,
            ButtonControl controlRight,
            ButtonControl controlDown,
            ButtonControl controlUp,
            AxisControl controlX,
            AxisControl controlY,
            Vector2Control control,
            Range deadZones
            )
        {
            // Calculate new vector.
            Vector2 vector = new Vector2(
                    -controlLeft.rawValue + controlRight.rawValue,
                    -controlDown.rawValue + controlUp.rawValue);

            // Set raw control values.
            control.rawValue = vector;
            controlX.rawValue = vector.x;
            controlY.rawValue = vector.y;

            // Apply deadzones.
            float magnitude = vector.magnitude;
            float newMagnitude = GetDeadZoneAdjustedValue(magnitude, deadZones);
            if (newMagnitude == 0)
                vector = Vector2.zero;
            else
                vector *= (newMagnitude / magnitude);

            // Set control values.
            control.value = vector;
            controlX.value = vector.x;
            controlY.value = vector.y;
            controlLeft.value = Mathf.Max(0, -vector.x);
            controlRight.value = Mathf.Max(0, vector.x);
            controlDown.value = Mathf.Max(0, -vector.y);
            controlUp.value = Mathf.Max(0, vector.y);
        }

        public static void BuildAxisInputCircularBasedOnXY(
            ButtonControl controlLeft,
            ButtonControl controlRight,
            ButtonControl controlDown,
            ButtonControl controlUp,
            AxisControl controlX,
            AxisControl controlY,
            Vector2Control control,
            Range deadZones
            )
        {
            // Calculate new vector.
            Vector2 vector = new Vector2(controlX.rawValue, controlY.rawValue);

            // Set raw control values.
            control.rawValue = vector;
            controlLeft.rawValue = Mathf.Max(0, -vector.x);
            controlRight.rawValue = Mathf.Max(0, vector.x);
            controlDown.rawValue = Mathf.Max(0, -vector.y);
            controlUp.rawValue = Mathf.Max(0, vector.y);

            // Apply deadzones.
            float magnitude = vector.magnitude;
            float newMagnitude = GetDeadZoneAdjustedValue(magnitude, deadZones);
            if (newMagnitude == 0)
                vector = Vector2.zero;
            else
                vector *= (newMagnitude / magnitude);

            // Set control values.
            control.value = vector;
            controlX.value = vector.x;
            controlY.value = vector.y;
            controlLeft.value = Mathf.Max(0, -vector.x);
            controlRight.value = Mathf.Max(0, vector.x);
            controlDown.value = Mathf.Max(0, -vector.y);
            controlUp.value = Mathf.Max(0, vector.y);
        }

        public static void BuildXYSquareInputBasedOnAxisInput(
            ButtonControl controlLeft,
            ButtonControl controlRight,
            ButtonControl controlDown,
            ButtonControl controlUp,
            AxisControl controlX,
            AxisControl controlY,
            Vector2Control control,
            Range deadZones
            )
        {
            // Calculate new vector.
            Vector2 vector = new Vector2(
                    -controlLeft.rawValue + controlRight.rawValue,
                    -controlDown.rawValue + controlUp.rawValue);

            // Set raw control values.
            control.rawValue = vector;
            controlX.rawValue = vector.x;
            controlY.rawValue = vector.y;

            // Apply deadzones.
            vector.x = GetDeadZoneAdjustedValue(vector.x, deadZones);
            vector.y = GetDeadZoneAdjustedValue(vector.y, deadZones);

            // Set control values.
            control.value = vector;
            controlX.value = vector.x;
            controlY.value = vector.y;
            controlLeft.value = Mathf.Max(0, -vector.x);
            controlRight.value = Mathf.Max(0, vector.x);
            controlDown.value = Mathf.Max(0, -vector.y);
            controlUp.value = Mathf.Max(0, vector.y);
        }

        public static float GetDeadZoneAdjustedValue(float value, Range deadZones)
        {
            float absValue = Mathf.Abs(value);
            if (absValue < deadZones.min)
                return 0;
            if (absValue > deadZones.max)
                return Mathf.Sign(value);
            return Mathf.Sign(value) * ((absValue - deadZones.min) / (deadZones.max - deadZones.min));
        }

        public static void SetAxisControlsEnabledBasedOnXY(
            InputControl controlLeft,
            InputControl controlRight,
            InputControl controlDown,
            InputControl controlUp,
            InputControl controlX,
            InputControl controlY,
            InputControl control)
        {
            if (control.enabled)
            {
                controlX.enabled = true;
                controlY.enabled = true;
            }
            if (controlX.enabled)
            {
                controlLeft.enabled = true;
                controlRight.enabled = true;
            }
            if (controlY.enabled)
            {
                controlDown.enabled = true;
                controlUp.enabled = true;
            }
        }
    }
}
