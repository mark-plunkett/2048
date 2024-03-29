module GameSIMDBranchless

#nowarn "9"
#nowarn "104"

open Microsoft.FSharp.NativeInterop
open System
open System.Collections.Generic
open System.Numerics
open System.Runtime.Intrinsics
open System.Runtime.Intrinsics.X86

open Common

type ArrayPool<'t>(aFactory:_ -> 't) =

    let unrented = Stack<'t>(Array.map aFactory [|1..100|])
    
    member _.Rent() =
        unrented.Pop()

    member _.Return(a) =
        unrented.Push(a)

type Board = 
    val mutable Score: int
    val Size: int
    val Cells: int16[]
    val RNG: Random
    new (size:int, cells:int16[], rng:Random) = {
        Score= 0
        Size = size
        Cells = cells
        RNG = rng
    }

    member this.SetScore(score) =
        this.Score <- score

module Board =

    type PosGenerator() =
        static let ps = [|3;7;15;8;14;11;0;9;12;1;4;13;10;6;5;2;11|]
        static let mutable i = 0

        static member Next() =
            i <- i + 1
            ps[i % ps.Length]

    let boardSize size = 2 + (size * size)
    let emptyCells size =
        let cells = Array.zeroCreate (boardSize size)
        cells.[0] <- -1s
        cells.[17] <- -1s
        cells

    let empty (size:int) =
        Board(size, emptyCells size, Random())
            
    let emptyWithSeed size seed =
        Board(size, emptyCells size, Random(seed))

    let randomValue (board:Board) =
        int16 <| if board.RNG.NextDouble() > 0.9 then 2 else 1

    let addRandomCell (board:inref<Board>) =
        let rec addRec value (board:Board) =
            let i = PosGenerator.Next()
            if board.Cells.[i + 1] = 0s then
                board.Cells.[i + 1] <- value
                board
            else
                addRec value board

        let value = randomValue board
        addRec value board

    let init (board:Board) =
        addRandomCell &board |> ignore
        addRandomCell &board |> ignore
        board

    let create = empty >> init

    let clone (board:Board) =
        Board(board.Size, Array.copy board.Cells, board.RNG)

    let toString (board:Board) =
        let newLine = Environment.NewLine
        board.Cells
        |> Array.skip 1
        |> Array.take 16
        |> Array.indexed
        |> Array.fold (fun acc (i, v) ->
            acc
                + if i % board.Size = 0 then newLine + newLine else String.Empty
                + match v with
                    | 0s -> "    -"
                    | v -> sprintf "%5i" <| int (2.0 ** float v)
        ) newLine

    let fromList (cells:int list list) =
        let empty = emptyCells 4
        let cells' =
            cells
            |> List.collect id
            |> List.toArray
            |> Array.map int16
        Array.blit cells' 0 empty 1 (cells'.Length)
        empty

module Constants =

    let antiClockwiseTransposeMap = Vector128.Create(
        3y, 7y, 11y, 15y,
        2y, 6y, 10y, 14y,
        1y, 5y, 9y, 13y,
        0y, 4y, 8y, 12y)

    let clockwiseTransposeMap = Vector128.Create(
        12y, 8y, 4y, 0y,
        13y, 9y, 5y, 1y,
        14y, 10y, 6y, 2y,
        15y, 11y, 7y, 3y)

    let flipTransposeMap = Vector128.Create(
        3y, 2y, 1y, 0y,
        7y, 6y, 5y, 4y,
        11y, 10y, 9y, 8y,
        15y, 14y, 13y, 12y)

    let rotateRightMask = Vector128.Create(
        15y, 0y, 1y, 2y,
        3y, 4y, 5y, 6y,
        7y, 8y, 9y, 10y,
        11y, 12y, 13y, 14y)

    let vIgnoreRowStartIndicies = Vector([|
        0s; -1s; -1s; -1s
        0s; -1s; -1s; -1s
        0s; -1s; -1s; -1s
        0s; -1s; -1s; -1s |])

    let vIgnoreRowEndIndicies = Vector([|
        -1s; -1s; -1s; 0s
        -1s; -1s; -1s; 0s
        -1s; -1s; -1s; 0s
        -1s; -1s; -1s; 0s |])

