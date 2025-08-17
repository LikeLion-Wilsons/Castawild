using Fusion;

[System.Serializable]
public enum InteractableType { None, Tree, Stone, Bed, Box, Campfire, WaterPurifier, Item, Gatherable }
public abstract class InteractableObject : NetworkBehaviour
{
    public InteractableType interactableType;
    [Networked] public string text { get; set; }
    public bool isPlaceable;
    public int itemIndex;

    // HasInputAuthority 에서만 실행
    abstract public bool CanInteract();
    abstract public void Interact(PlayerRef playerRef);
}