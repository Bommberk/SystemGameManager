function escapeHtml(text)
{
    const div = document.createElement("div");
    div.textContent = text ?? "";
    return div.innerHTML;
}

function handleHotKeys()
{
    const list = document.getElementById("hotkey-list");
    if (!list) return;

    const deviceSelect = document.getElementById("hotkey-device");
    deviceSelect.innerHTML = '<option value="">(unchanged)</option>';
    audioDevices.forEach(device => {
        const option = document.createElement("option");
        option.value = device;
        option.textContent = device;
        deviceSelect.appendChild(option);
    });

    const gameSelect = document.getElementById("hotkey-game-select");
    gameSelect.innerHTML = '<option value="">All games (global)</option>';
    games.forEach(game => {
        const option = document.createElement("option");
        option.value = game.SerializedGameName;
        option.textContent = game.Name;
        gameSelect.appendChild(option);
    });

    list.innerHTML = "";
    hotKeys.forEach((hotKey, index) => {
        const entry = document.createElement("div");
        entry.className = "hotkey-entry";
        const game = games.find(g => g.SerializedGameName === hotKey.GameSerializedName);
        entry.innerHTML = `
            <strong>${escapeHtml(hotKey.Name)}</strong> [${escapeHtml(hotKey.KeyDisplay)}]
            - ${game ? escapeHtml(game.Name) : "global"}
            - music: ${hotKey.MusicVolumePercent ?? "-"}%, game: ${hotKey.GameVolumePercent ?? "-"}%,
            device: ${escapeHtml(hotKey.AudioOutputDevice ?? "-")}
            ${hotKey.Description ? `<br><em>${escapeHtml(hotKey.Description)}</em>` : ""}
            <button type="button" onclick="deleteHotKey(${index})">Delete</button>`;
        list.appendChild(entry);
    });
}

function startHotKeyCapture()
{
    const display = document.getElementById("hotkey-keys-display");
    display.value = "Press keys...";
    api.startHotKeyCapture();
}

function handleHotKeyCaptured(data)
{
    const display = document.getElementById("hotkey-keys-display");
    if (!display) return;
    display.value = data.KeyDisplay;
    document.getElementById("hotkey-keys").value = data.KeyCodes;
}

function saveHotKeyFromForm(event)
{
    event.preventDefault();
    const numberOrNull = id => {
        const value = document.getElementById(id).value;
        return value === "" ? null : Math.min(100, Math.max(0, parseInt(value, 10)));
    };
    const keyCodes = document.getElementById("hotkey-keys").value;
    if (!keyCodes) {
        alert("Please capture a key or key combination first.");
        return;
    }
    api.saveHotKey({
        Name: document.getElementById("hotkey-name").value.trim(),
        MusicVolumePercent: numberOrNull("hotkey-music"),
        GameVolumePercent: numberOrNull("hotkey-game"),
        AudioOutputDevice: document.getElementById("hotkey-device").value || null,
        Description: document.getElementById("hotkey-description").value || null,
        KeyCodes: keyCodes,
        GameSerializedName: document.getElementById("hotkey-game-select").value || null
    });
    document.getElementById("hotkey-form").reset();
    document.getElementById("hotkey-keys").value = "";
}

function deleteHotKey(index)
{
    api.deleteHotKey({ Name: hotKeys[index].Name });
}
