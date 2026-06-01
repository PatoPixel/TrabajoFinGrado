using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SaveData
{
    // Datos del Gestor
    public int proximoIdLinaje;

    // Datos del Entorno (NUEVO)
    public float radioPlacaPetri;

    public List<DatosPlantillaEspecie> plantillasLaboratorio = new List<DatosPlantillaEspecie>();
    // Datos de las Entidades Vivas
    public List<DatosEntidad> bacteriasVivas = new List<DatosEntidad>();

    // Datos del EvolutionTracker (Snapshots por Especie)
    public List<DatosContenedorEspecie> historialEspecies = new List<DatosContenedorEspecie>();

    // Datos del Mundo
    public List<DatosComidas> datosComidas = new List<DatosComidas>();

    // Datos de los Spawners de Comida (NUEVO)
    public List<DatosSpawner> datosSpawners = new List<DatosSpawner>();
}

[System.Serializable]
public class DatosSpawner
{
    public float posX;
    public float posY;
    public float minEnergia;
    public float maxEnergia;
    public float intervalo;
    public float radioSpawn;

    public DatosSpawner() { }
    public DatosSpawner(float x, float y, float minE, float maxE, float intervalo, float radio)
    {
        this.posX = x;
        this.posY = y;
        this.minEnergia = minE;
        this.maxEnergia = maxE;
        this.intervalo = intervalo;
        this.radioSpawn = radio;
    }
}

[System.Serializable]
public class DatosComidas
{
    public float posX;
    public float posY;
    public float tamano;
    public DatosComidas() { }
    public DatosComidas(float x, float y, float tamano)
    {
        this.posX = x;
        this.posY = y;
        this.tamano = tamano;
    }
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
    public int idLinaje;
    public int generaciones;
    public string nombreDePila;
    public Color colorLinaje;

    public float velocidad;
    public float radioVision;
    public float energiaMax;
    public float consumo;
    public float tamano;
    public float vidaUtil;
    public float rangoMutacion;
    public float tiempreEntreReproduccion;

    public float posX;
    public float posY;
    public float quaternionZ;
    public float energiaActual;
    public float edadActual;
    public float cooldownRestante;

    public DatosEntidad() { }

    public DatosEntidad(SistemaVida sv, Vector3 posicion, float giro)
    {
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

        this.nombreDePila = sv.name;
        this.energiaActual = sv.EnergiaActual;
        this.edadActual = sv.EdadActual;
        this.cooldownRestante = 0;

        this.posX = posicion.x;
        this.posY = posicion.y;

        this.quaternionZ = giro;
    }
}

[System.Serializable]
public class DatosPlantillaEspecie
{
    public int idLinaje;
    public DatosGeneticos stats;
    public string nombre;

    public DatosPlantillaEspecie() { }
    public DatosPlantillaEspecie(int id, DatosGeneticos stats, string nombre)
    {
        this.idLinaje = id;
        this.stats = stats;
        this.nombre = nombre;
    }
}