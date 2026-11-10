# ShootStar
A demo game.

当前 Gameplay 临时闭环：

- 从 `Assets/Scenes/MainMenu_Improved.unity` 启动；也可直接运行 `GamePlay` 场景。
- 故事模式：清理前两波普通敌人，击败第三波 Boss 后通关。Boss 波次沿用 Enemy Manager 的 `bossWaveNumber` 配置，当前为 3。
- 无尽模式：Boss 波次结束后继续下一波，直到玩家死亡。
- 暂停可继续、进入设置并返回本局，或返回主菜单。设置页提供本次运行的音乐和音效音量调整。
- 通关或死亡后，确认进入结算；达到前十可提交名字保存成绩，取消则跳过保存。
- 结算页可选择 `PLAY AGAIN` 重开当前模式，或返回主菜单。新一局重置分数、角色和波次。

编辑器验证应覆盖故事通关、无尽跨过 Boss 波次、失败结算、重开，以及暂停/设置/主菜单的往返。
