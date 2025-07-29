using Fusion;

[System.Serializable]
public enum InteractableType { None, Tree, Stone, Box, Campfire }

public class TestInteractable : NetworkBehaviour
{
    public InteractableType InteractableType;
}