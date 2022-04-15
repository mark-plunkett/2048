
open System.Numerics

let print v = 
    printfn "%A" v
    v

let process (a:int[]) =
    let va = Vector(a)
    let vb = Vector(a, 1)

    let vEqual = print <| Vector.Equals(va, vb)
    let vCond = print <| Vector.ConditionalSelect(vEqual, va, Vector<int>.Zero)
    let doubled = print <| Vector.Multiply(vCond, Vector(2))
    doubled

let swipe (a:int[]) =
    let vOrig = Vector(a, 1) |> print
    let vLShift = Vector(a, 2) |> print
    let vMatches = Vector.Equals(vOrig, vLShift) |> print
    let vOrigMatches = Vector.ConditionalSelect(vMatches, vOrig, Vector<int>.Zero) |> print
    let vDoubleOrigMatches = Vector.Multiply(Vector(2), vOrigMatches) |> print
    printfn "rshift"
    let vRShift = Vector(a) |> print
    let vRShiftEqualsOrig = Vector.Equals(vOrig, vRShift) |> print
    let vRShiftMatches = Vector.Xor(vRShiftEqualsOrig, Vector<int>.One) |> print
    let vZeroedOrig = Vector.Multiply(vOrig, vRShiftMatches) |> print
    let vResult = Vector.Max(vDoubleOrigMatches, vZeroedOrig) |> print
    vResult.CopyTo(a, 1)

let a = [| -1; 2; 2; 4; 4; 0; 0; 0; 0; -1 |] 
swipe a
printfn "%A" a
printfn "---"
//process [| 2; 4; 4; 4; 0; 0; 0; 0; 0 |]

open System
open System.Numerics

let pad = Int16.MinValue
let v = [| 
    2s; 2s; 1s; 0s; 
    0s; 0s; 0s; 0s; 
    0s; 4s; 0s; 0s; 
    0s; 0s; 0s; 9s;
    pad; pad; pad; pad; pad; pad; pad; pad; |]

print v
v.Length |> print

let doVec() =
    let v1 = Vector (v)
    let vecLength = Vector<int16>.Count
    let v2span = System.Span (v, 8, vecLength)
    let v2 = Vector<int16>(v2span)
    let vNarrow = Vector.Narrow (v1, v2)
    vNarrow.GetType() |> print
    print vNarrow

doVec()