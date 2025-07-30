using Fusion;


public abstract class TestInteractable : NetworkBehaviour
{
    public InteractableType interactableType;

    abstract public bool CanInteract();
    abstract public void Interact(PlayerRef playerRef, int att);
}