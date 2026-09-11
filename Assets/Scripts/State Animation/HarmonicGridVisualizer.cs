using UnityEngine;
using UnityEngine.UI;

public class HarmonicGridVisualizer : MonoBehaviour
{
    private const int GRID_SIZE = 50; // 50 * 50 = 2500 grid cells

    public enum VisualMode
    {
        ModeA,  // Original color set
        ModeB   // Alternative second color set
    }

    [System.Serializable]
    public struct ModeSettings
    {
        public Color[] palette;
        public float animationSpeed;
        public int stepsPerFrame;
    }

    [Header("Binding Targets")]
    [Tooltip("Attach RawImage if using UI; leave empty to auto‑generate a world‑space Quad")]
    public RawImage targetRawImage;
    public Renderer targetMeshRenderer;

    [Header("Animation Parameters (Global)")]
    [Tooltip("Enable smooth interpolation transition; disable for pixel‑jump style")]
    public bool smoothTransition = true;

    [Header("Mode Selection")]
    [Tooltip("Switch between two independent color palette sets")]
    public VisualMode currentMode = VisualMode.ModeA;

    [Header("Mode A Settings")]
    public ModeSettings modeASettings;

    [Header("Mode B Settings")]
    public ModeSettings modeBSettings;

    // Internal state for a single grid cell
    private struct CellState
    {
        public Color currentColor;
        public Color targetColor;
        public float progress;      // 0 ~ 1
        public float speedFactor;   // Random fluctuation factor
    }

    private CellState[] cells = new CellState[GRID_SIZE * GRID_SIZE];
    private Texture2D gridTexture;
    private Color32[] pixelBuffer = new Color32[GRID_SIZE * GRID_SIZE];
    private float elapsedTime = 0f;
    private VisualMode _lastMode;

    void Start()
    {
        // Mode A default values (original)
        if (modeASettings.palette == null || modeASettings.palette.Length == 0)
        {
            modeASettings.palette = new Color[]
            {
                new Color32(255, 209, 102, 255), // Warm Yellow #FFD166
                new Color32(6, 214, 160, 255),   // Mint Green #06D6A0
                new Color32(255, 159, 28, 255),  // Vibrant Orange #FF9F1C
                new Color32(255, 112, 166, 255), // Pink‑Orange #FF70A6
                new Color32(17, 138, 178, 255),  // Sky Blue #118AB2
                new Color32(255, 133, 161, 255)  // Coral Pink #FF85A1
            };
            modeASettings.animationSpeed = 1.0f;
            modeASettings.stepsPerFrame = 40;
        }

        // Mode B default values
        if (modeBSettings.palette == null || modeBSettings.palette.Length == 0)
        {
            modeBSettings.palette = new Color[]
            {
                new Color32(30, 40, 70, 255),
                new Color32(80, 30, 110, 255),
                new Color32(20, 90, 95, 255),
                new Color32(180, 60, 90, 255),
                new Color32(10, 20, 35, 255),
                new Color32(60, 70, 130, 255)
            };
            modeBSettings.animationSpeed = 0.6f;
            modeBSettings.stepsPerFrame = 25;
        }

        _lastMode = currentMode;
        InitTexture();
        InitCells();
    }

    void InitTexture()
    {
        // Create 50*50 uncompressed texture, Point sampling for sharp pixel grid edges
        gridTexture = new Texture2D(GRID_SIZE, GRID_SIZE, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp
        };
        // Assign texture to UI RawImage
        if (targetRawImage != null)
        {
            targetRawImage.texture = gridTexture;
        }
        // Or assign to 3D MeshRenderer (e.g. Quad / Cube)
        else if (targetMeshRenderer != null)
        {
            targetMeshRenderer.material.mainTexture = gridTexture;
        }
        else
        {
            // Auto‑fetch component on current GameObject if nothing assigned
            if (TryGetComponent<RawImage>(out var rawImg))
            {
                targetRawImage = rawImg;
                targetRawImage.texture = gridTexture;
            }
            else if (TryGetComponent<Renderer>(out var rend))
            {
                targetMeshRenderer = rend;
                targetMeshRenderer.material.mainTexture = gridTexture;
            }
        }
    }

    void InitCells()
    {
        ModeSettings settings = GetActiveModeSettings();
        Color[] activePalette = settings.palette;

        for (int y = 0; y < GRID_SIZE; y++)
        {
            for (int x = 0; x < GRID_SIZE; x++)
            {
                int idx = y * GRID_SIZE + x;
                // Spawn initial color patches using spatial pseudo‑noise
                float noise = PseudoNoise2D(x, y, 0f);
                int colorIndex = Mathf.FloorToInt(noise * activePalette.Length) % activePalette.Length;
                Color initColor = activePalette[colorIndex];
                cells[idx] = new CellState
                {
                    currentColor = initColor,
                    targetColor = initColor,
                    progress = 1.0f,
                    speedFactor = UnityEngine.Random.Range(0.8f, 1.2f)
                };
                pixelBuffer[idx] = (Color32)initColor;
            }
        }
        gridTexture.SetPixels32(pixelBuffer);
        gridTexture.Apply(false);
    }

