public interface IInteractable
{
    string GetInteractionPrompt();
    void Interact(PlayerController player);
}