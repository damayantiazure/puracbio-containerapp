# Introduction 
MonitoringDashboard is an azure function app that contains the following functions:

## 1. AgentQueueTimes
### http trigger from monitoring dashboard

This function monitors the agent queue times.

## 2. AuditLogging
### http trigger from monitoring dashboard

This function monitors the agent queue times.

## 3. CompletenessLog
### http trigger from monitoring dashboard

This function monitors the agent queue times.

## 4. ComplianceScannerCis
### http trigger from monitoring dashboard

This function monitors the agent queue times.

## 5. ComplianceScannerItemsCount
### http trigger from monitoring dashboard

This function monitors the agent queue times.

## 6. ComplianceScannerItemsDate
### http trigger from monitoring dashboard

This function monitors the agent queue times.

## 7. ComplianceScannerPrinciples
### http trigger from monitoring dashboard

This function monitors the agent queue times.

## 8. ComplianceScannerRules
### http trigger from monitoring dashboard

This function monitors the agent queue times.

## 9. HeartMetricsActiveUsers
### http trigger from monitoring dashboard

This function monitors the agent queue times.

## 10. HeartMetricsAgentQueue
### http trigger from monitoring dashboard

This function monitors the agent queue times.

## 11. HeartMetricsProject
### http trigger from monitoring dashboard

This function monitors the agent queue times.

## 12. HeartMetricsProjects
### http trigger from monitoring dashboard

This function monitors the agent queue times.

## 13. HeartMetricsUserEntitlements
### http trigger from monitoring dashboard

This function monitors the agent queue times.

# Getting Started
- https://docs.microsoft.com/en-us/azure/azure-functions/functions-reference
- https://docs.microsoft.com/en-us/azure/azure-functions/functions-develop-local

# Build and Test
- dotnet build
- dotnet test

# Configuration

## Functional users
Connection with the Azure DevOps API is made with a PAT generated with the following accounts:
- eu.MonitDashboard

### Overview all Functional user accounts and expiration date of the PATs
Overview can be found on [this Confluence page](https://confluence.dev.rabobank.nl/x/SBNGF).


# Architecture
- Diagrams are made with https://app.diagrams.net/
- Azure icons: https://github.com/ourchitecture/azure-drawio-icons
- Recommended to use the 'Draw.io integration' vscode extension

## Context diagrams

![Context diagram](_docs/context-diagram-monitoring-dashboard.png "Context diagram")

## Deployment diagrams
![Deployment diagram](_docs/deployment-diagram.png "Deployment diagram")