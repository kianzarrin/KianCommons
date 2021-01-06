using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ColossalFramework.UI;
using UnityEngine;
using KianCommons;

namespace KianCommons.UI {
    internal static class UIExtensions {
        public static T AddUIComponent<T>(this UIView view) where T: UIComponent {
            return view.AddUIComponent(typeof(T)) as T;
        }
    }
}
