using Fusion;


public abstract class TestInteractable : NetworkBehaviour
{
    public InteractableType interactableType;// YSb_Scripts, EnvironmentObject로 이동

    abstract public bool CanInteract();
    abstract public void Interact(PlayerRef playerRef, int att);//YSB_Scripts, IInteractable로 이동
}