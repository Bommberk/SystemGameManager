function escapeHtml(text)
{
    const div = document.createElement("div");
    div.textContent = text ?? "";
    return div.innerHTML;
}

function fillHotKeyDevices()
{
    const deviceSelect = document.getElementById("hotkey-device");
    if (!deviceSelect) return;

    const selected = deviceSelect.value;
    deviceSelect.innerHTML = '<option value="">(unchanged)</option>';
    audioDevices.forEach(device => {
        const option = document.createElement("option");
        option.value = device;
        option.textContent = device;
        deviceSelect.appendChild(option);
    });
    deviceSelect.value = selected;
}

function handleHotKeys()
{
    const list = document.getElementById("hotkey-list");
    if (!list) return;

    fillHotKeyDevices();

    list.innerHTML = "";
    hotKeys.forEach((hotKey, index) => {
        const row = document.createElement("tr");
        row.innerHTML = `
            <td>${escapeHtml(hotKey.Name)}</td>
            <td>${hotKey.MusicVolumePercent ?? "-"}</td>
            <td>${hotKey.GameVolumePercent ?? "-"}</td>
            <td>${escapeHtml(hotKey.AudioOutputDevice ?? "-")}</td>
            <td class="hotkey-description" title="${escapeHtml(hotKey.Description ?? "")}">${escapeHtml(hotKey.Description ?? "")}</td>
            <td>${escapeHtml(hotKey.KeyDisplay)}</td>
            <td><button type="button" onclick="deleteHotKey(${index})">Delete</button></td>`;
        list.appendChild(row);
    });
}

function openHotKeyModal()
{
    document.getElementById("hotkey-form").reset();
    document.getElementById("hotkey-keys").value = "";
    document.getElementById("hotkey-modal").classList.add("active");
    fillHotKeyDevices();
    api.getAudioDevices();
}

function closeHotKeyModal()
{
    document.getElementById("hotkey-modal").classList.remove("active");
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
        GameSerializedName: null
    });
    closeHotKeyModal();
}

function deleteHotKey(index)
{
    api.deleteHotKey({ Name: hotKeys[index].Name });
}
