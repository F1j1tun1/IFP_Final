# Thornwatch

A tower defence game made with C# and Blazor WebAssembly.

## Run on Windows

1. Install the [.NET 10 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/10.0).
   Choose **SDK > Windows > x64** for an Intel or AMD laptop, or **Arm64** for an ARM device.
   The Runtime alone is not enough.
2. Download the `main` branch from [GitHub](https://github.com/F1j1tun1/IFP_Final)
   using **Code > Download ZIP**.
3. Extract the whole ZIP, then open the extracted project folder.
4. Double-click **Start Game.cmd**. The launcher builds the game and opens the browser.
5. Keep the launcher window open while playing. Press **Ctrl+C** in that window to stop.

The first launch needs internet to download packages and may take a few minutes.
If the browser does not open, visit [localhost:5042](http://localhost:5042).
If that address is already in use, close the other game launcher first.
Keep `Start Game.cmd`, `Start-Game.ps1` and the project folders together.
Git, Node.js, Visual Studio and VS Code are not required to play.

For later launches, open the same **Start Game.cmd** again.
To update a ZIP download, download and extract the latest `main` into a new folder.
Saves stay in the same browser on the same device; they do not transfer with the project folder.

## Run from a terminal

On Windows, macOS or Linux, install the .NET 10 SDK and run this from the
project folder containing `ChronoBastion.sln`:

```sh
dotnet run --project src/ChronoBastion.Web -c Release --launch-profile http
```

Then open [localhost:5042](http://localhost:5042). Leave the terminal running while playing.
The Windows launcher files are not used on macOS or Linux.

## Team

**IT-2502**

- **Yerkingali Saruar:** gameplay logic, enemy abilities, balance, auto waves,
  saves and tower placement.
- **Ainabek Chingiskhan:** game interface, tower and enemy sprites, maps,
  responsive layout and browser integration.
