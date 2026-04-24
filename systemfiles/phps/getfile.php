<?php
$ext = isset($_GET['ext']) ? $_GET['ext'] : "*";

$tmp = sys_get_temp_dir() . "\\select_file.ps1";

$ps = '
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8
Add-Type -AssemblyName System.Windows.Forms

$dialog = New-Object System.Windows.Forms.OpenFileDialog
$dialog.Filter = "Files (*.' . $ext . ')|*.' . $ext . '"

if($dialog.ShowDialog() -eq "OK"){
    Write-Output $dialog.FileName
}
';

file_put_contents($tmp, $ps);

header("Content-Type: text/plain; charset=UTF-8");
echo shell_exec("powershell -NoProfile -ExecutionPolicy Bypass -File \"$tmp\"");

unlink($tmp);
?>