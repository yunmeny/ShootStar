float sdRoundedBox_float(float2 p, float2 b, float4 r, out float d)
{
    // HLSL 支持向量分量的三元运算符，逻辑与 GLSL 一致
    r.xy = (p.x > 0.0f) ? r.xy : r.zw;
    r.x = (p.y > 0.0f) ? r.x : r.y;

    // 向量运算：abs/加减 在 HLSL 中与 GLSL 行为完全一致
    float2 q = abs(p) - b + r.x;

    d = min(max(q.x, q.y), 0.0f) + length(max(q, 0.0f)) - r.x;
    // HLSL 的 max/min 支持标量/向量重载，length 对 float2 的计算逻辑不变
    return min(max(q.x, q.y), 0.0f) + length(max(q, 0.0f)) - r.x;
}

void sdCircle_float(float2 p, float r, out float d)
{
    d = length(p) - r;
}

void sdParallelogram_float(float2 p, float wi, float he, float sk, out float res)
{
    // 1. 构造偏移向量e，GLSL vec2 → HLSL float2
    float2 e = float2(sk, he);

    // 2. 基于y轴符号翻转p，浮点常量添加f后缀（HLSL规范）
    p = (p.y < 0.0f) ? -p : p;

    // 3. 计算w向量并修正x分量
    float2 w = p - e;
    w.x -= clamp(w.x, -wi, wi);

    // 4. 初始化距离候选d（点积/向量运算与GLSL完全一致）
    float2 d = float2(dot(w, w), -w.y);

    // 5. 计算叉积符号项s（平行四边形核心判定）
    float s = p.x * e.y - p.y * e.x;

    // 6. 基于叉积符号翻转p
    p = (s < 0.0f) ? -p : p;

    // 7. 计算v向量并投影修正（clamp范围常量加f后缀）
    float2 v = p - float2(wi, 0.0f); // 0→0.0f避免整数/浮点混合
    v -= e * clamp(dot(v, e) / dot(e, e), -1.0f, 1.0f);

    // 8. 取最小距离候选（min对float2的重载行为与GLSL一致）
    d = min(d, float2(dot(v, v), wi * he - abs(s)));

    // 9. 计算最终有符号距离（sqrt/sign行为与GLSL一致）
    res = sqrt(d.x) * sign(-d.y);
}


void sdCoolS_float(float2 p, out float d)
{
    // 复制输入坐标到局部变量，避免修改原始输入
    float2 p_local = p;

    // 原代码：float six = (p.y<0.0) ? -p.x : p.x;
    float six = (p_local.y < 0.0) ? -p_local.x : p_local.x;

    // 原代码：p.x = abs(p.x); p.y = abs(p.y) - 0.2;
    p_local.x = abs(p_local.x);
    p_local.y = abs(p_local.y) - 0.2;

    // 原代码：float rex = p.x - min(round(p.x/0.4),0.4);
    float rex = p_local.x - min(round(p_local.x / 0.4), 0.4);

    // 原代码：float aby = abs(p.y-0.2)-0.6;
    float aby = abs(p_local.y - 0.2) - 0.6;

    // 原代码：float d = dot2(vec2(six,-p.y)-clamp(0.5*(six-p.y),0.0,0.2));
    // 注：GLSL的dot2(v) 等价于 HLSL的dot(v,v)（向量自身点乘，即长度平方）
    float clamp_val1 = clamp(0.5 * (six - p_local.y), 0.0, 0.2);
    float2 vec1 = float2(six, -p_local.y) - clamp_val1;
    float d_temp = dot(vec1, vec1);

    // 原代码：d = min(d,dot2(vec2(p.x,-aby)-clamp(0.5*(p.x-aby),0.0,0.4)));
    float clamp_val2 = clamp(0.5 * (p_local.x - aby), 0.0, 0.4);
    float2 vec2 = float2(p_local.x, -aby) - clamp_val2;
    d_temp = min(d_temp, dot(vec2, vec2));

    // 原代码：d = min(d,dot2(vec2(rex,p.y  -clamp(p.y          ,0.0,0.4))));
    float clamp_val3 = clamp(p_local.y, 0.0, 0.4);
    float2 vec3 = float2(rex, p_local.y - clamp_val3);
    d_temp = min(d_temp, dot(vec3, vec3));

    // 原代码：float s = 2.0*p.x + aby + abs(aby+0.4) - 0.4;
    float s = 2.0 * p_local.x + aby + abs(aby + 0.4) - 0.4;

    // 原代码：return sqrt(d) * sign(s);
    d = sqrt(d_temp) * sign(s);
}

