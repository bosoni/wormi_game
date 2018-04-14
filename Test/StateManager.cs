using System.Collections.Generic;
using Urho;

namespace Test
{
    public class StateManager
    {
        List<BaseState> states = new List<BaseState>();
        bool changed = false;

        public void Add(BaseState state)
        {
            state.Create();
            states.Add(state);
            changed = true;
        }

        public void Remove(BaseState state)
        {
            state.Dispose();
            states.Remove(state);
            changed = true;
        }

        public void RemoveAll()
        {
            foreach (BaseState bs in states)
                bs.Dispose();
            states.Clear();
            changed = true;
        }

        public void Update(float timeStep)
        {
            changed = false;
            for (int q = 0; q < states.Count; q++)
            {
                if (changed)
                {
                    q = 0;
                    continue; // aloita uudelleen alusta
                }
                states[q].OnUpdate(timeStep);
            }
        }
    }

    public class BaseState
    {
        protected bool drawDebug = false;
        protected Camera camera;
        protected Scene scene;

        protected float Yaw { get; set; }
        protected float Pitch { get; set; }
        protected bool TouchEnabled { get; set; }
        protected Node CameraNode { get; set; }

        public virtual void Create() { }
        public virtual void OnUpdate(float timeStep) { }
        public virtual void Dispose() { }

        public void SimpleMoveCamera3D(float timeStep, float moveSpeed = 10.0f)
        {
            const float mouseSensitivity = .1f;

            if (Main.Instance.UI.FocusElement != null)
                return;

            var mouseMove = Main.Instance.Input.MouseMove;
            Yaw += mouseSensitivity * mouseMove.X;
            Pitch += mouseSensitivity * mouseMove.Y;
            Pitch = MathHelper.Clamp(Pitch, -90, 90);

            CameraNode.Rotation = new Quaternion(Pitch, Yaw, 0);

            Input input = Main.Instance.Input;
            if (input.GetKeyDown(Key.W)) CameraNode.Translate(Vector3.UnitZ * moveSpeed * timeStep);
            if (input.GetKeyDown(Key.S)) CameraNode.Translate(-Vector3.UnitZ * moveSpeed * timeStep);
            if (input.GetKeyDown(Key.A)) CameraNode.Translate(-Vector3.UnitX * moveSpeed * timeStep);
            if (input.GetKeyDown(Key.D)) CameraNode.Translate(Vector3.UnitX * moveSpeed * timeStep);
        }

    }
}
