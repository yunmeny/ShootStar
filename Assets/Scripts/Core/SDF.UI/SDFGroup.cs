using UnityEngine;

namespace Core.SDF.UI
{
    public class SDFGroup : MonoBehaviour
    {
        public RenderTexture distanceField1;
        public RenderTexture distanceField2;
        public Material interactionMaterial;
        private int width = 256;
        private int height = 256;
    
        void Start()
        {
            // 初始化RenderTexture
            InitializeRenderTextures();
        }
    
        void InitializeRenderTextures()
        {
            distanceField1 = new RenderTexture(width, height, 0, RenderTextureFormat.ARGBFloat)
            {
                enableRandomWrite = true
            };
            distanceField1.Create();
        
            distanceField2 = new RenderTexture(width, height, 0, RenderTextureFormat.ARGBFloat)
            {
                enableRandomWrite = true
            };
            distanceField2.Create();
        }
    
        void Update()
        {
            // 设置材质参数
            interactionMaterial.SetTexture("_DistanceField1", distanceField1);
            interactionMaterial.SetTexture("_DistanceField2", distanceField2);
        }
    
        void OnDestroy()
        {
            // 释放资源
            distanceField1.Release();
            distanceField2.Release();
        }
    }
}