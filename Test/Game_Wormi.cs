/*
 matopeli "wormi"  by mjt, 2018

* mato sphere
* stickman,   idle ja juokseva,  näitä syödään
* floor plane ->  dirt, grass, stone  texturet  -> texture splatting/blending
* alkukuva
* partikkelit
* äänet

12.4.18 valmis

 */
using System;
using System.Collections.Generic;
using Urho;
using Urho.Audio;
using Urho.Gui;

namespace Test
{
    class WormPiece
    {
        public CModel mesh = null;
        public Vector3 pos, dir;
    };

    class Food
    {
        public CModel mesh = null;
        //public Vector3 pos, dir;
    };

    public class Wormi_Menu : BaseState
    {
        CModel bg;
        public override void Create()
        {
            scene = new Scene(Main.Instance.Context);
            scene.CreateComponent<Octree>();
            CameraNode = scene.CreateChild("Camera");
            camera = CameraNode.CreateComponent<Camera>();
            camera.FarClip = 1000;
            CameraNode.Position = new Vector3(0.0f, 2.0f, 0);
            CameraNode.LookAt(Vector3.Zero, Vector3.Up);
            Main.Instance.Renderer.SetViewport(0, new Viewport(Main.Instance.Context, scene, camera, null));

            bg = CModel.LoadPrefab(scene, "floor.xml");
            bg.SetMaterial("wormi_bg.xml");
            bg.SetTechnique("DiffUnlit.xml");

        }

        public override void OnUpdate(float timeStep)
        {
            bg.GetNode().Translate(new Vector3(0, timeStep * 0.08f, 0));

            if (bg.GetNode().WorldPosition.Y > 1.8f)
                bg.GetNode().SetWorldPosition(new Vector3(0, 0, 0));

            Input input = Main.Instance.Input;
            if (input.GetKeyDown(Key.Space))
            {
                scene.RemoveAllChildren();
                scene.Clear();

                Main.StateManager.Remove(this);
                Main.StateManager.Add(new Wormi());
            }
        }

    }

    public class Wormi : BaseState
    {
        List<Food> food = new List<Food>();
        List<WormPiece> wormi = new List<WormPiece>();

        int incLen = 0;
        float curSpeed, curTime = 0;

        bool gameOver = false;
        float gameOverTimer = 0;

        int score = 0;
        Text scoreText = null;

        Sound death, eating;
        Node soundNode;
        SoundSource soundSource;

        Node particleNode;
        ParticleEmitter emitter;

        float DIST = 0.5f;

        public override void Create()
        {
            food.Clear();
            wormi.Clear();

            incLen = 0;
            curSpeed = 0.1f;
            curTime = 0;

            // 3D scene with Octree
            scene = new Scene(Main.Instance.Context);
            scene.CreateComponent<Octree>();
            scene.CreateComponent<DebugRenderer>();

            soundNode = scene.CreateChild("Sound");
            eating = Main.Instance.ResourceCache.GetSound("Sounds/BigExplosion.wav");
            death = Main.Instance.ResourceCache.GetSound("Sounds/NutThrow.wav");
            soundSource = soundNode.CreateComponent<SoundSource>();

            // Create a directional light to the world. Enable cascaded shadows on it
            var lightNode = scene.CreateChild("DirectionalLight");
            lightNode.SetDirection(new Vector3(0.6f, -1.0f, 0.8f)); // The direction vector does not need to be normalized
            var light = lightNode.CreateComponent<Light>();
            light.LightType = LightType.Directional;
            light.CastShadows = true;
            light.ShadowBias = new BiasParameters(0.00025f, 0.5f);
            light.ShadowCascade = new CascadeParameters(10.0f, 50.0f, 200.0f, 0.0f, 0.8f);

            CameraNode = scene.CreateChild("Camera");
            camera = CameraNode.CreateComponent<Camera>();
            camera.FarClip = 1000;
            CameraNode.Position = new Vector3(0.0f, 35.0f, -35.0f);
            CameraNode.LookAt(Vector3.Zero, Vector3.Up);

            Main.Instance.Renderer.SetViewport(0, new Viewport(Main.Instance.Context, scene, camera, null));

            CModel floor = CModel.LoadPrefab(scene, "floor.xml");
            floor.GetNode().SetScale(50);
            floor.SetMaterial("Terrain.xml");

            for (int q = 0; q < 10; q++)
            {
                WormPiece p = new WormPiece();
                p.mesh = CModel.LoadPrefab(scene, "WormSphere.xml");
                p.mesh.GetNode().Position = new Vector3(-q * DIST, 0, 0);
                p.mesh.GetNode().Rotation = new Quaternion(0, 90, 0);
                //p.mesh.LoadAnimation("Move.ani");
                //p.mesh.LoadAnimation("Eat.ani");
                //p.mesh.Play("Move.ani");
                wormi.Add(p);
            }

            for (int q = 0; q < 20; q++)
                SpawnFood();

            particleNode = scene.CreateChild("Particles");
            emitter = particleNode.CreateComponent<ParticleEmitter>();

        }

