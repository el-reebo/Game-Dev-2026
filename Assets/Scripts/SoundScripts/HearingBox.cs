using UnityEngine;

public class HearingBox : MonoBehaviour, IHear
{
    public void RespondToSound(Sound sound)
    {
        Debug.Log("Hello I'm a box :3");
    }
}
