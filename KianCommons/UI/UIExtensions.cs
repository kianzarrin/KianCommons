using ColossalFramework.UI;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace KianCommons.UI {
    internal static class UIExtensions {
        public static void FitToScreen(this UIComponent target) {
            Log.Called("target=" + target);
            Vector2 resolution = target.GetUIView().GetScreenResolution();
            target.absolutePosition = new Vector2(
                Mathf.Clamp(target.absolutePosition.x, 0, resolution.x - target.width),
                Mathf.Clamp(target.absolutePosition.y, 0, resolution.y - target.height));
            Log.Info($"target.absolutePosition={target.absolutePosition}, resolution={resolution}");
        }

        public static T AddUIComponent<T>(this UIView view) where T : UIComponent {
            return view.AddUIComponent(typeof(T)) as T;
        }

        public static void DestroyFull(this UIComponent c) {
            c.SetAllDeclaredFieldsToNull();
            GameObject.Destroy(c.gameObject);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static bool FPSOptimisedIsVisble(this UIComponent c) => c.isVisible;
    }
}
