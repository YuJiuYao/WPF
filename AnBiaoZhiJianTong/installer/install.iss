; ====== 基本信息 ======
[Setup]
; 安装后的应用程序名称
AppName=暗标智检通
; 应用程序唯一 ID（GUID），用于区分不同应用和卸载
AppId={{A8C4A6C1-3A14-4E2B-9C0E-1B6E7E9F7A11}}
; 应用版本号
AppVersion=1.0.0
; 发布者信息（卸载程序和安装向导显示用）
AppPublisher=中星辰宇
; 默认安装路径（{autopf} = Program Files）
DefaultDirName={autopf}\AnBiaoZhiJianTong
; 默认开始菜单文件夹名
DefaultGroupName=暗标智检通
; 输出目录（生成安装包的路径）
OutputDir=artifacts
; 输出文件名（不含扩展名）
OutputBaseFilename=暗标智检通-setup
; 在 x86 和 x64 系统都允许安装，但使用 32 位安装模式
ArchitecturesAllowed=x86 x64
; 压缩算法：lzma2 压缩率高
Compression=lzma2
; 是否启用 solid 压缩（更高压缩率，安装时更快）
SolidCompression=yes
; 安装程序使用的图标
SetupIconFile=setup.ico
; 不显示“选择开始菜单文件夹”的页面
DisableProgramGroupPage=yes
; 是否需要管理员权限
PrivilegesRequired=admin
; 卸载程序显示的图标
UninstallDisplayIcon={app}\AnBiaoZhiJianTong.Shell.exe
; 向导样式（modern = Win10 风格）
WizardStyle=modern
; 自动检测安装语言，使用系统 UI 语言
LanguageDetectionMethod=uilanguage
; 是否显示语言选择对话框（auto = 有多语言包时自动显示）
ShowLanguageDialog=auto
; 安装时关闭可能占用文件的应用
CloseApplications=yes
; 定义关闭应用时的文件匹配规则
CloseApplicationsFilter=*.exe,*.dll
; 安装完成后是否提示重启应用（此处设为不重启）
RestartApplications=no
; 防并发（同一时间只允许一个实例安装；同时也能防止你程序在装/卸载时运行）
AppMutex=AnBiaoZhiJianTong_APP_MUTEX

; ====== 语言配置 ======
[Languages]
Name: "chinesesimp"; MessagesFile: "compiler:Languages\ChineseSimplified.isl"
Name: "english";     MessagesFile: "compiler:Default.isl"

; ====== 自定义任务 ======
[Tasks]
; 是否创建桌面图标
Name: "desktopicon"; Description: "创建桌面快捷方式"; GroupDescription: "其他："
; 是否安装完成后自动运行
Name: "autorun"; Description: "安装完成后立即运行"; GroupDescription: "其他："

; ====== 文件（把发布目录整体装到 {app}） ======
[Files]
Source: "..\src\AnBiaoZhiJianTong.Shell\bin\x86\Release\net462\publish-obf\*"; DestDir: "{app}"; Flags: recursesubdirs ignoreversion

; ====== 前置条件离线包（缺少才执行；若不打包可先注释掉） ======
; Source: "installer\payloads\ndp48-web.exe"; DestDir: "{tmp}"; Flags: deleteafterinstall; Check: not IsDotNet48OrLater()
; Source: "installer\payloads\VC_redist.x86.exe"; DestDir: "{tmp}"; Flags: deleteafterinstall; Check: not IsVCppX86Installed()

[Dirs]
; 给程序安装目录加写权限
Name: "{app}"; Permissions: users-modify

; ====== 快捷方式配置 ======
[Icons]
; 开始菜单快捷方式
Name: "{group}\暗标智检通"; Filename: "{app}\AnBiaoZhiJianTong.Shell.exe"; WorkingDir: "{app}"
; 桌面快捷方式（关联上面的 desktopicon 任务）
Name: "{commondesktop}\暗标智检通"; Filename: "{app}\AnBiaoZhiJianTong.Shell.exe"; Tasks: desktopicon

; ====== 安装完成后执行的动作 ======
[Run]
; 安装 .NET Framework 4.8（如未安装）
; Filename: "{tmp}\ndp48-web.exe"; Parameters: "/passive /norestart"; StatusMsg: "正在安装 .NET Framework 4.8..."; Check: not IsDotNet48OrLater(); Flags: waituntilterminated

; 安装 VC++ x86（如未安装）
; Filename: "{tmp}\VC_redist.x86.exe"; Parameters: "/install /quiet /norestart"; StatusMsg: "正在安装 VC++ 运行库 (x86)..."; Check: not IsVCppX86Installed(); Flags: waituntilterminated

; 运行主程序（可选）
Filename: "{app}\AnBiaoZhiJianTong.Shell.exe"; Description: "启动 暗标智检通"; Flags: nowait postinstall skipifsilent; Tasks: autorun

; ====== 卸载时额外删除的目录（存放用户数据时用） ======
[UninstallDelete]
; 如你的程序会在这两个目录生成数据，可在卸载时一并清理
Type: filesandordirs; Name: "{commonappdata}\AnBiaoZhiJianTong"
Type: filesandordirs; Name: "{localappdata}\AnBiaoZhiJianTong"

; ====== 代码：检测前置条件 ======
[Code]
function IsDotNet48OrLater(): Boolean;
var release: Cardinal;
begin
  { 检测 .NET Framework 4.8+：Release >= 528040 }
  Result := RegQueryDWordValue(HKLM,
    'SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full', 'Release', release)
    and (release >= 528040);
end;

function IsVCppX86Installed(): Boolean;
var installed: Cardinal;
begin
  { VS 2015-2022 x86 运行库：Installed=1 则认为已安装 }
  Result := RegQueryDWordValue(HKLM32,
    'SOFTWARE\Microsoft\VisualStudio\14.0\VC\Runtimes\x86', 'Installed', installed)
    and (installed = 1);
end;
