using Urho;
using Urho.Samples;

namespace Test
{
    public class Main : Sample
    {
        public Main(ApplicationOptions options = null) : base(options) { }

        public static Main Instance;
        public static StateManager StateManager = new StateManager();

        protected override void Start()
        {
            base.Start();
            Instance = this;

            Graphics.WindowTitle = "Wormi";
            StateManager.Add(new Wormi_Menu());

            Input.SetMouseVisible(true, false);
        }

        protected override void OnUpdate(float timeStep)
        {
            base.OnUpdate(timeStep);

            StateManager.Update(timeStep);
        }

    }

}
