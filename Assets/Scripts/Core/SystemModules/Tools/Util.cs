using System;
using System.Collections;
using UnityEngine;

namespace Core.SystemModules.Tools
{
    /// <summary>
    /// MonoBehaviour通用工具类（单例）
    /// 提供延迟执行、循环执行、条件等待执行、定时执行等协程封装
    /// </summary>
    public class Util : PersistentSingleton<Util>
    {
        #region 基础延迟执行

        /// <summary>
        /// 在指定时间后执行指定的动作
        /// </summary>
        /// <param name="delayTime">延迟时间（秒）</param>
        /// <param name="action">要执行的动作</param>
        /// <returns>协程引用（可用于取消）</returns>
        public Coroutine RunAfter(float delayTime, Action action)
        {
            if (action == null)
            {
                Debug.LogWarning($"[{nameof(Util)}] 延迟执行的Action为空，取消协程");
                return null;
            }

            if (delayTime < 0)
            {
                Debug.LogWarning($"[{nameof(Util)}] 延迟时间不能为负数，已修正为0秒");
                delayTime = 0;
            }

            return StartCoroutine(WaitForSecondsCoroutine(delayTime, action));
        }

        /// <summary>
        /// 延迟执行协程核心逻辑
        /// </summary>
        private IEnumerator WaitForSecondsCoroutine(float delayTime, Action action)
        {
            yield return new WaitForSeconds(delayTime);
            ExecuteActionSafely(action, nameof(RunAfter));
        }

        #endregion

        #region 循环执行

        /// <summary>
        /// 循环执行指定动作
        /// </summary>
        /// <param name="interval">执行间隔（秒）</param>
        /// <param name="action">要执行的动作</param>
        /// <param name="isImmediate">是否立即执行第一次（默认false：等待第一个间隔后执行）</param>
        /// <param name="loopCount">循环次数（-1=无限循环，0=不执行，默认-1）</param>
        /// <returns>协程引用（可用于取消）</returns>
        public Coroutine RunLoop(float interval, Action action, bool isImmediate = false, int loopCount = -1)
        {
            if (action == null)
            {
                Debug.LogWarning($"[{nameof(Util)}] 循环执行的Action为空，取消协程");
                return null;
            }

            if (interval < 0)
            {
                Debug.LogWarning($"[{nameof(Util)}] 循环间隔不能为负数，已修正为0秒");
                interval = 0;
            }

            if (loopCount == 0)
            {
                Debug.LogWarning($"[{nameof(Util)}] 循环次数为0，取消协程");
                return null;
            }

            return StartCoroutine(LoopCoroutine(interval, action, isImmediate, loopCount));
        }

        /// <summary>
        /// 循环执行协程核心逻辑
        /// </summary>
        private IEnumerator LoopCoroutine(float interval, Action action, bool isImmediate, int loopCount)
        {
            // 立即执行第一次
            if (isImmediate)
            {
                ExecuteActionSafely(action, nameof(RunLoop));
                // 若仅需执行一次，直接结束
                if (loopCount == 1) yield break;
            }

            int executedCount = isImmediate ? 1 : 0;
            while (true)
            {
                yield return new WaitForSeconds(interval);

                ExecuteActionSafely(action, nameof(RunLoop));
                executedCount++;

                // 有限循环：达到次数后终止
                if (loopCount > 0 && executedCount >= loopCount)
                {
                    break;
                }
            }
        }

        #endregion

        #region 等待条件执行

        /// <summary>
        /// 等待指定条件满足后执行动作（支持超时）
        /// </summary>
        /// <param name="condition">等待的条件（返回true表示满足）</param>
        /// <param name="onSuccess">条件满足时执行的动作</param>
        /// <param name="timeout">超时时间（秒，0=永不超时）</param>
        /// <param name="onTimeout">超时后执行的动作（可选）</param>
        /// <returns>协程引用（可用于取消）</returns>
        public Coroutine RunUntilCondition(Func<bool> condition, Action onSuccess, float timeout = 0,
            Action onTimeout = null)
        {
            if (condition == null)
            {
                Debug.LogWarning($"[{nameof(Util)}] 等待条件为空，取消协程");
                return null;
            }

            if (onSuccess == null)
            {
                Debug.LogWarning($"[{nameof(Util)}] 条件满足的执行动作为空，取消协程");
                return null;
            }

            if (timeout < 0)
            {
                Debug.LogWarning($"[{nameof(Util)}] 超时时间不能为负数，已修正为0秒（永不超时）");
                timeout = 0;
            }

            return StartCoroutine(ConditionWaitCoroutine(condition, onSuccess, timeout, onTimeout));
        }

