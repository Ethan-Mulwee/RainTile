# RainTile

RainTile is a tool for converting Rain World tiles to Minecraft JSON block/item models. 

basic usage

```bash
./RainTileCLI (path to png) 
./RainTileCLI # Passing no arugments displays --help
```

the output file will be put in the current directory and named automatically if no path is specified. Otherwise you can set one with `-o` or `--output`


```bash
./RainTileCLI "Background AC Fan.png" -o fizz/buzz.json
```
The name automatically given will be in all lower case and have the spaces replaced with '_' for Minecraft compatibility `Background AC Fan.png` becomes `background_ac_fan.json`

If you want to automatically convert all the files in a directory with bash you can do this.
```bash
for f in *.png; do ./RainTileCLI "$f" -n; done
```