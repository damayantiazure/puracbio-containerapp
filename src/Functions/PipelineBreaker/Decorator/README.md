# Introduction
You need to be secure in how you develop, build, test, and deploy your applications. To aid you in this, Rabobank has drawn up the Azure DevOps Security Blueprint.
The Security Blueprint has been translated into several Principles and Rules. The compliance status of each of these rules for your application can be found 
in the Compliancy Hub in your Azure DevOps project.
One of the prerequisites to get started with the Compliancy Hub is to register release pipelines. To prove that an application has been developed and released in a 
compliant manner, it is important to know which pipelines and repositories are associated with this application.
Therefore, it is mandatory to link release pipelines to one or more Configuration Items within the CMDB. After this registration, verification of these rules is automatically applied.
For pipelines that are not used to deploy applications to a production environment, it is mandatory to register the pipeline as non-production.

This pipeline decorator automatically adds a task at the beginning of each job.
The purpose of this 'Pre-Job' is to verify that the pipeline has been registered as well as being compliant.
If no valid registration has been found and/or there are rules found that are incompliant, the decorator will throw an error.

# Getting Started
The extension has no configurable options. Once installed, the decorator will automatically insert the 'Pre-Job' into every pipeline within an organization.
The 'Pre-Job' will not run for all pipelines, a condition has been applied that verifies if your pipeline does have a release stage.
Build pipelines do not have to be registered, as they are detected automatically once the corresponding release pipeline is registered.

# More information
This extension has been developed by Tech4Dev. For instructions on how to register a release pipeline, and how to be compliant,
checkout our [Confluence documentation](https://confluence.dev.rabobank.nl/x/2GflCw).