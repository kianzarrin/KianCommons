namespace UnifiedUILib.KianCommons.KianCommons.IImplict {
    public interface IUpdatableObject {
        void Update();
    }
    public interface ILateUpdatableObject {
        void LateUpdate();
    }
    public interface IGUIObject {
        void OnGUI();
    }

    public interface IDestroyableObject {
        void OnDestroy();
    }

    public interface IAwakingObject {
        void Awake();
    }
    public interface IStartingObject {
        void Start();
    }
}