void sdRoundedCross_float(float2 p, float h, out float d)
{
    // 复制输入坐标到局部变量，避免修改原始输入参数
    float2 p_local = p;

    // 原GLSL：float k = 0.5*(h+1.0/h);
    float k = 0.5 * (h + 1.0 / h);

    // 原GLSL：p = abs(p);
    p_local = abs(p_local);

    // 原GLSL的三元运算符逻辑，完整复刻计算规则
    // 注：GLSL的dot2(v) = 向量自身点乘 = HLSL的dot(v, v)
    d = (p_local.x < 1.0 && p_local.y < p_local.x * (k - h) + h)
            // 分支1：圆角区域的距离计算
            ? (k - sqrt(dot(p_local - float2(1.0, k), p_local - float2(1.0, k))))
            // 分支2：十字形臂部的距离计算
            : sqrt(min(
                dot(p_local - float2(0.0, h), p_local - float2(0.0, h)),
                dot(p_local - float2(1.0, 0.0), p_local - float2(1.0, 0.0))
            ));
}

// 菱形
void sdRhombus_float(float2 p, float2 b, out float d)
{
    b.y = -b.y;
    p = abs(p);
    
    float h = clamp( (dot(b, p) + b.y * b.y) / dot(b, b), 0.0, 1.0 );
    p -= b * float2(h, h - 1.0);
    
    d = length(p) * sign(p.x);
}

// 辅助函数：计算向量的点乘自身（对应原代码的 dot2）
float dot2(float2 v)
{
    return dot(v, v); // 等价于 v.x*v.x + v.y*v.y
}

// 转换后的心形距离场函数（与 sdCircle_float 格式完全一致）
void sdHeart_float(float2 p, out float d)
{
    // 1. 复制输入向量并处理 x 轴绝对值（原代码 p.x = abs(p.x)）
    float2 q = p;
    q.x = abs(q.x);

    // 2. 分支判断，计算距离值并赋值给 out 参数 d
    if (q.y + q.x > 1.0)
    {
        // 分支1：心形外侧尖角区域的距离计算
        d = sqrt(dot2(q - float2(0.25, 0.75))) - sqrt(2.0) / 4.0;
    }
    else
    {
        // 分支2：心形主体区域的距离计算
        float term1 = dot2(q - float2(0.00, 1.00));          // 上半圆项
        float term2 = dot2(q - 0.5 * max(q.x + q.y, 0.0));   // 下半部项
        float min_term = min(term1, term2);                  // 取两项最小值
        d = sqrt(min_term) * sign(q.x - q.y);                // 最终距离（带符号）
    }
}

// 转换后的圆角X形距离场函数（与 sdCircle_float 格式完全一致）
void sdRoundedX_float(float2 p, float w, float r, out float d)
{
    // 1. 对输入向量取绝对值（原代码 p = abs(p)）
    // 使用临时变量 q 避免修改原始输入参数
    float2 q = abs(p);
    
    // 2. 核心距离计算逻辑（完全保留原算法，仅拆解为更易读的步骤）
    // 计算 min(p.x+p.y, w) * 0.5 标量值
    float min_scalar = min(q.x + q.y, w) * 0.5;
    // 向量减法：q - (min_scalar, min_scalar)（与原代码 p-min(...) 等价）
    float2 temp_vec = q - float2(min_scalar, min_scalar);
    // 最终距离计算：向量长度减圆角半径
    d = length(temp_vec) - r;
}


// 转换后的十字形距离场函数（与 sdCircle_float 格式完全一致）
void sdCross_float(float2 p, float2 b, float r, out float d)
{
    // 1. 对输入向量取绝对值（原代码 p = abs(p)）
    // 使用临时变量 q 避免修改原始输入参数
    float2 q = abs(p);
    
    // 2. 交换x/y分量（原代码 (p.y>p.x) ? p.yx : p.xy）
    // GLSL的p.yx等价于float2(q.y, q.x)（交换x和y分量）
    q = (q.y > q.x) ? float2(q.y, q.x) : q;
    
    // 3. 原代码 vec2 q = p - b → 新增temp_q避免变量覆盖
    float2 temp_q = q - b;
    
    // 4. 计算k = max(q.y, q.x)（对应原代码 float k = max(q.y,q.x)）
    float k = max(temp_q.y, temp_q.x);
    
    // 5. 计算w向量（原代码三元运算符拆分为if-else，逻辑等价且更易读）
    float2 w;
    if (k > 0.0)
    {
        w = temp_q;
    }
    else
    {
        w = float2(b.y - q.x, -k);
    }
    
    // 6. 最终距离计算（原代码 return sign(k)*length(max(w,0.0)) + r）
    float2 max_w = max(w, float2(0.0, 0.0)); // GLSL的max(w,0.0)等价于向量与0向量比较
    d = sign(k) * length(max_w) + r;
}


