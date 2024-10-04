using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System;
using System.IO;

[CanEditMultipleObjects]
[CustomEditor(typeof(SaveAllSprites), true)]
public class SaveSpritesEditor : Editor
{
	public override void OnInspectorGUI ()
	{
		SaveAllSprites sas = (target as SaveAllSprites);
		GUILayout.BeginHorizontal();
		GUILayout.Label("File Prefix:");
		GUILayout.Space(20f);
		sas.withPrefix = GUILayout.TextField(sas.withPrefix);
		if(GUILayout.Button ("save all"))
		{
			UIAtlas atlas = sas.atlas;
			string basePath = EditorUtility.SaveFolderPanel("Save As",
			                                                "", atlas.name);
			if(basePath.Length > 0)
			{
				basePath+=("/"+sas.withPrefix);
				foreach(UISpriteData sprite in atlas.spriteList)
				{
					string path = basePath + sprite.name + ".png";
					#if UNITY_3_5
					string path = EditorUtility.SaveFilePanel("Save As",
					                                          NGUISettings.currentPath, sprite.name + ".png", "png");
					#else

					#endif
					
					if (!string.IsNullOrEmpty(path))
					{
						NGUISettings.currentPath = System.IO.Path.GetDirectoryName(path);
						UIAtlasMaker.SpriteEntry se = UIAtlasMaker.ExtractSprite(atlas, sprite.name);
						
						if (se != null)
						{
							byte[] bytes = se.tex.EncodeToPNG();
							File.WriteAllBytes(path, bytes);
							AssetDatabase.ImportAsset(path);
							if (se.temporaryTexture) DestroyImmediate(se.tex);
						}
					}
				}
			}
		}
	}
}
