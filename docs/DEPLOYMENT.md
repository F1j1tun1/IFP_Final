# Free deployment to GitHub Pages

The complete graphical game is packaged by the manual deployment workflow. See VERIFICATION.md for the exact tested commit and current publication status.

The intended public URL is https://f1j1tun1.github.io/IFP_Final/ . GitHub Pages is
available for public repositories on GitHub Free. This application needs only
static hosting, with no account system, database or paid service.

## Publish the finished game

1. Finish Part 2, commit its actual work under Chingiskhan's own Git identity,
   and merge the reviewed part-two branch into `main`.
2. Confirm **Build and test** is green for that exact main commit on Windows and Linux.
3. In the repository open **Settings > Pages > Build and deployment > Source**
   and choose **GitHub Actions**. If this setting is unavailable to the collaborator,
   F1j1tun1 must perform this one repository setting change.
4. Open **Actions > Deploy Pages > Run workflow**, choose `main`, and run it.
5. Wait for both build and deploy to finish. Open the URL returned by deployment.
6. Check loading, building, firing, start wave, pause, speed, loss/restart and saved
   record at the public URL. Reload it and inspect the browser console/network.
7. Add the verified URL and date to README and VERIFICATION.md. Keep the workflow
   manual unless automatic deployment on main is explicitly desired.

## Local release package

```powershell
pwsh -File scripts/Publish-Pages.ps1
```

This creates `artifacts/pages/wwwroot`, changes only the published HTML base to
`/IFP_Final/`, adds `.nojekyll`, and copies the index to `404.html`. The source
index remains at `/` for local development. Do not commit artifacts or bin/obj.
Use relative URLs for CSS, images, JavaScript modules and audio. Do not prefix
them with `/`, which would point outside the repository subpath.

For a local static-host check at the root, use:

```powershell
pwsh -File scripts/Publish-Pages.ps1 -BasePath /
```

Serve the resulting wwwroot directory through a local HTTP server. Do not open
index.html using file://. The ordinary development command remains:

```powershell
dotnet run --project src/ChronoBastion.Web --launch-profile http
```

## Troubleshooting

| Symptom | Check |
|---|---|
| Pages workflow unavailable | The workflow file must be on the default branch |
| Deployment skipped | Select main, not the part-two feature branch |
| Configure Pages fails | Enable Pages with GitHub Actions in repository settings |
| HTML loads but .wasm/JS/CSS return 404 | Confirm `/IFP_Final/` base and relative URLs |
| Assets under _framework disappear | Confirm .nojekyll is in the uploaded wwwroot |
| Record disappears between localhost and live site | Each origin has separate browser storage; expected |
| Development works but published game fails | Inspect trimming/runtime errors and test the release build |
| Changes seem stale | Reload without cache and check the deployment's commit |

## Official references

- [GitHub Pages custom workflows](https://docs.github.com/en/pages/getting-started-with-github-pages/using-custom-workflows-with-github-pages)
- [Blazor WebAssembly hosting](https://learn.microsoft.com/en-us/aspnet/core/blazor/host-and-deploy/webassembly/?view=aspnetcore-10.0)
- [Blazor application base paths](https://learn.microsoft.com/en-us/aspnet/core/blazor/host-and-deploy/app-base-path?view=aspnetcore-10.0)
