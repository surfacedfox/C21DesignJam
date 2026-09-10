using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ConwayCore : MonoBehaviour
{
    [Header ("References")]
    [SerializeField] private GameObject gameCellPrefab;
    [SerializeField] private GameObject gridLG;
    [Header ("Game Setup")]
    [SerializeField] private int numGridSize;
    [SerializeField] private float stepTimer;



    //private vars for setup
    private List<GameCell> gameCellList = new List<GameCell>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gridLG.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, numGridSize*gridLG.GetComponent<GridLayoutGroup>().cellSize.x);
        gridLG.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, numGridSize * gridLG.GetComponent<GridLayoutGroup>().cellSize.y);
        for (int i = 0; i < (numGridSize * numGridSize); i++)
        {
            var newCell = GameObject.Instantiate(gameCellPrefab, gridLG.GetComponent<RectTransform>()).GetComponent<GameCell>();
            gameCellList.Add(newCell);
        }



        //Finally, start the sim
        StartCoroutine(GameStep());
    }

    IEnumerator GameStep()
    {
        yield return new WaitForSeconds(stepTimer);
        foreach (var cell in gameCellList)
        {
            cell.UpdateState();
            cell.PaintCell();
        }
        StartCoroutine (GameStep());
    }

    private void OnApplicationQuit()
    {
        gameCellList.Clear();
        StopAllCoroutines();
    }
}
