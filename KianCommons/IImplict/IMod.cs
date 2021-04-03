namespace UnifiedUILib.KianCommons.KianCommons.IImplict {
    using ICities;

    internal interface IMod : IUserMod {
        void OnEnabled();
        void OnDisabled();
    }

    internal interface IModWithSettings : IUserMod {
        void OnSettingsUI(UIHelper helper);
    }

}
