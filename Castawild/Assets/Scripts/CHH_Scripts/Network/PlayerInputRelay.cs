using Fusion;

public class PlayerInputRelay : NetworkBehaviour
{
    private PlayerInputManager inputManager;

    public override void Spawned()
    {
        if (!HasInputAuthority)
            return;

        inputManager = GetComponent<PlayerInputManager>();
        Runner.GetComponent<NetworkEvents>().OnInput.AddListener(OnInput);
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        runner?.GetComponent<NetworkEvents>()?.OnInput.RemoveListener(OnInput);
    }

    private void OnInput(NetworkRunner runner, NetworkInput input)
    {
        if (inputManager == null)
            return;

        input.Set(inputManager.CollectInput());
    }
}
