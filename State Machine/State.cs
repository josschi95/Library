using UnityEngine;

public abstract class State : MonoBehaviour
{
    public abstract State OnStateEnter(CharacterController controller);
    public abstract State Tick(CharacterController controller);
    public abstract void OnStateExit(CharacterController controller);
}
