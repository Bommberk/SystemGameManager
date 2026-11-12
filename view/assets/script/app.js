loadSidebar();
loadPage("gamemanager");


// ***************************** //
// ******** GameManager ******** //
// ***************************** //
// Handle C# Responses
function handleGames()
{
    const gameAmount = document.getElementById("gameAmount");
    if (!gameAmount) return;
    gameAmount.textContent = games.filter(game => !game.IsRemovedFromView).length;
    createGameList();
}

function handleLaunchers()
{
    const launcherAmount = document.getElementById("launcherAmount");
    if (!launcherAmount) return;
    launcherAmount.textContent = launchers.length;
    createLauncherList();
}
let audioDevices = [];
function handleAudioDevices(devices)
{
    audioDevices = devices;
    fillHotKeyDevices();
    const audioDeviceSelection = document.getElementById("audioOutputDevice");
    if (!audioDeviceSelection) return;
    // audioDeviceSelection.innerHTML = "";
    devices.forEach(device => {
        const option = document.createElement("option");
        option.value = device;
        option.textContent = device;
        audioDeviceSelection.appendChild(option);
    });
}
// Screens
let screens = [];
function handleScreens(screenData)
{
    screens = screenData;
    console.log(screens);
}
function getScreens()
{
    return screens;
}
function startOpenWallpaperScreenModal(serializedGameName){
    api.getScreenForGame(getGameBySerializedName(serializedGameName));
}

function createLauncherList()
{
    const launcherList = document.getElementById("launcherlist");
    launcherList.innerHTML = "";
    launchers.forEach(launcher => {
        const launcherCard = document.createElement("div");
        launcherCard.className = "launcher card";
        let logoPath = `../assets/images/launcher_logos/${launcher.SearchName}-logo.png`;
        launcherCard.innerHTML = `
            <img src="${logoPath}" alt="${launcher.Name} logo" onerror="this.src='../assets/images/launcher_logos/placeholder-logo.png';">
            <div class="infos">
                <h3>${launcher.Name}</h3>
                <p class="installpath">${launcher.InstallPath}</p>
            </div>
        `;
        launcherList.appendChild(launcherCard);
    });
}

function createCollapsableSections()
{
    console.log("Creating collapsable sections");
    const collapsableHeader = document.querySelectorAll("section.collapsable h2");
    console.log("Found collapsable sections:", collapsableHeader);
    collapsableHeader.forEach(header => {
        console.log("Processing collapsable section:", header);
        const dropIcon = '<i class="fa-solid fa-chevron-down"></i>';
        header.insertAdjacentHTML('beforeend', dropIcon);

        header.addEventListener("click", () => {
            header.parentElement.classList.toggle("collapsed");
        });
    });
}

function toggleSidebar()
{
    const sidebar = document.getElementById("sidebar");
    sidebar.classList.toggle("collapsed");
    const toggleButton = document.getElementById("toggleSidebarButton");
}

/**
 * @param {HTMLInputElement} element
 */
function changeValue(element, valueType)
{
    let value = element.value;
    document.getElementById(valueType + "Value").textContent = value;

    // slider styling
    element.style.background = "linear-gradient(to right, var(--secondary-text-color) " + value + "%, var(--primary-text-color) " + value + "%)";
}


// ***************************** //
// ********* Settings ********** //
// ***************************** //

let userSettings = null;

function handleUserSettings(settings) {
    if (!settings) return;
    userSettings = settings;
    const nameInput = document.getElementById("user-name");
    if (nameInput) nameInput.value = settings.Name ?? "";
    const enableBackgroundRunning = document.getElementById("enable-background-running");
    const closeToBackground = document.getElementById("close-to-background");
    const startWithWindows = document.getElementById("start-with-windows");
    const startInBackground = document.getElementById("start-in-background");
    if (enableBackgroundRunning && closeToBackground && startWithWindows && startInBackground) {
        enableBackgroundRunning.checked = settings.EnableBackgroundRunning ?? false;
        closeToBackground.checked = settings.CloseToBackground ?? false;
        startWithWindows.checked = settings.StartWithWindows ?? false;
        startInBackground.checked = settings.StartInBackground ?? false;
        updateBackgroundSettingsAvailability();
    }

    if (settings.Theme) {
        document.body.className = settings.Theme;
        localStorage.setItem("theme", settings.Theme);
        updateActiveThemeButton();
    }

    const defaultWallpaperPath = document.getElementById("default-wallpaper-path");
    if (defaultWallpaperPath) defaultWallpaperPath.textContent = settings.DefaultWallpaperPath ?? "Windows wallpaper (automatic)";
    const defaultWallpaperPreview = document.getElementById("default-wallpaper-preview");
    if (defaultWallpaperPreview) {
        const previewPath = settings.DefaultWallpaperPath ?? detectedWallpaperInfo?.WindowsWallpaperPath;
        defaultWallpaperPreview.src = previewPath ? `https://local-image/${encodeURIComponent(previewPath)}` : "";
        defaultWallpaperPreview.hidden = !previewPath;
    }
    const livelyExePath = document.getElementById("lively-exe-path");
    if (livelyExePath) livelyExePath.textContent = settings.LivelyExePath ?? "Automatic detection";
    updateWallpaperButtonStates(settings);
}

