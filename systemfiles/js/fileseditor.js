const API_URL = '../phps/backend.php'; 

async function sendRequest(data) {
    try {
        const response = await fetch(API_URL, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(data)
        });
        const result = await response.json();
        console.log(result.message || "");
        return result;
    } catch (error) {
        console.error('Error:', error);
        return { status: 'error', message: error.toString() };
    }
}

// Теперь все функции возвращают Promise, чтобы их можно было ждать через await

function mdfile(path, content) {
    return sendRequest({ action: 'mdfile', path: path, content: content });
}

function readfile(path) {
    return sendRequest({ action: 'readfile', path: path });
}

function makedir(path) {
    return sendRequest({ action: 'makedir', path: path });
}

function copydir(source, destination, inside = true) {
    return sendRequest({ action: 'copydir', path: source, dest: destination, inside: inside });
}

function copyfile(source, destination) {
    return sendRequest({ action: 'copyfile', path: source, dest: destination });
}


function runfile(path, args = '', wait = false) {
    return sendRequest({ action: 'runfile', path: path, content: args, wait: wait });
}

function copyfile(source, destination) {
    return sendRequest({ action: 'copyfile', path: source, dest: destination });
}
function deldir(path) {
    return sendRequest({ action: 'deldir', path: path });
}

function delfile(path) {
    return sendRequest({ action: 'delfile', path: path });
}

function listdir(path) {
    return sendRequest({ action: 'listdir', path: path });
}



function getpathfromfile(ext, command){
    fetch("../phps/getfile.php?ext=" + encodeURIComponent(ext))
    .then(r => r.text())
    .then(path => {
        let gettedpath = path.trim();
        eval(command);
    });
}

function getpathfromselfd(command){
    fetch("../phps/getfolder.php")
    .then(r => r.text())
    .then(path => {
        let gettedpath = path.trim();
        eval(command);
    });
}


// Вспомогательные функции для работы с API архивации
function unzip(path, dest) {
    return sendRequest({ action: 'unzip', path: path, dest: dest });
}

function zipFolder(path, destFile) {
    return sendRequest({ action: 'zip', path: path, dest: destFile });
}

// --- ФУНКЦИЯ ИМПОРТА ---
async function importVuprojFile() {
    getpathfromfile('vuproj', async function(fileToImport) {
        if (!fileToImport) return;

        const tempDir = "C:\\vucapp\\temp_import_" + Date.now();
        await makedir(tempDir);

        // 1. Распаковка во временную папку
        const unzipRes = await unzip(fileToImport, tempDir);
        if (unzipRes.status === 'error') {
            alert("Ошибка распаковки архива.");
            return;
        }

        // 2. Чтение info.cfg
        const infoRes = await readfile(tempDir + "\\info.cfg");
        if (infoRes.status === 'error') {
            alert("Файл конфигурации info.cfg не найден внутри .vuproj");
            await deldir(tempDir);
            return;
        }

        // Парсинг конфига
        const config = {};
        infoRes.data.split('\n').forEach(line => {
            const [key, val] = line.split('=');
            if (key && val) config[key.trim()] = val.trim();
        });

        const { name, bundle, opej } = config;

        // 3. Диалог подтверждения
        if (confirm(`Вы действительно хотите импортировать проект ${name} в список ваших проектов?\n\nВыбрано: ${opej}\nБудет добавлен проект: ${bundle}`)) {
            
            const targetPath = `../../projects/${bundle}`;
            
            // Проверка существования папки
            const checkDir = await listdir(targetPath);
            if (checkDir.status !== 'error') {
                if (!confirm(`У вас существует папка ${bundle} в проектах!\nВы хотите заменить её новым проектом?`)) {
                    await deldir(tempDir);
                    return;
                }
                await deldir(targetPath);
            }

            // 4. Копирование core в папку проектов
            await makedir(targetPath);
            await copydir(tempDir + "\\core", targetPath, true);
            
            alert("Проект успешно импортирован!");
            showOpenDialog(); // Обновить список
        }

        // Очистка
        await deldir(tempDir);
    });
}

