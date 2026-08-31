# System & Game Manager

**System & Game Manager** ist eine Windows-App zur lokalen Erkennung installierter Game-Launcher und Spiele. Sie bündelt die Bibliothek in einer eingebetteten Web-Oberfläche und erlaubt, Audio-Profile pro Spiel zu speichern. Beim Wechsel in ein erkanntes Spiel kann die App die Lautstärke einer Musik-App sowie das Standard-Audiogerät automatisch anpassen und anschließend wiederherstellen.

> Aktueller Entwicklungsstand: Die App konzentriert sich momentan auf Game- und Audio-Management. Die Navigation enthält zwar einen Dashboard-Einstieg, der Funktionsschwerpunkt und der standardmäßig geöffnete Bereich ist aber der Game Manager.

## Funktionen

### Spiele und Launcher erkennen

- Erkennt derzeit Steam, Epic Games Launcher, GOG Galaxy, Ubisoft Connect, EA App, Battle.net, Rockstar Games Launcher, Origin, Minecraft Launcher und CurseForge.
- Prüft die Windows-Uninstall-Registry und, soweit vorhanden, launcher-spezifische Registry-Schlüssel.
- Liest bei Steam zusätzliche Bibliotheksordner aus `libraryfolders.vdf`.
- Sucht in ermittelten Spieleordnern nach ausführbaren Dateien und ergänzt Treffer aus der Registry.
- Führt gleiche Installationspfade zusammen.

### Bibliothek verwalten

- Zeigt gefundene Launcher und Spiele in einer modernen, lokalen WebView2-Oberfläche.
- Speichert Launcher- und Spieldaten in einer SQLite-Datenbank.
- Ermöglicht die Auswahl mehrerer Spiele, Suche/Filter sowie das Übernehmen eines Audio-Profils für die Auswahl.
- Unterstützt eigene Spielbilder. Lokale Bilddateien können über die Spielkarte ausgewählt werden.
- Spiele lassen sich aus der Ansicht ausblenden. Die Datenbankdaten bleiben erhalten; eine Wiederherstellung über die Oberfläche gibt es derzeit noch nicht.

### Audio-Automatisierung

- Für jedes Spiel lassen sich Spiel-Lautstärke, Musik-Lautstärke und ein Audioausgabegerät speichern.
- Erkennt das aktive Vordergrundfenster über den Prozesspfad.
- Setzt während eines erkannten Spiels die Lautstärke der konfigurierten Musik-App und kann das Windows-Standardausgabegerät wechseln.
- Stellt die zuvor gemerkte Musiklautstärke und das frühere Ausgabegerät wieder her, sobald kein konfiguriertes Spiel mehr aktiv ist.

## Voraussetzungen

- Windows 10 oder Windows 11
- .NET 10 SDK für die Entwicklung
- Microsoft Edge WebView2 Runtime (auf aktuellen Windows-Systemen normalerweise vorhanden)
- Für den automatischen Audio-Gerätewechsel kann die verwendete Windows-Audioschnittstelle je nach Windows-Version Einschränkungen haben.

## Installation und Start

### Release verwenden

1. Die passende Release-Datei aus dem Repository herunterladen.
2. Das Setup ausführen.
3. System & Game Manager starten.

Beim Start werden installierte Launcher und Spiele erfasst und in der lokalen Datenbank aktualisiert. Die App prüft bei einer installierten Release-Version außerdem auf Updates.

### Aus dem Quellcode starten

```powershell
git clone https://github.com/Bommberk/SystemGameManager.git
cd SystemGameManager
dotnet restore
dotnet run
```

Für einen Release-Build:

```powershell
dotnet publish -c Release -r win-x64 --self-contained false
```

Die Lösung kann in Visual Studio oder VS Code über `SystemGameManager.sln` geöffnet werden.

## Bedienung

1. Die App starten und die erkannte Bibliothek im **Gamemanager** prüfen.
2. Spiele über Checkboxen auswählen oder nach Name, Installationspfad oder Audiogerät filtern.
3. Spiel- und Musiklautstärke sowie optional ein Ausgabegerät auswählen.
4. Mit **Speichern** das Profil für alle ausgewählten Spiele übernehmen.
5. Während die App geöffnet ist, überwacht sie das Vordergrundfenster und aktiviert das Profil, wenn ein erkanntes Spiel aktiv ist.

Die Einstellungen werden beim erneuten Scan anhand des Installationspfads übernommen.

## Konfiguration und Daten

- `assets/game/knownLaunchers.json` enthält die Launcher-Definitionen und kann für weitere Launcher erweitert werden.
- `config/appsettings.json` wird beim ersten Start erstellt, falls sie noch nicht vorhanden ist.
- Die Nutzdatenbank liegt standardmäßig unter `%AppData%\\SystemGameManager\\systemgamemanager.db`; die mitgelieferte Template-Datenbank befindet sich unter `modules/database/`.
- In einer Produktionskonfiguration kann die App beim Start eine Gerätekennung an die konfigurierte Smart-Home-API senden. Vor einer Verteilung sollte die Konfiguration unter `SmarthomeApiConfig` geprüft und bei Bedarf angepasst oder entfernt werden.

## Kommandozeile

```powershell
SystemGameManager.exe --console
```

Der Konsolenmodus führt die Erkennung aus und bleibt anschließend geöffnet. Die Audioüberwachung wird derzeit beim Start der grafischen Oberfläche aktiviert.

```powershell
SystemGameManager.exe --infos
```

In einer Entwicklungsumgebung führt `--infos` zusätzlich die Erkennung aus, bevor die Oberfläche geöffnet wird.

## Bekannte Einschränkungen

- Die Menüeinträge **Starten** und **Ordner öffnen** sind in der Oberfläche sichtbar, aber noch nicht implementiert.
- Ein ausgeblendetes Spiel kann momentan nicht direkt über die Oberfläche wieder eingeblendet werden.
- Die automatische Musiklautstärke richtet sich derzeit ausschließlich an Spotify; der gespeicherte Wert für die Spiel-Lautstärke wird von der laufenden Überwachung noch nicht angewendet.
- Die Launcher-Erkennung basiert auf bekannten Registry- und Standardpfaden; individuelle Installationen können daher unvollständig erkannt werden.

## Projektstruktur

```text
assets/                 Launcher-Definitionen und Standardgrafiken
config/                 Laufzeitkonfiguration
modules/Database/       SQLite-Zugriff und Datenmodell-Synchronisierung
modules/game/           Launcher-, Spiele- und Audio-Logik
view2.0/                WebView2-Oberfläche (HTML, CSS, JavaScript)
Handler/                Kommunikation zwischen Web-Oberfläche und C#
```

## Technologien

- C# / .NET 10 / Windows Forms
- Microsoft WebView2
- SQLite (`Microsoft.Data.Sqlite`)
- NAudio
- Velopack für Updates

## Lizenz

Für dieses Repository ist derzeit keine Lizenzdatei hinterlegt.
