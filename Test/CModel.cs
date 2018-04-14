using Urho;

namespace Test
{
    public class CModel
    {
        Node node = null;
        StaticModel model = null;
        AnimationController animCtrl = null;

        public static CModel Load(Scene scene, string fileName, string materialName = "", bool castShadows = true)
        {
            CModel m = new CModel();
            m.node = scene.CreateChild();
            m.model = m.node.CreateComponent<StaticModel>();
            var cache = Main.Instance.ResourceCache;
            m.model.Model = cache.GetModel("Models/" + fileName);
            if (materialName != "")
                m.model.SetMaterial(cache.GetMaterial("Materials/" + materialName));
            m.model.CastShadows = castShadows;
            return m;
        }

        public static CModel LoadAnimated(Scene scene, string fileName, string materialName = "", bool castShadows = true)
        {
            CModel m = new CModel();
            m.node = scene.CreateChild();
            m.model = m.node.CreateComponent<AnimatedModel>();
            var cache = Main.Instance.ResourceCache;
            m.model.Model = cache.GetModel("Models/" + fileName);
            if (materialName != "")
                m.model.SetMaterial(cache.GetMaterial("Materials/" + materialName));

            m.animCtrl = new AnimationController();
            m.node.AddComponent(m.animCtrl);

            m.model.CastShadows = castShadows;
            return m;
        }

        public static CModel LoadPrefab(Scene scene, string fileName, bool castShadows = true)
        {
            CModel m = new CModel();

            Vector3 pos = new Vector3();
            Quaternion rot = new Quaternion();

            var cache = Main.Instance.ResourceCache;
            var xml = cache.GetXmlFile("Objects/" + fileName);
            m.node = scene.InstantiateXml(xml.GetRoot(), pos, rot);
            m.model = m.node.GetComponent<StaticModel>();
            if (m.model == null)
            {
                m.model = m.node.GetComponent<AnimatedModel>();
                if (m.model != null)
                {
                    m.animCtrl = m.node.GetComponent<AnimationController>();
                    if (m.animCtrl == null)
                    {
                        m.animCtrl = new AnimationController();
                        m.node.AddComponent(m.animCtrl);
                    }
                }
            }

            if (m.model == null)
                Globals.WriteLine("Prefab: can't load " + fileName, true);

            m.model.CastShadows = castShadows;
            return m;
        }

        public void Destroy()
        {
            if (node != null)
                node.Remove();
            if (model != null)
                model.Remove();
            if (animCtrl != null)
                animCtrl.Remove();

            animCtrl = null;
            node = null;
            model = null;
        }

        public Node GetNode()
        {
            return node;
        }

        public void SetTechnique(string name)
        {
            model.GetMaterial().SetTechnique(0, Main.Instance.ResourceCache.GetTechnique("Techniques/" + name), 0, 0);
        }

        public void SetMaterial(string name)
        {
            model.SetMaterial(Main.Instance.ResourceCache.GetMaterial("Materials/" + name));
        }

        // anim code ----------------------------------------

        public void LoadAnimation(string name)
        {
            if (animCtrl == null)
                return;

            var cache = Main.Instance.ResourceCache;
            Animation animation = cache.GetAnimation("Models/" + name);
            AnimationState state = ((AnimatedModel)model).AddAnimationState(animation);

            if (state != null)
            {
                state.Weight = 0;
                state.Looped = true;
            }

        }

        public void Play(string name)
        {
            if (animCtrl == null)
                return;

            animCtrl.PlayExclusive("Models/" + name, 0, true, 0.2f);
        }

        public void Stop(string name)
        {
            if (animCtrl == null)
                return;

            animCtrl.Stop("Models/" + name, 0.2f);
        }

        public void SetSpeed(string name, float speed)
        {
            if (animCtrl == null)
                return;

            animCtrl.SetSpeed(name, speed);
        }


        /*
        public void Update(uint animNum, float timeStep)
        {
            if (animCtrl == null)
                return;

            AnimationState state = ((AnimatedModel)model).GetAnimationState(animNum);
            if (state != null)
            {
                state.AddTime(timeStep);
            }
        }*/

    }
}
