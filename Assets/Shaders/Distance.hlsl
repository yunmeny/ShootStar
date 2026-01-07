float sdRoundedBox(float2 p, float2 b, float4 r, out float d)
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
