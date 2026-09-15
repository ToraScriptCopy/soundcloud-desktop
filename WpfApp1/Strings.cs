using System.Collections.Generic;
using System.Globalization;

namespace WpfApp1
{
    // Strings for 10 languages, no resx needed.
    // Array order: ru, en, uk, zh, de, fr, es, pl, tr, it.
    public static class Loc
    {
        public static readonly string[] Codes =
            new string[] { "ru", "en", "uk", "zh", "de", "fr", "es", "pl", "tr", "it" };
        public static readonly string[] Names =
            new string[] { "Русский", "English", "Українська", "中文", "Deutsch",
                "Français", "Español", "Polski", "Türkçe", "Italiano" };
        public static readonly string[] PrefCodes =
            new string[] { "auto", "ru", "en", "uk", "zh", "de", "fr", "es", "pl", "tr", "it" };

        public static string Current = "en";

        private static readonly Dictionary<string, string[]> T = new Dictionary<string, string[]>
        {
            { "NavHome", new string[] { "Главная", "Home", "Головна", "首页", "Start", "Accueil", "Inicio", "Strona główna", "Ana Sayfa", "Home" } },
            { "NavCharts", new string[] { "Чарты", "Charts", "Чарти", "排行榜", "Charts", "Classements", "Listas", "Listy przebojów", "Listeler", "Classifiche" } },
            { "NavLikes", new string[] { "Мои лайки", "My likes", "Вподобайки", "我的喜欢", "Meine Likes", "Mes likes", "Mis me gusta", "Polubione", "Beğeniler", "Mi piace" } },
            { "AddrPh", new string[] { "URL или поиск по SoundCloud…", "URL or search SoundCloud…", "URL або пошук SoundCloud…", "网址或搜索 SoundCloud…", "URL oder SoundCloud durchsuchen…", "URL ou recherche SoundCloud…", "URL o buscar en SoundCloud…", "URL lub wyszukiwanie SoundCloud…", "URL veya SoundCloud'da ara…", "URL o cerca su SoundCloud…" } },
            { "Go", new string[] { "Перейти", "Go", "Перейти", "前往", "Los", "Aller", "Ir", "Idź", "Git", "Vai" } },
            { "PipGroup", new string[] { "PiP-плеер", "PiP player", "PiP-плеєр", "PiP 播放器", "PiP-Player", "Lecteur PiP", "Reproductor PiP", "Odtwarzacz PiP", "PiP oynatıcı", "Lettore PiP" } },
            { "PlaylistPh", new string[] { "Ссылка на плейлист или трек…", "Playlist or track link…", "Посилання на плейлист або трек…", "粘贴播放列表或单曲链接…", "Playlist- oder Track-Link…", "Lien playlist ou titre…", "Enlace de playlist o tema…", "Link do playlisty lub utworu…", "Çalma listesi veya parça bağlantısı…", "Link playlist o brano…" } },
            { "PipPlay", new string[] { "PiP ▶ играть", "PiP ▶ play", "PiP ▶ грати", "PiP ▶ 播放", "PiP ▶ abspielen", "PiP ▶ lire", "PiP ▶ reproducir", "PiP ▶ odtwarzaj", "PiP ▶ oynat", "PiP ▶ riproduci" } },
            { "PipOpen", new string[] { "PiP открыть", "Open PiP", "Відкрити PiP", "打开 PiP", "PiP öffnen", "Ouvrir PiP", "Abrir PiP", "Otwórz PiP", "PiP aç", "Apri PiP" } },
            { "PipHint", new string[] { "Откроется справа внизу. Таскается за шапку.", "Opens at bottom right. Drag it by the title bar.", "Відкриється справа внизу. Тягається за шапку.", "将在右下角打开，可拖动标题栏移动。", "Öffnet sich rechts unten. Per Titelleiste ziehen.", "S'ouvre en bas à droite. Glisser par la barre.", "Se abre abajo a la derecha. Arrastra por la barra.", "Otwiera się na dole po prawej. Przeciągnij za pasek.", "Sağ altta açılır. Başlık çubuğundan sürükle.", "Si apre in basso a destra. Trascina dalla barra." } },
            { "Settings", new string[] { "Настройки", "Settings", "Налаштування", "设置", "Einstellungen", "Paramètres", "Ajustes", "Ustawienia", "Ayarlar", "Impostazioni" } },
            { "LangLabel", new string[] { "Язык", "Language", "Мова", "语言", "Sprache", "Langue", "Idioma", "Język", "Dil", "Lingua" } },
            { "LangAuto", new string[] { "Авто (язык системы)", "Auto (system language)", "Авто (мова системи)", "自动（系统语言）", "Auto (Systemsprache)", "Auto (langue système)", "Auto (idioma del sistema)", "Auto (język systemu)", "Otomatik (sistem dili)", "Auto (lingua di sistema)" } },
            { "HideHeader", new string[] { "Скрывать шапку сайта", "Hide site header", "Приховати шапку сайту", "隐藏网站顶栏", "Website-Kopf ausblenden", "Masquer l'en-tête du site", "Ocultar cabecera del sitio", "Ukryj nagłówek strony", "Site başlığını gizle", "Nascondi intestazione sito" } },
            { "TipBack", new string[] { "Назад", "Back", "Назад", "后退", "Zurück", "Retour", "Atrás", "Wstecz", "Geri", "Indietro" } },
            { "TipFwd", new string[] { "Вперёд", "Forward", "Вперед", "前进", "Vor", "Suivant", "Adelante", "Dalej", "İleri", "Avanti" } },
            { "TipReload", new string[] { "Обновить", "Reload", "Оновити", "刷新", "Neu laden", "Recharger", "Recargar", "Odśwież", "Yenile", "Ricarica" } },
            { "TipPipHere", new string[] { "PiP для текущей страницы", "PiP for current page", "PiP для поточної сторінки", "当前页面的 PiP", "PiP für aktuelle Seite", "PiP pour la page", "PiP para la página", "PiP dla strony", "Sayfa için PiP", "PiP per la pagina" } },
            { "TipPrev", new string[] { "Предыдущий трек", "Previous track", "Попередній трек", "上一首", "Vorheriger Titel", "Titre précédent", "Tema anterior", "Poprzedni utwór", "Önceki parça", "Brano precedente" } },
            { "TipPlay", new string[] { "Играть / пауза", "Play / pause", "Грати / пауза", "播放 / 暂停", "Abspielen / Pause", "Lecture / pause", "Reproducir / pausar", "Odtwórz / pauza", "Oynat / duraklat", "Riproduci / pausa" } },
            { "TipNext", new string[] { "Следующий трек", "Next track", "Наступний трек", "下一首", "Nächster Titel", "Titre suivant", "Tema siguiente", "Następny utwór", "Sonraki parça", "Brano successivo" } },
            { "VolCap", new string[] { "Громкость", "Volume", "Гучність", "音量", "Lautstärke", "Volume", "Volumen", "Głośność", "Ses", "Volume" } },
            { "Mute", new string[] { "Мут", "Mute", "Мут", "静音", "Stumm", "Muet", "Silenciar", "Wycisz", "Sessiz", "Muto" } },
            { "IdleTrack", new string[] { "Ничего не играет", "Nothing playing", "Нічого не грає", "当前未播放", "Nichts spielt", "Rien en lecture", "Nada en reproducción", "Nic nie gra", "Çalan bir şey yok", "Nulla in riproduzione" } },
            { "NeedLink", new string[] { "Вставь ссылку на плейлист или трек.", "Paste a playlist or track link first.", "Вставте посилання на плейлист або трек.", "请先粘贴播放列表或单曲链接。", "Füge zuerst einen Playlist- oder Track-Link ein.", "Colle d'abord un lien playlist ou titre.", "Pega primero un enlace de playlist o tema.", "Najpierw wklej link do playlisty lub utworu.", "Önce bir çalma listesi veya parça bağlantısı yapıştır.", "Prima incolla un link playlist o brano." } },
            { "NoControl", new string[] { "Не нашёл кнопки плеера на странице.", "Player buttons not found on the page.", "Не знайшов кнопок плеєра на сторінці.", "在页面上找不到播放器按钮。", "Player-Schaltflächen auf der Seite nicht gefunden.", "Boutons introuvables sur la page.", "Botones no encontrados en la página.", "Nie znaleziono przycisków na stronie.", "Sayfada oynatıcı düğmeleri bulunamadı.", "Pulsanti non trovati nella pagina." } },
            { "ErrTitle", new string[] { "Ошибка", "Error", "Помилка", "错误", "Fehler", "Erreur", "Error", "Błąd", "Hata", "Errore" } },
            { "VaultBroken", new string[] { "Локальное хранилище повреждено — настройки сброшены.", "Local storage corrupted — settings reset.", "Локальне сховище пошкоджено — налаштування скинуто.", "本地存储已损坏，设置已重置。", "Lokaler Speicher beschädigt — Einstellungen zurückgesetzt.", "Stockage local corrompu — réinitialisé.", "Almacén local dañado — restablecido.", "Magazyn uszkodzony — zresetowano.", "Yerel depolama bozuk — sıfırlandı.", "Archivio locale danneggiato — resettato." } },
            { "WebMsg", new string[] { "Не удалось запустить движок браузера. Проверь WebView2 Runtime.", "Browser engine failed to start. Check WebView2 Runtime.", "Не вдалося запустити рушій браузера. Перевір WebView2 Runtime.", "浏览器引擎启动失败，请检查 WebView2 Runtime。", "Browser-Engine konnte nicht gestartet werden. Prüfe WebView2 Runtime.", "Moteur introuvable. Vérifie WebView2 Runtime.", "No se pudo iniciar el motor. Revisa WebView2 Runtime.", "Nie można uruchomić silnika. Sprawdź WebView2 Runtime.", "Motor başlatılamadı. WebView2 Runtime'u kontrol et.", "Avvio motore fallito. Controlla WebView2 Runtime." } },
            { "PipTitle", new string[] { "PiP-плеер", "PiP player", "PiP-плеєр", "PiP 播放器", "PiP-Player", "Lecteur PiP", "Reproductor PiP", "Odtwarzacz PiP", "PiP oynatıcı", "Lettore PiP" } },
            { "TipClose", new string[] { "Закрыть", "Close", "Закрити", "关闭", "Schließen", "Fermer", "Cerrar", "Zamknij", "Kapat", "Chiudi" } },
            { "MenuTip", new string[] { "Панель", "Sidebar", "Панель", "侧边栏", "Seitenleiste", "Panneau", "Panel", "Panel", "Panel", "Pannello" } },
            { "SettingsTitle", new string[] { "Настройки", "Settings", "Налаштування", "设置", "Einstellungen", "Paramètres", "Ajustes", "Ustawienia", "Ayarlar", "Impostazioni" } },
            { "BindsGroup", new string[] { "Горячие клавиши (numpad)", "Hotkeys (numpad)", "Гарячі клавіші (numpad)", "快捷键（数字键盘）", "Tastenkürzel (Ziffernblock)", "Raccourcis (pavé num.)", "Atajos (teclado num.)", "Skróty (klawiatura num.)", "Kısayollar (sayı tuşları)", "Scorciatoie (tastierino)" } },
            { "HotPrev", new string[] { "Назад", "Previous", "Назад", "上一首", "Zurück", "Précédent", "Anterior", "Poprzedni", "Önceki", "Precedente" } },
            { "HotPlay", new string[] { "Играть / пауза", "Play / pause", "Грати / пауза", "播放 / 暂停", "Play / Pause", "Lecture / pause", "Reproducir / pausar", "Odtwarzaj / pauza", "Oynat / duraklat", "Riproduci / pausa" } },
            { "HotNext", new string[] { "Вперёд", "Next", "Вперед", "下一首", "Weiter", "Suivant", "Siguiente", "Następny", "Sonraki", "Successivo" } },
            { "HotVolDn", new string[] { "Тише −5%", "Volume −5%", "Тихіше −5%", "音量 −5%", "Leiser −5%", "Moins fort −5%", "Bajar −5%", "Ciszej −5%", "Sesi kıs −5%", "Abbassa −5%" } },
            { "HotVolUp", new string[] { "Громче +5%", "Volume +5%", "Гучніше +5%", "音量 +5%", "Lauter +5%", "Plus fort +5%", "Subir +5%", "Głośniej +5%", "Sesi aç +5%", "Alza +5%" } },
            { "KeyOff", new string[] { "Выкл", "Off", "Вимк", "关闭", "Aus", "Off", "Off", "Wył", "Kapalı", "Off" } },
            { "HotHint", new string[] { "Работают, даже когда окно не в фокусе.", "Work even when the window is not focused.", "Працюють, навіть коли вікно не у фокусі.", "即使窗口未聚焦也能使用。", "Funktionieren auch ohne Fensterfokus.", "Fonctionnent même sans focus.", "Funcionan sin foco.", "Działają bez fokusu.", "Odak yokken bile çalışır.", "Funzionano anche senza focus." } },
            { "CacheBtn", new string[] { "Очистить кэш браузера", "Clear browser cache", "Очистити кеш браузера", "清除浏览器缓存", "Browser-Cache leeren", "Vider le cache", "Borrar caché", "Wyczyść pamięć podręczną", "Önbelleği temizle", "Svuota cache" } },
            { "CacheDone", new string[] { "Кэш очищен.", "Cache cleared.", "Кеш очищено.", "缓存已清除。", "Cache geleert.", "Cache vidé.", "Caché borrada.", "Wyczyszczono.", "Önbellek temizlendi.", "Cache svuotata." } },
            { "VaultResetBtn", new string[] { "Сбросить настройки", "Reset settings", "Скинути налаштування", "重置设置", "Einstellungen zurücksetzen", "Réinitialiser", "Restablecer", "Resetuj ustawienia", "Ayarları sıfırla", "Resettato" } },
            { "VaultResetDone", new string[] { "Настройки сброшены.", "Settings reset.", "Налаштування скинуто.", "设置已重置。", "Einstellungen zurückgesetzt.", "Réinitialisé.", "Restablecido.", "Zresetowano.", "Sıfırlandı.", "Resettato." } },
            { "TopmostMain", new string[] { "Поверх всех окон", "Always on top", "Поверх усіх вікон", "窗口置顶", "Immer im Vordergrund", "Toujours visible", "Siempre visible", "Zawsze na wierzchu", "Her zaman üstte", "Sempre in primo piano" } },
            { "CopyLink", new string[] { "Скопировать ссылку", "Copy link", "Копіювати посилання", "复制链接", "Link kopieren", "Copier le lien", "Copiar enlace", "Kopiuj link", "Bağlantıyı kopyala", "Copia link" } },
            { "OpenExt", new string[] { "Открыть в браузере", "Open in browser", "Відкрити в браузері", "在浏览器中打开", "Im Browser öffnen", "Ouvrir dans navigateur", "Abrir en navegador", "Otwórz w przeglądarce", "Tarayıcıda aç", "Apri nel browser" } },
            { "LinkCopied", new string[] { "Ссылка скопирована.", "Link copied.", "Посилання скопійовано.", "链接已复制。", "Link kopiert.", "Lien copié.", "Enlace copiado.", "Skopiowano.", "Bağlantı kopyalandı.", "Link copiato." } },
            { "DefaultsBtn", new string[] { "Сбросить бинды", "Reset binds", "Скинути бінди", "重置快捷键", "Kürzel zurücksetzen", "Reset raccourcis", "Restablecer atajos", "Resetuj skróty", "Kısayolları sıfırla", "Resettato" } },
            { "ThemeLabel", new string[] { "Тема оформления", "Theme", "Тема", "主题", "Design", "Thème", "Tema", "Motyw", "Tema", "Tema" } },
            { "Theme0", new string[] { "Системная", "System", "Системна", "跟随系统", "System", "Système", "Sistema", "Systemowy", "Sistem", "Sistema" } },
            { "Theme1", new string[] { "Тёмная", "Dark", "Темна", "深色", "Dunkel", "Sombre", "Oscuro", "Ciemny", "Koyu", "Scuro" } },
            { "Theme2", new string[] { "Светлая", "Light", "Світла", "浅色", "Hell", "Clair", "Claro", "Jasny", "Açık", "Chiaro" } },
            { "Theme3", new string[] { "Оранжевая", "Orange", "Помаранчева", "橙色", "Orange", "Orange", "Naranja", "Pomarańczowy", "Turuncu", "Arancione" } },
            { "Theme4", new string[] { "Зелёная", "Green", "Зелена", "绿色", "Grün", "Vert", "Verde", "Zielony", "Yeşil", "Verde" } },
            { "Theme5", new string[] { "Красная", "Red", "Червона", "红色", "Rot", "Rouge", "Rojo", "Czerwony", "Kırmızı", "Rosso" } },
            { "Theme6", new string[] { "Синяя", "Blue", "Синя", "蓝色", "Blau", "Bleu", "Azul", "Niebieski", "Mavi", "Blu" } },
            { "Theme7", new string[] { "Фиолетовая", "Purple", "Фіолетова", "紫色", "Lila", "Violet", "Morado", "Fioletowy", "Mor", "Viola" } },
            { "Theme8", new string[] { "Розовая", "Pink", "Рожева", "粉色", "Rosa", "Rose", "Rosa", "Różowy", "Pembe", "Rosa" } },
            { "Theme9", new string[] { "Бирюзовая", "Teal", "Бірюзова", "青色", "Petrol", "Sarcelle", "Turquesa", "Morski", "Turkuaz", "Teal" } },
            { "TrayOpen", new string[] { "Открыть", "Open", "Відкрити", "打开", "Öffnen", "Ouvrir", "Abrir", "Otwórz", "Aç", "Apri" } },
            { "TrayExit", new string[] { "Выход", "Exit", "Вихід", "退出", "Beenden", "Quitter", "Salir", "Wyjdź", "Çık", "Esci" } },
            { "TrayHide", new string[] { "Сворачивать в трей", "Minimize to tray", "Згортати в трей", "最小化到托盘", "In Tray minimieren", "Réduire dans la barre", "Minimizar a bandeja", "Minimalizuj do zasobnika", "Tepsiye küçült", "Riduci nel tray" } },
            { "Autostart", new string[] { "Автозапуск с Windows", "Start with Windows", "Автозапуск із Windows", "开机自启动", "Mit Windows starten", "Démarrer avec Windows", "Iniciar con Windows", "Uruchamiaj z Windows", "Windows ile başlat", "Avvia con Windows" } },
            { "AdBlockLbl", new string[] { "Блокировка рекламы", "Ad blocking", "Блокування реклами", "广告拦截", "Werbeblocker", "Blocage pub", "Bloqueo de anuncios", "Blokowanie reklam", "Reklam engelleme", "Blocco annunci" } },
            { "StrictBlock", new string[] { "Строгая (режет и трекеры)", "Strict (blocks trackers too)", "Строга (ріже й трекери)", "严格（拦截追踪器）", "Streng (auch Tracker)", "Strict (anti-trackeurs)", "Estricto (anti-rastreo)", "Surowy (tnie trackery)", "Sıkı (izleyiciler dahil)", "Stretto (anti-tracker)" } },
            { "StrictHint", new string[] { "Может ломать вход через Google/Facebook.", "May break Google/Facebook login.", "Може ламати вхід через Google/Facebook.", "可能导致谷歌/脸书登录失败。", "Kann Google/Facebook-Login stören.", "Peut casser la connexion Google/Facebook.", "Puede romper el login con Google/Facebook.", "Może psuć logowanie Google/Facebook.", "Google/Facebook girişini bozabilir.", "Può rompere il login Google/Facebook." } },
            { "SetupTitle", new string[] { "Установка", "Setup", "Встановлення", "安装", "Setup", "Installation", "Instalación", "Instalator", "Kurulum", "Installazione" } },
            { "SetupLangLbl", new string[] { "Язык установки", "Setup language", "Мова встановлення", "安装语言", "Setup-Sprache", "Langue d'installation", "Idioma de instalación", "Język instalatora", "Kurulum dili", "Lingua di installazione" } },
            { "SetupNext", new string[] { "Далее", "Next", "Далі", "下一步", "Weiter", "Suivant", "Siguiente", "Dalej", "İleri", "Avanti" } },
            { "SetupBack", new string[] { "Назад", "Back", "Назад", "上一步", "Zurück", "Retour", "Atrás", "Wstecz", "Geri", "Indietro" } },
            { "SetupInstall", new string[] { "Установить", "Install", "Встановити", "安装", "Installieren", "Installer", "Instalar", "Zainstaluj", "Kur", "Installa" } },
            { "SetupBrowse", new string[] { "Обзор…", "Browse…", "Огляд…", "浏览…", "Durchsuchen…", "Parcourir…", "Examinar…", "Przeglądaj…", "Gözat…", "Sfoglia…" } },
            { "SetupPathLbl", new string[] { "Папка установки", "Install folder", "Папка встановлення", "安装目录", "Installationsordner", "Dossier d'installation", "Carpeta de instalación", "Folder instalacji", "Kurulum klasörü", "Cartella di installazione" } },
            { "SetupDesktopOpt", new string[] { "Ярлык на рабочем столе", "Desktop shortcut", "Ярлик на робочому столі", "桌面快捷方式", "Desktop-Verknüpfung", "Raccourci bureau", "Acceso directo", "Skrót na pulpicie", "Masaüstü kısayolu", "Collegamento desktop" } },
            { "SetupStartOpt", new string[] { "Ярлык в меню Пуск", "Start menu shortcut", "Ярлик у меню Пуск", "开始菜单快捷方式", "Startmenü-Eintrag", "Raccourci Démarrer", "Acceso menú inicio", "Skrót w menu Start", "Başlat menüsü kısayolu", "Collegamento Start" } },
            { "SetupLaunchOpt", new string[] { "Запустить после установки", "Launch after install", "Запустити після встановлення", "安装后启动", "Nach Installation starten", "Lancer après install", "Ejecutar tras instalar", "Uruchom po instalacji", "Kurulumdan sonra başlat", "Avvia dopo installazione" } },
            { "SetupInstalling", new string[] { "Копирование файлов…", "Copying files…", "Копіювання файлів…", "正在复制文件…", "Dateien werden kopiert…", "Copie des fichiers…", "Copiando archivos…", "Kopiowanie plików…", "Dosyalar kopyalanıyor…", "Copia dei file…" } },
            { "SetupDone", new string[] { "Готово! Программа установлена.", "Done! Installed.", "Готово! Встановлено.", "完成！已安装。", "Fertig! Installiert.", "Terminé ! Installé.", "¡Listo! Instalado.", "Gotowe! Zainstalowano.", "Bitti! Kuruldu.", "Fatto! Installato." } },
            { "SetupClose", new string[] { "Закрыть", "Close", "Закрити", "关闭", "Schließen", "Fermer", "Cerrar", "Zamknij", "Kapat", "Chiudi" } },
            { "SetupWebViewMsg", new string[] { "Не найден WebView2 Runtime — без него сайт не заведётся.", "WebView2 Runtime missing — the app needs it.", "Не знайдено WebView2 Runtime — без нього ніяк.", "未找到 WebView2 Runtime，程序需要它。", "WebView2 Runtime fehlt — wird benötigt.", "WebView2 Runtime manquant — requis.", "Falta WebView2 Runtime — necesario.", "Brak WebView2 Runtime — wymagany.", "WebView2 Runtime eksik — gerekli.", "WebView2 Runtime mancante — necessario." } },
            { "SetupGetWebView", new string[] { "Скачать WebView2", "Get WebView2", "Завантажити WebView2", "下载 WebView2", "WebView2 laden", "Télécharger WebView2", "Descargar WebView2", "Pobierz WebView2", "WebView2 indir", "Scarica WebView2" } },
            { "SetupUninstDone", new string[] { "Программа удалена.", "Uninstalled.", "Видалено.", "已卸载。", "Deinstalliert.", "Désinstallé.", "Desinstalado.", "Odinstalowano.", "Kaldırıldı.", "Disinstallato." } },
        };

