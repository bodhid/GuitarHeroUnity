[System.Serializable]
public class PlayerInput
{
	public int deviceId;
	public Device device;
	public bool[] fred;
	public bool strumPressed;
	public bool startPressed;
	public float tilt, whammy;

	public enum Device
	{
		Keyboard,
		Xinput
	}
	public PlayerInput(Device _device, int _deviceId)
	{
		device = _device;
		deviceId = _deviceId;
		fred = new bool[5];
	}
	public void Update()
	{
		fred[0] = XInput.GetButton(deviceId, XInput.Button.A);
		fred[1] = XInput.GetButton(deviceId, XInput.Button.B);
		fred[2] = XInput.GetButton(deviceId, XInput.Button.Y);
		fred[3] = XInput.GetButton(deviceId, XInput.Button.X);
		fred[4] = XInput.GetButton(deviceId, XInput.Button.LB);

		startPressed = XInput.GetButtonDown(deviceId, XInput.Button.Start);
		strumPressed = XInput.GetButtonDown(deviceId, XInput.Button.DPadDown) | XInput.GetButtonDown(deviceId, XInput.Button.DPadUp);
		tilt = XInput.GetAxis(deviceId, XInput.Axis.RT);
		whammy = XInput.GetAxis(deviceId, XInput.Axis.LT);
	}
}

