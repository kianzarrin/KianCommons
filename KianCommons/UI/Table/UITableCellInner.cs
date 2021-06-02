using ColossalFramework.UI;
using System;
using UnityEngine;

namespace KianCommons.UI.Table {
    class UITableCellInner : UIPanel {

        public override void Awake() {
            base.Awake();
            autoLayout = true;
            autoSize = true;
            autoFitChildrenHorizontally = true;
            autoFitChildrenVertically = true;
            padding = new RectOffset(4, 4, 4, 4);
            name = GetType().Name;
            atlas = TextureUtil.Ingame;
            eventSizeChanged += (_, _) => {
                try { // the first size change occurs within UITableCellOuter::Activate, at which point UITableCellOuter.parent isn't a UITableRow yet.
                    var outerCell = parent as UITableCellOuter;
                    var row = outerCell.parent as UITableRow;
                    var table = row.parent as UITable;
                    table.ResizeColumn(outerCell.columnIndex);
                }catch(NullReferenceException _) {

                }
            };
        }
    }
}
