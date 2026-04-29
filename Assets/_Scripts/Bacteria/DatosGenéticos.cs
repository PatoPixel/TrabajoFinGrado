using UnityEngine;

[System.Serializable] // Para que aparezca en el Inspector de Unity
public struct DatosGeneticos
{
    public int idLinaje;
    public int generaciones;
    public float velocidad;
    public float radioVision;
    public float energiaMax;
    public float consumo;
    public float tamano;
    public float vidaUtil;
    public float rangoMutacion;
    public float tiempreEntreReproduccion;
    public Color colorLinaje;
    public DatosGeneticos(int idLinaje, int generaciones, float velocidad, float radioVision, float energiaMax, float consumo, float tamano, float vidaUtil, float rangoMutacion, float tiempoEntreReproduccion, Color color)
    {
        this.idLinaje = idLinaje;
        this.generaciones = generaciones;
        this.velocidad = velocidad;
        this.radioVision = radioVision;
        this.energiaMax = energiaMax;
        this.consumo = consumo;
        this.tamano = tamano;
        this.vidaUtil = vidaUtil;
        this.rangoMutacion = rangoMutacion;
        this.tiempreEntreReproduccion = tiempoEntreReproduccion;
        this.colorLinaje = color;
    }
    public static float CalcularGasto(float t, float v, float r)
    {
        // Elevamos al cubo (t*t*t) para penalizar el tamaño masivamente
        return ((t * t * t) + (v * v) + r) * 0.25f;
    }
}
