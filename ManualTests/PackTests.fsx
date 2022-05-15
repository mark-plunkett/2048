#r "../Game/bin/Debug/net6.0/Game.dll"

open System
open System.Collections
open System.Numerics
open System.Text
open System.Runtime.Intrinsics
open GameSIMDBranchless

let inline collapse16 i =
    -(-i >>> 15)

let inline collapse32 i =
    -(-i >>> 31)

// Return the shuffle map needed to pack cells
let cells = [|
    1s; 2s; 0s; 4s;
    0s; 0s; 7s; 8s;
    0s; 0s; 0s; 0s;
    0s; 3s; 0s; 9s |]

// should produce
// 1 1 0 1, 0 0 1 1, 0 0 0 0, 0 0 0 1
// 1 1 1 0, 1 1 0 0, 0 0 0 0, 1 0 0 0

let temp () =

    let i1 = cells.[1] |> collapse16
    let v1 = Vector  (cells, 0)
    let v2 = Vector.GreaterThan (v1, Vector.Zero) |> Vector.Abs
    let cellBits = Array.zeroCreate<int16> 16
    v2.CopyTo (cellBits)

// 0 0 1 2
// 3 4 0 0

let buildMask (cells : int16[]) =
    let a = Array.zeroCreate 16
    let mutable anyZero = -1
    for k = 0 to 3 do
        for i = 0 to 3 do
            let baseIdx = (k * 4)
            let idx = baseIdx + i
            if anyZero = -1 && cells[idx] = 0s then
                anyZero <- idx

            let mutable j = idx
            let maxJ = baseIdx + 3
            while j < maxJ + 1 && cells[j] = 0s do
                j <- j + 1
                
            if j > maxJ then
                a[idx] <- sbyte anyZero
            else
                a[idx] <- sbyte j
                cells[j] <- 0s
    
    a

// 2 3 0 0, 7 0 0 0, 8 0 0 0, 0 0 0 0
// let m = buildMask "0011000110000000"

let m = buildMask cells

let aToVec128 (a : sbyte[]) =
    Vector128.Create(
        a[0],
        a[1],
        a[2],
        a[3],
        a[4],
        a[5],
        a[6],
        a[7],
        a[8],
        a[9],
        a[10],
        a[11],
        a[12],
        a[13],
        a[14],
        a[15])

let vMask = aToVec128 m

let cells' = Array.zeroCreate 17
Array.blit cells 0 cells' 1 16
GameSIMDBranchless.shuffle cells' vMask
cells'

// let t = buildMask "0010000000000000"
// t

let buildIndex (cells: int16[]) =

    let mutable index = 0
    let i0 = cells.[0] |> collapse16 |> int
    index <- index ||| (i0 <<< 0)
    let i1 = cells.[1] |> collapse16 |> int
    index <- index ||| (i1 <<< 1)
    let i2 = cells.[2] |> collapse16 |> int
    index <- index ||| (i2 <<< 2)
    let i3 = cells.[3] |> collapse16 |> int
    index <- index ||| (i3 <<< 3)
    let i4 = cells[4]  |> collapse16 |> int
    index <- index ||| (i4 <<< 4)
    let i5 = cells[5]  |> collapse16 |> int
    index <- index ||| (i5 <<< 5)
    let i6 = cells[6]  |> collapse16 |> int
    index <- index ||| (i6 <<< 6)
    let i7 = cells[7]  |> collapse16 |> int
    index <- index ||| (i7 <<< 7)
    let i8 = cells[8]  |> collapse16 |> int
    index <- index ||| (i8 <<< 8)
    let i9 = cells[9]  |> collapse16 |> int
    index <- index ||| (i9 <<< 9)
    let i10 = cells[10] |> collapse16 |> int
    index <- index ||| (i10 <<< 10)
    let i11 = cells[11] |> collapse16 |> int
    index <- index ||| (i11 <<< 11)
    let i12 = cells[12] |> collapse16 |> int
    index <- index ||| (i12 <<< 12)
    let i13 = cells[13] |> collapse16 |> int
    index <- index ||| (i13 <<< 13)
    let i14 = cells[14] |> collapse16 |> int
    index <- index ||| (i14 <<< 14)
    let i15 = cells[15] |> collapse16 |> int
    index <- index ||| (i15 <<< 15)
    index

let index = buildIndex cells
printfn "%B" index

let iToBits (i : int) =
    Convert.ToString (i, 2)
    |> fun s -> s.PadLeft (16, '0')
    |> fun s -> s.ToCharArray ()
    |> Array.map string
    |> Array.map Int16.Parse

let i = iToBits 2

for i = 0 to 15 do //int UInt16.MaxValue / 16 do
    // get string
    let iString = 
        i
        |> iToBits
        |> fun a ->
            printfn "%A" a
            a
        |> buildIndex

    printfn "%i" iString

let packMap = Array.zeroCreate (int UInt16.MaxValue)
for i = 0 to (int UInt16.MaxValue) - 1 do
    let bits = iToBits i
    packMap[buildIndex bits] <- buildMask bits |> aToVec128


let cells2 = [|
    1s; 2s; 0s; 4s;
    0s; 0s; 7s; 8s;
    0s; 0s; 0s; 0s;
    0s; 3s; 0s; 9s |]

cells2
let cellsIndex = buildIndex cells2
let mask' = packMap[cellsIndex]

