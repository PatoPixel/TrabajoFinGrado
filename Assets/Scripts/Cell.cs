using UnityEngine;

// 1. Definimos los tipos de materiales que existen en tu mundo
public enum ElementType
{
    Empty,  // Aire/Vacío
    Sand,   // Arena (Cae)
    Water,  // Agua (Fluye)
    Stone   // Piedra (Estática)
}

[System.Serializable]
public class Cell
{
    public int x;
    public int y;

    // 2. Ahora en vez de "isWater", tenemos un "Type"
    public ElementType Type;

    public Cell(int x, int y)
    {
        this.x = x;
        this.y = y;
        this.Type = ElementType.Empty; // Por defecto, vacío
    }
}