        public static string Get(string key)
        {
            string[] arr;
            if (!T.TryGetValue(key, out arr)) return key;
            int i = IndexOf(Current);
            if (i < 0 || i >= arr.Length) i = 1;
            return arr[i];
        }

        public static int IndexOf(string code)
        {
            if (code == null) return -1;
            for (int i = 0; i < Codes.Length; i++)
                if (Codes[i] == code) return i;
            return -1;
        }

        // Preference -> system language -> English.
        public static string Resolve(string pref)
        {
            if (!string.IsNullOrEmpty(pref) && pref != "auto" && IndexOf(pref) >= 0)
                return pref;
            try
            {
                string iso = CultureInfo.InstalledUICulture.TwoLetterISOLanguageName.ToLowerInvariant();
                if (IndexOf(iso) >= 0) return iso;
            }
            catch { }
            return "en";
        }

        public static void FillLangCombo(System.Windows.Controls.ComboBox box, string current)
        {
            box.Items.Clear();
            box.Items.Add(Get("LangAuto"));
            for (int i = 0; i < Names.Length; i++) box.Items.Add(Names[i]);
            int sel = 0;
            for (int i = 0; i < PrefCodes.Length; i++)
                if (PrefCodes[i] == current) sel = i;
            box.SelectedIndex = sel;
        }

        public static readonly int[] KeyChoices = new int[]
        {
            0,
            0x60, 0x61, 0x62, 0x63, 0x64, 0x65, 0x66, 0x67, 0x68, 0x69,
            0x70, 0x71, 0x72, 0x73, 0x74, 0x75, 0x76, 0x77, 0x78, 0x79, 0x7A, 0x7B
        };

        public static string KeyName(int vk)
        {
            if (vk == 0) return Get("KeyOff");
            if (vk >= 0x60 && vk <= 0x69) return "Num" + (vk - 0x60);
            if (vk >= 0x70 && vk <= 0x7B) return "F" + (vk - 0x6F);
            return "VK" + vk;
        }

        public static void FillKeyCombo(System.Windows.Controls.ComboBox box, int current)
        {
            box.Items.Clear();
            int sel = 0;
            for (int i = 0; i < KeyChoices.Length; i++)
            {
                box.Items.Add(KeyName(KeyChoices[i]));
                if (KeyChoices[i] == current) sel = i;
            }
            box.SelectedIndex = sel;
        }
    }
}
