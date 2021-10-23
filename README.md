# ExclamationPing

## Usage
```shell
$ dotnet run <IP> [timeout <ms>] [size <byte>] [repeat <count>]
```

## Example
```
> dotnet run 1.1.1.1
Type excape sequence to abort.
Sending 5, 32-byte ICMP Echos to 1.1.1.1, timeout is 2000 ms:
!!!!!
Success rate is 100 percent (5/5), round-trip min/avg/max = 13/15/26 ms
```
