using UnityEngine;

public class CellInfo : MonoBehaviour 
{
    [SerializeField] private SpriteRenderer _spriteRenderer;

    public void ChangeState(bool isActive)
    {
        _spriteRenderer.color = isActive ? Colors.ActiveColor : Colors.InactiveColor;
    }
}