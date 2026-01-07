Shader "SDF/CircleSDF"
{
    Properties
    {
        _CircleColor ("Circle Color", Color) = (1, 1, 1, 1)
        _BackgroundColor ("Background Color", Color) = (0, 0, 0, 0)
        _Radius ("Radius", Range(0, 1)) = 0.5
        _Softness ("Softness", Range(0, 0.1)) = 0.01
    }
    
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "UnityCG.cginc"
            
            struct appdata
            {
                uint id : SV_VertexID;
            };
            
            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };
            
            float4 _CircleColor;
            float4 _BackgroundColor;
            float _Radius;
            float _Softness;
            
            // 距离场函数：圆形
            void sdCircle_float(float2 p, float r, out float d)
            {
                d = length(p) - r;
            }
            
            v2f vert (appdata v)
            {
                v2f o;
                
                // 根据顶点ID生成全屏四边形的四个顶点
                float2 uv = float2((v.id << 1) & 2, v.id & 2);
                float4 pos = float4(uv * 2.0f - 1.0f, 0.0f, 1.0f);
                
                o.vertex = UnityObjectToClipPos(pos);
                o.uv = uv;
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                // 将UV坐标转换为中心在原点的坐标
                float2 p = i.uv * 2.0f - 1.0f;
                
                // 调用距离场函数
                float d;
                sdCircle_float(p, _Radius, d);
                
                // 计算平滑过渡
                float ddxy = fwidth(d);
                float negate = -d;
                float alpha = smoothstep(0, ddxy, negate);

                
                // 混合颜色
                fixed4 col = lerp(_BackgroundColor, _CircleColor, alpha);
                
                return col;
            }
            ENDCG
        }
    }
}