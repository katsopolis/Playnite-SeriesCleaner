# Playnite-SeriesCleaner

## 🚀 What's this extension?
A simple Playnite extension that cleans your library by removing series metadata from games which are the only entry in their series.

## 🛠️ How does it work?
- It scans your library and identifies series containing only a single game.
- Offers to remove these series entries from the database to declutter your metadata.

## 📥 Installation
1. Download the latest release from the [Releases](https://github.com/katsopolis/Playnite-SeriesCleaner/releases) page.
2. Extract the `SeriesCleaner` folder to: %AppData%\Roaming\Playnite\Extensions\
3. Restart Playnite, and you'll find the extension under `Extensions > Series Cleaner`.

## 🎯 How to Use
- Open `Extensions > Series Cleaner` from the main menu.
- Click `Clean Single-Game Series`.
- Confirm the removal of detected series.

## 🖼️ SS's
![Alt text](images/screenshot1.png?raw=true "From Main Menu")
![Alt text](images/screenshot2.png?raw=true "After Clicking")

## ⚠️ Disclaimer
**Always backup your Playnite database before using.**  
The author is not responsible for accidental loss of metadata.

## ❓ FAQ
**Q:** Some series still appear even though they're single entries?  
**A:** This may occur if multiple series have identical names but different database IDs. You should manually merge them in Playnite by editing each game's Series field.

## 📃 License
This project is released under the MIT License.
