using UnityEngine;


[CreateAssetMenu(fileName = "roomSpaceInstance", menuName = "Room Space")]
public class RoomSpace : ScriptableObject
{
    [SerializeField] private Color backgroundColor;


    public Color BackgroundColor
    {
        get { return backgroundColor; }
    }
}
