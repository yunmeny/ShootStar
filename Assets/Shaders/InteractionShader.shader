Shader "Custom/SDFFieldInteraction"
{
    Properties
    {
        // 暴露可调整参数，方便在Inspector中修改
        _Position ("Position", Vector) = (0, 0, 0, 0)
        _CircleRadius ("Circle Radius", Range(0,1)) = 0.1
        _BoxSize ("Box Half Size (X/Y)", Vector) = (0.5, 0.25, 0, 0)
        _BoxRound ("Box Rounded Radius", Vector) = (0, 0, 0, 0)
        _Smoothness ("Smooth Intersection K", Range(0,1)) = 0.1
        _Color ("Tint Color", Color) = (1, 0, 0, 1)
    }

    SubShader
    {
        Tags
        {
            "RenderType"="Transparent" 
            "Queue"="Transparent"
            "IgnoreProjector"="True" // 忽略投影器，避免干扰
            "DisableBatching"="False"
        }
        LOD 100

        // 透明渲染关键设置
        ZWrite Off // 关闭深度写入（透明物体必需）
        Blend SrcAlpha OneMinusSrcAlpha // 开启Alpha混合

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            // 对应Properties的参数
            float _CircleRadius;
            float4 _Position;
            float2 _BoxSize;
            float4 _BoxRound;
            float _Smoothness;
            float4 _Color;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            // SDF工具函数（保留原逻辑）
            float sdCircle(float2 p, float r)
            {
                return length(p) - r;
            }

            float sdRoundedBox(float2 p, float2 b, float4 r)
            {
                r.xy = (p.x > 0.0) ? r.xy : r.zw;
                r.x = (p.y > 0.0) ? r.x : r.y;
                float2 q = abs(p) - b + r.x;
                return min(max(q.x, q.y), 0.0) + length(max(q, 0.0)) - r.x;
            }

            float SmoothUnion(float a, float b, float k)
            {
                float h = clamp(0.5 + 0.5 * (b - a) / k, 0.0, 1.0);
                return lerp(b, a, h) - k * h * (1.0 - h);
            }

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 uv1 = i.uv * 2.0 - 1.0; // 转换到-1~1范围
                float2 uv2 = i.uv * 2.0 - 1.0; // 复制一份
                uv1 += _Position; // 偏移到中心

                float distance1 = sdCircle(uv1, _CircleRadius);
                float distance2 = sdRoundedBox(uv2, _BoxSize, _BoxRound);

                // 计算平滑相交
                float interaction = SmoothUnion(distance1, distance2, _Smoothness);

                // 自适应平滑步长，避免锯齿
                float fw = fwidth(interaction);
                interaction = smoothstep(0.0, fw, -interaction);
                
                // 仅丢弃几乎完全透明的像素（保留平滑过渡）
                if (interaction < 0.01) discard;

                // 叠加颜色和透明度
                fixed4 col = _Color;
                col.a = interaction; // Alpha通道=相交强度
                return col;
            }
            ENDCG
        }
    }
    FallBack "Transparent/VertexLit" // 降级方案，提升兼容性
}