Shader "SDfElement/RoundedCross"
{
    Properties
    {
        _Scale ("Scale", Vector) = (1,1,1,1) // 缩放系数（x,y分别控制宽度和高度）
        _CrossShapeParam ("Cross Shape Param (h)", Range(0.1, 2.0)) = 0.8 // 圆角十字形状参数（对应原函数h，控制圆角大小和十字臂比例）

        _BlockColor ("BlockColor", Color) = (1,1,1,1) // 十字主体颜色
        _Thickness ("Thickness", Range(0, 1)) = 0.01 // 轮廓厚度
        _OutlineColor ("OutlineColor", Color) = (1,1,1,1) // 轮廓颜色
        _Inner("Inner", Range(0, 1)) = 0 // 内部镂空区域大小
        _InnerColor("InnerColor", Color) = (1,1,1,1) // 内部镂空区域颜色
    }
    SubShader
    {
        Tags
        {
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

            // 圆角十字专属参数
            float _CrossShapeParam;
            // 原有通用属性
            float _Thickness;
            float _Inner;
            float4 _BlockColor;
            float4 _OutlineColor;
            float4 _InnerColor;
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

            // 核心改造：将给定的HLSL圆角十字SDF算法适配为CG兼容函数
            // 输入：归一化坐标p（中心在原点）、十字形状参数h（控制圆角和臂部比例）
            // 输出：圆角十字的有符号距离（内部为负，外部为正，边界为0）
            float sdRoundedCross(float2 p, float h)
            {
                // 1. 复制输入坐标到局部变量，避免修改原始输入参数（保留原逻辑）
                float2 p_local = p;

                // 2. 计算派生参数k（控制圆角过渡的关键系数，原公式复刻）
                float k = 0.5f * (h + 1.0f / h);

                // 3. 坐标取绝对值，实现十字的上下左右对称（简化计算，无需重复处理四个象限）
                p_local = abs(p_local);

                // 4. 三元运算符判断：区分圆角区域和十字臂部区域，计算对应距离
                float d;
                if (p_local.x < 1.0f && p_local.y < p_local.x * (k - h) + h)
                {
                    // 分支1：圆角区域的距离计算（十字拐角的圆角）
                    d = k - sqrt(dot(p_local - float2(1.0f, k), p_local - float2(1.0f, k)));
                }
                else
                {
                    // 分支2：十字形臂部的距离计算（十字横竖臂的直边区域）
                    d = sqrt(min(
                        dot(p_local - float2(0.0f, h), p_local - float2(0.0f, h)),
                        dot(p_local - float2(1.0f, 0.0f), p_local - float2(1.0f, 0.0f))
                    ));
                }

                // 5. 返回最终有符号距离（保持SDF函数统一特性：内部负、外部正）
                return d;
            }

            // 顶点着色器（与原有所有SDF Shader完全一致，无修改）
            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex); // 模型空间转裁剪空间
                float2 localScale = float2(
                    length(unity_ObjectToWorld[0].xy), // x轴缩放
                    length(unity_ObjectToWorld[1].xy) // y轴缩放
                );
                o.uv = (v.uv * 2 - 1) * localScale;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // 传递参数到圆角十字SDF函数（使用暴露的可调节属性_CrossShapeParam对应原函数h）
                float roundedCrossSDF = sdRoundedCross(i.uv*(1/_Scale), _CrossShapeParam);

                // 以下逻辑完全复用统一框架，保证三层区域效果与其他形状一致
                // 1. 计算轮廓区域的SDF（向内收缩厚度，实现边框效果）
                float outline = roundedCrossSDF - _Thickness;
                float fw = fwidth(outline); // 计算屏幕空间梯度，实现平滑抗锯齿
                outline = smoothstep(0.0, fw, -outline); // 生成轮廓遮罩

                // 2. 计算内部镂空区域的SDF（向外扩展镂空大小）
                float inner = roundedCrossSDF + _Inner;
                float fw_i = fwidth(inner);
                inner = smoothstep(0.0, fw_i, -inner);

                // 3. 计算圆角十字主体区域的SDF
                float block = roundedCrossSDF;
                float fw_b = fwidth(block);
                block = smoothstep(0.0, fw_b, -block);

                // 4. 区域遮罩分离（保证轮廓、主体、内部镂空互不重叠）
                outline = outline - block; // 轮廓 = 外边界 - 主体
                inner = block - inner; // 内部 = 主体 - 内边界
                block = block - inner; // 主体 = 原主体 - 内部镂空

                // 5. 颜色混合输出（三层颜色叠加，透明通道正常生效）
                float4 output = outline * _OutlineColor + block * _BlockColor + inner * _InnerColor;
                return output;
            }
            ENDCG
        }
    }
}