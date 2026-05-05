using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SaveData
{
    // Datos del Gestor
    public int proximoIdLinaje;

    // Datos de las Entidades Vivas
    public List<DatosEntidad> bacteriasVivas = new List<DatosEntidad>();

    // Datos del EvolutionTracker (Snapshots por Especie)
    public List<DatosContenedorEspecie> historialEspecies = new List<DatosContenedorEspecie>();

    // Datos del Mundo
    public List<Vector2> posicionesComida = new List<Vector2>();
}
[System.Serializable]
public class DatosContenedorEspecie
{
    public int idLinaje;
    public List<EspeciesSnapshot> historial;
    public RangoEstadisticoEspecie rango;

    public DatosContenedorEspecie() { }

    public DatosContenedorEspecie(int id, List<EspeciesSnapshot> historial, RangoEstadisticoEspecie rango)
    {
        this.idLinaje = id;
        this.historial = historial;
        this.rango = rango;
    }
}
[System.Serializable]
public class DatosEntidad
{
    // Identidad y Linaje
    public int idLinaje;
    public int generaciones;
    public string nombreDePila;
    public Color colorLinaje;

    // Genes (Estadísticas base)
    public float velocidad;
    public float radioVision;
    public float energiaMax;
    public float consumo;
    public float tamano;
    public float vidaUtil;
    public float rangoMutacion;
    public float tiempreEntreReproduccion;

    // Estado dinámico (Lo que cambia en tiempo real)
    public float posX;
    public float posY;
    public float energiaActual;
    public float edadActual;
    public float cooldownRestante;

    // Constructor vacío obligatorio para la serialización
    public DatosEntidad() { }

    // Constructor de ayuda para "sacar la foto" a una bacteria viva
    public DatosEntidad(SistemaVida sv, Vector3 posicion)
    {
        // Copiamos los genes desde misStats
        this.idLinaje = sv.misStats.idLinaje;
        this.generaciones = sv.misStats.generaciones;
        this.velocidad = sv.misStats.velocidad;
        this.radioVision = sv.misStats.radioVision;
        this.energiaMax = sv.misStats.energiaMax;
        this.consumo = sv.misStats.consumo;
        this.tamano = sv.misStats.tamano;
        this.vidaUtil = sv.misStats.vidaUtil;
        this.rangoMutacion = sv.misStats.rangoMutacion;
        this.tiempreEntreReproduccion = sv.misStats.tiempreEntreReproduccion;
        this.colorLinaje = sv.misStats.colorLinaje;

        // Copiamos el estado actual
        this.nombreDePila = sv.name; // O la variable que uses
        this.energiaActual = sv.EnergiaActual;
        this.edadActual = sv.EdadActual;
        this.cooldownRestante = 0; // O el valor que tenga en ese momento

        // Posición
        this.posX = posicion.x;
        this.posY = posicion.y;
    }
}