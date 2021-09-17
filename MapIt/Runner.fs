module MapIt.Runner

open System
open System.IO
open System.Text.Json
open System.Text.Json.Serialization

open FsToolkit.ErrorHandling
open MapIt.Types
open System.Collections.Generic
open FSharp.Control.Tasks
open Flurl.Http


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

[<Literal>]
let SKYPACK_CDN = "https://cdn.skypack.dev"

let checkPackageExists name =
    taskResult {
        try
            let! result = $"{SKYPACK_CDN}/%s{name}".GetAsync()
            return result.StatusCode = 200
        with
        | :? Flurl.Http.FlurlHttpException -> return! PackageNotFoundException |> Error
    }


let runInit options =
    result {
        let path =
            $"{defaultArg options.path (Environment.CurrentDirectory)}/mapit.json"

        let config =
            { name = ""
              importMapPath = "./public/imports.importmap" |> Some
              dependencies = dict (Seq.empty<string * JsonElement>) |> Some }

        let bytes =
            let opts =
                let opts = JsonSerializerOptions()
                opts.WriteIndented <- true
                opts

            JsonSerializer.SerializeToUtf8Bytes(config, options = opts)

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

let private getOptions () =
    try
        let bytes = File.ReadAllBytes("./mapit.json")

        JsonSerializer.Deserialize<MapitConfig>(ReadOnlySpan bytes)
        |> Ok
    with
    | ex -> ex |> Error


let private getOrCreateImportMap (path: string) : Result<ImportMap, exn> =
    try
        let path = Path.GetFullPath(path)

        Directory.CreateDirectory(Path.GetDirectoryName(path))
        |> ignore

        let bytes = File.ReadAllBytes(path)

        JsonSerializer.Deserialize<ImportMap>(ReadOnlySpan bytes)
        |> Ok
    with
    | :? System.IO.FileNotFoundException ->
        { imports = dict (Seq.empty)
          scopes = dict (Seq.empty) }
        |> Ok
    | ex -> ex |> Error

let private writeImportMap (path: string) (map: ImportMap) : Result<unit, exn> =
    result {
        let opts =
            let opts = JsonSerializerOptions()
            opts.WriteIndented <- true
            opts

        let bytes =
            JsonSerializer.SerializeToUtf8Bytes(map, options = opts)

        try
            let path = Path.GetFullPath(path)

            Directory.CreateDirectory(Path.GetDirectoryName(path))
            |> ignore

            File.WriteAllBytes(path, bytes)
        with
        | ex -> return! ex |> Error
    }

let runUninstall (options: UninstallPackageOptions) =
    result {
        let name = defaultArg options.package ""

        if name = "" then
            return! PackageNotFoundException |> Error

        let! opts = getOptions ()

        let! path =
            match opts.importMapPath with
            | Some path -> path |> Ok
            | None -> MissingImportMapPathException |> Error

        let! map = getOrCreateImportMap path

        let imports =
            seq {
                for pair in map.imports do
                    pair.Key, pair.Value
            }
            |> Map.ofSeq
            |> Map.remove name

        let map =
            { map with
                  imports = dict (imports |> Map.toSeq) }

        do! writeImportMap path map
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

        match! checkPackageExists $"{package}{version}" with
        | false -> return! PackageNotFoundException |> Error
        | true -> ()


        let url = $"{SKYPACK_CDN}/{package}{version}"

        let! opts = getOptions ()

        let! path =
            match opts.importMapPath with
            | Some path -> path |> Ok
            | None -> MissingImportMapPathException |> Error

        let! map = getOrCreateImportMap path

        let imports =
            seq {
                for pair in map.imports do
                    pair.Key, pair.Value
            }
            |> Map.ofSeq
            |> Map.change
                alias
                (fun v ->
                    v
                    |> Option.map (fun _ -> url)
                    |> Option.orElse (Some url))

        let map =
            { map with
                  imports = dict (imports |> Map.toSeq) }

        do! writeImportMap path map

        return 0
    }
