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
    public float EficienciaAlimentacion;
    public float vidaUtil;
    public float rangoMutacion;
    public DatosGeneticos(int idLinaje, int generaciones, float velocidad, float radioVision, float energiaMax, float consumo, float tamano, float eficienciaAlimentacion, float vidaUtil, float rangoMutacion)
    {
        this.idLinaje = idLinaje;
        this.generaciones = generaciones;
        this.velocidad = velocidad;
        this.radioVision = radioVision;
        this.energiaMax = energiaMax;
        this.consumo = consumo;
        this.tamano = tamano;
        this.EficienciaAlimentacion = eficienciaAlimentacion;
        this.vidaUtil = vidaUtil;
        this.rangoMutacion = rangoMutacion;
    }
}