        /// <summary>
        /// 条件等待协程核心逻辑
        /// </summary>
        private IEnumerator ConditionWaitCoroutine(Func<bool> condition, Action onSuccess, float timeout,
            Action onTimeout)
        {
            float elapsedTime = 0;
            while (true)
            {
                // 检查条件是否满足
                if (condition.Invoke())
                {
                    ExecuteActionSafely(onSuccess, nameof(RunUntilCondition));
                    yield break;
                }

                // 检查是否超时
                if (timeout > 0)
                {
                    elapsedTime += Time.deltaTime;
                    if (elapsedTime >= timeout)
                    {
                        if (onTimeout != null)
                        {
                            ExecuteActionSafely(onTimeout, nameof(RunUntilCondition));
                        }
                        else
                        {
                            Debug.LogWarning($"[{nameof(Util)}] 条件等待超时（{timeout}秒），无超时回调");
                        }

                        yield break;
                    }
                }

                yield return null; // 等待下一帧
            }
        }

        #endregion

        #region 定时执行（指定时间点）

        /// <summary>
        /// 在指定的时间点执行动作
        /// </summary>
        /// <param name="targetTime">目标执行时间</param>
        /// <param name="action">要执行的动作</param>
        /// <returns>协程引用（可用于取消）</returns>
        public Coroutine RunAtTime(DateTime targetTime, Action action)
        {
            if (action == null)
            {
                Debug.LogWarning($"[{nameof(Util)}] 定时执行的Action为空，取消协程");
                return null;
            }

            // 计算当前时间到目标时间的差值（秒）
            TimeSpan timeSpan = targetTime - DateTime.Now;
            float delayTime = (float)timeSpan.TotalSeconds;

            if (delayTime <= 0)
            {
                Debug.LogWarning($"[{nameof(Util)}] 目标时间已过期，立即执行动作");
                ExecuteActionSafely(action, nameof(RunAtTime));
                return null;
            }

            return RunAfter(delayTime, action);
        }

        /// <summary>
        /// 在指定的延迟后开始，按固定时间点循环执行（如：每小时整点执行）
        /// </summary>
        /// <param name="initialDelay">初始延迟（秒）</param>
        /// <param name="cycleInterval">循环间隔（秒，如3600=1小时）</param>
        /// <param name="action">要执行的动作</param>
        /// <returns>协程引用（可用于取消）</returns>
        public Coroutine RunAtFixedCycle(float initialDelay, float cycleInterval, Action action)
        {
            if (action == null)
            {
                Debug.LogWarning($"[{nameof(Util)}] 定时循环的Action为空，取消协程");
                return null;
            }

            if (cycleInterval <= 0)
            {
                Debug.LogWarning($"[{nameof(Util)}] 循环间隔必须大于0，取消协程");
                return null;
            }

            return StartCoroutine(FixedCycleCoroutine(initialDelay, cycleInterval, action));
        }

        /// <summary>
        /// 固定周期执行协程核心逻辑
        /// </summary>
        private IEnumerator FixedCycleCoroutine(float initialDelay, float cycleInterval, Action action)
        {
            // 初始延迟
            if (initialDelay > 0)
            {
                yield return new WaitForSeconds(initialDelay);
            }

            // 循环执行
            while (true)
            {
                ExecuteActionSafely(action, nameof(RunAtFixedCycle));
                yield return new WaitForSeconds(cycleInterval);
            }
        }

        #endregion

        #region 协程管理

        /// <summary>
        /// 取消指定协程
        /// </summary>
        /// <param name="coroutine">要取消的协程引用</param>
        public void CancelCoroutine(Coroutine coroutine)
        {
            if (coroutine != null)
            {
                StopCoroutine(coroutine);
            }
            else
            {
                Debug.LogWarning($"[{nameof(Util)}] 要取消的协程引用为空");
            }
        }

        /// <summary>
        /// 取消所有协程
        /// </summary>
        public void CancelAllCoroutines()
        {
            StopAllCoroutines();
            Debug.Log($"[{nameof(Util)}] 已取消所有协程");
        }

        #endregion

        #region 辅助方法

        /// <summary>
        /// 安全执行Action（捕获异常避免协程中断）
        /// </summary>
        /// <param name="action">要执行的Action</param>
        /// <param name="methodName">调用来源方法名</param>
        private void ExecuteActionSafely(Action action, string methodName)
        {
            try
            {
                action.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogError($"[{nameof(Util)}] {methodName} 执行Action时发生异常：{e.Message}\n{e.StackTrace}");
            }
        }


        /// <summary>
        /// 销毁时清理所有协程
        /// </summary>
        private void OnDestroy()
        {
            CancelAllCoroutines();
        }

        #endregion
    }
}