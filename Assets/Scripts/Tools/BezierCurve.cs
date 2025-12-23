using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BezierCurve
{
   
    /// <summary>
    /// 二阶贝塞尔曲线, 插值参数t的取值范围为[0, 1]
    /// </summary>
    /// <param name="p0">
    /// 起点
    /// </param>
    /// <param name="p1">
    /// 控制点
    /// </param>
    /// <param name="p2">
    /// 终点
    /// </param>
    /// <param name="t">
    /// 插值参数
    /// </param>
    /// <returns>
    /// 插值结果
    /// </returns>
    public static Vector3 QuadraticBezier(Vector3 p0, Vector3 p1, Vector3 p2, float t)
    {
        var a = Vector3.Lerp(p0, p1, t);
        var b = Vector3.Lerp(p1, p2, t);
        return Vector3.Lerp(a, b, t);
    }

    
    /// <summary>
    /// 三阶贝塞尔曲线, 插值参数t的取值范围为[0, 1]
    /// </summary>
    /// <param name="p0">
    /// 起点
    /// </param>
    /// <param name="p1">
    /// 控制点1
    /// </param>
    /// <param name="p2">
    /// 控制点2
    /// </param>
    /// <param name="p3">
    /// 终点
    /// </param>
    /// <param name="t">
    /// 插值参数
    /// </param>
    /// <returns>
    /// 插值结果
    /// </returns>
    public static Vector3 CubicBezier(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        var a = QuadraticBezier(p0, p1, p2, t);
        var b = QuadraticBezier(p1, p2, p3, t);
        return Vector3.Lerp(a, b, t);
    }
}
