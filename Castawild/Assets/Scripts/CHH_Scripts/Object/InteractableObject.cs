using Fusion;

[System.Serializable]
public enum InteractableType { None, Tree, Stone, Bed, Box, Campfire, WaterPurifier, Item, Gatherable }
public abstract class InteractableObject : NetworkBehaviour
{
    public InteractableType interactableType;
    public int itemIndex;
    public string text;
    public bool isPlaceable;

    // HasInputAuthority 에서만 실행
    abstract public bool CanInteract();
    abstract public void Interact(PlayerRef playerRef);
}