using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ConwayGame
{
    public enum State
    {
        Fill,
        Blank
    }

    public class ConwayCore : MonoBehaviour
    {
        public static ConwayCore Instance { get; private set; }

        [Header("References")]
        [SerializeField] private GameObject gameCellPrefab;
        [SerializeField] private GameObject gridLG;
        [SerializeField] private WaveAudioController waveAudioController;

        [Header("Game Setup")]
        [SerializeField] private int numGridSize;
        [SerializeField, Range(0.25f, 10f)]
        [Tooltip("Simulation steps per second. Higher values make the wave move more smoothly.")]
        private float simulationStepsPerSecond = 3f;

        [Header("Transition Rules")]
        [SerializeField, Min(0f)] private float recoveryRateX = 10f;
        [SerializeField, Min(0f)] private float infectionDecayY = 10f;

        [Header("Game Debug")]
        [SerializeField] private bool autoRun = true;
        [SerializeField] private TMP_Text autoRunText;
        [SerializeField] private Button stepButton;

        private readonly List<GameCell> gameCellList = new List<GameCell>();
        private readonly Dictionary<Vector2Int, GameCell> gameCellsByPosition =
            new Dictionary<Vector2Int, GameCell>();

        private Coroutine gameStepCoroutine;
        private bool waveActive;
        private int waveStep;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }

            Instance = this;

            int cellCount = numGridSize * numGridSize;
            DOTween.SetTweensCapacity(
                Mathf.Max(200, cellCount * 5),
                Mathf.Max(50, cellCount * 2));

            if (waveAudioController == null)
            {
                waveAudioController = GetComponent<WaveAudioController>();
            }

            if (waveAudioController == null)
            {
                waveAudioController = gameObject.AddComponent<WaveAudioController>();
            }
        }

        private void Start()
        {
            GridLayoutGroup gridLayout = gridLG.GetComponent<GridLayoutGroup>();
            RectTransform gridTransform = gridLG.GetComponent<RectTransform>();
            gridTransform.SetSizeWithCurrentAnchors(
                RectTransform.Axis.Horizontal,
                numGridSize * gridLayout.cellSize.x);
            gridTransform.SetSizeWithCurrentAnchors(
                RectTransform.Axis.Vertical,
                numGridSize * gridLayout.cellSize.y);

            for (int i = 0; i < numGridSize * numGridSize; i++)
            {
                GameCell newCell = Instantiate(gameCellPrefab, gridTransform).GetComponent<GameCell>();
                newCell.xCoOrd = i % numGridSize;
                newCell.yCoOrd = i / numGridSize;
                gameCellList.Add(newCell);
                gameCellsByPosition.Add(new Vector2Int(newCell.xCoOrd, newCell.yCoOrd), newCell);
            }

            if (autoRun)
            {
                StartAutoRun();
            }

            UpdateDebugControls();
        }

        private IEnumerator GameStep()
        {
            while (autoRun)
            {
                float stepInterval = 1f / Mathf.Max(0.25f, simulationStepsPerSecond);
                yield return new WaitForSeconds(stepInterval);
                StepNext();
            }

            gameStepCoroutine = null;
        }

        private void StartAutoRun()
        {
            if (gameStepCoroutine == null)
            {
                gameStepCoroutine = StartCoroutine(GameStep());
            }
        }

        private void StopAutoRun()
        {
            if (gameStepCoroutine == null)
            {
                return;
            }

            StopCoroutine(gameStepCoroutine);
            gameStepCoroutine = null;
        }

        private void StepNext()
        {
            if (!waveActive)
            {
                return;
            }

            waveStep++;

            Dictionary<GameCell, State> stateSnapshot = new Dictionary<GameCell, State>(gameCellList.Count);
            List<GameCell> negativeCells = new List<GameCell>();
            foreach (GameCell cell in gameCellList)
            {
                State state = cell.GetCellState();
                stateSnapshot.Add(cell, state);
                if (state == State.Blank)
                {
                    negativeCells.Add(cell);
                }
            }

            HashSet<GameCell> cellsToInfect = new HashSet<GameCell>();
            HashSet<GameCell> cellsToRecover = new HashSet<GameCell>();
            int infectionTargetCount = TransitionRules.GetInfectionTargetCount(waveStep);

            foreach (GameCell sourceCell in negativeCells)
            {
                sourceCell.AdvanceNegativeCounters();

                float recoveryChance = TransitionRules.GetRecoveryChance(
                    sourceCell.RecoveryCountN,
                    recoveryRateX);
                float infectionChance = TransitionRules.GetInfectionChance(
                    sourceCell.InfectionCountM,
                    infectionDecayY);

                if (Random.value < infectionChance)
                {
                    List<GameCell> positiveNeighbors = GetPositiveNeighbors(sourceCell, stateSnapshot);
                    Shuffle(positiveNeighbors);
                    int targetCount = Mathf.Min(infectionTargetCount, positiveNeighbors.Count);
                    for (int i = 0; i < targetCount; i++)
                    {
                        cellsToInfect.Add(positiveNeighbors[i]);
                    }
                }

                if (Random.value < recoveryChance)
                {
                    cellsToRecover.Add(sourceCell);
                }
            }

            foreach (GameCell cell in cellsToInfect)
            {
                cell.SetState(State.Blank);
            }

            foreach (GameCell cell in cellsToRecover)
            {
                cell.SetState(State.Fill);
            }

            waveAudioController.PlayNextWaveStep();

            if (!HasNegativeCells())
            {
                waveActive = false;
                waveAudioController.StopWave();
            }
        }

        private List<GameCell> GetPositiveNeighbors(
            GameCell sourceCell,
            IReadOnlyDictionary<GameCell, State> stateSnapshot)
        {
            List<GameCell> neighbors = new List<GameCell>(8);

            for (int yOffset = -1; yOffset <= 1; yOffset++)
            {
                for (int xOffset = -1; xOffset <= 1; xOffset++)
                {
                    if (xOffset == 0 && yOffset == 0)
                    {
                        continue;
                    }

                    GameCell neighbor = GetCellAtLocation(
                        sourceCell.xCoOrd + xOffset,
                        sourceCell.yCoOrd + yOffset);
                    if (neighbor != null && stateSnapshot[neighbor] == State.Fill)
                    {
                        neighbors.Add(neighbor);
                    }
                }
            }

            return neighbors;
        }

        private static void Shuffle<T>(IList<T> items)
        {
            for (int i = items.Count - 1; i > 0; i--)
            {
                int swapIndex = Random.Range(0, i + 1);
                (items[i], items[swapIndex]) = (items[swapIndex], items[i]);
            }
        }

        private bool HasNegativeCells()
        {
            foreach (GameCell cell in gameCellList)
            {
                if (cell.GetCellState() == State.Blank)
                {
                    return true;
                }
            }

            return false;
        }

        private void OnApplicationQuit()
        {
            waveAudioController.StopWave();
            StopAllCoroutines();
            gameCellList.Clear();
            gameCellsByPosition.Clear();
        }

        public GameCell GetCellAtLocation(int x, int y)
        {
            gameCellsByPosition.TryGetValue(new Vector2Int(x, y), out GameCell cell);
            return cell;
        }

        public void BeginNegativeWave(GameCell sourceCell)
        {
            if (sourceCell == null)
            {
                return;
            }

            sourceCell.SetState(State.Blank, true, true);
            waveStep = 0;
            waveActive = true;
            waveAudioController.BeginWave(State.Blank);
        }

        public void AutoRunButtonPressed()
        {
            autoRun = !autoRun;
            if (autoRun)
            {
                StartAutoRun();
            }
            else
            {
                StopAutoRun();
            }

            UpdateDebugControls();
        }

        private void UpdateDebugControls()
        {
            if (autoRunText != null)
            {
                autoRunText.text = autoRun ? "AutoRun: ON" : "AutoRun: OFF";
            }

            if (stepButton != null)
            {
                stepButton.gameObject.SetActive(!autoRun);
            }
        }

        public void OnReset()
        {
            waveAudioController.StopWave();
            SceneManager.LoadScene(0);
        }

        public void NextStepButtonPressed()
        {
            StepNext();
        }
    }
}
