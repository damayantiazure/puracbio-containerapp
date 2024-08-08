# Introduction 
This repo having a POC for Infra-as-code and deplying the Infra and apps as Container apps

# Pre-requisites!
1.Azure Subscription

2.VS Code

3.Docker Desktop

4.Bicep – For Infra-as-code

5.Git Version control system

6.DevOps Organization, Project/ Github

7.A Service Connection – A service Principal

# Azure Services getting deployed in this POC
1.Container App

2.Container App Environment

3.Application Insight

4.Log Analytics Workspace

5.Managed Identity

6.Virtual Network

7.Container Registry 

8.Key Vault

# DevOps Requirements
1.DevOps Organization

2.DevOps Project

3.Git Repo

4.Service Connection (Service principal)

5.Two Separate devops pipelines
	- For Infrastructure Deployment
	- For building apps, Pushing docker images to Container app creating 	container apps using the images

## Repo Structure

# Infrastructure (Bicep Templates)
- The Infrastructure code(Bicep) files are there under Infrastructure folder
  
- Platform folder contains all Platform related Bicep templates
- Container folder contains all Container app related
- The Script folder is having the script files for different deployments

  ![image](https://github.com/user-attachments/assets/86fa5a15-a672-4279-9b4b-4630fa387355)

#  Application/APIs
- There are three folders for apis - the docker files are created under each apis
  
  1. containerapps-albumapi
  2. containerapps-albumui
  3. containerapps-albumapi-python   


## Deployment diagrams
![Deployment diagram](_docs/deployment-diagram.png "Deployment diagram")
