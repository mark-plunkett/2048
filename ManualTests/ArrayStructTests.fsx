open System

[<Struct>]
type RunSummary = {
    mutable TotalScore: int
    NumRuns: int
}

let vs = Array.zeroCreate<RunSummary> 4

vs[0].TotalScore <- 123

printfn "%A" (vs[0].TotalScore)
