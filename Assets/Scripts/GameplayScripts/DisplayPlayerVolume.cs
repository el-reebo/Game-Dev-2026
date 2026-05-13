using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DisplayPlayerVolume : MonoBehaviour
{
    [Header("References")]
    public PlayerSound Player;
    public Gun Gun;

    [Header("Settings")]
    public float Peak = 150f;
    [SerializeField] private float DecayRate = 5f;

    private Slider visualiser;
    private Image fill;
    private float DisplayValue = 0f;

    // Visualiser colours
    private Color BottomColor = Color.white;
    private Color MiddleColor = new Color(1f, 0.5f, 0f); //orange
    private Color TopColor = Color.red;

    void Awake()
    {
        Gun.GunShot += VisualiseGunShot; 

        visualiser = GetComponent<Slider>();
        fill = visualiser.fillRect.GetComponent<Image>();
    }

    private void VisualiseGunShot()
    {
        // Show gun shot sound peak on visualiser
        DisplayValue = 6f;
    }

    private Color GetColor(float value)
    {
        if (value < 0.5f)
        {
            return Color.Lerp(BottomColor, MiddleColor, value * 2);
        }
        else
        {
            return Color.Lerp(MiddleColor, TopColor, (value - 0.5f) * 2);
        }
    }

    void Update()
    {
        float targetValue = Player.CurrentSoundRadius / Peak;

        DisplayValue = Mathf.Lerp(DisplayValue, targetValue, Time.deltaTime * DecayRate);
        visualiser.value = DisplayValue;

        fill.color = GetColor(DisplayValue);
    }

    
}