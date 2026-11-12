using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.U2D;
using UnityEngine;
using UnityEngine.U2D;

/// <summary>将 SpriteAtlas 当前构建平台的打包纹理导出为 PNG。</summary>
public static class SpriteAtlasTextureExporter
{
    private const string MenuPath = "Assets/图集工具/导出整张图集为 PNG";

    [MenuItem(MenuPath, true)]
    private static bool CanExport()
    {
        return Selection.activeObject is SpriteAtlas && !EditorApplication.isPlayingOrWillChangePlaymode;
    }

    [MenuItem(MenuPath)]
    private static void ExportSelected()
    {
        var atlas = Selection.activeObject as SpriteAtlas;
        if (atlas == null)
            return;

        string folder = EditorUtility.OpenFolderPanel("选择图集纹理输出目录", Application.dataPath, "");
        if (string.IsNullOrEmpty(folder))
            return;

        try
        {
            int count = Export(atlas, folder);
            AssetDatabase.Refresh();
            Debug.Log($"图集 {atlas.name} 已导出 {count} 张 PNG：{folder}", atlas);
            EditorUtility.RevealInFinder(folder);
        }
        catch (Exception exception)
        {
            AssetDatabase.Refresh();
            Debug.LogException(exception, atlas);
            EditorUtility.DisplayDialog("图集导出失败", exception.GetBaseException().Message, "确定");
        }
    }

    /// <summary>
    /// 导出所有主纹理页，不含独立 Alpha 页及 Secondary Textures。
    /// 不修改图集的导入设置；文件重名时自动添加编号。
    /// </summary>
    public static int Export(SpriteAtlas atlas, string outputDirectory)
    {
        if (atlas == null)
            throw new ArgumentNullException(nameof(atlas));
        if (string.IsNullOrWhiteSpace(outputDirectory))
            throw new ArgumentException("输出目录不能为空。", nameof(outputDirectory));

        // Unity 的预览接口为内部 API，通过反射读取以避免编译依赖。
        MethodInfo getPreviewTextures = typeof(SpriteAtlasExtensions).GetMethod(
            "GetPreviewTextures", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic,
            null, new[] { typeof(SpriteAtlas) }, null);
        if (getPreviewTextures == null)
            throw new NotSupportedException("当前 Unity 版本不支持读取 SpriteAtlas 预览纹理。");

        SpriteAtlasUtility.PackAtlases(new[] { atlas }, EditorUserBuildSettings.activeBuildTarget);
        var pages = getPreviewTextures.Invoke(null, new object[] { atlas }) as Texture2D[];
        if (pages == null || pages.Length == 0)
            throw new InvalidOperationException("图集没有生成纹理，请检查 Objects for Packing 和当前平台设置。");

        Directory.CreateDirectory(outputDirectory);
        string fileName = atlas.name;
        foreach (char invalid in Path.GetInvalidFileNameChars())
            fileName = fileName.Replace(invalid, '_');

        for (int i = 0; i < pages.Length; i++)
        {
            if (pages[i] == null)
                throw new InvalidOperationException($"图集第 {i + 1} 页纹理不可用。");

            string suffix = pages.Length > 1 ? $"_{i}" : "";
            string path = Path.Combine(outputDirectory, fileName + suffix + ".png");
            byte[] png = EncodeToPng(pages[i]);
            // CreateNew 同时保护已有文件与尚未导入 Unity 的文件。
            for (int duplicate = 1; ; duplicate++)
            {
                if (File.Exists(path))
                {
                    path = Path.Combine(outputDirectory, $"{fileName}{suffix} ({duplicate}).png");
                    continue;
                }

                using (var stream = new FileStream(path, FileMode.CreateNew, FileAccess.Write))
                    stream.Write(png, 0, png.Length);
                break;
            }
        }

        return pages.Length;
    }

    private static byte[] EncodeToPng(Texture2D source)
    {
        RenderTexture previousTarget = RenderTexture.active;
        bool previousSrgbWrite = GL.sRGBWrite;
        RenderTexture temporary = null;
        Texture2D readable = null;
        try
        {
            bool srgb = source.isDataSRGB;
            temporary = RenderTexture.GetTemporary(source.width, source.height, 0,
                RenderTextureFormat.ARGB32, srgb ? RenderTextureReadWrite.sRGB : RenderTextureReadWrite.Linear);
            GL.sRGBWrite = srgb && QualitySettings.activeColorSpace == ColorSpace.Linear;
            Graphics.Blit(source, temporary);
            RenderTexture.active = temporary;
            readable = new Texture2D(source.width, source.height, TextureFormat.RGBA32, false, !srgb);
            readable.ReadPixels(new Rect(0, 0, source.width, source.height), 0, 0, false);
            readable.Apply(false, false);
            return readable.EncodeToPNG();
        }
        finally
        {
            RenderTexture.active = previousTarget;
            GL.sRGBWrite = previousSrgbWrite;
            if (temporary != null)
                RenderTexture.ReleaseTemporary(temporary);
            if (readable != null)
                UnityEngine.Object.DestroyImmediate(readable);
        }
    }
}
