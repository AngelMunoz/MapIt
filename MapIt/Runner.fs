module MapIt.Runner

open System
open System.IO

open FsToolkit.ErrorHandling
open MapIt.Types

open type Fs.Paths

let private (|ScopedPackage|Package|) (package: string) =
    if package.StartsWith("@") then
        ScopedPackage(package.Substring(1))
    else
        Package package

let private parsePackageName (name: string) =

    let getVersion parts =

        let version =
            let version =
                parts |> Seq.tryLast |> Option.defaultValue ""

            if String.IsNullOrWhiteSpace version then
                None
            else
                Some version

        version

    match name with
    | ScopedPackage name ->
        // check if the user is looking to install a particular version
        // i.e. package@5.0.0
        if name.Contains("@") then
            let parts = name.Split("@")
            let version = getVersion parts

            $"@{parts.[0]}", version
        else
            $"@{name}", None
    | Package name ->
        if name.Contains("@") then
            let parts = name.Split("@")

            let version = getVersion parts
            parts.[0], version
        else
            name, None

let runInit options =
    result {
        let path =
            match options.path with
            | Some path -> GetMapItConfigPath(path)
            | None -> GetMapItConfigPath()

        let config =
            { name = ""
              importMapPath = "./wwwroot/imports.importmap" |> Some
              dependencies = Map.ofSeq (Seq.empty<string * string>) |> Some }

        let bytes = Json.ToBytes config

        try
            File.WriteAllBytes(path, bytes)
        with
        | ex -> return! ex |> Error

        return 0
    }

let runSearch options =
    result {
        printfn "%A" options
        return 0
    }

let runShow options =
    result {
        printfn "%A" options
        return 0
    }

let runUninstall (options: UninstallPackageOptions) =
    taskResult {
        let name = defaultArg options.package ""

        if name = "" then
            return! PackageNotFoundException |> Error

        let! opts = Fs.getMapitConfig (GetMapItConfigPath())

        let! path =
            match opts.importMapPath with
            | Some path -> path |> Ok
            | None -> MissingImportMapPathException |> Error

        let! map = Fs.getOrCreateImportMap path

        let imports = map.imports |> Map.remove name

        let map = { map with imports = imports }

        do! Fs.writeImportMap path map
        return 0
    }

let runInstall (options: InstallPackageOptions) =
    taskResult {
        let! package, version =
            match options.package with
            | Some package -> parsePackageName package |> Ok
            | None -> MissingPackageNameException |> Error

        let alias =
            options.alias |> Option.defaultValue package

        let version =
            match version with
            | Some version -> $"@{version}"
            | None -> ""

        let! info = Http.getPackageUrlInfo $"{package}{version}"

        let! opts = Fs.getMapitConfig (GetMapItConfigPath())
        let! lockFile = Fs.getorCreateLockFile (GetMapItConfigPath())

        let dependencies =
            opts.dependencies
            |> Option.defaultValue (Map.ofList [])
            |> Map.change
                alias
                (fun f ->
                    f
                    |> Option.map (fun _ -> $"{Http.SKYPACK_CDN}/{info.lookUp}")
                    |> Option.orElse (Some $"{Http.SKYPACK_CDN}/{info.lookUp}"))

        let opts =
            { opts with
                  dependencies = Some dependencies }

        let lockFile =
            lockFile
            |> Map.change
                alias
                (fun f ->
                    f
                    |> Option.map (fun _ -> info)
                    |> Option.orElse (Some info))

        let! importMapPath =
            match opts.importMapPath with
            | Some path -> path |> Ok
            | None -> MissingImportMapPathException |> Error

        let! map = Fs.getOrCreateImportMap importMapPath

        let imports =
            map.imports
            |> Map.change
                alias
                (fun v ->
                    v
                    |> Option.map (fun _ -> $"{Http.SKYPACK_CDN}/{info.lookUp}")
                    |> Option.orElse (Some $"{Http.SKYPACK_CDN}/{info.lookUp}"))

        let map = { map with imports = imports }

        do! Fs.createMapitConfig (GetMapItConfigPath()) opts
        do! Fs.writeImportMap importMapPath map
        do! Fs.writeLockFile (GetMapItConfigPath()) lockFile

        return 0
    }
