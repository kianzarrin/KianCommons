using ColossalFramework.UI;
using UnityEngine;
using System.IO;
using System.Reflection;
using System;

namespace KianCommons.UI {
    using static HelpersExtensions;
    public static class TextureUtil {
        static string PATH => typeof(TextureUtil).Assembly.GetName().Name + ".Resources.";
        static string ModPath => PluginUtil.GetPlugin().modPath;
        public static string FILE_PATH = Path.Combine(ModPath, "Resources");

        public static UITextureAtlas CreateTextureAtlas(
            string textureFile, string atlasName, int spriteWidth, int spriteHeight, string[] spriteNames)
        {
            Texture2D texture2D = LoadTextureFromAssembly( textureFile, spriteWidth * spriteNames.Length, spriteHeight);
            UITextureAtlas uitextureAtlas = InitializeAtalas(atlasName);

            for (int i = 0; i < spriteNames.Length; i++) {
                UITextureAtlas.SpriteInfo spriteInfo = new UITextureAtlas.SpriteInfo {
                    name = spriteNames[i],
                    texture = texture2D,
                    region = new Rect(i / (float)spriteNames.Length, 0f, spriteNames.Length, 1f)
                };
                uitextureAtlas.AddSprite(spriteInfo);
            }
            return uitextureAtlas;
        }

        public static UITextureAtlas InitializeAtalas(string name) {
            UITextureAtlas ret = ScriptableObject.CreateInstance<UITextureAtlas>();
            Assert(ret != null, "uitextureAtlas");
            Material material = UnityEngine.Object.Instantiate<Material>(UIView.GetAView().defaultAtlas.material);
            Assert(material != null, "material");
            ret.material = material;
            ret.name = name;
            return ret;
        }

        public static void AddTexturesInAtlas(UITextureAtlas atlas, Texture2D[] newTextures, bool locked = false) {
            Texture2D[] textures = new Texture2D[atlas.count + newTextures.Length];

            for (int i = 0; i < atlas.count; i++) {
                Texture2D texture2D = atlas.sprites[i].texture;

                if (locked) {
                    // Locked textures workaround
                    RenderTexture renderTexture = RenderTexture.GetTemporary(texture2D.width, texture2D.height, 0);
                    Graphics.Blit(texture2D, renderTexture);

                    RenderTexture active = RenderTexture.active;
                    texture2D = new Texture2D(renderTexture.width, renderTexture.height);
                    RenderTexture.active = renderTexture;
                    texture2D.ReadPixels(new Rect(0f, 0f, (float)renderTexture.width, (float)renderTexture.height), 0, 0);
                    texture2D.Apply();
                    RenderTexture.active = active;

                    RenderTexture.ReleaseTemporary(renderTexture);
                }

                textures[i] = texture2D;
                textures[i].name = atlas.sprites[i].name;
            }

            for (int i = 0; i < newTextures.Length; i++)
                textures[atlas.count + i] = newTextures[i];

            Rect[] regions = atlas.texture.PackTextures(textures, atlas.padding, 4096, false);

            atlas.sprites.Clear();

            for (int i = 0; i < textures.Length; i++) {
                UITextureAtlas.SpriteInfo spriteInfo = atlas[textures[i].name];
                atlas.sprites.Add(new UITextureAtlas.SpriteInfo {
                    texture = textures[i],
                    name = textures[i].name,
                    border = (spriteInfo != null) ? spriteInfo.border : new RectOffset(),
                    region = regions[i]
                });
            }

            atlas.RebuildIndexes();
        }

        public static UITextureAtlas GetAtlas(string name) {
            UITextureAtlas[] atlases = Resources.FindObjectsOfTypeAll(typeof(UITextureAtlas)) as UITextureAtlas[];
            for (int i = 0; i < atlases.Length; i++) {
                if (atlases[i].name == name)
                    return atlases[i];
            }
            return UIView.GetAView().defaultAtlas;
        }

        #region loading textures

        public static Stream GetFileStream(string file) {
            string path = Path.Combine(FILE_PATH, file);
            return File.OpenRead(path) ?? throw new Exception(path + "not find");
        }

        public static Texture2D GetTextureFromFile(string file) {
            using (Stream stream = GetFileStream(file))
                return GetTextureFromStream(stream);
        }

        // todo merge with GetTextureFromAssemblyManifest
        [Obsolete("use GetTextureFromAssemblyManifest instead")]
        private static Texture2D LoadTextureFromAssembly(string textureFile, int width, int height)
            => GetTextureFromAssemblyManifest(textureFile);

        public static Stream GetManifestResourceStream(string file) {
            string path = string.Concat(PATH, file);
            return Assembly.GetExecutingAssembly().GetManifestResourceStream(path)
                ?? throw new Exception(path + "not find");
        }

        // useful to load cursor textures.
        public static Texture2D GetTextureFromAssemblyManifest(string file) {
            using (Stream stream = GetManifestResourceStream(file))
                return GetTextureFromStream(stream);
        }

        public static Texture2D GetTextureFromStream(Stream stream) {
            Texture2D texture2D = new Texture2D(1, 1, TextureFormat.ARGB32, false);
            byte[] array = new byte[stream.Length];
            stream.Read(array, 0, array.Length);
            texture2D.filterMode = FilterMode.Bilinear;
            texture2D.LoadImage(array);
            texture2D.wrapMode = TextureWrapMode.Clamp; // for cursor.
            texture2D.Apply(true,true);
            return texture2D;
        }

        #endregion
    }
}
