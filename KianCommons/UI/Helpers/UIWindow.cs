namespace KianCommons.UI.Helpers {
    using ColossalFramework.UI;
    using System;
    using UnityEngine;

    public class UIWindow : UIAutoPanel {
        protected UIDragHandle DragHandle;
        public UILabel Caption;
        public UIAutoPanel Container;

        protected UIPanel Wrapper => parent as UIPanel;

        private Vector2 position_;
        public virtual Vector2 Position {
            get => position_;
            set => position_ = value;
        }

        public static UIWindow Create(UIComponent parent = null) {
            var wrapper = parent?.AddUIComponent<UIPanel>() ??
                UIView.GetAView().AddUIComponent<UIPanel>();
            return wrapper.AddUIComponent<UIWindow>();
        }

        public override void OnDestroy() {
            this.SetAllDeclaredFieldsToNull();
            base.OnDestroy();
        }

        public override void Awake() {
            try {
                base.Awake();
                Log.Called();
                atlas = TextureUtil.Ingame;
                backgroundSprite = "MenuPanel2";
            } catch (Exception ex) { ex.Log(); }
        }

        public override void Start() {
            try {
                base.Start();
                Log.Called();
                absolutePosition = Position;

                DragHandle = AddUIComponent<UIDragHandle>();
                DragHandle.height = 20;
                DragHandle.relativePosition = Vector3.zero;
                DragHandle.target = this;
                DragHandle.width = width;
                DragHandle.height = 32;

                Caption = DragHandle.AddUIComponent<UILabel>();
                Caption.name = name + "_caption";
                Caption.anchor = UIAnchorStyle.CenterHorizontal | UIAnchorStyle.CenterVertical;

                Container = AddUIComponent<UIAutoPanel>();
                isVisible = true;
            } catch (Exception ex) { ex.Log(); }
        }


        protected override void OnPositionChanged() {
            Assertion.AssertStack();
            base.OnPositionChanged();
            Log.DebugWait("OnPositionChanged called", id: "OnPositionChanged called", seconds: 0.2f, copyToGameLog: false);

            Vector2 resolution = GetUIView().GetScreenResolution();

            Position = absolutePosition = new Vector2(
                Mathf.Clamp(absolutePosition.x, 0, resolution.x - width),
                Mathf.Clamp(absolutePosition.y, 0, resolution.y - height));

            Log.DebugWait("absolutePosition: " + absolutePosition, id: "absolutePosition: ", seconds: 0.2f, copyToGameLog: false);
        }
    }
}