        void SpawnFood()
        {
            int R = 15;
            Vector3 pp = wormi[0].mesh.GetNode().WorldPosition;
            Vector3 newpos;
            // etsi ruualle paikka joka ei ole ihan madon pään lähellä
            while (true)
            {
                newpos = new Vector3(Urho.Randoms.Next(-R, R), 0, Urho.Randoms.Next(-R, R));
                Vector3 v = pp - newpos;
                if (v.Length > 5)
                    break;
            }

            Food p = new Food();
            p.mesh = CModel.LoadPrefab(scene, "Ukko.xml");
            p.mesh.GetNode().Position = newpos;
            p.mesh.GetNode().Rotation = new Quaternion(0, Urho.Randoms.Next(0, 360), 0);
            p.mesh.LoadAnimation("Idle.ani");
            //p.mesh.LoadAnimation("Walk.ani");
            p.mesh.Play("Idle.ani");
            food.Add(p);
        }

        void RenderSplash(Matrix3x4 transform)
        {
            particleNode.SetTransform(transform);
            emitter.Effect = Main.Instance.ResourceCache.GetParticleEffect("Particle/Blood.xml");
            emitter.Emitting = true;
        }

        void GameOver()
        {
            scene.RemoveAllChildren();
            scene.Clear();
            score = 0;
            gameOver = false;

            Create();
        }

        void ShowScore()
        {
            if (scoreText == null)
            {
                scoreText = new Text()
                {
                    Value = ""
                    //, HorizontalAlignment = HorizontalAlignment.Center,
                    //VerticalAlignment = VerticalAlignment.Center
                };
                scoreText.SetColor(new Color(1f, 1f, 1f));
                scoreText.SetFont(font: Main.Instance.ResourceCache.GetFont("Fonts/Anonymous Pro.ttf"), size: 30);
                Main.Instance.UI.Root.AddChild(scoreText);
            }

            string POS = "";
#if DEBUG
            POS = wormi[0].mesh.GetNode().Position.ToString();

#endif


            scoreText.Value = " Score: " + score + "   " + POS;
        }

