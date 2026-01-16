Shader "SDfElement/Circle"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Scale ("Scale", Vector) = (1,1,0,0) // 缩放（xy控制圆/椭圆大小，正圆建议保持xy相等）
        _Radius ("Circle Size Factor", Range(0, 1)) = 0.5 // 圆形大小系数（控制圆的半径强度）

        _BlockColor ("BlockColor", Color) = (1,1,1,1) // 圆形主体颜色
        _Thickness ("Thickness", Range(0, 1)) = 0.01 // 轮廓厚度
        _OutlineColor ("OutlineColor", Color) = (1,1,1,1) // 轮廓颜色
        _Inner("Inner", Range(0, 1)) = 0 // 内部镂空区域大小
        _InnerColor("InnerColor", Color) = (1,1,1,1) // 内部镂空区域颜色
    }
    SubShader
    {
        Tags {
            "RenderType"="Transparent"
            "Queue"="Transparent"
        }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha // 透明混合模式，与原Shader一致
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            sampler2D _MainTex;
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

            // 核心修改：圆形2D有符号距离场（SDF）函数
            // p: 归一化坐标（中心在原点）
            // r: 圆形有效半径（结合缩放和大小系数）
            float sdCircle(float2 p, float r)
            {
                // length(p)：计算坐标到原点的距离
                // 返回值：有符号距离（内部为负，外部为正，边界为0）
                return length(p) - r;
            }

            // 顶点着色器（与原Shader完全一致，无修改）
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex); // 模型空间转裁剪空间
                o.uv = v.uv; // 传递UV坐标
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // UV坐标映射：从[0,1]转换为[-1,1]，将纹理中心移到坐标原点
                i.uv = i.uv * 2 - 1;
                
                // 适配原缩放属性，计算圆形有效半径（正圆建议_Scale.xy值相等）
                // _Scale控制整体大小，_Radius控制大小系数，保证属性兼容性
                float circleRadius = min(_Scale.x, _Scale.y) * _Radius;
                
                // 以下逻辑与原Shader一致，仅替换SDF函数为sdCircle
                // 1. 计算轮廓区域的SDF（向内收缩厚度）
                float outline = sdCircle(i.uv, circleRadius) - _Thickness;
                float fw = fwidth(outline); // 计算屏幕空间的梯度，用于抗锯齿
                outline = smoothstep(0.0, fw, -outline); // 平滑过渡，生成轮廓遮罩
                
                // 2. 计算内部镂空区域的SDF（向外扩展内部大小）
                float inner = sdCircle(i.uv, circleRadius) + _Inner;
                float fw_i = fwidth(inner);
                inner = smoothstep(0.0, fw_i, -inner);
                
                // 3. 计算圆形主体区域的SDF
                float block = sdCircle(i.uv, circleRadius);
                float fw_b = fwidth(block);
                block = smoothstep(0.0, fw_b, -block);
                
                // 4. 区域遮罩叠加（分离轮廓、主体、内部镂空区域，与原逻辑一致）
                outline = outline - block; // 轮廓区域 = 外边界 - 主体区域
                inner = block - inner;     // 内部区域 = 主体区域 - 内边界
                block = block - inner;     // 主体区域 = 原主体 - 内部区域
                
                // 5. 颜色混合输出
                float4 output = outline*_OutlineColor + block*_BlockColor + inner*_InnerColor;
                return output;
            }
            ENDCG
        }
    }
}