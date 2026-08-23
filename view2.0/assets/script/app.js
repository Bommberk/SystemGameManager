loadSidebar();
loadPage("gamemanager");


// ***************************** //
// ******** GameManager ******** //
// ***************************** //
// Handle C# Responses
function handleGames()
{
    document.getElementById("gameAmount").textContent = games.length;
    createGameList();
}

function handleLaunchers()
{
    document.getElementById("launcherAmount").textContent = launchers.length;
    createLauncherList();
}
function handleAudioDevices(devices)
{
    const audioDeviceSelection = document.getElementById("audioOutputDevice");
    // audioDeviceSelection.innerHTML = "";
    devices.forEach(device => {
        const option = document.createElement("option");
        option.value = device;
        option.textContent = device;
        audioDeviceSelection.appendChild(option);
    });
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
            <div class="content">
                <h3>${launcher.Name}</h3>
                <p class="installpath">${launcher.InstallPath}</p>
            </div>
        `;
        launcherList.appendChild(launcherCard);
    });
}
function createGameList(gameArray = null)
{
    const gameList = document.getElementById("gamelist");
    gameList.innerHTML = "";
    (gameArray ?? games).forEach(game => {
        if(game.IsRemovedFromView) return;
        const gameCard = document.createElement("div");
        gameCard.className = "game card";
        gameCard.innerHTML = `
            <img src="${getGameImageUrl(game.GameImage)}" alt="${game.Name} image" onerror="this.src='../assets/images/bild.jpg';" onclick="selectGame('${game.SerializedGameName}')">
            <input type="checkbox" name="select${game.SerializedGameName}" class="game-checkbox" data-game-name="${game.SerializedGameName}" onchange="selectGame('${game.SerializedGameName}')">
            <div class="game-menu">
                <i class="fa-solid fa-ellipsis-vertical" role="button" onclick="toggleGameMenuPopup('gameMenuPopup-${game.SerializedGameName}')"></i>
                <div class="popup" id="gameMenuPopup-${game.SerializedGameName}">
                    <ul>
                        <li role="button" onclick="launchGame('${game.SerializedGameName}')">
                            <i class="fa-solid fa-play"></i>
                            <span>Starten</span>
                        </li>
                        <li role="button" onclick="startChangeGameImageProzess('${game.SerializedGameName}')">
                            <i class="fa-solid fa-gear"></i>
                            <span>Bild ändern</span>
                        </li>
                        <li role="button" onclick="openInstallFolder('${game.SerializedGameName}')">
                            <i class="fa-solid fa-folder-open"></i>
                            <span>Ordner öffnen</span>
                        </li>
                        <li role="button" onclick="removeGameFromView('${game.SerializedGameName}')" class="warning">
                            <i class="fa-solid fa-trash"></i>
                            <span>Spiel entfernen</span>
                        </li>
                    </ul>
                </div>
            </div>
            <div class="content">
                <h3>${game.Name}</h3>
                <p>Installationspfad:</p>
                <span id="installPath">${game.InstallFolderPath}</span>
                <p>Volume:</p>
                <span id="volume">Game: ${game.GameVolumePercent ?? 0}% | Music: ${game.MusicVolumePercent ?? 0}%</span>
                <p>Audio Output</p>
                <span id="audioOutput">${game.AudioOutputDevice ?? "N/A"}</span>
            </div>
        `;
        gameList.appendChild(gameCard);
    });
}

function getGameImageUrl(imagePath)
{
    if (!imagePath) {
        return "../assets/images/bild.jpg";
    }

    if (/^(?:[a-zA-Z]:[\\/]|\\\\)/.test(imagePath)) {
        return `https://local-image/${encodeURIComponent(imagePath)}`;
    }

    return imagePath;
}


