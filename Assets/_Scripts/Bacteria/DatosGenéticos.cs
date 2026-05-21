using UnityEngine;

/*
- Este struct se encarga de almacenar los datos geneticos de cada bacteria, incluyendo su linaje, generaciones, velocidad, radio de vision, energia maxima, consumo, tamano, vida util, rango de mutacion, tiempo entre reproduccion y color del linaje.
- Estos datos son utilizados para determinar el comportamiento de cada bacteria, su capacidad de supervivencia y
    su capacidad de reproduccion.
- El struct incluye un metodo para calcular el gasto energetico de una bacteria en funcion de su tamano, velocidad y radio de vision.
*/

[System.Serializable]
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
        // Elevamos al cubo (t*t*t) para penalizar el tamanno masivamente
        return ((t * t * t) + (v * v) + r) * 0.25f;
    }
    public static float CalcularCosteReproduccion(float consumo, float tiempoEntreReproduccion)
    {
        // Fórmula basada en el metabolismo: 
        // A mayor tiempo o mayor consumo, más cuesta reproducirse, 
        // pero permite alcanzar tamaños gigantes de hasta 4.0 sin morir.
        return (tiempoEntreReproduccion * 0.5f) + (consumo * 10f);
    }
}