let iToBits (i : int) =
    Convert.ToString (i, 2)
    |> fun s -> s.PadLeft (16, '0')
    |> fun s -> s.ToCharArray ()
    |> Array.map string
    |> Array.map Int16.Parse

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

let inline collapse16 i =
    -(-i >>> 15)

let inline buildIndexBranchless (cells : Span<int16>) =

    let vCells = Vector<int16> (cells)
    let vGreaterThanZero = Vector.GreaterThan (vCells, Vector.Zero)
    let vBytes = Vector.Narrow (vGreaterThanZero, vGreaterThanZero)
    let vLower = vBytes.AsVector128 ()
    Sse2.MoveMask (vLower)

let packMap = 
    Seq.init (int UInt16.MaxValue + 1) (id)
    |> Seq.fold (fun (a : Vector128<sbyte>[]) i -> 
        let bits = iToBits i
        let bitsSpan = Span (bits)
        a[buildIndexBranchless bitsSpan] <- bits |> buildMask |> aToVec128
        a
    ) (Array.zeroCreate (int UInt16.MaxValue + 1))

let dump msg (o : Vector<int16>) = 
    // let o' = [4..7] |> List.map (fun i -> sprintf "%i " o.[i]) |> fun s -> String.Join ("", s)
    printfn "%30s: %A" msg o
    o

let boardPool = ArrayPool(fun _ -> GameSIMD.Board.emptyCells 4)

let inline shuffleVec (cells: Vector<int16>) (mask : Vector128<sbyte>) =
    let cellBytes = Vector.Narrow (cells, cells)
    let cellBytes128 = cellBytes.AsVector128 ()
    let shuffledBytes = Ssse3.Shuffle (cellBytes128, mask)
    let vShuffledBytes = shuffledBytes.AsVector ()
    let mutable result = Vector ()
    let mutable discard = Vector ()
    Vector.Widen (vShuffledBytes, &result, &discard)
    result

let inline shuffle (cells : Span<int16>) (mask : Vector128<sbyte>) =
    let vec = Vector<int16> (cells)
    let shuffled = shuffleVec vec mask
    shuffled.CopyTo (cells)

let inline calcScore (scores: Span<int16>) i =
    let score = scores.[i]
    let score32 = int score
    let pow = 1 <<< score32
    (int (collapse16 score)) * pow |> int16

