<img src="http://project-mkp.5v.pl/project/apps/devplanner.png" width="100" height="100"></img>
# DEVPLANNER
*Author: gl00man <maciekbereda46@gmail.com>*

## Table of Content
  - [What is DevPlanner?](#what-is-devplanner)
  - [Requirements](#requirements)
  - [Installation](#installation)
  - [How to use it?](#how-to-use-it)
    - [Public projects](#public-projects)
	- [Private projects](#private-projects)

## What is DevPlanner?
DevPlanner is easy C# WPF application for developers. With this app You can plan your to-do private and public projects.

## Requirements
All You need to run this app is ".NET Core 3.1 Runtime" installed.
For Public Projects Google account(optional).

Program is on MIT License so You can use and edit code as You want.
The .exe file can be found in DevPlanner/bin/Debug/netcoreapp3.1 

## Installation
#### Using Visual Studio: 
Open soulution in Visual Studio and compile it, after it .exe file should appear in *DevPlanner\bin\Debug\netcoreapp3.1* directory.
#### Using cmd: 
Go into */DevPlanner* directory, then compile the project using command: 
```bash
  dotnet build
```
.exe file will be created in *DevPlanner\bin\Debug\netcoreapp3.1* directory.

## How to use it?

### Public projects
Public project list is based on Google Sheets, if you want to create new public list follow this steps:
1. Create new google sheet
2. Open dev_planner.json(api key file) file in DevPlanner/bin/Debug/netcoreapp3.1 directory and copy this client email address.
```bash
  "client_email": "devplanner@devplanner-280912.iam.gserviceaccount.com",
```
3. Now add this email as editor in Your Google sheet document.
4. Open DevPlanner, click add api file.
5. Click "Browse" button and attach api key .json file
6. In Sheet TextBox write any name for your list.
7. In Sheet Id TextBox paste your Google sheet id(You can find it in documets link as SHEET_ID : docs.google.com/spreadsheets/d/SHEET_ID/edit#gid=0).
8. Accept by clicking save and restart DevPlanner.

All Your friend need to access your sheet is Sheet's id.

### Private projects
Projects that can only You see.
