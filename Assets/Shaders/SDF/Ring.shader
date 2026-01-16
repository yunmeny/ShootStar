Shader "Unlit/Ring"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags {
            "RenderType"="Transparent"
            "Queue"="Transparent"
        }
        LOD 100
        
        Blend OneMinusDstColor One
        ZWrite Off
        
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag


            #include "UnityCG.cginc"

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

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                i.uv = i.uv * 2 - 1;
                float l = length(i.uv) - 0.5;
                float c = 1/abs(30*l);
                // alpha 使用高次幂运算，在 Belend 为 SrcAlpha OneMinusSrcAlpha（正常透明混合） 时，能够抑制黑框，但是辉光效果弱
                // 可以使用 smoothstep 来平滑过渡，在OneMinusDstColor One（滤色）下，保留辉光效果
                c = smoothstep(0.05,1,c);
                float3 color = float3(c,c,c)*_Color.rgb;
                float4 res = float4(color,c);
                return res;
            }
            ENDCG
        }
    }
}
