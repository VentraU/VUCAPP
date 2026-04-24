<?php

$tmp = sys_get_temp_dir() . "\\win11_folder.ps1";

$ps = '
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8
Add-Type -AssemblyName System.Windows.Forms

$folder = (New-Object -ComObject Shell.Application).BrowseForFolder(0,"Select Folder, BbIBEPNTE PAPKY",0)

if ($folder -ne $null) {
    Write-Output $folder.Self.Path
}
';

file_put_contents($tmp,$ps);

header("Content-Type: text/plain; charset=UTF-8");
echo shell_exec("powershell -NoProfile -STA -ExecutionPolicy Bypass -File \"$tmp\"");

unlink($tmp);

?>