module GameSIMDPlus

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

    let addRandomCell (board:Board<int16[]>) =
        let rec addRec pos value (board:Board<int16[]>) =
            let i = 1 + GameArray.Board.posToIndex board.Size pos
            match board.Cells.[i] with
            | v when v = 0s -> 
                board.Cells.[i] <- value
                board
            | _ -> 
                addRec (GameArray.Board.randomPos board) value board
        let value = GameArray.Board.randomValue board
        let pos = GameArray.Board.randomPos board
        addRec pos value board

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
    let scores = board16Pool.Rent()
    try
        vDoubleOrigMatches.CopyTo(scores)
        let mutable score = 0s
        for i = 0 to scores.Length - 1 do
            score <- score + scores.[i]
        
        score
    finally
        board16Pool.Return(scores)

let rotate (cells:int16[]) (map:int[]) =
    let cellsCopy = boardPool.Rent()
    try
        Array.blit cells 0 cellsCopy 0 cells.Length
        for i = 0 to 15 do
            cells.[map.[i] + 1] <- cellsCopy.[i + 1]
    finally
        boardPool.Return(cellsCopy)

let rotateCopy (cells:int16[]) (map:int[]) =
    let cellsCopy = Array.copy cells
    for i = 0 to 15 do
        cellsCopy.[map.[i] + 1] <- cells.[i + 1]
    
    cellsCopy

let rotateDirection (cells:int16[]) direction =
    match direction with
    | Left -> ()
    | Right -> rotate cells GameArray.flipTransposeMap
    | Up -> rotate cells GameArray.clockwiseTransposeMap
    | Down -> rotate cells GameArray.anticlockwiseTransposeMap

let rotateOppositeDirection (cells:int16[]) direction =
    match direction with
    | Left -> ()
    | Right -> rotate cells GameArray.flipTransposeMap
    | Up -> rotate cells GameArray.anticlockwiseTransposeMap
    | Down -> rotate cells GameArray.clockwiseTransposeMap

let swipe (board:Board<int16[]>) direction =
    rotateDirection board.Cells direction
    let score =
        GameSIMD.pack board.Cells
        |> swipeSIMD
    GameSIMD.pack board.Cells |> ignore
    rotateOppositeDirection board.Cells direction
    { board with Score = board.Score + int score }

let trySwipe (board:Board<int16[]>) direction =
    let origCells = Array.copy board.Cells
    let board' = swipe board direction
    if GameSIMD.arraysEqual origCells board.Cells then
        board
    else
        Board.addRandomCell board'

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