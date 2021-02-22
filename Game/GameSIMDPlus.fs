module GameSIMDPlus

open Microsoft.FSharp.NativeInterop
open System
open System.Collections.Generic
open System.Numerics

open Common

type ArrayPool<'t>(aFactory:_ -> 't) =

    let unrented = Stack<'t>(Array.map aFactory [|1..100|])
    
    member _.Rent() =
        unrented.Pop()

    member _.Return(a) =
        unrented.Push(a)

module Board =

    type PosGenerator() =
        static let ps = [|3;7;15;8;14;11;0;9;12;1;4;13;10;6;5;2;11|]
        static let mutable i = 0

        static member Next() =
            i <- i + 1
            if i >= ps.Length then i <- 0
            ps.[i]

    let addRandomCell (board:Board<int16[]>) =
        let rec addRec value (board:Board<int16[]>) =
            let i = 1 + PosGenerator.Next()
            if board.Cells.[i] = 0s then
                board.Cells.[i] <- value
                board
            else
                addRec value board
        let value = GameArray.Board.randomValue board
        addRec value board

    let init board =
        board
        |> addRandomCell
        |> addRandomCell

    let create = GameSIMD.Board.empty >> init

let boardPool = ArrayPool(fun _ -> GameSIMD.Board.emptyCells 4)
let board16Pool = ArrayPool(fun _ -> Array.zeroCreate<int16> 16)
let swipeSIMD (cells:int16[]) =
    let vOrig = Vector(cells, 1)
    let vLShift = Vector(cells, 2)
    let vMatches = Vector.BitwiseAnd(GameSIMD.vIgnoreMatchIndicies, Vector.Equals(vOrig, vLShift))
    let vOrigMatches = Vector.ConditionalSelect(vMatches, vOrig, Vector<int16>.Zero)
    let vDoubleOrigMatches = Vector.Multiply(Vector(2s), vOrigMatches)
    let vRShift = Vector(cells)
    let vRShiftEqualsOrig = Vector.Equals(vOrig, vRShift)
    let vRShiftMatches = Vector.Xor(vRShiftEqualsOrig, Vector<int16>.One)
    let vZeroedOrig = Vector.Multiply(vOrig, vRShiftMatches)
    let vResult = Vector.Max(vDoubleOrigMatches, vZeroedOrig)
    vResult.CopyTo(cells, 1)
    let mem = NativePtr.stackalloc<int16>(16)
    let scores = Span<int16>(NativePtr.toVoidPtr mem, 16)
    vDoubleOrigMatches.CopyTo(scores)
    let mutable score = 0s
    for i = 0 to scores.Length - 1 do
        score <- score + scores.[i]
    
    score

let inline rotate (cells:int16[]) (map:int[]) =
    let cellsCopy = boardPool.Rent()
    try
        Array.blit cells 0 cellsCopy 0 cells.Length
        cells.[map.[0] + 1] <- cellsCopy.[1]
        cells.[map.[1] + 1] <- cellsCopy.[2]
        cells.[map.[2] + 1] <- cellsCopy.[3]
        cells.[map.[3] + 1] <- cellsCopy.[4]
        cells.[map.[4] + 1] <- cellsCopy.[5]
        cells.[map.[5] + 1] <- cellsCopy.[6]
        cells.[map.[6] + 1] <- cellsCopy.[7]
        cells.[map.[7] + 1] <- cellsCopy.[8]
        cells.[map.[8] + 1] <- cellsCopy.[9]
        cells.[map.[9] + 1] <- cellsCopy.[10]
        cells.[map.[10] + 1] <- cellsCopy.[11]
        cells.[map.[11] + 1] <- cellsCopy.[12]
        cells.[map.[12] + 1] <- cellsCopy.[13]
        cells.[map.[13] + 1] <- cellsCopy.[14]
        cells.[map.[14] + 1] <- cellsCopy.[15]
        cells.[map.[15] + 1] <- cellsCopy.[16]

    finally
        boardPool.Return(cellsCopy)

let inline rotateCopy (cells:int16[]) (map:int[]) =
    let cellsCopy = Array.copy cells
    for i = 0 to 15 do
        cellsCopy.[map.[i] + 1] <- cells.[i + 1]
    
    cellsCopy

let inline rotateDirection (cells:int16[]) direction =
    match direction with
    | Left -> ()
    | Right -> rotate cells GameArray.flipTransposeMap
    | Up -> rotate cells GameArray.clockwiseTransposeMap
    | Down -> rotate cells GameArray.anticlockwiseTransposeMap

let inline rotateOppositeDirection (cells:int16[]) direction =
    match direction with
    | Left -> ()
    | Right -> rotate cells GameArray.flipTransposeMap
    | Up -> rotate cells GameArray.anticlockwiseTransposeMap
    | Down -> rotate cells GameArray.clockwiseTransposeMap

let pack (cells:int16[]) =
    // Indicies account for vector padding
    let mutable i = 1
    for j = 2 to cells.Length - 2 do
        let vi = cells.[i]
        let vj = cells.[j]
        if (j % 4) = 1 then
            i <- j
        elif vi > 0s then
            i <- i + 1
        elif vi = 0s && vj > 0s then
            cells.[i] <- vj
            cells.[j] <- 0s
            i <- i + 1
 
    cells

let swipe (board:Board<int16[]>) direction =
    rotateDirection board.Cells direction
    pack board.Cells |> ignore
    let score = swipeSIMD board.Cells
    pack board.Cells |> ignore
    rotateOppositeDirection board.Cells direction
    { board with Score = board.Score + int score }

let trySwipe (board:Board<int16[]>) direction =
    let origCells = boardPool.Rent()
    try
        Array.blit board.Cells 1 origCells 1 16
        let board' = swipe board direction
        if GameSIMD.arraysEqual origCells board.Cells then board
        else Board.addRandomCell board'
    finally
        boardPool.Return(origCells)

let canSwipe board =
    let hRows = ReadOnlySpan(board.Cells, 1, 16)
    let canSwipeHorizontal = GameArray.canSwipeRows board.Size &hRows (board.Size - 1)
    let rotated = rotateCopy board.Cells GameArray.clockwiseTransposeMap
    let vRows = ReadOnlySpan(rotated, 1, 16)
    let canSwipeVertical = GameArray.canSwipeRows board.Size &vRows (board.Size - 1)
    canSwipeHorizontal || canSwipeVertical

let boardContext = {
    TrySwipe = trySwipe
    Clone = GameArray.Board.clone
    Create = Board.create
    CreateWithSeed = fun i j -> GameSIMD.Board.emptyWithSeed i j |> Board.init
    CanSwipe = canSwipe
    ToString = GameSIMD.Board.toString
}
