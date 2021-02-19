module Common 

open System

type Position = {
    X: int
    Y: int
}

type Board<'t> = {
    Cells: 't
    Size: int
    Score: int
    RNG: Tedd.RandomUtils.FastRandom
    RNGSeed: int option
}

type Direction = | Up | Down | Left | Right

type BoardContext<'t> = {
    TrySwipe: Board<'t> -> Direction -> Board<'t>
    Clone: Board<'t> -> Board<'t>
    Create: int -> Board<'t>
    CreateWithSeed: int -> int -> Board<'t>
    CanSwipe: Board<'t> -> bool
    ToString: Board<'t> -> string
}
