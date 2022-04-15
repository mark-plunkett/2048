# Process

2 2 2 0 -> 3 0 2 0

## R shift

    2 2 2 0

    x 2 2 2

    f t t f

## L shift

    2 2 2 0

    2 2 0 x

    t t f f

## Comb

    f t t f

    t t f f

    f t f f

## Steps

- increment any lshift matches
- zero all lshift matches where also rshift matches

2 2 2 0

3 3 0 0

max

3 3 2 0

3 0 2 0

---

3 2 2 0

3 3 2 0

