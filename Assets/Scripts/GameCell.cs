using UnityEngine;
using UnityEngine.UI;

public class GameCell : MonoBehaviour
{
    public enum State
    {
        Fill,
        Blank
    }

    public State cellState;
    
    public void UpdateState()
    {
        //RULES GO HERE, IF THIS.....THEN DO THIS
        if (cellState == State.Fill)
            cellState = State.Blank;
        else if (cellState  == State.Blank)
            cellState = State.Fill;
    }

    public State GetCellState()
    {
        return cellState;
    }

    public void PaintCell()
    {
        switch (cellState) {
            case State.Fill:
                GetComponent<Image>().color = Color.black;
                break;
            case State.Blank:
                GetComponent<Image>().color = Color.white;
                break;
            default:
                GetComponent<Image>().color = Color.white;
                break;
        }
    }
}
