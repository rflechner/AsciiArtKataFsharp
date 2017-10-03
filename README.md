# AsciiArtKataFsharp
Ascii Art kata Fsharp

![ascii_2](images/ascii_1.png)

## Build project

> Run `build.cmd` or `build.sh`

## Run live tests

Windows:
> .\build.cmd Watch

Unix:
> ./build.sh Watch

A file watcher will run test each time you will save your implementation

---

## Alphabet

At first, we have to understand how to analyse the alphabet.

![ascii_2](images/ascii_2.png)

- Split the string in lines
- Identify column's spaces
- Group lines segments by letters
- Build a dictionary of `char * string []` 


## TODO

Each step is descripted in `Generator.fs`

