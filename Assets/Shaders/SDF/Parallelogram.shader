Shader "SDfElement/Parallelogram"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Scale ("Scale", Vector) = (1,1,1,1) // 缩放系数（x,y分别控制宽度和高度）
        _Width ("Width", Range(0, 3)) = 0.5 // 平行四边形宽度
        _Height ("Height", Range(0, 2)) = 0.5 // 平行四边形高度
        _Skew ("Skew", Range(-1, 1)) = 0.2 // 平行四边形倾斜系数（左右偏移，正负控制倾斜方向）

        _BlockColor ("BlockColor", Color) = (1,1,1,1) // 主体颜色
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
        Blend SrcAlpha OneMinusSrcAlpha // 保持透明混合模式，与原有Shader一致
        ZWrite Off // 关闭深度写入
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            // 平行四边形核心参数（对应Properties中的调节项）
            float _Width;
            float _Height;
            float _Skew;
            // 原有通用属性
            float _Thickness;
            float _Inner;
            float4 _BlockColor;
            float4 _OutlineColor;
            float4 _InnerColor;
            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _MainTex_TexelSize;
            float4 _Scale;
            
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

            // 核心改造：将给定的HLSL平行四边形SDF算法适配为CG兼容函数
            // 输入：归一化坐标p（中心在原点）、宽度wi、高度he、倾斜系数sk
            // 输出：平行四边形的有符号距离（内部为负，外部为正，边界为0）
            float sdParallelogram(float2 p, float wi, float he, float sk)
            {
                // 1. 构造偏移向量e（平行四边形的倾斜边向量）
                float2 e = float2(sk, he);

                // 2. 基于y轴符号翻转p，保证下半部分与上半部分对称
                p = p.y < 0.0f ? -p : p;

                // 3. 计算w向量并修正x分量（限制在宽度范围内）
                float2 w = p - e;
                w.x -= clamp(w.x, -wi, wi);

                // 4. 初始化距离候选d（x分量为向量模长平方，y分量为-w.y，用于后续符号判定）
                float2 d = float2(dot(w, w), -w.y);

                // 5. 计算叉积符号项s（平行四边形核心形状判定，区分内外侧）
                float s = p.x * e.y - p.y * e.x;

                // 6. 基于叉积符号翻转p，保证左右倾斜部分对称
                p = s < 0.0f ? -p : p;

                // 7. 计算v向量并投影修正（贴合平行四边形的斜边）
                float2 v = p - float2(wi, 0.0f);
                v -= e * clamp(dot(v, e) / dot(e, e), -1.0f, 1.0f);

                // 8. 取最小距离候选（保留最接近边界的距离值）
                d = min(d, float2(dot(v, v), wi * he - abs(s)));

                // 9. 计算最终有符号距离（开方得到实际距离，结合符号区分内外）
                return sqrt(d.x) * sign(-d.y);
            }

            // 顶点着色器（与原有Shader完全一致，无修改）
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex); // 模型空间转裁剪空间
                float2 localScale = float2(
                    length(unity_ObjectToWorld[0].xy), // x轴缩放
                    length(unity_ObjectToWorld[1].xy) // y轴缩放
                );
                o.uv = (v.uv*2-1) * localScale;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // 传递参数到平行四边形SDF函数（适配Shader属性，保证可调节）
                float parallelogramSDF = sdParallelogram(i.uv*(1/_Scale), _Width, _Height, _Skew);
                
                // 以下逻辑与原有圆形/矩形Shader完全一致，保证轮廓、主体、内部镂空的效果
                // 1. 计算轮廓区域的SDF（向内收缩厚度）
                float outline = parallelogramSDF - _Thickness;
                float fw = fwidth(outline); // 计算屏幕空间梯度，实现抗锯齿
                outline = smoothstep(0.0, fw, -outline); // 平滑过渡，生成轮廓遮罩
                
                // 2. 计算内部镂空区域的SDF（向外扩展内部大小）
                float inner = parallelogramSDF + _Inner;
                float fw_i = fwidth(inner);
                inner = smoothstep(0.0, fw_i, -inner);
                
                // 3. 计算平行四边形主体区域的SDF
                float block = parallelogramSDF;
                float fw_b = fwidth(block);
                block = smoothstep(0.0, fw_b, -block);
                
                // 4. 区域遮罩分离（与原有逻辑一致，保证三层区域不重叠）
                outline = outline - block; // 轮廓区域 = 外边界 - 主体区域
                inner = block - inner;     // 内部区域 = 主体区域 - 内边界
                block = block - inner;     // 主体区域 = 原主体 - 内部区域
                
                // 5. 颜色混合输出
                float4 output = outline*_OutlineColor + block*_BlockColor + inner*_InnerColor;

                
                float3 texColor = tex2D(_MainTex, (i.uv*(1/_Scale)*_MainTex_ST.xy+_MainTex_ST.zw)*float2(_MainTex_TexelSize.x/_MainTex_TexelSize.y,1)).rgb * output;
                output = float4(texColor, output.a);
                return output;
            }
            ENDCG
        }
    }
}