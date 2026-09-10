using UnityEngine;
using UnityEngine.UI;
using ConwayGame;
using Unity.VisualScripting;
using DG.Tweening;

namespace ConwayGame
{
    public class GameCell : MonoBehaviour
    {
        public int xCoOrd;
        public int yCoOrd;
        public State cellState;
        public State prevState;

        public void UpdateState(bool wasExtrinsic = false)
        {
            //RULES GO HERE, IF THIS.....THEN DO THIS
            prevState = cellState;
            if(wasExtrinsic)
            {
                //do extrinsic magic wooowoo stuff
            }
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
                    GetComponent<Image>().DOColor(Color.hotPink, 1.0f);
                    break;
                case State.Blank:
                    GetComponent<Image>().DOColor(Color.cornflowerBlue, 1.0f);
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
