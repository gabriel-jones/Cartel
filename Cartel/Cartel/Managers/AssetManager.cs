using System;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
using System.Collections.Generic;

namespace Cartel.Managers {
	public enum AssetDirectory {
		GUI, HardObjects, Pawns, Plants, SoftObjects, Structures, Tiles, Floors
	}

	public class AssetManager {
		static ContentManager content;

		public static Dictionary<string, object> AssetCache;

		public static void SetContentManager(ContentManager content) {
			AssetManager.content = content;

			AssetCache = new Dictionary<string, object>();

			LoadContent<Texture2D>();
			LoadContent<SpriteFont>();
			LoadContent<SoundEffect>();
			LoadContent<Song>();
			LoadContent<Effect>();
		}

		private static String DirectoryForContent<T>(bool includeContentPath = false) {
			Type contentType = typeof(T);
			String subDirectory = null;

			if (contentType == typeof(Texture2D)) {
				subDirectory = "Textures";
			} else if (contentType == typeof(SpriteFont)) {
				subDirectory = "Fonts";
			} else if (contentType == typeof(SoundEffect)) {
				subDirectory = Path.Combine("Audio", "SoundEffects");
			} else if (contentType == typeof(Song)) {
				subDirectory = Path.Combine("Audio", "Songs");
			} else if (contentType == typeof(Effect)) {
				subDirectory = "Effects";
			}

			if (subDirectory == null) {
				return null;
			}

			if (includeContentPath) {
				return Path.Combine(content.RootDirectory, subDirectory);
			}

			return subDirectory;
		}

		private static void LoadContent<T>() {
			String root = DirectoryForContent<T>(true);
			if (root == null) {
				throw new TypeAccessException();
			}
			Console.WriteLine("loading " + root);
			DirectoryInfo directory = new DirectoryInfo(root);
			if (!directory.Exists) {
				throw new DirectoryNotFoundException();
			}

			LoadDirectoryFiles<T>(directory);
		}

		private static void LoadDirectoryFiles<T>(DirectoryInfo directory, string subDirectory = null) {
			FileInfo[] files = directory.GetFiles("*");
			foreach (FileInfo file in files) {
				string key = Path.GetFileNameWithoutExtension(file.Name);
				string contentDirectory = DirectoryForContent<T>();
				string assetPath = Path.Combine(contentDirectory, key);

				if (subDirectory != null) {
					assetPath = Path.Combine(contentDirectory, subDirectory, key);
				}

				T asset = content.Load<T>(assetPath);
				if (asset != null) {
					Console.WriteLine(key);
					AssetCache[key] = asset;
				}
			}

			foreach (DirectoryInfo subDirectoryInfo in directory.GetDirectories()) {
				LoadDirectoryFiles<T>(subDirectoryInfo, subDirectoryInfo.Name);
			}
		}

		public static T GetDefault<T>() {
			Type contentType = typeof(T);
			if (contentType == typeof(Texture2D)) {
				return (T)AssetCache["tx_default"];
			} else if (contentType == typeof(SpriteFont)) {
				return (T)AssetCache["font_default"];
			} else if (contentType == typeof(SoundEffect)) {
				return (T)AssetCache["sound_default"];
			} else if (contentType == typeof(Song)) {
				return (T)AssetCache["song_default"];
			}
			return default(T);
		}

		public static T GetAsset<T>(String name) {
			if (AssetCache.ContainsKey(name)) {
				return (T)AssetCache[name];
			}

			return GetDefault<T>();
		}
	}
}
