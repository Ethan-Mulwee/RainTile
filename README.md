# RainTile

RainTile is a tool for converting Rain World tiles to Minecraft JSON block/item models. 

basic usage

```bash
./RainTileCLI (path to png) 
```

the output file will be put in the current directory and named automatically if no apth is specified. Otherwise you can set one with `-o` or `--output`

```bash
./RainTileCLI "Background AC Fan.png" -o fizz/buzz.json
```