// --- ФУНКЦИЯ ЭКСПОРТА ---
async function exportProjectAsVuproj() {
    if (!dfilename) return alert("Проект не открыт!");

    // Используем диалог сохранения (через prompt для примера имени, либо вашу функцию выбора пути)
    const saveName = prompt("Введите имя для сохранения файла (без расширения):", dfilename);
    if (!saveName) return;

    // Запрашиваем папку, куда сохранить (используем вашу функцию выбора папки)
    getpathfromselfd(async function(selectedFolder) {
        if (!selectedFolder) return;

        const fullDestFile = selectedFolder + "\\" + saveName + ".vuproj";
        const tempExportDir = "C:\\vucapp\\temp_export_" + Date.now();
        const coreDir = tempExportDir + "\\core";

        // 1. Подготовка структуры
        await makedir(tempExportDir);
        await makedir(coreDir);

        // 2. Копируем текущий проект в core
        await copydir(`../../projects/${dfilename}`, coreDir, true);

        // 3. Создаем info.cfg
        // Данные берем из глобальных переменных IDE
        const infoContent = `name=${dnameapp}\nbundle=${dfilename}\nopej=${saveName}.vuproj`;
        await mdfile(tempExportDir + "\\info.cfg", infoContent);

        // 4. Архивация
        const zipRes = await zipFolder(tempExportDir, fullDestFile);
        
        if (zipRes.status === 'success') {
            alert("Проект успешно сохранен как: " + fullDestFile);
        } else {
            alert("Ошибка при создании архива: " + zipRes.message);
        }

        // 5. Очистка
        await deldir(tempExportDir);
    });
}



// ==========================================
// ЛОГИКА ЗАКРЫТИЯ ЗАПУЩЕННОГО ТЕСТОВОГО ПРОЦЕССА
// ==========================================
async function closeProcess() {
    if (!dfilename) {
        alert("Проект не открыт!");
        return;
    }

    // Путь к временному скрипту в папке проекта
    const batPath = `../../projects/${dfilename}/killtest.bat`;
    
    // Содержимое скрипта из вашего ТЗ
    const batContent = `@echo off\ntaskkill /f /im test.exe\nexit`;

    try {
        // 1. Создаем .bat файл
        await mdfile(batPath, batContent);
        
        // 2. Запускаем его в скрытом режиме с ожиданием завершения
        await runfile(batPath, '', true);
        
        // 3. Удаляем .bat файл за собой
        await delfile(batPath);
        
        // 4. (Опционально) Сбрасываем иконку компиляции на тулбаре, 
        // если программа зависла и "спиннер" все еще крутится
        document.getElementById('compileIcon').textContent = "";
        document.getElementById('compileandrunbutton').disabled = false;
        document.querySelector('#spinball').style.display = "none";
        
        console.log("Процесс test.exe успешно завершен.");
    } catch (e) {
        console.error("Ошибка при попытке закрыть процесс: ", e);
        alert("Не удалось завершить процесс test.exe");
    }
}


async function closePProcess() {
    if (!dfilename) {
        alert("Проект не открыт!");
        return;
    }

    // Путь к временному скрипту в папке проекта
    const batPath = `../../projects/${dfilename}/killprocess.bat`;
    // Содержимое скрипта из вашего ТЗ
    const batContent = `@echo off\ntaskkill /f /im test.exe\ntaskkill /f /im ${dfilename}.exe\nexit`;

    try {
        // 1. Создаем .bat файл
        await mdfile(batPath, batContent);
        
        // 2. Запускаем его в скрытом режиме с ожиданием завершения
        await runfile(batPath, '', true);
        
        // 3. Удаляем .bat файл за собой
        await delfile(batPath);
        
        // 4. (Опционально) Сбрасываем иконку компиляции на тулбаре, 
        // если программа зависла и "спиннер" все еще крутится
        document.getElementById('compileIcon').textContent = "";
        document.getElementById('compileandrunbutton').disabled = false;
        document.querySelector('#spinball').style.display = "none";
        
        console.log("Процесс test.exe успешно завершен.");
    } catch (e) {
        console.error("Ошибка при попытке закрыть процесс: ", e);
        alert("Не удалось завершить процесс test.exe");
    }
}
