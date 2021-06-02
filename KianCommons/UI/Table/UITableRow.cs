using ColossalFramework.UI;
using System;

namespace KianCommons.UI.Table {
    class UITableRow : UIPanel {
        public UITableCellOuter[] cells = new UITableCellOuter[0];
        public int rowIndex = -1;
        public override void Awake() {
            base.Awake();
            autoLayout = true;
            autoFitChildrenHorizontally = true;
            autoFitChildrenVertically = true;
            autoLayoutDirection = LayoutDirection.Horizontal;
            autoLayoutStart = LayoutStart.TopLeft;
            name = GetType().Name;
            atlas = TextureUtil.Ingame;
        }

        public void Expand(int columnCount) {
            cells = cells.Expand(columnCount, (columnIndex)=> {
                var cell = AddUIComponent<UITableCellOuter>();
                cell.columnIndex = columnIndex;
                cell.rowIndex = rowIndex;
                return cell;
            });
        }

        internal void Shrink(int columnCount) {
            cells = cells.Shrink(columnCount, (cell, columnIndex) => {
                RemoveUIComponent(cell);
                Destroy(cell?.gameObject);
            });
        }
    }
}
