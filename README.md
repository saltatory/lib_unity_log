# Unity Logging Library

## Overview

This library provides a logging framework for Unity projects. Beyond the Unity built-in logging it provides some useful additional features :

* Serialization of objects
* Configurable list of objects to serialize
* Configurable priority settings for logging messages
* Configurable filter for what priorities to output
* File and console logging

## Usage

Clone this library into your unity

```bash
cd <UnityProject>
mkdir Libraries
git submodule add <thisGitRepo>
```

Open the Unity solution and add this library to your solution.

By default, the DLL created by this project will be placed in the Unity folder structure at :

```
<UnityProject>/Assets/Binaries
```
