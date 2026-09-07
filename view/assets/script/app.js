loadSidebar();
loadPage("gamemanager");


// ***************************** //
// ******** GameManager ******** //
// ***************************** //
// Handle C# Responses
function handleGames()
{
    document.getElementById("gameAmount").textContent = games.filter(game => !game.IsRemovedFromView).length;
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