<?php
header('Content-Type: application/json; charset=utf-8');

$nugetDir = "C:/vucapp/nugets/";
$assetsFile = "C:/vucapp/systemfiles/useapp/AppGenerated/obj/project.assets.json";

if (!is_dir($nugetDir)) {
    echo json_encode(["status" => "error", "message" => "Папка nugets не найдена"]);
    exit;
}

$installedVersions = [];
if (file_exists($assetsFile)) {
    $json = json_decode(file_get_contents($assetsFile), true);
    if (isset($json['libraries'])) {
        foreach ($json['libraries'] as $key => $val) {
            $parts = explode('/', $key);
            if (count($parts) == 2) {
                $installedVersions[strtolower($parts[0])] = $parts[1];
            }
        }
    }
}

$files = scandir($nugetDir);
$packages = [];

foreach ($files as $file) {
    if (strtolower(pathinfo($file, PATHINFO_EXTENSION)) === 'nupkg') {
        // Улучшенная регулярка для захвата имени и версии (включая превью версии)
        if (preg_match('/^(.+?)\.((?:\d+\.)+\d+(?:-[\w\.]+)?)\.nupkg$/i', $file, $matches)) {
            $name = $matches[1];
            $version = $matches[2];
            $lowerName = strtolower($name);

            $isOutdated = false;
            if (isset($installedVersions[$lowerName])) {
                // Если версия в папке не совпадает с той, что выбрал NuGet для проекта
                if ($installedVersions[$lowerName] !== $version) {
                    $isOutdated = true;
                }
            }

            $packages[] = [
                "name" => $name,
                "version" => $version,
                "outdated" => $isOutdated
            ];
        }
    }
}

echo json_encode(["status" => "success", "data" => $packages]);