        public override void OnUpdate(float timeStep)
        {
            base.OnUpdate(timeStep);
            //SimpleMoveCamera3D(timeStep, 20);

            Input input = Main.Instance.Input;

            if (input.GetKeyDown(Key.Tab))
            {
                drawDebug = !drawDebug;
                if (drawDebug) Main.Instance.Renderer.DrawDebugGeometry(false);
            }

            if (Urho.Randoms.Next() < 0.01f)
                SpawnFood();

            if (gameOver)
            {
                if (input.GetKeyDown(Key.Space))
                    gameOverTimer = 60;

                gameOverTimer += timeStep;
                if (gameOverTimer >= 60)
                    GameOver();

                scoreText.Value = "\n     GAME OVER!  Score: " + score +
                    "\n\n        Press space to continue";

                return;
            }

            ShowScore();

            float ROTSPD = 150;
            if (input.GetKeyDown(Key.Left))
            {
                wormi[0].mesh.GetNode().Rotate(new Quaternion(0, -timeStep * ROTSPD, 0), TransformSpace.Local);
            }
            if (input.GetKeyDown(Key.Right))
            {
                wormi[0].mesh.GetNode().Rotate(new Quaternion(0, timeStep * ROTSPD, 0), TransformSpace.Local);
            }

            // laske että  missä kohtaa [0, 1] välillä curTime nyt on.
            curTime += timeStep;
            if (curTime < curSpeed)
            {
                float delta = curTime / curSpeed;

                for (int q = wormi.Count - 1; q > 0; q--)
                {
                    Vector3 newpos = wormi[q].pos + (wormi[q].dir * delta);
                    wormi[q].mesh.GetNode().SetWorldTransform(
                        newpos,
                        wormi[q].mesh.GetNode().Rotation);
                }
                return;
            }

            curTime = 0;

            Vector3 headPos = wormi[0].mesh.GetNode().WorldPosition;
            // tarkista syökö itteään
            for (int q = 10; q < wormi.Count; q++)
            {
                Vector3 pp = wormi[q].mesh.GetNode().WorldPosition;
                Vector3 v = headPos - pp;
                if (v.Length < 1)
                {
                    gameOver = true;
                    gameOverTimer = 0;
                    RenderSplash(wormi[0].mesh.GetNode().Transform);
                    soundSource.Play(death);
                    soundSource.Gain = 0.7f;
                }
            }

            foreach (Food e in food)
            {
                // tsekkaa onko pää jossain että syödään
                Vector3 pp = e.mesh.GetNode().WorldPosition;
                Vector3 v = headPos - pp;
                if (v.Length < 1.5f)
                {
                    RenderSplash(e.mesh.GetNode().Transform);

                    e.mesh.GetNode().Remove();
                    e.mesh = null;

                    if (curSpeed > 0.040)
                        curSpeed -= 0.004f;

                    score++;
                    incLen = 5;
                    //wormi[0].mesh.Play("Eat.ani");

                    soundSource.Play(eating);
                    soundSource.Gain = 0.7f;
                }
            }

            // poista syödyt
            int fds = 0;
            for (int q = 0; q < food.Count; q++)
            {
                if (food[q].mesh == null)
                {
                    food.RemoveAt(q);
                    q = 0;
                }
                else fds++;
            }
            if (fds == 0) // jos kaikki syöty, bonus ja lisää ruokaa
            {
                for (int q = 0; q < 10; q++)
                {
                    SpawnFood();
                }
                score += 100;
                soundSource.Play(death);
            }

            int newPieces = 0;
            if (incLen > 0)
            {
                incLen--;
                newPieces = 1;

                WormPiece p = new WormPiece();
                p.mesh = CModel.LoadPrefab(scene, "WormSphere.xml");
                p.mesh.GetNode().SetWorldTransform(
                    wormi[wormi.Count - 1].mesh.GetNode().WorldPosition,
                    wormi[wormi.Count - 1].mesh.GetNode().Rotation);
                wormi.Add(p);
            }
            else
            {
                //wormi[0].mesh.Play("Move.ani");
            }


            // siirrä eteenpäin kaikkia osia (paitsi uutta jos semmoinen on) hännästä päähän
            for (int q = wormi.Count - newPieces - 1; q > 0; q--)
            {
                wormi[q].mesh.GetNode().SetWorldTransform(
                    wormi[q - 1].mesh.GetNode().WorldPosition,
                    wormi[q - 1].mesh.GetNode().Rotation);
            }

            // laske päälle uus paikka
            wormi[0].mesh.GetNode().Translate(Vector3.Forward * DIST, TransformSpace.Local);

            Vector3 cpos = wormi[0].mesh.GetNode().WorldPosition;
            if (cpos.X < -30 || cpos.X > 30 ||
                cpos.Z < -22 || cpos.Z > 46)
            {
                gameOver = true;
                gameOverTimer = 0;
                RenderSplash(wormi[0].mesh.GetNode().Transform);
                soundSource.Play(death);
                soundSource.Gain = 0.7f;
            }

            // aseta paikka ja suunta seuraavaan palaan
            for (int q = wormi.Count /* - newPieces*/ - 1; q > 0; q--)
            {
                wormi[q].pos = wormi[q].mesh.GetNode().WorldPosition;
                wormi[q].dir = wormi[q - 1].mesh.GetNode().WorldPosition - wormi[q].mesh.GetNode().WorldPosition;
            }

        }
    }
}
