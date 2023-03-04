module GameArray

open System

open Common

module Board =

    (*
        This board uses a 1 dimensional array of 16 int16s e.g.
        
        [ 0; 2; 256; 4; 16; 2; 4; 0; 2; 8; 4; 2; 0; 128; 16; 4 ]
        
        which represents the following board

          0   2 256   4 
         16   2   4   0 
          2   8   4   2 
          0 128  16   4
    *)
    let boardSize size = size * size
    let empty (size:int) =
        {
            Cells = Array.zeroCreate<int16> <| boardSize size
            Size = size
            Score = 0
            RNG = Random()
            RNGSeed = None
        } 
    
    let emptyWithSeed size seed =
        { empty size with
            RNG = Random(seed)
            RNGSeed = Some seed }

    let xyToIndex size x y =
        size * y + x

    let posToIndex size pos =
        xyToIndex size pos.X pos.Y

    let randomPos (board:Board<int16[]>) =
        { X = board.RNG.Next(0, board.Size); Y = board.RNG.Next(0, board.Size) }

    let randomValue (board:Board<int16[]>) =
        int16 <| if board.RNG.NextDouble() > 0.9 then 4 else 2

    let addRandomCell (board:Board<int16[]>) =
        let rec addRec pos value (board:Board<int16[]>) =
            let i = posToIndex board.Size pos
            match board.Cells.[i] with
            | v when v = 0s -> 
                board.Cells.[i] <- value
                board
            | _ -> 
                addRec (randomPos board) value board
        let value = randomValue board
        let pos = randomPos board
        addRec pos value board

    let init board =
        board
        |> addRandomCell
        |> addRandomCell

    let create = empty >> init

    let clone (board:Board<int16[]>) =
        let random =
            match board.RNGSeed with
            | Some s -> Random(s)
            | None -> Random()
        { board with
            Cells = Array.copy board.Cells
            RNG = random }

    let toString (board:Board<int16[]>) =
        let newLine = Environment.NewLine
        board.Cells
        |> Array.indexed
        |> Seq.fold (fun acc (i, v) ->
            acc
                + if i % board.Size = 0 then newLine + newLine else String.Empty
                + match v with
                    | 0s -> "    -"
                    | v -> sprintf "%5i" v
        ) newLine

    let fromList board =
        board
        |> List.collect id
        |> List.toArray
        |> Array.map int16

let flattenRow length rowIndex (cells:int16[]) =
    let xyToIndex = Board.xyToIndex length 
    let length' = length * (rowIndex + 1)
    // process each row
    let mutable score = 0
    let mutable x = 0
    while x < length - 1 do
        // process each cell
        let i = xyToIndex x rowIndex
        let vi = cells.[i]
        // find next non zero
        let mutable j = i + 1
        while j < length' && cells.[j] = 0s do
            j <- j + 1

        if j < length' then
            if vi = 0s then
                let vj = cells.[j]
                cells.[i] <- vj
                cells.[j] <- 0s
            elif cells.[i] = cells.[j] then
                cells.[i] <- cells.[i] + cells.[j]
                cells.[j] <- 0s
                score <- score + int cells.[i]
                x <- x + 1
            else
                x <- x + 1
        else
            x <- x + 1

    score

let anticlockwiseTransposeMap =    [| 3; 7; 11; 15; 2; 6; 10; 14; 1; 5; 9; 13; 0; 4; 8; 12 |]
let clockwiseTransposeMap =  [| 12; 8; 4; 0; 13; 9; 5; 1; 14; 10; 6; 2; 15; 11; 7; 3 |]
let flipTransposeMap = [| 3; 2; 1; 0; 7; 6; 5; 4; 11; 10; 9; 8; 15; 14; 13; 12 |]

let rotate (cells:int16[]) (map:int[]) =
    let cellsCopy = Array.copy cells
    for i = 0 to cells.Length - 1 do
        cells.[map.[i]] <- cellsCopy.[i]

let rotateCopy (cells:int16[]) (map:int[]) =
    let cellsCopy = Array.copy cells
    for i = 0 to cells.Length - 1 do
        cellsCopy.[map.[i]] <- cells.[i]
    
    cellsCopy

let rotateDirection (cells:int16[]) direction =
    match direction with
    | Direction.Left -> ()
    | Direction.Right -> rotate cells flipTransposeMap
    | Direction.Up -> rotate cells clockwiseTransposeMap
    | Direction.Down -> rotate cells anticlockwiseTransposeMap

let rotateOppositeDirection (cells:int16[]) direction =
    match direction with
    | Direction.Left -> ()
    | Direction.Right -> rotate cells flipTransposeMap
    | Direction.Up -> rotate cells anticlockwiseTransposeMap
    | Direction.Down -> rotate cells clockwiseTransposeMap

let swipe (board:Board<int16[]>) direction =
    rotateDirection board.Cells direction
    let mutable score = 0
    for rowIndex = 0 to board.Size - 1 do
        score <- score + flattenRow board.Size rowIndex board.Cells

    rotateOppositeDirection board.Cells direction
    { board with Score = board.Score + score }

let arraysEqual (a:_[]) (b:_[]) =
    let mutable equal = true
    for i = 0 to a.Length - 1 do
        if a.[i] <> b.[i] then equal <- false

    equal

let trySwipe (board:Board<int16[]>) direction =
    let origCells = Array.copy board.Cells
    let board' = swipe board direction
    if arraysEqual origCells board.Cells then
        board
    else
        Board.addRandomCell board'

let inline canSwipeRow (rowRef:inref<ReadOnlySpan<int16>>) =
    let mutable containsZero = false
    let mutable containsNonZero = false
    let mutable containsSpace = false
    let mutable containsPair = false
    for i = 0 to rowRef.Length - 1 do
        if rowRef.[i] = 0s then
            containsZero <- true
            if containsNonZero then containsSpace <- true

        if rowRef.[i] > 0s then 
            containsNonZero <- true
            if containsZero then containsSpace <- true
            if i < rowRef.Length - 1 && rowRef.[i] = rowRef.[i + 1] then containsPair <- true
    
    containsPair || containsSpace

let rec canSwipeRows size (rowsRef:inref<ReadOnlySpan<int16>>) i =
    if i = -1 then false
    else
        let rows = &rowsRef
        let slice = rows.Slice(i * size, size)
        if canSwipeRow &slice then true
        else canSwipeRows size &rowsRef (i - 1)

let canSwipe board =
    let hRows = ReadOnlySpan(board.Cells)
    let canSwipeHorizontal = canSwipeRows board.Size &hRows (board.Size - 1)
    let rotated = rotateCopy board.Cells clockwiseTransposeMap
    let vRows = ReadOnlySpan(rotated)
    let canSwipeVertical = canSwipeRows board.Size &vRows (board.Size - 1)
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