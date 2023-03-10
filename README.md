# MagicAgent

A background service that forwards Wake On Lan (WOL) packets to the broadcast address.  
Typically used on a server to enable WOL for users connected via VPN.  

**DockerHub**: https://hub.docker.com/r/bvandevliet/magicagent  

## Use case

When connected to your network via VPN, usually the firewall or router does not route broadcasted packets from the internet to your internal LAN.  
This makes it very hard to wake up a machine using Wake On Lan.  
MagicAgent solves this problem acting as an agent between the sender and the intended target machine.  

## How it works

When a magic packet is send to port 9 of the IP address of the device hosting MagicAgent,  
the packet will be forwarded to the network's broadcast address (255.255.255.255) on port 9.  

## Install

**This image is intended to use on a Raspberry Pi 4, hence the tag since it's build with `--platform linux/arm64/v8`.**  
**If you wish to use it on a device with a different architecture, you could consider [making a build](#build) yourself.**  

1. Add the `magicagent` service to your `docker-compose.yml` file.  
Make sure to set `network_mode: host` to allow broadcasting.  
1. You may change the listen and broadcast ports and addresses by overriding the defaults.  
To do so, uncomment the desired environment variable(s) and change its value(s).  
1. `docker compose up -d`  

```
# docker-compose.yml

version: '3.6'

services:
  magicagent:
    container_name: magicagent
    image: bvandevliet/magicagent:linux-arm64-v8
    restart: unless-stopped
    network_mode: host
    environment:
      - TZ=Europe/Amsterdam
      # - NetworkSettings__ListenPort=9
      # - NetworkSettings__ListenAddress=0.0.0.0
      # - NetworkSettings__BroadcastPort=9
      # - NetworkSettings__BroadcastAddress=255.255.255.255
```

## Build

Docker command to build the image from source.  
You may want to change the `--tag` and `--platform` values to meet your requirements.

```
docker image build --tag bvandevliet/magicagent:linux-arm64-v8 --platform linux/arm64/v8 --no-cache -f ./MagicAgent/Dockerfile .
```
