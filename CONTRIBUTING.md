## Local development - getting started

### Setting up

Tests (and many examples) require Node.js to be installed on your machine.
This is because `NetPack` uses NPM libraries to do much of it's file processing work (e.g. Rollup, rjs optimiser, Typescript compiler etc).
Please ensure you have Node.js installed (on your PATH) before running these tests.

**Recommended Node.js Version:** 20.18.1 (LTS - Iron)

You can install node.js 
- from https://nodejs.org/en/download/
- or via `nvm` or `fnm` (Node Version Manager) which allows you to switch between node versions as needed for different projects.
    - **Using fnm** (see https://github.com/Schniz/fnm):
      - `choco install fnm` (windows - as admin)
      - edit your powershell profile
        - (`Invoke-Item $profile`)
        - add this line to the end of the file: `fnm env --use-on-cd --shell powershell | Out-String | Invoke-Expression`
      - ```powershell
         cd src # contains .node-version file
         fnm install # ensures the correct version of node is downloaded
         fnm use  # makes the correct version active on your PATH
        ```
    - **Using nvm**: The repository includes a `.nvmrc` file at the root, so you can simply run:
      - `nvm install` (installs the version specified in .nvmrc)
      - `nvm use` (activates the correct version)
