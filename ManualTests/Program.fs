open System
open System.Numerics

open GameArray

[<EntryPoint>]
let main argv =

    let a = [| 2; 2; 4; 4; 0; 0; 0; 0 |]
    let va = Vector(a)
    let vb = Vector(a, 1)


    0