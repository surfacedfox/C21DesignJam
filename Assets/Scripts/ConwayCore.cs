using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using Unity.VisualScripting;
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
        [Header("Game Setup")]
        [SerializeField] private int numGridSize;
        [SerializeField] public float stepTimer;
        [SerializeField] private int coolDownSteps = 10;
        [SerializeField] public Color goodColor;
        [SerializeField] public Color badColor;
        [Header("Game Debug")]
        [SerializeField] private bool autoRun = true;
        [SerializeField] public State paintPickedState = State.Fill;
        [Header("Game Debug UI Button Refs")]
        [SerializeField] private TMP_Text autoRunText;
        [SerializeField] private Button stepButton;
        [SerializeField] private Image pickerColorImage;



        public int coolDownTimer;


        //private vars for setup
        private List<GameCell> gameCellList = new List<GameCell>();

        //Singleton boilerplate
        private void Awake()
        {
            // If there is an instance, and it's not me, delete myself.

            if (Instance != null && Instance != this)
            {
                Destroy(this);
            }
            else
            {
                Instance = this;

                int cellCount = numGridSize * numGridSize;
                DOTween.SetTweensCapacity(
                    Mathf.Max(200, cellCount * 5),
                    Mathf.Max(50, cellCount * 2));
            }
        }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            coolDownTimer = 0;
            gridLG.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, numGridSize * gridLG.GetComponent<GridLayoutGroup>().cellSize.x);
            gridLG.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, numGridSize * gridLG.GetComponent<GridLayoutGroup>().cellSize.y);
            for (int i = 0; i < (numGridSize * numGridSize); i++)
            {
                var newCell = GameObject.Instantiate(gameCellPrefab, gridLG.GetComponent<RectTransform>()).GetComponent<GameCell>();
                newCell.xCoOrd = i % numGridSize;
                newCell.yCoOrd = i / numGridSize;
                gameCellList.Add(newCell);
            }


            if (autoRun)
            {
                //Finally, start the sim
                StartCoroutine(GameStep());
            }
        }

        IEnumerator GameStep()
        {
            yield return new WaitForSeconds(stepTimer);
            StepNext();
            if (autoRun)
            {
                StartCoroutine(GameStep());
            }
            else
            {
                StopCoroutine(GameStep());
            }
        }

        void StepNext()
        {
            foreach (var cell in gameCellList)
            {
                cell.UpdateState();
                cell.PaintCell();
            }
            coolDownTimer--;
        }

        private void OnApplicationQuit()
        {
            gameCellList.Clear();
            StopAllCoroutines();
        }

        public GameCell GetCellAtLocation(int x, int y)
        {
            foreach(var cell in gameCellList)
            {
                if(cell.xCoOrd == x && cell.yCoOrd == y)
                    return cell;
            }
            return null;
        }




        //Input Buttons
        public void AutoRunButtonPressed()
        {
            if (autoRun)
            {
                autoRun = false;
                StopCoroutine(GameStep());
            }
            else
            {
                autoRun = true;
                StartCoroutine(GameStep());
            }
        }

        public void FillPicked()
        {
            ConwayCore.Instance.coolDownTimer = 10;
            paintPickedState = State.Fill;
        }
        public void BlankPicked()
        {
            ConwayCore.Instance.coolDownTimer = 10;
            paintPickedState = State.Blank;
        }
        void UpdatePickedColorUI()
        {
            pickerColorImage.color = paintPickedState==State.Fill?Color.hotPink : Color.cornflowerBlue;
        }

        public void OnReset()
        {
            SceneManager.LoadScene(0);
        }

        public void NextStepButtonPressed()
        {
            StepNext();
        }

    }
}
