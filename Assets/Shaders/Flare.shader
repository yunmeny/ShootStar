Shader "Custom/Flare"
{
    Properties {}
    SubShader
    {
        Tags
        {
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


            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float3 c;
                float l = 0, time = _Time.y;
                for (int ii = 0; ii < 3; ii++)
                {
                    float2 uv_com = i.uv;
                    float2 p = i.uv;
                    p -= 0.5;
                    time += 0.07;
                    l = length(p);
                    uv_com += p / l * (sin(time) + 1.0) * abs(sin(l * 9.0 - time*2));
                    c[ii] = 0.01 / length(uv_com-1.0*floor(uv_com/1.0) - 0.5);
                }
                float3 res = c / l;
                res = smoothstep(0.1, 1.0, res);
                return float4(res, 1);
            }
            ENDCG
        }
    }
}