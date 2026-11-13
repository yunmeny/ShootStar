#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TextCore;

public static class MouseButtonIconBuilder
{
    private const string Folder = "Assets/Images/Tiles White/";
    private const string TexturePath = Folder + "mouse.png";
    private const string AssetPath = Folder + "mouse.asset";
    private const string ScenePath = "Assets/Scenes/GamePlay.unity";
    private static readonly string[] Names = { "px_mouse_left", "px_mouse_right", "px_mouse_middle" };

    [MenuItem("Tools/Add Mouse Button Icons")]
    public static void Build()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            throw new InvalidOperationException("Exit Play mode before adding mouse icons.");
        var keyboard = AssetDatabase.LoadAssetAtPath<TMP_SpriteAsset>(Folder + "px.asset");
        if (!keyboard || keyboard != TMP_Settings.defaultSpriteAsset)
            throw new InvalidOperationException("The expected default keyboard sprite asset is missing.");

        WriteTexture();
        AssetDatabase.ImportAsset(TexturePath, ImportAssetOptions.ForceSynchronousImport);
        var importer = (TextureImporter)AssetImporter.GetAtPath(TexturePath);
        importer.textureType = TextureImporterType.Default;
        importer.filterMode = FilterMode.Point;
        importer.wrapMode = TextureWrapMode.Clamp;
        importer.mipmapEnabled = false;
        importer.alphaIsTransparency = true;
        importer.npotScale = TextureImporterNPOTScale.None;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.SaveAndReimport();

        var mouse = AssetDatabase.LoadAssetAtPath<TMP_SpriteAsset>(AssetPath);
        if (!mouse)
        {
            mouse = ScriptableObject.CreateInstance<TMP_SpriteAsset>();
            mouse.name = "mouse";
            AssetDatabase.CreateAsset(mouse, AssetPath);
        }
        var serialized = new SerializedObject(mouse);
        serialized.FindProperty("m_Version").stringValue = "1.1.0";
        serialized.ApplyModifiedPropertiesWithoutUndo();
        mouse.spriteSheet = AssetDatabase.LoadAssetAtPath<Texture2D>(TexturePath);
        if (!mouse.material)
        {
            mouse.material = new Material(Shader.Find("TextMeshPro/Sprite")) { name = "mouse Material" };
            AssetDatabase.AddObjectToAsset(mouse.material, mouse);
        }
        mouse.material.mainTexture = mouse.spriteSheet;
        mouse.spriteGlyphTable.Clear();
        mouse.spriteCharacterTable.Clear();
        mouse.fallbackSpriteAssets = new List<TMP_SpriteAsset>();
        for (int i = 0; i < Names.Length; i++)
        {
            var glyph = new TMP_SpriteGlyph((uint)i, new GlyphMetrics(15, 16, 0, 14, 15),
                new GlyphRect(1 + i * 20, 1, 15, 16), 1, 0);
            mouse.spriteGlyphTable.Add(glyph);
            mouse.spriteCharacterTable.Add(new TMP_SpriteCharacter((uint)(0xE100 + i), mouse, glyph)
            {
                name = Names[i], scale = 1
            });
        }
        mouse.UpdateLookupTables();
        if (keyboard.fallbackSpriteAssets == null) keyboard.fallbackSpriteAssets = new List<TMP_SpriteAsset>();
        if (!keyboard.fallbackSpriteAssets.Contains(mouse)) keyboard.fallbackSpriteAssets.Add(mouse);
        EditorUtility.SetDirty(mouse.material);
        EditorUtility.SetDirty(mouse);
        EditorUtility.SetDirty(keyboard);
        AssetDatabase.SaveAssets();
        AssetDatabase.ImportAsset(AssetPath, ImportAssetOptions.ForceSynchronousImport);
        mouse = AssetDatabase.LoadAssetAtPath<TMP_SpriteAsset>(AssetPath);
        foreach (var character in mouse.spriteCharacterTable)
        {
            var resolved = TMP_SpriteAsset.SearchForSpriteByHashCode(keyboard, character.hashCode, true, out int index);
            if (resolved != mouse || index < 0)
                throw new Exception("Mouse icon lookup failed: " + character.name);
        }
        int hints = UpdateHints(mouse);
        Debug.Log("MOUSE_ICONS_PASS: 3 icons; default TMP fallback lookup and " + hints + " gameplay hints verified.");
        Selection.activeObject = mouse;
    }

    private static void WriteTexture()
    {
        var texture = new Texture2D(64, 32, TextureFormat.RGBA32, false);
        try
        {
            texture.SetPixels32(new Color32[64 * 32]);
            for (int button = 0; button < 3; button++)
            {
                int originX = 1 + button * 20;
                for (int row = 1; row <= 14; row++)
                {
                    int edge = row == 1 || row == 14 ? 5 : row == 2 || row == 13 ? 3 : 2;
                    for (int column = edge; column <= 14 - edge; column++)
                    {
                        bool outline = row == 1 || row == 14 || column == edge || column == 14 - edge;
                        bool separator = row == 7 || column == 7 && row >= 2 && row <= 6;
                        bool pressed = row >= 3 && row <= 6 &&
                            (button == 0 && column >= 3 && column <= 6 ||
                             button == 1 && column >= 8 && column <= 11 ||
                             button == 2 && column >= 6 && column <= 8);
                        if (outline || separator || pressed)
                            texture.SetPixel(originX + column, 1 + 15 - row, Color.white);
                    }
                }
            }
            texture.Apply();
            File.WriteAllBytes(TexturePath, texture.EncodeToPNG());
        }
        finally { UnityEngine.Object.DestroyImmediate(texture); }
    }

    private static int UpdateHints(TMP_SpriteAsset mouse)
    {
        var scene = SceneManager.GetSceneByPath(ScenePath);
        bool opened = !scene.isLoaded;
        if (opened) scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
        int count = 0;
        try
        {
            foreach (var root in scene.GetRootGameObjects())
                foreach (var label in root.GetComponentsInChildren<TMP_Text>(true))
                {
                    string replacement = label.text
                        .Replace("[鼠标左键]", "<sprite name=px_mouse_left>")
                        .Replace("[鼠标右键]", "<sprite name=px_mouse_right>")
                        .Replace("【鼠标中健】", "<sprite name=px_mouse_middle>")
                        .Replace("【鼠标中键】", "<sprite name=px_mouse_middle>")
                        .Replace("[鼠标中键]", "<sprite name=px_mouse_middle>");
                    if (replacement != label.text)
                    {
                        Undo.RecordObject(label, "Add mouse button icons");
                        label.text = replacement;
                        EditorUtility.SetDirty(label);
                    }
                    if (!replacement.Contains("<sprite name=px_mouse_")) continue;
                    label.ForceMeshUpdate(true, true);
                    bool found = false;
                    for (int i = 0; i < label.textInfo.characterCount; i++)
                        if (label.textInfo.characterInfo[i].elementType == TMP_TextElementType.Sprite &&
                            label.textInfo.characterInfo[i].spriteAsset == mouse) found = true;
                    if (!found) throw new Exception("Mouse icon failed to render: " + label.name);
                    count++;
                }
            if (count != 3) throw new Exception("Expected three gameplay mouse hints, found " + count);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }
        finally { if (opened) EditorSceneManager.CloseScene(scene, true); }
        return count;
    }
}
#endif
