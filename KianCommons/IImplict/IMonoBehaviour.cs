namespace KianCommons.IImplict {
    internal interface IUpdatableObject {
        void Update();
    }
    internal interface ILateUpdatableObject {
        void LateUpdate();
    }
    internal interface IGUIObject {
        void OnGUI();
    }

    internal interface IDestroyableObject {
        void OnDestroy();
    }

    internal interface IAwakingObject {
        void Awake();
    }
    internal interface IStartingObject {
        void Start();
    }
}
