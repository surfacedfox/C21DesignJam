using UnityEngine;
using UnityEngine.UI;
using ConwayGame;
using Unity.VisualScripting;
using DG.Tweening;

namespace ConwayGame
{
    public class GameCell : MonoBehaviour
    {


        public State cellState;

        public void UpdateState()
        {
            //RULES GO HERE, IF THIS.....THEN DO THIS
            if (cellState == State.Fill)
                cellState = State.Blank;
            else if (cellState == State.Blank)
                cellState = State.Fill;
        }

        public State GetCellState()
        {
            return cellState;
        }

        public void PaintCell()
        {
            switch (cellState)
            {
                case State.Fill:
                    GetComponent<Image>().DOColor(Color.softRed, 1.0f);
                    break;
                case State.Blank:
                    GetComponent<Image>().DOColor(Color.aliceBlue, 1.0f);
                    break;
                default:
                    GetComponent<Image>().color = Color.white;
                    break;
            }
        }



        //DEBUG
        public void OnCellClicked()
        {
            cellState = ConwayCore.Instance.paintPickedState;
            PaintCell();
        }
    }
}
