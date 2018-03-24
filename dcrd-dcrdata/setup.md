Installing dcrd and dcrdata
--

Prior to deploying the Lykke.Service.Decred.Api project, there are 2 dependencies that must be available and running:

* dcrd - Decred network daemon
* dcrdata - Decred block explorer w/ postgresql backend

By default, both programs use the testnet network and save block information in the current directory.
To use mainnet, simply remove the `testnet=1` flag from both configuration files

1. Edit configuration files

dcrd configuration - `dcrd/dcrd.conf`
dcrdata configuration - `dcrdata/dcrdata.conf`

Ensure that all values in configuration files are set correctly.

2. Build dcrdata docker container

` $ cd docker && docker build -f dcrdata.Dockerfile -t lykke/dcrdata . `

3. Start dcrd

If this is the first time running dcrd, it may take a while to sync blocks from the network.
Wait until this finishes prior to starting dcrdata.

` $ docker run -d -p 19109:19109 -p 19108:19108 -v $(pwd)/dcrd:/home/decred/.dcrd decred/dcrd-mainnet `

4. Start dcrdata

After dcrd sync completes

` $ docker run -d --network=host -v ~/dcrdata:/home/decred/.dcrdata -v ~/dcrd/rpc.cert:/home/decred/.dcrd/rpc.cert lykke/dcrdata dcrdata `