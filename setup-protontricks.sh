#!/bin/bash

# 安装 protontricks (Flatpak 版本)
echo "Installing protontricks via Flatpak..."
flatpak install -y flathub com.github.Matoking.protontricks

echo ""
echo "=========================================="
echo "Protontricks installed successfully!"
echo "=========================================="
echo ""
echo "Next steps:"
echo "1. Run: flatpak run com.github.Matoking.protontricks --gui"
echo "2. Select 'Rain World' (App ID: 312520)"
echo "3. Select 'Select the default wineprefix'"
echo "4. Select 'Run winecfg'"
echo "5. Go to 'Libraries' tab"
echo "6. In 'New override for library' dropdown, select 'winhttp'"
echo "7. Click 'Add', then 'Apply', then 'OK'"
echo ""
echo "Alternatively, use the simple Steam launch option method:"
echo "Add this to Rain World's launch options in Steam:"
echo "  WINEDLLOVERRIDES=\"winhttp=n,b\" %command%"
echo ""
