using Urho.Resources;
using Urho.IO;
using Urho;

namespace Test
{
    public class Globals
    {
        public static bool CastShadows = true;

        public static string ReadTxt(string fileName)
        {
            File file = Main.Instance.ResourceCache.GetFile(fileName, false);
            int len = (int)file.Size;
            byte[] byteArray = new byte[len];
            file.Read(byteArray, (uint)len);
            file.Close();

            string str = "";
            for (int q = 0; q < len; q++)
            {
                if (byteArray[q] == '\n' || byteArray[q] == '\r')
                {
                    byteArray[q] = 0;
                    break;
                }
                str += (char)byteArray[q];
            }

            return str;
        }

        public static void PrintNodeHierarchy(Node node)
        {
            if (node == null) return;

            WriteLine("NodeName:" + node.Name);
            WriteLine("   Position:" + node.Position.ToString());
            WriteLine("   WPosition:" + node.WorldPosition.ToString());
            var children = node.Children;
            for (int i = 0; i < children.Count; i++)
            {
                Node nnode = children[i];
                PrintNodeHierarchy(nnode);
            }
        }

        public static void HideBoundingAreas(Node node)
        {
            if (node == null) return;

            // jos node on joku collision type, piilota se
            if (node.Name.Contains("_CB") ||
                node.Name.Contains("_CS") ||
                node.Name.Contains("_CS") ||
                node.Name.Contains("_CM"))
            {
                StaticModel m = node.GetComponent<StaticModel>();
                if (m != null)
                {
                    m.GetMaterial().SetTechnique(0, Main.Instance.ResourceCache.GetTechnique("Techniques/NoTextureUnlitAlpha.xml"), 0, 0);
                    m.GetMaterial().SetShaderParameter("MatDiffColor", new Color(0, 0, 0, 0));
                }
            }

            var children = node.Children;
            for (int i = 0; i < children.Count; i++)
            {
                Node nnode = children[i];
                HideBoundingAreas(nnode);
            }
        }

        public static void WriteLine(string str, bool error = false)
        {
            Log.WriteRaw(str, error);
            Log.WriteRaw("\n");
        }

        public static bool Raycast(Scene scene, Camera camera, float maxDistance, out Vector3 hitPos, out Drawable hitDrawable)
        {
            hitDrawable = null;
            hitPos = Vector3.Zero;

            var graphics = Main.Instance.Graphics;
            var input = Main.Instance.Input;

            IntVector2 pos = input.MousePosition;
            Ray cameraRay = camera.GetScreenRay((float)pos.X / graphics.Width, (float)pos.Y / graphics.Height);

            var result = scene.GetComponent<Octree>().RaycastSingle(cameraRay, RayQueryLevel.Triangle, maxDistance, DrawableFlags.Geometry, uint.MaxValue);
            if (result != null)
            {
                hitPos = result.Value.Position;
                hitDrawable = result.Value.Drawable;
                return true;
            }
            return false;
        }

    }
}
