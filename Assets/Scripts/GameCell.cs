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

        [Header("Transition State")]
        [SerializeField, Min(0)] private int recoveryCountN;
        [SerializeField, Min(0)] private int infectionCountM;

        [Header("State Visuals")]
        [SerializeField] private Image stateImage;
        [SerializeField] private Sprite positiveSprite;
        [SerializeField] private Sprite negativeSprite;
        [SerializeField] private Color positiveColor = new Color32(246, 200, 95, 255);
        [SerializeField] private Color negativeColor = new Color32(240, 113, 120, 255);
        [SerializeField, Min(0f)] private float transitionDuration = 0.2f;
        [SerializeField, Range(0.8f, 1f)] private float transitionScale = 0.9f;

        private Tween visualTween;
        private State renderedState;
        private bool hasRenderedState;

        public int RecoveryCountN => recoveryCountN;
        public int InfectionCountM => infectionCountM;

        private void Awake()
        {
            if (stateImage == null)
            {
                Button button = GetComponentInChildren<Button>();
                stateImage = button != null ? button.targetGraphic as Image : GetComponent<Image>();
            }

            RenderStateVisual(cellState, false);
        }

        public State GetCellState()
        {
            return cellState;
        }

        public void SetState(State nextState, bool animate = true, bool restartNegativeCounters = false)
        {
            bool stateChanged = nextState != cellState;

            if (!stateChanged)
            {
                if (nextState == State.Blank && restartNegativeCounters)
                {
                    ResetTransitionCounters();
                }

                if (!hasRenderedState)
                {
                    RenderStateVisual(cellState, false);
                }

                return;
            }

            cellState = nextState;
            ResetTransitionCounters();
            RenderStateVisual(cellState, animate);
        }

        public void AdvanceNegativeCounters()
        {
            if (cellState != State.Blank)
            {
                return;
            }

            recoveryCountN++;
            infectionCountM++;
        }

        private void ResetTransitionCounters()
        {
            recoveryCountN = 0;
            infectionCountM = 0;
        }

        private void RenderStateVisual(State state, bool animate)
        {
            if (stateImage == null)
            {
                return;
            }

            Sprite targetSprite = state == State.Fill ? positiveSprite : negativeSprite;
            Color targetColor = GetStateColor(state);
            targetColor.a = 1f;
            Color hiddenTargetColor = targetColor;
            hiddenTargetColor.a = 0f;

            visualTween?.Kill();
            stateImage.DOKill();
            stateImage.rectTransform.DOKill();

            if (!animate || transitionDuration <= 0f)
            {
                ApplyVisualState(targetSprite, state);
                return;
            }

            float fadeOutDuration = transitionDuration * 0.4f;
            float fadeInDuration = transitionDuration - fadeOutDuration;

            renderedState = state;
            hasRenderedState = true;

            Sequence sequence = DOTween.Sequence();
            sequence.Append(stateImage.DOColor(hiddenTargetColor, fadeOutDuration).SetEase(Ease.OutQuad));
            sequence.Join(stateImage.rectTransform.DOScale(transitionScale, fadeOutDuration).SetEase(Ease.OutQuad));
            sequence.AppendCallback(() =>
            {
                stateImage.sprite = targetSprite;
            });
            sequence.Append(stateImage.DOColor(targetColor, fadeInDuration).SetEase(Ease.OutQuad));
            sequence.Join(stateImage.rectTransform.DOScale(1f, fadeInDuration).SetEase(Ease.OutQuad));
            sequence.OnComplete(() => visualTween = null);

            visualTween = sequence;
        }

        private void ApplyVisualState(Sprite sprite, State state)
        {
            stateImage.sprite = sprite;
            stateImage.color = GetStateColor(state);
            stateImage.rectTransform.localScale = Vector3.one;
            renderedState = state;
            hasRenderedState = true;
            visualTween = null;
        }

        private Color GetStateColor(State state)
        {
            return state == State.Fill ? positiveColor : negativeColor;
        }

        private void OnDisable()
        {
            visualTween?.Kill();

            if (stateImage != null)
            {
                ApplyVisualState(cellState == State.Fill ? positiveSprite : negativeSprite, cellState);
            }
        }

        public void OnCellClicked()
        {
            ConwayCore.Instance.BeginNegativeWave(this);
        }
    }
}
