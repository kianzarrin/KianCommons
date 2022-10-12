namespace KianCommons {
    using HarmonyLib;
    using System.Reflection;
    using System.Linq;
    using System;

    internal static class RefChainExtension {
        public static RefChain ToRefChain(this Traverse traverse) => RefChain.Create(traverse.GetValue());
    }

    /// <summary>
    /// like traverse but capable of setting original struct values
    /// </summary>
    public class RefChain {
        object root_;
        MemberInfo []infoChain_;
        public override string ToString() => $"root:{root_.ToSTR()} refchain={infoChain_.ToSTR()}";

        public static RefChain Create(object root) => new RefChain(root);

        public RefChain Clone() {
            return new RefChain(root_) {
                infoChain_ = infoChain_.ToArray(),
            };
        }

        RefChain(object root) {
            Assertion.NotNull(root, "root");
            Assertion.Assert(root.GetType().IsClass, "root must be class");
            this.root_ = root;
            this.infoChain_ = new MemberInfo[0];
        }

        public Type GetValueType() {
            if(infoChain_.Length == 0) {
                return root_.GetType();
            } else {
                MemberInfo info = infoChain_[infoChain_.Length - 1];
                if (info is FieldInfo fieldInfo) {
                    return fieldInfo.FieldType;
                } else if (info is PropertyInfo propertyInfo) {
                    return propertyInfo.PropertyType;
                } else {
                    throw new InvalidOperationException("info:" + info);
                }
            }
        }

        private RefChain Member(MemberInfo info) {
            var ret = Clone();
            ret.infoChain_ = ret.infoChain_.AddToArray(info);
            return ret;
        }
        
        public RefChain Field(FieldInfo fieldInfo) => Member(fieldInfo);
        public RefChain Property(PropertyInfo propertyInfo) => Member(propertyInfo);

        public RefChain Field(string fieldName) {
            FieldInfo fieldInfo = ReflectionHelpers.GetField(GetValueType(), fieldName, throwOnError:true);
            return Field(fieldInfo);
        }

        public RefChain Property(string propertyName) {
            Type type = GetValueType();
            PropertyInfo propertyInfo = type.GetProperty(propertyName, ReflectionHelpers.ALL)
                ?? throw new Exception($"{type}.{propertyName} not found");
            return Property(propertyInfo);
        }

        public RefChain<T> Field<T>(FieldInfo fieldInfo) => new RefChain<T>(Field(fieldInfo));
        public RefChain<T> Property<T>(PropertyInfo propertyInfo) => new RefChain<T>(Property(propertyInfo));
        public RefChain<T> Field<T>(string fieldName) => new RefChain<T>(Field(fieldName));
        public RefChain<T> Property<T>(string propertyName) => new RefChain<T>(Property(propertyName));


        #region get set values
        private static object GetValue(object root, MemberInfo info) {
            if (info is FieldInfo fieldInfo) {
                return fieldInfo.GetValue(root);
            } else if (info is PropertyInfo propertyInfo) {
                return propertyInfo.GetValue(root, null);
            } else {
                throw new InvalidOperationException("info:" + info);
            }
        }

        private static void SetValue(object root, MemberInfo info, object value) {
            if (info is FieldInfo fieldInfo) {
                fieldInfo.SetValue(root, value);
            } else if (info is PropertyInfo propertyInfo) {
                propertyInfo.SetValue(root, value, null);
            } else {
                throw new InvalidOperationException("info:" + info);
            }
        }

        public object GetValue() {
            object ret = root_;
            foreach (var info in infoChain_) {
                Assertion.NotNull(ret, "value");
                ret = GetValue(ret, info);
            }
            return ret;
        }

        public T GetValue<T>() {
            object value = GetValue();
            if (value == null) {
                return default(T);
            }
            return (T)value;
        }

        public void SetValue(object value) {
            SetValue(root: root_, infoIndex:0, value: value);
        }

        
        private void SetValue(object root, int infoIndex, object value) {
            Assertion.NotNull(root, "root");
            if (infoIndex < infoChain_.Length - 1) {
                object root2 = GetValue(root, infoChain_[infoIndex]);
                SetValue(
                    root: root2,
                    infoIndex: infoIndex + 1,
                    value: value);
                value = root2;
            }

            SetValue(root: root, info: infoChain_[infoIndex], value: value);
        }

        #endregion
    }

    public class RefChain<T> {
        private readonly RefChain refchain_;
        private RefChain() { }

        /// <summary>Gets/Sets the current value</summary>
        /// <value>The value to read or write</value>
        public T Value {
            get => refchain_.GetValue<T>();
            set => refchain_.SetValue(value);
        }

        public RefChain(RefChain refchain) {
            Assertion.Assert(typeof(T) == refchain.GetValueType(), $"T:{typeof(T)} refchain:{refchain}");
            this.refchain_ = refchain;
        }
    }
}
