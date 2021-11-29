# ProcessPerformance
ProcessPerformance is an easy-to-use command-line tool that provides runtime information about the CPU, memory, and network resources consumed by any combination of running processes. It does not inject code in the program sources or the binaries, avoiding the overhead caused by that measurement technique. ProcessPerformance is implemented as an open-source .NET Core application, which runs on Linux, macOS, and Windows.

## Prerequisites
[.NET Core Runtime 3.1](https://dotnet.microsoft.com/download/dotnet-core/3.1) or higher.

## Usage
To run ProcessPerformance on Windows, you just need to execute:

```bash
ProcessPerformance.exe	
```
On Linux and macOS:

```bash
dotnet run ProcessPerformance	
```

This will start the process performance reporter with default parameters.

## Options
* `-help` Display the command line arguments.
* `-network:NETWORK_IP` Specify P address of the network interface used to measure data transmission (disabled by default).
* `-interval:MILLISECONDS` The interval used to gather the runtime information of resource consumption, expressed in milliseconds. The default value is 1,000 (one second).
* `-csv`  Show the output in comma-separated values (CSV) format (disabled by default).
* `process_1 ... process_n` A space-separated list of the names or PIDs (process identifiers) of the processes to be monitored. If no process is passed, the overall system resources are displayed.
* `Ctrl + c` Terminate the execution of ProcessPerformance.

## Example
An example of use is monitoring Google Chrome and Microsoft Teams applications every 5 seconds, including system network traffic: 

```bash
ProcessPerformance.exe -network:192.168.0.100 -interval:5000 chrome teams

chrome+teams (34 processes) = CPU: 5.18 % | Memory: 3,999 MB | Processes: Sent 0 KB (0 kbps) - Received 0 KB (0 kbps) | Network: Sent 12 KB (89 kbps) - Received 4 KB (29 kbps)
chrome+teams (26 processes) = CPU: 1.69 % | Memory: 3,761 MB | Processes: Sent 3 KB (24 kbps) - Received 29 KB (237 kbps) | Network: Sent 42 KB (219 kbps) - Received 62 KB (431 kbps)
chrome+teams (20 processes) = CPU: 0.00 % | Memory: 3,294 MB | Processes: Sent 46 KB (350 kbps) - Received 96 KB (542 kbps) | Network: Sent 169 KB (918 kbps) - Received 287 KB (1,622 kbps)
chrome+teams (24 processes) = CPU: 11.53 % | Memory: 3,425 MB | Processes: Sent 93 KB (382 kbps) - Received 201 KB (864 kbps) | Network: Sent 242 KB (562 kbps) - Received 380 KB (716 kbps)
...
```
	
## More information
* http://www.reflection.uniovi.es/
