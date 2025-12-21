#!/bin/bash

# 配置 Rain World BepInEx 在 Proton/Wine 环境下运行
# Rain World Steam App ID: 312520

echo "=========================================="
echo "BepInEx Wine/Proton Configuration Script"
echo "=========================================="
echo ""

# 检查 protontricks 是否安装
if command -v protontricks &> /dev/null; then
    PROTONTRICKS="protontricks"
elif flatpak list | grep -q protontricks; then
    PROTONTRICKS="flatpak run com.github.Matoking.protontricks"
else
    echo "ERROR: protontricks not found!"
    echo ""
    echo "Please use one of these methods:"
    echo ""
    echo "Method 1 (RECOMMENDED - Easiest):"
    echo "  Add this to Rain World's Steam launch options:"
    echo "  WINEDLLOVERRIDES=\"winhttp=n,b\" %command%"
    echo ""
    echo "Method 2 (Install protontricks):"
    echo "  Run: ./setup-protontricks.sh"
    echo ""
    exit 1
fi

echo "Found protontricks: $PROTONTRICKS"
echo ""
echo "Configuring winhttp.dll override for Rain World (App ID: 312520)..."
echo ""

# 使用 protontricks 配置 DLL override
$PROTONTRICKS 312520 --command sh -c 'wine reg add "HKEY_CURRENT_USER\\Software\\Wine\\DllOverrides" /v winhttp /t REG_SZ /d "native,builtin" /f'

if [ $? -eq 0 ]; then
    echo ""
    echo "=========================================="
    echo "Configuration successful!"
    echo "=========================================="
    echo ""
    echo "winhttp.dll has been configured to use native (game directory) version first."
    echo "BepInEx should now load when you start Rain World."
    echo ""
    echo "To verify:"
    echo "1. Start Rain World"
    echo "2. Check if BepInEx/LogOutput.log is created"
    echo "3. Your mod should appear in the game's mod menu"
    echo ""
else
    echo ""
    echo "ERROR: Configuration failed!"
    echo ""
    echo "Please try the manual method:"
    echo "Add this to Rain World's Steam launch options:"
    echo "  WINEDLLOVERRIDES=\"winhttp=n,b\" %command%"
    echo ""
fi
