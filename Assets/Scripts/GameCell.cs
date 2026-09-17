using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace ConwayGame
{
    public class GameCell : MonoBehaviour
    {
        public int xCoOrd;
        public int yCoOrd;
        public State cellState;
        public State freezeState;
        public State? unappliedWaveState;
        public State? waveState = null;
        public int waveStepsRemaining = 0;

        public void AdvanceWave()
        {
            if (waveState == null) return;

            var northCell = ConwayCore.Instance.GetCellAtLocation(xCoOrd, yCoOrd + 1);
            var southCell = ConwayCore.Instance.GetCellAtLocation(xCoOrd, yCoOrd - 1);
            var eastCell = ConwayCore.Instance.GetCellAtLocation(xCoOrd + 1, yCoOrd);
            var westCell = ConwayCore.Instance.GetCellAtLocation(xCoOrd - 1, yCoOrd);

            if (northCell != null)
            {
                northCell.unappliedWaveState = this.waveState;
            }

            if (southCell != null)
            {
                southCell.unappliedWaveState = this.waveState;
            }

            if (eastCell != null)
            {
                eastCell.unappliedWaveState = this.waveState;
            }

            if (westCell != null)
            {
                westCell.unappliedWaveState = this.waveState;
            }
        }

        public void UpdateState(bool wasExtrinsic = false)
        {
            if (unappliedWaveState != null)
            {
                freezeState = cellState;
                waveState = unappliedWaveState;
                unappliedWaveState = null;

                waveStepsRemaining = Random.Range(2, 5);
            }

            if (waveState != null)
            {
                cellState = waveState.Value;

                waveStepsRemaining--;
                if (waveStepsRemaining == 1)
                {
                    waveState = null;
                    cellState = State.Fill;
                }

                return;
            }

            //gather neighbours
            var northCell = ConwayCore.Instance.GetCellAtLocation(xCoOrd, yCoOrd + 1);
            var southCell = ConwayCore.Instance.GetCellAtLocation(xCoOrd, yCoOrd - 1);
            var eastCell = ConwayCore.Instance.GetCellAtLocation(xCoOrd + 1, yCoOrd);
            var westCell = ConwayCore.Instance.GetCellAtLocation(xCoOrd - 1, yCoOrd);
            int goodScore = 0;
            int badScore = 0;
            //check for nulls before
            if (northCell != null)
            {
                if (northCell.cellState == State.Fill)
                {
                    goodScore++;
                }
                else
                {
                    badScore++;
                }
            }
            if (southCell != null)
            {
                if (southCell.cellState == State.Fill)
                {
                    goodScore++;
                }
                else
                {
                    badScore++;
                }
            }
            if (eastCell != null)
            {
                if (eastCell.cellState == State.Fill)
                {
                    goodScore++;
                }
                else
                {
                    badScore++;
                }
            }
            if (westCell != null)
            {
                if (westCell.cellState == State.Fill)
                {
                    goodScore++;
                }
                else
                {
                    badScore++;
                }
            }
//            if (ConwayCore.Instance.coolDownTimer > 0)
//            {
//                if (ConwayCore.Instance.paintPickedState == State.Blank)
//                {
//                    goodScore = goodScore * 6;
//                }
//                else
//                {
//                    badScore = badScore * 6;
//                }
//            }
            float score = goodScore * 17.0f - badScore * 10.0f;

            if (Random.Range(0.0f, 100.0f) >= score)
            {
                cellState = State.Fill;
            }
            else { cellState = State.Blank; }
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
                    GetComponent<Image>().DOColor(ConwayCore.Instance.goodColor, ConwayCore.Instance.stepTimer);
                    break;
                case State.Blank:
                    GetComponent<Image>().DOColor(ConwayCore.Instance.badColor, ConwayCore.Instance.stepTimer);
                    break;
                default:
                    GetComponent<Image>().color = Color.white;
                    break;
            }
        }



        //DEBUG
        public void OnCellClicked()
        {
            waveState = ConwayCore.Instance.paintPickedState;
            waveStepsRemaining = Random.Range(3, 5);
            freezeState = cellState;
            cellState = ConwayCore.Instance.paintPickedState;
            PaintCell();
        }
    }
}
