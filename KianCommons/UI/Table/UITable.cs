using ColossalFramework.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KianCommons.UI.Table {
    class UITable : UIPanel {
        public UITableRow[] rows = new UITableRow[0];
        private float[] columnWidths = new float[0];
        public override void Awake() {
            base.Awake();
            autoLayout = true;
            autoSize = true;
            autoFitChildrenHorizontally = true;
            autoFitChildrenVertically = true;
            autoLayoutDirection = LayoutDirection.Vertical;
            autoLayoutStart = LayoutStart.TopLeft;
            name = GetType().Name;
            atlas = TextureUtil.Ingame;
        }

        public void Expand(int rowCount, int columnCount) {
            columnWidths = columnWidths.Expand(columnCount, (_) => -1f);
            rows = rows.Expand(rowCount, (rowIndex) => {
                var row = AddUIComponent<UITableRow>();
                row.rowIndex = rowIndex;
                return row;
            });
            foreach (var row in rows) {
                row.Expand(columnWidths.Length);
            }
        }
        public void Shrink(int rowCount, int columnCount) {
            var reducedRowCount = rowCount != rows.Length;
            columnWidths = columnWidths.Shrink(columnCount, (_,_)=> { });
            rows = rows.Shrink(rowCount, (row, rowIndex) => {
                RemoveUIComponent(row);
                Destroy(row?.gameObject);
            });
            foreach(var row in rows) {
                row.Shrink(columnCount);
            }
            if (reducedRowCount) {
                ResizeAllColums();
            }
        }
        public UITableCellInner GetCell(int row, int column) {
            return GetOuterCell(row, column).innerCell;
        }
        public UITableCellOuter GetOuterCell(int row, int column) {
            return rows[row].cells[column];
        }
        public void ResizeAllColums() {
            
        }
        public void ResizeColumn(int columnIndex) {
            float maxWidth = 0;
            for(int rowIndex = 0; rowIndex < rows.Length; rowIndex++) {
                var innerCell = GetCell(rowIndex, columnIndex);
                if(innerCell.width > maxWidth) {
                    maxWidth = innerCell.width;
                }
            }
            if(columnWidths[columnIndex] != maxWidth) {
                columnWidths[columnIndex] = maxWidth;
                for(int rowIndex = 0; rowIndex<rows.Length; rowIndex++) {
                    var outerCell = GetOuterCell(rowIndex, columnIndex);
                    outerCell.width = maxWidth;
                }
            }
        }
        public override void Start() {
            base.Start();
        }
    }
}