let inline swipeSIMD (cells : int16[]) =

    let vOrig = Vector (cells, 1)
    let vOrigLShift = Vector (cells, 2)
    let vOrigLShiftZeroed = Vector.BitwiseAnd (Constants.vIgnoreRowEndIndicies, vOrigLShift)
    let vLShiftMatches = Vector.Equals (vOrig, vOrigLShiftZeroed)
    let vLShiftMatchesNonZero = Vector.ConditionalSelect (Vector.GreaterThan (vOrigLShiftZeroed, Vector.Zero), vLShiftMatches, Vector.Zero)

    let vOrigRShift = shuffleVec vOrig Constants.rotateRightMask
    let vOrigRShiftZeroed = Vector.BitwiseAnd (Constants.vIgnoreRowStartIndicies, vOrigRShift)
    let vRShiftMatches = Vector.Equals (vOrig, vOrigRShiftZeroed)
    let vRShiftMatchesNonZero = Vector.ConditionalSelect (Vector.GreaterThan (vOrigRShiftZeroed, Vector.Zero), vRShiftMatches, Vector.Zero)

    let vBothMatches = Vector.BitwiseAnd (vLShiftMatchesNonZero, vRShiftMatchesNonZero)
    let vRMatchesNotBoth = Vector.Xor (vRShiftMatchesNonZero, vBothMatches)
    let vOrigZeroed = Vector.ConditionalSelect (vRMatchesNotBoth, Vector.Zero, vOrig)

    let vLMatchesNotBoth = Vector.Xor (vLShiftMatchesNonZero, vBothMatches)
    let vIncOrig = Vector.Add (vOrigZeroed, Vector.One)
    let vResult = Vector.ConditionalSelect (vLMatchesNotBoth, vIncOrig, vOrigZeroed)

    vResult.CopyTo (cells, 1)

    // Scores
    let vScores = Vector.ConditionalSelect (vLMatchesNotBoth, vIncOrig, Vector.Zero)
    let mem = NativePtr.stackalloc<int16> (16)
    let scores = Span<int16> (NativePtr.toVoidPtr mem, 16)
    vScores.CopyTo (scores)
    let mutable scoreA = 0s
    let mutable scoreB = 0s
    let mutable scoreC = 0s
    let mutable scoreD = 0s
    for i = 0 to 3 do
        let i' = i * 4
        scoreA <- scoreA + calcScore scores (i')
        scoreB <- scoreB + calcScore scores (i' + 1)
        scoreC <- scoreC + calcScore scores (i' + 2)
        scoreD <- scoreD + calcScore scores (i' + 3)

    (scoreA + scoreB) + (scoreC + scoreD)

let inline rotateDirection (cells : Span<int16>) direction =
    match direction with
    | Direction.Left -> ()
    | Direction.Right -> shuffle cells Constants.flipTransposeMap
    | Direction.Up -> shuffle cells Constants.antiClockwiseTransposeMap
    | Direction.Down -> shuffle cells Constants.clockwiseTransposeMap

let inline rotateOppositeDirection (cells : Span<int16>) direction =
    match direction with
    | Direction.Left -> ()
    | Direction.Right -> shuffle cells Constants.flipTransposeMap
    | Direction.Up -> shuffle cells Constants.clockwiseTransposeMap
    | Direction.Down -> shuffle cells Constants.antiClockwiseTransposeMap

let inline packBranchless (cells: Span<int16>) =
    let packIndex = buildIndexBranchless cells
    shuffle cells packMap[packIndex]

let swipe (board : Board) direction =
    let cells = Span (board.Cells, 1, 16)
    rotateDirection cells direction
    let _ = packBranchless cells
    let score = swipeSIMD board.Cells
    let _ = packBranchless cells
    rotateOppositeDirection cells direction
    board.SetScore (board.Score + int score)

let trySwipe (board : Board) direction =
    let origCells = boardPool.Rent()
    try
        Array.blit board.Cells 1 origCells 1 16
        swipe board direction
        if GameSIMD.arraysEqual origCells board.Cells then board
        else Board.addRandomCell &board
    finally
        boardPool.Return(origCells)

let inline rotateCopy (cells:int16[]) (map:int[]) =
    let cellsCopy = Array.copy cells
    for i = 0 to 15 do
        cellsCopy.[map.[i] + 1] <- cells.[i + 1]
    
    cellsCopy 

let canSwipe (board : Board) =
    let hRows = ReadOnlySpan(board.Cells, 1, 16)
    let canSwipeHorizontal = GameArray.canSwipeRows board.Size &hRows (board.Size - 1)
    let rotated = rotateCopy board.Cells GameArray.clockwiseTransposeMap
    let vRows = ReadOnlySpan(rotated, 1, 16)
    let canSwipeVertical = GameArray.canSwipeRows board.Size &vRows (board.Size - 1)
    canSwipeHorizontal || canSwipeVertical

let boardContext = {
    TrySwipe = trySwipe
    Clone = Board.clone
    Create = Board.create
    CreateWithSeed = fun i j -> Board.emptyWithSeed i j |> Board.init
    CanSwipe = canSwipe
    ToString = Board.toString
    Score = fun board -> board.Score
    RNG = fun board -> board.RNG
}
