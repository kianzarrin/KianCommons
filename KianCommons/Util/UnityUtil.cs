namespace KianCommons {
    using UnityEngine;

    public static class UnityUtil {
        public static T CreateComponent<T>(bool dontDestroyOnLoad) where T : Component =>
            CreateComponent<T>(dontDestroyOnLoad, out GameObject _);

        public static T CreateComponent<T>(bool dontDestroyOnLoad, out GameObject go) where T : Component {
            go = new GameObject(nameof(T), typeof(T));
            if(dontDestroyOnLoad)
                Object.DontDestroyOnLoad(go);
            return go.GetComponent<T>() as T;
        }

        ///<summary>
        /// live guard:
        /// returns null if o is null or not alive.
        /// use this instead of o?.member to make sure object is alive
        /// </summary>
        public static T E<T>(T o) where T : Object => o ? o : null;
        public static T Alive<T>(this T o) where T : Object => o ? o : null;
    }
}
