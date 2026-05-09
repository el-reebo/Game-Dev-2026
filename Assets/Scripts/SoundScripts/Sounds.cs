using UnityEngine;

public static class Sounds
{
    public static void MakeSound(Sound sound)
    {
        // Debug.Log("MakeSound() function called");
        Collider[] col = Physics.OverlapSphere(sound.pos, sound.range); // Ensure IHear object has includes atleast 1 non mesh collider
        // Debug.Log($"Sound Position: {sound.pos}");

        int hearerCheck = 0; // Debug variable

        // Loop to search for IHear objects
        for (int i = 0; i < col.Length; i++)
        {
            var hearer = col[i].GetComponentInParent<IHear>();
            if (hearer != null)
            {
                hearer.RespondToSound(sound);
                hearerCheck++; // Debug variable
            }
        }
                
        // Debug.Log($"{hearerCheck} hearers were detected");
    }
}
