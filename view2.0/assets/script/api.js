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
    getAudioDevices() {
        window.chrome.webview.postMessage({
            action: "getAudioDevices"
        });
    },
    setGames(data){
        window.chrome.webview.postMessage({
            action: "setGames",
            data: data
        });
    },
    changeGameImage(game){
        window.chrome.webview.postMessage({
            action: "changeGameImage",
            data: game
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
            case "getAudioDevices":
                handleAudioDevices(response.data);
                break;
            case "gameImageChanged":
                handleChangedGameImage(response.data.SerializedGameName, response.data.GameImage);
                break;
            default:
                console.error(`Unknown action: ${response.action}`);
        }
    }catch (error) {
        console.error("Error handling response:", error);
    }
};

function getGameBySerializedName(serializedName)
{
    return games.find(game => game.SerializedGameName === serializedName);
}

// Anfrage schicken
api.getGames();
api.getLaunchers();
api.getAudioDevices();