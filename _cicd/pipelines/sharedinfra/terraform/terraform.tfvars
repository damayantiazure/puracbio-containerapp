tables = {
	"tableDefs": [
		{
			"name": "compliancy_cis2_CL",
			"columns": [
				{
					"name": "TimeGenerated",
					"type": "datetime"
				},
				{
					"name": "scanDate_t",
		 			"type": "datetime"
				},
				{
		 			"name": "assignmentGroup_s",
		 			"type": "string"
		 		},
				{
					"name": "ciAICRating_s",
					"type": "string"
				},
				{
					"name": "ciId_s",
					"type": "string"
				},
				{
					"name": "ciName_s",
					"type": "string"
				},
				{
					"name": "ciSubtype_s",
					"type": "string"
				},
				{
					"name": "hasDeviation_b",
					"type": "boolean"
				},
				{
					"name": "isCompliant_b",
					"type": "boolean"
				},
				{
					"name": "isSOx_b",
					"type": "boolean"
				},
				{
					"name": "isSOxCompliant_b",
					"type": "boolean"
				},
				{
					"name": "organization_s",
					"type": "string"
				},
				{
					"name": "projectId_g",
					"type": "string"
				},
				{
					"name": "projectName_s",
					"type": "string"
				}
			]
		},
		{
			"name": "compliancy_items2_CL",
			"columns": [
				{
					"name": "TimeGenerated",
					"type": "datetime"
				},
				{
					"name": "ciId_s",
					"type": "string"
				},
				{
					"name": "ciName_s",
					"type": "string"
				},
				{
					"name": "deviation_Comment_s",
					"type": "string"
				},
				{
					"name": "deviation_Reason_s",
					"type": "string"
				},
				{
					"name": "deviation_ReasonNotApplicable_s",
					"type": "string"
				},
				{
					"name": "deviation_ReasonNotApplicableOther_s",
					"type": "string"
				},
				{
					"name": "deviation_ReasonOther_s",
					"type": "string"
				},
				{
					"name": "deviation_UpdatedBy_s",
					"type": "string"
				},
				{
					"name": "hasDeviation_b",
					"type": "boolean"
				},
				{
					"name": "isCompliant_b",
					"type": "boolean"
				},
				{
					"name": "itemId_g",
					"type": "string"
				},
				{
					"name": "itemId_s",
					"type": "string"
				},
				{
					"name": "itemName_s",
					"type": "string"
				},
				{
					"name": "organization_s",
					"type": "string"
				},
				{
					"name": "principleName_s",
					"type": "string"
				},
				{
					"name": "projectId_g",
					"type": "string"
				},
				{
					"name": "projectName_s",
					"type": "string"
				},
				{
					"name": "ruleName_s",
					"type": "string"
				},
				{
					"name": "scanDate_t",
					"type": "datetime"
				}
			]
		},
		{
			"name": "compliancy_pipelines2_CL",
			"columns": [
				{
					"name": "TimeGenerated",
					"type": "datetime"
				},
				{
					"name": "assignmentGroups_s",
					"type": "string"
				},				
				{
					"name": "ciNames_s",
					"type": "string"
				},
				{
					"name": "organization_s",
					"type": "string"
				},
				{
					"name": "pipelineId_s",
					"type": "string"
				},
				{
					"name": "pipelineName_s",
					"type": "string"
				},
				{
					"name": "pipelineType_s",
					"type": "string"
				},
				{
					"name": "pipelineUrl_s",
					"type": "string"
				},
				{
					"name": "projectId_g",
					"type": "string"
				},
				{
					"name": "projectName_s",
					"type": "string"
				},
				{
					"name": "registrationStatus_s",
					"type": "string"
				},
				{
					"name": "scanDate_t",
					"type": "datetime"
				}
			]
		},
		{
			"name": "compliancy_principles2_CL",
			"columns": [
				{
					"name": "TimeGenerated",
					"type": "datetime"
				},
				{
					"name": "ciId_s",
					"type": "string"
				},
				{
					"name": "ciName_s",
					"type": "string"
				},
				{
					"name": "hasDeviation_b",
					"type": "boolean"
				},
				{
					"name": "isCompliant_b",
					"type": "boolean"
				},
				{
					"name": "organization_s",
					"type": "string"
				},
				{
					"name": "principleName_s",
					"type": "string"
				},
				{
					"name": "projectId_g",
					"type": "string"
				},
				{
					"name": "projectName_s",
					"type": "string"
				},				
				{
					"name": "scanDate_t",
					"type": "datetime"
				}
			]
		},
		{
			"name": "compliancy_rules2_CL",
			"columns": [
				{
					"name": "TimeGenerated",
					"type": "datetime"
				},
				{
					"name": "ciId_s",
					"type": "string"
				},
				{
					"name": "ciName_s",
					"type": "string"
				},
				{
					"name": "hasDeviation_b",
					"type": "boolean"
				},
				{
					"name": "isCompliant_b",
					"type": "boolean"
				},
				{
					"name": "organization_s",
					"type": "string"
				},
				{
					"name": "principleName_s",
					"type": "string"
				},
				{
					"name": "projectId_g",
					"type": "string"
				},
				{
					"name": "projectName_s",
					"type": "string"
				},
				{
					"name": "ruleDocumentation_s",
					"type": "string"
				},
				{
					"name": "ruleName_s",
					"type": "string"
				},
				{
					"name": "scanDate_t",
					"type": "datetime"
				}
			]
		},
        {
			"name": "audit_deployment_log2_CL",
			"columns": [
				{
					"name": "TimeGenerated",
					"type": "datetime"
				},
				{
					"name": "ArtifactIntegrity_b",
					"type": "boolean"
				},
				{
					"name": "BuildUrls_s",
					"type": "string"
				},
				{
					"name": "CiIdentifier_s",
					"type": "string"
				},
				{
					"name": "CiName_s",
					"type": "string"
				},
				{
					"name": "CompletedOn_t",
					"type": "datetime"
				},
				{
					"name": "DeploymentStatus_s",
					"type": "string"
				},
				{
					"name": "FortifyRan_b",
					"type": "boolean"
				},
				{
					"name": "IsSox_b",
					"type": "boolean"
				},
				{
					"name": "Organization_s",
					"type": "string"
				},
				{
					"name": "PipelineApproval_b",
					"type": "boolean"
				},
				{
					"name": "PipelineId_s",
					"type": "string"
				},
                {
					"name": "PipelineName_s",
					"type": "string"
				},
                {
					"name": "ProjectId_g",
					"type": "string"
				},
                {
					"name": "ProjectName_s",
					"type": "string"
				},
                {
					"name": "PullRequestApproval_b",
					"type": "boolean"
				},
                {
					"name": "RepoUrls_s",
					"type": "string"
				},
                {
					"name": "RunId_s",
					"type": "string"
				},
                {
					"name": "RunName_s",
					"type": "string"
				},
                {
					"name": "RunUrl_s",
					"type": "string"
				},
                {
					"name": "SM9ChangeId_s",
					"type": "string"
				},
                {
					"name": "SM9ChangeUrl_s",
					"type": "string"
				},
                {
					"name": "SonarRan_b",
					"type": "boolean"
				},
                {
					"name": "StageId_g",
					"type": "string"
				},
                {
					"name": "StageId_s",
					"type": "string"
				},
                {
					"name": "StageName_s",
					"type": "string"
				}
			]
		},
        {
			"name": "audit_pull_request_approvers_log2_CL",
			"columns": [
				{
					"name": "TimeGenerated",
					"type": "datetime"
				},
				{
					"name": "Approvers_s",
					"type": "string"
				},
				{
					"name": "ClosedBy_s",
					"type": "string"
				},
				{
					"name": "ClosedDate_t",
					"type": "datetime"
				},
				{
					"name": "CreatedBy_s",
					"type": "string"
				},
				{
					"name": "CreationDate_t",
					"type": "datetime"
				},
				{
					"name": "LastMergeCommitId_s",
					"type": "string"
				},
				{
					"name": "LastMergeSourceCommit_s",
					"type": "string"
				},
				{
					"name": "LastMergeTargetCommit_s",
					"type": "string"
				},
				{
					"name": "Organization_s",
					"type": "string"
				},
				{
					"name": "ProjectId_g",
					"type": "string"
				},
				{
					"name": "ProjectName_s",
					"type": "string"
				},
                {
					"name": "PullRequestId_s",
					"type": "string"
				},
                {
					"name": "PullRequestUrl_s",
					"type": "string"
				},
                {
					"name": "RepositoryId_g",
					"type": "string"
				},
                {
					"name": "RepositoryUrl_s",
					"type": "boolean"
				},
                {
					"name": "Status_s",
					"type": "string"
				}
			]
		},
        {
			"name": "audit_logging_error_log2_CL",
			"columns": [
				{
					"name": "TimeGenerated",
					"type": "datetime"
				},
                {
					"name": "CorrelationId",
					"type": "string"
				},                
				{
					"name": "Date_t",
					"type": "datetime"
				},
				{
					"name": "EventQueueData_s",
					"type": "string"
				},
				{
					"name": "ExceptionMessage_s",
					"type": "string"
				},
				{
					"name": "ExceptionType_s",
					"type": "string"
				},
				{
					"name": "FunctionName_s",
					"type": "string"
				},
				{
					"name": "InnerExceptionMessage_s",
					"type": "string"
				},
				{
					"name": "InnerExceptionType_s",
					"type": "string"
				},
				{
					"name": "Organization_s",
					"type": "string"
				},
				{
					"name": "ProjectId_g",
					"type": "string"
				},
				{
					"name": "PullRequestUrl_s",
					"type": "string"
				},
				{
					"name": "ReleaseUrl_s",
					"type": "string"
				},
                {
					"name": "RequestData_s",
					"type": "string"
				},
                {
					"name": "RunUrl_s",
					"type": "string"
				}
			]
		},
        {
			"name": "audit_logging_hook_failure_log2_CL",
			"columns": [
				{
					"name": "TimeGenerated",
					"type": "datetime"
				},
                {
					"name": "Date_t",
					"type": "datetime"
				},                
				{
					"name": "ErrorDetail_s",
					"type": "string"
				},
				{
					"name": "ErrorMessage_s",
					"type": "string"
				},
				{
					"name": "EventId_g",
					"type": "string"
				},
				{
					"name": "EventResourceData_s",
					"type": "string"
				},
				{
					"name": "EventType_s",
					"type": "string"
				},
				{
					"name": "HookId_g",
					"type": "string"
				},
				{
					"name": "Organization_s",
					"type": "string"
				},
				{
					"name": "PipelineId_s",
					"type": "string"
				},
				{
					"name": "ProjectId_g",
					"type": "string"
				}
			]
		},
        {
			"name": "audit_poison_messages_log2_CL",
			"columns": [
				{
					"name": "TimeGenerated",
					"type": "datetime"
				},
                {
					"name": "FailedQueueTrigger_s",
					"type": "string"
				},                
				{
					"name": "MessageText_s",
					"type": "string"
				}
			]
		},
        {
			"name": "deviations_log2_CL",
			"columns": [
				{
					"name": "TimeGenerated",
					"type": "datetime"
				},
                {
					"name": "CiIdentifier_s",
					"type": "string"
				},                
				{
					"name": "Comment_s",
					"type": "string"
				},
				{
					"name": "ItemId_g",
					"type": "string"
				},
				{
					"name": "ItemId_s",
					"type": "string"
				},
				{
					"name": "ItemProjectId_g",
					"type": "string"
				},
				{
					"name": "ProjectId_g",
					"type": "string"
				},
				{
					"name": "Reason_s",
					"type": "string"
				},
				{
					"name": "ReasonNotApplicable_s",
					"type": "string"
				},
				{
					"name": "ReasonNotApplicableOther_s",
					"type": "string"
				},
                {
					"name": "ReasonOther_s",
					"type": "string"
				},
				{
					"name": "RecordType_s",
					"type": "string"
				},
				{
					"name": "RuleName_s",
					"type": "string"
				}
			]
		},
        {
			"name": "decorator_error_log2_CL",
			"columns": [
				{
					"name": "TimeGenerated",
					"type": "datetime"
				},
                {
					"name": "Message",
					"type": "string"
				},                
				{
					"name": "Organization_s",
					"type": "string"
				},
				{
					"name": "PipelineType_s",
					"type": "string"
				},
				{
					"name": "ProjectId_g",
					"type": "string"
				},
				{
					"name": "ReleaseId_s",
					"type": "string"
				},
				{
					"name": "RunId_s",
					"type": "string"
				},
				{
					"name": "StageName_s",
					"type": "string"
				}
			]
		},
        {
			"name": "pipeline_breaker_compliance_log2_CL",
			"columns": [
				{
					"name": "TimeGenerated",
					"type": "datetime"
				},
                {
					"name": "Approver_s",
					"type": "string"
				},                
				{
					"name": "CiIdentifier_s",
					"type": "string"
				},
				{
					"name": "CiName_s",
					"type": "string"
				},
				{
					"name": "Date_t",
					"type": "datetime"
				},
				{
					"name": "ExclusionReasonApprover_s",
					"type": "string"
				},
				{
					"name": "ExclusionReasonRequester_s",
					"type": "string"
				},
				{
					"name": "HasCorrectRetention_s",
					"type": "string"
				},
                {
					"name": "HasDeviation_b",
					"type": "boolean"
				},
                {
					"name": "IsCompliant_b",
					"type": "boolean"
				},
                {
					"name": "IsExcluded_b",
					"type": "boolean"
				},
                {
					"name": "Organization_s",
					"type": "string"
				},
                {
					"name": "PipelineId_s",
					"type": "string"
				},
                {
					"name": "PipelineName_s",
					"type": "string"
				},
                {
					"name": "PipelineType_s",
					"type": "string"
				},
                {
					"name": "PipelineVersion_s",
					"type": "string"
				},
                {
					"name": "ProjectId_g",
					"type": "string"
				},
                {
					"name": "ProjectName_s",
					"type": "string"
				},
                {
					"name": "Requester_s",
					"type": "string"
				},
                {
					"name": "Result_d",
					"type": "real"
				},
                {
					"name": "RuleCompliancyReports_s",
					"type": "string"
				},
                {
					"name": "RunId_s",
					"type": "string"
				},
                {
					"name": "RunUrl_s",
					"type": "string"
				},
                {
					"name": "StageId_s",
					"type": "string"
				}
			]
		},
        {
			"name": "pipeline_breaker_log2_CL",
			"columns": [
                {
					"name": "TimeGenerated",
					"type": "datetime"
				},
				{
					"name": "CiIdentifier_s",
					"type": "string"
				},
                {
					"name": "CiName_s",
					"type": "string"
				},                
				{
					"name": "Date_t",
					"type": "datetime"
				},
				{
					"name": "IsExcluded_b",
					"type": "boolean"
				},
				{
					"name": "IsRegistered_b",
					"type": "boolean"
				},
				{
					"name": "Organization_s",
					"type": "string"
				},
				{
					"name": "PipelineId_s",
					"type": "string"
				},
				{
					"name": "PipelineName_s",
					"type": "string"
				},
                {
					"name": "PipelineType_s",
					"type": "string"
				},
                {
					"name": "PipelineVersion_s",
					"type": "string"
				},
                {
					"name": "ProjectId_g",
					"type": "string"
				},
                {
					"name": "ProjectName_s",
					"type": "string"
				},
                {
					"name": "RegistrationStatus_s",
					"type": "string"
				},
                {
					"name": "Result_d",
					"type": "real"
				},
                {
					"name": "RunId_s",
					"type": "string"
				},
                {
					"name": "RunUrl_s",
					"type": "string"
				},
                {
					"name": "StageId_s",
					"type": "string"
				}
            ]
		},
		{
			"name": "compliance_scanner_online_error_log2_CL",
			"columns": [
                {
					"name": "TimeGenerated",
					"type": "datetime"
				},
				{
					"name": "CiIdentifier_s",
					"type": "string"
				},
                {
					"name": "CorrelationId",
					"type": "string"
				},                
				{
					"name": "Date_t",
					"type": "datetime"
				},
				{
					"name": "ExceptionMessage_s",
					"type": "string"
				},
				{
					"name": "ExceptionType_s",
					"type": "string"
				},
				{
					"name": "FunctionName_s",
					"type": "string"
				},
				{
					"name": "InnerExceptionMessage_s",
					"type": "string"
				},
				{
					"name": "InnerExceptionType_s",
					"type": "string"
				},
                {
					"name": "ItemId_g",
					"type": "string"
				},
                {
					"name": "ItemId_s",
					"type": "string"
				},
                {
					"name": "ItemType_s",
					"type": "string"
				},
                {
					"name": "Organization_s",
					"type": "string"
				},
                {
					"name": "ProjectId_g",
					"type": "string"
				},
                {
					"name": "ProjectId_s",
					"type": "string"
				},
                {
					"name": "Request_s",
					"type": "string"
				},
                {
					"name": "RequestUrl_s",
					"type": "string"
				},
                {
					"name": "RuleName_s",
					"type": "string"
				},
				{
					"name": "StageId_s",
					"type": "string"
				},
                {
					"name": "UserId_g",
					"type": "string"
				},
                {
					"name": "UserMail_s",
					"type": "string"
				}
            ]
		},
		{
			"name": "validate_gates_error_log2_CL",
			"columns": [
                {
					"name": "TimeGenerated",
					"type": "datetime"
				},
				{
					"name": "CorrelationId",
					"type": "string"
				},
                {
					"name": "Date_t",
					"type": "datetime"
				},				
				{
					"name": "ExceptionMessage_s",
					"type": "string"
				},
				{
					"name": "ExceptionType_s",
					"type": "string"
				},
				{
					"name": "FunctionName_s",
					"type": "string"
				},
				{
					"name": "InnerExceptionMessage_s",
					"type": "string"
				},
				{
					"name": "InnerExceptionType_s",
					"type": "string"
				},
                {
					"name": "ProjectId_g",
					"type": "string"
				},
				{
					"name": "ProjectId_s",
					"type": "string"
				},
                {
					"name": "ReleaseId_s",
					"type": "string"
				},
                {
					"name": "Request_s",
					"type": "string"
				},
                {
					"name": "RequestUrl_s",
					"type": "string"
				},
                {
					"name": "RunId_s",
					"type": "string"
				},
                {
					"name": "StageId_s",
					"type": "string"
				}
            ]
		},
		{
			"name": "sm9_changes_error_log2_CL",
			"columns": [
                {
					"name": "TimeGenerated",
					"type": "datetime"
				},
				{
					"name": "CorrelationId",
					"type": "string"
				},
                {
					"name": "Date_t",
					"type": "datetime"
				},				
				{
					"name": "ExceptionMessage_s",
					"type": "string"
				},
				{
					"name": "ExceptionType_s",
					"type": "string"
				},
				{
					"name": "FunctionName_s",
					"type": "string"
				},
				{
					"name": "InnerExceptionMessage_s",
					"type": "string"
				},
				{
					"name": "InnerExceptionType_s",
					"type": "string"
				},
				{
					"name": "Organization_s",
					"type": "string"
				},
				{
					"name": "PipelineType_s",
					"type": "string"
				},
                {
					"name": "ProjectId_g",
					"type": "string"
				},
				{
					"name": "ProjectId_s",
					"type": "string"
				},
                {
					"name": "Request_s",
					"type": "string"
				},
                {
					"name": "RequestUrl_s",
					"type": "string"
				},
                {
					"name": "RunId_s",
					"type": "string"
				}
            ]
		},
		{
			"name": "error_handling_log2_CL",
			"columns": [
                {
					"name": "TimeGenerated",
					"type": "datetime"
				},
				{
					"name": "CorrelationId",
					"type": "string"
				},
                {
					"name": "Date_t",
					"type": "datetime"
				},				
				{
					"name": "ExceptionMessage_s",
					"type": "string"
				},
				{
					"name": "ExceptionType_s",
					"type": "string"
				},
				{
					"name": "FunctionName_s",
					"type": "string"
				},
				{
					"name": "InnerExceptionMessage_s",
					"type": "string"
				},
				{
					"name": "InnerExceptionType_s",
					"type": "string"
				},
				{
					"name": "Organization_s",
					"type": "string"
				},
                {
					"name": "ProjectId_g",
					"type": "string"
				},
				{
					"name": "ProjectName_s",
					"type": "string"
				},
                {
					"name": "ScanDate_t",
					"type": "datetime"
				}
            ]
		},
		{
			"name": "pipeline_breaker_error_log2_CL",
			"columns": [
                {
					"name": "TimeGenerated",
					"type": "datetime"
				},
				{
					"name": "CorrelationId",
					"type": "string"
				},
                {
					"name": "Date_t",
					"type": "datetime"
				},				
				{
					"name": "ExceptionMessage_s",
					"type": "string"
				},
				{
					"name": "ExceptionType_s",
					"type": "string"
				},
				{
					"name": "FunctionName_s",
					"type": "string"
				},
				{
					"name": "InnerExceptionMessage_s",
					"type": "string"
				},
				{
					"name": "InnerExceptionType_s",
					"type": "string"
				},
				{
					"name": "ItemId_s",
					"type": "string"
				},
				{
					"name": "ItemType_s",
					"type": "string"
				},
				{
					"name": "Organization_s",
					"type": "string"
				},
				{
					"name": "ProjectId_g",
					"type": "string"
				},
                {
					"name": "ProjectId_s",
					"type": "string"
				},
				{
					"name": "Request_s",
					"type": "string"
				},
				{
					"name": "RequestUrl_s",
					"type": "string"
				},
                {
					"name": "RuleName_s",
					"type": "string"
				},
				{
					"name": "RunId_s",
					"type": "string"
				}
            ]
		}
	]
}