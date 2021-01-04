Remove-Item ..\QuickLook.Plugin.FolderViewer.qlplugin -ErrorAction SilentlyContinue

$files = Get-ChildItem -Path ..\bin\Release\ -Exclude *.pdb,*.xml
Compress-Archive $files ..\QuickLook.Plugin.FolderViewer.zip
Move-Item ..\QuickLook.Plugin.FolderViewer.zip ..\QuickLook.Plugin.FolderViewer.qlplugin