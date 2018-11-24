using UnityEngine;
using XInputDotNetPure;
public class XInput : MonoBehaviour
{
	private static XInput _instance;
	private static XInput instance
	{
		get
		{
			if (_instance == null) _instance = new GameObject("XInput").AddComponent<XInput>();
			return _instance;
		}
	}
	private GamePadState[] previousState, currentState;
	private int activePlayers = 4;
	public enum Button
	{
		A = 1,
		B = 2,
		X = 4,
		Y = 8,
		Back = 16,
		Start = 32,
		LS = 64,
		RS = 128,
		LB = 256,
		RB = 512,
		DPadUp = 1024,
		DPadDown = 2048,
		DPadLeft = 4096,
		DPadRight = 8192
	}
	public enum Axis
	{
		LX = 1,
		LY =2,
		RX =4,
		RY = 8,
		LT = 16,
		RT = 32
	}

	private void Awake()
	{
		previousState = new GamePadState[4];
		currentState = new GamePadState[4];
		string[] joyStickNames = Input.GetJoystickNames();
		for (int i = 0; i < joyStickNames.Length; ++i)
		{
			Debug.Log("Joystick Connected - ID:" + i + " - " + joyStickNames[i]);
		}
	}

	public static void SetActivePlayerCount(int players)
	{
		instance.activePlayers = players;
	}

	public static bool GetButton(int player, Button button)
	{
		if (player > instance.currentState.Length - 1) return false;
		return GetButtonState(instance.currentState[player], button) == ButtonState.Pressed;
	}
	public static bool GetButtonDown(int player, Button button)
	{
		if (player > instance.currentState.Length - 1) return false;
		return (GetButtonState(instance.currentState[player], button) == ButtonState.Pressed)&& (GetButtonState(instance.previousState[player], button) == ButtonState.Released);
	}
	public static bool GetButtonUp(int player, Button button)
	{
		if (player > instance.currentState.Length - 1) return false;
		return (GetButtonState(instance.currentState[player], button) == ButtonState.Released) && (GetButtonState(instance.previousState[player], button) == ButtonState.Pressed);
	}
	public static float GetAxis(int player, Axis axis)
	{
		if (player > instance.currentState.Length - 1) return 0;
		return GetAxis(instance.currentState[player], axis);
	}
	public static void SetVibration(int player, Vector2 vibration)
	{
		GamePad.SetVibration((PlayerIndex)player, vibration.x, vibration.y);
	}
	private static float GetAxis(GamePadState state, Axis axis)
	{
		switch (axis)
		{
			case Axis.LT:
				return state.Triggers.Left;
			case Axis.RT:
				return state.Triggers.Right;
			case Axis.LX:
				return state.ThumbSticks.Left.X;
			case Axis.LY:
				return state.ThumbSticks.Left.Y;
			case Axis.RX:
				return state.ThumbSticks.Right.X;
			case Axis.RY:
				return state.ThumbSticks.Right.Y;
			default:
				return 0;
		}
	}
	private static ButtonState GetButtonState(GamePadState state, Button button)
	{
		switch (button)
		{
			case Button.A:
				return state.Buttons.A;
			case Button.B:
				return state.Buttons.B;
			case Button.X:
				return state.Buttons.X;
			case Button.Y:
				return state.Buttons.Y;
			case Button.LB:
				return state.Buttons.LeftShoulder;
			case Button.RB:
				return state.Buttons.RightShoulder;
			case Button.LS:
				return state.Buttons.LeftStick;
			case Button.RS:
				return state.Buttons.RightStick;
			case Button.Back:
				return state.Buttons.Back;
			case Button.Start:
				return state.Buttons.Start;
			case Button.DPadUp:
				return state.DPad.Up;
			case Button.DPadDown:
				return state.DPad.Down;
			case Button.DPadLeft:
				return state.DPad.Left;
			case Button.DPadRight:
				return state.DPad.Right;
			default:
				return default(ButtonState);
		}
	}

	void Update()
    {
		for (int i = 0; i < activePlayers; ++i)
		{
			previousState[i] = currentState[i];
			currentState[i] = GamePad.GetState((PlayerIndex)i,GamePadDeadZone.None);
		}
    }
	private void OnDestroy()
	{
		for (int i = 0; i < 4; ++i) SetVibration(i, Vector2.zero);
	}
}
