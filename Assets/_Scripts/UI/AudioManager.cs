using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Gestor de música + ajustes. Persiste entre escenas (DontDestroyOnLoad).
/// Crea su propio canvas con botón de música, botón de ajustes y panel de ajustes.
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio")]
    [Range(0f, 1f)] public float volumen = 0.35f;

    private AudioClip[] playlist;
    private AudioSource _source;
    private int[]       _shuffled;
    private int         _indiceActual = 0;
    private bool        _silenciado   = false;

    // Sprites
    private Sprite _spriteOn;
    private Sprite _spriteOff;
    private Sprite _spriteGear;

    // UI refs
    private Image      _imagenBoton;
    private GameObject _panelAjustes;
    private Slider     _sliderVolumen;
    private Toggle     _togglePantallaCompleta;

    // ── Ciclo de vida ───────────────────────────────────────────────────────

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        _source        = gameObject.AddComponent<AudioSource>();
        _source.loop   = false;
        _source.volume = PlayerPrefs.GetFloat("volumen", volumen);

        // Aplicar pantalla completa guardada
        bool fs = PlayerPrefs.GetInt("pantallaCompleta", 1) == 1;
        Screen.fullScreen = fs;

        // Cargar sprites
        Texture2D texOn   = Resources.Load<Texture2D>("musicOn");
        Texture2D texOff  = Resources.Load<Texture2D>("musicOff");
        Texture2D texGear = Resources.Load<Texture2D>("gear");
        if (texOn   != null) _spriteOn   = Sprite.Create(texOn,   new Rect(0, 0, texOn.width,   texOn.height),   Vector2.one * 0.5f);
        if (texOff  != null) _spriteOff  = Sprite.Create(texOff,  new Rect(0, 0, texOff.width,  texOff.height),  Vector2.one * 0.5f);
        if (texGear != null) _spriteGear = Sprite.Create(texGear, new Rect(0, 0, texGear.width, texGear.height), Vector2.one * 0.5f);

        // Cargar playlist
        playlist = Resources.LoadAll<AudioClip>("Audio");
        if (playlist == null || playlist.Length == 0)
            Debug.LogWarning("[AudioManager] No se encontraron clips en Resources/Audio.");

        ConstruirUI();
        Barajar();
    }

    private void Start() => ReproducirActual();

    private void Update()
    {
        if (!_source.isPlaying && !_silenciado)
        {
            _indiceActual++;
            if (_indiceActual >= _shuffled.Length) { Barajar(); _indiceActual = 0; }
            ReproducirActual();
        }
    }

    // ── Reproducción ────────────────────────────────────────────────────────

    private void ReproducirActual()
    {
        if (playlist == null || playlist.Length == 0) return;
        AudioClip clip = playlist[_shuffled[_indiceActual]];
        if (clip == null) return;
        _source.clip = clip;
        _source.Play();
    }

    private void Barajar()
    {
        _shuffled = new int[playlist.Length];
        for (int i = 0; i < _shuffled.Length; i++) _shuffled[i] = i;
        for (int i = _shuffled.Length - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (_shuffled[i], _shuffled[j]) = (_shuffled[j], _shuffled[i]);
        }
    }

    // ── Música ──────────────────────────────────────────────────────────────

    public void ToggleMute()
    {
        _silenciado = !_silenciado;
        if (_silenciado)
        {
            _source.Pause();
            if (_imagenBoton && _spriteOff) _imagenBoton.sprite = _spriteOff;
        }
        else
        {
            _source.UnPause();
            if (!_source.isPlaying) ReproducirActual();
            if (_imagenBoton && _spriteOn) _imagenBoton.sprite = _spriteOn;
        }
    }

    // ── Ajustes ─────────────────────────────────────────────────────────────

    private void ToggleAjustes()
    {
        bool abierto = !_panelAjustes.activeSelf;
        _panelAjustes.SetActive(abierto);

        if (abierto)
        {
            // Sincronizar controles con valores actuales
            if (_sliderVolumen)          _sliderVolumen.value          = _source.volume;
            if (_togglePantallaCompleta) _togglePantallaCompleta.isOn  = Screen.fullScreen;
        }
    }

    private void OnCambiarVolumen(float valor)
    {
        _source.volume = valor;
        PlayerPrefs.SetFloat("volumen", valor);
        PlayerPrefs.Save();
    }

    private void OnCambiarPantallaCompleta(bool activo)
    {
        Screen.fullScreen = activo;
        PlayerPrefs.SetInt("pantallaCompleta", activo ? 1 : 0);
        PlayerPrefs.Save();
    }

    // ── Construcción de UI ──────────────────────────────────────────────────

    private void ConstruirUI()
    {
        // Canvas persistente
        GameObject canvasObj = new GameObject("AudioManager_Canvas");
        canvasObj.transform.SetParent(transform);
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999;
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasObj.AddComponent<GraphicRaycaster>();

        // ── Botón música (esquina inferior derecha) ──────────────────────
        _imagenBoton = CrearBoton(canvasObj.transform, "BotonMusica",
            new Vector2(-18f, 18f), ToggleMute, _spriteOn);

        // ── Botón ajustes (justo a la izquierda del de música) ───────────
        CrearBoton(canvasObj.transform, "BotonAjustes",
            new Vector2(-82f, 18f), ToggleAjustes, _spriteGear);

        // ── Panel de ajustes ─────────────────────────────────────────────
        _panelAjustes = ConstruirPanelAjustes(canvasObj.transform);
        _panelAjustes.SetActive(false);
    }

    /// <summary>Crea un botón cuadrado oscuro con sprite en la esquina inferior derecha.</summary>
    private Image CrearBoton(Transform parent, string nombre, Vector2 pos,
                             UnityEngine.Events.UnityAction accion, Sprite icono)
    {
        GameObject btnObj = new GameObject(nombre);
        btnObj.transform.SetParent(parent, false);
        RectTransform rt = btnObj.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(1f, 0f);
        rt.pivot            = new Vector2(1f, 0f);
        rt.anchoredPosition = pos;
        rt.sizeDelta        = new Vector2(56f, 56f);
        Image fondo = btnObj.AddComponent<Image>();
        fondo.color = new Color(0f, 0f, 0f, 0.45f);
        Button btn = btnObj.AddComponent<Button>();
        ColorBlock cb = btn.colors;
        cb.highlightedColor = new Color(1f, 1f, 1f, 0.2f);
        cb.pressedColor     = new Color(1f, 1f, 1f, 0.35f);
        btn.colors = cb;
        btn.onClick.AddListener(accion);

        // Icono sprite
        if (icono != null)
        {
            GameObject iconObj = new GameObject("Icono");
            iconObj.transform.SetParent(btnObj.transform, false);
            RectTransform iconRt = iconObj.AddComponent<RectTransform>();
            iconRt.anchorMin = new Vector2(0.1f, 0.1f);
            iconRt.anchorMax = new Vector2(0.9f, 0.9f);
            iconRt.offsetMin = iconRt.offsetMax = Vector2.zero;
            Image img = iconObj.AddComponent<Image>();
            img.sprite         = icono;
            img.preserveAspect = true;
            img.raycastTarget  = false;
            return img;
        }
        return null;
    }

    /// <summary>Crea un botón cuadrado oscuro con texto (para el icono de ajustes).</summary>
    private void CrearBotonTexto(Transform parent, string nombre, Vector2 pos,
                                 UnityEngine.Events.UnityAction accion, string texto)
    {
        GameObject btnObj = new GameObject(nombre);
        btnObj.transform.SetParent(parent, false);
        RectTransform rt = btnObj.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(1f, 0f);
        rt.pivot            = new Vector2(1f, 0f);
        rt.anchoredPosition = pos;
        rt.sizeDelta        = new Vector2(56f, 56f);
        Image fondo = btnObj.AddComponent<Image>();
        fondo.color = new Color(0f, 0f, 0f, 0.45f);
        Button btn = btnObj.AddComponent<Button>();
        ColorBlock cb = btn.colors;
        cb.highlightedColor = new Color(1f, 1f, 1f, 0.2f);
        cb.pressedColor     = new Color(1f, 1f, 1f, 0.35f);
        btn.colors = cb;
        btn.onClick.AddListener(accion);

        GameObject txtObj = new GameObject("Texto");
        txtObj.transform.SetParent(btnObj.transform, false);
        RectTransform txtRt = txtObj.AddComponent<RectTransform>();
        txtRt.anchorMin = Vector2.zero; txtRt.anchorMax = Vector2.one;
        txtRt.offsetMin = txtRt.offsetMax = Vector2.zero;
        TextMeshProUGUI tmp = txtObj.AddComponent<TextMeshProUGUI>();
        tmp.text          = texto;
        tmp.fontSize      = 28f;
        tmp.alignment     = TextAlignmentOptions.Center;
        tmp.color         = Color.white;
        tmp.raycastTarget = false;
    }

    /// <summary>Construye el panel de ajustes con slider de volumen y toggle de pantalla completa.</summary>
    private GameObject ConstruirPanelAjustes(Transform parent)
    {
        // Fondo panel — ancla esquina inferior derecha, crece hacia arriba/izquierda
        GameObject panel = new GameObject("PanelAjustes");
        panel.transform.SetParent(parent, false);
        RectTransform panelRt = panel.AddComponent<RectTransform>();
        panelRt.anchorMin        = new Vector2(1f, 0f);
        panelRt.anchorMax        = new Vector2(1f, 0f);
        panelRt.pivot            = new Vector2(1f, 0f);
        panelRt.anchoredPosition = new Vector2(-18f, 82f);   // Alineado con el borde derecho
        panelRt.sizeDelta        = new Vector2(300f, 170f);  // Más ancho para que el slider quepa
        Image panelImg = panel.AddComponent<Image>();
        panelImg.color = new Color(0.05f, 0.05f, 0.08f, 0.95f);

        // ── Título ───────────────────────────────────────────────────────
        AnadirTexto(panel.transform, "Ajustes", new Vector2(0f, 65f), new Vector2(280f, 30f), 18f, true);

        // ── Separador ────────────────────────────────────────────────────
        GameObject sep = new GameObject("Sep");
        sep.transform.SetParent(panel.transform, false);
        RectTransform sepRt = sep.AddComponent<RectTransform>();
        sepRt.anchoredPosition = new Vector2(0f, 48f);
        sepRt.sizeDelta        = new Vector2(270f, 1f);
        sep.AddComponent<Image>().color = new Color(1f, 1f, 1f, 0.15f);

        // ── Fila: Pantalla completa ──────────────────────────────────────
        // Etiqueta a la izquierda, toggle a la derecha
        AnadirTexto(panel.transform, "Pantalla completa", new Vector2(-50f, 20f), new Vector2(160f, 24f), 14f, false);
        _togglePantallaCompleta = AnadirToggle(panel.transform, new Vector2(118f, 20f));
        _togglePantallaCompleta.isOn = Screen.fullScreen;
        _togglePantallaCompleta.onValueChanged.AddListener(OnCambiarPantallaCompleta);

        // ── Fila: Volumen música ─────────────────────────────────────────
        // Etiqueta a la izquierda, slider a la derecha (más corto para que quepa)
        AnadirTexto(panel.transform, "Volumen musica", new Vector2(-62f, -20f), new Vector2(130f, 24f), 14f, false);
        _sliderVolumen = AnadirSlider(panel.transform, new Vector2(80f, -20f), 120f);
        _sliderVolumen.value = _source.volume;
        _sliderVolumen.onValueChanged.AddListener(OnCambiarVolumen);

        return panel;
    }

    // ── Helpers de UI ───────────────────────────────────────────────────────

    private TextMeshProUGUI AnadirTexto(Transform parent, string texto, Vector2 pos, Vector2 size,
                                         float fontSize, bool negrita)
    {
        GameObject obj = new GameObject("Txt_" + texto);
        obj.transform.SetParent(parent, false);
        RectTransform rt = obj.AddComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta        = size;
        TextMeshProUGUI tmp = obj.AddComponent<TextMeshProUGUI>();
        tmp.text          = texto;
        tmp.fontSize      = fontSize;
        tmp.color         = Color.white;
        tmp.alignment     = TextAlignmentOptions.Center;
        tmp.fontStyle     = negrita ? FontStyles.Bold : FontStyles.Normal;
        tmp.raycastTarget = false;
        return tmp;
    }

    private Toggle AnadirToggle(Transform parent, Vector2 pos)
    {
        GameObject obj = new GameObject("Toggle");
        obj.transform.SetParent(parent, false);
        RectTransform rt = obj.AddComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta        = new Vector2(24f, 24f);
        Toggle toggle = obj.AddComponent<Toggle>();

        // Fondo del toggle
        GameObject bg = new GameObject("Background");
        bg.transform.SetParent(obj.transform, false);
        RectTransform bgRt = bg.AddComponent<RectTransform>();
        bgRt.anchorMin = Vector2.zero; bgRt.anchorMax = Vector2.one;
        bgRt.offsetMin = bgRt.offsetMax = Vector2.zero;
        Image bgImg = bg.AddComponent<Image>();
        bgImg.color = new Color(0.3f, 0.3f, 0.3f, 1f);
        toggle.targetGraphic = bgImg;

        // Check (marca)
        GameObject check = new GameObject("Checkmark");
        check.transform.SetParent(bg.transform, false);
        RectTransform checkRt = check.AddComponent<RectTransform>();
        checkRt.anchorMin = new Vector2(0.1f, 0.1f);
        checkRt.anchorMax = new Vector2(0.9f, 0.9f);
        checkRt.offsetMin = checkRt.offsetMax = Vector2.zero;
        Image checkImg = check.AddComponent<Image>();
        checkImg.color = new Color(0.3f, 0.8f, 0.4f, 1f);
        toggle.graphic = checkImg;

        return toggle;
    }

    private Slider AnadirSlider(Transform parent, Vector2 pos, float ancho = 120f)
    {
        GameObject obj = new GameObject("Slider");
        obj.transform.SetParent(parent, false);
        RectTransform rt = obj.AddComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta        = new Vector2(ancho, 18f);

        Slider slider = obj.AddComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.wholeNumbers = false;

        // Fondo
        GameObject bg = new GameObject("Background");
        bg.transform.SetParent(obj.transform, false);
        RectTransform bgRt = bg.AddComponent<RectTransform>();
        bgRt.anchorMin = new Vector2(0f, 0.25f);
        bgRt.anchorMax = new Vector2(1f, 0.75f);
        bgRt.offsetMin = bgRt.offsetMax = Vector2.zero;
        Image bgImg = bg.AddComponent<Image>();
        bgImg.color = new Color(0.3f, 0.3f, 0.3f, 1f);

        // Fill area
        GameObject fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(obj.transform, false);
        RectTransform faRt = fillArea.AddComponent<RectTransform>();
        faRt.anchorMin = new Vector2(0f, 0.25f);
        faRt.anchorMax = new Vector2(1f, 0.75f);
        faRt.offsetMin = new Vector2(5f, 0f);
        faRt.offsetMax = new Vector2(-15f, 0f);

        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(fillArea.transform, false);
        RectTransform fillRt = fill.AddComponent<RectTransform>();
        fillRt.anchorMin = Vector2.zero;
        fillRt.anchorMax = Vector2.one;
        fillRt.offsetMin = fillRt.offsetMax = Vector2.zero;
        Image fillImg = fill.AddComponent<Image>();
        fillImg.color = new Color(0.3f, 0.7f, 1f, 1f);
        slider.fillRect = fillRt;

        // Handle
        GameObject handleArea = new GameObject("Handle Slide Area");
        handleArea.transform.SetParent(obj.transform, false);
        RectTransform haRt = handleArea.AddComponent<RectTransform>();
        haRt.anchorMin = Vector2.zero; haRt.anchorMax = Vector2.one;
        haRt.offsetMin = haRt.offsetMax = Vector2.zero;

        GameObject handle = new GameObject("Handle");
        handle.transform.SetParent(handleArea.transform, false);
        RectTransform hRt = handle.AddComponent<RectTransform>();
        hRt.sizeDelta = new Vector2(18f, 18f);
        Image handleImg = handle.AddComponent<Image>();
        handleImg.color = Color.white;
        slider.handleRect = hRt;
        slider.targetGraphic = handleImg;

        return slider;
    }
}
