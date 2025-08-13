using UnityEngine;
using Fusion;

namespace YSB_Scripts
{
    public interface IInteractable
    {
        bool CanInteract();
        void Interact(PlayerRef playerRef, int att);
    }
}