    ModeSettings GetActiveModeSettings()
    {
        if (currentMode == VisualMode.ModeB)
            return modeBSettings;
        return modeASettings;
    }

    void Update()
    {
        // Detect mode changed from inspector
        if (_lastMode != currentMode)
        {
            OnModeSwitched();
            _lastMode = currentMode;
        }

        ModeSettings settings = GetActiveModeSettings();
        Color[] activePalette = settings.palette;

        float dt = Mathf.Min(Time.deltaTime, 0.05f);
        elapsedTime += dt * settings.animationSpeed;

        // 1. Asynchronously select partial cells to sample flowing noise field
        int stepCount = Mathf.FloorToInt(settings.stepsPerFrame * settings.animationSpeed);
        for (int i = 0; i < stepCount; i++)
        {
            int rx = UnityEngine.Random.Range(0, GRID_SIZE);
            int ry = UnityEngine.Random.Range(0, GRID_SIZE);
            int idx = ry * GRID_SIZE + rx;
            // Trigger new flow only when previous transition is nearly finished
            if (cells[idx].progress >= 0.85f)
            {
                float noiseVal = PseudoNoise2D(rx, ry, elapsedTime * 0.45f);
                int colorIdx = Mathf.FloorToInt(noiseVal * activePalette.Length) % activePalette.Length;
                Color targetCol = activePalette[colorIdx];
                if (cells[idx].targetColor != targetCol)
                {
                    cells[idx].targetColor = targetCol;
                    cells[idx].progress = smoothTransition ? 0f : 1f;
                }
            }
        }
        // 2. Per‑cell RGB smooth lerp and write to pixel buffer
        float lerpRate = dt * 2.5f * settings.animationSpeed;
        for (int i = 0; i < cells.Length; i++)
        {
            if (cells[i].progress < 1.0f)
            {
                cells[i].progress = Mathf.Clamp01(cells[i].progress + lerpRate * cells[i].speedFactor);
                cells[i].currentColor = Color.Lerp(cells[i].currentColor, cells[i].targetColor, cells[i].progress);
            }
            pixelBuffer[i] = (Color32)cells[i].currentColor;
        }
        // 3. Upload pixel data to GPU in one batch (no GC overhead)
        gridTexture.SetPixels32(pixelBuffer);
        gridTexture.Apply(false);
    }

    /// <summary>
    /// Called when switching ModeA / ModeB in inspector
    /// </summary>
    void OnModeSwitched()
    {
        ModeSettings settings = GetActiveModeSettings();
        Color[] activePalette = settings.palette;
        for (int y = 0; y < GRID_SIZE; y++)
        {
            for (int x = 0; x < GRID_SIZE; x++)
            {
                int idx = y * GRID_SIZE + x;
                float noiseVal = PseudoNoise2D(x, y, elapsedTime * 0.45f);
                int colorIdx = Mathf.FloorToInt(noiseVal * activePalette.Length) % activePalette.Length;
                Color newTarget = activePalette[colorIdx];
                cells[idx].targetColor = newTarget;
                cells[idx].progress = smoothTransition ? 0f : 1f;
            }
        }
    }

    /// <summary>
    /// Core flow function: multi‑frequency sine‑based 2D pseudo‑continuous noise field
    /// </summary>
    private float PseudoNoise2D(int x, int y, float t)
    {
        // Component1: low‑frequency cross X‑Y harmonic wave
        float s1 = Mathf.Sin(x * 0.12f + t * 0.8f) * Mathf.Cos(y * 0.14f - t * 0.6f);
        // Component2: low‑frequency diagonal tilted wave
        float s2 = Mathf.Sin((x + y) * 0.08f + t * 0.5f);
        // Component3: radial wave centered at grid center (25, 25)
        float dx = x - 25f;
        float dy = y - 25f;
        float dist = Mathf.Sqrt(dx * dx + dy * dy);
        float s3 = Mathf.Cos(dist * 0.15f - t * 1.2f);
        // Normalize result to [0, 1]
        return (s1 + s2 + s3 + 3f) / 6f;
    }

    private void OnDestroy()
    {
        if (gridTexture != null)
        {
            Destroy(gridTexture);
        }
    }
}