let selectedGames = new Set();
function selectGame(gameName) {
    if (selectedGames.has(gameName)) {
        selectedGames.delete(gameName);
    } else {
        selectedGames.add(gameName);
    }
    document.querySelector("input[data-game-name='" + gameName + "']").checked = selectedGames.has(gameName);
    
    if(selectedGames.size > 0)
    {
        document.getElementById("selectAllGamesButton").textContent = "Alle abwählen";
    } else {
        document.getElementById("selectAllGamesButton").textContent = "Alle auswählen";
    }
}
function selectAllGames()
{
    if(selectedGames.size > 0){
        selectedGames.clear();
        document.querySelectorAll(".game-checkbox").forEach(checkbox => {
            checkbox.checked = false;
        });
        document.getElementById("selectAllGamesButton").textContent = "Alle auswählen";
    } else {
        games.forEach(game => {
            if(!game.IsRemovedFromView)
                selectGame(game.SerializedGameName);
        });
    }
}
function reverseSelection()
{
    games.forEach(game => {
        if (selectedGames.has(game.SerializedGameName)) {
            selectedGames.delete(game.SerializedGameName);
            document.querySelector("input[data-game-name='" + game.SerializedGameName + "']").checked = false;
        } else {
            selectedGames.add(game.SerializedGameName);
            document.querySelector("input[data-game-name='" + game.SerializedGameName + "']").checked = true;
        }
    });
}

function saveSelectedGames()
{
    const gameVolume = parseInt(document.getElementById("gameVolumeSlider").value);
    const musicVolume = parseInt(document.getElementById("musicVolumeSlider").value);
    const audioOutputDevice = document.getElementById("audioOutputDevice").value;
    selectedGames.forEach(gameName => {
        const game = games.find(g => g.SerializedGameName === gameName);
        setGameAudio(game.SerializedGameName, gameVolume, musicVolume, audioOutputDevice);
    });
    
    api.setGames(games);
    createGameList();
}
function setGameAudio(gameName, gameVolume, musicVolume, audioOutputDevice) 
{
    games.forEach(game => {
        if (game.SerializedGameName === gameName) {
            game.GameVolumePercent = gameVolume;
            game.MusicVolumePercent = musicVolume;
            game.AudioOutputDevice = audioOutputDevice;
        }
    });
}
function setGameImage(gameName, imagePath) 
{
    games.forEach(game => {
        if (game.SerializedGameName === gameName) {
            game.GameImage = imagePath;
        }
    });
}

function filterGames(searchTerm)
{
    let filter = document.getElementById("selectFilter").value;
    const lowerCaseSearchTerm = searchTerm.toLowerCase();
    const filteredGames = games.filter(game => {
        if (filter === "name") {
            return game.Name.toLowerCase().includes(lowerCaseSearchTerm);
        } else if (filter === "installpath") {
            return game.InstallFolderPath.toLowerCase().includes(lowerCaseSearchTerm);
        } else if (filter === "audioDevice") {
            return (game.AudioOutputDevice ?? "N/A").toLowerCase().includes(lowerCaseSearchTerm);
        }
        return false;
    });

    createGameList(filteredGames);
}


let currentPopup = null;
let previousPopup = null;
let isActivatedPopup = false;
document.addEventListener("click", function(event) {
    if(!isActivatedPopup){
        const popups = document.querySelectorAll(".popup.active");
        popups.forEach(popup => {
            popup.classList.remove("active");
        });
    }else{
        isActivatedPopup = false;
    }
});

function toggleGameMenuPopup(popupId)
{
    const popup = document.getElementById(popupId);
    if (currentPopup && currentPopup !== popup) {
        currentPopup.classList.remove("active");
        previousPopup = currentPopup;
    }
    popup.classList.toggle("active");
    currentPopup = popup;
    isActivatedPopup = true;
}
function launchGame(gameName)
{

    alert("Launching game: " + getGameBySerializedName(gameName).Name);
    alert("This feature is not yet implemented.");
}
function openInstallFolder(gameName)
{
    alert("Opening install folder for game: " + getGameBySerializedName(gameName).Name);
    alert("This feature is not yet implemented.");
}
function removeGameFromView(gameName)
{
    const game = getGameBySerializedName(gameName);
    const confirmRemove = confirm("Removing game: " + game.Name + "\nWarning: This action cannot be undone!");
    if (confirmRemove) {
        game.IsRemovedFromView = true;
        api.removeGameFromView(game);
        createGameList();
    }
}
function startChangeGameImageProzess(gameName)
{
    api.changeGameImage(getGameBySerializedName(gameName));
}
function handleChangedGameImage(gameName, imagePath)
{
    setGameImage(gameName, imagePath);
    createGameList();
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

function changeTheme(theme) {
    document.body.className = theme;
    localStorage.setItem('theme', theme);
}