module MapIt.Runner

open FsToolkit.ErrorHandling

let runInit options =
    result {
        printfn "%A" options
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
        printfn "%A" options
        return 0
    }
