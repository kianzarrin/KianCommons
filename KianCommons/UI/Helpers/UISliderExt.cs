namespace KianCommons.UI {
    using ColossalFramework.UI;
    using UnityEngine;

    public class UISliderExt : UISlider {
        public override void Awake() {
            base.Awake();
        }

        public float Padding = 0; // contianer has padding
        public UISlicedSprite SlicedSprite;

        public override void Start() {
            base.Start();

            builtinKeyNavigation = true;
            isInteractive = true;
            color = Color.grey;
            name = GetType().Name;
            height = 15f;
            width = parent.width - 2 * Padding;

            maxValue = 100;
            minValue = 0;
            stepSize = 1;
            AlignTo(parent, UIAlignAnchor.TopLeft);

            //Log.Debug("parent:" + parent);
            SlicedSprite = AddUIComponent<UISlicedSprite>();
            SlicedSprite.spriteName = "ScrollbarTrack";
            SlicedSprite.height = 12;
            SlicedSprite.width = width;
            SlicedSprite.relativePosition = new Vector3(Padding, 2f);

            UISprite thumbSprite = AddUIComponent<UISprite>();
            thumbSprite.spriteName = "ScrollbarThumb";
            thumbSprite.height = 20f;
            thumbSprite.width = 7f;
            thumbObject = thumbSprite;
            thumbOffset = new Vector2(Padding, 0);
        }

        protected override void OnSizeChanged() {
            base.OnSizeChanged();
            if (SlicedSprite == null || SlicedSprite.parent == null)
                return;
            SlicedSprite.width = SlicedSprite.parent.width - 2 * Padding;
        }
    }
}
