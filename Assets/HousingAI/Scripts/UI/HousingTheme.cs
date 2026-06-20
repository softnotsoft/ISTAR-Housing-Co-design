using UnityEngine;

[CreateAssetMenu(fileName = "HousingTheme", menuName = "HousingAI/UI Theme")]
/// <summary>
/// Paleta reutilizavel configurada como ScriptableObject no editor Unity.
/// </summary>
public class HousingTheme : ScriptableObject
{
    public Color Primary;
    public Color PrimaryDarker;
    public Color Secondary;
    public Color Background1;
    public Color Background2;
    public Color Gray;
    public Color Accent;
    public Color AccentHover;
}
