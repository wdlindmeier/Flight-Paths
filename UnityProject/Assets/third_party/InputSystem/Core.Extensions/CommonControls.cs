namespace UnityEngine.Experimental.Input
{
    // All controls that make sense to generalize across multiple devices should preferably be put in this class.
    // When different devices use controls with the same standard name and control type,
    // the chance of out-of-the box compatibility between them is increased.
    // Remember to look for existing applicable controls before adding new ones.
    //
    // Note that devices can use a common control from here but override the standard name with a custom one.
    // For example the Xbox device profiles rename controls Action 1, 2, 3, 4 to A, B, X, Y.
    // This does not break the compatibility since the SupportedControl hash is based on the standard name,
    // not the overridden one.
    //
    // The names passed as string parameters should be human-readable.
    // Capitalize every word and separate words by space ("Right Stick") or dash ("D-Pad") as appropriate.
    // Never use camelCase or PascalCase.
    //
    // The property names should be consistent with the human readable string names, but use PascalCase.
    // Furthermore the suffixes below should be used for the property names to reduce risk of future collisions
    // between the property names of different common controls:
    //  - Vector2Controls should have a 2d suffix.
    //  - Vector3Controls should have a 3d suffix.
    public static class CommonControls
    {
        public static readonly SupportedControl LeftStickLeft = SupportedControl.Get<ButtonControl>("Left Stick Left");
        public static readonly SupportedControl LeftStickRight = SupportedControl.Get<ButtonControl>("Left Stick Right");
        public static readonly SupportedControl LeftStickDown = SupportedControl.Get<ButtonControl>("Left Stick Down");
        public static readonly SupportedControl LeftStickUp = SupportedControl.Get<ButtonControl>("Left Stick Up");
        public static readonly SupportedControl LeftStickX = SupportedControl.Get<AxisControl>("Left Stick X");
        public static readonly SupportedControl LeftStickY = SupportedControl.Get<AxisControl>("Left Stick Y");
        public static readonly SupportedControl LeftStick = SupportedControl.Get<Vector2Control>("Left Stick");
        public static readonly SupportedControl LeftStickButton = SupportedControl.Get<ButtonControl>("Left Stick Button");
        public static readonly SupportedControl LeftStickTouch = SupportedControl.Get<ButtonControl>("Left Stick Touch");

        public static readonly SupportedControl RightStickLeft = SupportedControl.Get<ButtonControl>("Right Stick Left");
        public static readonly SupportedControl RightStickRight = SupportedControl.Get<ButtonControl>("Right Stick Right");
        public static readonly SupportedControl RightStickDown = SupportedControl.Get<ButtonControl>("Right Stick Down");
        public static readonly SupportedControl RightStickUp = SupportedControl.Get<ButtonControl>("Right Stick Up");
        public static readonly SupportedControl RightStickX = SupportedControl.Get<AxisControl>("Right Stick X");
        public static readonly SupportedControl RightStickY = SupportedControl.Get<AxisControl>("Right Stick Y");
        public static readonly SupportedControl RightStick = SupportedControl.Get<Vector2Control>("Right Stick");
        public static readonly SupportedControl RightStickButton = SupportedControl.Get<ButtonControl>("Right Stick Button");
        public static readonly SupportedControl RightStickTouch = SupportedControl.Get<ButtonControl>("Right Stick Touch");

        public static readonly SupportedControl DPadLeft = SupportedControl.Get<ButtonControl>("D-Pad Left");
        public static readonly SupportedControl DPadRight = SupportedControl.Get<ButtonControl>("D-Pad Right");
        public static readonly SupportedControl DPadDown = SupportedControl.Get<ButtonControl>("D-Pad Down");
        public static readonly SupportedControl DPadUp = SupportedControl.Get<ButtonControl>("D-Pad Up");
        public static readonly SupportedControl DPadX = SupportedControl.Get<AxisControl>("D-Pad X");
        public static readonly SupportedControl DPadY = SupportedControl.Get<AxisControl>("D-Pad Y");
        public static readonly SupportedControl DPad = SupportedControl.Get<Vector2Control>("D-Pad");

        // These are for the primary "normal" buttons on a device.
        // For an Xbox controllers Action 1, 2, 3, 4 correspond to A, B, X, Y
        // For a PlayStation controller, Action 1, 2, 3, 4 correspond to Cross, Circle, Square, Triangle.
        // For a mouse, Action 1, 2, 3 correspond to primary(left), secondary(right), tertiary(middle) buttons.
        public static readonly SupportedControl Action1 = SupportedControl.Get<ButtonControl>("Action 1");
        public static readonly SupportedControl Action2 = SupportedControl.Get<ButtonControl>("Action 2");
        public static readonly SupportedControl Action3 = SupportedControl.Get<ButtonControl>("Action 3");
        public static readonly SupportedControl Action4 = SupportedControl.Get<ButtonControl>("Action 4");

        public static readonly SupportedControl Trigger = SupportedControl.Get<ButtonControl>("Trigger");
        public static readonly SupportedControl LeftTrigger = SupportedControl.Get<ButtonControl>("Left Trigger");
        public static readonly SupportedControl RightTrigger = SupportedControl.Get<ButtonControl>("Right Trigger");
        public static readonly SupportedControl TriggerTouch = SupportedControl.Get<ButtonControl>("Trigger Touch");
        public static readonly SupportedControl Squeeze = SupportedControl.Get<ButtonControl>("Squeeze");

        public static readonly SupportedControl LeftBumper = SupportedControl.Get<ButtonControl>("Left Bumper");
        public static readonly SupportedControl RightBumper = SupportedControl.Get<ButtonControl>("Right Bumper");

        public static readonly SupportedControl Start = SupportedControl.Get<ButtonControl>("Start");
        public static readonly SupportedControl Back = SupportedControl.Get<ButtonControl>("Back");
        public static readonly SupportedControl Select = SupportedControl.Get<ButtonControl>("Select");
        public static readonly SupportedControl System = SupportedControl.Get<ButtonControl>("System");
        public static readonly SupportedControl Options = SupportedControl.Get<ButtonControl>("Options");
        public static readonly SupportedControl Pause = SupportedControl.Get<ButtonControl>("Pause");
        public static readonly SupportedControl Menu = SupportedControl.Get<ButtonControl>("Menu");
        public static readonly SupportedControl Share = SupportedControl.Get<ButtonControl>("Share");
        public static readonly SupportedControl Home = SupportedControl.Get<ButtonControl>("Home");
        public static readonly SupportedControl View = SupportedControl.Get<ButtonControl>("View");
        public static readonly SupportedControl Power = SupportedControl.Get<ButtonControl>("Power");

        public static readonly SupportedControl Tilt2d = SupportedControl.Get<Vector2Control>("Tilt");
        public static readonly SupportedControl TiltX = SupportedControl.Get<AxisControl>("Tilt X");
        public static readonly SupportedControl TiltY = SupportedControl.Get<AxisControl>("Tilt Y");

        public static readonly SupportedControl TouchpadX = SupportedControl.Get<AxisControl>("Touchpad X");
        public static readonly SupportedControl TouchpadY = SupportedControl.Get<AxisControl>("Touchpad Y");
        public static readonly SupportedControl TouchpadTap = SupportedControl.Get<ButtonControl>("Touchpad Tap");

        public static readonly SupportedControl Position3d = SupportedControl.Get<Vector3Control>("Position");
        public static readonly SupportedControl PositionX = SupportedControl.Get<AxisControl>("Position X");
        public static readonly SupportedControl PositionY = SupportedControl.Get<AxisControl>("Position Y");
        public static readonly SupportedControl PositionZ = SupportedControl.Get<AxisControl>("Position Z");

        public static readonly SupportedControl Rotation3d = SupportedControl.Get<QuaternionControl>("Rotation");

        public static readonly SupportedControl Pose = SupportedControl.Get<PoseControl>("Pose");

        public static readonly SupportedControl Velocity3d = SupportedControl.Get<Vector3Control>("Velocity");
        public static readonly SupportedControl AngularVelocity3d = SupportedControl.Get<Vector3Control>("Angular Velocity");
        public static readonly SupportedControl Acceleration3d = SupportedControl.Get<Vector3Control>("Acceleration");
        public static readonly SupportedControl AngularAcceleration3d = SupportedControl.Get<Vector3Control>("Angular Acceleration");

        public static readonly SupportedControl Delta3d = SupportedControl.Get<Vector3Control>("Delta");
        public static readonly SupportedControl DeltaX = SupportedControl.Get<DeltaAxisControl>("Delta X");
        public static readonly SupportedControl DeltaY = SupportedControl.Get<DeltaAxisControl>("Delta Y");
        public static readonly SupportedControl DeltaZ = SupportedControl.Get<DeltaAxisControl>("Delta Z");

        public static readonly SupportedControl ScrollWheel2d = SupportedControl.Get<Vector2Control>("Scroll Wheel");
        public static readonly SupportedControl ScrollWheelX = SupportedControl.Get<DeltaAxisControl>("Scroll Wheel X");
        public static readonly SupportedControl ScrollWheelY = SupportedControl.Get<DeltaAxisControl>("Scroll Wheel Y");

        public static readonly SupportedControl Pressure = SupportedControl.Get<AxisControl>("Pressure");
        public static readonly SupportedControl Twist = SupportedControl.Get<AxisControl>("Twist");

        public static readonly SupportedControl Radius3d = SupportedControl.Get<Vector3Control>("Radius");
        public static readonly SupportedControl RadiusX = SupportedControl.Get<AxisControl>("Radius X");
        public static readonly SupportedControl RadiusY = SupportedControl.Get<AxisControl>("Radius Y");
        public static readonly SupportedControl RadiusZ = SupportedControl.Get<AxisControl>("Radius Z");

        public static readonly SupportedControl DoubleClick = SupportedControl.Get<EventControl>("Double Click");

        public static readonly SupportedControl Vibration = SupportedControl.Get<AxisOutput>("Vibration");
    }
}
