using Fusion;

namespace Test {
  public struct NetworkInputData2 : INetworkInput {
    public const int BUTTON_FORWARD = 1;
    public const int BUTTON_BACKWARD = 2;
    public const int BUTTON_LEFT = 3;
    public const int BUTTON_RIGHT = 4;
    public const int BUTTON_INTERACT = 5;

    public NetworkButtons Buttons;

    public bool IsUp(int button) {
      return Buttons.IsSet(button) == false;
    }

    public bool IsDown(int button) {
      return Buttons.IsSet(button);
    }

    public bool WasPressed(NetworkButtons prev, int button) {
      return Buttons.WasPressed(prev, button);
    }
  }
}