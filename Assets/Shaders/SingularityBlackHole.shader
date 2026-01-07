Shader "Custom/SingularityBlackHole"
{
    Properties
    {
        // 预留属性（可扩展调整参数）
        _RotationSpeed("Rotation Speed", Float) = 1.0
        _Brightness("Brightness", Float) = 1.0
    }
    SubShader
    {
        Tags
        {
            "RenderType"="Opaque"
            "Queue"="Transparent"
        }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            // 对应Shadertoy的外部变量
            float _RotationSpeed;
            float _Brightness;

            // 顶点着色器输出（传递纹理坐标）
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

            void mainImage(out float4 O, float2 F, float2 iResolution, float iTime)
            {
                //迭代器和衰减（距离平方）
                float i = .2, a;

                //用于缩放和居中的分辨率
                float2 r = iResolution.xy;
                float2 uv = F / iResolution.xy;
                
                //居中的比例校正坐标
                float2 p = (F + F - r) / r.y / .7;

                
                //用于扭曲的对角线向量
                float2 d = float2(-1, 1);
                //黑洞中心
                float2 b = p - i * d;
                //用于着色的波纹累积总和
                float2 w;
                //旋转并应用透视
                // float2 c = mul(p, float2x2(1, 1, d / (.1 + i / dot(b, b))));
                float2 c = mul(float2x2(1, 1, d / (.1 + i / dot(b, b))), p);
                
                //旋转到螺旋坐标
                float2 v = mul(c, float2x2(cos(.5 * log(a = dot(c, c)) + iTime * i + float4(0, 33, 11, 0))) / i);
                // float2 v = mul(float2x2(cos(.5 * log(a = dot(c, c)) + iTime * i + float4(0, 33, 11, 0))) / i, c);
                
                //循环遍历波纹
                for (; i++ < 9.; w += 1. + sin(v))
                    //扭曲坐标
                    v += .7 * sin(v.yx * i + iTime) / i + .5;

                //吸积盘半径
                i = length(sin(v / .3) * .4 + c * (3. + d));
                //红/蓝渐变
                O = 1. - exp(-exp(c.x * float4(.6, -.4, -1, 0))
                    //波纹着色
                    / w.xyyx
                    //吸积盘亮度
                    / (2. + i * i / 4. - i)
                    //中心暗区
                    / (.5 + 1. / a)
                    //边缘高光
                    / (.03 + abs(length(p) - .7))
                );
            }

            // 顶点着色器（仅做坐标转换）
            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                // 传递UV并适配Shadertoy的坐标原点（Unity UV左上角，Shadertoy左下角，翻转Y轴）
                o.uv = float2(v.uv.x, 1.0 - v.uv.y);
                return o;
            }

            // 片段着色器（核心黑洞效果，适配Unity语法）
            fixed4 frag(v2f i) : SV_Target
            {
                float2 scale = float2(100, 100); // 缩放因子（对应Shadertoy的iResolution）
                float4 outColor = float4(0, 0, 0, 0);
                mainImage(outColor, i.uv * scale, scale, _Time.y * _RotationSpeed);
                return outColor;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}