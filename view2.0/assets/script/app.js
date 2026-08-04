// Globaler State
const state = {
    games: []
};

// API
const api = {
    getGames() {
        window.chrome.webview.postMessage({
            action: "getGames"
        });
    }
};

// Antwort von C#
window.apiResponse = function (response) {

    if (response.action === "getGames") {

        state.games = response.data;

        document.getElementById("gameAmount").textContent = state.games.length;
        document.getElementById("gameManagerImage").setAttribute("src", state.games[0]["GameImage"]);

        // Hier kannst du direkt dein HTML bauen
        // renderGames();
    }
};

// Anfrage schicken
api.getGames();

loadSidebar();
loadPage("gamemanager");