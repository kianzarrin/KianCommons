namespace KianCommons.UI.Helpers {
    using ColossalFramework.UI;
    using UnityEngine;

    public class UIAutoPanel : UIPanel{
        private int spacing_ = 3;
        public int Spacing {
            get => spacing_;
            set {
                spacing_ = value;
                padding = autoLayoutPadding = new RectOffset(Spacing, Spacing, Spacing, Spacing);
            }
        }
        public override void Awake() {
            base.Awake();
            name = GetType().Name;
            autoLayout = true;
            autoLayoutDirection = LayoutDirection.Vertical;
            autoFitChildrenHorizontally = true;
            autoFitChildrenVertically = true;
            autoLayoutPadding = new RectOffset(Spacing, Spacing, Spacing, Spacing);
            padding = new RectOffset(Spacing, Spacing, Spacing, Spacing);
            eventFitChildren += OnAutoFit;

            atlas = TextureUtil.Ingame;
        }

        private void OnAutoFit() {
            size += new Vector2(Spacing, Spacing);
        }
    }
}
