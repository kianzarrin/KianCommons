namespace KianCommons {
    using ICities;
    using KianCommons.IImplict;
    using System;
    using System.Diagnostics;
    using UnityEngine.SceneManagement;

    public abstract class LifeCycleBase : ILoadingExtension, IModWithSettings {
        public static LifeCycleBase Instance { get; private set; }
        internal LifeCycleBase() => Instance = this;
        public static string Scene => SceneManager.GetActiveScene().name;


        #region MOD
        public static Version ModVersion => typeof(LifeCycleBase).Assembly.GetName().Version;
#if DEBUG
        public string Name => ModName + " Beta V" + ModVersion.ToString(3);
#else
        public string Name => ModName + " V" + ModVersion.ToString(1);
#endif
        public abstract string ModName { get; }
        public abstract string Description { get; }
        public void OnEnabled() {
            try {
                Log.Debug("Testing StackTrace:\n" + new StackTrace(true).ToString(), copyToGameLog: false);
                Log.VERBOSE = false;

                Start();
                if(!Helpers.InStartupMenu)
                    HotReload();
            } catch(Exception ex) { Log.Exception(ex); }
        }

        public void OnDisabled() {
            try {
                UnLoad();
                End();
                Log.Flush();
            } catch(Exception ex) { Log.Exception(ex); }
        }

        public abstract void Start();
        public abstract void End();
        public abstract void OnSettingsUI(UIHelper helper);
        #endregion


        #region LoadingExtension
        public static SimulationManager.UpdateMode UpdateMode => SimulationManager.instance.m_metaData.m_updateMode;
        public static LoadMode Mode => (LoadMode)UpdateMode;

        public void OnCreated(ILoading _) { }
        public void OnReleased() { }
        public virtual void OnLevelLoaded(LoadMode _) {
            try {
                Load();
            } catch(Exception ex) { Log.Exception(ex); }
        }
        public virtual void OnLevelUnloading() {
            try {
                UnLoad();
            } catch(Exception ex) { Log.Exception(ex); }
        }

#endregion
        public virtual void HotReload() => Load();
        public abstract void Load();
        public abstract void UnLoad();
    }
}
