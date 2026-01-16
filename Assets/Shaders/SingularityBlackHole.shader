Shader "Custom/SingularityBlackHole"
{
    Properties
    {
        _RotationSpeed("Rotation Speed", Float) = 1.0
    }
    SubShader
    {
        Tags
        {
            "RenderType"="Transparent"
            "Queue"="Transparent"
        }
        LOD 100
//        Blend OneMinusDstColor One
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            float _RotationSpeed;

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
                float iterator = 0.2;
                float2 uv_trans = (i.uv - 0.5) * 4;
                float2 diagonal = float2(-1,1);
                float2 o = uv_trans - iterator * diagonal;
                
                //用于着色的波纹累积总和
                float2 wave;
                //旋转并应用透视
                float2 uv_rotated = mul(float2x2(1, 1, diagonal / (0.1 + iterator / dot(o, o))), uv_trans);
                
                float a = dot(uv_rotated, uv_rotated);
                
                //旋转到螺旋坐标
                float2 uv_helix = mul(float2x2(cos(0.5 * log(a) + _Time.y * _RotationSpeed * iterator + float4(0, 33, 11, 0))) / iterator, uv_rotated);
                //循环遍历波纹
                for (; iterator++ < 9.0; wave += 1.0 + sin(uv_helix))
                    //扭曲坐标
                    uv_helix += 0.7 * sin(uv_helix.yx * iterator + _Time.y * _RotationSpeed) / iterator + 0.5;
                //吸积盘半径
                iterator = length(sin(uv_helix / 0.3) * 0.4 + uv_rotated * (3.0 + diagonal));
                //红/蓝渐变
                float4 out_color = 1.0 - exp(-exp(uv_rotated.x * float4(0.6, -0.4, -1, 0))
                    //波纹着色
                    / wave.xyyx
                    //吸积盘亮度
                    / (2.0 + iterator * iterator / 4.0 - iterator)
                    //中心暗区
                    / (0.5 + 1.0 / a)
                    //边缘高光
                    / (0.03 + abs(length(uv_trans) - 0.7))
                );
                return out_color;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}