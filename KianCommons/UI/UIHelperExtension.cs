using ColossalFramework;
using ColossalFramework.UI;
using ICities;
using System;
using UnityEngine;

namespace KianCommons.UI {
    public static class UIHelperExtension {
        public static UICheckBox AddUpdatingCheckbox(
            this UIHelperBase helper, string label, Action<bool> SetValue, Func<bool> GetValue) {
            Log.Info($"option {label} is " + GetValue());
            var cb = helper.AddCheckbox(label, GetValue(), delegate (bool value) {
                try {
                    SetValue(value);
                    Log.Info($"option '{label}' is set to " + value);
                } catch (Exception ex) { ex.Log(); }
            }) as UICheckBox;
            cb.eventVisibilityChanged += (c, val) => (c as UICheckBox).isChecked = GetValue();
            return cb;

        }

        public static UICheckBox AddSavedToggle(this UIHelperBase helper, string label, SavedBool savedBool, Action<bool> OnToggled) {
            Log.Info($"option {label} is " + savedBool.value);
            return helper.AddCheckbox(label, savedBool, delegate (bool value) {
                try {
                    savedBool.value = value;
                    Log.Info($"option '{label}' is set to " + value);
                    OnToggled(value);
                } catch(Exception ex) { ex.Log(); }
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
            Assertion.NotNull(helper.self, "self");
            Assertion.Assert(helper.self is UIComponent, "self is " + helper.self.GetType().Name);
            var label = (helper.self as UIComponent).AddUIComponent<UILabel>();
            label.text = text;
            label.tooltip = tooltip;
            if (textColor.HasValue)
                label.textColor = textColor.Value;
            return label;
        }

        /// <param name="text">label will be text: tooltip</param>
        /// <param name="tooltip">set null for default tooltip</param>
        /// <param name="OnSubmit">Triggered when slider is released. Can be null</param>
        public static UISlider AddSavedSlider(
            this UIHelperBase helper,
            string text, Func<float, string> tooltip,
            SavedFloat savedFloat, float min, float max, float step,
            Action OnSubmit) {
            tooltip ??= (float val) => val.ToString();

            var slider = helper.AddSlider(
                text: text + ": " + tooltip(savedFloat.value),
                min, max, step, defaultValue: savedFloat.value,
                eventCallback: val => savedFloat.value = val) as UISlider;

            slider.eventValueChanged += (slider_, val) => {
                var label = slider_.parent.Find<UILabel>("Label");
                label.text = text + ": " + tooltip(savedFloat.value);
            };

            if(OnSubmit != null) slider.eventMouseUp += (_, val) => OnSubmit();

            return slider;
        }
    }
}
