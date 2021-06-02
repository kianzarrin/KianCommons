using ColossalFramework.UI;

namespace KianCommons.UI.Table {
    class UITableCellOuter : UIPanel {

        public UITableCellInner innerCell;
        public int rowIndex;
        public int columnIndex;
        public override void Awake() {
            base.Awake();
            autoLayout = true;
            autoFitChildrenHorizontally = false;
            autoFitChildrenVertically = true;
            autoLayoutDirection = LayoutDirection.Horizontal;
            autoLayoutStart = LayoutStart.TopLeft;
            name = GetType().Name;
            atlas = TextureUtil.Ingame;

            innerCell = AddUIComponent<UITableCellInner>();
        }
    }
}
