<?php
// backend.php

ini_set('display_errors', 0);
set_time_limit(0); // Отключаем лимит ожидания

header("Access-Control-Allow-Origin: *");
header("Content-Type: application/json; charset=UTF-8");

$input = json_decode(file_get_contents("php://input"), true);
$action = $input['action'] ?? '';
$path = $input['path'] ?? '';
$content = $input['content'] ?? ''; 
$dest = $input['dest'] ?? '';
$inside = $input['inside'] ?? true; 
$wait = $input['wait'] ?? false;

$response = ['status' => 'error', 'message' => 'Invalid action'];

try {
    switch ($action) {
        case 'mdfile': 
            $dir = dirname($path);
            if (!is_dir($dir)) mkdir($dir, 0777, true);
            
            if (file_put_contents($path, $content) !== false) {
                $response = ['status' => 'success', 'message' => "File created: $path"];
            }
            break;

        case 'makedir': 
            if (!is_dir($path)) {
                if (mkdir($path, 0777, true)) {
                    $response = ['status' => 'success', 'message' => "Dir created: $path"];
                }
            } else {
                $response = ['status' => 'success', 'message' => "Dir already exists"];
            }
            break;

        case 'copydir': 
            if (is_dir($path)) {
                $target = $dest;
                if ($inside === false) {
                    $target = $dest . '/' . basename($path);
                }
                recurseCopy($path, $target);
                $response = ['status' => 'success', 'message' => "Copied to $target"];
            } else {
                $response = ['status' => 'error', 'message' => "Source dir not found"];
            }
            break;

        case 'copyfile': 
            if (is_file($path)) {
                $dir = dirname($dest);
                if (!is_dir($dir)) mkdir($dir, 0777, true);
                
                if (copy($path, $dest)) {
                    $response = ['status' => 'success', 'message' => "File copied to $dest"];
                } else {
                    $response = ['status' => 'error', 'message' => "Failed to copy file"];
                }
            } else {
                $response = ['status' => 'error', 'message' => "Source file not found: $path"];
            }
            break;
















case 'listall': 
            if (is_dir($path)) {
                $items = [];
                $files = scandir($path);
                foreach ($files as $item) {
                    if ($item == '.' || $item == '..') continue;
                    $isDir = is_dir("$path/$item");
                    $items[] = [
                        'name' => $item,
                        'type' => $isDir ? 'dir' : 'file'
                    ];
                }
                // Сортировка: сначала папки, потом файлы
                usort($items, function($a, $b) {
                    if ($a['type'] == $b['type']) return strcmp($a['name'], $b['name']);
                    return $a['type'] == 'dir' ? -1 : 1;
                });
                $response = ['status' => 'success', 'data' => $items];
            } else {
                $response = ['status' => 'error', 'message' => 'Directory not found'];
            }
            break;

        case 'rename':
            if (file_exists($path)) {
                if (rename($path, $dest)) {
                    $response = ['status' => 'success', 'message' => "Renamed to $dest"];
                } else {
                    $response = ['status' => 'error', 'message' => "Rename failed"];
                }
            } else {
                $response = ['status' => 'error', 'message' => "Source not found"];
            }
            break;

        case 'upload_b64':
            $dir = dirname($path);
            if (!is_dir($dir)) mkdir($dir, 0777, true);
            $data = base64_decode($content);
            if (file_put_contents($path, $data) !== false) {
                $response = ['status' => 'success', 'message' => "File uploaded: $path"];
            } else {
                $response = ['status' => 'error', 'message' => "Upload failed"];
            }
            break;



















            





case 'unzip': 
            if (file_exists($path)) {
                $zip = new ZipArchive;
                if ($zip->open($path) === TRUE) {
                    // Создаем папку если её нет
                    if (!is_dir($dest)) mkdir($dest, 0777, true);
                    $zip->extractTo($dest);
                    $zip->close();
                    $response = ['status' => 'success', 'message' => "Unzipped successfully"];
                } else {
                    $response = ['status' => 'error', 'message' => "Failed to open ZIP file"];
                }
            } else {
                $response = ['status' => 'error', 'message' => "Source file not found: $path"];
            }
            break;

case 'zip':
            if (!class_exists('ZipArchive')) {
                $response = ['status' => 'error', 'message' => 'ZipArchive missing'];
                break;
            }

            $sourcePath = realpath($path);
            $zipPath = $dest;

            if (!$sourcePath) {
                $response = ['status' => 'error', 'message' => "Source path not found"];
                break;
            }

            $zip = new ZipArchive();
            if ($zip->open($zipPath, ZipArchive::CREATE | ZipArchive::OVERWRITE) !== TRUE) {
                $response = ['status' => 'error', 'message' => "Could not create zip: $zipPath"];
                break;
            }

            // Используем SELF_FIRST, чтобы итератор видел и папки, и файлы
            $files = new RecursiveIteratorIterator(
                new RecursiveDirectoryIterator($sourcePath, RecursiveDirectoryIterator::SKIP_DOTS),
                RecursiveIteratorIterator::SELF_FIRST
            );

            foreach ($files as $name => $file) {
                $filePath = $file->getRealPath();
                
                // Вычисляем относительный путь
                $relativePath = substr($filePath, strlen($sourcePath) + 1);
                $relativePath = str_replace('\\', '/', $relativePath); // Нормализация для ZIP

                if ($file->isDir()) {
                    // Явно добавляем пустую директорию в ZIP
                    $zip->addEmptyDir($relativePath);
                } else {
                    // Добавляем файл
                    $zip->addFile($filePath, $relativePath);
                }
            }

            if ($zip->close()) {
                $response = ['status' => 'success', 'message' => "Project exported with directories to $zipPath"];
            } else {
                $response = ['status' => 'error', 'message' => "Failed to save ZIP"];
            }
            break;


                
        case 'runfile': 
            // --- NEW CODE: Special handling for explorer.exe to bypass local file checks ---
            if (strtolower($path) === 'explorer.exe') {
                // Resolve the relative folder path into an absolute system path
                $targetDir = realpath(trim($content));
                $targetDir = $targetDir ? $targetDir : trim($content);
                
                // Execute explorer.exe with the absolute path
                $cmd = "start \"\" explorer.exe \"$targetDir\"";
                pclose(popen($cmd, "r"));
                
                $response = ['status' => 'success', 'message' => "Executed: explorer.exe"];
                break;
            }
            // -------------------------------------------------------------------------------

            $realPath = realpath($path);

            if ($realPath && file_exists($realPath)) {
                $workDir = dirname($realPath);
                $fileName = basename($realPath);
                
                $cmdPrefix = $wait ? 'start /wait' : 'start';

                $cmd = "cd /d \"$workDir\" && $cmdPrefix \"\" \"$fileName\" $content";
                
                pclose(popen($cmd, "r"));
                
                $response = ['status' => 'success', 'message' => "Executed: $realPath"];
            } else {
                $response = ['status' => 'error', 'message' => "File not found: $path"];
            }
            break;

        
        case 'readfile': 
            if (file_exists($path)) {
                $fileContent = file_get_contents($path);
                // ИСПРАВЛЕНО: возвращаем 'data' вместо 'content', чтобы index.html мог прочитать JSON
                $response = ['status' => 'success', 'data' => $fileContent, 'message' => "File read: $path"];
            } else {
                $response = ['status' => 'error', 'message' => "File not found: $path"];
            }
            break;

        case 'deldir': 
            if (is_dir($path)) {
                delTree($path);
                $response = ['status' => 'success', 'message' => "Dir deleted: $path"];
            }
            break;

        case 'delfile': 
            if (file_exists($path)) {
                unlink($path);
                $response = ['status' => 'success', 'message' => "File deleted: $path"];
            }
            break;

        case 'listdir': // ВОССТАНОВЛЕНО: Чтение списка проектов
            if (is_dir($path)) {
                $dirs = array_filter(glob($path . '/*'), 'is_dir');
                $dirs = array_map('basename', $dirs);
                $response = ['status' => 'success', 'data' => array_values($dirs)];
            } else {
                $response = ['status' => 'error', 'message' => 'Directory not found'];
            }
            break;
    }
} catch (Exception $e) {
    $response = ['status' => 'error', 'message' => $e->getMessage()];
}

echo json_encode($response);

// --- Вспомогательные функции ---
function recurseCopy($src, $dst) {
    $dir = opendir($src);
    @mkdir($dst);
    while (false !== ($file = readdir($dir))) {
        if (($file != '.') && ($file != '..')) {
            if (is_dir($src . '/' . $file)) {
                recurseCopy($src . '/' . $file, $dst . '/' . $file);
            } else {
                copy($src . '/' . $file, $dst . '/' . $file);
            }
        }
    }
    closedir($dir);
}

function delTree($dir) {
    $files = array_diff(scandir($dir), array('.', '..'));
    foreach ($files as $file) {
        (is_dir("$dir/$file")) ? delTree("$dir/$file") : @unlink("$dir/$file");
    }
    return @rmdir($dir);
}
?>