let detectedWallpaperInfo = null;

function handleDetectedWallpaperInfo(info) {
    if (!info) return;
    const livelyStatus = document.getElementById("lively-status");
    if (livelyStatus) {
        livelyStatus.textContent = info.LivelyExePath
            ? `Lively found: ${info.LivelyExePath}`
            : "Lively not found. Select livelycu.exe manually.";
    }
    const windowsWallpaper = document.getElementById("windows-wallpaper-path");
    if (windowsWallpaper) windowsWallpaper.textContent = info.WindowsWallpaperPath ?? "not detected";
    const livelyWallpaper = document.getElementById("lively-wallpaper-path");
    if (livelyWallpaper) livelyWallpaper.textContent = info.LivelyExePath ?? "not detected";
    handleUserSettings(userSettings);
}

function resetDefaultWallpaper() {
    api.setUserWallpaperSettings({ DefaultWallpaperPath: null, LivelyExePath: userSettings?.LivelyExePath ?? null });
    userSettings = { ...(userSettings ?? {}), DefaultWallpaperPath: null };
    updateWallpaperButtonStates(userSettings);
    handleUserSettings(userSettings);
}

function resetLivelyExe() {
    api.setUserWallpaperSettings({ DefaultWallpaperPath: userSettings?.DefaultWallpaperPath ?? null, LivelyExePath: null });
    userSettings = { ...(userSettings ?? {}), LivelyExePath: null };
    api.getDetectedWallpaperInfo();
    handleUserSettings(userSettings);
}

function saveUserName() {
    const nameInput = document.getElementById("user-name");
    if (nameInput) api.setUserName(nameInput.value);
}

function updateActiveThemeButton() {
    const activeTheme = document.body.className || "dark";
    document.querySelectorAll(".theme-options button").forEach(button => {
        button.classList.toggle("active", button.id === `theme-button-${activeTheme}`);
    });
}

function changeTheme(theme) {
    document.body.className = theme;
    updateActiveThemeButton();
    localStorage.setItem('theme', theme);
    userSettings = { ...(userSettings ?? {}), Theme: theme };
    api.setUserTheme(theme);
}

function updateBackgroundSettingsAvailability() {
    const enableBackgroundRunning = document.getElementById("enable-background-running");
    const closeToBackground = document.getElementById("close-to-background");
    const startWithWindows = document.getElementById("start-with-windows");
    const startInBackground = document.getElementById("start-in-background");
    if (!enableBackgroundRunning || !closeToBackground || !startWithWindows || !startInBackground) return;

    const backgroundEnabled = enableBackgroundRunning.checked;
    closeToBackground.disabled = !backgroundEnabled;
    startInBackground.disabled = !backgroundEnabled || !startWithWindows.checked;

    if (closeToBackground.disabled) {
        closeToBackground.checked = false;
    }
    if (startInBackground.disabled) {
        startInBackground.checked = false;
    }
}

function saveBackgroundSettings() {
    updateBackgroundSettingsAvailability();
    const enableBackgroundRunning = document.getElementById("enable-background-running");
    const closeToBackground = document.getElementById("close-to-background");
    const startWithWindows = document.getElementById("start-with-windows");
    const startInBackground = document.getElementById("start-in-background");
    if (!enableBackgroundRunning || !closeToBackground || !startWithWindows || !startInBackground) return;

    api.setUserBackgroundSettings({
        EnableBackgroundRunning: enableBackgroundRunning.checked,
        CloseToBackground: closeToBackground.checked,
        StartWithWindows: startWithWindows.checked,
        StartInBackground: startInBackground.checked
    });
}

function updateWallpaperButtonStates(settings) {
    const chooseDefaultBtn = document.querySelector('.choose-image button:first-child');
    const useWindowsBtn = document.querySelector('.choose-image button:nth-child(2)');
    
    if (chooseDefaultBtn && useWindowsBtn) {
        if (settings.DefaultWallpaperPath) {
            chooseDefaultBtn.classList.add("active");
            useWindowsBtn.classList.remove("active");
        } else {
            chooseDefaultBtn.classList.remove("active");
            useWindowsBtn.classList.add("active");
        }
    }
}