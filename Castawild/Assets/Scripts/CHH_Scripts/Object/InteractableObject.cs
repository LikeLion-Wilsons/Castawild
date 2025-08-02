using Fusion;

[System.Serializable]
public enum InteractableType { None, Tree, Stone, Bed, Box, Campfire, WaterPurifier, Item }
public abstract class InteractableObject : NetworkBehaviour
{
    public InteractableType interactableType;
    public string text;
    public bool isPlaceable;

    // HasInputAuthority 에서만 실행
    abstract public bool CanInteract();
    abstract public void Interact(PlayerRef playerRef); 
}