module MapIt.Runner

open System
open System.IO
open System.Text.Json
open System.Text.Json.Serialization

open FsToolkit.ErrorHandling
open MapIt.Types


let private (|ScopedPackage|Package|) (package: string) =
    if package.StartsWith("@") then
        ScopedPackage(package.Substring(1))
    else
        Package package

let private parsePackageName (name: string) (withAt: bool) =

    if name.Contains("@") then
        let parts = name.Split("@")
        let withAt = if withAt then "@" else ""

        let version =
            let version =
                parts |> Seq.tryLast |> Option.defaultValue ""

            if String.IsNullOrWhiteSpace version then
                None
            else
                Some version

        $"{withAt}{parts.[0]}", version
    else
        name, None



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

let runInstall options =
    result {
        let! package, version =
            match options.package with
            | Some (ScopedPackage package) -> parsePackageName package true |> Ok
            | Some (Package package) -> parsePackageName package false |> Ok
            | None -> MissingPackageNameException |> Error

        let alias =
            options.alias |> Option.defaultValue package

        let map =
            let version =
                match version with
                | Some version -> $"@{version}"
                | None -> ""

            { imports = dict [ alias, $"https://cdn.skypack.dev/{package}{version}" ]
              scopes = dict (Seq.empty) }
            |> JsonSerializer.Serialize

        printfn "%A" map
        return 0
    }
