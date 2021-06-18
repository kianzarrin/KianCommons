using ColossalFramework;
using ColossalFramework.UI;
using ICities;
using KianCommons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace KianCommons.UI {
    public static class UIHelperExtension {

        public static UICheckBox AddSavedToggle(this UIHelperBase helper, string label, SavedBool savedBool, Action<bool> OnToggled) {
            Log.Debug($"option {label} is set to " + savedBool.value);
            return helper.AddCheckbox(label, savedBool, delegate (bool value) {
                savedBool.value = value;
                Log.Debug($"option {label} is set to " + value);
                OnToggled(value);
            }) as UICheckBox;
        }
        public static UICheckBox AddSavedToggle(this UIHelperBase helper, string label, SavedBool savedBool, Action OnToggled) {
            return helper.AddSavedToggle(label, savedBool, (_) => OnToggled());
        }
        public static UICheckBox AddSavedToggle(this UIHelperBase helper, string label, SavedBool savedBool) {
            return helper.AddSavedToggle(label, savedBool, (_) => { });
        }

        public static UITextField AddSavedClampedIntTextfield(this UIHelperBase helper, string label, SavedInt savedInt, int min, int max, Action<int> OnSubmit) {
            UITextField field = null;
            field = helper.AddTextfield(label, savedInt.value.ToString(), (_) => { }, (string value) => {
                if (Int32.TryParse(value, out int newValue)) {
                    newValue = Mathf.Clamp(newValue, min, max);
                    if (newValue != savedInt.value) {
                        savedInt.value = newValue;
                        Log.Debug($"option {label} is set to " + savedInt.value);
                        OnSubmit(newValue);
                    }
                } else {
                    if (field is not null) field.text = savedInt.value.ToString();
                }
            }) as UITextField;
            field.numericalOnly = true;
            field.allowFloats = false;
            field.allowNegative = min < 0;
            Log.Debug($"option {label} is set to " + savedInt.value);
            return field;
        }
        public static UITextField AddSavedClampedIntTextfield(this UIHelperBase helper, string label, SavedInt savedInt, int min, int max, Action OnSubmit) {
            return helper.AddSavedClampedIntTextfield(label, savedInt, min, max, (_) => OnSubmit());
        }
        public static UITextField AddSavedClampedIntTextfield(this UIHelperBase helper, string label, SavedInt savedInt, int min, int max) {
            return helper.AddSavedClampedIntTextfield(label, savedInt, min, max, (_) => { });
        }

        public static UILabel AddLabel(this UIHelper helper, string text, string tooltip = null, Color32 ?textColor = null) {
            var label = (helper.self as UIComponent).AddUIComponent<UILabel>();
            label.text = text;
            label.tooltip = tooltip;
            if (textColor.HasValue)
                label.textColor = textColor.Value;
            return label;
        }
    }
}
