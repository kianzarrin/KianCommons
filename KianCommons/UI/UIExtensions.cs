using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ColossalFramework.UI;
using UnityEngine;
using UnityEngine.UI;
using KianCommons;

namespace KianCommons.UI {
    internal static class UIExtensions {
        public static T AddUIComponent<T>(this UIView view) where T: UIComponent {
            return view.AddUIComponent(typeof(T)) as T;
        }

        public static void DestroyFull(this UIComponent c) {
            c.SetAllDeclaredFieldsToNull();
            GameObject.Destroy(c.gameObject);
        }
    }
}
