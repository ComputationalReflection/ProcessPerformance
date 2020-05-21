# ProcessPerformance
ProcessPerformance is a tool that allows you to collect performance data and memory and network of running processes.

## Usage
To run ProcessPerformance you just need to execute:

```bash
ProcessPerformance.exe	
```

This will start the process performance reporter with default parameters.

## Options
* `-help` Displays the usage message
* `-network:NETWORK_IP` Specify the network interface IP (disable by default).
* `-interval:MILISENCONS` Specify the interval time in milisecons (default is 1000).
* `-csv` Specify output format as CSV (disable by default).
* `process_1 ... process_n` A list of process names (if empty, all running processes are used).
* `Ctrl + c`	interrupts the execution.

## Example
An example of use is monitoring Google Chrome and Microsoft Teams applications every 5 seconds, including system network traffic. 

```bash
ProcessPerformance.exe -network:192.168.0.100 -interval:500 chrome teams

chrome+teams (34 ths) = CPU: 5,18 % | Memory: 3.999 MB | Process: Sent 0 KB (0 kbps) - Received 0 KB (0 kbps) | Network: Sent 12 KB (89 kbps) - Received 4 KB (29 kbps)
chrome+teams (26 ths) = CPU: 1,69 % | Memory: 3.761 MB | Process: Sent 3 KB (24 kbps) - Received 29 KB (237 kbps) | Network: Sent 42 KB (219 kbps) - Received 62 KB (431 kbps)
chrome+teams (20 ths) = CPU: 0,00 % | Memory: 3.294 MB | Process: Sent 46 KB (350 kbps) - Received 96 KB (542 kbps) | Network: Sent 169 KB (918 kbps) - Received 287 KB (1.622 kbps)
chrome+teams (24 ths) = CPU: 11,53 % | Memory: 3.425 MB | Process: Sent 93 KB (382 kbps) - Received 201 KB (864 kbps) | Network: Sent 242 KB (562 kbps) - Received 380 KB (716 kbps)
...
```
	
## More information
* http://www.reflection.uniovi.es/
