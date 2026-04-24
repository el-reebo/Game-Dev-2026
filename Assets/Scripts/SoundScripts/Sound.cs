using UnityEngine;

public class Sound
{
    public Sound(Vector3 _pos, float _range, float _priority)
    {
        pos = _pos;
        range = _range;
        priority = _priority;
    }

    public readonly Vector3 pos;
    public readonly float range;
    public readonly float priority;
}
