#r "nuget:Tedd.RandomUtils"

let n = 1000000
let seed = 1572854

let sysR = System.Random()
let sysTenth = 
    [1..n]
    |> List.map (fun _ -> sysR.NextDouble())
    |> List.filter (fun v -> v > 0.9)
    |> List.length
    |> printfn "%i"

let teddR = Tedd.RandomUtils.FastRandom()
let teddTenth = 
    [1..n]
    |> List.map (fun _ -> teddR.NextDouble())
    |> List.filter (fun v -> v > 0.9)
    |> List.length
    |> printfn "%i"
