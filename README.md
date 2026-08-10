# Steam 视奸机（AMN2 / v2）

Bot is Watching you.👁️

基于 [Another-Mirai-Native2](https://github.com/Hellobaka/Another-Mirai-Native2) 的新插件实现（原 CoolQ/CQP 版本已迁移至本分支）。

## 配置

- 需要申请 [Steam APIKey](https://steamcommunity.com/dev/apikey)
- 可指定游戏名称翻译文本：修改配置中的 `AppInfoLanguage`，内容可参照 [文档](https://partner.steamgames.com/doc/store/localization/languages)

配置文件 `Config.json` 位于插件数据目录（`data\app\me.cqp.luohuaming.SteamWatcher\`），支持热重载。

## 可用的指令

- `#Steam时长图`
- `#绑定Steam`
- `#取消绑定Steam`

> [!WARNING]
> 若APIKey与欲查询的用户为同一个账号时，设置游戏私密功能将无效

> [!NOTE]
> 新成就大约4分钟后才能被Bot发现

> [!NOTE]
> 修改刷新间隔配置后需要重载插件

## 构建与安装

1. 构建 `me.cqp.luohuaming.SteamWatcher.Plugin` 项目（Debug/Release 均可）。
2. 将输出目录中的 `Native_me.cqp.luohuaming.SteamWatcher.Plugin.dll` 与 `Native_me.cqp.luohuaming.SteamWatcher.Plugin.json` 复制到框架的 `data\plugins` 目录。
3. 放置原生依赖（**仅原生库不打包**，需手动放置到框架根目录 / `x86` / `libraries`，任选其一）：
   - `libSkiaSharp.dll`
   - `libHarfBuzzSharp.dll`

   SkiaSharp / SkiaSharp.HarfBuzz / HarfBuzzSharp 的托管程序集已由打包工具合并进 `Native_*.dll`，无需单独放置。
4. 重载插件或重启框架。

`Assets\Frame.png` 会在插件启用时自动生成到插件数据目录；若未生成，请手动放置到插件数据目录的 `Assets` 文件夹。

## 项目结构

- `me.cqp.luohuaming.SteamWatcher.Plugin`：AMN2 插件入口（net9.0-windows，含 WPF 控制台窗口）
- `me.cqp.luohuaming.SteamWatcher.PublicInfos`：配置、Steam API、绘图等公共代码
- `Tester`：离线 Steam API / 绘图冒烟测试控制台
