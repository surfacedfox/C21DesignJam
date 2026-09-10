using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
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
        [SerializeField] private float stepTimer;
        [Header("Game Debug")]
        [SerializeField] private bool autoRun = true;
        [SerializeField] public State paintPickedState = State.Fill;
        [Header("Game Debug UI Button Refs")]
        [SerializeField] private TMP_Text autoRunText;
        [SerializeField] private Button stepButton;
        [SerializeField] private Image pickerColorImage;



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
            }
        }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
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
        }

        private void OnApplicationQuit()
        {
            gameCellList.Clear();
            StopAllCoroutines();
        }






        //Input Buttons
        public void AutoRunButtonPressed()
        {
            if (autoRun)
            {
                autoRun = false;
                autoRunText.text = "AutoRun: OFF";
                stepButton.gameObject.SetActive(true);
                StopCoroutine(GameStep());
            }
            else
            {
                autoRun = true;
                autoRunText.text = "AutoRun: ON";
                stepButton.gameObject.SetActive(false);
                StartCoroutine(GameStep());
            }
        }

        public void FillPicked()
        {
            paintPickedState = State.Fill;
            UpdatePickedColorUI();
        }
        public void BlankPicked()
        {
            paintPickedState = State.Blank;
            UpdatePickedColorUI();
        }
        void UpdatePickedColorUI()
        {
            pickerColorImage.color = paintPickedState==State.Fill?Color.hotPink : Color.cornflowerBlue;
        }
            


        public void NextStepButtonPressed()
        {
            StepNext();
        }

    }
}
