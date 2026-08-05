/**
 * @typedef {Object} Launcher
 * @property {string} Name
 * @property {string} SearchName
 * @property {string} StdInstallPath
 * @property {string} InstallPath
 * @property {string} StdGameFoldersPath
 * @property {(string[]|null)} GameFolderPath
 * @property {(string|null)} StdLibraryFilePath
 * @property {(string|null)} DirectRegistryKey
 */
/** @type {Launcher[]} */
let launchers = [];

/**
 * @typedef {Object} Game
 * @property {string} Name
 * @property {string} SerializedGameName
 * @property {string} InstallFolderPath
 * @property {string} ExePath
 * @property {string} ProzessName
 * @property {(number|null)} MusicVolumePercent
 * @property {(number|null)} GameVolumePercent
 * @property {(string|null)} AudioOutputDevice
 * @property {(string|null)} GameImage
 */
/** @type {Game[]} */
let games = [];

// API
const api = {
    getGames() {
        window.chrome.webview.postMessage({
            action: "getGames"
        });
    },
    getLaunchers() {
        window.chrome.webview.postMessage({
            action: "getLaunchers"
        });
    },
    setGames(data){
        window.chrome.webview.postMessage({
            action: "setGames",
            data: data
        });
    },
};

// Antwort von C#
window.apiResponse = function (response) {

    try{
        switch (response.action) {
            case "getGames":
                games = response.data;
                handleGames();
                break;
            case "getLaunchers":
                launchers = response.data;
                handleLaunchers();
                break;
            default:
                console.error(`Unknown action: ${response.action}`);
        }
    }catch (error) {
        console.error("Error handling response:", error);
    }
};

// Anfrage schicken
api.getGames();
api.getLaunchers();

loadSidebar();
loadPage("gamemanager");


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
function createGameList()
{
    const gameList = document.getElementById("gamelist");
    gameList.innerHTML = "";
    games.forEach(game => {
        const gameCard = document.createElement("div");
        gameCard.className = "game card";
        gameCard.innerHTML = `
            <img src="${game.GameImage}" alt="${game.Name} image" onerror="this.src='../assets/images/bild.jpg';" onclick="selectGame('${game.SerializedGameName}')">
            <input type="checkbox" name="select${game.SerializedGameName}" class="game-checkbox" data-game-name="${game.SerializedGameName}" onchange="selectGame('${game.SerializedGameName}')">
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

function changeTheme(theme) {
    document.body.className = theme;
    localStorage.setItem('theme', theme);
}

// document.getElementById("gameVolumeSlider").addEventListener("input", function() {
//     document.getElementById("gameVolumeValue").textContent = this.value;
// });
// document.getElementById("musicVolumeSlider").addEventListener("input", function() {
//     document.getElementById("musicVolumeValue").textContent = this.value;
// });