
# SIMD algo

|#| Data | Step |
|-|-|-|
|1| [ 2 4 4 0 ] 0 | Padded with zero for shift |
|2| [ 4 4 0 0 ] | Shift left |
|3| [ 0 1 0 0 ] | Find matches |
|4| [ 0 8 0 0 ] | Double matches |
|5| [ 0 2 4 4 ] | Shift #1 right |
|6| [ 0 0 1 0 ] | Find matches compared to #1 |
|7| [ 2 4 0 0 ] | Use mask to zero matching values of #1 |
|8| [ 2 8 0 0 ] | Combine #4 and #7, take larger |
