// Upgrade NOTE: replaced 'samplerRect' with 'sampler2D'

Shader "SDfElement/Rect"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Scale ("Scale", Vector) = (1,1,0,0)
        _Radius ("Radius", Range(0, 1)) = 0.5

        _BlockColor ("BlockColor", Color) = (1,1,1,1)
        _Thickness ("Thickness", Range(0, 1)) = 0.01
        _OutlineColor ("OutlineColor", Color) = (1,1,1,1)
        _Inner("Inner", Range(0, 1)) = 0
        _InnerColor("InnerColor", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags {
            "RenderType"="Transparent"
            "Queue"="Transparent"
        }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha
        
        ZWrite Off
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            float4 _MainTex_ST;
            float _Radius;
            float2 _Scale;
            float _Thickness;
            float _Inner;
            float4 _BlockColor;
            float4 _OutlineColor;
            float4 _InnerColor;
            
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

            float sdRoundedBox(float2 p, float2 b, float4 r)
            {
                r.xy = p.x > 0.0 ? r.xy : r.zw;
                r.x = p.y > 0.0 ? r.x : r.y;
                float2 q = abs(p) - b + r.x;
                return min(max(q.x, q.y), 0.0) + length(max(q, 0.0)) - r.x;
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float rate = _MainTex_TexelSize.x/_MainTex_TexelSize.y;
                float2 uv = i.uv;
                uv =uv*_MainTex_ST.xy + _MainTex_ST.zw;
                float4 tex = tex2D(_MainTex, uv*float2(rate,1));
                i.uv = i.uv * 2 - 1;
                float4 radius = float4(_Radius, _Radius, _Radius, _Radius);
                float outline = sdRoundedBox(i.uv,_Scale,radius)-_Thickness;
                float fw = fwidth(outline);
                outline = smoothstep(0.0, fw, -outline);

                float inner = sdRoundedBox(i.uv,_Scale,radius) + _Inner;
                float fw_i = fwidth(inner);
                inner = smoothstep(0.0, fw_i, -inner);
                
                float block = sdRoundedBox(i.uv,_Scale,radius);
                float fw_b = fwidth(block);
                block = smoothstep(0.0, fw_b, -block);
                
                outline = outline - block;
                inner = block - inner;
                block = block - inner;
                
                float4 output = outline*_OutlineColor + block*_BlockColor*tex + inner*_InnerColor;
                return output;
            }
            ENDCG
        }
    }
}
